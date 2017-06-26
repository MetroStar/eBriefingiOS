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
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreGraphics;

namespace eBriefingMobile
{
	public class DashboardDataSource : UICollectionViewSource
	{
		public enum DataType
		{
			CHAPTERS,
			BOOKMARKS,
			ANNOTATIONS
		}

		private NSString cellID;
		private Book book;
		private Object list;
		private DataType dataType;
		private Dictionary<String, NSIndexPath> indexDictionary;

		public delegate void DashboardDataSourceDelegate0(UIScrollView scrollView);

		public delegate void DashboardDataSourceDelegate1(String pageID);

		public event DashboardDataSourceDelegate0 ScrolledEvent;
		public event DashboardDataSourceDelegate1 ItemPressedEvent;

		public DashboardDataSource(Book book, Object list, DataType dataType, String cellID)
		{
			this.book = book;
			this.list = list;
			this.dataType = dataType;
			this.cellID = new NSString(cellID);

			indexDictionary = new Dictionary<String, NSIndexPath>();
		}

		public override nint NumberOfSections(UICollectionView collectionView)
		{
			return 1;
		}

		public override nint GetItemsCount(UICollectionView collectionView, nint section)
		{
			if (list != null)
			{
				if (dataType == DataType.CHAPTERS)
				{
					return (list as List<Chapter>).Count;
				}
				else if (dataType == DataType.BOOKMARKS)
				{
					return (list as List<Bookmark>).Count;
				}
				else
				{
					return (list as List<Annotation>).Count;
				}
			}

			return 0;
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			if (dataType == DataType.CHAPTERS)
			{
				var cell = (ChapterCell)collectionView.DequeueReusableCell(cellID, indexPath);

				var chapter = (list as List<Chapter>)[indexPath.Row] as Chapter;
				if (chapter != null)
				{
					ChapterView chapterView = new ChapterView(book.ID, chapter, indexPath.Row);
					cell.PageID = chapter.FirstPageID;
					cell.ChapterImage = chapterView;

					indexDictionary[chapter.ID] = indexPath;
				}
				return cell;
			}
			else
			{
				var cell = (ThumbnailCell)collectionView.DequeueReusableCell(cellID, indexPath);

				if (dataType == DataType.BOOKMARKS)
				{
					var bookmark = (list as List<Bookmark>)[indexPath.Row] as Bookmark;
					if (bookmark != null)
					{
						ThumbnailView thumbnailView = new ThumbnailView(bookmark, new CGRect(0, 0, 171, 221));
						cell.PageID = bookmark.PageID;
						cell.ThumbnailImage = thumbnailView;

						indexDictionary[bookmark.PageID] = indexPath;
					}
				}
				else
				{
					var annotation = (list as List<Annotation>)[indexPath.Row] as Annotation;
					if (annotation != null)
					{
						ThumbnailView thumbnailView = new ThumbnailView(annotation, new CGRect(0, 0, 171, 221));
						cell.PageID = annotation.PageID;
						cell.ThumbnailImage = thumbnailView;

						indexDictionary[annotation.PageID] = indexPath;
					}
				}
				return cell;
			}
		}

		public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			if (ItemPressedEvent != null)
			{
				if (dataType == DataType.CHAPTERS)
				{
					var cell = (ChapterCell)collectionView.CellForItem(indexPath);
					ItemPressedEvent(cell.PageID);
				}
				else
				{
					var cell = (ThumbnailCell)collectionView.CellForItem(indexPath);
					ItemPressedEvent(cell.PageID);
				}
			}
		}

		public override bool ShouldSelectItem(UICollectionView collectionView, NSIndexPath indexPath)
		{
			return true;
		}

		public override void Scrolled(UIScrollView scrollView)
		{
			if (ScrolledEvent != null)
			{
				ScrolledEvent(scrollView);
			}
		}

		public void Sort(UICollectionView collectionView)
		{
			if (dataType == DataType.CHAPTERS)
			{
				(list as List<Chapter>).Reverse();

				collectionView.PerformBatchUpdates(delegate
				{
					for (int i = 0; i < (list as List<Chapter>).Count; i++)
					{
						NSIndexPath fromIndexPath = indexDictionary[(list as List<Chapter>)[i].ID];
						NSIndexPath toIndexPath = NSIndexPath.FromRowSection(i, 0);
						collectionView.MoveItem(fromIndexPath, toIndexPath);   
					}
				}, delegate
				{
					collectionView.ReloadData();
				});
			}
			else if (dataType == DataType.BOOKMARKS)
			{
				(list as List<Bookmark>).Reverse();

				collectionView.PerformBatchUpdates(delegate
				{
					for (int i = 0; i < (list as List<Bookmark>).Count; i++)
					{
						NSIndexPath fromIndexPath = indexDictionary[(list as List<Bookmark>)[i].PageID];
						NSIndexPath toIndexPath = NSIndexPath.FromRowSection(i, 0);
						collectionView.MoveItem(fromIndexPath, toIndexPath);   
					}
				}, delegate
				{
					collectionView.ReloadData();
				});
			}
			else
			{
				(list as List<Annotation>).Reverse();

				collectionView.PerformBatchUpdates(delegate
				{
					for (int i = 0; i < (list as List<Annotation>).Count; i++)
					{
						NSIndexPath fromIndexPath = indexDictionary[(list as List<Annotation>)[i].PageID];
						NSIndexPath toIndexPath = NSIndexPath.FromRowSection(i, 0);
						collectionView.MoveItem(fromIndexPath, toIndexPath);   
					}
				}, delegate
				{
					collectionView.ReloadData();
				});
			}
		}
	}
}

