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
using Metrostar.Mobile.Framework;

namespace eBriefingMobile
{
	public class LibraryDataSource : UICollectionViewSource
	{
		private NSString cellID = new NSString(StringRef.LibraryCell);
		private Dictionary<NSIndexPath, LibraryBookView> indexDictionary;
		private LibraryViewController parentVC;

		public List<Book> BookList { get; set; }

		public delegate void LibraryDataSourceDelegate0(Book book);

		public delegate void LibraryDataSourceDelegate1(LibraryBookView bookView);

		public event LibraryDataSourceDelegate0 ItemPressedEvent;
		public event LibraryDataSourceDelegate1 DownloadEvent;

		public LibraryDataSource(List<Book> bookList, LibraryViewController parentVC)
		{
			this.BookList = bookList;
			this.parentVC = parentVC;

			if (BookList != null && BookList.Count > 0)
			{
				// Sort books based on current setting
				BookList = SortBooks(bookList);

				// Update dictionary
				indexDictionary = new Dictionary<NSIndexPath, LibraryBookView>();
			}
		}

		public override nint NumberOfSections(UICollectionView collectionView)
		{
			return 1;
		}

		public override nint GetItemsCount(UICollectionView collectionView, nint section)
		{
			if (BookList != null)
			{
				UpdateDictionary();

				return BookList.Count;
			}
            
			return 0;
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = (LibraryCell)collectionView.DequeueReusableCell(cellID, indexPath);

			var book = BookList[indexPath.Row];
			if (book != null)
			{
				cell.dataSource = this;
				cell.LibraryView = indexDictionary[indexPath];
				cell.LibraryView.UpdateImage(book.LargeImageURL);
			}

			return cell;
		}

		public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			if (ItemPressedEvent != null)
			{
				var cell = (LibraryCell)collectionView.CellForItem(indexPath);

				ItemPressedEvent(cell.LibraryView.LibraryBook);
			}
		}

		public override bool ShouldSelectItem(UICollectionView collectionView, NSIndexPath indexPath)
		{
			return true;
		}

		public void UpdateImage(String url)
		{
			var item = indexDictionary.Where(i => i.Value.LibraryBook.LargeImageURL == url).FirstOrDefault();
			if (item.Value != null)
			{
				item.Value.UpdateImage(url);
			}
		}

		public List<Book> SortBooks(List<Book> list)
		{
			if (list != null)
			{
				try
				{
					if (Settings.SortBy == StringRef.ByName)
					{
						if (Settings.SortAscending)
						{
							list.Sort((x, y) => x.Title.CompareTo(y.Title));
						}
						else
						{
							list.Sort((x, y) => y.Title.CompareTo(x.Title));
						}

					}
					else if (Settings.SortBy == StringRef.ByDateAdded)
					{
						if (Settings.SortAscending)
						{
							list.Sort((x, y) => x.ServerAddedDate.CompareTo(y.ServerAddedDate));
						}
						else
						{
							list.Sort((x, y) => y.ServerAddedDate.CompareTo(x.ServerAddedDate));
						}
					}
					else
					{
						if (Settings.SortAscending)
						{
							list.Sort((x, y) => x.ServerModifiedDate.CompareTo(y.ServerModifiedDate));
						}
						else
						{
							list.Sort((x, y) => y.ServerModifiedDate.CompareTo(x.ServerModifiedDate));
						}
					}
				}
				catch (Exception ex)
				{
					Logger.WriteLineDebugging("LibraryDataSource - SortBooks: {0}", ex.ToString());
				}
			}

			return list;
		}

		public void HandleDownloadEvent(LibraryBookView bookView)
		{
			if (DownloadEvent != null)
			{
				DownloadEvent(bookView);
			}
		}

		public LibraryBookView GetLibraryBookView(String bookID)
		{
			if (indexDictionary != null)
			{
				var item = indexDictionary.Where(i => i.Value.LibraryBook.ID == bookID).FirstOrDefault();
				if (item.Value != null)
				{
					return item.Value;
				}
			}

			return null;
		}

		public NSIndexPath GetIndexPath(String bookID)
		{
			if (indexDictionary != null)
			{
				var item = indexDictionary.Where(i => i.Value.LibraryBook.ID == bookID).FirstOrDefault();
				if (item.Value != null)
				{
					return item.Key;
				}
			}

			return null;
		}

		public int GetBookIndex(String bookID)
		{
			if (indexDictionary != null)
			{
				var item = indexDictionary.Where(i => i.Value.LibraryBook.ID == bookID).FirstOrDefault();
				if (item.Value != null)
				{
					return item.Key.Row;
				}
			}

			return -1;
		}

		private void UpdateDictionary()
		{
			indexDictionary.Clear();

			int row = 0;
			foreach (Book book in BookList)
			{
				LibraryBookView bookView = new LibraryBookView(book, parentVC);
				indexDictionary.Add(NSIndexPath.FromRowSection(row, 0), bookView);
				row++;
			}
		}
	}
	#region LibraryCell
	public class LibraryCell : UICollectionViewCell
	{
		private LibraryBookView libraryBookView;

		public LibraryDataSource dataSource { get; set; }

		public LibraryBookView LibraryView
		{
			get
			{
				return libraryBookView;
			}
			set
			{
				libraryBookView = value;
				libraryBookView.DownloadEvent += HandleDownloadEvent;
				ContentView.AddSubview(libraryBookView);
			}
		}

		[Export("initWithFrame:")]
		public LibraryCell(CGRect frame) : base(frame)
		{
			BackgroundView = new UIView {
				BackgroundColor = UIColor.Clear
			};

			SelectedBackgroundView = new UIView {
				BackgroundColor = UIColor.Clear
			};

			ContentView.Layer.BorderColor = UIColor.LightGray.CGColor;
			ContentView.BackgroundColor = UIColor.Clear;
		}

		public override void PrepareForReuse()
		{
			base.PrepareForReuse();

			foreach (UIView subview in ContentView)
			{
				subview.RemoveFromSuperview();
			}
		}

		void HandleDownloadEvent()
		{
			dataSource.HandleDownloadEvent(libraryBookView);
		}
	}
	#endregion
}

