namespace System.Threading.Tasks
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public static class Parallel
    {
        internal const int DEFAULT_LOOP_STRIDE = 0x10;
        internal static ParallelOptions s_defaultParallelOptions = new ParallelOptions();
        internal static int s_forkJoinContextID;

        public static ParallelLoopResult For(int fromInclusive, int toExclusive, Action<int> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            return ForWorker<object>(fromInclusive, toExclusive, s_defaultParallelOptions, body, null, null, null, null);
        }

        public static ParallelLoopResult For(int fromInclusive, int toExclusive, Action<int, ParallelLoopState> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            return ForWorker<object>(fromInclusive, toExclusive, s_defaultParallelOptions, null, body, null, null, null);
        }

        public static ParallelLoopResult For(long fromInclusive, long toExclusive, Action<long> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            return ForWorker64<object>(fromInclusive, toExclusive, s_defaultParallelOptions, body, null, null, null, null);
        }

        public static ParallelLoopResult For(long fromInclusive, long toExclusive, Action<long, ParallelLoopState> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            return ForWorker64<object>(fromInclusive, toExclusive, s_defaultParallelOptions, null, body, null, null, null);
        }

        public static ParallelLoopResult For(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Action<int> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            return ForWorker<object>(fromInclusive, toExclusive, parallelOptions, body, null, null, null, null);
        }

        public static ParallelLoopResult For(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Action<int, ParallelLoopState> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            return ForWorker<object>(fromInclusive, toExclusive, parallelOptions, null, body, null, null, null);
        }

        public static ParallelLoopResult For(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Action<long> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            return ForWorker64<object>(fromInclusive, toExclusive, parallelOptions, body, null, null, null, null);
        }

        public static ParallelLoopResult For(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Action<long, ParallelLoopState> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            return ForWorker64<object>(fromInclusive, toExclusive, parallelOptions, null, body, null, null, null);
        }

        public static ParallelLoopResult For<TLocal>(int fromInclusive, int toExclusive, Func<TLocal> localInit, Func<int, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (localInit == null)
            {
                throw new ArgumentNullException("localInit");
            }
            if (localFinally == null)
            {
                throw new ArgumentNullException("localFinally");
            }
            return ForWorker<TLocal>(fromInclusive, toExclusive, s_defaultParallelOptions, null, null, body, localInit, localFinally);
        }

        public static ParallelLoopResult For<TLocal>(long fromInclusive, long toExclusive, Func<TLocal> localInit, Func<long, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (localInit == null)
            {
                throw new ArgumentNullException("localInit");
            }
            if (localFinally == null)
            {
                throw new ArgumentNullException("localFinally");
            }
            return ForWorker64<TLocal>(fromInclusive, toExclusive, s_defaultParallelOptions, null, null, body, localInit, localFinally);
        }

        public static ParallelLoopResult For<TLocal>(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Func<TLocal> localInit, Func<int, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (localInit == null)
            {
                throw new ArgumentNullException("localInit");
            }
            if (localFinally == null)
            {
                throw new ArgumentNullException("localFinally");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            return ForWorker<TLocal>(fromInclusive, toExclusive, parallelOptions, null, null, body, localInit, localFinally);
        }

        public static ParallelLoopResult For<TLocal>(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Func<TLocal> localInit, Func<long, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (localInit == null)
            {
                throw new ArgumentNullException("localInit");
            }
            if (localFinally == null)
            {
                throw new ArgumentNullException("localFinally");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            return ForWorker64<TLocal>(fromInclusive, toExclusive, parallelOptions, null, null, body, localInit, localFinally);
        }

        public static ParallelLoopResult ForEach<TSource>(OrderablePartitioner<TSource> source, Action<TSource, ParallelLoopState, long> body)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (!source.KeysNormalized)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Parallel_ForEach_OrderedPartitionerKeysNotNormalized"));
            }
            return PartitionerForEachWorker<TSource, object>(source, s_defaultParallelOptions, null, null, body, null, null, null, null);
        }

        public static ParallelLoopResult ForEach<TSource>(Partitioner<TSource> source, Action<TSource> body)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            return PartitionerForEachWorker<TSource, object>(source, s_defaultParallelOptions, body, null, null, null, null, null, null);
        }

        public static ParallelLoopResult ForEach<TSource>(Partitioner<TSource> source, Action<TSource, ParallelLoopState> body)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            return PartitionerForEachWorker<TSource, object>(source, s_defaultParallelOptions, null, body, null, null, null, null, null);
        }

        public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, Action<TSource> body)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            return ForEachWorker<TSource, object>(source, s_defaultParallelOptions, body, null, null, null, null, null, null);
        }

        public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, Action<TSource, ParallelLoopState> body)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            return ForEachWorker<TSource, object>(source, s_defaultParallelOptions, null, body, null, null, null, null, null);
        }

        public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, Action<TSource, ParallelLoopState, long> body)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            return ForEachWorker<TSource, object>(source, s_defaultParallelOptions, null, null, body, null, null, null, null);
        }

        public static ParallelLoopResult ForEach<TSource>(OrderablePartitioner<TSource> source, ParallelOptions parallelOptions, Action<TSource, ParallelLoopState, long> body)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            if (!source.KeysNormalized)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Parallel_ForEach_OrderedPartitionerKeysNotNormalized"));
            }
            return PartitionerForEachWorker<TSource, object>(source, parallelOptions, null, null, body, null, null, null, null);
        }

        public static ParallelLoopResult ForEach<TSource>(Partitioner<TSource> source, ParallelOptions parallelOptions, Action<TSource> body)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            return PartitionerForEachWorker<TSource, object>(source, parallelOptions, body, null, null, null, null, null, null);
        }

        public static ParallelLoopResult ForEach<TSource>(Partitioner<TSource> source, ParallelOptions parallelOptions, Action<TSource, ParallelLoopState> body)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            return PartitionerForEachWorker<TSource, object>(source, parallelOptions, null, body, null, null, null, null, null);
        }

        public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource> body)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            return ForEachWorker<TSource, object>(source, parallelOptions, body, null, null, null, null, null, null);
        }

        public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource, ParallelLoopState> body)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            return ForEachWorker<TSource, object>(source, parallelOptions, null, body, null, null, null, null, null);
        }

        public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource, ParallelLoopState, long> body)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            return ForEachWorker<TSource, object>(source, parallelOptions, null, null, body, null, null, null, null);
        }

        public static ParallelLoopResult ForEach<TSource, TLocal>(OrderablePartitioner<TSource> source, Func<TLocal> localInit, Func<TSource, ParallelLoopState, long, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (localInit == null)
            {
                throw new ArgumentNullException("localInit");
            }
            if (localFinally == null)
            {
                throw new ArgumentNullException("localFinally");
            }
            if (!source.KeysNormalized)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Parallel_ForEach_OrderedPartitionerKeysNotNormalized"));
            }
            return PartitionerForEachWorker<TSource, TLocal>(source, s_defaultParallelOptions, null, null, null, null, body, localInit, localFinally);
        }

        public static ParallelLoopResult ForEach<TSource, TLocal>(Partitioner<TSource> source, Func<TLocal> localInit, Func<TSource, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (localInit == null)
            {
                throw new ArgumentNullException("localInit");
            }
            if (localFinally == null)
            {
                throw new ArgumentNullException("localFinally");
            }
            return PartitionerForEachWorker<TSource, TLocal>(source, s_defaultParallelOptions, null, null, null, body, null, localInit, localFinally);
        }

        public static ParallelLoopResult ForEach<TSource, TLocal>(IEnumerable<TSource> source, Func<TLocal> localInit, Func<TSource, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (localInit == null)
            {
                throw new ArgumentNullException("localInit");
            }
            if (localFinally == null)
            {
                throw new ArgumentNullException("localFinally");
            }
            return ForEachWorker<TSource, TLocal>(source, s_defaultParallelOptions, null, null, null, body, null, localInit, localFinally);
        }

        public static ParallelLoopResult ForEach<TSource, TLocal>(IEnumerable<TSource> source, Func<TLocal> localInit, Func<TSource, ParallelLoopState, long, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (localInit == null)
            {
                throw new ArgumentNullException("localInit");
            }
            if (localFinally == null)
            {
                throw new ArgumentNullException("localFinally");
            }
            return ForEachWorker<TSource, TLocal>(source, s_defaultParallelOptions, null, null, null, null, body, localInit, localFinally);
        }

        public static ParallelLoopResult ForEach<TSource, TLocal>(OrderablePartitioner<TSource> source, ParallelOptions parallelOptions, Func<TLocal> localInit, Func<TSource, ParallelLoopState, long, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (localInit == null)
            {
                throw new ArgumentNullException("localInit");
            }
            if (localFinally == null)
            {
                throw new ArgumentNullException("localFinally");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            if (!source.KeysNormalized)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Parallel_ForEach_OrderedPartitionerKeysNotNormalized"));
            }
            return PartitionerForEachWorker<TSource, TLocal>(source, parallelOptions, null, null, null, null, body, localInit, localFinally);
        }

        public static ParallelLoopResult ForEach<TSource, TLocal>(Partitioner<TSource> source, ParallelOptions parallelOptions, Func<TLocal> localInit, Func<TSource, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (localInit == null)
            {
                throw new ArgumentNullException("localInit");
            }
            if (localFinally == null)
            {
                throw new ArgumentNullException("localFinally");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            return PartitionerForEachWorker<TSource, TLocal>(source, parallelOptions, null, null, null, body, null, localInit, localFinally);
        }

        public static ParallelLoopResult ForEach<TSource, TLocal>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Func<TLocal> localInit, Func<TSource, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (localInit == null)
            {
                throw new ArgumentNullException("localInit");
            }
            if (localFinally == null)
            {
                throw new ArgumentNullException("localFinally");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            return ForEachWorker<TSource, TLocal>(source, parallelOptions, null, null, null, body, null, localInit, localFinally);
        }

        public static ParallelLoopResult ForEach<TSource, TLocal>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Func<TLocal> localInit, Func<TSource, ParallelLoopState, long, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (localInit == null)
            {
                throw new ArgumentNullException("localInit");
            }
            if (localFinally == null)
            {
                throw new ArgumentNullException("localFinally");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            return ForEachWorker<TSource, TLocal>(source, parallelOptions, null, null, null, null, body, localInit, localFinally);
        }

        private static ParallelLoopResult ForEachWorker<TSource, TLocal>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource> body, Action<TSource, ParallelLoopState> bodyWithState, Action<TSource, ParallelLoopState, long> bodyWithStateAndIndex, Func<TSource, ParallelLoopState, TLocal, TLocal> bodyWithStateAndLocal, Func<TSource, ParallelLoopState, long, TLocal, TLocal> bodyWithEverything, Func<TLocal> localInit, Action<TLocal> localFinally)
        {
            if (parallelOptions.CancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(parallelOptions.CancellationToken);
            }
            TSource[] array = source as TSource[];
            if (array != null)
            {
                return ForEachWorker<TSource, TLocal>(array, parallelOptions, body, bodyWithState, bodyWithStateAndIndex, bodyWithStateAndLocal, bodyWithEverything, localInit, localFinally);
            }
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                return ForEachWorker<TSource, TLocal>(list, parallelOptions, body, bodyWithState, bodyWithStateAndIndex, bodyWithStateAndLocal, bodyWithEverything, localInit, localFinally);
            }
            return PartitionerForEachWorker<TSource, TLocal>(Partitioner.Create<TSource>(source), parallelOptions, body, bodyWithState, bodyWithStateAndIndex, bodyWithStateAndLocal, bodyWithEverything, localInit, localFinally);
        }

        private static ParallelLoopResult ForEachWorker<TSource, TLocal>(TSource[] array, ParallelOptions parallelOptions, Action<TSource> body, Action<TSource, ParallelLoopState> bodyWithState, Action<TSource, ParallelLoopState, long> bodyWithStateAndIndex, Func<TSource, ParallelLoopState, TLocal, TLocal> bodyWithStateAndLocal, Func<TSource, ParallelLoopState, long, TLocal, TLocal> bodyWithEverything, Func<TLocal> localInit, Action<TLocal> localFinally)
        {
            Action<int> action = null;
            Action<int, ParallelLoopState> action2 = null;
            Action<int, ParallelLoopState> action3 = null;
            Func<int, ParallelLoopState, TLocal, TLocal> bodyWithLocal = null;
            Func<int, ParallelLoopState, TLocal, TLocal> func2 = null;
            int lowerBound = array.GetLowerBound(0);
            int toExclusive = array.GetUpperBound(0) + 1;
            if (body != null)
            {
                if (action == null)
                {
                    action = delegate (int i) {
                        body(array[i]);
                    };
                }
                return ForWorker<object>(lowerBound, toExclusive, parallelOptions, action, null, null, null, null);
            }
            if (bodyWithState != null)
            {
                if (action2 == null)
                {
                    action2 = delegate (int i, ParallelLoopState state) {
                        bodyWithState(array[i], state);
                    };
                }
                return ForWorker<object>(lowerBound, toExclusive, parallelOptions, null, action2, null, null, null);
            }
            if (bodyWithStateAndIndex != null)
            {
                if (action3 == null)
                {
                    action3 = delegate (int i, ParallelLoopState state) {
                        bodyWithStateAndIndex(array[i], state, (long) i);
                    };
                }
                return ForWorker<object>(lowerBound, toExclusive, parallelOptions, null, action3, null, null, null);
            }
            if (bodyWithStateAndLocal != null)
            {
                if (bodyWithLocal == null)
                {
                    bodyWithLocal = (i, state, local) => bodyWithStateAndLocal(array[i], state, local);
                }
                return ForWorker<TLocal>(lowerBound, toExclusive, parallelOptions, null, null, bodyWithLocal, localInit, localFinally);
            }
            if (func2 == null)
            {
                func2 = (i, state, local) => bodyWithEverything(array[i], state, (long) i, local);
            }
            return ForWorker<TLocal>(lowerBound, toExclusive, parallelOptions, null, null, func2, localInit, localFinally);
        }

        private static ParallelLoopResult ForEachWorker<TSource, TLocal>(IList<TSource> list, ParallelOptions parallelOptions, Action<TSource> body, Action<TSource, ParallelLoopState> bodyWithState, Action<TSource, ParallelLoopState, long> bodyWithStateAndIndex, Func<TSource, ParallelLoopState, TLocal, TLocal> bodyWithStateAndLocal, Func<TSource, ParallelLoopState, long, TLocal, TLocal> bodyWithEverything, Func<TLocal> localInit, Action<TLocal> localFinally)
        {
            Action<int> action = null;
            Action<int, ParallelLoopState> action2 = null;
            Action<int, ParallelLoopState> action3 = null;
            Func<int, ParallelLoopState, TLocal, TLocal> bodyWithLocal = null;
            Func<int, ParallelLoopState, TLocal, TLocal> func2 = null;
            if (body != null)
            {
                if (action == null)
                {
                    action = delegate (int i) {
                        body(list[i]);
                    };
                }
                return ForWorker<object>(0, list.Count, parallelOptions, action, null, null, null, null);
            }
            if (bodyWithState != null)
            {
                if (action2 == null)
                {
                    action2 = delegate (int i, ParallelLoopState state) {
                        bodyWithState(list[i], state);
                    };
                }
                return ForWorker<object>(0, list.Count, parallelOptions, null, action2, null, null, null);
            }
            if (bodyWithStateAndIndex != null)
            {
                if (action3 == null)
                {
                    action3 = delegate (int i, ParallelLoopState state) {
                        bodyWithStateAndIndex(list[i], state, (long) i);
                    };
                }
                return ForWorker<object>(0, list.Count, parallelOptions, null, action3, null, null, null);
            }
            if (bodyWithStateAndLocal != null)
            {
                if (bodyWithLocal == null)
                {
                    bodyWithLocal = (i, state, local) => bodyWithStateAndLocal(list[i], state, local);
                }
                return ForWorker<TLocal>(0, list.Count, parallelOptions, null, null, bodyWithLocal, localInit, localFinally);
            }
            if (func2 == null)
            {
                func2 = (i, state, local) => bodyWithEverything(list[i], state, (long) i, local);
            }
            return ForWorker<TLocal>(0, list.Count, parallelOptions, null, null, func2, localInit, localFinally);
        }

        private static ParallelLoopResult ForWorker<TLocal>(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Action<int> body, Action<int, ParallelLoopState> bodyWithState, Func<int, ParallelLoopState, TLocal, TLocal> bodyWithLocal, Func<TLocal> localInit, Action<TLocal> localFinally)
        {
            Action<object> callback = null;
            Action action = null;
            ParallelLoopResult result = new ParallelLoopResult();
            if (toExclusive <= fromInclusive)
            {
                result.m_completed = true;
                return result;
            }
            ParallelLoopStateFlags32 sharedPStateFlags = new ParallelLoopStateFlags32();
            TaskCreationOptions none = TaskCreationOptions.None;
            InternalTaskOptions selfReplicating = InternalTaskOptions.SelfReplicating;
            if (parallelOptions.CancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(parallelOptions.CancellationToken);
            }
            int nNumExpectedWorkers = (parallelOptions.EffectiveMaxConcurrencyLevel == -1) ? Environment.ProcessorCount : parallelOptions.EffectiveMaxConcurrencyLevel;
            RangeManager rangeManager = new RangeManager((long) fromInclusive, (long) toExclusive, 1L, nNumExpectedWorkers);
            OperationCanceledException oce = null;
            CancellationTokenRegistration registration = new CancellationTokenRegistration();
            if (parallelOptions.CancellationToken.CanBeCanceled)
            {
                if (callback == null)
                {
                    callback = delegate (object o) {
                        sharedPStateFlags.Cancel();
                        oce = new OperationCanceledException(parallelOptions.CancellationToken);
                    };
                }
                registration = parallelOptions.CancellationToken.InternalRegisterWithoutEC(callback, null);
            }
            int forkJoinContextID = 0;
            Task task = null;
            if (TplEtwProvider.Log.IsEnabled())
            {
                forkJoinContextID = Interlocked.Increment(ref s_forkJoinContextID);
                task = Task.InternalCurrent;
                TplEtwProvider.Log.ParallelLoopBegin((task != null) ? task.m_taskScheduler.Id : TaskScheduler.Current.Id, (task != null) ? task.Id : 0, forkJoinContextID, TplEtwProvider.ForkJoinOperationType.ParallelFor, (long) fromInclusive, (long) toExclusive);
            }
            ParallelForReplicatingTask rootTask = null;
            try
            {
                if (action == null)
                {
                    action = delegate {
                        int num;
                        int num2;
                        Task internalCurrent = Task.InternalCurrent;
                        bool flag = internalCurrent == rootTask;
                        RangeWorker worker = new RangeWorker();
                        object savedStateFromPreviousReplica = internalCurrent.SavedStateFromPreviousReplica;
                        if (savedStateFromPreviousReplica is RangeWorker)
                        {
                            worker = (RangeWorker) savedStateFromPreviousReplica;
                        }
                        else
                        {
                            worker = rangeManager.RegisterNewWorker();
                        }
                        if (worker.FindNewWork32(out num, out num2) && !sharedPStateFlags.ShouldExitLoop(num))
                        {
                            if (TplEtwProvider.Log.IsEnabled())
                            {
                                TplEtwProvider.Log.ParallelFork((internalCurrent != null) ? internalCurrent.m_taskScheduler.Id : TaskScheduler.Current.Id, (internalCurrent != null) ? internalCurrent.Id : 0, forkJoinContextID);
                            }
                            TLocal local = default(TLocal);
                            bool flag2 = false;
                            try
                            {
                                ParallelLoopState32 state = null;
                                if (bodyWithState != null)
                                {
                                    state = new ParallelLoopState32(sharedPStateFlags);
                                }
                                else if (bodyWithLocal != null)
                                {
                                    state = new ParallelLoopState32(sharedPStateFlags);
                                    if (localInit != null)
                                    {
                                        local = localInit();
                                        flag2 = true;
                                    }
                                }
                                LoopTimer timer = new LoopTimer(rootTask.ActiveChildCount);
                            Label_00FF:
                                if (body != null)
                                {
                                    for (int m = num; (m < num2) && ((sharedPStateFlags.LoopStateFlags == ParallelLoopStateFlags.PLS_NONE) || !sharedPStateFlags.ShouldExitLoop()); m++)
                                    {
                                        body(m);
                                    }
                                }
                                else if (bodyWithState != null)
                                {
                                    for (int i = num; (i < num2) && ((sharedPStateFlags.LoopStateFlags == ParallelLoopStateFlags.PLS_NONE) || !sharedPStateFlags.ShouldExitLoop(i)); i++)
                                    {
                                        state.CurrentIteration = i;
                                        bodyWithState(i, state);
                                    }
                                }
                                else
                                {
                                    for (int j = num; (j < num2) && ((sharedPStateFlags.LoopStateFlags == ParallelLoopStateFlags.PLS_NONE) || !sharedPStateFlags.ShouldExitLoop(j)); j++)
                                    {
                                        state.CurrentIteration = j;
                                        local = bodyWithLocal(j, state, local);
                                    }
                                }
                                if (!flag && timer.LimitExceeded())
                                {
                                    internalCurrent.SavedStateForNextReplica = worker;
                                }
                                else if (worker.FindNewWork32(out num, out num2) && ((sharedPStateFlags.LoopStateFlags == ParallelLoopStateFlags.PLS_NONE) || !sharedPStateFlags.ShouldExitLoop(num)))
                                {
                                    goto Label_00FF;
                                }
                            }
                            catch
                            {
                                sharedPStateFlags.SetExceptional();
                                throw;
                            }
                            finally
                            {
                                if ((localFinally != null) && flag2)
                                {
                                    localFinally(local);
                                }
                                if (TplEtwProvider.Log.IsEnabled())
                                {
                                    TplEtwProvider.Log.ParallelJoin((internalCurrent != null) ? internalCurrent.m_taskScheduler.Id : TaskScheduler.Current.Id, (internalCurrent != null) ? internalCurrent.Id : 0, forkJoinContextID);
                                }
                            }
                        }
                    };
                }
                rootTask = new ParallelForReplicatingTask(parallelOptions, action, none, selfReplicating);
                rootTask.RunSynchronously(parallelOptions.EffectiveTaskScheduler);
                rootTask.Wait();
                if (parallelOptions.CancellationToken.CanBeCanceled)
                {
                    registration.Dispose();
                }
                if (oce != null)
                {
                    throw oce;
                }
            }
            catch (AggregateException exception)
            {
                if (parallelOptions.CancellationToken.CanBeCanceled)
                {
                    registration.Dispose();
                }
                ThrowIfReducableToSingleOCE(exception.InnerExceptions, parallelOptions.CancellationToken);
                throw;
            }
            catch (TaskSchedulerException)
            {
                if (parallelOptions.CancellationToken.CanBeCanceled)
                {
                    registration.Dispose();
                }
                throw;
            }
            finally
            {
                int loopStateFlags = sharedPStateFlags.LoopStateFlags;
                result.m_completed = loopStateFlags == ParallelLoopStateFlags.PLS_NONE;
                if ((loopStateFlags & ParallelLoopStateFlags.PLS_BROKEN) != 0)
                {
                    result.m_lowestBreakIteration = new long?((long) sharedPStateFlags.LowestBreakIteration);
                }
                if ((rootTask != null) && rootTask.IsCompleted)
                {
                    rootTask.Dispose();
                }
                if (TplEtwProvider.Log.IsEnabled())
                {
                    int num3 = 0;
                    if (loopStateFlags == ParallelLoopStateFlags.PLS_NONE)
                    {
                        num3 = toExclusive - fromInclusive;
                    }
                    else if ((loopStateFlags & ParallelLoopStateFlags.PLS_BROKEN) != 0)
                    {
                        num3 = sharedPStateFlags.LowestBreakIteration - fromInclusive;
                    }
                    else
                    {
                        num3 = -1;
                    }
                    TplEtwProvider.Log.ParallelLoopEnd((task != null) ? task.m_taskScheduler.Id : TaskScheduler.Current.Id, (task != null) ? task.Id : 0, forkJoinContextID, (long) num3);
                }
            }
            return result;
        }

        private static ParallelLoopResult ForWorker64<TLocal>(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Action<long> body, Action<long, ParallelLoopState> bodyWithState, Func<long, ParallelLoopState, TLocal, TLocal> bodyWithLocal, Func<TLocal> localInit, Action<TLocal> localFinally)
        {
            Action<object> callback = null;
            Action action = null;
            ParallelLoopResult result = new ParallelLoopResult();
            if (toExclusive <= fromInclusive)
            {
                result.m_completed = true;
                return result;
            }
            ParallelLoopStateFlags64 sharedPStateFlags = new ParallelLoopStateFlags64();
            TaskCreationOptions none = TaskCreationOptions.None;
            InternalTaskOptions selfReplicating = InternalTaskOptions.SelfReplicating;
            if (parallelOptions.CancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(parallelOptions.CancellationToken);
            }
            int nNumExpectedWorkers = (parallelOptions.EffectiveMaxConcurrencyLevel == -1) ? Environment.ProcessorCount : parallelOptions.EffectiveMaxConcurrencyLevel;
            RangeManager rangeManager = new RangeManager(fromInclusive, toExclusive, 1L, nNumExpectedWorkers);
            OperationCanceledException oce = null;
            CancellationTokenRegistration registration = new CancellationTokenRegistration();
            if (parallelOptions.CancellationToken.CanBeCanceled)
            {
                if (callback == null)
                {
                    callback = delegate (object o) {
                        sharedPStateFlags.Cancel();
                        oce = new OperationCanceledException(parallelOptions.CancellationToken);
                    };
                }
                registration = parallelOptions.CancellationToken.InternalRegisterWithoutEC(callback, null);
            }
            Task task = null;
            int forkJoinContextID = 0;
            if (TplEtwProvider.Log.IsEnabled())
            {
                forkJoinContextID = Interlocked.Increment(ref s_forkJoinContextID);
                task = Task.InternalCurrent;
                TplEtwProvider.Log.ParallelLoopBegin((task != null) ? task.m_taskScheduler.Id : TaskScheduler.Current.Id, (task != null) ? task.Id : 0, forkJoinContextID, TplEtwProvider.ForkJoinOperationType.ParallelFor, fromInclusive, toExclusive);
            }
            ParallelForReplicatingTask rootTask = null;
            try
            {
                if (action == null)
                {
                    action = delegate {
                        long num;
                        long num2;
                        Task internalCurrent = Task.InternalCurrent;
                        bool flag = internalCurrent == rootTask;
                        RangeWorker worker = new RangeWorker();
                        object savedStateFromPreviousReplica = internalCurrent.SavedStateFromPreviousReplica;
                        if (savedStateFromPreviousReplica is RangeWorker)
                        {
                            worker = (RangeWorker) savedStateFromPreviousReplica;
                        }
                        else
                        {
                            worker = rangeManager.RegisterNewWorker();
                        }
                        if (worker.FindNewWork(out num, out num2) && !sharedPStateFlags.ShouldExitLoop(num))
                        {
                            if (TplEtwProvider.Log.IsEnabled())
                            {
                                TplEtwProvider.Log.ParallelFork((internalCurrent != null) ? internalCurrent.m_taskScheduler.Id : TaskScheduler.Current.Id, (internalCurrent != null) ? internalCurrent.Id : 0, forkJoinContextID);
                            }
                            TLocal local = default(TLocal);
                            bool flag2 = false;
                            try
                            {
                                ParallelLoopState64 state = null;
                                if (bodyWithState != null)
                                {
                                    state = new ParallelLoopState64(sharedPStateFlags);
                                }
                                else if (bodyWithLocal != null)
                                {
                                    state = new ParallelLoopState64(sharedPStateFlags);
                                    if (localInit != null)
                                    {
                                        local = localInit();
                                        flag2 = true;
                                    }
                                }
                                LoopTimer timer = new LoopTimer(rootTask.ActiveChildCount);
                            Label_00FF:
                                if (body != null)
                                {
                                    for (long m = num; (m < num2) && ((sharedPStateFlags.LoopStateFlags == ParallelLoopStateFlags.PLS_NONE) || !sharedPStateFlags.ShouldExitLoop()); m += 1L)
                                    {
                                        body(m);
                                    }
                                }
                                else if (bodyWithState != null)
                                {
                                    for (long i = num; (i < num2) && ((sharedPStateFlags.LoopStateFlags == ParallelLoopStateFlags.PLS_NONE) || !sharedPStateFlags.ShouldExitLoop(i)); i += 1L)
                                    {
                                        state.CurrentIteration = i;
                                        bodyWithState(i, state);
                                    }
                                }
                                else
                                {
                                    for (long j = num; (j < num2) && ((sharedPStateFlags.LoopStateFlags == ParallelLoopStateFlags.PLS_NONE) || !sharedPStateFlags.ShouldExitLoop(j)); j += 1L)
                                    {
                                        state.CurrentIteration = j;
                                        local = bodyWithLocal(j, state, local);
                                    }
                                }
                                if (!flag && timer.LimitExceeded())
                                {
                                    internalCurrent.SavedStateForNextReplica = worker;
                                }
                                else if (worker.FindNewWork(out num, out num2) && ((sharedPStateFlags.LoopStateFlags == ParallelLoopStateFlags.PLS_NONE) || !sharedPStateFlags.ShouldExitLoop(num)))
                                {
                                    goto Label_00FF;
                                }
                            }
                            catch
                            {
                                sharedPStateFlags.SetExceptional();
                                throw;
                            }
                            finally
                            {
                                if ((localFinally != null) && flag2)
                                {
                                    localFinally(local);
                                }
                                if (TplEtwProvider.Log.IsEnabled())
                                {
                                    TplEtwProvider.Log.ParallelJoin((internalCurrent != null) ? internalCurrent.m_taskScheduler.Id : TaskScheduler.Current.Id, (internalCurrent != null) ? internalCurrent.Id : 0, forkJoinContextID);
                                }
                            }
                        }
                    };
                }
                rootTask = new ParallelForReplicatingTask(parallelOptions, action, none, selfReplicating);
                rootTask.RunSynchronously(parallelOptions.EffectiveTaskScheduler);
                rootTask.Wait();
                if (parallelOptions.CancellationToken.CanBeCanceled)
                {
                    registration.Dispose();
                }
                if (oce != null)
                {
                    throw oce;
                }
            }
            catch (AggregateException exception)
            {
                if (parallelOptions.CancellationToken.CanBeCanceled)
                {
                    registration.Dispose();
                }
                ThrowIfReducableToSingleOCE(exception.InnerExceptions, parallelOptions.CancellationToken);
                throw;
            }
            catch (TaskSchedulerException)
            {
                if (parallelOptions.CancellationToken.CanBeCanceled)
                {
                    registration.Dispose();
                }
                throw;
            }
            finally
            {
                int loopStateFlags = sharedPStateFlags.LoopStateFlags;
                result.m_completed = loopStateFlags == ParallelLoopStateFlags.PLS_NONE;
                if ((loopStateFlags & ParallelLoopStateFlags.PLS_BROKEN) != 0)
                {
                    result.m_lowestBreakIteration = new long?(sharedPStateFlags.LowestBreakIteration);
                }
                if ((rootTask != null) && rootTask.IsCompleted)
                {
                    rootTask.Dispose();
                }
                if (TplEtwProvider.Log.IsEnabled())
                {
                    long totalIterations = 0L;
                    if (loopStateFlags == ParallelLoopStateFlags.PLS_NONE)
                    {
                        totalIterations = toExclusive - fromInclusive;
                    }
                    else if ((loopStateFlags & ParallelLoopStateFlags.PLS_BROKEN) != 0)
                    {
                        totalIterations = sharedPStateFlags.LowestBreakIteration - fromInclusive;
                    }
                    else
                    {
                        totalIterations = -1L;
                    }
                    TplEtwProvider.Log.ParallelLoopEnd((task != null) ? task.m_taskScheduler.Id : TaskScheduler.Current.Id, (task != null) ? task.Id : 0, forkJoinContextID, totalIterations);
                }
            }
            return result;
        }

        public static void Invoke(params Action[] actions)
        {
            Invoke(s_defaultParallelOptions, actions);
        }

        public static void Invoke(ParallelOptions parallelOptions, params Action[] actions)
        {
            if (actions == null)
            {
                throw new ArgumentNullException("actions");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            if (parallelOptions.CancellationToken.CanBeCanceled)
            {
                parallelOptions.CancellationToken.ThrowIfSourceDisposed();
            }
            if (parallelOptions.CancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(parallelOptions.CancellationToken);
            }
            Action[] actionsCopy = new Action[actions.Length];
            for (int j = 0; j < actionsCopy.Length; j++)
            {
                actionsCopy[j] = actions[j];
                if (actionsCopy[j] == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Parallel_Invoke_ActionNull"));
                }
            }
            int forkJoinContextID = 0;
            Task internalCurrent = null;
            if (TplEtwProvider.Log.IsEnabled())
            {
                forkJoinContextID = Interlocked.Increment(ref s_forkJoinContextID);
                internalCurrent = Task.InternalCurrent;
                TplEtwProvider.Log.ParallelInvokeBegin((internalCurrent != null) ? internalCurrent.m_taskScheduler.Id : TaskScheduler.Current.Id, (internalCurrent != null) ? internalCurrent.Id : 0, forkJoinContextID, TplEtwProvider.ForkJoinOperationType.ParallelInvoke, actionsCopy.Length);
            }
            if (actionsCopy.Length >= 1)
            {
                try
                {
                    if ((actionsCopy.Length > 10) || ((parallelOptions.MaxDegreeOfParallelism != -1) && (parallelOptions.MaxDegreeOfParallelism < actionsCopy.Length)))
                    {
                        ConcurrentQueue<Exception> exceptionQ = null;
                        try
                        {
                            int actionIndex = 0;
                            ParallelForReplicatingTask task2 = new ParallelForReplicatingTask(parallelOptions, delegate {
                                for (int k = Interlocked.Increment(ref actionIndex); k <= actionsCopy.Length; k = Interlocked.Increment(ref actionIndex))
                                {
                                    try
                                    {
                                        actionsCopy[k - 1]();
                                    }
                                    catch (Exception exception)
                                    {
                                        LazyInitializer.EnsureInitialized<ConcurrentQueue<Exception>>(ref exceptionQ, () => new ConcurrentQueue<Exception>());
                                        exceptionQ.Enqueue(exception);
                                    }
                                    if (parallelOptions.CancellationToken.IsCancellationRequested)
                                    {
                                        throw new OperationCanceledException(parallelOptions.CancellationToken);
                                    }
                                }
                            }, TaskCreationOptions.None, InternalTaskOptions.SelfReplicating);
                            task2.RunSynchronously(parallelOptions.EffectiveTaskScheduler);
                            task2.Wait();
                        }
                        catch (Exception exception)
                        {
                            LazyInitializer.EnsureInitialized<ConcurrentQueue<Exception>>(ref exceptionQ, () => new ConcurrentQueue<Exception>());
                            AggregateException exception2 = exception as AggregateException;
                            if (exception2 != null)
                            {
                                foreach (Exception exception3 in exception2.InnerExceptions)
                                {
                                    exceptionQ.Enqueue(exception3);
                                }
                            }
                            else
                            {
                                exceptionQ.Enqueue(exception);
                            }
                        }
                        if ((exceptionQ != null) && (exceptionQ.Count > 0))
                        {
                            ThrowIfReducableToSingleOCE(exceptionQ, parallelOptions.CancellationToken);
                            throw new AggregateException(exceptionQ);
                        }
                    }
                    else
                    {
                        Task[] tasks = new Task[actionsCopy.Length];
                        if (parallelOptions.CancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException(parallelOptions.CancellationToken);
                        }
                        for (int i = 0; i < tasks.Length; i++)
                        {
                            tasks[i] = Task.Factory.StartNew(actionsCopy[i], parallelOptions.CancellationToken, TaskCreationOptions.None, InternalTaskOptions.None, parallelOptions.EffectiveTaskScheduler);
                        }
                        try
                        {
                            if (tasks.Length <= 4)
                            {
                                Task.FastWaitAll(tasks);
                            }
                            else
                            {
                                Task.WaitAll(tasks);
                            }
                        }
                        catch (AggregateException exception4)
                        {
                            ThrowIfReducableToSingleOCE(exception4.InnerExceptions, parallelOptions.CancellationToken);
                            throw;
                        }
                        finally
                        {
                            for (int m = 0; m < tasks.Length; m++)
                            {
                                if (tasks[m].IsCompleted)
                                {
                                    tasks[m].Dispose();
                                }
                            }
                        }
                    }
                }
                finally
                {
                    if (TplEtwProvider.Log.IsEnabled())
                    {
                        TplEtwProvider.Log.ParallelInvokeEnd((internalCurrent != null) ? internalCurrent.m_taskScheduler.Id : TaskScheduler.Current.Id, (internalCurrent != null) ? internalCurrent.Id : 0, forkJoinContextID);
                    }
                }
            }
        }

        private static ParallelLoopResult PartitionerForEachWorker<TSource, TLocal>(Partitioner<TSource> source, ParallelOptions parallelOptions, Action<TSource> simpleBody, Action<TSource, ParallelLoopState> bodyWithState, Action<TSource, ParallelLoopState, long> bodyWithStateAndIndex, Func<TSource, ParallelLoopState, TLocal, TLocal> bodyWithStateAndLocal, Func<TSource, ParallelLoopState, long, TLocal, TLocal> bodyWithEverything, Func<TLocal> localInit, Action<TLocal> localFinally)
        {
            Action<object> callback = null;
            OrderablePartitioner<TSource> orderedSource = source as OrderablePartitioner<TSource>;
            if (!source.SupportsDynamicPartitions)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Parallel_ForEach_PartitionerNotDynamic"));
            }
            if (parallelOptions.CancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(parallelOptions.CancellationToken);
            }
            int forkJoinContextID = 0;
            Task task = null;
            if (TplEtwProvider.Log.IsEnabled())
            {
                forkJoinContextID = Interlocked.Increment(ref s_forkJoinContextID);
                task = Task.InternalCurrent;
                TplEtwProvider.Log.ParallelLoopBegin((task != null) ? task.m_taskScheduler.Id : TaskScheduler.Current.Id, (task != null) ? task.Id : 0, forkJoinContextID, TplEtwProvider.ForkJoinOperationType.ParallelForEach, 0L, 0L);
            }
            ParallelLoopStateFlags64 sharedPStateFlags = new ParallelLoopStateFlags64();
            ParallelLoopResult result = new ParallelLoopResult();
            OperationCanceledException oce = null;
            CancellationTokenRegistration registration = new CancellationTokenRegistration();
            if (parallelOptions.CancellationToken.CanBeCanceled)
            {
                if (callback == null)
                {
                    callback = delegate (object o) {
                        sharedPStateFlags.Cancel();
                        oce = new OperationCanceledException(parallelOptions.CancellationToken);
                    };
                }
                registration = parallelOptions.CancellationToken.InternalRegisterWithoutEC(callback, null);
            }
            IEnumerable<TSource> partitionerSource = null;
            IEnumerable<KeyValuePair<long, TSource>> orderablePartitionerSource = null;
            if (orderedSource != null)
            {
                orderablePartitionerSource = orderedSource.GetOrderableDynamicPartitions();
                if (orderablePartitionerSource == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Parallel_ForEach_PartitionerReturnedNull"));
                }
            }
            else
            {
                partitionerSource = source.GetDynamicPartitions();
                if (partitionerSource == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Parallel_ForEach_PartitionerReturnedNull"));
                }
            }
            ParallelForReplicatingTask rootTask = null;
            Action action = delegate {
                Task internalCurrent = Task.InternalCurrent;
                if (TplEtwProvider.Log.IsEnabled())
                {
                    TplEtwProvider.Log.ParallelFork((internalCurrent != null) ? internalCurrent.m_taskScheduler.Id : TaskScheduler.Current.Id, (internalCurrent != null) ? internalCurrent.Id : 0, forkJoinContextID);
                }
                TLocal local = default(TLocal);
                bool flag = false;
                try
                {
                    KeyValuePair<long, TSource> pair;
                    IEnumerator<TSource> enumerator;
                    ParallelLoopState64 state = null;
                    if ((bodyWithState != null) || (bodyWithStateAndIndex != null))
                    {
                        state = new ParallelLoopState64(sharedPStateFlags);
                    }
                    else if ((bodyWithStateAndLocal != null) || (bodyWithEverything != null))
                    {
                        state = new ParallelLoopState64(sharedPStateFlags);
                        if (localInit != null)
                        {
                            local = localInit();
                            flag = true;
                        }
                    }
                    bool flag2 = rootTask == internalCurrent;
                    LoopTimer timer = new LoopTimer(rootTask.ActiveChildCount);
                    if (orderedSource == null)
                    {
                        goto Label_01DC;
                    }
                    IEnumerator<KeyValuePair<long, TSource>> savedStateFromPreviousReplica = internalCurrent.SavedStateFromPreviousReplica as IEnumerator<KeyValuePair<long, TSource>>;
                    if (savedStateFromPreviousReplica != null)
                    {
                        goto Label_01CB;
                    }
                    savedStateFromPreviousReplica = orderablePartitionerSource.GetEnumerator();
                    if (savedStateFromPreviousReplica != null)
                    {
                        goto Label_01CB;
                    }
                    throw new InvalidOperationException(Environment.GetResourceString("Parallel_ForEach_NullEnumerator"));
                Label_0104:
                    pair = savedStateFromPreviousReplica.Current;
                    long key = pair.Key;
                    TSource local2 = pair.Value;
                    if (state != null)
                    {
                        state.CurrentIteration = key;
                    }
                    if (simpleBody != null)
                    {
                        simpleBody(local2);
                    }
                    else if (bodyWithState != null)
                    {
                        bodyWithState(local2, state);
                    }
                    else if (bodyWithStateAndIndex != null)
                    {
                        bodyWithStateAndIndex(local2, state, key);
                    }
                    else if (bodyWithStateAndLocal != null)
                    {
                        local = bodyWithStateAndLocal(local2, state, local);
                    }
                    else
                    {
                        local = bodyWithEverything(local2, state, key, local);
                    }
                    if (sharedPStateFlags.ShouldExitLoop(key))
                    {
                        return;
                    }
                    if (!flag2 && timer.LimitExceeded())
                    {
                        internalCurrent.SavedStateForNextReplica = savedStateFromPreviousReplica;
                        return;
                    }
                Label_01CB:
                    if (savedStateFromPreviousReplica.MoveNext())
                    {
                        goto Label_0104;
                    }
                    return;
                Label_01DC:
                    enumerator = internalCurrent.SavedStateFromPreviousReplica as IEnumerator<TSource>;
                    if (enumerator == null)
                    {
                        enumerator = partitionerSource.GetEnumerator();
                        if (enumerator == null)
                        {
                            throw new InvalidOperationException(Environment.GetResourceString("Parallel_ForEach_NullEnumerator"));
                        }
                    }
                    if (state != null)
                    {
                        state.CurrentIteration = 0L;
                    }
                    while (enumerator.MoveNext())
                    {
                        TSource current = enumerator.Current;
                        if (simpleBody != null)
                        {
                            simpleBody(current);
                        }
                        else if (bodyWithState != null)
                        {
                            bodyWithState(current, state);
                        }
                        else if (bodyWithStateAndLocal != null)
                        {
                            local = bodyWithStateAndLocal(current, state, local);
                        }
                        if (sharedPStateFlags.LoopStateFlags != ParallelLoopStateFlags.PLS_NONE)
                        {
                            return;
                        }
                        if (!flag2 && timer.LimitExceeded())
                        {
                            internalCurrent.SavedStateForNextReplica = enumerator;
                            return;
                        }
                    }
                }
                catch
                {
                    sharedPStateFlags.SetExceptional();
                    throw;
                }
                finally
                {
                    if ((localFinally != null) && flag)
                    {
                        localFinally(local);
                    }
                    if (TplEtwProvider.Log.IsEnabled())
                    {
                        TplEtwProvider.Log.ParallelJoin((internalCurrent != null) ? internalCurrent.m_taskScheduler.Id : TaskScheduler.Current.Id, (internalCurrent != null) ? internalCurrent.Id : 0, forkJoinContextID);
                    }
                }
            };
            try
            {
                rootTask = new ParallelForReplicatingTask(parallelOptions, action, TaskCreationOptions.None, InternalTaskOptions.SelfReplicating);
                rootTask.RunSynchronously(parallelOptions.EffectiveTaskScheduler);
                rootTask.Wait();
                if (parallelOptions.CancellationToken.CanBeCanceled)
                {
                    registration.Dispose();
                }
                if (oce != null)
                {
                    throw oce;
                }
            }
            catch (AggregateException exception)
            {
                if (parallelOptions.CancellationToken.CanBeCanceled)
                {
                    registration.Dispose();
                }
                ThrowIfReducableToSingleOCE(exception.InnerExceptions, parallelOptions.CancellationToken);
                throw;
            }
            catch (TaskSchedulerException)
            {
                if (parallelOptions.CancellationToken.CanBeCanceled)
                {
                    registration.Dispose();
                }
                throw;
            }
            finally
            {
                int loopStateFlags = sharedPStateFlags.LoopStateFlags;
                result.m_completed = loopStateFlags == ParallelLoopStateFlags.PLS_NONE;
                if ((loopStateFlags & ParallelLoopStateFlags.PLS_BROKEN) != 0)
                {
                    result.m_lowestBreakIteration = new long?(sharedPStateFlags.LowestBreakIteration);
                }
                if ((rootTask != null) && rootTask.IsCompleted)
                {
                    rootTask.Dispose();
                }
                IDisposable disposable = null;
                if (orderablePartitionerSource != null)
                {
                    disposable = orderablePartitionerSource as IDisposable;
                }
                else
                {
                    disposable = partitionerSource as IDisposable;
                }
                if (disposable != null)
                {
                    disposable.Dispose();
                }
                if (TplEtwProvider.Log.IsEnabled())
                {
                    TplEtwProvider.Log.ParallelLoopEnd((task != null) ? task.m_taskScheduler.Id : TaskScheduler.Current.Id, (task != null) ? task.Id : 0, forkJoinContextID, 0L);
                }
            }
            return result;
        }

        internal static void ThrowIfReducableToSingleOCE(IEnumerable<Exception> excCollection, CancellationToken ct)
        {
            bool flag = false;
            if (ct.IsCancellationRequested)
            {
                foreach (Exception exception in excCollection)
                {
                    flag = true;
                    OperationCanceledException exception2 = exception as OperationCanceledException;
                    if ((exception2 == null) || (exception2.CancellationToken != ct))
                    {
                        return;
                    }
                }
                if (flag)
                {
                    throw new OperationCanceledException(ct);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LoopTimer
        {
            private const int s_BaseNotifyPeriodMS = 100;
            private const int s_NotifyPeriodIncrementMS = 50;
            private int m_timeLimit;
            public LoopTimer(int nWorkerTaskIndex)
            {
                int num = 100 + ((nWorkerTaskIndex % Environment.ProcessorCount) * 50);
                this.m_timeLimit = Environment.TickCount + num;
            }

            public bool LimitExceeded()
            {
                return (Environment.TickCount > this.m_timeLimit);
            }
        }
    }
}

