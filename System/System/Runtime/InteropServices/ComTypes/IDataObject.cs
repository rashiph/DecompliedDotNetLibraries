namespace System.Runtime.InteropServices.ComTypes
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000010E-0000-0000-C000-000000000046")]
    public interface IDataObject
    {
        void GetData([In] ref FORMATETC format, out STGMEDIUM medium);
        void GetDataHere([In] ref FORMATETC format, ref STGMEDIUM medium);
        [PreserveSig]
        int QueryGetData([In] ref FORMATETC format);
        [PreserveSig]
        int GetCanonicalFormatEtc([In] ref FORMATETC formatIn, out FORMATETC formatOut);
        void SetData([In] ref FORMATETC formatIn, [In] ref STGMEDIUM medium, [MarshalAs(UnmanagedType.Bool)] bool release);
        IEnumFORMATETC EnumFormatEtc(DATADIR direction);
        [PreserveSig]
        int DAdvise([In] ref FORMATETC pFormatetc, ADVF advf, IAdviseSink adviseSink, out int connection);
        void DUnadvise(int connection);
        [PreserveSig]
        int EnumDAdvise(out IEnumSTATDATA enumAdvise);
    }
}

