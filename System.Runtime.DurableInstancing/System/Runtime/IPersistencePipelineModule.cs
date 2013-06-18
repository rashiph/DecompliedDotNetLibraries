namespace System.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal interface IPersistencePipelineModule
    {
        void Abort();
        IAsyncResult BeginOnLoad(IDictionary<XName, object> readWriteValues, TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginOnSave(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues, TimeSpan timeout, AsyncCallback callback, object state);
        void CollectValues(out IDictionary<XName, object> readWriteValues, out IDictionary<XName, object> writeOnlyValues);
        void EndOnLoad(IAsyncResult result);
        void EndOnSave(IAsyncResult result);
        IDictionary<XName, object> MapValues(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues);
        void PublishValues(IDictionary<XName, object> readWriteValues);

        bool IsIOParticipant { get; }

        bool IsLoadTransactionRequired { get; }

        bool IsSaveTransactionRequired { get; }
    }
}

