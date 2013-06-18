namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    [SRDescription("CompensateActivityDescription"), SRCategory("Standard"), ToolboxItem(typeof(ActivityToolboxItem)), Designer(typeof(CompensateDesigner), typeof(IDesigner)), ToolboxBitmap(typeof(CompensateActivity), "Resources.Compensate.png"), ActivityValidator(typeof(CompensateValidator))]
    public sealed class CompensateActivity : Activity, IPropertyValueProvider, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        public static readonly DependencyProperty TargetActivityNameProperty = DependencyProperty.Register("TargetActivityName", typeof(string), typeof(CompensateActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CompensateActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CompensateActivity(string name) : base(name)
        {
        }

        private ActivityExecutionStatus CompensateTargetActivity(ActivityExecutionContext context)
        {
            Activity activityByName = null;
            Activity activity2 = context.Activity;
            do
            {
                activityByName = activity2.Parent.GetActivityByName(this.TargetActivityName, true);
            }
            while (activityByName == null);
            if (((activityByName is ICompensatableActivity) && (activityByName.ExecutionStatus == ActivityExecutionStatus.Closed)) && (activityByName.ExecutionResult == ActivityExecutionResult.Succeeded))
            {
                activityByName.RegisterForStatusChange(Activity.ClosedEvent, this);
                context.CompensateActivity(activityByName);
                return context.Activity.ExecutionStatus;
            }
            if (activityByName.ExecutionStatus == ActivityExecutionStatus.Initialized)
            {
                ActivityExecutionContextManager executionContextManager = context.ExecutionContextManager;
                foreach (ActivityExecutionContext context2 in executionContextManager.ExecutionContexts)
                {
                    if ((activityByName.GetActivityByName(context2.Activity.QualifiedName, true) != null) && (((context2.Activity.ExecutionStatus == ActivityExecutionStatus.Compensating) || (context2.Activity.ExecutionStatus == ActivityExecutionStatus.Faulting)) || (context2.Activity.ExecutionStatus == ActivityExecutionStatus.Canceling)))
                    {
                        return context.Activity.ExecutionStatus;
                    }
                }
                for (int i = executionContextManager.CompletedExecutionContexts.Count - 1; i >= 0; i--)
                {
                    ActivityExecutionContextInfo contextInfo = executionContextManager.CompletedExecutionContexts[i];
                    if (((byte) (contextInfo.Flags & PersistFlags.NeedsCompensation)) != 0)
                    {
                        ActivityExecutionContext context3 = executionContextManager.DiscardPersistedExecutionContext(contextInfo);
                        if (context3.Activity is ICompensatableActivity)
                        {
                            context3.Activity.RegisterForStatusChange(Activity.ClosedEvent, this);
                            context3.CompensateActivity(context3.Activity);
                        }
                        return context.Activity.ExecutionStatus;
                    }
                }
            }
            else if (CompensationUtils.TryCompensateLastCompletedChildActivity(context, activityByName, this))
            {
                return context.Activity.ExecutionStatus;
            }
            return ActivityExecutionStatus.Closed;
        }

        protected internal override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            return this.CompensateTargetActivity(executionContext);
        }

        internal static StringCollection GetCompensatableTargets(CompensateActivity compensate)
        {
            StringCollection strings = new StringCollection();
            for (CompositeActivity parent = compensate.Parent; parent != null; parent = parent.Parent)
            {
                if (((parent is CompensationHandlerActivity) || (parent is FaultHandlersActivity)) || (parent is CancellationHandlerActivity))
                {
                    parent = parent.Parent;
                    if (parent != null)
                    {
                        if (Helpers.IsCustomActivity(parent))
                        {
                            strings.Add(parent.UserData[UserDataKeys.CustomActivityDefaultName] as string);
                        }
                        else
                        {
                            strings.Add(parent.Name);
                        }
                        foreach (Activity activity2 in parent.EnabledActivities)
                        {
                            if (activity2 is ICompensatableActivity)
                            {
                                strings.Add(activity2.Name);
                            }
                        }
                    }
                    return strings;
                }
            }
            return strings;
        }

        protected internal override void Initialize(IServiceProvider provider)
        {
            if (base.Parent == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_MustHaveParent"));
            }
            base.Initialize(provider);
        }

        ICollection IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            return GetCompensatableTargets(this);
        }

        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
            {
                throw new ArgumentException("Error_SenderMustBeActivityExecutionContext", "sender");
            }
            if (e.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
                if (this.CompensateTargetActivity(context) == ActivityExecutionStatus.Closed)
                {
                    context.CloseActivity();
                }
            }
        }

        [SRDescription("CompensatableActivityDescr"), DefaultValue(""), SRCategory("Activity"), TypeConverter(typeof(PropertyValueProviderTypeConverter)), MergableProperty(false)]
        public string TargetActivityName
        {
            get
            {
                return (base.GetValue(TargetActivityNameProperty) as string);
            }
            set
            {
                base.SetValue(TargetActivityNameProperty, value);
            }
        }
    }
}

