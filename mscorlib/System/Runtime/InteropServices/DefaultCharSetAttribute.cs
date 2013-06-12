namespace System.Runtime.InteropServices
{
    using System;

    [AttributeUsage(AttributeTargets.Module, Inherited=false), ComVisible(true)]
    public sealed class DefaultCharSetAttribute : Attribute
    {
        internal System.Runtime.InteropServices.CharSet _CharSet;

        public DefaultCharSetAttribute(System.Runtime.InteropServices.CharSet charSet)
        {
            this._CharSet = charSet;
        }

        public System.Runtime.InteropServices.CharSet CharSet
        {
            get
            {
                return this._CharSet;
            }
        }
    }
}

