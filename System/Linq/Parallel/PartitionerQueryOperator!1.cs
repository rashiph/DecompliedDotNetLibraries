namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class PartitionerQueryOperator<TElement> : QueryOperator<TElement>
    {
        private Partitioner<TElement> m_partitioner;

        internal PartitionerQueryOperator(Partitioner<TElement> partitioner) : base(false, QuerySettings.Empty)
        {
            this.m_partitioner = partitioner;
        }

        internal override IEnumerable<TElement> AsSequentialQuery(CancellationToken token)
        {
            using (IEnumerator<TElement> iteratorVariable0 = this.m_partitioner.GetPartitions(1)[0])
            {
                while (iteratorVariable0.MoveNext())
                {
                    yield return iteratorVariable0.Current;
                }
            }
        }

        internal static System.Linq.Parallel.OrdinalIndexState GetOrdinalIndexState(Partitioner<TElement> partitioner)
        {
            OrderablePartitioner<TElement> partitioner2 = partitioner as OrderablePartitioner<TElement>;
            if (partitioner2 == null)
            {
                return System.Linq.Parallel.OrdinalIndexState.Shuffled;
            }
            if (!partitioner2.KeysOrderedInEachPartition)
            {
                return System.Linq.Parallel.OrdinalIndexState.Shuffled;
            }
            if (partitioner2.KeysNormalized)
            {
                return System.Linq.Parallel.OrdinalIndexState.Correct;
            }
            return System.Linq.Parallel.OrdinalIndexState.Increasing;
        }

        internal override QueryResults<TElement> Open(QuerySettings settings, bool preferStriping)
        {
            return new PartitionerQueryOperatorResults<TElement>(this.m_partitioner, settings);
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return false;
            }
        }

        internal bool Orderable
        {
            get
            {
                return (this.m_partitioner is OrderablePartitioner<TElement>);
            }
        }

        internal override System.Linq.Parallel.OrdinalIndexState OrdinalIndexState
        {
            get
            {
                return PartitionerQueryOperator<TElement>.GetOrdinalIndexState(this.m_partitioner);
            }
        }

        [CompilerGenerated]
        private sealed class <AsSequentialQuery>d__0 : IEnumerable<TElement>, IEnumerable, IEnumerator<TElement>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TElement <>2__current;
            public PartitionerQueryOperator<TElement> <>4__this;
            private int <>l__initialThreadId;
            public IEnumerator<TElement> <enumerator>5__1;

            [DebuggerHidden]
            public <AsSequentialQuery>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally2()
            {
                this.<>1__state = -1;
                if (this.<enumerator>5__1 != null)
                {
                    this.<enumerator>5__1.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<enumerator>5__1 = this.<>4__this.m_partitioner.GetPartitions(1)[0];
                            this.<>1__state = 1;
                            goto Label_006B;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_006B;

                        default:
                            goto Label_007E;
                    }
                Label_0048:
                    this.<>2__current = this.<enumerator>5__1.Current;
                    this.<>1__state = 2;
                    return true;
                Label_006B:
                    if (this.<enumerator>5__1.MoveNext())
                    {
                        goto Label_0048;
                    }
                    this.<>m__Finally2();
                Label_007E:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
            {
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    return (PartitionerQueryOperator<TElement>.<AsSequentialQuery>d__0) this;
                }
                return new PartitionerQueryOperator<TElement>.<AsSequentialQuery>d__0(0) { <>4__this = this.<>4__this };
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TElement>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case 1:
                    case 2:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally2();
                        }
                        return;
                }
            }

            TElement IEnumerator<TElement>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }

        private class OrderablePartitionerEnumerator : QueryOperatorEnumerator<TElement, int>
        {
            private IEnumerator<KeyValuePair<long, TElement>> m_sourceEnumerator;

            internal OrderablePartitionerEnumerator(IEnumerator<KeyValuePair<long, TElement>> sourceEnumerator)
            {
                this.m_sourceEnumerator = sourceEnumerator;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_sourceEnumerator.Dispose();
            }

            internal override bool MoveNext(ref TElement currentElement, ref int currentKey)
            {
                if (!this.m_sourceEnumerator.MoveNext())
                {
                    return false;
                }
                KeyValuePair<long, TElement> current = this.m_sourceEnumerator.Current;
                currentElement = current.Value;
                currentKey = (int) current.Key;
                return true;
            }
        }

        private class PartitionerEnumerator : QueryOperatorEnumerator<TElement, int>
        {
            private IEnumerator<TElement> m_sourceEnumerator;

            internal PartitionerEnumerator(IEnumerator<TElement> sourceEnumerator)
            {
                this.m_sourceEnumerator = sourceEnumerator;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_sourceEnumerator.Dispose();
            }

            internal override bool MoveNext(ref TElement currentElement, ref int currentKey)
            {
                if (!this.m_sourceEnumerator.MoveNext())
                {
                    return false;
                }
                currentElement = this.m_sourceEnumerator.Current;
                currentKey = 0;
                return true;
            }
        }

        private class PartitionerQueryOperatorResults : QueryResults<TElement>
        {
            private Partitioner<TElement> m_partitioner;
            private QuerySettings m_settings;

            internal PartitionerQueryOperatorResults(Partitioner<TElement> partitioner, QuerySettings settings)
            {
                this.m_partitioner = partitioner;
                this.m_settings = settings;
            }

            internal override void GivePartitionedStream(IPartitionedStreamRecipient<TElement> recipient)
            {
                int partitionCount = this.m_settings.DegreeOfParallelism.Value;
                OrderablePartitioner<TElement> partitioner = this.m_partitioner as OrderablePartitioner<TElement>;
                OrdinalIndexState indexState = (partitioner != null) ? PartitionerQueryOperator<TElement>.GetOrdinalIndexState(partitioner) : OrdinalIndexState.Shuffled;
                PartitionedStream<TElement, int> partitionedStream = new PartitionedStream<TElement, int>(partitionCount, Util.GetDefaultComparer<int>(), indexState);
                if (partitioner != null)
                {
                    IList<IEnumerator<KeyValuePair<long, TElement>>> orderablePartitions = partitioner.GetOrderablePartitions(partitionCount);
                    if (orderablePartitions == null)
                    {
                        throw new InvalidOperationException(System.Linq.SR.GetString("PartitionerQueryOperator_NullPartitionList"));
                    }
                    if (orderablePartitions.Count != partitionCount)
                    {
                        throw new InvalidOperationException(System.Linq.SR.GetString("PartitionerQueryOperator_WrongNumberOfPartitions"));
                    }
                    for (int i = 0; i < partitionCount; i++)
                    {
                        IEnumerator<KeyValuePair<long, TElement>> sourceEnumerator = orderablePartitions[i];
                        if (sourceEnumerator == null)
                        {
                            throw new InvalidOperationException(System.Linq.SR.GetString("PartitionerQueryOperator_NullPartition"));
                        }
                        partitionedStream[i] = new PartitionerQueryOperator<TElement>.OrderablePartitionerEnumerator(sourceEnumerator);
                    }
                }
                else
                {
                    IList<IEnumerator<TElement>> partitions = this.m_partitioner.GetPartitions(partitionCount);
                    if (partitions == null)
                    {
                        throw new InvalidOperationException(System.Linq.SR.GetString("PartitionerQueryOperator_NullPartitionList"));
                    }
                    if (partitions.Count != partitionCount)
                    {
                        throw new InvalidOperationException(System.Linq.SR.GetString("PartitionerQueryOperator_WrongNumberOfPartitions"));
                    }
                    for (int j = 0; j < partitionCount; j++)
                    {
                        IEnumerator<TElement> enumerator2 = partitions[j];
                        if (enumerator2 == null)
                        {
                            throw new InvalidOperationException(System.Linq.SR.GetString("PartitionerQueryOperator_NullPartition"));
                        }
                        partitionedStream[j] = new PartitionerQueryOperator<TElement>.PartitionerEnumerator(enumerator2);
                    }
                }
                recipient.Receive<int>(partitionedStream);
            }
        }
    }
}

