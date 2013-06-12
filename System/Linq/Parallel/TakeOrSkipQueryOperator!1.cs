namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class TakeOrSkipQueryOperator<TResult> : UnaryQueryOperator<TResult, TResult>
    {
        private readonly int m_count;
        private bool m_prematureMerge;
        private readonly bool m_take;

        internal TakeOrSkipQueryOperator(IEnumerable<TResult> child, int count, bool take) : base(child)
        {
            this.m_count = count;
            this.m_take = take;
            base.SetOrdinalIndexState(this.OutputOrdinalIndexState());
        }

        internal override IEnumerable<TResult> AsSequentialQuery(CancellationToken token)
        {
            if (this.m_take)
            {
                return base.Child.AsSequentialQuery(token).Take<TResult>(this.m_count);
            }
            return CancellableEnumerable.Wrap<TResult>(base.Child.AsSequentialQuery(token), token).Skip<TResult>(this.m_count);
        }

        internal override QueryResults<TResult> Open(QuerySettings settings, bool preferStriping)
        {
            return TakeOrSkipQueryOperatorResults<TResult>.NewResults(base.Child.Open(settings, true), (TakeOrSkipQueryOperator<TResult>) this, settings, preferStriping);
        }

        private OrdinalIndexState OutputOrdinalIndexState()
        {
            OrdinalIndexState ordinalIndexState = base.Child.OrdinalIndexState;
            if (ordinalIndexState == OrdinalIndexState.Indexible)
            {
                return OrdinalIndexState.Indexible;
            }
            if (ordinalIndexState.IsWorseThan(OrdinalIndexState.Increasing))
            {
                this.m_prematureMerge = true;
                ordinalIndexState = OrdinalIndexState.Correct;
            }
            if (!this.m_take && (ordinalIndexState == OrdinalIndexState.Correct))
            {
                ordinalIndexState = OrdinalIndexState.Increasing;
            }
            return ordinalIndexState;
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TResult, TKey> inputStream, IPartitionedStreamRecipient<TResult> recipient, bool preferStriping, QuerySettings settings)
        {
            PartitionedStream<TResult, int> stream;
            if (this.m_prematureMerge)
            {
                stream = QueryOperator<TResult>.ExecuteAndCollectResults<TKey>(inputStream, inputStream.PartitionCount, base.Child.OutputOrdered, preferStriping, settings).GetPartitionedStream();
            }
            else
            {
                stream = (PartitionedStream<TResult, int>) inputStream;
            }
            int partitionCount = inputStream.PartitionCount;
            FixedMaxHeap<int> sharedIndices = new FixedMaxHeap<int>(this.m_count);
            CountdownEvent sharedBarrier = new CountdownEvent(partitionCount);
            PartitionedStream<TResult, int> partitionedStream = new PartitionedStream<TResult, int>(partitionCount, Util.GetDefaultComparer<int>(), this.OrdinalIndexState);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new TakeOrSkipQueryOperatorEnumerator<TResult>(stream[i], this.m_count, this.m_take, sharedIndices, sharedBarrier, settings.CancellationState.MergedCancellationToken);
            }
            recipient.Receive<int>(partitionedStream);
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return (this.OrdinalIndexState != OrdinalIndexState.Indexible);
            }
        }

        private class TakeOrSkipQueryOperatorEnumerator : QueryOperatorEnumerator<TResult, int>
        {
            private List<Pair<TResult, int>> m_buffer;
            private Shared<int> m_bufferIndex;
            private readonly CancellationToken m_cancellationToken;
            private readonly int m_count;
            private readonly CountdownEvent m_sharedBarrier;
            private readonly FixedMaxHeap<int> m_sharedIndices;
            private readonly QueryOperatorEnumerator<TResult, int> m_source;
            private readonly bool m_take;

            internal TakeOrSkipQueryOperatorEnumerator(QueryOperatorEnumerator<TResult, int> source, int count, bool take, FixedMaxHeap<int> sharedIndices, CountdownEvent sharedBarrier, CancellationToken cancellationToken)
            {
                this.m_source = source;
                this.m_count = count;
                this.m_take = take;
                this.m_sharedIndices = sharedIndices;
                this.m_sharedBarrier = sharedBarrier;
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            internal override bool MoveNext(ref TResult currentElement, ref int currentKey)
            {
                if ((this.m_buffer == null) && (this.m_count > 0))
                {
                    List<Pair<TResult, int>> list = new List<Pair<TResult, int>>();
                    TResult local = default(TResult);
                    int num = 0;
                    int num2 = 0;
                    while ((list.Count < this.m_count) && this.m_source.MoveNext(ref local, ref num))
                    {
                        if ((num2++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                        }
                        list.Add(new Pair<TResult, int>(local, num));
                        lock (this.m_sharedIndices)
                        {
                            if (!this.m_sharedIndices.Insert(num))
                            {
                                break;
                            }
                            continue;
                        }
                    }
                    this.m_sharedBarrier.Signal();
                    this.m_sharedBarrier.Wait(this.m_cancellationToken);
                    this.m_buffer = list;
                    this.m_bufferIndex = new Shared<int>(-1);
                }
                if (this.m_take)
                {
                    if ((this.m_count == 0) || (this.m_bufferIndex.Value >= (this.m_buffer.Count - 1)))
                    {
                        return false;
                    }
                    this.m_bufferIndex.Value += 1;
                    Pair<TResult, int> pair = this.m_buffer[this.m_bufferIndex.Value];
                    currentElement = pair.First;
                    Pair<TResult, int> pair2 = this.m_buffer[this.m_bufferIndex.Value];
                    currentKey = pair2.Second;
                    int maxValue = this.m_sharedIndices.MaxValue;
                    if (maxValue != -1)
                    {
                        Pair<TResult, int> pair3 = this.m_buffer[this.m_bufferIndex.Value];
                        return (pair3.Second <= maxValue);
                    }
                    return true;
                }
                int num4 = -1;
                if (this.m_count > 0)
                {
                    if (this.m_sharedIndices.Count < this.m_count)
                    {
                        return false;
                    }
                    num4 = this.m_sharedIndices.MaxValue;
                    if (this.m_bufferIndex.Value < (this.m_buffer.Count - 1))
                    {
                        this.m_bufferIndex.Value += 1;
                        while (this.m_bufferIndex.Value < this.m_buffer.Count)
                        {
                            Pair<TResult, int> pair4 = this.m_buffer[this.m_bufferIndex.Value];
                            if (pair4.Second > num4)
                            {
                                Pair<TResult, int> pair5 = this.m_buffer[this.m_bufferIndex.Value];
                                currentElement = pair5.First;
                                Pair<TResult, int> pair6 = this.m_buffer[this.m_bufferIndex.Value];
                                currentKey = pair6.Second;
                                return true;
                            }
                            this.m_bufferIndex.Value += 1;
                        }
                    }
                }
                return this.m_source.MoveNext(ref currentElement, ref currentKey);
            }
        }

        private class TakeOrSkipQueryOperatorResults : UnaryQueryOperator<TResult, TResult>.UnaryQueryOperatorResults
        {
            private int m_childCount;
            private TakeOrSkipQueryOperator<TResult> m_takeOrSkipOp;

            private TakeOrSkipQueryOperatorResults(QueryResults<TResult> childQueryResults, TakeOrSkipQueryOperator<TResult> takeOrSkipOp, QuerySettings settings, bool preferStriping) : base(childQueryResults, takeOrSkipOp, settings, preferStriping)
            {
                this.m_takeOrSkipOp = takeOrSkipOp;
                this.m_childCount = base.m_childQueryResults.ElementsCount;
            }

            internal override TResult GetElement(int index)
            {
                if (this.m_takeOrSkipOp.m_take)
                {
                    return base.m_childQueryResults.GetElement(index);
                }
                return base.m_childQueryResults.GetElement(this.m_takeOrSkipOp.m_count + index);
            }

            public static QueryResults<TResult> NewResults(QueryResults<TResult> childQueryResults, TakeOrSkipQueryOperator<TResult> op, QuerySettings settings, bool preferStriping)
            {
                if (childQueryResults.IsIndexible)
                {
                    return new TakeOrSkipQueryOperator<TResult>.TakeOrSkipQueryOperatorResults(childQueryResults, op, settings, preferStriping);
                }
                return new UnaryQueryOperator<TResult, TResult>.UnaryQueryOperatorResults(childQueryResults, op, settings, preferStriping);
            }

            internal override int ElementsCount
            {
                get
                {
                    if (this.m_takeOrSkipOp.m_take)
                    {
                        return Math.Min(this.m_childCount, this.m_takeOrSkipOp.m_count);
                    }
                    return Math.Max(this.m_childCount - this.m_takeOrSkipOp.m_count, 0);
                }
            }

            internal override bool IsIndexible
            {
                get
                {
                    return (this.m_childCount >= 0);
                }
            }
        }
    }
}

