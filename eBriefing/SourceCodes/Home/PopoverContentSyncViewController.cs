using System;
using UIKit;

namespace eBriefingMobile
{
	public class PopoverContentSyncViewController:UIViewController
	{
		public delegate void PopoverContentSyncDelegate ();

		public event PopoverContentSyncDelegate SyncButtonTouch;

		public PopoverContentSyncViewController ()
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// Perform any additional setup after loading the view, typically from a nib.
			InitializeControls();
		}

		private void InitializeControls()
		{
			this.View.BackgroundColor=UIColor.White.ColorWithAlpha(0.8f);

			//title
			UILabel titleView = new UILabel();
			titleView.Frame = new CoreGraphics.CGRect (10, 10, 280, 21);
			titleView.Text = "You have not yet performed a full content sync";
			titleView.Font = eBriefingAppearance.ThemeBoldFont (20f);
			titleView.TextAlignment = UITextAlignment.Center;
			titleView.TextColor = UIColor.Black;
			titleView.LineBreakMode = UILineBreakMode.WordWrap;
			titleView.Lines = 0;
			titleView.SizeToFit ();
			titleView.Frame = new CoreGraphics.CGRect (10, 10, 280, titleView.Frame.Height);
			this.View.AddSubview (titleView);

			//text
			UILabel textView = new UILabel();
			textView.Frame = new CoreGraphics.CGRect (10, titleView.Frame.Bottom +5, 280, 21);
			textView.Text = "Please perform a sync before closing this app to update books and sync notes and annotations to the server.";
			textView.Font = eBriefingAppearance.ThemeRegularFont (16f);
			textView.TextColor = UIColor.Black;
			textView.TextAlignment = UITextAlignment.Center;
			textView.LineBreakMode = UILineBreakMode.WordWrap;
			textView.Lines = 0;
			textView.SizeToFit ();
			textView.Frame = new CoreGraphics.CGRect (10, titleView.Frame.Bottom +10, 280,textView.Frame.Height);
			this.View.AddSubview (textView);

			//separator
			var separatorView=new UIView();
			separatorView.Frame = new CoreGraphics.CGRect (0, textView.Frame.Bottom + 15, 300, 0.5f);
			separatorView.BackgroundColor = eBriefingAppearance.Gray5;
			this.View.AddSubview (separatorView);

			//sync button
			var syncButton=new UIButton(UIButtonType.Custom);
			syncButton.SetTitle ("Sync Now", UIControlState.Normal);
			syncButton.Font = eBriefingAppearance.ThemeBoldFont (18f);
			syncButton.SetTitleColor (eBriefingAppearance.Blue1Color, UIControlState.Normal);
			syncButton.Frame = new CoreGraphics.CGRect (0, separatorView.Frame.Bottom, 300, 50);
			syncButton.TouchUpInside += (object sender, EventArgs e) => 
			{
				if(SyncButtonTouch!=null)
				{
					SyncButtonTouch();
				}
			};
			this.View.AddSubview (syncButton);
		}
	}
}

