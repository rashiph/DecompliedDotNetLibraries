namespace System.ServiceModel
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false, Inherited=false)]
    public sealed class MessageHeaderArrayAttribute : MessageHeaderAttribute
    {
    }
}

