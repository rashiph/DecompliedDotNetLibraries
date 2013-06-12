namespace System.Web
{
    using System;
    using System.Configuration.Provider;
    using System.Reflection;

    public sealed class SiteMapProviderCollection : ProviderCollection
    {
        public override void Add(ProviderBase provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (!(provider is SiteMapProvider))
            {
                throw new ArgumentException(System.Web.SR.GetString("Provider_must_implement_the_interface", new object[] { provider.GetType().Name, typeof(SiteMapProvider).Name }), "provider");
            }
            this.Add((SiteMapProvider) provider);
        }

        public void Add(SiteMapProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            base.Add(provider);
        }

        public void AddArray(SiteMapProvider[] providerArray)
        {
            if (providerArray == null)
            {
                throw new ArgumentNullException("providerArray");
            }
            foreach (SiteMapProvider provider in providerArray)
            {
                if (this[provider.Name] != null)
                {
                    throw new ArgumentException(System.Web.SR.GetString("SiteMapProvider_Multiple_Providers_With_Identical_Name", new object[] { provider.Name }));
                }
                this.Add(provider);
            }
        }

        public SiteMapProvider this[string name]
        {
            get
            {
                return (SiteMapProvider) base[name];
            }
        }
    }
}

