namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.IO;
    using System.Runtime.Remoting;

    internal static class StreamHelper
    {
        private static AsyncCallback _asyncCopyStreamReadCallback = new AsyncCallback(StreamHelper.AsyncCopyStreamReadCallback);
        private static AsyncCallback _asyncCopyStreamWriteCallback = new AsyncCallback(StreamHelper.AsyncCopyStreamWriteCallback);

        private static void AsyncCopyReadHelper(AsyncCopyStreamResult streamState)
        {
            if (streamState.AsyncRead)
            {
                byte[] buffer = streamState.Buffer;
                streamState.Source.BeginRead(buffer, 0, buffer.Length, _asyncCopyStreamReadCallback, streamState);
            }
            else
            {
                byte[] buffer2 = streamState.Buffer;
                int bytesRead = streamState.Source.Read(buffer2, 0, buffer2.Length);
                if (bytesRead == 0)
                {
                    streamState.SetComplete(null, null);
                }
                else
                {
                    if (bytesRead < 0)
                    {
                        throw new RemotingException(CoreChannel.GetResourceString("Remoting_Stream_UnknownReadError"));
                    }
                    AsyncCopyWriteHelper(streamState, bytesRead);
                }
            }
        }

        private static void AsyncCopyStreamReadCallback(IAsyncResult iar)
        {
            AsyncCopyStreamResult asyncState = (AsyncCopyStreamResult) iar.AsyncState;
            try
            {
                int bytesRead = asyncState.Source.EndRead(iar);
                if (bytesRead == 0)
                {
                    asyncState.SetComplete(null, null);
                }
                else
                {
                    if (bytesRead < 0)
                    {
                        throw new RemotingException(CoreChannel.GetResourceString("Remoting_Stream_UnknownReadError"));
                    }
                    AsyncCopyWriteHelper(asyncState, bytesRead);
                }
            }
            catch (Exception exception)
            {
                asyncState.SetComplete(null, exception);
            }
        }

        private static void AsyncCopyStreamWriteCallback(IAsyncResult iar)
        {
            AsyncCopyStreamResult asyncState = (AsyncCopyStreamResult) iar.AsyncState;
            try
            {
                asyncState.Target.EndWrite(iar);
                AsyncCopyReadHelper(asyncState);
            }
            catch (Exception exception)
            {
                asyncState.SetComplete(null, exception);
            }
        }

        private static void AsyncCopyWriteHelper(AsyncCopyStreamResult streamState, int bytesRead)
        {
            if (streamState.AsyncWrite)
            {
                byte[] buffer = streamState.Buffer;
                streamState.Target.BeginWrite(buffer, 0, bytesRead, _asyncCopyStreamWriteCallback, streamState);
            }
            else
            {
                byte[] buffer2 = streamState.Buffer;
                streamState.Target.Write(buffer2, 0, bytesRead);
                AsyncCopyReadHelper(streamState);
            }
        }

        internal static IAsyncResult BeginAsyncCopyStream(Stream source, Stream target, bool asyncRead, bool asyncWrite, bool closeSource, bool closeTarget, AsyncCallback callback, object state)
        {
            AsyncCopyStreamResult streamState = new AsyncCopyStreamResult(callback, state);
            byte[] buffer = CoreChannel.BufferPool.GetBuffer();
            streamState.Source = source;
            streamState.Target = target;
            streamState.Buffer = buffer;
            streamState.AsyncRead = asyncRead;
            streamState.AsyncWrite = asyncWrite;
            streamState.CloseSource = closeSource;
            streamState.CloseTarget = closeTarget;
            try
            {
                AsyncCopyReadHelper(streamState);
            }
            catch (Exception exception)
            {
                streamState.SetComplete(null, exception);
            }
            return streamState;
        }

        internal static void BufferCopy(byte[] source, int srcOffset, byte[] dest, int destOffset, int count)
        {
            if (count > 8)
            {
                Buffer.BlockCopy(source, srcOffset, dest, destOffset, count);
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    dest[destOffset + i] = source[srcOffset + i];
                }
            }
        }

        internal static void CopyStream(Stream source, Stream target)
        {
            if (source != null)
            {
                ChunkedMemoryStream stream = source as ChunkedMemoryStream;
                if (stream != null)
                {
                    stream.WriteTo(target);
                }
                else
                {
                    MemoryStream stream2 = source as MemoryStream;
                    if (stream2 != null)
                    {
                        stream2.WriteTo(target);
                    }
                    else
                    {
                        byte[] buffer = CoreChannel.BufferPool.GetBuffer();
                        int length = buffer.Length;
                        for (int i = source.Read(buffer, 0, length); i > 0; i = source.Read(buffer, 0, length))
                        {
                            target.Write(buffer, 0, i);
                        }
                        CoreChannel.BufferPool.ReturnBuffer(buffer);
                    }
                }
            }
        }

        internal static void EndAsyncCopyStream(IAsyncResult iar)
        {
            AsyncCopyStreamResult result = (AsyncCopyStreamResult) iar;
            if (!iar.IsCompleted)
            {
                iar.AsyncWaitHandle.WaitOne();
            }
            if (result.Exception != null)
            {
                throw result.Exception;
            }
        }
    }
}

