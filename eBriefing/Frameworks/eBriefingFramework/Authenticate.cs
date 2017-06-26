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
using System.Collections.Generic;
using Metrostar.Mobile.Framework;
using eBriefing.ebriefingforms.metrostarsystems.com;
using eBriefing.com.metrostarsystems.ebriefingweb2;
using System.Net.Http;
using ModernHttpClient;

namespace eBriefingMobile
{
    public static class Authenticate
    {
		public static WebExceptionStatus NTLM(String url, String id, String password, String domain)
		{
			WebExceptionStatus errorStatus = WebExceptionStatus.UnknownError;

			try
			{
				CredentialCache cache = new CredentialCache();
				cache.Add(new Uri(url), "Ntlm", new NetworkCredential(id, password, domain));

				var credentials = new NetworkCredential(id, password, domain);
				using(var client = new HttpClient(new NativeMessageHandler(false,false,null, credentials)))
				{
					var response = client.GetAsync(URL.Core2URL).Result;
					if (response.StatusCode == HttpStatusCode.OK) {
						errorStatus = WebExceptionStatus.Success;
					}
				}
			}
			catch (WebException ex)
			{
				errorStatus = ex.Status;
				HttpWebResponse error = (HttpWebResponse) ex.Response;
				Console.WriteLine ((int)error.StatusCode);
				Logger.WriteLineDebugging("Authenticate - NTLM: {0}", ex.ToString());
			}

			return errorStatus;
		}
        public static String Forms(String url, String id, String password)
        {
            String error = String.Empty;

            if (!String.IsNullOrEmpty(url))
            {
                try
                {
                    Authentication auth = new Authentication();
                    auth.CookieContainer = new CookieContainer();
                    auth.Url = Server.GenerateFormsAuthenticationURL(url);

                    LoginResult result = auth.Login(id, password);
                    if (result.ErrorCode == LoginErrorCode.NoError)
                    {
                        return error;
                    }
                    else if (result.ErrorCode == LoginErrorCode.PasswordNotMatch)
                    {
                        error = "You can not connect to this site with the given credentials. Check your Username, Password, and Domain or contact your system administrator for assistance.";
                    }
                    else
                    {
                        error = result.ErrorCode.ToString();
                    }
                }
                catch (WebException ex)
                {
                    Logger.WriteLineDebugging("Authenticate - Forms: {0}", ex.ToString());
                }
            }

            return error;
        }

        public static WebExceptionStatus NTLM_Authenticate(String url, String id, String password, String domain)
        {
            WebExceptionStatus errorStatus = WebExceptionStatus.UnknownError;

            try
            {
                errorStatus = NTLM(url, id, password, domain);
                if (errorStatus == WebExceptionStatus.Success)
                {
                    // Remove books if connected to different enterprise library
                    RemoveBooks(url, id);

                    // Save credential to keychain
                    Save2Keychain(url, id, password, domain);
                }
            }
            catch (WebException ex)
            {
                errorStatus = ex.Status;

                Logger.WriteLineDebugging("Authenticate - NTLM_Authenticate: {0}", ex.ToString());
            }

            return errorStatus;
        }

        public static String Forms_Authenticate(String url, String id, String password)
        {
            String error = String.Empty;

            if (!String.IsNullOrEmpty(url))
            {
                try
                {
                    Authentication auth = new Authentication();
                    auth.CookieContainer = new CookieContainer();
                    auth.Url = Server.GenerateFormsAuthenticationURL(url);

                    LoginResult result = auth.Login(id, password);
                    if (result.ErrorCode == LoginErrorCode.NoError)
                    {
                        CookieCollection cc = auth.CookieContainer.GetCookies(new Uri(auth.Url));
                        Cookie authCookie = cc[result.CookieName];

                        CookieContainer container = new CookieContainer();
                        container.Add(authCookie);

                        Settings.CookieContainer = container;

                        // Remove books if connected to different enterprise library
                        RemoveBooks(url, id);

                        // Save credential to keychain
                        Save2Keychain(url, id, password);
                    }
                    else if (result.ErrorCode == LoginErrorCode.PasswordNotMatch)
                    {
                        error = "You can not connect to this site with the given credentials. Check your Username, Password, and Domain or contact your system administrator for assistance.";
                    }
                    else
                    {
                        error = result.ErrorCode.ToString();
                    }
                }
                catch (WebException ex)
                {
                    Logger.WriteLineDebugging("Authenticate - Forms_Authenticate: {0}", ex.ToString());
                }
            }

            return error;
        }

        public static CookieContainer GetCookieContainer()
        {
            if (Settings.CookieContainer == null)
            {
                Forms_Authenticate(URL.ServerURL, Settings.UserID, KeychainAccessor.Password);

                return Settings.CookieContainer;
            }
            else
            {
                String url = Server.GenerateFormsAuthenticationURL(URL.ServerURL);
                CookieCollection cc = Settings.CookieContainer.GetCookies(new Uri(url));
                Cookie authCookie = cc[0];

                if (authCookie.Expired)
                {
                    Forms_Authenticate(URL.ServerURL, Settings.UserID, KeychainAccessor.Password);
                }

                return Settings.CookieContainer;
            }
        }

        private static void Save2Keychain(String url, String id, String password)
        {
            URL.ServerURL = url;

            // Save server info to Settings
            Settings.WriteServerInfo(id);

            KeychainAccessor.RemoveCredential();

            // Save password info to keychain
            KeychainAccessor.SaveCredential(password);
        }

        private static void Save2Keychain(String url, String id, String password, String domain)
        {
            URL.ServerURL = url;

            // Save server info to Settings
            Settings.WriteServerInfo(id, domain);

            KeychainAccessor.RemoveCredential();

            // Save password info to keychain
            KeychainAccessor.SaveCredential(password);
        }

        private static void RemoveBooks(String url, String id)
        {
            if (!String.IsNullOrEmpty(URL.ServerURL))
            {
                if (url != URL.ServerURL || id != Settings.UserID)
                {
                    List<Book> bookList = BooksOnDeviceAccessor.GetBooks();
                    if (bookList != null)
                    {
                        BookRemover.RemoveBooks(bookList);
                    }
                }
            }
        }
    }
}

