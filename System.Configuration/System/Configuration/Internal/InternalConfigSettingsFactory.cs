namespace System.Configuration.Internal
{
    using System;
    using System.Configuration;

    internal sealed class InternalConfigSettingsFactory : IInternalConfigSettingsFactory
    {
        private InternalConfigSettingsFactory()
        {
        }

        void IInternalConfigSettingsFactory.CompleteInit()
        {
            ConfigurationManager.CompleteConfigInit();
        }

        void IInternalConfigSettingsFactory.SetConfigurationSystem(IInternalConfigSystem configSystem, bool initComplete)
        {
            ConfigurationManager.SetConfigurationSystem(configSystem, initComplete);
        }
    }
}

