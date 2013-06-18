namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel;
    using System.Transactions;

    internal interface IInstanceTransaction
    {
        Transaction GetTransactionForInstance(OperationContext operationContext);
    }
}

