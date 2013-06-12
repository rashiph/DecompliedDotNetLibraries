namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Web;

    public abstract class PersonalizationState
    {
        private bool _dirty;
        private System.Web.UI.WebControls.WebParts.WebPartManager _webPartManager;

        protected PersonalizationState(System.Web.UI.WebControls.WebParts.WebPartManager webPartManager)
        {
            if (webPartManager == null)
            {
                throw new ArgumentNullException("webPartManager");
            }
            this._webPartManager = webPartManager;
        }

        public abstract void ApplyWebPartManagerPersonalization();
        public abstract void ApplyWebPartPersonalization(WebPart webPart);
        public abstract void ExtractWebPartManagerPersonalization();
        public abstract void ExtractWebPartPersonalization(WebPart webPart);
        public abstract string GetAuthorizationFilter(string webPartID);
        protected void SetDirty()
        {
            this._dirty = true;
        }

        public abstract void SetWebPartDirty(WebPart webPart);
        public abstract void SetWebPartManagerDirty();
        protected void ValidateWebPart(WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            if (!this._webPartManager.WebParts.Contains(webPart))
            {
                throw new ArgumentException(System.Web.SR.GetString("UnknownWebPart"), "webPart");
            }
        }

        public bool IsDirty
        {
            get
            {
                return this._dirty;
            }
        }

        public abstract bool IsEmpty { get; }

        public System.Web.UI.WebControls.WebParts.WebPartManager WebPartManager
        {
            get
            {
                return this._webPartManager;
            }
        }
    }
}

