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

namespace eBriefingMobile
{
    public static class BooksDataAccessor
    {
        private static PersistantStorageDatabase<Book> _database = new PersistantStorageDatabase<Book>("Books");
        private static String _tableId = "books";

        public static void AddBook(Book book)
        {
            if (book != null)
            {
                _database.SetRecord(_tableId, book.ID, book);
            }
        }

        public static void RemoveBook(String bookID)
        {
            _database.DeleteRecord(_tableId, bookID);
        }

        public static void UpdateBook(Book book)
        {
            AddBook(book);
        }

        public static void UpdateUserModifiedDate(Book book)
        {
            if (book != null)
            {
                book.UserModifiedDate = DateTime.UtcNow;

                UpdateBook(book);
            }
        }

        public static List<Book> GetAllBooks()
        {
            List<Book> result = _database.GetRecordsInTable(_tableId);
            if (result == null || result.Count == 0)
            {
                return null;
            }

            return result;
        }

        public static List<Book> GetBooks()
        {
            List<Book> list = _database.GetRecordsInTable(_tableId);
            if (list == null || list.Count == 0)
            {
                return null;
            }
            else
            {
                List<Book> result = null;
                foreach (Book book in list)
                {
					if (!book.Removed)
                    {
                        if (result == null)
                        {
                            result = new List<Book>();
                        }

                        result.Add(book);
                    }
                }

                return result;
            }
        }

        public static List<Book> GetRemovedBooks()
        {
            List<Book> list = _database.GetRecordsInTable(_tableId);
            if (list == null || list.Count == 0)
            {
                return null;
            }
            else
            {
                List<Book> result = null;
                foreach (Book book in list)
                {
                    if (book.Removed)
                    {
                        if (result == null)
                        {
                            result = new List<Book>();
                        }

                        result.Add(book);
                    }
                }

                return result;
            }
        }

        public static Book GetBook(String bookID)
        {
            return _database.GetRecord(_tableId, bookID);
        }

        public static List<Book> GetFavoriteBooks()
        {
            List<Book> bookList = GetBooks();
            if (bookList != null)
            {
				bookList.RemoveAll (x => String.IsNullOrEmpty (x.Title)&& x.PageCount==0 && String.IsNullOrEmpty(x.Description)&& x.ChapterCount==0);
				
                List<Book> favoriteList = null;
                foreach (var book in bookList)
                {
                    if (book.IsFavorite)
                    {
                        if (favoriteList == null)
                        {
                            favoriteList = new List<Book>();
                        }

                        favoriteList.Add(book);
                    }
                }

                return favoriteList;
            }

            return null;
        }

        public static void UpdateFavorite(String bookID, bool value)
        {
            Book book = GetBook(bookID);
            if (book != null)
            {
                book.IsFavorite = value;
                UpdateBook(book);
            }
        }

        public static bool IsFavorite(String bookID)
        {
            Book book = GetBook(bookID);
            if (book != null)
            {
                return book.IsFavorite;
            }

            return false;
        }
    }
}

