namespace System.Net
{
    using System;

    [Serializable]
    internal sealed class EmptyWebProxy : IAutoWebProxy, IWebProxy
    {
        [NonSerialized]
        private ICredentials m_credentials;

        public Uri GetProxy(Uri uri)
        {
            return uri;
        }

        public bool IsBypassed(Uri uri)
        {
            return true;
        }

        ProxyChain IAutoWebProxy.GetProxies(Uri destination)
        {
            return new DirectProxy(destination);
        }

        public ICredentials Credentials
        {
            get
            {
                return this.m_credentials;
            }
            set
            {
                this.m_credentials = value;
            }
        }
    }
}

