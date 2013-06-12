namespace System.Linq.Expressions
{
    using System;

    internal sealed class AssignBinaryExpression : BinaryExpression
    {
        internal AssignBinaryExpression(Expression left, Expression right) : base(left, right)
        {
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Assign;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                return base.Left.Type;
            }
        }
    }
}

