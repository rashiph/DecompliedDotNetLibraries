namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Linq.Expressions;
    using System.Runtime.InteropServices;

    internal sealed class LocationReferenceValue<T> : CodeActivity<T>, IExpressionContainer
    {
        private System.Activities.LocationReference locationReference;

        public LocationReferenceValue(System.Activities.LocationReference locationReference)
        {
            this.locationReference = locationReference;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
        }

        protected override T Execute(CodeActivityContext context)
        {
            return base.ExecuteWithTryGetValue(context);
        }

        internal override bool TryGetValue(ActivityContext context, out T value)
        {
            try
            {
                context.AllowChainedEnvironmentAccess = true;
                value = context.GetValue<T>(this.locationReference);
            }
            finally
            {
                context.AllowChainedEnvironmentAccess = false;
            }
            return true;
        }

        public System.Activities.LocationReference LocationReference
        {
            get
            {
                return this.locationReference;
            }
        }

        Expression IExpressionContainer.Expression
        {
            get
            {
                return ExpressionUtilities.CreateIdentifierExpression(this.locationReference);
            }
        }
    }
}

