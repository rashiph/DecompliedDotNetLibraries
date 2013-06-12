namespace System.Runtime.InteropServices
{
    using System;

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, Inherited=false), ComVisible(true)]
    public sealed class ComAliasNameAttribute : Attribute
    {
        internal string _val;

        public ComAliasNameAttribute(string alias)
        {
            this._val = alias;
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

