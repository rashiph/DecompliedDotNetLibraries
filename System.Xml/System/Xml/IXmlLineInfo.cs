namespace System.Xml
{
    using System;

    public interface IXmlLineInfo
    {
        bool HasLineInfo();

        int LineNumber { get; }

        int LinePosition { get; }
    }
}

