namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web;

    public sealed class DataGridPagerStyle : TableItemStyle
    {
        private DataGrid owner;
        private const int PROP_MODE = 0x80000;
        private const int PROP_NEXTPAGETEXT = 0x100000;
        private const int PROP_PAGEBUTTONCOUNT = 0x400000;
        private const int PROP_POSITION = 0x800000;
        private const int PROP_PREVPAGETEXT = 0x200000;
        private const int PROP_VISIBLE = 0x1000000;

        internal DataGridPagerStyle(DataGrid owner)
        {
            this.owner = owner;
        }

        public override void CopyFrom(Style s)
        {
            if ((s != null) && !s.IsEmpty)
            {
                base.CopyFrom(s);
                if (s is DataGridPagerStyle)
                {
                    DataGridPagerStyle style = (DataGridPagerStyle) s;
                    if (style.IsSet(0x80000))
                    {
                        this.Mode = style.Mode;
                    }
                    if (style.IsSet(0x100000))
                    {
                        this.NextPageText = style.NextPageText;
                    }
                    if (style.IsSet(0x200000))
                    {
                        this.PrevPageText = style.PrevPageText;
                    }
                    if (style.IsSet(0x400000))
                    {
                        this.PageButtonCount = style.PageButtonCount;
                    }
                    if (style.IsSet(0x800000))
                    {
                        this.Position = style.Position;
                    }
                    if (style.IsSet(0x1000000))
                    {
                        this.Visible = style.Visible;
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
                    if (s is DataGridPagerStyle)
                    {
                        DataGridPagerStyle style = (DataGridPagerStyle) s;
                        if (style.IsSet(0x80000) && !base.IsSet(0x80000))
                        {
                            this.Mode = style.Mode;
                        }
                        if (style.IsSet(0x100000) && !base.IsSet(0x100000))
                        {
                            this.NextPageText = style.NextPageText;
                        }
                        if (style.IsSet(0x200000) && !base.IsSet(0x200000))
                        {
                            this.PrevPageText = style.PrevPageText;
                        }
                        if (style.IsSet(0x400000) && !base.IsSet(0x400000))
                        {
                            this.PageButtonCount = style.PageButtonCount;
                        }
                        if (style.IsSet(0x800000) && !base.IsSet(0x800000))
                        {
                            this.Position = style.Position;
                        }
                        if (style.IsSet(0x1000000) && !base.IsSet(0x1000000))
                        {
                            this.Visible = style.Visible;
                        }
                    }
                }
            }
        }

        public override void Reset()
        {
            if (base.IsSet(0x80000))
            {
                base.ViewState.Remove("Mode");
            }
            if (base.IsSet(0x100000))
            {
                base.ViewState.Remove("NextPageText");
            }
            if (base.IsSet(0x200000))
            {
                base.ViewState.Remove("PrevPageText");
            }
            if (base.IsSet(0x400000))
            {
                base.ViewState.Remove("PageButtonCount");
            }
            if (base.IsSet(0x800000))
            {
                base.ViewState.Remove("Position");
            }
            if (base.IsSet(0x1000000))
            {
                base.ViewState.Remove("PagerVisible");
            }
            base.Reset();
        }

        internal bool IsPagerOnBottom
        {
            get
            {
                PagerPosition position = this.Position;
                if (position != PagerPosition.Bottom)
                {
                    return (position == PagerPosition.TopAndBottom);
                }
                return true;
            }
        }

        internal bool IsPagerOnTop
        {
            get
            {
                PagerPosition position = this.Position;
                if (position != PagerPosition.Top)
                {
                    return (position == PagerPosition.TopAndBottom);
                }
                return true;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("DataGridPagerStyle_Mode"), DefaultValue(0), NotifyParentProperty(true)]
        public PagerMode Mode
        {
            get
            {
                if (base.IsSet(0x80000))
                {
                    return (PagerMode) base.ViewState["Mode"];
                }
                return PagerMode.NextPrev;
            }
            set
            {
                if ((value < PagerMode.NextPrev) || (value > PagerMode.NumericPages))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.ViewState["Mode"] = value;
                this.SetBit(0x80000);
                this.owner.OnPagerChanged();
            }
        }

        [DefaultValue("&gt;"), Localizable(true), WebCategory("Appearance"), NotifyParentProperty(true), WebSysDescription("PagerSettings_NextPageText")]
        public string NextPageText
        {
            get
            {
                if (base.IsSet(0x100000))
                {
                    return (string) base.ViewState["NextPageText"];
                }
                return "&gt;";
            }
            set
            {
                base.ViewState["NextPageText"] = value;
                this.SetBit(0x100000);
                this.owner.OnPagerChanged();
            }
        }

        [WebSysDescription("DataGridPagerStyle_PageButtonCount"), WebCategory("Behavior"), DefaultValue(10), NotifyParentProperty(true)]
        public int PageButtonCount
        {
            get
            {
                if (base.IsSet(0x400000))
                {
                    return (int) base.ViewState["PageButtonCount"];
                }
                return 10;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.ViewState["PageButtonCount"] = value;
                this.SetBit(0x400000);
                this.owner.OnPagerChanged();
            }
        }

        [DefaultValue(0), WebCategory("Layout"), WebSysDescription("DataGridPagerStyle_Position"), NotifyParentProperty(true)]
        public PagerPosition Position
        {
            get
            {
                if (base.IsSet(0x800000))
                {
                    return (PagerPosition) base.ViewState["Position"];
                }
                return PagerPosition.Bottom;
            }
            set
            {
                if ((value < PagerPosition.Bottom) || (value > PagerPosition.TopAndBottom))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                base.ViewState["Position"] = value;
                this.SetBit(0x800000);
                this.owner.OnPagerChanged();
            }
        }

        [WebSysDescription("PagerSettings_PreviousPageText"), Localizable(true), WebCategory("Appearance"), DefaultValue("&lt;"), NotifyParentProperty(true)]
        public string PrevPageText
        {
            get
            {
                if (base.IsSet(0x200000))
                {
                    return (string) base.ViewState["PrevPageText"];
                }
                return "&lt;";
            }
            set
            {
                base.ViewState["PrevPageText"] = value;
                this.SetBit(0x200000);
                this.owner.OnPagerChanged();
            }
        }

        [WebCategory("Appearance"), WebSysDescription("DataGridPagerStyle_Visible"), DefaultValue(true), NotifyParentProperty(true)]
        public bool Visible
        {
            get
            {
                if (base.IsSet(0x1000000))
                {
                    return (bool) base.ViewState["PagerVisible"];
                }
                return true;
            }
            set
            {
                base.ViewState["PagerVisible"] = value;
                this.SetBit(0x1000000);
                this.owner.OnPagerChanged();
            }
        }
    }
}

