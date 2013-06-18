namespace MS.Internal.Xaml.Context
{
    using System;

    internal enum FixupType
    {
        MarkupExtensionFirstRun,
        MarkupExtensionRerun,
        PropertyValue,
        ObjectInitializationValue,
        UnresolvedChildren
    }
}

