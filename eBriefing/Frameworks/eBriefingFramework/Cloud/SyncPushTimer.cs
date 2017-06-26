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

namespace eBriefingMobile
{
    public static class SyncPushTimer
    {
        private static NSTimer timer = null;

        public static void Stop()
        {
            if (timer != null)
            {
                timer.Invalidate();
                timer.Dispose();
                timer = null;
            }
        }

        public static void Start()
        {
            if (timer == null)
            {
                timer = NSTimer.CreateRepeatingScheduledTimer(TimeSettings.SyncPushInterval, delegate(NSTimer obj)
                {
                    Push();
                });
            }
        }

        async public static void Push()
        {
            if (!CloudSync.SyncingInProgress)
            {
                await eBriefingService.Run(() => CloudSync.Push());
            }
        }
    }
}

