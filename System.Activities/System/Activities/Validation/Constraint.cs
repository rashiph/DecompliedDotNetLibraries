namespace System.Activities.Validation
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public abstract class Constraint : NativeActivity
    {
        private RuntimeArgument toValidate = new RuntimeArgument("ToValidate", typeof(object), ArgumentDirection.In);
        internal const string ToValidateArgumentName = "ToValidate";
        private RuntimeArgument toValidateContext = new RuntimeArgument("ToValidateContext", typeof(ValidationContext), ArgumentDirection.In);
        internal const string ToValidateContextArgumentName = "ToValidateContext";
        internal const string ValidationErrorListArgumentName = "ViolationList";
        public const string ValidationErrorListPropertyName = "System.Activities.Validation.Constraint.ValidationErrorList";
        private RuntimeArgument violationList = new RuntimeArgument("ViolationList", typeof(IList<ValidationError>), ArgumentDirection.Out);

        internal Constraint()
        {
        }

        public static void AddValidationError(NativeActivityContext context, ValidationError error)
        {
            List<ValidationError> list = context.Properties.Find("System.Activities.Validation.Constraint.ValidationErrorList") as List<ValidationError>;
            if (list == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.AddValidationErrorMustBeCalledFromConstraint(typeof(Constraint).Name)));
            }
            list.Add(error);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { this.toValidate, this.violationList, this.toValidateContext });
        }

        protected override void Execute(NativeActivityContext context)
        {
            object objectToValidate = this.toValidate.Get<object>(context);
            ValidationContext objectToValidateContext = this.toValidateContext.Get<ValidationContext>(context);
            if (objectToValidate == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotValidateNullObject(typeof(Constraint).Name, base.DisplayName)));
            }
            if (objectToValidateContext == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ValidationContextCannotBeNull(typeof(Constraint).Name, base.DisplayName)));
            }
            List<ValidationError> property = new List<ValidationError>(1);
            context.Properties.Add("System.Activities.Validation.Constraint.ValidationErrorList", property);
            this.violationList.Set(context, property);
            this.OnExecute(context, objectToValidate, objectToValidateContext);
        }

        protected abstract void OnExecute(NativeActivityContext context, object objectToValidate, ValidationContext objectToValidateContext);
    }
}

