namespace System.Collections.Concurrent
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public abstract class OrderablePartitioner<TSource> : Partitioner<TSource>
    {
        protected OrderablePartitioner(bool keysOrderedInEachPartition, bool keysOrderedAcrossPartitions, bool keysNormalized)
        {
            this.KeysOrderedInEachPartition = keysOrderedInEachPartition;
            this.KeysOrderedAcrossPartitions = keysOrderedAcrossPartitions;
            this.KeysNormalized = keysNormalized;
        }

        public override IEnumerable<TSource> GetDynamicPartitions()
        {
            return new EnumerableDropIndices<TSource>(this.GetOrderableDynamicPartitions());
        }

        public virtual IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions()
        {
            throw new NotSupportedException(Environment.GetResourceString("Partitioner_DynamicPartitionsNotSupported"));
        }

        public abstract IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(int partitionCount);
        public override IList<IEnumerator<TSource>> GetPartitions(int partitionCount)
        {
            IList<IEnumerator<KeyValuePair<long, TSource>>> orderablePartitions = this.GetOrderablePartitions(partitionCount);
            if (orderablePartitions.Count != partitionCount)
            {
                throw new InvalidOperationException("OrderablePartitioner_GetPartitions_WrongNumberOfPartitions");
            }
            IEnumerator<TSource>[] enumeratorArray = new IEnumerator<TSource>[partitionCount];
            for (int i = 0; i < partitionCount; i++)
            {
                enumeratorArray[i] = new EnumeratorDropIndices<TSource>(orderablePartitions[i]);
            }
            return enumeratorArray;
        }

        public bool KeysNormalized { get; private set; }

        public bool KeysOrderedAcrossPartitions { get; private set; }

        public bool KeysOrderedInEachPartition { get; private set; }

        private class EnumerableDropIndices : IEnumerable<TSource>, IEnumerable, IDisposable
        {
            private readonly IEnumerable<KeyValuePair<long, TSource>> m_source;

            public EnumerableDropIndices(IEnumerable<KeyValuePair<long, TSource>> source)
            {
                this.m_source = source;
            }

            public void Dispose()
            {
                IDisposable source = this.m_source as IDisposable;
                if (source != null)
                {
                    source.Dispose();
                }
            }

            public IEnumerator<TSource> GetEnumerator()
            {
                return new OrderablePartitioner<TSource>.EnumeratorDropIndices(this.m_source.GetEnumerator());
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        private class EnumeratorDropIndices : IEnumerator<TSource>, IDisposable, IEnumerator
        {
            private readonly IEnumerator<KeyValuePair<long, TSource>> m_source;

            public EnumeratorDropIndices(IEnumerator<KeyValuePair<long, TSource>> source)
            {
                this.m_source = source;
            }

            public void Dispose()
            {
                this.m_source.Dispose();
            }

            public bool MoveNext()
            {
                return this.m_source.MoveNext();
            }

            public void Reset()
            {
                this.m_source.Reset();
            }

            public TSource Current
            {
                get
                {
                    return this.m_source.Current.Value;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }
    }
}

