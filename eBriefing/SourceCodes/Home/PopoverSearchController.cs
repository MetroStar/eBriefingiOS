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
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreGraphics;
using Metrostar.Mobile.Framework;
using MssFramework;

namespace eBriefingMobile
{
    public class PopoverSearchController : UIViewController
    {
        private String searchString;
        private UISearchDisplayController searchDisplayController;
        private CustomSearchBar searchBar;
        private static nfloat RESULT_WIDTH = 320;
        private static nfloat RESULT_HEIGHT = 100;
        private static nfloat HEADER_HEIGHT = 40;

        public delegate void PopoverSearchDelegate0 (nfloat height);

        public delegate void PopoverSearchDelegate1 (Book book, bool availableInMyBooks);

        public event PopoverSearchDelegate0 ResizePopoverEvent;
        public event PopoverSearchDelegate1 RowSelectedEvent;

        public PopoverSearchController(String searchString)
        {
            this.searchString = searchString;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // searchBar
            searchBar = new CustomSearchBar();
            searchBar.Frame = new CGRect(0, 0, RESULT_WIDTH, searchBar.Frame.Height);
            searchBar.Placeholder = "Search for Books";
            searchBar.Text = searchString;
            this.View.AddSubview(searchBar);

            // searchDelegate
            BookSearchDisplayDelegate searchDelegate = new BookSearchDisplayDelegate(this);
            searchDelegate.DidBeginEvent += HandleDidBeginEvent;

            // searchDisplayController
            searchDisplayController = new UISearchDisplayController(searchBar, this);
            searchDisplayController.Delegate = searchDelegate;

            this.View.AutoresizingMask = UIViewAutoresizing.None;
            this.View.Frame = new CGRect(0, 0, searchBar.Frame.Width, 600);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            this.PreferredContentSize = new CGSize(this.View.Frame.Width, searchBar.Frame.Height);

            if (String.IsNullOrEmpty(searchBar.Text))
            {
                searchBar.BecomeFirstResponder();
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (!String.IsNullOrEmpty(searchBar.Text))
            {
                searchDisplayController.Delegate.ShouldReloadForSearchString(searchDisplayController, searchBar.Text);
            }
        }

        void HandleDidBeginEvent(String searchString, List<Book> myBooks, List<Book> availableBooks)
        {
            this.searchString = searchString;

            nfloat totalHeight = 0;
            if ((myBooks != null && myBooks.Count > 0) && (availableBooks != null && availableBooks.Count > 0))
            {
                totalHeight = (HEADER_HEIGHT * 2) + (myBooks.Count * RESULT_HEIGHT) + (availableBooks.Count * RESULT_HEIGHT);
            }
            else
            {
                if ((myBooks != null && myBooks.Count > 0) && (availableBooks == null || availableBooks.Count <= 0))
                {
                    totalHeight = HEADER_HEIGHT + (myBooks.Count * RESULT_HEIGHT);
                }
                else if ((availableBooks != null && availableBooks.Count > 0) && (myBooks == null || myBooks.Count <= 0))
                {
                    totalHeight = HEADER_HEIGHT + (availableBooks.Count * RESULT_HEIGHT);
                }
            }

            // Resize the view to fit the result
            UIView.Animate(0.3d, delegate
            {
                this.View.Frame = new CGRect(0, 0, this.View.Frame.Width, searchBar.Frame.Height + totalHeight);
            });

            // Update PreferredContentSize
            if (ResizePopoverEvent != null)
            {
                ResizePopoverEvent(this.View.Frame.Height);
            }
        }

        public class PopoverSearchDataSource : UITableViewSource
        {
            private enum Availability
            {
                NONE,
                ONLY_MYBOOKS,
                ONLY_AVAILABLE,
                BOTH
            }

            private List<Book> myBooks;
            private List<Book> availableBooks;
            private PopoverSearchController parent;
            private Dictionary<NSIndexPath, PopoverSearchResultView> dictionary = null;
            private Availability availability = Availability.NONE;

            public PopoverSearchDataSource(PopoverSearchController parent, List<Book> myBooks, List<Book> availableBooks)
            {
                this.parent = parent;
                this.myBooks = myBooks;
                this.availableBooks = availableBooks;

                dictionary = new Dictionary<NSIndexPath, PopoverSearchResultView>();
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if ((myBooks != null && myBooks.Count > 0) && (availableBooks != null && availableBooks.Count > 0))
                {
                    availability = Availability.BOTH;
                    return 2;
                }
                else
                {
                    if ((myBooks != null && myBooks.Count > 0) && (availableBooks == null || availableBooks.Count <= 0))
                    {
                        availability = Availability.ONLY_MYBOOKS;
                        return 1;
                    }
                    else if ((availableBooks != null && availableBooks.Count > 0) && (myBooks == null || myBooks.Count <= 0))
                    {
                        availability = Availability.ONLY_AVAILABLE;
                        return 1;
                    }

                    availability = Availability.NONE;

                    return 0;
                }
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                Book book = null;

                if (indexPath.Section == 0)
                {
                    if (availability == Availability.BOTH || availability == Availability.ONLY_MYBOOKS)
                    {
                        book = myBooks[indexPath.Row];
                    }
                    else
                    {
                        book = availableBooks[indexPath.Row];
                    }
                }
                else
                {
                    book = availableBooks[indexPath.Row];
                }

                if (book != null)
                {
                    PopoverSearchResultView resultView = new PopoverSearchResultView(book, new CGRect(0, 0, RESULT_WIDTH, RESULT_HEIGHT));

                    if (!dictionary.ContainsKey(indexPath))
                    {
                        dictionary.Add(indexPath, resultView);
                    }

                    return resultView.Frame.Height;
                }

                return 0;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (section == 0)
                {
                    if (availability == Availability.BOTH || availability == Availability.ONLY_MYBOOKS)
                    {
                        return myBooks.Count;
                    }
                    else
                    {
                        return availableBooks.Count;
                    }
                }
                else
                {
                    return availableBooks.Count;
                }
            }

            public override nfloat GetHeightForHeader(UITableView tableView, nint section)
            {
                return HEADER_HEIGHT;
            }

            public override String TitleForHeader(UITableView tableView, nint section)
            {
                if (section == 0)
                {
                    if (availability == Availability.BOTH || availability == Availability.ONLY_MYBOOKS)
                    {
                        return StringRef.myBooks;
                    }
                    else
                    {
                        return StringRef.available;
                    }
                }
                else
                {
                    return StringRef.available;
                }
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                UITableViewCell cell = tableView.DequeueReusableCell("PopoverSearchIdentifier");

                try
                {
                    if (cell == null)
                    {
                        cell = new UITableViewCell(UITableViewCellStyle.Default, "PopoverSearchIdentifier");
                    }
                    else
                    {
                        foreach (UIView subview in cell.ContentView)
                        {
                            if (subview.Tag == -1)
                            {
                                subview.RemoveFromSuperview();
                            }
                        }

                        cell.BackgroundView = null;
                    }

                    cell.Accessory = UITableViewCellAccessory.None;
                    cell.SelectionStyle = UITableViewCellSelectionStyle.Default;

                    if (dictionary.ContainsKey(indexPath))
                    {
                        PopoverSearchResultView resultView = dictionary[indexPath];
                        resultView.Tag = -1;
                        cell.ContentView.AddSubview(resultView);
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLineDebugging("PopoverSearchDataSource - GetCell: {0}", ex.ToString());
                }

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow(indexPath, true);

                if (parent.RowSelectedEvent != null)
                {
                    Book book = null;
                    bool availableInMyBooks = false;

                    if (indexPath.Section == 0)
                    {
                        if (availability == Availability.BOTH || availability == Availability.ONLY_MYBOOKS)
                        {
                            availableInMyBooks = true;
                            book = myBooks[indexPath.Row];
                        }
                        else
                        {
                            book = availableBooks[indexPath.Row];
                        }
                    }
                    else
                    {
                        book = availableBooks[indexPath.Row];
                    }

                    parent.RowSelectedEvent(book, availableInMyBooks);
                }
            }
        }

        private class BookSearchDisplayDelegate : CustomSearchDisplayDelegate
        {
            private List<Book> myBooks;
            private List<Book> availableBooks;
            private PopoverSearchController parent;

            public delegate void DidBeginDelegate (String searchString, List<Book> myBooks, List<Book> availableBooks);

            public event DidBeginDelegate DidBeginEvent;

            public BookSearchDisplayDelegate(PopoverSearchController parent)
            {
                this.parent = parent;

                myBooks = BooksOnDeviceAccessor.GetBooks();
                availableBooks = BooksOnServerAccessor.GetBooks();
            }

            public override bool ShouldReloadForSearchString(UISearchDisplayController controller, String forSearchString)
            {
                try
                {
                    this.searchDisplayController = controller;

                    // Filter myBooks
                    List<Book> filteredMyBooks = new List<Book>();
                    if (!String.IsNullOrEmpty(forSearchString) && myBooks != null)
                    {
                        foreach (Book book in myBooks)
                        {
                            if (!String.IsNullOrEmpty(book.Title) && book.Title.ToLower().Contains(forSearchString.ToLower())
                                || !String.IsNullOrEmpty(book.Description) && book.Description.ToLower().Contains(forSearchString.ToLower()))
                            {
                                filteredMyBooks.Add(book);
                            }
                        }
                    }

                    // Filter availableBooks
                    List<Book> filteredAvailableBooks = new List<Book>();
                    if (!String.IsNullOrEmpty(forSearchString) && availableBooks != null)
                    {
                        foreach (Book b1 in availableBooks)
                        {
                            if (!String.IsNullOrEmpty(b1.Title) && b1.Title.ToLower().Contains(forSearchString.ToLower())
                                || !String.IsNullOrEmpty(b1.Description) && b1.Description.ToLower().Contains(forSearchString.ToLower()))
                            {
                                bool notFound = true;
                                if (myBooks != null)
                                {
                                    foreach (Book b2 in myBooks)
                                    {
                                        if (b1.ID == b2.ID)
                                        {
                                            notFound = false;
                                        }
                                    }
                                }

                                if (notFound)
                                {
                                    filteredAvailableBooks.Add(b1);
                                }
                            }
                        }
                    }

                    // Reload
                    PopoverSearchDataSource dataSource = new PopoverSearchDataSource(parent, filteredMyBooks, filteredAvailableBooks);
                    controller.SearchResultsSource = dataSource;

                    if (DidBeginEvent != null)
                    {
                        DidBeginEvent(forSearchString, filteredMyBooks, filteredAvailableBooks);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("BookSearchDisplayDelegate - ShouldReloadForSearchString: {0}", ex.ToString());
                }

                return true;
            }
        }
    }
}

