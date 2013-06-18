namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public abstract class StandardBindingElement : ConfigurationElement, IBindingConfigurationElement, IConfigurationContextProviderInternal
    {
        [SecurityCritical]
        private EvaluationContextHelper contextHelper;
        private ConfigurationPropertyCollection properties;

        protected StandardBindingElement() : this(null)
        {
        }

        protected StandardBindingElement(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                this.Name = name;
            }
        }

        public void ApplyConfiguration(Binding binding)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (binding.GetType() != this.BindingElementType)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("ConfigInvalidTypeForBinding", new object[] { (this.BindingElementType == null) ? string.Empty : this.BindingElementType.AssemblyQualifiedName, binding.GetType().AssemblyQualifiedName }));
            }
            binding.CloseTimeout = this.CloseTimeout;
            binding.OpenTimeout = this.OpenTimeout;
            binding.ReceiveTimeout = this.ReceiveTimeout;
            binding.SendTimeout = this.SendTimeout;
            this.OnApplyConfiguration(binding);
        }

        protected internal virtual void InitializeFrom(Binding binding)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (binding.GetType() != this.BindingElementType)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("ConfigInvalidTypeForBinding", new object[] { (this.BindingElementType == null) ? string.Empty : this.BindingElementType.AssemblyQualifiedName, binding.GetType().AssemblyQualifiedName }));
            }
            this.CloseTimeout = binding.CloseTimeout;
            this.OpenTimeout = binding.OpenTimeout;
            this.ReceiveTimeout = binding.ReceiveTimeout;
            this.SendTimeout = binding.SendTimeout;
        }

        protected abstract void OnApplyConfiguration(Binding binding);
        [SecurityCritical]
        protected override void Reset(ConfigurationElement parentElement)
        {
            this.contextHelper.OnReset(parentElement);
            base.Reset(parentElement);
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return base.EvaluationContext;
        }

        [SecurityCritical]
        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            return this.contextHelper.GetOriginalContext(this);
        }

        protected abstract System.Type BindingElementType { get; }

        [TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ServiceModelTimeSpanValidator(MinValueString="00:00:00"), ConfigurationProperty("closeTimeout", DefaultValue="00:01:00")]
        public TimeSpan CloseTimeout
        {
            get
            {
                return (TimeSpan) base["closeTimeout"];
            }
            set
            {
                base["closeTimeout"] = value;
            }
        }

        [ConfigurationProperty("name", Options=ConfigurationPropertyOptions.IsKey), StringValidator(MinLength=0)]
        public string Name
        {
            get
            {
                return (string) base["name"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["name"] = value;
            }
        }

        [ConfigurationProperty("openTimeout", DefaultValue="00:01:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ServiceModelTimeSpanValidator(MinValueString="00:00:00")]
        public TimeSpan OpenTimeout
        {
            get
            {
                return (TimeSpan) base["openTimeout"];
            }
            set
            {
                base["openTimeout"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("name", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    propertys.Add(new ConfigurationProperty("closeTimeout", typeof(TimeSpan), TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("openTimeout", typeof(TimeSpan), TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("receiveTimeout", typeof(TimeSpan), TimeSpan.Parse("00:10:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("sendTimeout", typeof(TimeSpan), TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ConfigurationProperty("receiveTimeout", DefaultValue="00:10:00"), ServiceModelTimeSpanValidator(MinValueString="00:00:00")]
        public TimeSpan ReceiveTimeout
        {
            get
            {
                return (TimeSpan) base["receiveTimeout"];
            }
            set
            {
                base["receiveTimeout"] = value;
            }
        }

        [ServiceModelTimeSpanValidator(MinValueString="00:00:00"), ConfigurationProperty("sendTimeout", DefaultValue="00:01:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        public TimeSpan SendTimeout
        {
            get
            {
                return (TimeSpan) base["sendTimeout"];
            }
            set
            {
                base["sendTimeout"] = value;
            }
        }
    }
}

