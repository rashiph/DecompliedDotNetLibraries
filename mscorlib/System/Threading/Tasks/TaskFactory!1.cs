namespace System.Threading.Tasks
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class TaskFactory<TResult>
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
            TaskFactory.CheckCreationOptions(this.m_defaultCreationOptions);
            TaskFactory.CheckMultiTaskContinuationOptions(this.m_defaultContinuationOptions);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAll(Task[] tasks, Func<Task[], TResult> continuationFunction)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.ContinueWhenAll(tasks, continuationFunction, this.m_defaultContinuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.ContinueWhenAll<TAntecedentResult>(tasks, continuationFunction, this.m_defaultContinuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAll(Task[] tasks, Func<Task[], TResult> continuationFunction, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.ContinueWhenAll(tasks, continuationFunction, this.m_defaultContinuationOptions, cancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAll(Task[] tasks, Func<Task[], TResult> continuationFunction, TaskContinuationOptions continuationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.ContinueWhenAll(tasks, continuationFunction, continuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.ContinueWhenAll<TAntecedentResult>(tasks, continuationFunction, this.m_defaultContinuationOptions, cancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction, TaskContinuationOptions continuationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.ContinueWhenAll<TAntecedentResult>(tasks, continuationFunction, continuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAll(Task[] tasks, Func<Task[], TResult> continuationFunction, System.Threading.CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.ContinueWhenAll(tasks, continuationFunction, continuationOptions, cancellationToken, scheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction, System.Threading.CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.ContinueWhenAll<TAntecedentResult>(tasks, continuationFunction, continuationOptions, cancellationToken, scheduler, ref lookForMyCaller);
        }

        internal static Task<TResult> ContinueWhenAll(Task[] tasks, Func<Task[], TResult> continuationFunction, TaskContinuationOptions continuationOptions, System.Threading.CancellationToken cancellationToken, TaskScheduler scheduler, ref StackCrawlMark stackMark)
        {
            TaskFactory.CheckMultiTaskContinuationOptions(continuationOptions);
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            if (continuationFunction == null)
            {
                throw new ArgumentNullException("continuationFunction");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            Task[] tasksCopy = TaskFactory.CheckMultiContinuationTasksAndCopy(tasks);
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskFactory<TResult>.CreateCanceledTask(continuationOptions);
            }
            return TaskFactory.CommonCWAllLogic(tasksCopy).ContinueWith<TResult>(finishedTask => continuationFunction(tasksCopy), scheduler, cancellationToken, continuationOptions, ref stackMark);
        }

        internal static Task<TResult> ContinueWhenAll<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>[], TResult> continuationFunction, TaskContinuationOptions continuationOptions, System.Threading.CancellationToken cancellationToken, TaskScheduler scheduler, ref StackCrawlMark stackMark)
        {
            TaskFactory.CheckMultiTaskContinuationOptions(continuationOptions);
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            if (continuationFunction == null)
            {
                throw new ArgumentNullException("continuationFunction");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            Task<TAntecedentResult>[] tasksCopy = TaskFactory.CheckMultiContinuationTasksAndCopy<TAntecedentResult>(tasks);
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskFactory<TResult>.CreateCanceledTask(continuationOptions);
            }
            return TaskFactory.CommonCWAllLogic((Task[]) tasksCopy).ContinueWith<TResult>(finishedTask => this.continuationFunction(this.tasksCopy), scheduler, cancellationToken, continuationOptions, ref stackMark);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAny(Task[] tasks, Func<Task, TResult> continuationFunction)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.ContinueWhenAny(tasks, continuationFunction, this.m_defaultContinuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.ContinueWhenAny<TAntecedentResult>(tasks, continuationFunction, this.m_defaultContinuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAny(Task[] tasks, Func<Task, TResult> continuationFunction, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.ContinueWhenAny(tasks, continuationFunction, this.m_defaultContinuationOptions, cancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAny(Task[] tasks, Func<Task, TResult> continuationFunction, TaskContinuationOptions continuationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.ContinueWhenAny(tasks, continuationFunction, continuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.ContinueWhenAny<TAntecedentResult>(tasks, continuationFunction, this.m_defaultContinuationOptions, cancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction, TaskContinuationOptions continuationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.ContinueWhenAny<TAntecedentResult>(tasks, continuationFunction, continuationOptions, this.m_defaultCancellationToken, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAny(Task[] tasks, Func<Task, TResult> continuationFunction, System.Threading.CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.ContinueWhenAny(tasks, continuationFunction, continuationOptions, cancellationToken, scheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction, System.Threading.CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.ContinueWhenAny<TAntecedentResult>(tasks, continuationFunction, continuationOptions, cancellationToken, scheduler, ref lookForMyCaller);
        }

        internal static Task<TResult> ContinueWhenAny(Task[] tasks, Func<Task, TResult> continuationFunction, TaskContinuationOptions continuationOptions, System.Threading.CancellationToken cancellationToken, TaskScheduler scheduler, ref StackCrawlMark stackMark)
        {
            TaskFactory.CheckMultiTaskContinuationOptions(continuationOptions);
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            if (continuationFunction == null)
            {
                throw new ArgumentNullException("continuationFunction");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            Task[] tasksCopy = TaskFactory.CheckMultiContinuationTasksAndCopy(tasks);
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskFactory<TResult>.CreateCanceledTask(continuationOptions);
            }
            return TaskFactory.CommonCWAnyLogic(tasksCopy).ContinueWith<TResult>(completedTask => continuationFunction(completedTask.Result), scheduler, cancellationToken, continuationOptions, ref stackMark);
        }

        internal static Task<TResult> ContinueWhenAny<TAntecedentResult>(Task<TAntecedentResult>[] tasks, Func<Task<TAntecedentResult>, TResult> continuationFunction, TaskContinuationOptions continuationOptions, System.Threading.CancellationToken cancellationToken, TaskScheduler scheduler, ref StackCrawlMark stackMark)
        {
            TaskFactory.CheckMultiTaskContinuationOptions(continuationOptions);
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            if (continuationFunction == null)
            {
                throw new ArgumentNullException("continuationFunction");
            }
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            Task<TAntecedentResult>[] taskArray = TaskFactory.CheckMultiContinuationTasksAndCopy<TAntecedentResult>(tasks);
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskFactory<TResult>.CreateCanceledTask(continuationOptions);
            }
            return TaskFactory.CommonCWAnyLogic((Task[]) taskArray).ContinueWith<TResult>(completedTask => this.continuationFunction(completedTask.Result as Task<TAntecedentResult>), scheduler, cancellationToken, continuationOptions, ref stackMark);
        }

        private static Task<TResult> CreateCanceledTask(TaskContinuationOptions continuationOptions)
        {
            TaskCreationOptions options;
            InternalTaskOptions options2;
            Task.CreationOptionsFromContinuationOptions(continuationOptions, out options, out options2);
            return new Task<TResult>(true, default(TResult), options);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> FromAsync(IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.FromAsyncImpl(asyncResult, endMethod, this.m_defaultCreationOptions, this.DefaultScheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> FromAsync(IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod, TaskCreationOptions creationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.FromAsyncImpl(asyncResult, endMethod, creationOptions, this.DefaultScheduler, ref lookForMyCaller);
        }

        public Task<TResult> FromAsync(Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, object state)
        {
            return TaskFactory<TResult>.FromAsyncImpl(beginMethod, endMethod, state, this.m_defaultCreationOptions);
        }

        public Task<TResult> FromAsync<TArg1>(Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, object state)
        {
            return TaskFactory<TResult>.FromAsyncImpl<TArg1>(beginMethod, endMethod, arg1, state, this.m_defaultCreationOptions);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> FromAsync(IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return TaskFactory<TResult>.FromAsyncImpl(asyncResult, endMethod, creationOptions, scheduler, ref lookForMyCaller);
        }

        public Task<TResult> FromAsync(Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, object state, TaskCreationOptions creationOptions)
        {
            return TaskFactory<TResult>.FromAsyncImpl(beginMethod, endMethod, state, creationOptions);
        }

        public Task<TResult> FromAsync<TArg1>(Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, object state, TaskCreationOptions creationOptions)
        {
            return TaskFactory<TResult>.FromAsyncImpl<TArg1>(beginMethod, endMethod, arg1, state, creationOptions);
        }

        public Task<TResult> FromAsync<TArg1, TArg2>(Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, object state)
        {
            return TaskFactory<TResult>.FromAsyncImpl<TArg1, TArg2>(beginMethod, endMethod, arg1, arg2, state, this.m_defaultCreationOptions);
        }

        public Task<TResult> FromAsync<TArg1, TArg2>(Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, object state, TaskCreationOptions creationOptions)
        {
            return TaskFactory<TResult>.FromAsyncImpl<TArg1, TArg2>(beginMethod, endMethod, arg1, arg2, state, creationOptions);
        }

        public Task<TResult> FromAsync<TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, TArg3 arg3, object state)
        {
            return TaskFactory<TResult>.FromAsyncImpl<TArg1, TArg2, TArg3>(beginMethod, endMethod, arg1, arg2, arg3, state, this.m_defaultCreationOptions);
        }

        public Task<TResult> FromAsync<TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, TArg3 arg3, object state, TaskCreationOptions creationOptions)
        {
            return TaskFactory<TResult>.FromAsyncImpl<TArg1, TArg2, TArg3>(beginMethod, endMethod, arg1, arg2, arg3, state, creationOptions);
        }

        private static void FromAsyncCoreLogic(IAsyncResult iar, Func<IAsyncResult, TResult> endMethod, TaskCompletionSource<TResult> tcs)
        {
            Exception exception = null;
            OperationCanceledException exception2 = null;
            TResult result = default(TResult);
            try
            {
                result = endMethod(iar);
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
                    tcs.TrySetResult(result);
                }
            }
        }

        internal static Task<TResult> FromAsyncImpl(Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, object state, TaskCreationOptions creationOptions)
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
            TaskFactory.CheckFromAsyncOptions(creationOptions, true);
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>(state, creationOptions);
            try
            {
                if (callback == null)
                {
                    callback = delegate (IAsyncResult iar) {
                        TaskFactory<TResult>.FromAsyncCoreLogic(iar, endMethod, tcs);
                    };
                }
                beginMethod(callback, state);
            }
            catch
            {
                tcs.TrySetResult(default(TResult));
                throw;
            }
            return tcs.Task;
        }

        internal static Task<TResult> FromAsyncImpl<TArg1>(Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, object state, TaskCreationOptions creationOptions)
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
            TaskFactory.CheckFromAsyncOptions(creationOptions, true);
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>(state, creationOptions);
            try
            {
                if (callback == null)
                {
                    callback = delegate (IAsyncResult iar) {
                        TaskFactory<TResult>.FromAsyncCoreLogic(iar, this.endMethod, this.tcs);
                    };
                }
                beginMethod(arg1, callback, state);
            }
            catch
            {
                tcs.TrySetResult(default(TResult));
                throw;
            }
            return tcs.Task;
        }

        internal static Task<TResult> FromAsyncImpl(IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod, TaskCreationOptions creationOptions, TaskScheduler scheduler, ref StackCrawlMark stackMark)
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
            TaskFactory.CheckFromAsyncOptions(creationOptions, false);
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>(creationOptions);
            Task t = new Task(delegate {
                TaskFactory<TResult>.FromAsyncCoreLogic(asyncResult, endMethod, tcs);
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

        internal static Task<TResult> FromAsyncImpl<TArg1, TArg2>(Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, object state, TaskCreationOptions creationOptions)
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
            TaskFactory.CheckFromAsyncOptions(creationOptions, true);
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>(state, creationOptions);
            try
            {
                if (callback == null)
                {
                    callback = delegate (IAsyncResult iar) {
                        TaskFactory<TResult>.FromAsyncCoreLogic(iar, this.endMethod, this.tcs);
                    };
                }
                beginMethod(arg1, arg2, callback, state);
            }
            catch
            {
                tcs.TrySetResult(default(TResult));
                throw;
            }
            return tcs.Task;
        }

        internal static Task<TResult> FromAsyncImpl<TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg1 arg1, TArg2 arg2, TArg3 arg3, object state, TaskCreationOptions creationOptions)
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
            TaskFactory.CheckFromAsyncOptions(creationOptions, true);
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>(state, creationOptions);
            try
            {
                if (callback == null)
                {
                    callback = delegate (IAsyncResult iar) {
                        TaskFactory<TResult>.FromAsyncCoreLogic(iar, this.endMethod, this.tcs);
                    };
                }
                beginMethod(arg1, arg2, arg3, callback, state);
            }
            catch
            {
                tcs.TrySetResult(default(TResult));
                throw;
            }
            return tcs.Task;
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
        public Task<TResult> StartNew(Func<TResult> function)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task<TResult>.StartNew(internalCurrent, function, this.m_defaultCancellationToken, this.m_defaultCreationOptions, InternalTaskOptions.None, this.GetDefaultScheduler(internalCurrent), ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> StartNew(Func<TResult> function, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task<TResult>.StartNew(internalCurrent, function, cancellationToken, this.m_defaultCreationOptions, InternalTaskOptions.None, this.GetDefaultScheduler(internalCurrent), ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> StartNew(Func<TResult> function, TaskCreationOptions creationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task<TResult>.StartNew(internalCurrent, function, this.m_defaultCancellationToken, creationOptions, InternalTaskOptions.None, this.GetDefaultScheduler(internalCurrent), ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> StartNew(Func<object, TResult> function, object state)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task<TResult>.StartNew(internalCurrent, function, state, this.m_defaultCancellationToken, this.m_defaultCreationOptions, InternalTaskOptions.None, this.GetDefaultScheduler(internalCurrent), ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> StartNew(Func<object, TResult> function, object state, System.Threading.CancellationToken cancellationToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task<TResult>.StartNew(internalCurrent, function, state, cancellationToken, this.m_defaultCreationOptions, InternalTaskOptions.None, this.GetDefaultScheduler(internalCurrent), ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> StartNew(Func<object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Task internalCurrent = Task.InternalCurrent;
            return Task<TResult>.StartNew(internalCurrent, function, state, this.m_defaultCancellationToken, creationOptions, InternalTaskOptions.None, this.GetDefaultScheduler(internalCurrent), ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> StartNew(Func<TResult> function, System.Threading.CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return Task<TResult>.StartNew(Task.InternalCurrent, function, cancellationToken, creationOptions, InternalTaskOptions.None, scheduler, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<TResult> StartNew(Func<object, TResult> function, object state, System.Threading.CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
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

