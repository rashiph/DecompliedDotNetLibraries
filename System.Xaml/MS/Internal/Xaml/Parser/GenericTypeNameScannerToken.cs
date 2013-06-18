namespace MS.Internal.Xaml.Parser
{
    using System;

    internal enum GenericTypeNameScannerToken
    {
        NONE,
        ERROR,
        OPEN,
        CLOSE,
        COLON,
        COMMA,
        SUBSCRIPT,
        NAME
    }
}

