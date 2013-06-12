namespace System.Linq.Expressions
{
    using System;

    internal class FullConditionalExpressionWithType : FullConditionalExpression
    {
        private readonly System.Type _type;

        internal FullConditionalExpressionWithType(Expression test, Expression ifTrue, Expression ifFalse, System.Type type) : base(test, ifTrue, ifFalse)
        {
            this._type = type;
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

