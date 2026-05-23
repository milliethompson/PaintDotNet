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
using System.Text;

namespace PaintDotNet.SystemLayer
{
    internal sealed class VistaFileOpenDialog
        : VistaFileDialog,
          IFileOpenDialog
    {
        private NativeInterfaces.IFileOpenDialog FileOpenDialog
        {
            get
            {
                return this.FileDialog as NativeInterfaces.IFileOpenDialog;
            }
        }

        public bool CheckFileExists
        {
            get
            {
                return GetOptions(NativeConstants.FOS.FOS_FILEMUSTEXIST);
            }

            set
            {
                SetOptions(NativeConstants.FOS.FOS_FILEMUSTEXIST, value);
            }
        }

        public bool Multiselect
        {
            get
            {
                return GetOptions(NativeConstants.FOS.FOS_ALLOWMULTISELECT);
            }

            set
            {
                SetOptions(NativeConstants.FOS.FOS_ALLOWMULTISELECT, value);
            }
        }

        public string[] FileNames
        {
            get
            {
                NativeInterfaces.IShellItemArray array = null;
                this.FileOpenDialog.GetResults(out array);

                uint count;
                array.GetCount(out count);

                string[] fileNames = new string[(int)count];

                for (uint i = 0; i < count; ++i)
                {
                    NativeInterfaces.IShellItem shellItem = null;
                    array.GetItemAt(i, out shellItem);
                    string fileName = GetPathName(shellItem);
                    fileNames[i] = fileName;
                }

                return fileNames;
            }
        }

        public VistaFileOpenDialog()
            : base(new NativeInterfaces.NativeFileOpenDialog())
        {
        }
    }
}
