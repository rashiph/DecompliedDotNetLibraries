namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Activities.Validation;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public sealed class Not<TOperand, TResult> : CodeActivity<TResult>
    {
        private static Func<TOperand, TResult> operationFunction;

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            ValidationError error;
            UnaryExpressionHelper.OnGetArguments<TOperand>(metadata, this.Operand);
            if ((Not<TOperand, TResult>.operationFunction == null) && !UnaryExpressionHelper.TryGenerateLinqDelegate<TOperand, TResult>(ExpressionType.Not, out Not<TOperand, TResult>.operationFunction, out error))
            {
                metadata.AddValidationError(error);
            }
        }

        protected override TResult Execute(CodeActivityContext context)
        {
            TOperand arg = this.Operand.Get(context);
            return Not<TOperand, TResult>.operationFunction(arg);
        }

        [DefaultValue((string) null), RequiredArgument]
        public InArgument<TOperand> Operand { get; set; }
    }
}

