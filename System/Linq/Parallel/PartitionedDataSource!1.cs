namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class PartitionedDataSource<T> : PartitionedStream<T, int>
    {
        internal PartitionedDataSource(IEnumerable<T> source, int partitionCount, bool useStriping) : base(partitionCount, Util.GetDefaultComparer<int>(), (source is IList<T>) ? OrdinalIndexState.Indexible : OrdinalIndexState.Correct)
        {
            this.InitializePartitions(source, partitionCount, useStriping);
        }

        private void InitializePartitions(IEnumerable<T> source, int partitionCount, bool useStriping)
        {
            ParallelEnumerableWrapper<T> wrapper = source as ParallelEnumerableWrapper<T>;
            if (wrapper != null)
            {
                source = wrapper.WrappedEnumerable;
            }
            IList<T> data = source as IList<T>;
            if (data != null)
            {
                QueryOperatorEnumerator<T, int>[] enumeratorArray = new QueryOperatorEnumerator<T, int>[partitionCount];
                int count = data.Count;
                T[] localArray = source as T[];
                int maxChunkSize = -1;
                if (useStriping)
                {
                    maxChunkSize = Scheduling.GetDefaultChunkSize<T>();
                    if (maxChunkSize < 1)
                    {
                        maxChunkSize = 1;
                    }
                }
                for (int i = 0; i < partitionCount; i++)
                {
                    if (localArray != null)
                    {
                        if (useStriping)
                        {
                            enumeratorArray[i] = new ArrayIndexRangeEnumerator<T>(localArray, partitionCount, i, maxChunkSize);
                        }
                        else
                        {
                            enumeratorArray[i] = new ArrayContiguousIndexRangeEnumerator<T>(localArray, partitionCount, i);
                        }
                    }
                    else if (useStriping)
                    {
                        enumeratorArray[i] = new ListIndexRangeEnumerator<T>(data, partitionCount, i, maxChunkSize);
                    }
                    else
                    {
                        enumeratorArray[i] = new ListContiguousIndexRangeEnumerator<T>(data, partitionCount, i);
                    }
                }
                base.m_partitions = enumeratorArray;
            }
            else
            {
                base.m_partitions = PartitionedDataSource<T>.MakePartitions(source.GetEnumerator(), partitionCount);
            }
        }

        private static QueryOperatorEnumerator<T, int>[] MakePartitions(IEnumerator<T> source, int partitionCount)
        {
            QueryOperatorEnumerator<T, int>[] enumeratorArray = new QueryOperatorEnumerator<T, int>[partitionCount];
            object sourceSyncLock = new object();
            Shared<int> currentIndex = new Shared<int>(0);
            Shared<int> degreeOfParallelism = new Shared<int>(partitionCount);
            Shared<bool> exceptionTracker = new Shared<bool>(false);
            for (int i = 0; i < partitionCount; i++)
            {
                enumeratorArray[i] = new ContiguousChunkLazyEnumerator<T>(source, exceptionTracker, sourceSyncLock, currentIndex, degreeOfParallelism);
            }
            return enumeratorArray;
        }

        internal sealed class ArrayContiguousIndexRangeEnumerator : QueryOperatorEnumerator<T, int>
        {
            private Shared<int> m_currentIndex;
            private readonly T[] m_data;
            private readonly int m_maximumIndex;
            private readonly int m_startIndex;

            internal ArrayContiguousIndexRangeEnumerator(T[] data, int partitionCount, int partitionIndex)
            {
                this.m_data = data;
                int num = data.Length / partitionCount;
                int num2 = data.Length % partitionCount;
                int num3 = (partitionIndex * num) + ((partitionIndex < num2) ? partitionIndex : num2);
                this.m_startIndex = num3 - 1;
                this.m_maximumIndex = (num3 + num) + ((partitionIndex < num2) ? 1 : 0);
            }

            internal override bool MoveNext(ref T currentElement, ref int currentKey)
            {
                if (this.m_currentIndex == null)
                {
                    this.m_currentIndex = new Shared<int>(this.m_startIndex);
                }
                int index = this.m_currentIndex.Value += 1;
                if (index < this.m_maximumIndex)
                {
                    currentKey = index;
                    currentElement = this.m_data[index];
                    return true;
                }
                return false;
            }
        }

        internal sealed class ArrayIndexRangeEnumerator : QueryOperatorEnumerator<T, int>
        {
            private readonly T[] m_data;
            private readonly int m_elementCount;
            private readonly int m_maxChunkSize;
            private Mutables<T> m_mutables;
            private readonly int m_partitionCount;
            private readonly int m_partitionIndex;
            private readonly int m_sectionCount;

            internal ArrayIndexRangeEnumerator(T[] data, int partitionCount, int partitionIndex, int maxChunkSize)
            {
                this.m_data = data;
                this.m_elementCount = data.Length;
                this.m_partitionCount = partitionCount;
                this.m_partitionIndex = partitionIndex;
                this.m_maxChunkSize = maxChunkSize;
                int num = maxChunkSize * partitionCount;
                this.m_sectionCount = (this.m_elementCount / num) + (((this.m_elementCount % num) == 0) ? 0 : 1);
            }

            internal override bool MoveNext(ref T currentElement, ref int currentKey)
            {
                Mutables<T> mutables = this.m_mutables;
                if (mutables == null)
                {
                    mutables = this.m_mutables = new Mutables<T>();
                }
                if ((++mutables.m_currentPositionInChunk >= mutables.m_currentChunkSize) && !this.MoveNextSlowPath())
                {
                    return false;
                }
                currentKey = mutables.m_currentChunkOffset + mutables.m_currentPositionInChunk;
                currentElement = this.m_data[currentKey];
                return true;
            }

            private bool MoveNextSlowPath()
            {
                Mutables<T> mutables = this.m_mutables;
                int num = ++mutables.m_currentSection;
                int num2 = this.m_sectionCount - num;
                if (num2 <= 0)
                {
                    return false;
                }
                int num3 = (num * this.m_partitionCount) * this.m_maxChunkSize;
                mutables.m_currentPositionInChunk = 0;
                if (num2 > 1)
                {
                    mutables.m_currentChunkSize = this.m_maxChunkSize;
                    mutables.m_currentChunkOffset = num3 + (this.m_partitionIndex * this.m_maxChunkSize);
                }
                else
                {
                    int num4 = this.m_elementCount - num3;
                    int num5 = num4 / this.m_partitionCount;
                    int num6 = num4 % this.m_partitionCount;
                    mutables.m_currentChunkSize = num5;
                    if (this.m_partitionIndex < num6)
                    {
                        mutables.m_currentChunkSize++;
                    }
                    if (mutables.m_currentChunkSize == 0)
                    {
                        return false;
                    }
                    mutables.m_currentChunkOffset = (num3 + (this.m_partitionIndex * num5)) + ((this.m_partitionIndex < num6) ? this.m_partitionIndex : num6);
                }
                return true;
            }

            private class Mutables
            {
                internal int m_currentChunkOffset;
                internal int m_currentChunkSize;
                internal int m_currentPositionInChunk;
                internal int m_currentSection;

                internal Mutables()
                {
                    this.m_currentSection = -1;
                }
            }
        }

        private class ContiguousChunkLazyEnumerator : QueryOperatorEnumerator<T, int>
        {
            private const int chunksPerChunkSize = 7;
            private readonly Shared<int> m_activeEnumeratorsCount;
            private readonly Shared<int> m_currentIndex;
            private readonly Shared<bool> m_exceptionTracker;
            private Mutables<T> m_mutables;
            private readonly IEnumerator<T> m_source;
            private readonly object m_sourceSyncLock;

            internal ContiguousChunkLazyEnumerator(IEnumerator<T> source, Shared<bool> exceptionTracker, object sourceSyncLock, Shared<int> currentIndex, Shared<int> degreeOfParallelism)
            {
                this.m_source = source;
                this.m_sourceSyncLock = sourceSyncLock;
                this.m_currentIndex = currentIndex;
                this.m_activeEnumeratorsCount = degreeOfParallelism;
                this.m_exceptionTracker = exceptionTracker;
            }

            protected override void Dispose(bool disposing)
            {
                if (Interlocked.Decrement(ref this.m_activeEnumeratorsCount.Value) == 0)
                {
                    this.m_source.Dispose();
                }
            }

            internal override bool MoveNext(ref T currentElement, ref int currentKey)
            {
                Mutables<T> mutables = this.m_mutables;
                if (mutables == null)
                {
                    mutables = this.m_mutables = new Mutables<T>();
                }
                while (true)
                {
                    T[] chunkBuffer = mutables.m_chunkBuffer;
                    int index = ++mutables.m_currentChunkIndex;
                    if (index < mutables.m_currentChunkSize)
                    {
                        currentElement = chunkBuffer[index];
                        currentKey = mutables.m_chunkBaseIndex + index;
                        return true;
                    }
                    lock (this.m_sourceSyncLock)
                    {
                        int num2 = 0;
                        if (this.m_exceptionTracker.Value)
                        {
                            return false;
                        }
                        try
                        {
                            while ((num2 < mutables.m_nextChunkMaxSize) && this.m_source.MoveNext())
                            {
                                chunkBuffer[num2] = this.m_source.Current;
                                num2++;
                            }
                        }
                        catch
                        {
                            this.m_exceptionTracker.Value = true;
                            throw;
                        }
                        mutables.m_currentChunkSize = num2;
                        if (num2 == 0)
                        {
                            return false;
                        }
                        mutables.m_chunkBaseIndex = this.m_currentIndex.Value;
                        this.m_currentIndex.Value += num2;
                    }
                    if ((mutables.m_nextChunkMaxSize < chunkBuffer.Length) && ((mutables.m_chunkCounter++ & 7) == 7))
                    {
                        mutables.m_nextChunkMaxSize *= 2;
                        if (mutables.m_nextChunkMaxSize > chunkBuffer.Length)
                        {
                            mutables.m_nextChunkMaxSize = chunkBuffer.Length;
                        }
                    }
                    mutables.m_currentChunkIndex = -1;
                }
            }

            private class Mutables
            {
                internal int m_chunkBaseIndex;
                internal readonly T[] m_chunkBuffer;
                internal int m_chunkCounter;
                internal int m_currentChunkIndex;
                internal int m_currentChunkSize;
                internal int m_nextChunkMaxSize;

                internal Mutables()
                {
                    this.m_nextChunkMaxSize = 1;
                    this.m_chunkBuffer = new T[Scheduling.GetDefaultChunkSize<T>()];
                    this.m_currentChunkSize = 0;
                    this.m_currentChunkIndex = -1;
                    this.m_chunkBaseIndex = 0;
                    this.m_chunkCounter = 0;
                }
            }
        }

        internal sealed class ListContiguousIndexRangeEnumerator : QueryOperatorEnumerator<T, int>
        {
            private Shared<int> m_currentIndex;
            private readonly IList<T> m_data;
            private readonly int m_maximumIndex;
            private readonly int m_startIndex;

            internal ListContiguousIndexRangeEnumerator(IList<T> data, int partitionCount, int partitionIndex)
            {
                this.m_data = data;
                int num = data.Count / partitionCount;
                int num2 = data.Count % partitionCount;
                int num3 = (partitionIndex * num) + ((partitionIndex < num2) ? partitionIndex : num2);
                this.m_startIndex = num3 - 1;
                this.m_maximumIndex = (num3 + num) + ((partitionIndex < num2) ? 1 : 0);
            }

            internal override bool MoveNext(ref T currentElement, ref int currentKey)
            {
                if (this.m_currentIndex == null)
                {
                    this.m_currentIndex = new Shared<int>(this.m_startIndex);
                }
                int num = this.m_currentIndex.Value += 1;
                if (num < this.m_maximumIndex)
                {
                    currentKey = num;
                    currentElement = this.m_data[num];
                    return true;
                }
                return false;
            }
        }

        internal sealed class ListIndexRangeEnumerator : QueryOperatorEnumerator<T, int>
        {
            private readonly IList<T> m_data;
            private readonly int m_elementCount;
            private readonly int m_maxChunkSize;
            private Mutables<T> m_mutables;
            private readonly int m_partitionCount;
            private readonly int m_partitionIndex;
            private readonly int m_sectionCount;

            internal ListIndexRangeEnumerator(IList<T> data, int partitionCount, int partitionIndex, int maxChunkSize)
            {
                this.m_data = data;
                this.m_elementCount = data.Count;
                this.m_partitionCount = partitionCount;
                this.m_partitionIndex = partitionIndex;
                this.m_maxChunkSize = maxChunkSize;
                int num = maxChunkSize * partitionCount;
                this.m_sectionCount = (this.m_elementCount / num) + (((this.m_elementCount % num) == 0) ? 0 : 1);
            }

            internal override bool MoveNext(ref T currentElement, ref int currentKey)
            {
                Mutables<T> mutables = this.m_mutables;
                if (mutables == null)
                {
                    mutables = this.m_mutables = new Mutables<T>();
                }
                if ((++mutables.m_currentPositionInChunk >= mutables.m_currentChunkSize) && !this.MoveNextSlowPath())
                {
                    return false;
                }
                currentKey = mutables.m_currentChunkOffset + mutables.m_currentPositionInChunk;
                currentElement = this.m_data[currentKey];
                return true;
            }

            private bool MoveNextSlowPath()
            {
                Mutables<T> mutables = this.m_mutables;
                int num = ++mutables.m_currentSection;
                int num2 = this.m_sectionCount - num;
                if (num2 <= 0)
                {
                    return false;
                }
                int num3 = (num * this.m_partitionCount) * this.m_maxChunkSize;
                mutables.m_currentPositionInChunk = 0;
                if (num2 > 1)
                {
                    mutables.m_currentChunkSize = this.m_maxChunkSize;
                    mutables.m_currentChunkOffset = num3 + (this.m_partitionIndex * this.m_maxChunkSize);
                }
                else
                {
                    int num4 = this.m_elementCount - num3;
                    int num5 = num4 / this.m_partitionCount;
                    int num6 = num4 % this.m_partitionCount;
                    mutables.m_currentChunkSize = num5;
                    if (this.m_partitionIndex < num6)
                    {
                        mutables.m_currentChunkSize++;
                    }
                    if (mutables.m_currentChunkSize == 0)
                    {
                        return false;
                    }
                    mutables.m_currentChunkOffset = (num3 + (this.m_partitionIndex * num5)) + ((this.m_partitionIndex < num6) ? this.m_partitionIndex : num6);
                }
                return true;
            }

            private class Mutables
            {
                internal int m_currentChunkOffset;
                internal int m_currentChunkSize;
                internal int m_currentPositionInChunk;
                internal int m_currentSection;

                internal Mutables()
                {
                    this.m_currentSection = -1;
                }
            }
        }
    }
}

