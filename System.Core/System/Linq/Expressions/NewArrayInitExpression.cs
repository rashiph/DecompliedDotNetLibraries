namespace System.Linq.Expressions
{
    using System;
    using System.Collections.ObjectModel;

    internal sealed class NewArrayInitExpression : NewArrayExpression
    {
        internal NewArrayInitExpression(Type type, ReadOnlyCollection<Expression> expressions) : base(type, expressions)
        {
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.NewArrayInit;
            }
        }
    }
}

