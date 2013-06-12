namespace System.Dynamic.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq.Expressions;

    internal static class ContractUtils
    {
        internal static void Requires(bool precondition)
        {
            if (!precondition)
            {
                throw new ArgumentException(Strings.MethodPreconditionViolated);
            }
        }

        internal static void Requires(bool precondition, string paramName)
        {
            if (!precondition)
            {
                throw new ArgumentException(Strings.InvalidArgumentValue, paramName);
            }
        }

        internal static void RequiresArrayRange<T>(IList<T> array, int offset, int count, string offsetName, string countName)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(countName);
            }
            if ((offset < 0) || ((array.Count - offset) < count))
            {
                throw new ArgumentOutOfRangeException(offsetName);
            }
        }

        internal static void RequiresNotEmpty<T>(ICollection<T> collection, string paramName)
        {
            RequiresNotNull(collection, paramName);
            if (collection.Count == 0)
            {
                throw new ArgumentException(Strings.NonEmptyCollectionRequired, paramName);
            }
        }

        internal static void RequiresNotNull(object value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        internal static void RequiresNotNullItems<T>(IList<T> array, string arrayName)
        {
            RequiresNotNull(array, arrayName);
            for (int i = 0; i < array.Count; i++)
            {
                if (array[i] == null)
                {
                    throw new ArgumentNullException(string.Format(CultureInfo.CurrentCulture, "{0}[{1}]", new object[] { arrayName, i }));
                }
            }
        }

        internal static Exception Unreachable
        {
            get
            {
                return new InvalidOperationException("Code supposed to be unreachable");
            }
        }
    }
}

