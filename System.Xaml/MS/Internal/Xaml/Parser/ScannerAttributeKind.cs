namespace MS.Internal.Xaml.Parser
{
    using System;

    internal enum ScannerAttributeKind
    {
        Namespace,
        CtorDirective,
        Name,
        Directive,
        XmlSpace,
        Event,
        Property,
        AttachableProperty,
        Unknown
    }
}

