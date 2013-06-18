namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Web;
    using System.Web.Util;
    using System.Xml;

    [ConfigurationCollection(typeof(ProfilePropertySettings))]
    public class ProfilePropertySettingsCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public void Add(ProfilePropertySettings propertySettings)
        {
            this.BaseAdd(propertySettings);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ProfilePropertySettings();
        }

        public ProfilePropertySettings Get(int index)
        {
            return (ProfilePropertySettings) base.BaseGet(index);
        }

        public ProfilePropertySettings Get(string name)
        {
            return (ProfilePropertySettings) base.BaseGet(name);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProfilePropertySettings) element).Name;
        }

        public string GetKey(int index)
        {
            return (string) base.BaseGetKey(index);
        }

        public int IndexOf(ProfilePropertySettings propertySettings)
        {
            return base.BaseIndexOf(propertySettings);
        }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            if (!this.AllowClear && (elementName == "clear"))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Clear_not_valid"), reader);
            }
            if (elementName == "group")
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Nested_group_not_valid"), reader);
            }
            return base.OnDeserializeUnrecognizedElement(elementName, reader);
        }

        public void Remove(string name)
        {
            base.BaseRemove(name);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public void Set(ProfilePropertySettings propertySettings)
        {
            base.BaseAdd(propertySettings, false);
        }

        public string[] AllKeys
        {
            get
            {
                return System.Web.Util.StringUtil.ObjectArrayToStringArray(base.BaseGetAllKeys());
            }
        }

        protected virtual bool AllowClear
        {
            get
            {
                return false;
            }
        }

        public ProfilePropertySettings this[string name]
        {
            get
            {
                return (ProfilePropertySettings) base.BaseGet(name);
            }
        }

        public ProfilePropertySettings this[int index]
        {
            get
            {
                return (ProfilePropertySettings) base.BaseGet(index);
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

