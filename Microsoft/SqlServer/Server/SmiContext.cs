namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Transactions;

    internal abstract class SmiContext
    {
        internal abstract event EventHandler OutOfScope;

        protected SmiContext()
        {
        }

        internal abstract SmiRecordBuffer CreateRecordBuffer(SmiExtendedMetaData[] columnMetaData, SmiEventSink eventSink);
        internal abstract SmiRequestExecutor CreateRequestExecutor(string commandText, CommandType commandType, SmiParameterMetaData[] parameterMetaData, SmiEventSink eventSink);
        internal abstract object GetContextValue(int key);
        internal virtual SmiStream GetScratchStream(SmiEventSink sink)
        {
            ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
            return null;
        }

        internal abstract void GetTriggerInfo(SmiEventSink eventSink, out bool[] columnsUpdated, out TriggerAction action, out SqlXml eventInstanceData);
        internal abstract void SendMessageToPipe(string message, SmiEventSink eventSink);
        internal abstract void SendResultsEndToPipe(SmiRecordBuffer recordBuffer, SmiEventSink eventSink);
        internal abstract void SendResultsRowToPipe(SmiRecordBuffer recordBuffer, SmiEventSink eventSink);
        internal abstract void SendResultsStartToPipe(SmiRecordBuffer recordBuffer, SmiEventSink eventSink);
        internal abstract void SetContextValue(int key, object value);

        internal abstract SmiConnection ContextConnection { get; }

        internal abstract Transaction ContextTransaction { get; }

        internal abstract long ContextTransactionId { get; }

        internal abstract bool HasContextPipe { get; }

        internal abstract System.Security.Principal.WindowsIdentity WindowsIdentity { get; }
    }
}

