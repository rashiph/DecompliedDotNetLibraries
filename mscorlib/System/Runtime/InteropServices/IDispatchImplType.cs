namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, Obsolete("The IDispatchImplAttribute is deprecated.", false), ComVisible(true)]
    public enum IDispatchImplType
    {
        SystemDefinedImpl,
        InternalImpl,
        CompatibleImpl
    }
}

