namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;

    [ActivityValidator(typeof(CompositeActivityValidator)), ActivityExecutor(typeof(CompositeActivityExecutor<CompositeActivity>)), TypeDescriptionProvider(typeof(CompositeActivityTypeDescriptorProvider)), DesignerSerializer(typeof(CompositeActivityMarkupSerializer), typeof(WorkflowMarkupSerializer)), ActivityCodeGenerator(typeof(CompositeActivityCodeGenerator)), ContentProperty("Activities")]
    public class CompositeActivity : Activity, ISupportAlternateFlow
    {
        [NonSerialized]
        private ActivityCollection activities;
        private static DependencyProperty CanModifyActivitiesProperty = DependencyProperty.Register("CanModifyActivities", typeof(bool), typeof(CompositeActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));

        public CompositeActivity()
        {
            this.activities = new ActivityCollection(this);
            this.activities.ListChanging += new EventHandler<ActivityCollectionChangeEventArgs>(this.OnListChangingEventHandler);
            this.activities.ListChanged += new EventHandler<ActivityCollectionChangeEventArgs>(this.OnListChangedEventHandler);
            base.SetValue(CanModifyActivitiesProperty, false);
        }

        public CompositeActivity(string name) : base(name)
        {
            this.activities = new ActivityCollection(this);
            this.activities.ListChanging += new EventHandler<ActivityCollectionChangeEventArgs>(this.OnListChangingEventHandler);
            this.activities.ListChanged += new EventHandler<ActivityCollectionChangeEventArgs>(this.OnListChangedEventHandler);
            base.SetValue(CanModifyActivitiesProperty, false);
        }

        public CompositeActivity(IEnumerable<Activity> children) : this()
        {
            if (children == null)
            {
                throw new ArgumentNullException("children");
            }
            foreach (Activity activity in children)
            {
                this.activities.Add(activity);
            }
        }

        protected void ApplyWorkflowChanges(WorkflowChanges workflowChanges)
        {
            if (workflowChanges == null)
            {
                throw new ArgumentNullException("workflowChanges");
            }
            if (base.Parent != null)
            {
                throw new InvalidOperationException(SR.GetString("Error_InvalidActivityForWorkflowChanges"));
            }
            if (base.RootActivity == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_MissingRootActivity"));
            }
            if (base.WorkflowCoreRuntime == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_NoRuntimeAvailable"));
            }
            workflowChanges.ApplyTo(this);
        }

        private static bool CannotModifyChildren(CompositeActivity compositeActivity, bool parent)
        {
            if (compositeActivity == null)
            {
                throw new ArgumentNullException("compositeActivity");
            }
            if (parent && (compositeActivity.Parent == null))
            {
                return false;
            }
            return (((bool) compositeActivity.GetValue(Activity.CustomActivityProperty)) || ((compositeActivity.Parent != null) && CannotModifyChildren(compositeActivity.Parent, parent)));
        }

        internal override IList<Activity> CollectNestedActivities()
        {
            List<Activity> list = new List<Activity>();
            Queue<Activity> queue = new Queue<Activity>(this.activities);
            while (queue.Count > 0)
            {
                Activity item = queue.Dequeue();
                list.Add(item);
                if (item is CompositeActivity)
                {
                    foreach (Activity activity2 in ((CompositeActivity) item).activities)
                    {
                        queue.Enqueue(activity2);
                    }
                }
            }
            return list;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Activity activity in this.Activities)
                {
                    activity.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        internal override void FixUpMetaProperties(DependencyObject originalObject)
        {
            if (originalObject == null)
            {
                throw new ArgumentNullException();
            }
            if (!(originalObject is CompositeActivity))
            {
                throw new ArgumentException();
            }
            base.FixUpMetaProperties(originalObject);
            if (this.activities != null)
            {
                CompositeActivity activity = originalObject as CompositeActivity;
                if (activity != null)
                {
                    int num = 0;
                    foreach (Activity activity2 in this.activities)
                    {
                        activity2.FixUpMetaProperties(activity.activities[num++]);
                    }
                }
            }
        }

        internal override void FixUpParentChildRelationship(Activity definitionActivity, Activity parentActivity, Hashtable deserializedActivities)
        {
            CompositeActivity activity = definitionActivity as CompositeActivity;
            if (activity == null)
            {
                throw new ArgumentException("definitionActivity");
            }
            base.FixUpParentChildRelationship(definitionActivity, parentActivity, deserializedActivities);
            this.activities = new ActivityCollection(this);
            this.activities.ListChanging += new EventHandler<ActivityCollectionChangeEventArgs>(this.OnListChangingEventHandler);
            this.activities.ListChanged += new EventHandler<ActivityCollectionChangeEventArgs>(this.OnListChangedEventHandler);
            string dottedPath = base.DottedPath;
            int num = 0;
            foreach (Activity activity2 in activity.activities)
            {
                Activity activity3 = (Activity) deserializedActivities[(dottedPath.Length == 0) ? num.ToString(CultureInfo.InvariantCulture) : (dottedPath + "." + num.ToString(CultureInfo.InvariantCulture))];
                this.activities.InnerAdd(activity3);
                activity3.FixUpParentChildRelationship(activity2, this, deserializedActivities);
                num++;
            }
        }

        protected Activity[] GetDynamicActivities(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (activity.parent != this)
            {
                throw new ArgumentException(SR.GetString("GetDynamicActivities_InvalidActivity"), "activity");
            }
            Activity contextActivity = base.ContextActivity;
            List<Activity> list = new List<Activity>();
            if (contextActivity != null)
            {
                IList<Activity> list2 = (IList<Activity>) contextActivity.GetValue(Activity.ActiveExecutionContextsProperty);
                if (list2 != null)
                {
                    foreach (Activity activity3 in list2)
                    {
                        if (activity3.MetaEquals(activity))
                        {
                            list.Add(activity3);
                        }
                    }
                }
            }
            return list.ToArray();
        }

        protected internal override ActivityExecutionStatus HandleFault(ActivityExecutionContext executionContext, Exception exception)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            ActivityExecutionStatus status = this.Cancel(executionContext);
            if (status == ActivityExecutionStatus.Canceling)
            {
                return ActivityExecutionStatus.Faulting;
            }
            return status;
        }

        protected internal override void Initialize(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (!(provider is ActivityExecutionContext))
            {
                throw new ArgumentException(SR.GetString("Error_InvalidServiceProvider", new object[] { "provider" }));
            }
            foreach (Activity activity in Helpers.GetAllEnabledActivities(this))
            {
                (provider as ActivityExecutionContext).InitializeActivity(activity);
            }
        }

        private static bool IsDynamicMode(CompositeActivity compositeActivity)
        {
            if (compositeActivity == null)
            {
                throw new ArgumentNullException("compositeActivity");
            }
            while (compositeActivity.Parent != null)
            {
                if (compositeActivity.DynamicUpdateMode)
                {
                    return true;
                }
                compositeActivity = compositeActivity.Parent;
            }
            return compositeActivity.DynamicUpdateMode;
        }

        protected internal virtual void OnActivityChangeAdd(ActivityExecutionContext executionContext, Activity addedActivity)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (addedActivity == null)
            {
                throw new ArgumentNullException("addedActivity");
            }
        }

        protected internal virtual void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {
        }

        protected internal override void OnActivityExecutionContextLoad(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            base.OnActivityExecutionContextLoad(provider);
            foreach (Activity activity in Helpers.GetAllEnabledActivities(this))
            {
                activity.OnActivityExecutionContextLoad(provider);
            }
        }

        protected internal override void OnActivityExecutionContextUnload(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            base.OnActivityExecutionContextUnload(provider);
            foreach (Activity activity in Helpers.GetAllEnabledActivities(this))
            {
                activity.OnActivityExecutionContextUnload(provider);
            }
        }

        internal override void OnInitializeActivatingInstanceForRuntime(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            base.OnInitializeActivatingInstanceForRuntime(workflowCoreRuntime);
            foreach (Activity activity in this.activities)
            {
                if (activity.Enabled)
                {
                    ((IDependencyObjectAccessor) activity).InitializeActivatingInstanceForRuntime(null, workflowCoreRuntime);
                }
                else
                {
                    base.Readonly = true;
                }
            }
        }

        internal override void OnInitializeDefinitionForRuntime()
        {
            if (base.DesignMode)
            {
                base.OnInitializeDefinitionForRuntime();
                Hashtable hashtable = (Hashtable) base.RootActivity.UserData[UserDataKeys.LookupPaths];
                string str = (string) hashtable[base.QualifiedName];
                foreach (Activity activity2 in (IEnumerable) this.activities)
                {
                    if (activity2.Enabled)
                    {
                        string str2 = str;
                        if (!string.IsNullOrEmpty(str))
                        {
                            str2 = str2 + ".";
                        }
                        str2 = str2 + this.activities.IndexOf(activity2).ToString(CultureInfo.InvariantCulture);
                        hashtable.Add(activity2.QualifiedName, str2);
                        ((IDependencyObjectAccessor) activity2).InitializeDefinitionForRuntime(null);
                    }
                    else
                    {
                        activity2.OnInitializeDefinitionForRuntime();
                        activity2.Readonly = true;
                    }
                }
            }
        }

        internal override void OnInitializeInstanceForRuntime(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            base.OnInitializeInstanceForRuntime(workflowCoreRuntime);
            foreach (Activity activity in this.activities)
            {
                if (activity.Enabled)
                {
                    ((IDependencyObjectAccessor) activity).InitializeInstanceForRuntime(workflowCoreRuntime);
                }
            }
        }

        protected virtual void OnListChanged(ActivityCollectionChangeEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (((e.Action == ActivityCollectionChangeAction.Replace) || (e.Action == ActivityCollectionChangeAction.Remove)) && (e.RemovedItems != null))
            {
                foreach (Activity activity in e.RemovedItems)
                {
                    activity.SetParent(null);
                }
            }
            if (((e.Action == ActivityCollectionChangeAction.Replace) || (e.Action == ActivityCollectionChangeAction.Add)) && (e.AddedItems != null))
            {
                foreach (Activity activity2 in e.AddedItems)
                {
                    activity2.SetParent(this);
                }
                Queue<Activity> queue = new Queue<Activity>(e.AddedItems);
                while (queue.Count > 0)
                {
                    Activity activity3 = queue.Dequeue();
                    if ((activity3 != null) && (((activity3.Name == null) || (activity3.Name.Length == 0)) || (activity3.Name == activity3.GetType().Name)))
                    {
                        Activity rootActivity = Helpers.GetRootActivity(activity3);
                        string str = rootActivity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
                        if ((rootActivity.Parent == null) || !string.IsNullOrEmpty(str))
                        {
                            ArrayList list = new ArrayList();
                            list.AddRange(Helpers.GetIdentifiersInCompositeActivity(rootActivity as CompositeActivity));
                            activity3.Name = DesignerHelpers.GenerateUniqueIdentifier(this.Site, Helpers.GetBaseIdentifier(activity3), (string[]) list.ToArray(typeof(string)));
                        }
                    }
                    if (activity3 is CompositeActivity)
                    {
                        foreach (Activity activity5 in ((CompositeActivity) activity3).Activities)
                        {
                            queue.Enqueue(activity5);
                        }
                    }
                }
            }
            if (this.Site != null)
            {
                IComponentChangeService service = this.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (service != null)
                {
                    service.OnComponentChanged(this, null, e, null);
                }
            }
        }

        private void OnListChangedEventHandler(object sender, ActivityCollectionChangeEventArgs e)
        {
            this.OnListChanged(e);
        }

        protected virtual void OnListChanging(ActivityCollectionChangeEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if ((e.Action == ActivityCollectionChangeAction.Add) && (e.AddedItems != null))
            {
                foreach (Activity activity in e.AddedItems)
                {
                    if (activity.Parent != null)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_ActivityHasParent", new object[] { activity.QualifiedName, activity.Parent.QualifiedName }));
                    }
                    if (activity == this)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_Recursion", new object[] { activity.QualifiedName }));
                    }
                }
            }
            if (this.Site != null)
            {
                IComponentChangeService service = this.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (service != null)
                {
                    service.OnComponentChanging(this, null);
                }
            }
        }

        private void OnListChangingEventHandler(object sender, ActivityCollectionChangeEventArgs e)
        {
            if (!base.DesignMode && !base.DynamicUpdateMode)
            {
                throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
            }
            if (!this.CanModifyActivities)
            {
                if ((base.DesignMode && (Activity.ActivityType != null)) && (base.GetType() == Activity.ActivityType))
                {
                    throw new InvalidOperationException(SR.GetString("Error_Missing_CanModifyProperties_True", new object[] { base.GetType().FullName }));
                }
                if (!IsDynamicMode(this) && CannotModifyChildren(this, false))
                {
                    throw new InvalidOperationException(SR.GetString("Error_CannotAddRemoveChildActivities"));
                }
                if (IsDynamicMode(this) && CannotModifyChildren(this, true))
                {
                    throw new InvalidOperationException(SR.GetString("Error_CannotAddRemoveChildActivities"));
                }
            }
            if ((e.Action == ActivityCollectionChangeAction.Add) && (e.AddedItems != null))
            {
                for (Activity activity = this; activity != null; activity = activity.Parent)
                {
                    if (e.AddedItems.Contains(activity))
                    {
                        throw new InvalidOperationException(SR.GetString("Error_ActivityCircularReference"));
                    }
                }
            }
            this.OnListChanging(e);
        }

        protected internal virtual void OnWorkflowChangesCompleted(ActivityExecutionContext rootContext)
        {
        }

        internal override Activity TraverseDottedPath(string dottedPath)
        {
            string str = dottedPath;
            string str2 = string.Empty;
            int index = dottedPath.IndexOf('.');
            if (index != -1)
            {
                str = dottedPath.Substring(0, index);
                str2 = dottedPath.Substring(index + 1);
            }
            int num2 = Convert.ToInt32(str, CultureInfo.InvariantCulture);
            if (num2 >= this.activities.Count)
            {
                return null;
            }
            Activity activity = this.activities[num2];
            if (!string.IsNullOrEmpty(str2))
            {
                return activity.TraverseDottedPath(str2);
            }
            return activity;
        }

        protected internal override void Uninitialize(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            foreach (Activity activity in Helpers.GetAllEnabledActivities(this))
            {
                if (activity.ExecutionResult != ActivityExecutionResult.Uninitialized)
                {
                    activity.Uninitialize(provider);
                    activity.SetValue(Activity.ExecutionResultProperty, ActivityExecutionResult.Uninitialized);
                }
            }
            base.Uninitialize(provider);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Browsable(false)]
        public ActivityCollection Activities
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activities;
            }
        }

        protected internal bool CanModifyActivities
        {
            get
            {
                return (bool) base.GetValue(CanModifyActivitiesProperty);
            }
            set
            {
                base.SetValue(CanModifyActivitiesProperty, value);
                if (this.Activities.Count > 0)
                {
                    base.SetValue(Activity.CustomActivityProperty, true);
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ReadOnlyCollection<Activity> EnabledActivities
        {
            get
            {
                List<Activity> list = new List<Activity>();
                foreach (Activity activity in this.activities)
                {
                    if (activity.Enabled && !Helpers.IsFrameworkActivity(activity))
                    {
                        list.Add(activity);
                    }
                }
                return list.AsReadOnly();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        IList<Activity> ISupportAlternateFlow.AlternateFlowActivities
        {
            get
            {
                List<Activity> list = new List<Activity>();
                foreach (Activity activity in this.activities)
                {
                    if (activity.Enabled && Helpers.IsFrameworkActivity(activity))
                    {
                        list.Add(activity);
                    }
                }
                return list.AsReadOnly();
            }
        }
    }
}

