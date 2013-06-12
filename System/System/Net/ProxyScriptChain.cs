namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    internal class ProxyScriptChain : ProxyChain
    {
        private int m_CurrentIndex;
        private WebProxy m_Proxy;
        private Uri[] m_ScriptProxies;
        private int m_SyncStatus;

        internal ProxyScriptChain(WebProxy proxy, Uri destination) : base(destination)
        {
            this.m_Proxy = proxy;
        }

        internal override void Abort()
        {
            this.m_Proxy.AbortGetProxiesAuto(ref this.m_SyncStatus);
        }

        protected override bool GetNextProxy(out Uri proxy)
        {
            if (this.m_CurrentIndex < 0)
            {
                proxy = null;
                return false;
            }
            if (this.m_CurrentIndex == 0)
            {
                this.m_ScriptProxies = this.m_Proxy.GetProxiesAuto(base.Destination, ref this.m_SyncStatus);
            }
            if ((this.m_ScriptProxies == null) || (this.m_CurrentIndex >= this.m_ScriptProxies.Length))
            {
                proxy = this.m_Proxy.GetProxyAutoFailover(base.Destination);
                this.m_CurrentIndex = -1;
                return true;
            }
            proxy = this.m_ScriptProxies[this.m_CurrentIndex++];
            return true;
        }
    }
}

