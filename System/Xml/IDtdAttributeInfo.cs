namespace System.Xml
{
    using System;

    internal interface IDtdAttributeInfo
    {
        bool IsDeclaredInExternal { get; }

        bool IsNonCDataType { get; }

        bool IsXmlAttribute { get; }

        int LineNumber { get; }

        int LinePosition { get; }

        string LocalName { get; }

        string Prefix { get; }
    }
}

