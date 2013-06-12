namespace System.Runtime.InteropServices.ComTypes
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("0000010F-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAdviseSink
    {
        [PreserveSig]
        void OnDataChange([In] ref FORMATETC format, [In] ref STGMEDIUM stgmedium);
        [PreserveSig]
        void OnViewChange(int aspect, int index);
        [PreserveSig]
        void OnRename(IMoniker moniker);
        [PreserveSig]
        void OnSave();
        [PreserveSig]
        void OnClose();
    }
}

