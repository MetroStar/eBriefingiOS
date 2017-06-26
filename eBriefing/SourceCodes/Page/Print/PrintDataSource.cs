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
	public class PrintDataSource : UITableViewSource
	{
		private PrintPanel parent;
		private UISwitch annSwitch;
		private UISwitch noteSwitch;
		private PrintRangeCellView cellView;

		private static nuint MAX_PRINT_RANGE = 10;

		public PrintDataSource(PrintPanel parent)
		{
			this.parent = parent;
		}

		public override nint NumberOfSections(UITableView tableView)
		{
			return 3;
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
			if (section == 1)
			{
				return 30;
			}

			return 0.001f;
		}

		public override String TitleForHeader(UITableView tableView, nint section)
		{
			if (section == 0)
			{
				return "ORIENTATION";
			}
			else if (section == 1)
			{
				return "PRINT RANGE";
			}
			else if (section == 2)
			{
				return "ANNOTATIONS & NOTES";
			}

			return String.Empty;
		}

		public override string TitleForFooter(UITableView tableView, nint section)
		{
//			if (section == 1)
//			{
//				return "Printing is currently limited to " + MAX_PRINT_RANGE.ToString() + " pages";
//			}

			return String.Empty;
		}

		public override UIView GetViewForFooter(UITableView tableView, nint section)
		{
			String title = TitleForFooter(tableView, section);

			if (!String.IsNullOrEmpty(title))
			{
				// background
				UIView headerView = new UIView();
				headerView.Frame = new CGRect(15, 0, tableView.Frame.Size.Width, 30);
				headerView.BackgroundColor = UIColor.Clear;

				// header
				UILabel header = new UILabel();
				header.Frame = headerView.Frame;
				header.BackgroundColor = UIColor.Clear;
				header.Font = eBriefingAppearance.ThemeItalicFont(14f);
				header.TextColor = eBriefingAppearance.Gray2;
				header.Text = title;
				headerView.AddSubview(header);

				return headerView;
			}

			return null;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell("PrintIdentifier");

			try
			{
				if (cell == null)
				{
					cell = new CustomCell(UITableViewCellStyle.Default, "PrintIdentifier");
				}
				else
				{
					foreach (UIView subview in cell.ContentView)
					{
						if (subview.Tag == -1)
						{
							subview.RemoveFromSuperview();
						}
					}
				}

				cell.Accessory = UITableViewCellAccessory.None;
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;

				if (indexPath.Section == 0)
				{
					// Orientation
					if (indexPath.Row == 0)
					{
						cell.TextLabel.Text = "Portrait";
					}
					else
					{
						cell.TextLabel.Text = "Landscape";
					}

					if (indexPath.Row == 0)
					{
						if (parent.Orientation == PrintHelper.ORIENTATION.PORTRAIT)
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
						if (parent.Orientation == PrintHelper.ORIENTATION.LANDSCAPE)
						{
							cell.AccessoryView = eBriefingAppearance.Checkmark;
						}
						else
						{
							cell.AccessoryView = null;
						}
					}
				}
				else if (indexPath.Section == 1)
				{
					// Range
					if (indexPath.Row == 0)
					{
						cell.TextLabel.Text = "Current Page";

						if (parent.Range == PrintHelper.RANGE.CURRENT)
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
						cell.TextLabel.Text = "Pages:";

						if (parent.Range == PrintHelper.RANGE.CUSTOM)
						{
							cell.AccessoryView = eBriefingAppearance.Checkmark;
						}
						else
						{
							cell.AccessoryView = null;
						}

						cellView = new PrintRangeCellView(parent.StartPage, parent.EndPage, MAX_PRINT_RANGE);
						cellView.Tag = -1;
						cellView.Frame = new CGRect(70, 0, cellView.Frame.Width, cellView.Frame.Height);
						cellView.UpdateStartEvent += (nuint start) =>
						{
							parent.StartPage = start;

						};
						cellView.UpdateEndEvent += (nuint end) =>
						{
							parent.EndPage = end;
						};
						cellView.ApplyEvent += () =>
						{
							parent.Range = PrintHelper.RANGE.CUSTOM;
							tableView.SelectRow(indexPath, true, UITableViewScrollPosition.None);
							tableView.ReloadData();
						};
		
						cell.ContentView.AddSubview(cellView);
					}
				}
				else if (indexPath.Section == 2)
				{
					cell.AccessoryView = null;

					// Annotations
					if (indexPath.Row == 0)
					{
						cell.TextLabel.Text = "Print Annotations";

						if (annSwitch == null)
						{
							annSwitch = new UISwitch();
							annSwitch.Frame = new CGRect(tableView.Frame.Size.Width - 20 - annSwitch.Frame.Width, 22f - (annSwitch.Frame.Height / 2f), annSwitch.Frame.Width, annSwitch.Frame.Height);
							annSwitch.ValueChanged += (object sender, EventArgs e) =>
							{
								if (annSwitch.On)
								{
									parent.Annotation = PrintHelper.ANNOTATION.WITH;
								}
								else
								{
									parent.Annotation = PrintHelper.ANNOTATION.WITHOUT;
								}
							};
						}
						cell.ContentView.AddSubview(annSwitch);
					}
					else
					{
						cell.TextLabel.Text = "Print Notes";

						if (noteSwitch == null)
						{
							noteSwitch = new UISwitch();
							noteSwitch.Frame = new CGRect(tableView.Frame.Size.Width - 20 - noteSwitch.Frame.Width, 22f - (noteSwitch.Frame.Height / 2f), noteSwitch.Frame.Width, noteSwitch.Frame.Height);
							noteSwitch.ValueChanged += (object sender, EventArgs e) =>
							{
								if (noteSwitch.On)
								{
									parent.Note = PrintHelper.NOTE.WITH;
								}
								else
								{
									parent.Note = PrintHelper.NOTE.WITHOUT;
								}
							};
						}
						cell.ContentView.AddSubview(noteSwitch);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("PrintDataSource - GetCell: {0}", ex.ToString());
			}

			return cell;
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			tableView.DeselectRow(indexPath, true);

			if (indexPath.Section == 0)
			{
				if (indexPath.Row == 0)
				{
					parent.Orientation = PrintHelper.ORIENTATION.PORTRAIT;
				}
				else
				{
					parent.Orientation = PrintHelper.ORIENTATION.LANDSCAPE;
				}
			}
			else if (indexPath.Section == 1)
			{
				if (indexPath.Row == 0)
				{
					parent.Range = PrintHelper.RANGE.CURRENT;
				}
				else
				{
					parent.Range = PrintHelper.RANGE.CUSTOM;
				}
			}

			tableView.ReloadSections(NSIndexSet.FromIndex(indexPath.Section), UITableViewRowAnimation.None);
		}

		public void DismissKeyboard()
		{
			if ( cellView != null )
			{
				if ( cellView.StartField.IsFirstResponder )
				{
					cellView.StartField.ResignFirstResponder();
				}
				else
				{
					cellView.EndField.ResignFirstResponder();
				}
			}
		}
	}
}

