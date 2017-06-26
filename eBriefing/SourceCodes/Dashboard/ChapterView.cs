using System;
using System.Drawing;
using System.ComponentModel;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Metrostar.Mobile.Framework;

namespace eBriefingMobile
{
    public class ChapterView : UIView
    {
        public Chapter BookChapter { get; set; }

        public ChapterView(String bookID, Chapter chapter, Int32 index) : base(new RectangleF(0, 0, 220, 388.33f))
        {
            this.BackgroundColor = UIColor.Clear;
            this.Layer.ShadowColor = UIColor.Black.CGColor;
            this.Layer.ShadowOpacity = 0.3f;
            this.Layer.ShadowRadius = 2f;
            this.Layer.ShadowOffset = new SizeF(0f, 2f);

            this.Layer.ShadowPath = UIBezierPath.FromRoundedRect(this.Frame, 7f).CGPath;
            this.Layer.ShouldRasterize = true;
            this.Layer.RasterizationScale = UIScreen.MainScreen.Scale;

            this.BookChapter = chapter;

            // For rounded corner and shadow
            UIView subView = new UIView(new RectangleF(0, 0, 220, 388.33f));
            subView.BackgroundColor = UIColor.White;
            subView.Layer.CornerRadius = 7f;
            subView.Layer.MasksToBounds = true;
            this.AddSubview(subView);

            // imageView
            UIImageView imageView = new UIImageView();
            imageView.Frame = new RectangleF(0, 0, this.Frame.Width, 293.33f);
            String localImagePath = DownloadedFilesCache.BuildCachedFilePath(BookChapter.LargeImageURL);
            imageView.Image = UIImage.FromFile(localImagePath);
            imageView.ContentMode = UIViewContentMode.ScaleToFill;
            subView.AddSubview(imageView);

            // chapterLabel
            UILabel chapterLabel = new UILabel();
            chapterLabel.Frame = new RectangleF(10, imageView.Frame.Bottom + 8, 200, 21);
            chapterLabel.Font = UIFont.SystemFontOfSize(14f);
            chapterLabel.TextAlignment = UITextAlignment.Left;
            chapterLabel.BackgroundColor = UIColor.Clear;
            chapterLabel.TextColor = UIColor.DarkGray;
            chapterLabel.Text = "Chapter " + (index + 1).ToString();
            this.AddSubview(chapterLabel);
            
            // titleLabel
            UILabel titleLabel = new UILabel();
            titleLabel.Frame = new RectangleF(10, chapterLabel.Frame.Bottom + 4, 200, 21);
            titleLabel.Font = UIFont.SystemFontOfSize(14f);
            titleLabel.BackgroundColor = UIColor.Clear;
            titleLabel.TextColor = UIColor.DarkGray;
            titleLabel.LineBreakMode = UILineBreakMode.TailTruncation;
            titleLabel.Text = chapter.Title;
            this.AddSubview(titleLabel);

            // bookInfoView
            String numNotes = "0";
            String numBookmarks = "0";
            String numAnnotations = "0";

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate
            {
                numNotes = BooksOnDeviceAccessor.GetNumNotesInChapter(bookID, chapter.ID);
                numBookmarks = BooksOnDeviceAccessor.GetNumBookmarksInChapter(bookID, chapter.ID);
                numAnnotations = BooksOnDeviceAccessor.GetNumAnnotationsInChapter(bookID, chapter.ID);
            };
            worker.RunWorkerCompleted += delegate
            {
                this.InvokeOnMainThread(delegate
                {
                    BookInfoView bookInfoView = new BookInfoView(numNotes, numBookmarks, numAnnotations, chapter.Pagecount.ToString(), false, false, this.Frame.Width - 30);
                    bookInfoView.Frame = new RectangleF(10, this.Frame.Bottom - 40, bookInfoView.Frame.Width, bookInfoView.Frame.Height);
                    this.AddSubview(bookInfoView);
                });
            };
            worker.RunWorkerAsync();
        }
    }
}

