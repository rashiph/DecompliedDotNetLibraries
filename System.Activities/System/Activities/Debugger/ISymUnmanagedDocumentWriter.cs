namespace System.Activities.Debugger
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("B01FAFEB-C450-3A4D-BEEC-B4CEEC01E006"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(false)]
    internal interface ISymUnmanagedDocumentWriter
    {
        void SetSource(int sourceSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] byte[] source);
        void SetCheckSum(Guid algorithmId, int checkSumSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] checkSum);
    }
}

