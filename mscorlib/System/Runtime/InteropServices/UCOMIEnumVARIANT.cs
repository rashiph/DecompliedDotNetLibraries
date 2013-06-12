namespace System.Runtime.InteropServices
{
    using System;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Obsolete("Use System.Runtime.InteropServices.ComTypes.IEnumVARIANT instead. http://go.microsoft.com/fwlink/?linkid=14202", false), Guid("00020404-0000-0000-C000-000000000046")]
    public interface UCOMIEnumVARIANT
    {
        [PreserveSig]
        int Next(int celt, int rgvar, int pceltFetched);
        [PreserveSig]
        int Skip(int celt);
        [PreserveSig]
        int Reset();
        void Clone(int ppenum);
    }
}

