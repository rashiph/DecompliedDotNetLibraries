namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web;

    public sealed class SqlCacheDependencyDatabase : ConfigurationElement
    {
        private static readonly ConfigurationProperty _propConnectionStringName = new ConfigurationProperty("connectionStringName", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsRequired);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propPollTime = new ConfigurationProperty("pollTime", typeof(int), 0xea60, ConfigurationPropertyOptions.None);
        private int defaultPollTime;
        private static readonly ConfigurationElementProperty s_elemProperty = new ConfigurationElementProperty(new CallbackValidator(typeof(SqlCacheDependencyDatabase), new ValidatorCallback(SqlCacheDependencyDatabase.Validate)));

        static SqlCacheDependencyDatabase()
        {
            _properties.Add(_propName);
            _properties.Add(_propConnectionStringName);
            _properties.Add(_propPollTime);
        }

        internal SqlCacheDependencyDatabase()
        {
        }

        public SqlCacheDependencyDatabase(string name, string connectionStringName)
        {
            this.Name = name;
            this.ConnectionStringName = connectionStringName;
        }

        public SqlCacheDependencyDatabase(string name, string connectionStringName, int pollTime)
        {
            this.Name = name;
            this.ConnectionStringName = connectionStringName;
            this.PollTime = pollTime;
        }

        internal void CheckDefaultPollTime(int value)
        {
            if (base.ElementInformation.Properties["pollTime"].ValueOrigin == PropertyValueOrigin.Default)
            {
                this.defaultPollTime = value;
            }
        }

        private static void Validate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("sqlCacheDependencyDatabase");
            }
            SqlCacheDependencyDatabase database = (SqlCacheDependencyDatabase) value;
            if ((database.PollTime != 0) && (database.PollTime < 500))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_sql_cache_dep_polltime"), database.ElementInformation.Properties["pollTime"].Source, database.ElementInformation.Properties["pollTime"].LineNumber);
            }
        }

        [ConfigurationProperty("connectionStringName", IsRequired=true), StringValidator(MinLength=1)]
        public string ConnectionStringName
        {
            get
            {
                return (string) base[_propConnectionStringName];
            }
            set
            {
                base[_propConnectionStringName] = value;
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
            set
            {
                base[_propName] = value;
            }
        }

        [ConfigurationProperty("pollTime", DefaultValue=0xea60)]
        public int PollTime
        {
            get
            {
                if (base.ElementInformation.Properties["pollTime"].ValueOrigin == PropertyValueOrigin.Default)
                {
                    return this.defaultPollTime;
                }
                return (int) base[_propPollTime];
            }
            set
            {
                base[_propPollTime] = value;
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

