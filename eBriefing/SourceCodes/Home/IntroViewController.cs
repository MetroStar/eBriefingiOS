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

namespace eBriefingMobile
{
    public partial class IntroViewController : DispatcherViewController
    {
        private bool isFirst = true;
        private bool showVersion = false;
        private bool pageControlBeingUsed;
        private UIScrollView scrollView;
        private UIPageControl pageControl;
        private UIButton startButton;

        public IntroViewController(bool showVersion)
        {
            this.showVersion = showVersion;
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

            if (showVersion)
            {
                this.NavigationController.SetNavigationBarHidden(false, true);
            }
            else
            {
                this.NavigationController.SetNavigationBarHidden(true, true);
            }

            // Initialize controls
            if (isFirst)
            {
                InitializeControls();
            }
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            if (isFirst)
            {
                isFirst = false;

                for (int i = 0; i < pageControl.Pages; i++)
                {
                    IntroView introView = new IntroView(i);
                    introView.Frame = new CGRect(i * scrollView.Frame.Width, 0, scrollView.Frame.Width, scrollView.Frame.Height);
                    scrollView.AddSubview(introView);

                    if (showVersion)
                    {
                        if (i == pageControl.Pages - 1)
                        {
                            introView.AddVersion();
                        }
                    }
                }
            }
            else
            {
                int i = 0;
                foreach (UIView subview in scrollView)
                {
                    if (subview is IntroView)
                    {
                        IntroView introView = subview as IntroView;
                        introView.Frame = new CGRect(i * scrollView.Frame.Width, introView.Frame.Y, scrollView.Frame.Width, scrollView.Frame.Height);

                        i++;
                    }
                }

                scrollView.ContentOffset = new CGPoint(pageControl.CurrentPage * scrollView.Frame.Width, 0);
            }

            scrollView.ContentSize = new CGSize(scrollView.Frame.Width * pageControl.Pages, scrollView.Frame.Height);
        }

        private void InitializeControls()
        {
            this.View.BackgroundColor = eBriefingAppearance.BlueColor;
            this.NavigationItem.BackBarButtonItem = new UIBarButtonItem(String.Empty, UIBarButtonItemStyle.Plain, null);

            if (showVersion)
            {
                this.NavigationItem.Title = "About";
                this.NavigationController.NavigationBar.SetBackgroundImage(UIImage.FromBundle("Assets/Backgrounds/navbar.png").CreateResizableImage(new UIEdgeInsets(0, 1, 0, 1)), UIBarMetrics.Default);
                this.NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done, HandleDoneButtonTouchUpInside);
            }
            else
            {
                // Initialize credential
                KeychainAccessor.ClearCredential();
            }
                
            nfloat bottom = this.View.Frame.Bottom - 44;
            if (!showVersion)
            {
                // startButton
                startButton = UIButton.FromType(UIButtonType.Custom);
                startButton.Frame = new CGRect(this.View.Center.X - (207 / 2), bottom - 44, 207, 44);
                startButton.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;
                startButton.Font = eBriefingAppearance.ThemeBoldFont(21);
                startButton.SetTitle("Get Started", UIControlState.Normal);
                startButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                startButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/green_unfilled.png").CreateResizableImage(new UIEdgeInsets(15f, 14f, 15f, 14f)), UIControlState.Normal);
                startButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/white_unfilled.png").CreateResizableImage(new UIEdgeInsets(0f, 22f, 0f, 22f)), UIControlState.Highlighted);
                startButton.TouchUpInside += HandleStartButtonTouchUpInside;
                this.View.AddSubview(startButton);

                bottom = startButton.Frame.Y - 30;
            }

            // pageControl
            pageControl = new UIPageControl();
            pageControl.Frame = new CGRect(this.View.Center.X - (300 / 2), bottom - 36, 300, 36);
            pageControl.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;
            pageControl.ValueChanged += HandlePageControlValueChanged;
            pageControl.Pages = 4;
            this.View.AddSubview(pageControl);

            bottom = pageControl.Frame.Y - 20;

            // scrollView
            scrollView = new UIScrollView();
            scrollView.Frame = new CGRect(0, 20, this.View.Frame.Width, bottom - 20);
            scrollView.BackgroundColor = UIColor.Clear;
            scrollView.PagingEnabled = true;
            scrollView.ShowsHorizontalScrollIndicator = false;
            scrollView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
            scrollView.DraggingStarted += HandleScrollViewDraggingStarted;
            scrollView.DecelerationEnded += HandleScrollViewDecelerationEnded;
            this.View.AddSubview(scrollView);
        }

        private void ScrollToPage(nint pageNumber)
        {
            if (pageNumber < 0 || pageNumber >= pageControl.Pages)
            {
                return;
            }

            UIPageControl.AnimationsEnabled = true;
            UIPageControl.Animate(0.3d, delegate
            {
                scrollView.ContentOffset = new CGPoint(pageNumber * scrollView.Frame.Width, 0);
                pageControl.CurrentPage = pageNumber;
            });
        }

        void HandleScrollViewDecelerationEnded(object sender, EventArgs e)
        {
            if (pageControl.Pages > 0)
            {
                if (!pageControlBeingUsed)
                {
                    // Switch the indicator when more than 50% of the previous/next page is visible
                    double page = Math.Floor((scrollView.ContentOffset.X - (scrollView.Frame.Width / 2)) / scrollView.Frame.Width) + 1;
                    if (pageControl.CurrentPage != page)
                    {
                        pageControl.CurrentPage = (int)page;
                    }
                }
            }
            
            pageControlBeingUsed = false;
        }

        void HandleScrollViewDraggingStarted(object sender, EventArgs e)
        {
            pageControlBeingUsed = false;
        }

        void HandleStartButtonTouchUpInside(object sender, EventArgs e)
        {
            this.NavigationController.PushViewController(new SelectServerViewController(false), true);
        }

        void HandlePageControlValueChanged(object sender, EventArgs e)
        {
            // Scroll to next page
            ScrollToPage(pageControl.CurrentPage);

            // Keep track of when scrolls happen in response to the page control value changing.
            // If we don't do this, a noticeable "flashing" occurs as the the scroll delegate will temporarily switch back the page number.
            pageControlBeingUsed = true;
        }

        void HandleDoneButtonTouchUpInside(object sender, EventArgs e)
        {
            this.DismissViewController(true, null);
        }
    }
}

