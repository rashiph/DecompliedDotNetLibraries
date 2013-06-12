namespace System.Web.Configuration
{
    using System;
    using System.Configuration;

    public sealed class XhtmlConformanceSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propMode = new ConfigurationProperty("mode", typeof(XhtmlConformanceMode), XhtmlConformanceMode.Transitional, ConfigurationPropertyOptions.None);
        internal const XhtmlConformanceMode DefaultMode = XhtmlConformanceMode.Transitional;

        static XhtmlConformanceSection()
        {
            _properties.Add(_propMode);
        }

        [ConfigurationProperty("mode", DefaultValue=0)]
        public XhtmlConformanceMode Mode
        {
            get
            {
                return (XhtmlConformanceMode) base[_propMode];
            }
            set
            {
                base[_propMode] = value;
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

