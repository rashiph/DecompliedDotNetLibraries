namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data.Common;

    internal abstract class SmiEventSink
    {
        protected SmiEventSink()
        {
        }

        internal abstract void BatchCompleted();
        internal abstract void DefaultDatabaseChanged(string databaseName);
        internal abstract void MessagePosted(int number, byte state, byte errorClass, string server, string message, string procedure, int lineNumber);
        internal abstract void MetaDataAvailable(SmiQueryMetaData[] metaData, bool nextEventIsRow);
        internal virtual void ParameterAvailable(SmiParameterMetaData metaData, SmiTypedGetterSetter paramValue, int ordinal)
        {
            ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        internal virtual void ParametersAvailable(SmiParameterMetaData[] metaData, ITypedGettersV3 paramValues)
        {
            ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        internal virtual void RowAvailable(ITypedGetters rowData)
        {
            ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        internal virtual void RowAvailable(ITypedGettersV3 rowData)
        {
            ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        internal virtual void RowAvailable(SmiTypedGetterSetter rowData)
        {
            ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        internal abstract void StatementCompleted(int rowsAffected);
        internal abstract void TransactionCommitted(long transactionId);
        internal abstract void TransactionDefected(long transactionId);
        internal abstract void TransactionEnded(long transactionId);
        internal abstract void TransactionEnlisted(long transactionId);
        internal abstract void TransactionRolledBack(long transactionId);
        internal abstract void TransactionStarted(long transactionId);
    }
}

