namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Markup;

    [ContentProperty("DelegateArgument")]
    public sealed class DelegateArgumentValue<T> : CodeActivity<T>, IExpressionContainer
    {
        public DelegateArgumentValue()
        {
        }

        public DelegateArgumentValue(System.Activities.DelegateArgument delegateArgument) : this()
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
                if (!(this.DelegateArgument is DelegateInArgument<T>) && !TypeHelper.AreTypesCompatible(this.DelegateArgument.Type, typeof(T)))
                {
                    metadata.AddValidationError(System.Activities.SR.DelegateArgumentTypeInvalid(this.DelegateArgument, typeof(T), this.DelegateArgument.Type));
                }
            }
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
                value = context.GetValue<T>((LocationReference) this.DelegateArgument);
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

