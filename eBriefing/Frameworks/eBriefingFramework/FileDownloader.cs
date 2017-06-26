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
using System.ComponentModel;
using Foundation;
using UIKit;
using ObjCRuntime;
using Metrostar.Mobile.Framework;
using MssFramework;
using ASIHTTPRequestBinding;

namespace eBriefingMobile
{
    public static class FileDownloader
    {
        public static bool Download(String url, UIViewController controller, bool forceDownload = false)
        {
            bool exist = false;

            if (!String.IsNullOrEmpty(url) && url.Contains("http"))
            {
                try
                {
                    String localPath = DownloadedFilesCache.BuildCachedFilePath(url);
                    if (File.Exists(localPath) && !forceDownload)
                    {
                        exist = true;
                    }
                    else
                    {
                        if (Reachability.IsDefaultNetworkAvailable())
                        {
                            BackgroundWorker downloadWorker = new BackgroundWorker();
                            downloadWorker.DoWork += delegate
                            {
								ASIHTTPRequest request = new ASIHTTPRequest(NSUrl.FromString(url));
                                request.Username = Settings.UserID;
                                request.Password = KeychainAccessor.Password;
                                request.Domain = Settings.Domain;
                                request.Delegate = controller;
								request.DidFinishSelector = new Selector("requestFinish:");
                                request.DownloadDestinationPath = DownloadedFilesCache.BuildCachedFilePath(url);
                                request.StartAsynchronous();
                            };
                            downloadWorker.RunWorkerAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLineDebugging("FileDownloader - Download: {0}", ex.ToString());
                }
            }

            return exist;
        }
    }
}

