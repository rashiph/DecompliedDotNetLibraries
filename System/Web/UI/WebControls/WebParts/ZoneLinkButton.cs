namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [SupportsEventValidation]
    internal sealed class ZoneLinkButton : LinkButton
    {
        private string _eventArgument;
        private string _imageUrl;
        private WebZone _owner;

        public ZoneLinkButton(WebZone owner, string eventArgument)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            this._owner = owner;
            this._eventArgument = eventArgument;
        }

        protected override PostBackOptions GetPostBackOptions()
        {
            if (!string.IsNullOrEmpty(this._eventArgument) && (this._owner.Page != null))
            {
                return new PostBackOptions(this._owner, this._eventArgument) { RequiresJavaScriptProtocol = true };
            }
            return base.GetPostBackOptions();
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            string imageUrl = this.ImageUrl;
            if (!string.IsNullOrEmpty(imageUrl))
            {
                Image image = new Image {
                    ImageUrl = base.ResolveClientUrl(imageUrl)
                };
                string toolTip = this.ToolTip;
                if (!string.IsNullOrEmpty(toolTip))
                {
                    image.ToolTip = toolTip;
                }
                string text = this.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    image.AlternateText = text;
                }
                image.Page = this.Page;
                image.RenderControl(writer);
            }
            else
            {
                base.RenderContents(writer);
            }
        }

        public string ImageUrl
        {
            get
            {
                if (this._imageUrl == null)
                {
                    return string.Empty;
                }
                return this._imageUrl;
            }
            set
            {
                this._imageUrl = value;
            }
        }
    }
}

