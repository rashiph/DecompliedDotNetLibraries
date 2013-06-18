namespace System.Runtime.Serialization.Json
{
    using System;

    internal enum JsonNodeType
    {
        None,
        Object,
        Element,
        EndElement,
        QuotedText,
        StandaloneText,
        Collection
    }
}

