namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web;
    using System.Web.Configuration.Common;
    using System.Web.Security;
    using System.Web.Util;

    public sealed class HttpModuleAction : ConfigurationElement
    {
        private ModulesEntry _modualEntry;
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propType = new ConfigurationProperty("type", typeof(string), string.Empty, ConfigurationPropertyOptions.IsTypeStringTransformationRequired | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationElementProperty s_elemProperty = new ConfigurationElementProperty(new CallbackValidator(typeof(HttpModuleAction), new ValidatorCallback(HttpModuleAction.Validate)));

        static HttpModuleAction()
        {
            _properties.Add(_propName);
            _properties.Add(_propType);
        }

        internal HttpModuleAction()
        {
        }

        public HttpModuleAction(string name, string type) : this()
        {
            this.Name = name;
            this.Type = type;
            this._modualEntry = null;
        }

        internal static bool IsSpecialModule(string className)
        {
            return ModulesEntry.IsTypeMatch(typeof(DefaultAuthenticationModule), className);
        }

        internal static bool IsSpecialModuleName(string name)
        {
            return System.Web.Util.StringUtil.EqualsIgnoreCase(name, "DefaultAuthentication");
        }

        private static void Validate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("httpModule");
            }
            HttpModuleAction action = (HttpModuleAction) value;
            if (IsSpecialModule(action.Type))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Special_module_cannot_be_added_manually", new object[] { action.Type }), action.ElementInformation.Properties["type"].Source, action.ElementInformation.Properties["type"].LineNumber);
            }
            if (IsSpecialModuleName(action.Name))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Special_module_cannot_be_added_manually", new object[] { action.Name }), action.ElementInformation.Properties["name"].Source, action.ElementInformation.Properties["name"].LineNumber);
            }
        }

        protected override ConfigurationElementProperty ElementProperty
        {
            get
            {
                return s_elemProperty;
            }
        }

        internal ModulesEntry Entry
        {
            get
            {
                ModulesEntry entry;
                try
                {
                    if (this._modualEntry == null)
                    {
                        this._modualEntry = new ModulesEntry(this.Name, this.Type, _propType.Name, this);
                    }
                    entry = this._modualEntry;
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(exception.Message, base.ElementInformation.Properties[_propType.Name].Source, base.ElementInformation.Properties[_propType.Name].LineNumber);
                }
                return entry;
            }
        }

        internal string FileName
        {
            get
            {
                return base.ElementInformation.Properties["name"].Source;
            }
        }

        internal string Key
        {
            get
            {
                return this.Name;
            }
        }

        internal int LineNumber
        {
            get
            {
                return base.ElementInformation.Properties["name"].LineNumber;
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

        [ConfigurationProperty("type", IsRequired=true, DefaultValue="")]
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
    }
}

