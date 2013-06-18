namespace System.ServiceModel.Dispatcher
{
    using System;

    internal enum NodeQNameType : byte
    {
        Empty = 0,
        Name = 1,
        Namespace = 2,
        NamespaceWildcard = 8,
        NameWildcard = 4,
        Standard = 3,
        Wildcard = 12
    }
}

