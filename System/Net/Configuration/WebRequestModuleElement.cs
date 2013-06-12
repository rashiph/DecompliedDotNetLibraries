namespace System.Net.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;

    public sealed class WebRequestModuleElement : ConfigurationElement
    {
        private readonly ConfigurationProperty prefix;
        private ConfigurationPropertyCollection properties;
        private readonly ConfigurationProperty type;

        public WebRequestModuleElement()
        {
            this.properties = new ConfigurationPropertyCollection();
            this.prefix = new ConfigurationProperty("prefix", typeof(string), null, ConfigurationPropertyOptions.IsKey);
            this.type = new ConfigurationProperty("type", typeof(TypeAndName), null, new TypeTypeConverter(), null, ConfigurationPropertyOptions.None);
            this.properties.Add(this.prefix);
            this.properties.Add(this.type);
        }

        public WebRequestModuleElement(string prefix, string type) : this()
        {
            this.Prefix = prefix;
            base[this.type] = new TypeAndName(type);
        }

        public WebRequestModuleElement(string prefix, System.Type type) : this()
        {
            this.Prefix = prefix;
            this.Type = type;
        }

        internal string Key
        {
            get
            {
                return this.Prefix;
            }
        }

        [ConfigurationProperty("prefix", IsRequired=true, IsKey=true)]
        public string Prefix
        {
            get
            {
                return (string) base[this.prefix];
            }
            set
            {
                base[this.prefix] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        [TypeConverter(typeof(TypeTypeConverter)), ConfigurationProperty("type")]
        public System.Type Type
        {
            get
            {
                TypeAndName name = (TypeAndName) base[this.type];
                if (name != null)
                {
                    return name.type;
                }
                return null;
            }
            set
            {
                base[this.type] = new TypeAndName(value);
            }
        }

        private class TypeAndName
        {
            public readonly string name;
            public readonly Type type;

            public TypeAndName(string name)
            {
                this.type = Type.GetType(name, true, true);
                this.name = name;
            }

            public TypeAndName(Type type)
            {
                this.type = type;
            }

            public override bool Equals(object comparand)
            {
                return this.type.Equals(((WebRequestModuleElement.TypeAndName) comparand).type);
            }

            public override int GetHashCode()
            {
                return this.type.GetHashCode();
            }
        }

        private class TypeTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string)
                {
                    return new WebRequestModuleElement.TypeAndName((string) value);
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (!(destinationType == typeof(string)))
                {
                    return base.ConvertTo(context, culture, value, destinationType);
                }
                WebRequestModuleElement.TypeAndName name = (WebRequestModuleElement.TypeAndName) value;
                if (name.name != null)
                {
                    return name.name;
                }
                return name.type.AssemblyQualifiedName;
            }
        }
    }
}

