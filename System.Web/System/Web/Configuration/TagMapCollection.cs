namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Reflection;
    using System.Web;

    [ConfigurationCollection(typeof(TagMapInfo))]
    public sealed class TagMapCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private Hashtable _tagMappings;

        public void Add(TagMapInfo tagMapInformation)
        {
            this.BaseAdd(tagMapInformation);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TagMapInfo();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TagMapInfo) element).TagType;
        }

        public void Remove(TagMapInfo tagMapInformation)
        {
            base.BaseRemove(this.GetElementKey(tagMapInformation));
        }

        public TagMapInfo this[int index]
        {
            get
            {
                return (TagMapInfo) base.BaseGet(index);
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

        internal Hashtable TagTypeMappingInternal
        {
            get
            {
                if (this._tagMappings == null)
                {
                    lock (this)
                    {
                        if (this._tagMappings == null)
                        {
                            Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
                            foreach (TagMapInfo info in this)
                            {
                                Type type = ConfigUtil.GetType(info.TagType, "tagType", info);
                                Type c = ConfigUtil.GetType(info.MappedTagType, "mappedTagType", info);
                                if (!type.IsAssignableFrom(c))
                                {
                                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Mapped_type_must_inherit", new object[] { info.MappedTagType, info.TagType }), info.ElementInformation.Properties["mappedTagType"].Source, info.ElementInformation.Properties["mappedTagType"].LineNumber);
                                }
                                hashtable[type] = c;
                            }
                            this._tagMappings = hashtable;
                        }
                    }
                }
                return this._tagMappings;
            }
        }
    }
}

