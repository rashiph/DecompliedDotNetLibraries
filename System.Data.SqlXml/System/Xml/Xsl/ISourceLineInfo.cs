namespace System.Xml.Xsl
{
    using System;

    internal interface ISourceLineInfo
    {
        Location End { get; }

        bool IsNoSource { get; }

        Location Start { get; }

        string Uri { get; }
    }
}

