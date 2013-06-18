namespace System.Web
{
    using System;

    internal class IntegerArrayAllocator : BufferAllocator
    {
        private int _arraySize;

        internal IntegerArrayAllocator(int arraySize, int maxFree) : base(maxFree)
        {
            this._arraySize = arraySize;
        }

        protected override object AllocBuffer()
        {
            return new int[this._arraySize];
        }
    }
}

