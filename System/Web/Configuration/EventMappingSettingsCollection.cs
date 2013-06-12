namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(EventMappingSettings))]
    public sealed class EventMappingSettingsCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public void Add(EventMappingSettings eventMappingSettings)
        {
            this.BaseAdd(eventMappingSettings);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        public bool Contains(string name)
        {
            return (this.IndexOf(name) != -1);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new EventMappingSettings();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((EventMappingSettings) element).Name;
        }

        public int IndexOf(string name)
        {
            ConfigurationElement element = base.BaseGet(name);
            if (element == null)
            {
                return -1;
            }
            return base.BaseIndexOf(element);
        }

        public void Insert(int index, EventMappingSettings eventMappingSettings)
        {
            this.BaseAdd(index, eventMappingSettings);
        }

        public void Remove(string name)
        {
            base.BaseRemove(name);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public EventMappingSettings this[string key]
        {
            get
            {
                return (EventMappingSettings) base.BaseGet(key);
            }
        }

        public EventMappingSettings this[int index]
        {
            get
            {
                return (EventMappingSettings) base.BaseGet(index);
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

