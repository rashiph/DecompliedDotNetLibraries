namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Web;

    public sealed class WebPartTracker : IDisposable
    {
        private bool _disposed;
        private ProviderConnectionPoint _providerConnectionPoint;
        private WebPart _webPart;

        public WebPartTracker(WebPart webPart, ProviderConnectionPoint providerConnectionPoint)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if (providerConnectionPoint == null)
            {
                throw new ArgumentNullException("providerConnectionPoint");
            }
            if (providerConnectionPoint.ControlType != webPart.GetType())
            {
                throw new ArgumentException(System.Web.SR.GetString("WebPartManager_InvalidConnectionPoint"), "providerConnectionPoint");
            }
            this._webPart = webPart;
            this._providerConnectionPoint = providerConnectionPoint;
            if (++this.Count > 1)
            {
                webPart.SetConnectErrorMessage(System.Web.SR.GetString("WebPartTracker_CircularConnection", new object[] { this._providerConnectionPoint.DisplayName }));
            }
        }

        void IDisposable.Dispose()
        {
            if (!this._disposed)
            {
                this.Count--;
                this._disposed = true;
            }
        }

        private int Count
        {
            get
            {
                int num;
                this._webPart.TrackerCounter.TryGetValue(this._providerConnectionPoint, out num);
                return num;
            }
            set
            {
                this._webPart.TrackerCounter[this._providerConnectionPoint] = value;
            }
        }

        public bool IsCircularConnection
        {
            get
            {
                return (this.Count > 1);
            }
        }
    }
}

