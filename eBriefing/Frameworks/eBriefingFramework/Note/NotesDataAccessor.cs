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
    public static class NotesDataAccessor
    {
        private static PersistantStorageDatabase<Note> _database = new PersistantStorageDatabase<Note>("Notes");

        public static void AddNote(Note note, String ID)
        {
            _database.SetRecord(note.BookID, ID, note);
        }

        public static void RemoveNote(String bookID, String ID)
        {
            _database.DeleteRecord(bookID, ID);
        }

        public static void RemoveAllNotesForThisPage(String bookID, String pageID)
        {
            List<Note> noteList = GetNotes(bookID);
            if (noteList != null)
            {
                noteList = noteList.Where(i => i.PageID == pageID).ToList();
                if (noteList != null && noteList.Count > 0)
                {
                    foreach (var note in noteList)
                    {
                        if (String.IsNullOrEmpty(URL.MultipleNoteURL))
                        {
                            _database.DeleteRecord(bookID, pageID);
                        }
                        else
                        {
                            _database.DeleteRecord(bookID, note.BookID);
                        }
                    }
                }
            }
        }

        public static void UpdateNote(Note note, String ID)
        {
            AddNote(note, ID);
        }

        public static void RemoveOrphanNotes(String bookID, List<Page> newPageList)
        {
            List<Page> oldPageList = PagesOnDeviceDataAccessor.GetPages(bookID);
            if (oldPageList != null)
            {
                foreach (Page oldPage in oldPageList)
                {
                    var item = newPageList.Where(i => i.ID == oldPage.ID).FirstOrDefault();
                    if (item == null)
                    {
                        RemoveAllNotesForThisPage(bookID, oldPage.ID);
                    }
                }
            }
        }

        public static List<Note> GetAllNotes(String bookID)
        {
            List<Note> result = _database.GetRecordsInTable(bookID);
            if (result == null || result.Count == 0)
            {
                return null;
            }

            return result;
        }

        public static List<Note> GetAllNotes(String bookID, String pageID)
        {
            List<Note> noteList = GetAllNotes(bookID);
            if (noteList != null && noteList.Count > 0)
            {
                return noteList.Where(i => i.PageID == pageID).ToList();
            }

            return null;
        }

        public static List<Note> GetNotes(String bookID)
        {
            List<Note> list = _database.GetRecordsInTable(bookID);
            if (list == null || list.Count == 0)
            {
                return null;
            }
            else
            {
                List<Note> result = null;
                foreach (Note note in list)
                {
                    if (!note.Removed)
                    {
                        if (result == null)
                        {
                            result = new List<Note>();
                        }

                        result.Add(note);
                    }
                }

                return result;
            }
        }

        public static List<Note> GetNotesInChapter(String bookID, String chapterID)
        {
            List<Note> list = null;
            List<Page> pageList = PagesOnDeviceDataAccessor.GetPagesInChapter(bookID, chapterID);
            if (pageList != null)
            {
                List<Note> noteList = GetNotes(bookID);
                if (noteList != null && noteList.Count > 0)
                {
                    foreach (var page in pageList)
                    {
                        List<Note> tempList = noteList.Where(i => i.PageID == page.ID).ToList();
                        if (tempList != null && tempList.Count > 0)
                        {
                            if (list == null)
                            {
                                list = new List<Note>();
                            }
                            list.AddRange(tempList);
                        }
                    }
                }
            }

            return list;
        }

        public static nint GetNumPagesThatHaveNotesInChapter(String bookID, String chapterID)
        {
            nint i = 0;

            List<Note> noteList = GetNotesInChapter(bookID, chapterID);
            List<String> pageList = new List<String>();
            if (noteList != null && noteList.Count > 0)
            {
                foreach (Note note in noteList)
                {
                    if (!pageList.Contains(note.PageID))
                    {
                        pageList.Add(note.PageID);
                        i++;
                    }
                }
            }

            return i;
        }

        public static List<Note> GetNotes(String bookID, String pageID)
        {
            List<Note> noteList = GetNotes(bookID);
            if (noteList != null && noteList.Count > 0)
            {
                return noteList.Where(i => i.PageID == pageID).ToList();
            }

            return null;
        }

        public static List<Note> GetRemovedNotes(String bookID)
        {
            List<Note> list = _database.GetRecordsInTable(bookID);
            if (list == null || list.Count == 0)
            {
                return null;
            }
            else
            {
                List<Note> result = null;
                foreach (Note note in list)
                {
                    if (note.Removed)
                    {
                        if (result == null)
                        {
                            result = new List<Note>();
                        }

                        result.Add(note);
                    }
                }

                return result;
            }
        }

        public static Note GetNote(String bookID, String ID)
        {
            Note note = _database.GetRecord(bookID, ID);
            if (note != null && !String.IsNullOrEmpty(note.Text) && !note.Removed)
            {
                return note;
            }

            return null;
        }

        public static int GetNumNotesInBook(String bookID)
        {
            return GetNumNotes(bookID, PagesOnDeviceDataAccessor.GetPages(bookID));
        }

        public static int GetNumNotesInChapter(String bookID, String chapterID)
        {
            return GetNumNotes(bookID, PagesOnDeviceDataAccessor.GetPagesInChapter(bookID, chapterID));
        }

        private static int GetNumNotes(String bookID, List<Page> pageList)
        {
            int count = 0;
            if (pageList != null)
            {
                List<Note> noteList = GetNotes(bookID);
                if (noteList != null && noteList.Count > 0)
                {
                    foreach (var page in pageList)
                    {
                        List<Note> tempList = noteList.Where(i => i.PageID == page.ID).ToList();
                        if (tempList != null && tempList.Count > 0)
                        {
                            count += tempList.Count;
                        }
                    }
                }
            }
            return count;
        }
    }
}

