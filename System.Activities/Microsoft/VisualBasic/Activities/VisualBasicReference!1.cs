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

    public sealed class VisualBasicReference<TResult> : CodeActivity<Location<TResult>>, IValueSerializableExpression, IExpressionContainer, IVisualBasicExpression
    {
        private Expression<Func<ActivityContext, TResult>> expressionTree;
        private LocationFactory<TResult> locationFactory;

        public VisualBasicReference()
        {
            base.SkipArgumentResolution = true;
        }

        public VisualBasicReference(string expressionText) : this()
        {
            this.ExpressionText = expressionText;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            this.expressionTree = null;
            try
            {
                this.expressionTree = VisualBasicHelper.Compile<TResult>(this.ExpressionText, metadata);
                string extraErrorMessage = null;
                if (!metadata.HasViolations && ((this.expressionTree == null) || !ExpressionUtilities.IsLocation(this.expressionTree, typeof(TResult), out extraErrorMessage)))
                {
                    string invalidLValueExpression = System.Activities.SR.InvalidLValueExpression;
                    if (extraErrorMessage != null)
                    {
                        invalidLValueExpression = invalidLValueExpression + ":" + extraErrorMessage;
                    }
                    this.expressionTree = null;
                    metadata.AddValidationError(System.Activities.SR.CompilerErrorSpecificExpression(this.ExpressionText, invalidLValueExpression));
                }
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

        protected override Location<TResult> Execute(CodeActivityContext context)
        {
            if (this.expressionTree != null)
            {
                return this.GetValueCore(context);
            }
            return null;
        }

        private Location<TResult> GetValueCore(ActivityContext context)
        {
            if (this.locationFactory == null)
            {
                this.locationFactory = ExpressionUtilities.CreateLocationFactory<TResult>(this.expressionTree);
            }
            return this.locationFactory.CreateLocation(context);
        }

        internal override bool TryGetValue(ActivityContext context, out Location<TResult> value)
        {
            if (!base.SkipArgumentResolution && (base.RuntimeArguments.Count > 1))
            {
                value = null;
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

