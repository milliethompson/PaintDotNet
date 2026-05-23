/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    /// <summary>
    /// This exception is thrown by a foreground thread when a background worker thread
    /// had an exception. This allows all exceptions to be handled by the foreground thread.
    /// </summary>
    public class WorkerThreadException
        : PdnException
    {
        public WorkerThreadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
