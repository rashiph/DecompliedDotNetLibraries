namespace System.Linq.Expressions
{
    using System;
    using System.Reflection;

    internal class PropertyExpression : MemberExpression
    {
        private readonly PropertyInfo _property;

        public PropertyExpression(Expression expression, PropertyInfo member) : base(expression)
        {
            this._property = member;
        }

        internal override MemberInfo GetMember()
        {
            return this._property;
        }

        public sealed override System.Type Type
        {
            get
            {
                return this._property.PropertyType;
            }
        }
    }
}

