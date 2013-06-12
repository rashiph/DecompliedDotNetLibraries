namespace System.Runtime.Remoting.Lifetime
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface ISponsor
    {
        [SecurityCritical]
        TimeSpan Renewal(ILease lease);
    }
}

