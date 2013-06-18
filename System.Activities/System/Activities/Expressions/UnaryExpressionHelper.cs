namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Activities.Validation;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal static class UnaryExpressionHelper
    {
        public static void OnGetArguments<TOperand>(CodeActivityMetadata metadata, InArgument<TOperand> operand)
        {
            RuntimeArgument argument = new RuntimeArgument("Operand", typeof(TOperand), ArgumentDirection.In, true);
            metadata.Bind(operand, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
        }

        public static bool TryGenerateLinqDelegate<TOperand, TResult>(ExpressionType operatorType, out Func<TOperand, TResult> operation, out ValidationError validationError)
        {
            operation = null;
            validationError = null;
            try
            {
                ParameterExpression expression;
                operation = Expression.Lambda<Func<TOperand, TResult>>(Expression.MakeUnary(operatorType, expression = Expression.Parameter(typeof(TOperand), "operand"), typeof(TResult)), new ParameterExpression[] { expression }).Compile();
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

