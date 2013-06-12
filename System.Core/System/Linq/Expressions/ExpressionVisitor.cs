namespace System.Linq.Expressions
{
    using System;
    using System.Collections.ObjectModel;
    using System.Dynamic.Utils;
    using System.Runtime.CompilerServices;

    public abstract class ExpressionVisitor
    {
        protected ExpressionVisitor()
        {
        }

        private static BinaryExpression ValidateBinary(BinaryExpression before, BinaryExpression after)
        {
            if ((before != after) && (before.Method == null))
            {
                if (after.Method != null)
                {
                    throw Error.MustRewriteWithoutMethod(after.Method, "VisitBinary");
                }
                ValidateChildType(before.Left.Type, after.Left.Type, "VisitBinary");
                ValidateChildType(before.Right.Type, after.Right.Type, "VisitBinary");
            }
            return after;
        }

        private static void ValidateChildType(Type before, Type after, string methodName)
        {
            if (before.IsValueType)
            {
                if (TypeUtils.AreEquivalent(before, after))
                {
                    return;
                }
            }
            else if (!after.IsValueType)
            {
                return;
            }
            throw Error.MustRewriteChildToSameType(before, after, methodName);
        }

        private static SwitchExpression ValidateSwitch(SwitchExpression before, SwitchExpression after)
        {
            if ((before.Comparison == null) && (after.Comparison != null))
            {
                throw Error.MustRewriteWithoutMethod(after.Comparison, "VisitSwitch");
            }
            return after;
        }

        private static UnaryExpression ValidateUnary(UnaryExpression before, UnaryExpression after)
        {
            if ((before != after) && (before.Method == null))
            {
                if (after.Method != null)
                {
                    throw Error.MustRewriteWithoutMethod(after.Method, "VisitUnary");
                }
                if ((before.Operand != null) && (after.Operand != null))
                {
                    ValidateChildType(before.Operand.Type, after.Operand.Type, "VisitUnary");
                }
            }
            return after;
        }

        public ReadOnlyCollection<Expression> Visit(ReadOnlyCollection<Expression> nodes)
        {
            Expression[] list = null;
            int index = 0;
            int count = nodes.Count;
            while (index < count)
            {
                Expression objA = this.Visit(nodes[index]);
                if (list != null)
                {
                    list[index] = objA;
                }
                else if (!object.ReferenceEquals(objA, nodes[index]))
                {
                    list = new Expression[count];
                    for (int i = 0; i < index; i++)
                    {
                        list[i] = nodes[i];
                    }
                    list[index] = objA;
                }
                index++;
            }
            if (list == null)
            {
                return nodes;
            }
            return new TrueReadOnlyCollection<Expression>(list);
        }

        public virtual Expression Visit(Expression node)
        {
            if (node != null)
            {
                return node.Accept(this);
            }
            return null;
        }

        public static ReadOnlyCollection<T> Visit<T>(ReadOnlyCollection<T> nodes, Func<T, T> elementVisitor)
        {
            T[] list = null;
            int index = 0;
            int count = nodes.Count;
            while (index < count)
            {
                T objA = elementVisitor(nodes[index]);
                if (list != null)
                {
                    list[index] = objA;
                }
                else if (!object.ReferenceEquals(objA, nodes[index]))
                {
                    list = new T[count];
                    for (int i = 0; i < index; i++)
                    {
                        list[i] = nodes[i];
                    }
                    list[index] = objA;
                }
                index++;
            }
            if (list == null)
            {
                return nodes;
            }
            return new TrueReadOnlyCollection<T>(list);
        }

        public T VisitAndConvert<T>(T node, string callerName) where T: Expression
        {
            if (node == null)
            {
                return default(T);
            }
            node = this.Visit(node) as T;
            if (node == null)
            {
                throw Error.MustRewriteToSameNode(callerName, typeof(T), callerName);
            }
            return node;
        }

        public ReadOnlyCollection<T> VisitAndConvert<T>(ReadOnlyCollection<T> nodes, string callerName) where T: Expression
        {
            T[] list = null;
            int index = 0;
            int count = nodes.Count;
            while (index < count)
            {
                T objA = this.Visit(nodes[index]) as T;
                if (objA == null)
                {
                    throw Error.MustRewriteToSameNode(callerName, typeof(T), callerName);
                }
                if (list != null)
                {
                    list[index] = objA;
                }
                else if (!object.ReferenceEquals(objA, nodes[index]))
                {
                    list = new T[count];
                    for (int i = 0; i < index; i++)
                    {
                        list[i] = nodes[i];
                    }
                    list[index] = objA;
                }
                index++;
            }
            if (list == null)
            {
                return nodes;
            }
            return new TrueReadOnlyCollection<T>(list);
        }

        internal Expression[] VisitArguments(IArgumentProvider nodes)
        {
            Expression[] expressionArray = null;
            int index = 0;
            int argumentCount = nodes.ArgumentCount;
            while (index < argumentCount)
            {
                Expression argument = nodes.GetArgument(index);
                Expression objA = this.Visit(argument);
                if (expressionArray != null)
                {
                    expressionArray[index] = objA;
                }
                else if (!object.ReferenceEquals(objA, argument))
                {
                    expressionArray = new Expression[argumentCount];
                    for (int i = 0; i < index; i++)
                    {
                        expressionArray[i] = nodes.GetArgument(i);
                    }
                    expressionArray[index] = objA;
                }
                index++;
            }
            return expressionArray;
        }

        protected internal virtual Expression VisitBinary(BinaryExpression node)
        {
            return ValidateBinary(node, node.Update(this.Visit(node.Left), this.VisitAndConvert<LambdaExpression>(node.Conversion, "VisitBinary"), this.Visit(node.Right)));
        }

        protected internal virtual Expression VisitBlock(BlockExpression node)
        {
            int expressionCount = node.ExpressionCount;
            Expression[] args = null;
            for (int i = 0; i < expressionCount; i++)
            {
                Expression expression = node.GetExpression(i);
                Expression expression2 = this.Visit(expression);
                if (expression != expression2)
                {
                    if (args == null)
                    {
                        args = new Expression[expressionCount];
                    }
                    args[i] = expression2;
                }
            }
            ReadOnlyCollection<ParameterExpression> variables = this.VisitAndConvert<ParameterExpression>(node.Variables, "VisitBlock");
            if ((variables == node.Variables) && (args == null))
            {
                return node;
            }
            for (int j = 0; j < expressionCount; j++)
            {
                if (args[j] == null)
                {
                    args[j] = node.GetExpression(j);
                }
            }
            return node.Rewrite(variables, args);
        }

        protected virtual CatchBlock VisitCatchBlock(CatchBlock node)
        {
            return node.Update(this.VisitAndConvert<ParameterExpression>(node.Variable, "VisitCatchBlock"), this.Visit(node.Filter), this.Visit(node.Body));
        }

        protected internal virtual Expression VisitConditional(ConditionalExpression node)
        {
            return node.Update(this.Visit(node.Test), this.Visit(node.IfTrue), this.Visit(node.IfFalse));
        }

        protected internal virtual Expression VisitConstant(ConstantExpression node)
        {
            return node;
        }

        protected internal virtual Expression VisitDebugInfo(DebugInfoExpression node)
        {
            return node;
        }

        protected internal virtual Expression VisitDefault(DefaultExpression node)
        {
            return node;
        }

        protected internal virtual Expression VisitDynamic(DynamicExpression node)
        {
            Expression[] args = this.VisitArguments(node);
            if (args == null)
            {
                return node;
            }
            return node.Rewrite(args);
        }

        protected virtual ElementInit VisitElementInit(ElementInit node)
        {
            return node.Update(this.Visit(node.Arguments));
        }

        protected internal virtual Expression VisitExtension(Expression node)
        {
            return node.VisitChildren(this);
        }

        protected internal virtual Expression VisitGoto(GotoExpression node)
        {
            return node.Update(this.VisitLabelTarget(node.Target), this.Visit(node.Value));
        }

        protected internal virtual Expression VisitIndex(IndexExpression node)
        {
            Expression instance = this.Visit(node.Object);
            Expression[] arguments = this.VisitArguments(node);
            if ((instance == node.Object) && (arguments == null))
            {
                return node;
            }
            return node.Rewrite(instance, arguments);
        }

        protected internal virtual Expression VisitInvocation(InvocationExpression node)
        {
            Expression lambda = this.Visit(node.Expression);
            Expression[] arguments = this.VisitArguments(node);
            if ((lambda == node.Expression) && (arguments == null))
            {
                return node;
            }
            return node.Rewrite(lambda, arguments);
        }

        protected internal virtual Expression VisitLabel(LabelExpression node)
        {
            return node.Update(this.VisitLabelTarget(node.Target), this.Visit(node.DefaultValue));
        }

        protected virtual LabelTarget VisitLabelTarget(LabelTarget node)
        {
            return node;
        }

        protected internal virtual Expression VisitLambda<T>(Expression<T> node)
        {
            return node.Update(this.Visit(node.Body), this.VisitAndConvert<ParameterExpression>(node.Parameters, "VisitLambda"));
        }

        protected internal virtual Expression VisitListInit(ListInitExpression node)
        {
            return node.Update(this.VisitAndConvert<NewExpression>(node.NewExpression, "VisitListInit"), Visit<ElementInit>(node.Initializers, new Func<ElementInit, ElementInit>(this.VisitElementInit)));
        }

        protected internal virtual Expression VisitLoop(LoopExpression node)
        {
            return node.Update(this.VisitLabelTarget(node.BreakLabel), this.VisitLabelTarget(node.ContinueLabel), this.Visit(node.Body));
        }

        protected internal virtual Expression VisitMember(MemberExpression node)
        {
            return node.Update(this.Visit(node.Expression));
        }

        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            return node.Update(this.Visit(node.Expression));
        }

        protected virtual MemberBinding VisitMemberBinding(MemberBinding node)
        {
            switch (node.BindingType)
            {
                case MemberBindingType.Assignment:
                    return this.VisitMemberAssignment((MemberAssignment) node);

                case MemberBindingType.MemberBinding:
                    return this.VisitMemberMemberBinding((MemberMemberBinding) node);

                case MemberBindingType.ListBinding:
                    return this.VisitMemberListBinding((MemberListBinding) node);
            }
            throw Error.UnhandledBindingType(node.BindingType);
        }

        protected internal virtual Expression VisitMemberInit(MemberInitExpression node)
        {
            return node.Update(this.VisitAndConvert<NewExpression>(node.NewExpression, "VisitMemberInit"), Visit<MemberBinding>(node.Bindings, new Func<MemberBinding, MemberBinding>(this.VisitMemberBinding)));
        }

        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            return node.Update(Visit<ElementInit>(node.Initializers, new Func<ElementInit, ElementInit>(this.VisitElementInit)));
        }

        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            return node.Update(Visit<MemberBinding>(node.Bindings, new Func<MemberBinding, MemberBinding>(this.VisitMemberBinding)));
        }

        protected internal virtual Expression VisitMethodCall(MethodCallExpression node)
        {
            Expression instance = this.Visit(node.Object);
            Expression[] args = this.VisitArguments(node);
            if ((instance == node.Object) && (args == null))
            {
                return node;
            }
            return node.Rewrite(instance, args);
        }

        protected internal virtual Expression VisitNew(NewExpression node)
        {
            return node.Update(this.Visit(node.Arguments));
        }

        protected internal virtual Expression VisitNewArray(NewArrayExpression node)
        {
            return node.Update(this.Visit(node.Expressions));
        }

        protected internal virtual Expression VisitParameter(ParameterExpression node)
        {
            return node;
        }

        protected internal virtual Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            return node.Update(this.VisitAndConvert<ParameterExpression>(node.Variables, "VisitRuntimeVariables"));
        }

        protected internal virtual Expression VisitSwitch(SwitchExpression node)
        {
            return ValidateSwitch(node, node.Update(this.Visit(node.SwitchValue), Visit<SwitchCase>(node.Cases, new Func<SwitchCase, SwitchCase>(this.VisitSwitchCase)), this.Visit(node.DefaultBody)));
        }

        protected virtual SwitchCase VisitSwitchCase(SwitchCase node)
        {
            return node.Update(this.Visit(node.TestValues), this.Visit(node.Body));
        }

        protected internal virtual Expression VisitTry(TryExpression node)
        {
            return node.Update(this.Visit(node.Body), Visit<CatchBlock>(node.Handlers, new Func<CatchBlock, CatchBlock>(this.VisitCatchBlock)), this.Visit(node.Finally), this.Visit(node.Fault));
        }

        protected internal virtual Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            return node.Update(this.Visit(node.Expression));
        }

        protected internal virtual Expression VisitUnary(UnaryExpression node)
        {
            return ValidateUnary(node, node.Update(this.Visit(node.Operand)));
        }
    }
}

