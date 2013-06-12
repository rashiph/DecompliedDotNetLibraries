namespace System.Linq.Expressions
{
    using System;
    using System.Reflection;

    internal sealed class OpAssignMethodConversionBinaryExpression : MethodBinaryExpression
    {
        private readonly LambdaExpression _conversion;

        internal OpAssignMethodConversionBinaryExpression(ExpressionType nodeType, Expression left, Expression right, Type type, MethodInfo method, LambdaExpression conversion) : base(nodeType, left, right, type, method)
        {
            this._conversion = conversion;
        }

        internal override LambdaExpression GetConversion()
        {
            return this._conversion;
        }
    }
}

