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
    public class ColorPickerBar : UIView
    {
        private bool highlighter;
        private UIImageView backgroundView;
        private UIButton colorButton0;
        private UIButton colorButton1;
        private UIButton colorButton2;
        private UIButton colorButton3;

        public delegate void ColorPickerBarDelegate (UIColor color);

        public event ColorPickerBarDelegate ColorSelectedEvent;

        public ColorPickerBar(CGRect rectangle, bool highlighter) : base(rectangle)
        {
            this.highlighter = highlighter;

            // backgroundView
            backgroundView = new UIImageView(new CGRect(0, 0, rectangle.Width, rectangle.Height));
            backgroundView.Image = UIImage.FromBundle("Assets/Backgrounds/popout.png").CreateResizableImage(new UIEdgeInsets(0, 50, 0, 0));
            this.AddSubview(backgroundView);

            // colorButton0
            colorButton0 = UIButton.FromType(UIButtonType.Custom);
            colorButton0.Tag = 0;
            colorButton0.Frame = new CGRect(34, 16, 55, 15);
            colorButton0.TouchUpInside += HandleColorTouchUpInside;
            this.AddSubview(colorButton0);

            // colorButton1
            colorButton1 = UIButton.FromType(UIButtonType.Custom);
            colorButton1.Tag = 1;
            colorButton1.Frame = new CGRect(colorButton0.Frame.Right + 20, colorButton0.Frame.Y, 55, 15);
            colorButton1.TouchUpInside += HandleColorTouchUpInside;
            this.AddSubview(colorButton1);

            // colorButton2
            colorButton2 = UIButton.FromType(UIButtonType.Custom);
            colorButton2.Tag = 2;
            colorButton2.Frame = new CGRect(colorButton0.Frame.X, rectangle.Height - 31, 55, 15);
            colorButton2.TouchUpInside += HandleColorTouchUpInside;
            this.AddSubview(colorButton2);

            // colorButton3
            colorButton3 = UIButton.FromType(UIButtonType.Custom);
            colorButton3.Tag = 3;
            colorButton3.Frame = new CGRect(colorButton1.Frame.X, colorButton2.Frame.Y, 55, 15);
            colorButton3.TouchUpInside += HandleColorTouchUpInside;
            this.AddSubview(colorButton3);

            if (highlighter)
            {
                colorButton0.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/color_yellow.png"), UIControlState.Normal);
                colorButton1.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/color_cyan.png"), UIControlState.Normal);
                colorButton2.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/color_light_green.png"), UIControlState.Normal);
                colorButton3.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/color_pink.png"), UIControlState.Normal);
            }
            else
            {
                colorButton0.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/color_black.png"), UIControlState.Normal);
                colorButton1.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/color_red.png"), UIControlState.Normal);
                colorButton2.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/color_blue.png"), UIControlState.Normal);
                colorButton3.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/color_dark_green.png"), UIControlState.Normal);
            }
        }

        void HandleColorTouchUpInside(object sender, EventArgs e)
        {
            UIButton button = sender as UIButton;

            UIColor color;

            if (highlighter)
            {
                if (button.Tag == 0)
                {
                    color = eBriefingAppearance.Color(240, 232, 66);
                    Settings.WriteDefaultHighlighterColor(StringRef.Yellow);
                }
                else if (button.Tag == 1)
                {
                    color = eBriefingAppearance.Color(31, 162, 251);
                    Settings.WriteDefaultHighlighterColor(StringRef.Cyan);
                }
                else if (button.Tag == 2)
                {
                    color = eBriefingAppearance.Color(0, 228, 52);
                    Settings.WriteDefaultHighlighterColor(StringRef.LightGreen);
                }
                else
                {
                    color = eBriefingAppearance.Color(255, 33, 169);
                    Settings.WriteDefaultHighlighterColor(StringRef.Pink);
                }
            }
            else
            {
                if (button.Tag == 0)
                {
                    color = UIColor.Black;
                    Settings.WriteDefaultPenColor(StringRef.Black);
                }
                else if (button.Tag == 1)
                {
                    color = eBriefingAppearance.Color(255, 1, 22);
                    Settings.WriteDefaultPenColor(StringRef.Red);
                }
                else if (button.Tag == 2)
                {
                    color = eBriefingAppearance.Color(33, 105, 222);
                    Settings.WriteDefaultPenColor(StringRef.Blue);
                }
                else
                {
                    color = eBriefingAppearance.Color(0, 167, 86);
                    Settings.WriteDefaultPenColor(StringRef.DarkGreen);
                }
            }

            if (ColorSelectedEvent != null)
            {
                ColorSelectedEvent(color);
            }
        }
    }
}

