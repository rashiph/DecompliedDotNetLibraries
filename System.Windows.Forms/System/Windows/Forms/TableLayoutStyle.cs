namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows.Forms.Layout;

    [TypeConverter(typeof(TableLayoutSettings.StyleConverter))]
    public abstract class TableLayoutStyle
    {
        private IArrangedElement _owner;
        private float _size;
        private System.Windows.Forms.SizeType _sizeType;

        protected TableLayoutStyle()
        {
        }

        internal void SetSize(float size)
        {
            this._size = size;
        }

        private bool ShouldSerializeSize()
        {
            return (this.SizeType != System.Windows.Forms.SizeType.AutoSize);
        }

        internal IArrangedElement Owner
        {
            get
            {
                return this._owner;
            }
            set
            {
                this._owner = value;
            }
        }

        internal float Size
        {
            get
            {
                return this._size;
            }
            set
            {
                if (value < 0f)
                {
                    object[] args = new object[] { "Size", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("Size", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                if (this._size != value)
                {
                    this._size = value;
                    if (this.Owner != null)
                    {
                        LayoutTransaction.DoLayout(this.Owner, this.Owner, PropertyNames.Style);
                        Control owner = this.Owner as Control;
                        if (owner != null)
                        {
                            owner.Invalidate();
                        }
                    }
                }
            }
        }

        [DefaultValue(0)]
        public System.Windows.Forms.SizeType SizeType
        {
            get
            {
                return this._sizeType;
            }
            set
            {
                if (this._sizeType != value)
                {
                    this._sizeType = value;
                    if (this.Owner != null)
                    {
                        LayoutTransaction.DoLayout(this.Owner, this.Owner, PropertyNames.Style);
                        Control owner = this.Owner as Control;
                        if (owner != null)
                        {
                            owner.Invalidate();
                        }
                    }
                }
            }
        }
    }
}

