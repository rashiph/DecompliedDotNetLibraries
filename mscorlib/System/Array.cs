namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, ComVisible(true)]
    public abstract class Array : ICloneable, IList, ICollection, IEnumerable, IStructuralComparable, IStructuralEquatable
    {
        internal Array()
        {
        }

        public static ReadOnlyCollection<T> AsReadOnly<T>(T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            return new ReadOnlyCollection<T>(array);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int BinarySearch(Array array, object value)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int lowerBound = array.GetLowerBound(0);
            return BinarySearch(array, lowerBound, array.Length, value, null);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int BinarySearch<T>(T[] array, T value)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            return BinarySearch<T>(array, 0, array.Length, value, null);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int BinarySearch<T>(T[] array, T value, IComparer<T> comparer)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            return BinarySearch<T>(array, 0, array.Length, value, comparer);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int BinarySearch(Array array, object value, IComparer comparer)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int lowerBound = array.GetLowerBound(0);
            return BinarySearch(array, lowerBound, array.Length, value, comparer);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int BinarySearch(Array array, int index, int length, object value)
        {
            return BinarySearch(array, index, length, value, null);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int BinarySearch<T>(T[] array, int index, int length, T value)
        {
            return BinarySearch<T>(array, index, length, value, null);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecuritySafeCritical]
        public static int BinarySearch(Array array, int index, int length, object value, IComparer comparer)
        {
            int num2;
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int lowerBound = array.GetLowerBound(0);
            if ((index < lowerBound) || (length < 0))
            {
                throw new ArgumentOutOfRangeException((index < lowerBound) ? "index" : "length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - (index - lowerBound)) < length)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (array.Rank != 1)
            {
                throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
            }
            if (comparer == null)
            {
                comparer = Comparer.Default;
            }
            if ((comparer == Comparer.Default) && TrySZBinarySearch(array, index, length, value, out num2))
            {
                return num2;
            }
            int low = index;
            int hi = (index + length) - 1;
            object[] objArray = array as object[];
            if (objArray == null)
            {
                while (low <= hi)
                {
                    int num8;
                    int median = GetMedian(low, hi);
                    try
                    {
                        num8 = comparer.Compare(array.GetValue(median), value);
                    }
                    catch (Exception exception2)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), exception2);
                    }
                    if (num8 == 0)
                    {
                        return median;
                    }
                    if (num8 < 0)
                    {
                        low = median + 1;
                    }
                    else
                    {
                        hi = median - 1;
                    }
                }
            }
            else
            {
                while (low <= hi)
                {
                    int num6;
                    int num5 = GetMedian(low, hi);
                    try
                    {
                        num6 = comparer.Compare(objArray[num5], value);
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), exception);
                    }
                    if (num6 == 0)
                    {
                        return num5;
                    }
                    if (num6 < 0)
                    {
                        low = num5 + 1;
                    }
                    else
                    {
                        hi = num5 - 1;
                    }
                }
            }
            return ~low;
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int BinarySearch<T>(T[] array, int index, int length, T value, IComparer<T> comparer)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if ((index < 0) || (length < 0))
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - index) < length)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            return ArraySortHelper<T>.Default.BinarySearch(array, index, length, value, comparer);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static extern void Clear(Array array, int index, int length);
        [SecuritySafeCritical]
        public object Clone()
        {
            return base.MemberwiseClone();
        }

        internal static int CombineHashCodes(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        public static void ConstrainedCopy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
        {
            Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length, true);
        }

        public static TOutput[] ConvertAll<TInput, TOutput>(TInput[] array, Converter<TInput, TOutput> converter)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }
            TOutput[] localArray = new TOutput[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                localArray[i] = converter(array[i]);
            }
            return localArray;
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail), SecuritySafeCritical]
        public static void Copy(Array sourceArray, Array destinationArray, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException("sourceArray");
            }
            if (destinationArray == null)
            {
                throw new ArgumentNullException("destinationArray");
            }
            Copy(sourceArray, sourceArray.GetLowerBound(0), destinationArray, destinationArray.GetLowerBound(0), length, false);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Copy(Array sourceArray, Array destinationArray, long length)
        {
            if ((length > 0x7fffffffL) || (length < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            Copy(sourceArray, destinationArray, (int) length);
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
        {
            Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length, false);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Copy(Array sourceArray, long sourceIndex, Array destinationArray, long destinationIndex, long length)
        {
            if ((sourceIndex > 0x7fffffffL) || (sourceIndex < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("sourceIndex", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            if ((destinationIndex > 0x7fffffffL) || (destinationIndex < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("destinationIndex", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            if ((length > 0x7fffffffL) || (length < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            Copy(sourceArray, (int) sourceIndex, destinationArray, (int) destinationIndex, (int) length);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        internal static extern void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length, bool reliable);
        public void CopyTo(Array array, int index)
        {
            if ((array != null) && (array.Rank != 1))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
            }
            Copy(this, this.GetLowerBound(0), array, index, this.Length);
        }

        [ComVisible(false)]
        public void CopyTo(Array array, long index)
        {
            if ((index > 0x7fffffffL) || (index < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            this.CopyTo(array, (int) index);
        }

        [SecuritySafeCritical]
        public static unsafe Array CreateInstance(Type elementType, int length)
        {
            if (elementType == null)
            {
                throw new ArgumentNullException("elementType");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            RuntimeType underlyingSystemType = elementType.UnderlyingSystemType as RuntimeType;
            if (underlyingSystemType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "elementType");
            }
            return InternalCreate((void*) underlyingSystemType.TypeHandle.Value, 1, &length, null);
        }

        [SecuritySafeCritical]
        public static unsafe Array CreateInstance(Type elementType, params int[] lengths)
        {
            if (elementType == null)
            {
                throw new ArgumentNullException("elementType");
            }
            if (lengths == null)
            {
                throw new ArgumentNullException("lengths");
            }
            if (lengths.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_NeedAtLeast1Rank"));
            }
            RuntimeType underlyingSystemType = elementType.UnderlyingSystemType as RuntimeType;
            if (underlyingSystemType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "elementType");
            }
            for (int i = 0; i < lengths.Length; i++)
            {
                if (lengths[i] < 0)
                {
                    throw new ArgumentOutOfRangeException("lengths[" + i + ']', Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
                }
            }
            fixed (int* numRef = lengths)
            {
                return InternalCreate((void*) underlyingSystemType.TypeHandle.Value, lengths.Length, numRef, null);
            }
        }

        public static Array CreateInstance(Type elementType, params long[] lengths)
        {
            if (lengths == null)
            {
                throw new ArgumentNullException("lengths");
            }
            if (lengths.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_NeedAtLeast1Rank"));
            }
            int[] numArray = new int[lengths.Length];
            for (int i = 0; i < lengths.Length; i++)
            {
                long num2 = lengths[i];
                if ((num2 > 0x7fffffffL) || (num2 < -2147483648L))
                {
                    throw new ArgumentOutOfRangeException("len", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
                }
                numArray[i] = (int) num2;
            }
            return CreateInstance(elementType, numArray);
        }

        [SecuritySafeCritical]
        public static unsafe Array CreateInstance(Type elementType, int length1, int length2)
        {
            if (elementType == null)
            {
                throw new ArgumentNullException("elementType");
            }
            if ((length1 < 0) || (length2 < 0))
            {
                throw new ArgumentOutOfRangeException((length1 < 0) ? "length1" : "length2", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            RuntimeType underlyingSystemType = elementType.UnderlyingSystemType as RuntimeType;
            if (underlyingSystemType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "elementType");
            }
            int* pLengths = (int*) stackalloc byte[(((IntPtr) 2) * 4)];
            pLengths[0] = length1;
            pLengths[1] = length2;
            return InternalCreate((void*) underlyingSystemType.TypeHandle.Value, 2, pLengths, null);
        }

        [SecuritySafeCritical]
        public static unsafe Array CreateInstance(Type elementType, int[] lengths, int[] lowerBounds)
        {
            if (elementType == null)
            {
                throw new ArgumentNullException("elementType");
            }
            if (lengths == null)
            {
                throw new ArgumentNullException("lengths");
            }
            if (lowerBounds == null)
            {
                throw new ArgumentNullException("lowerBounds");
            }
            if (lengths.Length != lowerBounds.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_RanksAndBounds"));
            }
            if (lengths.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_NeedAtLeast1Rank"));
            }
            RuntimeType underlyingSystemType = elementType.UnderlyingSystemType as RuntimeType;
            if (underlyingSystemType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "elementType");
            }
            for (int i = 0; i < lengths.Length; i++)
            {
                if (lengths[i] < 0)
                {
                    throw new ArgumentOutOfRangeException("lengths[" + i + ']', Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
                }
            }
            fixed (int* numRef = lengths)
            {
                fixed (int* numRef2 = lowerBounds)
                {
                    return InternalCreate((void*) underlyingSystemType.TypeHandle.Value, lengths.Length, numRef, numRef2);
                }
            }
        }

        [SecuritySafeCritical]
        public static unsafe Array CreateInstance(Type elementType, int length1, int length2, int length3)
        {
            if (elementType == null)
            {
                throw new ArgumentNullException("elementType");
            }
            if (length1 < 0)
            {
                throw new ArgumentOutOfRangeException("length1", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (length2 < 0)
            {
                throw new ArgumentOutOfRangeException("length2", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (length3 < 0)
            {
                throw new ArgumentOutOfRangeException("length3", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            RuntimeType underlyingSystemType = elementType.UnderlyingSystemType as RuntimeType;
            if (underlyingSystemType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "elementType");
            }
            int* pLengths = (int*) stackalloc byte[(((IntPtr) 3) * 4)];
            pLengths[0] = length1;
            pLengths[1] = length2;
            pLengths[2] = length3;
            return InternalCreate((void*) underlyingSystemType.TypeHandle.Value, 3, pLengths, null);
        }

        public static bool Exists<T>(T[] array, Predicate<T> match)
        {
            return (FindIndex<T>(array, match) != -1);
        }

        public static T Find<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            for (int i = 0; i < array.Length; i++)
            {
                if (match(array[i]))
                {
                    return array[i];
                }
            }
            return default(T);
        }

        public static T[] FindAll<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            List<T> list = new List<T>();
            for (int i = 0; i < array.Length; i++)
            {
                if (match(array[i]))
                {
                    list.Add(array[i]);
                }
            }
            return list.ToArray();
        }

        public static int FindIndex<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            return FindIndex<T>(array, 0, array.Length, match);
        }

        public static int FindIndex<T>(T[] array, int startIndex, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            return FindIndex<T>(array, startIndex, array.Length - startIndex, match);
        }

        public static int FindIndex<T>(T[] array, int startIndex, int count, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if ((startIndex < 0) || (startIndex > array.Length))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((count < 0) || (startIndex > (array.Length - count)))
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            int num = startIndex + count;
            for (int i = startIndex; i < num; i++)
            {
                if (match(array[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static T FindLast<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            for (int i = array.Length - 1; i >= 0; i--)
            {
                if (match(array[i]))
                {
                    return array[i];
                }
            }
            return default(T);
        }

        public static int FindLastIndex<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            return FindLastIndex<T>(array, array.Length - 1, array.Length, match);
        }

        public static int FindLastIndex<T>(T[] array, int startIndex, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            return FindLastIndex<T>(array, startIndex, startIndex + 1, match);
        }

        public static int FindLastIndex<T>(T[] array, int startIndex, int count, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            if (array.Length == 0)
            {
                if (startIndex != -1)
                {
                    throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
                }
            }
            else if ((startIndex < 0) || (startIndex >= array.Length))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((count < 0) || (((startIndex - count) + 1) < 0))
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            }
            int num = startIndex - count;
            for (int i = startIndex; i > num; i--)
            {
                if (match(array[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static void ForEach<T>(T[] array, Action<T> action)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            for (int i = 0; i < array.Length; i++)
            {
                action(array[i]);
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ForceTokenStabilization, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal extern int GetDataPtrOffsetInternal();
        public IEnumerator GetEnumerator()
        {
            int lowerBound = this.GetLowerBound(0);
            if ((this.Rank == 1) && (lowerBound == 0))
            {
                return new SZArrayEnumerator(this);
            }
            return new ArrayEnumerator(this, lowerBound, this.Length);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public extern int GetLength(int dimension);
        [ComVisible(false)]
        public long GetLongLength(int dimension)
        {
            return (long) this.GetLength(dimension);
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        public extern int GetLowerBound(int dimension);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static int GetMedian(int low, int hi)
        {
            return (low + ((hi - low) >> 1));
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        public extern int GetUpperBound(int dimension);
        [SecuritySafeCritical]
        public unsafe object GetValue(params int[] indices)
        {
            if (indices == null)
            {
                throw new ArgumentNullException("indices");
            }
            if (this.Rank != indices.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_RankIndices"));
            }
            TypedReference reference = new TypedReference();
            fixed (int* numRef = indices)
            {
                this.InternalGetReference((void*) &reference, indices.Length, numRef);
            }
            return TypedReference.InternalToObject((void*) &reference);
        }

        [SecuritySafeCritical]
        public unsafe object GetValue(int index)
        {
            if (this.Rank != 1)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_Need1DArray"));
            }
            TypedReference reference = new TypedReference();
            this.InternalGetReference((void*) &reference, 1, &index);
            return TypedReference.InternalToObject((void*) &reference);
        }

        [ComVisible(false)]
        public object GetValue(long index)
        {
            if ((index > 0x7fffffffL) || (index < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            return this.GetValue((int) index);
        }

        [ComVisible(false)]
        public object GetValue(params long[] indices)
        {
            if (indices == null)
            {
                throw new ArgumentNullException("indices");
            }
            if (this.Rank != indices.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_RankIndices"));
            }
            int[] numArray = new int[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                long num2 = indices[i];
                if ((num2 > 0x7fffffffL) || (num2 < -2147483648L))
                {
                    throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
                }
                numArray[i] = (int) num2;
            }
            return this.GetValue(numArray);
        }

        [SecuritySafeCritical]
        public unsafe object GetValue(int index1, int index2)
        {
            if (this.Rank != 2)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_Need2DArray"));
            }
            int* pIndices = (int*) stackalloc byte[(((IntPtr) 2) * 4)];
            pIndices[0] = index1;
            pIndices[1] = index2;
            TypedReference reference = new TypedReference();
            this.InternalGetReference((void*) &reference, 2, pIndices);
            return TypedReference.InternalToObject((void*) &reference);
        }

        [ComVisible(false)]
        public object GetValue(long index1, long index2)
        {
            if ((index1 > 0x7fffffffL) || (index1 < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("index1", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            if ((index2 > 0x7fffffffL) || (index2 < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("index2", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            return this.GetValue((int) index1, (int) index2);
        }

        [SecuritySafeCritical]
        public unsafe object GetValue(int index1, int index2, int index3)
        {
            if (this.Rank != 3)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_Need3DArray"));
            }
            int* pIndices = (int*) stackalloc byte[(((IntPtr) 3) * 4)];
            pIndices[0] = index1;
            pIndices[1] = index2;
            pIndices[2] = index3;
            TypedReference reference = new TypedReference();
            this.InternalGetReference((void*) &reference, 3, pIndices);
            return TypedReference.InternalToObject((void*) &reference);
        }

        [ComVisible(false)]
        public object GetValue(long index1, long index2, long index3)
        {
            if ((index1 > 0x7fffffffL) || (index1 < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("index1", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            if ((index2 > 0x7fffffffL) || (index2 < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("index2", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            if ((index3 > 0x7fffffffL) || (index3 < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("index3", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            return this.GetValue((int) index1, (int) index2, (int) index3);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int IndexOf(Array array, object value)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int lowerBound = array.GetLowerBound(0);
            return IndexOf(array, value, lowerBound, array.Length);
        }

        public static int IndexOf<T>(T[] array, T value)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            return IndexOf<T>(array, value, 0, array.Length);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int IndexOf(Array array, object value, int startIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int lowerBound = array.GetLowerBound(0);
            return IndexOf(array, value, startIndex, (array.Length - startIndex) + lowerBound);
        }

        public static int IndexOf<T>(T[] array, T value, int startIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            return IndexOf<T>(array, value, startIndex, array.Length - startIndex);
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int IndexOf(Array array, object value, int startIndex, int count)
        {
            int num2;
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
            }
            int lowerBound = array.GetLowerBound(0);
            if ((startIndex < lowerBound) || (startIndex > (array.Length + lowerBound)))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((count < 0) || (count > ((array.Length - startIndex) + lowerBound)))
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            }
            if (TrySZIndexOf(array, startIndex, count, value, out num2))
            {
                return num2;
            }
            object[] objArray = array as object[];
            int num3 = startIndex + count;
            if (objArray != null)
            {
                if (value == null)
                {
                    for (int i = startIndex; i < num3; i++)
                    {
                        if (objArray[i] == null)
                        {
                            return i;
                        }
                    }
                }
                else
                {
                    for (int j = startIndex; j < num3; j++)
                    {
                        object obj2 = objArray[j];
                        if ((obj2 != null) && obj2.Equals(value))
                        {
                            return j;
                        }
                    }
                }
            }
            else
            {
                for (int k = startIndex; k < num3; k++)
                {
                    object obj3 = array.GetValue(k);
                    if (obj3 == null)
                    {
                        if (value == null)
                        {
                            return k;
                        }
                    }
                    else if (obj3.Equals(value))
                    {
                        return k;
                    }
                }
            }
            return (lowerBound - 1);
        }

        public static int IndexOf<T>(T[] array, T value, int startIndex, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if ((startIndex < 0) || (startIndex > array.Length))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((count < 0) || (count > (array.Length - startIndex)))
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            }
            return EqualityComparer<T>.Default.IndexOf(array, value, startIndex, count);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public extern void Initialize();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe Array InternalCreate(void* elementType, int rank, int* pLengths, int* pLowerBounds);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern unsafe void InternalGetReference(void* elemRef, int rank, int* pIndices);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe void InternalSetValue(void* target, object value);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int LastIndexOf(Array array, object value)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int lowerBound = array.GetLowerBound(0);
            return LastIndexOf(array, value, (array.Length - 1) + lowerBound, array.Length);
        }

        public static int LastIndexOf<T>(T[] array, T value)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            return LastIndexOf<T>(array, value, array.Length - 1, array.Length);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int LastIndexOf(Array array, object value, int startIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int lowerBound = array.GetLowerBound(0);
            return LastIndexOf(array, value, startIndex, (startIndex + 1) - lowerBound);
        }

        public static int LastIndexOf<T>(T[] array, T value, int startIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            return LastIndexOf<T>(array, value, startIndex, (array.Length == 0) ? 0 : (startIndex + 1));
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int LastIndexOf(Array array, object value, int startIndex, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int lowerBound = array.GetLowerBound(0);
            if (array.Length != 0)
            {
                int num2;
                if ((startIndex < lowerBound) || (startIndex >= (array.Length + lowerBound)))
                {
                    throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
                }
                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
                }
                if (count > ((startIndex - lowerBound) + 1))
                {
                    throw new ArgumentOutOfRangeException("endIndex", Environment.GetResourceString("ArgumentOutOfRange_EndIndexStartIndex"));
                }
                if (array.Rank != 1)
                {
                    throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
                }
                if (TrySZLastIndexOf(array, startIndex, count, value, out num2))
                {
                    return num2;
                }
                object[] objArray = array as object[];
                int num3 = (startIndex - count) + 1;
                if (objArray != null)
                {
                    if (value == null)
                    {
                        for (int i = startIndex; i >= num3; i--)
                        {
                            if (objArray[i] == null)
                            {
                                return i;
                            }
                        }
                    }
                    else
                    {
                        for (int j = startIndex; j >= num3; j--)
                        {
                            object obj2 = objArray[j];
                            if ((obj2 != null) && obj2.Equals(value))
                            {
                                return j;
                            }
                        }
                    }
                }
                else
                {
                    for (int k = startIndex; k >= num3; k--)
                    {
                        object obj3 = array.GetValue(k);
                        if (obj3 == null)
                        {
                            if (value == null)
                            {
                                return k;
                            }
                        }
                        else if (obj3.Equals(value))
                        {
                            return k;
                        }
                    }
                }
            }
            return (lowerBound - 1);
        }

        public static int LastIndexOf<T>(T[] array, T value, int startIndex, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Length == 0)
            {
                if ((startIndex != -1) && (startIndex != 0))
                {
                    throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
                }
                if (count != 0)
                {
                    throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
                }
                return -1;
            }
            if ((startIndex < 0) || (startIndex >= array.Length))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((count < 0) || (((startIndex - count) + 1) < 0))
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            }
            return EqualityComparer<T>.Default.LastIndexOf(array, value, startIndex, count);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static void Resize<T>(ref T[] array, int newSize)
        {
            if (newSize < 0)
            {
                throw new ArgumentOutOfRangeException("newSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            T[] sourceArray = array;
            if (sourceArray == null)
            {
                array = new T[newSize];
            }
            else if (sourceArray.Length != newSize)
            {
                T[] destinationArray = new T[newSize];
                Copy(sourceArray, 0, destinationArray, 0, (sourceArray.Length > newSize) ? newSize : sourceArray.Length);
                array = destinationArray;
            }
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Reverse(Array array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            Reverse(array, array.GetLowerBound(0), array.Length);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail), SecuritySafeCritical]
        public static void Reverse(Array array, int index, int length)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if ((index < array.GetLowerBound(0)) || (length < 0))
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - (index - array.GetLowerBound(0))) < length)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (array.Rank != 1)
            {
                throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
            }
            if (!TrySZReverse(array, index, length))
            {
                int num = index;
                int num2 = (index + length) - 1;
                object[] objArray = array as object[];
                if (objArray == null)
                {
                    while (num < num2)
                    {
                        object obj3 = array.GetValue(num);
                        array.SetValue(array.GetValue(num2), num);
                        array.SetValue(obj3, num2);
                        num++;
                        num2--;
                    }
                }
                else
                {
                    while (num < num2)
                    {
                        object obj2 = objArray[num];
                        objArray[num] = objArray[num2];
                        objArray[num2] = obj2;
                        num++;
                        num2--;
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public unsafe void SetValue(object value, int index)
        {
            if (this.Rank != 1)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_Need1DArray"));
            }
            TypedReference reference = new TypedReference();
            this.InternalGetReference((void*) &reference, 1, &index);
            InternalSetValue((void*) &reference, value);
        }

        [SecuritySafeCritical]
        public unsafe void SetValue(object value, params int[] indices)
        {
            if (indices == null)
            {
                throw new ArgumentNullException("indices");
            }
            if (this.Rank != indices.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_RankIndices"));
            }
            TypedReference reference = new TypedReference();
            fixed (int* numRef = indices)
            {
                this.InternalGetReference((void*) &reference, indices.Length, numRef);
            }
            InternalSetValue((void*) &reference, value);
        }

        [ComVisible(false)]
        public void SetValue(object value, long index)
        {
            if ((index > 0x7fffffffL) || (index < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            this.SetValue(value, (int) index);
        }

        [ComVisible(false)]
        public void SetValue(object value, params long[] indices)
        {
            if (indices == null)
            {
                throw new ArgumentNullException("indices");
            }
            if (this.Rank != indices.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_RankIndices"));
            }
            int[] numArray = new int[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                long num2 = indices[i];
                if ((num2 > 0x7fffffffL) || (num2 < -2147483648L))
                {
                    throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
                }
                numArray[i] = (int) num2;
            }
            this.SetValue(value, numArray);
        }

        [SecuritySafeCritical]
        public unsafe void SetValue(object value, int index1, int index2)
        {
            if (this.Rank != 2)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_Need2DArray"));
            }
            int* pIndices = (int*) stackalloc byte[(((IntPtr) 2) * 4)];
            pIndices[0] = index1;
            pIndices[1] = index2;
            TypedReference reference = new TypedReference();
            this.InternalGetReference((void*) &reference, 2, pIndices);
            InternalSetValue((void*) &reference, value);
        }

        [ComVisible(false)]
        public void SetValue(object value, long index1, long index2)
        {
            if ((index1 > 0x7fffffffL) || (index1 < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("index1", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            if ((index2 > 0x7fffffffL) || (index2 < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("index2", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            this.SetValue(value, (int) index1, (int) index2);
        }

        [SecuritySafeCritical]
        public unsafe void SetValue(object value, int index1, int index2, int index3)
        {
            if (this.Rank != 3)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_Need3DArray"));
            }
            int* pIndices = (int*) stackalloc byte[(((IntPtr) 3) * 4)];
            pIndices[0] = index1;
            pIndices[1] = index2;
            pIndices[2] = index3;
            TypedReference reference = new TypedReference();
            this.InternalGetReference((void*) &reference, 3, pIndices);
            InternalSetValue((void*) &reference, value);
        }

        [ComVisible(false)]
        public void SetValue(object value, long index1, long index2, long index3)
        {
            if ((index1 > 0x7fffffffL) || (index1 < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("index1", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            if ((index2 > 0x7fffffffL) || (index2 < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("index2", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            if ((index3 > 0x7fffffffL) || (index3 < -2147483648L))
            {
                throw new ArgumentOutOfRangeException("index3", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }
            this.SetValue(value, (int) index1, (int) index2, (int) index3);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Sort(Array array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            Sort(array, null, array.GetLowerBound(0), array.Length, null);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Sort<T>(T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            Sort<T>(array, array.GetLowerBound(0), array.Length, null);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Sort(Array keys, Array items)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            Sort(keys, items, keys.GetLowerBound(0), keys.Length, null);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Sort<T>(T[] array, IComparer<T> comparer)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            Sort<T>(array, 0, array.Length, comparer);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Sort(Array array, IComparer comparer)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            Sort(array, null, array.GetLowerBound(0), array.Length, comparer);
        }

        public static void Sort<T>(T[] array, Comparison<T> comparison)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (comparison == null)
            {
                throw new ArgumentNullException("comparison");
            }
            IComparer<T> comparer = new FunctorComparer<T>(comparison);
            Sort<T>(array, comparer);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            Sort<TKey, TValue>(keys, items, 0, keys.Length, null);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Sort(Array keys, Array items, IComparer comparer)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            Sort(keys, items, keys.GetLowerBound(0), keys.Length, comparer);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Sort(Array array, int index, int length)
        {
            Sort(array, null, index, length, null);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Sort<T>(T[] array, int index, int length)
        {
            Sort<T>(array, index, length, null);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, IComparer<TKey> comparer)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            Sort<TKey, TValue>(keys, items, 0, keys.Length, comparer);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Sort(Array keys, Array items, int index, int length)
        {
            Sort(keys, items, index, length, null);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail), SecuritySafeCritical]
        public static void Sort<T>(T[] array, int index, int length, IComparer<T> comparer)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if ((index < 0) || (length < 0))
            {
                throw new ArgumentOutOfRangeException((length < 0) ? "length" : "index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - index) < length)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if ((length > 1) && (((comparer != null) && (comparer != Comparer<T>.Default)) || !TrySZSort(array, null, index, (index + length) - 1)))
            {
                ArraySortHelper<T>.Default.Sort(array, index, length, comparer);
            }
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Sort(Array array, int index, int length, IComparer comparer)
        {
            Sort(array, null, index, length, comparer);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, int index, int length)
        {
            Sort<TKey, TValue>(keys, items, index, length, null);
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Sort(Array keys, Array items, int index, int length, IComparer comparer)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            if ((keys.Rank != 1) || ((items != null) && (items.Rank != 1)))
            {
                throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
            }
            if ((items != null) && (keys.GetLowerBound(0) != items.GetLowerBound(0)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_LowerBoundsMustMatch"));
            }
            if ((index < keys.GetLowerBound(0)) || (length < 0))
            {
                throw new ArgumentOutOfRangeException((length < 0) ? "length" : "index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (((keys.Length - (index - keys.GetLowerBound(0))) < length) || ((items != null) && ((index - items.GetLowerBound(0)) > (items.Length - length))))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if ((length > 1) && (((comparer != Comparer.Default) && (comparer != null)) || !TrySZSort(keys, items, index, (index + length) - 1)))
            {
                object[] objArray = keys as object[];
                object[] objArray2 = null;
                if (objArray != null)
                {
                    objArray2 = items as object[];
                }
                if ((objArray != null) && ((items == null) || (objArray2 != null)))
                {
                    new SorterObjectArray(objArray, objArray2, comparer).QuickSort(index, (index + length) - 1);
                }
                else
                {
                    new SorterGenericArray(keys, items, comparer).QuickSort(index, (index + length) - 1);
                }
            }
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, int index, int length, IComparer<TKey> comparer)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            if ((index < 0) || (length < 0))
            {
                throw new ArgumentOutOfRangeException((length < 0) ? "length" : "index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (((keys.Length - index) < length) || ((items != null) && (index > (items.Length - length))))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if ((length > 1) && (((comparer != null) && (comparer != Comparer<TKey>.Default)) || !TrySZSort(keys, items, index, (index + length) - 1)))
            {
                ArraySortHelper<TKey, TValue>.Default.Sort(keys, items, index, length, comparer);
            }
        }

        int IList.Add(object value)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
        }

        void IList.Clear()
        {
            Clear(this, 0, this.Length);
        }

        bool IList.Contains(object value)
        {
            return (IndexOf(this, value) >= this.GetLowerBound(0));
        }

        int IList.IndexOf(object value)
        {
            return IndexOf(this, value);
        }

        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
        }

        void IList.Remove(object value)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
        }

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }
            Array array = other as Array;
            if ((array == null) || (this.Length != array.Length))
            {
                throw new ArgumentException(Environment.GetResourceString("ArgumentException_OtherNotArrayOfCorrectLength"), "other");
            }
            int index = 0;
            int num2 = 0;
            while ((index < array.Length) && (num2 == 0))
            {
                object x = this.GetValue(index);
                object y = array.GetValue(index);
                num2 = comparer.Compare(x, y);
                index++;
            }
            return num2;
        }

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }
            if (!object.ReferenceEquals(this, other))
            {
                Array array = other as Array;
                if ((array == null) || (array.Length != this.Length))
                {
                    return false;
                }
                for (int i = 0; i < array.Length; i++)
                {
                    object x = this.GetValue(i);
                    object y = array.GetValue(i);
                    if (!comparer.Equals(x, y))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            int num = 0;
            for (int i = (this.Length >= 8) ? (this.Length - 8) : 0; i < this.Length; i++)
            {
                num = CombineHashCodes(num, comparer.GetHashCode(this.GetValue(0)));
            }
            return num;
        }

        public static bool TrueForAll<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            for (int i = 0; i < array.Length; i++)
            {
                if (!match(array[i]))
                {
                    return false;
                }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecurityCritical]
        private static extern bool TrySZBinarySearch(Array sourceArray, int sourceIndex, int count, object value, out int retVal);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private static extern bool TrySZIndexOf(Array sourceArray, int sourceIndex, int count, object value, out int retVal);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private static extern bool TrySZLastIndexOf(Array sourceArray, int sourceIndex, int count, object value, out int retVal);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        private static extern bool TrySZReverse(Array array, int index, int count);
        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail), SecurityCritical]
        private static extern bool TrySZSort(Array keys, Array items, int left, int right);
        [SecurityCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal static Array UnsafeCreateInstance(Type elementType, int length)
        {
            return CreateInstance(elementType, length);
        }

        [SecurityCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal static Array UnsafeCreateInstance(Type elementType, params int[] lengths)
        {
            return CreateInstance(elementType, lengths);
        }

        [SecurityCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal static Array UnsafeCreateInstance(Type elementType, int length1, int length2)
        {
            return CreateInstance(elementType, length1, length2);
        }

        [SecurityCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal static Array UnsafeCreateInstance(Type elementType, int[] lengths, int[] lowerBounds)
        {
            return CreateInstance(elementType, lengths, lowerBounds);
        }

        public bool IsFixedSize
        {
            get
            {
                return true;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public int Length { [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical] get; }

        [ComVisible(false)]
        public long LongLength
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return (long) this.Length;
            }
        }

        public int Rank { [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical] get; }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        int ICollection.Count
        {
            get
            {
                return this.Length;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this.GetValue(index);
            }
            set
            {
                this.SetValue(value, index);
            }
        }

        [Serializable]
        private sealed class ArrayEnumerator : IEnumerator, ICloneable
        {
            private bool _complete;
            private int[] _indices;
            private Array array;
            private int endIndex;
            private int index;
            private int startIndex;

            internal ArrayEnumerator(Array array, int index, int count)
            {
                this.array = array;
                this.index = index - 1;
                this.startIndex = index;
                this.endIndex = index + count;
                this._indices = new int[array.Rank];
                int num = 1;
                for (int i = 0; i < array.Rank; i++)
                {
                    this._indices[i] = array.GetLowerBound(i);
                    num *= array.GetLength(i);
                }
                this._indices[this._indices.Length - 1]--;
                this._complete = num == 0;
            }

            [SecuritySafeCritical]
            public object Clone()
            {
                return base.MemberwiseClone();
            }

            private void IncArray()
            {
                int rank = this.array.Rank;
                this._indices[rank - 1]++;
                for (int i = rank - 1; i >= 0; i--)
                {
                    if (this._indices[i] > this.array.GetUpperBound(i))
                    {
                        if (i == 0)
                        {
                            this._complete = true;
                            return;
                        }
                        for (int j = i; j < rank; j++)
                        {
                            this._indices[j] = this.array.GetLowerBound(j);
                        }
                        this._indices[i - 1]++;
                    }
                }
            }

            public bool MoveNext()
            {
                if (this._complete)
                {
                    this.index = this.endIndex;
                    return false;
                }
                this.index++;
                this.IncArray();
                return !this._complete;
            }

            public void Reset()
            {
                this.index = this.startIndex - 1;
                int num = 1;
                for (int i = 0; i < this.array.Rank; i++)
                {
                    this._indices[i] = this.array.GetLowerBound(i);
                    num *= this.array.GetLength(i);
                }
                this._complete = num == 0;
                this._indices[this._indices.Length - 1]--;
            }

            public object Current
            {
                get
                {
                    if (this.index < this.startIndex)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
                    }
                    if (this._complete)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
                    }
                    return this.array.GetValue(this._indices);
                }
            }
        }

        internal sealed class FunctorComparer<T> : IComparer<T>
        {
            private Comparer<T> c;
            private Comparison<T> comparison;

            public FunctorComparer(Comparison<T> comparison)
            {
                this.c = Comparer<T>.Default;
                this.comparison = comparison;
            }

            public int Compare(T x, T y)
            {
                return this.comparison(x, y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SorterGenericArray
        {
            private Array keys;
            private Array items;
            private IComparer comparer;
            internal SorterGenericArray(Array keys, Array items, IComparer comparer)
            {
                if (comparer == null)
                {
                    comparer = Comparer.Default;
                }
                this.keys = keys;
                this.items = items;
                this.comparer = comparer;
            }

            internal void SwapIfGreaterWithItems(int a, int b)
            {
                if (a != b)
                {
                    try
                    {
                        if (this.comparer.Compare(this.keys.GetValue(a), this.keys.GetValue(b)) > 0)
                        {
                            object obj2 = this.keys.GetValue(a);
                            this.keys.SetValue(this.keys.GetValue(b), a);
                            this.keys.SetValue(obj2, b);
                            if (this.items != null)
                            {
                                object obj3 = this.items.GetValue(a);
                                this.items.SetValue(this.items.GetValue(b), a);
                                this.items.SetValue(obj3, b);
                            }
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", new object[] { this.keys.GetValue(b), this.keys.GetValue(b).GetType().Name, this.comparer }));
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), exception);
                    }
                }
            }

            internal void QuickSort(int left, int right)
            {
                do
                {
                    int low = left;
                    int hi = right;
                    int median = Array.GetMedian(low, hi);
                    this.SwapIfGreaterWithItems(low, median);
                    this.SwapIfGreaterWithItems(low, hi);
                    this.SwapIfGreaterWithItems(median, hi);
                    object y = this.keys.GetValue(median);
                    do
                    {
                        try
                        {
                            while (this.comparer.Compare(this.keys.GetValue(low), y) < 0)
                            {
                                low++;
                            }
                            while (this.comparer.Compare(y, this.keys.GetValue(hi)) < 0)
                            {
                                hi--;
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", new object[] { y, y.GetType().Name, this.comparer }));
                        }
                        catch (Exception exception)
                        {
                            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), exception);
                        }
                        if (low > hi)
                        {
                            break;
                        }
                        if (low < hi)
                        {
                            object obj3 = this.keys.GetValue(low);
                            this.keys.SetValue(this.keys.GetValue(hi), low);
                            this.keys.SetValue(obj3, hi);
                            if (this.items != null)
                            {
                                object obj4 = this.items.GetValue(low);
                                this.items.SetValue(this.items.GetValue(hi), low);
                                this.items.SetValue(obj4, hi);
                            }
                        }
                        if (low != 0x7fffffff)
                        {
                            low++;
                        }
                        if (hi != -2147483648)
                        {
                            hi--;
                        }
                    }
                    while (low <= hi);
                    if ((hi - left) <= (right - low))
                    {
                        if (left < hi)
                        {
                            this.QuickSort(left, hi);
                        }
                        left = low;
                    }
                    else
                    {
                        if (low < right)
                        {
                            this.QuickSort(low, right);
                        }
                        right = hi;
                    }
                }
                while (left < right);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SorterObjectArray
        {
            private object[] keys;
            private object[] items;
            private IComparer comparer;
            internal SorterObjectArray(object[] keys, object[] items, IComparer comparer)
            {
                if (comparer == null)
                {
                    comparer = Comparer.Default;
                }
                this.keys = keys;
                this.items = items;
                this.comparer = comparer;
            }

            internal void SwapIfGreaterWithItems(int a, int b)
            {
                if (a != b)
                {
                    try
                    {
                        if (this.comparer.Compare(this.keys[a], this.keys[b]) > 0)
                        {
                            object obj2 = this.keys[a];
                            this.keys[a] = this.keys[b];
                            this.keys[b] = obj2;
                            if (this.items != null)
                            {
                                object obj3 = this.items[a];
                                this.items[a] = this.items[b];
                                this.items[b] = obj3;
                            }
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", new object[] { this.keys[b], this.keys[b].GetType().Name, this.comparer }));
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), exception);
                    }
                }
            }

            internal void QuickSort(int left, int right)
            {
                do
                {
                    int low = left;
                    int hi = right;
                    int median = Array.GetMedian(low, hi);
                    this.SwapIfGreaterWithItems(low, median);
                    this.SwapIfGreaterWithItems(low, hi);
                    this.SwapIfGreaterWithItems(median, hi);
                    object y = this.keys[median];
                    do
                    {
                        try
                        {
                            while (this.comparer.Compare(this.keys[low], y) < 0)
                            {
                                low++;
                            }
                            while (this.comparer.Compare(y, this.keys[hi]) < 0)
                            {
                                hi--;
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", new object[] { y, y.GetType().Name, this.comparer }));
                        }
                        catch (Exception exception)
                        {
                            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), exception);
                        }
                        if (low > hi)
                        {
                            break;
                        }
                        if (low < hi)
                        {
                            object obj3 = this.keys[low];
                            this.keys[low] = this.keys[hi];
                            this.keys[hi] = obj3;
                            if (this.items != null)
                            {
                                object obj4 = this.items[low];
                                this.items[low] = this.items[hi];
                                this.items[hi] = obj4;
                            }
                        }
                        low++;
                        hi--;
                    }
                    while (low <= hi);
                    if ((hi - left) <= (right - low))
                    {
                        if (left < hi)
                        {
                            this.QuickSort(left, hi);
                        }
                        left = low;
                    }
                    else
                    {
                        if (low < right)
                        {
                            this.QuickSort(low, right);
                        }
                        right = hi;
                    }
                }
                while (left < right);
            }
        }

        [Serializable]
        private sealed class SZArrayEnumerator : IEnumerator, ICloneable
        {
            private Array _array;
            private int _endIndex;
            private int _index;

            internal SZArrayEnumerator(Array array)
            {
                this._array = array;
                this._index = -1;
                this._endIndex = array.Length;
            }

            [SecuritySafeCritical]
            public object Clone()
            {
                return base.MemberwiseClone();
            }

            public bool MoveNext()
            {
                if (this._index < this._endIndex)
                {
                    this._index++;
                    return (this._index < this._endIndex);
                }
                return false;
            }

            public void Reset()
            {
                this._index = -1;
            }

            public object Current
            {
                get
                {
                    if (this._index < 0)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
                    }
                    if (this._index >= this._endIndex)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
                    }
                    return this._array.GetValue(this._index);
                }
            }
        }
    }
}

