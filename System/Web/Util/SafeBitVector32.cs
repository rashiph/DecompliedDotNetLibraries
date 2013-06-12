namespace System.Web.Util
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct SafeBitVector32
    {
        private volatile int _data;
        internal SafeBitVector32(int data)
        {
            this._data = data;
        }

        internal bool this[int bit]
        {
            get
            {
                return ((this._data & bit) == bit);
            }
            set
            {
                int num;
                int num2;
                do
                {
                    num = this._data;
                    if (value)
                    {
                        num2 = num | bit;
                    }
                    else
                    {
                        num2 = num & ~bit;
                    }
                }
                while (Interlocked.CompareExchange(ref this._data, num2, num) != num);
            }
        }
        internal bool ChangeValue(int bit, bool value)
        {
            int num;
            int num2;
            do
            {
                num = this._data;
                if (value)
                {
                    num2 = num | bit;
                }
                else
                {
                    num2 = num & ~bit;
                }
                if (num == num2)
                {
                    return false;
                }
            }
            while (Interlocked.CompareExchange(ref this._data, num2, num) != num);
            return true;
        }
    }
}

