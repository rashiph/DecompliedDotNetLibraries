namespace System.ServiceModel.Security
{
    using System;

    internal interface IChannelSecureConversationSessionSettings
    {
        TimeSpan KeyRenewalInterval { get; set; }

        TimeSpan KeyRolloverInterval { get; set; }

        bool TolerateTransportFailures { get; set; }
    }
}

