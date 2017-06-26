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
	public class BookOverviewDataSource : UITableViewSource
	{
		private List<Chapter> chapterList;

		public BookOverviewDataSource(List<Chapter> chapterList)
		{
			this.chapterList = chapterList;
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
			if (chapterList != null)
			{
				return chapterList.Count;
			}

			return 0;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell("OverviewIdentifier");

			try
			{
				if (cell == null)
				{
					cell = new UITableViewCell(UITableViewCellStyle.Default, "OverviewIdentifier");
				}

				cell.Accessory = UITableViewCellAccessory.None;
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
				cell.BackgroundColor = UIColor.Clear;

				cell.TextLabel.TextColor = eBriefingAppearance.Gray1;
				cell.TextLabel.Font = eBriefingAppearance.ThemeRegularFont(17);
				cell.TextLabel.BackgroundColor = UIColor.Clear;
				cell.TextLabel.Text = "Chapter " + (indexPath.Row + 1).ToString() + " : " + chapterList[indexPath.Row].Title;
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("BookOverviewDataSource - GetCell: {0}", ex.ToString());
			}

			return cell;
		}
	}
}

