namespace System.Linq.Expressions
{
    using System;
    using System.Diagnostics;

    [DebuggerTypeProxy(typeof(Expression.CatchBlockProxy))]
    public sealed class CatchBlock
    {
        private readonly Expression _body;
        private readonly Expression _filter;
        private readonly Type _test;
        private readonly ParameterExpression _var;

        internal CatchBlock(Type test, ParameterExpression variable, Expression body, Expression filter)
        {
            this._test = test;
            this._var = variable;
            this._body = body;
            this._filter = filter;
        }

        public override string ToString()
        {
            return ExpressionStringBuilder.CatchBlockToString(this);
        }

        public CatchBlock Update(ParameterExpression variable, Expression filter, Expression body)
        {
            if (((variable == this.Variable) && (filter == this.Filter)) && (body == this.Body))
            {
                return this;
            }
            return Expression.MakeCatchBlock(this.Test, variable, body, filter);
        }

        public Expression Body
        {
            get
            {
                return this._body;
            }
        }

        public Expression Filter
        {
            get
            {
                return this._filter;
            }
        }

        public Type Test
        {
            get
            {
                return this._test;
            }
        }

        public ParameterExpression Variable
        {
            get
            {
                return this._var;
            }
        }
    }
}

