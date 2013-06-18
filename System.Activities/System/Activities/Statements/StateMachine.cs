namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("States")]
    public sealed class StateMachine : NativeActivity
    {
        private Variable<StateMachineEventManager> eventManager;
        private const string exitProperty = "Exit";
        private static Func<StateMachineExtension> getDefaultExtension = new Func<StateMachineExtension>(StateMachine.GetStateMachineExtension);
        private Collection<ActivityFunc<StateMachineEventManager, string>> internalStateFuncs = new Collection<ActivityFunc<StateMachineEventManager, string>>();
        private Collection<InternalState> internalStates = new Collection<InternalState>();
        private CompletionCallback<string> onStateComplete;
        private const string rootId = "0";
        private Collection<System.Activities.Statements.State> states;
        private Collection<Variable> variables;

        public StateMachine()
        {
            Variable<StateMachineEventManager> variable = new Variable<StateMachineEventManager> {
                Name = "EventManager",
                Default = new LambdaValue<StateMachineEventManager>(ctx => new StateMachineEventManager())
            };
            this.eventManager = variable;
            this.onStateComplete = new CompletionCallback<string>(this.OnStateComplete);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            this.internalStateFuncs.Clear();
            this.internalStates.Clear();
            this.PassNumber++;
            this.TraverseViaTransitions(new Action<System.Activities.Statements.State>(StateMachine.ClearState), new Action<Transition>(StateMachine.ClearTransition));
            this.PassNumber++;
            bool checkReached = false;
            this.TraverseStates((m, states) => ClearStates(states), (m, state) => ClearTransitions(state), metadata, checkReached);
            this.PassNumber++;
            bool flag2 = false;
            this.TraverseStates(new Action<NativeActivityMetadata, Collection<System.Activities.Statements.State>>(this.MarkStatesViaChildren), (m, state) => MarkTransitionsInState(state), metadata, flag2);
            this.PassNumber++;
            this.TraverseViaTransitions(state => MarkStateViaTransition(state), null);
            this.PassNumber++;
            Action<Transition> actionForTransition = null;
            this.TraverseViaTransitions(state => ValidateTransitions(metadata, state), actionForTransition);
            this.PassNumber++;
            NativeActivityMetadata metadata2 = metadata;
            bool flag3 = true;
            this.TraverseStates(new Action<NativeActivityMetadata, Collection<System.Activities.Statements.State>>(StateMachine.ValidateStates), delegate (NativeActivityMetadata m, System.Activities.Statements.State state) {
                if (!state.Reachable)
                {
                    ValidateTransitions(m, state);
                }
            }, metadata2, flag3);
            this.ValidateStateMachine(metadata);
            this.ProcessStates(metadata);
            metadata.AddImplementationVariable(this.eventManager);
            foreach (Variable variable in this.Variables)
            {
                metadata.AddVariable(variable);
            }
            metadata.AddDefaultExtensionProvider<StateMachineExtension>(getDefaultExtension);
        }

        private static void ClearState(System.Activities.Statements.State state)
        {
            state.StateId = null;
            state.Reachable = false;
            state.ClearInternalState();
        }

        private static void ClearStates(Collection<System.Activities.Statements.State> states)
        {
            foreach (System.Activities.Statements.State state in states)
            {
                ClearState(state);
            }
        }

        private static void ClearTransition(Transition transition)
        {
            transition.Source = null;
        }

        private static void ClearTransitions(System.Activities.Statements.State state)
        {
            foreach (Transition transition in state.Transitions)
            {
                ClearTransition(transition);
            }
        }

        protected override void Execute(NativeActivityContext context)
        {
            StateMachineEventManager argument = this.eventManager.Get(context);
            argument.OnTransition = true;
            int childStateIndex = StateMachineIdHelper.GetChildStateIndex("0", this.InitialState.StateId);
            context.ScheduleFunc<StateMachineEventManager, string>(this.internalStateFuncs[childStateIndex], argument, this.onStateComplete, null);
        }

        private static StateMachineExtension GetStateMachineExtension()
        {
            return new StateMachineExtension();
        }

        private void MarkStatesViaChildren(NativeActivityMetadata metadata, Collection<System.Activities.Statements.State> states)
        {
            if (states.Count > 0)
            {
                for (int i = 0; i < states.Count; i++)
                {
                    System.Activities.Statements.State state = states[i];
                    if (string.IsNullOrEmpty(state.StateId))
                    {
                        state.StateId = StateMachineIdHelper.GenerateStateId("0", i);
                        state.StateMachineName = base.DisplayName;
                    }
                    else
                    {
                        bool isWarning = false;
                        metadata.AddValidationError(new ValidationError(System.Activities.SR.StateCannotBeAddedTwice(state.DisplayName), isWarning));
                    }
                }
            }
        }

        private static void MarkStateViaTransition(System.Activities.Statements.State state)
        {
            state.Reachable = true;
        }

        private static void MarkTransitionsInState(System.Activities.Statements.State state)
        {
            if (state.Transitions.Count > 0)
            {
                for (int i = 0; i < state.Transitions.Count; i++)
                {
                    Transition transition = state.Transitions[i];
                    if (!string.IsNullOrEmpty(state.StateId))
                    {
                        transition.Id = StateMachineIdHelper.GenerateTransitionId(state.StateId, i);
                    }
                }
            }
        }

        private void OnStateComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance, string result)
        {
            if (StateMachineIdHelper.IsAncestor("0", result))
            {
                int childStateIndex = StateMachineIdHelper.GetChildStateIndex("0", result);
                context.ScheduleFunc<StateMachineEventManager, string>(this.internalStateFuncs[childStateIndex], this.eventManager.Get(context), this.onStateComplete, null);
            }
        }

        private void ProcessStates(NativeActivityMetadata metadata)
        {
            foreach (System.Activities.Statements.State state in this.states.Distinct<System.Activities.Statements.State>())
            {
                InternalState internalState = state.InternalState;
                this.internalStates.Add(internalState);
                DelegateInArgument<StateMachineEventManager> argument = new DelegateInArgument<StateMachineEventManager>();
                internalState.EventManager = argument;
                ActivityFunc<StateMachineEventManager, string> activityDelegate = new ActivityFunc<StateMachineEventManager, string> {
                    Argument = argument,
                    Handler = internalState
                };
                if (state.Reachable)
                {
                    metadata.AddDelegate(activityDelegate);
                }
                this.internalStateFuncs.Add(activityDelegate);
            }
        }

        private void TraverseStates(Action<NativeActivityMetadata, Collection<System.Activities.Statements.State>> actionForStates, Action<NativeActivityMetadata, System.Activities.Statements.State> actionForTransitions, NativeActivityMetadata metadata, bool checkReached)
        {
            if (actionForStates != null)
            {
                actionForStates(metadata, this.States);
            }
            uint passNumber = this.PassNumber;
            foreach (System.Activities.Statements.State state in this.States.Distinct<System.Activities.Statements.State>())
            {
                if (!checkReached || state.Reachable)
                {
                    state.PassNumber = passNumber;
                    if (actionForTransitions != null)
                    {
                        actionForTransitions(metadata, state);
                    }
                }
            }
        }

        private void TraverseViaTransitions(Action<System.Activities.Statements.State> actionForState, Action<Transition> actionForTransition)
        {
            Stack<System.Activities.Statements.State> stack = new Stack<System.Activities.Statements.State>();
            stack.Push(this.InitialState);
            uint passNumber = this.PassNumber;
            while (stack.Count > 0)
            {
                System.Activities.Statements.State state = stack.Pop();
                if ((state != null) && (state.PassNumber != passNumber))
                {
                    state.PassNumber = passNumber;
                    if (actionForState != null)
                    {
                        actionForState(state);
                    }
                    foreach (Transition transition in state.Transitions)
                    {
                        if (actionForTransition != null)
                        {
                            actionForTransition(transition);
                        }
                        stack.Push(transition.To);
                    }
                }
            }
        }

        private static void ValidateState(NativeActivityMetadata metadata, System.Activities.Statements.State state)
        {
            if (state.Reachable)
            {
                if (state.IsFinal)
                {
                    if (state.Exit != null)
                    {
                        bool isWarning = false;
                        metadata.AddValidationError(new ValidationError(System.Activities.SR.FinalStateCannotHaveProperty(state.DisplayName, "Exit"), isWarning));
                    }
                    if (state.Transitions.Count > 0)
                    {
                        bool flag2 = false;
                        metadata.AddValidationError(new ValidationError(System.Activities.SR.FinalStateCannotHaveTransition(state.DisplayName), flag2));
                    }
                }
                else if (state.Transitions.Count == 0)
                {
                    bool flag3 = false;
                    metadata.AddValidationError(new ValidationError(System.Activities.SR.SimpleStateMustHaveOneTransition(state.DisplayName), flag3));
                }
            }
        }

        private void ValidateStateMachine(NativeActivityMetadata metadata)
        {
            if (this.InitialState == null)
            {
                metadata.AddValidationError(System.Activities.SR.StateMachineMustHaveInitialState(base.DisplayName));
            }
            else
            {
                if (this.InitialState.IsFinal)
                {
                    bool isWarning = false;
                    metadata.AddValidationError(new ValidationError(System.Activities.SR.InitialStateCannotBeFinalState(this.InitialState.DisplayName), isWarning));
                }
                if (!this.States.Contains(this.InitialState))
                {
                    bool flag2 = false;
                    metadata.AddValidationError(new ValidationError(System.Activities.SR.InitialStateNotInStatesCollection(this.InitialState.DisplayName), flag2));
                }
            }
        }

        private static void ValidateStates(NativeActivityMetadata metadata, Collection<System.Activities.Statements.State> states)
        {
            foreach (System.Activities.Statements.State state in states)
            {
                ValidateState(metadata, state);
            }
        }

        private static void ValidateTransitions(NativeActivityMetadata metadata, System.Activities.Statements.State currentState)
        {
            Collection<Transition> transitions = currentState.Transitions;
            HashSet<Activity> set = new HashSet<Activity>();
            Dictionary<Activity, List<Transition>> dictionary = new Dictionary<Activity, List<Transition>>();
            foreach (Transition transition in transitions)
            {
                if (transition.Source != null)
                {
                    bool isWarning = false;
                    metadata.AddValidationError(new ValidationError(System.Activities.SR.TransitionCannotBeAddedTwice(transition.DisplayName, currentState.DisplayName, transition.Source.DisplayName), isWarning));
                }
                else
                {
                    transition.Source = currentState;
                    if (transition.To == null)
                    {
                        bool flag2 = false;
                        metadata.AddValidationError(new ValidationError(System.Activities.SR.TransitionTargetCannotBeNull(transition.DisplayName, currentState.DisplayName), flag2));
                    }
                    else if (string.IsNullOrEmpty(transition.To.StateId))
                    {
                        bool flag3 = false;
                        metadata.AddValidationError(new ValidationError(System.Activities.SR.StateNotBelongToAnyParent(transition.DisplayName, transition.To.DisplayName), flag3));
                    }
                    Activity activeTrigger = transition.ActiveTrigger;
                    if (transition.Condition == null)
                    {
                        if (!dictionary.ContainsKey(activeTrigger))
                        {
                            dictionary.Add(activeTrigger, new List<Transition>());
                        }
                        dictionary[activeTrigger].Add(transition);
                    }
                    else
                    {
                        set.Add(activeTrigger);
                    }
                }
            }
            foreach (KeyValuePair<Activity, List<Transition>> pair in dictionary)
            {
                if (set.Contains(pair.Key) || (pair.Value.Count > 1))
                {
                    foreach (Transition transition2 in pair.Value)
                    {
                        if (transition2.Trigger != null)
                        {
                            bool flag4 = false;
                            metadata.AddValidationError(new ValidationError(System.Activities.SR.UnconditionalTransitionShouldNotShareTriggersWithOthers(transition2.DisplayName, currentState.DisplayName, transition2.Trigger.DisplayName), flag4));
                        }
                        else
                        {
                            bool flag5 = false;
                            metadata.AddValidationError(new ValidationError(System.Activities.SR.UnconditionalTransitionShouldNotShareNullTriggersWithOthers(transition2.DisplayName, currentState.DisplayName), flag5));
                        }
                    }
                }
            }
        }

        [DefaultValue((string) null)]
        public System.Activities.Statements.State InitialState { get; set; }

        private uint PassNumber { get; set; }

        [DependsOn("InitialState")]
        public Collection<System.Activities.Statements.State> States
        {
            get
            {
                if (this.states == null)
                {
                    ValidatingCollection<System.Activities.Statements.State> validatings = new ValidatingCollection<System.Activities.Statements.State> {
                        OnAddValidationCallback = delegate (System.Activities.Statements.State item) {
                            if (item == null)
                            {
                                throw FxTrace.Exception.AsError(new ArgumentNullException("item"));
                            }
                        }
                    };
                    this.states = validatings;
                }
                return this.states;
            }
        }

        [DependsOn("States")]
        public Collection<Variable> Variables
        {
            get
            {
                if (this.variables == null)
                {
                    ValidatingCollection<Variable> validatings = new ValidatingCollection<Variable> {
                        OnAddValidationCallback = delegate (Variable item) {
                            if (item == null)
                            {
                                throw FxTrace.Exception.AsError(new ArgumentNullException("item"));
                            }
                        }
                    };
                    this.variables = validatings;
                }
                return this.variables;
            }
        }
    }
}

