namespace System.Linq.Expressions
{
    using System;

    internal class SimpleBinaryExpression : BinaryExpression
    {
        private readonly ExpressionType _nodeType;
        private readonly System.Type _type;

        internal SimpleBinaryExpression(ExpressionType nodeType, Expression left, Expression right, System.Type type) : base(left, right)
        {
            this._nodeType = nodeType;
            this._type = type;
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
                return this._type;
            }
        }
    }
}

