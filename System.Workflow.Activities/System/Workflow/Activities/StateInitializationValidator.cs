namespace System.Workflow.Activities
{
    using System;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    [ComVisible(false)]
    internal sealed class StateInitializationValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);
            StateInitializationActivity stateInitialization = obj as StateInitializationActivity;
            if (stateInitialization == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(StateInitializationActivity).FullName }), "obj");
            }
            StateActivity parent = stateInitialization.Parent as StateActivity;
            if (parent == null)
            {
                validationErrors.Add(new ValidationError(SR.GetError_StateInitializationParentNotState(), 0x606));
                return validationErrors;
            }
            foreach (Activity activity3 in parent.EnabledActivities)
            {
                StateInitializationActivity activity4 = activity3 as StateInitializationActivity;
                if ((activity4 != null) && (activity4 != stateInitialization))
                {
                    validationErrors.Add(new ValidationError(SR.GetError_MultipleStateInitializationActivities(), 0x604));
                    break;
                }
            }
            this.ValidateSetStateInsideStateInitialization(stateInitialization, parent, validationErrors);
            if (StateMachineHelpers.ContainsEventActivity(stateInitialization))
            {
                validationErrors.Add(new ValidationError(SR.GetError_EventActivityNotValidInStateInitialization(), 0x603));
            }
            return validationErrors;
        }

        private void ValidateSetStateInsideStateInitialization(StateInitializationActivity stateInitialization, StateActivity state, ValidationErrorCollection validationErrors)
        {
            this.ValidateSetStateInsideStateInitializationCore(stateInitialization, state, validationErrors);
        }

        private void ValidateSetStateInsideStateInitializationCore(CompositeActivity compositeActivity, StateActivity state, ValidationErrorCollection validationErrors)
        {
            foreach (Activity activity in compositeActivity.EnabledActivities)
            {
                CompositeActivity activity2 = activity as CompositeActivity;
                if (activity2 != null)
                {
                    this.ValidateSetStateInsideStateInitializationCore(activity2, state, validationErrors);
                }
                else
                {
                    SetStateActivity activity3 = activity as SetStateActivity;
                    if (((activity3 != null) && !string.IsNullOrEmpty(activity3.TargetStateName)) && activity3.TargetStateName.Equals(state.QualifiedName))
                    {
                        validationErrors.Add(new ValidationError(SR.GetError_InvalidTargetStateInStateInitialization(), 0x605));
                        break;
                    }
                }
            }
        }
    }
}

