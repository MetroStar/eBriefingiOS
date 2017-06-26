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

namespace eBriefingMobile
{
	public partial class UpdateViewController : BookshelfViewController
	{
		public UpdateViewController()
		{

		}

		public override void DidReceiveMemoryWarning()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			
			// Perform any additional setup after loading the view, typically from a nib.
			this.Title = StringRef.updates;
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			BookUpdater.RegisterASIDelegate(this);

			RetrieveBooks();
		}

		public void RetrieveBooks()
		{
			if (BookUpdater.Books2Update != null)
			{
				UpdateUI(new List<Book>(BookUpdater.Books2Update));
			}
			else
			{
				UpdateUI(null);
			}
		}

		private void UpdateUI(List<Book> bookList)
		{
			if (bookList == null || bookList.Count == 0)
			{
				UpdateStatusLabel();
			}
			else
			{
				ShowHideStatusLabel(false);

				LoadCollectionView(bookList, true);
			}
		}

		protected override void UpdateStatusLabel()
		{
			statusLabel.Text = "There are no Updates.";

			base.UpdateStatusLabel();

			ClearUpdatesBadge();
		}

		protected override void RemoveBook(Book book)
		{
			UpdateUpdatesBadge();

			// Remove from update list
			if (BookUpdater.Books2Update != null)
			{
				BookUpdater.RemoveBookFromBooks2Update(book.ID);

				UpdateUpdatesBadge();
			}

			base.RemoveBook(book);
		}
	}
}

