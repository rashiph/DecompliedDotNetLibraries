namespace System.ServiceModel.Activation
{
    using System;

    internal interface IAspNetMessageProperty
    {
        IDisposable ApplyIntegrationContext();
        void Close();
        IDisposable Impersonate();

        Uri OriginalRequestUri { get; }
    }
}

