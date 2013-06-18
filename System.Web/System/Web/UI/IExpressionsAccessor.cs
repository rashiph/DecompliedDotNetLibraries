namespace System.Web.UI
{
    using System;

    public interface IExpressionsAccessor
    {
        ExpressionBindingCollection Expressions { get; }

        bool HasExpressions { get; }
    }
}

