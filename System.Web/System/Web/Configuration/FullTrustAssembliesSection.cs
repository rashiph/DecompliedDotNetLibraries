namespace System.Web.Configuration
{
    using System;
    using System.Configuration;

    public sealed class FullTrustAssembliesSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propFullTrustAssemblies = new ConfigurationProperty(null, typeof(FullTrustAssemblyCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        static FullTrustAssembliesSection()
        {
            _properties.Add(_propFullTrustAssemblies);
        }

        private FullTrustAssemblyCollection GetFullTrustAssembliesCollection()
        {
            return (FullTrustAssemblyCollection) base[_propFullTrustAssemblies];
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public FullTrustAssemblyCollection FullTrustAssemblies
        {
            get
            {
                return this.GetFullTrustAssembliesCollection();
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }
}

