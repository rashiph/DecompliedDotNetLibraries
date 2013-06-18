namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Web;

    internal class PageAsyncTaskManager
    {
        private HttpApplication _app;
        private HttpAsyncResult _asyncResult;
        private bool _failedToStart;
        private volatile bool _inProgress;
        private Page _page;
        private WaitCallback _resumeTasksCallback;
        private ArrayList _tasks;
        private int _tasksCompleted;
        private int _tasksStarted;
        private DateTime _timeoutEnd;
        private volatile bool _timeoutEndReached;
        private Timer _timeoutTimer;

        internal PageAsyncTaskManager(Page page)
        {
            this._page = page;
            this._app = page.Context.ApplicationInstance;
            this._tasks = new ArrayList();
            this._resumeTasksCallback = new WaitCallback(this.ResumeTasksThreadpoolThread);
        }

        internal void AddTask(PageAsyncTask task)
        {
            this._tasks.Add(task);
        }

        private IAsyncResult BeginExecuteAsyncTasks(object sender, EventArgs e, AsyncCallback cb, object extraData)
        {
            return this.ExecuteTasks(cb, extraData);
        }

        internal void CompleteAllTasksNow(bool syncCaller)
        {
            this.WaitForAllStartedTasks(syncCaller, true);
        }

        internal void DisposeTimer()
        {
            Timer comparand = this._timeoutTimer;
            if ((comparand != null) && (Interlocked.CompareExchange<Timer>(ref this._timeoutTimer, null, comparand) == comparand))
            {
                comparand.Dispose();
            }
        }

        private void EndExecuteAsyncTasks(IAsyncResult ar)
        {
            this._asyncResult.End();
        }

        internal HttpAsyncResult ExecuteTasks(AsyncCallback callback, object extraData)
        {
            this._failedToStart = false;
            this._timeoutEnd = DateTime.UtcNow + this._page.AsyncTimeout;
            this._timeoutEndReached = false;
            this._tasksStarted = 0;
            this._tasksCompleted = 0;
            this._asyncResult = new HttpAsyncResult(callback, extraData);
            bool waitUntilDone = callback == null;
            if (waitUntilDone)
            {
                try
                {
                }
                finally
                {
                    try
                    {
                        Monitor.Exit(this._app);
                        Monitor.Enter(this._app);
                    }
                    catch (SynchronizationLockException)
                    {
                        this._failedToStart = true;
                        throw new InvalidOperationException(System.Web.SR.GetString("Async_tasks_wrong_thread"));
                    }
                }
            }
            this._inProgress = true;
            try
            {
                this.ResumeTasks(waitUntilDone, true);
            }
            finally
            {
                if (waitUntilDone)
                {
                    this._inProgress = false;
                }
            }
            return this._asyncResult;
        }

        internal void RegisterHandlersForPagePreRenderCompleteAsync()
        {
            this._page.AddOnPreRenderCompleteAsync(new BeginEventHandler(this.BeginExecuteAsyncTasks), new EndEventHandler(this.EndExecuteAsyncTasks));
        }

        private void ResumeTasks(bool waitUntilDone, bool onCallerThread)
        {
            Interlocked.Increment(ref this._tasksStarted);
            try
            {
                if (onCallerThread)
                {
                    this.ResumeTasksPossiblyUnderLock(waitUntilDone);
                }
                else
                {
                    lock (this._app)
                    {
                        HttpApplication.ThreadContext context = null;
                        try
                        {
                            context = this._app.OnThreadEnter();
                            this.ResumeTasksPossiblyUnderLock(waitUntilDone);
                        }
                        finally
                        {
                            if (context != null)
                            {
                                context.Leave();
                            }
                        }
                    }
                }
            }
            finally
            {
                this.TaskCompleted(onCallerThread);
            }
        }

        private void ResumeTasksPossiblyUnderLock(bool waitUntilDone)
        {
            while (this.AnyTasksRemain)
            {
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                for (int i = 0; i < this._tasks.Count; i++)
                {
                    PageAsyncTask task = (PageAsyncTask) this._tasks[i];
                    if (!task.Started && (!flag3 || task.ExecuteInParallel))
                    {
                        flag = true;
                        Interlocked.Increment(ref this._tasksStarted);
                        task.Start(this, this._page, EventArgs.Empty);
                        if (!task.CompletedSynchronously)
                        {
                            flag2 = true;
                            if (!task.ExecuteInParallel)
                            {
                                break;
                            }
                            flag3 = true;
                        }
                    }
                }
                if (!flag)
                {
                    return;
                }
                if ((!this.TimeoutEndReached && flag2) && !waitUntilDone)
                {
                    this.StartTimerIfNeeeded();
                    return;
                }
                bool flag4 = true;
                try
                {
                    try
                    {
                    }
                    finally
                    {
                        Monitor.Exit(this._app);
                        flag4 = false;
                    }
                    this.WaitForAllStartedTasks(true, false);
                    continue;
                }
                finally
                {
                    if (!flag4)
                    {
                        Monitor.Enter(this._app);
                    }
                }
            }
        }

        private void ResumeTasksThreadpoolThread(object data)
        {
            this.ResumeTasks(false, false);
        }

        private void StartTimerIfNeeeded()
        {
            if (this._timeoutTimer == null)
            {
                DateTime utcNow = DateTime.UtcNow;
                if (utcNow < this._timeoutEnd)
                {
                    TimeSpan span = (TimeSpan) (this._timeoutEnd - utcNow);
                    double totalMilliseconds = span.TotalMilliseconds;
                    if (totalMilliseconds < 2147483647.0)
                    {
                        this._timeoutTimer = new Timer(new TimerCallback(this.TimeoutTimerCallback), null, (int) totalMilliseconds, -1);
                    }
                }
            }
        }

        internal void TaskCompleted(bool onCallerThread)
        {
            if (Interlocked.Increment(ref this._tasksCompleted) >= this._tasksStarted)
            {
                if (!this.AnyTasksRemain)
                {
                    this._inProgress = false;
                    this._asyncResult.Complete(onCallerThread, null, this.AnyTaskError);
                }
                else if (Thread.CurrentThread.IsThreadPoolThread)
                {
                    this.ResumeTasks(false, onCallerThread);
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(this._resumeTasksCallback);
                }
            }
        }

        private void TimeoutTimerCallback(object state)
        {
            this.DisposeTimer();
            this.WaitForAllStartedTasks(false, false);
        }

        private void WaitForAllStartedTasks(bool syncCaller, bool forceTimeout)
        {
            for (int i = 0; i < this._tasks.Count; i++)
            {
                PageAsyncTask task = (PageAsyncTask) this._tasks[i];
                if (task.Started && !task.Completed)
                {
                    if (!forceTimeout && !this.TimeoutEndReached)
                    {
                        DateTime utcNow = DateTime.UtcNow;
                        if (utcNow < this._timeoutEnd)
                        {
                            WaitHandle asyncWaitHandle = task.AsyncResult.AsyncWaitHandle;
                            if (((asyncWaitHandle != null) && asyncWaitHandle.WaitOne((TimeSpan) (this._timeoutEnd - utcNow), false)) && task.Completed)
                            {
                                continue;
                            }
                        }
                    }
                    bool flag2 = false;
                    while (!task.Completed)
                    {
                        if (forceTimeout || (!flag2 && this.TimeoutEndReached))
                        {
                            task.ForceTimeout(syncCaller);
                            flag2 = true;
                        }
                        else
                        {
                            Thread.Sleep(50);
                        }
                    }
                }
            }
        }

        private Exception AnyTaskError
        {
            get
            {
                for (int i = 0; i < this._tasks.Count; i++)
                {
                    PageAsyncTask task = (PageAsyncTask) this._tasks[i];
                    if (task.Error != null)
                    {
                        return task.Error;
                    }
                }
                return null;
            }
        }

        internal bool AnyTasksRemain
        {
            get
            {
                for (int i = 0; i < this._tasks.Count; i++)
                {
                    PageAsyncTask task = (PageAsyncTask) this._tasks[i];
                    if (!task.Started)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        internal HttpApplication Application
        {
            get
            {
                return this._app;
            }
        }

        internal bool FailedToStartTasks
        {
            get
            {
                return this._failedToStart;
            }
        }

        internal bool TaskExecutionInProgress
        {
            get
            {
                return this._inProgress;
            }
        }

        private bool TimeoutEndReached
        {
            get
            {
                if (!this._timeoutEndReached && (DateTime.UtcNow >= this._timeoutEnd))
                {
                    this._timeoutEndReached = true;
                }
                return this._timeoutEndReached;
            }
        }
    }
}

