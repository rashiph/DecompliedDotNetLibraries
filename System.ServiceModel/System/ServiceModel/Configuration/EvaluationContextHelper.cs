namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential), SecurityCritical(SecurityCriticalScope.Everything)]
    internal struct EvaluationContextHelper
    {
        private bool reset;
        private ContextInformation inheritedContext;
        internal void OnReset(ConfigurationElement parent)
        {
            this.reset = true;
            this.inheritedContext = ConfigurationHelpers.GetOriginalEvaluationContext(parent as IConfigurationContextProviderInternal);
        }

        internal ContextInformation GetOriginalContext(IConfigurationContextProviderInternal owner)
        {
            if (this.reset)
            {
                return this.inheritedContext;
            }
            return ConfigurationHelpers.GetEvaluationContext(owner);
        }
    }
}

