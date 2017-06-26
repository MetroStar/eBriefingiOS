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
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreGraphics;
using Metrostar.Mobile.Framework;
using PSPDFKit;
using MessageUI;
using MssFramework;
using System.Threading.Tasks;

namespace eBriefingMobile
{
	public partial class PageViewController : CustomPSPDFViewController
	{
		private Book book;
		private CustomPSPDFBarButtonItem CollectionMenuItem;
		private CustomPSPDFBarButtonItem PageModeItem;
		private CustomPSPDFBarButtonItem AnnButtonItem;
		private CustomPSPDFBarButtonItem NoteItem;
		private CustomPSPDFBarButtonItem contentSyncErrorButton;
		private CustomPSPDFBarButtonItem printBarButton;
		private UIPopoverController collectionViewController;
		private UIPopoverController penViewController;
		private UIPopoverController pageModeViewController;
		private UIButton transButton;
		private NotePanel notePanel;
		private PrintPanel printPanel;
		private AnnotationToolBar annotationView;
		private UICollectionView collectionView;
		private bool zoomed = false;
		private static nint COLLECTIONVIEW_HEIGHT = 200;
		private UIPopoverController contentSyncViewController;

		public PageViewController(Book book, PSPDFDocument document, PSPDFConfiguration configuration) : base(book, document, configuration)
		{
			this.book = book;
		}

		public override void DidReceiveMemoryWarning()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning();
            
//			this.InvokeOnMainThread(delegate
//			{
//				// Release any cached data, images, etc that aren't in use.
//				MessageBox alert = new MessageBox();
//				alert.ShowAlert("Alert", "Received Memory Warning", "Ok");
//
//			});
		}
				
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
            
			// Perform any additional setup after loading the view, typically from a nib.
			this.View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle("Assets/Backgrounds/page.png"));

			if (this.NavigationController != null)
			{
				this.NavigationController.Toolbar.BarTintColor = UIColor.White;
				this.NavigationController.View.TintColor = UIColor.White;
				this.NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes() { ForegroundColor = UIColor.White };
				this.NavigationController.NavigationBar.TintColor = UIColor.White;
				this.NavigationItem.Title = book.Title;

				// CollectionMenuItem
				CollectionMenuItem = new CustomPSPDFBarButtonItem(this);
				CollectionMenuItem.BarButton.Frame = new CGRect(0, 0, 49, 49);
				CollectionMenuItem.UpdateButtonImage(UIImage.FromBundle("Assets/Buttons/menu.png").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate));
				CollectionMenuItem.ButtonClickedEvent += HandleCollectionMenuButtonTouchUpInside;

				// PageModeItem
				PageModeItem = new CustomPSPDFBarButtonItem(this);
				PageModeItem.BarButton.Frame = new CGRect(0, 0, 49, 49);
				PageModeItem.ButtonClickedEvent += HandlePageModeButtonTouchUpInside;
				UpdatePageModeIcon();

				// AnnButtonItem
				AnnButtonItem = new CustomPSPDFBarButtonItem(this);
				AnnButtonItem.BarButton.Frame = new CGRect(0, 0, 49, 49);
				AnnButtonItem.UpdateButtonImage(UIImage.FromBundle("Assets/Buttons/pen.png").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate));
				AnnButtonItem.ButtonClickedEvent += HandleAnnButtonTouchUpInside;

				// NoteItem
				NoteItem = new CustomPSPDFBarButtonItem(this);
				NoteItem.BarButton.Frame = new CGRect(0, 0, 49, 49);
				NoteItem.UpdateButtonImage(UIImage.FromBundle("Assets/Buttons/note.png").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate));
				NoteItem.ButtonClickedEvent += HandleNoteButtonTouchUpInside;

				//printButton
				UIImage image = UIImage.FromBundle("Assets/Buttons/print.png");
				image = image.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);

				var printButton= new UIButton(UIButtonType.Custom);
				printButton.SetImage (image, UIControlState.Normal);
				printButton.TintColor = UIColor.White;
				printButton.TouchUpInside += HandlePrintButtonTouchUpInside;
				printButton.Frame = new CGRect (0, 0, printButton.CurrentImage.Size.Width, printButton.CurrentImage.Size.Height);
				printBarButton = new CustomPSPDFBarButtonItem(this);
				printBarButton.BarButton = printButton;

				//contentSyncButton
				var contentSyncButton= new UIButton(UIButtonType.Custom);
				contentSyncButton.SetImage (UIImage.FromBundle ("Assets/Buttons/icn_contentSyncError.png"), UIControlState.Normal);
				contentSyncButton.TouchUpInside += HandleContentSyncErrorButtonTouchUpInside;
				contentSyncButton.Frame = new CGRect (0, 0, contentSyncButton.CurrentImage.Size.Width, contentSyncButton.CurrentImage.Size.Height);
				contentSyncErrorButton = new CustomPSPDFBarButtonItem(this);
				contentSyncErrorButton.BarButton = contentSyncButton;

				// TopBar
				if (!URL.ServerURL.Contains (StringRef.DemoURL) )
				{
					this.RightBarButtonItems = new PSPDFBarButtonItem[] {
						contentSyncErrorButton,
						SearchButtonItem,
						printBarButton,
						ViewModeButtonItem
					};
				}
				else
				{
					this.RightBarButtonItems = new PSPDFBarButtonItem[] {
						SearchButtonItem,
						printBarButton,
						ViewModeButtonItem
					};
				}

				// BottomBar
				UIBarButtonItem flexSpace = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
				this.SetToolbarItems(new UIBarButtonItem[] {
					CollectionMenuItem,
					flexSpace,
					PageModeItem,
					flexSpace,
					AnnButtonItem,
					flexSpace,
					NoteItem,
				}, true);

				// Update bookmark riboons in thumbnailview
				UpdateDocumentBookmarkRibbons();

				// controllerDelegate
				CustomPSPDFViewControllerDelegate del = ((CustomPSPDFViewControllerDelegate)this.Delegate);
				del.DidShowPageViewEvent += HandleDidShowPageViewEvent;
				del.DidChangeViewModeEvent += HandleDidChangeViewModeEvent;
				del.DidEndPageZoomingEvent += HandleDidEndPageZoomingEvent;
				del.DidRenderPageViewEvent += HandleDidRenderPageViewEvent;
				del.DidShowHideHudEvent += HandleDidShowHideHudEvent;
				del.ShouldShowHudEvent += HandleShouldShowHudEvent;
				del.ShouldHideHudEvent += HandleShouldHideHudEvent;
				del.SaveAnnotationEvent += HandleSaveAnnotationEvent;
			}
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			BookUpdater.RegisterASIDelegate(this);

			this.NavigationController.InteractivePopGestureRecognizer.Enabled = false;
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

			if (Settings.OpenNotePanel)
			{
				Settings.OpenNotePanel = false;

				ShowNotePanel();
			}

			// PSPDFKit does not call this callback event on the first page due to bug
			HandleDidRenderPageViewEvent(this.PageViewForPage(this.Page));
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			DisposeNotePanel();
			DisposeAnnotationView();
		}

		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate(toInterfaceOrientation, duration);

			// Reload the notePanel;
			if (notePanel != null)
			{
				notePanel.UpdateContentSize();
			}
		}

		[Export("requestDidFail:")]
		protected virtual void RequestDidFail(NSObject sender)
		{
			BookUpdater.RequestDidFail(sender);
		}

		[Export("requestDidFinish:")]
		protected virtual void RequestDidFinish(NSObject sender)
		{
			BookUpdater.RequestDidFinish(sender);
		}

		[Export("queueDidFinish:")]
		protected virtual void QueueDidFinish(NSObject sender)
		{
			BookUpdater.QueueDidFinish(sender);
		}

		private void ShowHideBottomToolBar(bool show)
		{
			if (show)
			{
				this.NavigationController.SetToolbarHidden(false, true);
			}
			else
			{
				this.NavigationController.SetToolbarHidden(true, true);
			}
		}

		void HandleContentSyncErrorButtonTouchUpInside(object sender, EventArgs args)
		{
			HidePopover();

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
			HidePopover();
			if ( Reachability.IsDefaultNetworkAvailable () )
			{
				await OpenSyncView ();
			}
			else
			{
				AlertView.Show(StringRef.connectionFailure, StringRef.connectionRequired, StringRef.ok);
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
	
		private void UpdatePageModeIcon()
		{
			PSPDFPageMode pageMode;

			if (Settings.PageMode == StringRef.Single)
			{
				PageModeItem.UpdateButtonImage(UIImage.FromBundle("Assets/Buttons/page_single.png").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate));
				pageMode = PSPDFPageMode.Single;
			}
			else
			{
				HideNotePanel();

				PageModeItem.UpdateButtonImage(UIImage.FromBundle("Assets/Buttons/page_double.png").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate));
				pageMode = PSPDFPageMode.Double;
			}

			this.UpdateConfiguration(delegate(PSPDFConfigurationBuilder builder)
			{
				builder.RenderingMode = PSPDFPageRenderingMode.FullPageBlocking;
				builder.PageMode = pageMode;
				builder.PageTransition = PSPDFPageTransition.Curl;
			});
		}

		private void HidePopover(String panel = "")
		{
			if (this.PopoverController != null)
			{
				this.PopoverController.Dismiss(true);
			}

			if (pageModeViewController != null)
			{
				pageModeViewController.Dismiss(true);
				pageModeViewController = null;
			}

			if (collectionViewController != null)
			{
				collectionViewController.Dismiss(true);
				collectionViewController = null;
			}

			if (contentSyncViewController != null)
			{
				contentSyncViewController.Dismiss(true);
			}

			if (penViewController != null)
			{
				penViewController.Dismiss(true);
				penViewController = null;
			}

			if (penViewController != null)
			{
				penViewController.Dismiss(true);
				penViewController = null;
			}

			if (collectionView != null)
			{
				HideCollectionViewWithAnimation();
			}

			if (String.IsNullOrEmpty(panel))
			{
				HidePrintPanel();
				HideNotePanel();
			}
			else
			{
				if (panel == StringRef.Note)
				{
					HidePrintPanel();
				}
				else if (panel == StringRef.print)
				{
					HideNotePanel();
				}
			}
		}

		void HandleDidChangeViewModeEvent()
		{
			if (ViewMode == PSPDFViewMode.Document)
			{
				SetHudVisible(true, true);
				ShowHideBottomToolBar(true);
			}
			else
			{
				SetHudVisible(false, true);
				ShowHideBottomToolBar(false);
				HidePrintPanel();
				HideNotePanel();

				// Hide thumbnail view
				HideCollectionViewWithAnimation();
			}
		}

		void HandlePageModeButtonTouchUpInside()
		{
			HidePopover();
            
			if (pageModeViewController == null)
			{
				PopoverPageModeController pmc = new PopoverPageModeController();
				pmc.Title = StringRef.PageView;
				pmc.PageModeEvent += HandlePageModeEvent;
                
				UINavigationController navController = new UINavigationController();
				navController.SetViewControllers(new UIViewController[] { pmc }, true);
				navController.View.Frame = new CGRect(0, 0, 280, 44 * 3 + 20);
                
				pageModeViewController = new UIPopoverController(navController);
				pageModeViewController.SetPopoverContentSize(new CGSize(navController.View.Frame.Width, navController.View.Frame.Height), true);
				pageModeViewController.PresentFromBarButtonItem(PageModeItem, UIPopoverArrowDirection.Any, true);
				pageModeViewController.DidDismiss += delegate
				{
					pageModeViewController = null;
				};
			}
			else
			{
				pageModeViewController.Dismiss(true);
				pageModeViewController = null;
			}
		}

		void HandlePageModeEvent()
		{
			HidePopover();

			UpdatePageModeIcon();
		}

		#region Panel

		public void LoadPanel(String panelStr)
		{
			nfloat height = this.View.Bounds.Size.Height - this.NavigationController.NavigationBar.Frame.Bottom - 44;

			if (panelStr == "Note")
			{
				if (notePanel == null)
				{
					notePanel = new NotePanel(this, this.book, new CGRect(this.View.Bounds.Size.Width, this.NavigationController.NavigationBar.Frame.Bottom, 280, height));
					notePanel.CloseEvent += HideNotePanel;
					this.View.AddSubview(notePanel);
				}
			}
			else if (panelStr == "Print")
			{
				if (printPanel == null)
				{
					printPanel = new PrintPanel(this.book.ID, PrintButtonItem, new CGRect(this.View.Bounds.Size.Width, this.NavigationController.NavigationBar.Frame.Bottom, 280, height));
					printPanel.CloseEvent += HidePrintPanel;
					printPanel.EmailEvent += EmailPdfFile;
					this.View.AddSubview(printPanel);
				}
			}
		}

		private void ShowPanel(UIView panel)
		{
			if (Settings.PageMode == StringRef.Double)
			{
				Settings.WritePageMode(StringRef.Single);
				HandlePageModeEvent();
			}

			PSPDFPageView pageView = this.PageViewForPage(this.Page);
			if (pageView != null)
			{
				// Update
				Page page = BooksOnDeviceAccessor.GetPage(book.ID, (int)pageView.Page + 1);

				if (panel is NotePanel)
				{
					notePanel.LoadNoteListView(page.ID);
				}
				else if (panel is PrintPanel)
				{
					printPanel.UpdatePageID(page.ID);
				}

				UIView.Animate(0.3d, delegate
				{
					panel.Frame = new CGRect(this.View.Bounds.Size.Width - panel.Frame.Width, panel.Frame.Y, panel.Frame.Width, panel.Frame.Height);
				}, delegate
				{
					// Lock HUD
					((CustomPSPDFViewControllerDelegate)this.Delegate).Hud_Lock = true;

					this.UpdateConfigurationWithoutReloading(delegate(PSPDFConfigurationBuilder builder)
					{
						builder.ShouldHideHUDOnPageChange = false;
					});
				});
			}
		}

		private void HidePanel(UIView panel)
		{
			if (panel != null)
			{
				// Animate when hiding
				UIView.Animate(0.3d, delegate
				{
					panel.Frame = new CGRect(this.View.Bounds.Size.Width, panel.Frame.Y, panel.Frame.Width, panel.Frame.Height);
				}, delegate
				{
					// Unlock HUD
					((CustomPSPDFViewControllerDelegate)this.Delegate).Hud_Lock = false;

					this.UpdateConfigurationWithoutReloading(delegate(PSPDFConfigurationBuilder builder)
					{
						builder.ShouldHideHUDOnPageChange = true;
					});
				});
			}
		}

		private void DisposePanel(UIView panel)
		{
			if (panel != null)
			{
				panel.RemoveFromSuperview();
				panel.Dispose();
				panel = null;
			}
		}

		void PanelButtonTouchUpInside(UIView panel)
		{
			if (panel != null && panel.Frame.X < this.View.Bounds.Size.Width)
			{
				HidePanel(panel);
			}
			else
			{
				ShowPanel(panel);
			}
		}

		#endregion

		#region Print

		private void HidePrintPanel()
		{
			HidePanel(printPanel);
		}

		private void EmailPdfFile(string pdfFile)
		{
			// Give Feedback
			var mailer = new EmailComposer();

			if ( MFMailComposeViewController.CanSendMail )
			{

				string[] strArray= new String[3];
				strArray [0] = pdfFile;
				strArray [1] = "application/pdf";
				strArray [2] = "pdfFile.pdf";

				mailer.Attachments = strArray; 
				mailer.PresentViewController(this);
			}
			else
			{
				new UIAlertView ("No mail account", "Please set up a Mail account in order to send a mail.", null,"Ok", null).Show();
			}
		}

		private void DisposePrintPanel()
		{
			DisposePanel(printPanel);
		}

		void HandlePrintButtonTouchUpInside(object sender, EventArgs e)
		{
			HidePopover(StringRef.print);

			LoadPanel(StringRef.print);

			PanelButtonTouchUpInside(printPanel);
		}

		#endregion

		#region Note

		private void ShowNotePanel()
		{
			HandleNoteButtonTouchUpInside();
		}

		private void HideNotePanel()
		{
			HidePanel(notePanel);
		}

		private void DisposeNotePanel()
		{
			DisposePanel(notePanel);
		}

		void HandleDidShowPageViewEvent()
		{
			if (notePanel != null)
			{
				// Update notePanel
				Page page = BooksOnDeviceAccessor.GetPage(book.ID, (int)this.Page + 1);
				if (page != null)
				{
					notePanel.LoadNoteListView(page.ID);
				}
			}
		}

		void HandleNoteButtonTouchUpInside()
		{
			HidePopover(StringRef.Note);

			LoadPanel(StringRef.Note);

			PanelButtonTouchUpInside(notePanel);
		}

		#endregion

		#region CollectionView

		private void InitializeCollectionView()
		{
			// transButton
			transButton = new UIButton();
			transButton.AutoresizingMask = UIViewAutoresizing.All;
			transButton.Frame = new CGRect(0, 0, this.View.Frame.Width, this.View.Frame.Height);
			transButton.BackgroundColor = UIColor.Black;
			transButton.Alpha = 0f;
			transButton.TouchUpInside += HandleTransButtonTouchUpInside;
			this.View.AddSubview(transButton);

			// Set initial location to bottom of the screen
			UICollectionViewFlowLayout flowLayout = new UICollectionViewFlowLayout() {
				ScrollDirection = UIKit.UICollectionViewScrollDirection.Horizontal,
				ItemSize = new CGSize(150, 100),
				MinimumLineSpacing = 10.0f,
				SectionInset = new UIEdgeInsets(0, 20, 0, 20),
				HeaderReferenceSize = new CGSize(150, 200)
			};

			collectionView = new UICollectionView(new CGRect(0, this.View.Frame.Bottom, this.View.Frame.Width, COLLECTIONVIEW_HEIGHT), flowLayout);
			collectionView.BackgroundColor = UIColor.Black;
			collectionView.Alpha = 0.85f;
			collectionView.DecelerationRate = UIScrollView.DecelerationRateFast;
			collectionView.ShowsHorizontalScrollIndicator = false;
			collectionView.AlwaysBounceHorizontal = true;
			collectionView.RegisterClassForSupplementaryView(typeof(PageHeader), UICollectionElementKindSection.Header, new NSString("pageHeader"));
			collectionView.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin | UIViewAutoresizing.FlexibleWidth;
			this.View.AddSubview(collectionView);

			collectionView.BecomeFirstResponder();
		}

		private void ShowCollectionViewWithAnimation()
		{
			if (collectionView != null)
			{
				nfloat collectionViewY = this.NavigationController.Toolbar.Frame.Top - COLLECTIONVIEW_HEIGHT;

				UIView.Animate(0.15d, delegate
				{
					transButton.Alpha = 0.5f;

					collectionView.Frame = new CGRect(0, collectionViewY, this.View.Frame.Width, COLLECTIONVIEW_HEIGHT);
				});
			}
		}

		private void HideCollectionViewWithAnimation()
		{
			if (collectionView != null)
			{
				collectionView.ResignFirstResponder();

				UIView.Animate(0.15d, delegate
				{
					collectionView.Frame = new CGRect(0, this.View.Frame.Bottom, this.View.Frame.Width, COLLECTIONVIEW_HEIGHT);
				}, delegate
				{
					if (collectionView != null)
					{
						collectionView.RemoveFromSuperview();
						collectionView.Dispose();
						collectionView = null;
					}

					if (transButton != null)
					{
						transButton.RemoveFromSuperview();
						transButton.Dispose();
						transButton = null;
					} 
				});
			}
		}

		private void ShowCollectionView(NSString cellID, List<Page> pageList)
		{
			try
			{
				// collectionView
				InitializeCollectionView();
                
				CollectionDataSource dataSource = new CollectionDataSource(cellID, pageList);
				if (cellID.ToString().Contains("Note"))
				{
					dataSource.ItemPressedEvent += delegate(String pageID)
					{
						ShowNotePanel();

						HandleItemPressedEvent(pageID);
					};
				}
				else
				{
					dataSource.ItemPressedEvent += HandleItemPressedEvent;
				}
                
				collectionView.RegisterClassForCell(typeof(PageCell), cellID);
				collectionView.Source = dataSource;
				collectionView.ReloadData();
                
				// Show collection view with nice animation
				ShowCollectionViewWithAnimation();
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("PageViewController - ShowCollectionView: {0}", ex.ToString());
			}
		}

		private void ShowPageCollections()
		{
			try
			{
				List<Page> pageList = BooksOnDeviceAccessor.GetPages(book.ID);
				if (pageList != null)
				{
					ShowCollectionView(new NSString("PageCell"), pageList);
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("PageViewController - ShowPageCollections: {0}", ex.ToString());
			}
		}

		private void ShowTOCCollections()
		{
			try
			{
				List<Page> pageList = new List<Page>();
                
				List<Chapter> chapterList = BooksOnDeviceAccessor.GetChapters(book.ID);
				if (chapterList != null)
				{
					pageList = new List<Page>();
                    
					foreach (Chapter chapter in chapterList)
					{
						Page page = BooksOnDeviceAccessor.GetPage(book.ID, chapter.FirstPageID);
						if (page != null)
						{
							pageList.Add(page);
						}
					}
				}
                
				ShowCollectionView(new NSString("TOCCell"), pageList);
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("PageViewController - ShowTOCCollections: {0}", ex.ToString());
			}
		}

		private void ShowBookmarkCollections()
		{
			try
			{
				List<Page> pageList = new List<Page>();
                
				List<Bookmark> bookmarkList = BooksOnDeviceAccessor.GetBookmarks(book.ID);
				if (bookmarkList != null)
				{
					foreach (Bookmark bookmark in bookmarkList)
					{
						Page page = BooksOnDeviceAccessor.GetPage(bookmark.BookID, bookmark.PageID);
						pageList.Add(page);
					}
				}
                
				ShowCollectionView(new NSString("BookmarkCell"), pageList);
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("PageViewController - ShowBookmarkCollections: {0}", ex.ToString());
			}
		}

		private void ShowNoteCollections()
		{
			try
			{
				List<Page> pageList = new List<Page>();
                
				List<Note> noteList = BooksOnDeviceAccessor.GetNotes(book.ID);
				if (noteList != null)
				{
					pageList = new List<Page>();
                    
					foreach (Note note in noteList)
					{
						Page page = BooksOnDeviceAccessor.GetPage(note.BookID, note.PageID);
						pageList.Add(page);
					}
				}
                
				ShowCollectionView(new NSString("NoteCell"), pageList);
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("PageViewController - ShowNoteCollections: {0}", ex.ToString());
			}
		}

		private void ShowAnnotationCollections()
		{
			try
			{
				List<Page> pageList = new List<Page>();
                
				for (int i = 0; i < (int)this.Document.PageCount; i++)
				{
					Page page = BooksOnDeviceAccessor.GetPage(book.ID, (nint)i + 1);
					if (page != null)
					{
						Annotation annotation = AnnotationsDataAccessor.GetAnnotation(book.ID, page.ID);
						if (annotation != null)
						{
							pageList.Add(page);
						}
					}
				}
                
				ShowCollectionView(new NSString("AnnotationCell"), pageList);
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("PageViewController - ShowAnnotationCollections: {0}", ex.ToString());
			}
		}

		void HandleItemPressedEvent(String pageID)
		{
			int page = BooksOnDeviceAccessor.GetPage(book.ID, pageID).PageNumber - 1;
			this.SetPage((nuint)page, true);
		}

		void HandleTransButtonTouchUpInside(object sender, EventArgs e)
		{
			HideCollectionViewWithAnimation();
		}

		void HandleCollectionMenuButtonTouchUpInside()
		{
			HidePopover();
            
			if (collectionViewController == null)
			{
				PopoverCollectionController pcc = new PopoverCollectionController();
				pcc.Title = "Menu";
				pcc.RowSelectedEvent += HandleCollectionMenuButtonEvent;
                
				UINavigationController navController = new UINavigationController();
				navController.SetViewControllers(new UIViewController[] { pcc }, true);
				navController.View.Frame = new CGRect(0, 0, 280, 44 * 6 + 20);
                
				collectionViewController = new UIPopoverController(navController);
				collectionViewController.SetPopoverContentSize(new CGSize(navController.View.Frame.Width, navController.View.Frame.Height), true);
				collectionViewController.PresentFromBarButtonItem(CollectionMenuItem, UIPopoverArrowDirection.Any, true);
				collectionViewController.DidDismiss += delegate
				{
					collectionViewController = null;
				};
			}
			else
			{
				collectionViewController.Dismiss(true);
				collectionViewController = null;
			}
		}

		void HandleCollectionMenuButtonEvent(NSIndexPath indexPath)
		{
			HidePopover();

			if (indexPath.Row == 0)
			{
				ShowPageCollections();
			}
			else if (indexPath.Row == 1)
			{
				ShowTOCCollections();
			}
			else if (indexPath.Row == 2)
			{
				ShowBookmarkCollections();
			}
			else if (indexPath.Row == 3)
			{
				ShowNoteCollections();
			}
			else if (indexPath.Row == 4)
			{
				ShowAnnotationCollections();
			}
		}

		#endregion

		#region Annotation

		private void ShowAnnotationView()
		{
			this.ScrollingEnabled = false;

			if (annotationView != null)
			{
				// Make sure annotationView stays in bound
				UpdateToolViewLocation();

				SetHudVisible(false, true);

				// Animate when hiding
				UIView.Animate(0.3d, delegate
				{
					annotationView.Alpha = 1f;
				});
			}
		}

		private void HideAnnotationView()
		{
			this.ScrollingEnabled = true;

			if (annotationView != null)
			{
				SetHudVisible(true, true);

				// Animate when hiding
				UIView.Animate(0.3d, delegate
				{
					annotationView.Alpha = 0f;
				});
			}
		}

		private void UpdateToolViewLocation()
		{
			// left bound
			if (annotationView.Frame.Left < 0)
			{
				annotationView.Frame = new CGRect(0, annotationView.Frame.Y, annotationView.Frame.Width, annotationView.Frame.Height);
			}

			// right bound
			if (annotationView.Frame.Right > this.View.Frame.Width)
			{
				annotationView.Frame = new CGRect(this.View.Frame.Width - annotationView.Frame.Width, annotationView.Frame.Y, annotationView.Frame.Width, annotationView.Frame.Height);
			}

			// top bound
			if (annotationView.Frame.Top < 0)
			{
				annotationView.Frame = new CGRect(annotationView.Frame.X, 0, annotationView.Frame.Width, annotationView.Frame.Height);
			}

			// bottom bound
			if (annotationView.Frame.Bottom > this.View.Frame.Bottom)
			{
				annotationView.Frame = new CGRect(annotationView.Frame.X, this.View.Frame.Bottom - annotationView.Frame.Height, annotationView.Frame.Width, annotationView.Frame.Height);
			}

			this.View.BringSubviewToFront(annotationView);
		}

		private void DisposeAnnotationView()
		{
			if (annotationView != null)
			{
				annotationView.RemoveFromSuperview();
				annotationView.Dispose();
				annotationView = null;
			}
		}

		private UIColor GetAnnotationColor()
		{
			if (annotationView != null)
			{
				return annotationView.GetAnnotationColor();
			}

			return UIColor.Black;
		}

		private void EnterDrawingMode(UIColor color)
		{
			if (AnnotationStateManager.State != PSPDFAnnotationString.Ink)
			{
				AnnotationStateManager.ToggleState(PSPDFAnnotationString.Ink);
			}

			AnnotationStateManager.DrawColor = color;

			if (AnnotationStateManager.DrawColor.CGColor.Alpha < 1)
			{
				AnnotationStateManager.LineWidth = 20f;
			}
			else
			{
				AnnotationStateManager.LineWidth = 3f;
			}
		}

		void HandleAnnButtonTouchUpInside()
		{
			HidePopover();

			if (annotationView == null)
			{
				annotationView = new AnnotationToolBar(new CGRect(63, this.View.Center.Y - 170, 110, 354), this.View);
				annotationView.Alpha = 0f;
				annotationView.UpdateFrameEvent += HandleUpdateFrameEvent;
				annotationView.PenSelectedEvent += HandlePenSelectedEvent;
				annotationView.HighlighterSelectedEvent += HandleHighlighterSelectedEvent;
				annotationView.ColorSelectedEvent += HandleColorSelectedEvent;
				annotationView.UndoEvent += HandleUndoEvent;
				annotationView.RedoEvent += HandleRedoEvent;
				annotationView.CancelEvent += HandleCancelEvent;
				annotationView.DoneEvent += HandleDoneEvent;
				this.View.AddSubview(annotationView);
			}

			ShowAnnotationView();

			// Select pen mode as default
			annotationView.SelectPen();
		}

		void HandleUpdateFrameEvent(CGRect frame)
		{
			annotationView.Frame = frame;
		}

		void HandlePenSelectedEvent()
		{
			EnterDrawingMode(GetAnnotationColor());
		}

		void HandleHighlighterSelectedEvent()
		{
			EnterDrawingMode(GetAnnotationColor().ColorWithAlpha(0.5f));
		}

		void HandleColorSelectedEvent()
		{
			if (AnnotationStateManager.DrawColor.CGColor.Alpha < 1)
			{
				AnnotationStateManager.DrawColor = GetAnnotationColor().ColorWithAlpha(0.5f);
			}
			else
			{
				AnnotationStateManager.DrawColor = GetAnnotationColor();
			}
		}

		void HandleUndoEvent()
		{
			AnnotationStateManager.Undo();
		}

		void HandleRedoEvent()
		{
			AnnotationStateManager.Redo();
		}

		void HandleCancelEvent()
		{
			AnnotationStateManager.CancelDrawing(true);
			AnnotationStateManager.State = new NSString(String.Empty);

			// Hide annotationView
			HideAnnotationView();
		}

		void HandleDoneEvent()
		{
			AnnotationStateManager.FinishDrawing(false, true);
			AnnotationStateManager.State = new NSString(String.Empty);

			// Hide annotationView
			HideAnnotationView();

			// Save annotations
			SaveAnnotations();
		}

		void HandleSaveAnnotationEvent()
		{
			SaveAnnotations();
		}

		#endregion

		#region Bookmark

		private void AddBookmarkToBookmarkParser(nuint page)
		{
			this.Document.BookmarkParser.AddBookmark(page);

			SaveBookmark();
		}

		private void RemoveBookmarkFromBookmarkParser(nuint page)
		{
			this.Document.BookmarkParser.RemoveBookmark(page);

			SaveBookmark();
		}

		private void SaveBookmark()
		{
			NSError error = null;
			this.Document.BookmarkParser.SaveBookmarks(out error);
		}

		private void UpdateDocumentBookmarkRibbons()
		{
			List<Bookmark> bookmarkList = BooksOnDeviceAccessor.GetBookmarks(book.ID);
			if (bookmarkList != null)
			{
				// Clear all the bookmarks first
				NSError error;
				this.Document.BookmarkParser.ClearAllBookmarks(out error);

				// Add bookmarks
				foreach (Bookmark bookmark in bookmarkList)
				{
					Page page = BooksOnDeviceAccessor.GetPage(bookmark.BookID, bookmark.PageID);
					AddBookmarkToBookmarkParser((UInt32)page.PageNumber - 1);
				}
			}
		}

		public void UpdateBookmarkLocation(UIButton bookmarkButton)
		{
			if (!zoomed)
			{
				UIView.Animate(0.3d, delegate
				{
					if (this.NavigationBarHidden)
					{
						if (bookmarkButton != null)
						{
							bookmarkButton.Frame = new CGRect(bookmarkButton.Frame.X, 0, bookmarkButton.Frame.Width, bookmarkButton.Frame.Height);
						}
					}
					else
					{
						if (bookmarkButton != null && this.NavigationController != null)
						{
							CGRect buttonFrame = bookmarkButton.ConvertRectToView(bookmarkButton.Frame, this.View);
							nfloat diff = this.NavigationController.NavigationBar.Frame.Bottom - buttonFrame.Y;
							if (diff > 0)
							{
								bookmarkButton.Frame = new CGRect(bookmarkButton.Frame.X, diff, bookmarkButton.Frame.Width, bookmarkButton.Frame.Height);
							}
						}
					}
				});
			}
		}

		private UIButton GenerateBookmarkButton(PSPDFPageView pageView, Page page, Bookmark bookmark)
		{
			UIButton button = UIButton.FromType(UIButtonType.Custom);
            
			// Image
			if (bookmark == null)
			{
				button.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/bookmark_add.png"), UIControlState.Normal);
				button.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/bookmark_add.png"), UIControlState.Highlighted);
			}
			else
			{
				button.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/bookmark_solid.png"), UIControlState.Normal);
				button.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/bookmark_solid.png"), UIControlState.Highlighted);
			}

			button.Frame = new CGRect(30, 0, button.CurrentBackgroundImage.Size.Width, button.CurrentBackgroundImage.Size.Height);
            
			// Action
			button.TouchUpInside += (object sender, EventArgs e) =>
			{
				UIAlertView alertView = new UIAlertView("Bookmark", "Please enter bookmark title and press Save.", null, StringRef.cancel, "Save");
				alertView.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
				alertView.GetTextField(0).ReturnKeyType = UIReturnKeyType.Done;
				alertView.GetTextField(0).Placeholder = "Bookmark Title";
                
				// Show bookmark title if already exist
				bookmark = BooksOnDeviceAccessor.GetBookmark(book.ID, page.ID);
				if (bookmark != null)
				{
					alertView.GetTextField(0).Text = bookmark.Title;
				}
                
				alertView.Dismissed += delegate(object sender1, UIButtonEventArgs e1)
				{
					if (e1.ButtonIndex == 1)
					{
						String text = alertView.GetTextField(0).Text;
						BookmarkUpdater.SaveBookmark(book, page.ID, text);
                        
						if (String.IsNullOrEmpty(text))
						{
							RemoveBookmarkFromBookmarkParser(this.Page);
                            
							button.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/bookmark_add.png"), UIControlState.Normal);
							button.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/bookmark_add.png"), UIControlState.Highlighted);
						}
						else
						{
							AddBookmarkToBookmarkParser(this.Page);
                            
							button.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/bookmark_solid.png"), UIControlState.Normal);
							button.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/bookmark_solid.png"), UIControlState.Highlighted);
						}
					}
				};
				alertView.WillDismiss += delegate
				{
					alertView.GetTextField(0).ResignFirstResponder();
				};
				alertView.Show();
			};
            
			return button;
		}

		void HandleShouldShowHudEvent()
		{
			this.NavigationController.SetToolbarHidden(false, true);
		}

		void HandleShouldHideHudEvent()
		{
			this.NavigationController.SetToolbarHidden(true, true);
		}

		void HandleDidShowHideHudEvent()
		{
			UIButton bookmarkButton = null;
			PSPDFPageView pageView = this.PageViewForPage(this.Page);
			foreach (UIView subview in pageView)
			{
				if (subview is UIButton && subview.Tag == -1)
				{
					bookmarkButton = subview as UIButton;
					break;
				}
			}

			UpdateBookmarkLocation(bookmarkButton);
		}

		void HandleDidRenderPageViewEvent(PSPDFPageView pageView)
		{
			Page page = BooksOnDeviceAccessor.GetPage(book.ID, (int)pageView.Page + 1);
			if (page != null)
			{
				// Set current page id for Dashboard menu button action
				Settings.CurrentPageID = page.ID;

				// Clear the pageview
				foreach (UIView subview in pageView)
				{
					if (subview.Tag == -1)
					{
						subview.RemoveFromSuperview();
						break;
					}
				}

				// bookmark
				Bookmark bookmark = BooksOnDeviceAccessor.GetBookmark(book.ID, page.ID);

				// bookmarkButton
				UIButton bookmarkButton = GenerateBookmarkButton(pageView, page, bookmark);
				bookmarkButton.Tag = -1;
				pageView.AddSubview(bookmarkButton);

				UpdateBookmarkLocation(bookmarkButton);
			}
		}

		void HandleDidEndPageZoomingEvent(nfloat scale)
		{
			UIButton bookmarkButton = null;
			PSPDFPageView pageView = this.PageViewForPage(this.Page);

			if ( pageView != null )
			{
				foreach (UIView subview in pageView)
				{
					if ( subview is UIButton && subview.Tag == -1 )
					{
						bookmarkButton = subview as UIButton;
						break;
					}
				}

				if ( scale == 1f )
				{
					zoomed = false;

					UpdateBookmarkLocation (bookmarkButton);

					UIView.Animate (0.3d, delegate
					{
						if ( bookmarkButton != null )
						{
							bookmarkButton.Alpha = 1;
						}
					});
				}
				else
				{
					zoomed = true;

					UIView.Animate (0.3d, delegate
					{
						if ( bookmarkButton != null )
						{
							bookmarkButton.Alpha = 0;
						}
					});
				}
			}
		}

		#endregion
	}
}

