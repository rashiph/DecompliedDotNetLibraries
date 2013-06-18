namespace System.Runtime.Serialization.Formatters.Soap
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

