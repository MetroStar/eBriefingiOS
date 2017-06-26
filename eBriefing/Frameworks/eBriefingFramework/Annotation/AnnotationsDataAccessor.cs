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
using CoreGraphics;
using System.Collections.Generic;
using Foundation;
using UIKit;
using Metrostar.Mobile.Framework;
using PSPDFKit;

namespace eBriefingMobile
{
    public static class AnnotationsDataAccessor
    {
        private static PersistantStorageDatabase<Annotation> _database = new PersistantStorageDatabase<Annotation>("Annotations");

        public static void AddAnnotation(Annotation annotation)
        {
            _database.SetRecord(annotation.BookID, annotation.PageID, annotation);
        }

        public static void RemoveAnnotation(String bookID, String pageID)
        {
            _database.DeleteRecord(bookID, pageID);
        }

        public static void UpdateAnnotation(Annotation annotation)
        {
            AddAnnotation(annotation);
        }

        public static void UpdateAnnotations(String bookID, List<Page> newPageList)
        {
            List<Page> oldPageList = PagesOnDeviceDataAccessor.GetPages(bookID);
            if (oldPageList != null)
            {
                foreach (Page oldPage in oldPageList)
                {
                    bool notFound = true;
                    foreach (Page newPage in newPageList)
                    {
                        if (oldPage.ID == newPage.ID)
                        {
                            notFound = false;
                            break;
                        }
                    }
                    
                    if (notFound)
                    {
                        RemoveAnnotation(bookID, oldPage.ID);
                    }
                }
            }
        }

        public static List<Annotation> GetAllAnnotations(String bookID)
        {
            List<Annotation> result = _database.GetRecordsInTable(bookID);
            if (result == null || result.Count == 0)
            {
                return null;
            }

            return result;
        }

        public static List<Annotation> GetAnnotations(String bookID)
        {
            List<Annotation> list = _database.GetRecordsInTable(bookID);
            if (list == null || list.Count == 0)
            {
                return null;
            }
            else
            {
                List<Annotation> result = null;
                foreach (Annotation annotation in list)
                {
                    if (!annotation.Removed)
                    {
                        if (result == null)
                        {
                            result = new List<Annotation>();
                        }

                        result.Add(annotation);
                    }
                }

                return result;
            }
        }

        public static List<Annotation> GetRemovedAnnotations(String bookID)
        {
            List<Annotation> list = _database.GetRecordsInTable(bookID);
            if (list == null || list.Count == 0)
            {
                return null;
            }
            else
            {
                List<Annotation> result = null;
                foreach (Annotation annotation in list)
                {
                    if (annotation.Removed)
                    {
                        if (result == null)
                        {
                            result = new List<Annotation>();
                        }

                        result.Add(annotation);
                    }
                }

                return result;
            }
        }

        public static Annotation GetAnnotation(String bookID, String pageID)
        {
            Annotation annotation = _database.GetRecord(bookID, pageID);
            if (annotation != null && !annotation.Removed)
            {
                return annotation;
            }

            return null;
        }

        public static int GetNumAnnotationsInBook(String bookID)
        {
            return GetNumAnnotations(bookID, PagesOnDeviceDataAccessor.GetPages(bookID));
        }

        public static int GetNumAnnotationsInChapter(String bookID, String chapterID)
        {
            return GetNumAnnotations(bookID, PagesOnDeviceDataAccessor.GetPagesInChapter(bookID, chapterID));
        }

        private static int GetNumAnnotations(String bookID, List<Page> pageList)
        {
            int count = 0;

            if (pageList != null)
            {
                foreach (var page in pageList)
                {
                    Annotation annotation = GetAnnotation(bookID, page.ID);
                    if (annotation != null && !annotation.Removed)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public static PSPDFInkAnnotation OldGenerator(String desc)
        {
            PSPDFInkAnnotation annotation = new PSPDFInkAnnotation();

            try
            {
                desc = desc.Replace("<", String.Empty);
                desc = desc.Replace(">", String.Empty);
                desc = desc.Replace(@"""", String.Empty);
                desc = desc.Replace(@",", String.Empty);

                Char[] delimiterChars = { ':', ' ' };
                String[] words = desc.Split(delimiterChars);

                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i].Contains("type"))
                    {
                        i = i + 1;

                        String nextWord = words[i];
                        int startIdx = nextWord.IndexOf('(') + 1;
                        int length = nextWord.IndexOf(')') - startIdx;
                        annotation.TypeString = nextWord.Substring(startIdx, length);
                    }
                    else if (words[i].Contains("contents"))
                    {
                        int count = 0;
                        for (int j = i; j < words.Length; j++)
                        {
                            if (words[j].Contains("indexOnPage"))
                            {
                                count = j - i;
                                break;
                            }
                        }

                        for (int j = 1; j < count; j++)
                        {
                            String tempStr = words[i + j] + " ";
                            if (tempStr.Contains("\\n"))
                            {
                                tempStr = tempStr.Replace("\\n", String.Empty);
                                tempStr = '\n' + tempStr;
                            }

                            annotation.Contents += tempStr;
                        }

                        annotation.Contents = annotation.Contents.Trim();
                    }
                    else if (words[i].Contains("boundingBox"))
                    {
                        String xStr = words[i + 1].Replace("{{", String.Empty);
                        String yStr = words[i + 2].Replace("}", String.Empty);
                        String wStr = words[i + 3].Replace("{", String.Empty);
                        String hStr = words[i + 4].Replace("}}", String.Empty);

                        nfloat x = (nfloat)Convert.ToDouble(xStr);
                        nfloat y = (nfloat)Convert.ToDouble(yStr);
                        nfloat w = (nfloat)Convert.ToDouble(wStr);
                        nfloat h = (nfloat)Convert.ToDouble(hStr);
                        annotation.BoundingBox = new CGRect(x, y, w, h);

                        i = i + 4;
                    }
                    else if (words[i].Contains("UIDeviceRGBColorSpace"))
                    {
                        nfloat R = (nfloat)Convert.ToDouble(words[i + 1]);
                        nfloat G = (nfloat)Convert.ToDouble(words[i + 2]);
                        nfloat B = (nfloat)Convert.ToDouble(words[i + 3]);

                        String alpha = words[i + 4].Replace(")", String.Empty);
                        nfloat A = (nfloat)Convert.ToDouble(alpha);

                        annotation.Alpha = A;
                        annotation.Color = UIColor.FromRGB(R, G, B);

                        i = i + 3;
                    }
                    else if (words[i].Contains("page"))
                    {
                        i = i + 1;

                        annotation.Page = Convert.ToUInt32(words[i]);
                    }
                    else if (words[i].Contains("lineWidth"))
                    {
                        i = i + 1;

                        annotation.LineWidth = (nfloat)Convert.ToDouble(words[i]);
                    }
                }

                annotation.Editable = true;
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("AnnotationsDataAccessor - OldGenerator: {0}", ex.ToString());
            }

            return annotation;
        }

        private static PSPDFInkAnnotation NewGenerator(String desc)
        {
            PSPDFInkAnnotation annotation = new PSPDFInkAnnotation();

            try
            {
                // BoundingBox
                int colorIdx = desc.IndexOf("[Color]");
                if (colorIdx >= 0)
                {
                    String boundingBox = desc.Remove(colorIdx);
                    boundingBox = boundingBox.Replace("[BoundingBox]NSRect:", String.Empty);
                    boundingBox = boundingBox.Replace("{{", String.Empty);
                    boundingBox = boundingBox.Replace("}", String.Empty);
                    boundingBox = boundingBox.Replace("{", String.Empty);
                    boundingBox = boundingBox.Replace("}}", String.Empty).Trim();

                    Char[] splitter = { ',' };
                    String[] words = boundingBox.Split(splitter);

                    nfloat x = (nfloat)Convert.ToDouble(words[0].Trim());
                    nfloat y = (nfloat)Convert.ToDouble(words[1].Trim());
                    nfloat w = (nfloat)Convert.ToDouble(words[2].Trim());
                    nfloat h = (nfloat)Convert.ToDouble(words[3].Trim());
                    annotation.BoundingBox = new CGRect(x, y, w, h);
                }

                int lineIdx = 0;

                // Color
                colorIdx = desc.IndexOf("[Color]");
                if (colorIdx >= 0)
                {
                    String color = desc.Remove(0, colorIdx + 7);
                    lineIdx = color.IndexOf("[LineWidth]");
                    if (lineIdx >= 0)
                    {
                        color = color.Remove(lineIdx);
                        color = color.Replace("UIColor ", String.Empty).Trim();
                        color = color.Replace("[", String.Empty);
                        color = color.Replace("]", String.Empty);

                        Char[] splitter = { ',' };
                        String[] words = color.Split(splitter);

                        nfloat A = (nfloat)Convert.ToDouble(words[0].Trim().Remove(0, 2));
                        nfloat R = (nfloat)Convert.ToDouble(words[1].Trim().Remove(0, 2));
                        nfloat G = (nfloat)Convert.ToDouble(words[2].Trim().Remove(0, 2));
                        nfloat B = (nfloat)Convert.ToDouble(words[3].Trim().Remove(0, 2));

                        annotation.Alpha = A / 255;
                        annotation.Color = eBriefingAppearance.Color(R, G, B);
                    }
                }

                // LineWidth
                lineIdx = desc.IndexOf("[LineWidth]");
                if (lineIdx >= 0)
                {
                    String lineWidth = desc.Remove(0, lineIdx + 11);
                    annotation.LineWidth = (nfloat)Convert.ToDouble(lineWidth.Trim());
                }

                annotation.Editable = true;
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("AnnotationsDataAccessor - NewGenerator: {0}", ex.ToString());
            }

            return annotation;
        }

        private static PSPDFInkAnnotation GenerateAnnotation(String desc)
        {
            int colorIdx = desc.IndexOf("[Color]");
            if (colorIdx >= 0)
            {
                // New way
                return NewGenerator(desc);
            }
            else
            {
                // Old way
                return OldGenerator(desc);
            }
        }

        private static List<CGPoint> RetrievePoints(String pointStr)
        {
            List<CGPoint> pointList = new List<CGPoint>();

            Char[] delimiterChars = { ',' };
            String[] words = pointStr.Split(delimiterChars);

            nfloat x = 0;
            nfloat y = 0;

            try
            {
                for (int i = 0; i < words.Length; i++)
                {
                    if (!String.IsNullOrEmpty(words[i]))
                    {
                        if (i % 2 == 0)
                        {
                            x = (nfloat)Convert.ToDouble(words[i].Trim());
                        }
                        else
                        {
                            y = (nfloat)Convert.ToDouble(words[i].Trim());

                            pointList.Add(new CGPoint(x, y));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("AnnotationsDataAccessor - RetrievePoints: {0}", ex.ToString());
            }

            return pointList;
        }

        private static List<CGPoint> Convert2PointList(String pointStr)
        {
            pointStr = pointStr.Replace("{", String.Empty);
            pointStr = pointStr.Replace("}", ",");
            pointStr = pointStr.Replace("X=", String.Empty);
            pointStr = pointStr.Replace("Y=", String.Empty);
            pointStr = pointStr.Trim();

            return RetrievePoints(pointStr);
        }

        private static NSValue[] GeneratePDFPointLines(String pointStr)
        {
            List<CGPoint> pointList = null;

            // New way
            if (pointStr.StartsWith("["))
            {
                int pdfPointIdx = pointStr.IndexOf(StringRef.pdfPoints);
                if (pdfPointIdx >= 0)
                {
                    pointStr = pointStr.Remove(0, pdfPointIdx + 11);
                }

                pointList = Convert2PointList(pointStr);
            }
            else
            {
                // Old way
                pointList = GeneratePointF(pointStr);
            }

            if (pointList != null && pointList.Count > 0)
            {
                NSValue[] valueArray = new NSValue[pointList.Count];
                for (int i = 0; i < valueArray.Length; i++)
                {
                    valueArray[i] = NSValue.FromCGPoint(pointList[i]);
                }

                return valueArray;
            }

            return null;
        }

        public static List<CGPoint> GenerateViewPointLines(String pointStr)
        {
            if (pointStr.StartsWith("["))
            {
                int viewPointIdx = pointStr.IndexOf(StringRef.viewPoints);
                if (viewPointIdx >= 0)
                {
                    pointStr = pointStr.Remove(0, viewPointIdx);

                    int pdfPointIdx = pointStr.IndexOf(StringRef.pdfPoints);
                    if (pdfPointIdx >= 0)
                    {
                        pointStr = pointStr.Remove(pdfPointIdx);
                    }

                    pointStr = pointStr.Replace(StringRef.viewPoints, String.Empty);
                }

                return Convert2PointList(pointStr);
            }

            return null;
        }

        public static List<CGPoint> GeneratePointF(String pointStr)
        {
            pointStr = pointStr.Replace(@"NSPoint: {", String.Empty);
            pointStr = pointStr.Replace(@"}", ",");
            pointStr = pointStr.Replace(@"\n", String.Empty);
            pointStr = pointStr.Trim();

            return RetrievePoints(pointStr);
        }

        public static Dictionary<String, PSPDFInkAnnotation> GenerateAnnDictionary(nuint page, Annotation annotation)
        {
            // BL - DO NOT TOUCH THIS CODE IF YOU DON'T KNOW WHAT YOU'RE DOING - I KNOW WHAT I'M DOING :)
            Dictionary<String, PSPDFInkAnnotation> dictionary = null;

            if (annotation != null)
            {
                if (annotation.Items != null && annotation.Items.Count > 0)
                {
                    dictionary = new Dictionary<String, PSPDFInkAnnotation>();

                    // Pen / Highlighter
                    for (int i = 0; i < annotation.Items.Count; i += 2)
                    {
                        String[] points = annotation.Items[i + 1].Value.Split(new String[] { StringRef.annDivider }, StringSplitOptions.None);
                        for (int j = 0; j < points.Length; j++)
                        {
                            if (!String.IsNullOrEmpty(points[j]))
                            {
                                PSPDFInkAnnotation newAnn = GenerateAnnotation(annotation.Items[i].Value);
                                newAnn.Page = page;

                                try
                                {
                                    NSValue[] lines = GeneratePDFPointLines(points[j]);
                                    if (lines != null && lines.Length > 0)
                                    {
                                        newAnn.Lines = new List<NSValue[]>() {
                                            lines
                                        };
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.WriteLineDebugging("AnnotationsDataAccessor - GenerateAnnDictionary: {0}", ex.ToString());
                                }

                                // Use annotation Alpha, not the Alpha value in color (from PSPDFKit documentation)
                                if (annotation.Items[i + 1].Key.Contains(StringRef.Highlighter))
                                {
                                    newAnn.Alpha = 0.5f;
                                }
            
                                dictionary.Add(points[j], newAnn);
                            }
                        }
                    }
                }
            }

            return dictionary;
        }

        public static CGSize GetPDFSize(String pointStr)
        {
            try
            {
                if (pointStr.StartsWith("["))
                {
                    int viewPointIdx = pointStr.IndexOf(StringRef.viewPoints);
                    if (viewPointIdx >= 0)
                    {
                        pointStr = pointStr.Remove(viewPointIdx);
                        pointStr = pointStr.Replace(StringRef.pdfSize, String.Empty);
                    }

                    Char[] splitter = { 'x' };
                    String[] words = pointStr.Split(splitter);

                    int width = Convert.ToInt32(words[0]);
                    int height = Convert.ToInt32(words[1]);

                    return new CGSize(width, height);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("AnnotationsDataAccessor - GetPDFSize: {0}", ex.ToString());
            }

            return CGSize.Empty;
        }
        // Example of Annotation pointStr
        // [PDFSize]593x768 - Tells the actual size of the pdf when the annotation was saved. This is used when creating a thumbnail annotation overlay on the Dashboard
        // [ViewPoints]{X=376, Y=727}{X=355, Y=727}{X=334, Y=725}{X=309, Y=720}{X=273, Y=710} - Tells the array of View Points when the annotation was saved.
        // [PDFPoints]{X=388.0472, Y=42.28125}{X=366.3744, Y=42.28125}{X=344.7015, Y=44.34375} - Tells the array of PDF Points when the annotation was saved.
    }
}

