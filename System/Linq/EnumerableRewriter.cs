namespace System.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class EnumerableRewriter : OldExpressionVisitor
    {
        private static ILookup<string, MethodInfo> _seqMethods;

        internal EnumerableRewriter()
        {
        }

        private static bool ArgsMatch(MethodInfo m, ReadOnlyCollection<Expression> args, Type[] typeArgs)
        {
            ParameterInfo[] parameters = m.GetParameters();
            if (parameters.Length != args.Count)
            {
                return false;
            }
            if ((!m.IsGenericMethod && (typeArgs != null)) && (typeArgs.Length > 0))
            {
                return false;
            }
            if ((!m.IsGenericMethodDefinition && m.IsGenericMethod) && m.ContainsGenericParameters)
            {
                m = m.GetGenericMethodDefinition();
            }
            if (m.IsGenericMethodDefinition)
            {
                if ((typeArgs == null) || (typeArgs.Length == 0))
                {
                    return false;
                }
                if (m.GetGenericArguments().Length != typeArgs.Length)
                {
                    return false;
                }
                m = m.MakeGenericMethod(typeArgs);
                parameters = m.GetParameters();
            }
            int index = 0;
            int count = args.Count;
            while (index < count)
            {
                Type parameterType = parameters[index].ParameterType;
                if (parameterType == null)
                {
                    return false;
                }
                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                }
                Expression operand = args[index];
                if (!parameterType.IsAssignableFrom(operand.Type))
                {
                    if (operand.NodeType == ExpressionType.Quote)
                    {
                        operand = ((UnaryExpression) operand).Operand;
                    }
                    if (!parameterType.IsAssignableFrom(operand.Type) && !parameterType.IsAssignableFrom(StripExpression(operand.Type)))
                    {
                        return false;
                    }
                }
                index++;
            }
            return true;
        }

        private static MethodInfo FindEnumerableMethod(string name, ReadOnlyCollection<Expression> args, params Type[] typeArgs)
        {
            if (_seqMethods == null)
            {
                _seqMethods = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static).ToLookup<MethodInfo, string>(m => m.Name);
            }
            MethodInfo info = _seqMethods[name].FirstOrDefault<MethodInfo>(m => ArgsMatch(m, args, typeArgs));
            if (info == null)
            {
                throw System.Linq.Error.NoMethodOnTypeMatchingArguments(name, typeof(Enumerable));
            }
            if (typeArgs != null)
            {
                return info.MakeGenericMethod(typeArgs);
            }
            return info;
        }

        internal static MethodInfo FindMethod(Type type, string name, ReadOnlyCollection<Expression> args, Type[] typeArgs, BindingFlags flags)
        {
            MethodInfo[] source = (from m in type.GetMethods(flags)
                where m.Name == name
                select m).ToArray<MethodInfo>();
            if (source.Length == 0)
            {
                throw System.Linq.Error.NoMethodOnType(name, type);
            }
            MethodInfo info = source.FirstOrDefault<MethodInfo>(m => ArgsMatch(m, args, typeArgs));
            if (info == null)
            {
                throw System.Linq.Error.NoMethodOnTypeMatchingArguments(name, type);
            }
            if (typeArgs != null)
            {
                return info.MakeGenericMethod(typeArgs);
            }
            return info;
        }

        private ReadOnlyCollection<Expression> FixupQuotedArgs(MethodInfo mi, ReadOnlyCollection<Expression> argList)
        {
            ParameterInfo[] parameters = mi.GetParameters();
            if (parameters.Length > 0)
            {
                List<Expression> sequence = null;
                int index = 0;
                int length = parameters.Length;
                while (index < length)
                {
                    Expression expression = argList[index];
                    ParameterInfo info = parameters[index];
                    expression = this.FixupQuotedExpression(info.ParameterType, expression);
                    if ((sequence == null) && (expression != argList[index]))
                    {
                        sequence = new List<Expression>(argList.Count);
                        for (int i = 0; i < index; i++)
                        {
                            sequence.Add(argList[i]);
                        }
                    }
                    if (sequence != null)
                    {
                        sequence.Add(expression);
                    }
                    index++;
                }
                if (sequence != null)
                {
                    argList = sequence.ToReadOnlyCollection<Expression>();
                }
            }
            return argList;
        }

        private Expression FixupQuotedExpression(Type type, Expression expression)
        {
            Expression operand = expression;
        Label_0002:
            if (type.IsAssignableFrom(operand.Type))
            {
                return operand;
            }
            if (operand.NodeType == ExpressionType.Quote)
            {
                operand = ((UnaryExpression) operand).Operand;
                goto Label_0002;
            }
            if ((!type.IsAssignableFrom(operand.Type) && type.IsArray) && (operand.NodeType == ExpressionType.NewArrayInit))
            {
                Type c = StripExpression(operand.Type);
                if (!type.IsAssignableFrom(c))
                {
                    return expression;
                }
                Type elementType = type.GetElementType();
                NewArrayExpression expression3 = (NewArrayExpression) operand;
                List<Expression> initializers = new List<Expression>(expression3.Expressions.Count);
                int num = 0;
                int count = expression3.Expressions.Count;
                while (num < count)
                {
                    initializers.Add(this.FixupQuotedExpression(elementType, expression3.Expressions[num]));
                    num++;
                }
                expression = Expression.NewArrayInit(elementType, initializers);
            }
            return expression;
        }

        private static Type GetPublicType(Type t)
        {
            if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(Lookup<, >.Grouping)))
            {
                return typeof(IGrouping<,>).MakeGenericType(t.GetGenericArguments());
            }
            if (t.IsNestedPrivate)
            {
                foreach (Type type in t.GetInterfaces())
                {
                    if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                    {
                        return type;
                    }
                }
                if (typeof(IEnumerable).IsAssignableFrom(t))
                {
                    return typeof(IEnumerable);
                }
            }
            return t;
        }

        private static Type StripExpression(Type type)
        {
            bool isArray = type.IsArray;
            Type type2 = isArray ? type.GetElementType() : type;
            Type type3 = TypeHelper.FindGenericType(typeof(Expression<>), type2);
            if (type3 != null)
            {
                type2 = type3.GetGenericArguments()[0];
            }
            if (!isArray)
            {
                return type;
            }
            int arrayRank = type.GetArrayRank();
            if (arrayRank != 1)
            {
                return type2.MakeArrayType(arrayRank);
            }
            return type2.MakeArrayType();
        }

        internal override Expression VisitConstant(ConstantExpression c)
        {
            EnumerableQuery query = c.Value as EnumerableQuery;
            if (query == null)
            {
                return c;
            }
            if (query.Enumerable != null)
            {
                Type publicType = GetPublicType(query.Enumerable.GetType());
                return Expression.Constant(query.Enumerable, publicType);
            }
            return this.Visit(query.Expression);
        }

        internal override Expression VisitLambda(LambdaExpression lambda)
        {
            return lambda;
        }

        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            Expression instance = this.Visit(m.Object);
            ReadOnlyCollection<Expression> source = this.VisitExpressionList(m.Arguments);
            if ((instance == m.Object) && (source == m.Arguments))
            {
                return m;
            }
            source.ToArray<Expression>();
            Type[] typeArgs = m.Method.IsGenericMethod ? m.Method.GetGenericArguments() : null;
            if ((m.Method.IsStatic || m.Method.DeclaringType.IsAssignableFrom(instance.Type)) && ArgsMatch(m.Method, source, typeArgs))
            {
                return Expression.Call(instance, m.Method, source);
            }
            if (m.Method.DeclaringType == typeof(Queryable))
            {
                MethodInfo info = FindEnumerableMethod(m.Method.Name, source, typeArgs);
                source = this.FixupQuotedArgs(info, source);
                return Expression.Call(instance, info, source);
            }
            BindingFlags flags = BindingFlags.Static | (m.Method.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic);
            MethodInfo mi = FindMethod(m.Method.DeclaringType, m.Method.Name, source, typeArgs, flags);
            source = this.FixupQuotedArgs(mi, source);
            return Expression.Call(instance, mi, source);
        }

        internal override Expression VisitParameter(ParameterExpression p)
        {
            return p;
        }
    }
}

