namespace System.Linq.Expressions
{
    using System;
    using System.Reflection;

    public sealed class MemberAssignment : MemberBinding
    {
        private System.Linq.Expressions.Expression _expression;

        internal MemberAssignment(MemberInfo member, System.Linq.Expressions.Expression expression) : base(MemberBindingType.Assignment, member)
        {
            this._expression = expression;
        }

        public MemberAssignment Update(System.Linq.Expressions.Expression expression)
        {
            if (expression == this.Expression)
            {
                return this;
            }
            return System.Linq.Expressions.Expression.Bind(base.Member, expression);
        }

        public System.Linq.Expressions.Expression Expression
        {
            get
            {
                return this._expression;
            }
        }
    }
}

