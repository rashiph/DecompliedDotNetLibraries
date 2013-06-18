namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Activities.Validation;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public sealed class PropertyValue<TOperand, TResult> : CodeActivity<TResult>
    {
        private Func<TOperand, TResult> operationFunction;

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            bool isRequired = false;
            if (typeof(TOperand).IsEnum)
            {
                metadata.AddValidationError(System.Activities.SR.TargetTypeCannotBeEnum(base.GetType().Name, base.DisplayName));
            }
            if (string.IsNullOrEmpty(this.PropertyName))
            {
                metadata.AddValidationError(System.Activities.SR.ActivityPropertyMustBeSet("PropertyName", base.DisplayName));
            }
            else
            {
                PropertyInfo property = null;
                property = typeof(TOperand).GetProperty(this.PropertyName);
                if (property == null)
                {
                    metadata.AddValidationError(System.Activities.SR.MemberNotFound(this.PropertyName, typeof(TOperand).Name));
                }
                else
                {
                    ValidationError error;
                    isRequired = !property.GetAccessors()[0].IsStatic;
                    if (!MemberExpressionHelper.TryGenerateLinqDelegate<TOperand, TResult>(this.PropertyName, false, property.GetAccessors()[0].IsStatic, out this.operationFunction, out error))
                    {
                        metadata.AddValidationError(error);
                    }
                    MethodInfo getMethod = property.GetGetMethod();
                    MethodInfo setMethod = property.GetSetMethod();
                    if (((getMethod != null) && !getMethod.IsStatic) || ((setMethod != null) && !setMethod.IsStatic))
                    {
                        isRequired = true;
                    }
                }
            }
            MemberExpressionHelper.AddOperandArgument<TOperand>(metadata, this.Operand, isRequired);
        }

        protected override TResult Execute(CodeActivityContext context)
        {
            TOperand arg = this.Operand.Get(context);
            return this.operationFunction(arg);
        }

        public InArgument<TOperand> Operand { get; set; }

        [DefaultValue((string) null)]
        public string PropertyName { get; set; }
    }
}

