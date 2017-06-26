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
using System.Net;
using CoreGraphics;
using Foundation;
using UIKit;

namespace eBriefingMobile
{
    public class Settings
    {
        public static bool Authenticated { get; set; }

        public static bool IsNotFirstTime { get; set; }

        public static bool TutorialClosed { get; set; }

        public static bool SyncOn { get; set; }

        public static String AppVersion { get; set; }

        public static String SortBy { get; set; }

        public static bool SortAscending { get; set; }

        public static String PageMode { get; set; }

        public static String UserID { get; set; }

        public static String Domain { get; set; }

        public static String DefaultPenColor { get; set; }

        public static String DefaultHighlighterColor { get; set; }

        public static DateTime AvailableCheckTime { get; set; }

        public static CookieContainer CookieContainer { get; set; }

        public static bool UseFormsAuth { get; set; }

        public static bool OpenNotePanel { get; set; }

        public static CGPoint MyBooksTabLocation { get; set; }

        public static String CurrentPageID { get; set; }

        public static void Read()
        {
            // Load Settings.bundle and register keys
            RegisterDefaultsFromSettingsBundle();
            
            IsNotFirstTime = NSUserDefaults.StandardUserDefaults.BoolForKey("isNotFirstTime");
            SyncOn = NSUserDefaults.StandardUserDefaults.BoolForKey("syncOn");
            AppVersion = NSUserDefaults.StandardUserDefaults.StringForKey("version");
            PageMode = NSUserDefaults.StandardUserDefaults.StringForKey("pageMode");
            SortBy = NSUserDefaults.StandardUserDefaults.StringForKey("sortBy");
            SortAscending = NSUserDefaults.StandardUserDefaults.BoolForKey("sortAscending");
            TutorialClosed = NSUserDefaults.StandardUserDefaults.BoolForKey("tutorialClosed");
            DefaultPenColor = NSUserDefaults.StandardUserDefaults.StringForKey("defaultPenColor");
            DefaultHighlighterColor = NSUserDefaults.StandardUserDefaults.StringForKey("defaultHighlighterColor");
            UseFormsAuth = NSUserDefaults.StandardUserDefaults.BoolForKey("useFormsAuth");

            // Server URL settings
            URL.ServerURL = NSUserDefaults.StandardUserDefaults.StringForKey("serverURL");
            URL.Core2URL = NSUserDefaults.StandardUserDefaults.StringForKey("core2URL");
            URL.ContentSyncURL = NSUserDefaults.StandardUserDefaults.StringForKey("contentSyncURL");
            URL.MultipleNoteURL = NSUserDefaults.StandardUserDefaults.StringForKey("multipleNoteURL");

            UserID = NSUserDefaults.StandardUserDefaults.StringForKey("userID");
            Domain = NSUserDefaults.StandardUserDefaults.StringForKey("domain");

            if (!IsNotFirstTime)
            {
                WriteIsNotFirstTime(true);
                WritePageMode(StringRef.Single);
                WriteSortBy(StringRef.ByName);
                WriteSortAscending(true);
                WriteDefaultPenColor(StringRef.Red);
                WriteDefaultHighlighterColor(StringRef.Yellow);
                WriteSyncOn(true);
            }
        }

        public static void WriteServerInfo(String id)
        {
            UserID = id;
            WriteSetting("userID", id);

            WriteServerURLs();
        }

        public static void WriteServerInfo(String id, String domain)
        {
            UserID = id;
            WriteSetting("userID", id);

            Domain = domain;
            WriteSetting("domain", domain);

            WriteServerURLs();
        }

        private static void WriteServerURLs()
        {
            String serverURL = URL.ServerURL;
            if (URL.ServerURL == null)
                serverURL = String.Empty;

            String core2URL = URL.Core2URL;
            if (URL.Core2URL == null)
                core2URL = String.Empty;

            String contentSyncURL = URL.ContentSyncURL;
            if (URL.ContentSyncURL == null)
                contentSyncURL = String.Empty;

            String multipleNoteURL = URL.MultipleNoteURL;
            if (URL.MultipleNoteURL == null)
                multipleNoteURL = String.Empty;

            WriteSetting("serverURL", serverURL);
            WriteSetting("core2URL", core2URL);
            WriteSetting("contentSyncURL", contentSyncURL);
            WriteSetting("multipleNoteURL", multipleNoteURL);
        }

        public static void ClearCredential()
        {
            WriteSetting("userID", String.Empty);
            WriteSetting("domain", String.Empty);
            WriteSetting("serverURL", String.Empty);
            WriteSetting("core2URL", String.Empty);
            WriteSetting("contentSyncURL", String.Empty);
            WriteSetting("multipleNoteURL", String.Empty);
        }

        public static void WriteDefaultPenColor(String Value)
        {
            DefaultPenColor = Value;
            WriteSetting("defaultPenColor", Value);
        }

        public static void WriteDefaultHighlighterColor(String Value)
        {
            DefaultHighlighterColor = Value;
            WriteSetting("defaultHighlighterColor", Value);
        }

        public static void WriteIsNotFirstTime(Boolean Value)
        {
            IsNotFirstTime = Value;
            WriteSetting("isNotFirstTime", Value);
        }

        public static void WriteSyncOn(Boolean Value)
        {
            SyncOn = Value;
            WriteSetting("syncOn", Value);
        }

        public static void WritePageMode(String Value)
        {
            PageMode = Value;
            WriteSetting("pageMode", Value);
        }

        public static void WriteSortBy(String Value)
        {
            SortBy = Value;
            WriteSetting("sortBy", Value);
        }

        public static void WriteSortAscending(Boolean Value)
        {
            SortAscending = Value;
            WriteSetting("sortAscending", Value);
        }

        public static void WriteTutorialClosed(Boolean Value)
        {
            TutorialClosed = Value;
            WriteSetting("tutorialClosed", Value);
        }

        public static void WriteUseFormsAuth(Boolean Value)
        {
            UseFormsAuth = Value;
            WriteSetting("useFormsAuth", Value);
        }

        public static void WriteSetting(String name, Boolean Value)
        {
            NSUserDefaults.StandardUserDefaults.SetBool(Value, name);
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }

        public static void WriteSetting(String name, String Value)
        {
            NSUserDefaults.StandardUserDefaults.SetString(Value, name);
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }

        private static void RegisterDefaultsFromSettingsBundle()
        {
            var bundle = new NSDictionary(NSBundle.MainBundle.PathForResource("Settings.bundle/Root.plist", null));
            var preferences = bundle[(NSString)"PreferenceSpecifiers"] as NSArray;
            var defaultsToRegister = new NSMutableDictionary();

            foreach (var prefItem in NSArray.FromArray<NSDictionary> (preferences))
            {
                var key = prefItem[(NSString)"Key"] as NSString;
                if (key == null)
                {
                    continue;
                }

                var val = prefItem[(NSString)"DefaultValue"];
                defaultsToRegister.SetValueForKey(NSObject.FromObject(val.ToString()), key);
            }

            NSUserDefaults.StandardUserDefaults.RegisterDefaults(defaultsToRegister);
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }
    }
}

