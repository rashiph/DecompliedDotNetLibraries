namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Text;

    [ToolboxBitmap(typeof(DataGridViewImageColumn), "DataGridViewImageColumn.bmp")]
    public class DataGridViewImageColumn : DataGridViewColumn
    {
        private static System.Type columnType = typeof(DataGridViewImageColumn);
        private System.Drawing.Icon icon;
        private System.Drawing.Image image;

        public DataGridViewImageColumn() : this(false)
        {
        }

        public DataGridViewImageColumn(bool valuesAreIcons) : base(new DataGridViewImageCell(valuesAreIcons))
        {
            DataGridViewCellStyle style = new DataGridViewCellStyle {
                AlignmentInternal = DataGridViewContentAlignment.MiddleCenter
            };
            if (valuesAreIcons)
            {
                style.NullValue = DataGridViewImageCell.ErrorIcon;
            }
            else
            {
                style.NullValue = DataGridViewImageCell.ErrorBitmap;
            }
            this.DefaultCellStyle = style;
        }

        public override object Clone()
        {
            DataGridViewImageColumn column;
            System.Type type = base.GetType();
            if (type == columnType)
            {
                column = new DataGridViewImageColumn();
            }
            else
            {
                column = (DataGridViewImageColumn) Activator.CreateInstance(type);
            }
            if (column != null)
            {
                base.CloneInternal(column);
                column.Icon = this.icon;
                column.Image = this.image;
            }
            return column;
        }

        private bool ShouldSerializeDefaultCellStyle()
        {
            DataGridViewImageCell cellTemplate = this.CellTemplate as DataGridViewImageCell;
            if (cellTemplate != null)
            {
                object errorIcon;
                if (!base.HasDefaultCellStyle)
                {
                    return false;
                }
                if (cellTemplate.ValueIsIcon)
                {
                    errorIcon = DataGridViewImageCell.ErrorIcon;
                }
                else
                {
                    errorIcon = DataGridViewImageCell.ErrorBitmap;
                }
                DataGridViewCellStyle defaultCellStyle = this.DefaultCellStyle;
                if ((((defaultCellStyle.BackColor.IsEmpty && defaultCellStyle.ForeColor.IsEmpty) && (defaultCellStyle.SelectionBackColor.IsEmpty && defaultCellStyle.SelectionForeColor.IsEmpty)) && (((defaultCellStyle.Font == null) && errorIcon.Equals(defaultCellStyle.NullValue)) && (defaultCellStyle.IsDataSourceNullValueDefault && string.IsNullOrEmpty(defaultCellStyle.Format)))) && ((defaultCellStyle.FormatProvider.Equals(CultureInfo.CurrentCulture) && (defaultCellStyle.Alignment == DataGridViewContentAlignment.MiddleCenter)) && ((defaultCellStyle.WrapMode == DataGridViewTriState.NotSet) && (defaultCellStyle.Tag == null))))
                {
                    return !defaultCellStyle.Padding.Equals(Padding.Empty);
                }
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x40);
            builder.Append("DataGridViewImageColumn { Name=");
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
                if ((value != null) && !(value is DataGridViewImageCell))
                {
                    throw new InvalidCastException(System.Windows.Forms.SR.GetString("DataGridViewTypeColumn_WrongCellTemplateType", new object[] { "System.Windows.Forms.DataGridViewImageCell" }));
                }
                base.CellTemplate = value;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridView_ColumnDefaultCellStyleDescr"), Browsable(true), System.Windows.Forms.SRCategory("CatAppearance")]
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

        [DefaultValue(""), Browsable(true), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("DataGridViewImageColumn_DescriptionDescr")]
        public string Description
        {
            get
            {
                if (this.CellTemplate == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumn_CellTemplateRequired"));
                }
                return this.ImageCellTemplate.Description;
            }
            set
            {
                if (this.CellTemplate == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumn_CellTemplateRequired"));
                }
                this.ImageCellTemplate.Description = value;
                if (base.DataGridView != null)
                {
                    DataGridViewRowCollection rows = base.DataGridView.Rows;
                    int count = rows.Count;
                    for (int i = 0; i < count; i++)
                    {
                        DataGridViewImageCell cell = rows.SharedRow(i).Cells[base.Index] as DataGridViewImageCell;
                        if (cell != null)
                        {
                            cell.Description = value;
                        }
                    }
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Drawing.Icon Icon
        {
            get
            {
                return this.icon;
            }
            set
            {
                this.icon = value;
                if (base.DataGridView != null)
                {
                    base.DataGridView.OnColumnCommonChange(base.Index);
                }
            }
        }

        [System.Windows.Forms.SRDescription("DataGridViewImageColumn_ImageDescr"), DefaultValue((string) null), System.Windows.Forms.SRCategory("CatAppearance")]
        public System.Drawing.Image Image
        {
            get
            {
                return this.image;
            }
            set
            {
                this.image = value;
                if (base.DataGridView != null)
                {
                    base.DataGridView.OnColumnCommonChange(base.Index);
                }
            }
        }

        private DataGridViewImageCell ImageCellTemplate
        {
            get
            {
                return (DataGridViewImageCell) this.CellTemplate;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridViewImageColumn_ImageLayoutDescr"), DefaultValue(1), System.Windows.Forms.SRCategory("CatAppearance")]
        public DataGridViewImageCellLayout ImageLayout
        {
            get
            {
                if (this.CellTemplate == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumn_CellTemplateRequired"));
                }
                DataGridViewImageCellLayout imageLayout = this.ImageCellTemplate.ImageLayout;
                if (imageLayout == DataGridViewImageCellLayout.NotSet)
                {
                    imageLayout = DataGridViewImageCellLayout.Normal;
                }
                return imageLayout;
            }
            set
            {
                if (this.ImageLayout != value)
                {
                    this.ImageCellTemplate.ImageLayout = value;
                    if (base.DataGridView != null)
                    {
                        DataGridViewRowCollection rows = base.DataGridView.Rows;
                        int count = rows.Count;
                        for (int i = 0; i < count; i++)
                        {
                            DataGridViewImageCell cell = rows.SharedRow(i).Cells[base.Index] as DataGridViewImageCell;
                            if (cell != null)
                            {
                                cell.ImageLayoutInternal = value;
                            }
                        }
                        base.DataGridView.OnColumnCommonChange(base.Index);
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool ValuesAreIcons
        {
            get
            {
                if (this.ImageCellTemplate == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumn_CellTemplateRequired"));
                }
                return this.ImageCellTemplate.ValueIsIcon;
            }
            set
            {
                if (this.ValuesAreIcons != value)
                {
                    this.ImageCellTemplate.ValueIsIconInternal = value;
                    if (base.DataGridView != null)
                    {
                        DataGridViewRowCollection rows = base.DataGridView.Rows;
                        int count = rows.Count;
                        for (int i = 0; i < count; i++)
                        {
                            DataGridViewImageCell cell = rows.SharedRow(i).Cells[base.Index] as DataGridViewImageCell;
                            if (cell != null)
                            {
                                cell.ValueIsIconInternal = value;
                            }
                        }
                        base.DataGridView.OnColumnCommonChange(base.Index);
                    }
                    if ((value && (this.DefaultCellStyle.NullValue is Bitmap)) && (((Bitmap) this.DefaultCellStyle.NullValue) == DataGridViewImageCell.ErrorBitmap))
                    {
                        this.DefaultCellStyle.NullValue = DataGridViewImageCell.ErrorIcon;
                    }
                    else if ((!value && (this.DefaultCellStyle.NullValue is System.Drawing.Icon)) && (((System.Drawing.Icon) this.DefaultCellStyle.NullValue) == DataGridViewImageCell.ErrorIcon))
                    {
                        this.DefaultCellStyle.NullValue = DataGridViewImageCell.ErrorBitmap;
                    }
                }
            }
        }
    }
}

