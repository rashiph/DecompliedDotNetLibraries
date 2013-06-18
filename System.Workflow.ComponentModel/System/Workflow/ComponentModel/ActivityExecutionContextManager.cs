namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Workflow.Runtime;

    public sealed class ActivityExecutionContextManager
    {
        private List<ActivityExecutionContext> executionContexts = new List<ActivityExecutionContext>();
        private ActivityExecutionContext ownerContext;

        internal ActivityExecutionContextManager(ActivityExecutionContext ownerContext)
        {
            this.ownerContext = ownerContext;
            IList<Activity> list = (IList<Activity>) this.ownerContext.Activity.ContextActivity.GetValue(Activity.ActiveExecutionContextsProperty);
            if (list != null)
            {
                foreach (Activity activity in list)
                {
                    this.executionContexts.Add(new ActivityExecutionContext(activity));
                }
            }
        }

        public void CompleteExecutionContext(ActivityExecutionContext childContext)
        {
            if (this.ownerContext == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContextManager");
            }
            this.CompleteExecutionContext(childContext, false);
        }

        public void CompleteExecutionContext(ActivityExecutionContext childContext, bool forcePersist)
        {
            if (this.ownerContext == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContextManager");
            }
            if (childContext == null)
            {
                throw new ArgumentNullException("childContext");
            }
            if (childContext.Activity == null)
            {
                throw new ArgumentException("childContext", SR.GetString("Error_MissingActivityProperty"));
            }
            if (childContext.Activity.ContextActivity == null)
            {
                throw new ArgumentException("childContext", SR.GetString("Error_MissingContextActivityProperty"));
            }
            if (!this.executionContexts.Contains(childContext))
            {
                throw new ArgumentException();
            }
            if ((childContext.Activity.ContextActivity.ExecutionStatus != ActivityExecutionStatus.Closed) && (childContext.Activity.ContextActivity.ExecutionStatus != ActivityExecutionStatus.Initialized))
            {
                throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, "Error_CannotCompleteContext"));
            }
            ActivityExecutionContextInfo item = childContext.Activity.ContextActivity.GetValue(Activity.ActivityExecutionContextInfoProperty) as ActivityExecutionContextInfo;
            IList<Activity> list = (IList<Activity>) this.ownerContext.Activity.ContextActivity.GetValue(Activity.ActiveExecutionContextsProperty);
            if ((list == null) || !list.Contains(childContext.Activity.ContextActivity))
            {
                throw new ArgumentException();
            }
            bool needsCompensation = childContext.Activity.NeedsCompensation;
            if (needsCompensation || forcePersist)
            {
                List<ActivityExecutionContextInfo> list2 = this.ownerContext.Activity.ContextActivity.GetValue(Activity.CompletedExecutionContextsProperty) as List<ActivityExecutionContextInfo>;
                if (list2 == null)
                {
                    list2 = new List<ActivityExecutionContextInfo>();
                    this.ownerContext.Activity.ContextActivity.SetValue(Activity.CompletedExecutionContextsProperty, list2);
                }
                if (needsCompensation)
                {
                    item.Flags = PersistFlags.NeedsCompensation;
                }
                if (forcePersist)
                {
                    item.Flags = (PersistFlags) ((byte) (item.Flags | PersistFlags.ForcePersist));
                }
                item.SetCompletedOrderId(this.ownerContext.Activity.IncrementCompletedOrderId());
                list2.Add(item);
                this.ownerContext.Activity.WorkflowCoreRuntime.SaveContextActivity(childContext.Activity);
            }
            list.Remove(childContext.Activity.ContextActivity);
            this.executionContexts.Remove(childContext);
            if (childContext.Activity.ContextActivity.CanUninitializeNow && (childContext.Activity.ContextActivity.ExecutionResult != ActivityExecutionResult.Uninitialized))
            {
                childContext.Activity.ContextActivity.Uninitialize(this.ownerContext.Activity.RootActivity.WorkflowCoreRuntime);
                childContext.Activity.ContextActivity.SetValue(Activity.ExecutionResultProperty, ActivityExecutionResult.Uninitialized);
            }
            this.ownerContext.Activity.WorkflowCoreRuntime.UnregisterContextActivity(childContext.Activity);
            if (!needsCompensation && !forcePersist)
            {
                childContext.Activity.Dispose();
            }
        }

        public ActivityExecutionContext CreateExecutionContext(Activity activity)
        {
            ActivityExecutionContext context2;
            if (this.ownerContext == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContextManager");
            }
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (!this.ownerContext.IsValidChild(activity, true))
            {
                throw new ArgumentException(SR.GetString("AEC_InvalidActivity"), "activity");
            }
            Activity item = activity.Clone();
            ((IDependencyObjectAccessor) item).InitializeInstanceForRuntime(this.ownerContext.Activity.WorkflowCoreRuntime);
            Queue<Activity> queue = new Queue<Activity>();
            queue.Enqueue(item);
            while (queue.Count != 0)
            {
                Activity activity3 = queue.Dequeue();
                if (activity3.ExecutionStatus != ActivityExecutionStatus.Initialized)
                {
                    activity3.ResetAllKnownDependencyProperties();
                    CompositeActivity activity4 = activity3 as CompositeActivity;
                    if (activity4 != null)
                    {
                        for (int i = 0; i < activity4.EnabledActivities.Count; i++)
                        {
                            queue.Enqueue(activity4.EnabledActivities[i]);
                        }
                        ISupportAlternateFlow flow = activity4;
                        if (flow != null)
                        {
                            for (int j = 0; j < flow.AlternateFlowActivities.Count; j++)
                            {
                                queue.Enqueue(flow.AlternateFlowActivities[j]);
                            }
                        }
                    }
                }
            }
            IList<Activity> list = (IList<Activity>) this.ownerContext.Activity.ContextActivity.GetValue(Activity.ActiveExecutionContextsProperty);
            if (list == null)
            {
                list = new List<Activity>();
                this.ownerContext.Activity.ContextActivity.SetValue(Activity.ActiveExecutionContextsProperty, list);
            }
            list.Add(item);
            ActivityExecutionContextInfo info = new ActivityExecutionContextInfo(activity.QualifiedName, this.ownerContext.WorkflowCoreRuntime.GetNewContextActivityId(), Guid.NewGuid(), this.ownerContext.ContextId);
            item.SetValue(Activity.ActivityExecutionContextInfoProperty, info);
            item.SetValue(Activity.ActivityContextGuidProperty, info.ContextGuid);
            ActivityExecutionContext context = null;
            try
            {
                this.ownerContext.Activity.WorkflowCoreRuntime.RegisterContextActivity(item);
                context = new ActivityExecutionContext(item);
                this.executionContexts.Add(context);
                context.InitializeActivity(context.Activity);
                context2 = context;
            }
            catch (Exception)
            {
                if (context != null)
                {
                    this.CompleteExecutionContext(context);
                }
                else
                {
                    list.Remove(item);
                }
                throw;
            }
            return context2;
        }

        internal ActivityExecutionContext DiscardPersistedExecutionContext(ActivityExecutionContextInfo contextInfo)
        {
            if (contextInfo == null)
            {
                throw new ArgumentNullException("contextInfo");
            }
            IList<ActivityExecutionContextInfo> list = this.ownerContext.Activity.ContextActivity.GetValue(Activity.CompletedExecutionContextsProperty) as IList<ActivityExecutionContextInfo>;
            if ((list == null) || !list.Contains(contextInfo))
            {
                throw new ArgumentException();
            }
            Activity item = this.ownerContext.WorkflowCoreRuntime.LoadContextActivity(contextInfo, this.ownerContext.Activity.ContextActivity.GetActivityByName(contextInfo.ActivityQualifiedName));
            ((IDependencyObjectAccessor) item).InitializeInstanceForRuntime(this.ownerContext.Activity.WorkflowCoreRuntime);
            IList<Activity> list2 = (IList<Activity>) this.ownerContext.Activity.ContextActivity.GetValue(Activity.ActiveExecutionContextsProperty);
            if (list2 == null)
            {
                list2 = new List<Activity>();
                this.ownerContext.Activity.ContextActivity.SetValue(Activity.ActiveExecutionContextsProperty, list2);
            }
            list2.Add(item);
            this.ownerContext.Activity.WorkflowCoreRuntime.RegisterContextActivity(item);
            ActivityExecutionContext context = new ActivityExecutionContext(item);
            this.executionContexts.Add(context);
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Revoking context {0}:{1}", new object[] { context.ContextId, context.Activity.ContextActivity.QualifiedName });
            list.Remove(contextInfo);
            return context;
        }

        internal void Dispose()
        {
            if (this.ownerContext != null)
            {
                foreach (ActivityExecutionContext context in this.ExecutionContexts)
                {
                    ((IDisposable) context).Dispose();
                }
                this.ownerContext = null;
            }
        }

        public ActivityExecutionContext GetExecutionContext(Activity activity)
        {
            if (this.ownerContext == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContextManager");
            }
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            ActivityExecutionContextInfo info = activity.GetValue(Activity.ActivityExecutionContextInfoProperty) as ActivityExecutionContextInfo;
            foreach (ActivityExecutionContext context in this.ExecutionContexts)
            {
                if (info == null)
                {
                    if (context.Activity.ContextActivity.QualifiedName == activity.QualifiedName)
                    {
                        return context;
                    }
                }
                else if (context.ContextGuid.Equals(info.ContextGuid))
                {
                    return context;
                }
            }
            return null;
        }

        public ActivityExecutionContext GetPersistedExecutionContext(Guid contextGuid)
        {
            if (this.ownerContext == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContextManager");
            }
            IList<ActivityExecutionContextInfo> list = this.ownerContext.Activity.ContextActivity.GetValue(Activity.CompletedExecutionContextsProperty) as IList<ActivityExecutionContextInfo>;
            if (list == null)
            {
                throw new ArgumentException();
            }
            ActivityExecutionContextInfo contextInfo = null;
            foreach (ActivityExecutionContextInfo info2 in list)
            {
                if ((info2.ContextGuid == contextGuid) && (((byte) (info2.Flags & PersistFlags.ForcePersist)) != 0))
                {
                    contextInfo = info2;
                    break;
                }
            }
            if (contextInfo == null)
            {
                throw new ArgumentException();
            }
            contextInfo.Flags = (PersistFlags) ((byte) (((int) contextInfo.Flags) & 0xfd));
            return this.DiscardPersistedExecutionContext(contextInfo);
        }

        internal ReadOnlyCollection<ActivityExecutionContextInfo> CompletedExecutionContexts
        {
            get
            {
                List<ActivityExecutionContextInfo> list = this.ownerContext.Activity.ContextActivity.GetValue(Activity.CompletedExecutionContextsProperty) as List<ActivityExecutionContextInfo>;
                list = (list == null) ? new List<ActivityExecutionContextInfo>() : list;
                return list.AsReadOnly();
            }
        }

        public ReadOnlyCollection<ActivityExecutionContext> ExecutionContexts
        {
            get
            {
                if (this.ownerContext == null)
                {
                    throw new ObjectDisposedException("ActivityExecutionContextManager");
                }
                return new ReadOnlyCollection<ActivityExecutionContext>(this.executionContexts);
            }
        }

        public IEnumerable<Guid> PersistedExecutionContexts
        {
            get
            {
                if (this.ownerContext == null)
                {
                    throw new ObjectDisposedException("ActivityExecutionContextManager");
                }
                List<ActivityExecutionContextInfo> list = this.ownerContext.Activity.ContextActivity.GetValue(Activity.CompletedExecutionContextsProperty) as List<ActivityExecutionContextInfo>;
                list = (list == null) ? new List<ActivityExecutionContextInfo>() : list;
                List<Guid> list2 = new List<Guid>();
                foreach (ActivityExecutionContextInfo info in list)
                {
                    if (((byte) (info.Flags & PersistFlags.ForcePersist)) != 0)
                    {
                        list2.Add(info.ContextGuid);
                    }
                }
                return list2;
            }
        }
    }
}

