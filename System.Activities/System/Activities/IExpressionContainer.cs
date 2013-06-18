namespace System.Activities
{
    using System.Linq.Expressions;

    internal interface IExpressionContainer
    {
        System.Linq.Expressions.Expression Expression { get; }
    }
}

