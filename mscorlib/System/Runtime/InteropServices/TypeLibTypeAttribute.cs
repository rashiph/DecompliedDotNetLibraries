namespace System.Runtime.InteropServices
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class, Inherited=false), ComVisible(true)]
    public sealed class TypeLibTypeAttribute : Attribute
    {
        internal TypeLibTypeFlags _val;

        public TypeLibTypeAttribute(short flags)
        {
            this._val = (TypeLibTypeFlags) flags;
        }

        public TypeLibTypeAttribute(TypeLibTypeFlags flags)
        {
            this._val = flags;
        }

        public TypeLibTypeFlags Value
        {
            get
            {
                return this._val;
            }
        }
    }
}

