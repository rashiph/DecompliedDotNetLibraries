namespace System.Activities
{
    using System;

    internal class ActivityWithResultWrapper<T> : CodeActivity<T>, Argument.IExpressionWrapper
    {
        private ActivityWithResult expression;

        public ActivityWithResultWrapper(ActivityWithResult expression)
        {
            this.expression = expression;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
        }

        protected override T Execute(CodeActivityContext context)
        {
            return default(T);
        }

        ActivityWithResult Argument.IExpressionWrapper.InnerExpression
        {
            get
            {
                return this.expression;
            }
        }
    }
}

