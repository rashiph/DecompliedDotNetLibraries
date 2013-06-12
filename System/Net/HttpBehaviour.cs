namespace System.Net
{
    using System;

    internal enum HttpBehaviour : byte
    {
        HTTP10 = 1,
        HTTP11 = 3,
        HTTP11PartiallyCompliant = 2,
        Unknown = 0
    }
}

