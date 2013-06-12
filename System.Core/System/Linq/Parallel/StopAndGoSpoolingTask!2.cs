namespace System.Linq.Parallel
{
    using System;
    using System.Threading;

    internal class StopAndGoSpoolingTask<TInputOutput, TIgnoreKey> : SpoolingTaskBase
    {
        private SynchronousChannel<TInputOutput> m_destination;
        private QueryOperatorEnumerator<TInputOutput, TIgnoreKey> m_source;

        internal StopAndGoSpoolingTask(int taskIndex, QueryTaskGroupState groupState, QueryOperatorEnumerator<TInputOutput, TIgnoreKey> source, SynchronousChannel<TInputOutput> destination) : base(taskIndex, groupState)
        {
            this.m_source = source;
            this.m_destination = destination;
        }

        protected override void SpoolingFinally()
        {
            base.SpoolingFinally();
            if (this.m_destination != null)
            {
                this.m_destination.SetDone();
            }
            this.m_source.Dispose();
        }

        protected override void SpoolingWork()
        {
            TInputOutput currentElement = default(TInputOutput);
            TIgnoreKey currentKey = default(TIgnoreKey);
            QueryOperatorEnumerator<TInputOutput, TIgnoreKey> source = this.m_source;
            SynchronousChannel<TInputOutput> destination = this.m_destination;
            CancellationToken mergedCancellationToken = base.m_groupState.CancellationState.MergedCancellationToken;
            destination.Init();
            while (source.MoveNext(ref currentElement, ref currentKey))
            {
                if (mergedCancellationToken.IsCancellationRequested)
                {
                    return;
                }
                destination.Enqueue(currentElement);
            }
        }
    }
}

