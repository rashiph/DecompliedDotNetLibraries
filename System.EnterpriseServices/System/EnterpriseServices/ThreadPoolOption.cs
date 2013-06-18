namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(false)]
    public enum ThreadPoolOption
    {
        None,
        Inherit,
        STA,
        MTA
    }
}

