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
using System.Net;
using Foundation;
using UIKit;
using CoreGraphics;
using MssFramework;
using Metrostar.Mobile.Framework;
using SpinKitBinding;

namespace eBriefingMobile
{
    public partial class AuthenticationController : DispatcherViewController
    {
        private AuthenticationSubView authSubview;

        public delegate void AuthenticationDelegate ();

        public event AuthenticationDelegate DismissedEvent;

        public AuthenticationController()
        {

        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view, typically from a nib.
            InitializeControls();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // Start authentication check
            StartAuthentication();
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.Default;
        }

        private void InitializeControls()
        {
            this.View.BackgroundColor = UIColor.White;
            this.NavigationItem.BackBarButtonItem = new UIBarButtonItem(String.Empty, UIBarButtonItemStyle.Plain, null);
            this.NavigationController.SetNavigationBarHidden(true, false);

            if (InterfaceOrientation == UIInterfaceOrientation.Portrait || InterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown)
            {
                this.View.Frame = new CGRect(0, 0, 768, 1024);
            }
            else
            {
                this.View.Frame = new CGRect(0, 0, 1024, 768);
            }
            this.View.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;

            // authSubview
            authSubview = new AuthenticationSubView(this.View.Frame);
            authSubview.Center = this.View.Center;
            this.View.AddSubview(authSubview);
        }

        async private void StartAuthentication()
        {
            if (Reachability.IsDefaultNetworkAvailable())
            {
                if (!String.IsNullOrEmpty(URL.ServerURL))
                {
                    bool verified = await eBriefingService.Run(() => CheckCredential());
                    if (verified)
                    {
                        Dismiss();
                    }
                    else
                    {
                        ShowErrorView();
                    }
                }
            }
            else
            {
                ShowErrorView();
            }
        }

        private bool CheckCredential()
        {
			var status1 = WebExceptionStatus.ConnectFailure;

			// Check 2013 server info first, if xml returned is null, then use 2010 server
			var status=Server.CheckCompatibility2013(URL.ServerURL, Settings.UserID, KeychainAccessor.Password, Settings.Domain);

			if ( status != WebExceptionStatus.Success )
			{
				status1 = Server.CheckCompatibility2010 (URL.ServerURL, Settings.UserID, KeychainAccessor.Password, Settings.Domain);
			}

			if (status== WebExceptionStatus.Success || status1== WebExceptionStatus.Success)
			{
				if (Settings.UseFormsAuth)
				{
					String formsError = Authenticate.Forms(URL.ServerURL, Settings.UserID, KeychainAccessor.Password);
					if (String.IsNullOrEmpty(formsError))
					{
						return true;
					}
				}
				else
				{
					WebExceptionStatus errorStatus = Authenticate.NTLM(URL.ServerURL, Settings.UserID, KeychainAccessor.Password, Settings.Domain);
					if (errorStatus == WebExceptionStatus.Success)
					{
						return true;
					}
				}
			}

            return false;
        }

        private void ShowErrorView()
        {
            UIAlertView alertView = new UIAlertView(StringRef.connectionFailure, "Cannot access Library. Please check that your Library settings are correct.", null, "Exit", new string[] {
                "Try Again",
                "Change Connection Settings",
                "Work Offline"
            });
            alertView.Dismissed += HandleDismissed;
            alertView.Show();
        }

        private void Dismiss()
        {
            Settings.Authenticated = true;

            this.DismissViewController(true, delegate
            {
                if (DismissedEvent != null)
                {
                    DismissedEvent();
                }
            });
        }

        void HandleDismissed(object sender, UIButtonEventArgs e)
        {
            if (e.ButtonIndex == 0)
            {
                // Exit
                iOSUtilities.ForceCloseThisApp();
            }
            else if (e.ButtonIndex == 1)
            {
                // Try again
                StartAuthentication();
            }
            else if (e.ButtonIndex == 2)
            {
                // Change connection settings
                SelectServerViewController ssvc = new SelectServerViewController(false);
                ssvc.DismissEvent += (bool cancelled) =>
                {
                    if (!cancelled)
                    {
                        Dismiss();
                    }
                };

                this.NavigationController.PushViewController(ssvc, true);
            }
            else
            {
                // Work Offline
                Dismiss();
            }
        }
    }

    public class AuthenticationSubView : UIView
    {
        public AuthenticationSubView(CGRect frame)
        {
            this.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;

            try
            {
                // spinner
                RTSpinKitView spinner = eBriefingAppearance.GenerateBounceSpinner();
                spinner.Frame = new CGRect(0, 0, spinner.Frame.Width, spinner.Frame.Height);
                this.AddSubview(spinner);

                // StatusLabel
                UILabel label = eBriefingAppearance.GenerateLabel(25, eBriefingAppearance.BlueColor);
                label.Frame = new CGRect(spinner.Frame.Right + 10, spinner.Center.Y - (40f / 2f), 300, 40);
                label.TextAlignment = UITextAlignment.Center;
                label.Text = StringRef.checkingConnection;
                label.SizeToFit();
                label.Frame = new CGRect(label.Frame.X, label.Frame.Y, label.Frame.Width, 40);
                this.AddSubview(label);

                this.Frame = new CGRect(0, 0, label.Frame.Right, 40);
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("AuthenticationSubView - AuthenticationSubView: {0}", ex.ToString());
            }
        }
    }
}

