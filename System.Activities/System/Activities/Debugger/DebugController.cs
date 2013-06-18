namespace System.Activities.Debugger
{
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Diagnostics;

    [DebuggerNonUserCode]
    internal class DebugController
    {
        private DebugManager debugManager;
        private WorkflowInstance host;

        public DebugController(WorkflowInstance host)
        {
            this.host = host;
        }

        public void ActivityCompleted(System.Activities.ActivityInstance activityInstance)
        {
            if (!(activityInstance.Activity.RootActivity is Constraint))
            {
                this.EnsureActivityInstrumented(activityInstance, true);
                this.debugManager.OnLeaveState(activityInstance);
            }
        }

        public void ActivityStarted(System.Activities.ActivityInstance activityInstance)
        {
            if (!(activityInstance.Activity.RootActivity is Constraint))
            {
                this.EnsureActivityInstrumented(activityInstance, false);
                this.debugManager.OnEnterState(activityInstance);
            }
        }

        private void EnsureActivityInstrumented(System.Activities.ActivityInstance instance, bool primeCurrentInstance)
        {
            if (this.debugManager == null)
            {
                Stack<System.Activities.ActivityInstance> stack = new Stack<System.Activities.ActivityInstance>();
                while (instance.Parent != null)
                {
                    stack.Push(instance);
                    instance = instance.Parent;
                }
                Activity root = instance.Activity;
                this.debugManager = new DebugManager(root, "Workflow", "Workflow", "DebuggerThread", false, this.host, stack.Count == 0);
                if (stack.Count > 0)
                {
                    this.debugManager.IsPriming = true;
                    while (stack.Count > 0)
                    {
                        System.Activities.ActivityInstance instance2 = stack.Pop();
                        this.debugManager.OnEnterState(instance2);
                    }
                    if (primeCurrentInstance)
                    {
                        this.debugManager.OnEnterState(instance);
                    }
                    this.debugManager.IsPriming = false;
                }
            }
        }

        public void WorkflowCompleted()
        {
            if (this.debugManager != null)
            {
                this.debugManager.Exit();
                this.debugManager = null;
            }
        }

        public void WorkflowStarted()
        {
        }
    }
}

