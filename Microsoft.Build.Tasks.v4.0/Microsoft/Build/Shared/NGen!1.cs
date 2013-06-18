namespace Microsoft.Build.Shared
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NGen<T> where T: struct
    {
        private T value;
        public NGen(T value)
        {
            this.value = value;
        }

        public static implicit operator T(NGen<T> value)
        {
            return value.value;
        }

        public static implicit operator NGen<T>(T value)
        {
            return new NGen<T>(value);
        }
    }
}

