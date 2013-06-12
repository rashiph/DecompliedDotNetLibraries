namespace System.Net
{
    using System;

    internal enum BufferType
    {
        ChannelBindings = 14,
        Data = 1,
        Empty = 0,
        Extra = 5,
        Header = 7,
        Missing = 4,
        Padding = 9,
        Parameters = 3,
        ReadOnlyFlag = -2147483648,
        ReadOnlyWithChecksum = 0x10000000,
        Stream = 10,
        TargetHost = 0x10,
        Token = 2,
        Trailer = 6
    }
}

