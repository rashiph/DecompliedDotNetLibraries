namespace System.Linq.Expressions.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic.Utils;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class LambdaCompiler
    {
        private readonly BoundConstants _boundConstants;
        private static int _Counter;
        private readonly KeyedQueue<Type, LocalBuilder> _freeLocals;
        private readonly bool _hasClosureArgument;
        private readonly ILGenerator _ilg;
        private LabelScopeInfo _labelBlock;
        private readonly Dictionary<LabelTarget, LabelInfo> _labelInfo;
        private readonly LambdaExpression _lambda;
        private readonly MethodInfo _method;
        private CompilerScope _scope;
        private bool _sequencePointCleared;
        private readonly AnalyzedTree _tree;
        private readonly TypeBuilder _typeBuilder;

        private LambdaCompiler(AnalyzedTree tree, LambdaExpression lambda)
        {
            this._labelBlock = new LabelScopeInfo(null, LabelScopeKind.Lambda);
            this._labelInfo = new Dictionary<LabelTarget, LabelInfo>();
            this._freeLocals = new KeyedQueue<Type, LocalBuilder>();
            Type[] parameterTypes = GetParameterTypes(lambda).AddFirst<Type>(typeof(Closure));
            DynamicMethod method = new DynamicMethod(lambda.Name ?? "lambda_method", lambda.ReturnType, parameterTypes, true);
            this._tree = tree;
            this._lambda = lambda;
            this._method = method;
            this._ilg = method.GetILGenerator();
            this._hasClosureArgument = true;
            this._scope = tree.Scopes[lambda];
            this._boundConstants = tree.Constants[lambda];
            this.InitializeMethod();
        }

        private LambdaCompiler(LambdaCompiler parent, LambdaExpression lambda)
        {
            this._labelBlock = new LabelScopeInfo(null, LabelScopeKind.Lambda);
            this._labelInfo = new Dictionary<LabelTarget, LabelInfo>();
            this._freeLocals = new KeyedQueue<Type, LocalBuilder>();
            this._tree = parent._tree;
            this._lambda = lambda;
            this._method = parent._method;
            this._ilg = parent._ilg;
            this._hasClosureArgument = parent._hasClosureArgument;
            this._typeBuilder = parent._typeBuilder;
            this._scope = this._tree.Scopes[lambda];
            this._boundConstants = parent._boundConstants;
        }

        private LambdaCompiler(AnalyzedTree tree, LambdaExpression lambda, MethodBuilder method)
        {
            this._labelBlock = new LabelScopeInfo(null, LabelScopeKind.Lambda);
            this._labelInfo = new Dictionary<LabelTarget, LabelInfo>();
            this._freeLocals = new KeyedQueue<Type, LocalBuilder>();
            this._hasClosureArgument = tree.Scopes[lambda].NeedsClosure;
            Type[] parameterTypes = GetParameterTypes(lambda);
            if (this._hasClosureArgument)
            {
                parameterTypes = parameterTypes.AddFirst<Type>(typeof(Closure));
            }
            method.SetReturnType(lambda.ReturnType);
            method.SetParameters(parameterTypes);
            string[] strArray = lambda.Parameters.Map<ParameterExpression, string>(p => p.Name);
            int num = this._hasClosureArgument ? 2 : 1;
            for (int i = 0; i < strArray.Length; i++)
            {
                method.DefineParameter(i + num, ParameterAttributes.None, strArray[i]);
            }
            this._tree = tree;
            this._lambda = lambda;
            this._typeBuilder = (TypeBuilder) method.DeclaringType;
            this._method = method;
            this._ilg = method.GetILGenerator();
            this._scope = tree.Scopes[lambda];
            this._boundConstants = tree.Constants[lambda];
            this.InitializeMethod();
        }

        private void AddressOf(BinaryExpression node, Type type)
        {
            if (TypeUtils.AreEquivalent(type, node.Type))
            {
                this.EmitExpression(node.Left);
                this.EmitExpression(node.Right);
                Type type2 = node.Right.Type;
                if (type2.IsNullableType())
                {
                    LocalBuilder local = this.GetLocal(type2);
                    this._ilg.Emit(OpCodes.Stloc, local);
                    this._ilg.Emit(OpCodes.Ldloca, local);
                    this._ilg.EmitGetValue(type2);
                    this.FreeLocal(local);
                }
                Type nonNullableType = type2.GetNonNullableType();
                if (nonNullableType != typeof(int))
                {
                    this._ilg.EmitConvertToType(nonNullableType, typeof(int), true);
                }
                this._ilg.Emit(OpCodes.Ldelema, node.Type);
            }
            else
            {
                this.EmitExpressionAddress(node, type);
            }
        }

        private void AddressOf(IndexExpression node, Type type)
        {
            if (!TypeUtils.AreEquivalent(type, node.Type) || (node.Indexer != null))
            {
                this.EmitExpressionAddress(node, type);
            }
            else if (node.Arguments.Count == 1)
            {
                this.EmitExpression(node.Object);
                this.EmitExpression(node.Arguments[0]);
                this._ilg.Emit(OpCodes.Ldelema, node.Type);
            }
            else
            {
                MethodInfo method = node.Object.Type.GetMethod("Address", BindingFlags.Public | BindingFlags.Instance);
                this.EmitMethodCall(node.Object, method, node);
            }
        }

        private void AddressOf(MemberExpression node, Type type)
        {
            if (TypeUtils.AreEquivalent(type, node.Type))
            {
                Type objectType = null;
                if (node.Expression != null)
                {
                    this.EmitInstance(node.Expression, objectType = node.Expression.Type);
                }
                this.EmitMemberAddress(node.Member, objectType);
            }
            else
            {
                this.EmitExpressionAddress(node, type);
            }
        }

        private void AddressOf(MethodCallExpression node, Type type)
        {
            if ((!node.Method.IsStatic && node.Object.Type.IsArray) && (node.Method == node.Object.Type.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance)))
            {
                MethodInfo method = node.Object.Type.GetMethod("Address", BindingFlags.Public | BindingFlags.Instance);
                this.EmitMethodCall(node.Object, method, node);
            }
            else
            {
                this.EmitExpressionAddress(node, type);
            }
        }

        private void AddressOf(ParameterExpression node, Type type)
        {
            if (TypeUtils.AreEquivalent(type, node.Type))
            {
                if (node.IsByRef)
                {
                    this._scope.EmitGet(node);
                }
                else
                {
                    this._scope.EmitAddressOf(node);
                }
            }
            else
            {
                this.EmitExpressionAddress(node, type);
            }
        }

        private void AddressOf(UnaryExpression node, Type type)
        {
            this.EmitExpression(node.Operand);
            this._ilg.Emit(OpCodes.Unbox, type);
        }

        private WriteBack AddressOfWriteBack(IndexExpression node)
        {
            if ((node.Indexer == null) || !node.Indexer.CanWrite)
            {
                return null;
            }
            LocalBuilder instanceLocal = null;
            Type instanceType = null;
            if (node.Object != null)
            {
                this.EmitInstance(node.Object, instanceType = node.Object.Type);
                this._ilg.Emit(OpCodes.Dup);
                this._ilg.Emit(OpCodes.Stloc, instanceLocal = this.GetLocal(instanceType));
            }
            List<LocalBuilder> args = new List<LocalBuilder>();
            foreach (Expression expression in node.Arguments)
            {
                this.EmitExpression(expression);
                LocalBuilder local = this.GetLocal(expression.Type);
                this._ilg.Emit(OpCodes.Dup);
                this._ilg.Emit(OpCodes.Stloc, local);
                args.Add(local);
            }
            this.EmitGetIndexCall(node, instanceType);
            LocalBuilder valueLocal = this.GetLocal(node.Type);
            this._ilg.Emit(OpCodes.Stloc, valueLocal);
            this._ilg.Emit(OpCodes.Ldloca, valueLocal);
            return delegate {
                if (instanceLocal != null)
                {
                    this._ilg.Emit(OpCodes.Ldloc, instanceLocal);
                    this.FreeLocal(instanceLocal);
                }
                foreach (LocalBuilder builder in args)
                {
                    this._ilg.Emit(OpCodes.Ldloc, builder);
                    this.FreeLocal(builder);
                }
                this._ilg.Emit(OpCodes.Ldloc, valueLocal);
                this.FreeLocal(valueLocal);
                this.EmitSetIndexCall(node, instanceType);
            };
        }

        private WriteBack AddressOfWriteBack(MemberExpression node)
        {
            if ((node.Member.MemberType != MemberTypes.Property) || !((PropertyInfo) node.Member).CanWrite)
            {
                return null;
            }
            LocalBuilder instanceLocal = null;
            Type instanceType = null;
            if (node.Expression != null)
            {
                this.EmitInstance(node.Expression, instanceType = node.Expression.Type);
                this._ilg.Emit(OpCodes.Dup);
                this._ilg.Emit(OpCodes.Stloc, instanceLocal = this.GetLocal(instanceType));
            }
            PropertyInfo pi = (PropertyInfo) node.Member;
            this.EmitCall(instanceType, pi.GetGetMethod(true));
            LocalBuilder valueLocal = this.GetLocal(node.Type);
            this._ilg.Emit(OpCodes.Stloc, valueLocal);
            this._ilg.Emit(OpCodes.Ldloca, valueLocal);
            return delegate {
                if (instanceLocal != null)
                {
                    this._ilg.Emit(OpCodes.Ldloc, instanceLocal);
                    this.FreeLocal(instanceLocal);
                }
                this._ilg.Emit(OpCodes.Ldloc, valueLocal);
                this.FreeLocal(valueLocal);
                this.EmitCall(instanceType, pi.GetSetMethod(true));
            };
        }

        private void AddReturnLabel(LambdaExpression lambda)
        {
            Expression body = lambda.Body;
        Label_0007:
            switch (body.NodeType)
            {
                case ExpressionType.Block:
                {
                    BlockExpression expression2 = (BlockExpression) body;
                    for (int i = expression2.ExpressionCount - 1; i >= 0; i--)
                    {
                        body = expression2.GetExpression(i);
                        if (Significant(body))
                        {
                            break;
                        }
                    }
                    goto Label_0007;
                }
                case ExpressionType.Label:
                {
                    LabelTarget key = ((LabelExpression) body).Target;
                    this._labelInfo.Add(key, new LabelInfo(this._ilg, key, TypeUtils.AreReferenceAssignable(lambda.ReturnType, key.Type)));
                    break;
                }
            }
        }

        private static void AddToBuckets(List<List<SwitchLabel>> buckets, SwitchLabel key)
        {
            List<SwitchLabel> list2;
            if (buckets.Count > 0)
            {
                List<SwitchLabel> list = buckets[buckets.Count - 1];
                if (FitsInBucket(list, key.Key, 1))
                {
                    list.Add(key);
                    MergeBuckets(buckets);
                    return;
                }
            }
            list2 = new List<SwitchLabel> {
                key,
                list2
            };
        }

        private static AnalyzedTree AnalyzeLambda(ref LambdaExpression lambda)
        {
            lambda = StackSpiller.AnalyzeLambda(lambda);
            return VariableBinder.Bind(lambda);
        }

        private static bool CanOptimizeSwitchType(Type valueType)
        {
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return true;
            }
            return false;
        }

        private void CheckRethrow()
        {
            for (LabelScopeInfo info = this._labelBlock; info != null; info = info.Parent)
            {
                if (info.Kind == LabelScopeKind.Catch)
                {
                    return;
                }
                if (info.Kind == LabelScopeKind.Finally)
                {
                    break;
                }
            }
            throw System.Linq.Expressions.Error.RethrowRequiresCatch();
        }

        private void CheckTry()
        {
            for (LabelScopeInfo info = this._labelBlock; info != null; info = info.Parent)
            {
                if (info.Kind == LabelScopeKind.Filter)
                {
                    throw System.Linq.Expressions.Error.TryNotAllowedInFilter();
                }
            }
        }

        internal static Delegate Compile(LambdaExpression lambda, DebugInfoGenerator debugInfoGenerator)
        {
            AnalyzedTree tree = AnalyzeLambda(ref lambda);
            tree.DebugInfoGenerator = debugInfoGenerator;
            LambdaCompiler compiler = new LambdaCompiler(tree, lambda);
            compiler.EmitLambdaBody();
            return compiler.CreateDelegate();
        }

        internal static void Compile(LambdaExpression lambda, MethodBuilder method, DebugInfoGenerator debugInfoGenerator)
        {
            AnalyzedTree tree = AnalyzeLambda(ref lambda);
            tree.DebugInfoGenerator = debugInfoGenerator;
            new LambdaCompiler(tree, lambda, method).EmitLambdaBody();
        }

        private static decimal ConvertSwitchValue(object value)
        {
            if (value is char)
            {
                return (char) value;
            }
            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        private Delegate CreateDelegate()
        {
            return this._method.CreateDelegate(this._lambda.Type, new Closure(this._boundConstants.ToArray(), null));
        }

        private MemberExpression CreateLazyInitializedField<T>(string name)
        {
            if (this._method is DynamicMethod)
            {
                return Expression.Field(Expression.Constant(new StrongBox<T>()), "Value");
            }
            return Expression.Field(null, this.CreateStaticField(name, typeof(T)));
        }

        private FieldBuilder CreateStaticField(string name, Type type)
        {
            return this._typeBuilder.DefineField(string.Concat(new object[] { "<ExpressionCompilerImplementationDetails>{", Interlocked.Increment(ref _Counter), "}", name }), type, FieldAttributes.Static | FieldAttributes.Private);
        }

        private void DefineBlockLabels(Expression node)
        {
            BlockExpression expression = node as BlockExpression;
            if ((expression != null) && !(expression is SpilledExpressionBlock))
            {
                int index = 0;
                int expressionCount = expression.ExpressionCount;
                while (index < expressionCount)
                {
                    LabelExpression expression3 = expression.GetExpression(index) as LabelExpression;
                    if (expression3 != null)
                    {
                        this.DefineLabel(expression3.Target);
                    }
                    index++;
                }
            }
        }

        private LabelInfo DefineLabel(LabelTarget node)
        {
            if (node == null)
            {
                return new LabelInfo(this._ilg, null, false);
            }
            LabelInfo info = this.EnsureLabel(node);
            info.Define(this._labelBlock);
            return info;
        }

        private void DefineSwitchCaseLabel(SwitchCase @case, out Label label, out bool isGoto)
        {
            GotoExpression body = @case.Body as GotoExpression;
            if ((body != null) && (body.Value == null))
            {
                LabelInfo info = this.ReferenceLabel(body.Target);
                if (info.CanBranch)
                {
                    label = info.Label;
                    isGoto = true;
                    return;
                }
            }
            label = this._ilg.DefineLabel();
            isGoto = false;
        }

        private void Emit(BlockExpression node, CompilationFlags flags)
        {
            this.EnterScope(node);
            CompilationFlags flags2 = flags & CompilationFlags.EmitAsTypeMask;
            int expressionCount = node.ExpressionCount;
            CompilationFlags flags3 = flags & CompilationFlags.EmitAsTailCallMask;
            CompilationFlags flags4 = (flags3 == CompilationFlags.EmitAsNoTail) ? CompilationFlags.EmitAsNoTail : CompilationFlags.EmitAsMiddle;
            for (int i = 0; i < (expressionCount - 1); i++)
            {
                Expression expression = node.GetExpression(i);
                Expression expression2 = node.GetExpression(i + 1);
                if (this.EmitDebugSymbols)
                {
                    DebugInfoExpression expression3 = expression as DebugInfoExpression;
                    if (((expression3 != null) && expression3.IsClear) && (expression2 is DebugInfoExpression))
                    {
                        continue;
                    }
                }
                CompilationFlags newValue = flags4;
                GotoExpression expression4 = expression2 as GotoExpression;
                if (((expression4 != null) && ((expression4.Value == null) || !Significant(expression4.Value))) && this.ReferenceLabel(expression4.Target).CanReturn)
                {
                    newValue = CompilationFlags.EmitAsTail;
                }
                flags = UpdateEmitAsTailCallFlag(flags, newValue);
                this.EmitExpressionAsVoid(expression, flags);
            }
            if ((flags2 == CompilationFlags.EmitAsVoidType) || (node.Type == typeof(void)))
            {
                this.EmitExpressionAsVoid(node.GetExpression(expressionCount - 1), flags3);
            }
            else
            {
                this.EmitExpressionAsType(node.GetExpression(expressionCount - 1), node.Type, flags3);
            }
            this.ExitScope(node);
        }

        private void EmitAddress(Expression node, Type type)
        {
            this.EmitAddress(node, type, CompilationFlags.EmitExpressionStart);
        }

        private void EmitAddress(Expression node, Type type, CompilationFlags flags)
        {
            bool flag = (flags & CompilationFlags.EmitExpressionStartMask) == CompilationFlags.EmitExpressionStart;
            CompilationFlags flags2 = flag ? this.EmitExpressionStart(node) : CompilationFlags.EmitNoExpressionStart;
            switch (node.NodeType)
            {
                case ExpressionType.ArrayIndex:
                    this.AddressOf((BinaryExpression) node, type);
                    break;

                case ExpressionType.Call:
                    this.AddressOf((MethodCallExpression) node, type);
                    break;

                case ExpressionType.MemberAccess:
                    this.AddressOf((MemberExpression) node, type);
                    break;

                case ExpressionType.Parameter:
                    this.AddressOf((ParameterExpression) node, type);
                    break;

                case ExpressionType.Index:
                    this.AddressOf((IndexExpression) node, type);
                    break;

                case ExpressionType.Unbox:
                    this.AddressOf((UnaryExpression) node, type);
                    break;

                default:
                    this.EmitExpressionAddress(node, type);
                    break;
            }
            if (flag)
            {
                this.EmitExpressionEnd(flags2);
            }
        }

        private WriteBack EmitAddressWriteBack(Expression node, Type type)
        {
            CompilationFlags flags = this.EmitExpressionStart(node);
            WriteBack back = null;
            if (TypeUtils.AreEquivalent(type, node.Type))
            {
                switch (node.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        back = this.AddressOfWriteBack((MemberExpression) node);
                        break;

                    case ExpressionType.Index:
                        back = this.AddressOfWriteBack((IndexExpression) node);
                        break;
                }
            }
            if (back == null)
            {
                this.EmitAddress(node, type, CompilationFlags.EmitAsNoTail | CompilationFlags.EmitNoExpressionStart);
            }
            this.EmitExpressionEnd(flags);
            return back;
        }

        private void EmitAndAlsoBinaryExpression(Expression expr, CompilationFlags flags)
        {
            BinaryExpression b = (BinaryExpression) expr;
            if ((b.Method != null) && !b.IsLiftedLogical)
            {
                this.EmitMethodAndAlso(b, flags);
            }
            else if (b.Left.Type == typeof(bool?))
            {
                this.EmitLiftedAndAlso(b);
            }
            else if (b.IsLiftedLogical)
            {
                this.EmitExpression(b.ReduceUserdefinedLifted());
            }
            else
            {
                this.EmitUnliftedAndAlso(b);
            }
        }

        private List<WriteBack> EmitArguments(MethodBase method, IArgumentProvider args)
        {
            return this.EmitArguments(method, args, 0);
        }

        private List<WriteBack> EmitArguments(MethodBase method, IArgumentProvider args, int skipParameters)
        {
            ParameterInfo[] parametersCached = method.GetParametersCached();
            List<WriteBack> list = new List<WriteBack>();
            int index = skipParameters;
            int length = parametersCached.Length;
            while (index < length)
            {
                ParameterInfo info = parametersCached[index];
                Expression argument = args.GetArgument(index - skipParameters);
                Type parameterType = info.ParameterType;
                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                    WriteBack item = this.EmitAddressWriteBack(argument, parameterType);
                    if (item != null)
                    {
                        list.Add(item);
                    }
                }
                else
                {
                    this.EmitExpression(argument);
                }
                index++;
            }
            return list;
        }

        private void EmitAssign(BinaryExpression node, CompilationFlags emitAs)
        {
            ExpressionType nodeType = node.Left.NodeType;
            if (nodeType != ExpressionType.MemberAccess)
            {
                if (nodeType != ExpressionType.Parameter)
                {
                    if (nodeType != ExpressionType.Index)
                    {
                        throw System.Linq.Expressions.Error.InvalidLvalue(node.Left.NodeType);
                    }
                    this.EmitIndexAssignment(node, emitAs);
                    return;
                }
            }
            else
            {
                this.EmitMemberAssignment(node, emitAs);
                return;
            }
            this.EmitVariableAssignment(node, emitAs);
        }

        private void EmitAssignBinaryExpression(Expression expr)
        {
            this.EmitAssign((BinaryExpression) expr, CompilationFlags.EmitAsDefaultType);
        }

        private void EmitBinaryExpression(Expression expr)
        {
            this.EmitBinaryExpression(expr, CompilationFlags.EmitAsNoTail);
        }

        private void EmitBinaryExpression(Expression expr, CompilationFlags flags)
        {
            BinaryExpression b = (BinaryExpression) expr;
            if (b.Method != null)
            {
                this.EmitBinaryMethod(b, flags);
            }
            else
            {
                if (((b.NodeType == ExpressionType.Equal) || (b.NodeType == ExpressionType.NotEqual)) && ((b.Type == typeof(bool)) || (b.Type == typeof(bool?))))
                {
                    if ((ConstantCheck.IsNull(b.Left) && !ConstantCheck.IsNull(b.Right)) && b.Right.Type.IsNullableType())
                    {
                        this.EmitNullEquality(b.NodeType, b.Right, b.IsLiftedToNull);
                        return;
                    }
                    if ((ConstantCheck.IsNull(b.Right) && !ConstantCheck.IsNull(b.Left)) && b.Left.Type.IsNullableType())
                    {
                        this.EmitNullEquality(b.NodeType, b.Left, b.IsLiftedToNull);
                        return;
                    }
                    this.EmitExpression(GetEqualityOperand(b.Left));
                    this.EmitExpression(GetEqualityOperand(b.Right));
                }
                else
                {
                    this.EmitExpression(b.Left);
                    this.EmitExpression(b.Right);
                }
                this.EmitBinaryOperator(b.NodeType, b.Left.Type, b.Right.Type, b.Type, b.IsLiftedToNull);
            }
        }

        private void EmitBinaryMethod(BinaryExpression b, CompilationFlags flags)
        {
            if (!b.IsLifted)
            {
                this.EmitMethodCallExpression(Expression.Call(null, b.Method, b.Left, b.Right), flags);
                return;
            }
            ParameterExpression expression = Expression.Variable(b.Left.Type.GetNonNullableType(), null);
            ParameterExpression expression2 = Expression.Variable(b.Right.Type.GetNonNullableType(), null);
            MethodCallExpression mc = Expression.Call(null, b.Method, expression, expression2);
            Type resultType = null;
            if (b.IsLiftedToNull)
            {
                resultType = TypeUtils.GetNullableType(mc.Type);
            }
            else
            {
                switch (b.NodeType)
                {
                    case ExpressionType.Equal:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.NotEqual:
                        if (mc.Type != typeof(bool))
                        {
                            throw System.Linq.Expressions.Error.ArgumentMustBeBoolean();
                        }
                        resultType = typeof(bool);
                        goto Label_00D2;
                }
                resultType = TypeUtils.GetNullableType(mc.Type);
            }
        Label_00D2:;
            ParameterExpression[] variables = new ParameterExpression[] { expression, expression2 };
            Expression[] arguments = new Expression[] { b.Left, b.Right };
            ValidateLift(variables, arguments);
            this.EmitLift(b.NodeType, resultType, mc, variables, arguments);
        }

        private void EmitBinaryOperator(ExpressionType op, Type leftType, Type rightType, Type resultType, bool liftedToNull)
        {
            bool flag = leftType.IsNullableType();
            bool flag2 = rightType.IsNullableType();
            switch (op)
            {
                case ExpressionType.ArrayIndex:
                    if (rightType != typeof(int))
                    {
                        throw ContractUtils.Unreachable;
                    }
                    this._ilg.EmitLoadElement(leftType.GetElementType());
                    return;

                case ExpressionType.Coalesce:
                    throw System.Linq.Expressions.Error.UnexpectedCoalesceOperator();
            }
            if (flag || flag2)
            {
                this.EmitLiftedBinaryOp(op, leftType, rightType, resultType, liftedToNull);
            }
            else
            {
                this.EmitUnliftedBinaryOp(op, leftType, rightType);
                this.EmitConvertArithmeticResult(op, resultType);
            }
        }

        private void EmitBinding(MemberBinding binding, Type objectType)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    this.EmitMemberAssignment((MemberAssignment) binding, objectType);
                    return;

                case MemberBindingType.MemberBinding:
                    this.EmitMemberMemberBinding((MemberMemberBinding) binding);
                    return;

                case MemberBindingType.ListBinding:
                    this.EmitMemberListBinding((MemberListBinding) binding);
                    return;
            }
            throw System.Linq.Expressions.Error.UnknownBindingType();
        }

        private void EmitBlockExpression(Expression expr, CompilationFlags flags)
        {
            this.Emit((BlockExpression) expr, UpdateEmitAsTypeFlag(flags, CompilationFlags.EmitAsDefaultType));
        }

        private void EmitBranchAnd(bool branch, BinaryExpression node, Label label)
        {
            Label label2 = this._ilg.DefineLabel();
            this.EmitExpressionAndBranch(!branch, node.Left, label2);
            this.EmitExpressionAndBranch(branch, node.Right, label);
            this._ilg.MarkLabel(label2);
        }

        private void EmitBranchBlock(bool branch, BlockExpression node, Label label)
        {
            this.EnterScope(node);
            int expressionCount = node.ExpressionCount;
            for (int i = 0; i < (expressionCount - 1); i++)
            {
                this.EmitExpressionAsVoid(node.GetExpression(i));
            }
            this.EmitExpressionAndBranch(branch, node.GetExpression(expressionCount - 1), label);
            this.ExitScope(node);
        }

        private void EmitBranchComparison(bool branch, BinaryExpression node, Label label)
        {
            bool flag = branch == (node.NodeType == ExpressionType.Equal);
            if (node.Method != null)
            {
                this.EmitBinaryMethod(node, CompilationFlags.EmitAsNoTail);
                this.EmitBranchOp(branch, label);
            }
            else if (ConstantCheck.IsNull(node.Left))
            {
                if (node.Right.Type.IsNullableType())
                {
                    this.EmitAddress(node.Right, node.Right.Type);
                    this._ilg.EmitHasValue(node.Right.Type);
                }
                else
                {
                    this.EmitExpression(GetEqualityOperand(node.Right));
                }
                this.EmitBranchOp(!flag, label);
            }
            else if (ConstantCheck.IsNull(node.Right))
            {
                if (node.Left.Type.IsNullableType())
                {
                    this.EmitAddress(node.Left, node.Left.Type);
                    this._ilg.EmitHasValue(node.Left.Type);
                }
                else
                {
                    this.EmitExpression(GetEqualityOperand(node.Left));
                }
                this.EmitBranchOp(!flag, label);
            }
            else if (node.Left.Type.IsNullableType() || node.Right.Type.IsNullableType())
            {
                this.EmitBinaryExpression(node);
                this.EmitBranchOp(branch, label);
            }
            else
            {
                this.EmitExpression(GetEqualityOperand(node.Left));
                this.EmitExpression(GetEqualityOperand(node.Right));
                if (flag)
                {
                    this._ilg.Emit(OpCodes.Beq, label);
                }
                else
                {
                    this._ilg.Emit(OpCodes.Ceq);
                    this._ilg.Emit(OpCodes.Brfalse, label);
                }
            }
        }

        private void EmitBranchLogical(bool branch, BinaryExpression node, Label label)
        {
            if ((node.Method != null) || node.IsLifted)
            {
                this.EmitExpression(node);
                this.EmitBranchOp(branch, label);
            }
            else
            {
                bool flag = node.NodeType == ExpressionType.AndAlso;
                if (branch == flag)
                {
                    this.EmitBranchAnd(branch, node, label);
                }
                else
                {
                    this.EmitBranchOr(branch, node, label);
                }
            }
        }

        private void EmitBranchNot(bool branch, UnaryExpression node, Label label)
        {
            if (node.Method != null)
            {
                this.EmitExpression(node, CompilationFlags.EmitAsNoTail | CompilationFlags.EmitNoExpressionStart);
                this.EmitBranchOp(branch, label);
            }
            else
            {
                this.EmitExpressionAndBranch(!branch, node.Operand, label);
            }
        }

        private void EmitBranchOp(bool branch, Label label)
        {
            this._ilg.Emit(branch ? OpCodes.Brtrue : OpCodes.Brfalse, label);
        }

        private void EmitBranchOr(bool branch, BinaryExpression node, Label label)
        {
            this.EmitExpressionAndBranch(branch, node.Left, label);
            this.EmitExpressionAndBranch(branch, node.Right, label);
        }

        private void EmitCall(Type objectType, MethodInfo method)
        {
            if (method.CallingConvention == CallingConventions.VarArgs)
            {
                throw System.Linq.Expressions.Error.UnexpectedVarArgsCall(method);
            }
            OpCode opcode = UseVirtual(method) ? OpCodes.Callvirt : OpCodes.Call;
            if ((opcode == OpCodes.Callvirt) && objectType.IsValueType)
            {
                this._ilg.Emit(OpCodes.Constrained, objectType);
            }
            this._ilg.Emit(opcode, method);
        }

        private void EmitCatchStart(CatchBlock cb)
        {
            if (cb.Filter == null)
            {
                this.EmitSaveExceptionOrPop(cb);
            }
            else
            {
                Label label = this._ilg.DefineLabel();
                Label label2 = this._ilg.DefineLabel();
                this._ilg.Emit(OpCodes.Isinst, cb.Test);
                this._ilg.Emit(OpCodes.Dup);
                this._ilg.Emit(OpCodes.Brtrue, label2);
                this._ilg.Emit(OpCodes.Pop);
                this._ilg.Emit(OpCodes.Ldc_I4_0);
                this._ilg.Emit(OpCodes.Br, label);
                this._ilg.MarkLabel(label2);
                this.EmitSaveExceptionOrPop(cb);
                this.PushLabelBlock(LabelScopeKind.Filter);
                this.EmitExpression(cb.Filter);
                this.PopLabelBlock(LabelScopeKind.Filter);
                this._ilg.MarkLabel(label);
                this._ilg.BeginCatchBlock(null);
                this._ilg.Emit(OpCodes.Pop);
            }
        }

        internal void EmitClosureArgument()
        {
            this._ilg.EmitLoadArg(0);
        }

        private void EmitClosureCreation(LambdaCompiler inner)
        {
            bool needsClosure = inner._scope.NeedsClosure;
            bool flag2 = inner._boundConstants.Count > 0;
            if (!needsClosure && !flag2)
            {
                this._ilg.EmitNull();
            }
            else
            {
                if (flag2)
                {
                    this._boundConstants.EmitConstant(this, inner._boundConstants.ToArray(), typeof(object[]));
                }
                else
                {
                    this._ilg.EmitNull();
                }
                if (needsClosure)
                {
                    this._scope.EmitGet(this._scope.NearestHoistedLocals.SelfVariable);
                }
                else
                {
                    this._ilg.EmitNull();
                }
                this._ilg.EmitNew(typeof(Closure).GetConstructor(new Type[] { typeof(object[]), typeof(object[]) }));
            }
        }

        private void EmitCoalesceBinaryExpression(Expression expr)
        {
            BinaryExpression b = (BinaryExpression) expr;
            if (b.Left.Type.IsNullableType())
            {
                this.EmitNullableCoalesce(b);
            }
            else
            {
                if (b.Left.Type.IsValueType)
                {
                    throw System.Linq.Expressions.Error.CoalesceUsedOnNonNullType();
                }
                if (b.Conversion != null)
                {
                    this.EmitLambdaReferenceCoalesce(b);
                }
                else
                {
                    this.EmitReferenceCoalesceWithoutConversion(b);
                }
            }
        }

        private void EmitConditionalExpression(Expression expr, CompilationFlags flags)
        {
            ConditionalExpression expression = (ConditionalExpression) expr;
            Label label = this._ilg.DefineLabel();
            this.EmitExpressionAndBranch(false, expression.Test, label);
            this.EmitExpressionAsType(expression.IfTrue, expression.Type, flags);
            if (NotEmpty(expression.IfFalse))
            {
                Label label2 = this._ilg.DefineLabel();
                if ((flags & CompilationFlags.EmitAsTailCallMask) == CompilationFlags.EmitAsTail)
                {
                    this._ilg.Emit(OpCodes.Ret);
                }
                else
                {
                    this._ilg.Emit(OpCodes.Br, label2);
                }
                this._ilg.MarkLabel(label);
                this.EmitExpressionAsType(expression.IfFalse, expression.Type, flags);
                this._ilg.MarkLabel(label2);
            }
            else
            {
                this._ilg.MarkLabel(label);
            }
        }

        private void EmitConstant(object value, Type type)
        {
            if (ILGen.CanEmitConstant(value, type))
            {
                this._ilg.EmitConstant(value, type);
            }
            else
            {
                this._boundConstants.EmitConstant(this, value, type);
            }
        }

        internal void EmitConstantArray<T>(T[] array)
        {
            if (this._method is DynamicMethod)
            {
                this.EmitConstant(array, typeof(T[]));
            }
            else if (this._typeBuilder != null)
            {
                FieldBuilder field = this.CreateStaticField("ConstantArray", typeof(T[]));
                Label label = this._ilg.DefineLabel();
                this._ilg.Emit(OpCodes.Ldsfld, field);
                this._ilg.Emit(OpCodes.Ldnull);
                this._ilg.Emit(OpCodes.Bne_Un, label);
                this._ilg.EmitArray<T>(array);
                this._ilg.Emit(OpCodes.Stsfld, field);
                this._ilg.MarkLabel(label);
                this._ilg.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                this._ilg.EmitArray<T>(array);
            }
        }

        private void EmitConstantExpression(Expression expr)
        {
            ConstantExpression expression = (ConstantExpression) expr;
            this.EmitConstant(expression.Value, expression.Type);
        }

        private void EmitConstantOne(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    this._ilg.Emit(OpCodes.Ldc_I4_1);
                    return;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    this._ilg.Emit(OpCodes.Ldc_I8, (long) 1L);
                    return;

                case TypeCode.Single:
                    this._ilg.Emit(OpCodes.Ldc_R4, (float) 1f);
                    return;

                case TypeCode.Double:
                    this._ilg.Emit(OpCodes.Ldc_R8, (double) 1.0);
                    return;
            }
            throw ContractUtils.Unreachable;
        }

        private void EmitConvert(UnaryExpression node, CompilationFlags flags)
        {
            if (node.Method != null)
            {
                if (node.IsLifted && (!node.Type.IsValueType || !node.Operand.Type.IsValueType))
                {
                    ParameterInfo[] parametersCached = node.Method.GetParametersCached();
                    Type parameterType = parametersCached[0].ParameterType;
                    if (parameterType.IsByRef)
                    {
                        parameterType = parameterType.GetElementType();
                    }
                    UnaryExpression expression = Expression.Convert(Expression.Call(node.Method, Expression.Convert(node.Operand, parametersCached[0].ParameterType)), node.Type);
                    this.EmitConvert(expression, flags);
                }
                else
                {
                    this.EmitUnaryMethod(node, flags);
                }
            }
            else if (node.Type == typeof(void))
            {
                this.EmitExpressionAsVoid(node.Operand, flags);
            }
            else if (TypeUtils.AreEquivalent(node.Operand.Type, node.Type))
            {
                this.EmitExpression(node.Operand, flags);
            }
            else
            {
                this.EmitExpression(node.Operand);
                this._ilg.EmitConvertToType(node.Operand.Type, node.Type, node.NodeType == ExpressionType.ConvertChecked);
            }
        }

        private void EmitConvertArithmeticResult(ExpressionType op, Type resultType)
        {
            switch (Type.GetTypeCode(resultType))
            {
                case TypeCode.SByte:
                    this._ilg.Emit(IsChecked(op) ? OpCodes.Conv_Ovf_I1 : OpCodes.Conv_I1);
                    return;

                case TypeCode.Byte:
                    this._ilg.Emit(IsChecked(op) ? OpCodes.Conv_Ovf_U1 : OpCodes.Conv_U1);
                    return;

                case TypeCode.Int16:
                    this._ilg.Emit(IsChecked(op) ? OpCodes.Conv_Ovf_I2 : OpCodes.Conv_I2);
                    return;

                case TypeCode.UInt16:
                    this._ilg.Emit(IsChecked(op) ? OpCodes.Conv_Ovf_U2 : OpCodes.Conv_U2);
                    return;
            }
        }

        private void EmitConvertUnaryExpression(Expression expr, CompilationFlags flags)
        {
            this.EmitConvert((UnaryExpression) expr, flags);
        }

        private void EmitDebugInfoExpression(Expression expr)
        {
            if (this.EmitDebugSymbols)
            {
                DebugInfoExpression sequencePoint = (DebugInfoExpression) expr;
                if (!sequencePoint.IsClear || !this._sequencePointCleared)
                {
                    this._tree.DebugInfoGenerator.MarkSequencePoint(this._lambda, this._method, this._ilg, sequencePoint);
                    this._ilg.Emit(OpCodes.Nop);
                    this._sequencePointCleared = sequencePoint.IsClear;
                }
            }
        }

        private void EmitDefaultExpression(Expression expr)
        {
            DefaultExpression expression = (DefaultExpression) expr;
            if (expression.Type != typeof(void))
            {
                this._ilg.EmitDefault(expression.Type);
            }
        }

        private void EmitDelegateConstruction(LambdaCompiler inner)
        {
            Type type = inner._lambda.Type;
            DynamicMethod method = inner._method as DynamicMethod;
            if (method != null)
            {
                this._boundConstants.EmitConstant(this, method, typeof(DynamicMethod));
                this._ilg.EmitType(type);
                this.EmitClosureCreation(inner);
                this._ilg.Emit(OpCodes.Callvirt, typeof(DynamicMethod).GetMethod("CreateDelegate", new Type[] { typeof(Type), typeof(object) }));
                this._ilg.Emit(OpCodes.Castclass, type);
            }
            else
            {
                this.EmitClosureCreation(inner);
                this._ilg.Emit(OpCodes.Ldftn, inner._method);
                this._ilg.Emit(OpCodes.Newobj, (ConstructorInfo) type.GetMember(".ctor")[0]);
            }
        }

        private void EmitDelegateConstruction(LambdaExpression lambda)
        {
            LambdaCompiler compiler;
            if (this._method is DynamicMethod)
            {
                compiler = new LambdaCompiler(this._tree, lambda);
            }
            else
            {
                string name = string.IsNullOrEmpty(lambda.Name) ? GetUniqueMethodName() : lambda.Name;
                MethodBuilder method = this._typeBuilder.DefineMethod(name, MethodAttributes.Static | MethodAttributes.Private);
                compiler = new LambdaCompiler(this._tree, lambda, method);
            }
            compiler.EmitLambdaBody(this._scope, false, CompilationFlags.EmitAsNoTail);
            this.EmitDelegateConstruction(compiler);
        }

        private void EmitDynamicExpression(Expression expr)
        {
            if (!(this._method is DynamicMethod))
            {
                throw System.Linq.Expressions.Error.CannotCompileDynamic();
            }
            DynamicExpression args = (DynamicExpression) expr;
            CallSite site = CallSite.Create(args.DelegateType, args.Binder);
            Type type = site.GetType();
            MethodInfo method = args.DelegateType.GetMethod("Invoke");
            this.EmitConstant(site, type);
            this._ilg.Emit(OpCodes.Dup);
            LocalBuilder local = this.GetLocal(typeof(CallSite));
            this._ilg.Emit(OpCodes.Stloc, local);
            this._ilg.Emit(OpCodes.Ldfld, type.GetField("Target"));
            this._ilg.Emit(OpCodes.Ldloc, local);
            this.FreeLocal(local);
            List<WriteBack> writeBacks = this.EmitArguments(method, args, 1);
            this._ilg.Emit(OpCodes.Callvirt, method);
            EmitWriteBack(writeBacks);
        }

        internal void EmitExpression(Expression node)
        {
            this.EmitExpression(node, CompilationFlags.EmitAsNoTail | CompilationFlags.EmitExpressionStart);
        }

        private void EmitExpression(Expression node, CompilationFlags flags)
        {
            bool flag = (flags & CompilationFlags.EmitExpressionStartMask) == CompilationFlags.EmitExpressionStart;
            CompilationFlags flags2 = flag ? this.EmitExpressionStart(node) : CompilationFlags.EmitNoExpressionStart;
            flags &= CompilationFlags.EmitAsTailCallMask;
            switch (node.NodeType)
            {
                case ExpressionType.Add:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.AddChecked:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.And:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.AndAlso:
                    this.EmitAndAlsoBinaryExpression(node, flags);
                    break;

                case ExpressionType.ArrayLength:
                    this.EmitUnaryExpression(node, flags);
                    break;

                case ExpressionType.ArrayIndex:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.Call:
                    this.EmitMethodCallExpression(node, flags);
                    break;

                case ExpressionType.Coalesce:
                    this.EmitCoalesceBinaryExpression(node);
                    break;

                case ExpressionType.Conditional:
                    this.EmitConditionalExpression(node, flags);
                    break;

                case ExpressionType.Constant:
                    this.EmitConstantExpression(node);
                    break;

                case ExpressionType.Convert:
                    this.EmitConvertUnaryExpression(node, flags);
                    break;

                case ExpressionType.ConvertChecked:
                    this.EmitConvertUnaryExpression(node, flags);
                    break;

                case ExpressionType.Divide:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.Equal:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.ExclusiveOr:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.GreaterThan:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.Invoke:
                    this.EmitInvocationExpression(node, flags);
                    break;

                case ExpressionType.Lambda:
                    this.EmitLambdaExpression(node);
                    break;

                case ExpressionType.LeftShift:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.LessThan:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.LessThanOrEqual:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.ListInit:
                    this.EmitListInitExpression(node);
                    break;

                case ExpressionType.MemberAccess:
                    this.EmitMemberExpression(node);
                    break;

                case ExpressionType.MemberInit:
                    this.EmitMemberInitExpression(node);
                    break;

                case ExpressionType.Modulo:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.Multiply:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.MultiplyChecked:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.Negate:
                    this.EmitUnaryExpression(node, flags);
                    break;

                case ExpressionType.UnaryPlus:
                    this.EmitUnaryExpression(node, flags);
                    break;

                case ExpressionType.NegateChecked:
                    this.EmitUnaryExpression(node, flags);
                    break;

                case ExpressionType.New:
                    this.EmitNewExpression(node);
                    break;

                case ExpressionType.NewArrayInit:
                    this.EmitNewArrayExpression(node);
                    break;

                case ExpressionType.NewArrayBounds:
                    this.EmitNewArrayExpression(node);
                    break;

                case ExpressionType.Not:
                    this.EmitUnaryExpression(node, flags);
                    break;

                case ExpressionType.NotEqual:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.Or:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.OrElse:
                    this.EmitOrElseBinaryExpression(node, flags);
                    break;

                case ExpressionType.Parameter:
                    this.EmitParameterExpression(node);
                    break;

                case ExpressionType.Power:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.Quote:
                    this.EmitQuoteUnaryExpression(node);
                    break;

                case ExpressionType.RightShift:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.Subtract:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.SubtractChecked:
                    this.EmitBinaryExpression(node, flags);
                    break;

                case ExpressionType.TypeAs:
                    this.EmitUnaryExpression(node, flags);
                    break;

                case ExpressionType.TypeIs:
                    this.EmitTypeBinaryExpression(node);
                    break;

                case ExpressionType.Assign:
                    this.EmitAssignBinaryExpression(node);
                    break;

                case ExpressionType.Block:
                    this.EmitBlockExpression(node, flags);
                    break;

                case ExpressionType.DebugInfo:
                    this.EmitDebugInfoExpression(node);
                    break;

                case ExpressionType.Decrement:
                    this.EmitUnaryExpression(node, flags);
                    break;

                case ExpressionType.Dynamic:
                    this.EmitDynamicExpression(node);
                    break;

                case ExpressionType.Default:
                    this.EmitDefaultExpression(node);
                    break;

                case ExpressionType.Extension:
                    EmitExtensionExpression(node);
                    break;

                case ExpressionType.Goto:
                    this.EmitGotoExpression(node, flags);
                    break;

                case ExpressionType.Increment:
                    this.EmitUnaryExpression(node, flags);
                    break;

                case ExpressionType.Index:
                    this.EmitIndexExpression(node);
                    break;

                case ExpressionType.Label:
                    this.EmitLabelExpression(node, flags);
                    break;

                case ExpressionType.RuntimeVariables:
                    this.EmitRuntimeVariablesExpression(node);
                    break;

                case ExpressionType.Loop:
                    this.EmitLoopExpression(node);
                    break;

                case ExpressionType.Switch:
                    this.EmitSwitchExpression(node, flags);
                    break;

                case ExpressionType.Throw:
                    this.EmitThrowUnaryExpression(node);
                    break;

                case ExpressionType.Try:
                    this.EmitTryExpression(node);
                    break;

                case ExpressionType.Unbox:
                    this.EmitUnboxUnaryExpression(node);
                    break;

                case ExpressionType.TypeEqual:
                    this.EmitTypeBinaryExpression(node);
                    break;

                case ExpressionType.OnesComplement:
                    this.EmitUnaryExpression(node, flags);
                    break;

                case ExpressionType.IsTrue:
                    this.EmitUnaryExpression(node, flags);
                    break;

                case ExpressionType.IsFalse:
                    this.EmitUnaryExpression(node, flags);
                    break;

                default:
                    throw ContractUtils.Unreachable;
            }
            if (flag)
            {
                this.EmitExpressionEnd(flags2);
            }
        }

        private void EmitExpressionAddress(Expression node, Type type)
        {
            this.EmitExpression(node, CompilationFlags.EmitAsNoTail | CompilationFlags.EmitNoExpressionStart);
            LocalBuilder local = this.GetLocal(type);
            this._ilg.Emit(OpCodes.Stloc, local);
            this._ilg.Emit(OpCodes.Ldloca, local);
        }

        private void EmitExpressionAndBranch(bool branchValue, Expression node, Label label)
        {
            CompilationFlags flags = this.EmitExpressionStart(node);
            try
            {
                if (node.Type == typeof(bool))
                {
                    switch (node.NodeType)
                    {
                        case ExpressionType.Not:
                            this.EmitBranchNot(branchValue, (UnaryExpression) node, label);
                            return;

                        case ExpressionType.NotEqual:
                        case ExpressionType.Equal:
                            goto Label_0086;

                        case ExpressionType.OrElse:
                        case ExpressionType.AndAlso:
                            goto Label_0066;

                        case ExpressionType.Block:
                            goto Label_0076;
                    }
                }
                goto Label_0096;
            Label_0066:
                this.EmitBranchLogical(branchValue, (BinaryExpression) node, label);
                return;
            Label_0076:
                this.EmitBranchBlock(branchValue, (BlockExpression) node, label);
                return;
            Label_0086:
                this.EmitBranchComparison(branchValue, (BinaryExpression) node, label);
                return;
            Label_0096:
                this.EmitExpression(node, CompilationFlags.EmitAsNoTail | CompilationFlags.EmitNoExpressionStart);
                this.EmitBranchOp(branchValue, label);
            }
            finally
            {
                this.EmitExpressionEnd(flags);
            }
        }

        private void EmitExpressionAsType(Expression node, Type type, CompilationFlags flags)
        {
            if (type == typeof(void))
            {
                this.EmitExpressionAsVoid(node, flags);
            }
            else if (!TypeUtils.AreEquivalent(node.Type, type))
            {
                this.EmitExpression(node);
                this._ilg.Emit(OpCodes.Castclass, type);
            }
            else
            {
                this.EmitExpression(node, UpdateEmitExpressionStartFlag(flags, CompilationFlags.EmitExpressionStart));
            }
        }

        private void EmitExpressionAsVoid(Expression node)
        {
            this.EmitExpressionAsVoid(node, CompilationFlags.EmitAsNoTail);
        }

        private void EmitExpressionAsVoid(Expression node, CompilationFlags flags)
        {
            CompilationFlags flags2 = this.EmitExpressionStart(node);
            switch (node.NodeType)
            {
                case ExpressionType.Constant:
                case ExpressionType.Parameter:
                case ExpressionType.Default:
                    break;

                case ExpressionType.Assign:
                    this.EmitAssign((BinaryExpression) node, CompilationFlags.EmitAsVoidType);
                    break;

                case ExpressionType.Block:
                    this.Emit((BlockExpression) node, UpdateEmitAsTypeFlag(flags, CompilationFlags.EmitAsVoidType));
                    break;

                case ExpressionType.Goto:
                    this.EmitGotoExpression(node, UpdateEmitAsTypeFlag(flags, CompilationFlags.EmitAsVoidType));
                    break;

                case ExpressionType.Throw:
                    this.EmitThrow((UnaryExpression) node, CompilationFlags.EmitAsVoidType);
                    break;

                default:
                    if (node.Type == typeof(void))
                    {
                        this.EmitExpression(node, UpdateEmitExpressionStartFlag(flags, CompilationFlags.EmitNoExpressionStart));
                    }
                    else
                    {
                        this.EmitExpression(node, CompilationFlags.EmitAsNoTail | CompilationFlags.EmitNoExpressionStart);
                        this._ilg.Emit(OpCodes.Pop);
                    }
                    break;
            }
            this.EmitExpressionEnd(flags2);
        }

        private void EmitExpressionEnd(CompilationFlags flags)
        {
            if ((flags & CompilationFlags.EmitExpressionStartMask) == CompilationFlags.EmitExpressionStart)
            {
                this.PopLabelBlock(this._labelBlock.Kind);
            }
        }

        private CompilationFlags EmitExpressionStart(Expression node)
        {
            if (this.TryPushLabelBlock(node))
            {
                return CompilationFlags.EmitExpressionStart;
            }
            return CompilationFlags.EmitNoExpressionStart;
        }

        private static void EmitExtensionExpression(Expression expr)
        {
            throw System.Linq.Expressions.Error.ExtensionNotReduced();
        }

        private void EmitGetIndexCall(IndexExpression node, Type objectType)
        {
            if (node.Indexer != null)
            {
                MethodInfo getMethod = node.Indexer.GetGetMethod(true);
                this.EmitCall(objectType, getMethod);
            }
            else if (node.Arguments.Count != 1)
            {
                this._ilg.Emit(OpCodes.Call, node.Object.Type.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance));
            }
            else
            {
                this._ilg.EmitLoadElement(node.Type);
            }
        }

        private void EmitGotoExpression(Expression expr, CompilationFlags flags)
        {
            GotoExpression node = (GotoExpression) expr;
            LabelInfo info = this.ReferenceLabel(node.Target);
            CompilationFlags newValue = flags & CompilationFlags.EmitAsTailCallMask;
            if (newValue != CompilationFlags.EmitAsNoTail)
            {
                newValue = info.CanReturn ? CompilationFlags.EmitAsTail : CompilationFlags.EmitAsNoTail;
                flags = UpdateEmitAsTailCallFlag(flags, newValue);
            }
            if (node.Value != null)
            {
                if (node.Target.Type == typeof(void))
                {
                    this.EmitExpressionAsVoid(node.Value, flags);
                }
                else
                {
                    flags = UpdateEmitExpressionStartFlag(flags, CompilationFlags.EmitExpressionStart);
                    this.EmitExpression(node.Value, flags);
                }
            }
            info.EmitJump();
            this.EmitUnreachable(node, flags);
        }

        private void EmitIndexAssignment(BinaryExpression node, CompilationFlags flags)
        {
            IndexExpression left = (IndexExpression) node.Left;
            CompilationFlags flags2 = flags & CompilationFlags.EmitAsTypeMask;
            Type objectType = null;
            if (left.Object != null)
            {
                this.EmitInstance(left.Object, objectType = left.Object.Type);
            }
            foreach (Expression expression2 in left.Arguments)
            {
                this.EmitExpression(expression2);
            }
            this.EmitExpression(node.Right);
            LocalBuilder local = null;
            if (flags2 != CompilationFlags.EmitAsVoidType)
            {
                this._ilg.Emit(OpCodes.Dup);
                this._ilg.Emit(OpCodes.Stloc, local = this.GetLocal(node.Type));
            }
            this.EmitSetIndexCall(left, objectType);
            if (flags2 != CompilationFlags.EmitAsVoidType)
            {
                this._ilg.Emit(OpCodes.Ldloc, local);
                this.FreeLocal(local);
            }
        }

        private void EmitIndexExpression(Expression expr)
        {
            IndexExpression node = (IndexExpression) expr;
            Type objectType = null;
            if (node.Object != null)
            {
                this.EmitInstance(node.Object, objectType = node.Object.Type);
            }
            foreach (Expression expression2 in node.Arguments)
            {
                this.EmitExpression(expression2);
            }
            this.EmitGetIndexCall(node, objectType);
        }

        private void EmitInlinedInvoke(InvocationExpression invoke, CompilationFlags flags)
        {
            LambdaExpression lambdaOperand = invoke.LambdaOperand;
            List<WriteBack> writeBacks = this.EmitArguments(lambdaOperand.Type.GetMethod("Invoke"), invoke);
            LambdaCompiler compiler = new LambdaCompiler(this, lambdaOperand);
            if (writeBacks.Count != 0)
            {
                flags = UpdateEmitAsTailCallFlag(flags, CompilationFlags.EmitAsNoTail);
            }
            compiler.EmitLambdaBody(this._scope, true, flags);
            EmitWriteBack(writeBacks);
        }

        private void EmitInstance(Expression instance, Type type)
        {
            if (instance != null)
            {
                if (type.IsValueType)
                {
                    this.EmitAddress(instance, type);
                }
                else
                {
                    this.EmitExpression(instance);
                }
            }
        }

        private void EmitInvocationExpression(Expression expr, CompilationFlags flags)
        {
            InvocationExpression invoke = (InvocationExpression) expr;
            if (invoke.LambdaOperand != null)
            {
                this.EmitInlinedInvoke(invoke, flags);
            }
            else
            {
                expr = invoke.Expression;
                if (typeof(LambdaExpression).IsAssignableFrom(expr.Type))
                {
                    expr = Expression.Call(expr, expr.Type.GetMethod("Compile", new Type[0]));
                }
                expr = Expression.Call(expr, expr.Type.GetMethod("Invoke"), invoke.Arguments);
                this.EmitExpression(expr);
            }
        }

        private void EmitLabelExpression(Expression expr, CompilationFlags flags)
        {
            LabelExpression expression = (LabelExpression) expr;
            LabelInfo info = null;
            if (this._labelBlock.Kind == LabelScopeKind.Block)
            {
                this._labelBlock.TryGetLabelInfo(expression.Target, out info);
                if ((info == null) && (this._labelBlock.Parent.Kind == LabelScopeKind.Switch))
                {
                    this._labelBlock.Parent.TryGetLabelInfo(expression.Target, out info);
                }
            }
            if (info == null)
            {
                info = this.DefineLabel(expression.Target);
            }
            if (expression.DefaultValue != null)
            {
                if (expression.Target.Type == typeof(void))
                {
                    this.EmitExpressionAsVoid(expression.DefaultValue, flags);
                }
                else
                {
                    flags = UpdateEmitExpressionStartFlag(flags, CompilationFlags.EmitExpressionStart);
                    this.EmitExpression(expression.DefaultValue, flags);
                }
            }
            info.Mark();
        }

        internal void EmitLambdaArgument(int index)
        {
            this._ilg.EmitLoadArg(this.GetLambdaArgument(index));
        }

        private void EmitLambdaBody()
        {
            CompilationFlags flags = this._lambda.TailCall ? CompilationFlags.EmitAsTail : CompilationFlags.EmitAsNoTail;
            this.EmitLambdaBody(null, false, flags);
        }

        private void EmitLambdaBody(CompilerScope parent, bool inlined, CompilationFlags flags)
        {
            this._scope.Enter(this, parent);
            if (inlined)
            {
                for (int i = this._lambda.Parameters.Count - 1; i >= 0; i--)
                {
                    this._scope.EmitSet(this._lambda.Parameters[i]);
                }
            }
            flags = UpdateEmitExpressionStartFlag(flags, CompilationFlags.EmitExpressionStart);
            if (this._lambda.ReturnType == typeof(void))
            {
                this.EmitExpressionAsVoid(this._lambda.Body, flags);
            }
            else
            {
                this.EmitExpression(this._lambda.Body, flags);
            }
            if (!inlined)
            {
                this._ilg.Emit(OpCodes.Ret);
            }
            this._scope.Exit();
            foreach (LabelInfo info in this._labelInfo.Values)
            {
                info.ValidateFinish();
            }
        }

        private void EmitLambdaExpression(Expression expr)
        {
            LambdaExpression lambda = (LambdaExpression) expr;
            this.EmitDelegateConstruction(lambda);
        }

        private void EmitLambdaReferenceCoalesce(BinaryExpression b)
        {
            LocalBuilder local = this.GetLocal(b.Left.Type);
            Label label = this._ilg.DefineLabel();
            Label label2 = this._ilg.DefineLabel();
            this.EmitExpression(b.Left);
            this._ilg.Emit(OpCodes.Dup);
            this._ilg.Emit(OpCodes.Stloc, local);
            this._ilg.Emit(OpCodes.Ldnull);
            this._ilg.Emit(OpCodes.Ceq);
            this._ilg.Emit(OpCodes.Brfalse, label2);
            this.EmitExpression(b.Right);
            this._ilg.Emit(OpCodes.Br, label);
            this._ilg.MarkLabel(label2);
            ParameterExpression local1 = b.Conversion.Parameters[0];
            this.EmitLambdaExpression(b.Conversion);
            this._ilg.Emit(OpCodes.Ldloc, local);
            this.FreeLocal(local);
            this._ilg.Emit(OpCodes.Callvirt, b.Conversion.Type.GetMethod("Invoke"));
            this._ilg.MarkLabel(label);
        }

        private void EmitLift(ExpressionType nodeType, Type resultType, MethodCallExpression mc, ParameterExpression[] paramList, Expression[] argList)
        {
            switch (nodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                {
                    if (TypeUtils.AreEquivalent(resultType, TypeUtils.GetNullableType(mc.Type)))
                    {
                        break;
                    }
                    Label label3 = this._ilg.DefineLabel();
                    Label label4 = this._ilg.DefineLabel();
                    Label label5 = this._ilg.DefineLabel();
                    LocalBuilder builder3 = this._ilg.DeclareLocal(typeof(bool));
                    LocalBuilder builder4 = this._ilg.DeclareLocal(typeof(bool));
                    this._ilg.Emit(OpCodes.Ldc_I4_0);
                    this._ilg.Emit(OpCodes.Stloc, builder3);
                    this._ilg.Emit(OpCodes.Ldc_I4_1);
                    this._ilg.Emit(OpCodes.Stloc, builder4);
                    int num3 = 0;
                    int num4 = paramList.Length;
                    while (num3 < num4)
                    {
                        ParameterExpression variable = paramList[num3];
                        Expression node = argList[num3];
                        this._scope.AddLocal(this, variable);
                        if (node.Type.IsNullableType())
                        {
                            this.EmitAddress(node, node.Type);
                            this._ilg.Emit(OpCodes.Dup);
                            this._ilg.EmitHasValue(node.Type);
                            this._ilg.Emit(OpCodes.Ldc_I4_0);
                            this._ilg.Emit(OpCodes.Ceq);
                            this._ilg.Emit(OpCodes.Dup);
                            this._ilg.Emit(OpCodes.Ldloc, builder3);
                            this._ilg.Emit(OpCodes.Or);
                            this._ilg.Emit(OpCodes.Stloc, builder3);
                            this._ilg.Emit(OpCodes.Ldloc, builder4);
                            this._ilg.Emit(OpCodes.And);
                            this._ilg.Emit(OpCodes.Stloc, builder4);
                            this._ilg.EmitGetValueOrDefault(node.Type);
                        }
                        else
                        {
                            this.EmitExpression(node);
                            if (!node.Type.IsValueType)
                            {
                                this._ilg.Emit(OpCodes.Dup);
                                this._ilg.Emit(OpCodes.Ldnull);
                                this._ilg.Emit(OpCodes.Ceq);
                                this._ilg.Emit(OpCodes.Dup);
                                this._ilg.Emit(OpCodes.Ldloc, builder3);
                                this._ilg.Emit(OpCodes.Or);
                                this._ilg.Emit(OpCodes.Stloc, builder3);
                                this._ilg.Emit(OpCodes.Ldloc, builder4);
                                this._ilg.Emit(OpCodes.And);
                                this._ilg.Emit(OpCodes.Stloc, builder4);
                            }
                            else
                            {
                                this._ilg.Emit(OpCodes.Ldc_I4_0);
                                this._ilg.Emit(OpCodes.Stloc, builder4);
                            }
                        }
                        this._scope.EmitSet(variable);
                        num3++;
                    }
                    this._ilg.Emit(OpCodes.Ldloc, builder4);
                    this._ilg.Emit(OpCodes.Brtrue, label4);
                    this._ilg.Emit(OpCodes.Ldloc, builder3);
                    this._ilg.Emit(OpCodes.Brtrue, label5);
                    this.EmitMethodCallExpression(mc);
                    if (resultType.IsNullableType() && !TypeUtils.AreEquivalent(resultType, mc.Type))
                    {
                        ConstructorInfo constructor = resultType.GetConstructor(new Type[] { mc.Type });
                        this._ilg.Emit(OpCodes.Newobj, constructor);
                    }
                    this._ilg.Emit(OpCodes.Br_S, label3);
                    this._ilg.MarkLabel(label4);
                    this._ilg.EmitBoolean(nodeType == ExpressionType.Equal);
                    this._ilg.Emit(OpCodes.Br_S, label3);
                    this._ilg.MarkLabel(label5);
                    this._ilg.EmitBoolean(nodeType == ExpressionType.NotEqual);
                    this._ilg.MarkLabel(label3);
                    return;
                }
            }
            Label label = this._ilg.DefineLabel();
            Label label2 = this._ilg.DefineLabel();
            LocalBuilder local = this._ilg.DeclareLocal(typeof(bool));
            int index = 0;
            int length = paramList.Length;
            while (index < length)
            {
                ParameterExpression expression = paramList[index];
                Expression expression2 = argList[index];
                if (expression2.Type.IsNullableType())
                {
                    this._scope.AddLocal(this, expression);
                    this.EmitAddress(expression2, expression2.Type);
                    this._ilg.Emit(OpCodes.Dup);
                    this._ilg.EmitHasValue(expression2.Type);
                    this._ilg.Emit(OpCodes.Ldc_I4_0);
                    this._ilg.Emit(OpCodes.Ceq);
                    this._ilg.Emit(OpCodes.Stloc, local);
                    this._ilg.EmitGetValueOrDefault(expression2.Type);
                    this._scope.EmitSet(expression);
                }
                else
                {
                    this._scope.AddLocal(this, expression);
                    this.EmitExpression(expression2);
                    if (!expression2.Type.IsValueType)
                    {
                        this._ilg.Emit(OpCodes.Dup);
                        this._ilg.Emit(OpCodes.Ldnull);
                        this._ilg.Emit(OpCodes.Ceq);
                        this._ilg.Emit(OpCodes.Stloc, local);
                    }
                    this._scope.EmitSet(expression);
                }
                this._ilg.Emit(OpCodes.Ldloc, local);
                this._ilg.Emit(OpCodes.Brtrue, label2);
                index++;
            }
            this.EmitMethodCallExpression(mc);
            if (resultType.IsNullableType() && !TypeUtils.AreEquivalent(resultType, mc.Type))
            {
                ConstructorInfo con = resultType.GetConstructor(new Type[] { mc.Type });
                this._ilg.Emit(OpCodes.Newobj, con);
            }
            this._ilg.Emit(OpCodes.Br_S, label);
            this._ilg.MarkLabel(label2);
            if (TypeUtils.AreEquivalent(resultType, TypeUtils.GetNullableType(mc.Type)))
            {
                if (resultType.IsValueType)
                {
                    LocalBuilder builder2 = this.GetLocal(resultType);
                    this._ilg.Emit(OpCodes.Ldloca, builder2);
                    this._ilg.Emit(OpCodes.Initobj, resultType);
                    this._ilg.Emit(OpCodes.Ldloc, builder2);
                    this.FreeLocal(builder2);
                }
                else
                {
                    this._ilg.Emit(OpCodes.Ldnull);
                }
            }
            else
            {
                switch (nodeType)
                {
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                        this._ilg.Emit(OpCodes.Ldc_I4_0);
                        goto Label_02E9;
                }
                throw System.Linq.Expressions.Error.UnknownLiftType(nodeType);
            }
        Label_02E9:
            this._ilg.MarkLabel(label);
        }

        private void EmitLiftedAndAlso(BinaryExpression b)
        {
            Type type = typeof(bool?);
            Label label = this._ilg.DefineLabel();
            Label label2 = this._ilg.DefineLabel();
            Label label3 = this._ilg.DefineLabel();
            Label label4 = this._ilg.DefineLabel();
            Label label5 = this._ilg.DefineLabel();
            LocalBuilder local = this.GetLocal(type);
            LocalBuilder builder2 = this.GetLocal(type);
            this.EmitExpression(b.Left);
            this._ilg.Emit(OpCodes.Stloc, local);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.EmitHasValue(type);
            this._ilg.Emit(OpCodes.Brfalse, label);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.EmitGetValueOrDefault(type);
            this._ilg.Emit(OpCodes.Ldc_I4_0);
            this._ilg.Emit(OpCodes.Ceq);
            this._ilg.Emit(OpCodes.Brtrue, label2);
            this._ilg.MarkLabel(label);
            this.EmitExpression(b.Right);
            this._ilg.Emit(OpCodes.Stloc, builder2);
            this._ilg.Emit(OpCodes.Ldloca, builder2);
            this._ilg.EmitHasValue(type);
            this._ilg.Emit(OpCodes.Brfalse_S, label3);
            this._ilg.Emit(OpCodes.Ldloca, builder2);
            this._ilg.EmitGetValueOrDefault(type);
            this._ilg.Emit(OpCodes.Ldc_I4_0);
            this._ilg.Emit(OpCodes.Ceq);
            this._ilg.Emit(OpCodes.Brtrue_S, label2);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.EmitHasValue(type);
            this._ilg.Emit(OpCodes.Brfalse, label3);
            this._ilg.Emit(OpCodes.Ldc_I4_1);
            this._ilg.Emit(OpCodes.Br_S, label4);
            this._ilg.MarkLabel(label2);
            this._ilg.Emit(OpCodes.Ldc_I4_0);
            this._ilg.Emit(OpCodes.Br_S, label4);
            this._ilg.MarkLabel(label4);
            ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(bool) });
            this._ilg.Emit(OpCodes.Newobj, constructor);
            this._ilg.Emit(OpCodes.Stloc, local);
            this._ilg.Emit(OpCodes.Br, label5);
            this._ilg.MarkLabel(label3);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.Emit(OpCodes.Initobj, type);
            this._ilg.MarkLabel(label5);
            this._ilg.Emit(OpCodes.Ldloc, local);
            this.FreeLocal(local);
            this.FreeLocal(builder2);
        }

        private void EmitLiftedBinaryArithmetic(ExpressionType op, Type leftType, Type rightType, Type resultType)
        {
            bool flag = leftType.IsNullableType();
            bool flag2 = rightType.IsNullableType();
            Label label = this._ilg.DefineLabel();
            Label label2 = this._ilg.DefineLabel();
            LocalBuilder local = this.GetLocal(leftType);
            LocalBuilder builder2 = this.GetLocal(rightType);
            LocalBuilder builder3 = this.GetLocal(resultType);
            this._ilg.Emit(OpCodes.Stloc, builder2);
            this._ilg.Emit(OpCodes.Stloc, local);
            if (flag)
            {
                this._ilg.Emit(OpCodes.Ldloca, local);
                this._ilg.EmitHasValue(leftType);
                this._ilg.Emit(OpCodes.Brfalse_S, label);
            }
            if (flag2)
            {
                this._ilg.Emit(OpCodes.Ldloca, builder2);
                this._ilg.EmitHasValue(rightType);
                this._ilg.Emit(OpCodes.Brfalse_S, label);
            }
            if (flag)
            {
                this._ilg.Emit(OpCodes.Ldloca, local);
                this._ilg.EmitGetValueOrDefault(leftType);
            }
            else
            {
                this._ilg.Emit(OpCodes.Ldloc, local);
            }
            if (flag2)
            {
                this._ilg.Emit(OpCodes.Ldloca, builder2);
                this._ilg.EmitGetValueOrDefault(rightType);
            }
            else
            {
                this._ilg.Emit(OpCodes.Ldloc, builder2);
            }
            this.FreeLocal(local);
            this.FreeLocal(builder2);
            this.EmitBinaryOperator(op, leftType.GetNonNullableType(), rightType.GetNonNullableType(), resultType.GetNonNullableType(), false);
            ConstructorInfo constructor = resultType.GetConstructor(new Type[] { resultType.GetNonNullableType() });
            this._ilg.Emit(OpCodes.Newobj, constructor);
            this._ilg.Emit(OpCodes.Stloc, builder3);
            this._ilg.Emit(OpCodes.Br_S, label2);
            this._ilg.MarkLabel(label);
            this._ilg.Emit(OpCodes.Ldloca, builder3);
            this._ilg.Emit(OpCodes.Initobj, resultType);
            this._ilg.MarkLabel(label2);
            this._ilg.Emit(OpCodes.Ldloc, builder3);
            this.FreeLocal(builder3);
        }

        private void EmitLiftedBinaryOp(ExpressionType op, Type leftType, Type rightType, Type resultType, bool liftedToNull)
        {
            switch (op)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Divide:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.LeftShift:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    this.EmitLiftedBinaryArithmetic(op, leftType, rightType, resultType);
                    return;

                case ExpressionType.And:
                    if (!(leftType == typeof(bool?)))
                    {
                        this.EmitLiftedBinaryArithmetic(op, leftType, rightType, resultType);
                        return;
                    }
                    this.EmitLiftedBooleanAnd();
                    return;

                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                    this.EmitLiftedRelational(op, leftType, rightType, resultType, liftedToNull);
                    return;

                case ExpressionType.Or:
                    if (!(leftType == typeof(bool?)))
                    {
                        this.EmitLiftedBinaryArithmetic(op, leftType, rightType, resultType);
                        return;
                    }
                    this.EmitLiftedBooleanOr();
                    return;
            }
            throw ContractUtils.Unreachable;
        }

        private void EmitLiftedBooleanAnd()
        {
            Type type = typeof(bool?);
            Label label = this._ilg.DefineLabel();
            Label label2 = this._ilg.DefineLabel();
            Label label3 = this._ilg.DefineLabel();
            Label label4 = this._ilg.DefineLabel();
            Label label5 = this._ilg.DefineLabel();
            LocalBuilder local = this.GetLocal(type);
            LocalBuilder builder2 = this.GetLocal(type);
            this._ilg.Emit(OpCodes.Stloc, builder2);
            this._ilg.Emit(OpCodes.Stloc, local);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.EmitHasValue(type);
            this._ilg.Emit(OpCodes.Brfalse, label);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.EmitGetValueOrDefault(type);
            this._ilg.Emit(OpCodes.Ldc_I4_0);
            this._ilg.Emit(OpCodes.Ceq);
            this._ilg.Emit(OpCodes.Brtrue, label2);
            this._ilg.MarkLabel(label);
            this._ilg.Emit(OpCodes.Ldloca, builder2);
            this._ilg.EmitHasValue(type);
            this._ilg.Emit(OpCodes.Brfalse_S, label3);
            this._ilg.Emit(OpCodes.Ldloca, builder2);
            this.FreeLocal(builder2);
            this._ilg.EmitGetValueOrDefault(type);
            this._ilg.Emit(OpCodes.Ldc_I4_0);
            this._ilg.Emit(OpCodes.Ceq);
            this._ilg.Emit(OpCodes.Brtrue_S, label2);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.EmitHasValue(type);
            this._ilg.Emit(OpCodes.Brfalse, label3);
            this._ilg.Emit(OpCodes.Ldc_I4_1);
            this._ilg.Emit(OpCodes.Br_S, label4);
            this._ilg.MarkLabel(label2);
            this._ilg.Emit(OpCodes.Ldc_I4_0);
            this._ilg.Emit(OpCodes.Br_S, label4);
            this._ilg.MarkLabel(label4);
            ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(bool) });
            this._ilg.Emit(OpCodes.Newobj, constructor);
            this._ilg.Emit(OpCodes.Stloc, local);
            this._ilg.Emit(OpCodes.Br, label5);
            this._ilg.MarkLabel(label3);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.Emit(OpCodes.Initobj, type);
            this._ilg.MarkLabel(label5);
            this._ilg.Emit(OpCodes.Ldloc, local);
            this.FreeLocal(local);
        }

        private void EmitLiftedBooleanOr()
        {
            Type type = typeof(bool?);
            Label label = this._ilg.DefineLabel();
            Label label2 = this._ilg.DefineLabel();
            Label label3 = this._ilg.DefineLabel();
            Label label4 = this._ilg.DefineLabel();
            Label label5 = this._ilg.DefineLabel();
            LocalBuilder local = this.GetLocal(type);
            LocalBuilder builder2 = this.GetLocal(type);
            this._ilg.Emit(OpCodes.Stloc, builder2);
            this._ilg.Emit(OpCodes.Stloc, local);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.EmitHasValue(type);
            this._ilg.Emit(OpCodes.Brfalse, label);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.EmitGetValueOrDefault(type);
            this._ilg.Emit(OpCodes.Ldc_I4_0);
            this._ilg.Emit(OpCodes.Ceq);
            this._ilg.Emit(OpCodes.Brfalse, label2);
            this._ilg.MarkLabel(label);
            this._ilg.Emit(OpCodes.Ldloca, builder2);
            this._ilg.EmitHasValue(type);
            this._ilg.Emit(OpCodes.Brfalse_S, label3);
            this._ilg.Emit(OpCodes.Ldloca, builder2);
            this.FreeLocal(builder2);
            this._ilg.EmitGetValueOrDefault(type);
            this._ilg.Emit(OpCodes.Ldc_I4_0);
            this._ilg.Emit(OpCodes.Ceq);
            this._ilg.Emit(OpCodes.Brfalse_S, label2);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.EmitHasValue(type);
            this._ilg.Emit(OpCodes.Brfalse, label3);
            this._ilg.Emit(OpCodes.Ldc_I4_0);
            this._ilg.Emit(OpCodes.Br_S, label4);
            this._ilg.MarkLabel(label2);
            this._ilg.Emit(OpCodes.Ldc_I4_1);
            this._ilg.Emit(OpCodes.Br_S, label4);
            this._ilg.MarkLabel(label4);
            ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(bool) });
            this._ilg.Emit(OpCodes.Newobj, constructor);
            this._ilg.Emit(OpCodes.Stloc, local);
            this._ilg.Emit(OpCodes.Br, label5);
            this._ilg.MarkLabel(label3);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.Emit(OpCodes.Initobj, type);
            this._ilg.MarkLabel(label5);
            this._ilg.Emit(OpCodes.Ldloc, local);
            this.FreeLocal(local);
        }

        private void EmitLiftedOrElse(BinaryExpression b)
        {
            Type type = typeof(bool?);
            Label label = this._ilg.DefineLabel();
            Label label2 = this._ilg.DefineLabel();
            Label label3 = this._ilg.DefineLabel();
            Label label4 = this._ilg.DefineLabel();
            Label label5 = this._ilg.DefineLabel();
            LocalBuilder local = this.GetLocal(type);
            LocalBuilder builder2 = this.GetLocal(type);
            this.EmitExpression(b.Left);
            this._ilg.Emit(OpCodes.Stloc, local);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.EmitHasValue(type);
            this._ilg.Emit(OpCodes.Brfalse, label);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.EmitGetValueOrDefault(type);
            this._ilg.Emit(OpCodes.Ldc_I4_0);
            this._ilg.Emit(OpCodes.Ceq);
            this._ilg.Emit(OpCodes.Brfalse, label2);
            this._ilg.MarkLabel(label);
            this.EmitExpression(b.Right);
            this._ilg.Emit(OpCodes.Stloc, builder2);
            this._ilg.Emit(OpCodes.Ldloca, builder2);
            this._ilg.EmitHasValue(type);
            this._ilg.Emit(OpCodes.Brfalse_S, label3);
            this._ilg.Emit(OpCodes.Ldloca, builder2);
            this._ilg.EmitGetValueOrDefault(type);
            this._ilg.Emit(OpCodes.Ldc_I4_0);
            this._ilg.Emit(OpCodes.Ceq);
            this._ilg.Emit(OpCodes.Brfalse_S, label2);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.EmitHasValue(type);
            this._ilg.Emit(OpCodes.Brfalse, label3);
            this._ilg.Emit(OpCodes.Ldc_I4_0);
            this._ilg.Emit(OpCodes.Br_S, label4);
            this._ilg.MarkLabel(label2);
            this._ilg.Emit(OpCodes.Ldc_I4_1);
            this._ilg.Emit(OpCodes.Br_S, label4);
            this._ilg.MarkLabel(label4);
            ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(bool) });
            this._ilg.Emit(OpCodes.Newobj, constructor);
            this._ilg.Emit(OpCodes.Stloc, local);
            this._ilg.Emit(OpCodes.Br, label5);
            this._ilg.MarkLabel(label3);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.Emit(OpCodes.Initobj, type);
            this._ilg.MarkLabel(label5);
            this._ilg.Emit(OpCodes.Ldloc, local);
            this.FreeLocal(local);
            this.FreeLocal(builder2);
        }

        private void EmitLiftedRelational(ExpressionType op, Type leftType, Type rightType, Type resultType, bool liftedToNull)
        {
            Label label = this._ilg.DefineLabel();
            LocalBuilder local = this.GetLocal(leftType);
            LocalBuilder builder2 = this.GetLocal(rightType);
            this._ilg.Emit(OpCodes.Stloc, builder2);
            this._ilg.Emit(OpCodes.Stloc, local);
            if (op == ExpressionType.Equal)
            {
                this._ilg.Emit(OpCodes.Ldloca, local);
                this._ilg.EmitHasValue(leftType);
                this._ilg.Emit(OpCodes.Ldc_I4_0);
                this._ilg.Emit(OpCodes.Ceq);
                this._ilg.Emit(OpCodes.Ldloca, builder2);
                this._ilg.EmitHasValue(rightType);
                this._ilg.Emit(OpCodes.Ldc_I4_0);
                this._ilg.Emit(OpCodes.Ceq);
                this._ilg.Emit(OpCodes.And);
                this._ilg.Emit(OpCodes.Dup);
                this._ilg.Emit(OpCodes.Brtrue_S, label);
                this._ilg.Emit(OpCodes.Pop);
                this._ilg.Emit(OpCodes.Ldloca, local);
                this._ilg.EmitHasValue(leftType);
                this._ilg.Emit(OpCodes.Ldloca, builder2);
                this._ilg.EmitHasValue(rightType);
                this._ilg.Emit(OpCodes.And);
                this._ilg.Emit(OpCodes.Dup);
                this._ilg.Emit(OpCodes.Brfalse_S, label);
                this._ilg.Emit(OpCodes.Pop);
            }
            else if (op == ExpressionType.NotEqual)
            {
                this._ilg.Emit(OpCodes.Ldloca, local);
                this._ilg.EmitHasValue(leftType);
                this._ilg.Emit(OpCodes.Ldloca, builder2);
                this._ilg.EmitHasValue(rightType);
                this._ilg.Emit(OpCodes.Or);
                this._ilg.Emit(OpCodes.Dup);
                this._ilg.Emit(OpCodes.Brfalse_S, label);
                this._ilg.Emit(OpCodes.Pop);
                this._ilg.Emit(OpCodes.Ldloca, local);
                this._ilg.EmitHasValue(leftType);
                this._ilg.Emit(OpCodes.Ldc_I4_0);
                this._ilg.Emit(OpCodes.Ceq);
                this._ilg.Emit(OpCodes.Ldloca, builder2);
                this._ilg.EmitHasValue(rightType);
                this._ilg.Emit(OpCodes.Ldc_I4_0);
                this._ilg.Emit(OpCodes.Ceq);
                this._ilg.Emit(OpCodes.Or);
                this._ilg.Emit(OpCodes.Dup);
                this._ilg.Emit(OpCodes.Brtrue_S, label);
                this._ilg.Emit(OpCodes.Pop);
            }
            else
            {
                this._ilg.Emit(OpCodes.Ldloca, local);
                this._ilg.EmitHasValue(leftType);
                this._ilg.Emit(OpCodes.Ldloca, builder2);
                this._ilg.EmitHasValue(rightType);
                this._ilg.Emit(OpCodes.And);
                this._ilg.Emit(OpCodes.Dup);
                this._ilg.Emit(OpCodes.Brfalse_S, label);
                this._ilg.Emit(OpCodes.Pop);
            }
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.EmitGetValueOrDefault(leftType);
            this._ilg.Emit(OpCodes.Ldloca, builder2);
            this._ilg.EmitGetValueOrDefault(rightType);
            this.FreeLocal(local);
            this.FreeLocal(builder2);
            this.EmitBinaryOperator(op, leftType.GetNonNullableType(), rightType.GetNonNullableType(), resultType.GetNonNullableType(), false);
            if (!liftedToNull)
            {
                this._ilg.MarkLabel(label);
            }
            if (!TypeUtils.AreEquivalent(resultType, resultType.GetNonNullableType()))
            {
                this._ilg.EmitConvertToType(resultType.GetNonNullableType(), resultType, true);
            }
            if (liftedToNull)
            {
                Label label2 = this._ilg.DefineLabel();
                this._ilg.Emit(OpCodes.Br, label2);
                this._ilg.MarkLabel(label);
                this._ilg.Emit(OpCodes.Pop);
                this._ilg.Emit(OpCodes.Ldnull);
                this._ilg.Emit(OpCodes.Unbox_Any, resultType);
                this._ilg.MarkLabel(label2);
            }
        }

        private void EmitListInit(ListInitExpression init)
        {
            this.EmitExpression(init.NewExpression);
            LocalBuilder local = null;
            if (init.NewExpression.Type.IsValueType)
            {
                local = this._ilg.DeclareLocal(init.NewExpression.Type);
                this._ilg.Emit(OpCodes.Stloc, local);
                this._ilg.Emit(OpCodes.Ldloca, local);
            }
            this.EmitListInit(init.Initializers, local == null, init.NewExpression.Type);
            if (local != null)
            {
                this._ilg.Emit(OpCodes.Ldloc, local);
            }
        }

        private void EmitListInit(ReadOnlyCollection<ElementInit> initializers, bool keepOnStack, Type objectType)
        {
            int count = initializers.Count;
            if (count == 0)
            {
                if (!keepOnStack)
                {
                    this._ilg.Emit(OpCodes.Pop);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    if (keepOnStack || (i < (count - 1)))
                    {
                        this._ilg.Emit(OpCodes.Dup);
                    }
                    this.EmitMethodCall(initializers[i].AddMethod, initializers[i], objectType);
                    if (initializers[i].AddMethod.ReturnType != typeof(void))
                    {
                        this._ilg.Emit(OpCodes.Pop);
                    }
                }
            }
        }

        private void EmitListInitExpression(Expression expr)
        {
            this.EmitListInit((ListInitExpression) expr);
        }

        private void EmitLoopExpression(Expression expr)
        {
            LoopExpression expression = (LoopExpression) expr;
            this.PushLabelBlock(LabelScopeKind.Statement);
            LabelInfo info = this.DefineLabel(expression.BreakLabel);
            LabelInfo info2 = this.DefineLabel(expression.ContinueLabel);
            info2.MarkWithEmptyStack();
            this.EmitExpressionAsVoid(expression.Body);
            this._ilg.Emit(OpCodes.Br, info2.Label);
            this.PopLabelBlock(LabelScopeKind.Statement);
            info.MarkWithEmptyStack();
        }

        private void EmitMemberAddress(MemberInfo member, Type objectType)
        {
            if (member.MemberType == MemberTypes.Field)
            {
                FieldInfo fi = (FieldInfo) member;
                if (!fi.IsLiteral && !fi.IsInitOnly)
                {
                    this._ilg.EmitFieldAddress(fi);
                    return;
                }
            }
            this.EmitMemberGet(member, objectType);
            LocalBuilder local = this.GetLocal(GetMemberType(member));
            this._ilg.Emit(OpCodes.Stloc, local);
            this._ilg.Emit(OpCodes.Ldloca, local);
        }

        private void EmitMemberAssignment(BinaryExpression node, CompilationFlags flags)
        {
            MemberExpression left = (MemberExpression) node.Left;
            MemberInfo member = left.Member;
            Type objectType = null;
            if (left.Expression != null)
            {
                this.EmitInstance(left.Expression, objectType = left.Expression.Type);
            }
            this.EmitExpression(node.Right);
            LocalBuilder local = null;
            CompilationFlags flags2 = flags & CompilationFlags.EmitAsTypeMask;
            if (flags2 != CompilationFlags.EmitAsVoidType)
            {
                this._ilg.Emit(OpCodes.Dup);
                this._ilg.Emit(OpCodes.Stloc, local = this.GetLocal(node.Type));
            }
            MemberTypes memberType = member.MemberType;
            if (memberType != MemberTypes.Field)
            {
                if (memberType != MemberTypes.Property)
                {
                    throw System.Linq.Expressions.Error.InvalidMemberType(member.MemberType);
                }
            }
            else
            {
                this._ilg.EmitFieldSet((FieldInfo) member);
                goto Label_00CF;
            }
            this.EmitCall(objectType, ((PropertyInfo) member).GetSetMethod(true));
        Label_00CF:
            if (flags2 != CompilationFlags.EmitAsVoidType)
            {
                this._ilg.Emit(OpCodes.Ldloc, local);
                this.FreeLocal(local);
            }
        }

        private void EmitMemberAssignment(MemberAssignment binding, Type objectType)
        {
            this.EmitExpression(binding.Expression);
            FieldInfo member = binding.Member as FieldInfo;
            if (member != null)
            {
                this._ilg.Emit(OpCodes.Stfld, member);
            }
            else
            {
                PropertyInfo info2 = binding.Member as PropertyInfo;
                if (info2 == null)
                {
                    throw System.Linq.Expressions.Error.UnhandledBinding();
                }
                this.EmitCall(objectType, info2.GetSetMethod(true));
            }
        }

        private void EmitMemberExpression(Expression expr)
        {
            MemberExpression expression = (MemberExpression) expr;
            Type objectType = null;
            if (expression.Expression != null)
            {
                this.EmitInstance(expression.Expression, objectType = expression.Expression.Type);
            }
            this.EmitMemberGet(expression.Member, objectType);
        }

        private void EmitMemberGet(MemberInfo member, Type objectType)
        {
            MemberTypes memberType = member.MemberType;
            if (memberType != MemberTypes.Field)
            {
                if (memberType != MemberTypes.Property)
                {
                    throw ContractUtils.Unreachable;
                }
            }
            else
            {
                FieldInfo fi = (FieldInfo) member;
                if (fi.IsLiteral)
                {
                    this.EmitConstant(fi.GetRawConstantValue(), fi.FieldType);
                    return;
                }
                this._ilg.EmitFieldGet(fi);
                return;
            }
            this.EmitCall(objectType, ((PropertyInfo) member).GetGetMethod(true));
        }

        private void EmitMemberInit(MemberInitExpression init)
        {
            this.EmitExpression(init.NewExpression);
            LocalBuilder local = null;
            if (init.NewExpression.Type.IsValueType && (init.Bindings.Count > 0))
            {
                local = this._ilg.DeclareLocal(init.NewExpression.Type);
                this._ilg.Emit(OpCodes.Stloc, local);
                this._ilg.Emit(OpCodes.Ldloca, local);
            }
            this.EmitMemberInit(init.Bindings, local == null, init.NewExpression.Type);
            if (local != null)
            {
                this._ilg.Emit(OpCodes.Ldloc, local);
            }
        }

        private void EmitMemberInit(ReadOnlyCollection<MemberBinding> bindings, bool keepOnStack, Type objectType)
        {
            int count = bindings.Count;
            if (count == 0)
            {
                if (!keepOnStack)
                {
                    this._ilg.Emit(OpCodes.Pop);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    if (keepOnStack || (i < (count - 1)))
                    {
                        this._ilg.Emit(OpCodes.Dup);
                    }
                    this.EmitBinding(bindings[i], objectType);
                }
            }
        }

        private void EmitMemberInitExpression(Expression expr)
        {
            this.EmitMemberInit((MemberInitExpression) expr);
        }

        private void EmitMemberListBinding(MemberListBinding binding)
        {
            Type memberType = GetMemberType(binding.Member);
            if ((binding.Member is PropertyInfo) && memberType.IsValueType)
            {
                throw System.Linq.Expressions.Error.CannotAutoInitializeValueTypeElementThroughProperty(binding.Member);
            }
            if (memberType.IsValueType)
            {
                this.EmitMemberAddress(binding.Member, binding.Member.DeclaringType);
            }
            else
            {
                this.EmitMemberGet(binding.Member, binding.Member.DeclaringType);
            }
            this.EmitListInit(binding.Initializers, false, memberType);
        }

        private void EmitMemberMemberBinding(MemberMemberBinding binding)
        {
            Type memberType = GetMemberType(binding.Member);
            if ((binding.Member is PropertyInfo) && memberType.IsValueType)
            {
                throw System.Linq.Expressions.Error.CannotAutoInitializeValueTypeMemberThroughProperty(binding.Member);
            }
            if (memberType.IsValueType)
            {
                this.EmitMemberAddress(binding.Member, binding.Member.DeclaringType);
            }
            else
            {
                this.EmitMemberGet(binding.Member, binding.Member.DeclaringType);
            }
            this.EmitMemberInit(binding.Bindings, false, memberType);
        }

        private void EmitMethodAndAlso(BinaryExpression b, CompilationFlags flags)
        {
            Label label = this._ilg.DefineLabel();
            this.EmitExpression(b.Left);
            this._ilg.Emit(OpCodes.Dup);
            MethodInfo booleanOperator = TypeUtils.GetBooleanOperator(b.Method.DeclaringType, "op_False");
            this._ilg.Emit(OpCodes.Call, booleanOperator);
            this._ilg.Emit(OpCodes.Brtrue, label);
            LocalBuilder local = this.GetLocal(b.Left.Type);
            this._ilg.Emit(OpCodes.Stloc, local);
            this.EmitExpression(b.Right);
            LocalBuilder builder2 = this.GetLocal(b.Right.Type);
            this._ilg.Emit(OpCodes.Stloc, builder2);
            this._ilg.Emit(OpCodes.Ldloc, local);
            this._ilg.Emit(OpCodes.Ldloc, builder2);
            if ((flags & CompilationFlags.EmitAsTailCallMask) == CompilationFlags.EmitAsTail)
            {
                this._ilg.Emit(OpCodes.Tailcall);
            }
            this._ilg.Emit(OpCodes.Call, b.Method);
            this.FreeLocal(local);
            this.FreeLocal(builder2);
            this._ilg.MarkLabel(label);
        }

        private void EmitMethodCall(Expression obj, MethodInfo method, IArgumentProvider methodCallExpr)
        {
            this.EmitMethodCall(obj, method, methodCallExpr, CompilationFlags.EmitAsNoTail);
        }

        private void EmitMethodCall(MethodInfo mi, IArgumentProvider args, Type objectType)
        {
            this.EmitMethodCall(mi, args, objectType, CompilationFlags.EmitAsNoTail);
        }

        private void EmitMethodCall(Expression obj, MethodInfo method, IArgumentProvider methodCallExpr, CompilationFlags flags)
        {
            Type objectType = null;
            if (!method.IsStatic)
            {
                this.EmitInstance(obj, objectType = obj.Type);
            }
            if ((obj != null) && obj.Type.IsValueType)
            {
                this.EmitMethodCall(method, methodCallExpr, objectType);
            }
            else
            {
                this.EmitMethodCall(method, methodCallExpr, objectType, flags);
            }
        }

        private void EmitMethodCall(MethodInfo mi, IArgumentProvider args, Type objectType, CompilationFlags flags)
        {
            List<WriteBack> writeBacks = this.EmitArguments(mi, args);
            OpCode opcode = UseVirtual(mi) ? OpCodes.Callvirt : OpCodes.Call;
            if ((opcode == OpCodes.Callvirt) && objectType.IsValueType)
            {
                this._ilg.Emit(OpCodes.Constrained, objectType);
            }
            if (((flags & CompilationFlags.EmitAsTailCallMask) == CompilationFlags.EmitAsTail) && !MethodHasByRefParameter(mi))
            {
                this._ilg.Emit(OpCodes.Tailcall);
            }
            if (mi.CallingConvention == CallingConventions.VarArgs)
            {
                this._ilg.EmitCall(opcode, mi, args.Map<Type>(a => a.Type));
            }
            else
            {
                this._ilg.Emit(opcode, mi);
            }
            EmitWriteBack(writeBacks);
        }

        private void EmitMethodCallExpression(Expression expr)
        {
            this.EmitMethodCallExpression(expr, CompilationFlags.EmitAsNoTail);
        }

        private void EmitMethodCallExpression(Expression expr, CompilationFlags flags)
        {
            MethodCallExpression methodCallExpr = (MethodCallExpression) expr;
            this.EmitMethodCall(methodCallExpr.Object, methodCallExpr.Method, methodCallExpr, flags);
        }

        private void EmitMethodOrElse(BinaryExpression b, CompilationFlags flags)
        {
            Label label = this._ilg.DefineLabel();
            this.EmitExpression(b.Left);
            this._ilg.Emit(OpCodes.Dup);
            MethodInfo booleanOperator = TypeUtils.GetBooleanOperator(b.Method.DeclaringType, "op_True");
            this._ilg.Emit(OpCodes.Call, booleanOperator);
            this._ilg.Emit(OpCodes.Brtrue, label);
            LocalBuilder local = this.GetLocal(b.Left.Type);
            this._ilg.Emit(OpCodes.Stloc, local);
            this.EmitExpression(b.Right);
            LocalBuilder builder2 = this.GetLocal(b.Right.Type);
            this._ilg.Emit(OpCodes.Stloc, builder2);
            this._ilg.Emit(OpCodes.Ldloc, local);
            this._ilg.Emit(OpCodes.Ldloc, builder2);
            if ((flags & CompilationFlags.EmitAsTailCallMask) == CompilationFlags.EmitAsTail)
            {
                this._ilg.Emit(OpCodes.Tailcall);
            }
            this._ilg.Emit(OpCodes.Call, b.Method);
            this.FreeLocal(local);
            this.FreeLocal(builder2);
            this._ilg.MarkLabel(label);
        }

        private void EmitNewArrayExpression(Expression expr)
        {
            Action<int> emit = null;
            NewArrayExpression node = (NewArrayExpression) expr;
            if (node.NodeType == ExpressionType.NewArrayInit)
            {
                if (emit == null)
                {
                    emit = index => this.EmitExpression(node.Expressions[index]);
                }
                this._ilg.EmitArray(node.Type.GetElementType(), node.Expressions.Count, emit);
            }
            else
            {
                ReadOnlyCollection<Expression> expressions = node.Expressions;
                for (int i = 0; i < expressions.Count; i++)
                {
                    Expression expression = expressions[i];
                    this.EmitExpression(expression);
                    this._ilg.EmitConvertToType(expression.Type, typeof(int), true);
                }
                this._ilg.EmitArray(node.Type);
            }
        }

        private void EmitNewExpression(Expression expr)
        {
            NewExpression args = (NewExpression) expr;
            if (args.Constructor != null)
            {
                List<WriteBack> writeBacks = this.EmitArguments(args.Constructor, args);
                this._ilg.Emit(OpCodes.Newobj, args.Constructor);
                EmitWriteBack(writeBacks);
            }
            else
            {
                LocalBuilder local = this.GetLocal(args.Type);
                this._ilg.Emit(OpCodes.Ldloca, local);
                this._ilg.Emit(OpCodes.Initobj, args.Type);
                this._ilg.Emit(OpCodes.Ldloc, local);
                this.FreeLocal(local);
            }
        }

        private void EmitNullableCoalesce(BinaryExpression b)
        {
            LocalBuilder local = this.GetLocal(b.Left.Type);
            Label label = this._ilg.DefineLabel();
            Label label2 = this._ilg.DefineLabel();
            this.EmitExpression(b.Left);
            this._ilg.Emit(OpCodes.Stloc, local);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.EmitHasValue(b.Left.Type);
            this._ilg.Emit(OpCodes.Brfalse, label);
            Type nonNullableType = b.Left.Type.GetNonNullableType();
            if (b.Conversion != null)
            {
                ParameterExpression expression = b.Conversion.Parameters[0];
                this.EmitLambdaExpression(b.Conversion);
                if (!expression.Type.IsAssignableFrom(b.Left.Type))
                {
                    this._ilg.Emit(OpCodes.Ldloca, local);
                    this._ilg.EmitGetValueOrDefault(b.Left.Type);
                }
                else
                {
                    this._ilg.Emit(OpCodes.Ldloc, local);
                }
                this._ilg.Emit(OpCodes.Callvirt, b.Conversion.Type.GetMethod("Invoke"));
            }
            else if (!TypeUtils.AreEquivalent(b.Type, nonNullableType))
            {
                this._ilg.Emit(OpCodes.Ldloca, local);
                this._ilg.EmitGetValueOrDefault(b.Left.Type);
                this._ilg.EmitConvertToType(nonNullableType, b.Type, true);
            }
            else
            {
                this._ilg.Emit(OpCodes.Ldloca, local);
                this._ilg.EmitGetValueOrDefault(b.Left.Type);
            }
            this.FreeLocal(local);
            this._ilg.Emit(OpCodes.Br, label2);
            this._ilg.MarkLabel(label);
            this.EmitExpression(b.Right);
            if (!TypeUtils.AreEquivalent(b.Right.Type, b.Type))
            {
                this._ilg.EmitConvertToType(b.Right.Type, b.Type, true);
            }
            this._ilg.MarkLabel(label2);
        }

        private void EmitNullEquality(ExpressionType op, Expression e, bool isLiftedToNull)
        {
            if (isLiftedToNull)
            {
                this.EmitExpressionAsVoid(e);
                this._ilg.EmitDefault(typeof(bool?));
            }
            else
            {
                this.EmitAddress(e, e.Type);
                this._ilg.EmitHasValue(e.Type);
                if (op == ExpressionType.Equal)
                {
                    this._ilg.Emit(OpCodes.Ldc_I4_0);
                    this._ilg.Emit(OpCodes.Ceq);
                }
            }
        }

        private void EmitOrElseBinaryExpression(Expression expr, CompilationFlags flags)
        {
            BinaryExpression b = (BinaryExpression) expr;
            if ((b.Method != null) && !b.IsLiftedLogical)
            {
                this.EmitMethodOrElse(b, flags);
            }
            else if (b.Left.Type == typeof(bool?))
            {
                this.EmitLiftedOrElse(b);
            }
            else if (b.IsLiftedLogical)
            {
                this.EmitExpression(b.ReduceUserdefinedLifted());
            }
            else
            {
                this.EmitUnliftedOrElse(b);
            }
        }

        private void EmitParameterExpression(Expression expr)
        {
            ParameterExpression variable = (ParameterExpression) expr;
            this._scope.EmitGet(variable);
            if (variable.IsByRef)
            {
                this._ilg.EmitLoadValueIndirect(variable.Type);
            }
        }

        private void EmitQuote(UnaryExpression quote)
        {
            this.EmitConstant(quote.Operand, quote.Type);
            if (this._scope.NearestHoistedLocals != null)
            {
                this.EmitConstant(this._scope.NearestHoistedLocals, typeof(object));
                this._scope.EmitGet(this._scope.NearestHoistedLocals.SelfVariable);
                this._ilg.Emit(OpCodes.Call, typeof(RuntimeOps).GetMethod("Quote"));
                if (quote.Type != typeof(Expression))
                {
                    this._ilg.Emit(OpCodes.Castclass, quote.Type);
                }
            }
        }

        private void EmitQuoteUnaryExpression(Expression expr)
        {
            this.EmitQuote((UnaryExpression) expr);
        }

        private void EmitReferenceCoalesceWithoutConversion(BinaryExpression b)
        {
            Label label = this._ilg.DefineLabel();
            Label label2 = this._ilg.DefineLabel();
            this.EmitExpression(b.Left);
            this._ilg.Emit(OpCodes.Dup);
            this._ilg.Emit(OpCodes.Ldnull);
            this._ilg.Emit(OpCodes.Ceq);
            this._ilg.Emit(OpCodes.Brfalse, label2);
            this._ilg.Emit(OpCodes.Pop);
            this.EmitExpression(b.Right);
            if (!TypeUtils.AreEquivalent(b.Right.Type, b.Type))
            {
                if (b.Right.Type.IsValueType)
                {
                    this._ilg.Emit(OpCodes.Box, b.Right.Type);
                }
                this._ilg.Emit(OpCodes.Castclass, b.Type);
            }
            this._ilg.Emit(OpCodes.Br_S, label);
            this._ilg.MarkLabel(label2);
            if (!TypeUtils.AreEquivalent(b.Left.Type, b.Type))
            {
                this._ilg.Emit(OpCodes.Castclass, b.Type);
            }
            this._ilg.MarkLabel(label);
        }

        private void EmitRuntimeVariablesExpression(Expression expr)
        {
            RuntimeVariablesExpression expression = (RuntimeVariablesExpression) expr;
            this._scope.EmitVariableAccess(this, expression.Variables);
        }

        private void EmitSaveExceptionOrPop(CatchBlock cb)
        {
            if (cb.Variable != null)
            {
                this._scope.EmitSet(cb.Variable);
            }
            else
            {
                this._ilg.Emit(OpCodes.Pop);
            }
        }

        private void EmitSetIndexCall(IndexExpression node, Type objectType)
        {
            if (node.Indexer != null)
            {
                MethodInfo setMethod = node.Indexer.GetSetMethod(true);
                this.EmitCall(objectType, setMethod);
            }
            else if (node.Arguments.Count != 1)
            {
                this._ilg.Emit(OpCodes.Call, node.Object.Type.GetMethod("Set", BindingFlags.Public | BindingFlags.Instance));
            }
            else
            {
                this._ilg.EmitStoreElement(node.Type);
            }
        }

        private void EmitSwitchBucket(SwitchInfo info, List<SwitchLabel> bucket)
        {
            if (bucket.Count == 1)
            {
                this._ilg.Emit(OpCodes.Ldloc, info.Value);
                this._ilg.EmitConstant(bucket[0].Constant);
                this._ilg.Emit(OpCodes.Beq, bucket[0].Label);
            }
            else
            {
                Label? nullable = null;
                if (info.Is64BitSwitch)
                {
                    nullable = new Label?(this._ilg.DefineLabel());
                    this._ilg.Emit(OpCodes.Ldloc, info.Value);
                    this._ilg.EmitConstant(bucket.Last<SwitchLabel>().Constant);
                    this._ilg.Emit(info.IsUnsigned ? OpCodes.Bgt_Un : OpCodes.Bgt, nullable.Value);
                    this._ilg.Emit(OpCodes.Ldloc, info.Value);
                    this._ilg.EmitConstant(bucket[0].Constant);
                    this._ilg.Emit(info.IsUnsigned ? OpCodes.Blt_Un : OpCodes.Blt, nullable.Value);
                }
                this._ilg.Emit(OpCodes.Ldloc, info.Value);
                decimal key = bucket[0].Key;
                if (key != 0M)
                {
                    this._ilg.EmitConstant(bucket[0].Constant);
                    this._ilg.Emit(OpCodes.Sub);
                }
                if (info.Is64BitSwitch)
                {
                    this._ilg.Emit(OpCodes.Conv_I4);
                }
                int num2 = (int) decimal.op_Increment(bucket[bucket.Count - 1].Key - bucket[0].Key);
                Label[] labels = new Label[num2];
                int num3 = 0;
                foreach (SwitchLabel label in bucket)
                {
                    goto Label_01F3;
                Label_01DB:
                    labels[num3++] = info.Default;
                Label_01F3:
                    key = decimal.op_Increment(key);
                    if (key != label.Key)
                    {
                        goto Label_01DB;
                    }
                    labels[num3++] = label.Label;
                }
                this._ilg.Emit(OpCodes.Switch, labels);
                if (info.Is64BitSwitch)
                {
                    this._ilg.MarkLabel(nullable.Value);
                }
            }
        }

        private void EmitSwitchBuckets(SwitchInfo info, List<List<SwitchLabel>> buckets, int first, int last)
        {
            if (first == last)
            {
                this.EmitSwitchBucket(info, buckets[first]);
            }
            else
            {
                int num = (int) (((first + last) + 1L) / 2L);
                if (first == (num - 1))
                {
                    this.EmitSwitchBucket(info, buckets[first]);
                }
                else
                {
                    Label label = this._ilg.DefineLabel();
                    this._ilg.Emit(OpCodes.Ldloc, info.Value);
                    this._ilg.EmitConstant(buckets[num - 1].Last<SwitchLabel>().Constant);
                    this._ilg.Emit(info.IsUnsigned ? OpCodes.Bgt_Un : OpCodes.Bgt, label);
                    this.EmitSwitchBuckets(info, buckets, first, num - 1);
                    this._ilg.MarkLabel(label);
                }
                this.EmitSwitchBuckets(info, buckets, num, last);
            }
        }

        private void EmitSwitchCases(SwitchExpression node, Label[] labels, bool[] isGoto, Label @default, Label end, CompilationFlags flags)
        {
            this._ilg.Emit(OpCodes.Br, @default);
            int index = 0;
            int count = node.Cases.Count;
            while (index < count)
            {
                if (!isGoto[index])
                {
                    this._ilg.MarkLabel(labels[index]);
                    this.EmitExpressionAsType(node.Cases[index].Body, node.Type, flags);
                    if ((node.DefaultBody != null) || (index < (count - 1)))
                    {
                        if ((flags & CompilationFlags.EmitAsTailCallMask) == CompilationFlags.EmitAsTail)
                        {
                            this._ilg.Emit(OpCodes.Ret);
                        }
                        else
                        {
                            this._ilg.Emit(OpCodes.Br, end);
                        }
                    }
                }
                index++;
            }
            if (node.DefaultBody != null)
            {
                this._ilg.MarkLabel(@default);
                this.EmitExpressionAsType(node.DefaultBody, node.Type, flags);
            }
            this._ilg.MarkLabel(end);
        }

        private void EmitSwitchExpression(Expression expr, CompilationFlags flags)
        {
            SwitchExpression node = (SwitchExpression) expr;
            if (!this.TryEmitSwitchInstruction(node, flags) && !this.TryEmitHashtableSwitch(node, flags))
            {
                ParameterExpression variable = Expression.Parameter(node.SwitchValue.Type, "switchValue");
                ParameterExpression expression3 = Expression.Parameter(GetTestValueType(node), "testValue");
                this._scope.AddLocal(this, variable);
                this._scope.AddLocal(this, expression3);
                this.EmitExpression(node.SwitchValue);
                this._scope.EmitSet(variable);
                Label[] labels = new Label[node.Cases.Count];
                bool[] isGoto = new bool[node.Cases.Count];
                int index = 0;
                int count = node.Cases.Count;
                while (index < count)
                {
                    this.DefineSwitchCaseLabel(node.Cases[index], out labels[index], out isGoto[index]);
                    foreach (Expression expression4 in node.Cases[index].TestValues)
                    {
                        this.EmitExpression(expression4);
                        this._scope.EmitSet(expression3);
                        this.EmitExpressionAndBranch(true, Expression.Equal(variable, expression3, false, node.Comparison), labels[index]);
                    }
                    index++;
                }
                Label end = this._ilg.DefineLabel();
                Label label2 = (node.DefaultBody == null) ? end : this._ilg.DefineLabel();
                this.EmitSwitchCases(node, labels, isGoto, label2, end, flags);
            }
        }

        private void EmitThrow(UnaryExpression expr, CompilationFlags flags)
        {
            if (expr.Operand == null)
            {
                this.CheckRethrow();
                this._ilg.Emit(OpCodes.Rethrow);
            }
            else
            {
                this.EmitExpression(expr.Operand);
                this._ilg.Emit(OpCodes.Throw);
            }
            this.EmitUnreachable(expr, flags);
        }

        private void EmitThrowUnaryExpression(Expression expr)
        {
            this.EmitThrow((UnaryExpression) expr, CompilationFlags.EmitAsDefaultType);
        }

        private void EmitTryExpression(Expression expr)
        {
            TryExpression expression = (TryExpression) expr;
            this.CheckTry();
            this.PushLabelBlock(LabelScopeKind.Try);
            this._ilg.BeginExceptionBlock();
            this.EmitExpression(expression.Body);
            Type type = expr.Type;
            LocalBuilder local = null;
            if (type != typeof(void))
            {
                local = this.GetLocal(type);
                this._ilg.Emit(OpCodes.Stloc, local);
            }
            foreach (CatchBlock block in expression.Handlers)
            {
                this.PushLabelBlock(LabelScopeKind.Catch);
                if (block.Filter == null)
                {
                    this._ilg.BeginCatchBlock(block.Test);
                }
                else
                {
                    this._ilg.BeginExceptFilterBlock();
                }
                this.EnterScope(block);
                this.EmitCatchStart(block);
                this.EmitExpression(block.Body);
                if (type != typeof(void))
                {
                    this._ilg.Emit(OpCodes.Stloc, local);
                }
                this.ExitScope(block);
                this.PopLabelBlock(LabelScopeKind.Catch);
            }
            if ((expression.Finally != null) || (expression.Fault != null))
            {
                this.PushLabelBlock(LabelScopeKind.Finally);
                if (expression.Finally != null)
                {
                    this._ilg.BeginFinallyBlock();
                }
                else
                {
                    this._ilg.BeginFaultBlock();
                }
                this.EmitExpressionAsVoid(expression.Finally ?? expression.Fault);
                this._ilg.EndExceptionBlock();
                this.PopLabelBlock(LabelScopeKind.Finally);
            }
            else
            {
                this._ilg.EndExceptionBlock();
            }
            if (type != typeof(void))
            {
                this._ilg.Emit(OpCodes.Ldloc, local);
                this.FreeLocal(local);
            }
            this.PopLabelBlock(LabelScopeKind.Try);
        }

        private void EmitTypeBinaryExpression(Expression expr)
        {
            TypeBinaryExpression typeIs = (TypeBinaryExpression) expr;
            if (typeIs.NodeType == ExpressionType.TypeEqual)
            {
                this.EmitExpression(typeIs.ReduceTypeEqual());
            }
            else
            {
                Type type = typeIs.Expression.Type;
                AnalyzeTypeIsResult result = ConstantCheck.AnalyzeTypeIs(typeIs);
                switch (result)
                {
                    case AnalyzeTypeIsResult.KnownTrue:
                    case AnalyzeTypeIsResult.KnownFalse:
                        this.EmitExpressionAsVoid(typeIs.Expression);
                        this._ilg.EmitBoolean(result == AnalyzeTypeIsResult.KnownTrue);
                        return;
                }
                if (result == AnalyzeTypeIsResult.KnownAssignable)
                {
                    if (type.IsNullableType())
                    {
                        this.EmitAddress(typeIs.Expression, type);
                        this._ilg.EmitHasValue(type);
                    }
                    else
                    {
                        this.EmitExpression(typeIs.Expression);
                        this._ilg.Emit(OpCodes.Ldnull);
                        this._ilg.Emit(OpCodes.Ceq);
                        this._ilg.Emit(OpCodes.Ldc_I4_0);
                        this._ilg.Emit(OpCodes.Ceq);
                    }
                }
                else
                {
                    this.EmitExpression(typeIs.Expression);
                    if (type.IsValueType)
                    {
                        this._ilg.Emit(OpCodes.Box, type);
                    }
                    this._ilg.Emit(OpCodes.Isinst, typeIs.TypeOperand);
                    this._ilg.Emit(OpCodes.Ldnull);
                    this._ilg.Emit(OpCodes.Cgt_Un);
                }
            }
        }

        private void EmitUnary(UnaryExpression node, CompilationFlags flags)
        {
            if (node.Method != null)
            {
                this.EmitUnaryMethod(node, flags);
            }
            else if ((node.NodeType == ExpressionType.NegateChecked) && TypeUtils.IsInteger(node.Operand.Type))
            {
                this.EmitExpression(node.Operand);
                LocalBuilder local = this.GetLocal(node.Operand.Type);
                this._ilg.Emit(OpCodes.Stloc, local);
                this._ilg.EmitInt(0);
                this._ilg.EmitConvertToType(typeof(int), node.Operand.Type, false);
                this._ilg.Emit(OpCodes.Ldloc, local);
                this.FreeLocal(local);
                this.EmitBinaryOperator(ExpressionType.SubtractChecked, node.Operand.Type, node.Operand.Type, node.Type, false);
            }
            else
            {
                this.EmitExpression(node.Operand);
                this.EmitUnaryOperator(node.NodeType, node.Operand.Type, node.Type);
            }
        }

        private void EmitUnaryExpression(Expression expr, CompilationFlags flags)
        {
            this.EmitUnary((UnaryExpression) expr, flags);
        }

        private void EmitUnaryMethod(UnaryExpression node, CompilationFlags flags)
        {
            if (node.IsLifted)
            {
                ParameterExpression expression = Expression.Variable(node.Operand.Type.GetNonNullableType(), null);
                MethodCallExpression mc = Expression.Call(node.Method, expression);
                Type nullableType = TypeUtils.GetNullableType(mc.Type);
                this.EmitLift(node.NodeType, nullableType, mc, new ParameterExpression[] { expression }, new Expression[] { node.Operand });
                this._ilg.EmitConvertToType(nullableType, node.Type, false);
            }
            else
            {
                this.EmitMethodCallExpression(Expression.Call(node.Method, node.Operand), flags);
            }
        }

        private void EmitUnaryOperator(ExpressionType op, Type operandType, Type resultType)
        {
            bool flag = operandType.IsNullableType();
            if (op == ExpressionType.ArrayLength)
            {
                this._ilg.Emit(OpCodes.Ldlen);
                return;
            }
            if (!flag)
            {
                switch (op)
                {
                    case ExpressionType.OnesComplement:
                        this._ilg.Emit(OpCodes.Not);
                        goto Label_04DB;

                    case ExpressionType.IsTrue:
                        this._ilg.Emit(OpCodes.Ldc_I4_1);
                        this._ilg.Emit(OpCodes.Ceq);
                        return;

                    case ExpressionType.IsFalse:
                        this._ilg.Emit(OpCodes.Ldc_I4_0);
                        this._ilg.Emit(OpCodes.Ceq);
                        return;

                    case ExpressionType.Increment:
                        this.EmitConstantOne(resultType);
                        this._ilg.Emit(OpCodes.Add);
                        goto Label_04DB;

                    case ExpressionType.Decrement:
                        this.EmitConstantOne(resultType);
                        this._ilg.Emit(OpCodes.Sub);
                        goto Label_04DB;

                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                        this._ilg.Emit(OpCodes.Neg);
                        goto Label_04DB;

                    case ExpressionType.UnaryPlus:
                        this._ilg.Emit(OpCodes.Nop);
                        goto Label_04DB;

                    case ExpressionType.Not:
                        if (!(operandType == typeof(bool)))
                        {
                            this._ilg.Emit(OpCodes.Not);
                        }
                        else
                        {
                            this._ilg.Emit(OpCodes.Ldc_I4_0);
                            this._ilg.Emit(OpCodes.Ceq);
                        }
                        goto Label_04DB;

                    case ExpressionType.TypeAs:
                        if (operandType.IsValueType)
                        {
                            this._ilg.Emit(OpCodes.Box, operandType);
                        }
                        this._ilg.Emit(OpCodes.Isinst, resultType);
                        if (resultType.IsNullableType())
                        {
                            this._ilg.Emit(OpCodes.Unbox_Any, resultType);
                        }
                        return;
                }
                throw System.Linq.Expressions.Error.UnhandledUnary(op);
            }
            switch (op)
            {
                case ExpressionType.OnesComplement:
                case ExpressionType.IsTrue:
                case ExpressionType.IsFalse:
                case ExpressionType.Decrement:
                case ExpressionType.Increment:
                case ExpressionType.Negate:
                case ExpressionType.UnaryPlus:
                case ExpressionType.NegateChecked:
                    break;

                case ExpressionType.Not:
                {
                    if (operandType != typeof(bool?))
                    {
                        break;
                    }
                    this._ilg.DefineLabel();
                    Label label = this._ilg.DefineLabel();
                    LocalBuilder builder = this.GetLocal(operandType);
                    this._ilg.Emit(OpCodes.Stloc, builder);
                    this._ilg.Emit(OpCodes.Ldloca, builder);
                    this._ilg.EmitHasValue(operandType);
                    this._ilg.Emit(OpCodes.Brfalse_S, label);
                    this._ilg.Emit(OpCodes.Ldloca, builder);
                    this._ilg.EmitGetValueOrDefault(operandType);
                    Type type = operandType.GetNonNullableType();
                    this.EmitUnaryOperator(op, type, typeof(bool));
                    ConstructorInfo con = resultType.GetConstructor(new Type[] { typeof(bool) });
                    this._ilg.Emit(OpCodes.Newobj, con);
                    this._ilg.Emit(OpCodes.Stloc, builder);
                    this._ilg.MarkLabel(label);
                    this._ilg.Emit(OpCodes.Ldloc, builder);
                    this.FreeLocal(builder);
                    return;
                }
                case ExpressionType.TypeAs:
                    this._ilg.Emit(OpCodes.Box, operandType);
                    this._ilg.Emit(OpCodes.Isinst, resultType);
                    if (resultType.IsNullableType())
                    {
                        this._ilg.Emit(OpCodes.Unbox_Any, resultType);
                    }
                    return;

                default:
                    throw System.Linq.Expressions.Error.UnhandledUnary(op);
            }
            Label label2 = this._ilg.DefineLabel();
            Label label3 = this._ilg.DefineLabel();
            LocalBuilder local = this.GetLocal(operandType);
            this._ilg.Emit(OpCodes.Stloc, local);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.EmitHasValue(operandType);
            this._ilg.Emit(OpCodes.Brfalse_S, label2);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.EmitGetValueOrDefault(operandType);
            Type nonNullableType = resultType.GetNonNullableType();
            this.EmitUnaryOperator(op, nonNullableType, nonNullableType);
            ConstructorInfo constructor = resultType.GetConstructor(new Type[] { nonNullableType });
            this._ilg.Emit(OpCodes.Newobj, constructor);
            this._ilg.Emit(OpCodes.Stloc, local);
            this._ilg.Emit(OpCodes.Br_S, label3);
            this._ilg.MarkLabel(label2);
            this._ilg.Emit(OpCodes.Ldloca, local);
            this._ilg.Emit(OpCodes.Initobj, resultType);
            this._ilg.MarkLabel(label3);
            this._ilg.Emit(OpCodes.Ldloc, local);
            this.FreeLocal(local);
            return;
        Label_04DB:
            this.EmitConvertArithmeticResult(op, resultType);
        }

        private void EmitUnboxUnaryExpression(Expression expr)
        {
            UnaryExpression expression = (UnaryExpression) expr;
            this.EmitExpression(expression.Operand);
            this._ilg.Emit(OpCodes.Unbox_Any, expression.Type);
        }

        private void EmitUnliftedAndAlso(BinaryExpression b)
        {
            Label label = this._ilg.DefineLabel();
            Label label2 = this._ilg.DefineLabel();
            this.EmitExpressionAndBranch(false, b.Left, label);
            this.EmitExpression(b.Right);
            this._ilg.Emit(OpCodes.Br, label2);
            this._ilg.MarkLabel(label);
            this._ilg.Emit(OpCodes.Ldc_I4_0);
            this._ilg.MarkLabel(label2);
        }

        private void EmitUnliftedBinaryOp(ExpressionType op, Type leftType, Type rightType)
        {
            if ((op == ExpressionType.Equal) || (op == ExpressionType.NotEqual))
            {
                this.EmitUnliftedEquality(op, leftType);
            }
            else
            {
                Label label;
                Label label2;
                if (!leftType.IsPrimitive)
                {
                    throw System.Linq.Expressions.Error.OperatorNotImplementedForType(op, leftType);
                }
                switch (op)
                {
                    case ExpressionType.Add:
                        this._ilg.Emit(OpCodes.Add);
                        return;

                    case ExpressionType.AddChecked:
                        if (!TypeUtils.IsFloatingPoint(leftType))
                        {
                            if (TypeUtils.IsUnsigned(leftType))
                            {
                                this._ilg.Emit(OpCodes.Add_Ovf_Un);
                                return;
                            }
                            this._ilg.Emit(OpCodes.Add_Ovf);
                            return;
                        }
                        this._ilg.Emit(OpCodes.Add);
                        return;

                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                        this._ilg.Emit(OpCodes.And);
                        return;

                    case ExpressionType.Divide:
                        if (!TypeUtils.IsUnsigned(leftType))
                        {
                            this._ilg.Emit(OpCodes.Div);
                            return;
                        }
                        this._ilg.Emit(OpCodes.Div_Un);
                        return;

                    case ExpressionType.ExclusiveOr:
                        this._ilg.Emit(OpCodes.Xor);
                        return;

                    case ExpressionType.GreaterThan:
                        if (!TypeUtils.IsUnsigned(leftType))
                        {
                            this._ilg.Emit(OpCodes.Cgt);
                            return;
                        }
                        this._ilg.Emit(OpCodes.Cgt_Un);
                        return;

                    case ExpressionType.GreaterThanOrEqual:
                    {
                        Label label3 = this._ilg.DefineLabel();
                        Label label4 = this._ilg.DefineLabel();
                        if (!TypeUtils.IsUnsigned(leftType))
                        {
                            this._ilg.Emit(OpCodes.Bge_S, label3);
                        }
                        else
                        {
                            this._ilg.Emit(OpCodes.Bge_Un_S, label3);
                        }
                        this._ilg.Emit(OpCodes.Ldc_I4_0);
                        this._ilg.Emit(OpCodes.Br_S, label4);
                        this._ilg.MarkLabel(label3);
                        this._ilg.Emit(OpCodes.Ldc_I4_1);
                        this._ilg.MarkLabel(label4);
                        return;
                    }
                    case ExpressionType.LeftShift:
                        if (rightType != typeof(int))
                        {
                            throw ContractUtils.Unreachable;
                        }
                        this._ilg.Emit(OpCodes.Shl);
                        return;

                    case ExpressionType.LessThan:
                        if (!TypeUtils.IsUnsigned(leftType))
                        {
                            this._ilg.Emit(OpCodes.Clt);
                            return;
                        }
                        this._ilg.Emit(OpCodes.Clt_Un);
                        return;

                    case ExpressionType.LessThanOrEqual:
                        label = this._ilg.DefineLabel();
                        label2 = this._ilg.DefineLabel();
                        if (!TypeUtils.IsUnsigned(leftType))
                        {
                            this._ilg.Emit(OpCodes.Ble_S, label);
                            break;
                        }
                        this._ilg.Emit(OpCodes.Ble_Un_S, label);
                        break;

                    case ExpressionType.Modulo:
                        if (!TypeUtils.IsUnsigned(leftType))
                        {
                            this._ilg.Emit(OpCodes.Rem);
                            return;
                        }
                        this._ilg.Emit(OpCodes.Rem_Un);
                        return;

                    case ExpressionType.Multiply:
                        this._ilg.Emit(OpCodes.Mul);
                        return;

                    case ExpressionType.MultiplyChecked:
                        if (!TypeUtils.IsFloatingPoint(leftType))
                        {
                            if (TypeUtils.IsUnsigned(leftType))
                            {
                                this._ilg.Emit(OpCodes.Mul_Ovf_Un);
                                return;
                            }
                            this._ilg.Emit(OpCodes.Mul_Ovf);
                            return;
                        }
                        this._ilg.Emit(OpCodes.Mul);
                        return;

                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                        this._ilg.Emit(OpCodes.Or);
                        return;

                    case ExpressionType.RightShift:
                        if (rightType != typeof(int))
                        {
                            throw ContractUtils.Unreachable;
                        }
                        if (TypeUtils.IsUnsigned(leftType))
                        {
                            this._ilg.Emit(OpCodes.Shr_Un);
                            return;
                        }
                        this._ilg.Emit(OpCodes.Shr);
                        return;

                    case ExpressionType.Subtract:
                        this._ilg.Emit(OpCodes.Sub);
                        return;

                    case ExpressionType.SubtractChecked:
                        if (!TypeUtils.IsFloatingPoint(leftType))
                        {
                            if (TypeUtils.IsUnsigned(leftType))
                            {
                                this._ilg.Emit(OpCodes.Sub_Ovf_Un);
                                return;
                            }
                            this._ilg.Emit(OpCodes.Sub_Ovf);
                            return;
                        }
                        this._ilg.Emit(OpCodes.Sub);
                        return;

                    default:
                        throw System.Linq.Expressions.Error.UnhandledBinary(op);
                }
                this._ilg.Emit(OpCodes.Ldc_I4_0);
                this._ilg.Emit(OpCodes.Br_S, label2);
                this._ilg.MarkLabel(label);
                this._ilg.Emit(OpCodes.Ldc_I4_1);
                this._ilg.MarkLabel(label2);
            }
        }

        private void EmitUnliftedEquality(ExpressionType op, Type type)
        {
            if ((!type.IsPrimitive && type.IsValueType) && !type.IsEnum)
            {
                throw System.Linq.Expressions.Error.OperatorNotImplementedForType(op, type);
            }
            this._ilg.Emit(OpCodes.Ceq);
            if (op == ExpressionType.NotEqual)
            {
                this._ilg.Emit(OpCodes.Ldc_I4_0);
                this._ilg.Emit(OpCodes.Ceq);
            }
        }

        private void EmitUnliftedOrElse(BinaryExpression b)
        {
            Label label = this._ilg.DefineLabel();
            Label label2 = this._ilg.DefineLabel();
            this.EmitExpressionAndBranch(false, b.Left, label);
            this._ilg.Emit(OpCodes.Ldc_I4_1);
            this._ilg.Emit(OpCodes.Br, label2);
            this._ilg.MarkLabel(label);
            this.EmitExpression(b.Right);
            this._ilg.MarkLabel(label2);
        }

        private void EmitUnreachable(Expression node, CompilationFlags flags)
        {
            if ((node.Type != typeof(void)) && ((flags & CompilationFlags.EmitAsVoidType) == 0))
            {
                this._ilg.EmitDefault(node.Type);
            }
        }

        private void EmitVariableAssignment(BinaryExpression node, CompilationFlags flags)
        {
            ParameterExpression left = (ParameterExpression) node.Left;
            CompilationFlags flags2 = flags & CompilationFlags.EmitAsTypeMask;
            this.EmitExpression(node.Right);
            if (flags2 != CompilationFlags.EmitAsVoidType)
            {
                this._ilg.Emit(OpCodes.Dup);
            }
            if (left.IsByRef)
            {
                LocalBuilder local = this.GetLocal(left.Type);
                this._ilg.Emit(OpCodes.Stloc, local);
                this._scope.EmitGet(left);
                this._ilg.Emit(OpCodes.Ldloc, local);
                this.FreeLocal(local);
                this._ilg.EmitStoreValueIndirect(left.Type);
            }
            else
            {
                this._scope.EmitSet(left);
            }
        }

        private static void EmitWriteBack(IList<WriteBack> writeBacks)
        {
            foreach (WriteBack back in writeBacks)
            {
                back();
            }
        }

        private LabelInfo EnsureLabel(LabelTarget node)
        {
            LabelInfo info;
            if (!this._labelInfo.TryGetValue(node, out info))
            {
                this._labelInfo.Add(node, info = new LabelInfo(this._ilg, node, false));
            }
            return info;
        }

        private void EnterScope(object node)
        {
            if (HasVariables(node) && ((this._scope.MergedScopes == null) || !this._scope.MergedScopes.Contains(node)))
            {
                CompilerScope scope;
                if (!this._tree.Scopes.TryGetValue(node, out scope))
                {
                    scope = new CompilerScope(node, false) {
                        NeedsClosure = this._scope.NeedsClosure
                    };
                }
                this._scope = scope.Enter(this, this._scope);
            }
        }

        private void ExitScope(object node)
        {
            if (this._scope.Node == node)
            {
                this._scope = this._scope.Exit();
            }
        }

        private static bool FitsInBucket(List<SwitchLabel> buckets, decimal key, int count)
        {
            decimal num = decimal.op_Increment(key - buckets[0].Key);
            if (num > 2147483647M)
            {
                return false;
            }
            return (((buckets.Count + count) * 2) > num);
        }

        internal void FreeLocal(LocalBuilder local)
        {
            if (local != null)
            {
                this._freeLocals.Enqueue(local.LocalType, local);
            }
        }

        private static Expression GetEqualityOperand(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Convert)
            {
                UnaryExpression expression2 = (UnaryExpression) expression;
                if (TypeUtils.AreReferenceAssignable(expression2.Type, expression2.Operand.Type))
                {
                    return expression2.Operand;
                }
            }
            return expression;
        }

        internal int GetLambdaArgument(int index)
        {
            return ((index + (this._hasClosureArgument ? 1 : 0)) + (this._method.IsStatic ? 0 : 1));
        }

        internal LocalBuilder GetLocal(Type type)
        {
            LocalBuilder builder;
            if (this._freeLocals.TryDequeue(type, out builder))
            {
                return builder;
            }
            return this._ilg.DeclareLocal(type);
        }

        private static Type GetMemberType(MemberInfo member)
        {
            FieldInfo info = member as FieldInfo;
            if (info != null)
            {
                return info.FieldType;
            }
            PropertyInfo info2 = member as PropertyInfo;
            if (info2 == null)
            {
                throw System.Linq.Expressions.Error.MemberNotFieldOrProperty(member);
            }
            return info2.PropertyType;
        }

        internal LocalBuilder GetNamedLocal(Type type, ParameterExpression variable)
        {
            LocalBuilder localBuilder = this._ilg.DeclareLocal(type);
            if (this.EmitDebugSymbols && (variable.Name != null))
            {
                this._tree.DebugInfoGenerator.SetLocalName(localBuilder, variable.Name);
            }
            return localBuilder;
        }

        private static Type[] GetParameterTypes(LambdaExpression lambda)
        {
            return lambda.Parameters.Map<ParameterExpression, Type>(delegate (ParameterExpression p) {
                if (!p.IsByRef)
                {
                    return p.Type;
                }
                return p.Type.MakeByRefType();
            });
        }

        private static Type GetTestValueType(SwitchExpression node)
        {
            if (node.Comparison == null)
            {
                return node.Cases[0].TestValues[0].Type;
            }
            Type nonRefType = node.Comparison.GetParametersCached()[1].ParameterType.GetNonRefType();
            if (node.IsLifted)
            {
                nonRefType = TypeUtils.GetNullableType(nonRefType);
            }
            return nonRefType;
        }

        private static string GetUniqueMethodName()
        {
            return ("<ExpressionCompilerImplementationDetails>{" + Interlocked.Increment(ref _Counter) + "}lambda_method");
        }

        private static bool HasVariables(object node)
        {
            BlockExpression expression = node as BlockExpression;
            if (expression != null)
            {
                return (expression.Variables.Count > 0);
            }
            return (((CatchBlock) node).Variable != null);
        }

        private void InitializeMethod()
        {
            this.AddReturnLabel(this._lambda);
            this._boundConstants.EmitCacheConstants(this);
        }

        private static bool IsChecked(ExpressionType op)
        {
            switch (op)
            {
                case ExpressionType.AddAssignChecked:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.NegateChecked:
                case ExpressionType.SubtractChecked:
                case ExpressionType.AddChecked:
                case ExpressionType.ConvertChecked:
                case ExpressionType.MultiplyChecked:
                    return true;
            }
            return false;
        }

        private static void MergeBuckets(List<List<SwitchLabel>> buckets)
        {
            while (buckets.Count > 1)
            {
                List<SwitchLabel> list = buckets[buckets.Count - 2];
                List<SwitchLabel> collection = buckets[buckets.Count - 1];
                if (!FitsInBucket(list, collection[collection.Count - 1].Key, collection.Count))
                {
                    return;
                }
                list.AddRange(collection);
                buckets.RemoveAt(buckets.Count - 1);
            }
        }

        private static bool MethodHasByRefParameter(MethodInfo mi)
        {
            foreach (ParameterInfo info in mi.GetParametersCached())
            {
                if (info.IsByRefParameter())
                {
                    return true;
                }
            }
            return false;
        }

        private static bool NotEmpty(Expression node)
        {
            DefaultExpression expression = node as DefaultExpression;
            if ((expression != null) && !(expression.Type != typeof(void)))
            {
                return false;
            }
            return true;
        }

        private void PopLabelBlock(LabelScopeKind kind)
        {
            this._labelBlock = this._labelBlock.Parent;
        }

        private void PushLabelBlock(LabelScopeKind type)
        {
            this._labelBlock = new LabelScopeInfo(this._labelBlock, type);
        }

        private LabelInfo ReferenceLabel(LabelTarget node)
        {
            LabelInfo info = this.EnsureLabel(node);
            info.Reference(this._labelBlock);
            return info;
        }

        private static bool Significant(Expression node)
        {
            BlockExpression expression = node as BlockExpression;
            if (expression != null)
            {
                for (int i = 0; i < expression.ExpressionCount; i++)
                {
                    if (Significant(expression.GetExpression(i)))
                    {
                        return true;
                    }
                }
                return false;
            }
            return (NotEmpty(node) && !(node is DebugInfoExpression));
        }

        public override string ToString()
        {
            return this._method.ToString();
        }

        private bool TryEmitHashtableSwitch(SwitchExpression node, CompilationFlags flags)
        {
            if (node.Comparison != typeof(string).GetMethod("op_Equality", BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(string) }, null))
            {
                return false;
            }
            int capacity = 0;
            foreach (SwitchCase @case in node.Cases)
            {
                foreach (Expression expression in @case.TestValues)
                {
                    if (!(expression is ConstantExpression))
                    {
                        return false;
                    }
                    capacity++;
                }
            }
            if (capacity < 7)
            {
                return false;
            }
            List<ElementInit> initializers = new List<ElementInit>(capacity);
            List<SwitchCase> cases = new List<SwitchCase>(node.Cases.Count);
            int num2 = -1;
            MethodInfo method = typeof(Dictionary<string, int>).GetMethod("Add", new Type[] { typeof(string), typeof(int) });
            int num3 = 0;
            int count = node.Cases.Count;
            while (num3 < count)
            {
                foreach (ConstantExpression expression2 in node.Cases[num3].TestValues)
                {
                    if (expression2.Value != null)
                    {
                        initializers.Add(Expression.ElementInit(method, new Expression[] { expression2, Expression.Constant(num3) }));
                    }
                    else
                    {
                        num2 = num3;
                    }
                }
                cases.Add(Expression.SwitchCase(node.Cases[num3].Body, new Expression[] { Expression.Constant(num3) }));
                num3++;
            }
            MemberExpression ifFalse = this.CreateLazyInitializedField<Dictionary<string, int>>("dictionarySwitch");
            Expression instance = Expression.Condition(Expression.Equal(ifFalse, Expression.Constant(null, ifFalse.Type)), Expression.Assign(ifFalse, Expression.ListInit(Expression.New(typeof(Dictionary<string, int>).GetConstructor(new Type[] { typeof(int) }), new Expression[] { Expression.Constant(initializers.Count) }), initializers)), ifFalse);
            ParameterExpression left = Expression.Variable(typeof(string), "switchValue");
            ParameterExpression expression6 = Expression.Variable(typeof(int), "switchIndex");
            BlockExpression expression7 = Expression.Block(new ParameterExpression[] { expression6, left }, new Expression[] { Expression.Assign(left, node.SwitchValue), Expression.IfThenElse(Expression.Equal(left, Expression.Constant(null, typeof(string))), Expression.Assign(expression6, Expression.Constant(num2)), Expression.IfThenElse(Expression.Call(instance, "TryGetValue", (Type[]) null, new Expression[] { left, expression6 }), Expression.Empty(), Expression.Assign(expression6, Expression.Constant(-1)))), Expression.Switch(node.Type, expression6, node.DefaultBody, null, cases) });
            this.EmitExpression(expression7, flags);
            return true;
        }

        private bool TryEmitSwitchInstruction(SwitchExpression node, CompilationFlags flags)
        {
            if (node.Comparison != null)
            {
                return false;
            }
            Type valueType = node.SwitchValue.Type;
            if (!CanOptimizeSwitchType(valueType) || !TypeUtils.AreEquivalent(valueType, node.Cases[0].TestValues[0].Type))
            {
                return false;
            }
            if (!node.Cases.All<SwitchCase>(c => c.TestValues.All<Expression>(t => (t is ConstantExpression))))
            {
                return false;
            }
            Label[] labels = new Label[node.Cases.Count];
            bool[] isGoto = new bool[node.Cases.Count];
            System.Linq.Expressions.Set<decimal> set = new System.Linq.Expressions.Set<decimal>();
            List<SwitchLabel> list = new List<SwitchLabel>();
            for (int i = 0; i < node.Cases.Count; i++)
            {
                this.DefineSwitchCaseLabel(node.Cases[i], out labels[i], out isGoto[i]);
                foreach (ConstantExpression expression in node.Cases[i].TestValues)
                {
                    decimal item = ConvertSwitchValue(expression.Value);
                    if (!set.Contains(item))
                    {
                        list.Add(new SwitchLabel(item, expression.Value, labels[i]));
                        set.Add(item);
                    }
                }
            }
            list.Sort((Comparison<SwitchLabel>) ((x, y) => Math.Sign((decimal) (x.Key - y.Key))));
            List<List<SwitchLabel>> buckets = new List<List<SwitchLabel>>();
            foreach (SwitchLabel label in list)
            {
                AddToBuckets(buckets, label);
            }
            LocalBuilder local = this.GetLocal(node.SwitchValue.Type);
            this.EmitExpression(node.SwitchValue);
            this._ilg.Emit(OpCodes.Stloc, local);
            Label end = this._ilg.DefineLabel();
            Label label3 = (node.DefaultBody == null) ? end : this._ilg.DefineLabel();
            SwitchInfo info = new SwitchInfo(node, local, label3);
            this.EmitSwitchBuckets(info, buckets, 0, buckets.Count - 1);
            this.EmitSwitchCases(node, labels, isGoto, label3, end, flags);
            this.FreeLocal(local);
            return true;
        }

        private bool TryPushLabelBlock(Expression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Conditional:
                case ExpressionType.Goto:
                case ExpressionType.Loop:
                    this.PushLabelBlock(LabelScopeKind.Statement);
                    return true;

                case ExpressionType.Convert:
                    if (node.Type != typeof(void))
                    {
                        break;
                    }
                    this.PushLabelBlock(LabelScopeKind.Statement);
                    return true;

                case ExpressionType.Block:
                    if (node is SpilledExpressionBlock)
                    {
                        break;
                    }
                    this.PushLabelBlock(LabelScopeKind.Block);
                    if (this._labelBlock.Parent.Kind != LabelScopeKind.Switch)
                    {
                        this.DefineBlockLabels(node);
                    }
                    return true;

                case ExpressionType.Label:
                {
                    if (this._labelBlock.Kind != LabelScopeKind.Block)
                    {
                        goto Label_00B3;
                    }
                    LabelTarget target = ((LabelExpression) node).Target;
                    if (!this._labelBlock.ContainsTarget(target))
                    {
                        if ((this._labelBlock.Parent.Kind == LabelScopeKind.Switch) && this._labelBlock.Parent.ContainsTarget(target))
                        {
                            return false;
                        }
                        goto Label_00B3;
                    }
                    return false;
                }
                case ExpressionType.Switch:
                {
                    this.PushLabelBlock(LabelScopeKind.Switch);
                    SwitchExpression expression = (SwitchExpression) node;
                    foreach (SwitchCase @case in expression.Cases)
                    {
                        this.DefineBlockLabels(@case.Body);
                    }
                    this.DefineBlockLabels(expression.DefaultBody);
                    return true;
                }
            }
            if (this._labelBlock.Kind != LabelScopeKind.Expression)
            {
                this.PushLabelBlock(LabelScopeKind.Expression);
                return true;
            }
            return false;
        Label_00B3:
            this.PushLabelBlock(LabelScopeKind.Statement);
            return true;
        }

        private static CompilationFlags UpdateEmitAsTailCallFlag(CompilationFlags flags, CompilationFlags newValue)
        {
            CompilationFlags flags2 = flags & CompilationFlags.EmitAsTailCallMask;
            return ((flags ^ flags2) | newValue);
        }

        private static CompilationFlags UpdateEmitAsTypeFlag(CompilationFlags flags, CompilationFlags newValue)
        {
            CompilationFlags flags2 = flags & CompilationFlags.EmitAsTypeMask;
            return ((flags ^ flags2) | newValue);
        }

        private static CompilationFlags UpdateEmitExpressionStartFlag(CompilationFlags flags, CompilationFlags newValue)
        {
            CompilationFlags flags2 = flags & CompilationFlags.EmitExpressionStartMask;
            return ((flags ^ flags2) | newValue);
        }

        private static bool UseVirtual(MethodInfo mi)
        {
            if (mi.IsStatic)
            {
                return false;
            }
            if (mi.DeclaringType.IsValueType)
            {
                return false;
            }
            return true;
        }

        internal static void ValidateLift(IList<ParameterExpression> variables, IList<Expression> arguments)
        {
            if (variables.Count != arguments.Count)
            {
                throw System.Linq.Expressions.Error.IncorrectNumberOfIndexes();
            }
            int num = 0;
            int count = variables.Count;
            while (num < count)
            {
                if (!TypeUtils.AreReferenceAssignable(variables[num].Type, arguments[num].Type.GetNonNullableType()))
                {
                    throw System.Linq.Expressions.Error.ArgumentTypesMustMatch();
                }
                num++;
            }
        }

        internal bool CanEmitBoundConstants
        {
            get
            {
                return (this._method is DynamicMethod);
            }
        }

        private bool EmitDebugSymbols
        {
            get
            {
                return (this._tree.DebugInfoGenerator != null);
            }
        }

        internal ILGenerator IL
        {
            get
            {
                return this._ilg;
            }
        }

        internal ReadOnlyCollection<ParameterExpression> Parameters
        {
            get
            {
                return this._lambda.Parameters;
            }
        }

        [Flags]
        internal enum CompilationFlags
        {
            EmitAsDefaultType = 0x10,
            EmitAsMiddle = 0x200,
            EmitAsNoTail = 0x400,
            EmitAsTail = 0x100,
            EmitAsTailCallMask = 0xf00,
            EmitAsTypeMask = 240,
            EmitAsVoidType = 0x20,
            EmitExpressionStart = 1,
            EmitExpressionStartMask = 15,
            EmitNoExpressionStart = 2
        }

        private sealed class SwitchInfo
        {
            internal readonly Label Default;
            internal readonly bool Is64BitSwitch;
            internal readonly bool IsUnsigned;
            internal readonly SwitchExpression Node;
            internal readonly System.Type Type;
            internal readonly LocalBuilder Value;

            internal SwitchInfo(SwitchExpression node, LocalBuilder value, Label @default)
            {
                this.Node = node;
                this.Value = value;
                this.Default = @default;
                this.Type = this.Node.SwitchValue.Type;
                this.IsUnsigned = TypeUtils.IsUnsigned(this.Type);
                TypeCode typeCode = System.Type.GetTypeCode(this.Type);
                this.Is64BitSwitch = (typeCode == TypeCode.UInt64) || (typeCode == TypeCode.Int64);
            }
        }

        private sealed class SwitchLabel
        {
            internal readonly object Constant;
            internal readonly decimal Key;
            internal readonly System.Reflection.Emit.Label Label;

            internal SwitchLabel(decimal key, object constant, System.Reflection.Emit.Label label)
            {
                this.Key = key;
                this.Constant = constant;
                this.Label = label;
            }
        }

        private delegate void WriteBack();
    }
}

