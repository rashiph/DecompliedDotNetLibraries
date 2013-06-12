namespace System.Linq.Expressions
{
    using System;

    internal class TypedParameterExpression : ParameterExpression
    {
        private readonly System.Type _paramType;

        internal TypedParameterExpression(System.Type type, string name) : base(name)
        {
            this._paramType = type;
        }

        public sealed override System.Type Type
        {
            get
            {
                return this._paramType;
            }
        }
    }
}

