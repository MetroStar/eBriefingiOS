using System;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreAnimation;

namespace eBriefing
{
    public class FlowLayout : UICollectionViewFlowLayout
    {
        public FlowLayout()
        {
            ScrollDirection = UICollectionViewScrollDirection.Vertical;
            ItemSize = new SizeF(280, 280);
            SectionInset = new UIEdgeInsets(0, 0, 20, 0);
            HeaderReferenceSize = new SizeF(0, 0);
            MinimumLineSpacing = 20.0f;
        }

        public override bool ShouldInvalidateLayoutForBoundsChange(RectangleF newBounds)
        {
            return true;
        }
    }
}

