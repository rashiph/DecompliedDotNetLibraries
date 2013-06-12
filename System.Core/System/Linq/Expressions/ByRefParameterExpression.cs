namespace System.Linq.Expressions
{
    using System;

    internal sealed class ByRefParameterExpression : TypedParameterExpression
    {
        internal ByRefParameterExpression(Type type, string name) : base(type, name)
        {
        }

        internal override bool GetIsByRef()
        {
            return true;
        }
    }
}

