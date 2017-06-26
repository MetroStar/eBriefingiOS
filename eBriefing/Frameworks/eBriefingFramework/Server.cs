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
using System.Xml;
using Metrostar.Mobile.Framework;
using eBriefing.com.metrostarsystems.ebriefingweb3;
using System.Net;

namespace eBriefingMobile
{
    public class Server
    {
        public static String Generate2010ServerInfoURL(String url)
        {
            return GenerateURL(url, "_layouts/eBriefing/ServerInfo.asmx");
        }

        public static String Generate2013ServerInfoURL(String url)
        {
            return GenerateURL(url, "_layouts/15/eBriefing/ServerInfo.asmx");
        }

        public static String GenerateFormsAuthenticationURL(String url)
        {
            return GenerateURL(url, "_vti_bin/authentication.asmx");
        }

		public static WebExceptionStatus CheckCompatibility2013(String url, String id, String password, String domain)
        {
            ServerInfo webService = new ServerInfo(Server.Generate2013ServerInfoURL(url));
            webService.Credentials = new NetworkCredential(id, password, domain);

            return CheckCompatibility(webService, url);
        }

		public static WebExceptionStatus CheckCompatibility2010(String url, String id, String password, String domain)
        {
            ServerInfo webService = new ServerInfo(Server.Generate2010ServerInfoURL(url));
            webService.Credentials = new NetworkCredential(id, password, domain);

            return CheckCompatibility(webService, url);
        }

		private static WebExceptionStatus CheckCompatibility(ServerInfo webService, String url)
		{
			String xml = URL.Core2URL = URL.ContentSyncURL = URL.MultipleNoteURL = String.Empty;
		
			try
			{
				webService.Timeout = 45000;
				xml = webService.GetServerInfo();
			}
			catch (Exception ex)
			{
				if ( ex is WebException )
				{
					return (ex as WebException).Status;
				}
				else
				{
					return WebExceptionStatus.ConnectFailure;
				}
			}

			if (!String.IsNullOrEmpty(xml))
			{
				try
				{
					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(xml.Trim());

					XmlNodeList nodes = xmlDoc.SelectNodes("//ServerInfo/Features/Feature");
					if (nodes != null)
					{
						foreach (XmlNode node in nodes)
						{
							String name = node["Name"].InnerText;
							String path = node["RelativePath"].InnerText;
							String newURL = GenerateURL(url, path);

							if (!String.IsNullOrEmpty(name) && name == "Core")
							{
								URL.Core2URL = newURL;
							}
							else if (!String.IsNullOrEmpty(name) && name == "SaveMyStuff")
							{
								URL.ContentSyncURL = newURL;
							}
							else if (!String.IsNullOrEmpty(name) && name == "MultiNotes")
							{
								URL.MultipleNoteURL = newURL;
							}
						}
					}

					return WebExceptionStatus.Success;
				}
				catch (WebException ex)
				{
					Logger.WriteLineDebugging("Server - CheckCompatibility2013: {0}", ex.ToString());
					return ex.Status;
				}
			}

			return WebExceptionStatus.ConnectFailure;
        }

        private static String GenerateURL(String preUrl, String postUrl)
        {
            if (!preUrl.Contains(postUrl))
            {
                if (!preUrl.EndsWith("/"))
                {
                    preUrl += "/";
                }
                preUrl += postUrl;
            }

            if (!preUrl.StartsWith("http"))
            {
                preUrl = "http://" + preUrl;
            }

            return preUrl;
        }
    }
}

