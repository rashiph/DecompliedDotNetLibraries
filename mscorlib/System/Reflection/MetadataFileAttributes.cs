namespace System.Reflection
{
    using System;

    [Serializable, Flags]
    internal enum MetadataFileAttributes
    {
        ContainsMetadata,
        ContainsNoMetadata
    }
}

