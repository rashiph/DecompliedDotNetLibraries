namespace System.Workflow.Activities
{
    using System;
    using System.Reflection;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class InvokeWorkflowValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            System.Workflow.Activities.Common.WalkerEventHandler handler = null;
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);
            InvokeWorkflowActivity activity = obj as InvokeWorkflowActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(InvokeWorkflowActivity).FullName }), "obj");
            }
            if (activity.TargetWorkflow == null)
            {
                ValidationError error = new ValidationError(SR.GetString("Error_TypePropertyInvalid", new object[] { "TargetWorkflow" }), 0x116) {
                    PropertyName = "TargetWorkflow"
                };
                validationErrors.Add(error);
                return validationErrors;
            }
            ITypeProvider service = (ITypeProvider) manager.GetService(typeof(ITypeProvider));
            if (service == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
            }
            Type targetWorkflow = activity.TargetWorkflow;
            if ((targetWorkflow.Assembly == null) && (service.LocalAssembly != null))
            {
                Type type = service.LocalAssembly.GetType(targetWorkflow.FullName);
                if (type != null)
                {
                    targetWorkflow = type;
                }
            }
            if (!TypeProvider.IsAssignable(typeof(Activity), targetWorkflow))
            {
                ValidationError error2 = new ValidationError(SR.GetString("Error_TypeIsNotRootActivity", new object[] { "TargetWorkflow" }), 0x60e) {
                    PropertyName = "TargetWorkflow"
                };
                validationErrors.Add(error2);
                return validationErrors;
            }
            Activity seedActivity = null;
            try
            {
                seedActivity = Activator.CreateInstance(targetWorkflow) as Activity;
            }
            catch (Exception)
            {
            }
            if (seedActivity == null)
            {
                ValidationError error3 = new ValidationError(SR.GetString("Error_GetCalleeWorkflow", new object[] { activity.TargetWorkflow }), 0x500) {
                    PropertyName = "TargetWorkflow"
                };
                validationErrors.Add(error3);
                return validationErrors;
            }
            System.Workflow.Activities.Common.Walker walker = new System.Workflow.Activities.Common.Walker();
            if (handler == null)
            {
                handler = delegate (System.Workflow.Activities.Common.Walker w, System.Workflow.Activities.Common.WalkerEventArgs args) {
                    if ((args.CurrentActivity is WebServiceInputActivity) && ((WebServiceInputActivity) args.CurrentActivity).IsActivating)
                    {
                        ValidationError item = new ValidationError(SR.GetString("Error_ExecWithActivationReceive"), 0x614) {
                            PropertyName = "Name"
                        };
                        validationErrors.Add(item);
                        args.Action = System.Workflow.Activities.Common.WalkerAction.Abort;
                    }
                };
            }
            walker.FoundActivity += handler;
            walker.Walk(seedActivity);
            bool flag = false;
            for (Activity activity3 = activity.Parent; activity3 != null; activity3 = activity3.Parent)
            {
                if ((activity3 is CompensatableTransactionScopeActivity) || (activity3 is TransactionScopeActivity))
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                ValidationError error4 = new ValidationError(SR.GetString("Error_ExecInAtomicScope"), 0x502);
                validationErrors.Add(error4);
            }
            foreach (WorkflowParameterBinding binding in activity.ParameterBindings)
            {
                PropertyInfo property = null;
                property = targetWorkflow.GetProperty(binding.ParameterName);
                if (property == null)
                {
                    ValidationError error5 = new ValidationError(SR.GetString("Error_ParameterNotFound", new object[] { binding.ParameterName }), 0x504);
                    if (InvokeWorkflowActivity.ReservedParameterNames.Contains(binding.ParameterName))
                    {
                        error5.PropertyName = ParameterInfoBasedPropertyDescriptor.GetParameterPropertyName(activity.GetType(), binding.ParameterName);
                    }
                    validationErrors.Add(error5);
                }
                else
                {
                    Type propertyType = property.PropertyType;
                    if (binding.GetBinding(WorkflowParameterBinding.ValueProperty) != null)
                    {
                        ValidationErrorCollection errors = System.Workflow.Activities.Common.ValidationHelpers.ValidateProperty(manager, activity, binding.GetBinding(WorkflowParameterBinding.ValueProperty), new PropertyValidationContext(binding, null, binding.ParameterName), new BindValidationContext(propertyType, AccessTypes.Read));
                        if (errors.Count != 0)
                        {
                            validationErrors.AddRange(errors);
                        }
                    }
                }
            }
            return validationErrors;
        }
    }
}

