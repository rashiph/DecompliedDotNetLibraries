namespace System.Net.Sockets
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal abstract class MultipleConnectAsync
    {
        protected IPAddress[] addressList;
        protected DnsEndPoint endPoint;
        protected SocketAsyncEventArgs internalArgs;
        private object lockObject = new object();
        protected int nextAddress;
        private State state;
        protected SocketAsyncEventArgs userArgs;

        protected MultipleConnectAsync()
        {
        }

        private void AsyncFail(Exception e)
        {
            this.OnFail(false);
            if (this.internalArgs != null)
            {
                this.internalArgs.Dispose();
            }
            this.userArgs.FinishOperationAsyncFailure(e, 0, SocketFlags.None);
        }

        private Exception AttemptConnection()
        {
            try
            {
                Socket attemptSocket = null;
                IPAddress nextAddress = this.GetNextAddress(out attemptSocket);
                if (nextAddress == null)
                {
                    return new SocketException(SocketError.NoData);
                }
                this.internalArgs.RemoteEndPoint = new IPEndPoint(nextAddress, this.endPoint.Port);
                if (!attemptSocket.ConnectAsync(this.internalArgs))
                {
                    return new SocketException(this.internalArgs.SocketError);
                }
            }
            catch (ObjectDisposedException)
            {
                return new SocketException(SocketError.OperationAborted);
            }
            catch (Exception exception)
            {
                return exception;
            }
            return null;
        }

        private void CallAsyncFail(object ignored)
        {
            this.AsyncFail(new SocketException(SocketError.OperationAborted));
        }

        public void Cancel()
        {
            bool flag = false;
            lock (this.lockObject)
            {
                switch (this.state)
                {
                    case State.NotStarted:
                        flag = true;
                        break;

                    case State.DnsQuery:
                        ThreadPool.QueueUserWorkItem(new WaitCallback(this.CallAsyncFail));
                        flag = true;
                        break;

                    case State.ConnectAttempt:
                        flag = true;
                        break;
                }
                this.state = State.Canceled;
            }
            if (flag)
            {
                this.OnFail(true);
            }
        }

        private void DnsCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                this.DoDnsCallback(result, false);
            }
        }

        private bool DoDnsCallback(IAsyncResult result, bool sync)
        {
            Exception e = null;
            lock (this.lockObject)
            {
                if (this.state == State.Canceled)
                {
                    return true;
                }
                try
                {
                    this.addressList = Dns.EndGetHostAddresses(result);
                }
                catch (Exception exception2)
                {
                    this.state = State.Completed;
                    e = exception2;
                }
                if (e == null)
                {
                    this.state = State.ConnectAttempt;
                    this.internalArgs = new SocketAsyncEventArgs();
                    this.internalArgs.Completed += new EventHandler<SocketAsyncEventArgs>(this.InternalConnectCallback);
                    this.internalArgs.SetBuffer(this.userArgs.Buffer, this.userArgs.Offset, this.userArgs.Count);
                    e = this.AttemptConnection();
                    if (e != null)
                    {
                        this.state = State.Completed;
                    }
                }
            }
            if (e != null)
            {
                return this.Fail(sync, e);
            }
            return true;
        }

        private bool Fail(bool sync, Exception e)
        {
            if (sync)
            {
                this.SyncFail(e);
                return false;
            }
            this.AsyncFail(e);
            return true;
        }

        protected abstract IPAddress GetNextAddress(out Socket attemptSocket);
        private void InternalConnectCallback(object sender, SocketAsyncEventArgs args)
        {
            Exception e = null;
            lock (this.lockObject)
            {
                if (this.state == State.Canceled)
                {
                    e = new SocketException(SocketError.OperationAborted);
                }
                else if (args.SocketError == SocketError.Success)
                {
                    this.state = State.Completed;
                }
                else if (args.SocketError == SocketError.OperationAborted)
                {
                    e = new SocketException(SocketError.OperationAborted);
                    this.state = State.Canceled;
                }
                else
                {
                    SocketError socketError = args.SocketError;
                    Exception exception2 = this.AttemptConnection();
                    if (exception2 == null)
                    {
                        return;
                    }
                    SocketException exception3 = exception2 as SocketException;
                    if ((exception3 != null) && (exception3.SocketErrorCode == SocketError.NoData))
                    {
                        e = new SocketException(socketError);
                    }
                    else
                    {
                        e = exception2;
                    }
                    this.state = State.Completed;
                }
            }
            if (e == null)
            {
                this.Succeed();
            }
            else
            {
                this.AsyncFail(e);
            }
        }

        protected abstract void OnFail(bool abortive);
        protected abstract void OnSucceed();
        public bool StartConnectAsync(SocketAsyncEventArgs args, DnsEndPoint endPoint)
        {
            lock (this.lockObject)
            {
                this.userArgs = args;
                this.endPoint = endPoint;
                if (this.state == State.Canceled)
                {
                    this.SyncFail(new SocketException(SocketError.OperationAborted));
                    return false;
                }
                this.state = State.DnsQuery;
                IAsyncResult result = Dns.BeginGetHostAddresses(endPoint.Host, new AsyncCallback(this.DnsCallback), null);
                if (result.CompletedSynchronously)
                {
                    return this.DoDnsCallback(result, true);
                }
                return true;
            }
        }

        protected void Succeed()
        {
            this.OnSucceed();
            this.userArgs.FinishWrapperConnectSuccess(this.internalArgs.ConnectSocket, this.internalArgs.BytesTransferred, this.internalArgs.SocketFlags);
            this.internalArgs.Dispose();
        }

        private void SyncFail(Exception e)
        {
            this.OnFail(false);
            if (this.internalArgs != null)
            {
                this.internalArgs.Dispose();
            }
            SocketException exception = e as SocketException;
            if (exception == null)
            {
                throw e;
            }
            this.userArgs.FinishConnectByNameSyncFailure(exception, 0, SocketFlags.None);
        }

        private enum State
        {
            NotStarted,
            DnsQuery,
            ConnectAttempt,
            Completed,
            Canceled
        }
    }
}

