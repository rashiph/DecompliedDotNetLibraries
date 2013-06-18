namespace System.EnterpriseServices.Internal
{
    using System;
    using System.EnterpriseServices.Thunk;
    using System.Runtime.InteropServices;

    [Guid("ef24f689-14f8-4d92-b4af-d7b1f0e70fd4")]
    public class AppDomainHelper : IAppDomainHelper
    {
        private AppDomain _ad;
        private IntPtr _pfnShutdownCB;
        private IntPtr _punkPool;

        ~AppDomainHelper()
        {
            if (this._punkPool != IntPtr.Zero)
            {
                Marshal.Release(this._punkPool);
                this._punkPool = IntPtr.Zero;
            }
        }

        private void OnDomainUnload(object sender, EventArgs e)
        {
            if (this._pfnShutdownCB != IntPtr.Zero)
            {
                Proxy.CallFunction(this._pfnShutdownCB, this._punkPool);
                this._pfnShutdownCB = IntPtr.Zero;
                Marshal.Release(this._punkPool);
                this._punkPool = IntPtr.Zero;
            }
        }

        void IAppDomainHelper.DoCallback(IntPtr pUnkAD, IntPtr pfnCallbackCB, IntPtr data)
        {
            CallbackWrapper wrapper = new CallbackWrapper(pfnCallbackCB, data);
            if (this._ad != AppDomain.CurrentDomain)
            {
                this._ad.DoCallBack(new CrossAppDomainDelegate(wrapper.ReceiveCallback));
            }
            else
            {
                wrapper.ReceiveCallback();
            }
        }

        void IAppDomainHelper.Initialize(IntPtr pUnkAD, IntPtr pfnShutdownCB, IntPtr punkPool)
        {
            this._ad = (AppDomain) Marshal.GetObjectForIUnknown(pUnkAD);
            this._pfnShutdownCB = pfnShutdownCB;
            this._punkPool = punkPool;
            Marshal.AddRef(this._punkPool);
            this._ad.DomainUnload += new EventHandler(this.OnDomainUnload);
        }

        private class CallbackWrapper
        {
            private IntPtr _pfnCB;
            private IntPtr _pv;

            public CallbackWrapper(IntPtr pfnCB, IntPtr pv)
            {
                this._pfnCB = pfnCB;
                this._pv = pv;
            }

            public void ReceiveCallback()
            {
                int errorCode = Proxy.CallFunction(this._pfnCB, this._pv);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
        }
    }
}

