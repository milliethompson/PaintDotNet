/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    public interface IFileDialog
        : IDisposable
    {
        bool CheckPathExists
        {
            get;
            set;
        }

        bool DereferenceLinks 
        { 
            get; 
            set; 
        }

        string Filter 
        { 
            get; 
            set; 
        }

        int FilterIndex 
        { 
            get; 
            set; 
        }

        string InitialDirectory
        {
            get;
            set;
        }

        string Title 
        { 
            set; 
        }

        DialogResult ShowDialog(Control owner);
    }
}
