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
    public class PopoverSearchResultView : UIView
    {
        public PopoverSearchResultView(Book book, CGRect frame) : base(frame)
        {
            this.BackgroundColor = UIColor.Clear;

            // imageView
            UIImageView imageView = new UIImageView(new CGRect(10, 10, 80, 80));
            imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            this.AddSubview(imageView);

            String localImagePath = DownloadedFilesCache.BuildCachedFilePath(book.LargeImageURL);
            if (File.Exists(localImagePath))
            {
                imageView.Image = UIImage.FromFile(localImagePath);
            }

            TitleDescriptionView tdView = new TitleDescriptionView(book.Title, book.Description, frame.Width - (imageView.Frame.Width + 30));
            tdView.Frame = new CGRect(imageView.Frame.Right + 10, frame.Height / 2 - tdView.Frame.Height / 2, tdView.Frame.Width, tdView.TotalHeight);
            this.AddSubview(tdView);
        }
    }

    public class TitleDescriptionView : UIView
    {
        public nfloat TotalHeight { get; set; }

        public TitleDescriptionView(String title, String description, nfloat width)
        {
            // titleLabel
            UILabel titleLabel = eBriefingAppearance.GenerateLabel(18, eBriefingAppearance.Gray1, true);
            titleLabel.Frame = new CGRect(0, 0, width, 21);
            titleLabel.Text = title;
            this.AddSubview(titleLabel);

            // descLabel
            UILabel descLabel = eBriefingAppearance.GenerateLabel(16);
            descLabel.Frame = new CGRect(titleLabel.Frame.X, titleLabel.Frame.Bottom, width, 21);
            descLabel.Text = description;
            descLabel.Lines = 2;
            descLabel.LineBreakMode = UILineBreakMode.WordWrap;
            descLabel.SizeToFit();
            this.AddSubview(descLabel);

            TotalHeight = descLabel.Frame.Bottom;

            this.Frame = new CGRect(0, 0, width, TotalHeight);
        }
    }
}

