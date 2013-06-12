namespace System.Diagnostics
{
    using System;
    using System.Configuration;

    internal class AssertSection : ConfigurationElement
    {
        private static readonly ConfigurationProperty _propAssertUIEnabled = new ConfigurationProperty("assertuienabled", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propLogFile = new ConfigurationProperty("logfilename", typeof(string), string.Empty, ConfigurationPropertyOptions.None);

        static AssertSection()
        {
            _properties.Add(_propAssertUIEnabled);
            _properties.Add(_propLogFile);
        }

        [ConfigurationProperty("assertuienabled", DefaultValue=true)]
        public bool AssertUIEnabled
        {
            get
            {
                return (bool) base[_propAssertUIEnabled];
            }
        }

        [ConfigurationProperty("logfilename", DefaultValue="")]
        public string LogFileName
        {
            get
            {
                return (string) base[_propLogFile];
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

