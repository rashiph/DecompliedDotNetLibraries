namespace System.Workflow.Runtime
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public class WorkflowQueue
    {
        private WorkflowQueuingService qService;
        private IComparable queueName;

        public event EventHandler<QueueEventArgs> QueueItemArrived
        {
            add
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                lock (this.qService.SyncRoot)
                {
                    this.qService.GetQueueState(this.queueName).SynchronousListeners.Add(new ActivityExecutorDelegateInfo<QueueEventArgs>(value, this.qService.CallingActivity));
                }
            }
            remove
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                lock (this.qService.SyncRoot)
                {
                    this.qService.GetQueueState(this.queueName).SynchronousListeners.Remove(new ActivityExecutorDelegateInfo<QueueEventArgs>(value, this.qService.CallingActivity));
                }
            }
        }

        public event EventHandler<QueueEventArgs> QueueItemAvailable
        {
            add
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                lock (this.qService.SyncRoot)
                {
                    EventQueueState queueState = this.qService.GetQueueState(this.queueName);
                    ActivityExecutorDelegateInfo<QueueEventArgs> item = new ActivityExecutorDelegateInfo<QueueEventArgs>(value, this.qService.CallingActivity);
                    queueState.AsynchronousListeners.Add(item);
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "WorkflowQueue:QueueItemAvailable subscribe for activity '{0}' with context Id {1}", new object[] { item.ActivityQualifiedName, item.ContextId });
                    if (queueState.AsynchronousListeners.Count == 1)
                    {
                        this.qService.NotifyAsynchronousSubscribers(this.queueName, queueState, queueState.Messages.Count);
                    }
                }
            }
            remove
            {
                lock (this.qService.SyncRoot)
                {
                    ActivityExecutorDelegateInfo<QueueEventArgs> item = new ActivityExecutorDelegateInfo<QueueEventArgs>(value, this.qService.CallingActivity);
                    if (!this.qService.GetQueueState(this.queueName).AsynchronousListeners.Remove(item))
                    {
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "WorkflowQueue:QueueItemAvailable unsubscribe failed for activity '{0}' with context Id {1} ", new object[] { item.ActivityQualifiedName, item.ContextId });
                    }
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal WorkflowQueue(WorkflowQueuingService qService, IComparable queueName)
        {
            this.qService = qService;
            this.queueName = queueName;
        }

        public object Dequeue()
        {
            lock (this.qService.SyncRoot)
            {
                this.qService.Peek(this.queueName);
                return this.qService.DequeueEvent(this.queueName);
            }
        }

        public void Enqueue(object item)
        {
            lock (this.qService.SyncRoot)
            {
                this.qService.EnqueueEvent(this.queueName, item);
            }
        }

        public object Peek()
        {
            lock (this.qService.SyncRoot)
            {
                return this.qService.Peek(this.queueName);
            }
        }

        public void RegisterForQueueItemArrived(IActivityEventListener<QueueEventArgs> eventListener)
        {
            if (eventListener == null)
            {
                throw new ArgumentNullException("eventListener");
            }
            lock (this.qService.SyncRoot)
            {
                this.qService.GetQueueState(this.queueName).SynchronousListeners.Add(new ActivityExecutorDelegateInfo<QueueEventArgs>(eventListener, this.qService.CallingActivity));
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void RegisterForQueueItemAvailable(IActivityEventListener<QueueEventArgs> eventListener)
        {
            this.RegisterForQueueItemAvailable(eventListener, null);
        }

        public void RegisterForQueueItemAvailable(IActivityEventListener<QueueEventArgs> eventListener, string subscriberQualifiedName)
        {
            if (eventListener == null)
            {
                throw new ArgumentNullException("eventListener");
            }
            lock (this.qService.SyncRoot)
            {
                EventQueueState queueState = this.qService.GetQueueState(this.queueName);
                ActivityExecutorDelegateInfo<QueueEventArgs> item = new ActivityExecutorDelegateInfo<QueueEventArgs>(eventListener, this.qService.CallingActivity);
                if (subscriberQualifiedName != null)
                {
                    item.SubscribedActivityQualifiedName = subscriberQualifiedName;
                }
                queueState.AsynchronousListeners.Add(item);
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "WorkflowQueue:QueueItemAvailable subscribe for activity '{0}' with context Id {1}", new object[] { item.ActivityQualifiedName, item.ContextId });
                if (queueState.AsynchronousListeners.Count == 1)
                {
                    this.qService.NotifyAsynchronousSubscribers(this.queueName, queueState, queueState.Messages.Count);
                }
            }
        }

        public void UnregisterForQueueItemArrived(IActivityEventListener<QueueEventArgs> eventListener)
        {
            if (eventListener == null)
            {
                throw new ArgumentNullException("eventListener");
            }
            lock (this.qService.SyncRoot)
            {
                this.qService.GetQueueState(this.queueName).SynchronousListeners.Remove(new ActivityExecutorDelegateInfo<QueueEventArgs>(eventListener, this.qService.CallingActivity));
            }
        }

        public void UnregisterForQueueItemAvailable(IActivityEventListener<QueueEventArgs> eventListener)
        {
            if (eventListener == null)
            {
                throw new ArgumentNullException("eventListener");
            }
            lock (this.qService.SyncRoot)
            {
                ActivityExecutorDelegateInfo<QueueEventArgs> item = new ActivityExecutorDelegateInfo<QueueEventArgs>(eventListener, this.qService.CallingActivity);
                if (!this.qService.GetQueueState(this.queueName).AsynchronousListeners.Remove(item))
                {
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "WorkflowQueue:QueueItemAvailable unsubscribe failed for activity '{0}' with context Id {1}", new object[] { item.ActivityQualifiedName, item.ContextId });
                }
            }
        }

        public int Count
        {
            get
            {
                lock (this.qService.SyncRoot)
                {
                    return this.qService.GetQueueState(this.queueName).Messages.Count;
                }
            }
        }

        public bool Enabled
        {
            get
            {
                lock (this.qService.SyncRoot)
                {
                    return this.qService.GetQueueState(this.queueName).Enabled;
                }
            }
            set
            {
                lock (this.qService.SyncRoot)
                {
                    this.qService.GetQueueState(this.queueName).Enabled = value;
                }
            }
        }

        public IComparable QueueName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.queueName;
            }
        }

        public WorkflowQueuingService QueuingService
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.qService;
            }
        }
    }
}

