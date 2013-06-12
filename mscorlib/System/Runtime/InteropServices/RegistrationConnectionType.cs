namespace System.Runtime.InteropServices
{
    using System;

    [Flags]
    public enum RegistrationConnectionType
    {
        MultipleUse = 1,
        MultiSeparate = 2,
        SingleUse = 0,
        Surrogate = 8,
        Suspended = 4
    }
}

