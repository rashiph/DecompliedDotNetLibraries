namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.ServiceModel.Diagnostics;

    internal class SynchronizationManager
    {
        private object mutex = new object();
        private Queue<SynchronizationEvent> queue = new Queue<SynchronizationEvent>();
        private StateMachine stateMachine;

        public SynchronizationManager(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public void Enqueue(SynchronizationEvent newEvent)
        {
            bool flag;
            if (newEvent == null)
            {
                DiagnosticUtility.FailFast("The synchronization manager cannot enqueue a null event");
            }
            if (DiagnosticUtility.ShouldUseActivity)
            {
                DiagnosticUtility.DiagnosticTrace.TraceTransfer(this.stateMachine.Enlistment.EnlistmentId);
            }
            lock (this.mutex)
            {
                this.queue.Enqueue(newEvent);
                flag = this.queue.Count == 1;
            }
            if (flag)
            {
                this.Execute();
            }
        }

        private void Execute()
        {
            if (DebugTrace.Verbose)
            {
                DebugTrace.TxTrace(TraceLevel.Verbose, this.stateMachine.Enlistment.EnlistmentId, "Now processing events on {0}", this.stateMachine.GetType().Name);
            }
            while (true)
            {
                SynchronizationEvent e = this.queue.Peek();
                if (e == null)
                {
                    DiagnosticUtility.FailFast("Peek returned null synchronization event");
                }
                try
                {
                    if (DebugTrace.Verbose)
                    {
                        DebugTrace.TxTrace(TraceLevel.Verbose, this.stateMachine.Enlistment.EnlistmentId, "Dispatching {0} event to {1} in {2}", e.GetType().Name, this.stateMachine.GetType().Name, this.stateMachine.State.GetType().Name);
                    }
                    using (Activity.CreateActivity(this.stateMachine.Enlistment.EnlistmentId))
                    {
                        this.stateMachine.Dispatch(e);
                    }
                    if (DebugTrace.Verbose)
                    {
                        DebugTrace.TxTrace(TraceLevel.Verbose, this.stateMachine.Enlistment.EnlistmentId, "Dispatched {0} event to {1} in {2}", e.GetType().Name, this.stateMachine.GetType().Name, this.stateMachine.State.GetType().Name);
                    }
                }
                catch (Exception exception)
                {
                    DebugTrace.TxTrace(TraceLevel.Error, this.stateMachine.Enlistment.EnlistmentId, "REALLY BAD ERROR: Unhandled exception caught by synchronization manager: {0}", exception);
                    UnhandledStateMachineExceptionRecord.TraceAndLog(this.stateMachine.Enlistment.EnlistmentId, (this.stateMachine.Enlistment.Enlistment != null) ? this.stateMachine.Enlistment.Enlistment.RemoteTransactionId : string.Empty, this.stateMachine.ToString(), this.stateMachine.State.ToString(), this.stateMachine.History, exception);
                    DiagnosticUtility.FailFast(string.Format(CultureInfo.InvariantCulture, "Failfasting due to unhandled exception: {0}\r\n\r\n{1}", new object[] { exception.Message, exception }));
                }
                lock (this.mutex)
                {
                    this.queue.Dequeue();
                    if (this.queue.Count == 0)
                    {
                        break;
                    }
                }
                if (DebugTrace.Verbose)
                {
                    DebugTrace.TxTrace(TraceLevel.Verbose, this.stateMachine.Enlistment.EnlistmentId, "Continuing to process events on {0}", this.stateMachine.GetType().Name);
                }
            }
            if (DebugTrace.Verbose)
            {
                DebugTrace.TxTrace(TraceLevel.Verbose, this.stateMachine.Enlistment.EnlistmentId, "Stopped processing events on {0}", this.stateMachine.GetType().Name);
            }
        }
    }
}

