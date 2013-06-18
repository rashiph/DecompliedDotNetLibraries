namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Activities.Validation;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public sealed class Cast<TOperand, TResult> : CodeActivity<TResult>
    {
        private bool checkedOperation;
        private static Func<TOperand, TResult> checkedOperationFunction;
        private static Func<TOperand, TResult> uncheckedOperationFunction;

        public Cast()
        {
            this.checkedOperation = true;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            UnaryExpressionHelper.OnGetArguments<TOperand>(metadata, this.Operand);
            if (this.checkedOperation)
            {
                this.EnsureOperationFunction(metadata, ref Cast<TOperand, TResult>.checkedOperationFunction, ExpressionType.ConvertChecked);
            }
            else
            {
                this.EnsureOperationFunction(metadata, ref Cast<TOperand, TResult>.uncheckedOperationFunction, ExpressionType.Convert);
            }
        }

        private void EnsureOperationFunction(CodeActivityMetadata metadata, ref Func<TOperand, TResult> operationFunction, ExpressionType operatorType)
        {
            ValidationError error;
            if ((operationFunction == null) && !UnaryExpressionHelper.TryGenerateLinqDelegate<TOperand, TResult>(operatorType, out operationFunction, out error))
            {
                metadata.AddValidationError(error);
            }
        }

        protected override TResult Execute(CodeActivityContext context)
        {
            TOperand arg = this.Operand.Get(context);
            if (this.checkedOperation)
            {
                return Cast<TOperand, TResult>.checkedOperationFunction(arg);
            }
            return Cast<TOperand, TResult>.uncheckedOperationFunction(arg);
        }

        [DefaultValue(true)]
        public bool Checked
        {
            get
            {
                return this.checkedOperation;
            }
            set
            {
                this.checkedOperation = value;
            }
        }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument<TOperand> Operand { get; set; }
    }
}

