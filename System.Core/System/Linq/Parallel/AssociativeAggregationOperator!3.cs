namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class AssociativeAggregationOperator<TInput, TIntermediate, TOutput> : UnaryQueryOperator<TInput, TIntermediate>
    {
        private Func<TIntermediate, TIntermediate, TIntermediate> m_finalReduce;
        private Func<TIntermediate, TInput, TIntermediate> m_intermediateReduce;
        private Func<TIntermediate, TOutput> m_resultSelector;
        private readonly TIntermediate m_seed;
        private Func<TIntermediate> m_seedFactory;
        private readonly bool m_seedIsSpecified;
        private readonly bool m_throwIfEmpty;

        internal AssociativeAggregationOperator(IEnumerable<TInput> child, TIntermediate seed, Func<TIntermediate> seedFactory, bool seedIsSpecified, Func<TIntermediate, TInput, TIntermediate> intermediateReduce, Func<TIntermediate, TIntermediate, TIntermediate> finalReduce, Func<TIntermediate, TOutput> resultSelector, bool throwIfEmpty, QueryAggregationOptions options) : base(child)
        {
            this.m_seed = seed;
            this.m_seedFactory = seedFactory;
            this.m_seedIsSpecified = seedIsSpecified;
            this.m_intermediateReduce = intermediateReduce;
            this.m_finalReduce = finalReduce;
            this.m_resultSelector = resultSelector;
            this.m_throwIfEmpty = throwIfEmpty;
        }

        internal TOutput Aggregate()
        {
            TOutput local2;
            TIntermediate current = default(TIntermediate);
            bool flag = false;
            using (IEnumerator<TIntermediate> enumerator = this.GetEnumerator(3, true))
            {
                while (enumerator.MoveNext())
                {
                    if (flag)
                    {
                        try
                        {
                            current = this.m_finalReduce(current, enumerator.Current);
                            continue;
                        }
                        catch (ThreadAbortException)
                        {
                            throw;
                        }
                        catch (Exception exception)
                        {
                            throw new AggregateException(new Exception[] { exception });
                        }
                    }
                    current = enumerator.Current;
                    flag = true;
                }
                if (!flag)
                {
                    if (this.m_throwIfEmpty)
                    {
                        throw new InvalidOperationException(System.Linq.SR.GetString("NoElements"));
                    }
                    current = (this.m_seedFactory == null) ? this.m_seed : this.m_seedFactory();
                }
            }
            try
            {
                local2 = this.m_resultSelector(current);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception exception2)
            {
                throw new AggregateException(new Exception[] { exception2 });
            }
            return local2;
        }

        internal override IEnumerable<TIntermediate> AsSequentialQuery(CancellationToken token)
        {
            throw new NotSupportedException();
        }

        internal override QueryResults<TIntermediate> Open(QuerySettings settings, bool preferStriping)
        {
            return new UnaryQueryOperator<TInput, TIntermediate>.UnaryQueryOperatorResults(base.Child.Open(settings, preferStriping), this, settings, preferStriping);
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TInput, TKey> inputStream, IPartitionedStreamRecipient<TIntermediate> recipient, bool preferStriping, QuerySettings settings)
        {
            int partitionCount = inputStream.PartitionCount;
            PartitionedStream<TIntermediate, int> partitionedStream = new PartitionedStream<TIntermediate, int>(partitionCount, Util.GetDefaultComparer<int>(), OrdinalIndexState.Correct);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new AssociativeAggregationOperatorEnumerator<TInput, TIntermediate, TOutput, TKey>(inputStream[i], (AssociativeAggregationOperator<TInput, TIntermediate, TOutput>) this, i, settings.CancellationState.MergedCancellationToken);
            }
            recipient.Receive<int>(partitionedStream);
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return false;
            }
        }

        private class AssociativeAggregationOperatorEnumerator<TKey> : QueryOperatorEnumerator<TIntermediate, int>
        {
            private bool m_accumulated;
            private readonly CancellationToken m_cancellationToken;
            private readonly int m_partitionIndex;
            private readonly AssociativeAggregationOperator<TInput, TIntermediate, TOutput> m_reduceOperator;
            private readonly QueryOperatorEnumerator<TInput, TKey> m_source;

            internal AssociativeAggregationOperatorEnumerator(QueryOperatorEnumerator<TInput, TKey> source, AssociativeAggregationOperator<TInput, TIntermediate, TOutput> reduceOperator, int partitionIndex, CancellationToken cancellationToken)
            {
                this.m_source = source;
                this.m_reduceOperator = reduceOperator;
                this.m_partitionIndex = partitionIndex;
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            internal override bool MoveNext(ref TIntermediate currentElement, ref int currentKey)
            {
                if (!this.m_accumulated)
                {
                    this.m_accumulated = true;
                    bool flag = false;
                    TIntermediate local = default(TIntermediate);
                    if (this.m_reduceOperator.m_seedIsSpecified)
                    {
                        local = (this.m_reduceOperator.m_seedFactory == null) ? this.m_reduceOperator.m_seed : this.m_reduceOperator.m_seedFactory();
                    }
                    else
                    {
                        TInput local2 = default(TInput);
                        TKey local3 = default(TKey);
                        if (!this.m_source.MoveNext(ref local2, ref local3))
                        {
                            return false;
                        }
                        flag = true;
                        local = (TIntermediate) local2;
                    }
                    TInput local4 = default(TInput);
                    TKey local5 = default(TKey);
                    int num = 0;
                    while (this.m_source.MoveNext(ref local4, ref local5))
                    {
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                        }
                        flag = true;
                        local = this.m_reduceOperator.m_intermediateReduce(local, local4);
                    }
                    if (flag)
                    {
                        currentElement = local;
                        currentKey = this.m_partitionIndex;
                        return true;
                    }
                }
                return false;
            }
        }
    }
}

