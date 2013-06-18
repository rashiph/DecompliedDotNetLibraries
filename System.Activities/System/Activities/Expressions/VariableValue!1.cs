namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public sealed class VariableValue<T> : CodeActivity<T>, IExpressionContainer
    {
        public VariableValue()
        {
        }

        public VariableValue(System.Activities.Variable variable)
        {
            this.Variable = variable;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (this.Variable == null)
            {
                metadata.AddValidationError(System.Activities.SR.VariableMustBeSet);
            }
            else
            {
                if (!(this.Variable is Variable<T>) && !TypeHelper.AreTypesCompatible(this.Variable.Type, typeof(T)))
                {
                    metadata.AddValidationError(System.Activities.SR.VariableTypeInvalid(this.Variable, typeof(T), this.Variable.Type));
                }
                if (!this.Variable.IsInTree)
                {
                    metadata.AddValidationError(System.Activities.SR.VariableShouldBeOpen(this.Variable.Name));
                }
                if (!metadata.Environment.IsVisible(this.Variable))
                {
                    metadata.AddValidationError(System.Activities.SR.VariableNotVisible(this.Variable.Name));
                }
            }
        }

        protected override T Execute(CodeActivityContext context)
        {
            return base.ExecuteWithTryGetValue(context);
        }

        public override string ToString()
        {
            if ((this.Variable != null) && !string.IsNullOrEmpty(this.Variable.Name))
            {
                return this.Variable.Name;
            }
            return base.ToString();
        }

        internal override bool TryGetValue(ActivityContext context, out T value)
        {
            try
            {
                context.AllowChainedEnvironmentAccess = true;
                value = context.GetValue<T>((LocationReference) this.Variable);
            }
            finally
            {
                context.AllowChainedEnvironmentAccess = false;
            }
            return true;
        }

        Expression IExpressionContainer.Expression
        {
            get
            {
                return ExpressionUtilities.CreateIdentifierExpression(this.Variable);
            }
        }

        public System.Activities.Variable Variable { get; set; }
    }
}

