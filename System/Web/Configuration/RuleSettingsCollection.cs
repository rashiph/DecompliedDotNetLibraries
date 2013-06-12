namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(RuleSettings))]
    public sealed class RuleSettingsCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public void Add(RuleSettings ruleSettings)
        {
            this.BaseAdd(ruleSettings);
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
            return new RuleSettings();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RuleSettings) element).Name;
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

        public void Insert(int index, RuleSettings eventSettings)
        {
            this.BaseAdd(index, eventSettings);
        }

        public void Remove(string name)
        {
            base.BaseRemove(name);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public RuleSettings this[int index]
        {
            get
            {
                return (RuleSettings) base.BaseGet(index);
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

        public RuleSettings this[string key]
        {
            get
            {
                return (RuleSettings) base.BaseGet(key);
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

