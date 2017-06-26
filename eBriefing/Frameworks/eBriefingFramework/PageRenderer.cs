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
using UIKit;
using CoreGraphics;
using Foundation;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using CoreFoundation;

namespace eBriefingMobile
{
	public class PageRenderer : UIPrintPageRenderer
	{
		private nfloat points_per_inch= 72;
		private nfloat paperWidth;
		private nfloat paperHeight;
		private Dictionary<nint,Dictionary<int,PrintContent>> printItem;
		private nfloat xPoint;
		private nfloat yPoint;
		NSRange pageRange;

		public PageRenderer (eBriefingMobile.PrintHelper.ORIENTATION orientation,Dictionary<int,Dictionary<UIImage,List<Note>>>_pdf)
		{
			printItem = new Dictionary<nint,Dictionary<int,PrintContent>> ();

			if ( orientation == PrintHelper.ORIENTATION.LANDSCAPE )
			{
				paperWidth = 792f;
				paperHeight = 612f;
			}
			else
			{
				paperWidth = 612f;
				paperHeight = 792f;
			}

			foreach (var item in _pdf)
			{
				var noteList = new List<Note> ();

				foreach (var value in item.Value)
				{
					//notes included
					if ( value.Value != null && value.Value.Count > 0 )
					{
						string notes = "";
						string noteText = "";

						foreach (var note in value.Value)
						{
							noteText += note.ModifiedUtc.ToString ("MMM dd, yyyy") + "\n" + note.Text + "\n\n";

							var totalHeight = CalculateStringSize (noteText,eBriefingAppearance.ThemeRegularFont (9f)).Height;

							//if the totalHeight of notes is larger than total height of paper,later notes should go to next page.
							if ( totalHeight > (paperHeight + (points_per_inch / 4)) )
							{							
								noteList.Add (note);
							}
							else
							{
								notes = noteText;
							}
						}

						var content = new PrintContent ();
						content.PdfImage = value.Key;
						content.Note = notes;
						content.NoteCount = value.Value.Count;

						var printContentDict= new Dictionary<int,PrintContent>();
						printContentDict.Add (item.Key, content);
						printItem.Add (printItem.Count, printContentDict);

						//nextPage
						if ( noteList.Count > 0 )
						{
							var content1 = new PrintContent ();
							string expandedNote = "";
							string noteColumn0 = "";
							var expandedNoteList = new List<Note> ();

							foreach (var note in noteList)
							{
								expandedNote += note.ModifiedUtc.ToString ("MMM dd, yyyy") + "\n" + note.Text + "\n\n";
								var totalHeight = CalculateStringSize (expandedNote,eBriefingAppearance.ThemeRegularFont (9f)).Height;

								//if the totalHeight of notes is larger than total height of paper,later notes should go to next column.
								if ( totalHeight > (paperHeight) )
								{							
									expandedNoteList.Add (note);
								}
								else
								{
									noteColumn0 = expandedNote;
								}
							}

							if ( expandedNoteList.Count > 0 )
							{
								expandedNote = "";
								
								foreach (var note in expandedNoteList)
								{
									expandedNote += note.ModifiedUtc.ToString ("MMM dd, yyyy") + "\n" + note.Text + "\n\n";
								}
								content1.Note2 = expandedNote;
							}

							content1.Note = noteColumn0;

							var printContentDict1= new Dictionary<int,PrintContent>();
							printContentDict1.Add (item.Key, content1);
							printItem.Add (printItem.Count, printContentDict1);
						}
					}
					else //notes is note included
					{
						var content = new PrintContent ();
						content.PdfImage = value.Key;

						var printContentDict= new Dictionary<int,PrintContent>();
						printContentDict.Add (item.Key, content);

						printItem.Add (printItem.Count, printContentDict);
					}
				}
			}

			this.HeaderHeight = points_per_inch/4;
			this.FooterHeight = points_per_inch/6;
		}


		// This property must be overriden when doing custom drawing as we are.
		// Since our custom drawing is really only for the borders and we are
		// relying on a series of UIMarkupTextPrintFormatters to display the recipe
		// content, UIKit can calculate the number of pages based on informtation
		// provided by those formatters.
		//
		// Therefore, setup the formatters, and ask super to count the pages.
		// HACK: Changed overridden member int to nint
		public override nint NumberOfPages {
			get
			{
				return printItem.Count;
			}
		}

		public override void DrawHeaderForPage (nint index, CoreGraphics.CGRect headerRect)
		{

			xPoint = paperWidth / 1.7f;
			yPoint = 36f;

			var item = printItem [index].First ();
			var printContent = item.Value;

			string headerText = "";
			if ( printContent.PdfImage == null )
			{
				headerText = String.Format ("Notes Continued (pg {0})", item.Key.ToString ());
			}
			else if ( printContent.PdfImage != null && printContent.Note != null )
			{
				headerText = String.Format ("{0} Notes (pg {1})", printContent.NoteCount.ToString (), item.Key.ToString ());
			}

			var pageNumberString = new NSString (headerText);
			pageNumberString.DrawString (headerRect, eBriefingAppearance.ThemeRegularFont (12f), UILineBreakMode.Clip, UITextAlignment.Center);
			pageNumberString.Dispose ();
		}

		public override void PrepareForDrawingPages (NSRange range)
		{
			this.InvokeOnMainThread (delegate
			{
				base.PrepareForDrawingPages (range);
				pageRange = range;
			});
		}

		public override void DrawFooterForPage (nint index, CGRect footerRect)
		{
			var item = printItem [index].First ();
			NSString footer = new NSString (item.Key.ToString ());
			footer.DrawString (footerRect,  eBriefingAppearance.ThemeRegularFont (10f), UILineBreakMode.Clip, UITextAlignment.Center);
			footer.Dispose ();
		}

		private void DrawImages(UIImage image)
		{
			nfloat expectedWidth = (paperWidth / 1.7f) - (points_per_inch/2);
			var factor = expectedWidth / image.Size.Width;

			var expectedHeight = image.Size.Height * factor;

			var sizedRect = new CGRect (this.points_per_inch/4,yPoint*2,expectedWidth-(yPoint*2),expectedHeight-(yPoint*2));
			image.Draw (sizedRect);
		}

		private void DrawImagesFullSize(UIImage image)
		{
			//landscape
			if ( paperWidth > paperHeight )
			{
				nfloat expectedHeight = paperHeight-points_per_inch;
				var factor = expectedHeight / image.Size.Height;

				var expectedWidth = image.Size.Width * factor;

				var sizedRect = new CGRect ((paperWidth-expectedWidth)/2,(paperHeight-expectedHeight)/2 ,expectedWidth,expectedHeight);
				image.Draw (sizedRect);
			}
			else
			{
				nfloat expectedWidth = paperWidth-points_per_inch;
				var factor = expectedWidth / image.Size.Width;

				var expectedHeight = image.Size.Height * factor;

				var sizedRect = new CGRect (this.points_per_inch/2,(paperHeight-expectedHeight)/2 ,expectedWidth,expectedHeight);
				image.Draw (sizedRect);
			}
		}

		public override void DrawContentForPage (nint index, CGRect contentRect)
		{
			var item = printItem [index].First ();
			var content = item.Value;

			if ( content.PdfImage != null )
			{
				if ( content.Note != null )
				{
					this.DrawImages (content.PdfImage);
				}
				else
				{
					this.DrawImagesFullSize (content.PdfImage);
				}
			}

			if ( content.Note != null )
			{
				UIFont font = eBriefingAppearance.ThemeRegularFont (9f);

				UIStringAttributes attributes = new UIStringAttributes ();
				attributes.Font = eBriefingAppearance.ThemeBoldFont (9f);

				NSMutableParagraphStyle paragraphStyle = new NSMutableParagraphStyle ();
				paragraphStyle.Alignment = UITextAlignment.Justified;

				if ( content.PdfImage != null )
				{
					var overlayRect = new CGRect (xPoint, yPoint * 2f, paperWidth - xPoint - (points_per_inch / 4), paperHeight - points_per_inch);

					var attStr = new NSMutableAttributedString (content.Note, font, UIColor.Black, UIColor.Clear, null, paragraphStyle);
					var dict = ChangeFontWeight (content.Note);
					foreach (KeyValuePair<int,string> x in dict)
					{
						var range = new NSRange (x.Key, x.Value.Length);
						attStr.SetAttributes (attributes, range);
					}
					attStr.DrawString (overlayRect);
					attStr.Dispose ();
				}
				else
				{
					var overlayRect = new CGRect (yPoint, yPoint * 2.5f, paperWidth / 2 - (points_per_inch / 2), paperHeight - points_per_inch);
					var attStr = new NSMutableAttributedString (content.Note, font, UIColor.Black, UIColor.Clear, null, paragraphStyle);
					var dict = ChangeFontWeight (content.Note);
					foreach (KeyValuePair<int,string> x in dict)
					{
						var range = new NSRange (x.Key, x.Value.Length);
						attStr.SetAttributes (attributes, range);
					}
					attStr.DrawString (overlayRect);
					attStr.Dispose ();

					if ( content.Note2 != null )
					{
						var overlayRect1 = new CGRect (overlayRect.Right + yPoint, yPoint * 2.5f, paperWidth / 2 - (points_per_inch / 2), paperHeight - points_per_inch);
						var attStr1 = new NSMutableAttributedString (content.Note2, font, UIColor.Black, UIColor.Clear, null, paragraphStyle);
						var dict1 = ChangeFontWeight (content.Note2);

						foreach (KeyValuePair<int,string> x in dict1)
						{
							var range = new NSRange (x.Key, x.Value.Length);
							attStr1.SetAttributes (attributes, range);
						}
						attStr1.DrawString (overlayRect1);
						attStr1.Dispose ();
					}
				}
			}
		}

		private CGSize CalculateStringSize(string note,UIFont font)
		{			
			CGSize constraint = new CGSize(paperWidth-(paperWidth / 1.7f)-points_per_inch,nfloat.MaxValue);

			NSString str = new NSString(note);
			UIStringAttributes attributes1 = new UIStringAttributes ();
			attributes1.Font =font;

			var rect=str.GetBoundingRect (constraint, NSStringDrawingOptions.UsesLineFragmentOrigin,attributes1, null);
 			return rect.Size;
		}

		private Dictionary<int,string> ChangeFontWeight(string text)
		{
			Dictionary<int,string> dict = new Dictionary<int, string> ();
			StringBuilder sb = new StringBuilder();

			sb.Append (@"(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s(\d\d)[,]\s(\d{4})\s");

			Regex exp = new Regex (sb.ToString (),RegexOptions.Compiled);

			MatchCollection matches = exp.Matches (text);

			foreach(Match match in matches)
			{
				int index = match.Index;
				dict.Add(index, match.Value);
			}
			return dict;
		}
	}

	public class PrintContent
	{
		public UIImage PdfImage
		{
			get;
			set;
		}
		public string Note
		{
			get;
			set;
		}
		public int NoteCount
		{
			get;
			set;
		}
		public string Note2
		{
			get;
			set;
		}
	}
}

