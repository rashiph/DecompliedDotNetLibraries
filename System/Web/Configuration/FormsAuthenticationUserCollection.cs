namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Web.Util;

    [ConfigurationCollection(typeof(FormsAuthenticationUser), AddItemName="user", CollectionType=ConfigurationElementCollectionType.BasicMap)]
    public sealed class FormsAuthenticationUserCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public void Add(FormsAuthenticationUser user)
        {
            this.BaseAdd(user);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FormsAuthenticationUser();
        }

        public FormsAuthenticationUser Get(int index)
        {
            return (FormsAuthenticationUser) base.BaseGet(index);
        }

        public FormsAuthenticationUser Get(string name)
        {
            return (FormsAuthenticationUser) base.BaseGet(name);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FormsAuthenticationUser) element).Name;
        }

        public string GetKey(int index)
        {
            return (string) base.BaseGetKey(index);
        }

        public void Remove(string name)
        {
            base.BaseRemove(name);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public void Set(FormsAuthenticationUser user)
        {
            base.BaseAdd(user, false);
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
                return "user";
            }
        }

        public FormsAuthenticationUser this[string name]
        {
            get
            {
                return (FormsAuthenticationUser) base.BaseGet(name);
            }
        }

        public FormsAuthenticationUser this[int index]
        {
            get
            {
                return (FormsAuthenticationUser) base.BaseGet(index);
            }
            set
            {
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

        protected override bool ThrowOnDuplicate
        {
            get
            {
                return true;
            }
        }
    }
}

