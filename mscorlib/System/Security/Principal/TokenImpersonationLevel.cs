namespace System.Security.Principal
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum TokenImpersonationLevel
    {
        None,
        Anonymous,
        Identification,
        Impersonation,
        Delegation
    }
}

