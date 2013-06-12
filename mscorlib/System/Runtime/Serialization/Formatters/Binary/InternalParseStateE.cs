namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;

    [Serializable]
    internal enum InternalParseStateE
    {
        Initial,
        Object,
        Member,
        MemberChild
    }
}

