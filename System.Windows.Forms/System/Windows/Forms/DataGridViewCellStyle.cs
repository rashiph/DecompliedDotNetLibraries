namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Text;

    [Editor("System.Windows.Forms.Design.DataGridViewCellStyleEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), TypeConverter(typeof(DataGridViewCellStyleConverter))]
    public class DataGridViewCellStyle : ICloneable
    {
        private DataGridView dataGridView;
        private const string DATAGRIDVIEWCELLSTYLE_nullText = "";
        private static readonly int PropAlignment = PropertyStore.CreateKey();
        private static readonly int PropBackColor = PropertyStore.CreateKey();
        private static readonly int PropDataSourceNullValue = PropertyStore.CreateKey();
        private PropertyStore propertyStore;
        private static readonly int PropFont = PropertyStore.CreateKey();
        private static readonly int PropForeColor = PropertyStore.CreateKey();
        private static readonly int PropFormat = PropertyStore.CreateKey();
        private static readonly int PropFormatProvider = PropertyStore.CreateKey();
        private static readonly int PropNullValue = PropertyStore.CreateKey();
        private static readonly int PropPadding = PropertyStore.CreateKey();
        private static readonly int PropSelectionBackColor = PropertyStore.CreateKey();
        private static readonly int PropSelectionForeColor = PropertyStore.CreateKey();
        private static readonly int PropTag = PropertyStore.CreateKey();
        private static readonly int PropWrapMode = PropertyStore.CreateKey();
        private DataGridViewCellStyleScopes scope;

        public DataGridViewCellStyle()
        {
            this.propertyStore = new PropertyStore();
            this.scope = DataGridViewCellStyleScopes.None;
        }

        public DataGridViewCellStyle(DataGridViewCellStyle dataGridViewCellStyle)
        {
            if (dataGridViewCellStyle == null)
            {
                throw new ArgumentNullException("dataGridViewCellStyle");
            }
            this.propertyStore = new PropertyStore();
            this.scope = DataGridViewCellStyleScopes.None;
            this.BackColor = dataGridViewCellStyle.BackColor;
            this.ForeColor = dataGridViewCellStyle.ForeColor;
            this.SelectionBackColor = dataGridViewCellStyle.SelectionBackColor;
            this.SelectionForeColor = dataGridViewCellStyle.SelectionForeColor;
            this.Font = dataGridViewCellStyle.Font;
            this.NullValue = dataGridViewCellStyle.NullValue;
            this.DataSourceNullValue = dataGridViewCellStyle.DataSourceNullValue;
            this.Format = dataGridViewCellStyle.Format;
            if (!dataGridViewCellStyle.IsFormatProviderDefault)
            {
                this.FormatProvider = dataGridViewCellStyle.FormatProvider;
            }
            this.AlignmentInternal = dataGridViewCellStyle.Alignment;
            this.WrapModeInternal = dataGridViewCellStyle.WrapMode;
            this.Tag = dataGridViewCellStyle.Tag;
            this.PaddingInternal = dataGridViewCellStyle.Padding;
        }

        internal void AddScope(DataGridView dataGridView, DataGridViewCellStyleScopes scope)
        {
            this.scope |= scope;
            this.dataGridView = dataGridView;
        }

        public virtual void ApplyStyle(DataGridViewCellStyle dataGridViewCellStyle)
        {
            if (dataGridViewCellStyle == null)
            {
                throw new ArgumentNullException("dataGridViewCellStyle");
            }
            if (!dataGridViewCellStyle.BackColor.IsEmpty)
            {
                this.BackColor = dataGridViewCellStyle.BackColor;
            }
            if (!dataGridViewCellStyle.ForeColor.IsEmpty)
            {
                this.ForeColor = dataGridViewCellStyle.ForeColor;
            }
            if (!dataGridViewCellStyle.SelectionBackColor.IsEmpty)
            {
                this.SelectionBackColor = dataGridViewCellStyle.SelectionBackColor;
            }
            if (!dataGridViewCellStyle.SelectionForeColor.IsEmpty)
            {
                this.SelectionForeColor = dataGridViewCellStyle.SelectionForeColor;
            }
            if (dataGridViewCellStyle.Font != null)
            {
                this.Font = dataGridViewCellStyle.Font;
            }
            if (!dataGridViewCellStyle.IsNullValueDefault)
            {
                this.NullValue = dataGridViewCellStyle.NullValue;
            }
            if (!dataGridViewCellStyle.IsDataSourceNullValueDefault)
            {
                this.DataSourceNullValue = dataGridViewCellStyle.DataSourceNullValue;
            }
            if (dataGridViewCellStyle.Format.Length != 0)
            {
                this.Format = dataGridViewCellStyle.Format;
            }
            if (!dataGridViewCellStyle.IsFormatProviderDefault)
            {
                this.FormatProvider = dataGridViewCellStyle.FormatProvider;
            }
            if (dataGridViewCellStyle.Alignment != DataGridViewContentAlignment.NotSet)
            {
                this.AlignmentInternal = dataGridViewCellStyle.Alignment;
            }
            if (dataGridViewCellStyle.WrapMode != DataGridViewTriState.NotSet)
            {
                this.WrapModeInternal = dataGridViewCellStyle.WrapMode;
            }
            if (dataGridViewCellStyle.Tag != null)
            {
                this.Tag = dataGridViewCellStyle.Tag;
            }
            if (dataGridViewCellStyle.Padding != System.Windows.Forms.Padding.Empty)
            {
                this.PaddingInternal = dataGridViewCellStyle.Padding;
            }
        }

        public virtual DataGridViewCellStyle Clone()
        {
            return new DataGridViewCellStyle(this);
        }

        public override bool Equals(object o)
        {
            DataGridViewCellStyle dgvcs = o as DataGridViewCellStyle;
            return ((dgvcs != null) && (this.GetDifferencesFrom(dgvcs) == DataGridViewCellStyleDifferences.None));
        }

        internal DataGridViewCellStyleDifferences GetDifferencesFrom(DataGridViewCellStyle dgvcs)
        {
            bool flag = ((((dgvcs.Alignment != this.Alignment) || (dgvcs.DataSourceNullValue != this.DataSourceNullValue)) || ((dgvcs.Font != this.Font) || (dgvcs.Format != this.Format))) || (((dgvcs.FormatProvider != this.FormatProvider) || (dgvcs.NullValue != this.NullValue)) || ((dgvcs.Padding != this.Padding) || (dgvcs.Tag != this.Tag)))) || (dgvcs.WrapMode != this.WrapMode);
            bool flag2 = (((dgvcs.BackColor != this.BackColor) || (dgvcs.ForeColor != this.ForeColor)) || (dgvcs.SelectionBackColor != this.SelectionBackColor)) || (dgvcs.SelectionForeColor != this.SelectionForeColor);
            if (flag)
            {
                return DataGridViewCellStyleDifferences.AffectPreferredSize;
            }
            if (flag2)
            {
                return DataGridViewCellStyleDifferences.DoNotAffectPreferredSize;
            }
            return DataGridViewCellStyleDifferences.None;
        }

        public override int GetHashCode()
        {
            return WindowsFormsUtils.GetCombinedHashCodes(new int[] { this.Alignment, this.WrapMode, this.Padding.GetHashCode(), this.Format.GetHashCode(), this.BackColor.GetHashCode(), this.ForeColor.GetHashCode(), this.SelectionBackColor.GetHashCode(), this.SelectionForeColor.GetHashCode(), (this.Font == null) ? 1 : this.Font.GetHashCode(), (this.NullValue == null) ? 1 : this.NullValue.GetHashCode(), (this.DataSourceNullValue == null) ? 1 : this.DataSourceNullValue.GetHashCode(), (this.Tag == null) ? 1 : this.Tag.GetHashCode() });
        }

        private void OnPropertyChanged(DataGridViewCellStylePropertyInternal property)
        {
            if ((this.dataGridView != null) && (this.scope != DataGridViewCellStyleScopes.None))
            {
                this.dataGridView.OnCellStyleContentChanged(this, property);
            }
        }

        internal void RemoveScope(DataGridViewCellStyleScopes scope)
        {
            this.scope &= ~scope;
            if (this.scope == DataGridViewCellStyleScopes.None)
            {
                this.dataGridView = null;
            }
        }

        private bool ShouldSerializeBackColor()
        {
            bool flag;
            this.Properties.GetColor(PropBackColor, out flag);
            return flag;
        }

        private bool ShouldSerializeFont()
        {
            return (this.Properties.GetObject(PropFont) != null);
        }

        private bool ShouldSerializeForeColor()
        {
            bool flag;
            this.Properties.GetColor(PropForeColor, out flag);
            return flag;
        }

        private bool ShouldSerializeFormatProvider()
        {
            return (this.Properties.GetObject(PropFormatProvider) != null);
        }

        private bool ShouldSerializePadding()
        {
            return (this.Padding != System.Windows.Forms.Padding.Empty);
        }

        private bool ShouldSerializeSelectionBackColor()
        {
            bool flag;
            this.Properties.GetObject(PropSelectionBackColor, out flag);
            return flag;
        }

        private bool ShouldSerializeSelectionForeColor()
        {
            bool flag;
            this.Properties.GetColor(PropSelectionForeColor, out flag);
            return flag;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x80);
            builder.Append("DataGridViewCellStyle {");
            bool flag = true;
            if (this.BackColor != Color.Empty)
            {
                builder.Append(" BackColor=" + this.BackColor.ToString());
                flag = false;
            }
            if (this.ForeColor != Color.Empty)
            {
                if (!flag)
                {
                    builder.Append(",");
                }
                builder.Append(" ForeColor=" + this.ForeColor.ToString());
                flag = false;
            }
            if (this.SelectionBackColor != Color.Empty)
            {
                if (!flag)
                {
                    builder.Append(",");
                }
                builder.Append(" SelectionBackColor=" + this.SelectionBackColor.ToString());
                flag = false;
            }
            if (this.SelectionForeColor != Color.Empty)
            {
                if (!flag)
                {
                    builder.Append(",");
                }
                builder.Append(" SelectionForeColor=" + this.SelectionForeColor.ToString());
                flag = false;
            }
            if (this.Font != null)
            {
                if (!flag)
                {
                    builder.Append(",");
                }
                builder.Append(" Font=" + this.Font.ToString());
                flag = false;
            }
            if (!this.IsNullValueDefault && (this.NullValue != null))
            {
                if (!flag)
                {
                    builder.Append(",");
                }
                builder.Append(" NullValue=" + this.NullValue.ToString());
                flag = false;
            }
            if (!this.IsDataSourceNullValueDefault && (this.DataSourceNullValue != null))
            {
                if (!flag)
                {
                    builder.Append(",");
                }
                builder.Append(" DataSourceNullValue=" + this.DataSourceNullValue.ToString());
                flag = false;
            }
            if (!string.IsNullOrEmpty(this.Format))
            {
                if (!flag)
                {
                    builder.Append(",");
                }
                builder.Append(" Format=" + this.Format);
                flag = false;
            }
            if (this.WrapMode != DataGridViewTriState.NotSet)
            {
                if (!flag)
                {
                    builder.Append(",");
                }
                builder.Append(" WrapMode=" + this.WrapMode.ToString());
                flag = false;
            }
            if (this.Alignment != DataGridViewContentAlignment.NotSet)
            {
                if (!flag)
                {
                    builder.Append(",");
                }
                builder.Append(" Alignment=" + this.Alignment.ToString());
                flag = false;
            }
            if (this.Padding != System.Windows.Forms.Padding.Empty)
            {
                if (!flag)
                {
                    builder.Append(",");
                }
                builder.Append(" Padding=" + this.Padding.ToString());
                flag = false;
            }
            if (this.Tag != null)
            {
                if (!flag)
                {
                    builder.Append(",");
                }
                builder.Append(" Tag=" + this.Tag.ToString());
                flag = false;
            }
            builder.Append(" }");
            return builder.ToString();
        }

        [System.Windows.Forms.SRDescription("DataGridViewCellStyleAlignmentDescr"), DefaultValue(0), System.Windows.Forms.SRCategory("CatLayout")]
        public DataGridViewContentAlignment Alignment
        {
            get
            {
                bool flag;
                int integer = this.Properties.GetInteger(PropAlignment, out flag);
                if (flag)
                {
                    return (DataGridViewContentAlignment) integer;
                }
                return DataGridViewContentAlignment.NotSet;
            }
            set
            {
                switch (value)
                {
                    case DataGridViewContentAlignment.NotSet:
                    case DataGridViewContentAlignment.TopLeft:
                    case DataGridViewContentAlignment.TopCenter:
                    case DataGridViewContentAlignment.TopRight:
                    case DataGridViewContentAlignment.MiddleLeft:
                    case DataGridViewContentAlignment.MiddleCenter:
                    case DataGridViewContentAlignment.MiddleRight:
                    case DataGridViewContentAlignment.BottomLeft:
                    case DataGridViewContentAlignment.BottomCenter:
                    case DataGridViewContentAlignment.BottomRight:
                        this.AlignmentInternal = value;
                        return;
                }
                throw new InvalidEnumArgumentException("value", (int) value, typeof(DataGridViewContentAlignment));
            }
        }

        internal DataGridViewContentAlignment AlignmentInternal
        {
            set
            {
                if (this.Alignment != value)
                {
                    this.Properties.SetInteger(PropAlignment, (int) value);
                    this.OnPropertyChanged(DataGridViewCellStylePropertyInternal.Other);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance")]
        public Color BackColor
        {
            get
            {
                return this.Properties.GetColor(PropBackColor);
            }
            set
            {
                Color backColor = this.BackColor;
                if (!value.IsEmpty || this.Properties.ContainsObject(PropBackColor))
                {
                    this.Properties.SetColor(PropBackColor, value);
                }
                if (!backColor.Equals(this.BackColor))
                {
                    this.OnPropertyChanged(DataGridViewCellStylePropertyInternal.Color);
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object DataSourceNullValue
        {
            get
            {
                if (this.Properties.ContainsObject(PropDataSourceNullValue))
                {
                    return this.Properties.GetObject(PropDataSourceNullValue);
                }
                return DBNull.Value;
            }
            set
            {
                object dataSourceNullValue = this.DataSourceNullValue;
                if ((dataSourceNullValue != value) && ((dataSourceNullValue == null) || !dataSourceNullValue.Equals(value)))
                {
                    if ((value == DBNull.Value) && this.Properties.ContainsObject(PropDataSourceNullValue))
                    {
                        this.Properties.RemoveObject(PropDataSourceNullValue);
                    }
                    else
                    {
                        this.Properties.SetObject(PropDataSourceNullValue, value);
                    }
                    this.OnPropertyChanged(DataGridViewCellStylePropertyInternal.Other);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance")]
        public System.Drawing.Font Font
        {
            get
            {
                return (System.Drawing.Font) this.Properties.GetObject(PropFont);
            }
            set
            {
                System.Drawing.Font font = this.Font;
                if ((value != null) || this.Properties.ContainsObject(PropFont))
                {
                    this.Properties.SetObject(PropFont, value);
                }
                if ((((font == null) && (value != null)) || ((font != null) && (value == null))) || (((font != null) && (value != null)) && !font.Equals(this.Font)))
                {
                    this.OnPropertyChanged(DataGridViewCellStylePropertyInternal.Font);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance")]
        public Color ForeColor
        {
            get
            {
                return this.Properties.GetColor(PropForeColor);
            }
            set
            {
                Color foreColor = this.ForeColor;
                if (!value.IsEmpty || this.Properties.ContainsObject(PropForeColor))
                {
                    this.Properties.SetColor(PropForeColor, value);
                }
                if (!foreColor.Equals(this.ForeColor))
                {
                    this.OnPropertyChanged(DataGridViewCellStylePropertyInternal.ForeColor);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), Editor("System.Windows.Forms.Design.FormatStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), EditorBrowsable(EditorBrowsableState.Advanced)]
        public string Format
        {
            get
            {
                object obj2 = this.Properties.GetObject(PropFormat);
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                string format = this.Format;
                if (((value != null) && (value.Length > 0)) || this.Properties.ContainsObject(PropFormat))
                {
                    this.Properties.SetObject(PropFormat, value);
                }
                if (!format.Equals(this.Format))
                {
                    this.OnPropertyChanged(DataGridViewCellStylePropertyInternal.Other);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public IFormatProvider FormatProvider
        {
            get
            {
                object obj2 = this.Properties.GetObject(PropFormatProvider);
                if (obj2 == null)
                {
                    return CultureInfo.CurrentCulture;
                }
                return (IFormatProvider) obj2;
            }
            set
            {
                object obj2 = this.Properties.GetObject(PropFormatProvider);
                this.Properties.SetObject(PropFormatProvider, value);
                if (value != obj2)
                {
                    this.OnPropertyChanged(DataGridViewCellStylePropertyInternal.Other);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public bool IsDataSourceNullValueDefault
        {
            get
            {
                return (!this.Properties.ContainsObject(PropDataSourceNullValue) || (this.Properties.GetObject(PropDataSourceNullValue) == DBNull.Value));
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool IsFormatProviderDefault
        {
            get
            {
                return (this.Properties.GetObject(PropFormatProvider) == null);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool IsNullValueDefault
        {
            get
            {
                if (!this.Properties.ContainsObject(PropNullValue))
                {
                    return true;
                }
                object obj2 = this.Properties.GetObject(PropNullValue);
                return ((obj2 is string) && obj2.Equals(""));
            }
        }

        [TypeConverter(typeof(StringConverter)), DefaultValue(""), System.Windows.Forms.SRCategory("CatData")]
        public object NullValue
        {
            get
            {
                if (this.Properties.ContainsObject(PropNullValue))
                {
                    return this.Properties.GetObject(PropNullValue);
                }
                return "";
            }
            set
            {
                object nullValue = this.NullValue;
                if ((nullValue != value) && ((nullValue == null) || !nullValue.Equals(value)))
                {
                    if (((value is string) && value.Equals("")) && this.Properties.ContainsObject(PropNullValue))
                    {
                        this.Properties.RemoveObject(PropNullValue);
                    }
                    else
                    {
                        this.Properties.SetObject(PropNullValue, value);
                    }
                    this.OnPropertyChanged(DataGridViewCellStylePropertyInternal.Other);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout")]
        public System.Windows.Forms.Padding Padding
        {
            get
            {
                return this.Properties.GetPadding(PropPadding);
            }
            set
            {
                if (((value.Left < 0) || (value.Right < 0)) || ((value.Top < 0) || (value.Bottom < 0)))
                {
                    if (value.All != -1)
                    {
                        value.All = 0;
                    }
                    else
                    {
                        value.Left = Math.Max(0, value.Left);
                        value.Right = Math.Max(0, value.Right);
                        value.Top = Math.Max(0, value.Top);
                        value.Bottom = Math.Max(0, value.Bottom);
                    }
                }
                this.PaddingInternal = value;
            }
        }

        internal System.Windows.Forms.Padding PaddingInternal
        {
            set
            {
                if (value != this.Padding)
                {
                    this.Properties.SetPadding(PropPadding, value);
                    this.OnPropertyChanged(DataGridViewCellStylePropertyInternal.Other);
                }
            }
        }

        internal PropertyStore Properties
        {
            get
            {
                return this.propertyStore;
            }
        }

        internal DataGridViewCellStyleScopes Scope
        {
            get
            {
                return this.scope;
            }
            set
            {
                this.scope = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance")]
        public Color SelectionBackColor
        {
            get
            {
                return this.Properties.GetColor(PropSelectionBackColor);
            }
            set
            {
                Color selectionBackColor = this.SelectionBackColor;
                if (!value.IsEmpty || this.Properties.ContainsObject(PropSelectionBackColor))
                {
                    this.Properties.SetColor(PropSelectionBackColor, value);
                }
                if (!selectionBackColor.Equals(this.SelectionBackColor))
                {
                    this.OnPropertyChanged(DataGridViewCellStylePropertyInternal.Color);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance")]
        public Color SelectionForeColor
        {
            get
            {
                return this.Properties.GetColor(PropSelectionForeColor);
            }
            set
            {
                Color selectionForeColor = this.SelectionForeColor;
                if (!value.IsEmpty || this.Properties.ContainsObject(PropSelectionForeColor))
                {
                    this.Properties.SetColor(PropSelectionForeColor, value);
                }
                if (!selectionForeColor.Equals(this.SelectionForeColor))
                {
                    this.OnPropertyChanged(DataGridViewCellStylePropertyInternal.Color);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public object Tag
        {
            get
            {
                return this.Properties.GetObject(PropTag);
            }
            set
            {
                if ((value != null) || this.Properties.ContainsObject(PropTag))
                {
                    this.Properties.SetObject(PropTag, value);
                }
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRCategory("CatLayout")]
        public DataGridViewTriState WrapMode
        {
            get
            {
                bool flag;
                int integer = this.Properties.GetInteger(PropWrapMode, out flag);
                if (flag)
                {
                    return (DataGridViewTriState) integer;
                }
                return DataGridViewTriState.NotSet;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DataGridViewTriState));
                }
                this.WrapModeInternal = value;
            }
        }

        internal DataGridViewTriState WrapModeInternal
        {
            set
            {
                if (this.WrapMode != value)
                {
                    this.Properties.SetInteger(PropWrapMode, (int) value);
                    this.OnPropertyChanged(DataGridViewCellStylePropertyInternal.Other);
                }
            }
        }

        internal enum DataGridViewCellStylePropertyInternal
        {
            Color,
            Other,
            Font,
            ForeColor
        }
    }
}

