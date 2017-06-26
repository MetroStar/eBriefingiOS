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
using System.Net;
using CoreGraphics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UIKit;
using Foundation;
using Metrostar.Mobile.Framework;
using eBriefing.com.metrostarsystems.staging.ebriefingweb2;
using eBriefing.com.metrostarsystems.staging.ebriefingweb;
using eBriefing.com.metrostarsystems.staging.eb13;

namespace eBriefingMobile
{
	public class eBriefingService
	{
		#region Run

		public static async Task<T> Run<T>(Func<T> func)
		{
			T result = default(T);

			try
			{
				result = await Task.Run(func);
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("eBriefingService - Run: {0}", ex.ToString());
			}

			return result;
		}

		public static async Task<T> Run<T>(Func<Task<T>> func)
		{
			T result = default(T);

			try
			{
				result = await func();
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("eBriefingService - Run: {0}", ex.ToString());
			}

			return result;
		}

		public static async Task Run(Action func)
		{
			try
			{
				await Task.Run(func);
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("eBriefingService - Run: {0}", ex.ToString());
			}
		}

		public static async Task Run(Action func, CancellationToken token)
		{
			try
			{
				await Task.Run(func, token);
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("eBriefingService - Run: {0}", ex.ToString());
			}
		}

		public static Core_2 GenerateCore_2Client()
		{
			var client = new Core_2(URL.Core2URL);
			if (Settings.UseFormsAuth)
			{
				client.CookieContainer = Authenticate.GetCookieContainer();
			}
			else
			{
				client.Credentials = KeychainAccessor.NetworkCredential;
			}
			return client;
		}

		public static SaveMyStuff_2 GenerateSaveMyStuffClient()
		{
			var client = new SaveMyStuff_2(URL.ContentSyncURL);
			if (Settings.UseFormsAuth)
			{
				client.CookieContainer = Authenticate.GetCookieContainer();
			}
			else
			{
				client.Credentials = KeychainAccessor.NetworkCredential;
			}

			return client;
		}

		public static MultiNotes_1 GenerateMultiNoteClient()
		{
			var client = new MultiNotes_1(URL.MultipleNoteURL);
			if (Settings.UseFormsAuth)
			{
				client.CookieContainer = Authenticate.GetCookieContainer();
			}
			else
			{
				client.Credentials = KeychainAccessor.NetworkCredential;
			}

			return client;
		}

		#endregion

		#region SaveMyBooks

		public static MyBookObj[] GetMyBooksFromCloud()
		{
			SaveMyStuff_2 client = GenerateSaveMyStuffClient();
			return client.GetMyBooks();
		}

		public static void SetMyBooksToCloud(MyBookObj[] myBooks)
		{
			SaveMyStuff_2 client = GenerateSaveMyStuffClient();
			client.SetMyBooks(myBooks);
		}

		public static void DeleteMyBooksFromCloud()
		{
			SaveMyStuff_2 client = GenerateSaveMyStuffClient();
			client.DeleteMyBooks();
		}

		public static void DeleteMyStuffsFromCloud()
		{
			SaveMyStuff_2 client = GenerateSaveMyStuffClient();
			client.DeleteMyStuff();
		}

		#endregion

		#region SaveMyBookmarks

		public static BookmarkObj[] GetMyBookmarksFromCloud(String bookID)
		{
			SaveMyStuff_2 client = GenerateSaveMyStuffClient();
			return client.GetMyBookmarks(bookID);
		}

		public static void SetMyBookmarksToCloud(BookmarkObj[] myBookmarks)
		{
			SaveMyStuff_2 client = GenerateSaveMyStuffClient();
			client.SetMyBookmarks(myBookmarks);
		}

		public static void DeleteMyBookmarksFromCloud()
		{
			SaveMyStuff_2 client = GenerateSaveMyStuffClient();
			client.DeleteMyBookmarks();
		}

		#endregion

		#region Save Old Notes

		public static TextAnnotationObj[] GetMyNotesFromCloud(String bookID)
		{
			SaveMyStuff_2 client = GenerateSaveMyStuffClient();
			return client.GetMyTextAnnotations(bookID);
		}

		public static void SetMyNoteToCloud(Note note)
		{
			SaveMyStuff_2 client = GenerateSaveMyStuffClient();
			client.SetMyTextAnnotation(note.BookID, note.BookVersion, note.PageID, note.ModifiedUtc, note.Text);
		}

		public static void DeleteMyNotesFromCloud()
		{
			SaveMyStuff_2 client = GenerateSaveMyStuffClient();
			client.DeleteMyTextAnnotations();
		}

		#endregion

		#region Save New MultiNotes

		public static eBriefing.com.metrostarsystems.eb13.Note[] GetMyNotesFromCloud(String bookID, DateTime lastSyncedDate)
		{
			MultiNotes_1 client = GenerateMultiNoteClient();
			return client.GetNotesUpdates(bookID, lastSyncedDate);
		}

		public static bool SetMyNotesToCloud(eBriefing.com.metrostarsystems.eb13.Note[] notes)
		{
			MultiNotes_1 client = GenerateMultiNoteClient();
			return client.SaveNotes(notes);
		}

		public static eBriefing.com.metrostarsystems.eb13.Note[] GetAllMyNotesFromCloud(String bookID, String offset, int pageSize)
		{
			MultiNotes_1 client = GenerateMultiNoteClient();
			return client.GetAllNotes(bookID, offset, pageSize);
		}

		#endregion

		#region SaveMyAnnotations

		public static PenAnnotationObj[] GetMyAnnotationsFromCloud(String bookID)
		{
			SaveMyStuff_2 client = GenerateSaveMyStuffClient();
			return client.GetMyPenAnnotations(bookID, "iOS");
		}

		public static void SetMyAnnotationToCloud(String bookID, int bookVersion, String pageID, DateTime modifiedUtc, String content)
		{
			SaveMyStuff_2 client = GenerateSaveMyStuffClient();
			client.SetMyPenAnnotation(bookID, bookVersion, pageID, "iOS", modifiedUtc, content, null);
		}

		public static void RemoveMyAnnotationFromCloud(String bookID, String pageID, DateTime modifiedUtc)
		{
			SaveMyStuff_2 client = GenerateSaveMyStuffClient();
			client.RemoveMyPenAnnotation(bookID, pageID, "iOS", modifiedUtc);
		}

		public static void DeleteMyAnnotationsFromCloud()
		{
			SaveMyStuff_2 client = GenerateSaveMyStuffClient();
			client.DeleteMyPenAnnotations();
		}

		#endregion

		#region BookDownloader

		public static List<Book> StartDownloadBooks()
		{
			List<Book> bookList = null;

			try
			{
				Core_2 client = GenerateCore_2Client();
				GetBooksReturn[] results = client.GetBooks();
				if (results != null)
				{
					bookList = new List<Book>();

					foreach (GetBooksReturn r in results)
					{
						if (!String.IsNullOrEmpty(r.ID))
						{
							Book book = new Book();
							book.ID = r.ID.ToString();
							book.Title = r.Title.Replace(System.Environment.NewLine, " ");
							book.Description = r.Description.Replace(System.Environment.NewLine, " ");
							book.ChapterCount = r.ChapterCount;
							book.PageCount = r.PageCount;
							book.SmallImageURL = r.SmallImageURL;
							book.LargeImageURL = r.LargeImageURL;
							book.ImageVersion = r.ImageVersion;
							book.Version = Convert.ToInt32(r.Version);
							book.ServerAddedDate = r.DateAdded;
							book.ServerModifiedDate = r.DateModified;
							book.UserAddedDate = DateTime.UtcNow;
							book.UserModifiedDate = DateTime.UtcNow;
							book.Status = Book.BookStatus.NONE;
							book.Removed = false;
							book.Viewed = false;
							book.LastSyncedDate = DateTime.MinValue;
							bookList.Add(book);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("eBriefingService - StartDownloadBooks: {0}", ex.ToString());
			}

			return bookList;
		}

		#endregion

		#region ChapterDownloader

		public static List<Chapter> StartDownloadChapters(String bookID)
		{
			List<Chapter> chapterList = null;

			try
			{
				Core_2 client = GenerateCore_2Client();
				GetChaptersInBookReturn[] results = client.GetChaptersInBook(bookID);
				if (results != null)
				{
					chapterList = new List<Chapter>();

					int i = 0;
					foreach (GetChaptersInBookReturn r in results)
					{
						Chapter chapter = new Chapter();
						chapter.ID = r.ID.ToString();
						chapter.Title = r.Title;
						chapter.Description = r.Description;
						chapter.Pagecount = r.PageCount;
						chapter.SmallImageURL = r.SmallImageURL;
						chapter.LargeImageURL = r.LargeImageURL;
						chapter.ImageVersion = r.ImageVersion;
						chapter.FirstPageID = r.FirstPageID;
						chapter.ChapterNumber = ++i;

						chapterList.Add(chapter);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("eBriefingService - StartDownloadChapters: {0}", ex.ToString());
			}

			return chapterList;
		}

		#endregion

		#region PageDownloader

		public static List<Page> StartDownloadPages(String bookID)
		{
			List<Page> pageList = null;

			try
			{
				Core_2 client = GenerateCore_2Client();
				GetPagesInBookReturn[] results = client.GetPagesInBook(bookID);
				if (results != null)
				{
					pageList = new List<Page>();

					foreach (GetPagesInBookReturn r in results)
					{
						Page page = new Page();
						page.ID = r.ID.ToString();
						page.URL = r.URL;
						page.PageNumber = r.PageNumber;
						page.MD5 = r.MD5;
						page.Type = r.Type;
						page.Version = r.Version;
						pageList.Add(page);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLineDebugging("eBriefingService - StartDownloadPages: {0}", ex.ToString());
			}

			return pageList;
		}

		#endregion
	}
}

