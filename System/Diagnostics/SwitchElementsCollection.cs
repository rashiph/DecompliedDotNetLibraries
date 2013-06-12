namespace System.Diagnostics
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(SwitchElement))]
    internal class SwitchElementsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new SwitchElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SwitchElement) element).Name;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        public SwitchElement this[string name]
        {
            get
            {
                return (SwitchElement) base.BaseGet(name);
            }
        }
    }
}

