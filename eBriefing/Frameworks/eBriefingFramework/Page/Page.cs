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

namespace eBriefingMobile
{
    [Serializable]
    public class Page
    {
        public String ID { get; set; }

        public String URL { get; set; }

        public int PageNumber { get; set; }

        public String MD5 { get; set; }

        public String Type { get; set; }

        public int Version { get; set; }

        public String ChapterID { get; set; }

        public Page()
        {
            PageNumber = int.MinValue;
            Version = int.MinValue;
        }
    }
}

