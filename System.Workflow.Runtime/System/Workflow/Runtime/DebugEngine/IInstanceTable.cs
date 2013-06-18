namespace System.Workflow.Runtime.DebugEngine
{
    using System;
    using System.Workflow.ComponentModel;

    public interface IInstanceTable
    {
        Activity GetActivity(string instanceId, string activityName);
    }
}

