namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0af57545-a72a-4fbe-813c-8554ed7d4528")]
    internal interface IActContext
    {
        [SecurityCritical]
        void GetAppId([MarshalAs(UnmanagedType.Interface)] out object AppId);
        [SecurityCritical]
        void EnumCategories([In] uint Flags, [In] System.Deployment.Internal.Isolation.IReferenceIdentity CategoryToMatch, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object EnumOut);
        [SecurityCritical]
        void EnumSubcategories([In] uint Flags, [In] System.Deployment.Internal.Isolation.IDefinitionIdentity CategoryId, [In, MarshalAs(UnmanagedType.LPWStr)] string SubcategoryPattern, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object EnumOut);
        [SecurityCritical]
        void EnumCategoryInstances([In] uint Flags, [In] System.Deployment.Internal.Isolation.IDefinitionIdentity CategoryId, [In, MarshalAs(UnmanagedType.LPWStr)] string Subcategory, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object EnumOut);
        [SecurityCritical]
        void ReplaceStringMacros([In] uint Flags, [In, MarshalAs(UnmanagedType.LPWStr)] string Culture, [In, MarshalAs(UnmanagedType.LPWStr)] string ReplacementPattern, [MarshalAs(UnmanagedType.LPWStr)] out string Replaced);
        [SecurityCritical]
        void GetComponentStringTableStrings([In] uint Flags, [In, MarshalAs(UnmanagedType.SysUInt)] IntPtr ComponentIndex, [In, MarshalAs(UnmanagedType.SysUInt)] IntPtr StringCount, [Out, MarshalAs(UnmanagedType.LPArray)] string[] SourceStrings, [MarshalAs(UnmanagedType.LPArray)] out string[] DestinationStrings, [In, MarshalAs(UnmanagedType.SysUInt)] IntPtr CultureFallbacks);
        [SecurityCritical]
        void GetApplicationProperties([In] uint Flags, [In] UIntPtr cProperties, [In, MarshalAs(UnmanagedType.LPArray)] string[] PropertyNames, [MarshalAs(UnmanagedType.LPArray)] out string[] PropertyValues, [MarshalAs(UnmanagedType.LPArray)] out UIntPtr[] ComponentIndicies);
        [SecurityCritical]
        void ApplicationBasePath([In] uint Flags, [MarshalAs(UnmanagedType.LPWStr)] out string ApplicationPath);
        [SecurityCritical]
        void GetComponentManifest([In] uint Flags, [In] System.Deployment.Internal.Isolation.IDefinitionIdentity ComponentId, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ManifestInteface);
        [SecurityCritical]
        void GetComponentPayloadPath([In] uint Flags, [In] System.Deployment.Internal.Isolation.IDefinitionIdentity ComponentId, [MarshalAs(UnmanagedType.LPWStr)] out string PayloadPath);
        [SecurityCritical]
        void FindReferenceInContext([In] uint dwFlags, [In] System.Deployment.Internal.Isolation.IReferenceIdentity Reference, [MarshalAs(UnmanagedType.Interface)] out object MatchedDefinition);
        [SecurityCritical]
        void CreateActContextFromCategoryInstance([In] uint dwFlags, [In] ref System.Deployment.Internal.Isolation.CATEGORY_INSTANCE CategoryInstance, [MarshalAs(UnmanagedType.Interface)] out object ppCreatedAppContext);
        [SecurityCritical]
        void EnumComponents([In] uint dwFlags, [MarshalAs(UnmanagedType.Interface)] out object ppIdentityEnum);
        [SecurityCritical]
        void PrepareForExecution([In, MarshalAs(UnmanagedType.SysInt)] IntPtr Inputs, [In, MarshalAs(UnmanagedType.SysInt)] IntPtr Outputs);
        [SecurityCritical]
        void SetApplicationRunningState([In] uint dwFlags, [In] uint ulState, out uint ulDisposition);
        [SecurityCritical]
        void GetApplicationStateFilesystemLocation([In] uint dwFlags, [In] UIntPtr Component, [In, MarshalAs(UnmanagedType.SysInt)] IntPtr pCoordinateList, [MarshalAs(UnmanagedType.LPWStr)] out string ppszPath);
        [SecurityCritical]
        void FindComponentsByDefinition([In] uint dwFlags, [In] UIntPtr ComponentCount, [In, MarshalAs(UnmanagedType.LPArray)] System.Deployment.Internal.Isolation.IDefinitionIdentity[] Components, [Out, MarshalAs(UnmanagedType.LPArray)] UIntPtr[] Indicies, [Out, MarshalAs(UnmanagedType.LPArray)] uint[] Dispositions);
        [SecurityCritical]
        void FindComponentsByReference([In] uint dwFlags, [In] UIntPtr Components, [In, MarshalAs(UnmanagedType.LPArray)] System.Deployment.Internal.Isolation.IReferenceIdentity[] References, [Out, MarshalAs(UnmanagedType.LPArray)] UIntPtr[] Indicies, [Out, MarshalAs(UnmanagedType.LPArray)] uint[] Dispositions);
    }
}

