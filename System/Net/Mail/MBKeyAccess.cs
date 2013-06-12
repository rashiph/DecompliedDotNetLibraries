namespace System.Net.Mail
{
    using System;

    [Flags]
    internal enum MBKeyAccess : uint
    {
        Read = 1,
        Write = 2
    }
}

