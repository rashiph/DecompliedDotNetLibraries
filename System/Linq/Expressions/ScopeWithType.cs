namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal class ScopeWithType : ScopeN
    {
        private readonly System.Type _type;

        internal ScopeWithType(IList<ParameterExpression> variables, IList<Expression> expressions, System.Type type) : base(variables, expressions)
        {
            this._type = type;
        }

        internal override BlockExpression Rewrite(ReadOnlyCollection<ParameterExpression> variables, Expression[] args)
        {
            return new ScopeWithType(base.ReuseOrValidateVariables(variables), args, this._type);
        }

        public sealed override System.Type Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

