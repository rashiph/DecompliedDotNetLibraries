namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Selectors;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    internal abstract class CommunicationObjectSecurityTokenProvider : SecurityTokenProvider, ICommunicationObject, ISecurityCommunicationObject
    {
        private WrapperSecurityCommunicationObject communicationObject;

        public event EventHandler Closed
        {
            add
            {
                this.communicationObject.Closed += value;
            }
            remove
            {
                this.communicationObject.Closed -= value;
            }
        }

        public event EventHandler Closing
        {
            add
            {
                this.communicationObject.Closing += value;
            }
            remove
            {
                this.communicationObject.Closing -= value;
            }
        }

        public event EventHandler Faulted
        {
            add
            {
                this.communicationObject.Faulted += value;
            }
            remove
            {
                this.communicationObject.Faulted -= value;
            }
        }

        public event EventHandler Opened
        {
            add
            {
                this.communicationObject.Opened += value;
            }
            remove
            {
                this.communicationObject.Opened -= value;
            }
        }

        public event EventHandler Opening
        {
            add
            {
                this.communicationObject.Opening += value;
            }
            remove
            {
                this.communicationObject.Opening -= value;
            }
        }

        protected CommunicationObjectSecurityTokenProvider()
        {
            this.communicationObject = new WrapperSecurityCommunicationObject(this);
        }

        public void Abort()
        {
            this.communicationObject.Abort();
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginClose(callback, state);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginClose(timeout, callback, state);
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginOpen(callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginOpen(timeout, callback, state);
        }

        public void Close()
        {
            this.communicationObject.Close();
        }

        public void Close(TimeSpan timeout)
        {
            this.communicationObject.Close(timeout);
        }

        public void Dispose()
        {
            this.Close();
        }

        public void EndClose(IAsyncResult result)
        {
            this.communicationObject.EndClose(result);
        }

        public void EndOpen(IAsyncResult result)
        {
            this.communicationObject.EndOpen(result);
        }

        public virtual void OnAbort()
        {
        }

        public IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnClose), timeout, callback, state);
        }

        public IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnOpen), timeout, callback, state);
        }

        public virtual void OnClose(TimeSpan timeout)
        {
        }

        public virtual void OnClosed()
        {
            SecurityTraceRecordHelper.TraceTokenProviderClosed(this);
        }

        public virtual void OnClosing()
        {
        }

        public void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutAsyncResult.End(result);
        }

        public void OnEndOpen(IAsyncResult result)
        {
            OperationWithTimeoutAsyncResult.End(result);
        }

        public virtual void OnFaulted()
        {
            this.OnAbort();
        }

        public virtual void OnOpen(TimeSpan timeout)
        {
        }

        public virtual void OnOpened()
        {
            SecurityTraceRecordHelper.TraceTokenProviderOpened(this);
        }

        public virtual void OnOpening()
        {
        }

        public void Open()
        {
            this.communicationObject.Open();
        }

        public void Open(TimeSpan timeout)
        {
            this.communicationObject.Open(timeout);
        }

        protected WrapperSecurityCommunicationObject CommunicationObject
        {
            get
            {
                return this.communicationObject;
            }
        }

        public virtual TimeSpan DefaultCloseTimeout
        {
            get
            {
                return ServiceDefaults.CloseTimeout;
            }
        }

        public virtual TimeSpan DefaultOpenTimeout
        {
            get
            {
                return ServiceDefaults.OpenTimeout;
            }
        }

        public CommunicationState State
        {
            get
            {
                return this.communicationObject.State;
            }
        }
    }
}

