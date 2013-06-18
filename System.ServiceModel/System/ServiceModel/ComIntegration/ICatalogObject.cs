namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("6EB22871-8A19-11D0-81B6-00A0C9231C29")]
    internal interface ICatalogObject
    {
        [DispId(1)]
        object GetValue([In, MarshalAs(UnmanagedType.BStr)] string propName);
        [DispId(1)]
        void SetValue([In, MarshalAs(UnmanagedType.BStr)] string propName, [In] object value);
        [DispId(2)]
        object Key();
        [DispId(3)]
        object Name();
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [DispId(4)]
        bool IsPropertyReadOnly([In, MarshalAs(UnmanagedType.BStr)] string bstrPropName);
        bool Valid { [return: MarshalAs(UnmanagedType.VariantBool)] [DispId(5)] get; }
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [DispId(6)]
        bool IsPropertyWriteOnly([In, MarshalAs(UnmanagedType.BStr)] string bstrPropName);
    }
}

