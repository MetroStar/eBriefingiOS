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
    public class ThumbnailCell : UICollectionViewCell
    {
        private ThumbnailView thumbnailView;

        public String PageID { get; set; }

        public ThumbnailView ThumbnailImage
        {
            set
            {
                thumbnailView = value;
                ContentView.AddSubview(thumbnailView);
            }
        }

        [Export("initWithFrame:")]
        public ThumbnailCell(CGRect frame) : base(frame)
        {
            BackgroundView = new UIView {
                BackgroundColor = UIColor.Clear
            };

            SelectedBackgroundView = new UIView {
                BackgroundColor = UIColor.Clear
            };

            ContentView.Layer.BorderColor = UIColor.LightGray.CGColor;
            ContentView.BackgroundColor = UIColor.Clear;
        }

        public override void PrepareForReuse()
        {
            base.PrepareForReuse();

            PageID = String.Empty;

            if (thumbnailView != null)
            {
                thumbnailView.RemoveFromSuperview();
                thumbnailView.Dispose();
                thumbnailView = null;
            }
        }
    }
}

