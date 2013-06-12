namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    internal abstract class DataGridRow : MarshalByRefObject
    {
        private System.Windows.Forms.AccessibleObject accessibleObject;
        private static ColorMap[] colorMap = new ColorMap[] { new ColorMap() };
        protected System.Windows.Forms.DataGridTableStyle dgTable;
        private static Bitmap errorBmp = null;
        private int height;
        private static Bitmap leftArrow = null;
        protected internal int number;
        private static Bitmap pencilBmp = null;
        private static Bitmap rightArrow = null;
        private bool selected;
        private static Bitmap starBmp = null;
        private string tooltip = string.Empty;
        private IntPtr tooltipID = new IntPtr(-1);
        protected const int xOffset = 3;
        protected const int yOffset = 2;

        public DataGridRow(System.Windows.Forms.DataGrid dataGrid, System.Windows.Forms.DataGridTableStyle dgTable, int rowNumber)
        {
            if ((dataGrid == null) || (dgTable.DataGrid == null))
            {
                throw new ArgumentNullException("dataGrid");
            }
            if (rowNumber < 0)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridRowRowNumber"), "rowNumber");
            }
            this.number = rowNumber;
            colorMap[0].OldColor = Color.Black;
            colorMap[0].NewColor = dgTable.HeaderForeColor;
            this.dgTable = dgTable;
            this.height = this.MinimumRowHeight(dgTable);
        }

        protected Brush BackBrushForDataPaint(ref DataGridCell current, DataGridColumnStyle gridColumn, int column)
        {
            Brush backBrush = this.GetBackBrush();
            if (this.Selected)
            {
                backBrush = this.dgTable.IsDefault ? this.DataGrid.SelectionBackBrush : this.dgTable.SelectionBackBrush;
            }
            return backBrush;
        }

        protected virtual System.Windows.Forms.AccessibleObject CreateAccessibleObject()
        {
            return new DataGridRowAccessibleObject(this);
        }

        protected Brush ForeBrushForDataPaint(ref DataGridCell current, DataGridColumnStyle gridColumn, int column)
        {
            Brush brush = this.dgTable.IsDefault ? this.DataGrid.ForeBrush : this.dgTable.ForeBrush;
            if (this.Selected)
            {
                brush = this.dgTable.IsDefault ? this.DataGrid.SelectionForeBrush : this.dgTable.SelectionForeBrush;
            }
            return brush;
        }

        protected Brush GetBackBrush()
        {
            Brush brush = this.dgTable.IsDefault ? this.DataGrid.BackBrush : this.dgTable.BackBrush;
            if (this.DataGrid.LedgerStyle && ((this.RowNumber % 2) == 1))
            {
                brush = this.dgTable.IsDefault ? this.DataGrid.AlternatingBackBrush : this.dgTable.AlternatingBackBrush;
            }
            return brush;
        }

        protected Bitmap GetBitmap(string bitmapName)
        {
            Bitmap bitmap = null;
            try
            {
                bitmap = new Bitmap(typeof(DataGridCaption), bitmapName);
                bitmap.MakeTransparent();
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return bitmap;
        }

        public virtual Rectangle GetCellBounds(int col)
        {
            int firstVisibleColumn = this.dgTable.DataGrid.FirstVisibleColumn;
            int x = 0;
            Rectangle rectangle = new Rectangle();
            GridColumnStylesCollection gridColumnStyles = this.dgTable.GridColumnStyles;
            if (gridColumnStyles == null)
            {
                return rectangle;
            }
            for (int i = firstVisibleColumn; i < col; i++)
            {
                if (gridColumnStyles[i].PropertyDescriptor != null)
                {
                    x += gridColumnStyles[i].Width;
                }
            }
            int gridLineWidth = this.dgTable.GridLineWidth;
            return new Rectangle(x, 0, gridColumnStyles[col].Width - gridLineWidth, this.Height - gridLineWidth);
        }

        protected Bitmap GetErrorBitmap()
        {
            if (errorBmp == null)
            {
                errorBmp = this.GetBitmap("DataGridRow.error.bmp");
            }
            errorBmp.MakeTransparent();
            return errorBmp;
        }

        protected Bitmap GetLeftArrowBitmap()
        {
            if (leftArrow == null)
            {
                leftArrow = this.GetBitmap("DataGridRow.left.bmp");
            }
            return leftArrow;
        }

        public virtual Rectangle GetNonScrollableArea()
        {
            return Rectangle.Empty;
        }

        protected Bitmap GetPencilBitmap()
        {
            if (pencilBmp == null)
            {
                pencilBmp = this.GetBitmap("DataGridRow.pencil.bmp");
            }
            return pencilBmp;
        }

        protected Bitmap GetRightArrowBitmap()
        {
            if (rightArrow == null)
            {
                rightArrow = this.GetBitmap("DataGridRow.right.bmp");
            }
            return rightArrow;
        }

        protected Bitmap GetStarBitmap()
        {
            if (starBmp == null)
            {
                starBmp = this.GetBitmap("DataGridRow.star.bmp");
            }
            return starBmp;
        }

        public virtual void InvalidateRow()
        {
            this.dgTable.DataGrid.InvalidateRow(this.number);
        }

        public virtual void InvalidateRowRect(Rectangle r)
        {
            this.dgTable.DataGrid.InvalidateRowRect(this.number, r);
        }

        internal abstract void LoseChildFocus(Rectangle rowHeaders, bool alignToRight);
        protected internal virtual int MinimumRowHeight(System.Windows.Forms.DataGridTableStyle dgTable)
        {
            return this.MinimumRowHeight(dgTable.GridColumnStyles);
        }

        protected internal virtual int MinimumRowHeight(GridColumnStylesCollection columns)
        {
            int num = this.dgTable.IsDefault ? this.DataGrid.PreferredRowHeight : this.dgTable.PreferredRowHeight;
            try
            {
                if (this.dgTable.DataGrid.DataSource == null)
                {
                    return num;
                }
                int count = columns.Count;
                for (int i = 0; i < count; i++)
                {
                    if (columns[i].PropertyDescriptor != null)
                    {
                        num = Math.Max(num, columns[i].GetMinimumHeight());
                    }
                }
            }
            catch
            {
            }
            return num;
        }

        public virtual void OnEdit()
        {
        }

        public virtual bool OnKeyPress(Keys keyData)
        {
            int columnNumber = this.dgTable.DataGrid.CurrentCell.ColumnNumber;
            GridColumnStylesCollection gridColumnStyles = this.dgTable.GridColumnStyles;
            if (((gridColumnStyles != null) && (columnNumber >= 0)) && (columnNumber < gridColumnStyles.Count))
            {
                DataGridColumnStyle style = gridColumnStyles[columnNumber];
                if (style.KeyPress(this.RowNumber, keyData))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool OnMouseDown(int x, int y, Rectangle rowHeaders)
        {
            return this.OnMouseDown(x, y, rowHeaders, false);
        }

        public virtual bool OnMouseDown(int x, int y, Rectangle rowHeaders, bool alignToRight)
        {
            this.LoseChildFocus(rowHeaders, alignToRight);
            return false;
        }

        public virtual void OnMouseLeft()
        {
        }

        public virtual void OnMouseLeft(Rectangle rowHeaders, bool alignToRight)
        {
        }

        public virtual bool OnMouseMove(int x, int y, Rectangle rowHeaders)
        {
            return false;
        }

        public virtual bool OnMouseMove(int x, int y, Rectangle rowHeaders, bool alignToRight)
        {
            return false;
        }

        public virtual void OnRowEnter()
        {
        }

        public virtual void OnRowLeave()
        {
        }

        public abstract int Paint(Graphics g, Rectangle dataBounds, Rectangle rowBounds, int firstVisibleColumn, int numVisibleColumns);
        public abstract int Paint(Graphics g, Rectangle dataBounds, Rectangle rowBounds, int firstVisibleColumn, int numVisibleColumns, bool alignToRight);
        protected virtual void PaintBottomBorder(Graphics g, Rectangle bounds, int dataWidth)
        {
            this.PaintBottomBorder(g, bounds, dataWidth, this.dgTable.GridLineWidth, false);
        }

        protected virtual void PaintBottomBorder(Graphics g, Rectangle bounds, int dataWidth, int borderWidth, bool alignToRight)
        {
            Rectangle rect = new Rectangle(alignToRight ? (bounds.Right - dataWidth) : bounds.X, bounds.Bottom - borderWidth, dataWidth, borderWidth);
            g.FillRectangle(this.dgTable.IsDefault ? this.DataGrid.GridLineBrush : this.dgTable.GridLineBrush, rect);
            if (dataWidth < bounds.Width)
            {
                g.FillRectangle(this.dgTable.DataGrid.BackgroundBrush, alignToRight ? bounds.X : rect.Right, rect.Y, bounds.Width - rect.Width, borderWidth);
            }
        }

        protected virtual void PaintCellContents(Graphics g, Rectangle cellBounds, DataGridColumnStyle column, Brush backBr, Brush foreBrush)
        {
            this.PaintCellContents(g, cellBounds, column, backBr, foreBrush, false);
        }

        protected virtual void PaintCellContents(Graphics g, Rectangle cellBounds, DataGridColumnStyle column, Brush backBr, Brush foreBrush, bool alignToRight)
        {
            g.FillRectangle(backBr, cellBounds);
        }

        public virtual int PaintData(Graphics g, Rectangle bounds, int firstVisibleColumn, int columnCount)
        {
            return this.PaintData(g, bounds, firstVisibleColumn, columnCount, false);
        }

        public virtual int PaintData(Graphics g, Rectangle bounds, int firstVisibleColumn, int columnCount, bool alignToRight)
        {
            Rectangle cellBounds = bounds;
            int width = this.dgTable.IsDefault ? this.DataGrid.GridLineWidth : this.dgTable.GridLineWidth;
            int num2 = 0;
            DataGridCell currentCell = this.dgTable.DataGrid.CurrentCell;
            GridColumnStylesCollection gridColumnStyles = this.dgTable.GridColumnStyles;
            int count = gridColumnStyles.Count;
            for (int i = firstVisibleColumn; i < count; i++)
            {
                if (num2 > bounds.Width)
                {
                    break;
                }
                if ((gridColumnStyles[i].PropertyDescriptor != null) && (gridColumnStyles[i].Width > 0))
                {
                    cellBounds.Width = gridColumnStyles[i].Width - width;
                    if (alignToRight)
                    {
                        cellBounds.X = (bounds.Right - num2) - cellBounds.Width;
                    }
                    else
                    {
                        cellBounds.X = bounds.X + num2;
                    }
                    Brush backBr = this.BackBrushForDataPaint(ref currentCell, gridColumnStyles[i], i);
                    Brush foreBrush = this.ForeBrushForDataPaint(ref currentCell, gridColumnStyles[i], i);
                    this.PaintCellContents(g, cellBounds, gridColumnStyles[i], backBr, foreBrush, alignToRight);
                    if (width > 0)
                    {
                        g.FillRectangle(this.dgTable.IsDefault ? this.DataGrid.GridLineBrush : this.dgTable.GridLineBrush, alignToRight ? (cellBounds.X - width) : cellBounds.Right, cellBounds.Y, width, cellBounds.Height);
                    }
                    num2 += cellBounds.Width + width;
                }
            }
            if (num2 < bounds.Width)
            {
                g.FillRectangle(this.dgTable.DataGrid.BackgroundBrush, alignToRight ? bounds.X : (bounds.X + num2), bounds.Y, bounds.Width - num2, bounds.Height);
            }
            return num2;
        }

        public virtual void PaintHeader(Graphics g, Rectangle visualBounds)
        {
            this.PaintHeader(g, visualBounds, false);
        }

        public virtual void PaintHeader(Graphics g, Rectangle visualBounds, bool alignToRight)
        {
            this.PaintHeader(g, visualBounds, alignToRight, false);
        }

        public virtual void PaintHeader(Graphics g, Rectangle visualBounds, bool alignToRight, bool rowIsDirty)
        {
            Bitmap starBitmap;
            object obj2;
            Rectangle rectangle = visualBounds;
            if (this is DataGridAddNewRow)
            {
                starBitmap = this.GetStarBitmap();
                lock (starBitmap)
                {
                    rectangle.X += this.PaintIcon(g, rectangle, true, alignToRight, starBitmap).Width + 3;
                }
                return;
            }
            if (rowIsDirty)
            {
                starBitmap = this.GetPencilBitmap();
                lock (starBitmap)
                {
                    rectangle.X += this.PaintIcon(g, rectangle, this.RowNumber == this.DataGrid.CurrentCell.RowNumber, alignToRight, starBitmap).Width + 3;
                    goto Label_0121;
                }
            }
            starBitmap = alignToRight ? this.GetLeftArrowBitmap() : this.GetRightArrowBitmap();
            lock (starBitmap)
            {
                rectangle.X += this.PaintIcon(g, rectangle, this.RowNumber == this.DataGrid.CurrentCell.RowNumber, alignToRight, starBitmap).Width + 3;
            }
        Label_0121:
            obj2 = this.DataGrid.ListManager[this.number];
            if (obj2 is IDataErrorInfo)
            {
                string error = ((IDataErrorInfo) obj2).Error;
                if (error == null)
                {
                    error = string.Empty;
                }
                if ((this.tooltip != error) && !string.IsNullOrEmpty(this.tooltip))
                {
                    this.DataGrid.ToolTipProvider.RemoveToolTip(this.tooltipID);
                    this.tooltip = string.Empty;
                    this.tooltipID = new IntPtr(-1);
                }
                if (!string.IsNullOrEmpty(error))
                {
                    Rectangle rectangle2;
                    int num;
                    starBitmap = this.GetErrorBitmap();
                    lock (starBitmap)
                    {
                        rectangle2 = this.PaintIcon(g, rectangle, true, alignToRight, starBitmap);
                    }
                    rectangle.X += rectangle2.Width + 3;
                    this.tooltip = error;
                    System.Windows.Forms.DataGrid dataGrid = this.DataGrid;
                    dataGrid.ToolTipId = (num = dataGrid.ToolTipId) + 1;
                    this.tooltipID = (IntPtr) num;
                    this.DataGrid.ToolTipProvider.AddToolTip(this.tooltip, this.tooltipID, rectangle2);
                }
            }
        }

        protected Rectangle PaintIcon(Graphics g, Rectangle visualBounds, bool paintIcon, bool alignToRight, Bitmap bmp)
        {
            return this.PaintIcon(g, visualBounds, paintIcon, alignToRight, bmp, this.dgTable.IsDefault ? this.DataGrid.HeaderBackBrush : this.dgTable.HeaderBackBrush);
        }

        protected Rectangle PaintIcon(Graphics g, Rectangle visualBounds, bool paintIcon, bool alignToRight, Bitmap bmp, Brush backBrush)
        {
            Size size = bmp.Size;
            Rectangle destRect = new Rectangle(alignToRight ? ((visualBounds.Right - 3) - size.Width) : (visualBounds.X + 3), visualBounds.Y + 2, size.Width, size.Height);
            g.FillRectangle(backBrush, visualBounds);
            if (paintIcon)
            {
                colorMap[0].NewColor = this.dgTable.IsDefault ? this.DataGrid.HeaderForeColor : this.dgTable.HeaderForeColor;
                colorMap[0].OldColor = Color.Black;
                ImageAttributes imageAttr = new ImageAttributes();
                imageAttr.SetRemapTable(colorMap, ColorAdjustType.Bitmap);
                g.DrawImage(bmp, destRect, 0, 0, destRect.Width, destRect.Height, GraphicsUnit.Pixel, imageAttr);
                imageAttr.Dispose();
            }
            return destRect;
        }

        internal abstract bool ProcessTabKey(Keys keyData, Rectangle rowHeaders, bool alignToRight);

        public System.Windows.Forms.AccessibleObject AccessibleObject
        {
            get
            {
                if (this.accessibleObject == null)
                {
                    this.accessibleObject = this.CreateAccessibleObject();
                }
                return this.accessibleObject;
            }
        }

        public System.Windows.Forms.DataGrid DataGrid
        {
            get
            {
                return this.dgTable.DataGrid;
            }
        }

        internal System.Windows.Forms.DataGridTableStyle DataGridTableStyle
        {
            get
            {
                return this.dgTable;
            }
            set
            {
                this.dgTable = value;
            }
        }

        public virtual int Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = Math.Max(0, value);
                this.dgTable.DataGrid.OnRowHeightChanged(this);
            }
        }

        public int RowNumber
        {
            get
            {
                return this.number;
            }
        }

        public virtual bool Selected
        {
            get
            {
                return this.selected;
            }
            set
            {
                this.selected = value;
                this.InvalidateRow();
            }
        }

        [ComVisible(true)]
        protected class DataGridCellAccessibleObject : AccessibleObject
        {
            private int column;
            private DataGridRow owner;

            public DataGridCellAccessibleObject(DataGridRow owner, int column)
            {
                this.owner = owner;
                this.column = column;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                this.Select(AccessibleSelection.TakeSelection | AccessibleSelection.TakeFocus);
            }

            public override AccessibleObject GetFocused()
            {
                return this.DataGrid.AccessibilityObject.GetFocused();
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation navdir)
            {
                switch (navdir)
                {
                    case AccessibleNavigation.Up:
                        return this.DataGrid.AccessibilityObject.GetChild(((1 + this.owner.dgTable.GridColumnStyles.Count) + this.owner.RowNumber) - 1).Navigate(AccessibleNavigation.FirstChild);

                    case AccessibleNavigation.Down:
                        return this.DataGrid.AccessibilityObject.GetChild(((1 + this.owner.dgTable.GridColumnStyles.Count) + this.owner.RowNumber) + 1).Navigate(AccessibleNavigation.FirstChild);

                    case AccessibleNavigation.Left:
                    case AccessibleNavigation.Previous:
                        if (this.column <= 0)
                        {
                            AccessibleObject child = this.DataGrid.AccessibilityObject.GetChild(((1 + this.owner.dgTable.GridColumnStyles.Count) + this.owner.RowNumber) - 1);
                            if (child != null)
                            {
                                return child.Navigate(AccessibleNavigation.LastChild);
                            }
                            break;
                        }
                        return this.owner.AccessibleObject.GetChild(this.column - 1);

                    case AccessibleNavigation.Right:
                    case AccessibleNavigation.Next:
                        if (this.column >= (this.owner.AccessibleObject.GetChildCount() - 1))
                        {
                            AccessibleObject obj2 = this.DataGrid.AccessibilityObject.GetChild(((1 + this.owner.dgTable.GridColumnStyles.Count) + this.owner.RowNumber) + 1);
                            if (obj2 == null)
                            {
                                break;
                            }
                            return obj2.Navigate(AccessibleNavigation.FirstChild);
                        }
                        return this.owner.AccessibleObject.GetChild(this.column + 1);
                }
                return null;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void Select(AccessibleSelection flags)
            {
                if ((flags & AccessibleSelection.TakeFocus) == AccessibleSelection.TakeFocus)
                {
                    this.DataGrid.Focus();
                }
                if ((flags & AccessibleSelection.TakeSelection) == AccessibleSelection.TakeSelection)
                {
                    this.DataGrid.CurrentCell = new DataGridCell(this.owner.RowNumber, this.column);
                }
            }

            public override Rectangle Bounds
            {
                get
                {
                    return this.DataGrid.RectangleToScreen(this.DataGrid.GetCellBounds(new DataGridCell(this.owner.RowNumber, this.column)));
                }
            }

            protected System.Windows.Forms.DataGrid DataGrid
            {
                get
                {
                    return this.owner.DataGrid;
                }
            }

            public override string DefaultAction
            {
                get
                {
                    return System.Windows.Forms.SR.GetString("AccDGEdit");
                }
            }

            public override string Name
            {
                get
                {
                    return this.DataGrid.myGridTable.GridColumnStyles[this.column].HeaderText;
                }
            }

            public override AccessibleObject Parent
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return this.owner.AccessibleObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.Cell;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates states = AccessibleStates.Selectable | AccessibleStates.Focusable;
                    if ((this.DataGrid.CurrentCell.RowNumber != this.owner.RowNumber) || (this.DataGrid.CurrentCell.ColumnNumber != this.column))
                    {
                        return states;
                    }
                    if (this.DataGrid.Focused)
                    {
                        states |= AccessibleStates.Focused;
                    }
                    return (states | AccessibleStates.Selected);
                }
            }

            public override string Value
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    if (this.owner is DataGridAddNewRow)
                    {
                        return null;
                    }
                    return DataGridRow.DataGridRowAccessibleObject.CellToDisplayString(this.DataGrid, this.owner.RowNumber, this.column);
                }
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                set
                {
                    if (!(this.owner is DataGridAddNewRow))
                    {
                        object obj2 = DataGridRow.DataGridRowAccessibleObject.DisplayStringToCell(this.DataGrid, this.owner.RowNumber, this.column, value);
                        this.DataGrid[this.owner.RowNumber, this.column] = obj2;
                    }
                }
            }
        }

        [ComVisible(true)]
        protected class DataGridRowAccessibleObject : AccessibleObject
        {
            private ArrayList cells;
            private DataGridRow owner;

            public DataGridRowAccessibleObject(DataGridRow owner)
            {
                this.owner = owner;
                System.Windows.Forms.DataGrid dataGrid = this.DataGrid;
                this.EnsureChildren();
            }

            protected virtual void AddChildAccessibleObjects(IList children)
            {
                int count = this.DataGrid.myGridTable.GridColumnStyles.Count;
                for (int i = 0; i < count; i++)
                {
                    children.Add(this.CreateCellAccessibleObject(i));
                }
            }

            internal static string CellToDisplayString(System.Windows.Forms.DataGrid grid, int row, int column)
            {
                if (column < grid.myGridTable.GridColumnStyles.Count)
                {
                    return grid.myGridTable.GridColumnStyles[column].PropertyDescriptor.Converter.ConvertToString(grid[row, column]);
                }
                return "";
            }

            protected virtual AccessibleObject CreateCellAccessibleObject(int column)
            {
                return new DataGridRow.DataGridCellAccessibleObject(this.owner, column);
            }

            internal static object DisplayStringToCell(System.Windows.Forms.DataGrid grid, int row, int column, string value)
            {
                if (column < grid.myGridTable.GridColumnStyles.Count)
                {
                    return grid.myGridTable.GridColumnStyles[column].PropertyDescriptor.Converter.ConvertFromString(value);
                }
                return null;
            }

            private void EnsureChildren()
            {
                if (this.cells == null)
                {
                    this.cells = new ArrayList(this.DataGrid.myGridTable.GridColumnStyles.Count + 2);
                    this.AddChildAccessibleObjects(this.cells);
                }
            }

            public override AccessibleObject GetChild(int index)
            {
                if (index < this.cells.Count)
                {
                    return (AccessibleObject) this.cells[index];
                }
                return null;
            }

            public override int GetChildCount()
            {
                return this.cells.Count;
            }

            public override AccessibleObject GetFocused()
            {
                if (this.DataGrid.Focused)
                {
                    DataGridCell currentCell = this.DataGrid.CurrentCell;
                    if (currentCell.RowNumber == this.owner.RowNumber)
                    {
                        return (AccessibleObject) this.cells[currentCell.ColumnNumber];
                    }
                }
                return null;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation navdir)
            {
                switch (navdir)
                {
                    case AccessibleNavigation.Up:
                    case AccessibleNavigation.Left:
                    case AccessibleNavigation.Previous:
                        return this.DataGrid.AccessibilityObject.GetChild(((1 + this.owner.dgTable.GridColumnStyles.Count) + this.owner.RowNumber) - 1);

                    case AccessibleNavigation.Down:
                    case AccessibleNavigation.Right:
                    case AccessibleNavigation.Next:
                        return this.DataGrid.AccessibilityObject.GetChild(((1 + this.owner.dgTable.GridColumnStyles.Count) + this.owner.RowNumber) + 1);

                    case AccessibleNavigation.FirstChild:
                        if (this.GetChildCount() <= 0)
                        {
                            break;
                        }
                        return this.GetChild(0);

                    case AccessibleNavigation.LastChild:
                        if (this.GetChildCount() <= 0)
                        {
                            break;
                        }
                        return this.GetChild(this.GetChildCount() - 1);
                }
                return null;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void Select(AccessibleSelection flags)
            {
                if ((flags & AccessibleSelection.TakeFocus) == AccessibleSelection.TakeFocus)
                {
                    this.DataGrid.Focus();
                }
                if ((flags & AccessibleSelection.TakeSelection) == AccessibleSelection.TakeSelection)
                {
                    this.DataGrid.CurrentRowIndex = this.owner.RowNumber;
                }
            }

            public override Rectangle Bounds
            {
                get
                {
                    return this.DataGrid.RectangleToScreen(this.DataGrid.GetRowBounds(this.owner));
                }
            }

            private System.Windows.Forms.DataGrid DataGrid
            {
                get
                {
                    return this.owner.DataGrid;
                }
            }

            public override string Name
            {
                get
                {
                    if (this.owner is DataGridAddNewRow)
                    {
                        return System.Windows.Forms.SR.GetString("AccDGNewRow");
                    }
                    return CellToDisplayString(this.DataGrid, this.owner.RowNumber, 0);
                }
            }

            protected DataGridRow Owner
            {
                get
                {
                    return this.owner;
                }
            }

            public override AccessibleObject Parent
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return this.DataGrid.AccessibilityObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.Row;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates states = AccessibleStates.Selectable | AccessibleStates.Focusable;
                    if (this.DataGrid.CurrentCell.RowNumber == this.owner.RowNumber)
                    {
                        states |= AccessibleStates.Focused;
                    }
                    if (this.DataGrid.CurrentRowIndex == this.owner.RowNumber)
                    {
                        states |= AccessibleStates.Selected;
                    }
                    return states;
                }
            }

            public override string Value
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return this.Name;
                }
            }
        }
    }
}

