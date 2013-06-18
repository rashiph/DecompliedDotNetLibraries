namespace System.ServiceModel.Dispatcher
{
    using System;

    internal enum QueryAxisType : byte
    {
        Ancestor = 1,
        AncestorOrSelf = 2,
        Attribute = 3,
        Child = 4,
        Descendant = 5,
        DescendantOrSelf = 6,
        Following = 7,
        FollowingSibling = 8,
        Namespace = 9,
        None = 0,
        Parent = 10,
        Preceding = 11,
        PrecedingSibling = 12,
        Self = 13
    }
}

