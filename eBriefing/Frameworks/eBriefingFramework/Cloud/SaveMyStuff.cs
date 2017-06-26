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
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Foundation;
using UIKit;
using Metrostar.Mobile.Framework;
using eBriefing.com.metrostarsystems.ebriefingweb2;
using eBriefing.com.metrostarsystems.eb13;
using Newtonsoft.Json;

namespace eBriefingMobile
{
	public class SaveMyStuff
	{
		#region Book

		public static List<Book> GetMyBooks()
		{
			try
			{
				MyBookObj[] results = eBriefingService.GetMyBooksFromCloud();
				if (results != null && results.Length > 0)
				{
					List<Book> bookList = new List<Book>();
					foreach (MyBookObj mb in results)
					{
						Book book = GenerateBook(mb);
						bookList.Add(book);
					}

					return bookList;
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - GetMyBooks: {0}", ex.ToString());
			}

			return null;
		}

		public static void SetMyBooks(List<Book> bookList)
		{
			try
			{
				MyBookObj[] myBooks = new MyBookObj[bookList.Count];
				for (int i = 0; i < bookList.Count; i++)
				{
					MyBookObj mb = GenerateBookObj(bookList[i]);
					myBooks[i] = mb;
				}

				eBriefingService.SetMyBooksToCloud(myBooks);
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - SetMyBooks: {0}", ex.ToString());
			}
		}

		private static MyBookObj GenerateBookObj(Book book)
		{
			MyBookObj mb = new MyBookObj();
			mb.BookId = book.ID;
			mb.BookVersion = book.Version;
			mb.IsFavorite = book.IsFavorite;
			mb.ModifiedUtc = book.UserModifiedDate;
			mb.Removed = book.Removed;

			return mb;
		}

		private static Book GenerateBook(MyBookObj mb)
		{
			Book book = new Book();
			book.ID = mb.BookId;
			book.Version = mb.BookVersion;
			book.IsFavorite = mb.IsFavorite;
			book.UserModifiedDate = mb.ModifiedUtc;
			book.Removed = mb.Removed;

			return book;
		}

		#endregion

		#region Bookmark

		public static List<Bookmark> GetMyBookmarks(String bookID)
		{
			try
			{
				BookmarkObj[] results = eBriefingService.GetMyBookmarksFromCloud(bookID);
				if (results != null && results.Length > 0)
				{
					List<Bookmark> bookmarkList = new List<Bookmark>();
					foreach (BookmarkObj bm in results)
					{
						Bookmark bookmark = GenerateBookmark(bm);
						bookmarkList.Add(bookmark);
					}

					return bookmarkList;
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - GetMyBookmarks: {0}", ex.ToString());
			}

			return null;
		}

		public static void SetMyBookmarks(List<Bookmark> bookmarkList)
		{
			try
			{
				BookmarkObj[] myBookmarks = new BookmarkObj[bookmarkList.Count];
				for (int i = 0; i < bookmarkList.Count; i++)
				{
					BookmarkObj bm = GenerateBookmarkObj(bookmarkList[i]);
					myBookmarks[i] = bm;
				}

				eBriefingService.SetMyBookmarksToCloud(myBookmarks);
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - SetMyBookmarks: {0}", ex.ToString());
			}
		}

		private static BookmarkObj GenerateBookmarkObj(Bookmark bookmark)
		{
			BookmarkObj bm = new BookmarkObj();
			bm.BookId = bookmark.BookID;
			bm.BookVersion = bookmark.BookVersion;
			bm.PageId = bookmark.PageID;
			bm.Value = bookmark.Title;
			bm.ModifiedUtc = bookmark.ModifiedUtc;
			bm.Removed = bookmark.Removed;

			return bm;
		}

		private static Bookmark GenerateBookmark(BookmarkObj bm)
		{
			Bookmark bookmark = new Bookmark();
			bookmark.BookID = bm.BookId;
			bookmark.BookVersion = bm.BookVersion;
			bookmark.PageID = bm.PageId;
			bookmark.Title = bm.Value;
			bookmark.ModifiedUtc = bm.ModifiedUtc;
			bookmark.Removed = bm.Removed;

			return bookmark;
		}

		#endregion

		#region Old Note

		public static List<Note> GetMyNotes(String bookID)
		{
			try
			{
				TextAnnotationObj[] results = eBriefingService.GetMyNotesFromCloud(bookID);
				if (results != null && results.Length > 0)
				{
					List<Note> noteList = new List<Note>();
					foreach (TextAnnotationObj ta in results)
					{
						Note note = GenerateNote(ta);
						noteList.Add(note);
					}

					return noteList;
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - GetMyNotes: {0}", ex.ToString());
			}

			return null;
		}

		public static void SetMyNote(Note note)
		{
			try
			{
				eBriefingService.SetMyNoteToCloud(note);
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - SetMyNote: {0}", ex.ToString());
			}
		}

		private static Note GenerateNote(TextAnnotationObj ta)
		{
			Note note = new Note();
			note.BookID = ta.BookId;
			note.BookVersion = ta.BookVersion;
			note.PageID = ta.PageId;
			note.ModifiedUtc = ta.ModifiedUtc;
			note.Removed = ta.Removed;

			if (!note.Removed)
			{
				note.Text = DownloadData(ta.ValueUrl);
			}

			return note;
		}

		#endregion

		#region New Notes

		public static List<Note> GetAllMyNotes(String bookID, DateTime lastSyncedDate)
		{
			List<Note> noteList = null;

			try
			{
				string offset = null;
				int count = 0;

				do
				{
					eBriefing.com.metrostarsystems.eb13.Note[] notes = eBriefingService.GetAllMyNotesFromCloud(bookID, offset, 50);
					if (notes != null && notes.Length > 0)
					{
						if (noteList == null)
						{
							noteList = new List<Note>();
						}

						foreach (eBriefing.com.metrostarsystems.eb13.Note ta in notes)
						{
							Note note = GenerateNote(ta);
							noteList.Add(note);
						}

						count = notes.Length;
					}
				} while (50 == count);
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - GetAllMyNotes: {0}", ex.ToString());
			}

			return noteList;
		}

		public static List<Note> GetMyNotes(String bookID, DateTime lastSyncedDate)
		{
			try
			{
				eBriefing.com.metrostarsystems.eb13.Note[] notes = eBriefingService.GetMyNotesFromCloud(bookID, lastSyncedDate);
				if (notes != null && notes.Length > 0)
				{
					List<Note> noteList = new List<Note>();
					foreach (eBriefing.com.yourSharePointBackEndURL.eb13.Note ta in notes)
					{
						Note note = GenerateNote(ta);
						noteList.Add(note);
					}

					return noteList;
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - GetMyNotes: {0}", ex.ToString());
			}

			return null;
		}

		public static void SetMyNotes(List<Note> noteList)
		{
			try
			{
				if (noteList != null)
				{
					eBriefing.com.metrostarsystems.eb13.Note[] notes = new eBriefing.com.metrostarsystems.eb13.Note[noteList.Count];

					for (int i = 0; i < noteList.Count; i++)
					{
						notes[i] = GenerateNoteObj(noteList[i]);
					}

					eBriefingService.SetMyNotesToCloud(notes);
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - SetMyNote: {0}", ex.ToString());
			}
		}


		private static Note GenerateNote(eBriefing.com.metrostarsystems.eb13.Note ta)
		{
			Note note = new Note();
			note.BookID = ta.BookId;
			note.BookVersion = ta.BookVersion;
			note.PageID = ta.PageId;
			note.ModifiedUtc = ta.Modified;
			note.Removed = ta.IsDeleted;
			note.CreatedUtc = ta.Created;
			note.NoteID = ta.NoteId;
			note.Text = ta.NoteText;

			return note;
		}

		private static eBriefing.com.metrostarsystems.eb13.Note GenerateNoteObj(Note note)
		{
			eBriefing.com.metrostarsystems.eb13.Note noteObj = new eBriefing.com.metrostarsystems.eb13.Note();

			noteObj.BookId = note.BookID;
			noteObj.BookVersion = note.BookVersion;
			noteObj.PageId = note.PageID;
			noteObj.Modified = note.ModifiedUtc;
			noteObj.IsDeleted = note.Removed;
			noteObj.Created = note.CreatedUtc;
			noteObj.NoteId = note.NoteID;
			noteObj.NoteText = note.Text;

			return noteObj;
		}

		#endregion

		#region Annotation

		public static List<Annotation> GetMyAnnotations(String bookID)
		{
			try
			{
				PenAnnotationObj[] results = eBriefingService.GetMyAnnotationsFromCloud(bookID);
				if (results != null && results.Length > 0)
				{
					List<Annotation> annotationList = new List<Annotation>();
					foreach (PenAnnotationObj pa in results)
					{
						Annotation ann = GenerateAnnotation(pa);
						annotationList.Add(ann);
					}
                        
					return annotationList;
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - GetMyAnnotations: {0}", ex.ToString());
			}

			return null;
		}

		public static void SetMyAnnotation(Annotation ann)
		{
			try
			{
				eBriefingService.SetMyAnnotationToCloud(ann.BookID, ann.BookVersion, ann.PageID, ann.ModifiedUtc, JsonConvert.SerializeObject(ann.Items));
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - SetMyAnnotation: {0}", ex.ToString());
			}
		}

		public static void RemoveMyAnnotation(Annotation ann)
		{
			try
			{
				eBriefingService.RemoveMyAnnotationFromCloud(ann.BookID, ann.PageID, ann.ModifiedUtc);
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - RemoveMyAnnotation: {0}", ex.ToString());
			}
		}

		private static Annotation GenerateAnnotation(PenAnnotationObj pa)
		{
			Annotation ann = new Annotation();
			ann.BookID = pa.BookId;
			ann.BookVersion = pa.BookVersion;
			ann.PageID = pa.PageId;
			ann.ModifiedUtc = pa.ModifiedUtc;
			ann.Removed = pa.Removed;

			if (!ann.Removed)
			{
				String result = DownloadData(pa.TextDataUrl);
				ann.Items = JsonConvert.DeserializeObject<List<AnnotationItem>>(result);
			}

			return ann;
		}

		#endregion

		#region Delete

		public static void DeleteMyBooks()
		{
			try
			{
				eBriefingService.DeleteMyBooksFromCloud();
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - DeleteMyBooks: {0}", ex.ToString());
			}
		}

		public static void DeleteMyBookmarks()
		{
			try
			{
				eBriefingService.DeleteMyBookmarksFromCloud();
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - DeleteMyBookmarks: {0}", ex.ToString());
			}
		}

		public static void DeleteMyNotes()
		{
			try
			{
				eBriefingService.DeleteMyNotesFromCloud();
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - DeleteMyNotes: {0}", ex.ToString());
			}
		}

		public static void DeleteMyAnnotations()
		{
			try
			{
				eBriefingService.DeleteMyAnnotationsFromCloud();
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - DeleteMyAnnotations: {0}", ex.ToString());
			}
		}

		public static void DeleteMyStuff()
		{
			try
			{
				Settings.SyncOn = false;
				eBriefingService.DeleteMyStuffsFromCloud();
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - DeleteMyStuff: {0}", ex.ToString());
			}
		}

		#endregion

		private static String DownloadData(String url)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

			if (Settings.UseFormsAuth)
			{
				request.CookieContainer = Authenticate.GetCookieContainer();
			}
			else
			{
				request.Credentials = KeychainAccessor.NetworkCredential;
			}

			WebResponse response = request.GetResponse();
			StreamReader reader = new StreamReader(response.GetResponseStream());

			return reader.ReadToEnd();
		}
	}
}