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
using Metrostar.Mobile.Framework;
using SpinKitBinding;

namespace eBriefingMobile
{
    public static class LoadingView
    {
        private static UIAlertView alert;

        public delegate void LoadingViewDelegate ();

        public static event LoadingViewDelegate CancelEvent;

        public static void Show(String title, String message, bool showCancel = true)
        {
			if (alert == null || (alert !=null && showCancel==true))
            {
                if (showCancel)
                {
                    alert = new UIAlertView(title, message, null, StringRef.cancel, null);
                }
                else
                {
                    alert = new UIAlertView(title, message, null, null, null);
                }
                alert.Dismissed += HandleDismissed;

                UIView customView = new UIView();
                customView.BackgroundColor = UIColor.Clear;
                customView.Frame = new CGRect(0, 0, 270, 48);

                try
                {
                    RTSpinKitView spinner = eBriefingAppearance.GenerateBounceSpinner();
                    spinner.Frame = new CGRect(customView.Center.X - (spinner.Frame.Width / 2), 0, spinner.Frame.Width, spinner.Frame.Height);
                    customView.AddSubview(spinner);
                }
                catch (Exception ex)
                {
                    Logger.WriteLineDebugging("LoadingView - Show: {0}", ex.ToString());
                }

                alert.SetValueForKey(customView, new NSString("accessoryView"));
                alert.Show();
            }
            else
            {
                alert.Title = title;
                alert.Message = message;
            }
        }

        public static void Hide()
        {
            if (alert != null)
            {
                try
                {
                    alert.Dismissed -= HandleDismissed;
                    alert.DismissWithClickedButtonIndex(0, true);
                    alert = null;
                }
                catch (Exception ex)
                {
                    Logger.WriteLineDebugging("eBriefingViewController - LoadingView: {0}", ex.ToString());
                }
            }
        }

        static void HandleDismissed(object sender, UIButtonEventArgs e)
        {
            if (e.ButtonIndex == 0)
            {
				Hide ();

                if (CancelEvent != null)
                {
                    CancelEvent();
                }
            }
        }
    }
}