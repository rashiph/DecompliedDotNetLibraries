namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class ElementAtQueryOperator<TSource> : UnaryQueryOperator<TSource, TSource>
    {
        private readonly int m_index;
        private bool m_prematureMerge;

        internal ElementAtQueryOperator(IEnumerable<TSource> child, int index) : base(child)
        {
            this.m_index = index;
            if (base.Child.OrdinalIndexState.IsWorseThan(OrdinalIndexState.Correct))
            {
                this.m_prematureMerge = true;
            }
        }

        internal bool Aggregate(out TSource result, bool withDefaultValue)
        {
            if (this.LimitsParallelism && (((ParallelExecutionMode) base.SpecifiedQuerySettings.WithDefaults().ExecutionMode.Value) != ParallelExecutionMode.ForceParallelism))
            {
                CancellationState cancellationState = base.SpecifiedQuerySettings.CancellationState;
                if (withDefaultValue)
                {
                    IEnumerable<TSource> source = CancellableEnumerable.Wrap<TSource>(base.Child.AsSequentialQuery(cancellationState.ExternalCancellationToken), cancellationState.ExternalCancellationToken);
                    result = ExceptionAggregator.WrapEnumerable<TSource>(source, cancellationState).ElementAtOrDefault<TSource>(this.m_index);
                }
                else
                {
                    IEnumerable<TSource> enumerable4 = CancellableEnumerable.Wrap<TSource>(base.Child.AsSequentialQuery(cancellationState.ExternalCancellationToken), cancellationState.ExternalCancellationToken);
                    result = ExceptionAggregator.WrapEnumerable<TSource>(enumerable4, cancellationState).ElementAt<TSource>(this.m_index);
                }
                return true;
            }
            using (IEnumerator<TSource> enumerator = base.GetEnumerator(3))
            {
                if (enumerator.MoveNext())
                {
                    TSource current = enumerator.Current;
                    result = current;
                    return true;
                }
            }
            result = default(TSource);
            return false;
        }

        internal override IEnumerable<TSource> AsSequentialQuery(CancellationToken token)
        {
            throw new NotSupportedException();
        }

        internal override QueryResults<TSource> Open(QuerySettings settings, bool preferStriping)
        {
            return new UnaryQueryOperator<TSource, TSource>.UnaryQueryOperatorResults(base.Child.Open(settings, false), this, settings, preferStriping);
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TSource, TKey> inputStream, IPartitionedStreamRecipient<TSource> recipient, bool preferStriping, QuerySettings settings)
        {
            PartitionedStream<TSource, int> stream;
            int partitionCount = inputStream.PartitionCount;
            if (this.m_prematureMerge)
            {
                stream = QueryOperator<TSource>.ExecuteAndCollectResults<TKey>(inputStream, partitionCount, base.Child.OutputOrdered, preferStriping, settings).GetPartitionedStream();
            }
            else
            {
                stream = (PartitionedStream<TSource, int>) inputStream;
            }
            Shared<bool> resultFoundFlag = new Shared<bool>(false);
            PartitionedStream<TSource, int> partitionedStream = new PartitionedStream<TSource, int>(partitionCount, Util.GetDefaultComparer<int>(), OrdinalIndexState.Correct);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new ElementAtQueryOperatorEnumerator<TSource>(stream[i], this.m_index, resultFoundFlag, settings.CancellationState.MergedCancellationToken);
            }
            recipient.Receive<int>(partitionedStream);
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return this.m_prematureMerge;
            }
        }

        private class ElementAtQueryOperatorEnumerator : QueryOperatorEnumerator<TSource, int>
        {
            private CancellationToken m_cancellationToken;
            private int m_index;
            private Shared<bool> m_resultFoundFlag;
            private QueryOperatorEnumerator<TSource, int> m_source;

            internal ElementAtQueryOperatorEnumerator(QueryOperatorEnumerator<TSource, int> source, int index, Shared<bool> resultFoundFlag, CancellationToken cancellationToken)
            {
                this.m_source = source;
                this.m_index = index;
                this.m_resultFoundFlag = resultFoundFlag;
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            internal override bool MoveNext(ref TSource currentElement, ref int currentKey)
            {
                int num = 0;
                while (this.m_source.MoveNext(ref currentElement, ref currentKey))
                {
                    if ((num++ & 0x3f) == 0)
                    {
                        CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                    }
                    if (this.m_resultFoundFlag.Value)
                    {
                        break;
                    }
                    if (currentKey == this.m_index)
                    {
                        this.m_resultFoundFlag.Value = true;
                        return true;
                    }
                }
                return false;
            }
        }
    }
}

