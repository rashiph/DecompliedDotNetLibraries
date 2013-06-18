namespace System.Configuration
{
    using System;
    using System.Reflection;
    using System.Runtime;

    [ConfigurationCollection(typeof(KeyValueConfigurationElement))]
    public class KeyValueConfigurationCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public KeyValueConfigurationCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
            base.internalAddToEnd = true;
        }

        public void Add(KeyValueConfigurationElement keyValue)
        {
            keyValue.Init();
            KeyValueConfigurationElement element = (KeyValueConfigurationElement) base.BaseGet(keyValue.Key);
            if (element == null)
            {
                this.BaseAdd(keyValue);
            }
            else
            {
                element.Value = element.Value + "," + keyValue.Value;
                int index = base.BaseIndexOf(element);
                base.BaseRemoveAt(index);
                this.BaseAdd(index, element);
            }
        }

        public void Add(string key, string value)
        {
            KeyValueConfigurationElement keyValue = new KeyValueConfigurationElement(key, value);
            this.Add(keyValue);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new KeyValueConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((KeyValueConfigurationElement) element).Key;
        }

        public void Remove(string key)
        {
            base.BaseRemove(key);
        }

        public string[] AllKeys
        {
            get
            {
                return StringUtil.ObjectArrayToStringArray(base.BaseGetAllKeys());
            }
        }

        public KeyValueConfigurationElement this[string key]
        {
            get
            {
                return (KeyValueConfigurationElement) base.BaseGet(key);
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

        protected override bool ThrowOnDuplicate
        {
            get
            {
                return false;
            }
        }
    }
}

