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
	public class PopoverPageModeController : UITableViewController
	{
		public delegate void PageModeDelegate();

		public event PageModeDelegate PageModeEvent;

		public PopoverPageModeController() : base(UITableViewStyle.Plain)
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
			TableView.Source = new PopoverPageModeDataSource(this);
			TableView.ScrollEnabled = false;
		}

		public class PopoverPageModeDataSource : UITableViewSource
		{
			private PopoverPageModeController parent;

			public PopoverPageModeDataSource(PopoverPageModeController parent)
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
				return 2;
			}

			public override nfloat GetHeightForFooter(UITableView tableView, nint section)
			{
				return 0.001f;
			}

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				UITableViewCell cell = tableView.DequeueReusableCell("PopoverPageModeIdentifier");

				try
				{
					if (cell == null)
					{
						cell = new CustomCell(UITableViewCellStyle.Default, "PopoverPageModeIdentifier");
					}

					cell.Accessory = UITableViewCellAccessory.None;
					cell.SelectionStyle = UITableViewCellSelectionStyle.Default;

					if (indexPath.Row == 0)
					{
						cell.TextLabel.Text = StringRef.Single;
						cell.ImageView.Image = UIImage.FromBundle("Assets/Icons/page_single.png");

						if (Settings.PageMode == StringRef.Single)
						{
							cell.AccessoryView = eBriefingAppearance.Checkmark;
						}
						else
						{
							cell.AccessoryView = null;
						}
					}
					else
					{
						cell.TextLabel.Text = StringRef.Double;
						cell.ImageView.Image = UIImage.FromBundle("Assets/Icons/page_double.png");

						if (Settings.PageMode == StringRef.Double)
						{
							cell.AccessoryView = eBriefingAppearance.Checkmark;
						}
						else
						{
							cell.AccessoryView = null;
						}
					}
				}
				catch (Exception ex)
				{
					Logger.WriteLineDebugging("PopoverPageModeDataSource - GetCell: {0}", ex.ToString());
				}

				return cell;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				tableView.DeselectRow(indexPath, true);

				if (indexPath.Row == 0)
				{
					Settings.WritePageMode(StringRef.Single);
				}
				else if (indexPath.Row == 1)
				{
					Settings.WritePageMode(StringRef.Double);
				}
				else
				{
					Settings.WritePageMode(StringRef.Continuous);
				}

				parent.PageModeEvent();
			}
		}
	}
}

