namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;

    public class WebPartDisplayModeCancelEventArgs : CancelEventArgs
    {
        private WebPartDisplayMode _newDisplayMode;

        public WebPartDisplayModeCancelEventArgs(WebPartDisplayMode newDisplayMode)
        {
            this._newDisplayMode = newDisplayMode;
        }

        public WebPartDisplayMode NewDisplayMode
        {
            get
            {
                return this._newDisplayMode;
            }
            set
            {
                this._newDisplayMode = value;
            }
        }
    }
}

