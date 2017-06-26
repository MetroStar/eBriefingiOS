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
using System.IO;
using Foundation;
using UIKit;
using CoreGraphics;
using MssFramework;
using MaryPopinBinding;

namespace eBriefingMobile
{
	public partial class PrivacyPolicyViewController : DispatcherViewController
	{
		private UIScrollView scrollView;
		private UINavigationBar navBar;
		private UIWebView webView;
		private static nfloat FRAME_WIDTH = 646;
		private static nfloat FRAME_HEIGHT = 600;

		public delegate void PrivacyPolicyDelegate();

		public event PrivacyPolicyDelegate DismissEvent;

		public PrivacyPolicyViewController()
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

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			BookUpdater.RegisterASIDelegate(this);
		}

		public override void WillAnimateRotation(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillAnimateRotation(toInterfaceOrientation, duration);

			UpdateContentSize();
		}

		private void InitializeControls()
		{
			this.View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle("Assets/Backgrounds/background_portrait.png"));
			this.View.Frame = new CGRect(0, 0, FRAME_WIDTH, FRAME_HEIGHT);

			// navBar
			UIImage navImage = UIImage.FromBundle("Assets/Backgrounds/topBar.png").CreateResizableImage(new UIEdgeInsets(0, 22, 0, 22));
			navBar = new UINavigationBar(new CGRect(0, 0, this.View.Frame.Width, 44));
			navBar.TitleTextAttributes = new UIStringAttributes() { ForegroundColor = UIColor.White };
			navBar.TintColor = UIColor.White;
			navBar.SetBackgroundImage(navImage, UIBarMetrics.Default);
			this.View.AddSubview(navBar);

			// doneButton
			UINavigationItem item = new UINavigationItem();
			item.Title = "Privacy Policy";
			item.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done, HandleCloseButtonTouchUpInside);
			navBar.PushNavigationItem(item, false);

			// scrollView
			scrollView = new UIScrollView(new CGRect(0, navBar.Frame.Bottom, this.View.Frame.Width, this.View.Frame.Height - 44));
			scrollView.BackgroundColor = UIColor.Clear;
			this.View.AddSubview(scrollView);

			// webView
			webView = new UIWebView(new CGRect(15, 15, scrollView.Frame.Width - 30, scrollView.Frame.Height - 30));
			webView.Alpha = 0f;
			webView.Opaque = false;

			CustomWebViewDelegate customDelegate = new CustomWebViewDelegate();
			customDelegate.LoadFinishedEvent += HandleLoadFinishedEvent;
			webView.Delegate = customDelegate;
			webView.BackgroundColor = UIColor.Clear;

			foreach (var sv in webView.Subviews)
			{
				if (sv is UIScrollView)
				{
					((UIScrollView)sv).Bounces = false;
				}
			}
			scrollView.AddSubview(webView);
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

			// Read privacy policy from file
			String localHtmlUrl = Path.Combine(NSBundle.MainBundle.BundlePath, "HTML/privacy_policy.html");
			webView.LoadRequest(new NSUrlRequest(new NSUrl(localHtmlUrl, false)));
		}

		private void UpdateContentSize()
		{
			// webView
			webView.ScrollView.ScrollEnabled = false;
			webView.Frame = new CGRect(webView.Frame.X, webView.Frame.Y, webView.Frame.Width, webView.ScrollView.ContentSize.Height);

			// scrollView
			scrollView.ContentSize = new CGSize(this.View.Frame.Width, webView.Frame.Bottom);
		}

		void HandleLoadFinishedEvent()
		{
			UpdateContentSize();

			UIView.Animate(0.3d, delegate
			{
				webView.Alpha = 1.0f;
			});
		}

		void HandleCloseButtonTouchUpInside(object sender, EventArgs e)
		{
			if (DismissEvent != null)
			{
				DismissEvent();
			}
		}
	}

	public class CustomWebViewDelegate : UIWebViewDelegate
	{
		public delegate void LoadFinishedDelegate();

		public event LoadFinishedDelegate LoadFinishedEvent;

		public override void LoadingFinished(UIWebView webView)
		{
			if (LoadFinishedEvent != null)
			{
				LoadFinishedEvent();
			}
		}

		public override bool ShouldStartLoad(UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
		{
			if (navigationType == UIWebViewNavigationType.LinkClicked)
			{
				iOSUtilities.OpenUrl(request.Url.AbsoluteString);
				return false;
			}
			else
			{
				return true;
			}
		}
	}
}

