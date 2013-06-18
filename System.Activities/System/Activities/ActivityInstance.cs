namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Activities.Tracking;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [DataContract(Name="ActivityInstance", Namespace="http://schemas.datacontract.org/2010/02/System.Activities")]
    public sealed class ActivityInstance : ActivityInstanceMap.IActivityReference
    {
        private System.Activities.Activity activity;
        [DataMember(EmitDefaultValue=false)]
        private int busyCount;
        private ReadOnlyCollection<System.Activities.ActivityInstance> childCache;
        private ChildList childList;
        private System.Activities.Runtime.CompletionBookmark completionBookmark;
        private LocationEnvironment environment;
        [DataMember(EmitDefaultValue=false)]
        private ExtendedData extendedData;
        [DataMember(EmitDefaultValue=false)]
        private long id;
        [DataMember(EmitDefaultValue=false)]
        private bool initializationIncomplete;
        private ActivityInstanceMap instanceMap;
        [DataMember(EmitDefaultValue=false)]
        private bool isCancellationRequested;
        [DataMember(EmitDefaultValue=false)]
        private bool noSymbols;
        private string ownerName;
        private System.Activities.ActivityInstance parent;
        [DataMember(EmitDefaultValue=false)]
        private bool performingDefaultCancelation;
        private ExecutionPropertyManager propertyManager;
        [DataMember(EmitDefaultValue=false)]
        private ActivityInstanceState state;
        [DataMember(EmitDefaultValue=false)]
        private Substate substate;

        internal ActivityInstance(System.Activities.Activity activity)
        {
            this.activity = activity;
            this.state = ActivityInstanceState.Executing;
            this.substate = Substate.Created;
        }

        internal void Abort(ActivityExecutor executor, BookmarkManager bookmarkManager, Exception terminationReason, bool isTerminate)
        {
            AbortEnumerator enumerator = new AbortEnumerator(this);
            while (enumerator.MoveNext())
            {
                System.Activities.ActivityInstance current = enumerator.Current;
                if (!current.HasNotExecuted)
                {
                    current.Activity.InternalAbort(current, executor, terminationReason);
                    executor.DebugActivityCompleted(this);
                }
                if (current.PropertyManager != null)
                {
                    current.PropertyManager.UnregisterProperties(current, current.Activity.MemberOf, true);
                }
                executor.TerminateSpecialExecutionBlocks(current, terminationReason);
                executor.CancelPendingOperation(current);
                executor.HandleRootCompletion(current);
                current.MarkAsComplete(executor.RawBookmarkScopeManager, bookmarkManager);
                current.state = ActivityInstanceState.Faulted;
                current.FinalizeState(executor, false, !isTerminate);
            }
        }

        internal void AddActivityReference(ActivityInstanceReference reference)
        {
            this.EnsureExtendedData();
            this.extendedData.AddActivityReference(reference);
        }

        internal void AddBookmark(Bookmark bookmark, BookmarkOptions options)
        {
            bool affectsBusyCount = false;
            if (!BookmarkOptionsHelper.IsNonBlocking(options))
            {
                this.IncrementBusyCount();
                affectsBusyCount = true;
            }
            this.EnsureExtendedData();
            this.extendedData.AddBookmark(bookmark, affectsBusyCount);
        }

        internal void AddChild(System.Activities.ActivityInstance item)
        {
            if (this.childList == null)
            {
                this.childList = new ChildList();
            }
            this.childList.Add(item);
            this.childCache = null;
        }

        internal void AppendChildren(ActivityUtilities.TreeProcessingList nextInstanceList, ref Queue<IList<System.Activities.ActivityInstance>> instancesRemaining)
        {
            this.childList.AppendChildren(nextInstanceList, ref instancesRemaining);
        }

        internal void BaseCancel(NativeActivityContext context)
        {
            this.performingDefaultCancelation = true;
            this.CancelChildren(context);
        }

        internal void Cancel(ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            this.Activity.InternalCancel(this, executor, bookmarkManager);
        }

        internal void CancelChildren(NativeActivityContext context)
        {
            if (this.HasChildren)
            {
                foreach (System.Activities.ActivityInstance instance in this.GetChildren())
                {
                    context.CancelChild(instance);
                }
            }
        }

        internal static System.Activities.ActivityInstance CreateCanceledInstance(System.Activities.Activity activity)
        {
            return new System.Activities.ActivityInstance(activity) { state = ActivityInstanceState.Canceled };
        }

        internal static System.Activities.ActivityInstance CreateCompletedInstance(System.Activities.Activity activity)
        {
            return new System.Activities.ActivityInstance(activity) { state = ActivityInstanceState.Closed };
        }

        internal void DecrementBusyCount()
        {
            this.busyCount--;
        }

        internal void DecrementBusyCount(int amount)
        {
            this.busyCount -= amount;
        }

        private void EnsureExtendedData()
        {
            if (this.extendedData == null)
            {
                this.extendedData = new ExtendedData();
            }
        }

        internal void Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            if (this.initializationIncomplete)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InitializationIncomplete));
            }
            this.MarkExecuted();
            this.Activity.InternalExecute(this, executor, bookmarkManager);
        }

        internal void FillInstanceMap(ActivityInstanceMap instanceMap)
        {
            if (!this.IsCompleted)
            {
                this.instanceMap = instanceMap;
                ActivityUtilities.ProcessActivityInstanceTree(this, null, new Func<System.Activities.ActivityInstance, ActivityExecutor, bool>(this.GenerateInstanceMapCallback));
            }
        }

        internal void FinalizeState(ActivityExecutor executor, bool faultActivity)
        {
            this.FinalizeState(executor, faultActivity, false);
        }

        internal void FinalizeState(ActivityExecutor executor, bool faultActivity, bool skipTracking)
        {
            if (faultActivity)
            {
                this.TryCancelParent();
                this.state = ActivityInstanceState.Faulted;
            }
            if (this.state == ActivityInstanceState.Closed)
            {
                if ((executor.ShouldTrackActivityStateRecordsClosedState && !skipTracking) && executor.ShouldTrackActivity(this.Activity.DisplayName))
                {
                    executor.AddTrackingRecord(new ActivityStateRecord(executor.WorkflowInstanceId, this, this.state));
                }
            }
            else if (executor.ShouldTrackActivityStateRecords && !skipTracking)
            {
                executor.AddTrackingRecord(new ActivityStateRecord(executor.WorkflowInstanceId, this, this.state));
            }
            if (TD.ActivityCompletedIsEnabled())
            {
                TD.ActivityCompleted(this.Activity.GetType().ToString(), this.Activity.DisplayName, this.Id, this.State.ToString());
            }
        }

        internal void FixupInstance(System.Activities.ActivityInstance parent, ActivityInstanceMap instanceMap, ActivityExecutor executor)
        {
            if (!this.IsCompleted)
            {
                if (this.Activity == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ActivityInstanceFixupFailed));
                }
                this.parent = parent;
                this.instanceMap = instanceMap;
                if (this.PropertyManager != null)
                {
                    this.PropertyManager.OnDeserialized(this, parent, this.Activity.MemberOf, executor);
                }
                else if (this.parent != null)
                {
                    this.PropertyManager = this.parent.PropertyManager;
                }
                else
                {
                    this.PropertyManager = executor.RootPropertyManager;
                }
                if (!this.noSymbols)
                {
                    this.environment.OnDeserialized(executor, this);
                }
            }
        }

        private bool GenerateInstanceMapCallback(System.Activities.ActivityInstance instance, ActivityExecutor executor)
        {
            this.instanceMap.AddEntry(instance);
            instance.instanceMap = this.instanceMap;
            if (instance.HasActivityReferences)
            {
                instance.extendedData.FillInstanceMap(instance.instanceMap);
            }
            return true;
        }

        internal ReadOnlyCollection<System.Activities.ActivityInstance> GetChildren()
        {
            if (!this.HasChildren)
            {
                return ChildList.Empty;
            }
            if (this.childCache == null)
            {
                this.childCache = this.childList.AsReadOnly();
            }
            return this.childCache;
        }

        internal HybridCollection<System.Activities.ActivityInstance> GetRawChildren()
        {
            return this.childList;
        }

        internal void IncrementBusyCount()
        {
            this.busyCount++;
        }

        internal bool Initialize(System.Activities.ActivityInstance parent, ActivityInstanceMap instanceMap, LocationEnvironment parentEnvironment, long instanceId, ActivityExecutor executor)
        {
            return this.Initialize(parent, instanceMap, parentEnvironment, instanceId, executor, 0);
        }

        internal bool Initialize(System.Activities.ActivityInstance parent, ActivityInstanceMap instanceMap, LocationEnvironment parentEnvironment, long instanceId, ActivityExecutor executor, int delegateParameterCount)
        {
            this.parent = parent;
            this.instanceMap = instanceMap;
            this.id = instanceId;
            if (this.instanceMap != null)
            {
                this.instanceMap.AddEntry(this);
            }
            if (this.parent != null)
            {
                if (this.parent.PropertyManager != null)
                {
                    this.PropertyManager = this.parent.PropertyManager;
                }
                if (parentEnvironment == null)
                {
                    parentEnvironment = this.parent.Environment;
                }
            }
            int capacity = this.Activity.SymbolCount + delegateParameterCount;
            if (capacity == 0)
            {
                if (parentEnvironment == null)
                {
                    this.environment = new LocationEnvironment(executor, this.Activity);
                }
                else
                {
                    this.noSymbols = true;
                    this.environment = parentEnvironment;
                }
                return false;
            }
            this.environment = new LocationEnvironment(executor, this.Activity, parentEnvironment, capacity);
            this.substate = Substate.ResolvingArguments;
            return true;
        }

        internal void MarkAsComplete(BookmarkScopeManager bookmarkScopeManager, BookmarkManager bookmarkManager)
        {
            if (this.extendedData != null)
            {
                this.extendedData.PurgeBookmarks(bookmarkScopeManager, bookmarkManager, this);
                if (this.extendedData.DataContext != null)
                {
                    this.extendedData.DataContext.Dispose();
                }
            }
            if (this.instanceMap != null)
            {
                this.instanceMap.RemoveEntry(this);
                if (this.HasActivityReferences)
                {
                    this.extendedData.PurgeActivityReferences(this.instanceMap);
                }
            }
            if (this.Parent != null)
            {
                this.Parent.RemoveChild(this);
            }
        }

        internal void MarkCanceled()
        {
            this.substate = Substate.Canceling;
        }

        private void MarkExecuted()
        {
            this.substate = Substate.Executing;
        }

        internal void RemoveAllBookmarks(BookmarkScopeManager bookmarkScopeManager, BookmarkManager bookmarkManager)
        {
            if (this.extendedData != null)
            {
                this.extendedData.PurgeBookmarks(bookmarkScopeManager, bookmarkManager, this);
            }
        }

        internal void RemoveBookmark(Bookmark bookmark, BookmarkOptions options)
        {
            bool affectsBusyCount = false;
            if (!BookmarkOptionsHelper.IsNonBlocking(options))
            {
                this.DecrementBusyCount();
                affectsBusyCount = true;
            }
            this.extendedData.RemoveBookmark(bookmark, affectsBusyCount);
        }

        internal void RemoveChild(System.Activities.ActivityInstance item)
        {
            this.childList.Remove(item, true);
            this.childCache = null;
        }

        internal bool ResolveArguments(ActivityExecutor executor, IDictionary<string, object> argumentValueOverrides, Location resultLocation, int startIndex = 0)
        {
            bool flag = true;
            if (this.Activity.SkipArgumentResolution)
            {
                using (ActivityContext context = executor.GetResolutionContext(this))
                {
                    RuntimeArgument resultRuntimeArgument = ((ActivityWithResult) this.Activity).ResultRuntimeArgument;
                    if (!resultRuntimeArgument.TryPopulateValue(this.environment, this, context, null, resultLocation, false))
                    {
                        flag = false;
                        Location specificLocation = this.environment.GetSpecificLocation(resultRuntimeArgument.Id);
                        executor.ScheduleExpression(resultRuntimeArgument.BoundArgument.Expression, this, context.Environment, specificLocation.CreateReference(true));
                    }
                    goto Label_0166;
                }
            }
            IList<RuntimeArgument> runtimeArguments = this.Activity.RuntimeArguments;
            int count = runtimeArguments.Count;
            if (count > 0)
            {
                using (ActivityContext context2 = executor.GetResolutionContext(this))
                {
                    for (int i = startIndex; i < count; i++)
                    {
                        RuntimeArgument argument2 = runtimeArguments[i];
                        object obj2 = null;
                        if (argumentValueOverrides != null)
                        {
                            argumentValueOverrides.TryGetValue(argument2.Name, out obj2);
                        }
                        if (!argument2.TryPopulateValue(this.environment, this, context2, obj2, resultLocation, false))
                        {
                            flag = false;
                            int nextArgumentIndex = i + 1;
                            if (nextArgumentIndex < runtimeArguments.Count)
                            {
                                ResolveNextArgumentWorkItem workItem = executor.ResolveNextArgumentWorkItemPool.Acquire();
                                workItem.Initialize(this, nextArgumentIndex, argumentValueOverrides, resultLocation);
                                executor.ScheduleItem(workItem);
                            }
                            Location location2 = this.environment.GetSpecificLocation(argument2.Id);
                            executor.ScheduleExpression(argument2.BoundArgument.Expression, this, context2.Environment, location2.CreateReference(true));
                            goto Label_0166;
                        }
                    }
                }
            }
        Label_0166:
            if (flag && (startIndex == 0))
            {
                this.substate = Substate.ResolvingVariables;
            }
            return flag;
        }

        internal bool ResolveVariables(ActivityExecutor executor)
        {
            this.substate = Substate.ResolvingVariables;
            bool flag = true;
            IList<Variable> implementationVariables = this.Activity.ImplementationVariables;
            IList<Variable> runtimeVariables = this.Activity.RuntimeVariables;
            int count = implementationVariables.Count;
            int num2 = runtimeVariables.Count;
            if ((count > 0) || (num2 > 0))
            {
                using (ActivityContext context = executor.GetResolutionContext(this))
                {
                    for (int i = 0; i < count; i++)
                    {
                        Variable variable = implementationVariables[i];
                        context.Activity = variable.Default;
                        if (!variable.TryPopulateLocation(executor, context))
                        {
                            Location specificLocation = this.environment.GetSpecificLocation(variable.Id);
                            executor.ScheduleExpression(variable.Default, this, this.environment, specificLocation.CreateReference(true));
                            flag = false;
                        }
                    }
                    for (int j = 0; j < num2; j++)
                    {
                        Variable variable2 = runtimeVariables[j];
                        context.Activity = variable2.Default;
                        if (!variable2.TryPopulateLocation(executor, context))
                        {
                            Location location2 = this.environment.GetSpecificLocation(variable2.Id);
                            executor.ScheduleExpression(variable2.Default, this, this.environment, location2.CreateReference(true));
                            flag = false;
                        }
                    }
                }
            }
            return flag;
        }

        private void SetCanceled()
        {
            this.TryCancelParent();
            this.state = ActivityInstanceState.Canceled;
        }

        private void SetClosed()
        {
            this.state = ActivityInstanceState.Closed;
        }

        internal void SetInitializationIncomplete()
        {
            this.initializationIncomplete = true;
        }

        internal void SetInitializedSubstate(ActivityExecutor executor)
        {
            this.substate = Substate.Initialized;
            if (executor.ShouldTrackActivityStateRecordsExecutingState && executor.ShouldTrackActivity(this.Activity.DisplayName))
            {
                executor.AddTrackingRecord(new ActivityStateRecord(executor.WorkflowInstanceId, this, this.state));
            }
            if ((this.Activity.RuntimeArguments.Count > 0) && TD.InArgumentBoundIsEnabled())
            {
                for (int i = 0; i < this.Activity.RuntimeArguments.Count; i++)
                {
                    Location location;
                    RuntimeArgument argument = this.Activity.RuntimeArguments[i];
                    if (ArgumentDirectionHelper.IsIn(argument.Direction) && this.environment.TryGetLocation(argument.Id, this.Activity, out location))
                    {
                        string str = null;
                        if (location.Value == null)
                        {
                            str = "<Null>";
                        }
                        else
                        {
                            str = "'" + location.Value.ToString() + "'";
                        }
                        TD.InArgumentBound(argument.Name, this.Activity.GetType().ToString(), this.Activity.DisplayName, this.Id, str);
                    }
                }
            }
        }

        void ActivityInstanceMap.IActivityReference.Load(System.Activities.Activity activity, ActivityInstanceMap instanceMap)
        {
            if (activity.GetType().Name != this.OwnerName)
            {
                throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.ActivityTypeMismatch(activity.DisplayName, this.OwnerName)));
            }
            this.Activity = activity;
        }

        private void TryCancelParent()
        {
            if ((this.parent != null) && this.parent.IsPerformingDefaultCancelation)
            {
                this.parent.MarkCanceled();
            }
        }

        internal bool TryFixupChildren(ActivityInstanceMap instanceMap, ActivityExecutor executor)
        {
            if (!this.HasChildren)
            {
                return false;
            }
            this.childList.FixupList(this, instanceMap, executor);
            return true;
        }

        internal bool UpdateState(ActivityExecutor executor)
        {
            bool flag = false;
            if (this.HasNotExecuted)
            {
                if (this.IsCancellationRequested)
                {
                    if (this.HasChildren)
                    {
                        foreach (System.Activities.ActivityInstance instance in this.GetChildren())
                        {
                            executor.CancelActivity(instance);
                        }
                        return flag;
                    }
                    this.SetCanceled();
                    return true;
                }
                if (!this.HasPendingWork)
                {
                    bool flag2 = false;
                    if (this.substate == Substate.ResolvingArguments)
                    {
                        this.Environment.CollapseTemporaryResolutionLocations();
                        this.substate = Substate.ResolvingVariables;
                        flag2 = this.ResolveVariables(executor);
                    }
                    else if (this.substate == Substate.ResolvingVariables)
                    {
                        flag2 = true;
                    }
                    if (flag2)
                    {
                        executor.ScheduleBody(this, false, null, null);
                    }
                }
                return flag;
            }
            if (!this.HasPendingWork)
            {
                if (!executor.IsCompletingTransaction(this))
                {
                    flag = true;
                    if (this.substate == Substate.Canceling)
                    {
                        this.SetCanceled();
                        return flag;
                    }
                    this.SetClosed();
                }
                return flag;
            }
            if (this.performingDefaultCancelation && this.OnlyHasOutstandingBookmarks)
            {
                this.RemoveAllBookmarks(executor.RawBookmarkScopeManager, executor.RawBookmarkManager);
                this.MarkCanceled();
                this.SetCanceled();
                flag = true;
            }
            return flag;
        }

        public System.Activities.Activity Activity
        {
            get
            {
                return this.activity;
            }
            internal set
            {
                this.activity = value;
            }
        }

        [DataMember(EmitDefaultValue=false)]
        internal System.Activities.Runtime.CompletionBookmark CompletionBookmark
        {
            get
            {
                return this.completionBookmark;
            }
            set
            {
                this.completionBookmark = value;
            }
        }

        internal WorkflowDataContext DataContext
        {
            get
            {
                if (this.extendedData != null)
                {
                    return this.extendedData.DataContext;
                }
                return null;
            }
            set
            {
                this.EnsureExtendedData();
                this.extendedData.DataContext = value;
            }
        }

        internal LocationEnvironment Environment
        {
            get
            {
                return this.environment;
            }
        }

        internal System.Activities.Runtime.FaultBookmark FaultBookmark
        {
            get
            {
                if (this.extendedData == null)
                {
                    return null;
                }
                return this.extendedData.FaultBookmark;
            }
            set
            {
                if (value != null)
                {
                    this.EnsureExtendedData();
                    this.extendedData.FaultBookmark = value;
                }
            }
        }

        internal bool HasActivityReferences
        {
            get
            {
                return ((this.extendedData != null) && this.extendedData.HasActivityReferences);
            }
        }

        internal bool HasChildren
        {
            get
            {
                return ((this.childList != null) && (this.childList.Count > 0));
            }
        }

        internal bool HasNotExecuted
        {
            get
            {
                return (((byte) (this.substate & Substate.PreExecuting)) != 0);
            }
        }

        internal bool HasPendingWork
        {
            get
            {
                return (this.HasChildren || (this.busyCount > 0));
            }
        }

        public string Id
        {
            get
            {
                return this.id.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal ActivityInstanceMap InstanceMap
        {
            get
            {
                return this.instanceMap;
            }
        }

        internal bool IsCancellationRequested
        {
            get
            {
                return this.isCancellationRequested;
            }
            set
            {
                this.isCancellationRequested = value;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return ActivityUtilities.IsCompletedState(this.State);
            }
        }

        internal bool IsEnvironmentOwner
        {
            get
            {
                return !this.noSymbols;
            }
        }

        internal bool IsPerformingDefaultCancelation
        {
            get
            {
                return this.performingDefaultCancelation;
            }
        }

        internal bool OnlyHasOutstandingBookmarks
        {
            get
            {
                return ((!this.HasChildren && (this.extendedData != null)) && (this.extendedData.BlockingBookmarkCount == this.busyCount));
            }
        }

        [DataMember(Name="owner", EmitDefaultValue=false)]
        private string OwnerName
        {
            get
            {
                if (this.ownerName == null)
                {
                    this.ownerName = this.Activity.GetType().Name;
                }
                return this.ownerName;
            }
            set
            {
                this.ownerName = value;
            }
        }

        internal System.Activities.ActivityInstance Parent
        {
            get
            {
                return this.parent;
            }
        }

        internal ExecutionPropertyManager PropertyManager
        {
            get
            {
                return this.propertyManager;
            }
            set
            {
                this.propertyManager = value;
            }
        }

        [DataMember(Name="children", EmitDefaultValue=false)]
        private ChildList SerializedChildren
        {
            get
            {
                if (this.HasChildren)
                {
                    this.childList.Compress();
                    return this.childList;
                }
                return null;
            }
            set
            {
                this.childList = value;
            }
        }

        [DataMember(EmitDefaultValue=false)]
        private LocationEnvironment SerializedEnvironment
        {
            get
            {
                if (this.IsCompleted)
                {
                    return null;
                }
                return this.environment;
            }
            set
            {
                this.environment = value;
            }
        }

        [DataMember(Name="propertyManager", EmitDefaultValue=false)]
        private ExecutionPropertyManager SerializedPropertyManager
        {
            get
            {
                if ((this.propertyManager != null) && this.propertyManager.ShouldSerialize(this))
                {
                    return this.propertyManager;
                }
                return null;
            }
            set
            {
                this.propertyManager = value;
            }
        }

        public ActivityInstanceState State
        {
            get
            {
                return this.state;
            }
        }

        System.Activities.Activity ActivityInstanceMap.IActivityReference.Activity
        {
            get
            {
                return this.Activity;
            }
        }

        internal bool WaitingForTransactionContext
        {
            get
            {
                if (this.extendedData == null)
                {
                    return false;
                }
                return this.extendedData.WaitingForTransactionContext;
            }
            set
            {
                this.EnsureExtendedData();
                this.extendedData.WaitingForTransactionContext = value;
            }
        }

        private class AbortEnumerator : IEnumerator<System.Activities.ActivityInstance>, IDisposable, IEnumerator
        {
            private System.Activities.ActivityInstance current;
            private bool initialized;
            private System.Activities.ActivityInstance root;

            public AbortEnumerator(System.Activities.ActivityInstance root)
            {
                this.root = root;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (!this.initialized)
                {
                    this.current = this.root;
                    while (this.current.HasChildren)
                    {
                        this.current = this.current.GetChildren()[0];
                    }
                    this.initialized = true;
                    return true;
                }
                if (this.current == this.root)
                {
                    return false;
                }
                this.current = this.current.Parent;
                while (this.current.HasChildren)
                {
                    this.current = this.current.GetChildren()[0];
                }
                return true;
            }

            public void Reset()
            {
                this.current = null;
                this.initialized = false;
            }

            public System.Activities.ActivityInstance Current
            {
                get
                {
                    return this.current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }

        [DataContract]
        private class ChildList : HybridCollection<System.Activities.ActivityInstance>
        {
            private static ReadOnlyCollection<System.Activities.ActivityInstance> emptyChildren;

            public void AppendChildren(ActivityUtilities.TreeProcessingList nextInstanceList, ref Queue<IList<System.Activities.ActivityInstance>> instancesRemaining)
            {
                if (base.SingleItem != null)
                {
                    nextInstanceList.Add(base.SingleItem);
                }
                else if (nextInstanceList.Count == 0)
                {
                    nextInstanceList.Set(base.MultipleItems);
                }
                else
                {
                    if (instancesRemaining == null)
                    {
                        instancesRemaining = new Queue<IList<System.Activities.ActivityInstance>>();
                    }
                    instancesRemaining.Enqueue(base.MultipleItems);
                }
            }

            public void FixupList(System.Activities.ActivityInstance parent, ActivityInstanceMap instanceMap, ActivityExecutor executor)
            {
                if (base.SingleItem != null)
                {
                    base.SingleItem.FixupInstance(parent, instanceMap, executor);
                }
                else
                {
                    for (int i = 0; i < base.MultipleItems.Count; i++)
                    {
                        base.MultipleItems[i].FixupInstance(parent, instanceMap, executor);
                    }
                }
            }

            public static ReadOnlyCollection<System.Activities.ActivityInstance> Empty
            {
                get
                {
                    if (emptyChildren == null)
                    {
                        emptyChildren = new ReadOnlyCollection<System.Activities.ActivityInstance>(new System.Activities.ActivityInstance[0]);
                    }
                    return emptyChildren;
                }
            }
        }

        [DataContract]
        private class ExtendedData
        {
            private ActivityReferenceList activityReferences;
            private BookmarkList bookmarks;

            public void AddActivityReference(ActivityInstanceReference reference)
            {
                if (this.activityReferences == null)
                {
                    this.activityReferences = new ActivityReferenceList();
                }
                this.activityReferences.Add(reference);
            }

            public void AddBookmark(Bookmark bookmark, bool affectsBusyCount)
            {
                if (this.bookmarks == null)
                {
                    this.bookmarks = new BookmarkList();
                }
                if (affectsBusyCount)
                {
                    this.BlockingBookmarkCount++;
                }
                this.bookmarks.Add(bookmark);
            }

            public void FillInstanceMap(ActivityInstanceMap instanceMap)
            {
                this.activityReferences.FillInstanceMap(instanceMap);
            }

            public void PurgeActivityReferences(ActivityInstanceMap instanceMap)
            {
                this.activityReferences.PurgeActivityReferences(instanceMap);
            }

            public void PurgeBookmarks(BookmarkScopeManager bookmarkScopeManager, BookmarkManager bookmarkManager, System.Activities.ActivityInstance owningInstance)
            {
                if ((this.bookmarks != null) && (this.bookmarks.Count > 0))
                {
                    Bookmark bookmark;
                    IList<Bookmark> list;
                    this.bookmarks.TransferBookmarks(out bookmark, out list);
                    this.bookmarks = null;
                    if (bookmarkScopeManager != null)
                    {
                        bookmarkScopeManager.PurgeBookmarks(bookmarkManager, bookmark, list);
                    }
                    else
                    {
                        bookmarkManager.PurgeBookmarks(bookmark, list);
                    }
                    owningInstance.DecrementBusyCount(this.BlockingBookmarkCount);
                    this.BlockingBookmarkCount = 0;
                }
            }

            public void RemoveBookmark(Bookmark bookmark, bool affectsBusyCount)
            {
                if (affectsBusyCount)
                {
                    this.BlockingBookmarkCount--;
                }
                this.bookmarks.Remove(bookmark);
            }

            [DataMember(Name="activityReferences", EmitDefaultValue=false)]
            private ActivityReferenceList ActivityReferences
            {
                get
                {
                    if ((this.activityReferences != null) && (this.activityReferences.Count != 0))
                    {
                        return this.activityReferences;
                    }
                    return null;
                }
                set
                {
                    this.activityReferences = value;
                }
            }

            [DataMember(Name="blockingBookmarkCount", EmitDefaultValue=false)]
            public int BlockingBookmarkCount { get; private set; }

            [DataMember(Name="bookmarks", EmitDefaultValue=false)]
            private BookmarkList Bookmarks
            {
                get
                {
                    if ((this.bookmarks != null) && (this.bookmarks.Count != 0))
                    {
                        return this.bookmarks;
                    }
                    return null;
                }
                set
                {
                    this.bookmarks = value;
                }
            }

            public WorkflowDataContext DataContext { get; set; }

            [DataMember(Name="faultBookmark", EmitDefaultValue=false)]
            public System.Activities.Runtime.FaultBookmark FaultBookmark { get; set; }

            public bool HasActivityReferences
            {
                get
                {
                    return ((this.activityReferences != null) && (this.activityReferences.Count > 0));
                }
            }

            [DataMember(Name="waitingForTransactionContext", EmitDefaultValue=false)]
            public bool WaitingForTransactionContext { get; set; }

            [DataContract]
            private class ActivityReferenceList : HybridCollection<ActivityInstanceReference>
            {
                public void FillInstanceMap(ActivityInstanceMap instanceMap)
                {
                    if (base.SingleItem != null)
                    {
                        instanceMap.AddEntry(base.SingleItem);
                    }
                    else
                    {
                        for (int i = 0; i < base.MultipleItems.Count; i++)
                        {
                            ActivityInstanceReference reference = base.MultipleItems[i];
                            instanceMap.AddEntry(reference);
                        }
                    }
                }

                public void PurgeActivityReferences(ActivityInstanceMap instanceMap)
                {
                    if (base.SingleItem != null)
                    {
                        instanceMap.RemoveEntry(base.SingleItem);
                    }
                    else
                    {
                        for (int i = 0; i < base.MultipleItems.Count; i++)
                        {
                            instanceMap.RemoveEntry(base.MultipleItems[i]);
                        }
                    }
                }
            }
        }

        private enum Substate : byte
        {
            Canceling = 5,
            Created = 0x81,
            Executing = 0,
            Initialized = 0x84,
            PreExecuting = 0x80,
            ResolvingArguments = 130,
            ResolvingVariables = 0x83
        }
    }
}

