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
using System.ComponentModel;
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreGraphics;
using Metrostar.Mobile.Framework;

namespace eBriefingMobile
{
    public class CollectionDataSource : UICollectionViewSource
    {
        private NSString cellID;
        private List<Page> list;

        public delegate void ItemPressedDelegate (String pageID);

        public event ItemPressedDelegate ItemPressedEvent;

        public CollectionDataSource(NSString cellID, List<Page> list)
        {
            this.cellID = cellID;
            this.list = list;

            if (list != null)
            {
                if (list.Count == 0)
                {
                    Page page = new Page();
                    page.ID = StringRef.Empty;
                    page.ChapterID = StringRef.Empty;
                    list.Add(page);
                }
                else
                {
                    try
                    {
                        // Sort based on page number
                        list.Sort((x, y) => x.PageNumber.CompareTo(y.PageNumber));
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLineDebugging("CollectionDataSource - CollectionDataSource: {0}", ex.ToString());
                    }
                }
            }
        }

        public override nint NumberOfSections(UICollectionView collectionView)
        {
            return 1;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            if (list != null)
            {
                return list.Count;
            }

            return 0;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = (PageCell)collectionView.DequeueReusableCell(cellID, indexPath);

            var page = list[indexPath.Row];
            if (page != null)
            {
                cell.PageID = page.ID;

                if (page.ID != StringRef.Empty || page.ChapterID != StringRef.Empty)
                {
                    String url = DownloadedFilesCache.BuildCachedFilePath(page.URL);
                    if (!String.IsNullOrEmpty(url))
                    {
                        CGPDFDocument pdfDoc = CGPDFDocument.FromFile(url);
                        if (pdfDoc != null)
                        {
                            CGPDFPage pdfPage = pdfDoc.GetPage(1);

							UIImage image = ImageHelper.PDF2Image(pdfPage, 150, UIScreen.MainScreen.Scale);

                            // THIS IS REQUIRED TO SKIP iCLOUD BACKUP
                            SkipBackup2iCloud.SetAttribute(url);

                            if (image != null)
                            {
                                cell.Image = image;
                            }
                        }
                    }
                }
            }

            return cell;
        }

        public override UICollectionReusableView GetViewForSupplementaryElement(UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
        {
            if (elementKind.Description.Contains(UICollectionElementKindSection.Header.ToString()))
            {
                var headerView = (PageHeader)collectionView.DequeueReusableSupplementaryView(elementKind, new NSString("pageHeader"), indexPath);

                if (cellID == "PageCell")
                {
                    headerView.Text = "All Pages";
                }
                else if (cellID == "TOCCell")
                {
                    headerView.Text = "Table of Contents";
                }
                else if (cellID == "BookmarkCell")
                {
                    headerView.Text = "Bookmarks";
                }
                else if (cellID == "NoteCell")
                {
                    headerView.Text = "Notes ";
                }
                else if (cellID == "AnnotationCell")
                {
                    headerView.Text = "Annotations";
                }

                // Add word 'Empty' when there are no pages
                if (list.Count == 1 && list[0].ID == StringRef.Empty && list[0].ChapterID == StringRef.Empty)
                {
                    headerView.Text += " " + StringRef.Empty;
                }

                return headerView;
            }

            return null;
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (ItemPressedEvent != null)
            {
                var cell = (PageCell)collectionView.CellForItem(indexPath);

                if (cell.PageID != StringRef.Empty)
                {
                    ItemPressedEvent(cell.PageID);
                }
            }
        }

        public override bool ShouldSelectItem(UICollectionView collectionView, NSIndexPath indexPath)
        {
            return true;
        }
    }

    #region PageCell
    public class PageCell : UICollectionViewCell
    {
        private UIImageView imageView;

        public String PageID { get; set; }

        public UIImage Image
        {
            set
            {
                imageView.Frame = new CGRect(0, 0, value.Size.Width, value.Size.Height);
                imageView.Center = ContentView.Center;
                imageView.Image = value;
            }
        }

        [Export("initWithFrame:")]
        public PageCell(CGRect frame) : base(frame)
        {
            BackgroundView = new UIView {
                BackgroundColor = UIColor.Clear
            };

            SelectedBackgroundView = new UIView {
                BackgroundColor = UIColor.Clear
            };

            ContentView.Layer.BorderColor = UIColor.LightGray.CGColor;
            ContentView.BackgroundColor = UIColor.Clear;

            imageView = new UIImageView(UIImage.FromBundle("Assets/Icons/empty_page.png"));
            imageView.Center = ContentView.Center;
            ContentView.AddSubview(imageView);
        }

        public override void PrepareForReuse()
        {
            base.PrepareForReuse();

            PageID = String.Empty;

            if (imageView != null)
            {
                imageView.Image = UIImage.FromBundle("Assets/Icons/empty_page.png");
            }
        }
    }
    #endregion

    #region PageHeader
    public class PageHeader : UICollectionReusableView
    {
        private UILabel headerLabel;

        [Export("initWithFrame:")]
        public PageHeader(CGRect frame) : base(frame)
        {
            this.BackgroundColor = UIColor.Clear;

            headerLabel = eBriefingAppearance.GenerateLabel(21, UIColor.White, true);
            headerLabel.Frame = frame;
            headerLabel.TextAlignment = UITextAlignment.Center;
            AddSubview(headerLabel);
        }

        public string Text
        {
            get
            {
                return headerLabel.Text;
            }
            set
            {
                headerLabel.Lines = 0;
                headerLabel.LineBreakMode = UILineBreakMode.WordWrap;
                headerLabel.Text = value;
                headerLabel.SizeToFit();
                headerLabel.Center = new CGPoint(this.Frame.Width / 2, this.Frame.Height / 2);

                SetNeedsDisplay();
            }
        }
    }
    #endregion
}

