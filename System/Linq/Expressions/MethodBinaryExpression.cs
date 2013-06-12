namespace System.Linq.Expressions
{
    using System;
    using System.Reflection;

    internal class MethodBinaryExpression : SimpleBinaryExpression
    {
        private readonly MethodInfo _method;

        internal MethodBinaryExpression(ExpressionType nodeType, Expression left, Expression right, Type type, MethodInfo method) : base(nodeType, left, right, type)
        {
            this._method = method;
        }

        internal override MethodInfo GetMethod()
        {
            return this._method;
        }
    }
}

