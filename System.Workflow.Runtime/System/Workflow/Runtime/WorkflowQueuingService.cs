namespace System.Workflow.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Messaging;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public class WorkflowQueuingService
    {
        private Activity caller;
        private List<IComparable> dirtyQueues;
        internal static DependencyProperty LocalPersistedQueueStatesProperty = DependencyProperty.RegisterAttached("LocalPersistedQueueStates", typeof(Dictionary<IComparable, EventQueueState>), typeof(WorkflowQueuingService));
        private List<WorkflowQueuingService> messageArrivalEventHandlers;
        public static readonly DependencyProperty PendingMessagesProperty = DependencyProperty.RegisterAttached("PendingMessages", typeof(Queue), typeof(WorkflowQueuingService), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));
        private const string pendingNotification = "*PendingNotifications";
        private EventQueueState pendingQueueState;
        private EventQueueState pendingQueueStateSnapshot;
        private Dictionary<IComparable, EventQueueState> persistedQueueStates;
        private Dictionary<IComparable, EventQueueState> persistedQueueStatesSnapshot;
        internal static DependencyProperty RootPersistedQueueStatesProperty = DependencyProperty.RegisterAttached("RootPersistedQueueStates", typeof(Dictionary<IComparable, EventQueueState>), typeof(WorkflowQueuingService));
        private WorkflowQueuingService rootQueuingService;
        private IWorkflowCoreRuntime rootWorkflowExecutor;
        private object syncRoot;

        internal WorkflowQueuingService(IWorkflowCoreRuntime rootWorkflowExecutor)
        {
            this.syncRoot = new object();
            this.pendingQueueState = new EventQueueState();
            this.rootWorkflowExecutor = rootWorkflowExecutor;
            this.rootWorkflowExecutor.RootActivity.SetValue(PendingMessagesProperty, this.pendingQueueState.Messages);
            this.persistedQueueStates = (Dictionary<IComparable, EventQueueState>) this.rootWorkflowExecutor.RootActivity.GetValue(RootPersistedQueueStatesProperty);
            if (this.persistedQueueStates == null)
            {
                this.persistedQueueStates = new Dictionary<IComparable, EventQueueState>();
                this.rootWorkflowExecutor.RootActivity.SetValue(RootPersistedQueueStatesProperty, this.persistedQueueStates);
            }
            if (!this.Exists("*PendingNotifications"))
            {
                this.CreateWorkflowQueue("*PendingNotifications", false);
            }
        }

        internal WorkflowQueuingService(WorkflowQueuingService copyFromQueuingService)
        {
            this.syncRoot = new object();
            this.pendingQueueState = new EventQueueState();
            this.rootQueuingService = copyFromQueuingService;
            this.rootWorkflowExecutor = copyFromQueuingService.rootWorkflowExecutor;
            this.rootWorkflowExecutor.RootActivity.SetValue(PendingMessagesProperty, this.pendingQueueState.Messages);
            this.persistedQueueStates = new Dictionary<IComparable, EventQueueState>();
            this.rootWorkflowExecutor.RootActivity.SetValue(LocalPersistedQueueStatesProperty, this.persistedQueueStates);
            this.SubscribeForRootMessageDelivery();
        }

        private void AddMessageArrivedEventHandler(WorkflowQueuingService handler)
        {
            lock (this.SyncRoot)
            {
                if (this.messageArrivalEventHandlers == null)
                {
                    this.messageArrivalEventHandlers = new List<WorkflowQueuingService>();
                }
                this.messageArrivalEventHandlers.Add(handler);
            }
        }

        private void ApplyChangesFrom(EventQueueState srcPendingQueueState, Dictionary<IComparable, EventQueueState> srcPersistedQueueStates)
        {
            lock (this.SyncRoot)
            {
                Dictionary<IComparable, EventQueueState> dictionary = new Dictionary<IComparable, EventQueueState>();
                foreach (KeyValuePair<IComparable, EventQueueState> pair in srcPersistedQueueStates)
                {
                    if (pair.Value.Transactional)
                    {
                        if (this.persistedQueueStates.ContainsKey(pair.Key))
                        {
                            EventQueueState state = this.persistedQueueStates[pair.Key];
                            if (!state.Dirty)
                            {
                                throw new QueueException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.QueueBusyException, new object[] { pair.Key }), MessageQueueErrorCode.QueueNotAvailable);
                            }
                        }
                        dictionary.Add(pair.Key, pair.Value);
                    }
                }
                foreach (KeyValuePair<IComparable, EventQueueState> pair2 in dictionary)
                {
                    this.persistedQueueStates[pair2.Key] = pair2.Value;
                }
                this.pendingQueueState.CopyFrom(srcPendingQueueState);
            }
        }

        internal void Complete(bool commitSucceeded)
        {
            if (commitSucceeded)
            {
                this.rootQueuingService.ApplyChangesFrom(this.pendingQueueState, this.persistedQueueStates);
            }
            this.UnSubscribeFromRootMessageDelivery();
        }

        public WorkflowQueue CreateWorkflowQueue(IComparable queueName, bool transactional)
        {
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            lock (this.SyncRoot)
            {
                if ((this.rootQueuingService != null) && !transactional)
                {
                    return this.rootQueuingService.CreateWorkflowQueue(queueName, false);
                }
                this.NewQueue(queueName, true, transactional);
                return new WorkflowQueue(this, queueName);
            }
        }

        public void DeleteWorkflowQueue(IComparable queueName)
        {
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            lock (this.SyncRoot)
            {
                if ((this.rootQueuingService != null) && !this.IsTransactionalQueue(queueName))
                {
                    this.rootQueuingService.DeleteWorkflowQueue(queueName);
                }
                else
                {
                    Queue messages = this.GetEventQueueState(queueName).Messages;
                    Queue queue2 = this.pendingQueueState.Messages;
                    while (messages.Count != 0)
                    {
                        queue2.Enqueue(messages.Dequeue());
                    }
                    WorkflowTrace.Runtime.TraceInformation("Queuing Service: Deleting Queue with ID {0} for {1}", new object[] { queueName.GetHashCode(), queueName });
                    this.persistedQueueStates.Remove(queueName);
                }
            }
        }

        internal object DequeueEvent(IComparable queueName)
        {
            object obj2;
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            lock (this.SyncRoot)
            {
                if ((this.rootQueuingService != null) && !this.IsTransactionalQueue(queueName))
                {
                    return this.rootQueuingService.DequeueEvent(queueName);
                }
                EventQueueState eventQueueState = this.GetEventQueueState(queueName);
                if (eventQueueState.Messages.Count != 0)
                {
                    obj2 = eventQueueState.Messages.Dequeue();
                }
                else
                {
                    object[] args = new object[] { MessageQueueErrorCode.MessageNotFound, queueName };
                    throw new QueueException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.EventQueueException, args), MessageQueueErrorCode.MessageNotFound);
                }
            }
            return obj2;
        }

        internal void EnqueueEvent(IComparable queueName, object item)
        {
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            lock (this.SyncRoot)
            {
                if ((this.rootQueuingService != null) && !this.IsTransactionalQueue(queueName))
                {
                    this.rootQueuingService.EnqueueEvent(queueName, item);
                }
                else
                {
                    EventQueueState queue = this.GetQueue(queueName);
                    if (!queue.Enabled)
                    {
                        throw new QueueException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.QueueNotEnabled, new object[] { queueName }), MessageQueueErrorCode.QueueNotAvailable);
                    }
                    queue.Messages.Enqueue(item);
                    WorkflowTrace.Runtime.TraceInformation("Queuing Service: Enqueue item Queue ID {0} for {1}", new object[] { queueName.GetHashCode(), queueName });
                    for (int i = 0; (this.messageArrivalEventHandlers != null) && (i < this.messageArrivalEventHandlers.Count); i++)
                    {
                        this.messageArrivalEventHandlers[i].OnItemEnqueued(queueName, item);
                    }
                    this.NotifyExternalSubscribers(queueName, queue, item);
                }
            }
        }

        public bool Exists(IComparable queueName)
        {
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            lock (this.SyncRoot)
            {
                if ((this.rootQueuingService != null) && !this.IsTransactionalQueue(queueName))
                {
                    return this.rootQueuingService.Exists(queueName);
                }
                return this.persistedQueueStates.ContainsKey(queueName);
            }
        }

        private EventQueueState GetEventQueueState(IComparable queueName)
        {
            EventQueueState queue = this.GetQueue(queueName);
            if (queue.Dirty)
            {
                throw new QueueException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.QueueBusyException, new object[] { queueName }), MessageQueueErrorCode.QueueNotAvailable);
            }
            return queue;
        }

        internal EventQueueState GetQueue(IComparable queueID)
        {
            EventQueueState state;
            if (this.persistedQueueStates.TryGetValue(queueID, out state))
            {
                state.queueName = queueID;
                return state;
            }
            object[] args = new object[] { MessageQueueErrorCode.QueueNotFound, queueID };
            throw new QueueException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.EventQueueException, args), MessageQueueErrorCode.QueueNotFound);
        }

        internal EventQueueState GetQueueState(IComparable eventType)
        {
            lock (this.SyncRoot)
            {
                return this.GetQueue(eventType);
            }
        }

        public WorkflowQueue GetWorkflowQueue(IComparable queueName)
        {
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            lock (this.SyncRoot)
            {
                if ((this.rootQueuingService != null) && !this.IsTransactionalQueue(queueName))
                {
                    return this.rootQueuingService.GetWorkflowQueue(queueName);
                }
                this.GetEventQueueState(queueName);
                return new WorkflowQueue(this, queueName);
            }
        }

        private bool IsNestedListenersExist(IComparable queueName)
        {
            for (int i = 0; (this.messageArrivalEventHandlers != null) && (i < this.messageArrivalEventHandlers.Count); i++)
            {
                WorkflowQueuingService service = this.messageArrivalEventHandlers[i];
                EventQueueState state = null;
                if (service.persistedQueueStates.TryGetValue(queueName, out state) && (state.AsynchronousListeners.Count != 0))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsTransactionalQueue(IComparable queueName)
        {
            if (this.persistedQueueStates.ContainsKey(queueName))
            {
                return true;
            }
            EventQueueState copyFromState = this.rootQueuingService.MarkQueueDirtyIfTransactional(queueName);
            if (copyFromState != null)
            {
                EventQueueState state2 = new EventQueueState();
                state2.CopyFrom(copyFromState);
                this.persistedQueueStates.Add(queueName, state2);
                return true;
            }
            return false;
        }

        private EventQueueState MarkQueueDirtyIfTransactional(IComparable queueName)
        {
            lock (this.SyncRoot)
            {
                if (!this.persistedQueueStates.ContainsKey(queueName))
                {
                    return null;
                }
                EventQueueState queue = this.GetQueue(queueName);
                if (!queue.Transactional)
                {
                    return null;
                }
                if (!queue.Dirty)
                {
                    queue.Dirty = true;
                    if (this.dirtyQueues == null)
                    {
                        this.dirtyQueues = new List<IComparable>();
                    }
                    this.dirtyQueues.Add(queueName);
                }
                return queue;
            }
        }

        internal void MoveAllMessagesToPendingQueue()
        {
            lock (this.SyncRoot)
            {
                Queue messages = this.pendingQueueState.Messages;
                foreach (EventQueueState state in this.persistedQueueStates.Values)
                {
                    Queue queue2 = state.Messages;
                    while (queue2.Count != 0)
                    {
                        messages.Enqueue(queue2.Dequeue());
                    }
                }
            }
        }

        private void NewQueue(IComparable queueID, bool enabled, bool transactional)
        {
            WorkflowTrace.Runtime.TraceInformation("Queuing Service: Creating new Queue with ID {0} for {1}", new object[] { queueID.GetHashCode(), queueID });
            if (this.persistedQueueStates.ContainsKey(queueID))
            {
                object[] args = new object[] { MessageQueueErrorCode.QueueExists, queueID };
                throw new QueueException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.EventQueueException, args), MessageQueueErrorCode.QueueExists);
            }
            EventQueueState state = new EventQueueState {
                Enabled = enabled,
                queueName = queueID,
                Transactional = transactional
            };
            this.persistedQueueStates.Add(queueID, state);
        }

        internal void NotifyAsynchronousSubscribers(IComparable queueName, EventQueueState qState, int numberOfNotification)
        {
            for (int i = 0; i < numberOfNotification; i++)
            {
                QueueEventArgs e = new QueueEventArgs(queueName);
                lock (this.SyncRoot)
                {
                    foreach (ActivityExecutorDelegateInfo<QueueEventArgs> info in qState.AsynchronousListeners)
                    {
                        Activity contextActivityForId = this.rootWorkflowExecutor.GetContextActivityForId(info.ContextId);
                        info.InvokeDelegate(contextActivityForId, e, false);
                        WorkflowTrace.Runtime.TraceInformation("Queuing Service: Notifying async subscriber on queue:'{0}' activity:{1}", new object[] { queueName.ToString(), info.ActivityQualifiedName });
                    }
                }
            }
        }

        private void NotifyExternalSubscribers(IComparable queueName, EventQueueState qState, object eventInstance)
        {
            this.NotifySynchronousSubscribers(queueName, qState, eventInstance);
            this.NotifyAsynchronousSubscribers(queueName, qState, 1);
        }

        private void NotifySynchronousSubscribers(IComparable queueName, EventQueueState qState, object eventInstance)
        {
            QueueEventArgs e = new QueueEventArgs(queueName);
            for (int i = 0; i < qState.SynchronousListeners.Count; i++)
            {
                if (qState.SynchronousListeners[i].HandlerDelegate != null)
                {
                    qState.SynchronousListeners[i].HandlerDelegate(new WorkflowQueue(this, queueName), e);
                }
                else
                {
                    qState.SynchronousListeners[i].EventListener.OnEvent(new WorkflowQueue(this, queueName), e);
                }
            }
        }

        private void OnItemEnqueued(IComparable queueName, object item)
        {
            if (this.persistedQueueStates.ContainsKey(queueName))
            {
                EventQueueState queue = this.GetQueue(queueName);
                if (!queue.Enabled)
                {
                    object[] args = new object[] { MessageQueueErrorCode.QueueNotFound, queueName };
                    throw new QueueException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.EventQueueException, args), MessageQueueErrorCode.QueueNotAvailable);
                }
                queue.Messages.Enqueue(item);
                this.NotifyExternalSubscribers(queueName, queue, item);
            }
        }

        private void OnItemSafeEnqueued(IComparable queueName, object item)
        {
            if (this.persistedQueueStates.ContainsKey(queueName))
            {
                EventQueueState queue = this.GetQueue(queueName);
                if (!queue.Enabled)
                {
                    object[] args = new object[] { MessageQueueErrorCode.QueueNotFound, queueName };
                    throw new QueueException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.EventQueueException, args), MessageQueueErrorCode.QueueNotAvailable);
                }
                queue.Messages.Enqueue(item);
                this.NotifySynchronousSubscribers(queueName, queue, item);
            }
        }

        internal object Peek(IComparable queueName)
        {
            object obj2;
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            lock (this.SyncRoot)
            {
                if ((this.rootQueuingService != null) && !this.IsTransactionalQueue(queueName))
                {
                    return this.rootQueuingService.Peek(queueName);
                }
                EventQueueState eventQueueState = this.GetEventQueueState(queueName);
                if (eventQueueState.Messages.Count != 0)
                {
                    obj2 = eventQueueState.Messages.Peek();
                }
                else
                {
                    object[] args = new object[] { MessageQueueErrorCode.MessageNotFound, queueName };
                    throw new QueueException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.EventQueueException, args), MessageQueueErrorCode.MessageNotFound);
                }
            }
            return obj2;
        }

        internal void PostPersist(bool isPersistSuccessful)
        {
            if (!isPersistSuccessful)
            {
                TransactionalProperties properties = this.rootWorkflowExecutor.CurrentAtomicActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty) as TransactionalProperties;
                this.pendingQueueState = this.pendingQueueStateSnapshot;
                this.persistedQueueStates = this.persistedQueueStatesSnapshot;
                this.rootWorkflowExecutor.RootActivity.SetValue(RootPersistedQueueStatesProperty, this.persistedQueueStatesSnapshot);
                this.rootWorkflowExecutor.RootActivity.SetValue(PendingMessagesProperty, this.pendingQueueStateSnapshot.Messages);
                properties.LocalQueuingService.SubscribeForRootMessageDelivery();
            }
            this.persistedQueueStatesSnapshot = null;
            this.pendingQueueStateSnapshot = null;
        }

        internal void PrePersist()
        {
            if (this.rootWorkflowExecutor.CurrentAtomicActivity != null)
            {
                TransactionalProperties properties = this.rootWorkflowExecutor.CurrentAtomicActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty) as TransactionalProperties;
                this.persistedQueueStatesSnapshot = new Dictionary<IComparable, EventQueueState>();
                foreach (KeyValuePair<IComparable, EventQueueState> pair in this.persistedQueueStates)
                {
                    EventQueueState state = new EventQueueState();
                    state.CopyFrom(pair.Value);
                    this.persistedQueueStatesSnapshot.Add(pair.Key, state);
                }
                this.pendingQueueStateSnapshot = new EventQueueState();
                this.pendingQueueStateSnapshot.CopyFrom(this.pendingQueueState);
                properties.LocalQueuingService.Complete(true);
            }
        }

        internal void ProcessesQueuedAsynchronousEvents()
        {
            Queue messages = this.GetQueue("*PendingNotifications").Messages;
            while (messages.Count > 0)
            {
                KeyValuePair<IComparable, EventQueueState> pair = (KeyValuePair<IComparable, EventQueueState>) messages.Dequeue();
                WorkflowTrace.Runtime.TraceInformation("Queuing Service: Processing delayed message notification '{0}'", new object[] { pair.Key.ToString() });
                for (int i = 0; (this.messageArrivalEventHandlers != null) && (i < this.messageArrivalEventHandlers.Count); i++)
                {
                    WorkflowQueuingService service = this.messageArrivalEventHandlers[i];
                    if (service.persistedQueueStates.ContainsKey(pair.Key))
                    {
                        EventQueueState queue = service.GetQueue(pair.Key);
                        if (queue.Enabled)
                        {
                            service.NotifyAsynchronousSubscribers(pair.Key, queue, 1);
                        }
                    }
                }
                this.NotifyAsynchronousSubscribers(pair.Key, pair.Value, 1);
            }
        }

        private bool QueueAsynchronousEvent(IComparable queueName, EventQueueState qState)
        {
            if ((qState.AsynchronousListeners.Count == 0) && !this.IsNestedListenersExist(queueName))
            {
                return false;
            }
            Queue messages = this.GetQueue("*PendingNotifications").Messages;
            messages.Enqueue(new KeyValuePair<IComparable, EventQueueState>(queueName, qState));
            WorkflowTrace.Runtime.TraceInformation("Queuing Service: Queued delayed message notification for '{0}'", new object[] { queueName.ToString() });
            return (messages.Count == 1);
        }

        private void RemoveMessageArrivedEventHandler(WorkflowQueuingService handler)
        {
            lock (this.SyncRoot)
            {
                if (this.messageArrivalEventHandlers != null)
                {
                    this.messageArrivalEventHandlers.Remove(handler);
                }
                if (this.dirtyQueues != null)
                {
                    foreach (IComparable comparable in this.dirtyQueues)
                    {
                        this.GetQueue(comparable).Dirty = false;
                    }
                }
            }
        }

        internal bool SafeEnqueueEvent(IComparable queueName, object item)
        {
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            lock (this.SyncRoot)
            {
                if ((this.rootQueuingService != null) && !this.IsTransactionalQueue(queueName))
                {
                    return this.rootQueuingService.SafeEnqueueEvent(queueName, item);
                }
                EventQueueState queue = this.GetQueue(queueName);
                if (!queue.Enabled)
                {
                    throw new QueueException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.QueueNotEnabled, new object[] { queueName }), MessageQueueErrorCode.QueueNotAvailable);
                }
                queue.Messages.Enqueue(item);
                WorkflowTrace.Runtime.TraceInformation("Queuing Service: Enqueue item Queue ID {0} for {1}", new object[] { queueName.GetHashCode(), queueName });
                for (int i = 0; (this.messageArrivalEventHandlers != null) && (i < this.messageArrivalEventHandlers.Count); i++)
                {
                    this.messageArrivalEventHandlers[i].OnItemSafeEnqueued(queueName, item);
                }
                this.NotifySynchronousSubscribers(queueName, queue, item);
                return this.QueueAsynchronousEvent(queueName, queue);
            }
        }

        private void SubscribeForRootMessageDelivery()
        {
            if (this.rootQueuingService != null)
            {
                this.rootQueuingService.AddMessageArrivedEventHandler(this);
            }
        }

        private void UnSubscribeFromRootMessageDelivery()
        {
            if (this.rootQueuingService != null)
            {
                this.rootQueuingService.RemoveMessageArrivedEventHandler(this);
            }
        }

        internal Activity CallingActivity
        {
            get
            {
                if (this.rootQueuingService != null)
                {
                    return this.rootQueuingService.CallingActivity;
                }
                return this.caller;
            }
            set
            {
                if (this.rootQueuingService != null)
                {
                    this.rootQueuingService.CallingActivity = value;
                }
                this.caller = value;
            }
        }

        internal IEnumerable<IComparable> QueueNames
        {
            get
            {
                List<IComparable> list = new List<IComparable>(this.persistedQueueStates.Keys);
                foreach (IComparable comparable in list)
                {
                    if ((comparable is string) && (((string) comparable) == "*PendingNotifications"))
                    {
                        list.Remove(comparable);
                        return list;
                    }
                }
                return list;
            }
        }

        internal object SyncRoot
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.syncRoot;
            }
        }
    }
}

