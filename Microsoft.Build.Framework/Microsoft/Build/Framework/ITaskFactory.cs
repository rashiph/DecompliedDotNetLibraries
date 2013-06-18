namespace Microsoft.Build.Framework
{
    using System;
    using System.Collections.Generic;

    public interface ITaskFactory
    {
        void CleanupTask(ITask task);
        ITask CreateTask(IBuildEngine taskFactoryLoggingHost);
        TaskPropertyInfo[] GetTaskParameters();
        bool Initialize(string taskName, IDictionary<string, TaskPropertyInfo> parameterGroup, string taskBody, IBuildEngine taskFactoryLoggingHost);

        string FactoryName { get; }

        Type TaskType { get; }
    }
}

