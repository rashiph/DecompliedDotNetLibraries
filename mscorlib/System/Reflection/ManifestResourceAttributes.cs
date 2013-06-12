namespace System.Reflection
{
    using System;

    [Serializable, Flags]
    internal enum ManifestResourceAttributes
    {
        Private = 2,
        Public = 1,
        VisibilityMask = 7
    }
}

