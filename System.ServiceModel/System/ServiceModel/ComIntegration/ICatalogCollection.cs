namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("6EB22872-8A19-11D0-81B6-00A0C9231C29")]
    internal interface ICatalogCollection
    {
        [DispId(-4)]
        void GetEnumerator(out IEnumerator pEnum);
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(1)]
        object Item([In] int lIndex);
        [DispId(0x60020002)]
        int Count();
        [DispId(0x60020003)]
        void Remove([In] int lIndex);
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(0x60020004)]
        object Add();
        [DispId(2)]
        void Populate();
        [DispId(3)]
        int SaveChanges();
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(4)]
        object GetCollection([In, MarshalAs(UnmanagedType.BStr)] string bstrCollName, [In] object varObjectKey);
        [DispId(6)]
        object Name();
        bool IsAddEnabled { [return: MarshalAs(UnmanagedType.VariantBool)] [DispId(7)] get; }
        bool IsRemoveEnabled { [return: MarshalAs(UnmanagedType.VariantBool)] [DispId(8)] get; }
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(9)]
        object GetUtilInterface();
        int DataStoreMajorVersion { [DispId(10)] get; }
        int DataStoreMinorVersion { [DispId(11)] get; }
        void PopulateByKey([In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] object[] aKeys);
        [DispId(13)]
        void PopulateByQuery([In, MarshalAs(UnmanagedType.BStr)] string bstrQueryString, [In] int lQueryType);
    }
}

