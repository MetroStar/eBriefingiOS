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
using PSPDFKit;
using SpinKitBinding;

namespace eBriefingMobile
{
    public class ThumbnailView : UICollectionReusableView
    {
        public ThumbnailView(Bookmark bookmark, CGRect frame) : base(frame)
        {
            Initialize(frame);

            GenerateThumbnail(bookmark.BookID, bookmark.PageID, bookmark.Title, true);
        }

        public ThumbnailView(Note note, CGRect frame) : base(frame)
        {
            Initialize(frame);

            GenerateThumbnail(note.BookID, note.PageID, note.Text);
        }

        public ThumbnailView(Annotation annotation, CGRect frame) : base(frame)
        {
            Initialize(frame);

            GenerateThumbnail(annotation.BookID, annotation.PageID, String.Empty);
        }

        private void Initialize(CGRect frame)
        {
            this.BackgroundColor = eBriefingAppearance.Gray2;
            this.Frame = frame;
        }

        async private void GenerateThumbnail(String bookID, String pageID, String text, bool bookmark = false)
        {
            try
            {
                // spinner
                RTSpinKitView spinner = eBriefingAppearance.GenerateBounceSpinner();
                spinner.Center = this.Center;
                this.AddSubview(spinner);

                // Generate pdf thumbnail
                Page page = await eBriefingService.Run(() => BooksOnDeviceAccessor.GetPage(bookID, pageID));

                Annotation annotation = null;
                if (String.IsNullOrEmpty(text))
                {
                    annotation = await eBriefingService.Run(() => BooksOnDeviceAccessor.GetAnnotation(bookID, pageID));
                }

                spinner.StopAnimating();

                if (page != null)
                {
                    String localPath = DownloadedFilesCache.BuildCachedFilePath(page.URL);
                    if (!String.IsNullOrEmpty(localPath))
                    {
                        CGPDFDocument pdfDoc = CGPDFDocument.FromFile(localPath);
                        if (pdfDoc != null)
                        {
                            CGPDFPage pdfPage = pdfDoc.GetPage(1);
                            if (pdfPage != null)
                            {
								UIImage pdfImg = ImageHelper.PDF2Image(pdfPage, this.Frame.Width, UIScreen.MainScreen.Scale);

                                // pageView
                                UIImageView pageView = new UIImageView();
                                pageView.Frame = new CGRect(0, (this.Frame.Height / 2) - (pdfImg.Size.Height / 2), pdfImg.Size.Width, pdfImg.Size.Height);

                                // If this is annotation thumbnail, draw annotation overlay on top of pdf
                                if (annotation != null)
                                {
                                    Dictionary<String, PSPDFInkAnnotation> dictionary = AnnotationsDataAccessor.GenerateAnnDictionary((nuint)page.PageNumber - 1, annotation);
                                    if (dictionary != null)
                                    {
                                        foreach (KeyValuePair<String, PSPDFInkAnnotation> item in dictionary)
                                        {
                                            // Create full size annotation
                                            UIImage annImg = ImageHelper.DrawPSPDFAnnotation(item.Key, item.Value);

                                            if (annImg != null)
                                            {
                                                // Scale down the annotation image
                                                annImg = annImg.Scale(new CGSize(pdfImg.Size.Width, pdfImg.Size.Height));

                                                // Overlap pdfImg and annImg
												pdfImg = ImageHelper.Overlap(pdfImg, annImg, CGPoint.Empty, CGPoint.Empty, UIScreen.MainScreen.Scale);
                                            }
                                        }
                                    }
                                }

                                pageView.Image = pdfImg;
                                this.AddSubview(pageView);

                                if (pdfImg.Size.Height < this.Frame.Height)
                                {
                                    this.BackgroundColor = UIColor.Clear;
                                }

                                // THIS IS REQUIRED TO SKIP iCLOUD BACKUP
                                SkipBackup2iCloud.SetAttribute(localPath);

                                // Add ribbon if this is bookmark thumbnail
                                if (bookmark)
                                {
                                    UIImageView ribbon = new UIImageView();
                                    ribbon.Image = UIImage.FromBundle("Assets/Buttons/bookmark_solid.png");
                                    ribbon.Frame = new CGRect(pageView.Frame.Right - 35, pageView.Frame.Y, 25, 33.78f);
                                    this.AddSubview(ribbon);
                                }

                                // Do not add text if this is annotation thumbnail
                                if (!String.IsNullOrEmpty(text))
                                {
                                    // titleLabel
                                    UILabel titleLabel = eBriefingAppearance.GenerateLabel(16);
                                    titleLabel.Frame = new CGRect(0, pageView.Frame.Bottom + 4, this.Frame.Width, 42);
                                    titleLabel.Lines = 2;
                                    titleLabel.LineBreakMode = UILineBreakMode.TailTruncation;
                                    titleLabel.TextAlignment = UITextAlignment.Center;
                                    titleLabel.Text = text;
                                    this.AddSubview(titleLabel);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("ThumbnailView - GenerateThumbnail: {0}", ex.ToString());
            }
        }
    }
}

