namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed class FontInfo
    {
        private Style owner;

        internal FontInfo(Style owner)
        {
            this.owner = owner;
        }

        public void ClearDefaults()
        {
            if (this.Names.Length == 0)
            {
                this.owner.ViewState.Remove("Font_Names");
                this.owner.ClearBit(0x200);
            }
            if (this.Size == FontUnit.Empty)
            {
                this.owner.ViewState.Remove("Font_Size");
                this.owner.ClearBit(0x400);
            }
            if (!this.Bold)
            {
                this.ResetBold();
            }
            if (!this.Italic)
            {
                this.ResetItalic();
            }
            if (!this.Underline)
            {
                this.ResetUnderline();
            }
            if (!this.Overline)
            {
                this.ResetOverline();
            }
            if (!this.Strikeout)
            {
                this.ResetStrikeout();
            }
        }

        public void CopyFrom(FontInfo f)
        {
            if (f != null)
            {
                Style owner = f.Owner;
                if (owner.RegisteredCssClass.Length != 0)
                {
                    if (owner.IsSet(0x200))
                    {
                        this.ResetNames();
                    }
                    if (owner.IsSet(0x400) && (f.Size != FontUnit.Empty))
                    {
                        this.ResetFontSize();
                    }
                    if (owner.IsSet(0x800))
                    {
                        this.ResetBold();
                    }
                    if (owner.IsSet(0x1000))
                    {
                        this.ResetItalic();
                    }
                    if (owner.IsSet(0x4000))
                    {
                        this.ResetOverline();
                    }
                    if (owner.IsSet(0x8000))
                    {
                        this.ResetStrikeout();
                    }
                    if (owner.IsSet(0x2000))
                    {
                        this.ResetUnderline();
                    }
                }
                else
                {
                    if (owner.IsSet(0x200))
                    {
                        this.Names = f.Names;
                    }
                    if (owner.IsSet(0x400) && (f.Size != FontUnit.Empty))
                    {
                        this.Size = f.Size;
                    }
                    if (owner.IsSet(0x800))
                    {
                        this.Bold = f.Bold;
                    }
                    if (owner.IsSet(0x1000))
                    {
                        this.Italic = f.Italic;
                    }
                    if (owner.IsSet(0x4000))
                    {
                        this.Overline = f.Overline;
                    }
                    if (owner.IsSet(0x8000))
                    {
                        this.Strikeout = f.Strikeout;
                    }
                    if (owner.IsSet(0x2000))
                    {
                        this.Underline = f.Underline;
                    }
                }
            }
        }

        public void MergeWith(FontInfo f)
        {
            if (f != null)
            {
                Style owner = f.Owner;
                if (owner.RegisteredCssClass.Length == 0)
                {
                    if (owner.IsSet(0x200) && !this.owner.IsSet(0x200))
                    {
                        this.Names = f.Names;
                    }
                    if (owner.IsSet(0x400) && (!this.owner.IsSet(0x400) || (this.Size == FontUnit.Empty)))
                    {
                        this.Size = f.Size;
                    }
                    if (owner.IsSet(0x800) && !this.owner.IsSet(0x800))
                    {
                        this.Bold = f.Bold;
                    }
                    if (owner.IsSet(0x1000) && !this.owner.IsSet(0x1000))
                    {
                        this.Italic = f.Italic;
                    }
                    if (owner.IsSet(0x4000) && !this.owner.IsSet(0x4000))
                    {
                        this.Overline = f.Overline;
                    }
                    if (owner.IsSet(0x8000) && !this.owner.IsSet(0x8000))
                    {
                        this.Strikeout = f.Strikeout;
                    }
                    if (owner.IsSet(0x2000) && !this.owner.IsSet(0x2000))
                    {
                        this.Underline = f.Underline;
                    }
                }
            }
        }

        internal void Reset()
        {
            if (this.owner.IsSet(0x200))
            {
                this.ResetNames();
            }
            if (this.owner.IsSet(0x400))
            {
                this.ResetFontSize();
            }
            if (this.owner.IsSet(0x800))
            {
                this.ResetBold();
            }
            if (this.owner.IsSet(0x1000))
            {
                this.ResetItalic();
            }
            if (this.owner.IsSet(0x2000))
            {
                this.ResetUnderline();
            }
            if (this.owner.IsSet(0x4000))
            {
                this.ResetOverline();
            }
            if (this.owner.IsSet(0x8000))
            {
                this.ResetStrikeout();
            }
        }

        private void ResetBold()
        {
            this.owner.ViewState.Remove("Font_Bold");
            this.owner.ClearBit(0x800);
        }

        private void ResetFontSize()
        {
            this.owner.ViewState.Remove("Font_Size");
            this.owner.ClearBit(0x400);
        }

        private void ResetItalic()
        {
            this.owner.ViewState.Remove("Font_Italic");
            this.owner.ClearBit(0x1000);
        }

        private void ResetNames()
        {
            this.owner.ViewState.Remove("Font_Names");
            this.owner.ClearBit(0x200);
        }

        private void ResetOverline()
        {
            this.owner.ViewState.Remove("Font_Overline");
            this.owner.ClearBit(0x4000);
        }

        private void ResetStrikeout()
        {
            this.owner.ViewState.Remove("Font_Strikeout");
            this.owner.ClearBit(0x8000);
        }

        private void ResetUnderline()
        {
            this.owner.ViewState.Remove("Font_Underline");
            this.owner.ClearBit(0x2000);
        }

        private bool ShouldSerializeBold()
        {
            return this.owner.IsSet(0x800);
        }

        private bool ShouldSerializeItalic()
        {
            return this.owner.IsSet(0x1000);
        }

        public bool ShouldSerializeNames()
        {
            return (this.Names.Length > 0);
        }

        private bool ShouldSerializeOverline()
        {
            return this.owner.IsSet(0x4000);
        }

        private bool ShouldSerializeStrikeout()
        {
            return this.owner.IsSet(0x8000);
        }

        private bool ShouldSerializeUnderline()
        {
            return this.owner.IsSet(0x2000);
        }

        public override string ToString()
        {
            string str = this.Size.ToString(CultureInfo.InvariantCulture);
            string name = this.Name;
            if (str.Length == 0)
            {
                return name;
            }
            if (name.Length != 0)
            {
                return (name + ", " + str);
            }
            return str;
        }

        [NotifyParentProperty(true), WebCategory("Appearance"), DefaultValue(false), WebSysDescription("FontInfo_Bold")]
        public bool Bold
        {
            get
            {
                return (this.owner.IsSet(0x800) && ((bool) this.owner.ViewState["Font_Bold"]));
            }
            set
            {
                this.owner.ViewState["Font_Bold"] = value;
                this.owner.SetBit(0x800);
            }
        }

        [WebCategory("Appearance"), NotifyParentProperty(true), DefaultValue(false), WebSysDescription("FontInfo_Italic")]
        public bool Italic
        {
            get
            {
                return (this.owner.IsSet(0x1000) && ((bool) this.owner.ViewState["Font_Italic"]));
            }
            set
            {
                this.owner.ViewState["Font_Italic"] = value;
                this.owner.SetBit(0x1000);
            }
        }

        [WebSysDescription("FontInfo_Name"), Editor("System.Drawing.Design.FontNameEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), TypeConverter(typeof(FontConverter.FontNameConverter)), WebCategory("Appearance"), DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), NotifyParentProperty(true), RefreshProperties(RefreshProperties.Repaint)]
        public string Name
        {
            get
            {
                string[] names = this.Names;
                if (names.Length > 0)
                {
                    return names[0];
                }
                return string.Empty;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Length == 0)
                {
                    this.Names = null;
                }
                else
                {
                    this.Names = new string[] { value };
                }
            }
        }

        [NotifyParentProperty(true), TypeConverter(typeof(FontNamesConverter)), WebCategory("Appearance"), Editor("System.Windows.Forms.Design.StringArrayEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebSysDescription("FontInfo_Names"), RefreshProperties(RefreshProperties.Repaint)]
        public string[] Names
        {
            get
            {
                if (this.owner.IsSet(0x200))
                {
                    string[] strArray = (string[]) this.owner.ViewState["Font_Names"];
                    if (strArray != null)
                    {
                        return strArray;
                    }
                }
                return new string[0];
            }
            set
            {
                this.owner.ViewState["Font_Names"] = value;
                this.owner.SetBit(0x200);
            }
        }

        [WebSysDescription("FontInfo_Overline"), NotifyParentProperty(true), WebCategory("Appearance"), DefaultValue(false)]
        public bool Overline
        {
            get
            {
                return (this.owner.IsSet(0x4000) && ((bool) this.owner.ViewState["Font_Overline"]));
            }
            set
            {
                this.owner.ViewState["Font_Overline"] = value;
                this.owner.SetBit(0x4000);
            }
        }

        internal Style Owner
        {
            get
            {
                return this.owner;
            }
        }

        [DefaultValue(typeof(FontUnit), ""), WebCategory("Appearance"), RefreshProperties(RefreshProperties.Repaint), WebSysDescription("FontInfo_Size"), NotifyParentProperty(true)]
        public FontUnit Size
        {
            get
            {
                if (this.owner.IsSet(0x400))
                {
                    return (FontUnit) this.owner.ViewState["Font_Size"];
                }
                return FontUnit.Empty;
            }
            set
            {
                if ((value.Type == FontSize.AsUnit) && (value.Unit.Value < 0.0))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.owner.ViewState["Font_Size"] = value;
                this.owner.SetBit(0x400);
            }
        }

        [NotifyParentProperty(true), WebCategory("Appearance"), DefaultValue(false), WebSysDescription("FontInfo_Strikeout")]
        public bool Strikeout
        {
            get
            {
                return (this.owner.IsSet(0x8000) && ((bool) this.owner.ViewState["Font_Strikeout"]));
            }
            set
            {
                this.owner.ViewState["Font_Strikeout"] = value;
                this.owner.SetBit(0x8000);
            }
        }

        [DefaultValue(false), WebCategory("Appearance"), WebSysDescription("FontInfo_Underline"), NotifyParentProperty(true)]
        public bool Underline
        {
            get
            {
                return (this.owner.IsSet(0x2000) && ((bool) this.owner.ViewState["Font_Underline"]));
            }
            set
            {
                this.owner.ViewState["Font_Underline"] = value;
                this.owner.SetBit(0x2000);
            }
        }
    }
}

