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
using System.Linq;

namespace eBriefingMobile
{
    public class BooksOnDeviceAccessor
    {
        #region Books

        public static void AddBook(Book book)
        {
            BooksDataAccessor.AddBook(book);
        }

        public static void RemoveBook(String bookID)
        {
            BooksDataAccessor.RemoveBook(bookID);
        }

        public static void UpdateBook(Book book)
        {
            BooksDataAccessor.UpdateBook(book);
        }

        public static void UpdateUserModifiedDate(Book book)
        {
            BooksDataAccessor.UpdateUserModifiedDate(book);
        }

        public static List<Book> GetAllBooks()
        {
            return BooksDataAccessor.GetAllBooks();
        }

        public static List<Book> GetBooks()
        {
            return BooksDataAccessor.GetBooks();
        }

        public static List<Book> GetRemovedBooks()
        {
            return BooksDataAccessor.GetRemovedBooks();
        }

        public static Book GetBook(String bookID)
        {
            return BooksDataAccessor.GetBook(bookID);
        }

        public static List<Book> GetFavoriteBooks()
        {
            return BooksDataAccessor.GetFavoriteBooks();
        }

        public static void UpdateFavorite(Book book, bool value)
        {
            book.IsFavorite = value;
            book.UserModifiedDate = DateTime.UtcNow;
            BooksDataAccessor.UpdateFavorite(book.ID, value);
        }

        public static bool IsFavorite(String bookID)
        {
            return BooksDataAccessor.IsFavorite(bookID);
        }

        public static void MarkAsRemovedBook(Book book)
        {
            book.Removed = true;
            book.UserModifiedDate = DateTime.UtcNow;
            BooksDataAccessor.UpdateBook(book);
        }

        #endregion

        #region Chapters

        public static void AddChapters(String bookID, List<Chapter> chapterList)
        {
            ChaptersOnDeviceDataAccessor.AddChapters(bookID, chapterList);
        }

        public static void RemoveChapters(String bookID)
        {
            ChaptersOnDeviceDataAccessor.RemoveChapters(bookID);
        }

        public static void UpdateChapters(String bookID, List<Chapter> chapterList)
        {
            ChaptersOnDeviceDataAccessor.UpdateChapters(bookID, chapterList);
        }

        public static List<Chapter> GetChapters(String bookID)
        {
            return ChaptersOnDeviceDataAccessor.GetChapters(bookID);
        }

        public static Chapter GetChapter(String bookID, String chapterID)
        {
            return ChaptersOnDeviceDataAccessor.GetChapter(bookID, chapterID);
        }

        #endregion

        #region Pages

        public static void AddPages(String bookID, List<Page> pageList)
        {
            PagesOnDeviceDataAccessor.AddPages(bookID, pageList);
        }

        public static void RemovePages(String bookID)
        {
            PagesOnDeviceDataAccessor.RemovePages(bookID);
        }

        public static void UpdatePages(String bookID, List<Page> pageList)
        {
            PagesOnDeviceDataAccessor.UpdatePages(bookID, pageList);
        }

        public static List<Page> GetPages(String bookID)
        {
            return PagesOnDeviceDataAccessor.GetPages(bookID);
        }

        public static List<Page> GetPages(String bookID, String chapterID)
        {
            return PagesOnDeviceDataAccessor.GetPagesInChapter(bookID, chapterID);
        }

        public static Page GetPage(String bookID, nint pageNumber)
        {
            return PagesOnDeviceDataAccessor.GetPage(bookID, pageNumber);
        }

        public static Page GetPage(String bookID, String pageID)
        {
            return PagesOnDeviceDataAccessor.GetPage(bookID, pageID);
        }

        public static String GetChapterID(String bookID, nint pageNumber)
        {
            Page page = PagesOnDeviceDataAccessor.GetPage(bookID, pageNumber);
            if (page != null)
            {
                return page.ChapterID;
            }
            else
            {
                return String.Empty;
            }
        }

        public static void MapPagesToChapter(List<Chapter> chatperList, List<Page> pageList)
        {
            if (chatperList != null && pageList != null)
            {
                int nextChIndex = 0;
                Chapter currentChapter = chatperList[nextChIndex++];
                Chapter nextChapter = null;
                if (nextChIndex < chatperList.Count)
                {
                    nextChapter = chatperList[nextChIndex++];
                }

                foreach (Page page in pageList)
                {
                    if ((nextChapter != null) && (page.ID == nextChapter.FirstPageID))
                    {
                        currentChapter = nextChapter;
                        if (nextChIndex < chatperList.Count)
                        {
                            nextChapter = chatperList[nextChIndex++];
                        }
                        else
                        {
                            nextChapter = null;
                        }

                        page.ChapterID = currentChapter.ID;
                    }
                    else
                    {
                        page.ChapterID = currentChapter.ID;
                    }
                }
            }
        }

        #endregion

        #region Bookmarks

        public static void AddBookmark(Bookmark bookmark)
        {
            BookmarksDataAccessor.AddBookmark(bookmark);
        }

        public static void RemoveBookmark(String bookID, String pageID)
        {
            BookmarksDataAccessor.RemoveBookmark(bookID, pageID);
        }

        public static void UpdateBookmark(Bookmark bookmark)
        {
            BookmarksDataAccessor.UpdateBookmark(bookmark);
        }

        public static void RemoveOrphanBookmarks(String bookID, List<Page> pageList)
        {
            BookmarksDataAccessor.RemoveOrphanBookmarks(bookID, pageList);
        }

        public static List<Bookmark> GetAllBookmarks(String bookID)
        {
            return BookmarksDataAccessor.GetAllBookmarks(bookID);
        }

        public static List<Bookmark> GetBookmarks(String bookID)
        {
            return BookmarksDataAccessor.GetBookmarks(bookID);
        }

        public static List<Bookmark> GetRemovedBookmarks(String bookID)
        {
            return BookmarksDataAccessor.GetRemovedBookmarks(bookID);
        }

        public static Bookmark GetBookmark(String bookID, String pageID)
        {
            return BookmarksDataAccessor.GetBookmark(bookID, pageID);
        }

        public static String GetNumBookmarksInBook(String bookID)
        {
            return BookmarksDataAccessor.GetNumBookmarksInBook(bookID).ToString();
        }

        public static String GetNumBookmarksInChapter(String bookID, String chapterID)
        {
            return BookmarksDataAccessor.GetNumBookmarksInChapter(bookID, chapterID).ToString();
        }

        public static void MarkAsRemovedBookmark(String bookID, String pageID)
        {
            Bookmark bookmark = GetBookmark(bookID, pageID);
            if (bookmark != null)
            {
                bookmark.Removed = true;
                bookmark.ModifiedUtc = DateTime.UtcNow;
                BookmarksDataAccessor.UpdateBookmark(bookmark);
            }
        }

        #endregion

        #region Notes

        public static void AddNote(Note note)
        {
            if (String.IsNullOrEmpty(URL.MultipleNoteURL))
            {
                NotesDataAccessor.AddNote(note, note.PageID);
            }
            else
            {
                NotesDataAccessor.AddNote(note, note.NoteID);
            }
        }

        public static void RemoveNote(String bookID, String ID)
        {
            NotesDataAccessor.RemoveNote(bookID, ID);
        }

        public static void RemoveAllNotesForThisPage(String bookID, String pageID)
        {
            NotesDataAccessor.RemoveAllNotesForThisPage(bookID, pageID);
        }

        public static void RemoveOrphanNotes(String bookID, List<Page> pageList)
        {
            NotesDataAccessor.RemoveOrphanNotes(bookID, pageList);
        }

        public static void UpdateNote(Note note)
        {
            if (String.IsNullOrEmpty(URL.MultipleNoteURL))
            {
                NotesDataAccessor.UpdateNote(note, note.PageID);
            }
            else
            {
                NotesDataAccessor.UpdateNote(note, note.NoteID);
            }
        }

        public static List<Note> GetAllNotes(String bookID)
        {
            return NotesDataAccessor.GetAllNotes(bookID);
        }

        public static List<Note> GetNotes(String bookID)
        {
            return NotesDataAccessor.GetNotes(bookID);
        }

        public static List<Note> GetNotes(String bookID, String pageID)
        {
            return NotesDataAccessor.GetNotes(bookID, pageID);
        }

        public static List<Note> GetNotesInChapter(String bookID, String chapterID)
        {
            return NotesDataAccessor.GetNotesInChapter(bookID, chapterID);
        }

        public static nint GetNumPagesThatHaveNotesInChapter(String bookID, String chapterID)
        {
            return NotesDataAccessor.GetNumPagesThatHaveNotesInChapter(bookID, chapterID);
        }

        public static Note GetNote(String bookID, String ID)
        {
            return NotesDataAccessor.GetNote(bookID, ID);
        }

        public static String GetNumNotesInBook(String bookID)
        {
            return NotesDataAccessor.GetNumNotesInBook(bookID).ToString();
        }

        public static String GetNumNotesInChapter(String bookID, String chapterID)
        {
            return NotesDataAccessor.GetNumNotesInChapter(bookID, chapterID).ToString();
        }

        public static void MarkAsRemovedNote(String bookID, String ID)
        {
            Note note = GetNote(bookID, ID);
            if (note != null)
            {
                note.Removed = true;
                note.ModifiedUtc = DateTime.UtcNow;
                NotesDataAccessor.UpdateNote(note, ID);
            }
        }

        #endregion

        #region Annotations

        public static void AddAnnotation(Annotation annotation)
        {
            AnnotationsDataAccessor.AddAnnotation(annotation);
        }

        public static void RemoveAnnotation(String bookID, String pageID)
        {
            AnnotationsDataAccessor.RemoveAnnotation(bookID, pageID);
        }

        public static void UpdateAnnotations(String bookID, List<Page> pageList)
        {
            AnnotationsDataAccessor.UpdateAnnotations(bookID, pageList);
        }

        public static void UpdateAnnotation(Annotation annotation)
        {
            AnnotationsDataAccessor.UpdateAnnotation(annotation);
        }

        public static List<Annotation> GetAllAnnotations(String bookID)
        {
            return AnnotationsDataAccessor.GetAllAnnotations(bookID);
        }

        public static List<Annotation> GetAnnotations(String bookID)
        {
            return AnnotationsDataAccessor.GetAnnotations(bookID);
        }

        public static List<Annotation> GetRemovedAnnotations(String bookID)
        {
            return AnnotationsDataAccessor.GetRemovedAnnotations(bookID);
        }

        public static Annotation GetAnnotation(String bookID, String pageID)
        {
            return AnnotationsDataAccessor.GetAnnotation(bookID, pageID);
        }

        public static String GetNumAnnotationsInBook(String bookID)
        {
            return AnnotationsDataAccessor.GetNumAnnotationsInBook(bookID).ToString();
        }

        public static String GetNumAnnotationsInChapter(String bookID, String chapterID)
        {
            return AnnotationsDataAccessor.GetNumAnnotationsInChapter(bookID, chapterID).ToString();
        }

        public static void MarkAsRemovedAnnotation(String bookID, String pageID)
        {
            Annotation annotation = GetAnnotation(bookID, pageID);
            if (annotation != null)
            {
                annotation.Removed = true;
                annotation.ModifiedUtc = DateTime.UtcNow;
                AnnotationsDataAccessor.UpdateAnnotation(annotation);
            }
        }

        #endregion
    }
}

