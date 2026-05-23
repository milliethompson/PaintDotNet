/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System;

namespace PaintDotNet.Data.TgaFileType
{
    /// <summary>
    /// Summary description for TgaFileTypeFactory.
    /// </summary>
    public sealed class TgaFileTypeFactory
        : IFileTypeFactory
    {
        public TgaFileTypeFactory()
        {
        }

        public FileType[] GetFileTypeInstances()
        {
            return new FileType[] { new TgaFileType() };
        }
    }
}
