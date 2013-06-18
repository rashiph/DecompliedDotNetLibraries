namespace System.Xml.Linq
{
    using System;

    [Flags]
    public enum LoadOptions
    {
        None = 0,
        PreserveWhitespace = 1,
        SetBaseUri = 2,
        SetLineInfo = 4
    }
}

