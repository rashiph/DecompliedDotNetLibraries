namespace System.Net.Mail
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;

    [ComImport, Guid("70b51430-b6ca-11d0-b9b9-00a0c922e750"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMSAdminBase
    {
        [PreserveSig]
        int AddKey(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string Path);
        [PreserveSig]
        int DeleteKey(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string Path);
        void DeleteChildKeys(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string Path);
        [PreserveSig]
        int EnumKeys(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string Path, StringBuilder Buffer, int EnumKeyIndex);
        void CopyKey(IntPtr source, [MarshalAs(UnmanagedType.LPWStr)] string SourcePath, IntPtr dest, [MarshalAs(UnmanagedType.LPWStr)] string DestPath, bool OverwriteFlag, bool CopyFlag);
        void RenameKey(IntPtr key, [MarshalAs(UnmanagedType.LPWStr)] string path, [MarshalAs(UnmanagedType.LPWStr)] string newName);
        [PreserveSig]
        int SetData(IntPtr key, [MarshalAs(UnmanagedType.LPWStr)] string path, ref MetadataRecord data);
        [PreserveSig]
        int GetData(IntPtr key, [MarshalAs(UnmanagedType.LPWStr)] string path, ref MetadataRecord data, [In, Out] ref uint RequiredDataLen);
        [PreserveSig]
        int DeleteData(IntPtr key, [MarshalAs(UnmanagedType.LPWStr)] string path, uint Identifier, uint DataType);
        [PreserveSig]
        int EnumData(IntPtr key, [MarshalAs(UnmanagedType.LPWStr)] string path, ref MetadataRecord data, int EnumDataIndex, [In, Out] ref uint RequiredDataLen);
        [PreserveSig]
        int GetAllData(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string Path, uint Attributes, uint UserType, uint DataType, [In, Out] ref uint NumDataEntries, [In, Out] ref uint DataSetNumber, uint BufferSize, IntPtr buffer, [In, Out] ref uint RequiredBufferSize);
        void DeleteAllData(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string Path, uint UserType, uint DataType);
        [PreserveSig]
        int CopyData(IntPtr sourcehandle, [MarshalAs(UnmanagedType.LPWStr)] string SourcePath, IntPtr desthandle, [MarshalAs(UnmanagedType.LPWStr)] string DestPath, int Attributes, int UserType, int DataType, [MarshalAs(UnmanagedType.Bool)] bool CopyFlag);
        [PreserveSig]
        void GetDataPaths(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string Path, int Identifier, int DataType, int BufferSize, [MarshalAs(UnmanagedType.LPWStr)] out char[] Buffer, [In, Out, MarshalAs(UnmanagedType.U4)] ref int RequiredBufferSize);
        [PreserveSig]
        int OpenKey(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string Path, [MarshalAs(UnmanagedType.U4)] MBKeyAccess AccessRequested, int TimeOut, [In, Out] ref IntPtr NewHandle);
        [PreserveSig]
        int CloseKey(IntPtr handle);
        void ChangePermissions(IntPtr handle, int TimeOut, [MarshalAs(UnmanagedType.U4)] MBKeyAccess AccessRequested);
        void SaveData();
        [PreserveSig]
        void GetHandleInfo(IntPtr handle, [In, Out] ref _METADATA_HANDLE_INFO Info);
        [PreserveSig]
        void GetSystemChangeNumber([In, Out, MarshalAs(UnmanagedType.U4)] ref uint SystemChangeNumber);
        [PreserveSig]
        void GetDataSetNumber(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string Path, [In, Out] ref uint DataSetNumber);
        [PreserveSig]
        void SetLastChangeTime(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string Path, out System.Runtime.InteropServices.ComTypes.FILETIME LastChangeTime, bool LocalTime);
        [PreserveSig]
        int GetLastChangeTime(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string Path, [In, Out] ref System.Runtime.InteropServices.ComTypes.FILETIME LastChangeTime, bool LocalTime);
        [PreserveSig]
        int KeyExchangePhase1();
        [PreserveSig]
        int KeyExchangePhase2();
        [PreserveSig]
        int Backup([MarshalAs(UnmanagedType.LPWStr)] string Location, int Version, int Flags);
        [PreserveSig]
        int Restore([MarshalAs(UnmanagedType.LPWStr)] string Location, int Version, int Flags);
        [PreserveSig]
        void EnumBackups([MarshalAs(UnmanagedType.LPWStr)] out string Location, [MarshalAs(UnmanagedType.U4)] out uint Version, out System.Runtime.InteropServices.ComTypes.FILETIME BackupTime, uint EnumIndex);
        [PreserveSig]
        void DeleteBackup([MarshalAs(UnmanagedType.LPWStr)] string Location, int Version);
        [PreserveSig]
        int UnmarshalInterface([MarshalAs(UnmanagedType.Interface)] out IMSAdminBase interf);
        [PreserveSig]
        int GetServerGuid();
    }
}

