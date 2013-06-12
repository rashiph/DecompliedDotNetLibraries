namespace System.Security
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false, Inherited=false)]
    public sealed class SecurityTransparentAttribute : Attribute
    {
    }
}

