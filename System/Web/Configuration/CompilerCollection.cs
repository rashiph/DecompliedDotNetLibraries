namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Web.Util;

    [ConfigurationCollection(typeof(Compiler), AddItemName="compiler", CollectionType=ConfigurationElementCollectionType.BasicMap)]
    public sealed class CompilerCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public CompilerCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Compiler();
        }

        public Compiler Get(int index)
        {
            return (Compiler) base.BaseGet(index);
        }

        public Compiler Get(string language)
        {
            return (Compiler) base.BaseGet(language);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Compiler) element).Language;
        }

        public string GetKey(int index)
        {
            return (string) base.BaseGetKey(index);
        }

        public string[] AllKeys
        {
            get
            {
                return System.Web.Util.StringUtil.ObjectArrayToStringArray(base.BaseGetAllKeys());
            }
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
                return "compiler";
            }
        }

        public Compiler this[string language]
        {
            get
            {
                return (Compiler) base.BaseGet(language);
            }
        }

        public Compiler this[int index]
        {
            get
            {
                return (Compiler) base.BaseGet(index);
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

