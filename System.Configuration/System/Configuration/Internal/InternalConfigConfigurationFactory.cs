namespace System.Configuration.Internal
{
    using System;
    using System.Configuration;

    internal sealed class InternalConfigConfigurationFactory : IInternalConfigConfigurationFactory
    {
        private InternalConfigConfigurationFactory()
        {
        }

        System.Configuration.Configuration IInternalConfigConfigurationFactory.Create(Type typeConfigHost, params object[] hostInitConfigurationParams)
        {
            return new System.Configuration.Configuration(null, typeConfigHost, hostInitConfigurationParams);
        }

        string IInternalConfigConfigurationFactory.NormalizeLocationSubPath(string subPath, IConfigErrorInfo errorInfo)
        {
            return BaseConfigurationRecord.NormalizeLocationSubPath(subPath, errorInfo);
        }
    }
}

