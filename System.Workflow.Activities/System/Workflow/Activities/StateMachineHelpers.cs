namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel;

    internal static class StateMachineHelpers
    {
        internal static bool ContainsEventActivity(CompositeActivity compositeActivity)
        {
            Queue<Activity> queue = new Queue<Activity>();
            queue.Enqueue(compositeActivity);
            while (queue.Count > 0)
            {
                Activity activity = queue.Dequeue();
                if (activity is IEventActivity)
                {
                    return true;
                }
                compositeActivity = activity as CompositeActivity;
                if (compositeActivity != null)
                {
                    foreach (Activity activity2 in compositeActivity.Activities)
                    {
                        if (activity2.Enabled)
                        {
                            queue.Enqueue(activity2);
                        }
                    }
                }
            }
            return false;
        }

        internal static bool ContainsState(StateActivity state, string stateName)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }
            if (string.IsNullOrEmpty(stateName))
            {
                throw new ArgumentNullException("stateName");
            }
            Queue<StateActivity> queue = new Queue<StateActivity>();
            queue.Enqueue(state);
            while (queue.Count > 0)
            {
                state = queue.Dequeue();
                if (state.QualifiedName.Equals(stateName))
                {
                    return true;
                }
                foreach (Activity activity in state.EnabledActivities)
                {
                    StateActivity item = activity as StateActivity;
                    if (item != null)
                    {
                        queue.Enqueue(item);
                    }
                }
            }
            return false;
        }

        internal static Activity FindActivityByName(CompositeActivity parentActivity, string qualifiedName)
        {
            return parentActivity.GetActivityByName(qualifiedName, true);
        }

        internal static StateActivity FindDynamicStateByName(StateActivity state, string stateQualifiedName)
        {
            while (!state.QualifiedName.Equals(stateQualifiedName) && ContainsState(state, stateQualifiedName))
            {
                foreach (Activity activity in state.EnabledActivities)
                {
                    StateActivity activity2 = activity as StateActivity;
                    if ((activity2 != null) && ContainsState(activity2, stateQualifiedName))
                    {
                        StateActivity dynamicActivity = (StateActivity) state.GetDynamicActivity(activity2.QualifiedName);
                        if (dynamicActivity == null)
                        {
                            return null;
                        }
                        state = dynamicActivity;
                        continue;
                    }
                }
            }
            if (state.QualifiedName.Equals(stateQualifiedName))
            {
                return state;
            }
            return null;
        }

        internal static StateActivity FindEnclosingState(Activity activity)
        {
            StateActivity activity2 = activity as StateActivity;
            if (activity2 != null)
            {
                return activity2;
            }
            if (activity.Parent == null)
            {
                return null;
            }
            return FindEnclosingState(activity.Parent);
        }

        internal static StateActivity FindStateByName(StateActivity state, string qualifiedName)
        {
            return (FindActivityByName(state, qualifiedName) as StateActivity);
        }

        internal static string GetCompletedStateName(StateActivity state)
        {
            return (string) GetRootState(state).GetValue(StateMachineWorkflowActivity.CompletedStateNameProperty);
        }

        internal static StateActivity GetCurrentState(ActivityExecutionContext context)
        {
            StateActivity state = context.Activity as StateActivity;
            if (state == null)
            {
                state = FindEnclosingState(context.Activity);
            }
            StateActivity rootState = GetRootState(state);
            string currentStateName = StateMachineExecutionState.Get(rootState).CurrentStateName;
            if (currentStateName == null)
            {
                return null;
            }
            return FindDynamicStateByName(rootState, currentStateName);
        }

        internal static IEventActivity GetEventActivity(EventDrivenActivity eventDriven)
        {
            CompositeActivity activity = eventDriven;
            return (activity.EnabledActivities[0] as IEventActivity);
        }

        internal static string GetInitialStateName(StateActivity state)
        {
            return (string) GetRootState(state).GetValue(StateMachineWorkflowActivity.InitialStateNameProperty);
        }

        internal static EventDrivenActivity GetParentEventDriven(IEventActivity eventActivity)
        {
            for (Activity activity = ((Activity) eventActivity).Parent; activity != null; activity = activity.Parent)
            {
                EventDrivenActivity activity2 = activity as EventDrivenActivity;
                if (activity2 != null)
                {
                    return activity2;
                }
            }
            return null;
        }

        internal static StateActivity GetRootState(StateActivity state)
        {
            if ((state.Parent != null) && (state.Parent is StateActivity))
            {
                return GetRootState((StateActivity) state.Parent);
            }
            return state;
        }

        internal static bool IsCompletedState(StateActivity state)
        {
            string completedStateName = GetCompletedStateName(state);
            if (completedStateName == null)
            {
                return false;
            }
            return state.QualifiedName.Equals(completedStateName);
        }

        internal static bool IsInitialState(StateActivity state)
        {
            string initialStateName = GetInitialStateName(state);
            if (initialStateName == null)
            {
                return false;
            }
            return state.QualifiedName.Equals(initialStateName);
        }

        internal static bool IsLeafState(StateActivity state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }
            if (IsRootState(state))
            {
                return false;
            }
            foreach (Activity activity in state.EnabledActivities)
            {
                if (activity is StateActivity)
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsRootExecutionContext(ActivityExecutionContext context)
        {
            return (context.Activity.Parent == null);
        }

        internal static bool IsRootState(StateActivity state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }
            StateActivity parent = state.Parent as StateActivity;
            return (parent == null);
        }

        internal static bool IsStateMachine(StateActivity state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }
            return (state is StateMachineWorkflowActivity);
        }
    }
}

