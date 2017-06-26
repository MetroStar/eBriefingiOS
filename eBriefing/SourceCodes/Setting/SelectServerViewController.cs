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
using System.Threading;
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreGraphics;
using MssFramework;
using Metrostar.Mobile.Framework;
using eBriefing.com.metrostarsystems.ebriefingweb3;

namespace eBriefingMobile
{
    public partial class SelectServerViewController : DispatcherViewController
    {
        private bool showCancel = false;
        private bool isFirst = true;
        private CancellationTokenSource cts;
        private UIImageView helpView;
        private UIButton demoButton;
        private UIButton enterpriseButton;
        //private UIButton learnButton;

        public delegate void SelectServerViewDelegate0 ();

        public delegate void SelectServerViewDelegate1 (bool cancelled);

        public event SelectServerViewDelegate1 DismissEvent;
        public event SelectServerViewDelegate0 CancelDownloadEvent;

        public SelectServerViewController(bool showCancel)
        {
            this.showCancel = showCancel;
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
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Initialize controls
            if (isFirst)
            {
                isFirst = false;

                InitializeControls();
            }

            UpdateScreen(InterfaceOrientation);

            BookUpdater.RegisterASIDelegate(this);
        }

        public override void WillAnimateRotation(UIInterfaceOrientation toInterfaceOrientation, double duration)
        {
            base.WillAnimateRotation(toInterfaceOrientation, duration);

            UpdateScreen(toInterfaceOrientation);
        }

        private void InitializeControls()
        {
            this.View.BackgroundColor = eBriefingAppearance.BlueColor;

            this.NavigationItem.Title = "Select a Library";
            this.NavigationItem.BackBarButtonItem = new UIBarButtonItem(String.Empty, UIBarButtonItemStyle.Plain, null);
            this.NavigationController.SetNavigationBarHidden(false, true);
			this.NavigationController.NavigationBar.TintColor = UIColor.White;
            this.NavigationController.NavigationBar.SetBackgroundImage(UIImage.FromBundle("Assets/Backgrounds/navbar.png").CreateResizableImage(new UIEdgeInsets(0, 1, 0, 1)), UIBarMetrics.Default);

            // Add cancel button if this is not startup
            if (showCancel)
            {
                this.NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, HandleCancelButtonTouchUpInside);
            }

            // demoButton
            demoButton = UIButton.FromType(UIButtonType.Custom);
            demoButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/demo.png"), UIControlState.Normal);
            demoButton.Frame = new CGRect(0, 0, 424, 259);
            demoButton.TouchUpInside += HandleDemoButtonTouchUpInside;
            this.View.AddSubview(demoButton);

            // enterpriseButton
            enterpriseButton = UIButton.FromType(UIButtonType.Custom);
            enterpriseButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/enterprise.png"), UIControlState.Normal);
            enterpriseButton.Frame = new CGRect(0, 0, 424, 259);
            enterpriseButton.TouchUpInside += HandleEnterpriseButtonTouchUpInside;
            this.View.AddSubview(enterpriseButton);

            // helpView
            helpView = new UIImageView();
            this.View.AddSubview(helpView);

//            // learnButton
//            learnButton = UIButton.FromType(UIButtonType.Custom);
//            learnButton.Frame = new CGRect(0, 0, 220, 44);
//            learnButton.Font = eBriefingAppearance.ThemeBoldFont(21);
//            learnButton.SetTitle("Learn More", UIControlState.Normal);
//            learnButton.SetTitleColor(UIColor.White, UIControlState.Normal);
//            learnButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/green_unfilled.png").CreateResizableImage(new UIEdgeInsets(15f, 14f, 15f, 14f)), UIControlState.Normal);
//            learnButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/white_unfilled.png").CreateResizableImage(new UIEdgeInsets(0f, 22f, 0f, 22f)), UIControlState.Highlighted);
//            learnButton.TouchUpInside += HandleLearnButtonTouchUpInside;
//            this.View.AddSubview(learnButton);
        }

        private void UpdateScreen(UIInterfaceOrientation orientation)
        {
            if (orientation == UIInterfaceOrientation.Portrait || orientation == UIInterfaceOrientation.PortraitUpsideDown)
            {
                demoButton.Frame = new CGRect((this.View.Frame.Width / 2) - (demoButton.Frame.Width / 2), (this.View.Frame.Height / 2) - demoButton.Frame.Height - 20 - (126f / 2f), demoButton.Frame.Width, demoButton.Frame.Height);
                enterpriseButton.Frame = new CGRect(demoButton.Frame.X, demoButton.Frame.Bottom + 20, enterpriseButton.Frame.Width, enterpriseButton.Frame.Height);
                helpView.Image = UIImage.FromBundle("Assets/Backgrounds/help_portrait.png");
                helpView.Frame = new CGRect(demoButton.Frame.X, enterpriseButton.Frame.Bottom + 30, 424, 126);
                //learnButton.Frame = new CGRect(enterpriseButton.Frame.Right - learnButton.Frame.Width - 10, helpView.Frame.Y, learnButton.Frame.Width, learnButton.Frame.Height);
            }
            else
            {
                demoButton.Frame = new CGRect((this.View.Frame.Width / 2) - (demoButton.Frame.Width) - 10, (this.View.Frame.Height / 2) - (demoButton.Frame.Height / 2) - 20 - (110f / 2f), demoButton.Frame.Width, demoButton.Frame.Height);
                enterpriseButton.Frame = new CGRect((this.View.Frame.Width / 2) + 10, demoButton.Frame.Y, enterpriseButton.Frame.Width, enterpriseButton.Frame.Height);
                helpView.Image = UIImage.FromBundle("Assets/Backgrounds/help_landscape.png");
                helpView.Frame = new CGRect(demoButton.Frame.X, demoButton.Frame.Bottom + 30, 861, 110);
                //learnButton.Frame = new CGRect(enterpriseButton.Frame.Right - learnButton.Frame.Width - 10, helpView.Frame.Y + 20, learnButton.Frame.Width, learnButton.Frame.Height);
            }
        }

        private void Dismiss(bool cancelled)
        {
            this.InvokeOnMainThread(delegate
            {
                LoadingView.Hide();

                this.DismissViewController(true, delegate
                {
                    if (DismissEvent != null)
                    {
                        DismissEvent(cancelled);
                    }
                });
            });
        }

        private void CancelAllDownloads()
        {
            if (CancelDownloadEvent != null)
            {
                CancelDownloadEvent();
            }
        }

        private void StartAuthenticate(out WebExceptionStatus errorStatus, out bool notCompatible)
        {
            String id = "ebriefingdemoacct";
            String password = "5t@Wa*7uT%c#A!";
			String domain = "ebriefingprod";

            notCompatible = false;
            errorStatus = WebExceptionStatus.ConnectFailure;

            try
            {
				var status= Server.CheckCompatibility2010(StringRef.DemoURL, id, password, domain);
				if (status==WebExceptionStatus.Success)
                {
                    errorStatus = Authenticate.NTLM_Authenticate(StringRef.DemoURL, id, password, domain);
                }
                else
                {
					errorStatus=status;
                }
            }
            catch (WebException ex)
            {
                errorStatus = ex.Status;
            }
        }

        async private void Connect()
        {
            if (!Reachability.IsDefaultNetworkAvailable())
            {
                AlertView.Show(StringRef.connectionFailure, StringRef.connectionRequired, StringRef.ok);
            }
            else
            {
                try
                {
                    bool notCompatible = false;
                    WebExceptionStatus errorStatus = WebExceptionStatus.ConnectFailure;
                    cts = new CancellationTokenSource();

                    Settings.WriteUseFormsAuth(false);

                    LoadingView.Show("Connecting", "Please wait while we're connecting to Demo Library...");
                    await eBriefingService.Run(() => StartAuthenticate(out errorStatus, out notCompatible), cts.Token);
                    LoadingView.Hide();
                    cts.Token.ThrowIfCancellationRequested();

                    if (errorStatus == WebExceptionStatus.Success)
                    {
                        Settings.Authenticated = true;
                        Settings.AvailableCheckTime = DateTime.MinValue;
                        Settings.WriteSyncOn(false);

                        CancelAllDownloads();

                        // Success
                        Dismiss(false);
                    }
                    else
                    {
                        Settings.Authenticated = false;

                        if (notCompatible)
                        {
                            AlertView.Show(StringRef.alert, StringRef.notCompatible, StringRef.ok);
                        }
                        else
                        {
							WebExceptionAlertView.ShowAlert(errorStatus);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    KeychainAccessor.ClearCredential();
                }
            }
        }

        void HandleDemoButtonTouchUpInside(object sender, EventArgs e)
        {
            if (!String.Equals(URL.ServerURL, StringRef.DemoURL))
            {
                Connect();
            }
            else
            {
                Dismiss(true);
            }
        }

        void HandleEnterpriseButtonTouchUpInside(object sender, EventArgs e)
        {
            EnterpriseViewController evc = new EnterpriseViewController();
            evc.DismissEvent += (bool cancelled) =>
            {
                Dismiss(cancelled);
            };
            evc.CancelDownloadEvent += CancelAllDownloads;

            this.NavigationController.PushViewController(evc, true);
        }

        void HandleLearnButtonTouchUpInside(object sender, EventArgs e)
        {
            iOSUtilities.OpenUrl("http://yourEbriefingSharePointInstall/licensing");
        }

        void HandleCancelButtonTouchUpInside(object sender, EventArgs e)
        {
            Dismiss(true);
        }

        void HandleCancelEvent()
        {
            cts.Cancel();

            LoadingView.Hide();
        }
    }
}

