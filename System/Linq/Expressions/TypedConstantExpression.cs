namespace System.Linq.Expressions
{
    using System;

    internal class TypedConstantExpression : ConstantExpression
    {
        private readonly System.Type _type;

        internal TypedConstantExpression(object value, System.Type type) : base(value)
        {
            this._type = type;
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

