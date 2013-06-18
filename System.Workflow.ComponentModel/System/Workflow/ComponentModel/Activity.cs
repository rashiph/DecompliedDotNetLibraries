namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Workflow.Runtime;

    [DesignerSerializer(typeof(ActivityTypeCodeDomSerializer), typeof(TypeCodeDomSerializer)), DesignerSerializer(typeof(ActivityCodeDomSerializer), typeof(CodeDomSerializer)), Designer(typeof(ActivityDesigner), typeof(IDesigner)), ActivityExecutor(typeof(ActivityExecutor<Activity>)), RuntimeNameProperty("Name"), Designer(typeof(ActivityDesigner), typeof(IRootDesigner)), ToolboxItem(typeof(ActivityToolboxItem)), DesignerCategory("Component"), ActivityValidator(typeof(ActivityValidator)), ToolboxBitmap(typeof(Activity), "Design.Resources.Activity.png"), DesignerSerializer(typeof(ActivityMarkupSerializer), typeof(WorkflowMarkupSerializer)), ActivityCodeGenerator(typeof(ActivityCodeGenerator)), ToolboxItemFilter("Microsoft.Workflow.VSDesigner", ToolboxItemFilterType.Require), ToolboxItemFilter("System.Workflow.ComponentModel.Design.ActivitySet", ToolboxItemFilterType.Allow)]
    public class Activity : DependencyObject
    {
        internal static readonly DependencyProperty ActiveExecutionContextsProperty = DependencyProperty.RegisterAttached("ActiveExecutionContexts", typeof(IList), typeof(Activity));
        public static readonly DependencyProperty ActivityContextGuidProperty = DependencyProperty.RegisterAttached("ActivityContextGuid", typeof(Guid), typeof(Activity), new PropertyMetadata(Guid.Empty));
        private static ActivityResolveEventHandler activityDefinitionResolve = null;
        internal static readonly DependencyProperty ActivityExecutionContextInfoProperty = DependencyProperty.RegisterAttached("ActivityExecutionContextInfo", typeof(ActivityExecutionContextInfo), typeof(Activity));
        [ThreadStatic]
        internal static ArrayList ActivityRoots = null;
        internal static Type ActivityType = null;
        private static readonly BinaryFormatter binaryFormatter = null;
        [NonSerialized]
        private string cachedDottedPath;
        public static readonly DependencyProperty CancelingEvent = DependencyProperty.Register("Canceling", typeof(EventHandler<ActivityExecutionStatusChangedEventArgs>), typeof(Activity));
        public static readonly DependencyProperty ClosedEvent = DependencyProperty.Register("Closed", typeof(EventHandler<ActivityExecutionStatusChangedEventArgs>), typeof(Activity));
        public static readonly DependencyProperty CompensatingEvent = DependencyProperty.Register("Compensating", typeof(EventHandler<ActivityExecutionStatusChangedEventArgs>), typeof(Activity));
        internal static readonly DependencyProperty CompletedExecutionContextsProperty = DependencyProperty.RegisterAttached("CompletedExecutionContexts", typeof(IList), typeof(Activity));
        internal static readonly DependencyProperty CompletedOrderIdProperty = DependencyProperty.Register("CompletedOrderId", typeof(int), typeof(Activity), new PropertyMetadata(0));
        [ThreadStatic]
        internal static Hashtable ContextIdToActivityMap = null;
        internal static readonly DependencyProperty CustomActivityProperty = DependencyProperty.Register("CustomActivity", typeof(bool), typeof(Activity), new PropertyMetadata(DependencyPropertyOptions.Metadata));
        [ThreadStatic]
        internal static Activity DefinitionActivity = null;
        private static DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(Activity), new PropertyMetadata("", DependencyPropertyOptions.Metadata));
        private static DependencyProperty DottedPathProperty = DependencyProperty.Register("DottedPath", typeof(string), typeof(Activity), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly));
        private static DependencyProperty EnabledProperty = DependencyProperty.Register("Enabled", typeof(bool), typeof(Activity), new PropertyMetadata(true, DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty ExecutingEvent = DependencyProperty.Register("Executing", typeof(EventHandler<ActivityExecutionStatusChangedEventArgs>), typeof(Activity));
        internal static readonly DependencyProperty ExecutionResultProperty = DependencyProperty.RegisterAttached("ExecutionResult", typeof(ActivityExecutionResult), typeof(Activity), new PropertyMetadata(ActivityExecutionResult.None, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        internal static readonly DependencyProperty ExecutionStatusProperty = DependencyProperty.RegisterAttached("ExecutionStatus", typeof(ActivityExecutionStatus), typeof(Activity), new PropertyMetadata(ActivityExecutionStatus.Initialized, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        public static readonly DependencyProperty FaultingEvent = DependencyProperty.Register("Faulting", typeof(EventHandler<ActivityExecutionStatusChangedEventArgs>), typeof(Activity));
        internal static readonly DependencyProperty HasPrimaryClosedProperty = DependencyProperty.RegisterAttached("HasPrimaryClosed", typeof(bool), typeof(Activity), new PropertyMetadata(false));
        internal static readonly DependencyProperty LockCountOnStatusChangeChangedEvent = DependencyProperty.Register("LockCountOnStatusChangeChanged", typeof(EventHandler<ActivityExecutionStatusChangedEventArgs>), typeof(Activity));
        private static readonly DependencyProperty LockCountOnStatusChangeProperty = DependencyProperty.RegisterAttached("LockCountOnStatusChange", typeof(int), typeof(Activity), new PropertyMetadata(0));
        private static DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(Activity), new PropertyMetadata("", DependencyPropertyOptions.Metadata, new Attribute[] { new ValidationOptionAttribute(ValidationOption.Required) }));
        private static readonly DependencyProperty NestedActivitiesProperty = DependencyProperty.RegisterAttached("NestedActivities", typeof(IList<Activity>), typeof(Activity));
        [NonSerialized]
        internal CompositeActivity parent;
        private static DependencyProperty QualifiedNameProperty = DependencyProperty.Register("QualifiedName", typeof(string), typeof(Activity), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly));
        private static readonly DependencyProperty SerializedStreamLengthProperty = DependencyProperty.RegisterAttached("SerializedStreamLength", typeof(long), typeof(Activity), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));
        private static object staticSyncRoot = new object();
        public static readonly DependencyProperty StatusChangedEvent = DependencyProperty.Register("StatusChanged", typeof(EventHandler<ActivityExecutionStatusChangedEventArgs>), typeof(Activity));
        internal static readonly DependencyProperty StatusChangedLockedEvent = DependencyProperty.Register("StatusChangedLocked", typeof(EventHandler<ActivityExecutionStatusChangedEventArgs>), typeof(Activity));
        internal static readonly DependencyProperty SynchronizationHandlesProperty = DependencyProperty.Register("SynchronizationHandles", typeof(ICollection<string>), typeof(Activity), new PropertyMetadata(DependencyPropertyOptions.Metadata));
        internal static readonly DependencyProperty WasExecutingProperty = DependencyProperty.RegisterAttached("WasExecuting", typeof(bool), typeof(Activity), new PropertyMetadata(false, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        private static WorkflowChangeActionsResolveEventHandler workflowChangeActionsResolve = null;
        [NonSerialized]
        private IWorkflowCoreRuntime workflowCoreRuntime;
        internal static readonly DependencyProperty WorkflowDefinitionProperty = DependencyProperty.RegisterAttached("WorkflowDefinition", typeof(Activity), typeof(Activity), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));
        internal static readonly DependencyProperty WorkflowRulesMarkupProperty = DependencyProperty.Register("WorkflowRulesMarkup", typeof(string), typeof(Activity));
        internal static readonly DependencyProperty WorkflowRuntimeProperty = DependencyProperty.RegisterAttached("WorkflowRuntime", typeof(IServiceProvider), typeof(Activity), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));
        internal static readonly DependencyProperty WorkflowXamlMarkupProperty = DependencyProperty.Register("WorkflowXamlMarkup", typeof(string), typeof(Activity));

        internal static  event ActivityResolveEventHandler ActivityResolve
        {
            add
            {
                lock (staticSyncRoot)
                {
                    activityDefinitionResolve = (ActivityResolveEventHandler) Delegate.Combine(activityDefinitionResolve, value);
                }
            }
            remove
            {
                lock (staticSyncRoot)
                {
                    activityDefinitionResolve = (ActivityResolveEventHandler) Delegate.Remove(activityDefinitionResolve, value);
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler<ActivityExecutionStatusChangedEventArgs> Canceling
        {
            add
            {
                this.AddStatusChangeHandler(CancelingEvent, value);
            }
            remove
            {
                this.RemoveStatusChangeHandler(CancelingEvent, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public event EventHandler<ActivityExecutionStatusChangedEventArgs> Closed
        {
            add
            {
                this.AddStatusChangeHandler(ClosedEvent, value);
            }
            remove
            {
                this.RemoveStatusChangeHandler(ClosedEvent, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public event EventHandler<ActivityExecutionStatusChangedEventArgs> Compensating
        {
            add
            {
                this.AddStatusChangeHandler(CompensatingEvent, value);
            }
            remove
            {
                this.RemoveStatusChangeHandler(CompensatingEvent, value);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler<ActivityExecutionStatusChangedEventArgs> Executing
        {
            add
            {
                this.AddStatusChangeHandler(ExecutingEvent, value);
            }
            remove
            {
                this.RemoveStatusChangeHandler(ExecutingEvent, value);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler<ActivityExecutionStatusChangedEventArgs> Faulting
        {
            add
            {
                this.AddStatusChangeHandler(FaultingEvent, value);
            }
            remove
            {
                this.RemoveStatusChangeHandler(FaultingEvent, value);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler<ActivityExecutionStatusChangedEventArgs> StatusChanged
        {
            add
            {
                this.AddStatusChangeHandler(StatusChangedEvent, value);
            }
            remove
            {
                this.RemoveStatusChangeHandler(StatusChangedEvent, value);
            }
        }

        internal static  event WorkflowChangeActionsResolveEventHandler WorkflowChangeActionsResolve
        {
            add
            {
                lock (staticSyncRoot)
                {
                    workflowChangeActionsResolve = (WorkflowChangeActionsResolveEventHandler) Delegate.Combine(workflowChangeActionsResolve, value);
                }
            }
            remove
            {
                lock (staticSyncRoot)
                {
                    workflowChangeActionsResolve = (WorkflowChangeActionsResolveEventHandler) Delegate.Remove(workflowChangeActionsResolve, value);
                }
            }
        }

        static Activity()
        {
            binaryFormatter = new BinaryFormatter();
            binaryFormatter.SurrogateSelector = ActivitySurrogateSelector.Default;
            DependencyProperty.RegisterAsKnown(ActivityExecutionContextInfoProperty, 1, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(CompletedExecutionContextsProperty, 2, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(ActiveExecutionContextsProperty, 3, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CompletedOrderIdProperty, 4, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(ExecutionStatusProperty, 5, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(ExecutionResultProperty, 6, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(WasExecutingProperty, 7, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(LockCountOnStatusChangeProperty, 8, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(HasPrimaryClosedProperty, 9, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(NestedActivitiesProperty, 10, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(ActivityContextGuidProperty, 11, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(WorkflowXamlMarkupProperty, 12, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(WorkflowRulesMarkupProperty, 13, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(ActivityExecutionContext.CurrentExceptionProperty, 0x17, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(ActivityExecutionContext.GrantedLocksProperty, 0x18, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(ActivityExecutionContext.LockAcquiredCallbackProperty, 0x19, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(ExecutingEvent, 0x1f, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CancelingEvent, 0x20, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(ClosedEvent, 0x21, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CompensatingEvent, 0x22, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(StatusChangedEvent, 0x23, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(StatusChangedLockedEvent, 0x24, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(LockCountOnStatusChangeChangedEvent, 0x25, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(FaultingEvent, 0x26, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(FaultAndCancellationHandlingFilter.FaultProcessedProperty, 0x29, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CompensationHandlingFilter.CompensateProcessedProperty, 0x2b, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CompensationHandlingFilter.LastCompensatedOrderIdProperty, 0x2c, DependencyProperty.PropertyValidity.Uninitialize);
        }

        public Activity()
        {
            base.SetValue(CustomActivityProperty, false);
            base.SetValue(NameProperty, base.GetType().Name);
        }

        public Activity(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            base.SetValue(CustomActivityProperty, false);
            base.SetValue(NameProperty, name);
        }

        internal static string ActivityExecutionResultEnumToString(ActivityExecutionResult activityExecutionResult)
        {
            string str = string.Empty;
            switch (activityExecutionResult)
            {
                case ActivityExecutionResult.None:
                    return "None";

                case ActivityExecutionResult.Succeeded:
                    return "Succeeded";

                case ActivityExecutionResult.Canceled:
                    return "Canceled";

                case ActivityExecutionResult.Compensated:
                    return "Compensated";

                case ActivityExecutionResult.Faulted:
                    return "Faulted";
            }
            return str;
        }

        internal static string ActivityExecutionStatusEnumToString(ActivityExecutionStatus status)
        {
            string str = string.Empty;
            switch (status)
            {
                case ActivityExecutionStatus.Initialized:
                    return "Initialized";

                case ActivityExecutionStatus.Executing:
                    return "Executing";

                case ActivityExecutionStatus.Canceling:
                    return "Canceling";

                case ActivityExecutionStatus.Closed:
                    return "Closed";

                case ActivityExecutionStatus.Compensating:
                    return "Compensating";

                case ActivityExecutionStatus.Faulting:
                    return "Faulting";
            }
            return str;
        }

        private void AddStatusChangeHandler(DependencyProperty dependencyProp, EventHandler<ActivityExecutionStatusChangedEventArgs> delegateValue)
        {
            IList list = null;
            if (base.DependencyPropertyValues.ContainsKey(dependencyProp))
            {
                list = base.DependencyPropertyValues[dependencyProp] as IList;
            }
            else
            {
                list = new ArrayList();
                base.DependencyPropertyValues[dependencyProp] = list;
            }
            list.Add(new ActivityExecutorDelegateInfo<ActivityExecutionStatusChangedEventArgs>(true, delegateValue, this.ContextActivity ?? this.RootActivity));
        }

        protected internal virtual ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            return ActivityExecutionStatus.Closed;
        }

        public Activity Clone()
        {
            if (base.DesignMode)
            {
                throw new InvalidOperationException(SR.GetString("Error_NoRuntimeAvailable"));
            }
            long num = (long) base.GetValue(SerializedStreamLengthProperty);
            if (num == 0L)
            {
                num = 0x2800L;
            }
            MemoryStream stream = new MemoryStream((int) num);
            this.Save(stream);
            stream.Position = 0L;
            base.SetValue(SerializedStreamLengthProperty, (stream.Length > num) ? stream.Length : num);
            return Load(stream, this);
        }

        internal virtual IList<Activity> CollectNestedActivities()
        {
            return null;
        }

        internal void DecrementCompletedOrderId()
        {
            int num = (int) this.RootActivity.GetValue(CompletedOrderIdProperty);
            this.RootActivity.SetValue(CompletedOrderIdProperty, num - 1);
        }

        protected internal virtual ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            return ActivityExecutionStatus.Closed;
        }

        private static void FillContextIdToActivityMap(Activity seedActivity)
        {
            Queue<Activity> queue = new Queue<Activity>();
            queue.Enqueue(seedActivity);
            while (queue.Count > 0)
            {
                Activity activity = queue.Dequeue();
                if (activity.IsContextActivity)
                {
                    ContextIdToActivityMap[activity.ContextId] = activity;
                    IList<Activity> list = (IList<Activity>) activity.GetValue(ActiveExecutionContextsProperty);
                    if (list != null)
                    {
                        foreach (Activity activity2 in list)
                        {
                            queue.Enqueue(activity2);
                        }
                    }
                }
                else
                {
                    ContextIdToActivityMap[0] = activity;
                }
            }
            ActivityRoots = new ArrayList(ContextIdToActivityMap.Values);
        }

        private void FireStatusChangedEvents(DependencyProperty dependencyProperty, bool transacted)
        {
            IList statusChangeHandlers = this.GetStatusChangeHandlers(dependencyProperty);
            if (statusChangeHandlers != null)
            {
                ActivityExecutionStatusChangedEventArgs e = new ActivityExecutionStatusChangedEventArgs(this.ExecutionStatus, this.ExecutionResult, this);
                foreach (ActivityExecutorDelegateInfo<ActivityExecutionStatusChangedEventArgs> info in statusChangeHandlers)
                {
                    info.InvokeDelegate(this.ContextActivity, e, info.ActivityQualifiedName == null, transacted);
                }
            }
        }

        internal override void FixUpMetaProperties(DependencyObject originalObject)
        {
            if (originalObject == null)
            {
                throw new ArgumentNullException();
            }
            base.FixUpMetaProperties(originalObject);
        }

        internal virtual void FixUpParentChildRelationship(Activity definitionActivity, Activity parentActivity, Hashtable deserializedActivities)
        {
            if (parentActivity != null)
            {
                this.SetParent((CompositeActivity) parentActivity);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Activity GetActivityByName(string activityQualifiedName)
        {
            return this.GetActivityByName(activityQualifiedName, false);
        }

        public Activity GetActivityByName(string activityQualifiedName, bool withinThisActivityOnly)
        {
            if (activityQualifiedName == null)
            {
                throw new ArgumentNullException("activityQualifiedName");
            }
            if (this.QualifiedName == activityQualifiedName)
            {
                return this;
            }
            Activity activity = null;
            activity = this.ResolveActivityByName(activityQualifiedName, withinThisActivityOnly);
            if (((activity == null) && (this is CompositeActivity)) && Helpers.IsCustomActivity(this as CompositeActivity))
            {
                activity = this.ResolveActivityByName(this.QualifiedName + "." + activityQualifiedName, withinThisActivityOnly);
            }
            return activity;
        }

        private IList GetStatusChangeHandlers(DependencyProperty dependencyProp)
        {
            IList list = null;
            if (base.DependencyPropertyValues.ContainsKey(dependencyProp))
            {
                list = base.DependencyPropertyValues[dependencyProp] as IList;
            }
            return list;
        }

        protected internal virtual ActivityExecutionStatus HandleFault(ActivityExecutionContext executionContext, Exception exception)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            return ActivityExecutionStatus.Closed;
        }

        internal void HoldLockOnStatusChange(IActivityEventListener<ActivityExecutionStatusChangedEventArgs> eventListener)
        {
            this.RegisterForStatusChange(StatusChangedLockedEvent, eventListener);
            base.SetValue(LockCountOnStatusChangeProperty, this.LockCountOnStatusChange + 1);
        }

        internal int IncrementCompletedOrderId()
        {
            int num = (int) this.RootActivity.GetValue(CompletedOrderIdProperty);
            this.RootActivity.SetValue(CompletedOrderIdProperty, num + 1);
            return (num + 1);
        }

        protected internal virtual void Initialize(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
        }

        protected internal void Invoke<T>(EventHandler<T> handler, T e) where T: EventArgs
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (this.WorkflowCoreRuntime == null)
            {
                throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, "Error_NoRuntimeAvailable"));
            }
            if ((this.ExecutionStatus == ActivityExecutionStatus.Initialized) || (this.ExecutionStatus == ActivityExecutionStatus.Closed))
            {
                throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, "Error_InvalidInvokingState"));
            }
            ActivityExecutorDelegateInfo<T> info = null;
            using (this.WorkflowCoreRuntime.SetCurrentActivity(this))
            {
                info = new ActivityExecutorDelegateInfo<T>(handler, this.ContextActivity);
            }
            info.InvokeDelegate(this.WorkflowCoreRuntime.CurrentActivity.ContextActivity, e, false);
        }

        protected internal void Invoke<T>(IActivityEventListener<T> eventListener, T e) where T: EventArgs
        {
            if (eventListener == null)
            {
                throw new ArgumentNullException("eventListener");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (this.WorkflowCoreRuntime == null)
            {
                throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, "Error_NoRuntimeAvailable"));
            }
            if ((this.ExecutionStatus == ActivityExecutionStatus.Initialized) || (this.ExecutionStatus == ActivityExecutionStatus.Closed))
            {
                throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, "Error_InvalidInvokingState"));
            }
            ActivityExecutorDelegateInfo<T> info = null;
            using (this.WorkflowCoreRuntime.SetCurrentActivity(this))
            {
                info = new ActivityExecutorDelegateInfo<T>(eventListener, this.ContextActivity);
            }
            info.InvokeDelegate(this.WorkflowCoreRuntime.CurrentActivity.ContextActivity, e, false);
        }

        public static Activity Load(Stream stream, Activity outerActivity)
        {
            return Load(stream, outerActivity, binaryFormatter);
        }

        public static Activity Load(Stream stream, Activity outerActivity, IFormatter formatter)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (formatter == null)
            {
                throw new ArgumentNullException("formatter");
            }
            if ((outerActivity != null) && outerActivity.DesignMode)
            {
                throw new InvalidOperationException(SR.GetString("Error_NoRuntimeAvailable"));
            }
            Activity item = null;
            Hashtable contextIdToActivityMap = ContextIdToActivityMap;
            Activity definitionActivity = DefinitionActivity;
            ContextIdToActivityMap = new Hashtable();
            DefinitionActivity = outerActivity;
            try
            {
                if (outerActivity != null)
                {
                    FillContextIdToActivityMap(outerActivity.RootActivity);
                }
                item = (Activity) formatter.Deserialize(stream);
                Queue<Activity> queue = new Queue<Activity>();
                queue.Enqueue(item);
                while (queue.Count > 0)
                {
                    Activity activity3 = queue.Dequeue();
                    Activity activityByName = DefinitionActivity;
                    Activity parentActivity = (outerActivity != null) ? outerActivity.parent : null;
                    if (activity3.IsContextActivity)
                    {
                        ActivityExecutionContextInfo info = (ActivityExecutionContextInfo) activity3.GetValue(ActivityExecutionContextInfoProperty);
                        activityByName = activityByName.GetActivityByName(info.ActivityQualifiedName);
                        Activity activity6 = (Activity) ContextIdToActivityMap[info.ParentContextId];
                        if (activity6 != null)
                        {
                            parentActivity = activity6.GetActivityByName(info.ActivityQualifiedName).parent;
                        }
                        ContextIdToActivityMap[activity3.ContextId] = activity3;
                        IList<Activity> list = (IList<Activity>) activity3.GetValue(ActiveExecutionContextsProperty);
                        if (list != null)
                        {
                            foreach (Activity activity7 in list)
                            {
                                queue.Enqueue(activity7);
                            }
                        }
                    }
                    Hashtable deserializedActivities = new Hashtable();
                    IList<Activity> list2 = (IList<Activity>) activity3.GetValue(NestedActivitiesProperty);
                    if (list2 != null)
                    {
                        foreach (Activity activity8 in list2)
                        {
                            deserializedActivities.Add(activity8.DottedPath, activity8);
                        }
                    }
                    activity3.FixUpParentChildRelationship(activityByName, parentActivity, deserializedActivities);
                    activity3.FixUpMetaProperties(activityByName);
                    activity3.RemoveProperty(NestedActivitiesProperty);
                }
                if (item.Parent == null)
                {
                    item.SetValue(WorkflowDefinitionProperty, DefinitionActivity);
                }
            }
            finally
            {
                ContextIdToActivityMap = contextIdToActivityMap;
                DefinitionActivity = definitionActivity;
                ActivityRoots = null;
            }
            return item;
        }

        internal void MarkCanceled()
        {
            if (this.ExecutionStatus != ActivityExecutionStatus.Closed)
            {
                if (this.ExecutionStatus != ActivityExecutionStatus.Canceling)
                {
                    throw new InvalidOperationException(SR.GetString("Error_InvalidCancelActivityState"));
                }
                base.SetValue(ExecutionResultProperty, ActivityExecutionResult.Canceled);
                this.MarkClosed();
            }
        }

        private void MarkClosed()
        {
            switch (this.ExecutionStatus)
            {
                case ActivityExecutionStatus.Executing:
                case ActivityExecutionStatus.Canceling:
                case ActivityExecutionStatus.Compensating:
                case ActivityExecutionStatus.Faulting:
                {
                    if (this is CompositeActivity)
                    {
                        foreach (Activity activity in ((CompositeActivity) this).Activities)
                        {
                            if ((activity.Enabled && (activity.ExecutionStatus != ActivityExecutionStatus.Initialized)) && (activity.ExecutionStatus != ActivityExecutionStatus.Closed))
                            {
                                throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, "Error_ActiveChildExist"));
                            }
                        }
                        ActivityExecutionContext context = new ActivityExecutionContext(this);
                        foreach (ActivityExecutionContext context2 in context.ExecutionContextManager.ExecutionContexts)
                        {
                            if (this.GetActivityByName(context2.Activity.QualifiedName, true) != null)
                            {
                                throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, "Error_ActiveChildContextExist"));
                            }
                        }
                    }
                    if (this.LockCountOnStatusChange > 0)
                    {
                        base.SetValue(HasPrimaryClosedProperty, true);
                        this.FireStatusChangedEvents(StatusChangedLockedEvent, false);
                        return;
                    }
                    if ((this.parent == null) || ((this.ExecutionResult == ActivityExecutionResult.Succeeded) && ((this is ICompensatableActivity) || this.PersistOnClose)))
                    {
                        ActivityExecutionStatus executionStatus = this.ExecutionStatus;
                        ActivityExecutionResult executionResult = this.ExecutionResult;
                        this.SetStatus(ActivityExecutionStatus.Closed, true);
                        try
                        {
                            this.OnClosed(this.RootActivity.WorkflowCoreRuntime);
                        }
                        catch (Exception exception)
                        {
                            base.SetValue(ExecutionResultProperty, ActivityExecutionResult.Faulted);
                            base.SetValueCommon(ActivityExecutionContext.CurrentExceptionProperty, exception, ActivityExecutionContext.CurrentExceptionProperty.DefaultMetadata, false);
                        }
                        if ((this.parent != null) && (this is ICompensatableActivity))
                        {
                            base.SetValue(CompletedOrderIdProperty, this.IncrementCompletedOrderId());
                        }
                        if (this.CanUninitializeNow)
                        {
                            this.Uninitialize(this.RootActivity.WorkflowCoreRuntime);
                            base.SetValue(ExecutionResultProperty, ActivityExecutionResult.Uninitialized);
                        }
                        else if (this.parent == null)
                        {
                            UninitializeCompletedContext(this, new ActivityExecutionContext(this));
                        }
                        try
                        {
                            Exception exception2 = (Exception) base.GetValue(ActivityExecutionContext.CurrentExceptionProperty);
                            if ((exception2 != null) && (this.parent == null))
                            {
                                this.WorkflowCoreRuntime.ActivityStatusChanged(this, false, true);
                                string message = "Uncaught exception escaped to the root of the workflow.\n" + string.Format(CultureInfo.CurrentCulture, "    In instance {0} in activity {1}\n", new object[] { this.WorkflowInstanceId, string.Empty }) + string.Format(CultureInfo.CurrentCulture, "Inner exception: {0}", new object[] { exception2 });
                                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Critical, 0, message);
                                this.WorkflowCoreRuntime.TerminateInstance(exception2);
                            }
                            else if ((exception2 != null) && (this.parent != null))
                            {
                                this.WorkflowCoreRuntime.RaiseException(exception2, this.Parent, string.Empty);
                                base.RemoveProperty(ActivityExecutionContext.CurrentExceptionProperty);
                            }
                            else if ((this.parent == null) || this.PersistOnClose)
                            {
                                this.WorkflowCoreRuntime.PersistInstanceState(this);
                                this.WorkflowCoreRuntime.ActivityStatusChanged(this, false, true);
                                if (exception2 != null)
                                {
                                    this.WorkflowCoreRuntime.RaiseException(exception2, this.Parent, string.Empty);
                                    base.RemoveProperty(ActivityExecutionContext.CurrentExceptionProperty);
                                }
                            }
                            for (Activity activity2 = this.parent; activity2 != null; activity2 = activity2.parent)
                            {
                                if (activity2.SupportsSynchronization || (activity2.Parent == null))
                                {
                                    activity2.RemoveProperty(ActivityExecutionContext.CachedGrantedLocksProperty);
                                }
                            }
                            return;
                        }
                        catch
                        {
                            if ((this.parent != null) && (this is ICompensatableActivity))
                            {
                                base.RemoveProperty(CompletedOrderIdProperty);
                                this.DecrementCompletedOrderId();
                            }
                            base.SetValue(ExecutionResultProperty, executionResult);
                            this.SetStatus(executionStatus, true);
                            for (Activity activity3 = this.parent; activity3 != null; activity3 = activity3.parent)
                            {
                                if (activity3.SupportsSynchronization || (activity3.Parent == null))
                                {
                                    object obj2 = activity3.GetValue(ActivityExecutionContext.CachedGrantedLocksProperty);
                                    if (obj2 != null)
                                    {
                                        activity3.SetValue(ActivityExecutionContext.GrantedLocksProperty, obj2);
                                    }
                                    activity3.RemoveProperty(ActivityExecutionContext.CachedGrantedLocksProperty);
                                }
                            }
                            throw;
                        }
                    }
                    this.SetStatus(ActivityExecutionStatus.Closed, false);
                    try
                    {
                        this.OnClosed(this.RootActivity.WorkflowCoreRuntime);
                    }
                    catch (Exception exception3)
                    {
                        base.SetValue(ExecutionResultProperty, ActivityExecutionResult.Faulted);
                        base.SetValueCommon(ActivityExecutionContext.CurrentExceptionProperty, exception3, ActivityExecutionContext.CurrentExceptionProperty.DefaultMetadata, false);
                    }
                    if (this.CanUninitializeNow)
                    {
                        this.Uninitialize(this.RootActivity.WorkflowCoreRuntime);
                        base.SetValue(ExecutionResultProperty, ActivityExecutionResult.Uninitialized);
                    }
                    Exception e = (Exception) base.GetValue(ActivityExecutionContext.CurrentExceptionProperty);
                    if (e != null)
                    {
                        this.WorkflowCoreRuntime.RaiseException(e, this.Parent, string.Empty);
                        base.RemoveProperty(ActivityExecutionContext.CurrentExceptionProperty);
                    }
                    return;
                }
            }
            throw new InvalidOperationException(SR.GetString("Error_InvalidCloseActivityState"));
        }

        internal void MarkCompensated()
        {
            if (this.ExecutionStatus != ActivityExecutionStatus.Compensating)
            {
                throw new InvalidOperationException(SR.GetString("Error_InvalidCompensateActivityState"));
            }
            base.SetValue(ExecutionResultProperty, ActivityExecutionResult.Compensated);
            this.MarkClosed();
        }

        internal void MarkCompleted()
        {
            base.SetValue(ExecutionResultProperty, ActivityExecutionResult.Succeeded);
            this.MarkClosed();
        }

        internal void MarkFaulted()
        {
            base.SetValue(ExecutionResultProperty, ActivityExecutionResult.Faulted);
            this.MarkClosed();
        }

        protected internal virtual void OnActivityExecutionContextLoad(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
        }

        protected internal virtual void OnActivityExecutionContextUnload(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
        }

        protected virtual void OnClosed(IServiceProvider provider)
        {
        }

        internal override void OnInitializeActivatingInstanceForRuntime(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            base.OnInitializeActivatingInstanceForRuntime(workflowCoreRuntime);
            this.workflowCoreRuntime = workflowCoreRuntime;
        }

        internal override void OnInitializeDefinitionForRuntime()
        {
            if (base.DesignMode)
            {
                base.OnInitializeDefinitionForRuntime();
                base.UserData[UserDataKeys.CustomActivity] = base.GetValue(CustomActivityProperty);
                ICollection<string> collection = (ICollection<string>) base.GetValue(SynchronizationHandlesProperty);
                if (this.SupportsTransaction)
                {
                    if (collection == null)
                    {
                        collection = new List<string>();
                    }
                    collection.Add(TransactionScopeActivity.TransactionScopeActivityIsolationHandle);
                }
                if (collection != null)
                {
                    base.SetValue(SynchronizationHandlesProperty, new ReadOnlyCollection<string>(new List<string>(collection)));
                }
                if (this.Parent == null)
                {
                    Hashtable hashtable = new Hashtable();
                    base.UserData[UserDataKeys.LookupPaths] = hashtable;
                    hashtable.Add(this.QualifiedName, string.Empty);
                }
                base.SetReadOnlyPropertyValue(QualifiedNameProperty, this.QualifiedName);
                base.SetReadOnlyPropertyValue(DottedPathProperty, this.DottedPath);
                base.UserData[typeof(PersistOnCloseAttribute)] = base.GetType().GetCustomAttributes(typeof(PersistOnCloseAttribute), true).Length > 0;
            }
        }

        internal override void OnInitializeInstanceForRuntime(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            base.OnInitializeInstanceForRuntime(workflowCoreRuntime);
            this.workflowCoreRuntime = workflowCoreRuntime;
        }

        internal static Activity OnResolveActivityDefinition(Type type, string workflowMarkup, string rulesMarkup, bool createNew, bool initForRuntime, IServiceProvider serviceProvider)
        {
            Delegate[] invocationList = null;
            lock (staticSyncRoot)
            {
                if (activityDefinitionResolve != null)
                {
                    invocationList = activityDefinitionResolve.GetInvocationList();
                }
            }
            Activity activity = null;
            if (invocationList != null)
            {
                foreach (ActivityResolveEventHandler handler in invocationList)
                {
                    activity = handler(null, new ActivityResolveEventArgs(type, workflowMarkup, rulesMarkup, createNew, initForRuntime, serviceProvider));
                    if (activity != null)
                    {
                        return activity;
                    }
                }
            }
            return null;
        }

        internal static ArrayList OnResolveWorkflowChangeActions(string workflowChangesMarkup, Activity root)
        {
            Delegate[] invocationList = null;
            lock (staticSyncRoot)
            {
                if (workflowChangeActionsResolve != null)
                {
                    invocationList = workflowChangeActionsResolve.GetInvocationList();
                }
            }
            ArrayList list = null;
            if (invocationList != null)
            {
                foreach (WorkflowChangeActionsResolveEventHandler handler in invocationList)
                {
                    list = handler(root, new WorkflowChangeActionsResolveEventArgs(workflowChangesMarkup));
                    if (list != null)
                    {
                        return list;
                    }
                }
            }
            return null;
        }

        protected internal void RaiseEvent(DependencyProperty dependencyEvent, object sender, EventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (dependencyEvent == null)
            {
                throw new ArgumentNullException("dependencyEvent");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (this.WorkflowCoreRuntime == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_NoRuntimeAvailable"));
            }
            EventHandler[] invocationList = this.GetInvocationList<EventHandler>(dependencyEvent);
            if (invocationList != null)
            {
                foreach (EventHandler handler in invocationList)
                {
                    this.WorkflowCoreRuntime.RaiseHandlerInvoking(handler);
                    try
                    {
                        handler(sender, e);
                    }
                    finally
                    {
                        this.WorkflowCoreRuntime.RaiseHandlerInvoked();
                    }
                }
            }
        }

        protected internal void RaiseGenericEvent<T>(DependencyProperty dependencyEvent, object sender, T e) where T: EventArgs
        {
            if (dependencyEvent == null)
            {
                throw new ArgumentNullException("dependencyEvent");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (this.WorkflowCoreRuntime == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_NoRuntimeAvailable"));
            }
            EventHandler<T>[] invocationList = this.GetInvocationList<EventHandler<T>>(dependencyEvent);
            if (invocationList != null)
            {
                foreach (EventHandler<T> handler in invocationList)
                {
                    this.WorkflowCoreRuntime.RaiseHandlerInvoking(handler);
                    try
                    {
                        handler(sender, e);
                    }
                    finally
                    {
                        this.WorkflowCoreRuntime.RaiseHandlerInvoked();
                    }
                }
            }
        }

        public void RegisterForStatusChange(DependencyProperty dependencyProp, IActivityEventListener<ActivityExecutionStatusChangedEventArgs> activityStatusChangeListener)
        {
            if (dependencyProp == null)
            {
                throw new ArgumentNullException("dependencyProp");
            }
            if (activityStatusChangeListener == null)
            {
                throw new ArgumentNullException("activityStatusChangeListener");
            }
            if ((((dependencyProp != ExecutingEvent) && (dependencyProp != CancelingEvent)) && ((dependencyProp != ClosedEvent) && (dependencyProp != CompensatingEvent))) && (((dependencyProp != FaultingEvent) && (dependencyProp != StatusChangedEvent)) && ((dependencyProp != StatusChangedLockedEvent) && (dependencyProp != LockCountOnStatusChangeChangedEvent))))
            {
                throw new ArgumentException();
            }
            IList list = null;
            if (base.DependencyPropertyValues.ContainsKey(dependencyProp))
            {
                list = base.DependencyPropertyValues[dependencyProp] as IList;
            }
            else
            {
                list = new ArrayList();
                base.DependencyPropertyValues[dependencyProp] = list;
            }
            list.Add(new ActivityExecutorDelegateInfo<ActivityExecutionStatusChangedEventArgs>(true, activityStatusChangeListener, this.ContextActivity));
        }

        internal void ReleaseLockOnStatusChange(IActivityEventListener<ActivityExecutionStatusChangedEventArgs> eventListener)
        {
            this.UnregisterForStatusChange(StatusChangedLockedEvent, eventListener);
            int lockCountOnStatusChange = this.LockCountOnStatusChange;
            base.SetValue(LockCountOnStatusChangeProperty, --lockCountOnStatusChange);
            if (lockCountOnStatusChange == 0)
            {
                if (!this.HasPrimaryClosed)
                {
                    base.SetValue(ExecutionResultProperty, ActivityExecutionResult.Canceled);
                }
                try
                {
                    this.MarkClosed();
                    return;
                }
                catch
                {
                    base.SetValue(LockCountOnStatusChangeProperty, ++lockCountOnStatusChange);
                    this.RegisterForStatusChange(StatusChangedLockedEvent, eventListener);
                    throw;
                }
            }
            this.FireStatusChangedEvents(LockCountOnStatusChangeChangedEvent, false);
        }

        private void RemoveStatusChangeHandler(DependencyProperty dependencyProp, EventHandler<ActivityExecutionStatusChangedEventArgs> delegateValue)
        {
            if (base.DependencyPropertyValues.ContainsKey(dependencyProp))
            {
                IList list = base.DependencyPropertyValues[dependencyProp] as IList;
                if (list != null)
                {
                    list.Remove(new ActivityExecutorDelegateInfo<ActivityExecutionStatusChangedEventArgs>(true, delegateValue, this.ContextActivity));
                    if (list.Count == 0)
                    {
                        base.DependencyPropertyValues.Remove(dependencyProp);
                    }
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal void ResetAllKnownDependencyProperties()
        {
            this.ResetKnownDependencyProperties(true);
        }

        private void ResetKnownDependencyProperties(bool forReexecute)
        {
            DependencyProperty[] array = new DependencyProperty[base.DependencyPropertyValues.Keys.Count];
            base.DependencyPropertyValues.Keys.CopyTo(array, 0);
            foreach (DependencyProperty property in array)
            {
                if (property.IsKnown && ((property.Validity == DependencyProperty.PropertyValidity.Uninitialize) || (forReexecute && (property.Validity == DependencyProperty.PropertyValidity.Reexecute))))
                {
                    base.RemoveProperty(property);
                }
            }
        }

        private Activity ResolveActivityByName(string activityQualifiedName, bool withinThisActivityOnly)
        {
            Activity activity = null;
            if (!base.DesignMode && !this.DynamicUpdateMode)
            {
                Activity rootActivity = this.RootActivity;
                Hashtable hashtable = (Hashtable) rootActivity.UserData[UserDataKeys.LookupPaths];
                if (hashtable != null)
                {
                    string dottedPath = (string) hashtable[activityQualifiedName];
                    if (dottedPath == null)
                    {
                        return activity;
                    }
                    if (dottedPath.Length != 0)
                    {
                        string str2 = (string) hashtable[this.QualifiedName];
                        if (dottedPath.StartsWith(str2, StringComparison.Ordinal))
                        {
                            if (dottedPath.Length == str2.Length)
                            {
                                return this;
                            }
                            if ((str2.Length == 0) || (dottedPath[str2.Length] == '.'))
                            {
                                activity = this.TraverseDottedPath(dottedPath.Substring((str2.Length > 0) ? (str2.Length + 1) : 0));
                            }
                            return activity;
                        }
                        if (!withinThisActivityOnly)
                        {
                            activity = rootActivity.TraverseDottedPath(dottedPath);
                        }
                        return activity;
                    }
                    if (!withinThisActivityOnly)
                    {
                        activity = rootActivity;
                    }
                }
                return activity;
            }
            if (!base.DesignMode)
            {
                CompositeActivity compositeActivity = (withinThisActivityOnly ? ((CompositeActivity) this) : ((CompositeActivity) this.RootActivity)) as CompositeActivity;
                if (compositeActivity != null)
                {
                    foreach (Activity activity4 in Helpers.GetNestedActivities(compositeActivity))
                    {
                        if (activity4.QualifiedName == activityQualifiedName)
                        {
                            return activity4;
                        }
                    }
                }
                return activity;
            }
            activity = Helpers.ParseActivity(this, activityQualifiedName);
            if ((activity == null) && !withinThisActivityOnly)
            {
                activity = Helpers.ParseActivity(this.RootActivity, activityQualifiedName);
            }
            return activity;
        }

        public void Save(Stream stream)
        {
            this.Save(stream, binaryFormatter);
        }

        public void Save(Stream stream, IFormatter formatter)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (formatter == null)
            {
                throw new ArgumentNullException("formatter");
            }
            if (base.DesignMode)
            {
                throw new InvalidOperationException(SR.GetString("Error_NoRuntimeAvailable"));
            }
            Hashtable contextIdToActivityMap = ContextIdToActivityMap;
            ContextIdToActivityMap = new Hashtable();
            try
            {
                FillContextIdToActivityMap(this);
                foreach (Activity activity in ContextIdToActivityMap.Values)
                {
                    IList<Activity> list = activity.CollectNestedActivities();
                    if ((list != null) && (list.Count > 0))
                    {
                        activity.SetValue(NestedActivitiesProperty, list);
                    }
                }
                formatter.Serialize(stream, this);
            }
            finally
            {
                foreach (Activity activity2 in ContextIdToActivityMap.Values)
                {
                    activity2.RemoveProperty(NestedActivitiesProperty);
                }
                ContextIdToActivityMap = contextIdToActivityMap;
                ActivityRoots = null;
            }
        }

        internal void SetParent(CompositeActivity compositeActivity)
        {
            this.parent = compositeActivity;
        }

        internal void SetStatus(ActivityExecutionStatus newStatus, bool transacted)
        {
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Activity Status Change - Activity: {0} Old:{1}; New:{2}", new object[] { this.QualifiedName, ActivityExecutionStatusEnumToString(this.ExecutionStatus), ActivityExecutionStatusEnumToString(newStatus) });
            if ((newStatus == ActivityExecutionStatus.Faulting) && (this.ExecutionStatus == ActivityExecutionStatus.Executing))
            {
                base.SetValue(WasExecutingProperty, true);
            }
            base.SetValue(ExecutionStatusProperty, newStatus);
            this.FireStatusChangedEvents(StatusChangedEvent, transacted);
            switch (newStatus)
            {
                case ActivityExecutionStatus.Executing:
                    this.FireStatusChangedEvents(ExecutingEvent, transacted);
                    break;

                case ActivityExecutionStatus.Canceling:
                    this.FireStatusChangedEvents(CancelingEvent, transacted);
                    break;

                case ActivityExecutionStatus.Closed:
                    this.FireStatusChangedEvents(ClosedEvent, transacted);
                    break;

                case ActivityExecutionStatus.Compensating:
                    this.FireStatusChangedEvents(CompensatingEvent, transacted);
                    break;

                case ActivityExecutionStatus.Faulting:
                    this.FireStatusChangedEvents(FaultingEvent, transacted);
                    break;

                default:
                    return;
            }
            this.WorkflowCoreRuntime.ActivityStatusChanged(this, transacted, false);
            if (newStatus == ActivityExecutionStatus.Closed)
            {
                base.RemoveProperty(LockCountOnStatusChangeProperty);
                base.RemoveProperty(HasPrimaryClosedProperty);
                base.RemoveProperty(WasExecutingProperty);
            }
        }

        public override string ToString()
        {
            return (this.QualifiedName + " [" + base.GetType().FullName + "]");
        }

        protected void TrackData(object userData)
        {
            if (userData == null)
            {
                throw new ArgumentNullException("userData");
            }
            if (this.WorkflowCoreRuntime == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_NoRuntimeAvailable"));
            }
            this.WorkflowCoreRuntime.Track(null, userData);
        }

        protected void TrackData(string userDataKey, object userData)
        {
            if (userData == null)
            {
                throw new ArgumentNullException("userData");
            }
            if (this.WorkflowCoreRuntime == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_NoRuntimeAvailable"));
            }
            this.WorkflowCoreRuntime.Track(userDataKey, userData);
        }

        internal virtual Activity TraverseDottedPath(string dottedPath)
        {
            return null;
        }

        internal Activity TraverseDottedPathFromRoot(string dottedPathFromRoot)
        {
            string dottedPath = this.DottedPath;
            if (dottedPathFromRoot == dottedPath)
            {
                return this;
            }
            if (!dottedPathFromRoot.StartsWith(dottedPath, StringComparison.Ordinal))
            {
                return null;
            }
            string str2 = dottedPathFromRoot;
            if (dottedPath.Length > 0)
            {
                str2 = dottedPathFromRoot.Substring(dottedPath.Length + 1);
            }
            return this.TraverseDottedPath(str2);
        }

        protected internal virtual void Uninitialize(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            this.ResetKnownDependencyProperties(false);
        }

        private static void UninitializeCompletedContext(Activity activity, ActivityExecutionContext executionContext)
        {
            IList<ActivityExecutionContextInfo> collection = activity.GetValue(CompletedExecutionContextsProperty) as IList<ActivityExecutionContextInfo>;
            if ((collection != null) && (collection.Count > 0))
            {
                IList<ActivityExecutionContextInfo> list2 = new List<ActivityExecutionContextInfo>(collection);
                foreach (ActivityExecutionContextInfo info in list2)
                {
                    if ((((byte) (info.Flags & PersistFlags.NeedsCompensation)) != 0) && (activity.GetActivityByName(info.ActivityQualifiedName, true) != null))
                    {
                        ActivityExecutionContext context = executionContext.ExecutionContextManager.DiscardPersistedExecutionContext(info);
                        UninitializeCompletedContext(context.Activity, context);
                        executionContext.ExecutionContextManager.CompleteExecutionContext(context);
                    }
                }
            }
            CompositeActivity compositeActivity = activity as CompositeActivity;
            if (compositeActivity != null)
            {
                Activity[] compensatableChildren = CompensationUtils.GetCompensatableChildren(compositeActivity);
                for (int i = compensatableChildren.Length - 1; i >= 0; i--)
                {
                    Activity activity3 = (Activity) compensatableChildren.GetValue(i);
                    activity3.Uninitialize(activity.RootActivity.WorkflowCoreRuntime);
                    activity3.SetValue(ExecutionResultProperty, ActivityExecutionResult.Uninitialized);
                }
            }
            activity.Uninitialize(activity.RootActivity.WorkflowCoreRuntime);
            activity.SetValue(ExecutionResultProperty, ActivityExecutionResult.Uninitialized);
        }

        public void UnregisterForStatusChange(DependencyProperty dependencyProp, IActivityEventListener<ActivityExecutionStatusChangedEventArgs> activityStatusChangeListener)
        {
            if (dependencyProp == null)
            {
                throw new ArgumentNullException("dependencyProp");
            }
            if (activityStatusChangeListener == null)
            {
                throw new ArgumentNullException("activityStatusChangeListener");
            }
            if ((((dependencyProp != ExecutingEvent) && (dependencyProp != CancelingEvent)) && ((dependencyProp != ClosedEvent) && (dependencyProp != CompensatingEvent))) && (((dependencyProp != FaultingEvent) && (dependencyProp != StatusChangedEvent)) && ((dependencyProp != StatusChangedLockedEvent) && (dependencyProp != LockCountOnStatusChangeChangedEvent))))
            {
                throw new ArgumentException();
            }
            if (base.DependencyPropertyValues.ContainsKey(dependencyProp))
            {
                IList list = base.DependencyPropertyValues[dependencyProp] as IList;
                if (list != null)
                {
                    list.Remove(new ActivityExecutorDelegateInfo<ActivityExecutionStatusChangedEventArgs>(true, activityStatusChangeListener, this.ContextActivity));
                    if (list.Count == 0)
                    {
                        base.DependencyPropertyValues.Remove(dependencyProp);
                    }
                }
            }
        }

        internal string CachedDottedPath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.cachedDottedPath;
            }
        }

        internal bool CanUninitializeNow
        {
            get
            {
                if (this.NeedsCompensation)
                {
                    return false;
                }
                Activity contextActivity = this.ContextActivity;
                if (contextActivity != this)
                {
                    IList<ActivityExecutionContextInfo> list = contextActivity.GetValue(CompletedExecutionContextsProperty) as IList<ActivityExecutionContextInfo>;
                    if ((list != null) && (list.Count > 0))
                    {
                        foreach (ActivityExecutionContextInfo info in list)
                        {
                            if ((((byte) (info.Flags & PersistFlags.NeedsCompensation)) != 0) && (this.GetActivityByName(info.ActivityQualifiedName, true) != null))
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
        }

        internal Activity ContextActivity
        {
            get
            {
                Activity parent = this;
                while ((parent != null) && (parent.GetValue(ActivityExecutionContextInfoProperty) == null))
                {
                    parent = parent.parent;
                }
                return parent;
            }
        }

        internal Guid ContextGuid
        {
            get
            {
                return ((ActivityExecutionContextInfo) this.ContextActivity.GetValue(ActivityExecutionContextInfoProperty)).ContextGuid;
            }
        }

        internal int ContextId
        {
            get
            {
                return ((ActivityExecutionContextInfo) this.ContextActivity.GetValue(ActivityExecutionContextInfoProperty)).ContextId;
            }
        }

        [SRDescription("DescriptionDescr"), DefaultValue(""), SRCategory("Activity"), Browsable(true), Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string Description
        {
            get
            {
                return (string) base.GetValue(DescriptionProperty);
            }
            set
            {
                base.SetValue(DescriptionProperty, value);
            }
        }

        internal string DottedPath
        {
            get
            {
                if (!base.DesignMode && !this.DynamicUpdateMode)
                {
                    string str = (string) base.GetValue(DottedPathProperty);
                    if (str != null)
                    {
                        return str;
                    }
                }
                StringBuilder builder = new StringBuilder();
                for (Activity activity = this; activity.parent != null; activity = activity.parent)
                {
                    builder.Insert(0, activity.parent.Activities.IndexOf(activity).ToString(CultureInfo.InvariantCulture));
                    builder.Insert(0, '.');
                }
                if (builder.Length > 0)
                {
                    builder.Remove(0, 1);
                }
                return builder.ToString();
            }
        }

        internal bool DynamicUpdateMode
        {
            get
            {
                return (this.cachedDottedPath != null);
            }
            set
            {
                if (value)
                {
                    this.cachedDottedPath = this.DottedPath;
                }
                else
                {
                    this.cachedDottedPath = null;
                }
            }
        }

        [DefaultValue(true), SRCategory("Activity"), SRDescription("EnabledDescr"), Browsable(true)]
        public bool Enabled
        {
            get
            {
                return (bool) base.GetValue(EnabledProperty);
            }
            set
            {
                base.SetValue(EnabledProperty, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public ActivityExecutionResult ExecutionResult
        {
            get
            {
                return (ActivityExecutionResult) base.GetValue(ExecutionResultProperty);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public ActivityExecutionStatus ExecutionStatus
        {
            get
            {
                return (ActivityExecutionStatus) base.GetValue(ExecutionStatusProperty);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal bool HasPrimaryClosed
        {
            get
            {
                return (bool) base.GetValue(HasPrimaryClosedProperty);
            }
        }

        internal bool IsContextActivity
        {
            get
            {
                return (base.GetValue(ActivityExecutionContextInfoProperty) != null);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool IsDynamicActivity
        {
            get
            {
                if (base.DesignMode)
                {
                    return false;
                }
                return (this.ContextActivity != this.RootActivity);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        internal int LockCountOnStatusChange
        {
            get
            {
                return (int) base.GetValue(LockCountOnStatusChangeProperty);
            }
        }

        [DefaultValue(""), ParenthesizePropertyName(true), SRDescription("NameDescr"), MergableProperty(false), Browsable(true), SRCategory("Activity")]
        public string Name
        {
            get
            {
                return (string) base.GetValue(NameProperty);
            }
            set
            {
                base.SetValue(NameProperty, value);
            }
        }

        internal bool NeedsCompensation
        {
            get
            {
                IList<ActivityExecutionContextInfo> list = base.GetValue(CompletedExecutionContextsProperty) as IList<ActivityExecutionContextInfo>;
                if ((list != null) && (list.Count > 0))
                {
                    foreach (ActivityExecutionContextInfo info in list)
                    {
                        if ((((byte) (info.Flags & PersistFlags.NeedsCompensation)) != 0) && (this.GetActivityByName(info.ActivityQualifiedName, true) != null))
                        {
                            return true;
                        }
                    }
                }
                Queue<Activity> queue = new Queue<Activity>();
                queue.Enqueue(this);
                while (queue.Count > 0)
                {
                    Activity activity = queue.Dequeue();
                    if (((activity is ICompensatableActivity) && (activity.ExecutionStatus == ActivityExecutionStatus.Closed)) && (activity.ExecutionResult == ActivityExecutionResult.Succeeded))
                    {
                        return true;
                    }
                    if (activity is CompositeActivity)
                    {
                        foreach (Activity activity2 in ((CompositeActivity) activity).Activities)
                        {
                            if (activity2.Enabled)
                            {
                                queue.Enqueue(activity2);
                            }
                        }
                    }
                }
                return false;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public CompositeActivity Parent
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parent;
            }
        }

        internal Activity ParentContextActivity
        {
            get
            {
                ActivityExecutionContextInfo info = (ActivityExecutionContextInfo) this.ContextActivity.GetValue(ActivityExecutionContextInfoProperty);
                if (info.ParentContextId == -1)
                {
                    return null;
                }
                return this.WorkflowCoreRuntime.GetContextActivityForId(info.ParentContextId);
            }
        }

        internal bool PersistOnClose
        {
            get
            {
                if (base.UserData.Contains(typeof(PersistOnCloseAttribute)))
                {
                    return (bool) base.UserData[typeof(PersistOnCloseAttribute)];
                }
                object[] customAttributes = base.GetType().GetCustomAttributes(typeof(PersistOnCloseAttribute), true);
                return ((customAttributes != null) && (customAttributes.Length > 0));
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string QualifiedName
        {
            get
            {
                if (!base.DesignMode && !this.DynamicUpdateMode)
                {
                    string str = (string) base.GetValue(QualifiedNameProperty);
                    if (str != null)
                    {
                        return str;
                    }
                }
                if (Helpers.IsActivityLocked(this))
                {
                    return InternalHelpers.GenerateQualifiedNameForLockedActivity(this, null);
                }
                return (string) base.GetValue(NameProperty);
            }
        }

        internal Activity RootActivity
        {
            get
            {
                Activity parent = this;
                while (parent.parent != null)
                {
                    parent = parent.parent;
                }
                return parent;
            }
        }

        internal Activity RootContextActivity
        {
            get
            {
                return this.WorkflowCoreRuntime.RootActivity;
            }
        }

        internal bool SupportsSynchronization
        {
            get
            {
                return (this is SynchronizationScopeActivity);
            }
        }

        internal bool SupportsTransaction
        {
            get
            {
                return ((this is CompensatableTransactionScopeActivity) || (this is TransactionScopeActivity));
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        internal bool WasExecuting
        {
            get
            {
                return (bool) base.GetValue(WasExecutingProperty);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        internal IWorkflowCoreRuntime WorkflowCoreRuntime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.workflowCoreRuntime;
            }
        }

        protected Guid WorkflowInstanceId
        {
            get
            {
                if (this.WorkflowCoreRuntime == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_NoRuntimeAvailable"));
                }
                return this.WorkflowCoreRuntime.InstanceID;
            }
        }
    }
}

