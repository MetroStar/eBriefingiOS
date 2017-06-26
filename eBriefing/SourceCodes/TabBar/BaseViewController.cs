using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreGraphics;
using ObjCRuntime;
using Metrostar.Mobile.Framework;
using MssFramework;

namespace eBriefingMobile
{
	public partial class BaseViewController : DispatcherViewController
	{
		protected bool downloadAnimation = false;
		protected UILabel statusLabel;
		protected UICollectionView collectionView;

		public virtual event EventHandler RefreshEvent;
		public virtual event EventHandler UpdateMyBooksBadgeEvent;
		public virtual event EventHandler ClearUpdatesBadgeEvent;
		public virtual event EventHandler UpdateUpdatesBadgeEvent;
		public virtual event EventHandler UpdateAvailableBadgeEvent;
		public virtual event EventHandler UpdateTabLocationEvent;

		public BaseViewController()
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
			InitializeStatusLabel();
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			// Hide collectionView and statusLabel before showing
			if (collectionView != null)
			{
				collectionView.Hidden = true;
			}

			if (statusLabel != null)
			{
				statusLabel.Hidden = true;
			}
		}

		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();

			// Update collectionView layout
			if (!downloadAnimation)
			{
				if (!collectionView.Hidden)
				{
					collectionView.SetNeedsLayout();
				}
			}

			// Update background image
			if (InterfaceOrientation == UIInterfaceOrientation.Portrait || InterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown)
			{
				this.View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle("Assets/Backgrounds/background_portrait.png"));
			}
			else
			{
				this.View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle("Assets/Backgrounds/background_landscape.png"));
			}
		}

		protected virtual void UpdateStatusLabel()
		{
			ShowHideStatusLabel(true);
		}

		protected virtual void UpdateMyBooksBadge()
		{
			if (UpdateMyBooksBadgeEvent != null)
			{
				UpdateMyBooksBadgeEvent(this, EventArgs.Empty);
			}
		}

		protected virtual void ClearUpdatesBadge()
		{
			if (ClearUpdatesBadgeEvent != null)
			{
				ClearUpdatesBadgeEvent(this, EventArgs.Empty);
			}
		}

		protected virtual void UpdateUpdatesBadge()
		{
			if (UpdateUpdatesBadgeEvent != null)
			{
				UpdateUpdatesBadgeEvent(this, EventArgs.Empty);
			}
		}

		protected virtual void UpdateAvailableBadge()
		{
			if (UpdateAvailableBadgeEvent != null)
			{
				UpdateAvailableBadgeEvent(this, EventArgs.Empty);
			}
		}

		protected virtual void UpdateTabLocation()
		{
			if (UpdateTabLocationEvent != null)
			{
				UpdateTabLocationEvent(this, EventArgs.Empty);
			}
		}

		protected virtual void InitializeCollectionView(String cellID)
		{
			DashboardLayout layout = new DashboardLayout() {
				SectionInset = new UIEdgeInsets(30, 32, 30, 32),
				MinimumInteritemSpacing = 30,
				MinimumLineSpacing = 40
			};

			collectionView = new UICollectionView(new CGRect(20, 0, this.View.Frame.Width - 40, this.View.Frame.Height), layout);

			if (String.Equals(cellID, StringRef.LibraryCell))
			{
				collectionView.RegisterClassForCell(typeof(LibraryCell), new NSString(cellID));
			}
			else
			{
				collectionView.RegisterClassForCell(typeof(BookshelfCell), new NSString(cellID));
			}

			collectionView.Source = null;
			collectionView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			collectionView.BackgroundColor = UIColor.Clear;
			collectionView.ShowsVerticalScrollIndicator = false;
			collectionView.AlwaysBounceVertical = true;
			collectionView.Hidden = true;
			this.View.AddSubview(collectionView);
		}

		protected virtual void RefreshTable()
		{
			if (RefreshEvent != null)
			{
				RefreshEvent(this, EventArgs.Empty);
			}
		}

		protected virtual void ShowHideStatusLabel(bool show)
		{
			if (show)
			{
				statusLabel.SizeToFit();
				statusLabel.Center = this.View.Center;
			}

			statusLabel.Hidden = !show;
			collectionView.Hidden = show;
		}

		private void InitializeStatusLabel()
		{
			statusLabel = eBriefingAppearance.GenerateLabel(27);
			statusLabel.Frame = new CGRect(0, 0, this.View.Frame.Width - 80, 80);
			statusLabel.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;
			statusLabel.BackgroundColor = UIColor.Clear;
			statusLabel.TextAlignment = UITextAlignment.Center;
			statusLabel.Lines = 0;
			statusLabel.LineBreakMode = UILineBreakMode.WordWrap;
			statusLabel.Hidden = true;
			this.View.AddSubview(statusLabel);
		}
	}
}

