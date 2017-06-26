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
using Foundation;
using UIKit;
using CoreGraphics;
using Metrostar.Mobile.Framework;

namespace eBriefingMobile
{
	public class ChapterView : UIView
	{
		public Chapter BookChapter { get; set; }

		public ChapterView(String bookID, Chapter chapter, int index) : base(new CGRect(0, 0, 220, 388.33f))
		{
			this.BookChapter = chapter;

			this.BackgroundColor = UIColor.White;
			this.Layer.ShadowColor = UIColor.Black.CGColor;
			this.Layer.ShadowOpacity = 0.3f;
			this.Layer.ShadowRadius = 2f;
			this.Layer.ShadowOffset = new CGSize(5f, 5f);

			// imageView
			UIImageView imageView = new UIImageView();
			imageView.Frame = new CGRect(0, 0, this.Frame.Width, 293.33f);
			String localImagePath = DownloadedFilesCache.BuildCachedFilePath(BookChapter.LargeImageURL);
			imageView.Image = UIImage.FromFile(localImagePath);
			imageView.ContentMode = UIViewContentMode.ScaleToFill;
			this.AddSubview(imageView);

			// chapterLabel
			UILabel chapterLabel = eBriefingAppearance.GenerateLabel(14);
			chapterLabel.Frame = new CGRect(10, imageView.Frame.Bottom + 8, 200, 21);
			chapterLabel.Text = "Chapter " + (index + 1).ToString();
			this.AddSubview(chapterLabel);
            
			// titleLabel
			UILabel titleLabel = eBriefingAppearance.GenerateLabel(14);
			titleLabel.Frame = new CGRect(10, chapterLabel.Frame.Bottom + 4, 200, 21);
			titleLabel.LineBreakMode = UILineBreakMode.TailTruncation;
			titleLabel.Text = chapter.Title;
			this.AddSubview(titleLabel);

			// Add book info layer
			AddBookInfoView(bookID, chapter);
		}

		private void AddBookInfoView(String bookID, Chapter chapter)
		{
			String numNotes = "0";
			String numBookmarks = "0";
			String numAnnotations = "0";

			numNotes = BooksOnDeviceAccessor.GetNumNotesInChapter(bookID, chapter.ID);
			numBookmarks = BooksOnDeviceAccessor.GetNumBookmarksInChapter(bookID, chapter.ID);
			numAnnotations = BooksOnDeviceAccessor.GetNumAnnotationsInChapter(bookID, chapter.ID);

			// bookInfoView
			BookInfoView bookInfoView = new BookInfoView(numNotes, numBookmarks, numAnnotations, chapter.Pagecount.ToString(), false, false, this.Frame.Width - 30);
			bookInfoView.Frame = new CGRect(10, this.Frame.Bottom - 40, bookInfoView.Frame.Width, bookInfoView.Frame.Height);
			this.AddSubview(bookInfoView);
		}
	}
}

