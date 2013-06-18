namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Runtime;
    using System.Runtime.Serialization;

    [DataContract, KnownType(typeof(ActivityCompletionCallbackWrapper)), KnownType(typeof(DelegateCompletionCallbackWrapper))]
    internal abstract class CompletionCallbackWrapper : CallbackWrapper
    {
        [DataMember(EmitDefaultValue=false)]
        private bool checkForCancelation;
        private static Type[] completionCallbackParameters = new Type[] { typeof(NativeActivityContext), typeof(System.Activities.ActivityInstance) };
        private static Type completionCallbackType = typeof(CompletionCallback);
        [DataMember(EmitDefaultValue=false)]
        private bool needsToGatherOutputs;

        protected CompletionCallbackWrapper(Delegate callback, System.Activities.ActivityInstance owningInstance) : base(callback, owningInstance)
        {
        }

        public void CheckForCancelation()
        {
            this.checkForCancelation = true;
        }

        internal System.Activities.Runtime.WorkItem CreateWorkItem(System.Activities.ActivityInstance completedInstance, ActivityExecutor executor)
        {
            CompletionWorkItem item;
            if (this.NeedsToGatherOutputs)
            {
                this.GatherOutputs(completedInstance);
            }
            if (this.checkForCancelation)
            {
                item = new CompletionWithCancelationCheckWorkItem(this, completedInstance);
            }
            else
            {
                item = executor.CompletionWorkItemPool.Acquire();
                item.Initialize(this, completedInstance);
            }
            if (completedInstance.InstanceMap != null)
            {
                completedInstance.InstanceMap.AddEntry(item);
            }
            return item;
        }

        protected virtual void GatherOutputs(System.Activities.ActivityInstance completedInstance)
        {
        }

        protected internal abstract void Invoke(NativeActivityContext context, System.Activities.ActivityInstance completedInstance);

        protected bool NeedsToGatherOutputs
        {
            get
            {
                return this.needsToGatherOutputs;
            }
            set
            {
                this.needsToGatherOutputs = value;
            }
        }

        [DataContract]
        private class CompletionWithCancelationCheckWorkItem : CompletionCallbackWrapper.CompletionWorkItem
        {
            public CompletionWithCancelationCheckWorkItem(CompletionCallbackWrapper callbackWrapper, System.Activities.ActivityInstance completedInstance) : base(callbackWrapper, completedInstance)
            {
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                if ((base.CompletedInstance.State != ActivityInstanceState.Closed) && base.ActivityInstance.IsPerformingDefaultCancelation)
                {
                    base.ActivityInstance.MarkCanceled();
                }
                return base.Execute(executor, bookmarkManager);
            }
        }

        [DataContract]
        public class CompletionWorkItem : ActivityExecutionWorkItem, ActivityInstanceMap.IActivityReference
        {
            [DataMember]
            private CompletionCallbackWrapper callbackWrapper;
            [DataMember]
            private System.Activities.ActivityInstance completedInstance;

            public CompletionWorkItem()
            {
                base.IsPooled = true;
            }

            protected CompletionWorkItem(CompletionCallbackWrapper callbackWrapper, System.Activities.ActivityInstance completedInstance) : base(callbackWrapper.ActivityInstance)
            {
                this.callbackWrapper = callbackWrapper;
                this.completedInstance = completedInstance;
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                NativeActivityContext context = executor.NativeActivityContextPool.Acquire();
                try
                {
                    context.Initialize(base.ActivityInstance, executor, bookmarkManager);
                    this.callbackWrapper.Invoke(context, this.completedInstance);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    base.ExceptionToPropagate = exception;
                }
                finally
                {
                    context.Dispose();
                    executor.NativeActivityContextPool.Release(context);
                    if (base.ActivityInstance.InstanceMap != null)
                    {
                        base.ActivityInstance.InstanceMap.RemoveEntry(this);
                    }
                }
                return true;
            }

            public void Initialize(CompletionCallbackWrapper callbackWrapper, System.Activities.ActivityInstance completedInstance)
            {
                base.Reinitialize(callbackWrapper.ActivityInstance);
                this.callbackWrapper = callbackWrapper;
                this.completedInstance = completedInstance;
            }

            protected override void ReleaseToPool(ActivityExecutor executor)
            {
                base.ClearForReuse();
                this.callbackWrapper = null;
                this.completedInstance = null;
                executor.CompletionWorkItemPool.Release(this);
            }

            void ActivityInstanceMap.IActivityReference.Load(Activity activity, ActivityInstanceMap instanceMap)
            {
                if (this.completedInstance.Activity == null)
                {
                    ((ActivityInstanceMap.IActivityReference) this.completedInstance).Load(activity, instanceMap);
                }
            }

            public override void TraceCompleted()
            {
                if (TD.CompleteCompletionWorkItemIsEnabled())
                {
                    TD.CompleteCompletionWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id, this.completedInstance.Activity.GetType().ToString(), this.completedInstance.Activity.DisplayName, this.completedInstance.Id);
                }
            }

            public override void TraceScheduled()
            {
                if (TD.ScheduleCompletionWorkItemIsEnabled())
                {
                    TD.ScheduleCompletionWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id, this.completedInstance.Activity.GetType().ToString(), this.completedInstance.Activity.DisplayName, this.completedInstance.Id);
                }
            }

            public override void TraceStarting()
            {
                if (TD.StartCompletionWorkItemIsEnabled())
                {
                    TD.StartCompletionWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id, this.completedInstance.Activity.GetType().ToString(), this.completedInstance.Activity.DisplayName, this.completedInstance.Id);
                }
            }

            protected System.Activities.ActivityInstance CompletedInstance
            {
                get
                {
                    return this.completedInstance;
                }
            }

            Activity ActivityInstanceMap.IActivityReference.Activity
            {
                get
                {
                    return this.completedInstance.Activity;
                }
            }
        }
    }
}

