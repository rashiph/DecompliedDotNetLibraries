namespace System.Web.UI.WebControls.WebParts
{
    using System;

    public class WebPartDisplayModeEventArgs : EventArgs
    {
        private WebPartDisplayMode _oldDisplayMode;

        public WebPartDisplayModeEventArgs(WebPartDisplayMode oldDisplayMode)
        {
            this._oldDisplayMode = oldDisplayMode;
        }

        public WebPartDisplayMode OldDisplayMode
        {
            get
            {
                return this._oldDisplayMode;
            }
            set
            {
                this._oldDisplayMode = value;
            }
        }
    }
}

