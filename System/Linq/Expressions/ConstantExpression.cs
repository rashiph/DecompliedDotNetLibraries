namespace System.Linq.Expressions
{
    using System;
    using System.Diagnostics;

    [DebuggerTypeProxy(typeof(Expression.ConstantExpressionProxy))]
    public class ConstantExpression : Expression
    {
        private readonly object _value;

        internal ConstantExpression(object value)
        {
            this._value = value;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitConstant(this);
        }

        internal static ConstantExpression Make(object value, System.Type type)
        {
            if (((value != null) || (type != typeof(object))) && ((value == null) || !(value.GetType() == type)))
            {
                return new TypedConstantExpression(value, type);
            }
            return new ConstantExpression(value);
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Constant;
            }
        }

        public override System.Type Type
        {
            get
            {
                if (this._value == null)
                {
                    return typeof(object);
                }
                return this._value.GetType();
            }
        }

        public object Value
        {
            get
            {
                return this._value;
            }
        }
    }
}

