namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    internal class StaticProxy : ProxyChain
    {
        private Uri m_Proxy;

        internal StaticProxy(Uri destination, Uri proxy) : base(destination)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException("proxy");
            }
            this.m_Proxy = proxy;
        }

        protected override bool GetNextProxy(out Uri proxy)
        {
            proxy = this.m_Proxy;
            if (proxy == null)
            {
                return false;
            }
            this.m_Proxy = null;
            return true;
        }
    }
}

