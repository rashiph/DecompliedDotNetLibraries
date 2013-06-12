namespace System.Linq.Expressions
{
    using System;
    using System.Collections.ObjectModel;

    internal sealed class Block5 : BlockExpression
    {
        private object _arg0;
        private readonly Expression _arg1;
        private readonly Expression _arg2;
        private readonly Expression _arg3;
        private readonly Expression _arg4;

        internal Block5(Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4)
        {
            this._arg0 = arg0;
            this._arg1 = arg1;
            this._arg2 = arg2;
            this._arg3 = arg3;
            this._arg4 = arg4;
        }

        internal override Expression GetExpression(int index)
        {
            switch (index)
            {
                case 0:
                    return Expression.ReturnObject<Expression>(this._arg0);

                case 1:
                    return this._arg1;

                case 2:
                    return this._arg2;

                case 3:
                    return this._arg3;

                case 4:
                    return this._arg4;
            }
            throw new InvalidOperationException();
        }

        internal override ReadOnlyCollection<Expression> GetOrMakeExpressions()
        {
            return BlockExpression.ReturnReadOnlyExpressions(this, ref this._arg0);
        }

        internal override BlockExpression Rewrite(ReadOnlyCollection<ParameterExpression> variables, Expression[] args)
        {
            return new Block5(args[0], args[1], args[2], args[3], args[4]);
        }

        internal override int ExpressionCount
        {
            get
            {
                return 5;
            }
        }
    }
}

