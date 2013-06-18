namespace System.Activities.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public abstract class PersistenceParticipant : IPersistencePipelineModule
    {
        private bool isIOParticipant;
        private bool isLoadTransactionRequired;
        private bool isSaveTransactionRequired;

        protected PersistenceParticipant()
        {
        }

        internal PersistenceParticipant(bool isSaveTransactionRequired, bool isLoadTransactionRequired)
        {
            this.isIOParticipant = true;
            this.isSaveTransactionRequired = isSaveTransactionRequired;
            this.isLoadTransactionRequired = isLoadTransactionRequired;
        }

        protected virtual void CollectValues(out IDictionary<XName, object> readWriteValues, out IDictionary<XName, object> writeOnlyValues)
        {
            readWriteValues = null;
            writeOnlyValues = null;
        }

        internal virtual void InternalAbort()
        {
        }

        internal virtual IAsyncResult InternalBeginOnLoad(IDictionary<XName, object> readWriteValues, TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw Fx.AssertAndThrow("BeginOnLoad should not be called on PersistenceParticipant.");
        }

        internal virtual IAsyncResult InternalBeginOnSave(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues, TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw Fx.AssertAndThrow("BeginOnSave should not be called on PersistenceParticipant.");
        }

        internal virtual void InternalEndOnLoad(IAsyncResult result)
        {
        }

        internal virtual void InternalEndOnSave(IAsyncResult result)
        {
        }

        protected virtual IDictionary<XName, object> MapValues(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues)
        {
            return null;
        }

        protected virtual void PublishValues(IDictionary<XName, object> readWriteValues)
        {
        }

        void IPersistencePipelineModule.Abort()
        {
            this.InternalAbort();
        }

        IAsyncResult IPersistencePipelineModule.BeginOnLoad(IDictionary<XName, object> readWriteValues, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InternalBeginOnLoad(readWriteValues, timeout, callback, state);
        }

        IAsyncResult IPersistencePipelineModule.BeginOnSave(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InternalBeginOnSave(readWriteValues, writeOnlyValues, timeout, callback, state);
        }

        void IPersistencePipelineModule.CollectValues(out IDictionary<XName, object> readWriteValues, out IDictionary<XName, object> writeOnlyValues)
        {
            this.CollectValues(out readWriteValues, out writeOnlyValues);
        }

        void IPersistencePipelineModule.EndOnLoad(IAsyncResult result)
        {
            this.InternalEndOnLoad(result);
        }

        void IPersistencePipelineModule.EndOnSave(IAsyncResult result)
        {
            this.InternalEndOnSave(result);
        }

        IDictionary<XName, object> IPersistencePipelineModule.MapValues(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues)
        {
            return this.MapValues(readWriteValues, writeOnlyValues);
        }

        void IPersistencePipelineModule.PublishValues(IDictionary<XName, object> readWriteValues)
        {
            this.PublishValues(readWriteValues);
        }

        bool IPersistencePipelineModule.IsIOParticipant
        {
            get
            {
                return this.isIOParticipant;
            }
        }

        bool IPersistencePipelineModule.IsLoadTransactionRequired
        {
            get
            {
                return this.isLoadTransactionRequired;
            }
        }

        bool IPersistencePipelineModule.IsSaveTransactionRequired
        {
            get
            {
                return this.isSaveTransactionRequired;
            }
        }
    }
}

