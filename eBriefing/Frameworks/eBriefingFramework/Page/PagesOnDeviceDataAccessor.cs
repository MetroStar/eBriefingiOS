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
    public static class PagesOnDeviceDataAccessor
    {
        private static PersistantStorageDatabase<Page> _database = new PersistantStorageDatabase<Page>("pages");

        public static void AddPages(String bookID, List<Page> pageList)
        {
            if (pageList != null)
            {
                _database.StartBatchUpdate(bookID);

                _database.ClearTable(bookID);

                foreach (var page in pageList)
                {
                    _database.SetRecordBatch(bookID, page.ID, page);
                }
                _database.EndBatchUpdate(bookID);
            }
        }

        public static void RemovePages(String bookID)
        {
            _database.DeleteTable(bookID);
        }

        public static void UpdatePages(String bookID, List<Page> pageList)
        {
            AddPages(bookID, pageList);
        }

        public static List<Page> GetPages(String bookID)
        {
            return _database.GetRecordsInTable(bookID);
        }

        public static Page GetPage(String bookID, nint pageNumber)
        {
            int pageIndex = (int)pageNumber - 1;
            List<Page> pagesInBook = _database.GetRecordsInTable(bookID);
            if ((pagesInBook != null) && (pagesInBook.Count > pageIndex))
            {
                return pagesInBook[pageIndex];
            }
			
            return null;
        }

        public static Page GetPage(String bookID, String pageID)
        {
            return _database.GetRecord(bookID, pageID);
        }

        public static List<Page> GetPagesInChapter(String bookID, String chapterID)
        {
            List<Page> pagesInBook = _database.GetRecordsInTable(bookID);
            if (pagesInBook != null)
            {
                List<Page> pagesInChapter = null;
                foreach (var page in pagesInBook)
                {
                    if (page.ChapterID == chapterID)
                    {
                        if (pagesInChapter == null)
                        {
                            pagesInChapter = new List<Page>();
                        }

                        pagesInChapter.Add(page);
                    }
                }

                return pagesInChapter;
            }

            return null;
        }
    }
}

