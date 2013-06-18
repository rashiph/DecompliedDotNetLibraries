namespace System.Runtime.Serialization.Formatters.Soap
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

