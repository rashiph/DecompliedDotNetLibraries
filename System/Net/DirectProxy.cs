namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    internal class DirectProxy : ProxyChain
    {
        private bool m_ProxyRetrieved;

        internal DirectProxy(Uri destination) : base(destination)
        {
        }

        protected override bool GetNextProxy(out Uri proxy)
        {
            proxy = null;
            if (this.m_ProxyRetrieved)
            {
                return false;
            }
            this.m_ProxyRetrieved = true;
            return true;
        }
    }
}

