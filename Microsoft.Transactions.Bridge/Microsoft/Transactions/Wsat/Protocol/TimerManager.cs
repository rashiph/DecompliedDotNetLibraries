namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime;
    using System.Threading;

    internal class TimerManager : IComparer
    {
        private bool active;
        private TimeSpan maxNotificationTime = new TimeSpan(0, 30, 0);
        private TimeSpan reminderGranularity = new TimeSpan(0, 0, 10);
        private TimeSpan reminderTolerance = new TimeSpan(0, 0, 1);
        private ProtocolState state;
        private Timer timer;
        private SortedList timerList;

        public TimerManager(ProtocolState state)
        {
            this.state = state;
            this.timerList = new SortedList(this);
            this.timer = new Timer(Fx.ThunkCallback(new TimerCallback(this.OnTimer)), null, -1, -1);
        }

        private void ActivateTimer()
        {
            if (!this.timer.Change(this.reminderGranularity, this.reminderGranularity) && DebugTrace.Info)
            {
                DebugTrace.Trace(TraceLevel.Info, "TimerManager.ActivateTimer: Timer.Change returned false");
            }
            this.active = true;
            this.AssertState();
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Activated timer notification to {0} seconds", (int) this.reminderGranularity.TotalSeconds);
            }
        }

        public void Add(ITimerRecipient recipient, object token)
        {
            int count;
            DebugTrace.TraceEnter(this, "Add");
            TimeSpan span = recipient.NextNotification - this.state.ElapsedTime;
            if ((span <= TimeSpan.Zero) || (span >= this.maxNotificationTime))
            {
                DiagnosticUtility.FailFast("The timer object has an invalid notification time");
            }
            lock (this.timerList.SyncRoot)
            {
                this.AssertState();
                this.timerList.Add(recipient, token);
                count = this.timerList.Count;
                if (count == 1)
                {
                    this.ActivateTimer();
                }
            }
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Added timer recipient to be reminded in {0} ms", (long) span.TotalMilliseconds);
                DebugTrace.Trace(TraceLevel.Verbose, "Timer list depth at {0}", count);
            }
            DebugTrace.TraceLeave(this, "Add");
        }

        private void AssertState()
        {
            if (this.active)
            {
                if (this.timerList.Count <= 0)
                {
                    DiagnosticUtility.FailFast("The timer list must not be empty");
                }
            }
            else if (this.timerList.Count != 0)
            {
                DiagnosticUtility.FailFast("The timer list must be empty");
            }
        }

        public int Compare(object x, object y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return 0;
            }
            ITimerRecipient recipient = (ITimerRecipient) x;
            ITimerRecipient recipient2 = (ITimerRecipient) y;
            TimeSpan nextNotification = recipient.NextNotification;
            TimeSpan span2 = recipient2.NextNotification;
            int num = nextNotification.CompareTo(span2);
            if (num == 0)
            {
                num = recipient.UniqueId.CompareTo(recipient2.UniqueId);
                if (num == 0)
                {
                    DiagnosticUtility.FailFast("A duplicate object was added to the timer list");
                }
            }
            return -num;
        }

        private void DeactivateTimer()
        {
            DebugTrace.Trace(TraceLevel.Verbose, "Timer list is now empty. Canceling periodic timer notification");
            if (!this.timer.Change(-1, -1) && DebugTrace.Info)
            {
                DebugTrace.Trace(TraceLevel.Info, "TimerManager.DeactivateTimer: Timer.Change returned false");
            }
            this.active = false;
            this.AssertState();
        }

        private void ExecuteTimer()
        {
            DebugTrace.TraceEnter(this, "OnTimer");
            ArrayList list = null;
            TimeSpan elapsedTime = this.state.ElapsedTime;
            lock (this.timerList.SyncRoot)
            {
                int count = this.timerList.Count;
                DebugTrace.Trace(TraceLevel.Verbose, "Timer list depth at {0}", count);
                for (int i = count - 1; i >= 0; i--)
                {
                    ITimerRecipient key = (ITimerRecipient) this.timerList.GetKey(i);
                    TimeSpan span2 = key.NextNotification - elapsedTime;
                    if (span2 > this.reminderTolerance)
                    {
                        if (DebugTrace.Verbose)
                        {
                            DebugTrace.Trace(TraceLevel.Verbose, "Timer list found entry scheduled for {0} ms in the future", (long) span2.TotalMilliseconds);
                        }
                        break;
                    }
                    if (DebugTrace.Verbose)
                    {
                        DebugTrace.Trace(TraceLevel.Verbose, "Timer list dispatching to recipient scheduled for {0} ms in the {1}", (long) span2.Duration().TotalMilliseconds, (span2 > TimeSpan.Zero) ? "future" : "past");
                    }
                    object byIndex = this.timerList.GetByIndex(i);
                    this.timerList.RemoveAt(i);
                    if (list == null)
                    {
                        list = new ArrayList(0x20);
                    }
                    list.Add(key);
                    list.Add(byIndex);
                }
                if ((list != null) && (this.timerList.Count == 0))
                {
                    this.DeactivateTimer();
                }
            }
            if (list != null)
            {
                int num3 = list.Count;
                if ((num3 % 2) != 0)
                {
                    DiagnosticUtility.FailFast("Recipient list count must be even");
                }
                if (DebugTrace.Verbose)
                {
                    int num4 = num3 / 2;
                    DebugTrace.Trace(TraceLevel.Verbose, "Dispatching timer notification to {0} recipient{1}", num4, (num4 != 1) ? "s" : string.Empty);
                }
                for (int j = 0; j < num3; j += 2)
                {
                    ITimerRecipient recipient2 = (ITimerRecipient) list[j];
                    object token = list[j + 1];
                    recipient2.OnTimerNotification(token);
                }
            }
            DebugTrace.TraceLeave(this, "OnTimer");
        }

        private void OnTimer(object obj)
        {
            try
            {
                this.ExecuteTimer();
            }
            catch (Exception exception)
            {
                DiagnosticUtility.InvokeFinalHandler(exception);
            }
        }

        public void Remove(ITimerRecipient recipient)
        {
            int count;
            int num2;
            DebugTrace.TraceEnter(this, "Remove");
            lock (this.timerList.SyncRoot)
            {
                this.AssertState();
                count = this.timerList.Count;
                this.timerList.Remove(recipient);
                num2 = this.timerList.Count;
                if ((count != num2) && (this.timerList.Count == 0))
                {
                    this.DeactivateTimer();
                }
            }
            if (DebugTrace.Verbose)
            {
                if (count == num2)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Timer recipient was not present. Timer list depth is still {0}", num2);
                }
                else
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Removed timer recipient. Timer list depth is now {0}", num2);
                }
            }
            DebugTrace.TraceLeave(this, "Remove");
        }
    }
}

