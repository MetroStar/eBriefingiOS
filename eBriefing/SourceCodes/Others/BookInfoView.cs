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

namespace eBriefingMobile
{
    public class BookInfoView : UIView
    {
        private UIImageView noteView;
        private UIImageView bookmarkView;
        private UIImageView annView;
        private UIImageView pageView;

        public UILabel NumNoteLabel  { get; set; }

        public UILabel NumBookmarkLabel { get; set; }

        public UILabel NumAnnLabel  { get; set; }

        public UILabel NumPageLabel { get; set; }

        public BookInfoView(String noteCount, String bookmarkCount, String annCount, String pageCount, bool whiteIcon, bool whiteColor, nfloat width) : base(new CGRect(0, 0, width, 40))
        {
            this.BackgroundColor = UIColor.Clear;

            // NumNoteLabel
            NumNoteLabel = eBriefingAppearance.GenerateLabel(16);
            NumNoteLabel.Text = noteCount;
            if (whiteColor)
            {
                NumNoteLabel.TextColor = UIColor.White;
            }
            else
            {
                NumNoteLabel.TextColor = eBriefingAppearance.Gray1;
            }
            NumNoteLabel.Frame = new CGRect(0, 0, 21, 40);
            NumNoteLabel.AdjustsFontSizeToFitWidth = true;
            NumNoteLabel.TextAlignment = UITextAlignment.Right;
            this.AddSubview(NumNoteLabel);

            // noteView
            noteView = new UIImageView();
            if (whiteIcon)
            {
                noteView.Image = UIImage.FromBundle("Assets/Icons/stat_note_white.png");
            }
            else
            {
                noteView.Image = UIImage.FromBundle("Assets/Icons/stat_note_gray.png");
            }
            noteView.Frame = new CGRect(NumNoteLabel.Frame.Right + 4, 11, 18, 18);
            this.AddSubview(noteView);

            // NumBookmarkLabel
            NumBookmarkLabel = eBriefingAppearance.GenerateLabel(16);
            NumBookmarkLabel.Text = bookmarkCount;
            if (whiteColor)
            {
                NumBookmarkLabel.TextColor = UIColor.White;
            }
            else
            {
                NumBookmarkLabel.TextColor = eBriefingAppearance.Gray1;
            }
            NumBookmarkLabel.Frame = new CGRect(noteView.Frame.Right + 8, 0, 21, 40);
            NumBookmarkLabel.AdjustsFontSizeToFitWidth = true;
            NumBookmarkLabel.TextAlignment = UITextAlignment.Right;
            this.AddSubview(NumBookmarkLabel);

            // bookmarkView
            bookmarkView = new UIImageView();
            if (whiteIcon)
            {
                bookmarkView.Image = UIImage.FromBundle("Assets/Icons/stat_bookmark_white.png");
            }
            else
            {
                bookmarkView.Image = UIImage.FromBundle("Assets/Icons/stat_bookmark_gray.png");
            }
            bookmarkView.Frame = new CGRect(NumBookmarkLabel.Frame.Right + 4, 11, 18, 18);
            this.AddSubview(bookmarkView);

            // NumAnnLabel
            NumAnnLabel = eBriefingAppearance.GenerateLabel(16);
            NumAnnLabel.Text = annCount;
            NumAnnLabel.Frame = new CGRect(bookmarkView.Frame.Right + 8, 0, 21, 40);
            if (whiteColor)
            {
                NumAnnLabel.TextColor = UIColor.White;
            }
            else
            {
                NumAnnLabel.TextColor = eBriefingAppearance.Gray1;
            }
            NumAnnLabel.AdjustsFontSizeToFitWidth = true;
            NumAnnLabel.TextAlignment = UITextAlignment.Right;
            this.AddSubview(NumAnnLabel);

            // annView
            annView = new UIImageView();
            if (whiteIcon)
            {
                annView.Image = UIImage.FromBundle("Assets/Icons/stat_ann_white.png");
            }
            else
            {
                annView.Image = UIImage.FromBundle("Assets/Icons/stat_ann_gray.png");
            }
            annView.Frame = new CGRect(NumAnnLabel.Frame.Right + 4, 11, 18, 18);
            this.AddSubview(annView);

            // pageView
            pageView = new UIImageView();
            if (whiteIcon)
            {
                pageView.Image = UIImage.FromBundle("Assets/Icons/stat_page_white.png");
            }
            else
            {
                pageView.Image = UIImage.FromBundle("Assets/Icons/stat_page_gray.png");
            }
            pageView.Frame = new CGRect(width - 8, 11, 18, 18);
            this.AddSubview(pageView);

            // NumPageLabel
            NumPageLabel = eBriefingAppearance.GenerateLabel(16);
            NumPageLabel.Text = pageCount;
            NumPageLabel.TextAlignment = UITextAlignment.Right;
            if (whiteColor)
            {
                NumPageLabel.TextColor = UIColor.White;
            }
            else
            {
                NumPageLabel.TextColor = eBriefingAppearance.Gray1;
            }
            NumPageLabel.AdjustsFontSizeToFitWidth = true;
            this.AddSubview(NumPageLabel);

            if (NumPageLabel.Text.Length <= 3)
            {
                NumPageLabel.Frame = new CGRect(pageView.Frame.Left - 44, 0, 40, 40);
            }
            else
            {
                NumPageLabel.Frame = new CGRect(pageView.Frame.Left - 32, 0, 32, 40);
            }
        }
    }
}

