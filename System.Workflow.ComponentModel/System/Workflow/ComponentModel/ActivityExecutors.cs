namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Threading;

    internal static class ActivityExecutors
    {
        private static Hashtable executors = new Hashtable();
        private static Hashtable typeToExecutorMapping = new Hashtable();

        public static ActivityExecutor GetActivityExecutor(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            return GetActivityExecutors(activity)[0];
        }

        public static ActivityExecutor GetActivityExecutorFromType(Type executorType)
        {
            if (executorType == null)
            {
                throw new ArgumentNullException("executorType");
            }
            if (!typeof(ActivityExecutor).IsAssignableFrom(executorType))
            {
                throw new ArgumentException(SR.GetString("Error_NonActivityExecutor", new object[] { executorType.FullName }), "executorType");
            }
            ActivityExecutor executor = typeToExecutorMapping[executorType] as ActivityExecutor;
            if (executor != null)
            {
                return executor;
            }
            lock (typeToExecutorMapping.SyncRoot)
            {
                executor = typeToExecutorMapping[executorType] as ActivityExecutor;
                if (executor != null)
                {
                    return executor;
                }
                Thread.MemoryBarrier();
                typeToExecutorMapping[executorType] = Activator.CreateInstance(executorType);
            }
            return (ActivityExecutor) typeToExecutorMapping[executorType];
        }

        internal static ActivityExecutor[] GetActivityExecutors(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            Type type = activity.GetType();
            ActivityExecutor[] executorArray = executors[type] as ActivityExecutor[];
            if (executorArray == null)
            {
                lock (executors.SyncRoot)
                {
                    executorArray = executors[type] as ActivityExecutor[];
                    if (executorArray != null)
                    {
                        return executorArray;
                    }
                    object[] objArray = null;
                    try
                    {
                        objArray = ComponentDispenser.CreateActivityExecutors(activity);
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException(SR.GetString("ExecutorCreationFailedErrorMessage", new object[] { type.FullName }), exception);
                    }
                    if ((objArray == null) || (objArray.Length == 0))
                    {
                        throw new InvalidOperationException(SR.GetString("ExecutorCreationFailedErrorMessage", new object[] { type.FullName }));
                    }
                    executorArray = new ActivityExecutor[objArray.Length];
                    for (int i = 0; i < objArray.Length; i++)
                    {
                        if (!typeToExecutorMapping.Contains(objArray[i].GetType()))
                        {
                            lock (typeToExecutorMapping.SyncRoot)
                            {
                                if (!typeToExecutorMapping.Contains(objArray[i].GetType()))
                                {
                                    Thread.MemoryBarrier();
                                    typeToExecutorMapping[objArray[i].GetType()] = objArray[i];
                                }
                            }
                        }
                        executorArray[i] = (ActivityExecutor) typeToExecutorMapping[objArray[i].GetType()];
                    }
                    Thread.MemoryBarrier();
                    executors[type] = executorArray;
                }
            }
            return executorArray;
        }
    }
}

