namespace System.Net
{
    using System;

    internal class WorkerAsyncResult : LazyAsyncResult
    {
        public byte[] Buffer;
        public int End;
        public bool HandshakeDone;
        public bool HeaderDone;
        public bool IsWrite;
        public int Offset;
        public WorkerAsyncResult ParentResult;

        public WorkerAsyncResult(object asyncObject, object asyncState, AsyncCallback savedAsyncCallback, byte[] buffer, int offset, int end) : base(asyncObject, asyncState, savedAsyncCallback)
        {
            this.Buffer = buffer;
            this.Offset = offset;
            this.End = end;
        }
    }
}

