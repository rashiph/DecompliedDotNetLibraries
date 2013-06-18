namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Text;

    [ToolboxBitmap(typeof(DataGridViewTextBoxColumn), "DataGridViewTextBoxColumn.bmp")]
    public class DataGridViewTextBoxColumn : DataGridViewColumn
    {
        private const int DATAGRIDVIEWTEXTBOXCOLUMN_maxInputLength = 0x7fff;

        public DataGridViewTextBoxColumn() : base(new DataGridViewTextBoxCell())
        {
            this.SortMode = DataGridViewColumnSortMode.Automatic;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x40);
            builder.Append("DataGridViewTextBoxColumn { Name=");
            builder.Append(base.Name);
            builder.Append(", Index=");
            builder.Append(base.Index.ToString(CultureInfo.CurrentCulture));
            builder.Append(" }");
            return builder.ToString();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public override DataGridViewCell CellTemplate
        {
            get
            {
                return base.CellTemplate;
            }
            set
            {
                if ((value != null) && !(value is DataGridViewTextBoxCell))
                {
                    throw new InvalidCastException(System.Windows.Forms.SR.GetString("DataGridViewTypeColumn_WrongCellTemplateType", new object[] { "System.Windows.Forms.DataGridViewTextBoxCell" }));
                }
                base.CellTemplate = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("DataGridView_TextBoxColumnMaxInputLengthDescr"), DefaultValue(0x7fff)]
        public int MaxInputLength
        {
            get
            {
                if (this.TextBoxCellTemplate == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumn_CellTemplateRequired"));
                }
                return this.TextBoxCellTemplate.MaxInputLength;
            }
            set
            {
                if (this.MaxInputLength != value)
                {
                    this.TextBoxCellTemplate.MaxInputLength = value;
                    if (base.DataGridView != null)
                    {
                        DataGridViewRowCollection rows = base.DataGridView.Rows;
                        int count = rows.Count;
                        for (int i = 0; i < count; i++)
                        {
                            DataGridViewTextBoxCell cell = rows.SharedRow(i).Cells[base.Index] as DataGridViewTextBoxCell;
                            if (cell != null)
                            {
                                cell.MaxInputLength = value;
                            }
                        }
                    }
                }
            }
        }

        [DefaultValue(1)]
        public DataGridViewColumnSortMode SortMode
        {
            get
            {
                return base.SortMode;
            }
            set
            {
                base.SortMode = value;
            }
        }

        private DataGridViewTextBoxCell TextBoxCellTemplate
        {
            get
            {
                return (DataGridViewTextBoxCell) this.CellTemplate;
            }
        }
    }
}

