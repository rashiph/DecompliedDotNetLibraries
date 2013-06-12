namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Method, Inherited=false), ComVisible(true)]
    public sealed class OnDeserializedAttribute : Attribute
    {
    }
}

