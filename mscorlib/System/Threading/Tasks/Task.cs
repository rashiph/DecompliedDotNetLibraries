namespace System.Threading.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Eventing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [DebuggerTypeProxy(typeof(SystemThreadingTasks_TaskDebugView)), DebuggerDisplay("Id = {Id}, Status = {Status}, Method = {DebuggerDisplayMethodDescription}"), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class Task : IThreadPoolWorkItem, IAsyncResult, IDisposable
    {
        internal static int CANCELLATION_REQUESTED = 1;
        internal object m_action;
        internal ExecutionContext m_capturedContext;
        private volatile ManualResetEventSlim m_completionEvent;
        internal volatile ContingentProperties m_contingentProperties;
        internal readonly Task m_parent;
        internal volatile int m_stateFlags;
        internal object m_stateObject;
        private int m_taskId;
        internal TaskScheduler m_taskScheduler;
        private const int OptionsMask = 0xffff;
        internal static Func<ContingentProperties> s_contingentPropertyCreator = new Func<ContingentProperties>(Task.ContingentPropertyCreator);
        [SecurityCritical]
        private static ContextCallback s_ecCallback = new ContextCallback(Task.ExecutionContextCallback);
        private static TaskFactory s_factory = new TaskFactory();
        private static Predicate<Task> s_IsExceptionObservedByParentPredicate = t => t.IsExceptionObservedByParent;
        internal static Action<object> s_taskCancelCallback = new Action<object>(Task.TaskCancelCallback);
        internal static int s_taskIdCounter;
        internal const int TASK_STATE_CANCELED = 0x400000;
        internal const int TASK_STATE_CANCELLATIONACKNOWLEDGED = 0x100000;
        internal const int TASK_STATE_COMPLETION_RESERVED = 0x4000000;
        internal const int TASK_STATE_DELEGATE_INVOKED = 0x20000;
        internal const int TASK_STATE_DISPOSED = 0x40000;
        internal const int TASK_STATE_EXCEPTIONOBSERVEDBYPARENT = 0x80000;
        internal const int TASK_STATE_FAULTED = 0x200000;
        internal const int TASK_STATE_RAN_TO_COMPLETION = 0x1000000;
        internal const int TASK_STATE_STARTED = 0x10000;
        internal const int TASK_STATE_THREAD_WAS_ABORTED = 0x8000000;
        internal const int TASK_STATE_WAITING_ON_CHILDREN = 0x800000;
        internal const int TASK_STATE_WAITINGFORACTIVATION = 0x2000000;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task(Action action) : this(action, null, InternalCurrent, System.Threading.CancellationToken.None, TaskCreationOptions.None, InternalTaskOptions.None, null)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            this.PossiblyCaptureContext(ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task(Action action, System.Threading.CancellationToken cancellationToken) : this(action, null, InternalCurrent, cancellationToken, TaskCreationOptions.None, InternalTaskOptions.None, null)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            this.PossiblyCaptureContext(ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task(Action action, TaskCreationOptions creationOptions) : this(action, null, InternalCurrent, System.Threading.CancellationToken.None, creationOptions, InternalTaskOptions.None, null)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            this.PossiblyCaptureContext(ref lookForMyCaller);
        }

        internal Task(bool canceled, TaskCreationOptions creationOptions)
        {
            int num = (int) creationOptions;
            if (canceled)
            {
                this.m_stateFlags = 0x500000 | num;
            }
            else
            {
                this.m_stateFlags = 0x1000000 | num;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task(Action<object> action, object state) : this(action, state, InternalCurrent, System.Threading.CancellationToken.None, TaskCreationOptions.None, InternalTaskOptions.None, null)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            this.PossiblyCaptureContext(ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task(Action action, System.Threading.CancellationToken cancellationToken, TaskCreationOptions creationOptions) : this(action, null, InternalCurrent, cancellationToken, creationOptions, InternalTaskOptions.None, null)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            this.PossiblyCaptureContext(ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task(Action<object> action, object state, System.Threading.CancellationToken cancellationToken) : this(action, state, InternalCurrent, cancellationToken, TaskCreationOptions.None, InternalTaskOptions.None, null)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            this.PossiblyCaptureContext(ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task(Action<object> action, object state, TaskCreationOptions creationOptions) : this(action, state, InternalCurrent, System.Threading.CancellationToken.None, creationOptions, InternalTaskOptions.None, null)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            this.PossiblyCaptureContext(ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task(Action<object> action, object state, System.Threading.CancellationToken cancellationToken, TaskCreationOptions creationOptions) : this(action, state, InternalCurrent, cancellationToken, creationOptions, InternalTaskOptions.None, null)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            this.PossiblyCaptureContext(ref lookForMyCaller);
        }

        internal Task(object state, System.Threading.CancellationToken cancelationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, bool promiseStyle)
        {
            if ((creationOptions & ~TaskCreationOptions.AttachedToParent) != TaskCreationOptions.None)
            {
                throw new ArgumentOutOfRangeException("creationOptions");
            }
            if ((internalOptions & ~InternalTaskOptions.PromiseTask) != InternalTaskOptions.None)
            {
                throw new ArgumentOutOfRangeException("internalOptions", Environment.GetResourceString("Task_PromiseCtor_IllegalInternalOptions"));
            }
            if ((creationOptions & TaskCreationOptions.AttachedToParent) != TaskCreationOptions.None)
            {
                this.m_parent = InternalCurrent;
            }
            this.TaskConstructorCore(null, state, cancelationToken, creationOptions, internalOptions, TaskScheduler.Current);
        }

        internal Task(object action, object state, Task parent, System.Threading.CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            if (((creationOptions & TaskCreationOptions.AttachedToParent) != TaskCreationOptions.None) || ((internalOptions & InternalTaskOptions.SelfReplicating) != InternalTaskOptions.None))
            {
                this.m_parent = parent;
            }
            this.TaskConstructorCore(action, state, cancellationToken, creationOptions, internalOptions, scheduler);
        }

        internal Task(Action<object> action, object state, Task parent, System.Threading.CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler, ref StackCrawlMark stackMark) : this(action, state, parent, cancellationToken, creationOptions, internalOptions, scheduler)
        {
            this.PossiblyCaptureContext(ref stackMark);
        }

        [CompilerGenerated]
        private static bool <.cctor>b__0(Task t)
        {
            return t.IsExceptionObservedByParent;
        }

        internal void AddCompletionAction(Action<Task> action)
        {
            if (!this.IsCompleted)
            {
                LazyInitializer.EnsureInitialized<ContingentProperties>(ref this.m_contingentProperties, s_contingentPropertyCreator);
                TaskContinuation item = new TaskContinuation(action);
                if (this.m_contingentProperties.m_continuations == null)
                {
                    Interlocked.CompareExchange<List<TaskContinuation>>(ref this.m_contingentProperties.m_continuations, new List<TaskContinuation>(), null);
                }
                lock (this.m_contingentProperties)
                {
                    if (!this.IsCompleted)
                    {
                        this.m_contingentProperties.m_continuations.Add(item);
                        return;
                    }
                }
            }
            action(this);
        }

        internal void AddException(object exceptionObject)
        {
            LazyInitializer.EnsureInitialized<ContingentProperties>(ref this.m_contingentProperties, s_contingentPropertyCreator);
            if (this.m_contingentProperties.m_exceptionsHolder == null)
            {
                TaskExceptionHolder holder = new TaskExceptionHolder(this);
                if (Interlocked.CompareExchange<TaskExceptionHolder>(ref this.m_contingentProperties.m_exceptionsHolder, holder, null) != null)
                {
                    holder.MarkAsHandled(false);
                }
            }
            lock (this.m_contingentProperties)
            {
                this.m_contingentProperties.m_exceptionsHolder.Add(exceptionObject);
            }
        }

        internal static void AddExceptionsForCompletedTask(ref List<System.Exception> exceptions, Task t)
        {
            AggregateException exception = t.GetExceptions(true);
            if (exception != null)
            {
                t.UpdateExceptionObservedStatus();
                if (exceptions == null)
                {
                    exceptions = new List<System.Exception>(exception.InnerExceptions.Count);
                }
                exceptions.AddRange(exception.InnerExceptions);
            }
        }

        internal void AddExceptionsFromChildren()
        {
            List<Task> list = (this.m_contingentProperties != null) ? this.m_contingentProperties.m_exceptionalChildren : null;
            if (list != null)
            {
                lock (list)
                {
                    foreach (Task task in list)
                    {
                        if (task.IsFaulted && !task.IsExceptionObservedByParent)
                        {
                            TaskExceptionHolder exceptionsHolder = task.m_contingentProperties.m_exceptionsHolder;
                            this.AddException(exceptionsHolder.CreateExceptionObject(false, null));
                        }
                    }
                }
                this.m_contingentProperties.m_exceptionalChildren = null;
            }
        }

        internal void AddNewChild()
        {
            LazyInitializer.EnsureInitialized<ContingentProperties>(ref this.m_contingentProperties, s_contingentPropertyCreator);
            if ((this.m_contingentProperties.m_completionCountdown == 1) && !this.IsSelfReplicatingRoot)
            {
                this.m_contingentProperties.m_completionCountdown++;
            }
            else
            {
                Interlocked.Increment(ref this.m_contingentProperties.m_completionCountdown);
            }
        }

        internal bool AtomicStateUpdate(int newBits, int illegalBits)
        {
            int oldFlags = 0;
            return this.AtomicStateUpdate(newBits, illegalBits, ref oldFlags);
        }

        internal bool AtomicStateUpdate(int newBits, int illegalBits, ref int oldFlags)
        {
            SpinWait wait = new SpinWait();
            while (true)
            {
                oldFlags = this.m_stateFlags;
                if ((oldFlags & illegalBits) != 0)
                {
                    return false;
                }
                if (Interlocked.CompareExchange(ref this.m_stateFlags, oldFlags | newBits, oldFlags) == oldFlags)
                {
                    return true;
                }
                wait.SpinOnce();
            }
        }

        internal void CancellationCleanupLogic()
        {
            Interlocked.Exchange(ref this.m_stateFlags, this.m_stateFlags | 0x400000);
            this.SetCompleted();
            this.FinishStageThree();
        }

        private static ContingentProperties ContingentPropertyCreator()
        {
            return new ContingentProperties();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWith(Action<Task> continuationAction)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith(continuationAction, TaskScheduler.Current, System.Threading.CancellationToken.None, TaskContinuationOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith<TResult>(continuationFunction, TaskScheduler.Current, System.Threading.CancellationToken.None, TaskContinuationOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWith(Action<Task> continuationAction, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith(continuationAction, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWith(Action<Task> continuationAction, TaskContinuationOptions continuationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith(continuationAction, TaskScheduler.Current, System.Threading.CancellationToken.None, continuationOptions, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWith(Action<Task> continuationAction, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith(continuationAction, scheduler, System.Threading.CancellationToken.None, TaskContinuationOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith<TResult>(continuationFunction, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction, TaskContinuationOptions continuationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith<TResult>(continuationFunction, TaskScheduler.Current, System.Threading.CancellationToken.None, continuationOptions, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith<TResult>(continuationFunction, scheduler, System.Threading.CancellationToken.None, TaskContinuationOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWith(Action<Task> continuationAction, System.Threading.CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith(continuationAction, scheduler, cancellationToken, continuationOptions, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction, System.Threading.CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith<TResult>(continuationFunction, scheduler, cancellationToken, continuationOptions, ref lookForMyCaller);
        }

        private Task ContinueWith(Action<Task> continuationAction, TaskScheduler scheduler, System.Threading.CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, ref StackCrawlMark stackMark)
        {
            TaskCreationOptions options;
            InternalTaskOptions options2;
            this.ThrowIfDisposed();
            if (continuationAction == null)
            {
                throw new ArgumentNullException("continuationAction");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            CreationOptionsFromContinuationOptions(continuationOptions, out options, out options2);
            Task thisTask = this;
            Task continuationTask = new Task(delegate (object obj) {
                continuationAction(thisTask);
            }, null, InternalCurrent, cancellationToken, options, options2, null, ref stackMark);
            this.ContinueWithCore(continuationTask, scheduler, continuationOptions);
            return continuationTask;
        }

        private Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction, TaskScheduler scheduler, System.Threading.CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, ref StackCrawlMark stackMark)
        {
            TaskCreationOptions options;
            InternalTaskOptions options2;
            this.ThrowIfDisposed();
            if (continuationFunction == null)
            {
                throw new ArgumentNullException("continuationFunction");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            CreationOptionsFromContinuationOptions(continuationOptions, out options, out options2);
            Task thisTask = this;
            Task<TResult> continuationTask = new Task<TResult>(() => continuationFunction(thisTask), InternalCurrent, cancellationToken, options, options2, null, ref stackMark);
            this.ContinueWithCore(continuationTask, scheduler, continuationOptions);
            return continuationTask;
        }

        internal void ContinueWithCore(Task continuationTask, TaskScheduler scheduler, TaskContinuationOptions options)
        {
            if (!continuationTask.IsCompleted)
            {
                TaskContinuation item = new TaskContinuation(continuationTask, scheduler, options);
                if (!this.IsCompleted)
                {
                    LazyInitializer.EnsureInitialized<ContingentProperties>(ref this.m_contingentProperties, s_contingentPropertyCreator);
                    if (this.m_contingentProperties.m_continuations == null)
                    {
                        Interlocked.CompareExchange<List<TaskContinuation>>(ref this.m_contingentProperties.m_continuations, new List<TaskContinuation>(), null);
                    }
                    lock (this.m_contingentProperties)
                    {
                        if (!this.IsCompleted)
                        {
                            this.m_contingentProperties.m_continuations.Add(item);
                            return;
                        }
                    }
                }
                item.Run(this, true);
            }
        }

        internal bool ContinueWithIsRightKind(TaskContinuationOptions options)
        {
            if (this.IsFaulted)
            {
                return ((options & TaskContinuationOptions.NotOnFaulted) == TaskContinuationOptions.None);
            }
            if (this.IsCanceled)
            {
                return ((options & TaskContinuationOptions.NotOnCanceled) == TaskContinuationOptions.None);
            }
            return ((options & TaskContinuationOptions.NotOnRanToCompletion) == TaskContinuationOptions.None);
        }

        internal virtual Task CreateReplicaTask(Action<object> taskReplicaDelegate, object stateObject, Task parentTask, TaskScheduler taskScheduler, TaskCreationOptions creationOptionsForReplica, InternalTaskOptions internalOptionsForReplica)
        {
            return new Task(taskReplicaDelegate, stateObject, parentTask, System.Threading.CancellationToken.None, creationOptionsForReplica, internalOptionsForReplica, parentTask.ExecutingTaskScheduler);
        }

        internal static void CreationOptionsFromContinuationOptions(TaskContinuationOptions continuationOptions, out TaskCreationOptions creationOptions, out InternalTaskOptions internalOptions)
        {
            TaskContinuationOptions options = TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.NotOnRanToCompletion;
            TaskContinuationOptions options2 = TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.LongRunning | TaskContinuationOptions.PreferFairness;
            TaskContinuationOptions options3 = TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.LongRunning;
            if ((continuationOptions & options3) == options3)
            {
                throw new ArgumentOutOfRangeException("continuationOptions", Environment.GetResourceString("Task_ContinueWith_ESandLR"));
            }
            if ((continuationOptions & ~((options2 | options) | TaskContinuationOptions.ExecuteSynchronously)) != TaskContinuationOptions.None)
            {
                throw new ArgumentOutOfRangeException("continuationOptions");
            }
            if ((continuationOptions & options) == options)
            {
                throw new ArgumentOutOfRangeException("continuationOptions", Environment.GetResourceString("Task_ContinueWith_NotOnAnything"));
            }
            creationOptions = ((TaskCreationOptions) continuationOptions) & ((TaskCreationOptions) options2);
            internalOptions = InternalTaskOptions.ContinuationTask;
        }

        internal void DeregisterCancellationCallback()
        {
            if ((this.m_contingentProperties != null) && (this.m_contingentProperties.m_cancellationRegistration != null))
            {
                try
                {
                    this.m_contingentProperties.m_cancellationRegistration.Value.Dispose();
                }
                catch (ObjectDisposedException)
                {
                }
                this.m_contingentProperties.m_cancellationRegistration = null;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!this.IsCompleted)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Task_Dispose_NotCompleted"));
                }
                ManualResetEventSlim completionEvent = this.m_completionEvent;
                if (completionEvent != null)
                {
                    if (!completionEvent.IsSet)
                    {
                        completionEvent.Set();
                    }
                    completionEvent.Dispose();
                    this.m_completionEvent = null;
                }
            }
            this.m_stateFlags |= 0x40000;
        }

        internal void DisregardChild()
        {
            Interlocked.Decrement(ref this.m_contingentProperties.m_completionCountdown);
        }

        private void Execute()
        {
            if (this.IsSelfReplicatingRoot)
            {
                ExecuteSelfReplicating(this);
            }
            else
            {
                try
                {
                    this.InnerInvoke();
                }
                catch (ThreadAbortException exception)
                {
                    if (!this.IsChildReplica)
                    {
                        this.HandleException(exception);
                        this.FinishThreadAbortedTask(true, true);
                    }
                }
                catch (System.Exception exception2)
                {
                    this.HandleException(exception2);
                }
            }
        }

        [SecuritySafeCritical]
        internal bool ExecuteEntry(bool bPreventDoubleExecution)
        {
            if (bPreventDoubleExecution || ((this.Options & 0x800) != TaskCreationOptions.None))
            {
                int oldFlags = 0;
                if (!this.AtomicStateUpdate(0x20000, 0x20000, ref oldFlags) && ((oldFlags & 0x400000) == 0))
                {
                    return false;
                }
            }
            else
            {
                this.m_stateFlags |= 0x20000;
            }
            if (!this.IsCancellationRequested && !this.IsCanceled)
            {
                this.ExecuteWithThreadLocal(ref ThreadLocals.s_currentTask);
            }
            else if (!this.IsCanceled && ((Interlocked.Exchange(ref this.m_stateFlags, this.m_stateFlags | 0x400000) & 0x400000) == 0))
            {
                this.CancellationCleanupLogic();
            }
            return true;
        }

        private static void ExecuteSelfReplicating(Task root)
        {
            TaskCreationOptions creationOptionsForReplicas = root.CreationOptions | TaskCreationOptions.AttachedToParent;
            InternalTaskOptions internalOptionsForReplicas = InternalTaskOptions.QueuedByRuntime | InternalTaskOptions.SelfReplicating | InternalTaskOptions.ChildReplica;
            bool replicasAreQuitting = false;
            Action<object> taskReplicaDelegate = null;
            taskReplicaDelegate = delegate {
                Task childTask = InternalCurrent;
                Task handedOverChildReplica = childTask.HandedOverChildReplica;
                if (handedOverChildReplica == null)
                {
                    if (!root.ShouldReplicate())
                    {
                        return;
                    }
                    if (replicasAreQuitting)
                    {
                        return;
                    }
                    ExecutionContext capturedContext = root.m_capturedContext;
                    handedOverChildReplica = root.CreateReplicaTask(taskReplicaDelegate, root.m_stateObject, root, root.ExecutingTaskScheduler, creationOptionsForReplicas, internalOptionsForReplicas);
                    handedOverChildReplica.m_capturedContext = (capturedContext == null) ? null : capturedContext.CreateCopy();
                    handedOverChildReplica.ScheduleAndStart(false);
                }
                try
                {
                    root.InnerInvokeWithArg(childTask);
                }
                catch (System.Exception exception)
                {
                    root.HandleException(exception);
                    if (exception is ThreadAbortException)
                    {
                        childTask.FinishThreadAbortedTask(false, true);
                    }
                }
                object savedStateForNextReplica = childTask.SavedStateForNextReplica;
                if (savedStateForNextReplica != null)
                {
                    Task task3 = root.CreateReplicaTask(taskReplicaDelegate, root.m_stateObject, root, root.ExecutingTaskScheduler, creationOptionsForReplicas, internalOptionsForReplicas);
                    ExecutionContext context2 = root.m_capturedContext;
                    task3.m_capturedContext = (context2 == null) ? null : context2.CreateCopy();
                    task3.HandedOverChildReplica = handedOverChildReplica;
                    task3.SavedStateFromPreviousReplica = savedStateForNextReplica;
                    task3.ScheduleAndStart(false);
                }
                else
                {
                    replicasAreQuitting = true;
                    try
                    {
                        handedOverChildReplica.InternalCancel(true);
                    }
                    catch (System.Exception exception2)
                    {
                        root.HandleException(exception2);
                    }
                }
            };
            taskReplicaDelegate(null);
        }

        [SecurityCritical]
        private void ExecuteWithThreadLocal(ref Task currentTaskSlot)
        {
            Task task = currentTaskSlot;
            if (TplEtwProvider.Log.IsEnabled(EventLevel.Verbose, ~EventKeywords.None))
            {
                if (task != null)
                {
                    TplEtwProvider.Log.TaskStarted(task.m_taskScheduler.Id, task.Id, this.Id);
                }
                else
                {
                    TplEtwProvider.Log.TaskStarted(TaskScheduler.Current.Id, 0, this.Id);
                }
            }
            try
            {
                currentTaskSlot = this;
                ExecutionContext capturedContext = this.m_capturedContext;
                if (capturedContext == null)
                {
                    this.Execute();
                }
                else
                {
                    if (this.IsSelfReplicatingRoot || this.IsChildReplica)
                    {
                        this.m_capturedContext = capturedContext.CreateCopy();
                    }
                    ExecutionContext.Run(capturedContext, s_ecCallback, this, true);
                }
                this.Finish(true);
            }
            finally
            {
                currentTaskSlot = task;
            }
            if (TplEtwProvider.Log.IsEnabled(EventLevel.Verbose, ~EventKeywords.None))
            {
                if (task != null)
                {
                    TplEtwProvider.Log.TaskCompleted(task.m_taskScheduler.Id, task.Id, this.Id, this.IsFaulted);
                }
                else
                {
                    TplEtwProvider.Log.TaskCompleted(TaskScheduler.Current.Id, 0, this.Id, this.IsFaulted);
                }
            }
        }

        [SecurityCritical]
        private static void ExecutionContextCallback(object obj)
        {
            (obj as Task).Execute();
        }

        internal static void FastWaitAll(Task[] tasks)
        {
            List<System.Exception> exceptions = null;
            TaskScheduler current = TaskScheduler.Current;
            object threadStatics = current.GetThreadStatics();
            for (int i = tasks.Length - 1; i >= 0; i--)
            {
                if (!tasks[i].IsCompleted)
                {
                    tasks[i].WrappedTryRunInline(current, threadStatics);
                }
            }
            for (int j = tasks.Length - 1; j >= 0; j--)
            {
                tasks[j].CompletedEvent.Wait();
                AddExceptionsForCompletedTask(ref exceptions, tasks[j]);
            }
            if (exceptions != null)
            {
                throw new AggregateException(exceptions);
            }
        }

        internal void Finish(bool bUserDelegateExecuted)
        {
            if (!bUserDelegateExecuted)
            {
                this.FinishStageTwo();
            }
            else
            {
                ContingentProperties contingentProperties = this.m_contingentProperties;
                if (((contingentProperties == null) || ((contingentProperties.m_completionCountdown == 1) && !this.IsSelfReplicatingRoot)) || (Interlocked.Decrement(ref contingentProperties.m_completionCountdown) == 0))
                {
                    this.FinishStageTwo();
                }
                else
                {
                    this.AtomicStateUpdate(0x800000, 0x1600000);
                }
            }
            List<Task> list = (this.m_contingentProperties != null) ? this.m_contingentProperties.m_exceptionalChildren : null;
            if (list != null)
            {
                lock (list)
                {
                    list.RemoveAll(s_IsExceptionObservedByParentPredicate);
                }
            }
        }

        private void FinishContinuations()
        {
            List<TaskContinuation> list = (this.m_contingentProperties == null) ? null : this.m_contingentProperties.m_continuations;
            if (list != null)
            {
                lock (this.m_contingentProperties)
                {
                }
                bool bCanInlineContinuationTask = ((this.m_stateFlags & 0x8000000) == 0) && (Thread.CurrentThread.ThreadState != ThreadState.AbortRequested);
                int num = -1;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    TaskContinuation continuation = list[i];
                    if ((continuation.m_taskScheduler != null) && ((continuation.m_options & TaskContinuationOptions.ExecuteSynchronously) == TaskContinuationOptions.None))
                    {
                        continuation.Run(this, bCanInlineContinuationTask);
                    }
                    else
                    {
                        num = i;
                    }
                }
                if (num > -1)
                {
                    for (int j = num; j < list.Count; j++)
                    {
                        TaskContinuation continuation2 = list[j];
                        if ((continuation2.m_taskScheduler == null) || ((continuation2.m_options & TaskContinuationOptions.ExecuteSynchronously) != TaskContinuationOptions.None))
                        {
                            continuation2.Run(this, bCanInlineContinuationTask);
                        }
                    }
                }
                this.m_contingentProperties.m_continuations = null;
            }
        }

        private void FinishStageThree()
        {
            if ((this.m_parent != null) && (((this.m_stateFlags & 0xffff) & 4) != 0))
            {
                this.m_parent.ProcessChildCompletion(this);
            }
            this.FinishContinuations();
            this.m_action = null;
        }

        internal void FinishStageTwo()
        {
            int num;
            this.AddExceptionsFromChildren();
            if (this.ExceptionRecorded)
            {
                num = 0x200000;
            }
            else if (this.IsCancellationRequested && this.IsCancellationAcknowledged)
            {
                num = 0x400000;
            }
            else
            {
                num = 0x1000000;
            }
            Interlocked.Exchange(ref this.m_stateFlags, this.m_stateFlags | num);
            this.SetCompleted();
            this.DeregisterCancellationCallback();
            this.FinishStageThree();
        }

        internal void FinishThreadAbortedTask(bool bTAEAddedToExceptionHolder, bool delegateRan)
        {
            if (bTAEAddedToExceptionHolder)
            {
                this.m_contingentProperties.m_exceptionsHolder.MarkAsHandled(false);
            }
            if (this.AtomicStateUpdate(0x8000000, 0x9600000))
            {
                this.Finish(delegateRan);
            }
        }

        private AggregateException GetExceptions(bool includeTaskCanceledExceptions)
        {
            System.Exception includeThisException = null;
            if (includeTaskCanceledExceptions && this.IsCanceled)
            {
                includeThisException = new TaskCanceledException(this);
            }
            if (this.ExceptionRecorded)
            {
                return this.m_contingentProperties.m_exceptionsHolder.CreateExceptionObject(false, includeThisException);
            }
            if (includeThisException != null)
            {
                return new AggregateException(new System.Exception[] { includeThisException });
            }
            return null;
        }

        private void HandleException(System.Exception unhandledException)
        {
            OperationCanceledException exception = unhandledException as OperationCanceledException;
            if (((exception != null) && this.IsCancellationRequested) && (this.m_contingentProperties.m_cancellationToken == exception.CancellationToken))
            {
                this.SetCancellationAcknowledged();
            }
            else
            {
                this.AddException(unhandledException);
            }
        }

        internal void InnerInvoke()
        {
            Action action = this.m_action as Action;
            if (action != null)
            {
                action();
            }
            else
            {
                Action<object> action2 = this.m_action as Action<object>;
                action2(this.m_stateObject);
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        internal void InnerInvokeWithArg(Task childTask)
        {
            this.InnerInvoke();
        }

        [SecuritySafeCritical]
        internal bool InternalCancel(bool bCancelNonExecutingOnly)
        {
            this.ThrowIfDisposed();
            bool flag = false;
            bool flag2 = false;
            TaskSchedulerException exception = null;
            if ((this.m_stateFlags & 0x10000) != 0)
            {
                TaskScheduler taskScheduler = this.m_taskScheduler;
                try
                {
                    flag = (taskScheduler != null) && taskScheduler.TryDequeue(this);
                }
                catch (System.Exception exception2)
                {
                    if (!(exception2 is ThreadAbortException))
                    {
                        exception = new TaskSchedulerException(exception2);
                    }
                }
                bool flag3 = ((taskScheduler != null) && taskScheduler.RequiresAtomicStartTransition) || ((this.Options & 0x800) != TaskCreationOptions.None);
                if ((!flag && bCancelNonExecutingOnly) && flag3)
                {
                    flag2 = this.AtomicStateUpdate(0x400000, 0x420000);
                }
            }
            if ((!bCancelNonExecutingOnly || flag) || flag2)
            {
                this.RecordInternalCancellationRequest();
                if (flag)
                {
                    flag2 = this.AtomicStateUpdate(0x400000, 0x420000);
                }
                else if (!flag2 && ((this.m_stateFlags & 0x10000) == 0))
                {
                    flag2 = this.AtomicStateUpdate(0x400000, 0x1630000);
                }
                if (flag2)
                {
                    this.CancellationCleanupLogic();
                }
            }
            if (exception != null)
            {
                throw exception;
            }
            return flag2;
        }

        [SecuritySafeCritical]
        internal void InternalRunSynchronously(TaskScheduler scheduler)
        {
            this.ThrowIfDisposed();
            if ((this.Options & 0x200) != TaskCreationOptions.None)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Task_RunSynchronously_Continuation"));
            }
            if (this.IsCompleted)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Task_RunSynchronously_TaskCompleted"));
            }
            if (this.m_action == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Task_RunSynchronously_Promise"));
            }
            if (Interlocked.CompareExchange<TaskScheduler>(ref this.m_taskScheduler, scheduler, null) != null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Task_RunSynchronously_AlreadyStarted"));
            }
            if (this.MarkStarted())
            {
                bool flag = false;
                try
                {
                    if (!scheduler.TryRunInline(this, false))
                    {
                        scheduler.QueueTask(this);
                        flag = true;
                    }
                    if (!this.IsCompleted)
                    {
                        this.CompletedEvent.Wait();
                    }
                    return;
                }
                catch (System.Exception exception)
                {
                    if (!flag && !(exception is ThreadAbortException))
                    {
                        TaskSchedulerException exceptionObject = new TaskSchedulerException(exception);
                        this.AddException(exceptionObject);
                        this.Finish(false);
                        this.m_contingentProperties.m_exceptionsHolder.MarkAsHandled(false);
                        throw exceptionObject;
                    }
                    throw;
                }
            }
            throw new InvalidOperationException(Environment.GetResourceString("Task_RunSynchronously_TaskCompleted"));
        }

        internal static Task InternalStartNew(Task creatingTask, object action, object state, TaskScheduler scheduler, TaskCreationOptions options, InternalTaskOptions internalOptions, ExecutionContext context)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            Task task = new Task(action, state, creatingTask, System.Threading.CancellationToken.None, options, internalOptions | InternalTaskOptions.QueuedByRuntime, scheduler) {
                m_capturedContext = context
            };
            task.ScheduleAndStart(false);
            return task;
        }

        internal static Task InternalStartNew(Task creatingTask, object action, object state, System.Threading.CancellationToken cancellationToken, TaskScheduler scheduler, TaskCreationOptions options, InternalTaskOptions internalOptions, ref StackCrawlMark stackMark)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            Task task = new Task(action, state, creatingTask, cancellationToken, options, internalOptions | InternalTaskOptions.QueuedByRuntime, scheduler);
            task.PossiblyCaptureContext(ref stackMark);
            task.ScheduleAndStart(false);
            return task;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        internal bool InternalWait(int millisecondsTimeout, System.Threading.CancellationToken cancellationToken)
        {
            if (TplEtwProvider.Log.IsEnabled(EventLevel.Verbose, ~EventKeywords.None))
            {
                Task internalCurrent = InternalCurrent;
                TplEtwProvider.Log.TaskWaitBegin((internalCurrent != null) ? internalCurrent.m_taskScheduler.Id : TaskScheduler.Current.Id, (internalCurrent != null) ? internalCurrent.Id : 0, this.Id);
            }
            bool isCompleted = this.IsCompleted;
            if (!isCompleted)
            {
                if (((millisecondsTimeout == -1) && !cancellationToken.CanBeCanceled) && (this.WrappedTryRunInline() && this.IsCompleted))
                {
                    isCompleted = true;
                }
                else
                {
                    isCompleted = this.CompletedEvent.Wait(millisecondsTimeout, cancellationToken);
                }
            }
            if (TplEtwProvider.Log.IsEnabled(EventLevel.Verbose, ~EventKeywords.None))
            {
                Task task2 = InternalCurrent;
                TplEtwProvider.Log.TaskWaitEnd((task2 != null) ? task2.m_taskScheduler.Id : TaskScheduler.Current.Id, (task2 != null) ? task2.Id : 0, this.Id);
            }
            return isCompleted;
        }

        internal bool MarkStarted()
        {
            return this.AtomicStateUpdate(0x10000, 0x410000);
        }

        [SecuritySafeCritical]
        internal void PossiblyCaptureContext(ref StackCrawlMark stackMark)
        {
            if (!ExecutionContext.IsFlowSuppressed())
            {
                this.m_capturedContext = ExecutionContext.Capture(ref stackMark, ExecutionContext.CaptureOptions.OptimizeDefaultCase | ExecutionContext.CaptureOptions.IgnoreSyncCtx);
            }
        }

        internal void ProcessChildCompletion(Task childTask)
        {
            if (childTask.IsFaulted && !childTask.IsExceptionObservedByParent)
            {
                if (this.m_contingentProperties.m_exceptionalChildren == null)
                {
                    Interlocked.CompareExchange<List<Task>>(ref this.m_contingentProperties.m_exceptionalChildren, new List<Task>(), null);
                }
                List<Task> exceptionalChildren = this.m_contingentProperties.m_exceptionalChildren;
                if (exceptionalChildren != null)
                {
                    lock (exceptionalChildren)
                    {
                        exceptionalChildren.Add(childTask);
                    }
                }
            }
            if (Interlocked.Decrement(ref this.m_contingentProperties.m_completionCountdown) == 0)
            {
                this.FinishStageTwo();
            }
        }

        internal void RecordInternalCancellationRequest()
        {
            LazyInitializer.EnsureInitialized<ContingentProperties>(ref this.m_contingentProperties, s_contingentPropertyCreator);
            this.m_contingentProperties.m_internalCancellationRequested = CANCELLATION_REQUESTED;
        }

        public void RunSynchronously()
        {
            this.InternalRunSynchronously(TaskScheduler.Current);
        }

        public void RunSynchronously(TaskScheduler scheduler)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            this.InternalRunSynchronously(scheduler);
        }

        [SecuritySafeCritical]
        internal void ScheduleAndStart(bool needsProtection)
        {
            if (needsProtection)
            {
                if (!this.MarkStarted())
                {
                    return;
                }
            }
            else
            {
                this.m_stateFlags |= 0x10000;
            }
            try
            {
                this.m_taskScheduler.QueueTask(this);
            }
            catch (ThreadAbortException exception)
            {
                this.AddException(exception);
                this.FinishThreadAbortedTask(true, false);
            }
            catch (System.Exception exception2)
            {
                TaskSchedulerException exceptionObject = new TaskSchedulerException(exception2);
                this.AddException(exceptionObject);
                this.Finish(false);
                if ((this.Options & 0x200) == TaskCreationOptions.None)
                {
                    this.m_contingentProperties.m_exceptionsHolder.MarkAsHandled(false);
                }
                throw exceptionObject;
            }
        }

        private void SetCancellationAcknowledged()
        {
            this.m_stateFlags |= 0x100000;
        }

        private void SetCompleted()
        {
            ManualResetEventSlim completionEvent = this.m_completionEvent;
            if (completionEvent != null)
            {
                completionEvent.Set();
            }
        }

        internal virtual bool ShouldReplicate()
        {
            return true;
        }

        public void Start()
        {
            this.Start(TaskScheduler.Current);
        }

        public void Start(TaskScheduler scheduler)
        {
            this.ThrowIfDisposed();
            if (this.IsCompleted)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Task_Start_TaskCompleted"));
            }
            if (this.m_action == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Task_Start_NullAction"));
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            if ((this.Options & 0x200) != TaskCreationOptions.None)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Task_Start_ContinuationTask"));
            }
            if (Interlocked.CompareExchange<TaskScheduler>(ref this.m_taskScheduler, scheduler, null) != null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Task_Start_AlreadyStarted"));
            }
            this.ScheduleAndStart(true);
        }

        [SecurityCritical]
        void IThreadPoolWorkItem.ExecuteWorkItem()
        {
            this.ExecuteEntry(false);
        }

        [SecurityCritical]
        void IThreadPoolWorkItem.MarkAborted(ThreadAbortException tae)
        {
            if (!this.IsCompleted)
            {
                this.HandleException(tae);
                this.FinishThreadAbortedTask(true, false);
            }
        }

        private static void TaskCancelCallback(object o)
        {
            ((Task) o).InternalCancel(false);
        }

        internal void TaskConstructorCore(object action, object state, System.Threading.CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler)
        {
            this.m_action = action;
            this.m_stateObject = state;
            this.m_taskScheduler = scheduler;
            if ((creationOptions & ~(TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness)) != TaskCreationOptions.None)
            {
                throw new ArgumentOutOfRangeException("creationOptions");
            }
            if ((internalOptions & ~(InternalTaskOptions.QueuedByRuntime | InternalTaskOptions.SelfReplicating | InternalTaskOptions.PromiseTask | InternalTaskOptions.ContinuationTask | InternalTaskOptions.ChildReplica)) != InternalTaskOptions.None)
            {
                throw new ArgumentOutOfRangeException("internalOptions", Environment.GetResourceString("Task_ctor_IllegalInternalOptions"));
            }
            if (((creationOptions & TaskCreationOptions.LongRunning) != TaskCreationOptions.None) && ((internalOptions & InternalTaskOptions.SelfReplicating) != InternalTaskOptions.None))
            {
                throw new InvalidOperationException(Environment.GetResourceString("Task_ctor_LRandSR"));
            }
            this.m_stateFlags = (volatile int) (creationOptions | ((TaskCreationOptions) ((int) internalOptions)));
            if ((this.m_action == null) || ((internalOptions & InternalTaskOptions.ContinuationTask) != InternalTaskOptions.None))
            {
                this.m_stateFlags |= 0x2000000;
            }
            if ((this.m_parent != null) && ((creationOptions & TaskCreationOptions.AttachedToParent) != TaskCreationOptions.None))
            {
                this.m_parent.AddNewChild();
            }
            if (cancellationToken.CanBeCanceled)
            {
                LazyInitializer.EnsureInitialized<ContingentProperties>(ref this.m_contingentProperties, s_contingentPropertyCreator);
                this.m_contingentProperties.m_cancellationToken = cancellationToken;
                try
                {
                    cancellationToken.ThrowIfSourceDisposed();
                    if ((internalOptions & (InternalTaskOptions.QueuedByRuntime | InternalTaskOptions.PromiseTask)) == InternalTaskOptions.None)
                    {
                        CancellationTokenRegistration registration = cancellationToken.InternalRegisterWithoutEC(s_taskCancelCallback, this);
                        this.m_contingentProperties.m_cancellationRegistration = new Shared<CancellationTokenRegistration>(registration);
                    }
                }
                catch
                {
                    if ((this.m_parent != null) && ((creationOptions & TaskCreationOptions.AttachedToParent) != TaskCreationOptions.None))
                    {
                        this.m_parent.DisregardChild();
                    }
                    throw;
                }
            }
        }

        internal void ThrowIfDisposed()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("Task_ThrowIfDisposed"));
            }
        }

        internal void ThrowIfExceptional(bool includeTaskCanceledExceptions)
        {
            System.Exception exceptions = this.GetExceptions(includeTaskCanceledExceptions);
            if (exceptions != null)
            {
                this.UpdateExceptionObservedStatus();
                throw exceptions;
            }
        }

        internal void UpdateExceptionObservedStatus()
        {
            if (((this.Options & TaskCreationOptions.AttachedToParent) != TaskCreationOptions.None) && (InternalCurrent == this.m_parent))
            {
                this.m_stateFlags |= 0x80000;
            }
        }

        public void Wait()
        {
            this.Wait(-1, System.Threading.CancellationToken.None);
        }

        public bool Wait(int millisecondsTimeout)
        {
            return this.Wait(millisecondsTimeout, System.Threading.CancellationToken.None);
        }

        public void Wait(System.Threading.CancellationToken cancellationToken)
        {
            this.Wait(-1, cancellationToken);
        }

        public bool Wait(TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            return this.Wait((int) totalMilliseconds, System.Threading.CancellationToken.None);
        }

        public bool Wait(int millisecondsTimeout, System.Threading.CancellationToken cancellationToken)
        {
            this.ThrowIfDisposed();
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout");
            }
            if (!this.CompletedSuccessfully)
            {
                if (!this.InternalWait(millisecondsTimeout, cancellationToken))
                {
                    return false;
                }
                this.ThrowIfExceptional(true);
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void WaitAll(params Task[] tasks)
        {
            WaitAll(tasks, -1);
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static bool WaitAll(Task[] tasks, int millisecondsTimeout)
        {
            return WaitAll(tasks, millisecondsTimeout, System.Threading.CancellationToken.None);
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void WaitAll(Task[] tasks, System.Threading.CancellationToken cancellationToken)
        {
            WaitAll(tasks, -1, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static bool WaitAll(Task[] tasks, TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            return WaitAll(tasks, (int) totalMilliseconds);
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static bool WaitAll(Task[] tasks, int millisecondsTimeout, System.Threading.CancellationToken cancellationToken)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            cancellationToken.ThrowIfCancellationRequested();
            List<System.Exception> exceptions = null;
            List<Task> list2 = null;
            bool flag = true;
            Task internalCurrent = InternalCurrent;
            TaskScheduler currentScheduler = (internalCurrent == null) ? TaskScheduler.Default : internalCurrent.ExecutingTaskScheduler;
            object threadStatics = currentScheduler.GetThreadStatics();
            for (int i = tasks.Length - 1; i >= 0; i--)
            {
                Task item = tasks[i];
                if (item == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Task_WaitMulti_NullTask"), "tasks");
                }
                item.ThrowIfDisposed();
                bool isCompleted = item.IsCompleted;
                if (!isCompleted)
                {
                    if ((millisecondsTimeout != -1) || cancellationToken.CanBeCanceled)
                    {
                        if (list2 == null)
                        {
                            list2 = new List<Task>(tasks.Length);
                        }
                        list2.Add(item);
                    }
                    else
                    {
                        isCompleted = item.WrappedTryRunInline(currentScheduler, threadStatics) && item.IsCompleted;
                        if (!isCompleted)
                        {
                            if (list2 == null)
                            {
                                list2 = new List<Task>(tasks.Length);
                            }
                            list2.Add(item);
                        }
                    }
                }
                if (isCompleted)
                {
                    AddExceptionsForCompletedTask(ref exceptions, item);
                }
            }
            if (list2 != null)
            {
                WaitHandle[] waitHandles = new WaitHandle[list2.Count];
                for (int j = 0; j < waitHandles.Length; j++)
                {
                    waitHandles[j] = list2[j].CompletedEvent.WaitHandle;
                }
                flag = WaitAllSTAAnd64Aware(waitHandles, millisecondsTimeout, cancellationToken);
                if (flag)
                {
                    for (int k = 0; k < list2.Count; k++)
                    {
                        AddExceptionsForCompletedTask(ref exceptions, list2[k]);
                    }
                }
                GC.KeepAlive(tasks);
            }
            if (exceptions != null)
            {
                throw new AggregateException(exceptions);
            }
            return flag;
        }

        private static bool WaitAllSTAAnd64Aware(WaitHandle[] waitHandles, int millisecondsTimeout, System.Threading.CancellationToken cancellationToken)
        {
            if ((Thread.CurrentThread.GetApartmentState() == ApartmentState.STA) || cancellationToken.CanBeCanceled)
            {
                WaitHandle[] handleArray = null;
                if (cancellationToken.CanBeCanceled)
                {
                    handleArray = new WaitHandle[2];
                    handleArray[1] = cancellationToken.WaitHandle;
                }
                for (int i = 0; i < waitHandles.Length; i++)
                {
                    long num2 = (millisecondsTimeout == -1) ? 0L : DateTime.UtcNow.Ticks;
                    if (cancellationToken.CanBeCanceled)
                    {
                        handleArray[0] = waitHandles[i];
                        if (WaitHandle.WaitAny(handleArray, millisecondsTimeout, false) == 0x102)
                        {
                            return false;
                        }
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    else if (!waitHandles[i].WaitOne(millisecondsTimeout, false))
                    {
                        return false;
                    }
                    if (millisecondsTimeout != -1)
                    {
                        long num4 = (DateTime.UtcNow.Ticks - num2) / 0x2710L;
                        if ((num4 > 0x7fffffffL) || (num4 > millisecondsTimeout))
                        {
                            return false;
                        }
                        millisecondsTimeout -= (int) num4;
                    }
                }
            }
            else if (waitHandles.Length <= 0x40)
            {
                if (!WaitHandle.WaitAll(waitHandles, millisecondsTimeout, false))
                {
                    return false;
                }
            }
            else
            {
                int num5 = ((waitHandles.Length + 0x40) - 1) / 0x40;
                WaitHandle[] destinationArray = new WaitHandle[0x40];
                long num6 = (millisecondsTimeout == -1) ? 0L : DateTime.UtcNow.Ticks;
                for (int j = 0; j < num5; j++)
                {
                    if ((j == (num5 - 1)) && ((waitHandles.Length % 0x40) != 0))
                    {
                        destinationArray = new WaitHandle[waitHandles.Length % 0x40];
                    }
                    Array.Copy(waitHandles, j * 0x40, destinationArray, 0, destinationArray.Length);
                    if (!WaitHandle.WaitAll(destinationArray, millisecondsTimeout, false))
                    {
                        return false;
                    }
                    if (millisecondsTimeout != -1)
                    {
                        long num8 = (DateTime.UtcNow.Ticks - num6) / 0x2710L;
                        if ((num8 > 0x7fffffffL) || (num8 > millisecondsTimeout))
                        {
                            return false;
                        }
                        millisecondsTimeout -= (int) num8;
                    }
                }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static int WaitAny(params Task[] tasks)
        {
            return WaitAny(tasks, -1);
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static int WaitAny(Task[] tasks, int millisecondsTimeout)
        {
            return WaitAny(tasks, millisecondsTimeout, System.Threading.CancellationToken.None);
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static int WaitAny(Task[] tasks, System.Threading.CancellationToken cancellationToken)
        {
            return WaitAny(tasks, -1, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static int WaitAny(Task[] tasks, TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            return WaitAny(tasks, (int) totalMilliseconds);
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static int WaitAny(Task[] tasks, int millisecondsTimeout, System.Threading.CancellationToken cancellationToken)
        {
            Func<Task, int> continuationFunction = null;
            Task[] tasksLocalCopy2;
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout");
            }
            cancellationToken.ThrowIfCancellationRequested();
            int num = -1;
            int num2 = (cancellationToken.CanBeCanceled ? 1 : 0) + ((Thread.CurrentThread.GetApartmentState() == ApartmentState.STA) ? 1 : 0);
            int num3 = 0x40 - num2;
            int index = 0;
            int num5 = 0;
            if (tasks.Length > num3)
            {
                index = num3 - 1;
                num5 = tasks.Length - index;
            }
            else
            {
                index = tasks.Length;
            }
            for (int j = 0; j < tasks.Length; j++)
            {
                Task task = tasks[j];
                if (task == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Task_WaitMulti_NullTask"), "tasks");
                }
                task.ThrowIfDisposed();
                if (task.IsCompleted && (num == -1))
                {
                    num = j;
                }
            }
            if (num == -1)
            {
                Task[] taskArray = new Task[index];
                tasksLocalCopy2 = (num5 > 0) ? new Task[num5] : null;
                for (int k = 0; k < tasks.Length; k++)
                {
                    Task task2 = tasks[k];
                    if (task2 == null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Task_WaitMulti_NullTask"), "tasks");
                    }
                    if (k < index)
                    {
                        taskArray[k] = task2;
                    }
                    else
                    {
                        tasksLocalCopy2[k - index] = task2;
                    }
                    task2.ThrowIfDisposed();
                    if (task2.IsCompleted && (num == -1))
                    {
                        num = k;
                    }
                }
                if ((num == -1) && (tasks.Length != 0))
                {
                    int num8 = (index + ((num5 > 0) ? 1 : 0)) + (cancellationToken.CanBeCanceled ? 1 : 0);
                    Task<int> task3 = null;
                    WaitHandle[] waitHandles = new WaitHandle[num8];
                    for (int m = 0; m < index; m++)
                    {
                        waitHandles[m] = taskArray[m].CompletedEvent.WaitHandle;
                    }
                    if (num5 > 0)
                    {
                        if (continuationFunction == null)
                        {
                            continuationFunction = delegate (Task antecedent) {
                                for (int n = 0; n < tasksLocalCopy2.Length; n++)
                                {
                                    if (antecedent == tasksLocalCopy2[n])
                                    {
                                        return n;
                                    }
                                }
                                return tasksLocalCopy2.Length;
                            };
                        }
                        task3 = Factory.ContinueWhenAny<int>(tasksLocalCopy2, continuationFunction);
                        waitHandles[index] = task3.CompletedEvent.WaitHandle;
                    }
                    if (cancellationToken.CanBeCanceled)
                    {
                        waitHandles[num8 - 1] = cancellationToken.WaitHandle;
                    }
                    int num10 = WaitHandle.WaitAny(waitHandles, millisecondsTimeout, false);
                    cancellationToken.ThrowIfCancellationRequested();
                    if (num10 != 0x102)
                    {
                        if ((num5 > 0) && (num10 == index))
                        {
                            num10 = index + task3.Result;
                        }
                        num = num10;
                    }
                }
                GC.KeepAlive(tasks);
            }
            return num;
        }

        private bool WrappedTryRunInline()
        {
            bool flag;
            if (this.m_taskScheduler == null)
            {
                return false;
            }
            try
            {
                flag = this.m_taskScheduler.TryRunInline(this, true);
            }
            catch (System.Exception exception)
            {
                if (exception is ThreadAbortException)
                {
                    throw;
                }
                TaskSchedulerException exception2 = new TaskSchedulerException(exception);
                throw exception2;
            }
            return flag;
        }

        private bool WrappedTryRunInline(TaskScheduler currentScheduler, object currentSchedulerStatics)
        {
            bool flag;
            if (this.m_taskScheduler == null)
            {
                return false;
            }
            try
            {
                if (currentScheduler == this.m_taskScheduler)
                {
                    return currentScheduler.TryRunInline(this, true, currentSchedulerStatics);
                }
                flag = this.m_taskScheduler.TryRunInline(this, true);
            }
            catch (System.Exception exception)
            {
                if (exception is ThreadAbortException)
                {
                    throw;
                }
                TaskSchedulerException exception2 = new TaskSchedulerException(exception);
                throw exception2;
            }
            return flag;
        }

        internal int ActiveChildCount
        {
            get
            {
                if (this.m_contingentProperties == null)
                {
                    return 0;
                }
                return (this.m_contingentProperties.m_completionCountdown - 1);
            }
        }

        public object AsyncState
        {
            get
            {
                return this.InternalAsyncState;
            }
        }

        internal System.Threading.CancellationToken CancellationToken
        {
            get
            {
                if (this.m_contingentProperties != null)
                {
                    return this.m_contingentProperties.m_cancellationToken;
                }
                return System.Threading.CancellationToken.None;
            }
        }

        internal ManualResetEventSlim CompletedEvent
        {
            get
            {
                if (this.m_completionEvent == null)
                {
                    bool isCompleted = this.IsCompleted;
                    ManualResetEventSlim slim = new ManualResetEventSlim(isCompleted);
                    if (Interlocked.CompareExchange<ManualResetEventSlim>(ref this.m_completionEvent, slim, null) != null)
                    {
                        slim.Dispose();
                    }
                    else if (!isCompleted && this.IsCompleted)
                    {
                        slim.Set();
                    }
                }
                return this.m_completionEvent;
            }
        }

        internal bool CompletedSuccessfully
        {
            get
            {
                int num = 0x1600000;
                return ((this.m_stateFlags & num) == 0x1000000);
            }
        }

        public TaskCreationOptions CreationOptions
        {
            get
            {
                return (this.Options & -65281);
            }
        }

        public static int? CurrentId
        {
            get
            {
                Task internalCurrent = InternalCurrent;
                if (internalCurrent != null)
                {
                    return new int?(internalCurrent.Id);
                }
                return null;
            }
        }

        internal static StackGuard CurrentStackGuard
        {
            get
            {
                StackGuard guard = ThreadLocals.s_stackGuard;
                if (guard == null)
                {
                    guard = new StackGuard();
                    ThreadLocals.s_stackGuard = guard;
                }
                return guard;
            }
        }

        private string DebuggerDisplayMethodDescription
        {
            get
            {
                Delegate action = (Delegate) this.m_action;
                if (action == null)
                {
                    return "{null}";
                }
                return action.Method.ToString();
            }
        }

        public AggregateException Exception
        {
            get
            {
                AggregateException exceptions = null;
                if (this.IsFaulted)
                {
                    exceptions = this.GetExceptions(false);
                }
                return exceptions;
            }
        }

        internal bool ExceptionRecorded
        {
            get
            {
                return ((this.m_contingentProperties != null) && (this.m_contingentProperties.m_exceptionsHolder != null));
            }
        }

        internal TaskScheduler ExecutingTaskScheduler
        {
            get
            {
                return this.m_taskScheduler;
            }
        }

        public static TaskFactory Factory
        {
            get
            {
                return s_factory;
            }
        }

        internal virtual Task HandedOverChildReplica
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public int Id
        {
            get
            {
                if (this.m_taskId == 0)
                {
                    int num = 0;
                    do
                    {
                        num = Interlocked.Increment(ref s_taskIdCounter);
                    }
                    while (num == 0);
                    Interlocked.CompareExchange(ref this.m_taskId, num, 0);
                }
                return this.m_taskId;
            }
        }

        internal virtual object InternalAsyncState
        {
            get
            {
                return this.m_stateObject;
            }
        }

        internal static Task InternalCurrent
        {
            get
            {
                return ThreadLocals.s_currentTask;
            }
        }

        public bool IsCanceled
        {
            get
            {
                return ((this.m_stateFlags & 0x600000) == 0x400000);
            }
        }

        internal bool IsCancellationAcknowledged
        {
            get
            {
                return ((this.m_stateFlags & 0x100000) != 0);
            }
        }

        internal bool IsCancellationRequested
        {
            get
            {
                return (((this.m_contingentProperties != null) && (this.m_contingentProperties.m_internalCancellationRequested == CANCELLATION_REQUESTED)) || this.CancellationToken.IsCancellationRequested);
            }
        }

        internal bool IsChildReplica
        {
            get
            {
                return ((this.Options & 0x100) != TaskCreationOptions.None);
            }
        }

        public bool IsCompleted
        {
            get
            {
                return ((this.m_stateFlags & 0x1600000) != 0);
            }
        }

        internal bool IsDelegateInvoked
        {
            get
            {
                return ((this.m_stateFlags & 0x20000) != 0);
            }
        }

        internal bool IsDisposed
        {
            get
            {
                return ((this.m_stateFlags & 0x40000) != 0);
            }
        }

        internal bool IsExceptionObservedByParent
        {
            get
            {
                return ((this.m_stateFlags & 0x80000) != 0);
            }
        }

        public bool IsFaulted
        {
            get
            {
                return ((this.m_stateFlags & 0x200000) != 0);
            }
        }

        internal bool IsSelfReplicatingRoot
        {
            get
            {
                return (((this.Options & 0x800) != TaskCreationOptions.None) && ((this.Options & 0x100) == TaskCreationOptions.None));
            }
        }

        internal TaskCreationOptions Options
        {
            get
            {
                return (((TaskCreationOptions) this.m_stateFlags) & ((TaskCreationOptions) 0xffff));
            }
        }

        internal virtual object SavedStateForNextReplica
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        internal virtual object SavedStateFromPreviousReplica
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public TaskStatus Status
        {
            get
            {
                int stateFlags = this.m_stateFlags;
                if ((stateFlags & 0x200000) != 0)
                {
                    return TaskStatus.Faulted;
                }
                if ((stateFlags & 0x400000) != 0)
                {
                    return TaskStatus.Canceled;
                }
                if ((stateFlags & 0x1000000) != 0)
                {
                    return TaskStatus.RanToCompletion;
                }
                if ((stateFlags & 0x800000) != 0)
                {
                    return TaskStatus.WaitingForChildrenToComplete;
                }
                if ((stateFlags & 0x20000) != 0)
                {
                    return TaskStatus.Running;
                }
                if ((stateFlags & 0x10000) != 0)
                {
                    return TaskStatus.WaitingToRun;
                }
                if ((stateFlags & 0x2000000) != 0)
                {
                    return TaskStatus.WaitingForActivation;
                }
                return TaskStatus.Created;
            }
        }

        WaitHandle IAsyncResult.AsyncWaitHandle
        {
            get
            {
                this.ThrowIfDisposed();
                return this.CompletedEvent.WaitHandle;
            }
        }

        bool IAsyncResult.CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        internal class ContingentProperties
        {
            public Shared<CancellationTokenRegistration> m_cancellationRegistration;
            public CancellationToken m_cancellationToken;
            internal volatile int m_completionCountdown = 1;
            public volatile List<Task.TaskContinuation> m_continuations;
            public volatile List<Task> m_exceptionalChildren;
            public volatile TaskExceptionHolder m_exceptionsHolder;
            public volatile int m_internalCancellationRequested;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TaskContinuation
        {
            internal object m_task;
            internal TaskScheduler m_taskScheduler;
            internal TaskContinuationOptions m_options;
            internal TaskContinuation(Task task, TaskScheduler scheduler, TaskContinuationOptions options)
            {
                this.m_task = task;
                this.m_taskScheduler = scheduler;
                this.m_options = options;
            }

            internal TaskContinuation(Action<Task> action)
            {
                this.m_task = action;
                this.m_taskScheduler = null;
                this.m_options = TaskContinuationOptions.None;
            }

            [SecuritySafeCritical]
            internal void Run(Task completedTask, bool bCanInlineContinuationTask)
            {
                Task task = this.m_task as Task;
                if (task != null)
                {
                    if (!completedTask.ContinueWithIsRightKind(this.m_options))
                    {
                        task.InternalCancel(false);
                    }
                    else
                    {
                        task.m_taskScheduler = this.m_taskScheduler;
                        if (bCanInlineContinuationTask && ((this.m_options & TaskContinuationOptions.ExecuteSynchronously) != TaskContinuationOptions.None))
                        {
                            if (task.MarkStarted())
                            {
                                try
                                {
                                    if (!this.m_taskScheduler.TryRunInline(task, false))
                                    {
                                        this.m_taskScheduler.QueueTask(task);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    if (!(exception is ThreadAbortException) || ((task.m_stateFlags & 0x8000000) == 0))
                                    {
                                        TaskSchedulerException exceptionObject = new TaskSchedulerException(exception);
                                        task.AddException(exceptionObject);
                                        task.Finish(false);
                                    }
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                task.ScheduleAndStart(true);
                            }
                            catch (TaskSchedulerException)
                            {
                            }
                        }
                    }
                }
                else
                {
                    Action<Task> action = this.m_task as Action<Task>;
                    action(completedTask);
                }
            }
        }

        private static class ThreadLocals
        {
            [ThreadStatic]
            internal static Task s_currentTask;
            [ThreadStatic]
            internal static StackGuard s_stackGuard;
        }
    }
}

