namespace System.Xml
{
    using System;

    internal abstract class IncrementalReadDecoder
    {
        protected IncrementalReadDecoder()
        {
        }

        internal abstract int Decode(char[] chars, int startPos, int len);
        internal abstract int Decode(string str, int startPos, int len);
        internal abstract void Reset();
        internal abstract void SetNextOutputBuffer(Array array, int offset, int len);

        internal abstract int DecodedCount { get; }

        internal abstract bool IsFull { get; }
    }
}

