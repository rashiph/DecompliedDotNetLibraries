namespace System.Linq.Expressions
{
    using System;

    internal sealed class PrimitiveParameterExpression<T> : ParameterExpression
    {
        internal PrimitiveParameterExpression(string name) : base(name)
        {
        }

        public sealed override System.Type Type
        {
            get
            {
                return typeof(T);
            }
        }
    }
}

