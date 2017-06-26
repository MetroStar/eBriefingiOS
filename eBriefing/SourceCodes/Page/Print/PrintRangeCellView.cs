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
	public class PrintRangeCellView : UIView
	{
		private nuint MAX_PRINT_RANGE;

		public UITextField StartField { get; set; }

		public UITextField EndField { get; set; }

		public delegate void PrintRangeCellDelegate0(nuint page);

		public delegate void PrintRangeCellDelegate1();

		public event PrintRangeCellDelegate0 UpdateStartEvent;
		public event PrintRangeCellDelegate0 UpdateEndEvent;
		public event PrintRangeCellDelegate1 ApplyEvent;

		public PrintRangeCellView(nuint start, nuint end, nuint MaxRange)
		{
			NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardNotification);

			this.BackgroundColor = UIColor.Clear;
			this.Frame = new CGRect(0, 0, 200, 44);

			MAX_PRINT_RANGE = MaxRange;

			// StartField
			StartField = GenerateTextField(String.Empty, new CGPoint(10, 5), 0);
			StartField.AllEditingEvents += HandleStartFieldEditingEvents;

			if (start > 0)
			{
				StartField.Text = start.ToString();
			}

			// toLabel
			UILabel toLabel = eBriefingAppearance.GenerateLabel(17, UIColor.Black);
			toLabel.Text = "to";
			toLabel.TextAlignment = UITextAlignment.Center;
			toLabel.Frame = new CGRect(StartField.Frame.Right + 8, (StartField.Center.Y - (21f / 2f)), 21, 21);
			this.AddSubview(toLabel);

			// EndField
			EndField = GenerateTextField(String.Empty, new CGPoint(toLabel.Frame.Right + 8, StartField.Frame.Y), 1);
			EndField.ReturnKeyType = UIReturnKeyType.Done;
			EndField.AllEditingEvents += HandleEndFieldEditingEvents;

			if (end > 0)
			{
				EndField.Text = end.ToString();
			}
		}

		private UITextField GenerateTextField(String placeholder, CGPoint position, int tag)
		{
			UITextField textField = eBriefingAppearance.GenerateTextField(placeholder);
			textField.Tag = tag;
			textField.Frame = new CGRect(position.X, position.Y, 50, this.Frame.Height - 10);
			textField.KeyboardType = UIKeyboardType.NumberPad;
			textField.Layer.BorderColor = eBriefingAppearance.Gray5.CGColor;
			textField.Layer.BorderWidth = 1f;
			textField.ShouldReturn = delegate
			{
				return HandleShouldReturn(tag);
			};
			this.AddSubview(textField);

			return textField;
		}

		public void EnableTextField(bool enable)
		{
			StartField.UserInteractionEnabled = EndField.UserInteractionEnabled = enable;

			if (enable)
			{
				StartField.BecomeFirstResponder();
			}
		}

		bool HandleShouldReturn(int tag)
		{
			try
			{
				if (ApplyEvent != null)
				{
					ApplyEvent();
				}

				nuint start = 0;
				if (!String.IsNullOrEmpty(StartField.Text))
				{
					start = Convert.ToUInt32(StartField.Text);
				}

				nuint end = 0;
				if (!String.IsNullOrEmpty(EndField.Text))
				{
					end = Convert.ToUInt32(EndField.Text);
				}

				if (tag == 0)
				{
					EndField.BecomeFirstResponder();
				}

				if (!String.IsNullOrEmpty(StartField.Text) && !String.IsNullOrEmpty(EndField.Text))
				{
					if (start <= end)
					{
						EndField.ResignFirstResponder();
						return true;
					}
					else
					{
						new UIAlertView(StringRef.alert, "Start page number must be less than the End page number.", null, StringRef.ok, null).Show();
					}

					return false;
				}
			}
			catch (Exception)
			{
				new UIAlertView(StringRef.alert, "Page number must be an Integer.", null, StringRef.ok, null).Show();

				return false;
			}

			return true;
		}

		void HandleStartFieldEditingEvents(object sender, EventArgs e)
		{
			try
			{
				if (!String.IsNullOrEmpty(StartField.Text))
				{
					nuint start = Convert.ToUInt32(StartField.Text);
					if (UpdateStartEvent != null)
					{
						UpdateStartEvent(start);
					}
				}
			}
			catch
			{
				StartField.Text = String.Empty;
			}
		}

		void HandleEndFieldEditingEvents(object sender, EventArgs e)
		{
			try
			{
				if (!String.IsNullOrEmpty(EndField.Text))
				{
					nuint end = Convert.ToUInt32(EndField.Text);
					if (UpdateEndEvent != null)
					{
						UpdateEndEvent(end);
					}
				}
			}
			catch
			{
				EndField.Text = String.Empty;
			}
		}

		private void OnKeyboardNotification (NSNotification notification)
		{
			if(!String.IsNullOrEmpty(StartField.Text) || !String.IsNullOrEmpty(EndField.Text))
			{
				if (ApplyEvent != null)
				{
					ApplyEvent();
				}
			}
		}

	}
}