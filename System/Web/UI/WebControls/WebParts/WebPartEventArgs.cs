namespace System.Web.UI.WebControls.WebParts
{
    using System;

    public class WebPartEventArgs : EventArgs
    {
        private System.Web.UI.WebControls.WebParts.WebPart _webPart;

        public WebPartEventArgs(System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            this._webPart = webPart;
        }

        public System.Web.UI.WebControls.WebParts.WebPart WebPart
        {
            get
            {
                return this._webPart;
            }
        }
    }
}

