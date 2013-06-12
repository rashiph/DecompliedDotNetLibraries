namespace System.Runtime.InteropServices
{
    using System;

    [ComVisible(true), AttributeUsage(AttributeTargets.Method, Inherited=false)]
    public sealed class TypeLibFuncAttribute : Attribute
    {
        internal TypeLibFuncFlags _val;

        public TypeLibFuncAttribute(short flags)
        {
            this._val = (TypeLibFuncFlags) flags;
        }

        public TypeLibFuncAttribute(TypeLibFuncFlags flags)
        {
            this._val = flags;
        }

        public TypeLibFuncFlags Value
        {
            get
            {
                return this._val;
            }
        }
    }
}

