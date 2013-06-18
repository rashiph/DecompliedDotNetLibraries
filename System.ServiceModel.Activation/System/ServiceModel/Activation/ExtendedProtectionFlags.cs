namespace System.ServiceModel.Activation
{
    using System;

    [Flags]
    internal enum ExtendedProtectionFlags
    {
        AllowDotlessSpn = 4,
        None = 0,
        NoServiceNameCheck = 2,
        Proxy = 1,
        ProxyCohosting = 0x20
    }
}

