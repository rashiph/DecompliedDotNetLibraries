namespace Microsoft.Build.Framework
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public sealed class RunInMTAAttribute : Attribute
    {
    }
}

