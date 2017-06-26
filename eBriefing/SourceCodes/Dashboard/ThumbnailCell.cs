using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

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
        public ThumbnailCell(RectangleF frame) : base(frame)
        {
            BackgroundView = new UIView
            {
                BackgroundColor = UIColor.Clear
            };

            SelectedBackgroundView = new UIView
            {
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

