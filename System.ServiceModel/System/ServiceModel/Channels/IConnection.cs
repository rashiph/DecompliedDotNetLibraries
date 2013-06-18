namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;

    internal interface IConnection
    {
        void Abort();
        AsyncReadResult BeginRead(int offset, int size, TimeSpan timeout, WaitCallback callback, object state);
        IAsyncResult BeginWrite(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, AsyncCallback callback, object state);
        void Close(TimeSpan timeout, bool asyncAndLinger);
        object DuplicateAndClose(int targetProcessId);
        int EndRead();
        void EndWrite(IAsyncResult result);
        object GetCoreTransport();
        int Read(byte[] buffer, int offset, int size, TimeSpan timeout);
        void Shutdown(TimeSpan timeout);
        bool Validate(Uri uri);
        void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout);
        void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, BufferManager bufferManager);

        byte[] AsyncReadBuffer { get; }

        int AsyncReadBufferSize { get; }

        TraceEventType ExceptionEventType { get; set; }

        IPEndPoint RemoteIPEndPoint { get; }
    }
}

