namespace MS.Internal.Xaml.Parser
{
    using System;

    internal enum ScannerNodeType
    {
        NONE,
        ELEMENT,
        EMPTYELEMENT,
        ATTRIBUTE,
        DIRECTIVE,
        PREFIXDEFINITION,
        PROPERTYELEMENT,
        EMPTYPROPERTYELEMENT,
        TEXT,
        ENDTAG
    }
}

