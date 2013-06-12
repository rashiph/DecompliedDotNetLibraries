namespace System.Linq.Expressions.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Dynamic.Utils;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class StackSpiller
    {
        private RewriteAction _lambdaRewrite;
        private readonly Stack _startingStack;
        private readonly TempMaker _tm = new TempMaker();

        private StackSpiller(Stack stack)
        {
            this._startingStack = stack;
        }

        internal static LambdaExpression AnalyzeLambda(LambdaExpression lambda)
        {
            return lambda.Accept(new StackSpiller(Stack.Empty));
        }

        private static T[] Clone<T>(ReadOnlyCollection<T> original, int max)
        {
            T[] localArray = new T[original.Count];
            for (int i = 0; i < max; i++)
            {
                localArray[i] = original[i];
            }
            return localArray;
        }

        private void Free(int mark)
        {
            this._tm.Free(mark);
        }

        private static Expression MakeBlock(params Expression[] expressions)
        {
            return MakeBlock((IList<Expression>) expressions);
        }

        private static Expression MakeBlock(IList<Expression> expressions)
        {
            return new SpilledExpressionBlock(expressions);
        }

        private ParameterExpression MakeTemp(Type type)
        {
            return this._tm.Temp(type);
        }

        private int Mark()
        {
            return this._tm.Mark();
        }

        private static void RequireNoRefArgs(MethodBase method)
        {
            if ((method != null) && method.GetParametersCached().Any<ParameterInfo>(p => p.ParameterType.IsByRef))
            {
                throw System.Linq.Expressions.Error.TryNotSupportedForMethodsWithRefArgs(method);
            }
        }

        private static void RequireNotRefInstance(Expression instance)
        {
            if (((instance != null) && instance.Type.IsValueType) && (Type.GetTypeCode(instance.Type) == TypeCode.Object))
            {
                throw System.Linq.Expressions.Error.TryNotSupportedForValueTypeInstances(instance.Type);
            }
        }

        internal Expression<T> Rewrite<T>(Expression<T> lambda)
        {
            Result result = this.RewriteExpressionFreeTemps(lambda.Body, this._startingStack);
            this._lambdaRewrite = result.Action;
            if (result.Action == RewriteAction.None)
            {
                return lambda;
            }
            Expression node = result.Node;
            if (this._tm.Temps.Count > 0)
            {
                node = Expression.Block(this._tm.Temps, new Expression[] { node });
            }
            return new Expression<T>(node, lambda.Name, lambda.TailCall, lambda.Parameters);
        }

        private Result RewriteAssignBinaryExpression(Expression expr, Stack stack)
        {
            BinaryExpression node = (BinaryExpression) expr;
            switch (node.Left.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return this.RewriteMemberAssignment(node, stack);

                case ExpressionType.Parameter:
                    return this.RewriteVariableAssignment(node, stack);

                case ExpressionType.Extension:
                    return this.RewriteExtensionAssignment(node, stack);

                case ExpressionType.Index:
                    return this.RewriteIndexAssignment(node, stack);
            }
            throw System.Linq.Expressions.Error.InvalidLvalue(node.Left.NodeType);
        }

        private Result RewriteBinaryExpression(Expression expr, Stack stack)
        {
            BinaryExpression expression = (BinaryExpression) expr;
            ChildRewriter rewriter = new ChildRewriter(this, stack, 3);
            rewriter.Add(expression.Left);
            rewriter.Add(expression.Right);
            rewriter.Add(expression.Conversion);
            if (rewriter.Action == RewriteAction.SpillStack)
            {
                RequireNoRefArgs(expression.Method);
            }
            return rewriter.Finish(rewriter.Rewrite ? BinaryExpression.Create(expression.NodeType, rewriter[0], rewriter[1], expression.Type, expression.Method, (LambdaExpression) rewriter[2]) : expr);
        }

        private Result RewriteBlockExpression(Expression expr, Stack stack)
        {
            BlockExpression expression = (BlockExpression) expr;
            int expressionCount = expression.ExpressionCount;
            RewriteAction none = RewriteAction.None;
            Expression[] args = null;
            for (int i = 0; i < expressionCount; i++)
            {
                Expression node = expression.GetExpression(i);
                Result result = this.RewriteExpression(node, stack);
                none |= result.Action;
                if ((args == null) && (result.Action != RewriteAction.None))
                {
                    args = Clone<Expression>(expression.Expressions, i);
                }
                if (args != null)
                {
                    args[i] = result.Node;
                }
            }
            if (none != RewriteAction.None)
            {
                expr = expression.Rewrite(null, args);
            }
            return new Result(none, expr);
        }

        private Result RewriteConditionalExpression(Expression expr, Stack stack)
        {
            ConditionalExpression expression = (ConditionalExpression) expr;
            Result result = this.RewriteExpression(expression.Test, stack);
            Result result2 = this.RewriteExpression(expression.IfTrue, stack);
            Result result3 = this.RewriteExpression(expression.IfFalse, stack);
            RewriteAction action = (result.Action | result2.Action) | result3.Action;
            if (action != RewriteAction.None)
            {
                expr = Expression.Condition(result.Node, result2.Node, result3.Node, expression.Type);
            }
            return new Result(action, expr);
        }

        private Result RewriteDynamicExpression(Expression expr, Stack stack)
        {
            DynamicExpression expression = (DynamicExpression) expr;
            IArgumentProvider expressions = expression;
            ChildRewriter rewriter = new ChildRewriter(this, Stack.NonEmpty, expressions.ArgumentCount);
            rewriter.AddArguments(expressions);
            if (rewriter.Action == RewriteAction.SpillStack)
            {
                RequireNoRefArgs(expression.DelegateType.GetMethod("Invoke"));
            }
            return rewriter.Finish(rewriter.Rewrite ? expression.Rewrite(rewriter[0, -1]) : expr);
        }

        private Result RewriteExpression(Expression node, Stack stack)
        {
            if (node == null)
            {
                return new Result(RewriteAction.None, null);
            }
            switch (node.NodeType)
            {
                case ExpressionType.Add:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.AddChecked:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.And:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.AndAlso:
                    return this.RewriteLogicalBinaryExpression(node, stack);

                case ExpressionType.ArrayLength:
                    return this.RewriteUnaryExpression(node, stack);

                case ExpressionType.ArrayIndex:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.Call:
                    return this.RewriteMethodCallExpression(node, stack);

                case ExpressionType.Coalesce:
                    return this.RewriteLogicalBinaryExpression(node, stack);

                case ExpressionType.Conditional:
                    return this.RewriteConditionalExpression(node, stack);

                case ExpressionType.Constant:
                case ExpressionType.Parameter:
                case ExpressionType.Quote:
                case ExpressionType.DebugInfo:
                case ExpressionType.Default:
                case ExpressionType.RuntimeVariables:
                    return new Result(RewriteAction.None, node);

                case ExpressionType.Convert:
                    return this.RewriteUnaryExpression(node, stack);

                case ExpressionType.ConvertChecked:
                    return this.RewriteUnaryExpression(node, stack);

                case ExpressionType.Divide:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.Equal:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.ExclusiveOr:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.GreaterThan:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.GreaterThanOrEqual:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.Invoke:
                    return this.RewriteInvocationExpression(node, stack);

                case ExpressionType.Lambda:
                    return RewriteLambdaExpression(node, stack);

                case ExpressionType.LeftShift:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.LessThan:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.LessThanOrEqual:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.ListInit:
                    return this.RewriteListInitExpression(node, stack);

                case ExpressionType.MemberAccess:
                    return this.RewriteMemberExpression(node, stack);

                case ExpressionType.MemberInit:
                    return this.RewriteMemberInitExpression(node, stack);

                case ExpressionType.Modulo:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.Multiply:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.MultiplyChecked:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.Negate:
                    return this.RewriteUnaryExpression(node, stack);

                case ExpressionType.UnaryPlus:
                    return this.RewriteUnaryExpression(node, stack);

                case ExpressionType.NegateChecked:
                    return this.RewriteUnaryExpression(node, stack);

                case ExpressionType.New:
                    return this.RewriteNewExpression(node, stack);

                case ExpressionType.NewArrayInit:
                    return this.RewriteNewArrayExpression(node, stack);

                case ExpressionType.NewArrayBounds:
                    return this.RewriteNewArrayExpression(node, stack);

                case ExpressionType.Not:
                    return this.RewriteUnaryExpression(node, stack);

                case ExpressionType.NotEqual:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.Or:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.OrElse:
                    return this.RewriteLogicalBinaryExpression(node, stack);

                case ExpressionType.Power:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.RightShift:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.Subtract:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.SubtractChecked:
                    return this.RewriteBinaryExpression(node, stack);

                case ExpressionType.TypeAs:
                    return this.RewriteUnaryExpression(node, stack);

                case ExpressionType.TypeIs:
                    return this.RewriteTypeBinaryExpression(node, stack);

                case ExpressionType.Assign:
                    return this.RewriteAssignBinaryExpression(node, stack);

                case ExpressionType.Block:
                    return this.RewriteBlockExpression(node, stack);

                case ExpressionType.Decrement:
                    return this.RewriteUnaryExpression(node, stack);

                case ExpressionType.Dynamic:
                    return this.RewriteDynamicExpression(node, stack);

                case ExpressionType.Extension:
                    return this.RewriteExtensionExpression(node, stack);

                case ExpressionType.Goto:
                    return this.RewriteGotoExpression(node, stack);

                case ExpressionType.Increment:
                    return this.RewriteUnaryExpression(node, stack);

                case ExpressionType.Index:
                    return this.RewriteIndexExpression(node, stack);

                case ExpressionType.Label:
                    return this.RewriteLabelExpression(node, stack);

                case ExpressionType.Loop:
                    return this.RewriteLoopExpression(node, stack);

                case ExpressionType.Switch:
                    return this.RewriteSwitchExpression(node, stack);

                case ExpressionType.Throw:
                    return this.RewriteThrowUnaryExpression(node, stack);

                case ExpressionType.Try:
                    return this.RewriteTryExpression(node, stack);

                case ExpressionType.Unbox:
                    return this.RewriteUnaryExpression(node, stack);

                case ExpressionType.AddAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PostIncrementAssign:
                case ExpressionType.PostDecrementAssign:
                    return this.RewriteReducibleExpression(node, stack);

                case ExpressionType.TypeEqual:
                    return this.RewriteTypeBinaryExpression(node, stack);

                case ExpressionType.OnesComplement:
                    return this.RewriteUnaryExpression(node, stack);

                case ExpressionType.IsTrue:
                    return this.RewriteUnaryExpression(node, stack);

                case ExpressionType.IsFalse:
                    return this.RewriteUnaryExpression(node, stack);
            }
            throw ContractUtils.Unreachable;
        }

        private Result RewriteExpressionFreeTemps(Expression expression, Stack stack)
        {
            int mark = this.Mark();
            Result result = this.RewriteExpression(expression, stack);
            this.Free(mark);
            return result;
        }

        private Result RewriteExtensionAssignment(BinaryExpression node, Stack stack)
        {
            node = Expression.Assign(node.Left.ReduceExtensions(), node.Right);
            Result result = this.RewriteAssignBinaryExpression(node, stack);
            return new Result(result.Action | RewriteAction.Copy, result.Node);
        }

        private Result RewriteExtensionExpression(Expression expr, Stack stack)
        {
            Result result = this.RewriteExpression(expr.ReduceExtensions(), stack);
            return new Result(result.Action | RewriteAction.Copy, result.Node);
        }

        private Result RewriteGotoExpression(Expression expr, Stack stack)
        {
            GotoExpression expression = (GotoExpression) expr;
            Result result = this.RewriteExpressionFreeTemps(expression.Value, Stack.Empty);
            RewriteAction spillStack = result.Action;
            if (stack != Stack.Empty)
            {
                spillStack = RewriteAction.SpillStack;
            }
            if (spillStack != RewriteAction.None)
            {
                expr = Expression.MakeGoto(expression.Kind, expression.Target, result.Node, expression.Type);
            }
            return new Result(spillStack, expr);
        }

        private Result RewriteIndexAssignment(BinaryExpression node, Stack stack)
        {
            IndexExpression left = (IndexExpression) node.Left;
            ChildRewriter rewriter = new ChildRewriter(this, stack, 2 + left.Arguments.Count);
            rewriter.Add(left.Object);
            rewriter.Add(left.Arguments);
            rewriter.Add(node.Right);
            if (rewriter.Action == RewriteAction.SpillStack)
            {
                RequireNotRefInstance(left.Object);
            }
            if (rewriter.Rewrite)
            {
                node = new AssignBinaryExpression(new IndexExpression(rewriter[0], left.Indexer, rewriter[1, -2]), rewriter[-1]);
            }
            return rewriter.Finish(node);
        }

        private Result RewriteIndexExpression(Expression expr, Stack stack)
        {
            IndexExpression expression = (IndexExpression) expr;
            ChildRewriter rewriter = new ChildRewriter(this, stack, expression.Arguments.Count + 1);
            rewriter.Add(expression.Object);
            rewriter.Add(expression.Arguments);
            if (rewriter.Action == RewriteAction.SpillStack)
            {
                RequireNotRefInstance(expression.Object);
            }
            if (rewriter.Rewrite)
            {
                expr = new IndexExpression(rewriter[0], expression.Indexer, rewriter[1, -1]);
            }
            return rewriter.Finish(expr);
        }

        private Result RewriteInvocationExpression(Expression expr, Stack stack)
        {
            ChildRewriter rewriter;
            InvocationExpression expression = (InvocationExpression) expr;
            LambdaExpression lambdaOperand = expression.LambdaOperand;
            if (lambdaOperand != null)
            {
                rewriter = new ChildRewriter(this, stack, expression.Arguments.Count);
                rewriter.Add(expression.Arguments);
                if (rewriter.Action == RewriteAction.SpillStack)
                {
                    RequireNoRefArgs(Expression.GetInvokeMethod(expression.Expression));
                }
                StackSpiller spiller = new StackSpiller(stack);
                lambdaOperand = lambdaOperand.Accept(spiller);
                if (rewriter.Rewrite || (spiller._lambdaRewrite != RewriteAction.None))
                {
                    expression = new InvocationExpression(lambdaOperand, rewriter[0, -1], expression.Type);
                }
                Result result = rewriter.Finish(expression);
                return new Result(result.Action | spiller._lambdaRewrite, result.Node);
            }
            rewriter = new ChildRewriter(this, stack, expression.Arguments.Count + 1);
            rewriter.Add(expression.Expression);
            rewriter.Add(expression.Arguments);
            if (rewriter.Action == RewriteAction.SpillStack)
            {
                RequireNoRefArgs(Expression.GetInvokeMethod(expression.Expression));
            }
            return rewriter.Finish(rewriter.Rewrite ? new InvocationExpression(rewriter[0], rewriter[1, -1], expression.Type) : expr);
        }

        private Result RewriteLabelExpression(Expression expr, Stack stack)
        {
            LabelExpression expression = (LabelExpression) expr;
            Result result = this.RewriteExpression(expression.DefaultValue, stack);
            if (result.Action != RewriteAction.None)
            {
                expr = Expression.Label(expression.Target, result.Node);
            }
            return new Result(result.Action, expr);
        }

        private static Result RewriteLambdaExpression(Expression expr, Stack stack)
        {
            LambdaExpression lambda = (LambdaExpression) expr;
            expr = AnalyzeLambda(lambda);
            return new Result((expr == lambda) ? RewriteAction.None : RewriteAction.Copy, expr);
        }

        private Result RewriteListInitExpression(Expression expr, Stack stack)
        {
            ListInitExpression expression = (ListInitExpression) expr;
            Result result = this.RewriteExpression(expression.NewExpression, stack);
            Expression node = result.Node;
            RewriteAction action = result.Action;
            ReadOnlyCollection<ElementInit> initializers = expression.Initializers;
            ChildRewriter[] rewriterArray = new ChildRewriter[initializers.Count];
            for (int i = 0; i < initializers.Count; i++)
            {
                ElementInit init = initializers[i];
                ChildRewriter rewriter = new ChildRewriter(this, Stack.NonEmpty, init.Arguments.Count);
                rewriter.Add(init.Arguments);
                action |= rewriter.Action;
                rewriterArray[i] = rewriter;
            }
            switch (action)
            {
                case RewriteAction.None:
                    break;

                case RewriteAction.Copy:
                {
                    ElementInit[] list = new ElementInit[initializers.Count];
                    for (int j = 0; j < initializers.Count; j++)
                    {
                        ChildRewriter rewriter2 = rewriterArray[j];
                        if (rewriter2.Action == RewriteAction.None)
                        {
                            list[j] = initializers[j];
                        }
                        else
                        {
                            list[j] = Expression.ElementInit(initializers[j].AddMethod, rewriter2[0, -1]);
                        }
                    }
                    expr = Expression.ListInit((NewExpression) node, new TrueReadOnlyCollection<ElementInit>(list));
                    break;
                }
                case RewriteAction.SpillStack:
                {
                    RequireNotRefInstance(expression.NewExpression);
                    ParameterExpression left = this.MakeTemp(node.Type);
                    Expression[] expressions = new Expression[initializers.Count + 2];
                    expressions[0] = Expression.Assign(left, node);
                    for (int k = 0; k < initializers.Count; k++)
                    {
                        ChildRewriter rewriter3 = rewriterArray[k];
                        Result result2 = rewriter3.Finish(Expression.Call(left, initializers[k].AddMethod, rewriter3[0, -1]));
                        expressions[k + 1] = result2.Node;
                    }
                    expressions[initializers.Count + 1] = left;
                    expr = MakeBlock(expressions);
                    break;
                }
                default:
                    throw ContractUtils.Unreachable;
            }
            return new Result(action, expr);
        }

        private Result RewriteLogicalBinaryExpression(Expression expr, Stack stack)
        {
            BinaryExpression expression = (BinaryExpression) expr;
            Result result = this.RewriteExpression(expression.Left, stack);
            Result result2 = this.RewriteExpression(expression.Right, stack);
            Result result3 = this.RewriteExpression(expression.Conversion, stack);
            RewriteAction action = (result.Action | result2.Action) | result3.Action;
            if (action != RewriteAction.None)
            {
                expr = BinaryExpression.Create(expression.NodeType, result.Node, result2.Node, expression.Type, expression.Method, (LambdaExpression) result3.Node);
            }
            return new Result(action, expr);
        }

        private Result RewriteLoopExpression(Expression expr, Stack stack)
        {
            LoopExpression expression = (LoopExpression) expr;
            Result result = this.RewriteExpression(expression.Body, Stack.Empty);
            RewriteAction spillStack = result.Action;
            if (stack != Stack.Empty)
            {
                spillStack = RewriteAction.SpillStack;
            }
            if (spillStack != RewriteAction.None)
            {
                expr = new LoopExpression(result.Node, expression.BreakLabel, expression.ContinueLabel);
            }
            return new Result(spillStack, expr);
        }

        private Result RewriteMemberAssignment(BinaryExpression node, Stack stack)
        {
            MemberExpression left = (MemberExpression) node.Left;
            ChildRewriter rewriter = new ChildRewriter(this, stack, 2);
            rewriter.Add(left.Expression);
            rewriter.Add(node.Right);
            if (rewriter.Action == RewriteAction.SpillStack)
            {
                RequireNotRefInstance(left.Expression);
            }
            if (rewriter.Rewrite)
            {
                return rewriter.Finish(new AssignBinaryExpression(MemberExpression.Make(rewriter[0], left.Member), rewriter[1]));
            }
            return new Result(RewriteAction.None, node);
        }

        private Result RewriteMemberExpression(Expression expr, Stack stack)
        {
            MemberExpression expression = (MemberExpression) expr;
            Result result = this.RewriteExpression(expression.Expression, stack);
            if (result.Action != RewriteAction.None)
            {
                if ((result.Action == RewriteAction.SpillStack) && (expression.Member.MemberType == MemberTypes.Property))
                {
                    RequireNotRefInstance(expression.Expression);
                }
                expr = MemberExpression.Make(result.Node, expression.Member);
            }
            return new Result(result.Action, expr);
        }

        private Result RewriteMemberInitExpression(Expression expr, Stack stack)
        {
            MemberInitExpression expression = (MemberInitExpression) expr;
            Result result = this.RewriteExpression(expression.NewExpression, stack);
            Expression node = result.Node;
            RewriteAction action = result.Action;
            ReadOnlyCollection<MemberBinding> bindings = expression.Bindings;
            BindingRewriter[] rewriterArray = new BindingRewriter[bindings.Count];
            for (int i = 0; i < bindings.Count; i++)
            {
                MemberBinding binding = bindings[i];
                BindingRewriter rewriter = BindingRewriter.Create(binding, this, Stack.NonEmpty);
                rewriterArray[i] = rewriter;
                action |= rewriter.Action;
            }
            switch (action)
            {
                case RewriteAction.None:
                    break;

                case RewriteAction.Copy:
                {
                    MemberBinding[] list = new MemberBinding[bindings.Count];
                    for (int j = 0; j < bindings.Count; j++)
                    {
                        list[j] = rewriterArray[j].AsBinding();
                    }
                    expr = Expression.MemberInit((NewExpression) node, new TrueReadOnlyCollection<MemberBinding>(list));
                    break;
                }
                case RewriteAction.SpillStack:
                {
                    RequireNotRefInstance(expression.NewExpression);
                    ParameterExpression left = this.MakeTemp(node.Type);
                    Expression[] expressions = new Expression[bindings.Count + 2];
                    expressions[0] = Expression.Assign(left, node);
                    for (int k = 0; k < bindings.Count; k++)
                    {
                        Expression expression4 = rewriterArray[k].AsExpression(left);
                        expressions[k + 1] = expression4;
                    }
                    expressions[bindings.Count + 1] = left;
                    expr = MakeBlock(expressions);
                    break;
                }
                default:
                    throw ContractUtils.Unreachable;
            }
            return new Result(action, expr);
        }

        private Result RewriteMethodCallExpression(Expression expr, Stack stack)
        {
            MethodCallExpression expressions = (MethodCallExpression) expr;
            ChildRewriter rewriter = new ChildRewriter(this, stack, expressions.Arguments.Count + 1);
            rewriter.Add(expressions.Object);
            rewriter.AddArguments(expressions);
            if (rewriter.Action == RewriteAction.SpillStack)
            {
                RequireNotRefInstance(expressions.Object);
                RequireNoRefArgs(expressions.Method);
            }
            return rewriter.Finish(rewriter.Rewrite ? expressions.Rewrite(rewriter[0], rewriter[1, -1]) : expr);
        }

        private Result RewriteNewArrayExpression(Expression expr, Stack stack)
        {
            NewArrayExpression expression = (NewArrayExpression) expr;
            if (expression.NodeType == ExpressionType.NewArrayInit)
            {
                stack = Stack.NonEmpty;
            }
            ChildRewriter rewriter = new ChildRewriter(this, stack, expression.Expressions.Count);
            rewriter.Add(expression.Expressions);
            if (rewriter.Rewrite)
            {
                Type elementType = expression.Type.GetElementType();
                if (expression.NodeType == ExpressionType.NewArrayInit)
                {
                    expr = Expression.NewArrayInit(elementType, rewriter[0, -1]);
                }
                else
                {
                    expr = Expression.NewArrayBounds(elementType, rewriter[0, -1]);
                }
            }
            return rewriter.Finish(expr);
        }

        private Result RewriteNewExpression(Expression expr, Stack stack)
        {
            NewExpression expressions = (NewExpression) expr;
            ChildRewriter rewriter = new ChildRewriter(this, stack, expressions.Arguments.Count);
            rewriter.AddArguments(expressions);
            if (rewriter.Action == RewriteAction.SpillStack)
            {
                RequireNoRefArgs(expressions.Constructor);
            }
            return rewriter.Finish(rewriter.Rewrite ? new NewExpression(expressions.Constructor, rewriter[0, -1], expressions.Members) : expr);
        }

        private Result RewriteReducibleExpression(Expression expr, Stack stack)
        {
            Result result = this.RewriteExpression(expr.Reduce(), stack);
            return new Result(result.Action | RewriteAction.Copy, result.Node);
        }

        private Result RewriteSwitchExpression(Expression expr, Stack stack)
        {
            SwitchExpression expression = (SwitchExpression) expr;
            Result result = this.RewriteExpressionFreeTemps(expression.SwitchValue, stack);
            RewriteAction action = result.Action;
            ReadOnlyCollection<SwitchCase> cases = expression.Cases;
            SwitchCase[] list = null;
            for (int i = 0; i < cases.Count; i++)
            {
                SwitchCase @case = cases[i];
                Expression[] expressionArray = null;
                ReadOnlyCollection<Expression> testValues = @case.TestValues;
                for (int j = 0; j < testValues.Count; j++)
                {
                    Result result2 = this.RewriteExpression(testValues[j], stack);
                    action |= result2.Action;
                    if ((expressionArray == null) && (result2.Action != RewriteAction.None))
                    {
                        expressionArray = Clone<Expression>(testValues, j);
                    }
                    if (expressionArray != null)
                    {
                        expressionArray[j] = result2.Node;
                    }
                }
                Result result3 = this.RewriteExpression(@case.Body, stack);
                action |= result3.Action;
                if ((result3.Action != RewriteAction.None) || (expressionArray != null))
                {
                    if (expressionArray != null)
                    {
                        testValues = new ReadOnlyCollection<Expression>(expressionArray);
                    }
                    @case = new SwitchCase(result3.Node, testValues);
                    if (list == null)
                    {
                        list = Clone<SwitchCase>(cases, i);
                    }
                }
                if (list != null)
                {
                    list[i] = @case;
                }
            }
            Result result4 = this.RewriteExpression(expression.DefaultBody, stack);
            action |= result4.Action;
            if (action != RewriteAction.None)
            {
                if (list != null)
                {
                    cases = new ReadOnlyCollection<SwitchCase>(list);
                }
                expr = new SwitchExpression(expression.Type, result.Node, result4.Node, expression.Comparison, cases);
            }
            return new Result(action, expr);
        }

        private Result RewriteThrowUnaryExpression(Expression expr, Stack stack)
        {
            UnaryExpression expression = (UnaryExpression) expr;
            Result result = this.RewriteExpressionFreeTemps(expression.Operand, Stack.Empty);
            RewriteAction spillStack = result.Action;
            if (stack != Stack.Empty)
            {
                spillStack = RewriteAction.SpillStack;
            }
            if (spillStack != RewriteAction.None)
            {
                expr = Expression.Throw(result.Node, expression.Type);
            }
            return new Result(spillStack, expr);
        }

        private Result RewriteTryExpression(Expression expr, Stack stack)
        {
            TryExpression expression = (TryExpression) expr;
            Result result = this.RewriteExpression(expression.Body, Stack.Empty);
            ReadOnlyCollection<CatchBlock> handlers = expression.Handlers;
            CatchBlock[] list = null;
            RewriteAction spillStack = result.Action;
            if (handlers != null)
            {
                for (int i = 0; i < handlers.Count; i++)
                {
                    RewriteAction action2 = result.Action;
                    CatchBlock block = handlers[i];
                    Expression filter = block.Filter;
                    if (block.Filter != null)
                    {
                        Result result2 = this.RewriteExpression(block.Filter, Stack.Empty);
                        spillStack |= result2.Action;
                        action2 |= result2.Action;
                        filter = result2.Node;
                    }
                    Result result3 = this.RewriteExpression(block.Body, Stack.Empty);
                    spillStack |= result3.Action;
                    if ((action2 | result3.Action) != RewriteAction.None)
                    {
                        block = Expression.MakeCatchBlock(block.Test, block.Variable, result3.Node, filter);
                        if (list == null)
                        {
                            list = Clone<CatchBlock>(handlers, i);
                        }
                    }
                    if (list != null)
                    {
                        list[i] = block;
                    }
                }
            }
            Result result4 = this.RewriteExpression(expression.Fault, Stack.Empty);
            spillStack |= result4.Action;
            Result result5 = this.RewriteExpression(expression.Finally, Stack.Empty);
            spillStack |= result5.Action;
            if (stack != Stack.Empty)
            {
                spillStack = RewriteAction.SpillStack;
            }
            if (spillStack != RewriteAction.None)
            {
                if (list != null)
                {
                    handlers = new ReadOnlyCollection<CatchBlock>(list);
                }
                expr = new TryExpression(expression.Type, result.Node, result5.Node, result4.Node, handlers);
            }
            return new Result(spillStack, expr);
        }

        private Result RewriteTypeBinaryExpression(Expression expr, Stack stack)
        {
            TypeBinaryExpression expression = (TypeBinaryExpression) expr;
            Result result = this.RewriteExpression(expression.Expression, stack);
            if (result.Action != RewriteAction.None)
            {
                if (expression.NodeType == ExpressionType.TypeIs)
                {
                    expr = Expression.TypeIs(result.Node, expression.TypeOperand);
                }
                else
                {
                    expr = Expression.TypeEqual(result.Node, expression.TypeOperand);
                }
            }
            return new Result(result.Action, expr);
        }

        private Result RewriteUnaryExpression(Expression expr, Stack stack)
        {
            UnaryExpression expression = (UnaryExpression) expr;
            Result result = this.RewriteExpression(expression.Operand, stack);
            if (result.Action == RewriteAction.SpillStack)
            {
                RequireNoRefArgs(expression.Method);
            }
            if (result.Action != RewriteAction.None)
            {
                expr = new UnaryExpression(expression.NodeType, result.Node, expression.Type, expression.Method);
            }
            return new Result(result.Action, expr);
        }

        private Result RewriteVariableAssignment(BinaryExpression node, Stack stack)
        {
            Result result = this.RewriteExpression(node.Right, stack);
            if (result.Action != RewriteAction.None)
            {
                node = Expression.Assign(node.Left, result.Node);
            }
            return new Result(result.Action, node);
        }

        private ParameterExpression ToTemp(Expression expression, out Expression save)
        {
            ParameterExpression left = this.MakeTemp(expression.Type);
            save = Expression.Assign(left, expression);
            return left;
        }

        [Conditional("DEBUG")]
        private static void VerifyRewrite(Result result, Expression node)
        {
        }

        [Conditional("DEBUG")]
        private void VerifyTemps()
        {
        }

        private abstract class BindingRewriter
        {
            protected StackSpiller.RewriteAction _action;
            protected MemberBinding _binding;
            protected StackSpiller _spiller;

            internal BindingRewriter(MemberBinding binding, StackSpiller spiller)
            {
                this._binding = binding;
                this._spiller = spiller;
            }

            internal abstract MemberBinding AsBinding();
            internal abstract Expression AsExpression(Expression target);
            internal static StackSpiller.BindingRewriter Create(MemberBinding binding, StackSpiller spiller, StackSpiller.Stack stack)
            {
                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        return new StackSpiller.MemberAssignmentRewriter((MemberAssignment) binding, spiller, stack);

                    case MemberBindingType.MemberBinding:
                        return new StackSpiller.MemberMemberBindingRewriter((MemberMemberBinding) binding, spiller, stack);

                    case MemberBindingType.ListBinding:
                        return new StackSpiller.ListBindingRewriter((MemberListBinding) binding, spiller, stack);
                }
                throw System.Linq.Expressions.Error.UnhandledBinding();
            }

            internal StackSpiller.RewriteAction Action
            {
                get
                {
                    return this._action;
                }
            }
        }

        private class ChildRewriter
        {
            private StackSpiller.RewriteAction _action;
            private List<Expression> _comma;
            private bool _done;
            private readonly Expression[] _expressions;
            private int _expressionsCount;
            private readonly StackSpiller _self;
            private StackSpiller.Stack _stack;

            internal ChildRewriter(StackSpiller self, StackSpiller.Stack stack, int count)
            {
                this._self = self;
                this._stack = stack;
                this._expressions = new Expression[count];
            }

            internal void Add(IList<Expression> expressions)
            {
                int num = 0;
                int count = expressions.Count;
                while (num < count)
                {
                    this.Add(expressions[num]);
                    num++;
                }
            }

            internal void Add(Expression node)
            {
                if (node == null)
                {
                    this._expressions[this._expressionsCount++] = null;
                }
                else
                {
                    StackSpiller.Result result = this._self.RewriteExpression(node, this._stack);
                    this._action |= result.Action;
                    this._stack = StackSpiller.Stack.NonEmpty;
                    this._expressions[this._expressionsCount++] = result.Node;
                }
            }

            internal void AddArguments(IArgumentProvider expressions)
            {
                int index = 0;
                int argumentCount = expressions.ArgumentCount;
                while (index < argumentCount)
                {
                    this.Add(expressions.GetArgument(index));
                    index++;
                }
            }

            private void EnsureDone()
            {
                if (!this._done)
                {
                    this._done = true;
                    if (this._action == StackSpiller.RewriteAction.SpillStack)
                    {
                        Expression[] expressionArray = this._expressions;
                        int length = expressionArray.Length;
                        List<Expression> list = new List<Expression>(length + 1);
                        for (int i = 0; i < length; i++)
                        {
                            if (expressionArray[i] != null)
                            {
                                Expression expression;
                                expressionArray[i] = this._self.ToTemp(expressionArray[i], out expression);
                                list.Add(expression);
                            }
                        }
                        list.Capacity = list.Count + 1;
                        this._comma = list;
                    }
                }
            }

            internal StackSpiller.Result Finish(Expression expr)
            {
                this.EnsureDone();
                if (this._action == StackSpiller.RewriteAction.SpillStack)
                {
                    this._comma.Add(expr);
                    expr = StackSpiller.MakeBlock(this._comma);
                }
                return new StackSpiller.Result(this._action, expr);
            }

            internal StackSpiller.RewriteAction Action
            {
                get
                {
                    return this._action;
                }
            }

            internal Expression this[int index]
            {
                get
                {
                    this.EnsureDone();
                    if (index < 0)
                    {
                        index += this._expressions.Length;
                    }
                    return this._expressions[index];
                }
            }

            internal Expression[] this[int first, int last]
            {
                get
                {
                    this.EnsureDone();
                    if (last < 0)
                    {
                        last += this._expressions.Length;
                    }
                    int count = (last - first) + 1;
                    ContractUtils.RequiresArrayRange<Expression>(this._expressions, first, count, "first", "last");
                    if (count == this._expressions.Length)
                    {
                        return this._expressions;
                    }
                    Expression[] destinationArray = new Expression[count];
                    Array.Copy(this._expressions, first, destinationArray, 0, count);
                    return destinationArray;
                }
            }

            internal bool Rewrite
            {
                get
                {
                    return (this._action != StackSpiller.RewriteAction.None);
                }
            }
        }

        private class ListBindingRewriter : StackSpiller.BindingRewriter
        {
            private StackSpiller.ChildRewriter[] _childRewriters;
            private ReadOnlyCollection<ElementInit> _inits;

            internal ListBindingRewriter(MemberListBinding binding, StackSpiller spiller, StackSpiller.Stack stack) : base(binding, spiller)
            {
                this._inits = binding.Initializers;
                this._childRewriters = new StackSpiller.ChildRewriter[this._inits.Count];
                for (int i = 0; i < this._inits.Count; i++)
                {
                    ElementInit init = this._inits[i];
                    StackSpiller.ChildRewriter rewriter = new StackSpiller.ChildRewriter(spiller, stack, init.Arguments.Count);
                    rewriter.Add(init.Arguments);
                    base._action |= rewriter.Action;
                    this._childRewriters[i] = rewriter;
                }
            }

            internal override MemberBinding AsBinding()
            {
                switch (base._action)
                {
                    case StackSpiller.RewriteAction.None:
                        return base._binding;

                    case StackSpiller.RewriteAction.Copy:
                    {
                        ElementInit[] list = new ElementInit[this._inits.Count];
                        for (int i = 0; i < this._inits.Count; i++)
                        {
                            StackSpiller.ChildRewriter rewriter = this._childRewriters[i];
                            if (rewriter.Action == StackSpiller.RewriteAction.None)
                            {
                                list[i] = this._inits[i];
                            }
                            else
                            {
                                list[i] = Expression.ElementInit(this._inits[i].AddMethod, rewriter[0, -1]);
                            }
                        }
                        return Expression.ListBind(base._binding.Member, new TrueReadOnlyCollection<ElementInit>(list));
                    }
                }
                throw ContractUtils.Unreachable;
            }

            internal override Expression AsExpression(Expression target)
            {
                if (target.Type.IsValueType && (base._binding.Member is PropertyInfo))
                {
                    throw System.Linq.Expressions.Error.CannotAutoInitializeValueTypeElementThroughProperty(base._binding.Member);
                }
                StackSpiller.RequireNotRefInstance(target);
                MemberExpression right = Expression.MakeMemberAccess(target, base._binding.Member);
                ParameterExpression left = base._spiller.MakeTemp(right.Type);
                Expression[] expressions = new Expression[this._inits.Count + 2];
                expressions[0] = Expression.Assign(left, right);
                for (int i = 0; i < this._inits.Count; i++)
                {
                    StackSpiller.ChildRewriter rewriter = this._childRewriters[i];
                    StackSpiller.Result result = rewriter.Finish(Expression.Call(left, this._inits[i].AddMethod, rewriter[0, -1]));
                    expressions[i + 1] = result.Node;
                }
                if (left.Type.IsValueType)
                {
                    expressions[this._inits.Count + 1] = Expression.Block(typeof(void), new Expression[] { Expression.Assign(Expression.MakeMemberAccess(target, base._binding.Member), left) });
                }
                else
                {
                    expressions[this._inits.Count + 1] = Expression.Empty();
                }
                return StackSpiller.MakeBlock(expressions);
            }
        }

        private class MemberAssignmentRewriter : StackSpiller.BindingRewriter
        {
            private Expression _rhs;

            internal MemberAssignmentRewriter(MemberAssignment binding, StackSpiller spiller, StackSpiller.Stack stack) : base(binding, spiller)
            {
                StackSpiller.Result result = spiller.RewriteExpression(binding.Expression, stack);
                base._action = result.Action;
                this._rhs = result.Node;
            }

            internal override MemberBinding AsBinding()
            {
                switch (base._action)
                {
                    case StackSpiller.RewriteAction.None:
                        return base._binding;

                    case StackSpiller.RewriteAction.Copy:
                        return Expression.Bind(base._binding.Member, this._rhs);
                }
                throw ContractUtils.Unreachable;
            }

            internal override Expression AsExpression(Expression target)
            {
                StackSpiller.RequireNotRefInstance(target);
                MemberExpression left = Expression.MakeMemberAccess(target, base._binding.Member);
                ParameterExpression expression2 = base._spiller.MakeTemp(left.Type);
                return StackSpiller.MakeBlock(new Expression[] { Expression.Assign(expression2, this._rhs), Expression.Assign(left, expression2), Expression.Empty() });
            }
        }

        private class MemberMemberBindingRewriter : StackSpiller.BindingRewriter
        {
            private StackSpiller.BindingRewriter[] _bindingRewriters;
            private ReadOnlyCollection<MemberBinding> _bindings;

            internal MemberMemberBindingRewriter(MemberMemberBinding binding, StackSpiller spiller, StackSpiller.Stack stack) : base(binding, spiller)
            {
                this._bindings = binding.Bindings;
                this._bindingRewriters = new StackSpiller.BindingRewriter[this._bindings.Count];
                for (int i = 0; i < this._bindings.Count; i++)
                {
                    StackSpiller.BindingRewriter rewriter = StackSpiller.BindingRewriter.Create(this._bindings[i], spiller, stack);
                    base._action |= rewriter.Action;
                    this._bindingRewriters[i] = rewriter;
                }
            }

            internal override MemberBinding AsBinding()
            {
                switch (base._action)
                {
                    case StackSpiller.RewriteAction.None:
                        return base._binding;

                    case StackSpiller.RewriteAction.Copy:
                    {
                        MemberBinding[] list = new MemberBinding[this._bindings.Count];
                        for (int i = 0; i < this._bindings.Count; i++)
                        {
                            list[i] = this._bindingRewriters[i].AsBinding();
                        }
                        return Expression.MemberBind(base._binding.Member, new TrueReadOnlyCollection<MemberBinding>(list));
                    }
                }
                throw ContractUtils.Unreachable;
            }

            internal override Expression AsExpression(Expression target)
            {
                if (target.Type.IsValueType && (base._binding.Member is PropertyInfo))
                {
                    throw System.Linq.Expressions.Error.CannotAutoInitializeValueTypeMemberThroughProperty(base._binding.Member);
                }
                StackSpiller.RequireNotRefInstance(target);
                MemberExpression right = Expression.MakeMemberAccess(target, base._binding.Member);
                ParameterExpression left = base._spiller.MakeTemp(right.Type);
                Expression[] expressions = new Expression[this._bindings.Count + 2];
                expressions[0] = Expression.Assign(left, right);
                for (int i = 0; i < this._bindings.Count; i++)
                {
                    expressions[i + 1] = this._bindingRewriters[i].AsExpression(left);
                }
                if (left.Type.IsValueType)
                {
                    expressions[this._bindings.Count + 1] = Expression.Block(typeof(void), new Expression[] { Expression.Assign(Expression.MakeMemberAccess(target, base._binding.Member), left) });
                }
                else
                {
                    expressions[this._bindings.Count + 1] = Expression.Empty();
                }
                return StackSpiller.MakeBlock(expressions);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Result
        {
            internal readonly StackSpiller.RewriteAction Action;
            internal readonly Expression Node;
            internal Result(StackSpiller.RewriteAction action, Expression node)
            {
                this.Action = action;
                this.Node = node;
            }
        }

        [Flags]
        private enum RewriteAction
        {
            Copy = 1,
            None = 0,
            SpillStack = 3
        }

        private enum Stack
        {
            Empty,
            NonEmpty
        }

        private class TempMaker
        {
            private List<ParameterExpression> _freeTemps;
            private int _temp;
            private List<ParameterExpression> _temps = new List<ParameterExpression>();
            private Stack<ParameterExpression> _usedTemps;

            internal void Free(int mark)
            {
                if (this._usedTemps != null)
                {
                    while (mark < this._usedTemps.Count)
                    {
                        this.FreeTemp(this._usedTemps.Pop());
                    }
                }
            }

            private void FreeTemp(ParameterExpression temp)
            {
                if (this._freeTemps == null)
                {
                    this._freeTemps = new List<ParameterExpression>();
                }
                this._freeTemps.Add(temp);
            }

            internal int Mark()
            {
                if (this._usedTemps == null)
                {
                    return 0;
                }
                return this._usedTemps.Count;
            }

            internal ParameterExpression Temp(Type type)
            {
                ParameterExpression expression;
                if (this._freeTemps != null)
                {
                    for (int i = this._freeTemps.Count - 1; i >= 0; i--)
                    {
                        expression = this._freeTemps[i];
                        if (expression.Type == type)
                        {
                            this._freeTemps.RemoveAt(i);
                            return this.UseTemp(expression);
                        }
                    }
                }
                expression = Expression.Variable(type, "$temp$" + this._temp++);
                this._temps.Add(expression);
                return this.UseTemp(expression);
            }

            private ParameterExpression UseTemp(ParameterExpression temp)
            {
                if (this._usedTemps == null)
                {
                    this._usedTemps = new Stack<ParameterExpression>();
                }
                this._usedTemps.Push(temp);
                return temp;
            }

            [Conditional("DEBUG")]
            internal void VerifyTemps()
            {
            }

            internal List<ParameterExpression> Temps
            {
                get
                {
                    return this._temps;
                }
            }
        }
    }
}

