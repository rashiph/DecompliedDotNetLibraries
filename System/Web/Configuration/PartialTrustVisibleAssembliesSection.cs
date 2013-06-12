namespace System.Web.Configuration
{
    using System;
    using System.Configuration;

    public sealed class PartialTrustVisibleAssembliesSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propPartialTrustVisibleAssemblies = new ConfigurationProperty(null, typeof(PartialTrustVisibleAssemblyCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        static PartialTrustVisibleAssembliesSection()
        {
            _properties.Add(_propPartialTrustVisibleAssemblies);
        }

        private PartialTrustVisibleAssemblyCollection GetPartialTrustVisibleAssembliesCollection()
        {
            return (PartialTrustVisibleAssemblyCollection) base[_propPartialTrustVisibleAssemblies];
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public PartialTrustVisibleAssemblyCollection PartialTrustVisibleAssemblies
        {
            get
            {
                return this.GetPartialTrustVisibleAssembliesCollection();
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

