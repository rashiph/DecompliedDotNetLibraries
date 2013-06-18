namespace Microsoft.VisualC
{
    using System;

    [Obsolete("Microsoft.VisualC.dll is an obsolete assembly and exists only for backwards compatibility."), AttributeUsage(AttributeTargets.All)]
    public sealed class DecoratedNameAttribute : Attribute
    {
        public DecoratedNameAttribute()
        {
        }

        public DecoratedNameAttribute(string decoratedName)
        {
        }
    }
}

