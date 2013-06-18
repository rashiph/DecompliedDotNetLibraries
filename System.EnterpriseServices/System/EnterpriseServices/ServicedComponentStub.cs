namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    internal class ServicedComponentStub : IManagedObjectInfo
    {
        private WeakReference _scp;

        internal ServicedComponentStub(ServicedComponentProxy scp)
        {
            this.Refresh(scp);
        }

        internal void Refresh(ServicedComponentProxy scp)
        {
            this._scp = new WeakReference(scp, true);
        }

        void IManagedObjectInfo.GetIObjectControl(out IObjectControl pCtrl)
        {
            ServicedComponentProxy target = (ServicedComponentProxy) this._scp.Target;
            pCtrl = target.GetProxyTearoff() as IObjectControl;
        }

        void IManagedObjectInfo.GetIUnknown(out IntPtr pUnk)
        {
            pUnk = ((ServicedComponentProxy) this._scp.Target).GetOuterIUnknown();
        }

        void IManagedObjectInfo.SetInPool(bool fInPool, IntPtr pPooledObject)
        {
            ((ServicedComponentProxy) this._scp.Target).SetInPool(fInPool, pPooledObject);
        }

        void IManagedObjectInfo.SetWrapperStrength(bool bStrong)
        {
            Marshal.ChangeWrapperHandleStrength(((ServicedComponentProxy) this._scp.Target).GetTransparentProxy(), !bStrong);
        }
    }
}

