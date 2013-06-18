namespace System.Workflow.Runtime
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime.Hosting;

    [Serializable]
    public class TimerEventSubscriptionCollection : ICollection, IEnumerable
    {
        [NonSerialized]
        private IWorkflowCoreRuntime executor;
        private Guid instanceId;
        private object locker = new object();
        private KeyedPriorityQueue<Guid, TimerEventSubscription, DateTime> queue = new KeyedPriorityQueue<Guid, TimerEventSubscription, DateTime>();
        private bool suspended;
        public static readonly DependencyProperty TimerCollectionProperty = DependencyProperty.RegisterAttached("TimerCollection", typeof(TimerEventSubscriptionCollection), typeof(TimerEventSubscriptionCollection));

        internal TimerEventSubscriptionCollection(IWorkflowCoreRuntime executor, Guid instanceId)
        {
            this.executor = executor;
            this.instanceId = instanceId;
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TimerEventSubscriptionQueue: {0} Created", new object[] { instanceId });
            this.queue.FirstElementChanged += new EventHandler<KeyedPriorityQueueHeadChangedEventArgs<TimerEventSubscription>>(this.OnFirstElementChanged);
        }

        public void Add(TimerEventSubscription item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            this.Enqueue(item);
        }

        public void CopyTo(Array array, int index)
        {
            TimerEventSubscription[] subscriptionArray = null;
            lock (this.locker)
            {
                subscriptionArray = new TimerEventSubscription[this.queue.Count];
                this.queue.Values.CopyTo(subscriptionArray, 0);
            }
            if (subscriptionArray != null)
            {
                subscriptionArray.CopyTo(array, index);
            }
        }

        internal TimerEventSubscription Dequeue()
        {
            lock (this.locker)
            {
                TimerEventSubscription subscription = this.queue.Dequeue();
                if (subscription != null)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TimerEventSubscriptionQueue: {0} Dequeue Timer {1} for {2} ", new object[] { this.instanceId, subscription.SubscriptionId, subscription.ExpiresAt });
                }
                return subscription;
            }
        }

        internal void Enqueue(TimerEventSubscription timerEventSubscription)
        {
            lock (this.locker)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TimerEventSubscriptionQueue: {0} Enqueue Timer {1} for {2} ", new object[] { this.instanceId, timerEventSubscription.SubscriptionId, timerEventSubscription.ExpiresAt });
                this.queue.Enqueue(timerEventSubscription.SubscriptionId, timerEventSubscription, timerEventSubscription.ExpiresAt);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return this.queue.Values.GetEnumerator();
        }

        private void OnFirstElementChanged(object source, KeyedPriorityQueueHeadChangedEventArgs<TimerEventSubscription> e)
        {
            lock (this.locker)
            {
                ITimerService service = this.executor.GetService(typeof(ITimerService)) as ITimerService;
                if ((e.NewFirstElement != null) && (this.executor != null))
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TimerEventSubscriptionQueue: {0} Schedule Timer {1} for {2} ", new object[] { this.instanceId, e.NewFirstElement.SubscriptionId, e.NewFirstElement.ExpiresAt });
                    service.ScheduleTimer(this.executor.ProcessTimersCallback, e.NewFirstElement.WorkflowInstanceId, e.NewFirstElement.ExpiresAt, e.NewFirstElement.SubscriptionId);
                }
                if (e.OldFirstElement != null)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TimerEventSubscriptionQueue: {0} Unschedule Timer {1} for {2} ", new object[] { this.instanceId, e.OldFirstElement.SubscriptionId, e.OldFirstElement.ExpiresAt });
                    service.CancelTimer(e.OldFirstElement.SubscriptionId);
                }
            }
        }

        public TimerEventSubscription Peek()
        {
            lock (this.locker)
            {
                return this.queue.Peek();
            }
        }

        public void Remove(Guid timerSubscriptionId)
        {
            lock (this.locker)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TimerEventSubscriptionQueue: {0} Remove Timer {1}", new object[] { this.instanceId, timerSubscriptionId });
                this.queue.Remove(timerSubscriptionId);
            }
        }

        public void Remove(TimerEventSubscription item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            this.Remove(item.SubscriptionId);
        }

        internal void ResumeDelivery()
        {
            lock (this.locker)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TimerEventSubscriptionQueue: {0} Resume", new object[] { this.instanceId });
                WorkflowSchedulerService service = this.executor.GetService(typeof(WorkflowSchedulerService)) as WorkflowSchedulerService;
                TimerEventSubscription subscription = this.queue.Peek();
                if (subscription != null)
                {
                    service.Schedule(this.executor.ProcessTimersCallback, subscription.WorkflowInstanceId, subscription.ExpiresAt, subscription.SubscriptionId);
                }
            }
        }

        internal void SuspendDelivery()
        {
            lock (this.locker)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TimerEventSubscriptionQueue: {0} Suspend", new object[] { this.instanceId });
                WorkflowSchedulerService service = this.executor.GetService(typeof(WorkflowSchedulerService)) as WorkflowSchedulerService;
                TimerEventSubscription subscription = this.queue.Peek();
                if (subscription != null)
                {
                    service.Cancel(subscription.SubscriptionId);
                }
            }
        }

        public int Count
        {
            get
            {
                return this.queue.Count;
            }
        }

        internal IWorkflowCoreRuntime Executor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.executor;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.executor = value;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public object SyncRoot
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.locker;
            }
        }
    }
}

