namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal class ScopeExpression : BlockExpression
    {
        private IList<ParameterExpression> _variables;

        internal ScopeExpression(IList<ParameterExpression> variables)
        {
            this._variables = variables;
        }

        internal override ReadOnlyCollection<ParameterExpression> GetOrMakeVariables()
        {
            return Expression.ReturnReadOnly<ParameterExpression>(ref this._variables);
        }

        internal override ParameterExpression GetVariable(int index)
        {
            return this._variables[index];
        }

        internal IList<ParameterExpression> ReuseOrValidateVariables(ReadOnlyCollection<ParameterExpression> variables)
        {
            if ((variables != null) && (variables != this.VariablesList))
            {
                Expression.ValidateVariables(variables, "variables");
                return variables;
            }
            return this.VariablesList;
        }

        internal override int VariableCount
        {
            get
            {
                return this._variables.Count;
            }
        }

        protected IList<ParameterExpression> VariablesList
        {
            get
            {
                return this._variables;
            }
        }
    }
}

