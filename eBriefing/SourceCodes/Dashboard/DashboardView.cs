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
using Foundation;
using UIKit;
using CoreGraphics;
using Metrostar.Mobile.Framework;

namespace eBriefingMobile
{
	public class DashboardView : UIScrollView
	{
		private Book book;
		private BookInfoView bookInfoView;

		public delegate void DashboardViewDelegate();

		public event DashboardViewDelegate InfoUpdatedEvent;

		public DashboardView(Book book, CGRect frame) : base(frame)
		{
			this.book = book;
			this.BackgroundColor = UIColor.White;

			// overview
			UILabel overview = eBriefingAppearance.GenerateLabel(21);
			overview.Frame = new CGRect(20, 20, frame.Width - 40, 21);
			overview.Text = "Overview";
			this.AddSubview(overview);

			// imageView
			UIImageView imageView = new UIImageView();
			imageView.Frame = new CGRect(overview.Frame.X, overview.Frame.Bottom + 16, overview.Frame.Width, 150);

			String localImagePath = DownloadedFilesCache.BuildCachedFilePath(book.LargeImageURL);
			if (File.Exists(localImagePath))
			{
				imageView.Image = UIImage.FromFile(localImagePath);
			}

			this.AddSubview(imageView);

			// bookInfoView
			bookInfoView = new BookInfoView("0", "0", "0", book.PageCount.ToString(), false, false, frame.Width - 60);
			bookInfoView.Frame = new CGRect(30, imageView.Frame.Bottom + 8, bookInfoView.Frame.Width, bookInfoView.Frame.Height);
			this.AddSubview(bookInfoView);

			// line0
			UIView line0 = new UIView();
			line0.BackgroundColor = eBriefingAppearance.Gray4;
			line0.Frame = new CGRect(overview.Frame.X, bookInfoView.Frame.Bottom + 10, overview.Frame.Width, 1);
			this.AddSubview(line0);

			// descLabel
			UILabel descLabel = GenerateLabel("Description", true);
			descLabel.Frame = new CGRect(overview.Frame.X, line0.Frame.Bottom + 10, overview.Frame.Width, 21);
			this.AddSubview(descLabel);

			// desc
			UILabel desc = GenerateLabel(book.Description, false);
			desc.Frame = new CGRect(overview.Frame.X, descLabel.Frame.Bottom + 8, overview.Frame.Width, 1);
			desc.Lines = 0;
			desc.SizeToFit();
			this.AddSubview(desc);

			// line1
			UIView line1 = new UIView();
			line1.BackgroundColor = eBriefingAppearance.Gray4;
			line1.Frame = new CGRect(overview.Frame.X, desc.Frame.Bottom + 10, overview.Frame.Width, 1);
			this.AddSubview(line1);

			// versionLabel
			UILabel versionLabel = GenerateLabel("Version", true);
			versionLabel.Frame = new CGRect(overview.Frame.X, line1.Frame.Bottom + 10, overview.Frame.Width, 21);
			this.AddSubview(versionLabel);

			// version
			UILabel version = GenerateLabel(book.Version.ToString(), false);
			version.Frame = new CGRect(overview.Frame.X, versionLabel.Frame.Bottom + 8, overview.Frame.Width, 21);
			this.AddSubview(version);

			// line2
			UIView line2 = new UIView();
			line2.BackgroundColor = eBriefingAppearance.Gray4;
			line2.Frame = new CGRect(overview.Frame.X, version.Frame.Bottom + 10, overview.Frame.Width, 1);
			this.AddSubview(line2);

			// createdLabel
			UILabel createdLabel = GenerateLabel("Date Created", true);
			createdLabel.Frame = new CGRect(overview.Frame.X, line2.Frame.Bottom + 10, overview.Frame.Width, 21);
			this.AddSubview(createdLabel);

			// created
			UILabel created = GenerateLabel(book.UserAddedDate.ToString("MMMM dd, yyyy"), false);
			created.Frame = new CGRect(overview.Frame.X, createdLabel.Frame.Bottom + 8, overview.Frame.Width, 21);
			this.AddSubview(created);

			// line3
			UIView line3 = new UIView();
			line3.BackgroundColor = eBriefingAppearance.Gray4;
			line3.Frame = new CGRect(overview.Frame.X, created.Frame.Bottom + 10, overview.Frame.Width, 1);
			this.AddSubview(line3);

			// modifiedLabel
			UILabel modifiedLabel = GenerateLabel("Date Modified", true);
			modifiedLabel.Frame = new CGRect(overview.Frame.X, line3.Frame.Bottom + 10, overview.Frame.Width, 21);
			this.AddSubview(modifiedLabel);

			// modified
			UILabel modified = GenerateLabel(book.UserModifiedDate.ToString("MMMM dd, yyyy"), false);
			modified.Frame = new CGRect(overview.Frame.X, modifiedLabel.Frame.Bottom + 8, overview.Frame.Width, 21);
			this.AddSubview(modified);

			// shadow
			UIImageView shadow = new UIImageView();
			shadow.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			shadow.Frame = new CGRect(frame.Right - 3, 0, 3, frame.Height);
			shadow.Image = UIImage.FromBundle("Assets/Backgrounds/shadow.png").CreateResizableImage(new UIEdgeInsets(2, 0, 2, 0));
			this.AddSubview(shadow);

			this.ContentSize = new CGSize(frame.Width, modified.Frame.Bottom + 20);
		}

		public void UpdateBookInfo()
		{
			if (bookInfoView != null)
			{
				String numNotes = BooksOnDeviceAccessor.GetNumNotesInBook(book.ID);
				String numBookmarks = BooksOnDeviceAccessor.GetNumBookmarksInBook(book.ID);
				String numAnnotations = BooksOnDeviceAccessor.GetNumAnnotationsInBook(book.ID);

				bookInfoView.NumNoteLabel.Text = numNotes;
				bookInfoView.NumBookmarkLabel.Text = numBookmarks;
				bookInfoView.NumAnnLabel.Text = numAnnotations;

				if (InfoUpdatedEvent != null)
				{
					InfoUpdatedEvent();
				}
			}
		}

		private UILabel GenerateLabel(String text, bool bold)
		{
			UILabel label = eBriefingAppearance.GenerateLabel();

			if (bold)
			{
				label.Font = eBriefingAppearance.ThemeBoldFont(17);
			}
			else
			{
				label.Font = eBriefingAppearance.ThemeRegularFont(17);
			}
			label.Text = text;

			return label;
		}
	}
}