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
    public static class NoteUpdater
    {
        public static Note SaveNote(Book book, String pageID, String text, String noteID = "")
        {
            String ID = String.Empty;
            if (String.IsNullOrEmpty(URL.MultipleNoteURL))
            {
                ID = pageID;
            }
            else
            {
                ID = noteID;
            }

            Note note = BooksOnDeviceAccessor.GetNote(book.ID, ID);
            if (note == null)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    note = new Note();
                    note.BookID = book.ID;
                    note.PageID = pageID;
                    note.NoteID = Guid.NewGuid().ToString();
                    note.Text = text;
                    note.BookVersion = book.Version;
                    note.ModifiedUtc = note.CreatedUtc = DateTime.UtcNow;
                    Add(note);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(text))
                {
                    Remove(book.ID, ID);
                }
                else
                {
                    note.Text = text;
                    note.ModifiedUtc = DateTime.UtcNow;

                    Update(note);
                }
            }

            BooksOnDeviceAccessor.UpdateUserModifiedDate(book);

            return note;
        }

        private static void Add(Note note)
        {
            BooksOnDeviceAccessor.AddNote(note);
        }

        private static void Remove(String bookID, String ID)
        {
            BooksOnDeviceAccessor.MarkAsRemovedNote(bookID, ID);
        }

        private static void Update(Note note)
        {
            BooksOnDeviceAccessor.UpdateNote(note);
        }
    }
}

