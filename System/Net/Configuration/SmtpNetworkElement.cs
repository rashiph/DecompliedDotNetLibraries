namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Net.Mail;

    public sealed class SmtpNetworkElement : ConfigurationElement
    {
        private readonly ConfigurationProperty clientDomain = new ConfigurationProperty("clientDomain", typeof(string), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty defaultCredentials = new ConfigurationProperty("defaultCredentials", typeof(bool), false, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty enableSsl = new ConfigurationProperty("enableSsl", typeof(bool), false, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty host = new ConfigurationProperty("host", typeof(string), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty password = new ConfigurationProperty("password", typeof(string), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty port = new ConfigurationProperty("port", typeof(int), 0x19, null, new IntegerValidator(1, 0xffff), ConfigurationPropertyOptions.None);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private readonly ConfigurationProperty targetName = new ConfigurationProperty("targetName", typeof(string), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty userName = new ConfigurationProperty("userName", typeof(string), null, ConfigurationPropertyOptions.None);

        public SmtpNetworkElement()
        {
            this.properties.Add(this.defaultCredentials);
            this.properties.Add(this.host);
            this.properties.Add(this.clientDomain);
            this.properties.Add(this.password);
            this.properties.Add(this.port);
            this.properties.Add(this.userName);
            this.properties.Add(this.targetName);
            this.properties.Add(this.enableSsl);
        }

        protected override void PostDeserialize()
        {
            if (!base.EvaluationContext.IsMachineLevel)
            {
                PropertyInformation information = base.ElementInformation.Properties["port"];
                if ((information.ValueOrigin == PropertyValueOrigin.SetHere) && (((int) information.Value) != ((int) information.DefaultValue)))
                {
                    try
                    {
                        new SmtpPermission(SmtpAccess.ConnectToUnrestrictedPort).Demand();
                    }
                    catch (Exception exception)
                    {
                        throw new ConfigurationErrorsException(System.SR.GetString("net_config_property_permission", new object[] { information.Name }), exception);
                    }
                }
            }
        }

        [ConfigurationProperty("clientDomain")]
        public string ClientDomain
        {
            get
            {
                return (string) base[this.clientDomain];
            }
            set
            {
                base[this.clientDomain] = value;
            }
        }

        [ConfigurationProperty("defaultCredentials", DefaultValue=false)]
        public bool DefaultCredentials
        {
            get
            {
                return (bool) base[this.defaultCredentials];
            }
            set
            {
                base[this.defaultCredentials] = value;
            }
        }

        [ConfigurationProperty("enableSsl", DefaultValue=false)]
        public bool EnableSsl
        {
            get
            {
                return (bool) base[this.enableSsl];
            }
            set
            {
                base[this.enableSsl] = value;
            }
        }

        [ConfigurationProperty("host")]
        public string Host
        {
            get
            {
                return (string) base[this.host];
            }
            set
            {
                base[this.host] = value;
            }
        }

        [ConfigurationProperty("password")]
        public string Password
        {
            get
            {
                return (string) base[this.password];
            }
            set
            {
                base[this.password] = value;
            }
        }

        [ConfigurationProperty("port", DefaultValue=0x19)]
        public int Port
        {
            get
            {
                return (int) base[this.port];
            }
            set
            {
                base[this.port] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty("targetName")]
        public string TargetName
        {
            get
            {
                return (string) base[this.targetName];
            }
            set
            {
                base[this.targetName] = value;
            }
        }

        [ConfigurationProperty("userName")]
        public string UserName
        {
            get
            {
                return (string) base[this.userName];
            }
            set
            {
                base[this.userName] = value;
            }
        }
    }
}

