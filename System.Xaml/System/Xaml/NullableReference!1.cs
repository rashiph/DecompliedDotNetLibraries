namespace System.Xaml
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NullableReference<T> where T: class
    {
        private static object s_NullSentinel;
        private static object s_NotPresentSentinel;
        private object _value;
        public bool IsNotPresent
        {
            get
            {
                return object.ReferenceEquals(this._value, NullableReference<T>.s_NotPresentSentinel);
            }
            set
            {
                this._value = value ? NullableReference<T>.s_NotPresentSentinel : null;
            }
        }
        public bool IsSet
        {
            get
            {
                return !object.ReferenceEquals(this._value, null);
            }
        }
        public bool IsSetVolatile
        {
            get
            {
                return !object.ReferenceEquals(Thread.VolatileRead(ref this._value), null);
            }
        }
        public T Value
        {
            get
            {
                object objA = this._value;
                if (!object.ReferenceEquals(objA, NullableReference<T>.s_NullSentinel))
                {
                    return (T) objA;
                }
                return default(T);
            }
            set
            {
                this._value = object.ReferenceEquals(value, null) ? NullableReference<T>.s_NullSentinel : value;
            }
        }
        public void SetIfNull(T value)
        {
            object obj2 = object.ReferenceEquals(value, null) ? NullableReference<T>.s_NullSentinel : value;
            Interlocked.CompareExchange(ref this._value, obj2, null);
        }

        public void SetVolatile(T value)
        {
            object obj2 = object.ReferenceEquals(value, null) ? NullableReference<T>.s_NullSentinel : value;
            Thread.VolatileWrite(ref this._value, obj2);
        }

        static NullableReference()
        {
            NullableReference<T>.s_NullSentinel = new object();
            NullableReference<T>.s_NotPresentSentinel = new object();
        }
    }
}

