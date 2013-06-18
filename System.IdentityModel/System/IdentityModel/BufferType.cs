namespace System.IdentityModel
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
        Stream = 10,
        Token = 2,
        Trailer = 6
    }
}

