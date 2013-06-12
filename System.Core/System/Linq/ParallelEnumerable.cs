namespace System.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq.Parallel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ParallelEnumerable
    {
        private const string RIGHT_SOURCE_NOT_PARALLEL_STR = "The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() extension method to convert the right data source to System.Linq.ParallelQuery<T>.";

        public static TSource Aggregate<TSource>(this ParallelQuery<TSource> source, Func<TSource, TSource, TSource> func)
        {
            return source.Aggregate<TSource>(func, QueryAggregationOptions.AssociativeCommutative);
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(this ParallelQuery<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            return source.Aggregate<TSource, TAccumulate>(seed, func, QueryAggregationOptions.AssociativeCommutative);
        }

        internal static TSource Aggregate<TSource>(this ParallelQuery<TSource> source, Func<TSource, TSource, TSource> func, QueryAggregationOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }
            if ((~QueryAggregationOptions.AssociativeCommutative & options) != QueryAggregationOptions.None)
            {
                throw new ArgumentOutOfRangeException("options");
            }
            if ((options & QueryAggregationOptions.Associative) != QueryAggregationOptions.Associative)
            {
                return source.PerformSequentialAggregation<TSource, TSource>(default(TSource), false, func);
            }
            return source.PerformAggregation<TSource>(func, default(TSource), false, true, options);
        }

        internal static TAccumulate Aggregate<TSource, TAccumulate>(this ParallelQuery<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, QueryAggregationOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }
            if ((~QueryAggregationOptions.AssociativeCommutative & options) != QueryAggregationOptions.None)
            {
                throw new ArgumentOutOfRangeException("options");
            }
            return source.PerformSequentialAggregation<TSource, TAccumulate>(seed, true, func);
        }

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this ParallelQuery<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
        {
            TResult local2;
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }
            TAccumulate arg = source.PerformSequentialAggregation<TSource, TAccumulate>(seed, true, func);
            try
            {
                local2 = resultSelector(arg);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new AggregateException(new Exception[] { exception });
            }
            return local2;
        }

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this ParallelQuery<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> updateAccumulatorFunc, Func<TAccumulate, TAccumulate, TAccumulate> combineAccumulatorsFunc, Func<TAccumulate, TResult> resultSelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (updateAccumulatorFunc == null)
            {
                throw new ArgumentNullException("updateAccumulatorFunc");
            }
            if (combineAccumulatorsFunc == null)
            {
                throw new ArgumentNullException("combineAccumulatorsFunc");
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }
            return new AssociativeAggregationOperator<TSource, TAccumulate, TResult>(source, seed, null, true, updateAccumulatorFunc, combineAccumulatorsFunc, resultSelector, false, QueryAggregationOptions.AssociativeCommutative).Aggregate();
        }

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this ParallelQuery<TSource> source, Func<TAccumulate> seedFactory, Func<TAccumulate, TSource, TAccumulate> updateAccumulatorFunc, Func<TAccumulate, TAccumulate, TAccumulate> combineAccumulatorsFunc, Func<TAccumulate, TResult> resultSelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (seedFactory == null)
            {
                throw new ArgumentNullException("seedFactory");
            }
            if (updateAccumulatorFunc == null)
            {
                throw new ArgumentNullException("updateAccumulatorFunc");
            }
            if (combineAccumulatorsFunc == null)
            {
                throw new ArgumentNullException("combineAccumulatorsFunc");
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }
            return new AssociativeAggregationOperator<TSource, TAccumulate, TResult>(source, default(TAccumulate), seedFactory, true, updateAccumulatorFunc, combineAccumulatorsFunc, resultSelector, false, QueryAggregationOptions.AssociativeCommutative).Aggregate();
        }

        public static bool All<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return new AnyAllSearchOperator<TSource>(source, false, predicate).Aggregate();
        }

        public static bool Any<TSource>(this ParallelQuery<TSource> source)
        {
            IEnumerator<TSource> enumerator;
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            QueryOperator<TSource> @operator = source as QueryOperator<TSource>;
            if (@operator != null)
            {
                enumerator = @operator.GetEnumerator(3);
            }
            else
            {
                enumerator = source.GetEnumerator();
            }
            using (enumerator)
            {
                if (enumerator.MoveNext())
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Any<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return new AnyAllSearchOperator<TSource>(source, true, predicate).Aggregate();
        }

        public static IEnumerable<TSource> AsEnumerable<TSource>(this ParallelQuery<TSource> source)
        {
            return source.AsSequential<TSource>();
        }

        public static ParallelQuery AsOrdered(this ParallelQuery source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ParallelEnumerableWrapper wrapper = source as ParallelEnumerableWrapper;
            if (wrapper == null)
            {
                throw new InvalidOperationException(System.Linq.SR.GetString("ParallelQuery_InvalidNonGenericAsOrderedCall"));
            }
            return new OrderingQueryOperator<object>(QueryOperator<object>.AsQueryOperator(wrapper), true);
        }

        public static ParallelQuery<TSource> AsOrdered<TSource>(this ParallelQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!(source is ParallelEnumerableWrapper<TSource>) && !(source is IParallelPartitionable<TSource>))
            {
                PartitionerQueryOperator<TSource> @operator = source as PartitionerQueryOperator<TSource>;
                if (@operator == null)
                {
                    throw new InvalidOperationException(System.Linq.SR.GetString("ParallelQuery_InvalidAsOrderedCall"));
                }
                if (!@operator.Orderable)
                {
                    throw new InvalidOperationException(System.Linq.SR.GetString("ParallelQuery_PartitionerNotOrderable"));
                }
            }
            return new OrderingQueryOperator<TSource>(QueryOperator<TSource>.AsQueryOperator(source), true);
        }

        public static ParallelQuery<TSource> AsParallel<TSource>(this Partitioner<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new PartitionerQueryOperator<TSource>(source);
        }

        public static ParallelQuery<TSource> AsParallel<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new ParallelEnumerableWrapper<TSource>(source);
        }

        public static ParallelQuery AsParallel(this IEnumerable source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new ParallelEnumerableWrapper(source);
        }

        public static IEnumerable<TSource> AsSequential<TSource>(this ParallelQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ParallelEnumerableWrapper<TSource> wrapper = source as ParallelEnumerableWrapper<TSource>;
            if (wrapper != null)
            {
                return wrapper.WrappedEnumerable;
            }
            return source;
        }

        public static ParallelQuery<TSource> AsUnordered<TSource>(this ParallelQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new OrderingQueryOperator<TSource>(QueryOperator<TSource>.AsQueryOperator(source), false);
        }

        public static decimal Average(this ParallelQuery<decimal> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new DecimalAverageAggregationOperator(source).Aggregate();
        }

        public static double Average(this ParallelQuery<double> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new DoubleAverageAggregationOperator(source).Aggregate();
        }

        public static double Average(this ParallelQuery<int> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new IntAverageAggregationOperator(source).Aggregate();
        }

        public static double Average(this ParallelQuery<long> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new LongAverageAggregationOperator(source).Aggregate();
        }

        public static float Average(this ParallelQuery<float> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new FloatAverageAggregationOperator(source).Aggregate();
        }

        public static decimal? Average(this ParallelQuery<decimal?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableDecimalAverageAggregationOperator(source).Aggregate();
        }

        public static double? Average(this ParallelQuery<double?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableDoubleAverageAggregationOperator(source).Aggregate();
        }

        public static double? Average(this ParallelQuery<int?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableIntAverageAggregationOperator(source).Aggregate();
        }

        public static double? Average(this ParallelQuery<long?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableLongAverageAggregationOperator(source).Aggregate();
        }

        public static float? Average(this ParallelQuery<float?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableFloatAverageAggregationOperator(source).Aggregate();
        }

        public static decimal Average<TSource>(this ParallelQuery<TSource> source, Func<TSource, decimal> selector)
        {
            return source.Select<TSource, decimal>(selector).Average();
        }

        public static double Average<TSource>(this ParallelQuery<TSource> source, Func<TSource, double> selector)
        {
            return source.Select<TSource, double>(selector).Average();
        }

        public static double Average<TSource>(this ParallelQuery<TSource> source, Func<TSource, int> selector)
        {
            return source.Select<TSource, int>(selector).Average();
        }

        public static double Average<TSource>(this ParallelQuery<TSource> source, Func<TSource, long> selector)
        {
            return source.Select<TSource, long>(selector).Average();
        }

        public static decimal? Average<TSource>(this ParallelQuery<TSource> source, Func<TSource, decimal?> selector)
        {
            return source.Select<TSource, decimal?>(selector).Average();
        }

        public static double? Average<TSource>(this ParallelQuery<TSource> source, Func<TSource, double?> selector)
        {
            return source.Select<TSource, double?>(selector).Average();
        }

        public static double? Average<TSource>(this ParallelQuery<TSource> source, Func<TSource, int?> selector)
        {
            return source.Select<TSource, int?>(selector).Average();
        }

        public static float Average<TSource>(this ParallelQuery<TSource> source, Func<TSource, float> selector)
        {
            return source.Select<TSource, float>(selector).Average();
        }

        public static double? Average<TSource>(this ParallelQuery<TSource> source, Func<TSource, long?> selector)
        {
            return source.Select<TSource, long?>(selector).Average();
        }

        public static float? Average<TSource>(this ParallelQuery<TSource> source, Func<TSource, float?> selector)
        {
            return source.Select<TSource, float?>(selector).Average();
        }

        public static ParallelQuery<TResult> Cast<TResult>(this ParallelQuery source)
        {
            return source.Cast<TResult>();
        }

        [Obsolete("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
        public static ParallelQuery<TSource> Concat<TSource>(this ParallelQuery<TSource> first, IEnumerable<TSource> second)
        {
            throw new NotSupportedException(System.Linq.SR.GetString("ParallelEnumerable_BinaryOpMustUseAsParallel"));
        }

        public static ParallelQuery<TSource> Concat<TSource>(this ParallelQuery<TSource> first, ParallelQuery<TSource> second)
        {
            if (first == null)
            {
                throw new ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new ArgumentNullException("second");
            }
            return new ConcatQueryOperator<TSource>(first, second);
        }

        public static bool Contains<TSource>(this ParallelQuery<TSource> source, TSource value)
        {
            return source.Contains<TSource>(value, null);
        }

        public static bool Contains<TSource>(this ParallelQuery<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new ContainsSearchOperator<TSource>(source, value, comparer).Aggregate();
        }

        public static int Count<TSource>(this ParallelQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ParallelEnumerableWrapper<TSource> wrapper = source as ParallelEnumerableWrapper<TSource>;
            if (wrapper != null)
            {
                ICollection<TSource> wrappedEnumerable = wrapper.WrappedEnumerable as ICollection<TSource>;
                if (wrappedEnumerable != null)
                {
                    return wrappedEnumerable.Count;
                }
            }
            return new CountAggregationOperator<TSource>(source).Aggregate();
        }

        public static int Count<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return new CountAggregationOperator<TSource>(source.Where<TSource>(predicate)).Aggregate();
        }

        public static ParallelQuery<TSource> DefaultIfEmpty<TSource>(this ParallelQuery<TSource> source)
        {
            return source.DefaultIfEmpty<TSource>(default(TSource));
        }

        public static ParallelQuery<TSource> DefaultIfEmpty<TSource>(this ParallelQuery<TSource> source, TSource defaultValue)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new DefaultIfEmptyQueryOperator<TSource>(source, defaultValue);
        }

        private static void DisposeEnumerator<TSource>(IEnumerator<TSource> e, CancellationState cancelState)
        {
            try
            {
                e.Dispose();
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception exception)
            {
                ExceptionAggregator.ThrowOCEorAggregateException(exception, cancelState);
            }
        }

        public static ParallelQuery<TSource> Distinct<TSource>(this ParallelQuery<TSource> source)
        {
            return source.Distinct<TSource>(null);
        }

        public static ParallelQuery<TSource> Distinct<TSource>(this ParallelQuery<TSource> source, IEqualityComparer<TSource> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new DistinctQueryOperator<TSource>(source, comparer);
        }

        public static TSource ElementAt<TSource>(this ParallelQuery<TSource> source, int index)
        {
            TSource local;
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            ElementAtQueryOperator<TSource> @operator = new ElementAtQueryOperator<TSource>(source, index);
            if (!@operator.Aggregate(out local, false))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return local;
        }

        public static TSource ElementAtOrDefault<TSource>(this ParallelQuery<TSource> source, int index)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (index >= 0)
            {
                TSource local;
                ElementAtQueryOperator<TSource> @operator = new ElementAtQueryOperator<TSource>(source, index);
                if (@operator.Aggregate(out local, true))
                {
                    return local;
                }
            }
            return default(TSource);
        }

        public static ParallelQuery<TResult> Empty<TResult>()
        {
            return System.Linq.Parallel.EmptyEnumerable<TResult>.Instance;
        }

        [Obsolete("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
        public static ParallelQuery<TSource> Except<TSource>(this ParallelQuery<TSource> first, IEnumerable<TSource> second)
        {
            throw new NotSupportedException(System.Linq.SR.GetString("ParallelEnumerable_BinaryOpMustUseAsParallel"));
        }

        public static ParallelQuery<TSource> Except<TSource>(this ParallelQuery<TSource> first, ParallelQuery<TSource> second)
        {
            return first.Except<TSource>(second, null);
        }

        [Obsolete("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
        public static ParallelQuery<TSource> Except<TSource>(this ParallelQuery<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            throw new NotSupportedException(System.Linq.SR.GetString("ParallelEnumerable_BinaryOpMustUseAsParallel"));
        }

        public static ParallelQuery<TSource> Except<TSource>(this ParallelQuery<TSource> first, ParallelQuery<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null)
            {
                throw new ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new ArgumentNullException("second");
            }
            return new ExceptQueryOperator<TSource>(first, second, comparer);
        }

        public static TSource First<TSource>(this ParallelQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            FirstQueryOperator<TSource> queryOp = new FirstQueryOperator<TSource>(source, null);
            QuerySettings settings = queryOp.SpecifiedQuerySettings.WithDefaults();
            if (queryOp.LimitsParallelism && (((ParallelExecutionMode) settings.ExecutionMode) != ParallelExecutionMode.ForceParallelism))
            {
                IEnumerable<TSource> introduced6 = queryOp.Child.AsSequentialQuery(settings.CancellationState.ExternalCancellationToken);
                IEnumerable<TSource> introduced7 = CancellableEnumerable.Wrap<TSource>(introduced6, settings.CancellationState.ExternalCancellationToken);
                return ExceptionAggregator.WrapEnumerable<TSource>(introduced7, settings.CancellationState).First<TSource>();
            }
            return GetOneWithPossibleDefault<TSource>(queryOp, false, false);
        }

        public static TSource First<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            FirstQueryOperator<TSource> queryOp = new FirstQueryOperator<TSource>(source, predicate);
            QuerySettings settings = queryOp.SpecifiedQuerySettings.WithDefaults();
            if (queryOp.LimitsParallelism && (((ParallelExecutionMode) settings.ExecutionMode) != ParallelExecutionMode.ForceParallelism))
            {
                IEnumerable<TSource> introduced6 = queryOp.Child.AsSequentialQuery(settings.CancellationState.ExternalCancellationToken);
                IEnumerable<TSource> introduced7 = CancellableEnumerable.Wrap<TSource>(introduced6, settings.CancellationState.ExternalCancellationToken);
                return ExceptionAggregator.WrapEnumerable<TSource>(introduced7, settings.CancellationState).First<TSource>(ExceptionAggregator.WrapFunc<TSource, bool>(predicate, settings.CancellationState));
            }
            return GetOneWithPossibleDefault<TSource>(queryOp, false, false);
        }

        public static TSource FirstOrDefault<TSource>(this ParallelQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            FirstQueryOperator<TSource> queryOp = new FirstQueryOperator<TSource>(source, null);
            QuerySettings settings = queryOp.SpecifiedQuerySettings.WithDefaults();
            if (queryOp.LimitsParallelism && (((ParallelExecutionMode) settings.ExecutionMode) != ParallelExecutionMode.ForceParallelism))
            {
                IEnumerable<TSource> introduced6 = queryOp.Child.AsSequentialQuery(settings.CancellationState.ExternalCancellationToken);
                IEnumerable<TSource> introduced7 = CancellableEnumerable.Wrap<TSource>(introduced6, settings.CancellationState.ExternalCancellationToken);
                return ExceptionAggregator.WrapEnumerable<TSource>(introduced7, settings.CancellationState).FirstOrDefault<TSource>();
            }
            return GetOneWithPossibleDefault<TSource>(queryOp, false, true);
        }

        public static TSource FirstOrDefault<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            FirstQueryOperator<TSource> queryOp = new FirstQueryOperator<TSource>(source, predicate);
            QuerySettings settings = queryOp.SpecifiedQuerySettings.WithDefaults();
            if (queryOp.LimitsParallelism && (((ParallelExecutionMode) settings.ExecutionMode) != ParallelExecutionMode.ForceParallelism))
            {
                IEnumerable<TSource> introduced6 = queryOp.Child.AsSequentialQuery(settings.CancellationState.ExternalCancellationToken);
                IEnumerable<TSource> introduced7 = CancellableEnumerable.Wrap<TSource>(introduced6, settings.CancellationState.ExternalCancellationToken);
                return ExceptionAggregator.WrapEnumerable<TSource>(introduced7, settings.CancellationState).FirstOrDefault<TSource>(ExceptionAggregator.WrapFunc<TSource, bool>(predicate, settings.CancellationState));
            }
            return GetOneWithPossibleDefault<TSource>(queryOp, false, true);
        }

        public static void ForAll<TSource>(this ParallelQuery<TSource> source, Action<TSource> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            new ForAllOperator<TSource>(source, action).RunSynchronously();
        }

        private static TSource GetOneWithPossibleDefault<TSource>(QueryOperator<TSource> queryOp, bool throwIfTwo, bool defaultIfEmpty)
        {
            using (IEnumerator<TSource> enumerator = queryOp.GetEnumerator(3))
            {
                if (enumerator.MoveNext())
                {
                    TSource current = enumerator.Current;
                    if (throwIfTwo && enumerator.MoveNext())
                    {
                        throw new InvalidOperationException(System.Linq.SR.GetString("MoreThanOneMatch"));
                    }
                    return current;
                }
            }
            if (!defaultIfEmpty)
            {
                throw new InvalidOperationException(System.Linq.SR.GetString("NoElements"));
            }
            return default(TSource);
        }

        public static ParallelQuery<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.GroupBy<TSource, TKey>(keySelector, null);
        }

        public static ParallelQuery<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            return new GroupByQueryOperator<TSource, TKey, TSource>(source, keySelector, null, comparer);
        }

        public static ParallelQuery<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return source.GroupBy<TSource, TKey, TElement>(keySelector, elementSelector, null);
        }

        public static ParallelQuery<TResult> GroupBy<TSource, TKey, TResult>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
        {
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }
            return source.GroupBy<TSource, TKey>(keySelector).Select<IGrouping<TKey, TSource>, TResult>(((Func<IGrouping<TKey, TSource>, TResult>) (grouping => resultSelector(grouping.Key, grouping))));
        }

        public static ParallelQuery<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (elementSelector == null)
            {
                throw new ArgumentNullException("elementSelector");
            }
            return new GroupByQueryOperator<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
        }

        public static ParallelQuery<TResult> GroupBy<TSource, TKey, TElement, TResult>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
        {
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }
            return source.GroupBy<TSource, TKey, TElement>(keySelector, elementSelector).Select<IGrouping<TKey, TElement>, TResult>(((Func<IGrouping<TKey, TElement>, TResult>) (grouping => resultSelector(grouping.Key, grouping))));
        }

        public static ParallelQuery<TResult> GroupBy<TSource, TKey, TResult>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }
            return source.GroupBy<TSource, TKey>(keySelector, comparer).Select<IGrouping<TKey, TSource>, TResult>(((Func<IGrouping<TKey, TSource>, TResult>) (grouping => resultSelector(grouping.Key, grouping))));
        }

        public static ParallelQuery<TResult> GroupBy<TSource, TKey, TElement, TResult>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }
            return source.GroupBy<TSource, TKey, TElement>(keySelector, elementSelector, comparer).Select<IGrouping<TKey, TElement>, TResult>(((Func<IGrouping<TKey, TElement>, TResult>) (grouping => resultSelector(grouping.Key, grouping))));
        }

        [Obsolete("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
        public static ParallelQuery<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this ParallelQuery<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
        {
            throw new NotSupportedException(System.Linq.SR.GetString("ParallelEnumerable_BinaryOpMustUseAsParallel"));
        }

        public static ParallelQuery<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this ParallelQuery<TOuter> outer, ParallelQuery<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
        {
            return outer.GroupJoin<TOuter, TInner, TKey, TResult>(inner, outerKeySelector, innerKeySelector, resultSelector, null);
        }

        [Obsolete("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
        public static ParallelQuery<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this ParallelQuery<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            throw new NotSupportedException(System.Linq.SR.GetString("ParallelEnumerable_BinaryOpMustUseAsParallel"));
        }

        public static ParallelQuery<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this ParallelQuery<TOuter> outer, ParallelQuery<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (outer == null)
            {
                throw new ArgumentNullException("outer");
            }
            if (inner == null)
            {
                throw new ArgumentNullException("inner");
            }
            if (outerKeySelector == null)
            {
                throw new ArgumentNullException("outerKeySelector");
            }
            if (innerKeySelector == null)
            {
                throw new ArgumentNullException("innerKeySelector");
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }
            return (ParallelQuery<TResult>) new GroupJoinQueryOperator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        }

        [Obsolete("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
        public static ParallelQuery<TSource> Intersect<TSource>(this ParallelQuery<TSource> first, IEnumerable<TSource> second)
        {
            throw new NotSupportedException(System.Linq.SR.GetString("ParallelEnumerable_BinaryOpMustUseAsParallel"));
        }

        public static ParallelQuery<TSource> Intersect<TSource>(this ParallelQuery<TSource> first, ParallelQuery<TSource> second)
        {
            return first.Intersect<TSource>(second, null);
        }

        [Obsolete("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
        public static ParallelQuery<TSource> Intersect<TSource>(this ParallelQuery<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            throw new NotSupportedException(System.Linq.SR.GetString("ParallelEnumerable_BinaryOpMustUseAsParallel"));
        }

        public static ParallelQuery<TSource> Intersect<TSource>(this ParallelQuery<TSource> first, ParallelQuery<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null)
            {
                throw new ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new ArgumentNullException("second");
            }
            return new IntersectQueryOperator<TSource>(first, second, comparer);
        }

        [Obsolete("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
        public static ParallelQuery<TResult> Join<TOuter, TInner, TKey, TResult>(this ParallelQuery<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
        {
            throw new NotSupportedException(System.Linq.SR.GetString("ParallelEnumerable_BinaryOpMustUseAsParallel"));
        }

        public static ParallelQuery<TResult> Join<TOuter, TInner, TKey, TResult>(this ParallelQuery<TOuter> outer, ParallelQuery<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
        {
            return outer.Join<TOuter, TInner, TKey, TResult>(inner, outerKeySelector, innerKeySelector, resultSelector, null);
        }

        [Obsolete("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
        public static ParallelQuery<TResult> Join<TOuter, TInner, TKey, TResult>(this ParallelQuery<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            throw new NotSupportedException(System.Linq.SR.GetString("ParallelEnumerable_BinaryOpMustUseAsParallel"));
        }

        public static ParallelQuery<TResult> Join<TOuter, TInner, TKey, TResult>(this ParallelQuery<TOuter> outer, ParallelQuery<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            if (outer == null)
            {
                throw new ArgumentNullException("outer");
            }
            if (inner == null)
            {
                throw new ArgumentNullException("inner");
            }
            if (outerKeySelector == null)
            {
                throw new ArgumentNullException("outerKeySelector");
            }
            if (innerKeySelector == null)
            {
                throw new ArgumentNullException("innerKeySelector");
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }
            return (ParallelQuery<TResult>) new JoinQueryOperator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        }

        public static TSource Last<TSource>(this ParallelQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            LastQueryOperator<TSource> queryOp = new LastQueryOperator<TSource>(source, null);
            QuerySettings settings = queryOp.SpecifiedQuerySettings.WithDefaults();
            if (queryOp.LimitsParallelism && (((ParallelExecutionMode) settings.ExecutionMode) != ParallelExecutionMode.ForceParallelism))
            {
                IEnumerable<TSource> introduced6 = queryOp.Child.AsSequentialQuery(settings.CancellationState.ExternalCancellationToken);
                IEnumerable<TSource> introduced7 = CancellableEnumerable.Wrap<TSource>(introduced6, settings.CancellationState.ExternalCancellationToken);
                return ExceptionAggregator.WrapEnumerable<TSource>(introduced7, settings.CancellationState).Last<TSource>();
            }
            return GetOneWithPossibleDefault<TSource>(queryOp, false, false);
        }

        public static TSource Last<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            LastQueryOperator<TSource> queryOp = new LastQueryOperator<TSource>(source, predicate);
            QuerySettings settings = queryOp.SpecifiedQuerySettings.WithDefaults();
            if (queryOp.LimitsParallelism && (((ParallelExecutionMode) settings.ExecutionMode) != ParallelExecutionMode.ForceParallelism))
            {
                IEnumerable<TSource> introduced6 = queryOp.Child.AsSequentialQuery(settings.CancellationState.ExternalCancellationToken);
                IEnumerable<TSource> introduced7 = CancellableEnumerable.Wrap<TSource>(introduced6, settings.CancellationState.ExternalCancellationToken);
                return ExceptionAggregator.WrapEnumerable<TSource>(introduced7, settings.CancellationState).Last<TSource>(ExceptionAggregator.WrapFunc<TSource, bool>(predicate, settings.CancellationState));
            }
            return GetOneWithPossibleDefault<TSource>(queryOp, false, false);
        }

        public static TSource LastOrDefault<TSource>(this ParallelQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            LastQueryOperator<TSource> queryOp = new LastQueryOperator<TSource>(source, null);
            QuerySettings settings = queryOp.SpecifiedQuerySettings.WithDefaults();
            if (queryOp.LimitsParallelism && (((ParallelExecutionMode) settings.ExecutionMode) != ParallelExecutionMode.ForceParallelism))
            {
                IEnumerable<TSource> introduced6 = queryOp.Child.AsSequentialQuery(settings.CancellationState.ExternalCancellationToken);
                IEnumerable<TSource> introduced7 = CancellableEnumerable.Wrap<TSource>(introduced6, settings.CancellationState.ExternalCancellationToken);
                return ExceptionAggregator.WrapEnumerable<TSource>(introduced7, settings.CancellationState).LastOrDefault<TSource>();
            }
            return GetOneWithPossibleDefault<TSource>(queryOp, false, true);
        }

        public static TSource LastOrDefault<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            LastQueryOperator<TSource> queryOp = new LastQueryOperator<TSource>(source, predicate);
            QuerySettings settings = queryOp.SpecifiedQuerySettings.WithDefaults();
            if (queryOp.LimitsParallelism && (((ParallelExecutionMode) settings.ExecutionMode) != ParallelExecutionMode.ForceParallelism))
            {
                IEnumerable<TSource> introduced6 = queryOp.Child.AsSequentialQuery(settings.CancellationState.ExternalCancellationToken);
                IEnumerable<TSource> introduced7 = CancellableEnumerable.Wrap<TSource>(introduced6, settings.CancellationState.ExternalCancellationToken);
                return ExceptionAggregator.WrapEnumerable<TSource>(introduced7, settings.CancellationState).LastOrDefault<TSource>(ExceptionAggregator.WrapFunc<TSource, bool>(predicate, settings.CancellationState));
            }
            return GetOneWithPossibleDefault<TSource>(queryOp, false, true);
        }

        public static long LongCount<TSource>(this ParallelQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            ParallelEnumerableWrapper<TSource> wrapper = source as ParallelEnumerableWrapper<TSource>;
            if (wrapper != null)
            {
                ICollection<TSource> wrappedEnumerable = wrapper.WrappedEnumerable as ICollection<TSource>;
                if (wrappedEnumerable != null)
                {
                    return (long) wrappedEnumerable.Count;
                }
            }
            return new LongCountAggregationOperator<TSource>(source).Aggregate();
        }

        public static long LongCount<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return new LongCountAggregationOperator<TSource>(source.Where<TSource>(predicate)).Aggregate();
        }

        public static decimal Max(this ParallelQuery<decimal> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new DecimalMinMaxAggregationOperator(source, 1).Aggregate();
        }

        public static double Max(this ParallelQuery<double> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new DoubleMinMaxAggregationOperator(source, 1).Aggregate();
        }

        public static int Max(this ParallelQuery<int> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new IntMinMaxAggregationOperator(source, 1).Aggregate();
        }

        public static long Max(this ParallelQuery<long> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new LongMinMaxAggregationOperator(source, 1).Aggregate();
        }

        public static decimal? Max(this ParallelQuery<decimal?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableDecimalMinMaxAggregationOperator(source, 1).Aggregate();
        }

        public static double? Max(this ParallelQuery<double?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableDoubleMinMaxAggregationOperator(source, 1).Aggregate();
        }

        public static int? Max(this ParallelQuery<int?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableIntMinMaxAggregationOperator(source, 1).Aggregate();
        }

        public static long? Max(this ParallelQuery<long?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableLongMinMaxAggregationOperator(source, 1).Aggregate();
        }

        public static float Max(this ParallelQuery<float> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new FloatMinMaxAggregationOperator(source, 1).Aggregate();
        }

        public static float? Max(this ParallelQuery<float?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableFloatMinMaxAggregationOperator(source, 1).Aggregate();
        }

        public static TSource Max<TSource>(this ParallelQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return AggregationMinMaxHelpers<TSource>.ReduceMax(source);
        }

        public static decimal Max<TSource>(this ParallelQuery<TSource> source, Func<TSource, decimal> selector)
        {
            return source.Select<TSource, decimal>(selector).Max<decimal>();
        }

        public static double Max<TSource>(this ParallelQuery<TSource> source, Func<TSource, double> selector)
        {
            return source.Select<TSource, double>(selector).Max<double>();
        }

        public static int Max<TSource>(this ParallelQuery<TSource> source, Func<TSource, int> selector)
        {
            return source.Select<TSource, int>(selector).Max<int>();
        }

        public static long Max<TSource>(this ParallelQuery<TSource> source, Func<TSource, long> selector)
        {
            return source.Select<TSource, long>(selector).Max<long>();
        }

        public static float Max<TSource>(this ParallelQuery<TSource> source, Func<TSource, float> selector)
        {
            return source.Select<TSource, float>(selector).Max<float>();
        }

        public static decimal? Max<TSource>(this ParallelQuery<TSource> source, Func<TSource, decimal?> selector)
        {
            return source.Select<TSource, decimal?>(selector).Max<decimal?>();
        }

        public static double? Max<TSource>(this ParallelQuery<TSource> source, Func<TSource, double?> selector)
        {
            return source.Select<TSource, double?>(selector).Max<double?>();
        }

        public static int? Max<TSource>(this ParallelQuery<TSource> source, Func<TSource, int?> selector)
        {
            return source.Select<TSource, int?>(selector).Max<int?>();
        }

        public static long? Max<TSource>(this ParallelQuery<TSource> source, Func<TSource, long?> selector)
        {
            return source.Select<TSource, long?>(selector).Max<long?>();
        }

        public static float? Max<TSource>(this ParallelQuery<TSource> source, Func<TSource, float?> selector)
        {
            return source.Select<TSource, float?>(selector).Max<float?>();
        }

        public static TResult Max<TSource, TResult>(this ParallelQuery<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Select<TSource, TResult>(selector).Max<TResult>();
        }

        public static decimal Min(this ParallelQuery<decimal> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new DecimalMinMaxAggregationOperator(source, -1).Aggregate();
        }

        public static double Min(this ParallelQuery<double> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new DoubleMinMaxAggregationOperator(source, -1).Aggregate();
        }

        public static int Min(this ParallelQuery<int> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new IntMinMaxAggregationOperator(source, -1).Aggregate();
        }

        public static long Min(this ParallelQuery<long> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new LongMinMaxAggregationOperator(source, -1).Aggregate();
        }

        public static decimal? Min(this ParallelQuery<decimal?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableDecimalMinMaxAggregationOperator(source, -1).Aggregate();
        }

        public static double? Min(this ParallelQuery<double?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableDoubleMinMaxAggregationOperator(source, -1).Aggregate();
        }

        public static int? Min(this ParallelQuery<int?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableIntMinMaxAggregationOperator(source, -1).Aggregate();
        }

        public static long? Min(this ParallelQuery<long?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableLongMinMaxAggregationOperator(source, -1).Aggregate();
        }

        public static float Min(this ParallelQuery<float> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new FloatMinMaxAggregationOperator(source, -1).Aggregate();
        }

        public static float? Min(this ParallelQuery<float?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableFloatMinMaxAggregationOperator(source, -1).Aggregate();
        }

        public static TSource Min<TSource>(this ParallelQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return AggregationMinMaxHelpers<TSource>.ReduceMin(source);
        }

        public static decimal Min<TSource>(this ParallelQuery<TSource> source, Func<TSource, decimal> selector)
        {
            return source.Select<TSource, decimal>(selector).Min<decimal>();
        }

        public static double Min<TSource>(this ParallelQuery<TSource> source, Func<TSource, double> selector)
        {
            return source.Select<TSource, double>(selector).Min<double>();
        }

        public static int Min<TSource>(this ParallelQuery<TSource> source, Func<TSource, int> selector)
        {
            return source.Select<TSource, int>(selector).Min<int>();
        }

        public static long Min<TSource>(this ParallelQuery<TSource> source, Func<TSource, long> selector)
        {
            return source.Select<TSource, long>(selector).Min<long>();
        }

        public static decimal? Min<TSource>(this ParallelQuery<TSource> source, Func<TSource, decimal?> selector)
        {
            return source.Select<TSource, decimal?>(selector).Min<decimal?>();
        }

        public static double? Min<TSource>(this ParallelQuery<TSource> source, Func<TSource, double?> selector)
        {
            return source.Select<TSource, double?>(selector).Min<double?>();
        }

        public static int? Min<TSource>(this ParallelQuery<TSource> source, Func<TSource, int?> selector)
        {
            return source.Select<TSource, int?>(selector).Min<int?>();
        }

        public static long? Min<TSource>(this ParallelQuery<TSource> source, Func<TSource, long?> selector)
        {
            return source.Select<TSource, long?>(selector).Min<long?>();
        }

        public static float? Min<TSource>(this ParallelQuery<TSource> source, Func<TSource, float?> selector)
        {
            return source.Select<TSource, float?>(selector).Min<float?>();
        }

        public static float Min<TSource>(this ParallelQuery<TSource> source, Func<TSource, float> selector)
        {
            return source.Select<TSource, float>(selector).Min<float>();
        }

        public static TResult Min<TSource, TResult>(this ParallelQuery<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Select<TSource, TResult>(selector).Min<TResult>();
        }

        public static ParallelQuery<TResult> OfType<TResult>(this ParallelQuery source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return source.OfType<TResult>();
        }

        public static OrderedParallelQuery<TSource> OrderBy<TSource, TKey>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            return new OrderedParallelQuery<TSource>(new SortQueryOperator<TSource, TKey>(source, keySelector, null, false));
        }

        public static OrderedParallelQuery<TSource> OrderBy<TSource, TKey>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            return new OrderedParallelQuery<TSource>(new SortQueryOperator<TSource, TKey>(source, keySelector, comparer, false));
        }

        public static OrderedParallelQuery<TSource> OrderByDescending<TSource, TKey>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            return new OrderedParallelQuery<TSource>(new SortQueryOperator<TSource, TKey>(source, keySelector, null, true));
        }

        public static OrderedParallelQuery<TSource> OrderByDescending<TSource, TKey>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            return new OrderedParallelQuery<TSource>(new SortQueryOperator<TSource, TKey>(source, keySelector, comparer, true));
        }

        private static T PerformAggregation<T>(this ParallelQuery<T> source, Func<T, T, T> reduce, T seed, bool seedIsSpecified, bool throwIfEmpty, QueryAggregationOptions options)
        {
            AssociativeAggregationOperator<T, T, T> @operator = new AssociativeAggregationOperator<T, T, T>(source, seed, null, seedIsSpecified, reduce, reduce, obj => obj, throwIfEmpty, options);
            return @operator.Aggregate();
        }

        private static TAccumulate PerformSequentialAggregation<TSource, TAccumulate>(this ParallelQuery<TSource> source, TAccumulate seed, bool seedIsSpecified, Func<TAccumulate, TSource, TAccumulate> func)
        {
            using (IEnumerator<TSource> enumerator = source.GetEnumerator())
            {
                TAccumulate current;
                if (seedIsSpecified)
                {
                    current = seed;
                }
                else
                {
                    if (!enumerator.MoveNext())
                    {
                        throw new InvalidOperationException(System.Linq.SR.GetString("NoElements"));
                    }
                    current = (TAccumulate) enumerator.Current;
                }
                while (enumerator.MoveNext())
                {
                    TSource local2 = enumerator.Current;
                    try
                    {
                        current = func(current, local2);
                        continue;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        throw new AggregateException(new Exception[] { exception });
                    }
                }
                return current;
            }
        }

        public static ParallelQuery<int> Range(int start, int count)
        {
            if ((count < 0) || ((count > 0) && ((0x7fffffff - (count - 1)) < start)))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            return new RangeEnumerable(start, count);
        }

        public static ParallelQuery<TResult> Repeat<TResult>(TResult element, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            return new RepeatEnumerable<TResult>(element, count);
        }

        public static ParallelQuery<TSource> Reverse<TSource>(this ParallelQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new ReverseQueryOperator<TSource>(source);
        }

        public static ParallelQuery<TResult> Select<TSource, TResult>(this ParallelQuery<TSource> source, Func<TSource, TResult> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            return new SelectQueryOperator<TSource, TResult>(source, selector);
        }

        public static ParallelQuery<TResult> Select<TSource, TResult>(this ParallelQuery<TSource> source, Func<TSource, int, TResult> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            return new IndexedSelectQueryOperator<TSource, TResult>(source, selector);
        }

        public static ParallelQuery<TResult> SelectMany<TSource, TResult>(this ParallelQuery<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            return new SelectManyQueryOperator<TSource, TResult, TResult>(source, selector, null, null);
        }

        public static ParallelQuery<TResult> SelectMany<TSource, TResult>(this ParallelQuery<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            return new SelectManyQueryOperator<TSource, TResult, TResult>(source, null, selector, null);
        }

        public static ParallelQuery<TResult> SelectMany<TSource, TCollection, TResult>(this ParallelQuery<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (collectionSelector == null)
            {
                throw new ArgumentNullException("collectionSelector");
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }
            return (ParallelQuery<TResult>) new SelectManyQueryOperator<TSource, TCollection, TResult>(source, collectionSelector, null, resultSelector);
        }

        public static ParallelQuery<TResult> SelectMany<TSource, TCollection, TResult>(this ParallelQuery<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (collectionSelector == null)
            {
                throw new ArgumentNullException("collectionSelector");
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }
            return (ParallelQuery<TResult>) new SelectManyQueryOperator<TSource, TCollection, TResult>(source, null, collectionSelector, resultSelector);
        }

        [Obsolete("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
        public static bool SequenceEqual<TSource>(this ParallelQuery<TSource> first, IEnumerable<TSource> second)
        {
            throw new NotSupportedException(System.Linq.SR.GetString("ParallelEnumerable_BinaryOpMustUseAsParallel"));
        }

        public static bool SequenceEqual<TSource>(this ParallelQuery<TSource> first, ParallelQuery<TSource> second)
        {
            if (first == null)
            {
                throw new ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new ArgumentNullException("second");
            }
            return first.SequenceEqual<TSource>(second, null);
        }

        [Obsolete("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
        public static bool SequenceEqual<TSource>(this ParallelQuery<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            throw new NotSupportedException(System.Linq.SR.GetString("ParallelEnumerable_BinaryOpMustUseAsParallel"));
        }

        public static bool SequenceEqual<TSource>(this ParallelQuery<TSource> first, ParallelQuery<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null)
            {
                throw new ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new ArgumentNullException("second");
            }
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            QueryOperator<TSource> @operator = QueryOperator<TSource>.AsQueryOperator(first);
            QueryOperator<TSource> operator2 = QueryOperator<TSource>.AsQueryOperator(second);
            QuerySettings settings = @operator.SpecifiedQuerySettings.Merge(operator2.SpecifiedQuerySettings).WithDefaults().WithPerExecutionSettings(new CancellationTokenSource(), new System.Linq.Parallel.Shared<bool>(false));
            IEnumerator<TSource> e = first.GetEnumerator();
            try
            {
                IEnumerator<TSource> enumerator = second.GetEnumerator();
                try
                {
                    while (e.MoveNext())
                    {
                        if (!enumerator.MoveNext() || !comparer.Equals(e.Current, enumerator.Current))
                        {
                            return false;
                        }
                    }
                    if (enumerator.MoveNext())
                    {
                        return false;
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    ExceptionAggregator.ThrowOCEorAggregateException(exception, settings.CancellationState);
                }
                finally
                {
                    DisposeEnumerator<TSource>(enumerator, settings.CancellationState);
                }
            }
            finally
            {
                DisposeEnumerator<TSource>(e, settings.CancellationState);
            }
            return true;
        }

        public static TSource Single<TSource>(this ParallelQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return GetOneWithPossibleDefault<TSource>(new SingleQueryOperator<TSource>(source, null), true, false);
        }

        public static TSource Single<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return GetOneWithPossibleDefault<TSource>(new SingleQueryOperator<TSource>(source, predicate), true, false);
        }

        public static TSource SingleOrDefault<TSource>(this ParallelQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return GetOneWithPossibleDefault<TSource>(new SingleQueryOperator<TSource>(source, null), true, true);
        }

        public static TSource SingleOrDefault<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return GetOneWithPossibleDefault<TSource>(new SingleQueryOperator<TSource>(source, predicate), true, true);
        }

        public static ParallelQuery<TSource> Skip<TSource>(this ParallelQuery<TSource> source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (count <= 0)
            {
                return source;
            }
            return new TakeOrSkipQueryOperator<TSource>(source, count, false);
        }

        public static ParallelQuery<TSource> SkipWhile<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return new TakeOrSkipWhileQueryOperator<TSource>(source, predicate, null, false);
        }

        public static ParallelQuery<TSource> SkipWhile<TSource>(this ParallelQuery<TSource> source, Func<TSource, int, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return new TakeOrSkipWhileQueryOperator<TSource>(source, null, predicate, false);
        }

        public static decimal Sum(this ParallelQuery<decimal> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new DecimalSumAggregationOperator(source).Aggregate();
        }

        public static double Sum(this ParallelQuery<double> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new DoubleSumAggregationOperator(source).Aggregate();
        }

        public static int Sum(this ParallelQuery<int> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new IntSumAggregationOperator(source).Aggregate();
        }

        public static long Sum(this ParallelQuery<long> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new LongSumAggregationOperator(source).Aggregate();
        }

        public static decimal? Sum(this ParallelQuery<decimal?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableDecimalSumAggregationOperator(source).Aggregate();
        }

        public static double? Sum(this ParallelQuery<double?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableDoubleSumAggregationOperator(source).Aggregate();
        }

        public static int? Sum(this ParallelQuery<int?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableIntSumAggregationOperator(source).Aggregate();
        }

        public static long? Sum(this ParallelQuery<long?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableLongSumAggregationOperator(source).Aggregate();
        }

        public static float Sum(this ParallelQuery<float> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new FloatSumAggregationOperator(source).Aggregate();
        }

        public static float? Sum(this ParallelQuery<float?> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new NullableFloatSumAggregationOperator(source).Aggregate();
        }

        public static decimal Sum<TSource>(this ParallelQuery<TSource> source, Func<TSource, decimal> selector)
        {
            return source.Select<TSource, decimal>(selector).Sum();
        }

        public static double Sum<TSource>(this ParallelQuery<TSource> source, Func<TSource, double> selector)
        {
            return source.Select<TSource, double>(selector).Sum();
        }

        public static int Sum<TSource>(this ParallelQuery<TSource> source, Func<TSource, int> selector)
        {
            return source.Select<TSource, int>(selector).Sum();
        }

        public static long Sum<TSource>(this ParallelQuery<TSource> source, Func<TSource, long> selector)
        {
            return source.Select<TSource, long>(selector).Sum();
        }

        public static int? Sum<TSource>(this ParallelQuery<TSource> source, Func<TSource, int?> selector)
        {
            return source.Select<TSource, int?>(selector).Sum();
        }

        public static long? Sum<TSource>(this ParallelQuery<TSource> source, Func<TSource, long?> selector)
        {
            return source.Select<TSource, long?>(selector).Sum();
        }

        public static decimal? Sum<TSource>(this ParallelQuery<TSource> source, Func<TSource, decimal?> selector)
        {
            return source.Select<TSource, decimal?>(selector).Sum();
        }

        public static float Sum<TSource>(this ParallelQuery<TSource> source, Func<TSource, float> selector)
        {
            return source.Select<TSource, float>(selector).Sum();
        }

        public static double? Sum<TSource>(this ParallelQuery<TSource> source, Func<TSource, double?> selector)
        {
            return source.Select<TSource, double?>(selector).Sum();
        }

        public static float? Sum<TSource>(this ParallelQuery<TSource> source, Func<TSource, float?> selector)
        {
            return source.Select<TSource, float?>(selector).Sum();
        }

        public static ParallelQuery<TSource> Take<TSource>(this ParallelQuery<TSource> source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (count > 0)
            {
                return new TakeOrSkipQueryOperator<TSource>(source, count, true);
            }
            return Empty<TSource>();
        }

        public static ParallelQuery<TSource> TakeWhile<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return new TakeOrSkipWhileQueryOperator<TSource>(source, predicate, null, true);
        }

        public static ParallelQuery<TSource> TakeWhile<TSource>(this ParallelQuery<TSource> source, Func<TSource, int, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return new TakeOrSkipWhileQueryOperator<TSource>(source, null, predicate, true);
        }

        public static OrderedParallelQuery<TSource> ThenBy<TSource, TKey>(this OrderedParallelQuery<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            return new OrderedParallelQuery<TSource>((QueryOperator<TSource>) source.OrderedEnumerable.CreateOrderedEnumerable<TKey>(keySelector, null, false));
        }

        public static OrderedParallelQuery<TSource> ThenBy<TSource, TKey>(this OrderedParallelQuery<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            return new OrderedParallelQuery<TSource>((QueryOperator<TSource>) source.OrderedEnumerable.CreateOrderedEnumerable<TKey>(keySelector, comparer, false));
        }

        public static OrderedParallelQuery<TSource> ThenByDescending<TSource, TKey>(this OrderedParallelQuery<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            return new OrderedParallelQuery<TSource>((QueryOperator<TSource>) source.OrderedEnumerable.CreateOrderedEnumerable<TKey>(keySelector, null, true));
        }

        public static OrderedParallelQuery<TSource> ThenByDescending<TSource, TKey>(this OrderedParallelQuery<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            return new OrderedParallelQuery<TSource>((QueryOperator<TSource>) source.OrderedEnumerable.CreateOrderedEnumerable<TKey>(keySelector, comparer, true));
        }

        public static TSource[] ToArray<TSource>(this ParallelQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            QueryOperator<TSource> @operator = source as QueryOperator<TSource>;
            if (@operator != null)
            {
                return @operator.ExecuteAndGetResultsAsArray();
            }
            return source.ToList<TSource>().ToArray<TSource>();
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.ToDictionary<TSource, TKey>(keySelector, EqualityComparer<TKey>.Default);
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            Dictionary<TKey, TSource> dictionary = new Dictionary<TKey, TSource>(comparer);
            QueryOperator<TSource> @operator = source as QueryOperator<TSource>;
            IEnumerator<TSource> enumerator = (@operator == null) ? source.GetEnumerator() : @operator.GetEnumerator(3, true);
            using (enumerator)
            {
                while (enumerator.MoveNext())
                {
                    TSource current = enumerator.Current;
                    try
                    {
                        TKey key = keySelector(current);
                        dictionary.Add(key, current);
                        continue;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        throw new AggregateException(new Exception[] { exception });
                    }
                }
            }
            return dictionary;
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return source.ToDictionary<TSource, TKey, TElement>(keySelector, elementSelector, EqualityComparer<TKey>.Default);
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (elementSelector == null)
            {
                throw new ArgumentNullException("elementSelector");
            }
            Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>(comparer);
            QueryOperator<TSource> @operator = source as QueryOperator<TSource>;
            IEnumerator<TSource> enumerator = (@operator == null) ? source.GetEnumerator() : @operator.GetEnumerator(3, true);
            using (enumerator)
            {
                while (enumerator.MoveNext())
                {
                    TSource current = enumerator.Current;
                    try
                    {
                        dictionary.Add(keySelector(current), elementSelector(current));
                        continue;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        throw new AggregateException(new Exception[] { exception });
                    }
                }
            }
            return dictionary;
        }

        public static List<TSource> ToList<TSource>(this ParallelQuery<TSource> source)
        {
            IEnumerator<TSource> enumerator;
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            List<TSource> list = new List<TSource>();
            QueryOperator<TSource> @operator = source as QueryOperator<TSource>;
            if (@operator != null)
            {
                if ((@operator.OrdinalIndexState == OrdinalIndexState.Indexible) && @operator.OutputOrdered)
                {
                    return new List<TSource>(source.ToArray<TSource>());
                }
                enumerator = @operator.GetEnumerator(3);
            }
            else
            {
                enumerator = source.GetEnumerator();
            }
            using (enumerator)
            {
                while (enumerator.MoveNext())
                {
                    list.Add(enumerator.Current);
                }
            }
            return list;
        }

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.ToLookup<TSource, TKey>(keySelector, EqualityComparer<TKey>.Default);
        }

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            ParallelQuery<IGrouping<TKey, TSource>> query = source.GroupBy<TSource, TKey>(keySelector, comparer);
            System.Linq.Parallel.Lookup<TKey, TSource> lookup = new System.Linq.Parallel.Lookup<TKey, TSource>(comparer);
            QueryOperator<IGrouping<TKey, TSource>> @operator = query as QueryOperator<IGrouping<TKey, TSource>>;
            IEnumerator<IGrouping<TKey, TSource>> enumerator = (@operator == null) ? query.GetEnumerator() : @operator.GetEnumerator(3);
            using (enumerator)
            {
                while (enumerator.MoveNext())
                {
                    lookup.Add(enumerator.Current);
                }
            }
            return lookup;
        }

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return source.ToLookup<TSource, TKey, TElement>(keySelector, elementSelector, EqualityComparer<TKey>.Default);
        }

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (elementSelector == null)
            {
                throw new ArgumentNullException("elementSelector");
            }
            ParallelQuery<IGrouping<TKey, TElement>> query = source.GroupBy<TSource, TKey, TElement>(keySelector, elementSelector, comparer);
            System.Linq.Parallel.Lookup<TKey, TElement> lookup = new System.Linq.Parallel.Lookup<TKey, TElement>(comparer);
            QueryOperator<IGrouping<TKey, TElement>> @operator = query as QueryOperator<IGrouping<TKey, TElement>>;
            IEnumerator<IGrouping<TKey, TElement>> enumerator = (@operator == null) ? query.GetEnumerator() : @operator.GetEnumerator(3);
            using (enumerator)
            {
                while (enumerator.MoveNext())
                {
                    lookup.Add(enumerator.Current);
                }
            }
            return lookup;
        }

        [Obsolete("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
        public static ParallelQuery<TSource> Union<TSource>(this ParallelQuery<TSource> first, IEnumerable<TSource> second)
        {
            throw new NotSupportedException(System.Linq.SR.GetString("ParallelEnumerable_BinaryOpMustUseAsParallel"));
        }

        public static ParallelQuery<TSource> Union<TSource>(this ParallelQuery<TSource> first, ParallelQuery<TSource> second)
        {
            return first.Union<TSource>(second, null);
        }

        [Obsolete("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
        public static ParallelQuery<TSource> Union<TSource>(this ParallelQuery<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            throw new NotSupportedException(System.Linq.SR.GetString("ParallelEnumerable_BinaryOpMustUseAsParallel"));
        }

        public static ParallelQuery<TSource> Union<TSource>(this ParallelQuery<TSource> first, ParallelQuery<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null)
            {
                throw new ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new ArgumentNullException("second");
            }
            return new UnionQueryOperator<TSource>(first, second, comparer);
        }

        public static ParallelQuery<TSource> Where<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return new WhereQueryOperator<TSource>(source, predicate);
        }

        public static ParallelQuery<TSource> Where<TSource>(this ParallelQuery<TSource> source, Func<TSource, int, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return new IndexedWhereQueryOperator<TSource>(source, predicate);
        }

        public static ParallelQuery<TSource> WithCancellation<TSource>(this ParallelQuery<TSource> source, CancellationToken cancellationToken)
        {
            Action callback = null;
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            CancellationTokenRegistration registration = new CancellationTokenRegistration();
            try
            {
                if (callback == null)
                {
                    callback = delegate {
                    };
                }
                registration = cancellationToken.Register(callback);
            }
            catch (ObjectDisposedException)
            {
                throw new ArgumentException(System.Linq.SR.GetString("ParallelEnumerable_WithCancellation_TokenSourceDisposed"), "cancellationToken");
            }
            finally
            {
                registration.Dispose();
            }
            QuerySettings empty = QuerySettings.Empty;
            empty.CancellationState = new CancellationState(cancellationToken);
            return new QueryExecutionOption<TSource>(QueryOperator<TSource>.AsQueryOperator(source), empty);
        }

        public static ParallelQuery<TSource> WithDegreeOfParallelism<TSource>(this ParallelQuery<TSource> source, int degreeOfParallelism)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if ((degreeOfParallelism < 1) || (degreeOfParallelism > 0x3f))
            {
                throw new ArgumentOutOfRangeException("degreeOfParallelism");
            }
            QuerySettings empty = QuerySettings.Empty;
            empty.DegreeOfParallelism = new int?(degreeOfParallelism);
            return new QueryExecutionOption<TSource>(QueryOperator<TSource>.AsQueryOperator(source), empty);
        }

        public static ParallelQuery<TSource> WithExecutionMode<TSource>(this ParallelQuery<TSource> source, ParallelExecutionMode executionMode)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if ((executionMode != ParallelExecutionMode.Default) && (executionMode != ParallelExecutionMode.ForceParallelism))
            {
                throw new ArgumentException(System.Linq.SR.GetString("ParallelEnumerable_WithQueryExecutionMode_InvalidMode"));
            }
            QuerySettings empty = QuerySettings.Empty;
            empty.ExecutionMode = new ParallelExecutionMode?(executionMode);
            return new QueryExecutionOption<TSource>(QueryOperator<TSource>.AsQueryOperator(source), empty);
        }

        public static ParallelQuery<TSource> WithMergeOptions<TSource>(this ParallelQuery<TSource> source, ParallelMergeOptions mergeOptions)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (((mergeOptions != ParallelMergeOptions.Default) && (mergeOptions != ParallelMergeOptions.AutoBuffered)) && ((mergeOptions != ParallelMergeOptions.NotBuffered) && (mergeOptions != ParallelMergeOptions.FullyBuffered)))
            {
                throw new ArgumentException(System.Linq.SR.GetString("ParallelEnumerable_WithMergeOptions_InvalidOptions"));
            }
            QuerySettings empty = QuerySettings.Empty;
            empty.MergeOptions = new ParallelMergeOptions?(mergeOptions);
            return new QueryExecutionOption<TSource>(QueryOperator<TSource>.AsQueryOperator(source), empty);
        }

        internal static ParallelQuery<TSource> WithTaskScheduler<TSource>(this ParallelQuery<TSource> source, TaskScheduler taskScheduler)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (taskScheduler == null)
            {
                throw new ArgumentNullException("taskScheduler");
            }
            QuerySettings empty = QuerySettings.Empty;
            empty.TaskScheduler = taskScheduler;
            return new QueryExecutionOption<TSource>(QueryOperator<TSource>.AsQueryOperator(source), empty);
        }

        [Obsolete("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
        public static ParallelQuery<TResult> Zip<TFirst, TSecond, TResult>(this ParallelQuery<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
        {
            throw new NotSupportedException(System.Linq.SR.GetString("ParallelEnumerable_BinaryOpMustUseAsParallel"));
        }

        public static ParallelQuery<TResult> Zip<TFirst, TSecond, TResult>(this ParallelQuery<TFirst> first, ParallelQuery<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
        {
            if (first == null)
            {
                throw new ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new ArgumentNullException("second");
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }
            return new ZipQueryOperator<TFirst, TSecond, TResult>(first, second, resultSelector);
        }
    }
}

