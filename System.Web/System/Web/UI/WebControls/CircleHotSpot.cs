namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web;

    public sealed class CircleHotSpot : HotSpot
    {
        public override string GetCoordinates()
        {
            return string.Concat(new object[] { this.X, ",", this.Y, ",", this.Radius });
        }

        protected internal override string MarkupName
        {
            get
            {
                return "circle";
            }
        }

        [WebCategory("Appearance"), DefaultValue(0), WebSysDescription("CircleHotSpot_Radius")]
        public int Radius
        {
            get
            {
                object obj2 = base.ViewState["Radius"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 0;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.ViewState["Radius"] = value;
            }
        }

        [WebSysDescription("CircleHotSpot_X"), DefaultValue(0), WebCategory("Appearance")]
        public int X
        {
            get
            {
                object obj2 = base.ViewState["X"];
                if (obj2 == null)
                {
                    return 0;
                }
                return (int) obj2;
            }
            set
            {
                base.ViewState["X"] = value;
            }
        }

        [WebSysDescription("CircleHotSpot_Y"), DefaultValue(0), WebCategory("Appearance")]
        public int Y
        {
            get
            {
                object obj2 = base.ViewState["Y"];
                if (obj2 == null)
                {
                    return 0;
                }
                return (int) obj2;
            }
            set
            {
                base.ViewState["Y"] = value;
            }
        }
    }
}

