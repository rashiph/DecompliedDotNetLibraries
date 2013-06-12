namespace System.Runtime.InteropServices
{
    using System;

    [AttributeUsage(AttributeTargets.Method, Inherited=false), ComVisible(true)]
    public sealed class LCIDConversionAttribute : Attribute
    {
        internal int _val;

        public LCIDConversionAttribute(int lcid)
        {
            this._val = lcid;
        }

        public int Value
        {
            get
            {
                return this._val;
            }
        }
    }
}

