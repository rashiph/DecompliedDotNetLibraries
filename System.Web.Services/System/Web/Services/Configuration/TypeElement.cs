namespace System.Web.Services.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;

    public sealed class TypeElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;
        private readonly ConfigurationProperty type;

        public TypeElement()
        {
            this.properties = new ConfigurationPropertyCollection();
            this.type = new ConfigurationProperty("type", typeof(TypeAndName), null, new TypeAndNameConverter(), null, ConfigurationPropertyOptions.IsKey);
            this.properties.Add(this.type);
        }

        public TypeElement(string type) : this()
        {
            base[this.type] = new TypeAndName(type);
        }

        public TypeElement(System.Type type) : this(type.AssemblyQualifiedName)
        {
        }

        protected override ConfigurationPropertyCollection Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty("type", IsKey=true), TypeConverter(typeof(TypeAndNameConverter))]
        public System.Type Type
        {
            get
            {
                return ((TypeAndName) base[this.type]).type;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                base[this.type] = new TypeAndName(value);
            }
        }
    }
}

