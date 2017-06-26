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
using MssFramework;
using Metrostar.Mobile.Framework;

namespace eBriefingMobile
{
	public static class CloudSync
	{
		public static bool SyncingInProgress { get; set; }

		#region Push and Pull

		public static void PushAndPull()
		{
			StartPushAndPull();
		}

		private static void StartPushAndPull()
		{
			// First we push everything. It will only stomp what was on the server.
			StartPush();

			// Second we pull any changes that were on the server, just incase we were not the last used device, or were not synced lately.
			StartPull();
		}

		#endregion

		#region Push

		public static void Push()
		{
			if (Settings.SyncOn && Reachability.IsDefaultNetworkAvailable())
			{
				StartPush();
			}
		}

		private static void StartPush()
		{
			// Push books to the server
			PushMyBooksWork();

			// Push my stuffs to the srever
			List<Book> bookList = BooksOnDeviceAccessor.GetAllBooks();
			if (bookList != null && bookList.Count > 0)
			{
				foreach (var book in bookList)
				{
					PushMyStuffsWork(book);
				}
			}
		}

		private static void PushMyBooksWork()
		{
			try
			{
				List<Book> bookList = BooksOnDeviceAccessor.GetAllBooks();
				if (bookList != null && bookList.Count > 0)
				{
					SaveMyStuff.SetMyBooks(bookList);

					// Remove books that are marked as removed
					List<Book> removeList = BooksOnDeviceAccessor.GetRemovedBooks();
					if (removeList != null && removeList.Count > 0)
					{
						BookRemover.RemoveBooks(removeList);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("CloudSync - PushMyBooksWork: {0}", ex.ToString());
			}
		}

		private static void PushMyStuffsWork(Book book)
		{
			// Push my bookmarks
			PushBookmarksWork(book);

			// Push my notes
			PushNotesWork(book);

			// Push my annotations
			PushAnnotationsWork(book);
		}

		private static void PushBookmarksWork(Book book)
		{
			List<Bookmark> bookmarkList = BooksOnDeviceAccessor.GetAllBookmarks(book.ID);
			if (bookmarkList != null && bookmarkList.Count > 0)
			{
				SaveMyStuff.SetMyBookmarks(bookmarkList);

				// Remove bookmarks that are marked as Removed
				foreach (var bookmark in bookmarkList)
				{
					if (bookmark.Removed)
					{
						BooksOnDeviceAccessor.RemoveBookmark(bookmark.BookID, bookmark.PageID);
					}
				}
			}
		}

		private static void PushNotesWork(Book book)
		{
			List<Note> noteList = BooksOnDeviceAccessor.GetAllNotes(book.ID);
			if (noteList != null && noteList.Count > 0)
			{
				if (String.IsNullOrEmpty(URL.MultipleNoteURL))
				{
					foreach (var note in noteList)
					{
						SaveMyStuff.SetMyNote(note);

						// Remove note marked as Removed
						if (note.Removed)
						{
							BooksOnDeviceAccessor.RemoveNote(note.BookID, note.PageID);
						}
					}
				}
				else
				{
					SaveMyStuff.SetMyNotes(noteList);

					// Remove notes that are marked as Removed
					foreach (var note in noteList)
					{
						if (note.Removed)
						{
							BooksOnDeviceAccessor.RemoveNote(note.BookID, note.NoteID);
						}
					}
				}
			}
		}

		private static void PushAnnotationsWork(Book book)
		{
			List<Annotation> annList = BooksOnDeviceAccessor.GetAllAnnotations(book.ID);
			if (annList != null && annList.Count > 0)
			{
				foreach (var ann in annList)
				{
					if (ann.Removed)
					{
						SaveMyStuff.RemoveMyAnnotation(ann);

						// Remove annotations that are marked as Removed
						BooksOnDeviceAccessor.RemoveAnnotation(ann.BookID, ann.PageID);
					}
					else
					{
						SaveMyStuff.SetMyAnnotation(ann);
					}
				}
			}
		}

		#endregion

		#region Pull

		private static void StartPull()
		{
			// Pull books from the server
			PullMyBooksWork();

			// Pull my stuffs from the server
			List<Book> bookList = BooksOnDeviceAccessor.GetAllBooks();
			if (bookList != null && bookList.Count > 0)
			{
				foreach (var book in bookList)
				{
					PullMyStuffsWork(book);
				}
			}
		}

		private static void PullMyBooksWork()
		{
			try
			{
				List<Book> sBooks = SaveMyStuff.GetMyBooks();
				if (sBooks != null && sBooks.Count > 0)
				{
					foreach (var sBook in sBooks)
					{
						Book dBook = BooksOnDeviceAccessor.GetBook(sBook.ID);
						if (dBook == null)
						{
							// This is a new book from the server
							BooksOnDeviceAccessor.AddBook(sBook);
						}
						else
						{
							if (dBook.ServerModifiedDate <= sBook.ServerModifiedDate)
							{
								if (sBook.Removed)
								{
									// Remove bookmark if the bookmark on the cloud has 'Removed' checked
									BookRemover.RemoveBook(sBook);
								}
								else
								{
									sBook.UserAddedDate = DateTime.UtcNow;
									sBook.New = true;

									BooksOnDeviceAccessor.UpdateBook(sBook);
								}
							}
						}
					}
				}

				// Download detail information about these books
				List<Book> bookList = eBriefingService.StartDownloadBooks();
				if (bookList != null && bookList.Count > 0)
				{
					List<Book> dBooks = BooksOnDeviceAccessor.GetBooks();
					if (dBooks != null && dBooks.Count > 0)
					{
						foreach (var book in bookList)
						{
							var item = dBooks.Where(i => i.ID == book.ID && i.Status == Book.BookStatus.NONE).FirstOrDefault();
							if (item != null)
							{
								book.Status = Book.BookStatus.PENDING2DOWNLOAD;
								book.New = true;
								BooksOnDeviceAccessor.UpdateBook(book);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("CloudSync - PullMyBooksWork: {0}", ex.ToString());
			}
		}

		public static void PullMyStuffs(Book book)
		{
			if (Settings.SyncOn && Reachability.IsDefaultNetworkAvailable())
			{
				PullMyStuffsWork(book);
			}
		}

		private static void PullMyStuffsWork(Book book)
		{
			// Push my bookmarks
			PullBookmarksWork(book);

			// Push my notes
			PullNotesWork(book);

			// Push my annotations
			PullAnnotationsWork(book);
		}

		private static void PullBookmarksWork(Book book)
		{
			List<Bookmark> sBookmarks = SaveMyStuff.GetMyBookmarks(book.ID);
			if (sBookmarks != null && sBookmarks.Count > 0)
			{
				foreach (var sBookmark in sBookmarks)
				{
					Bookmark dBookmark = BooksOnDeviceAccessor.GetBookmark(book.ID, sBookmark.PageID);
					if (dBookmark == null)
					{
						// This is a new bookmark from the server
						BooksOnDeviceAccessor.AddBookmark(sBookmark);
					}
					else
					{
						if (dBookmark.ModifiedUtc <= sBookmark.ModifiedUtc)
						{
							if (sBookmark.Removed)
							{
								// Remove bookmark if the bookmark on the cloud has 'Removed' checked
								BooksOnDeviceAccessor.RemoveBookmark(dBookmark.BookID, dBookmark.PageID);
							}
							else
							{
								BooksOnDeviceAccessor.UpdateBookmark(sBookmark);
							}
						}
					}
				}
			}
		}

		private static void PullNotesWork(Book book)
		{
			List<Note> sNotes = null;
			if (String.IsNullOrEmpty(URL.MultipleNoteURL))
			{
				sNotes = SaveMyStuff.GetMyNotes(book.ID);
			}
			else
			{
				sNotes = SaveMyStuff.GetMyNotes(book.ID, book.LastSyncedDate);
			}

			if (sNotes != null && sNotes.Count > 0)
			{
				foreach (var sNote in sNotes)
				{
					Note dNote = null;
					if (String.IsNullOrEmpty(URL.MultipleNoteURL))
					{
						dNote = BooksOnDeviceAccessor.GetNote(book.ID, sNote.PageID);
					}
					else
					{
						dNote = BooksOnDeviceAccessor.GetNote(book.ID, sNote.NoteID);
					}

					if (dNote == null)
					{
						// This is a new note from the server
						BooksOnDeviceAccessor.AddNote(sNote);
					}
					else
					{
						if (dNote.ModifiedUtc <= sNote.ModifiedUtc)
						{
							if (sNote.Removed)
							{
								// Remove note if the note on the cloud has 'Removed' checked
								if (String.IsNullOrEmpty(URL.MultipleNoteURL))
								{
									BooksOnDeviceAccessor.RemoveNote(dNote.BookID, dNote.PageID);
								}
								else
								{
									BooksOnDeviceAccessor.RemoveNote(dNote.BookID, dNote.NoteID);
								}
							}
							else
							{
								BooksOnDeviceAccessor.UpdateNote(sNote);
							}
						}
					}
				}
			}

			// Update the last time synced for this book
			book.LastSyncedDate = DateTime.UtcNow;
			BooksOnDeviceAccessor.UpdateBook(book);
		}

		private static void PullAnnotationsWork(Book book)
		{
			List<Annotation> sAnnotations = SaveMyStuff.GetMyAnnotations(book.ID);
			if (sAnnotations != null && sAnnotations.Count > 0)
			{
				foreach (var sAnn in sAnnotations)
				{
					Annotation dAnn = BooksOnDeviceAccessor.GetAnnotation(book.ID, sAnn.PageID);
					if (dAnn == null)
					{
						// This is a new note from the server
						BooksOnDeviceAccessor.AddAnnotation(sAnn);
					}
					else
					{
						if (dAnn.ModifiedUtc <= sAnn.ModifiedUtc)
						{
							if (sAnn.Removed)
							{
								// Remove annotation if the annotation on the cloud has 'Removed' checked
								BooksOnDeviceAccessor.RemoveAnnotation(dAnn.BookID, dAnn.PageID);
							}
							else
							{
								BooksOnDeviceAccessor.UpdateAnnotation(sAnn);
							}
						}
					}
				}
			}
		}

		#endregion

		#region Delete

		public static void DeleteMyBooks()
		{
			SaveMyStuff.DeleteMyBooks();
		}

		public static void DeleteMyBookmarks()
		{
			SaveMyStuff.DeleteMyBookmarks();
		}

		public static void DeleteMyNotes()
		{
			SaveMyStuff.DeleteMyNotes();
		}

		public static void DeleteMyAnnotations()
		{
			SaveMyStuff.DeleteMyAnnotations();
		}

		public static void DeleteMyStuff()
		{
			SaveMyStuff.DeleteMyStuff();
		}

		#endregion
	}
}