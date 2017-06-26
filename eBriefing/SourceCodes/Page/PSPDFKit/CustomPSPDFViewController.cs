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
using ObjCRuntime;
using Metrostar.Mobile.Framework;
using PSPDFKit;

namespace eBriefingMobile
{
    public class CustomPSPDFViewController : PSPDFViewController
    {
        private Book book;

        public CustomPSPDFViewController(Book book, PSPDFDocument document, PSPDFConfiguration configuration) : base(document, configuration)
        {
            this.book = book;

            this.ExtendedLayoutIncludesOpaqueBars = true;
            this.ThumbnailController.FilterSegment.TintColor = UIColor.White;

            this.Document.AnnotationSaveMode = PSPDFAnnotationSaveMode.Disabled;
            this.Document.AllowsCopying = false;
			this.Document.DiskCacheStrategy = PSPDFDiskCacheStrategy.Nothing;
	
//            this.ThumbnailController.StickyHeaderEnabled = true;
//            this.ThumbnailController.CollectionView.ContentInset = new UIEdgeInsets(80, 0, 0, 0);
//            this.ThumbnailController.ExtendedLayoutIncludesOpaqueBars = false;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            this.NavigationController.SetToolbarHidden(false, false);

            this.ViewMode = PSPDFViewMode.Document;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            this.NavigationController.SetToolbarHidden(true, true);
        }

        public void AddAnnotations()
        {
            try
            {
                for (nint i = 0; i < (nint)this.Document.PageCount; i++)
                {
                    Page page = BooksOnDeviceAccessor.GetPage(book.ID, i + 1);
                    if (page != null)
                    {
                        Annotation annotation = BooksOnDeviceAccessor.GetAnnotation(book.ID, page.ID);
                        if (annotation != null)
                        {
                            Dictionary<String, PSPDFInkAnnotation> dictionary = AnnotationsDataAccessor.GenerateAnnDictionary((nuint)i, annotation);
                            if (dictionary != null)
                            {
                                List<PSPDFAnnotation> annList = new List<PSPDFAnnotation>();
                                foreach (KeyValuePair<String, PSPDFInkAnnotation> item in dictionary)
                                {
                                    annList.Add(item.Value);
                                }
                                this.Document.AddAnnotations(annList.ToArray());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("CustomPSPDFViewController - AddAnnotations: {0}", ex.ToString());
            }
        }

        public void SaveAnnotations()
        {
            // Save annotation for current page
            SaveAnnotation(this.Page);

            // If this is double page mode, then save the next page as well, except if it's the last page
            if (IsDoublePageMode)
            {
                if (!this.IsLastPage)
                {
                    SaveAnnotation(this.Page + 1);
                }
            }
        }

        private void SaveAnnotation(nuint pageNumber)
        {
            Page page = BooksOnDeviceAccessor.GetPage(book.ID, (nint)pageNumber + 1);
            if (page != null)
            {
                List<AnnotationItem> inkItems = GenerateParseItem(pageNumber, PSPDFAnnotationType.Ink);
                if (inkItems != null && inkItems.Count > 0)
                {
                    Annotation annotation = new Annotation();
                    annotation.BookID = book.ID;
                    annotation.BookVersion = book.Version;
                    annotation.PageID = page.ID;
                    annotation.ModifiedUtc = DateTime.UtcNow;
                    annotation.Items = inkItems;

                    BooksOnDeviceAccessor.AddAnnotation(annotation);
                    BooksOnDeviceAccessor.UpdateUserModifiedDate(book);
                }
                else
                {
                    BooksOnDeviceAccessor.RemoveAnnotation(book.ID, page.ID);
                    BooksOnDeviceAccessor.UpdateUserModifiedDate(book);
                }
            }
        }

        public List<AnnotationItem> GenerateParseItem(nuint page, PSPDFAnnotationType type)
        {
            List<AnnotationItem> items = null;

            PSPDFAnnotation[] anns = this.Document.AnnotationsForPage(page, type);
            if (anns != null)
            {
                if (anns.Length > 0)
                {
                    items = new List<AnnotationItem>();

                    PSPDFPageView pageView = this.PageViewForPage(page);
                    if (pageView != null)
                    {
                        for (int i = 0; i < anns.Length; i++)
                        {
                            if (!anns[i].Deleted)
                            {
                                String dictionaryKey = StringRef.Pen;
                                PSPDFInkAnnotation ann = (PSPDFInkAnnotation)anns[i];
                                if (ann.LineWidth > 10)
                                {
                                    dictionaryKey = StringRef.Highlighter;
                                }
                                dictionaryKey += " " + i.ToString();

                                // boundingBox
                                String description = String.Empty;
                                String boundingBox = ann.DictionaryValue["boundingBox"].ToString();
                                description += "[BoundingBox]" + boundingBox;

                                // color
                                String color = ann.DictionaryValue["color"].ToString();
                                description += "[Color]" + color;

                                // lineWidth
                                String lineWidth = ann.DictionaryValue["lineWidth"].ToString();
                                description += "[LineWidth]" + lineWidth;

                                // Add description to the dictionary
                                items.Add(new AnnotationItem(dictionaryKey, description));

                                // Lines
                                String points = String.Empty;
                                String pdfSize = StringRef.pdfSize + pageView.Frame.Width + "x" + pageView.Frame.Height;
                                for (int j = 0; j < ann.Lines.Count; j++)
                                {
                                    if (j != 0)
                                    {
                                        points += StringRef.annDivider;
                                    }

                                    String pointStr = String.Empty;
                                    if (ann.Lines[j] != null && ann.Lines[j].Length > 0)
                                    {
                                        for (int k = 0; k < ann.Lines[j].Length; k++)
                                        {
                                            pointStr += ann.Lines[j][k].ToString();
                                        }
                                    }

                                    List<CGPoint> pointList = AnnotationsDataAccessor.GeneratePointF(pointStr);
                                    if (pointList != null && pointList.Count > 0)
                                    {
                                        points += pdfSize + StringRef.viewPoints;
                                        foreach (CGPoint point in pointList)
                                        {
                                            points += pageView.ConvertPdfPointToViewPoint(point);
                                        }

                                        points += StringRef.pdfPoints;
                                        foreach (CGPoint point in pointList)
                                        {
                                            points += point;
                                        }
                                    }
                                }

                                // Add line points to the dictionary
                                items.Add(new AnnotationItem(dictionaryKey, points));
                            }
                        }
                    }
                }
            }

            return items;
        }
    }
}

