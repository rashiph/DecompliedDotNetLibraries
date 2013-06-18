namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Markup;

    [ContentProperty("DelegateArgument")]
    public sealed class DelegateArgumentReference<T> : CodeActivity<Location<T>>, IExpressionContainer
    {
        public DelegateArgumentReference()
        {
        }

        public DelegateArgumentReference(System.Activities.DelegateArgument delegateArgument) : this()
        {
            this.DelegateArgument = delegateArgument;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (this.DelegateArgument == null)
            {
                metadata.AddValidationError(System.Activities.SR.DelegateArgumentMustBeSet);
            }
            else
            {
                if (!this.DelegateArgument.IsInTree)
                {
                    metadata.AddValidationError(System.Activities.SR.DelegateArgumentMustBeReferenced(this.DelegateArgument.Name));
                }
                if (!metadata.Environment.IsVisible(this.DelegateArgument))
                {
                    metadata.AddValidationError(System.Activities.SR.DelegateArgumentNotVisible(this.DelegateArgument.Name));
                }
                if (!(this.DelegateArgument is DelegateOutArgument<T>) && !(this.DelegateArgument is DelegateInArgument<T>))
                {
                    metadata.AddValidationError(System.Activities.SR.DelegateArgumentTypeInvalid(this.DelegateArgument, typeof(T), this.DelegateArgument.Type));
                }
            }
        }

        protected override Location<T> Execute(CodeActivityContext context)
        {
            return base.ExecuteWithTryGetValue(context);
        }

        internal override bool TryGetValue(ActivityContext context, out Location<T> value)
        {
            try
            {
                context.AllowChainedEnvironmentAccess = true;
                value = context.GetLocation<T>(this.DelegateArgument);
            }
            finally
            {
                context.AllowChainedEnvironmentAccess = false;
            }
            return true;
        }

        public System.Activities.DelegateArgument DelegateArgument { get; set; }

        Expression IExpressionContainer.Expression
        {
            get
            {
                return ExpressionUtilities.CreateIdentifierExpression(this.DelegateArgument);
            }
        }
    }
}

