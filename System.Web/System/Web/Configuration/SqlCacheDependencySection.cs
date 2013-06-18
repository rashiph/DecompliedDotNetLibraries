namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web;

    public sealed class SqlCacheDependencySection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _propDatabases = new ConfigurationProperty("databases", typeof(SqlCacheDependencyDatabaseCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propEnabled = new ConfigurationProperty("enabled", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propPollTime = new ConfigurationProperty("pollTime", typeof(int), 0xea60, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationElementProperty s_elemProperty = new ConfigurationElementProperty(new CallbackValidator(typeof(SqlCacheDependencySection), new ValidatorCallback(SqlCacheDependencySection.Validate)));

        static SqlCacheDependencySection()
        {
            _properties.Add(_propEnabled);
            _properties.Add(_propPollTime);
            _properties.Add(_propDatabases);
        }

        protected override void PostDeserialize()
        {
            int pollTime = this.PollTime;
            foreach (SqlCacheDependencyDatabase database in this.Databases)
            {
                database.CheckDefaultPollTime(pollTime);
            }
        }

        private static void Validate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("sqlCacheDependency");
            }
            SqlCacheDependencySection section = (SqlCacheDependencySection) value;
            int pollTime = section.PollTime;
            if ((pollTime != 0) && (pollTime < 500))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_sql_cache_dep_polltime"), section.ElementInformation.Properties["pollTime"].Source, section.ElementInformation.Properties["pollTime"].LineNumber);
            }
        }

        [ConfigurationProperty("databases")]
        public SqlCacheDependencyDatabaseCollection Databases
        {
            get
            {
                return (SqlCacheDependencyDatabaseCollection) base[_propDatabases];
            }
        }

        protected override ConfigurationElementProperty ElementProperty
        {
            get
            {
                return s_elemProperty;
            }
        }

        [ConfigurationProperty("enabled", DefaultValue=true)]
        public bool Enabled
        {
            get
            {
                return (bool) base[_propEnabled];
            }
            set
            {
                base[_propEnabled] = value;
            }
        }

        [ConfigurationProperty("pollTime", DefaultValue=0xea60)]
        public int PollTime
        {
            get
            {
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

