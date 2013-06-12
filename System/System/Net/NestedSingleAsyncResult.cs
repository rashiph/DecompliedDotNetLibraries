namespace System.Net
{
    using System;

    internal class NestedSingleAsyncResult : LazyAsyncResult
    {
        internal byte[] Buffer;
        internal int Offset;
        internal int Size;

        internal NestedSingleAsyncResult(object asyncObject, object asyncState, AsyncCallback asyncCallback, object result) : base(asyncObject, asyncState, asyncCallback, result)
        {
        }

        internal NestedSingleAsyncResult(object asyncObject, object asyncState, AsyncCallback asyncCallback, byte[] buffer, int offset, int size) : base(asyncObject, asyncState, asyncCallback)
        {
            this.Buffer = buffer;
            this.Offset = offset;
            this.Size = size;
        }
    }
}

