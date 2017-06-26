using System;
using System.Linq;
using System.Drawing;
using System.ComponentModel;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using Metrostar.Mobile.Framework;
using PSPDFKit;
using System.Collections.Generic;

namespace eBriefingMobile
{
    public class ThumbnailView : UIView
    {
        public ThumbnailView(Bookmark bookmark, RectangleF frame) : base(frame)
        {
            Initialize(frame);

            GenerateThumbnail(bookmark.BookID, bookmark.PageID, bookmark.Title, true);
        }

        public ThumbnailView(Note note, RectangleF frame) : base(frame)
        {
            Initialize(frame);

            GenerateThumbnail(note.BookID, note.PageID, note.Text);
        }

        public ThumbnailView(Annotation annotation, RectangleF frame) : base(frame)
        {
            Initialize(frame);

            GenerateThumbnail(annotation.BookID, annotation.PageID, String.Empty);
        }

        private void Initialize(RectangleF frame)
        {
            this.BackgroundColor = UIColor.Clear;
            this.Frame = frame;
        }

        private void GenerateThumbnail(String bookID, String pageID, String text, bool bookmark = false)
        {
            try
            {
                // activityIndicator
                UIActivityIndicatorView activityIndicator = new UIActivityIndicatorView();
                activityIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.White;
                activityIndicator.StartAnimating();
                activityIndicator.HidesWhenStopped = true;
                activityIndicator.Frame = new RectangleF((this.Frame.Width / 2) - 10, (this.Frame.Height / 2) - 10, 20, 20);
                this.AddSubview(activityIndicator);

                // Generate pdf thumbnail
                Page page = null;
                Annotation annotation = null;
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += delegate
                {
                    page = BooksOnDeviceAccessor.GetPage(bookID, pageID);

                    if (String.IsNullOrEmpty(text))
                    {
                        annotation = BooksOnDeviceAccessor.GetAnnotation(bookID, pageID);
                    }
                };
                worker.RunWorkerCompleted += delegate
                {
                    this.InvokeOnMainThread(delegate
                    {
                        activityIndicator.StopAnimating();

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
                                        UIImage pdfImg = PDFConverter.Transform2Image(pdfPage, this.Frame.Width);
                                        
                                        // pageView
                                        UIImageView pageView = new UIImageView();
                                        pageView.Frame = new RectangleF(0, (this.Frame.Height / 2) - (pdfImg.Size.Height / 2), pdfImg.Size.Width, pdfImg.Size.Height);

                                        // If this is annotation thumbnail, draw annotation overlay on top of pdf
                                        if (annotation != null)
                                        {
                                            Dictionary<String, PSPDFInkAnnotation> dictionary = AnnotationsDataAccessor.GenerateAnnDictionary((UInt32)page.PageNumber - 1, annotation);
                                            if (dictionary != null)
                                            {
                                                foreach (KeyValuePair<String, PSPDFInkAnnotation> item in dictionary)
                                                {
                                                    // Create full size annotation
                                                    UIImage annImg = DrawAnnotation(item.Key, item.Value);

                                                    if (annImg != null)
                                                    {
                                                        // Scale down the annotation image
                                                        annImg = annImg.Scale(new SizeF(pdfImg.Size.Width, pdfImg.Size.Height));
                                                        
                                                        // Overlap pdfImg and annImg
                                                        pdfImg = Overlap(pdfImg, annImg);
                                                    }
                                                }
                                            }
                                        }

                                        pageView.Image = pdfImg;
                                        this.AddSubview(pageView);

                                        // THIS IS REQUIRED TO SKIP iCLOUD BACKUP
                                        SkipBackup2iCloud.SetAttribute(localPath);

                                        // Add ribbon if this is bookmark thumbnail
                                        if (bookmark)
                                        {
                                            UIImageView ribbon = new UIImageView();
                                            ribbon.Image = UIImage.FromBundle("Assets/Buttons/bookmark_solid.png");
                                            ribbon.Frame = new RectangleF(pageView.Frame.Right - 35, pageView.Frame.Y, 25, 33.78f);
                                            this.AddSubview(ribbon);
                                        }

                                        // Do not add text if this is annotation thumbnail
                                        if (!String.IsNullOrEmpty(text))
                                        {
                                            // titleLabel
                                            UILabel titleLabel = new UILabel();
                                            titleLabel.Frame = new RectangleF(0, pageView.Frame.Bottom + 4, this.Frame.Width, 42);
                                            titleLabel.Font = UIFont.SystemFontOfSize(16f);
                                            titleLabel.BackgroundColor = UIColor.Clear;
                                            titleLabel.TextColor = eBriefingAppearance.DarkGrayColor;
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
                    });
                };
                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("ThumbnailView - GenerateThumbnail: {0}", ex.ToString());
            }
        }

        private UIImage DrawAnnotation(String pointStr, PSPDFInkAnnotation annotation)
        {
            try
            {
                SizeF pdfSize = AnnotationsDataAccessor.GetPDFSize(pointStr);

                if (!pdfSize.IsEmpty)
                {
                    // Begin a graphics context of sufficient size
                    UIGraphics.BeginImageContext(pdfSize);

                    // Get the context for CoreGraphics
                    CGContext ctx = UIGraphics.GetCurrentContext();

                    // Set stroking color
                    annotation.Color.SetStroke();

                    // Set line width
                    ctx.SetLineWidth(annotation.LineWidth);

                    // Set alpha
                    ctx.SetAlpha(annotation.Alpha);

                    // Set path
                    CGPath path = new CGPath();
                    List<NSValue> pointList = AnnotationsDataAccessor.GenerateViewPointLines(pointStr);
                    if (pointList != null)
                    {
                        path.MoveToPoint(pointList[0].CGPointValue.X, pointList[0].CGPointValue.Y);
                        for (Int32 i = 1; i < pointList.Count; i++)
                        {
                            if (i + 1 < pointList.Count)
                            {
                                path.CGPathAddLineToPoint(pointList[i].CGPointValue.X, pointList[i].CGPointValue.Y);
                            }
                        }
                        ctx.AddPath(path);
                    }

                    // Draw
                    ctx.StrokePath();

                    // Make image out of bitmap context
                    UIImage newImg = UIGraphics.GetImageFromCurrentImageContext();

                    UIGraphics.EndImageContext();

                    return newImg;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("ThumbnailView - DrawAnnotation: {0}", ex.ToString());
            }

            return null;
        }

        private UIImage Overlap(UIImage image1, UIImage image2)
        {
            // Begin a graphics context of sufficient size
            UIGraphics.BeginImageContext(image1.Size);

            // Overlap two images together
            image1.Draw(PointF.Empty);
            image2.Draw(PointF.Empty);

            // Make image out of bitmap context
            UIImage newImg = UIGraphics.GetImageFromCurrentImageContext();

            UIGraphics.EndImageContext();

            return newImg;
        }
    }
}

