namespace System.Web.Security
{
    using System;
    using System.Configuration.Provider;
    using System.Reflection;
    using System.Web;

    public sealed class RoleProviderCollection : ProviderCollection
    {
        public override void Add(ProviderBase provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (!(provider is RoleProvider))
            {
                throw new ArgumentException(System.Web.SR.GetString("Provider_must_implement_type", new object[] { typeof(RoleProvider).ToString() }), "provider");
            }
            base.Add(provider);
        }

        public void CopyTo(RoleProvider[] array, int index)
        {
            base.CopyTo(array, index);
        }

        public RoleProvider this[string name]
        {
            get
            {
                return (RoleProvider) base[name];
            }
        }
    }
}

