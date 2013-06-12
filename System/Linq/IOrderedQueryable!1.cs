namespace System.Linq
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IOrderedQueryable<out T> : IQueryable<T>, IEnumerable<T>, IOrderedQueryable, IQueryable, IEnumerable
    {
    }
}

