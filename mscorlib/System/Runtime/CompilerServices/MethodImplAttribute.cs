namespace System.Runtime.CompilerServices
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited=false), ComVisible(true)]
    public sealed class MethodImplAttribute : Attribute
    {
        internal MethodImplOptions _val;
        public System.Runtime.CompilerServices.MethodCodeType MethodCodeType;

        public MethodImplAttribute()
        {
        }

        public MethodImplAttribute(short value)
        {
            this._val = (MethodImplOptions) value;
        }

        internal MethodImplAttribute(MethodImplAttributes methodImplAttributes)
        {
            MethodImplOptions options = MethodImplOptions.InternalCall | MethodImplOptions.PreserveSig | MethodImplOptions.NoOptimization | MethodImplOptions.Synchronized | MethodImplOptions.ForwardRef | MethodImplOptions.NoInlining | MethodImplOptions.Unmanaged;
            this._val = ((MethodImplOptions) methodImplAttributes) & options;
        }

        public MethodImplAttribute(MethodImplOptions methodImplOptions)
        {
            this._val = methodImplOptions;
        }

        public MethodImplOptions Value
        {
            get
            {
                return this._val;
            }
        }
    }
}

