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

namespace eBriefingMobile
{
    public partial class TutorialViewController : DispatcherViewController
    {
        private bool isFirst = true;
        private bool showIntro = false;
        private UIImageView panel;
        private UIImageView sliderIndicator;
        private UIButton slider1;
        private UIButton slider2;
        private UIButton slider3;
        private UIButton slider4;
        private UIButton closeButton;
        private TutorialSubview tutorialView0;
        private TutorialSubview tutorialView1;
        private TutorialSubview tutorialView2;
        private TutorialSubview tutorialView3;
        private TutorialSubview tutorialView4;
        private UIPageControl pageControl;
        private UIScrollView scrollView;
        private String orientationStr;

        public delegate void TutorialViewDelegate ();

        public event TutorialViewDelegate HideEvent;

        public TutorialViewController(bool showIntro)
        {
            this.showIntro = showIntro;
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

            if (showIntro && isFirst)
            {
                isFirst = false;
                this.View.Hidden = true;

                CustomNavigationController navController = new CustomNavigationController();
                navController.SetViewControllers(new UIViewController[] { new IntroViewController(false) }, false);
                this.NavigationController.PresentViewControllerAsync(navController, false);
            }
            else
            {
                this.View.Hidden = false;
            }
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.Default;
        }

        private void InitializeControls()
        {
            this.View.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
            this.NavigationController.SetNavigationBarHidden(true, false);

            // scrollView
            scrollView = new UIScrollView();
            scrollView.Frame = this.View.Frame;
            scrollView.PagingEnabled = true;
            scrollView.ShowsVerticalScrollIndicator = false;
            scrollView.DecelerationEnded += HandleScrollViewDecelerationEnded;
            scrollView.Scrolled += HandleScrollViewScrolled;
            scrollView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
            this.View.AddSubview(scrollView);

            // pageControl
            pageControl = new UIPageControl();
            pageControl.Pages = 5;

            // Add page 0
            AddTutorial0();

            // Add page 1
            AddTutorial1();

            // Add page 2
            AddTutorial2();

            // Add page 3
            AddTutorial3();

            // Add page 4
            AddTutorial4();

            // Add panel
            AddPanel();
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            if (InterfaceOrientation == UIInterfaceOrientation.Portrait || InterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown)
            {
                orientationStr = StringRef.Portrait;
                this.View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle("Assets/Backgrounds/background_portrait.png"));
            }
            else
            {
                orientationStr = StringRef.Landscape;
                this.View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle("Assets/Backgrounds/background_landscape.png"));
            }

            // Update images
            UpdateImages();
        }

        public void UpdateImages()
        {
            // panel
            panel.Image = UIImage.FromBundle("Assets/Tutorial/" + orientationStr + "/panel.png");
            panel.Frame = scrollView.Frame;

            // tutorialView0
            tutorialView0.Background.Image = UIImage.FromBundle("Assets/Tutorial/" + orientationStr + "/tutorial_0.png");

            // tutorialView1
            tutorialView1.Background.Image = UIImage.FromBundle("Assets/Tutorial/" + orientationStr + "/tutorial_1.png");
            tutorialView1.Overlay1.Image = UIImage.FromBundle("Assets/Tutorial/" + orientationStr + "/tutorial_1_0.png");
            tutorialView1.Overlay2.Image = UIImage.FromBundle("Assets/Tutorial/" + orientationStr + "/tutorial_1_1.png");
            tutorialView1.Button.Frame = new CGRect(scrollView.Center.X - (tutorialView1.Button.Frame.Width / 2), scrollView.Frame.Bottom - 240f, tutorialView1.Button.Frame.Width, tutorialView1.Button.Frame.Height);

            // tutorialView2
            tutorialView2.Background.Image = UIImage.FromBundle("Assets/Tutorial/" + orientationStr + "/tutorial_2.png");
            tutorialView2.Overlay1.Image = UIImage.FromBundle("Assets/Tutorial/" + orientationStr + "/tutorial_2_0.png");
            tutorialView2.Overlay2.Image = UIImage.FromBundle("Assets/Tutorial/" + orientationStr + "/tutorial_2_1.png");
            tutorialView2.Button.Frame = tutorialView1.Button.Frame;

            // tutorialView3
            tutorialView3.Background.Image = UIImage.FromBundle("Assets/Tutorial/" + orientationStr + "/tutorial_3.png");
            tutorialView3.Overlay1.Image = UIImage.FromBundle("Assets/Tutorial/" + orientationStr + "/tutorial_3_0.png");
            tutorialView3.Overlay2.Image = UIImage.FromBundle("Assets/Tutorial/" + orientationStr + "/tutorial_3_1.png");
            tutorialView3.Button.Frame = tutorialView1.Button.Frame;

            // tutorialView4
            tutorialView4.Background.Image = UIImage.FromBundle("Assets/Tutorial/" + orientationStr + "/tutorial_4.png");
            tutorialView4.Overlay1.Image = UIImage.FromBundle("Assets/Tutorial/" + orientationStr + "/tutorial_4_0.png");

            if (orientationStr == StringRef.Portrait)
            {
                closeButton.Frame = new CGRect(this.View.Frame.Width - closeButton.Frame.Width, 641.5f, closeButton.Frame.Width, closeButton.Frame.Height);
                tutorialView0.Button.Frame = new CGRect(scrollView.Center.X - (tutorialView0.Button.Frame.Width / 2), 615f, tutorialView0.Button.Frame.Width, tutorialView0.Button.Frame.Height);
                slider1.Frame = new CGRect(this.View.Frame.Width - slider1.Frame.Width, 400f, slider1.Frame.Width, slider1.Frame.Height);
            }
            else
            {
                closeButton.Frame = new CGRect(this.View.Frame.Width - closeButton.Frame.Width, 513.5f, closeButton.Frame.Width, closeButton.Frame.Height);
                tutorialView0.Button.Frame = new CGRect(scrollView.Center.X - (tutorialView0.Button.Frame.Width / 2), scrollView.Frame.Bottom - 280f, tutorialView0.Button.Frame.Width, tutorialView0.Button.Frame.Height);
                slider1.Frame = new CGRect(this.View.Frame.Width - slider1.Frame.Width, 274f, slider1.Frame.Width, slider1.Frame.Height);
            }

            // slider
            slider2.Frame = new CGRect(slider1.Frame.X, slider1.Frame.Bottom, slider1.Frame.Width, slider1.Frame.Height);
            slider3.Frame = new CGRect(slider1.Frame.X, slider2.Frame.Bottom, slider1.Frame.Width, slider1.Frame.Height);
            slider4.Frame = new CGRect(slider1.Frame.X, slider3.Frame.Bottom, slider1.Frame.Width, slider1.Frame.Height);

            // Update tutorialView frame so that it fits the updated paging size
            int i = 0;
            foreach (UIView subview in scrollView)
            {
                if (subview is TutorialSubview)
                {
                    TutorialSubview tutorialView = subview as TutorialSubview;
                    tutorialView.Frame = new CGRect(0, scrollView.Frame.Height * i, scrollView.Frame.Width, scrollView.Frame.Height);

                    i++;
                }
            }

            UpdateSliderIndicatorFrame();

            scrollView.ContentOffset = new CGPoint(0, pageControl.CurrentPage * scrollView.Frame.Height);
            scrollView.ContentSize = new CGSize(scrollView.Frame.Width, scrollView.Frame.Height * 5);
        }

        private void AddPanel()
        {
            // panel
            panel = new UIImageView();
            panel.Alpha = 0f;
            this.View.AddSubview(panel);

            // closeButton
            UIImage image = UIImage.FromBundle("Assets/Tutorial/closeButton.png");
            closeButton = UIButton.FromType(UIButtonType.Custom);
            closeButton.SetImage(image, UIControlState.Normal);
            closeButton.SetImage(UIImage.FromBundle("Assets/Tutorial/closeButton_pressed.png"), UIControlState.Highlighted);
            closeButton.Frame = new CGRect(this.View.Frame.Width - image.Size.Width, 641.5f, image.Size.Width, image.Size.Height);
            closeButton.TouchUpInside += HandleCloseTouchUpInside;
            this.View.AddSubview(closeButton);

            // sliderIndicator
            image = UIImage.FromBundle("Assets/Tutorial/slider_on.png");
            sliderIndicator = new UIImageView(image);

            nfloat y = 297;
            if (orientationStr == StringRef.Portrait)
            {
                y = 425;
            }

            sliderIndicator.Frame = new CGRect(this.View.Frame.Width - 44.5f, y, image.Size.Width, image.Size.Height);
            sliderIndicator.Alpha = 0f;
            this.View.AddSubview(sliderIndicator);

            // slider1
            slider1 = GenerateSlider();
            slider1.Tag = 1;
            this.View.AddSubview(slider1);

            // slider2
            slider2 = GenerateSlider();
            slider2.Tag = 2;
            this.View.AddSubview(slider2);

            // slider3
            slider3 = GenerateSlider();
            slider3.Tag = 3;
            this.View.AddSubview(slider3);

            // slider4
            slider4 = GenerateSlider();
            slider4.Tag = 4;
            this.View.AddSubview(slider4);
        }

        private UIButton GenerateSlider()
        {
            UIButton button = UIButton.FromType(UIButtonType.Custom);
            button.Frame = new CGRect(0, 0, 75, 58);
            button.BackgroundColor = UIColor.Clear;
            button.TouchUpInside += HandleSliderTouchUpInside;
            button.Alpha = 0f;
            return button;
        }

        private void ToggleArrow(UIImageView imageView)
        {
            UIView.Animate(1d, delegate
            {
                if (imageView.Alpha == 1)
                {
                    imageView.Alpha = 0f;
                }
                else
                {
                    imageView.Alpha = 1f;
                }
            }, delegate
            {
                ToggleArrow(imageView);
            });
        }

        private void AddTutorial0()
        {
            // tutorialView0
            tutorialView0 = new TutorialSubview(scrollView.Frame);

            // Swipe button
            UIImage bgImage = UIImage.FromBundle("Assets/Tutorial/circleButton_large.png");
            tutorialView0.AddLargeButton(bgImage);
            tutorialView0.Button.Frame = new CGRect(scrollView.Center.X - (bgImage.Size.Width / 2), 615, bgImage.Size.Width, bgImage.Size.Height);
            tutorialView0.Button.TouchUpInside += delegate
            {
                ScrollToPage(1);
            };
            scrollView.AddSubview(tutorialView0);

            // Flash animation
            ToggleArrow(tutorialView0.Button.ImageView);
        }

        private void AddTutorial1()
        {
            tutorialView1 = new TutorialSubview(scrollView.Frame);
            tutorialView1.Frame = new CGRect(0, scrollView.Frame.Height, scrollView.Frame.Width, scrollView.Frame.Height);
            tutorialView1.AddOverlay1();
            tutorialView1.AddOverlay2();

            // Swipe button
            UIImage bgImage = UIImage.FromBundle("Assets/Tutorial/circleButton_small.png");
            tutorialView1.AddSmallButton(bgImage);
            tutorialView1.Button.Frame = new CGRect(scrollView.Center.X - (bgImage.Size.Width / 2), scrollView.Frame.Bottom - 240f, bgImage.Size.Width, bgImage.Size.Height);
            tutorialView1.Button.TouchUpInside += delegate
            {
                ScrollToPage(2);
            };
            scrollView.AddSubview(tutorialView1);

            // Flash animation
            ToggleArrow(tutorialView1.Button.ImageView);
        }

        private void AddTutorial2()
        {
            tutorialView2 = new TutorialSubview(scrollView.Frame);
            tutorialView2.Frame = new CGRect(0, scrollView.Frame.Height * 2, scrollView.Frame.Width, scrollView.Frame.Height);
            tutorialView2.AddOverlay1();
            tutorialView2.AddOverlay2();

            // Swipe button
            UIImage bgImage = UIImage.FromBundle("Assets/Tutorial/circleButton_small.png");
            tutorialView2.AddSmallButton(bgImage);
            tutorialView2.Button.Frame = new CGRect(scrollView.Center.X - (bgImage.Size.Width / 2), scrollView.Frame.Bottom - 240f, bgImage.Size.Width, bgImage.Size.Height);
            tutorialView2.Button.TouchUpInside += delegate
            {
                ScrollToPage(3);
            };
            scrollView.AddSubview(tutorialView2);

            // Flash animation
            ToggleArrow(tutorialView2.Button.ImageView);
        }

        private void AddTutorial3()
        {
            tutorialView3 = new TutorialSubview(scrollView.Frame);
            tutorialView3.Frame = new CGRect(0, scrollView.Frame.Height * 3, scrollView.Frame.Width, scrollView.Frame.Height);
            tutorialView3.AddOverlay1();
            tutorialView3.AddOverlay2();

            // Swipe button
            UIImage bgImage = UIImage.FromBundle("Assets/Tutorial/circleButton_small.png");
            tutorialView3.AddSmallButton(bgImage);
            tutorialView3.Button.Frame = new CGRect(scrollView.Center.X - (bgImage.Size.Width / 2), scrollView.Frame.Bottom - 240f, bgImage.Size.Width, bgImage.Size.Height);
            tutorialView3.Button.TouchUpInside += delegate
            {
                ScrollToPage(4);
            };
            scrollView.AddSubview(tutorialView3);

            // Flash animation
            ToggleArrow(tutorialView3.Button.ImageView);
        }

        private void AddTutorial4()
        {
            tutorialView4 = new TutorialSubview(scrollView.Frame);
            tutorialView4.Frame = new CGRect(0, scrollView.Frame.Height * 4, scrollView.Frame.Width, scrollView.Frame.Height);
            tutorialView4.AddOverlay1();
            scrollView.AddSubview(tutorialView4);
        }

        private void AnimateOverlay(UIImageView overlay1, UIImageView overlay2 = null)
        {
            // Show animation if not shown
            if (overlay1.Alpha == 0)
            {
                UIView.Animate(0.5d, delegate
                {
                    overlay1.Alpha = 1f;
                }, delegate
                {
                    UIView.Animate(0.5d, delegate
                    {
                        if (overlay2 != null)
                        {
                            overlay2.Alpha = 1f;
                        }
                    });
                });
            }
        }

        private void ShowOverlayAnimation()
        {
            if (pageControl.CurrentPage == 1)
            {
                AnimateOverlay(tutorialView1.Overlay1, tutorialView1.Overlay2);
            }
            else if (pageControl.CurrentPage == 2)
            {
                AnimateOverlay(tutorialView2.Overlay1, tutorialView2.Overlay2);
            }
            else if (pageControl.CurrentPage == 3)
            {
                AnimateOverlay(tutorialView3.Overlay1, tutorialView3.Overlay2);
            }
            else if (pageControl.CurrentPage == 4)
            {
                AnimateOverlay(tutorialView4.Overlay1);
            }

            ShowHidePanel();
        }

        private void ShowHidePanel()
        {
            nfloat alpha = 1f;
            if (pageControl.CurrentPage == 0)
            {
                alpha = 0f;
            }

            UIView.Animate(0.3d, delegate
            {
                panel.Alpha = sliderIndicator.Alpha = slider1.Alpha = slider2.Alpha = slider3.Alpha = slider4.Alpha = alpha;

                UpdateSliderIndicatorFrame();
            });
        }

        private void UpdateSliderIndicatorFrame()
        {
            nfloat x = scrollView.Frame.Width - 44.5f;
            nfloat y = 297;
            if (orientationStr == StringRef.Portrait)
            {
                x += 0.5f;
                y = 425;
            }

            // Calculate offset
            if (pageControl.CurrentPage > 0)
            {
                y += (58f * (pageControl.CurrentPage - 1));
            }

            sliderIndicator.Frame = new CGRect(x, y, sliderIndicator.Frame.Width, sliderIndicator.Frame.Height);
        }

        private void ScrollToPage(nint page)
        {
            if (page < 0 || page >= pageControl.Pages)
            {
                return;
            }

            UIPageControl.AnimationsEnabled = true;
            UIPageControl.Animate(0.3d, delegate
            {
                scrollView.SetContentOffset(new CGPoint(0, scrollView.Frame.Height * page), false);
                pageControl.CurrentPage = page;
            }, delegate
            {
                // Show animation
                ShowOverlayAnimation();
            });
        }

        void HandleScrollViewDecelerationEnded(object sender, EventArgs e)
        {
            if (pageControl.Pages > 0)
            {
                // Switch the indicator when more than 50% of the previous/next page is visible
                double page = Math.Floor((scrollView.ContentOffset.Y - (scrollView.Frame.Height / 2)) / scrollView.Frame.Height) + 1;
                if (pageControl.CurrentPage != page)
                {
                    pageControl.CurrentPage = (int)page;

                    // Show animation
                    ShowOverlayAnimation();
                }
            }
        }

        void HandleScrollViewScrolled(object sender, EventArgs e)
        {
            // Calculate newAlpha value for the current page
            nfloat offset = scrollView.ContentOffset.Y - (pageControl.CurrentPage * scrollView.Frame.Height);
            if (pageControl.CurrentPage == 0 && offset <= 0)
            {
                offset = 0;
            }
            else
            {
                if (pageControl.CurrentPage == 4 && offset >= 0)
                {
                    offset = 0;
                }
            }

            nfloat newAlpha = (nfloat)1f - (nfloat)Math.Abs(offset / scrollView.Frame.Height);
            if (offset == 0)
            {
                newAlpha = 1f;
            }

            // Hide panel and sliderIndicator for the first page
            if (pageControl.CurrentPage == 0 && offset >= 0)
            {
                panel.Alpha = sliderIndicator.Alpha = slider1.Alpha = slider2.Alpha = slider3.Alpha = slider4.Alpha = 1f - newAlpha;
            }
            else if (pageControl.CurrentPage == 1 && offset <= 0)
            {
                panel.Alpha = sliderIndicator.Alpha = slider1.Alpha = slider2.Alpha = slider3.Alpha = slider4.Alpha = newAlpha;
            }
            else
            {
                panel.Alpha = sliderIndicator.Alpha = slider1.Alpha = slider2.Alpha = slider3.Alpha = slider4.Alpha = 1f;
            }

            // Update with newAlpha
            if (pageControl.CurrentPage == 0)
            {
                tutorialView0.Alpha = newAlpha;
                tutorialView1.Alpha = tutorialView2.Alpha = tutorialView3.Alpha = tutorialView4.Alpha = 1f;
            }
            else if (pageControl.CurrentPage == 1)
            {
                tutorialView1.Alpha = newAlpha;
                tutorialView0.Alpha = tutorialView2.Alpha = tutorialView3.Alpha = tutorialView4.Alpha = 1f;
            }
            else if (pageControl.CurrentPage == 2)
            {
                tutorialView2.Alpha = newAlpha;
                tutorialView0.Alpha = tutorialView1.Alpha = tutorialView3.Alpha = tutorialView4.Alpha = 1f;
            }
            else if (pageControl.CurrentPage == 3)
            {
                tutorialView3.Alpha = newAlpha;
                tutorialView0.Alpha = tutorialView1.Alpha = tutorialView2.Alpha = tutorialView4.Alpha = 1f;
            }
            else if (pageControl.CurrentPage == 4)
            {
                tutorialView4.Alpha = newAlpha;
                tutorialView0.Alpha = tutorialView1.Alpha = tutorialView2.Alpha = tutorialView3.Alpha = 1f;
            }
        }

        void HandleSliderTouchUpInside(object sender, EventArgs e)
        {
            UIButton slider = sender as UIButton;
            ScrollToPage(slider.Tag);
        }

        void HandleCloseTouchUpInside(object sender, EventArgs e)
        {
            Settings.WriteTutorialClosed(true);

            if (HideEvent != null)
            {
                HideEvent();
            }
        }
    }

    public class TutorialSubview : UIView
    {
        public UIButton Button { get; set; }

        public UIImageView Background { get; set; }

        public UIImageView Overlay1 { get; set; }

        public UIImageView Overlay2 { get; set; }

        public TutorialSubview(CGRect frame)
        {
            this.Frame = frame;
            this.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;

            // Background
            Background = new UIImageView(frame);
            Background.AutoresizingMask = UIViewAutoresizing.All;
            this.AddSubview(Background);
        }

        public void AddOverlay1()
        {
            // Overlay1
            Overlay1 = new UIImageView(new CGRect(0, 0, this.Frame.Width, this.Frame.Height));
            Overlay1.Alpha = 0f;
            Overlay1.AutoresizingMask = UIViewAutoresizing.All;
            this.AddSubview(Overlay1);
        }

        public void AddOverlay2()
        {
            // Overlay2
            Overlay2 = new UIImageView(new CGRect(0, 0, this.Frame.Width, this.Frame.Height));
            Overlay2.Alpha = 0f;
            Overlay2.AutoresizingMask = UIViewAutoresizing.All;
            this.AddSubview(Overlay2);
        }

        public void AddLargeButton(UIImage bgImage)
        {
            Button = UIButton.FromType(UIButtonType.Custom);
            Button.SetBackgroundImage(bgImage, UIControlState.Normal);
            Button.SetBackgroundImage(UIImage.FromBundle("Assets/Tutorial/circleButton_large_pressed.png"), UIControlState.Highlighted);
            Button.SetImage(UIImage.FromBundle("Assets/Tutorial/arrow_up_large.png"), UIControlState.Normal);
            this.AddSubview(Button);
        }

        public void AddSmallButton(UIImage bgImage)
        {
            Button = UIButton.FromType(UIButtonType.Custom);
            Button.SetBackgroundImage(bgImage, UIControlState.Normal);
            Button.SetBackgroundImage(UIImage.FromBundle("Assets/Tutorial/circleButton_small_pressed.png"), UIControlState.Highlighted);
            Button.SetImage(UIImage.FromBundle("Assets/Tutorial/arrow_up_small.png"), UIControlState.Normal);
            this.AddSubview(Button);
        }
    }
}

