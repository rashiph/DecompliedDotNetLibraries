namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class ZipQueryOperator<TLeftInput, TRightInput, TOutput> : QueryOperator<TOutput>
    {
        private readonly QueryOperator<TLeftInput> m_leftChild;
        private readonly bool m_prematureMergeLeft;
        private readonly bool m_prematureMergeRight;
        private readonly Func<TLeftInput, TRightInput, TOutput> m_resultSelector;
        private readonly QueryOperator<TRightInput> m_rightChild;

        private ZipQueryOperator(QueryOperator<TLeftInput> left, QueryOperator<TRightInput> right, Func<TLeftInput, TRightInput, TOutput> resultSelector) : base(left.SpecifiedQuerySettings.Merge(right.SpecifiedQuerySettings))
        {
            this.m_leftChild = left;
            this.m_rightChild = right;
            this.m_resultSelector = resultSelector;
            base.m_outputOrdered = this.m_leftChild.OutputOrdered || this.m_rightChild.OutputOrdered;
            this.m_prematureMergeLeft = this.m_leftChild.OrdinalIndexState != System.Linq.Parallel.OrdinalIndexState.Indexible;
            this.m_prematureMergeRight = this.m_rightChild.OrdinalIndexState != System.Linq.Parallel.OrdinalIndexState.Indexible;
        }

        internal ZipQueryOperator(ParallelQuery<TLeftInput> leftChildSource, IEnumerable<TRightInput> rightChildSource, Func<TLeftInput, TRightInput, TOutput> resultSelector) : this(QueryOperator<TLeftInput>.AsQueryOperator(leftChildSource), QueryOperator<TRightInput>.AsQueryOperator(rightChildSource), resultSelector)
        {
        }

        internal override IEnumerable<TOutput> AsSequentialQuery(CancellationToken token)
        {
            using (IEnumerator<TLeftInput> iteratorVariable0 = this.m_leftChild.AsSequentialQuery(token).GetEnumerator())
            {
                using (IEnumerator<TRightInput> iteratorVariable1 = this.m_rightChild.AsSequentialQuery(token).GetEnumerator())
                {
                    while (iteratorVariable0.MoveNext() && iteratorVariable1.MoveNext())
                    {
                        yield return this.m_resultSelector(iteratorVariable0.Current, iteratorVariable1.Current);
                    }
                }
            }
        }

        internal override QueryResults<TOutput> Open(QuerySettings settings, bool preferStriping)
        {
            QueryResults<TLeftInput> leftChildResults = this.m_leftChild.Open(settings, preferStriping);
            QueryResults<TRightInput> rightChildResults = this.m_rightChild.Open(settings, preferStriping);
            int partitionCount = settings.DegreeOfParallelism.Value;
            if (this.m_prematureMergeLeft)
            {
                PartitionedStreamMerger<TLeftInput> recipient = new PartitionedStreamMerger<TLeftInput>(false, ParallelMergeOptions.FullyBuffered, settings.TaskScheduler, this.m_leftChild.OutputOrdered, settings.CancellationState, settings.QueryId);
                leftChildResults.GivePartitionedStream(recipient);
                leftChildResults = new ListQueryResults<TLeftInput>(recipient.MergeExecutor.GetResultsAsArray(), partitionCount, preferStriping);
            }
            if (this.m_prematureMergeRight)
            {
                PartitionedStreamMerger<TRightInput> merger2 = new PartitionedStreamMerger<TRightInput>(false, ParallelMergeOptions.FullyBuffered, settings.TaskScheduler, this.m_rightChild.OutputOrdered, settings.CancellationState, settings.QueryId);
                rightChildResults.GivePartitionedStream(merger2);
                rightChildResults = new ListQueryResults<TRightInput>(merger2.MergeExecutor.GetResultsAsArray(), partitionCount, preferStriping);
            }
            return new ZipQueryOperatorResults<TLeftInput, TRightInput, TOutput>(leftChildResults, rightChildResults, this.m_resultSelector, partitionCount, preferStriping);
        }

        internal override bool LimitsParallelism
        {
            get
            {
                if (!this.m_prematureMergeLeft)
                {
                    return this.m_prematureMergeRight;
                }
                return true;
            }
        }

        internal override System.Linq.Parallel.OrdinalIndexState OrdinalIndexState
        {
            get
            {
                return System.Linq.Parallel.OrdinalIndexState.Indexible;
            }
        }

        [CompilerGenerated]
        private sealed class <AsSequentialQuery>d__0 : IEnumerable<TOutput>, IEnumerable, IEnumerator<TOutput>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TOutput <>2__current;
            public CancellationToken <>3__token;
            public ZipQueryOperator<TLeftInput, TRightInput, TOutput> <>4__this;
            private int <>l__initialThreadId;
            public IEnumerator<TLeftInput> <leftEnumerator>5__1;
            public IEnumerator<TRightInput> <rightEnumerator>5__2;
            public CancellationToken token;

            [DebuggerHidden]
            public <AsSequentialQuery>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally3()
            {
                this.<>1__state = -1;
                if (this.<leftEnumerator>5__1 != null)
                {
                    this.<leftEnumerator>5__1.Dispose();
                }
            }

            private void <>m__Finally4()
            {
                this.<>1__state = 1;
                if (this.<rightEnumerator>5__2 != null)
                {
                    this.<rightEnumerator>5__2.Dispose();
                }
            }

            private bool MoveNext()
            {
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<leftEnumerator>5__1 = this.<>4__this.m_leftChild.AsSequentialQuery(this.token).GetEnumerator();
                            this.<>1__state = 1;
                            this.<rightEnumerator>5__2 = this.<>4__this.m_rightChild.AsSequentialQuery(this.token).GetEnumerator();
                            this.<>1__state = 2;
                            while (this.<leftEnumerator>5__1.MoveNext() && this.<rightEnumerator>5__2.MoveNext())
                            {
                                this.<>2__current = this.<>4__this.m_resultSelector(this.<leftEnumerator>5__1.Current, this.<rightEnumerator>5__2.Current);
                                this.<>1__state = 3;
                                return true;
                            Label_00A7:
                                this.<>1__state = 2;
                            }
                            this.<>m__Finally4();
                            this.<>m__Finally3();
                            break;

                        case 3:
                            goto Label_00A7;
                    }
                    return false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<TOutput> IEnumerable<TOutput>.GetEnumerator()
            {
                ZipQueryOperator<TLeftInput, TRightInput, TOutput>.<AsSequentialQuery>d__0 d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (ZipQueryOperator<TLeftInput, TRightInput, TOutput>.<AsSequentialQuery>d__0) this;
                }
                else
                {
                    d__ = new ZipQueryOperator<TLeftInput, TRightInput, TOutput>.<AsSequentialQuery>d__0(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.token = this.<>3__token;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TOutput>.GetEnumerator();
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
                    case 3:
                        try
                        {
                            switch (this.<>1__state)
                            {
                                case 2:
                                case 3:
                                    try
                                    {
                                    }
                                    finally
                                    {
                                        this.<>m__Finally4();
                                    }
                                    return;
                            }
                        }
                        finally
                        {
                            this.<>m__Finally3();
                        }
                        break;

                    default:
                        return;
                }
            }

            TOutput IEnumerator<TOutput>.Current
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

        internal class ZipQueryOperatorResults : QueryResults<TOutput>
        {
            private readonly int m_count;
            private readonly QueryResults<TLeftInput> m_leftChildResults;
            private readonly int m_partitionCount;
            private readonly bool m_preferStriping;
            private readonly Func<TLeftInput, TRightInput, TOutput> m_resultSelector;
            private readonly QueryResults<TRightInput> m_rightChildResults;

            internal ZipQueryOperatorResults(QueryResults<TLeftInput> leftChildResults, QueryResults<TRightInput> rightChildResults, Func<TLeftInput, TRightInput, TOutput> resultSelector, int partitionCount, bool preferStriping)
            {
                this.m_leftChildResults = leftChildResults;
                this.m_rightChildResults = rightChildResults;
                this.m_resultSelector = resultSelector;
                this.m_partitionCount = partitionCount;
                this.m_preferStriping = preferStriping;
                this.m_count = Math.Min(this.m_leftChildResults.Count, this.m_rightChildResults.Count);
            }

            internal override TOutput GetElement(int index)
            {
                return this.m_resultSelector(this.m_leftChildResults.GetElement(index), this.m_rightChildResults.GetElement(index));
            }

            internal override void GivePartitionedStream(IPartitionedStreamRecipient<TOutput> recipient)
            {
                PartitionedStream<TOutput, int> partitionedStream = ExchangeUtilities.PartitionDataSource<TOutput>(this, this.m_partitionCount, this.m_preferStriping);
                recipient.Receive<int>(partitionedStream);
            }

            internal override int ElementsCount
            {
                get
                {
                    return this.m_count;
                }
            }

            internal override bool IsIndexible
            {
                get
                {
                    return true;
                }
            }
        }
    }
}

