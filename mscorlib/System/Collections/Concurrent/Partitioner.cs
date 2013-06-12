namespace System.Collections.Concurrent
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public static class Partitioner
    {
        private const int DEFAULT_BYTES_PER_CHUNK = 0x200;

        public static OrderablePartitioner<TSource> Create<TSource>(IEnumerable<TSource> source)
        {
            return Create<TSource>(source, -1);
        }

        public static OrderablePartitioner<TSource> Create<TSource>(TSource[] array, bool loadBalance)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (loadBalance)
            {
                return new DynamicPartitionerForArray<TSource>(array);
            }
            return new StaticIndexRangePartitionerForArray<TSource>(array);
        }

        public static OrderablePartitioner<Tuple<long, long>> Create(long fromInclusive, long toExclusive)
        {
            int num = 3;
            if (toExclusive <= fromInclusive)
            {
                throw new ArgumentOutOfRangeException("toExclusive");
            }
            long rangeSize = (toExclusive - fromInclusive) / ((long) (Environment.ProcessorCount * num));
            if (rangeSize == 0L)
            {
                rangeSize = 1L;
            }
            return Create<Tuple<long, long>>(CreateRanges(fromInclusive, toExclusive, rangeSize), 1);
        }

        internal static OrderablePartitioner<TSource> Create<TSource>(IEnumerable<TSource> source, int maxChunkSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new DynamicPartitionerForIEnumerable<TSource>(source, maxChunkSize);
        }

        public static OrderablePartitioner<Tuple<int, int>> Create(int fromInclusive, int toExclusive)
        {
            int num = 3;
            if (toExclusive <= fromInclusive)
            {
                throw new ArgumentOutOfRangeException("toExclusive");
            }
            int rangeSize = (toExclusive - fromInclusive) / (Environment.ProcessorCount * num);
            if (rangeSize == 0)
            {
                rangeSize = 1;
            }
            return Create<Tuple<int, int>>(CreateRanges(fromInclusive, toExclusive, rangeSize), 1);
        }

        public static OrderablePartitioner<TSource> Create<TSource>(IList<TSource> list, bool loadBalance)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }
            if (loadBalance)
            {
                return new DynamicPartitionerForIList<TSource>(list);
            }
            return new StaticIndexRangePartitionerForIList<TSource>(list);
        }

        public static OrderablePartitioner<Tuple<int, int>> Create(int fromInclusive, int toExclusive, int rangeSize)
        {
            if (toExclusive <= fromInclusive)
            {
                throw new ArgumentOutOfRangeException("toExclusive");
            }
            if (rangeSize <= 0)
            {
                throw new ArgumentOutOfRangeException("rangeSize");
            }
            return Create<Tuple<int, int>>(CreateRanges(fromInclusive, toExclusive, rangeSize), 1);
        }

        public static OrderablePartitioner<Tuple<long, long>> Create(long fromInclusive, long toExclusive, long rangeSize)
        {
            if (toExclusive <= fromInclusive)
            {
                throw new ArgumentOutOfRangeException("toExclusive");
            }
            if (rangeSize <= 0L)
            {
                throw new ArgumentOutOfRangeException("rangeSize");
            }
            return Create<Tuple<long, long>>(CreateRanges(fromInclusive, toExclusive, rangeSize), 1);
        }

        private static IEnumerable<Tuple<int, int>> CreateRanges(int fromInclusive, int toExclusive, int rangeSize)
        {
            bool iteratorVariable2 = false;
            int iteratorVariable3 = fromInclusive;
            while (true)
            {
                int iteratorVariable1;
                if ((iteratorVariable3 >= toExclusive) || iteratorVariable2)
                {
                    yield break;
                }
                int iteratorVariable0 = iteratorVariable3;
                try
                {
                    iteratorVariable1 = iteratorVariable3 + rangeSize;
                }
                catch (OverflowException)
                {
                    iteratorVariable1 = toExclusive;
                    iteratorVariable2 = true;
                }
                if (iteratorVariable1 > toExclusive)
                {
                    iteratorVariable1 = toExclusive;
                }
                yield return new Tuple<int, int>(iteratorVariable0, iteratorVariable1);
                iteratorVariable3 += rangeSize;
            }
        }

        private static IEnumerable<Tuple<long, long>> CreateRanges(long fromInclusive, long toExclusive, long rangeSize)
        {
            bool iteratorVariable2 = false;
            long iteratorVariable3 = fromInclusive;
            while (true)
            {
                long iteratorVariable1;
                if ((iteratorVariable3 >= toExclusive) || iteratorVariable2)
                {
                    yield break;
                }
                long iteratorVariable0 = iteratorVariable3;
                try
                {
                    iteratorVariable1 = iteratorVariable3 + rangeSize;
                }
                catch (OverflowException)
                {
                    iteratorVariable1 = toExclusive;
                    iteratorVariable2 = true;
                }
                if (iteratorVariable1 > toExclusive)
                {
                    iteratorVariable1 = toExclusive;
                }
                yield return new Tuple<long, long>(iteratorVariable0, iteratorVariable1);
                iteratorVariable3 += rangeSize;
            }
        }

        private static int GetDefaultChunkSize<TSource>()
        {
            if (typeof(TSource).IsValueType)
            {
                if (typeof(TSource).StructLayoutAttribute.Value == LayoutKind.Explicit)
                {
                    return Math.Max(1, 0x200 / Marshal.SizeOf(typeof(TSource)));
                }
                return 0x80;
            }
            return (0x200 / IntPtr.Size);
        }



        private abstract class DynamicPartitionEnumerator_Abstract<TSource, TSourceReader> : IEnumerator<KeyValuePair<long, TSource>>, IDisposable, IEnumerator
        {
            private const int CHUNK_DOUBLING_RATE = 3;
            protected Partitioner.Shared<int> m_currentChunkSize;
            private int m_doublingCountdown;
            protected Partitioner.Shared<int> m_localOffset;
            protected readonly int m_maxChunkSize;
            protected readonly Partitioner.Shared<long> m_sharedIndex;
            protected readonly TSourceReader m_sharedReader;
            protected static int s_defaultMaxChunkSize;

            static DynamicPartitionEnumerator_Abstract()
            {
                Partitioner.DynamicPartitionEnumerator_Abstract<TSource, TSourceReader>.s_defaultMaxChunkSize = Partitioner.GetDefaultChunkSize<TSource>();
            }

            protected DynamicPartitionEnumerator_Abstract(TSourceReader sharedReader, Partitioner.Shared<long> sharedIndex) : this(sharedReader, sharedIndex, -1)
            {
            }

            protected DynamicPartitionEnumerator_Abstract(TSourceReader sharedReader, Partitioner.Shared<long> sharedIndex, int maxChunkSize)
            {
                this.m_sharedReader = sharedReader;
                this.m_sharedIndex = sharedIndex;
                if (maxChunkSize == -1)
                {
                    this.m_maxChunkSize = Partitioner.DynamicPartitionEnumerator_Abstract<TSource, TSourceReader>.s_defaultMaxChunkSize;
                }
                else
                {
                    this.m_maxChunkSize = maxChunkSize;
                }
            }

            public abstract void Dispose();
            protected abstract bool GrabNextChunk(int requestedChunkSize);
            public bool MoveNext()
            {
                int num;
                if (this.m_localOffset == null)
                {
                    this.m_localOffset = new Partitioner.Shared<int>(-1);
                    this.m_currentChunkSize = new Partitioner.Shared<int>(0);
                    this.m_doublingCountdown = 3;
                }
                if (this.m_localOffset.Value < (this.m_currentChunkSize.Value - 1))
                {
                    this.m_localOffset.Value += 1;
                    return true;
                }
                if (this.m_currentChunkSize.Value == 0)
                {
                    num = 1;
                }
                else if (this.m_doublingCountdown > 0)
                {
                    num = this.m_currentChunkSize.Value;
                }
                else
                {
                    num = Math.Min(this.m_currentChunkSize.Value * 2, this.m_maxChunkSize);
                    this.m_doublingCountdown = 3;
                }
                this.m_doublingCountdown--;
                if (this.GrabNextChunk(num))
                {
                    this.m_localOffset.Value = 0;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public abstract KeyValuePair<long, TSource> Current { get; }

            protected abstract bool HasNoElementsLeft { get; set; }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }

        private abstract class DynamicPartitionEnumeratorForIndexRange_Abstract<TSource, TSourceReader> : Partitioner.DynamicPartitionEnumerator_Abstract<TSource, TSourceReader>
        {
            protected int m_startIndex;

            protected DynamicPartitionEnumeratorForIndexRange_Abstract(TSourceReader sharedReader, Partitioner.Shared<long> sharedIndex) : base(sharedReader, sharedIndex)
            {
            }

            public override void Dispose()
            {
            }

            protected override bool GrabNextChunk(int requestedChunkSize)
            {
                while (!this.HasNoElementsLeft)
                {
                    long comparand = base.m_sharedIndex.Value;
                    if (this.HasNoElementsLeft)
                    {
                        return false;
                    }
                    long num2 = Math.Min((long) (this.SourceCount - 1), comparand + requestedChunkSize);
                    if (Interlocked.CompareExchange(ref base.m_sharedIndex.Value, num2, comparand) == comparand)
                    {
                        base.m_currentChunkSize.Value = (int) (num2 - comparand);
                        base.m_localOffset.Value = -1;
                        this.m_startIndex = (int) (comparand + 1L);
                        return true;
                    }
                }
                return false;
            }

            protected override bool HasNoElementsLeft
            {
                get
                {
                    return (base.m_sharedIndex.Value >= (this.SourceCount - 1));
                }
                set
                {
                }
            }

            protected abstract int SourceCount { get; }
        }

        private class DynamicPartitionerForArray<TSource> : Partitioner.DynamicPartitionerForIndexRange_Abstract<TSource, TSource[]>
        {
            internal DynamicPartitionerForArray(TSource[] source) : base(source)
            {
            }

            protected override IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions_Factory(TSource[] m_data)
            {
                return new InternalPartitionEnumerable<TSource>(m_data);
            }

            private class InternalPartitionEnumerable : IEnumerable<KeyValuePair<long, TSource>>, IEnumerable
            {
                private Partitioner.Shared<long> m_sharedIndex;
                private readonly TSource[] m_sharedReader;

                internal InternalPartitionEnumerable(TSource[] sharedReader)
                {
                    this.m_sharedReader = sharedReader;
                    this.m_sharedIndex = new Partitioner.Shared<long>(-1L);
                }

                public IEnumerator<KeyValuePair<long, TSource>> GetEnumerator()
                {
                    return new Partitioner.DynamicPartitionerForArray<TSource>.InternalPartitionEnumerator(this.m_sharedReader, this.m_sharedIndex);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }
            }

            private class InternalPartitionEnumerator : Partitioner.DynamicPartitionEnumeratorForIndexRange_Abstract<TSource, TSource[]>
            {
                internal InternalPartitionEnumerator(TSource[] sharedReader, Partitioner.Shared<long> sharedIndex) : base(sharedReader, sharedIndex)
                {
                }

                public override KeyValuePair<long, TSource> Current
                {
                    get
                    {
                        if (base.m_currentChunkSize == null)
                        {
                            throw new InvalidOperationException(Environment.GetResourceString("PartitionerStatic_CurrentCalledBeforeMoveNext"));
                        }
                        return new KeyValuePair<long, TSource>((long) (base.m_startIndex + base.m_localOffset.Value), base.m_sharedReader[base.m_startIndex + base.m_localOffset.Value]);
                    }
                }

                protected override int SourceCount
                {
                    get
                    {
                        return base.m_sharedReader.Length;
                    }
                }
            }
        }

        private class DynamicPartitionerForIEnumerable<TSource> : OrderablePartitioner<TSource>
        {
            private int m_maxChunkSize;
            private IEnumerable<TSource> m_source;

            internal DynamicPartitionerForIEnumerable(IEnumerable<TSource> source, int maxChunkSize) : base(true, false, true)
            {
                this.m_source = source;
                this.m_maxChunkSize = maxChunkSize;
            }

            public override IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions()
            {
                return new InternalPartitionEnumerable<TSource>(this.m_source.GetEnumerator(), this.m_maxChunkSize);
            }

            public override IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(int partitionCount)
            {
                if (partitionCount <= 0)
                {
                    throw new ArgumentOutOfRangeException("partitionCount");
                }
                IEnumerator<KeyValuePair<long, TSource>>[] enumeratorArray = new IEnumerator<KeyValuePair<long, TSource>>[partitionCount];
                IEnumerable<KeyValuePair<long, TSource>> enumerable = new InternalPartitionEnumerable<TSource>(this.m_source.GetEnumerator(), this.m_maxChunkSize);
                for (int i = 0; i < partitionCount; i++)
                {
                    enumeratorArray[i] = enumerable.GetEnumerator();
                }
                return enumeratorArray;
            }

            public override bool SupportsDynamicPartitions
            {
                get
                {
                    return true;
                }
            }

            private class InternalPartitionEnumerable : IEnumerable<KeyValuePair<long, TSource>>, IEnumerable, IDisposable
            {
                private Partitioner.Shared<int> m_activePartitionCount;
                private bool m_disposed;
                private Partitioner.Shared<bool> m_hasNoElementsLeft;
                private readonly int m_maxChunkSize;
                private Partitioner.Shared<long> m_sharedIndex;
                private object m_sharedLock;
                private readonly IEnumerator<TSource> m_sharedReader;

                internal InternalPartitionEnumerable(IEnumerator<TSource> sharedReader, int maxChunkSize)
                {
                    this.m_sharedReader = sharedReader;
                    this.m_sharedIndex = new Partitioner.Shared<long>(-1L);
                    this.m_hasNoElementsLeft = new Partitioner.Shared<bool>(false);
                    this.m_sharedLock = new object();
                    this.m_activePartitionCount = new Partitioner.Shared<int>(0);
                    this.m_maxChunkSize = maxChunkSize;
                }

                public void Dispose()
                {
                    if (!this.m_disposed)
                    {
                        this.m_disposed = true;
                        this.m_sharedReader.Dispose();
                    }
                }

                public IEnumerator<KeyValuePair<long, TSource>> GetEnumerator()
                {
                    if (this.m_disposed)
                    {
                        throw new ObjectDisposedException(Environment.GetResourceString("PartitionerStatic_CanNotCallGetEnumeratorAfterSourceHasBeenDisposed"));
                    }
                    return new Partitioner.DynamicPartitionerForIEnumerable<TSource>.InternalPartitionEnumerator(this.m_sharedReader, this.m_sharedIndex, this.m_hasNoElementsLeft, this.m_sharedLock, this.m_activePartitionCount, (Partitioner.DynamicPartitionerForIEnumerable<TSource>.InternalPartitionEnumerable) this, this.m_maxChunkSize);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }
            }

            private class InternalPartitionEnumerator : Partitioner.DynamicPartitionEnumerator_Abstract<TSource, IEnumerator<TSource>>
            {
                private readonly Partitioner.Shared<int> m_activePartitionCount;
                private Partitioner.DynamicPartitionerForIEnumerable<TSource>.InternalPartitionEnumerable m_enumerable;
                private readonly Partitioner.Shared<bool> m_hasNoElementsLeft;
                private KeyValuePair<long, TSource>[] m_localList;
                private readonly object m_sharedLock;

                internal InternalPartitionEnumerator(IEnumerator<TSource> sharedReader, Partitioner.Shared<long> sharedIndex, Partitioner.Shared<bool> hasNoElementsLeft, object sharedLock, Partitioner.Shared<int> activePartitionCount, Partitioner.DynamicPartitionerForIEnumerable<TSource>.InternalPartitionEnumerable enumerable, int maxChunkSize) : base(sharedReader, sharedIndex, maxChunkSize)
                {
                    this.m_hasNoElementsLeft = hasNoElementsLeft;
                    this.m_sharedLock = sharedLock;
                    this.m_enumerable = enumerable;
                    this.m_activePartitionCount = activePartitionCount;
                    Interlocked.Increment(ref this.m_activePartitionCount.Value);
                }

                public override void Dispose()
                {
                    if (Interlocked.Decrement(ref this.m_activePartitionCount.Value) == 0)
                    {
                        this.m_enumerable.Dispose();
                    }
                }

                protected override bool GrabNextChunk(int requestedChunkSize)
                {
                    bool flag2;
                    if (this.HasNoElementsLeft)
                    {
                        return false;
                    }
                    lock (this.m_sharedLock)
                    {
                        if (this.HasNoElementsLeft)
                        {
                            flag2 = false;
                        }
                        else
                        {
                            try
                            {
                                int index = 0;
                                while (index < requestedChunkSize)
                                {
                                    if (base.m_sharedReader.MoveNext())
                                    {
                                        if (this.m_localList == null)
                                        {
                                            this.m_localList = new KeyValuePair<long, TSource>[base.m_maxChunkSize];
                                        }
                                        base.m_sharedIndex.Value += 1L;
                                        this.m_localList[index] = new KeyValuePair<long, TSource>(base.m_sharedIndex.Value, base.m_sharedReader.Current);
                                    }
                                    else
                                    {
                                        this.HasNoElementsLeft = true;
                                        break;
                                    }
                                    index++;
                                }
                                if (index > 0)
                                {
                                    base.m_currentChunkSize.Value = index;
                                    return true;
                                }
                                flag2 = false;
                            }
                            catch
                            {
                                this.HasNoElementsLeft = true;
                                throw;
                            }
                        }
                    }
                    return flag2;
                }

                public override KeyValuePair<long, TSource> Current
                {
                    get
                    {
                        if (base.m_currentChunkSize == null)
                        {
                            throw new InvalidOperationException(Environment.GetResourceString("PartitionerStatic_CurrentCalledBeforeMoveNext"));
                        }
                        return this.m_localList[base.m_localOffset.Value];
                    }
                }

                protected override bool HasNoElementsLeft
                {
                    get
                    {
                        return this.m_hasNoElementsLeft.Value;
                    }
                    set
                    {
                        this.m_hasNoElementsLeft.Value = true;
                    }
                }
            }
        }

        private class DynamicPartitionerForIList<TSource> : Partitioner.DynamicPartitionerForIndexRange_Abstract<TSource, IList<TSource>>
        {
            internal DynamicPartitionerForIList(IList<TSource> source) : base(source)
            {
            }

            protected override IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions_Factory(IList<TSource> m_data)
            {
                return new InternalPartitionEnumerable<TSource>(m_data);
            }

            private class InternalPartitionEnumerable : IEnumerable<KeyValuePair<long, TSource>>, IEnumerable
            {
                private Partitioner.Shared<long> m_sharedIndex;
                private readonly IList<TSource> m_sharedReader;

                internal InternalPartitionEnumerable(IList<TSource> sharedReader)
                {
                    this.m_sharedReader = sharedReader;
                    this.m_sharedIndex = new Partitioner.Shared<long>(-1L);
                }

                public IEnumerator<KeyValuePair<long, TSource>> GetEnumerator()
                {
                    return new Partitioner.DynamicPartitionerForIList<TSource>.InternalPartitionEnumerator(this.m_sharedReader, this.m_sharedIndex);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }
            }

            private class InternalPartitionEnumerator : Partitioner.DynamicPartitionEnumeratorForIndexRange_Abstract<TSource, IList<TSource>>
            {
                internal InternalPartitionEnumerator(IList<TSource> sharedReader, Partitioner.Shared<long> sharedIndex) : base(sharedReader, sharedIndex)
                {
                }

                public override KeyValuePair<long, TSource> Current
                {
                    get
                    {
                        if (base.m_currentChunkSize == null)
                        {
                            throw new InvalidOperationException(Environment.GetResourceString("PartitionerStatic_CurrentCalledBeforeMoveNext"));
                        }
                        return new KeyValuePair<long, TSource>((long) (base.m_startIndex + base.m_localOffset.Value), base.m_sharedReader[base.m_startIndex + base.m_localOffset.Value]);
                    }
                }

                protected override int SourceCount
                {
                    get
                    {
                        return base.m_sharedReader.Count;
                    }
                }
            }
        }

        private abstract class DynamicPartitionerForIndexRange_Abstract<TSource, TCollection> : OrderablePartitioner<TSource>
        {
            private TCollection m_data;

            protected DynamicPartitionerForIndexRange_Abstract(TCollection data) : base(true, false, true)
            {
                this.m_data = data;
            }

            public override IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions()
            {
                return this.GetOrderableDynamicPartitions_Factory(this.m_data);
            }

            protected abstract IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions_Factory(TCollection data);
            public override IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(int partitionCount)
            {
                if (partitionCount <= 0)
                {
                    throw new ArgumentOutOfRangeException("partitionCount");
                }
                IEnumerator<KeyValuePair<long, TSource>>[] enumeratorArray = new IEnumerator<KeyValuePair<long, TSource>>[partitionCount];
                IEnumerable<KeyValuePair<long, TSource>> enumerable = this.GetOrderableDynamicPartitions_Factory(this.m_data);
                for (int i = 0; i < partitionCount; i++)
                {
                    enumeratorArray[i] = enumerable.GetEnumerator();
                }
                return enumeratorArray;
            }

            public override bool SupportsDynamicPartitions
            {
                get
                {
                    return true;
                }
            }
        }

        private class Shared<TSource>
        {
            internal TSource Value;

            internal Shared(TSource value)
            {
                this.Value = value;
            }
        }

        private abstract class StaticIndexRangePartition<TSource> : IEnumerator<KeyValuePair<long, TSource>>, IDisposable, IEnumerator
        {
            protected readonly int m_endIndex;
            protected volatile int m_offset;
            protected readonly int m_startIndex;

            protected StaticIndexRangePartition(int startIndex, int endIndex)
            {
                this.m_startIndex = startIndex;
                this.m_endIndex = endIndex;
                this.m_offset = startIndex - 1;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (this.m_offset < this.m_endIndex)
                {
                    this.m_offset++;
                    return true;
                }
                this.m_offset = this.m_endIndex + 1;
                return false;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public abstract KeyValuePair<long, TSource> Current { get; }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }

        private abstract class StaticIndexRangePartitioner<TSource, TCollection> : OrderablePartitioner<TSource>
        {
            protected StaticIndexRangePartitioner() : base(true, true, true)
            {
            }

            protected abstract IEnumerator<KeyValuePair<long, TSource>> CreatePartition(int startIndex, int endIndex);
            public override IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(int partitionCount)
            {
                int num2;
                if (partitionCount <= 0)
                {
                    throw new ArgumentOutOfRangeException("partitionCount");
                }
                int num = Math.DivRem(this.SourceCount, partitionCount, out num2);
                IEnumerator<KeyValuePair<long, TSource>>[] enumeratorArray = new IEnumerator<KeyValuePair<long, TSource>>[partitionCount];
                int endIndex = -1;
                for (int i = 0; i < partitionCount; i++)
                {
                    int startIndex = endIndex + 1;
                    if (i < num2)
                    {
                        endIndex = startIndex + num;
                    }
                    else
                    {
                        endIndex = (startIndex + num) - 1;
                    }
                    enumeratorArray[i] = this.CreatePartition(startIndex, endIndex);
                }
                return enumeratorArray;
            }

            protected abstract int SourceCount { get; }
        }

        private class StaticIndexRangePartitionerForArray<TSource> : Partitioner.StaticIndexRangePartitioner<TSource, TSource[]>
        {
            private TSource[] m_array;

            internal StaticIndexRangePartitionerForArray(TSource[] array)
            {
                this.m_array = array;
            }

            protected override IEnumerator<KeyValuePair<long, TSource>> CreatePartition(int startIndex, int endIndex)
            {
                return new Partitioner.StaticIndexRangePartitionForArray<TSource>(this.m_array, startIndex, endIndex);
            }

            protected override int SourceCount
            {
                get
                {
                    return this.m_array.Length;
                }
            }
        }

        private class StaticIndexRangePartitionerForIList<TSource> : Partitioner.StaticIndexRangePartitioner<TSource, IList<TSource>>
        {
            private IList<TSource> m_list;

            internal StaticIndexRangePartitionerForIList(IList<TSource> list)
            {
                this.m_list = list;
            }

            protected override IEnumerator<KeyValuePair<long, TSource>> CreatePartition(int startIndex, int endIndex)
            {
                return new Partitioner.StaticIndexRangePartitionForIList<TSource>(this.m_list, startIndex, endIndex);
            }

            protected override int SourceCount
            {
                get
                {
                    return this.m_list.Count;
                }
            }
        }

        private class StaticIndexRangePartitionForArray<TSource> : Partitioner.StaticIndexRangePartition<TSource>
        {
            private volatile TSource[] m_array;

            internal StaticIndexRangePartitionForArray(TSource[] array, int startIndex, int endIndex) : base(startIndex, endIndex)
            {
                this.m_array = array;
            }

            public override KeyValuePair<long, TSource> Current
            {
                get
                {
                    if (base.m_offset < base.m_startIndex)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("PartitionerStatic_CurrentCalledBeforeMoveNext"));
                    }
                    return new KeyValuePair<long, TSource>((long) base.m_offset, this.m_array[base.m_offset]);
                }
            }
        }

        private class StaticIndexRangePartitionForIList<TSource> : Partitioner.StaticIndexRangePartition<TSource>
        {
            private volatile IList<TSource> m_list;

            internal StaticIndexRangePartitionForIList(IList<TSource> list, int startIndex, int endIndex) : base(startIndex, endIndex)
            {
                this.m_list = list;
            }

            public override KeyValuePair<long, TSource> Current
            {
                get
                {
                    if (base.m_offset < base.m_startIndex)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("PartitionerStatic_CurrentCalledBeforeMoveNext"));
                    }
                    return new KeyValuePair<long, TSource>((long) base.m_offset, this.m_list[base.m_offset]);
                }
            }
        }
    }
}

