namespace System.Linq.Expressions
{
    using System;
    using System.Diagnostics;

    [DebuggerTypeProxy(typeof(Expression.LabelExpressionProxy))]
    public sealed class LabelExpression : Expression
    {
        private readonly Expression _defaultValue;
        private readonly LabelTarget _target;

        internal LabelExpression(LabelTarget label, Expression defaultValue)
        {
            this._target = label;
            this._defaultValue = defaultValue;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitLabel(this);
        }

        public LabelExpression Update(LabelTarget target, Expression defaultValue)
        {
            if ((target == this.Target) && (defaultValue == this.DefaultValue))
            {
                return this;
            }
            return Expression.Label(target, defaultValue);
        }

        public Expression DefaultValue
        {
            get
            {
                return this._defaultValue;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Label;
            }
        }

        public LabelTarget Target
        {
            get
            {
                return this._target;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                return this._target.Type;
            }
        }
    }
}

