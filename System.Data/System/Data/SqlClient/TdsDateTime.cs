namespace System.Data.SqlClient
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TdsDateTime
    {
        public int days;
        public int time;
    }
}

