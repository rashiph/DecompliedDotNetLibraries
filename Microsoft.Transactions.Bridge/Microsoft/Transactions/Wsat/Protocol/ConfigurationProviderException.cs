namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime;

    internal class ConfigurationProviderException : Exception
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConfigurationProviderException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConfigurationProviderException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

