namespace System.Linq.Expressions.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;

    internal sealed class VariableBinder : ExpressionVisitor
    {
        private readonly Stack<BoundConstants> _constants = new Stack<BoundConstants>();
        private bool _inQuote;
        private readonly Stack<CompilerScope> _scopes = new Stack<CompilerScope>();
        private readonly AnalyzedTree _tree = new AnalyzedTree();

        private VariableBinder()
        {
        }

        internal static AnalyzedTree Bind(LambdaExpression lambda)
        {
            VariableBinder binder = new VariableBinder();
            binder.Visit(lambda);
            return binder._tree;
        }

        private ReadOnlyCollection<Expression> MergeScopes(Expression node)
        {
            ReadOnlyCollection<Expression> expressions;
            LambdaExpression expression = node as LambdaExpression;
            if (expression != null)
            {
                expressions = new ReadOnlyCollection<Expression>(new Expression[] { expression.Body });
            }
            else
            {
                expressions = ((BlockExpression) node).Expressions;
            }
            CompilerScope scope = this._scopes.Peek();
            while ((expressions.Count == 1) && (expressions[0].NodeType == ExpressionType.Block))
            {
                BlockExpression item = (BlockExpression) expressions[0];
                if (item.Variables.Count > 0)
                {
                    foreach (ParameterExpression expression3 in item.Variables)
                    {
                        if (scope.Definitions.ContainsKey(expression3))
                        {
                            return expressions;
                        }
                    }
                    if (scope.MergedScopes == null)
                    {
                        scope.MergedScopes = new Set<object>(ReferenceEqualityComparer<object>.Instance);
                    }
                    scope.MergedScopes.Add(item);
                    foreach (ParameterExpression expression4 in item.Variables)
                    {
                        scope.Definitions.Add(expression4, VariableStorageKind.Local);
                    }
                }
                node = item;
                expressions = item.Expressions;
            }
            return expressions;
        }

        private void Reference(ParameterExpression node, VariableStorageKind storage)
        {
            CompilerScope scope = null;
            foreach (CompilerScope scope2 in this._scopes)
            {
                if (scope2.Definitions.ContainsKey(node))
                {
                    scope = scope2;
                    break;
                }
                scope2.NeedsClosure = true;
                if (scope2.IsMethod)
                {
                    storage = VariableStorageKind.Hoisted;
                }
            }
            if (scope == null)
            {
                throw Error.UndefinedVariable(node.Name, node.Type, this.CurrentLambdaName);
            }
            if (storage == VariableStorageKind.Hoisted)
            {
                if (node.IsByRef)
                {
                    throw Error.CannotCloseOverByRef(node.Name, this.CurrentLambdaName);
                }
                scope.Definitions[node] = VariableStorageKind.Hoisted;
            }
        }

        protected internal override Expression VisitBlock(BlockExpression node)
        {
            CompilerScope scope;
            if (node.Variables.Count == 0)
            {
                base.Visit(node.Expressions);
                return node;
            }
            this._tree.Scopes[node] = scope = new CompilerScope(node, false);
            this._scopes.Push(scope);
            base.Visit(this.MergeScopes(node));
            this._scopes.Pop();
            return node;
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            CompilerScope scope;
            if (node.Variable == null)
            {
                this.Visit(node.Body);
                return node;
            }
            this._tree.Scopes[node] = scope = new CompilerScope(node, false);
            this._scopes.Push(scope);
            this.Visit(node.Body);
            this._scopes.Pop();
            return node;
        }

        protected internal override Expression VisitConstant(ConstantExpression node)
        {
            if (!this._inQuote)
            {
                if (ILGen.CanEmitConstant(node.Value, node.Type))
                {
                    return node;
                }
                this._constants.Peek().AddReference(node.Value, node.Type);
            }
            return node;
        }

        protected internal override Expression VisitInvocation(InvocationExpression node)
        {
            LambdaExpression lambdaOperand = node.LambdaOperand;
            if (lambdaOperand != null)
            {
                CompilerScope scope;
                this._tree.Scopes[lambdaOperand] = scope = new CompilerScope(lambdaOperand, false);
                this._scopes.Push(scope);
                base.Visit(this.MergeScopes(lambdaOperand));
                this._scopes.Pop();
                base.Visit(node.Arguments);
                return node;
            }
            return base.VisitInvocation(node);
        }

        protected internal override Expression VisitLambda<T>(Expression<T> node)
        {
            CompilerScope scope;
            BoundConstants constants;
            this._tree.Scopes[node] = scope = new CompilerScope(node, true);
            this._scopes.Push(scope);
            this._tree.Constants[node] = constants = new BoundConstants();
            this._constants.Push(constants);
            base.Visit(this.MergeScopes(node));
            this._constants.Pop();
            this._scopes.Pop();
            return node;
        }

        protected internal override Expression VisitParameter(ParameterExpression node)
        {
            this.Reference(node, VariableStorageKind.Local);
            CompilerScope scope = null;
            foreach (CompilerScope scope2 in this._scopes)
            {
                if (scope2.IsMethod || scope2.Definitions.ContainsKey(node))
                {
                    scope = scope2;
                    break;
                }
            }
            if (scope.ReferenceCount == null)
            {
                scope.ReferenceCount = new Dictionary<ParameterExpression, int>();
            }
            Helpers.IncrementCount<ParameterExpression>(node, scope.ReferenceCount);
            return node;
        }

        protected internal override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            foreach (ParameterExpression expression in node.Variables)
            {
                this.Reference(expression, VariableStorageKind.Hoisted);
            }
            return node;
        }

        protected internal override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Quote)
            {
                bool flag = this._inQuote;
                this._inQuote = true;
                this.Visit(node.Operand);
                this._inQuote = flag;
                return node;
            }
            this.Visit(node.Operand);
            return node;
        }

        private string CurrentLambdaName
        {
            get
            {
                foreach (CompilerScope scope in this._scopes)
                {
                    LambdaExpression node = scope.Node as LambdaExpression;
                    if (node != null)
                    {
                        return node.Name;
                    }
                }
                throw ContractUtils.Unreachable;
            }
        }
    }
}

