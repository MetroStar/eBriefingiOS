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

namespace eBriefingMobile
{
    public static class BookmarksDataAccessor
    {
        private static PersistantStorageDatabase<Bookmark> _database = new PersistantStorageDatabase<Bookmark>("Bookmarks");

        public static void AddBookmark(Bookmark bookmark)
        {
            _database.SetRecord(bookmark.BookID, bookmark.PageID, bookmark);
        }

        public static void RemoveBookmark(String bookID, String pageID)
        {
            _database.DeleteRecord(bookID, pageID);
        }

        public static void UpdateBookmark(Bookmark bookmark)
        {
            AddBookmark(bookmark);
        }

        public static void RemoveOrphanBookmarks(String bookID, List<Page> newPageList)
        {
            List<Page> oldPageList = PagesOnDeviceDataAccessor.GetPages(bookID);
            if (oldPageList != null)
            {
                foreach (Page oldPage in oldPageList)
                {
                    var item = newPageList.Where(i => i.ID == oldPage.ID).FirstOrDefault();
                    if (item == null)
                    {
                        RemoveBookmark(bookID, oldPage.ID);
                    }
                }
            }
        }

        public static List<Bookmark> GetAllBookmarks(String bookID)
        {
            List<Bookmark> result = _database.GetRecordsInTable(bookID);
            if (result == null || result.Count == 0)
            {
                return null;
            }

            return result;
        }

        public static List<Bookmark> GetBookmarks(String bookID)
        {
            List<Bookmark> list = _database.GetRecordsInTable(bookID);
            if (list == null || list.Count == 0)
            {
                return null;
            }
            else
            {
                List<Bookmark> result = null;
                foreach (Bookmark bookmark in list)
                {
                    if (!bookmark.Removed)
                    {
                        if (result == null)
                        {
                            result = new List<Bookmark>();
                        }

                        result.Add(bookmark);
                    }
                }

                return result;
            }
        }

        public static List<Bookmark> GetRemovedBookmarks(String bookID)
        {
            List<Bookmark> list = _database.GetRecordsInTable(bookID);
            if (list == null || list.Count == 0)
            {
                return null;
            }
            else
            {
                List<Bookmark> result = null;
                foreach (Bookmark bookmark in list)
                {
                    if (bookmark.Removed)
                    {
                        if (result == null)
                        {
                            result = new List<Bookmark>();
                        }

                        result.Add(bookmark);
                    }
                }

                return result;
            }
        }

        public static Bookmark GetBookmark(String bookID, String pageID)
        {
            Bookmark bookmark = _database.GetRecord(bookID, pageID);
            if (bookmark != null && !String.IsNullOrEmpty(bookmark.Title) && !bookmark.Removed)
            {
                return bookmark;
            }

            return null;
        }

        public static int GetNumBookmarksInBook(String bookID)
        {
            return GetNumBookmarks(bookID, PagesOnDeviceDataAccessor.GetPages(bookID));
        }

        public static int GetNumBookmarksInChapter(String bookID, String chapterID)
        {
            return GetNumBookmarks(bookID, PagesOnDeviceDataAccessor.GetPagesInChapter(bookID, chapterID));
        }

        private static int GetNumBookmarks(String bookID, List<Page> pageList)
        {
            int count = 0;

            if (pageList != null)
            {
                foreach (var page in pageList)
                {
                    Bookmark bookmark = GetBookmark(bookID, page.ID);
                    if (bookmark != null && !String.IsNullOrEmpty(bookmark.Title) && !bookmark.Removed)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}

