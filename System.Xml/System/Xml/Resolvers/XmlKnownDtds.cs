namespace System.Xml.Resolvers
{
    using System;

    [Flags]
    public enum XmlKnownDtds
    {
        All = 0xffff,
        None = 0,
        Rss091 = 2,
        Xhtml10 = 1
    }
}

