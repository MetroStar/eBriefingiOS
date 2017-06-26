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
    public class PopoverSettingController : UITableViewController
    {
        public delegate void PopoverSettingDelegate0 ();

        public delegate void PopoverSettingDelegate1 (NSIndexPath indexPath);

        public event PopoverSettingDelegate0 SyncOnEvent;
		public event PopoverSettingDelegate0 SyncOffEvent;
        public event PopoverSettingDelegate1 RowSelectedEvent;

        public PopoverSettingController(UITableViewStyle style) : base(style)
        {

        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.ContentInset = new UIEdgeInsets(-10, 0, 0, 0);
            TableView.Source = new PopoverSettingDataSource(this);
            TableView.ScrollEnabled = false;
        }

        public class PopoverSettingDataSource : UITableViewSource
        {
            private PopoverSettingController parent;
            private UISwitch syncSwitch;

            public PopoverSettingDataSource(PopoverSettingController parent)
            {
                this.parent = parent;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return 2;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 44;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (section == 0)
                {
                    return 2;
                }
                else
                {
                    return 4;
                }
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (section == 0)
                {
                    return "Account";
                }
                else
                {
                    return "What is eBriefing?";
                }
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                UITableViewCell cell = tableView.DequeueReusableCell("PopoverSettingIdentifier");

                try
                {
                    if (cell == null)
                    {
                        cell = new UITableViewCell(UITableViewCellStyle.Default, "PopoverSettingIdentifier");
                    }

                    cell.Accessory = UITableViewCellAccessory.None;
                    cell.SelectionStyle = UITableViewCellSelectionStyle.Default;

                    if (indexPath.Section == 0 && indexPath.Row == 1)
                    {
                        cell.SelectionStyle = UITableViewCellSelectionStyle.None;

                        cell.TextLabel.Text = "Content Sync";
                        cell.ImageView.Image = UIImage.FromBundle("Assets/Icons/sync.png");

                        if (syncSwitch == null)
                        {
                            syncSwitch = new UISwitch();
                            syncSwitch.Frame = new CGRect(tableView.Frame.Size.Width - 20 - syncSwitch.Frame.Width, 22f - (syncSwitch.Frame.Height / 2f), syncSwitch.Frame.Width, syncSwitch.Frame.Height);
                            syncSwitch.ValueChanged += (object sender, EventArgs e) =>
                            {
                                Settings.WriteSyncOn(syncSwitch.On);

                                if (Settings.SyncOn)
                                {
                                    if (parent.SyncOnEvent != null)
                                    {
                                        parent.SyncOnEvent();
                                    }
                                }
								else
								{
									if (parent.SyncOffEvent != null)
									{
										parent.SyncOffEvent();
									}
								}
                            };

                            // Disable syncing for Demo server
                            syncSwitch.SetState(Settings.SyncOn, false);

                            if (Reachability.IsDefaultNetworkAvailable() && !URL.ServerURL.Contains(StringRef.DemoURL))
                            {
                                syncSwitch.Enabled = true;
                            }
                            else
                            {
                                syncSwitch.Enabled = false;
                            }
                        }
                        cell.ContentView.AddSubview(syncSwitch);
                    }
                    else
                    {
                        if (indexPath.Section == 0)
                        {
                            cell.TextLabel.Text = "Library Setting";
                            cell.ImageView.Image = UIImage.FromBundle("Assets/Icons/server.png");
                        }
                        else
                        {
                            if (indexPath.Row == 0)
                            {
                                cell.TextLabel.Text = "About eBriefing";
                                cell.ImageView.Image = UIImage.FromBundle("Assets/Icons/about.png");
                            }
                            else if (indexPath.Row == 1)
                            {
                                cell.TextLabel.Text = "Tutorial";
                                cell.ImageView.Image = UIImage.FromBundle("Assets/Icons/tutorial.png");
                            }
//                            else if (indexPath.Row == 2)
//                            {
//                                cell.TextLabel.Text = "Privacy Policy";
//                                cell.ImageView.Image = UIImage.FromBundle("Assets/Icons/privacy.png");
//                            }
                            else if (indexPath.Row == 2)
                            {
                                cell.TextLabel.Text = "Give Feedback";
                                cell.ImageView.Image = UIImage.FromBundle("Assets/Icons/feedback.png");
                            }
                            else
                            {
                                cell.TextLabel.Text = "Rate This App";
                                cell.ImageView.Image = UIImage.FromBundle("Assets/Icons/rate.png");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLineDebugging("PopoverSettingDataSource - GetCell: {0}", ex.ToString());
                }

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow(indexPath, true);

                if (parent.RowSelectedEvent != null)
                {
                    parent.RowSelectedEvent(indexPath);
                }
            }
        }
    }
}

