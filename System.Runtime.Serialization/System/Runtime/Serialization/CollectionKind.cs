namespace System.Runtime.Serialization
{
    using System;

    internal enum CollectionKind : byte
    {
        Array = 9,
        Collection = 7,
        Dictionary = 2,
        Enumerable = 8,
        GenericCollection = 4,
        GenericDictionary = 1,
        GenericEnumerable = 6,
        GenericList = 3,
        List = 5,
        None = 0
    }
}

