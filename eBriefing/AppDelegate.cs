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
using System.Linq;
using CoreGraphics;
using System.Collections.Generic;
using Foundation;
using UIKit;
using Metrostar.Mobile.Framework;
using MTiRate;
using PSPDFKit;

namespace eBriefingMobile
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		protected CustomNavigationController nav;
		private static AppDelegate current;
		private DashboardMenuView floatMenu;

		public static AppDelegate Current
		{
			get
			{
				return current;
			}
		}

		public CustomNavigationController Nav
		{
			get
			{
				return nav;
			}
		}

		protected AppDelegate()
		{
			nav = new CustomNavigationController();
		}

		static AppDelegate()
		{
			current = new AppDelegate();
		}
		//
		// This method is invoked when the application has loaded and is ready to run. In this
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			window = new UIWindow();
			window.BackgroundColor = eBriefingAppearance.BlueColor;

			// IMPORTANT: Read settings up front or the app will crash
			Settings.Read();
			eBriefingAppearance.SetAppearances();

			Logger.DebugLoggingOn = false;

			// Activates PSPDFKit for com.metrostarsystems.ebriefing.ipad
			PSPDFKit.PSPDFLicenseManager.SetLicenseKey("{PDFKit License Key");

			// Register iRate
			RegisteriRate();

			// eBriefingViewController
			AppDelegate.Current.Nav.SetViewControllers(new UIViewController[] { new eBriefingViewController() }, true);

			// Show
			window.RootViewController = AppDelegate.Current.Nav;
			window.MakeKeyAndVisible();
			window.Frame = UIScreen.MainScreen.Bounds;

			return true;
		}

		private void RegisteriRate()
		{
			var rateAlert = iRate.SharedInstance;
			rateAlert.AppStoreID = 639062834;
			rateAlert.OnlyPromptIfLatestVersion = false;
			rateAlert.RemindButtonLabel = String.Empty;
			rateAlert.MessageTitle = "Rate eBriefing";
			rateAlert.Message = "If you enjoy using eBriefing,would you mind taking a moment to rate it? It won't take more than a minute. Thanks for your support!";
			rateAlert.CancelButtonLabel = "No, Thanks";
			rateAlert.RateButtonLabel = "Rate It Now";
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow forWindow)
		{
			return UIInterfaceOrientationMask.All;
		}

		public DashboardMenuView AddDashboardFloatMenu()
		{
			UIWindow subWindow = UIApplication.SharedApplication.Windows[0];

			// floatMenu
			floatMenu = new DashboardMenuView();
			floatMenu.Frame = new CGRect(subWindow.Frame.Width - floatMenu.Frame.Width, subWindow.Frame.Bottom, floatMenu.Frame.Width, floatMenu.Frame.Height);
			floatMenu.Layer.ZPosition = 100;
			subWindow.AddSubview(floatMenu);

			return floatMenu;
		}

		public void RemoveDashboardFloatMenu(bool forceHide = false)
		{
			if (floatMenu != null)
			{
				floatMenu.RemoveFromSuperview();
				floatMenu.Dispose();
				floatMenu = null;
			}
		}
	}
}

