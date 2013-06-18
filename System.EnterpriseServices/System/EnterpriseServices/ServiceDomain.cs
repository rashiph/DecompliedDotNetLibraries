namespace System.EnterpriseServices
{
    using System;
    using System.EnterpriseServices.Thunk;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public sealed class ServiceDomain
    {
        private const int S_OK = 0;
        private const int XACT_E_ABORTED = -2147168231;
        private const int XACT_E_ABORTING = -2147168215;
        private const int XACT_E_NOTRANSACTION = -2147168242;
        private const int XACT_S_LOCALLY_OK = 0x4d00a;

        private ServiceDomain()
        {
        }

        public static void Enter(ServiceConfig cfg)
        {
            ServiceDomainThunk.EnterServiceDomain(cfg.SCT);
        }

        public static System.EnterpriseServices.TransactionStatus Leave()
        {
            int errorCode = ServiceDomainThunk.LeaveServiceDomain();
            switch (errorCode)
            {
                case -2147168242:
                    return System.EnterpriseServices.TransactionStatus.NoTransaction;

                case -2147168231:
                    return System.EnterpriseServices.TransactionStatus.Aborted;

                case -2147168215:
                    return System.EnterpriseServices.TransactionStatus.Aborting;

                case 0:
                    return System.EnterpriseServices.TransactionStatus.Commited;

                case 0x4d00a:
                    return System.EnterpriseServices.TransactionStatus.LocallyOk;
            }
            Marshal.ThrowExceptionForHR(errorCode);
            return System.EnterpriseServices.TransactionStatus.Commited;
        }
    }
}

