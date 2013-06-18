namespace Microsoft.Build.Framework
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public sealed class LoadInSeparateAppDomainAttribute : Attribute
    {
    }
}

