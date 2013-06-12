namespace System.Diagnostics
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(ListenerElement), AddItemName="add", CollectionType=ConfigurationElementCollectionType.BasicMap)]
    internal class SharedListenerElementsCollection : ListenerElementsCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ListenerElement(false);
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
                return "add";
            }
        }
    }
}

