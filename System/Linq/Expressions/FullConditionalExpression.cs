namespace System.Linq.Expressions
{
    using System;

    internal class FullConditionalExpression : ConditionalExpression
    {
        private readonly Expression _false;

        internal FullConditionalExpression(Expression test, Expression ifTrue, Expression ifFalse) : base(test, ifTrue)
        {
            this._false = ifFalse;
        }

        internal override Expression GetFalse()
        {
            return this._false;
        }
    }
}

