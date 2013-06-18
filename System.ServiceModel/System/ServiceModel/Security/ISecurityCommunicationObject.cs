namespace System.ServiceModel.Security
{
    using System;

    internal interface ISecurityCommunicationObject
    {
        void OnAbort();
        IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state);
        void OnClose(TimeSpan timeout);
        void OnClosed();
        void OnClosing();
        void OnEndClose(IAsyncResult result);
        void OnEndOpen(IAsyncResult result);
        void OnFaulted();
        void OnOpen(TimeSpan timeout);
        void OnOpened();
        void OnOpening();

        TimeSpan DefaultCloseTimeout { get; }

        TimeSpan DefaultOpenTimeout { get; }
    }
}

