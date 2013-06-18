namespace System.Web
{
    using System;

    internal class IntPtrArrayAllocator : BufferAllocator
    {
        private int _arraySize;

        internal IntPtrArrayAllocator(int arraySize, int maxFree) : base(maxFree)
        {
            this._arraySize = arraySize;
        }

        protected override object AllocBuffer()
        {
            return new IntPtr[this._arraySize];
        }
    }
}

