namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Workflow.Runtime;

    public class ManualWorkflowSchedulerService : WorkflowSchedulerService
    {
        private Timer callbackTimer;
        private static TimeSpan fiveMinutes = new TimeSpan(0, 5, 0);
        private static TimeSpan infinite = new TimeSpan(-1L);
        private object locker;
        private KeyedPriorityQueue<Guid, CallbackInfo, DateTime> pendingScheduleRequests;
        private IList<PerformanceCounter> queueCounters;
        private Dictionary<Guid, DefaultWorkflowSchedulerService.WorkItem> scheduleRequests;
        private volatile bool threadRunning;
        private readonly TimerCallback timerCallback;
        private const string USE_ACTIVE_TIMERS_KEY = "UseActiveTimers";

        public ManualWorkflowSchedulerService()
        {
            this.pendingScheduleRequests = new KeyedPriorityQueue<Guid, CallbackInfo, DateTime>();
            this.scheduleRequests = new Dictionary<Guid, DefaultWorkflowSchedulerService.WorkItem>();
            this.locker = new object();
        }

        public ManualWorkflowSchedulerService(bool useActiveTimers)
        {
            this.pendingScheduleRequests = new KeyedPriorityQueue<Guid, CallbackInfo, DateTime>();
            this.scheduleRequests = new Dictionary<Guid, DefaultWorkflowSchedulerService.WorkItem>();
            this.locker = new object();
            if (useActiveTimers)
            {
                this.timerCallback = new TimerCallback(this.OnTimerCallback);
                this.pendingScheduleRequests.FirstElementChanged += new EventHandler<KeyedPriorityQueueHeadChangedEventArgs<CallbackInfo>>(this.OnFirstElementChanged);
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: started with active timers");
            }
        }

        public ManualWorkflowSchedulerService(NameValueCollection parameters)
        {
            this.pendingScheduleRequests = new KeyedPriorityQueue<Guid, CallbackInfo, DateTime>();
            this.scheduleRequests = new Dictionary<Guid, DefaultWorkflowSchedulerService.WorkItem>();
            this.locker = new object();
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            foreach (string str in parameters.Keys)
            {
                bool flag;
                if (str == null)
                {
                    throw new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, ExecutionStringManager.UnknownConfigurationParameter, new object[] { "null" }));
                }
                string str2 = parameters[str];
                if (!str.Equals("UseActiveTimers", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, ExecutionStringManager.UnknownConfigurationParameter, new object[] { str }));
                }
                if (!bool.TryParse(str2, out flag))
                {
                    throw new FormatException("UseActiveTimers");
                }
                if (flag)
                {
                    this.timerCallback = new TimerCallback(this.OnTimerCallback);
                    this.pendingScheduleRequests.FirstElementChanged += new EventHandler<KeyedPriorityQueueHeadChangedEventArgs<CallbackInfo>>(this.OnFirstElementChanged);
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: Started with active timers");
                }
            }
        }

        protected internal override void Cancel(Guid timerId)
        {
            if (timerId.Equals(Guid.Empty))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CantBeEmptyGuid, new object[] { "timerId" }));
            }
            lock (this.locker)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: Cancel timer {0}", new object[] { timerId });
                this.pendingScheduleRequests.Remove(timerId);
            }
        }

        private bool CanRun(Guid workflowInstanceId)
        {
            bool flag = false;
            lock (this.locker)
            {
                Guid guid;
                flag = this.scheduleRequests.ContainsKey(workflowInstanceId) || this.HasExpiredTimer(workflowInstanceId, out guid);
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: CanRun is {0}", new object[] { flag });
            }
            return flag;
        }

        private Timer CreateTimerCallback(CallbackInfo info)
        {
            DateTime utcNow = DateTime.UtcNow;
            TimeSpan dueTime = (info.When > utcNow) ? ((TimeSpan) (info.When - utcNow)) : TimeSpan.Zero;
            if (dueTime > fiveMinutes)
            {
                dueTime = fiveMinutes;
            }
            return new Timer(this.timerCallback, info.InstanceId, dueTime, infinite);
        }

        private bool HasExpiredTimer(Guid workflowInstanceId, out Guid timerId)
        {
            Predicate<CallbackInfo> match = null;
            lock (this.locker)
            {
                if (match == null)
                {
                    match = c => c.InstanceId == workflowInstanceId;
                }
                CallbackInfo info = this.pendingScheduleRequests.FindByPriority(DateTime.UtcNow, match);
                if (info != null)
                {
                    timerId = info.TimerId;
                    return true;
                }
            }
            timerId = Guid.Empty;
            return false;
        }

        private void OnFirstElementChanged(object source, KeyedPriorityQueueHeadChangedEventArgs<CallbackInfo> e)
        {
            lock (this.locker)
            {
                if (!this.threadRunning)
                {
                    if (this.callbackTimer != null)
                    {
                        this.callbackTimer.Dispose();
                        this.callbackTimer = null;
                    }
                    if ((e.NewFirstElement != null) && (base.State == WorkflowRuntimeServiceState.Started))
                    {
                        this.callbackTimer = this.CreateTimerCallback(e.NewFirstElement);
                    }
                }
            }
        }

        protected override void OnStarted()
        {
            base.OnStarted();
            if (this.timerCallback != null)
            {
                lock (this.locker)
                {
                    CallbackInfo info = this.pendingScheduleRequests.Peek();
                    if (info != null)
                    {
                        this.callbackTimer = this.CreateTimerCallback(info);
                    }
                }
            }
            lock (this.locker)
            {
                if ((this.queueCounters == null) && (base.Runtime.PerformanceCounterManager != null))
                {
                    this.queueCounters = base.Runtime.PerformanceCounterManager.CreateCounters(ExecutionStringManager.PerformanceCounterWorkflowsWaitingName);
                }
            }
        }

        private void OnTimerCallback(object ignored)
        {
            CallbackInfo info = null;
            try
            {
                lock (this.locker)
                {
                    if (base.State.Equals(WorkflowRuntimeServiceState.Started))
                    {
                        info = this.pendingScheduleRequests.Peek();
                        if (info != null)
                        {
                            if (info.IsExpired)
                            {
                                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Timeout occured for timer for instance {0}", new object[] { info.InstanceId });
                                this.threadRunning = true;
                                this.pendingScheduleRequests.Dequeue();
                            }
                            else
                            {
                                this.callbackTimer = this.CreateTimerCallback(info);
                            }
                        }
                    }
                }
                if (this.threadRunning)
                {
                    info.Callback(info.InstanceId);
                    this.RunWorkflow(info.InstanceId);
                }
            }
            catch (ThreadAbortException exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "Timeout for instance, {0} threw exception {1}", new object[] { (info == null) ? Guid.Empty : info.InstanceId, exception.Message });
                base.RaiseServicesExceptionNotHandledEvent(exception, info.InstanceId);
                throw;
            }
            catch (Exception exception2)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "Timeout for instance, {0} threw exception {1}", new object[] { (info == null) ? Guid.Empty : info.InstanceId, exception2.Message });
                base.RaiseServicesExceptionNotHandledEvent(exception2, info.InstanceId);
            }
            finally
            {
                lock (this.locker)
                {
                    if (this.threadRunning)
                    {
                        this.threadRunning = false;
                        info = this.pendingScheduleRequests.Peek();
                        if (info != null)
                        {
                            this.callbackTimer = this.CreateTimerCallback(info);
                        }
                    }
                }
            }
        }

        private bool ProcessTimer(Guid workflowInstanceId)
        {
            bool flag = false;
            CallbackInfo info = null;
            Guid empty = Guid.Empty;
            lock (this.locker)
            {
                Guid guid2;
                if (this.HasExpiredTimer(workflowInstanceId, out guid2))
                {
                    info = this.pendingScheduleRequests.Remove(guid2);
                }
            }
            try
            {
                if (info != null)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: Processing timer {0}", new object[] { empty });
                    info.Callback(info.InstanceId);
                    flag = true;
                }
            }
            catch (Exception exception)
            {
                base.RaiseServicesExceptionNotHandledEvent(exception, workflowInstanceId);
            }
            return flag;
        }

        private bool RunOne(Guid workflowInstanceId)
        {
            bool flag = false;
            DefaultWorkflowSchedulerService.WorkItem item = null;
            lock (this.locker)
            {
                if (this.scheduleRequests.ContainsKey(workflowInstanceId))
                {
                    item = this.scheduleRequests[workflowInstanceId];
                    this.scheduleRequests.Remove(workflowInstanceId);
                }
            }
            try
            {
                if (item == null)
                {
                    return flag;
                }
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: Executing {0}", new object[] { workflowInstanceId });
                if (this.queueCounters != null)
                {
                    foreach (PerformanceCounter counter in this.queueCounters)
                    {
                        counter.RawValue = this.scheduleRequests.Count;
                    }
                }
                item.Invoke(this);
                flag = true;
            }
            catch (Exception exception)
            {
                base.RaiseServicesExceptionNotHandledEvent(exception, workflowInstanceId);
            }
            return flag;
        }

        public bool RunWorkflow(Guid workflowInstanceId)
        {
            if (workflowInstanceId.Equals(Guid.Empty))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CantBeEmptyGuid, new object[] { "workflowInstanceId" }));
            }
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: Running workflow {0}", new object[] { workflowInstanceId });
            bool flag = false;
            while (this.CanRun(workflowInstanceId))
            {
                if (!this.RunOne(workflowInstanceId) && !this.ProcessTimer(workflowInstanceId))
                {
                    return flag;
                }
                flag = true;
            }
            return flag;
        }

        protected internal override void Schedule(WaitCallback callback, Guid workflowInstanceId)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            if (workflowInstanceId.Equals(Guid.Empty))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CantBeEmptyGuid, new object[] { "workflowInstanceId" }));
            }
            lock (this.locker)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: Schedule workflow {0}", new object[] { workflowInstanceId });
                if (!this.scheduleRequests.ContainsKey(workflowInstanceId))
                {
                    this.scheduleRequests.Add(workflowInstanceId, new DefaultWorkflowSchedulerService.WorkItem(callback, workflowInstanceId));
                }
            }
            if (this.queueCounters != null)
            {
                foreach (PerformanceCounter counter in this.queueCounters)
                {
                    counter.RawValue = this.scheduleRequests.Count;
                }
            }
        }

        protected internal override void Schedule(WaitCallback callback, Guid workflowInstanceId, DateTime whenUtc, Guid timerId)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            if (workflowInstanceId.Equals(Guid.Empty))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CantBeEmptyGuid, new object[] { "workflowInstanceId" }));
            }
            if (timerId.Equals(Guid.Empty))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CantBeEmptyGuid, new object[] { "timerId" }));
            }
            lock (this.locker)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: Schedule timer {0} for workflow {1} at {2}", new object[] { timerId, workflowInstanceId, whenUtc });
                this.pendingScheduleRequests.Enqueue(timerId, new CallbackInfo(callback, workflowInstanceId, timerId, whenUtc), whenUtc);
            }
        }

        protected internal override void Stop()
        {
            base.Stop();
            if (this.timerCallback != null)
            {
                lock (this.locker)
                {
                    if (this.callbackTimer != null)
                    {
                        this.callbackTimer.Dispose();
                        this.callbackTimer = null;
                    }
                }
            }
        }

        private class CallbackInfo
        {
            private WaitCallback callback;
            private Guid instanceId;
            private Guid timerId;
            private DateTime when;

            public CallbackInfo(WaitCallback callback, Guid instanceId, Guid timerId, DateTime when)
            {
                this.callback = callback;
                this.when = when;
                this.instanceId = instanceId;
                this.timerId = timerId;
            }

            public WaitCallback Callback
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.callback;
                }
            }

            public Guid InstanceId
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.instanceId;
                }
            }

            public bool IsExpired
            {
                get
                {
                    return (DateTime.UtcNow >= this.when);
                }
            }

            public Guid TimerId
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.timerId;
                }
            }

            public DateTime When
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.when;
                }
            }
        }
    }
}

