namespace System.Configuration
{
    using System;

    public sealed class SettingElementCollection : ConfigurationElementCollection
    {
        public void Add(SettingElement element)
        {
            this.BaseAdd(element);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SettingElement();
        }

        public SettingElement Get(string elementKey)
        {
            return (SettingElement) base.BaseGet(elementKey);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SettingElement) element).Key;
        }

        public void Remove(SettingElement element)
        {
            base.BaseRemove(this.GetElementKey(element));
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "setting";
            }
        }
    }
}

