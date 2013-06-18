namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Web;

    internal abstract class HostedTransportConfigurationBase : HostedTransportConfiguration
    {
        private List<BaseUriWithWildcard> listenAddresses;
        private string scheme;

        protected internal HostedTransportConfigurationBase(string scheme)
        {
            this.scheme = scheme;
            this.listenAddresses = new List<BaseUriWithWildcard>();
        }

        internal BaseUriWithWildcard FindBaseAddress(Uri uri)
        {
            BaseUriWithWildcard wildcard = null;
            BaseUriWithWildcard wildcard2 = null;
            for (int i = 0; i < this.listenAddresses.Count; i++)
            {
                if ((string.Compare(this.listenAddresses[i].BaseAddress.Scheme, uri.Scheme, StringComparison.OrdinalIgnoreCase) == 0) && (this.listenAddresses[i].BaseAddress.Port == uri.Port))
                {
                    if (this.listenAddresses[i].HostNameComparisonMode == HostNameComparisonMode.StrongWildcard)
                    {
                        return this.listenAddresses[i];
                    }
                    if (this.listenAddresses[i].HostNameComparisonMode == HostNameComparisonMode.WeakWildcard)
                    {
                        wildcard2 = this.listenAddresses[i];
                    }
                    if ((this.listenAddresses[i].HostNameComparisonMode == HostNameComparisonMode.Exact) && (string.Compare(this.listenAddresses[i].BaseAddress.Host, uri.Host, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        wildcard = this.listenAddresses[i];
                    }
                }
            }
            if (wildcard == null)
            {
                wildcard = wildcard2;
            }
            return wildcard;
        }

        public override Uri[] GetBaseAddresses(string virtualPath)
        {
            Uri[] uriArray = new Uri[this.listenAddresses.Count];
            for (int i = 0; i < this.listenAddresses.Count; i++)
            {
                string relativeUri = VirtualPathUtility.ToAbsolute(virtualPath, HostingEnvironmentWrapper.ApplicationVirtualPath);
                uriArray[i] = new Uri(this.listenAddresses[i].BaseAddress, relativeUri);
            }
            return uriArray;
        }

        protected internal IList<BaseUriWithWildcard> ListenAddresses
        {
            get
            {
                return this.listenAddresses;
            }
        }

        internal string Scheme
        {
            get
            {
                return this.scheme;
            }
        }
    }
}

