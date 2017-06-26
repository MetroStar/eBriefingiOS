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
    [Serializable]
    public class Book
    {
        public enum BookStatus
        {
            NONE,
            PENDING2DOWNLOAD,
            PENDING2UPDATE,
            DOWNLOADING,
            UPDATING,
            DOWNLOADED,
            ISUPDATE
        }

        public String ID { get; set; }

        public String Title { get; set; }

        public String Description { get; set; }

        public int ChapterCount { get; set; }
        // Total number of chapters in this book
        public int PageCount { get; set; }
        // Total number of pages in this book
        public String SmallImageURL { get; set; }

        public String LargeImageURL { get; set; }

        public int ImageVersion { get; set; }
        // Book cover image version
        public int Version { get; set; }
        // Book version
        public DateTime ServerAddedDate { get; set; }
        // Server added date
        public DateTime ServerModifiedDate { get; set; }
        // Server modified date
        public DateTime UserAddedDate { get; set; }
        // User added date
        public DateTime UserModifiedDate { get; set; }
        // User modified date (Bookmark, Note, or Annotation)
        public bool IsFavorite { get; set; }
        // Indicates whether this book is marked as favorite
        public bool Cancelled { get; set; }
        // Cancelled download
        public float DownloadProgress { get; set; }
        // 1 being maximum
        public float DownloadCount { get; set; }
        // Current download count
        public List<String> FailedURLs { get; set; }
        // Failed to download urls
        public BookStatus Status = BookStatus.NONE;
        // Download status
        public bool Removed { get; set; }
        // Indicates whether this book was removed from the cloud
        public bool New { get; set; }
        // Indicates whether this book was opened in My books tab
        public bool Viewed { get; set; }
        // Indicates whether this book was viewed in Available tab
        public DateTime LastSyncedDate;

        public Book()
        {
            ChapterCount = 0;
            PageCount = 0;
            DownloadProgress = 0;
            DownloadCount = 0;
            New = true;
            Viewed = false;
        }

        public Book Copy()
        {
            Book book = new Book();
            book.ID = this.ID.ToString();
            book.Title = this.Title.Replace(System.Environment.NewLine, " ");
            book.Description = this.Description.Replace(System.Environment.NewLine, " ");
            book.ChapterCount = this.ChapterCount;
            book.PageCount = this.PageCount;
            book.SmallImageURL = this.SmallImageURL;
            book.LargeImageURL = this.LargeImageURL;
            book.ImageVersion = this.ImageVersion;
            book.Version = Convert.ToInt32(this.Version);
            book.ServerAddedDate = this.ServerAddedDate;
            book.ServerModifiedDate = this.ServerModifiedDate;
            book.UserAddedDate = this.UserAddedDate;
            book.UserModifiedDate = this.UserModifiedDate;
            book.IsFavorite = this.IsFavorite;
            book.Cancelled = this.Cancelled;
            book.DownloadProgress = this.DownloadProgress;
            book.DownloadCount = this.DownloadCount;
            book.FailedURLs = this.FailedURLs;
            book.Status = this.Status;
            book.Removed = this.Removed;
            book.New = this.New;
            book.Viewed = this.Viewed;
			book.LastSyncedDate = this.LastSyncedDate;

            return book;
        }
    }
}

