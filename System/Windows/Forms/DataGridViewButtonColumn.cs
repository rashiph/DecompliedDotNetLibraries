namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Text;

    [ToolboxBitmap(typeof(DataGridViewButtonColumn), "DataGridViewButtonColumn.bmp")]
    public class DataGridViewButtonColumn : DataGridViewColumn
    {
        private static System.Type columnType = typeof(DataGridViewButtonColumn);
        private string text;

        public DataGridViewButtonColumn() : base(new DataGridViewButtonCell())
        {
            DataGridViewCellStyle style = new DataGridViewCellStyle {
                AlignmentInternal = DataGridViewContentAlignment.MiddleCenter
            };
            this.DefaultCellStyle = style;
        }

        public override object Clone()
        {
            DataGridViewButtonColumn column;
            System.Type type = base.GetType();
            if (type == columnType)
            {
                column = new DataGridViewButtonColumn();
            }
            else
            {
                column = (DataGridViewButtonColumn) Activator.CreateInstance(type);
            }
            if (column != null)
            {
                base.CloneInternal(column);
                column.Text = this.text;
            }
            return column;
        }

        private bool ShouldSerializeDefaultCellStyle()
        {
            if (!base.HasDefaultCellStyle)
            {
                return false;
            }
            DataGridViewCellStyle defaultCellStyle = this.DefaultCellStyle;
            if ((((defaultCellStyle.BackColor.IsEmpty && defaultCellStyle.ForeColor.IsEmpty) && (defaultCellStyle.SelectionBackColor.IsEmpty && defaultCellStyle.SelectionForeColor.IsEmpty)) && (((defaultCellStyle.Font == null) && defaultCellStyle.IsNullValueDefault) && (defaultCellStyle.IsDataSourceNullValueDefault && string.IsNullOrEmpty(defaultCellStyle.Format)))) && ((defaultCellStyle.FormatProvider.Equals(CultureInfo.CurrentCulture) && (defaultCellStyle.Alignment == DataGridViewContentAlignment.MiddleCenter)) && ((defaultCellStyle.WrapMode == DataGridViewTriState.NotSet) && (defaultCellStyle.Tag == null))))
            {
                return !defaultCellStyle.Padding.Equals(Padding.Empty);
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x40);
            builder.Append("DataGridViewButtonColumn { Name=");
            builder.Append(base.Name);
            builder.Append(", Index=");
            builder.Append(base.Index.ToString(CultureInfo.CurrentCulture));
            builder.Append(" }");
            return builder.ToString();
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override DataGridViewCell CellTemplate
        {
            get
            {
                return base.CellTemplate;
            }
            set
            {
                if ((value != null) && !(value is DataGridViewButtonCell))
                {
                    throw new InvalidCastException(System.Windows.Forms.SR.GetString("DataGridViewTypeColumn_WrongCellTemplateType", new object[] { "System.Windows.Forms.DataGridViewButtonCell" }));
                }
                base.CellTemplate = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), Browsable(true), System.Windows.Forms.SRDescription("DataGridView_ColumnDefaultCellStyleDescr")]
        public override DataGridViewCellStyle DefaultCellStyle
        {
            get
            {
                return base.DefaultCellStyle;
            }
            set
            {
                base.DefaultCellStyle = value;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridView_ButtonColumnFlatStyleDescr"), DefaultValue(2), System.Windows.Forms.SRCategory("CatAppearance")]
        public System.Windows.Forms.FlatStyle FlatStyle
        {
            get
            {
                if (this.CellTemplate == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumn_CellTemplateRequired"));
                }
                return ((DataGridViewButtonCell) this.CellTemplate).FlatStyle;
            }
            set
            {
                if (this.FlatStyle != value)
                {
                    ((DataGridViewButtonCell) this.CellTemplate).FlatStyle = value;
                    if (base.DataGridView != null)
                    {
                        DataGridViewRowCollection rows = base.DataGridView.Rows;
                        int count = rows.Count;
                        for (int i = 0; i < count; i++)
                        {
                            DataGridViewButtonCell cell = rows.SharedRow(i).Cells[base.Index] as DataGridViewButtonCell;
                            if (cell != null)
                            {
                                cell.FlatStyleInternal = value;
                            }
                        }
                        base.DataGridView.OnColumnCommonChange(base.Index);
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("DataGridView_ButtonColumnTextDescr"), DefaultValue((string) null)]
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                if (!string.Equals(value, this.text, StringComparison.Ordinal))
                {
                    this.text = value;
                    if (base.DataGridView != null)
                    {
                        if (this.UseColumnTextForButtonValue)
                        {
                            base.DataGridView.OnColumnCommonChange(base.Index);
                        }
                        else
                        {
                            DataGridViewRowCollection rows = base.DataGridView.Rows;
                            int count = rows.Count;
                            for (int i = 0; i < count; i++)
                            {
                                DataGridViewButtonCell cell = rows.SharedRow(i).Cells[base.Index] as DataGridViewButtonCell;
                                if ((cell != null) && cell.UseColumnTextForButtonValue)
                                {
                                    base.DataGridView.OnColumnCommonChange(base.Index);
                                    return;
                                }
                            }
                            base.DataGridView.InvalidateColumn(base.Index);
                        }
                    }
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("DataGridView_ButtonColumnUseColumnTextForButtonValueDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public bool UseColumnTextForButtonValue
        {
            get
            {
                if (this.CellTemplate == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumn_CellTemplateRequired"));
                }
                return ((DataGridViewButtonCell) this.CellTemplate).UseColumnTextForButtonValue;
            }
            set
            {
                if (this.UseColumnTextForButtonValue != value)
                {
                    ((DataGridViewButtonCell) this.CellTemplate).UseColumnTextForButtonValueInternal = value;
                    if (base.DataGridView != null)
                    {
                        DataGridViewRowCollection rows = base.DataGridView.Rows;
                        int count = rows.Count;
                        for (int i = 0; i < count; i++)
                        {
                            DataGridViewButtonCell cell = rows.SharedRow(i).Cells[base.Index] as DataGridViewButtonCell;
                            if (cell != null)
                            {
                                cell.UseColumnTextForButtonValueInternal = value;
                            }
                        }
                        base.DataGridView.OnColumnCommonChange(base.Index);
                    }
                }
            }
        }
    }
}

