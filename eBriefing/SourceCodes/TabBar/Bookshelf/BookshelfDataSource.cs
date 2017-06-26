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
	public class BookshelfDataSource : UICollectionViewSource
	{
		private NSString cellID = new NSString(StringRef.BookshelfCell);
		private Dictionary<NSIndexPath, BookshelfBookView> indexDictionary;
		private BookshelfViewController parentVC;
		private bool updateMenu;

		public List<Book> BookList { get; set; }

		public delegate void BookshelfDataSourceDelegate1();

		public delegate void BookshelfDataSourceDelegate2(Book book);

		public delegate void BookshelfDataSourceDelegate3(BookshelfBookView bookView, bool isFavorite);

		public event BookshelfDataSourceDelegate1 RefreshBookshelfEvent;
		public event BookshelfDataSourceDelegate2 UpdateBookEvent;
		public event BookshelfDataSourceDelegate2 ItemPressedEvent;
		public event BookshelfDataSourceDelegate3 ShowMenuEvent;

		public BookshelfDataSource(List<Book> bookList, bool updateMenu, BookshelfViewController parentVC)
		{
			this.updateMenu = updateMenu;
			this.parentVC = parentVC;
			this.BookList = bookList;

			if (BookList != null && BookList.Count > 0)
			{
				indexDictionary = new Dictionary<NSIndexPath, BookshelfBookView>();

				// Sort books based on current setting
				BookList = SortBooks(bookList);
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
			var cell = (BookshelfCell)collectionView.DequeueReusableCell(cellID, indexPath);
            
			var book = BookList[indexPath.Row];
			if (book != null)
			{
				cell.dataSource = this;
				cell.BookshelfView = indexDictionary[indexPath];
			}
            
			return cell;
		}

		public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			if (ItemPressedEvent != null)
			{
				var cell = (BookshelfCell)collectionView.CellForItem(indexPath);

				// Open only if the book is downloaded completely
				if (cell.BookshelfView.BookshelfBook.Status == Book.BookStatus.DOWNLOADED || cell.BookshelfView.BookshelfBook.Status == Book.BookStatus.ISUPDATE)
				{
					ItemPressedEvent(cell.BookshelfView.BookshelfBook);
				}
			}
		}

		public override bool ShouldSelectItem(UICollectionView collectionView, NSIndexPath indexPath)
		{
			return true;
		}

		public void UpdateStatus(String bookID, bool updating)
		{
			if (indexDictionary != null)
			{
				var item = indexDictionary.Where(i => i.Value.BookshelfBook.ID == bookID).FirstOrDefault();
				if (item.Value != null)
				{
					item.Value.UpdateStatus(updating);
				}
			}
		}

		public void Waiting2Update(String bookID)
		{
			if (indexDictionary != null)
			{
				var item = indexDictionary.Where(i => i.Value.BookshelfBook.ID == bookID).FirstOrDefault();
				if (item.Value != null)
				{
					item.Value.Wait2Update();
				}
			}
		}

		public void UpdateProgressView(String bookID, float progress)
		{
			if (indexDictionary != null)
			{
				var item = indexDictionary.Where(i => i.Value.BookshelfBook.ID == bookID).FirstOrDefault();
				if (item.Value != null)
				{
					item.Value.UpdateProgressView(progress);
				}
			}
		}

		public void UpdateImage(String url)
		{
			if (indexDictionary != null)
			{
				var item = indexDictionary.Where(i => i.Value.BookshelfBook.LargeImageURL == url).FirstOrDefault();
				if (item.Value != null)
				{
					item.Value.UpdateImage(url);
				}
			}
		}

		public void FinishedDownloading(String bookID)
		{
			if (indexDictionary != null)
			{
				var item = indexDictionary.Where(i => i.Value.BookshelfBook.ID == bookID).FirstOrDefault();
				if (item.Value != null)
				{
					item.Value.FinishedDownloading();
				}
			}
		}

		public void UpdateFavorite(String bookID, bool isFavorite)
		{
			if (indexDictionary != null)
			{
				var item = indexDictionary.Where(i => i.Value.BookshelfBook.ID == bookID).FirstOrDefault();
				if (item.Value != null)
				{
					item.Value.UpdateFavorite(isFavorite);
				}
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
							list.Sort((x, y) => x.UserAddedDate.CompareTo(y.UserAddedDate));
						}
						else
						{
							list.Sort((x, y) => y.UserAddedDate.CompareTo(x.UserAddedDate));
						}
					}
					else
					{
						if (Settings.SortAscending)
						{
							list.Sort((x, y) => x.UserModifiedDate.CompareTo(y.UserModifiedDate));
						}
						else
						{
							list.Sort((x, y) => y.UserModifiedDate.CompareTo(x.UserModifiedDate));
						}
					}
				}
				catch (Exception ex)
				{
					Logger.WriteLineDebugging("BookshelfDataSource - SortBooks: {0}", ex.ToString());
				}
			}

			return list;
		}

		public BookshelfBookView GetBookshelfBookView(UICollectionView collectionView, String bookID)
		{
			var indexPath = GetIndexPath(bookID);
			if (indexPath != null)
			{
				var cell = (BookshelfCell)collectionView.CellForItem(indexPath);
				return cell.BookshelfView;
			}

			return null;
		}

		public NSIndexPath GetIndexPath(String bookID)
		{
			if (indexDictionary != null)
			{
				var item = indexDictionary.Where(i => i.Value.BookshelfBook.ID == bookID).FirstOrDefault();
				if (item.Value != null)
				{
					return item.Key;
				}
			}

			return null;
		}

		public int GetBookIndex(String bookID)
		{
			var item = BookList.Where(i => i.ID == bookID).FirstOrDefault();
			if (item != null)
			{
				return BookList.IndexOf(item);
			}

			return -1;
		}

		public void HandleUpdateBookEvent(Book book)
		{
			if (UpdateBookEvent != null)
			{
				UpdateBookEvent(book);
			}
		}

		public void HandleRefreshBookshelfEvent()
		{
			if (RefreshBookshelfEvent != null)
			{
				RefreshBookshelfEvent();
			}
		}

		public void HandleShowMenuEvent(BookshelfBookView bookView, bool isFavorite)
		{
			if (ShowMenuEvent != null)
			{
				ShowMenuEvent(bookView, isFavorite);
			}
		}

		private void UpdateDictionary()
		{
			if ( indexDictionary != null )
			{
				indexDictionary.Clear ();
			}
			int row = 0;
			foreach (Book book in BookList)
			{
				BookshelfBookView bookView = new BookshelfBookView(book, updateMenu, parentVC);
				indexDictionary.Add(NSIndexPath.FromRowSection(row, 0), bookView);
				row++;
			}
		}
	}
	#region BookshelfCell
	public class BookshelfCell : UICollectionViewCell
	{
		private BookshelfBookView bookshelfBookView;

		public BookshelfDataSource dataSource { get; set; }

		public BookshelfBookView BookshelfView
		{
			get
			{
				return bookshelfBookView;
			}
			set
			{
				bookshelfBookView = value;
				bookshelfBookView.UpdateBookEvent += HandleUpdateBookEvent;
				bookshelfBookView.RefreshBookshelfEvent += HandleRefreshBookshelfEvent;
				bookshelfBookView.ShowMenuEvent += HandleShowMenuEvent;
				ContentView.AddSubview(bookshelfBookView);
			}
		}

		[Export("initWithFrame:")]
		public BookshelfCell(CGRect frame) : base(frame)
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

		void HandleUpdateBookEvent(Book book)
		{
			dataSource.HandleUpdateBookEvent(book);
		}

		void HandleRefreshBookshelfEvent()
		{
			dataSource.HandleRefreshBookshelfEvent();
		}

		void HandleShowMenuEvent(BookshelfBookView bookView, bool isFavorite)
		{
			dataSource.HandleShowMenuEvent(bookView, isFavorite);
		}
	}
	#endregion
}

