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
    public class IntroView : UIView
    {
        public IntroView(int index)
        {
            this.BackgroundColor = UIColor.Clear;
            this.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;

            // imageView
            UIImageView imageView = new UIImageView();
            imageView.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;
            imageView.Image = UIImage.FromBundle("Assets/Backgrounds/intro_" + index.ToString() + ".png");
            imageView.Frame = new CGRect(0, 0, imageView.Image.Size.Width, imageView.Image.Size.Height);
            imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            this.AddSubview(imageView);
        }

        public void AddVersion()
        {
            UILabel versionLabel = eBriefingAppearance.GenerateLabel(21, UIColor.White, true);
            versionLabel.Frame = new CGRect(0, this.Frame.Y + 60, this.Frame.Width, 40);
            versionLabel.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;
            versionLabel.TextAlignment = UITextAlignment.Center;
            versionLabel.Text = "Version " + Settings.AppVersion;
            this.AddSubview(versionLabel);
        }
    }
}
