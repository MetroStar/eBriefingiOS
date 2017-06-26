using System;
using System.Collections.Generic;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MssFramework;
using Metrostar.Mobile.Framework;

namespace eBriefingMobile
{
    public static class CloudSync
    {
        public enum SyncType
        {
            PUSH,
            PULL,
            PUSH_AND_PULL,
        }

        private static bool cancelled = false;
        private static bool sentBooks = false;
        private static bool sentBookmarks = false;
        private static bool sentNotes = false;
        private static bool sentAnnotations = false;
        private static bool receiveBookmarks = false;
        private static bool receiveNotes = false;
        private static bool receiveAnnotations = false;
        private static SyncType currentSyncType;

        public delegate void SendDoneDelegate();

        public delegate void ReceiveDoneDelegate();

        public delegate void SyncDoneDelegate();

        public static event ReceiveDoneDelegate ReceiveDoneEvent;
        public static event SyncDoneDelegate SyncDoneEvent;

        public static bool PullRequired
        {
            get
            {
                if (Settings.SyncOn)
                {
                    // Do not start the timer if connected to Demo server
                    if (!String.Equals(Settings.ServerURL, StringRef.DemoURL))
                    {
                        TimeSpan diff = TimeSpan.MinValue;
                        if (Settings.LastPullTime == null)
                        {
                            return TimeSettings.SyncPullRequired(TimeSpan.FromDays(1));
                        }
                        else
                        {
                            DateTime lastSync = DateTime.Parse(Settings.LastPullTime);
                            diff = DateTime.UtcNow.Subtract(lastSync);
                            return TimeSettings.SyncPullRequired(diff);
                        }
                    }
                }

                return false;
            }
        }

        public static void PushAndPull()
        {
            currentSyncType = SyncType.PUSH_AND_PULL;

            if (Settings.SyncOn)
            {
                InitializeAll();

                PushStart();
            }
        }

        public static void SyncOff()
        {
            InitializeAll();
        }

        public static void CancelSync()
        {
            cancelled = true;
        }

        private static void InitializeAll()
        {
            cancelled = false;

            SetSent(false);

            SetReceive(false);
        }

        private static void SetSent(bool value)
        {
            sentBooks = sentBookmarks = sentNotes = sentAnnotations = value;
        }

        private static void SetReceive(bool value)
        {
            receiveBookmarks = receiveNotes = receiveAnnotations = value;
        }

        #region Send

        public static void Push()
        {
            currentSyncType = SyncType.PUSH;

            InitializeAll();

            PushStart();
        }

        private static void PushStart()
        {
            if (Reachability.IsDefaultNetworkAvailable())
            {
                // SEND
                List<Book> bookList = BooksOnDeviceAccessor.GetAllBooks();
                if (bookList != null && bookList.Count > 0)
                {
                    SendMyBooks(bookList);

                    SendMyBookmarks(bookList);

                    SendMyNotes(bookList);

                    SendMyAnnotations(bookList);
                }
                else
                {
                    sentBooks = sentBookmarks = sentNotes = sentAnnotations = true;

                    CheckSentDone();
                }
            }
        }

        private static void CheckSentDone()
        {
            if (sentBooks && sentBookmarks && sentNotes && sentAnnotations)
            {
                SetSent(false);

                if (currentSyncType == SyncType.PUSH_AND_PULL)
                {
                    PullStart();
                }
            }
        }

        private static void SendMyBooks(List<Book> bookList)
        {
            if (cancelled)
            {
                SetSent(true);

                CheckSentDone();
            }
            else
            {
                if (Reachability.IsDefaultNetworkAvailable())
                {
                    SaveMyStuff.SetMyBooksEvent += HandleSetMyBooksEvent;
                    SaveMyStuff.SetMyBooks(bookList);

                    // Remove books that are marked as removed
                    List<Book> removeList = BooksOnDeviceAccessor.GetRemovedBooks();
                    if (removeList != null && removeList.Count > 0)
                    {
                        BookRemover.RemoveBooks(removeList);
                    }
                }
            }
        }

        private static void SendMyBookmarks(List<Book> bookList)
        {
            if (cancelled)
            {
                SetSent(true);

                CheckSentDone();
            }
            else
            {
                if (Reachability.IsDefaultNetworkAvailable())
                {
                    SaveMyStuff.SetMyBookmarksEvent += HandleSetMyBookmarksEvent;

                    foreach (Book book in bookList)
                    {
                        List<Bookmark> bookmarkList = BooksOnDeviceAccessor.GetAllBookmarks(book.ID);
                        if (bookmarkList != null && bookmarkList.Count > 0)
                        {
                            SaveMyStuff.SetMyBookmarks(bookmarkList);
                        }
                        else
                        {
                            sentBookmarks = true;
                        }
                    }

                    // Remove bookmarks that are marked as removed
                    foreach (Book book in bookList)
                    {
                        List<Bookmark> bookmarkList = BooksOnDeviceAccessor.GetRemovedBookmarks(book.ID);
                        if (bookmarkList != null && bookmarkList.Count > 0)
                        {
                            foreach (Bookmark bookmark in bookmarkList)
                            {
                                BooksOnDeviceAccessor.RemoveBookmark(bookmark.BookID, bookmark.PageID);
                            }
                        }
                    }
                }
            }
        }

        private static void SendMyNotes(List<Book> bookList)
        {
            if (cancelled)
            {
                SetSent(true);

                CheckSentDone();
            }
            else
            {
                if (Reachability.IsDefaultNetworkAvailable())
                {
                    SaveMyStuff.SetMyNoteEvent += HandleSetMyNoteEvent;

                    foreach (Book book in bookList)
                    {
                        List<Note> noteList = BooksOnDeviceAccessor.GetAllNotes(book.ID);
                        if (noteList != null && noteList.Count > 0)
                        {
                            for (Int32 i = 0; i < noteList.Count; i++)
                            {
                                bool lastItem = false;
                                if (i == noteList.Count - 1)
                                {
                                    lastItem = true;
                                }

                                if (noteList[i].Removed)
                                {
                                    SaveMyStuff.RemoveMyNote(noteList[i], lastItem);
                                }
                                else
                                {
                                    SaveMyStuff.SetMyNote(noteList[i], lastItem);
                                }
                            }
                        }
                        else
                        {
                            sentNotes = true;
                        }
                    }

                    // Remove notes that are marked as removed
                    foreach (Book book in bookList)
                    {
                        List<Note> noteList = BooksOnDeviceAccessor.GetRemovedNotes(book.ID);
                        if (noteList != null && noteList.Count > 0)
                        {
                            foreach (Note note in noteList)
                            {
                                BooksOnDeviceAccessor.RemoveNote(note.BookID, note.PageID);
                            }
                        }
                    }
                }
            }
        }

        private static void SendMyAnnotations(List<Book> bookList)
        {
            if (cancelled)
            {
                SetSent(true);

                CheckSentDone();
            }
            else
            {
                if (Reachability.IsDefaultNetworkAvailable())
                {
                    SaveMyStuff.SetMyAnnotationEvent += HandleSetMyAnnotationEvent;

                    foreach (Book book in bookList)
                    {
                        List<Annotation> annotationList = BooksOnDeviceAccessor.GetAllAnnotations(book.ID);
                        if (annotationList != null && annotationList.Count > 0)
                        {
                            for (Int32 i = 0; i < annotationList.Count; i++)
                            {
                                bool lastItem = false;
                                if (i == annotationList.Count - 1)
                                {
                                    lastItem = true;
                                }

                                if (annotationList[i].Removed)
                                {
                                    SaveMyStuff.RemoveMyAnnotation(annotationList[i], lastItem);
                                }
                                else
                                {
                                    SaveMyStuff.SetMyAnnotation(annotationList[i], lastItem);
                                }
                            }
                        }
                        else
                        {
                            sentAnnotations = true;
                        }
                    }

                    // Remove annotation that are marked as removed
                    foreach (Book book in bookList)
                    {
                        List<Annotation> annotationList = BooksOnDeviceAccessor.GetRemovedAnnotations(book.ID);
                        if (annotationList != null && annotationList.Count > 0)
                        {
                            foreach (Annotation annotation in annotationList)
                            {
                                BooksOnDeviceAccessor.RemoveAnnotation(annotation.BookID, annotation.PageID);
                            }
                        }
                    }
                }
            }
        }

        static void HandleSetMyBooksEvent()
        {
            SaveMyStuff.SetMyBooksEvent -= HandleSetMyBooksEvent;
            sentBooks = true;

            CheckSentDone();
        }

        static void HandleSetMyBookmarksEvent()
        {
            SaveMyStuff.SetMyBookmarksEvent -= HandleSetMyBookmarksEvent;
            sentBookmarks = true;

            CheckSentDone();
        }

        static void HandleSetMyNoteEvent(bool lastItem)
        {
            if (lastItem)
            {
                SaveMyStuff.SetMyNoteEvent -= HandleSetMyNoteEvent;
                sentNotes = true;

                CheckSentDone();
            }
        }

        static void HandleSetMyAnnotationEvent(bool lastItem)
        {
            if (lastItem)
            {
                SaveMyStuff.SetMyAnnotationEvent -= HandleSetMyAnnotationEvent;
                sentAnnotations = true;

                CheckSentDone();
            }
        }

        #endregion

        #region Receive

        public static void GetMyStuff(String bookID)
        {
            if (Settings.SyncOn)
            {
                if (Reachability.IsDefaultNetworkAvailable())
                {
                    SaveMyStuff.GetMyBookmarksEvent += HandleGetMyBookmarksEvent;
                    SaveMyStuff.GetMyNotesEvent += HandleGetMyNotesEvent;
                    SaveMyStuff.GetMyAnnotationsEvent += HandleGetMyAnnotationsEvent;

                    SaveMyStuff.GetMyBookmarks(bookID, true);
                    SaveMyStuff.GetMyNotes(bookID, true);
                    SaveMyStuff.GetMyAnnotations(bookID, true);
                }
            }
        }

        public static void Pull()
        {
            currentSyncType = SyncType.PULL;

            InitializeAll();

            PullStart();
        }

        private static void PullStart()
        {
            if (cancelled)
            {
                SetReceive(true);

                CheckReceiveDone();
            }
            else
            {
                if (Reachability.IsDefaultNetworkAvailable())
                {
                    SaveMyStuff.GetMyBooksEvent += HandleGetMyBooksEvent;
                    SaveMyStuff.GetMyBooks();
                }
                else
                {
                    if (currentSyncType == SyncType.PUSH_AND_PULL)
                    {
                        if (SyncDoneEvent != null)
                        {
                            SyncDoneEvent();
                        }
                    }
                }
            }
        }

        private static void CheckReceiveDone()
        {
            if (receiveBookmarks && receiveNotes && receiveAnnotations)
            {
                SetReceive(false);

                Settings.WriteLastPullTime(DateTime.UtcNow.ToString());

                if (currentSyncType == SyncType.PUSH_AND_PULL)
                {
                    if (SyncDoneEvent != null)
                    {
                        SyncDoneEvent();
                    }
                }
                else
                {
                    if (ReceiveDoneEvent != null)
                    {
                        ReceiveDoneEvent();
                    }
                }
            }
        }

        static void HandleGetMyBooksEvent(List<Book> sBooks)
        {
            try
            {
                SaveMyStuff.GetMyBooksEvent -= HandleGetMyBooksEvent;

                if (sBooks == null)
                {
                    receiveBookmarks = receiveNotes = receiveAnnotations = true;

                    CheckReceiveDone();
                }
                else
                {
                    List<Book> dBooks = BooksOnDeviceAccessor.GetBooks();
                    if (dBooks != null && dBooks.Count > 0)
                    {
                        foreach (Book dBook in dBooks)
                        {
                            if (sBooks != null && sBooks.Count > 0)
                            {
                                foreach (Book sBook in sBooks)
                                {
                                    if (dBook.ID == sBook.ID)
                                    {
                                        if (dBook.UserModifiedDate < sBook.UserModifiedDate)
                                        {
                                            if (sBook.Removed)
                                            {
                                                // Remove book if book on the cloud has 'Removed' checked
                                                BookRemover.RemoveBook(dBook);
                                            }
                                            else
                                            {
                                                // Update book if book on the cloud has the latest ModifiedUtc
                                                dBook.Version = sBook.Version;
                                                dBook.IsFavorite = sBook.IsFavorite;
                                                dBook.UserModifiedDate = sBook.UserModifiedDate;
                                                dBook.UserAddedDate = DateTime.UtcNow;
                                                dBook.New = true;

                                                BooksOnDeviceAccessor.UpdateBook(dBook);
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // Add book if the book is not on the device
                    if (sBooks != null && sBooks.Count > 0)
                    {
                        foreach (Book sBook in sBooks)
                        {
                            if (!sBook.Removed)
                            {
                                if (BooksOnDeviceAccessor.GetBook(sBook.ID) == null)
                                {
                                    BooksOnDeviceAccessor.AddBook(sBook);
                                }
                            }
                        }
                    }

                    // Add book details for new books
                    BookDownloader bd = new BookDownloader();
                    bd.DownloadedEvent += (List<Book> bookList) =>
                    {
                        if (bookList != null)
                        {
                            dBooks = BooksOnDeviceAccessor.GetBooks();
                            if (dBooks != null)
                            {
                                foreach (Book dBook in dBooks)
                                {
                                    if (dBook.Status == Book.BookStatus.NONE)
                                    {
                                        foreach (Book nBook in bookList)
                                        {
                                            if (dBook.ID == nBook.ID)
                                            {
                                                dBook.Title = nBook.Title;
                                                dBook.Description = nBook.Description;
                                                dBook.ChapterCount = nBook.ChapterCount;
                                                dBook.PageCount = nBook.PageCount;
                                                dBook.SmallImageURL = nBook.SmallImageURL;
                                                dBook.LargeImageURL = nBook.LargeImageURL;
                                                dBook.ImageVersion = nBook.ImageVersion;
                                                dBook.ServerAddedDate = nBook.ServerAddedDate;
                                                dBook.ServerModifiedDate = nBook.ServerModifiedDate;
                                                dBook.UserAddedDate = nBook.UserAddedDate;
                                                dBook.UserModifiedDate = nBook.UserModifiedDate;
                                                dBook.Status = Book.BookStatus.PENDING2DOWNLOAD;
                                                dBook.Removed = false;
                                                dBook.New = true;
                                                dBook.Viewed = false;

                                                BooksOnDeviceAccessor.UpdateBook(dBook);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (currentSyncType == SyncType.PULL)
                        {
                            receiveBookmarks = receiveNotes = receiveAnnotations = true;

                            CheckReceiveDone();
                        }
                        else
                        {
                            // Receive Bookmarks, Notes, and Annotations
                            if (sBooks != null)
                            {
                                SaveMyStuff.GetMyBookmarksEvent += HandleGetMyBookmarksEvent;
                                SaveMyStuff.GetMyNotesEvent += HandleGetMyNotesEvent;
                                SaveMyStuff.GetMyAnnotationsEvent += HandleGetMyAnnotationsEvent;

                                for (Int32 i = 0; i < sBooks.Count; i++)
                                {
                                    bool lastItem = false;
                                    if (i == sBooks.Count - 1)
                                    {
                                        lastItem = true;
                                    }

                                    SaveMyStuff.GetMyBookmarks(sBooks[i].ID, lastItem);
                                    SaveMyStuff.GetMyNotes(sBooks[i].ID, lastItem);
                                    SaveMyStuff.GetMyAnnotations(sBooks[i].ID, lastItem);
                                }
                            }
                        }
                    };
                    bd.StartDownload();
                }
            }
            catch (Exception ex)
            {
                SetReceive(true);

                CheckReceiveDone();

                Logger.WriteLineDebugging("CloudSync - HandleGetMyBooksEvent: {0}", ex.ToString());
            }
        }

        static void HandleGetMyBookmarksEvent(String bookID, List<Bookmark> sBookmarks, bool lastItem)
        {
            try
            {
                List<Bookmark> dBookmarks = BooksOnDeviceAccessor.GetBookmarks(bookID);
                if (dBookmarks != null && dBookmarks.Count > 0)
                {
                    foreach (Bookmark dBookmark in dBookmarks)
                    {
                        if (sBookmarks != null && sBookmarks.Count > 0)
                        {
                            foreach (Bookmark sBookmark in sBookmarks)
                            {
                                if (dBookmark.PageID == sBookmark.PageID)
                                {
                                    if (dBookmark.ModifiedUtc < sBookmark.ModifiedUtc)
                                    {
                                        if (sBookmark.Removed)
                                        {
                                            // Remove bookmark if bookmark on the cloud has 'Removed' checked
                                            BooksOnDeviceAccessor.RemoveBookmark(dBookmark.BookID, dBookmark.PageID);
                                        }
                                        else
                                        {
                                            // Update bookmark if bookmark on the cloud has the latest ModifiedUtc
                                            dBookmark.BookVersion = sBookmark.BookVersion;
                                            dBookmark.Title = sBookmark.Title;
                                            dBookmark.ModifiedUtc = sBookmark.ModifiedUtc;

                                            BooksOnDeviceAccessor.UpdateBookmark(dBookmark);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                // Add bookmark if the bookmark is not on the device
                if (sBookmarks != null && sBookmarks.Count > 0)
                {
                    foreach (Bookmark sBookmark in sBookmarks)
                    {
                        if (!sBookmark.Removed)
                        {
                            if (BooksOnDeviceAccessor.GetBookmark(sBookmark.BookID, sBookmark.PageID) == null)
                            {
                                BooksOnDeviceAccessor.AddBookmark(sBookmark);
                            }
                        }
                    }
                }

                // Check if syncing is done
                if (cancelled)
                {
                    SetReceive(true);

                    CheckReceiveDone();
                }
                else
                {
                    if (lastItem)
                    {
                        SaveMyStuff.GetMyBookmarksEvent -= HandleGetMyBookmarksEvent;
                        receiveBookmarks = true;

                        CheckReceiveDone();
                    }
                }
            }
            catch (Exception ex)
            {
                SetReceive(true);

                CheckReceiveDone();

                Logger.WriteLineDebugging("CloudSync - HandleGetMyBookmarksEvent: {0}", ex.ToString());
            }
        }

        static void HandleGetMyNotesEvent(String bookID, List<Note> sNotes, bool lastItem)
        {
            try
            {
                List<Note> dNotes = BooksOnDeviceAccessor.GetNotes(bookID);
                if (dNotes != null && dNotes.Count > 0)
                {
                    foreach (Note dNote in dNotes)
                    {
                        if (sNotes != null && sNotes.Count > 0)
                        {
                            foreach (Note sNote in sNotes)
                            {
                                if (dNote.PageID == sNote.PageID)
                                {
                                    if (dNote.ModifiedUtc < sNote.ModifiedUtc)
                                    {
                                        if (sNote.Removed)
                                        {
                                            // Remove note if note on the cloud has 'Removed' checked
                                            BooksOnDeviceAccessor.RemoveNote(dNote.BookID, dNote.PageID);
                                        }
                                        else
                                        {
                                            // Update note if note on the cloud has the latest ModifiedUtc
                                            dNote.BookVersion = sNote.BookVersion;
                                            dNote.Text = sNote.Text;
                                            dNote.ModifiedUtc = sNote.ModifiedUtc;

                                            BooksOnDeviceAccessor.UpdateNote(dNote);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                // Add note if the note is not on the device
                if (sNotes != null && sNotes.Count > 0)
                {
                    foreach (Note sNote in sNotes)
                    {
                        if (!sNote.Removed)
                        {
                            if (BooksOnDeviceAccessor.GetNote(sNote.BookID, sNote.PageID) == null)
                            {
                                BooksOnDeviceAccessor.AddNote(sNote);
                            }
                        }
                    }
                }

                // Check if syncing is done
                if (cancelled)
                {
                    SetReceive(true);

                    CheckReceiveDone();
                }
                else
                {
                    if (lastItem)
                    {
                        SaveMyStuff.GetMyNotesEvent -= HandleGetMyNotesEvent;
                        receiveNotes = true;

                        CheckReceiveDone();
                    }
                }
            }
            catch (Exception ex)
            {
                SetReceive(true);

                CheckReceiveDone();

                Logger.WriteLineDebugging("CloudSync - HandleGetMyNotesEvent: {0}", ex.ToString());
            }
        }

        static void HandleGetMyAnnotationsEvent(String bookID, List<Annotation> sAnnotations, bool lastItem)
        {
            try
            {
                List<Annotation> dAnnotations = BooksOnDeviceAccessor.GetAnnotations(bookID);
                if (dAnnotations != null && dAnnotations.Count > 0)
                {
                    foreach (Annotation dAnnotation in dAnnotations)
                    {
                        if (sAnnotations != null && sAnnotations.Count > 0)
                        {
                            foreach (Annotation sAnnotation in sAnnotations)
                            {
                                if (dAnnotation.PageID == sAnnotation.PageID)
                                {
                                    if (dAnnotation.ModifiedUtc < sAnnotation.ModifiedUtc)
                                    {
                                        if (sAnnotation.Removed)
                                        {
                                            // Remove annotation if annotation on the cloud has 'Removed' checked
                                            BooksOnDeviceAccessor.RemoveAnnotation(dAnnotation.BookID, dAnnotation.PageID);
                                        }
                                        else
                                        {
                                            // Update annotation if annotation on the cloud has the latest ModifiedUtc
                                            dAnnotation.BookVersion = sAnnotation.BookVersion;
                                            dAnnotation.Items = sAnnotation.Items;
                                            dAnnotation.ModifiedUtc = sAnnotation.ModifiedUtc;

                                            BooksOnDeviceAccessor.UpdateAnnotation(dAnnotation);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                // Add annotation if the annotation is not on the device
                if (sAnnotations != null && sAnnotations.Count > 0)
                {
                    foreach (Annotation sAnnotation in sAnnotations)
                    {
                        if (!sAnnotation.Removed)
                        {
                            if (BooksOnDeviceAccessor.GetAnnotation(sAnnotation.BookID, sAnnotation.PageID) == null)
                            {
                                BooksOnDeviceAccessor.AddAnnotation(sAnnotation);
                            }
                        }
                    }
                }

                // Check if syncing is done
                if (cancelled)
                {
                    SetReceive(true);

                    CheckReceiveDone();
                }
                else
                {
                    if (lastItem)
                    {
                        SaveMyStuff.GetMyAnnotationsEvent -= HandleGetMyAnnotationsEvent;
                        receiveAnnotations = true;

                        CheckReceiveDone();
                    }
                }
            }
            catch (Exception ex)
            {
                SetReceive(true);

                CheckReceiveDone();

                Logger.WriteLineDebugging("CloudSync - HandleGetMyAnnotationsEvent: {0}", ex.ToString());
            }
        }

        #endregion

        #region Delete

        public static void DeleteMyBooks()
        {
            SaveMyStuff.DeleteMyBooks();
        }

        public static void DeleteMyBookmarks()
        {
            SaveMyStuff.DeleteMyBookmarks();
        }

        public static void DeleteMyNotes()
        {
            SaveMyStuff.DeleteMyNotes();
        }

        public static void DeleteMyAnnotations()
        {
            SaveMyStuff.DeleteMyAnnotations();
        }

        public static void DeleteMyStuff()
        {
            SaveMyStuff.DeleteMyStuff();
        }

        #endregion
    }
}