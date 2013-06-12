namespace System.Runtime.InteropServices
{
    using System;

    [ComImport, Guid("B196B287-BAB4-101A-B69C-00AA00341D07"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Obsolete("Use System.Runtime.InteropServices.ComTypes.IEnumConnections instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
    public interface UCOMIEnumConnections
    {
        [PreserveSig]
        int Next(int celt, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] CONNECTDATA[] rgelt, out int pceltFetched);
        [PreserveSig]
        int Skip(int celt);
        [PreserveSig]
        void Reset();
        void Clone(out UCOMIEnumConnections ppenum);
    }
}

