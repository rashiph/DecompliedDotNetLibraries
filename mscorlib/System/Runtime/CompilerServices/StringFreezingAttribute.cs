namespace System.Runtime.CompilerServices
{
    using System;

    [Serializable, AttributeUsage(AttributeTargets.Assembly, Inherited=false)]
    public sealed class StringFreezingAttribute : Attribute
    {
    }
}

