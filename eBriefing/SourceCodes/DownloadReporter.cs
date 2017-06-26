using System;
using Foundation;
using UIKit;

namespace eBriefingMobile
{
	public static class DownloadReporter
	{
		public static BookshelfViewController MyBooksVC { get; set; }

		public static void UpdateProgress()
		{
			if (BookUpdater.ParentViewController == MyBooksVC)
			{
				MyBooksVC.UpdateProgress();
			}
		}

		public static void DownloadFinished(bool updated)
		{
			if (BookUpdater.ParentViewController == MyBooksVC)
			{
				MyBooksVC.DownloadFinished(updated);
			}
		}

		public static void ShowAlertView()
		{
			if (MyBooksVC != null)
			{
				String message = "We're sorry, but we failed to download " + BookUpdater.CurrentBook.FailedURLs.Count.ToString() + " page(s) in " + BookUpdater.CurrentBook.Title + ". Press 'Yes' to download them again or 'No' to cancel downloading this book.";
				UIAlertView alert = new UIAlertView(StringRef.alert, message, null, StringRef.no, StringRef.yes);
				alert.Dismissed += (object sender, UIButtonEventArgs e) =>
				{
					if (e.ButtonIndex == 0)
					{
						MyBooksVC.CancelDownload(BookUpdater.CurrentBook);
					}
					else
					{
						MyBooksVC.RedownloadFailedURLs();
					}
				};
				alert.Show();
			}
		}
	}
}

