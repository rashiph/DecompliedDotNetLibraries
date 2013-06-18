namespace System.ServiceModel.Security
{
    using System;

    internal interface IListenerSecureConversationSessionSettings
    {
        TimeSpan InactivityTimeout { get; set; }

        TimeSpan KeyRolloverInterval { get; set; }

        TimeSpan MaximumKeyRenewalInterval { get; set; }

        int MaximumPendingKeysPerSession { get; set; }

        int MaximumPendingSessions { get; set; }

        bool TolerateTransportFailures { get; set; }
    }
}

