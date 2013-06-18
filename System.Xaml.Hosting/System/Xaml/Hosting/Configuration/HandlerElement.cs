namespace System.Xaml.Hosting.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;

    public sealed class HandlerElement : ConfigurationElement
    {
        private Type httpHandlerCLRType;
        private static ConfigurationPropertyCollection properties = InitializeProperties();
        private Type xamlRootElementClrType;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public HandlerElement()
        {
        }

        public HandlerElement(string xamlType, string handlerType)
        {
            this.XamlRootElementType = xamlType;
            this.HttpHandlerType = handlerType;
        }

        private static ConfigurationPropertyCollection InitializeProperties()
        {
            ConfigurationProperty property = new ConfigurationProperty("httpHandlerType", typeof(string), " ", null, new StringValidator(1), ConfigurationPropertyOptions.IsRequired);
            ConfigurationProperty property2 = new ConfigurationProperty("xamlRootElementType", typeof(string), " ", null, new StringValidator(1), ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
            ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
            propertys.Add(property2);
            propertys.Add(property);
            return propertys;
        }

        internal Type LoadHttpHandlerType()
        {
            if (this.httpHandlerCLRType == null)
            {
                this.httpHandlerCLRType = Type.GetType(this.HttpHandlerType, true);
            }
            return this.httpHandlerCLRType;
        }

        internal Type LoadXamlRootElementType()
        {
            if (this.xamlRootElementClrType == null)
            {
                this.xamlRootElementClrType = Type.GetType(this.XamlRootElementType, true);
            }
            return this.xamlRootElementClrType;
        }

        [ConfigurationProperty("httpHandlerType", DefaultValue=" ", Options=ConfigurationPropertyOptions.IsRequired), StringValidator(MinLength=1)]
        public string HttpHandlerType
        {
            get
            {
                return (string) base["httpHandlerType"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["httpHandlerType"] = value;
            }
        }

        internal string Key
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.XamlRootElementType;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return properties;
            }
        }

        [ConfigurationProperty("xamlRootElementType", DefaultValue=" ", Options=ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired), StringValidator(MinLength=1)]
        public string XamlRootElementType
        {
            get
            {
                return (string) base["xamlRootElementType"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["xamlRootElementType"] = value;
            }
        }
    }
}

