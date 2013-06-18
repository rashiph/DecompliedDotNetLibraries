namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal class ServiceProvider : IServiceProvider
    {
        private static readonly Guid IID_IUnknown = new Guid("{00000000-0000-0000-C000-000000000046}");
        private IOleServiceProvider serviceProvider;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ServiceProvider(IOleServiceProvider sp)
        {
            this.serviceProvider = sp;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }
            IntPtr zero = IntPtr.Zero;
            Guid gUID = serviceType.GUID;
            Guid riid = IID_IUnknown;
            int num = this.serviceProvider.QueryService(ref gUID, ref riid, out zero);
            object objectForIUnknown = null;
            if (num >= 0)
            {
                try
                {
                    objectForIUnknown = Marshal.GetObjectForIUnknown(zero);
                }
                finally
                {
                    Marshal.Release(zero);
                }
            }
            return objectForIUnknown;
        }
    }
}

