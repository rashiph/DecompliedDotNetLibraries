namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Xml;

    public sealed class WorkflowChanges
    {
        private Activity clonedRootActivity;
        public static readonly DependencyProperty ConditionProperty = DependencyProperty.RegisterAttached("Condition", typeof(ActivityCondition), typeof(WorkflowChanges), new PropertyMetadata(DependencyPropertyOptions.Metadata));
        private List<WorkflowChangeAction> modelChangeActions = new List<WorkflowChangeAction>();
        private Activity originalRootActivity;
        private bool saved;
        internal static DependencyProperty WorkflowChangeActionsProperty = DependencyProperty.RegisterAttached("WorkflowChangeActions", typeof(IList), typeof(WorkflowChanges), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));
        internal static DependencyProperty WorkflowChangeVersionProperty = DependencyProperty.RegisterAttached("WorkflowChangeVersion", typeof(Guid), typeof(WorkflowChanges), new PropertyMetadata(Guid.Empty, DependencyPropertyOptions.NonSerialized));

        public WorkflowChanges(Activity rootActivity)
        {
            if (rootActivity == null)
            {
                throw new ArgumentNullException("rootActivity");
            }
            if (!(rootActivity is CompositeActivity) || (rootActivity.Parent != null))
            {
                throw new ArgumentException(SR.GetString("Error_RootActivityTypeInvalid2"), "rootActivity");
            }
            if (rootActivity.DesignMode)
            {
                throw new InvalidOperationException(SR.GetString("Error_NoRuntimeAvailable"));
            }
            this.originalRootActivity = (Activity) rootActivity.GetValue(Activity.WorkflowDefinitionProperty);
            if (this.originalRootActivity == null)
            {
                this.originalRootActivity = rootActivity;
            }
            this.clonedRootActivity = CloneRootActivity(this.originalRootActivity);
            this.ApplyDynamicUpdateMode(this.clonedRootActivity);
        }

        private void ApplyDynamicUpdateMode(Activity seedActivity)
        {
            Queue<Activity> queue = new Queue<Activity>();
            queue.Enqueue(seedActivity);
            while (queue.Count > 0)
            {
                Activity activity = queue.Dequeue();
                activity.Readonly = true;
                activity.DynamicUpdateMode = true;
                foreach (DependencyProperty property in activity.MetaDependencyProperties)
                {
                    if (activity.IsBindingSet(property))
                    {
                        ActivityBind binding = activity.GetBinding(property);
                        if (binding != null)
                        {
                            binding.DynamicUpdateMode = true;
                        }
                    }
                }
                if (activity is CompositeActivity)
                {
                    CompositeActivity activity2 = activity as CompositeActivity;
                    activity2.Activities.ListChanged += new EventHandler<ActivityCollectionChangeEventArgs>(this.OnActivityListChanged);
                    foreach (Activity activity3 in ((CompositeActivity) activity).Activities)
                    {
                        queue.Enqueue(activity3);
                    }
                }
            }
        }

        internal void ApplyTo(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (activity.Parent != null)
            {
                throw new ArgumentException(SR.GetString("Error_RootActivityTypeInvalid"), "activity");
            }
            if (activity.RootActivity == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_MissingRootActivity"));
            }
            if (activity.WorkflowCoreRuntime == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_NoRuntimeAvailable"));
            }
            if (this.saved)
            {
                throw new InvalidOperationException(SR.GetString("Error_TransactionAlreadyApplied"));
            }
            if (!CompareWorkflowDefinition(this.originalRootActivity, (Activity) activity.RootActivity.GetValue(Activity.WorkflowDefinitionProperty)))
            {
                throw new ArgumentException(SR.GetString("Error_WorkflowDefinitionModified"), "activity");
            }
            this.Save();
            IWorkflowCoreRuntime workflowCoreRuntime = activity.WorkflowCoreRuntime;
            if (workflowCoreRuntime.CurrentAtomicActivity != null)
            {
                throw new InvalidOperationException(SR.GetString("Error_InsideAtomicScope"));
            }
            bool flag = workflowCoreRuntime.SuspendInstance(SR.GetString("SuspendReason_WorkflowChange"));
            try
            {
                List<Activity> list = new List<Activity>();
                Queue<Activity> queue = new Queue<Activity>();
                queue.Enqueue(workflowCoreRuntime.RootActivity);
                while (queue.Count > 0)
                {
                    Activity item = queue.Dequeue();
                    list.Add(item);
                    IList<Activity> list2 = (IList<Activity>) item.GetValue(Activity.ActiveExecutionContextsProperty);
                    if (list2 != null)
                    {
                        foreach (Activity activity3 in list2)
                        {
                            queue.Enqueue(activity3);
                        }
                    }
                }
                ValidationErrorCollection errors = new ValidationErrorCollection();
                foreach (WorkflowChangeAction action in this.modelChangeActions)
                {
                    if (action is ActivityChangeAction)
                    {
                        foreach (Activity activity4 in list)
                        {
                            if ((action is RemovedActivityAction) && (activity4.DottedPath == ((RemovedActivityAction) action).OriginalRemovedActivity.DottedPath))
                            {
                                errors.AddRange(action.ValidateChanges(activity4));
                            }
                            if (activity4.TraverseDottedPathFromRoot(((ActivityChangeAction) action).OwnerActivityDottedPath) != null)
                            {
                                errors.AddRange(action.ValidateChanges(activity4));
                            }
                        }
                    }
                }
                if (errors.HasErrors)
                {
                    throw new WorkflowValidationFailedException(SR.GetString("Error_RuntimeValidationFailed"), errors);
                }
                this.VerifyWorkflowCanBeChanged(workflowCoreRuntime);
                workflowCoreRuntime.OnBeforeDynamicChange(this.modelChangeActions);
                workflowCoreRuntime.RootActivity.SetValue(Activity.WorkflowDefinitionProperty, this.clonedRootActivity);
                foreach (Activity activity5 in list)
                {
                    foreach (WorkflowChangeAction action2 in this.modelChangeActions)
                    {
                        if ((action2 is ActivityChangeAction) && (activity5.TraverseDottedPathFromRoot(((ActivityChangeAction) action2).OwnerActivityDottedPath) != null))
                        {
                            action2.ApplyTo(activity5);
                        }
                    }
                    Activity activityByName = this.clonedRootActivity.GetActivityByName(activity5.QualifiedName);
                    if (activityByName != null)
                    {
                        activity5.FixUpMetaProperties(activityByName);
                    }
                    this.NotifyChangesToChildExecutors(workflowCoreRuntime, activity5, this.modelChangeActions);
                    this.NotifyChangesCompletedToChildExecutors(workflowCoreRuntime, activity5);
                }
                workflowCoreRuntime.OnAfterDynamicChange(true, this.modelChangeActions);
            }
            catch
            {
                workflowCoreRuntime.OnAfterDynamicChange(false, this.modelChangeActions);
                throw;
            }
            finally
            {
                if (flag)
                {
                    workflowCoreRuntime.Resume();
                }
            }
        }

        private static Activity CloneRootActivity(Activity originalRootActivity)
        {
            string str = originalRootActivity.GetValue(Activity.WorkflowXamlMarkupProperty) as string;
            string rulesMarkup = null;
            Activity rootActivity = null;
            IServiceProvider serviceProvider = originalRootActivity.GetValue(Activity.WorkflowRuntimeProperty) as IServiceProvider;
            if (!string.IsNullOrEmpty(str))
            {
                rulesMarkup = originalRootActivity.GetValue(Activity.WorkflowRulesMarkupProperty) as string;
                rootActivity = Activity.OnResolveActivityDefinition(null, str, rulesMarkup, true, false, serviceProvider);
            }
            else
            {
                rootActivity = Activity.OnResolveActivityDefinition(originalRootActivity.GetType(), null, null, true, false, serviceProvider);
            }
            if (rootActivity == null)
            {
                throw new NullReferenceException(SR.GetString("Error_InvalidRootForWorkflowChanges"));
            }
            ArrayList workflowChanges = (ArrayList) originalRootActivity.GetValue(WorkflowChangeActionsProperty);
            if (workflowChanges != null)
            {
                workflowChanges = CloneWorkflowChangeActions(workflowChanges, originalRootActivity);
                if (workflowChanges == null)
                {
                    return rootActivity;
                }
                foreach (WorkflowChangeAction action in workflowChanges)
                {
                    action.ApplyTo(rootActivity);
                }
                rootActivity.SetValue(WorkflowChangeActionsProperty, workflowChanges);
            }
            return rootActivity;
        }

        private static ArrayList CloneWorkflowChangeActions(ArrayList workflowChanges, Activity rootActivity)
        {
            if (workflowChanges == null)
            {
                throw new ArgumentNullException("workflowChanges");
            }
            if (rootActivity == null)
            {
                throw new ArgumentNullException("rootActivity");
            }
            string s = null;
            TypeProvider serviceInstance = CreateTypeProvider(rootActivity);
            ServiceContainer provider = new ServiceContainer();
            provider.AddService(typeof(ITypeProvider), serviceInstance);
            DesignerSerializationManager manager = new DesignerSerializationManager(provider);
            WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
            using (manager.CreateSession())
            {
                using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
                {
                    using (XmlWriter writer2 = Helpers.CreateXmlWriter(writer))
                    {
                        WorkflowMarkupSerializationManager serializationManager = new WorkflowMarkupSerializationManager(manager);
                        serializer.Serialize(serializationManager, writer2, workflowChanges);
                        s = writer.ToString();
                    }
                }
                using (StringReader reader = new StringReader(s))
                {
                    using (XmlReader reader2 = XmlReader.Create(reader))
                    {
                        WorkflowMarkupSerializationManager manager3 = new WorkflowMarkupSerializationManager(manager);
                        return (serializer.Deserialize(manager3, reader2) as ArrayList);
                    }
                }
            }
        }

        private static bool CompareWorkflowDefinition(Activity originalWorkflowDefinition, Activity currentWorkflowDefinition)
        {
            if (originalWorkflowDefinition == currentWorkflowDefinition)
            {
                return true;
            }
            if (originalWorkflowDefinition.GetType() != currentWorkflowDefinition.GetType())
            {
                return false;
            }
            Guid guid = (Guid) originalWorkflowDefinition.GetValue(WorkflowChangeVersionProperty);
            Guid guid2 = (Guid) currentWorkflowDefinition.GetValue(WorkflowChangeVersionProperty);
            return (guid == guid2);
        }

        internal static TypeProvider CreateTypeProvider(Activity rootActivity)
        {
            TypeProvider provider = new TypeProvider(null);
            Type type = rootActivity.GetType();
            provider.SetLocalAssembly(type.Assembly);
            provider.AddAssembly(type.Assembly);
            foreach (AssemblyName name in type.Assembly.GetReferencedAssemblies())
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.Load(name);
                    if (assembly != null)
                    {
                        provider.AddAssembly(assembly);
                    }
                }
                catch
                {
                }
                if ((assembly == null) && (name.CodeBase != null))
                {
                    provider.AddAssemblyReference(name.CodeBase);
                }
            }
            return provider;
        }

        private static List<WorkflowChangeAction> DiffTrees(CompositeActivity originalCompositeActivity, CompositeActivity clonedCompositeActivity)
        {
            List<WorkflowChangeAction> list = new List<WorkflowChangeAction>();
            IEnumerator<Activity> enumerator = clonedCompositeActivity.Activities.GetEnumerator();
            IEnumerator<Activity> enumerator2 = originalCompositeActivity.Activities.GetEnumerator();
            int removedActivityIndex = 0;
            while (enumerator2.MoveNext())
            {
                bool flag = false;
                Activity current = enumerator2.Current;
                while (enumerator.MoveNext())
                {
                    Activity activityAdded = enumerator.Current;
                    if (activityAdded.Readonly)
                    {
                        if (current.DottedPath == activityAdded.CachedDottedPath)
                        {
                            removedActivityIndex++;
                            flag = true;
                            if (current is CompositeActivity)
                            {
                                list.AddRange(DiffTrees(current as CompositeActivity, activityAdded as CompositeActivity));
                            }
                        }
                        else
                        {
                            list.Add(new RemovedActivityAction(removedActivityIndex, current, clonedCompositeActivity));
                            while (enumerator2.MoveNext())
                            {
                                current = enumerator2.Current;
                                if (current.DottedPath == activityAdded.CachedDottedPath)
                                {
                                    removedActivityIndex++;
                                    flag = true;
                                    if (current is CompositeActivity)
                                    {
                                        list.AddRange(DiffTrees(current as CompositeActivity, activityAdded as CompositeActivity));
                                    }
                                    break;
                                }
                                list.Add(new RemovedActivityAction(removedActivityIndex, current, clonedCompositeActivity));
                            }
                        }
                        break;
                    }
                    list.Add(new AddedActivityAction(clonedCompositeActivity, activityAdded));
                    removedActivityIndex++;
                }
                if (!flag)
                {
                    list.Add(new RemovedActivityAction(removedActivityIndex, current, clonedCompositeActivity));
                }
            }
            while (enumerator.MoveNext())
            {
                list.Add(new AddedActivityAction(clonedCompositeActivity, enumerator.Current));
            }
            return list;
        }

        public static object GetCondition(object dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException("dependencyObject");
            }
            if (!(dependencyObject is DependencyObject))
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(DependencyObject).FullName }), "dependencyObject");
            }
            return (dependencyObject as DependencyObject).GetValue(ConditionProperty);
        }

        internal static bool IsActivityExecutable(Activity activity)
        {
            if (!activity.Enabled)
            {
                return false;
            }
            if (activity.Parent != null)
            {
                return IsActivityExecutable(activity.Parent);
            }
            return activity.Enabled;
        }

        private void NotifyChangesCompletedToChildExecutors(IWorkflowCoreRuntime workflowCoreRuntime, Activity contextActivity)
        {
            Queue queue = new Queue();
            queue.Enqueue(contextActivity);
            while (queue.Count > 0)
            {
                CompositeActivity activity = queue.Dequeue() as CompositeActivity;
                if ((activity != null) && IsActivityExecutable(activity))
                {
                    ISupportWorkflowChanges activityExecutor = ActivityExecutors.GetActivityExecutor(activity) as ISupportWorkflowChanges;
                    if (activityExecutor != null)
                    {
                        using (workflowCoreRuntime.SetCurrentActivity(activity))
                        {
                            using (ActivityExecutionContext context = new ActivityExecutionContext(activity))
                            {
                                activityExecutor.OnWorkflowChangesCompleted(context);
                            }
                        }
                    }
                    foreach (Activity activity2 in activity.Activities)
                    {
                        if (activity2 is CompositeActivity)
                        {
                            queue.Enqueue(activity2);
                        }
                    }
                }
            }
        }

        private void NotifyChangesToChildExecutors(IWorkflowCoreRuntime workflowCoreRuntime, Activity contextActivity, IList<WorkflowChangeAction> changeActions)
        {
            foreach (WorkflowChangeAction action in changeActions)
            {
                if (action is ActivityChangeAction)
                {
                    CompositeActivity activity = contextActivity.TraverseDottedPathFromRoot(((ActivityChangeAction) action).OwnerActivityDottedPath) as CompositeActivity;
                    if ((activity != null) && IsActivityExecutable(activity))
                    {
                        ISupportWorkflowChanges activityExecutor = ActivityExecutors.GetActivityExecutor(activity) as ISupportWorkflowChanges;
                        if (activityExecutor == null)
                        {
                            throw new ApplicationException(SR.GetString("Error_WorkflowChangesNotSupported", new object[] { activity.GetType().FullName }));
                        }
                        using (workflowCoreRuntime.SetCurrentActivity(activity))
                        {
                            using (ActivityExecutionContext context = new ActivityExecutionContext(activity))
                            {
                                if (action is AddedActivityAction)
                                {
                                    Activity activity2 = activity.Activities[((AddedActivityAction) action).Index];
                                    if (IsActivityExecutable(activity2))
                                    {
                                        activity2.OnActivityExecutionContextLoad(context.Activity.RootActivity.WorkflowCoreRuntime);
                                        context.InitializeActivity(activity2);
                                        activityExecutor.OnActivityAdded(context, activity2);
                                    }
                                }
                                else if (action is RemovedActivityAction)
                                {
                                    RemovedActivityAction action2 = (RemovedActivityAction) action;
                                    if (IsActivityExecutable(action2.OriginalRemovedActivity))
                                    {
                                        activityExecutor.OnActivityRemoved(context, action2.OriginalRemovedActivity);
                                        if (action2.OriginalRemovedActivity.ExecutionResult != ActivityExecutionResult.Uninitialized)
                                        {
                                            action2.OriginalRemovedActivity.Uninitialize(context.Activity.RootActivity.WorkflowCoreRuntime);
                                            action2.OriginalRemovedActivity.SetValue(Activity.ExecutionResultProperty, ActivityExecutionResult.Uninitialized);
                                        }
                                        action2.OriginalRemovedActivity.OnActivityExecutionContextUnload(context.Activity.RootActivity.WorkflowCoreRuntime);
                                        action2.OriginalRemovedActivity.Dispose();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnActivityListChanged(object sender, ActivityCollectionChangeEventArgs e)
        {
            if (e.RemovedItems != null)
            {
                foreach (Activity activity in e.RemovedItems)
                {
                    if (activity.Readonly)
                    {
                        this.ReleaseDynamicUpdateMode(activity);
                    }
                }
            }
        }

        private void ReleaseDynamicUpdateMode(Activity seedActivity)
        {
            Queue<Activity> queue = new Queue<Activity>();
            queue.Enqueue(seedActivity);
            while (queue.Count > 0)
            {
                Activity activity = queue.Dequeue();
                activity.Readonly = false;
                activity.DynamicUpdateMode = false;
                foreach (DependencyProperty property in activity.MetaDependencyProperties)
                {
                    if (activity.IsBindingSet(property))
                    {
                        ActivityBind binding = activity.GetBinding(property);
                        if (binding != null)
                        {
                            binding.DynamicUpdateMode = false;
                        }
                    }
                }
                if (activity is CompositeActivity)
                {
                    CompositeActivity activity2 = activity as CompositeActivity;
                    activity2.Activities.ListChanged -= new EventHandler<ActivityCollectionChangeEventArgs>(this.OnActivityListChanged);
                    foreach (Activity activity3 in ((CompositeActivity) activity).Activities)
                    {
                        queue.Enqueue(activity3);
                    }
                }
            }
        }

        private void Save()
        {
            ValidationErrorCollection errors = this.Validate();
            if (errors.HasErrors)
            {
                throw new WorkflowValidationFailedException(SR.GetString("Error_CompilerValidationFailed"), errors);
            }
            object originalDefinition = this.originalRootActivity.GetValue(ConditionTypeConverter.DeclarativeConditionDynamicProp);
            object changedDefinition = this.clonedRootActivity.GetValue(ConditionTypeConverter.DeclarativeConditionDynamicProp);
            if (originalDefinition != null)
            {
                this.modelChangeActions.AddRange(((IWorkflowChangeDiff) originalDefinition).Diff(originalDefinition, changedDefinition));
            }
            else if (changedDefinition != null)
            {
                this.modelChangeActions.AddRange(((IWorkflowChangeDiff) changedDefinition).Diff(originalDefinition, changedDefinition));
            }
            this.modelChangeActions.AddRange(DiffTrees(this.originalRootActivity as CompositeActivity, this.clonedRootActivity as CompositeActivity));
            this.ReleaseDynamicUpdateMode(this.clonedRootActivity);
            ArrayList list = (ArrayList) this.clonedRootActivity.GetValue(WorkflowChangeActionsProperty);
            if (list == null)
            {
                list = new ArrayList();
                this.clonedRootActivity.SetValue(WorkflowChangeActionsProperty, list);
            }
            list.AddRange(this.modelChangeActions);
            this.clonedRootActivity.SetValue(WorkflowChangeVersionProperty, Guid.NewGuid());
            this.saved = true;
            ((IDependencyObjectAccessor) this.clonedRootActivity).InitializeDefinitionForRuntime(null);
        }

        public static void SetCondition(object dependencyObject, object value)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException("dependencyObject");
            }
            if (!(dependencyObject is DependencyObject))
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(DependencyObject).FullName }), "dependencyObject");
            }
            (dependencyObject as DependencyObject).SetValue(ConditionProperty, value);
        }

        public ValidationErrorCollection Validate()
        {
            ValidationErrorCollection errors;
            TypeProvider serviceInstance = CreateTypeProvider(this.originalRootActivity);
            ServiceContainer serviceProvider = new ServiceContainer();
            serviceProvider.AddService(typeof(ITypeProvider), serviceInstance);
            ValidationManager manager = new ValidationManager(serviceProvider);
            using (WorkflowCompilationContext.CreateScope(manager))
            {
                errors = ValidationHelpers.ValidateObject(manager, this.clonedRootActivity);
            }
            return XomlCompilerHelper.MorphIntoFriendlyValidationErrors(errors);
        }

        private void VerifyWorkflowCanBeChanged(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            ActivityCondition condition = workflowCoreRuntime.RootActivity.GetValue(ConditionProperty) as ActivityCondition;
            if (condition != null)
            {
                using (workflowCoreRuntime.SetCurrentActivity(workflowCoreRuntime.RootActivity))
                {
                    if (!condition.Evaluate(workflowCoreRuntime.RootActivity, workflowCoreRuntime))
                    {
                        throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, "Error_DynamicUpdateEvaluation", new object[] { workflowCoreRuntime.InstanceID.ToString() }));
                    }
                }
            }
        }

        public CompositeActivity TransientWorkflow
        {
            get
            {
                return (this.clonedRootActivity as CompositeActivity);
            }
        }
    }
}

