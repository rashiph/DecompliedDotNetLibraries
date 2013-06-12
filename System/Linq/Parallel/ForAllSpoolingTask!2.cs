namespace System.Linq.Parallel
{
    using System;

    internal class ForAllSpoolingTask<TInputOutput, TIgnoreKey> : SpoolingTaskBase
    {
        private QueryOperatorEnumerator<TInputOutput, TIgnoreKey> m_source;

        internal ForAllSpoolingTask(int taskIndex, QueryTaskGroupState groupState, QueryOperatorEnumerator<TInputOutput, TIgnoreKey> source) : base(taskIndex, groupState)
        {
            this.m_source = source;
        }

        protected override void SpoolingFinally()
        {
            base.SpoolingFinally();
            this.m_source.Dispose();
        }

        protected override void SpoolingWork()
        {
            TInputOutput currentElement = default(TInputOutput);
            TIgnoreKey currentKey = default(TIgnoreKey);
            while (this.m_source.MoveNext(ref currentElement, ref currentKey))
            {
            }
        }
    }
}

