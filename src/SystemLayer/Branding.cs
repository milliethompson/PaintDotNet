/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Provides methods that control branding aspects of Paint.NET. For instance,
    /// the URL we ping for update manifests, and the e-mail address to send
    /// feedback to.
    /// </summary>
    // TODO: remove
    public static class Branding
    {
        [Obsolete("Use InvariantStrings.FeedbackEmail instead")]
        public const string FeedbackEmail = "paint.net@hotmail.com";

        [Obsolete("Use InvariantStrings.WebsiteUrl instead")]
        public const string WebsiteUrl = "http://www.getpaint.net";
    }
}