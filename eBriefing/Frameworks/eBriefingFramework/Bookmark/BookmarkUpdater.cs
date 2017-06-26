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
using CoreGraphics;
using Foundation;
using UIKit;

namespace eBriefingMobile
{
    public static class BookmarkUpdater
    {
        public static void SaveBookmark(Book book, String pageID, String text)
        {
            Bookmark bookmark = BooksOnDeviceAccessor.GetBookmark(book.ID, pageID);
            if (bookmark == null)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    bookmark = GenerateBookmarkObj(book, pageID, text);
                    Add(bookmark);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(text))
                {
                    Remove(book.ID, pageID);
                }
                else
                {
                    bookmark = GenerateBookmarkObj(book, pageID, text);
                    Update(bookmark);
                }
            }

            BooksOnDeviceAccessor.UpdateUserModifiedDate(book);
        }

        private static void Add(Bookmark bookmark)
        {
            BooksOnDeviceAccessor.AddBookmark(bookmark);
        }

        private static void Remove(String bookID, String pageID)
        {
            BooksOnDeviceAccessor.MarkAsRemovedBookmark(bookID, pageID);
        }

        private static void Update(Bookmark bookmark)
        {
            BooksOnDeviceAccessor.UpdateBookmark(bookmark);
        }

        private static Bookmark GenerateBookmarkObj(Book book, String pageID, String text)
        {
            Bookmark bookmark = new Bookmark();
            bookmark.BookID = book.ID;
            bookmark.PageID = pageID;
            bookmark.Title = text;
            bookmark.BookVersion = book.Version;
            bookmark.ModifiedUtc = DateTime.UtcNow;

            return bookmark;
        }
    }
}

