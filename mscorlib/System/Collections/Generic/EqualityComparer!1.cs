namespace System.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Security;

    [Serializable, TypeDependency("System.Collections.Generic.GenericEqualityComparer`1"), TypeDependency("System.Collections.Generic.EnumEqualityComparer`1")]
    public abstract class EqualityComparer<T> : IEqualityComparer, IEqualityComparer<T>
    {
        private static EqualityComparer<T> defaultComparer;

        protected EqualityComparer()
        {
        }

        [SecuritySafeCritical]
        private static EqualityComparer<T> CreateComparer()
        {
            RuntimeType c = (RuntimeType) typeof(T);
            if (c == typeof(byte))
            {
                return (EqualityComparer<T>) new ByteEqualityComparer();
            }
            if (typeof(IEquatable<T>).IsAssignableFrom(c))
            {
                return (EqualityComparer<T>) RuntimeTypeHandle.CreateInstanceForAnotherGenericParameter((RuntimeType) typeof(GenericEqualityComparer<int>), c);
            }
            if (c.IsGenericType && (c.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                RuntimeType type2 = (RuntimeType) c.GetGenericArguments()[0];
                if (typeof(IEquatable<>).MakeGenericType(new Type[] { type2 }).IsAssignableFrom(type2))
                {
                    return (EqualityComparer<T>) RuntimeTypeHandle.CreateInstanceForAnotherGenericParameter((RuntimeType) typeof(NullableEqualityComparer<int>), type2);
                }
            }
            if (c.IsEnum && (Enum.GetUnderlyingType(c) == typeof(int)))
            {
                return (EqualityComparer<T>) RuntimeTypeHandle.CreateInstanceForAnotherGenericParameter((RuntimeType) typeof(EnumEqualityComparer<int>), c);
            }
            return new ObjectEqualityComparer<T>();
        }

        public abstract bool Equals(T x, T y);
        public abstract int GetHashCode(T obj);
        internal virtual int IndexOf(T[] array, T value, int startIndex, int count)
        {
            int num = startIndex + count;
            for (int i = startIndex; i < num; i++)
            {
                if (this.Equals(array[i], value))
                {
                    return i;
                }
            }
            return -1;
        }

        internal virtual int LastIndexOf(T[] array, T value, int startIndex, int count)
        {
            int num = (startIndex - count) + 1;
            for (int i = startIndex; i >= num; i--)
            {
                if (this.Equals(array[i], value))
                {
                    return i;
                }
            }
            return -1;
        }

        bool IEqualityComparer.Equals(object x, object y)
        {
            if (x == y)
            {
                return true;
            }
            if ((x != null) && (y != null))
            {
                if ((x is T) && (y is T))
                {
                    return this.Equals((T) x, (T) y);
                }
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArgumentForComparison);
            }
            return false;
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            if (obj != null)
            {
                if (obj is T)
                {
                    return this.GetHashCode((T) obj);
                }
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArgumentForComparison);
            }
            return 0;
        }

        public static EqualityComparer<T> Default
        {
            [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                EqualityComparer<T> defaultComparer = EqualityComparer<T>.defaultComparer;
                if (defaultComparer == null)
                {
                    defaultComparer = EqualityComparer<T>.CreateComparer();
                    EqualityComparer<T>.defaultComparer = defaultComparer;
                }
                return defaultComparer;
            }
        }
    }
}

