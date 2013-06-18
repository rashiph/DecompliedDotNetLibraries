namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal abstract class StateMachine : ITimerRecipient
    {
        private Microsoft.Transactions.Wsat.StateMachines.State current;
        protected TransactionEnlistment enlistment;
        private StateMachineHistory history;
        protected ProtocolState state;
        protected SynchronizationManager synchronization;
        private TimerState timer;
        private object timerLock = new object();

        protected StateMachine(TransactionEnlistment enlistment)
        {
            this.enlistment = enlistment;
            this.state = enlistment.State;
            this.synchronization = new SynchronizationManager(this);
            if (DebugTrace.Warning || DiagnosticUtility.ShouldTraceWarning)
            {
                this.history = new StateMachineHistory();
            }
        }

        public void CancelTimer()
        {
            lock (this.timerLock)
            {
                if (this.timer.Active)
                {
                    if (DebugTrace.Verbose)
                    {
                        DebugTrace.TxTrace(TraceLevel.Verbose, this.enlistment.EnlistmentId, "Removing active timer");
                    }
                    this.state.TimerManager.Remove(this);
                    this.timer.Instance = null;
                    this.timer.Active = false;
                }
            }
        }

        public void ChangeState(Microsoft.Transactions.Wsat.StateMachines.State newState)
        {
            if (this.history != null)
            {
                this.history.AddState(newState.ToString());
            }
            if (this.current != null)
            {
                if (DebugTrace.Info)
                {
                    DebugTrace.TxTrace(TraceLevel.Info, this.enlistment.EnlistmentId, "Leaving [{0}]", this.current);
                }
                this.current.Leave(this);
            }
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, this.enlistment.EnlistmentId, "Entering [{0}]", newState);
            }
            this.current = newState;
            this.current.Enter(this);
        }

        private bool ChooseProfile(TimerProfile profile)
        {
            bool flag = true;
            switch (profile)
            {
                case TimerProfile.Preparing:
                {
                    ParticipantEnlistment enlistment = (ParticipantEnlistment) this.enlistment;
                    TimeSpan span = enlistment.TimeoutEstimate - this.state.ElapsedTime;
                    if (span >= this.state.Config.PreparePolicy.InitialDelay)
                    {
                        this.timer.Instance = TimerInstance.Preparing;
                        this.timer.Policy = this.state.Config.PreparePolicy;
                        return flag;
                    }
                    return false;
                }
                case TimerProfile.Prepared:
                    this.timer.Instance = TimerInstance.Prepared;
                    this.timer.Policy = this.state.Config.PreparedPolicy;
                    return flag;

                case TimerProfile.Replaying:
                    this.timer.Instance = TimerInstance.Replaying;
                    this.timer.Policy = this.state.Config.ReplayPolicy;
                    return flag;

                case TimerProfile.Committing:
                    this.timer.Instance = TimerInstance.Committing;
                    this.timer.Policy = this.state.Config.CommitPolicy;
                    return flag;

                case TimerProfile.VolatileOutcomeAssurance:
                    this.timer.Instance = TimerInstance.VolatileOutcomeAssurance;
                    this.timer.Policy = this.state.Config.VolatileOutcomePolicy;
                    return flag;
            }
            DiagnosticUtility.FailFast("Invalid TimerProfile");
            return flag;
        }

        public virtual void Cleanup()
        {
            this.enlistment.OnStateMachineComplete();
        }

        public void Dispatch(SynchronizationEvent e)
        {
            if (this.history != null)
            {
                this.history.AddEvent(e.ToString());
            }
            e.Execute(this);
        }

        public void Enqueue(SynchronizationEvent e)
        {
            this.synchronization.Enqueue(e);
        }

        protected virtual void OnTimer(TimerProfile profile)
        {
        }

        public void OnTimerNotification(object token)
        {
            TimerInstance objA = (TimerInstance) token;
            lock (this.timerLock)
            {
                if (!this.timer.Active)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Notification discarded due to inactive timer");
                    return;
                }
                if (!object.ReferenceEquals(objA, this.timer.Instance))
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Notification discarded due to mismatched policy");
                    return;
                }
                this.timer.Notifications += 1L;
                if ((this.timer.Policy.MaxNotifications == 0) || (this.timer.Notifications < this.timer.Policy.MaxNotifications))
                {
                    if (this.timer.Notifications == 1L)
                    {
                        this.timer.CurrentInterval = this.timer.Policy.NotificationInterval;
                    }
                    else
                    {
                        int intervalIncreasePercentage = this.timer.Policy.IntervalIncreasePercentage;
                        long ticks = this.timer.CurrentInterval.Ticks;
                        ticks += (ticks / 100L) * intervalIncreasePercentage;
                        if (ticks < 0L)
                        {
                            ticks = 0x7ffffffffffffffeL;
                        }
                        TimeSpan maxNotificationInterval = new TimeSpan(ticks);
                        if (maxNotificationInterval > this.timer.Policy.MaxNotificationInterval)
                        {
                            maxNotificationInterval = this.timer.Policy.MaxNotificationInterval;
                        }
                        this.timer.CurrentInterval = maxNotificationInterval;
                    }
                    this.timer.NextNotification = this.state.ElapsedTime + this.timer.CurrentInterval;
                    this.state.TimerManager.Add(this, this.timer.Instance);
                }
                else
                {
                    this.timer.Active = false;
                    if (this.timer.Notifications > this.timer.Policy.MaxNotifications)
                    {
                        return;
                    }
                }
            }
            this.OnTimer(objA.Profile);
        }

        public bool StartTimer(TimerProfile profile)
        {
            lock (this.timerLock)
            {
                this.CancelTimer();
                if (!this.ChooseProfile(profile))
                {
                    return false;
                }
                this.timer.Active = true;
                this.timer.Notifications = 0L;
                this.timer.CurrentInterval = this.timer.Policy.InitialDelay;
                this.timer.NextNotification = this.state.ElapsedTime + this.timer.CurrentInterval;
                this.state.TimerManager.Add(this, this.timer.Instance);
            }
            return true;
        }

        public override string ToString()
        {
            return base.GetType().Name;
        }

        public void TraceInvalidEvent(SynchronizationEvent e, bool fatal)
        {
            if (DebugTrace.Error)
            {
                if (this.history != null)
                {
                    DebugTrace.TxTrace(TraceLevel.Error, e.Enlistment.EnlistmentId, "The {0} was not expected by the {1} state. The state machine history history follows:\n\n{2}", e, this.current, this.history.ToString());
                }
                else
                {
                    DebugTrace.TxTrace(TraceLevel.Error, e.Enlistment.EnlistmentId, "The {0} was not expected by the {1} state", e, this.current);
                }
            }
            if (fatal)
            {
                FatalUnexpectedStateMachineEventRecord.TraceAndLog(this.enlistment.EnlistmentId, this.enlistment.Enlistment.RemoteTransactionId, this.ToString(), this.current.ToString(), this.history, e.ToString(), null);
            }
            else
            {
                NonFatalUnexpectedStateMachineEventRecord.TraceAndLog(this.enlistment.EnlistmentId, this.enlistment.Enlistment.RemoteTransactionId, this.ToString(), this.current.ToString(), this.history, e.ToString(), null);
            }
        }

        public abstract Microsoft.Transactions.Wsat.StateMachines.State AbortedState { get; }

        public TransactionEnlistment Enlistment
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.enlistment;
            }
        }

        public StateMachineHistory History
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.history;
            }
        }

        public TimeSpan NextNotification
        {
            get
            {
                return this.timer.NextNotification;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.State State
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.current;
            }
        }

        public Guid UniqueId
        {
            get
            {
                return this.enlistment.EnlistmentId;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TimerState
        {
            public TimerInstance Instance;
            public TimerPolicy Policy;
            public long Notifications;
            public bool Active;
            public TimeSpan CurrentInterval;
            public TimeSpan NextNotification;
        }
    }
}

