namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Activities.Validation;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public sealed class FieldValue<TOperand, TResult> : CodeActivity<TResult>
    {
        private Func<TOperand, TResult> operationFunction;

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            bool isRequired = false;
            if (typeof(TOperand).IsEnum)
            {
                metadata.AddValidationError(System.Activities.SR.TargetTypeCannotBeEnum(base.GetType().Name, base.DisplayName));
            }
            if (string.IsNullOrEmpty(this.FieldName))
            {
                metadata.AddValidationError(System.Activities.SR.ActivityPropertyMustBeSet("FieldName", base.DisplayName));
            }
            else
            {
                FieldInfo field = null;
                field = typeof(TOperand).GetField(this.FieldName);
                if (field == null)
                {
                    metadata.AddValidationError(System.Activities.SR.MemberNotFound(this.FieldName, typeof(TOperand).Name));
                }
                else
                {
                    ValidationError error;
                    isRequired = !field.IsStatic;
                    if (!MemberExpressionHelper.TryGenerateLinqDelegate<TOperand, TResult>(this.FieldName, true, field.IsStatic, out this.operationFunction, out error))
                    {
                        metadata.AddValidationError(error);
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

        [DefaultValue((string) null)]
        public string FieldName { get; set; }

        [DefaultValue((string) null)]
        public InArgument<TOperand> Operand { get; set; }
    }
}

