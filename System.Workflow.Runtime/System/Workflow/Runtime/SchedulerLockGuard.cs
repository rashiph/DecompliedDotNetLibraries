namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.Generic;

    internal sealed class SchedulerLockGuard : IDisposable
    {
        private InstanceLock.InstanceLockGuard lg;
        private WorkflowExecutor workflowExec;

        internal SchedulerLockGuard(InstanceLock il, WorkflowExecutor w)
        {
            this.lg = il.Enter();
            this.workflowExec = w;
        }

        public void Dispose()
        {
            List<SchedulerLockGuardInfo> eventList = new List<SchedulerLockGuardInfo>(this.workflowExec.EventsToFireList);
            this.workflowExec.EventsToFireList.Clear();
            this.lg.Dispose();
            FireEvents(eventList, this.workflowExec);
        }

        internal static void Exit(InstanceLock il, WorkflowExecutor w)
        {
            List<SchedulerLockGuardInfo> eventList = new List<SchedulerLockGuardInfo>(w.EventsToFireList);
            w.EventsToFireList.Clear();
            il.Exit();
            FireEvents(eventList, w);
        }

        private static void FireEvents(List<SchedulerLockGuardInfo> eventList, WorkflowExecutor workflowExec)
        {
            if (!workflowExec.IsInstanceValid && ((workflowExec.WorkflowStatus == WorkflowStatus.Completed) || (workflowExec.WorkflowStatus == WorkflowStatus.Terminated)))
            {
                workflowExec.WorkflowInstance.DeadWorkflow = workflowExec;
            }
            for (int i = 0; i < eventList.Count; i++)
            {
                SchedulerLockGuardInfo info = eventList[i];
                WorkflowEventInternal eventType = info.EventType;
                if (eventType != WorkflowEventInternal.Suspended)
                {
                    if (eventType == WorkflowEventInternal.Terminated)
                    {
                        goto Label_0057;
                    }
                    goto Label_008A;
                }
                workflowExec.FireWorkflowSuspended((string) info.EventInfo);
                continue;
            Label_0057:
                if (info.EventInfo is Exception)
                {
                    workflowExec.FireWorkflowTerminated((Exception) info.EventInfo);
                }
                else
                {
                    workflowExec.FireWorkflowTerminated((string) info.EventInfo);
                }
                continue;
            Label_008A:
                workflowExec.FireWorkflowExecutionEvent(info.Sender, info.EventType);
            }
        }
    }
}

