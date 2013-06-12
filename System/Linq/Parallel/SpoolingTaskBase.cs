namespace System.Linq.Parallel
{
    using System;

    internal abstract class SpoolingTaskBase : QueryTask
    {
        protected SpoolingTaskBase(int taskIndex, QueryTaskGroupState groupState) : base(taskIndex, groupState)
        {
        }

        protected virtual void SpoolingFinally()
        {
        }

        protected abstract void SpoolingWork();
        protected override void Work()
        {
            try
            {
                this.SpoolingWork();
            }
            catch (Exception exception)
            {
                OperationCanceledException exception2 = exception as OperationCanceledException;
                if (((exception2 == null) || (exception2.CancellationToken != base.m_groupState.CancellationState.MergedCancellationToken)) || !base.m_groupState.CancellationState.MergedCancellationToken.IsCancellationRequested)
                {
                    base.m_groupState.CancellationState.InternalCancellationTokenSource.Cancel();
                    throw;
                }
            }
            finally
            {
                this.SpoolingFinally();
            }
        }
    }
}

