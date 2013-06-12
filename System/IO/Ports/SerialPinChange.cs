namespace System.IO.Ports
{
    using System;

    public enum SerialPinChange
    {
        Break = 0x40,
        CDChanged = 0x20,
        CtsChanged = 8,
        DsrChanged = 0x10,
        Ring = 0x100
    }
}

