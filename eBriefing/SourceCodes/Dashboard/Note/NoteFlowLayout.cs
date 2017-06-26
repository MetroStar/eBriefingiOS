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

namespace eBriefingMobile
{
	public class NoteFlowLayout : UICollectionViewFlowLayout
	{
		public NoteFlowLayout()
		{
			ScrollDirection = UICollectionViewScrollDirection.Vertical;
			SectionInset = new UIEdgeInsets(0, 0, 20, 0);
			MinimumLineSpacing = 0;
			MinimumInteritemSpacing = 0;
		}

		public override bool ShouldInvalidateLayoutForBoundsChange(CGRect newBounds)
		{
			return true;
		}

		public override UICollectionViewLayoutAttributes[] LayoutAttributesForElementsInRect(CGRect rect)
		{
			List<UICollectionViewLayoutAttributes> answer = base.LayoutAttributesForElementsInRect(rect).ToList();

			List<NSIndexSet> missingSections = new List<NSIndexSet>();

			foreach (var attribute in answer)
			{
				if (attribute.RepresentedElementCategory == UICollectionElementCategory.Cell)
				{
					missingSections.Add(NSIndexSet.FromIndex(attribute.IndexPath.Section));
				}
			}

			foreach (var attribute in answer)
			{
				if (attribute.RepresentedElementCategory == UICollectionElementCategory.SupplementaryView)
				{
					missingSections.Remove(NSIndexSet.FromIndex(attribute.IndexPath.Section));
				}
			}

			foreach (var item in missingSections)
			{
				item.EnumerateIndexes(delegate(nuint idx, ref bool stop)
				{
					NSIndexPath indexPath = NSIndexPath.FromItemSection(0, (int)idx);
					UICollectionViewLayoutAttributes attributes = this.LayoutAttributesForSupplementaryView(UICollectionElementKindSection.Header, indexPath);
					answer.Add(attributes);
				});
			}

			foreach (var attributes in answer)
			{
				if (attributes.Description.Contains(UICollectionElementKindSection.Header.ToString()))
				{
					nint section = attributes.IndexPath.Section;
					nint numberOfItemsInSection = CollectionView.NumberOfItemsInSection(section);

					if (numberOfItemsInSection > 0)
					{
						NSIndexPath firstCellIndexPath = NSIndexPath.FromItemSection(0, section);
						NSIndexPath lastCellIndexPath = NSIndexPath.FromItemSection((int)Math.Max(0, numberOfItemsInSection - 1), (int)section);

						UICollectionViewLayoutAttributes firstCellAttrs = this.LayoutAttributesForItem(firstCellIndexPath);
						UICollectionViewLayoutAttributes lastCellAttrs = this.LayoutAttributesForItem(lastCellIndexPath);

						nfloat headerHeight = attributes.Frame.Height;
						CGPoint origin = new CGPoint(attributes.Frame.X, attributes.Frame.Y);
						origin.Y = (nfloat)Math.Min((nfloat)Math.Max(this.CollectionView.ContentOffset.Y, firstCellAttrs.Frame.Y - headerHeight), lastCellAttrs.Frame.Y - headerHeight);
										
						attributes.ZIndex = 1024;
						attributes.Frame = new CGRect(origin, attributes.Frame.Size);
					}
				}
			}
//
			for(int i = 1; i < answer.Count; ++i) 
			{
				UICollectionViewLayoutAttributes currentLayoutAttributes = answer[i];
				UICollectionViewLayoutAttributes prevLayoutAttributes = answer[i - 1];
	
				if ( prevLayoutAttributes.IndexPath.Section == currentLayoutAttributes.IndexPath.Section )
				{
					nint maximumSpacing = 10;
					var origin = prevLayoutAttributes.Frame.Right;
					if ( origin + maximumSpacing + currentLayoutAttributes.Frame.Width < this.CollectionViewContentSize.Width )
					{
						CGRect frame = currentLayoutAttributes.Frame;
						frame.X = origin + maximumSpacing;
						currentLayoutAttributes.Frame = frame;
					}
				}
			}

			return answer.ToArray();
		}
	}
}

