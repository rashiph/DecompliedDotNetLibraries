namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;

    internal interface ITransactionChannelManager
    {
        TransactionFlowOption GetTransaction(MessageDirection direction, string action);

        IDictionary<DirectionalAction, TransactionFlowOption> Dictionary { get; }

        TransactionFlowOption FlowIssuedTokens { get; set; }

        SecurityStandardsManager StandardsManager { get; }

        System.ServiceModel.TransactionProtocol TransactionProtocol { get; set; }
    }
}

