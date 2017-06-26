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


namespace eBriefingMobile
{
	public partial class EnterpriseViewController : DispatcherViewController
	{
		private UILabel domainLabel;
		private UILabel passwordLabel;
		//private UILabel licenseLabel;
		private UISwitch formSwitch;
		//private UIButton learnButton;
		private UIButton connectButton;
		private UITextField urlField;
		private UITextField idField;
		private UITextField passwordField;
		private UITextField domainField;
		private UIScrollView scrollView;
		private KeyboardAutoScroll autoScroll;
		private CancellationTokenSource cts;

		public delegate void EnterpriseViewDelegate0();

		public delegate void EnterpriseViewDelegate1(bool cancelled);

		public event EnterpriseViewDelegate1 DismissEvent;
		public event EnterpriseViewDelegate0 CancelDownloadEvent;

		public EnterpriseViewController()
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
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			autoScroll.UnregisterKeyboardNotifications();
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			BookUpdater.RegisterASIDelegate(this);

			this.NavigationController.InteractivePopGestureRecognizer.Enabled = false;

			// Initialize controls
			InitializeControls();

			if (!String.Equals(URL.ServerURL, StringRef.DemoURL))
			{
				if (!String.IsNullOrEmpty(URL.ServerURL))
				{
					urlField.Text = URL.ServerURL;
				}

				if (!String.IsNullOrEmpty(Settings.UserID))
				{
					idField.Text = Settings.UserID;
				}

				String password = KeychainAccessor.Password;
				if (!String.IsNullOrEmpty(password))
				{
					passwordField.Text = password;
				}

				if (!String.IsNullOrEmpty(Settings.Domain))
				{
					domainField.Text = Settings.Domain;
				}
			}
		}

		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();

			if ( scrollView != null )
			{
				scrollView.ContentSize = new CGSize (this.View.Frame.Width, connectButton.Frame.Bottom + 50);
			}
		}

		private void InitializeControls()
		{
			this.View.BackgroundColor = UIColor.White;
			this.NavigationItem.Title = "Connect to Enterprise Library";

			// scrollView
			scrollView = new UIScrollView();
			scrollView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			scrollView.Frame = new CGRect(0, 0, this.View.Frame.Width, this.View.Frame.Height);
			this.View.AddSubview(scrollView);

			// licenseLabel
//			licenseLabel = eBriefingAppearance.GenerateLabel(17);
//			licenseLabel.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin;
//			licenseLabel.Text = "NEED ENTERPRISE LICENSE?";
//			licenseLabel.Frame = new CGRect(scrollView.Frame.Right - 280, 50, 240, 21);
//			scrollView.AddSubview(licenseLabel);

			// noticeLabel
			UILabel noticeLabel = eBriefingAppearance.GenerateLabel(21);
			noticeLabel.Frame = new CGRect(40, 50, 400, 42);
			noticeLabel.Lines = 2;
			noticeLabel.LineBreakMode = UILineBreakMode.WordWrap;
			noticeLabel.Text = "*Requires web front end for your SharePoint Environment to continue setup";
			noticeLabel.SizeToFit();
			scrollView.AddSubview(noticeLabel);

			// learnButton
//			learnButton = UIButton.FromType(UIButtonType.Custom);
//			learnButton.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin;
//			learnButton.Frame = new CGRect(licenseLabel.Frame.X, licenseLabel.Frame.Bottom + 20, licenseLabel.Frame.Width, 44);
//			learnButton.Font = eBriefingAppearance.ThemeBoldFont(21);
//			learnButton.SetTitle("Learn More", UIControlState.Normal);
//			learnButton.SetTitleColor(eBriefingAppearance.Gray1, UIControlState.Normal);
//			learnButton.SetTitleColor(UIColor.White, UIControlState.Highlighted);
//			learnButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/green_unfilled.png").CreateResizableImage(new UIEdgeInsets(15f, 14f, 15f, 14f)), UIControlState.Normal);
//			learnButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/green_filled.png").CreateResizableImage(new UIEdgeInsets(15f, 14f, 15f, 14f)), UIControlState.Highlighted);
//			learnButton.TouchUpInside += HandleLearnButtonTouchUpInside;
//			scrollView.AddSubview(learnButton);

			// formsLabel
			UILabel formsLabel = eBriefingAppearance.GenerateLabel(21);
			formsLabel.Text = "Use Forms Authentication";
			formsLabel.Frame = new CGRect(noticeLabel.Frame.X, noticeLabel.Frame.Bottom + 20, this.View.Frame.Width - 80, 21);
			scrollView.AddSubview(formsLabel);

			// formSwitch
			formSwitch = new UISwitch();
			formSwitch.Frame = new CGRect(300, formsLabel.Center.Y - (formSwitch.Frame.Height / 2), formSwitch.Frame.Width, formSwitch.Frame.Height);
			formSwitch.ValueChanged += HandleFormSwitchValueChanged;
			formSwitch.SetState(Settings.UseFormsAuth, true);
			scrollView.AddSubview(formSwitch);

			// urlLabel
			UILabel urlLabel = eBriefingAppearance.GenerateLabel(21);
			urlLabel.Text = "Library URL";
			urlLabel.Frame = new CGRect(noticeLabel.Frame.X, formSwitch.Frame.Bottom+40, 130, 21);
			scrollView.AddSubview(urlLabel);

			// urlField
			urlField = GenerateTextField("  Type library URL here", new CGPoint(178,  formSwitch.Frame.Bottom+30), 0);

			// idLabel
			UILabel idLabel = eBriefingAppearance.GenerateLabel(21);
			idLabel.Text = "User ID";
			idLabel.Frame = new CGRect(noticeLabel.Frame.X, urlLabel.Frame.Bottom+40, 130, 21);
			scrollView.AddSubview(idLabel);

			// idField
			idField = GenerateTextField("  Type user ID here", new CGPoint(178,  urlLabel.Frame.Bottom+30), 1);

			// passwordLabel
			passwordLabel = eBriefingAppearance.GenerateLabel(21);
			passwordLabel.Text = "Password";
			passwordLabel.Frame = new CGRect(noticeLabel.Frame.X, idLabel.Frame.Bottom+40, 130, 21);
			scrollView.AddSubview(passwordLabel);

			// passwordField
			passwordField = GenerateTextField("  Type password here", new CGPoint(178, idLabel.Frame.Bottom+30), 2);
			passwordField.SecureTextEntry = true;

			// domainLabel
			domainLabel = eBriefingAppearance.GenerateLabel(21);
			domainLabel.Text = "Domain";
			domainLabel.Frame = new CGRect(noticeLabel.Frame.X, passwordLabel.Frame.Bottom+40, 130, 21);
			scrollView.AddSubview(domainLabel);

			// domainField
			domainField = GenerateTextField("  Type domain here", new CGPoint(178, passwordLabel.Frame.Bottom+30), 3);
			domainField.ReturnKeyType = UIReturnKeyType.Go;

			// connectButton
			connectButton = UIButton.FromType(UIButtonType.Custom);
			connectButton.Frame = new CGRect(scrollView.Frame.Width - 247, domainField.Frame.Bottom + 60, 207, 44);
			connectButton.Font = eBriefingAppearance.ThemeBoldFont(21);
			connectButton.SetTitle("Connect", UIControlState.Normal);
			connectButton.SetTitleColor(eBriefingAppearance.Gray1, UIControlState.Normal);
			connectButton.SetTitleColor(UIColor.White, UIControlState.Highlighted);
			connectButton.SetTitleColor(UIColor.LightGray, UIControlState.Disabled);
			connectButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/green_unfilled.png").CreateResizableImage(new UIEdgeInsets(15f, 14f, 15f, 14f)), UIControlState.Normal);
			connectButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/gray_unfilled.png").CreateResizableImage(new UIEdgeInsets(0f, 22f, 0f, 22f)), UIControlState.Disabled);
			connectButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/green_filled.png").CreateResizableImage(new UIEdgeInsets(15f, 14f, 15f, 14f)), UIControlState.Highlighted);
			connectButton.TouchUpInside += HandleConnectButtonTouchUpInside;
			connectButton.Enabled = false;
			connectButton.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleBottomMargin;
			scrollView.AddSubview(connectButton);

			// Register tableView Keyboard Notification for auto scroll
			autoScroll = new KeyboardAutoScroll();
			autoScroll.RegisterForKeyboardNotifications(this.View, KeyboardAutoScroll.ScrollType.SCROLLVIEW);
		}

		private UITextField GenerateTextField(String placeholder, CGPoint position, int tag)
		{
			UITextField textField = eBriefingAppearance.GenerateTextField(placeholder);
			textField.Tag = tag;
			textField.Frame = new CGRect(position.X, position.Y, scrollView.Frame.Width - position.X - 40, 42);
			textField.ClearButtonMode = UITextFieldViewMode.WhileEditing;
			textField.AutocapitalizationType = UITextAutocapitalizationType.None;
			textField.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			textField.ShouldReturn = delegate
			{
				if (tag == 0)
				{
					idField.BecomeFirstResponder();
				}
				else if (tag == 1)
				{
					passwordField.BecomeFirstResponder();
				}
				else if (tag == 2)
				{
					if (formSwitch.On)
					{
						HandleConnectButtonTouchUpInside(this, EventArgs.Empty);
					}
					else
					{
						domainField.BecomeFirstResponder();
					}
				}
				else
				{
					HandleConnectButtonTouchUpInside(this, EventArgs.Empty);
				}
				return true;
			};
			textField.AllEditingEvents += HandleAllEditingEvents;
			scrollView.AddSubview(textField);

			return textField;
		}

		private void HandleAllEditingEvents(object sender, EventArgs e)
		{
			EnableConnectButton();
		}

		private void EnableConnectButton()
		{
			if (!String.IsNullOrEmpty(urlField.Text) && !String.IsNullOrEmpty(idField.Text) && !String.IsNullOrEmpty(passwordField.Text))
			{
				if (formSwitch.On)
				{
					connectButton.Enabled = true;
				}
				else if (!String.IsNullOrEmpty(domainField.Text))
				{
					connectButton.Enabled = true;
				}
				else
				{
					connectButton.Enabled = false;
				}
			}
			else
			{
				connectButton.Enabled = false;
			}
		}

		private void StartAuthenticate(bool formSwitchOn, out WebExceptionStatus errorStatus, out bool notCompatible, out String formsError)
		{
			String url = String.Empty;
			String id = String.Empty;
			String password = String.Empty;
			String domain = String.Empty;

			this.InvokeOnMainThread(delegate
			{
				url = urlField.Text;
				id = idField.Text;
				password = passwordField.Text;
				domain = domainField.Text;
			});

			notCompatible = false;
			formsError = String.Empty;
			errorStatus = WebExceptionStatus.ConnectFailure;
			var status1 = WebExceptionStatus.ConnectFailure;

			try
			{
				// Check 2013 server info first, if xml returned is null, then use 2010 server
				var status=Server.CheckCompatibility2013(url, id, password, domain);

				if(status != WebExceptionStatus.Success)
				{
					 status1= Server.CheckCompatibility2010(url, id, password, domain);
				}

 				if (status== WebExceptionStatus.Success || status1== WebExceptionStatus.Success)
				{
					if (formSwitchOn)
					{
						formsError = Authenticate.Forms_Authenticate(url, id, password);
					}
					else
					{
						errorStatus = Authenticate.NTLM_Authenticate(url, id, password, domain);
					}
				}
				else
				{
					if(status== WebExceptionStatus.ConnectFailure)
					{
						errorStatus=status1;
					}
					else 
					{
						errorStatus=status;
					}
				}
			}
			catch (WebException ex)
			{
				errorStatus = ex.Status;
			}
		}

		async private void Connect()
		{
			this.View.EndEditing(true);

			if (!Reachability.IsDefaultNetworkAvailable())
			{
				AlertView.Show(StringRef.connectionFailure, StringRef.connectionRequired, StringRef.ok);
			}
			else
			{
				try
				{
					bool notCompatible = false;
					bool formSwitchOn = formSwitch.On;
					String formsError = String.Empty;
					WebExceptionStatus errorStatus = WebExceptionStatus.ConnectFailure;
					cts = new CancellationTokenSource();

					LoadingView.Show("Connecting", "Please wait while we're connecting to Enterprise Library...");
					await eBriefingService.Run(() => StartAuthenticate(formSwitchOn, out errorStatus, out notCompatible, out formsError), cts.Token);
					LoadingView.Hide();
					cts.Token.ThrowIfCancellationRequested();

					if (errorStatus == WebExceptionStatus.Success || (formSwitchOn && String.IsNullOrEmpty(formsError)))
					{
						Settings.WriteUseFormsAuth(formSwitchOn);
						Settings.Authenticated = true;
						Settings.AvailableCheckTime = DateTime.MinValue;
						Settings.WriteSyncOn(true);

						CancelAllDownloads();

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
							if (formSwitchOn)
							{
								AlertView.Show(StringRef.alert, formsError, StringRef.ok);
							}
							else
							{
								WebExceptionAlertView.ShowAlert(errorStatus);
							}
						}
					}
				}
				catch (OperationCanceledException)
				{
					KeychainAccessor.ClearCredential();
				}
			}
		}

		private void Dismiss(bool cancelled)
		{
			LoadingView.Hide();

			if (DismissEvent != null)
			{
				DismissEvent(cancelled);
			}
		}

		private void CancelAllDownloads()
		{
			if (CancelDownloadEvent != null)
			{
				CancelDownloadEvent();
			}
		}

		void HandleConnectButtonTouchUpInside(object sender, EventArgs e)
		{
			if (!String.Equals(URL.ServerURL, urlField.Text) || !String.Equals(Settings.UserID, idField.Text) || Settings.UseFormsAuth != formSwitch.On || !String.Equals(Settings.Domain, domainField.Text)|| !String.IsNullOrEmpty(passwordField.Text))
			{
				Connect();
			}
			else
			{
				Dismiss(true);
			}
		}

		void HandleLearnButtonTouchUpInside(object sender, EventArgs e)
		{
			iOSUtilities.OpenUrl("http://yourEbriefingSharePointInstall/licensing");
		}

		void HandleFormSwitchValueChanged(object sender, EventArgs e)
		{
			// If forms auth is selected, change return type to Go
			if (formSwitch.On)
			{
				UIView.Animate(0.3d, delegate
				{
					domainLabel.Alpha = domainField.Alpha = 0f;

					connectButton.Frame = new CGRect(connectButton.Frame.X, passwordField.Frame.Bottom + 60, connectButton.Frame.Width, connectButton.Frame.Height);
				}, delegate
				{
					domainField.Text = String.Empty;
					passwordField.ReturnKeyType = UIReturnKeyType.Go;
				});
			}
			else
			{
				UIView.Animate(0.3d, delegate
				{
					domainLabel.Alpha = domainField.Alpha = 1f;

					connectButton.Frame = new CGRect(connectButton.Frame.X, domainField.Frame.Bottom + 60, connectButton.Frame.Width, connectButton.Frame.Height);
				}, delegate
				{
					passwordField.ReturnKeyType = UIReturnKeyType.Next;
				});
			}

			EnableConnectButton();
		}

		void HandleCancelEvent()
		{
			cts.Cancel();

			LoadingView.Hide();
		}
	}
}

