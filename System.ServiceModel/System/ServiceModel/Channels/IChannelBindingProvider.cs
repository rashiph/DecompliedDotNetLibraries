namespace System.ServiceModel.Channels
{
    using System;

    internal interface IChannelBindingProvider
    {
        void EnableChannelBindingSupport();

        bool IsChannelBindingSupportEnabled { get; }
    }
}

