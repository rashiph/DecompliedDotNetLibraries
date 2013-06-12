namespace System.Linq.Expressions
{
    using System;
    using System.Diagnostics;
    using System.Dynamic.Utils;

    [DebuggerTypeProxy(typeof(System.Linq.Expressions.Expression.TypeBinaryExpressionProxy))]
    public sealed class TypeBinaryExpression : System.Linq.Expressions.Expression
    {
        private readonly System.Linq.Expressions.Expression _expression;
        private readonly ExpressionType _nodeKind;
        private readonly System.Type _typeOperand;

        internal TypeBinaryExpression(System.Linq.Expressions.Expression expression, System.Type typeOperand, ExpressionType nodeKind)
        {
            this._expression = expression;
            this._typeOperand = typeOperand;
            this._nodeKind = nodeKind;
        }

        protected internal override System.Linq.Expressions.Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitTypeBinary(this);
        }

        private System.Linq.Expressions.Expression ByValParameterTypeEqual(ParameterExpression value)
        {
            System.Linq.Expressions.Expression right = System.Linq.Expressions.Expression.Call(value, typeof(object).GetMethod("GetType"));
            if (this._typeOperand.IsInterface)
            {
                ParameterExpression left = System.Linq.Expressions.Expression.Parameter(typeof(System.Type));
                right = System.Linq.Expressions.Expression.Block(new ParameterExpression[] { left }, new System.Linq.Expressions.Expression[] { System.Linq.Expressions.Expression.Assign(left, right), left });
            }
            return System.Linq.Expressions.Expression.AndAlso(System.Linq.Expressions.Expression.ReferenceNotEqual(value, System.Linq.Expressions.Expression.Constant(null)), System.Linq.Expressions.Expression.ReferenceEqual(right, System.Linq.Expressions.Expression.Constant(this._typeOperand.GetNonNullableType(), typeof(System.Type))));
        }

        private System.Linq.Expressions.Expression ReduceConstantTypeEqual()
        {
            ConstantExpression expression = this.Expression as ConstantExpression;
            if (expression.Value == null)
            {
                return System.Linq.Expressions.Expression.Constant(false);
            }
            return System.Linq.Expressions.Expression.Constant(this._typeOperand.GetNonNullableType() == expression.Value.GetType());
        }

        internal System.Linq.Expressions.Expression ReduceTypeEqual()
        {
            System.Type type = this.Expression.Type;
            if (type.IsValueType && !type.IsNullableType())
            {
                return System.Linq.Expressions.Expression.Block(this.Expression, System.Linq.Expressions.Expression.Constant(type == this._typeOperand.GetNonNullableType()));
            }
            if (this.Expression.NodeType == ExpressionType.Constant)
            {
                return this.ReduceConstantTypeEqual();
            }
            if (type.IsSealed && (type == this._typeOperand))
            {
                if (type.IsNullableType())
                {
                    return System.Linq.Expressions.Expression.NotEqual(this.Expression, System.Linq.Expressions.Expression.Constant(null, this.Expression.Type));
                }
                return System.Linq.Expressions.Expression.ReferenceNotEqual(this.Expression, System.Linq.Expressions.Expression.Constant(null, this.Expression.Type));
            }
            ParameterExpression expression = this.Expression as ParameterExpression;
            if ((expression != null) && !expression.IsByRef)
            {
                return this.ByValParameterTypeEqual(expression);
            }
            expression = System.Linq.Expressions.Expression.Parameter(typeof(object));
            System.Linq.Expressions.Expression expression2 = this.Expression;
            if (!TypeUtils.AreReferenceAssignable(typeof(object), expression2.Type))
            {
                expression2 = System.Linq.Expressions.Expression.Convert(expression2, typeof(object));
            }
            return System.Linq.Expressions.Expression.Block(new ParameterExpression[] { expression }, new System.Linq.Expressions.Expression[] { System.Linq.Expressions.Expression.Assign(expression, expression2), this.ByValParameterTypeEqual(expression) });
        }

        public TypeBinaryExpression Update(System.Linq.Expressions.Expression expression)
        {
            if (expression == this.Expression)
            {
                return this;
            }
            if (this.NodeType == ExpressionType.TypeIs)
            {
                return System.Linq.Expressions.Expression.TypeIs(expression, this.TypeOperand);
            }
            return System.Linq.Expressions.Expression.TypeEqual(expression, this.TypeOperand);
        }

        public System.Linq.Expressions.Expression Expression
        {
            get
            {
                return this._expression;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return this._nodeKind;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                return typeof(bool);
            }
        }

        public System.Type TypeOperand
        {
            get
            {
                return this._typeOperand;
            }
        }
    }
}

