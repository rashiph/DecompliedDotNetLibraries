namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    public sealed class TagPrefixInfo : ConfigurationElement
    {
        private static readonly ConfigurationProperty _propAssembly = new ConfigurationProperty("assembly", typeof(string), string.Empty, null, null, ConfigurationPropertyOptions.IsAssemblyStringTransformationRequired);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propNamespace = new ConfigurationProperty("namespace", typeof(string), string.Empty, null, null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propSource = new ConfigurationProperty("src", typeof(string), string.Empty, null, null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propTagName = new ConfigurationProperty("tagName", typeof(string), string.Empty, null, null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propTagPrefix = new ConfigurationProperty("tagPrefix", typeof(string), "/", null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationElementProperty s_elemProperty = new ConfigurationElementProperty(new CallbackValidator(typeof(TagPrefixInfo), new ValidatorCallback(TagPrefixInfo.Validate)));

        static TagPrefixInfo()
        {
            _properties.Add(_propTagPrefix);
            _properties.Add(_propTagName);
            _properties.Add(_propNamespace);
            _properties.Add(_propAssembly);
            _properties.Add(_propSource);
        }

        internal TagPrefixInfo()
        {
        }

        public TagPrefixInfo(string tagPrefix, string nameSpace, string assembly, string tagName, string source) : this()
        {
            this.TagPrefix = tagPrefix;
            this.Namespace = nameSpace;
            this.Assembly = assembly;
            this.TagName = tagName;
            this.Source = source;
        }

        public override bool Equals(object prefix)
        {
            TagPrefixInfo info = prefix as TagPrefixInfo;
            return (((System.Web.Util.StringUtil.Equals(this.TagPrefix, info.TagPrefix) && System.Web.Util.StringUtil.Equals(this.TagName, info.TagName)) && (System.Web.Util.StringUtil.Equals(this.Namespace, info.Namespace) && System.Web.Util.StringUtil.Equals(this.Assembly, info.Assembly))) && System.Web.Util.StringUtil.Equals(this.Source, info.Source));
        }

        public override int GetHashCode()
        {
            return ((((this.TagPrefix.GetHashCode() ^ this.TagName.GetHashCode()) ^ this.Namespace.GetHashCode()) ^ this.Assembly.GetHashCode()) ^ this.Source.GetHashCode());
        }

        private static void Validate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("control");
            }
            TagPrefixInfo info = (TagPrefixInfo) value;
            if (Util.ContainsWhiteSpace(info.TagPrefix))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Space_attribute", new object[] { "tagPrefix" }));
            }
            bool flag = false;
            if (!string.IsNullOrEmpty(info.Namespace))
            {
                if (!string.IsNullOrEmpty(info.TagName) || !string.IsNullOrEmpty(info.Source))
                {
                    flag = true;
                }
            }
            else if (!string.IsNullOrEmpty(info.TagName))
            {
                if ((!string.IsNullOrEmpty(info.Namespace) || !string.IsNullOrEmpty(info.Assembly)) || string.IsNullOrEmpty(info.Source))
                {
                    flag = true;
                }
            }
            else
            {
                flag = true;
            }
            if (flag)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_tagprefix_entry"));
            }
        }

        [ConfigurationProperty("assembly")]
        public string Assembly
        {
            get
            {
                return (string) base[_propAssembly];
            }
            set
            {
                base[_propAssembly] = value;
            }
        }

        protected override ConfigurationElementProperty ElementProperty
        {
            get
            {
                return s_elemProperty;
            }
        }

        [ConfigurationProperty("namespace")]
        public string Namespace
        {
            get
            {
                return (string) base[_propNamespace];
            }
            set
            {
                base[_propNamespace] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("src")]
        public string Source
        {
            get
            {
                return (string) base[_propSource];
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    base[_propSource] = value;
                }
                else
                {
                    base[_propSource] = null;
                }
            }
        }

        [ConfigurationProperty("tagName")]
        public string TagName
        {
            get
            {
                return (string) base[_propTagName];
            }
            set
            {
                base[_propTagName] = value;
            }
        }

        [StringValidator(MinLength=1), ConfigurationProperty("tagPrefix", IsRequired=true, DefaultValue="/")]
        public string TagPrefix
        {
            get
            {
                return (string) base[_propTagPrefix];
            }
            set
            {
                base[_propTagPrefix] = value;
            }
        }
    }
}

