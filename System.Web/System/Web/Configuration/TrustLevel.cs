namespace System.Web.Configuration
{
    using System;
    using System.Configuration;

    public sealed class TrustLevel : ConfigurationElement
    {
        private string _LegacyPolicyFileExpanded;
        private string _PolicyFileExpanded;
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), "Full", null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propPolicyFile = new ConfigurationProperty("policyFile", typeof(string), "internal", ConfigurationPropertyOptions.IsRequired);

        static TrustLevel()
        {
            _properties.Add(_propName);
            _properties.Add(_propPolicyFile);
        }

        internal TrustLevel()
        {
        }

        public TrustLevel(string name, string policyFile)
        {
            this.Name = name;
            this.PolicyFile = policyFile;
        }

        internal string LegacyPolicyFileExpanded
        {
            get
            {
                if (this._LegacyPolicyFileExpanded == null)
                {
                    string source = base.ElementInformation.Properties["policyFile"].Source;
                    string str2 = source.Substring(0, source.LastIndexOf('\\') + 1);
                    bool flag = true;
                    if (this.PolicyFile.Length > 1)
                    {
                        char ch = this.PolicyFile[1];
                        char ch2 = this.PolicyFile[0];
                        if (ch == ':')
                        {
                            flag = false;
                        }
                        else if ((ch2 == '\\') && (ch == '\\'))
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        this._LegacyPolicyFileExpanded = str2 + "legacy." + this.PolicyFile;
                    }
                    else
                    {
                        this._LegacyPolicyFileExpanded = this.PolicyFile;
                    }
                }
                return this._LegacyPolicyFileExpanded;
            }
        }

        [ConfigurationProperty("name", IsRequired=true, DefaultValue="Full", IsKey=true), StringValidator(MinLength=1)]
        public string Name
        {
            get
            {
                return (string) base[_propName];
            }
            set
            {
                base[_propName] = value;
            }
        }

        [ConfigurationProperty("policyFile", IsRequired=true, DefaultValue="internal")]
        public string PolicyFile
        {
            get
            {
                return (string) base[_propPolicyFile];
            }
            set
            {
                base[_propPolicyFile] = value;
            }
        }

        internal string PolicyFileExpanded
        {
            get
            {
                if (this._PolicyFileExpanded == null)
                {
                    string source = base.ElementInformation.Properties["policyFile"].Source;
                    string str2 = source.Substring(0, source.LastIndexOf('\\') + 1);
                    bool flag = true;
                    if (this.PolicyFile.Length > 1)
                    {
                        char ch = this.PolicyFile[1];
                        char ch2 = this.PolicyFile[0];
                        if (ch == ':')
                        {
                            flag = false;
                        }
                        else if ((ch2 == '\\') && (ch == '\\'))
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        this._PolicyFileExpanded = str2 + this.PolicyFile;
                    }
                    else
                    {
                        this._PolicyFileExpanded = this.PolicyFile;
                    }
                }
                return this._PolicyFileExpanded;
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

