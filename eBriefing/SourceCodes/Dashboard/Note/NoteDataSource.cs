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
using LiveButtonBinding;
using Metrostar.Mobile.Framework;

namespace eBriefingMobile
{
	public class NoteDataSource : UICollectionViewSource
	{
		private NSString cellID;
		private String bookID;
		private Dictionary<Chapter, List<Note>> dictionary;
		private UICollectionView collectionView;
		private int _headerIndex;

		public delegate void NoteDataSourceDelegate0 (UIScrollView scrollView);

		public delegate void NoteDataSourceDelegate1 (String pageID, Note note);

		public event NoteDataSourceDelegate0 ScrolledEvent;
		public event NoteDataSourceDelegate1 ItemPressedEvent;

		public NoteDataSource (Book book, UICollectionView collectionView, String cellID)
		{
			this.cellID = new NSString (cellID);
			this.bookID = book.ID;
			this.collectionView = collectionView;

			// dictionary
			dictionary = new Dictionary<Chapter, List<Note>> ();

			List<Page> pageList = BooksOnDeviceAccessor.GetPages (bookID);
			if ( pageList != null )
			{
				foreach (var page in pageList)
				{
					List<Note> notesOnPage = BooksOnDeviceAccessor.GetNotes (bookID, page.ID);
					if ( notesOnPage != null && notesOnPage.Count > 0 )
					{
						// chapter
						Chapter chapter = BooksOnDeviceAccessor.GetChapter (bookID, page.ChapterID);
						if ( chapter != null )
						{
							if ( !dictionary.ContainsKey (chapter) )
							{
								dictionary.Add (chapter, new List<Note> ());
							}
						}
					}
				}
			}
		}

		public override nint NumberOfSections (UICollectionView collectionView)
		{
			if ( dictionary != null )
			{
				return dictionary.Count;
			}

			return 1;
		}


		public override nint GetItemsCount (UICollectionView collectionView, nint section)
		{
			if ( dictionary != null )
			{
				int count = dictionary.Skip ((int)section).FirstOrDefault ().Value.Count;
				return count;
			}

			return 0;
		}

		public CGSize GetCellSize (UICollectionView collectionView, NSIndexPath indexPath)
		{
			Note note = dictionary.Skip (indexPath.Section).FirstOrDefault ().Value [indexPath.Row];
			if ( note.Text == "Header: " + note.PageID )
			{
				_headerIndex = indexPath.Row;
				return new CGSize (200, 300);
			}
			else if ( note.Text == "Footer: " + note.PageID )
			{
				var remainingWidth = GetRemainingWidthForRow (indexPath);

				return new CGSize (remainingWidth, 300);
				//return new CGSize(51, 300);
			}
			else
			{
				return new CGSize (250, 300);
			}
		}


		public override UICollectionViewCell GetCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = (NoteCell)collectionView.DequeueReusableCell (cellID, indexPath);

			Note note = dictionary.Skip (indexPath.Section).FirstOrDefault ().Value [indexPath.Row];

			if ( note != null )
			{				
				NoteCellView noteView = new NoteCellView (bookID, note);
				noteView.ItemPressedEvent += HandleItemPressedEvent;
				cell.NoteView = noteView;
						
				cell.Frame = new CGRect (cell.Frame.X, cell.Frame.Y, noteView.Frame.Width, noteView.Frame.Height);
			}

			return cell;
		}

		public override bool ShouldSelectItem (UICollectionView collectionView, NSIndexPath indexPath)
		{
			return true;
		}

		public override UICollectionReusableView GetViewForSupplementaryElement (UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
		{
			if ( elementKind.Description.Contains (UICollectionElementKindSection.Header.ToString ()) )
			{
				var headerView = (NoteHeader)collectionView.DequeueReusableSupplementaryView (elementKind, new NSString ("noteHeader"), indexPath);

				Chapter chapter = dictionary.Skip (indexPath.Section).FirstOrDefault ().Key;
				if ( chapter != null )
				{
					headerView.CH = chapter;
					headerView.IndexPath = indexPath;
					headerView.Text = "Chapter " + chapter.ChapterNumber.ToString () + "  |  " + chapter.Title;
					headerView.Expanded = dictionary [chapter].Count == 0 ? false : true;

					if ( headerView.Expanded )
					{
						headerView.Update (FRDLivelyButtonStyle.CaretDown);
					}
					else
					{
						headerView.Update (FRDLivelyButtonStyle.CaretUp);
					}

					headerView.CollapseEvent -= HandleCollapseEvent;
					headerView.CollapseEvent += HandleCollapseEvent;

					headerView.ExpandEvent -= HandleExpandEvent;
					headerView.ExpandEvent += HandleExpandEvent;
				}

				return headerView;
			}

			return null;
		}

		public void ScrollEvent (UIScrollView scrollView)
		{
			if ( ScrolledEvent != null )
			{
				ScrolledEvent (scrollView);
			}
		}

		public void Sort ()
		{
			foreach (var item in dictionary)
			{
				int start = 0;
				int end = 0;

				for (int i = 0; i < item.Value.Count; i++)
				{
					if ( item.Value [i].Text.Contains ("Header") )
					{
						start = i;
					}
					else if ( item.Value [i].Text.Contains ("Footer") )
					{
						end = i;

						item.Value.Reverse (start, end - start + 1);

						Note note = item.Value [start];
						item.Value [start] = item.Value [end];
						item.Value [end] = note;
					}
					else if ( item.Value.Count == i + 1 )
					{
						end = i;

						item.Value.Reverse (start, end - start + 1);

						Note note = item.Value [end];
						item.Value.RemoveAt (end);
						item.Value.Insert (start, note);
					}
				}
			}

			collectionView.ReloadData ();
		}

		public void ExpandAll ()
		{
			for (int i = 0; i < dictionary.Count; i++)
			{
				var chapter = dictionary.Keys.ToList () [i];
				if ( dictionary [chapter].Count == 0 )
				{
					HandleExpandEvent (dictionary.Keys.ToList () [i], NSIndexPath.FromRowSection (0, i));
				}
			}
		}

		public void CollapseAll ()
		{
			for (int i = 0; i < dictionary.Count; i++)
			{
				var chapter = dictionary.Keys.ToList () [i];
				if ( dictionary [chapter].Count > 0 )
				{
					HandleCollapseEvent (dictionary.Keys.ToList () [i], NSIndexPath.FromRowSection (0, i));
				}
			}
		}

		void HandleItemPressedEvent (String pageID, Note note)
		{
			if ( ItemPressedEvent != null )
			{
				ItemPressedEvent (pageID, note);
			}
		}

		void HandleExpandEvent (Chapter chapter, NSIndexPath indexPath)
		{
			try
			{
				List<Page> pageList = BooksOnDeviceAccessor.GetPages (bookID, chapter.ID);
				if ( pageList != null )
				{
					foreach (var page in pageList)
					{
						List<Note> notesOnPage = BooksOnDeviceAccessor.GetNotes (bookID, page.ID);
						if ( notesOnPage != null && notesOnPage.Count > 0 )
						{
							// Sort by latest
							notesOnPage.Sort ((x, y) => x.ModifiedUtc.CompareTo (y.ModifiedUtc));

							// headerNote
							Note headerNote = new Note ();
							headerNote.Text = "Header: " + page.ID;
							headerNote.PageID = page.ID;
							notesOnPage.Insert (0, headerNote);

							// footerNote
							Note footerNote = new Note ();
							footerNote.Text = "Footer: " + page.ID;
							footerNote.PageID = page.ID;
							notesOnPage.Add (footerNote);

							if ( dictionary.ContainsKey (chapter) )
							{
								List<Note> noteList = dictionary [chapter];
								noteList.AddRange (notesOnPage);
								dictionary [chapter] = noteList;
							}
						}
					}

					// Remove last separator
					dictionary [chapter].RemoveAt (dictionary [chapter].Count - 1);
				}

				collectionView.ReloadSections (NSIndexSet.FromIndex (indexPath.Section));
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging ("NoteDataSource - HandleExpandEvent: {0}", ex.ToString ());
			}
		}

		void HandleCollapseEvent (Chapter chapter, NSIndexPath indexPath)
		{
			try
			{
				dictionary [chapter].Clear ();

				collectionView.ReloadSections (NSIndexSet.FromIndex (indexPath.Section));
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging ("NoteDataSource - HandleCollapseEvent: {0}", ex.ToString ());
			}
		}

		private nfloat GetRemainingWidthForRow(NSIndexPath indexPath)
		{
			if ( dictionary != null )
			{
				var footer = dictionary.Skip (indexPath.Section).FirstOrDefault ().Value;

				if ( footer != null )
				{
					var noOfNotes = indexPath.Row-_headerIndex-1;
					var usedWidth = 200 + ((noOfNotes) * 250);
					int noOfNotesPerRow = (int)this.collectionView.ContentSize.Width / 250;

					var remainder = (noOfNotes-noOfNotesPerRow) % noOfNotesPerRow;

					if(remainder >0)
					{
						var footerWidth= this.collectionView.ContentSize.Width-(remainder*250);
						return footerWidth;
					}
					else
					{
						nfloat remainingWidth = 0f;
						if ( noOfNotes < noOfNotesPerRow )
						{
							remainingWidth = (nfloat)this.collectionView.ContentSize.Width - (noOfNotes * 250)-200;
						}
						else if(noOfNotes > noOfNotesPerRow)
						{
							remainingWidth = (nfloat)this.collectionView.ContentSize.Width - (noOfNotesPerRow * 250);
						}
						return remainingWidth;
					}
				}
				else
				{
					return 51;
				}
			}
			return 51;
		}
	}

	#region NoteHeader
	public class NoteHeader : UICollectionReusableView
	{
		private UILabel headerLabel;
		private FRDLivelyButton collapseButton;
		private UIButton transButton;
		private Chapter chapter;
		private NSIndexPath indexPath;
		private bool expanded = false;

		public delegate void NoteHeaderDelegate (Chapter chapter, NSIndexPath indexPath);

		public event NoteHeaderDelegate CollapseEvent;
		public event NoteHeaderDelegate ExpandEvent;

		[Export ("initWithFrame:")]
		public NoteHeader (CGRect frame) : base (frame)
		{
			this.BackgroundColor = UIColor.White;
			this.Layer.BorderColor = eBriefingAppearance.Gray4.CGColor;
			this.Layer.BorderWidth = 1f;

			// collapseButton
			collapseButton = eBriefingAppearance.GenerateLiveButton (eBriefingAppearance.BlueColor, eBriefingAppearance.GreenColor, 2f);
			collapseButton.Frame = new CGRect (20, 12, 25, 20);
			collapseButton.SetStyle (FRDLivelyButtonStyle.CaretUp, false);
			collapseButton.UserInteractionEnabled = false;
			this.AddSubview (collapseButton);

			// headerLabel
			headerLabel = eBriefingAppearance.GenerateLabel (21, eBriefingAppearance.Gray1, true);
			headerLabel.Frame = new CGRect (collapseButton.Frame.Right + 20, 0, frame.Width - 20, frame.Height);
			headerLabel.TextColor = eBriefingAppearance.Gray2;
			this.AddSubview (headerLabel);

			transButton = UIButton.FromType (UIButtonType.Custom);
			transButton.Frame = new CGRect (0, 0, this.Frame.Width, this.Frame.Height);
			transButton.TouchUpInside += HandleTouchUpInside;
			this.AddSubview (transButton);
		}

		public void Update (FRDLivelyButtonStyle style)
		{
			collapseButton.SetStyle (style, true);
		}

		void HandleTouchUpInside (object sender, EventArgs e)
		{
			expanded = !expanded;

			if ( expanded )
			{
				if ( ExpandEvent != null )
				{
					ExpandEvent (chapter, indexPath);
				}
			}
			else
			{
				if ( CollapseEvent != null )
				{
					CollapseEvent (chapter, indexPath);
				}
			}
		}

		public String Text
		{
			get
			{
				return headerLabel.Text;
			}
			set
			{
				headerLabel.Text = value;
			}
		}

		public NSIndexPath IndexPath
		{
			get
			{
				return indexPath;
			}
			set
			{
				indexPath = value;
			}
		}

		public Chapter CH
		{
			get
			{
				return chapter;
			}
			set
			{
				chapter = value;
			}
		}

		public bool Expanded
		{
			get
			{
				return expanded;
			}
			set
			{
				expanded = value;
			}
		}
	}
	#endregion
}

