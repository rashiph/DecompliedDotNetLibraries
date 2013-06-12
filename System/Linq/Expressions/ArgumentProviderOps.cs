namespace System.Linq.Expressions
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class ArgumentProviderOps
    {
        internal static T[] Map<T>(this IArgumentProvider collection, Func<Expression, T> select)
        {
            T[] localArray = new T[collection.ArgumentCount];
            int num = 0;
            for (int i = 0; i < num; i++)
            {
                localArray[i] = select(collection.GetArgument(i));
            }
            return localArray;
        }
    }
}

