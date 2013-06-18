namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.EnterpriseServices;
    using System.Runtime.InteropServices;

    [Guid("59856830-3ECB-4D29-9CFE-DDD0F74B96A2"), ComVisible(true)]
    public class DllHostInitializer : IProcessInitializer
    {
        private DllHostInitializeWorker worker = new DllHostInitializeWorker();

        public void Shutdown()
        {
            this.worker.Shutdown();
        }

        public void Startup(object punkProcessControl)
        {
            IProcessInitControl control = punkProcessControl as IProcessInitControl;
            this.worker.Startup(control);
        }
    }
}

