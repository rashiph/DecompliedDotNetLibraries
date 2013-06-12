namespace System.Runtime.InteropServices
{
    using System;

    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, Inherited=false), ComVisible(true)]
    public sealed class DispIdAttribute : Attribute
    {
        internal int _val;

        public DispIdAttribute(int dispId)
        {
            this._val = dispId;
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

