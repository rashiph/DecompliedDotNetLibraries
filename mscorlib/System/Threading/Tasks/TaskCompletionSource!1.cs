namespace System.Threading.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class TaskCompletionSource<TResult>
    {
        private Task<TResult> m_task;

        public TaskCompletionSource() : this(null, TaskCreationOptions.None)
        {
        }

        public TaskCompletionSource(object state) : this(state, TaskCreationOptions.None)
        {
        }

        public TaskCompletionSource(TaskCreationOptions creationOptions) : this(null, creationOptions)
        {
        }

        public TaskCompletionSource(object state, TaskCreationOptions creationOptions)
        {
            this.m_task = new Task<TResult>(state, CancellationToken.None, creationOptions, InternalTaskOptions.PromiseTask);
        }

        public void SetCanceled()
        {
            if (!this.TrySetCanceled())
            {
                throw new InvalidOperationException(Environment.GetResourceString("TaskT_TransitionToFinal_AlreadyCompleted"));
            }
        }

        public void SetException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            if (!this.TrySetException(exception))
            {
                throw new InvalidOperationException(Environment.GetResourceString("TaskT_TransitionToFinal_AlreadyCompleted"));
            }
        }

        public void SetException(IEnumerable<Exception> exceptions)
        {
            if (!this.TrySetException(exceptions))
            {
                throw new InvalidOperationException(Environment.GetResourceString("TaskT_TransitionToFinal_AlreadyCompleted"));
            }
        }

        public void SetResult(TResult result)
        {
            this.m_task.Result = result;
        }

        public bool TrySetCanceled()
        {
            if (this.m_task.AtomicStateUpdate(0x4000000, 0x5600000))
            {
                this.m_task.RecordInternalCancellationRequest();
                this.m_task.CancellationCleanupLogic();
                return true;
            }
            if (!this.m_task.IsCompleted)
            {
                SpinWait wait = new SpinWait();
                while (!this.m_task.IsCompleted)
                {
                    wait.SpinOnce();
                }
            }
            return false;
        }

        public bool TrySetException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            bool flag = this.m_task.TrySetException(exception);
            if (!flag && !this.m_task.IsCompleted)
            {
                SpinWait wait = new SpinWait();
                while (!this.m_task.IsCompleted)
                {
                    wait.SpinOnce();
                }
            }
            return flag;
        }

        public bool TrySetException(IEnumerable<Exception> exceptions)
        {
            if (exceptions == null)
            {
                throw new ArgumentNullException("exceptions");
            }
            List<Exception> exceptionObject = new List<Exception>();
            foreach (Exception exception in exceptions)
            {
                if (exception == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("TaskCompletionSourceT_TrySetException_NullException"), "exceptions");
                }
                exceptionObject.Add(exception);
            }
            if (exceptionObject.Count == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("TaskCompletionSourceT_TrySetException_NoExceptions"), "exceptions");
            }
            bool flag = this.m_task.TrySetException(exceptionObject);
            if (!flag && !this.m_task.IsCompleted)
            {
                SpinWait wait = new SpinWait();
                while (!this.m_task.IsCompleted)
                {
                    wait.SpinOnce();
                }
            }
            return flag;
        }

        public bool TrySetResult(TResult result)
        {
            if (this.m_task.IsCompleted)
            {
                return false;
            }
            bool flag = this.m_task.TrySetResult(result);
            if (!flag && !this.m_task.IsCompleted)
            {
                SpinWait wait = new SpinWait();
                while (!this.m_task.IsCompleted)
                {
                    wait.SpinOnce();
                }
            }
            return flag;
        }

        public Task<TResult> Task
        {
            get
            {
                return this.m_task;
            }
        }
    }
}

