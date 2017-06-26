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
using MaryPopinBinding;
using PSPDFKit;

namespace eBriefingMobile
{
	public class NotePanel : UIView
	{
		private Book book;
		private NoteListView noteListView;
		private UIViewController parentVC;

		public delegate void NotePanelDelegate();

		public event NotePanelDelegate CloseEvent;

		public NotePanel(UIViewController parentVC, Book book, CGRect frame) : base(frame)
		{
			this.BackgroundColor = eBriefingAppearance.Gray5;
			this.parentVC = parentVC;
			this.book = book;

			this.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleHeight;

			// navBar
			UINavigationBar navBar = new UINavigationBar();
			navBar.Frame = new CGRect(0, 0, this.Frame.Width, 44);
			this.AddSubview(navBar);

			// closeButton
			UIBarButtonItem closeButton = new UIBarButtonItem("Close", UIBarButtonItemStyle.Plain, HandleCloseTouchUpInside);
			closeButton.TintColor = eBriefingAppearance.BlueColor;

			UINavigationItem item = new UINavigationItem();
			item.RightBarButtonItem = closeButton;
			item.Title = "Notes";
			navBar.PushNavigationItem(item, false);

			UIStringAttributes stringAttributes = new UIStringAttributes();
			stringAttributes.StrokeColor = eBriefingAppearance.Gray3;
			stringAttributes.Font = eBriefingAppearance.ThemeRegularFont(17f);
			navBar.TitleTextAttributes = stringAttributes;
		}

		public void LoadNoteListView(String pageID)
		{
			Reinitialize();

			noteListView = new NoteListView(book, pageID, new CGRect(0, 44, this.Frame.Width, this.Frame.Height - 44));
			noteListView.EditNoteEvent += (Note note) =>
			{
				NoteViewController nvc = null;
				if (note == null)
				{
					nvc = new NoteViewController(book, pageID);
				}
				else
				{
					nvc = new NoteViewController(book, pageID, note);
				}
				nvc.SetPopinTransitionStyle(BKTPopinTransitionStyle.SpringySlide);
				nvc.SetPopinOptions(BKTPopinOption.DisableAutoDismiss);
				nvc.SetPopinTransitionDirection(BKTPopinTransitionDirection.Top);
				nvc.CancelEvent += delegate
				{
					parentVC.DismissCurrentPopinControllerAnimated(true);

					noteListView.FlipBack();
				};
				nvc.AddNoteEvent += delegate(Note nt)
				{
					parentVC.DismissCurrentPopinControllerAnimated(true);

					noteListView.LoadNoteViews();
				};
				parentVC.PresentPopinController(nvc, true, null);
			};
			this.AddSubview(noteListView);
		}

		public void UpdateContentSize()
		{
			if (noteListView != null)
			{
				noteListView.UpdateContentSize();
			}
		}

		private void Reinitialize()
		{
			if (noteListView != null)
			{
				noteListView.RemoveFromSuperview();
				noteListView.Dispose();
				noteListView = null;
			}
		}

		void HandleCloseTouchUpInside(object sender, EventArgs e)
		{
			if (CloseEvent != null)
			{
				CloseEvent();
			}
		}
	}
}

