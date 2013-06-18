namespace System.EnterpriseServices.Admin
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("790C6E0B-9194-4cc9-9426-A48A63185696"), SuppressUnmanagedCodeSecurity]
    internal interface ICatalog2
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(1)]
        object GetCollection([In, MarshalAs(UnmanagedType.BStr)] string bstrCollName);
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(2)]
        object Connect([In, MarshalAs(UnmanagedType.BStr)] string connectStr);
        [DispId(3)]
        int MajorVersion();
        [DispId(4)]
        int MinorVersion();
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(5)]
        object GetCollectionByQuery([In, MarshalAs(UnmanagedType.BStr)] string collName, [In, MarshalAs(UnmanagedType.SafeArray)] ref object[] aQuery);
        [DispId(6)]
        void ImportComponent([In, MarshalAs(UnmanagedType.BStr)] string bstrApplIdOrName, [In, MarshalAs(UnmanagedType.BStr)] string bstrCLSIDOrProgId);
        [DispId(7)]
        void InstallComponent([In, MarshalAs(UnmanagedType.BStr)] string bstrApplIdOrName, [In, MarshalAs(UnmanagedType.BStr)] string bstrDLL, [In, MarshalAs(UnmanagedType.BStr)] string bstrTLB, [In, MarshalAs(UnmanagedType.BStr)] string bstrPSDLL);
        [DispId(8)]
        void ShutdownApplication([In, MarshalAs(UnmanagedType.BStr)] string bstrApplIdOrName);
        [DispId(9)]
        void ExportApplication([In, MarshalAs(UnmanagedType.BStr)] string bstrApplIdOrName, [In, MarshalAs(UnmanagedType.BStr)] string bstrApplicationFile, [In] int lOptions);
        [DispId(10)]
        void InstallApplication([In, MarshalAs(UnmanagedType.BStr)] string bstrApplicationFile, [In, MarshalAs(UnmanagedType.BStr)] string bstrDestinationDirectory, [In] int lOptions, [In, MarshalAs(UnmanagedType.BStr)] string bstrUserId, [In, MarshalAs(UnmanagedType.BStr)] string bstrPassword, [In, MarshalAs(UnmanagedType.BStr)] string bstrRSN);
        [DispId(11)]
        void StopRouter();
        [DispId(12)]
        void RefreshRouter();
        [DispId(13)]
        void StartRouter();
        [DispId(14)]
        void Reserved1();
        [DispId(15)]
        void Reserved2();
        [DispId(0x10)]
        void InstallMultipleComponents([In, MarshalAs(UnmanagedType.BStr)] string bstrApplIdOrName, [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] ref object[] fileNames, [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] ref object[] CLSIDS);
        [DispId(0x11)]
        void GetMultipleComponentsInfo([In, MarshalAs(UnmanagedType.BStr)] string bstrApplIdOrName, [In] object varFileNames, [MarshalAs(UnmanagedType.SafeArray)] out object[] varCLSIDS, [MarshalAs(UnmanagedType.SafeArray)] out object[] varClassNames, [MarshalAs(UnmanagedType.SafeArray)] out object[] varFileFlags, [MarshalAs(UnmanagedType.SafeArray)] out object[] varComponentFlags);
        [DispId(0x12)]
        void RefreshComponents();
        [DispId(0x13)]
        void BackupREGDB([In, MarshalAs(UnmanagedType.BStr)] string bstrBackupFilePath);
        [DispId(20)]
        void RestoreREGDB([In, MarshalAs(UnmanagedType.BStr)] string bstrBackupFilePath);
        [DispId(0x15)]
        void QueryApplicationFile([In, MarshalAs(UnmanagedType.BStr)] string bstrApplicationFile, [MarshalAs(UnmanagedType.BStr)] out string bstrApplicationName, [MarshalAs(UnmanagedType.BStr)] out string bstrApplicationDescription, [MarshalAs(UnmanagedType.VariantBool)] out bool bHasUsers, [MarshalAs(UnmanagedType.VariantBool)] out bool bIsProxy, [MarshalAs(UnmanagedType.SafeArray)] out object[] varFileNames);
        [DispId(0x16)]
        void StartApplication([In, MarshalAs(UnmanagedType.BStr)] string bstrApplIdOrName);
        [DispId(0x17)]
        int ServiceCheck([In] int lService);
        [DispId(0x18)]
        void InstallMultipleEventClasses([In, MarshalAs(UnmanagedType.BStr)] string bstrApplIdOrName, [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] ref object[] fileNames, [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] ref object[] CLSIDS);
        [DispId(0x19)]
        void InstallEventClass([In, MarshalAs(UnmanagedType.BStr)] string bstrApplIdOrName, [In, MarshalAs(UnmanagedType.BStr)] string bstrDLL, [In, MarshalAs(UnmanagedType.BStr)] string bstrTLB, [In, MarshalAs(UnmanagedType.BStr)] string bstrPSDLL);
        [DispId(0x1a)]
        void GetEventClassesForIID([In] string bstrIID, [In, Out, MarshalAs(UnmanagedType.SafeArray)] ref object[] varCLSIDS, [In, Out, MarshalAs(UnmanagedType.SafeArray)] ref object[] varProgIDs, [In, Out, MarshalAs(UnmanagedType.SafeArray)] ref object[] varDescriptions);
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(0x1b)]
        object GetCollectionByQuery2([In, MarshalAs(UnmanagedType.BStr)] string bstrCollectionName, [In, MarshalAs(UnmanagedType.LPStruct)] object pVarQueryStrings);
        [return: MarshalAs(UnmanagedType.BStr)]
        [DispId(0x1c)]
        string GetApplicationInstanceIDFromProcessID([In, MarshalAs(UnmanagedType.I4)] int lProcessID);
        [DispId(0x1d)]
        void ShutdownApplicationInstances([In, MarshalAs(UnmanagedType.LPStruct)] object pVarApplicationInstanceID);
        [DispId(30)]
        void PauseApplicationInstances([In, MarshalAs(UnmanagedType.LPStruct)] object pVarApplicationInstanceID);
        [DispId(0x1f)]
        void ResumeApplicationInstances([In, MarshalAs(UnmanagedType.LPStruct)] object pVarApplicationInstanceID);
        [DispId(0x20)]
        void RecycleApplicationInstances([In, MarshalAs(UnmanagedType.LPStruct)] object pVarApplicationInstanceID, [In, MarshalAs(UnmanagedType.I4)] int lReasonCode);
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [DispId(0x21)]
        bool AreApplicationInstancesPaused([In, MarshalAs(UnmanagedType.LPStruct)] object pVarApplicationInstanceID);
        [return: MarshalAs(UnmanagedType.BStr)]
        [DispId(0x22)]
        string DumpApplicationInstance([In, MarshalAs(UnmanagedType.BStr)] string bstrApplicationInstanceID, [In, MarshalAs(UnmanagedType.BStr)] string bstrDirectory, [In, MarshalAs(UnmanagedType.I4)] int lMaxImages);
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [DispId(0x23)]
        bool IsApplicationInstanceDumpSupported();
        [DispId(0x24)]
        void CreateServiceForApplication([In, MarshalAs(UnmanagedType.BStr)] string bstrApplicationIDOrName, [In, MarshalAs(UnmanagedType.BStr)] string bstrServiceName, [In, MarshalAs(UnmanagedType.BStr)] string bstrStartType, [In, MarshalAs(UnmanagedType.BStr)] string bstrErrorControl, [In, MarshalAs(UnmanagedType.BStr)] string bstrDependencies, [In, MarshalAs(UnmanagedType.BStr)] string bstrRunAs, [In, MarshalAs(UnmanagedType.BStr)] string bstrPassword, [In, MarshalAs(UnmanagedType.VariantBool)] bool bDesktopOk);
        [DispId(0x25)]
        void DeleteServiceForApplication([In, MarshalAs(UnmanagedType.BStr)] string bstrApplicationIDOrName);
        [return: MarshalAs(UnmanagedType.BStr)]
        [DispId(0x26)]
        string GetPartitionID([In, MarshalAs(UnmanagedType.BStr)] string bstrApplicationIDOrName);
        [return: MarshalAs(UnmanagedType.BStr)]
        [DispId(0x27)]
        string GetPartitionName([In, MarshalAs(UnmanagedType.BStr)] string bstrApplicationIDOrName);
        [DispId(40)]
        void CurrentPartition([In, MarshalAs(UnmanagedType.BStr)] string bstrPartitionIDOrName);
        [return: MarshalAs(UnmanagedType.BStr)]
        [DispId(0x29)]
        string CurrentPartitionID();
        [return: MarshalAs(UnmanagedType.BStr)]
        [DispId(0x2a)]
        string CurrentPartitionName();
        [return: MarshalAs(UnmanagedType.BStr)]
        [DispId(0x2b)]
        string GlobalPartitionID();
        [DispId(0x2c)]
        void FlushPartitionCache();
        [DispId(0x2d)]
        void CopyApplications([In, MarshalAs(UnmanagedType.BStr)] string bstrSourcePartitionIDOrName, [In, MarshalAs(UnmanagedType.LPStruct)] object pVarApplicationID, [In, MarshalAs(UnmanagedType.BStr)] string bstrDestinationPartitionIDOrName);
        [DispId(0x2e)]
        void CopyComponents([In, MarshalAs(UnmanagedType.BStr)] string bstrSourceApplicationIDOrName, [In, MarshalAs(UnmanagedType.LPStruct)] object pVarCLSIDOrProgID, [In, MarshalAs(UnmanagedType.BStr)] string bstrDestinationApplicationIDOrName);
        [DispId(0x2f)]
        void MoveComponents([In, MarshalAs(UnmanagedType.BStr)] string bstrSourceApplicationIDOrName, [In, MarshalAs(UnmanagedType.LPStruct)] object pVarCLSIDOrProgID, [In, MarshalAs(UnmanagedType.BStr)] string bstrDestinationApplicationIDOrName);
        [DispId(0x30)]
        void AliasComponent([In, MarshalAs(UnmanagedType.BStr)] string bstrSrcApplicationIDOrName, [In, MarshalAs(UnmanagedType.BStr)] string bstrCLSIDOrProgID, [In, MarshalAs(UnmanagedType.BStr)] string bstrDestApplicationIDOrName, [In, MarshalAs(UnmanagedType.BStr)] string bstrNewProgId, [In, MarshalAs(UnmanagedType.BStr)] string bstrNewClsid);
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(0x31)]
        object IsSafeToDelete([In, MarshalAs(UnmanagedType.BStr)] string bstrDllName);
        [DispId(50)]
        void ImportUnconfiguredComponents([In, MarshalAs(UnmanagedType.BStr)] string bstrApplicationIDOrName, [In, MarshalAs(UnmanagedType.LPStruct)] object pVarCLSIDOrProgID, [In, MarshalAs(UnmanagedType.LPStruct)] object pVarComponentType);
        [DispId(0x33)]
        void PromoteUnconfiguredComponents([In, MarshalAs(UnmanagedType.BStr)] string bstrApplicationIDOrName, [In, MarshalAs(UnmanagedType.LPStruct)] object pVarCLSIDOrProgID, [In, MarshalAs(UnmanagedType.LPStruct)] object pVarComponentType);
        [DispId(0x34)]
        void ImportComponents([In, MarshalAs(UnmanagedType.BStr)] string bstrApplicationIDOrName, [In] ref object pVarCLSIDOrProgID, [In] ref object pVarComponentType);
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [DispId(0x35)]
        bool Is64BitCatalogServer();
        [DispId(0x36)]
        void ExportPartition([In, MarshalAs(UnmanagedType.BStr)] string bstrPartitionIDOrName, [In, MarshalAs(UnmanagedType.BStr)] string bstrPartitionFileName, [In, MarshalAs(UnmanagedType.I4)] int lOptions);
        [DispId(0x37)]
        void InstallPartition([In, MarshalAs(UnmanagedType.BStr)] string bstrFileName, [In, MarshalAs(UnmanagedType.BStr)] string bstrDestDirectory, [In, MarshalAs(UnmanagedType.I4)] int lOptions, [In, MarshalAs(UnmanagedType.BStr)] string bstrUserID, [In, MarshalAs(UnmanagedType.BStr)] string bstrPassword, [In, MarshalAs(UnmanagedType.BStr)] string bstrRSN);
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [DispId(0x38)]
        object QueryApplicationFile2([In, MarshalAs(UnmanagedType.BStr)] string bstrApplicationFile);
        [return: MarshalAs(UnmanagedType.I4)]
        [DispId(0x39)]
        int GetComponentVersionCount([In, MarshalAs(UnmanagedType.BStr)] string bstrCLSIDOrProgID);
    }
}

