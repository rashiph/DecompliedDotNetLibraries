namespace System.Diagnostics.SymbolStore
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ISymbolDocumentWriter
    {
        void SetCheckSum(Guid algorithmId, byte[] checkSum);
        void SetSource(byte[] source);
    }
}

