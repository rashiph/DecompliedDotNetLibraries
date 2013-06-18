namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;

    public class WebPartCancelEventArgs : CancelEventArgs
    {
        private System.Web.UI.WebControls.WebParts.WebPart _webPart;

        public WebPartCancelEventArgs(System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            this._webPart = webPart;
        }

        public System.Web.UI.WebControls.WebParts.WebPart WebPart
        {
            get
            {
                return this._webPart;
            }
            set
            {
                this._webPart = value;
            }
        }
    }
}

