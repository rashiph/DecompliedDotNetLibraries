namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [ToolboxItem(false)]
    public class ErrorWebPart : ProxyWebPart, ITrackingPersonalizable
    {
        private string _errorMessage;

        public ErrorWebPart(string originalID, string originalTypeName, string originalPath, string genericWebPartID) : base(originalID, originalTypeName, originalPath, genericWebPartID)
        {
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            WebPartZoneBase zone = base.Zone;
            if ((zone != null) && !zone.ErrorStyle.IsEmpty)
            {
                zone.ErrorStyle.AddAttributesToRender(writer, this);
            }
            base.AddAttributesToRender(writer);
        }

        protected virtual void EndLoadPersonalization()
        {
            this.AllowEdit = false;
            this.ChromeState = PartChromeState.Normal;
            this.Hidden = false;
            this.AllowHide = false;
            this.AllowMinimize = false;
            this.ExportMode = WebPartExportMode.None;
            this.AuthorizationFilter = string.Empty;
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            string errorMessage = this.ErrorMessage;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                writer.WriteEncodedText(System.Web.SR.GetString("ErrorWebPart_ErrorText", new object[] { errorMessage }));
            }
        }

        void ITrackingPersonalizable.BeginLoad()
        {
        }

        void ITrackingPersonalizable.BeginSave()
        {
        }

        void ITrackingPersonalizable.EndLoad()
        {
            this.EndLoadPersonalization();
        }

        void ITrackingPersonalizable.EndSave()
        {
        }

        public string ErrorMessage
        {
            get
            {
                if (this._errorMessage == null)
                {
                    return string.Empty;
                }
                return this._errorMessage;
            }
            set
            {
                this._errorMessage = value;
            }
        }

        bool ITrackingPersonalizable.TracksChanges
        {
            get
            {
                return true;
            }
        }
    }
}

