namespace System.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public static class Enumerable
    {
        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (func == null)
            {
                throw Error.ArgumentNull("func");
            }
            using (IEnumerator<TSource> enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    throw Error.NoElements();
                }
                TSource current = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    current = func(current, enumerator.Current);
                }
                return current;
            }
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (func == null)
            {
                throw Error.ArgumentNull("func");
            }
            TAccumulate local = seed;
            foreach (TSource local2 in source)
            {
                local = func(local, local2);
            }
            return local;
        }

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (func == null)
            {
                throw Error.ArgumentNull("func");
            }
            if (resultSelector == null)
            {
                throw Error.ArgumentNull("resultSelector");
            }
            TAccumulate local = seed;
            foreach (TSource local2 in source)
            {
                local = func(local, local2);
            }
            return resultSelector(local);
        }

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            foreach (TSource local in source)
            {
                if (!predicate(local))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            using (IEnumerator<TSource> enumerator = source.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            foreach (TSource local in source)
            {
                if (predicate(local))
                {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source)
        {
            return source;
        }

        public static decimal Average(this IEnumerable<decimal> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            decimal num = 0M;
            long num2 = 0L;
            foreach (decimal num3 in source)
            {
                num += num3;
                num2 += 1L;
            }
            if (num2 <= 0L)
            {
                throw Error.NoElements();
            }
            return (num / num2);
        }

        public static double Average(this IEnumerable<double> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            double num = 0.0;
            long num2 = 0L;
            foreach (double num3 in source)
            {
                num += num3;
                num2 += 1L;
            }
            if (num2 <= 0L)
            {
                throw Error.NoElements();
            }
            return (num / ((double) num2));
        }

        public static double Average(this IEnumerable<int> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            long num = 0L;
            long num2 = 0L;
            foreach (int num3 in source)
            {
                num += num3;
                num2 += 1L;
            }
            if (num2 <= 0L)
            {
                throw Error.NoElements();
            }
            return (((double) num) / ((double) num2));
        }

        public static double Average(this IEnumerable<long> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            long num = 0L;
            long num2 = 0L;
            foreach (long num3 in source)
            {
                num += num3;
                num2 += 1L;
            }
            if (num2 <= 0L)
            {
                throw Error.NoElements();
            }
            return (((double) num) / ((double) num2));
        }

        public static float Average(this IEnumerable<float> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            double num = 0.0;
            long num2 = 0L;
            foreach (float num3 in source)
            {
                num += num3;
                num2 += 1L;
            }
            if (num2 <= 0L)
            {
                throw Error.NoElements();
            }
            return (float) (num / ((double) num2));
        }

        public static decimal? Average(this IEnumerable<decimal?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            decimal num = 0M;
            long num2 = 0L;
            foreach (decimal? nullable in source)
            {
                if (nullable.HasValue)
                {
                    num += nullable.GetValueOrDefault();
                    num2 += 1L;
                }
            }
            if (num2 > 0L)
            {
                return new decimal?(num / num2);
            }
            return null;
        }

        public static double? Average(this IEnumerable<double?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            double num = 0.0;
            long num2 = 0L;
            foreach (double? nullable in source)
            {
                if (nullable.HasValue)
                {
                    num += nullable.GetValueOrDefault();
                    num2 += 1L;
                }
            }
            if (num2 > 0L)
            {
                return new double?(num / ((double) num2));
            }
            return null;
        }

        public static double? Average(this IEnumerable<int?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            long num = 0L;
            long num2 = 0L;
            foreach (int? nullable in source)
            {
                if (nullable.HasValue)
                {
                    num += (long) nullable.GetValueOrDefault();
                    num2 += 1L;
                }
            }
            if (num2 > 0L)
            {
                return new double?(((double) num) / ((double) num2));
            }
            return null;
        }

        public static double? Average(this IEnumerable<long?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            long num = 0L;
            long num2 = 0L;
            foreach (long? nullable in source)
            {
                if (nullable.HasValue)
                {
                    num += nullable.GetValueOrDefault();
                    num2 += 1L;
                }
            }
            if (num2 > 0L)
            {
                return new double?(((double) num) / ((double) num2));
            }
            return null;
        }

        public static float? Average(this IEnumerable<float?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            double num = 0.0;
            long num2 = 0L;
            foreach (float? nullable in source)
            {
                if (nullable.HasValue)
                {
                    num += (double) nullable.GetValueOrDefault();
                    num2 += 1L;
                }
            }
            if (num2 > 0L)
            {
                return new float?((float) (num / ((double) num2)));
            }
            return null;
        }

        public static decimal Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            return source.Select<TSource, decimal>(selector).Average();
        }

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            return source.Select<TSource, double>(selector).Average();
        }

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            return source.Select<TSource, int>(selector).Average();
        }

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            return source.Select<TSource, long>(selector).Average();
        }

        public static decimal? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            return source.Select<TSource, decimal?>(selector).Average();
        }

        public static float Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            return source.Select<TSource, float>(selector).Average();
        }

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            return source.Select<TSource, double?>(selector).Average();
        }

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            return source.Select<TSource, int?>(selector).Average();
        }

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            return source.Select<TSource, long?>(selector).Average();
        }

        public static float? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
        {
            return source.Select<TSource, float?>(selector).Average();
        }

        public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source)
        {
            IEnumerable<TResult> enumerable = source as IEnumerable<TResult>;
            if (enumerable != null)
            {
                return enumerable;
            }
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            return CastIterator<TResult>(source);
        }

        private static IEnumerable<TResult> CastIterator<TResult>(IEnumerable source)
        {
            IEnumerator enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                object current = enumerator.Current;
                yield return (TResult) current;
            }
        }

        private static Func<TSource, bool> CombinePredicates<TSource>(Func<TSource, bool> predicate1, Func<TSource, bool> predicate2)
        {
            return delegate (TSource x) {
                if (predicate1(x))
                {
                    return predicate2(x);
                }
                return false;
            };
        }

        private static Func<TSource, TResult> CombineSelectors<TSource, TMiddle, TResult>(Func<TSource, TMiddle> selector1, Func<TMiddle, TResult> selector2)
        {
            return x => selector2(selector1(x));
        }

        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            if (first == null)
            {
                throw Error.ArgumentNull("first");
            }
            if (second == null)
            {
                throw Error.ArgumentNull("second");
            }
            return ConcatIterator<TSource>(first, second);
        }

        private static IEnumerable<TSource> ConcatIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            foreach (TSource iteratorVariable0 in first)
            {
                yield return iteratorVariable0;
            }
            foreach (TSource iteratorVariable1 in second)
            {
                yield return iteratorVariable1;
            }
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            ICollection<TSource> is2 = source as ICollection<TSource>;
            if (is2 != null)
            {
                return is2.Contains(value);
            }
            return source.Contains<TSource>(value, null);
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<TSource>.Default;
            }
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            foreach (TSource local in source)
            {
                if (comparer.Equals(local, value))
                {
                    return true;
                }
            }
            return false;
        }

        public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            ICollection<TSource> is2 = source as ICollection<TSource>;
            if (is2 != null)
            {
                return is2.Count;
            }
            ICollection is3 = source as ICollection;
            if (is3 != null)
            {
                return is3.Count;
            }
            int num = 0;
            using (IEnumerator<TSource> enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    num++;
                }
            }
            return num;
        }

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            int num = 0;
            foreach (TSource local in source)
            {
                if (predicate(local))
                {
                    num++;
                }
            }
            return num;
        }

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source)
        {
            return source.DefaultIfEmpty<TSource>(default(TSource));
        }

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            return DefaultIfEmptyIterator<TSource>(source, defaultValue);
        }

        private static IEnumerable<TSource> DefaultIfEmptyIterator<TSource>(IEnumerable<TSource> source, TSource defaultValue)
        {
            using (IEnumerator<TSource> iteratorVariable0 = source.GetEnumerator())
            {
                if (iteratorVariable0.MoveNext())
                {
                    do
                    {
                        yield return iteratorVariable0.Current;
                    }
                    while (iteratorVariable0.MoveNext());
                }
                else
                {
                    yield return defaultValue;
                }
            }
        }

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            return DistinctIterator<TSource>(source, null);
        }

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            return DistinctIterator<TSource>(source, comparer);
        }

        private static IEnumerable<TSource> DistinctIterator<TSource>(IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
        {
            Set<TSource> iteratorVariable0 = new Set<TSource>(comparer);
            foreach (TSource iteratorVariable1 in source)
            {
                if (iteratorVariable0.Add(iteratorVariable1))
                {
                    yield return iteratorVariable1;
                }
            }
        }

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
        {
            TSource current;
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                return list[index];
            }
            if (index < 0)
            {
                throw Error.ArgumentOutOfRange("index");
            }
            using (IEnumerator<TSource> enumerator = source.GetEnumerator())
            {
            Label_0036:
                if (!enumerator.MoveNext())
                {
                    throw Error.ArgumentOutOfRange("index");
                }
                if (index == 0)
                {
                    current = enumerator.Current;
                }
                else
                {
                    index--;
                    goto Label_0036;
                }
            }
            return current;
        }

        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (index >= 0)
            {
                IList<TSource> list = source as IList<TSource>;
                if (list != null)
                {
                    if (index < list.Count)
                    {
                        return list[index];
                    }
                }
                else
                {
                    using (IEnumerator<TSource> enumerator = source.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            if (index == 0)
                            {
                                return enumerator.Current;
                            }
                            index--;
                        }
                    }
                }
            }
            return default(TSource);
        }

        public static IEnumerable<TResult> Empty<TResult>()
        {
            return EmptyEnumerable<TResult>.Instance;
        }

        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            if (first == null)
            {
                throw Error.ArgumentNull("first");
            }
            if (second == null)
            {
                throw Error.ArgumentNull("second");
            }
            return ExceptIterator<TSource>(first, second, null);
        }

        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null)
            {
                throw Error.ArgumentNull("first");
            }
            if (second == null)
            {
                throw Error.ArgumentNull("second");
            }
            return ExceptIterator<TSource>(first, second, comparer);
        }

        private static IEnumerable<TSource> ExceptIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            Set<TSource> iteratorVariable0 = new Set<TSource>(comparer);
            foreach (TSource local in second)
            {
                iteratorVariable0.Add(local);
            }
            foreach (TSource iteratorVariable1 in first)
            {
                if (!iteratorVariable0.Add(iteratorVariable1))
                {
                    continue;
                }
                yield return iteratorVariable1;
            }
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                if (list.Count > 0)
                {
                    return list[0];
                }
            }
            else
            {
                using (IEnumerator<TSource> enumerator = source.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        return enumerator.Current;
                    }
                }
            }
            throw Error.NoElements();
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            foreach (TSource local in source)
            {
                if (predicate(local))
                {
                    return local;
                }
            }
            throw Error.NoMatch();
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                if (list.Count > 0)
                {
                    return list[0];
                }
            }
            else
            {
                using (IEnumerator<TSource> enumerator = source.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        return enumerator.Current;
                    }
                }
            }
            return default(TSource);
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            foreach (TSource local in source)
            {
                if (predicate(local))
                {
                    return local;
                }
            }
            return default(TSource);
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return new GroupedEnumerable<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, null);
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            return new GroupedEnumerable<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, comparer);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return new GroupedEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector, null);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
        {
            return new GroupedEnumerable<TSource, TKey, TSource, TResult>(source, keySelector, IdentityFunction<TSource>.Instance, resultSelector, null);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            return new GroupedEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
        {
            return new GroupedEnumerable<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, null);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            return new GroupedEnumerable<TSource, TKey, TSource, TResult>(source, keySelector, IdentityFunction<TSource>.Instance, resultSelector, comparer);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            return new GroupedEnumerable<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, comparer);
        }

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
        {
            if (outer == null)
            {
                throw Error.ArgumentNull("outer");
            }
            if (inner == null)
            {
                throw Error.ArgumentNull("inner");
            }
            if (outerKeySelector == null)
            {
                throw Error.ArgumentNull("outerKeySelector");
            }
            if (innerKeySelector == null)
            {
                throw Error.ArgumentNull("innerKeySelector");
            }
            if (resultSelector == null)
            {
                throw Error.ArgumentNull("resultSelector");
            }
            return GroupJoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
        }

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (outer == null)
            {
                throw Error.ArgumentNull("outer");
            }
            if (inner == null)
            {
                throw Error.ArgumentNull("inner");
            }
            if (outerKeySelector == null)
            {
                throw Error.ArgumentNull("outerKeySelector");
            }
            if (innerKeySelector == null)
            {
                throw Error.ArgumentNull("innerKeySelector");
            }
            if (resultSelector == null)
            {
                throw Error.ArgumentNull("resultSelector");
            }
            return GroupJoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        }

        private static IEnumerable<TResult> GroupJoinIterator<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            Lookup<TKey, TInner> iteratorVariable0 = Lookup<TKey, TInner>.CreateForJoin(inner, innerKeySelector, comparer);
            foreach (TOuter iteratorVariable1 in outer)
            {
                yield return resultSelector(iteratorVariable1, iteratorVariable0[outerKeySelector(iteratorVariable1)]);
            }
        }

        public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            if (first == null)
            {
                throw Error.ArgumentNull("first");
            }
            if (second == null)
            {
                throw Error.ArgumentNull("second");
            }
            return IntersectIterator<TSource>(first, second, null);
        }

        public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null)
            {
                throw Error.ArgumentNull("first");
            }
            if (second == null)
            {
                throw Error.ArgumentNull("second");
            }
            return IntersectIterator<TSource>(first, second, comparer);
        }

        private static IEnumerable<TSource> IntersectIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            Set<TSource> iteratorVariable0 = new Set<TSource>(comparer);
            foreach (TSource local in second)
            {
                iteratorVariable0.Add(local);
            }
            foreach (TSource iteratorVariable1 in first)
            {
                if (!iteratorVariable0.Remove(iteratorVariable1))
                {
                    continue;
                }
                yield return iteratorVariable1;
            }
        }

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
        {
            if (outer == null)
            {
                throw Error.ArgumentNull("outer");
            }
            if (inner == null)
            {
                throw Error.ArgumentNull("inner");
            }
            if (outerKeySelector == null)
            {
                throw Error.ArgumentNull("outerKeySelector");
            }
            if (innerKeySelector == null)
            {
                throw Error.ArgumentNull("innerKeySelector");
            }
            if (resultSelector == null)
            {
                throw Error.ArgumentNull("resultSelector");
            }
            return JoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
        }

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (outer == null)
            {
                throw Error.ArgumentNull("outer");
            }
            if (inner == null)
            {
                throw Error.ArgumentNull("inner");
            }
            if (outerKeySelector == null)
            {
                throw Error.ArgumentNull("outerKeySelector");
            }
            if (innerKeySelector == null)
            {
                throw Error.ArgumentNull("innerKeySelector");
            }
            if (resultSelector == null)
            {
                throw Error.ArgumentNull("resultSelector");
            }
            return JoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        }

        private static IEnumerable<TResult> JoinIterator<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            Lookup<TKey, TInner> iteratorVariable0 = Lookup<TKey, TInner>.CreateForJoin(inner, innerKeySelector, comparer);
            foreach (TOuter iteratorVariable1 in outer)
            {
                Lookup<TKey, TInner>.Grouping grouping = iteratorVariable0.GetGrouping(outerKeySelector(iteratorVariable1), false);
                if (grouping != null)
                {
                    for (int i = 0; i < grouping.count; i++)
                    {
                        yield return resultSelector(iteratorVariable1, grouping.elements[i]);
                    }
                }
            }
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                int count = list.Count;
                if (count > 0)
                {
                    return list[count - 1];
                }
            }
            else
            {
                using (IEnumerator<TSource> enumerator = source.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        TSource current;
                        do
                        {
                            current = enumerator.Current;
                        }
                        while (enumerator.MoveNext());
                        return current;
                    }
                }
            }
            throw Error.NoElements();
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            TSource local = default(TSource);
            bool flag = false;
            foreach (TSource local2 in source)
            {
                if (predicate(local2))
                {
                    local = local2;
                    flag = true;
                }
            }
            if (!flag)
            {
                throw Error.NoMatch();
            }
            return local;
        }

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                int count = list.Count;
                if (count > 0)
                {
                    return list[count - 1];
                }
            }
            else
            {
                using (IEnumerator<TSource> enumerator = source.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        TSource current;
                        do
                        {
                            current = enumerator.Current;
                        }
                        while (enumerator.MoveNext());
                        return current;
                    }
                }
            }
            return default(TSource);
        }

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            TSource local = default(TSource);
            foreach (TSource local2 in source)
            {
                if (predicate(local2))
                {
                    local = local2;
                }
            }
            return local;
        }

        public static long LongCount<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            long num = 0L;
            using (IEnumerator<TSource> enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    num += 1L;
                }
            }
            return num;
        }

        public static long LongCount<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            long num = 0L;
            foreach (TSource local in source)
            {
                if (predicate(local))
                {
                    num += 1L;
                }
            }
            return num;
        }

        public static decimal Max(this IEnumerable<decimal> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            decimal num = 0M;
            bool flag = false;
            foreach (decimal num2 in source)
            {
                if (flag)
                {
                    if (num2 > num)
                    {
                        num = num2;
                    }
                }
                else
                {
                    num = num2;
                    flag = true;
                }
            }
            if (!flag)
            {
                throw Error.NoElements();
            }
            return num;
        }

        public static double Max(this IEnumerable<double> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            double d = 0.0;
            bool flag = false;
            foreach (double num2 in source)
            {
                if (flag)
                {
                    if ((num2 > d) || double.IsNaN(d))
                    {
                        d = num2;
                    }
                }
                else
                {
                    d = num2;
                    flag = true;
                }
            }
            if (!flag)
            {
                throw Error.NoElements();
            }
            return d;
        }

        public static int Max(this IEnumerable<int> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            int num = 0;
            bool flag = false;
            foreach (int num2 in source)
            {
                if (flag)
                {
                    if (num2 > num)
                    {
                        num = num2;
                    }
                }
                else
                {
                    num = num2;
                    flag = true;
                }
            }
            if (!flag)
            {
                throw Error.NoElements();
            }
            return num;
        }

        public static long Max(this IEnumerable<long> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            long num = 0L;
            bool flag = false;
            foreach (long num2 in source)
            {
                if (flag)
                {
                    if (num2 > num)
                    {
                        num = num2;
                    }
                }
                else
                {
                    num = num2;
                    flag = true;
                }
            }
            if (!flag)
            {
                throw Error.NoElements();
            }
            return num;
        }

        public static float Max(this IEnumerable<float> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            float num = 0f;
            bool flag = false;
            foreach (float num2 in source)
            {
                if (flag)
                {
                    if ((num2 > num) || double.IsNaN((double) num))
                    {
                        num = num2;
                    }
                }
                else
                {
                    num = num2;
                    flag = true;
                }
            }
            if (!flag)
            {
                throw Error.NoElements();
            }
            return num;
        }

        public static decimal? Max(this IEnumerable<decimal?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            decimal? nullable = null;
            foreach (decimal? nullable2 in source)
            {
                if (nullable.HasValue)
                {
                    decimal? nullable3 = nullable2;
                    decimal? nullable4 = nullable;
                    if ((nullable3.GetValueOrDefault() <= nullable4.GetValueOrDefault()) || !(nullable3.HasValue & nullable4.HasValue))
                    {
                        continue;
                    }
                }
                nullable = nullable2;
            }
            return nullable;
        }

        public static double? Max(this IEnumerable<double?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            double? nullable = null;
            foreach (double? nullable2 in source)
            {
                if (nullable2.HasValue)
                {
                    if (nullable.HasValue)
                    {
                        double? nullable3 = nullable2;
                        double? nullable4 = nullable;
                        if (((nullable3.GetValueOrDefault() <= nullable4.GetValueOrDefault()) || !(nullable3.HasValue & nullable4.HasValue)) && !double.IsNaN(nullable.Value))
                        {
                            continue;
                        }
                    }
                    nullable = nullable2;
                }
            }
            return nullable;
        }

        public static int? Max(this IEnumerable<int?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            int? nullable = null;
            foreach (int? nullable2 in source)
            {
                if (nullable.HasValue)
                {
                    int? nullable3 = nullable2;
                    int? nullable4 = nullable;
                    if ((nullable3.GetValueOrDefault() <= nullable4.GetValueOrDefault()) || !(nullable3.HasValue & nullable4.HasValue))
                    {
                        continue;
                    }
                }
                nullable = nullable2;
            }
            return nullable;
        }

        public static long? Max(this IEnumerable<long?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            long? nullable = null;
            foreach (long? nullable2 in source)
            {
                if (nullable.HasValue)
                {
                    long? nullable3 = nullable2;
                    long? nullable4 = nullable;
                    if ((nullable3.GetValueOrDefault() <= nullable4.GetValueOrDefault()) || !(nullable3.HasValue & nullable4.HasValue))
                    {
                        continue;
                    }
                }
                nullable = nullable2;
            }
            return nullable;
        }

        public static float? Max(this IEnumerable<float?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            float? nullable = null;
            foreach (float? nullable2 in source)
            {
                if (nullable2.HasValue)
                {
                    if (nullable.HasValue)
                    {
                        float? nullable3 = nullable2;
                        float? nullable4 = nullable;
                        if (((nullable3.GetValueOrDefault() <= nullable4.GetValueOrDefault()) || !(nullable3.HasValue & nullable4.HasValue)) && !float.IsNaN(nullable.Value))
                        {
                            continue;
                        }
                    }
                    nullable = nullable2;
                }
            }
            return nullable;
        }

        public static TSource Max<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            Comparer<TSource> comparer = Comparer<TSource>.Default;
            TSource y = default(TSource);
            if (y == null)
            {
                foreach (TSource local2 in source)
                {
                    if ((local2 != null) && ((y == null) || (comparer.Compare(local2, y) > 0)))
                    {
                        y = local2;
                    }
                }
                return y;
            }
            bool flag = false;
            foreach (TSource local3 in source)
            {
                if (flag)
                {
                    if (comparer.Compare(local3, y) > 0)
                    {
                        y = local3;
                    }
                }
                else
                {
                    y = local3;
                    flag = true;
                }
            }
            if (!flag)
            {
                throw Error.NoElements();
            }
            return y;
        }

        public static decimal Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            return source.Select<TSource, decimal>(selector).Max();
        }

        public static double Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            return source.Select<TSource, double>(selector).Max();
        }

        public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            return source.Select<TSource, int>(selector).Max();
        }

        public static long Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            return source.Select<TSource, long>(selector).Max();
        }

        public static float Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            return source.Select<TSource, float>(selector).Max();
        }

        public static decimal? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            return source.Select<TSource, decimal?>(selector).Max();
        }

        public static double? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            return source.Select<TSource, double?>(selector).Max();
        }

        public static int? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            return source.Select<TSource, int?>(selector).Max();
        }

        public static long? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            return source.Select<TSource, long?>(selector).Max();
        }

        public static float? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
        {
            return source.Select<TSource, float?>(selector).Max();
        }

        public static TResult Max<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Select<TSource, TResult>(selector).Max<TResult>();
        }

        public static decimal Min(this IEnumerable<decimal> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            decimal num = 0M;
            bool flag = false;
            foreach (decimal num2 in source)
            {
                if (flag)
                {
                    if (num2 < num)
                    {
                        num = num2;
                    }
                }
                else
                {
                    num = num2;
                    flag = true;
                }
            }
            if (!flag)
            {
                throw Error.NoElements();
            }
            return num;
        }

        public static double Min(this IEnumerable<double> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            double num = 0.0;
            bool flag = false;
            foreach (double num2 in source)
            {
                if (flag)
                {
                    if ((num2 < num) || double.IsNaN(num2))
                    {
                        num = num2;
                    }
                }
                else
                {
                    num = num2;
                    flag = true;
                }
            }
            if (!flag)
            {
                throw Error.NoElements();
            }
            return num;
        }

        public static int Min(this IEnumerable<int> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            int num = 0;
            bool flag = false;
            foreach (int num2 in source)
            {
                if (flag)
                {
                    if (num2 < num)
                    {
                        num = num2;
                    }
                }
                else
                {
                    num = num2;
                    flag = true;
                }
            }
            if (!flag)
            {
                throw Error.NoElements();
            }
            return num;
        }

        public static long Min(this IEnumerable<long> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            long num = 0L;
            bool flag = false;
            foreach (long num2 in source)
            {
                if (flag)
                {
                    if (num2 < num)
                    {
                        num = num2;
                    }
                }
                else
                {
                    num = num2;
                    flag = true;
                }
            }
            if (!flag)
            {
                throw Error.NoElements();
            }
            return num;
        }

        public static float Min(this IEnumerable<float> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            float num = 0f;
            bool flag = false;
            foreach (float num2 in source)
            {
                if (flag)
                {
                    if ((num2 < num) || float.IsNaN(num2))
                    {
                        num = num2;
                    }
                }
                else
                {
                    num = num2;
                    flag = true;
                }
            }
            if (!flag)
            {
                throw Error.NoElements();
            }
            return num;
        }

        public static decimal? Min(this IEnumerable<decimal?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            decimal? nullable = null;
            foreach (decimal? nullable2 in source)
            {
                if (nullable.HasValue)
                {
                    decimal? nullable3 = nullable2;
                    decimal? nullable4 = nullable;
                    if ((nullable3.GetValueOrDefault() >= nullable4.GetValueOrDefault()) || !(nullable3.HasValue & nullable4.HasValue))
                    {
                        continue;
                    }
                }
                nullable = nullable2;
            }
            return nullable;
        }

        public static double? Min(this IEnumerable<double?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            double? nullable = null;
            foreach (double? nullable2 in source)
            {
                if (nullable2.HasValue)
                {
                    if (nullable.HasValue)
                    {
                        double? nullable3 = nullable2;
                        double? nullable4 = nullable;
                        if (((nullable3.GetValueOrDefault() >= nullable4.GetValueOrDefault()) || !(nullable3.HasValue & nullable4.HasValue)) && !double.IsNaN(nullable2.Value))
                        {
                            continue;
                        }
                    }
                    nullable = nullable2;
                }
            }
            return nullable;
        }

        public static int? Min(this IEnumerable<int?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            int? nullable = null;
            foreach (int? nullable2 in source)
            {
                if (nullable.HasValue)
                {
                    int? nullable3 = nullable2;
                    int? nullable4 = nullable;
                    if ((nullable3.GetValueOrDefault() >= nullable4.GetValueOrDefault()) || !(nullable3.HasValue & nullable4.HasValue))
                    {
                        continue;
                    }
                }
                nullable = nullable2;
            }
            return nullable;
        }

        public static long? Min(this IEnumerable<long?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            long? nullable = null;
            foreach (long? nullable2 in source)
            {
                if (nullable.HasValue)
                {
                    long? nullable3 = nullable2;
                    long? nullable4 = nullable;
                    if ((nullable3.GetValueOrDefault() >= nullable4.GetValueOrDefault()) || !(nullable3.HasValue & nullable4.HasValue))
                    {
                        continue;
                    }
                }
                nullable = nullable2;
            }
            return nullable;
        }

        public static float? Min(this IEnumerable<float?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            float? nullable = null;
            foreach (float? nullable2 in source)
            {
                if (nullable2.HasValue)
                {
                    if (nullable.HasValue)
                    {
                        float? nullable3 = nullable2;
                        float? nullable4 = nullable;
                        if (((nullable3.GetValueOrDefault() >= nullable4.GetValueOrDefault()) || !(nullable3.HasValue & nullable4.HasValue)) && !float.IsNaN(nullable2.Value))
                        {
                            continue;
                        }
                    }
                    nullable = nullable2;
                }
            }
            return nullable;
        }

        public static TSource Min<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            Comparer<TSource> comparer = Comparer<TSource>.Default;
            TSource y = default(TSource);
            if (y == null)
            {
                foreach (TSource local2 in source)
                {
                    if ((local2 != null) && ((y == null) || (comparer.Compare(local2, y) < 0)))
                    {
                        y = local2;
                    }
                }
                return y;
            }
            bool flag = false;
            foreach (TSource local3 in source)
            {
                if (flag)
                {
                    if (comparer.Compare(local3, y) < 0)
                    {
                        y = local3;
                    }
                }
                else
                {
                    y = local3;
                    flag = true;
                }
            }
            if (!flag)
            {
                throw Error.NoElements();
            }
            return y;
        }

        public static decimal Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            return source.Select<TSource, decimal>(selector).Min();
        }

        public static double Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            return source.Select<TSource, double>(selector).Min();
        }

        public static int Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            return source.Select<TSource, int>(selector).Min();
        }

        public static long Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            return source.Select<TSource, long>(selector).Min();
        }

        public static decimal? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            return source.Select<TSource, decimal?>(selector).Min();
        }

        public static double? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            return source.Select<TSource, double?>(selector).Min();
        }

        public static int? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            return source.Select<TSource, int?>(selector).Min();
        }

        public static long? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            return source.Select<TSource, long?>(selector).Min();
        }

        public static float Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            return source.Select<TSource, float>(selector).Min();
        }

        public static float? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
        {
            return source.Select<TSource, float?>(selector).Min();
        }

        public static TResult Min<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Select<TSource, TResult>(selector).Min<TResult>();
        }

        public static IEnumerable<TResult> OfType<TResult>(this IEnumerable source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            return OfTypeIterator<TResult>(source);
        }

        private static IEnumerable<TResult> OfTypeIterator<TResult>(IEnumerable source)
        {
            IEnumerator enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                object current = enumerator.Current;
                if (current is TResult)
                {
                    yield return (TResult) current;
                }
            }
        }

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, null, false);
        }

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, false);
        }

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, null, true);
        }

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, true);
        }

        public static IEnumerable<int> Range(int start, int count)
        {
            long num = (start + count) - 1L;
            if ((count < 0) || (num > 0x7fffffffL))
            {
                throw Error.ArgumentOutOfRange("count");
            }
            return RangeIterator(start, count);
        }

        private static IEnumerable<int> RangeIterator(int start, int count)
        {
            int iteratorVariable0 = 0;
            while (true)
            {
                if (iteratorVariable0 >= count)
                {
                    yield break;
                }
                yield return (start + iteratorVariable0);
                iteratorVariable0++;
            }
        }

        public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count)
        {
            if (count < 0)
            {
                throw Error.ArgumentOutOfRange("count");
            }
            return RepeatIterator<TResult>(element, count);
        }

        private static IEnumerable<TResult> RepeatIterator<TResult>(TResult element, int count)
        {
            int iteratorVariable0 = 0;
            while (true)
            {
                if (iteratorVariable0 >= count)
                {
                    yield break;
                }
                yield return element;
                iteratorVariable0++;
            }
        }

        public static IEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            return ReverseIterator<TSource>(source);
        }

        private static IEnumerable<TSource> ReverseIterator<TSource>(IEnumerable<TSource> source)
        {
            Buffer<TSource> iteratorVariable0 = new Buffer<TSource>(source);
            int index = iteratorVariable0.count - 1;
            while (true)
            {
                if (index < 0)
                {
                    yield break;
                }
                yield return iteratorVariable0.items[index];
                index--;
            }
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (selector == null)
            {
                throw Error.ArgumentNull("selector");
            }
            if (source is Iterator<TSource>)
            {
                return ((Iterator<TSource>) source).Select<TResult>(selector);
            }
            if (source is TSource[])
            {
                return new WhereSelectArrayIterator<TSource, TResult>((TSource[]) source, null, selector);
            }
            if (source is List<TSource>)
            {
                return new WhereSelectListIterator<TSource, TResult>((List<TSource>) source, null, selector);
            }
            return new WhereSelectEnumerableIterator<TSource, TResult>(source, null, selector);
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (selector == null)
            {
                throw Error.ArgumentNull("selector");
            }
            return SelectIterator<TSource, TResult>(source, selector);
        }

        private static IEnumerable<TResult> SelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
        {
            int iteratorVariable0 = -1;
            foreach (TSource iteratorVariable1 in source)
            {
                iteratorVariable0++;
                yield return selector(iteratorVariable1, iteratorVariable0);
            }
        }

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (selector == null)
            {
                throw Error.ArgumentNull("selector");
            }
            return SelectManyIterator<TSource, TResult>(source, selector);
        }

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (selector == null)
            {
                throw Error.ArgumentNull("selector");
            }
            return SelectManyIterator<TSource, TResult>(source, selector);
        }

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (collectionSelector == null)
            {
                throw Error.ArgumentNull("collectionSelector");
            }
            if (resultSelector == null)
            {
                throw Error.ArgumentNull("resultSelector");
            }
            return SelectManyIterator<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
        }

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (collectionSelector == null)
            {
                throw Error.ArgumentNull("collectionSelector");
            }
            if (resultSelector == null)
            {
                throw Error.ArgumentNull("resultSelector");
            }
            return SelectManyIterator<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
        }

        private static IEnumerable<TResult> SelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            foreach (TSource iteratorVariable0 in source)
            {
                foreach (TResult iteratorVariable1 in selector(iteratorVariable0))
                {
                    yield return iteratorVariable1;
                }
            }
        }

        private static IEnumerable<TResult> SelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
        {
            int iteratorVariable0 = -1;
            foreach (TSource iteratorVariable1 in source)
            {
                iteratorVariable0++;
                foreach (TResult iteratorVariable2 in selector(iteratorVariable1, iteratorVariable0))
                {
                    yield return iteratorVariable2;
                }
            }
        }

        private static IEnumerable<TResult> SelectManyIterator<TSource, TCollection, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            foreach (TSource iteratorVariable0 in source)
            {
                foreach (TCollection iteratorVariable1 in collectionSelector(iteratorVariable0))
                {
                    yield return resultSelector(iteratorVariable0, iteratorVariable1);
                }
            }
        }

        private static IEnumerable<TResult> SelectManyIterator<TSource, TCollection, TResult>(IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            int iteratorVariable0 = -1;
            foreach (TSource iteratorVariable1 in source)
            {
                iteratorVariable0++;
                foreach (TCollection iteratorVariable2 in collectionSelector(iteratorVariable1, iteratorVariable0))
                {
                    yield return resultSelector(iteratorVariable1, iteratorVariable2);
                }
            }
        }

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            return first.SequenceEqual<TSource>(second, null);
        }

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<TSource>.Default;
            }
            if (first == null)
            {
                throw Error.ArgumentNull("first");
            }
            if (second == null)
            {
                throw Error.ArgumentNull("second");
            }
            using (IEnumerator<TSource> enumerator = first.GetEnumerator())
            {
                using (IEnumerator<TSource> enumerator2 = second.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (!enumerator2.MoveNext() || !comparer.Equals(enumerator.Current, enumerator2.Current))
                        {
                            return false;
                        }
                    }
                    if (enumerator2.MoveNext())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static TSource Single<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                switch (list.Count)
                {
                    case 0:
                        throw Error.NoElements();

                    case 1:
                        return list[0];
                }
            }
            else
            {
                using (IEnumerator<TSource> enumerator = source.GetEnumerator())
                {
                    if (!enumerator.MoveNext())
                    {
                        throw Error.NoElements();
                    }
                    TSource current = enumerator.Current;
                    if (!enumerator.MoveNext())
                    {
                        return current;
                    }
                }
            }
            throw Error.MoreThanOneElement();
        }

        public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            TSource local = default(TSource);
            long num = 0L;
            foreach (TSource local2 in source)
            {
                if (predicate(local2))
                {
                    local = local2;
                    num += 1L;
                }
            }
            long num2 = num;
            if ((num2 <= 1L) && (num2 >= 0L))
            {
                switch (((int) num2))
                {
                    case 0:
                        throw Error.NoMatch();

                    case 1:
                        return local;
                }
            }
            throw Error.MoreThanOneMatch();
        }

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                switch (list.Count)
                {
                    case 0:
                        return default(TSource);

                    case 1:
                        return list[0];
                }
            }
            else
            {
                using (IEnumerator<TSource> enumerator = source.GetEnumerator())
                {
                    if (!enumerator.MoveNext())
                    {
                        return default(TSource);
                    }
                    TSource current = enumerator.Current;
                    if (!enumerator.MoveNext())
                    {
                        return current;
                    }
                }
            }
            throw Error.MoreThanOneElement();
        }

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            TSource local = default(TSource);
            long num = 0L;
            foreach (TSource local2 in source)
            {
                if (predicate(local2))
                {
                    local = local2;
                    num += 1L;
                }
            }
            long num2 = num;
            if ((num2 <= 1L) && (num2 >= 0L))
            {
                switch (((int) num2))
                {
                    case 0:
                        return default(TSource);

                    case 1:
                        return local;
                }
            }
            throw Error.MoreThanOneMatch();
        }

        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            return SkipIterator<TSource>(source, count);
        }

        private static IEnumerable<TSource> SkipIterator<TSource>(IEnumerable<TSource> source, int count)
        {
            using (IEnumerator<TSource> iteratorVariable0 = source.GetEnumerator())
            {
                while ((count > 0) && iteratorVariable0.MoveNext())
                {
                    count--;
                }
                if (count <= 0)
                {
                    while (iteratorVariable0.MoveNext())
                    {
                        yield return iteratorVariable0.Current;
                    }
                }
            }
        }

        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            return SkipWhileIterator<TSource>(source, predicate);
        }

        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            return SkipWhileIterator<TSource>(source, predicate);
        }

        private static IEnumerable<TSource> SkipWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            bool iteratorVariable0 = false;
            foreach (TSource iteratorVariable1 in source)
            {
                if (!iteratorVariable0 && !predicate(iteratorVariable1))
                {
                    iteratorVariable0 = true;
                }
                if (iteratorVariable0)
                {
                    yield return iteratorVariable1;
                }
            }
        }

        private static IEnumerable<TSource> SkipWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            int iteratorVariable0 = -1;
            bool iteratorVariable1 = false;
            foreach (TSource iteratorVariable2 in source)
            {
                iteratorVariable0++;
                if (!iteratorVariable1 && !predicate(iteratorVariable2, iteratorVariable0))
                {
                    iteratorVariable1 = true;
                }
                if (iteratorVariable1)
                {
                    yield return iteratorVariable2;
                }
            }
        }

        public static decimal Sum(this IEnumerable<decimal> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            decimal num = 0M;
            foreach (decimal num2 in source)
            {
                num += num2;
            }
            return num;
        }

        public static double Sum(this IEnumerable<double> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            double num = 0.0;
            foreach (double num2 in source)
            {
                num += num2;
            }
            return num;
        }

        public static int Sum(this IEnumerable<int> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            int num = 0;
            foreach (int num2 in source)
            {
                num += num2;
            }
            return num;
        }

        public static long Sum(this IEnumerable<long> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            long num = 0L;
            foreach (long num2 in source)
            {
                num += num2;
            }
            return num;
        }

        public static double? Sum(this IEnumerable<double?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            double num = 0.0;
            foreach (double? nullable in source)
            {
                if (nullable.HasValue)
                {
                    num += nullable.GetValueOrDefault();
                }
            }
            return new double?(num);
        }

        public static float Sum(this IEnumerable<float> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            double num = 0.0;
            foreach (float num2 in source)
            {
                num += num2;
            }
            return (float) num;
        }

        public static long? Sum(this IEnumerable<long?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            long num = 0L;
            foreach (long? nullable in source)
            {
                if (nullable.HasValue)
                {
                    num += nullable.GetValueOrDefault();
                }
            }
            return new long?(num);
        }

        public static float? Sum(this IEnumerable<float?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            double num = 0.0;
            foreach (float? nullable in source)
            {
                if (nullable.HasValue)
                {
                    num += (double) nullable.GetValueOrDefault();
                }
            }
            return new float?((float) num);
        }

        public static decimal? Sum(this IEnumerable<decimal?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            decimal num = 0M;
            foreach (decimal? nullable in source)
            {
                if (nullable.HasValue)
                {
                    num += nullable.GetValueOrDefault();
                }
            }
            return new decimal?(num);
        }

        public static int? Sum(this IEnumerable<int?> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            int num = 0;
            foreach (int? nullable in source)
            {
                if (nullable.HasValue)
                {
                    num += nullable.GetValueOrDefault();
                }
            }
            return new int?(num);
        }

        public static decimal Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            return source.Select<TSource, decimal>(selector).Sum();
        }

        public static double Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            return source.Select<TSource, double>(selector).Sum();
        }

        public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            return source.Select<TSource, int>(selector).Sum();
        }

        public static long Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            return source.Select<TSource, long>(selector).Sum();
        }

        public static decimal? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            return source.Select<TSource, decimal?>(selector).Sum();
        }

        public static double? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            return source.Select<TSource, double?>(selector).Sum();
        }

        public static int? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            return source.Select<TSource, int?>(selector).Sum();
        }

        public static float Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            return source.Select<TSource, float>(selector).Sum();
        }

        public static long? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            return source.Select<TSource, long?>(selector).Sum();
        }

        public static float? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
        {
            return source.Select<TSource, float?>(selector).Sum();
        }

        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            return TakeIterator<TSource>(source, count);
        }

        private static IEnumerable<TSource> TakeIterator<TSource>(IEnumerable<TSource> source, int count)
        {
            if (count > 0)
            {
                foreach (TSource iteratorVariable0 in source)
                {
                    yield return iteratorVariable0;
                    if (--count == 0)
                    {
                        break;
                    }
                }
            }
        }

        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            return TakeWhileIterator<TSource>(source, predicate);
        }

        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            return TakeWhileIterator<TSource>(source, predicate);
        }

        private static IEnumerable<TSource> TakeWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource iteratorVariable0 in source)
            {
                if (!predicate(iteratorVariable0))
                {
                    break;
                }
                yield return iteratorVariable0;
            }
        }

        private static IEnumerable<TSource> TakeWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            int iteratorVariable0 = -1;
            foreach (TSource iteratorVariable1 in source)
            {
                iteratorVariable0++;
                if (!predicate(iteratorVariable1, iteratorVariable0))
                {
                    break;
                }
                yield return iteratorVariable1;
            }
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            return source.CreateOrderedEnumerable<TKey>(keySelector, null, false);
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            return source.CreateOrderedEnumerable<TKey>(keySelector, comparer, false);
        }

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            return source.CreateOrderedEnumerable<TKey>(keySelector, null, true);
        }

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            return source.CreateOrderedEnumerable<TKey>(keySelector, comparer, true);
        }

        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            Buffer<TSource> buffer = new Buffer<TSource>(source);
            return buffer.ToArray();
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.ToDictionary<TSource, TKey, TSource>(keySelector, IdentityFunction<TSource>.Instance, null);
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            return source.ToDictionary<TSource, TKey, TSource>(keySelector, IdentityFunction<TSource>.Instance, comparer);
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return source.ToDictionary<TSource, TKey, TElement>(keySelector, elementSelector, null);
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (keySelector == null)
            {
                throw Error.ArgumentNull("keySelector");
            }
            if (elementSelector == null)
            {
                throw Error.ArgumentNull("elementSelector");
            }
            Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>(comparer);
            foreach (TSource local in source)
            {
                dictionary.Add(keySelector(local), elementSelector(local));
            }
            return dictionary;
        }

        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            return new List<TSource>(source);
        }

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return (ILookup<TKey, TSource>) Lookup<TKey, TSource>.Create<TSource>(source, keySelector, IdentityFunction<TSource>.Instance, null);
        }

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            return (ILookup<TKey, TSource>) Lookup<TKey, TSource>.Create<TSource>(source, keySelector, IdentityFunction<TSource>.Instance, comparer);
        }

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return (ILookup<TKey, TElement>) Lookup<TKey, TElement>.Create<TSource>(source, keySelector, elementSelector, null);
        }

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            return (ILookup<TKey, TElement>) Lookup<TKey, TElement>.Create<TSource>(source, keySelector, elementSelector, comparer);
        }

        public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            if (first == null)
            {
                throw Error.ArgumentNull("first");
            }
            if (second == null)
            {
                throw Error.ArgumentNull("second");
            }
            return UnionIterator<TSource>(first, second, null);
        }

        public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null)
            {
                throw Error.ArgumentNull("first");
            }
            if (second == null)
            {
                throw Error.ArgumentNull("second");
            }
            return UnionIterator<TSource>(first, second, comparer);
        }

        private static IEnumerable<TSource> UnionIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            Set<TSource> iteratorVariable0 = new Set<TSource>(comparer);
            foreach (TSource iteratorVariable1 in first)
            {
                if (iteratorVariable0.Add(iteratorVariable1))
                {
                    yield return iteratorVariable1;
                }
            }
            foreach (TSource iteratorVariable2 in second)
            {
                if (!iteratorVariable0.Add(iteratorVariable2))
                {
                    continue;
                }
                yield return iteratorVariable2;
            }
        }

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            if (source is Iterator<TSource>)
            {
                return ((Iterator<TSource>) source).Where(predicate);
            }
            if (source is TSource[])
            {
                return new WhereArrayIterator<TSource>((TSource[]) source, predicate);
            }
            if (source is List<TSource>)
            {
                return new WhereListIterator<TSource>((List<TSource>) source, predicate);
            }
            return new WhereEnumerableIterator<TSource>(source, predicate);
        }

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            return WhereIterator<TSource>(source, predicate);
        }

        private static IEnumerable<TSource> WhereIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            int iteratorVariable0 = -1;
            foreach (TSource iteratorVariable1 in source)
            {
                iteratorVariable0++;
                if (predicate(iteratorVariable1, iteratorVariable0))
                {
                    yield return iteratorVariable1;
                }
            }
        }

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
        {
            if (first == null)
            {
                throw Error.ArgumentNull("first");
            }
            if (second == null)
            {
                throw Error.ArgumentNull("second");
            }
            if (resultSelector == null)
            {
                throw Error.ArgumentNull("resultSelector");
            }
            return ZipIterator<TFirst, TSecond, TResult>(first, second, resultSelector);
        }

        private static IEnumerable<TResult> ZipIterator<TFirst, TSecond, TResult>(IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
        {
            using (IEnumerator<TFirst> iteratorVariable0 = first.GetEnumerator())
            {
                using (IEnumerator<TSecond> iteratorVariable1 = second.GetEnumerator())
                {
                    while (iteratorVariable0.MoveNext() && iteratorVariable1.MoveNext())
                    {
                        yield return resultSelector(iteratorVariable0.Current, iteratorVariable1.Current);
                    }
                }
            }
        }

        [CompilerGenerated]
        private sealed class <CastIterator>d__b1<TResult> : IEnumerable<TResult>, IEnumerable, IEnumerator<TResult>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TResult <>2__current;
            public IEnumerable <>3__source;
            public IEnumerator <>7__wrapb3;
            public IDisposable <>7__wrapb4;
            private int <>l__initialThreadId;
            public object <obj>5__b2;
            public IEnumerable source;

            [DebuggerHidden]
            public <CastIterator>d__b1(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finallyb5()
            {
                this.<>1__state = -1;
                this.<>7__wrapb4 = this.<>7__wrapb3 as IDisposable;
                if (this.<>7__wrapb4 != null)
                {
                    this.<>7__wrapb4.Dispose();
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
                            this.<>7__wrapb3 = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_0070;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_0070;

                        default:
                            goto Label_0083;
                    }
                Label_003C:
                    this.<obj>5__b2 = this.<>7__wrapb3.Current;
                    this.<>2__current = (TResult) this.<obj>5__b2;
                    this.<>1__state = 2;
                    return true;
                Label_0070:
                    if (this.<>7__wrapb3.MoveNext())
                    {
                        goto Label_003C;
                    }
                    this.<>m__Finallyb5();
                Label_0083:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
            {
                Enumerable.<CastIterator>d__b1<TResult> _b;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    _b = (Enumerable.<CastIterator>d__b1<TResult>) this;
                }
                else
                {
                    _b = new Enumerable.<CastIterator>d__b1<TResult>(0);
                }
                _b.source = this.<>3__source;
                return _b;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TResult>.GetEnumerator();
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
                            this.<>m__Finallyb5();
                        }
                        return;
                }
            }

            TResult IEnumerator<TResult>.Current
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
        private sealed class <ConcatIterator>d__71<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TSource <>2__current;
            public IEnumerable<TSource> <>3__first;
            public IEnumerable<TSource> <>3__second;
            public IEnumerator<TSource> <>7__wrap74;
            public IEnumerator<TSource> <>7__wrap76;
            private int <>l__initialThreadId;
            public TSource <element>5__72;
            public TSource <element>5__73;
            public IEnumerable<TSource> first;
            public IEnumerable<TSource> second;

            [DebuggerHidden]
            public <ConcatIterator>d__71(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally75()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap74 != null)
                {
                    this.<>7__wrap74.Dispose();
                }
            }

            private void <>m__Finally77()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap76 != null)
                {
                    this.<>7__wrap76.Dispose();
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
                            this.<>7__wrap74 = this.first.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_0079;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_0079;

                        case 4:
                            goto Label_00CE;

                        default:
                            goto Label_00E8;
                    }
                Label_0047:
                    this.<element>5__72 = this.<>7__wrap74.Current;
                    this.<>2__current = this.<element>5__72;
                    this.<>1__state = 2;
                    return true;
                Label_0079:
                    if (this.<>7__wrap74.MoveNext())
                    {
                        goto Label_0047;
                    }
                    this.<>m__Finally75();
                    this.<>7__wrap76 = this.second.GetEnumerator();
                    this.<>1__state = 3;
                    while (this.<>7__wrap76.MoveNext())
                    {
                        this.<element>5__73 = this.<>7__wrap76.Current;
                        this.<>2__current = this.<element>5__73;
                        this.<>1__state = 4;
                        return true;
                    Label_00CE:
                        this.<>1__state = 3;
                    }
                    this.<>m__Finally77();
                Label_00E8:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                Enumerable.<ConcatIterator>d__71<TSource> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Enumerable.<ConcatIterator>d__71<TSource>) this;
                }
                else
                {
                    d__ = new Enumerable.<ConcatIterator>d__71<TSource>(0);
                }
                d__.first = this.<>3__first;
                d__.second = this.<>3__second;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();
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
                            this.<>m__Finally75();
                        }
                        break;

                    case 3:
                    case 4:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally77();
                        }
                        return;
                }
            }

            TSource IEnumerator<TSource>.Current
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
        private sealed class <DefaultIfEmptyIterator>d__a5<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TSource <>2__current;
            public TSource <>3__defaultValue;
            public IEnumerable<TSource> <>3__source;
            private int <>l__initialThreadId;
            public IEnumerator<TSource> <e>5__a6;
            public TSource defaultValue;
            public IEnumerable<TSource> source;

            [DebuggerHidden]
            public <DefaultIfEmptyIterator>d__a5(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finallya7()
            {
                this.<>1__state = -1;
                if (this.<e>5__a6 != null)
                {
                    this.<e>5__a6.Dispose();
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
                            break;

                        case 2:
                            this.<>1__state = 1;
                            if (this.<e>5__a6.MoveNext())
                            {
                                goto Label_004E;
                            }
                            goto Label_009E;

                        case 3:
                            this.<>1__state = 1;
                            goto Label_009E;

                        default:
                            goto Label_00A4;
                    }
                    this.<>1__state = -1;
                    this.<e>5__a6 = this.source.GetEnumerator();
                    this.<>1__state = 1;
                    if (!this.<e>5__a6.MoveNext())
                    {
                        goto Label_0080;
                    }
                Label_004E:
                    this.<>2__current = this.<e>5__a6.Current;
                    this.<>1__state = 2;
                    return true;
                Label_0080:
                    this.<>2__current = this.defaultValue;
                    this.<>1__state = 3;
                    return true;
                Label_009E:
                    this.<>m__Finallya7();
                Label_00A4:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                Enumerable.<DefaultIfEmptyIterator>d__a5<TSource> _a;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    _a = (Enumerable.<DefaultIfEmptyIterator>d__a5<TSource>) this;
                }
                else
                {
                    _a = new Enumerable.<DefaultIfEmptyIterator>d__a5<TSource>(0);
                }
                _a.source = this.<>3__source;
                _a.defaultValue = this.<>3__defaultValue;
                return _a;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();
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
                    case 3:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finallya7();
                        }
                        return;
                }
            }

            TSource IEnumerator<TSource>.Current
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
        private sealed class <DistinctIterator>d__81<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TSource <>2__current;
            public IEqualityComparer<TSource> <>3__comparer;
            public IEnumerable<TSource> <>3__source;
            public IEnumerator<TSource> <>7__wrap84;
            private int <>l__initialThreadId;
            public TSource <element>5__83;
            public Set<TSource> <set>5__82;
            public IEqualityComparer<TSource> comparer;
            public IEnumerable<TSource> source;

            [DebuggerHidden]
            public <DistinctIterator>d__81(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally85()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap84 != null)
                {
                    this.<>7__wrap84.Dispose();
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
                            this.<set>5__82 = new Set<TSource>(this.comparer);
                            this.<>7__wrap84 = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_0092;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_0092;

                        default:
                            goto Label_00A5;
                    }
                Label_0050:
                    this.<element>5__83 = this.<>7__wrap84.Current;
                    if (this.<set>5__82.Add(this.<element>5__83))
                    {
                        this.<>2__current = this.<element>5__83;
                        this.<>1__state = 2;
                        return true;
                    }
                Label_0092:
                    if (this.<>7__wrap84.MoveNext())
                    {
                        goto Label_0050;
                    }
                    this.<>m__Finally85();
                Label_00A5:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                Enumerable.<DistinctIterator>d__81<TSource> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Enumerable.<DistinctIterator>d__81<TSource>) this;
                }
                else
                {
                    d__ = new Enumerable.<DistinctIterator>d__81<TSource>(0);
                }
                d__.source = this.<>3__source;
                d__.comparer = this.<>3__comparer;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();
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
                            this.<>m__Finally85();
                        }
                        return;
                }
            }

            TSource IEnumerator<TSource>.Current
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
        private sealed class <ExceptIterator>d__99<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TSource <>2__current;
            public IEqualityComparer<TSource> <>3__comparer;
            public IEnumerable<TSource> <>3__first;
            public IEnumerable<TSource> <>3__second;
            public IEnumerator<TSource> <>7__wrap9c;
            private int <>l__initialThreadId;
            public TSource <element>5__9b;
            public Set<TSource> <set>5__9a;
            public IEqualityComparer<TSource> comparer;
            public IEnumerable<TSource> first;
            public IEnumerable<TSource> second;

            [DebuggerHidden]
            public <ExceptIterator>d__99(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally9d()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap9c != null)
                {
                    this.<>7__wrap9c.Dispose();
                }
            }

            private bool MoveNext()
            {
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<set>5__9a = new Set<TSource>(this.comparer);
                            foreach (TSource local in this.second)
                            {
                                this.<set>5__9a.Add(local);
                            }
                            this.<>7__wrap9c = this.first.GetEnumerator();
                            this.<>1__state = 2;
                            while (this.<>7__wrap9c.MoveNext())
                            {
                                this.<element>5__9b = this.<>7__wrap9c.Current;
                                if (!this.<set>5__9a.Add(this.<element>5__9b))
                                {
                                    continue;
                                }
                                this.<>2__current = this.<element>5__9b;
                                this.<>1__state = 3;
                                return true;
                            Label_00BA:
                                this.<>1__state = 2;
                            }
                            this.<>m__Finally9d();
                            break;

                        case 3:
                            goto Label_00BA;
                    }
                    return false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                Enumerable.<ExceptIterator>d__99<TSource> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Enumerable.<ExceptIterator>d__99<TSource>) this;
                }
                else
                {
                    d__ = new Enumerable.<ExceptIterator>d__99<TSource>(0);
                }
                d__.first = this.<>3__first;
                d__.second = this.<>3__second;
                d__.comparer = this.<>3__comparer;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();
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
                    case 2:
                    case 3:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally9d();
                        }
                        return;
                }
            }

            TSource IEnumerator<TSource>.Current
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
        private sealed class <GroupJoinIterator>d__6a<TOuter, TInner, TKey, TResult> : IEnumerable<TResult>, IEnumerable, IEnumerator<TResult>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TResult <>2__current;
            public IEqualityComparer<TKey> <>3__comparer;
            public IEnumerable<TInner> <>3__inner;
            public Func<TInner, TKey> <>3__innerKeySelector;
            public IEnumerable<TOuter> <>3__outer;
            public Func<TOuter, TKey> <>3__outerKeySelector;
            public Func<TOuter, IEnumerable<TInner>, TResult> <>3__resultSelector;
            public IEnumerator<TOuter> <>7__wrap6d;
            private int <>l__initialThreadId;
            public TOuter <item>5__6c;
            public Lookup<TKey, TInner> <lookup>5__6b;
            public IEqualityComparer<TKey> comparer;
            public IEnumerable<TInner> inner;
            public Func<TInner, TKey> innerKeySelector;
            public IEnumerable<TOuter> outer;
            public Func<TOuter, TKey> outerKeySelector;
            public Func<TOuter, IEnumerable<TInner>, TResult> resultSelector;

            [DebuggerHidden]
            public <GroupJoinIterator>d__6a(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally6e()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap6d != null)
                {
                    this.<>7__wrap6d.Dispose();
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
                            this.<lookup>5__6b = Lookup<TKey, TInner>.CreateForJoin(this.inner, this.innerKeySelector, this.comparer);
                            this.<>7__wrap6d = this.outer.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_00B2;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_00B2;

                        default:
                            goto Label_00C5;
                    }
                Label_005C:
                    this.<item>5__6c = this.<>7__wrap6d.Current;
                    this.<>2__current = this.resultSelector(this.<item>5__6c, this.<lookup>5__6b[this.outerKeySelector(this.<item>5__6c)]);
                    this.<>1__state = 2;
                    return true;
                Label_00B2:
                    if (this.<>7__wrap6d.MoveNext())
                    {
                        goto Label_005C;
                    }
                    this.<>m__Finally6e();
                Label_00C5:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
            {
                Enumerable.<GroupJoinIterator>d__6a<TOuter, TInner, TKey, TResult> d__a;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__a = (Enumerable.<GroupJoinIterator>d__6a<TOuter, TInner, TKey, TResult>) this;
                }
                else
                {
                    d__a = new Enumerable.<GroupJoinIterator>d__6a<TOuter, TInner, TKey, TResult>(0);
                }
                d__a.outer = this.<>3__outer;
                d__a.inner = this.<>3__inner;
                d__a.outerKeySelector = this.<>3__outerKeySelector;
                d__a.innerKeySelector = this.<>3__innerKeySelector;
                d__a.resultSelector = this.<>3__resultSelector;
                d__a.comparer = this.<>3__comparer;
                return d__a;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TResult>.GetEnumerator();
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
                            this.<>m__Finally6e();
                        }
                        return;
                }
            }

            TResult IEnumerator<TResult>.Current
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
        private sealed class <IntersectIterator>d__92<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TSource <>2__current;
            public IEqualityComparer<TSource> <>3__comparer;
            public IEnumerable<TSource> <>3__first;
            public IEnumerable<TSource> <>3__second;
            public IEnumerator<TSource> <>7__wrap95;
            private int <>l__initialThreadId;
            public TSource <element>5__94;
            public Set<TSource> <set>5__93;
            public IEqualityComparer<TSource> comparer;
            public IEnumerable<TSource> first;
            public IEnumerable<TSource> second;

            [DebuggerHidden]
            public <IntersectIterator>d__92(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally96()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap95 != null)
                {
                    this.<>7__wrap95.Dispose();
                }
            }

            private bool MoveNext()
            {
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<set>5__93 = new Set<TSource>(this.comparer);
                            foreach (TSource local in this.second)
                            {
                                this.<set>5__93.Add(local);
                            }
                            this.<>7__wrap95 = this.first.GetEnumerator();
                            this.<>1__state = 2;
                            while (this.<>7__wrap95.MoveNext())
                            {
                                this.<element>5__94 = this.<>7__wrap95.Current;
                                if (!this.<set>5__93.Remove(this.<element>5__94))
                                {
                                    continue;
                                }
                                this.<>2__current = this.<element>5__94;
                                this.<>1__state = 3;
                                return true;
                            Label_00BA:
                                this.<>1__state = 2;
                            }
                            this.<>m__Finally96();
                            break;

                        case 3:
                            goto Label_00BA;
                    }
                    return false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                Enumerable.<IntersectIterator>d__92<TSource> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Enumerable.<IntersectIterator>d__92<TSource>) this;
                }
                else
                {
                    d__ = new Enumerable.<IntersectIterator>d__92<TSource>(0);
                }
                d__.first = this.<>3__first;
                d__.second = this.<>3__second;
                d__.comparer = this.<>3__comparer;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();
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
                    case 2:
                    case 3:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally96();
                        }
                        return;
                }
            }

            TSource IEnumerator<TSource>.Current
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
        private sealed class <JoinIterator>d__61<TOuter, TInner, TKey, TResult> : IEnumerable<TResult>, IEnumerable, IEnumerator<TResult>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TResult <>2__current;
            public IEqualityComparer<TKey> <>3__comparer;
            public IEnumerable<TInner> <>3__inner;
            public Func<TInner, TKey> <>3__innerKeySelector;
            public IEnumerable<TOuter> <>3__outer;
            public Func<TOuter, TKey> <>3__outerKeySelector;
            public Func<TOuter, TInner, TResult> <>3__resultSelector;
            public IEnumerator<TOuter> <>7__wrap66;
            private int <>l__initialThreadId;
            public Lookup<TKey, TInner>.Grouping <g>5__64;
            public int <i>5__65;
            public TOuter <item>5__63;
            public Lookup<TKey, TInner> <lookup>5__62;
            public IEqualityComparer<TKey> comparer;
            public IEnumerable<TInner> inner;
            public Func<TInner, TKey> innerKeySelector;
            public IEnumerable<TOuter> outer;
            public Func<TOuter, TKey> outerKeySelector;
            public Func<TOuter, TInner, TResult> resultSelector;

            [DebuggerHidden]
            public <JoinIterator>d__61(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally67()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap66 != null)
                {
                    this.<>7__wrap66.Dispose();
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
                            this.<lookup>5__62 = Lookup<TKey, TInner>.CreateForJoin(this.inner, this.innerKeySelector, this.comparer);
                            this.<>7__wrap66 = this.outer.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_0104;

                        case 2:
                            goto Label_00DC;

                        default:
                            goto Label_011A;
                    }
                Label_005F:
                    this.<item>5__63 = this.<>7__wrap66.Current;
                    this.<g>5__64 = this.<lookup>5__62.GetGrouping(this.outerKeySelector(this.<item>5__63), false);
                    if (this.<g>5__64 != null)
                    {
                        this.<i>5__65 = 0;
                        while (this.<i>5__65 < this.<g>5__64.count)
                        {
                            this.<>2__current = this.resultSelector(this.<item>5__63, this.<g>5__64.elements[this.<i>5__65]);
                            this.<>1__state = 2;
                            return true;
                        Label_00DC:
                            this.<>1__state = 1;
                            this.<i>5__65++;
                        }
                    }
                Label_0104:
                    if (this.<>7__wrap66.MoveNext())
                    {
                        goto Label_005F;
                    }
                    this.<>m__Finally67();
                Label_011A:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
            {
                Enumerable.<JoinIterator>d__61<TOuter, TInner, TKey, TResult> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Enumerable.<JoinIterator>d__61<TOuter, TInner, TKey, TResult>) this;
                }
                else
                {
                    d__ = new Enumerable.<JoinIterator>d__61<TOuter, TInner, TKey, TResult>(0);
                }
                d__.outer = this.<>3__outer;
                d__.inner = this.<>3__inner;
                d__.outerKeySelector = this.<>3__outerKeySelector;
                d__.innerKeySelector = this.<>3__innerKeySelector;
                d__.resultSelector = this.<>3__resultSelector;
                d__.comparer = this.<>3__comparer;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TResult>.GetEnumerator();
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
                            this.<>m__Finally67();
                        }
                        return;
                }
            }

            TResult IEnumerator<TResult>.Current
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
        private sealed class <OfTypeIterator>d__aa<TResult> : IEnumerable<TResult>, IEnumerable, IEnumerator<TResult>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TResult <>2__current;
            public IEnumerable <>3__source;
            public IEnumerator <>7__wrapac;
            public IDisposable <>7__wrapad;
            private int <>l__initialThreadId;
            public object <obj>5__ab;
            public IEnumerable source;

            [DebuggerHidden]
            public <OfTypeIterator>d__aa(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finallyae()
            {
                this.<>1__state = -1;
                this.<>7__wrapad = this.<>7__wrapac as IDisposable;
                if (this.<>7__wrapad != null)
                {
                    this.<>7__wrapad.Dispose();
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
                            this.<>7__wrapac = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_007D;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_007D;

                        default:
                            goto Label_0090;
                    }
                Label_003C:
                    this.<obj>5__ab = this.<>7__wrapac.Current;
                    if (this.<obj>5__ab is TResult)
                    {
                        this.<>2__current = (TResult) this.<obj>5__ab;
                        this.<>1__state = 2;
                        return true;
                    }
                Label_007D:
                    if (this.<>7__wrapac.MoveNext())
                    {
                        goto Label_003C;
                    }
                    this.<>m__Finallyae();
                Label_0090:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
            {
                Enumerable.<OfTypeIterator>d__aa<TResult> _aa;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    _aa = (Enumerable.<OfTypeIterator>d__aa<TResult>) this;
                }
                else
                {
                    _aa = new Enumerable.<OfTypeIterator>d__aa<TResult>(0);
                }
                _aa.source = this.<>3__source;
                return _aa;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TResult>.GetEnumerator();
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
                            this.<>m__Finallyae();
                        }
                        return;
                }
            }

            TResult IEnumerator<TResult>.Current
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
        private sealed class <RepeatIterator>d__bc<TResult> : IEnumerable<TResult>, IEnumerable, IEnumerator<TResult>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TResult <>2__current;
            public int <>3__count;
            public TResult <>3__element;
            private int <>l__initialThreadId;
            public int <i>5__bd;
            public int count;
            public TResult element;

            [DebuggerHidden]
            public <RepeatIterator>d__bc(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<i>5__bd = 0;
                        break;

                    case 1:
                        this.<>1__state = -1;
                        this.<i>5__bd++;
                        break;

                    default:
                        goto Label_005F;
                }
                if (this.<i>5__bd < this.count)
                {
                    this.<>2__current = this.element;
                    this.<>1__state = 1;
                    return true;
                }
            Label_005F:
                return false;
            }

            [DebuggerHidden]
            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
            {
                Enumerable.<RepeatIterator>d__bc<TResult> _bc;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    _bc = (Enumerable.<RepeatIterator>d__bc<TResult>) this;
                }
                else
                {
                    _bc = new Enumerable.<RepeatIterator>d__bc<TResult>(0);
                }
                _bc.element = this.<>3__element;
                _bc.count = this.<>3__count;
                return _bc;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TResult>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
            }

            TResult IEnumerator<TResult>.Current
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
        private sealed class <ReverseIterator>d__a0<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TSource <>2__current;
            public IEnumerable<TSource> <>3__source;
            private int <>l__initialThreadId;
            public Buffer<TSource> <buffer>5__a1;
            public int <i>5__a2;
            public IEnumerable<TSource> source;

            [DebuggerHidden]
            public <ReverseIterator>d__a0(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<buffer>5__a1 = new Buffer<TSource>(this.source);
                        this.<i>5__a2 = this.<buffer>5__a1.count - 1;
                        break;

                    case 1:
                        this.<>1__state = -1;
                        this.<i>5__a2--;
                        break;

                    default:
                        goto Label_008C;
                }
                if (this.<i>5__a2 >= 0)
                {
                    this.<>2__current = this.<buffer>5__a1.items[this.<i>5__a2];
                    this.<>1__state = 1;
                    return true;
                }
            Label_008C:
                return false;
            }

            [DebuggerHidden]
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                Enumerable.<ReverseIterator>d__a0<TSource> _a;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    _a = (Enumerable.<ReverseIterator>d__a0<TSource>) this;
                }
                else
                {
                    _a = new Enumerable.<ReverseIterator>d__a0<TSource>(0);
                }
                _a.source = this.<>3__source;
                return _a;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
            }

            TSource IEnumerator<TSource>.Current
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
        private sealed class <SelectIterator>d__7<TSource, TResult> : IEnumerable<TResult>, IEnumerable, IEnumerator<TResult>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TResult <>2__current;
            public Func<TSource, int, TResult> <>3__selector;
            public IEnumerable<TSource> <>3__source;
            public IEnumerator<TSource> <>7__wrapa;
            private int <>l__initialThreadId;
            public TSource <element>5__9;
            public int <index>5__8;
            public Func<TSource, int, TResult> selector;
            public IEnumerable<TSource> source;

            [DebuggerHidden]
            public <SelectIterator>d__7(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finallyb()
            {
                this.<>1__state = -1;
                if (this.<>7__wrapa != null)
                {
                    this.<>7__wrapa.Dispose();
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
                            this.<index>5__8 = -1;
                            this.<>7__wrapa = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_0094;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_0094;

                        default:
                            goto Label_00A7;
                    }
                Label_0046:
                    this.<element>5__9 = this.<>7__wrapa.Current;
                    this.<index>5__8++;
                    this.<>2__current = this.selector(this.<element>5__9, this.<index>5__8);
                    this.<>1__state = 2;
                    return true;
                Label_0094:
                    if (this.<>7__wrapa.MoveNext())
                    {
                        goto Label_0046;
                    }
                    this.<>m__Finallyb();
                Label_00A7:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
            {
                Enumerable.<SelectIterator>d__7<TSource, TResult> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Enumerable.<SelectIterator>d__7<TSource, TResult>) this;
                }
                else
                {
                    d__ = new Enumerable.<SelectIterator>d__7<TSource, TResult>(0);
                }
                d__.source = this.<>3__source;
                d__.selector = this.<>3__selector;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TResult>.GetEnumerator();
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
                            this.<>m__Finallyb();
                        }
                        return;
                }
            }

            TResult IEnumerator<TResult>.Current
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
        private sealed class <SelectManyIterator>d__14<TSource, TResult> : IEnumerable<TResult>, IEnumerable, IEnumerator<TResult>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TResult <>2__current;
            public Func<TSource, IEnumerable<TResult>> <>3__selector;
            public IEnumerable<TSource> <>3__source;
            public IEnumerator<TSource> <>7__wrap17;
            public IEnumerator<TResult> <>7__wrap19;
            private int <>l__initialThreadId;
            public TSource <element>5__15;
            public TResult <subElement>5__16;
            public Func<TSource, IEnumerable<TResult>> selector;
            public IEnumerable<TSource> source;

            [DebuggerHidden]
            public <SelectManyIterator>d__14(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally18()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap17 != null)
                {
                    this.<>7__wrap17.Dispose();
                }
            }

            private void <>m__Finally1a()
            {
                this.<>1__state = 1;
                if (this.<>7__wrap19 != null)
                {
                    this.<>7__wrap19.Dispose();
                }
            }

            private bool MoveNext()
            {
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<>7__wrap17 = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            while (this.<>7__wrap17.MoveNext())
                            {
                                this.<element>5__15 = this.<>7__wrap17.Current;
                                this.<>7__wrap19 = this.selector(this.<element>5__15).GetEnumerator();
                                this.<>1__state = 2;
                                while (this.<>7__wrap19.MoveNext())
                                {
                                    this.<subElement>5__16 = this.<>7__wrap19.Current;
                                    this.<>2__current = this.<subElement>5__16;
                                    this.<>1__state = 3;
                                    return true;
                                Label_0096:
                                    this.<>1__state = 2;
                                }
                                this.<>m__Finally1a();
                            }
                            this.<>m__Finally18();
                            break;

                        case 3:
                            goto Label_0096;
                    }
                    return false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
            {
                Enumerable.<SelectManyIterator>d__14<TSource, TResult> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Enumerable.<SelectManyIterator>d__14<TSource, TResult>) this;
                }
                else
                {
                    d__ = new Enumerable.<SelectManyIterator>d__14<TSource, TResult>(0);
                }
                d__.source = this.<>3__source;
                d__.selector = this.<>3__selector;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TResult>.GetEnumerator();
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
                    case 3:
                        try
                        {
                            switch (this.<>1__state)
                            {
                                case 2:
                                case 3:
                                    try
                                    {
                                    }
                                    finally
                                    {
                                        this.<>m__Finally1a();
                                    }
                                    return;
                            }
                        }
                        finally
                        {
                            this.<>m__Finally18();
                        }
                        break;

                    default:
                        return;
                }
            }

            TResult IEnumerator<TResult>.Current
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
        private sealed class <SelectManyIterator>d__1d<TSource, TResult> : IEnumerable<TResult>, IEnumerable, IEnumerator<TResult>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TResult <>2__current;
            public Func<TSource, int, IEnumerable<TResult>> <>3__selector;
            public IEnumerable<TSource> <>3__source;
            public IEnumerator<TSource> <>7__wrap21;
            public IEnumerator<TResult> <>7__wrap23;
            private int <>l__initialThreadId;
            public TSource <element>5__1f;
            public int <index>5__1e;
            public TResult <subElement>5__20;
            public Func<TSource, int, IEnumerable<TResult>> selector;
            public IEnumerable<TSource> source;

            [DebuggerHidden]
            public <SelectManyIterator>d__1d(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally22()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap21 != null)
                {
                    this.<>7__wrap21.Dispose();
                }
            }

            private void <>m__Finally24()
            {
                this.<>1__state = 1;
                if (this.<>7__wrap23 != null)
                {
                    this.<>7__wrap23.Dispose();
                }
            }

            private bool MoveNext()
            {
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<index>5__1e = -1;
                            this.<>7__wrap21 = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            while (this.<>7__wrap21.MoveNext())
                            {
                                this.<element>5__1f = this.<>7__wrap21.Current;
                                this.<index>5__1e++;
                                this.<>7__wrap23 = this.selector(this.<element>5__1f, this.<index>5__1e).GetEnumerator();
                                this.<>1__state = 2;
                                while (this.<>7__wrap23.MoveNext())
                                {
                                    this.<subElement>5__20 = this.<>7__wrap23.Current;
                                    this.<>2__current = this.<subElement>5__20;
                                    this.<>1__state = 3;
                                    return true;
                                Label_00B4:
                                    this.<>1__state = 2;
                                }
                                this.<>m__Finally24();
                            }
                            this.<>m__Finally22();
                            break;

                        case 3:
                            goto Label_00B4;
                    }
                    return false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
            {
                Enumerable.<SelectManyIterator>d__1d<TSource, TResult> d__d;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__d = (Enumerable.<SelectManyIterator>d__1d<TSource, TResult>) this;
                }
                else
                {
                    d__d = new Enumerable.<SelectManyIterator>d__1d<TSource, TResult>(0);
                }
                d__d.source = this.<>3__source;
                d__d.selector = this.<>3__selector;
                return d__d;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TResult>.GetEnumerator();
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
                    case 3:
                        try
                        {
                            switch (this.<>1__state)
                            {
                                case 2:
                                case 3:
                                    try
                                    {
                                    }
                                    finally
                                    {
                                        this.<>m__Finally24();
                                    }
                                    return;
                            }
                        }
                        finally
                        {
                            this.<>m__Finally22();
                        }
                        break;

                    default:
                        return;
                }
            }

            TResult IEnumerator<TResult>.Current
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
        private sealed class <SelectManyIterator>d__27<TSource, TCollection, TResult> : IEnumerable<TResult>, IEnumerable, IEnumerator<TResult>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TResult <>2__current;
            public Func<TSource, int, IEnumerable<TCollection>> <>3__collectionSelector;
            public Func<TSource, TCollection, TResult> <>3__resultSelector;
            public IEnumerable<TSource> <>3__source;
            public IEnumerator<TSource> <>7__wrap2b;
            public IEnumerator<TCollection> <>7__wrap2d;
            private int <>l__initialThreadId;
            public TSource <element>5__29;
            public int <index>5__28;
            public TCollection <subElement>5__2a;
            public Func<TSource, int, IEnumerable<TCollection>> collectionSelector;
            public Func<TSource, TCollection, TResult> resultSelector;
            public IEnumerable<TSource> source;

            [DebuggerHidden]
            public <SelectManyIterator>d__27(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally2c()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap2b != null)
                {
                    this.<>7__wrap2b.Dispose();
                }
            }

            private void <>m__Finally2e()
            {
                this.<>1__state = 1;
                if (this.<>7__wrap2d != null)
                {
                    this.<>7__wrap2d.Dispose();
                }
            }

            private bool MoveNext()
            {
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<index>5__28 = -1;
                            this.<>7__wrap2b = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            while (this.<>7__wrap2b.MoveNext())
                            {
                                this.<element>5__29 = this.<>7__wrap2b.Current;
                                this.<index>5__28++;
                                this.<>7__wrap2d = this.collectionSelector(this.<element>5__29, this.<index>5__28).GetEnumerator();
                                this.<>1__state = 2;
                                while (this.<>7__wrap2d.MoveNext())
                                {
                                    this.<subElement>5__2a = this.<>7__wrap2d.Current;
                                    this.<>2__current = this.resultSelector(this.<element>5__29, this.<subElement>5__2a);
                                    this.<>1__state = 3;
                                    return true;
                                Label_00C5:
                                    this.<>1__state = 2;
                                }
                                this.<>m__Finally2e();
                            }
                            this.<>m__Finally2c();
                            break;

                        case 3:
                            goto Label_00C5;
                    }
                    return false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
            {
                Enumerable.<SelectManyIterator>d__27<TSource, TCollection, TResult> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Enumerable.<SelectManyIterator>d__27<TSource, TCollection, TResult>) this;
                }
                else
                {
                    d__ = new Enumerable.<SelectManyIterator>d__27<TSource, TCollection, TResult>(0);
                }
                d__.source = this.<>3__source;
                d__.collectionSelector = this.<>3__collectionSelector;
                d__.resultSelector = this.<>3__resultSelector;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TResult>.GetEnumerator();
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
                    case 3:
                        try
                        {
                            switch (this.<>1__state)
                            {
                                case 2:
                                case 3:
                                    try
                                    {
                                    }
                                    finally
                                    {
                                        this.<>m__Finally2e();
                                    }
                                    return;
                            }
                        }
                        finally
                        {
                            this.<>m__Finally2c();
                        }
                        break;

                    default:
                        return;
                }
            }

            TResult IEnumerator<TResult>.Current
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
        private sealed class <SelectManyIterator>d__31<TSource, TCollection, TResult> : IEnumerable<TResult>, IEnumerable, IEnumerator<TResult>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TResult <>2__current;
            public Func<TSource, IEnumerable<TCollection>> <>3__collectionSelector;
            public Func<TSource, TCollection, TResult> <>3__resultSelector;
            public IEnumerable<TSource> <>3__source;
            public IEnumerator<TSource> <>7__wrap34;
            public IEnumerator<TCollection> <>7__wrap36;
            private int <>l__initialThreadId;
            public TSource <element>5__32;
            public TCollection <subElement>5__33;
            public Func<TSource, IEnumerable<TCollection>> collectionSelector;
            public Func<TSource, TCollection, TResult> resultSelector;
            public IEnumerable<TSource> source;

            [DebuggerHidden]
            public <SelectManyIterator>d__31(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally35()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap34 != null)
                {
                    this.<>7__wrap34.Dispose();
                }
            }

            private void <>m__Finally37()
            {
                this.<>1__state = 1;
                if (this.<>7__wrap36 != null)
                {
                    this.<>7__wrap36.Dispose();
                }
            }

            private bool MoveNext()
            {
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<>7__wrap34 = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            while (this.<>7__wrap34.MoveNext())
                            {
                                this.<element>5__32 = this.<>7__wrap34.Current;
                                this.<>7__wrap36 = this.collectionSelector(this.<element>5__32).GetEnumerator();
                                this.<>1__state = 2;
                                while (this.<>7__wrap36.MoveNext())
                                {
                                    this.<subElement>5__33 = this.<>7__wrap36.Current;
                                    this.<>2__current = this.resultSelector(this.<element>5__32, this.<subElement>5__33);
                                    this.<>1__state = 3;
                                    return true;
                                Label_00AA:
                                    this.<>1__state = 2;
                                }
                                this.<>m__Finally37();
                            }
                            this.<>m__Finally35();
                            break;

                        case 3:
                            goto Label_00AA;
                    }
                    return false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
            {
                Enumerable.<SelectManyIterator>d__31<TSource, TCollection, TResult> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Enumerable.<SelectManyIterator>d__31<TSource, TCollection, TResult>) this;
                }
                else
                {
                    d__ = new Enumerable.<SelectManyIterator>d__31<TSource, TCollection, TResult>(0);
                }
                d__.source = this.<>3__source;
                d__.collectionSelector = this.<>3__collectionSelector;
                d__.resultSelector = this.<>3__resultSelector;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TResult>.GetEnumerator();
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
                    case 3:
                        try
                        {
                            switch (this.<>1__state)
                            {
                                case 2:
                                case 3:
                                    try
                                    {
                                    }
                                    finally
                                    {
                                        this.<>m__Finally37();
                                    }
                                    return;
                            }
                        }
                        finally
                        {
                            this.<>m__Finally35();
                        }
                        break;

                    default:
                        return;
                }
            }

            TResult IEnumerator<TResult>.Current
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
        private sealed class <SkipIterator>d__4d<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TSource <>2__current;
            public int <>3__count;
            public IEnumerable<TSource> <>3__source;
            private int <>l__initialThreadId;
            public IEnumerator<TSource> <e>5__4e;
            public int count;
            public IEnumerable<TSource> source;

            [DebuggerHidden]
            public <SkipIterator>d__4d(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally4f()
            {
                this.<>1__state = -1;
                if (this.<e>5__4e != null)
                {
                    this.<e>5__4e.Dispose();
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
                            this.<e>5__4e = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_004D;

                        case 2:
                            goto Label_008A;

                        default:
                            goto Label_00A4;
                    }
                Label_003F:
                    this.count--;
                Label_004D:
                    if ((this.count > 0) && this.<e>5__4e.MoveNext())
                    {
                        goto Label_003F;
                    }
                    if (this.count <= 0)
                    {
                        while (this.<e>5__4e.MoveNext())
                        {
                            this.<>2__current = this.<e>5__4e.Current;
                            this.<>1__state = 2;
                            return true;
                        Label_008A:
                            this.<>1__state = 1;
                        }
                    }
                    this.<>m__Finally4f();
                Label_00A4:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                Enumerable.<SkipIterator>d__4d<TSource> d__d;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__d = (Enumerable.<SkipIterator>d__4d<TSource>) this;
                }
                else
                {
                    d__d = new Enumerable.<SkipIterator>d__4d<TSource>(0);
                }
                d__d.source = this.<>3__source;
                d__d.count = this.<>3__count;
                return d__d;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();
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
                            this.<>m__Finally4f();
                        }
                        return;
                }
            }

            TSource IEnumerator<TSource>.Current
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
        private sealed class <SkipWhileIterator>d__52<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TSource <>2__current;
            public Func<TSource, bool> <>3__predicate;
            public IEnumerable<TSource> <>3__source;
            public IEnumerator<TSource> <>7__wrap55;
            private int <>l__initialThreadId;
            public TSource <element>5__54;
            public bool <yielding>5__53;
            public Func<TSource, bool> predicate;
            public IEnumerable<TSource> source;

            [DebuggerHidden]
            public <SkipWhileIterator>d__52(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally56()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap55 != null)
                {
                    this.<>7__wrap55.Dispose();
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
                            this.<yielding>5__53 = false;
                            this.<>7__wrap55 = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_009F;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_009F;

                        default:
                            goto Label_00B2;
                    }
                Label_0046:
                    this.<element>5__54 = this.<>7__wrap55.Current;
                    if (!this.<yielding>5__53 && !this.predicate(this.<element>5__54))
                    {
                        this.<yielding>5__53 = true;
                    }
                    if (this.<yielding>5__53)
                    {
                        this.<>2__current = this.<element>5__54;
                        this.<>1__state = 2;
                        return true;
                    }
                Label_009F:
                    if (this.<>7__wrap55.MoveNext())
                    {
                        goto Label_0046;
                    }
                    this.<>m__Finally56();
                Label_00B2:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                Enumerable.<SkipWhileIterator>d__52<TSource> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Enumerable.<SkipWhileIterator>d__52<TSource>) this;
                }
                else
                {
                    d__ = new Enumerable.<SkipWhileIterator>d__52<TSource>(0);
                }
                d__.source = this.<>3__source;
                d__.predicate = this.<>3__predicate;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();
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
                            this.<>m__Finally56();
                        }
                        return;
                }
            }

            TSource IEnumerator<TSource>.Current
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
        private sealed class <SkipWhileIterator>d__59<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TSource <>2__current;
            public Func<TSource, int, bool> <>3__predicate;
            public IEnumerable<TSource> <>3__source;
            public IEnumerator<TSource> <>7__wrap5d;
            private int <>l__initialThreadId;
            public TSource <element>5__5c;
            public int <index>5__5a;
            public bool <yielding>5__5b;
            public Func<TSource, int, bool> predicate;
            public IEnumerable<TSource> source;

            [DebuggerHidden]
            public <SkipWhileIterator>d__59(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally5e()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap5d != null)
                {
                    this.<>7__wrap5d.Dispose();
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
                            this.<index>5__5a = -1;
                            this.<yielding>5__5b = false;
                            this.<>7__wrap5d = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_00BA;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_00BA;

                        default:
                            goto Label_00CD;
                    }
                Label_004D:
                    this.<element>5__5c = this.<>7__wrap5d.Current;
                    this.<index>5__5a++;
                    if (!this.<yielding>5__5b && !this.predicate(this.<element>5__5c, this.<index>5__5a))
                    {
                        this.<yielding>5__5b = true;
                    }
                    if (this.<yielding>5__5b)
                    {
                        this.<>2__current = this.<element>5__5c;
                        this.<>1__state = 2;
                        return true;
                    }
                Label_00BA:
                    if (this.<>7__wrap5d.MoveNext())
                    {
                        goto Label_004D;
                    }
                    this.<>m__Finally5e();
                Label_00CD:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                Enumerable.<SkipWhileIterator>d__59<TSource> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Enumerable.<SkipWhileIterator>d__59<TSource>) this;
                }
                else
                {
                    d__ = new Enumerable.<SkipWhileIterator>d__59<TSource>(0);
                }
                d__.source = this.<>3__source;
                d__.predicate = this.<>3__predicate;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();
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
                            this.<>m__Finally5e();
                        }
                        return;
                }
            }

            TSource IEnumerator<TSource>.Current
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
        private sealed class <TakeIterator>d__3a<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TSource <>2__current;
            public int <>3__count;
            public IEnumerable<TSource> <>3__source;
            public IEnumerator<TSource> <>7__wrap3c;
            private int <>l__initialThreadId;
            public TSource <element>5__3b;
            public int count;
            public IEnumerable<TSource> source;

            [DebuggerHidden]
            public <TakeIterator>d__3a(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally3d()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap3c != null)
                {
                    this.<>7__wrap3c.Dispose();
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
                            if (this.count <= 0)
                            {
                                goto Label_009A;
                            }
                            this.<>7__wrap3c = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_0087;

                        case 2:
                            this.<>1__state = 1;
                            if (--this.count == 0)
                            {
                                goto Label_0094;
                            }
                            goto Label_0087;

                        default:
                            goto Label_009A;
                    }
                Label_0045:
                    this.<element>5__3b = this.<>7__wrap3c.Current;
                    this.<>2__current = this.<element>5__3b;
                    this.<>1__state = 2;
                    return true;
                Label_0087:
                    if (this.<>7__wrap3c.MoveNext())
                    {
                        goto Label_0045;
                    }
                Label_0094:
                    this.<>m__Finally3d();
                Label_009A:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                Enumerable.<TakeIterator>d__3a<TSource> d__a;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__a = (Enumerable.<TakeIterator>d__3a<TSource>) this;
                }
                else
                {
                    d__a = new Enumerable.<TakeIterator>d__3a<TSource>(0);
                }
                d__a.source = this.<>3__source;
                d__a.count = this.<>3__count;
                return d__a;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();
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
                            this.<>m__Finally3d();
                        }
                        return;
                }
            }

            TSource IEnumerator<TSource>.Current
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
        private sealed class <TakeWhileIterator>d__40<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TSource <>2__current;
            public Func<TSource, bool> <>3__predicate;
            public IEnumerable<TSource> <>3__source;
            public IEnumerator<TSource> <>7__wrap42;
            private int <>l__initialThreadId;
            public TSource <element>5__41;
            public Func<TSource, bool> predicate;
            public IEnumerable<TSource> source;

            [DebuggerHidden]
            public <TakeWhileIterator>d__40(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally43()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap42 != null)
                {
                    this.<>7__wrap42.Dispose();
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
                            this.<>7__wrap42 = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_007E;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_007E;

                        default:
                            goto Label_0091;
                    }
                Label_003C:
                    this.<element>5__41 = this.<>7__wrap42.Current;
                    if (!this.predicate(this.<element>5__41))
                    {
                        goto Label_008B;
                    }
                    this.<>2__current = this.<element>5__41;
                    this.<>1__state = 2;
                    return true;
                Label_007E:
                    if (this.<>7__wrap42.MoveNext())
                    {
                        goto Label_003C;
                    }
                Label_008B:
                    this.<>m__Finally43();
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
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                Enumerable.<TakeWhileIterator>d__40<TSource> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Enumerable.<TakeWhileIterator>d__40<TSource>) this;
                }
                else
                {
                    d__ = new Enumerable.<TakeWhileIterator>d__40<TSource>(0);
                }
                d__.source = this.<>3__source;
                d__.predicate = this.<>3__predicate;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();
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
                            this.<>m__Finally43();
                        }
                        return;
                }
            }

            TSource IEnumerator<TSource>.Current
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
        private sealed class <TakeWhileIterator>d__46<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TSource <>2__current;
            public Func<TSource, int, bool> <>3__predicate;
            public IEnumerable<TSource> <>3__source;
            public IEnumerator<TSource> <>7__wrap49;
            private int <>l__initialThreadId;
            public TSource <element>5__48;
            public int <index>5__47;
            public Func<TSource, int, bool> predicate;
            public IEnumerable<TSource> source;

            [DebuggerHidden]
            public <TakeWhileIterator>d__46(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally4a()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap49 != null)
                {
                    this.<>7__wrap49.Dispose();
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
                            this.<index>5__47 = -1;
                            this.<>7__wrap49 = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_009C;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_009C;

                        default:
                            goto Label_00AF;
                    }
                Label_0046:
                    this.<element>5__48 = this.<>7__wrap49.Current;
                    this.<index>5__47++;
                    if (!this.predicate(this.<element>5__48, this.<index>5__47))
                    {
                        goto Label_00A9;
                    }
                    this.<>2__current = this.<element>5__48;
                    this.<>1__state = 2;
                    return true;
                Label_009C:
                    if (this.<>7__wrap49.MoveNext())
                    {
                        goto Label_0046;
                    }
                Label_00A9:
                    this.<>m__Finally4a();
                Label_00AF:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                Enumerable.<TakeWhileIterator>d__46<TSource> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Enumerable.<TakeWhileIterator>d__46<TSource>) this;
                }
                else
                {
                    d__ = new Enumerable.<TakeWhileIterator>d__46<TSource>(0);
                }
                d__.source = this.<>3__source;
                d__.predicate = this.<>3__predicate;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();
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
                            this.<>m__Finally4a();
                        }
                        return;
                }
            }

            TSource IEnumerator<TSource>.Current
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
        private sealed class <UnionIterator>d__88<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TSource <>2__current;
            public IEqualityComparer<TSource> <>3__comparer;
            public IEnumerable<TSource> <>3__first;
            public IEnumerable<TSource> <>3__second;
            public IEnumerator<TSource> <>7__wrap8c;
            public IEnumerator<TSource> <>7__wrap8e;
            private int <>l__initialThreadId;
            public TSource <element>5__8a;
            public TSource <element>5__8b;
            public Set<TSource> <set>5__89;
            public IEqualityComparer<TSource> comparer;
            public IEnumerable<TSource> first;
            public IEnumerable<TSource> second;

            [DebuggerHidden]
            public <UnionIterator>d__88(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally8d()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap8c != null)
                {
                    this.<>7__wrap8c.Dispose();
                }
            }

            private void <>m__Finally8f()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap8e != null)
                {
                    this.<>7__wrap8e.Dispose();
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
                            this.<set>5__89 = new Set<TSource>(this.comparer);
                            this.<>7__wrap8c = this.first.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_009D;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_009D;

                        case 4:
                            goto Label_0105;

                        default:
                            goto Label_011F;
                    }
                Label_0058:
                    this.<element>5__8a = this.<>7__wrap8c.Current;
                    if (this.<set>5__89.Add(this.<element>5__8a))
                    {
                        this.<>2__current = this.<element>5__8a;
                        this.<>1__state = 2;
                        return true;
                    }
                Label_009D:
                    if (this.<>7__wrap8c.MoveNext())
                    {
                        goto Label_0058;
                    }
                    this.<>m__Finally8d();
                    this.<>7__wrap8e = this.second.GetEnumerator();
                    this.<>1__state = 3;
                    while (this.<>7__wrap8e.MoveNext())
                    {
                        this.<element>5__8b = this.<>7__wrap8e.Current;
                        if (!this.<set>5__89.Add(this.<element>5__8b))
                        {
                            continue;
                        }
                        this.<>2__current = this.<element>5__8b;
                        this.<>1__state = 4;
                        return true;
                    Label_0105:
                        this.<>1__state = 3;
                    }
                    this.<>m__Finally8f();
                Label_011F:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                Enumerable.<UnionIterator>d__88<TSource> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Enumerable.<UnionIterator>d__88<TSource>) this;
                }
                else
                {
                    d__ = new Enumerable.<UnionIterator>d__88<TSource>(0);
                }
                d__.first = this.<>3__first;
                d__.second = this.<>3__second;
                d__.comparer = this.<>3__comparer;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();
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
                            this.<>m__Finally8d();
                        }
                        break;

                    case 3:
                    case 4:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally8f();
                        }
                        return;
                }
            }

            TSource IEnumerator<TSource>.Current
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
        private sealed class <WhereIterator>d__0<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TSource <>2__current;
            public Func<TSource, int, bool> <>3__predicate;
            public IEnumerable<TSource> <>3__source;
            public IEnumerator<TSource> <>7__wrap3;
            private int <>l__initialThreadId;
            public TSource <element>5__2;
            public int <index>5__1;
            public Func<TSource, int, bool> predicate;
            public IEnumerable<TSource> source;

            [DebuggerHidden]
            public <WhereIterator>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally4()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap3 != null)
                {
                    this.<>7__wrap3.Dispose();
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
                            this.<index>5__1 = -1;
                            this.<>7__wrap3 = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_009C;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_009C;

                        default:
                            goto Label_00AF;
                    }
                Label_0046:
                    this.<element>5__2 = this.<>7__wrap3.Current;
                    this.<index>5__1++;
                    if (this.predicate(this.<element>5__2, this.<index>5__1))
                    {
                        this.<>2__current = this.<element>5__2;
                        this.<>1__state = 2;
                        return true;
                    }
                Label_009C:
                    if (this.<>7__wrap3.MoveNext())
                    {
                        goto Label_0046;
                    }
                    this.<>m__Finally4();
                Label_00AF:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                Enumerable.<WhereIterator>d__0<TSource> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Enumerable.<WhereIterator>d__0<TSource>) this;
                }
                else
                {
                    d__ = new Enumerable.<WhereIterator>d__0<TSource>(0);
                }
                d__.source = this.<>3__source;
                d__.predicate = this.<>3__predicate;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();
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
                            this.<>m__Finally4();
                        }
                        return;
                }
            }

            TSource IEnumerator<TSource>.Current
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
        private sealed class <ZipIterator>d__7a<TFirst, TSecond, TResult> : IEnumerable<TResult>, IEnumerable, IEnumerator<TResult>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TResult <>2__current;
            public IEnumerable<TFirst> <>3__first;
            public Func<TFirst, TSecond, TResult> <>3__resultSelector;
            public IEnumerable<TSecond> <>3__second;
            private int <>l__initialThreadId;
            public IEnumerator<TFirst> <e1>5__7b;
            public IEnumerator<TSecond> <e2>5__7c;
            public IEnumerable<TFirst> first;
            public Func<TFirst, TSecond, TResult> resultSelector;
            public IEnumerable<TSecond> second;

            [DebuggerHidden]
            public <ZipIterator>d__7a(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally7d()
            {
                this.<>1__state = -1;
                if (this.<e1>5__7b != null)
                {
                    this.<e1>5__7b.Dispose();
                }
            }

            private void <>m__Finally7e()
            {
                this.<>1__state = 1;
                if (this.<e2>5__7c != null)
                {
                    this.<e2>5__7c.Dispose();
                }
            }

            private bool MoveNext()
            {
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<e1>5__7b = this.first.GetEnumerator();
                            this.<>1__state = 1;
                            this.<e2>5__7c = this.second.GetEnumerator();
                            this.<>1__state = 2;
                            while (this.<e1>5__7b.MoveNext() && this.<e2>5__7c.MoveNext())
                            {
                                this.<>2__current = this.resultSelector(this.<e1>5__7b.Current, this.<e2>5__7c.Current);
                                this.<>1__state = 3;
                                return true;
                            Label_007F:
                                this.<>1__state = 2;
                            }
                            this.<>m__Finally7e();
                            this.<>m__Finally7d();
                            break;

                        case 3:
                            goto Label_007F;
                    }
                    return false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
            {
                Enumerable.<ZipIterator>d__7a<TFirst, TSecond, TResult> d__a;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__a = (Enumerable.<ZipIterator>d__7a<TFirst, TSecond, TResult>) this;
                }
                else
                {
                    d__a = new Enumerable.<ZipIterator>d__7a<TFirst, TSecond, TResult>(0);
                }
                d__a.first = this.<>3__first;
                d__a.second = this.<>3__second;
                d__a.resultSelector = this.<>3__resultSelector;
                return d__a;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TResult>.GetEnumerator();
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
                    case 3:
                        try
                        {
                            switch (this.<>1__state)
                            {
                                case 2:
                                case 3:
                                    try
                                    {
                                    }
                                    finally
                                    {
                                        this.<>m__Finally7e();
                                    }
                                    return;
                            }
                        }
                        finally
                        {
                            this.<>m__Finally7d();
                        }
                        break;

                    default:
                        return;
                }
            }

            TResult IEnumerator<TResult>.Current
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

        private abstract class Iterator<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IDisposable, IEnumerator
        {
            internal TSource current;
            internal int state;
            private int threadId;

            public Iterator()
            {
                this.threadId = Thread.CurrentThread.ManagedThreadId;
            }

            public abstract Enumerable.Iterator<TSource> Clone();
            public virtual void Dispose()
            {
                this.current = default(TSource);
                this.state = -1;
            }

            public IEnumerator<TSource> GetEnumerator()
            {
                if ((this.threadId == Thread.CurrentThread.ManagedThreadId) && (this.state == 0))
                {
                    this.state = 1;
                    return this;
                }
                Enumerable.Iterator<TSource> iterator = this.Clone();
                iterator.state = 1;
                return iterator;
            }

            public abstract bool MoveNext();
            public abstract IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector);
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            void IEnumerator.Reset()
            {
                throw new NotImplementedException();
            }

            public abstract IEnumerable<TSource> Where(Func<TSource, bool> predicate);

            public TSource Current
            {
                get
                {
                    return this.current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }

        private class WhereArrayIterator<TSource> : Enumerable.Iterator<TSource>
        {
            private int index;
            private Func<TSource, bool> predicate;
            private TSource[] source;

            public WhereArrayIterator(TSource[] source, Func<TSource, bool> predicate)
            {
                this.source = source;
                this.predicate = predicate;
            }

            public override Enumerable.Iterator<TSource> Clone()
            {
                return new Enumerable.WhereArrayIterator<TSource>(this.source, this.predicate);
            }

            public override bool MoveNext()
            {
                if (base.state == 1)
                {
                    while (this.index < this.source.Length)
                    {
                        TSource arg = this.source[this.index];
                        this.index++;
                        if (this.predicate(arg))
                        {
                            base.current = arg;
                            return true;
                        }
                    }
                    this.Dispose();
                }
                return false;
            }

            public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
            {
                return new Enumerable.WhereSelectArrayIterator<TSource, TResult>(this.source, this.predicate, selector);
            }

            public override IEnumerable<TSource> Where(Func<TSource, bool> predicate)
            {
                return new Enumerable.WhereArrayIterator<TSource>(this.source, Enumerable.CombinePredicates<TSource>(this.predicate, predicate));
            }
        }

        private class WhereEnumerableIterator<TSource> : Enumerable.Iterator<TSource>
        {
            private IEnumerator<TSource> enumerator;
            private Func<TSource, bool> predicate;
            private IEnumerable<TSource> source;

            public WhereEnumerableIterator(IEnumerable<TSource> source, Func<TSource, bool> predicate)
            {
                this.source = source;
                this.predicate = predicate;
            }

            public override Enumerable.Iterator<TSource> Clone()
            {
                return new Enumerable.WhereEnumerableIterator<TSource>(this.source, this.predicate);
            }

            public override void Dispose()
            {
                if (this.enumerator != null)
                {
                    this.enumerator.Dispose();
                }
                this.enumerator = null;
                base.Dispose();
            }

            public override bool MoveNext()
            {
                switch (base.state)
                {
                    case 1:
                        this.enumerator = this.source.GetEnumerator();
                        base.state = 2;
                        break;

                    case 2:
                        break;

                    default:
                        goto Label_0069;
                }
                while (this.enumerator.MoveNext())
                {
                    TSource current = this.enumerator.Current;
                    if (this.predicate(current))
                    {
                        base.current = current;
                        return true;
                    }
                }
                this.Dispose();
            Label_0069:
                return false;
            }

            public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
            {
                return new Enumerable.WhereSelectEnumerableIterator<TSource, TResult>(this.source, this.predicate, selector);
            }

            public override IEnumerable<TSource> Where(Func<TSource, bool> predicate)
            {
                return new Enumerable.WhereEnumerableIterator<TSource>(this.source, Enumerable.CombinePredicates<TSource>(this.predicate, predicate));
            }
        }

        private class WhereListIterator<TSource> : Enumerable.Iterator<TSource>
        {
            private List<TSource>.Enumerator enumerator;
            private Func<TSource, bool> predicate;
            private List<TSource> source;

            public WhereListIterator(List<TSource> source, Func<TSource, bool> predicate)
            {
                this.source = source;
                this.predicate = predicate;
            }

            public override Enumerable.Iterator<TSource> Clone()
            {
                return new Enumerable.WhereListIterator<TSource>(this.source, this.predicate);
            }

            public override bool MoveNext()
            {
                switch (base.state)
                {
                    case 1:
                        this.enumerator = this.source.GetEnumerator();
                        base.state = 2;
                        break;

                    case 2:
                        break;

                    default:
                        goto Label_0069;
                }
                while (this.enumerator.MoveNext())
                {
                    TSource current = this.enumerator.Current;
                    if (this.predicate(current))
                    {
                        base.current = current;
                        return true;
                    }
                }
                this.Dispose();
            Label_0069:
                return false;
            }

            public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
            {
                return new Enumerable.WhereSelectListIterator<TSource, TResult>(this.source, this.predicate, selector);
            }

            public override IEnumerable<TSource> Where(Func<TSource, bool> predicate)
            {
                return new Enumerable.WhereListIterator<TSource>(this.source, Enumerable.CombinePredicates<TSource>(this.predicate, predicate));
            }
        }

        private class WhereSelectArrayIterator<TSource, TResult> : Enumerable.Iterator<TResult>
        {
            private int index;
            private Func<TSource, bool> predicate;
            private Func<TSource, TResult> selector;
            private TSource[] source;

            public WhereSelectArrayIterator(TSource[] source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
            {
                this.source = source;
                this.predicate = predicate;
                this.selector = selector;
            }

            public override Enumerable.Iterator<TResult> Clone()
            {
                return new Enumerable.WhereSelectArrayIterator<TSource, TResult>(this.source, this.predicate, this.selector);
            }

            public override bool MoveNext()
            {
                if (base.state == 1)
                {
                    while (this.index < this.source.Length)
                    {
                        TSource arg = this.source[this.index];
                        this.index++;
                        if ((this.predicate == null) || this.predicate(arg))
                        {
                            base.current = this.selector(arg);
                            return true;
                        }
                    }
                    this.Dispose();
                }
                return false;
            }

            public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
            {
                return new Enumerable.WhereSelectArrayIterator<TSource, TResult2>(this.source, this.predicate, Enumerable.CombineSelectors<TSource, TResult, TResult2>(this.selector, selector));
            }

            public override IEnumerable<TResult> Where(Func<TResult, bool> predicate)
            {
                return (IEnumerable<TResult>) new Enumerable.WhereEnumerableIterator<TResult>(this, predicate);
            }
        }

        private class WhereSelectEnumerableIterator<TSource, TResult> : Enumerable.Iterator<TResult>
        {
            private IEnumerator<TSource> enumerator;
            private Func<TSource, bool> predicate;
            private Func<TSource, TResult> selector;
            private IEnumerable<TSource> source;

            public WhereSelectEnumerableIterator(IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
            {
                this.source = source;
                this.predicate = predicate;
                this.selector = selector;
            }

            public override Enumerable.Iterator<TResult> Clone()
            {
                return new Enumerable.WhereSelectEnumerableIterator<TSource, TResult>(this.source, this.predicate, this.selector);
            }

            public override void Dispose()
            {
                if (this.enumerator != null)
                {
                    this.enumerator.Dispose();
                }
                this.enumerator = null;
                base.Dispose();
            }

            public override bool MoveNext()
            {
                switch (base.state)
                {
                    case 1:
                        this.enumerator = this.source.GetEnumerator();
                        base.state = 2;
                        break;

                    case 2:
                        break;

                    default:
                        goto Label_007C;
                }
                while (this.enumerator.MoveNext())
                {
                    TSource current = this.enumerator.Current;
                    if ((this.predicate == null) || this.predicate(current))
                    {
                        base.current = this.selector(current);
                        return true;
                    }
                }
                this.Dispose();
            Label_007C:
                return false;
            }

            public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
            {
                return new Enumerable.WhereSelectEnumerableIterator<TSource, TResult2>(this.source, this.predicate, Enumerable.CombineSelectors<TSource, TResult, TResult2>(this.selector, selector));
            }

            public override IEnumerable<TResult> Where(Func<TResult, bool> predicate)
            {
                return (IEnumerable<TResult>) new Enumerable.WhereEnumerableIterator<TResult>(this, predicate);
            }
        }

        private class WhereSelectListIterator<TSource, TResult> : Enumerable.Iterator<TResult>
        {
            private List<TSource>.Enumerator enumerator;
            private Func<TSource, bool> predicate;
            private Func<TSource, TResult> selector;
            private List<TSource> source;

            public WhereSelectListIterator(List<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
            {
                this.source = source;
                this.predicate = predicate;
                this.selector = selector;
            }

            public override Enumerable.Iterator<TResult> Clone()
            {
                return new Enumerable.WhereSelectListIterator<TSource, TResult>(this.source, this.predicate, this.selector);
            }

            public override bool MoveNext()
            {
                switch (base.state)
                {
                    case 1:
                        this.enumerator = this.source.GetEnumerator();
                        base.state = 2;
                        break;

                    case 2:
                        break;

                    default:
                        goto Label_007C;
                }
                while (this.enumerator.MoveNext())
                {
                    TSource current = this.enumerator.Current;
                    if ((this.predicate == null) || this.predicate(current))
                    {
                        base.current = this.selector(current);
                        return true;
                    }
                }
                this.Dispose();
            Label_007C:
                return false;
            }

            public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
            {
                return new Enumerable.WhereSelectListIterator<TSource, TResult2>(this.source, this.predicate, Enumerable.CombineSelectors<TSource, TResult, TResult2>(this.selector, selector));
            }

            public override IEnumerable<TResult> Where(Func<TResult, bool> predicate)
            {
                return (IEnumerable<TResult>) new Enumerable.WhereEnumerableIterator<TResult>(this, predicate);
            }
        }
    }
}

