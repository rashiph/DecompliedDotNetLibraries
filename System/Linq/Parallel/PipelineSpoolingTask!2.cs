namespace System.Linq.Parallel
{
    using System;
    using System.Threading;

    internal class PipelineSpoolingTask<TInputOutput, TIgnoreKey> : SpoolingTaskBase
    {
        private AsynchronousChannel<TInputOutput> m_destination;
        private QueryOperatorEnumerator<TInputOutput, TIgnoreKey> m_source;

        internal PipelineSpoolingTask(int taskIndex, QueryTaskGroupState groupState, QueryOperatorEnumerator<TInputOutput, TIgnoreKey> source, AsynchronousChannel<TInputOutput> destination) : base(taskIndex, groupState)
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
            AsynchronousChannel<TInputOutput> destination = this.m_destination;
            CancellationToken mergedCancellationToken = base.m_groupState.CancellationState.MergedCancellationToken;
            while (source.MoveNext(ref currentElement, ref currentKey))
            {
                if (mergedCancellationToken.IsCancellationRequested)
                {
                    break;
                }
                destination.Enqueue(currentElement);
            }
            destination.FlushBuffers();
        }
    }
}

