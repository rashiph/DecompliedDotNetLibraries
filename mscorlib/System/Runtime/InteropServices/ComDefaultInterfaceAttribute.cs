namespace System.Runtime.InteropServices
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited=false), ComVisible(true)]
    public sealed class ComDefaultInterfaceAttribute : Attribute
    {
        internal Type _val;

        public ComDefaultInterfaceAttribute(Type defaultInterface)
        {
            this._val = defaultInterface;
        }

        public Type Value
        {
            get
            {
                return this._val;
            }
        }
    }
}

