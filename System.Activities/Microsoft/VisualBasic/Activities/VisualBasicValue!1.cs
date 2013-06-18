namespace Microsoft.VisualBasic.Activities
{
    using System;
    using System.Activities;
    using System.Activities.ExpressionParser;
    using System.Activities.XamlIntegration;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Markup;

    public sealed class VisualBasicValue<TResult> : CodeActivity<TResult>, IValueSerializableExpression, IExpressionContainer, IVisualBasicExpression
    {
        private Func<ActivityContext, TResult> compiledExpression;
        private Expression<Func<ActivityContext, TResult>> expressionTree;

        public VisualBasicValue()
        {
            base.SkipArgumentResolution = true;
        }

        public VisualBasicValue(string expressionText) : this()
        {
            this.ExpressionText = expressionText;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            this.expressionTree = null;
            try
            {
                this.expressionTree = VisualBasicHelper.Compile<TResult>(this.ExpressionText, metadata);
            }
            catch (SourceExpressionException exception)
            {
                metadata.AddValidationError(exception.Message);
            }
        }

        public bool CanConvertToString(IValueSerializerContext context)
        {
            return true;
        }

        public string ConvertToString(IValueSerializerContext context)
        {
            return ("[" + this.ExpressionText + "]");
        }

        protected override TResult Execute(CodeActivityContext context)
        {
            if (this.expressionTree != null)
            {
                return this.GetValueCore(context);
            }
            return default(TResult);
        }

        private TResult GetValueCore(ActivityContext context)
        {
            if (this.compiledExpression == null)
            {
                if (this.expressionTree == null)
                {
                    return default(TResult);
                }
                this.compiledExpression = this.expressionTree.Compile();
            }
            return this.compiledExpression(context);
        }

        internal override bool TryGetValue(ActivityContext context, out TResult value)
        {
            if (!base.SkipArgumentResolution && (base.RuntimeArguments.Count > 1))
            {
                value = default(TResult);
                return false;
            }
            value = this.GetValueCore(context);
            return true;
        }

        public string ExpressionText { get; set; }

        Expression IExpressionContainer.Expression
        {
            get
            {
                return this.expressionTree;
            }
        }
    }
}

