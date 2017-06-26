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

namespace eBriefingMobile
{
	public class NoteViewController : UIViewController
	{
		private bool edited;
		private Book book;
		private Note note;
		private String pageID;
		private UILabel limitLabel;
		private UITextView textView;
		private UIButton addButton;
		private static nint MAX_LENGTH = 500;

		public delegate void NoteViewDelegate0();

		public delegate void NoteViewDelegate1(Note note);

		public event NoteViewDelegate0 CancelEvent;
		public event NoteViewDelegate1 AddNoteEvent;

		public NoteViewController(Book book, String pageID)
		{
			this.book = book;
			this.pageID = pageID;
		}

		public NoteViewController(Book book, String pageID, Note note)
		{
			this.book = book;
			this.pageID = pageID;
			this.note = note;
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
			this.View.BackgroundColor = eBriefingAppearance.Gray5;
			this.View.Frame = new CGRect(0, 0, 600, 1);

			// textView
			textView = eBriefingAppearance.GenerateTextView(19f);
			textView.BackgroundColor = UIColor.White;
			textView.Frame = new CGRect(20, 20, this.View.Frame.Width - 40, 220);
			textView.ShouldChangeText += delegate (UITextView tv, NSRange range, String text)
			{
				edited = true;

//				if (text.Equals("\n"))
//				{
//					textView.ResignFirstResponder();
//					return false;
//				}
//				else
				{
					var newLength = textView.Text.Length + text.Length - range.Length;
					if (newLength <= MAX_LENGTH)
					{
						UpdateLimitText(MAX_LENGTH - newLength);
					}

					// Enable/Disable add button
					UpdateAddButton(newLength);

					return newLength <= MAX_LENGTH;
				}
			};
			this.View.AddSubview(textView);

			// limitLabel
			limitLabel = eBriefingAppearance.GenerateLabel(17);
			limitLabel.Frame = new CGRect(textView.Frame.X, textView.Frame.Bottom + 16, textView.Frame.Width, 21);
			this.View.AddSubview(limitLabel);

			// line
			UIView line = new UIView(new CGRect(limitLabel.Frame.X, limitLabel.Frame.Bottom + 16, limitLabel.Frame.Width, 1));
			line.BackgroundColor = eBriefingAppearance.Gray3;
			this.View.AddSubview(line);

			// addButton
			addButton = UIButton.FromType(UIButtonType.Custom);
			addButton.Frame = new CGRect(line.Frame.X, line.Frame.Bottom + 16, (this.View.Frame.Width / 2) - line.Frame.X - (line.Frame.X / 2), 37);
			addButton.Font = eBriefingAppearance.ThemeRegularFont(17);
			addButton.BackgroundColor = eBriefingAppearance.BlueColor;
			addButton.SetTitleColor(eBriefingAppearance.Gray3, UIControlState.Disabled);
			addButton.TouchUpInside += HandleAddTouchUpInside;
			this.View.AddSubview(addButton);

			// cancelButton
			UIButton cancelButton = UIButton.FromType(UIButtonType.Custom);
			cancelButton.Frame = new CGRect(addButton.Frame.Right + line.Frame.X, addButton.Frame.Y, addButton.Frame.Width, addButton.Frame.Height);
			cancelButton.Font = eBriefingAppearance.ThemeRegularFont(17);
			cancelButton.BackgroundColor = UIColor.White;
			cancelButton.SetTitle(StringRef.cancel, UIControlState.Normal);
			cancelButton.SetTitleColor(eBriefingAppearance.BlueColor, UIControlState.Normal);
			cancelButton.TouchUpInside += HandleCancelTouchUpInside;
			this.View.AddSubview(cancelButton);

			if (note == null)
			{
				addButton.SetTitle("Add Note", UIControlState.Normal);

				UpdateLimitText(MAX_LENGTH);
			}
			else
			{
				addButton.SetTitle("Update Note", UIControlState.Normal);
				textView.Text = note.Text;

				UpdateLimitText(MAX_LENGTH - note.Text.Length);
			}

			UpdateAddButton(textView.Text.Length);

			this.View.Frame = new CGRect(0, 0, this.View.Frame.Width, cancelButton.Frame.Bottom + 16);
		}

		private void UpdateAddButton(nint length)
		{
			if (length == 0)
			{
				addButton.Enabled = false;
				addButton.BackgroundColor = eBriefingAppearance.Gray4;
			}
			else
			{
				addButton.Enabled = true;
				addButton.BackgroundColor = eBriefingAppearance.BlueColor;
			}
		}

		private void UpdateLimitText(nint length)
		{
			limitLabel.Text = length.ToString() + " Characters Remaining";

			if (length == 0)
			{
				limitLabel.TextColor = eBriefingAppearance.RedColor;
			}
			else
			{
				limitLabel.TextColor = eBriefingAppearance.GreenColor;
			}
		}

		void HandleAddTouchUpInside(object sender, EventArgs e)
		{
			if (note == null)
			{
				note = NoteUpdater.SaveNote(book, pageID, textView.Text);
			}
			else
			{
				note = NoteUpdater.SaveNote(book, pageID, textView.Text, note.NoteID);
			}

			if (AddNoteEvent != null)
			{
				AddNoteEvent(note);
			}
		}

		void HandleCancelTouchUpInside(object sender, EventArgs e)
		{
			if (edited)
			{
				UIAlertView alert = new UIAlertView(StringRef.cancel, StringRef.cancelConfirm, null, StringRef.cancel, StringRef.ok);
				alert.Dismissed += (object sender1, UIButtonEventArgs e1) =>
				{
					if (e1.ButtonIndex == 1)
					{
						if (CancelEvent != null)
						{
							CancelEvent();
						}
					}
				};
				alert.Show();
			}
			else
			{
				if (CancelEvent != null)
				{
					CancelEvent();
				}
			}
		}
	}
}

