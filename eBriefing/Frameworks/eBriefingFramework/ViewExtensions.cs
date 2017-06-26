using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace eBriefingMobile
{
    public static class ViewExtensions
    {
        public static UIView FindFirstResponder(this UIView view)
        {
            if (view.IsFirstResponder)
            {
                return view;
            }
            
            foreach (UIView subView in view.Subviews)
            {
                var firstResponder = subView.FindFirstResponder();
                if (firstResponder != null)
                    return firstResponder;
            }
            
            return null;
        }

        public static UIView FindSuperviewOfType(this UIView view, UIView stopAt, Type type)
        {
            if (view.Superview != null)
            {
                if (type.IsAssignableFrom(view.Superview.GetType()))
                {
                    return view.Superview;
                }
                
                if (view.Superview != stopAt)
                    return view.Superview.FindSuperviewOfType(stopAt, type);
            }
            
            return null;
        }
    }
}

