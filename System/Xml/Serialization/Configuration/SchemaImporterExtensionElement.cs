namespace System.Xml.Serialization.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;

    public sealed class SchemaImporterExtensionElement : ConfigurationElement
    {
        private readonly ConfigurationProperty name;
        private ConfigurationPropertyCollection properties;
        private readonly ConfigurationProperty type;

        public SchemaImporterExtensionElement()
        {
            this.properties = new ConfigurationPropertyCollection();
            this.name = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsKey);
            this.type = new ConfigurationProperty("type", typeof(System.Type), null, new TypeTypeConverter(), null, ConfigurationPropertyOptions.IsRequired);
            this.properties.Add(this.name);
            this.properties.Add(this.type);
        }

        public SchemaImporterExtensionElement(string name, string type) : this()
        {
            this.Name = name;
            base[this.type] = new TypeAndName(type);
        }

        public SchemaImporterExtensionElement(string name, System.Type type) : this()
        {
            this.Name = name;
            this.Type = type;
        }

        internal string Key
        {
            get
            {
                return this.Name;
            }
        }

        [ConfigurationProperty("name", IsRequired=true, IsKey=true)]
        public string Name
        {
            get
            {
                return (string) base[this.name];
            }
            set
            {
                base[this.name] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        [TypeConverter(typeof(TypeTypeConverter)), ConfigurationProperty("type", IsRequired=true, IsKey=false)]
        public System.Type Type
        {
            get
            {
                return ((TypeAndName) base[this.type]).type;
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
                return this.type.Equals(((SchemaImporterExtensionElement.TypeAndName) comparand).type);
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
                    return new SchemaImporterExtensionElement.TypeAndName((string) value);
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (!(destinationType == typeof(string)))
                {
                    return base.ConvertTo(context, culture, value, destinationType);
                }
                SchemaImporterExtensionElement.TypeAndName name = (SchemaImporterExtensionElement.TypeAndName) value;
                if (name.name != null)
                {
                    return name.name;
                }
                return name.type.AssemblyQualifiedName;
            }
        }
    }
}

