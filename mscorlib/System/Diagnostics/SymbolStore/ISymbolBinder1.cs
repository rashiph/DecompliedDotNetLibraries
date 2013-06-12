namespace System.Diagnostics.SymbolStore
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ISymbolBinder1
    {
        ISymbolReader GetReader(IntPtr importer, string filename, string searchPath);
    }
}

