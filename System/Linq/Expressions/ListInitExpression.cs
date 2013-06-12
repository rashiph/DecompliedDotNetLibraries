namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    [DebuggerTypeProxy(typeof(Expression.ListInitExpressionProxy))]
    public sealed class ListInitExpression : Expression
    {
        private readonly ReadOnlyCollection<ElementInit> _initializers;
        private readonly System.Linq.Expressions.NewExpression _newExpression;

        internal ListInitExpression(System.Linq.Expressions.NewExpression newExpression, ReadOnlyCollection<ElementInit> initializers)
        {
            this._newExpression = newExpression;
            this._initializers = initializers;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitListInit(this);
        }

        public override Expression Reduce()
        {
            return MemberInitExpression.ReduceListInit(this._newExpression, this._initializers, true);
        }

        public ListInitExpression Update(System.Linq.Expressions.NewExpression newExpression, IEnumerable<ElementInit> initializers)
        {
            if ((newExpression == this.NewExpression) && (initializers == this.Initializers))
            {
                return this;
            }
            return Expression.ListInit(newExpression, initializers);
        }

        public override bool CanReduce
        {
            get
            {
                return true;
            }
        }

        public ReadOnlyCollection<ElementInit> Initializers
        {
            get
            {
                return this._initializers;
            }
        }

        public System.Linq.Expressions.NewExpression NewExpression
        {
            get
            {
                return this._newExpression;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.ListInit;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                return this._newExpression.Type;
            }
        }
    }
}

