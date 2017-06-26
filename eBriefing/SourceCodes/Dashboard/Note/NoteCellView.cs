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

namespace eBriefingMobile
{
    public class NoteCellView : UIView
    {
        public String pageID { get; set; }

        public Note note { get; set; }

        public delegate void NoteCellViewDelegate (String pageID, Note note);

        public event NoteCellViewDelegate ItemPressedEvent;

        public NoteCellView(String bookID, Note note)
        {
            this.BackgroundColor = UIColor.Clear;
            this.pageID = pageID;
            this.note = note;

            nfloat height = 300;

            Page page = BooksOnDeviceAccessor.GetPage(bookID, note.PageID);
            if (note.Text == "Header: " + note.PageID)
            {
                this.Frame = new CGRect(0, 0, 200, height);

                // pageView
                UIImageView pageView = new UIImageView();
                pageView.Frame = new CGRect(15, 20, 170, height - 80);
                this.AddSubview(pageView);

                bool notFound = true;
                if (page != null)
                {
                    String localPath = DownloadedFilesCache.BuildCachedFilePath(page.URL);
                    CGPDFDocument pdfDoc = CGPDFDocument.FromFile(localPath);
                    if (pdfDoc != null)
                    {
                        notFound = false;

                        CGPDFPage pdfPage = pdfDoc.GetPage(1);
						UIImage pageImage = ImageHelper.PDF2Image(pdfPage, pageView.Frame.Width, UIScreen.MainScreen.Scale);
                        pageView.Image = pageImage;
                    }
                }

                if (notFound)
                {
                    pageView.Image = UIImage.FromBundle("Assets/Icons/empty_page.png");

                    // emptyLabel
                    UILabel emptyLabel = eBriefingAppearance.GenerateLabel(17);
                    emptyLabel.Frame = pageView.Frame;
                    emptyLabel.Text = "Empty";
                    emptyLabel.TextAlignment = UITextAlignment.Center;
                    emptyLabel.SizeToFit();
                    emptyLabel.Center = pageView.Center;
                    this.AddSubview(emptyLabel);
                }

                // pageView
                NotePageView circleView = new NotePageView(page, pageView.Frame.Width / 2);
                circleView.Center = new CGPoint(pageView.Center.X, pageView.Frame.Bottom);
                this.AddSubview(circleView);
            }
            else if (note.Text == "Footer: " + note.PageID)
            {
                this.Frame = new CGRect(0, 0, 50, height);

                // footerView
//				UIImageView footerView = new UIImageView(UIImage.FromBundle("Assets/Icons/endPageNote.png"));
//				footerView.Frame = new CGRect(15, ((height - 40) / 2 - 10), 20, 20);
//                this.AddSubview(footerView);
            }
            else
            {
                this.Frame = new CGRect(0, 0, 250, height);

                // noteView
                NoteCellNoteView noteView = new NoteCellNoteView(note, height - 80);
                noteView.Frame = new CGRect(15, 20, noteView.Frame.Width, noteView.Frame.Height);
                noteView.TouchUpInside += delegate
                {
                    if (ItemPressedEvent != null)
                    {
                        ItemPressedEvent(page.ID, note);
                    }
                };
                this.AddSubview(noteView);
            }
        }
    }
}

