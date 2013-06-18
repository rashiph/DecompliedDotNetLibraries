namespace System.ServiceModel.Activation
{
    using System;
    using System.ServiceModel;

    internal interface IConnectionDuplicator
    {
        [OperationContract(IsOneWay=false, AsyncPattern=true)]
        IAsyncResult BeginDuplicate(DuplicateContext duplicateContext, AsyncCallback callback, object state);
        void EndDuplicate(IAsyncResult result);
    }
}

