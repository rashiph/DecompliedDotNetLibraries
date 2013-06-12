namespace System.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Parallel;

    internal static class AggregationMinMaxHelpers<T>
    {
        private static Func<Pair<bool, T>, Pair<bool, T>, Pair<bool, T>> MakeFinalReduceFunction(int sign)
        {
            Comparer<T> comparer = Util.GetDefaultComparer<T>();
            return delegate (Pair<bool, T> accumulator, Pair<bool, T> element) {
                if (!element.First || (accumulator.First && (Util.Sign(comparer.Compare(element.Second, accumulator.Second)) != sign)))
                {
                    return accumulator;
                }
                return new Pair<bool, T>(true, element.Second);
            };
        }

        private static Func<Pair<bool, T>, T, Pair<bool, T>> MakeIntermediateReduceFunction(int sign)
        {
            Comparer<T> comparer = Util.GetDefaultComparer<T>();
            return delegate (Pair<bool, T> accumulator, T element) {
                if ((default(T) == null) && (element == null))
                {
                    return accumulator;
                }
                if (accumulator.First && (Util.Sign(comparer.Compare(element, accumulator.Second)) != sign))
                {
                    return accumulator;
                }
                return new Pair<bool, T>(true, element);
            };
        }

        private static Func<Pair<bool, T>, T> MakeResultSelectorFunction()
        {
            return accumulator => accumulator.Second;
        }

        private static T Reduce(IEnumerable<T> source, int sign)
        {
            Func<Pair<bool, T>, T, Pair<bool, T>> intermediateReduce = AggregationMinMaxHelpers<T>.MakeIntermediateReduceFunction(sign);
            Func<Pair<bool, T>, Pair<bool, T>, Pair<bool, T>> finalReduce = AggregationMinMaxHelpers<T>.MakeFinalReduceFunction(sign);
            Func<Pair<bool, T>, T> resultSelector = AggregationMinMaxHelpers<T>.MakeResultSelectorFunction();
            AssociativeAggregationOperator<T, Pair<bool, T>, T> @operator = new AssociativeAggregationOperator<T, Pair<bool, T>, T>(source, new Pair<bool, T>(false, default(T)), null, true, intermediateReduce, finalReduce, resultSelector, default(T) != null, QueryAggregationOptions.AssociativeCommutative);
            return @operator.Aggregate();
        }

        internal static T ReduceMax(IEnumerable<T> source)
        {
            return AggregationMinMaxHelpers<T>.Reduce(source, 1);
        }

        internal static T ReduceMin(IEnumerable<T> source)
        {
            return AggregationMinMaxHelpers<T>.Reduce(source, -1);
        }
    }
}

