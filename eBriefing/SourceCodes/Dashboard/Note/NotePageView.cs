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
    public class NotePageView : UIView
    {
        public NotePageView(Page page, nfloat width)
        {
            this.BackgroundColor = eBriefingAppearance.Gray3;
            this.Frame = new CGRect(0, 0, width, width);
            this.Layer.MasksToBounds = true;
            this.Layer.BorderWidth = 0;
            this.Layer.CornerRadius = width / 2;

            // subView
            UIView subView = new UIView(new CGRect(2, 2, width - 4, width - 4));
            subView.BackgroundColor = UIColor.White;
            subView.Layer.MasksToBounds = true;
            subView.Layer.BorderWidth = 0;
            subView.Layer.CornerRadius = width / 2;
            this.AddSubview(subView);

            // pageLabel
            UILabel pageLabel = eBriefingAppearance.GenerateLabel(21, eBriefingAppearance.Gray2);
            pageLabel.Text = "page";
            pageLabel.Frame = new CGRect(0, 15, subView.Frame.Width, 30);
            pageLabel.TextAlignment = UITextAlignment.Center;
            subView.AddSubview(pageLabel);

            // numberLabel
            UILabel numberLabel = eBriefingAppearance.GenerateLabel(25, eBriefingAppearance.Gray1, true);
            numberLabel.Text = page.PageNumber.ToString();
            numberLabel.Frame = new CGRect(0, pageLabel.Frame.Bottom, subView.Frame.Width, 21);
            numberLabel.TextAlignment = UITextAlignment.Center;
            subView.AddSubview(numberLabel);
        }
    }
}

