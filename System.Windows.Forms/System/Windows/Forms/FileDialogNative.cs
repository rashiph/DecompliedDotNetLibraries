namespace System.Windows.Forms
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class FileDialogNative
    {
        internal class CLSIDGuid
        {
            internal const string FileOpenDialog = "DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7";
            internal const string FileSaveDialog = "C0B4E2F3-BA21-4773-8DBA-335EC946EB8B";

            private CLSIDGuid()
            {
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto, Pack=4)]
        internal struct COMDLG_FILTERSPEC
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pszName;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pszSpec;
        }

        internal enum FDE_OVERWRITE_RESPONSE
        {
            FDEOR_DEFAULT,
            FDEOR_ACCEPT,
            FDEOR_REFUSE
        }

        internal enum FDE_SHAREVIOLATION_RESPONSE
        {
            FDESVR_DEFAULT,
            FDESVR_ACCEPT,
            FDESVR_REFUSE
        }

        [ComImport, TypeLibType(TypeLibTypeFlags.FCanCreate), Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7"), ClassInterface(ClassInterfaceType.None)]
        internal class FileOpenDialogRCW
        {
        }

        [ComImport, ClassInterface(ClassInterfaceType.None), TypeLibType(TypeLibTypeFlags.FCanCreate), Guid("C0B4E2F3-BA21-4773-8DBA-335EC946EB8B")]
        internal class FileSaveDialogRCW
        {
        }

        [Flags]
        internal enum FOS : uint
        {
            FOS_ALLNONSTORAGEITEMS = 0x80,
            FOS_ALLOWMULTISELECT = 0x200,
            FOS_CREATEPROMPT = 0x2000,
            FOS_DEFAULTNOMINIMODE = 0x20000000,
            FOS_DONTADDTORECENT = 0x2000000,
            FOS_FILEMUSTEXIST = 0x1000,
            FOS_FORCEFILESYSTEM = 0x40,
            FOS_FORCESHOWHIDDEN = 0x10000000,
            FOS_HIDEMRUPLACES = 0x20000,
            FOS_HIDEPINNEDPLACES = 0x40000,
            FOS_NOCHANGEDIR = 8,
            FOS_NODEREFERENCELINKS = 0x100000,
            FOS_NOREADONLYRETURN = 0x8000,
            FOS_NOTESTFILECREATE = 0x10000,
            FOS_NOVALIDATE = 0x100,
            FOS_OVERWRITEPROMPT = 2,
            FOS_PATHMUSTEXIST = 0x800,
            FOS_PICKFOLDERS = 0x20,
            FOS_SHAREAWARE = 0x4000,
            FOS_STRICTFILETYPES = 4
        }

        [ComImport, Guid("42f85136-db7e-439c-85f1-e4075d135fc8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IFileDialog
        {
            [PreserveSig]
            int Show([In] IntPtr parent);
            void SetFileTypes([In] uint cFileTypes, [In, MarshalAs(UnmanagedType.LPArray)] FileDialogNative.COMDLG_FILTERSPEC[] rgFilterSpec);
            void SetFileTypeIndex([In] uint iFileType);
            void GetFileTypeIndex(out uint piFileType);
            void Advise([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IFileDialogEvents pfde, out uint pdwCookie);
            void Unadvise([In] uint dwCookie);
            void SetOptions([In] FileDialogNative.FOS fos);
            void GetOptions(out FileDialogNative.FOS pfos);
            void SetDefaultFolder([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IShellItem psi);
            void SetFolder([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IShellItem psi);
            void GetFolder([MarshalAs(UnmanagedType.Interface)] out FileDialogNative.IShellItem ppsi);
            void GetCurrentSelection([MarshalAs(UnmanagedType.Interface)] out FileDialogNative.IShellItem ppsi);
            void SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszText);
            void SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            void GetResult([MarshalAs(UnmanagedType.Interface)] out FileDialogNative.IShellItem ppsi);
            void AddPlace([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IShellItem psi, int alignment);
            void SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            void Close([MarshalAs(UnmanagedType.Error)] int hr);
            void SetClientGuid([In] ref Guid guid);
            void ClearClientData();
            void SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("973510DB-7D7F-452B-8975-74A85828D354")]
        internal interface IFileDialogEvents
        {
            [PreserveSig]
            int OnFileOk([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IFileDialog pfd);
            [PreserveSig]
            int OnFolderChanging([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IFileDialog pfd, [In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IShellItem psiFolder);
            void OnFolderChange([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IFileDialog pfd);
            void OnSelectionChange([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IFileDialog pfd);
            void OnShareViolation([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IFileDialog pfd, [In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IShellItem psi, out FileDialogNative.FDE_SHAREVIOLATION_RESPONSE pResponse);
            void OnTypeChange([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IFileDialog pfd);
            void OnOverwrite([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IFileDialog pfd, [In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IShellItem psi, out FileDialogNative.FDE_OVERWRITE_RESPONSE pResponse);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("d57c7288-d4ad-4768-be02-9d969532d960")]
        internal interface IFileOpenDialog : FileDialogNative.IFileDialog
        {
            [PreserveSig]
            int Show([In] IntPtr parent);
            void SetFileTypes([In] uint cFileTypes, [In] ref FileDialogNative.COMDLG_FILTERSPEC rgFilterSpec);
            void SetFileTypeIndex([In] uint iFileType);
            void GetFileTypeIndex(out uint piFileType);
            void Advise([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IFileDialogEvents pfde, out uint pdwCookie);
            void Unadvise([In] uint dwCookie);
            void SetOptions([In] FileDialogNative.FOS fos);
            void GetOptions(out FileDialogNative.FOS pfos);
            void SetDefaultFolder([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IShellItem psi);
            void SetFolder([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IShellItem psi);
            void GetFolder([MarshalAs(UnmanagedType.Interface)] out FileDialogNative.IShellItem ppsi);
            void GetCurrentSelection([MarshalAs(UnmanagedType.Interface)] out FileDialogNative.IShellItem ppsi);
            void SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszText);
            void SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            void GetResult([MarshalAs(UnmanagedType.Interface)] out FileDialogNative.IShellItem ppsi);
            void AddPlace([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IShellItem psi, FileDialogCustomPlace fdcp);
            void SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            void Close([MarshalAs(UnmanagedType.Error)] int hr);
            void SetClientGuid([In] ref Guid guid);
            void ClearClientData();
            void SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);
            void GetResults([MarshalAs(UnmanagedType.Interface)] out FileDialogNative.IShellItemArray ppenum);
            void GetSelectedItems([MarshalAs(UnmanagedType.Interface)] out FileDialogNative.IShellItemArray ppsai);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("84bccd23-5fde-4cdb-aea4-af64b83d78ab")]
        internal interface IFileSaveDialog : FileDialogNative.IFileDialog
        {
            [PreserveSig]
            int Show([In] IntPtr parent);
            void SetFileTypes([In] uint cFileTypes, [In] ref FileDialogNative.COMDLG_FILTERSPEC rgFilterSpec);
            void SetFileTypeIndex([In] uint iFileType);
            void GetFileTypeIndex(out uint piFileType);
            void Advise([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IFileDialogEvents pfde, out uint pdwCookie);
            void Unadvise([In] uint dwCookie);
            void SetOptions([In] FileDialogNative.FOS fos);
            void GetOptions(out FileDialogNative.FOS pfos);
            void SetDefaultFolder([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IShellItem psi);
            void SetFolder([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IShellItem psi);
            void GetFolder([MarshalAs(UnmanagedType.Interface)] out FileDialogNative.IShellItem ppsi);
            void GetCurrentSelection([MarshalAs(UnmanagedType.Interface)] out FileDialogNative.IShellItem ppsi);
            void SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszText);
            void SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            void GetResult([MarshalAs(UnmanagedType.Interface)] out FileDialogNative.IShellItem ppsi);
            void AddPlace([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IShellItem psi, FileDialogCustomPlace fdcp);
            void SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            void Close([MarshalAs(UnmanagedType.Error)] int hr);
            void SetClientGuid([In] ref Guid guid);
            void ClearClientData();
            void SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);
            void SetSaveAsItem([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IShellItem psi);
            void SetProperties([In, MarshalAs(UnmanagedType.Interface)] IntPtr pStore);
            void SetCollectedProperties([In, MarshalAs(UnmanagedType.Interface)] IntPtr pList, [In] int fAppendDefault);
            void GetProperties([MarshalAs(UnmanagedType.Interface)] out IntPtr ppStore);
            void ApplyProperties([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IShellItem psi, [In, MarshalAs(UnmanagedType.Interface)] IntPtr pStore, [In, ComAliasName("ShellObjects.wireHWND")] ref IntPtr hwnd, [In, MarshalAs(UnmanagedType.Interface)] IntPtr pSink);
        }

        internal class IIDGuid
        {
            internal const string IFileDialog = "42f85136-db7e-439c-85f1-e4075d135fc8";
            internal const string IFileDialogEvents = "973510DB-7D7F-452B-8975-74A85828D354";
            internal const string IFileOpenDialog = "d57c7288-d4ad-4768-be02-9d969532d960";
            internal const string IFileSaveDialog = "84bccd23-5fde-4cdb-aea4-af64b83d78ab";
            internal const string IModalWindow = "b4db1657-70d7-485e-8e3e-6fcb5a5c1802";
            internal const string IShellItem = "43826D1E-E718-42EE-BC55-A1E261C37BFE";
            internal const string IShellItemArray = "B63EA76D-1F85-456F-A19C-48159EFA858B";

            private IIDGuid()
            {
            }
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("b4db1657-70d7-485e-8e3e-6fcb5a5c1802")]
        internal interface IModalWindow
        {
            [PreserveSig]
            int Show([In] IntPtr parent);
        }

        [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellItem
        {
            void BindToHandler([In, MarshalAs(UnmanagedType.Interface)] IntPtr pbc, [In] ref Guid bhid, [In] ref Guid riid, out IntPtr ppv);
            void GetParent([MarshalAs(UnmanagedType.Interface)] out FileDialogNative.IShellItem ppsi);
            void GetDisplayName([In] FileDialogNative.SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
            void GetAttributes([In] uint sfgaoMask, out uint psfgaoAttribs);
            void Compare([In, MarshalAs(UnmanagedType.Interface)] FileDialogNative.IShellItem psi, [In] uint hint, out int piOrder);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("B63EA76D-1F85-456F-A19C-48159EFA858B")]
        internal interface IShellItemArray
        {
            void BindToHandler([In, MarshalAs(UnmanagedType.Interface)] IntPtr pbc, [In] ref Guid rbhid, [In] ref Guid riid, out IntPtr ppvOut);
            void GetPropertyStore([In] int Flags, [In] ref Guid riid, out IntPtr ppv);
            void GetPropertyDescriptionList([In] ref FileDialogNative.PROPERTYKEY keyType, [In] ref Guid riid, out IntPtr ppv);
            void GetAttributes([In] FileDialogNative.SIATTRIBFLAGS dwAttribFlags, [In] uint sfgaoMask, out uint psfgaoAttribs);
            void GetCount(out uint pdwNumItems);
            void GetItemAt([In] uint dwIndex, [MarshalAs(UnmanagedType.Interface)] out FileDialogNative.IShellItem ppsi);
            void EnumItems([MarshalAs(UnmanagedType.Interface)] out IntPtr ppenumShellItems);
        }

        [ComImport, Guid("d57c7288-d4ad-4768-be02-9d969532d960"), CoClass(typeof(FileDialogNative.FileOpenDialogRCW))]
        internal interface NativeFileOpenDialog : FileDialogNative.IFileOpenDialog, FileDialogNative.IFileDialog
        {
        }

        [ComImport, CoClass(typeof(FileDialogNative.FileSaveDialogRCW)), Guid("84bccd23-5fde-4cdb-aea4-af64b83d78ab")]
        internal interface NativeFileSaveDialog : FileDialogNative.IFileSaveDialog, FileDialogNative.IFileDialog
        {
        }

        [StructLayout(LayoutKind.Sequential, Pack=4)]
        internal struct PROPERTYKEY
        {
            internal Guid fmtid;
            internal uint pid;
        }

        internal enum SIATTRIBFLAGS
        {
            SIATTRIBFLAGS_AND = 1,
            SIATTRIBFLAGS_APPCOMPAT = 3,
            SIATTRIBFLAGS_OR = 2
        }

        internal enum SIGDN : uint
        {
            SIGDN_DESKTOPABSOLUTEEDITING = 0x8004c000,
            SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,
            SIGDN_FILESYSPATH = 0x80058000,
            SIGDN_NORMALDISPLAY = 0,
            SIGDN_PARENTRELATIVE = 0x80080001,
            SIGDN_PARENTRELATIVEEDITING = 0x80031001,
            SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
            SIGDN_PARENTRELATIVEPARSING = 0x80018001,
            SIGDN_URL = 0x80068000
        }
    }
}

