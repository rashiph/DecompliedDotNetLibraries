namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class TakeOrSkipWhileQueryOperator<TResult> : UnaryQueryOperator<TResult, TResult>
    {
        private Func<TResult, int, bool> m_indexedPredicate;
        private Func<TResult, bool> m_predicate;
        private bool m_prematureMerge;
        private readonly bool m_take;

        internal TakeOrSkipWhileQueryOperator(IEnumerable<TResult> child, Func<TResult, bool> predicate, Func<TResult, int, bool> indexedPredicate, bool take) : base(child)
        {
            this.m_predicate = predicate;
            this.m_indexedPredicate = indexedPredicate;
            this.m_take = take;
            base.SetOrdinalIndexState(this.OutputOrderIndexState());
        }

        internal override IEnumerable<TResult> AsSequentialQuery(CancellationToken token)
        {
            if (this.m_take)
            {
                if (this.m_indexedPredicate != null)
                {
                    return base.Child.AsSequentialQuery(token).TakeWhile<TResult>(this.m_indexedPredicate);
                }
                return base.Child.AsSequentialQuery(token).TakeWhile<TResult>(this.m_predicate);
            }
            if (this.m_indexedPredicate != null)
            {
                return CancellableEnumerable.Wrap<TResult>(base.Child.AsSequentialQuery(token), token).SkipWhile<TResult>(this.m_indexedPredicate);
            }
            return CancellableEnumerable.Wrap<TResult>(base.Child.AsSequentialQuery(token), token).SkipWhile<TResult>(this.m_predicate);
        }

        internal override QueryResults<TResult> Open(QuerySettings settings, bool preferStriping)
        {
            return new UnaryQueryOperator<TResult, TResult>.UnaryQueryOperatorResults(base.Child.Open(settings, true), this, settings, preferStriping);
        }

        private OrdinalIndexState OutputOrderIndexState()
        {
            OrdinalIndexState increasing = OrdinalIndexState.Increasing;
            if (this.m_indexedPredicate != null)
            {
                increasing = OrdinalIndexState.Correct;
            }
            OrdinalIndexState state2 = base.Child.OrdinalIndexState.Worse(OrdinalIndexState.Correct);
            if (state2.IsWorseThan(increasing))
            {
                this.m_prematureMerge = true;
            }
            if (!this.m_take)
            {
                state2 = state2.Worse(OrdinalIndexState.Increasing);
            }
            return state2;
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TResult, TKey> inputStream, IPartitionedStreamRecipient<TResult> recipient, bool preferStriping, QuerySettings settings)
        {
            PartitionedStream<TResult, int> stream;
            int partitionCount = inputStream.PartitionCount;
            if (this.m_prematureMerge)
            {
                stream = QueryOperator<TResult>.ExecuteAndCollectResults<TKey>(inputStream, partitionCount, base.Child.OutputOrdered, preferStriping, settings).GetPartitionedStream();
            }
            else
            {
                stream = (PartitionedStream<TResult, int>) inputStream;
            }
            Shared<int> sharedLowFalse = new Shared<int>(-1);
            CountdownEvent sharedBarrier = new CountdownEvent(partitionCount);
            PartitionedStream<TResult, int> partitionedStream = new PartitionedStream<TResult, int>(partitionCount, Util.GetDefaultComparer<int>(), this.OrdinalIndexState);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new TakeOrSkipWhileQueryOperatorEnumerator<TResult>(stream[i], this.m_predicate, this.m_indexedPredicate, this.m_take, sharedLowFalse, sharedBarrier, settings.CancellationState.MergedCancellationToken);
            }
            recipient.Receive<int>(partitionedStream);
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return true;
            }
        }

        private class TakeOrSkipWhileQueryOperatorEnumerator : QueryOperatorEnumerator<TResult, int>
        {
            private List<Pair<TResult, int>> m_buffer;
            private Shared<int> m_bufferIndex;
            private readonly CancellationToken m_cancellationToken;
            private readonly Func<TResult, int, bool> m_indexedPredicate;
            private readonly Func<TResult, bool> m_predicate;
            private readonly CountdownEvent m_sharedBarrier;
            private readonly Shared<int> m_sharedLowFalse;
            private readonly QueryOperatorEnumerator<TResult, int> m_source;
            private readonly bool m_take;

            internal TakeOrSkipWhileQueryOperatorEnumerator(QueryOperatorEnumerator<TResult, int> source, Func<TResult, bool> predicate, Func<TResult, int, bool> indexedPredicate, bool take, Shared<int> sharedLowFalse, CountdownEvent sharedBarrier, CancellationToken cancelToken)
            {
                this.m_source = source;
                this.m_predicate = predicate;
                this.m_indexedPredicate = indexedPredicate;
                this.m_take = take;
                this.m_sharedLowFalse = sharedLowFalse;
                this.m_sharedBarrier = sharedBarrier;
                this.m_cancellationToken = cancelToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            internal override bool MoveNext(ref TResult currentElement, ref int currentKey)
            {
                if (this.m_buffer != null)
                {
                    goto Label_0114;
                }
                List<Pair<TResult, int>> list = new List<Pair<TResult, int>>();
                try
                {
                    TResult local = default(TResult);
                    int num = 0;
                    int num2 = 0;
                    while (this.m_source.MoveNext(ref local, ref num))
                    {
                        bool flag;
                        if ((num2++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                        }
                        list.Add(new Pair<TResult, int>(local, num));
                        int num3 = this.m_sharedLowFalse.Value;
                        if ((num3 != -1) && (num > num3))
                        {
                            goto Label_00F0;
                        }
                        if (this.m_predicate != null)
                        {
                            flag = this.m_predicate(local);
                        }
                        else
                        {
                            flag = this.m_indexedPredicate(local, num);
                        }
                        if (!flag)
                        {
                            SpinWait wait = new SpinWait();
                            while (true)
                            {
                                int comparand = Thread.VolatileRead(ref this.m_sharedLowFalse.Value);
                                if (((comparand != -1) && (comparand < num)) || (Interlocked.CompareExchange(ref this.m_sharedLowFalse.Value, num, comparand) == comparand))
                                {
                                    goto Label_00F0;
                                }
                                wait.SpinOnce();
                            }
                        }
                    }
                }
                finally
                {
                    this.m_sharedBarrier.Signal();
                }
            Label_00F0:
                this.m_sharedBarrier.Wait(this.m_cancellationToken);
                this.m_buffer = list;
                this.m_bufferIndex = new Shared<int>(-1);
            Label_0114:
                if (this.m_take)
                {
                    if (this.m_bufferIndex.Value >= (this.m_buffer.Count - 1))
                    {
                        return false;
                    }
                    this.m_bufferIndex.Value += 1;
                    Pair<TResult, int> pair = this.m_buffer[this.m_bufferIndex.Value];
                    currentElement = pair.First;
                    Pair<TResult, int> pair2 = this.m_buffer[this.m_bufferIndex.Value];
                    currentKey = pair2.Second;
                    if (this.m_sharedLowFalse.Value != -1)
                    {
                        Pair<TResult, int> pair3 = this.m_buffer[this.m_bufferIndex.Value];
                        return (this.m_sharedLowFalse.Value > pair3.Second);
                    }
                    return true;
                }
                if (this.m_sharedLowFalse.Value == -1)
                {
                    return false;
                }
                if (this.m_bufferIndex.Value < (this.m_buffer.Count - 1))
                {
                    this.m_bufferIndex.Value += 1;
                    while (this.m_bufferIndex.Value < this.m_buffer.Count)
                    {
                        Pair<TResult, int> pair4 = this.m_buffer[this.m_bufferIndex.Value];
                        if (pair4.Second >= this.m_sharedLowFalse.Value)
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
                return this.m_source.MoveNext(ref currentElement, ref currentKey);
            }
        }
    }
}

