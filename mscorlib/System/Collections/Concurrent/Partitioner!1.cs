namespace System.Collections.Concurrent
{
    using System;
    using System.Collections.Generic;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public abstract class Partitioner<TSource>
    {
        protected Partitioner()
        {
        }

        public virtual IEnumerable<TSource> GetDynamicPartitions()
        {
            throw new NotSupportedException(Environment.GetResourceString("Partitioner_DynamicPartitionsNotSupported"));
        }

        public abstract IList<IEnumerator<TSource>> GetPartitions(int partitionCount);

        public virtual bool SupportsDynamicPartitions
        {
            get
            {
                return false;
            }
        }
    }
}

