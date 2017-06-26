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
using System.IO;
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreGraphics;
using ObjCRuntime;
using Metrostar.Mobile.Framework;
using SpinKitBinding;
using ASIHTTPRequestBinding;

namespace eBriefingMobile
{
	public class BookshelfBookView : UIView
	{
		private UILabel titleLabel;
		private UILabel descriptionLabel;
		private UILabel downloadLabel;
		private BookInfoView bookInfoView;
		private RTSpinKitView downloadSpinner;
		private RTSpinKitView imageSpinner;
		private UIProgressView progressView;
		private UIImageView imageView;
		private UIImageView favoriteView;
		private UIImageView ribbonView;
		private UIButton updateButton;
		private bool updateMenu;

		public Book BookshelfBook { get; set; }

		public delegate void BookshelfBookViewDelegate0();

		public delegate void BookshelfBookViewDelegate1(Book book);

		public delegate void BookshelfBookViewDelegate2(BookshelfBookView bookView, bool isFavorite);

		public event BookshelfBookViewDelegate0 RefreshBookshelfEvent;
		public event BookshelfBookViewDelegate1 UpdateBookEvent;
		public event BookshelfBookViewDelegate2 ShowMenuEvent;

		public BookshelfBookView(Book book, bool updateMenu, BookshelfViewController parentVC) : base(new CGRect(0, 0, 280, 280))
		{
			this.BookshelfBook = book;
			this.updateMenu = updateMenu;

			this.BackgroundColor = UIColor.White;
			this.Layer.ShadowColor = UIColor.Black.CGColor;
			this.Layer.ShadowOpacity = 0.3f;
			this.Layer.ShadowRadius = 2f;
			this.Layer.ShadowOffset = new CGSize(5f, 5f);

			// imageView
			imageView = new UIImageView();
			imageView.Frame = new CGRect(0, 0, this.Frame.Width, 150);
			this.AddSubview(imageView);

			// recognizer
			this.AddGestureRecognizer(new UILongPressGestureRecognizer(this, new Selector("HandleLongPress:")));

			if (!String.IsNullOrEmpty(BookshelfBook.LargeImageURL))
			{
				// imageSpinner
				imageSpinner = eBriefingAppearance.GenerateBounceSpinner();
				imageSpinner.Center = imageView.Center;
				this.AddSubview(imageSpinner);

				// Download image
				bool exist = FileDownloader.Download(BookshelfBook.LargeImageURL, parentVC);
				if (exist)
				{
					bool outDated = false;
					var item = BooksOnDeviceAccessor.GetBook(BookshelfBook.ID);
					if (item != null)
					{
						if (item.ImageVersion < BookshelfBook.ImageVersion)
						{
							DownloadedFilesCache.RemoveFile(item.LargeImageURL);
							DownloadedFilesCache.RemoveFile(item.SmallImageURL);

							outDated = true;
						}
					}

					if (outDated)
					{
						FileDownloader.Download(BookshelfBook.LargeImageURL, parentVC, true);
					}
					else
					{
						UpdateImage(BookshelfBook.LargeImageURL);
					}
				}
			}

			// favoriteView
			if (BooksDataAccessor.IsFavorite(book.ID))
			{
				AddFavorite();
			}

			if (book.New)
			{
				AddRibbon();
			}

			// titleLabel
			titleLabel = eBriefingAppearance.GenerateLabel(16);
			titleLabel.Frame = new CGRect(10, imageView.Frame.Bottom + 8, 260, 21);
			titleLabel.Lines = 2;
			titleLabel.LineBreakMode = UILineBreakMode.WordWrap;
			titleLabel.Text = book.Title;
			titleLabel.SizeToFit();
			titleLabel.Frame = new CGRect(10, titleLabel.Frame.Y, 260, titleLabel.Frame.Height);
			this.AddSubview(titleLabel);

			UpdateUI();
		}

		public void UpdateImage(String url)
		{
			if (imageSpinner != null)
			{
				imageSpinner.StopAnimating();
			}

			String localPath = DownloadedFilesCache.BuildCachedFilePath(url);
			UIImage image = UIImage.FromFile(localPath);
			if (image != null)
			{
				imageView.Image = image;
			}
		}

		public void UpdateProgressView(float progress)
		{
			if (progressView != null)
			{
				BookshelfBook.DownloadProgress = progress;
				progressView.SetProgress((float)progress, true);
			}
		}

		public void UpdateStatus(bool updating)
		{
			if (updating)
			{
				UpdateUI();

				ShowHideInfo(0f);
			}

			UpdateDownloadLabel();
		}

		public void Wait2Update()
		{
			UpdateUI();

			ShowHideInfo(0f);
		}

		public void FinishedDownloading()
		{
			// Update UI
			UpdateUI();
		}

		public void UpdateFavorite(bool isFavorite)
		{
			if (isFavorite)
			{
				AddFavorite();
			}
			else
			{
				RemoveFavorite();
			}

			BooksOnDeviceAccessor.UpdateFavorite(BookshelfBook, isFavorite);

			if (RefreshBookshelfEvent != null)
			{
				RefreshBookshelfEvent();
			}
		}

		[Export("HandleLongPress:")]
		protected void HandleLongPress(UIGestureRecognizer sender)
		{
			if (sender.State == UIGestureRecognizerState.Began && !updateMenu)
			{
				bool isFavorite = BooksOnDeviceAccessor.IsFavorite(BookshelfBook.ID);

				if (ShowMenuEvent != null)
				{
					ShowMenuEvent(this, isFavorite);
				}
			}
		}

		private void ShowHideInfo(nfloat value)
		{
			bookInfoView.Alpha = value;

			if (descriptionLabel != null)
			{
				descriptionLabel.Alpha = value;
			}

			if (updateButton != null)
			{
				updateButton.Alpha = value;
			}
		}

		private void AddRibbon()
		{
			// ribbonView
			ribbonView = new UIImageView();
			ribbonView.Image = UIImage.FromBundle("Assets/Icons/new.png");
			ribbonView.Frame = new CGRect(this.Frame.Width - ribbonView.Image.Size.Width, imageView.Frame.Bottom - ribbonView.Image.Size.Height, ribbonView.Image.Size.Width, ribbonView.Image.Size.Height);
			this.AddSubview(ribbonView);
		}

		private void AddFavorite()
		{
			// favoriteView
			favoriteView = new UIImageView();
			favoriteView.Image = UIImage.FromBundle("Assets/Icons/favorite.png");
			favoriteView.Frame = new CGRect(0, 0, favoriteView.Image.Size.Width, favoriteView.Image.Size.Height);
			this.AddSubview(favoriteView);
		}

		private void RemoveFavorite()
		{
			if (favoriteView != null)
			{
				favoriteView.RemoveFromSuperview();
				favoriteView.Dispose();
				favoriteView = null;
			}
		}

		private void UpdateDownloadLabel()
		{
			if (downloadLabel != null)
			{
				if (BookshelfBook.Status == Book.BookStatus.PENDING2DOWNLOAD)
				{
					downloadLabel.Text = "Waiting to Download...";
				}
				else if (BookshelfBook.Status == Book.BookStatus.PENDING2UPDATE)
				{
					downloadLabel.Text = "Waiting to Update...";
				}
				else if (BookshelfBook.Status == Book.BookStatus.DOWNLOADING)
				{
					downloadLabel.Text = "Downloading...";
				}
				else if (BookshelfBook.Status == Book.BookStatus.UPDATING)
				{
					downloadLabel.Text = "Updating...";
				}
			}
		}

		private void UpdateUI()
		{
			if (BookshelfBook.Status == Book.BookStatus.DOWNLOADED || BookshelfBook.Status == Book.BookStatus.ISUPDATE)
			{
				if (downloadSpinner != null)
				{
					downloadSpinner.StopAnimating();
					downloadSpinner.RemoveFromSuperview();
					downloadSpinner.Dispose();
					downloadSpinner = null;
				}

				if (downloadLabel != null)
				{
					downloadLabel.RemoveFromSuperview();
					downloadLabel.Dispose();
					downloadLabel = null;
				}

				if (progressView != null)
				{
					progressView.RemoveFromSuperview();
					progressView.Dispose();
					progressView = null;
				}

				// bookInfoView
				if (bookInfoView == null)
				{
					UpdateBookInfo();
				}

				if (BookshelfBook.Status == Book.BookStatus.DOWNLOADED)
				{
					// descriptionLabel
					if (descriptionLabel == null)
					{
						descriptionLabel = eBriefingAppearance.GenerateLabel(14, eBriefingAppearance.Gray2);
						descriptionLabel.Frame = new CGRect(10, titleLabel.Frame.Bottom + 4, 260, 42);
						descriptionLabel.Lines = 2;
						descriptionLabel.Text = BookshelfBook.Description;
						descriptionLabel.Alpha = 0f;
						this.AddSubview(descriptionLabel);
					}
				}
				else
				{
					// updateButton
					if (updateButton == null)
					{
						updateButton = UIButton.FromType(UIButtonType.Custom);
						updateButton.Font = eBriefingAppearance.ThemeBoldFont(14);
						updateButton.SetTitleColor(eBriefingAppearance.Color("37b878"), UIControlState.Normal);
						updateButton.SetTitleColor(UIColor.White, UIControlState.Highlighted);
						updateButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/green_unfilled.png").CreateResizableImage(new UIEdgeInsets(0, 14f, 0, 14f)), UIControlState.Normal);
						updateButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/green_filled.png").CreateResizableImage(new UIEdgeInsets(0, 14f, 0, 14f)), UIControlState.Highlighted);
						updateButton.Frame = new CGRect(this.Center.X - 65, bookInfoView.Frame.Top - 28, 130, updateButton.CurrentBackgroundImage.Size.Height);
						updateButton.SetTitle("UPDATE", UIControlState.Normal);
						updateButton.TouchUpInside += HandleUpdateButtonTouchUpInside;
						this.AddSubview(updateButton);
					}
				}

				ShowHideInfo(1f);
			}
			else
			{
				// downloadSpinner
				if (downloadSpinner == null)
				{
					downloadSpinner = eBriefingAppearance.GenerateBounceSpinner();
					downloadSpinner.Frame = new CGRect(10, (this.Frame.Bottom - 50), downloadSpinner.Frame.Width, downloadSpinner.Frame.Height);
					this.AddSubview(downloadSpinner);
				}

				// downloadLabel
				if (downloadLabel == null)
				{
					downloadLabel = eBriefingAppearance.GenerateLabel(17);
					downloadLabel.Frame = new CGRect(downloadSpinner.Frame.Right + 8, downloadSpinner.Frame.Y + 2, this.Frame.Width - (downloadSpinner.Frame.Right + 8) - 10, 21);
					this.AddSubview(downloadLabel);

					UpdateDownloadLabel();
				}

				// progressView
				if (progressView == null)
				{
					progressView = new UIProgressView(UIProgressViewStyle.Default);
					progressView.Frame = new CGRect(downloadLabel.Frame.X, downloadLabel.Frame.Bottom + 8, downloadLabel.Frame.Width, progressView.Frame.Height);
					progressView.ProgressTintColor = eBriefingAppearance.BlueColor;
					progressView.Progress = BookshelfBook.DownloadProgress;
					this.AddSubview(progressView);
				}
			}
		}

		private void UpdateBookInfo()
		{
			String numNotes = "0";
			String numBookmarks = "0";
			String numAnnotations = "0";

			bookInfoView = new BookInfoView(numNotes, numBookmarks, numAnnotations, BookshelfBook.PageCount.ToString(), false, false, this.Frame.Width - 30);
			bookInfoView.Frame = new CGRect(10, this.Frame.Bottom - 44, bookInfoView.Frame.Width, bookInfoView.Frame.Height);
			bookInfoView.Alpha = 0f;
			this.AddSubview(bookInfoView);

			numNotes = BooksOnDeviceAccessor.GetNumNotesInBook(BookshelfBook.ID);
			numBookmarks = BooksOnDeviceAccessor.GetNumBookmarksInBook(BookshelfBook.ID);
			numAnnotations = BooksOnDeviceAccessor.GetNumAnnotationsInBook(BookshelfBook.ID);

			bookInfoView.NumNoteLabel.Text = numNotes;
			bookInfoView.NumBookmarkLabel.Text = numBookmarks;
			bookInfoView.NumAnnLabel.Text = numAnnotations;
		}

		void HandleUpdateButtonTouchUpInside(object sender, EventArgs e)
		{
			if (UpdateBookEvent != null)
			{
				UpdateBookEvent(BookshelfBook);
			}
		}
	}
}


