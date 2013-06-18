namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    internal interface IConnectionOrientedTransportFactorySettings : ITransportFactorySettings, IDefaultCommunicationTimeouts, IConnectionOrientedConnectionSettings
    {
        ServiceSecurityAuditBehavior AuditBehavior { get; }

        int MaxBufferSize { get; }

        System.ServiceModel.TransferMode TransferMode { get; }

        StreamUpgradeProvider Upgrade { get; }
    }
}

