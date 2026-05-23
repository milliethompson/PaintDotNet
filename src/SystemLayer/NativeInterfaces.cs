/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PaintDotNet.SystemLayer
{
    // Most of this code is from the "Vista Bridge" samples: http://msdn2.microsoft.com/en-us/library/ms756482.aspx

    internal static class NativeInterfaces
    {
        internal class IIDGuid
        {
            // IID GUID strings for relevant COM interfaces
            internal const string IModalWindow = "b4db1657-70d7-485e-8e3e-6fcb5a5c1802";
            internal const string IFileDialog = "42f85136-db7e-439c-85f1-e4075d135fc8";
            internal const string IFileOpenDialog = "d57c7288-d4ad-4768-be02-9d969532d960";
            internal const string IFileSaveDialog = "84bccd23-5fde-4cdb-aea4-af64b83d78ab";
            internal const string IFileDialogEvents = "973510DB-7D7F-452B-8975-74A85828D354";
            internal const string IFileDialogControlEvents = "36116642-D713-4b97-9B83-7484A9D00433";
            internal const string IFileDialogCustomize = "8016b7b3-3d49-4504-a0aa-2a37494e606f";
            internal const string IShellItem = "43826D1E-E718-42EE-BC55-A1E261C37BFE";
            internal const string IShellItemArray = "B63EA76D-1F85-456F-A19C-48159EFA858B";
            internal const string IKnownFolder = "38521333-6A87-46A7-AE10-0F16706816C3";
            internal const string IKnownFolderManager = "44BEAAEC-24F4-4E90-B3F0-23D258FBB146";
            internal const string IPropertyStore = "886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99";
        }

        internal class CLSIDGuid
        {
            // CLSID GUID strings for relevant coclasses
            internal const string FileOpenDialog = "DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7";
            internal const string FileSaveDialog = "C0B4E2F3-BA21-4773-8DBA-335EC946EB8B";
            internal const string KnownFolderManager = "4df0c730-df9d-4ae3-9153-aa6b82e9795a";
        }

        [ComImport()]
        [Guid(IIDGuid.IModalWindow)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IModalWindow
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), PreserveSig]
            int Show([In] IntPtr parent);
        }

        [ComImport()]
        [Guid(IIDGuid.IFileDialog)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IFileDialog : IModalWindow
        {
            // Defined on IModalWindow - repeated here due to requirements of COM interop layer
            // --------------------------------------------------------------------------------
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), PreserveSig]
            new int Show([In] IntPtr parent);

            // IFileDialog-Specific interface members
            // --------------------------------------------------------------------------------
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetFileTypes(
                [In] uint cFileTypes,
                [In] [MarshalAs(UnmanagedType.LPArray)] NativeStructs.COMDLG_FILTERSPEC[] rgFilterSpec);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetFileTypeIndex([In] uint iFileType);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetFileTypeIndex(out uint piFileType);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void Advise([In, MarshalAs(UnmanagedType.Interface)] IFileDialogEvents pfde, out uint pdwCookie);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void Unadvise([In] uint dwCookie);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetOptions([In] NativeConstants.FOS fos);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetOptions(out NativeConstants.FOS pfos);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetDefaultFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetFolder([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetCurrentSelection([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string pszName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszText);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetResult([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void AddPlace([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, NativeConstants.FDAP fdap);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void Close([MarshalAs(UnmanagedType.Error)] int hr);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetClientGuid([In] ref Guid guid);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void ClearClientData();

            // Not supported:  IShellItemFilter is not defined, converting to IntPtr
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);
        }

        [ComImport()]
        [Guid(IIDGuid.IFileOpenDialog)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IFileOpenDialog : IFileDialog
        {
            // Defined on IModalWindow - repeated here due to requirements of COM interop layer
            // --------------------------------------------------------------------------------
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), PreserveSig]
            new int Show([In] IntPtr parent);

            // Defined on IFileDialog - repeated here due to requirements of COM interop layer
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetFileTypes(
                [In] uint cFileTypes,
                [In] [MarshalAs(UnmanagedType.LPArray)] NativeStructs.COMDLG_FILTERSPEC[] rgFilterSpec);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetFileTypeIndex([In] uint iFileType);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void GetFileTypeIndex(out uint piFileType);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void Advise([In, MarshalAs(UnmanagedType.Interface)] IFileDialogEvents pfde, out uint pdwCookie);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void Unadvise([In] uint dwCookie);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetOptions([In] NativeConstants.FOS fos);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void GetOptions(out NativeConstants.FOS pfos);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetDefaultFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void GetFolder([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void GetCurrentSelection([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string pszName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszText);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void GetResult([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void AddPlace([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, NativeConstants.FDAP fdap);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void Close([MarshalAs(UnmanagedType.Error)] int hr);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetClientGuid([In] ref Guid guid);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void ClearClientData();

            // Not supported:  IShellItemFilter is not defined, converting to IntPtr
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);

            // Defined by IFileOpenDialog
            // ---------------------------------------------------------------------------------
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetResults([MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppenum);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetSelectedItems([MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppsai);
        }

        [ComImport()]
        [Guid(IIDGuid.IFileSaveDialog)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IFileSaveDialog : IFileDialog
        {
            // Defined on IModalWindow - repeated here due to requirements of COM interop layer
            // --------------------------------------------------------------------------------
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), PreserveSig]
            new int Show([In] IntPtr parent);

            // Defined on IFileDialog - repeated here due to requirements of COM interop layer
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetFileTypes(
                [In] uint cFileTypes, 
                [In] [MarshalAs(UnmanagedType.LPArray)] NativeStructs.COMDLG_FILTERSPEC[] rgFilterSpec);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetFileTypeIndex([In] uint iFileType);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void GetFileTypeIndex(out uint piFileType);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void Advise([In, MarshalAs(UnmanagedType.Interface)] IFileDialogEvents pfde, out uint pdwCookie);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void Unadvise([In] uint dwCookie);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetOptions([In] NativeConstants.FOS fos);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void GetOptions(out NativeConstants.FOS pfos);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetDefaultFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void GetFolder([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void GetCurrentSelection([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string pszName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszText);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void GetResult([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void AddPlace([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, NativeConstants.FDAP fdap);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void Close([MarshalAs(UnmanagedType.Error)] int hr);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetClientGuid([In] ref Guid guid);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void ClearClientData();

            // Not supported:  IShellItemFilter is not defined, converting to IntPtr
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            new void SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);

            // Defined by IFileSaveDialog interface
            // -----------------------------------------------------------------------------------

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetSaveAsItem([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            // Not currently supported: IPropertyStore
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetProperties([In, MarshalAs(UnmanagedType.Interface)] IntPtr pStore);

            // Not currently supported: IPropertyDescriptionList
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetCollectedProperties([In, MarshalAs(UnmanagedType.Interface)] IntPtr pList, [In] int fAppendDefault);

            // Not currently supported: IPropertyStore
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetProperties([MarshalAs(UnmanagedType.Interface)] out IntPtr ppStore);

            // Not currently supported: IPropertyStore, IFileOperationProgressSink
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void ApplyProperties([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, [In, MarshalAs(UnmanagedType.Interface)] IntPtr pStore, [In, ComAliasName("ShellObjects.wireHWND")] ref IntPtr hwnd, [In, MarshalAs(UnmanagedType.Interface)] IntPtr pSink);
        }

        [ComImport]
        [Guid(IIDGuid.IFileDialogEvents)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IFileDialogEvents
        {
            // NOTE: some of these callbacks are cancelable - returning S_FALSE means that 
            // the dialog should not proceed (e.g. with closing, changing folder); to 
            // support this, we need to use the PreserveSig attribute to enable us to return
            // the proper HRESULT
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), PreserveSig]
            int OnFileOk([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), PreserveSig]
            int OnFolderChanging([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd, [In, MarshalAs(UnmanagedType.Interface)] IShellItem psiFolder);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void OnFolderChange([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void OnSelectionChange([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void OnShareViolation(
                [In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd, 
                [In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, 
                out NativeConstants.FDE_SHAREVIOLATION_RESPONSE pResponse);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void OnTypeChange([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void OnOverwrite(
                [In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd, 
                [In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, 
                out NativeConstants.FDE_OVERWRITE_RESPONSE pResponse);
        }

        [ComImport]
        [Guid(IIDGuid.IShellItem)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellItem
        {
            // Not supported: IBindCtx
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void BindToHandler([In, MarshalAs(UnmanagedType.Interface)] IntPtr pbc, [In] ref Guid bhid, [In] ref Guid riid, out IntPtr ppv);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetParent([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetDisplayName([In] NativeConstants.SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetAttributes([In] uint sfgaoMask, out uint psfgaoAttribs);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void Compare([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, [In] uint hint, out int piOrder);
        }

        [ComImport]
        [Guid(IIDGuid.IShellItemArray)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellItemArray
        {
            // Not supported: IBindCtx
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void BindToHandler([In, MarshalAs(UnmanagedType.Interface)] IntPtr pbc, [In] ref Guid rbhid, [In] ref Guid riid, out IntPtr ppvOut);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetPropertyStore([In] int Flags, [In] ref Guid riid, out IntPtr ppv);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetPropertyDescriptionList([In] ref NativeStructs.PROPERTYKEY keyType, [In] ref Guid riid, out IntPtr ppv);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetAttributes([In] NativeConstants.SIATTRIBFLAGS dwAttribFlags, [In] uint sfgaoMask, out uint psfgaoAttribs);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetCount(out uint pdwNumItems);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetItemAt([In] uint dwIndex, [MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            // Not supported: IEnumShellItems (will use GetCount and GetItemAt instead)
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void EnumItems([MarshalAs(UnmanagedType.Interface)] out IntPtr ppenumShellItems);
        }

        [ComImport]
        [Guid(IIDGuid.IKnownFolder)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IKnownFolder
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetId(out Guid pkfid);

            // Not yet supported - adding to fill slot in vtable
            void spacer1();
            //[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            //void GetCategory(out mbtagKF_CATEGORY pCategory);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetShellItem([In] uint dwFlags, ref Guid riid, out IShellItem ppv);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetPath([In] uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] out string ppszPath);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetPath([In] uint dwFlags, [In, MarshalAs(UnmanagedType.LPWStr)] string pszPath);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetLocation([In] uint dwFlags, [Out, ComAliasName("ShellObjects.wirePIDL")] IntPtr ppidl);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetFolderType(out Guid pftid);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetRedirectionCapabilities(out uint pCapabilities);

            // Not yet supported - adding to fill slot in vtable
            void spacer2();
            //[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            //void GetFolderDefinition(out tagKNOWNFOLDER_DEFINITION pKFD);
        }
        
        [ComImport]
        [Guid(IIDGuid.IKnownFolderManager)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IKnownFolderManager
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void FolderIdFromCsidl([In] int nCsidl, out Guid pfid);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void FolderIdToCsidl([In] ref Guid rfid, out int pnCsidl);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetFolderIds([Out] IntPtr ppKFId, [In, Out] ref uint pCount);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetFolder([In] ref Guid rfid, [MarshalAs(UnmanagedType.Interface)] out IKnownFolder ppkf);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetFolderByName([In, MarshalAs(UnmanagedType.LPWStr)] string pszCanonicalName, [MarshalAs(UnmanagedType.Interface)] out IKnownFolder ppkf);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void RegisterFolder([In] ref Guid rfid, [In] ref NativeStructs.KNOWNFOLDER_DEFINITION pKFD);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void UnregisterFolder([In] ref Guid rfid);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void FindFolderFromPath([In, MarshalAs(UnmanagedType.LPWStr)] string pszPath, [In] NativeConstants.FFFP_MODE mode, [MarshalAs(UnmanagedType.Interface)] out IKnownFolder ppkf);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void FindFolderFromIDList([In] IntPtr pidl, [MarshalAs(UnmanagedType.Interface)] out IKnownFolder ppkf);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void Redirect([In] ref Guid rfid, [In] IntPtr hwnd, [In] uint Flags, [In, MarshalAs(UnmanagedType.LPWStr)] string pszTargetPath, [In] uint cFolders, [In] ref Guid pExclusion, [MarshalAs(UnmanagedType.LPWStr)] out string ppszError);
        }

        [ComImport]
        [Guid(IIDGuid.IFileDialogCustomize)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IFileDialogCustomize
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void EnableOpenDropDown([In] int dwIDCtl);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void AddMenu([In] int dwIDCtl, [In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void AddPushButton([In] int dwIDCtl, [In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void AddComboBox([In] int dwIDCtl);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void AddRadioButtonList([In] int dwIDCtl);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void AddCheckButton([In] int dwIDCtl, [In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel, [In] bool bChecked);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void AddEditBox([In] int dwIDCtl, [In, MarshalAs(UnmanagedType.LPWStr)] string pszText);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void AddSeparator([In] int dwIDCtl);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void AddText([In] int dwIDCtl, [In, MarshalAs(UnmanagedType.LPWStr)] string pszText);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetControlLabel([In] int dwIDCtl, [In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetControlState([In] int dwIDCtl, [Out] out NativeConstants.CDCONTROLSTATE pdwState);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetControlState([In] int dwIDCtl, [In] NativeConstants.CDCONTROLSTATE dwState);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetEditBoxText([In] int dwIDCtl, [Out] IntPtr ppszText);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetEditBoxText([In] int dwIDCtl, [In, MarshalAs(UnmanagedType.LPWStr)] string pszText);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetCheckButtonState([In] int dwIDCtl, [Out] out bool pbChecked);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetCheckButtonState([In] int dwIDCtl, [In] bool bChecked);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void AddControlItem([In] int dwIDCtl, [In] int dwIDItem, [In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void RemoveControlItem([In] int dwIDCtl, [In] int dwIDItem);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void RemoveAllControlItems([In] int dwIDCtl);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetControlItemState([In] int dwIDCtl, [In] int dwIDItem, [Out] out NativeConstants.CDCONTROLSTATE pdwState);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetControlItemState([In] int dwIDCtl, [In] int dwIDItem, [In] NativeConstants.CDCONTROLSTATE dwState);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetSelectedControlItem([In] int dwIDCtl, [Out] out int pdwIDItem);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetSelectedControlItem([In] int dwIDCtl, [In] int dwIDItem); // Not valid for OpenDropDown

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void StartVisualGroup([In] int dwIDCtl, [In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void EndVisualGroup();

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void MakeProminent([In] int dwIDCtl);
        }

        [ComImport]
        [Guid(IIDGuid.IFileDialogControlEvents)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IFileDialogControlEvents
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void OnItemSelected([In, MarshalAs(UnmanagedType.Interface)] IFileDialogCustomize pfdc, [In] int dwIDCtl, [In] int dwIDItem);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void OnButtonClicked([In, MarshalAs(UnmanagedType.Interface)] IFileDialogCustomize pfdc, [In] int dwIDCtl);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void OnCheckButtonToggled([In, MarshalAs(UnmanagedType.Interface)] IFileDialogCustomize pfdc, [In] int dwIDCtl, [In] bool bChecked);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void OnControlActivating([In, MarshalAs(UnmanagedType.Interface)] IFileDialogCustomize pfdc, [In] int dwIDCtl);
        }

        [ComImport]
        [Guid(IIDGuid.IPropertyStore)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IPropertyStore
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetCount([Out] out uint cProps);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetAt([In] uint iProp, out NativeStructs.PROPERTYKEY pkey);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetValue([In] ref NativeStructs.PROPERTYKEY key, out object pv);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetValue([In] ref NativeStructs.PROPERTYKEY key, [In] ref object pv);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void Commit();
        }

        // ---------------------------------------------------------
        // Coclass interfaces - designed to "look like" the object 
        // in the API, so that the 'new' operator can be used in a 
        // straightforward way. Behind the scenes, the C# compiler
        // morphs all 'new CoClass()' calls to 'new CoClassWrapper()'
        [ComImport]
        [Guid(IIDGuid.IFileOpenDialog)]
        [CoClass(typeof(FileOpenDialogRCW))]
        internal interface NativeFileOpenDialog : IFileOpenDialog
        {
        }

        [ComImport]
        [Guid(IIDGuid.IFileSaveDialog)]
        [CoClass(typeof(FileSaveDialogRCW))]
        internal interface NativeFileSaveDialog : IFileSaveDialog
        {
        }

        [ComImport]
        [Guid(IIDGuid.IKnownFolderManager)]
        [CoClass(typeof(KnownFolderManagerRCW))]
        internal interface KnownFolderManager : IKnownFolderManager
        {
        }

        // ---------------------------------------------------
        // .NET classes representing runtime callable wrappers
        [ComImport]
        [ClassInterface(ClassInterfaceType.None)]
        [TypeLibType(TypeLibTypeFlags.FCanCreate)]
        [Guid(CLSIDGuid.FileOpenDialog)]
        internal class FileOpenDialogRCW
        {
        }

        [ComImport]
        [ClassInterface(ClassInterfaceType.None)]
        [TypeLibType(TypeLibTypeFlags.FCanCreate)]
        [Guid(CLSIDGuid.FileSaveDialog)]
        internal class FileSaveDialogRCW
        {
        }

        [ComImport]
        [ClassInterface(ClassInterfaceType.None)]
        [TypeLibType(TypeLibTypeFlags.FCanCreate)]
        [Guid(CLSIDGuid.KnownFolderManager)]
        internal class KnownFolderManagerRCW
        {
        }
    }
}
