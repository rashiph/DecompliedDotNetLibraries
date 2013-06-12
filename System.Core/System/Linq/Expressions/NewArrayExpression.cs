namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    [DebuggerTypeProxy(typeof(Expression.NewArrayExpressionProxy))]
    public class NewArrayExpression : Expression
    {
        private readonly ReadOnlyCollection<Expression> _expressions;
        private readonly System.Type _type;

        internal NewArrayExpression(System.Type type, ReadOnlyCollection<Expression> expressions)
        {
            this._expressions = expressions;
            this._type = type;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitNewArray(this);
        }

        internal static NewArrayExpression Make(ExpressionType nodeType, System.Type type, ReadOnlyCollection<Expression> expressions)
        {
            if (nodeType == ExpressionType.NewArrayInit)
            {
                return new NewArrayInitExpression(type, expressions);
            }
            return new NewArrayBoundsExpression(type, expressions);
        }

        public NewArrayExpression Update(IEnumerable<Expression> expressions)
        {
            if (expressions == this.Expressions)
            {
                return this;
            }
            if (this.NodeType == ExpressionType.NewArrayInit)
            {
                return Expression.NewArrayInit(this.Type.GetElementType(), expressions);
            }
            return Expression.NewArrayBounds(this.Type.GetElementType(), expressions);
        }

        public ReadOnlyCollection<Expression> Expressions
        {
            get
            {
                return this._expressions;
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

