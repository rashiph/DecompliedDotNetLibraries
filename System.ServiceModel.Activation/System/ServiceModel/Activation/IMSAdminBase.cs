namespace System.ServiceModel.Activation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;

    [ComImport, Guid("70B51430-B6CA-11d0-B9B9-00A0C922E750"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    internal interface IMSAdminBase
    {
        [PreserveSig]
        uint AddKey(uint hMDHandle, string pszMDPath);
        [PreserveSig]
        uint DeleteKey(uint hMDHandle, string pszMDPath);
        [PreserveSig]
        uint DeleteChildKeys(uint hMDHandle, string pszMDPath);
        [PreserveSig]
        uint EnumKeys(uint hMDHandle, string pszMDPath, string pszMDName, uint dwMDEnumObjectIndex);
        [PreserveSig]
        uint CopyKey(uint hMDSourceHandle, string pszMDSourcePath, uint hMDDestHandle, string pszMDDestPath, int bMDOverwriteFlag, int bMDCopyFlag);
        [PreserveSig]
        uint RenameKey(uint hMDHandle, string pszMDPath, string pszMDNewName);
        [PreserveSig, SecurityCritical]
        uint SetData(uint hMDHandle, string pszMDPath, METADATA_RECORD pmdrMDData);
        [PreserveSig, SecurityCritical]
        uint GetData(uint hMDHandle, [MarshalAs(UnmanagedType.LPWStr)] string pszMDPath, ref METADATA_RECORD pmdrMDData, ref uint pdwMDRequiredDataLen);
        [PreserveSig]
        uint DeleteData(uint hMDHandle, string pszMDPath, uint dwMDIdentifier, uint dwMDDataType);
        [PreserveSig, SecurityCritical]
        uint EnumData(uint hMDHandle, string pszMDPath, METADATA_RECORD pmdrMDData, uint dwMDEnumDataIndex, ref uint pdwMDRequiredDataLen);
        [PreserveSig]
        uint GetAllData(uint hMDHandle, string pszMDPath, uint dwMDAttributes, uint dwMDUserType, uint dwMDDataType, ref uint pdwMDNumDataEntries, ref uint pdwMDDataSetNumber, uint dwMDBufferSize, ref uint pdwMDRequiredBufferSize, IntPtr ppDataBlob);
        [PreserveSig]
        uint DeleteAllData(uint hMDHandle, string pszMDPath, uint dwMDUserType, uint dwMDDataType);
        [PreserveSig]
        uint CopyData(uint hMDSourceHandle, string pszMDSourcePath, uint hMDDestHandle, string pszMDDestPath, uint dwMDAttributes, uint dwMDUserType, uint dwMDDataType, int bMDCopyFlag);
        [PreserveSig]
        uint GetDataPaths(uint hMDHandle, string pszMDPath, uint dwMDIdentifier, uint dwMDDataType, uint dwMDBufferSize, IntPtr pszBuffer, ref uint pdwMDRequiredBufferSize);
        [PreserveSig]
        uint OpenKey(uint hMDHandle, string pszMDPath, uint dwMDAccessRequested, uint dwMDTimeOut, out uint phMDNewHandle);
        [PreserveSig]
        uint CloseKey(uint hMDHandle);
        [PreserveSig]
        uint ChangePermissions(uint hMDHandle, uint dwMDTimeOut, uint dwMDAccessRequested);
        [PreserveSig]
        uint SaveData();
        [PreserveSig]
        uint GetHandleInfo(uint hMDHandle, METADATA_HANDLE_INFO pmdhiInfo);
        [PreserveSig]
        uint GetSystemChangeNumber(ref uint pdwSystemChangeNumber);
        [PreserveSig]
        uint GetDataSetNumber(uint hMDHandle, string pszMDPath, ref uint pdwMDDataSetNumber);
        [PreserveSig]
        uint SetLastChangeTime(uint hMDHandle, string pszMDPath, ref System.Runtime.InteropServices.ComTypes.FILETIME pftMDLastChangeTime, int bLocalTime);
        [PreserveSig]
        uint GetLastChangeTime(uint hMDHandle, string pszMDPath, ref System.Runtime.InteropServices.ComTypes.FILETIME pftMDLastChangeTime, int bLocalTime);
        [PreserveSig]
        uint KeyExchangePhase1();
        [PreserveSig]
        uint KeyExchangePhase2();
        [PreserveSig]
        uint Backup(string pszMDBackupLocation, uint dwMDVersion, uint dwMDFlags);
        [PreserveSig]
        uint Restore(string pszMDBackupLocation, uint dwMDVersion, uint dwMDFlags);
        [PreserveSig]
        uint EnumBackups(string pszMDBackupLocation, ref uint pdwMDVersion, ref System.Runtime.InteropServices.ComTypes.FILETIME pftMDBackupTime, uint dwMDEnumIndex);
        [PreserveSig]
        uint DeleteBackup(string pszMDBackupLocation, uint dwMDVersion);
        [PreserveSig]
        uint UnmarshalInterface(ref IMSAdminBase piadmbwInterface);
        [PreserveSig]
        uint GetServerGuid(ref Guid pServerGuid);
    }
}

