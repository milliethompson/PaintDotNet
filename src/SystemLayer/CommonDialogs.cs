/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

//#define NOVISTA

using System;
using System.Collections.Generic;
using System.Text;

namespace PaintDotNet.SystemLayer
{
    public static class CommonDialogs
    {
        public static IFileOpenDialog CreateFileOpenDialog()
        {
#if !NOVISTA
            if (OS.IsVistaOrLater)
            {
                return new VistaFileOpenDialog();
            }
            else
#endif
            {
                return new ClassicFileOpenDialog();
            }
        }

        public static IFileSaveDialog CreateFileSaveDialog()
        {
#if !NOVISTA
            if (OS.IsVistaOrLater)
            {
                return new VistaFileSaveDialog();
            }
            else
#endif
            {
                return new ClassicFileSaveDialog();
            }
        }
    }
}
