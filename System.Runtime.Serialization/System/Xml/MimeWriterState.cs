namespace System.Xml
{
    using System;

    internal enum MimeWriterState
    {
        Start,
        StartPreface,
        StartPart,
        Header,
        Content,
        Closed
    }
}

