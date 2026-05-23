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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PaintDotNet.SystemLayer
{
    internal sealed class VistaFileSaveDialog
          : VistaFileDialog,
            IFileSaveDialog
    {
        private bool addExtension = true;

        private NativeInterfaces.IFileSaveDialog FileSaveDialog
        {
            get
            {
                return this.FileDialog as NativeInterfaces.IFileSaveDialog;
            }
        }

        public bool AddExtension
        {
            get
            {
                return this.addExtension;
            }

            set
            {
                this.addExtension = value;
            }
        }

        private string FileNameCore
        {
            get
            {
                NativeInterfaces.IShellItem shellItem = null;
                string path;

                try
                {
                    this.FileSaveDialog.GetResult(out shellItem);
                    path = this.GetPathName(shellItem);
                }

                catch (Exception)
                {
                    this.FileSaveDialog.GetFileName(out path);
                }

                finally
                {
                    if (shellItem != null)
                    {
                        Marshal.ReleaseComObject(shellItem);
                        shellItem = null;
                    }
                }

                return path;
            }
        }

        public string FileName
        {
            get
            {
                string path = FileNameCore;

                if (this.addExtension && path != null)
                {
                    string ext = Path.GetExtension(path);

                    // If they did not specify an extension, then we should add on the default one for the file type they chose
                    if (string.IsNullOrEmpty(ext))
                    {
                        int filterIndex = this.FilterIndex;
                        string allFilters = this.Filter;
                        string[] filtersArray = allFilters.Split('|');
                        string filter = filtersArray[1 + ((filterIndex - 1) * 2)];
                        string[] exts = filter.Split(';');

                        string newSpec = exts[0];
                        if (newSpec[0] == '*')
                        {
                            newSpec = newSpec.Substring(1);
                        }

                        path = Path.ChangeExtension(path, newSpec);
                    }
                }

                return path;
            }

            set
            {
                this.FileSaveDialog.SetFileName(value);
            }
        }

        protected override void OnBeforeShow()
        {
            if (!string.IsNullOrEmpty(FileNameCore))
            {
                string justTheFileName = Path.GetFileName(FileNameCore);
                string dir = Path.GetDirectoryName(FileNameCore);
                string fullPathName = Path.GetFullPath(dir);

                InitialDirectory = fullPathName;
                FileName = justTheFileName;
            }

            base.OnBeforeShow();
        }

        public bool OverwritePrompt
        {
            get
            {
                return GetOptions(NativeConstants.FOS.FOS_OVERWRITEPROMPT);
            }

            set
            {
                SetOptions(NativeConstants.FOS.FOS_OVERWRITEPROMPT, value);
            }
        }

        public VistaFileSaveDialog()
            : base(new NativeInterfaces.NativeFileSaveDialog())
        {
        }
    }
}
