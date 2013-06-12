namespace System.Runtime.InteropServices.ComTypes
{
    using System;

    [Flags]
    public enum TYMED
    {
        TYMED_ENHMF = 0x40,
        TYMED_FILE = 2,
        TYMED_GDI = 0x10,
        TYMED_HGLOBAL = 1,
        TYMED_ISTORAGE = 8,
        TYMED_ISTREAM = 4,
        TYMED_MFPICT = 0x20,
        TYMED_NULL = 0
    }
}

