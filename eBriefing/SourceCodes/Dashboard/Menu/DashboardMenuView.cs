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
    public class DashboardMenuView : UIView
    {
        private DashboardMenuItem leftItem;
        private DashboardMenuItem centerItem;
        private DashboardMenuItem rightItem;
        private UIView line0;
        private UIView line1;
        private bool full = false;

        public delegate void DashboardMenuDelegate ();

        public event DashboardMenuDelegate GoToCurrentEvent;
        public event DashboardMenuDelegate GoToFirstEvent;
        public event DashboardMenuDelegate CollapseEvent;
        public event DashboardMenuDelegate ExpandEvent;
        public event DashboardMenuDelegate SortEvent;

        public DashboardMenuView()
        {
            this.BackgroundColor = eBriefingAppearance.BlueColor;

            // leftItem
            leftItem = new DashboardMenuItem("current", "first", "go to page");
            leftItem.Frame = new CGRect(20, -37, leftItem.Frame.Width, leftItem.Frame.Height);
            leftItem.LeftEvent += HandleLeftLeftEvent;
            leftItem.RightEvent += HandleLeftRightEvent;
            leftItem.BottomEvent += HandleBottomEvent;
            this.AddSubview(leftItem);

            // centerItem
            centerItem = new DashboardMenuItem("expand all", String.Empty, "expand / collapse");
            centerItem.Frame = new CGRect(leftItem.Frame.Right + 60, leftItem.Frame.Y, centerItem.Frame.Width, centerItem.Frame.Height);
            centerItem.LeftEvent += HandleCenterLeftEvent;
            centerItem.BottomEvent += HandleBottomEvent;
            this.AddSubview(centerItem);

            // rightItem
            rightItem = new DashboardMenuItem("descending", String.Empty, "sort");
            rightItem.Frame = new CGRect(centerItem.Frame.Right + 60, leftItem.Frame.Y, rightItem.Frame.Width, rightItem.Frame.Height);
            rightItem.LeftEvent += HandleRightLeftEvent;
            rightItem.BottomEvent += HandleBottomEvent;
            this.AddSubview(rightItem);

            this.Frame = new CGRect(0, 0, rightItem.Frame.Right + 20, leftItem.Frame.Bottom + 10);
            ApplyMask();

            // line0
            line0 = new UIView();
            line0.Frame = new CGRect(leftItem.Frame.Right + 30, this.Bounds.Top + 10, 1, this.Bounds.Bottom - 20);
            line0.BackgroundColor = UIColor.White;
            line0.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
            this.AddSubview(line0);

            // line1
            line1 = new UIView();
            line1.Frame = new CGRect(centerItem.Frame.Right + 30, line0.Frame.Y, line0.Frame.Width, line0.Frame.Height);
            line1.BackgroundColor = UIColor.White;
            line1.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
            this.AddSubview(line1);
        }

        void HandleLeftLeftEvent()
        {
            if (GoToCurrentEvent != null)
            {
                GoToCurrentEvent();
            }
        }

        void HandleLeftRightEvent()
        {
            if (GoToFirstEvent != null)
            {
                GoToFirstEvent();
            }
        }

        void HandleCenterLeftEvent()
        {
			String text = String.Empty;
			if (centerItem.LeftButton.Title(UIControlState.Normal).Contains("exp"))
			{
				text = "collapse all";

				if (ExpandEvent != null)
				{
					ExpandEvent();
				}
			}
			else
			{
				text = "expand all";

				if (CollapseEvent != null)
				{
					CollapseEvent();
				}
			}

			centerItem.LeftButton.SetTitle(text, UIControlState.Normal);
        }

        void HandleRightLeftEvent()
        {
            String text = String.Empty;
            if (rightItem.LeftButton.Title(UIControlState.Normal).Contains("asc"))
            {
				text = "descending";
            }
            else
            {
				text = "ascending";
            }

            rightItem.LeftButton.SetTitle(text, UIControlState.Normal);

            if (SortEvent != null)
            {
                SortEvent();
            }
        }

        void HandleBottomEvent()
        {
            if (!full)
            {
                UIView.Animate(0.15d, delegate
                {
                    ExpandNCollapse(!full);
                });
            }
        }

        private void ApplyMask()
        {
            CAShapeLayer maskLayer = new CAShapeLayer();
            maskLayer.Path = UIBezierPath.FromRoundedRect(this.Bounds, UIRectCorner.TopLeft, new CGSize(10, 10)).CGPath;
            this.Layer.Mask = maskLayer;
        }

        public void ExpandNCollapse(bool expand)
        {
            if (full != expand)
            {
                full = expand;

                nfloat menuY = this.Frame.Y;
                nfloat menuHeight = this.Frame.Height;

                if (expand)
                {
                    menuY -= 57;
                    menuHeight += 57;
                }
                else
                {
                    menuHeight -= 57;
                }

                this.Frame = new CGRect(this.Frame.X, menuY, this.Frame.Width, menuHeight);

                if (expand)
                {
                    ApplyMask();
                }

                leftItem.ExpandCollapse(expand);
                centerItem.ExpandCollapse(expand);
                rightItem.ExpandCollapse(expand);
            }
        }
    }
}