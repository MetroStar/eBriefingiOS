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
using System.Linq;
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreGraphics;
using ObjCRuntime;
using Metrostar.Mobile.Framework;
using SpinKitBinding;

namespace eBriefingMobile
{
	public class LibraryBookView : UIView
	{
		private UIImageView imageView;
		private RTSpinKitView imageSpinner;

		public Book LibraryBook { get; set; }

		public delegate void LibraryBookViewDelegate();

		public event LibraryBookViewDelegate DownloadEvent;

		public LibraryBookView(Book book, LibraryViewController parentVC) : base(new CGRect(0, 0, 280, 280))
		{
			this.LibraryBook = book;

			this.BackgroundColor = UIColor.White;
			this.Layer.ShadowColor = UIColor.Black.CGColor;
			this.Layer.ShadowOpacity = 0.3f;
			this.Layer.ShadowRadius = 2f;
			this.Layer.ShadowOffset = new CGSize(5f, 5f);

			// imageView
			imageView = new UIImageView();
			imageView.Frame = new CGRect(0, 0, this.Frame.Width, 150);
			this.AddSubview(imageView);

			if (!String.IsNullOrEmpty(LibraryBook.LargeImageURL))
			{
				// imageSpinner
				imageSpinner = eBriefingAppearance.GenerateBounceSpinner();
				imageSpinner.Center = imageView.Center;
				this.AddSubview(imageSpinner);

				// Download image
				bool exist = FileDownloader.Download(LibraryBook.LargeImageURL, parentVC);
				if (exist)
				{
					bool outDated = false;
					var item = BooksOnServerAccessor.GetBook(LibraryBook.ID);
					if (item != null)
					{
						if (item.ImageVersion < LibraryBook.ImageVersion)
						{
							DownloadedFilesCache.RemoveFile(item.LargeImageURL);
							DownloadedFilesCache.RemoveFile(item.SmallImageURL);

							outDated = true;
						}
					}

					if (outDated)
					{
						FileDownloader.Download(LibraryBook.LargeImageURL, parentVC, true);
					}
					else
					{
						UpdateImage(LibraryBook.LargeImageURL);
					}
				}
			}

			// titleLabel
			UILabel titleLabel = eBriefingAppearance.GenerateLabel(16);
			titleLabel.Frame = new CGRect(10, imageView.Frame.Bottom + 8, 260, 21);
			titleLabel.Lines = 2;
			titleLabel.LineBreakMode = UILineBreakMode.WordWrap;
			titleLabel.Text = book.Title;
			titleLabel.SizeToFit();
			titleLabel.Frame = new CGRect(10, titleLabel.Frame.Y, 260, titleLabel.Frame.Height);
			this.AddSubview(titleLabel);

			// bookInfoView
			BookInfoView bookInfoView = new BookInfoView("0", "0", "0", book.PageCount.ToString(), false, false, this.Frame.Width - 30);
			bookInfoView.Frame = new CGRect(10, this.Frame.Bottom - 44, bookInfoView.Frame.Width, bookInfoView.Frame.Height);
			this.AddSubview(bookInfoView);

			// downloadButton
			UIButton downloadButton = UIButton.FromType(UIButtonType.Custom);
			downloadButton.Font = eBriefingAppearance.ThemeBoldFont(14);
			downloadButton.SetTitleColor(eBriefingAppearance.Color("37b878"), UIControlState.Normal);
			downloadButton.SetTitleColor(UIColor.White, UIControlState.Highlighted);
			downloadButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/green_unfilled.png").CreateResizableImage(new UIEdgeInsets(15f, 14f, 15f, 14f)), UIControlState.Normal);
			downloadButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/green_filled.png").CreateResizableImage(new UIEdgeInsets(15f, 14f, 15f, 14f)), UIControlState.Highlighted);
			downloadButton.Frame = new CGRect(this.Center.X - 65, bookInfoView.Frame.Top - 28, 130, downloadButton.CurrentBackgroundImage.Size.Height);
			downloadButton.SetTitle("DOWNLOAD", UIControlState.Normal);
			downloadButton.TouchUpInside += HandleDownloadButtonTouchUpInside;
			this.AddSubview(downloadButton);
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

		void HandleDownloadButtonTouchUpInside(object sender, EventArgs e)
		{
			if (DownloadEvent != null)
			{
				DownloadEvent();
			}
		}
	}
}

