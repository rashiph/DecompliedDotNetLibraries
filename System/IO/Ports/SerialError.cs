namespace System.IO.Ports
{
    using System;

    public enum SerialError
    {
        Frame = 8,
        Overrun = 2,
        RXOver = 1,
        RXParity = 4,
        TXFull = 0x100
    }
}

