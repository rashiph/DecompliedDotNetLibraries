namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Workflow.Runtime;

    public class DefaultWorkflowSchedulerService : WorkflowSchedulerService
    {
        private Timer callbackTimer;
        private const int DEFAULT_MAX_SIMULTANEOUS_WORKFLOWS = 5;
        private static TimeSpan fiveMinutes = new TimeSpan(0, 5, 0);
        private static TimeSpan infinite = new TimeSpan(-1L);
        private const string MAX_SIMULTANEOUS_WORKFLOWS_KEY = "maxSimultaneousWorkflows";
        private readonly int maxSimultaneousWorkflows;
        private int numCurrentWorkers;
        private IList<PerformanceCounter> queueCounters;
        private volatile bool running;
        private TimerCallback timerCallback;
        private KeyedPriorityQueue<Guid, CallbackInfo, DateTime> timerQueue;
        private Queue<WorkItem> waitingQueue;

        public DefaultWorkflowSchedulerService() : this(DefaultThreadCount)
        {
        }

        public DefaultWorkflowSchedulerService(NameValueCollection parameters)
        {
            this.timerQueue = new KeyedPriorityQueue<Guid, CallbackInfo, DateTime>();
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            this.maxSimultaneousWorkflows = DefaultThreadCount;
            foreach (string str in parameters.Keys)
            {
                if (str == null)
                {
                    throw new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, ExecutionStringManager.UnknownConfigurationParameter, new object[] { "null" }));
                }
                string s = parameters[str];
                if (!str.Equals("maxSimultaneousWorkflows", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, ExecutionStringManager.UnknownConfigurationParameter, new object[] { str }));
                }
                if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.CurrentCulture, out this.maxSimultaneousWorkflows))
                {
                    throw new FormatException("maxSimultaneousWorkflows");
                }
            }
            if (this.maxSimultaneousWorkflows < 1)
            {
                throw new ArgumentOutOfRangeException("maxSimultaneousWorkflows", this.maxSimultaneousWorkflows, string.Empty);
            }
            this.init();
        }

        public DefaultWorkflowSchedulerService(int maxSimultaneousWorkflows)
        {
            this.timerQueue = new KeyedPriorityQueue<Guid, CallbackInfo, DateTime>();
            if (maxSimultaneousWorkflows < 1)
            {
                throw new ArgumentOutOfRangeException("maxSimultaneousWorkflows", maxSimultaneousWorkflows, string.Empty);
            }
            this.maxSimultaneousWorkflows = maxSimultaneousWorkflows;
            this.init();
        }

        protected internal override void Cancel(Guid timerId)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Cancelling work with timer ID {0}", new object[] { timerId });
            if (timerId == Guid.Empty)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, ExecutionStringManager.CantBeEmptyGuid, new object[] { "timerId" }), "timerId");
            }
            lock (this.timerQueue)
            {
                this.timerQueue.Remove(timerId);
            }
        }

        private Timer CreateTimerCallback(CallbackInfo info)
        {
            DateTime utcNow = DateTime.UtcNow;
            TimeSpan dueTime = (info.When > utcNow) ? ((TimeSpan) (info.When - utcNow)) : TimeSpan.Zero;
            if (dueTime > fiveMinutes)
            {
                dueTime = fiveMinutes;
            }
            return new Timer(this.timerCallback, info.State, dueTime, infinite);
        }

        private void EnqueueWorkItem(WorkItem workItem)
        {
            lock (this.waitingQueue)
            {
                this.waitingQueue.Enqueue(workItem);
                if ((this.running && (this.numCurrentWorkers < this.maxSimultaneousWorkflows)) && ThreadPool.QueueUserWorkItem(new WaitCallback(this.QueueWorkerProcess)))
                {
                    this.numCurrentWorkers++;
                }
            }
            if (this.queueCounters != null)
            {
                foreach (PerformanceCounter counter in this.queueCounters)
                {
                    counter.RawValue = this.waitingQueue.Count;
                }
            }
        }

        private void init()
        {
            this.timerCallback = new TimerCallback(this.OnTimerCallback);
            this.timerQueue.FirstElementChanged += new EventHandler<KeyedPriorityQueueHeadChangedEventArgs<CallbackInfo>>(this.OnFirstElementChanged);
            this.waitingQueue = new Queue<WorkItem>();
        }

        private void OnFirstElementChanged(object source, KeyedPriorityQueueHeadChangedEventArgs<CallbackInfo> e)
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

        protected override void OnStarted()
        {
            lock (this.timerQueue)
            {
                base.OnStarted();
                CallbackInfo info = this.timerQueue.Peek();
                if (info != null)
                {
                    this.callbackTimer = this.CreateTimerCallback(info);
                }
                this.running = true;
            }
            lock (this.waitingQueue)
            {
                int num = Math.Min(this.maxSimultaneousWorkflows, this.waitingQueue.Count);
                for (int i = 0; i < num; i++)
                {
                    if (ThreadPool.QueueUserWorkItem(new WaitCallback(this.QueueWorkerProcess)))
                    {
                        this.numCurrentWorkers++;
                    }
                }
            }
            if ((this.queueCounters == null) && (base.Runtime.PerformanceCounterManager != null))
            {
                this.queueCounters = base.Runtime.PerformanceCounterManager.CreateCounters(ExecutionStringManager.PerformanceCounterWorkflowsWaitingName);
            }
        }

        private void OnTimerCallback(object ignored)
        {
            Trace.CorrelationManager.ActivityId = Guid.Empty;
            CallbackInfo info = null;
            bool flag = false;
            try
            {
                lock (this.timerQueue)
                {
                    if (base.State == WorkflowRuntimeServiceState.Started)
                    {
                        info = this.timerQueue.Peek();
                        if (info != null)
                        {
                            if (info.IsExpired)
                            {
                                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Timeout occured for timer for instance {0}", new object[] { info.State });
                                this.timerQueue.Dequeue();
                                flag = true;
                            }
                            else
                            {
                                this.callbackTimer = this.CreateTimerCallback(info);
                            }
                        }
                    }
                }
                if (flag && (info != null))
                {
                    info.Callback(info.State);
                }
            }
            catch (WorkflowOwnershipException)
            {
            }
            catch (ThreadAbortException exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "Timeout for instance, {0} threw exception {1}", new object[] { (info == null) ? null : info.State, exception.Message });
                base.RaiseServicesExceptionNotHandledEvent(exception, (Guid) info.State);
                throw;
            }
            catch (Exception exception2)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "Timeout for instance, {0} threw exception {1}", new object[] { (info == null) ? null : info.State, exception2.Message });
                base.RaiseServicesExceptionNotHandledEvent(exception2, (Guid) info.State);
            }
        }

        private void QueueWorkerProcess(object state)
        {
            Trace.CorrelationManager.ActivityId = Guid.Empty;
            while (true)
            {
                WorkItem item;
                lock (this.waitingQueue)
                {
                    if ((this.waitingQueue.Count == 0) || !this.running)
                    {
                        this.numCurrentWorkers--;
                        Monitor.Pulse(this.waitingQueue);
                        return;
                    }
                    item = this.waitingQueue.Dequeue();
                }
                if (this.queueCounters != null)
                {
                    foreach (PerformanceCounter counter in this.queueCounters)
                    {
                        counter.RawValue = this.waitingQueue.Count;
                    }
                }
                item.Invoke(this);
            }
        }

        protected internal override void Schedule(WaitCallback callback, Guid workflowInstanceId)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Scheduling work for instance {0}", new object[] { workflowInstanceId });
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            if (workflowInstanceId == Guid.Empty)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, ExecutionStringManager.CantBeEmptyGuid, new object[] { "workflowInstanceId" }));
            }
            this.EnqueueWorkItem(new WorkItem(callback, workflowInstanceId));
        }

        protected internal override void Schedule(WaitCallback callback, Guid workflowInstanceId, DateTime whenUtc, Guid timerId)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Scheduling work for instance {0} on timer ID {1} in {2}", new object[] { workflowInstanceId, timerId, (TimeSpan) (whenUtc - DateTime.UtcNow) });
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            if (timerId == Guid.Empty)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, ExecutionStringManager.CantBeEmptyGuid, new object[] { "timerId" }));
            }
            if (workflowInstanceId == Guid.Empty)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, ExecutionStringManager.CantBeEmptyGuid, new object[] { "workflowInstanceId" }));
            }
            CallbackInfo info = new CallbackInfo(this, callback, workflowInstanceId, whenUtc);
            lock (this.timerQueue)
            {
                this.timerQueue.Enqueue(timerId, info, whenUtc);
            }
        }

        protected internal override void Stop()
        {
            lock (this.timerQueue)
            {
                base.Stop();
                if (this.callbackTimer != null)
                {
                    this.callbackTimer.Dispose();
                    this.callbackTimer = null;
                }
                this.running = false;
            }
            lock (this.waitingQueue)
            {
                while (this.numCurrentWorkers > 0)
                {
                    Monitor.Wait(this.waitingQueue);
                }
            }
        }

        private static int DefaultThreadCount
        {
            get
            {
                if (Environment.ProcessorCount != 1)
                {
                    return (int) ((5 * Environment.ProcessorCount) * 0.8);
                }
                return 5;
            }
        }

        public int MaxSimultaneousWorkflows
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.maxSimultaneousWorkflows;
            }
        }

        internal class CallbackInfo
        {
            private WaitCallback callback;
            private WorkflowSchedulerService service;
            private object state;
            private DateTime when;

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public CallbackInfo(WorkflowSchedulerService service, WaitCallback callback, object state, DateTime when)
            {
                this.service = service;
                this.callback = callback;
                this.state = state;
                this.when = when;
            }

            public WaitCallback Callback
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.callback;
                }
            }

            public bool IsExpired
            {
                get
                {
                    return (DateTime.UtcNow >= this.when);
                }
            }

            public object State
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.state;
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

        internal class WorkItem
        {
            private WaitCallback callback;
            private object state;

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public WorkItem(WaitCallback callback, object state)
            {
                this.callback = callback;
                this.state = state;
            }

            public void Invoke(WorkflowSchedulerService service)
            {
                try
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Running workflow {0}", new object[] { this.state });
                    this.Callback(this.state);
                }
                catch (Exception exception)
                {
                    if (WorkflowExecutor.IsIrrecoverableException(exception))
                    {
                        throw;
                    }
                    service.RaiseExceptionNotHandledEvent(exception, (Guid) this.state);
                }
            }

            public WaitCallback Callback
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.callback;
                }
            }
        }
    }
}

