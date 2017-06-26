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
using PSPDFKit;
using System.Threading.Tasks;
using MssFramework;

namespace eBriefingMobile
{
	public class PrintPanel : UIView
	{
		private String bookID;
		private String pageID;
		private PrintHelper printHelper;
		private PSPDFPrintBarButtonItem barButton;
		private PrintDataSource dataSource;

		public PrintHelper.ORIENTATION Orientation { get; set; }

		public PrintHelper.RANGE Range { get; set; }

		public PrintHelper.ANNOTATION Annotation { get; set; }

		public PrintHelper.NOTE Note { get; set; }

		public nuint StartPage { get; set; }

		public nuint EndPage { get; set; }

		public delegate void PrintPanelDelegate();

		public event PrintPanelDelegate CloseEvent;

		public delegate void PrintEmailDelegate(string data);

		public event PrintEmailDelegate EmailEvent;

		public PrintPanel(String bookID, PSPDFPrintBarButtonItem barButton, CGRect frame) : base(frame)
		{
			this.BackgroundColor = eBriefingAppearance.Gray5;
			this.bookID = bookID;
			this.barButton = barButton;
			this.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleHeight;

			Range = PrintHelper.RANGE.CURRENT;
			Orientation = PrintHelper.ORIENTATION.PORTRAIT;
			Annotation = PrintHelper.ANNOTATION.WITHOUT;
			Note = PrintHelper.NOTE.WITHOUT;

			// navBar
			UINavigationBar navBar = new UINavigationBar();
			navBar.Frame = new CGRect(0, 0, this.Frame.Width, 44);
			this.AddSubview(navBar);

			// closeButton
			UIBarButtonItem closeButton = new UIBarButtonItem("Close", UIBarButtonItemStyle.Plain, HandleCloseTouchUpInside);
			closeButton.TintColor = eBriefingAppearance.BlueColor;

			UINavigationItem item = new UINavigationItem();
			item.RightBarButtonItem = closeButton;
			item.Title = "Print Options";
			navBar.PushNavigationItem(item, false);

			UIStringAttributes stringAttributes = new UIStringAttributes();
			stringAttributes.StrokeColor = eBriefingAppearance.Gray3;
			stringAttributes.Font = eBriefingAppearance.ThemeRegularFont(17f);
			navBar.TitleTextAttributes = stringAttributes;

			// tableView
			UITableView tableView = new UITableView(new CGRect(0, navBar.Frame.Bottom + 1, this.Frame.Width, this.Frame.Height - navBar.Frame.Bottom - 1), UITableViewStyle.Grouped);
			tableView.BackgroundColor = UIColor.Clear;
			dataSource = new PrintDataSource(this);
			tableView.Source = dataSource;
			this.AddSubview(tableView);

			tableView.LayoutIfNeeded();
			tableView.Frame = new CGRect(tableView.Frame.X, tableView.Frame.Y, tableView.Frame.Width, tableView.ContentSize.Height);

			// printView
			UIView printView = new UIView(new CGRect(0, tableView.Frame.Bottom + 30, this.Frame.Width, 44));
			printView.BackgroundColor = UIColor.White;
			this.AddSubview(printView);

			// printButton
			UIButton printButton = UIButton.FromType(UIButtonType.Custom);
			printButton.Frame = new CGRect(0, 0, printView.Frame.Width, printView.Frame.Height);
			printButton.Font = eBriefingAppearance.ThemeRegularFont(17);
			printButton.SetTitle(StringRef.print, UIControlState.Normal);
			printButton.SetTitleColor(eBriefingAppearance.BlueColor, UIControlState.Normal);
			printButton.TouchUpInside += HandlePrintTouchUpInside;
			printView.AddSubview(printButton);

			// emailButton
//			UIButton emailButton = UIButton.FromType(UIButtonType.Custom);
//			emailButton.Frame = new CGRect(0,printView.Frame.Bottom+30 , printView.Frame.Width, printView.Frame.Height);
//			emailButton.Font = eBriefingAppearance.ThemeRegularFont(17);
//			emailButton.BackgroundColor = UIColor.White;
//			emailButton.SetTitle("Email", UIControlState.Normal);
//			emailButton.SetTitleColor(eBriefingAppearance.BlueColor, UIControlState.Normal);
//			emailButton.TouchUpInside +=  (object sender, EventArgs e) => 
//			{
//				 PrintSetup (true);
//			};
//
//			this.AddSubview(emailButton);
		}

		public void UpdatePageID(String pageID)
		{
			this.pageID = pageID;
		}

		async void HandlePrintTouchUpInside(object sender, EventArgs e)
		{
			PrintSetup (false);
		}

		async private void PrintSetup(bool bGeneratePdfFile)
		{
			List<Page> pageList = new List<Page>();
			if (Range == PrintHelper.RANGE.ALL)
			{
				pageList = BooksOnDeviceAccessor.GetPages(bookID);
			}
			else if (Range == PrintHelper.RANGE.CURRENT)
			{
				pageList.Add(BooksOnDeviceAccessor.GetPage(bookID, pageID));
			}
			else
			{
				if ( StartPage == 0 && EndPage == 0 )
				{
					new UIAlertView (StringRef.alert, "Please enter Start page number & End page number.", null, StringRef.ok, null).Show ();
					return;
				}
				else if ( StartPage > EndPage )
				{
					new UIAlertView (StringRef.alert, "Start page number must be less than the End page number.", null, StringRef.ok, null).Show ();
					return;
				}
				else
				{
					for (int i = (int)StartPage; i <= (int)EndPage; i++)
					{
						Page page = BooksOnDeviceAccessor.GetPage (bookID, i);
						if ( page == null )
						{
							new UIAlertView (StringRef.alert, "Page number " + i.ToString () + " is not available. Please check page range. Print operation aborted.", null, StringRef.ok, null).Show ();

							return;
						}
						else
						{
							pageList.Add (BooksOnDeviceAccessor.GetPage (bookID, i));
						}
					}			
				}
			}

			printHelper = new PrintHelper(bookID, pageList);
			printHelper.Orientation = Orientation;
			printHelper.Range = Range;
			printHelper.Annotation = Annotation;
			printHelper.Note = Note;

			LoadingView.Show("Loading", "Getting ready to print...", false);

			string data =null;

			// Generate pdf images
			var isSuccessful= await eBriefingService.Run(() => printHelper.Generate());

			if ( isSuccessful )
			{
				this.InvokeOnMainThread (delegate
				{
					data = printHelper.GeneratePageRender (bGeneratePdfFile);

					if ( data != null )
					{
						if ( EmailEvent != null )
						{
							EmailEvent (data);
						}
					}
				});

				LoadingView.Hide ();

				// Close panel
				ClosePanel ();

				if ( data == null )
				{
					printHelper.Print (barButton);
				}
			}
			else
			{
				LoadingView.Hide ();

				MessageBox alert = new MessageBox();
				if (  printHelper.PagesNum!=null && printHelper.PagesNum > 0 )
				{
					alert.ShowAlert ("Error", string.Format("eBriefing is reaching its memory limit. At this level, you can print {0} number of pages. Please reduce your print range.",printHelper.PagesNum), "Ok");
				}
				else
				{
					alert.ShowAlert ("Error", "Please try again", "Ok");

					// Close panel
					ClosePanel ();
				}
			}
		}

		private void ClosePanel()
		{
			if (CloseEvent != null)
			{
				CloseEvent();
			}
		}

		void HandleCloseTouchUpInside(object sender, EventArgs e)
		{
			ClosePanel();

			if ( dataSource != null )
			{
				dataSource.DismissKeyboard ();
			}
		}
	}
}

