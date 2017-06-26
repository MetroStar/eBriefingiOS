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
using System.Net;
using Foundation;
using Security;
using Metrostar.Mobile.Framework;

namespace eBriefingMobile
{
    public class KeychainAccessor
    {
        public static String Password
        {
            get
            {
                if (!String.IsNullOrEmpty(URL.ServerURL))
                {
                    var record = new SecRecord(SecKind.GenericPassword) {
                        Service = URL.ServerURL,
                    };
                
                    SecStatusCode statusCode;
                    var data = SecKeyChain.QueryAsRecord(record, out statusCode);
                    if (statusCode == SecStatusCode.Success)
                    {
                        return NSString.FromData(data.Generic, NSStringEncoding.UTF8);
                    }
                    else if (statusCode != SecStatusCode.ItemNotFound)
                    {
                        Logger.WriteLineDebugging("Could not retrieve password from KeyChain: {0}", statusCode.ToString());
                    }
                }

                return String.Empty;
            }
        }

        public static NetworkCredential NetworkCredential
        {
            get
            {
                String password = Password;
                if (!String.IsNullOrEmpty(password))
                {
                    return new NetworkCredential(Settings.UserID, password, Settings.Domain);
                }

                return null;
            }
        }

        public static void SaveCredential(String password)
        {
            SecRecord record = new SecRecord(SecKind.GenericPassword);
            record.Service = URL.ServerURL;
            record.Generic = NSData.FromString(password, NSStringEncoding.UTF8);
            record.Accessible = SecAccessible.Always;

            var statusCode = SecKeyChain.Add(record);
            if (statusCode != SecStatusCode.Success)
            {
                Logger.WriteLineDebugging("Could not save account to KeyChain: {0}", statusCode.ToString());
            }
        }

        public static void RemoveCredential()
        {
            if (!String.IsNullOrEmpty(URL.ServerURL))
            {
                var record = new SecRecord(SecKind.GenericPassword)
                {
                    Service = URL.ServerURL,
                };

                SecKeyChain.Remove(record);
            }
        }

        public static void ClearCredential()
        {
            RemoveCredential();

            Settings.ClearCredential();
        }
    }
}

