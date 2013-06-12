namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;

    internal class ListenerClientCertAsyncResult : LazyAsyncResult
    {
        private byte[] m_BackingBuffer;
        private unsafe UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* m_MemoryBlob;
        private unsafe System.Threading.NativeOverlapped* m_pOverlapped;
        private uint m_Size;
        private static readonly IOCompletionCallback s_IOCallback = new IOCompletionCallback(ListenerClientCertAsyncResult.WaitCallback);

        internal ListenerClientCertAsyncResult(object asyncObject, object userState, AsyncCallback callback, uint size) : base(asyncObject, userState, callback)
        {
            this.Reset(size);
        }

        protected override unsafe void Cleanup()
        {
            if (this.m_pOverlapped != null)
            {
                this.m_MemoryBlob = null;
                Overlapped.Free(this.m_pOverlapped);
                this.m_pOverlapped = null;
            }
            GC.SuppressFinalize(this);
            base.Cleanup();
        }

        ~ListenerClientCertAsyncResult()
        {
            if ((this.m_pOverlapped != null) && !NclUtilities.HasShutdownStarted)
            {
                Overlapped.Free(this.m_pOverlapped);
                this.m_pOverlapped = null;
            }
        }

        internal unsafe void Reset(uint size)
        {
            if (size != this.m_Size)
            {
                if (this.m_Size != 0)
                {
                    Overlapped.Free(this.m_pOverlapped);
                }
                this.m_Size = size;
                if (size == 0)
                {
                    this.m_pOverlapped = null;
                    this.m_MemoryBlob = null;
                    this.m_BackingBuffer = null;
                }
                else
                {
                    this.m_BackingBuffer = new byte[size];
                    this.m_pOverlapped = new Overlapped { AsyncResult = this }.Pack(s_IOCallback, this.m_BackingBuffer);
                    this.m_MemoryBlob = (UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO*) Marshal.UnsafeAddrOfPinnedArrayElement(this.m_BackingBuffer, 0);
                }
            }
        }

        private static unsafe void WaitCallback(uint errorCode, uint numBytes, System.Threading.NativeOverlapped* nativeOverlapped)
        {
            ListenerClientCertAsyncResult asyncResult = (ListenerClientCertAsyncResult) Overlapped.Unpack(nativeOverlapped).AsyncResult;
            HttpListenerRequest asyncObject = (HttpListenerRequest) asyncResult.AsyncObject;
            object result = null;
            try
            {
                if (errorCode == 0xea)
                {
                    UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* requestBlob = asyncResult.RequestBlob;
                    asyncResult.Reset(numBytes + requestBlob->CertEncodedSize);
                    uint pBytesReceived = 0;
                    errorCode = UnsafeNclNativeMethods.HttpApi.HttpReceiveClientCertificate(asyncObject.HttpListenerContext.RequestQueueHandle, asyncObject.m_ConnectionId, 0, asyncResult.m_MemoryBlob, asyncResult.m_Size, &pBytesReceived, asyncResult.m_pOverlapped);
                    if ((errorCode == 0x3e5) || (errorCode == 0))
                    {
                        return;
                    }
                }
                if (errorCode != 0)
                {
                    asyncResult.ErrorCode = (int) errorCode;
                    result = new HttpListenerException((int) errorCode);
                }
                else
                {
                    UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* memoryBlob = asyncResult.m_MemoryBlob;
                    if (memoryBlob != null)
                    {
                        if (memoryBlob->pCertEncoded != null)
                        {
                            try
                            {
                                byte[] destination = new byte[memoryBlob->CertEncodedSize];
                                Marshal.Copy((IntPtr) memoryBlob->pCertEncoded, destination, 0, destination.Length);
                                result = asyncObject.ClientCertificate = new X509Certificate2(destination);
                            }
                            catch (CryptographicException exception)
                            {
                                result = exception;
                            }
                            catch (SecurityException exception2)
                            {
                                result = exception2;
                            }
                        }
                        asyncObject.SetClientCertificateError((int) memoryBlob->CertFlags);
                    }
                }
            }
            catch (Exception exception3)
            {
                if (NclUtilities.IsFatal(exception3))
                {
                    throw;
                }
                result = exception3;
            }
            finally
            {
                if (errorCode != 0x3e5)
                {
                    asyncObject.ClientCertState = ListenerClientCertState.Completed;
                }
            }
            asyncResult.InvokeCallback(result);
        }

        internal System.Threading.NativeOverlapped* NativeOverlapped
        {
            get
            {
                return this.m_pOverlapped;
            }
        }

        internal UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* RequestBlob
        {
            get
            {
                return this.m_MemoryBlob;
            }
        }
    }
}

