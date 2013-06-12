namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [DebuggerTypeProxy(typeof(Expression.RuntimeVariablesExpressionProxy))]
    public sealed class RuntimeVariablesExpression : Expression
    {
        private readonly ReadOnlyCollection<ParameterExpression> _variables;

        internal RuntimeVariablesExpression(ReadOnlyCollection<ParameterExpression> variables)
        {
            this._variables = variables;
        }

        protected internal override Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitRuntimeVariables(this);
        }

        public RuntimeVariablesExpression Update(IEnumerable<ParameterExpression> variables)
        {
            if (variables == this.Variables)
            {
                return this;
            }
            return Expression.RuntimeVariables(variables);
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.RuntimeVariables;
            }
        }

        public sealed override System.Type Type
        {
            get
            {
                return typeof(IRuntimeVariables);
            }
        }

        public ReadOnlyCollection<ParameterExpression> Variables
        {
            get
            {
                return this._variables;
            }
        }
    }
}

