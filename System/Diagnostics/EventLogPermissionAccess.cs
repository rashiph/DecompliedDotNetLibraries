namespace System.Diagnostics
{
    using System;

    [Flags]
    public enum EventLogPermissionAccess
    {
        Administer = 0x30,
        [Obsolete("This member has been deprecated.  Please use System.Diagnostics.EventLogPermissionAccess.Administer instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        Audit = 10,
        [Obsolete("This member has been deprecated.  Please use System.Diagnostics.EventLogPermissionAccess.Administer instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        Browse = 2,
        [Obsolete("This member has been deprecated.  Please use System.Diagnostics.EventLogPermissionAccess.Write instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        Instrument = 6,
        None = 0,
        Write = 0x10
    }
}

