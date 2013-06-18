namespace System.Web
{
    using System;
    using System.Collections.Specialized;
    using System.Threading;

    internal class PartitionManager : IDisposable
    {
        private CreatePartitionInfo _createCallback;
        private ReaderWriterLock _lock = new ReaderWriterLock();
        private HybridDictionary _partitions = new HybridDictionary();

        internal PartitionManager(CreatePartitionInfo createCallback)
        {
            this._createCallback = createCallback;
        }

        public void Dispose()
        {
            if (this._partitions != null)
            {
                try
                {
                    this._lock.AcquireWriterLock(-1);
                    if (this._partitions != null)
                    {
                        foreach (PartitionInfo info in this._partitions.Values)
                        {
                            info.Dispose();
                        }
                        this._partitions = null;
                    }
                }
                catch
                {
                }
                finally
                {
                    if (this._lock.IsWriterLockHeld)
                    {
                        this._lock.ReleaseWriterLock();
                    }
                }
            }
        }

        internal object GetPartition(IPartitionResolver partitionResolver, string id)
        {
            object obj2;
            if (EtwTrace.IsTraceEnabled(5, 1))
            {
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_SESSIONSTATE_PARTITION_START, HttpContext.Current.WorkerRequest, partitionResolver.GetType().FullName, id);
            }
            string connectionString = null;
            string message = null;
            IPartitionInfo info = null;
            try
            {
                try
                {
                    connectionString = partitionResolver.ResolvePartition(id);
                    if (connectionString == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("Bad_partition_resolver_connection_string", new object[] { partitionResolver.GetType().FullName }));
                    }
                }
                catch (Exception exception)
                {
                    message = exception.Message;
                    throw;
                }
                try
                {
                    this._lock.AcquireReaderLock(-1);
                    info = (IPartitionInfo) this._partitions[connectionString];
                    if (info != null)
                    {
                        return info;
                    }
                }
                finally
                {
                    if (this._lock.IsReaderLockHeld)
                    {
                        this._lock.ReleaseReaderLock();
                    }
                }
                try
                {
                    this._lock.AcquireWriterLock(-1);
                    info = (IPartitionInfo) this._partitions[connectionString];
                    if (info == null)
                    {
                        info = this._createCallback(connectionString);
                        this._partitions.Add(connectionString, info);
                    }
                    return info;
                }
                finally
                {
                    if (this._lock.IsWriterLockHeld)
                    {
                        this._lock.ReleaseWriterLock();
                    }
                }
            }
            finally
            {
                if (EtwTrace.IsTraceEnabled(5, 1))
                {
                    string tracingPartitionString = message;
                    if (tracingPartitionString == null)
                    {
                        if (info != null)
                        {
                            tracingPartitionString = info.GetTracingPartitionString();
                        }
                        else
                        {
                            tracingPartitionString = string.Empty;
                        }
                    }
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_SESSIONSTATE_PARTITION_END, HttpContext.Current.WorkerRequest, tracingPartitionString);
                }
            }
            return obj2;
        }
    }
}

