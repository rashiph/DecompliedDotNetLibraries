namespace System.Threading.Tasks
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Threading;

    [DebuggerTypeProxy(typeof(SystemThreadingTasks_FutureDebugView<>)), DebuggerDisplay("Id = {Id}, Status = {Status}, Method = {DebuggerDisplayMethodDescription}, Result = {DebuggerDisplayResultDescription}"), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class Task<TResult> : Task
    {
        private object m_futureState;
        private TResult m_result;
        internal bool m_resultWasSet;
        private object m_valueSelector;
        private static TaskFactory<TResult> s_Factory;

        static Task()
        {
            Task<TResult>.s_Factory = new TaskFactory<TResult>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task(Func<TResult> function) : this(function, Task.InternalCurrent, CancellationToken.None, TaskCreationOptions.None, InternalTaskOptions.None, null)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            base.PossiblyCaptureContext(ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task(Func<TResult> function, CancellationToken cancellationToken) : this(function, Task.InternalCurrent, cancellationToken, TaskCreationOptions.None, InternalTaskOptions.None, null)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            base.PossiblyCaptureContext(ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task(Func<TResult> function, TaskCreationOptions creationOptions) : this(function, Task.InternalCurrent, CancellationToken.None, creationOptions, InternalTaskOptions.None, null)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            base.PossiblyCaptureContext(ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task(Func<object, TResult> function, object state) : this(function, state, Task.InternalCurrent, CancellationToken.None, TaskCreationOptions.None, InternalTaskOptions.None, null)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            base.PossiblyCaptureContext(ref lookForMyCaller);
        }

        internal Task(bool canceled, TResult result, TaskCreationOptions creationOptions) : base(canceled, creationOptions)
        {
            if (!canceled)
            {
                this.m_result = result;
                this.m_resultWasSet = true;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task(Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : this(function, Task.InternalCurrent, cancellationToken, creationOptions, InternalTaskOptions.None, null)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            base.PossiblyCaptureContext(ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task(Func<object, TResult> function, object state, CancellationToken cancellationToken) : this(function, state, Task.InternalCurrent, cancellationToken, TaskCreationOptions.None, InternalTaskOptions.None, null)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            base.PossiblyCaptureContext(ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task(Func<object, TResult> function, object state, TaskCreationOptions creationOptions) : this(function, state, Task.InternalCurrent, CancellationToken.None, creationOptions, InternalTaskOptions.None, null)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            base.PossiblyCaptureContext(ref lookForMyCaller);
        }

        internal Task(object state, CancellationToken cancellationToken, TaskCreationOptions options, InternalTaskOptions internalOptions) : base(null, cancellationToken, options, internalOptions, true)
        {
            this.m_valueSelector = null;
            this.m_futureState = state;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task(Func<object, TResult> function, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : this(function, state, Task.InternalCurrent, cancellationToken, creationOptions, InternalTaskOptions.None, null)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            base.PossiblyCaptureContext(ref lookForMyCaller);
        }

        internal Task(Func<TResult> valueSelector, Task parent, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler) : base((valueSelector != null) ? new Action<object>(Task<TResult>.InvokeFuture) : null, null, parent, cancellationToken, creationOptions, internalOptions, scheduler)
        {
            if ((internalOptions & InternalTaskOptions.SelfReplicating) != InternalTaskOptions.None)
            {
                throw new ArgumentOutOfRangeException("creationOptions", Environment.GetResourceString("TaskT_ctor_SelfReplicating"));
            }
            this.m_valueSelector = valueSelector;
            base.m_stateObject = this;
        }

        internal Task(Func<TResult> valueSelector, Task parent, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler, ref StackCrawlMark stackMark) : this(valueSelector, parent, cancellationToken, creationOptions, internalOptions, scheduler)
        {
            base.PossiblyCaptureContext(ref stackMark);
        }

        internal Task(Func<object, TResult> valueSelector, object state, Task parent, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler) : base((valueSelector != null) ? new Action<object>(Task<TResult>.InvokeFuture) : null, null, parent, cancellationToken, creationOptions, internalOptions, scheduler)
        {
            if ((internalOptions & InternalTaskOptions.SelfReplicating) != InternalTaskOptions.None)
            {
                throw new ArgumentOutOfRangeException("creationOptions", Environment.GetResourceString("TaskT_ctor_SelfReplicating"));
            }
            this.m_valueSelector = valueSelector;
            base.m_stateObject = this;
            this.m_futureState = state;
        }

        internal Task(Func<object, TResult> valueSelector, object state, Task parent, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler, ref StackCrawlMark stackMark) : this(valueSelector, state, parent, cancellationToken, creationOptions, internalOptions, scheduler)
        {
            base.PossiblyCaptureContext(ref stackMark);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWith(Action<Task<TResult>> continuationAction)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith(continuationAction, TaskScheduler.Current, CancellationToken.None, TaskContinuationOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith<TNewResult>(continuationFunction, TaskScheduler.Current, CancellationToken.None, TaskContinuationOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWith(Action<Task<TResult>> continuationAction, CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith(continuationAction, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWith(Action<Task<TResult>> continuationAction, TaskContinuationOptions continuationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith(continuationAction, TaskScheduler.Current, CancellationToken.None, continuationOptions, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWith(Action<Task<TResult>> continuationAction, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith(continuationAction, scheduler, CancellationToken.None, TaskContinuationOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction, CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith<TNewResult>(continuationFunction, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction, TaskContinuationOptions continuationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith<TNewResult>(continuationFunction, TaskScheduler.Current, CancellationToken.None, continuationOptions, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith<TNewResult>(continuationFunction, scheduler, CancellationToken.None, TaskContinuationOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWith(Action<Task<TResult>> continuationAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith(continuationAction, scheduler, cancellationToken, continuationOptions, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWith<TNewResult>(continuationFunction, scheduler, cancellationToken, continuationOptions, ref lookForMyCaller);
        }

        internal Task ContinueWith(Action<Task<TResult>> continuationAction, TaskScheduler scheduler, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, ref StackCrawlMark stackMark)
        {
            TaskCreationOptions options;
            InternalTaskOptions options2;
            base.ThrowIfDisposed();
            if (continuationAction == null)
            {
                throw new ArgumentNullException("continuationAction");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            Task.CreationOptionsFromContinuationOptions(continuationOptions, out options, out options2);
            Task continuationTask = new Task(delegate (object obj) {
                continuationAction((Task<TResult>) this);
            }, null, Task.InternalCurrent, cancellationToken, options, options2, null, ref stackMark);
            base.ContinueWithCore(continuationTask, scheduler, continuationOptions);
            return continuationTask;
        }

        internal Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction, TaskScheduler scheduler, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, ref StackCrawlMark stackMark)
        {
            TaskCreationOptions options;
            InternalTaskOptions options2;
            base.ThrowIfDisposed();
            if (continuationFunction == null)
            {
                throw new ArgumentNullException("continuationFunction");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            Task.CreationOptionsFromContinuationOptions(continuationOptions, out options, out options2);
            Task<TNewResult> continuationTask = new Task<TNewResult>((Func<TNewResult>) (() => this.continuationFunction(this.thisFuture)), Task.InternalCurrent, cancellationToken, options, options2, null, ref stackMark);
            base.ContinueWithCore(continuationTask, scheduler, continuationOptions);
            return continuationTask;
        }

        private static void InvokeFuture(object futureAsObj)
        {
            Task<TResult> task = (Task<TResult>) futureAsObj;
            Func<TResult> valueSelector = task.m_valueSelector as Func<TResult>;
            try
            {
                if (valueSelector != null)
                {
                    task.m_result = valueSelector();
                }
                else
                {
                    task.m_result = ((Func<object, TResult>) task.m_valueSelector)(task.m_futureState);
                }
                task.m_resultWasSet = true;
            }
            finally
            {
                task.m_valueSelector = null;
            }
        }

        internal static Task<TResult> StartNew(Task parent, Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler, ref StackCrawlMark stackMark)
        {
            if (function == null)
            {
                throw new ArgumentNullException("function");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            if ((internalOptions & InternalTaskOptions.SelfReplicating) != InternalTaskOptions.None)
            {
                throw new ArgumentOutOfRangeException("creationOptions", Environment.GetResourceString("TaskT_ctor_SelfReplicating"));
            }
            Task<TResult> task = new Task<TResult>(function, parent, cancellationToken, creationOptions, internalOptions | InternalTaskOptions.QueuedByRuntime, scheduler, ref stackMark);
            task.ScheduleAndStart(false);
            return task;
        }

        internal static Task<TResult> StartNew(Task parent, Func<object, TResult> function, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler, ref StackCrawlMark stackMark)
        {
            if (function == null)
            {
                throw new ArgumentNullException("function");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            if ((internalOptions & InternalTaskOptions.SelfReplicating) != InternalTaskOptions.None)
            {
                throw new ArgumentOutOfRangeException("creationOptions", Environment.GetResourceString("TaskT_ctor_SelfReplicating"));
            }
            Task<TResult> task = new Task<TResult>(function, state, parent, cancellationToken, creationOptions, internalOptions | InternalTaskOptions.QueuedByRuntime, scheduler, ref stackMark);
            task.ScheduleAndStart(false);
            return task;
        }

        internal bool TrySetException(object exceptionObject)
        {
            base.ThrowIfDisposed();
            bool flag = false;
            LazyInitializer.EnsureInitialized<Task.ContingentProperties>(ref this.m_contingentProperties, Task.s_contingentPropertyCreator);
            if (base.AtomicStateUpdate(0x4000000, 0x5600000))
            {
                base.AddException(exceptionObject);
                base.Finish(false);
                flag = true;
            }
            return flag;
        }

        internal bool TrySetResult(TResult result)
        {
            base.ThrowIfDisposed();
            if (base.AtomicStateUpdate(0x4000000, 0x5600000))
            {
                this.m_result = result;
                this.m_resultWasSet = true;
                base.Finish(false);
                return true;
            }
            return false;
        }

        private string DebuggerDisplayMethodDescription
        {
            get
            {
                Delegate valueSelector = (Delegate) this.m_valueSelector;
                if (valueSelector == null)
                {
                    return "{null}";
                }
                return valueSelector.Method.ToString();
            }
        }

        private string DebuggerDisplayResultDescription
        {
            get
            {
                if (!this.m_resultWasSet)
                {
                    return Environment.GetResourceString("TaskT_DebuggerNoResult");
                }
                return (this.m_result);
            }
        }

        public static TaskFactory<TResult> Factory
        {
            get
            {
                return Task<TResult>.s_Factory;
            }
        }

        internal override object InternalAsyncState
        {
            get
            {
                return this.m_futureState;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public TResult Result
        {
            get
            {
                if (!base.IsCompleted)
                {
                    Debugger.NotifyOfCrossThreadDependency();
                    base.Wait();
                }
                base.ThrowIfExceptional(!this.m_resultWasSet);
                return this.m_result;
            }
            internal set
            {
                if (this.m_valueSelector != null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("TaskT_SetResult_HasAnInitializer"));
                }
                if (!this.TrySetResult(value))
                {
                    throw new InvalidOperationException(Environment.GetResourceString("TaskT_TransitionToFinal_AlreadyCompleted"));
                }
            }
        }
    }
}

