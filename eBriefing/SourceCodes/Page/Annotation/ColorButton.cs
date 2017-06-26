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
    public class ColorButton : UIButton
    {
        private UIImageView imageView;
        private UILabel label;

        public ColorButton(CGRect rectangle, bool highlighter) : base(rectangle)
        {
            this.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/toolBar_unpressed.png"), UIControlState.Normal);
            this.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/toolBar_pressed.png"), UIControlState.Selected);
            this.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/toolBar_pressed.png"), UIControlState.Selected | UIControlState.Highlighted);

            // image
            imageView = new UIImageView(new CGRect((rectangle.Width / 2) - (55f / 2f), 16, 55, 15));
            this.AddSubview(imageView);

            // label
            label = eBriefingAppearance.GenerateLabel(15, UIColor.White);
            label.Frame = new CGRect(0, imageView.Frame.Bottom + 8, this.Frame.Width, 21);
            label.TextAlignment = UITextAlignment.Center;
            label.BackgroundColor = UIColor.Clear;
            this.AddSubview(label);

            UpdateColor(highlighter);
        }

        public void UpdateColor(bool highlighter)
        {
            if (highlighter)
            {
                if (String.IsNullOrEmpty(Settings.DefaultHighlighterColor) || Settings.DefaultHighlighterColor.Equals(StringRef.Yellow))
                {
                    imageView.Image = UIImage.FromBundle("Assets/Buttons/color_yellow.png");
                    label.Text = StringRef.Yellow.ToUpper();
                }
                else if (Settings.DefaultHighlighterColor.Equals(StringRef.Cyan))
                {
                    imageView.Image = UIImage.FromBundle("Assets/Buttons/color_cyan.png");
                    label.Text = StringRef.Cyan.ToUpper();
                }
                else if (Settings.DefaultHighlighterColor.Equals(StringRef.LightGreen))
                {
                    imageView.Image = UIImage.FromBundle("Assets/Buttons/color_light_green.png");
                    label.Text = "GREEN";
                }
                else
                {
                    imageView.Image = UIImage.FromBundle("Assets/Buttons/color_pink.png");
                    label.Text = StringRef.Pink.ToUpper();
                }
            }
            else
            {
                if (String.IsNullOrEmpty(Settings.DefaultPenColor) || Settings.DefaultPenColor.Equals(StringRef.Black))
                {
                    imageView.Image = UIImage.FromBundle("Assets/Buttons/color_black.png");
                    label.Text = StringRef.Black.ToUpper();
                }
                else if (Settings.DefaultPenColor.Equals(StringRef.Red))
                {
                    imageView.Image = UIImage.FromBundle("Assets/Buttons/color_red.png");
                    label.Text = StringRef.Red.ToUpper();
                }
                else if (Settings.DefaultPenColor.Equals(StringRef.Blue))
                {
                    imageView.Image = UIImage.FromBundle("Assets/Buttons/color_blue.png");
                    label.Text = StringRef.Blue.ToUpper();
                }
                else
                {
                    imageView.Image = UIImage.FromBundle("Assets/Buttons/color_dark_green.png");
                    label.Text = "GREEN";
                }
            }
        }
    }
}

