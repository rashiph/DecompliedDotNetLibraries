namespace System.Runtime.InteropServices
{
    using System;

    [ComVisible(true), AttributeUsage(AttributeTargets.Field, Inherited=false)]
    public sealed class TypeLibVarAttribute : Attribute
    {
        internal TypeLibVarFlags _val;

        public TypeLibVarAttribute(short flags)
        {
            this._val = (TypeLibVarFlags) flags;
        }

        public TypeLibVarAttribute(TypeLibVarFlags flags)
        {
            this._val = flags;
        }

        public TypeLibVarFlags Value
        {
            get
            {
                return this._val;
            }
        }
    }
}

