using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MssFramework;

namespace eBriefingMobile
{
    public static class SyncView
    {
        private static NSTimer timer = null;
        public static bool PushNPullInProgress;

        public delegate void ReceiveDoneDelegate();

        public delegate void SyncDoneDelegate();

        public static event ReceiveDoneDelegate ReceiveDoneEvent;
        public static event SyncDoneDelegate SyncDoneEvent;

        public static void Show(UIView view, CloudSync.SyncType syncType)
        {
            if (Reachability.IsDefaultNetworkAvailable())
            {
                LoadingView.CancelEvent += HandleCancelEvent;
                LoadingView.Show("Syncing", "Please wait while" + '\n' + "eBriefing is syncing." + '\n' + "This may take a few minutes...");

                // Pull sync
                if (syncType == CloudSync.SyncType.PULL)
                {
                    CloudSync.ReceiveDoneEvent += HandleReceiveDoneEvent;

                    CloudSync.Pull();
                }

                // Push and Pull sync
                else if (syncType == CloudSync.SyncType.PUSH_AND_PULL)
                {
                    PushNPullInProgress = true;
                    CloudSync.SyncDoneEvent += HandleSyncDoneEvent;

                    CloudSync.PushAndPull();
                }
            }
            else
            {
                StartTimer();
            }
        }

        #region Timer

        private static void StartTimer()
        {
            if (timer != null)
            {
                timer.Invalidate();
                timer.Dispose();
                timer = null;
            }

            timer = NSTimer.CreateRepeatingScheduledTimer(TimeSettings.SyncInterval, Sync);
        }

        private static void Sync()
        {
            CloudSync.Push();
        }

        #endregion

        #region Sync Event

        static void HandleReceiveDoneEvent()
        {
            StartTimer();

            CloudSync.ReceiveDoneEvent -= HandleReceiveDoneEvent;

            LoadingView.Hide();

            if (ReceiveDoneEvent != null)
            {
                ReceiveDoneEvent();
            }
        }

        static void HandleSyncDoneEvent()
        {
            StartTimer();

            CloudSync.SyncDoneEvent -= HandleSyncDoneEvent;

            PushNPullInProgress = false;

            LoadingView.Hide();

            if (SyncDoneEvent != null)
            {
                SyncDoneEvent();
            }
        }

        static void HandleCancelEvent()
        {
            LoadingView.CancelEvent -= HandleCancelEvent;

            LoadingView.Hide();

            CloudSync.CancelSync();
        }

        #endregion
    }
}