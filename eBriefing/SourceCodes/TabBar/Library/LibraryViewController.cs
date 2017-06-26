/*
Copyright (C) 2017 MetroStar Systems

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

The full license text can be found is the included LICENSE file.

You can freely use any of this software which you make publicly 
available at no charge.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

using System;
using System.Linq;
using CoreGraphics;
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreAnimation;
using MssFramework;
using Metrostar.Mobile.Framework;
using MaryPopinBinding;
using ASIHTTPRequestBinding;

namespace eBriefingMobile
{
    public partial class LibraryViewController : BaseViewController
    {
        private bool updateMyBookTab;
        private LibraryDataSource dataSource;
        private BookOverviewController boc;

        public delegate void LibraryViewDelegate ();

        public event LibraryViewDelegate OpenBookshelfEvent;

        public LibraryViewController()
        {

        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();
            
            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            // Perform any additional setup after loading the view, typically from a nib.
            this.Title = StringRef.available;

            InitializeCollectionView(StringRef.LibraryCell);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            updateMyBookTab = false;

            BookUpdater.RegisterASIDelegate(this);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // Load books only if URL is not empty and the user is Authenticated
            if (!String.IsNullOrEmpty(URL.ServerURL) && Settings.Authenticated)
            {
                LoadBooks();
            }
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // Update My Books TabBar location
            if (!updateMyBookTab)
            {
                updateMyBookTab = true;

                UpdateTabLocation();
            }
        }

        public void Sort()
        {
            if (dataSource != null && dataSource.BookList != null && dataSource.BookList.Count > 0)
            {
                try
                {
                    List<Book> newBookList = new List<Book>(dataSource.BookList);
                    newBookList = dataSource.SortBooks(newBookList);

                    // Update dataSource before batch updates
                    dataSource.BookList = newBookList;

                    collectionView.PerformBatchUpdates(delegate
                    {
                        foreach (Book book in dataSource.BookList)
                        {
                            NSIndexPath fromIndexPath = dataSource.GetIndexPath(book.ID);
                            int toRow = newBookList.IndexOf(book);
                            if (fromIndexPath != null && fromIndexPath.Row >= 0 && toRow >= 0)
                            {
                                NSIndexPath toIndexPath = NSIndexPath.FromRowSection(toRow, 0);
                                collectionView.MoveItem(fromIndexPath, toIndexPath);
                            }
                        }
                    }, delegate
                    {
                        collectionView.ReloadData();
                    });
                }
                catch (Exception ex)
                {
                    Logger.WriteLineDebugging("LibraryViewController - Sort: {0}", ex.ToString());
                }
            }
        }

        async public void LoadBooks()
        {
            TimeSpan diff = DateTime.UtcNow.Subtract(Settings.AvailableCheckTime);
            if (TimeSettings.LibraryRefreshRequired(diff))
            {
                if (Reachability.IsDefaultNetworkAvailable())
                {
                    LoadingView.Show("Loading", "Please wait while we're checking Available Books...", false);

                    List<Book> bookList = await eBriefingService.Run(() => eBriefingService.StartDownloadBooks());
					if ( bookList != null )
					{
						Settings.AvailableCheckTime = DateTime.UtcNow;

						// Save in the cache
						BooksOnServerAccessor.SaveBooks (bookList);

						// Load books
						RetrieveBooks ();

						// Update available badge
						UpdateAvailableBadge ();
					}
					else
					{
						LoadingView.Hide ();
					}
                }
                else
                {
                    UpdateStatusLabel(StringRef.connectionRequired);
                }
            }
            else
            {
                RetrieveBooks();
            }
        }

        public void RetrieveBooks()
        {
            if (!Reachability.IsDefaultNetworkAvailable())
            {
                UpdateStatusLabel(StringRef.connectionRequired);
            }
            else
            {
                List<Book> bookList = BooksOnServerAccessor.GetBooks();
                if (bookList == null)
                {
                    UpdateStatusLabel("There are no available books.");

                    Failed2RetrieveBooks();
                }
                else
                {
                    // Load collectionView
                    LoadCollectionView();
                }
            }
        }

        public void OpenOverview(Book book)
        {
            HandleItemPressedEvent(book);
        }

        private void LoadCollectionView()
        {
            // Load collectionView
            if (collectionView != null)
            {
                List<Book> bookList = null;

                // Only show those books that are not on the device
                List<Book> sBooks = BooksOnServerAccessor.GetBooks();
                List<Book> dBooks = BooksOnDeviceAccessor.GetBooks();

                if (sBooks != null && dBooks != null)
                {
                    HashSet<String> dIDs = new HashSet<String>(dBooks.Select(d => d.ID));
                    var results = sBooks.Where(s => !dIDs.Contains(s.ID)).ToList();
                    if (results != null)
                    {
                        bookList = results;
                    }
                }
                else if (sBooks != null)
                {
                    bookList = sBooks;
                }

                LoadingView.Hide();

                if (bookList == null || bookList.Count == 0)
                {
                    UpdateStatusLabel("There are no more available books.");

                    UpdateAvailableBadge();
                }
                else
                {
                    dataSource = new LibraryDataSource(new List<Book>(bookList), this);
                    dataSource.ItemPressedEvent += HandleItemPressedEvent;
                    dataSource.DownloadEvent += HandleDownloadEvent;
                    collectionView.Source = dataSource;
                    collectionView.ReloadData();

                    ShowHideStatusLabel(false);
                }
            }
        }

        private void Failed2RetrieveBooks()
        {
            LoadingView.Hide();

            UIAlertView alert = new UIAlertView(StringRef.alert, StringRef.failed2Retrieve + "books.", null, StringRef.ok);
            alert.Dismissed += (object sender, UIButtonEventArgs e) =>
            {
                if (!BooksOnServerAccessor.HasBooks())
                {
                    if (String.IsNullOrEmpty(URL.ServerURL))
                    {
                        OpenBookshelf();
                    }
                }
            };
            alert.Show();
        }

        private void OpenBookshelf()
        {
            if (OpenBookshelfEvent != null)
            {
                OpenBookshelfEvent();
            }
        }

        private void UpdateStatusLabel(String text)
        {
            statusLabel.Text = text;

            base.UpdateStatusLabel();
        }

		[Export("requestFinish:")]
		void RequestFinish(NSObject sender)
        {
            try
            {
                ASIHTTPRequest request = sender as ASIHTTPRequest;
                if (request != null)
                {
                    // THIS IS REQUIRED TO SKIP iCLOUD BACKUP
                    SkipBackup2iCloud.SetAttribute(request.DownloadDestinationPath);

                    if (dataSource != null)
                    {
                        dataSource.UpdateImage(request.Url.AbsoluteString);
                    }

                    if (boc != null)
                    {
                        boc.UpdateCover(request.Url.AbsoluteString);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("LibraryViewController - RequestFinish: {0}", ex.ToString());
            }
        }

        void HandleItemPressedEvent(Book book)
        {
            try
            {
                if (boc != null)
                {
                    boc.Dispose();
                    boc = null;
                }

                boc = new BookOverviewController(book, false);
                boc.View.Frame = new CGRect(0, 0, 646, 449);
                boc.DownloadEvent += HandleOverviewDownloadEvent;
                boc.SetPopinTransitionStyle(BKTPopinTransitionStyle.SpringySlide);
                boc.SetPopinOptions(BKTPopinOption.Default);
                boc.SetPopinTransitionDirection(BKTPopinTransitionDirection.Top);
                this.PresentPopinController(boc, true, null);
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("LibraryViewController - HandleItemPressedEvent: {0}", ex.ToString());
            }
        }

        void HandleOverviewDownloadEvent(Book book)
        {
            this.DismissCurrentPopinControllerAnimated(true);

            LibraryBookView bookView = dataSource.GetLibraryBookView(book.ID);
            if (bookView != null)
            {
                HandleDownloadEvent(bookView);
            }
        }

        void HandleDownloadEvent(LibraryBookView bookView)
        {
            try
            {
                if (!downloadAnimation)
                {
                    downloadAnimation = true;

                    // Remove from collectionView
                    bookView.RemoveFromSuperview();

                    // Add to the current view at exact position
                    NSIndexPath indexPath = dataSource.GetIndexPath(bookView.LibraryBook.ID);
                    UICollectionViewLayoutAttributes attributes = collectionView.GetLayoutAttributesForItem(indexPath);
                    bookView.Frame = new CGRect(attributes.Frame.X + collectionView.Frame.X, attributes.Frame.Y - collectionView.ContentOffset.Y, attributes.Frame.Width, attributes.Frame.Height);
                    this.View.AddSubview(bookView);

                    // Fly Animation
                    CGPoint startPoint = new CGPoint(bookView.Frame.X + bookView.Frame.Width / 2, bookView.Frame.Y + bookView.Frame.Height / 2);
                    CGPoint endPoint = new CGPoint(Settings.MyBooksTabLocation.X + 76f / 2f, this.View.Frame.Bottom + 55f / 2f);

                    UIView.Animate(0.75d, delegate
                    {
                        bookView.Transform = CGAffineTransform.MakeScale(0.1f, 0.1f);
                        bookView.Center = endPoint;
                        bookView.Alpha = 0.5f;

                        // Prepare my own keypath animation for the layer position
                        CGPath animationPath = CreatePath(startPoint, endPoint);
                        CAKeyFrameAnimation keyFrameAnimation = CAKeyFrameAnimation.GetFromKeyPath("position");
                        keyFrameAnimation.Path = animationPath;

                        // Copy properties from UIView's animation
                        CAAnimation autoAnimation = bookView.Layer.AnimationForKey("position");
                        keyFrameAnimation.Duration = autoAnimation.Duration;
                        keyFrameAnimation.FillMode = autoAnimation.FillMode;
                        keyFrameAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut);

                        // Replace UIView's animation with my animation
                        bookView.Layer.AddAnimation(keyFrameAnimation, keyFrameAnimation.KeyPath);
                    }, delegate
                    {
                        this.InvokeOnMainThread(delegate
                        {
                            UpdateCollectionView(bookView);
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("LibraryViewController - HandleDownloadEvent: {0}", ex.ToString());
            }
        }

        private CGPath CreatePath(CGPoint startPoint, CGPoint endPoint)
        {
            // Creates the path that we'll use to animate on
            CGPoint controlPoint = CGPoint.Empty;
            if (startPoint.X > endPoint.X)
            {
                controlPoint = new CGPoint(endPoint.X + ((startPoint.X - endPoint.X) / 2), startPoint.Y - 400);
            }
            else
            {
                controlPoint = new CGPoint(startPoint.X + ((endPoint.X - startPoint.X) / 2), startPoint.Y - 400);
            }

            CGPath animationPath = new CGPath(); 
            animationPath.MoveToPoint(startPoint.X, startPoint.Y);
            animationPath.AddQuadCurveToPoint(controlPoint.X, controlPoint.Y, endPoint.X, endPoint.Y);
            return animationPath;
        }

        private void UpdateCollectionView(LibraryBookView bookView)
        {
            if (dataSource != null && dataSource.BookList != null && dataSource.BookList.Count > 0)
            {
                try
                {
                    bookView.RemoveFromSuperview();

                    Book book = bookView.LibraryBook.Copy();
                    book.Status = Book.BookStatus.PENDING2DOWNLOAD;

                    // Add the book to the device
                    BooksOnDeviceAccessor.AddBook(book);

                    // Start the download
                    BookUpdater.CheckBooks2Download();

                    // Update available badge
                    UpdateAvailableBadge();

                    // Remove from the list
                    List<Book> newBookList = new List<Book>(dataSource.BookList);
                    int removeIdx = dataSource.GetBookIndex(book.ID);
                    newBookList.RemoveAt(removeIdx);

                    collectionView.PerformBatchUpdates(delegate
                    {
                        foreach (Book b in dataSource.BookList)
                        {
                            if (b.ID != book.ID)
                            {
                                NSIndexPath fromIndexPath = dataSource.GetIndexPath(b.ID);
                                int toRow = newBookList.IndexOf(b);
                                if (fromIndexPath != null && fromIndexPath.Row >= 0 && toRow >= 0)
                                {
                                    NSIndexPath toIndexPath = NSIndexPath.FromRowSection(toRow, 0);
                                    collectionView.MoveItem(fromIndexPath, toIndexPath);
                                }
                            }
                        }
                    }, delegate
                    {
                        downloadAnimation = false;

                        RefreshTable();
                    });
                }
                catch (Exception ex)
                {
                    Logger.WriteLineDebugging("LibraryViewController - UpdateCollectionView: {0}", ex.ToString());
                }
            }
        }
    }
}

