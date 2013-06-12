namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false), AttributeUsage(AttributeTargets.All)]
    internal sealed class DecoratedNameAttribute : Attribute
    {
        public DecoratedNameAttribute(string decoratedName)
        {
        }
    }
}

