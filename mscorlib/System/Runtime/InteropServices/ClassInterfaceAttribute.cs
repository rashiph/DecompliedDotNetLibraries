namespace System.Runtime.InteropServices
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited=false), ComVisible(true)]
    public sealed class ClassInterfaceAttribute : Attribute
    {
        internal ClassInterfaceType _val;

        public ClassInterfaceAttribute(short classInterfaceType)
        {
            this._val = (ClassInterfaceType) classInterfaceType;
        }

        public ClassInterfaceAttribute(ClassInterfaceType classInterfaceType)
        {
            this._val = classInterfaceType;
        }

        public ClassInterfaceType Value
        {
            get
            {
                return this._val;
            }
        }
    }
}

