namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    [ComVisible(false)]
    public class StateActivityValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection(base.Validate(manager, obj));
            StateActivity state = obj as StateActivity;
            if (state == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(StateActivity).FullName }), "obj");
            }
            if (state.Parent != null)
            {
                if (StateMachineHelpers.IsStateMachine(state))
                {
                    validationErrors.Add(new ValidationError(SR.GetError_StateMachineWorkflowMustBeARootActivity(), 0x621));
                    return validationErrors;
                }
                if (!(state.Parent is StateActivity))
                {
                    validationErrors.Add(new ValidationError(SR.GetError_InvalidStateActivityParent(), 0x62b));
                    return validationErrors;
                }
            }
            if ((state.Parent == null) && !StateMachineHelpers.IsStateMachine(state))
            {
                ValidateCustomStateActivity(state, validationErrors);
            }
            if (StateMachineHelpers.IsLeafState(state))
            {
                ValidateLeafState(state, validationErrors);
            }
            else if (StateMachineHelpers.IsRootState(state))
            {
                ValidateRootState(state, validationErrors);
            }
            else
            {
                ValidateState(state, validationErrors);
            }
            ValidateEventDrivenActivities(state, validationErrors);
            return validationErrors;
        }

        public override ValidationError ValidateActivityChange(Activity activity, ActivityChangeAction action)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            if (((activity.ExecutionStatus != ActivityExecutionStatus.Initialized) && (activity.ExecutionStatus != ActivityExecutionStatus.Executing)) && (activity.ExecutionStatus != ActivityExecutionStatus.Closed))
            {
                return new ValidationError(SR.GetString("Error_DynamicActivity2", new object[] { activity.QualifiedName, activity.ExecutionStatus, activity.GetType().FullName }), 0x50f);
            }
            RemovedActivityAction action2 = action as RemovedActivityAction;
            if (action2 != null)
            {
                StateActivity originalRemovedActivity = action2.OriginalRemovedActivity as StateActivity;
                if (originalRemovedActivity != null)
                {
                    return new ValidationError(SR.GetError_CantRemoveState(originalRemovedActivity.QualifiedName), 0x61b);
                }
                if (activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    EventDrivenActivity activity3 = action2.OriginalRemovedActivity as EventDrivenActivity;
                    if (activity3 != null)
                    {
                        return new ValidationError(SR.GetError_CantRemoveEventDrivenFromExecutingState(activity3.QualifiedName, activity.QualifiedName), 0x620);
                    }
                }
            }
            return null;
        }

        private static void ValidateCompletedState(StateActivity state, ValidationErrorCollection validationErrors)
        {
            string completedStateName = StateMachineHelpers.GetCompletedStateName(state);
            if (!string.IsNullOrEmpty(completedStateName))
            {
                StateActivity activity = StateMachineHelpers.FindStateByName(state, completedStateName);
                if (activity == null)
                {
                    validationErrors.Add(new ValidationError(SR.GetError_CompletedStateMustPointToAState(), 0x5f6, false, "CompletedStateName"));
                }
                else if (StateMachineHelpers.IsLeafState(activity))
                {
                    if (activity.EnabledActivities.Count > 0)
                    {
                        validationErrors.Add(new ValidationError(SR.GetString("Error_CompletedStateCannotContainActivities"), 0x5ff, false, "CompletedStateName"));
                    }
                }
                else
                {
                    validationErrors.Add(new ValidationError(SR.GetError_CompletedStateMustPointToALeafNodeState(), 0x5f8, false, "CompletedStateName"));
                }
            }
        }

        private static void ValidateCompositeStateChildren(StateActivity state, ValidationErrorCollection validationErrors)
        {
            bool flag = false;
            foreach (Activity activity in state.Activities)
            {
                if ((activity.Enabled && !(activity is EventDrivenActivity)) && !(activity is StateActivity))
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                validationErrors.Add(new ValidationError(SR.GetError_InvalidCompositeStateChild(), 0x5f0));
            }
        }

        private static void ValidateCustomStateActivity(StateActivity state, ValidationErrorCollection validationErrors)
        {
            if (state.Activities.Count > 0)
            {
                validationErrors.Add(new ValidationError(SR.GetError_BlackBoxCustomStateNotSupported(), 0x623));
            }
        }

        private static void ValidateEventDrivenActivities(StateActivity state, ValidationErrorCollection validationErrors)
        {
            List<EventDrivenActivity> list = new List<EventDrivenActivity>();
            foreach (Activity activity in state.EnabledActivities)
            {
                EventDrivenActivity item = activity as EventDrivenActivity;
                if (item != null)
                {
                    list.Add(item);
                }
            }
            foreach (EventDrivenActivity activity3 in list)
            {
                if (!ValidateMultipleIEventActivity(activity3, validationErrors))
                {
                    break;
                }
            }
        }

        private static void ValidateInitialState(StateActivity state, ValidationErrorCollection validationErrors)
        {
            string initialStateName = StateMachineHelpers.GetInitialStateName(state);
            if (string.IsNullOrEmpty(initialStateName))
            {
                if (state.Activities.Count > 0)
                {
                    validationErrors.Add(new ValidationError(SR.GetString("Error_PropertyNotSet", new object[] { "InitialStateName" }), 0x116, false, "InitialStateName"));
                }
            }
            else
            {
                StateActivity activity = StateMachineHelpers.FindStateByName(state, initialStateName);
                if (activity == null)
                {
                    validationErrors.Add(new ValidationError(SR.GetError_InitialStateMustPointToAState(), 0x5f5, false, "InitialStateName"));
                }
                else
                {
                    if (!StateMachineHelpers.IsLeafState(activity))
                    {
                        validationErrors.Add(new ValidationError(SR.GetError_InitialStateMustPointToALeafNodeState(), 0x5f7, false, "InitialStateName"));
                    }
                    string completedStateName = StateMachineHelpers.GetCompletedStateName(state);
                    if (initialStateName == completedStateName)
                    {
                        validationErrors.Add(new ValidationError(SR.GetError_InitialStateMustBeDifferentThanCompletedState(), 0x62c, false, "InitialStateName"));
                    }
                }
            }
        }

        private static void ValidateLeafState(StateActivity state, ValidationErrorCollection validationErrors)
        {
            ValidateLeafStateChildren(state, validationErrors);
        }

        private static void ValidateLeafStateChildren(StateActivity state, ValidationErrorCollection validationErrors)
        {
            bool flag = false;
            foreach (Activity activity in state.Activities)
            {
                if ((activity.Enabled && !(activity is EventDrivenActivity)) && (!(activity is StateInitializationActivity) && !(activity is StateFinalizationActivity)))
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                validationErrors.Add(new ValidationError(SR.GetError_InvalidLeafStateChild(), 0x5f1));
            }
        }

        private static bool ValidateMultipleIEventActivity(EventDrivenActivity eventDriven, ValidationErrorCollection validationErrors)
        {
            IEventActivity firstEventActivity = null;
            if (eventDriven.EnabledActivities.Count > 0)
            {
                firstEventActivity = eventDriven.EnabledActivities[0] as IEventActivity;
            }
            return ValidateMultipleIEventActivityInCompositeActivity(eventDriven, firstEventActivity, eventDriven, validationErrors);
        }

        private static bool ValidateMultipleIEventActivityInCompositeActivity(EventDrivenActivity eventDriven, IEventActivity firstEventActivity, CompositeActivity parent, ValidationErrorCollection validationErrors)
        {
            foreach (Activity activity in parent.Activities)
            {
                if (activity.Enabled && (activity != firstEventActivity))
                {
                    if (activity is IEventActivity)
                    {
                        validationErrors.Add(new ValidationError(SR.GetString("Error_EventDrivenMultipleEventActivity", new object[] { eventDriven.Name, typeof(IEventActivity).FullName, typeof(EventDrivenActivity).Name }), 0x524));
                        return false;
                    }
                    CompositeActivity activity2 = activity as CompositeActivity;
                    if ((activity2 != null) && !ValidateMultipleIEventActivityInCompositeActivity(eventDriven, firstEventActivity, activity2, validationErrors))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static void ValidateRootState(StateActivity state, ValidationErrorCollection validationErrors)
        {
            ValidateCompositeStateChildren(state, validationErrors);
            if (StateMachineHelpers.IsStateMachine(state))
            {
                ValidateInitialState(state, validationErrors);
                ValidateCompletedState(state, validationErrors);
            }
        }

        private static void ValidateState(StateActivity state, ValidationErrorCollection validationErrors)
        {
            ValidateCompositeStateChildren(state, validationErrors);
        }
    }
}

