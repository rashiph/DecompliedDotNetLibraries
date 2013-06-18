namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x200), InterfaceType((short) 1), Guid("BFBF883A-CAD7-11D3-A11B-00105A1F515A")]
    internal interface IWbemObjectTextSrc
    {
        [PreserveSig]
        int GetText_([In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemClassObject_DoNotMarshal pObj, [In] uint uObjTextFormat, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.BStr)] out string strText);
        [PreserveSig]
        int CreateFromText_([In] int lFlags, [In, MarshalAs(UnmanagedType.BStr)] string strText, [In] uint uObjTextFormat, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out IWbemClassObject_DoNotMarshal pNewObj);
    }
}

