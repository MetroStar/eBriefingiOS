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
using Foundation;
using UIKit;
using PSPDFKit;

namespace eBriefingMobile
{
    public class CustomPSPDFViewControllerDelegate : PSPDFViewControllerDelegate
    {
        public bool Hud_Lock { get; set; }

        public delegate void CustomPSPDFDelegate0 ();

        public delegate void CustomPSPDFDelegate1 (nfloat scale);

        public delegate void CustomPSPDFDelegate2 (PSPDFPageView pageView);

        public event CustomPSPDFDelegate0 DidShowPageViewEvent;
        public event CustomPSPDFDelegate0 DidChangeViewModeEvent;
        public event CustomPSPDFDelegate0 DidShowHideHudEvent;
        public event CustomPSPDFDelegate0 ShouldShowHudEvent;
        public event CustomPSPDFDelegate0 ShouldHideHudEvent;
        public event CustomPSPDFDelegate0 SaveAnnotationEvent;
        public event CustomPSPDFDelegate1 DidEndPageZoomingEvent;
        public event CustomPSPDFDelegate2 DidRenderPageViewEvent;
        public event CustomPSPDFDelegate2 WillUnloadPageViewEvent;

		public CustomPSPDFViewControllerDelegate()
		{

		}

        public override PSPDFMenuItem[] ShouldShowMenuItemsForSelectedText(PSPDFViewController pdfController, PSPDFMenuItem[] menuItems, CGRect rect, String selectedText, CGRect textRect, PSPDFPageView pageView)
		{
            int addIndex = 0;
            PSPDFMenuItem[] newItems = new PSPDFMenuItem[1];
			if ( menuItems != null )
			{
				for (int i = 0; i < menuItems.Length; i++)
				{
					if ( menuItems [i].Identifier == "Search" )
					{
						newItems [addIndex] = menuItems [i];
						addIndex++;
					}
				}
			}

			if ( newItems != null && addIndex!=0 )
			{
				return newItems;
			}
			return null;
        }

        public override PSPDFMenuItem[] ShouldShowMenuItemsForAnnotations(PSPDFViewController pdfController, PSPDFMenuItem[] menuItems, CGRect rect, PSPDFAnnotation[] annotations, CGRect textRect, PSPDFPageView pageView)
        {
            if (annotations != null)
            {
                PSPDFMenuItem[] newItems = new PSPDFMenuItem[1];

                PSPDFMenuItem removeMenu = new PSPDFMenuItem(StringRef.Remove, delegate
                {
                    annotations[0].Deleted = true;
                    pageView.RemoveAnnotation(annotations[0], null, true);

                    if (SaveAnnotationEvent != null)
                    {
                        SaveAnnotationEvent();
                    }
                }, StringRef.Remove);
                newItems[0] = removeMenu;

                return newItems;
            }

            return menuItems;
        }

        public override PSPDFMenuItem[] ShouldShowMenuItemsForSelectedImage(PSPDFViewController pdfController, PSPDFMenuItem[] menuItems, CGRect rect, PSPDFImageInfo selectedImage, CGRect textRect, PSPDFPageView pageView)
        {
            return null;
        }

        public override void DidShowPageView(PSPDFViewController pdfController, PSPDFPageView pageView)
        {
            if (DidShowPageViewEvent != null)
            {
                DidShowPageViewEvent();
            }
        }

        public override void DidChangeViewMode(PSPDFViewController pdfController, PSPDFViewMode viewMode)
        {
            if (DidChangeViewModeEvent != null)
            {
                DidChangeViewModeEvent();
            }
        }

        public override void DidEndPageZooming(PSPDFViewController pdfController, UIScrollView scrollView, nfloat scale)
        {
            if (DidEndPageZoomingEvent != null)
            {
                DidEndPageZoomingEvent(scale);
            }
        }

        public override void DidRenderPageView(PSPDFViewController pdfController, PSPDFPageView pageView)
        {
            if (DidRenderPageViewEvent != null)
            {
                DidRenderPageViewEvent(pageView);
            }
        }

        public override void WillUnloadPageView(PSPDFViewController pdfController, PSPDFPageView pageView)
        {
            if (WillUnloadPageViewEvent != null)
            {
                WillUnloadPageViewEvent(pageView);
            }
        }

        public override void DidShowHud(PSPDFViewController pdfController, bool animated)
        {
            UIApplication.SharedApplication.SetStatusBarHidden(false, true);

            if (DidShowHideHudEvent != null)
            {
                DidShowHideHudEvent();
            }
        }

        public override void DidHideHud(PSPDFViewController pdfController, bool animated)
        {
            UIApplication.SharedApplication.SetStatusBarHidden(true, true);

            if (DidShowHideHudEvent != null)
            {
                DidShowHideHudEvent();
            }
        }

        public override bool ShouldShowHud(PSPDFViewController pdfController, bool animated)
        {
            if (ShouldShowHudEvent != null)
            {
                ShouldShowHudEvent();
            }

            return true;
        }

        public override bool ShouldHideHud(PSPDFViewController pdfController, bool animated)
        {
            if (!Hud_Lock)
            {
                if (ShouldHideHudEvent != null)
                {
                    ShouldHideHudEvent();
                }

                return true;
            }

            return false;
        }
    }
}

