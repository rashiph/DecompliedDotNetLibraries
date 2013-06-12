namespace System.Runtime.CompilerServices
{
    using System;

    [Serializable, AttributeUsage(AttributeTargets.Struct)]
    public sealed class HasCopySemanticsAttribute : Attribute
    {
    }
}

