namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web;

    public sealed class PolygonHotSpot : HotSpot
    {
        public override string GetCoordinates()
        {
            return this.Coordinates;
        }

        [DefaultValue(""), WebCategory("Appearance"), WebSysDescription("PolygonHotSpot_Coordinates")]
        public string Coordinates
        {
            get
            {
                string str = base.ViewState["Coordinates"] as string;
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                base.ViewState["Coordinates"] = value;
            }
        }

        protected internal override string MarkupName
        {
            get
            {
                return "poly";
            }
        }
    }
}

