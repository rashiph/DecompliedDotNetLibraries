namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;

    internal static class StreamingConnectionHelper
    {
        public static IAsyncResult BeginWriteMessage(Message message, IConnection connection, bool isRequest, IConnectionOrientedTransportFactorySettings settings, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
        {
            return new WriteMessageAsyncResult(message, connection, isRequest, settings, ref timeoutHelper, callback, state);
        }

        public static void EndWriteMessage(IAsyncResult result)
        {
            WriteMessageAsyncResult.End(result);
        }

        public static void WriteMessage(Message message, IConnection connection, bool isRequest, IConnectionOrientedTransportFactorySettings settings, ref TimeoutHelper timeoutHelper)
        {
            byte[] envelopeEndFramingEndBytes = null;
            if (message != null)
            {
                bool flag;
                MessageEncoder encoder = settings.MessageEncoderFactory.Encoder;
                byte[] envelopeStartBytes = SingletonEncoder.EnvelopeStartBytes;
                if (isRequest)
                {
                    envelopeEndFramingEndBytes = SingletonEncoder.EnvelopeEndFramingEndBytes;
                    flag = TransferModeHelper.IsRequestStreamed(settings.TransferMode);
                }
                else
                {
                    envelopeEndFramingEndBytes = SingletonEncoder.EnvelopeEndBytes;
                    flag = TransferModeHelper.IsResponseStreamed(settings.TransferMode);
                }
                if (flag)
                {
                    connection.Write(envelopeStartBytes, 0, envelopeStartBytes.Length, false, timeoutHelper.RemainingTime());
                    Stream stream = new StreamingOutputConnectionStream(connection, settings);
                    Stream stream2 = new TimeoutStream(stream, ref timeoutHelper);
                    encoder.WriteMessage(message, stream2);
                }
                else
                {
                    ArraySegment<byte> segment = SingletonEncoder.EncodeMessageFrame(encoder.WriteMessage(message, 0x7fffffff, settings.BufferManager, envelopeStartBytes.Length + 5));
                    Buffer.BlockCopy(envelopeStartBytes, 0, segment.Array, segment.Offset - envelopeStartBytes.Length, envelopeStartBytes.Length);
                    connection.Write(segment.Array, segment.Offset - envelopeStartBytes.Length, segment.Count + envelopeStartBytes.Length, true, timeoutHelper.RemainingTime(), settings.BufferManager);
                }
            }
            else if (isRequest)
            {
                envelopeEndFramingEndBytes = SingletonEncoder.EndBytes;
            }
            if (envelopeEndFramingEndBytes != null)
            {
                connection.Write(envelopeEndFramingEndBytes, 0, envelopeEndFramingEndBytes.Length, true, timeoutHelper.RemainingTime());
            }
        }

        private class StreamingOutputConnectionStream : ConnectionStream
        {
            private byte[] encodedSize;

            public StreamingOutputConnectionStream(IConnection connection, IDefaultCommunicationTimeouts timeouts) : base(connection, timeouts)
            {
                this.encodedSize = new byte[5];
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                this.WriteChunkSize(count);
                return base.BeginWrite(buffer, offset, count, callback, state);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                this.WriteChunkSize(count);
                base.Write(buffer, offset, count);
            }

            public override void WriteByte(byte value)
            {
                this.WriteChunkSize(1);
                base.WriteByte(value);
            }

            private void WriteChunkSize(int size)
            {
                if (size > 0)
                {
                    int num = IntEncoder.Encode(size, this.encodedSize, 0);
                    base.Connection.Write(this.encodedSize, 0, num, false, TimeSpan.FromMilliseconds((double) this.WriteTimeout));
                }
            }
        }

        private class WriteMessageAsyncResult : AsyncResult
        {
            private BufferManager bufferManager;
            private byte[] bufferToFree;
            private IConnection connection;
            private MessageEncoder encoder;
            private byte[] endBytes;
            private Message message;
            private static AsyncCallback onWriteBufferedMessage;
            private static AsyncCallback onWriteEndBytes = Fx.ThunkCallback(new AsyncCallback(StreamingConnectionHelper.WriteMessageAsyncResult.OnWriteEndBytes));
            private static AsyncCallback onWriteStartBytes;
            private static Action<object> onWriteStartBytesScheduled;
            private IConnectionOrientedTransportFactorySettings settings;
            private TimeoutHelper timeoutHelper;

            public WriteMessageAsyncResult(Message message, IConnection connection, bool isRequest, IConnectionOrientedTransportFactorySettings settings, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state) : base(callback, state)
            {
                this.connection = connection;
                this.encoder = settings.MessageEncoderFactory.Encoder;
                this.bufferManager = settings.BufferManager;
                this.timeoutHelper = timeoutHelper;
                this.message = message;
                this.settings = settings;
                bool flag = true;
                bool flag2 = false;
                if (message == null)
                {
                    if (isRequest)
                    {
                        this.endBytes = SingletonEncoder.EndBytes;
                    }
                    flag2 = this.WriteEndBytes();
                }
                else
                {
                    try
                    {
                        bool flag3;
                        byte[] envelopeStartBytes = SingletonEncoder.EnvelopeStartBytes;
                        if (isRequest)
                        {
                            this.endBytes = SingletonEncoder.EnvelopeEndFramingEndBytes;
                            flag3 = TransferModeHelper.IsRequestStreamed(settings.TransferMode);
                        }
                        else
                        {
                            this.endBytes = SingletonEncoder.EnvelopeEndBytes;
                            flag3 = TransferModeHelper.IsResponseStreamed(settings.TransferMode);
                        }
                        if (flag3)
                        {
                            if (onWriteStartBytes == null)
                            {
                                onWriteStartBytes = Fx.ThunkCallback(new AsyncCallback(StreamingConnectionHelper.WriteMessageAsyncResult.OnWriteStartBytes));
                            }
                            IAsyncResult result = connection.BeginWrite(envelopeStartBytes, 0, envelopeStartBytes.Length, true, timeoutHelper.RemainingTime(), onWriteStartBytes, this);
                            if (result.CompletedSynchronously)
                            {
                                if (onWriteStartBytesScheduled == null)
                                {
                                    onWriteStartBytesScheduled = new Action<object>(StreamingConnectionHelper.WriteMessageAsyncResult.OnWriteStartBytesScheduled);
                                }
                                ActionItem.Schedule(onWriteStartBytesScheduled, result);
                            }
                        }
                        else
                        {
                            ArraySegment<byte> segment = SingletonEncoder.EncodeMessageFrame(settings.MessageEncoderFactory.Encoder.WriteMessage(message, 0x7fffffff, this.bufferManager, envelopeStartBytes.Length + 5));
                            this.bufferToFree = segment.Array;
                            Buffer.BlockCopy(envelopeStartBytes, 0, segment.Array, segment.Offset - envelopeStartBytes.Length, envelopeStartBytes.Length);
                            if (onWriteBufferedMessage == null)
                            {
                                onWriteBufferedMessage = Fx.ThunkCallback(new AsyncCallback(StreamingConnectionHelper.WriteMessageAsyncResult.OnWriteBufferedMessage));
                            }
                            IAsyncResult result2 = connection.BeginWrite(segment.Array, segment.Offset - envelopeStartBytes.Length, segment.Count + envelopeStartBytes.Length, true, timeoutHelper.RemainingTime(), onWriteBufferedMessage, this);
                            if (result2.CompletedSynchronously)
                            {
                                flag2 = this.HandleWriteBufferedMessage(result2);
                            }
                        }
                        flag = false;
                    }
                    finally
                    {
                        if (flag)
                        {
                            this.Cleanup();
                        }
                    }
                }
                if (flag2)
                {
                    base.Complete(true);
                }
            }

            private void Cleanup()
            {
                if (this.bufferToFree != null)
                {
                    this.bufferManager.ReturnBuffer(this.bufferToFree);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<StreamingConnectionHelper.WriteMessageAsyncResult>(result);
            }

            private bool HandleWriteBufferedMessage(IAsyncResult result)
            {
                this.connection.EndWrite(result);
                return this.WriteEndBytes();
            }

            private bool HandleWriteEndBytes(IAsyncResult result)
            {
                this.connection.EndWrite(result);
                this.Cleanup();
                return true;
            }

            private bool HandleWriteStartBytes(IAsyncResult result)
            {
                this.connection.EndWrite(result);
                Stream stream = new StreamingConnectionHelper.StreamingOutputConnectionStream(this.connection, this.settings);
                Stream stream2 = new TimeoutStream(stream, ref this.timeoutHelper);
                this.encoder.WriteMessage(this.message, stream2);
                return this.WriteEndBytes();
            }

            private static void OnWriteBufferedMessage(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    StreamingConnectionHelper.WriteMessageAsyncResult asyncState = (StreamingConnectionHelper.WriteMessageAsyncResult) result.AsyncState;
                    Exception exception = null;
                    bool flag = false;
                    bool flag2 = true;
                    try
                    {
                        flag = asyncState.HandleWriteBufferedMessage(result);
                        flag2 = false;
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    finally
                    {
                        if (flag2)
                        {
                            asyncState.Cleanup();
                        }
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private static void OnWriteEndBytes(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    StreamingConnectionHelper.WriteMessageAsyncResult asyncState = (StreamingConnectionHelper.WriteMessageAsyncResult) result.AsyncState;
                    Exception exception = null;
                    bool flag = false;
                    bool flag2 = false;
                    try
                    {
                        flag = asyncState.HandleWriteEndBytes(result);
                        flag2 = true;
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    finally
                    {
                        if (!flag2)
                        {
                            asyncState.Cleanup();
                        }
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private static void OnWriteStartBytes(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    OnWriteStartBytesCallbackHelper(result);
                }
            }

            private static void OnWriteStartBytesCallbackHelper(IAsyncResult result)
            {
                StreamingConnectionHelper.WriteMessageAsyncResult asyncState = (StreamingConnectionHelper.WriteMessageAsyncResult) result.AsyncState;
                Exception exception = null;
                bool flag = false;
                bool flag2 = true;
                try
                {
                    flag = asyncState.HandleWriteStartBytes(result);
                    flag2 = false;
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    flag = true;
                    exception = exception2;
                }
                finally
                {
                    if (flag2)
                    {
                        asyncState.Cleanup();
                    }
                }
                if (flag)
                {
                    asyncState.Complete(false, exception);
                }
            }

            private static void OnWriteStartBytesScheduled(object state)
            {
                OnWriteStartBytesCallbackHelper((IAsyncResult) state);
            }

            private bool WriteEndBytes()
            {
                if (this.endBytes == null)
                {
                    this.Cleanup();
                    return true;
                }
                IAsyncResult result = this.connection.BeginWrite(this.endBytes, 0, this.endBytes.Length, true, this.timeoutHelper.RemainingTime(), onWriteEndBytes, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                return this.HandleWriteEndBytes(result);
            }
        }
    }
}

