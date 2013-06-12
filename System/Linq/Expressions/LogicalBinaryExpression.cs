namespace System.Linq.Expressions
{
    using System;

    internal sealed class LogicalBinaryExpression : BinaryExpression
    {
        private readonly ExpressionType _nodeType;

        internal LogicalBinaryExpression(ExpressionType nodeType, Expression left, Expression right) : base(left, right)
        {
            this._nodeType = nodeType;
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return this._nodeType;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                return typeof(bool);
            }
        }
    }
}

