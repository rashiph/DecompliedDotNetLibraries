namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.Runtime.DebugEngine;

    [DefaultEvent("Initialized"), WorkflowDebuggerStepping(WorkflowDebuggerSteppingOption.Concurrent), ActivityValidator(typeof(ReplicatorValidator)), ToolboxBitmap(typeof(ReplicatorActivity), "Resources.Replicator.png"), ToolboxItem(typeof(ActivityToolboxItem)), SRDescription("ReplicatorActivityDescription"), Designer(typeof(ReplicatorDesigner), typeof(IDesigner))]
    public sealed class ReplicatorActivity : CompositeActivity
    {
        private static DependencyProperty ActivityStateProperty = DependencyProperty.Register("ActivityState", typeof(ReplicatorStateInfo), typeof(ReplicatorActivity));
        public static readonly DependencyProperty ChildCompletedEvent = DependencyProperty.Register("ChildCompleted", typeof(EventHandler<ReplicatorChildEventArgs>), typeof(ReplicatorActivity));
        private ReplicatorChildInstanceList childDataList;
        public static readonly DependencyProperty ChildInitializedEvent = DependencyProperty.Register("ChildInitialized", typeof(EventHandler<ReplicatorChildEventArgs>), typeof(ReplicatorActivity));
        public static readonly DependencyProperty CompletedEvent = DependencyProperty.Register("Completed", typeof(EventHandler), typeof(ReplicatorActivity));
        public static readonly DependencyProperty ExecutionTypeProperty = DependencyProperty.Register("ExecutionType", typeof(System.Workflow.Activities.ExecutionType), typeof(ReplicatorActivity), new PropertyMetadata(System.Workflow.Activities.ExecutionType.Sequence));
        public static readonly DependencyProperty InitialChildDataProperty = DependencyProperty.Register("InitialChildData", typeof(IList), typeof(ReplicatorActivity));
        public static readonly DependencyProperty InitializedEvent = DependencyProperty.Register("Initialized", typeof(EventHandler), typeof(ReplicatorActivity));
        public static readonly DependencyProperty UntilConditionProperty = DependencyProperty.Register("UntilCondition", typeof(ActivityCondition), typeof(ReplicatorActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata));

        [SRCategory("Handlers"), SRDescription("OnGeneratorChildCompletedDescr"), MergableProperty(false)]
        public event EventHandler<ReplicatorChildEventArgs> ChildCompleted
        {
            add
            {
                base.AddHandler(ChildCompletedEvent, value);
            }
            remove
            {
                base.RemoveHandler(ChildCompletedEvent, value);
            }
        }

        [MergableProperty(false), SRDescription("OnGeneratorChildInitializedDescr"), SRCategory("Handlers")]
        public event EventHandler<ReplicatorChildEventArgs> ChildInitialized
        {
            add
            {
                base.AddHandler(ChildInitializedEvent, value);
            }
            remove
            {
                base.RemoveHandler(ChildInitializedEvent, value);
            }
        }

        [SRDescription("OnCompletedDescr"), MergableProperty(false), SRCategory("Handlers")]
        public event EventHandler Completed
        {
            add
            {
                base.AddHandler(CompletedEvent, value);
            }
            remove
            {
                base.RemoveHandler(CompletedEvent, value);
            }
        }

        [SRCategory("Handlers"), MergableProperty(false), SRDescription("OnInitializedDescr")]
        public event EventHandler Initialized
        {
            add
            {
                base.AddHandler(InitializedEvent, value);
            }
            remove
            {
                base.RemoveHandler(InitializedEvent, value);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ReplicatorActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ReplicatorActivity(string name) : base(name)
        {
        }

        private int Add(object value)
        {
            if (base.ExecutionStatus != ActivityExecutionStatus.Executing)
            {
                throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotExecuting"));
            }
            if (this.ActivityState == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotInitialized"));
            }
            ChildExecutionStateInfo item = new ChildExecutionStateInfo(value);
            this.ActivityState.Add(item);
            int index = this.ActivityState.AbsoluteCount - 1;
            this.ScheduleExecutionIfNeeded(item, index);
            return index;
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            this.TryCancelChildren(executionContext);
            if (this.ActivityState.IsChildActive)
            {
                return base.ExecutionStatus;
            }
            if (base.ExecutionStatus == ActivityExecutionStatus.Faulting)
            {
                if (this.ActivityState.AttemptedCloseWhileFaulting)
                {
                    return ActivityExecutionStatus.Closed;
                }
                this.ActivityState.AttemptedCloseWhileFaulting = true;
            }
            base.RaiseEvent(CompletedEvent, this, EventArgs.Empty);
            return ActivityExecutionStatus.Closed;
        }

        private void CancelChildExecution(ActivityExecutionContext executionContext, ChildExecutionStateInfo childStateInfo)
        {
            if (childStateInfo.Status != ChildRunStatus.Running)
            {
                this.ActivityState.Remove(childStateInfo);
            }
            else
            {
                this.TryCancelChild(executionContext, childStateInfo);
            }
        }

        private void Clear()
        {
            if (base.ExecutionStatus != ActivityExecutionStatus.Executing)
            {
                throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotExecuting"));
            }
            if (this.ActivityState == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotInitialized"));
            }
            while (this.ActivityState.AbsoluteCount != 0)
            {
                this.RemoveAt(0);
            }
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            this.ActivityState = new ReplicatorStateInfo();
            base.RaiseEvent(InitializedEvent, this, EventArgs.Empty);
            if (this.InitialChildData != null)
            {
                for (int i = 0; i < this.InitialChildData.Count; i++)
                {
                    this.Add(this.InitialChildData[i]);
                }
            }
            bool flag = this.UntilCondition == null;
            if ((this.UntilCondition != null) && this.UntilCondition.Evaluate(this, executionContext))
            {
                flag = true;
            }
            else if (this.ActivityState.Count != 0)
            {
                flag = false;
            }
            if (flag)
            {
                this.ActivityState.CompletionConditionTrueAlready = true;
                if (!this.TryCancelChildren(executionContext))
                {
                    base.RaiseEvent(CompletedEvent, this, EventArgs.Empty);
                    return ActivityExecutionStatus.Closed;
                }
            }
            return ActivityExecutionStatus.Executing;
        }

        private void ExecuteTemplate(ActivityExecutionContext executionContext, ChildExecutionStateInfo childStateInfo)
        {
            ActivityExecutionContextManager executionContextManager = executionContext.ExecutionContextManager;
            ActivityExecutionContext childContext = executionContextManager.CreateExecutionContext(base.EnabledActivities[0]);
            childStateInfo.RunId = childContext.ContextGuid;
            childStateInfo.Status = ChildRunStatus.Running;
            try
            {
                base.RaiseGenericEvent<ReplicatorChildEventArgs>(ChildInitializedEvent, this, new ReplicatorChildEventArgs(childStateInfo.InstanceData, childContext.Activity));
            }
            catch
            {
                childStateInfo.RunId = Guid.Empty;
                childStateInfo.Status = ChildRunStatus.Completed;
                executionContextManager.CompleteExecutionContext(childContext);
                throw;
            }
            childContext.ExecuteActivity(childContext.Activity);
            childContext.Activity.RegisterForStatusChange(Activity.ClosedEvent, new ReplicatorSubscriber(this, childContext.ContextGuid));
        }

        private ActivityExecutionContext GetExecutionContext(ActivityExecutionContextManager contextManager, Guid contextIdGuid)
        {
            foreach (ActivityExecutionContext context in contextManager.ExecutionContexts)
            {
                if (context.ContextGuid == contextIdGuid)
                {
                    return context;
                }
            }
            return null;
        }

        private void HandleChildUpdateOperation(object sender, ReplicatorInterActivityEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            ActivityExecutionContext executionContext = sender as ActivityExecutionContext;
            if (executionContext == null)
            {
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");
            }
            if (base.ExecutionStatus == ActivityExecutionStatus.Executing)
            {
                if (!e.IsAdd)
                {
                    this.CancelChildExecution(executionContext, e.ChildStateInfo);
                }
                else
                {
                    this.ExecuteTemplate(executionContext, e.ChildStateInfo);
                }
            }
        }

        private void HandleStatusChange(ActivityExecutionContext executionContext, ActivityExecutionStatusChangedEventArgs e, ReplicatorSubscriber subscriber)
        {
            int num = this.ActivityState.FindIndexOfChildStateInfo(subscriber.RunIdentifier);
            if (num != -1)
            {
                ChildExecutionStateInfo item = this.ActivityState[num];
                bool markedForRemoval = item.MarkedForRemoval;
                try
                {
                    try
                    {
                        base.RaiseGenericEvent<ReplicatorChildEventArgs>(ChildCompletedEvent, this, new ReplicatorChildEventArgs(item.InstanceData, e.Activity));
                        e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, subscriber);
                    }
                    finally
                    {
                        ActivityExecutionContextManager executionContextManager = executionContext.ExecutionContextManager;
                        ActivityExecutionContext childContext = executionContextManager.GetExecutionContext(e.Activity);
                        executionContextManager.CompleteExecutionContext(childContext);
                    }
                    if (!this.ActivityState.CompletionConditionTrueAlready)
                    {
                        this.ActivityState.CompletionConditionTrueAlready = (this.UntilCondition != null) && this.UntilCondition.Evaluate(this, executionContext);
                    }
                }
                finally
                {
                    item.RunId = Guid.Empty;
                    item.Status = ChildRunStatus.Completed;
                    if (markedForRemoval)
                    {
                        this.ActivityState.Remove(item);
                        num--;
                    }
                }
                if (!this.ActivityState.IsChildActive)
                {
                    if (((base.ExecutionStatus == ActivityExecutionStatus.Canceling) || (base.ExecutionStatus == ActivityExecutionStatus.Faulting)) || this.ActivityState.CompletionConditionTrueAlready)
                    {
                        base.RaiseEvent(CompletedEvent, this, EventArgs.Empty);
                        executionContext.CloseActivity();
                        return;
                    }
                }
                else if (((base.ExecutionStatus != ActivityExecutionStatus.Canceling) && (base.ExecutionStatus != ActivityExecutionStatus.Faulting)) && this.ActivityState.CompletionConditionTrueAlready)
                {
                    this.TryCancelChildren(executionContext);
                    return;
                }
                switch (this.ExecutionType)
                {
                    case System.Workflow.Activities.ExecutionType.Sequence:
                        if (num >= (this.ActivityState.Count - 1))
                        {
                            if ((this.UntilCondition == null) || this.UntilCondition.Evaluate(this, executionContext))
                            {
                                base.RaiseEvent(CompletedEvent, this, EventArgs.Empty);
                                executionContext.CloseActivity();
                                return;
                            }
                            break;
                        }
                        this.ExecuteTemplate(executionContext, this.ActivityState[num + 1]);
                        return;

                    case System.Workflow.Activities.ExecutionType.Parallel:
                        if (this.ActivityState.IsChildActive || ((this.UntilCondition != null) && !this.UntilCondition.Evaluate(this, executionContext)))
                        {
                            break;
                        }
                        base.RaiseEvent(CompletedEvent, this, EventArgs.Empty);
                        executionContext.CloseActivity();
                        return;

                    default:
                        throw new InvalidOperationException(SR.GetString("Error_ReplicatorInvalidExecutionType"));
                }
            }
        }

        private int IndexOf(object value)
        {
            if (base.ExecutionStatus != ActivityExecutionStatus.Executing)
            {
                throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotExecuting"));
            }
            if (this.ActivityState == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotInitialized"));
            }
            int num = 0;
            for (int i = 0; i < this.ActivityState.Count; i++)
            {
                ChildExecutionStateInfo info = this.ActivityState[i];
                if (!info.MarkedForRemoval)
                {
                    if (object.Equals(info.InstanceData, value))
                    {
                        return num;
                    }
                    num++;
                }
            }
            return -1;
        }

        private void Insert(int index, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (base.ExecutionStatus != ActivityExecutionStatus.Executing)
            {
                throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotExecuting"));
            }
            if (this.ActivityState == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotInitialized"));
            }
            if ((index < 0) || (index > this.ActivityState.AbsoluteCount))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            ChildExecutionStateInfo info = new ChildExecutionStateInfo(value);
            this.ActivityState.Insert(index, info, false);
            this.ScheduleExecutionIfNeeded(info, index);
        }

        public bool IsExecuting(int index)
        {
            if (this.ActivityState == null)
            {
                return false;
            }
            if ((index < 0) || (index >= this.ActivityState.AbsoluteCount))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            ChildExecutionStateInfo info = this.ActivityState[index, false];
            if (info.Status != ChildRunStatus.PendingExecute)
            {
                return (info.Status == ChildRunStatus.Running);
            }
            return true;
        }

        protected override void OnClosed(IServiceProvider provider)
        {
        }

        private void Remove(object obj)
        {
            int index = this.IndexOf(obj);
            if (index >= 0)
            {
                this.RemoveAt(index);
            }
        }

        private void RemoveAt(int index)
        {
            if (base.ExecutionStatus != ActivityExecutionStatus.Executing)
            {
                throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotExecuting"));
            }
            if (this.ActivityState == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotInitialized"));
            }
            if ((index < 0) || (index >= this.ActivityState.AbsoluteCount))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            ChildExecutionStateInfo item = this.ActivityState[index, false];
            if ((item.Status == ChildRunStatus.Completed) || (item.Status == ChildRunStatus.Created))
            {
                this.ActivityState.Remove(item);
            }
            else
            {
                item.MarkedForRemoval = true;
                base.Invoke<ReplicatorInterActivityEventArgs>(new EventHandler<ReplicatorInterActivityEventArgs>(this.HandleChildUpdateOperation), new ReplicatorInterActivityEventArgs(item, false));
            }
        }

        private void ScheduleExecutionIfNeeded(ChildExecutionStateInfo childStateInfo, int index)
        {
            bool flag = this.ExecutionType == System.Workflow.Activities.ExecutionType.Parallel;
            if (!flag)
            {
                int absoluteCount = this.ActivityState.AbsoluteCount;
                if (((index == 0) && (absoluteCount == 1)) || ((index == (absoluteCount - 1)) && (this.ActivityState[absoluteCount - 2, false].Status == ChildRunStatus.Completed)))
                {
                    flag = true;
                }
            }
            if (flag)
            {
                childStateInfo.Status = ChildRunStatus.PendingExecute;
                base.Invoke<ReplicatorInterActivityEventArgs>(new EventHandler<ReplicatorInterActivityEventArgs>(this.HandleChildUpdateOperation), new ReplicatorInterActivityEventArgs(childStateInfo, true));
            }
        }

        private bool TryCancelChild(ActivityExecutionContext outerProvider, ChildExecutionStateInfo childStateInfo)
        {
            bool flag = false;
            ActivityExecutionContextManager executionContextManager = outerProvider.ExecutionContextManager;
            ActivityExecutionContext executionContext = this.GetExecutionContext(executionContextManager, childStateInfo.RunId);
            if (executionContext != null)
            {
                switch (executionContext.Activity.ExecutionStatus)
                {
                    case ActivityExecutionStatus.Executing:
                        executionContext.CancelActivity(executionContext.Activity);
                        return true;

                    case ActivityExecutionStatus.Canceling:
                    case ActivityExecutionStatus.Faulting:
                        return true;

                    case ActivityExecutionStatus.Closed:
                    case ActivityExecutionStatus.Compensating:
                        return flag;
                }
                return flag;
            }
            if ((base.ExecutionStatus != ActivityExecutionStatus.Executing) && (childStateInfo.Status == ChildRunStatus.PendingExecute))
            {
                childStateInfo.Status = ChildRunStatus.Completed;
            }
            return flag;
        }

        private bool TryCancelChildren(ActivityExecutionContext executionContext)
        {
            if (this.ActivityState == null)
            {
                return false;
            }
            ReplicatorStateInfo activityState = this.ActivityState;
            bool flag = false;
            for (int i = 0; i < activityState.Count; i++)
            {
                if (this.TryCancelChild(executionContext, activityState[i]))
                {
                    flag = true;
                }
            }
            return flag;
        }

        private ReplicatorStateInfo ActivityState
        {
            get
            {
                return (ReplicatorStateInfo) base.GetValue(ActivityStateProperty);
            }
            set
            {
                base.SetValue(ActivityStateProperty, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool AllChildrenComplete
        {
            get
            {
                if (this.ActivityState != null)
                {
                    return !this.ActivityState.IsChildActive;
                }
                return true;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public IList CurrentChildData
        {
            get
            {
                if (this.childDataList == null)
                {
                    this.childDataList = new ReplicatorChildInstanceList(this);
                }
                return this.childDataList;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int CurrentIndex
        {
            get
            {
                if (this.ActivityState == null)
                {
                    return -1;
                }
                if (this.ExecutionType == System.Workflow.Activities.ExecutionType.Sequence)
                {
                    return this.ActivityState.CurrentIndex;
                }
                return (this.ActivityState.AbsoluteCount - 1);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public ICollection<Activity> DynamicActivities
        {
            get
            {
                if (base.EnabledActivities.Count > 0)
                {
                    return base.GetDynamicActivities(base.EnabledActivities[0]);
                }
                return new Activity[0];
            }
        }

        [SRDescription("ExecutionTypeDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), SRCategory("Properties"), Browsable(true)]
        public System.Workflow.Activities.ExecutionType ExecutionType
        {
            get
            {
                return (System.Workflow.Activities.ExecutionType) base.GetValue(ExecutionTypeProperty);
            }
            set
            {
                if ((value != System.Workflow.Activities.ExecutionType.Sequence) && (value != System.Workflow.Activities.ExecutionType.Parallel))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if ((this.ActivityState != null) && this.ActivityState.IsChildActive)
                {
                    throw new InvalidOperationException(SR.GetString("Error_ReplicatorChildRunning"));
                }
                base.SetValue(ExecutionTypeProperty, value);
            }
        }

        [Editor(typeof(BindUITypeEditor), typeof(UITypeEditor)), SRCategory("Properties"), DefaultValue((string) null), Browsable(true), SRDescription("InitialChildDataDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public IList InitialChildData
        {
            get
            {
                return (base.GetValue(InitialChildDataProperty) as IList);
            }
            set
            {
                base.SetValue(InitialChildDataProperty, value);
            }
        }

        [SRCategory("Conditions"), DefaultValue((string) null), SRDescription("ReplicatorUntilConditionDescr")]
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

        [Serializable]
        private class ChildExecutionStateInfo
        {
            private object data;
            private bool markedForRemoval;
            private Guid runId;
            private ReplicatorActivity.ChildRunStatus status;

            internal ChildExecutionStateInfo(object instanceData)
            {
                this.data = instanceData;
                this.markedForRemoval = false;
                this.status = ReplicatorActivity.ChildRunStatus.Created;
            }

            internal object InstanceData
            {
                get
                {
                    return this.data;
                }
                set
                {
                    this.data = value;
                }
            }

            internal bool MarkedForRemoval
            {
                get
                {
                    return this.markedForRemoval;
                }
                set
                {
                    this.markedForRemoval = value;
                }
            }

            internal Guid RunId
            {
                get
                {
                    return this.runId;
                }
                set
                {
                    this.runId = value;
                }
            }

            internal ReplicatorActivity.ChildRunStatus Status
            {
                get
                {
                    return this.status;
                }
                set
                {
                    this.status = value;
                }
            }
        }

        private enum ChildRunStatus : byte
        {
            Completed = 3,
            Created = 0,
            PendingExecute = 1,
            Running = 2
        }

        [Serializable]
        private sealed class ReplicatorChildInstanceList : IList, ICollection, IEnumerable
        {
            private ReplicatorActivity replicatorActivity;

            internal ReplicatorChildInstanceList(ReplicatorActivity replicatorActivity)
            {
                this.replicatorActivity = replicatorActivity;
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (this.replicatorActivity == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_ReplicatorDisconnected"));
                }
                if (this.replicatorActivity.ExecutionStatus != ActivityExecutionStatus.Executing)
                {
                    throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotExecuting"));
                }
                if (this.replicatorActivity.ActivityState == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotInitialized"));
                }
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (array.Rank != 1)
                {
                    throw new ArgumentException(SR.GetString("Error_MultiDimensionalArray"), "array");
                }
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                if ((array.Length - index) < this.replicatorActivity.ActivityState.AbsoluteCount)
                {
                    throw new ArgumentException(SR.GetString("Error_InsufficientArrayPassedIn"), "array");
                }
                for (int i = 0; i < this.replicatorActivity.ActivityState.AbsoluteCount; i++)
                {
                    array.SetValue(this.replicatorActivity.ActivityState[i, false].InstanceData, (int) (i + index));
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                if (this.replicatorActivity == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_ReplicatorDisconnected"));
                }
                if (this.replicatorActivity.ExecutionStatus != ActivityExecutionStatus.Executing)
                {
                    throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotExecuting"));
                }
                if (this.replicatorActivity.ActivityState == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotInitialized"));
                }
                for (int i = 0; i < this.replicatorActivity.ActivityState.AbsoluteCount; i++)
                {
                    yield return this.replicatorActivity.ActivityState[i, false].InstanceData;
                }
            }

            int IList.Add(object value)
            {
                if (this.replicatorActivity == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_ReplicatorDisconnected"));
                }
                return this.replicatorActivity.Add(value);
            }

            void IList.Clear()
            {
                if (this.replicatorActivity == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_ReplicatorDisconnected"));
                }
                this.replicatorActivity.Clear();
            }

            bool IList.Contains(object value)
            {
                if (this.replicatorActivity == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_ReplicatorDisconnected"));
                }
                return (this.replicatorActivity.IndexOf(value) != -1);
            }

            int IList.IndexOf(object value)
            {
                if (this.replicatorActivity == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_ReplicatorDisconnected"));
                }
                return this.replicatorActivity.IndexOf(value);
            }

            void IList.Insert(int index, object value)
            {
                if (this.replicatorActivity == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_ReplicatorDisconnected"));
                }
                this.replicatorActivity.Insert(index, value);
            }

            void IList.Remove(object value)
            {
                if (this.replicatorActivity == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_ReplicatorDisconnected"));
                }
                this.replicatorActivity.Remove(value);
            }

            void IList.RemoveAt(int index)
            {
                if (this.replicatorActivity == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_ReplicatorDisconnected"));
                }
                this.replicatorActivity.RemoveAt(index);
            }

            int ICollection.Count
            {
                get
                {
                    if (this.replicatorActivity == null)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_ReplicatorDisconnected"));
                    }
                    if (this.replicatorActivity.ExecutionStatus != ActivityExecutionStatus.Executing)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotExecuting"));
                    }
                    if (this.replicatorActivity.ActivityState == null)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotInitialized"));
                    }
                    return this.replicatorActivity.ActivityState.AbsoluteCount;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            bool IList.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    if (this.replicatorActivity == null)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_ReplicatorDisconnected"));
                    }
                    if (this.replicatorActivity.ExecutionStatus != ActivityExecutionStatus.Executing)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotExecuting"));
                    }
                    if (this.replicatorActivity.ActivityState == null)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotInitialized"));
                    }
                    return this.replicatorActivity.ActivityState[index, false].InstanceData;
                }
                set
                {
                    if (this.replicatorActivity == null)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_ReplicatorDisconnected"));
                    }
                    if (this.replicatorActivity.ExecutionStatus != ActivityExecutionStatus.Executing)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotExecuting"));
                    }
                    if (this.replicatorActivity.ActivityState == null)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_ReplicatorNotInitialized"));
                    }
                    this.replicatorActivity.ActivityState[index, false].InstanceData = value;
                }
            }

        }

        private sealed class ReplicatorInterActivityEventArgs : EventArgs
        {
            private ReplicatorActivity.ChildExecutionStateInfo childStateInfo;
            private bool isAdd;

            internal ReplicatorInterActivityEventArgs(ReplicatorActivity.ChildExecutionStateInfo childStateInfo, bool isAdd)
            {
                this.childStateInfo = childStateInfo;
                this.isAdd = isAdd;
            }

            internal ReplicatorActivity.ChildExecutionStateInfo ChildStateInfo
            {
                get
                {
                    return this.childStateInfo;
                }
            }

            internal bool IsAdd
            {
                get
                {
                    return this.isAdd;
                }
            }
        }

        [Serializable]
        private class ReplicatorStateInfo : List<ReplicatorActivity.ChildExecutionStateInfo>
        {
            internal bool AttemptedCloseWhileFaulting;
            internal bool CompletionConditionTrueAlready;

            internal int Add(ReplicatorActivity.ChildExecutionStateInfo value, bool includeStaleEntries)
            {
                base.Add(value);
                if (includeStaleEntries)
                {
                    return (base.Count - 1);
                }
                return (this.AbsoluteCount - 1);
            }

            internal int FindIndexOfChildStateInfo(Guid runId)
            {
                for (int i = 0; i < base.Count; i++)
                {
                    ReplicatorActivity.ChildExecutionStateInfo info = base[i];
                    if (info.RunId == runId)
                    {
                        return i;
                    }
                }
                throw new IndexOutOfRangeException();
            }

            internal void Insert(int index, ReplicatorActivity.ChildExecutionStateInfo value, bool includeStaleEntries)
            {
                if (includeStaleEntries)
                {
                    base.Insert(index, value);
                }
                else
                {
                    int num = 0;
                    num = 0;
                    while ((num < base.Count) && (index > 0))
                    {
                        if (!base[num].MarkedForRemoval)
                        {
                            index--;
                        }
                        num++;
                    }
                    if (index != 0)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    base.Insert(num, value);
                }
            }

            internal int AbsoluteCount
            {
                get
                {
                    int num = 0;
                    int num2 = 0;
                    while (num2 < base.Count)
                    {
                        if (!base[num2++].MarkedForRemoval)
                        {
                            num++;
                        }
                    }
                    return num;
                }
            }

            internal int CurrentIndex
            {
                get
                {
                    for (int i = 0; i < this.AbsoluteCount; i++)
                    {
                        if (this[i, false].RunId != Guid.Empty)
                        {
                            return i;
                        }
                    }
                    return (this.AbsoluteCount - 1);
                }
            }

            internal bool IsChildActive
            {
                get
                {
                    for (int i = 0; i < base.Count; i++)
                    {
                        ReplicatorActivity.ChildExecutionStateInfo info = base[i];
                        if ((info.Status == ReplicatorActivity.ChildRunStatus.Running) || (info.Status == ReplicatorActivity.ChildRunStatus.PendingExecute))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            internal ReplicatorActivity.ChildExecutionStateInfo this[int index, bool includeStaleEntries]
            {
                get
                {
                    if (includeStaleEntries)
                    {
                        return base[index];
                    }
                    for (int i = 0; i < base.Count; i++)
                    {
                        if (!base[i].MarkedForRemoval && (index-- == 0))
                        {
                            return base[i];
                        }
                    }
                    throw new IndexOutOfRangeException();
                }
            }
        }

        [Serializable]
        private class ReplicatorSubscriber : IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
        {
            private Guid runId;

            internal ReplicatorSubscriber(Activity ownerActivity, Guid runIdentifier)
            {
                this.runId = runIdentifier;
            }

            public override bool Equals(object obj)
            {
                ReplicatorActivity.ReplicatorSubscriber subscriber = obj as ReplicatorActivity.ReplicatorSubscriber;
                return (((subscriber != null) && base.Equals(obj)) && this.runId.Equals(subscriber.runId));
            }

            public override int GetHashCode()
            {
                return (base.GetHashCode() ^ this.runId.GetHashCode());
            }

            void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
            {
                if (sender == null)
                {
                    throw new ArgumentNullException("sender");
                }
                ActivityExecutionContext executionContext = sender as ActivityExecutionContext;
                if (executionContext == null)
                {
                    throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");
                }
                ((ReplicatorActivity) executionContext.Activity).HandleStatusChange(executionContext, e, this);
            }

            internal Guid RunIdentifier
            {
                get
                {
                    return this.runId;
                }
            }
        }
    }
}

