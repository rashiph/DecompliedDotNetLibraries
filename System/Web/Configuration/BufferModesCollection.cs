namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(BufferModeSettings))]
    public sealed class BufferModesCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public void Add(BufferModeSettings bufferModeSettings)
        {
            this.BaseAdd(bufferModeSettings);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new BufferModeSettings();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((BufferModeSettings) element).Name;
        }

        public void Remove(string s)
        {
            base.BaseRemove(s);
        }

        public BufferModeSettings this[string key]
        {
            get
            {
                return (BufferModeSettings) base.BaseGet(key);
            }
        }

        public BufferModeSettings this[int index]
        {
            get
            {
                return (BufferModeSettings) base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
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

