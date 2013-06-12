namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Util;
    using System.Xml;

    public sealed class GlobalizationSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _propCulture = new ConfigurationProperty("culture", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableBestFitResponseEncoding = new ConfigurationProperty("enableBestFitResponseEncoding", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableClientBasedCulture = new ConfigurationProperty("enableClientBasedCulture", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propFileEncoding = new ConfigurationProperty("fileEncoding", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRequestEncoding = new ConfigurationProperty("requestEncoding", typeof(string), Encoding.UTF8.WebName, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propResourceProviderFactoryType = new ConfigurationProperty("resourceProviderFactoryType", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propResponseEncoding = new ConfigurationProperty("responseEncoding", typeof(string), Encoding.UTF8.WebName, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propResponseHeaderEncoding = new ConfigurationProperty("responseHeaderEncoding", typeof(string), Encoding.UTF8.WebName, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propUICulture = new ConfigurationProperty("uiCulture", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private Type _resourceProviderFactoryType;
        private string cultureCache;
        private Encoding fileEncodingCache;
        private Encoding requestEncodingCache;
        private Encoding responseEncodingCache;
        private Encoding responseHeaderEncodingCache;
        private string uiCultureCache;

        static GlobalizationSection()
        {
            _properties.Add(_propRequestEncoding);
            _properties.Add(_propResponseEncoding);
            _properties.Add(_propFileEncoding);
            _properties.Add(_propCulture);
            _properties.Add(_propUICulture);
            _properties.Add(_propEnableClientBasedCulture);
            _properties.Add(_propResponseHeaderEncoding);
            _properties.Add(_propResourceProviderFactoryType);
            _properties.Add(_propEnableBestFitResponseEncoding);
        }

        private void CheckCulture(string configCulture)
        {
            if (!System.Web.Util.StringUtil.EqualsIgnoreCase(configCulture, HttpApplication.AutoCulture))
            {
                if (System.Web.Util.StringUtil.StringStartsWithIgnoreCase(configCulture, HttpApplication.AutoCulture))
                {
                    new CultureInfo(configCulture.Substring(5));
                }
                else
                {
                    new CultureInfo(configCulture);
                }
            }
        }

        protected override void PostDeserialize()
        {
            ConfigurationPropertyCollection properties = this.Properties;
            ConfigurationProperty property = null;
            int lineNumber = 0x7fffffff;
            try
            {
                if (!string.IsNullOrEmpty((string) base[_propResponseEncoding]))
                {
                    this.responseEncodingCache = Encoding.GetEncoding((string) base[_propResponseEncoding]);
                }
            }
            catch
            {
                property = _propResponseEncoding;
                lineNumber = base.ElementInformation.Properties[property.Name].LineNumber;
            }
            try
            {
                if (!string.IsNullOrEmpty((string) base[_propResponseHeaderEncoding]))
                {
                    this.responseHeaderEncodingCache = Encoding.GetEncoding((string) base[_propResponseHeaderEncoding]);
                }
            }
            catch
            {
                if (lineNumber > base.ElementInformation.Properties[_propResponseHeaderEncoding.Name].LineNumber)
                {
                    property = _propResponseHeaderEncoding;
                    lineNumber = base.ElementInformation.Properties[property.Name].LineNumber;
                }
            }
            try
            {
                if (!string.IsNullOrEmpty((string) base[_propRequestEncoding]))
                {
                    this.requestEncodingCache = Encoding.GetEncoding((string) base[_propRequestEncoding]);
                }
            }
            catch
            {
                if (lineNumber > base.ElementInformation.Properties[_propRequestEncoding.Name].LineNumber)
                {
                    property = _propRequestEncoding;
                    lineNumber = base.ElementInformation.Properties[property.Name].LineNumber;
                }
            }
            try
            {
                if (!string.IsNullOrEmpty((string) base[_propFileEncoding]))
                {
                    this.fileEncodingCache = Encoding.GetEncoding((string) base[_propFileEncoding]);
                }
            }
            catch
            {
                if (lineNumber > base.ElementInformation.Properties[_propFileEncoding.Name].LineNumber)
                {
                    property = _propFileEncoding;
                    lineNumber = base.ElementInformation.Properties[property.Name].LineNumber;
                }
            }
            try
            {
                if (!string.IsNullOrEmpty((string) base[_propCulture]))
                {
                    this.CheckCulture((string) base[_propCulture]);
                }
            }
            catch
            {
                if (lineNumber > base.ElementInformation.Properties[_propCulture.Name].LineNumber)
                {
                    property = _propCulture;
                    lineNumber = base.ElementInformation.Properties[_propCulture.Name].LineNumber;
                }
            }
            try
            {
                if (!string.IsNullOrEmpty((string) base[_propUICulture]))
                {
                    this.CheckCulture((string) base[_propUICulture]);
                }
            }
            catch
            {
                if (lineNumber > base.ElementInformation.Properties[_propUICulture.Name].LineNumber)
                {
                    property = _propUICulture;
                    lineNumber = base.ElementInformation.Properties[_propUICulture.Name].LineNumber;
                }
            }
            if (property != null)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_value_for_globalization_attr", new object[] { property.Name }), base.ElementInformation.Properties[property.Name].Source, base.ElementInformation.Properties[property.Name].LineNumber);
            }
        }

        protected override void PreSerialize(XmlWriter writer)
        {
            this.PostDeserialize();
        }

        [ConfigurationProperty("culture", DefaultValue="")]
        public string Culture
        {
            get
            {
                if (this.cultureCache == null)
                {
                    this.cultureCache = (string) base[_propCulture];
                }
                return this.cultureCache;
            }
            set
            {
                base[_propCulture] = value;
                this.cultureCache = value;
            }
        }

        [ConfigurationProperty("enableBestFitResponseEncoding", DefaultValue=false)]
        public bool EnableBestFitResponseEncoding
        {
            get
            {
                return (bool) base[_propEnableBestFitResponseEncoding];
            }
            set
            {
                base[_propEnableBestFitResponseEncoding] = value;
            }
        }

        [ConfigurationProperty("enableClientBasedCulture", DefaultValue=false)]
        public bool EnableClientBasedCulture
        {
            get
            {
                return (bool) base[_propEnableClientBasedCulture];
            }
            set
            {
                base[_propEnableClientBasedCulture] = value;
            }
        }

        [ConfigurationProperty("fileEncoding")]
        public Encoding FileEncoding
        {
            get
            {
                if (this.fileEncodingCache == null)
                {
                    this.fileEncodingCache = Encoding.Default;
                }
                return this.fileEncodingCache;
            }
            set
            {
                if (value != null)
                {
                    base[_propFileEncoding] = value.WebName;
                    this.fileEncodingCache = value;
                }
                else
                {
                    base[_propFileEncoding] = value;
                    this.fileEncodingCache = Encoding.Default;
                }
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("requestEncoding", DefaultValue="utf-8")]
        public Encoding RequestEncoding
        {
            get
            {
                if (this.requestEncodingCache == null)
                {
                    this.requestEncodingCache = Encoding.UTF8;
                }
                return this.requestEncodingCache;
            }
            set
            {
                if (value != null)
                {
                    base[_propRequestEncoding] = value.WebName;
                    this.requestEncodingCache = value;
                }
                else
                {
                    base[_propRequestEncoding] = value;
                    this.requestEncodingCache = Encoding.UTF8;
                }
            }
        }

        [ConfigurationProperty("resourceProviderFactoryType", DefaultValue="")]
        public string ResourceProviderFactoryType
        {
            get
            {
                return (string) base[_propResourceProviderFactoryType];
            }
            set
            {
                base[_propResourceProviderFactoryType] = value;
            }
        }

        internal Type ResourceProviderFactoryTypeInternal
        {
            get
            {
                if ((this._resourceProviderFactoryType == null) && !string.IsNullOrEmpty(this.ResourceProviderFactoryType))
                {
                    lock (this)
                    {
                        if (this._resourceProviderFactoryType == null)
                        {
                            Type userBaseType = ConfigUtil.GetType(this.ResourceProviderFactoryType, "resourceProviderFactoryType", this);
                            ConfigUtil.CheckBaseType(typeof(ResourceProviderFactory), userBaseType, "resourceProviderFactoryType", this);
                            this._resourceProviderFactoryType = userBaseType;
                        }
                    }
                }
                return this._resourceProviderFactoryType;
            }
        }

        [ConfigurationProperty("responseEncoding", DefaultValue="utf-8")]
        public Encoding ResponseEncoding
        {
            get
            {
                if (this.responseEncodingCache == null)
                {
                    this.responseEncodingCache = Encoding.UTF8;
                }
                return this.responseEncodingCache;
            }
            set
            {
                if (value != null)
                {
                    base[_propResponseEncoding] = value.WebName;
                    this.responseEncodingCache = value;
                }
                else
                {
                    base[_propResponseEncoding] = value;
                    this.responseEncodingCache = Encoding.UTF8;
                }
            }
        }

        [ConfigurationProperty("responseHeaderEncoding", DefaultValue="utf-8")]
        public Encoding ResponseHeaderEncoding
        {
            get
            {
                if (this.responseHeaderEncodingCache == null)
                {
                    this.responseHeaderEncodingCache = Encoding.UTF8;
                }
                return this.responseHeaderEncodingCache;
            }
            set
            {
                if (value != null)
                {
                    base[_propResponseHeaderEncoding] = value.WebName;
                    this.responseHeaderEncodingCache = value;
                }
                else
                {
                    base[_propResponseHeaderEncoding] = value;
                    this.responseHeaderEncodingCache = Encoding.UTF8;
                }
            }
        }

        [ConfigurationProperty("uiCulture", DefaultValue="")]
        public string UICulture
        {
            get
            {
                if (this.uiCultureCache == null)
                {
                    this.uiCultureCache = (string) base[_propUICulture];
                }
                return this.uiCultureCache;
            }
            set
            {
                base[_propUICulture] = value;
                this.uiCultureCache = value;
            }
        }
    }
}

