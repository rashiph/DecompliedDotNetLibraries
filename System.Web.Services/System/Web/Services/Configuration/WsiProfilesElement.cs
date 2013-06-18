namespace System.Web.Services.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;
    using System.Web.Services;

    public sealed class WsiProfilesElement : ConfigurationElement
    {
        private readonly ConfigurationProperty name;
        private ConfigurationPropertyCollection properties;

        public WsiProfilesElement()
        {
            this.properties = new ConfigurationPropertyCollection();
            this.name = new ConfigurationProperty("name", typeof(WsiProfiles), WsiProfiles.None, ConfigurationPropertyOptions.IsKey);
            this.properties.Add(this.name);
        }

        public WsiProfilesElement(WsiProfiles name) : this()
        {
            this.Name = name;
        }

        private bool IsValidWsiProfilesValue(WsiProfiles value)
        {
            return Enum.IsDefined(typeof(WsiProfiles), value);
        }

        [ConfigurationProperty("name", IsKey=true, DefaultValue=0)]
        public WsiProfiles Name
        {
            get
            {
                return (WsiProfiles) base[this.name];
            }
            set
            {
                if (!this.IsValidWsiProfilesValue(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base[this.name] = value;
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
    }
}

