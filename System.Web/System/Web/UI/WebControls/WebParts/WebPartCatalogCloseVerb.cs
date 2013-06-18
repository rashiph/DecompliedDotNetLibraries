namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Web;

    internal sealed class WebPartCatalogCloseVerb : WebPartActionVerb
    {
        [WebSysDefaultValue("WebPartCatalogCloseVerb_Description")]
        public override string Description
        {
            get
            {
                object obj2 = base.ViewState["Description"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("WebPartCatalogCloseVerb_Description");
            }
            set
            {
                base.ViewState["Description"] = value;
            }
        }

        [WebSysDefaultValue("WebPartCatalogCloseVerb_Text")]
        public override string Text
        {
            get
            {
                object obj2 = base.ViewState["Text"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("WebPartCatalogCloseVerb_Text");
            }
            set
            {
                base.ViewState["Text"] = value;
            }
        }
    }
}

