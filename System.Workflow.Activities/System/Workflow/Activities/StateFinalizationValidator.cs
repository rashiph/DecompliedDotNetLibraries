namespace System.Workflow.Activities
{
    using System;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    [ComVisible(false)]
    internal sealed class StateFinalizationValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            StateFinalizationActivity compositeActivity = obj as StateFinalizationActivity;
            if (compositeActivity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(StateFinalizationActivity).FullName }), "obj");
            }
            StateActivity parent = compositeActivity.Parent as StateActivity;
            if (parent == null)
            {
                errors.Add(new ValidationError(SR.GetError_StateFinalizationParentNotState(), 0x606));
                return errors;
            }
            foreach (Activity activity3 in parent.EnabledActivities)
            {
                StateFinalizationActivity activity4 = activity3 as StateFinalizationActivity;
                if ((activity4 != null) && (activity4 != compositeActivity))
                {
                    errors.Add(new ValidationError(SR.GetError_MultipleStateFinalizationActivities(), 0x61a));
                    break;
                }
            }
            if (StateMachineHelpers.ContainsEventActivity(compositeActivity))
            {
                errors.Add(new ValidationError(SR.GetError_EventActivityNotValidInStateFinalization(), 0x603));
            }
            return errors;
        }
    }
}

