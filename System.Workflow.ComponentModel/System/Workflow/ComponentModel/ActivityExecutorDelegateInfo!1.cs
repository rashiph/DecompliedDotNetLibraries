namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.Runtime;

    [Serializable]
    internal sealed class ActivityExecutorDelegateInfo<T> where T: EventArgs
    {
        private string activityQualifiedName;
        private int contextId;
        private EventHandler<T> delegateValue;
        private IActivityEventListener<T> eventListener;
        private string subscribedActivityQualifiedName;
        private bool wantInTransact;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityExecutorDelegateInfo(EventHandler<T> delegateValue, Activity contextActivity) : this(false, delegateValue, contextActivity)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityExecutorDelegateInfo(IActivityEventListener<T> eventListener, Activity contextActivity) : this(false, eventListener, contextActivity)
        {
        }

        internal ActivityExecutorDelegateInfo(bool useCurrentContext, EventHandler<T> delegateValue, Activity contextActivity)
        {
            this.contextId = -1;
            this.delegateValue = delegateValue;
            Activity target = delegateValue.Target as Activity;
            if (contextActivity.WorkflowCoreRuntime != null)
            {
                if (useCurrentContext)
                {
                    this.contextId = contextActivity.WorkflowCoreRuntime.CurrentActivity.ContextActivity.ContextId;
                }
                else
                {
                    this.contextId = contextActivity.ContextId;
                }
                this.activityQualifiedName = (target ?? contextActivity.WorkflowCoreRuntime.CurrentActivity).QualifiedName;
            }
            else
            {
                this.contextId = 1;
                this.activityQualifiedName = (target ?? contextActivity.RootActivity).QualifiedName;
            }
        }

        internal ActivityExecutorDelegateInfo(bool useCurrentContext, IActivityEventListener<T> eventListener, Activity contextActivity)
        {
            this.contextId = -1;
            this.eventListener = eventListener;
            Activity activity = eventListener as Activity;
            if (contextActivity.WorkflowCoreRuntime != null)
            {
                if (useCurrentContext)
                {
                    this.contextId = contextActivity.WorkflowCoreRuntime.CurrentActivity.ContextActivity.ContextId;
                }
                else
                {
                    this.contextId = contextActivity.ContextId;
                }
                this.activityQualifiedName = (activity ?? contextActivity.WorkflowCoreRuntime.CurrentActivity).QualifiedName;
            }
            else
            {
                this.contextId = 1;
                this.activityQualifiedName = (activity ?? contextActivity.RootActivity).QualifiedName;
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityExecutorDelegateInfo(EventHandler<T> delegateValue, Activity contextActivity, bool wantInTransact) : this(delegateValue, contextActivity)
        {
            this.wantInTransact = wantInTransact;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityExecutorDelegateInfo(IActivityEventListener<T> eventListener, Activity contextActivity, bool wantInTransact) : this(eventListener, contextActivity)
        {
            this.wantInTransact = wantInTransact;
        }

        public override bool Equals(object obj)
        {
            ActivityExecutorDelegateInfo<T> info = obj as ActivityExecutorDelegateInfo<T>;
            if (info == null)
            {
                return false;
            }
            return ((((((info.delegateValue == null) && (this.delegateValue == null)) || ((info.delegateValue != null) && info.delegateValue.Equals(this.delegateValue))) && (((info.eventListener == null) && (this.eventListener == null)) || ((info.eventListener != null) && info.eventListener.Equals(this.eventListener)))) && ((info.activityQualifiedName == this.activityQualifiedName) && (info.contextId == this.contextId))) && (info.wantInTransact == this.wantInTransact));
        }

        private Activity FindExecutorForActivityDown(Activity contextActivity, string activityQualifiedName)
        {
            Queue<Activity> queue = new Queue<Activity>();
            queue.Enqueue(contextActivity);
            while (queue.Count > 0)
            {
                Activity activity = queue.Dequeue();
                Activity activityByName = activity.GetActivityByName(activityQualifiedName, true);
                if ((activityByName != null) && (activityByName.ExecutionStatus != ActivityExecutionStatus.Initialized))
                {
                    return activity;
                }
                IList<Activity> list = (IList<Activity>) activity.GetValue(Activity.ActiveExecutionContextsProperty);
                if (list != null)
                {
                    foreach (Activity activity3 in list)
                    {
                        queue.Enqueue(activity3);
                    }
                }
            }
            return null;
        }

        private Activity FindExecutorForActivityUp(Activity contextActivity, string activityQualifiedName)
        {
            while (contextActivity != null)
            {
                Activity activityByName = contextActivity.GetActivityByName(activityQualifiedName, true);
                if ((activityByName != null) && (activityByName.ExecutionStatus != ActivityExecutionStatus.Initialized))
                {
                    return contextActivity;
                }
                contextActivity = contextActivity.ParentContextActivity;
            }
            return contextActivity;
        }

        public override int GetHashCode()
        {
            if (this.delegateValue == null)
            {
                return (this.eventListener.GetHashCode() ^ this.activityQualifiedName.GetHashCode());
            }
            return this.delegateValue.GetHashCode();
        }

        public void InvokeDelegate(Activity currentContextActivity, T e, bool transacted)
        {
            Activity targetContextActivity = this.FindExecutorForActivityUp(currentContextActivity, this.activityQualifiedName);
            if (targetContextActivity == null)
            {
                targetContextActivity = this.FindExecutorForActivityDown(currentContextActivity, this.activityQualifiedName);
            }
            if (targetContextActivity != null)
            {
                this.InvokeDelegate(currentContextActivity, targetContextActivity, e, false, transacted);
            }
        }

        internal void InvokeDelegate(Activity currentContextActivity, T e, bool sync, bool transacted)
        {
            Activity contextActivityForId = currentContextActivity.WorkflowCoreRuntime.GetContextActivityForId(this.contextId);
            if (contextActivityForId == null)
            {
                contextActivityForId = this.FindExecutorForActivityUp(currentContextActivity, this.activityQualifiedName);
                if (contextActivityForId == null)
                {
                    contextActivityForId = this.FindExecutorForActivityDown(currentContextActivity, this.activityQualifiedName);
                }
            }
            if (contextActivityForId != null)
            {
                this.InvokeDelegate(currentContextActivity, contextActivityForId, e, sync, transacted);
            }
        }

        private void InvokeDelegate(Activity currentContextActivity, Activity targetContextActivity, T e, bool sync, bool transacted)
        {
            ActivityExecutorDelegateOperation<T> item = null;
            if (this.delegateValue != null)
            {
                item = new ActivityExecutorDelegateOperation<T>(this.activityQualifiedName, this.delegateValue, e, this.ContextId);
            }
            else
            {
                item = new ActivityExecutorDelegateOperation<T>(this.activityQualifiedName, this.eventListener, e, this.ContextId);
            }
            bool flag = this.MayInvokeDelegateNow(currentContextActivity);
            if (flag && sync)
            {
                Activity activity = targetContextActivity.GetActivityByName(this.activityQualifiedName);
                using (currentContextActivity.WorkflowCoreRuntime.SetCurrentActivity(activity))
                {
                    item.SynchronousInvoke = true;
                    item.Run(currentContextActivity.WorkflowCoreRuntime);
                    return;
                }
            }
            Activity activityByName = targetContextActivity.GetActivityByName(this.activityQualifiedName);
            currentContextActivity.WorkflowCoreRuntime.ScheduleItem(item, ActivityExecutionContext.IsInAtomicTransaction(activityByName), transacted, !flag);
        }

        private bool MayInvokeDelegateNow(Activity currentContextActivity)
        {
            if ((this.activityQualifiedName == null) || this.wantInTransact)
            {
                return true;
            }
            if (!ActivityExecutionContext.IsInAtomicTransaction(currentContextActivity.WorkflowCoreRuntime.CurrentActivity))
            {
                return true;
            }
            Activity contextActivityForId = currentContextActivity.WorkflowCoreRuntime.GetContextActivityForId(this.contextId);
            if (contextActivityForId == null)
            {
                return false;
            }
            Activity activityByName = contextActivityForId.GetActivityByName(this.activityQualifiedName, true);
            if (activityByName == null)
            {
                return false;
            }
            return ((ActivityExecutionContext.IsInAtomicTransaction(activityByName) && ActivityExecutionContext.IsInAtomicTransaction(currentContextActivity.WorkflowCoreRuntime.CurrentActivity)) || activityByName.MetaEquals(currentContextActivity));
        }

        public string ActivityQualifiedName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activityQualifiedName;
            }
        }

        public int ContextId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.contextId;
            }
        }

        public IActivityEventListener<T> EventListener
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.eventListener;
            }
        }

        public EventHandler<T> HandlerDelegate
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.delegateValue;
            }
        }

        public string SubscribedActivityQualifiedName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.subscribedActivityQualifiedName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.subscribedActivityQualifiedName = value;
            }
        }

        [Serializable]
        private sealed class ActivityExecutorDelegateOperation : SchedulableItem
        {
            private string activityQualifiedName;
            private T args;
            private EventHandler<T> delegateValue;
            private IActivityEventListener<T> eventListener;
            [NonSerialized]
            private bool synchronousInvoke;

            public ActivityExecutorDelegateOperation(string activityQualifiedName, EventHandler<T> delegateValue, T e, int contextId) : base(contextId, activityQualifiedName)
            {
                this.args = default(T);
                this.activityQualifiedName = activityQualifiedName;
                this.delegateValue = delegateValue;
                this.args = e;
            }

            public ActivityExecutorDelegateOperation(string activityQualifiedName, IActivityEventListener<T> eventListener, T e, int contextId) : base(contextId, activityQualifiedName)
            {
                this.args = default(T);
                this.activityQualifiedName = activityQualifiedName;
                this.eventListener = eventListener;
                this.args = e;
            }

            public override bool Run(IWorkflowCoreRuntime workflowCoreRuntime)
            {
                Activity contextActivityForId = workflowCoreRuntime.GetContextActivityForId(base.ContextId);
                ActivityExecutionStatusChangedEventArgs args = this.args as ActivityExecutionStatusChangedEventArgs;
                if (args != null)
                {
                    args.BaseExecutor = workflowCoreRuntime;
                    if (args.Activity == null)
                    {
                        args.BaseExecutor = null;
                        return false;
                    }
                }
                Activity activityByName = contextActivityForId.GetActivityByName(this.activityQualifiedName);
                if (((activityByName == null) || (((activityByName.ExecutionStatus == ActivityExecutionStatus.Closed) || (activityByName.ExecutionStatus == ActivityExecutionStatus.Initialized)) && !this.synchronousInvoke)) || (activityByName.HasPrimaryClosed && !(this.eventListener is ActivityExecutionFilter)))
                {
                    return false;
                }
                try
                {
                    using (workflowCoreRuntime.SetCurrentActivity(activityByName))
                    {
                        using (ActivityExecutionContext context = new ActivityExecutionContext(activityByName))
                        {
                            if (this.delegateValue != null)
                            {
                                this.delegateValue(context, this.args);
                            }
                            else
                            {
                                this.eventListener.OnEvent(context, this.args);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (activityByName != null)
                    {
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 1, "Subscription handler of Activity {0} threw {1}", new object[] { activityByName.QualifiedName, exception.ToString() });
                    }
                    else
                    {
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 1, "Subscription handler threw {0}", new object[] { exception.ToString() });
                    }
                    throw;
                }
                finally
                {
                    if (args != null)
                    {
                        args.BaseExecutor = null;
                    }
                }
                return true;
            }

            public override string ToString()
            {
                return ("SubscriptionEvent((" + base.ContextId.ToString(CultureInfo.CurrentCulture) + ")" + this.activityQualifiedName + ", " + this.args.ToString() + ")");
            }

            internal bool SynchronousInvoke
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.synchronousInvoke;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.synchronousInvoke = value;
                }
            }
        }
    }
}

