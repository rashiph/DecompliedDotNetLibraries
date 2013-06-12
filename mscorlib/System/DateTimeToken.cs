namespace System
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct DateTimeToken
    {
        internal DateTimeParse.DTT dtt;
        internal TokenType suffix;
        internal int num;
    }
}

