namespace System.Linq.Expressions
{
    using System;
    using System.Reflection;

    internal class FieldExpression : MemberExpression
    {
        private readonly FieldInfo _field;

        public FieldExpression(Expression expression, FieldInfo member) : base(expression)
        {
            this._field = member;
        }

        internal override MemberInfo GetMember()
        {
            return this._field;
        }

        public sealed override System.Type Type
        {
            get
            {
                return this._field.FieldType;
            }
        }
    }
}

