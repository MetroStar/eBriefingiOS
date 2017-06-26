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
using Metrostar.Mobile.Framework;
using MssFramework;

namespace eBriefingMobile
{
	public class PopoverMenuController : UITableViewController
	{
		public BookshelfBookView bookView;
		public bool isFavorite;

		public delegate void PopoverMenuDelegate();

		public event PopoverMenuDelegate FavoriteEvent;
		public event PopoverMenuDelegate RemoveBookEvent;
		public event PopoverMenuDelegate CancelDownloadEvent;

		public PopoverMenuController(BookshelfBookView bookView, bool isFavorite) : base(UITableViewStyle.Plain)
		{
			this.bookView = bookView;
			this.isFavorite = isFavorite;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			TableView.ContentInset = TableView.SeparatorInset = UIEdgeInsets.Zero;
			TableView.Source = new PopoverMenuDataSource(this);
			TableView.ScrollEnabled = false;
		}

		public class PopoverMenuDataSource : UITableViewSource
		{
			private PopoverMenuController parent;

			public PopoverMenuDataSource(PopoverMenuController parent)
			{
				this.parent = parent;
			}

			public override nint NumberOfSections(UITableView tableView)
			{
				return 1;
			}

			public override nfloat GetHeightForFooter(UITableView tableView, nint section)
			{
				return 0.001f;
			}

			public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
			{
				return 44;
			}

			public override nint RowsInSection(UITableView tableview, nint section)
			{
				if (parent.bookView.BookshelfBook.Status == Book.BookStatus.DOWNLOADED)
				{
					return 2;
				}

				return 1;
			}

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				UITableViewCell cell = tableView.DequeueReusableCell("PopoverMenuIdentifier");

				try
				{
					if (cell == null)
					{
						cell = new UITableViewCell(UITableViewCellStyle.Default, "PopoverMenuIdentifier");
					}

					cell.Accessory = UITableViewCellAccessory.None;
					cell.SelectionStyle = UITableViewCellSelectionStyle.Default;

					cell.TextLabel.Font = eBriefingAppearance.ThemeRegularFont(19);
					cell.TextLabel.TextAlignment = UITextAlignment.Center;

					if (parent.bookView.BookshelfBook.Status == Book.BookStatus.DOWNLOADED)
					{
						cell.TextLabel.TextColor = eBriefingAppearance.BlueColor;

						if (indexPath.Row == 0)
						{
							if (parent.isFavorite)
							{
								cell.TextLabel.Text = "Remove from Favorite";
							}
							else
							{
								cell.TextLabel.Text = "Add to Favorite";
							}
						}
						else
						{
							cell.TextLabel.Text = "Remove from Bookshelf";
						}
					}
					else
					{
						cell.TextLabel.TextColor = UIColor.Red;
						cell.TextLabel.Text = "Cancel Download";
					}
				}
				catch (Exception ex)
				{
					Logger.WriteLineDebugging("PopoverMenuDataSource - GetCell: {0}", ex.ToString());
				}

				return cell;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				tableView.DeselectRow(indexPath, true);

				if (parent.bookView.BookshelfBook.Status == Book.BookStatus.DOWNLOADED)
				{
					if (indexPath.Row == 0)
					{
						if (parent.FavoriteEvent != null)
						{
							parent.FavoriteEvent();
						}
					}
					else
					{
						if (parent.RemoveBookEvent != null)
						{
							parent.RemoveBookEvent();
						}
					}
				}
				else
				{
					if (parent.CancelDownloadEvent != null)
					{
						parent.CancelDownloadEvent();
					}
				}
			}
		}
	}
}

