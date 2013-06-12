namespace System.Runtime.InteropServices
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited=false), Obsolete("This attribute is deprecated and will be removed in a future version.", false), ComVisible(true)]
    public sealed class IDispatchImplAttribute : Attribute
    {
        internal IDispatchImplType _val;

        public IDispatchImplAttribute(short implType)
        {
            this._val = (IDispatchImplType) implType;
        }

        public IDispatchImplAttribute(IDispatchImplType implType)
        {
            this._val = implType;
        }

        public IDispatchImplType Value
        {
            get
            {
                return this._val;
            }
        }
    }
}

