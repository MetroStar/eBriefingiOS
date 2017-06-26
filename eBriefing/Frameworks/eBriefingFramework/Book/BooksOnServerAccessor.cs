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
using System.Threading.Tasks;
using System.Collections.Generic;
using Metrostar.Mobile.Framework;

namespace eBriefingMobile
{
    public class BooksOnServerAccessor
    {
        private static String mainKey = "Server/";
        private static String bookKey = "/Books/";
        private static String chapterKey = "/Chapters";
        private static String pageKey = "/Pages";

        public static bool HasBooks()
        {
            if (!String.IsNullOrEmpty(URL.ServerURL))
            {
                object record = PersistantObjectsStorage.GetValue(mainKey + URL.ServerURL + bookKey);
                if (record != null)
                {
                    return true;
                }
            }
            
            return false;
        }

        public static bool HasChapters(String bookID)
        {
            if (!String.IsNullOrEmpty(URL.ServerURL))
            {
                object record = PersistantObjectsStorage.GetValue(mainKey + URL.ServerURL + bookKey + bookID + chapterKey);
                if (record != null)
                {
                    return true;
                }
            }
            
            return false;
        }

        public static bool HasPages(String bookID)
        {
            if (!String.IsNullOrEmpty(URL.ServerURL))
            {
                object record = PersistantObjectsStorage.GetValue(mainKey + URL.ServerURL + bookKey + bookID + chapterKey + pageKey);
                if (record != null)
                {
                    return true;
                }
            }
            
            return false;
        }

        public static Book GetBook(String bookID)
        {
            var item = GetBooks().Where(i => i.ID == bookID).FirstOrDefault();
            if (item != null)
            {
                return item;
            }

            return null;
        }

        public static List<Book> GetBooks()
        {
            if (!String.IsNullOrEmpty(URL.ServerURL))
            {
                object record = PersistantObjectsStorage.GetValue(mainKey + URL.ServerURL + bookKey);
                if (record != null)
                {
                    return (List<Book>)record;
                }
            }

            return null;
        }

        public static List<Chapter> GetChapters(String bookID)
        {
            if (!String.IsNullOrEmpty(URL.ServerURL))
            {
                object record = PersistantObjectsStorage.GetValue(mainKey + URL.ServerURL + bookKey + bookID + chapterKey);
                if (record != null)
                {
                    return (List<Chapter>)record;
                }
            }
            
            return null;
        }

        public static List<Page> GetPages(String bookID)
        {
            if (!String.IsNullOrEmpty(URL.ServerURL))
            {
                object record = PersistantObjectsStorage.GetValue(mainKey + URL.ServerURL + bookKey + bookID + chapterKey + pageKey);
                if (record != null)
                {
                    return (List<Page>)record;
                }
            }
            
            return null;
        }

        public static void UpdateViewed()
        {
            List<Book> bookList = GetBooks();
            if (bookList != null)
            {
                foreach (Book book in bookList)
                {
                    book.Viewed = true;
                }

                SaveBooks(bookList);
            }
        }

        public static void SaveBooks(List<Book> list)
        {
            if (!String.IsNullOrEmpty(URL.ServerURL))
            {
                if (list != null)
                {
                    List<Book> bookList = GetBooks();
                    if (bookList != null)
                    {
                        foreach (Book b1 in list)
                        {
                            foreach (Book b2 in bookList)
                            {
                                if (b1.ID == b2.ID)
                                {
                                    b1.Viewed = b2.Viewed;
                                    break;
                                }
                            }
                        }
                    }

                    PersistantObjectsStorage.Add(mainKey + URL.ServerURL + bookKey, list);
                }
            }
        }

        public static void SaveChapters(String bookID, List<Chapter> list)
        {
            if (!String.IsNullOrEmpty(URL.ServerURL))
            {
                PersistantObjectsStorage.Add(mainKey + URL.ServerURL + bookKey + bookID + chapterKey, list);
            }
        }

        public static void SavePages(String bookID, List<Page> list)
        {
            if (!String.IsNullOrEmpty(URL.ServerURL))
            {
                PersistantObjectsStorage.Add(mainKey + URL.ServerURL + bookKey + bookID + chapterKey + pageKey, list);
            }
        }

        public static void RemoveChapters(String bookID)
        {
            if (!String.IsNullOrEmpty(URL.ServerURL))
            {
                SaveChapters(bookID, null);
            }
        }

        public static void RemovePages(String bookID)
        {
            if (!String.IsNullOrEmpty(URL.ServerURL))
            {
                SavePages(bookID, null);
            }
        }
    }
}

