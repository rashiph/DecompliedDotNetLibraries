namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class OrderedHashRepartitionStream<TInputOutput, THashKey, TOrderKey> : HashRepartitionStream<TInputOutput, THashKey, TOrderKey>
    {
        internal OrderedHashRepartitionStream(PartitionedStream<TInputOutput, TOrderKey> inputStream, Func<TInputOutput, THashKey> hashKeySelector, IEqualityComparer<THashKey> hashKeyComparer, IEqualityComparer<TInputOutput> elementComparer, CancellationToken cancellationToken) : base(inputStream.PartitionCount, inputStream.KeyComparer, hashKeyComparer, elementComparer)
        {
            base.m_partitions = (QueryOperatorEnumerator<Pair<TInputOutput, THashKey>, TOrderKey>[]) new OrderedHashRepartitionEnumerator<TInputOutput, THashKey, TOrderKey>[inputStream.PartitionCount];
            CountdownEvent barrier = new CountdownEvent(inputStream.PartitionCount);
            ListChunk<Pair<TInputOutput, THashKey>>[,] valueExchangeMatrix = new ListChunk<Pair<TInputOutput, THashKey>>[inputStream.PartitionCount, inputStream.PartitionCount];
            ListChunk<TOrderKey>[,] keyExchangeMatrix = new ListChunk<TOrderKey>[inputStream.PartitionCount, inputStream.PartitionCount];
            for (int i = 0; i < inputStream.PartitionCount; i++)
            {
                base.m_partitions[i] = new OrderedHashRepartitionEnumerator<TInputOutput, THashKey, TOrderKey>(inputStream[i], inputStream.PartitionCount, i, hashKeySelector, (OrderedHashRepartitionStream<TInputOutput, THashKey, TOrderKey>) this, barrier, valueExchangeMatrix, keyExchangeMatrix, cancellationToken);
            }
        }
    }
}

