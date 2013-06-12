namespace System.Threading.Tasks
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class TaskFactory
    {
        private System.Threading.CancellationToken m_defaultCancellationToken;
        private TaskContinuationOptions m_defaultContinuationOptions;
        private TaskCreationOptions m_defaultCreationOptions;
        private TaskScheduler m_defaultScheduler;

        public TaskFactory() : this(System.Threading.CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, null)
        {
        }

        public TaskFactory(System.Threading.CancellationToken cancellationToken) : this(cancellationToken, TaskCreationOptions.None, TaskContinuationOptions.None, null)
        {
        }

        public TaskFactory(TaskScheduler scheduler) : this(System.Threading.CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, scheduler)
        {
        }

        public TaskFactory(TaskCreationOptions creationOptions, TaskContinuationOptions continuationOptions) : this(System.Threading.CancellationToken.None, creationOptions, continuationOptions, null)
        {
        }

        public TaskFactory(System.Threading.CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            this.m_defaultCancellationToken = cancellationToken;
            this.m_defaultScheduler = scheduler;
            this.m_defaultCreationOptions = creationOptions;
            this.m_defaultContinuationOptions = continuationOptions;
            CheckCreationOptions(this.m_defaultCreationOptions);
            CheckMultiTaskContinuationOptions(this.m_defaultContinuationOptions);
        }

        internal static void CheckCreationOptions(TaskCreationOptions creationOptions)
        {
            if ((creationOptions & ~(TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness)) != TaskCreationOptions.None)
            {
                throw new ArgumentOutOfRangeException("creationOptions");
            }
        }

        internal static void CheckFromAsyncOptions(TaskCreationOptions creationOptions, bool hasBeginMethod)
        {
            if (hasBeginMethod)
            {
                if ((creationOptions & TaskCreationOptions.LongRunning) != TaskCreationOptions.None)
                {
                    throw new ArgumentOutOfRangeException("creationOptions", Environment.GetResourceString("Task_FromAsync_LongRunning"));
                }
                if ((creationOptions & TaskCreationOptions.PreferFairness) != TaskCreationOptions.None)
                {
                    throw new ArgumentOutOfRangeException("creationOptions", Environment.GetResourceString("Task_FromAsync_PreferFairness"));
                }
            }
            if ((creationOptions & ~(TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness)) != TaskCreationOptions.None)
            {
                throw new ArgumentOutOfRangeException("creationOptions");
            }
        }

        internal static Task[] CheckMultiContinuationTasksAndCopy(Task[] tasks)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            if (tasks.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Task_MultiTaskContinuation_EmptyTaskList"), "tasks");
            }
            Task[] taskArray = new Task[tasks.Length];
            for (int i = 0; i < tasks.Length; i++)
            {
                taskArray[i] = tasks[i];
                if (taskArray[i] == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Task_MultiTaskContinuation_NullTask"), "tasks");
                }
                taskArray[i].ThrowIfDisposed();
            }
            return taskArray;
        }

        internal static Task<TResult>[] CheckMultiContinuationTasksAndCopy<TResult>(Task<TResult>[] tasks)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            if (tasks.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Task_MultiTaskContinuation_EmptyTaskList"), "tasks");
            }
            Task<TResult>[] taskArray = new Task<TResult>[tasks.Length];
            for (int i = 0; i < tasks.Length; i++)
            {
                taskArray[i] = tasks[i];
                if (taskArray[i] == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Task_MultiTaskContinuation_NullTask"), "tasks");
                }
                taskArray[i].ThrowIfDisposed();
            }
            return taskArray;
        }

        internal static void CheckMultiTaskContinuationOptions(TaskContinuationOptions continuationOptions)
        {
            TaskContinuationOptions options = TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.NotOnRanToCompletion;
            TaskContinuationOptions options2 = TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.LongRunning;
            if ((continuationOptions & options2) == options2)
            {
                throw new ArgumentOutOfRangeException("continuationOptions", Environment.GetResourceString("Task_ContinueWith_ESandLR"));
            }
            if ((continuationOptions & ~(((TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.LongRunning | TaskContinuationOptions.PreferFairness) | options) | TaskContinuationOptions.ExecuteSynchronously)) != TaskContinuationOptions.None)
            {
                throw new ArgumentOutOfRangeException("continuationOptions");
            }
            if ((continuationOptions & options) != TaskContinuationOptions.None)
            {
                throw new ArgumentOutOfRangeException("continuationOptions", Environment.GetResourceString("Task_MultiTaskContinuation_FireOptions"));
            }
        }

        internal static Task<bool> CommonCWAllLogic(Task[] tasksCopy)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            int tasksLeft = tasksCopy.Length;
            Action<Task> action = delegate (Task completedTask) {
                if (Interlocked.Decrement(ref tasksLeft) == 0)
                {
                    tcs.TrySetResult(true);
                }
            };
            for (int i = 0; i < tasksCopy.Length; i++)
            {
                if (tasksCopy[i].IsCompleted)
                {
                    action(tasksCopy[i]);
                }
                else
                {
                    tasksCopy[i].AddCompletionAction(action);
                }
            }
            return tcs.Task;
        }

        internal static Task<Task> CommonCWAnyLogic(Task[] tasksCopy)
        {
            TaskCompletionSource<Task> tcs = new TaskCompletionSource<Task>();
            Action<Task> action = delegate (Task t) {
                tcs.TrySetResult(t);
            };
            for (int i = 0; i < tasksCopy.Length; i++)
            {
                if (tcs.Task.IsCompleted)
                {
                    break;
                }
                if (tasksCopy[i].IsCompleted)
                {
                    action(tasksCopy[i]);
                    break;
                }
                tasksCopy[i].AddCompletionAction(action);
            }
            return tcs.Task;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWhenAll(Task[] tasks, Action<Task[]> continuationAction)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return ContinueWhenAll(tasks, continuationAction, this.m_defaultContinuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAll<TResult>(Task[] tasks, Func<Task[], TResult> continuationFunction)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAll<TResult>(tasks, continuationFunction, this.m_defaultContinuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>[]> continuationAction)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return ContinueWhenAll<TAntecedentResult>(tasks, continuationAction, this.m_defaultContinuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAll<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAll<TAntecedentResult, TResult>(tasks, continuationFunction, this.m_defaultContinuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWhenAll(Task[] tasks, Action<Task[]> continuationAction, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return ContinueWhenAll(tasks, continuationAction, this.m_defaultContinuationOptions, cancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWhenAll(Task[] tasks, Action<Task[]> continuationAction, TaskContinuationOptions continuationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return ContinueWhenAll(tasks, continuationAction, continuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAll<TResult>(Task[] tasks, Func<Task[], TResult> continuationFunction, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAll<TResult>(tasks, continuationFunction, this.m_defaultContinuationOptions, cancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAll<TResult>(Task[] tasks, Func<Task[], TResult> continuationFunction, TaskContinuationOptions continuationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAll<TResult>(tasks, continuationFunction, continuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>[]> continuationAction, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return ContinueWhenAll<TAntecedentResult>(tasks, continuationAction, this.m_defaultContinuationOptions, cancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>[]> continuationAction, TaskContinuationOptions continuationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return ContinueWhenAll<TAntecedentResult>(tasks, continuationAction, continuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAll<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAll<TAntecedentResult, TResult>(tasks, continuationFunction, this.m_defaultContinuationOptions, cancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAll<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction, TaskContinuationOptions continuationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAll<TAntecedentResult, TResult>(tasks, continuationFunction, continuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWhenAll(Task[] tasks, Action<Task[]> continuationAction, System.Threading.CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return ContinueWhenAll(tasks, continuationAction, continuationOptions, cancellationToken, scheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAll<TResult>(Task[] tasks, Func<Task[], TResult> continuationFunction, System.Threading.CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAll<TResult>(tasks, continuationFunction, continuationOptions, cancellationToken, scheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>[]> continuationAction, System.Threading.CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return ContinueWhenAll<TAntecedentResult>(tasks, continuationAction, continuationOptions, cancellationToken, scheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAll<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction, System.Threading.CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAll<TAntecedentResult, TResult>(tasks, continuationFunction, continuationOptions, cancellationToken, scheduler, ref lookForMyCaller);
        }

        private static Task ContinueWhenAll(Task[] tasks, Action<Task[]> continuationAction, TaskContinuationOptions continuationOptions, System.Threading.CancellationToken cancellationToken, TaskScheduler scheduler, ref StackCrawlMark stackMark)
        {
            CheckMultiTaskContinuationOptions(continuationOptions);
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            if (continuationAction == null)
            {
                throw new ArgumentNullException("continuationAction");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            Task[] tasksCopy = CheckMultiContinuationTasksAndCopy(tasks);
            if (cancellationToken.IsCancellationRequested)
            {
                return CreateCanceledTask(continuationOptions);
            }
            return CommonCWAllLogic(tasksCopy).ContinueWith(delegate (Task<bool> finishedTask) {
                continuationAction(tasksCopy);
            }, scheduler, cancellationToken, continuationOptions, ref stackMark);
        }

        private Task<TResult> ContinueWhenAll<TResult>(Task[] tasks, Func<Task[], TResult> continuationFunction, TaskContinuationOptions continuationOptions, System.Threading.CancellationToken cancellationToken, TaskScheduler scheduler, ref StackCrawlMark stackMark)
        {
            return TaskFactory<TResult>.ContinueWhenAll(tasks, continuationFunction, continuationOptions, cancellationToken, scheduler, ref stackMark);
        }

        private static Task ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>[]> continuationAction, TaskContinuationOptions continuationOptions, System.Threading.CancellationToken cancellationToken, TaskScheduler scheduler, ref StackCrawlMark stackMark)
        {
            CheckMultiTaskContinuationOptions(continuationOptions);
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            if (continuationAction == null)
            {
                throw new ArgumentNullException("continuationAction");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            Task<TAntecedentResult>[] tasksCopy = CheckMultiContinuationTasksAndCopy<TAntecedentResult>(tasks);
            if (cancellationToken.IsCancellationRequested)
            {
                return CreateCanceledTask(continuationOptions);
            }
            return CommonCWAllLogic((Task[]) tasksCopy).ContinueWith(delegate (Task<bool> finishedTask) {
                continuationAction(tasksCopy);
            }, scheduler, cancellationToken, continuationOptions, ref stackMark);
        }

        private Task<TResult> ContinueWhenAll<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction, TaskContinuationOptions continuationOptions, System.Threading.CancellationToken cancellationToken, TaskScheduler scheduler, ref StackCrawlMark stackMark)
        {
            return TaskFactory<TResult>.ContinueWhenAll<TAntecedentResult>(tasks, continuationFunction, continuationOptions, cancellationToken, scheduler, ref stackMark);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWhenAny(Task[] tasks, Action<Task> continuationAction)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAny(tasks, continuationAction, this.m_defaultContinuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAny<TResult>(Task[] tasks, Func<Task, TResult> continuationFunction)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAny<TResult>(tasks, continuationFunction, this.m_defaultContinuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>> continuationAction)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAny<TAntecedentResult>(tasks, continuationAction, this.m_defaultContinuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAny<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAny<TAntecedentResult, TResult>(tasks, continuationFunction, this.m_defaultContinuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWhenAny(Task[] tasks, Action<Task> continuationAction, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAny(tasks, continuationAction, this.m_defaultContinuationOptions, cancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWhenAny(Task[] tasks, Action<Task> continuationAction, TaskContinuationOptions continuationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAny(tasks, continuationAction, continuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAny<TResult>(Task[] tasks, Func<Task, TResult> continuationFunction, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAny<TResult>(tasks, continuationFunction, this.m_defaultContinuationOptions, cancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAny<TResult>(Task[] tasks, Func<Task, TResult> continuationFunction, TaskContinuationOptions continuationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAny<TResult>(tasks, continuationFunction, continuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>> continuationAction, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAny<TAntecedentResult>(tasks, continuationAction, this.m_defaultContinuationOptions, cancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>> continuationAction, TaskContinuationOptions continuationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAny<TAntecedentResult>(tasks, continuationAction, continuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAny<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAny<TAntecedentResult, TResult>(tasks, continuationFunction, this.m_defaultContinuationOptions, cancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAny<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction, TaskContinuationOptions continuationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAny<TAntecedentResult, TResult>(tasks, continuationFunction, continuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWhenAny(Task[] tasks, Action<Task> continuationAction, System.Threading.CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAny(tasks, continuationAction, continuationOptions, cancellationToken, scheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAny<TResult>(Task[] tasks, Func<Task, TResult> continuationFunction, System.Threading.CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAny<TResult>(tasks, continuationFunction, continuationOptions, cancellationToken, scheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>> continuationAction, System.Threading.CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAny<TAntecedentResult>(tasks, continuationAction, continuationOptions, cancellationToken, scheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAny<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction, System.Threading.CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.ContinueWhenAny<TAntecedentResult, TResult>(tasks, continuationFunction, continuationOptions, cancellationToken, scheduler, ref lookForMyCaller);
        }

        private Task ContinueWhenAny(Task[] tasks, Action<Task> continuationAction, TaskContinuationOptions continuationOptions, System.Threading.CancellationToken cancellationToken, TaskScheduler scheduler, ref StackCrawlMark stackMark)
        {
            CheckMultiTaskContinuationOptions(continuationOptions);
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            if (continuationAction == null)
            {
                throw new ArgumentNullException("continuationAction");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            Task[] tasksCopy = CheckMultiContinuationTasksAndCopy(tasks);
            if (cancellationToken.IsCancellationRequested)
            {
                return CreateCanceledTask(continuationOptions);
            }
            return CommonCWAnyLogic(tasksCopy).ContinueWith(delegate (Task<Task> completedTask) {
                continuationAction(completedTask.Result);
            }, scheduler, cancellationToken, continuationOptions, ref stackMark);
        }

        private Task<TResult> ContinueWhenAny<TResult>(Task[] tasks, Func<Task, TResult> continuationFunction, TaskContinuationOptions continuationOptions, System.Threading.CancellationToken cancellationToken, TaskScheduler scheduler, ref StackCrawlMark stackMark)
        {
            return TaskFactory<TResult>.ContinueWhenAny(tasks, continuationFunction, continuationOptions, cancellationToken, scheduler, ref stackMark);
        }

        private Task ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>> continuationAction, TaskContinuationOptions continuationOptions, System.Threading.CancellationToken cancellationToken, TaskScheduler scheduler, ref StackCrawlMark stackMark)
        {
            CheckMultiTaskContinuationOptions(continuationOptions);
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            if (continuationAction == null)
            {
                throw new ArgumentNullException("continuationAction");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            Task<TAntecedentResult>[] taskArray = CheckMultiContinuationTasksAndCopy<TAntecedentResult>(tasks);
            if (cancellationToken.IsCancellationRequested)
            {
                return CreateCanceledTask(continuationOptions);
            }
            return CommonCWAnyLogic((Task[]) taskArray).ContinueWith(delegate (Task<Task> completedTask) {
                Task<TAntecedentResult> result = completedTask.Result as Task<TAntecedentResult>;
                continuationAction(result);
            }, scheduler, cancellationToken, continuationOptions, ref stackMark);
        }

        private Task<TResult> ContinueWhenAny<TAntecedentResult, TResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction, TaskContinuationOptions continuationOptions, System.Threading.CancellationToken cancellationToken, TaskScheduler scheduler, ref StackCrawlMark stackMark)
        {
            return TaskFactory<TResult>.ContinueWhenAny<TAntecedentResult>(tasks, continuationFunction, continuationOptions, cancellationToken, scheduler, ref stackMark);
        }

        private static Task CreateCanceledTask(TaskContinuationOptions continuationOptions)
        {
            InternalTaskOptions options;
            TaskCreationOptions options2;
            Task.CreationOptionsFromContinuationOptions(continuationOptions, out options2, out options);
            return new Task(true, options2);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task FromAsync(IAsyncResult asyncResult, Action<IAsyncResult> endMethod)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.FromAsync(asyncResult, endMethod, this.m_defaultCreationOptions, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> FromAsync<TResult>(IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.FromAsyncImpl(asyncResult, endMethod, this.m_defaultCreationOptions, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task FromAsync(IAsyncResult asyncResult, Action<IAsyncResult> endMethod, TaskCreationOptions creationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.FromAsync(asyncResult, endMethod, creationOptions, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> FromAsync<TResult>(IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod, TaskCreationOptions creationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.FromAsyncImpl(asyncResult, endMethod, creationOptions, this.DefaultScheduler, ref lookForMyCaller);
        }

        public Task FromAsync(Func<AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, object state)
        {
            return this.FromAsync(beginMethod, endMethod, state, this.m_defaultCreationOptions);
        }

        public Task<TResult> FromAsync<TResult>(Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, object state)
        {
            return TaskFactory<TResult>.FromAsyncImpl(beginMethod, endMethod, state, this.m_defaultCreationOptions);
        }

        public Task<TResult> FromAsync<TResult>(Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, object state, TaskCreationOptions creationOptions)
        {
            return TaskFactory<TResult>.FromAsyncImpl(beginMethod, endMethod, state, creationOptions);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task FromAsync(IAsyncResult asyncResult, Action<IAsyncResult> endMethod, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.FromAsync(asyncResult, endMethod, creationOptions, scheduler, ref lookForMyCaller);
        }

        public Task<TResult> FromAsync<TArg1, TResult>(Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, object state)
        {
            return TaskFactory<TResult>.FromAsyncImpl<TArg1>(beginMethod, endMethod, arg1, state, this.m_defaultCreationOptions);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> FromAsync<TResult>(IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.FromAsyncImpl(asyncResult, endMethod, creationOptions, scheduler, ref lookForMyCaller);
        }

        public Task FromAsync<TArg1>(Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, TArg1 arg1, object state)
        {
            return this.FromAsync<TArg1>(beginMethod, endMethod, arg1, state, this.m_defaultCreationOptions);
        }

        public Task FromAsync(Func<AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, object state, TaskCreationOptions creationOptions)
        {
            AsyncCallback callback = null;
            if (beginMethod == null)
            {
                throw new ArgumentNullException("beginMethod");
            }
            if (endMethod == null)
            {
                throw new ArgumentNullException("endMethod");
            }
            CheckFromAsyncOptions(creationOptions, true);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(state, creationOptions);
            try
            {
                if (callback == null)
                {
                    callback = delegate (IAsyncResult iar) {
                        FromAsyncCoreLogic(iar, endMethod, tcs);
                    };
                }
                beginMethod(callback, state);
            }
            catch
            {
                tcs.TrySetResult(null);
                throw;
            }
            return tcs.Task;
        }

        public Task<TResult> FromAsync<TArg1, TResult>(Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, object state, TaskCreationOptions creationOptions)
        {
            return TaskFactory<TResult>.FromAsyncImpl<TArg1>(beginMethod, endMethod, arg1, state, creationOptions);
        }

        public Task<TResult> FromAsync<TArg1, TArg2, TResult>(Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, object state)
        {
            return TaskFactory<TResult>.FromAsyncImpl<TArg1, TArg2>(beginMethod, endMethod, arg1, arg2, state, this.m_defaultCreationOptions);
        }

        private Task FromAsync(IAsyncResult asyncResult, Action<IAsyncResult> endMethod, TaskCreationOptions creationOptions, TaskScheduler scheduler, ref StackCrawlMark stackMark)
        {
            WaitOrTimerCallback callBack = null;
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (endMethod == null)
            {
                throw new ArgumentNullException("endMethod");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            CheckFromAsyncOptions(creationOptions, false);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(null, creationOptions);
            Task t = new Task(delegate {
                FromAsyncCoreLogic(asyncResult, endMethod, tcs);
            }, null, Task.InternalCurrent, System.Threading.CancellationToken.None, TaskCreationOptions.None, InternalTaskOptions.None, null, ref stackMark);
            if (asyncResult.IsCompleted)
            {
                try
                {
                    t.RunSynchronously(scheduler);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            }
            else
            {
                if (callBack == null)
                {
                    callBack = delegate {
                        try
                        {
                            t.RunSynchronously(scheduler);
                        }
                        catch (Exception exception)
                        {
                            tcs.TrySetException(exception);
                        }
                    };
                }
                ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, callBack, null, -1, true);
            }
            return tcs.Task;
        }

        public Task FromAsync<TArg1>(Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, TArg1 arg1, object state, TaskCreationOptions creationOptions)
        {
            AsyncCallback callback = null;
            if (beginMethod == null)
            {
                throw new ArgumentNullException("beginMethod");
            }
            if (endMethod == null)
            {
                throw new ArgumentNullException("endMethod");
            }
            CheckFromAsyncOptions(creationOptions, true);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(state, creationOptions);
            try
            {
                if (callback == null)
                {
                    callback = delegate (IAsyncResult iar) {
                        FromAsyncCoreLogic(iar, endMethod, tcs);
                    };
                }
                beginMethod(arg1, callback, state);
            }
            catch
            {
                tcs.TrySetResult(null);
                throw;
            }
            return tcs.Task;
        }

        public Task FromAsync<TArg1, TArg2>(Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, TArg1 arg1, TArg2 arg2, object state)
        {
            return this.FromAsync<TArg1, TArg2>(beginMethod, endMethod, arg1, arg2, state, this.m_defaultCreationOptions);
        }

        public Task FromAsync<TArg1, TArg2>(Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, TArg1 arg1, TArg2 arg2, object state, TaskCreationOptions creationOptions)
        {
            AsyncCallback callback = null;
            if (beginMethod == null)
            {
                throw new ArgumentNullException("beginMethod");
            }
            if (endMethod == null)
            {
                throw new ArgumentNullException("endMethod");
            }
            CheckFromAsyncOptions(creationOptions, true);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(state, creationOptions);
            try
            {
                if (callback == null)
                {
                    callback = delegate (IAsyncResult iar) {
                        FromAsyncCoreLogic(iar, endMethod, tcs);
                    };
                }
                beginMethod(arg1, arg2, callback, state);
            }
            catch
            {
                tcs.TrySetResult(null);
                throw;
            }
            return tcs.Task;
        }

        public Task<TResult> FromAsync<TArg1, TArg2, TResult>(Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, object state, TaskCreationOptions creationOptions)
        {
            return TaskFactory<TResult>.FromAsyncImpl<TArg1, TArg2>(beginMethod, endMethod, arg1, arg2, state, creationOptions);
        }

        public Task FromAsync<TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, TArg1 arg1, TArg2 arg2, TArg3 arg3, object state)
        {
            return this.FromAsync<TArg1, TArg2, TArg3>(beginMethod, endMethod, arg1, arg2, arg3, state, this.m_defaultCreationOptions);
        }

        public Task<TResult> FromAsync<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, TArg3 arg3, object state)
        {
            return TaskFactory<TResult>.FromAsyncImpl<TArg1, TArg2, TArg3>(beginMethod, endMethod, arg1, arg2, arg3, state, this.m_defaultCreationOptions);
        }

        public Task FromAsync<TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, TArg1 arg1, TArg2 arg2, TArg3 arg3, object state, TaskCreationOptions creationOptions)
        {
            AsyncCallback callback = null;
            if (beginMethod == null)
            {
                throw new ArgumentNullException("beginMethod");
            }
            if (endMethod == null)
            {
                throw new ArgumentNullException("endMethod");
            }
            CheckFromAsyncOptions(creationOptions, true);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(state, creationOptions);
            try
            {
                if (callback == null)
                {
                    callback = delegate (IAsyncResult iar) {
                        FromAsyncCoreLogic(iar, endMethod, tcs);
                    };
                }
                beginMethod(arg1, arg2, arg3, callback, state);
            }
            catch
            {
                tcs.TrySetResult(null);
                throw;
            }
            return tcs.Task;
        }

        public Task<TResult> FromAsync<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, TArg3 arg3, object state, TaskCreationOptions creationOptions)
        {
            return TaskFactory<TResult>.FromAsyncImpl<TArg1, TArg2, TArg3>(beginMethod, endMethod, arg1, arg2, arg3, state, creationOptions);
        }

        private static void FromAsyncCoreLogic(IAsyncResult iar, Action<IAsyncResult> endMethod, TaskCompletionSource<object> tcs)
        {
            Exception exception = null;
            OperationCanceledException exception2 = null;
            try
            {
                endMethod(iar);
            }
            catch (OperationCanceledException exception3)
            {
                exception2 = exception3;
            }
            catch (Exception exception4)
            {
                exception = exception4;
            }
            finally
            {
                if (exception2 != null)
                {
                    tcs.TrySetCanceled();
                }
                else if (exception != null)
                {
                    if (tcs.TrySetException(exception) && (exception is ThreadAbortException))
                    {
                        tcs.Task.m_contingentProperties.m_exceptionsHolder.MarkAsHandled(false);
                    }
                }
                else
                {
                    tcs.TrySetResult(null);
                }
            }
        }

        private TaskScheduler GetDefaultScheduler(Task currTask)
        {
            if (this.m_defaultScheduler != null)
            {
                return this.m_defaultScheduler;
            }
            if (currTask != null)
            {
                return currTask.ExecutingTaskScheduler;
            }
            return TaskScheduler.Default;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task StartNew(Action action)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task.InternalStartNew(internalCurrent, action, null, this.m_defaultCancellationToken, this.GetDefaultScheduler(internalCurrent), this.m_defaultCreationOptions, InternalTaskOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> StartNew<TResult>(Func<TResult> function)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task<TResult>.StartNew(internalCurrent, function, this.m_defaultCancellationToken, this.m_defaultCreationOptions, InternalTaskOptions.None, this.GetDefaultScheduler(internalCurrent), ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task StartNew(Action action, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task.InternalStartNew(internalCurrent, action, null, cancellationToken, this.GetDefaultScheduler(internalCurrent), this.m_defaultCreationOptions, InternalTaskOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task StartNew(Action action, TaskCreationOptions creationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task.InternalStartNew(internalCurrent, action, null, this.m_defaultCancellationToken, this.GetDefaultScheduler(internalCurrent), creationOptions, InternalTaskOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task StartNew(Action<object> action, object state)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task.InternalStartNew(internalCurrent, action, state, this.m_defaultCancellationToken, this.GetDefaultScheduler(internalCurrent), this.m_defaultCreationOptions, InternalTaskOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> StartNew<TResult>(Func<TResult> function, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task<TResult>.StartNew(internalCurrent, function, cancellationToken, this.m_defaultCreationOptions, InternalTaskOptions.None, this.GetDefaultScheduler(internalCurrent), ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> StartNew<TResult>(Func<TResult> function, TaskCreationOptions creationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task<TResult>.StartNew(internalCurrent, function, this.m_defaultCancellationToken, creationOptions, InternalTaskOptions.None, this.GetDefaultScheduler(internalCurrent), ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> StartNew<TResult>(Func<object, TResult> function, object state)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task<TResult>.StartNew(internalCurrent, function, state, this.m_defaultCancellationToken, this.m_defaultCreationOptions, InternalTaskOptions.None, this.GetDefaultScheduler(internalCurrent), ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task StartNew(Action<object> action, object state, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task.InternalStartNew(internalCurrent, action, state, cancellationToken, this.GetDefaultScheduler(internalCurrent), this.m_defaultCreationOptions, InternalTaskOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task StartNew(Action<object> action, object state, TaskCreationOptions creationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task.InternalStartNew(internalCurrent, action, state, this.m_defaultCancellationToken, this.GetDefaultScheduler(internalCurrent), creationOptions, InternalTaskOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> StartNew<TResult>(Func<object, TResult> function, object state, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task<TResult>.StartNew(internalCurrent, function, state, cancellationToken, this.m_defaultCreationOptions, InternalTaskOptions.None, this.GetDefaultScheduler(internalCurrent), ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> StartNew<TResult>(Func<object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task<TResult>.StartNew(internalCurrent, function, state, this.m_defaultCancellationToken, creationOptions, InternalTaskOptions.None, this.GetDefaultScheduler(internalCurrent), ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task StartNew(Action action, System.Threading.CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return Task.InternalStartNew(Task.InternalCurrent, action, null, cancellationToken, scheduler, creationOptions, InternalTaskOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> StartNew<TResult>(Func<TResult> function, System.Threading.CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return Task<TResult>.StartNew(Task.InternalCurrent, function, cancellationToken, creationOptions, InternalTaskOptions.None, scheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal Task StartNew(Action action, System.Threading.CancellationToken cancellationToken, TaskCreationOptions creationOptions, InternalTaskOptions internalOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return Task.InternalStartNew(Task.InternalCurrent, action, null, cancellationToken, scheduler, creationOptions, internalOptions, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task StartNew(Action<object> action, object state, System.Threading.CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return Task.InternalStartNew(Task.InternalCurrent, action, state, cancellationToken, scheduler, creationOptions, InternalTaskOptions.None, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> StartNew<TResult>(Func<object, TResult> function, object state, System.Threading.CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return Task<TResult>.StartNew(Task.InternalCurrent, function, state, cancellationToken, creationOptions, InternalTaskOptions.None, scheduler, ref lookForMyCaller);
        }

        public System.Threading.CancellationToken CancellationToken
        {
            get
            {
                return this.m_defaultCancellationToken;
            }
        }

        public TaskContinuationOptions ContinuationOptions
        {
            get
            {
                return this.m_defaultContinuationOptions;
            }
        }

        public TaskCreationOptions CreationOptions
        {
            get
            {
                return this.m_defaultCreationOptions;
            }
        }

        private TaskScheduler DefaultScheduler
        {
            get
            {
                if (this.m_defaultScheduler == null)
                {
                    return TaskScheduler.Current;
                }
                return this.m_defaultScheduler;
            }
        }

        public TaskScheduler Scheduler
        {
            get
            {
                return this.m_defaultScheduler;
            }
        }
    }
}

