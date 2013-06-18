namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.Runtime.DebugEngine;

    [Designer(typeof(ConditionedActivityGroupDesigner), typeof(IDesigner)), ActivityValidator(typeof(ConditionedActivityGroupValidator)), ToolboxItem(typeof(ActivityToolboxItem)), ToolboxBitmap(typeof(ConditionedActivityGroup), "Resources.cag.png"), SRDescription("ConstrainedGroupActivityDescription"), SRCategory("Standard"), WorkflowDebuggerStepping(WorkflowDebuggerSteppingOption.Concurrent)]
    public sealed class ConditionedActivityGroup : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        private static DependencyProperty CAGStateProperty = DependencyProperty.Register("CAGState", typeof(ConditionedActivityGroupStateInfo), typeof(ConditionedActivityGroup));
        public static readonly DependencyProperty UntilConditionProperty = DependencyProperty.Register("UntilCondition", typeof(ActivityCondition), typeof(ConditionedActivityGroup), new PropertyMetadata(DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty WhenConditionProperty = DependencyProperty.RegisterAttached("WhenCondition", typeof(ActivityCondition), typeof(ConditionedActivityGroup), new PropertyMetadata(DependencyPropertyOptions.Metadata), typeof(WhenUnlessConditionDynamicPropertyValidator));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConditionedActivityGroup()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConditionedActivityGroup(string name) : base(name)
        {
        }

        private bool AllChildrenQuiet(ConditionedActivityGroup cag, ActivityExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            foreach (ActivityExecutionContext context2 in context.ExecutionContextManager.ExecutionContexts)
            {
                if (cag.GetActivityByName(context2.Activity.QualifiedName, true) != null)
                {
                    return false;
                }
            }
            return true;
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if ((this.CAGState != null) && !this.Cleanup(this, executionContext))
            {
                return ActivityExecutionStatus.Canceling;
            }
            return ActivityExecutionStatus.Closed;
        }

        internal bool Cleanup(ConditionedActivityGroup cag, ActivityExecutionContext context)
        {
            ConditionedActivityGroupStateInfo cAGState = cag.CAGState;
            cAGState.Completed = true;
            bool flag = false;
            Dictionary<string, CAGChildStats> childrenStats = cAGState.ChildrenStats;
            foreach (Activity activity in cag.EnabledActivities)
            {
                if (childrenStats[activity.QualifiedName].State == CAGChildState.Pending)
                {
                    childrenStats[activity.QualifiedName].State = CAGChildState.Idle;
                }
                ActivityExecutionContext context2 = GetChildExecutionContext(context, activity, false);
                if (context2 != null)
                {
                    Activity runtimeInitializedActivity = this.GetRuntimeInitializedActivity(context, activity);
                    switch (runtimeInitializedActivity.ExecutionStatus)
                    {
                        case ActivityExecutionStatus.Executing:
                        {
                            context2.CancelActivity(runtimeInitializedActivity);
                            flag = true;
                            continue;
                        }
                        case ActivityExecutionStatus.Canceling:
                        case ActivityExecutionStatus.Faulting:
                        {
                            flag = true;
                            continue;
                        }
                        case ActivityExecutionStatus.Closed:
                        {
                            this.CleanupChildAtClosure(context, runtimeInitializedActivity);
                            continue;
                        }
                    }
                    activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
                }
            }
            if (!flag)
            {
                context.CloseActivity();
            }
            return !flag;
        }

        private void CleanupChildAtClosure(ActivityExecutionContext context, Activity childActivity)
        {
            childActivity.UnregisterForStatusChange(Activity.ClosedEvent, this);
            ActivityExecutionContext childContext = GetChildExecutionContext(context, childActivity, false);
            context.ExecutionContextManager.CompleteExecutionContext(childContext);
        }

        private bool EvaluateChildConditions(ConditionedActivityGroup cag, Activity child, ActivityExecutionContext context)
        {
            bool flag;
            ConditionedActivityGroupStateInfo cAGState = cag.CAGState;
            try
            {
                cAGState.Testing = true;
                ActivityCondition condition = (ActivityCondition) child.GetValue(WhenConditionProperty);
                flag = (condition != null) ? condition.Evaluate(child, context) : (cAGState.ChildrenStats[child.QualifiedName].ExecutedCount == 0);
            }
            finally
            {
                cAGState.Testing = false;
            }
            return flag;
        }

        internal bool EvaluateConditions(ConditionedActivityGroup cag, ActivityExecutionContext context)
        {
            if (cag.CAGState.Completed)
            {
                return false;
            }
            if ((cag.UntilCondition == null) || !cag.UntilCondition.Evaluate(cag, context))
            {
                int num = 0;
                Dictionary<string, CAGChildStats> childrenStats = cag.CAGState.ChildrenStats;
                foreach (Activity activity in cag.EnabledActivities)
                {
                    if (childrenStats[activity.QualifiedName].State == CAGChildState.Excuting)
                    {
                        num++;
                    }
                    else
                    {
                        Activity runtimeInitializedActivity = this.GetRuntimeInitializedActivity(context, activity);
                        if (this.EvaluateChildConditions(cag, runtimeInitializedActivity, context))
                        {
                            num++;
                            childrenStats[activity.QualifiedName].State = CAGChildState.Pending;
                        }
                    }
                }
                if (num > 0)
                {
                    return false;
                }
                if (cag.UntilCondition != null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CAGQuiet", new object[] { cag.QualifiedName }));
                }
            }
            return true;
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            this.CAGState = new ConditionedActivityGroupStateInfo(this);
            if (this.EvaluateConditions(this, executionContext))
            {
                return ActivityExecutionStatus.Closed;
            }
            this.TriggerChildren(this, executionContext);
            return base.ExecutionStatus;
        }

        private void ExecuteChild(ConditionedActivityGroup cag, Activity childActivity, ActivityExecutionContext context)
        {
            ActivityExecutionContext context2 = GetChildExecutionContext(context, childActivity, true);
            cag.CAGState.ChildrenStats[childActivity.QualifiedName].State = CAGChildState.Excuting;
            context2.Activity.RegisterForStatusChange(Activity.ClosedEvent, this);
            context2.ExecuteActivity(context2.Activity);
        }

        public int GetChildActivityExecutedCount(Activity child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }
            ConditionedActivityGroupStateInfo cAGState = this.CAGState;
            if (cAGState == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_CAGNotExecuting", new object[] { base.QualifiedName }));
            }
            if (!cAGState.ChildrenStats.ContainsKey(child.QualifiedName))
            {
                throw new ArgumentException(SR.GetString("Error_CAGChildNotFound", new object[] { child.QualifiedName, base.QualifiedName }), "child");
            }
            return cAGState.ChildrenStats[child.QualifiedName].ExecutedCount;
        }

        private static ActivityExecutionContext GetChildExecutionContext(ActivityExecutionContext context, Activity childActivity, bool createIfNotExists)
        {
            ActivityExecutionContextManager executionContextManager = context.ExecutionContextManager;
            ActivityExecutionContext executionContext = executionContextManager.GetExecutionContext(childActivity);
            if ((executionContext == null) && createIfNotExists)
            {
                executionContext = executionContextManager.CreateExecutionContext(childActivity);
            }
            return executionContext;
        }

        public Activity GetDynamicActivity(string childActivityName)
        {
            if (childActivityName == null)
            {
                throw new ArgumentNullException("childActivityName");
            }
            Activity childActivity = null;
            for (int i = 0; i < base.EnabledActivities.Count; i++)
            {
                if (base.EnabledActivities[i].QualifiedName.Equals(childActivityName))
                {
                    childActivity = base.EnabledActivities[i];
                    break;
                }
            }
            if (childActivity == null)
            {
                throw new ArgumentException(SR.GetString("Error_CAGChildNotFound", new object[] { childActivityName, base.QualifiedName }), "childActivityName");
            }
            return this.GetDynamicActivity(childActivity);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Activity GetDynamicActivity(Activity childActivity)
        {
            if (childActivity == null)
            {
                throw new ArgumentNullException("childActivity");
            }
            if (!base.EnabledActivities.Contains(childActivity))
            {
                throw new ArgumentException(SR.GetString("Error_CAGChildNotFound", new object[] { childActivity.QualifiedName, base.QualifiedName }), "childActivity");
            }
            Activity[] dynamicActivities = base.GetDynamicActivities(childActivity);
            if (dynamicActivities.Length != 0)
            {
                return dynamicActivities[0];
            }
            return null;
        }

        private Activity GetRuntimeInitializedActivity(ActivityExecutionContext context, Activity childActivity)
        {
            ActivityExecutionContext context2 = GetChildExecutionContext(context, childActivity, false);
            if (context2 == null)
            {
                return childActivity;
            }
            return context2.Activity;
        }

        public static object GetWhenCondition(object dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException("dependencyObject");
            }
            if (!(dependencyObject is DependencyObject))
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(DependencyObject).FullName }), "dependencyObject");
            }
            return (dependencyObject as DependencyObject).GetValue(WhenConditionProperty);
        }

        internal void HandleEvent(ActivityExecutionContext context, SubscriptionEventArg e)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            ConditionedActivityGroup activity = context.Activity as ConditionedActivityGroup;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_InvalidCAGActivityType"), "activity");
            }
            if (activity.ExecutionStatus != ActivityExecutionStatus.Closed)
            {
                EventType subscriptionType = e.SubscriptionType;
                ActivityExecutionStatusChangedEventArgs args = (ActivityExecutionStatusChangedEventArgs) e.Args;
                bool flag = false;
                Dictionary<string, CAGChildStats> childrenStats = activity.CAGState.ChildrenStats;
                if (childrenStats.ContainsKey(args.Activity.QualifiedName))
                {
                    if (args.ExecutionStatus != ActivityExecutionStatus.Executing)
                    {
                        childrenStats[args.Activity.QualifiedName].State = CAGChildState.Idle;
                    }
                    if (args.ExecutionStatus == ActivityExecutionStatus.Closed)
                    {
                        CAGChildStats local1 = childrenStats[args.Activity.QualifiedName];
                        local1.ExecutedCount++;
                    }
                    try
                    {
                        if (activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                        {
                            flag = this.EvaluateConditions(activity, context);
                        }
                    }
                    finally
                    {
                        this.CleanupChildAtClosure(context, args.Activity);
                    }
                }
                else if (activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    flag = this.EvaluateConditions(activity, context);
                }
                if (flag)
                {
                    this.Cleanup(activity, context);
                }
                else if (activity.CAGState.Completed)
                {
                    if (this.AllChildrenQuiet(activity, context))
                    {
                        context.CloseActivity();
                    }
                }
                else
                {
                    this.TriggerChildren(activity, context);
                }
            }
        }

        protected override void OnActivityChangeAdd(ActivityExecutionContext executionContext, Activity addedActivity)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (addedActivity == null)
            {
                throw new ArgumentNullException("addedActivity");
            }
            if (addedActivity.Enabled)
            {
                ConditionedActivityGroup activity = executionContext.Activity as ConditionedActivityGroup;
                ConditionedActivityGroupStateInfo cAGState = activity.CAGState;
                if ((activity.ExecutionStatus == ActivityExecutionStatus.Executing) && (cAGState != null))
                {
                    cAGState.ChildrenStats[addedActivity.QualifiedName] = new CAGChildStats();
                }
            }
        }

        protected override void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (removedActivity == null)
            {
                throw new ArgumentNullException("removedActivity");
            }
            if (removedActivity.Enabled)
            {
                ConditionedActivityGroup activity = executionContext.Activity as ConditionedActivityGroup;
                ConditionedActivityGroupStateInfo cAGState = activity.CAGState;
                if ((activity.ExecutionStatus == ActivityExecutionStatus.Executing) && (cAGState != null))
                {
                    cAGState.ChildrenStats.Remove(removedActivity.QualifiedName);
                }
            }
        }

        protected override void OnClosed(IServiceProvider provider)
        {
            base.RemoveProperty(CAGStateProperty);
        }

        protected override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            ConditionedActivityGroup activity = executionContext.Activity as ConditionedActivityGroup;
            if (activity.ExecutionStatus == ActivityExecutionStatus.Executing)
            {
                ConditionedActivityGroupStateInfo cAGState = activity.CAGState;
                if ((cAGState != null) && !cAGState.Testing)
                {
                    if (this.EvaluateConditions(activity, executionContext))
                    {
                        this.Cleanup(activity, executionContext);
                    }
                    else
                    {
                        this.TriggerChildren(activity, executionContext);
                    }
                }
            }
        }

        public static void SetWhenCondition(object dependencyObject, object value)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException("dependencyObject");
            }
            if (!(dependencyObject is DependencyObject))
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(DependencyObject).FullName }), "dependencyObject");
            }
            (dependencyObject as DependencyObject).SetValue(WhenConditionProperty, value);
        }

        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            this.HandleEvent(sender as ActivityExecutionContext, new SubscriptionEventArg(e, EventType.StatusChange));
        }

        internal void TriggerChildren(ConditionedActivityGroup cag, ActivityExecutionContext context)
        {
            Dictionary<string, CAGChildStats> childrenStats = cag.CAGState.ChildrenStats;
            foreach (Activity activity in cag.EnabledActivities)
            {
                if (childrenStats[activity.QualifiedName].State == CAGChildState.Pending)
                {
                    Activity runtimeInitializedActivity = this.GetRuntimeInitializedActivity(context, activity);
                    if (runtimeInitializedActivity.ExecutionStatus == ActivityExecutionStatus.Initialized)
                    {
                        this.ExecuteChild(cag, runtimeInitializedActivity, context);
                    }
                }
            }
        }

        internal ConditionedActivityGroupStateInfo CAGState
        {
            get
            {
                return (ConditionedActivityGroupStateInfo) base.GetValue(CAGStateProperty);
            }
            set
            {
                base.SetValue(CAGStateProperty, value);
            }
        }

        [SRDescription("UntilConditionDescr"), DefaultValue((string) null), SRCategory("Conditions")]
        public ActivityCondition UntilCondition
        {
            get
            {
                return (base.GetValue(UntilConditionProperty) as ActivityCondition);
            }
            set
            {
                base.SetValue(UntilConditionProperty, value);
            }
        }

        private sealed class WhenUnlessConditionDynamicPropertyValidator : Validator
        {
            public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
            {
                ValidationErrorCollection errors = System.Workflow.Activities.Common.ValidationHelpers.ValidateObject(manager, obj);
                if (errors.Count == 0)
                {
                    Activity context = manager.Context[typeof(Activity)] as Activity;
                    if (context == null)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_ContextStackItemMissing", new object[] { typeof(Activity).Name }));
                    }
                    CodeCondition condition = obj as CodeCondition;
                    if ((condition != null) && condition.IsBindingSet(CodeCondition.ConditionEvent))
                    {
                        ActivityBind binding = condition.GetBinding(CodeCondition.ConditionEvent);
                        if (binding != null)
                        {
                            Activity activity = System.Workflow.Activities.Common.Helpers.ParseActivityForBind(context, binding.Name);
                            if ((activity != null) && System.Workflow.Activities.Common.Helpers.IsChildActivity(context.Parent, activity))
                            {
                                string fullPropertyName = base.GetFullPropertyName(manager);
                                ValidationError item = new ValidationError(SR.GetString("Error_NestedConstrainedGroupConditions", new object[] { fullPropertyName }), 0x615) {
                                    PropertyName = fullPropertyName
                                };
                                errors.Add(item);
                            }
                        }
                    }
                }
                return errors;
            }
        }
    }
}

