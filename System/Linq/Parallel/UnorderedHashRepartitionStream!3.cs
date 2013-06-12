namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class UnorderedHashRepartitionStream<TInputOutput, THashKey, TIgnoreKey> : HashRepartitionStream<TInputOutput, THashKey, int>
    {
        internal UnorderedHashRepartitionStream(PartitionedStream<TInputOutput, TIgnoreKey> inputStream, Func<TInputOutput, THashKey> keySelector, IEqualityComparer<THashKey> keyComparer, IEqualityComparer<TInputOutput> elementComparer, CancellationToken cancellationToken) : base(inputStream.PartitionCount, Util.GetDefaultComparer<int>(), keyComparer, elementComparer)
        {
            base.m_partitions = (QueryOperatorEnumerator<Pair<TInputOutput, THashKey>, int>[]) new HashRepartitionEnumerator<TInputOutput, THashKey, TIgnoreKey>[inputStream.PartitionCount];
            CountdownEvent barrier = new CountdownEvent(inputStream.PartitionCount);
            ListChunk<Pair<TInputOutput, THashKey>>[,] valueExchangeMatrix = new ListChunk<Pair<TInputOutput, THashKey>>[inputStream.PartitionCount, inputStream.PartitionCount];
            for (int i = 0; i < inputStream.PartitionCount; i++)
            {
                base.m_partitions[i] = new HashRepartitionEnumerator<TInputOutput, THashKey, TIgnoreKey>(inputStream[i], inputStream.PartitionCount, i, keySelector, this, barrier, valueExchangeMatrix, cancellationToken);
            }
        }
    }
}

