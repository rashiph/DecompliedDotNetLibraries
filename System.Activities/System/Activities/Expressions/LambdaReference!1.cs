namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Activities.XamlIntegration;
    using System.Linq.Expressions;
    using System.Runtime.InteropServices;
    using System.Windows.Markup;

    public sealed class LambdaReference<T> : CodeActivity<Location<T>>, IExpressionContainer, IValueSerializableExpression
    {
        private Expression<Func<ActivityContext, T>> locationExpression;
        private LocationFactory<T> locationFactory;
        private Expression<Func<ActivityContext, T>> rewrittenTree;

        public LambdaReference(Expression<Func<ActivityContext, T>> locationExpression)
        {
            this.locationExpression = locationExpression;
            base.SkipArgumentResolution = true;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            Expression expression;
            if (ExpressionUtilities.TryRewriteLambdaExpression(this.locationExpression, out expression, metadata))
            {
                this.rewrittenTree = (Expression<Func<ActivityContext, T>>) expression;
            }
            else
            {
                this.rewrittenTree = this.locationExpression;
            }
            string extraErrorMessage = null;
            if (!ExpressionUtilities.IsLocation(this.rewrittenTree, typeof(T), out extraErrorMessage))
            {
                string invalidLValueExpression = System.Activities.SR.InvalidLValueExpression;
                if (extraErrorMessage != null)
                {
                    invalidLValueExpression = invalidLValueExpression + ":" + extraErrorMessage;
                }
                metadata.AddValidationError(invalidLValueExpression);
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

        protected override Location<T> Execute(CodeActivityContext context)
        {
            return base.ExecuteWithTryGetValue(context);
        }

        internal override bool TryGetValue(ActivityContext context, out Location<T> value)
        {
            if (this.locationFactory == null)
            {
                this.locationFactory = ExpressionUtilities.CreateLocationFactory<T>(this.rewrittenTree);
            }
            value = this.locationFactory.CreateLocation(context);
            return true;
        }

        Expression IExpressionContainer.Expression
        {
            get
            {
                return this.locationExpression;
            }
        }
    }
}

