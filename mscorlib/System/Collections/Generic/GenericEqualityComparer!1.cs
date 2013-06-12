namespace System.Collections.Generic
{
    using System;
    using System.Runtime;

    [Serializable]
    internal class GenericEqualityComparer<T> : EqualityComparer<T> where T: IEquatable<T>
    {
        public override bool Equals(object obj)
        {
            GenericEqualityComparer<T> comparer = obj as GenericEqualityComparer<T>;
            return (comparer != null);
        }

        public override bool Equals(T x, T y)
        {
            if (x != null)
            {
                return ((y != null) && x.Equals(y));
            }
            if (y != null)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetType().Name.GetHashCode();
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override int GetHashCode(T obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return obj.GetHashCode();
        }

        internal override int IndexOf(T[] array, T value, int startIndex, int count)
        {
            int num = startIndex + count;
            if (value == null)
            {
                for (int i = startIndex; i < num; i++)
                {
                    if (array[i] == null)
                    {
                        return i;
                    }
                }
            }
            else
            {
                for (int j = startIndex; j < num; j++)
                {
                    if ((array[j] != null) && array[j].Equals(value))
                    {
                        return j;
                    }
                }
            }
            return -1;
        }

        internal override int LastIndexOf(T[] array, T value, int startIndex, int count)
        {
            int num = (startIndex - count) + 1;
            if (value == null)
            {
                for (int i = startIndex; i >= num; i--)
                {
                    if (array[i] == null)
                    {
                        return i;
                    }
                }
            }
            else
            {
                for (int j = startIndex; j >= num; j--)
                {
                    if ((array[j] != null) && array[j].Equals(value))
                    {
                        return j;
                    }
                }
            }
            return -1;
        }
    }
}

