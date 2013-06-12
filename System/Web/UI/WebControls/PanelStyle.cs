namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    public class PanelStyle : Style
    {
        private const int PROP_BACKIMAGEURL = 0x10000;
        private const int PROP_DIRECTION = 0x20000;
        private const int PROP_HORIZONTALALIGN = 0x40000;
        private const int PROP_SCROLLBARS = 0x80000;
        private const int PROP_WRAP = 0x100000;
        private const string STR_BACKIMAGEURL = "BackImageUrl";
        private const string STR_DIRECTION = "Direction";
        private const string STR_HORIZONTALALIGN = "HorizontalAlign";
        private const string STR_SCROLLBARS = "ScrollBars";
        private const string STR_WRAP = "Wrap";

        public PanelStyle(StateBag bag) : base(bag)
        {
        }

        public override void CopyFrom(Style s)
        {
            if ((s != null) && !s.IsEmpty)
            {
                base.CopyFrom(s);
                if (s is PanelStyle)
                {
                    PanelStyle style = (PanelStyle) s;
                    if (s.RegisteredCssClass.Length != 0)
                    {
                        if (style.IsSet(0x10000))
                        {
                            base.ViewState.Remove("BackImageUrl");
                            base.ClearBit(0x10000);
                        }
                        if (style.IsSet(0x80000))
                        {
                            base.ViewState.Remove("ScrollBars");
                            base.ClearBit(0x80000);
                        }
                        if (style.IsSet(0x100000))
                        {
                            base.ViewState.Remove("Wrap");
                            base.ClearBit(0x100000);
                        }
                    }
                    else
                    {
                        if (style.IsSet(0x10000))
                        {
                            this.BackImageUrl = style.BackImageUrl;
                        }
                        if (style.IsSet(0x80000))
                        {
                            this.ScrollBars = style.ScrollBars;
                        }
                        if (style.IsSet(0x100000))
                        {
                            this.Wrap = style.Wrap;
                        }
                    }
                    if (style.IsSet(0x20000))
                    {
                        this.Direction = style.Direction;
                    }
                    if (style.IsSet(0x40000))
                    {
                        this.HorizontalAlign = style.HorizontalAlign;
                    }
                }
            }
        }

        public override void MergeWith(Style s)
        {
            if ((s != null) && !s.IsEmpty)
            {
                if (this.IsEmpty)
                {
                    this.CopyFrom(s);
                }
                else
                {
                    base.MergeWith(s);
                    if (s is PanelStyle)
                    {
                        PanelStyle style = (PanelStyle) s;
                        if (s.RegisteredCssClass.Length == 0)
                        {
                            if (style.IsSet(0x10000) && !base.IsSet(0x10000))
                            {
                                this.BackImageUrl = style.BackImageUrl;
                            }
                            if (style.IsSet(0x80000) && !base.IsSet(0x80000))
                            {
                                this.ScrollBars = style.ScrollBars;
                            }
                            if (style.IsSet(0x100000) && !base.IsSet(0x100000))
                            {
                                this.Wrap = style.Wrap;
                            }
                        }
                        if (style.IsSet(0x20000) && !base.IsSet(0x20000))
                        {
                            this.Direction = style.Direction;
                        }
                        if (style.IsSet(0x40000) && !base.IsSet(0x40000))
                        {
                            this.HorizontalAlign = style.HorizontalAlign;
                        }
                    }
                }
            }
        }

        public override void Reset()
        {
            if (base.IsSet(0x10000))
            {
                base.ViewState.Remove("BackImageUrl");
            }
            if (base.IsSet(0x20000))
            {
                base.ViewState.Remove("Direction");
            }
            if (base.IsSet(0x40000))
            {
                base.ViewState.Remove("HorizontalAlign");
            }
            if (base.IsSet(0x80000))
            {
                base.ViewState.Remove("ScrollBars");
            }
            if (base.IsSet(0x100000))
            {
                base.ViewState.Remove("Wrap");
            }
            base.Reset();
        }

        [WebSysDescription("Panel_BackImageUrl"), WebCategory("Appearance"), DefaultValue(""), UrlProperty]
        public virtual string BackImageUrl
        {
            get
            {
                if (base.IsSet(0x10000))
                {
                    return (string) base.ViewState["BackImageUrl"];
                }
                return string.Empty;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                base.ViewState["BackImageUrl"] = value;
                this.SetBit(0x10000);
            }
        }

        [DefaultValue(""), WebCategory("Appearance"), WebSysDescription("Panel_Direction")]
        public virtual ContentDirection Direction
        {
            get
            {
                if (base.IsSet(0x20000))
                {
                    return (ContentDirection) base.ViewState["Direction"];
                }
                return ContentDirection.NotSet;
            }
            set
            {
                if ((value < ContentDirection.NotSet) || (value > ContentDirection.RightToLeft))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.ViewState["Direction"] = value;
                this.SetBit(0x20000);
            }
        }

        [WebSysDescription("Panel_HorizontalAlign"), DefaultValue(""), WebCategory("Appearance")]
        public virtual System.Web.UI.WebControls.HorizontalAlign HorizontalAlign
        {
            get
            {
                if (base.IsSet(0x40000))
                {
                    return (System.Web.UI.WebControls.HorizontalAlign) base.ViewState["HorizontalAlign"];
                }
                return System.Web.UI.WebControls.HorizontalAlign.NotSet;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.HorizontalAlign.NotSet) || (value > System.Web.UI.WebControls.HorizontalAlign.Justify))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.ViewState["HorizontalAlign"] = value;
                this.SetBit(0x40000);
            }
        }

        [WebSysDescription("Panel_ScrollBars"), DefaultValue(""), WebCategory("Appearance")]
        public virtual System.Web.UI.WebControls.ScrollBars ScrollBars
        {
            get
            {
                if (base.IsSet(0x80000))
                {
                    return (System.Web.UI.WebControls.ScrollBars) base.ViewState["ScrollBars"];
                }
                return System.Web.UI.WebControls.ScrollBars.None;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.ScrollBars.None) || (value > System.Web.UI.WebControls.ScrollBars.Auto))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.ViewState["ScrollBars"] = value;
                this.SetBit(0x80000);
            }
        }

        [WebSysDescription("Panel_Wrap"), DefaultValue(""), WebCategory("Appearance")]
        public virtual bool Wrap
        {
            get
            {
                if (base.IsSet(0x100000))
                {
                    return (bool) base.ViewState["Wrap"];
                }
                return true;
            }
            set
            {
                base.ViewState["Wrap"] = value;
                this.SetBit(0x100000);
            }
        }
    }
}

