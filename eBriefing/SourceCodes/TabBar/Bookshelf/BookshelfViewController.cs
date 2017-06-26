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
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreGraphics;
using ObjCRuntime;
using Metrostar.Mobile.Framework;
using MssFramework;
using MaryPopinBinding;
using ASIHTTPRequestBinding;

namespace eBriefingMobile
{
	public partial class BookshelfViewController : BaseViewController
	{
		private BookshelfDataSource dataSource;
		private UIPopoverController menuViewController;

		public BookshelfViewController()
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
			this.NavigationItem.BackBarButtonItem = new UIBarButtonItem(String.Empty, UIBarButtonItemStyle.Plain, null);

			InitializeCollectionView(StringRef.BookshelfCell);

			BookUpdater.DownloadStartEvent += HandleDownloadStartEvent;
			BookUpdater.DownloadFinishEvent += HandleDownloadFinishEvent;
			BookUpdater.NewBookAddedEvent += HandleNewBookAddedEvent;
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			DownloadReporter.MyBooksVC = this;

			Settings.CurrentPageID = String.Empty;
		}
			
		private void StartDownload(List<Book> bookList = null)
		{
			if (bookList != null)
			{
				BookUpdater.Enqueue(bookList);
			}

			// Only start only if it is not in progress
			if (!BookUpdater.InProgress)
			{
				BookUpdater.Start();
			}
		}

		protected virtual void LoadCollectionView(List<Book> bookList, bool updateMenu = false)
		{
			// collectionView
			if (collectionView != null)
			{
				if ( dataSource != null )
				{
					dataSource = null;
				}

				dataSource = new BookshelfDataSource(bookList, updateMenu, (BookshelfViewController)this);
				dataSource.ItemPressedEvent += HandleItemPressedEvent;
				dataSource.UpdateBookEvent += HandleUpdateBookEvent;
				dataSource.RefreshBookshelfEvent += HandleRefreshBookshelfEvent;
				dataSource.ShowMenuEvent += HandleShowMenuEvent;
				collectionView.Source = dataSource;
				collectionView.ReloadData();

				// Start downloading if needed
				if (BookUpdater.Books2Download != null && !BookUpdater.InProgress)
				{
					StartDownload();
				}
			}
		}

		[Export("requestFinish:")]
		protected virtual void RequestFinish(NSObject sender)
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
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("BookshelfViewController - RequestFinish: {0}", ex.ToString());
			}
		}

		protected virtual void RemoveBook(Book book)
		{
			RefreshTable();
		}

		protected virtual void ReloadCollectionView()
		{
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
					Logger.WriteLineDebugging("BookshelfViewController - Sort: {0}", ex.ToString());
				}
			}
		}

		public void UpdateProgress()
		{
			this.InvokeOnMainThread(delegate
			{
				if (dataSource != null)
				{
					Book book = BookUpdater.CurrentBook;
					if (book != null)
					{
						if (BookUpdater.TotalDownloadCount == 0)
						{
							BookUpdater.CurrentBook.DownloadCount = 0;
							dataSource.UpdateProgressView(book.ID, 1);
						}
						else
						{
							dataSource.UpdateProgressView(book.ID, book.DownloadCount / BookUpdater.TotalDownloadCount);
						}
					}
				}
			});
		}

		public void DownloadFinished(bool updated)
		{
			this.InvokeOnMainThread(delegate
			{
				Book book = BookUpdater.CurrentBook;
				if (book != null)
				{
					HideMenuPopover();

					dataSource.FinishedDownloading(book.ID);

					if (updated)
					{
						// Update collectionView
						GetBookViewAndUpdateCollectionView(book.ID);

						// Update Updates Tab badge
						UpdateUpdatesBadge();

						// Update My Books Tab badge
						UpdateMyBooksBadge();
					}
				}
			});
		}

		public void RedownloadFailedURLs()
		{
			if (BookUpdater.CurrentBook != null && BookUpdater.CurrentBook.FailedURLs != null)
			{
				ASINetworkQueue networkQueue = new ASINetworkQueue();
				networkQueue.Reset();
				networkQueue.ShowAccurateProgress = false;
				networkQueue.ShouldCancelAllRequestsOnFailure = false;
				networkQueue.Delegate = this;

				networkQueue.RequestDidFail = new Selector("requestDidFail:");
				networkQueue.RequestDidFinish = new Selector("requestDidFinish:");
				networkQueue.QueueDidFinish = new Selector("queueDidFinish:");

				ASIHTTPRequest request = null;
				foreach (String url in BookUpdater.CurrentBook.FailedURLs)
				{
					DownloadedFilesCache.RemoveFile(url);

					request = new ASIHTTPRequest(NSUrl.FromString(url));
					request.DownloadDestinationPath = DownloadedFilesCache.BuildCachedFilePath(url);
					request.Username = Settings.UserID;
					request.Password = KeychainAccessor.Password;
					request.Domain = Settings.Domain;

					networkQueue.AddOperation(request);
				}

				// Clear failedUrls before starting to re-download
				BookUpdater.CurrentBook.FailedURLs.Clear();

				networkQueue.Go();
			}
		}

		public void CancelDownload(Book book)
		{
			try
			{
				if (BookUpdater.CurrentBook.ID == book.ID)
				{
					book.Cancelled = true;

					BookUpdater.CancelDownloadOperations();

					// If internet connection is available, remove the current book and start download next one
					if (Reachability.IsDefaultNetworkAvailable())
					{
						GetBookViewAndUpdateCollectionView(book.ID);
					}
					else
					{
						// Cancel current downloads if it's in progress
						CancelAllDownloads();
					}
				}
				else
				{
					BookUpdater.RemoveBookFromDevice(book);
					BookUpdater.Dequeue(book.ID);
				}

				RefreshTable();
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("BookshelfViewController - CancelDownload: {0}", ex.ToString());
			}
		}

		public void CancelAllDownloads()
		{
			if (BookUpdater.CurrentBook != null)
			{
				BookUpdater.CurrentBook.Cancelled = true;
			}

			BookUpdater.CancelDownloadOperations();

			RemovePendingBooks();

			BookUpdater.DownloadFinished();

			RefreshTable();
		}

		private void GetBookViewAndUpdateCollectionView(String bookID)
		{
			if (dataSource != null)
			{
				BookshelfBookView bookView = dataSource.GetBookshelfBookView(collectionView, bookID);
				if (bookView != null)
				{
					UpdateCollectionView(bookView);
				}
			}
		}

		private void RemovePendingBooks()
		{
			// Remove pending books
			List<Book> removeList = null;
			List<Book> bookList = BooksOnDeviceAccessor.GetBooks();
			if (bookList != null)
			{
				foreach (Book b in bookList)
				{
					if (b.Status == Book.BookStatus.PENDING2DOWNLOAD || b.Status == Book.BookStatus.PENDING2UPDATE)
					{
						if (removeList == null)
						{
							removeList = new List<Book>();
						}

						removeList.Add(b);
					}
				}

				if (removeList != null)
				{
					foreach (Book b in removeList)
					{
						BooksOnDeviceAccessor.RemoveBook(b.ID);
					}
				}
			}
		}

		private void HideMenuPopover()
		{
			if (menuViewController != null)
			{
				menuViewController.Dismiss(true);
				menuViewController = null;
			}
		}

		void HandleDownloadStartEvent(String bookID)
		{
			if (dataSource != null)
			{
				Book book = BooksOnDeviceAccessor.GetBook(bookID);
				if (book != null)
				{
					if (book.Status == Book.BookStatus.UPDATING)
					{
						dataSource.UpdateStatus(bookID, true);
					}
					else
					{
						dataSource.UpdateStatus(bookID, false);
					}
				}
			}
		}

		void HandleDownloadFinishEvent(String bookID)
		{
			RequestDidFinish(this);
		}

		void HandleNewBookAddedEvent()
		{
			RefreshTable();
		}

		void HandleItemPressedEvent(Book book)
		{
			if (book.Status == Book.BookStatus.ISUPDATE)
			{
				try
				{
					BookOverviewController boc = new BookOverviewController(book, true);
					boc.View.Frame = new CGRect(0, 0, 646, 449);
					boc.DownloadEvent += HandleUpdateBookEvent;
					boc.SetPopinTransitionStyle(BKTPopinTransitionStyle.SpringySlide);
					boc.SetPopinOptions(BKTPopinOption.Default);
					boc.SetPopinTransitionDirection(BKTPopinTransitionDirection.Top);
					this.PresentPopinController(boc, true, null);
				}
				catch (Exception ex)
				{
					Logger.WriteLineDebugging("BookshelfViewController - HandleItemPressedEvent: {0}", ex.ToString());
				}
			}
			else
			{
				AppDelegate.Current.Nav.PushViewController(new DashboardViewController(book), true);
			}
		}

		void HandleUpdateBookEvent(Book book)
		{
			this.DismissCurrentPopinControllerAnimated(true);

			List<Book> bookList = new List<Book>();
			book.Status = Book.BookStatus.PENDING2UPDATE;
			bookList.Add(book);

			dataSource.Waiting2Update(book.ID);

			// Start the download
			StartDownload(bookList);
		}

		void HandleRefreshBookshelfEvent()
		{
			ReloadCollectionView();
		}

		void HandleShowMenuEvent(BookshelfBookView bookView, bool isFavorite)
		{
			PopoverMenuController pmc = new PopoverMenuController(bookView, isFavorite);
			if (bookView.BookshelfBook.Status == Book.BookStatus.DOWNLOADED)
			{
				pmc.View.Frame = new CGRect(0, 0, bookView.Frame.Width, 88);
			}
			else
			{
				pmc.View.Frame = new CGRect(0, 0, bookView.Frame.Width, 44);
			}

			pmc.FavoriteEvent += delegate
			{
				HideMenuPopover();

				dataSource.UpdateFavorite(bookView.BookshelfBook.ID, !isFavorite);

				if (this.Title != StringRef.myBooks)
				{
					UpdateCollectionView(bookView);
				}
			};
			pmc.RemoveBookEvent += delegate
			{
				HideMenuPopover();

				UIAlertView alert = new UIAlertView(StringRef.confirmation, "Are you sure you want to remove this book from the Bookshelf?", null, StringRef.no, StringRef.yes);
				alert.Dismissed += (object sender, UIButtonEventArgs e) =>
				{
					if (e.ButtonIndex == 1)
					{
						BooksOnDeviceAccessor.MarkAsRemovedBook(bookView.BookshelfBook);
						BookRemover.RemoveBookInCache(bookView.BookshelfBook);
						BookRemover.RemoveBook(bookView.BookshelfBook);
						UpdateCollectionView(bookView);
					}
				};
				alert.Show();
			};
			pmc.CancelDownloadEvent += delegate
			{
				HideMenuPopover();

				UIAlertView alert = new UIAlertView(StringRef.confirmation, "Are you sure you want to cancel the download for this book?", null, StringRef.no, StringRef.yes);
				alert.Dismissed += (object sender, UIButtonEventArgs e) =>
				{
					if (e.ButtonIndex == 1)
					{
						if (bookView.BookshelfBook.Status != Book.BookStatus.DOWNLOADED)
						{
							CancelDownload(bookView.BookshelfBook);
						}
					}
				};
				alert.Show();
			};

			menuViewController = new UIPopoverController(pmc);
			menuViewController.DidDismiss += (object sender, EventArgs e) =>
			{
				HideMenuPopover();
			};
			menuViewController.SetPopoverContentSize(new CGSize(pmc.View.Frame.Width, pmc.View.Frame.Height), true);
			menuViewController.PresentFromRect(bookView.Frame, bookView, UIPopoverArrowDirection.Any, true);
		}

		private void UpdateCollectionView(BookshelfBookView bookView)
		{
			if (dataSource != null && dataSource.BookList != null && dataSource.BookList.Count > 0)
			{
				try
				{
					bookView.RemoveFromSuperview();

					Book book = bookView.BookshelfBook.Copy();

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
						RefreshTable();
					});
				}
				catch (Exception ex)
				{
					Logger.WriteLineDebugging("BookshelfViewController - UpdateCollectionView: {0}", ex.ToString());
				}
			}
		}
	}
}

