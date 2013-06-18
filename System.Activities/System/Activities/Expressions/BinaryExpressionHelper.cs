namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Activities.Validation;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal static class BinaryExpressionHelper
    {
        public static void OnGetArguments<TLeft, TRight>(CodeActivityMetadata metadata, InArgument<TLeft> left, InArgument<TRight> right)
        {
            RuntimeArgument argument = new RuntimeArgument("Right", typeof(TRight), ArgumentDirection.In, true);
            metadata.Bind(right, argument);
            RuntimeArgument argument2 = new RuntimeArgument("Left", typeof(TLeft), ArgumentDirection.In, true);
            metadata.Bind(left, argument2);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument, argument2 });
        }

        public static bool TryGenerateLinqDelegate<TLeft, TRight, TResult>(ExpressionType operatorType, out Func<TLeft, TRight, TResult> function, out ValidationError validationError)
        {
            function = null;
            validationError = null;
            try
            {
                ParameterExpression expression;
                ParameterExpression expression2;
                function = Expression.Lambda<Func<TLeft, TRight, TResult>>(Expression.MakeBinary(operatorType, expression = Expression.Parameter(typeof(TLeft), "left"), expression2 = Expression.Parameter(typeof(TRight), "right")), new ParameterExpression[] { expression, expression2 }).Compile();
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

