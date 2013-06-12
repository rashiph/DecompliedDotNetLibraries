namespace System.Linq.Expressions
{
    using System;

    internal sealed class CoalesceConversionBinaryExpression : BinaryExpression
    {
        private readonly LambdaExpression _conversion;

        internal CoalesceConversionBinaryExpression(Expression left, Expression right, LambdaExpression conversion) : base(left, right)
        {
            this._conversion = conversion;
        }

        internal override LambdaExpression GetConversion()
        {
            return this._conversion;
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Coalesce;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                return base.Right.Type;
            }
        }
    }
}

