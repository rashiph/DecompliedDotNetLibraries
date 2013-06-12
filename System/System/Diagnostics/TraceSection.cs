namespace System.Diagnostics
{
    using System;
    using System.Configuration;

    internal class TraceSection : ConfigurationElement
    {
        private static readonly ConfigurationProperty _propAutoFlush = new ConfigurationProperty("autoflush", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propIndentSize = new ConfigurationProperty("indentsize", typeof(int), 4, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propListeners = new ConfigurationProperty("listeners", typeof(ListenerElementsCollection), new ListenerElementsCollection(), ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propUseGlobalLock = new ConfigurationProperty("useGlobalLock", typeof(bool), true, ConfigurationPropertyOptions.None);

        static TraceSection()
        {
            _properties.Add(_propListeners);
            _properties.Add(_propAutoFlush);
            _properties.Add(_propIndentSize);
            _properties.Add(_propUseGlobalLock);
        }

        [ConfigurationProperty("autoflush", DefaultValue=false)]
        public bool AutoFlush
        {
            get
            {
                return (bool) base[_propAutoFlush];
            }
        }

        [ConfigurationProperty("indentsize", DefaultValue=4)]
        public int IndentSize
        {
            get
            {
                return (int) base[_propIndentSize];
            }
        }

        [ConfigurationProperty("listeners")]
        public ListenerElementsCollection Listeners
        {
            get
            {
                return (ListenerElementsCollection) base[_propListeners];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("useGlobalLock", DefaultValue=true)]
        public bool UseGlobalLock
        {
            get
            {
                return (bool) base[_propUseGlobalLock];
            }
        }
    }
}

