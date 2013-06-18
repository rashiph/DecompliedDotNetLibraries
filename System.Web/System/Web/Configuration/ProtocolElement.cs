namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web.Hosting;
    using System.Web.Util;

    public sealed class ProtocolElement : ConfigurationElement
    {
        private static readonly ConfigurationProperty _propAppDomainHandlerType = new ConfigurationProperty("appDomainHandlerType", typeof(string), null);
        private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propProcessHandlerType = new ConfigurationProperty("processHandlerType", typeof(string), null);
        private static readonly ConfigurationProperty _propValidate = new ConfigurationProperty("validate", typeof(bool), false);

        static ProtocolElement()
        {
            _properties.Add(_propName);
            _properties.Add(_propProcessHandlerType);
            _properties.Add(_propAppDomainHandlerType);
            _properties.Add(_propValidate);
        }

        public ProtocolElement()
        {
        }

        public ProtocolElement(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw System.Web.Util.ExceptionUtil.ParameterNullOrEmpty("name");
            }
            base[_propName] = name;
        }

        protected override void PostDeserialize()
        {
            if (this.Validate)
            {
                this.ValidateTypes();
            }
        }

        private void ValidateTypes()
        {
            Type type;
            Type type2;
            try
            {
                type = Type.GetType(this.ProcessHandlerType, true);
            }
            catch (Exception exception)
            {
                throw new ConfigurationErrorsException(exception.Message, exception, base.ElementInformation.Properties["ProcessHandlerType"].Source, base.ElementInformation.Properties["ProcessHandlerType"].LineNumber);
            }
            ConfigUtil.CheckAssignableType(typeof(ProcessProtocolHandler), type, this, "ProcessHandlerType");
            try
            {
                type2 = Type.GetType(this.AppDomainHandlerType, true);
            }
            catch (Exception exception2)
            {
                throw new ConfigurationErrorsException(exception2.Message, exception2, base.ElementInformation.Properties["AppDomainHandlerType"].Source, base.ElementInformation.Properties["AppDomainHandlerType"].LineNumber);
            }
            ConfigUtil.CheckAssignableType(typeof(AppDomainProtocolHandler), type2, this, "AppDomainHandlerType");
        }

        [ConfigurationProperty("appDomainHandlerType")]
        public string AppDomainHandlerType
        {
            get
            {
                return (string) base[_propAppDomainHandlerType];
            }
            set
            {
                base[_propAppDomainHandlerType] = value;
            }
        }

        [StringValidator(MinLength=1), ConfigurationProperty("name", IsRequired=true, IsKey=true)]
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

        [ConfigurationProperty("processHandlerType")]
        public string ProcessHandlerType
        {
            get
            {
                return (string) base[_propProcessHandlerType];
            }
            set
            {
                base[_propProcessHandlerType] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("validate", DefaultValue=false)]
        public bool Validate
        {
            get
            {
                return (bool) base[_propValidate];
            }
            set
            {
                base[_propValidate] = value;
            }
        }
    }
}

