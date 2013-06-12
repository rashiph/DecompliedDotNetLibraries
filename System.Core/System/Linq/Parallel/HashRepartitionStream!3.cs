namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;

    internal abstract class HashRepartitionStream<TInputOutput, THashKey, TOrderKey> : PartitionedStream<Pair<TInputOutput, THashKey>, TOrderKey>
    {
        private readonly int m_distributionMod;
        private readonly IEqualityComparer<TInputOutput> m_elementComparer;
        private readonly IEqualityComparer<THashKey> m_keyComparer;
        private const int NULL_ELEMENT_HASH_CODE = 0;

        internal HashRepartitionStream(int partitionsCount, IComparer<TOrderKey> orderKeyComparer, IEqualityComparer<THashKey> hashKeyComparer, IEqualityComparer<TInputOutput> elementComparer) : base(partitionsCount, orderKeyComparer, OrdinalIndexState.Shuffled)
        {
            this.m_keyComparer = hashKeyComparer;
            this.m_elementComparer = elementComparer;
            this.m_distributionMod = 0x1f7;
            while (this.m_distributionMod < partitionsCount)
            {
                this.m_distributionMod *= 2;
            }
        }

        internal int GetHashCode(THashKey key)
        {
            return ((0x7fffffff & ((this.m_keyComparer == null) ? ((key == null) ? 0 : key.GetHashCode()) : this.m_keyComparer.GetHashCode(key))) % this.m_distributionMod);
        }

        internal int GetHashCode(TInputOutput element)
        {
            return ((0x7fffffff & ((this.m_elementComparer == null) ? ((element == null) ? 0 : element.GetHashCode()) : this.m_elementComparer.GetHashCode(element))) % this.m_distributionMod);
        }
    }
}

