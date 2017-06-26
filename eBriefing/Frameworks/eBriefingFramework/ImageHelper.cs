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
using System.Drawing;

namespace eBriefingMobile
{
	public static class ImageHelper
	{
		public static UIImage PDF2Image(CGPDFPage page, nfloat width, nfloat scale)
		{
			UIImage img=new UIImage();
			try
			{
				CGRect pageRect = page.GetBoxRect(CGPDFBox.Media);
				nfloat pdfScale = width / pageRect.Size.Width;
				pageRect.Size = new CGSize(pageRect.Size.Width * pdfScale, pageRect.Size.Height * pdfScale);

				UIGraphics.BeginImageContextWithOptions(pageRect.Size,true,scale);
				CGContext context = UIGraphics.GetCurrentContext();

				// White BG
				context.SetFillColor(1.0f, 1.0f, 1.0f, 1f);
				context.FillRect(pageRect);
				context.SaveState();

				//border
				context.SetStrokeColor(0f,0f,0f,0.5f);
				context.StrokeRect(pageRect);

				// Next 3 lines makes the rotations so that the page look in the right direction
				context.TranslateCTM(0.0f, pageRect.Size.Height);
				context.ScaleCTM(1.0f, -1.0f);
				CGAffineTransform transform = page.GetDrawingTransform(CGPDFBox.Media, pageRect, 0, true);
				context.ConcatCTM(transform);

				context.DrawPDFPage(page);
				context.RestoreState();

				img = UIGraphics.GetImageFromCurrentImageContext();

				UIGraphics.EndImageContext();

				context.Dispose();
			}
			catch(Exception ex)
			{
			}
			return img;
		}


		public static UIImage Text2Image(String text, CGSize imageSize)
		{
			NSString ns = new NSString(text);
			CGSize size = new CGSize(500, imageSize.Height);
			CGSize expectedSize = ns.StringSize (eBriefingAppearance.ThemeRegularFont (13), size, UILineBreakMode.WordWrap);

			UIGraphics.BeginImageContext(expectedSize);
			ns.DrawString(new CGRect(0, 0, expectedSize.Width, expectedSize.Height), eBriefingAppearance.ThemeRegularFont(13), UILineBreakMode.WordWrap);

			UIImage image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return image;
		}

		public static UIImage MaxResizeImage(UIImage sourceImage, int maxSize,nfloat scale)
		{
			var sourceSize = sourceImage.Size;
		
			nfloat width = sourceSize.Width;
			nfloat height = sourceSize.Height;
			if (height >= width)
			{
				width = (nfloat)Math.Floor((double)width * ((double)maxSize / (double)height));
				height = maxSize;
			}
			else
			{
				height = (nfloat)Math.Floor((double)height * ((double)maxSize / (double)width));
				width = maxSize;
			}

			// Begin a graphics context of sufficient size
			UIGraphics.BeginImageContextWithOptions(new CGSize(width, height), false,scale);

			sourceImage.Draw(new CGRect(0, 0, width, height));
			var result = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();

			return result;
		}

		public static UIImage Scale(UIImage image, int maxSize)
		{
			UIImage res = null;
			using (CGImage imageRef = image.CGImage)
			{
				CGImageAlphaInfo alphaInfo = imageRef.AlphaInfo;
				CGColorSpace colorSpaceInfo = CGColorSpace.CreateDeviceRGB();
				if (alphaInfo == CGImageAlphaInfo.None)
				{
					alphaInfo = CGImageAlphaInfo.NoneSkipLast;
				}

				nfloat width = imageRef.Width;
				nfloat height = imageRef.Height;
				if (height >= width)
				{
					width = (nfloat)Math.Floor((double)width * ((double)maxSize / (double)height));
					height = maxSize;
				}
				else
				{
					height = (nfloat)Math.Floor((double)height * ((double)maxSize / (double)width));
					width = maxSize;
				}

				CGBitmapContext bitmap = new CGBitmapContext(IntPtr.Zero, (nint)width, (nint)height, imageRef.BitsPerComponent, 0, colorSpaceInfo, alphaInfo);
				bitmap.DrawImage(new CGRect(0, 0, width, height), imageRef);

				res = UIImage.FromImage(bitmap.ToImage());
				bitmap = null;
			}

			return res;
		}

		public static UIImage Rotate(UIImage image)
		{
			UIImage res = null;
			using (CGImage imageRef = image.CGImage)
			{
				CGImageAlphaInfo alphaInfo = imageRef.AlphaInfo;
				CGColorSpace colorSpaceInfo = CGColorSpace.CreateDeviceRGB();
				if (alphaInfo == CGImageAlphaInfo.None)
				{
					alphaInfo = CGImageAlphaInfo.NoneSkipLast;
				}

				CGBitmapContext bitmap = new CGBitmapContext(IntPtr.Zero, imageRef.Height, imageRef.Width, imageRef.BitsPerComponent, 0, colorSpaceInfo, alphaInfo);
				bitmap.RotateCTM((nfloat)Math.PI / 2);
				bitmap.TranslateCTM(0, -imageRef.Height);
				bitmap.DrawImage(new CGRect(0, 0, imageRef.Width, imageRef.Height), imageRef);

				res = UIImage.FromImage(bitmap.ToImage());
				bitmap = null;
			}

			return res;
		}

		public static UIImage Overlap(UIImage image1, UIImage image2, CGPoint point1, CGPoint point2, nfloat scale)
		{
			// Begin a graphics context of sufficient size
			UIGraphics.BeginImageContextWithOptions(image1.Size, true, scale*2);

			// Overlap two images together
			image1.Draw(point1);
			image2.Draw(point2);

			// Make image out of bitmap context
			UIImage newImg = UIGraphics.GetImageFromCurrentImageContext();

			UIGraphics.EndImageContext();

			return newImg;
		}

		public static UIImage Add2Canvas(UIImage pdfImg, CGPoint point, nfloat scale, UIInterfaceOrientation orientation = UIInterfaceOrientation.Portrait)
		{
			CGSize size = CGSize.Empty;
			if (orientation == UIInterfaceOrientation.Portrait)
			{
				size = new CGSize(768, 1024);
			}
			else
			{
				size = new CGSize(1024, 768);
			}

			// Begin a graphics context of sufficient size
			UIGraphics.BeginImageContextWithOptions(size, true, scale);

			CGContext context = UIGraphics.GetCurrentContext();

			// White Background
			context.SetFillColor(1.0f, 1.0f, 1.0f, 1.0f);
			context.FillRect(new CGRect(0, 0, size.Width, size.Height));
			context.SaveState();

			pdfImg.Draw(point);

			// Make image out of bitmap context
			UIImage newImg = UIGraphics.GetImageFromCurrentImageContext();

			UIGraphics.EndImageContext();

			return newImg;
		}

		public static UIImage DrawPSPDFAnnotation(String pointStr, PSPDFInkAnnotation annotation)
		{
			try
			{
				CGSize pdfSize = AnnotationsDataAccessor.GetPDFSize(pointStr);

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
					List<CGPoint> pointList = AnnotationsDataAccessor.GenerateViewPointLines(pointStr);
					if (pointList != null)
					{
						path.MoveToPoint(pointList[0].X, pointList[0].Y);
						for (int i = 1; i < pointList.Count; i++)
						{
							if (i + 1 < pointList.Count)
							{
								path.AddLineToPoint(pointList[i].X, pointList[i].Y);
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
				Logger.WriteLineDebugging("ImageHelper - DrawPSPDFAnnotation: {0}", ex.ToString());
			}

			return null;
		}
	}
}

