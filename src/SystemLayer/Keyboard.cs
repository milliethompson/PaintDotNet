/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
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
                int kbSpeed;
                NativeMethods.SystemParametersInfo(NativeConstants.SPI_GETKEYBOARDSPEED, 0, &kbSpeed, 0);
                return kbSpeed;
            }
        }
	}
}
