namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field, Inherited=false), ComVisible(true)]
    public abstract class CustomConstantAttribute : Attribute
    {
        protected CustomConstantAttribute()
        {
        }

        public abstract object Value { get; }
    }
}

