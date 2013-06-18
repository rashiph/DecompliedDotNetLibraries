namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class CustomBindingElement : NamedServiceModelExtensionCollectionElement<BindingElementExtensionElement>, ICollection<BindingElementExtensionElement>, IEnumerable<BindingElementExtensionElement>, IEnumerable, IBindingConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        public CustomBindingElement() : this(null)
        {
        }

        public CustomBindingElement(string name) : base("bindingElementExtensions", name)
        {
        }

        public override void Add(BindingElementExtensionElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            BindingElementExtensionElement existingElement = null;
            if (!this.CanAddEncodingElement(element, ref existingElement))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigMessageEncodingAlreadyInBinding", new object[] { existingElement.ConfigurationElementName, existingElement.GetType().AssemblyQualifiedName })));
            }
            if (!this.CanAddStreamUpgradeElement(element, ref existingElement))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigStreamUpgradeElementAlreadyInBinding", new object[] { existingElement.ConfigurationElementName, existingElement.GetType().AssemblyQualifiedName })));
            }
            if (!this.CanAddTransportElement(element, ref existingElement))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigTransportAlreadyInBinding", new object[] { existingElement.ConfigurationElementName, existingElement.GetType().AssemblyQualifiedName })));
            }
            base.Add(element);
        }

        public void ApplyConfiguration(Binding binding)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (binding.GetType() != typeof(CustomBinding))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("ConfigInvalidTypeForBinding", new object[] { typeof(CustomBinding).AssemblyQualifiedName, binding.GetType().AssemblyQualifiedName }));
            }
            binding.CloseTimeout = this.CloseTimeout;
            binding.OpenTimeout = this.OpenTimeout;
            binding.ReceiveTimeout = this.ReceiveTimeout;
            binding.SendTimeout = this.SendTimeout;
            this.OnApplyConfiguration(binding);
        }

        public override bool CanAdd(BindingElementExtensionElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            BindingElementExtensionElement existingElement = null;
            return (((!base.ContainsKey(element.GetType()) && this.CanAddEncodingElement(element, ref existingElement)) && this.CanAddStreamUpgradeElement(element, ref existingElement)) && this.CanAddTransportElement(element, ref existingElement));
        }

        private bool CanAddEncodingElement(BindingElementExtensionElement element, ref BindingElementExtensionElement existingElement)
        {
            return this.CanAddExclusiveElement(typeof(MessageEncodingBindingElement), element.BindingElementType, ref existingElement);
        }

        private bool CanAddExclusiveElement(System.Type exclusiveType, System.Type bindingElementType, ref BindingElementExtensionElement existingElement)
        {
            bool flag = true;
            if (exclusiveType.IsAssignableFrom(bindingElementType))
            {
                foreach (BindingElementExtensionElement element in this)
                {
                    if (exclusiveType.IsAssignableFrom(element.BindingElementType))
                    {
                        flag = false;
                        existingElement = element;
                        return flag;
                    }
                }
            }
            return flag;
        }

        private bool CanAddStreamUpgradeElement(BindingElementExtensionElement element, ref BindingElementExtensionElement existingElement)
        {
            return this.CanAddExclusiveElement(typeof(StreamUpgradeBindingElement), element.BindingElementType, ref existingElement);
        }

        private bool CanAddTransportElement(BindingElementExtensionElement element, ref BindingElementExtensionElement existingElement)
        {
            return this.CanAddExclusiveElement(typeof(TransportBindingElement), element.BindingElementType, ref existingElement);
        }

        protected void OnApplyConfiguration(Binding binding)
        {
            CustomBinding binding2 = (CustomBinding) binding;
            foreach (BindingElementExtensionElement element in this)
            {
                binding2.Elements.Add(element.CreateBindingElement());
            }
        }

        [ConfigurationProperty("closeTimeout", DefaultValue="00:01:00"), ServiceModelTimeSpanValidator(MinValueString="00:00:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        public TimeSpan CloseTimeout
        {
            get
            {
                return (TimeSpan) base["closeTimeout"];
            }
            set
            {
                base["closeTimeout"] = value;
                base.SetIsModified();
            }
        }

        [ServiceModelTimeSpanValidator(MinValueString="00:00:00"), ConfigurationProperty("openTimeout", DefaultValue="00:01:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        public TimeSpan OpenTimeout
        {
            get
            {
                return (TimeSpan) base["openTimeout"];
            }
            set
            {
                base["openTimeout"] = value;
                base.SetIsModified();
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("closeTimeout", typeof(TimeSpan), TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("openTimeout", typeof(TimeSpan), TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("receiveTimeout", typeof(TimeSpan), TimeSpan.Parse("00:10:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("sendTimeout", typeof(TimeSpan), TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    this.properties = properties;
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
                base.SetIsModified();
            }
        }

        [ConfigurationProperty("sendTimeout", DefaultValue="00:01:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ServiceModelTimeSpanValidator(MinValueString="00:00:00")]
        public TimeSpan SendTimeout
        {
            get
            {
                return (TimeSpan) base["sendTimeout"];
            }
            set
            {
                base["sendTimeout"] = value;
                base.SetIsModified();
            }
        }
    }
}

