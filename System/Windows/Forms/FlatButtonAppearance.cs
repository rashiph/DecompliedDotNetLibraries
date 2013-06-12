namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms.Layout;

    [TypeConverter(typeof(FlatButtonAppearanceConverter))]
    public class FlatButtonAppearance
    {
        private Color borderColor = Color.Empty;
        private int borderSize = 1;
        private Color checkedBackColor = Color.Empty;
        private Color mouseDownBackColor = Color.Empty;
        private Color mouseOverBackColor = Color.Empty;
        private ButtonBase owner;

        internal FlatButtonAppearance(ButtonBase owner)
        {
            this.owner = owner;
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), NotifyParentProperty(true), DefaultValue(typeof(Color), ""), ApplicableToButton, Browsable(true), System.Windows.Forms.SRDescription("ButtonBorderColorDescr"), EditorBrowsable(EditorBrowsableState.Always)]
        public Color BorderColor
        {
            get
            {
                return this.borderColor;
            }
            set
            {
                if (value.Equals(Color.Transparent))
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ButtonFlatAppearanceInvalidBorderColor"));
                }
                if (this.borderColor != value)
                {
                    this.borderColor = value;
                    this.owner.Invalidate();
                }
            }
        }

        [DefaultValue(1), EditorBrowsable(EditorBrowsableState.Always), Browsable(true), ApplicableToButton, NotifyParentProperty(true), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ButtonBorderSizeDescr")]
        public int BorderSize
        {
            get
            {
                return this.borderSize;
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "BorderSize", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("BorderSize", value, System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                if (this.borderSize != value)
                {
                    this.borderSize = value;
                    if ((this.owner != null) && (this.owner.ParentInternal != null))
                    {
                        LayoutTransaction.DoLayoutIf(this.owner.AutoSize, this.owner.ParentInternal, this.owner, PropertyNames.FlatAppearanceBorderSize);
                    }
                    this.owner.Invalidate();
                }
            }
        }

        [DefaultValue(typeof(Color), ""), NotifyParentProperty(true), Browsable(true), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ButtonCheckedBackColorDescr"), EditorBrowsable(EditorBrowsableState.Always)]
        public Color CheckedBackColor
        {
            get
            {
                return this.checkedBackColor;
            }
            set
            {
                if (this.checkedBackColor != value)
                {
                    this.checkedBackColor = value;
                    this.owner.Invalidate();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ButtonMouseDownBackColorDescr"), Browsable(true), NotifyParentProperty(true), ApplicableToButton, EditorBrowsable(EditorBrowsableState.Always), DefaultValue(typeof(Color), "")]
        public Color MouseDownBackColor
        {
            get
            {
                return this.mouseDownBackColor;
            }
            set
            {
                if (this.mouseDownBackColor != value)
                {
                    this.mouseDownBackColor = value;
                    this.owner.Invalidate();
                }
            }
        }

        [Browsable(true), ApplicableToButton, NotifyParentProperty(true), DefaultValue(typeof(Color), ""), System.Windows.Forms.SRDescription("ButtonMouseOverBackColorDescr"), System.Windows.Forms.SRCategory("CatAppearance"), EditorBrowsable(EditorBrowsableState.Always)]
        public Color MouseOverBackColor
        {
            get
            {
                return this.mouseOverBackColor;
            }
            set
            {
                if (this.mouseOverBackColor != value)
                {
                    this.mouseOverBackColor = value;
                    this.owner.Invalidate();
                }
            }
        }
    }
}

