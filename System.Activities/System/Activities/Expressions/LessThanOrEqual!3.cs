namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Activities.Validation;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public sealed class LessThanOrEqual<TLeft, TRight, TResult> : CodeActivity<TResult>
    {
        private static Func<TLeft, TRight, TResult> operationFunction;

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            ValidationError error;
            BinaryExpressionHelper.OnGetArguments<TLeft, TRight>(metadata, this.Left, this.Right);
            if ((LessThanOrEqual<TLeft, TRight, TResult>.operationFunction == null) && !BinaryExpressionHelper.TryGenerateLinqDelegate<TLeft, TRight, TResult>(ExpressionType.LessThanOrEqual, out LessThanOrEqual<TLeft, TRight, TResult>.operationFunction, out error))
            {
                metadata.AddValidationError(error);
            }
        }

        protected override TResult Execute(CodeActivityContext context)
        {
            TLeft local = this.Left.Get(context);
            TRight local2 = this.Right.Get(context);
            return LessThanOrEqual<TLeft, TRight, TResult>.operationFunction(local, local2);
        }

        [DefaultValue((string) null), RequiredArgument]
        public InArgument<TLeft> Left { get; set; }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument<TRight> Right { get; set; }
    }
}

