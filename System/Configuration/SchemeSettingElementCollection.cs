namespace System.Configuration
{
    using System;
    using System.Reflection;

    [ConfigurationCollection(typeof(SchemeSettingElementCollection), CollectionType=ConfigurationElementCollectionType.AddRemoveClearMap, AddItemName="add", ClearItemsName="clear", RemoveItemName="remove")]
    public sealed class SchemeSettingElementCollection : ConfigurationElementCollection
    {
        internal const string AddItemName = "add";
        internal const string ClearItemsName = "clear";
        internal const string RemoveItemName = "remove";

        public SchemeSettingElementCollection()
        {
            base.AddElementName = "add";
            base.ClearElementName = "clear";
            base.RemoveElementName = "remove";
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SchemeSettingElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SchemeSettingElement) element).Name;
        }

        public int IndexOf(SchemeSettingElement element)
        {
            return base.BaseIndexOf(element);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        public SchemeSettingElement this[int index]
        {
            get
            {
                return (SchemeSettingElement) base.BaseGet(index);
            }
        }

        public SchemeSettingElement this[string name]
        {
            get
            {
                return (SchemeSettingElement) base.BaseGet(name);
            }
        }
    }
}

