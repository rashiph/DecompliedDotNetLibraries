namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web;
    using System.Web.Util;
    using System.Xml;

    public sealed class TagMapInfo : ConfigurationElement
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propMappedTagTypeName = new ConfigurationProperty("mappedTagType", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propTagTypeName = new ConfigurationProperty("tagType", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);

        static TagMapInfo()
        {
            _properties.Add(_propTagTypeName);
            _properties.Add(_propMappedTagTypeName);
        }

        internal TagMapInfo()
        {
        }

        public TagMapInfo(string tagTypeName, string mappedTagTypeName) : this()
        {
            this.TagType = tagTypeName;
            this.MappedTagType = mappedTagTypeName;
        }

        public override bool Equals(object o)
        {
            TagMapInfo info = o as TagMapInfo;
            return (System.Web.Util.StringUtil.Equals(this.TagType, info.TagType) && System.Web.Util.StringUtil.Equals(this.MappedTagType, info.MappedTagType));
        }

        public override int GetHashCode()
        {
            return (this.TagType.GetHashCode() ^ this.MappedTagType.GetHashCode());
        }

        protected override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey)
        {
            this.Verify();
            return base.SerializeElement(writer, serializeCollectionKey);
        }

        private void Verify()
        {
            if (string.IsNullOrEmpty(this.TagType))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_base_required_attribute_missing", new object[] { "tagType" }));
            }
            if (string.IsNullOrEmpty(this.MappedTagType))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_base_required_attribute_missing", new object[] { "mappedTagType" }));
            }
        }

        [ConfigurationProperty("mappedTagType"), StringValidator(MinLength=1)]
        public string MappedTagType
        {
            get
            {
                return (string) base[_propMappedTagTypeName];
            }
            set
            {
                base[_propMappedTagTypeName] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("tagType", IsRequired=true, IsKey=true, DefaultValue=""), StringValidator(MinLength=1)]
        public string TagType
        {
            get
            {
                return (string) base[_propTagTypeName];
            }
            set
            {
                base[_propTagTypeName] = value;
            }
        }
    }
}

