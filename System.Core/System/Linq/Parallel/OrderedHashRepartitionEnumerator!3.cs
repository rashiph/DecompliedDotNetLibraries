namespace System.Linq.Parallel
{
    using System;
    using System.Threading;

    internal class OrderedHashRepartitionEnumerator<TInputOutput, THashKey, TOrderKey> : QueryOperatorEnumerator<Pair<TInputOutput, THashKey>, TOrderKey>
    {
        private const int ENUMERATION_NOT_STARTED = -1;
        private CountdownEvent m_barrier;
        private readonly CancellationToken m_cancellationToken;
        private readonly ListChunk<TOrderKey>[,] m_keyExchangeMatrix;
        private readonly Func<TInputOutput, THashKey> m_keySelector;
        private Mutables<TInputOutput, THashKey, TOrderKey> m_mutables;
        private readonly int m_partitionCount;
        private readonly int m_partitionIndex;
        private readonly HashRepartitionStream<TInputOutput, THashKey, TOrderKey> m_repartitionStream;
        private readonly QueryOperatorEnumerator<TInputOutput, TOrderKey> m_source;
        private readonly ListChunk<Pair<TInputOutput, THashKey>>[,] m_valueExchangeMatrix;

        internal OrderedHashRepartitionEnumerator(QueryOperatorEnumerator<TInputOutput, TOrderKey> source, int partitionCount, int partitionIndex, Func<TInputOutput, THashKey> keySelector, OrderedHashRepartitionStream<TInputOutput, THashKey, TOrderKey> repartitionStream, CountdownEvent barrier, ListChunk<Pair<TInputOutput, THashKey>>[,] valueExchangeMatrix, ListChunk<TOrderKey>[,] keyExchangeMatrix, CancellationToken cancellationToken)
        {
            this.m_source = source;
            this.m_partitionCount = partitionCount;
            this.m_partitionIndex = partitionIndex;
            this.m_keySelector = keySelector;
            this.m_repartitionStream = repartitionStream;
            this.m_barrier = barrier;
            this.m_valueExchangeMatrix = valueExchangeMatrix;
            this.m_keyExchangeMatrix = keyExchangeMatrix;
            this.m_cancellationToken = cancellationToken;
        }

        protected override void Dispose(bool disposing)
        {
            if (this.m_barrier != null)
            {
                if ((this.m_mutables == null) || (this.m_mutables.m_currentBufferIndex == -1))
                {
                    this.m_barrier.Signal();
                    this.m_barrier = null;
                }
                this.m_source.Dispose();
            }
        }

        private void EnumerateAndRedistributeElements()
        {
            Mutables<TInputOutput, THashKey, TOrderKey> mutables = this.m_mutables;
            ListChunk<Pair<TInputOutput, THashKey>>[] chunkArray = new ListChunk<Pair<TInputOutput, THashKey>>[this.m_partitionCount];
            ListChunk<TOrderKey>[] chunkArray2 = new ListChunk<TOrderKey>[this.m_partitionCount];
            TInputOutput currentElement = default(TInputOutput);
            TOrderKey currentKey = default(TOrderKey);
            int num = 0;
            while (this.m_source.MoveNext(ref currentElement, ref currentKey))
            {
                int num2;
                if ((num++ & 0x3f) == 0)
                {
                    CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                }
                THashKey key = default(THashKey);
                if (this.m_keySelector != null)
                {
                    key = this.m_keySelector(currentElement);
                    num2 = this.m_repartitionStream.GetHashCode(key) % this.m_partitionCount;
                }
                else
                {
                    num2 = this.m_repartitionStream.GetHashCode(currentElement) % this.m_partitionCount;
                }
                ListChunk<Pair<TInputOutput, THashKey>> chunk = chunkArray[num2];
                ListChunk<TOrderKey> chunk2 = chunkArray2[num2];
                if (chunk == null)
                {
                    chunkArray[num2] = chunk = new ListChunk<Pair<TInputOutput, THashKey>>(0x80);
                    chunkArray2[num2] = chunk2 = new ListChunk<TOrderKey>(0x80);
                }
                chunk.Add(new Pair<TInputOutput, THashKey>(currentElement, key));
                chunk2.Add(currentKey);
            }
            for (int i = 0; i < this.m_partitionCount; i++)
            {
                this.m_valueExchangeMatrix[this.m_partitionIndex, i] = chunkArray[i];
                this.m_keyExchangeMatrix[this.m_partitionIndex, i] = chunkArray2[i];
            }
            this.m_barrier.Signal();
            mutables.m_currentBufferIndex = this.m_partitionIndex;
            mutables.m_currentBuffer = chunkArray[this.m_partitionIndex];
            mutables.m_currentKeyBuffer = chunkArray2[this.m_partitionIndex];
            mutables.m_currentIndex = -1;
        }

        internal override bool MoveNext(ref Pair<TInputOutput, THashKey> currentElement, ref TOrderKey currentKey)
        {
            if (this.m_partitionCount == 1)
            {
                TInputOutput local = default(TInputOutput);
                if (this.m_source.MoveNext(ref local, ref currentKey))
                {
                    currentElement = new Pair<TInputOutput, THashKey>(local, (this.m_keySelector == null) ? default(THashKey) : this.m_keySelector(local));
                    return true;
                }
                return false;
            }
            Mutables<TInputOutput, THashKey, TOrderKey> mutables = this.m_mutables;
            if (mutables == null)
            {
                mutables = this.m_mutables = new Mutables<TInputOutput, THashKey, TOrderKey>();
            }
            if (mutables.m_currentBufferIndex == -1)
            {
                this.EnumerateAndRedistributeElements();
            }
            while (mutables.m_currentBufferIndex < this.m_partitionCount)
            {
                if (mutables.m_currentBuffer != null)
                {
                    if (++mutables.m_currentIndex < mutables.m_currentBuffer.Count)
                    {
                        currentElement = mutables.m_currentBuffer.m_chunk[mutables.m_currentIndex];
                        currentKey = mutables.m_currentKeyBuffer.m_chunk[mutables.m_currentIndex];
                        return true;
                    }
                    mutables.m_currentIndex = -1;
                    mutables.m_currentBuffer = mutables.m_currentBuffer.Next;
                    mutables.m_currentKeyBuffer = mutables.m_currentKeyBuffer.Next;
                }
                else
                {
                    if (mutables.m_currentBufferIndex == this.m_partitionIndex)
                    {
                        this.m_barrier.Wait(this.m_cancellationToken);
                        mutables.m_currentBufferIndex = -1;
                    }
                    mutables.m_currentBufferIndex++;
                    mutables.m_currentIndex = -1;
                    if (mutables.m_currentBufferIndex == this.m_partitionIndex)
                    {
                        mutables.m_currentBufferIndex++;
                    }
                    if (mutables.m_currentBufferIndex < this.m_partitionCount)
                    {
                        mutables.m_currentBuffer = this.m_valueExchangeMatrix[mutables.m_currentBufferIndex, this.m_partitionIndex];
                        mutables.m_currentKeyBuffer = this.m_keyExchangeMatrix[mutables.m_currentBufferIndex, this.m_partitionIndex];
                    }
                }
            }
            return false;
        }

        private class Mutables
        {
            internal ListChunk<Pair<TInputOutput, THashKey>> m_currentBuffer;
            internal int m_currentBufferIndex;
            internal int m_currentIndex;
            internal ListChunk<TOrderKey> m_currentKeyBuffer;

            internal Mutables()
            {
                this.m_currentBufferIndex = -1;
            }
        }
    }
}

