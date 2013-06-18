namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    internal class WeakProxyTearoff : ProxyTearoff, IObjectControl, IObjectConstruct
    {
        private bool _fCanBePooled;
        private WeakReference _scp;

        internal override void Init(ServicedComponentProxy scp)
        {
            this._scp = new WeakReference(scp, true);
        }

        internal override void SetCanBePooled(bool fCanBePooled)
        {
            this._fCanBePooled = fCanBePooled;
        }

        void IObjectConstruct.Construct(object obj)
        {
            ((ServicedComponentProxy) this._scp.Target).DispatchConstruct(((IObjectConstructString) obj).ConstructString);
        }

        void IObjectControl.Activate()
        {
            ((ServicedComponentProxy) this._scp.Target).ActivateObject();
        }

        bool IObjectControl.CanBePooled()
        {
            return this._fCanBePooled;
        }

        void IObjectControl.Deactivate()
        {
            ServicedComponentProxy target = (ServicedComponentProxy) this._scp.Target;
            if (target != null)
            {
                ComponentServices.DeactivateObject(target.GetTransparentProxy(), true);
            }
        }
    }
}

