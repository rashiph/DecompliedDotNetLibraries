namespace System.Dynamic.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal static class CollectionExtensions
    {
        internal static T[] AddFirst<T>(this IList<T> list, T item)
        {
            T[] array = new T[list.Count + 1];
            array[0] = item;
            list.CopyTo(array, 1);
            return array;
        }

        internal static T[] AddLast<T>(this IList<T> list, T item)
        {
            T[] array = new T[list.Count + 1];
            list.CopyTo(array, 0);
            array[list.Count] = item;
            return array;
        }

        internal static bool All<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (T local in source)
            {
                if (!predicate(local))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool Any<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (T local in source)
            {
                if (predicate(local))
                {
                    return true;
                }
            }
            return false;
        }

        internal static T[] Copy<T>(this T[] array)
        {
            T[] destinationArray = new T[array.Length];
            Array.Copy(array, destinationArray, array.Length);
            return destinationArray;
        }

        internal static T First<T>(this IEnumerable<T> source)
        {
            IList<T> list = source as IList<T>;
            if (list != null)
            {
                return list[0];
            }
            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
            }
            throw new InvalidOperationException();
        }

        internal static T Last<T>(this IList<T> list)
        {
            return list[list.Count - 1];
        }

        internal static bool ListEquals<T>(this ICollection<T> first, ICollection<T> second)
        {
            if (first.Count != second.Count)
            {
                return false;
            }
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            IEnumerator<T> enumerator = first.GetEnumerator();
            IEnumerator<T> enumerator2 = second.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator2.MoveNext();
                if (!comparer.Equals(enumerator.Current, enumerator2.Current))
                {
                    return false;
                }
            }
            return true;
        }

        internal static int ListHashCode<T>(this IEnumerable<T> list)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            int num = 0x1997;
            foreach (T local in list)
            {
                num ^= (num << 5) ^ comparer.GetHashCode(local);
            }
            return num;
        }

        internal static U[] Map<T, U>(this ICollection<T> collection, Func<T, U> select)
        {
            U[] localArray = new U[collection.Count];
            int num = 0;
            foreach (T local in collection)
            {
                localArray[num++] = select(local);
            }
            return localArray;
        }

        internal static T[] RemoveFirst<T>(this T[] array)
        {
            T[] destinationArray = new T[array.Length - 1];
            Array.Copy(array, 1, destinationArray, 0, destinationArray.Length);
            return destinationArray;
        }

        internal static T[] RemoveLast<T>(this T[] array)
        {
            T[] destinationArray = new T[array.Length - 1];
            Array.Copy(array, 0, destinationArray, 0, destinationArray.Length);
            return destinationArray;
        }

        internal static IEnumerable<U> Select<T, U>(this IEnumerable<T> enumerable, Func<T, U> select)
        {
            foreach (T iteratorVariable0 in enumerable)
            {
                yield return select(iteratorVariable0);
            }
        }

        internal static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return EmptyReadOnlyCollection<T>.Instance;
            }
            TrueReadOnlyCollection<T> onlys = enumerable as TrueReadOnlyCollection<T>;
            if (onlys != null)
            {
                return onlys;
            }
            ReadOnlyCollectionBuilder<T> builder = enumerable as ReadOnlyCollectionBuilder<T>;
            if (builder != null)
            {
                return builder.ToReadOnlyCollection();
            }
            ICollection<T> is2 = enumerable as ICollection<T>;
            if (is2 == null)
            {
                return new TrueReadOnlyCollection<T>(new List<T>(enumerable).ToArray());
            }
            int count = is2.Count;
            if (count == 0)
            {
                return EmptyReadOnlyCollection<T>.Instance;
            }
            T[] array = new T[count];
            is2.CopyTo(array, 0);
            return new TrueReadOnlyCollection<T>(array);
        }

        internal static IEnumerable<T> Where<T>(this IEnumerable<T> enumerable, Func<T, bool> where)
        {
            foreach (T iteratorVariable0 in enumerable)
            {
                if (where(iteratorVariable0))
                {
                    yield return iteratorVariable0;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <Select>d__0<T, U> : IEnumerable<U>, IEnumerable, IEnumerator<U>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private U <>2__current;
            public IEnumerable<T> <>3__enumerable;
            public Func<T, U> <>3__select;
            public IEnumerator<T> <>7__wrap2;
            private int <>l__initialThreadId;
            public T <t>5__1;
            public IEnumerable<T> enumerable;
            public Func<T, U> select;

            [DebuggerHidden]
            public <Select>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally3()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap2 != null)
                {
                    this.<>7__wrap2.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<>7__wrap2 = this.enumerable.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_0076;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_0076;

                        default:
                            goto Label_0089;
                    }
                Label_003C:
                    this.<t>5__1 = this.<>7__wrap2.Current;
                    this.<>2__current = this.select(this.<t>5__1);
                    this.<>1__state = 2;
                    return true;
                Label_0076:
                    if (this.<>7__wrap2.MoveNext())
                    {
                        goto Label_003C;
                    }
                    this.<>m__Finally3();
                Label_0089:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<U> IEnumerable<U>.GetEnumerator()
            {
                CollectionExtensions.<Select>d__0<T, U> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (CollectionExtensions.<Select>d__0<T, U>) this;
                }
                else
                {
                    d__ = new CollectionExtensions.<Select>d__0<T, U>(0);
                }
                d__.enumerable = this.<>3__enumerable;
                d__.select = this.<>3__select;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<U>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case 1:
                    case 2:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally3();
                        }
                        return;
                }
            }

            U IEnumerator<U>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <Where>d__6<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private T <>2__current;
            public IEnumerable<T> <>3__enumerable;
            public Func<T, bool> <>3__where;
            public IEnumerator<T> <>7__wrap8;
            private int <>l__initialThreadId;
            public T <t>5__7;
            public IEnumerable<T> enumerable;
            public Func<T, bool> where;

            [DebuggerHidden]
            public <Where>d__6(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally9()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap8 != null)
                {
                    this.<>7__wrap8.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<>7__wrap8 = this.enumerable.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_007E;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_007E;

                        default:
                            goto Label_0091;
                    }
                Label_003C:
                    this.<t>5__7 = this.<>7__wrap8.Current;
                    if (this.where(this.<t>5__7))
                    {
                        this.<>2__current = this.<t>5__7;
                        this.<>1__state = 2;
                        return true;
                    }
                Label_007E:
                    if (this.<>7__wrap8.MoveNext())
                    {
                        goto Label_003C;
                    }
                    this.<>m__Finally9();
                Label_0091:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                CollectionExtensions.<Where>d__6<T> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (CollectionExtensions.<Where>d__6<T>) this;
                }
                else
                {
                    d__ = new CollectionExtensions.<Where>d__6<T>(0);
                }
                d__.enumerable = this.<>3__enumerable;
                d__.where = this.<>3__where;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<T>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case 1:
                    case 2:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally9();
                        }
                        return;
                }
            }

            T IEnumerator<T>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }
    }
}

