namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics.Application;

    public sealed class ServiceActivationElement : ConfigurationElement
    {
        private const string PathSeparatorString = "/";
        private ConfigurationPropertyCollection properties;
        private const string ReversSlashString = @"\";

        public ServiceActivationElement()
        {
        }

        public ServiceActivationElement(string relativeAddress) : this()
        {
            if (relativeAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("relativeAddress");
            }
            this.RelativeAddress = relativeAddress;
        }

        public ServiceActivationElement(string relativeAddress, string service) : this(relativeAddress)
        {
            this.Service = service;
        }

        public ServiceActivationElement(string relativeAddress, string service, string factory) : this(relativeAddress, service)
        {
            this.Factory = factory;
        }

        [StringValidator(MinLength=0), ConfigurationProperty("factory", Options=ConfigurationPropertyOptions.None)]
        public string Factory
        {
            get
            {
                return (string) base["factory"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["factory"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("relativeAddress", typeof(string), null, null, new RelativeAddressValidator(), ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired));
                    propertys.Add(new ConfigurationProperty("service", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("factory", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [RelativeAddressValidator, ConfigurationProperty("relativeAddress", Options=ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        public string RelativeAddress
        {
            get
            {
                return (string) base["relativeAddress"];
            }
            set
            {
                base["relativeAddress"] = value;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("service", Options=ConfigurationPropertyOptions.None)]
        public string Service
        {
            get
            {
                return (string) base["service"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["service"] = value;
            }
        }

        private class RelativeAddressValidator : ConfigurationValidatorBase
        {
            public override bool CanValidate(Type type)
            {
                return (type == typeof(string));
            }

            public override void Validate(object value)
            {
                string str = value as string;
                if ((string.IsNullOrEmpty(str) || string.IsNullOrEmpty(str.Trim())) || (str.Length < 3))
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(System.ServiceModel.SR.GetString("Hosting_RelativeAddressFormatError", new object[] { str })));
                }
                if (str.StartsWith("/", StringComparison.CurrentCultureIgnoreCase) || str.StartsWith(@"\", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(System.ServiceModel.SR.GetString("Hosting_NoAbsoluteRelativeAddress", new object[] { str })));
                }
            }
        }

        [AttributeUsage(AttributeTargets.Property)]
        private sealed class RelativeAddressValidatorAttribute : ConfigurationValidatorAttribute
        {
            public override ConfigurationValidatorBase ValidatorInstance
            {
                get
                {
                    return new ServiceActivationElement.RelativeAddressValidator();
                }
            }
        }
    }
}

