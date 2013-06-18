namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime;
    using System.Web;
    using System.Web.UI;

    [TypeConverter(typeof(EmptyStringExpandableObjectConverter)), ToolboxItem(false)]
    public class Style : Component, IStateManager
    {
        internal static readonly string[] borderStyles = new string[] { "NotSet", "None", "Dotted", "Dashed", "Solid", "Double", "Groove", "Ridge", "Inset", "Outset" };
        private FontInfo fontInfo;
        private bool marked;
        private int markedBits;
        private bool ownStateBag;
        internal const int PROP_BACKCOLOR = 8;
        internal const int PROP_BORDERCOLOR = 0x10;
        internal const int PROP_BORDERSTYLE = 0x40;
        internal const int PROP_BORDERWIDTH = 0x20;
        internal const int PROP_CSSCLASS = 2;
        internal const int PROP_FONT_BOLD = 0x800;
        internal const int PROP_FONT_ITALIC = 0x1000;
        internal const int PROP_FONT_NAMES = 0x200;
        internal const int PROP_FONT_OVERLINE = 0x4000;
        internal const int PROP_FONT_SIZE = 0x400;
        internal const int PROP_FONT_STRIKEOUT = 0x8000;
        internal const int PROP_FONT_UNDERLINE = 0x2000;
        internal const int PROP_FORECOLOR = 4;
        internal const int PROP_HEIGHT = 0x80;
        internal const int PROP_WIDTH = 0x100;
        private string registeredCssClass;
        private int setBits;
        internal const string SetBitsKey = "_!SB";
        private StateBag statebag;
        internal const int UNUSED = 1;

        public Style() : this(null)
        {
            this.ownStateBag = true;
        }

        public Style(StateBag bag)
        {
            this.statebag = bag;
            this.marked = false;
            this.setBits = 0;
            GC.SuppressFinalize(this);
        }

        public void AddAttributesToRender(HtmlTextWriter writer)
        {
            this.AddAttributesToRender(writer, null);
        }

        public virtual void AddAttributesToRender(HtmlTextWriter writer, WebControl owner)
        {
            string registeredCssClass = string.Empty;
            bool flag = true;
            if (this.IsSet(2))
            {
                registeredCssClass = (string) this.ViewState["CssClass"];
                if (registeredCssClass == null)
                {
                    registeredCssClass = string.Empty;
                }
            }
            if (!string.IsNullOrEmpty(this.registeredCssClass))
            {
                flag = false;
                if (registeredCssClass.Length != 0)
                {
                    registeredCssClass = registeredCssClass + " " + this.registeredCssClass;
                }
                else
                {
                    registeredCssClass = this.registeredCssClass;
                }
            }
            if (registeredCssClass.Length > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, registeredCssClass);
            }
            if (flag)
            {
                this.GetStyleAttributes(owner).Render(writer);
            }
        }

        internal void ClearBit(int bit)
        {
            this.setBits &= ~bit;
        }

        public virtual void CopyFrom(Style s)
        {
            if (this.RegisteredCssClass.Length != 0)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Style_RegisteredStylesAreReadOnly"));
            }
            if ((s != null) && !s.IsEmpty)
            {
                this.Font.CopyFrom(s.Font);
                if (s.IsSet(2))
                {
                    this.CssClass = s.CssClass;
                }
                if (s.RegisteredCssClass.Length != 0)
                {
                    if (this.IsSet(2))
                    {
                        this.CssClass = this.CssClass + " " + s.RegisteredCssClass;
                    }
                    else
                    {
                        this.CssClass = s.RegisteredCssClass;
                    }
                    if (s.IsSet(8) && (s.BackColor != Color.Empty))
                    {
                        this.ViewState.Remove("BackColor");
                        this.ClearBit(8);
                    }
                    if (s.IsSet(4) && (s.ForeColor != Color.Empty))
                    {
                        this.ViewState.Remove("ForeColor");
                        this.ClearBit(4);
                    }
                    if (s.IsSet(0x10) && (s.BorderColor != Color.Empty))
                    {
                        this.ViewState.Remove("BorderColor");
                        this.ClearBit(0x10);
                    }
                    if (s.IsSet(0x20) && (s.BorderWidth != Unit.Empty))
                    {
                        this.ViewState.Remove("BorderWidth");
                        this.ClearBit(0x20);
                    }
                    if (s.IsSet(0x40))
                    {
                        this.ViewState.Remove("BorderStyle");
                        this.ClearBit(0x40);
                    }
                    if (s.IsSet(0x80) && (s.Height != Unit.Empty))
                    {
                        this.ViewState.Remove("Height");
                        this.ClearBit(0x80);
                    }
                    if (s.IsSet(0x100) && (s.Width != Unit.Empty))
                    {
                        this.ViewState.Remove("Width");
                        this.ClearBit(0x100);
                    }
                }
                else
                {
                    if (s.IsSet(8) && (s.BackColor != Color.Empty))
                    {
                        this.BackColor = s.BackColor;
                    }
                    if (s.IsSet(4) && (s.ForeColor != Color.Empty))
                    {
                        this.ForeColor = s.ForeColor;
                    }
                    if (s.IsSet(0x10) && (s.BorderColor != Color.Empty))
                    {
                        this.BorderColor = s.BorderColor;
                    }
                    if (s.IsSet(0x20) && (s.BorderWidth != Unit.Empty))
                    {
                        this.BorderWidth = s.BorderWidth;
                    }
                    if (s.IsSet(0x40))
                    {
                        this.BorderStyle = s.BorderStyle;
                    }
                    if (s.IsSet(0x80) && (s.Height != Unit.Empty))
                    {
                        this.Height = s.Height;
                    }
                    if (s.IsSet(0x100) && (s.Width != Unit.Empty))
                    {
                        this.Width = s.Width;
                    }
                }
            }
        }

        protected virtual void FillStyleAttributes(CssStyleCollection attributes, IUrlResolutionService urlResolver)
        {
            Color color;
            Unit unit3;
            StateBag viewState = this.ViewState;
            if (this.IsSet(4))
            {
                color = (Color) viewState["ForeColor"];
                if (!color.IsEmpty)
                {
                    attributes.Add(HtmlTextWriterStyle.Color, ColorTranslator.ToHtml(color));
                }
            }
            if (this.IsSet(8))
            {
                color = (Color) viewState["BackColor"];
                if (!color.IsEmpty)
                {
                    attributes.Add(HtmlTextWriterStyle.BackgroundColor, ColorTranslator.ToHtml(color));
                }
            }
            if (this.IsSet(0x10))
            {
                color = (Color) viewState["BorderColor"];
                if (!color.IsEmpty)
                {
                    attributes.Add(HtmlTextWriterStyle.BorderColor, ColorTranslator.ToHtml(color));
                }
            }
            System.Web.UI.WebControls.BorderStyle borderStyle = this.BorderStyle;
            Unit borderWidth = this.BorderWidth;
            if (!borderWidth.IsEmpty)
            {
                attributes.Add(HtmlTextWriterStyle.BorderWidth, borderWidth.ToString(CultureInfo.InvariantCulture));
                if (borderStyle == System.Web.UI.WebControls.BorderStyle.NotSet)
                {
                    if (borderWidth.Value != 0.0)
                    {
                        attributes.Add(HtmlTextWriterStyle.BorderStyle, "solid");
                    }
                }
                else
                {
                    attributes.Add(HtmlTextWriterStyle.BorderStyle, borderStyles[(int) borderStyle]);
                }
            }
            else if (borderStyle != System.Web.UI.WebControls.BorderStyle.NotSet)
            {
                attributes.Add(HtmlTextWriterStyle.BorderStyle, borderStyles[(int) borderStyle]);
            }
            FontInfo font = this.Font;
            string[] names = font.Names;
            if (names.Length > 0)
            {
                attributes.Add(HtmlTextWriterStyle.FontFamily, FormatStringArray(names, ','));
            }
            FontUnit size = font.Size;
            if (!size.IsEmpty)
            {
                attributes.Add(HtmlTextWriterStyle.FontSize, size.ToString(CultureInfo.InvariantCulture));
            }
            if (this.IsSet(0x800))
            {
                if (font.Bold)
                {
                    attributes.Add(HtmlTextWriterStyle.FontWeight, "bold");
                }
                else
                {
                    attributes.Add(HtmlTextWriterStyle.FontWeight, "normal");
                }
            }
            if (this.IsSet(0x1000))
            {
                if (font.Italic)
                {
                    attributes.Add(HtmlTextWriterStyle.FontStyle, "italic");
                }
                else
                {
                    attributes.Add(HtmlTextWriterStyle.FontStyle, "normal");
                }
            }
            string str = string.Empty;
            if (font.Underline)
            {
                str = "underline";
            }
            if (font.Overline)
            {
                str = str + " overline";
            }
            if (font.Strikeout)
            {
                str = str + " line-through";
            }
            if (str.Length > 0)
            {
                attributes.Add(HtmlTextWriterStyle.TextDecoration, str);
            }
            else if ((this.IsSet(0x2000) || this.IsSet(0x4000)) || this.IsSet(0x8000))
            {
                attributes.Add(HtmlTextWriterStyle.TextDecoration, "none");
            }
            if (this.IsSet(0x80))
            {
                unit3 = (Unit) viewState["Height"];
                if (!unit3.IsEmpty)
                {
                    attributes.Add(HtmlTextWriterStyle.Height, unit3.ToString(CultureInfo.InvariantCulture));
                }
            }
            if (this.IsSet(0x100))
            {
                unit3 = (Unit) viewState["Width"];
                if (!unit3.IsEmpty)
                {
                    attributes.Add(HtmlTextWriterStyle.Width, unit3.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        private static string FormatStringArray(string[] array, char delimiter)
        {
            switch (array.Length)
            {
                case 1:
                    return array[0];

                case 0:
                    return string.Empty;
            }
            return string.Join(delimiter.ToString(CultureInfo.InvariantCulture), array);
        }

        public CssStyleCollection GetStyleAttributes(IUrlResolutionService urlResolver)
        {
            CssStyleCollection attributes = new CssStyleCollection();
            this.FillStyleAttributes(attributes, urlResolver);
            return attributes;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal bool IsSet(int propKey)
        {
            return ((this.setBits & propKey) != 0);
        }

        protected internal void LoadViewState(object state)
        {
            if ((state != null) && this.ownStateBag)
            {
                this.ViewState.LoadViewState(state);
            }
            if (this.statebag != null)
            {
                object obj2 = this.ViewState["_!SB"];
                if (obj2 != null)
                {
                    this.markedBits = (int) obj2;
                    this.setBits |= this.markedBits;
                }
            }
        }

        public virtual void MergeWith(Style s)
        {
            if (this.RegisteredCssClass.Length != 0)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Style_RegisteredStylesAreReadOnly"));
            }
            if ((s != null) && !s.IsEmpty)
            {
                if (this.IsEmpty)
                {
                    this.CopyFrom(s);
                }
                else
                {
                    this.Font.MergeWith(s.Font);
                    if (s.IsSet(2) && !this.IsSet(2))
                    {
                        this.CssClass = s.CssClass;
                    }
                    if (s.RegisteredCssClass.Length == 0)
                    {
                        if (s.IsSet(8) && (!this.IsSet(8) || (this.BackColor == Color.Empty)))
                        {
                            this.BackColor = s.BackColor;
                        }
                        if (s.IsSet(4) && (!this.IsSet(4) || (this.ForeColor == Color.Empty)))
                        {
                            this.ForeColor = s.ForeColor;
                        }
                        if (s.IsSet(0x10) && (!this.IsSet(0x10) || (this.BorderColor == Color.Empty)))
                        {
                            this.BorderColor = s.BorderColor;
                        }
                        if (s.IsSet(0x20) && (!this.IsSet(0x20) || (this.BorderWidth == Unit.Empty)))
                        {
                            this.BorderWidth = s.BorderWidth;
                        }
                        if (s.IsSet(0x40) && !this.IsSet(0x40))
                        {
                            this.BorderStyle = s.BorderStyle;
                        }
                        if (s.IsSet(0x80) && (!this.IsSet(0x80) || (this.Height == Unit.Empty)))
                        {
                            this.Height = s.Height;
                        }
                        if (s.IsSet(0x100) && (!this.IsSet(0x100) || (this.Width == Unit.Empty)))
                        {
                            this.Width = s.Width;
                        }
                    }
                    else if (this.IsSet(2))
                    {
                        this.CssClass = this.CssClass + " " + s.RegisteredCssClass;
                    }
                    else
                    {
                        this.CssClass = s.RegisteredCssClass;
                    }
                }
            }
        }

        public virtual void Reset()
        {
            if (this.statebag != null)
            {
                if (this.IsSet(2))
                {
                    this.ViewState.Remove("CssClass");
                }
                if (this.IsSet(8))
                {
                    this.ViewState.Remove("BackColor");
                }
                if (this.IsSet(4))
                {
                    this.ViewState.Remove("ForeColor");
                }
                if (this.IsSet(0x10))
                {
                    this.ViewState.Remove("BorderColor");
                }
                if (this.IsSet(0x20))
                {
                    this.ViewState.Remove("BorderWidth");
                }
                if (this.IsSet(0x40))
                {
                    this.ViewState.Remove("BorderStyle");
                }
                if (this.IsSet(0x80))
                {
                    this.ViewState.Remove("Height");
                }
                if (this.IsSet(0x100))
                {
                    this.ViewState.Remove("Width");
                }
                this.Font.Reset();
                this.ViewState.Remove("_!SB");
                this.markedBits = 0;
            }
            this.setBits = 0;
        }

        protected internal virtual object SaveViewState()
        {
            if (this.statebag != null)
            {
                if (this.markedBits != 0)
                {
                    this.ViewState["_!SB"] = this.markedBits;
                }
                if (this.ownStateBag)
                {
                    return this.ViewState.SaveViewState();
                }
            }
            return null;
        }

        protected internal virtual void SetBit(int bit)
        {
            this.setBits |= bit;
            if (this.IsTrackingViewState)
            {
                this.markedBits |= bit;
            }
        }

        public void SetDirty()
        {
            this.ViewState.SetDirty(true);
            this.markedBits = this.setBits;
        }

        internal void SetRegisteredCssClass(string cssClass)
        {
            this.registeredCssClass = cssClass;
        }

        void IStateManager.LoadViewState(object state)
        {
            this.LoadViewState(state);
        }

        object IStateManager.SaveViewState()
        {
            return this.SaveViewState();
        }

        void IStateManager.TrackViewState()
        {
            this.TrackViewState();
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected internal virtual void TrackViewState()
        {
            if (this.ownStateBag)
            {
                this.ViewState.TrackViewState();
            }
            this.marked = true;
        }

        [NotifyParentProperty(true), WebCategory("Appearance"), DefaultValue(typeof(Color), ""), WebSysDescription("Style_BackColor"), TypeConverter(typeof(WebColorConverter))]
        public Color BackColor
        {
            get
            {
                if (this.IsSet(8))
                {
                    return (Color) this.ViewState["BackColor"];
                }
                return Color.Empty;
            }
            set
            {
                this.ViewState["BackColor"] = value;
                this.SetBit(8);
            }
        }

        [NotifyParentProperty(true), WebCategory("Appearance"), DefaultValue(typeof(Color), ""), WebSysDescription("Style_BorderColor"), TypeConverter(typeof(WebColorConverter))]
        public Color BorderColor
        {
            get
            {
                if (this.IsSet(0x10))
                {
                    return (Color) this.ViewState["BorderColor"];
                }
                return Color.Empty;
            }
            set
            {
                this.ViewState["BorderColor"] = value;
                this.SetBit(0x10);
            }
        }

        [NotifyParentProperty(true), WebCategory("Appearance"), DefaultValue(0), WebSysDescription("Style_BorderStyle")]
        public System.Web.UI.WebControls.BorderStyle BorderStyle
        {
            get
            {
                if (this.IsSet(0x40))
                {
                    return (System.Web.UI.WebControls.BorderStyle) this.ViewState["BorderStyle"];
                }
                return System.Web.UI.WebControls.BorderStyle.NotSet;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.BorderStyle.NotSet) || (value > System.Web.UI.WebControls.BorderStyle.Outset))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["BorderStyle"] = value;
                this.SetBit(0x40);
            }
        }

        [WebCategory("Appearance"), NotifyParentProperty(true), DefaultValue(typeof(Unit), ""), WebSysDescription("Style_BorderWidth")]
        public Unit BorderWidth
        {
            get
            {
                if (this.IsSet(0x20))
                {
                    return (Unit) this.ViewState["BorderWidth"];
                }
                return Unit.Empty;
            }
            set
            {
                if ((value.Type == UnitType.Percentage) || (value.Value < 0.0))
                {
                    throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("Style_InvalidBorderWidth"));
                }
                this.ViewState["BorderWidth"] = value;
                this.SetBit(0x20);
            }
        }

        [WebSysDescription("Style_CSSClass"), WebCategory("Appearance"), DefaultValue(""), NotifyParentProperty(true), CssClassProperty]
        public string CssClass
        {
            get
            {
                if (this.IsSet(2))
                {
                    string str = (string) this.ViewState["CssClass"];
                    if (str != null)
                    {
                        return str;
                    }
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["CssClass"] = value;
                this.SetBit(2);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebSysDescription("Style_Font"), WebCategory("Appearance"), NotifyParentProperty(true)]
        public FontInfo Font
        {
            get
            {
                if (this.fontInfo == null)
                {
                    this.fontInfo = new FontInfo(this);
                }
                return this.fontInfo;
            }
        }

        [WebCategory("Appearance"), DefaultValue(typeof(Color), ""), WebSysDescription("Style_ForeColor"), NotifyParentProperty(true), TypeConverter(typeof(WebColorConverter))]
        public Color ForeColor
        {
            get
            {
                if (this.IsSet(4))
                {
                    return (Color) this.ViewState["ForeColor"];
                }
                return Color.Empty;
            }
            set
            {
                this.ViewState["ForeColor"] = value;
                this.SetBit(4);
            }
        }

        [DefaultValue(typeof(Unit), ""), WebCategory("Layout"), WebSysDescription("Style_Height"), NotifyParentProperty(true)]
        public Unit Height
        {
            get
            {
                if (this.IsSet(0x80))
                {
                    return (Unit) this.ViewState["Height"];
                }
                return Unit.Empty;
            }
            set
            {
                if (value.Value < 0.0)
                {
                    throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("Style_InvalidHeight"));
                }
                this.ViewState["Height"] = value;
                this.SetBit(0x80);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool IsEmpty
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return ((this.setBits == 0) && (this.RegisteredCssClass.Length == 0));
            }
        }

        protected bool IsTrackingViewState
        {
            get
            {
                return this.marked;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RegisteredCssClass
        {
            get
            {
                if (this.registeredCssClass == null)
                {
                    return string.Empty;
                }
                return this.registeredCssClass;
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this.IsTrackingViewState;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected internal StateBag ViewState
        {
            get
            {
                if (this.statebag == null)
                {
                    this.statebag = new StateBag(false);
                    if (this.IsTrackingViewState)
                    {
                        this.statebag.TrackViewState();
                    }
                }
                return this.statebag;
            }
        }

        [WebCategory("Layout"), NotifyParentProperty(true), DefaultValue(typeof(Unit), ""), WebSysDescription("Style_Width")]
        public Unit Width
        {
            get
            {
                if (this.IsSet(0x100))
                {
                    return (Unit) this.ViewState["Width"];
                }
                return Unit.Empty;
            }
            set
            {
                if (value.Value < 0.0)
                {
                    throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("Style_InvalidWidth"));
                }
                this.ViewState["Width"] = value;
                this.SetBit(0x100);
            }
        }
    }
}

