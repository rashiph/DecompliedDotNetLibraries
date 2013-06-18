namespace System.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class InputQueue<T> : IDisposable where T: class
    {
        private static Action<object> completeOutstandingReadersCallback;
        private static Action<object> completeWaitersFalseCallback;
        private static Action<object> completeWaitersTrueCallback;
        private ItemQueue<T> itemQueue;
        private static Action<object> onDispatchCallback;
        private static Action<object> onInvokeDequeuedCallback;
        private QueueState<T> queueState;
        private Queue<IQueueReader<T>> readerQueue;
        private List<IQueueWaiter<T>> waiterList;

        public InputQueue()
        {
            this.itemQueue = new ItemQueue<T>();
            this.readerQueue = new Queue<IQueueReader<T>>();
            this.waiterList = new List<IQueueWaiter<T>>();
            this.queueState = QueueState<T>.Open;
        }

        public InputQueue(Func<Action<AsyncCallback, IAsyncResult>> asyncCallbackGenerator) : this()
        {
            this.AsyncCallbackGenerator = asyncCallbackGenerator;
        }

        public IAsyncResult BeginDequeue(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Item<T> item = new Item<T>();
            lock (this.ThisLock)
            {
                if (this.queueState == QueueState<T>.Open)
                {
                    if (!this.itemQueue.HasAvailableItem)
                    {
                        AsyncQueueReader<T> reader = new AsyncQueueReader<T>((InputQueue<T>) this, timeout, callback, state);
                        this.readerQueue.Enqueue(reader);
                        return reader;
                    }
                    item = this.itemQueue.DequeueAvailableItem();
                }
                else if (this.queueState == QueueState<T>.Shutdown)
                {
                    if (this.itemQueue.HasAvailableItem)
                    {
                        item = this.itemQueue.DequeueAvailableItem();
                    }
                    else if (this.itemQueue.HasAnyItem)
                    {
                        AsyncQueueReader<T> reader2 = new AsyncQueueReader<T>((InputQueue<T>) this, timeout, callback, state);
                        this.readerQueue.Enqueue(reader2);
                        return reader2;
                    }
                }
            }
            InputQueue<T>.InvokeDequeuedCallback(item.DequeuedCallback);
            return new CompletedAsyncResult<T>(item.GetValue(), callback, state);
        }

        public IAsyncResult BeginWaitForItem(TimeSpan timeout, AsyncCallback callback, object state)
        {
            lock (this.ThisLock)
            {
                if (this.queueState == QueueState<T>.Open)
                {
                    if (!this.itemQueue.HasAvailableItem)
                    {
                        AsyncQueueWaiter<T> item = new AsyncQueueWaiter<T>(timeout, callback, state);
                        this.waiterList.Add(item);
                        return item;
                    }
                }
                else if (((this.queueState == QueueState<T>.Shutdown) && !this.itemQueue.HasAvailableItem) && this.itemQueue.HasAnyItem)
                {
                    AsyncQueueWaiter<T> waiter2 = new AsyncQueueWaiter<T>(timeout, callback, state);
                    this.waiterList.Add(waiter2);
                    return waiter2;
                }
            }
            return new CompletedAsyncResult<bool>(true, callback, state);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Close()
        {
            this.Dispose();
        }

        private static void CompleteOutstandingReadersCallback(object state)
        {
            IQueueReader<T>[] readerArray = (IQueueReader<T>[]) state;
            for (int i = 0; i < readerArray.Length; i++)
            {
                Item<T> item = new Item<T>();
                readerArray[i].Set(item);
            }
        }

        private static void CompleteWaiters(bool itemAvailable, IQueueWaiter<T>[] waiters)
        {
            for (int i = 0; i < waiters.Length; i++)
            {
                waiters[i].Set(itemAvailable);
            }
        }

        private static void CompleteWaitersFalseCallback(object state)
        {
            InputQueue<T>.CompleteWaiters(false, (IQueueWaiter<T>[]) state);
        }

        private static void CompleteWaitersLater(bool itemAvailable, IQueueWaiter<T>[] waiters)
        {
            if (itemAvailable)
            {
                if (InputQueue<T>.completeWaitersTrueCallback == null)
                {
                    InputQueue<T>.completeWaitersTrueCallback = new Action<object>(InputQueue<T>.CompleteWaitersTrueCallback);
                }
                ActionItem.Schedule(InputQueue<T>.completeWaitersTrueCallback, waiters);
            }
            else
            {
                if (InputQueue<T>.completeWaitersFalseCallback == null)
                {
                    InputQueue<T>.completeWaitersFalseCallback = new Action<object>(InputQueue<T>.CompleteWaitersFalseCallback);
                }
                ActionItem.Schedule(InputQueue<T>.completeWaitersFalseCallback, waiters);
            }
        }

        private static void CompleteWaitersTrueCallback(object state)
        {
            InputQueue<T>.CompleteWaiters(true, (IQueueWaiter<T>[]) state);
        }

        public T Dequeue(TimeSpan timeout)
        {
            T local;
            if (!this.Dequeue(timeout, out local))
            {
                throw Fx.Exception.AsError(new TimeoutException(SRCore.TimeoutInputQueueDequeue(timeout)));
            }
            return local;
        }

        public bool Dequeue(TimeSpan timeout, out T value)
        {
            WaitQueueReader<T> reader = null;
            Item<T> item = new Item<T>();
            lock (this.ThisLock)
            {
                if (this.queueState == QueueState<T>.Open)
                {
                    if (this.itemQueue.HasAvailableItem)
                    {
                        item = this.itemQueue.DequeueAvailableItem();
                    }
                    else
                    {
                        reader = new WaitQueueReader<T>((InputQueue<T>) this);
                        this.readerQueue.Enqueue(reader);
                    }
                }
                else if (this.queueState == QueueState<T>.Shutdown)
                {
                    if (!this.itemQueue.HasAvailableItem)
                    {
                        if (!this.itemQueue.HasAnyItem)
                        {
                            value = default(T);
                            return true;
                        }
                        reader = new WaitQueueReader<T>((InputQueue<T>) this);
                        this.readerQueue.Enqueue(reader);
                    }
                    else
                    {
                        item = this.itemQueue.DequeueAvailableItem();
                    }
                }
                else
                {
                    value = default(T);
                    return true;
                }
            }
            if (reader != null)
            {
                return reader.Wait(timeout, out value);
            }
            InputQueue<T>.InvokeDequeuedCallback(item.DequeuedCallback);
            value = item.GetValue();
            return true;
        }

        public void Dispatch()
        {
            IQueueReader<T> reader = null;
            Item<T> item = new Item<T>();
            IQueueReader<T>[] array = null;
            IQueueWaiter<T>[] waiters = null;
            bool itemAvailable = true;
            lock (this.ThisLock)
            {
                itemAvailable = (this.queueState != QueueState<T>.Closed) && (this.queueState != QueueState<T>.Shutdown);
                this.GetWaiters(out waiters);
                if (this.queueState != QueueState<T>.Closed)
                {
                    this.itemQueue.MakePendingItemAvailable();
                    if (this.readerQueue.Count > 0)
                    {
                        item = this.itemQueue.DequeueAvailableItem();
                        reader = this.readerQueue.Dequeue();
                        if (((this.queueState == QueueState<T>.Shutdown) && (this.readerQueue.Count > 0)) && (this.itemQueue.ItemCount == 0))
                        {
                            array = new IQueueReader<T>[this.readerQueue.Count];
                            this.readerQueue.CopyTo(array, 0);
                            this.readerQueue.Clear();
                            itemAvailable = false;
                        }
                    }
                }
            }
            if (array != null)
            {
                if (InputQueue<T>.completeOutstandingReadersCallback == null)
                {
                    InputQueue<T>.completeOutstandingReadersCallback = new Action<object>(InputQueue<T>.CompleteOutstandingReadersCallback);
                }
                ActionItem.Schedule(InputQueue<T>.completeOutstandingReadersCallback, array);
            }
            if (waiters != null)
            {
                InputQueue<T>.CompleteWaitersLater(itemAvailable, waiters);
            }
            if (reader != null)
            {
                InputQueue<T>.InvokeDequeuedCallback(item.DequeuedCallback);
                reader.Set(item);
            }
        }

        public void Dispose()
        {
            bool flag = false;
            lock (this.ThisLock)
            {
                if (this.queueState != QueueState<T>.Closed)
                {
                    this.queueState = QueueState<T>.Closed;
                    flag = true;
                }
            }
            if (flag)
            {
                while (this.readerQueue.Count > 0)
                {
                    Item<T> item2 = new Item<T>();
                    this.readerQueue.Dequeue().Set(item2);
                }
                while (this.itemQueue.HasAnyItem)
                {
                    Item<T> item = this.itemQueue.DequeueAnyItem();
                    this.DisposeItem(item);
                    InputQueue<T>.InvokeDequeuedCallback(item.DequeuedCallback);
                }
            }
        }

        private void DisposeItem(Item<T> item)
        {
            T local = item.Value;
            if (local != null)
            {
                if (local is IDisposable)
                {
                    ((IDisposable) local).Dispose();
                }
                else
                {
                    Action<T> disposeItemCallback = this.DisposeItemCallback;
                    if (disposeItemCallback != null)
                    {
                        disposeItemCallback(local);
                    }
                }
            }
        }

        public T EndDequeue(IAsyncResult result)
        {
            T local;
            if (!this.EndDequeue(result, out local))
            {
                throw Fx.Exception.AsError(new TimeoutException());
            }
            return local;
        }

        public bool EndDequeue(IAsyncResult result, out T value)
        {
            if (result is CompletedAsyncResult<T>)
            {
                value = CompletedAsyncResult<T>.End(result);
                return true;
            }
            return AsyncQueueReader<T>.End(result, out value);
        }

        public bool EndWaitForItem(IAsyncResult result)
        {
            if (result is CompletedAsyncResult<bool>)
            {
                return CompletedAsyncResult<bool>.End(result);
            }
            return AsyncQueueWaiter<T>.End(result);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void EnqueueAndDispatch(T item)
        {
            this.EnqueueAndDispatch(item, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void EnqueueAndDispatch(T item, Action dequeuedCallback)
        {
            this.EnqueueAndDispatch(item, dequeuedCallback, true);
        }

        private void EnqueueAndDispatch(Item<T> item, bool canDispatchOnThisThread)
        {
            bool flag = false;
            IQueueReader<T> reader = null;
            bool flag2 = false;
            IQueueWaiter<T>[] waiters = null;
            bool itemAvailable = true;
            lock (this.ThisLock)
            {
                itemAvailable = (this.queueState != QueueState<T>.Closed) && (this.queueState != QueueState<T>.Shutdown);
                this.GetWaiters(out waiters);
                if (this.queueState == QueueState<T>.Open)
                {
                    if (canDispatchOnThisThread)
                    {
                        if (this.readerQueue.Count == 0)
                        {
                            this.itemQueue.EnqueueAvailableItem(item);
                        }
                        else
                        {
                            reader = this.readerQueue.Dequeue();
                        }
                    }
                    else if (this.readerQueue.Count == 0)
                    {
                        this.itemQueue.EnqueueAvailableItem(item);
                    }
                    else
                    {
                        this.itemQueue.EnqueuePendingItem(item);
                        flag2 = true;
                    }
                }
                else
                {
                    flag = true;
                }
            }
            if (waiters != null)
            {
                if (canDispatchOnThisThread)
                {
                    InputQueue<T>.CompleteWaiters(itemAvailable, waiters);
                }
                else
                {
                    InputQueue<T>.CompleteWaitersLater(itemAvailable, waiters);
                }
            }
            if (reader != null)
            {
                InputQueue<T>.InvokeDequeuedCallback(item.DequeuedCallback);
                reader.Set(item);
            }
            if (flag2)
            {
                if (InputQueue<T>.onDispatchCallback == null)
                {
                    InputQueue<T>.onDispatchCallback = new Action<object>(InputQueue<T>.OnDispatchCallback);
                }
                ActionItem.Schedule(InputQueue<T>.onDispatchCallback, this);
            }
            else if (flag)
            {
                InputQueue<T>.InvokeDequeuedCallback(item.DequeuedCallback);
                this.DisposeItem(item);
            }
        }

        public void EnqueueAndDispatch(Exception exception, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            this.EnqueueAndDispatch(new Item<T>(exception, dequeuedCallback), canDispatchOnThisThread);
        }

        public void EnqueueAndDispatch(T item, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            this.EnqueueAndDispatch(new Item<T>(item, dequeuedCallback), canDispatchOnThisThread);
        }

        private bool EnqueueWithoutDispatch(Item<T> item)
        {
            lock (this.ThisLock)
            {
                if ((this.queueState != QueueState<T>.Closed) && (this.queueState != QueueState<T>.Shutdown))
                {
                    if ((this.readerQueue.Count == 0) && (this.waiterList.Count == 0))
                    {
                        this.itemQueue.EnqueueAvailableItem(item);
                        return false;
                    }
                    this.itemQueue.EnqueuePendingItem(item);
                    return true;
                }
            }
            this.DisposeItem(item);
            InputQueue<T>.InvokeDequeuedCallbackLater(item.DequeuedCallback);
            return false;
        }

        public bool EnqueueWithoutDispatch(T item, Action dequeuedCallback)
        {
            return this.EnqueueWithoutDispatch(new Item<T>(item, dequeuedCallback));
        }

        public bool EnqueueWithoutDispatch(Exception exception, Action dequeuedCallback)
        {
            return this.EnqueueWithoutDispatch(new Item<T>(exception, dequeuedCallback));
        }

        private void GetWaiters(out IQueueWaiter<T>[] waiters)
        {
            if (this.waiterList.Count > 0)
            {
                waiters = this.waiterList.ToArray();
                this.waiterList.Clear();
            }
            else
            {
                waiters = null;
            }
        }

        private static void InvokeDequeuedCallback(Action dequeuedCallback)
        {
            if (dequeuedCallback != null)
            {
                dequeuedCallback();
            }
        }

        private static void InvokeDequeuedCallbackLater(Action dequeuedCallback)
        {
            if (dequeuedCallback != null)
            {
                if (InputQueue<T>.onInvokeDequeuedCallback == null)
                {
                    InputQueue<T>.onInvokeDequeuedCallback = new Action<object>(InputQueue<T>.OnInvokeDequeuedCallback);
                }
                ActionItem.Schedule(InputQueue<T>.onInvokeDequeuedCallback, dequeuedCallback);
            }
        }

        private static void OnDispatchCallback(object state)
        {
            ((InputQueue<T>) state).Dispatch();
        }

        private static void OnInvokeDequeuedCallback(object state)
        {
            Action action = (Action) state;
            action();
        }

        private bool RemoveReader(IQueueReader<T> reader)
        {
            lock (this.ThisLock)
            {
                if ((this.queueState == QueueState<T>.Open) || (this.queueState == QueueState<T>.Shutdown))
                {
                    bool flag = false;
                    for (int i = this.readerQueue.Count; i > 0; i--)
                    {
                        IQueueReader<T> objA = this.readerQueue.Dequeue();
                        if (object.ReferenceEquals(objA, reader))
                        {
                            flag = true;
                        }
                        else
                        {
                            this.readerQueue.Enqueue(objA);
                        }
                    }
                    return flag;
                }
            }
            return false;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Shutdown()
        {
            this.Shutdown(null);
        }

        public void Shutdown(Func<Exception> pendingExceptionGenerator)
        {
            IQueueReader<T>[] array = null;
            lock (this.ThisLock)
            {
                if ((this.queueState == QueueState<T>.Shutdown) || (this.queueState == QueueState<T>.Closed))
                {
                    return;
                }
                this.queueState = QueueState<T>.Shutdown;
                if ((this.readerQueue.Count > 0) && (this.itemQueue.ItemCount == 0))
                {
                    array = new IQueueReader<T>[this.readerQueue.Count];
                    this.readerQueue.CopyTo(array, 0);
                    this.readerQueue.Clear();
                }
            }
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    Exception exception = (pendingExceptionGenerator != null) ? pendingExceptionGenerator() : null;
                    array[i].Set(new Item<T>(exception, null));
                }
            }
        }

        public bool WaitForItem(TimeSpan timeout)
        {
            WaitQueueWaiter<T> item = null;
            bool flag = false;
            lock (this.ThisLock)
            {
                if (this.queueState == QueueState<T>.Open)
                {
                    if (this.itemQueue.HasAvailableItem)
                    {
                        flag = true;
                    }
                    else
                    {
                        item = new WaitQueueWaiter<T>();
                        this.waiterList.Add(item);
                    }
                }
                else if (this.queueState == QueueState<T>.Shutdown)
                {
                    if (!this.itemQueue.HasAvailableItem)
                    {
                        if (!this.itemQueue.HasAnyItem)
                        {
                            return true;
                        }
                        item = new WaitQueueWaiter<T>();
                        this.waiterList.Add(item);
                    }
                    else
                    {
                        flag = true;
                    }
                }
                else
                {
                    return true;
                }
            }
            if (item != null)
            {
                return item.Wait(timeout);
            }
            return flag;
        }

        private Func<Action<AsyncCallback, IAsyncResult>> AsyncCallbackGenerator
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<AsyncCallbackGenerator>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<AsyncCallbackGenerator>k__BackingField = value;
            }
        }

        public Action<T> DisposeItemCallback
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<DisposeItemCallback>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<DisposeItemCallback>k__BackingField = value;
            }
        }

        public int PendingCount
        {
            get
            {
                lock (this.ThisLock)
                {
                    return this.itemQueue.ItemCount;
                }
            }
        }

        private object ThisLock
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.itemQueue;
            }
        }

        private class AsyncQueueReader : AsyncResult, InputQueue<T>.IQueueReader
        {
            private bool expired;
            private InputQueue<T> inputQueue;
            private T item;
            private IOThreadTimer timer;
            private static Action<object> timerCallback;

            static AsyncQueueReader()
            {
                InputQueue<T>.AsyncQueueReader.timerCallback = new Action<object>(InputQueue<T>.AsyncQueueReader.TimerCallback);
            }

            public AsyncQueueReader(InputQueue<T> inputQueue, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                if (inputQueue.AsyncCallbackGenerator != null)
                {
                    base.VirtualCallback = inputQueue.AsyncCallbackGenerator();
                }
                this.inputQueue = inputQueue;
                if (timeout != TimeSpan.MaxValue)
                {
                    this.timer = new IOThreadTimer(InputQueue<T>.AsyncQueueReader.timerCallback, this, false);
                    this.timer.Set(timeout);
                }
            }

            public static bool End(IAsyncResult result, out T value)
            {
                InputQueue<T>.AsyncQueueReader reader = AsyncResult.End<InputQueue<T>.AsyncQueueReader>(result);
                if (reader.expired)
                {
                    value = default(T);
                    return false;
                }
                value = reader.item;
                return true;
            }

            public void Set(InputQueue<T>.Item item)
            {
                this.item = item.Value;
                if (this.timer != null)
                {
                    this.timer.Cancel();
                }
                base.Complete(false, item.Exception);
            }

            private static void TimerCallback(object state)
            {
                InputQueue<T>.AsyncQueueReader reader = (InputQueue<T>.AsyncQueueReader) state;
                if (reader.inputQueue.RemoveReader(reader))
                {
                    reader.expired = true;
                    reader.Complete(false);
                }
            }
        }

        private class AsyncQueueWaiter : AsyncResult, InputQueue<T>.IQueueWaiter
        {
            private bool itemAvailable;
            private object thisLock;
            private IOThreadTimer timer;
            private static Action<object> timerCallback;

            static AsyncQueueWaiter()
            {
                InputQueue<T>.AsyncQueueWaiter.timerCallback = new Action<object>(InputQueue<T>.AsyncQueueWaiter.TimerCallback);
            }

            public AsyncQueueWaiter(TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.thisLock = new object();
                if (timeout != TimeSpan.MaxValue)
                {
                    this.timer = new IOThreadTimer(InputQueue<T>.AsyncQueueWaiter.timerCallback, this, false);
                    this.timer.Set(timeout);
                }
            }

            public static bool End(IAsyncResult result)
            {
                return AsyncResult.End<InputQueue<T>.AsyncQueueWaiter>(result).itemAvailable;
            }

            public void Set(bool itemAvailable)
            {
                bool flag;
                lock (this.ThisLock)
                {
                    flag = (this.timer == null) || this.timer.Cancel();
                    this.itemAvailable = itemAvailable;
                }
                if (flag)
                {
                    base.Complete(false);
                }
            }

            private static void TimerCallback(object state)
            {
                ((InputQueue<T>.AsyncQueueWaiter) state).Complete(false);
            }

            private object ThisLock
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.thisLock;
                }
            }
        }

        private interface IQueueReader
        {
            void Set(InputQueue<T>.Item item);
        }

        private interface IQueueWaiter
        {
            void Set(bool itemAvailable);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Item
        {
            private Action dequeuedCallback;
            private System.Exception exception;
            private T value;
            public Item(T value, Action dequeuedCallback) : this(value, null, dequeuedCallback)
            {
            }

            public Item(System.Exception exception, Action dequeuedCallback) : this(default(T), exception, dequeuedCallback)
            {
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            private Item(T value, System.Exception exception, Action dequeuedCallback)
            {
                this.value = value;
                this.exception = exception;
                this.dequeuedCallback = dequeuedCallback;
            }

            public Action DequeuedCallback
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.dequeuedCallback;
                }
            }
            public System.Exception Exception
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.exception;
                }
            }
            public T Value
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.value;
                }
            }
            public T GetValue()
            {
                if (this.exception != null)
                {
                    throw Fx.Exception.AsError(this.exception);
                }
                return this.value;
            }
        }

        private class ItemQueue
        {
            private int head;
            private InputQueue<T>.Item[] items;
            private int pendingCount;
            private int totalCount;

            public ItemQueue()
            {
                this.items = new InputQueue<T>.Item[1];
            }

            public InputQueue<T>.Item DequeueAnyItem()
            {
                if (this.pendingCount == this.totalCount)
                {
                    this.pendingCount--;
                }
                return this.DequeueItemCore();
            }

            public InputQueue<T>.Item DequeueAvailableItem()
            {
                Fx.AssertAndThrow(this.totalCount != this.pendingCount, "ItemQueue does not contain any available items");
                return this.DequeueItemCore();
            }

            private InputQueue<T>.Item DequeueItemCore()
            {
                Fx.AssertAndThrow(this.totalCount != 0, "ItemQueue does not contain any items");
                InputQueue<T>.Item item = this.items[this.head];
                this.items[this.head] = new InputQueue<T>.Item();
                this.totalCount--;
                this.head = (this.head + 1) % this.items.Length;
                return item;
            }

            public void EnqueueAvailableItem(InputQueue<T>.Item item)
            {
                this.EnqueueItemCore(item);
            }

            private void EnqueueItemCore(InputQueue<T>.Item item)
            {
                if (this.totalCount == this.items.Length)
                {
                    InputQueue<T>.Item[] itemArray = new InputQueue<T>.Item[this.items.Length * 2];
                    for (int i = 0; i < this.totalCount; i++)
                    {
                        itemArray[i] = this.items[(this.head + i) % this.items.Length];
                    }
                    this.head = 0;
                    this.items = itemArray;
                }
                int index = (this.head + this.totalCount) % this.items.Length;
                this.items[index] = item;
                this.totalCount++;
            }

            public void EnqueuePendingItem(InputQueue<T>.Item item)
            {
                this.EnqueueItemCore(item);
                this.pendingCount++;
            }

            public void MakePendingItemAvailable()
            {
                Fx.AssertAndThrow(this.pendingCount != 0, "ItemQueue does not contain any pending items");
                this.pendingCount--;
            }

            public bool HasAnyItem
            {
                get
                {
                    return (this.totalCount > 0);
                }
            }

            public bool HasAvailableItem
            {
                get
                {
                    return (this.totalCount > this.pendingCount);
                }
            }

            public int ItemCount
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.totalCount;
                }
            }
        }

        private enum QueueState
        {
            public const InputQueue<T>.QueueState Closed = InputQueue<T>.QueueState.Closed;,
            public const InputQueue<T>.QueueState Open = InputQueue<T>.QueueState.Open;,
            public const InputQueue<T>.QueueState Shutdown = InputQueue<T>.QueueState.Shutdown;
        }

        private class WaitQueueReader : InputQueue<T>.IQueueReader
        {
            private Exception exception;
            private InputQueue<T> inputQueue;
            private T item;
            private ManualResetEvent waitEvent;

            public WaitQueueReader(InputQueue<T> inputQueue)
            {
                this.inputQueue = inputQueue;
                this.waitEvent = new ManualResetEvent(false);
            }

            public void Set(InputQueue<T>.Item item)
            {
                lock (((InputQueue<T>.WaitQueueReader) this))
                {
                    this.exception = item.Exception;
                    this.item = item.Value;
                    this.waitEvent.Set();
                }
            }

            public bool Wait(TimeSpan timeout, out T value)
            {
                bool flag = false;
                try
                {
                    if (!TimeoutHelper.WaitOne(this.waitEvent, timeout))
                    {
                        if (this.inputQueue.RemoveReader(this))
                        {
                            value = default(T);
                            flag = true;
                            return false;
                        }
                        this.waitEvent.WaitOne();
                    }
                    flag = true;
                }
                finally
                {
                    if (flag)
                    {
                        this.waitEvent.Close();
                    }
                }
                if (this.exception != null)
                {
                    throw Fx.Exception.AsError(this.exception);
                }
                value = this.item;
                return true;
            }
        }

        private class WaitQueueWaiter : InputQueue<T>.IQueueWaiter
        {
            private bool itemAvailable;
            private ManualResetEvent waitEvent;

            public WaitQueueWaiter()
            {
                this.waitEvent = new ManualResetEvent(false);
            }

            public void Set(bool itemAvailable)
            {
                lock (((InputQueue<T>.WaitQueueWaiter) this))
                {
                    this.itemAvailable = itemAvailable;
                    this.waitEvent.Set();
                }
            }

            public bool Wait(TimeSpan timeout)
            {
                if (!TimeoutHelper.WaitOne(this.waitEvent, timeout))
                {
                    return false;
                }
                return this.itemAvailable;
            }
        }
    }
}

