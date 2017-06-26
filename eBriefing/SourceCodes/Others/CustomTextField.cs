using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace eBriefingMobile
{
    public class CustomTextField : UITextField
    {
        private String placeholderText;
        private Int32 offset;

        public CustomTextField(String placeholderText, Int32 offset = 10)
        {
            this.placeholderText = placeholderText;
            this.offset = offset;

            this.BorderStyle = UITextBorderStyle.None;
            this.Layer.BorderColor = eBriefingAppearance.Color("A1A1A1").CGColor;
            this.Layer.BorderWidth = 1.0f;

            this.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
        }

        public override void DrawPlaceholder(RectangleF rect)
        {
            using (UIFont font = UIFont.SystemFontOfSize(17f))
            {
                using (UIColor color = eBriefingAppearance.Color("A1A1A1"))
                {
                    color.SetFill();

                    base.DrawString(placeholderText, new PointF(rect.X, rect.Y + offset), font);
                }
            }
        }

        public override RectangleF TextRect(RectangleF forBounds)
        {
            return new RectangleF(10, 0, forBounds.Width, forBounds.Height);
        }

        public override RectangleF EditingRect(RectangleF forBounds)
        {
            return new RectangleF(10, 0, forBounds.Width, forBounds.Height);
        }
    }
}

