namespace System.Linq.Expressions
{
    using System;
    using System.Dynamic.Utils;

    internal static class ConstantCheck
    {
        internal static AnalyzeTypeIsResult AnalyzeTypeIs(TypeBinaryExpression typeIs)
        {
            return AnalyzeTypeIs(typeIs.Expression, typeIs.TypeOperand);
        }

        internal static AnalyzeTypeIsResult AnalyzeTypeIs(UnaryExpression typeAs)
        {
            return AnalyzeTypeIs(typeAs.Operand, typeAs.Type);
        }

        private static AnalyzeTypeIsResult AnalyzeTypeIs(Expression operand, Type testType)
        {
            Type type = operand.Type;
            if (type == typeof(void))
            {
                return AnalyzeTypeIsResult.KnownFalse;
            }
            Type nonNullableType = type.GetNonNullableType();
            if (!testType.GetNonNullableType().IsAssignableFrom(nonNullableType))
            {
                return AnalyzeTypeIsResult.Unknown;
            }
            if (type.IsValueType && !type.IsNullableType())
            {
                return AnalyzeTypeIsResult.KnownTrue;
            }
            return AnalyzeTypeIsResult.KnownAssignable;
        }

        internal static bool IsNull(Expression e)
        {
            return ((e.NodeType == ExpressionType.Constant) && (((ConstantExpression) e).Value == null));
        }
    }
}

