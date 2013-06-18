namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Specialized;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class CompensateValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            CompensateActivity compensate = obj as CompensateActivity;
            if (compensate == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(CompensateActivity).FullName }), "obj");
            }
            CompositeActivity parent = compensate.Parent;
            while (parent != null)
            {
                if (((parent is CompensationHandlerActivity) || (parent is FaultHandlerActivity)) || (parent is CancellationHandlerActivity))
                {
                    break;
                }
                parent = parent.Parent;
            }
            if (parent == null)
            {
                errors.Add(new ValidationError(SR.GetString("Error_CompensateBadNesting"), 0x509));
            }
            ValidationError item = null;
            StringCollection compensatableTargets = CompensateActivity.GetCompensatableTargets(compensate);
            if (string.IsNullOrEmpty(compensate.TargetActivityName))
            {
                item = ValidationError.GetNotSetValidationError("TargetActivityName");
            }
            else if (!compensatableTargets.Contains(compensate.TargetActivityName))
            {
                item = new ValidationError(SR.GetString("Error_CompensateBadTargetTX", new object[] { "TargetActivityName", compensate.TargetActivityName, compensate.QualifiedName }), 0x563, false, "TargetActivityName");
            }
            if (item != null)
            {
                errors.Add(item);
            }
            return errors;
        }
    }
}

