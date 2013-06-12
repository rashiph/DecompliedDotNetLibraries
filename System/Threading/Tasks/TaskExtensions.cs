namespace System.Threading.Tasks
{
    using System;
    using System.Runtime.CompilerServices;

    public static class TaskExtensions
    {
        private static bool TrySetFromTask<TResult>(this TaskCompletionSource<TResult> me, Task source)
        {
            switch (source.Status)
            {
                case TaskStatus.RanToCompletion:
                    if (source is Task<TResult>)
                    {
                        return me.TrySetResult(((Task<TResult>) source).Result);
                    }
                    return me.TrySetResult(default(TResult));

                case TaskStatus.Canceled:
                    return me.TrySetCanceled();

                case TaskStatus.Faulted:
                    return me.TrySetException(source.Exception.InnerExceptions);
            }
            return false;
        }

        public static Task Unwrap(this Task<Task> task)
        {
            bool result;
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }
            TaskCompletionSource<Task> tcs = new TaskCompletionSource<Task>(task.CreationOptions & TaskCreationOptions.AttachedToParent);
            task.ContinueWith(delegate {
                Action<Task> continuationAction = null;
                Action<Task> action2 = null;
                switch (task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        if (task.Result != null)
                        {
                            if (action2 == null)
                            {
                                action2 = _ => result = tcs.TrySetFromTask<Task>(task.Result);
                            }
                            if (continuationAction == null)
                            {
                                continuationAction = antecedent => tcs.TrySetException(antecedent.Exception);
                            }
                            task.Result.ContinueWith(action2, TaskContinuationOptions.ExecuteSynchronously).ContinueWith(continuationAction, TaskContinuationOptions.OnlyOnFaulted);
                            return;
                        }
                        tcs.TrySetCanceled();
                        return;

                    case TaskStatus.Canceled:
                    case TaskStatus.Faulted:
                        result = tcs.TrySetFromTask<Task>(task);
                        return;
                }
            }, TaskContinuationOptions.ExecuteSynchronously).ContinueWith(delegate (Task antecedent) {
                tcs.TrySetException(antecedent.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);
            return tcs.Task;
        }

        public static Task<TResult> Unwrap<TResult>(this Task<Task<TResult>> task)
        {
            bool result;
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>(task.CreationOptions & TaskCreationOptions.AttachedToParent);
            task.ContinueWith(delegate {
                Action<Task> continuationAction = null;
                Action<Task<TResult>> action2 = null;
                switch (task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        if (task.Result != null)
                        {
                            if (action2 == null)
                            {
                                action2 = _ => base.result = base.tcs.TrySetFromTask<TResult>(base.task.Result);
                            }
                            if (continuationAction == null)
                            {
                                continuationAction = antecedent => base.tcs.TrySetException(antecedent.Exception);
                            }
                            task.Result.ContinueWith(action2, TaskContinuationOptions.ExecuteSynchronously).ContinueWith(continuationAction, TaskContinuationOptions.OnlyOnFaulted);
                            return;
                        }
                        tcs.TrySetCanceled();
                        return;

                    case TaskStatus.Canceled:
                    case TaskStatus.Faulted:
                        result = tcs.TrySetFromTask<TResult>(task);
                        return;
                }
            }, TaskContinuationOptions.ExecuteSynchronously).ContinueWith(delegate (Task antecedent) {
                tcs.TrySetException(antecedent.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);
            return tcs.Task;
        }
    }
}

