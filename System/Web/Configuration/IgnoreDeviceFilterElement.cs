namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web;
    using System.Web.UI;

    public sealed class IgnoreDeviceFilterElement : ConfigurationElement
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationElementProperty s_elemProperty = new ConfigurationElementProperty(new CallbackValidator(typeof(IgnoreDeviceFilterElement), new ValidatorCallback(IgnoreDeviceFilterElement.ValidateElement)));

        static IgnoreDeviceFilterElement()
        {
            _properties.Add(_propName);
        }

        internal IgnoreDeviceFilterElement()
        {
        }

        public IgnoreDeviceFilterElement(string name)
        {
            base[_propName] = name;
        }

        private static void ValidateElement(object value)
        {
            IgnoreDeviceFilterElement element = (IgnoreDeviceFilterElement) value;
            if (Util.ContainsWhiteSpace(element.Name))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Space_attribute", new object[] { "name" }));
            }
        }

        protected override ConfigurationElementProperty ElementProperty
        {
            get
            {
                return s_elemProperty;
            }
        }

        [ConfigurationProperty("name", IsRequired=true, IsKey=true), StringValidator(MinLength=1)]
        public string Name
        {
            get
            {
                return (string) base[_propName];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }
}

