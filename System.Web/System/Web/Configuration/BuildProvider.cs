namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Web.Compilation;
    using System.Web.Util;

    public sealed class BuildProvider : ConfigurationElement
    {
        private readonly System.Web.Compilation.BuildProviderInfo _info;
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propExtension = new ConfigurationProperty("extension", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propType = new ConfigurationProperty("type", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsTypeStringTransformationRequired | ConfigurationPropertyOptions.IsRequired);

        static BuildProvider()
        {
            _properties.Add(_propExtension);
            _properties.Add(_propType);
        }

        internal BuildProvider()
        {
            this._info = new ConfigurationBuildProviderInfo(this);
        }

        public BuildProvider(string extension, string type) : this()
        {
            this.Extension = extension;
            this.Type = type;
        }

        public override bool Equals(object provider)
        {
            System.Web.Configuration.BuildProvider provider2 = provider as System.Web.Configuration.BuildProvider;
            return (((provider2 != null) && System.Web.Util.StringUtil.EqualsIgnoreCase(this.Extension, provider2.Extension)) && (this.Type == provider2.Type));
        }

        public override int GetHashCode()
        {
            return HashCodeCombiner.CombineHashCodes(this.Extension.ToLower(CultureInfo.InvariantCulture).GetHashCode(), this.Type.GetHashCode());
        }

        internal System.Web.Compilation.BuildProviderInfo BuildProviderInfo
        {
            get
            {
                return this._info;
            }
        }

        [StringValidator(MinLength=1), ConfigurationProperty("extension", IsRequired=true, IsKey=true, DefaultValue="")]
        public string Extension
        {
            get
            {
                return (string) base[_propExtension];
            }
            set
            {
                base[_propExtension] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("type", IsRequired=true, DefaultValue=""), StringValidator(MinLength=1)]
        public string Type
        {
            get
            {
                return (string) base[_propType];
            }
            set
            {
                base[_propType] = value;
            }
        }

        private class ConfigurationBuildProviderInfo : BuildProviderInfo
        {
            private readonly System.Web.Configuration.BuildProvider _buildProvider;
            private object _lock = new object();
            private System.Type _type;

            public ConfigurationBuildProviderInfo(System.Web.Configuration.BuildProvider buildProvider)
            {
                this._buildProvider = buildProvider;
            }

            internal override System.Type Type
            {
                get
                {
                    if (this._type == null)
                    {
                        lock (this._lock)
                        {
                            if (this._type == null)
                            {
                                this._type = CompilationUtil.LoadTypeWithChecks(this._buildProvider.Type, typeof(System.Web.Compilation.BuildProvider), null, this._buildProvider, "type");
                            }
                        }
                    }
                    return this._type;
                }
            }
        }
    }
}

