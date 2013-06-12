namespace System.Linq.Expressions
{
    using System;
    using System.Diagnostics;
    using System.Dynamic.Utils;
    using System.Reflection;

    [DebuggerTypeProxy(typeof(System.Linq.Expressions.Expression.MemberExpressionProxy))]
    public class MemberExpression : System.Linq.Expressions.Expression
    {
        private readonly System.Linq.Expressions.Expression _expression;

        internal MemberExpression(System.Linq.Expressions.Expression expression)
        {
            this._expression = expression;
        }

        protected internal override System.Linq.Expressions.Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.VisitMember(this);
        }

        internal virtual MemberInfo GetMember()
        {
            throw ContractUtils.Unreachable;
        }

        internal static MemberExpression Make(System.Linq.Expressions.Expression expression, MemberInfo member)
        {
            if (member.MemberType == MemberTypes.Field)
            {
                return new FieldExpression(expression, (FieldInfo) member);
            }
            return new PropertyExpression(expression, (PropertyInfo) member);
        }

        public MemberExpression Update(System.Linq.Expressions.Expression expression)
        {
            if (expression == this.Expression)
            {
                return this;
            }
            return System.Linq.Expressions.Expression.MakeMemberAccess(expression, this.Member);
        }

        public System.Linq.Expressions.Expression Expression
        {
            get
            {
                return this._expression;
            }
        }

        public MemberInfo Member
        {
            get
            {
                return this.GetMember();
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.MemberAccess;
            }
        }
    }
}

