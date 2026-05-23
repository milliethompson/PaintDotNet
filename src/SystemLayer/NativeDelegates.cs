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
    internal sealed class NativeDelegates
    {
        internal delegate void OVERLAPPED_COMPLETION_ROUTINE(
            uint dwErrorCode,
            uint dwNumberOfBytesTransferred,
            ref NativeStructs.OVERLAPPED lpOverlapped
            );

        private NativeDelegates()
        {
        }
    }
}
