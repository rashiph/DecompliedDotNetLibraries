namespace System.Internal
{
    using System;
    using System.Threading;

    internal sealed class HandleCollector
    {
        private static int handleTypeCount;
        private static HandleType[] handleTypes;
        private static object internalSyncObject = new object();
        private static int suspendCount;

        internal static  event System.Internal.HandleChangeEventHandler HandleAdded;

        internal static  event System.Internal.HandleChangeEventHandler HandleRemoved;

        internal static IntPtr Add(IntPtr handle, int type)
        {
            handleTypes[type - 1].Add(handle);
            return handle;
        }

        internal static int RegisterType(string typeName, int expense, int initialThreshold)
        {
            lock (internalSyncObject)
            {
                if ((handleTypeCount == 0) || (handleTypeCount == handleTypes.Length))
                {
                    HandleType[] destinationArray = new HandleType[handleTypeCount + 10];
                    if (handleTypes != null)
                    {
                        Array.Copy(handleTypes, 0, destinationArray, 0, handleTypeCount);
                    }
                    handleTypes = destinationArray;
                }
                handleTypes[handleTypeCount++] = new HandleType(typeName, expense, initialThreshold);
                return handleTypeCount;
            }
        }

        internal static IntPtr Remove(IntPtr handle, int type)
        {
            return handleTypes[type - 1].Remove(handle);
        }

        internal static void ResumeCollect()
        {
            bool flag = false;
            lock (internalSyncObject)
            {
                if (suspendCount > 0)
                {
                    suspendCount--;
                }
                if (suspendCount == 0)
                {
                    for (int i = 0; i < handleTypeCount; i++)
                    {
                        lock (handleTypes[i])
                        {
                            if (handleTypes[i].NeedCollection())
                            {
                                flag = true;
                            }
                        }
                    }
                }
            }
            if (flag)
            {
                GC.Collect();
            }
        }

        internal static void SuspendCollect()
        {
            lock (internalSyncObject)
            {
                suspendCount++;
            }
        }

        private class HandleType
        {
            private readonly int deltaPercent;
            private int handleCount;
            private int initialThreshHold;
            internal readonly string name;
            private int threshHold;

            internal HandleType(string name, int expense, int initialThreshHold)
            {
                this.name = name;
                this.initialThreshHold = initialThreshHold;
                this.threshHold = initialThreshHold;
                this.deltaPercent = 100 - expense;
            }

            internal void Add(IntPtr handle)
            {
                if (handle != IntPtr.Zero)
                {
                    bool flag = false;
                    int currentHandleCount = 0;
                    lock (this)
                    {
                        this.handleCount++;
                        flag = this.NeedCollection();
                        currentHandleCount = this.handleCount;
                    }
                    lock (System.Internal.HandleCollector.internalSyncObject)
                    {
                        if (System.Internal.HandleCollector.HandleAdded != null)
                        {
                            System.Internal.HandleCollector.HandleAdded(this.name, handle, currentHandleCount);
                        }
                    }
                    if (flag && flag)
                    {
                        GC.Collect();
                        int millisecondsTimeout = (100 - this.deltaPercent) / 4;
                        Thread.Sleep(millisecondsTimeout);
                    }
                }
            }

            internal int GetHandleCount()
            {
                lock (this)
                {
                    return this.handleCount;
                }
            }

            internal bool NeedCollection()
            {
                if (System.Internal.HandleCollector.suspendCount <= 0)
                {
                    if (this.handleCount > this.threshHold)
                    {
                        this.threshHold = this.handleCount + ((this.handleCount * this.deltaPercent) / 100);
                        return true;
                    }
                    int num = (100 * this.threshHold) / (100 + this.deltaPercent);
                    if ((num >= this.initialThreshHold) && (this.handleCount < ((int) (num * 0.9f))))
                    {
                        this.threshHold = num;
                    }
                }
                return false;
            }

            internal IntPtr Remove(IntPtr handle)
            {
                if (handle != IntPtr.Zero)
                {
                    int currentHandleCount = 0;
                    lock (this)
                    {
                        this.handleCount--;
                        if (this.handleCount < 0)
                        {
                            this.handleCount = 0;
                        }
                        currentHandleCount = this.handleCount;
                    }
                    lock (System.Internal.HandleCollector.internalSyncObject)
                    {
                        if (System.Internal.HandleCollector.HandleRemoved != null)
                        {
                            System.Internal.HandleCollector.HandleRemoved(this.name, handle, currentHandleCount);
                        }
                    }
                }
                return handle;
            }
        }
    }
}

