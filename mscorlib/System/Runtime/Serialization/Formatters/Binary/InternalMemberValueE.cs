namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;

    [Serializable]
    internal enum InternalMemberValueE
    {
        Empty,
        InlineValue,
        Nested,
        Reference,
        Null
    }
}

