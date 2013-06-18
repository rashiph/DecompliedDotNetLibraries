namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    internal static class ExpressionUtilities
    {
        private static MethodInfo activityContextGetLocationGenericMethod = typeof(ActivityContext).GetMethod("GetLocation", new Type[] { typeof(LocationReference) });
        private static MethodInfo activityContextGetValueGenericMethod = typeof(ActivityContext).GetMethod("GetValue", new Type[] { typeof(LocationReference) });
        private static Type activityContextType = typeof(ActivityContext);
        private static MethodInfo argumentGetLocationMethod = typeof(Argument).GetMethod("GetLocation", new Type[] { typeof(ActivityContext) });
        private static Type argumentType = typeof(Argument);
        private static MethodInfo createLocationFactoryGenericMethod = typeof(ExpressionUtilities).GetMethod("CreateLocationFactory");
        private static MethodInfo delegateArgumentGetMethod = typeof(DelegateArgument).GetMethod("Get", new Type[] { typeof(ActivityContext) });
        private static Type delegateArgumentType = typeof(DelegateArgument);
        private static Type delegateInArgumentGenericType = typeof(DelegateInArgument<>);
        private static Type delegateOutArgumentGenericType = typeof(DelegateOutArgument<>);
        private static Type inArgumentGenericType = typeof(InArgument<>);
        private static Type inOutArgumentGenericType = typeof(InOutArgument<>);
        private static Assembly linqAssembly = typeof(Func<>).Assembly;
        private static MethodInfo locationReferenceGetLocationMethod = typeof(LocationReference).GetMethod("GetLocation", new Type[] { typeof(ActivityContext) });
        private static Type locationReferenceType = typeof(LocationReference);
        private static Type outArgumentGenericType = typeof(OutArgument<>);
        private static MethodInfo propertyDescriptorGetValue;
        private static Type runtimeArgumentType = typeof(RuntimeArgument);
        public static ParameterExpression RuntimeContextParameter = Expression.Parameter(typeof(ActivityContext), "context");
        private static Type variableGenericType = typeof(Variable<>);
        private static MethodInfo variableGetMethod = typeof(Variable).GetMethod("Get", new Type[] { typeof(ActivityContext) });
        private static Type variableType = typeof(Variable);

        private static Func<ActivityContext, T> Compile<T>(Expression objectExpression, ReadOnlyCollection<ParameterExpression> parametersCollection)
        {
            ParameterExpression[] parameters = null;
            if (parametersCollection != null)
            {
                parameters = parametersCollection.ToArray<ParameterExpression>();
            }
            return Expression.Lambda<Func<ActivityContext, T>>(objectExpression, parameters).Compile();
        }

        public static Expression CreateIdentifierExpression(LocationReference locationReference)
        {
            return Expression.Call(RuntimeContextParameter, activityContextGetValueGenericMethod.MakeGenericMethod(new Type[] { locationReference.Type }), new Expression[] { Expression.Constant(locationReference) });
        }

        public static LocationFactory<T> CreateLocationFactory<T>(LambdaExpression expression)
        {
            Expression body = expression.Body;
            switch (body.NodeType)
            {
                case ExpressionType.ArrayIndex:
                    return new ArrayLocationFactory<T>(expression);

                case ExpressionType.Call:
                {
                    MethodCallExpression expression3 = (MethodCallExpression) body;
                    MethodInfo method = expression3.Method;
                    Type declaringType = method.DeclaringType;
                    if (!(declaringType.BaseType == System.Runtime.TypeHelper.ArrayType) || !(method.Name == "Get"))
                    {
                        if (method.IsSpecialName && method.Name.StartsWith("get_", StringComparison.Ordinal))
                        {
                            return new IndexerLocationFactory<T>(expression);
                        }
                        if ((method.Name == "GetValue") && (declaringType == activityContextType))
                        {
                            return new LocationReferenceFactory<T>(expression3.Arguments[0], expression.Parameters);
                        }
                        if ((method.Name == "Get") && declaringType.IsGenericType)
                        {
                            Type genericTypeDefinition = declaringType.GetGenericTypeDefinition();
                            if ((genericTypeDefinition == inOutArgumentGenericType) || (genericTypeDefinition == outArgumentGenericType))
                            {
                                return new ArgumentFactory<T>(expression3.Object, expression.Parameters);
                            }
                        }
                        throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InvalidExpressionForLocation(body.NodeType)));
                    }
                    return new MultidimensionalArrayLocationFactory<T>(expression);
                }
                case ExpressionType.MemberAccess:
                {
                    MemberTypes memberType = ((MemberExpression) body).Member.MemberType;
                    switch (memberType)
                    {
                        case MemberTypes.Field:
                            return new FieldLocationFactory<T>(expression);

                        case MemberTypes.Property:
                            return new PropertyLocationFactory<T>(expression);
                    }
                    throw FxTrace.Exception.AsError(new NotSupportedException("Lvalues of member type " + memberType));
                }
            }
            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InvalidExpressionForLocation(body.NodeType)));
        }

        private static LocationFactory CreateParentReference(Expression expression, ReadOnlyCollection<ParameterExpression> lambdaParameters)
        {
            int count = lambdaParameters.Count;
            Type type = linqAssembly.GetType("System.Func`" + (count + 1), true);
            Type[] typeArguments = new Type[count + 1];
            for (int i = 0; i < count; i++)
            {
                typeArguments[i] = lambdaParameters[i].Type;
            }
            typeArguments[count] = expression.Type;
            LambdaExpression expression2 = Expression.Lambda(type.MakeGenericType(typeArguments), expression, lambdaParameters);
            return (LocationFactory) createLocationFactoryGenericMethod.MakeGenericMethod(new Type[] { expression.Type }).Invoke(null, new object[] { expression2 });
        }

        private static bool CustomMemberResolver(Expression expression, out object memberValue)
        {
            memberValue = null;
            ExpressionType nodeType = expression.NodeType;
            if (nodeType != ExpressionType.Constant)
            {
                if (nodeType != ExpressionType.MemberAccess)
                {
                    return false;
                }
            }
            else
            {
                ConstantExpression expression2 = expression as ConstantExpression;
                memberValue = expression2.Value;
                return (memberValue != null);
            }
            MemberExpression expression3 = expression as MemberExpression;
            if (expression3.Expression != null)
            {
                CustomMemberResolver(expression3.Expression, out memberValue);
                memberValue = GetMemberValue(expression3.Member, memberValue);
            }
            return (memberValue != null);
        }

        private static T Evaluate<T>(Expression objectExpression, ReadOnlyCollection<ParameterExpression> parametersCollection, ActivityContext context)
        {
            return Compile<T>(objectExpression, parametersCollection)(context);
        }

        private static object GetMemberValue(MemberInfo memberInfo, object owner)
        {
            if (owner != null)
            {
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Property:
                    {
                        PropertyInfo info = memberInfo as PropertyInfo;
                        return info.GetValue(owner, null);
                    }
                    case MemberTypes.Field:
                    {
                        FieldInfo info2 = memberInfo as FieldInfo;
                        return info2.GetValue(owner);
                    }
                }
            }
            return null;
        }

        public static bool IsLocation(LambdaExpression expression, Type targetType, out string extraErrorMessage)
        {
            extraErrorMessage = null;
            Expression body = expression.Body;
            if ((targetType != null) && (body.Type != targetType))
            {
                extraErrorMessage = System.Activities.SR.MustMatchReferenceExpressionReturnType;
                return false;
            }
            switch (body.NodeType)
            {
                case ExpressionType.ArrayIndex:
                    return true;

                case ExpressionType.Call:
                {
                    MethodCallExpression expression4 = (MethodCallExpression) body;
                    MethodInfo method = expression4.Method;
                    Type declaringType = method.DeclaringType;
                    if (!(declaringType.BaseType == System.Runtime.TypeHelper.ArrayType) || !(method.Name == "Get"))
                    {
                        if (method.IsSpecialName && method.Name.StartsWith("get_", StringComparison.Ordinal))
                        {
                            return true;
                        }
                        if ((method.Name == "GetValue") && (declaringType == activityContextType))
                        {
                            return true;
                        }
                        if ((method.Name == "Get") && declaringType.IsGenericType)
                        {
                            Type genericTypeDefinition = declaringType.GetGenericTypeDefinition();
                            if ((genericTypeDefinition == inOutArgumentGenericType) || (genericTypeDefinition == outArgumentGenericType))
                            {
                                return true;
                            }
                        }
                        break;
                    }
                    return true;
                }
                case ExpressionType.Convert:
                    extraErrorMessage = System.Activities.SR.MustMatchReferenceExpressionReturnType;
                    return false;

                case ExpressionType.MemberAccess:
                {
                    MemberExpression expression3 = (MemberExpression) body;
                    MemberTypes memberType = expression3.Member.MemberType;
                    if (memberType != MemberTypes.Field)
                    {
                        if (memberType != MemberTypes.Property)
                        {
                            break;
                        }
                        PropertyInfo info2 = (PropertyInfo) expression3.Member;
                        if (!info2.CanWrite)
                        {
                            return false;
                        }
                        return true;
                    }
                    FieldInfo member = (FieldInfo) expression3.Member;
                    if (member.IsInitOnly)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        private static bool TryGetInlinedArgumentReference(MethodCallExpression originalExpression, Expression argumentExpression, out LocationReference inlinedReference, CodeActivityMetadata metadata)
        {
            object obj2;
            inlinedReference = null;
            Argument argument = null;
            if (CustomMemberResolver(argumentExpression, out obj2) && (obj2 is Argument))
            {
                argument = (Argument) obj2;
            }
            else
            {
                try
                {
                    Expression<Func<Argument>> expression = Expression.Lambda<Func<Argument>>(argumentExpression, new ParameterExpression[0]);
                    argument = expression.Compile()();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    metadata.AddValidationError(System.Activities.SR.ErrorExtractingValuesForLambdaRewrite(argumentExpression.Type, originalExpression, exception));
                    return false;
                }
            }
            if (argument == null)
            {
                if (argumentExpression.NodeType == ExpressionType.MemberAccess)
                {
                    MemberExpression expression2 = (MemberExpression) argumentExpression;
                    if (expression2.Member.MemberType == MemberTypes.Property)
                    {
                        RuntimeArgument sourceReference = ActivityUtilities.FindArgument(expression2.Member.Name, metadata.CurrentActivity);
                        if ((sourceReference != null) && metadata.TryGetInlinedLocationReference(sourceReference, out inlinedReference))
                        {
                            return true;
                        }
                    }
                }
                metadata.AddValidationError(System.Activities.SR.ErrorExtractingValuesForLambdaRewrite(argumentExpression.Type, originalExpression, System.Activities.SR.SubexpressionResultWasNull(argumentExpression.Type)));
                return false;
            }
            if ((argument.RuntimeArgument != null) && metadata.TryGetInlinedLocationReference(argument.RuntimeArgument, out inlinedReference))
            {
                return true;
            }
            metadata.AddValidationError(System.Activities.SR.ErrorExtractingValuesForLambdaRewrite(argumentExpression.Type, originalExpression, System.Activities.SR.SubexpressionResultWasNotVisible(argumentExpression.Type)));
            return false;
        }

        private static bool TryGetInlinedLocationReference(MethodCallExpression originalExpression, Expression locationReferenceExpression, out LocationReference inlinedReference, CodeActivityMetadata metadata)
        {
            object obj2;
            inlinedReference = null;
            LocationReference sourceReference = null;
            if (CustomMemberResolver(locationReferenceExpression, out obj2) && (obj2 is LocationReference))
            {
                sourceReference = (LocationReference) obj2;
            }
            else
            {
                try
                {
                    Expression<Func<LocationReference>> expression = Expression.Lambda<Func<LocationReference>>(locationReferenceExpression, new ParameterExpression[0]);
                    sourceReference = expression.Compile()();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    metadata.AddValidationError(System.Activities.SR.ErrorExtractingValuesForLambdaRewrite(locationReferenceExpression.Type, originalExpression, exception));
                    return false;
                }
            }
            if (sourceReference == null)
            {
                metadata.AddValidationError(System.Activities.SR.ErrorExtractingValuesForLambdaRewrite(locationReferenceExpression.Type, originalExpression, System.Activities.SR.SubexpressionResultWasNull(locationReferenceExpression.Type)));
                return false;
            }
            if (!metadata.TryGetInlinedLocationReference(sourceReference, out inlinedReference))
            {
                metadata.AddValidationError(System.Activities.SR.ErrorExtractingValuesForLambdaRewrite(locationReferenceExpression.Type, originalExpression, System.Activities.SR.SubexpressionResultWasNotVisible(locationReferenceExpression.Type)));
                return false;
            }
            return true;
        }

        private static bool TryRewriteActivityContextGetLocationCall(MethodCallExpression originalExpression, Type returnType, out Expression newExpression, CodeActivityMetadata metadata)
        {
            ReadOnlyCollection<Expression> arguments = originalExpression.Arguments;
            if (arguments.Count == 1)
            {
                LocationReference reference;
                Expression expression = arguments[0];
                if (System.Runtime.TypeHelper.AreTypesCompatible(expression.Type, locationReferenceType) && TryGetInlinedLocationReference(originalExpression, originalExpression.Arguments[0], out reference, metadata))
                {
                    newExpression = Expression.Call(originalExpression.Object, activityContextGetLocationGenericMethod.MakeGenericMethod(new Type[] { returnType }), new Expression[] { Expression.Constant(reference) });
                    return true;
                }
            }
            newExpression = originalExpression;
            return false;
        }

        private static bool TryRewriteActivityContextGetValueCall(MethodCallExpression originalExpression, Type returnType, out Expression newExpression, CodeActivityMetadata metadata)
        {
            newExpression = originalExpression;
            LocationReference inlinedReference = null;
            ReadOnlyCollection<Expression> arguments = originalExpression.Arguments;
            if (arguments.Count == 1)
            {
                Expression argumentExpression = arguments[0];
                if (System.Runtime.TypeHelper.AreTypesCompatible(argumentExpression.Type, typeof(Argument)))
                {
                    if (!TryGetInlinedArgumentReference(originalExpression, argumentExpression, out inlinedReference, metadata))
                    {
                        metadata.AddValidationError(System.Activities.SR.ErrorExtractingValuesForLambdaRewrite(argumentExpression.Type, originalExpression, System.Activities.SR.SubexpressionResultWasNotVisible(argumentExpression.Type)));
                        return false;
                    }
                }
                else if (System.Runtime.TypeHelper.AreTypesCompatible(argumentExpression.Type, typeof(LocationReference)) && !TryGetInlinedLocationReference(originalExpression, argumentExpression, out inlinedReference, metadata))
                {
                    metadata.AddValidationError(System.Activities.SR.ErrorExtractingValuesForLambdaRewrite(argumentExpression.Type, originalExpression, System.Activities.SR.SubexpressionResultWasNotVisible(argumentExpression.Type)));
                    return false;
                }
            }
            if (inlinedReference != null)
            {
                newExpression = Expression.Call(originalExpression.Object, activityContextGetValueGenericMethod.MakeGenericMethod(new Type[] { returnType }), new Expression[] { Expression.Constant(inlinedReference) });
                return true;
            }
            return false;
        }

        private static bool TryRewriteArgumentGetCall(MethodCallExpression originalExpression, Type returnType, out Expression newExpression, CodeActivityMetadata metadata)
        {
            ReadOnlyCollection<Expression> arguments = originalExpression.Arguments;
            if (arguments.Count == 1)
            {
                LocationReference reference;
                Expression instance = arguments[0];
                if ((instance.Type == activityContextType) && TryGetInlinedArgumentReference(originalExpression, originalExpression.Object, out reference, metadata))
                {
                    newExpression = Expression.Call(instance, activityContextGetValueGenericMethod.MakeGenericMethod(new Type[] { returnType }), new Expression[] { Expression.Constant(reference) });
                    return true;
                }
            }
            newExpression = originalExpression;
            return false;
        }

        private static bool TryRewriteArgumentGetLocationCall(MethodCallExpression originalExpression, Type returnType, out Expression newExpression, CodeActivityMetadata metadata)
        {
            ReadOnlyCollection<Expression> arguments = originalExpression.Arguments;
            if (arguments.Count == 1)
            {
                LocationReference reference;
                Expression instance = arguments[0];
                if ((instance.Type == activityContextType) && TryGetInlinedArgumentReference(originalExpression, originalExpression.Object, out reference, metadata))
                {
                    if (returnType == null)
                    {
                        newExpression = Expression.Call(Expression.Constant(reference), locationReferenceGetLocationMethod, new Expression[] { instance });
                    }
                    else
                    {
                        newExpression = Expression.Call(instance, activityContextGetLocationGenericMethod.MakeGenericMethod(new Type[] { returnType }), new Expression[] { Expression.Constant(reference) });
                    }
                    return true;
                }
            }
            newExpression = originalExpression;
            return false;
        }

        public static bool TryRewriteLambdaExpression(Expression expression, out Expression newExpression, CodeActivityMetadata metadata)
        {
            newExpression = expression;
            if (expression == null)
            {
                return false;
            }
            Expression expression2 = null;
            Expression expression3 = null;
            Expression expression4 = null;
            bool flag = false;
            IList<Expression> newExpressions = null;
            IList<ElementInit> newInitializers = null;
            IList<MemberBinding> newBindings = null;
            MethodCallExpression methodCall = null;
            BinaryExpression expression6 = null;
            NewArrayExpression expression7 = null;
            UnaryExpression expression8 = null;
            switch (expression.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Coalesce:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    expression6 = (BinaryExpression) expression;
                    flag |= TryRewriteLambdaExpression(expression6.Left, out expression2, metadata);
                    flag |= TryRewriteLambdaExpression(expression6.Right, out expression3, metadata);
                    flag |= TryRewriteLambdaExpression(expression6.Conversion, out expression4, metadata);
                    if (flag)
                    {
                        newExpression = Expression.MakeBinary(expression6.NodeType, expression2, expression3, expression6.IsLiftedToNull, expression6.Method, (LambdaExpression) expression4);
                    }
                    return flag;

                case ExpressionType.ArrayLength:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    expression8 = (UnaryExpression) expression;
                    flag |= TryRewriteLambdaExpression(expression8.Operand, out expression2, metadata);
                    if (flag)
                    {
                        newExpression = Expression.MakeUnary(expression8.NodeType, expression2, expression8.Type, expression8.Method);
                    }
                    return flag;

                case ExpressionType.ArrayIndex:
                    methodCall = expression as MethodCallExpression;
                    if (methodCall == null)
                    {
                        expression6 = (BinaryExpression) expression;
                        flag |= TryRewriteLambdaExpression(expression6.Left, out expression2, metadata);
                        flag |= TryRewriteLambdaExpression(expression6.Right, out expression3, metadata);
                        if (flag)
                        {
                            newExpression = Expression.ArrayIndex(expression2, expression3);
                        }
                        return flag;
                    }
                    flag |= TryRewriteLambdaExpression(methodCall.Object, out expression4, metadata);
                    flag |= TryRewriteLambdaExpressionCollection(methodCall.Arguments, out newExpressions, metadata);
                    if (flag)
                    {
                        newExpression = Expression.ArrayIndex(expression4, newExpressions);
                    }
                    return flag;

                case ExpressionType.Call:
                    methodCall = (MethodCallExpression) expression;
                    return TryRewriteMethodCall(methodCall, out newExpression, metadata);

                case ExpressionType.Conditional:
                {
                    ConditionalExpression expression9 = (ConditionalExpression) expression;
                    flag |= TryRewriteLambdaExpression(expression9.Test, out expression4, metadata);
                    flag |= TryRewriteLambdaExpression(expression9.IfTrue, out expression2, metadata);
                    flag |= TryRewriteLambdaExpression(expression9.IfFalse, out expression3, metadata);
                    if (flag)
                    {
                        newExpression = Expression.Condition(expression4, expression2, expression3);
                    }
                    return flag;
                }
                case ExpressionType.Constant:
                case ExpressionType.Parameter:
                    return flag;

                case ExpressionType.Invoke:
                {
                    InvocationExpression expression10 = (InvocationExpression) expression;
                    flag |= TryRewriteLambdaExpression(expression10.Expression, out expression4, metadata);
                    flag |= TryRewriteLambdaExpressionCollection(expression10.Arguments, out newExpressions, metadata);
                    if (flag)
                    {
                        newExpression = Expression.Invoke(expression4, newExpressions);
                    }
                    return flag;
                }
                case ExpressionType.Lambda:
                {
                    LambdaExpression expression11 = (LambdaExpression) expression;
                    flag |= TryRewriteLambdaExpression(expression11.Body, out expression4, metadata);
                    if (flag)
                    {
                        newExpression = Expression.Lambda(expression11.Type, expression4, expression11.Parameters);
                    }
                    return flag;
                }
                case ExpressionType.ListInit:
                {
                    ListInitExpression expression12 = (ListInitExpression) expression;
                    flag |= TryRewriteLambdaExpression(expression12.NewExpression, out expression4, metadata);
                    flag |= TryRewriteLambdaExpressionInitializersCollection(expression12.Initializers, out newInitializers, metadata);
                    if (flag)
                    {
                        newExpression = Expression.ListInit((NewExpression) expression4, newInitializers);
                    }
                    return flag;
                }
                case ExpressionType.MemberAccess:
                {
                    MemberExpression expression13 = (MemberExpression) expression;
                    flag |= TryRewriteLambdaExpression(expression13.Expression, out expression4, metadata);
                    if (flag)
                    {
                        newExpression = Expression.MakeMemberAccess(expression4, expression13.Member);
                    }
                    return flag;
                }
                case ExpressionType.MemberInit:
                {
                    MemberInitExpression expression14 = (MemberInitExpression) expression;
                    flag |= TryRewriteLambdaExpression(expression14.NewExpression, out expression4, metadata);
                    flag |= TryRewriteLambdaExpressionBindingsCollection(expression14.Bindings, out newBindings, metadata);
                    if (flag)
                    {
                        newExpression = Expression.MemberInit((NewExpression) expression4, newBindings);
                    }
                    return flag;
                }
                case ExpressionType.UnaryPlus:
                    expression8 = (UnaryExpression) expression;
                    flag |= TryRewriteLambdaExpression(expression8.Operand, out expression2, metadata);
                    if (flag)
                    {
                        newExpression = Expression.UnaryPlus(expression2, expression8.Method);
                    }
                    return flag;

                case ExpressionType.New:
                {
                    NewExpression expression15 = (NewExpression) expression;
                    if (expression15.Constructor != null)
                    {
                        flag |= TryRewriteLambdaExpressionCollection(expression15.Arguments, out newExpressions, metadata);
                        if (flag)
                        {
                            newExpression = Expression.New(expression15.Constructor, newExpressions);
                        }
                    }
                    return flag;
                }
                case ExpressionType.NewArrayInit:
                    expression7 = (NewArrayExpression) expression;
                    flag |= TryRewriteLambdaExpressionCollection(expression7.Expressions, out newExpressions, metadata);
                    if (flag)
                    {
                        newExpression = Expression.NewArrayInit(expression7.Type.GetElementType(), newExpressions);
                    }
                    return flag;

                case ExpressionType.NewArrayBounds:
                    expression7 = (NewArrayExpression) expression;
                    flag |= TryRewriteLambdaExpressionCollection(expression7.Expressions, out newExpressions, metadata);
                    if (flag)
                    {
                        newExpression = Expression.NewArrayBounds(expression7.Type.GetElementType(), newExpressions);
                    }
                    return flag;

                case ExpressionType.TypeIs:
                {
                    TypeBinaryExpression expression16 = (TypeBinaryExpression) expression;
                    flag |= TryRewriteLambdaExpression(expression16.Expression, out expression4, metadata);
                    if (flag)
                    {
                        newExpression = Expression.TypeIs(expression4, expression16.TypeOperand);
                    }
                    return flag;
                }
                case ExpressionType.Assign:
                    expression6 = (BinaryExpression) expression;
                    flag |= TryRewriteLambdaExpression(expression6.Left, out expression2, metadata);
                    flag |= TryRewriteLambdaExpression(expression6.Right, out expression3, metadata);
                    if (flag)
                    {
                        newExpression = Expression.Assign(expression2, expression3);
                    }
                    return flag;

                case ExpressionType.Block:
                {
                    BlockExpression expression17 = (BlockExpression) expression;
                    flag |= TryRewriteLambdaExpressionCollection(expression17.Expressions, out newExpressions, metadata);
                    if (flag)
                    {
                        newExpression = Expression.Block((IEnumerable<ParameterExpression>) expression17.Variables, (IEnumerable<Expression>) newExpressions);
                    }
                    return flag;
                }
            }
            return flag;
        }

        private static bool TryRewriteLambdaExpressionBindingsCollection(IList<MemberBinding> bindings, out IList<MemberBinding> newBindings, CodeActivityMetadata metadata)
        {
            IList<MemberBinding> list = null;
            for (int i = 0; i < bindings.Count; i++)
            {
                MemberBinding binding2;
                MemberBinding binding = bindings[i];
                if (TryRewriteMemberBinding(binding, out binding2, metadata) && (list == null))
                {
                    list = new List<MemberBinding>(bindings.Count);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(bindings[j]);
                    }
                }
                if (list != null)
                {
                    list.Add(binding2);
                }
            }
            if (list != null)
            {
                newBindings = list;
                return true;
            }
            newBindings = bindings;
            return false;
        }

        private static bool TryRewriteLambdaExpressionCollection(IList<Expression> expressions, out IList<Expression> newExpressions, CodeActivityMetadata metadata)
        {
            IList<Expression> list = null;
            for (int i = 0; i < expressions.Count; i++)
            {
                Expression expression2;
                Expression expression = expressions[i];
                if (TryRewriteLambdaExpression(expression, out expression2, metadata) && (list == null))
                {
                    list = new List<Expression>(expressions.Count);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(expressions[j]);
                    }
                }
                if (list != null)
                {
                    list.Add(expression2);
                }
            }
            if (list != null)
            {
                newExpressions = list;
                return true;
            }
            newExpressions = expressions;
            return false;
        }

        private static bool TryRewriteLambdaExpressionInitializersCollection(IList<ElementInit> initializers, out IList<ElementInit> newInitializers, CodeActivityMetadata metadata)
        {
            IList<ElementInit> list = null;
            for (int i = 0; i < initializers.Count; i++)
            {
                IList<Expression> list2;
                ElementInit item = initializers[i];
                if (TryRewriteLambdaExpressionCollection(item.Arguments, out list2, metadata))
                {
                    if (list == null)
                    {
                        list = new List<ElementInit>(initializers.Count);
                        for (int j = 0; j < i; j++)
                        {
                            list.Add(initializers[j]);
                        }
                    }
                    item = Expression.ElementInit(item.AddMethod, list2);
                }
                if (list != null)
                {
                    list.Add(item);
                }
            }
            if (list != null)
            {
                newInitializers = list;
                return true;
            }
            newInitializers = initializers;
            return false;
        }

        private static bool TryRewriteLocationReferenceSubclassGetCall(MethodCallExpression originalExpression, Type returnType, out Expression newExpression, CodeActivityMetadata metadata)
        {
            ReadOnlyCollection<Expression> arguments = originalExpression.Arguments;
            if (arguments.Count == 1)
            {
                LocationReference reference;
                Expression instance = arguments[0];
                if ((instance.Type == activityContextType) && TryGetInlinedLocationReference(originalExpression, originalExpression.Object, out reference, metadata))
                {
                    newExpression = Expression.Call(instance, activityContextGetValueGenericMethod.MakeGenericMethod(new Type[] { returnType }), new Expression[] { Expression.Constant(reference) });
                    return true;
                }
            }
            newExpression = originalExpression;
            return false;
        }

        private static bool TryRewriteLocationReferenceSubclassGetLocationCall(MethodCallExpression originalExpression, Type returnType, out Expression newExpression, CodeActivityMetadata metadata)
        {
            ReadOnlyCollection<Expression> arguments = originalExpression.Arguments;
            if (arguments.Count == 1)
            {
                LocationReference reference;
                Expression instance = arguments[0];
                if ((instance.Type == activityContextType) && TryGetInlinedLocationReference(originalExpression, originalExpression.Object, out reference, metadata))
                {
                    if (returnType == null)
                    {
                        newExpression = Expression.Call(Expression.Constant(reference), locationReferenceGetLocationMethod, new Expression[] { originalExpression.Arguments[0] });
                    }
                    else
                    {
                        newExpression = Expression.Call(instance, activityContextGetLocationGenericMethod.MakeGenericMethod(new Type[] { returnType }), new Expression[] { Expression.Constant(reference) });
                    }
                    return true;
                }
            }
            newExpression = originalExpression;
            return false;
        }

        private static bool TryRewriteMemberBinding(MemberBinding binding, out MemberBinding newBinding, CodeActivityMetadata metadata)
        {
            newBinding = binding;
            bool flag = false;
            Expression newExpression = null;
            IList<ElementInit> newInitializers = null;
            IList<MemberBinding> newBindings = null;
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                {
                    MemberAssignment assignment = (MemberAssignment) binding;
                    flag |= TryRewriteLambdaExpression(assignment.Expression, out newExpression, metadata);
                    if (flag)
                    {
                        newBinding = Expression.Bind(assignment.Member, newExpression);
                    }
                    return flag;
                }
                case MemberBindingType.MemberBinding:
                {
                    MemberMemberBinding binding3 = (MemberMemberBinding) binding;
                    flag |= TryRewriteLambdaExpressionBindingsCollection(binding3.Bindings, out newBindings, metadata);
                    if (flag)
                    {
                        newBinding = Expression.MemberBind(binding3.Member, newBindings);
                    }
                    return flag;
                }
                case MemberBindingType.ListBinding:
                {
                    MemberListBinding binding2 = (MemberListBinding) binding;
                    flag |= TryRewriteLambdaExpressionInitializersCollection(binding2.Initializers, out newInitializers, metadata);
                    if (flag)
                    {
                        newBinding = Expression.ListBind(binding2.Member, newInitializers);
                    }
                    return flag;
                }
            }
            return flag;
        }

        private static bool TryRewriteMethodCall(MethodCallExpression methodCall, out Expression newExpression, CodeActivityMetadata metadata)
        {
            Expression expression;
            IList<Expression> list;
            MethodInfo method = methodCall.Method;
            Type declaringType = method.DeclaringType;
            if (declaringType.IsGenericType)
            {
                Type genericTypeDefinition = declaringType.GetGenericTypeDefinition();
                if (genericTypeDefinition != variableGenericType)
                {
                    if (genericTypeDefinition != inArgumentGenericType)
                    {
                        if ((genericTypeDefinition != outArgumentGenericType) && (genericTypeDefinition != inOutArgumentGenericType))
                        {
                            if (genericTypeDefinition != delegateInArgumentGenericType)
                            {
                                if (genericTypeDefinition == delegateOutArgumentGenericType)
                                {
                                    if (method.Name == "Get")
                                    {
                                        return TryRewriteLocationReferenceSubclassGetCall(methodCall, declaringType.GetGenericArguments()[0], out newExpression, metadata);
                                    }
                                    if (method.Name == "GetLocation")
                                    {
                                        return TryRewriteLocationReferenceSubclassGetLocationCall(methodCall, declaringType.GetGenericArguments()[0], out newExpression, metadata);
                                    }
                                }
                            }
                            else if (method.Name == "Get")
                            {
                                return TryRewriteLocationReferenceSubclassGetCall(methodCall, declaringType.GetGenericArguments()[0], out newExpression, metadata);
                            }
                        }
                        else
                        {
                            if (method.Name == "Get")
                            {
                                return TryRewriteArgumentGetCall(methodCall, declaringType.GetGenericArguments()[0], out newExpression, metadata);
                            }
                            if (method.Name == "GetLocation")
                            {
                                return TryRewriteArgumentGetLocationCall(methodCall, declaringType.GetGenericArguments()[0], out newExpression, metadata);
                            }
                        }
                    }
                    else if (method.Name == "Get")
                    {
                        return TryRewriteArgumentGetCall(methodCall, declaringType.GetGenericArguments()[0], out newExpression, metadata);
                    }
                }
                else
                {
                    if (method.Name == "Get")
                    {
                        return TryRewriteLocationReferenceSubclassGetCall(methodCall, declaringType.GetGenericArguments()[0], out newExpression, metadata);
                    }
                    if (method.Name == "GetLocation")
                    {
                        return TryRewriteLocationReferenceSubclassGetLocationCall(methodCall, declaringType.GetGenericArguments()[0], out newExpression, metadata);
                    }
                }
            }
            else if (declaringType == variableType)
            {
                if (object.ReferenceEquals(method, variableGetMethod))
                {
                    return TryRewriteLocationReferenceSubclassGetCall(methodCall, System.Runtime.TypeHelper.ObjectType, out newExpression, metadata);
                }
            }
            else if (declaringType == delegateArgumentType)
            {
                if (object.ReferenceEquals(method, delegateArgumentGetMethod))
                {
                    return TryRewriteLocationReferenceSubclassGetCall(methodCall, System.Runtime.TypeHelper.ObjectType, out newExpression, metadata);
                }
            }
            else if (declaringType == activityContextType)
            {
                if (method.Name == "GetValue")
                {
                    Type objectType = System.Runtime.TypeHelper.ObjectType;
                    if (method.IsGenericMethod)
                    {
                        objectType = method.GetGenericArguments()[0];
                    }
                    return TryRewriteActivityContextGetValueCall(methodCall, objectType, out newExpression, metadata);
                }
                if (method.IsGenericMethod && (method.Name == "GetLocation"))
                {
                    return TryRewriteActivityContextGetLocationCall(methodCall, method.GetGenericArguments()[0], out newExpression, metadata);
                }
            }
            else if (declaringType == locationReferenceType)
            {
                if (object.ReferenceEquals(method, locationReferenceGetLocationMethod))
                {
                    return TryRewriteLocationReferenceSubclassGetLocationCall(methodCall, null, out newExpression, metadata);
                }
            }
            else if (declaringType == runtimeArgumentType)
            {
                if (method.Name == "Get")
                {
                    Type returnType = System.Runtime.TypeHelper.ObjectType;
                    if (method.IsGenericMethod)
                    {
                        returnType = method.GetGenericArguments()[0];
                    }
                    return TryRewriteLocationReferenceSubclassGetCall(methodCall, returnType, out newExpression, metadata);
                }
            }
            else if (declaringType == argumentType)
            {
                if (method.Name == "Get")
                {
                    Type type5 = System.Runtime.TypeHelper.ObjectType;
                    if (method.IsGenericMethod)
                    {
                        type5 = method.GetGenericArguments()[0];
                    }
                    return TryRewriteArgumentGetCall(methodCall, type5, out newExpression, metadata);
                }
                if (object.ReferenceEquals(method, argumentGetLocationMethod))
                {
                    return TryRewriteArgumentGetLocationCall(methodCall, null, out newExpression, metadata);
                }
            }
            newExpression = methodCall;
            bool flag = TryRewriteLambdaExpression(methodCall.Object, out expression, metadata) | TryRewriteLambdaExpressionCollection(methodCall.Arguments, out list, metadata);
            if (flag)
            {
                newExpression = Expression.Call(expression, method, list);
            }
            return flag;
        }

        private static MethodInfo PropertyDescriptorGetValue
        {
            get
            {
                if (propertyDescriptorGetValue == null)
                {
                    propertyDescriptorGetValue = typeof(PropertyDescriptor).GetMethod("GetValue");
                }
                return propertyDescriptorGetValue;
            }
        }

        private class ArgumentFactory<T> : LocationFactory<T>
        {
            private Func<ActivityContext, Argument> argumentFunction;

            public ArgumentFactory(Expression argumentExpression, ReadOnlyCollection<ParameterExpression> expressionParameters)
            {
                this.argumentFunction = ExpressionUtilities.Compile<Argument>(argumentExpression, expressionParameters);
            }

            public override Location<T> CreateLocation(ActivityContext context)
            {
                return (this.argumentFunction(context).RuntimeArgument.GetLocation(context) as Location<T>);
            }
        }

        private class ArrayLocationFactory<T> : LocationFactory<T>
        {
            private Func<ActivityContext, T[]> arrayFunction;
            private Func<ActivityContext, int> indexFunction;

            public ArrayLocationFactory(LambdaExpression expression)
            {
                BinaryExpression body = (BinaryExpression) expression.Body;
                this.arrayFunction = ExpressionUtilities.Compile<T[]>(body.Left, expression.Parameters);
                this.indexFunction = ExpressionUtilities.Compile<int>(body.Right, expression.Parameters);
            }

            public override Location<T> CreateLocation(ActivityContext context)
            {
                return new ArrayLocation<T>(this.arrayFunction(context), this.indexFunction(context));
            }

            [DataContract]
            private class ArrayLocation : Location<T>
            {
                [DataMember]
                private T[] array;
                [DataMember(EmitDefaultValue=false)]
                private int index;

                public ArrayLocation(T[] array, int index)
                {
                    this.array = array;
                    this.index = index;
                }

                public override T Value
                {
                    get
                    {
                        return this.array[this.index];
                    }
                    set
                    {
                        this.array[this.index] = value;
                    }
                }
            }
        }

        private class FieldLocationFactory<T> : LocationFactory<T>
        {
            private FieldInfo fieldInfo;
            private Func<ActivityContext, object> ownerFunction;
            private LocationFactory parentFactory;

            public FieldLocationFactory(LambdaExpression expression)
            {
                MemberExpression body = (MemberExpression) expression.Body;
                this.fieldInfo = (FieldInfo) body.Member;
                if (this.fieldInfo.IsStatic)
                {
                    this.ownerFunction = null;
                }
                else
                {
                    this.ownerFunction = ExpressionUtilities.Compile<object>(Expression.Convert(body.Expression, System.Runtime.TypeHelper.ObjectType), expression.Parameters);
                }
                if (this.fieldInfo.DeclaringType.IsValueType)
                {
                    this.parentFactory = ExpressionUtilities.CreateParentReference(body.Expression, expression.Parameters);
                }
            }

            public override Location<T> CreateLocation(ActivityContext context)
            {
                object owner = null;
                if (this.ownerFunction != null)
                {
                    owner = this.ownerFunction(context);
                }
                System.Activities.Location parent = null;
                if (this.parentFactory != null)
                {
                    parent = this.parentFactory.CreateLocation(context);
                }
                return new FieldLocation<T>(this.fieldInfo, owner, parent);
            }

            [DataContract]
            private class FieldLocation : Location<T>
            {
                [DataMember]
                private FieldInfo fieldInfo;
                [DataMember(EmitDefaultValue=false)]
                private object owner;
                [DataMember(EmitDefaultValue=false)]
                private System.Activities.Location parent;

                public FieldLocation(FieldInfo fieldInfo, object owner, System.Activities.Location parent)
                {
                    this.fieldInfo = fieldInfo;
                    this.owner = owner;
                    this.parent = parent;
                }

                public override T Value
                {
                    get
                    {
                        if ((this.owner == null) && !this.fieldInfo.IsStatic)
                        {
                            throw FxTrace.Exception.AsError(new NullReferenceException(System.Activities.SR.CannotDereferenceNull(this.fieldInfo.Name)));
                        }
                        return (T) this.fieldInfo.GetValue(this.owner);
                    }
                    set
                    {
                        if ((this.owner == null) && !this.fieldInfo.IsStatic)
                        {
                            throw FxTrace.Exception.AsError(new NullReferenceException(System.Activities.SR.CannotDereferenceNull(this.fieldInfo.Name)));
                        }
                        this.fieldInfo.SetValue(this.owner, value);
                        if (this.parent != null)
                        {
                            this.parent.Value = this.owner;
                        }
                    }
                }
            }
        }

        private class IndexerLocationFactory<T> : LocationFactory<T>
        {
            private MethodInfo getItemMethod;
            private string indexerName;
            private Func<ActivityContext, object>[] setItemArgumentFunctions;
            private MethodInfo setItemMethod;
            private Func<ActivityContext, object> targetObjectFunction;

            public IndexerLocationFactory(LambdaExpression expression)
            {
                MethodCallExpression body = (MethodCallExpression) expression.Body;
                this.getItemMethod = body.Method;
                this.indexerName = this.getItemMethod.Name.Substring(4);
                string name = "set_" + this.indexerName;
                ParameterInfo[] parameters = this.getItemMethod.GetParameters();
                Type[] types = new Type[parameters.Length + 1];
                for (int i = 0; i < parameters.Length; i++)
                {
                    types[i] = parameters[i].ParameterType;
                }
                types[parameters.Length] = this.getItemMethod.ReturnType;
                this.setItemMethod = this.getItemMethod.DeclaringType.GetMethod(name, BindingFlags.Public | BindingFlags.Instance, null, types, null);
                if (this.setItemMethod != null)
                {
                    this.targetObjectFunction = ExpressionUtilities.Compile<object>(body.Object, expression.Parameters);
                    this.setItemArgumentFunctions = new Func<ActivityContext, object>[body.Arguments.Count];
                    for (int j = 0; j < body.Arguments.Count; j++)
                    {
                        Expression expression3 = body.Arguments[j];
                        if (expression3.Type.IsValueType)
                        {
                            expression3 = Expression.Convert(expression3, System.Runtime.TypeHelper.ObjectType);
                        }
                        this.setItemArgumentFunctions[j] = ExpressionUtilities.Compile<object>(expression3, expression.Parameters);
                    }
                }
            }

            public override Location<T> CreateLocation(ActivityContext context)
            {
                object targetObject = null;
                object[] getItemArguments = null;
                if (this.setItemMethod != null)
                {
                    targetObject = this.targetObjectFunction(context);
                    getItemArguments = new object[this.setItemArgumentFunctions.Length];
                    for (int i = 0; i < this.setItemArgumentFunctions.Length; i++)
                    {
                        getItemArguments[i] = this.setItemArgumentFunctions[i](context);
                    }
                }
                return new IndexerLocation<T>(this.indexerName, this.getItemMethod, this.setItemMethod, targetObject, getItemArguments);
            }

            [DataContract]
            private class IndexerLocation : Location<T>
            {
                [DataMember(EmitDefaultValue=false)]
                private MethodInfo getItemMethod;
                [DataMember]
                private string indexerName;
                [DataMember(EmitDefaultValue=false)]
                private object[] setItemArguments;
                [DataMember(EmitDefaultValue=false)]
                private MethodInfo setItemMethod;
                [DataMember(EmitDefaultValue=false)]
                private object targetObject;

                public IndexerLocation(string indexerName, MethodInfo getItemMethod, MethodInfo setItemMethod, object targetObject, object[] getItemArguments)
                {
                    this.indexerName = indexerName;
                    this.getItemMethod = getItemMethod;
                    this.setItemMethod = setItemMethod;
                    this.targetObject = targetObject;
                    this.setItemArguments = getItemArguments;
                }

                public override T Value
                {
                    get
                    {
                        if ((this.targetObject == null) && !this.getItemMethod.IsStatic)
                        {
                            throw FxTrace.Exception.AsError(new NullReferenceException(System.Activities.SR.CannotDereferenceNull(this.getItemMethod.Name)));
                        }
                        return (T) this.getItemMethod.Invoke(this.targetObject, this.setItemArguments);
                    }
                    set
                    {
                        if (this.setItemMethod == null)
                        {
                            string name = this.targetObject.GetType().Name;
                            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.MissingSetAccessorForIndexer(this.indexerName, name)));
                        }
                        if ((this.targetObject == null) && !this.setItemMethod.IsStatic)
                        {
                            throw FxTrace.Exception.AsError(new NullReferenceException(System.Activities.SR.CannotDereferenceNull(this.setItemMethod.Name)));
                        }
                        object[] destinationArray = new object[this.setItemArguments.Length + 1];
                        Array.ConstrainedCopy(this.setItemArguments, 0, destinationArray, 0, this.setItemArguments.Length);
                        destinationArray[destinationArray.Length - 1] = value;
                        this.setItemMethod.Invoke(this.targetObject, destinationArray);
                    }
                }
            }
        }

        private class LocationReferenceFactory<T> : LocationFactory<T>
        {
            private Func<ActivityContext, LocationReference> locationReferenceFunction;

            public LocationReferenceFactory(Expression locationReferenceExpression, ReadOnlyCollection<ParameterExpression> expressionParameters)
            {
                this.locationReferenceFunction = ExpressionUtilities.Compile<LocationReference>(locationReferenceExpression, expressionParameters);
            }

            public override Location<T> CreateLocation(ActivityContext context)
            {
                return (this.locationReferenceFunction(context).GetLocation(context) as Location<T>);
            }
        }

        private class MultidimensionalArrayLocationFactory<T> : LocationFactory<T>
        {
            private Func<ActivityContext, Array> arrayFunction;
            private Func<ActivityContext, int>[] indexFunctions;

            public MultidimensionalArrayLocationFactory(LambdaExpression expression)
            {
                MethodCallExpression body = (MethodCallExpression) expression.Body;
                this.arrayFunction = ExpressionUtilities.Compile<Array>(body.Object, expression.Parameters);
                this.indexFunctions = new Func<ActivityContext, int>[body.Arguments.Count];
                for (int i = 0; i < this.indexFunctions.Length; i++)
                {
                    this.indexFunctions[i] = ExpressionUtilities.Compile<int>(body.Arguments[i], expression.Parameters);
                }
            }

            public override Location<T> CreateLocation(ActivityContext context)
            {
                int[] indices = new int[this.indexFunctions.Length];
                for (int i = 0; i < indices.Length; i++)
                {
                    indices[i] = this.indexFunctions[i](context);
                }
                return new MultidimensionalArrayLocation<T>(this.arrayFunction(context), indices);
            }

            [DataContract]
            private class MultidimensionalArrayLocation : Location<T>
            {
                [DataMember]
                private Array array;
                [DataMember]
                private int[] indices;

                public MultidimensionalArrayLocation(Array array, int[] indices)
                {
                    this.array = array;
                    this.indices = indices;
                }

                public override T Value
                {
                    get
                    {
                        return (T) this.array.GetValue(this.indices);
                    }
                    set
                    {
                        this.array.SetValue(value, this.indices);
                    }
                }
            }
        }

        private class PropertyLocationFactory<T> : LocationFactory<T>
        {
            private Func<ActivityContext, object> ownerFunction;
            private LocationFactory parentFactory;
            private PropertyInfo propertyInfo;

            public PropertyLocationFactory(LambdaExpression expression)
            {
                MemberExpression body = (MemberExpression) expression.Body;
                this.propertyInfo = (PropertyInfo) body.Member;
                if (body.Expression == null)
                {
                    this.ownerFunction = null;
                }
                else
                {
                    this.ownerFunction = ExpressionUtilities.Compile<object>(Expression.Convert(body.Expression, System.Runtime.TypeHelper.ObjectType), expression.Parameters);
                }
                if (this.propertyInfo.DeclaringType.IsValueType)
                {
                    this.parentFactory = ExpressionUtilities.CreateParentReference(body.Expression, expression.Parameters);
                }
            }

            public override Location<T> CreateLocation(ActivityContext context)
            {
                object owner = null;
                if (this.ownerFunction != null)
                {
                    owner = this.ownerFunction(context);
                }
                System.Activities.Location parent = null;
                if (this.parentFactory != null)
                {
                    parent = this.parentFactory.CreateLocation(context);
                }
                return new PropertyLocation<T>(this.propertyInfo, owner, parent);
            }

            [DataContract]
            private class PropertyLocation : Location<T>
            {
                [DataMember(EmitDefaultValue=false)]
                private object owner;
                [DataMember(EmitDefaultValue=false)]
                private System.Activities.Location parent;
                [DataMember]
                private PropertyInfo propertyInfo;

                public PropertyLocation(PropertyInfo propertyInfo, object owner, System.Activities.Location parent)
                {
                    this.propertyInfo = propertyInfo;
                    this.owner = owner;
                    this.parent = parent;
                }

                public override T Value
                {
                    get
                    {
                        MethodInfo getMethod = this.propertyInfo.GetGetMethod();
                        if ((getMethod == null) && !System.Runtime.TypeHelper.AreTypesCompatible(this.propertyInfo.DeclaringType, typeof(System.Activities.Location)))
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.WriteonlyPropertyCannotBeRead(this.propertyInfo.DeclaringType, this.propertyInfo.Name)));
                        }
                        if ((this.owner == null) && ((getMethod == null) || !getMethod.IsStatic))
                        {
                            throw FxTrace.Exception.AsError(new NullReferenceException(System.Activities.SR.CannotDereferenceNull(this.propertyInfo.Name)));
                        }
                        return (T) this.propertyInfo.GetValue(this.owner, null);
                    }
                    set
                    {
                        MethodInfo setMethod = this.propertyInfo.GetSetMethod();
                        if ((setMethod == null) && !System.Runtime.TypeHelper.AreTypesCompatible(this.propertyInfo.DeclaringType, typeof(System.Activities.Location)))
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ReadonlyPropertyCannotBeSet(this.propertyInfo.DeclaringType, this.propertyInfo.Name)));
                        }
                        if ((this.owner == null) && ((setMethod == null) || !setMethod.IsStatic))
                        {
                            throw FxTrace.Exception.AsError(new NullReferenceException(System.Activities.SR.CannotDereferenceNull(this.propertyInfo.Name)));
                        }
                        this.propertyInfo.SetValue(this.owner, value, null);
                        if (this.parent != null)
                        {
                            this.parent.Value = this.owner;
                        }
                    }
                }
            }
        }
    }
}

