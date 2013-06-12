namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Web;

    internal sealed class WebPartConnectionsDisconnectVerb : WebPartActionVerb
    {
        [WebSysDefaultValue("WebPartConnectionsDisconnectVerb_Description")]
        public override string Description
        {
            get
            {
                object obj2 = base.ViewState["Description"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("WebPartConnectionsDisconnectVerb_Description");
            }
            set
            {
                base.ViewState["Description"] = value;
            }
        }

        [WebSysDefaultValue("WebPartConnectionsDisconnectVerb_Text")]
        public override string Text
        {
            get
            {
                object obj2 = base.ViewState["Text"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return System.Web.SR.GetString("WebPartConnectionsDisconnectVerb_Text");
            }
            set
            {
                base.ViewState["Text"] = value;
            }
        }
    }
}

