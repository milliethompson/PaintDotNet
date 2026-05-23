/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// This class contains static methods related to the keyboard.
    /// </summary>
    public sealed class Keyboard
    {
        private Keyboard()
        {
        }

        /// <summary>
        /// Gets the time, in milliseconds, before keyboard input starts automatically repeating.
        /// </summary>
        public static int GetRepeatDelay()
        {
            unsafe
            {
                int kbDelay;
                NativeMethods.SystemParametersInfo(NativeConstants.SPI_GETKEYBOARDDELAY, 0, &kbDelay, 0);
                kbDelay = (1 + kbDelay) * 250;
                return kbDelay;
            }
        }

        /// <summary>
        /// Gets the duration of time, in milliseconds, that keyboard input automatically repeats at.
        /// </summary>
        public static int GetRepeatSpeed()
        {
            unsafe
            {
                // "Retrieves the keyboard repeat-speed setting, which is a value in the range from 0 (approximately 
                // 2.5 repetitions per second) through 31 (approximately 30 repetitions per second). The actual 
                // repeat rates are hardware-dependent and may vary from a linear scale by as much as 20%. The 
                // pvParam parameter must point to a DWORD variable that receives the setting.
                                                                                                                                                                                                                                                                                                                                                                        //
                int kbSpeed;
                NativeMethods.SystemParametersInfo(NativeConstants.SPI_GETKEYBOARDSPEED, 0, &kbSpeed, 0);

                const float slope = 27.5f / 31.0f;
                float hz = 2.5f + (slope * (float)kbSpeed);
                float period = 1000.0f / hz;

                return (int)Math.Round(period);
            }
        }
    }
}
