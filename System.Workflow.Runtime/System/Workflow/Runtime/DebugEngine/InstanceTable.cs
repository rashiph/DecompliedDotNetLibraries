namespace System.Workflow.Runtime.DebugEngine
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    internal sealed class InstanceTable : BreakSafeBase<InstanceMap>, IInstanceTable
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstanceTable(int controllerManagedThreadId) : base(controllerManagedThreadId)
        {
        }

        public void AddInstance(Guid instanceId, Activity rootActivity)
        {
            try
            {
                base.Lock();
                InstanceMap writerData = base.GetWriterData();
                writerData[instanceId] = new InstanceData(rootActivity);
                base.SaveData(writerData);
            }
            finally
            {
                base.Unlock();
            }
        }

        public Activity GetRootActivity(Guid instanceId)
        {
            Activity rootActivity;
            try
            {
                base.Lock();
                rootActivity = base.GetReaderData()[instanceId].RootActivity;
            }
            finally
            {
                base.Unlock();
            }
            return rootActivity;
        }

        public void RemoveInstance(Guid instanceId)
        {
            try
            {
                base.Lock();
                InstanceMap writerData = base.GetWriterData();
                writerData.Remove(instanceId);
                base.SaveData(writerData);
            }
            finally
            {
                base.Unlock();
            }
        }

        Activity IInstanceTable.GetActivity(string instanceId, string activityQualifiedName)
        {
            Activity activity2;
            try
            {
                base.Lock();
                activity2 = DebuggerHelpers.ParseActivity(base.GetReaderData()[new Guid(instanceId)].RootActivity, activityQualifiedName);
            }
            finally
            {
                base.Unlock();
            }
            return activity2;
        }

        public void UpdateRootActivity(Guid instanceId, Activity rootActivity)
        {
            try
            {
                base.Lock();
                InstanceMap writerData = base.GetWriterData();
                writerData[instanceId].RootActivity = rootActivity;
                base.SaveData(writerData);
            }
            finally
            {
                base.Unlock();
            }
        }
    }
}

