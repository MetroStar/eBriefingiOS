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
using System.Threading.Tasks;
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreGraphics;
using MessageUI;
using ObjCRuntime;
using Metrostar.Mobile.Framework;
using MssFramework;
using MaryPopinBinding;

namespace eBriefingMobile
{
    public partial class eBriefingViewController : UIViewController
    {
        private String searchString;
        private UITabBarController tabBarController;
        private MyBooksViewController vc1;
        private FavoriteViewController vc2;
        private UpdateViewController vc3;
        private LibraryViewController vc4;
        private PopoverSearchController psc;
        private UIPopoverController settingViewController;
        private UIPopoverController searchViewController;
		private UIPopoverController contentSyncViewController;
        private UIBarButtonItem settingButton;
		private UIBarButtonItem contentSyncErrorButton;
        private UIBarButtonItem searchButton;
		private EmailComposer mailer;
		private UIButton contentSyncButton;

        public eBriefingViewController()
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

            InitializeControls();

//            DeleteMyStuff();
        }

        async private void DeleteMyStuff()
        {
            await eBriefingService.Run(() => CloudSync.DeleteMyStuff());
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Open introduction if this is the first time
            if (String.IsNullOrEmpty(URL.ServerURL))
            {
                ShowTutorial(false, true, false);
            }
            else if (!Settings.TutorialClosed)
            {
                ShowTutorial(false, false, false);
            }
            else
            {
                // Show authentication view before anything
                if (!Settings.Authenticated)
                {
                    AuthenticationController avc = new AuthenticationController();
                    avc.DismissedEvent += HandleAuthenticatedEvent;

                    CustomNavigationController navController = new CustomNavigationController();
                    navController.SetViewControllers(new UIViewController[] { avc }, false);
                    navController.NavigationBar.BarStyle = UIBarStyle.Default;
                    this.NavigationController.PresentViewController(navController, false, null);
                }
                else
                {
                    this.NavigationController.NavigationBar.SetBackgroundImage(UIImage.FromBundle("Assets/Backgrounds/home_nav.png").CreateResizableImage(new UIEdgeInsets(0, 120, 0, 1)), UIBarMetrics.Default);
                    this.NavigationController.NavigationBarHidden = false;
                    tabBarController.View.Hidden = false;

                    // Update My Books badge
                    CalculateMyBooksBadge();
                }
            }
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);

            HidePopovers();

            // This updates the first tab(My Books) location for downloading animation
            UpdateTabLocation();
        }

        private void InitializeControls()
        {
            this.NavigationItem.BackBarButtonItem = new UIBarButtonItem(String.Empty, UIBarButtonItemStyle.Plain, null);
			this.NavigationController.SetNavigationBarHidden (true, false); 
			this.NavigationController.NavigationBar.TintColor = UIColor.White;

			//contentSyncButton
			contentSyncButton= new UIButton(UIButtonType.Custom);
			contentSyncButton.SetImage (UIImage.FromBundle ("Assets/Buttons/icn_contentSyncError.png"), UIControlState.Normal);
			contentSyncButton.TouchUpInside += HandleContentSyncErrorButtonTouchUpInside;
			contentSyncButton.Frame = new CGRect (0, 0, contentSyncButton.CurrentImage.Size.Width, contentSyncButton.CurrentImage.Size.Height);

            // segView
            SegmentedView segView = new SegmentedView();
            segView.ValueChanged += HandleSortEvent;
            this.NavigationItem.TitleView = segView;

            // settingButton
			if (URL.ServerURL!=null && !URL.ServerURL.Contains (StringRef.DemoURL) )
			{
				this.NavigationItem.SetRightBarButtonItems (new UIBarButtonItem[] {
					settingButton = new UIBarButtonItem (UIImage.FromBundle ("Assets/Buttons/setting.png"), UIBarButtonItemStyle.Plain, HandleSettingButtonTouchUpInside),
					searchButton = new UIBarButtonItem (UIBarButtonSystemItem.Search, HandleSearchButtonTouchUpInside),
					contentSyncErrorButton = new UIBarButtonItem (contentSyncButton)
				}, true);
			}
			else
			{
				this.NavigationItem.SetRightBarButtonItems (new UIBarButtonItem[] {
					settingButton = new UIBarButtonItem (UIImage.FromBundle ("Assets/Buttons/setting.png"), UIBarButtonItemStyle.Plain, HandleSettingButtonTouchUpInside),
					searchButton = new UIBarButtonItem (UIBarButtonSystemItem.Search, HandleSearchButtonTouchUpInside),
				}, true);
			}

            // Initialize tabbar
            InitializeTabBarController();

            BookUpdater.UpdateNeededEvent += HandleUpdateNeededEvent;
        }

        private void ShowTutorial(bool fromSettingsMenu, bool showIntro, bool animated)
        {
            TutorialViewController tvc = new TutorialViewController(showIntro);
            tvc.HideEvent += () =>
            {
                this.NavigationController.DismissViewController(true, delegate
                {
                    tvc.Dispose();
                    tvc = null;

                    if (!fromSettingsMenu)
                    {
                        HandleSelectServerDismissEvent(false);
                    }
                });
            };

            CustomNavigationController navController = new CustomNavigationController();
            navController.SetViewControllers(new UIViewController[] { tvc }, false);
            navController.NavigationBar.BarStyle = UIBarStyle.Default;
            this.NavigationController.PresentViewControllerAsync(navController, animated);
        }

        private void UpdateTabLocation()
        {
            // Calculate tabbar location which will be used on shrink animation when downloading
            foreach (UIView view in tabBarController.TabBar.Subviews)
            {
                if (view.GetType() == typeof(UIControl))
                {
                    CGRect rect0 = view.Frame;
                    CGRect rect1 = this.View.ConvertRectFromView(view.Frame, view);
                    Settings.MyBooksTabLocation = new CGPoint(rect0.X, rect1.Y);
                    break;
                }
            }
        }

        private void InitializeTabBarController()
        {
            // My Books
            vc1 = new MyBooksViewController();
            vc1.Title = StringRef.myBooks;
            vc1.RefreshEvent += HandleRefreshEvent1;
            vc1.UpdateUpdatesBadgeEvent += HandleUpdateUpdatesBadgeEvent;
            vc1.UpdateMyBooksBadgeEvent += HandleUpdateMyBooksBadgeEvent;

            // Favorite
            vc2 = new FavoriteViewController();
            vc2.Title = StringRef.favorites;
            vc2.RefreshEvent += HandleRefreshEvent2;
            vc2.UpdateUpdatesBadgeEvent += HandleUpdateUpdatesBadgeEvent;

            // Updates
            vc3 = new UpdateViewController();
            vc3.Title = StringRef.updates;
            vc3.RefreshEvent += HandleRefreshEvent3;
            vc3.ClearUpdatesBadgeEvent += HandleClearUpdatesBadgeEvent;
            vc3.UpdateUpdatesBadgeEvent += HandleUpdateUpdatesBadgeEvent;
            vc3.UpdateMyBooksBadgeEvent += HandleUpdateMyBooksBadgeEvent;

            // Available
            vc4 = new LibraryViewController();
            vc4.Title = StringRef.available;
            vc4.RefreshEvent += HandleRefreshEvent4;
            vc4.OpenBookshelfEvent += HandleOpenBookshelfEvent;
            vc4.UpdateUpdatesBadgeEvent += HandleUpdateUpdatesBadgeEvent;
            vc4.UpdateMyBooksBadgeEvent += HandleUpdateMyBooksBadgeEvent;
            vc4.UpdateAvailableBadgeEvent += HandleUpdateAvailableBadgeEvent;
            vc4.UpdateTabLocationEvent += HandleUpdateTabLocationEvent;

            // tabBarController
            tabBarController = new UITabBarController();
            tabBarController.TabBar.Translucent = false;
            tabBarController.ViewControllerSelected += HandleViewControllerSelected;
            tabBarController.SetViewControllers(new UIViewController[] {
                vc1,
                vc2,
                vc3,
                vc4
            }, false);

            vc1.TabBarItem.Title = StringRef.myBooks;
            vc1.TabBarItem.Image = UIImage.FromBundle("Assets/Icons/tab0.png");
            vc1.TabBarItem.Tag = 0;

            vc2.TabBarItem.Title = StringRef.favorites;
            vc2.TabBarItem.Image = UIImage.FromBundle("Assets/Icons/tab1.png");
            vc2.TabBarItem.Tag = 1;

            vc3.TabBarItem.Title = StringRef.updates;
            vc3.TabBarItem.Image = UIImage.FromBundle("Assets/Icons/tab2.png");
            vc3.TabBarItem.Tag = 2;

            vc4.TabBarItem.Title = StringRef.available;
            vc4.TabBarItem.Image = UIImage.FromBundle("Assets/Icons/tab3.png");
            vc4.TabBarItem.Tag = 3;

            tabBarController.View.Hidden = true;
            this.View.AddSubview(tabBarController.View);

            List<Book> dBooks = BooksOnDeviceAccessor.GetBooks();
            if (dBooks != null)
            {
                tabBarController.SelectedIndex = 0;
            }
            else
            {
                tabBarController.SelectedIndex = 3;
            }
        }

        async private Task OpenSyncView(bool bfromSyncButton = false)
		{
			LoadingView.Show ("Syncing", "Please wait while" + '\n' + "eBriefing is syncing." + '\n' + "This may take a few minutes...", false);

			// Start Push and Pull
			if ( !CloudSync.SyncingInProgress )
			{
				CloudSync.SyncingInProgress = true;
				await eBriefingService.Run (() => CloudSync.PushAndPull ());
				CloudSync.SyncingInProgress = false;
			}

			LoadingView.Hide ();

			// Once syncing is finished, check books to download
			BookUpdater.CheckBooks2Download ();

			// Refresh
			HandleRefreshEvent1 (this, EventArgs.Empty);

			if ( tabBarController.SelectedIndex == 1 )
			{
				HandleRefreshEvent2 (this, EventArgs.Empty);
			}
			else if ( tabBarController.SelectedIndex == 2 )
			{
				HandleRefreshEvent3 (this, EventArgs.Empty);
			}
			else if ( tabBarController.SelectedIndex == 3 )
			{
				vc4.LoadBooks ();
			}

			if ( !bfromSyncButton )
			{
				// Start the PUSH timer in the background
				SyncPushTimer.Start ();
			}
		}

        private void UpdateBadge(int index, int number)
        {
            if (index == 0)
            {
                if (number == 0)
                {
                    vc1.TabBarItem.BadgeValue = null;
                }
                else
                {
                    vc1.TabBarItem.BadgeValue = number.ToString();
                }
            }
            else if (index == 2)
            {
                if (number == 0)
                {
                    vc3.TabBarItem.BadgeValue = null;
                }
                else
                {
                    vc3.TabBarItem.BadgeValue = number.ToString();
                }

                // If current tab is already Updates Tab, refresh and show books
                if (tabBarController.SelectedIndex == 2)
                {
                    vc3.RetrieveBooks();
                }
            }
            else if (index == 3)
            {
                if (number == 0)
                {
                    BooksOnServerAccessor.UpdateViewed();
                    vc4.TabBarItem.BadgeValue = null;
                }
                else
                {
                    vc4.TabBarItem.BadgeValue = number.ToString();
                }
            }
        }

        void CalculateMyBooksBadge()
        {
            try
            {
                int count = 0;
                List<Book> dBooks = BooksOnDeviceAccessor.GetBooks();
                if (dBooks != null)
                {
					count = dBooks.Where(d => d.New && (!String.IsNullOrEmpty (d.Title)&& d.PageCount!=0&& d.ChapterCount!=0)).ToList().Count;
                }

                UpdateBadge(0, count);
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("eBriefingViewController - CalculateMyBooksBadge: {0}", ex.ToString());
            }
        }

        void HandleUpdateAvailableBadgeEvent(object sender, EventArgs e)
        {
            try
            {
                List<Book> sBooks = BooksOnServerAccessor.GetBooks();
                List<Book> dBooks = BooksOnDeviceAccessor.GetBooks();

                int count = 0;
                if (sBooks != null && dBooks != null)
                {
                    HashSet<String> dIDs = new HashSet<String>(dBooks.Select(d => d.ID));
                    var results = sBooks.Where(s => !dIDs.Contains(s.ID)).Where(s => !s.Viewed).ToList();
                    if (results != null)
                    {
                        count = results.Count;
                    }
                }
                else if (sBooks != null)
                {
                    count = sBooks.Where(s => !s.Viewed).ToList().Count;
                }

                UpdateBadge(3, count);
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("eBriefingViewController - HandleUpdateAvailableBadgeEvent: {0}", ex.ToString());
            }
        }

        private void HidePopovers()
        {
            if (settingViewController != null)
            {
                settingViewController.Dismiss(true);
            }

            if (searchViewController != null)
            {
                searchViewController.Dismiss(true);
            }

			if (contentSyncViewController != null)
			{
				contentSyncViewController.Dismiss(true);
			}
        }

        private void OpenSelectServer()
        {
            SelectServerViewController ssvc = new SelectServerViewController(true);
            ssvc.DismissEvent += HandleSelectServerDismissEvent;
            ssvc.CancelDownloadEvent += HandleCancelDownloadEvent;

            CustomNavigationController navController = new CustomNavigationController();
            navController.SetViewControllers(new UIViewController[] { ssvc }, true);
            AppDelegate.Current.Nav.PresentViewController(navController, true, null);
        }

        async void HandleAuthenticatedEvent()
        {
			// set again
			if (URL.ServerURL!=null && !URL.ServerURL.Contains (StringRef.DemoURL) )
			{
				this.NavigationItem.SetRightBarButtonItems (new UIBarButtonItem[] {
					settingButton = new UIBarButtonItem (UIImage.FromBundle ("Assets/Buttons/setting.png"), UIBarButtonItemStyle.Plain, HandleSettingButtonTouchUpInside),
					searchButton = new UIBarButtonItem (UIBarButtonSystemItem.Search, HandleSearchButtonTouchUpInside),
					contentSyncErrorButton = new UIBarButtonItem (contentSyncButton)
				}, true);
			}
			else
			{
				this.NavigationItem.SetRightBarButtonItems (new UIBarButtonItem[] {
					settingButton = new UIBarButtonItem (UIImage.FromBundle ("Assets/Buttons/setting.png"), UIBarButtonItemStyle.Plain, HandleSettingButtonTouchUpInside),
					searchButton = new UIBarButtonItem (UIBarButtonSystemItem.Search, HandleSearchButtonTouchUpInside),
				}, true);
			}

            // Check books to download
            BookUpdater.CheckBooks2Download();

            // Start the check for update timer
            BookUpdateTimer.Start();

            // Start syncing if necessary
			if (Settings.SyncOn)
            {
                await OpenSyncView();
            }
        }

        void HandleRefreshEvent1(object sender, EventArgs e)
        {
            CalculateMyBooksBadge();

            vc1.RetrieveBooks();
        }

        void HandleRefreshEvent2(object sender, EventArgs e)
        {
            vc2.RetrieveBooks();
        }

        void HandleRefreshEvent3(object sender, EventArgs e)
        {
            vc3.RetrieveBooks();
        }

        void HandleRefreshEvent4(object sender, EventArgs e)
        {
            vc4.RetrieveBooks();
        }

        void HandleOpenBookshelfEvent()
        {
            tabBarController.SelectedIndex = 0;
        }

        void HandleViewControllerSelected(object sender, UITabBarSelectionEventArgs e)
        {
            UpdateBadge(3, 0);
        }

        void HandleUpdateMyBooksBadgeEvent(object sender, EventArgs e)
        {
            CalculateMyBooksBadge();
        }

        void HandleClearUpdatesBadgeEvent(object sender, EventArgs e)
        {
            vc3.TabBarItem.BadgeValue = null;
        }

        void HandleUpdateUpdatesBadgeEvent(object sender, EventArgs e)
        {
            if (BookUpdater.Books2Update != null)
            {
                UpdateBadge(2, BookUpdater.Books2Update.Count);
            }
            else
            {
                UpdateBadge(2, 0);
            }
        }

        void HandleSortEvent()
        {
            if (tabBarController.SelectedIndex == 0)
            {
                vc1.Sort();
            }
            else if (tabBarController.SelectedIndex == 1)
            {
                vc2.Sort();
            }
            else if (tabBarController.SelectedIndex == 2)
            {
                vc3.Sort();
            }
            else
            {
                vc4.Sort();
            }
        }

        void HandleSelectServerDismissEvent(bool cancelled)
        {
            if (!cancelled)
            {
                UpdateBadge(0, 0);

                HandleAuthenticatedEvent();

                HandleRefreshEvent1(this, EventArgs.Empty);
            }
        }

        void HandleUpdateNeededEvent(int count)
        {
            if (count > 0)
            {
                UpdateBadge(2, count);
            }
        }

        void HandleCancelDownloadEvent()
        {
            vc1.CancelAllDownloads();

            tabBarController.SelectedIndex = 3;
        }

		void HandleContentSyncErrorButtonTouchUpInside(object sender, EventArgs args)
		{
			HidePopovers();

			//popoverContentSyncViewController
			var popover= new PopoverContentSyncViewController();
			popover.SyncButtonTouch += HandleSyncButtonTouchEvent;
			popover.View.Frame = new CGRect (0, 0, 300, 190);

			// settingViewController
			contentSyncViewController = new UIPopoverController(popover);
			contentSyncViewController.DidDismiss += delegate
			{
				contentSyncViewController.Dispose();
				contentSyncViewController = null;
			};

			contentSyncViewController.SetPopoverContentSize(new CGSize(popover.View.Frame.Width, popover.View.Frame.Height), true);
			contentSyncViewController.PresentFromBarButtonItem(contentSyncErrorButton, UIPopoverArrowDirection.Any, true);
		}

		async void HandleSyncButtonTouchEvent()
		{
			HidePopovers();

			if ( Reachability.IsDefaultNetworkAvailable () )
			{
				await OpenSyncView (true);
			}
			else
			{
				AlertView.Show(StringRef.connectionFailure, StringRef.connectionRequired, StringRef.ok);
			}
		}

        void HandleSettingButtonTouchUpInside(object sender, EventArgs args)
        {
            HidePopovers();

            // psc
            PopoverSettingController psc = new PopoverSettingController(UITableViewStyle.Grouped);
            psc.View.Frame = new CGRect(0, 0, 300, 444);
            psc.RowSelectedEvent += HandleSettingRowSelectedEvent;
            psc.SyncOnEvent += HandleSyncOnEvent;
			psc.SyncOffEvent += HandleSyncOffEvent;

            // settingViewController
            settingViewController = new UIPopoverController(psc);
            settingViewController.DidDismiss += delegate
            {
                settingViewController.Dispose();
                settingViewController = null;
            };
            settingViewController.SetPopoverContentSize(new CGSize(psc.View.Frame.Width, psc.View.Frame.Height), true);
            settingViewController.PresentFromBarButtonItem(settingButton, UIPopoverArrowDirection.Any, true);
        }

        void HandleSearchButtonTouchUpInside(object sender, EventArgs args)
        {
            HidePopovers();

            // psc
            if (psc == null)
            {
                psc = new PopoverSearchController(searchString);
                psc.RowSelectedEvent += HandleSearchResultSelectedEvent;
                psc.ResizePopoverEvent += (nfloat height) =>
                {
                    // Do not cover the TabBar
                    nfloat viewHeight = this.View.Frame.Height - 55;
                    if (height > viewHeight)
                    {
                        height = viewHeight - 20;
                    }

                    psc.PreferredContentSize = new CGSize(psc.View.Frame.Width, height);
                };
            }

            // searchViewController
            searchViewController = new UIPopoverController(psc);
            searchViewController.DidDismiss += delegate
            {
                searchString = psc.SearchDisplayController.SearchBar.Text;

                searchViewController.Dispose();
                searchViewController = null;
            };
            searchViewController.SetPopoverContentSize(new CGSize(psc.View.Frame.Width, psc.View.Frame.Height), true);
            searchViewController.PresentFromBarButtonItem(searchButton, UIPopoverArrowDirection.Any, true);
        }

        void HandleSearchResultSelectedEvent(Book book, bool availableInMyBooks)
        {
            // Hide searchViewController
            if (searchViewController != null)
            {
                searchViewController.Dismiss(true);
            }

            if (availableInMyBooks)
            {
                // Switch to My Books in case the user is in Updates or Available
                if (tabBarController.SelectedIndex == 2 || tabBarController.SelectedIndex == 3)
                {
                    tabBarController.SelectedIndex = 0;
                }

                // Open Dashboard
                if (book.Status == Book.BookStatus.DOWNLOADED)
                {
                    AppDelegate.Current.Nav.PushViewController(new DashboardViewController(book), true);
                }
            }
            else
            {
                // Switch to Available in case the user is My Books or Favorites
                if (tabBarController.SelectedIndex == 0 || tabBarController.SelectedIndex == 1)
                {
                    tabBarController.SelectedIndex = 3;
                }

                // Open Overview
                vc4.OpenOverview(book);
            }
        }

        void HandleSettingRowSelectedEvent(NSIndexPath indexPath)
        {
            if (indexPath.Section == 0)
            {
                if (indexPath.Row == 0)
                {
                    HidePopovers();

                    settingViewController.Dispose();
                    settingViewController = null;

                    // Server Setting
                    OpenSelectServer();
                }
            }
            else
            {
                HidePopovers();

                if (indexPath.Row == 0)
                {
                    // About eBriefing
                    CustomNavigationController navController = new CustomNavigationController();
					navController.NavigationBar.TintColor = UIColor.White;
                    navController.SetViewControllers(new UIViewController[] { new AboutViewController() }, false);
                    AppDelegate.Current.Nav.PresentViewController(navController, true, null);
                }
                else if (indexPath.Row == 1)
                {
                    // Tutorial
                    ShowTutorial(true, false, true);
                }
//                else if (indexPath.Row == 2)
//                {
//                    // Privacy Policy
//                    PrivacyPolicyViewController ppvc = new PrivacyPolicyViewController();
//                    ppvc.DismissEvent += delegate
//                    {
//                        this.DismissCurrentPopinControllerAnimated(true);
//                    };
//                    ppvc.View.Frame = new CGRect(0, 0, 646, 600);
//                    ppvc.SetPopinTransitionStyle(BKTPopinTransitionStyle.SpringySlide);
//                    ppvc.SetPopinOptions(BKTPopinOption.Default);
//                    ppvc.SetPopinTransitionDirection(BKTPopinTransitionDirection.Top);
//                    this.PresentPopinController(ppvc, true, null);
//                }
                else if (indexPath.Row == 2)
                {
					if ( MFMailComposeViewController.CanSendMail )
					{
						// Give Feedback
						mailer = new EmailComposer();
						mailer.Recipient="eBriefing@metrostarsystems.com";
						mailer.Subject="eBriefing Feedback (iOS)";
						mailer.Body="eBriefing App Version: " + Settings.AppVersion + "\niOS Version: " + Constants.Version
							+ "\niOS Device: " + UIDevice.CurrentDevice.Model + "\n\nDescription of Problem, Concern, or Question:";

						mailer.PresentViewController(this);
					}
					else
					{
						new UIAlertView ("No mail account", "Please set up a Mail account in order to send a mail.", null,"Ok", null).Show();
					}               
                }
                else
                {
                    // Rate This App
                    MTiRate.iRate.SharedInstance.PromptForRating();
                }
            }
        }

        async void HandleSyncOnEvent()
        {
            HidePopovers();

			if ( Reachability.IsDefaultNetworkAvailable () )
			{
				await OpenSyncView ();
			}
			else
			{
				AlertView.Show(StringRef.connectionFailure, StringRef.connectionRequired, StringRef.ok);
			}

        }

		void HandleSyncOffEvent()
		{
			HidePopovers();
		}

        void HandleUpdateTabLocationEvent(object sender, EventArgs e)
        {
            UpdateTabLocation();
        }
    }
}

