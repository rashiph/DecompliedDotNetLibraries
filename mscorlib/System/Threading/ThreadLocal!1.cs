namespace System.Threading
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Security;
    using System.Security.Permissions;

    [DebuggerTypeProxy(typeof(SystemThreading_ThreadLocalDebugView<>)), DebuggerDisplay("IsValueCreated={IsValueCreated}, Value={ValueForDebugDisplay}"), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class ThreadLocal<T> : IDisposable
    {
        private int m_currentInstanceIndex;
        private HolderBase<T> m_holder;
        private Func<T> m_valueFactory;
        internal static int MAXIMUM_TYPES_LENGTH;
        private static ConcurrentStack<int> s_availableIndices;
        private static int s_currentTypeId;
        private static Type[] s_dummyTypes;
        private static int TYPE_DIMENSIONS;

        static ThreadLocal()
        {
            ThreadLocal<T>.s_dummyTypes = new Type[] { typeof(C0<T>), typeof(C1<T>), typeof(C2<T>), typeof(C3<T>), typeof(C4<T>), typeof(C5<T>), typeof(C6<T>), typeof(C7<T>), typeof(C8<T>), typeof(C9<T>), typeof(C10<T>), typeof(C11<T>), typeof(C12<T>), typeof(C13<T>), typeof(C14<T>), typeof(C15<T>) };
            ThreadLocal<T>.s_currentTypeId = -1;
            ThreadLocal<T>.s_availableIndices = new ConcurrentStack<int>();
            ThreadLocal<T>.TYPE_DIMENSIONS = typeof(GenericHolder).GetGenericArguments().Length;
            ThreadLocal<T>.MAXIMUM_TYPES_LENGTH = (int) Math.Pow((double) ThreadLocal<T>.s_dummyTypes.Length, (double) (ThreadLocal<T>.TYPE_DIMENSIONS - 1));
        }

        [SecuritySafeCritical]
        public ThreadLocal()
        {
            if (this.FindNextTypeIndex())
            {
                Type[] typesFromIndex = this.GetTypesFromIndex();
                new PermissionSet(PermissionState.Unrestricted).Assert();
                try
                {
                    this.m_holder = (HolderBase<T>) Activator.CreateInstance(typeof(GenericHolder).MakeGenericType(typesFromIndex));
                }
                finally
                {
                    PermissionSet.RevertAssert();
                }
            }
            else
            {
                this.m_holder = new TLSHolder<T>();
            }
        }

        public ThreadLocal(Func<T> valueFactory) : this()
        {
            if (valueFactory == null)
            {
                throw new ArgumentNullException("valueFactory");
            }
            this.m_valueFactory = valueFactory;
        }

        private Boxed<T> CreateValue()
        {
            Boxed<T> boxed = new Boxed<T> {
                m_ownerHolder = this.m_holder,
                Value = (this.m_valueFactory == null) ? default(T) : this.m_valueFactory()
            };
            if ((this.m_holder.Boxed != null) && (this.m_holder.Boxed.m_ownerHolder == this.m_holder))
            {
                throw new InvalidOperationException(Environment.GetResourceString("ThreadLocal_Value_RecursiveCallsToValue"));
            }
            this.m_holder.Boxed = boxed;
            return boxed;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            int currentInstanceIndex = this.m_currentInstanceIndex;
            if ((currentInstanceIndex > -1) && (Interlocked.CompareExchange(ref this.m_currentInstanceIndex, -1, currentInstanceIndex) == currentInstanceIndex))
            {
                ThreadLocal<T>.s_availableIndices.Push(currentInstanceIndex);
            }
            this.m_holder = null;
        }

        ~ThreadLocal()
        {
            this.Dispose(false);
        }

        private bool FindNextTypeIndex()
        {
            int result = -1;
            if (ThreadLocal<T>.s_availableIndices.TryPop(out result))
            {
                this.m_currentInstanceIndex = result;
                return true;
            }
            if (((ThreadLocal<T>.s_currentTypeId < (ThreadLocal<T>.MAXIMUM_TYPES_LENGTH - 1)) && (ThreadLocalGlobalCounter.s_fastPathCount < ThreadLocalGlobalCounter.MAXIMUM_GLOBAL_COUNT)) && (Interlocked.Increment(ref ThreadLocalGlobalCounter.s_fastPathCount) <= ThreadLocalGlobalCounter.MAXIMUM_GLOBAL_COUNT))
            {
                result = Interlocked.Increment(ref ThreadLocal<T>.s_currentTypeId);
                if (result < ThreadLocal<T>.MAXIMUM_TYPES_LENGTH)
                {
                    this.m_currentInstanceIndex = result;
                    return true;
                }
            }
            this.m_currentInstanceIndex = -1;
            return false;
        }

        private Type[] GetTypesFromIndex()
        {
            Type[] typeArray = new Type[ThreadLocal<T>.TYPE_DIMENSIONS];
            typeArray[0] = typeof(T);
            int currentInstanceIndex = this.m_currentInstanceIndex;
            for (int i = 1; i < ThreadLocal<T>.TYPE_DIMENSIONS; i++)
            {
                typeArray[i] = ThreadLocal<T>.s_dummyTypes[currentInstanceIndex % ThreadLocal<T>.s_dummyTypes.Length];
                currentInstanceIndex /= ThreadLocal<T>.s_dummyTypes.Length;
            }
            return typeArray;
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }

        public bool IsValueCreated
        {
            get
            {
                if (this.m_holder == null)
                {
                    throw new ObjectDisposedException(Environment.GetResourceString("ThreadLocal_Disposed"));
                }
                Boxed<T> boxed = this.m_holder.Boxed;
                return ((boxed != null) && (boxed.m_ownerHolder == this.m_holder));
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public T Value
        {
            get
            {
                if (this.m_holder == null)
                {
                    throw new ObjectDisposedException(Environment.GetResourceString("ThreadLocal_Disposed"));
                }
                Boxed<T> boxed = this.m_holder.Boxed;
                if ((boxed == null) || (boxed.m_ownerHolder != this.m_holder))
                {
                    Debugger.NotifyOfCrossThreadDependency();
                    boxed = this.CreateValue();
                }
                return boxed.Value;
            }
            set
            {
                if (this.m_holder == null)
                {
                    throw new ObjectDisposedException(Environment.GetResourceString("ThreadLocal_Disposed"));
                }
                Boxed<T> boxed = this.m_holder.Boxed;
                if ((boxed != null) && (boxed.m_ownerHolder == this.m_holder))
                {
                    boxed.Value = value;
                }
                else
                {
                    Boxed<T> boxed2 = new Boxed<T> {
                        Value = value,
                        m_ownerHolder = this.m_holder
                    };
                    this.m_holder.Boxed = boxed2;
                }
            }
        }

        internal T ValueForDebugDisplay
        {
            get
            {
                if (((this.m_holder != null) && (this.m_holder.Boxed != null)) && (this.m_holder.Boxed.m_ownerHolder == this.m_holder))
                {
                    return this.m_holder.Boxed.Value;
                }
                return default(T);
            }
        }

        private class Boxed
        {
            internal ThreadLocal<T>.HolderBase m_ownerHolder;
            internal T Value;
        }

        private class C0
        {
        }

        private class C1
        {
        }

        private class C10
        {
        }

        private class C11
        {
        }

        private class C12
        {
        }

        private class C13
        {
        }

        private class C14
        {
        }

        private class C15
        {
        }

        private class C2
        {
        }

        private class C3
        {
        }

        private class C4
        {
        }

        private class C5
        {
        }

        private class C6
        {
        }

        private class C7
        {
        }

        private class C8
        {
        }

        private class C9
        {
        }

        private sealed class GenericHolder<U, V, W> : ThreadLocal<T>.HolderBase
        {
            [ThreadStatic]
            private static ThreadLocal<T>.Boxed s_value;

            internal override ThreadLocal<T>.Boxed Boxed
            {
                get
                {
                    return ThreadLocal<T>.GenericHolder<U, V, W>.s_value;
                }
                set
                {
                    ThreadLocal<T>.GenericHolder<U, V, W>.s_value = value;
                }
            }
        }

        private abstract class HolderBase
        {
            protected HolderBase()
            {
            }

            internal abstract ThreadLocal<T>.Boxed Boxed { get; set; }
        }

        private sealed class TLSHolder : ThreadLocal<T>.HolderBase
        {
            private LocalDataStoreSlot m_slot;

            public TLSHolder()
            {
                this.m_slot = Thread.AllocateDataSlot();
            }

            internal override ThreadLocal<T>.Boxed Boxed
            {
                get
                {
                    return (ThreadLocal<T>.Boxed) Thread.GetData(this.m_slot);
                }
                set
                {
                    Thread.SetData(this.m_slot, value);
                }
            }
        }
    }
}

