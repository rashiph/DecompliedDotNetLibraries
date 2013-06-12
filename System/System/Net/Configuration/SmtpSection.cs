namespace System.Net.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Net.Mail;

    public sealed class SmtpSection : ConfigurationSection
    {
        private readonly ConfigurationProperty deliveryMethod = new ConfigurationProperty("deliveryMethod", typeof(SmtpDeliveryMethod), SmtpDeliveryMethod.Network, new SmtpDeliveryMethodTypeConverter(), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty from = new ConfigurationProperty("from", typeof(string), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty network = new ConfigurationProperty("network", typeof(SmtpNetworkElement), null, ConfigurationPropertyOptions.None);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private readonly ConfigurationProperty specifiedPickupDirectory = new ConfigurationProperty("specifiedPickupDirectory", typeof(SmtpSpecifiedPickupDirectoryElement), null, ConfigurationPropertyOptions.None);

        public SmtpSection()
        {
            this.properties.Add(this.deliveryMethod);
            this.properties.Add(this.from);
            this.properties.Add(this.network);
            this.properties.Add(this.specifiedPickupDirectory);
        }

        [ConfigurationProperty("deliveryMethod", DefaultValue=0)]
        public SmtpDeliveryMethod DeliveryMethod
        {
            get
            {
                return (SmtpDeliveryMethod) base[this.deliveryMethod];
            }
            set
            {
                base[this.deliveryMethod] = value;
            }
        }

        [ConfigurationProperty("from")]
        public string From
        {
            get
            {
                return (string) base[this.from];
            }
            set
            {
                base[this.from] = value;
            }
        }

        [ConfigurationProperty("network")]
        public SmtpNetworkElement Network
        {
            get
            {
                return (SmtpNetworkElement) base[this.network];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty("specifiedPickupDirectory")]
        public SmtpSpecifiedPickupDirectoryElement SpecifiedPickupDirectory
        {
            get
            {
                return (SmtpSpecifiedPickupDirectoryElement) base[this.specifiedPickupDirectory];
            }
        }

        private class SmtpDeliveryMethodTypeConverter : TypeConverter
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
                    if (str2 == "network")
                    {
                        return SmtpDeliveryMethod.Network;
                    }
                    if (str2 == "specifiedpickupdirectory")
                    {
                        return SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    }
                    if (str2 == "pickupdirectoryfromiis")
                    {
                        return SmtpDeliveryMethod.PickupDirectoryFromIis;
                    }
                }
                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}

