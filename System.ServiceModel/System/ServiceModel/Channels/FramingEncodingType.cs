namespace System.ServiceModel.Channels
{
    using System;

    internal enum FramingEncodingType
    {
        Soap11Utf8,
        Soap11Utf16,
        Soap11Utf16FFFE,
        Soap12Utf8,
        Soap12Utf16,
        Soap12Utf16FFFE,
        MTOM,
        Binary,
        BinarySession
    }
}

