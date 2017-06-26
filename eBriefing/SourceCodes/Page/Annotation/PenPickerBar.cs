using System;
using Foundation;
using UIKit;
using CoreGraphics;

namespace eBriefingMobile
{
    public class PenPickerBar : UIView
    {
        private UIImageView backgroundView;
        private UIButton pen;
        private UIButton highlighter;

        public delegate void PenPickerBarDelegate ();

        public event PenPickerBarDelegate PenSelectedEvent;
        public event PenPickerBarDelegate HighlighterSelectedEvent;

        public PenPickerBar(CGRect rectangle) : base(rectangle)
        {
            // backgroundView
            backgroundView = new UIImageView(new CGRect(0, 0, rectangle.Width, rectangle.Height));
            backgroundView.Image = UIImage.FromBundle("Assets/Backgrounds/popout.png").CreateResizableImage(new UIEdgeInsets(5, 50, 5, 0));
            this.AddSubview(backgroundView);

            // pen
            pen = UIButton.FromType(UIButtonType.Custom);
            pen.Frame = new CGRect(24, 6, 72, 72);
            pen.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/toolBar_unpressed.png"), UIControlState.Normal);
            pen.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/toolBar_pressed.png"), UIControlState.Highlighted);
            pen.SetImage(UIImage.FromBundle("Assets/Icons/pen.png"), UIControlState.Normal);
            pen.SetImage(UIImage.FromBundle("Assets/Icons/pen.png"), UIControlState.Highlighted);
            pen.TouchUpInside += HandlePenTouchUpInside;
            this.AddSubview(pen);

            // highlighter
            highlighter = UIButton.FromType(UIButtonType.Custom);
            highlighter.Frame = new CGRect(pen.Frame.Right + 5, pen.Frame.Y, 72, 72);
            highlighter.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/toolBar_unpressed.png"), UIControlState.Normal);
            highlighter.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/toolBar_pressed.png"), UIControlState.Highlighted);
            highlighter.SetImage(UIImage.FromBundle("Assets/Icons/highlighter.png"), UIControlState.Normal);
            highlighter.SetImage(UIImage.FromBundle("Assets/Icons/highlighter.png"), UIControlState.Highlighted);
            highlighter.TouchUpInside += HandleHighlighterTouchUpInside;
            this.AddSubview(highlighter);
        }

        void HandlePenTouchUpInside(object sender, EventArgs e)
        {
            if (PenSelectedEvent != null)
            {
                PenSelectedEvent();
            }
        }

        void HandleHighlighterTouchUpInside(object sender, EventArgs e)
        {
            if (HighlighterSelectedEvent != null)
            {
                HighlighterSelectedEvent();
            }
        }
    }
}

