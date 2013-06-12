namespace System.Configuration
{
    using System;
    using System.Configuration.Provider;

    public abstract class SettingsProvider : ProviderBase
    {
        protected SettingsProvider()
        {
        }

        public abstract SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection);
        public abstract void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection);

        public abstract string ApplicationName { get; set; }
    }
}

