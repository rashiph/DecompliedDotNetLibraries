namespace Microsoft.Build.Framework
{
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=false)]
    public sealed class RequiredAttribute : Attribute
    {
    }
}

