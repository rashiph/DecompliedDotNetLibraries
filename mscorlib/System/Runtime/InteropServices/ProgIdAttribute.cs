namespace System.Runtime.InteropServices
{
    using System;

    [ComVisible(true), AttributeUsage(AttributeTargets.Class, Inherited=false)]
    public sealed class ProgIdAttribute : Attribute
    {
        internal string _val;

        public ProgIdAttribute(string progId)
        {
            this._val = progId;
        }

        public string Value
        {
            get
            {
                return this._val;
            }
        }
    }
}

