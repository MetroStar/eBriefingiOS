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
using PSPDFKit;
using CoreFoundation;
using System.Threading.Tasks;

namespace eBriefingMobile
{
	public class PrintHelper
	{
		private String bookID;
		private List<Page> pageList;
		private Dictionary<Page, Annotation> dict1;
		private Dictionary<Page, List<eBriefingMobile.Note>> dict2;
		private nfloat scale = UIScreen.MainScreen.Scale;
		private PageRenderer renderer;
		private Dictionary<int,Dictionary<UIImage,List<Note>>> dict;
		public int PagesNum;

		public ORIENTATION Orientation { get; set; }

		public ANNOTATION Annotation { get; set; }

		public NOTE Note { get; set; }

		public RANGE Range { get; set; }

		public enum ORIENTATION
		{
			PORTRAIT,
			LANDSCAPE
		}

		public enum RANGE
		{
			ALL,
			CURRENT,
			CUSTOM
		}

		public enum ANNOTATION
		{
			WITHOUT,
			WITH
		}

		public enum NOTE
		{
			WITHOUT,
			WITH
		}

		public PrintHelper(String bookID, List<Page> pageList)
		{
			this.bookID = bookID;
			this.pageList = pageList;
		}

		public bool Generate()
		{
			try
			{
				dict1 = new Dictionary<Page, eBriefingMobile.Annotation>();
				dict2 = new Dictionary<Page, List<eBriefingMobile.Note>>();

				foreach (var page in pageList)
				{
					// Add annotations to dictionary if necessary
					if (Annotation == ANNOTATION.WITH)
					{
						Annotation ann = BooksOnDeviceAccessor.GetAnnotation(bookID, page.ID);
						dict1.Add(page, ann);
					}

					// Add notes to dictionary if necessary
					if (Note == NOTE.WITH)
					{
						List<Note> noteList=new List<eBriefingMobile.Note>();
						Note note = null;
						if (String.IsNullOrEmpty(URL.MultipleNoteURL))
						{
							note = BooksOnDeviceAccessor.GetNote(bookID, page.ID);
							if (note != null)
							{
								noteList.Add(note);
								dict2.Add(page, noteList);
							}
						}
						else
						{
							noteList = BooksOnDeviceAccessor.GetNotes(bookID, page.ID);
							if (noteList != null && noteList.Count > 0)
							{
								dict2.Add(page, noteList);
							}
						}
					}
				}

				return GenerateImage(dict1,dict2);
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("PrintHelper - Generate: {0}", ex.ToString());
				return false;
			}
		}

		bool GenerateImage(Dictionary<Page, eBriefingMobile.Annotation> dict1, Dictionary<Page,List< eBriefingMobile.Note>> dict2)
		{
			try
			{
				if ( pageList != null )
				{
					nuint totalImageSize=0;
					List<Note> notes = new List<eBriefingMobile.Note> ();

					foreach (var page in pageList)
					{
						String localPath = DownloadedFilesCache.BuildCachedFilePath (page.URL);
						var printItemDict= new Dictionary<UIImage,List<Note>>();

						if ( !String.IsNullOrEmpty (localPath) )
						{
							CGPDFDocument pdfDoc = CGPDFDocument.FromFile (localPath);
							if ( pdfDoc != null )
							{
								CGPDFPage pdfPage = pdfDoc.GetPage (1);
								if ( pdfPage != null )
								{
									CGRect pageRect = pdfPage.GetBoxRect (CGPDFBox.Media);
									UIImage pdfImg = ImageHelper.PDF2Image (pdfPage, pageRect.Width, scale);

									// Add annotation if option selected
									if ( dict1.ContainsKey (page) )
									{
										Annotation annotation = dict1 [page];
										if ( annotation != null )
										{
											Dictionary<String, PSPDFInkAnnotation> coordinateDict = AnnotationsDataAccessor.GenerateAnnDictionary ((UInt32)page.PageNumber - 1, annotation);
											if ( coordinateDict != null )
											{
												foreach (KeyValuePair<String, PSPDFInkAnnotation> item in coordinateDict)
												{
													// Create full size annotation
													UIImage annImg = ImageHelper.DrawPSPDFAnnotation (item.Key, item.Value);

													if ( annImg != null )
													{
														// Scale down the annotation image
														annImg = annImg.Scale (new CGSize (pdfImg.Size.Width, pdfImg.Size.Height));

														// Overlap pdfImg and annImg
														pdfImg = ImageHelper.Overlap (pdfImg, annImg, CGPoint.Empty, CGPoint.Empty, scale);
													}
												}
											}
										}
									}

									// Create image from text
									bool printNote = false;
									UIImage noteImg = null;
									if ( dict2.ContainsKey (page) && dict2 [page] != null )
									{
										printNote = true;
										notes = dict2 [page];

										// Create image from text
										//noteImg = ImageHelper.Text2Image(_notesText, pdfImg.Size);
									}
									else
									{
										notes = null;
									}

									// Scale down and add to canvas
									// Used 900 and 1200 because couldn't control the paper margin
//								if (Orientation == ORIENTATION.PORTRAIT)
//								{
//									//if (printNote)
//									{
//										//pdfImg = ImageHelper.Scale(pdfImg, 500);
//										//pdfImg=ImageHelper.MaxResizeImage(pdfImg,1000,scale);
//										//pdfImg = ImageHelper.Add2Canvas(pdfImg, new CGPoint(0, (1024 / 2) - (pdfImg.Size.Height / 2)), scale);
//
//										// Overlap pdfImg and noteImg
//										//pdfImg = ImageHelper.Overlap(pdfImg, noteImg, CGPoint.Empty, new CGPoint(500, 0), scale);
//									}
//									//else
//									{
//										//pdfImg=ImageHelper.MaxResizeImage(pdfImg,1000,scale);
//										//pdfImg = ImageHelper.Scale(pdfImg, 900);
//										//pdfImg = ImageHelper.Add2Canvas(pdfImg, new CGPoint((768 / 2) - (pdfImg.Size.Width / 2), (1024 / 2) - (pdfImg.Size.Height / 2)), scale);
//									}
//								}
//								else
//								{
//									//if (printNote)
//									{
//										//pdfImg=ImageHelper.MaxResizeImage(pdfImg,500,scale);
//										//pdfImg = ImageHelper.Scale(pdfImg, 500);
//									//		pdfImg = ImageHelper.Add2Canvas(pdfImg, new CGPoint(0,0), scale*2, UIInterfaceOrientation.LandscapeLeft);
//										// Overlap pdfImg and noteImg
//										//pdfImg = ImageHelper.Overlap(pdfImg, noteImg, CGPoint.Empty, new CGPoint(756, 0), scale);
//									}
//									//else
//									{
//										//pdfImg=ImageHelper.MaxResizeImage(pdfImg,1000,scale);
//										//pdfImg = ImageHelper.Scale(pdfImg, 500);
//										///pdfImg = ImageHelper.Add2Canvas(pdfImg, new CGPoint((1024 / 2) - (pdfImg.Size.Width / 2), (768 / 2) - (pdfImg.Size.Height / 2)), scale*2, UIInterfaceOrientation.LandscapeLeft);
//									}
//
//									// Rotate canvas
//									//pdfImg = ImageHelper.Rotate(pdfImg);
//								}

									// Save
//								if (printItems == null)
//								{
//									printItems = new List<UIImage>();
//								}
//								printItems.Add(pdfImg);

									if ( dict == null )
									{
										dict = new Dictionary<int,Dictionary<UIImage,List<Note>>> ();
									}

									if ( pdfImg != null )
									{
										printItemDict.Add (pdfImg, notes);
										dict.Add(page.PageNumber,printItemDict);

										var pngImage = pdfImg.AsPNG();
										totalImageSize=pngImage.Length + totalImageSize;
										Console.WriteLine("Img : " + totalImageSize.ToString());

										//image dispose
										pdfImg=null;

										if(CheckReachMemoryLimit(totalImageSize))
										{
											PagesNum=dict.Count-1;

											dict.Clear();
											dict=null;

											return false;
										}
									}
								}
							}
						}
					}

					PagesNum=dict.Count;

					return true;
				}
				return false;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		private bool CheckReachMemoryLimit(nuint totalImageSize)
		{
			//if converting image is more than 10MB,warning user
			if ( totalImageSize > 10000000 )
			{
				return true;
			}

			return false;
		}

		public string GeneratePageRender(bool bGeneratePdf=false)
		{
			try
			{
				renderer = new PageRenderer (Orientation, dict);

				if(bGeneratePdf)
				{
					 return GeneratePdf();
				}
				else
				{
					return null;
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine ("PrintHelper Exception" + ex.Message);
				return null;
			}
		}

		private string GeneratePdf()
		{
			NSMutableData printData = new NSMutableData();

			//create a PDF with empty rectangle, which will configure it for 8.5x11 inches

			var frame = new CGRect (0, 0, 0, 0);

			if ( Orientation == PrintHelper.ORIENTATION.LANDSCAPE )
			{
				frame.Width = 792f;
				frame.Height = 612f;
			}
			else
			{
				frame.Width = 612f;
				frame.Height = 792f;
			}

			UIGraphics.BeginPDFContext(printData,frame, null);

			if ( renderer != null )
			{
				for (int i = 0; i < renderer.NumberOfPages; i++)
				{
					UIGraphics.BeginPDFPage ();

					renderer.DrawHeaderForPage (i, new CGRect ());
					renderer.DrawFooterForPage (i, new CGRect ());
					renderer.DrawContentForPage (i, new CGRect ());
					renderer.DrawPage (i, new CGRect ());
				}
			}

			// complete a PDF page
			UIGraphics.EndPDFContent();

			var path =  NSFileManager.DefaultManager.GetUrls (NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User) [0].Path;
			var pdfFile=System.IO.Path.Combine (path,"test.pdf");

			printData.Save (pdfFile, true);
		
			return pdfFile;
		}

		public void Print(PSPDFPrintBarButtonItem barButton)
		{
			try
			{
				if (UIPrintInteractionController.PrintingAvailable)
				{
					UIPrintInteractionController pic = UIPrintInteractionController.SharedPrintController;
					if (pic != null)
					{
						// PrintInfo
						UIPrintInfo printInfo = UIPrintInfo.PrintInfo;
						printInfo.OutputType = UIPrintInfoOutputType.General;
						printInfo.JobName = "Print Job: eBriefing";
						printInfo.Duplex = UIPrintInfoDuplex.None;

						if(Orientation==ORIENTATION.LANDSCAPE)
						{
							printInfo.Orientation=UIPrintInfoOrientation.Landscape;
						}
						else
						{
							printInfo.Orientation=UIPrintInfoOrientation.Portrait;
						}

						pic.PrintInfo = printInfo;
						pic.ShowsNumberOfCopies = true;
						pic.ShowsPaperSelectionForLoadedPapers = true;
						pic.ShowsPageRange = false;

						pic.PrintPageRenderer = renderer;

						// Show print options
						pic.PresentFromBarButtonItem(barButton, true, (printController, completed, error) =>
						{
							if (!completed && error != null)
							{
								Console.WriteLine("PrintHelper - Print Error Code " + error.Code);
							}

							renderer.Dispose();
							renderer= null;
							dict.Clear();
							dict=null;
						});
					}
				}
				else
				{
					new UIAlertView(StringRef.alert, "Print is not available at this time.", null, StringRef.ok, null).Show();
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("PrintHelper - Print: {0}", ex.ToString());
			}
		}
	}
}

