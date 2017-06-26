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

namespace eBriefingMobile
{
	public class DashboardLayout : UICollectionViewFlowLayout
	{
		public DashboardLayout()
		{
			ScrollDirection = UICollectionViewScrollDirection.Vertical;
			ItemSize = new CGSize(280, 280);
			SectionInset = new UIEdgeInsets(0, 0, 20, 0);
			HeaderReferenceSize = new CGSize(0, 0);
			MinimumLineSpacing = 20.0f;
		}

		public override bool ShouldInvalidateLayoutForBoundsChange(CGRect newBounds)
		{
			return true;
		}

		public override UICollectionViewLayoutAttributes[] LayoutAttributesForElementsInRect(CGRect rect)
		{
			return base.LayoutAttributesForElementsInRect(rect);
		}
	}
}

