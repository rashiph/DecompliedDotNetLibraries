namespace System
{
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct ArraySegment<T>
    {
        private T[] _array;
        private int _offset;
        private int _count;
        public ArraySegment(T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            this._array = array;
            this._offset = 0;
            this._count = array.Length;
        }

        public ArraySegment(T[] array, int offset, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - offset) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            this._array = array;
            this._offset = offset;
            this._count = count;
        }

        public T[] Array
        {
            get
            {
                return this._array;
            }
        }
        public int Offset
        {
            get
            {
                return this._offset;
            }
        }
        public int Count
        {
            get
            {
                return this._count;
            }
        }
        public override int GetHashCode()
        {
            return ((this._array.GetHashCode() ^ this._offset) ^ this._count);
        }

        public override bool Equals(object obj)
        {
            return ((obj is ArraySegment<T>) && this.Equals((ArraySegment<T>) obj));
        }

        public bool Equals(ArraySegment<T> obj)
        {
            return (((obj._array == this._array) && (obj._offset == this._offset)) && (obj._count == this._count));
        }

        public static bool operator ==(ArraySegment<T> a, ArraySegment<T> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ArraySegment<T> a, ArraySegment<T> b)
        {
            return !(a == b);
        }
    }
}

