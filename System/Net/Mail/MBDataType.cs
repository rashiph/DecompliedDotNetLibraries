namespace System.Net.Mail
{
    using System;

    internal enum MBDataType : byte
    {
        All = 0,
        Binary = 3,
        Dword = 1,
        MultiString = 5,
        String = 2,
        StringExpand = 4
    }
}

