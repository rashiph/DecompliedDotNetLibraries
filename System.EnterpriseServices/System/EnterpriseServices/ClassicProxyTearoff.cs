namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    internal class ClassicProxyTearoff : ProxyTearoff, IObjectControl, IObjectConstruct
    {
        private bool _fCanBePooled;
        private ServicedComponentProxy _scp;

        internal override void Init(ServicedComponentProxy scp)
        {
            this._scp = scp;
        }

        internal override void SetCanBePooled(bool fCanBePooled)
        {
            this._fCanBePooled = fCanBePooled;
        }

        void IObjectConstruct.Construct(object obj)
        {
            this._scp.DispatchConstruct(((IObjectConstructString) obj).ConstructString);
        }

        void IObjectControl.Activate()
        {
            this._scp.ActivateObject();
        }

        bool IObjectControl.CanBePooled()
        {
            return this._fCanBePooled;
        }

        void IObjectControl.Deactivate()
        {
            ComponentServices.DeactivateObject(this._scp.GetTransparentProxy(), true);
        }
    }
}

