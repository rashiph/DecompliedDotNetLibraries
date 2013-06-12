namespace System.Diagnostics
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(SourceElement), AddItemName="source", CollectionType=ConfigurationElementCollectionType.BasicMap)]
    internal class SourceElementsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            SourceElement element = new SourceElement();
            element.Listeners.InitializeDefaultInternal();
            return element;
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SourceElement) element).Name;
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
                return "source";
            }
        }

        public SourceElement this[string name]
        {
            get
            {
                return (SourceElement) base.BaseGet(name);
            }
        }
    }
}

