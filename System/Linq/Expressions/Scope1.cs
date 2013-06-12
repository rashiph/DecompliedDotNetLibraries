namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal sealed class Scope1 : ScopeExpression
    {
        private object _body;

        internal Scope1(IList<ParameterExpression> variables, Expression body) : base(variables)
        {
            this._body = body;
        }

        internal override Expression GetExpression(int index)
        {
            if (index != 0)
            {
                throw new InvalidOperationException();
            }
            return Expression.ReturnObject<Expression>(this._body);
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeExpressions()
        {
            return BlockExpression.ReturnReadOnlyExpressions(this, ref this._body);
        }

        internal override BlockExpression Rewrite(ReadOnlyCollection<ParameterExpression> variables, Expression[] args)
        {
            return new Scope1(base.ReuseOrValidateVariables(variables), args[0]);
        }

        internal override int ExpressionCount
        {
            get
            {
                return 1;
            }
        }
    }
}

