namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Activities.XamlIntegration;
    using System.Linq.Expressions;
    using System.Runtime.InteropServices;
    using System.Windows.Markup;

    public sealed class LambdaValue<TResult> : CodeActivity<TResult>, IExpressionContainer, IValueSerializableExpression
    {
        private Func<ActivityContext, TResult> compiledLambdaValue;
        private Expression<Func<ActivityContext, TResult>> lambdaValue;
        private Expression<Func<ActivityContext, TResult>> rewrittenTree;

        public LambdaValue(Expression<Func<ActivityContext, TResult>> lambdaValue)
        {
            this.lambdaValue = lambdaValue;
            base.SkipArgumentResolution = true;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            Expression expression;
            if (ExpressionUtilities.TryRewriteLambdaExpression(this.lambdaValue, out expression, metadata))
            {
                this.rewrittenTree = (Expression<Func<ActivityContext, TResult>>) expression;
            }
            else
            {
                this.rewrittenTree = this.lambdaValue;
            }
        }

        public bool CanConvertToString(IValueSerializerContext context)
        {
            return true;
        }

        public string ConvertToString(IValueSerializerContext context)
        {
            throw FxTrace.Exception.AsError(new LambdaSerializationException());
        }

        protected override TResult Execute(CodeActivityContext context)
        {
            return base.ExecuteWithTryGetValue(context);
        }

        internal override bool TryGetValue(ActivityContext context, out TResult value)
        {
            if (this.compiledLambdaValue == null)
            {
                this.compiledLambdaValue = this.rewrittenTree.Compile();
            }
            value = this.compiledLambdaValue(context);
            return true;
        }

        Expression IExpressionContainer.Expression
        {
            get
            {
                return this.lambdaValue;
            }
        }
    }
}

