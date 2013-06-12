namespace System.Web.UI.WebControls.WebParts
{
    using System;

    public class WebPartVerbsEventArgs : EventArgs
    {
        private WebPartVerbCollection _verbs;

        public WebPartVerbsEventArgs() : this(null)
        {
        }

        public WebPartVerbsEventArgs(WebPartVerbCollection verbs)
        {
            this._verbs = verbs;
        }

        public WebPartVerbCollection Verbs
        {
            get
            {
                if (this._verbs == null)
                {
                    return WebPartVerbCollection.Empty;
                }
                return this._verbs;
            }
            set
            {
                this._verbs = value;
            }
        }
    }
}

