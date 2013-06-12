namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Dynamic.Utils;
    using System.Threading;

    [DebuggerTypeProxy(typeof(Expression.BlockExpressionProxy))]
    public class BlockExpression : Expression
    {
        internal BlockExpression()
        {
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitBlock(this);
        }

        internal virtual Expression GetExpression(int index)
        {
            throw ContractUtils.Unreachable;
        }

        internal virtual ReadOnlyCollection<Expression> GetOrMakeExpressions()
        {
            throw ContractUtils.Unreachable;
        }

        internal virtual ReadOnlyCollection<ParameterExpression> GetOrMakeVariables()
        {
            return EmptyReadOnlyCollection<ParameterExpression>.Instance;
        }

        internal virtual ParameterExpression GetVariable(int index)
        {
            throw ContractUtils.Unreachable;
        }

        internal static ReadOnlyCollection<Expression> ReturnReadOnlyExpressions(BlockExpression provider, ref object collection)
        {
            Expression comparand = collection as Expression;
            if (comparand != null)
            {
                Interlocked.CompareExchange(ref collection, new ReadOnlyCollection<Expression>(new BlockExpressionList(provider, comparand)), comparand);
            }
            return (ReadOnlyCollection<Expression>) collection;
        }

        internal virtual BlockExpression Rewrite(ReadOnlyCollection<ParameterExpression> variables, Expression[] args)
        {
            throw ContractUtils.Unreachable;
        }

        public BlockExpression Update(IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions)
        {
            if ((variables == this.Variables) && (expressions == this.Expressions))
            {
                return this;
            }
            return Expression.Block(this.Type, variables, expressions);
        }

        internal virtual int ExpressionCount
        {
            get
            {
                throw ContractUtils.Unreachable;
            }
        }

        public ReadOnlyCollection<Expression> Expressions
        {
            get
            {
                return this.GetOrMakeExpressions();
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Block;
            }
        }

        public Expression Result
        {
            get
            {
                return this.GetExpression(this.ExpressionCount - 1);
            }
        }

        public override System.Type Type
        {
            get
            {
                return this.GetExpression(this.ExpressionCount - 1).Type;
            }
        }

        internal virtual int VariableCount
        {
            get
            {
                return 0;
            }
        }

        public ReadOnlyCollection<ParameterExpression> Variables
        {
            get
            {
                return this.GetOrMakeVariables();
            }
        }
    }
}

