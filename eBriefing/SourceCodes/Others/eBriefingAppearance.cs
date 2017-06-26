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
using System.Globalization;
using Foundation;
using UIKit;
using CoreGraphics;
using SpinKitBinding;
using LiveButtonBinding;

namespace eBriefingMobile
{
	public class eBriefingAppearance
	{
		public static void SetAppearances()
		{
			// Switch
			UISwitch.Appearance.TintColor = UIColor.LightGray;
			UISwitch.Appearance.OnTintColor = GreenColor;

			// SegmentedControl
			UISegmentedControl.Appearance.TintColor = UIColor.White;

			// UITabBar
			UITabBar.Appearance.TintColor = BlueColor;

			UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, false);
		}

		public static RTSpinKitView GenerateBounceSpinner()
		{
			RTSpinKitView spinner = new RTSpinKitView(RTSpinKitViewStyle.ArcAlt);
			spinner.HidesWhenStopped = true;
			spinner.Color = eBriefingAppearance.BlueColor;
			spinner.StartAnimating();
			return spinner;
		}

		public static UILabel GenerateLabel(float fontSize = 17, UIColor color = null, bool bold = false)
		{
			UILabel label = new UILabel();
			label.BackgroundColor = UIColor.Clear;
			label.TextAlignment = UITextAlignment.Left;
			label.Font = ThemeRegularFont(fontSize);
			if (color != null)
			{
				label.TextColor = color;
			}
			else
			{
				label.TextColor = Gray1;
			}
			return label;
		}

		public static UITextField GenerateTextField(String placeholder)
		{
			UITextField textField = new UITextField();
			textField.Placeholder = placeholder;
			textField.TextColor = eBriefingAppearance.Gray1;
			textField.VerticalAlignment = UIControlContentVerticalAlignment.Center;
			textField.AutocorrectionType = UITextAutocorrectionType.No;
			textField.KeyboardType = UIKeyboardType.Default;
			textField.ReturnKeyType = UIReturnKeyType.Next;
			textField.Layer.BorderColor = eBriefingAppearance.Gray1.CGColor;
			textField.Layer.BorderWidth = 1f;
			textField.LeftView = new UIView(new CGRect(0, 0, 10, 1));
			textField.LeftViewMode = UITextFieldViewMode.Always;

			return textField;
		}

		public static UITextView GenerateTextView(float fontSize = 17)
		{
			UITextView textView = new UITextView();
			textView.BackgroundColor = UIColor.Clear;
			textView.TextColor = textView.TintColor = eBriefingAppearance.Gray1;
			textView.Font = UIFont.SystemFontOfSize(fontSize);
			textView.SpellCheckingType = UITextSpellCheckingType.Yes;
			textView.AutocorrectionType = UITextAutocorrectionType.Yes;
			textView.TintColor = eBriefingAppearance.Gray1;
			textView.TextContainerInset = new UIEdgeInsets(10, 10, 0, 10);

			return textView;
		}

		public static FRDLivelyButton GenerateLiveButton(UIColor normal, UIColor highlighted, float width)
		{
			// button
			FRDLivelyButton button = new FRDLivelyButton(new CGRect(20, 12, 25, 20));
			button.SetStyle(FRDLivelyButtonStyle.CaretUp, false);

			var keys = new NSObject [] {
				new NSString("kFRDLivelyButtonColor"),
				new NSString("kFRDLivelyButtonHighlightedColor"),
				new NSString("kFRDLivelyButtonLineWidth")
			};
			var objects = new NSObject [] {
				normal,
				highlighted,
				new NSNumber(width)
			};

			button.Options = NSDictionary.FromObjectsAndKeys(objects, keys);
			return button;
		}

		public static UIImageView Checkmark
		{
			get
			{
				UIImageView disclosure = new UIImageView();
				disclosure.Image = UIImage.FromBundle("Assets/Icons/checkmark.png");
				disclosure.Frame = new CGRect(0, 0, 13, 10);
				return disclosure;
			}
		}

		public static UIColor RedColor
		{
			get
			{
				return Color("A84126");
			}
		}

		public static UIColor BlueColor
		{
			get
			{
				return Color("114893");
//                return Color(0f, 77f, 149f);
			}
		}

		public static UIColor Blue1Color
		{
			get
			{
				return Color("1567FD");
			}
		}

		public static UIColor GreenColor
		{
			get
			{
				return Color("3cb778");
			}
		}

		public static UIColor Gray1
		{
			get
			{
				return Color("333333");
			}
		}

		public static UIColor Gray2
		{
			get
			{
				return Color("6D6D6D");
			}
		}

		public static UIColor Gray3
		{
			get
			{
				return Color("B2B2B2");
			}
		}

		public static UIColor Gray4
		{
			get
			{
				return Color("CDCDCD");
			}
		}

		public static UIColor Gray5
		{
			get
			{
				return Color("D5D5D5");
			}
		}

		public static UIColor Color(nfloat r, nfloat g, nfloat b)
		{
			return new UIColor(r / 255f, g / 255f, b / 255f, 1f);
		}

		public static UIColor Color(String hexColor)
		{
			int rgb = 0;
			int.TryParse(hexColor, NumberStyles.AllowHexSpecifier, null, out rgb);
			int r = (rgb & 0xff0000) >> 16;
			int g = (rgb & 0xff00) >> 8;
			int b = (rgb & 0xff);

			return Color(r, g, b);
		}

		public static UIFont ThemeRegularFont(nfloat size)
		{
			return UIFont.SystemFontOfSize(size);
		}

		public static UIFont ThemeBoldFont(nfloat size)
		{
			return UIFont.BoldSystemFontOfSize(size);
		}

		public static UIFont ThemeItalicFont(nfloat size)
		{
			return UIFont.ItalicSystemFontOfSize(size);
		}
	}
}