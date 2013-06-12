namespace System.Net
{
    using System;

    [Flags]
    internal enum AddressInfoHints
    {
        AI_CANONNAME = 2,
        AI_NUMERICHOST = 4,
        AI_PASSIVE = 1
    }
}

