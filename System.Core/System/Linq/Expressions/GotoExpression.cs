namespace System.Linq.Expressions
{
    using System;
    using System.Diagnostics;

    [DebuggerTypeProxy(typeof(Expression.GotoExpressionProxy))]
    public sealed class GotoExpression : Expression
    {
        private readonly GotoExpressionKind _kind;
        private readonly LabelTarget _target;
        private readonly System.Type _type;
        private readonly Expression _value;

        internal GotoExpression(GotoExpressionKind kind, LabelTarget target, Expression value, System.Type type)
        {
            this._kind = kind;
            this._value = value;
            this._target = target;
            this._type = type;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitGoto(this);
        }

        public GotoExpression Update(LabelTarget target, Expression value)
        {
            if ((target == this.Target) && (value == this.Value))
            {
                return this;
            }
            return Expression.MakeGoto(this.Kind, target, value, this.Type);
        }

        public GotoExpressionKind Kind
        {
            get
            {
                return this._kind;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Goto;
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
                return this._type;
            }
        }

        public Expression Value
        {
            get
            {
                return this._value;
            }
        }
    }
}

