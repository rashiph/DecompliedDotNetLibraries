namespace System.Runtime.InteropServices
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Assembly, Inherited=false), ComVisible(true)]
    public sealed class AutomationProxyAttribute : Attribute
    {
        internal bool _val;

        public AutomationProxyAttribute(bool val)
        {
            this._val = val;
        }

        public bool Value
        {
            get
            {
                return this._val;
            }
        }
    }
}

