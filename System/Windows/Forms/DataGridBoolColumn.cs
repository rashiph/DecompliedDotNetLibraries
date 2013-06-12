namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    public class DataGridBoolColumn : DataGridColumnStyle
    {
        private bool allowNull;
        private object currentValue;
        private int editingRow;
        private static readonly object EventAllowNull = new object();
        private static readonly object EventFalseValue = new object();
        private static readonly object EventTrueValue = new object();
        private object falseValue;
        private static readonly int idealCheckSize = 14;
        private bool isEditing;
        private bool isSelected;
        private object nullValue;
        private object trueValue;

        public event EventHandler AllowNullChanged
        {
            add
            {
                base.Events.AddHandler(EventAllowNull, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventAllowNull, value);
            }
        }

        public event EventHandler FalseValueChanged
        {
            add
            {
                base.Events.AddHandler(EventFalseValue, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventFalseValue, value);
            }
        }

        public event EventHandler TrueValueChanged
        {
            add
            {
                base.Events.AddHandler(EventTrueValue, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventTrueValue, value);
            }
        }

        public DataGridBoolColumn()
        {
            this.allowNull = true;
            this.editingRow = -1;
            this.currentValue = Convert.DBNull;
            this.trueValue = true;
            this.falseValue = false;
            this.nullValue = Convert.DBNull;
        }

        public DataGridBoolColumn(PropertyDescriptor prop) : base(prop)
        {
            this.allowNull = true;
            this.editingRow = -1;
            this.currentValue = Convert.DBNull;
            this.trueValue = true;
            this.falseValue = false;
            this.nullValue = Convert.DBNull;
        }

        public DataGridBoolColumn(PropertyDescriptor prop, bool isDefault) : base(prop, isDefault)
        {
            this.allowNull = true;
            this.editingRow = -1;
            this.currentValue = Convert.DBNull;
            this.trueValue = true;
            this.falseValue = false;
            this.nullValue = Convert.DBNull;
        }

        protected internal override void Abort(int rowNum)
        {
            this.isSelected = false;
            this.isEditing = false;
            this.Invalidate();
        }

        protected internal override bool Commit(CurrencyManager dataSource, int rowNum)
        {
            this.isSelected = false;
            this.Invalidate();
            if (this.isEditing)
            {
                this.SetColumnValueAtRow(dataSource, rowNum, this.currentValue);
                this.isEditing = false;
            }
            return true;
        }

        protected internal override void ConcedeFocus()
        {
            base.ConcedeFocus();
            this.isSelected = false;
            this.isEditing = false;
        }

        protected internal override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string displayText, bool cellIsVisible)
        {
            this.isSelected = true;
            DataGrid dataGrid = this.DataGridTableStyle.DataGrid;
            if (!dataGrid.Focused)
            {
                dataGrid.FocusInternal();
            }
            if (!readOnly && !this.IsReadOnly())
            {
                this.editingRow = rowNum;
                this.currentValue = this.GetColumnValueAtRow(source, rowNum);
            }
            base.Invalidate();
        }

        protected internal override void EnterNullValue()
        {
            if ((this.AllowNull && !this.IsReadOnly()) && (this.currentValue != Convert.DBNull))
            {
                this.currentValue = Convert.DBNull;
                this.Invalidate();
            }
        }

        private Rectangle GetCheckBoxBounds(Rectangle bounds, bool alignToRight)
        {
            if (alignToRight)
            {
                return new Rectangle(bounds.X + ((bounds.Width - idealCheckSize) / 2), bounds.Y + ((bounds.Height - idealCheckSize) / 2), (bounds.Width < idealCheckSize) ? bounds.Width : idealCheckSize, idealCheckSize);
            }
            return new Rectangle(Math.Max(0, bounds.X + ((bounds.Width - idealCheckSize) / 2)), Math.Max(0, bounds.Y + ((bounds.Height - idealCheckSize) / 2)), (bounds.Width < idealCheckSize) ? bounds.Width : idealCheckSize, idealCheckSize);
        }

        protected internal override object GetColumnValueAtRow(CurrencyManager lm, int row)
        {
            object columnValueAtRow = base.GetColumnValueAtRow(lm, row);
            object dBNull = Convert.DBNull;
            if (columnValueAtRow.Equals(this.trueValue))
            {
                return true;
            }
            if (columnValueAtRow.Equals(this.falseValue))
            {
                dBNull = false;
            }
            return dBNull;
        }

        protected internal override int GetMinimumHeight()
        {
            return (idealCheckSize + 2);
        }

        protected internal override int GetPreferredHeight(Graphics g, object value)
        {
            return (idealCheckSize + 2);
        }

        protected internal override Size GetPreferredSize(Graphics g, object value)
        {
            return new Size(idealCheckSize + 2, idealCheckSize + 2);
        }

        private bool IsReadOnly()
        {
            bool readOnly = this.ReadOnly;
            if (this.DataGridTableStyle != null)
            {
                readOnly = readOnly || this.DataGridTableStyle.ReadOnly;
                if (this.DataGridTableStyle.DataGrid != null)
                {
                    readOnly = readOnly || this.DataGridTableStyle.DataGrid.ReadOnly;
                }
            }
            return readOnly;
        }

        internal override bool KeyPress(int rowNum, Keys keyData)
        {
            if ((this.isSelected && (this.editingRow == rowNum)) && (!this.IsReadOnly() && ((keyData & Keys.KeyCode) == Keys.Space)))
            {
                this.ToggleValue();
                this.Invalidate();
                return true;
            }
            return base.KeyPress(rowNum, keyData);
        }

        internal override bool MouseDown(int rowNum, int x, int y)
        {
            base.MouseDown(rowNum, x, y);
            if ((this.isSelected && (this.editingRow == rowNum)) && !this.IsReadOnly())
            {
                this.ToggleValue();
                this.Invalidate();
                return true;
            }
            return false;
        }

        private void OnAllowNullChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventAllowNull] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnFalseValueChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventFalseValue] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnTrueValueChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventTrueValue] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum)
        {
            this.Paint(g, bounds, source, rowNum, false);
        }

        protected internal override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, bool alignToRight)
        {
            this.Paint(g, bounds, source, rowNum, this.DataGridTableStyle.BackBrush, this.DataGridTableStyle.ForeBrush, alignToRight);
        }

        protected internal override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
        {
            object obj2 = (this.isEditing && (this.editingRow == rowNum)) ? this.currentValue : this.GetColumnValueAtRow(source, rowNum);
            ButtonState inactive = ButtonState.Inactive;
            if (!Convert.IsDBNull(obj2))
            {
                inactive = ((bool) obj2) ? ButtonState.Checked : ButtonState.Normal;
            }
            Rectangle checkBoxBounds = this.GetCheckBoxBounds(bounds, alignToRight);
            Region clip = g.Clip;
            g.ExcludeClip(checkBoxBounds);
            Brush brush = this.DataGridTableStyle.IsDefault ? this.DataGridTableStyle.DataGrid.SelectionBackBrush : this.DataGridTableStyle.SelectionBackBrush;
            if ((this.isSelected && (this.editingRow == rowNum)) && !this.IsReadOnly())
            {
                g.FillRectangle(brush, bounds);
            }
            else
            {
                g.FillRectangle(backBrush, bounds);
            }
            g.Clip = clip;
            if (inactive == ButtonState.Inactive)
            {
                ControlPaint.DrawMixedCheckBox(g, checkBoxBounds, ButtonState.Checked);
            }
            else
            {
                ControlPaint.DrawCheckBox(g, checkBoxBounds, inactive);
            }
            if ((this.IsReadOnly() && this.isSelected) && (source.Position == rowNum))
            {
                bounds.Inflate(-1, -1);
                Pen pen = new Pen(brush) {
                    DashStyle = DashStyle.Dash
                };
                g.DrawRectangle(pen, bounds);
                pen.Dispose();
                bounds.Inflate(1, 1);
            }
        }

        private void ResetNullValue()
        {
            this.NullValue = Convert.DBNull;
        }

        protected internal override void SetColumnValueAtRow(CurrencyManager lm, int row, object value)
        {
            object trueValue = null;
            bool flag = true;
            if (flag.Equals(value))
            {
                trueValue = this.TrueValue;
            }
            else
            {
                bool flag2 = false;
                if (flag2.Equals(value))
                {
                    trueValue = this.FalseValue;
                }
                else if (Convert.IsDBNull(value))
                {
                    trueValue = this.NullValue;
                }
            }
            this.currentValue = trueValue;
            base.SetColumnValueAtRow(lm, row, trueValue);
        }

        private bool ShouldSerializeNullValue()
        {
            return (this.nullValue != Convert.DBNull);
        }

        private void ToggleValue()
        {
            if ((this.currentValue is bool) && !((bool) this.currentValue))
            {
                this.currentValue = true;
            }
            else if (this.AllowNull)
            {
                if (Convert.IsDBNull(this.currentValue))
                {
                    this.currentValue = false;
                }
                else
                {
                    this.currentValue = Convert.DBNull;
                }
            }
            else
            {
                this.currentValue = false;
            }
            this.isEditing = true;
            this.DataGridTableStyle.DataGrid.ColumnStartedEditing(Rectangle.Empty);
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), System.Windows.Forms.SRDescription("DataGridBoolColumnAllowNullValue")]
        public bool AllowNull
        {
            get
            {
                return this.allowNull;
            }
            set
            {
                if (this.allowNull != value)
                {
                    this.allowNull = value;
                    if (!value && Convert.IsDBNull(this.currentValue))
                    {
                        this.currentValue = false;
                        this.Invalidate();
                    }
                    this.OnAllowNullChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(false), TypeConverter(typeof(StringConverter))]
        public object FalseValue
        {
            get
            {
                return this.falseValue;
            }
            set
            {
                if (!this.falseValue.Equals(value))
                {
                    this.falseValue = value;
                    this.OnFalseValueChanged(EventArgs.Empty);
                    this.Invalidate();
                }
            }
        }

        [TypeConverter(typeof(StringConverter))]
        public object NullValue
        {
            get
            {
                return this.nullValue;
            }
            set
            {
                if (!this.nullValue.Equals(value))
                {
                    this.nullValue = value;
                    this.OnFalseValueChanged(EventArgs.Empty);
                    this.Invalidate();
                }
            }
        }

        [TypeConverter(typeof(StringConverter)), DefaultValue(true)]
        public object TrueValue
        {
            get
            {
                return this.trueValue;
            }
            set
            {
                if (!this.trueValue.Equals(value))
                {
                    this.trueValue = value;
                    this.OnTrueValueChanged(EventArgs.Empty);
                    this.Invalidate();
                }
            }
        }
    }
}

