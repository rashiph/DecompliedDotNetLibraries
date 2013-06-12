namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal static class ExchangeUtilities
    {
        internal static PartitionedStream<Pair<TElement, THashKey>, int> HashRepartition<TElement, THashKey, TIgnoreKey>(PartitionedStream<TElement, TIgnoreKey> source, Func<TElement, THashKey> keySelector, IEqualityComparer<THashKey> keyComparer, IEqualityComparer<TElement> elementComparer, CancellationToken cancellationToken)
        {
            return new UnorderedHashRepartitionStream<TElement, THashKey, TIgnoreKey>(source, keySelector, keyComparer, elementComparer, cancellationToken);
        }

        internal static PartitionedStream<Pair<TElement, THashKey>, TOrderKey> HashRepartitionOrdered<TElement, THashKey, TOrderKey>(PartitionedStream<TElement, TOrderKey> source, Func<TElement, THashKey> keySelector, IEqualityComparer<THashKey> keyComparer, IEqualityComparer<TElement> elementComparer, CancellationToken cancellationToken)
        {
            return new OrderedHashRepartitionStream<TElement, THashKey, TOrderKey>(source, keySelector, keyComparer, elementComparer, cancellationToken);
        }

        internal static bool IsWorseThan(this OrdinalIndexState state1, OrdinalIndexState state2)
        {
            return (state1 > state2);
        }

        internal static PartitionedStream<T, int> PartitionDataSource<T>(IEnumerable<T> source, int partitionCount, bool useStriping)
        {
            IParallelPartitionable<T> partitionable = source as IParallelPartitionable<T>;
            if (partitionable != null)
            {
                QueryOperatorEnumerator<T, int>[] partitions = partitionable.GetPartitions(partitionCount);
                if (partitions == null)
                {
                    throw new InvalidOperationException(System.Linq.SR.GetString("ParallelPartitionable_NullReturn"));
                }
                if (partitions.Length != partitionCount)
                {
                    throw new InvalidOperationException(System.Linq.SR.GetString("ParallelPartitionable_IncorretElementCount"));
                }
                PartitionedStream<T, int> stream2 = new PartitionedStream<T, int>(partitionCount, Util.GetDefaultComparer<int>(), OrdinalIndexState.Correct);
                for (int i = 0; i < partitionCount; i++)
                {
                    QueryOperatorEnumerator<T, int> enumerator = partitions[i];
                    if (enumerator == null)
                    {
                        throw new InvalidOperationException(System.Linq.SR.GetString("ParallelPartitionable_NullElement"));
                    }
                    stream2[i] = enumerator;
                }
                return stream2;
            }
            return new PartitionedDataSource<T>(source, partitionCount, useStriping);
        }

        internal static OrdinalIndexState Worse(this OrdinalIndexState state1, OrdinalIndexState state2)
        {
            if (state1 <= state2)
            {
                return state2;
            }
            return state1;
        }
    }
}

