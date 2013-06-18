namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    [SRCategory("Standard"), ComVisible(false), SRDescription("StateActivityDescription"), ToolboxItem(typeof(ActivityToolboxItem)), Designer(typeof(StateDesigner), typeof(IDesigner)), ToolboxBitmap(typeof(StateActivity), "Resources.StateActivity.png"), ActivityValidator(typeof(StateActivityValidator))]
    public class StateActivity : CompositeActivity
    {
        public const string StateChangeTrackingDataKey = "StateActivity.StateChange";
        internal static DependencyProperty StateMachineExecutionStateProperty = DependencyProperty.Register("StateMachineExecutionState", typeof(StateMachineExecutionState), typeof(StateActivity), new PropertyMetadata());

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public StateActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public StateActivity(string name) : base(name)
        {
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            CleanUp(executionContext);
            bool flag = true;
            foreach (ActivityExecutionContext context in executionContext.ExecutionContextManager.ExecutionContexts)
            {
                if (context.Activity.Parent == this)
                {
                    flag = false;
                    if (context.Activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                    {
                        context.CancelActivity(context.Activity);
                    }
                }
            }
            if (!flag)
            {
                return base.ExecutionStatus;
            }
            return ActivityExecutionStatus.Closed;
        }

        private static void CleanUp(ActivityExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            StateActivity state = (StateActivity) context.Activity;
            if (state.ExecutionStatus != ActivityExecutionStatus.Faulting)
            {
                StateMachineSubscriptionManager subscriptionManager = GetExecutionState(state).SubscriptionManager;
                subscriptionManager.UnsubscribeState(context);
                if (StateMachineHelpers.IsRootState(state))
                {
                    subscriptionManager.DeleteSetStateEventQueue(context);
                }
                else if (StateMachineHelpers.IsLeafState(state))
                {
                    subscriptionManager.UnsubscribeToSetStateEvent(context);
                }
            }
        }

        private static void CleanupChildAtClosure(ActivityExecutionContext context, Activity childActivity)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (childActivity == null)
            {
                throw new ArgumentNullException("childActivity");
            }
            StateActivity state = (StateActivity) context.Activity;
            GetExecutionState(state);
            childActivity.Closed -= new EventHandler<ActivityExecutionStatusChangedEventArgs>(state.HandleChildActivityClosed);
            ActivityExecutionContextManager executionContextManager = context.ExecutionContextManager;
            ActivityExecutionContext executionContext = executionContextManager.GetExecutionContext(childActivity);
            executionContextManager.CompleteExecutionContext(executionContext);
        }

        private static void Complete(ActivityExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            StateActivity activity = (StateActivity) context.Activity;
            StateMachineExecutionState executionState = GetExecutionState(activity);
            if (StateMachineHelpers.IsLeafState(activity))
            {
                executionState.PreviousStateName = activity.Name;
            }
            CleanUp(context);
            executionState.SchedulerBusy = true;
            context.CloseActivity();
        }

        private static void EnteringLeafState(ActivityExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            StateActivity activity = (StateActivity) context.Activity;
            StateMachineExecutionState executionState = GetExecutionState(activity);
            executionState.SubscriptionManager.SubscribeToSetStateEvent(context);
            StateMachineHelpers.GetCompletedStateName(activity);
            if (StateMachineHelpers.IsCompletedState(activity))
            {
                EnteringStateAction action = new EnteringStateAction(activity.QualifiedName);
                executionState.EnqueueAction(action);
                executionState.ProcessActions(context);
                executionState.Completed = true;
                LeavingState(context);
            }
            else
            {
                if (string.IsNullOrEmpty(executionState.NextStateName))
                {
                    executionState.SubscriptionManager.ReevaluateSubscriptions(context);
                    EnteringStateAction action2 = new EnteringStateAction(activity.QualifiedName);
                    executionState.EnqueueAction(action2);
                    executionState.LockQueue();
                }
                else
                {
                    EnteringStateAction action3 = new EnteringStateAction(activity.QualifiedName);
                    executionState.EnqueueAction(action3);
                    executionState.ProcessTransitionRequest(context);
                }
                executionState.ProcessActions(context);
            }
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (StateMachineHelpers.IsRootState(this))
            {
                this.ExecuteRootState(executionContext);
            }
            else if (StateMachineHelpers.IsLeafState(this))
            {
                ExecuteLeafState(executionContext);
            }
            else
            {
                ExecuteState(executionContext);
            }
            return base.ExecutionStatus;
        }

        private static void ExecuteChild(ActivityExecutionContext context, Activity childActivity)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (childActivity == null)
            {
                throw new ArgumentNullException("childActivity");
            }
            StateActivity state = (StateActivity) context.Activity;
            GetExecutionState(state).SchedulerBusy = true;
            ActivityExecutionContext context2 = context.ExecutionContextManager.CreateExecutionContext(childActivity);
            context2.Activity.Closed += new EventHandler<ActivityExecutionStatusChangedEventArgs>(state.HandleChildActivityClosed);
            context2.ExecuteActivity(context2.Activity);
        }

        internal static void ExecuteEventDriven(ActivityExecutionContext context, EventDrivenActivity eventDriven)
        {
            GetExecutionState(context);
            ExecuteChild(context, eventDriven);
        }

        private static void ExecuteLeafState(ActivityExecutionContext context)
        {
            StateActivity activity = (StateActivity) context.Activity;
            StateMachineExecutionState executionState = GetExecutionState(activity);
            executionState.SchedulerBusy = false;
            executionState.CurrentStateName = activity.QualifiedName;
            StateInitializationActivity stateInitialization = GetStateInitialization(context);
            if (stateInitialization != null)
            {
                ExecuteStateInitialization(context, stateInitialization);
            }
            else
            {
                EnteringLeafState(context);
            }
        }

        private void ExecuteRootState(ActivityExecutionContext context)
        {
            StateActivity activity = (StateActivity) context.Activity;
            StateMachineExecutionState state = new StateMachineExecutionState(base.WorkflowInstanceId) {
                SchedulerBusy = false
            };
            activity.SetValue(StateMachineExecutionStateProperty, state);
            state.SubscriptionManager.CreateSetStateEventQueue(context);
            string initialStateName = StateMachineHelpers.GetInitialStateName(activity);
            state.CalculateStateTransition(this, initialStateName);
            state.ProcessActions(context);
        }

        private static void ExecuteState(ActivityExecutionContext context)
        {
            StateMachineExecutionState executionState = GetExecutionState(context);
            executionState.SchedulerBusy = false;
            executionState.ProcessActions(context);
        }

        internal static void ExecuteState(ActivityExecutionContext context, StateActivity state)
        {
            GetExecutionState(context);
            ExecuteChild(context, state);
        }

        private static void ExecuteStateFinalization(ActivityExecutionContext context, StateFinalizationActivity stateFinalization)
        {
            GetExecutionState(context);
            ExecuteChild(context, stateFinalization);
        }

        private static void ExecuteStateInitialization(ActivityExecutionContext context, StateInitializationActivity stateInitialization)
        {
            GetExecutionState(context);
            ExecuteChild(context, stateInitialization);
        }

        public Activity GetDynamicActivity(string childActivityName)
        {
            if (childActivityName == null)
            {
                throw new ArgumentNullException("childActivityName");
            }
            Activity childActivity = null;
            for (int i = 0; i < base.EnabledActivities.Count; i++)
            {
                if (base.EnabledActivities[i].QualifiedName.Equals(childActivityName))
                {
                    childActivity = base.EnabledActivities[i];
                    break;
                }
            }
            if (childActivity == null)
            {
                throw new ArgumentException(SR.GetString("Error_StateChildNotFound"), "childActivityName");
            }
            return this.GetDynamicActivity(childActivity);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        private Activity GetDynamicActivity(Activity childActivity)
        {
            if (childActivity == null)
            {
                throw new ArgumentNullException("childActivity");
            }
            if (!base.EnabledActivities.Contains(childActivity))
            {
                throw new ArgumentException(SR.GetString("Error_StateChildNotFound"), "childActivity");
            }
            Activity[] dynamicActivities = base.GetDynamicActivities(childActivity);
            if (dynamicActivities.Length != 0)
            {
                return dynamicActivities[0];
            }
            return null;
        }

        private static StateMachineExecutionState GetExecutionState(StateActivity state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }
            return StateMachineExecutionState.Get(StateMachineHelpers.GetRootState(state));
        }

        private static StateMachineExecutionState GetExecutionState(ActivityExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            StateActivity state = (StateActivity) context.Activity;
            return GetExecutionState(state);
        }

        private static T GetHandlerActivity<T>(ActivityExecutionContext context) where T: class
        {
            StateActivity activity = (StateActivity) context.Activity;
            foreach (Activity activity2 in activity.EnabledActivities)
            {
                T local = activity2 as T;
                if (local != null)
                {
                    return local;
                }
            }
            return default(T);
        }

        private static StateFinalizationActivity GetStateFinalization(ActivityExecutionContext context)
        {
            StateActivity activity = (StateActivity) context.Activity;
            return GetHandlerActivity<StateFinalizationActivity>(context);
        }

        private static StateInitializationActivity GetStateInitialization(ActivityExecutionContext context)
        {
            StateActivity activity = (StateActivity) context.Activity;
            return GetHandlerActivity<StateInitializationActivity>(context);
        }

        private void HandleChildActivityClosed(object sender, ActivityExecutionStatusChangedEventArgs eventArgs)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
            {
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");
            }
            if (eventArgs == null)
            {
                throw new ArgumentNullException("eventArgs");
            }
            Activity childActivity = eventArgs.Activity;
            StateActivity activity = (StateActivity) context.Activity;
            GetExecutionState(context).SchedulerBusy = false;
            CleanupChildAtClosure(context, childActivity);
            switch (activity.ExecutionStatus)
            {
                case ActivityExecutionStatus.Executing:
                    if (!(childActivity is EventDrivenActivity))
                    {
                        StateInitializationActivity stateInitialization = childActivity as StateInitializationActivity;
                        if (stateInitialization != null)
                        {
                            HandleStateInitializationCompleted(context, stateInitialization);
                            return;
                        }
                        if (childActivity is StateFinalizationActivity)
                        {
                            HandleStateFinalizationCompleted(context);
                            return;
                        }
                        if (childActivity is StateActivity)
                        {
                            HandleSubStateCompleted(context);
                            return;
                        }
                        InvalidChildActivity(activity);
                        return;
                    }
                    HandleEventDrivenCompleted(context);
                    return;

                case ActivityExecutionStatus.Canceling:
                case ActivityExecutionStatus.Faulting:
                    context.CloseActivity();
                    return;
            }
            throw new InvalidOperationException(SR.GetInvalidActivityStatus(context.Activity));
        }

        private static void HandleEventDrivenCompleted(ActivityExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            StateActivity activity = (StateActivity) context.Activity;
            StateMachineExecutionState executionState = GetExecutionState(context);
            if (string.IsNullOrEmpty(executionState.NextStateName))
            {
                executionState.SubscriptionManager.ReevaluateSubscriptions(context);
                executionState.LockQueue();
            }
            else
            {
                executionState.ProcessTransitionRequest(context);
            }
            executionState.ProcessActions(context);
        }

        private void HandleProcessActionEvent(object sender, EventArgs eventArgs)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
            {
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");
            }
            StateMachineExecutionState executionState = GetExecutionState(context);
            executionState.SchedulerBusy = false;
            executionState.ProcessActions(context);
        }

        private static void HandleStateFinalizationCompleted(ActivityExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            GetExecutionState(context);
            Complete(context);
        }

        private static void HandleStateInitializationCompleted(ActivityExecutionContext context, StateInitializationActivity stateInitialization)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (stateInitialization == null)
            {
                throw new ArgumentNullException("stateInitialization");
            }
            StateActivity activity = (StateActivity) context.Activity;
            StateMachineExecutionState executionState = GetExecutionState(activity);
            if (!string.IsNullOrEmpty(executionState.NextStateName) && executionState.NextStateName.Equals(activity.QualifiedName))
            {
                throw new InvalidOperationException(SR.GetInvalidSetStateInStateInitialization());
            }
            EnteringLeafState(context);
        }

        private static void HandleSubStateCompleted(ActivityExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            StateMachineExecutionState executionState = GetExecutionState(context);
            if (executionState.Completed)
            {
                LeavingState(context);
            }
            else
            {
                executionState.ProcessActions(context);
            }
        }

        protected override void Initialize(IServiceProvider provider)
        {
            base.Initialize(provider);
            ActivityExecutionContext context = (ActivityExecutionContext) provider;
            if (!StateMachineHelpers.IsStateMachine(StateMachineHelpers.GetRootState(this)))
            {
                throw new InvalidOperationException(SR.GetError_StateActivityMustBeContainedInAStateMachine());
            }
            string initialStateName = StateMachineHelpers.GetInitialStateName(this);
            if (string.IsNullOrEmpty(initialStateName))
            {
                throw new InvalidOperationException(SR.GetError_CannotExecuteStateMachineWithoutInitialState());
            }
            if (base.QualifiedName != initialStateName)
            {
                StateMachineSubscriptionManager.DisableStateWorkflowQueues(context, this);
            }
        }

        private static void InvalidChildActivity(StateActivity state)
        {
            if (StateMachineHelpers.IsLeafState(state))
            {
                throw new InvalidOperationException(SR.GetError_InvalidLeafStateChild());
            }
            throw new InvalidOperationException(SR.GetError_InvalidCompositeStateChild());
        }

        internal static void LeavingState(ActivityExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            StateActivity state = (StateActivity) context.Activity;
            if (StateMachineHelpers.IsLeafState(state))
            {
                StateFinalizationActivity stateFinalization = GetStateFinalization(context);
                if (stateFinalization == null)
                {
                    Complete(context);
                }
                else
                {
                    ExecuteStateFinalization(context, stateFinalization);
                }
            }
            else
            {
                Complete(context);
            }
        }

        protected override void OnActivityChangeAdd(ActivityExecutionContext executionContext, Activity addedActivity)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (addedActivity == null)
            {
                throw new ArgumentNullException("addedActivity");
            }
            if (addedActivity.Enabled && (executionContext.Activity.ExecutionStatus == ActivityExecutionStatus.Executing))
            {
                EventDrivenActivity eventDriven = addedActivity as EventDrivenActivity;
                if (eventDriven != null)
                {
                    StateMachineSubscriptionManager.ChangeEventDrivenQueueState(executionContext, eventDriven, false);
                    StateMachineExecutionState state = StateMachineExecutionState.Get(StateMachineHelpers.GetRootState(executionContext.Activity as StateActivity));
                    if (StateMachineHelpers.GetCurrentState(executionContext) != null)
                    {
                        state.SubscriptionManager.ReevaluateSubscriptions(executionContext);
                        state.LockQueue();
                        state.ProcessActions(executionContext);
                    }
                }
            }
        }

        protected override void OnClosed(IServiceProvider provider)
        {
            base.RemoveProperty(StateMachineExecutionStateProperty);
        }

        internal void RaiseProcessActionEvent(ActivityExecutionContext context)
        {
            GetExecutionState(context).SchedulerBusy = true;
            base.Invoke<EventArgs>(new EventHandler<EventArgs>(this.HandleProcessActionEvent), new EventArgs());
        }
    }
}

