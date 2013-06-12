namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, ComVisible(true)]
    public enum ComInterfaceType
    {
        InterfaceIsDual,
        InterfaceIsIUnknown,
        InterfaceIsIDispatch
    }
}

