namespace System.Linq.Expressions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic.Utils;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions.Compiler;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    public abstract class Expression
    {
        private static readonly CacheDict<System.Type, MethodInfo> _LambdaDelegateCache = new CacheDict<System.Type, MethodInfo>(40);
        private static CacheDict<System.Type, LambdaFactory> _LambdaFactories;
        private static ConditionalWeakTable<Expression, ExtensionInfo> _legacyCtorSupportTable;

        protected Expression()
        {
        }

        [Obsolete("use a different constructor that does not take ExpressionType. Then override NodeType and Type properties to provide the values that would be specified to this constructor.")]
        protected Expression(ExpressionType nodeType, System.Type type)
        {
            if (_legacyCtorSupportTable == null)
            {
                Interlocked.CompareExchange<ConditionalWeakTable<Expression, ExtensionInfo>>(ref _legacyCtorSupportTable, new ConditionalWeakTable<Expression, ExtensionInfo>(), null);
            }
            _legacyCtorSupportTable.Add(this, new ExtensionInfo(nodeType, type));
        }

        protected internal virtual Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitExtension(this);
        }

        public static BinaryExpression Add(Expression left, Expression right)
        {
            return Add(left, right, null);
        }

        public static BinaryExpression Add(Expression left, Expression right, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedBinaryOperator(ExpressionType.Add, left, right, method, true);
            }
            if ((left.Type == right.Type) && TypeUtils.IsArithmetic(left.Type))
            {
                return new SimpleBinaryExpression(ExpressionType.Add, left, right, left.Type);
            }
            return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Add, "op_Addition", left, right, true);
        }

        public static BinaryExpression AddAssign(Expression left, Expression right)
        {
            return AddAssign(left, right, null, null);
        }

        public static BinaryExpression AddAssign(Expression left, Expression right, MethodInfo method)
        {
            return AddAssign(left, right, method, null);
        }

        public static BinaryExpression AddAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion)
        {
            RequiresCanRead(left, "left");
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedAssignOperator(ExpressionType.AddAssign, left, right, method, conversion, true);
            }
            if (!(left.Type == right.Type) || !TypeUtils.IsArithmetic(left.Type))
            {
                return GetUserDefinedAssignOperatorOrThrow(ExpressionType.AddAssign, "op_Addition", left, right, conversion, true);
            }
            if (conversion != null)
            {
                throw System.Linq.Expressions.Error.ConversionIsNotSupportedForArithmeticTypes();
            }
            return new SimpleBinaryExpression(ExpressionType.AddAssign, left, right, left.Type);
        }

        public static BinaryExpression AddAssignChecked(Expression left, Expression right)
        {
            return AddAssignChecked(left, right, null);
        }

        public static BinaryExpression AddAssignChecked(Expression left, Expression right, MethodInfo method)
        {
            return AddAssignChecked(left, right, method, null);
        }

        public static BinaryExpression AddAssignChecked(Expression left, Expression right, MethodInfo method, LambdaExpression conversion)
        {
            RequiresCanRead(left, "left");
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedAssignOperator(ExpressionType.AddAssignChecked, left, right, method, conversion, true);
            }
            if (!(left.Type == right.Type) || !TypeUtils.IsArithmetic(left.Type))
            {
                return GetUserDefinedAssignOperatorOrThrow(ExpressionType.AddAssignChecked, "op_Addition", left, right, conversion, true);
            }
            if (conversion != null)
            {
                throw System.Linq.Expressions.Error.ConversionIsNotSupportedForArithmeticTypes();
            }
            return new SimpleBinaryExpression(ExpressionType.AddAssignChecked, left, right, left.Type);
        }

        public static BinaryExpression AddChecked(Expression left, Expression right)
        {
            return AddChecked(left, right, null);
        }

        public static BinaryExpression AddChecked(Expression left, Expression right, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedBinaryOperator(ExpressionType.AddChecked, left, right, method, true);
            }
            if ((left.Type == right.Type) && TypeUtils.IsArithmetic(left.Type))
            {
                return new SimpleBinaryExpression(ExpressionType.AddChecked, left, right, left.Type);
            }
            return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.AddChecked, "op_Addition", left, right, false);
        }

        public static BinaryExpression And(Expression left, Expression right)
        {
            return And(left, right, null);
        }

        public static BinaryExpression And(Expression left, Expression right, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedBinaryOperator(ExpressionType.And, left, right, method, true);
            }
            if ((left.Type == right.Type) && TypeUtils.IsIntegerOrBool(left.Type))
            {
                return new SimpleBinaryExpression(ExpressionType.And, left, right, left.Type);
            }
            return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.And, "op_BitwiseAnd", left, right, true);
        }

        public static BinaryExpression AndAlso(Expression left, Expression right)
        {
            return AndAlso(left, right, null);
        }

        public static BinaryExpression AndAlso(Expression left, Expression right, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null)
            {
                if (left.Type == right.Type)
                {
                    if (left.Type == typeof(bool))
                    {
                        return new LogicalBinaryExpression(ExpressionType.AndAlso, left, right);
                    }
                    if (left.Type == typeof(bool?))
                    {
                        return new SimpleBinaryExpression(ExpressionType.AndAlso, left, right, left.Type);
                    }
                }
                method = GetUserDefinedBinaryOperator(ExpressionType.AndAlso, left.Type, right.Type, "op_BitwiseAnd");
                if (method == null)
                {
                    throw System.Linq.Expressions.Error.BinaryOperatorNotDefined(ExpressionType.AndAlso, left.Type, right.Type);
                }
                ValidateUserDefinedConditionalLogicOperator(ExpressionType.AndAlso, left.Type, right.Type, method);
                return new MethodBinaryExpression(ExpressionType.AndAlso, left, right, (left.Type.IsNullableType() && TypeUtils.AreEquivalent(method.ReturnType, left.Type.GetNonNullableType())) ? left.Type : method.ReturnType, method);
            }
            ValidateUserDefinedConditionalLogicOperator(ExpressionType.AndAlso, left.Type, right.Type, method);
            return new MethodBinaryExpression(ExpressionType.AndAlso, left, right, (left.Type.IsNullableType() && TypeUtils.AreEquivalent(method.ReturnType, left.Type.GetNonNullableType())) ? left.Type : method.ReturnType, method);
        }

        public static BinaryExpression AndAssign(Expression left, Expression right)
        {
            return AndAssign(left, right, null, null);
        }

        public static BinaryExpression AndAssign(Expression left, Expression right, MethodInfo method)
        {
            return AndAssign(left, right, method, null);
        }

        public static BinaryExpression AndAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion)
        {
            RequiresCanRead(left, "left");
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedAssignOperator(ExpressionType.AndAssign, left, right, method, conversion, true);
            }
            if (!(left.Type == right.Type) || !TypeUtils.IsIntegerOrBool(left.Type))
            {
                return GetUserDefinedAssignOperatorOrThrow(ExpressionType.AndAssign, "op_BitwiseAnd", left, right, conversion, true);
            }
            if (conversion != null)
            {
                throw System.Linq.Expressions.Error.ConversionIsNotSupportedForArithmeticTypes();
            }
            return new SimpleBinaryExpression(ExpressionType.AndAssign, left, right, left.Type);
        }

        private static MethodInfo ApplyTypeArgs(MethodInfo m, System.Type[] typeArgs)
        {
            if ((typeArgs == null) || (typeArgs.Length == 0))
            {
                if (!m.IsGenericMethodDefinition)
                {
                    return m;
                }
            }
            else if (m.IsGenericMethodDefinition && (m.GetGenericArguments().Length == typeArgs.Length))
            {
                return m.MakeGenericMethod(typeArgs);
            }
            return null;
        }

        public static IndexExpression ArrayAccess(Expression array, params Expression[] indexes)
        {
            return ArrayAccess(array, (IEnumerable<Expression>) indexes);
        }

        public static IndexExpression ArrayAccess(Expression array, IEnumerable<Expression> indexes)
        {
            RequiresCanRead(array, "array");
            System.Type type = array.Type;
            if (!type.IsArray)
            {
                throw System.Linq.Expressions.Error.ArgumentMustBeArray();
            }
            ReadOnlyCollection<Expression> arguments = indexes.ToReadOnly<Expression>();
            if (type.GetArrayRank() != arguments.Count)
            {
                throw System.Linq.Expressions.Error.IncorrectNumberOfIndexes();
            }
            foreach (Expression expression in arguments)
            {
                RequiresCanRead(expression, "indexes");
                if (expression.Type != typeof(int))
                {
                    throw System.Linq.Expressions.Error.ArgumentMustBeArrayIndexType();
                }
            }
            return new IndexExpression(array, null, arguments);
        }

        public static BinaryExpression ArrayIndex(Expression array, Expression index)
        {
            RequiresCanRead(array, "array");
            RequiresCanRead(index, "index");
            if (index.Type != typeof(int))
            {
                throw System.Linq.Expressions.Error.ArgumentMustBeArrayIndexType();
            }
            System.Type type = array.Type;
            if (!type.IsArray)
            {
                throw System.Linq.Expressions.Error.ArgumentMustBeArray();
            }
            if (type.GetArrayRank() != 1)
            {
                throw System.Linq.Expressions.Error.IncorrectNumberOfIndexes();
            }
            return new SimpleBinaryExpression(ExpressionType.ArrayIndex, array, index, type.GetElementType());
        }

        public static MethodCallExpression ArrayIndex(Expression array, params Expression[] indexes)
        {
            return ArrayIndex(array, (IEnumerable<Expression>) indexes);
        }

        public static MethodCallExpression ArrayIndex(Expression array, IEnumerable<Expression> indexes)
        {
            RequiresCanRead(array, "array");
            ContractUtils.RequiresNotNull(indexes, "indexes");
            System.Type type = array.Type;
            if (!type.IsArray)
            {
                throw System.Linq.Expressions.Error.ArgumentMustBeArray();
            }
            ReadOnlyCollection<Expression> arguments = indexes.ToReadOnly<Expression>();
            if (type.GetArrayRank() != arguments.Count)
            {
                throw System.Linq.Expressions.Error.IncorrectNumberOfIndexes();
            }
            foreach (Expression expression in arguments)
            {
                RequiresCanRead(expression, "indexes");
                if (expression.Type != typeof(int))
                {
                    throw System.Linq.Expressions.Error.ArgumentMustBeArrayIndexType();
                }
            }
            MethodInfo method = array.Type.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
            return Call(array, method, arguments);
        }

        public static UnaryExpression ArrayLength(Expression array)
        {
            ContractUtils.RequiresNotNull(array, "array");
            if (!array.Type.IsArray || !typeof(Array).IsAssignableFrom(array.Type))
            {
                throw System.Linq.Expressions.Error.ArgumentMustBeArray();
            }
            if (array.Type.GetArrayRank() != 1)
            {
                throw System.Linq.Expressions.Error.ArgumentMustBeSingleDimensionalArrayType();
            }
            return new UnaryExpression(ExpressionType.ArrayLength, array, typeof(int), null);
        }

        public static BinaryExpression Assign(Expression left, Expression right)
        {
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            TypeUtils.ValidateType(left.Type);
            TypeUtils.ValidateType(right.Type);
            if (!TypeUtils.AreReferenceAssignable(left.Type, right.Type))
            {
                throw System.Linq.Expressions.Error.ExpressionTypeDoesNotMatchAssignment(right.Type, left.Type);
            }
            return new AssignBinaryExpression(left, right);
        }

        public static MemberAssignment Bind(MemberInfo member, Expression expression)
        {
            System.Type type;
            ContractUtils.RequiresNotNull(member, "member");
            RequiresCanRead(expression, "expression");
            ValidateSettableFieldOrPropertyMember(member, out type);
            if (!type.IsAssignableFrom(expression.Type))
            {
                throw System.Linq.Expressions.Error.ArgumentTypesMustMatch();
            }
            return new MemberAssignment(member, expression);
        }

        public static MemberAssignment Bind(MethodInfo propertyAccessor, Expression expression)
        {
            ContractUtils.RequiresNotNull(propertyAccessor, "propertyAccessor");
            ContractUtils.RequiresNotNull(expression, "expression");
            ValidateMethodInfo(propertyAccessor);
            return Bind(GetProperty(propertyAccessor), expression);
        }

        public static BlockExpression Block(params Expression[] expressions)
        {
            ContractUtils.RequiresNotNull(expressions, "expressions");
            switch (expressions.Length)
            {
                case 2:
                    return Block(expressions[0], expressions[1]);

                case 3:
                    return Block(expressions[0], expressions[1], expressions[2]);

                case 4:
                    return Block(expressions[0], expressions[1], expressions[2], expressions[3]);

                case 5:
                    return Block(expressions[0], expressions[1], expressions[2], expressions[3], expressions[4]);
            }
            ContractUtils.RequiresNotEmpty<Expression>(expressions, "expressions");
            RequiresCanRead(expressions, "expressions");
            return new BlockN(expressions.Copy<Expression>());
        }

        public static BlockExpression Block(IEnumerable<Expression> expressions)
        {
            return Block(EmptyReadOnlyCollection<ParameterExpression>.Instance, expressions);
        }

        public static BlockExpression Block(Expression arg0, Expression arg1)
        {
            RequiresCanRead(arg0, "arg0");
            RequiresCanRead(arg1, "arg1");
            return new Block2(arg0, arg1);
        }

        public static BlockExpression Block(IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions)
        {
            ContractUtils.RequiresNotNull(expressions, "expressions");
            ReadOnlyCollection<Expression> collection = expressions.ToReadOnly<Expression>();
            ContractUtils.RequiresNotEmpty<Expression>(collection, "expressions");
            RequiresCanRead(collection, "expressions");
            return Block(collection.Last<Expression>().Type, variables, collection);
        }

        public static BlockExpression Block(System.Type type, params Expression[] expressions)
        {
            ContractUtils.RequiresNotNull(expressions, "expressions");
            return Block(type, (IEnumerable<Expression>) expressions);
        }

        public static BlockExpression Block(System.Type type, IEnumerable<Expression> expressions)
        {
            return Block(type, EmptyReadOnlyCollection<ParameterExpression>.Instance, expressions);
        }

        public static BlockExpression Block(IEnumerable<ParameterExpression> variables, params Expression[] expressions)
        {
            return Block(variables, (IEnumerable<Expression>) expressions);
        }

        public static BlockExpression Block(Expression arg0, Expression arg1, Expression arg2)
        {
            RequiresCanRead(arg0, "arg0");
            RequiresCanRead(arg1, "arg1");
            RequiresCanRead(arg2, "arg2");
            return new Block3(arg0, arg1, arg2);
        }

        public static BlockExpression Block(System.Type type, IEnumerable<ParameterExpression> variables, params Expression[] expressions)
        {
            return Block(type, variables, (IEnumerable<Expression>) expressions);
        }

        public static BlockExpression Block(System.Type type, IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions)
        {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(expressions, "expressions");
            ReadOnlyCollection<Expression> collection = expressions.ToReadOnly<Expression>();
            ReadOnlyCollection<ParameterExpression> varList = variables.ToReadOnly<ParameterExpression>();
            ContractUtils.RequiresNotEmpty<Expression>(collection, "expressions");
            RequiresCanRead(collection, "expressions");
            ValidateVariables(varList, "variables");
            Expression expression = collection.Last<Expression>();
            if ((type != typeof(void)) && !TypeUtils.AreReferenceAssignable(type, expression.Type))
            {
                throw System.Linq.Expressions.Error.ArgumentTypesMustMatch();
            }
            if (!TypeUtils.AreEquivalent(type, expression.Type))
            {
                return new ScopeWithType(varList, collection, type);
            }
            if (collection.Count == 1)
            {
                return new Scope1(varList, collection[0]);
            }
            return new ScopeN(varList, collection);
        }

        public static BlockExpression Block(Expression arg0, Expression arg1, Expression arg2, Expression arg3)
        {
            RequiresCanRead(arg0, "arg0");
            RequiresCanRead(arg1, "arg1");
            RequiresCanRead(arg2, "arg2");
            RequiresCanRead(arg3, "arg3");
            return new Block4(arg0, arg1, arg2, arg3);
        }

        public static BlockExpression Block(Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4)
        {
            RequiresCanRead(arg0, "arg0");
            RequiresCanRead(arg1, "arg1");
            RequiresCanRead(arg2, "arg2");
            RequiresCanRead(arg3, "arg3");
            RequiresCanRead(arg4, "arg4");
            return new Block5(arg0, arg1, arg2, arg3, arg4);
        }

        public static GotoExpression Break(LabelTarget target)
        {
            return MakeGoto(GotoExpressionKind.Break, target, null, typeof(void));
        }

        public static GotoExpression Break(LabelTarget target, Expression value)
        {
            return MakeGoto(GotoExpressionKind.Break, target, value, typeof(void));
        }

        public static GotoExpression Break(LabelTarget target, System.Type type)
        {
            return MakeGoto(GotoExpressionKind.Break, target, null, type);
        }

        public static GotoExpression Break(LabelTarget target, Expression value, System.Type type)
        {
            return MakeGoto(GotoExpressionKind.Break, target, value, type);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method)
        {
            return Call(instance, method, EmptyReadOnlyCollection<Expression>.Instance);
        }

        public static MethodCallExpression Call(MethodInfo method, IEnumerable<Expression> arguments)
        {
            return Call(null, method, arguments);
        }

        public static MethodCallExpression Call(MethodInfo method, Expression arg0)
        {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(arg0, "arg0");
            ParameterInfo[] pis = ValidateMethodAndGetParameters(null, method);
            ValidateArgumentCount(method, ExpressionType.Call, 1, pis);
            arg0 = ValidateOneArgument(method, ExpressionType.Call, arg0, pis[0]);
            return new MethodCallExpression1(method, arg0);
        }

        public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments)
        {
            return Call(null, method, arguments);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, params Expression[] arguments)
        {
            return Call(instance, method, (IEnumerable<Expression>) arguments);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, IEnumerable<Expression> arguments)
        {
            ContractUtils.RequiresNotNull(method, "method");
            ReadOnlyCollection<Expression> onlys = arguments.ToReadOnly<Expression>();
            ValidateMethodInfo(method);
            ValidateStaticOrInstanceMethod(instance, method);
            ValidateArgumentTypes(method, ExpressionType.Call, ref onlys);
            if (instance == null)
            {
                return new MethodCallExpressionN(method, onlys);
            }
            return new InstanceMethodCallExpressionN(method, instance, onlys);
        }

        public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1)
        {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(arg0, "arg0");
            ContractUtils.RequiresNotNull(arg1, "arg1");
            ParameterInfo[] pis = ValidateMethodAndGetParameters(null, method);
            ValidateArgumentCount(method, ExpressionType.Call, 2, pis);
            arg0 = ValidateOneArgument(method, ExpressionType.Call, arg0, pis[0]);
            arg1 = ValidateOneArgument(method, ExpressionType.Call, arg1, pis[1]);
            return new MethodCallExpression2(method, arg0, arg1);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, Expression arg0, Expression arg1)
        {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(arg0, "arg0");
            ContractUtils.RequiresNotNull(arg1, "arg1");
            ParameterInfo[] pis = ValidateMethodAndGetParameters(instance, method);
            ValidateArgumentCount(method, ExpressionType.Call, 2, pis);
            arg0 = ValidateOneArgument(method, ExpressionType.Call, arg0, pis[0]);
            arg1 = ValidateOneArgument(method, ExpressionType.Call, arg1, pis[1]);
            if (instance != null)
            {
                return new InstanceMethodCallExpression2(method, instance, arg0, arg1);
            }
            return new MethodCallExpression2(method, arg0, arg1);
        }

        public static MethodCallExpression Call(Expression instance, string methodName, System.Type[] typeArguments, params Expression[] arguments)
        {
            ContractUtils.RequiresNotNull(instance, "instance");
            ContractUtils.RequiresNotNull(methodName, "methodName");
            if (arguments == null)
            {
                arguments = new Expression[0];
            }
            BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            return Call(instance, FindMethod(instance.Type, methodName, typeArguments, arguments, flags), arguments);
        }

        public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2)
        {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(arg0, "arg0");
            ContractUtils.RequiresNotNull(arg1, "arg1");
            ContractUtils.RequiresNotNull(arg2, "arg2");
            ParameterInfo[] pis = ValidateMethodAndGetParameters(null, method);
            ValidateArgumentCount(method, ExpressionType.Call, 3, pis);
            arg0 = ValidateOneArgument(method, ExpressionType.Call, arg0, pis[0]);
            arg1 = ValidateOneArgument(method, ExpressionType.Call, arg1, pis[1]);
            arg2 = ValidateOneArgument(method, ExpressionType.Call, arg2, pis[2]);
            return new MethodCallExpression3(method, arg0, arg1, arg2);
        }

        public static MethodCallExpression Call(System.Type type, string methodName, System.Type[] typeArguments, params Expression[] arguments)
        {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(methodName, "methodName");
            if (arguments == null)
            {
                arguments = new Expression[0];
            }
            BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            return Call(null, FindMethod(type, methodName, typeArguments, arguments, flags), arguments);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, Expression arg0, Expression arg1, Expression arg2)
        {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(arg0, "arg0");
            ContractUtils.RequiresNotNull(arg1, "arg1");
            ContractUtils.RequiresNotNull(arg2, "arg2");
            ParameterInfo[] pis = ValidateMethodAndGetParameters(instance, method);
            ValidateArgumentCount(method, ExpressionType.Call, 3, pis);
            arg0 = ValidateOneArgument(method, ExpressionType.Call, arg0, pis[0]);
            arg1 = ValidateOneArgument(method, ExpressionType.Call, arg1, pis[1]);
            arg2 = ValidateOneArgument(method, ExpressionType.Call, arg2, pis[2]);
            if (instance != null)
            {
                return new InstanceMethodCallExpression3(method, instance, arg0, arg1, arg2);
            }
            return new MethodCallExpression3(method, arg0, arg1, arg2);
        }

        public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
        {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(arg0, "arg0");
            ContractUtils.RequiresNotNull(arg1, "arg1");
            ContractUtils.RequiresNotNull(arg2, "arg2");
            ContractUtils.RequiresNotNull(arg3, "arg3");
            ParameterInfo[] pis = ValidateMethodAndGetParameters(null, method);
            ValidateArgumentCount(method, ExpressionType.Call, 4, pis);
            arg0 = ValidateOneArgument(method, ExpressionType.Call, arg0, pis[0]);
            arg1 = ValidateOneArgument(method, ExpressionType.Call, arg1, pis[1]);
            arg2 = ValidateOneArgument(method, ExpressionType.Call, arg2, pis[2]);
            arg3 = ValidateOneArgument(method, ExpressionType.Call, arg3, pis[3]);
            return new MethodCallExpression4(method, arg0, arg1, arg2, arg3);
        }

        public static MethodCallExpression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4)
        {
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(arg0, "arg0");
            ContractUtils.RequiresNotNull(arg1, "arg1");
            ContractUtils.RequiresNotNull(arg2, "arg2");
            ContractUtils.RequiresNotNull(arg3, "arg3");
            ContractUtils.RequiresNotNull(arg4, "arg4");
            ParameterInfo[] pis = ValidateMethodAndGetParameters(null, method);
            ValidateArgumentCount(method, ExpressionType.Call, 5, pis);
            arg0 = ValidateOneArgument(method, ExpressionType.Call, arg0, pis[0]);
            arg1 = ValidateOneArgument(method, ExpressionType.Call, arg1, pis[1]);
            arg2 = ValidateOneArgument(method, ExpressionType.Call, arg2, pis[2]);
            arg3 = ValidateOneArgument(method, ExpressionType.Call, arg3, pis[3]);
            arg4 = ValidateOneArgument(method, ExpressionType.Call, arg4, pis[4]);
            return new MethodCallExpression5(method, arg0, arg1, arg2, arg3, arg4);
        }

        public static CatchBlock Catch(ParameterExpression variable, Expression body)
        {
            ContractUtils.RequiresNotNull(variable, "variable");
            return MakeCatchBlock(variable.Type, variable, body, null);
        }

        public static CatchBlock Catch(System.Type type, Expression body)
        {
            return MakeCatchBlock(type, null, body, null);
        }

        public static CatchBlock Catch(ParameterExpression variable, Expression body, Expression filter)
        {
            ContractUtils.RequiresNotNull(variable, "variable");
            return MakeCatchBlock(variable.Type, variable, body, filter);
        }

        public static CatchBlock Catch(System.Type type, Expression body, Expression filter)
        {
            return MakeCatchBlock(type, null, body, filter);
        }

        private static bool CheckMethod(MethodInfo method, MethodInfo propertyMethod)
        {
            if (method == propertyMethod)
            {
                return true;
            }
            System.Type declaringType = method.DeclaringType;
            return ((declaringType.IsInterface && (method.Name == propertyMethod.Name)) && (declaringType.GetMethod(method.Name) == propertyMethod));
        }

        public static DebugInfoExpression ClearDebugInfo(SymbolDocumentInfo document)
        {
            ContractUtils.RequiresNotNull(document, "document");
            return new ClearDebugInfoExpression(document);
        }

        public static BinaryExpression Coalesce(Expression left, Expression right)
        {
            return Coalesce(left, right, null);
        }

        public static BinaryExpression Coalesce(Expression left, Expression right, LambdaExpression conversion)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (conversion == null)
            {
                return new SimpleBinaryExpression(ExpressionType.Coalesce, left, right, ValidateCoalesceArgTypes(left.Type, right.Type));
            }
            if (left.Type.IsValueType && !left.Type.IsNullableType())
            {
                throw System.Linq.Expressions.Error.CoalesceUsedOnNonNullType();
            }
            MethodInfo method = conversion.Type.GetMethod("Invoke");
            if (method.ReturnType == typeof(void))
            {
                throw System.Linq.Expressions.Error.UserDefinedOperatorMustNotBeVoid(conversion);
            }
            ParameterInfo[] parametersCached = method.GetParametersCached();
            if (parametersCached.Length != 1)
            {
                throw System.Linq.Expressions.Error.IncorrectNumberOfMethodCallArguments(conversion);
            }
            if (!TypeUtils.AreEquivalent(method.ReturnType, right.Type))
            {
                throw System.Linq.Expressions.Error.OperandTypesDoNotMatchParameters(ExpressionType.Coalesce, conversion.ToString());
            }
            if (!ParameterIsAssignable(parametersCached[0], left.Type.GetNonNullableType()) && !ParameterIsAssignable(parametersCached[0], left.Type))
            {
                throw System.Linq.Expressions.Error.OperandTypesDoNotMatchParameters(ExpressionType.Coalesce, conversion.ToString());
            }
            return new CoalesceConversionBinaryExpression(left, right, conversion);
        }

        public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse)
        {
            RequiresCanRead(test, "test");
            RequiresCanRead(ifTrue, "ifTrue");
            RequiresCanRead(ifFalse, "ifFalse");
            if (test.Type != typeof(bool))
            {
                throw System.Linq.Expressions.Error.ArgumentMustBeBoolean();
            }
            if (!TypeUtils.AreEquivalent(ifTrue.Type, ifFalse.Type))
            {
                throw System.Linq.Expressions.Error.ArgumentTypesMustMatch();
            }
            return ConditionalExpression.Make(test, ifTrue, ifFalse, ifTrue.Type);
        }

        public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse, System.Type type)
        {
            RequiresCanRead(test, "test");
            RequiresCanRead(ifTrue, "ifTrue");
            RequiresCanRead(ifFalse, "ifFalse");
            ContractUtils.RequiresNotNull(type, "type");
            if (test.Type != typeof(bool))
            {
                throw System.Linq.Expressions.Error.ArgumentMustBeBoolean();
            }
            if ((type != typeof(void)) && (!TypeUtils.AreReferenceAssignable(type, ifTrue.Type) || !TypeUtils.AreReferenceAssignable(type, ifFalse.Type)))
            {
                throw System.Linq.Expressions.Error.ArgumentTypesMustMatch();
            }
            return ConditionalExpression.Make(test, ifTrue, ifFalse, type);
        }

        public static ConstantExpression Constant(object value)
        {
            return ConstantExpression.Make(value, (value == null) ? typeof(object) : value.GetType());
        }

        public static ConstantExpression Constant(object value, System.Type type)
        {
            ContractUtils.RequiresNotNull(type, "type");
            if (((value == null) && type.IsValueType) && !type.IsNullableType())
            {
                throw System.Linq.Expressions.Error.ArgumentTypesMustMatch();
            }
            if ((value != null) && !type.IsAssignableFrom(value.GetType()))
            {
                throw System.Linq.Expressions.Error.ArgumentTypesMustMatch();
            }
            return ConstantExpression.Make(value, type);
        }

        public static GotoExpression Continue(LabelTarget target)
        {
            return MakeGoto(GotoExpressionKind.Continue, target, null, typeof(void));
        }

        public static GotoExpression Continue(LabelTarget target, System.Type type)
        {
            return MakeGoto(GotoExpressionKind.Continue, target, null, type);
        }

        public static UnaryExpression Convert(Expression expression, System.Type type)
        {
            return Convert(expression, type, null);
        }

        public static UnaryExpression Convert(Expression expression, System.Type type, MethodInfo method)
        {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            TypeUtils.ValidateType(type);
            if (method != null)
            {
                return GetMethodBasedCoercionOperator(ExpressionType.Convert, expression, type, method);
            }
            if (!TypeUtils.HasIdentityPrimitiveOrNullableConversion(expression.Type, type) && !TypeUtils.HasReferenceConversion(expression.Type, type))
            {
                return GetUserDefinedCoercionOrThrow(ExpressionType.Convert, expression, type);
            }
            return new UnaryExpression(ExpressionType.Convert, expression, type, null);
        }

        public static UnaryExpression ConvertChecked(Expression expression, System.Type type)
        {
            return ConvertChecked(expression, type, null);
        }

        public static UnaryExpression ConvertChecked(Expression expression, System.Type type, MethodInfo method)
        {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            TypeUtils.ValidateType(type);
            if (method != null)
            {
                return GetMethodBasedCoercionOperator(ExpressionType.ConvertChecked, expression, type, method);
            }
            if (TypeUtils.HasIdentityPrimitiveOrNullableConversion(expression.Type, type))
            {
                return new UnaryExpression(ExpressionType.ConvertChecked, expression, type, null);
            }
            if (TypeUtils.HasReferenceConversion(expression.Type, type))
            {
                return new UnaryExpression(ExpressionType.Convert, expression, type, null);
            }
            return GetUserDefinedCoercionOrThrow(ExpressionType.ConvertChecked, expression, type);
        }

        internal static LambdaExpression CreateLambda(System.Type delegateType, Expression body, string name, bool tailCall, ReadOnlyCollection<ParameterExpression> parameters)
        {
            LambdaFactory factory;
            if (_LambdaFactories == null)
            {
                Interlocked.CompareExchange<CacheDict<System.Type, LambdaFactory>>(ref _LambdaFactories, new CacheDict<System.Type, LambdaFactory>(50), null);
            }
            MethodInfo method = null;
            lock (_LambdaFactories)
            {
                if (!_LambdaFactories.TryGetValue(delegateType, out factory))
                {
                    method = typeof(Expression<>).MakeGenericType(new System.Type[] { delegateType }).GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static);
                    if (delegateType.CanCache())
                    {
                        _LambdaFactories[delegateType] = factory = (LambdaFactory) Delegate.CreateDelegate(typeof(LambdaFactory), method);
                    }
                }
            }
            if (factory != null)
            {
                return factory(body, name, tailCall, parameters);
            }
            return (LambdaExpression) method.Invoke(null, new object[] { body, name, tailCall, parameters });
        }

        public static DebugInfoExpression DebugInfo(SymbolDocumentInfo document, int startLine, int startColumn, int endLine, int endColumn)
        {
            ContractUtils.RequiresNotNull(document, "document");
            if (((startLine == 0xfeefee) && (startColumn == 0)) && ((endLine == 0xfeefee) && (endColumn == 0)))
            {
                return new ClearDebugInfoExpression(document);
            }
            ValidateSpan(startLine, startColumn, endLine, endColumn);
            return new SpanDebugInfoExpression(document, startLine, startColumn, endLine, endColumn);
        }

        public static UnaryExpression Decrement(Expression expression)
        {
            return Decrement(expression, null);
        }

        public static UnaryExpression Decrement(Expression expression, MethodInfo method)
        {
            RequiresCanRead(expression, "expression");
            if (method != null)
            {
                return GetMethodBasedUnaryOperator(ExpressionType.Decrement, expression, method);
            }
            if (TypeUtils.IsArithmetic(expression.Type))
            {
                return new UnaryExpression(ExpressionType.Decrement, expression, expression.Type, null);
            }
            return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Decrement, "op_Decrement", expression);
        }

        public static DefaultExpression Default(System.Type type)
        {
            if (type == typeof(void))
            {
                return Empty();
            }
            return new DefaultExpression(type);
        }

        public static BinaryExpression Divide(Expression left, Expression right)
        {
            return Divide(left, right, null);
        }

        public static BinaryExpression Divide(Expression left, Expression right, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedBinaryOperator(ExpressionType.Divide, left, right, method, true);
            }
            if ((left.Type == right.Type) && TypeUtils.IsArithmetic(left.Type))
            {
                return new SimpleBinaryExpression(ExpressionType.Divide, left, right, left.Type);
            }
            return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Divide, "op_Division", left, right, true);
        }

        public static BinaryExpression DivideAssign(Expression left, Expression right)
        {
            return DivideAssign(left, right, null, null);
        }

        public static BinaryExpression DivideAssign(Expression left, Expression right, MethodInfo method)
        {
            return DivideAssign(left, right, method, null);
        }

        public static BinaryExpression DivideAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion)
        {
            RequiresCanRead(left, "left");
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedAssignOperator(ExpressionType.DivideAssign, left, right, method, conversion, true);
            }
            if (!(left.Type == right.Type) || !TypeUtils.IsArithmetic(left.Type))
            {
                return GetUserDefinedAssignOperatorOrThrow(ExpressionType.DivideAssign, "op_Division", left, right, conversion, true);
            }
            if (conversion != null)
            {
                throw System.Linq.Expressions.Error.ConversionIsNotSupportedForArithmeticTypes();
            }
            return new SimpleBinaryExpression(ExpressionType.DivideAssign, left, right, left.Type);
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, System.Type returnType, params Expression[] arguments)
        {
            return Dynamic(binder, returnType, (IEnumerable<Expression>) arguments);
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, System.Type returnType, Expression arg0)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            ValidateDynamicArgument(arg0);
            DelegateHelpers.TypeInfo nextTypeInfo = DelegateHelpers.GetNextTypeInfo(returnType, DelegateHelpers.GetNextTypeInfo(arg0.Type, DelegateHelpers.NextTypeInfo(typeof(CallSite))));
            System.Type delegateType = nextTypeInfo.DelegateType;
            if (delegateType == null)
            {
                delegateType = nextTypeInfo.MakeDelegateType(returnType, new Expression[] { arg0 });
            }
            return DynamicExpression.Make(returnType, delegateType, binder, arg0);
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, System.Type returnType, IEnumerable<Expression> arguments)
        {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.RequiresNotNull(returnType, "returnType");
            ReadOnlyCollection<Expression> collection = arguments.ToReadOnly<Expression>();
            ContractUtils.RequiresNotEmpty<Expression>(collection, "args");
            return MakeDynamic(binder, returnType, collection);
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, System.Type returnType, Expression arg0, Expression arg1)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            ValidateDynamicArgument(arg0);
            ValidateDynamicArgument(arg1);
            DelegateHelpers.TypeInfo nextTypeInfo = DelegateHelpers.GetNextTypeInfo(returnType, DelegateHelpers.GetNextTypeInfo(arg1.Type, DelegateHelpers.GetNextTypeInfo(arg0.Type, DelegateHelpers.NextTypeInfo(typeof(CallSite)))));
            System.Type delegateType = nextTypeInfo.DelegateType;
            if (delegateType == null)
            {
                delegateType = nextTypeInfo.MakeDelegateType(returnType, new Expression[] { arg0, arg1 });
            }
            return DynamicExpression.Make(returnType, delegateType, binder, arg0, arg1);
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, System.Type returnType, Expression arg0, Expression arg1, Expression arg2)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            ValidateDynamicArgument(arg0);
            ValidateDynamicArgument(arg1);
            ValidateDynamicArgument(arg2);
            DelegateHelpers.TypeInfo nextTypeInfo = DelegateHelpers.GetNextTypeInfo(returnType, DelegateHelpers.GetNextTypeInfo(arg2.Type, DelegateHelpers.GetNextTypeInfo(arg1.Type, DelegateHelpers.GetNextTypeInfo(arg0.Type, DelegateHelpers.NextTypeInfo(typeof(CallSite))))));
            System.Type delegateType = nextTypeInfo.DelegateType;
            if (delegateType == null)
            {
                delegateType = nextTypeInfo.MakeDelegateType(returnType, new Expression[] { arg0, arg1, arg2 });
            }
            return DynamicExpression.Make(returnType, delegateType, binder, arg0, arg1, arg2);
        }

        public static DynamicExpression Dynamic(CallSiteBinder binder, System.Type returnType, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            ValidateDynamicArgument(arg0);
            ValidateDynamicArgument(arg1);
            ValidateDynamicArgument(arg2);
            ValidateDynamicArgument(arg3);
            DelegateHelpers.TypeInfo nextTypeInfo = DelegateHelpers.GetNextTypeInfo(returnType, DelegateHelpers.GetNextTypeInfo(arg3.Type, DelegateHelpers.GetNextTypeInfo(arg2.Type, DelegateHelpers.GetNextTypeInfo(arg1.Type, DelegateHelpers.GetNextTypeInfo(arg0.Type, DelegateHelpers.NextTypeInfo(typeof(CallSite)))))));
            System.Type delegateType = nextTypeInfo.DelegateType;
            if (delegateType == null)
            {
                delegateType = nextTypeInfo.MakeDelegateType(returnType, new Expression[] { arg0, arg1, arg2, arg3 });
            }
            return DynamicExpression.Make(returnType, delegateType, binder, arg0, arg1, arg2, arg3);
        }

        public static System.Linq.Expressions.ElementInit ElementInit(MethodInfo addMethod, params Expression[] arguments)
        {
            return ElementInit(addMethod, (IEnumerable<Expression>) arguments);
        }

        public static System.Linq.Expressions.ElementInit ElementInit(MethodInfo addMethod, IEnumerable<Expression> arguments)
        {
            ContractUtils.RequiresNotNull(addMethod, "addMethod");
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ReadOnlyCollection<Expression> items = arguments.ToReadOnly<Expression>();
            RequiresCanRead(items, "arguments");
            ValidateElementInitAddMethodInfo(addMethod);
            ValidateArgumentTypes(addMethod, ExpressionType.Call, ref items);
            return new System.Linq.Expressions.ElementInit(addMethod, items);
        }

        public static DefaultExpression Empty()
        {
            return new DefaultExpression(typeof(void));
        }

        public static BinaryExpression Equal(Expression left, Expression right)
        {
            return Equal(left, right, false, null);
        }

        public static BinaryExpression Equal(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null)
            {
                return GetEqualityComparisonOperator(ExpressionType.Equal, "op_Equality", left, right, liftToNull);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.Equal, left, right, method, liftToNull);
        }

        public static BinaryExpression ExclusiveOr(Expression left, Expression right)
        {
            return ExclusiveOr(left, right, null);
        }

        public static BinaryExpression ExclusiveOr(Expression left, Expression right, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedBinaryOperator(ExpressionType.ExclusiveOr, left, right, method, true);
            }
            if ((left.Type == right.Type) && TypeUtils.IsIntegerOrBool(left.Type))
            {
                return new SimpleBinaryExpression(ExpressionType.ExclusiveOr, left, right, left.Type);
            }
            return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.ExclusiveOr, "op_ExclusiveOr", left, right, true);
        }

        public static BinaryExpression ExclusiveOrAssign(Expression left, Expression right)
        {
            return ExclusiveOrAssign(left, right, null, null);
        }

        public static BinaryExpression ExclusiveOrAssign(Expression left, Expression right, MethodInfo method)
        {
            return ExclusiveOrAssign(left, right, method, null);
        }

        public static BinaryExpression ExclusiveOrAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion)
        {
            RequiresCanRead(left, "left");
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedAssignOperator(ExpressionType.ExclusiveOrAssign, left, right, method, conversion, true);
            }
            if (!(left.Type == right.Type) || !TypeUtils.IsIntegerOrBool(left.Type))
            {
                return GetUserDefinedAssignOperatorOrThrow(ExpressionType.ExclusiveOrAssign, "op_ExclusiveOr", left, right, conversion, true);
            }
            if (conversion != null)
            {
                throw System.Linq.Expressions.Error.ConversionIsNotSupportedForArithmeticTypes();
            }
            return new SimpleBinaryExpression(ExpressionType.ExclusiveOrAssign, left, right, left.Type);
        }

        public static MemberExpression Field(Expression expression, FieldInfo field)
        {
            ContractUtils.RequiresNotNull(field, "field");
            if (field.IsStatic)
            {
                if (expression != null)
                {
                    throw new ArgumentException(System.Linq.Expressions.Strings.OnlyStaticFieldsHaveNullInstance, "expression");
                }
            }
            else
            {
                if (expression == null)
                {
                    throw new ArgumentException(System.Linq.Expressions.Strings.OnlyStaticFieldsHaveNullInstance, "field");
                }
                RequiresCanRead(expression, "expression");
                if (!TypeUtils.AreReferenceAssignable(field.DeclaringType, expression.Type))
                {
                    throw System.Linq.Expressions.Error.FieldInfoNotDefinedForType(field.DeclaringType, field.Name, expression.Type);
                }
            }
            return MemberExpression.Make(expression, field);
        }

        public static MemberExpression Field(Expression expression, string fieldName)
        {
            RequiresCanRead(expression, "expression");
            FieldInfo field = expression.Type.GetField(fieldName, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field == null)
            {
                field = expression.Type.GetField(fieldName, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            }
            if (field == null)
            {
                throw System.Linq.Expressions.Error.InstanceFieldNotDefinedForType(fieldName, expression.Type);
            }
            return Field(expression, field);
        }

        public static MemberExpression Field(Expression expression, System.Type type, string fieldName)
        {
            ContractUtils.RequiresNotNull(type, "type");
            FieldInfo field = type.GetField(fieldName, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field == null)
            {
                field = type.GetField(fieldName, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
            }
            if (field == null)
            {
                throw System.Linq.Expressions.Error.FieldNotDefinedForType(fieldName, type);
            }
            return Field(expression, field);
        }

        private static int FindBestMethod(IEnumerable<MethodInfo> methods, System.Type[] typeArgs, Expression[] args, out MethodInfo method)
        {
            int num = 0;
            method = null;
            foreach (MethodInfo info in methods)
            {
                MethodInfo m = ApplyTypeArgs(info, typeArgs);
                if ((m != null) && IsCompatible(m, args))
                {
                    if ((method == null) || (!method.IsPublic && m.IsPublic))
                    {
                        method = m;
                        num = 1;
                    }
                    else if (method.IsPublic == m.IsPublic)
                    {
                        num++;
                    }
                }
            }
            return num;
        }

        private static int FindBestProperty(IEnumerable<PropertyInfo> properties, Expression[] args, out PropertyInfo property)
        {
            int num = 0;
            property = null;
            foreach (PropertyInfo info in properties)
            {
                if ((info != null) && IsCompatible(info, args))
                {
                    if (property == null)
                    {
                        property = info;
                        num = 1;
                    }
                    else
                    {
                        num++;
                    }
                }
            }
            return num;
        }

        private static PropertyInfo FindInstanceProperty(System.Type type, string propertyName, Expression[] arguments)
        {
            BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
            PropertyInfo info = FindProperty(type, propertyName, arguments, flags);
            if (info == null)
            {
                flags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase;
                info = FindProperty(type, propertyName, arguments, flags);
            }
            if (info != null)
            {
                return info;
            }
            if ((arguments != null) && (arguments.Length != 0))
            {
                throw System.Linq.Expressions.Error.InstancePropertyWithSpecifiedParametersNotDefinedForType(propertyName, GetArgTypesString(arguments), type);
            }
            throw System.Linq.Expressions.Error.InstancePropertyWithoutParameterNotDefinedForType(propertyName, type);
        }

        private static MethodInfo FindMethod(System.Type type, string methodName, System.Type[] typeArgs, Expression[] args, BindingFlags flags)
        {
            MethodInfo info;
            MemberInfo[] collection = type.FindMembers(MemberTypes.Method, flags, System.Type.FilterNameIgnoreCase, methodName);
            if ((collection == null) || (collection.Length == 0))
            {
                throw System.Linq.Expressions.Error.MethodDoesNotExistOnType(methodName, type);
            }
            int num = FindBestMethod(collection.Map<MemberInfo, MethodInfo>(t => (MethodInfo) t), typeArgs, args, out info);
            if (num == 0)
            {
                if ((typeArgs != null) && (typeArgs.Length > 0))
                {
                    throw System.Linq.Expressions.Error.GenericMethodWithArgsDoesNotExistOnType(methodName, type);
                }
                throw System.Linq.Expressions.Error.MethodWithArgsDoesNotExistOnType(methodName, type);
            }
            if (num > 1)
            {
                throw System.Linq.Expressions.Error.MethodWithMoreThanOneMatch(methodName, type);
            }
            return info;
        }

        private static PropertyInfo FindProperty(System.Type type, string propertyName, Expression[] arguments, BindingFlags flags)
        {
            PropertyInfo info;
            MemberInfo[] collection = type.FindMembers(MemberTypes.Property, flags, System.Type.FilterNameIgnoreCase, propertyName);
            if ((collection == null) || (collection.Length == 0))
            {
                return null;
            }
            int num = FindBestProperty(collection.Map<MemberInfo, PropertyInfo>(t => (PropertyInfo) t), arguments, out info);
            if (num == 0)
            {
                return null;
            }
            if (num > 1)
            {
                throw System.Linq.Expressions.Error.PropertyWithMoreThanOneMatch(propertyName, type);
            }
            return info;
        }

        public static System.Type GetActionType(params System.Type[] typeArgs)
        {
            if (!ValidateTryGetFuncActionArgs(typeArgs))
            {
                throw System.Linq.Expressions.Error.TypeMustNotBeByRef();
            }
            System.Type actionType = DelegateHelpers.GetActionType(typeArgs);
            if (actionType == null)
            {
                throw System.Linq.Expressions.Error.IncorrectNumberOfTypeArgsForAction();
            }
            return actionType;
        }

        private static string GetArgTypesString(Expression[] arguments)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            builder.Append("(");
            foreach (System.Type type in from arg in arguments select arg.Type)
            {
                if (!flag)
                {
                    builder.Append(", ");
                }
                builder.Append(type.Name);
                flag = false;
            }
            builder.Append(")");
            return builder.ToString();
        }

        private static BinaryExpression GetComparisonOperator(ExpressionType binaryType, string opName, Expression left, Expression right, bool liftToNull)
        {
            if (!(left.Type == right.Type) || !TypeUtils.IsNumeric(left.Type))
            {
                return GetUserDefinedBinaryOperatorOrThrow(binaryType, opName, left, right, liftToNull);
            }
            if (left.Type.IsNullableType() && liftToNull)
            {
                return new SimpleBinaryExpression(binaryType, left, right, typeof(bool?));
            }
            return new LogicalBinaryExpression(binaryType, left, right);
        }

        public static System.Type GetDelegateType(params System.Type[] typeArgs)
        {
            ContractUtils.RequiresNotEmpty<System.Type>(typeArgs, "typeArgs");
            ContractUtils.RequiresNotNullItems<System.Type>(typeArgs, "typeArgs");
            return DelegateHelpers.MakeDelegateType(typeArgs);
        }

        private static BinaryExpression GetEqualityComparisonOperator(ExpressionType binaryType, string opName, Expression left, Expression right, bool liftToNull)
        {
            if ((left.Type == right.Type) && ((TypeUtils.IsNumeric(left.Type) || (left.Type == typeof(object))) || (TypeUtils.IsBool(left.Type) || left.Type.GetNonNullableType().IsEnum)))
            {
                if (left.Type.IsNullableType() && liftToNull)
                {
                    return new SimpleBinaryExpression(binaryType, left, right, typeof(bool?));
                }
                return new LogicalBinaryExpression(binaryType, left, right);
            }
            BinaryExpression expression = GetUserDefinedBinaryOperator(binaryType, opName, left, right, liftToNull);
            if (expression != null)
            {
                return expression;
            }
            if (!TypeUtils.HasBuiltInEqualityOperator(left.Type, right.Type) && !IsNullComparison(left, right))
            {
                throw System.Linq.Expressions.Error.BinaryOperatorNotDefined(binaryType, left.Type, right.Type);
            }
            if (left.Type.IsNullableType() && liftToNull)
            {
                return new SimpleBinaryExpression(binaryType, left, right, typeof(bool?));
            }
            return new LogicalBinaryExpression(binaryType, left, right);
        }

        public static System.Type GetFuncType(params System.Type[] typeArgs)
        {
            if (!ValidateTryGetFuncActionArgs(typeArgs))
            {
                throw System.Linq.Expressions.Error.TypeMustNotBeByRef();
            }
            System.Type funcType = DelegateHelpers.GetFuncType(typeArgs);
            if (funcType == null)
            {
                throw System.Linq.Expressions.Error.IncorrectNumberOfTypeArgsForFunc();
            }
            return funcType;
        }

        internal static MethodInfo GetInvokeMethod(Expression expression)
        {
            System.Type type = expression.Type;
            if (type == typeof(Delegate))
            {
                throw System.Linq.Expressions.Error.ExpressionTypeNotInvocable(type);
            }
            if (!typeof(Delegate).IsAssignableFrom(expression.Type))
            {
                System.Type type2 = TypeUtils.FindGenericType(typeof(Expression<>), expression.Type);
                if (type2 == null)
                {
                    throw System.Linq.Expressions.Error.ExpressionTypeNotInvocable(expression.Type);
                }
                type = type2.GetGenericArguments()[0];
            }
            return type.GetMethod("Invoke");
        }

        private static BinaryExpression GetMethodBasedAssignOperator(ExpressionType binaryType, Expression left, Expression right, MethodInfo method, LambdaExpression conversion, bool liftToNull)
        {
            BinaryExpression expression = GetMethodBasedBinaryOperator(binaryType, left, right, method, liftToNull);
            if (conversion == null)
            {
                if (!TypeUtils.AreReferenceAssignable(left.Type, expression.Type))
                {
                    throw System.Linq.Expressions.Error.UserDefinedOpMustHaveValidReturnType(binaryType, expression.Method.Name);
                }
                return expression;
            }
            ValidateOpAssignConversionLambda(conversion, expression.Left, expression.Method, expression.NodeType);
            return new OpAssignMethodConversionBinaryExpression(expression.NodeType, expression.Left, expression.Right, expression.Left.Type, expression.Method, conversion);
        }

        private static BinaryExpression GetMethodBasedBinaryOperator(ExpressionType binaryType, Expression left, Expression right, MethodInfo method, bool liftToNull)
        {
            ValidateOperator(method);
            ParameterInfo[] parametersCached = method.GetParametersCached();
            if (parametersCached.Length != 2)
            {
                throw System.Linq.Expressions.Error.IncorrectNumberOfMethodCallArguments(method);
            }
            if (ParameterIsAssignable(parametersCached[0], left.Type) && ParameterIsAssignable(parametersCached[1], right.Type))
            {
                ValidateParamswithOperandsOrThrow(parametersCached[0].ParameterType, left.Type, binaryType, method.Name);
                ValidateParamswithOperandsOrThrow(parametersCached[1].ParameterType, right.Type, binaryType, method.Name);
                return new MethodBinaryExpression(binaryType, left, right, method.ReturnType, method);
            }
            if (((!left.Type.IsNullableType() || !right.Type.IsNullableType()) || (!ParameterIsAssignable(parametersCached[0], left.Type.GetNonNullableType()) || !ParameterIsAssignable(parametersCached[1], right.Type.GetNonNullableType()))) || (!method.ReturnType.IsValueType || method.ReturnType.IsNullableType()))
            {
                throw System.Linq.Expressions.Error.OperandTypesDoNotMatchParameters(binaryType, method.Name);
            }
            if (!(method.ReturnType != typeof(bool)) && !liftToNull)
            {
                return new MethodBinaryExpression(binaryType, left, right, typeof(bool), method);
            }
            return new MethodBinaryExpression(binaryType, left, right, TypeUtils.GetNullableType(method.ReturnType), method);
        }

        private static UnaryExpression GetMethodBasedCoercionOperator(ExpressionType unaryType, Expression operand, System.Type convertToType, MethodInfo method)
        {
            ValidateOperator(method);
            ParameterInfo[] parametersCached = method.GetParametersCached();
            if (parametersCached.Length != 1)
            {
                throw System.Linq.Expressions.Error.IncorrectNumberOfMethodCallArguments(method);
            }
            if (ParameterIsAssignable(parametersCached[0], operand.Type) && TypeUtils.AreEquivalent(method.ReturnType, convertToType))
            {
                return new UnaryExpression(unaryType, operand, method.ReturnType, method);
            }
            if ((!operand.Type.IsNullableType() && !convertToType.IsNullableType()) || (!ParameterIsAssignable(parametersCached[0], operand.Type.GetNonNullableType()) || !TypeUtils.AreEquivalent(method.ReturnType, convertToType.GetNonNullableType())))
            {
                throw System.Linq.Expressions.Error.OperandTypesDoNotMatchParameters(unaryType, method.Name);
            }
            return new UnaryExpression(unaryType, operand, convertToType, method);
        }

        private static UnaryExpression GetMethodBasedUnaryOperator(ExpressionType unaryType, Expression operand, MethodInfo method)
        {
            ValidateOperator(method);
            ParameterInfo[] parametersCached = method.GetParametersCached();
            if (parametersCached.Length != 1)
            {
                throw System.Linq.Expressions.Error.IncorrectNumberOfMethodCallArguments(method);
            }
            if (ParameterIsAssignable(parametersCached[0], operand.Type))
            {
                ValidateParamswithOperandsOrThrow(parametersCached[0].ParameterType, operand.Type, unaryType, method.Name);
                return new UnaryExpression(unaryType, operand, method.ReturnType, method);
            }
            if ((!operand.Type.IsNullableType() || !ParameterIsAssignable(parametersCached[0], operand.Type.GetNonNullableType())) || (!method.ReturnType.IsValueType || method.ReturnType.IsNullableType()))
            {
                throw System.Linq.Expressions.Error.OperandTypesDoNotMatchParameters(unaryType, method.Name);
            }
            return new UnaryExpression(unaryType, operand, TypeUtils.GetNullableType(method.ReturnType), method);
        }

        private static ParameterInfo[] GetParametersForValidation(MethodBase method, ExpressionType nodeKind)
        {
            ParameterInfo[] parametersCached = method.GetParametersCached();
            if (nodeKind == ExpressionType.Dynamic)
            {
                parametersCached = parametersCached.RemoveFirst<ParameterInfo>();
            }
            return parametersCached;
        }

        private static PropertyInfo GetProperty(MethodInfo mi)
        {
            System.Type declaringType = mi.DeclaringType;
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public;
            bindingAttr |= mi.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
            foreach (PropertyInfo info in declaringType.GetProperties(bindingAttr))
            {
                if (info.CanRead && CheckMethod(mi, info.GetGetMethod(true)))
                {
                    return info;
                }
                if (info.CanWrite && CheckMethod(mi, info.GetSetMethod(true)))
                {
                    return info;
                }
            }
            throw System.Linq.Expressions.Error.MethodNotPropertyAccessor(mi.DeclaringType, mi.Name);
        }

        private static System.Type GetResultTypeOfShift(System.Type left, System.Type right)
        {
            if (!left.IsNullableType() && right.IsNullableType())
            {
                return typeof(Nullable<>).MakeGenericType(new System.Type[] { left });
            }
            return left;
        }

        private static BinaryExpression GetUserDefinedAssignOperatorOrThrow(ExpressionType binaryType, string name, Expression left, Expression right, LambdaExpression conversion, bool liftToNull)
        {
            BinaryExpression expression = GetUserDefinedBinaryOperatorOrThrow(binaryType, name, left, right, liftToNull);
            if (conversion == null)
            {
                if (!TypeUtils.AreReferenceAssignable(left.Type, expression.Type))
                {
                    throw System.Linq.Expressions.Error.UserDefinedOpMustHaveValidReturnType(binaryType, expression.Method.Name);
                }
                return expression;
            }
            ValidateOpAssignConversionLambda(conversion, expression.Left, expression.Method, expression.NodeType);
            return new OpAssignMethodConversionBinaryExpression(expression.NodeType, expression.Left, expression.Right, expression.Left.Type, expression.Method, conversion);
        }

        private static MethodInfo GetUserDefinedBinaryOperator(ExpressionType binaryType, System.Type leftType, System.Type rightType, string name)
        {
            System.Type[] types = new System.Type[] { leftType, rightType };
            System.Type nonNullableType = leftType.GetNonNullableType();
            System.Type type = rightType.GetNonNullableType();
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            MethodInfo method = nonNullableType.GetMethodValidated(name, bindingAttr, null, types, null);
            if ((method == null) && !TypeUtils.AreEquivalent(leftType, rightType))
            {
                method = type.GetMethodValidated(name, bindingAttr, null, types, null);
            }
            if (IsLiftingConditionalLogicalOperator(leftType, rightType, method, binaryType))
            {
                method = GetUserDefinedBinaryOperator(binaryType, nonNullableType, type, name);
            }
            return method;
        }

        private static BinaryExpression GetUserDefinedBinaryOperator(ExpressionType binaryType, string name, Expression left, Expression right, bool liftToNull)
        {
            MethodInfo method = GetUserDefinedBinaryOperator(binaryType, left.Type, right.Type, name);
            if (method != null)
            {
                return new MethodBinaryExpression(binaryType, left, right, method.ReturnType, method);
            }
            if (left.Type.IsNullableType() && right.Type.IsNullableType())
            {
                System.Type nonNullableType = left.Type.GetNonNullableType();
                System.Type rightType = right.Type.GetNonNullableType();
                method = GetUserDefinedBinaryOperator(binaryType, nonNullableType, rightType, name);
                if (((method != null) && method.ReturnType.IsValueType) && !method.ReturnType.IsNullableType())
                {
                    if (!(method.ReturnType != typeof(bool)) && !liftToNull)
                    {
                        return new MethodBinaryExpression(binaryType, left, right, typeof(bool), method);
                    }
                    return new MethodBinaryExpression(binaryType, left, right, TypeUtils.GetNullableType(method.ReturnType), method);
                }
            }
            return null;
        }

        private static BinaryExpression GetUserDefinedBinaryOperatorOrThrow(ExpressionType binaryType, string name, Expression left, Expression right, bool liftToNull)
        {
            BinaryExpression expression = GetUserDefinedBinaryOperator(binaryType, name, left, right, liftToNull);
            if (expression == null)
            {
                throw System.Linq.Expressions.Error.BinaryOperatorNotDefined(binaryType, left.Type, right.Type);
            }
            ParameterInfo[] parametersCached = expression.Method.GetParametersCached();
            ValidateParamswithOperandsOrThrow(parametersCached[0].ParameterType, left.Type, binaryType, name);
            ValidateParamswithOperandsOrThrow(parametersCached[1].ParameterType, right.Type, binaryType, name);
            return expression;
        }

        private static UnaryExpression GetUserDefinedCoercion(ExpressionType coercionType, Expression expression, System.Type convertToType)
        {
            MethodInfo method = TypeUtils.GetUserDefinedCoercionMethod(expression.Type, convertToType, false);
            if (method != null)
            {
                return new UnaryExpression(coercionType, expression, convertToType, method);
            }
            return null;
        }

        private static UnaryExpression GetUserDefinedCoercionOrThrow(ExpressionType coercionType, Expression expression, System.Type convertToType)
        {
            UnaryExpression expression2 = GetUserDefinedCoercion(coercionType, expression, convertToType);
            if (expression2 == null)
            {
                throw System.Linq.Expressions.Error.CoercionOperatorNotDefined(expression.Type, convertToType);
            }
            return expression2;
        }

        private static UnaryExpression GetUserDefinedUnaryOperator(ExpressionType unaryType, string name, Expression operand)
        {
            System.Type type = operand.Type;
            System.Type[] types = new System.Type[] { type };
            System.Type nonNullableType = type.GetNonNullableType();
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            MethodInfo method = nonNullableType.GetMethodValidated(name, bindingAttr, null, types, null);
            if (method != null)
            {
                return new UnaryExpression(unaryType, operand, method.ReturnType, method);
            }
            if (type.IsNullableType())
            {
                types[0] = nonNullableType;
                method = nonNullableType.GetMethodValidated(name, bindingAttr, null, types, null);
                if (((method != null) && method.ReturnType.IsValueType) && !method.ReturnType.IsNullableType())
                {
                    return new UnaryExpression(unaryType, operand, TypeUtils.GetNullableType(method.ReturnType), method);
                }
            }
            return null;
        }

        private static UnaryExpression GetUserDefinedUnaryOperatorOrThrow(ExpressionType unaryType, string name, Expression operand)
        {
            UnaryExpression expression = GetUserDefinedUnaryOperator(unaryType, name, operand);
            if (expression == null)
            {
                throw System.Linq.Expressions.Error.UnaryOperatorNotDefined(unaryType, operand.Type);
            }
            ValidateParamswithOperandsOrThrow(expression.Method.GetParametersCached()[0].ParameterType, operand.Type, unaryType, name);
            return expression;
        }

        private static MethodInfo GetValidMethodForDynamic(System.Type delegateType)
        {
            MethodInfo method = delegateType.GetMethod("Invoke");
            ParameterInfo[] parametersCached = method.GetParametersCached();
            if ((parametersCached.Length == 0) || (parametersCached[0].ParameterType != typeof(CallSite)))
            {
                throw System.Linq.Expressions.Error.FirstArgumentMustBeCallSite();
            }
            return method;
        }

        public static GotoExpression Goto(LabelTarget target)
        {
            return MakeGoto(GotoExpressionKind.Goto, target, null, typeof(void));
        }

        public static GotoExpression Goto(LabelTarget target, Expression value)
        {
            return MakeGoto(GotoExpressionKind.Goto, target, value, typeof(void));
        }

        public static GotoExpression Goto(LabelTarget target, System.Type type)
        {
            return MakeGoto(GotoExpressionKind.Goto, target, null, type);
        }

        public static GotoExpression Goto(LabelTarget target, Expression value, System.Type type)
        {
            return MakeGoto(GotoExpressionKind.Goto, target, value, type);
        }

        public static BinaryExpression GreaterThan(Expression left, Expression right)
        {
            return GreaterThan(left, right, false, null);
        }

        public static BinaryExpression GreaterThan(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null)
            {
                return GetComparisonOperator(ExpressionType.GreaterThan, "op_GreaterThan", left, right, liftToNull);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.GreaterThan, left, right, method, liftToNull);
        }

        public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right)
        {
            return GreaterThanOrEqual(left, right, false, null);
        }

        public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null)
            {
                return GetComparisonOperator(ExpressionType.GreaterThanOrEqual, "op_GreaterThanOrEqual", left, right, liftToNull);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.GreaterThanOrEqual, left, right, method, liftToNull);
        }

        public static ConditionalExpression IfThen(Expression test, Expression ifTrue)
        {
            return Condition(test, ifTrue, Empty(), typeof(void));
        }

        public static ConditionalExpression IfThenElse(Expression test, Expression ifTrue, Expression ifFalse)
        {
            return Condition(test, ifTrue, ifFalse, typeof(void));
        }

        public static UnaryExpression Increment(Expression expression)
        {
            return Increment(expression, null);
        }

        public static UnaryExpression Increment(Expression expression, MethodInfo method)
        {
            RequiresCanRead(expression, "expression");
            if (method != null)
            {
                return GetMethodBasedUnaryOperator(ExpressionType.Increment, expression, method);
            }
            if (TypeUtils.IsArithmetic(expression.Type))
            {
                return new UnaryExpression(ExpressionType.Increment, expression, expression.Type, null);
            }
            return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Increment, "op_Increment", expression);
        }

        public static InvocationExpression Invoke(Expression expression, params Expression[] arguments)
        {
            return Invoke(expression, (IEnumerable<Expression>) arguments);
        }

        public static InvocationExpression Invoke(Expression expression, IEnumerable<Expression> arguments)
        {
            RequiresCanRead(expression, "expression");
            ReadOnlyCollection<Expression> onlys = arguments.ToReadOnly<Expression>();
            MethodInfo invokeMethod = GetInvokeMethod(expression);
            ValidateArgumentTypes(invokeMethod, ExpressionType.Invoke, ref onlys);
            return new InvocationExpression(expression, onlys, invokeMethod.ReturnType);
        }

        private static bool IsCompatible(MethodBase m, Expression[] args)
        {
            ParameterInfo[] parametersCached = m.GetParametersCached();
            if (parametersCached.Length != args.Length)
            {
                return false;
            }
            for (int i = 0; i < args.Length; i++)
            {
                Expression expression = args[i];
                ContractUtils.RequiresNotNull(expression, "argument");
                System.Type src = expression.Type;
                System.Type parameterType = parametersCached[i].ParameterType;
                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                }
                if (!TypeUtils.AreReferenceAssignable(parameterType, src) && (!TypeUtils.IsSameOrSubclass(typeof(LambdaExpression), parameterType) || !parameterType.IsAssignableFrom(expression.GetType())))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsCompatible(PropertyInfo pi, Expression[] args)
        {
            ParameterInfo[] parametersCached;
            MethodInfo getMethod = pi.GetGetMethod(true);
            if (getMethod != null)
            {
                parametersCached = getMethod.GetParametersCached();
            }
            else
            {
                getMethod = pi.GetSetMethod(true);
                parametersCached = getMethod.GetParametersCached().RemoveLast<ParameterInfo>();
            }
            if (getMethod == null)
            {
                return false;
            }
            if (args == null)
            {
                return (parametersCached.Length == 0);
            }
            if (parametersCached.Length != args.Length)
            {
                return false;
            }
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                {
                    return false;
                }
                if (!TypeUtils.AreReferenceAssignable(parametersCached[i].ParameterType, args[i].Type))
                {
                    return false;
                }
            }
            return true;
        }

        public static UnaryExpression IsFalse(Expression expression)
        {
            return IsFalse(expression, null);
        }

        public static UnaryExpression IsFalse(Expression expression, MethodInfo method)
        {
            RequiresCanRead(expression, "expression");
            if (method != null)
            {
                return GetMethodBasedUnaryOperator(ExpressionType.IsFalse, expression, method);
            }
            if (TypeUtils.IsBool(expression.Type))
            {
                return new UnaryExpression(ExpressionType.IsFalse, expression, expression.Type, null);
            }
            return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.IsFalse, "op_False", expression);
        }

        private static bool IsLiftingConditionalLogicalOperator(System.Type left, System.Type right, MethodInfo method, ExpressionType binaryType)
        {
            if ((!right.IsNullableType() || !left.IsNullableType()) || (method != null))
            {
                return false;
            }
            if (binaryType != ExpressionType.AndAlso)
            {
                return (binaryType == ExpressionType.OrElse);
            }
            return true;
        }

        private static bool IsNullComparison(Expression left, Expression right)
        {
            return (((IsNullConstant(left) && !IsNullConstant(right)) && right.Type.IsNullableType()) || ((IsNullConstant(right) && !IsNullConstant(left)) && left.Type.IsNullableType()));
        }

        private static bool IsNullConstant(Expression e)
        {
            ConstantExpression expression = e as ConstantExpression;
            return ((expression != null) && (expression.Value == null));
        }

        private static bool IsSimpleShift(System.Type left, System.Type right)
        {
            return (TypeUtils.IsInteger(left) && (right.GetNonNullableType() == typeof(int)));
        }

        public static UnaryExpression IsTrue(Expression expression)
        {
            return IsTrue(expression, null);
        }

        public static UnaryExpression IsTrue(Expression expression, MethodInfo method)
        {
            RequiresCanRead(expression, "expression");
            if (method != null)
            {
                return GetMethodBasedUnaryOperator(ExpressionType.IsTrue, expression, method);
            }
            if (TypeUtils.IsBool(expression.Type))
            {
                return new UnaryExpression(ExpressionType.IsTrue, expression, expression.Type, null);
            }
            return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.IsTrue, "op_True", expression);
        }

        private static bool IsValidLiftedConditionalLogicalOperator(System.Type left, System.Type right, ParameterInfo[] pms)
        {
            return ((TypeUtils.AreEquivalent(left, right) && right.IsNullableType()) && TypeUtils.AreEquivalent(pms[1].ParameterType, right.GetNonNullableType()));
        }

        public static LabelTarget Label()
        {
            return Label(typeof(void), null);
        }

        public static LabelExpression Label(LabelTarget target)
        {
            return Label(target, null);
        }

        public static LabelTarget Label(string name)
        {
            return Label(typeof(void), name);
        }

        public static LabelTarget Label(System.Type type)
        {
            return Label(type, null);
        }

        public static LabelExpression Label(LabelTarget target, Expression defaultValue)
        {
            ValidateGoto(target, ref defaultValue, "label", "defaultValue");
            return new LabelExpression(target, defaultValue);
        }

        public static LabelTarget Label(System.Type type, string name)
        {
            ContractUtils.RequiresNotNull(type, "type");
            TypeUtils.ValidateType(type);
            return new LabelTarget(type, name);
        }

        public static LambdaExpression Lambda(Expression body, params ParameterExpression[] parameters)
        {
            return Lambda(body, false, (IEnumerable<ParameterExpression>) parameters);
        }

        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, IEnumerable<ParameterExpression> parameters)
        {
            return Lambda<TDelegate>(body, null, false, parameters);
        }

        public static LambdaExpression Lambda(Expression body, IEnumerable<ParameterExpression> parameters)
        {
            return Lambda(body, null, false, parameters);
        }

        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, params ParameterExpression[] parameters)
        {
            return Lambda<TDelegate>(body, false, (IEnumerable<ParameterExpression>) parameters);
        }

        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, bool tailCall, IEnumerable<ParameterExpression> parameters)
        {
            return Lambda<TDelegate>(body, null, tailCall, parameters);
        }

        public static LambdaExpression Lambda(Expression body, bool tailCall, params ParameterExpression[] parameters)
        {
            return Lambda(body, tailCall, (IEnumerable<ParameterExpression>) parameters);
        }

        public static LambdaExpression Lambda(Expression body, bool tailCall, IEnumerable<ParameterExpression> parameters)
        {
            return Lambda(body, null, tailCall, parameters);
        }

        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, bool tailCall, params ParameterExpression[] parameters)
        {
            return Lambda<TDelegate>(body, tailCall, (IEnumerable<ParameterExpression>) parameters);
        }

        public static LambdaExpression Lambda(Expression body, string name, IEnumerable<ParameterExpression> parameters)
        {
            return Lambda(body, name, false, parameters);
        }

        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, string name, IEnumerable<ParameterExpression> parameters)
        {
            return Lambda<TDelegate>(body, name, false, parameters);
        }

        public static LambdaExpression Lambda(System.Type delegateType, Expression body, params ParameterExpression[] parameters)
        {
            return Lambda(delegateType, body, null, false, parameters);
        }

        public static LambdaExpression Lambda(System.Type delegateType, Expression body, IEnumerable<ParameterExpression> parameters)
        {
            return Lambda(delegateType, body, null, false, parameters);
        }

        public static LambdaExpression Lambda(Expression body, string name, bool tailCall, IEnumerable<ParameterExpression> parameters)
        {
            ContractUtils.RequiresNotNull(body, "body");
            ReadOnlyCollection<ParameterExpression> onlys = parameters.ToReadOnly<ParameterExpression>();
            int count = onlys.Count;
            System.Type[] types = new System.Type[count + 1];
            if (count > 0)
            {
                System.Linq.Expressions.Set<ParameterExpression> set = new System.Linq.Expressions.Set<ParameterExpression>(onlys.Count);
                for (int i = 0; i < count; i++)
                {
                    ParameterExpression expression = onlys[i];
                    ContractUtils.RequiresNotNull(expression, "parameter");
                    types[i] = expression.IsByRef ? expression.Type.MakeByRefType() : expression.Type;
                    if (set.Contains(expression))
                    {
                        throw System.Linq.Expressions.Error.DuplicateVariable(expression);
                    }
                    set.Add(expression);
                }
            }
            types[count] = body.Type;
            return CreateLambda(DelegateHelpers.MakeDelegateType(types), body, name, tailCall, onlys);
        }

        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, string name, bool tailCall, IEnumerable<ParameterExpression> parameters)
        {
            ReadOnlyCollection<ParameterExpression> onlys = parameters.ToReadOnly<ParameterExpression>();
            ValidateLambdaArgs(typeof(TDelegate), ref body, onlys);
            return new Expression<TDelegate>(body, name, tailCall, onlys);
        }

        public static LambdaExpression Lambda(System.Type delegateType, Expression body, bool tailCall, params ParameterExpression[] parameters)
        {
            return Lambda(delegateType, body, null, tailCall, parameters);
        }

        public static LambdaExpression Lambda(System.Type delegateType, Expression body, bool tailCall, IEnumerable<ParameterExpression> parameters)
        {
            return Lambda(delegateType, body, null, tailCall, parameters);
        }

        public static LambdaExpression Lambda(System.Type delegateType, Expression body, string name, IEnumerable<ParameterExpression> parameters)
        {
            ReadOnlyCollection<ParameterExpression> onlys = parameters.ToReadOnly<ParameterExpression>();
            ValidateLambdaArgs(delegateType, ref body, onlys);
            return CreateLambda(delegateType, body, name, false, onlys);
        }

        public static LambdaExpression Lambda(System.Type delegateType, Expression body, string name, bool tailCall, IEnumerable<ParameterExpression> parameters)
        {
            ReadOnlyCollection<ParameterExpression> onlys = parameters.ToReadOnly<ParameterExpression>();
            ValidateLambdaArgs(delegateType, ref body, onlys);
            return CreateLambda(delegateType, body, name, tailCall, onlys);
        }

        public static BinaryExpression LeftShift(Expression left, Expression right)
        {
            return LeftShift(left, right, null);
        }

        public static BinaryExpression LeftShift(Expression left, Expression right, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedBinaryOperator(ExpressionType.LeftShift, left, right, method, true);
            }
            if (IsSimpleShift(left.Type, right.Type))
            {
                return new SimpleBinaryExpression(ExpressionType.LeftShift, left, right, GetResultTypeOfShift(left.Type, right.Type));
            }
            return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.LeftShift, "op_LeftShift", left, right, true);
        }

        public static BinaryExpression LeftShiftAssign(Expression left, Expression right)
        {
            return LeftShiftAssign(left, right, null, null);
        }

        public static BinaryExpression LeftShiftAssign(Expression left, Expression right, MethodInfo method)
        {
            return LeftShiftAssign(left, right, method, null);
        }

        public static BinaryExpression LeftShiftAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion)
        {
            RequiresCanRead(left, "left");
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedAssignOperator(ExpressionType.LeftShiftAssign, left, right, method, conversion, true);
            }
            if (!IsSimpleShift(left.Type, right.Type))
            {
                return GetUserDefinedAssignOperatorOrThrow(ExpressionType.LeftShiftAssign, "op_LeftShift", left, right, conversion, true);
            }
            if (conversion != null)
            {
                throw System.Linq.Expressions.Error.ConversionIsNotSupportedForArithmeticTypes();
            }
            return new SimpleBinaryExpression(ExpressionType.LeftShiftAssign, left, right, GetResultTypeOfShift(left.Type, right.Type));
        }

        public static BinaryExpression LessThan(Expression left, Expression right)
        {
            return LessThan(left, right, false, null);
        }

        public static BinaryExpression LessThan(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null)
            {
                return GetComparisonOperator(ExpressionType.LessThan, "op_LessThan", left, right, liftToNull);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.LessThan, left, right, method, liftToNull);
        }

        public static BinaryExpression LessThanOrEqual(Expression left, Expression right)
        {
            return LessThanOrEqual(left, right, false, null);
        }

        public static BinaryExpression LessThanOrEqual(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null)
            {
                return GetComparisonOperator(ExpressionType.LessThanOrEqual, "op_LessThanOrEqual", left, right, liftToNull);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.LessThanOrEqual, left, right, method, liftToNull);
        }

        public static MemberListBinding ListBind(MemberInfo member, params System.Linq.Expressions.ElementInit[] initializers)
        {
            ContractUtils.RequiresNotNull(member, "member");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            return ListBind(member, (IEnumerable<System.Linq.Expressions.ElementInit>) initializers);
        }

        public static MemberListBinding ListBind(MemberInfo member, IEnumerable<System.Linq.Expressions.ElementInit> initializers)
        {
            System.Type type;
            ContractUtils.RequiresNotNull(member, "member");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            ValidateGettableFieldOrPropertyMember(member, out type);
            ReadOnlyCollection<System.Linq.Expressions.ElementInit> onlys = initializers.ToReadOnly<System.Linq.Expressions.ElementInit>();
            ValidateListInitArgs(type, onlys);
            return new MemberListBinding(member, onlys);
        }

        public static MemberListBinding ListBind(MethodInfo propertyAccessor, params System.Linq.Expressions.ElementInit[] initializers)
        {
            ContractUtils.RequiresNotNull(propertyAccessor, "propertyAccessor");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            return ListBind(propertyAccessor, (IEnumerable<System.Linq.Expressions.ElementInit>) initializers);
        }

        public static MemberListBinding ListBind(MethodInfo propertyAccessor, IEnumerable<System.Linq.Expressions.ElementInit> initializers)
        {
            ContractUtils.RequiresNotNull(propertyAccessor, "propertyAccessor");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            return ListBind(GetProperty(propertyAccessor), initializers);
        }

        public static ListInitExpression ListInit(NewExpression newExpression, params System.Linq.Expressions.ElementInit[] initializers)
        {
            return ListInit(newExpression, (IEnumerable<System.Linq.Expressions.ElementInit>) initializers);
        }

        public static ListInitExpression ListInit(NewExpression newExpression, params Expression[] initializers)
        {
            ContractUtils.RequiresNotNull(newExpression, "newExpression");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            return ListInit(newExpression, (IEnumerable<Expression>) initializers);
        }

        public static ListInitExpression ListInit(NewExpression newExpression, IEnumerable<System.Linq.Expressions.ElementInit> initializers)
        {
            ContractUtils.RequiresNotNull(newExpression, "newExpression");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            ReadOnlyCollection<System.Linq.Expressions.ElementInit> onlys = initializers.ToReadOnly<System.Linq.Expressions.ElementInit>();
            if (onlys.Count == 0)
            {
                throw System.Linq.Expressions.Error.ListInitializerWithZeroMembers();
            }
            ValidateListInitArgs(newExpression.Type, onlys);
            return new ListInitExpression(newExpression, onlys);
        }

        public static ListInitExpression ListInit(NewExpression newExpression, IEnumerable<Expression> initializers)
        {
            ContractUtils.RequiresNotNull(newExpression, "newExpression");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            ReadOnlyCollection<Expression> onlys = initializers.ToReadOnly<Expression>();
            if (onlys.Count == 0)
            {
                throw System.Linq.Expressions.Error.ListInitializerWithZeroMembers();
            }
            MethodInfo addMethod = FindMethod(newExpression.Type, "Add", null, new Expression[] { onlys[0] }, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            return ListInit(newExpression, addMethod, initializers);
        }

        public static ListInitExpression ListInit(NewExpression newExpression, MethodInfo addMethod, params Expression[] initializers)
        {
            if (addMethod == null)
            {
                return ListInit(newExpression, (IEnumerable<Expression>) initializers);
            }
            ContractUtils.RequiresNotNull(newExpression, "newExpression");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            return ListInit(newExpression, addMethod, (IEnumerable<Expression>) initializers);
        }

        public static ListInitExpression ListInit(NewExpression newExpression, MethodInfo addMethod, IEnumerable<Expression> initializers)
        {
            if (addMethod == null)
            {
                return ListInit(newExpression, initializers);
            }
            ContractUtils.RequiresNotNull(newExpression, "newExpression");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            ReadOnlyCollection<Expression> onlys = initializers.ToReadOnly<Expression>();
            if (onlys.Count == 0)
            {
                throw System.Linq.Expressions.Error.ListInitializerWithZeroMembers();
            }
            System.Linq.Expressions.ElementInit[] list = new System.Linq.Expressions.ElementInit[onlys.Count];
            for (int i = 0; i < onlys.Count; i++)
            {
                list[i] = ElementInit(addMethod, new Expression[] { onlys[i] });
            }
            return ListInit(newExpression, new TrueReadOnlyCollection<System.Linq.Expressions.ElementInit>(list));
        }

        public static LoopExpression Loop(Expression body)
        {
            return Loop(body, null);
        }

        public static LoopExpression Loop(Expression body, LabelTarget @break)
        {
            return Loop(body, @break, null);
        }

        public static LoopExpression Loop(Expression body, LabelTarget @break, LabelTarget @continue)
        {
            RequiresCanRead(body, "body");
            if ((@continue != null) && (@continue.Type != typeof(void)))
            {
                throw System.Linq.Expressions.Error.LabelTypeMustBeVoid();
            }
            return new LoopExpression(body, @break, @continue);
        }

        public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right)
        {
            return MakeBinary(binaryType, left, right, false, null, null);
        }

        public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            return MakeBinary(binaryType, left, right, liftToNull, method, null);
        }

        public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right, bool liftToNull, MethodInfo method, LambdaExpression conversion)
        {
            switch (binaryType)
            {
                case ExpressionType.Add:
                    return Add(left, right, method);

                case ExpressionType.AddChecked:
                    return AddChecked(left, right, method);

                case ExpressionType.And:
                    return And(left, right, method);

                case ExpressionType.AndAlso:
                    return AndAlso(left, right, method);

                case ExpressionType.ArrayIndex:
                    return ArrayIndex(left, right);

                case ExpressionType.Coalesce:
                    return Coalesce(left, right, conversion);

                case ExpressionType.Divide:
                    return Divide(left, right, method);

                case ExpressionType.Equal:
                    return Equal(left, right, liftToNull, method);

                case ExpressionType.ExclusiveOr:
                    return ExclusiveOr(left, right, method);

                case ExpressionType.GreaterThan:
                    return GreaterThan(left, right, liftToNull, method);

                case ExpressionType.GreaterThanOrEqual:
                    return GreaterThanOrEqual(left, right, liftToNull, method);

                case ExpressionType.LeftShift:
                    return LeftShift(left, right, method);

                case ExpressionType.LessThan:
                    return LessThan(left, right, liftToNull, method);

                case ExpressionType.LessThanOrEqual:
                    return LessThanOrEqual(left, right, liftToNull, method);

                case ExpressionType.Modulo:
                    return Modulo(left, right, method);

                case ExpressionType.Multiply:
                    return Multiply(left, right, method);

                case ExpressionType.MultiplyChecked:
                    return MultiplyChecked(left, right, method);

                case ExpressionType.NotEqual:
                    return NotEqual(left, right, liftToNull, method);

                case ExpressionType.Or:
                    return Or(left, right, method);

                case ExpressionType.OrElse:
                    return OrElse(left, right, method);

                case ExpressionType.Power:
                    return Power(left, right, method);

                case ExpressionType.RightShift:
                    return RightShift(left, right, method);

                case ExpressionType.Subtract:
                    return Subtract(left, right, method);

                case ExpressionType.SubtractChecked:
                    return SubtractChecked(left, right, method);

                case ExpressionType.Assign:
                    return Assign(left, right);

                case ExpressionType.AddAssign:
                    return AddAssign(left, right, method, conversion);

                case ExpressionType.AndAssign:
                    return AndAssign(left, right, method, conversion);

                case ExpressionType.DivideAssign:
                    return DivideAssign(left, right, method, conversion);

                case ExpressionType.ExclusiveOrAssign:
                    return ExclusiveOrAssign(left, right, method, conversion);

                case ExpressionType.LeftShiftAssign:
                    return LeftShiftAssign(left, right, method, conversion);

                case ExpressionType.ModuloAssign:
                    return ModuloAssign(left, right, method, conversion);

                case ExpressionType.MultiplyAssign:
                    return MultiplyAssign(left, right, method, conversion);

                case ExpressionType.OrAssign:
                    return OrAssign(left, right, method, conversion);

                case ExpressionType.PowerAssign:
                    return PowerAssign(left, right, method, conversion);

                case ExpressionType.RightShiftAssign:
                    return RightShiftAssign(left, right, method, conversion);

                case ExpressionType.SubtractAssign:
                    return SubtractAssign(left, right, method, conversion);

                case ExpressionType.AddAssignChecked:
                    return AddAssignChecked(left, right, method, conversion);

                case ExpressionType.MultiplyAssignChecked:
                    return MultiplyAssignChecked(left, right, method, conversion);

                case ExpressionType.SubtractAssignChecked:
                    return SubtractAssignChecked(left, right, method, conversion);
            }
            throw System.Linq.Expressions.Error.UnhandledBinary(binaryType);
        }

        public static CatchBlock MakeCatchBlock(System.Type type, ParameterExpression variable, Expression body, Expression filter)
        {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires((variable == null) || TypeUtils.AreEquivalent(variable.Type, type), "variable");
            if ((variable != null) && variable.IsByRef)
            {
                throw System.Linq.Expressions.Error.VariableMustNotBeByRef(variable, variable.Type);
            }
            RequiresCanRead(body, "body");
            if (filter != null)
            {
                RequiresCanRead(filter, "filter");
                if (filter.Type != typeof(bool))
                {
                    throw System.Linq.Expressions.Error.ArgumentMustBeBoolean();
                }
            }
            return new CatchBlock(type, variable, body, filter);
        }

        private static DynamicExpression MakeDynamic(CallSiteBinder binder, System.Type returnType, ReadOnlyCollection<Expression> args)
        {
            ContractUtils.RequiresNotNull(binder, "binder");
            for (int i = 0; i < args.Count; i++)
            {
                Expression arg = args[i];
                ValidateDynamicArgument(arg);
            }
            System.Type delegateType = DelegateHelpers.MakeCallSiteDelegate(args, returnType);
            switch (args.Count)
            {
                case 1:
                    return DynamicExpression.Make(returnType, delegateType, binder, args[0]);

                case 2:
                    return DynamicExpression.Make(returnType, delegateType, binder, args[0], args[1]);

                case 3:
                    return DynamicExpression.Make(returnType, delegateType, binder, args[0], args[1], args[2]);

                case 4:
                    return DynamicExpression.Make(returnType, delegateType, binder, args[0], args[1], args[2], args[3]);
            }
            return DynamicExpression.Make(returnType, delegateType, binder, args);
        }

        public static DynamicExpression MakeDynamic(System.Type delegateType, CallSiteBinder binder, params Expression[] arguments)
        {
            return MakeDynamic(delegateType, binder, (IEnumerable<Expression>) arguments);
        }

        public static DynamicExpression MakeDynamic(System.Type delegateType, CallSiteBinder binder, Expression arg0)
        {
            ContractUtils.RequiresNotNull(delegateType, "delegatType");
            ContractUtils.RequiresNotNull(binder, "binder");
            if (!delegateType.IsSubclassOf(typeof(Delegate)))
            {
                throw System.Linq.Expressions.Error.TypeMustBeDerivedFromSystemDelegate();
            }
            MethodInfo validMethodForDynamic = GetValidMethodForDynamic(delegateType);
            ParameterInfo[] parametersCached = validMethodForDynamic.GetParametersCached();
            ValidateArgumentCount(validMethodForDynamic, ExpressionType.Dynamic, 2, parametersCached);
            ValidateDynamicArgument(arg0);
            ValidateOneArgument(validMethodForDynamic, ExpressionType.Dynamic, arg0, parametersCached[1]);
            return DynamicExpression.Make(validMethodForDynamic.GetReturnType(), delegateType, binder, arg0);
        }

        public static DynamicExpression MakeDynamic(System.Type delegateType, CallSiteBinder binder, IEnumerable<Expression> arguments)
        {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            ContractUtils.RequiresNotNull(binder, "binder");
            if (!delegateType.IsSubclassOf(typeof(Delegate)))
            {
                throw System.Linq.Expressions.Error.TypeMustBeDerivedFromSystemDelegate();
            }
            MethodInfo validMethodForDynamic = GetValidMethodForDynamic(delegateType);
            ReadOnlyCollection<Expression> onlys = arguments.ToReadOnly<Expression>();
            ValidateArgumentTypes(validMethodForDynamic, ExpressionType.Dynamic, ref onlys);
            return DynamicExpression.Make(validMethodForDynamic.GetReturnType(), delegateType, binder, onlys);
        }

        public static DynamicExpression MakeDynamic(System.Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1)
        {
            ContractUtils.RequiresNotNull(delegateType, "delegatType");
            ContractUtils.RequiresNotNull(binder, "binder");
            if (!delegateType.IsSubclassOf(typeof(Delegate)))
            {
                throw System.Linq.Expressions.Error.TypeMustBeDerivedFromSystemDelegate();
            }
            MethodInfo validMethodForDynamic = GetValidMethodForDynamic(delegateType);
            ParameterInfo[] parametersCached = validMethodForDynamic.GetParametersCached();
            ValidateArgumentCount(validMethodForDynamic, ExpressionType.Dynamic, 3, parametersCached);
            ValidateDynamicArgument(arg0);
            ValidateOneArgument(validMethodForDynamic, ExpressionType.Dynamic, arg0, parametersCached[1]);
            ValidateDynamicArgument(arg1);
            ValidateOneArgument(validMethodForDynamic, ExpressionType.Dynamic, arg1, parametersCached[2]);
            return DynamicExpression.Make(validMethodForDynamic.GetReturnType(), delegateType, binder, arg0, arg1);
        }

        public static DynamicExpression MakeDynamic(System.Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2)
        {
            ContractUtils.RequiresNotNull(delegateType, "delegatType");
            ContractUtils.RequiresNotNull(binder, "binder");
            if (!delegateType.IsSubclassOf(typeof(Delegate)))
            {
                throw System.Linq.Expressions.Error.TypeMustBeDerivedFromSystemDelegate();
            }
            MethodInfo validMethodForDynamic = GetValidMethodForDynamic(delegateType);
            ParameterInfo[] parametersCached = validMethodForDynamic.GetParametersCached();
            ValidateArgumentCount(validMethodForDynamic, ExpressionType.Dynamic, 4, parametersCached);
            ValidateDynamicArgument(arg0);
            ValidateOneArgument(validMethodForDynamic, ExpressionType.Dynamic, arg0, parametersCached[1]);
            ValidateDynamicArgument(arg1);
            ValidateOneArgument(validMethodForDynamic, ExpressionType.Dynamic, arg1, parametersCached[2]);
            ValidateDynamicArgument(arg2);
            ValidateOneArgument(validMethodForDynamic, ExpressionType.Dynamic, arg2, parametersCached[3]);
            return DynamicExpression.Make(validMethodForDynamic.GetReturnType(), delegateType, binder, arg0, arg1, arg2);
        }

        public static DynamicExpression MakeDynamic(System.Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
        {
            ContractUtils.RequiresNotNull(delegateType, "delegatType");
            ContractUtils.RequiresNotNull(binder, "binder");
            if (!delegateType.IsSubclassOf(typeof(Delegate)))
            {
                throw System.Linq.Expressions.Error.TypeMustBeDerivedFromSystemDelegate();
            }
            MethodInfo validMethodForDynamic = GetValidMethodForDynamic(delegateType);
            ParameterInfo[] parametersCached = validMethodForDynamic.GetParametersCached();
            ValidateArgumentCount(validMethodForDynamic, ExpressionType.Dynamic, 5, parametersCached);
            ValidateDynamicArgument(arg0);
            ValidateOneArgument(validMethodForDynamic, ExpressionType.Dynamic, arg0, parametersCached[1]);
            ValidateDynamicArgument(arg1);
            ValidateOneArgument(validMethodForDynamic, ExpressionType.Dynamic, arg1, parametersCached[2]);
            ValidateDynamicArgument(arg2);
            ValidateOneArgument(validMethodForDynamic, ExpressionType.Dynamic, arg2, parametersCached[3]);
            ValidateDynamicArgument(arg3);
            ValidateOneArgument(validMethodForDynamic, ExpressionType.Dynamic, arg3, parametersCached[4]);
            return DynamicExpression.Make(validMethodForDynamic.GetReturnType(), delegateType, binder, arg0, arg1, arg2, arg3);
        }

        public static GotoExpression MakeGoto(GotoExpressionKind kind, LabelTarget target, Expression value, System.Type type)
        {
            ValidateGoto(target, ref value, "target", "value");
            return new GotoExpression(kind, target, value, type);
        }

        public static IndexExpression MakeIndex(Expression instance, PropertyInfo indexer, IEnumerable<Expression> arguments)
        {
            if (indexer != null)
            {
                return Property(instance, indexer, arguments);
            }
            return ArrayAccess(instance, arguments);
        }

        public static MemberExpression MakeMemberAccess(Expression expression, MemberInfo member)
        {
            ContractUtils.RequiresNotNull(member, "member");
            FieldInfo field = member as FieldInfo;
            if (field != null)
            {
                return Field(expression, field);
            }
            PropertyInfo property = member as PropertyInfo;
            if (property == null)
            {
                throw System.Linq.Expressions.Error.MemberNotFieldOrProperty(member);
            }
            return Property(expression, property);
        }

        private static UnaryExpression MakeOpAssignUnary(ExpressionType kind, Expression expression, MethodInfo method)
        {
            UnaryExpression expression2;
            RequiresCanRead(expression, "expression");
            RequiresCanWrite(expression, "expression");
            if (method == null)
            {
                string str;
                if (TypeUtils.IsArithmetic(expression.Type))
                {
                    return new UnaryExpression(kind, expression, expression.Type, null);
                }
                if ((kind == ExpressionType.PreIncrementAssign) || (kind == ExpressionType.PostIncrementAssign))
                {
                    str = "op_Increment";
                }
                else
                {
                    str = "op_Decrement";
                }
                expression2 = GetUserDefinedUnaryOperatorOrThrow(kind, str, expression);
            }
            else
            {
                expression2 = GetMethodBasedUnaryOperator(kind, expression, method);
            }
            if (!TypeUtils.AreReferenceAssignable(expression.Type, expression2.Type))
            {
                throw System.Linq.Expressions.Error.UserDefinedOpMustHaveValidReturnType(kind, method.Name);
            }
            return expression2;
        }

        public static TryExpression MakeTry(System.Type type, Expression body, Expression @finally, Expression fault, IEnumerable<CatchBlock> handlers)
        {
            RequiresCanRead(body, "body");
            ReadOnlyCollection<CatchBlock> array = handlers.ToReadOnly<CatchBlock>();
            ContractUtils.RequiresNotNullItems<CatchBlock>(array, "handlers");
            ValidateTryAndCatchHaveSameType(type, body, array);
            if (fault != null)
            {
                if ((@finally != null) || (array.Count > 0))
                {
                    throw System.Linq.Expressions.Error.FaultCannotHaveCatchOrFinally();
                }
                RequiresCanRead(fault, "fault");
            }
            else if (@finally != null)
            {
                RequiresCanRead(@finally, "finally");
            }
            else if (array.Count == 0)
            {
                throw System.Linq.Expressions.Error.TryMustHaveCatchFinallyOrFault();
            }
            return new TryExpression(type ?? body.Type, body, @finally, fault, array);
        }

        public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, System.Type type)
        {
            return MakeUnary(unaryType, operand, type, null);
        }

        public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, System.Type type, MethodInfo method)
        {
            switch (unaryType)
            {
                case ExpressionType.Negate:
                    return Negate(operand, method);

                case ExpressionType.UnaryPlus:
                    return UnaryPlus(operand, method);

                case ExpressionType.NegateChecked:
                    return NegateChecked(operand, method);

                case ExpressionType.Not:
                    return Not(operand, method);

                case ExpressionType.Quote:
                    return Quote(operand);

                case ExpressionType.Convert:
                    return Convert(operand, type, method);

                case ExpressionType.ConvertChecked:
                    return ConvertChecked(operand, type, method);

                case ExpressionType.ArrayLength:
                    return ArrayLength(operand);

                case ExpressionType.TypeAs:
                    return TypeAs(operand, type);

                case ExpressionType.Decrement:
                    return Decrement(operand, method);

                case ExpressionType.Throw:
                    return Throw(operand, type);

                case ExpressionType.Unbox:
                    return Unbox(operand, type);

                case ExpressionType.Increment:
                    return Increment(operand, method);

                case ExpressionType.PreIncrementAssign:
                    return PreIncrementAssign(operand, method);

                case ExpressionType.PreDecrementAssign:
                    return PreDecrementAssign(operand, method);

                case ExpressionType.PostIncrementAssign:
                    return PostIncrementAssign(operand, method);

                case ExpressionType.PostDecrementAssign:
                    return PostDecrementAssign(operand, method);

                case ExpressionType.OnesComplement:
                    return OnesComplement(operand, method);

                case ExpressionType.IsTrue:
                    return IsTrue(operand, method);

                case ExpressionType.IsFalse:
                    return IsFalse(operand, method);
            }
            throw System.Linq.Expressions.Error.UnhandledUnary(unaryType);
        }

        public static MemberMemberBinding MemberBind(MemberInfo member, params MemberBinding[] bindings)
        {
            ContractUtils.RequiresNotNull(member, "member");
            ContractUtils.RequiresNotNull(bindings, "bindings");
            return MemberBind(member, (IEnumerable<MemberBinding>) bindings);
        }

        public static MemberMemberBinding MemberBind(MemberInfo member, IEnumerable<MemberBinding> bindings)
        {
            System.Type type;
            ContractUtils.RequiresNotNull(member, "member");
            ContractUtils.RequiresNotNull(bindings, "bindings");
            ReadOnlyCollection<MemberBinding> onlys = bindings.ToReadOnly<MemberBinding>();
            ValidateGettableFieldOrPropertyMember(member, out type);
            ValidateMemberInitArgs(type, onlys);
            return new MemberMemberBinding(member, onlys);
        }

        public static MemberMemberBinding MemberBind(MethodInfo propertyAccessor, params MemberBinding[] bindings)
        {
            ContractUtils.RequiresNotNull(propertyAccessor, "propertyAccessor");
            return MemberBind(GetProperty(propertyAccessor), bindings);
        }

        public static MemberMemberBinding MemberBind(MethodInfo propertyAccessor, IEnumerable<MemberBinding> bindings)
        {
            ContractUtils.RequiresNotNull(propertyAccessor, "propertyAccessor");
            return MemberBind(GetProperty(propertyAccessor), bindings);
        }

        public static MemberInitExpression MemberInit(NewExpression newExpression, params MemberBinding[] bindings)
        {
            return MemberInit(newExpression, (IEnumerable<MemberBinding>) bindings);
        }

        public static MemberInitExpression MemberInit(NewExpression newExpression, IEnumerable<MemberBinding> bindings)
        {
            ContractUtils.RequiresNotNull(newExpression, "newExpression");
            ContractUtils.RequiresNotNull(bindings, "bindings");
            ReadOnlyCollection<MemberBinding> onlys = bindings.ToReadOnly<MemberBinding>();
            ValidateMemberInitArgs(newExpression.Type, onlys);
            return new MemberInitExpression(newExpression, onlys);
        }

        public static BinaryExpression Modulo(Expression left, Expression right)
        {
            return Modulo(left, right, null);
        }

        public static BinaryExpression Modulo(Expression left, Expression right, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedBinaryOperator(ExpressionType.Modulo, left, right, method, true);
            }
            if ((left.Type == right.Type) && TypeUtils.IsArithmetic(left.Type))
            {
                return new SimpleBinaryExpression(ExpressionType.Modulo, left, right, left.Type);
            }
            return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Modulo, "op_Modulus", left, right, true);
        }

        public static BinaryExpression ModuloAssign(Expression left, Expression right)
        {
            return ModuloAssign(left, right, null, null);
        }

        public static BinaryExpression ModuloAssign(Expression left, Expression right, MethodInfo method)
        {
            return ModuloAssign(left, right, method, null);
        }

        public static BinaryExpression ModuloAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion)
        {
            RequiresCanRead(left, "left");
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedAssignOperator(ExpressionType.ModuloAssign, left, right, method, conversion, true);
            }
            if (!(left.Type == right.Type) || !TypeUtils.IsArithmetic(left.Type))
            {
                return GetUserDefinedAssignOperatorOrThrow(ExpressionType.ModuloAssign, "op_Modulus", left, right, conversion, true);
            }
            if (conversion != null)
            {
                throw System.Linq.Expressions.Error.ConversionIsNotSupportedForArithmeticTypes();
            }
            return new SimpleBinaryExpression(ExpressionType.ModuloAssign, left, right, left.Type);
        }

        public static BinaryExpression Multiply(Expression left, Expression right)
        {
            return Multiply(left, right, null);
        }

        public static BinaryExpression Multiply(Expression left, Expression right, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedBinaryOperator(ExpressionType.Multiply, left, right, method, true);
            }
            if ((left.Type == right.Type) && TypeUtils.IsArithmetic(left.Type))
            {
                return new SimpleBinaryExpression(ExpressionType.Multiply, left, right, left.Type);
            }
            return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Multiply, "op_Multiply", left, right, true);
        }

        public static BinaryExpression MultiplyAssign(Expression left, Expression right)
        {
            return MultiplyAssign(left, right, null, null);
        }

        public static BinaryExpression MultiplyAssign(Expression left, Expression right, MethodInfo method)
        {
            return MultiplyAssign(left, right, method, null);
        }

        public static BinaryExpression MultiplyAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion)
        {
            RequiresCanRead(left, "left");
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedAssignOperator(ExpressionType.MultiplyAssign, left, right, method, conversion, true);
            }
            if (!(left.Type == right.Type) || !TypeUtils.IsArithmetic(left.Type))
            {
                return GetUserDefinedAssignOperatorOrThrow(ExpressionType.MultiplyAssign, "op_Multiply", left, right, conversion, true);
            }
            if (conversion != null)
            {
                throw System.Linq.Expressions.Error.ConversionIsNotSupportedForArithmeticTypes();
            }
            return new SimpleBinaryExpression(ExpressionType.MultiplyAssign, left, right, left.Type);
        }

        public static BinaryExpression MultiplyAssignChecked(Expression left, Expression right)
        {
            return MultiplyAssignChecked(left, right, null);
        }

        public static BinaryExpression MultiplyAssignChecked(Expression left, Expression right, MethodInfo method)
        {
            return MultiplyAssignChecked(left, right, method, null);
        }

        public static BinaryExpression MultiplyAssignChecked(Expression left, Expression right, MethodInfo method, LambdaExpression conversion)
        {
            RequiresCanRead(left, "left");
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedAssignOperator(ExpressionType.MultiplyAssignChecked, left, right, method, conversion, true);
            }
            if (!(left.Type == right.Type) || !TypeUtils.IsArithmetic(left.Type))
            {
                return GetUserDefinedAssignOperatorOrThrow(ExpressionType.MultiplyAssignChecked, "op_Multiply", left, right, conversion, true);
            }
            if (conversion != null)
            {
                throw System.Linq.Expressions.Error.ConversionIsNotSupportedForArithmeticTypes();
            }
            return new SimpleBinaryExpression(ExpressionType.MultiplyAssignChecked, left, right, left.Type);
        }

        public static BinaryExpression MultiplyChecked(Expression left, Expression right)
        {
            return MultiplyChecked(left, right, null);
        }

        public static BinaryExpression MultiplyChecked(Expression left, Expression right, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedBinaryOperator(ExpressionType.MultiplyChecked, left, right, method, true);
            }
            if ((left.Type == right.Type) && TypeUtils.IsArithmetic(left.Type))
            {
                return new SimpleBinaryExpression(ExpressionType.MultiplyChecked, left, right, left.Type);
            }
            return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.MultiplyChecked, "op_Multiply", left, right, true);
        }

        public static UnaryExpression Negate(Expression expression)
        {
            return Negate(expression, null);
        }

        public static UnaryExpression Negate(Expression expression, MethodInfo method)
        {
            RequiresCanRead(expression, "expression");
            if (method != null)
            {
                return GetMethodBasedUnaryOperator(ExpressionType.Negate, expression, method);
            }
            if (TypeUtils.IsArithmetic(expression.Type) && !TypeUtils.IsUnsignedInt(expression.Type))
            {
                return new UnaryExpression(ExpressionType.Negate, expression, expression.Type, null);
            }
            return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Negate, "op_UnaryNegation", expression);
        }

        public static UnaryExpression NegateChecked(Expression expression)
        {
            return NegateChecked(expression, null);
        }

        public static UnaryExpression NegateChecked(Expression expression, MethodInfo method)
        {
            RequiresCanRead(expression, "expression");
            if (method != null)
            {
                return GetMethodBasedUnaryOperator(ExpressionType.NegateChecked, expression, method);
            }
            if (TypeUtils.IsArithmetic(expression.Type) && !TypeUtils.IsUnsignedInt(expression.Type))
            {
                return new UnaryExpression(ExpressionType.NegateChecked, expression, expression.Type, null);
            }
            return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.NegateChecked, "op_UnaryNegation", expression);
        }

        public static NewExpression New(ConstructorInfo constructor)
        {
            return New(constructor, (IEnumerable<Expression>) null);
        }

        public static NewExpression New(System.Type type)
        {
            ContractUtils.RequiresNotNull(type, "type");
            if (type == typeof(void))
            {
                throw System.Linq.Expressions.Error.ArgumentCannotBeOfTypeVoid();
            }
            ConstructorInfo constructor = null;
            if (type.IsValueType)
            {
                return new NewValueTypeExpression(type, EmptyReadOnlyCollection<Expression>.Instance, null);
            }
            constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, System.Type.EmptyTypes, null);
            if (constructor == null)
            {
                throw System.Linq.Expressions.Error.TypeMissingDefaultConstructor(type);
            }
            return New(constructor);
        }

        public static NewExpression New(ConstructorInfo constructor, params Expression[] arguments)
        {
            return New(constructor, (IEnumerable<Expression>) arguments);
        }

        public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments)
        {
            ContractUtils.RequiresNotNull(constructor, "constructor");
            ContractUtils.RequiresNotNull(constructor.DeclaringType, "constructor.DeclaringType");
            TypeUtils.ValidateType(constructor.DeclaringType);
            ReadOnlyCollection<Expression> onlys = arguments.ToReadOnly<Expression>();
            ValidateArgumentTypes(constructor, ExpressionType.New, ref onlys);
            return new NewExpression(constructor, onlys, null);
        }

        public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments, IEnumerable<MemberInfo> members)
        {
            ContractUtils.RequiresNotNull(constructor, "constructor");
            ReadOnlyCollection<MemberInfo> onlys = members.ToReadOnly<MemberInfo>();
            ReadOnlyCollection<Expression> onlys2 = arguments.ToReadOnly<Expression>();
            ValidateNewArgs(constructor, ref onlys2, ref onlys);
            return new NewExpression(constructor, onlys2, onlys);
        }

        public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments, params MemberInfo[] members)
        {
            return New(constructor, arguments, (IEnumerable<MemberInfo>) members);
        }

        public static NewArrayExpression NewArrayBounds(System.Type type, params Expression[] bounds)
        {
            return NewArrayBounds(type, (IEnumerable<Expression>) bounds);
        }

        public static NewArrayExpression NewArrayBounds(System.Type type, IEnumerable<Expression> bounds)
        {
            System.Type type2;
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(bounds, "bounds");
            if (type.Equals(typeof(void)))
            {
                throw System.Linq.Expressions.Error.ArgumentCannotBeOfTypeVoid();
            }
            ReadOnlyCollection<Expression> onlys = bounds.ToReadOnly<Expression>();
            int count = onlys.Count;
            if (count <= 0)
            {
                throw System.Linq.Expressions.Error.BoundsCannotBeLessThanOne();
            }
            for (int i = 0; i < count; i++)
            {
                Expression expression = onlys[i];
                RequiresCanRead(expression, "bounds");
                if (!TypeUtils.IsInteger(expression.Type))
                {
                    throw System.Linq.Expressions.Error.ArgumentMustBeInteger();
                }
            }
            if (count == 1)
            {
                type2 = type.MakeArrayType();
            }
            else
            {
                type2 = type.MakeArrayType(count);
            }
            return NewArrayExpression.Make(ExpressionType.NewArrayBounds, type2, bounds.ToReadOnly<Expression>());
        }

        public static NewArrayExpression NewArrayInit(System.Type type, params Expression[] initializers)
        {
            return NewArrayInit(type, (IEnumerable<Expression>) initializers);
        }

        public static NewArrayExpression NewArrayInit(System.Type type, IEnumerable<Expression> initializers)
        {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(initializers, "initializers");
            if (type.Equals(typeof(void)))
            {
                throw System.Linq.Expressions.Error.ArgumentCannotBeOfTypeVoid();
            }
            ReadOnlyCollection<Expression> expressions = initializers.ToReadOnly<Expression>();
            Expression[] list = null;
            int index = 0;
            int count = expressions.Count;
            while (index < count)
            {
                Expression expression = expressions[index];
                RequiresCanRead(expression, "initializers");
                if (!TypeUtils.AreReferenceAssignable(type, expression.Type))
                {
                    if (!TypeUtils.IsSameOrSubclass(typeof(LambdaExpression), type) || !type.IsAssignableFrom(expression.GetType()))
                    {
                        throw System.Linq.Expressions.Error.ExpressionTypeCannotInitializeArrayType(expression.Type, type);
                    }
                    expression = Quote(expression);
                    if (list == null)
                    {
                        list = new Expression[expressions.Count];
                        for (int i = 0; i < index; i++)
                        {
                            list[i] = expressions[i];
                        }
                    }
                }
                if (list != null)
                {
                    list[index] = expression;
                }
                index++;
            }
            if (list != null)
            {
                expressions = new TrueReadOnlyCollection<Expression>(list);
            }
            return NewArrayExpression.Make(ExpressionType.NewArrayInit, type.MakeArrayType(), expressions);
        }

        public static UnaryExpression Not(Expression expression)
        {
            return Not(expression, null);
        }

        public static UnaryExpression Not(Expression expression, MethodInfo method)
        {
            RequiresCanRead(expression, "expression");
            if (method != null)
            {
                return GetMethodBasedUnaryOperator(ExpressionType.Not, expression, method);
            }
            if (TypeUtils.IsIntegerOrBool(expression.Type))
            {
                return new UnaryExpression(ExpressionType.Not, expression, expression.Type, null);
            }
            UnaryExpression expression2 = GetUserDefinedUnaryOperator(ExpressionType.Not, "op_LogicalNot", expression);
            if (expression2 != null)
            {
                return expression2;
            }
            return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Not, "op_OnesComplement", expression);
        }

        public static BinaryExpression NotEqual(Expression left, Expression right)
        {
            return NotEqual(left, right, false, null);
        }

        public static BinaryExpression NotEqual(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null)
            {
                return GetEqualityComparisonOperator(ExpressionType.NotEqual, "op_Inequality", left, right, liftToNull);
            }
            return GetMethodBasedBinaryOperator(ExpressionType.NotEqual, left, right, method, liftToNull);
        }

        public static UnaryExpression OnesComplement(Expression expression)
        {
            return OnesComplement(expression, null);
        }

        public static UnaryExpression OnesComplement(Expression expression, MethodInfo method)
        {
            RequiresCanRead(expression, "expression");
            if (method != null)
            {
                return GetMethodBasedUnaryOperator(ExpressionType.OnesComplement, expression, method);
            }
            if (TypeUtils.IsInteger(expression.Type))
            {
                return new UnaryExpression(ExpressionType.OnesComplement, expression, expression.Type, null);
            }
            return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.OnesComplement, "op_OnesComplement", expression);
        }

        public static BinaryExpression Or(Expression left, Expression right)
        {
            return Or(left, right, null);
        }

        public static BinaryExpression Or(Expression left, Expression right, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedBinaryOperator(ExpressionType.Or, left, right, method, true);
            }
            if ((left.Type == right.Type) && TypeUtils.IsIntegerOrBool(left.Type))
            {
                return new SimpleBinaryExpression(ExpressionType.Or, left, right, left.Type);
            }
            return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Or, "op_BitwiseOr", left, right, true);
        }

        public static BinaryExpression OrAssign(Expression left, Expression right)
        {
            return OrAssign(left, right, null, null);
        }

        public static BinaryExpression OrAssign(Expression left, Expression right, MethodInfo method)
        {
            return OrAssign(left, right, method, null);
        }

        public static BinaryExpression OrAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion)
        {
            RequiresCanRead(left, "left");
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedAssignOperator(ExpressionType.OrAssign, left, right, method, conversion, true);
            }
            if (!(left.Type == right.Type) || !TypeUtils.IsIntegerOrBool(left.Type))
            {
                return GetUserDefinedAssignOperatorOrThrow(ExpressionType.OrAssign, "op_BitwiseOr", left, right, conversion, true);
            }
            if (conversion != null)
            {
                throw System.Linq.Expressions.Error.ConversionIsNotSupportedForArithmeticTypes();
            }
            return new SimpleBinaryExpression(ExpressionType.OrAssign, left, right, left.Type);
        }

        public static BinaryExpression OrElse(Expression left, Expression right)
        {
            return OrElse(left, right, null);
        }

        public static BinaryExpression OrElse(Expression left, Expression right, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null)
            {
                if (left.Type == right.Type)
                {
                    if (left.Type == typeof(bool))
                    {
                        return new LogicalBinaryExpression(ExpressionType.OrElse, left, right);
                    }
                    if (left.Type == typeof(bool?))
                    {
                        return new SimpleBinaryExpression(ExpressionType.OrElse, left, right, left.Type);
                    }
                }
                method = GetUserDefinedBinaryOperator(ExpressionType.OrElse, left.Type, right.Type, "op_BitwiseOr");
                if (method == null)
                {
                    throw System.Linq.Expressions.Error.BinaryOperatorNotDefined(ExpressionType.OrElse, left.Type, right.Type);
                }
                ValidateUserDefinedConditionalLogicOperator(ExpressionType.OrElse, left.Type, right.Type, method);
                return new MethodBinaryExpression(ExpressionType.OrElse, left, right, (left.Type.IsNullableType() && (method.ReturnType == left.Type.GetNonNullableType())) ? left.Type : method.ReturnType, method);
            }
            ValidateUserDefinedConditionalLogicOperator(ExpressionType.OrElse, left.Type, right.Type, method);
            return new MethodBinaryExpression(ExpressionType.OrElse, left, right, (left.Type.IsNullableType() && (method.ReturnType == left.Type.GetNonNullableType())) ? left.Type : method.ReturnType, method);
        }

        public static ParameterExpression Parameter(System.Type type)
        {
            return Parameter(type, null);
        }

        public static ParameterExpression Parameter(System.Type type, string name)
        {
            ContractUtils.RequiresNotNull(type, "type");
            if (type == typeof(void))
            {
                throw System.Linq.Expressions.Error.ArgumentCannotBeOfTypeVoid();
            }
            bool isByRef = type.IsByRef;
            if (isByRef)
            {
                type = type.GetElementType();
            }
            return ParameterExpression.Make(type, name, isByRef);
        }

        internal static bool ParameterIsAssignable(ParameterInfo pi, System.Type argType)
        {
            System.Type parameterType = pi.ParameterType;
            if (parameterType.IsByRef)
            {
                parameterType = parameterType.GetElementType();
            }
            return TypeUtils.AreReferenceAssignable(parameterType, argType);
        }

        public static UnaryExpression PostDecrementAssign(Expression expression)
        {
            return MakeOpAssignUnary(ExpressionType.PostDecrementAssign, expression, null);
        }

        public static UnaryExpression PostDecrementAssign(Expression expression, MethodInfo method)
        {
            return MakeOpAssignUnary(ExpressionType.PostDecrementAssign, expression, method);
        }

        public static UnaryExpression PostIncrementAssign(Expression expression)
        {
            return MakeOpAssignUnary(ExpressionType.PostIncrementAssign, expression, null);
        }

        public static UnaryExpression PostIncrementAssign(Expression expression, MethodInfo method)
        {
            return MakeOpAssignUnary(ExpressionType.PostIncrementAssign, expression, method);
        }

        public static BinaryExpression Power(Expression left, Expression right)
        {
            return Power(left, right, null);
        }

        public static BinaryExpression Power(Expression left, Expression right, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method == null)
            {
                method = typeof(Math).GetMethod("Pow", BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                {
                    throw System.Linq.Expressions.Error.BinaryOperatorNotDefined(ExpressionType.Power, left.Type, right.Type);
                }
            }
            return GetMethodBasedBinaryOperator(ExpressionType.Power, left, right, method, true);
        }

        public static BinaryExpression PowerAssign(Expression left, Expression right)
        {
            return PowerAssign(left, right, null, null);
        }

        public static BinaryExpression PowerAssign(Expression left, Expression right, MethodInfo method)
        {
            return PowerAssign(left, right, method, null);
        }

        public static BinaryExpression PowerAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion)
        {
            RequiresCanRead(left, "left");
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            if (method == null)
            {
                method = typeof(Math).GetMethod("Pow", BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                {
                    throw System.Linq.Expressions.Error.BinaryOperatorNotDefined(ExpressionType.PowerAssign, left.Type, right.Type);
                }
            }
            return GetMethodBasedAssignOperator(ExpressionType.PowerAssign, left, right, method, conversion, true);
        }

        public static UnaryExpression PreDecrementAssign(Expression expression)
        {
            return MakeOpAssignUnary(ExpressionType.PreDecrementAssign, expression, null);
        }

        public static UnaryExpression PreDecrementAssign(Expression expression, MethodInfo method)
        {
            return MakeOpAssignUnary(ExpressionType.PreDecrementAssign, expression, method);
        }

        public static UnaryExpression PreIncrementAssign(Expression expression)
        {
            return MakeOpAssignUnary(ExpressionType.PreIncrementAssign, expression, null);
        }

        public static UnaryExpression PreIncrementAssign(Expression expression, MethodInfo method)
        {
            return MakeOpAssignUnary(ExpressionType.PreIncrementAssign, expression, method);
        }

        public static MemberExpression Property(Expression expression, MethodInfo propertyAccessor)
        {
            ContractUtils.RequiresNotNull(propertyAccessor, "propertyAccessor");
            ValidateMethodInfo(propertyAccessor);
            return Property(expression, GetProperty(propertyAccessor));
        }

        public static MemberExpression Property(Expression expression, PropertyInfo property)
        {
            ContractUtils.RequiresNotNull(property, "property");
            MethodInfo info = property.GetGetMethod(true) ?? property.GetSetMethod(true);
            if (info == null)
            {
                throw System.Linq.Expressions.Error.PropertyDoesNotHaveAccessor(property);
            }
            if (info.IsStatic)
            {
                if (expression != null)
                {
                    throw new ArgumentException(System.Linq.Expressions.Strings.OnlyStaticPropertiesHaveNullInstance, "expression");
                }
            }
            else
            {
                if (expression == null)
                {
                    throw new ArgumentException(System.Linq.Expressions.Strings.OnlyStaticPropertiesHaveNullInstance, "property");
                }
                RequiresCanRead(expression, "expression");
                if (!TypeUtils.IsValidInstanceType(property, expression.Type))
                {
                    throw System.Linq.Expressions.Error.PropertyNotDefinedForType(property, expression.Type);
                }
            }
            return MemberExpression.Make(expression, property);
        }

        public static MemberExpression Property(Expression expression, string propertyName)
        {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(propertyName, "propertyName");
            PropertyInfo property = expression.Type.GetProperty(propertyName, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property == null)
            {
                property = expression.Type.GetProperty(propertyName, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            }
            if (property == null)
            {
                throw System.Linq.Expressions.Error.InstancePropertyNotDefinedForType(propertyName, expression.Type);
            }
            return Property(expression, property);
        }

        public static IndexExpression Property(Expression instance, PropertyInfo indexer, params Expression[] arguments)
        {
            return Property(instance, indexer, (IEnumerable<Expression>) arguments);
        }

        public static IndexExpression Property(Expression instance, PropertyInfo indexer, IEnumerable<Expression> arguments)
        {
            ReadOnlyCollection<Expression> argList = arguments.ToReadOnly<Expression>();
            ValidateIndexedProperty(instance, indexer, ref argList);
            return new IndexExpression(instance, indexer, argList);
        }

        public static IndexExpression Property(Expression instance, string propertyName, params Expression[] arguments)
        {
            RequiresCanRead(instance, "instance");
            ContractUtils.RequiresNotNull(propertyName, "indexerName");
            PropertyInfo indexer = FindInstanceProperty(instance.Type, propertyName, arguments);
            return Property(instance, indexer, arguments);
        }

        public static MemberExpression Property(Expression expression, System.Type type, string propertyName)
        {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(propertyName, "propertyName");
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property == null)
            {
                property = type.GetProperty(propertyName, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
            }
            if (property == null)
            {
                throw System.Linq.Expressions.Error.PropertyNotDefinedForType(propertyName, type);
            }
            return Property(expression, property);
        }

        public static MemberExpression PropertyOrField(Expression expression, string propertyOrFieldName)
        {
            RequiresCanRead(expression, "expression");
            PropertyInfo property = expression.Type.GetProperty(propertyOrFieldName, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null)
            {
                return Property(expression, property);
            }
            FieldInfo field = expression.Type.GetField(propertyOrFieldName, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field == null)
            {
                property = expression.Type.GetProperty(propertyOrFieldName, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    return Property(expression, property);
                }
                field = expression.Type.GetField(propertyOrFieldName, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (field == null)
                {
                    throw System.Linq.Expressions.Error.NotAMemberOfType(propertyOrFieldName, expression.Type);
                }
            }
            return Field(expression, field);
        }

        public static UnaryExpression Quote(Expression expression)
        {
            RequiresCanRead(expression, "expression");
            if (!(expression is LambdaExpression))
            {
                throw System.Linq.Expressions.Error.QuotedExpressionMustBeLambda();
            }
            return new UnaryExpression(ExpressionType.Quote, expression, expression.GetType(), null);
        }

        public virtual Expression Reduce()
        {
            if (this.CanReduce)
            {
                throw System.Linq.Expressions.Error.ReducibleMustOverrideReduce();
            }
            return this;
        }

        public Expression ReduceAndCheck()
        {
            if (!this.CanReduce)
            {
                throw System.Linq.Expressions.Error.MustBeReducible();
            }
            Expression expression = this.Reduce();
            if ((expression == null) || (expression == this))
            {
                throw System.Linq.Expressions.Error.MustReduceToDifferent();
            }
            if (!TypeUtils.AreReferenceAssignable(this.Type, expression.Type))
            {
                throw System.Linq.Expressions.Error.ReducedNotCompatible();
            }
            return expression;
        }

        public Expression ReduceExtensions()
        {
            Expression expression = this;
            while (expression.NodeType == ExpressionType.Extension)
            {
                expression = expression.ReduceAndCheck();
            }
            return expression;
        }

        public static BinaryExpression ReferenceEqual(Expression left, Expression right)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (!TypeUtils.HasReferenceEquality(left.Type, right.Type))
            {
                throw System.Linq.Expressions.Error.ReferenceEqualityNotDefined(left.Type, right.Type);
            }
            return new LogicalBinaryExpression(ExpressionType.Equal, left, right);
        }

        public static BinaryExpression ReferenceNotEqual(Expression left, Expression right)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (!TypeUtils.HasReferenceEquality(left.Type, right.Type))
            {
                throw System.Linq.Expressions.Error.ReferenceEqualityNotDefined(left.Type, right.Type);
            }
            return new LogicalBinaryExpression(ExpressionType.NotEqual, left, right);
        }

        private static void RequiresCanRead(Expression expression, string paramName)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(paramName);
            }
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                {
                    MemberExpression expression3 = (MemberExpression) expression;
                    MemberInfo member = expression3.Member;
                    if (member.MemberType == MemberTypes.Property)
                    {
                        PropertyInfo info2 = (PropertyInfo) member;
                        if (!info2.CanRead)
                        {
                            throw new ArgumentException(System.Linq.Expressions.Strings.ExpressionMustBeReadable, paramName);
                        }
                    }
                    break;
                }
                case ExpressionType.Index:
                {
                    IndexExpression expression2 = (IndexExpression) expression;
                    if ((expression2.Indexer != null) && !expression2.Indexer.CanRead)
                    {
                        throw new ArgumentException(System.Linq.Expressions.Strings.ExpressionMustBeReadable, paramName);
                    }
                    break;
                }
            }
        }

        private static void RequiresCanRead(IEnumerable<Expression> items, string paramName)
        {
            if (items != null)
            {
                IList<Expression> list = items as IList<Expression>;
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        RequiresCanRead(list[i], paramName);
                    }
                }
                else
                {
                    foreach (Expression expression in items)
                    {
                        RequiresCanRead(expression, paramName);
                    }
                }
            }
        }

        private static void RequiresCanWrite(Expression expression, string paramName)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(paramName);
            }
            bool canWrite = false;
            ExpressionType nodeType = expression.NodeType;
            switch (nodeType)
            {
                case ExpressionType.MemberAccess:
                {
                    MemberExpression expression3 = (MemberExpression) expression;
                    switch (expression3.Member.MemberType)
                    {
                        case MemberTypes.Field:
                        {
                            FieldInfo member = (FieldInfo) expression3.Member;
                            canWrite = !member.IsInitOnly && !member.IsLiteral;
                            break;
                        }
                        case MemberTypes.Property:
                        {
                            PropertyInfo info = (PropertyInfo) expression3.Member;
                            canWrite = info.CanWrite;
                            break;
                        }
                    }
                    goto Label_00AF;
                }
                case ExpressionType.Parameter:
                    canWrite = true;
                    goto Label_00AF;
            }
            if (nodeType == ExpressionType.Index)
            {
                IndexExpression expression2 = (IndexExpression) expression;
                if (expression2.Indexer != null)
                {
                    canWrite = expression2.Indexer.CanWrite;
                }
                else
                {
                    canWrite = true;
                }
            }
        Label_00AF:
            if (!canWrite)
            {
                throw new ArgumentException(System.Linq.Expressions.Strings.ExpressionMustBeWriteable, paramName);
            }
        }

        public static UnaryExpression Rethrow()
        {
            return Throw(null);
        }

        public static UnaryExpression Rethrow(System.Type type)
        {
            return Throw(null, type);
        }

        public static GotoExpression Return(LabelTarget target)
        {
            return MakeGoto(GotoExpressionKind.Return, target, null, typeof(void));
        }

        public static GotoExpression Return(LabelTarget target, Expression value)
        {
            return MakeGoto(GotoExpressionKind.Return, target, value, typeof(void));
        }

        public static GotoExpression Return(LabelTarget target, System.Type type)
        {
            return MakeGoto(GotoExpressionKind.Return, target, null, type);
        }

        public static GotoExpression Return(LabelTarget target, Expression value, System.Type type)
        {
            return MakeGoto(GotoExpressionKind.Return, target, value, type);
        }

        internal static T ReturnObject<T>(object collectionOrT) where T: class
        {
            T local = collectionOrT as T;
            if (local != null)
            {
                return local;
            }
            return ((ReadOnlyCollection<T>) collectionOrT)[0];
        }

        internal static ReadOnlyCollection<T> ReturnReadOnly<T>(ref IList<T> collection)
        {
            IList<T> comparand = collection;
            ReadOnlyCollection<T> onlys = comparand as ReadOnlyCollection<T>;
            if (onlys != null)
            {
                return onlys;
            }
            Interlocked.CompareExchange<IList<T>>(ref collection, comparand.ToReadOnly<T>(), comparand);
            return (ReadOnlyCollection<T>) collection;
        }

        internal static ReadOnlyCollection<Expression> ReturnReadOnly(IArgumentProvider provider, ref object collection)
        {
            Expression comparand = collection as Expression;
            if (comparand != null)
            {
                Interlocked.CompareExchange(ref collection, new ReadOnlyCollection<Expression>(new ListArgumentProvider(provider, comparand)), comparand);
            }
            return (ReadOnlyCollection<Expression>) collection;
        }

        public static BinaryExpression RightShift(Expression left, Expression right)
        {
            return RightShift(left, right, null);
        }

        public static BinaryExpression RightShift(Expression left, Expression right, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedBinaryOperator(ExpressionType.RightShift, left, right, method, true);
            }
            if (IsSimpleShift(left.Type, right.Type))
            {
                return new SimpleBinaryExpression(ExpressionType.RightShift, left, right, GetResultTypeOfShift(left.Type, right.Type));
            }
            return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.RightShift, "op_RightShift", left, right, true);
        }

        public static BinaryExpression RightShiftAssign(Expression left, Expression right)
        {
            return RightShiftAssign(left, right, null, null);
        }

        public static BinaryExpression RightShiftAssign(Expression left, Expression right, MethodInfo method)
        {
            return RightShiftAssign(left, right, method, null);
        }

        public static BinaryExpression RightShiftAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion)
        {
            RequiresCanRead(left, "left");
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedAssignOperator(ExpressionType.RightShiftAssign, left, right, method, conversion, true);
            }
            if (!IsSimpleShift(left.Type, right.Type))
            {
                return GetUserDefinedAssignOperatorOrThrow(ExpressionType.RightShiftAssign, "op_RightShift", left, right, conversion, true);
            }
            if (conversion != null)
            {
                throw System.Linq.Expressions.Error.ConversionIsNotSupportedForArithmeticTypes();
            }
            return new SimpleBinaryExpression(ExpressionType.RightShiftAssign, left, right, GetResultTypeOfShift(left.Type, right.Type));
        }

        public static RuntimeVariablesExpression RuntimeVariables(params ParameterExpression[] variables)
        {
            return RuntimeVariables((IEnumerable<ParameterExpression>) variables);
        }

        public static RuntimeVariablesExpression RuntimeVariables(IEnumerable<ParameterExpression> variables)
        {
            ContractUtils.RequiresNotNull(variables, "variables");
            ReadOnlyCollection<ParameterExpression> onlys = variables.ToReadOnly<ParameterExpression>();
            for (int i = 0; i < onlys.Count; i++)
            {
                if (onlys[i] == null)
                {
                    throw new ArgumentNullException("variables[" + i + "]");
                }
            }
            return new RuntimeVariablesExpression(onlys);
        }

        public static BinaryExpression Subtract(Expression left, Expression right)
        {
            return Subtract(left, right, null);
        }

        public static BinaryExpression Subtract(Expression left, Expression right, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedBinaryOperator(ExpressionType.Subtract, left, right, method, true);
            }
            if ((left.Type == right.Type) && TypeUtils.IsArithmetic(left.Type))
            {
                return new SimpleBinaryExpression(ExpressionType.Subtract, left, right, left.Type);
            }
            return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Subtract, "op_Subtraction", left, right, true);
        }

        public static BinaryExpression SubtractAssign(Expression left, Expression right)
        {
            return SubtractAssign(left, right, null, null);
        }

        public static BinaryExpression SubtractAssign(Expression left, Expression right, MethodInfo method)
        {
            return SubtractAssign(left, right, method, null);
        }

        public static BinaryExpression SubtractAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion)
        {
            RequiresCanRead(left, "left");
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedAssignOperator(ExpressionType.SubtractAssign, left, right, method, conversion, true);
            }
            if (!(left.Type == right.Type) || !TypeUtils.IsArithmetic(left.Type))
            {
                return GetUserDefinedAssignOperatorOrThrow(ExpressionType.SubtractAssign, "op_Subtraction", left, right, conversion, true);
            }
            if (conversion != null)
            {
                throw System.Linq.Expressions.Error.ConversionIsNotSupportedForArithmeticTypes();
            }
            return new SimpleBinaryExpression(ExpressionType.SubtractAssign, left, right, left.Type);
        }

        public static BinaryExpression SubtractAssignChecked(Expression left, Expression right)
        {
            return SubtractAssignChecked(left, right, null);
        }

        public static BinaryExpression SubtractAssignChecked(Expression left, Expression right, MethodInfo method)
        {
            return SubtractAssignChecked(left, right, method, null);
        }

        public static BinaryExpression SubtractAssignChecked(Expression left, Expression right, MethodInfo method, LambdaExpression conversion)
        {
            RequiresCanRead(left, "left");
            RequiresCanWrite(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedAssignOperator(ExpressionType.SubtractAssignChecked, left, right, method, conversion, true);
            }
            if (!(left.Type == right.Type) || !TypeUtils.IsArithmetic(left.Type))
            {
                return GetUserDefinedAssignOperatorOrThrow(ExpressionType.SubtractAssignChecked, "op_Subtraction", left, right, conversion, true);
            }
            if (conversion != null)
            {
                throw System.Linq.Expressions.Error.ConversionIsNotSupportedForArithmeticTypes();
            }
            return new SimpleBinaryExpression(ExpressionType.SubtractAssignChecked, left, right, left.Type);
        }

        public static BinaryExpression SubtractChecked(Expression left, Expression right)
        {
            return SubtractChecked(left, right, null);
        }

        public static BinaryExpression SubtractChecked(Expression left, Expression right, MethodInfo method)
        {
            RequiresCanRead(left, "left");
            RequiresCanRead(right, "right");
            if (method != null)
            {
                return GetMethodBasedBinaryOperator(ExpressionType.SubtractChecked, left, right, method, true);
            }
            if ((left.Type == right.Type) && TypeUtils.IsArithmetic(left.Type))
            {
                return new SimpleBinaryExpression(ExpressionType.SubtractChecked, left, right, left.Type);
            }
            return GetUserDefinedBinaryOperatorOrThrow(ExpressionType.SubtractChecked, "op_Subtraction", left, right, true);
        }

        public static SwitchExpression Switch(Expression switchValue, params System.Linq.Expressions.SwitchCase[] cases)
        {
            return Switch(switchValue, null, null, (IEnumerable<System.Linq.Expressions.SwitchCase>) cases);
        }

        public static SwitchExpression Switch(Expression switchValue, Expression defaultBody, params System.Linq.Expressions.SwitchCase[] cases)
        {
            return Switch(switchValue, defaultBody, null, (IEnumerable<System.Linq.Expressions.SwitchCase>) cases);
        }

        public static SwitchExpression Switch(Expression switchValue, Expression defaultBody, MethodInfo comparison, params System.Linq.Expressions.SwitchCase[] cases)
        {
            return Switch(switchValue, defaultBody, comparison, (IEnumerable<System.Linq.Expressions.SwitchCase>) cases);
        }

        public static SwitchExpression Switch(Expression switchValue, Expression defaultBody, MethodInfo comparison, IEnumerable<System.Linq.Expressions.SwitchCase> cases)
        {
            return Switch(null, switchValue, defaultBody, comparison, cases);
        }

        public static SwitchExpression Switch(System.Type type, Expression switchValue, Expression defaultBody, MethodInfo comparison, params System.Linq.Expressions.SwitchCase[] cases)
        {
            return Switch(type, switchValue, defaultBody, comparison, (IEnumerable<System.Linq.Expressions.SwitchCase>) cases);
        }

        public static SwitchExpression Switch(System.Type type, Expression switchValue, Expression defaultBody, MethodInfo comparison, IEnumerable<System.Linq.Expressions.SwitchCase> cases)
        {
            RequiresCanRead(switchValue, "switchValue");
            if (switchValue.Type == typeof(void))
            {
                throw System.Linq.Expressions.Error.ArgumentCannotBeOfTypeVoid();
            }
            ReadOnlyCollection<System.Linq.Expressions.SwitchCase> collection = cases.ToReadOnly<System.Linq.Expressions.SwitchCase>();
            ContractUtils.RequiresNotEmpty<System.Linq.Expressions.SwitchCase>(collection, "cases");
            ContractUtils.RequiresNotNullItems<System.Linq.Expressions.SwitchCase>(collection, "cases");
            System.Type resultType = type ?? collection[0].Body.Type;
            bool customType = type != null;
            if (comparison != null)
            {
                ParameterInfo[] parametersCached = comparison.GetParametersCached();
                if (parametersCached.Length != 2)
                {
                    throw System.Linq.Expressions.Error.IncorrectNumberOfMethodCallArguments(comparison);
                }
                ParameterInfo pi = parametersCached[0];
                bool flag2 = false;
                if (!ParameterIsAssignable(pi, switchValue.Type))
                {
                    flag2 = ParameterIsAssignable(pi, switchValue.Type.GetNonNullableType());
                    if (!flag2)
                    {
                        throw System.Linq.Expressions.Error.SwitchValueTypeDoesNotMatchComparisonMethodParameter(switchValue.Type, pi.ParameterType);
                    }
                }
                ParameterInfo info2 = parametersCached[1];
                foreach (System.Linq.Expressions.SwitchCase @case in collection)
                {
                    ContractUtils.RequiresNotNull(@case, "cases");
                    ValidateSwitchCaseType(@case.Body, customType, resultType, "cases");
                    for (int i = 0; i < @case.TestValues.Count; i++)
                    {
                        System.Type nonNullableType = @case.TestValues[i].Type;
                        if (flag2)
                        {
                            if (!nonNullableType.IsNullableType())
                            {
                                throw System.Linq.Expressions.Error.TestValueTypeDoesNotMatchComparisonMethodParameter(nonNullableType, info2.ParameterType);
                            }
                            nonNullableType = nonNullableType.GetNonNullableType();
                        }
                        if (!ParameterIsAssignable(info2, nonNullableType))
                        {
                            throw System.Linq.Expressions.Error.TestValueTypeDoesNotMatchComparisonMethodParameter(nonNullableType, info2.ParameterType);
                        }
                    }
                }
            }
            else
            {
                Expression right = collection[0].TestValues[0];
                foreach (System.Linq.Expressions.SwitchCase case2 in collection)
                {
                    ContractUtils.RequiresNotNull(case2, "cases");
                    ValidateSwitchCaseType(case2.Body, customType, resultType, "cases");
                    for (int j = 0; j < case2.TestValues.Count; j++)
                    {
                        if (!TypeUtils.AreEquivalent(right.Type, case2.TestValues[j].Type))
                        {
                            throw new ArgumentException(System.Linq.Expressions.Strings.AllTestValuesMustHaveSameType, "cases");
                        }
                    }
                }
                comparison = Equal(switchValue, right, false, comparison).Method;
            }
            if (defaultBody == null)
            {
                if (resultType != typeof(void))
                {
                    throw System.Linq.Expressions.Error.DefaultBodyMustBeSupplied();
                }
            }
            else
            {
                ValidateSwitchCaseType(defaultBody, customType, resultType, "defaultBody");
            }
            if ((comparison != null) && (comparison.ReturnType != typeof(bool)))
            {
                throw System.Linq.Expressions.Error.EqualityMustReturnBoolean(comparison);
            }
            return new SwitchExpression(resultType, switchValue, defaultBody, comparison, collection);
        }

        public static System.Linq.Expressions.SwitchCase SwitchCase(Expression body, params Expression[] testValues)
        {
            return SwitchCase(body, (IEnumerable<Expression>) testValues);
        }

        public static System.Linq.Expressions.SwitchCase SwitchCase(Expression body, IEnumerable<Expression> testValues)
        {
            RequiresCanRead(body, "body");
            ReadOnlyCollection<Expression> items = testValues.ToReadOnly<Expression>();
            RequiresCanRead(items, "testValues");
            ContractUtils.RequiresNotEmpty<Expression>(items, "testValues");
            return new System.Linq.Expressions.SwitchCase(body, items);
        }

        public static SymbolDocumentInfo SymbolDocument(string fileName)
        {
            return new SymbolDocumentInfo(fileName);
        }

        public static SymbolDocumentInfo SymbolDocument(string fileName, Guid language)
        {
            return new SymbolDocumentWithGuids(fileName, ref language);
        }

        public static SymbolDocumentInfo SymbolDocument(string fileName, Guid language, Guid languageVendor)
        {
            return new SymbolDocumentWithGuids(fileName, ref language, ref languageVendor);
        }

        public static SymbolDocumentInfo SymbolDocument(string fileName, Guid language, Guid languageVendor, Guid documentType)
        {
            return new SymbolDocumentWithGuids(fileName, ref language, ref languageVendor, ref documentType);
        }

        public static UnaryExpression Throw(Expression value)
        {
            return Throw(value, typeof(void));
        }

        public static UnaryExpression Throw(Expression value, System.Type type)
        {
            ContractUtils.RequiresNotNull(type, "type");
            TypeUtils.ValidateType(type);
            if (value != null)
            {
                RequiresCanRead(value, "value");
                if (value.Type.IsValueType)
                {
                    throw System.Linq.Expressions.Error.ArgumentMustNotHaveValueType();
                }
            }
            return new UnaryExpression(ExpressionType.Throw, value, type, null);
        }

        public override string ToString()
        {
            return ExpressionStringBuilder.ExpressionToString(this);
        }

        public static TryExpression TryCatch(Expression body, params CatchBlock[] handlers)
        {
            return MakeTry(null, body, null, null, handlers);
        }

        public static TryExpression TryCatchFinally(Expression body, Expression @finally, params CatchBlock[] handlers)
        {
            return MakeTry(null, body, @finally, null, handlers);
        }

        public static TryExpression TryFault(Expression body, Expression fault)
        {
            return MakeTry(null, body, null, fault, null);
        }

        public static TryExpression TryFinally(Expression body, Expression @finally)
        {
            return MakeTry(null, body, @finally, null, null);
        }

        public static bool TryGetActionType(System.Type[] typeArgs, out System.Type actionType)
        {
            if (ValidateTryGetFuncActionArgs(typeArgs))
            {
                System.Type type;
                actionType = type = DelegateHelpers.GetActionType(typeArgs);
                return (type != null);
            }
            actionType = null;
            return false;
        }

        public static bool TryGetFuncType(System.Type[] typeArgs, out System.Type funcType)
        {
            if (ValidateTryGetFuncActionArgs(typeArgs))
            {
                System.Type type;
                funcType = type = DelegateHelpers.GetFuncType(typeArgs);
                return (type != null);
            }
            funcType = null;
            return false;
        }

        public static UnaryExpression TypeAs(Expression expression, System.Type type)
        {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            TypeUtils.ValidateType(type);
            if (type.IsValueType && !type.IsNullableType())
            {
                throw System.Linq.Expressions.Error.IncorrectTypeForTypeAs(type);
            }
            return new UnaryExpression(ExpressionType.TypeAs, expression, type, null);
        }

        public static TypeBinaryExpression TypeEqual(Expression expression, System.Type type)
        {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            if (type.IsByRef)
            {
                throw System.Linq.Expressions.Error.TypeMustNotBeByRef();
            }
            return new TypeBinaryExpression(expression, type, ExpressionType.TypeEqual);
        }

        public static TypeBinaryExpression TypeIs(Expression expression, System.Type type)
        {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            if (type.IsByRef)
            {
                throw System.Linq.Expressions.Error.TypeMustNotBeByRef();
            }
            return new TypeBinaryExpression(expression, type, ExpressionType.TypeIs);
        }

        public static UnaryExpression UnaryPlus(Expression expression)
        {
            return UnaryPlus(expression, null);
        }

        public static UnaryExpression UnaryPlus(Expression expression, MethodInfo method)
        {
            RequiresCanRead(expression, "expression");
            if (method != null)
            {
                return GetMethodBasedUnaryOperator(ExpressionType.UnaryPlus, expression, method);
            }
            if (TypeUtils.IsArithmetic(expression.Type))
            {
                return new UnaryExpression(ExpressionType.UnaryPlus, expression, expression.Type, null);
            }
            return GetUserDefinedUnaryOperatorOrThrow(ExpressionType.UnaryPlus, "op_UnaryPlus", expression);
        }

        public static UnaryExpression Unbox(Expression expression, System.Type type)
        {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            if (!expression.Type.IsInterface && (expression.Type != typeof(object)))
            {
                throw System.Linq.Expressions.Error.InvalidUnboxType();
            }
            if (!type.IsValueType)
            {
                throw System.Linq.Expressions.Error.InvalidUnboxType();
            }
            TypeUtils.ValidateType(type);
            return new UnaryExpression(ExpressionType.Unbox, expression, type, null);
        }

        private static void ValidateAccessor(Expression instance, MethodInfo method, ParameterInfo[] indexes, ref ReadOnlyCollection<Expression> arguments)
        {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ValidateMethodInfo(method);
            if ((method.CallingConvention & CallingConventions.VarArgs) != 0)
            {
                throw System.Linq.Expressions.Error.AccessorsCannotHaveVarArgs();
            }
            if (method.IsStatic)
            {
                if (instance != null)
                {
                    throw System.Linq.Expressions.Error.OnlyStaticMethodsHaveNullInstance();
                }
            }
            else
            {
                if (instance == null)
                {
                    throw System.Linq.Expressions.Error.OnlyStaticMethodsHaveNullInstance();
                }
                RequiresCanRead(instance, "instance");
                ValidateCallInstanceType(instance.Type, method);
            }
            ValidateAccessorArgumentTypes(method, indexes, ref arguments);
        }

        private static void ValidateAccessorArgumentTypes(MethodInfo method, ParameterInfo[] indexes, ref ReadOnlyCollection<Expression> arguments)
        {
            if (indexes.Length > 0)
            {
                if (indexes.Length != arguments.Count)
                {
                    throw System.Linq.Expressions.Error.IncorrectNumberOfMethodCallArguments(method);
                }
                Expression[] list = null;
                int index = 0;
                int length = indexes.Length;
                while (index < length)
                {
                    Expression expression = arguments[index];
                    ParameterInfo info = indexes[index];
                    RequiresCanRead(expression, "arguments");
                    System.Type parameterType = info.ParameterType;
                    if (parameterType.IsByRef)
                    {
                        throw System.Linq.Expressions.Error.AccessorsCannotHaveByRefArgs();
                    }
                    TypeUtils.ValidateType(parameterType);
                    if (!TypeUtils.AreReferenceAssignable(parameterType, expression.Type))
                    {
                        if (!TypeUtils.IsSameOrSubclass(typeof(LambdaExpression), parameterType) || !parameterType.IsAssignableFrom(expression.GetType()))
                        {
                            throw System.Linq.Expressions.Error.ExpressionTypeDoesNotMatchMethodParameter(expression.Type, parameterType, method);
                        }
                        expression = Quote(expression);
                    }
                    if ((list == null) && (expression != arguments[index]))
                    {
                        list = new Expression[arguments.Count];
                        for (int i = 0; i < index; i++)
                        {
                            list[i] = arguments[i];
                        }
                    }
                    if (list != null)
                    {
                        list[index] = expression;
                    }
                    index++;
                }
                if (list != null)
                {
                    arguments = new TrueReadOnlyCollection<Expression>(list);
                }
            }
            else if (arguments.Count > 0)
            {
                throw System.Linq.Expressions.Error.IncorrectNumberOfMethodCallArguments(method);
            }
        }

        private static void ValidateAnonymousTypeMember(ref MemberInfo member, out System.Type memberType)
        {
            MemberTypes types = member.MemberType;
            if (types != MemberTypes.Field)
            {
                if (types != MemberTypes.Method)
                {
                    if (types != MemberTypes.Property)
                    {
                        throw System.Linq.Expressions.Error.ArgumentMustBeFieldInfoOrPropertInfoOrMethod();
                    }
                    PropertyInfo info2 = member as PropertyInfo;
                    if (!info2.CanRead)
                    {
                        throw System.Linq.Expressions.Error.PropertyDoesNotHaveGetter(info2);
                    }
                    if (info2.GetGetMethod().IsStatic)
                    {
                        throw System.Linq.Expressions.Error.ArgumentMustBeInstanceMember();
                    }
                    memberType = info2.PropertyType;
                }
                else
                {
                    MethodInfo mi = member as MethodInfo;
                    if (mi.IsStatic)
                    {
                        throw System.Linq.Expressions.Error.ArgumentMustBeInstanceMember();
                    }
                    PropertyInfo property = GetProperty(mi);
                    member = property;
                    memberType = property.PropertyType;
                }
            }
            else
            {
                FieldInfo info = member as FieldInfo;
                if (info.IsStatic)
                {
                    throw System.Linq.Expressions.Error.ArgumentMustBeInstanceMember();
                }
                memberType = info.FieldType;
            }
        }

        private static void ValidateArgumentCount(MethodBase method, ExpressionType nodeKind, int count, ParameterInfo[] pis)
        {
            if (pis.Length == count)
            {
                return;
            }
            ExpressionType type = nodeKind;
            if (type <= ExpressionType.Invoke)
            {
                switch (type)
                {
                    case ExpressionType.Call:
                        goto Label_0030;

                    case ExpressionType.Invoke:
                        throw System.Linq.Expressions.Error.IncorrectNumberOfLambdaArguments();
                }
                goto Label_0037;
            }
            if (type != ExpressionType.New)
            {
                if (type == ExpressionType.Dynamic)
                {
                    goto Label_0030;
                }
                goto Label_0037;
            }
            throw System.Linq.Expressions.Error.IncorrectNumberOfConstructorArguments();
        Label_0030:
            throw System.Linq.Expressions.Error.IncorrectNumberOfMethodCallArguments(method);
        Label_0037:
            throw ContractUtils.Unreachable;
        }

        private static void ValidateArgumentTypes(MethodBase method, ExpressionType nodeKind, ref ReadOnlyCollection<Expression> arguments)
        {
            ParameterInfo[] parametersForValidation = GetParametersForValidation(method, nodeKind);
            ValidateArgumentCount(method, nodeKind, arguments.Count, parametersForValidation);
            Expression[] list = null;
            int index = 0;
            int length = parametersForValidation.Length;
            while (index < length)
            {
                Expression arg = arguments[index];
                ParameterInfo pi = parametersForValidation[index];
                arg = ValidateOneArgument(method, nodeKind, arg, pi);
                if ((list == null) && (arg != arguments[index]))
                {
                    list = new Expression[arguments.Count];
                    for (int i = 0; i < index; i++)
                    {
                        list[i] = arguments[i];
                    }
                }
                if (list != null)
                {
                    list[index] = arg;
                }
                index++;
            }
            if (list != null)
            {
                arguments = new TrueReadOnlyCollection<Expression>(list);
            }
        }

        private static void ValidateCallInstanceType(System.Type instanceType, MethodInfo method)
        {
            if (!TypeUtils.IsValidInstanceType(method, instanceType))
            {
                throw System.Linq.Expressions.Error.InstanceAndMethodTypeMismatch(method, method.DeclaringType, instanceType);
            }
        }

        private static System.Type ValidateCoalesceArgTypes(System.Type left, System.Type right)
        {
            System.Type nonNullableType = left.GetNonNullableType();
            if (left.IsValueType && !left.IsNullableType())
            {
                throw System.Linq.Expressions.Error.CoalesceUsedOnNonNullType();
            }
            if (left.IsNullableType() && TypeUtils.IsImplicitlyConvertible(right, nonNullableType))
            {
                return nonNullableType;
            }
            if (TypeUtils.IsImplicitlyConvertible(right, left))
            {
                return left;
            }
            if (!TypeUtils.IsImplicitlyConvertible(nonNullableType, right))
            {
                throw System.Linq.Expressions.Error.ArgumentTypesMustMatch();
            }
            return right;
        }

        private static void ValidateDynamicArgument(Expression arg)
        {
            RequiresCanRead(arg, "arguments");
            System.Type type = arg.Type;
            ContractUtils.RequiresNotNull(type, "type");
            TypeUtils.ValidateType(type);
            if (type == typeof(void))
            {
                throw System.Linq.Expressions.Error.ArgumentTypeCannotBeVoid();
            }
        }

        private static void ValidateElementInitAddMethodInfo(MethodInfo addMethod)
        {
            ValidateMethodInfo(addMethod);
            ParameterInfo[] parametersCached = addMethod.GetParametersCached();
            if (parametersCached.Length == 0)
            {
                throw System.Linq.Expressions.Error.ElementInitializerMethodWithZeroArgs();
            }
            if (!addMethod.Name.Equals("Add", StringComparison.OrdinalIgnoreCase))
            {
                throw System.Linq.Expressions.Error.ElementInitializerMethodNotAdd();
            }
            if (addMethod.IsStatic)
            {
                throw System.Linq.Expressions.Error.ElementInitializerMethodStatic();
            }
            foreach (ParameterInfo info in parametersCached)
            {
                if (info.ParameterType.IsByRef)
                {
                    throw System.Linq.Expressions.Error.ElementInitializerMethodNoRefOutParam(info.Name, addMethod.Name);
                }
            }
        }

        private static void ValidateGettableFieldOrPropertyMember(MemberInfo member, out System.Type memberType)
        {
            FieldInfo info = member as FieldInfo;
            if (info == null)
            {
                PropertyInfo info2 = member as PropertyInfo;
                if (info2 == null)
                {
                    throw System.Linq.Expressions.Error.ArgumentMustBeFieldInfoOrPropertInfo();
                }
                if (!info2.CanRead)
                {
                    throw System.Linq.Expressions.Error.PropertyDoesNotHaveGetter(info2);
                }
                memberType = info2.PropertyType;
            }
            else
            {
                memberType = info.FieldType;
            }
        }

        private static void ValidateGoto(LabelTarget target, ref Expression value, string targetParameter, string valueParameter)
        {
            ContractUtils.RequiresNotNull(target, targetParameter);
            if (value == null)
            {
                if (target.Type != typeof(void))
                {
                    throw System.Linq.Expressions.Error.LabelMustBeVoidOrHaveExpression();
                }
            }
            else
            {
                ValidateGotoType(target.Type, ref value, valueParameter);
            }
        }

        private static void ValidateGotoType(System.Type expectedType, ref Expression value, string paramName)
        {
            RequiresCanRead(value, paramName);
            if ((expectedType != typeof(void)) && !TypeUtils.AreReferenceAssignable(expectedType, value.Type))
            {
                if (TypeUtils.IsSameOrSubclass(typeof(LambdaExpression), expectedType) && expectedType.IsAssignableFrom(value.GetType()))
                {
                    value = Quote(value);
                }
                throw System.Linq.Expressions.Error.ExpressionTypeDoesNotMatchLabel(value.Type, expectedType);
            }
        }

        private static void ValidateIndexedProperty(Expression instance, PropertyInfo property, ref ReadOnlyCollection<Expression> argList)
        {
            ContractUtils.RequiresNotNull(property, "property");
            if (property.PropertyType.IsByRef)
            {
                throw System.Linq.Expressions.Error.PropertyCannotHaveRefType();
            }
            if (property.PropertyType == typeof(void))
            {
                throw System.Linq.Expressions.Error.PropertyTypeCannotBeVoid();
            }
            ParameterInfo[] indexes = null;
            MethodInfo getMethod = property.GetGetMethod(true);
            if (getMethod != null)
            {
                indexes = getMethod.GetParametersCached();
                ValidateAccessor(instance, getMethod, indexes, ref argList);
            }
            MethodInfo setMethod = property.GetSetMethod(true);
            if (setMethod != null)
            {
                ParameterInfo[] parametersCached = setMethod.GetParametersCached();
                if (parametersCached.Length == 0)
                {
                    throw System.Linq.Expressions.Error.SetterHasNoParams();
                }
                System.Type parameterType = parametersCached[parametersCached.Length - 1].ParameterType;
                if (parameterType.IsByRef)
                {
                    throw System.Linq.Expressions.Error.PropertyCannotHaveRefType();
                }
                if (setMethod.ReturnType != typeof(void))
                {
                    throw System.Linq.Expressions.Error.SetterMustBeVoid();
                }
                if (property.PropertyType != parameterType)
                {
                    throw System.Linq.Expressions.Error.PropertyTyepMustMatchSetter();
                }
                if (getMethod != null)
                {
                    if (getMethod.IsStatic ^ setMethod.IsStatic)
                    {
                        throw System.Linq.Expressions.Error.BothAccessorsMustBeStatic();
                    }
                    if (indexes.Length != (parametersCached.Length - 1))
                    {
                        throw System.Linq.Expressions.Error.IndexesOfSetGetMustMatch();
                    }
                    for (int i = 0; i < indexes.Length; i++)
                    {
                        if (indexes[i].ParameterType != parametersCached[i].ParameterType)
                        {
                            throw System.Linq.Expressions.Error.IndexesOfSetGetMustMatch();
                        }
                    }
                }
                else
                {
                    ValidateAccessor(instance, setMethod, parametersCached.RemoveLast<ParameterInfo>(), ref argList);
                }
            }
            if ((getMethod == null) && (setMethod == null))
            {
                throw System.Linq.Expressions.Error.PropertyDoesNotHaveAccessor(property);
            }
        }

        private static void ValidateLambdaArgs(System.Type delegateType, ref Expression body, ReadOnlyCollection<ParameterExpression> parameters)
        {
            MethodInfo method;
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            RequiresCanRead(body, "body");
            if (!typeof(Delegate).IsAssignableFrom(delegateType) || (delegateType == typeof(Delegate)))
            {
                throw System.Linq.Expressions.Error.LambdaTypeMustBeDerivedFromSystemDelegate();
            }
            lock (_LambdaDelegateCache)
            {
                if (!_LambdaDelegateCache.TryGetValue(delegateType, out method))
                {
                    method = delegateType.GetMethod("Invoke");
                    if (delegateType.CanCache())
                    {
                        _LambdaDelegateCache[delegateType] = method;
                    }
                }
            }
            ParameterInfo[] parametersCached = method.GetParametersCached();
            if (parametersCached.Length > 0)
            {
                if (parametersCached.Length != parameters.Count)
                {
                    throw System.Linq.Expressions.Error.IncorrectNumberOfLambdaDeclarationParameters();
                }
                System.Linq.Expressions.Set<ParameterExpression> set = new System.Linq.Expressions.Set<ParameterExpression>(parametersCached.Length);
                int index = 0;
                int length = parametersCached.Length;
                while (index < length)
                {
                    ParameterExpression expression = parameters[index];
                    ParameterInfo info2 = parametersCached[index];
                    RequiresCanRead(expression, "parameters");
                    System.Type parameterType = info2.ParameterType;
                    if (expression.IsByRef)
                    {
                        if (!parameterType.IsByRef)
                        {
                            throw System.Linq.Expressions.Error.ParameterExpressionNotValidAsDelegate(expression.Type.MakeByRefType(), parameterType);
                        }
                        parameterType = parameterType.GetElementType();
                    }
                    if (!TypeUtils.AreReferenceAssignable(expression.Type, parameterType))
                    {
                        throw System.Linq.Expressions.Error.ParameterExpressionNotValidAsDelegate(expression.Type, parameterType);
                    }
                    if (set.Contains(expression))
                    {
                        throw System.Linq.Expressions.Error.DuplicateVariable(expression);
                    }
                    set.Add(expression);
                    index++;
                }
            }
            else if (parameters.Count > 0)
            {
                throw System.Linq.Expressions.Error.IncorrectNumberOfLambdaDeclarationParameters();
            }
            if ((method.ReturnType != typeof(void)) && !TypeUtils.AreReferenceAssignable(method.ReturnType, body.Type))
            {
                if (!TypeUtils.IsSameOrSubclass(typeof(LambdaExpression), method.ReturnType) || !method.ReturnType.IsAssignableFrom(body.GetType()))
                {
                    throw System.Linq.Expressions.Error.ExpressionTypeDoesNotMatchReturn(body.Type, method.ReturnType);
                }
                body = Quote(body);
            }
        }

        private static void ValidateListInitArgs(System.Type listType, ReadOnlyCollection<System.Linq.Expressions.ElementInit> initializers)
        {
            if (!typeof(IEnumerable).IsAssignableFrom(listType))
            {
                throw System.Linq.Expressions.Error.TypeNotIEnumerable(listType);
            }
            int num = 0;
            int count = initializers.Count;
            while (num < count)
            {
                System.Linq.Expressions.ElementInit init = initializers[num];
                ContractUtils.RequiresNotNull(init, "initializers");
                ValidateCallInstanceType(listType, init.AddMethod);
                num++;
            }
        }

        private static void ValidateMemberInitArgs(System.Type type, ReadOnlyCollection<MemberBinding> bindings)
        {
            int num = 0;
            int count = bindings.Count;
            while (num < count)
            {
                MemberBinding binding = bindings[num];
                ContractUtils.RequiresNotNull(binding, "bindings");
                if (!binding.Member.DeclaringType.IsAssignableFrom(type))
                {
                    throw System.Linq.Expressions.Error.NotAMemberOfType(binding.Member.Name, type);
                }
                num++;
            }
        }

        private static ParameterInfo[] ValidateMethodAndGetParameters(Expression instance, MethodInfo method)
        {
            ValidateMethodInfo(method);
            ValidateStaticOrInstanceMethod(instance, method);
            return GetParametersForValidation(method, ExpressionType.Call);
        }

        private static void ValidateMethodInfo(MethodInfo method)
        {
            if (method.IsGenericMethodDefinition)
            {
                throw System.Linq.Expressions.Error.MethodIsGeneric(method);
            }
            if (method.ContainsGenericParameters)
            {
                throw System.Linq.Expressions.Error.MethodContainsGenericParameters(method);
            }
        }

        private static void ValidateNewArgs(ConstructorInfo constructor, ref ReadOnlyCollection<Expression> arguments, ref ReadOnlyCollection<MemberInfo> members)
        {
            ParameterInfo[] infoArray;
            if ((infoArray = constructor.GetParametersCached()).Length > 0)
            {
                if (arguments.Count != infoArray.Length)
                {
                    throw System.Linq.Expressions.Error.IncorrectNumberOfConstructorArguments();
                }
                if (arguments.Count != members.Count)
                {
                    throw System.Linq.Expressions.Error.IncorrectNumberOfArgumentsForMembers();
                }
                Expression[] list = null;
                MemberInfo[] infoArray2 = null;
                int index = 0;
                int count = arguments.Count;
                while (index < count)
                {
                    System.Type type;
                    Expression expression = arguments[index];
                    RequiresCanRead(expression, "argument");
                    MemberInfo info = members[index];
                    ContractUtils.RequiresNotNull(info, "member");
                    if (!TypeUtils.AreEquivalent(info.DeclaringType, constructor.DeclaringType))
                    {
                        throw System.Linq.Expressions.Error.ArgumentMemberNotDeclOnType(info.Name, constructor.DeclaringType.Name);
                    }
                    ValidateAnonymousTypeMember(ref info, out type);
                    if (!TypeUtils.AreReferenceAssignable(type, expression.Type))
                    {
                        if (!TypeUtils.IsSameOrSubclass(typeof(LambdaExpression), type) || !type.IsAssignableFrom(expression.GetType()))
                        {
                            throw System.Linq.Expressions.Error.ArgumentTypeDoesNotMatchMember(expression.Type, type);
                        }
                        expression = Quote(expression);
                    }
                    ParameterInfo info2 = infoArray[index];
                    System.Type parameterType = info2.ParameterType;
                    if (parameterType.IsByRef)
                    {
                        parameterType = parameterType.GetElementType();
                    }
                    if (!TypeUtils.AreReferenceAssignable(parameterType, expression.Type))
                    {
                        if (!TypeUtils.IsSameOrSubclass(typeof(LambdaExpression), parameterType) || !parameterType.IsAssignableFrom(expression.Type))
                        {
                            throw System.Linq.Expressions.Error.ExpressionTypeDoesNotMatchConstructorParameter(expression.Type, parameterType);
                        }
                        expression = Quote(expression);
                    }
                    if ((list == null) && (expression != arguments[index]))
                    {
                        list = new Expression[arguments.Count];
                        for (int i = 0; i < index; i++)
                        {
                            list[i] = arguments[i];
                        }
                    }
                    if (list != null)
                    {
                        list[index] = expression;
                    }
                    if ((infoArray2 == null) && (info != members[index]))
                    {
                        infoArray2 = new MemberInfo[members.Count];
                        for (int j = 0; j < index; j++)
                        {
                            infoArray2[j] = members[j];
                        }
                    }
                    if (infoArray2 != null)
                    {
                        infoArray2[index] = info;
                    }
                    index++;
                }
                if (list != null)
                {
                    arguments = new TrueReadOnlyCollection<Expression>(list);
                }
                if (infoArray2 != null)
                {
                    members = new TrueReadOnlyCollection<MemberInfo>(infoArray2);
                }
            }
            else
            {
                if ((arguments != null) && (arguments.Count > 0))
                {
                    throw System.Linq.Expressions.Error.IncorrectNumberOfConstructorArguments();
                }
                if ((members != null) && (members.Count > 0))
                {
                    throw System.Linq.Expressions.Error.IncorrectNumberOfMembersForGivenConstructor();
                }
            }
        }

        private static Expression ValidateOneArgument(MethodBase method, ExpressionType nodeKind, Expression arg, ParameterInfo pi)
        {
            RequiresCanRead(arg, "arguments");
            System.Type parameterType = pi.ParameterType;
            if (parameterType.IsByRef)
            {
                parameterType = parameterType.GetElementType();
            }
            TypeUtils.ValidateType(parameterType);
            if (TypeUtils.AreReferenceAssignable(parameterType, arg.Type))
            {
                return arg;
            }
            if (TypeUtils.IsSameOrSubclass(typeof(LambdaExpression), parameterType) && parameterType.IsAssignableFrom(arg.GetType()))
            {
                arg = Quote(arg);
                return arg;
            }
            ExpressionType type2 = nodeKind;
            if (type2 <= ExpressionType.Invoke)
            {
                switch (type2)
                {
                    case ExpressionType.Call:
                        goto Label_0097;

                    case ExpressionType.Invoke:
                        throw System.Linq.Expressions.Error.ExpressionTypeDoesNotMatchParameter(arg.Type, parameterType);
                }
                goto Label_00A5;
            }
            if (type2 != ExpressionType.New)
            {
                if (type2 == ExpressionType.Dynamic)
                {
                    goto Label_0097;
                }
                goto Label_00A5;
            }
            throw System.Linq.Expressions.Error.ExpressionTypeDoesNotMatchConstructorParameter(arg.Type, parameterType);
        Label_0097:
            throw System.Linq.Expressions.Error.ExpressionTypeDoesNotMatchMethodParameter(arg.Type, parameterType, method);
        Label_00A5:
            throw ContractUtils.Unreachable;
        }

        private static void ValidateOpAssignConversionLambda(LambdaExpression conversion, Expression left, MethodInfo method, ExpressionType nodeType)
        {
            MethodInfo info = conversion.Type.GetMethod("Invoke");
            ParameterInfo[] parametersCached = info.GetParametersCached();
            if (parametersCached.Length != 1)
            {
                throw System.Linq.Expressions.Error.IncorrectNumberOfMethodCallArguments(conversion);
            }
            if (!TypeUtils.AreEquivalent(info.ReturnType, left.Type))
            {
                throw System.Linq.Expressions.Error.OperandTypesDoNotMatchParameters(nodeType, conversion.ToString());
            }
            if ((method != null) && !TypeUtils.AreEquivalent(parametersCached[0].ParameterType, method.ReturnType))
            {
                throw System.Linq.Expressions.Error.OverloadOperatorTypeDoesNotMatchConversionType(nodeType, conversion.ToString());
            }
        }

        private static void ValidateOperator(MethodInfo method)
        {
            ValidateMethodInfo(method);
            if (!method.IsStatic)
            {
                throw System.Linq.Expressions.Error.UserDefinedOperatorMustBeStatic(method);
            }
            if (method.ReturnType == typeof(void))
            {
                throw System.Linq.Expressions.Error.UserDefinedOperatorMustNotBeVoid(method);
            }
        }

        private static void ValidateParamswithOperandsOrThrow(System.Type paramType, System.Type operandType, ExpressionType exprType, string name)
        {
            if (paramType.IsNullableType() && !operandType.IsNullableType())
            {
                throw System.Linq.Expressions.Error.OperandTypesDoNotMatchParameters(exprType, name);
            }
        }

        private static void ValidateSettableFieldOrPropertyMember(MemberInfo member, out System.Type memberType)
        {
            FieldInfo info = member as FieldInfo;
            if (info == null)
            {
                PropertyInfo info2 = member as PropertyInfo;
                if (info2 == null)
                {
                    throw System.Linq.Expressions.Error.ArgumentMustBeFieldInfoOrPropertInfo();
                }
                if (!info2.CanWrite)
                {
                    throw System.Linq.Expressions.Error.PropertyDoesNotHaveSetter(info2);
                }
                memberType = info2.PropertyType;
            }
            else
            {
                memberType = info.FieldType;
            }
        }

        private static void ValidateSpan(int startLine, int startColumn, int endLine, int endColumn)
        {
            if (startLine < 1)
            {
                throw System.Linq.Expressions.Error.OutOfRange("startLine", 1);
            }
            if (startColumn < 1)
            {
                throw System.Linq.Expressions.Error.OutOfRange("startColumn", 1);
            }
            if (endLine < 1)
            {
                throw System.Linq.Expressions.Error.OutOfRange("endLine", 1);
            }
            if (endColumn < 1)
            {
                throw System.Linq.Expressions.Error.OutOfRange("endColumn", 1);
            }
            if (startLine > endLine)
            {
                throw System.Linq.Expressions.Error.StartEndMustBeOrdered();
            }
            if ((startLine == endLine) && (startColumn > endColumn))
            {
                throw System.Linq.Expressions.Error.StartEndMustBeOrdered();
            }
        }

        private static void ValidateStaticOrInstanceMethod(Expression instance, MethodInfo method)
        {
            if (method.IsStatic)
            {
                if (instance != null)
                {
                    throw new ArgumentException(System.Linq.Expressions.Strings.OnlyStaticMethodsHaveNullInstance, "instance");
                }
            }
            else
            {
                if (instance == null)
                {
                    throw new ArgumentException(System.Linq.Expressions.Strings.OnlyStaticMethodsHaveNullInstance, "method");
                }
                RequiresCanRead(instance, "instance");
                ValidateCallInstanceType(instance.Type, method);
            }
        }

        private static void ValidateSwitchCaseType(Expression @case, bool customType, System.Type resultType, string parameterName)
        {
            if (customType)
            {
                if ((resultType != typeof(void)) && !TypeUtils.AreReferenceAssignable(resultType, @case.Type))
                {
                    throw new ArgumentException(System.Linq.Expressions.Strings.ArgumentTypesMustMatch, parameterName);
                }
            }
            else if (!TypeUtils.AreEquivalent(resultType, @case.Type))
            {
                throw new ArgumentException(System.Linq.Expressions.Strings.AllCaseBodiesMustHaveSameType, parameterName);
            }
        }

        private static void ValidateTryAndCatchHaveSameType(System.Type type, Expression tryBody, ReadOnlyCollection<CatchBlock> handlers)
        {
            if (type != null)
            {
                if (type != typeof(void))
                {
                    if (!TypeUtils.AreReferenceAssignable(type, tryBody.Type))
                    {
                        throw System.Linq.Expressions.Error.ArgumentTypesMustMatch();
                    }
                    foreach (CatchBlock block in handlers)
                    {
                        if (!TypeUtils.AreReferenceAssignable(type, block.Body.Type))
                        {
                            throw System.Linq.Expressions.Error.ArgumentTypesMustMatch();
                        }
                    }
                }
            }
            else if ((tryBody == null) || (tryBody.Type == typeof(void)))
            {
                foreach (CatchBlock block2 in handlers)
                {
                    if ((block2.Body != null) && (block2.Body.Type != typeof(void)))
                    {
                        throw System.Linq.Expressions.Error.BodyOfCatchMustHaveSameTypeAsBodyOfTry();
                    }
                }
            }
            else
            {
                type = tryBody.Type;
                foreach (CatchBlock block3 in handlers)
                {
                    if ((block3.Body == null) || !TypeUtils.AreEquivalent(block3.Body.Type, type))
                    {
                        throw System.Linq.Expressions.Error.BodyOfCatchMustHaveSameTypeAsBodyOfTry();
                    }
                }
            }
        }

        private static bool ValidateTryGetFuncActionArgs(System.Type[] typeArgs)
        {
            if (typeArgs == null)
            {
                throw new ArgumentNullException("typeArgs");
            }
            int index = 0;
            int length = typeArgs.Length;
            while (index < length)
            {
                System.Type type = typeArgs[index];
                if (type == null)
                {
                    throw new ArgumentNullException("typeArgs");
                }
                if (type.IsByRef)
                {
                    return false;
                }
                index++;
            }
            return true;
        }

        private static void ValidateUserDefinedConditionalLogicOperator(ExpressionType nodeType, System.Type left, System.Type right, MethodInfo method)
        {
            ValidateOperator(method);
            ParameterInfo[] parametersCached = method.GetParametersCached();
            if (parametersCached.Length != 2)
            {
                throw System.Linq.Expressions.Error.IncorrectNumberOfMethodCallArguments(method);
            }
            if (!ParameterIsAssignable(parametersCached[0], left) && (!left.IsNullableType() || !ParameterIsAssignable(parametersCached[0], left.GetNonNullableType())))
            {
                throw System.Linq.Expressions.Error.OperandTypesDoNotMatchParameters(nodeType, method.Name);
            }
            if (!ParameterIsAssignable(parametersCached[1], right) && (!right.IsNullableType() || !ParameterIsAssignable(parametersCached[1], right.GetNonNullableType())))
            {
                throw System.Linq.Expressions.Error.OperandTypesDoNotMatchParameters(nodeType, method.Name);
            }
            if (parametersCached[0].ParameterType != parametersCached[1].ParameterType)
            {
                throw System.Linq.Expressions.Error.UserDefinedOpMustHaveConsistentTypes(nodeType, method.Name);
            }
            if (method.ReturnType != parametersCached[0].ParameterType)
            {
                throw System.Linq.Expressions.Error.UserDefinedOpMustHaveConsistentTypes(nodeType, method.Name);
            }
            if (IsValidLiftedConditionalLogicalOperator(left, right, parametersCached))
            {
                left = left.GetNonNullableType();
                right = left.GetNonNullableType();
            }
            MethodInfo booleanOperator = TypeUtils.GetBooleanOperator(method.DeclaringType, "op_True");
            MethodInfo opTrue = TypeUtils.GetBooleanOperator(method.DeclaringType, "op_False");
            if (((booleanOperator == null) || (booleanOperator.ReturnType != typeof(bool))) || ((opTrue == null) || (opTrue.ReturnType != typeof(bool))))
            {
                throw System.Linq.Expressions.Error.LogicalOperatorMustHaveBooleanOperators(nodeType, method.Name);
            }
            VerifyOpTrueFalse(nodeType, left, opTrue);
            VerifyOpTrueFalse(nodeType, left, booleanOperator);
        }

        internal static void ValidateVariables(ReadOnlyCollection<ParameterExpression> varList, string collectionName)
        {
            if (varList.Count != 0)
            {
                int count = varList.Count;
                System.Linq.Expressions.Set<ParameterExpression> set = new System.Linq.Expressions.Set<ParameterExpression>(count);
                for (int i = 0; i < count; i++)
                {
                    ParameterExpression expression = varList[i];
                    if (expression == null)
                    {
                        throw new ArgumentNullException(string.Format(CultureInfo.CurrentCulture, "{0}[{1}]", new object[] { collectionName, set.Count }));
                    }
                    if (expression.IsByRef)
                    {
                        throw System.Linq.Expressions.Error.VariableMustNotBeByRef(expression, expression.Type);
                    }
                    if (set.Contains(expression))
                    {
                        throw System.Linq.Expressions.Error.DuplicateVariable(expression);
                    }
                    set.Add(expression);
                }
            }
        }

        public static ParameterExpression Variable(System.Type type)
        {
            return Variable(type, null);
        }

        public static ParameterExpression Variable(System.Type type, string name)
        {
            ContractUtils.RequiresNotNull(type, "type");
            if (type == typeof(void))
            {
                throw System.Linq.Expressions.Error.ArgumentCannotBeOfTypeVoid();
            }
            if (type.IsByRef)
            {
                throw System.Linq.Expressions.Error.TypeMustNotBeByRef();
            }
            return ParameterExpression.Make(type, name, false);
        }

        private static void VerifyOpTrueFalse(ExpressionType nodeType, System.Type left, MethodInfo opTrue)
        {
            ParameterInfo[] parametersCached = opTrue.GetParametersCached();
            if (parametersCached.Length != 1)
            {
                throw System.Linq.Expressions.Error.IncorrectNumberOfMethodCallArguments(opTrue);
            }
            if (!ParameterIsAssignable(parametersCached[0], left) && (!left.IsNullableType() || !ParameterIsAssignable(parametersCached[0], left.GetNonNullableType())))
            {
                throw System.Linq.Expressions.Error.OperandTypesDoNotMatchParameters(nodeType, opTrue.Name);
            }
        }

        protected internal virtual Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (!this.CanReduce)
            {
                throw System.Linq.Expressions.Error.MustBeReducible();
            }
            return visitor.Visit(this.ReduceAndCheck());
        }

        public virtual bool CanReduce
        {
            get
            {
                return false;
            }
        }

        private string DebugView
        {
            get
            {
                using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
                {
                    DebugViewWriter.WriteTo(this, writer);
                    return writer.ToString();
                }
            }
        }

        public virtual ExpressionType NodeType
        {
            get
            {
                ExtensionInfo info;
                if ((_legacyCtorSupportTable == null) || !_legacyCtorSupportTable.TryGetValue(this, out info))
                {
                    throw System.Linq.Expressions.Error.ExtensionNodeMustOverrideProperty("Expression.NodeType");
                }
                return info.NodeType;
            }
        }

        public virtual System.Type Type
        {
            get
            {
                ExtensionInfo info;
                if ((_legacyCtorSupportTable == null) || !_legacyCtorSupportTable.TryGetValue(this, out info))
                {
                    throw System.Linq.Expressions.Error.ExtensionNodeMustOverrideProperty("Expression.Type");
                }
                return info.Type;
            }
        }

        internal class BinaryExpressionProxy
        {
            private readonly BinaryExpression _node;

            public BinaryExpressionProxy(BinaryExpression node)
            {
                this._node = node;
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public LambdaExpression Conversion
            {
                get
                {
                    return this._node.Conversion;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public bool IsLifted
            {
                get
                {
                    return this._node.IsLifted;
                }
            }

            public bool IsLiftedToNull
            {
                get
                {
                    return this._node.IsLiftedToNull;
                }
            }

            public Expression Left
            {
                get
                {
                    return this._node.Left;
                }
            }

            public MethodInfo Method
            {
                get
                {
                    return this._node.Method;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public Expression Right
            {
                get
                {
                    return this._node.Right;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class BlockExpressionProxy
        {
            private readonly BlockExpression _node;

            public BlockExpressionProxy(BlockExpression node)
            {
                this._node = node;
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public ReadOnlyCollection<Expression> Expressions
            {
                get
                {
                    return this._node.Expressions;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public Expression Result
            {
                get
                {
                    return this._node.Result;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }

            public ReadOnlyCollection<ParameterExpression> Variables
            {
                get
                {
                    return this._node.Variables;
                }
            }
        }

        internal class CatchBlockProxy
        {
            private readonly CatchBlock _node;

            public CatchBlockProxy(CatchBlock node)
            {
                this._node = node;
            }

            public Expression Body
            {
                get
                {
                    return this._node.Body;
                }
            }

            public Expression Filter
            {
                get
                {
                    return this._node.Filter;
                }
            }

            public Type Test
            {
                get
                {
                    return this._node.Test;
                }
            }

            public ParameterExpression Variable
            {
                get
                {
                    return this._node.Variable;
                }
            }
        }

        internal class ConditionalExpressionProxy
        {
            private readonly ConditionalExpression _node;

            public ConditionalExpressionProxy(ConditionalExpression node)
            {
                this._node = node;
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public Expression IfFalse
            {
                get
                {
                    return this._node.IfFalse;
                }
            }

            public Expression IfTrue
            {
                get
                {
                    return this._node.IfTrue;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public Expression Test
            {
                get
                {
                    return this._node.Test;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class ConstantExpressionProxy
        {
            private readonly ConstantExpression _node;

            public ConstantExpressionProxy(ConstantExpression node)
            {
                this._node = node;
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }

            public object Value
            {
                get
                {
                    return this._node.Value;
                }
            }
        }

        internal class DebugInfoExpressionProxy
        {
            private readonly DebugInfoExpression _node;

            public DebugInfoExpressionProxy(DebugInfoExpression node)
            {
                this._node = node;
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public SymbolDocumentInfo Document
            {
                get
                {
                    return this._node.Document;
                }
            }

            public int EndColumn
            {
                get
                {
                    return this._node.EndColumn;
                }
            }

            public int EndLine
            {
                get
                {
                    return this._node.EndLine;
                }
            }

            public bool IsClear
            {
                get
                {
                    return this._node.IsClear;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public int StartColumn
            {
                get
                {
                    return this._node.StartColumn;
                }
            }

            public int StartLine
            {
                get
                {
                    return this._node.StartLine;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class DefaultExpressionProxy
        {
            private readonly DefaultExpression _node;

            public DefaultExpressionProxy(DefaultExpression node)
            {
                this._node = node;
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class DynamicExpressionProxy
        {
            private readonly DynamicExpression _node;

            public DynamicExpressionProxy(DynamicExpression node)
            {
                this._node = node;
            }

            public ReadOnlyCollection<Expression> Arguments
            {
                get
                {
                    return this._node.Arguments;
                }
            }

            public CallSiteBinder Binder
            {
                get
                {
                    return this._node.Binder;
                }
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public System.Type DelegateType
            {
                get
                {
                    return this._node.DelegateType;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        private class ExtensionInfo
        {
            internal readonly ExpressionType NodeType;
            internal readonly System.Type Type;

            public ExtensionInfo(ExpressionType nodeType, System.Type type)
            {
                this.NodeType = nodeType;
                this.Type = type;
            }
        }

        internal class GotoExpressionProxy
        {
            private readonly GotoExpression _node;

            public GotoExpressionProxy(GotoExpression node)
            {
                this._node = node;
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public GotoExpressionKind Kind
            {
                get
                {
                    return this._node.Kind;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public LabelTarget Target
            {
                get
                {
                    return this._node.Target;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }

            public Expression Value
            {
                get
                {
                    return this._node.Value;
                }
            }
        }

        internal class IndexExpressionProxy
        {
            private readonly IndexExpression _node;

            public IndexExpressionProxy(IndexExpression node)
            {
                this._node = node;
            }

            public ReadOnlyCollection<Expression> Arguments
            {
                get
                {
                    return this._node.Arguments;
                }
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public PropertyInfo Indexer
            {
                get
                {
                    return this._node.Indexer;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public Expression Object
            {
                get
                {
                    return this._node.Object;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class InvocationExpressionProxy
        {
            private readonly InvocationExpression _node;

            public InvocationExpressionProxy(InvocationExpression node)
            {
                this._node = node;
            }

            public ReadOnlyCollection<System.Linq.Expressions.Expression> Arguments
            {
                get
                {
                    return this._node.Arguments;
                }
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public System.Linq.Expressions.Expression Expression
            {
                get
                {
                    return this._node.Expression;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class LabelExpressionProxy
        {
            private readonly LabelExpression _node;

            public LabelExpressionProxy(LabelExpression node)
            {
                this._node = node;
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public Expression DefaultValue
            {
                get
                {
                    return this._node.DefaultValue;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public LabelTarget Target
            {
                get
                {
                    return this._node.Target;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class LambdaExpressionProxy
        {
            private readonly LambdaExpression _node;

            public LambdaExpressionProxy(LambdaExpression node)
            {
                this._node = node;
            }

            public Expression Body
            {
                get
                {
                    return this._node.Body;
                }
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public string Name
            {
                get
                {
                    return this._node.Name;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public ReadOnlyCollection<ParameterExpression> Parameters
            {
                get
                {
                    return this._node.Parameters;
                }
            }

            public System.Type ReturnType
            {
                get
                {
                    return this._node.ReturnType;
                }
            }

            public bool TailCall
            {
                get
                {
                    return this._node.TailCall;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        private delegate LambdaExpression LambdaFactory(Expression body, string name, bool tailCall, ReadOnlyCollection<ParameterExpression> parameters);

        internal class ListInitExpressionProxy
        {
            private readonly ListInitExpression _node;

            public ListInitExpressionProxy(ListInitExpression node)
            {
                this._node = node;
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public ReadOnlyCollection<ElementInit> Initializers
            {
                get
                {
                    return this._node.Initializers;
                }
            }

            public System.Linq.Expressions.NewExpression NewExpression
            {
                get
                {
                    return this._node.NewExpression;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class LoopExpressionProxy
        {
            private readonly LoopExpression _node;

            public LoopExpressionProxy(LoopExpression node)
            {
                this._node = node;
            }

            public Expression Body
            {
                get
                {
                    return this._node.Body;
                }
            }

            public LabelTarget BreakLabel
            {
                get
                {
                    return this._node.BreakLabel;
                }
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public LabelTarget ContinueLabel
            {
                get
                {
                    return this._node.ContinueLabel;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class MemberExpressionProxy
        {
            private readonly MemberExpression _node;

            public MemberExpressionProxy(MemberExpression node)
            {
                this._node = node;
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public System.Linq.Expressions.Expression Expression
            {
                get
                {
                    return this._node.Expression;
                }
            }

            public MemberInfo Member
            {
                get
                {
                    return this._node.Member;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class MemberInitExpressionProxy
        {
            private readonly MemberInitExpression _node;

            public MemberInitExpressionProxy(MemberInitExpression node)
            {
                this._node = node;
            }

            public ReadOnlyCollection<MemberBinding> Bindings
            {
                get
                {
                    return this._node.Bindings;
                }
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public System.Linq.Expressions.NewExpression NewExpression
            {
                get
                {
                    return this._node.NewExpression;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class MethodCallExpressionProxy
        {
            private readonly MethodCallExpression _node;

            public MethodCallExpressionProxy(MethodCallExpression node)
            {
                this._node = node;
            }

            public ReadOnlyCollection<Expression> Arguments
            {
                get
                {
                    return this._node.Arguments;
                }
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public MethodInfo Method
            {
                get
                {
                    return this._node.Method;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public Expression Object
            {
                get
                {
                    return this._node.Object;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class NewArrayExpressionProxy
        {
            private readonly NewArrayExpression _node;

            public NewArrayExpressionProxy(NewArrayExpression node)
            {
                this._node = node;
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public ReadOnlyCollection<Expression> Expressions
            {
                get
                {
                    return this._node.Expressions;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class NewExpressionProxy
        {
            private readonly NewExpression _node;

            public NewExpressionProxy(NewExpression node)
            {
                this._node = node;
            }

            public ReadOnlyCollection<Expression> Arguments
            {
                get
                {
                    return this._node.Arguments;
                }
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public ConstructorInfo Constructor
            {
                get
                {
                    return this._node.Constructor;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public ReadOnlyCollection<MemberInfo> Members
            {
                get
                {
                    return this._node.Members;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class ParameterExpressionProxy
        {
            private readonly ParameterExpression _node;

            public ParameterExpressionProxy(ParameterExpression node)
            {
                this._node = node;
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public bool IsByRef
            {
                get
                {
                    return this._node.IsByRef;
                }
            }

            public string Name
            {
                get
                {
                    return this._node.Name;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class RuntimeVariablesExpressionProxy
        {
            private readonly RuntimeVariablesExpression _node;

            public RuntimeVariablesExpressionProxy(RuntimeVariablesExpression node)
            {
                this._node = node;
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }

            public ReadOnlyCollection<ParameterExpression> Variables
            {
                get
                {
                    return this._node.Variables;
                }
            }
        }

        internal class SwitchCaseProxy
        {
            private readonly SwitchCase _node;

            public SwitchCaseProxy(SwitchCase node)
            {
                this._node = node;
            }

            public Expression Body
            {
                get
                {
                    return this._node.Body;
                }
            }

            public ReadOnlyCollection<Expression> TestValues
            {
                get
                {
                    return this._node.TestValues;
                }
            }
        }

        internal class SwitchExpressionProxy
        {
            private readonly SwitchExpression _node;

            public SwitchExpressionProxy(SwitchExpression node)
            {
                this._node = node;
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public ReadOnlyCollection<SwitchCase> Cases
            {
                get
                {
                    return this._node.Cases;
                }
            }

            public MethodInfo Comparison
            {
                get
                {
                    return this._node.Comparison;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public Expression DefaultBody
            {
                get
                {
                    return this._node.DefaultBody;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public Expression SwitchValue
            {
                get
                {
                    return this._node.SwitchValue;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class TryExpressionProxy
        {
            private readonly TryExpression _node;

            public TryExpressionProxy(TryExpression node)
            {
                this._node = node;
            }

            public Expression Body
            {
                get
                {
                    return this._node.Body;
                }
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public Expression Fault
            {
                get
                {
                    return this._node.Fault;
                }
            }

            public Expression Finally
            {
                get
                {
                    return this._node.Finally;
                }
            }

            public ReadOnlyCollection<CatchBlock> Handlers
            {
                get
                {
                    return this._node.Handlers;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }

        internal class TypeBinaryExpressionProxy
        {
            private readonly TypeBinaryExpression _node;

            public TypeBinaryExpressionProxy(TypeBinaryExpression node)
            {
                this._node = node;
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public System.Linq.Expressions.Expression Expression
            {
                get
                {
                    return this._node.Expression;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }

            public System.Type TypeOperand
            {
                get
                {
                    return this._node.TypeOperand;
                }
            }
        }

        internal class UnaryExpressionProxy
        {
            private readonly UnaryExpression _node;

            public UnaryExpressionProxy(UnaryExpression node)
            {
                this._node = node;
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public bool IsLifted
            {
                get
                {
                    return this._node.IsLifted;
                }
            }

            public bool IsLiftedToNull
            {
                get
                {
                    return this._node.IsLiftedToNull;
                }
            }

            public MethodInfo Method
            {
                get
                {
                    return this._node.Method;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public Expression Operand
            {
                get
                {
                    return this._node.Operand;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }
        }
    }
}

