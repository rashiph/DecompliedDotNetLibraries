namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data;
    using System.Data.Common;

    internal abstract class SmiConnection : IDisposable
    {
        protected SmiConnection()
        {
        }

        internal abstract void BeginTransaction(string name, IsolationLevel level, SmiEventSink eventSink);
        public virtual void Close(SmiEventSink eventSink)
        {
            ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        internal abstract void CommitTransaction(long transactionId, SmiEventSink eventSink);
        internal abstract void CreateTransactionSavePoint(long transactionId, string name, SmiEventSink eventSink);
        public virtual void Dispose()
        {
            ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        internal abstract void EnlistTransaction(byte[] token, SmiEventSink eventSink);
        internal abstract string GetCurrentDatabase(SmiEventSink eventSink);
        internal abstract byte[] GetDTCAddress(SmiEventSink eventSink);
        internal abstract byte[] PromoteTransaction(long transactionId, SmiEventSink eventSink);
        internal abstract void RollbackTransaction(long transactionId, string savePointName, SmiEventSink eventSink);
        internal abstract void SetCurrentDatabase(string databaseName, SmiEventSink eventSink);
    }
}

