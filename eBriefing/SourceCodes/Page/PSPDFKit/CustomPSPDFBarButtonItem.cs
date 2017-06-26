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
using PSPDFKit;

namespace eBriefingMobile
{
    public class CustomPSPDFBarButtonItem : PSPDFBarButtonItem
    {
        public UIButton BarButton { get; set; }

        public delegate void ButtonClickedDelegate ();

        public event ButtonClickedDelegate ButtonClickedEvent;

        public CustomPSPDFBarButtonItem(PSPDFViewController viewController) : base(viewController)
        {

        }

        public override UIView CustomView
        {
            get
            {
                if (BarButton == null)
                {
                    BarButton = UIButton.FromType(UIButtonType.Custom);
                    BarButton.TintColor = eBriefingAppearance.BlueColor;
                    BarButton.Frame = new CGRect(0, 0, 30, 30);
                    BarButton.ShowsTouchWhenHighlighted = true;
                    BarButton.TouchUpInside += HandleTouchUpInside;
                }

                return BarButton;
            }
        }

        public void UpdateButtonImage(UIImage image)
        {
            if (BarButton != null)
            {
                BarButton.SetImage(image, UIControlState.Normal);
            }
        }

        void HandleTouchUpInside(object sender, EventArgs e)
        {
            if (ButtonClickedEvent != null)
            {
                ButtonClickedEvent();
            }
        }
    }
}

