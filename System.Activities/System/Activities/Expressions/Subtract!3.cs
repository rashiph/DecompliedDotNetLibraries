namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Activities.Validation;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public sealed class Subtract<TLeft, TRight, TResult> : CodeActivity<TResult>
    {
        private bool checkedOperation;
        private static Func<TLeft, TRight, TResult> checkedOperationFunction;
        private static Func<TLeft, TRight, TResult> uncheckedOperationFunction;

        public Subtract()
        {
            this.checkedOperation = true;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            BinaryExpressionHelper.OnGetArguments<TLeft, TRight>(metadata, this.Left, this.Right);
            if (this.checkedOperation)
            {
                this.EnsureOperationFunction(metadata, ref Subtract<TLeft, TRight, TResult>.checkedOperationFunction, ExpressionType.SubtractChecked);
            }
            else
            {
                this.EnsureOperationFunction(metadata, ref Subtract<TLeft, TRight, TResult>.uncheckedOperationFunction, ExpressionType.Subtract);
            }
        }

        private void EnsureOperationFunction(CodeActivityMetadata metadata, ref Func<TLeft, TRight, TResult> operationFunction, ExpressionType operatorType)
        {
            ValidationError error;
            if ((operationFunction == null) && !BinaryExpressionHelper.TryGenerateLinqDelegate<TLeft, TRight, TResult>(operatorType, out operationFunction, out error))
            {
                metadata.AddValidationError(error);
            }
        }

        protected override TResult Execute(CodeActivityContext context)
        {
            TLeft local = this.Left.Get(context);
            TRight local2 = this.Right.Get(context);
            if (this.checkedOperation)
            {
                return Subtract<TLeft, TRight, TResult>.checkedOperationFunction(local, local2);
            }
            return Subtract<TLeft, TRight, TResult>.uncheckedOperationFunction(local, local2);
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
        public InArgument<TLeft> Left { get; set; }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument<TRight> Right { get; set; }
    }
}

