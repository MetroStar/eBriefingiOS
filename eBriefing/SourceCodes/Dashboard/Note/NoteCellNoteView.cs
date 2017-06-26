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
using Foundation;
using UIKit;
using CoreGraphics;

namespace eBriefingMobile
{
    public class NoteCellNoteView : UIButton
    {
        public NoteCellNoteView(Note note, nfloat height)
        {
            this.BackgroundColor = UIColor.White;
            this.Frame = new CGRect(0, 0, 220, height);
            this.Layer.BorderColor = eBriefingAppearance.Gray5.CGColor;
            this.Layer.BorderWidth = 1f;

            // label
            UILabel label = eBriefingAppearance.GenerateLabel(14, eBriefingAppearance.Gray3);
            label.Text = note.ModifiedUtc.ToString("MM.dd.yyyy");
            label.Frame = new CGRect(10, this.Frame.Bottom - 26, this.Frame.Width - 20, 21);
            this.AddSubview(label);

            // line
            UIView line = new UIView(new CGRect(10, label.Frame.Y - 5, this.Frame.Width - 20, 1));
            line.BackgroundColor = eBriefingAppearance.Gray5;
            this.AddSubview(line);

            // noteView
            UILabel noteView = eBriefingAppearance.GenerateLabel(17);
            noteView.Frame = new CGRect(10, 10, this.Frame.Width - 20, line.Frame.Y - 10);
            noteView.Text = note.Text;
            noteView.TextColor = eBriefingAppearance.Gray2;
            noteView.Lines = 0;
            noteView.LineBreakMode = UILineBreakMode.TailTruncation;
            this.AddSubview(noteView);
        }
    }
}

