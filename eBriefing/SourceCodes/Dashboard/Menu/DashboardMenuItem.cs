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
using CoreAnimation;

namespace eBriefingMobile
{
    public class DashboardMenuItem : UIView
    {
        public UIButton LeftButton { get; set; }

        private UIButton rightButton;
        private UIButton bottomButton;
        private static nfloat MAX_WIDTH = 200;

        public delegate void DashboardMenuDelegate ();

        public event DashboardMenuDelegate LeftEvent;
        public event DashboardMenuDelegate RightEvent;
        public event DashboardMenuDelegate BottomEvent;

        public DashboardMenuItem(String leftStr, String rightStr, String menuTitle)
        {
            this.BackgroundColor = eBriefingAppearance.BlueColor;

            nfloat width = MAX_WIDTH;
            if (!String.IsNullOrEmpty(leftStr) && !String.IsNullOrEmpty(rightStr))
            {
                width = width / 2f;
            }

            // LeftButton
            if (!String.IsNullOrEmpty(leftStr))
            {
                LeftButton = UIButton.FromType(UIButtonType.Custom);
                LeftButton.Frame = new CGRect(0, 0, width, 37);
                LeftButton.Font = eBriefingAppearance.ThemeRegularFont(17f);
                LeftButton.BackgroundColor = UIColor.Clear;
                LeftButton.SetTitle(leftStr, UIControlState.Normal);
                LeftButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                LeftButton.TouchUpInside += HandleLeftTouchUpInside;
                LeftButton.Layer.BorderColor = UIColor.White.CGColor;
                LeftButton.Layer.BorderWidth = 1f;
                this.AddSubview(LeftButton);
            }

            // rightButton
            if (!String.IsNullOrEmpty(rightStr))
            {
                rightButton = UIButton.FromType(UIButtonType.Custom);
                rightButton.Frame = new CGRect(width, LeftButton.Frame.Y, width, LeftButton.Frame.Height);
                rightButton.Font = eBriefingAppearance.ThemeRegularFont(17f);
                rightButton.BackgroundColor = UIColor.Clear;
                rightButton.SetTitle(rightStr, UIControlState.Normal);
                rightButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                rightButton.TouchUpInside += HandleRightTouchUpInside;
                rightButton.Layer.BorderColor = UIColor.White.CGColor;
                rightButton.Layer.BorderWidth = 1f;
                this.AddSubview(rightButton);
            }

            // bottomButton
            bottomButton = UIButton.FromType(UIButtonType.Custom);
            bottomButton.Frame = new CGRect(0, LeftButton.Frame.Bottom + 10, MAX_WIDTH, 21);
            bottomButton.Font = eBriefingAppearance.ThemeRegularFont(17f);
            bottomButton.BackgroundColor = UIColor.Clear;
            bottomButton.SetTitle(menuTitle, UIControlState.Normal);
            bottomButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            bottomButton.TouchUpInside += HandleBottomTouchUpInside;
            this.AddSubview(bottomButton);

            this.Frame = new CGRect(0, 0, MAX_WIDTH, bottomButton.Frame.Bottom);
        }

        public void ExpandCollapse(bool expand)
        {
            if (expand)
            {
                this.Frame = new CGRect(this.Frame.X, 20, this.Frame.Width, this.Frame.Height + LeftButton.Frame.Height);
            }
            else
            {
                this.Frame = new CGRect(this.Frame.X, -LeftButton.Frame.Height, this.Frame.Width, this.Frame.Height - LeftButton.Frame.Height);
            }
        }

        void HandleLeftTouchUpInside(object sender, EventArgs e)
        {
            if (LeftEvent != null)
            {
                LeftEvent();
            }
        }

        void HandleRightTouchUpInside(object sender, EventArgs e)
        {
            if (RightEvent != null)
            {
                RightEvent();
            }
        }

        void HandleBottomTouchUpInside(object sender, EventArgs e)
        {
            if (BottomEvent != null)
            {
                BottomEvent();
            }
        }
    }
}