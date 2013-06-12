namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web;

    public sealed class RectangleHotSpot : HotSpot
    {
        public override string GetCoordinates()
        {
            return string.Concat(new object[] { this.Left, ",", this.Top, ",", this.Right, ",", this.Bottom });
        }

        [DefaultValue(0), WebCategory("Appearance"), WebSysDescription("RectangleHotSpot_Bottom")]
        public int Bottom
        {
            get
            {
                object obj2 = base.ViewState["Bottom"];
                if (obj2 == null)
                {
                    return 0;
                }
                return (int) obj2;
            }
            set
            {
                base.ViewState["Bottom"] = value;
            }
        }

        [WebSysDescription("RectangleHotSpot_Left"), WebCategory("Appearance"), DefaultValue(0)]
        public int Left
        {
            get
            {
                object obj2 = base.ViewState["Left"];
                if (obj2 == null)
                {
                    return 0;
                }
                return (int) obj2;
            }
            set
            {
                base.ViewState["Left"] = value;
            }
        }

        protected internal override string MarkupName
        {
            get
            {
                return "rect";
            }
        }

        [WebCategory("Appearance"), WebSysDescription("RectangleHotSpot_Right"), DefaultValue(0)]
        public int Right
        {
            get
            {
                object obj2 = base.ViewState["Right"];
                if (obj2 == null)
                {
                    return 0;
                }
                return (int) obj2;
            }
            set
            {
                base.ViewState["Right"] = value;
            }
        }

        [WebSysDescription("RectangleHotSpot_Top"), WebCategory("Appearance"), DefaultValue(0)]
        public int Top
        {
            get
            {
                object obj2 = base.ViewState["Top"];
                if (obj2 == null)
                {
                    return 0;
                }
                return (int) obj2;
            }
            set
            {
                base.ViewState["Top"] = value;
            }
        }
    }
}

