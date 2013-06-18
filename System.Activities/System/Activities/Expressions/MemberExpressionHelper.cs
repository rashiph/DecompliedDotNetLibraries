namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Activities.Validation;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal static class MemberExpressionHelper
    {
        public static void AddOperandArgument<TOperand>(CodeActivityMetadata metadata, InArgument<TOperand> operand, bool isRequired)
        {
            RuntimeArgument argument = new RuntimeArgument("Operand", typeof(TOperand), ArgumentDirection.In, isRequired);
            metadata.Bind(operand, argument);
            metadata.AddArgument(argument);
        }

        public static void AddOperandLocationArgument<TOperand>(CodeActivityMetadata metadata, InOutArgument<TOperand> operandLocation, bool isRequired)
        {
            RuntimeArgument argument = new RuntimeArgument("OperandLocation", typeof(TOperand), ArgumentDirection.InOut, isRequired);
            metadata.Bind(operandLocation, argument);
            metadata.AddArgument(argument);
        }

        private static MemberInfo GetMemberInfo<TOperand>(string memberName, bool isField)
        {
            MemberInfo property = null;
            Type type = typeof(TOperand);
            if (!isField)
            {
                property = type.GetProperty(memberName);
            }
            else
            {
                property = type.GetField(memberName);
            }
            if (property == null)
            {
                throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.MemberNotFound(memberName, typeof(TOperand).Name)));
            }
            return property;
        }

        public static bool TryGenerateLinqDelegate<TOperand, TResult>(string memberName, bool isField, bool isStatic, out Func<TOperand, TResult> operation, out ValidationError validationError)
        {
            operation = null;
            validationError = null;
            try
            {
                ParameterExpression expression;
                MemberExpression body = null;
                if (isStatic)
                {
                    body = Expression.MakeMemberAccess(null, GetMemberInfo<TOperand>(memberName, isField));
                }
                else
                {
                    body = Expression.MakeMemberAccess(expression = Expression.Parameter(typeof(TOperand), "operand"), GetMemberInfo<TOperand>(memberName, isField));
                }
                operation = Expression.Lambda<Func<TOperand, TResult>>(body, new ParameterExpression[] { expression }).Compile();
                return true;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                validationError = new ValidationError(exception.Message);
                return false;
            }
        }
    }
}

