namespace System.Web.Services.Protocols
{
    using System;

    [Flags]
    public enum SoapHeaderDirection
    {
        Fault = 4,
        In = 1,
        InOut = 3,
        Out = 2
    }
}

