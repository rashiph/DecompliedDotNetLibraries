namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Statements.Tracking;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal sealed class InternalState : NativeActivity<string>
    {
        private Variable<int> currentRunningTriggers;
        private Variable<Bookmark> evaluateConditionBookmark;
        private BookmarkCallback evaluateConditionCallback;
        private Collection<InternalTransition> internalTransitions;
        private Variable<bool> isExiting;
        private CompletionCallback<bool> onConditionComplete;
        private CompletionCallback onEntryComplete;
        private CompletionCallback onExitComplete;
        private CompletionCallback onTriggerComplete;
        private System.Activities.Statements.State state;
        private Dictionary<Activity, InternalTransition> triggerInternalTransitionMapping = new Dictionary<Activity, InternalTransition>();

        public InternalState(System.Activities.Statements.State state)
        {
            this.state = state;
            base.DisplayName = state.DisplayName;
            this.onEntryComplete = new CompletionCallback(this.OnEntryComplete);
            this.onTriggerComplete = new CompletionCallback(this.OnTriggerComplete);
            this.onConditionComplete = new CompletionCallback<bool>(this.OnConditionComplete);
            this.onExitComplete = new CompletionCallback(this.OnExitComplete);
            this.evaluateConditionCallback = new BookmarkCallback(this.StartEvaluateCondition);
            this.currentRunningTriggers = new Variable<int>();
            this.isExiting = new Variable<bool>();
            this.evaluateConditionBookmark = new Variable<Bookmark>();
            this.internalTransitions = new Collection<InternalTransition>();
            this.triggerInternalTransitionMapping = new Dictionary<Activity, InternalTransition>();
        }

        protected override void Abort(NativeActivityAbortContext context)
        {
            this.RemoveActiveBookmark(context);
            base.Abort(context);
        }

        private void AddEvaluateConditionBookmark(NativeActivityContext context)
        {
            Bookmark bookmark = context.CreateBookmark(this.evaluateConditionCallback, BookmarkOptions.MultipleResume);
            this.evaluateConditionBookmark.Set(context, bookmark);
            this.EventManager.Get(context).AddActiveBookmark(bookmark);
        }

        private static void AddTransitionData(NativeActivityMetadata metadata, InternalTransition internalTransition, Transition transition)
        {
            TransitionData item = new TransitionData();
            Activity<bool> condition = transition.Condition;
            item.Condition = condition;
            if (condition != null)
            {
                metadata.AddChild(condition);
            }
            Activity action = transition.Action;
            item.Action = action;
            if (action != null)
            {
                metadata.AddChild(action);
            }
            if (transition.To != null)
            {
                item.To = transition.To.InternalState;
            }
            internalTransition.TransitionDataList.Add(item);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            this.internalTransitions.Clear();
            if (this.Entry != null)
            {
                metadata.AddChild(this.Entry);
            }
            if (this.Exit != null)
            {
                metadata.AddChild(this.Exit);
            }
            this.ProcessTransitions(metadata);
            metadata.SetVariablesCollection(this.Variables);
            metadata.AddArgument(new RuntimeArgument("EventManager", this.EventManager.ArgumentType, ArgumentDirection.In));
            metadata.AddImplementationVariable(this.currentRunningTriggers);
            metadata.AddImplementationVariable(this.isExiting);
            metadata.AddImplementationVariable(this.evaluateConditionBookmark);
        }

        protected override void Cancel(NativeActivityContext context)
        {
            this.RemoveActiveBookmark(context);
            base.Cancel(context);
        }

        protected override void Execute(NativeActivityContext context)
        {
            this.isExiting.Set(context, false);
            this.ScheduleEntry(context);
        }

        private Activity<bool> GetCondition(int triggerIndex, int conditionIndex)
        {
            return this.internalTransitions[triggerIndex].TransitionDataList[conditionIndex].Condition;
        }

        private InternalTransition GetInternalTransition(int triggerIndex)
        {
            return this.internalTransitions[triggerIndex];
        }

        private string GetTo(int triggerIndex, int conditionIndex)
        {
            return this.internalTransitions[triggerIndex].TransitionDataList[conditionIndex].To.StateId;
        }

        private void OnConditionComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance, bool result)
        {
            StateMachineEventManager eventManager = this.EventManager.Get(context);
            int triggedId = eventManager.CurrentBeingProcessedEvent.TriggedId;
            if (result)
            {
                this.TakeTransition(context, eventManager, triggedId);
            }
            else
            {
                int currentConditionIndex = eventManager.CurrentConditionIndex;
                InternalTransition internalTransition = this.GetInternalTransition(triggedId);
                currentConditionIndex++;
                if (currentConditionIndex < internalTransition.TransitionDataList.Count)
                {
                    eventManager.CurrentConditionIndex = currentConditionIndex;
                    context.ScheduleActivity<bool>(internalTransition.TransitionDataList[currentConditionIndex].Condition, this.onConditionComplete, null);
                }
                else
                {
                    context.ScheduleActivity(internalTransition.Trigger, this.onTriggerComplete);
                    this.currentRunningTriggers.Set(context, this.currentRunningTriggers.Get(context) + 1);
                    ProcessNextTriggerCompletedEvent(context, eventManager);
                }
            }
        }

        private void OnEntryComplete(NativeActivityContext context, System.Activities.ActivityInstance instance)
        {
            ProcessNextTriggerCompletedEvent(context, this.EventManager.Get(context));
            this.ScheduleTriggers(context);
        }

        private void OnExitComplete(NativeActivityContext context, System.Activities.ActivityInstance instance)
        {
            this.ScheduleAction(context);
        }

        private void OnTriggerComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            int num = this.currentRunningTriggers.Get(context);
            this.currentRunningTriggers.Set(context, --num);
            bool flag = this.isExiting.Get(context);
            if ((!context.IsCancellationRequested && (num == 0)) && flag)
            {
                this.ScheduleExit(context);
            }
            else if (completedInstance.State == ActivityInstanceState.Closed)
            {
                bool flag2;
                InternalTransition transition = null;
                this.triggerInternalTransitionMapping.TryGetValue(completedInstance.Activity, out transition);
                StateMachineEventManager eventManager = this.EventManager.Get(context);
                TriggerCompletedEvent completedEvent = new TriggerCompletedEvent {
                    Bookmark = this.evaluateConditionBookmark.Get(context),
                    TriggedId = transition.InternalTransitionIndex
                };
                eventManager.RegisterCompletedEvent(completedEvent, out flag2);
                if (flag2)
                {
                    ProcessNextTriggerCompletedEvent(context, eventManager);
                }
            }
        }

        private void PrepareForExit(NativeActivityContext context, string targetStateId)
        {
            ReadOnlyCollection<System.Activities.ActivityInstance> children = context.GetChildren();
            base.Result.Set(context, targetStateId);
            this.isExiting.Set(context, true);
            if (children.Count > 0)
            {
                context.CancelChildren();
            }
            else
            {
                this.ScheduleExit(context);
            }
        }

        private static void ProcessNextTriggerCompletedEvent(NativeActivityContext context, StateMachineEventManager eventManager)
        {
            eventManager.CurrentBeingProcessedEvent = null;
            eventManager.OnTransition = false;
            TriggerCompletedEvent nextCompletedEvent = eventManager.GetNextCompletedEvent();
            if (nextCompletedEvent != null)
            {
                context.GetExtension<StateMachineExtension>().ResumeBookmark(nextCompletedEvent.Bookmark);
            }
        }

        private void ProcessTransitions(NativeActivityMetadata metadata)
        {
            for (int i = 0; i < this.Transitions.Count; i++)
            {
                Transition transition = this.Transitions[i];
                InternalTransition transition2 = null;
                Activity activeTrigger = transition.ActiveTrigger;
                if (!this.triggerInternalTransitionMapping.TryGetValue(activeTrigger, out transition2))
                {
                    metadata.AddChild(activeTrigger);
                    transition2 = new InternalTransition {
                        Trigger = activeTrigger,
                        InternalTransitionIndex = this.internalTransitions.Count
                    };
                    this.triggerInternalTransitionMapping.Add(activeTrigger, transition2);
                    this.internalTransitions.Add(transition2);
                }
                AddTransitionData(metadata, transition2, transition);
            }
        }

        private void RemoveActiveBookmark(ActivityContext context)
        {
            StateMachineEventManager manager = this.EventManager.Get(context);
            Bookmark bookmark = this.evaluateConditionBookmark.Get(context);
            if (bookmark != null)
            {
                manager.RemoveActiveBookmark(bookmark);
            }
        }

        private void RemoveBookmarks(NativeActivityContext context)
        {
            context.RemoveAllBookmarks();
            this.RemoveActiveBookmark(context);
        }

        private void ScheduleAction(NativeActivityContext context)
        {
            StateMachineEventManager manager = this.EventManager.Get(context);
            if (manager.IsReferredByBeingProcessedEvent(this.evaluateConditionBookmark.Get(context)))
            {
                Activity action = this.GetInternalTransition(manager.CurrentBeingProcessedEvent.TriggedId).TransitionDataList[manager.CurrentConditionIndex].Action;
                if (action != null)
                {
                    context.ScheduleActivity(action);
                }
            }
            this.RemoveBookmarks(context);
        }

        private void ScheduleEntry(NativeActivityContext context)
        {
            StateMachineStateRecord record = new StateMachineStateRecord {
                StateMachineName = this.StateMachineName,
                StateName = base.DisplayName
            };
            context.Track(record);
            if (this.Entry != null)
            {
                context.ScheduleActivity(this.Entry, this.onEntryComplete);
            }
            else
            {
                this.onEntryComplete(context, null);
            }
        }

        private void ScheduleExit(NativeActivityContext context)
        {
            if (this.Exit != null)
            {
                context.ScheduleActivity(this.Exit, this.onExitComplete);
            }
            else
            {
                this.onExitComplete(context, null);
            }
        }

        private void ScheduleTriggers(NativeActivityContext context)
        {
            if (!this.IsFinal)
            {
                this.AddEvaluateConditionBookmark(context);
            }
            if (this.internalTransitions.Count > 0)
            {
                foreach (InternalTransition transition in this.internalTransitions)
                {
                    context.ScheduleActivity(transition.Trigger, this.onTriggerComplete);
                }
                this.currentRunningTriggers.Set(context, this.currentRunningTriggers.Get(context) + this.internalTransitions.Count);
            }
        }

        private void StartEvaluateCondition(NativeActivityContext context, Bookmark bookmark, object value)
        {
            StateMachineEventManager eventManager = this.EventManager.Get(context);
            int triggedId = eventManager.CurrentBeingProcessedEvent.TriggedId;
            eventManager.CurrentConditionIndex = 0;
            if (this.GetInternalTransition(triggedId).IsUnconditional)
            {
                this.TakeTransition(context, eventManager, triggedId);
            }
            else
            {
                context.ScheduleActivity<bool>(this.GetCondition(triggedId, eventManager.CurrentConditionIndex), this.onConditionComplete, null);
            }
        }

        private void TakeTransition(NativeActivityContext context, StateMachineEventManager eventManager, int triggerId)
        {
            this.EventManager.Get(context).OnTransition = true;
            this.PrepareForExit(context, this.GetTo(triggerId, eventManager.CurrentConditionIndex));
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        public Activity Entry
        {
            get
            {
                return this.state.Entry;
            }
        }

        [RequiredArgument]
        public InArgument<StateMachineEventManager> EventManager { get; set; }

        public Activity Exit
        {
            get
            {
                return this.state.Exit;
            }
        }

        [DefaultValue(false)]
        public bool IsFinal
        {
            get
            {
                return this.state.IsFinal;
            }
        }

        public string StateId
        {
            get
            {
                return this.state.StateId;
            }
        }

        public string StateMachineName
        {
            get
            {
                return this.state.StateMachineName;
            }
        }

        public Collection<Transition> Transitions
        {
            get
            {
                return this.state.Transitions;
            }
        }

        public Collection<Variable> Variables
        {
            get
            {
                return this.state.Variables;
            }
        }
    }
}

