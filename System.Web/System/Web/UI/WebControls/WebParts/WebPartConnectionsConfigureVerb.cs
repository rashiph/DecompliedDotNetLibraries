namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Web;

    internal sealed class WebPartConnectionsConfigureVerb : WebPartActionVerb
    {
        [WebSysDefaultValue("WebPartConnectionsConfigureVerb_Description")]
        public override string Description
        {
            get
            {
                object obj2 = base.ViewState["Description"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("WebPartConnectionsConfigureVerb_Description");
            }
            set
            {
                base.ViewState["Description"] = value;
            }
        }

        [WebSysDefaultValue("WebPartConnectionsConfigureVerb_Text")]
        public override string Text
        {
            get
            {
                object obj2 = base.ViewState["Text"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("WebPartConnectionsConfigureVerb_Text");
            }
            set
            {
                base.ViewState["Text"] = value;
            }
        }
    }
}

