namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.CompilerServices;

    internal class ChannelBindingProviderHelper : IChannelBindingProvider
    {
        public void EnableChannelBindingSupport()
        {
            this.IsChannelBindingSupportEnabled = true;
        }

        public bool IsChannelBindingSupportEnabled { get; private set; }
    }
}

