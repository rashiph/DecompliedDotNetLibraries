namespace System.Runtime.InteropServices
{
    using System;

    [ComVisible(true), AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, Inherited=false)]
    public sealed class ComVisibleAttribute : Attribute
    {
        internal bool _val;

        public ComVisibleAttribute(bool visibility)
        {
            this._val = visibility;
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

