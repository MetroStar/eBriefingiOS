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

namespace eBriefingMobile
{
	public class NoteView : UIButton
	{
		private NoteOptionView optionView;
		private UIView containerView;
		private Note note;

		public bool Flipped { get; set; }

		public nfloat TotalHeight { get; set; }

		public delegate void NoteViewDelegate0(Note note);

		public delegate void NoteViewDelegate1(NoteView noteView);

		public event NoteViewDelegate0 EditNoteEvent;
		public event NoteViewDelegate0 RemoveNoteEvent;
		public event NoteViewDelegate1 FlipBackEvent;

		public NoteView(Note note, nfloat width)
		{
			this.BackgroundColor = UIColor.White;
			this.Frame = new CGRect(0, 0, width, 1);
			this.Layer.BorderColor = eBriefingAppearance.Gray3.CGColor;
			this.Layer.BorderWidth = 1f;

			this.note = note;

			// textView
			UITextView textView = eBriefingAppearance.GenerateTextView();
			textView.Frame = new CGRect(0, 0, width, 1);
			textView.Text = note.Text;
			textView.TextColor = eBriefingAppearance.Gray2;
			textView.UserInteractionEnabled = false;
			textView.SizeToFit();
			this.AddSubview(textView);

			TotalHeight = (nfloat)Math.Max(textView.Frame.Bottom + 42, 37 * 3 + 40);

			// label
			UILabel label = eBriefingAppearance.GenerateLabel(14, eBriefingAppearance.Gray2);
			label.Text = note.ModifiedUtc.ToString("MMM.dd.yyyy");
			label.Frame = new CGRect(10, TotalHeight - 21 - 5, width - 20, 21);
			label.UserInteractionEnabled = false;
			this.AddSubview(label);

			// line
			UIView line = new UIView(new CGRect(10, label.Frame.Y - 6, this.Frame.Width - 20, 1));
			line.BackgroundColor = eBriefingAppearance.Gray3;
			this.AddSubview(line);

			this.TouchUpInside += HandleTouchUpInside;
		}

		public void FlipBack()
		{
			Flipped = false;

			UIView.Transition(optionView, containerView, 0.5d, UIViewAnimationOptions.TransitionFlipFromLeft, delegate
			{
				containerView.RemoveFromSuperview();
				containerView.Dispose();
				containerView = null;

				optionView.RemoveFromSuperview();
				optionView.Dispose();
				optionView = null;
			});
		}

		void HandleTouchUpInside(object sender, EventArgs e)
		{
			Flipped = true;

			// Flip back other noteViews
			if (FlipBackEvent != null)
			{
				FlipBackEvent(this);
			}

			// Animation containerView
			containerView = new UIView(this.Frame);
			this.AddSubview(containerView);

			// optionView
			optionView = new NoteOptionView(this.Frame);
			optionView.EditNoteEvent += HandleEditNote;
			optionView.RemoveNoteEvent += HandleRemoveNote;
			optionView.CancelEvent += FlipBack;
			containerView.AddSubview(optionView);

			// Start flip
			UIView.Transition(containerView, optionView, 0.5d, UIViewAnimationOptions.TransitionFlipFromRight, null);
		}

		void HandleEditNote()
		{
			if (EditNoteEvent != null)
			{
				EditNoteEvent(note);
			}
		}

		void HandleRemoveNote()
		{
			if (RemoveNoteEvent != null)
			{
				RemoveNoteEvent(note);
			}
		}
	}
}

