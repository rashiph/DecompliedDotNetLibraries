namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public static class ExpressionServices
    {
        private static MethodInfo TryConvertArgumentExpressionHandle = typeof(ExpressionServices).GetMethod("TryConvertArgumentExpressionWorker", BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo TryConvertBinaryExpressionHandle = typeof(ExpressionServices).GetMethod("TryConvertBinaryExpressionWorker", BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo TryConvertIndexerReferenceHandle = typeof(ExpressionServices).GetMethod("TryConvertIndexerReferenceWorker", BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo TryConvertMemberExpressionHandle = typeof(ExpressionServices).GetMethod("TryConvertMemberExpressionWorker", BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo TryConvertReferenceMemberExpressionHandle = typeof(ExpressionServices).GetMethod("TryConvertReferenceMemberExpressionWorker", BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo TryConvertUnaryExpressionHandle = typeof(ExpressionServices).GetMethod("TryConvertUnaryExpressionWorker", BindingFlags.NonPublic | BindingFlags.Static);

        public static Activity<TResult> Convert<TResult>(Expression<Func<ActivityContext, TResult>> expression)
        {
            Activity<TResult> activity;
            if (expression == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("expression", System.Activities.SR.ExpressionRequiredForConversion));
            }
            TryConvert<TResult>(expression.Body, true, out activity);
            return activity;
        }

        public static Activity<Location<TResult>> ConvertReference<TResult>(Expression<Func<ActivityContext, TResult>> expression)
        {
            Activity<Location<TResult>> activity;
            if (expression == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("expression", System.Activities.SR.ExpressionRequiredForConversion));
            }
            TryConvertReference<TResult>(expression.Body, true, out activity);
            return activity;
        }

        public static bool TryConvert<TResult>(Expression<Func<ActivityContext, TResult>> expression, out Activity<TResult> result)
        {
            if (expression == null)
            {
                result = null;
                return false;
            }
            return (TryConvert<TResult>(expression.Body, false, out result) == null);
        }

        private static string TryConvert<TResult>(Expression body, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            UnaryExpression unaryExpressionBody = body as UnaryExpression;
            if (unaryExpressionBody != null)
            {
                Type operandType = unaryExpressionBody.Operand.Type;
                return TryConvertUnaryExpression<TResult>(unaryExpressionBody, operandType, throwOnError, out result);
            }
            BinaryExpression binaryExpression = body as BinaryExpression;
            if (binaryExpression != null)
            {
                Type type = binaryExpression.Left.Type;
                Type rightType = binaryExpression.Right.Type;
                if (binaryExpression.NodeType == ExpressionType.ArrayIndex)
                {
                    return TryConvertArrayItemValue<TResult>(binaryExpression, type, rightType, throwOnError, out result);
                }
                return TryConvertBinaryExpression<TResult>(binaryExpression, type, rightType, throwOnError, out result);
            }
            MemberExpression memberExpressionBody = body as MemberExpression;
            if (memberExpressionBody != null)
            {
                Type type4 = (memberExpressionBody.Expression == null) ? memberExpressionBody.Member.DeclaringType : memberExpressionBody.Expression.Type;
                return TryConvertMemberExpression<TResult>(memberExpressionBody, type4, throwOnError, out result);
            }
            MethodCallExpression methodCallExpression = body as MethodCallExpression;
            if (methodCallExpression != null)
            {
                MethodInfo method = methodCallExpression.Method;
                Type declaringType = method.DeclaringType;
                ParameterInfo[] parameters = method.GetParameters();
                if ((TypeHelper.AreTypesCompatible(declaringType, typeof(Variable)) && (method.Name == "Get")) && ((parameters.Length == 1) && TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(ActivityContext))))
                {
                    return TryConvertVariableValue<TResult>(methodCallExpression, throwOnError, out result);
                }
                if ((TypeHelper.AreTypesCompatible(declaringType, typeof(Argument)) && (method.Name == "Get")) && ((parameters.Length == 1) && TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(ActivityContext))))
                {
                    return TryConvertArgumentValue<TResult>(methodCallExpression.Object as MemberExpression, throwOnError, out result);
                }
                if ((TypeHelper.AreTypesCompatible(declaringType, typeof(DelegateArgument)) && (method.Name == "Get")) && ((parameters.Length == 1) && TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(ActivityContext))))
                {
                    return TryConvertDelegateArgumentValue<TResult>(methodCallExpression, throwOnError, out result);
                }
                if (((!TypeHelper.AreTypesCompatible(declaringType, typeof(ActivityContext)) || !(method.Name == "GetValue")) || (parameters.Length != 1)) || (!TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(Argument)) && !TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(RuntimeArgument))))
                {
                    return TryConvertMethodCallExpression<TResult>(methodCallExpression, throwOnError, out result);
                }
                MemberExpression memberExpression = methodCallExpression.Arguments[0] as MemberExpression;
                return TryConvertArgumentValue<TResult>(memberExpression, throwOnError, out result);
            }
            InvocationExpression invocationExpression = body as InvocationExpression;
            if (invocationExpression != null)
            {
                return TryConvertInvocationExpression<TResult>(invocationExpression, throwOnError, out result);
            }
            NewExpression newExpression = body as NewExpression;
            if (newExpression != null)
            {
                return TryConvertNewExpression<TResult>(newExpression, throwOnError, out result);
            }
            NewArrayExpression newArrayExpression = body as NewArrayExpression;
            if ((newArrayExpression != null) && (newArrayExpression.NodeType != ExpressionType.NewArrayInit))
            {
                return TryConvertNewArrayExpression<TResult>(newArrayExpression, throwOnError, out result);
            }
            ConstantExpression expression9 = body as ConstantExpression;
            if (expression9 != null)
            {
                Literal<TResult> literal = new Literal<TResult> {
                    Value = (TResult) expression9.Value
                };
                result = literal;
                return null;
            }
            if (throwOnError)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.UnsupportedExpressionType(body.NodeType)));
            }
            return System.Activities.SR.UnsupportedExpressionType(body.NodeType);
        }

        private static string TryConvertArgumentExpressionWorker<TArgument>(Expression expression, bool isByRef, bool throwOnError, out Argument result)
        {
            Activity<TArgument> activity2;
            result = null;
            string str = null;
            if (isByRef)
            {
                Activity<Location<TArgument>> activity;
                str = TryConvertReference<TArgument>(expression, throwOnError, out activity);
                if (str == null)
                {
                    InOutArgument<TArgument> argument = new InOutArgument<TArgument> {
                        Expression = activity
                    };
                    result = argument;
                }
                return str;
            }
            str = TryConvert<TArgument>(expression, throwOnError, out activity2);
            if (str == null)
            {
                InArgument<TArgument> argument2 = new InArgument<TArgument> {
                    Expression = activity2
                };
                result = argument2;
            }
            return str;
        }

        private static string TryConvertArgumentReference<TResult>(MethodCallExpression methodCallExpression, bool throwOnError, out Activity<Location<TResult>> result)
        {
            result = null;
            if (methodCallExpression.Object is MemberExpression)
            {
                MemberExpression expression = methodCallExpression.Object as MemberExpression;
                if (expression.Member is PropertyInfo)
                {
                    PropertyInfo member = expression.Member as PropertyInfo;
                    ArgumentReference<TResult> reference = new ArgumentReference<TResult> {
                        ArgumentName = member.Name
                    };
                    result = reference;
                    return null;
                }
            }
            if (throwOnError)
            {
                throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.ArgumentMustbePropertyofWorkflowElement));
            }
            return System.Activities.SR.ArgumentMustbePropertyofWorkflowElement;
        }

        private static string TryConvertArguments(ReadOnlyCollection<Expression> source, IList target, Type expressionType, int baseEvaluationOrder, ParameterInfo[] parameterInfoArray, bool throwOnError)
        {
            for (int i = 0; i < source.Count; i++)
            {
                bool isByRef = false;
                Expression expression = source[i];
                if (parameterInfoArray != null)
                {
                    ParameterInfo info = parameterInfoArray[i];
                    if ((info == null) || (info.ParameterType == null))
                    {
                        if (throwOnError)
                        {
                            throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.InvalidParameterInfo(i, expressionType.Name)));
                        }
                        return System.Activities.SR.InvalidParameterInfo(i, expressionType.Name);
                    }
                    isByRef = info.ParameterType.IsByRef;
                }
                object[] objArray2 = new object[4];
                objArray2[0] = expression;
                objArray2[1] = isByRef;
                objArray2[2] = throwOnError;
                object[] parameters = objArray2;
                string str = TryConvertArgumentExpressionHandle.MakeGenericMethod(new Type[] { expression.Type }).Invoke(null, parameters) as string;
                if (str != null)
                {
                    return str;
                }
                Argument argument = (Argument) parameters[3];
                argument.EvaluationOrder = i + baseEvaluationOrder;
                target.Add(argument);
            }
            return null;
        }

        private static string TryConvertArgumentValue<TResult>(MemberExpression memberExpression, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            if ((memberExpression != null) && TypeHelper.AreTypesCompatible(memberExpression.Type, typeof(RuntimeArgument)))
            {
                RuntimeArgument argument = null;
                try
                {
                    Expression<Func<RuntimeArgument>> expression = Expression.Lambda<Func<RuntimeArgument>>(memberExpression, (ParameterExpression[]) null);
                    argument = expression.Compile()();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    return exception.Message;
                }
                if (argument != null)
                {
                    ArgumentValue<TResult> value2 = new ArgumentValue<TResult> {
                        ArgumentName = argument.Name
                    };
                    result = value2;
                    return null;
                }
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.RuntimeArgumentNotCreated));
                }
                return System.Activities.SR.RuntimeArgumentNotCreated;
            }
            if ((memberExpression != null) && (memberExpression.Member is PropertyInfo))
            {
                PropertyInfo member = memberExpression.Member as PropertyInfo;
                ArgumentValue<TResult> value3 = new ArgumentValue<TResult> {
                    ArgumentName = member.Name
                };
                result = value3;
                return null;
            }
            if (throwOnError)
            {
                throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.ArgumentMustbePropertyofWorkflowElement));
            }
            return System.Activities.SR.ArgumentMustbePropertyofWorkflowElement;
        }

        private static string TryConvertArrayItemReference<TResult>(BinaryExpression binaryExpression, Type leftType, Type rightType, bool throwOnError, out Activity<Location<TResult>> result)
        {
            Activity<TResult[]> activity;
            Activity<int> activity2;
            result = null;
            if (!leftType.IsArray)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.DoNotSupportArrayIndexerOnNonArrayType(leftType)));
                }
                return System.Activities.SR.DoNotSupportArrayIndexerOnNonArrayType(leftType);
            }
            if (leftType.GetElementType() != typeof(TResult))
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.DoNotSupportArrayIndexerReferenceWithDifferentArrayTypeAndResultType(leftType, typeof(TResult))));
                }
                return System.Activities.SR.DoNotSupportArrayIndexerReferenceWithDifferentArrayTypeAndResultType(leftType, typeof(TResult));
            }
            if (rightType != typeof(int))
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.DoNotSupportArrayIndexerWithNonIntIndex(rightType)));
                }
                return System.Activities.SR.DoNotSupportArrayIndexerWithNonIntIndex(rightType);
            }
            string str = TryConvert<TResult[]>(binaryExpression.Left, throwOnError, out activity);
            if (str != null)
            {
                return str;
            }
            string str2 = TryConvert<int>(binaryExpression.Right, throwOnError, out activity2);
            if (str2 != null)
            {
                return str2;
            }
            ArrayItemReference<TResult> reference = new ArrayItemReference<TResult>();
            InArgument<TResult[]> argument = new InArgument<TResult[]>(activity) {
                EvaluationOrder = 0
            };
            reference.Array = argument;
            InArgument<int> argument2 = new InArgument<int>(activity2) {
                EvaluationOrder = 1
            };
            reference.Index = argument2;
            result = reference;
            return null;
        }

        private static string TryConvertArrayItemValue<TResult>(BinaryExpression binaryExpression, Type leftType, Type rightType, bool throwOnError, out Activity<TResult> result)
        {
            Activity<TResult[]> activity;
            Activity<int> activity2;
            result = null;
            if (!leftType.IsArray)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.DoNotSupportArrayIndexerOnNonArrayType(leftType)));
                }
                return System.Activities.SR.DoNotSupportArrayIndexerOnNonArrayType(leftType);
            }
            if (!TypeHelper.AreTypesCompatible(leftType.GetElementType(), typeof(TResult)))
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.DoNotSupportArrayIndexerValueWithIncompatibleArrayTypeAndResultType(leftType, typeof(TResult))));
                }
                return System.Activities.SR.DoNotSupportArrayIndexerValueWithIncompatibleArrayTypeAndResultType(leftType, typeof(TResult));
            }
            if (rightType != typeof(int))
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.DoNotSupportArrayIndexerWithNonIntIndex(rightType)));
                }
                return System.Activities.SR.DoNotSupportArrayIndexerWithNonIntIndex(rightType);
            }
            string str = TryConvert<TResult[]>(binaryExpression.Left, throwOnError, out activity);
            if (str != null)
            {
                return str;
            }
            string str2 = TryConvert<int>(binaryExpression.Right, throwOnError, out activity2);
            if (str2 != null)
            {
                return str2;
            }
            ArrayItemValue<TResult> value2 = new ArrayItemValue<TResult>();
            InArgument<TResult[]> argument = new InArgument<TResult[]>(activity) {
                EvaluationOrder = 0
            };
            value2.Array = argument;
            InArgument<int> argument2 = new InArgument<int>(activity2) {
                EvaluationOrder = 1
            };
            value2.Index = argument2;
            result = value2;
            return null;
        }

        private static string TryConvertBinaryExpression<TResult>(BinaryExpression binaryExpressionBody, Type leftType, Type rightType, bool throwOnError, out Activity<TResult> result)
        {
            string str2;
            try
            {
                MethodInfo info = TryConvertBinaryExpressionHandle.MakeGenericMethod(new Type[] { leftType, rightType, typeof(TResult) });
                object[] objArray2 = new object[3];
                objArray2[0] = binaryExpressionBody;
                objArray2[1] = throwOnError;
                object[] parameters = objArray2;
                string str = info.Invoke(null, parameters) as string;
                result = parameters[2] as Activity<TResult>;
                str2 = str;
            }
            catch (TargetInvocationException exception)
            {
                throw FxTrace.Exception.AsError(exception.InnerException);
            }
            return str2;
        }

        private static string TryConvertBinaryExpressionWorker<TLeft, TRight, TResult>(BinaryExpression binaryExpressionBody, bool throwOnError, out Activity<TResult> result)
        {
            Activity<TLeft> activity;
            Activity<TRight> activity2;
            result = null;
            string str = TryConvert<TLeft>(binaryExpressionBody.Left, throwOnError, out activity);
            if (str != null)
            {
                return str;
            }
            string str2 = TryConvert<TRight>(binaryExpressionBody.Right, throwOnError, out activity2);
            if (str2 != null)
            {
                return str2;
            }
            if (binaryExpressionBody.Method != null)
            {
                return TryConvertOverloadingBinaryOperator<TLeft, TRight, TResult>(binaryExpressionBody, activity, activity2, throwOnError, out result);
            }
            InArgument<TLeft> argument = new InArgument<TLeft>(activity) {
                EvaluationOrder = 0
            };
            InArgument<TRight> argument2 = new InArgument<TRight>(activity2) {
                EvaluationOrder = 1
            };
            switch (binaryExpressionBody.NodeType)
            {
                case ExpressionType.Add:
                {
                    Add<TLeft, TRight, TResult> add = new Add<TLeft, TRight, TResult> {
                        Left = argument,
                        Right = argument2,
                        Checked = false
                    };
                    result = add;
                    break;
                }
                case ExpressionType.AddChecked:
                {
                    Add<TLeft, TRight, TResult> add2 = new Add<TLeft, TRight, TResult> {
                        Left = argument,
                        Right = argument2,
                        Checked = true
                    };
                    result = add2;
                    break;
                }
                case ExpressionType.And:
                {
                    And<TLeft, TRight, TResult> and = new And<TLeft, TRight, TResult> {
                        Left = argument,
                        Right = argument2
                    };
                    result = and;
                    break;
                }
                case ExpressionType.AndAlso:
                {
                    object obj2 = activity;
                    object obj3 = activity2;
                    AndAlso also = new AndAlso {
                        Left = (Activity<bool>) obj2,
                        Right = (Activity<bool>) obj3
                    };
                    object obj4 = also;
                    result = (Activity<TResult>) obj4;
                    break;
                }
                case ExpressionType.Divide:
                {
                    Divide<TLeft, TRight, TResult> divide = new Divide<TLeft, TRight, TResult> {
                        Left = argument,
                        Right = argument2
                    };
                    result = divide;
                    break;
                }
                case ExpressionType.Equal:
                {
                    Equal<TLeft, TRight, TResult> equal3 = new Equal<TLeft, TRight, TResult> {
                        Left = argument,
                        Right = argument2
                    };
                    result = equal3;
                    break;
                }
                case ExpressionType.GreaterThan:
                {
                    GreaterThan<TLeft, TRight, TResult> than2 = new GreaterThan<TLeft, TRight, TResult> {
                        Left = argument,
                        Right = argument2
                    };
                    result = than2;
                    break;
                }
                case ExpressionType.GreaterThanOrEqual:
                {
                    GreaterThanOrEqual<TLeft, TRight, TResult> equal2 = new GreaterThanOrEqual<TLeft, TRight, TResult> {
                        Left = argument,
                        Right = argument2
                    };
                    result = equal2;
                    break;
                }
                case ExpressionType.LessThan:
                {
                    LessThan<TLeft, TRight, TResult> than = new LessThan<TLeft, TRight, TResult> {
                        Left = argument,
                        Right = argument2
                    };
                    result = than;
                    break;
                }
                case ExpressionType.LessThanOrEqual:
                {
                    LessThanOrEqual<TLeft, TRight, TResult> equal = new LessThanOrEqual<TLeft, TRight, TResult> {
                        Left = argument,
                        Right = argument2
                    };
                    result = equal;
                    break;
                }
                case ExpressionType.Multiply:
                {
                    Multiply<TLeft, TRight, TResult> multiply = new Multiply<TLeft, TRight, TResult> {
                        Left = argument,
                        Right = argument2,
                        Checked = false
                    };
                    result = multiply;
                    break;
                }
                case ExpressionType.MultiplyChecked:
                {
                    Multiply<TLeft, TRight, TResult> multiply2 = new Multiply<TLeft, TRight, TResult> {
                        Left = argument,
                        Right = argument2,
                        Checked = true
                    };
                    result = multiply2;
                    break;
                }
                case ExpressionType.NotEqual:
                {
                    NotEqual<TLeft, TRight, TResult> equal4 = new NotEqual<TLeft, TRight, TResult> {
                        Left = argument,
                        Right = argument2
                    };
                    result = equal4;
                    break;
                }
                case ExpressionType.Or:
                {
                    Or<TLeft, TRight, TResult> or = new Or<TLeft, TRight, TResult> {
                        Left = argument,
                        Right = argument2
                    };
                    result = or;
                    break;
                }
                case ExpressionType.OrElse:
                {
                    object obj5 = activity;
                    object obj6 = activity2;
                    OrElse @else = new OrElse {
                        Left = (Activity<bool>) obj5,
                        Right = (Activity<bool>) obj6
                    };
                    object obj7 = @else;
                    result = (Activity<TResult>) obj7;
                    break;
                }
                case ExpressionType.Subtract:
                {
                    Subtract<TLeft, TRight, TResult> subtract = new Subtract<TLeft, TRight, TResult> {
                        Left = argument,
                        Right = argument2,
                        Checked = false
                    };
                    result = subtract;
                    break;
                }
                case ExpressionType.SubtractChecked:
                {
                    Subtract<TLeft, TRight, TResult> subtract2 = new Subtract<TLeft, TRight, TResult> {
                        Left = argument,
                        Right = argument2,
                        Checked = true
                    };
                    result = subtract2;
                    break;
                }
                default:
                    if (throwOnError)
                    {
                        throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.UnsupportedExpressionType(binaryExpressionBody.NodeType)));
                    }
                    return System.Activities.SR.UnsupportedExpressionType(binaryExpressionBody.NodeType);
            }
            return null;
        }

        private static string TryConvertDelegateArgumentReference<TResult>(MethodCallExpression methodCallExpression, bool throwOnError, out Activity<Location<TResult>> result)
        {
            result = null;
            DelegateArgument delegateArgument = null;
            try
            {
                Expression<Func<DelegateArgument>> expression = Expression.Lambda<Func<DelegateArgument>>(methodCallExpression.Object, new ParameterExpression[0]);
                delegateArgument = expression.Compile()();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(exception);
                }
                return exception.Message;
            }
            result = new DelegateArgumentReference<TResult>(delegateArgument);
            return null;
        }

        private static string TryConvertDelegateArgumentValue<TResult>(MethodCallExpression methodCallExpression, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            DelegateArgument delegateArgument = null;
            try
            {
                Expression<Func<DelegateArgument>> expression = Expression.Lambda<Func<DelegateArgument>>(methodCallExpression.Object, new ParameterExpression[0]);
                delegateArgument = expression.Compile()();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(exception);
                }
                return exception.Message;
            }
            result = new DelegateArgumentValue<TResult>(delegateArgument);
            return null;
        }

        private static string TryConvertIndexerReference<TResult>(MethodCallExpression methodCallExpressionBody, bool throwOnError, out Activity<Location<TResult>> result)
        {
            string str2;
            result = null;
            try
            {
                if (methodCallExpressionBody.Object == null)
                {
                    if (throwOnError)
                    {
                        throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.InstanceMethodCallRequiresTargetObject));
                    }
                    return System.Activities.SR.InstanceMethodCallRequiresTargetObject;
                }
                MethodInfo info = TryConvertIndexerReferenceHandle.MakeGenericMethod(new Type[] { methodCallExpressionBody.Object.Type, typeof(TResult) });
                object[] objArray2 = new object[3];
                objArray2[0] = methodCallExpressionBody;
                objArray2[1] = throwOnError;
                object[] parameters = objArray2;
                string str = info.Invoke(null, parameters) as string;
                result = parameters[2] as Activity<Location<TResult>>;
                str2 = str;
            }
            catch (TargetInvocationException exception)
            {
                throw FxTrace.Exception.AsError(exception.InnerException);
            }
            return str2;
        }

        private static string TryConvertIndexerReferenceWorker<TOperand, TResult>(MethodCallExpression methodCallExpressionBody, bool throwOnError, out Activity<Location<TResult>> result)
        {
            result = null;
            if (!typeof(TOperand).IsValueType)
            {
                Activity<TOperand> activity = null;
                string str = TryConvert<TOperand>(methodCallExpressionBody.Object, throwOnError, out activity);
                if (str != null)
                {
                    return str;
                }
                IndexerReference<TOperand, TResult> reference2 = new IndexerReference<TOperand, TResult>();
                InArgument<TOperand> argument = new InArgument<TOperand>(activity) {
                    EvaluationOrder = 0
                };
                reference2.Operand = argument;
                IndexerReference<TOperand, TResult> reference = reference2;
                string str2 = TryConvertArguments(methodCallExpressionBody.Arguments, reference.Indices, methodCallExpressionBody.GetType(), 1, null, throwOnError);
                if (str2 != null)
                {
                    return str2;
                }
                result = reference;
            }
            else
            {
                Activity<Location<TOperand>> activity2 = null;
                string str3 = TryConvertReference<TOperand>(methodCallExpressionBody.Object, throwOnError, out activity2);
                if (str3 != null)
                {
                    return str3;
                }
                ValueTypeIndexerReference<TOperand, TResult> reference4 = new ValueTypeIndexerReference<TOperand, TResult>();
                InOutArgument<TOperand> argument2 = new InOutArgument<TOperand>(activity2) {
                    EvaluationOrder = 0
                };
                reference4.OperandLocation = argument2;
                ValueTypeIndexerReference<TOperand, TResult> reference3 = reference4;
                string str4 = TryConvertArguments(methodCallExpressionBody.Arguments, reference3.Indices, methodCallExpressionBody.GetType(), 1, null, throwOnError);
                if (str4 != null)
                {
                    return str4;
                }
                result = reference3;
            }
            return null;
        }

        private static string TryConvertInvocationExpression<TResult>(InvocationExpression invocationExpression, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            if ((invocationExpression.Expression == null) || (invocationExpression.Expression.Type == null))
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.InvalidExpressionProperty(invocationExpression.GetType().Name)));
                }
                return System.Activities.SR.InvalidExpressionProperty(invocationExpression.GetType().Name);
            }
            InvokeMethod<TResult> method = new InvokeMethod<TResult> {
                MethodName = "Invoke"
            };
            object[] objArray2 = new object[4];
            objArray2[0] = invocationExpression.Expression;
            objArray2[1] = false;
            objArray2[2] = throwOnError;
            object[] parameters = objArray2;
            string str = TryConvertArgumentExpressionHandle.MakeGenericMethod(new Type[] { invocationExpression.Expression.Type }).Invoke(null, parameters) as string;
            if (str != null)
            {
                return str;
            }
            InArgument argument = (InArgument) parameters[3];
            argument.EvaluationOrder = 0;
            method.TargetObject = argument;
            str = TryConvertArguments(invocationExpression.Arguments, method.Parameters, invocationExpression.GetType(), 1, null, throwOnError);
            if (str != null)
            {
                return str;
            }
            result = method;
            return null;
        }

        private static string TryConvertMemberExpression<TResult>(MemberExpression memberExpressionBody, Type operandType, bool throwOnError, out Activity<TResult> result)
        {
            string str2;
            try
            {
                MethodInfo info = TryConvertMemberExpressionHandle.MakeGenericMethod(new Type[] { operandType, typeof(TResult) });
                object[] objArray2 = new object[3];
                objArray2[0] = memberExpressionBody;
                objArray2[1] = throwOnError;
                object[] parameters = objArray2;
                string str = info.Invoke(null, parameters) as string;
                result = parameters[2] as Activity<TResult>;
                str2 = str;
            }
            catch (TargetInvocationException exception)
            {
                throw FxTrace.Exception.AsError(exception.InnerException);
            }
            return str2;
        }

        private static string TryConvertMemberExpressionWorker<TOperand, TResult>(MemberExpression memberExpressionBody, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            Activity<TOperand> activity = null;
            if (memberExpressionBody.Expression != null)
            {
                string str = TryConvert<TOperand>(memberExpressionBody.Expression, throwOnError, out activity);
                if (str != null)
                {
                    return str;
                }
            }
            if (memberExpressionBody.Member is PropertyInfo)
            {
                if (activity == null)
                {
                    PropertyValue<TOperand, TResult> value2 = new PropertyValue<TOperand, TResult> {
                        PropertyName = memberExpressionBody.Member.Name
                    };
                    result = value2;
                }
                else
                {
                    PropertyValue<TOperand, TResult> value3 = new PropertyValue<TOperand, TResult> {
                        Operand = activity,
                        PropertyName = memberExpressionBody.Member.Name
                    };
                    result = value3;
                }
                return null;
            }
            if (memberExpressionBody.Member is FieldInfo)
            {
                if (activity == null)
                {
                    FieldValue<TOperand, TResult> value4 = new FieldValue<TOperand, TResult> {
                        FieldName = memberExpressionBody.Member.Name
                    };
                    result = value4;
                }
                else
                {
                    FieldValue<TOperand, TResult> value5 = new FieldValue<TOperand, TResult> {
                        Operand = activity,
                        FieldName = memberExpressionBody.Member.Name
                    };
                    result = value5;
                }
                return null;
            }
            if (throwOnError)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.UnsupportedMemberExpressionWithType(memberExpressionBody.Member.GetType().Name)));
            }
            return System.Activities.SR.UnsupportedMemberExpressionWithType(memberExpressionBody.Member.GetType().Name);
        }

        private static string TryConvertMethodCallExpression<TResult>(MethodCallExpression methodCallExpression, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            MethodInfo info = methodCallExpression.Method;
            if (info == null)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.MethodInfoRequired(methodCallExpression.GetType().Name)));
                }
                return System.Activities.SR.MethodInfoRequired(methodCallExpression.GetType().Name);
            }
            if (string.IsNullOrEmpty(info.Name) || (info.DeclaringType == null))
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.MethodNameRequired(info.GetType().Name)));
                }
                return System.Activities.SR.MethodNameRequired(info.GetType().Name);
            }
            InvokeMethod<TResult> method = new InvokeMethod<TResult> {
                MethodName = info.Name
            };
            ParameterInfo[] parameters = info.GetParameters();
            if (methodCallExpression.Arguments.Count != parameters.Length)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.ArgumentNumberRequiresTheSameAsParameterNumber(methodCallExpression.GetType().Name)));
                }
                return System.Activities.SR.ArgumentNumberRequiresTheSameAsParameterNumber(methodCallExpression.GetType().Name);
            }
            string str = TryConvertArguments(methodCallExpression.Arguments, method.Parameters, methodCallExpression.GetType(), 1, parameters, throwOnError);
            if (str != null)
            {
                return str;
            }
            foreach (Type type in info.GetGenericArguments())
            {
                if (type == null)
                {
                    if (throwOnError)
                    {
                        throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.InvalidGenericTypeInfo(methodCallExpression.GetType().Name)));
                    }
                    return System.Activities.SR.InvalidGenericTypeInfo(methodCallExpression.GetType().Name);
                }
                method.GenericTypeArguments.Add(type);
            }
            if (info.IsStatic)
            {
                method.TargetType = info.DeclaringType;
            }
            else
            {
                if (methodCallExpression.Object == null)
                {
                    if (throwOnError)
                    {
                        throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.InstanceMethodCallRequiresTargetObject));
                    }
                    return System.Activities.SR.InstanceMethodCallRequiresTargetObject;
                }
                object[] objArray2 = new object[4];
                objArray2[0] = methodCallExpression.Object;
                objArray2[1] = false;
                objArray2[2] = throwOnError;
                object[] objArray = objArray2;
                str = TryConvertArgumentExpressionHandle.MakeGenericMethod(new Type[] { methodCallExpression.Object.Type }).Invoke(null, objArray) as string;
                if (str != null)
                {
                    return str;
                }
                InArgument argument = (InArgument) objArray[3];
                argument.EvaluationOrder = 0;
                method.TargetObject = argument;
            }
            result = method;
            return null;
        }

        private static string TryConvertMultiDimensionalArrayItemReference<TResult>(MethodCallExpression methodCallExpression, bool throwOnError, out Activity<Location<TResult>> result)
        {
            Activity<Array> activity;
            result = null;
            if (methodCallExpression.Object == null)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.InstanceMethodCallRequiresTargetObject));
                }
                return System.Activities.SR.InstanceMethodCallRequiresTargetObject;
            }
            string str = TryConvert<Array>(methodCallExpression.Object, throwOnError, out activity);
            if (str != null)
            {
                return str;
            }
            MultidimensionalArrayItemReference<TResult> reference2 = new MultidimensionalArrayItemReference<TResult>();
            InArgument<Array> argument = new InArgument<Array>(activity) {
                EvaluationOrder = 0
            };
            reference2.Array = argument;
            MultidimensionalArrayItemReference<TResult> reference = reference2;
            Collection<InArgument<int>> indices = reference.Indices;
            string str2 = TryConvertArguments(methodCallExpression.Arguments, reference.Indices, methodCallExpression.GetType(), 1, null, throwOnError);
            if (str2 != null)
            {
                return str2;
            }
            result = reference;
            return null;
        }

        private static string TryConvertNewArrayExpression<TResult>(NewArrayExpression newArrayExpression, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            NewArray<TResult> array = new NewArray<TResult>();
            string str = TryConvertArguments(newArrayExpression.Expressions, array.Bounds, newArrayExpression.GetType(), 0, null, throwOnError);
            if (str != null)
            {
                return str;
            }
            result = array;
            return null;
        }

        private static string TryConvertNewExpression<TResult>(NewExpression newExpression, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            New<TResult> new2 = new New<TResult>();
            ParameterInfo[] parameterInfoArray = null;
            if (newExpression.Constructor != null)
            {
                parameterInfoArray = newExpression.Constructor.GetParameters();
                if (newExpression.Arguments.Count != parameterInfoArray.Length)
                {
                    if (throwOnError)
                    {
                        throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.ArgumentNumberRequiresTheSameAsParameterNumber(newExpression.GetType().Name)));
                    }
                    return System.Activities.SR.ArgumentNumberRequiresTheSameAsParameterNumber(newExpression.GetType().Name);
                }
            }
            string str = TryConvertArguments(newExpression.Arguments, new2.Arguments, newExpression.GetType(), 0, parameterInfoArray, throwOnError);
            if (str != null)
            {
                return str;
            }
            result = new2;
            return null;
        }

        private static string TryConvertOverloadingBinaryOperator<TLeft, TRight, TResult>(BinaryExpression binaryExpression, Activity<TLeft> left, Activity<TRight> right, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            if (!binaryExpression.Method.IsStatic)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.OverloadingMethodMustBeStatic));
                }
                return System.Activities.SR.OverloadingMethodMustBeStatic;
            }
            InvokeMethod<TResult> method = new InvokeMethod<TResult> {
                MethodName = binaryExpression.Method.Name,
                TargetType = binaryExpression.Method.DeclaringType
            };
            InArgument<TLeft> item = new InArgument<TLeft> {
                Expression = left,
                EvaluationOrder = 0
            };
            method.Parameters.Add(item);
            InArgument<TRight> argument2 = new InArgument<TRight> {
                Expression = right,
                EvaluationOrder = 1
            };
            method.Parameters.Add(argument2);
            result = (Activity<TResult>) method;
            return null;
        }

        private static string TryConvertOverloadingUnaryOperator<TOperand, TResult>(UnaryExpression unaryExpression, Activity<TOperand> operand, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            if (!unaryExpression.Method.IsStatic)
            {
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.OverloadingMethodMustBeStatic));
                }
                return System.Activities.SR.OverloadingMethodMustBeStatic;
            }
            InvokeMethod<TResult> method = new InvokeMethod<TResult> {
                MethodName = unaryExpression.Method.Name,
                TargetType = unaryExpression.Method.DeclaringType
            };
            InArgument<TOperand> item = new InArgument<TOperand> {
                Expression = operand
            };
            method.Parameters.Add(item);
            result = (Activity<TResult>) method;
            return null;
        }

        public static bool TryConvertReference<TResult>(Expression<Func<ActivityContext, TResult>> expression, out Activity<Location<TResult>> result)
        {
            if (expression == null)
            {
                result = null;
                return false;
            }
            return (TryConvertReference<TResult>(expression.Body, false, out result) == null);
        }

        private static string TryConvertReference<TResult>(Expression body, bool throwOnError, out Activity<Location<TResult>> result)
        {
            result = null;
            MemberExpression memberExpressionBody = body as MemberExpression;
            if (memberExpressionBody != null)
            {
                Type operandType = (memberExpressionBody.Expression == null) ? memberExpressionBody.Member.DeclaringType : memberExpressionBody.Expression.Type;
                return TryConvertReferenceMemberExpression<TResult>(memberExpressionBody, operandType, throwOnError, out result);
            }
            BinaryExpression binaryExpression = body as BinaryExpression;
            if (binaryExpression != null)
            {
                Type type = binaryExpression.Left.Type;
                Type rightType = binaryExpression.Right.Type;
                if (binaryExpression.NodeType == ExpressionType.ArrayIndex)
                {
                    return TryConvertArrayItemReference<TResult>(binaryExpression, type, rightType, throwOnError, out result);
                }
            }
            MethodCallExpression methodCallExpression = body as MethodCallExpression;
            if (methodCallExpression != null)
            {
                Type declaringType = methodCallExpression.Method.DeclaringType;
                MethodInfo method = methodCallExpression.Method;
                if (declaringType.IsArray && (method.Name == "Get"))
                {
                    return TryConvertMultiDimensionalArrayItemReference<TResult>(methodCallExpression, throwOnError, out result);
                }
                if (method.IsSpecialName && (method.Name == "get_Item"))
                {
                    return TryConvertIndexerReference<TResult>(methodCallExpression, throwOnError, out result);
                }
                ParameterInfo[] parameters = method.GetParameters();
                if ((TypeHelper.AreTypesCompatible(declaringType, typeof(Variable)) && (method.Name == "Get")) && ((parameters.Length == 1) && TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(ActivityContext))))
                {
                    return TryConvertVariableReference<TResult>(methodCallExpression, throwOnError, out result);
                }
                if ((TypeHelper.AreTypesCompatible(declaringType, typeof(Argument)) && (method.Name == "Get")) && ((parameters.Length == 1) && TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(ActivityContext))))
                {
                    return TryConvertArgumentReference<TResult>(methodCallExpression, throwOnError, out result);
                }
                if ((TypeHelper.AreTypesCompatible(declaringType, typeof(DelegateArgument)) && (method.Name == "Get")) && ((parameters.Length == 1) && TypeHelper.AreTypesCompatible(parameters[0].ParameterType, typeof(ActivityContext))))
                {
                    return TryConvertDelegateArgumentReference<TResult>(methodCallExpression, throwOnError, out result);
                }
            }
            if (throwOnError)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.UnsupportedReferenceExpressionType(body.NodeType)));
            }
            return System.Activities.SR.UnsupportedReferenceExpressionType(body.NodeType);
        }

        private static string TryConvertReferenceMemberExpression<TResult>(MemberExpression memberExpressionBody, Type operandType, bool throwOnError, out Activity<Location<TResult>> result)
        {
            string str2;
            try
            {
                MethodInfo info = TryConvertReferenceMemberExpressionHandle.MakeGenericMethod(new Type[] { operandType, typeof(TResult) });
                object[] objArray2 = new object[3];
                objArray2[0] = memberExpressionBody;
                objArray2[1] = throwOnError;
                object[] parameters = objArray2;
                string str = info.Invoke(null, parameters) as string;
                result = parameters[2] as Activity<Location<TResult>>;
                str2 = str;
            }
            catch (TargetInvocationException exception)
            {
                throw FxTrace.Exception.AsError(exception.InnerException);
            }
            return str2;
        }

        private static string TryConvertReferenceMemberExpressionWorker<TOperand, TResult>(MemberExpression memberExpressionBody, bool throwOnError, out Activity<Location<TResult>> result)
        {
            result = null;
            Activity<TOperand> activity = null;
            Activity<Location<TOperand>> activity2 = null;
            bool isValueType = typeof(TOperand).IsValueType;
            if (memberExpressionBody.Expression != null)
            {
                if (!isValueType)
                {
                    string str = TryConvert<TOperand>(memberExpressionBody.Expression, throwOnError, out activity);
                    if (str != null)
                    {
                        return str;
                    }
                }
                else
                {
                    string str2 = TryConvertReference<TOperand>(memberExpressionBody.Expression, throwOnError, out activity2);
                    if (str2 != null)
                    {
                        return str2;
                    }
                }
            }
            if (memberExpressionBody.Member is PropertyInfo)
            {
                if (!isValueType)
                {
                    if (activity == null)
                    {
                        PropertyReference<TOperand, TResult> reference = new PropertyReference<TOperand, TResult> {
                            PropertyName = memberExpressionBody.Member.Name
                        };
                        result = reference;
                    }
                    else
                    {
                        PropertyReference<TOperand, TResult> reference2 = new PropertyReference<TOperand, TResult> {
                            Operand = activity,
                            PropertyName = memberExpressionBody.Member.Name
                        };
                        result = reference2;
                    }
                }
                else if (activity2 == null)
                {
                    ValueTypePropertyReference<TOperand, TResult> reference3 = new ValueTypePropertyReference<TOperand, TResult> {
                        PropertyName = memberExpressionBody.Member.Name
                    };
                    result = reference3;
                }
                else
                {
                    ValueTypePropertyReference<TOperand, TResult> reference4 = new ValueTypePropertyReference<TOperand, TResult> {
                        OperandLocation = activity2,
                        PropertyName = memberExpressionBody.Member.Name
                    };
                    result = reference4;
                }
                return null;
            }
            if (memberExpressionBody.Member is FieldInfo)
            {
                if (!isValueType)
                {
                    if (activity == null)
                    {
                        FieldReference<TOperand, TResult> reference5 = new FieldReference<TOperand, TResult> {
                            FieldName = memberExpressionBody.Member.Name
                        };
                        result = reference5;
                    }
                    else
                    {
                        FieldReference<TOperand, TResult> reference6 = new FieldReference<TOperand, TResult> {
                            Operand = activity,
                            FieldName = memberExpressionBody.Member.Name
                        };
                        result = reference6;
                    }
                }
                else if (activity2 == null)
                {
                    ValueTypeFieldReference<TOperand, TResult> reference7 = new ValueTypeFieldReference<TOperand, TResult> {
                        FieldName = memberExpressionBody.Member.Name
                    };
                    result = reference7;
                }
                else
                {
                    ValueTypeFieldReference<TOperand, TResult> reference8 = new ValueTypeFieldReference<TOperand, TResult> {
                        OperandLocation = activity2,
                        FieldName = memberExpressionBody.Member.Name
                    };
                    result = reference8;
                }
                return null;
            }
            if (throwOnError)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.UnsupportedMemberExpressionWithType(memberExpressionBody.Member.GetType().Name)));
            }
            return System.Activities.SR.UnsupportedMemberExpressionWithType(memberExpressionBody.Member.GetType().Name);
        }

        private static string TryConvertUnaryExpression<TResult>(UnaryExpression unaryExpressionBody, Type operandType, bool throwOnError, out Activity<TResult> result)
        {
            string str2;
            try
            {
                MethodInfo info = TryConvertUnaryExpressionHandle.MakeGenericMethod(new Type[] { operandType, typeof(TResult) });
                object[] objArray2 = new object[3];
                objArray2[0] = unaryExpressionBody;
                objArray2[1] = throwOnError;
                object[] parameters = objArray2;
                string str = info.Invoke(null, parameters) as string;
                result = parameters[2] as Activity<TResult>;
                str2 = str;
            }
            catch (TargetInvocationException exception)
            {
                throw FxTrace.Exception.AsError(exception.InnerException);
            }
            return str2;
        }

        private static string TryConvertUnaryExpressionWorker<TOperand, TResult>(UnaryExpression unaryExpressionBody, bool throwOnError, out Activity<TResult> result)
        {
            Activity<TOperand> activity;
            result = null;
            string str = TryConvert<TOperand>(unaryExpressionBody.Operand, throwOnError, out activity);
            if (str != null)
            {
                return str;
            }
            if (unaryExpressionBody.Method != null)
            {
                return TryConvertOverloadingUnaryOperator<TOperand, TResult>(unaryExpressionBody, activity, throwOnError, out result);
            }
            switch (unaryExpressionBody.NodeType)
            {
                case ExpressionType.Convert:
                {
                    Cast<TOperand, TResult> cast = new Cast<TOperand, TResult> {
                        Operand = activity,
                        Checked = false
                    };
                    result = cast;
                    break;
                }
                case ExpressionType.ConvertChecked:
                {
                    Cast<TOperand, TResult> cast2 = new Cast<TOperand, TResult> {
                        Operand = activity,
                        Checked = true
                    };
                    result = cast2;
                    break;
                }
                case ExpressionType.Not:
                {
                    Not<TOperand, TResult> not = new Not<TOperand, TResult> {
                        Operand = activity
                    };
                    result = not;
                    break;
                }
                case ExpressionType.TypeAs:
                {
                    As<TOperand, TResult> as2 = new As<TOperand, TResult> {
                        Operand = activity
                    };
                    result = as2;
                    break;
                }
                default:
                    if (throwOnError)
                    {
                        throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.UnsupportedExpressionType(unaryExpressionBody.NodeType)));
                    }
                    return System.Activities.SR.UnsupportedExpressionType(unaryExpressionBody.NodeType);
            }
            return null;
        }

        private static string TryConvertVariableReference<TResult>(MethodCallExpression methodCallExpression, bool throwOnError, out Activity<Location<TResult>> result)
        {
            result = null;
            Variable variable = null;
            if (methodCallExpression.Object is MemberExpression)
            {
                MemberExpression expression = methodCallExpression.Object as MemberExpression;
                if (expression.Expression is ConstantExpression)
                {
                    ConstantExpression expression2 = expression.Expression as ConstantExpression;
                    if (expression.Member is FieldInfo)
                    {
                        FieldInfo member = expression.Member as FieldInfo;
                        variable = member.GetValue(expression2.Value) as Variable;
                        VariableReference<TResult> reference = new VariableReference<TResult> {
                            Variable = variable
                        };
                        result = reference;
                        return null;
                    }
                }
            }
            try
            {
                Expression<Func<Variable>> expression3 = Expression.Lambda<Func<Variable>>(methodCallExpression.Object, new ParameterExpression[0]);
                variable = expression3.Compile()();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(exception);
                }
                return exception.Message;
            }
            VariableReference<TResult> reference2 = new VariableReference<TResult> {
                Variable = variable
            };
            result = reference2;
            return null;
        }

        private static string TryConvertVariableValue<TResult>(MethodCallExpression methodCallExpression, bool throwOnError, out Activity<TResult> result)
        {
            result = null;
            Variable variable = null;
            if (methodCallExpression.Object is MemberExpression)
            {
                MemberExpression expression = methodCallExpression.Object as MemberExpression;
                if (expression.Expression is ConstantExpression)
                {
                    ConstantExpression expression2 = expression.Expression as ConstantExpression;
                    if (expression.Member is FieldInfo)
                    {
                        FieldInfo member = expression.Member as FieldInfo;
                        variable = member.GetValue(expression2.Value) as Variable;
                        VariableValue<TResult> value2 = new VariableValue<TResult> {
                            Variable = variable
                        };
                        result = value2;
                        return null;
                    }
                }
            }
            try
            {
                Expression<Func<Variable>> expression3 = Expression.Lambda<Func<Variable>>(methodCallExpression.Object, new ParameterExpression[0]);
                variable = expression3.Compile()();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (throwOnError)
                {
                    throw FxTrace.Exception.AsError(exception);
                }
                return exception.Message;
            }
            VariableValue<TResult> value3 = new VariableValue<TResult> {
                Variable = variable
            };
            result = value3;
            return null;
        }
    }
}

