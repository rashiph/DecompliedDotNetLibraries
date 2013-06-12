namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web;

    public class TableSectionStyle : Style
    {
        [NotifyParentProperty(true), WebSysDescription("TableSectionStyle_Visible"), WebCategory("Behavior"), DefaultValue(true)]
        public bool Visible
        {
            get
            {
                object obj2 = base.ViewState["Visible"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                base.ViewState["Visible"] = value;
            }
        }
    }
}

