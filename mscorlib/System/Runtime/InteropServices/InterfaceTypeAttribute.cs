namespace System.Runtime.InteropServices
{
    using System;

    [AttributeUsage(AttributeTargets.Interface, Inherited=false), ComVisible(true)]
    public sealed class InterfaceTypeAttribute : Attribute
    {
        internal ComInterfaceType _val;

        public InterfaceTypeAttribute(short interfaceType)
        {
            this._val = (ComInterfaceType) interfaceType;
        }

        public InterfaceTypeAttribute(ComInterfaceType interfaceType)
        {
            this._val = interfaceType;
        }

        public ComInterfaceType Value
        {
            get
            {
                return this._val;
            }
        }
    }
}

