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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    internal abstract class VistaFileDialog
        : IFileDialog
    {
        private DialogResult dialogResult = DialogResult.None;
        private NativeInterfaces.IFileDialog fileDialog;
        private string initialDirectory = null;
        private string filter = null;

        protected NativeInterfaces.IFileDialog FileDialog
        {
            get
            {
                return this.fileDialog;
            }
        }

        protected void SetOptions(NativeConstants.FOS flags, bool enable)
        {
            if (enable)
            {
                EnableOption(flags);
            }
            else
            {
                DisableOption(flags);
            }
        }

        protected void EnableOption(NativeConstants.FOS flags)
        {
            NativeConstants.FOS oldOptions;
            this.fileDialog.GetOptions(out oldOptions);
            NativeConstants.FOS newOptions = oldOptions | flags;
            this.fileDialog.SetOptions(newOptions);
        }

        protected void DisableOption(NativeConstants.FOS flags)
        {
            NativeConstants.FOS oldOptions;
            this.fileDialog.GetOptions(out oldOptions);
            NativeConstants.FOS newOptions = oldOptions & ~flags;
            this.fileDialog.SetOptions(newOptions);
        }

        protected bool GetOptions(NativeConstants.FOS flags)
        {
            NativeConstants.FOS options;
            this.fileDialog.GetOptions(out options);
            NativeConstants.FOS masked = options & flags;
            return masked == flags;
        }

        protected NativeInterfaces.IShellItem GetShellItem(string path)
        {
            Guid IID_IShellItem = new Guid(NativeInterfaces.IIDGuid.IShellItem);
            IntPtr ppv = IntPtr.Zero;
            NativeMethods.SHCreateItemFromParsingName(path, IntPtr.Zero, ref IID_IShellItem, out ppv);
            object iUnknown = Marshal.GetObjectForIUnknown(ppv);
            NativeInterfaces.IShellItem shellItem = (NativeInterfaces.IShellItem)iUnknown;
            return shellItem;
        }

        protected string GetPathName(NativeInterfaces.IShellItem shellItem)
        {
            string pathName;
            shellItem.GetDisplayName(NativeConstants.SIGDN.SIGDN_FILESYSPATH, out pathName);
            return pathName;
        }            

        public bool CheckPathExists
        {
            get
            {
                return GetOptions(NativeConstants.FOS.FOS_PATHMUSTEXIST);
            }

            set
            {
                SetOptions(NativeConstants.FOS.FOS_PATHMUSTEXIST, value);
            }
        }

        public bool DereferenceLinks
        {
            get
            {
                return !this.GetOptions(NativeConstants.FOS.FOS_NODEREFERENCELINKS);
            }

            set
            {
                SetOptions(NativeConstants.FOS.FOS_NODEREFERENCELINKS, !value);
            }
        }

        public string Filter
        {
            get
            {
                return this.filter;
            }

            set
            {
                string[] split = value.Split('|');

                if ((split.Length % 2) != 0)
                {
                    throw new ArgumentException();
                }

                NativeStructs.COMDLG_FILTERSPEC[] filterSpecs = new NativeStructs.COMDLG_FILTERSPEC[split.Length / 2];

                for (int i = 0; i < filterSpecs.Length; ++i)
                {
                    NativeStructs.COMDLG_FILTERSPEC filterSpec = new NativeStructs.COMDLG_FILTERSPEC();
                    filterSpec.pszName = split[i * 2];
                    filterSpec.pszSpec = split[(i * 2) + 1];
                    filterSpecs[i] = filterSpec;
                }

                this.FileDialog.SetFileTypes((uint)filterSpecs.Length, filterSpecs);
                this.filter = value;
            }
        }

        public int FilterIndex
        {
            get
            {
                uint index = 0;
                this.FileDialog.GetFileTypeIndex(out index);
                return (int)index;
            }

            set
            {
                this.FileDialog.SetFileTypeIndex((uint)value);
            }
        }

        public string InitialDirectory
        {
            get
            {
                return this.initialDirectory;
            }

            set
            {
                this.initialDirectory = value;
            }
        }

        public string Title
        {
            set
            {
                this.fileDialog.SetTitle(value);
            }
        }

        protected virtual void OnBeforeShow()
        {
            NativeInterfaces.IShellItem shellItem = null;

            try
            {
                shellItem = GetShellItem(this.initialDirectory);
                this.fileDialog.SetDefaultFolder(shellItem);
            }

            finally
            {
                if (shellItem != null)
                {
                    Marshal.ReleaseComObject(shellItem);
                    shellItem = null;
                }
            }
        }

        public DialogResult ShowDialog(Control owner)
        {
            IntPtr parent;

            if (owner == null)
            {
                parent = IntPtr.Zero;
            }
            else
            {
                parent = owner.Handle;
            }

            try
            {
                this.fileDialog.ClearClientData();
            }

            catch (Exception)
            {
            }

            OnBeforeShow();

            int hr = this.fileDialog.Show(parent);
            DialogResult result;

            if (hr >= 0)
            {
                result = DialogResult.OK;
            }
            else
            {
                result = DialogResult.Cancel;
            }

            this.dialogResult = result;

            GC.KeepAlive(owner);
            return result;
        }

        public VistaFileDialog(NativeInterfaces.IFileDialog fileDialog)
        {
            this.fileDialog = fileDialog;
        }

        ~VistaFileDialog()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
