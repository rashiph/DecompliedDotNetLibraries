namespace System.Data
{
    using System.Configuration;

    internal sealed class LocalDBConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("localdbinstances", IsRequired=true)]
        public LocalDBInstancesCollection LocalDbInstances
        {
            get
            {
                return (((LocalDBInstancesCollection) base["localdbinstances"]) ?? new LocalDBInstancesCollection());
            }
        }
    }
}

