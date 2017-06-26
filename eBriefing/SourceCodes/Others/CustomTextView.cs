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
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreGraphics;
using Metrostar.Mobile.Framework;

namespace eBriefingMobile
{
    public class CustomTextView : UITextView
    {
        public string PlaceHolderText { get; set; }

        public UIColor PlaceHolderColor { get; set; }

        public UILabel PlaceHolder { get; set; }

        public CustomTextView()
        {
            NSNotificationCenter.DefaultCenter.AddObserver((NSString)"UITextViewTextDidChangeNotification", TextChanged);
        }

        void TextChanged(NSNotification notification)
        {
            try
            {
                CGRect line = GetCaretRectForPosition(SelectedTextRange.Start);
                nfloat overflow = line.Y + line.Height - (ContentOffset.Y + Bounds.Size.Height - ContentInset.Bottom - ContentInset.Top);

                if (overflow > 0)
                {
                    // We are at the bottom of the visible text and introduced a line feed, scroll down (iOS 7 does not do it)
                    // Scroll caret to visible area
                    CGPoint offset = ContentOffset;
                    offset.Y += overflow + 8;

                    UIView.Animate(0.3d, delegate
                    {
                        this.SetContentOffset(offset, true);
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("CustomTextView - TextChanged: {0}", ex.ToString());
            }
        }
    }
}

