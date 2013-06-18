namespace System.EnterpriseServices
{
    using System;
    using System.EnterpriseServices.Thunk;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public sealed class Activity
    {
        private ServiceActivityThunk m_sat;

        public Activity(ServiceConfig cfg)
        {
            this.m_sat = new ServiceActivityThunk(cfg.SCT);
        }

        public void AsynchronousCall(System.EnterpriseServices.IServiceCall serviceCall)
        {
            this.m_sat.AsynchronousCall(serviceCall);
        }

        public void BindToCurrentThread()
        {
            this.m_sat.BindToCurrentThread();
        }

        public void SynchronousCall(System.EnterpriseServices.IServiceCall serviceCall)
        {
            this.m_sat.SynchronousCall(serviceCall);
        }

        public void UnbindFromThread()
        {
            this.m_sat.UnbindFromThread();
        }
    }
}

