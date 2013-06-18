namespace System.Workflow.Activities
{
    using System;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    [ComVisible(false)]
    internal sealed class SetStateValidator : ActivityValidator
    {
        internal static bool IsValidContainer(CompositeActivity activity)
        {
            return ((activity is EventDrivenActivity) || (activity is StateInitializationActivity));
        }

        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection(base.Validate(manager, obj));
            SetStateActivity setState = obj as SetStateActivity;
            if (setState == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(StateActivity).FullName }), "obj");
            }
            if (SetStateContainment.Validate(setState, validationErrors))
            {
                if (string.IsNullOrEmpty(setState.TargetStateName))
                {
                    validationErrors.Add(new ValidationError(SR.GetString("Error_PropertyNotSet", new object[] { "TargetStateName" }), 0x116, false, "TargetStateName"));
                    return validationErrors;
                }
                StateActivity state = StateMachineHelpers.FindStateByName(StateMachineHelpers.GetRootState(StateMachineHelpers.FindEnclosingState(setState)), setState.TargetStateName);
                if (state == null)
                {
                    validationErrors.Add(new ValidationError(SR.GetError_SetStateMustPointToAState(), 0x5f3, false, "TargetStateName"));
                    return validationErrors;
                }
                if (!StateMachineHelpers.IsLeafState(state))
                {
                    validationErrors.Add(new ValidationError(SR.GetError_SetStateMustPointToALeafNodeState(), 0x5f4, false, "TargetStateName"));
                }
            }
            return validationErrors;
        }

        private class SetStateContainment
        {
            private bool validParentFound = true;
            private bool validParentStateFound;

            private SetStateContainment()
            {
            }

            public static bool Validate(SetStateActivity setState, ValidationErrorCollection validationErrors)
            {
                SetStateValidator.SetStateContainment containment = new SetStateValidator.SetStateContainment();
                ValidateContainment(containment, setState);
                if (containment.validParentFound && containment.validParentStateFound)
                {
                    return true;
                }
                validationErrors.Add(new ValidationError(SR.GetError_SetStateOnlyWorksOnStateMachineWorkflow(), 0x5f2));
                return false;
            }

            private static void ValidateContainment(SetStateValidator.SetStateContainment containment, Activity activity)
            {
                if ((activity.Parent == null) || (activity.Parent == activity))
                {
                    containment.validParentFound = false;
                }
                else if (SetStateValidator.IsValidContainer(activity.Parent))
                {
                    ValidateParentState(containment, activity.Parent);
                }
                else
                {
                    ValidateContainment(containment, activity.Parent);
                }
            }

            private static void ValidateParentState(SetStateValidator.SetStateContainment containment, CompositeActivity activity)
            {
                if (activity.Parent != null)
                {
                    if (activity.Parent is StateActivity)
                    {
                        containment.validParentStateFound = true;
                    }
                    else
                    {
                        ValidateParentState(containment, activity.Parent);
                    }
                }
            }
        }
    }
}

