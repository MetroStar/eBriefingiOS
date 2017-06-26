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
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreGraphics;
using ObjCRuntime;
using Metrostar.Mobile.Framework;
using SpinKitBinding;

namespace eBriefingMobile
{
	public partial class BookOverviewController : DispatcherViewController
	{
		private bool isUpdate;
		private Book book;
		private UIImageView imageView;
		private RTSpinKitView spinner1;
		private RTSpinKitView spinner2;
		private UILabel titleLabel;
		private UILabel pageLabel;
		private UILabel dateLabel;
		private UIButton downloadButton;
		private UITableView tableView;
		private UITextView textView;
		private UISegmentedControl segmentedControl;
		private static nfloat FRAME_WIDTH = 646;
		private static nfloat FRAME_HEIGHT = 449;

		public delegate void BookOverviewDelegate(Book book);

		public event BookOverviewDelegate DownloadEvent;

		public BookOverviewController(Book book, bool isUpdate)
		{
			this.book = book;
			this.isUpdate = isUpdate;
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

			// Initialize controls
			InitializeControls();

			// Retrieve chapters
			RetrieveChapters();
		}

		private void InitializeControls()
		{
			this.View.BackgroundColor = UIColor.Clear;

			this.View.Layer.ShadowPath = UIBezierPath.FromRoundedRect(this.View.Frame, 7f).CGPath;
			this.View.Layer.ShouldRasterize = true;
			this.View.Layer.RasterizationScale = UIScreen.MainScreen.Scale;

			// For rounded corner and shadow
			UIView subView = new UIView(new CGRect(0, 0, FRAME_WIDTH, FRAME_HEIGHT));
			subView.BackgroundColor = UIColor.White;
			subView.Layer.CornerRadius = 7f;
			subView.Layer.MasksToBounds = true;
			this.View.AddSubview(subView);

			// imageView
			imageView = new UIImageView();
			imageView.BackgroundColor = eBriefingAppearance.Gray3;
			imageView.Frame = new CGRect(20, 20, 280, 150);
			subView.AddSubview(imageView);

			// spinner1
			spinner1 = eBriefingAppearance.GenerateBounceSpinner();
			spinner1.Center = imageView.Center;
			subView.AddSubview(spinner1);

			// titleLabel
			titleLabel = eBriefingAppearance.GenerateLabel(21);
			titleLabel.Text = book.Title;
			titleLabel.Frame = new CGRect(319, 20, 307, 52);
			titleLabel.Lines = 0;
			titleLabel.SizeToFit();
			if (titleLabel.Frame.Height > 78f)
			{
				titleLabel.Frame = new CGRect(titleLabel.Frame.X, titleLabel.Frame.Y, titleLabel.Frame.Width, 78);
			}
			subView.AddSubview(titleLabel);

			// pageLabel
			pageLabel = eBriefingAppearance.GenerateLabel(14);
			pageLabel.Text = "Number of Pages : " + book.PageCount.ToString();
			pageLabel.Frame = new CGRect(titleLabel.Frame.X, titleLabel.Frame.Bottom + 8, 307, 21);
			subView.AddSubview(pageLabel);

			// dateLabel
			dateLabel = eBriefingAppearance.GenerateLabel(14);
			dateLabel.Text = "Last Modified : " + book.ServerModifiedDate.ToString();
			dateLabel.Frame = new CGRect(titleLabel.Frame.X, pageLabel.Frame.Bottom + 8, 307, 21);
			subView.AddSubview(dateLabel);

			// downloadButton
			downloadButton = UIButton.FromType(UIButtonType.Custom);
			downloadButton.Font = eBriefingAppearance.ThemeBoldFont(14);
			downloadButton.SetTitleColor(eBriefingAppearance.Color("37b878"), UIControlState.Normal);
			downloadButton.SetTitleColor(UIColor.White, UIControlState.Highlighted);
			downloadButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/green_unfilled.png").CreateResizableImage(new UIEdgeInsets(15f, 14f, 15f, 14f)), UIControlState.Normal);
			downloadButton.SetBackgroundImage(UIImage.FromBundle("Assets/Buttons/green_unfilled.png").CreateResizableImage(new UIEdgeInsets(15f, 14f, 15f, 14f)), UIControlState.Highlighted);
			downloadButton.Frame = new CGRect(titleLabel.Frame.X, 142, 130, downloadButton.CurrentBackgroundImage.Size.Height);
			if (isUpdate)
			{
				downloadButton.SetTitle("UPDATE", UIControlState.Normal);
			}
			else
			{
				downloadButton.SetTitle("DOWNLOAD", UIControlState.Normal);
			}
			downloadButton.TouchUpInside += HandleDownloadButtonTouchUpInside;
			subView.AddSubview(downloadButton);

			// segmentedControl
			segmentedControl = new UISegmentedControl(new object[] {
				"Description",
				"Chapters"
			});
			segmentedControl.Frame = new CGRect((FRAME_WIDTH / 2) - (226 / 2), imageView.Frame.Bottom + 20, 226, 29);
			segmentedControl.SelectedSegment = 0;
			segmentedControl.ValueChanged += HandleValueChanged;
			segmentedControl.TintColor = eBriefingAppearance.Color("848484");
			segmentedControl.Enabled = false;
			subView.AddSubview(segmentedControl);

			// line
			UIView line = new UIView();
			line.Frame = new CGRect(0, segmentedControl.Frame.Bottom + 19, FRAME_WIDTH, 1);
			line.BackgroundColor = eBriefingAppearance.Color("b4b4b4");
			subView.AddSubview(line);

			UIView subView2 = new UIView(new CGRect(0, line.Frame.Bottom, FRAME_WIDTH, FRAME_HEIGHT - line.Frame.Bottom));
			subView2.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle("Assets/Backgrounds/background_portrait.png"));
			subView.AddSubview(subView2);

			// textView
			textView = new UITextView();
			textView.Frame = new CGRect(0, 0, subView2.Frame.Width, subView2.Frame.Height);
			textView.Font = eBriefingAppearance.ThemeRegularFont(17);
			textView.Text = book.Description;
			textView.Editable = false;
			textView.BackgroundColor = UIColor.Clear;
			textView.TextContainerInset = new UIEdgeInsets(15, 10, 20, 10);
			textView.TextColor = eBriefingAppearance.Gray1;
			subView2.AddSubview(textView);

			// tableView
			tableView = new UITableView();
			tableView.Frame = textView.Frame;
			tableView.BackgroundColor = UIColor.Clear;
			tableView.ContentInset = new UIEdgeInsets(5, 0, 10, 0);
			tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			subView2.AddSubview(tableView);

			// spinner2
			spinner2 = eBriefingAppearance.GenerateBounceSpinner();
			spinner2.Center = tableView.Center;
			subView2.AddSubview(spinner2);
		}

		async private void RetrieveChapters()
		{
			// Update cover image
			UpdateImage();

			List<Chapter> chapterList = await eBriefingService.Run(() => eBriefingService.StartDownloadChapters(book.ID));
			if (chapterList != null)
			{
				// Save in the cache
				BooksOnServerAccessor.SaveChapters(book.ID, chapterList);

				// Update UI
				UpdateUI();

				// Enable segmentedControl
				segmentedControl.Enabled = true;
				spinner2.StopAnimating();
			}
		}

		public void UpdateCover(String url)
		{
			if (book.LargeImageURL == url)
			{
				UpdateImage();
			}
		}

		private void UpdateImage()
		{
			try
			{
				String localImagePath = DownloadedFilesCache.BuildCachedFilePath(book.LargeImageURL);
				if (File.Exists(localImagePath))
				{
					spinner1.StopAnimating();

					imageView.Alpha = 0f;
					imageView.Image = UIImage.FromFile(localImagePath);

					UIView.Animate(0.3d, delegate
					{
						imageView.Alpha = 1f;
					});
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("BookOverviewController - UpdateImage: {0}", ex.ToString());
			}
		}

		private void UpdateUI()
		{
			if (segmentedControl.SelectedSegment == 0)
			{
				tableView.Hidden = true;
				textView.Hidden = false;
			}
			else
			{
				textView.Hidden = true;
				tableView.Hidden = false;

				// tableView
				if (tableView.Source == null)
				{
					tableView.Source = new BookOverviewDataSource(BooksOnServerAccessor.GetChapters(book.ID));
					tableView.ReloadData();
				}
			}
		}

		void HandleDownloadButtonTouchUpInside(object sender, EventArgs e)
		{
			if (DownloadEvent != null)
			{
				DownloadEvent(book);
			}
		}

		void HandleValueChanged(object sender, EventArgs e)
		{
			UpdateUI();
		}
	}
}

