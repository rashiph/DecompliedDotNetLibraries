namespace System.Web.Caching
{
    using System;
    using System.Configuration.Provider;
    using System.Reflection;
    using System.Web;

    public sealed class OutputCacheProviderCollection : ProviderCollection
    {
        public override void Add(ProviderBase provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (!(provider is OutputCacheProvider))
            {
                throw new ArgumentException(System.Web.SR.GetString("Provider_must_implement_type", new object[] { typeof(OutputCacheProvider).Name }), "provider");
            }
            base.Add(provider);
        }

        public void CopyTo(OutputCacheProvider[] array, int index)
        {
            base.CopyTo(array, index);
        }

        public OutputCacheProvider this[string name]
        {
            get
            {
                return (OutputCacheProvider) base[name];
            }
        }
    }
}

