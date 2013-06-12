namespace System.Collections.Concurrent
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    [DebuggerDisplay("Count = {Count}, Type = {m_collection}"), DebuggerTypeProxy(typeof(SystemThreadingCollections_BlockingCollectionDebugView<>)), ComVisible(false), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class BlockingCollection<T> : IEnumerable<T>, ICollection, IEnumerable, IDisposable
    {
        private const int COMPLETE_ADDING_ON_MASK = -2147483648;
        private int m_boundedCapacity;
        private IProducerConsumerCollection<T> m_collection;
        private CancellationTokenSource m_ConsumersCancellationTokenSource;
        private volatile int m_currentAdders;
        private SemaphoreSlim m_freeNodes;
        private volatile bool m_isDisposed;
        private SemaphoreSlim m_occupiedNodes;
        private CancellationTokenSource m_ProducersCancellationTokenSource;
        private const int NON_BOUNDED = -1;

        public BlockingCollection() : this(new ConcurrentQueue<T>())
        {
        }

        public BlockingCollection(int boundedCapacity) : this(new ConcurrentQueue<T>(), boundedCapacity)
        {
        }

        public BlockingCollection(IProducerConsumerCollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.Initialize(collection, -1, collection.Count);
        }

        public BlockingCollection(IProducerConsumerCollection<T> collection, int boundedCapacity)
        {
            if (boundedCapacity < 1)
            {
                throw new ArgumentOutOfRangeException("boundedCapacity", boundedCapacity, SR.GetString("BlockingCollection_ctor_BoundedCapacityRange"));
            }
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            int count = collection.Count;
            if (count > boundedCapacity)
            {
                throw new ArgumentException(SR.GetString("BlockingCollection_ctor_CountMoreThanCapacity"));
            }
            this.Initialize(collection, boundedCapacity, count);
        }

        public void Add(T item)
        {
            this.TryAddWithNoTimeValidation(item, -1, new CancellationToken());
        }

        public void Add(T item, CancellationToken cancellationToken)
        {
            this.TryAddWithNoTimeValidation(item, -1, cancellationToken);
        }

        public static int AddToAny(BlockingCollection<T>[] collections, T item)
        {
            return BlockingCollection<T>.TryAddToAny(collections, item, -1, new CancellationToken());
        }

        public static int AddToAny(BlockingCollection<T>[] collections, T item, CancellationToken cancellationToken)
        {
            return BlockingCollection<T>.TryAddToAny(collections, item, -1, cancellationToken);
        }

        private void CancelWaitingConsumers()
        {
            this.m_ConsumersCancellationTokenSource.Cancel();
        }

        private void CancelWaitingProducers()
        {
            this.m_ProducersCancellationTokenSource.Cancel();
        }

        private void CheckDisposed()
        {
            if (this.m_isDisposed)
            {
                throw new ObjectDisposedException("BlockingCollection", SR.GetString("BlockingCollection_Disposed"));
            }
        }

        public void CompleteAdding()
        {
            int num;
            this.CheckDisposed();
            if (this.IsAddingCompleted)
            {
                return;
            }
            SpinWait wait = new SpinWait();
        Label_0017:
            num = this.m_currentAdders;
            if ((num & -2147483648) != 0)
            {
                wait.Reset();
                while (this.m_currentAdders != -2147483648)
                {
                    wait.SpinOnce();
                }
            }
            else if (Interlocked.CompareExchange(ref this.m_currentAdders, num | -2147483648, num) == num)
            {
                wait.Reset();
                while (this.m_currentAdders != -2147483648)
                {
                    wait.SpinOnce();
                }
                if (this.Count == 0)
                {
                    this.CancelWaitingConsumers();
                }
                this.CancelWaitingProducers();
            }
            else
            {
                wait.SpinOnce();
                goto Label_0017;
            }
        }

        public void CopyTo(T[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.m_isDisposed)
            {
                if (this.m_freeNodes != null)
                {
                    this.m_freeNodes.Dispose();
                }
                this.m_occupiedNodes.Dispose();
                this.m_isDisposed = true;
            }
        }

        public IEnumerable<T> GetConsumingEnumerable()
        {
            return this.GetConsumingEnumerable(CancellationToken.None);
        }

        public IEnumerable<T> GetConsumingEnumerable(CancellationToken cancellationToken)
        {
            CancellationTokenSource combinedTokenSource = null;
            combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.m_ConsumersCancellationTokenSource.Token);
            while (!this.IsCompleted)
            {
                T iteratorVariable1;
                if (this.TryTakeWithNoTimeValidation(out iteratorVariable1, -1, cancellationToken, combinedTokenSource))
                {
                    yield return iteratorVariable1;
                }
            }
        }

        private static List<WaitHandle> GetHandles(BlockingCollection<T>[] collections, OperationMode<T> operationMode, CancellationToken externalCancellationToken, bool excludeCompleted, out CancellationToken[] cancellationTokens)
        {
            List<WaitHandle> list = new List<WaitHandle>(collections.Length);
            List<CancellationToken> list2 = new List<CancellationToken>(collections.Length + 1) {
                externalCancellationToken
            };
            if (operationMode == OperationMode<T>.Add)
            {
                for (int i = 0; i < collections.Length; i++)
                {
                    if (collections[i].m_freeNodes != null)
                    {
                        list.Add(collections[i].m_freeNodes.AvailableWaitHandle);
                        list2.Add(collections[i].m_ProducersCancellationTokenSource.Token);
                    }
                }
            }
            else
            {
                for (int j = 0; j < collections.Length; j++)
                {
                    if (!excludeCompleted || !collections[j].IsCompleted)
                    {
                        list.Add(collections[j].m_occupiedNodes.AvailableWaitHandle);
                        list2.Add(collections[j].m_ConsumersCancellationTokenSource.Token);
                    }
                }
            }
            cancellationTokens = list2.ToArray();
            return list;
        }

        private void Initialize(IProducerConsumerCollection<T> collection, int boundedCapacity, int collectionCount)
        {
            this.m_collection = collection;
            this.m_boundedCapacity = boundedCapacity;
            this.m_isDisposed = false;
            this.m_ConsumersCancellationTokenSource = new CancellationTokenSource();
            this.m_ProducersCancellationTokenSource = new CancellationTokenSource();
            if (boundedCapacity == -1)
            {
                this.m_freeNodes = null;
            }
            else
            {
                this.m_freeNodes = new SemaphoreSlim(boundedCapacity - collectionCount);
            }
            this.m_occupiedNodes = new SemaphoreSlim(collectionCount);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            this.CheckDisposed();
            return this.m_collection.GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.CheckDisposed();
            T[] sourceArray = this.m_collection.ToArray();
            try
            {
                Array.Copy(sourceArray, 0, array, index, sourceArray.Length);
            }
            catch (ArgumentNullException)
            {
                throw new ArgumentNullException("array");
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("index", index, SR.GetString("BlockingCollection_CopyTo_NonNegative"));
            }
            catch (ArgumentException)
            {
                throw new ArgumentException(SR.GetString("BlockingCollection_CopyTo_TooManyElems"), "index");
            }
            catch (RankException)
            {
                throw new ArgumentException(SR.GetString("BlockingCollection_CopyTo_MultiDim"), "array");
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(SR.GetString("BlockingCollection_CopyTo_IncorrectType"), "array");
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException(SR.GetString("BlockingCollection_CopyTo_IncorrectType"), "array");
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>) this).GetEnumerator();
        }

        public T Take()
        {
            T local;
            if (!this.TryTake(out local, -1, CancellationToken.None))
            {
                throw new InvalidOperationException(SR.GetString("BlockingCollection_CantTakeWhenDone"));
            }
            return local;
        }

        public T Take(CancellationToken cancellationToken)
        {
            T local;
            if (!this.TryTake(out local, -1, cancellationToken))
            {
                throw new InvalidOperationException(SR.GetString("BlockingCollection_CantTakeWhenDone"));
            }
            return local;
        }

        public static int TakeFromAny(BlockingCollection<T>[] collections, out T item)
        {
            return BlockingCollection<T>.TryTakeFromAny(collections, out item, -1);
        }

        public static int TakeFromAny(BlockingCollection<T>[] collections, out T item, CancellationToken cancellationToken)
        {
            return BlockingCollection<T>.TryTakeFromAny(collections, out item, -1, cancellationToken);
        }

        public T[] ToArray()
        {
            this.CheckDisposed();
            return this.m_collection.ToArray();
        }

        public bool TryAdd(T item)
        {
            return this.TryAddWithNoTimeValidation(item, 0, new CancellationToken());
        }

        public bool TryAdd(T item, int millisecondsTimeout)
        {
            BlockingCollection<T>.ValidateMillisecondsTimeout(millisecondsTimeout);
            return this.TryAddWithNoTimeValidation(item, millisecondsTimeout, new CancellationToken());
        }

        public bool TryAdd(T item, TimeSpan timeout)
        {
            BlockingCollection<T>.ValidateTimeout(timeout);
            return this.TryAddWithNoTimeValidation(item, (int) timeout.TotalMilliseconds, new CancellationToken());
        }

        public bool TryAdd(T item, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            BlockingCollection<T>.ValidateMillisecondsTimeout(millisecondsTimeout);
            return this.TryAddWithNoTimeValidation(item, millisecondsTimeout, cancellationToken);
        }

        private static int TryAddTakeAny(BlockingCollection<T>[] collections, ref T item, int millisecondsTimeout, OperationMode<T> operationMode, CancellationToken externalCancellationToken)
        {
            CancellationToken[] tokenArray;
            BlockingCollection<T>[] blockingsArray = BlockingCollection<T>.ValidateCollectionsArray(collections, operationMode);
            int num = millisecondsTimeout;
            long startTimeTicks = 0L;
            if (millisecondsTimeout != -1)
            {
                startTimeTicks = DateTime.UtcNow.Ticks;
            }
            if (operationMode == OperationMode<T>.Add)
            {
                for (int i = 0; i < blockingsArray.Length; i++)
                {
                    if (blockingsArray[i].m_freeNodes == null)
                    {
                        blockingsArray[i].TryAdd(item);
                        return i;
                    }
                }
            }
            List<WaitHandle> handles = BlockingCollection<T>.GetHandles(blockingsArray, operationMode, externalCancellationToken, false, out tokenArray);
            while ((millisecondsTimeout == -1) || (num >= 0))
            {
                int index = -1;
                CancellationTokenSource source = null;
                try
                {
                    index = BlockingCollection<T>.WaitHandle_WaitAny(handles, 0, externalCancellationToken, externalCancellationToken);
                    if (index == 0x102)
                    {
                        source = CancellationTokenSource.CreateLinkedTokenSource(tokenArray);
                        index = BlockingCollection<T>.WaitHandle_WaitAny(handles, num, source.Token, externalCancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    if (externalCancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    if ((operationMode != OperationMode<T>.Take) || (millisecondsTimeout == -1))
                    {
                        throw new ArgumentException(SR.GetString("BlockingCollection_CantTakeAnyWhenDone"), "collections");
                    }
                    handles = BlockingCollection<T>.GetHandles(blockingsArray, operationMode, externalCancellationToken, true, out tokenArray);
                    num = BlockingCollection<T>.UpdateTimeOut(startTimeTicks, millisecondsTimeout);
                    if ((handles.Count != 0) && (num != 0))
                    {
                        continue;
                    }
                    return -1;
                }
                finally
                {
                    if (source != null)
                    {
                        source.Dispose();
                    }
                }
                if (index == 0x102)
                {
                    return -1;
                }
                if (operationMode == OperationMode<T>.Add)
                {
                    if (blockingsArray[index].TryAdd(item))
                    {
                        return index;
                    }
                }
                else if ((operationMode == OperationMode<T>.Take) && blockingsArray[index].TryTake(out item))
                {
                    return index;
                }
                if (millisecondsTimeout > 0)
                {
                    num = BlockingCollection<T>.UpdateTimeOut(startTimeTicks, millisecondsTimeout);
                    if (num <= 0)
                    {
                        return -1;
                    }
                }
            }
            return -1;
        }

        public static int TryAddToAny(BlockingCollection<T>[] collections, T item)
        {
            return BlockingCollection<T>.TryAddToAny(collections, item, 0, new CancellationToken());
        }

        public static int TryAddToAny(BlockingCollection<T>[] collections, T item, int millisecondsTimeout)
        {
            BlockingCollection<T>.ValidateMillisecondsTimeout(millisecondsTimeout);
            return BlockingCollection<T>.TryAddTakeAny(collections, ref item, millisecondsTimeout, OperationMode<T>.Add, new CancellationToken());
        }

        public static int TryAddToAny(BlockingCollection<T>[] collections, T item, TimeSpan timeout)
        {
            BlockingCollection<T>.ValidateTimeout(timeout);
            return BlockingCollection<T>.TryAddTakeAny(collections, ref item, (int) timeout.TotalMilliseconds, OperationMode<T>.Add, new CancellationToken());
        }

        public static int TryAddToAny(BlockingCollection<T>[] collections, T item, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            BlockingCollection<T>.ValidateMillisecondsTimeout(millisecondsTimeout);
            return BlockingCollection<T>.TryAddTakeAny(collections, ref item, millisecondsTimeout, OperationMode<T>.Add, cancellationToken);
        }

        private bool TryAddWithNoTimeValidation(T item, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            int num;
            this.CheckDisposed();
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(SR.GetString("Common_OperationCanceled"), cancellationToken);
            }
            if (this.IsAddingCompleted)
            {
                throw new InvalidOperationException(SR.GetString("BlockingCollection_Completed"));
            }
            bool flag = true;
            if (this.m_freeNodes != null)
            {
                CancellationTokenSource source = null;
                try
                {
                    flag = this.m_freeNodes.Wait(0);
                    if (!flag && (millisecondsTimeout != 0))
                    {
                        source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.m_ProducersCancellationTokenSource.Token);
                        flag = this.m_freeNodes.Wait(millisecondsTimeout, source.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(SR.GetString("Common_OperationCanceled"), cancellationToken);
                    }
                    throw new InvalidOperationException(SR.GetString("BlockingCollection_Add_ConcurrentCompleteAdd"));
                }
                finally
                {
                    if (source != null)
                    {
                        source.Dispose();
                    }
                }
            }
            if (!flag)
            {
                return flag;
            }
            SpinWait wait = new SpinWait();
        Label_00C3:
            num = this.m_currentAdders;
            if ((num & -2147483648) != 0)
            {
                wait.Reset();
                while (this.m_currentAdders != -2147483648)
                {
                    wait.SpinOnce();
                }
                throw new InvalidOperationException(SR.GetString("BlockingCollection_Completed"));
            }
            if (Interlocked.CompareExchange(ref this.m_currentAdders, num + 1, num) != num)
            {
                wait.SpinOnce();
                goto Label_00C3;
            }
            try
            {
                bool flag2 = false;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    flag2 = this.m_collection.TryAdd(item);
                }
                catch
                {
                    if (this.m_freeNodes != null)
                    {
                        this.m_freeNodes.Release();
                    }
                    throw;
                }
                if (!flag2)
                {
                    throw new InvalidOperationException(SR.GetString("BlockingCollection_Add_Failed"));
                }
                this.m_occupiedNodes.Release();
                return flag;
            }
            finally
            {
                Interlocked.Decrement(ref this.m_currentAdders);
            }
            return flag;
        }

        public bool TryTake(out T item)
        {
            return this.TryTake(out item, 0, CancellationToken.None);
        }

        public bool TryTake(out T item, int millisecondsTimeout)
        {
            BlockingCollection<T>.ValidateMillisecondsTimeout(millisecondsTimeout);
            return this.TryTakeWithNoTimeValidation(out item, millisecondsTimeout, CancellationToken.None, null);
        }

        public bool TryTake(out T item, TimeSpan timeout)
        {
            BlockingCollection<T>.ValidateTimeout(timeout);
            return this.TryTakeWithNoTimeValidation(out item, (int) timeout.TotalMilliseconds, CancellationToken.None, null);
        }

        public bool TryTake(out T item, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            BlockingCollection<T>.ValidateMillisecondsTimeout(millisecondsTimeout);
            return this.TryTakeWithNoTimeValidation(out item, millisecondsTimeout, cancellationToken, null);
        }

        public static int TryTakeFromAny(BlockingCollection<T>[] collections, out T item)
        {
            return BlockingCollection<T>.TryTakeFromAny(collections, out item, 0);
        }

        public static int TryTakeFromAny(BlockingCollection<T>[] collections, out T item, int millisecondsTimeout)
        {
            BlockingCollection<T>.ValidateMillisecondsTimeout(millisecondsTimeout);
            T local = default(T);
            int num = BlockingCollection<T>.TryAddTakeAny(collections, ref local, millisecondsTimeout, OperationMode<T>.Take, new CancellationToken());
            item = local;
            return num;
        }

        public static int TryTakeFromAny(BlockingCollection<T>[] collections, out T item, TimeSpan timeout)
        {
            BlockingCollection<T>.ValidateTimeout(timeout);
            T local = default(T);
            int num = BlockingCollection<T>.TryAddTakeAny(collections, ref local, (int) timeout.TotalMilliseconds, OperationMode<T>.Take, new CancellationToken());
            item = local;
            return num;
        }

        public static int TryTakeFromAny(BlockingCollection<T>[] collections, out T item, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            BlockingCollection<T>.ValidateMillisecondsTimeout(millisecondsTimeout);
            T local = default(T);
            int num = BlockingCollection<T>.TryAddTakeAny(collections, ref local, millisecondsTimeout, OperationMode<T>.Take, cancellationToken);
            item = local;
            return num;
        }

        private bool TryTakeWithNoTimeValidation(out T item, int millisecondsTimeout, CancellationToken cancellationToken, CancellationTokenSource combinedTokenSource)
        {
            this.CheckDisposed();
            item = default(T);
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(SR.GetString("Common_OperationCanceled"), cancellationToken);
            }
            if (this.IsCompleted)
            {
                return false;
            }
            bool flag = false;
            CancellationTokenSource source = combinedTokenSource;
            try
            {
                flag = this.m_occupiedNodes.Wait(0);
                if (!flag && (millisecondsTimeout != 0))
                {
                    if (combinedTokenSource == null)
                    {
                        source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.m_ConsumersCancellationTokenSource.Token);
                    }
                    flag = this.m_occupiedNodes.Wait(millisecondsTimeout, source.Token);
                }
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(SR.GetString("Common_OperationCanceled"), cancellationToken);
                }
                return false;
            }
            finally
            {
                if ((source != null) && (combinedTokenSource == null))
                {
                    source.Dispose();
                }
            }
            if (flag)
            {
                bool flag2 = false;
                bool flag3 = true;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    flag2 = this.m_collection.TryTake(out item);
                    flag3 = false;
                    if (!flag2)
                    {
                        throw new InvalidOperationException(SR.GetString("BlockingCollection_Take_CollectionModified"));
                    }
                }
                finally
                {
                    if (flag2)
                    {
                        if (this.m_freeNodes != null)
                        {
                            this.m_freeNodes.Release();
                        }
                    }
                    else if (flag3)
                    {
                        this.m_occupiedNodes.Release();
                    }
                    if (this.IsCompleted)
                    {
                        this.CancelWaitingConsumers();
                    }
                }
            }
            return flag;
        }

        private static int UpdateTimeOut(long startTimeTicks, int originalWaitMillisecondsTimeout)
        {
            if (originalWaitMillisecondsTimeout == 0)
            {
                return 0;
            }
            long num = (DateTime.UtcNow.Ticks - startTimeTicks) / 0x2710L;
            if (num > 0x7fffffffL)
            {
                return 0;
            }
            int num2 = originalWaitMillisecondsTimeout - ((int) num);
            if (num2 <= 0)
            {
                return 0;
            }
            return num2;
        }

        private static BlockingCollection<T>[] ValidateCollectionsArray(BlockingCollection<T>[] collections, OperationMode<T> operationMode)
        {
            if (collections == null)
            {
                throw new ArgumentNullException("collections");
            }
            if (collections.Length < 1)
            {
                throw new ArgumentException(SR.GetString("BlockingCollection_ValidateCollectionsArray_ZeroSize"), "collections");
            }
            if (((Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA) && (collections.Length > 0x3f)) || ((Thread.CurrentThread.GetApartmentState() == ApartmentState.STA) && (collections.Length > 0x3e)))
            {
                throw new ArgumentOutOfRangeException("collections", SR.GetString("BlockingCollection_ValidateCollectionsArray_LargeSize"));
            }
            BlockingCollection<T>[] blockingsArray = new BlockingCollection<T>[collections.Length];
            for (int i = 0; i < blockingsArray.Length; i++)
            {
                blockingsArray[i] = collections[i];
                if (blockingsArray[i] == null)
                {
                    throw new ArgumentException(SR.GetString("BlockingCollection_ValidateCollectionsArray_NullElems"), "collections");
                }
                if (blockingsArray[i].m_isDisposed)
                {
                    throw new ObjectDisposedException("collections", SR.GetString("BlockingCollection_ValidateCollectionsArray_DispElems"));
                }
                if ((operationMode == OperationMode<T>.Add) && blockingsArray[i].IsAddingCompleted)
                {
                    throw new ArgumentException(SR.GetString("BlockingCollection_CantTakeAnyWhenDone"), "collections");
                }
            }
            return blockingsArray;
        }

        private static void ValidateMillisecondsTimeout(int millisecondsTimeout)
        {
            if ((millisecondsTimeout < 0) && (millisecondsTimeout != -1))
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout", millisecondsTimeout, string.Format(CultureInfo.InvariantCulture, SR.GetString("BlockingCollection_TimeoutInvalid"), new object[] { 0x7fffffff }));
            }
        }

        private static void ValidateTimeout(TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if (((totalMilliseconds < 0L) || (totalMilliseconds > 0x7fffffffL)) && (totalMilliseconds != -1L))
            {
                throw new ArgumentOutOfRangeException("timeout", timeout, string.Format(CultureInfo.InvariantCulture, SR.GetString("BlockingCollection_TimeoutInvalid"), new object[] { 0x7fffffff }));
            }
        }

        private static int WaitHandle_WaitAny(List<WaitHandle> handles, int millisecondsTimeout, CancellationToken combinedToken, CancellationToken externalToken)
        {
            WaitHandle[] waitHandles = new WaitHandle[handles.Count + 1];
            for (int i = 0; i < handles.Count; i++)
            {
                waitHandles[i] = handles[i];
            }
            waitHandles[handles.Count] = combinedToken.WaitHandle;
            int num2 = WaitHandle.WaitAny(waitHandles, millisecondsTimeout, false);
            if (!combinedToken.IsCancellationRequested)
            {
                return num2;
            }
            if (externalToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(SR.GetString("Common_OperationCanceled"), externalToken);
            }
            throw new OperationCanceledException(SR.GetString("Common_OperationCanceled"));
        }

        public int BoundedCapacity
        {
            get
            {
                this.CheckDisposed();
                return this.m_boundedCapacity;
            }
        }

        public int Count
        {
            get
            {
                this.CheckDisposed();
                return this.m_occupiedNodes.CurrentCount;
            }
        }

        public bool IsAddingCompleted
        {
            get
            {
                this.CheckDisposed();
                return (this.m_currentAdders == -2147483648);
            }
        }

        public bool IsCompleted
        {
            get
            {
                this.CheckDisposed();
                return (this.IsAddingCompleted && (this.m_occupiedNodes.CurrentCount == 0));
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                this.CheckDisposed();
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                throw new NotSupportedException(SR.GetString("ConcurrentCollection_SyncRoot_NotSupported"));
            }
        }

        [CompilerGenerated]
        private sealed class <GetConsumingEnumerable>d__0 : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private T <>2__current;
            public CancellationToken <>3__cancellationToken;
            public BlockingCollection<T> <>4__this;
            private int <>l__initialThreadId;
            public T <item>5__2;
            public CancellationTokenSource <linkedTokenSource>5__1;
            public CancellationToken cancellationToken;

            [DebuggerHidden]
            public <GetConsumingEnumerable>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally3()
            {
                this.<>1__state = -1;
                if (this.<linkedTokenSource>5__1 != null)
                {
                    this.<linkedTokenSource>5__1.Dispose();
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
                            this.<linkedTokenSource>5__1 = null;
                            this.<>1__state = 1;
                            this.<linkedTokenSource>5__1 = CancellationTokenSource.CreateLinkedTokenSource(this.cancellationToken, this.<>4__this.m_ConsumersCancellationTokenSource.Token);
                            goto Label_0094;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_0094;

                        default:
                            goto Label_00A7;
                    }
                Label_0056:
                    if (this.<>4__this.TryTakeWithNoTimeValidation(out this.<item>5__2, -1, this.cancellationToken, this.<linkedTokenSource>5__1))
                    {
                        this.<>2__current = this.<item>5__2;
                        this.<>1__state = 2;
                        return true;
                    }
                Label_0094:
                    if (!this.<>4__this.IsCompleted)
                    {
                        goto Label_0056;
                    }
                    this.<>m__Finally3();
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
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                BlockingCollection<T>.<GetConsumingEnumerable>d__0 d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (BlockingCollection<T>.<GetConsumingEnumerable>d__0) this;
                }
                else
                {
                    d__ = new BlockingCollection<T>.<GetConsumingEnumerable>d__0(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.cancellationToken = this.<>3__cancellationToken;
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
                            this.<>m__Finally3();
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

        private enum OperationMode
        {
            public const BlockingCollection<T>.OperationMode Add = BlockingCollection<T>.OperationMode.Add;,
            public const BlockingCollection<T>.OperationMode Take = BlockingCollection<T>.OperationMode.Take;
        }
    }
}

