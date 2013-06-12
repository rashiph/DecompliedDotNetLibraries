namespace System.Runtime.InteropServices
{
    using System;

    [ComVisible(true), AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, Inherited=false)]
    public sealed class GuidAttribute : Attribute
    {
        internal string _val;

        public GuidAttribute(string guid)
        {
            this._val = guid;
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

