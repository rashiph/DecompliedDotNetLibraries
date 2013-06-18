namespace System.Workflow.ComponentModel
{
    using System;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class FaultHandlerActivityValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            FaultHandlerActivity activity = obj as FaultHandlerActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(FaultHandlerActivity).FullName }), "obj");
            }
            if (!(activity.Parent is FaultHandlersActivity))
            {
                errors.Add(new ValidationError(SR.GetString("Error_FaultHandlerActivityParentNotFaultHandlersActivity"), 0x519));
            }
            if (!(manager.GetService(typeof(ITypeProvider)) is ITypeProvider))
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
            }
            ValidationError item = null;
            if (activity.FaultType == null)
            {
                item = new ValidationError(SR.GetString("Error_TypePropertyInvalid", new object[] { "FaultType" }), 0x116) {
                    PropertyName = "FaultType"
                };
                errors.Add(item);
            }
            else if (!TypeProvider.IsAssignable(typeof(Exception), activity.FaultType))
            {
                item = new ValidationError(SR.GetString("Error_TypeTypeMismatch", new object[] { "FaultType", typeof(Exception).FullName }), 0x51a) {
                    PropertyName = "FaultType"
                };
                errors.Add(item);
            }
            if (activity.EnabledActivities.Count == 0)
            {
                errors.Add(new ValidationError(SR.GetString("Warning_EmptyBehaviourActivity", new object[] { typeof(FaultHandlerActivity).FullName, activity.QualifiedName }), 0x1a3, true));
            }
            if (activity.AlternateFlowActivities.Count > 0)
            {
                errors.Add(new ValidationError(SR.GetString("Error_ModelingConstructsCanNotContainModelingConstructs"), 0x61f));
            }
            return errors;
        }
    }
}

