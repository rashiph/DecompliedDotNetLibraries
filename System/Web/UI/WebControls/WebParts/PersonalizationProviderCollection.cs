namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Configuration.Provider;
    using System.Reflection;
    using System.Web;

    public sealed class PersonalizationProviderCollection : ProviderCollection
    {
        public override void Add(ProviderBase provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (!(provider is PersonalizationProvider))
            {
                throw new ArgumentException(System.Web.SR.GetString("Provider_must_implement_the_interface", new object[] { provider.GetType().FullName, "PersonalizationProvider" }));
            }
            base.Add(provider);
        }

        public void CopyTo(PersonalizationProvider[] array, int index)
        {
            base.CopyTo(array, index);
        }

        public PersonalizationProvider this[string name]
        {
            get
            {
                return (PersonalizationProvider) base[name];
            }
        }
    }
}

