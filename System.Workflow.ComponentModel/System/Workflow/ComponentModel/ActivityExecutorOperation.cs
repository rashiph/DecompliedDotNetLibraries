namespace System.Workflow.ComponentModel
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.Runtime;

    [Serializable]
    internal sealed class ActivityExecutorOperation : SchedulableItem
    {
        private string activityName;
        private Exception exceptionToDeliver;
        private ActivityOperationType operation;

        public ActivityExecutorOperation(Activity activity, ActivityOperationType opt, int contextId) : base(contextId, activity.QualifiedName)
        {
            this.activityName = activity.QualifiedName;
            this.operation = opt;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityExecutorOperation(Activity activity, ActivityOperationType opt, int contextId, Exception e) : this(activity, opt, contextId)
        {
            this.exceptionToDeliver = e;
        }

        private string ActivityOperationToString(ActivityOperationType operationType)
        {
            string str = string.Empty;
            switch (operationType)
            {
                case ActivityOperationType.Execute:
                    return "Execute";

                case ActivityOperationType.Cancel:
                    return "Cancel";

                case ActivityOperationType.Compensate:
                    return "Compensate";

                case ActivityOperationType.HandleFault:
                    return "HandleFault";
            }
            return str;
        }

        public override bool Run(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            Activity activityByName = workflowCoreRuntime.GetContextActivityForId(base.ContextId).GetActivityByName(this.activityName);
            using (workflowCoreRuntime.SetCurrentActivity(activityByName))
            {
                using (ActivityExecutionContext context = new ActivityExecutionContext(activityByName))
                {
                    ActivityExecutor activityExecutor = ActivityExecutors.GetActivityExecutor(activityByName);
                    switch (this.operation)
                    {
                        case ActivityOperationType.Execute:
                            if (activityByName.ExecutionStatus != ActivityExecutionStatus.Executing)
                            {
                                goto Label_0309;
                            }
                            try
                            {
                                workflowCoreRuntime.RaiseActivityExecuting(activityByName);
                                ActivityExecutionStatus status = activityExecutor.Execute(activityByName, context);
                                if (status == ActivityExecutionStatus.Closed)
                                {
                                    context.CloseActivity();
                                }
                                else if (status != ActivityExecutionStatus.Executing)
                                {
                                    throw new InvalidOperationException(SR.GetString("InvalidExecutionStatus", new object[] { activityByName.QualifiedName, status.ToString(), ActivityExecutionStatus.Executing.ToString() }));
                                }
                                goto Label_0309;
                            }
                            catch (Exception exception)
                            {
                                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 1, "Execute of Activity {0} threw {1}", new object[] { activityByName.QualifiedName, exception.ToString() });
                                throw;
                            }
                            break;

                        case ActivityOperationType.Cancel:
                            break;

                        case ActivityOperationType.Compensate:
                            goto Label_01A4;

                        case ActivityOperationType.HandleFault:
                            goto Label_0248;

                        default:
                            goto Label_0309;
                    }
                    if (activityByName.ExecutionStatus != ActivityExecutionStatus.Canceling)
                    {
                        goto Label_0309;
                    }
                    try
                    {
                        ActivityExecutionStatus status2 = activityExecutor.Cancel(activityByName, context);
                        if (status2 == ActivityExecutionStatus.Closed)
                        {
                            context.CloseActivity();
                        }
                        else if (status2 != ActivityExecutionStatus.Canceling)
                        {
                            throw new InvalidOperationException(SR.GetString("InvalidExecutionStatus", new object[] { activityByName.QualifiedName, status2.ToString(), ActivityExecutionStatus.Canceling.ToString() }));
                        }
                        goto Label_0309;
                    }
                    catch (Exception exception2)
                    {
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 1, "Cancel of Activity {0} threw {1}", new object[] { activityByName.QualifiedName, exception2.ToString() });
                        throw;
                    }
                Label_01A4:
                    if (activityByName.ExecutionStatus != ActivityExecutionStatus.Compensating)
                    {
                        goto Label_0309;
                    }
                    try
                    {
                        ActivityExecutionStatus status3 = activityExecutor.Compensate(activityByName, context);
                        if (status3 == ActivityExecutionStatus.Closed)
                        {
                            context.CloseActivity();
                        }
                        else if (status3 != ActivityExecutionStatus.Compensating)
                        {
                            throw new InvalidOperationException(SR.GetString("InvalidExecutionStatus", new object[] { activityByName.QualifiedName, status3.ToString(), ActivityExecutionStatus.Compensating.ToString() }));
                        }
                        goto Label_0309;
                    }
                    catch (Exception exception3)
                    {
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 1, "Compensate of Activity {0} threw {1}", new object[] { activityByName.QualifiedName, exception3.ToString() });
                        throw;
                    }
                Label_0248:
                    if (activityByName.ExecutionStatus == ActivityExecutionStatus.Faulting)
                    {
                        try
                        {
                            ActivityExecutionStatus status4 = activityExecutor.HandleFault(activityByName, context, this.exceptionToDeliver);
                            if (status4 == ActivityExecutionStatus.Closed)
                            {
                                context.CloseActivity();
                            }
                            else if (status4 != ActivityExecutionStatus.Faulting)
                            {
                                throw new InvalidOperationException(SR.GetString("InvalidExecutionStatus", new object[] { activityByName.QualifiedName, status4.ToString(), ActivityExecutionStatus.Faulting.ToString() }));
                            }
                            goto Label_0309;
                        }
                        catch (Exception exception4)
                        {
                            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 1, "Compensate of Activity {0} threw {1}", new object[] { activityByName.QualifiedName, exception4.ToString() });
                            throw;
                        }
                    }
                }
            }
        Label_0309:
            return true;
        }

        public override string ToString()
        {
            return ("ActivityOperation((" + base.ContextId.ToString(CultureInfo.CurrentCulture) + ")" + this.activityName + ", " + this.ActivityOperationToString(this.operation) + ")");
        }
    }
}

