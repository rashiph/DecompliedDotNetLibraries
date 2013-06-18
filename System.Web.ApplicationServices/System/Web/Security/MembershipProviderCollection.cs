namespace System.Web.Security
{
    using System;
    using System.Configuration.Provider;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Web;

    [TypeForwardedFrom("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public sealed class MembershipProviderCollection : ProviderCollection
    {
        public override void Add(ProviderBase provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (!(provider is MembershipProvider))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ApplicationServicesStrings.Provider_must_implement_type, new object[] { typeof(MembershipProvider).ToString() }), "provider");
            }
            base.Add(provider);
        }

        public void CopyTo(MembershipProvider[] array, int index)
        {
            base.CopyTo(array, index);
        }

        public MembershipProvider this[string name]
        {
            get
            {
                return (MembershipProvider) base[name];
            }
        }
    }
}

