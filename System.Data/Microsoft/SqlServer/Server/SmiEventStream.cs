namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data.Common;

    internal abstract class SmiEventStream : IDisposable
    {
        protected SmiEventStream()
        {
        }

        internal abstract void Close(SmiEventSink sink);
        public virtual void Dispose()
        {
            ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        internal abstract void ProcessEvent(SmiEventSink sink);

        internal abstract bool HasEvents { get; }
    }
}

