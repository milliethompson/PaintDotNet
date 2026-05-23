/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
	/// <summary>
	/// Encapsulates information and events about the current user session.
	/// This relates to Terminal Services in Windows.
	/// </summary>
	public sealed class UserSessions
	{
        private static OurControl messageControl;
        private static System.Windows.Forms.Timer win2kTsDetectionTimer; // this timer is used in Win2K so we can poll every few seconds to see if we are running in a remote session
        private static bool lastRemoteSessionValue;
        private static EventHandler sessionChanged;
        private static int sessionChangedCount;
        private static object lockObject = new object();

        private UserSessions()
        {
        }

        private sealed class OurControl
            : Control
        {
            public event EventHandler WmWtSessionChange;

            private void OnWmWtSessionChange()
            {
                if (WmWtSessionChange != null)
                {
                    WmWtSessionChange(this, EventArgs.Empty);
                }
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case NativeConstants.WM_WTSSESSION_CHANGE:
                        OnWmWtSessionChange();
                        break;

                    default:
                        base.WndProc(ref m);
                        break;
                }
            }
        }

        private static void OnSessionChanged()
        {
            if (sessionChanged != null)
            {
                sessionChanged(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Occurs when the user changes between sessions. This event will only be
        /// raised when the value returned by IsRemote() changes.
        /// </summary>
        /// <remarks>
        /// For example, if the user is currently logged in at the console, and then
        /// switches to a remote session (they use Remote Desktop from another computer),
        /// then this event will be raised.
        /// </remarks>
        public static event EventHandler SessionChanged
        {
            add
            {
                lock (lockObject)
                {
                    sessionChanged += value;
                    ++sessionChangedCount;

                    if (sessionChangedCount == 1)
                    {
                        messageControl = new OurControl();
                        messageControl.CreateControl(); // force the HWND to be created
                        messageControl.WmWtSessionChange += new EventHandler(SessionStrobeHandler);

                        // Our preferred way of detection remote<->console session transitions is via a window message.
                        // But if that can't be done, we'll settle for a timer that pulses every 5 seconds.
                        // It's called the Win2K Timer because generally this code path is only required on Win2K.
                        try
                        {
                            SafeNativeMethods.WTSRegisterSessionNotification(messageControl.Handle, NativeConstants.NOTIFY_FOR_ALL_SESSIONS);
                        }

                        catch (EntryPointNotFoundException)
                        {
                            messageControl.WmWtSessionChange -= new EventHandler(SessionStrobeHandler);
                            messageControl.Dispose();
                            messageControl = null;

                            win2kTsDetectionTimer = new System.Windows.Forms.Timer();
                            win2kTsDetectionTimer.Interval = 5000;
                            win2kTsDetectionTimer.Tick += new System.EventHandler(SessionStrobeHandler);
                            win2kTsDetectionTimer.Enabled = true;
                        }

                        lastRemoteSessionValue = IsRemote();
                    }
                }
            }

            remove
            {
                lock (lockObject)
                {
                    sessionChanged -= value;
                    int decremented = Interlocked.Decrement(ref sessionChangedCount);

                    if (decremented == 0)
                    {
                        if (win2kTsDetectionTimer != null)
                        {
                            win2kTsDetectionTimer.Tick -= new System.EventHandler(SessionStrobeHandler);
                            win2kTsDetectionTimer.Enabled = false;
                            win2kTsDetectionTimer.Dispose();
                            win2kTsDetectionTimer = null;
                        }
                        else
                        {
                            try
                            {
                                SafeNativeMethods.WTSUnRegisterSessionNotification(messageControl.Handle);
                            }

                            catch (EntryPointNotFoundException)
                            {
                            }

                            messageControl.Dispose();
                            messageControl = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the user is running within a remoted session (Terminal Server, Remote Desktop).
        /// </summary>
        /// <returns>
        /// <b>true</b> if we're running in a remote session, <b>false</b> otherwise.
        /// </returns>
        /// <remarks>
        /// You can use this to optimize the presentation of visual elements. Remote sessions
        /// are often bandwidth limited and less suitable for complex drawing.
        /// </remarks>
        public static bool IsRemote()
        {
            return 0 != SafeNativeMethods.GetSystemMetrics(NativeConstants.SM_REMOTESESSION);
        }

        private static void SessionStrobeHandler(object sender, EventArgs e)
        {
            if (IsRemote() != lastRemoteSessionValue)
            {
                lastRemoteSessionValue = IsRemote();
                OnSessionChanged();
            }
        }
    }
}
