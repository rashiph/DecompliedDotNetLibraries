namespace System.Diagnostics
{
    using System;
    using System.Configuration;

    internal class PerfCounterSection : ConfigurationElement
    {
        private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propFileMappingSize = new ConfigurationProperty("filemappingsize", typeof(int), 0x80000, ConfigurationPropertyOptions.None);

        static PerfCounterSection()
        {
            _properties.Add(_propFileMappingSize);
        }

        [ConfigurationProperty("filemappingsize", DefaultValue=0x80000)]
        public int FileMappingSize
        {
            get
            {
                return (int) base[_propFileMappingSize];
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

