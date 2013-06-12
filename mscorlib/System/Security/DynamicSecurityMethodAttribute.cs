namespace System.Security
{
    using System;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
    internal sealed class DynamicSecurityMethodAttribute : Attribute
    {
    }
}

