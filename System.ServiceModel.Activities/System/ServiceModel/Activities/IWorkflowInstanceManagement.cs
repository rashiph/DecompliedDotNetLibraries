namespace System.ServiceModel.Activities
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Description;

    [ServiceContract(Name="IWorkflowInstanceManagement", Namespace="http://schemas.datacontract.org/2008/10/WorkflowServices"), WorkflowContractBehavior]
    public interface IWorkflowInstanceManagement
    {
        [OperationContract(Name="Abandon")]
        void Abandon(Guid instanceId, string reason);
        [OperationContract(Name="Abandon", AsyncPattern=true)]
        IAsyncResult BeginAbandon(Guid instanceId, string reason, AsyncCallback callback, object state);
        [OperationContract(Name="Cancel", AsyncPattern=true)]
        IAsyncResult BeginCancel(Guid instanceId, AsyncCallback callback, object state);
        [OperationContract(Name="Run", AsyncPattern=true)]
        IAsyncResult BeginRun(Guid instanceId, AsyncCallback callback, object state);
        [OperationContract(Name="Suspend", AsyncPattern=true)]
        IAsyncResult BeginSuspend(Guid instanceId, string reason, AsyncCallback callback, object state);
        [OperationContract(Name="Terminate", AsyncPattern=true)]
        IAsyncResult BeginTerminate(Guid instanceId, string reason, AsyncCallback callback, object state);
        [OperationContract(Name="TransactedCancel", AsyncPattern=true)]
        IAsyncResult BeginTransactedCancel(Guid instanceId, AsyncCallback callback, object state);
        [OperationContract(Name="TransactedRun", AsyncPattern=true)]
        IAsyncResult BeginTransactedRun(Guid instanceId, AsyncCallback callback, object state);
        [OperationContract(AsyncPattern=true, Name="TransactedSuspend")]
        IAsyncResult BeginTransactedSuspend(Guid instanceId, string reason, AsyncCallback callback, object state);
        [OperationContract(AsyncPattern=true, Name="TransactedTerminate")]
        IAsyncResult BeginTransactedTerminate(Guid instanceId, string reason, AsyncCallback callback, object state);
        [OperationContract(AsyncPattern=true, Name="TransactedUnsuspend")]
        IAsyncResult BeginTransactedUnsuspend(Guid instanceId, AsyncCallback callback, object state);
        [OperationContract(Name="Unsuspend", AsyncPattern=true)]
        IAsyncResult BeginUnsuspend(Guid instanceId, AsyncCallback callback, object state);
        [OperationContract(Name="Cancel")]
        void Cancel(Guid instanceId);
        void EndAbandon(IAsyncResult result);
        void EndCancel(IAsyncResult result);
        void EndRun(IAsyncResult result);
        void EndSuspend(IAsyncResult result);
        void EndTerminate(IAsyncResult result);
        void EndTransactedCancel(IAsyncResult result);
        void EndTransactedRun(IAsyncResult result);
        void EndTransactedSuspend(IAsyncResult result);
        void EndTransactedTerminate(IAsyncResult result);
        void EndTransactedUnsuspend(IAsyncResult result);
        void EndUnsuspend(IAsyncResult result);
        [OperationContract(Name="Run")]
        void Run(Guid instanceId);
        [OperationContract(Name="Suspend")]
        void Suspend(Guid instanceId, string reason);
        [OperationContract(Name="Terminate")]
        void Terminate(Guid instanceId, string reason);
        [OperationContract(Name="TransactedCancel"), TransactionFlow(TransactionFlowOption.Allowed)]
        void TransactedCancel(Guid instanceId);
        [OperationContract(Name="TransactedRun"), TransactionFlow(TransactionFlowOption.Allowed)]
        void TransactedRun(Guid instanceId);
        [OperationContract(Name="TransactedSuspend"), TransactionFlow(TransactionFlowOption.Allowed)]
        void TransactedSuspend(Guid instanceId, string reason);
        [TransactionFlow(TransactionFlowOption.Allowed), OperationContract(Name="TransactedTerminate")]
        void TransactedTerminate(Guid instanceId, string reason);
        [OperationContract(Name="TransactedUnsuspend"), TransactionFlow(TransactionFlowOption.Allowed)]
        void TransactedUnsuspend(Guid instanceId);
        [OperationContract(Name="Unsuspend")]
        void Unsuspend(Guid instanceId);
    }
}

