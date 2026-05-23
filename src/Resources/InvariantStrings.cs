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
    /// Contains strings that must be the same no matter what locale the UI is running with.
    /// </summary>
    public sealed class InvariantStrings
    {
        // {0} is "All Rights Reserved"
        public const string CopyrightFormat = 
            "Copyright © 2005 Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, and Luke Walker. Portions Copyright © 2005 Microsoft Corporation. {0}";

        public const string FeedbackEmail = "paint.net@hotmail.com";

        private InvariantStrings()
        {
        }
    }
}
