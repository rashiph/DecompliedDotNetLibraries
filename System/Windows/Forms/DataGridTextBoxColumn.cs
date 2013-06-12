namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;

    public class DataGridTextBoxColumn : DataGridColumnStyle
    {
        private DataGridTextBox edit;
        private int editRow;
        private string format;
        private IFormatProvider formatInfo;
        private string oldValue;
        private MethodInfo parseMethod;
        private TypeConverter typeConverter;
        private int xMargin;
        private int yMargin;

        public DataGridTextBoxColumn() : this(null, (string) null)
        {
        }

        public DataGridTextBoxColumn(System.ComponentModel.PropertyDescriptor prop) : this(prop, null, false)
        {
        }

        public DataGridTextBoxColumn(System.ComponentModel.PropertyDescriptor prop, bool isDefault) : this(prop, null, isDefault)
        {
        }

        public DataGridTextBoxColumn(System.ComponentModel.PropertyDescriptor prop, string format) : this(prop, format, false)
        {
        }

        public DataGridTextBoxColumn(System.ComponentModel.PropertyDescriptor prop, string format, bool isDefault) : base(prop, isDefault)
        {
            this.xMargin = 2;
            this.yMargin = 1;
            this.editRow = -1;
            this.edit = new DataGridTextBox();
            this.edit.BorderStyle = BorderStyle.None;
            this.edit.Multiline = true;
            this.edit.AcceptsReturn = true;
            this.edit.Visible = false;
            this.Format = format;
        }

        protected internal override void Abort(int rowNum)
        {
            this.RollBack();
            this.HideEditBox();
            this.EndEdit();
        }

        protected internal override bool Commit(CurrencyManager dataSource, int rowNum)
        {
            this.edit.Bounds = Rectangle.Empty;
            if (!this.edit.IsInEditOrNavigateMode)
            {
                try
                {
                    object text = this.edit.Text;
                    if (this.NullText.Equals(text))
                    {
                        text = Convert.DBNull;
                        this.edit.Text = this.NullText;
                    }
                    else if (((this.format != null) && (this.format.Length != 0)) && ((this.parseMethod != null) && (this.FormatInfo != null)))
                    {
                        text = this.parseMethod.Invoke(null, new object[] { this.edit.Text, this.FormatInfo });
                        if (text is IFormattable)
                        {
                            this.edit.Text = ((IFormattable) text).ToString(this.format, this.formatInfo);
                        }
                        else
                        {
                            this.edit.Text = text.ToString();
                        }
                    }
                    else if ((this.typeConverter != null) && this.typeConverter.CanConvertFrom(typeof(string)))
                    {
                        text = this.typeConverter.ConvertFromString(this.edit.Text);
                        this.edit.Text = this.typeConverter.ConvertToString(text);
                    }
                    this.SetColumnValueAtRow(dataSource, rowNum, text);
                }
                catch
                {
                    this.RollBack();
                    return false;
                }
                this.DebugOut("OnCommit completed without Exception.");
                this.EndEdit();
            }
            return true;
        }

        protected internal override void ConcedeFocus()
        {
            this.edit.Bounds = Rectangle.Empty;
        }

        private void DebugOut(string s)
        {
        }

        protected internal override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string displayText, bool cellIsVisible)
        {
            this.DebugOut("Begining Edit, rowNum :" + rowNum.ToString(CultureInfo.InvariantCulture));
            Rectangle rc = bounds;
            this.edit.ReadOnly = (readOnly || this.ReadOnly) || this.DataGridTableStyle.ReadOnly;
            this.edit.Text = this.GetText(this.GetColumnValueAtRow(source, rowNum));
            if (!this.edit.ReadOnly && (displayText != null))
            {
                this.DataGridTableStyle.DataGrid.ColumnStartedEditing(bounds);
                this.edit.IsInEditOrNavigateMode = false;
                this.edit.Text = displayText;
            }
            if (cellIsVisible)
            {
                bounds.Offset(this.xMargin, 2 * this.yMargin);
                bounds.Width -= this.xMargin;
                bounds.Height -= 2 * this.yMargin;
                this.DebugOut("edit bounds: " + bounds.ToString());
                this.edit.Bounds = bounds;
                this.edit.Visible = true;
                this.edit.TextAlign = this.Alignment;
            }
            else
            {
                this.edit.Bounds = Rectangle.Empty;
            }
            this.edit.RightToLeft = this.DataGridTableStyle.DataGrid.RightToLeft;
            this.edit.FocusInternal();
            this.editRow = rowNum;
            if (!this.edit.ReadOnly)
            {
                this.oldValue = this.edit.Text;
            }
            if (displayText == null)
            {
                this.edit.SelectAll();
            }
            else
            {
                int length = this.edit.Text.Length;
                this.edit.Select(length, 0);
            }
            if (this.edit.Visible)
            {
                this.DataGridTableStyle.DataGrid.Invalidate(rc);
            }
        }

        protected void EndEdit()
        {
            this.edit.IsInEditOrNavigateMode = true;
            this.DebugOut("Ending Edit");
            this.Invalidate();
        }

        protected internal override void EnterNullValue()
        {
            if ((!this.ReadOnly && this.edit.Visible) && this.edit.IsInEditOrNavigateMode)
            {
                this.edit.Text = this.NullText;
                this.edit.IsInEditOrNavigateMode = false;
                if ((this.DataGridTableStyle != null) && (this.DataGridTableStyle.DataGrid != null))
                {
                    this.DataGridTableStyle.DataGrid.ColumnStartedEditing(this.edit.Bounds);
                }
            }
        }

        internal override string GetDisplayText(object value)
        {
            return this.GetText(value);
        }

        protected internal override int GetMinimumHeight()
        {
            return ((base.FontHeight + this.yMargin) + 3);
        }

        protected internal override int GetPreferredHeight(Graphics g, object value)
        {
            int index = 0;
            int num2 = 0;
            string text = this.GetText(value);
            while ((index != -1) && (index < text.Length))
            {
                index = text.IndexOf("\r\n", (int) (index + 1));
                num2++;
            }
            return ((base.FontHeight * num2) + this.yMargin);
        }

        protected internal override Size GetPreferredSize(Graphics g, object value)
        {
            Size size = Size.Ceiling(g.MeasureString(this.GetText(value), this.DataGridTableStyle.DataGrid.Font));
            size.Width += (this.xMargin * 2) + this.DataGridTableStyle.GridLineWidth;
            size.Height += this.yMargin;
            return size;
        }

        private string GetText(object value)
        {
            if (value is DBNull)
            {
                return this.NullText;
            }
            if (((this.format != null) && (this.format.Length != 0)) && (value is IFormattable))
            {
                try
                {
                    return ((IFormattable) value).ToString(this.format, this.formatInfo);
                }
                catch
                {
                    goto Label_0084;
                }
            }
            if ((this.typeConverter != null) && this.typeConverter.CanConvertTo(typeof(string)))
            {
                return (string) this.typeConverter.ConvertTo(value, typeof(string));
            }
        Label_0084:
            if (value == null)
            {
                return "";
            }
            return value.ToString();
        }

        protected void HideEditBox()
        {
            bool focused = this.edit.Focused;
            this.edit.Visible = false;
            if ((focused && (this.DataGridTableStyle != null)) && ((this.DataGridTableStyle.DataGrid != null) && this.DataGridTableStyle.DataGrid.CanFocus))
            {
                this.DataGridTableStyle.DataGrid.FocusInternal();
            }
        }

        internal override bool KeyPress(int rowNum, Keys keyData)
        {
            return (this.edit.IsInEditOrNavigateMode && base.KeyPress(rowNum, keyData));
        }

        protected internal override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum)
        {
            this.Paint(g, bounds, source, rowNum, false);
        }

        protected internal override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, bool alignToRight)
        {
            string text = this.GetText(this.GetColumnValueAtRow(source, rowNum));
            this.PaintText(g, bounds, text, alignToRight);
        }

        protected internal override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
        {
            string text = this.GetText(this.GetColumnValueAtRow(source, rowNum));
            this.PaintText(g, bounds, text, backBrush, foreBrush, alignToRight);
        }

        protected void PaintText(Graphics g, Rectangle bounds, string text, bool alignToRight)
        {
            this.PaintText(g, bounds, text, this.DataGridTableStyle.BackBrush, this.DataGridTableStyle.ForeBrush, alignToRight);
        }

        protected void PaintText(Graphics g, Rectangle textBounds, string text, Brush backBrush, Brush foreBrush, bool alignToRight)
        {
            Rectangle rect = textBounds;
            StringFormat format = new StringFormat();
            if (alignToRight)
            {
                format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
            }
            format.Alignment = (this.Alignment == HorizontalAlignment.Left) ? StringAlignment.Near : ((this.Alignment == HorizontalAlignment.Center) ? StringAlignment.Center : StringAlignment.Far);
            format.FormatFlags |= StringFormatFlags.NoWrap;
            g.FillRectangle(backBrush, rect);
            rect.Offset(0, 2 * this.yMargin);
            rect.Height -= 2 * this.yMargin;
            g.DrawString(text, this.DataGridTableStyle.DataGrid.Font, foreBrush, rect, format);
            format.Dispose();
        }

        protected internal override void ReleaseHostedControl()
        {
            if (this.edit.ParentInternal != null)
            {
                this.edit.ParentInternal.Controls.Remove(this.edit);
            }
        }

        private void RollBack()
        {
            this.edit.Text = this.oldValue;
        }

        protected override void SetDataGridInColumn(DataGrid value)
        {
            base.SetDataGridInColumn(value);
            if (this.edit.ParentInternal != null)
            {
                this.edit.ParentInternal.Controls.Remove(this.edit);
            }
            if (value != null)
            {
                value.Controls.Add(this.edit);
            }
            this.edit.SetDataGrid(value);
        }

        protected internal override void UpdateUI(CurrencyManager source, int rowNum, string displayText)
        {
            this.edit.Text = this.GetText(this.GetColumnValueAtRow(source, rowNum));
            if (!this.edit.ReadOnly && (displayText != null))
            {
                this.edit.Text = displayText;
            }
        }

        [Editor("System.Windows.Forms.Design.DataGridColumnStyleFormatEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue((string) null)]
        public string Format
        {
            get
            {
                return this.format;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                if ((this.format == null) || !this.format.Equals(value))
                {
                    this.format = value;
                    if (((this.format.Length == 0) && (this.typeConverter != null)) && !this.typeConverter.CanConvertFrom(typeof(string)))
                    {
                        this.ReadOnly = true;
                    }
                    this.Invalidate();
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public IFormatProvider FormatInfo
        {
            get
            {
                return this.formatInfo;
            }
            set
            {
                if ((this.formatInfo == null) || !this.formatInfo.Equals(value))
                {
                    this.formatInfo = value;
                }
            }
        }

        [System.Windows.Forms.SRDescription("FormatControlFormatDescr"), DefaultValue((string) null)]
        public override System.ComponentModel.PropertyDescriptor PropertyDescriptor
        {
            set
            {
                base.PropertyDescriptor = value;
                if ((this.PropertyDescriptor != null) && (this.PropertyDescriptor.PropertyType != typeof(object)))
                {
                    this.typeConverter = TypeDescriptor.GetConverter(this.PropertyDescriptor.PropertyType);
                    this.parseMethod = this.PropertyDescriptor.PropertyType.GetMethod("Parse", new System.Type[] { typeof(string), typeof(IFormatProvider) });
                }
            }
        }

        public override bool ReadOnly
        {
            get
            {
                return base.ReadOnly;
            }
            set
            {
                if ((value || ((this.format != null) && (this.format.Length != 0))) || ((this.typeConverter == null) || this.typeConverter.CanConvertFrom(typeof(string))))
                {
                    base.ReadOnly = value;
                }
            }
        }

        [Browsable(false)]
        public virtual System.Windows.Forms.TextBox TextBox
        {
            get
            {
                return this.edit;
            }
        }
    }
}

