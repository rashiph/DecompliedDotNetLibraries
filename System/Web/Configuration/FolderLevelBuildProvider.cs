namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Web.Compilation;
    using System.Web.Util;

    public sealed class FolderLevelBuildProvider : ConfigurationElement
    {
        private FolderLevelBuildProviderAppliesTo _appliesToInternal;
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propType = new ConfigurationProperty("type", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsTypeStringTransformationRequired | ConfigurationPropertyOptions.IsRequired);
        private System.Type _type;

        static FolderLevelBuildProvider()
        {
            _properties.Add(_propName);
            _properties.Add(_propType);
        }

        internal FolderLevelBuildProvider()
        {
        }

        public FolderLevelBuildProvider(string name, string type) : this()
        {
            this.Name = name;
            this.Type = type;
        }

        public override bool Equals(object provider)
        {
            FolderLevelBuildProvider provider2 = provider as FolderLevelBuildProvider;
            return (((provider2 != null) && System.Web.Util.StringUtil.EqualsIgnoreCase(this.Name, provider2.Name)) && (this.Type == provider2.Type));
        }

        public override int GetHashCode()
        {
            return HashCodeCombiner.CombineHashCodes(this.Name.ToLower(CultureInfo.InvariantCulture).GetHashCode(), this.Type.GetHashCode());
        }

        internal FolderLevelBuildProviderAppliesTo AppliesToInternal
        {
            get
            {
                if (this._appliesToInternal == FolderLevelBuildProviderAppliesTo.None)
                {
                    object[] customAttributes = this.TypeInternal.GetCustomAttributes(typeof(FolderLevelBuildProviderAppliesToAttribute), true);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                    {
                        this._appliesToInternal = ((FolderLevelBuildProviderAppliesToAttribute) customAttributes[0]).AppliesTo;
                    }
                    else
                    {
                        this._appliesToInternal = FolderLevelBuildProviderAppliesTo.None;
                    }
                }
                return this._appliesToInternal;
            }
        }

        [StringValidator(MinLength=1), ConfigurationProperty("name", IsRequired=true, IsKey=true, DefaultValue="")]
        public string Name
        {
            get
            {
                return (string) base[_propName];
            }
            set
            {
                base[_propName] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [StringValidator(MinLength=1), ConfigurationProperty("type", IsRequired=true, DefaultValue="")]
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

        internal System.Type TypeInternal
        {
            get
            {
                if (this._type == null)
                {
                    lock (this)
                    {
                        if (this._type == null)
                        {
                            this._type = CompilationUtil.LoadTypeWithChecks(this.Type, typeof(System.Web.Compilation.BuildProvider), null, this, "type");
                        }
                    }
                }
                return this._type;
            }
        }
    }
}

