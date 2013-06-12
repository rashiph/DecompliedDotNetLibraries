namespace System.Linq.Parallel
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class QueryTaskGroupState
    {
        private int m_alreadyEnded;
        private System.Linq.Parallel.CancellationState m_cancellationState;
        private int m_queryId;
        private Task m_rootTask;

        internal QueryTaskGroupState(System.Linq.Parallel.CancellationState cancellationState, int queryId)
        {
            this.m_cancellationState = cancellationState;
            this.m_queryId = queryId;
        }

        internal void QueryBegin(Task rootTask)
        {
            this.m_rootTask = rootTask;
        }

        internal void QueryEnd(bool userInitiatedDispose)
        {
            if (Interlocked.Exchange(ref this.m_alreadyEnded, 1) == 0)
            {
                try
                {
                    this.m_rootTask.Wait();
                }
                catch (AggregateException exception)
                {
                    AggregateException exception2 = exception.Flatten();
                    bool flag = true;
                    for (int i = 0; i < exception2.InnerExceptions.Count; i++)
                    {
                        OperationCanceledException exception3 = exception2.InnerExceptions[i] as OperationCanceledException;
                        if (((exception3 == null) || !exception3.CancellationToken.IsCancellationRequested) || (exception3.CancellationToken != this.m_cancellationState.ExternalCancellationToken))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        throw exception2;
                    }
                }
                finally
                {
                    this.m_rootTask.Dispose();
                }
                if (this.m_cancellationState.MergedCancellationToken.IsCancellationRequested)
                {
                    if (!this.m_cancellationState.TopLevelDisposedFlag.Value)
                    {
                        System.Linq.Parallel.CancellationState.ThrowWithStandardMessageIfCanceled(this.m_cancellationState.ExternalCancellationToken);
                    }
                    if (!userInitiatedDispose)
                    {
                        throw new ObjectDisposedException("enumerator", System.Linq.SR.GetString("PLINQ_DisposeRequested"));
                    }
                }
            }
        }

        internal System.Linq.Parallel.CancellationState CancellationState
        {
            get
            {
                return this.m_cancellationState;
            }
        }

        internal bool IsAlreadyEnded
        {
            get
            {
                return (this.m_alreadyEnded == 1);
            }
        }

        internal int QueryId
        {
            get
            {
                return this.m_queryId;
            }
        }
    }
}

