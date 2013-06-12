namespace System.Runtime.Serialization.Formatters
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum FormatterTypeStyle
    {
        TypesWhenNeeded,
        TypesAlways,
        XsdString
    }
}

