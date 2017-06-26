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
using ObjCRuntime;
using Metrostar.Mobile.Framework;
using PaperFoldBinding;
using PSPDFKit;
using MssFramework;
using System.Threading.Tasks;

namespace eBriefingMobile
{
	public partial class DashboardViewController : DispatcherViewController
	{
		private bool isFirst = true;
		private nfloat lastOffset = 0;
		private Book book;
		private UIButton expandButton;
		private DashboardView dashboardView;
		private UISegmentedControl segmentedControl;
		private UICollectionView collectionView;
		private UILabel statusLabel;
		private List<Chapter> chapterList = null;
		private List<Bookmark> bookmarkList = null;
		private List<Note> noteList = null;
		private List<Annotation> annList = null;
		private UIView centerView;
		private UIView topView;
		private PaperFoldView foldView;
		private DashboardMenuView floatMenu;
		private bool expandDashboard = false;
		private bool menuAnimating = false;
		private NSObject rotationNotification;
		private UIBarButtonItem contentSyncErrorButton;
		private UIPopoverController contentSyncViewController;

		public DashboardViewController(Book book)
		{
			this.book = book;
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

			//for rotation
			UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications ();
			rotationNotification=NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIDeviceOrientationDidChangeNotification"), DeviceOrientationDidChange );

			this.NavigationController.NavigationBar.SetBackgroundImage(UIImage.FromBundle("Assets/Backgrounds/navbar.png").CreateResizableImage(new UIEdgeInsets(0, 1, 0, 1)), UIBarMetrics.Default);
			this.NavigationController.InteractivePopGestureRecognizer.Enabled = false;

			BookUpdater.RegisterASIDelegate(this);

			if (isFirst)
			{
				InitializeControls();

				// Update new status
				UpdateBookStatus();
			}
			else
			{
				ExpandCollapseDashboard(false, false);
			}

			// floatMenu
			floatMenu = AppDelegate.Current.AddDashboardFloatMenu();
			floatMenu.SortEvent += HandleSortEvent;
			floatMenu.CollapseEvent += HandleCollapseEvent;
			floatMenu.ExpandEvent += HandleExpandEvent;
			floatMenu.GoToFirstEvent += HandleGoToFirstEvent;
			floatMenu.GoToCurrentEvent += HandleGoToCurrentEvent;

			// Reload data
			LoadData();

			isFirst = false;
		}

		private void DeviceOrientationDidChange(NSNotification notification)
		{
			UIInterfaceOrientation orientation = UIApplication.SharedApplication.StatusBarOrientation;
			this.WillRotate (orientation, 1.0);
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			if (floatMenu != null)
			{
				floatMenu.RemoveFromSuperview();
				floatMenu.Dispose();
				floatMenu = null;
			}

			UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications ();
			if ( rotationNotification != null )
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver (this,new NSString ("UIDeviceOrientationDidChangeNotification"), null);
			}
		}

		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate(toInterfaceOrientation, duration);

			// Due to the bug in PaperFold library, collapse LeftFoldView in WillRotate and expand in DidRotate
			if (foldView.State != PaperFoldState.Default)
			{
				expandDashboard = true;

				ExpandCollapseDashboard(false, false);
			}

			if ( collectionView != null )
			{
				collectionView.ReloadData ();
			}

			foldView.Frame = new CGRect(0, 0, this.View.Frame.Width, this.View.Frame.Height);
			centerView.Frame = new CGRect(0, 0, foldView.Frame.Width, foldView.Frame.Height);
			topView.Frame = new CGRect (-2, 0, centerView.Frame.Width + 2, 69);
		
			if ( floatMenu != null )
			{
				floatMenu.Frame = new CGRect (UIScreen.MainScreen.Bounds.Size.Width - floatMenu.Frame.Width, UIScreen.MainScreen.Bounds.Size.Height - floatMenu.Frame.Height, floatMenu.Frame.Width, floatMenu.Frame.Height);
			}
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);

			if (expandDashboard)
			{
				// Due to the bug in PaperFold library, collapse LeftFoldView in WillRotate and expand in DidRotate
				expandDashboard = false;

				ExpandCollapseDashboard(true, true);
			}
			else
			{
				// Due to the bug in PaperFold library, take screenshot of dashBoardView and set it to the LeftFoldView
				UpdateScreenshot();
			}
		}

		async private void LoadData()
		{
			if (isFirst)
			{
				LoadingView.Show("Loading", "Please wait while we're loading the book information...", false);

				await eBriefingService.Run(() => RetrieveData());
			}
			else
			{
				LoadingView.Show("Loading", "Please wait while we're updating the book information...", false);

				await eBriefingService.Run(() => RetrieveData());
			}

			LoadingView.Hide();

			// Update number of bookmarks
			int count = 0;
			if (bookmarkList != null && bookmarkList.Count > 0)
			{
				count = bookmarkList.Count;
			}
			segmentedControl.SetTitle("Bookmarks (" + count.ToString() + ")", 1);

			// Update number of notes
			count = 0;
			if (noteList != null && noteList.Count > 0)
			{
				count = noteList.Count;
			}
			segmentedControl.SetTitle("Notes (" + count.ToString() + ")", 2);

			// Update number of annotations
			count = 0;
			if (annList != null && annList.Count > 0)
			{
				count = annList.Count;
			}
			segmentedControl.SetTitle("Annotations (" + count.ToString() + ")", 3);

			// Load collectionView
			LoadCollectionView();

			// Update overview pane info
			if (dashboardView != null)
			{
				dashboardView.UpdateBookInfo();
			}
		}

		private void InitializeControls()
		{
			this.NavigationItem.BackBarButtonItem = new UIBarButtonItem(String.Empty, UIBarButtonItemStyle.Plain, null);
			this.NavigationItem.Title = book.Title;

			// foldView
			foldView = new PaperFoldView();
			foldView.BackgroundColor = UIColor.Clear;
			foldView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			foldView.EnableLeftFoldDragging = true;
			foldView.UseOptimizedScreenshot = true;
			foldView.Frame = new CGRect(0, 0, this.View.Frame.Width, this.View.Frame.Height);
			this.View.AddSubview(foldView);

			// dashboardView
			dashboardView = new DashboardView(book, new CGRect(0, 0, 290, foldView.Frame.Height));
			dashboardView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			dashboardView.InfoUpdatedEvent += delegate
			{
				UpdateScreenshot();
			};
			foldView.SetLeftFoldContentView(dashboardView, 1, 0.9f);

			// centerView
			centerView = new UIView();
			centerView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			centerView.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle("Assets/Backgrounds/background_landscape.png"));
			centerView.Frame = new CGRect(0, 0, foldView.Frame.Width, foldView.Frame.Height);
			foldView.CenterContentView = centerView;

			// topView
			topView = new UIView(new CGRect(-2, 0, centerView.Frame.Width + 2, 69));
			topView.BackgroundColor = UIColor.White;
			topView.Layer.BorderColor = eBriefingAppearance.Gray5.CGColor;
			topView.Layer.BorderWidth = 1f;
			centerView.AddSubview(topView);

			// expandButton
			expandButton = UIButton.FromType(UIButtonType.Custom);
			expandButton.Frame = new CGRect(0, 20, 34, 29);
			expandButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/hamburger.png"), UIControlState.Normal);
			expandButton.TouchUpInside += delegate
			{
				if (foldView.State == PaperFoldState.Default)
				{
					ExpandCollapseDashboard(true, true);
				}
				else
				{
					ExpandCollapseDashboard(false, true);
				}
			};
			topView.AddSubview(expandButton);

			// segmentedControl
			segmentedControl = new UISegmentedControl(new object[] {
				"Chapters",
				"Bookmarks (0)",
				"Notes (0)",
				"Annotations (0)"
			});
			segmentedControl.Frame = new CGRect(expandButton.Frame.Right + 10, 20, topView.Frame.Width - (expandButton.Frame.Right + 30), 37);
			segmentedControl.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			segmentedControl.TintColor = eBriefingAppearance.BlueColor;
			segmentedControl.SelectedSegment = 0;
			segmentedControl.ValueChanged += HandleValueChanged;
			topView.AddSubview(segmentedControl);

			// statusLabel
			statusLabel = eBriefingAppearance.GenerateLabel(27);
			statusLabel.Frame = new CGRect(0, (centerView.Frame.Height / 2) - (21f / 2f), centerView.Frame.Width, 21);
			statusLabel.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;
			statusLabel.TextAlignment = UITextAlignment.Center;
			centerView.AddSubview(statusLabel);

			//contentSyncButton
			var contentSyncButton= new UIButton(UIButtonType.Custom);
			contentSyncButton.SetImage (UIImage.FromBundle ("Assets/Buttons/icn_contentSyncError.png"), UIControlState.Normal);
			contentSyncButton.TouchUpInside += HandleContentSyncErrorButtonTouchUpInside;
			contentSyncButton.Frame = new CGRect (0, 0, contentSyncButton.CurrentImage.Size.Width, contentSyncButton.CurrentImage.Size.Height);
			contentSyncErrorButton = new UIBarButtonItem (contentSyncButton);

			if(!URL.ServerURL.Contains(StringRef.DemoURL))
			{
				this.NavigationItem.SetRightBarButtonItem (contentSyncErrorButton, true);
			}
		}

		void HandleContentSyncErrorButtonTouchUpInside(object sender, EventArgs args)
		{
			HidePopovers();

			//popoverContentSyncViewController
			var popover= new PopoverContentSyncViewController();
			popover.SyncButtonTouch += HandleSyncButtonTouchEvent;
			popover.View.Frame = new CGRect (0, 0, 300, 190);

			// contentSyncViewController
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
				await OpenSyncView ();
			}
			else
			{
				AlertView.Show(StringRef.connectionFailure, StringRef.connectionRequired, StringRef.ok);
			}
		}

		private void HidePopovers()
		{
			if (contentSyncViewController != null)
			{
				contentSyncViewController.Dismiss(true);
			}
		}

		async private Task OpenSyncView()
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
		}

		private void UpdateScreenshot()
		{
			foldView.LeftFoldView.ScreenshotImage = dashboardView.Screenshot();
		}

		private void ExpandCollapseDashboard(bool expand, bool animated)
		{
			if (expand)
			{
				foldView.SetPaperFoldState(PaperFoldState.LeftUnfolded, animated);
			}
			else
			{
				foldView.SetPaperFoldState(PaperFoldState.Default, animated);
			}
		}

		private void InitializeCollectionView(Type cellType, String cellID)
		{
			// collectionView
			if (segmentedControl != null)
			{
				foreach (UIView subview in centerView)
				{
					if (subview is UICollectionView)
					{
						subview.RemoveFromSuperview();
					}
				}

				collectionView = new UICollectionView(new CGRect(segmentedControl.Frame.X, topView.Frame.Bottom + 20, segmentedControl.Frame.Width, centerView.Frame.Bottom - (topView.Frame.Bottom + 20)), new DashboardLayout());
				collectionView.RegisterClassForCell(cellType, new NSString(cellID));
				collectionView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
				collectionView.BackgroundColor = UIColor.Clear;
				collectionView.ShowsVerticalScrollIndicator = false;
				collectionView.AlwaysBounceVertical = true;
				collectionView.ContentInset = new UIEdgeInsets(0, 0, floatMenu.Frame.Height, 0);
				centerView.AddSubview(collectionView);
			}
		}

		private void UpdateBookStatus()
		{
			// Update new status
			book.New = false;

			BooksOnDeviceAccessor.UpdateBook(book);
		}

		private void RetrieveData()
		{
			chapterList = BooksOnDeviceAccessor.GetChapters(book.ID);
			bookmarkList = BooksOnDeviceAccessor.GetBookmarks(book.ID);
			noteList = BooksOnDeviceAccessor.GetNotes(book.ID);
			annList = BooksOnDeviceAccessor.GetAnnotations(book.ID);

			// Update chapter number for backward compatibililty
			if (chapterList != null && chapterList.Count > 0)
			{
				for (int i = 0; i < chapterList.Count; i++)
				{
					chapterList[i].ChapterNumber = i + 1;
				}

				BooksOnDeviceAccessor.UpdateChapters(book.ID, chapterList);
			}
		}

		private void LoadCollectionView()
		{
			if (segmentedControl.SelectedSegment == 0)
			{
				LoadChapterView();
			}
			else if (segmentedControl.SelectedSegment == 1)
			{
				LoadBookmarkView();
			}
			else if (segmentedControl.SelectedSegment == 2)
			{
				LoadNotePanel();
			}
			else
			{
				LoadAnnotationView();
			}
		}

		#region FloatMenu

		void HandleValueChanged(object sender, EventArgs e)
		{
			floatMenu.Frame = new CGRect(floatMenu.Frame.X, centerView.Frame.Bottom + 64, floatMenu.Frame.Width, floatMenu.Frame.Height);

			LoadCollectionView();
		}

		void HandleScrolledEvent(UIScrollView scrollView)
		{
			if (floatMenu != null && !menuAnimating)
			{
				nfloat centerViewBottom = centerView.Frame.Bottom + 64;

				// Hide menu
				if (scrollView.ContentOffset.Y > lastOffset && scrollView.ContentOffset.Y > 0)
				{
					menuAnimating = true;

					UIView.Animate(0.3d, delegate
					{
						floatMenu.Frame = new CGRect(floatMenu.Frame.X, centerViewBottom, floatMenu.Frame.Width, floatMenu.Frame.Height);
					}, delegate
					{
						floatMenu.ExpandNCollapse(false);
						menuAnimating = false;
					});
				}

				// Show menu
				if (scrollView.ContentOffset.Y < lastOffset)
				{
					menuAnimating = true;

					UIView.Animate(0.3d, delegate
					{
						floatMenu.Frame = new CGRect(floatMenu.Frame.X, centerViewBottom - floatMenu.Frame.Height, floatMenu.Frame.Width, floatMenu.Frame.Height);
					}, delegate
					{
						menuAnimating = false;
					});
				}

				lastOffset = scrollView.ContentOffset.Y;
			}
		}

		void HandleGoToCurrentEvent()
		{
			if (String.IsNullOrEmpty(Settings.CurrentPageID))
			{
				List<Page> pageList = BooksOnDeviceAccessor.GetPages(book.ID);
				if (pageList != null)
				{
					LoadPageViewController(pageList[0].ID);
				}
			}
			else
			{
				LoadPageViewController(Settings.CurrentPageID);
			}
		}

		void HandleGoToFirstEvent()
		{
			List<Page> pageList = BooksOnDeviceAccessor.GetPages(book.ID);
			if (pageList != null)
			{
				LoadPageViewController(pageList[0].ID);
			}
		}

		void HandleCollapseEvent()
		{
			((NoteDataSource)collectionView.DataSource).CollapseAll();
		}

		void HandleExpandEvent()
		{
			((NoteDataSource)collectionView.DataSource).ExpandAll();
		}

		void HandleSortEvent()
		{
			if (segmentedControl.SelectedSegment == 2)
			{
				((NoteDataSource)collectionView.DataSource).Sort();
			}
			else
			{
				((DashboardDataSource)collectionView.DataSource).Sort(collectionView);
			}
		}

		#endregion

		#region Chapter

		private void LoadChapterView()
		{
			if (chapterList != null && chapterList.Count > 0)
			{
				// collectionView
				InitializeCollectionView(typeof(ChapterCell), "chapterCell");

				// flowLayout
				collectionView.CollectionViewLayout = new DashboardLayout() {
					ItemSize = new CGSize(220, 388.33f)
				};

				// dataSource
				DashboardDataSource dataSource = new DashboardDataSource(book, chapterList as Object, DashboardDataSource.DataType.CHAPTERS, "chapterCell");
				dataSource.ItemPressedEvent += HandleChapterPressedEvent;
				collectionView.Source = dataSource;
				collectionView.ReloadData();
				collectionView.LayoutIfNeeded();

				ShowHideChapterStatus(false);
			}
			else
			{
				ShowHideChapterStatus(true);
			}
		}

		private void ShowHideChapterStatus(bool isEmpty)
		{
			statusLabel.Hidden = !isEmpty;
			if ( collectionView != null )
			{
				collectionView.Hidden = isEmpty;
			}

			if (!statusLabel.Hidden)
			{
				statusLabel.Text = "There are no Chapters.";
			}
		}

		void HandleChapterPressedEvent(String pageID)
		{
			if (String.IsNullOrEmpty(pageID))
			{
				AlertView.Show(StringRef.alert, "The book does not contain this page.", StringRef.ok);
			}
			else
			{
				LoadPageViewController(pageID);
			}
		}

		#endregion

		#region Bookmark

		private void LoadBookmarkView()
		{
			if (bookmarkList != null && bookmarkList.Count > 0)
			{
				// collectionView
				InitializeCollectionView(typeof(ThumbnailCell), "bookmarkCell");

				// flowLayout
				collectionView.CollectionViewLayout = new DashboardLayout() {
					ItemSize = new CGSize(170, 270),
					SectionInset = new UIEdgeInsets(0, 0, 20, 0),
					HeaderReferenceSize = new CGSize(0, 0),
					MinimumLineSpacing = 20.0f
				};

				// dataSource
				DashboardDataSource dataSource = new DashboardDataSource(book, bookmarkList as Object, DashboardDataSource.DataType.BOOKMARKS, "bookmarkCell");
				dataSource.ItemPressedEvent += HandleBookmarkPressedEvent;
				collectionView.Source = dataSource;
				collectionView.ReloadData();
				collectionView.LayoutIfNeeded();

				ShowHideBookmarkStatus(false);
			}
			else
			{
				ShowHideBookmarkStatus(true);
			}
		}

		private void ShowHideBookmarkStatus(bool isEmpty)
		{
			statusLabel.Hidden = !isEmpty;
			if ( collectionView != null )
			{
				collectionView.Hidden = isEmpty;
			}

			if (!statusLabel.Hidden)
			{
				statusLabel.Text = "There are no Bookmarks.";
			}
		}

		void HandleBookmarkPressedEvent(String pageID)
		{
			if (String.IsNullOrEmpty(pageID))
			{
				AlertView.Show(StringRef.alert, "The book does not contain this page.", StringRef.ok);
			}
			else
			{
				LoadPageViewController(pageID);
			}
		}

		#endregion

		#region Note

		private void LoadNotePanel()
		{
			if (noteList != null && noteList.Count > 0)
			{
				// collectionView
				InitializeCollectionView(typeof(NoteCell), "noteCell");

				// layout
				collectionView.CollectionViewLayout = new NoteFlowLayout() {
					HeaderReferenceSize = new CGSize(collectionView.Frame.Width, 42)
				};

				collectionView.RegisterClassForSupplementaryView(typeof(NoteHeader), UICollectionElementKindSection.Header, new NSString("noteHeader"));

				// dataSource
				NoteDataSource dataSource = new NoteDataSource(book, collectionView, "noteCell");
				dataSource.ItemPressedEvent += HandleNotePressedEvent;
				dataSource.ScrolledEvent += HandleScrolledEvent;

				collectionView.Source = dataSource;
				collectionView.Delegate = new NoteCollectionViewDelegateFlowLayout(dataSource);
				collectionView.ReloadData();
				collectionView.LayoutIfNeeded();

				if (collectionView.ContentSize.Height < collectionView.Frame.Height)
				{
					floatMenu.Frame = new CGRect(floatMenu.Frame.X, centerView.Frame.Bottom + 64 - floatMenu.Frame.Height, floatMenu.Frame.Width, floatMenu.Frame.Height);
				}

				ShowHideNoteStatus(false);
			}
			else
			{
				ShowHideNoteStatus(true);
			}
		}

		private void ShowHideNoteStatus(bool isEmpty)
		{
			statusLabel.Hidden = !isEmpty;
			if ( collectionView != null )
			{
				collectionView.Hidden = isEmpty;
			}

			if (!statusLabel.Hidden)
			{
				statusLabel.Text = "There are no Notes.";
			}
		}

		void HandleNotePressedEvent(String pageID, Note note)
		{
			if (String.IsNullOrEmpty(pageID))
			{
				AlertView.Show(StringRef.alert, "The book does not contain this page.", StringRef.ok);
			}
			else
			{
				Settings.OpenNotePanel = true;

				LoadPageViewController(pageID);
			}
		}

		#endregion

		#region Annotation

		private void LoadAnnotationView()
		{
			if (annList != null && annList.Count > 0)
			{
				// collectionView
				InitializeCollectionView(typeof(ThumbnailCell), "annotationCell");

				// flowLayout
				collectionView.CollectionViewLayout = new DashboardLayout() {
					ItemSize = new CGSize(170, 270),
					SectionInset = new UIEdgeInsets(0, 0, 20, 0),
					HeaderReferenceSize = new CGSize(0, 0),
					MinimumLineSpacing = 20.0f
				};

				// dataSource
				DashboardDataSource dataSource = new DashboardDataSource(book, annList as Object, DashboardDataSource.DataType.ANNOTATIONS, "annotationCell");
				dataSource.ItemPressedEvent += HandleAnnotationPressedEvent;
				collectionView.Source = dataSource;
				collectionView.ReloadData();
				collectionView.LayoutIfNeeded();

				ShowHideAnnotationStatus(false);
			}
			else
			{
				ShowHideAnnotationStatus(true);
			}
		}

		private void ShowHideAnnotationStatus(bool isEmpty)
		{
			statusLabel.Hidden = !isEmpty;
			if ( collectionView != null )
			{
				collectionView.Hidden = isEmpty;
			}

			if (!statusLabel.Hidden)
			{
				statusLabel.Text = "There are no Annotations.";
			}
		}

		void HandleAnnotationPressedEvent(String pageID)
		{
			if (String.IsNullOrEmpty(pageID))
			{
				AlertView.Show(StringRef.alert, "The book does not contain this page.", StringRef.ok);
			}
			else
			{
				LoadPageViewController(pageID);
			}
		}

		#endregion

		#region Page

		private PSPDFDocument GenerateDocument()
		{
			try
			{
				// Build local pdf file path array
				List<Page> pageList = BooksOnDeviceAccessor.GetPages(book.ID);
				if (pageList != null && pageList.Count > 0)
				{
					List<String> urlList = new List<String>();
					foreach (Page page in pageList)
					{
						urlList.Add(DownloadedFilesCache.BuildCachedFilePath(page.URL));
					}

					// Generate NSData array for each pdf file
					List<NSData> dataList = new List<NSData>();
					foreach (String url in urlList)
					{
						NSData data = NSData.FromFile(url);
						dataList.Add(data);
					}
			
					// Generate PSPDFDocument from NSData array
					if (dataList.Count > 0)
					{
						PSPDFDocument document = PSPDFDocument.FromData(dataList.ToArray());
						if (document != null)
						{
							document.Title = book.Title;
							return document;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("DashboardViewController - GenerateDocument: {0}", ex.ToString());
			}

			return null;
		}

		async private void LoadPageViewController(String pageID)
		{
			try
			{
				LoadingView.Show("Loading", "Please wait while we're loading " + book.Title + "...", false);

				int pageNumber = BooksOnDeviceAccessor.GetPage(book.ID, pageID).PageNumber;
				PSPDFDocument document = await eBriefingService.Run(() => GenerateDocument());

				if (document == null)
				{
					LoadingView.Hide();

					AlertView.Show(StringRef.alert, "We're sorry, but we could not open the document.", StringRef.ok);
				}
				else
				{
					PSPDFConfiguration configuration = PSPDFConfiguration.FromConfigurationBuilder(delegate(PSPDFConfigurationBuilder builder)
					{
						builder.CreateAnnotationMenuEnabled = builder.ShouldHideStatusBar = builder.ShouldCacheThumbnails = false;
						builder.ShouldHideStatusBarWithHUD = builder.AlwaysBouncePages = builder.SmartZoomEnabled = builder.DoublePageModeOnFirstPage
                            = builder.ShouldHideHUDOnPageChange = builder.ShouldHideNavigationBarWithHUD = true;
						builder.HUDViewMode = PSPDFHUDViewMode.Automatic;
						builder.HUDViewAnimation = PSPDFHUDViewAnimation.Fade;
						builder.ThumbnailBarMode = PSPDFThumbnailBarMode.None;
						builder.RenderingMode = PSPDFPageRenderingMode.Render;
						builder.PageTransition = PSPDFPageTransition.Curl;
						builder.ShouldAskForAnnotationUsername = false;
						builder.AllowBackgroundSaving = false;
						builder.OverrideClass(new Class(typeof(PSPDFHUDView)), new Class(typeof(CustomPSPDFHUDView)));
						builder.OverrideClass(new Class(typeof(PSPDFViewControllerDelegate)), new Class(typeof(CustomPSPDFViewControllerDelegate)));
						builder.OverrideClass(new Class(typeof(PSPDFBarButtonItem)), new Class(typeof(CustomPSPDFBarButtonItem)));

					});

					PageViewController pvc = new PageViewController(book, document, configuration);
					pvc.Delegate = new CustomPSPDFViewControllerDelegate();
					pvc.Page = (nuint)pageNumber - 1;

					pvc.AddAnnotations();

					LoadingView.Hide();
					this.NavigationController.PushViewController(pvc, true);
				}
			}
			catch (Exception ex)
			{
				LoadingView.Hide();

				Logger.WriteLineDebugging("DashboardViewController - LoadPageViewController: {0}", ex.ToString());
			}
		}

		#endregion
	}

}

