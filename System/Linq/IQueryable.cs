namespace System.Linq
{
    using System;
    using System.Collections;
    using System.Linq.Expressions;

    public interface IQueryable : IEnumerable
    {
        Type ElementType { get; }

        System.Linq.Expressions.Expression Expression { get; }

        IQueryProvider Provider { get; }
    }
}

