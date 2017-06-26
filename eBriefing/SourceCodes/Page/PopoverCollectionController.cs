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
using Metrostar.Mobile.Framework;

namespace eBriefingMobile
{
	public class PopoverCollectionController : UITableViewController
	{
		public delegate void RowSelectedDelegate(NSIndexPath indexPath);

		public event RowSelectedDelegate RowSelectedEvent;

		public PopoverCollectionController() : base(UITableViewStyle.Plain)
		{

		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			UIStringAttributes stringAttributes = new UIStringAttributes();
			stringAttributes.StrokeColor = eBriefingAppearance.Gray3;
			stringAttributes.Font = eBriefingAppearance.ThemeRegularFont(17f);
			this.NavigationController.NavigationBar.TitleTextAttributes = stringAttributes;

			TableView.ContentInset = new UIEdgeInsets(10, 0, 10, 0);
			TableView.Source = new PopoverCollectioneDataSource(this);
			TableView.ScrollEnabled = false;
		}

		public class PopoverCollectioneDataSource : UITableViewSource
		{
			private PopoverCollectionController parent;

			public PopoverCollectioneDataSource(PopoverCollectionController parent)
			{
				this.parent = parent;
			}

			public override nint NumberOfSections(UITableView tableView)
			{
				return 1;
			}

			public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
			{
				return 44;
			}

			public override nint RowsInSection(UITableView tableview, nint section)
			{
				return 5;
			}

			public override nfloat GetHeightForFooter(UITableView tableView, nint section)
			{
				return 0.001f;
			}

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				UITableViewCell cell = tableView.DequeueReusableCell("PopoverCollectionIdentifier");

				try
				{
					if (cell == null)
					{
						cell = new CustomCell(UITableViewCellStyle.Default, "PopoverCollectionIdentifier");
					}

					cell.Accessory = UITableViewCellAccessory.None;
					cell.SelectionStyle = UITableViewCellSelectionStyle.Default;

					if (indexPath.Row == 0)
					{
						cell.TextLabel.Text = "All Pages";
						cell.ImageView.Image = UIImage.FromBundle("Assets/Icons/menu_allpages.png");
					}
					else if (indexPath.Row == 1)
					{
						cell.TextLabel.Text = "Table of Contents";
						cell.ImageView.Image = UIImage.FromBundle("Assets/Icons/menu_toc.png");
					}
					else if (indexPath.Row == 2)
					{
						cell.TextLabel.Text = "Bookmarks";
						cell.ImageView.Image = UIImage.FromBundle("Assets/Icons/menu_bookmarks.png");
					}
					else if (indexPath.Row == 3)
					{
						cell.TextLabel.Text = "Notes";
						cell.ImageView.Image = UIImage.FromBundle("Assets/Icons/menu_notes.png");
					}
					else
					{
						cell.TextLabel.Text = "Annotations";
						cell.ImageView.Image = UIImage.FromBundle("Assets/Icons/menu_annotations.png");
					}
				}
				catch (Exception ex)
				{
					Logger.WriteLineDebugging("PopoverCollectioneDataSource - GetCell: {0}", ex.ToString());
				}

				return cell;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				tableView.DeselectRow(indexPath, true);

				parent.RowSelectedEvent(indexPath);
			}
		}
	}
}

