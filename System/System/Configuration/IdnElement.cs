namespace System.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public sealed class IdnElement : ConfigurationElement
    {
        private readonly ConfigurationProperty enabled = new ConfigurationProperty("enabled", typeof(UriIdnScope), UriIdnScope.None, new UriIdnScopeTypeConverter(), null, ConfigurationPropertyOptions.None);
        internal const UriIdnScope EnabledDefaultValue = UriIdnScope.None;
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        public IdnElement()
        {
            this.properties.Add(this.enabled);
        }

        [ConfigurationProperty("enabled", DefaultValue=0)]
        public UriIdnScope Enabled
        {
            get
            {
                return (UriIdnScope) base[this.enabled];
            }
            set
            {
                base[this.enabled] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        private class UriIdnScopeTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                string str2;
                string str = value as string;
                if ((str != null) && ((str2 = str.ToLower(CultureInfo.InvariantCulture)) != null))
                {
                    if (str2 == "all")
                    {
                        return UriIdnScope.All;
                    }
                    if (str2 == "none")
                    {
                        return UriIdnScope.None;
                    }
                    if (str2 == "allexceptintranet")
                    {
                        return UriIdnScope.AllExceptIntranet;
                    }
                }
                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}

