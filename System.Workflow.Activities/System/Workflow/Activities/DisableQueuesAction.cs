namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;

    [Serializable]
    internal class DisableQueuesAction : StateMachineAction
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DisableQueuesAction(string stateName) : base(stateName)
        {
        }

        internal override void Execute(ActivityExecutionContext context)
        {
            base.Execute(context);
            StateActivity rootState = StateMachineHelpers.GetRootState(base.State);
            Queue<StateActivity> queue = new Queue<StateActivity>();
            queue.Enqueue(rootState);
            while (queue.Count > 0)
            {
                foreach (Activity activity3 in queue.Dequeue().EnabledActivities)
                {
                    EventDrivenActivity eventDriven = activity3 as EventDrivenActivity;
                    if (eventDriven != null)
                    {
                        IComparable queueName = StateMachineHelpers.GetEventActivity(eventDriven).QueueName;
                        if (queueName != null)
                        {
                            WorkflowQueue workflowQueue = StateMachineSubscriptionManager.GetWorkflowQueue(context, queueName);
                            if (workflowQueue != null)
                            {
                                workflowQueue.Enabled = base.SubscriptionManager.Subscriptions.ContainsKey(queueName);
                            }
                        }
                    }
                    else
                    {
                        StateActivity item = activity3 as StateActivity;
                        if (item != null)
                        {
                            queue.Enqueue(item);
                        }
                    }
                }
            }
        }
    }
}

