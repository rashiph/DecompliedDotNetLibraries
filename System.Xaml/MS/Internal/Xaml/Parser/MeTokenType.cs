namespace MS.Internal.Xaml.Parser
{
    using System;

    internal enum MeTokenType
    {
        Close = 0x7d,
        Comma = 0x2c,
        EqualSign = 0x3d,
        None = 0,
        Open = 0x7b,
        PropertyName = 0x2e,
        QuotedMarkupExtension = 0x30,
        String = 0x2f,
        TypeName = 0x2d
    }
}

