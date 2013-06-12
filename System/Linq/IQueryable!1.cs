namespace System.Linq
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IQueryable<out T> : IEnumerable<T>, IQueryable, IEnumerable
    {
    }
}

