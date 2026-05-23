/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.Data;
using System;

namespace PaintDotNet.Data
{
    public sealed class HDPhotoFileTypeFactory
        : IFileTypeFactory
    {
        public FileType[] GetFileTypeInstances()
        {
            // We expect to have this built-in to Paint.NET soon
            if (DateTime.Now < new DateTime(2007, 7, 1))
            {
                return new FileType[] { new HDPhotoFileType() };
            }
            else
            {
                return new FileType[0];
            }
        }
    }
}