namespace System.EnterpriseServices
{
    using System;
    using System.EnterpriseServices.Thunk;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Proxies;

    internal class SCUnMarshaler
    {
        private bool _fUnMarshaled;
        private RealProxy _rp;
        private byte[] buffer;
        private Type servertype;

        internal SCUnMarshaler(Type _servertype, byte[] _buffer)
        {
            this.buffer = _buffer;
            this.servertype = _servertype;
            this._rp = null;
            this._fUnMarshaled = false;
        }

        internal void Dispose()
        {
            if (!this._fUnMarshaled && (this.buffer != null))
            {
                Proxy.ReleaseMarshaledObject(this.buffer);
            }
        }

        internal RealProxy GetRealProxy()
        {
            if ((this._rp == null) && !this._fUnMarshaled)
            {
                this._rp = this.UnmarshalRemoteReference();
            }
            return this._rp;
        }

        private RealProxy UnmarshalRemoteReference()
        {
            IntPtr zero = IntPtr.Zero;
            RealProxy proxy = null;
            try
            {
                this._fUnMarshaled = true;
                if (this.buffer != null)
                {
                    zero = Proxy.UnmarshalObject(this.buffer);
                }
                proxy = new RemoteServicedComponentProxy(this.servertype, zero, false);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.Release(zero);
                }
                this.buffer = null;
            }
            return proxy;
        }
    }
}

