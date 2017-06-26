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
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using Foundation;
using UIKit;
using ObjCRuntime;
using Metrostar.Mobile.Framework;
using ASIHTTPRequestBinding;
using eBriefing.com.yourSharePointBackEndURL.ebriefingweb;

namespace eBriefingMobile
{
    public static class BookUpdater
    {
        private static ASINetworkQueue networkQueue = new ASINetworkQueue();

        public static UIViewController ParentViewController { get; set; }

        public static List<Book> Books2Update { get; set; }

        public static List<Book> Books2Download { get; set; }

        public static int TotalDownloadCount { get; set; }

        public static bool InProgress { get; set; }

        public static Book CurrentBook { get; set; }

        public delegate void BookUpdaterDelegate0 ();

        public delegate void BookUpdaterDelegate1 (String bookID);

        public delegate void BookUpdaterDelegate2 (int count);

        public static event BookUpdaterDelegate0 NewBookAddedEvent;
        public static event BookUpdaterDelegate1 DownloadStartEvent;
        public static event BookUpdaterDelegate1 DownloadFinishEvent;
        public static event BookUpdaterDelegate2 UpdateNeededEvent;

        public static void RegisterASIDelegate(UIViewController vc)
        {
            ParentViewController = vc;
            networkQueue.Delegate = vc;
        }

        public static void CheckBooks2Download()
        {
            bool added = false;
            List<Book> bookList = BooksOnDeviceAccessor.GetBooks();
            if (bookList != null)
            {
                foreach (Book book in bookList)
                {
                    if (book.Status == Book.BookStatus.DOWNLOADING || book.Status == Book.BookStatus.PENDING2DOWNLOAD)
                    {
                        if (Books2Download == null)
                        {
                            Books2Download = new List<Book>();
                        }

                        // Do not add if Books2Download already has it
                        if (!Books2Download.Contains(book))
                        {
                            added = true;
                            Books2Download.Add(book);
                        }
                    }
                }
            }

            if (added)
            {
                if (NewBookAddedEvent != null)
                {
                    NewBookAddedEvent();
                }
            }
        }

        public static void Enqueue(List<Book> bookList)
        {
            if (Books2Download == null)
            {
                Books2Download = new List<Book>();
            }

            if (bookList != null)
            {
                foreach (Book book in bookList)
                {
                    Books2Download.Add(book);
                }
            }
        }

        public static void Dequeue(String bookID)
        {
            if (Books2Download != null && Books2Download.Count > 0)
            {
                var index = Books2Download.IndexOf(Books2Download.Where(i => i.ID == bookID).FirstOrDefault());
                if (index >= 0)
                {
                    Books2Download.RemoveAt(index);
                }
            }
        }

        public static void CancelDownloadOperations()
        {
            networkQueue.CancelAllOperations();

            RemoveBookFromDevice(CurrentBook);
        }

        public static void RemoveBookFromBooks2Update(String bookID)
        {
            if (Books2Update != null && Books2Update.Count > 0)
            {
                var index = Books2Update.IndexOf(Books2Update.Where(i => i.ID == bookID).FirstOrDefault());
                if (index >= 0)
                {
                    Books2Update.RemoveAt(index);
                }
            }
        }

        public static void RemoveBookFromDevice(Book book)
        {
            if (book != null)
            {
                if (book.Status == Book.BookStatus.DOWNLOADING || book.Status == Book.BookStatus.PENDING2DOWNLOAD)
                {
                    if (book.Status == Book.BookStatus.DOWNLOADING)
                    {
                        // Remove page images from the file system
                        List<Page> pageList = BooksOnServerAccessor.GetPages(book.ID);
                        if (pageList != null)
                        {
                            foreach (Page page in pageList)
                            {
                                DownloadedFilesCache.RemoveFile(page.URL);
                            }
                        }
                        BooksOnServerAccessor.RemovePages(book.ID);

                        // Remove chapter images from the file system
                        List<Chapter> chapterList = BooksOnServerAccessor.GetChapters(book.ID);
                        if (chapterList != null)
                        {
                            foreach (Chapter chapter in chapterList)
                            {
                                DownloadedFilesCache.RemoveFile(chapter.SmallImageURL);
                                DownloadedFilesCache.RemoveFile(chapter.LargeImageURL);
                            }
                        }
                        BooksOnServerAccessor.RemoveChapters(book.ID);
                    }

                    // Remove book images from the file system
                    DownloadedFilesCache.RemoveFile(book.SmallImageURL);
                    DownloadedFilesCache.RemoveFile(book.LargeImageURL);

                    BooksOnDeviceAccessor.RemoveBook(book.ID);

                    // Initialize CurrentBook
                    InitializeFailedURLs();
                }
                else if (book.Status == Book.BookStatus.PENDING2UPDATE || book.Status == Book.BookStatus.UPDATING)
                {
                    RemoveBookFromBooks2Update(book.ID);

                    BooksOnServerAccessor.RemovePages(book.ID);
                    BooksOnServerAccessor.RemoveChapters(book.ID);
                }
            }
        }

        public static void RequestDidFail(NSObject sender)
        {
            try
            {
                ASIHTTPRequest request = sender as ASIHTTPRequest;
                if (request != null && CurrentBook != null)
                {
                    if (request.Url != null)
                    {
                        if (CurrentBook.FailedURLs == null)
                        {
                            CurrentBook.FailedURLs = new List<String>();
                        }

                        if (!CurrentBook.FailedURLs.Contains(request.Url.AbsoluteString))
                        {
                            CurrentBook.FailedURLs.Add(request.Url.AbsoluteString);

                            Logger.WriteLineDebugging("RequestDidFail: {0} pages failed to download so far...", CurrentBook.FailedURLs.Count.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("BookUpdater - RequestDidFail: {0}", ex.ToString());
            }
        }

        public static void RequestDidFinish(NSObject sender)
        {
            try
            {
                ASIHTTPRequest request = sender as ASIHTTPRequest;
                if (request != null)
                {
                    // THIS IS REQUIRED TO SKIP iCLOUD BACKUP
                    SkipBackup2iCloud.SetAttribute(request.DownloadDestinationPath);

                    // Removed from the list if succeeded re-downloading
                    if (CurrentBook.FailedURLs != null)
                    {
                        if (CurrentBook.FailedURLs.Contains(request.Url.AbsoluteString))
                        {
                            CurrentBook.FailedURLs.Remove(request.Url.AbsoluteString);
                        }
                    }

                    CurrentBook.DownloadCount += 1;

                    DownloadReporter.UpdateProgress();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("BookUpdater - RequestDidFinish: {0}", ex.ToString());
            }
        }

        public static void QueueDidFinish(NSObject sender)
        {
            try
            {
                if (CurrentBook != null)
                {
                    if (CurrentBook.Cancelled)
                    {
                        // Dequeue
                        Dequeue(CurrentBook.ID);

                        // If all the update has been completed, do followings
                        if (Books2Download != null && Books2Download.Count == 0)
                        {
                            DownloadFinished();
                        }
                        else
                        {
                            // Start downloading next book
                            Start();
                        }
                    }
                    else
                    {
                        if (CurrentBook.FailedURLs != null && CurrentBook.FailedURLs.Count > 0)
                        {
                            DownloadReporter.ShowAlertView();
                        }
                        else
                        {
                            // Initialize FailedURLs
                            if (CurrentBook.FailedURLs != null)
                            {
                                InitializeFailedURLs();
                            }

                            bool updated = false;
                            if (CurrentBook.Status == Book.BookStatus.UPDATING)
                            {
                                updated = true;

                                RemoveBookFromBooks2Update(CurrentBook.ID);
                            }

                            UpdateDatabase(CurrentBook);

                            DownloadReporter.DownloadFinished(updated);

                            // Dequeue
                            Dequeue(CurrentBook.ID);

                            // Start downloading next book
                            Start();

                            // If all the update has been completed, do followings
                            if (Books2Download != null && Books2Download.Count == 0)
                            {
                                DownloadFinished();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("BookUpdater - QueueDidFinish: {0}", ex.ToString());
            }
        }

        public static void Start()
        {
            if (Books2Download != null && Books2Download.Count > 0)
            {
                try
                {
                    // Check to see if there is any book that was downloading or updating in progress.
                    // If there is, then resume downloading and do not sort
                    bool sortRequired = true;
                    foreach (Book book in Books2Download)
                    {
                        if (book.Status == Book.BookStatus.DOWNLOADING || book.Status == Book.BookStatus.UPDATING)
                        {
                            sortRequired = false;
                        }
                    }

                    if (sortRequired)
                    {
                        SortBooks();
                    }

                    InProgress = true;
                    CurrentBook = Books2Download[0];

                    if (CurrentBook.Status == Book.BookStatus.PENDING2UPDATE)
                    {
                        CurrentBook.Status = Book.BookStatus.UPDATING;
                    }
                    else if (CurrentBook.Status == Book.BookStatus.PENDING2DOWNLOAD)
                    {
                        CurrentBook.Status = Book.BookStatus.DOWNLOADING;
                    }

                    BackgroundWorker downloadWorker = new BackgroundWorker();
                    downloadWorker.WorkerSupportsCancellation = true;
                    downloadWorker.DoWork += async delegate
                    {
                        // Get my stuff for this book
                        if (CurrentBook.Status == Book.BookStatus.DOWNLOADING)
                        {
                            if (!CloudSync.SyncingInProgress)
                            {
                                await eBriefingService.Run(() => CloudSync.PullMyStuffs(CurrentBook));
                            }
                        }

                        List<String> fileUrlsToDownload = new List<String>();

                        // Notify UI the start of the downloading
                        ParentViewController.InvokeOnMainThread(delegate
                        {
                            if (DownloadStartEvent != null)
                            {
                                DownloadStartEvent(CurrentBook.ID);
                            }
                        });

                        // Queue up cover images for the book only if the image version is different
                        if (CurrentBook.Status == Book.BookStatus.UPDATING)
                        {
                            Book deviceBook = BooksOnDeviceAccessor.GetBook(CurrentBook.ID);
                            if (deviceBook != null && (deviceBook.ImageVersion != CurrentBook.ImageVersion))
                            {
                                DownloadedFilesCache.RemoveFile(deviceBook.LargeImageURL);
                                DownloadedFilesCache.RemoveFile(deviceBook.SmallImageURL);

                                fileUrlsToDownload.Add(CurrentBook.LargeImageURL);
                                fileUrlsToDownload.Add(CurrentBook.SmallImageURL);
                            }
                        }

                        // Download chapters
                        await eBriefingService.Run(() => DownloadChaptersWork(CurrentBook.ID, fileUrlsToDownload));
                        
                        // Download pages
                        List<Page> pageList = await eBriefingService.Run(() => DownloadPagesWork(CurrentBook.ID));
                        if (pageList != null)
                        {
                            List<Page> differentPageList = Pages2Download(CurrentBook.ID, pageList);
                            foreach (Page page in differentPageList)
                            {
                                fileUrlsToDownload.Add(page.URL);
                            }
                        }

                        // Filter out those files that are already downloaded (Only if the status is DOWNLOADING, not UPDATING)
                        // CoreServices 2.0 will check for updates too since we can check for version number for each file
						fileUrlsToDownload = RemoveAlreadyDownloadedFiles(fileUrlsToDownload);

                        // Download Start
                        if (fileUrlsToDownload.Count == 0)
                        {
                            QueueDidFinish(null);
                        }
                        else
                        {
                            DownloadPDFsWork(CurrentBook.ID, fileUrlsToDownload);
                        }
                    };
                    downloadWorker.RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    Logger.WriteLineDebugging("BookUpdater - Start: {0}", ex.ToString());
                }
            }
        }

        public static void Stop()
        {
            networkQueue.CancelAllOperations();

            DownloadFinished();

            CheckBooks2Download();
        }

        public static void DownloadFinished()
        {
            InProgress = false;

            if (Books2Download != null)
            {
                Books2Download.Clear();
                Books2Download = null;
            }
        }

        private static void InitializeFailedURLs()
        {
            if (CurrentBook != null && CurrentBook.FailedURLs != null)
            {
                CurrentBook.FailedURLs.Clear();
                CurrentBook.FailedURLs = null;
            }
        }

        private static void UpdateDatabase(Book book)
        {
            try
            {
                List<Chapter> chapterList = BooksOnServerAccessor.GetChapters(book.ID);
                List<Page> pageList = BooksOnServerAccessor.GetPages(book.ID);

                // Update notes
                if (BooksOnDeviceAccessor.GetNotes(book.ID) != null)
                {
                    // Remove orphans
                    BooksOnDeviceAccessor.RemoveOrphanNotes(book.ID, pageList);
                }

                // Update bookmarks
                if (BooksOnDeviceAccessor.GetBookmarks(book.ID) != null)
                {
                    // Remove orphans
                    BooksOnDeviceAccessor.RemoveOrphanBookmarks(book.ID, pageList);
                }

                // Update new chapters
                if (BooksOnDeviceAccessor.GetChapters(book.ID) == null)
                {
                    BooksOnDeviceAccessor.AddChapters(book.ID, chapterList);
                }
                else
                {
                    BooksOnDeviceAccessor.UpdateChapters(book.ID, chapterList);
                }
                
                BooksOnDeviceAccessor.MapPagesToChapter(chapterList, pageList); // ML: 4/9/2013 We need to map them after each update

                // Update new pages
                if (BooksOnDeviceAccessor.GetPages(book.ID) == null)
                {
                    BooksOnDeviceAccessor.AddPages(book.ID, pageList);
                }
                else
                {
                    BooksOnDeviceAccessor.UpdatePages(book.ID, pageList);
                }

                // Remove chapters and pages list from BooksOnServer because they now exist on the device
                BooksOnServerAccessor.RemoveChapters(book.ID);
                BooksOnServerAccessor.RemovePages(book.ID);

                // Update new book
                book.New = true;
                book.Status = Book.BookStatus.DOWNLOADED;
                book.UserAddedDate = DateTime.UtcNow;

                if (BooksOnDeviceAccessor.GetBook(book.ID) == null)
                {
                    BooksOnDeviceAccessor.AddBook(book);
                }
                else
                {
                    if (BooksDataAccessor.IsFavorite(book.ID))
                    {
                        book.IsFavorite = true;
                    }
                    BooksOnDeviceAccessor.UpdateBook(book);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLineDebugging("BookUpdater - UpdateDatabase: {0}", ex.ToString());
            }
        }

        private static List<Chapter> DownloadChaptersWork(String bookID, List<String> fileUrlsToDownload)
        {
            // If chapters are not downloaded yet or expired, download them again
            List<Chapter> chapterList = null;
            if (!BooksOnServerAccessor.HasChapters(bookID) || CurrentBook.Status == Book.BookStatus.UPDATING)
            {
                chapterList = eBriefingService.StartDownloadChapters(bookID);
                if (chapterList != null)
                {
                    BooksOnServerAccessor.SaveChapters(bookID, chapterList);
                }
            }
            else
            {
                // Or get them from the cache
                chapterList = BooksOnServerAccessor.GetChapters(bookID);
            }

            // Queue up cover images for the chapter only if the image version is different
            if (chapterList != null)
            {
                foreach (Chapter serverCh in chapterList)
                {
                    // Remove cover image if this is an update
                    bool download = false;
                    if (CurrentBook.Status == Book.BookStatus.UPDATING)
                    {
                        Chapter deviceCh = BooksOnDeviceAccessor.GetChapter(CurrentBook.ID, serverCh.ID);
                        if (deviceCh == null)
                        {
                            download = true;
                        }
                        else if (serverCh.ImageVersion != deviceCh.ImageVersion)
                        {
                            DownloadedFilesCache.RemoveFile(deviceCh.LargeImageURL);
                            DownloadedFilesCache.RemoveFile(deviceCh.SmallImageURL);

                            download = true;
                        }
                    }
                    else
                    {
                        download = true;
                    }

                    if (download)
                    {
                        fileUrlsToDownload.Add(serverCh.LargeImageURL);
                        fileUrlsToDownload.Add(serverCh.SmallImageURL);
                    }
                }
            }

            return chapterList;
        }

        private static List<Page> DownloadPagesWork(String bookID)
        {
            // If pages are not downloaded yet or expired, download them again
            if (!BooksOnServerAccessor.HasPages(bookID) || CurrentBook.Status == Book.BookStatus.UPDATING)
            {
                List<Page> pageList = eBriefingService.StartDownloadPages(bookID);
                if (pageList != null)
                {
                    BooksOnServerAccessor.SavePages(bookID, pageList);

                    return pageList;
                }
                return null;
            }
            else
            {
                // Or get them from the cache
                return BooksOnServerAccessor.GetPages(bookID);
            }
        }

        private static List<Page> Pages2Download(String bookID, List<Page> serverPageList)
        {
            // Compare pages on server and device and download only that are different
            List<Page> differentPageList = new List<Page>();
            List<Page> devicePageList = BooksOnDeviceAccessor.GetPages(bookID);
            
            if (devicePageList == null || devicePageList.Count == 0)
            {
                differentPageList = serverPageList;
            }
            else
            {
				foreach (Page serverPage in serverPageList)
               	{
                    var item = devicePageList.Where(i => i.ID == serverPage.ID).FirstOrDefault();
                    if (item != null)
                    {
                        if (item.Version != serverPage.Version)
                        {
                            differentPageList.Add(serverPage);
                        }
                    }
                    else
                    {
                        differentPageList.Add(serverPage);
                    }
                }
				RemovePages (serverPageList, devicePageList);
            }

            return differentPageList;
        }

		//to remove page that not being use anymore in device.
		private static void RemovePages(List<Page> serverPageList, List<Page> devicePageList)
		{
	        foreach (Page devicePage in devicePageList)
	        {
	            var item = serverPageList.Where(i => i.ID == devicePage.ID).FirstOrDefault();
	            if (item == null)
	            {
	                DownloadedFilesCache.RemoveFile(devicePage.URL);
	            }
	        }
		}

        private static void DownloadPDFsWork(String bookID, List<String> urlList)
        {
            if (urlList.Count > 0)
            {
                networkQueue.Reset();
                networkQueue.ShowAccurateProgress = false;
                networkQueue.ShouldCancelAllRequestsOnFailure = false;
                networkQueue.Delegate = ParentViewController;

                networkQueue.RequestDidFail = new Selector("requestDidFail:");
                networkQueue.RequestDidFinish = new Selector("requestDidFinish:");
                networkQueue.QueueDidFinish = new Selector("queueDidFinish:");

                ASIHTTPRequest request = null;
                foreach (String url in urlList)
                {
                    // Remove page if this is an update
                    if (CurrentBook.Status == Book.BookStatus.UPDATING)
                    {
                        DownloadedFilesCache.RemoveFile(url);
                    }

					request = new ASIHTTPRequest(NSUrl.FromString(url));
                    request.DownloadDestinationPath = DownloadedFilesCache.BuildCachedFilePath(url);
                    request.Username = Settings.UserID;
                    request.Password = KeychainAccessor.Password;
                    request.Domain = Settings.Domain;
                    
                    networkQueue.AddOperation(request);
                }

                // Clear failedUrls before starting to re-download
                InitializeFailedURLs();

                networkQueue.Go();
            }
            else
            {
                // Finished downloading
                if (DownloadFinishEvent != null)
                {
                    DownloadFinishEvent(bookID);
                }
            }
        }

        private static List<String> RemoveAlreadyDownloadedFiles(List<String> fileUrlsToDownload)
        {
            // Set total number of downloads required
            TotalDownloadCount = fileUrlsToDownload.Count;

            if (CurrentBook.Status == Book.BookStatus.DOWNLOADING)
            {
                List<String> removeList = null;
                for (int i = 0; i < fileUrlsToDownload.Count; i++)
                {
                    String localPath = DownloadedFilesCache.BuildCachedFilePath(fileUrlsToDownload[i]);
                    if (File.Exists(localPath))
                    {
                        if (removeList == null)
                        {
                            removeList = new List<String>();
                        }

                        removeList.Add(fileUrlsToDownload[i]);
                    }
                }

                if (removeList != null)
                {
                    for (int i = 0; i < removeList.Count; i++)
                    {
                        fileUrlsToDownload.Remove(removeList[i]);
                    }
                }
            }

            // Set total number of already downloaded files
            CurrentBook.DownloadCount = TotalDownloadCount - fileUrlsToDownload.Count;

            return fileUrlsToDownload;
        }

        public static void DoesBooksNeedUpdate(List<Book> booksOnServer)
        {
            List<Book> booksOnDevice = BooksOnDeviceAccessor.GetBooks();

            if (booksOnDevice != null)
            {
                if (Books2Update != null)
                {
                    Books2Update.Clear();
                }

                foreach (Book bookOnDevice in booksOnDevice)
                {
                    if (booksOnServer != null)
                    {
                        List<Book> list = booksOnServer.Where(i => i.ID == bookOnDevice.ID && bookOnDevice.Status == Book.BookStatus.DOWNLOADED && i.Version > bookOnDevice.Version).ToList();
                        if (list != null && list.Count > 0)
                        {
                            foreach (var item in list)
                            {
                                if (Books2Update == null)
                                {
                                    Books2Update = new List<Book>();
                                }

                                item.New = false;
                                item.Status = Book.BookStatus.ISUPDATE;
                                Books2Update.Add(item);
                            }
                        }
                    }
                }

                if (Books2Update != null && Books2Update.Count > 0)
                {
                    if (UpdateNeededEvent != null)
                    {
                        UpdateNeededEvent(Books2Update.Count);
                    }
                }
            }
        }

        private static void SortBooks()
        {
            if (Settings.SortBy == StringRef.ByName)
            {
                if (Settings.SortAscending)
                {
                    Books2Download.Sort((x, y) => x.Title.CompareTo(y.Title));
                }
                else
                {
                    Books2Download.Sort((x, y) => y.Title.CompareTo(x.Title));
                }

            }
            else if (Settings.SortBy == StringRef.ByDateAdded)
            {
                if (Settings.SortAscending)
                {
                    Books2Download.Sort((x, y) => x.ServerAddedDate.CompareTo(y.ServerAddedDate));
                }
                else
                {
                    Books2Download.Sort((x, y) => y.ServerAddedDate.CompareTo(x.ServerAddedDate));
                }
            }
            else
            {
                if (Settings.SortAscending)
                {
                    Books2Download.Sort((x, y) => x.ServerModifiedDate.CompareTo(y.ServerModifiedDate));
                }
                else
                {
                    Books2Download.Sort((x, y) => y.ServerModifiedDate.CompareTo(x.ServerModifiedDate));
                }
            }
        }
    }
}

