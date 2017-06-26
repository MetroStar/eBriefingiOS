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

namespace eBriefingMobile
{
	public class NoteListView : UIScrollView
	{
		private nfloat bottom;
		private Book book;
		private String pageID;
		private UIButton addButton;

		public delegate void NoteListViewDelegate(Note note);

		public event NoteListViewDelegate EditNoteEvent;

		public NoteListView(Book book, String pageID, CGRect frame) : base(frame)
		{
			this.BackgroundColor = UIColor.Clear;
			this.book = book;
			this.pageID = pageID;

			this.Bounces = true;
			this.ShowsVerticalScrollIndicator = true;
			this.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			// Load all notes
			LoadNoteViews();
		}

		public void LoadNoteViews()
		{
			List<Note> noteList = BooksOnDeviceAccessor.GetNotes(book.ID, pageID);

			// Remove everything first
			Reinitialize(noteList);

			// Then, start adding noteViews
			if (noteList != null && noteList.Count > 0)
			{
				// Sort by most recent
				noteList.Sort((x, y) => y.ModifiedUtc.CompareTo(x.ModifiedUtc));

				// Add noteView
				foreach (var note in noteList)
				{
					AddNoteView(note);
				}
			}
		}

		public void UpdateContentSize()
		{
			nfloat bottom = 0;
			foreach (var subview in this.Subviews)
			{
				if (subview is NoteView)
				{
					if (bottom <= subview.Frame.Bottom + 23)
					{
						bottom = subview.Frame.Bottom + 23;
					}
				}
			}

			this.ContentSize = new CGSize(1, bottom);
		}

		private void AddNoteView(Note note)
		{
			NoteView noteView = new NoteView(note, this.Frame.Width - 46);
			noteView.Frame = new CGRect(23, bottom, noteView.Frame.Width, noteView.TotalHeight);
			noteView.EditNoteEvent += HandleEditNoteEvent;
			noteView.RemoveNoteEvent += HandleRemoveNoteEvent;
			noteView.FlipBackEvent += HandleFlipBackEvent;
			this.AddSubview(noteView);

			bottom = noteView.Frame.Bottom + 23;
			this.ContentSize = new CGSize(1, bottom);
		}

		private void Reinitialize(List<Note> noteList)
		{
			foreach (var subview in this.Subviews)
			{
				subview.RemoveFromSuperview();
			}

			bottom = 0;

			if ((noteList == null || noteList.Count == 0) || !String.IsNullOrEmpty(URL.MultipleNoteURL))
			{
				UIView addView = new UIView(new CGRect(0, 0, this.Frame.Width, 61));
				addView.BackgroundColor = UIColor.White;
				addView.Layer.BorderColor = eBriefingAppearance.Gray5.CGColor;
				addView.Layer.BorderWidth = 1f;
				this.AddSubview(addView);

				// addButton
				addButton = UIButton.FromType(UIButtonType.Custom);
				addButton.Frame = new CGRect(23, 12, this.Frame.Width - 46, 37);
				addButton.Font = eBriefingAppearance.ThemeRegularFont(17);
				addButton.SetTitle("Add Note", UIControlState.Normal);
				addButton.SetTitleColor(eBriefingAppearance.BlueColor, UIControlState.Normal);
				addButton.SetImage(UIImage.FromBundle("Assets/Buttons/add_note.png"), UIControlState.Normal);
				addButton.Layer.BorderColor = eBriefingAppearance.Gray5.CGColor;
				addButton.Layer.BorderWidth = 1f;
				addButton.TouchUpInside += delegate
				{
					EditNote(pageID);
				};
				addView.AddSubview(addButton);

				bottom = addButton.Frame.Bottom;
			}

			bottom += 23;
		}

		private void EditNote(String pageID, Note note = null)
		{
			if (EditNoteEvent != null)
			{
				EditNoteEvent(note);
			}
		}

		void HandleEditNoteEvent(Note note)
		{
			if (EditNoteEvent != null)
			{
				EditNoteEvent(note);
			}
		}

		void HandleRemoveNoteEvent(Note note)
		{
			// Remove
			NoteUpdater.SaveNote(book, pageID, String.Empty, note.NoteID);

			LoadNoteViews();
		}

		void HandleFlipBackEvent(NoteView noteView)
		{
			var list = this.Subviews.Where(i => i is NoteView && i != noteView).ToList();

			Flip(list);
		}

		public void FlipBack()
		{
			var list = this.Subviews.Where(i => i is NoteView).ToList();

			Flip(list);
		}

		private void Flip(List<UIView> viewList)
		{
			if (viewList != null)
			{
				foreach (var view in viewList)
				{
					if (((NoteView)view).Flipped)
					{
						((NoteView)view).FlipBack();
					}
				}
			}
		}
	}
}

