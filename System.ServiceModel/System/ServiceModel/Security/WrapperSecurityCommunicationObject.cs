namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class WrapperSecurityCommunicationObject : CommunicationObject
    {
        private ISecurityCommunicationObject innerCommunicationObject;

        public WrapperSecurityCommunicationObject(ISecurityCommunicationObject innerCommunicationObject)
        {
            if (innerCommunicationObject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerCommunicationObject");
            }
            this.innerCommunicationObject = innerCommunicationObject;
        }

        protected override System.Type GetCommunicationObjectType()
        {
            return this.innerCommunicationObject.GetType();
        }

        protected override void OnAbort()
        {
            this.innerCommunicationObject.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerCommunicationObject.OnBeginClose(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerCommunicationObject.OnBeginOpen(timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.innerCommunicationObject.OnClose(timeout);
        }

        protected override void OnClosed()
        {
            this.innerCommunicationObject.OnClosed();
            base.OnClosed();
        }

        protected override void OnClosing()
        {
            this.innerCommunicationObject.OnClosing();
            base.OnClosing();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            this.innerCommunicationObject.OnEndClose(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.innerCommunicationObject.OnEndOpen(result);
        }

        protected override void OnFaulted()
        {
            this.innerCommunicationObject.OnFaulted();
            base.OnFaulted();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.innerCommunicationObject.OnOpen(timeout);
        }

        protected override void OnOpened()
        {
            this.innerCommunicationObject.OnOpened();
            base.OnOpened();
        }

        protected override void OnOpening()
        {
            this.innerCommunicationObject.OnOpening();
            base.OnOpening();
        }

        internal void ThrowIfDisposedOrImmutable()
        {
            base.ThrowIfDisposedOrImmutable();
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                return this.innerCommunicationObject.DefaultCloseTimeout;
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                return this.innerCommunicationObject.DefaultOpenTimeout;
            }
        }
    }
}

