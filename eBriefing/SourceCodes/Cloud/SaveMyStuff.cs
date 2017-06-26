using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Metrostar.Mobile.Framework;
using eBriefing.com.metrostarsystems.staging.ebriefingweb2;
using Newtonsoft.Json;

namespace eBriefingMobile
{
	public class SaveMyStuff
	{
		public delegate void SetMyBooksDelegate();

		public delegate void SetMyBookmarksDelegate();

		public delegate void SetMyNoteDelegate(bool lastItem);

		public delegate void SetMyAnnotationDelegate(bool lastItem);

		public delegate void GetMyBooksDelegate(List<Book> bookList);

		public delegate void GetMyBookmarksDelegate(String bookID, List<Bookmark> bookmarkList, bool lastItem);

		public delegate void GetMyNotesDelegate(String bookID, List<Note> noteList, bool lastItem);

		public delegate void GetMyAnnotationsDelegate(String bookID, List<Annotation> annotationList, bool lastItem);

		public static event SetMyBooksDelegate SetMyBooksEvent;
		public static event SetMyBookmarksDelegate SetMyBookmarksEvent;
		public static event SetMyNoteDelegate SetMyNoteEvent;
		public static event SetMyAnnotationDelegate SetMyAnnotationEvent;
		public static event GetMyBooksDelegate GetMyBooksEvent;
		public static event GetMyBookmarksDelegate GetMyBookmarksEvent;
		public static event GetMyNotesDelegate GetMyNotesEvent;
		public static event GetMyAnnotationsDelegate GetMyAnnotationsEvent;

		#region Book

		public static void GetMyBooks()
		{
			try
			{
				List<Book> bookList = null;
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += delegate
				{
					SaveMyStuff_2 webService = new SaveMyStuff_2(Server.GenerateContentSyncURL(Settings.ServerURL));
					if (Settings.UseFormsAuth)
					{
						webService.CookieContainer = Authenticate.GetCookieContainer();
					}
					else
					{
						webService.Credentials = KeychainAccessor.NetworkCredential;
					}
					MyBookObj[] results = webService.GetMyBooks();

					if (results != null && results.Length > 0)
					{
						bookList = new List<Book>();
						foreach (MyBookObj mb in results)
						{
							Book book = GenerateBook(mb);
							bookList.Add(book);
						}
					}
				};
				worker.RunWorkerCompleted += delegate
				{
					if (GetMyBooksEvent != null)
					{
						GetMyBooksEvent(bookList);
					}
				};
				worker.RunWorkerAsync();
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - GetMyBooks: {0}", ex.ToString());
			}
		}

		public static void SetMyBooks(List<Book> bookList)
		{
			try
			{
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += delegate
				{
					SaveMyStuff_2 webService = new SaveMyStuff_2(Server.GenerateContentSyncURL(Settings.ServerURL));
					if (Settings.UseFormsAuth)
					{
						webService.CookieContainer = Authenticate.GetCookieContainer();
					}
					else
					{
						webService.Credentials = KeychainAccessor.NetworkCredential;
					}

					MyBookObj[] myBooks = new MyBookObj[bookList.Count];
					for (Int32 i = 0; i < bookList.Count; i++)
					{
						MyBookObj mb = GenerateBookObj(bookList[i]);
						myBooks[i] = mb;
					}

					webService.SetMyBooks(myBooks);
				};
				worker.RunWorkerCompleted += delegate
				{
					if (SetMyBooksEvent != null)
					{
						SetMyBooksEvent();
					}
				};
				worker.RunWorkerAsync();
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

		public static void GetMyBookmarks(String bookID, bool lastItem)
		{
			try
			{
				List<Bookmark> bookmarkList = null;
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += delegate
				{
					SaveMyStuff_2 webService = new SaveMyStuff_2(Server.GenerateContentSyncURL(Settings.ServerURL));
					if (Settings.UseFormsAuth)
					{
						webService.CookieContainer = Authenticate.GetCookieContainer();
					}
					else
					{
						webService.Credentials = KeychainAccessor.NetworkCredential;
					}
					BookmarkObj[] results = webService.GetMyBookmarks(bookID);

					if (results != null && results.Length > 0)
					{
						bookmarkList = new List<Bookmark>();
						foreach (BookmarkObj bm in results)
						{
							Bookmark bookmark = GenerateBookmark(bm);
							bookmarkList.Add(bookmark);
						}
					}
				};
				worker.RunWorkerCompleted += delegate
				{
					if (GetMyBookmarksEvent != null)
					{
						GetMyBookmarksEvent(bookID, bookmarkList, lastItem);
					}
				};
				worker.RunWorkerAsync();
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - GetMyBookmarks: {0}", ex.ToString());
			}
		}

		public static void SetMyBookmarks(List<Bookmark> bookmarkList)
		{
			try
			{
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += delegate
				{
					SaveMyStuff_2 webService = new SaveMyStuff_2(Server.GenerateContentSyncURL(Settings.ServerURL));
					if (Settings.UseFormsAuth)
					{
						webService.CookieContainer = Authenticate.GetCookieContainer();
					}
					else
					{
						webService.Credentials = KeychainAccessor.NetworkCredential;
					}

					BookmarkObj[] myBookmarks = new BookmarkObj[bookmarkList.Count];
					for (Int32 i = 0; i < bookmarkList.Count; i++)
					{
						BookmarkObj bm = GenerateBookmarkObj(bookmarkList[i]);
						myBookmarks[i] = bm;
					}

					webService.SetMyBookmarks(myBookmarks);
				};
				worker.RunWorkerCompleted += delegate
				{
					if (SetMyBookmarksEvent != null)
					{
						SetMyBookmarksEvent();
					}
				};
				worker.RunWorkerAsync();
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

		#region Note

		public static void GetMyNotes(String bookID, bool lastItem)
		{
			try
			{
				List<Note> noteList = null;
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += delegate
				{
					SaveMyStuff_2 webService = new SaveMyStuff_2(Server.GenerateContentSyncURL(Settings.ServerURL));
					if (Settings.UseFormsAuth)
					{
						webService.CookieContainer = Authenticate.GetCookieContainer();
					}
					else
					{
						webService.Credentials = KeychainAccessor.NetworkCredential;
					}
					TextAnnotationObj[] results = webService.GetMyTextAnnotations(bookID);

					if (results != null && results.Length > 0)
					{
						noteList = new List<Note>();
						foreach (TextAnnotationObj ta in results)
						{
							Note note = GenerateNote(ta);
							noteList.Add(note);
						}
					}
				};
				worker.RunWorkerCompleted += delegate
				{
					if (GetMyNotesEvent != null)
					{
						GetMyNotesEvent(bookID, noteList, lastItem);
					}
				};
				worker.RunWorkerAsync();
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - GetMyNotes: {0}", ex.ToString());
			}
		}

		public static void SetMyNote(Note note, bool lastItem)
		{
			try
			{
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += delegate
				{
					SaveMyStuff_2 webService = new SaveMyStuff_2(Server.GenerateContentSyncURL(Settings.ServerURL));
					if (Settings.UseFormsAuth)
					{
						webService.CookieContainer = Authenticate.GetCookieContainer();
					}
					else
					{
						webService.Credentials = KeychainAccessor.NetworkCredential;
					}
					webService.SetMyTextAnnotation(note.BookID, note.BookVersion, note.PageID, note.ModifiedUtc, note.Text);
				};
				worker.RunWorkerCompleted += delegate
				{
					if (SetMyNoteEvent != null)
					{
						SetMyNoteEvent(lastItem);
					}
				};
				worker.RunWorkerAsync();
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - SetMyNote: {0}", ex.ToString());
			}
		}

		public static void RemoveMyNote(Note note, bool lastItem)
		{
			try
			{
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += delegate
				{
					SaveMyStuff_2 webService = new SaveMyStuff_2(Server.GenerateContentSyncURL(Settings.ServerURL));
					if (Settings.UseFormsAuth)
					{
						webService.CookieContainer = Authenticate.GetCookieContainer();
					}
					else
					{
						webService.Credentials = KeychainAccessor.NetworkCredential;
					}
					webService.RemoveMyTextAnnotation(note.BookID, note.PageID, note.ModifiedUtc);
				};
				worker.RunWorkerCompleted += delegate
				{
					if (SetMyNoteEvent != null)
					{
						SetMyNoteEvent(lastItem);
					}
				};
				worker.RunWorkerAsync();
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - RemoveMyNote: {0}", ex.ToString());
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

		#region Annotation

		public static void GetMyAnnotations(String bookID, bool lastItem)
		{
			try
			{
				List<Annotation> annotationList = null;
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += delegate
				{
					SaveMyStuff_2 webService = new SaveMyStuff_2(Server.GenerateContentSyncURL(Settings.ServerURL));
					if (Settings.UseFormsAuth)
					{
						webService.CookieContainer = Authenticate.GetCookieContainer();
					}
					else
					{
						webService.Credentials = KeychainAccessor.NetworkCredential;
					}
					PenAnnotationObj[] results = webService.GetMyPenAnnotations(bookID, "iOS");

					if (results != null && results.Length > 0)
					{
						annotationList = new List<Annotation>();
						foreach (PenAnnotationObj pa in results)
						{
							Annotation ann = GenerateAnnotation(pa);
							annotationList.Add(ann);
						}
					}
				};
				worker.RunWorkerCompleted += delegate
				{
					if (GetMyAnnotationsEvent != null)
					{
						GetMyAnnotationsEvent(bookID, annotationList, lastItem);
					}
				};
				worker.RunWorkerAsync();
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - GetMyAnnotations: {0}", ex.ToString());
			}
		}

		public static void SetMyAnnotation(Annotation ann, bool lastItem)
		{
			try
			{
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += delegate
				{
					SaveMyStuff_2 webService = new SaveMyStuff_2(Server.GenerateContentSyncURL(Settings.ServerURL));
					if (Settings.UseFormsAuth)
					{
						webService.CookieContainer = Authenticate.GetCookieContainer();
					}
					else
					{
						webService.Credentials = KeychainAccessor.NetworkCredential;
					}
					webService.SetMyPenAnnotation(ann.BookID, ann.BookVersion, ann.PageID, "iOS", ann.ModifiedUtc, JsonConvert.SerializeObject(ann.Items), null);
				};
				worker.RunWorkerCompleted += delegate
				{
					if (SetMyAnnotationEvent != null)
					{
						SetMyAnnotationEvent(lastItem);
					}
				};
				worker.RunWorkerAsync();
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("SaveMyStuff - SetMyAnnotation: {0}", ex.ToString());
			}
		}

		public static void RemoveMyAnnotation(Annotation ann, bool lastItem)
		{
			try
			{
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += delegate
				{
					SaveMyStuff_2 webService = new SaveMyStuff_2(Server.GenerateContentSyncURL(Settings.ServerURL));
					if (Settings.UseFormsAuth)
					{
						webService.CookieContainer = Authenticate.GetCookieContainer();
					}
					else
					{
						webService.Credentials = KeychainAccessor.NetworkCredential;
					}
					webService.RemoveMyPenAnnotation(ann.BookID, ann.PageID, "iOS", ann.ModifiedUtc);
				};
				worker.RunWorkerCompleted += delegate
				{
					if (SetMyAnnotationEvent != null)
					{
						SetMyAnnotationEvent(lastItem);
					}
				};
				worker.RunWorkerAsync();
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
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += delegate
				{
					SaveMyStuff_2 webService = new SaveMyStuff_2(Server.GenerateContentSyncURL(Settings.ServerURL));
					if (Settings.UseFormsAuth)
					{
						webService.CookieContainer = Authenticate.GetCookieContainer();
					}
					else
					{
						webService.Credentials = KeychainAccessor.NetworkCredential;
					}
					webService.DeleteMyBooks();
				};
				worker.RunWorkerAsync();
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
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += delegate
				{
					SaveMyStuff_2 webService = new SaveMyStuff_2(Server.GenerateContentSyncURL(Settings.ServerURL));
					if (Settings.UseFormsAuth)
					{
						webService.CookieContainer = Authenticate.GetCookieContainer();
					}
					else
					{
						webService.Credentials = KeychainAccessor.NetworkCredential;
					}
					webService.DeleteMyBookmarks();
				};
				worker.RunWorkerAsync();
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
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += delegate
				{
					SaveMyStuff_2 webService = new SaveMyStuff_2(Server.GenerateContentSyncURL(Settings.ServerURL));
					if (Settings.UseFormsAuth)
					{
						webService.CookieContainer = Authenticate.GetCookieContainer();
					}
					else
					{
						webService.Credentials = KeychainAccessor.NetworkCredential;
					}
					webService.DeleteMyTextAnnotations();
				};
				worker.RunWorkerAsync();
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
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += delegate
				{
					SaveMyStuff_2 webService = new SaveMyStuff_2(Server.GenerateContentSyncURL(Settings.ServerURL));
					if (Settings.UseFormsAuth)
					{
						webService.CookieContainer = Authenticate.GetCookieContainer();
					}
					else
					{
						webService.Credentials = KeychainAccessor.NetworkCredential;
					}
					webService.DeleteMyPenAnnotations();
				};
				worker.RunWorkerAsync();
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
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += delegate
				{
					SaveMyStuff_2 webService = new SaveMyStuff_2(Server.GenerateContentSyncURL(Settings.ServerURL));
					if (Settings.UseFormsAuth)
					{
						webService.CookieContainer = Authenticate.GetCookieContainer();
					}
					else
					{
						webService.Credentials = KeychainAccessor.NetworkCredential;
					}
					webService.DeleteMyStuff();
				};
				worker.RunWorkerAsync();
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