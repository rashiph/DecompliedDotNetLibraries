namespace System.Runtime.Remoting.Lifetime
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface ILease
    {
        [SecurityCritical]
        void Register(ISponsor obj);
        [SecurityCritical]
        void Register(ISponsor obj, TimeSpan renewalTime);
        [SecurityCritical]
        TimeSpan Renew(TimeSpan renewalTime);
        [SecurityCritical]
        void Unregister(ISponsor obj);

        TimeSpan CurrentLeaseTime { [SecurityCritical] get; }

        LeaseState CurrentState { [SecurityCritical] get; }

        TimeSpan InitialLeaseTime { [SecurityCritical] get; [SecurityCritical] set; }

        TimeSpan RenewOnCallTime { [SecurityCritical] get; [SecurityCritical] set; }

        TimeSpan SponsorshipTimeout { [SecurityCritical] get; [SecurityCritical] set; }
    }
}

