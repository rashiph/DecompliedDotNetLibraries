namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;

    public class DataGridViewRowPrePaintEventArgs : HandledEventArgs
    {
        private Rectangle clipBounds;
        private DataGridView dataGridView;
        private string errorText;
        private System.Drawing.Graphics graphics;
        private DataGridViewCellStyle inheritedRowStyle;
        private bool isFirstDisplayedRow;
        private bool isLastVisibleRow;
        private DataGridViewPaintParts paintParts;
        private Rectangle rowBounds;
        private int rowIndex;
        private DataGridViewElementStates rowState;

        internal DataGridViewRowPrePaintEventArgs(DataGridView dataGridView)
        {
            this.dataGridView = dataGridView;
        }

        public DataGridViewRowPrePaintEventArgs(DataGridView dataGridView, System.Drawing.Graphics graphics, Rectangle clipBounds, Rectangle rowBounds, int rowIndex, DataGridViewElementStates rowState, string errorText, DataGridViewCellStyle inheritedRowStyle, bool isFirstDisplayedRow, bool isLastVisibleRow)
        {
            if (dataGridView == null)
            {
                throw new ArgumentNullException("dataGridView");
            }
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (inheritedRowStyle == null)
            {
                throw new ArgumentNullException("inheritedRowStyle");
            }
            this.dataGridView = dataGridView;
            this.graphics = graphics;
            this.clipBounds = clipBounds;
            this.rowBounds = rowBounds;
            this.rowIndex = rowIndex;
            this.rowState = rowState;
            this.errorText = errorText;
            this.inheritedRowStyle = inheritedRowStyle;
            this.isFirstDisplayedRow = isFirstDisplayedRow;
            this.isLastVisibleRow = isLastVisibleRow;
            this.paintParts = DataGridViewPaintParts.All;
        }

        public void DrawFocus(Rectangle bounds, bool cellsPaintSelectionBackground)
        {
            if ((this.rowIndex < 0) || (this.rowIndex >= this.dataGridView.Rows.Count))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewElementPaintingEventArgs_RowIndexOutOfRange"));
            }
            this.dataGridView.Rows.SharedRow(this.rowIndex).DrawFocus(this.graphics, this.clipBounds, bounds, this.rowIndex, this.rowState, this.inheritedRowStyle, cellsPaintSelectionBackground);
        }

        public void PaintCells(Rectangle clipBounds, DataGridViewPaintParts paintParts)
        {
            if ((this.rowIndex < 0) || (this.rowIndex >= this.dataGridView.Rows.Count))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewElementPaintingEventArgs_RowIndexOutOfRange"));
            }
            this.dataGridView.Rows.SharedRow(this.rowIndex).PaintCells(this.graphics, clipBounds, this.rowBounds, this.rowIndex, this.rowState, this.isFirstDisplayedRow, this.isLastVisibleRow, paintParts);
        }

        public void PaintCellsBackground(Rectangle clipBounds, bool cellsPaintSelectionBackground)
        {
            if ((this.rowIndex < 0) || (this.rowIndex >= this.dataGridView.Rows.Count))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewElementPaintingEventArgs_RowIndexOutOfRange"));
            }
            DataGridViewPaintParts paintParts = DataGridViewPaintParts.Border | DataGridViewPaintParts.Background;
            if (cellsPaintSelectionBackground)
            {
                paintParts |= DataGridViewPaintParts.SelectionBackground;
            }
            this.dataGridView.Rows.SharedRow(this.rowIndex).PaintCells(this.graphics, clipBounds, this.rowBounds, this.rowIndex, this.rowState, this.isFirstDisplayedRow, this.isLastVisibleRow, paintParts);
        }

        public void PaintCellsContent(Rectangle clipBounds)
        {
            if ((this.rowIndex < 0) || (this.rowIndex >= this.dataGridView.Rows.Count))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewElementPaintingEventArgs_RowIndexOutOfRange"));
            }
            this.dataGridView.Rows.SharedRow(this.rowIndex).PaintCells(this.graphics, clipBounds, this.rowBounds, this.rowIndex, this.rowState, this.isFirstDisplayedRow, this.isLastVisibleRow, DataGridViewPaintParts.ErrorIcon | DataGridViewPaintParts.ContentForeground | DataGridViewPaintParts.ContentBackground);
        }

        public void PaintHeader(bool paintSelectionBackground)
        {
            DataGridViewPaintParts paintParts = DataGridViewPaintParts.ErrorIcon | DataGridViewPaintParts.ContentForeground | DataGridViewPaintParts.ContentBackground | DataGridViewPaintParts.Border | DataGridViewPaintParts.Background;
            if (paintSelectionBackground)
            {
                paintParts |= DataGridViewPaintParts.SelectionBackground;
            }
            this.PaintHeader(paintParts);
        }

        public void PaintHeader(DataGridViewPaintParts paintParts)
        {
            if ((this.rowIndex < 0) || (this.rowIndex >= this.dataGridView.Rows.Count))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewElementPaintingEventArgs_RowIndexOutOfRange"));
            }
            this.dataGridView.Rows.SharedRow(this.rowIndex).PaintHeader(this.graphics, this.clipBounds, this.rowBounds, this.rowIndex, this.rowState, this.isFirstDisplayedRow, this.isLastVisibleRow, paintParts);
        }

        internal void SetProperties(System.Drawing.Graphics graphics, Rectangle clipBounds, Rectangle rowBounds, int rowIndex, DataGridViewElementStates rowState, string errorText, DataGridViewCellStyle inheritedRowStyle, bool isFirstDisplayedRow, bool isLastVisibleRow)
        {
            this.graphics = graphics;
            this.clipBounds = clipBounds;
            this.rowBounds = rowBounds;
            this.rowIndex = rowIndex;
            this.rowState = rowState;
            this.errorText = errorText;
            this.inheritedRowStyle = inheritedRowStyle;
            this.isFirstDisplayedRow = isFirstDisplayedRow;
            this.isLastVisibleRow = isLastVisibleRow;
            this.paintParts = DataGridViewPaintParts.All;
            base.Handled = false;
        }

        public Rectangle ClipBounds
        {
            get
            {
                return this.clipBounds;
            }
            set
            {
                this.clipBounds = value;
            }
        }

        public string ErrorText
        {
            get
            {
                return this.errorText;
            }
        }

        public System.Drawing.Graphics Graphics
        {
            get
            {
                return this.graphics;
            }
        }

        public DataGridViewCellStyle InheritedRowStyle
        {
            get
            {
                return this.inheritedRowStyle;
            }
        }

        public bool IsFirstDisplayedRow
        {
            get
            {
                return this.isFirstDisplayedRow;
            }
        }

        public bool IsLastVisibleRow
        {
            get
            {
                return this.isLastVisibleRow;
            }
        }

        public DataGridViewPaintParts PaintParts
        {
            get
            {
                return this.paintParts;
            }
            set
            {
                if ((value & ~DataGridViewPaintParts.All) != DataGridViewPaintParts.None)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_InvalidDataGridViewPaintPartsCombination", new object[] { "value" }));
                }
                this.paintParts = value;
            }
        }

        public Rectangle RowBounds
        {
            get
            {
                return this.rowBounds;
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
                return this.rowState;
            }
        }
    }
}

