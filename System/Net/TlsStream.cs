namespace System.Net
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Net.Configuration;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;

    internal class TlsStream : NetworkStream, IDisposable
    {
        private static AsyncCallback _CompleteIOCallback = new AsyncCallback(TlsStream.CompleteIOCallback);
        private ExecutionContext _ExecutionContext;
        private ChannelBinding m_CachedChannelBinding;
        private X509CertificateCollection m_ClientCertificates;
        private string m_DestinationHost;
        private WebExceptionStatus m_ExceptionStatus;
        private ArrayList m_PendingIO;
        private int m_ShutDown;
        private SslState m_Worker;

        public TlsStream(string destinationHost, NetworkStream networkStream, X509CertificateCollection clientCertificates, ServicePoint servicePoint, object initiatingRequest, ExecutionContext executionContext) : base(networkStream, true)
        {
            this.m_PendingIO = new ArrayList();
            this._ExecutionContext = executionContext;
            if (this._ExecutionContext == null)
            {
                this._ExecutionContext = ExecutionContext.Capture();
            }
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, this, ".ctor", "host=" + destinationHost + ", #certs=" + ((clientCertificates == null) ? "null" : clientCertificates.Count.ToString(NumberFormatInfo.InvariantInfo)));
            }
            this.m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
            this.m_Worker = new SslState(networkStream, initiatingRequest is HttpWebRequest, SettingsSectionInternal.Section.EncryptionPolicy);
            this.m_DestinationHost = destinationHost;
            this.m_ClientCertificates = clientCertificates;
            RemoteCertValidationCallback certValidationCallback = servicePoint.SetupHandshakeDoneProcedure(this, initiatingRequest);
            this.m_Worker.SetCertValidationDelegate(certValidationCallback);
        }

        internal override IAsyncResult BeginMultipleWrite(BufferOffsetSize[] buffers, AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            if (!this.m_Worker.IsAuthenticated)
            {
                BufferAsyncResult result = new BufferAsyncResult(this, buffers, state, callback);
                if (this.ProcessAuthentication(result))
                {
                    return result;
                }
            }
            try
            {
                result2 = this.m_Worker.SecureStream.BeginWrite(buffers, callback, state);
            }
            catch
            {
                if (this.m_Worker.IsCertValidationFailed)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (this.m_Worker.LastSecurityStatus != SecurityStatus.OK)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else
                {
                    this.m_ExceptionStatus = WebExceptionStatus.SendFailure;
                }
                throw;
            }
            return result2;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback asyncCallback, object asyncState)
        {
            IAsyncResult result2;
            if (!this.m_Worker.IsAuthenticated)
            {
                BufferAsyncResult result = new BufferAsyncResult(this, buffer, offset, size, false, asyncState, asyncCallback);
                if (this.ProcessAuthentication(result))
                {
                    return result;
                }
            }
            try
            {
                result2 = this.m_Worker.SecureStream.BeginRead(buffer, offset, size, asyncCallback, asyncState);
            }
            catch
            {
                if (this.m_Worker.IsCertValidationFailed)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (this.m_Worker.LastSecurityStatus != SecurityStatus.OK)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else
                {
                    this.m_ExceptionStatus = WebExceptionStatus.ReceiveFailure;
                }
                throw;
            }
            return result2;
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback asyncCallback, object asyncState)
        {
            IAsyncResult result2;
            if (!this.m_Worker.IsAuthenticated)
            {
                BufferAsyncResult result = new BufferAsyncResult(this, buffer, offset, size, true, asyncState, asyncCallback);
                if (this.ProcessAuthentication(result))
                {
                    return result;
                }
            }
            try
            {
                result2 = this.m_Worker.SecureStream.BeginWrite(buffer, offset, size, asyncCallback, asyncState);
            }
            catch
            {
                if (this.m_Worker.IsCertValidationFailed)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (this.m_Worker.LastSecurityStatus != SecurityStatus.OK)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else
                {
                    this.m_ExceptionStatus = WebExceptionStatus.SendFailure;
                }
                throw;
            }
            return result2;
        }

        private void CallProcessAuthentication(object state)
        {
            this.m_Worker.ProcessAuthentication((LazyAsyncResult) state);
        }

        private static void CompleteIO(IAsyncResult result)
        {
            BufferAsyncResult asyncState = (BufferAsyncResult) result.AsyncState;
            object obj2 = null;
            if (asyncState.IsWrite)
            {
                ((TlsStream) asyncState.AsyncObject).m_Worker.SecureStream.EndWrite(result);
            }
            else
            {
                obj2 = ((TlsStream) asyncState.AsyncObject).m_Worker.SecureStream.EndRead(result);
            }
            asyncState.InvokeCallback(obj2);
        }

        private static void CompleteIOCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                try
                {
                    CompleteIO(result);
                }
                catch (Exception exception)
                {
                    if (((exception is OutOfMemoryException) || (exception is StackOverflowException)) || (exception is ThreadAbortException))
                    {
                        throw;
                    }
                    if (((LazyAsyncResult) result.AsyncState).InternalPeekCompleted)
                    {
                        throw;
                    }
                    ((LazyAsyncResult) result.AsyncState).InvokeCallback(exception);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref this.m_ShutDown, 1) != 1)
            {
                try
                {
                    if (disposing)
                    {
                        this.m_CachedChannelBinding = this.GetChannelBinding(ChannelBindingKind.Endpoint);
                        this.m_Worker.Close();
                    }
                    else
                    {
                        this.m_Worker = null;
                    }
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }

        internal override void EndMultipleWrite(IAsyncResult asyncResult)
        {
            this.EndWrite(asyncResult);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            int num;
            try
            {
                BufferAsyncResult result = asyncResult as BufferAsyncResult;
                if ((result == null) || (result.AsyncObject != this))
                {
                    return this.m_Worker.SecureStream.EndRead(asyncResult);
                }
                result.InternalWaitForCompletion();
                Exception exception = result.Result as Exception;
                if (exception != null)
                {
                    throw exception;
                }
                num = (int) result.Result;
            }
            catch
            {
                if (this.m_Worker.IsCertValidationFailed)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (this.m_Worker.LastSecurityStatus != SecurityStatus.OK)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else
                {
                    this.m_ExceptionStatus = WebExceptionStatus.ReceiveFailure;
                }
                throw;
            }
            return num;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            try
            {
                BufferAsyncResult result = asyncResult as BufferAsyncResult;
                if ((result == null) || (result.AsyncObject != this))
                {
                    this.m_Worker.SecureStream.EndWrite(asyncResult);
                }
                else
                {
                    result.InternalWaitForCompletion();
                    Exception exception = result.Result as Exception;
                    if (exception != null)
                    {
                        throw exception;
                    }
                }
            }
            catch
            {
                Socket socket = base.Socket;
                if (socket != null)
                {
                    socket.InternalShutdown(SocketShutdown.Both);
                }
                if (this.m_Worker.IsCertValidationFailed)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (this.m_Worker.LastSecurityStatus != SecurityStatus.OK)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else
                {
                    this.m_ExceptionStatus = WebExceptionStatus.SendFailure;
                }
                throw;
            }
        }

        internal ChannelBinding GetChannelBinding(ChannelBindingKind kind)
        {
            if ((kind == ChannelBindingKind.Endpoint) && (this.m_CachedChannelBinding != null))
            {
                return this.m_CachedChannelBinding;
            }
            return this.m_Worker.GetChannelBinding(kind);
        }

        internal override void MultipleWrite(BufferOffsetSize[] buffers)
        {
            if (!this.m_Worker.IsAuthenticated)
            {
                this.ProcessAuthentication(null);
            }
            try
            {
                this.m_Worker.SecureStream.Write(buffers);
            }
            catch
            {
                Socket socket = base.Socket;
                if (socket != null)
                {
                    socket.InternalShutdown(SocketShutdown.Both);
                }
                if (this.m_Worker.IsCertValidationFailed)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (this.m_Worker.LastSecurityStatus != SecurityStatus.OK)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else
                {
                    this.m_ExceptionStatus = WebExceptionStatus.SendFailure;
                }
                throw;
            }
        }

        internal bool ProcessAuthentication(LazyAsyncResult result)
        {
            bool flag = false;
            bool flag2 = result == null;
            lock (this.m_PendingIO)
            {
                if (this.m_Worker.IsAuthenticated)
                {
                    return false;
                }
                if (this.m_PendingIO.Count == 0)
                {
                    flag = true;
                }
                if (flag2)
                {
                    result = new LazyAsyncResult(this, null, null);
                }
                this.m_PendingIO.Add(result);
            }
            try
            {
                if (flag)
                {
                    bool flag3 = true;
                    LazyAsyncResult state = null;
                    try
                    {
                        try
                        {
                            this.m_Worker.ValidateCreateContext(false, this.m_DestinationHost, (SslProtocols) ServicePointManager.SecurityProtocol, null, this.m_ClientCertificates, true, ServicePointManager.CheckCertificateRevocationList, ServicePointManager.CheckCertificateName);
                            if (!flag2)
                            {
                                state = new LazyAsyncResult(this.m_Worker, null, new AsyncCallback(this.WakeupPendingIO));
                            }
                            if (this._ExecutionContext != null)
                            {
                                ExecutionContext.Run(this._ExecutionContext.CreateCopy(), new ContextCallback(this.CallProcessAuthentication), state);
                            }
                            else
                            {
                                this.m_Worker.ProcessAuthentication(state);
                            }
                        }
                        catch
                        {
                            flag3 = false;
                            throw;
                        }
                        goto Label_0198;
                    }
                    finally
                    {
                        if (flag2 || !flag3)
                        {
                            lock (this.m_PendingIO)
                            {
                                if (this.m_PendingIO.Count > 1)
                                {
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.StartWakeupPendingIO), null);
                                }
                                else
                                {
                                    this.m_PendingIO.Clear();
                                }
                            }
                        }
                    }
                }
                if (flag2)
                {
                    Exception exception = result.InternalWaitForCompletion() as Exception;
                    if (exception != null)
                    {
                        throw exception;
                    }
                }
            }
            catch
            {
                if (this.m_Worker.IsCertValidationFailed)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (this.m_Worker.LastSecurityStatus != SecurityStatus.OK)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else
                {
                    this.m_ExceptionStatus = WebExceptionStatus.ReceiveFailure;
                }
                throw;
            }
        Label_0198:
            return true;
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            int num;
            if (!this.m_Worker.IsAuthenticated)
            {
                this.ProcessAuthentication(null);
            }
            try
            {
                num = this.m_Worker.SecureStream.Read(buffer, offset, size);
            }
            catch
            {
                if (this.m_Worker.IsCertValidationFailed)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (this.m_Worker.LastSecurityStatus != SecurityStatus.OK)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else
                {
                    this.m_ExceptionStatus = WebExceptionStatus.ReceiveFailure;
                }
                throw;
            }
            return num;
        }

        private void ResumeIO(BufferAsyncResult bufferResult)
        {
            IAsyncResult result;
            if (bufferResult.IsWrite)
            {
                if (bufferResult.Buffers != null)
                {
                    result = this.m_Worker.SecureStream.BeginWrite(bufferResult.Buffers, _CompleteIOCallback, bufferResult);
                }
                else
                {
                    result = this.m_Worker.SecureStream.BeginWrite(bufferResult.Buffer, bufferResult.Offset, bufferResult.Count, _CompleteIOCallback, bufferResult);
                }
            }
            else
            {
                result = this.m_Worker.SecureStream.BeginRead(bufferResult.Buffer, bufferResult.Offset, bufferResult.Count, _CompleteIOCallback, bufferResult);
            }
            if (result.CompletedSynchronously)
            {
                CompleteIO(result);
            }
        }

        private void ResumeIOWorker(object result)
        {
            BufferAsyncResult bufferResult = (BufferAsyncResult) result;
            try
            {
                this.ResumeIO(bufferResult);
            }
            catch (Exception exception)
            {
                if (((exception is OutOfMemoryException) || (exception is StackOverflowException)) || (exception is ThreadAbortException))
                {
                    throw;
                }
                if (bufferResult.InternalPeekCompleted)
                {
                    throw;
                }
                bufferResult.InvokeCallback(exception);
            }
        }

        private void StartWakeupPendingIO(object nullState)
        {
            this.WakeupPendingIO(null);
        }

        internal override IAsyncResult UnsafeBeginMultipleWrite(BufferOffsetSize[] buffers, AsyncCallback callback, object state)
        {
            return this.BeginMultipleWrite(buffers, callback, state);
        }

        internal override IAsyncResult UnsafeBeginRead(byte[] buffer, int offset, int size, AsyncCallback asyncCallback, object asyncState)
        {
            return this.BeginRead(buffer, offset, size, asyncCallback, asyncState);
        }

        internal override IAsyncResult UnsafeBeginWrite(byte[] buffer, int offset, int size, AsyncCallback asyncCallback, object asyncState)
        {
            return this.BeginWrite(buffer, offset, size, asyncCallback, asyncState);
        }

        private void WakeupPendingIO(IAsyncResult ar)
        {
            Exception exception = null;
            try
            {
                if (ar != null)
                {
                    this.m_Worker.EndProcessAuthentication(ar);
                }
            }
            catch (Exception exception2)
            {
                exception = exception2;
                if (this.m_Worker.IsCertValidationFailed)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (this.m_Worker.LastSecurityStatus != SecurityStatus.OK)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else
                {
                    this.m_ExceptionStatus = WebExceptionStatus.ReceiveFailure;
                }
            }
            lock (this.m_PendingIO)
            {
                while (this.m_PendingIO.Count != 0)
                {
                    LazyAsyncResult result = (LazyAsyncResult) this.m_PendingIO[this.m_PendingIO.Count - 1];
                    this.m_PendingIO.RemoveAt(this.m_PendingIO.Count - 1);
                    if (result is BufferAsyncResult)
                    {
                        if (this.m_PendingIO.Count == 0)
                        {
                            this.ResumeIOWorker(result);
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(this.ResumeIOWorker), result);
                        }
                    }
                    else
                    {
                        try
                        {
                            result.InvokeCallback(exception);
                            continue;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            if (!this.m_Worker.IsAuthenticated)
            {
                this.ProcessAuthentication(null);
            }
            try
            {
                this.m_Worker.SecureStream.Write(buffer, offset, size);
            }
            catch
            {
                if (this.m_Worker.IsCertValidationFailed)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.TrustFailure;
                }
                else if (this.m_Worker.LastSecurityStatus != SecurityStatus.OK)
                {
                    this.m_ExceptionStatus = WebExceptionStatus.SecureChannelFailure;
                }
                else
                {
                    this.m_ExceptionStatus = WebExceptionStatus.SendFailure;
                }
                Socket socket = base.Socket;
                if (socket != null)
                {
                    socket.InternalShutdown(SocketShutdown.Both);
                }
                throw;
            }
        }

        public X509Certificate ClientCertificate
        {
            get
            {
                return this.m_Worker.InternalLocalCertificate;
            }
        }

        public override bool DataAvailable
        {
            get
            {
                if (!this.m_Worker.DataAvailable)
                {
                    return base.DataAvailable;
                }
                return true;
            }
        }

        internal WebExceptionStatus ExceptionStatus
        {
            get
            {
                return this.m_ExceptionStatus;
            }
        }
    }
}

