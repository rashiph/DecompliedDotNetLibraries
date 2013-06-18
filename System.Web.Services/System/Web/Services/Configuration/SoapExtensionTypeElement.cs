namespace System.Web.Services.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.Web.Services;

    public sealed class SoapExtensionTypeElement : ConfigurationElement
    {
        private readonly ConfigurationProperty group;
        private readonly ConfigurationProperty priority;
        private ConfigurationPropertyCollection properties;
        private readonly ConfigurationProperty type;

        public SoapExtensionTypeElement()
        {
            this.properties = new ConfigurationPropertyCollection();
            this.group = new ConfigurationProperty("group", typeof(PriorityGroup), PriorityGroup.Low, new EnumConverter(typeof(PriorityGroup)), null, ConfigurationPropertyOptions.IsKey);
            this.priority = new ConfigurationProperty("priority", typeof(int), 0, null, new IntegerValidator(0, 0x7fffffff), ConfigurationPropertyOptions.IsKey);
            this.type = new ConfigurationProperty("type", typeof(System.Type), null, new TypeTypeConverter(), null, ConfigurationPropertyOptions.IsKey);
            this.properties.Add(this.group);
            this.properties.Add(this.priority);
            this.properties.Add(this.type);
        }

        public SoapExtensionTypeElement(string type, int priority, PriorityGroup group) : this()
        {
            this.Type = System.Type.GetType(type, true, true);
            this.Priority = priority;
            this.Group = group;
        }

        public SoapExtensionTypeElement(System.Type type, int priority, PriorityGroup group) : this(type.AssemblyQualifiedName, priority, group)
        {
        }

        [ConfigurationProperty("group", IsKey=true, DefaultValue=1)]
        public PriorityGroup Group
        {
            get
            {
                return (PriorityGroup) base[this.group];
            }
            set
            {
                if (!Enum.IsDefined(typeof(PriorityGroup), value))
                {
                    throw new ArgumentException(Res.GetString("Invalid_priority_group_value"), "value");
                }
                base[this.group] = value;
            }
        }

        [IntegerValidator(MinValue=0), ConfigurationProperty("priority", IsKey=true, DefaultValue=0)]
        public int Priority
        {
            get
            {
                return (int) base[this.priority];
            }
            set
            {
                base[this.priority] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty("type", IsKey=true), TypeConverter(typeof(TypeTypeConverter))]
        public System.Type Type
        {
            get
            {
                return (System.Type) base[this.type];
            }
            set
            {
                base[this.type] = value;
            }
        }
    }
}

