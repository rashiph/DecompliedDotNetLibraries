namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Activities.Tracking;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public class NativeActivityContext : ActivityContext
    {
        private BookmarkManager bookmarkManager;
        private ActivityExecutor executor;

        internal NativeActivityContext()
        {
        }

        internal NativeActivityContext(System.Activities.ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager) : base(instance, executor)
        {
            this.executor = executor;
            this.bookmarkManager = bookmarkManager;
        }

        public void Abort()
        {
            this.Abort(null);
        }

        public void Abort(Exception reason)
        {
            base.ThrowIfDisposed();
            this.executor.AbortWorkflowInstance(reason);
        }

        public void AbortChildInstance(System.Activities.ActivityInstance activity)
        {
            this.AbortChildInstance(activity, null);
        }

        public void AbortChildInstance(System.Activities.ActivityInstance activity, Exception reason)
        {
            base.ThrowIfDisposed();
            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull("activity");
            }
            if (!activity.IsCompleted)
            {
                if (!object.ReferenceEquals(activity.Parent, base.CurrentInstance))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CanOnlyAbortDirectChildren));
                }
                this.executor.AbortActivityInstance(activity, reason);
            }
        }

        internal void Cancel()
        {
            base.ThrowIfDisposed();
            base.CurrentInstance.BaseCancel(this);
        }

        public void CancelChild(System.Activities.ActivityInstance activityInstance)
        {
            base.ThrowIfDisposed();
            if (activityInstance == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityInstance");
            }
            if (!activityInstance.IsCompleted)
            {
                if (!object.ReferenceEquals(activityInstance.Parent, base.CurrentInstance))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CanOnlyCancelDirectChildren));
                }
                this.executor.CancelActivity(activityInstance);
            }
        }

        public void CancelChildren()
        {
            base.ThrowIfDisposed();
            base.CurrentInstance.CancelChildren(this);
        }

        internal void CompleteTransaction(RuntimeTransactionHandle handle, BookmarkCallback callback)
        {
            if (callback != null)
            {
                this.ThrowIfCanInduceIdleNotSet();
            }
            this.executor.CompleteTransaction(handle, callback, base.CurrentInstance);
        }

        public Bookmark CreateBookmark()
        {
            return this.CreateBookmark((BookmarkCallback) null);
        }

        public Bookmark CreateBookmark(BookmarkCallback callback)
        {
            return this.CreateBookmark(callback, BookmarkOptions.None);
        }

        public Bookmark CreateBookmark(string name)
        {
            base.ThrowIfDisposed();
            this.ThrowIfCanInduceIdleNotSet();
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }
            return this.bookmarkManager.CreateBookmark(name, null, base.CurrentInstance, BookmarkOptions.None);
        }

        public Bookmark CreateBookmark(BookmarkCallback callback, BookmarkOptions options)
        {
            base.ThrowIfDisposed();
            this.ThrowIfCanInduceIdleNotSet();
            if ((callback != null) && !CallbackWrapper.IsValidCallback(callback, base.CurrentInstance))
            {
                throw FxTrace.Exception.Argument("callback", System.Activities.SR.InvalidExecutionCallback(callback, base.Activity.ToString()));
            }
            BookmarkOptionsHelper.Validate(options, "options");
            return this.bookmarkManager.CreateBookmark(callback, base.CurrentInstance, options);
        }

        public Bookmark CreateBookmark(string name, BookmarkCallback callback)
        {
            return this.CreateBookmark(name, callback, BookmarkOptions.None);
        }

        public Bookmark CreateBookmark(string name, BookmarkCallback callback, BookmarkOptions options)
        {
            base.ThrowIfDisposed();
            this.ThrowIfCanInduceIdleNotSet();
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }
            if (callback == null)
            {
                throw FxTrace.Exception.ArgumentNull("callback");
            }
            if (!CallbackWrapper.IsValidCallback(callback, base.CurrentInstance))
            {
                throw FxTrace.Exception.Argument("callback", System.Activities.SR.InvalidExecutionCallback(callback, base.Activity.ToString()));
            }
            BookmarkOptionsHelper.Validate(options, "options");
            return this.bookmarkManager.CreateBookmark(name, callback, base.CurrentInstance, options);
        }

        public Bookmark CreateBookmark(string name, BookmarkCallback callback, BookmarkScope scope)
        {
            return this.CreateBookmark(name, callback, scope, BookmarkOptions.None);
        }

        public Bookmark CreateBookmark(string name, BookmarkCallback callback, BookmarkScope scope, BookmarkOptions options)
        {
            base.ThrowIfDisposed();
            this.ThrowIfCanInduceIdleNotSet();
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }
            if (!CallbackWrapper.IsValidCallback(callback, base.CurrentInstance))
            {
                throw FxTrace.Exception.Argument("callback", System.Activities.SR.InvalidExecutionCallback(callback, base.Activity.ToString()));
            }
            if (scope == null)
            {
                throw FxTrace.Exception.ArgumentNull("scope");
            }
            BookmarkOptionsHelper.Validate(options, "options");
            return this.executor.BookmarkScopeManager.CreateBookmark(name, scope, callback, base.CurrentInstance, options);
        }

        internal BookmarkScope CreateBookmarkScope()
        {
            return this.CreateBookmarkScope(Guid.Empty);
        }

        internal BookmarkScope CreateBookmarkScope(Guid scopeId)
        {
            return this.CreateBookmarkScope(scopeId, null);
        }

        internal BookmarkScope CreateBookmarkScope(Guid scopeId, BookmarkScopeHandle scopeHandle)
        {
            if ((scopeId != Guid.Empty) && !this.executor.KeysAllowed)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.BookmarkScopesRequireKeys));
            }
            return this.executor.BookmarkScopeManager.CreateAndRegisterScope(scopeId, scopeHandle);
        }

        internal void EnterNoPersist(NoPersistHandle handle)
        {
            base.ThrowIfDisposed();
            ExecutionProperties executionProperties = this.GetExecutionProperties(handle);
            NoPersistProperty property = (NoPersistProperty) executionProperties.FindAtCurrentScope("System.Activities.NoPersistProperty");
            if (property == null)
            {
                property = this.executor.CreateNoPersistProperty();
                executionProperties.Add("System.Activities.NoPersistProperty", property, true, false);
            }
            property.Enter();
        }

        internal void ExitNoPersist(NoPersistHandle handle)
        {
            base.ThrowIfDisposed();
            ExecutionProperties executionProperties = this.GetExecutionProperties(handle);
            NoPersistProperty property = (NoPersistProperty) executionProperties.FindAtCurrentScope("System.Activities.NoPersistProperty");
            if (property == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.UnmatchedNoPersistExit));
            }
            if (property.Exit())
            {
                executionProperties.Remove("System.Activities.NoPersistProperty", true);
            }
        }

        private System.Activities.ActivityInstance FindDeclaringActivityInstance(System.Activities.ActivityInstance startingInstance, Activity activityToMatch)
        {
            for (System.Activities.ActivityInstance instance = startingInstance; instance != null; instance = instance.Parent)
            {
                if (object.ReferenceEquals(instance.Activity, activityToMatch))
                {
                    return instance;
                }
            }
            return null;
        }

        public ReadOnlyCollection<System.Activities.ActivityInstance> GetChildren()
        {
            base.ThrowIfDisposed();
            return base.CurrentInstance.GetChildren();
        }

        private ExecutionProperties GetExecutionProperties(Handle handle)
        {
            if (handle.Owner == base.CurrentInstance)
            {
                return this.Properties;
            }
            if (handle.Owner == null)
            {
                return new ExecutionProperties(this, null, this.executor.RootPropertyManager);
            }
            return new ExecutionProperties(this, handle.Owner, handle.Owner.PropertyManager);
        }

        public object GetValue(Variable variable)
        {
            base.ThrowIfDisposed();
            if (variable == null)
            {
                throw FxTrace.Exception.ArgumentNull("variable");
            }
            return base.GetValueCore<object>(variable);
        }

        public T GetValue<T>(Variable<T> variable)
        {
            base.ThrowIfDisposed();
            if (variable == null)
            {
                throw FxTrace.Exception.ArgumentNull("variable");
            }
            return base.GetValueCore<T>(variable);
        }

        internal void Initialize(System.Activities.ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            base.Reinitialize(instance, executor);
            this.executor = executor;
            this.bookmarkManager = bookmarkManager;
        }

        internal void InitializeBookmarkScope(BookmarkScope scope, Guid id)
        {
            base.ThrowIfDisposed();
            if (!this.executor.KeysAllowed)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.BookmarkScopesRequireKeys));
            }
            this.executor.BookmarkScopeManager.InitializeScope(scope, id);
        }

        private System.Activities.ActivityInstance InternalScheduleActivity(Activity activity, CompletionBookmark onCompleted, FaultBookmark onFaulted)
        {
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (!activity.IsMetadataCached || (activity.CacheId != currentInstance.Activity.CacheId))
            {
                throw FxTrace.Exception.Argument("activity", System.Activities.SR.ActivityNotPartOfThisTree(activity.DisplayName, currentInstance.Activity.DisplayName));
            }
            if (!activity.CanBeScheduledBy(currentInstance.Activity))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CanOnlyScheduleDirectChildren(currentInstance.Activity.DisplayName, activity.DisplayName, activity.Parent.DisplayName)));
            }
            if (activity.HandlerOf != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.DelegateHandlersCannotBeScheduledDirectly(currentInstance.Activity.DisplayName, activity.DisplayName)));
            }
            if (currentInstance.WaitingForTransactionContext)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotScheduleChildrenWhileEnteringIsolation));
            }
            if (currentInstance.IsPerformingDefaultCancelation)
            {
                currentInstance.MarkCanceled();
                return System.Activities.ActivityInstance.CreateCanceledInstance(activity);
            }
            return this.executor.ScheduleActivity(activity, currentInstance, onCompleted, onFaulted, null);
        }

        private System.Activities.ActivityInstance InternalScheduleDelegate(ActivityDelegate activityDelegate, IDictionary<string, object> inputParameters, CompletionBookmark completionBookmark, FaultBookmark faultBookmark)
        {
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityDelegate.Handler != null)
            {
                Activity handler = activityDelegate.Handler;
                if (!handler.IsMetadataCached || (handler.CacheId != currentInstance.Activity.CacheId))
                {
                    throw FxTrace.Exception.Argument("activity", System.Activities.SR.ActivityNotPartOfThisTree(handler.DisplayName, currentInstance.Activity.DisplayName));
                }
            }
            if (activityDelegate.Owner == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ActivityDelegateOwnerMissing(activityDelegate)));
            }
            if (!activityDelegate.CanBeScheduledBy(currentInstance.Activity))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CanOnlyScheduleDirectChildren(currentInstance.Activity.DisplayName, activityDelegate.DisplayName, activityDelegate.Owner.DisplayName)));
            }
            if (currentInstance.WaitingForTransactionContext)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotScheduleChildrenWhileEnteringIsolation));
            }
            System.Activities.ActivityInstance instance2 = this.FindDeclaringActivityInstance(base.CurrentInstance, activityDelegate.Owner);
            if (currentInstance.IsPerformingDefaultCancelation)
            {
                currentInstance.MarkCanceled();
                return System.Activities.ActivityInstance.CreateCanceledInstance(activityDelegate.Handler);
            }
            return this.executor.ScheduleDelegate(activityDelegate, inputParameters, currentInstance, instance2.Environment, completionBookmark, faultBookmark);
        }

        public void MarkCanceled()
        {
            base.ThrowIfDisposed();
            if (!base.CurrentInstance.IsCancellationRequested)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.MarkCanceledOnlyCallableIfCancelRequested));
            }
            base.CurrentInstance.MarkCanceled();
        }

        internal void RegisterMainRootCompleteCallback(Bookmark bookmark)
        {
            this.executor.RegisterMainRootCompleteCallback(bookmark);
        }

        public void RemoveAllBookmarks()
        {
            base.ThrowIfDisposed();
            base.CurrentInstance.RemoveAllBookmarks(this.executor.RawBookmarkScopeManager, this.bookmarkManager);
        }

        public bool RemoveBookmark(Bookmark bookmark)
        {
            base.ThrowIfDisposed();
            if (bookmark == null)
            {
                throw FxTrace.Exception.ArgumentNull("bookmark");
            }
            return this.bookmarkManager.Remove(bookmark, base.CurrentInstance);
        }

        public bool RemoveBookmark(string name)
        {
            base.ThrowIfDisposed();
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNull("name");
            }
            return this.RemoveBookmark(new Bookmark(name));
        }

        public bool RemoveBookmark(string name, BookmarkScope scope)
        {
            base.ThrowIfDisposed();
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }
            if (scope == null)
            {
                throw FxTrace.Exception.ArgumentNull("scope");
            }
            return this.executor.BookmarkScopeManager.RemoveBookmark(new Bookmark(name), scope, base.CurrentInstance);
        }

        internal void RequestPersist(BookmarkCallback onPersistComplete)
        {
            Bookmark onPersistBookmark = this.CreateBookmark(onPersistComplete);
            this.executor.RequestPersist(onPersistBookmark, base.CurrentInstance);
        }

        internal void RequestTransactionContext(bool isRequires, RuntimeTransactionHandle handle, Action<NativeActivityTransactionContext, object> callback, object state)
        {
            this.executor.RequestTransactionContext(base.CurrentInstance, isRequires, handle, callback, state);
        }

        public BookmarkResumptionResult ResumeBookmark(Bookmark bookmark, object value)
        {
            base.ThrowIfDisposed();
            if (bookmark == null)
            {
                throw FxTrace.Exception.ArgumentNull("bookmark");
            }
            return this.executor.TryResumeUserBookmark(bookmark, value, false);
        }

        internal void RethrowException(FaultContext context)
        {
            this.executor.RethrowException(base.CurrentInstance, context);
        }

        public System.Activities.ActivityInstance ScheduleAction(ActivityAction activityAction, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            return this.InternalScheduleDelegate(activityAction, ActivityUtilities.EmptyParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleAction<T>(ActivityAction<T> activityAction, T argument, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(1);
            dictionary2.Add(ActivityDelegate.ArgumentName, argument);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityAction, inputParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleAction<T1, T2>(ActivityAction<T1, T2> activityAction, T1 argument1, T2 argument2, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(2);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityAction, inputParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleAction<T1, T2, T3>(ActivityAction<T1, T2, T3> activityAction, T1 argument1, T2 argument2, T3 argument3, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(3);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityAction, inputParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleAction<T1, T2, T3, T4>(ActivityAction<T1, T2, T3, T4> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(4);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityAction, inputParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleAction<T1, T2, T3, T4, T5>(ActivityAction<T1, T2, T3, T4, T5> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(5);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityAction, inputParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6>(ActivityAction<T1, T2, T3, T4, T5, T6> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(6);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityAction, inputParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7>(ActivityAction<T1, T2, T3, T4, T5, T6, T7> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(7);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityAction, inputParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8>(ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(8);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityAction, inputParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(9);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            dictionary2.Add(ActivityDelegate.Argument9Name, argument9);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityAction, inputParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(10);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            dictionary2.Add(ActivityDelegate.Argument9Name, argument9);
            dictionary2.Add(ActivityDelegate.Argument10Name, argument10);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityAction, inputParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(11);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            dictionary2.Add(ActivityDelegate.Argument9Name, argument9);
            dictionary2.Add(ActivityDelegate.Argument10Name, argument10);
            dictionary2.Add(ActivityDelegate.Argument11Name, argument11);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityAction, inputParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(12);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            dictionary2.Add(ActivityDelegate.Argument9Name, argument9);
            dictionary2.Add(ActivityDelegate.Argument10Name, argument10);
            dictionary2.Add(ActivityDelegate.Argument11Name, argument11);
            dictionary2.Add(ActivityDelegate.Argument12Name, argument12);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityAction, inputParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(13);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            dictionary2.Add(ActivityDelegate.Argument9Name, argument9);
            dictionary2.Add(ActivityDelegate.Argument10Name, argument10);
            dictionary2.Add(ActivityDelegate.Argument11Name, argument11);
            dictionary2.Add(ActivityDelegate.Argument12Name, argument12);
            dictionary2.Add(ActivityDelegate.Argument13Name, argument13);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityAction, inputParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(14);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            dictionary2.Add(ActivityDelegate.Argument9Name, argument9);
            dictionary2.Add(ActivityDelegate.Argument10Name, argument10);
            dictionary2.Add(ActivityDelegate.Argument11Name, argument11);
            dictionary2.Add(ActivityDelegate.Argument12Name, argument12);
            dictionary2.Add(ActivityDelegate.Argument13Name, argument13);
            dictionary2.Add(ActivityDelegate.Argument14Name, argument14);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityAction, inputParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14, T15 argument15, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(15);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            dictionary2.Add(ActivityDelegate.Argument9Name, argument9);
            dictionary2.Add(ActivityDelegate.Argument10Name, argument10);
            dictionary2.Add(ActivityDelegate.Argument11Name, argument11);
            dictionary2.Add(ActivityDelegate.Argument12Name, argument12);
            dictionary2.Add(ActivityDelegate.Argument13Name, argument13);
            dictionary2.Add(ActivityDelegate.Argument14Name, argument14);
            dictionary2.Add(ActivityDelegate.Argument15Name, argument15);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityAction, inputParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14, T15 argument15, T16 argument16, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(0x10);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            dictionary2.Add(ActivityDelegate.Argument9Name, argument9);
            dictionary2.Add(ActivityDelegate.Argument10Name, argument10);
            dictionary2.Add(ActivityDelegate.Argument11Name, argument11);
            dictionary2.Add(ActivityDelegate.Argument12Name, argument12);
            dictionary2.Add(ActivityDelegate.Argument13Name, argument13);
            dictionary2.Add(ActivityDelegate.Argument14Name, argument14);
            dictionary2.Add(ActivityDelegate.Argument15Name, argument15);
            dictionary2.Add(ActivityDelegate.Argument16Name, argument16);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityAction, inputParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleActivity(Activity activity)
        {
            return this.ScheduleActivity(activity, null, null);
        }

        public System.Activities.ActivityInstance ScheduleActivity(Activity activity, CompletionCallback onCompleted)
        {
            return this.ScheduleActivity(activity, onCompleted, null);
        }

        public System.Activities.ActivityInstance ScheduleActivity(Activity activity, FaultCallback onFaulted)
        {
            return this.ScheduleActivity(activity, null, onFaulted);
        }

        public System.Activities.ActivityInstance ScheduleActivity(Activity activity, CompletionCallback onCompleted, FaultCallback onFaulted)
        {
            base.ThrowIfDisposed();
            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull("activity");
            }
            CompletionBookmark bookmark = null;
            FaultBookmark bookmark2 = null;
            if (onCompleted != null)
            {
                if (!CallbackWrapper.IsValidCallback(onCompleted, base.CurrentInstance))
                {
                    throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, base.Activity.ToString()));
                }
                bookmark = ActivityUtilities.CreateCompletionBookmark(onCompleted, base.CurrentInstance);
            }
            if (onFaulted != null)
            {
                if (!CallbackWrapper.IsValidCallback(onFaulted, base.CurrentInstance))
                {
                    throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, base.Activity.ToString()));
                }
                bookmark2 = ActivityUtilities.CreateFaultBookmark(onFaulted, base.CurrentInstance);
            }
            return this.InternalScheduleActivity(activity, bookmark, bookmark2);
        }

        public System.Activities.ActivityInstance ScheduleActivity<TResult>(Activity<TResult> activity, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull("activity");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            return this.InternalScheduleActivity(activity, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleDelegate(ActivityDelegate activityDelegate, IDictionary<string, object> inputParameters, DelegateCompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityDelegate == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityDelegate");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            IEnumerable<RuntimeDelegateArgument> source = from p in activityDelegate.RuntimeDelegateArguments
                where ArgumentDirectionHelper.IsIn(p.Direction)
                select p;
            int num = source.Count<RuntimeDelegateArgument>();
            if (((inputParameters == null) && (num > 0)) || ((inputParameters != null) && (inputParameters.Count != num)))
            {
                throw FxTrace.Exception.Argument("inputParameters", System.Activities.SR.InputParametersCountMismatch((inputParameters == null) ? 0 : inputParameters.Count, num));
            }
            if (num > 0)
            {
                foreach (RuntimeDelegateArgument argument in source)
                {
                    object obj2 = null;
                    string name = argument.Name;
                    if (!inputParameters.TryGetValue(name, out obj2))
                    {
                        throw FxTrace.Exception.Argument("inputParameters", System.Activities.SR.InputParametersMissing(argument.Name));
                    }
                    if (!System.Runtime.TypeHelper.AreTypesCompatible(obj2, argument.Type))
                    {
                        throw FxTrace.Exception.Argument("inputParameters", System.Activities.SR.InputParametersTypeMismatch(argument.Type, name));
                    }
                }
            }
            return this.InternalScheduleDelegate(activityDelegate, inputParameters ?? ActivityUtilities.EmptyParameters, ActivityUtilities.CreateCompletionBookmark(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<TResult>(ActivityFunc<TResult> activityFunc, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            return this.InternalScheduleDelegate(activityFunc, ActivityUtilities.EmptyParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<T, TResult>(ActivityFunc<T, TResult> activityFunc, T argument, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(1);
            dictionary2.Add(ActivityDelegate.ArgumentName, argument);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityFunc, inputParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<T1, T2, TResult>(ActivityFunc<T1, T2, TResult> activityFunc, T1 argument1, T2 argument2, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(2);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityFunc, inputParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<T1, T2, T3, TResult>(ActivityFunc<T1, T2, T3, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(3);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityFunc, inputParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<T1, T2, T3, T4, TResult>(ActivityFunc<T1, T2, T3, T4, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(4);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityFunc, inputParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, TResult>(ActivityFunc<T1, T2, T3, T4, T5, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(5);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityFunc, inputParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, TResult>(ActivityFunc<T1, T2, T3, T4, T5, T6, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(6);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityFunc, inputParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, TResult>(ActivityFunc<T1, T2, T3, T4, T5, T6, T7, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(7);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityFunc, inputParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(8);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityFunc, inputParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(9);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            dictionary2.Add(ActivityDelegate.Argument9Name, argument9);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityFunc, inputParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(10);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            dictionary2.Add(ActivityDelegate.Argument9Name, argument9);
            dictionary2.Add(ActivityDelegate.Argument10Name, argument10);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityFunc, inputParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(11);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            dictionary2.Add(ActivityDelegate.Argument9Name, argument9);
            dictionary2.Add(ActivityDelegate.Argument10Name, argument10);
            dictionary2.Add(ActivityDelegate.Argument11Name, argument11);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityFunc, inputParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(12);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            dictionary2.Add(ActivityDelegate.Argument9Name, argument9);
            dictionary2.Add(ActivityDelegate.Argument10Name, argument10);
            dictionary2.Add(ActivityDelegate.Argument11Name, argument11);
            dictionary2.Add(ActivityDelegate.Argument12Name, argument12);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityFunc, inputParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(13);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            dictionary2.Add(ActivityDelegate.Argument9Name, argument9);
            dictionary2.Add(ActivityDelegate.Argument10Name, argument10);
            dictionary2.Add(ActivityDelegate.Argument11Name, argument11);
            dictionary2.Add(ActivityDelegate.Argument12Name, argument12);
            dictionary2.Add(ActivityDelegate.Argument13Name, argument13);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityFunc, inputParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(14);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            dictionary2.Add(ActivityDelegate.Argument9Name, argument9);
            dictionary2.Add(ActivityDelegate.Argument10Name, argument10);
            dictionary2.Add(ActivityDelegate.Argument11Name, argument11);
            dictionary2.Add(ActivityDelegate.Argument12Name, argument12);
            dictionary2.Add(ActivityDelegate.Argument13Name, argument13);
            dictionary2.Add(ActivityDelegate.Argument14Name, argument14);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityFunc, inputParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14, T15 argument15, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(15);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            dictionary2.Add(ActivityDelegate.Argument9Name, argument9);
            dictionary2.Add(ActivityDelegate.Argument10Name, argument10);
            dictionary2.Add(ActivityDelegate.Argument11Name, argument11);
            dictionary2.Add(ActivityDelegate.Argument12Name, argument12);
            dictionary2.Add(ActivityDelegate.Argument13Name, argument13);
            dictionary2.Add(ActivityDelegate.Argument14Name, argument14);
            dictionary2.Add(ActivityDelegate.Argument15Name, argument15);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityFunc, inputParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        public System.Activities.ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14, T15 argument15, T16 argument16, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            base.ThrowIfDisposed();
            System.Activities.ActivityInstance currentInstance = base.CurrentInstance;
            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }
            if ((onCompleted != null) && !CallbackWrapper.IsValidCallback(onCompleted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onCompleted", System.Activities.SR.InvalidExecutionCallback(onCompleted, currentInstance.Activity.ToString()));
            }
            if ((onFaulted != null) && !CallbackWrapper.IsValidCallback(onFaulted, currentInstance))
            {
                throw FxTrace.Exception.Argument("onFaulted", System.Activities.SR.InvalidExecutionCallback(onFaulted, currentInstance.Activity.ToString()));
            }
            Dictionary<string, object> dictionary2 = new Dictionary<string, object>(0x10);
            dictionary2.Add(ActivityDelegate.Argument1Name, argument1);
            dictionary2.Add(ActivityDelegate.Argument2Name, argument2);
            dictionary2.Add(ActivityDelegate.Argument3Name, argument3);
            dictionary2.Add(ActivityDelegate.Argument4Name, argument4);
            dictionary2.Add(ActivityDelegate.Argument5Name, argument5);
            dictionary2.Add(ActivityDelegate.Argument6Name, argument6);
            dictionary2.Add(ActivityDelegate.Argument7Name, argument7);
            dictionary2.Add(ActivityDelegate.Argument8Name, argument8);
            dictionary2.Add(ActivityDelegate.Argument9Name, argument9);
            dictionary2.Add(ActivityDelegate.Argument10Name, argument10);
            dictionary2.Add(ActivityDelegate.Argument11Name, argument11);
            dictionary2.Add(ActivityDelegate.Argument12Name, argument12);
            dictionary2.Add(ActivityDelegate.Argument13Name, argument13);
            dictionary2.Add(ActivityDelegate.Argument14Name, argument14);
            dictionary2.Add(ActivityDelegate.Argument15Name, argument15);
            dictionary2.Add(ActivityDelegate.Argument16Name, argument16);
            Dictionary<string, object> inputParameters = dictionary2;
            return this.InternalScheduleDelegate(activityFunc, inputParameters, ActivityUtilities.CreateCompletionBookmark<TResult>(onCompleted, currentInstance), ActivityUtilities.CreateFaultBookmark(onFaulted, currentInstance));
        }

        internal System.Activities.ActivityInstance ScheduleSecondaryRoot(Activity activity, LocationEnvironment environment)
        {
            return this.executor.ScheduleSecondaryRootActivity(activity, environment);
        }

        public void SetValue(Variable variable, object value)
        {
            base.ThrowIfDisposed();
            if (variable == null)
            {
                throw FxTrace.Exception.ArgumentNull("variable");
            }
            base.SetValueCore<object>(variable, value);
        }

        public void SetValue<T>(Variable<T> variable, T value)
        {
            base.ThrowIfDisposed();
            if (variable == null)
            {
                throw FxTrace.Exception.ArgumentNull("variable");
            }
            base.SetValueCore<T>(variable, value);
        }

        internal void Terminate(Exception reason)
        {
            this.executor.ScheduleTerminate(reason);
        }

        private void ThrowIfCanInduceIdleNotSet()
        {
            Activity activity = base.Activity;
            if (!activity.InternalCanInduceIdle)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CanInduceIdleNotSpecified(activity.GetType().FullName)));
            }
        }

        public void Track(CustomTrackingRecord record)
        {
            base.ThrowIfDisposed();
            if (record == null)
            {
                throw FxTrace.Exception.ArgumentNull("record");
            }
            base.TrackCore(record);
        }

        internal void UnregisterBookmarkScope(BookmarkScope scope)
        {
            this.executor.BookmarkScopeManager.UnregisterScope(scope);
        }

        public BookmarkScope DefaultBookmarkScope
        {
            get
            {
                base.ThrowIfDisposed();
                return this.executor.BookmarkScopeManager.Default;
            }
        }

        internal bool HasRuntimeTransaction
        {
            get
            {
                return this.executor.HasRuntimeTransaction;
            }
        }

        public bool IsCancellationRequested
        {
            get
            {
                base.ThrowIfDisposed();
                return base.CurrentInstance.IsCancellationRequested;
            }
        }

        internal bool IsInNoPersistScope
        {
            get
            {
                if ((this.Properties.Find("System.Activities.NoPersistProperty") == null) && !this.executor.HasRuntimeTransaction)
                {
                    return false;
                }
                return true;
            }
        }

        public ExecutionProperties Properties
        {
            get
            {
                base.ThrowIfDisposed();
                return new ExecutionProperties(this, base.CurrentInstance, base.CurrentInstance.PropertyManager);
            }
        }

        internal bool RequiresTransactionContextWaiterExists
        {
            get
            {
                return this.executor.RequiresTransactionContextWaiterExists;
            }
        }
    }
}

