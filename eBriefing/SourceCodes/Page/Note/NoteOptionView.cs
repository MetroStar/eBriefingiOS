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
	public class NoteOptionView : UIView
	{
		public delegate void GroupOptionDelegate();

		public event GroupOptionDelegate EditNoteEvent;
		public event GroupOptionDelegate RemoveNoteEvent;
		public event GroupOptionDelegate CancelEvent;

		public NoteOptionView(CGRect frame)
		{
			this.BackgroundColor = eBriefingAppearance.Gray4;
			this.Frame = new CGRect(0, 0, frame.Width, frame.Height);
			this.Layer.BorderColor = eBriefingAppearance.Gray3.CGColor;
			this.Layer.BorderWidth = 1f;

			// removeButton
			UIButton removeButton = UIButton.FromType(UIButtonType.Custom);
			removeButton.Font = eBriefingAppearance.ThemeRegularFont(17f);
			removeButton.BackgroundColor = eBriefingAppearance.RedColor;
			removeButton.SetTitle(StringRef.Remove, UIControlState.Normal);
			removeButton.Frame = new CGRect((this.Frame.Width / 2) - (150f / 2f), this.Center.Y - (37f / 2f), 150, 37);
			removeButton.TouchUpInside += HandleRemoveTouchUpInside;
			this.AddSubview(removeButton);

			// editButton
			UIButton editButton = UIButton.FromType(UIButtonType.Custom);
			editButton.Font = eBriefingAppearance.ThemeRegularFont(17f);
			editButton.BackgroundColor = eBriefingAppearance.BlueColor;
			editButton.SetTitle("Edit", UIControlState.Normal);
			editButton.Frame = new CGRect(removeButton.Frame.X, removeButton.Frame.Y - 10 - removeButton.Frame.Height, removeButton.Frame.Width, removeButton.Frame.Height);
			editButton.TouchUpInside += HandleEditTouchUpInside;
			this.AddSubview(editButton);

			// cancelButton
			UIButton cancelButton = UIButton.FromType(UIButtonType.Custom);
			cancelButton.Font = eBriefingAppearance.ThemeRegularFont(17f);
			cancelButton.BackgroundColor = UIColor.White;
			cancelButton.SetTitle(StringRef.cancel, UIControlState.Normal);
			cancelButton.SetTitleColor(eBriefingAppearance.BlueColor, UIControlState.Normal);
			cancelButton.Frame = new CGRect(removeButton.Frame.X, removeButton.Frame.Bottom + 10, removeButton.Frame.Width, removeButton.Frame.Height);
			cancelButton.TouchUpInside += HandleCancelTouchUpInside;
			this.AddSubview(cancelButton);
		}

		void HandleEditTouchUpInside(object sender, EventArgs e)
		{
			if (EditNoteEvent != null)
			{
				EditNoteEvent();
			}
		}

		void HandleRemoveTouchUpInside(object sender, EventArgs e)
		{
			if (RemoveNoteEvent != null)
			{
				RemoveNoteEvent();
			}
		}

		void HandleCancelTouchUpInside(object sender, EventArgs e)
		{
			if (CancelEvent != null)
			{
				CancelEvent();
			}
		}
	}
}

