namespace Microsoft.JScript
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("581AD3D9-2BAA-3770-B92B-38607E1B463A")]
    public enum VSAITEMTYPE2
    {
        EXPRESSION = 0x16,
        HOSTOBJECT = 0x10,
        HOSTSCOPE = 0x11,
        HOSTSCOPEANDOBJECT = 0x12,
        None = 0,
        SCRIPTBLOCK = 20,
        SCRIPTSCOPE = 0x13,
        STATEMENT = 0x15
    }
}

