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
using CoreGraphics;
using Foundation;
using UIKit;

namespace eBriefingMobile
{
    public class KeyboardAutoScroll
    {
        public enum ScrollType
        {
            SCROLLVIEW,
            TABLEVIEW,
            TEXTVIEW
        }

        private UIView parentView;
        private ScrollType scrollType;
        private NSObject keyboardObserverWillShow;
        private NSObject keyboardObserverWillHide;

        public delegate void KeyboardWillShowDelegate(CGRect keyboardBounds);

        public delegate void KeyboardWillHideDelegate();

        public event KeyboardWillShowDelegate KeyboardWillShowEvent;
        public event KeyboardWillHideDelegate KeyboardWillHideEvent;

        public void RegisterForKeyboardNotifications(UIView parentView, ScrollType scrollType)
        {
            this.parentView = parentView;
            this.scrollType = scrollType;
            
            keyboardObserverWillShow = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, KeyboardWillShowNotification);
            keyboardObserverWillHide = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, KeyboardWillHideNotification);
        }

        public void UnregisterKeyboardNotifications()
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(keyboardObserverWillShow);
            NSNotificationCenter.DefaultCenter.RemoveObserver(keyboardObserverWillHide);
        }

        public void ResetContentInset(NSNotification notification)
        {
            UIView activeView = KeyboardGetActiveView();
            if (activeView == null)
                return;
            
            if (scrollType == ScrollType.SCROLLVIEW)
            {
                UIScrollView scrollView = activeView.FindSuperviewOfType(parentView, typeof(UIScrollView)) as UIScrollView;
                
                if (scrollView == null)
                    return;
                
                double animationDuration = 0.3f;
                if (notification != null)
                {
                    animationDuration = UIKeyboard.AnimationDurationFromNotification(notification);
                }
                
                UIView.Animate(animationDuration, delegate
                {
                    scrollView.ContentInset = UIEdgeInsets.Zero;
                    scrollView.ScrollIndicatorInsets = UIEdgeInsets.Zero;
                });
            }
            else if (scrollType == ScrollType.TABLEVIEW)
            {
                UITableView tableView = activeView.FindSuperviewOfType(parentView, typeof(UITableView)) as UITableView;
                if (tableView == null)
                    return;
                
                double animationDuration = 0.3f;
                if (notification != null)
                {
                    animationDuration = UIKeyboard.AnimationDurationFromNotification(notification);
                }
                
                UIView.Animate(animationDuration, delegate
                {
                    tableView.ContentInset = UIEdgeInsets.Zero;
                    tableView.ScrollIndicatorInsets = UIEdgeInsets.Zero;
                });
            }
            else if (scrollType == ScrollType.TEXTVIEW)
            {
                UITextView textView = activeView.FindSuperviewOfType(parentView, typeof(UITextView)) as UITextView;
                if (textView == null)
                    return;

                double animationDuration = 0.3f;
                if (notification != null)
                {
                    animationDuration = UIKeyboard.AnimationDurationFromNotification(notification);
                }

                UIView.Animate(animationDuration, delegate
                {
                    textView.ContentInset = UIEdgeInsets.Zero;
                    textView.ScrollIndicatorInsets = UIEdgeInsets.Zero;
                });
            }
        }

        protected virtual UIView KeyboardGetActiveView()
        {
            return parentView.FindFirstResponder();
        }

        protected virtual void KeyboardWillShowNotification(NSNotification notification)
        {
            UIView activeView = KeyboardGetActiveView();
            if (activeView == null)
                return;
            
            CGRect keyboardBounds = UIKeyboard.FrameBeginFromNotification(notification);

            if (scrollType == ScrollType.SCROLLVIEW)
            {
                UIScrollView scrollView = activeView.FindSuperviewOfType(parentView, typeof(UIScrollView)) as UIScrollView;
                if (scrollView == null)
                    return;

                nfloat keyboardHeight;
                if (keyboardBounds.Size.Height > keyboardBounds.Size.Width)
                {
                    keyboardHeight = keyboardBounds.Size.Width;
                }
                else
                {
                    keyboardHeight = keyboardBounds.Size.Height;
                }

                UIEdgeInsets contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboardHeight, 0.0f);
                scrollView.ContentInset = contentInsets;
                scrollView.ScrollIndicatorInsets = contentInsets;

                // If activeField is hidden by keyboard, scroll it so it's visible
                CGRect viewRectAboveKeyboard = new CGRect(parentView.Frame.Location, new CGSize(parentView.Frame.Width, parentView.Frame.Size.Height - keyboardHeight));
                
                // activeFieldAbsoluteFrame is relative to this.View so does not include any scrollView.ContentOffset
                CGRect activeFieldAbsoluteFrame = activeView.Superview.ConvertRectToView(activeView.Frame, parentView);
                
                // Check if the activeField will be partially or entirely covered by the keyboard
                if (!viewRectAboveKeyboard.Contains(activeFieldAbsoluteFrame))
                {
                    // Scroll to the activeField Y position + activeField.Height + current scrollView.ContentOffset.Y - the keyboard Height
                    CGPoint scrollPoint = new CGPoint(0.0f, activeFieldAbsoluteFrame.Location.Y + activeFieldAbsoluteFrame.Height + scrollView.ContentOffset.Y - viewRectAboveKeyboard.Height);
                    scrollView.SetContentOffset(scrollPoint, true);
                }
            }
            else if (scrollType == ScrollType.TABLEVIEW)
            {
                UITableView tableView = activeView.FindSuperviewOfType(parentView, typeof(UITableView)) as UITableView;
                if (tableView == null)
                    return;

                nfloat keyboardHeight;
                if (keyboardBounds.Size.Height > keyboardBounds.Size.Width)
                {
                    keyboardHeight = keyboardBounds.Size.Width;
                }
                else
                {
                    keyboardHeight = keyboardBounds.Size.Height;
                }

                UIEdgeInsets contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboardHeight, 0.0f);
                tableView.ContentInset = contentInsets;
                tableView.ScrollIndicatorInsets = contentInsets;
                
                // If activeField is hidden by keyboard, scroll it so it's visible
                CGRect viewRectAboveKeyboard = new CGRect(parentView.Frame.Location, new CGSize(parentView.Frame.Width, parentView.Frame.Size.Height - keyboardHeight));
                
                // activeFieldAbsoluteFrame is relative to this.View so does not include any tableView.ContentOffset
                CGRect activeFieldAbsoluteFrame = activeView.Superview.ConvertRectToView(activeView.Frame, parentView);
                
                // Check if the activeField will be partially or entirely covered by the keyboard
                if (!viewRectAboveKeyboard.Contains(activeFieldAbsoluteFrame))
                {
                    // Scroll to the activeField Y position + activeField.Height + current tableView.ContentOffset.Y - the keyboard Height
                    CGPoint scrollPoint = new CGPoint(0.0f, activeFieldAbsoluteFrame.Location.Y + activeFieldAbsoluteFrame.Height + tableView.ContentOffset.Y - viewRectAboveKeyboard.Height);
                    tableView.SetContentOffset(scrollPoint, true);
                }
            }
            else if (scrollType == ScrollType.TEXTVIEW)
            {
                UITextView textView = activeView as UITextView;
                if (textView == null)
                    return;

                nfloat keyboardHeight;
                if (keyboardBounds.Size.Height > keyboardBounds.Size.Width)
                {
                    keyboardHeight = keyboardBounds.Size.Width;
                }
                else
                {
                    keyboardHeight = keyboardBounds.Size.Height;
                }

                UIEdgeInsets contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboardHeight, 0.0f);
                textView.ContentInset = contentInsets;
                textView.ScrollIndicatorInsets = contentInsets;

                // If activeField is hidden by keyboard, scroll it so it's visible
                CGRect viewRectAboveKeyboard = new CGRect(parentView.Frame.Location, new CGSize(parentView.Frame.Width, parentView.Frame.Size.Height - keyboardHeight));

                // activeFieldAbsoluteFrame is relative to this.View so does not include any tableView.ContentOffset
                CGRect activeFieldAbsoluteFrame = activeView.Superview.ConvertRectToView(activeView.Frame, parentView);

                // Check if the activeField will be partially or entirely covered by the keyboard
                if (!viewRectAboveKeyboard.Contains(activeFieldAbsoluteFrame))
                {
                    // Scroll to the activeField Y position + activeField.Height + current tableView.ContentOffset.Y - the keyboard Height
                    CGPoint scrollPoint = new CGPoint(0.0f, activeFieldAbsoluteFrame.Location.Y + activeFieldAbsoluteFrame.Height + textView.ContentOffset.Y - viewRectAboveKeyboard.Height);
                    textView.SetContentOffset(scrollPoint, true);
                }
            }

            if (KeyboardWillShowEvent != null)
            {
                KeyboardWillShowEvent(keyboardBounds);
            }
        }

        protected virtual void KeyboardWillHideNotification(NSNotification notification)
        {
            // Reset the content inset of the scrollView and animate using the current keyboard animation duration
            ResetContentInset(notification);

            if (KeyboardWillHideEvent != null)
            {
                KeyboardWillHideEvent();
            }
        }
    }
}

