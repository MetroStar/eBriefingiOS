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
using Metrostar.Mobile.Framework;

namespace eBriefingMobile
{
    public static class BookRemover
    {
        public static void RemoveBooks(List<Book> bookList)
        {
            foreach (Book book in bookList)
            {
                RemoveBook(book);
            }
        }

        public static void RemoveBook(Book book)
        {
            // Remove page images from the file system
            List<Page> pageList = BooksOnDeviceAccessor.GetPages(book.ID);
            if (pageList != null)
            {
                foreach (Page page in pageList)
                {
                    BooksOnDeviceAccessor.RemoveBookmark(book.ID, page.ID);
                    BooksOnDeviceAccessor.RemoveAllNotesForThisPage(book.ID, page.ID);
                    BooksOnDeviceAccessor.RemoveAnnotation(book.ID, page.ID);

                    DownloadedFilesCache.RemoveFile(page.URL);
                }
            }
            BooksOnServerAccessor.RemovePages(book.ID);
            BooksOnDeviceAccessor.RemovePages(book.ID);

            // Remove chapter images from the file system
            List<Chapter> chapterList = BooksOnDeviceAccessor.GetChapters(book.ID);
            if (chapterList != null)
            {
                foreach (Chapter chapter in chapterList)
                {
                    DownloadedFilesCache.RemoveFile(chapter.SmallImageURL);
                    DownloadedFilesCache.RemoveFile(chapter.LargeImageURL);
                }
            }
            BooksOnServerAccessor.RemoveChapters(book.ID);
            BooksOnDeviceAccessor.RemoveChapters(book.ID);

            // Remove book images from the file system
            DownloadedFilesCache.RemoveFile(book.SmallImageURL);
            DownloadedFilesCache.RemoveFile(book.LargeImageURL);

            BooksOnDeviceAccessor.RemoveBook(book.ID);
        }

		public static void RemoveBookInCache(Book book)
		{
			// Remove page images from the file system
			List<Page> pageList = BooksOnDeviceAccessor.GetPages(book.ID);
			if (pageList != null)
			{
				foreach (Page page in pageList)
				{
					DownloadedFilesCache.RemoveFile(page.URL);
				}
			}

			// Remove chapter images from the file system
			List<Chapter> chapterList = BooksOnDeviceAccessor.GetChapters(book.ID);
			if (chapterList != null)
			{
				foreach (Chapter chapter in chapterList)
				{
					DownloadedFilesCache.RemoveFile(chapter.SmallImageURL);
					DownloadedFilesCache.RemoveFile(chapter.LargeImageURL);
				}
			}

			// Remove book images from the file system
			DownloadedFilesCache.RemoveFile(book.SmallImageURL);
			DownloadedFilesCache.RemoveFile(book.LargeImageURL);
		}
    }
}

