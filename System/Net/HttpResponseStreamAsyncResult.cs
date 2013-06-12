namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class HttpResponseStreamAsyncResult : LazyAsyncResult
    {
        private UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK[] m_DataChunks;
        internal unsafe NativeOverlapped* m_pOverlapped;
        internal bool m_SentHeaders;
        private static readonly IOCompletionCallback s_IOCallback = new IOCompletionCallback(HttpResponseStreamAsyncResult.Callback);

        internal HttpResponseStreamAsyncResult(object asyncObject, object userState, AsyncCallback callback) : base(asyncObject, userState, callback)
        {
        }

        internal unsafe HttpResponseStreamAsyncResult(object asyncObject, object userState, AsyncCallback callback, byte[] buffer, int offset, int size, bool chunked, bool sentHeaders) : base(asyncObject, userState, callback)
        {
            this.m_SentHeaders = sentHeaders;
            Overlapped overlapped = new Overlapped {
                AsyncResult = this
            };
            if (size == 0)
            {
                this.m_DataChunks = null;
                this.m_pOverlapped = overlapped.Pack(s_IOCallback, null);
            }
            else
            {
                this.m_DataChunks = new UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK[chunked ? 3 : 1];
                object[] userData = new object[1 + this.m_DataChunks.Length];
                userData[this.m_DataChunks.Length] = this.m_DataChunks;
                int num = 0;
                byte[] arr = null;
                if (chunked)
                {
                    arr = ConnectStream.GetChunkHeader(size, out num);
                    this.m_DataChunks[0] = new UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK();
                    this.m_DataChunks[0].DataChunkType = UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK_TYPE.HttpDataChunkFromMemory;
                    this.m_DataChunks[0].BufferLength = (uint) (arr.Length - num);
                    userData[0] = arr;
                    this.m_DataChunks[1] = new UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK();
                    this.m_DataChunks[1].DataChunkType = UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK_TYPE.HttpDataChunkFromMemory;
                    this.m_DataChunks[1].BufferLength = (uint) size;
                    userData[1] = buffer;
                    this.m_DataChunks[2] = new UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK();
                    this.m_DataChunks[2].DataChunkType = UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK_TYPE.HttpDataChunkFromMemory;
                    this.m_DataChunks[2].BufferLength = (uint) NclConstants.CRLF.Length;
                    userData[2] = NclConstants.CRLF;
                }
                else
                {
                    this.m_DataChunks[0] = new UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK();
                    this.m_DataChunks[0].DataChunkType = UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK_TYPE.HttpDataChunkFromMemory;
                    this.m_DataChunks[0].BufferLength = (uint) size;
                    userData[0] = buffer;
                }
                this.m_pOverlapped = overlapped.Pack(s_IOCallback, userData);
                if (chunked)
                {
                    this.m_DataChunks[0].pBuffer = (byte*) Marshal.UnsafeAddrOfPinnedArrayElement(arr, num);
                    this.m_DataChunks[1].pBuffer = (byte*) Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset);
                    this.m_DataChunks[2].pBuffer = (byte*) Marshal.UnsafeAddrOfPinnedArrayElement(NclConstants.CRLF, 0);
                }
                else
                {
                    this.m_DataChunks[0].pBuffer = (byte*) Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset);
                }
            }
        }

        private static unsafe void Callback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
        {
            HttpResponseStreamAsyncResult asyncResult = Overlapped.Unpack(nativeOverlapped).AsyncResult as HttpResponseStreamAsyncResult;
            object result = null;
            try
            {
                if ((errorCode != 0) && (errorCode != 0x26))
                {
                    asyncResult.ErrorCode = (int) errorCode;
                    result = new HttpListenerException((int) errorCode);
                }
                else if (asyncResult.m_DataChunks == null)
                {
                    result = 0;
                    if (Logging.On)
                    {
                        Logging.Dump(Logging.HttpListener, asyncResult, "Callback", IntPtr.Zero, 0);
                    }
                }
                else
                {
                    result = (asyncResult.m_DataChunks.Length == 1) ? ((object) asyncResult.m_DataChunks[0].BufferLength) : ((object) 0);
                    if (Logging.On)
                    {
                        for (int i = 0; i < asyncResult.m_DataChunks.Length; i++)
                        {
                            Logging.Dump(Logging.HttpListener, asyncResult, "Callback", (IntPtr) asyncResult.m_DataChunks[0].pBuffer, (int) asyncResult.m_DataChunks[0].BufferLength);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                result = exception;
            }
            asyncResult.InvokeCallback(result);
        }

        protected override unsafe void Cleanup()
        {
            base.Cleanup();
            if (this.m_pOverlapped != null)
            {
                Overlapped.Free(this.m_pOverlapped);
            }
        }

        internal ushort dataChunkCount
        {
            get
            {
                if (this.m_DataChunks == null)
                {
                    return 0;
                }
                return (ushort) this.m_DataChunks.Length;
            }
        }

        internal UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK* pDataChunks
        {
            get
            {
                if (this.m_DataChunks == null)
                {
                    return null;
                }
                return (UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK*) Marshal.UnsafeAddrOfPinnedArrayElement(this.m_DataChunks, 0);
            }
        }
    }
}

