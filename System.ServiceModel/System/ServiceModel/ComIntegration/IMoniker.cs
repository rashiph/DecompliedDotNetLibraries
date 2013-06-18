namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000000f-0000-0000-C000-000000000046")]
    internal interface IMoniker
    {
        void GetClassID(out Guid pClassID);
        [PreserveSig]
        int IsDirty();
        void Load(IStream pStm);
        void Save(IStream pStm, [MarshalAs(UnmanagedType.Bool)] bool fClearDirty);
        void GetSizeMax(out long pcbSize);
        void BindToObject(IBindCtx pbc, System.ServiceModel.ComIntegration.IMoniker pmkToLeft, [In] ref Guid riidResult, IntPtr ppvResult);
        void BindToStorage(IBindCtx pbc, System.ServiceModel.ComIntegration.IMoniker pmkToLeft, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppvObj);
        void Reduce(IBindCtx pbc, int dwReduceHowFar, ref System.ServiceModel.ComIntegration.IMoniker ppmkToLeft, out System.ServiceModel.ComIntegration.IMoniker ppmkReduced);
        void ComposeWith(System.ServiceModel.ComIntegration.IMoniker pmkRight, [MarshalAs(UnmanagedType.Bool)] bool fOnlyIfNotGeneric, out System.ServiceModel.ComIntegration.IMoniker ppmkComposite);
        void Enum([MarshalAs(UnmanagedType.Bool)] bool fForward, out IEnumMoniker ppenumMoniker);
        [PreserveSig]
        int IsEqual(System.ServiceModel.ComIntegration.IMoniker pmkOtherMoniker);
        void Hash(IntPtr pdwHash);
        [PreserveSig]
        int IsRunning(IBindCtx pbc, System.ServiceModel.ComIntegration.IMoniker pmkToLeft, System.ServiceModel.ComIntegration.IMoniker pmkNewlyRunning);
        void GetTimeOfLastChange(IBindCtx pbc, System.ServiceModel.ComIntegration.IMoniker pmkToLeft, out System.Runtime.InteropServices.ComTypes.FILETIME pFileTime);
        void Inverse(out System.ServiceModel.ComIntegration.IMoniker ppmk);
        void CommonPrefixWith(System.ServiceModel.ComIntegration.IMoniker pmkOther, out System.ServiceModel.ComIntegration.IMoniker ppmkPrefix);
        void RelativePathTo(System.ServiceModel.ComIntegration.IMoniker pmkOther, out System.ServiceModel.ComIntegration.IMoniker ppmkRelPath);
        void GetDisplayName(IBindCtx pbc, System.ServiceModel.ComIntegration.IMoniker pmkToLeft, [MarshalAs(UnmanagedType.LPWStr)] out string ppszDisplayName);
        void ParseDisplayName(IBindCtx pbc, System.ServiceModel.ComIntegration.IMoniker pmkToLeft, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, out int pchEaten, out System.ServiceModel.ComIntegration.IMoniker ppmkOut);
        [PreserveSig]
        int IsSystemMoniker(IntPtr pdwMksys);
    }
}

