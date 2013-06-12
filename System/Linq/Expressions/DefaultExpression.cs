namespace System.Linq.Expressions
{
    using System;
    using System.Diagnostics;

    [DebuggerTypeProxy(typeof(Expression.DefaultExpressionProxy))]
    public sealed class DefaultExpression : Expression
    {
        private readonly System.Type _type;

        internal DefaultExpression(System.Type type)
        {
            this._type = type;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitDefault(this);
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Default;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

