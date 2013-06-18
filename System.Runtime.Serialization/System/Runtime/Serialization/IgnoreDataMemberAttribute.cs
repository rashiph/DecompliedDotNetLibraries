namespace System.Runtime.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited=false, AllowMultiple=false)]
    public sealed class IgnoreDataMemberAttribute : Attribute
    {
    }
}

