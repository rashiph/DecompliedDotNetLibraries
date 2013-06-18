namespace System.Configuration
{
    using System;
    using System.Reflection;
    using System.Runtime;

    [ConfigurationCollection(typeof(NameValueConfigurationElement))]
    public sealed class NameValueConfigurationCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public void Add(NameValueConfigurationElement nameValue)
        {
            this.BaseAdd(nameValue);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new NameValueConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NameValueConfigurationElement) element).Name;
        }

        public void Remove(NameValueConfigurationElement nameValue)
        {
            if (base.BaseIndexOf(nameValue) >= 0)
            {
                base.BaseRemove(nameValue.Name);
            }
        }

        public void Remove(string name)
        {
            base.BaseRemove(name);
        }

        public string[] AllKeys
        {
            get
            {
                return StringUtil.ObjectArrayToStringArray(base.BaseGetAllKeys());
            }
        }

        public NameValueConfigurationElement this[string name]
        {
            get
            {
                return (NameValueConfigurationElement) base.BaseGet(name);
            }
            set
            {
                int index = -1;
                NameValueConfigurationElement element = (NameValueConfigurationElement) base.BaseGet(name);
                if (element != null)
                {
                    index = base.BaseIndexOf(element);
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        protected internal override ConfigurationPropertyCollection Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return _properties;
            }
        }
    }
}

