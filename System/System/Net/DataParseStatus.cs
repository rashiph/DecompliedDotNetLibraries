namespace System.Net
{
    using System;

    internal enum DataParseStatus
    {
        NeedMoreData,
        ContinueParsing,
        Done,
        Invalid,
        DataTooBig
    }
}

