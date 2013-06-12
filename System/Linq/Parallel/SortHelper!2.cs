namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class SortHelper<TInputOutput, TKey> : SortHelper<TInputOutput>, IDisposable
    {
        private QueryTaskGroupState m_groupState;
        private OrdinalIndexState m_indexState;
        private IComparer<TKey> m_keyComparer;
        private int m_partitionCount;
        private int m_partitionIndex;
        private Barrier[,] m_sharedBarriers;
        private int[][] m_sharedIndices;
        private GrowingArray<TKey>[] m_sharedKeys;
        private TInputOutput[][] m_sharedValues;
        private QueryOperatorEnumerator<TInputOutput, TKey> m_source;

        private SortHelper(QueryOperatorEnumerator<TInputOutput, TKey> source, int partitionCount, int partitionIndex, QueryTaskGroupState groupState, int[][] sharedIndices, OrdinalIndexState indexState, IComparer<TKey> keyComparer, GrowingArray<TKey>[] sharedkeys, TInputOutput[][] sharedValues, Barrier[,] sharedBarriers)
        {
            this.m_source = source;
            this.m_partitionCount = partitionCount;
            this.m_partitionIndex = partitionIndex;
            this.m_groupState = groupState;
            this.m_sharedIndices = sharedIndices;
            this.m_indexState = indexState;
            this.m_keyComparer = keyComparer;
            this.m_sharedKeys = sharedkeys;
            this.m_sharedValues = sharedValues;
            this.m_sharedBarriers = sharedBarriers;
        }

        private void BuildKeysFromSource(ref GrowingArray<TKey> keys, ref List<TInputOutput> values)
        {
            values = new List<TInputOutput>();
            CancellationToken mergedCancellationToken = this.m_groupState.CancellationState.MergedCancellationToken;
            try
            {
                TInputOutput currentElement = default(TInputOutput);
                TKey currentKey = default(TKey);
                bool flag = this.m_source.MoveNext(ref currentElement, ref currentKey);
                if (keys == null)
                {
                    keys = new GrowingArray<TKey>();
                }
                if (flag)
                {
                    int num = 0;
                    do
                    {
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(mergedCancellationToken);
                        }
                        keys.Add(currentKey);
                        values.Add(currentElement);
                    }
                    while (this.m_source.MoveNext(ref currentElement, ref currentKey));
                }
            }
            finally
            {
                this.m_source.Dispose();
            }
        }

        private int ComputePartnerIndex(int phase)
        {
            int num = ((int) 1) << phase;
            return (this.m_partitionIndex + (((this.m_partitionIndex % (num * 2)) == 0) ? num : -num));
        }

        public void Dispose()
        {
            if (this.m_partitionIndex == 0)
            {
                for (int i = 0; i < this.m_sharedBarriers.GetLength(0); i++)
                {
                    for (int j = 0; j < this.m_sharedBarriers.GetLength(1); j++)
                    {
                        Barrier barrier = this.m_sharedBarriers[i, j];
                        if (barrier != null)
                        {
                            barrier.Dispose();
                        }
                    }
                }
            }
        }

        internal static SortHelper<TInputOutput, TKey>[] GenerateSortHelpers(PartitionedStream<TInputOutput, TKey> partitions, QueryTaskGroupState groupState)
        {
            int partitionCount = partitions.PartitionCount;
            SortHelper<TInputOutput, TKey>[] helperArray = new SortHelper<TInputOutput, TKey>[partitionCount];
            int num2 = 1;
            int num3 = 0;
            while (num2 < partitionCount)
            {
                num3++;
                num2 = num2 << 1;
            }
            int[][] sharedIndices = new int[partitionCount][];
            GrowingArray<TKey>[] sharedkeys = new GrowingArray<TKey>[partitionCount];
            TInputOutput[][] sharedValues = new TInputOutput[partitionCount][];
            Barrier[,] sharedBarriers = new Barrier[num3, partitionCount];
            if (partitionCount > 1)
            {
                int num4 = 1;
                for (int j = 0; j < sharedBarriers.GetLength(0); j++)
                {
                    for (int k = 0; k < sharedBarriers.GetLength(1); k++)
                    {
                        if ((k % num4) == 0)
                        {
                            sharedBarriers[j, k] = new Barrier(2);
                        }
                    }
                    num4 *= 2;
                }
            }
            for (int i = 0; i < partitionCount; i++)
            {
                helperArray[i] = new SortHelper<TInputOutput, TKey>(partitions[i], partitionCount, i, groupState, sharedIndices, partitions.OrdinalIndexState, partitions.KeyComparer, sharedkeys, sharedValues, sharedBarriers);
            }
            return helperArray;
        }

        private void MergeSortCooperatively()
        {
            CancellationToken mergedCancellationToken = this.m_groupState.CancellationState.MergedCancellationToken;
            int length = this.m_sharedBarriers.GetLength(0);
            for (int i = 0; i < length; i++)
            {
                bool flag = i == (length - 1);
                int num3 = this.ComputePartnerIndex(i);
                if (num3 < this.m_partitionCount)
                {
                    int[] numArray = this.m_sharedIndices[this.m_partitionIndex];
                    GrowingArray<TKey> array = this.m_sharedKeys[this.m_partitionIndex];
                    TKey[] internalArray = array.InternalArray;
                    TInputOutput[] sourceArray = this.m_sharedValues[this.m_partitionIndex];
                    this.m_sharedBarriers[i, Math.Min(this.m_partitionIndex, num3)].SignalAndWait(mergedCancellationToken);
                    if (this.m_partitionIndex >= num3)
                    {
                        this.m_sharedBarriers[i, num3].SignalAndWait(mergedCancellationToken);
                        int[] numArray4 = this.m_sharedIndices[this.m_partitionIndex];
                        TKey[] localArray6 = this.m_sharedKeys[this.m_partitionIndex].InternalArray;
                        TInputOutput[] localArray7 = this.m_sharedValues[this.m_partitionIndex];
                        int[] numArray5 = this.m_sharedIndices[num3];
                        GrowingArray<TKey> array2 = this.m_sharedKeys[num3];
                        TInputOutput[] localArray8 = this.m_sharedValues[num3];
                        int destinationIndex = localArray7.Length;
                        int num12 = sourceArray.Length;
                        int num13 = destinationIndex + num12;
                        int num14 = (num13 + 1) / 2;
                        int num15 = num13 - 1;
                        int num16 = destinationIndex - 1;
                        int num17 = num12 - 1;
                        while (num15 >= num14)
                        {
                            if ((num15 & 0x3f) == 0)
                            {
                                CancellationState.ThrowIfCanceled(mergedCancellationToken);
                            }
                            if ((num16 >= 0) && ((num17 < 0) || (this.m_keyComparer.Compare(localArray6[numArray4[num16]], internalArray[numArray[num17]]) > 0)))
                            {
                                if (flag)
                                {
                                    localArray8[num15] = localArray7[numArray4[num16]];
                                }
                                else
                                {
                                    numArray5[num15] = numArray4[num16];
                                }
                                num16--;
                            }
                            else
                            {
                                if (flag)
                                {
                                    localArray8[num15] = sourceArray[numArray[num17]];
                                }
                                else
                                {
                                    numArray5[num15] = destinationIndex + numArray[num17];
                                }
                                num17--;
                            }
                            num15--;
                        }
                        if (!flag && (sourceArray.Length > 0))
                        {
                            array2.CopyFrom(internalArray, sourceArray.Length);
                            Array.Copy(sourceArray, 0, localArray8, destinationIndex, sourceArray.Length);
                        }
                        this.m_sharedBarriers[i, num3].SignalAndWait(mergedCancellationToken);
                        return;
                    }
                    int[] numArray2 = this.m_sharedIndices[num3];
                    TKey[] localArray3 = this.m_sharedKeys[num3].InternalArray;
                    TInputOutput[] localArray4 = this.m_sharedValues[num3];
                    this.m_sharedIndices[num3] = numArray;
                    this.m_sharedKeys[num3] = array;
                    this.m_sharedValues[num3] = sourceArray;
                    int num4 = sourceArray.Length;
                    int num5 = localArray4.Length;
                    int num6 = num4 + num5;
                    int[] numArray3 = null;
                    TInputOutput[] destinationArray = new TInputOutput[num6];
                    if (!flag)
                    {
                        numArray3 = new int[num6];
                    }
                    this.m_sharedIndices[this.m_partitionIndex] = numArray3;
                    this.m_sharedKeys[this.m_partitionIndex] = array;
                    this.m_sharedValues[this.m_partitionIndex] = destinationArray;
                    this.m_sharedBarriers[i, this.m_partitionIndex].SignalAndWait(mergedCancellationToken);
                    int num7 = (num6 + 1) / 2;
                    int index = 0;
                    int num9 = 0;
                    int num10 = 0;
                    while (index < num7)
                    {
                        if ((index & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(mergedCancellationToken);
                        }
                        if ((num9 < num4) && ((num10 >= num5) || (this.m_keyComparer.Compare(internalArray[numArray[num9]], localArray3[numArray2[num10]]) <= 0)))
                        {
                            if (flag)
                            {
                                destinationArray[index] = sourceArray[numArray[num9]];
                            }
                            else
                            {
                                numArray3[index] = numArray[num9];
                            }
                            num9++;
                        }
                        else
                        {
                            if (flag)
                            {
                                destinationArray[index] = localArray4[numArray2[num10]];
                            }
                            else
                            {
                                numArray3[index] = num4 + numArray2[num10];
                            }
                            num10++;
                        }
                        index++;
                    }
                    if (!flag && (num4 > 0))
                    {
                        Array.Copy(sourceArray, 0, destinationArray, 0, num4);
                    }
                    this.m_sharedBarriers[i, this.m_partitionIndex].SignalAndWait(mergedCancellationToken);
                }
            }
        }

        private void QuickSort(int left, int right, TKey[] keys, int[] indices, CancellationToken cancelToken)
        {
            if ((right - left) > 0x3f)
            {
                CancellationState.ThrowIfCanceled(cancelToken);
            }
            do
            {
                int index = left;
                int num2 = right;
                int num3 = indices[index + ((num2 - index) >> 1)];
                TKey y = keys[num3];
                do
                {
                    while (this.m_keyComparer.Compare(keys[indices[index]], y) < 0)
                    {
                        index++;
                    }
                    while (this.m_keyComparer.Compare(keys[indices[num2]], y) > 0)
                    {
                        num2--;
                    }
                    if (index > num2)
                    {
                        break;
                    }
                    if (index < num2)
                    {
                        int num4 = indices[index];
                        indices[index] = indices[num2];
                        indices[num2] = num4;
                    }
                    index++;
                    num2--;
                }
                while (index <= num2);
                if ((num2 - left) <= (right - index))
                {
                    if (left < num2)
                    {
                        this.QuickSort(left, num2, keys, indices, cancelToken);
                    }
                    left = index;
                }
                else
                {
                    if (index < right)
                    {
                        this.QuickSort(index, right, keys, indices, cancelToken);
                    }
                    right = num2;
                }
            }
            while (left < right);
        }

        private void QuickSortIndicesInPlace(GrowingArray<TKey> keys, List<TInputOutput> values, OrdinalIndexState ordinalIndexState)
        {
            int[] indices = new int[values.Count];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = i;
            }
            if ((indices.Length > 1) && ordinalIndexState.IsWorseThan(OrdinalIndexState.Increasing))
            {
                this.QuickSort(0, indices.Length - 1, keys.InternalArray, indices, this.m_groupState.CancellationState.MergedCancellationToken);
            }
            if (this.m_partitionCount == 1)
            {
                TInputOutput[] localArray = new TInputOutput[values.Count];
                for (int j = 0; j < indices.Length; j++)
                {
                    localArray[j] = values[indices[j]];
                }
                this.m_sharedValues[this.m_partitionIndex] = localArray;
            }
            else
            {
                this.m_sharedIndices[this.m_partitionIndex] = indices;
                this.m_sharedKeys[this.m_partitionIndex] = keys;
                this.m_sharedValues[this.m_partitionIndex] = new TInputOutput[values.Count];
                values.CopyTo(this.m_sharedValues[this.m_partitionIndex]);
            }
        }

        internal override TInputOutput[] Sort()
        {
            GrowingArray<TKey> keys = null;
            List<TInputOutput> values = null;
            this.BuildKeysFromSource(ref keys, ref values);
            this.QuickSortIndicesInPlace(keys, values, this.m_indexState);
            if (this.m_partitionCount > 1)
            {
                this.MergeSortCooperatively();
            }
            return this.m_sharedValues[this.m_partitionIndex];
        }
    }
}

