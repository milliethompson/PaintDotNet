/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    public interface IFileDialogUICallbacks
    {
        FileOverwriteAction ShowOverwritePrompt(IWin32Window owner, string pathName);
        IFileTransferProgressEvents CreateFileTransferProgressEvents();
    }
}
