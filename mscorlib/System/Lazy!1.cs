namespace System
{
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable, DebuggerDisplay("ThreadSafetyMode={Mode}, IsValueCreated={IsValueCreated}, IsValueFaulted={IsValueFaulted}, Value={ValueForDebugDisplay}"), DebuggerTypeProxy(typeof(System_LazyDebugView<>)), ComVisible(false), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class Lazy<T>
    {
        private volatile object m_boxed;
        [NonSerialized]
        private readonly object m_threadSafeObj;
        [NonSerialized]
        private Func<T> m_valueFactory;
        private static Func<T> PUBLICATION_ONLY_OR_ALREADY_INITIALIZED;

        static Lazy()
        {
            Lazy<T>.PUBLICATION_ONLY_OR_ALREADY_INITIALIZED = () => default(T);
        }

        public Lazy() : this(LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }

        public Lazy(bool isThreadSafe) : this(isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
        {
        }

        public Lazy(LazyThreadSafetyMode mode)
        {
            this.m_threadSafeObj = Lazy<T>.GetObjectFromMode(mode);
        }

        public Lazy(Func<T> valueFactory) : this(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }

        public Lazy(Func<T> valueFactory, bool isThreadSafe) : this(valueFactory, isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
        {
        }

        public Lazy(Func<T> valueFactory, LazyThreadSafetyMode mode)
        {
            if (valueFactory == null)
            {
                throw new ArgumentNullException("valueFactory");
            }
            this.m_threadSafeObj = Lazy<T>.GetObjectFromMode(mode);
            this.m_valueFactory = valueFactory;
        }

        private Boxed<T> CreateValue()
        {
            Boxed<T> boxed = null;
            LazyThreadSafetyMode mode = this.Mode;
            if (this.m_valueFactory != null)
            {
                try
                {
                    if ((mode != LazyThreadSafetyMode.PublicationOnly) && (this.m_valueFactory == Lazy<T>.PUBLICATION_ONLY_OR_ALREADY_INITIALIZED))
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("Lazy_Value_RecursiveCallsToValue"));
                    }
                    Func<T> valueFactory = this.m_valueFactory;
                    if (mode != LazyThreadSafetyMode.PublicationOnly)
                    {
                        this.m_valueFactory = Lazy<T>.PUBLICATION_ONLY_OR_ALREADY_INITIALIZED;
                    }
                    return new Boxed<T>(valueFactory());
                }
                catch (Exception exception)
                {
                    if (mode != LazyThreadSafetyMode.PublicationOnly)
                    {
                        this.m_boxed = new LazyInternalExceptionHolder<T>(exception.PrepForRemoting());
                    }
                    throw;
                }
            }
            try
            {
                boxed = new Boxed<T>((T) Activator.CreateInstance(typeof(T)));
            }
            catch (MissingMethodException)
            {
                Exception ex = new MissingMemberException(Environment.GetResourceString("Lazy_CreateValue_NoParameterlessCtorForT"));
                if (mode != LazyThreadSafetyMode.PublicationOnly)
                {
                    this.m_boxed = new LazyInternalExceptionHolder<T>(ex);
                }
                throw ex;
            }
            return boxed;
        }

        private static object GetObjectFromMode(LazyThreadSafetyMode mode)
        {
            if (mode == LazyThreadSafetyMode.ExecutionAndPublication)
            {
                return new object();
            }
            if (mode == LazyThreadSafetyMode.PublicationOnly)
            {
                return Lazy<T>.PUBLICATION_ONLY_OR_ALREADY_INITIALIZED;
            }
            if (mode != LazyThreadSafetyMode.None)
            {
                throw new ArgumentOutOfRangeException("mode", Environment.GetResourceString("Lazy_ctor_ModeInvalid"));
            }
            return null;
        }

        private T LazyInitValue()
        {
            Boxed<T> boxed = null;
            switch (this.Mode)
            {
                case LazyThreadSafetyMode.None:
                    boxed = this.CreateValue();
                    this.m_boxed = boxed;
                    break;

                case LazyThreadSafetyMode.PublicationOnly:
                    boxed = this.CreateValue();
                    if (Interlocked.CompareExchange(ref this.m_boxed, boxed, null) != null)
                    {
                        boxed = (Boxed<T>) this.m_boxed;
                    }
                    break;

                default:
                    lock (this.m_threadSafeObj)
                    {
                        if (this.m_boxed == null)
                        {
                            boxed = this.CreateValue();
                            this.m_boxed = boxed;
                        }
                        else
                        {
                            boxed = this.m_boxed as Boxed<T>;
                            if (boxed == null)
                            {
                                LazyInternalExceptionHolder<T> holder = this.m_boxed as LazyInternalExceptionHolder<T>;
                                throw holder.m_exception;
                            }
                        }
                    }
                    break;
            }
            return boxed.m_value;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            T local1 = this.Value;
        }

        public override string ToString()
        {
            if (!this.IsValueCreated)
            {
                return Environment.GetResourceString("Lazy_ToString_ValueNotCreated");
            }
            return this.Value.ToString();
        }

        public bool IsValueCreated
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return ((this.m_boxed != null) && (this.m_boxed is Boxed<T>));
            }
        }

        internal bool IsValueFaulted
        {
            get
            {
                return (this.m_boxed is LazyInternalExceptionHolder<T>);
            }
        }

        internal LazyThreadSafetyMode Mode
        {
            get
            {
                if (this.m_threadSafeObj == null)
                {
                    return LazyThreadSafetyMode.None;
                }
                if (this.m_threadSafeObj == Lazy<T>.PUBLICATION_ONLY_OR_ALREADY_INITIALIZED)
                {
                    return LazyThreadSafetyMode.PublicationOnly;
                }
                return LazyThreadSafetyMode.ExecutionAndPublication;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public T Value
        {
            get
            {
                Boxed<T> boxed = null;
                if (this.m_boxed != null)
                {
                    boxed = this.m_boxed as Boxed<T>;
                    if (boxed != null)
                    {
                        return boxed.m_value;
                    }
                    LazyInternalExceptionHolder<T> holder = this.m_boxed as LazyInternalExceptionHolder<T>;
                    throw holder.m_exception;
                }
                Debugger.NotifyOfCrossThreadDependency();
                return this.LazyInitValue();
            }
        }

        internal T ValueForDebugDisplay
        {
            get
            {
                if (!this.IsValueCreated)
                {
                    return default(T);
                }
                return ((Boxed<T>) this.m_boxed).m_value;
            }
        }

        [Serializable]
        private class Boxed
        {
            internal T m_value;

            internal Boxed(T value)
            {
                this.m_value = value;
            }
        }

        private class LazyInternalExceptionHolder
        {
            internal Exception m_exception;

            internal LazyInternalExceptionHolder(Exception ex)
            {
                this.m_exception = ex;
            }
        }
    }
}

