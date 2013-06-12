namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;

    [Serializable]
    internal enum ValueFixupEnum
    {
        Empty,
        Array,
        Header,
        Member
    }
}

