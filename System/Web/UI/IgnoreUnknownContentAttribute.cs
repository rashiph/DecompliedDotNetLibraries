namespace System.Web.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class IgnoreUnknownContentAttribute : Attribute
    {
        internal IgnoreUnknownContentAttribute()
        {
        }
    }
}

