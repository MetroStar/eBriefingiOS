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
    public class SegmentedView : UIView
    {
        private UISegmentedControl segCtrl1;
        private UISegmentedControl segCtrl2;

        public delegate void SegmentedViewDelegate ();

        public event SegmentedViewDelegate ValueChanged;

        public SegmentedView()
        {
            this.BackgroundColor = UIColor.Clear;

            // segCtrl1
            segCtrl1 = new UISegmentedControl();
            segCtrl1.InsertSegment("Title", 0, false);
            segCtrl1.InsertSegment("Added", 1, false);
            segCtrl1.InsertSegment("Modified", 2, false);
            segCtrl1.Frame = new CGRect(0, 0, 280, 29);
            segCtrl1.ValueChanged += HandleValue1Changed;
            this.AddSubview(segCtrl1);

            if (Settings.SortBy == StringRef.ByName)
            {
                segCtrl1.SelectedSegment = 0;
            }
            else if (Settings.SortBy == StringRef.ByDateAdded)
            {
                segCtrl1.SelectedSegment = 1;
            }
            else
            {
                segCtrl1.SelectedSegment = 2;
            }

            // segCtrl2
            segCtrl2 = new UISegmentedControl();
            segCtrl2.InsertSegment(UIImage.FromBundle("Assets/Buttons/ascending.png"), 0, false);
            segCtrl2.InsertSegment(UIImage.FromBundle("Assets/Buttons/descending.png"), 1, false);
            segCtrl2.Frame = new CGRect(segCtrl1.Frame.Right + 20, 0, 100, 29);
            segCtrl2.ValueChanged += HandleValue2Changed;
            this.AddSubview(segCtrl2);

            if (Settings.SortAscending)
            {
                segCtrl2.SelectedSegment = 0;
            }
            else
            {
                segCtrl2.SelectedSegment = 1;
            }

            this.Frame = new CGRect(0, 0, segCtrl2.Frame.Right, segCtrl2.Frame.Height);
        }

        void HandleValue1Changed(object sender, EventArgs e)
        {
            var selectedSegmentID = (sender as UISegmentedControl).SelectedSegment;
            if (selectedSegmentID == 0)
            {
                Settings.WriteSortBy(StringRef.ByName);
            }
            else if (selectedSegmentID == 1)
            {
                Settings.WriteSortBy(StringRef.ByDateAdded);
            }
            else
            {
                Settings.WriteSortBy(StringRef.ByDateModified);
            }

            if (ValueChanged != null)
            {
                ValueChanged();
            }
        }

        void HandleValue2Changed(object sender, EventArgs e)
        {
            var selectedSegmentID = (sender as UISegmentedControl).SelectedSegment;
            if (selectedSegmentID == 0)
            {
                Settings.WriteSortAscending(true);
            }
            else
            {
                Settings.WriteSortAscending(false);
            }

            if (ValueChanged != null)
            {
                ValueChanged();
            }
        }
    }
}

