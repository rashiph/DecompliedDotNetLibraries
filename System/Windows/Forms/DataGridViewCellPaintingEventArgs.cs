namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;

    public class DataGridViewCellPaintingEventArgs : HandledEventArgs
    {
        private DataGridViewAdvancedBorderStyle advancedBorderStyle;
        private Rectangle cellBounds;
        private DataGridViewElementStates cellState;
        private DataGridViewCellStyle cellStyle;
        private Rectangle clipBounds;
        private int columnIndex;
        private DataGridView dataGridView;
        private string errorText;
        private object formattedValue;
        private System.Drawing.Graphics graphics;
        private DataGridViewPaintParts paintParts;
        private int rowIndex;
        private object value;

        internal DataGridViewCellPaintingEventArgs(DataGridView dataGridView)
        {
            this.dataGridView = dataGridView;
        }

        public DataGridViewCellPaintingEventArgs(DataGridView dataGridView, System.Drawing.Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, int columnIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            if (dataGridView == null)
            {
                throw new ArgumentNullException("dataGridView");
            }
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            if ((paintParts & ~DataGridViewPaintParts.All) != DataGridViewPaintParts.None)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewPaintPartsCombination", new object[] { "paintParts" }));
            }
            this.graphics = graphics;
            this.clipBounds = clipBounds;
            this.cellBounds = cellBounds;
            this.rowIndex = rowIndex;
            this.columnIndex = columnIndex;
            this.cellState = cellState;
            this.value = value;
            this.formattedValue = formattedValue;
            this.errorText = errorText;
            this.cellStyle = cellStyle;
            this.advancedBorderStyle = advancedBorderStyle;
            this.paintParts = paintParts;
        }

        public void Paint(Rectangle clipBounds, DataGridViewPaintParts paintParts)
        {
            if ((this.rowIndex < -1) || (this.rowIndex >= this.dataGridView.Rows.Count))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewElementPaintingEventArgs_RowIndexOutOfRange"));
            }
            if ((this.columnIndex < -1) || (this.columnIndex >= this.dataGridView.Columns.Count))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewElementPaintingEventArgs_ColumnIndexOutOfRange"));
            }
            this.dataGridView.GetCellInternal(this.columnIndex, this.rowIndex).PaintInternal(this.graphics, clipBounds, this.cellBounds, this.rowIndex, this.cellState, this.value, this.formattedValue, this.errorText, this.cellStyle, this.advancedBorderStyle, paintParts);
        }

        public void PaintBackground(Rectangle clipBounds, bool cellsPaintSelectionBackground)
        {
            if ((this.rowIndex < -1) || (this.rowIndex >= this.dataGridView.Rows.Count))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewElementPaintingEventArgs_RowIndexOutOfRange"));
            }
            if ((this.columnIndex < -1) || (this.columnIndex >= this.dataGridView.Columns.Count))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewElementPaintingEventArgs_ColumnIndexOutOfRange"));
            }
            DataGridViewPaintParts paintParts = DataGridViewPaintParts.Border | DataGridViewPaintParts.Background;
            if (cellsPaintSelectionBackground)
            {
                paintParts |= DataGridViewPaintParts.SelectionBackground;
            }
            this.dataGridView.GetCellInternal(this.columnIndex, this.rowIndex).PaintInternal(this.graphics, clipBounds, this.cellBounds, this.rowIndex, this.cellState, this.value, this.formattedValue, this.errorText, this.cellStyle, this.advancedBorderStyle, paintParts);
        }

        public void PaintContent(Rectangle clipBounds)
        {
            if ((this.rowIndex < -1) || (this.rowIndex >= this.dataGridView.Rows.Count))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewElementPaintingEventArgs_RowIndexOutOfRange"));
            }
            if ((this.columnIndex < -1) || (this.columnIndex >= this.dataGridView.Columns.Count))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewElementPaintingEventArgs_ColumnIndexOutOfRange"));
            }
            this.dataGridView.GetCellInternal(this.columnIndex, this.rowIndex).PaintInternal(this.graphics, clipBounds, this.cellBounds, this.rowIndex, this.cellState, this.value, this.formattedValue, this.errorText, this.cellStyle, this.advancedBorderStyle, DataGridViewPaintParts.ErrorIcon | DataGridViewPaintParts.ContentForeground | DataGridViewPaintParts.ContentBackground);
        }

        internal void SetProperties(System.Drawing.Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, int columnIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            this.graphics = graphics;
            this.clipBounds = clipBounds;
            this.cellBounds = cellBounds;
            this.rowIndex = rowIndex;
            this.columnIndex = columnIndex;
            this.cellState = cellState;
            this.value = value;
            this.formattedValue = formattedValue;
            this.errorText = errorText;
            this.cellStyle = cellStyle;
            this.advancedBorderStyle = advancedBorderStyle;
            this.paintParts = paintParts;
            base.Handled = false;
        }

        public DataGridViewAdvancedBorderStyle AdvancedBorderStyle
        {
            get
            {
                return this.advancedBorderStyle;
            }
        }

        public Rectangle CellBounds
        {
            get
            {
                return this.cellBounds;
            }
        }

        public DataGridViewCellStyle CellStyle
        {
            get
            {
                return this.cellStyle;
            }
        }

        public Rectangle ClipBounds
        {
            get
            {
                return this.clipBounds;
            }
        }

        public int ColumnIndex
        {
            get
            {
                return this.columnIndex;
            }
        }

        public string ErrorText
        {
            get
            {
                return this.errorText;
            }
        }

        public object FormattedValue
        {
            get
            {
                return this.formattedValue;
            }
        }

        public System.Drawing.Graphics Graphics
        {
            get
            {
                return this.graphics;
            }
        }

        public DataGridViewPaintParts PaintParts
        {
            get
            {
                return this.paintParts;
            }
        }

        public int RowIndex
        {
            get
            {
                return this.rowIndex;
            }
        }

        public DataGridViewElementStates State
        {
            get
            {
                return this.cellState;
            }
        }

        public object Value
        {
            get
            {
                return this.value;
            }
        }
    }
}

