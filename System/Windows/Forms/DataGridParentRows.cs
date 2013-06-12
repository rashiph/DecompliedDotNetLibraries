namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    internal class DataGridParentRows
    {
        private System.Windows.Forms.AccessibleObject accessibleObject;
        private SolidBrush backBrush = DataGrid.DefaultParentRowsBackBrush;
        private Brush borderBrush = new SolidBrush(SystemColors.WindowFrame);
        private int borderWidth = 1;
        private ColorMap[] colorMap = new ColorMap[] { new ColorMap() };
        private DataGrid dataGrid;
        private bool downLeftArrow;
        private bool downRightArrow;
        private SolidBrush foreBrush = DataGrid.DefaultParentRowsForeBrush;
        private Pen gridLinePen = SystemPens.Control;
        private int horizOffset;
        private Layout layout = new Layout();
        private static Bitmap leftArrow;
        private ArrayList parents = new ArrayList();
        private int parentsCount;
        private static Bitmap rightArrow;
        private ArrayList rowHeights = new ArrayList();
        private int textRegionHeight;
        private int totalHeight;

        internal DataGridParentRows(DataGrid dataGrid)
        {
            this.colorMap[0].OldColor = Color.Black;
            this.dataGrid = dataGrid;
        }

        internal void AddParent(DataGridState dgs)
        {
            CurrencyManager manager1 = (CurrencyManager) this.dataGrid.BindingContext[dgs.DataSource, dgs.DataMember];
            this.parents.Add(dgs);
            this.SetParentCount(this.parentsCount + 1);
        }

        private int CellCount()
        {
            int num = 0;
            num = this.ColsCount();
            if ((this.dataGrid.ParentRowsLabelStyle == DataGridParentRowsLabelStyle.TableName) || (this.dataGrid.ParentRowsLabelStyle == DataGridParentRowsLabelStyle.Both))
            {
                num++;
            }
            return num;
        }

        internal void CheckNull(object value, string propName)
        {
            if (value == null)
            {
                throw new ArgumentNullException("propName");
            }
        }

        internal void Clear()
        {
            for (int i = 0; i < this.parents.Count; i++)
            {
                (this.parents[i] as DataGridState).RemoveChangeNotification();
            }
            this.parents.Clear();
            this.rowHeights.Clear();
            this.totalHeight = 0;
            this.SetParentCount(0);
        }

        private int ColsCount()
        {
            int num = 0;
            for (int i = 0; i < this.parentsCount; i++)
            {
                DataGridState state = (DataGridState) this.parents[i];
                num = Math.Max(num, state.GridColumnStyles.Count);
            }
            return num;
        }

        private void ComputeLayout(Rectangle bounds, int tableNameBoxWidth, int[] colsNameWidths, int[] colsDataWidths)
        {
            if (this.TotalWidth(tableNameBoxWidth, colsNameWidths, colsDataWidths) > bounds.Width)
            {
                this.layout.leftArrow = new Rectangle(bounds.X, bounds.Y, 15, bounds.Height);
                this.layout.data = new Rectangle(this.layout.leftArrow.Right, bounds.Y, bounds.Width - 30, bounds.Height);
                this.layout.rightArrow = new Rectangle(this.layout.data.Right, bounds.Y, 15, bounds.Height);
            }
            else
            {
                this.layout.data = bounds;
                this.layout.leftArrow = Rectangle.Empty;
                this.layout.rightArrow = Rectangle.Empty;
            }
        }

        internal void Dispose()
        {
            this.gridLinePen.Dispose();
        }

        private Bitmap GetBitmap(string bitmapName, Color transparentColor)
        {
            Bitmap bitmap = null;
            try
            {
                bitmap = new Bitmap(typeof(DataGridParentRows), bitmapName);
                bitmap.MakeTransparent(transparentColor);
            }
            catch (Exception)
            {
            }
            return bitmap;
        }

        internal Rectangle GetBoundsForDataGridStateAccesibility(DataGridState dgs)
        {
            Rectangle empty = Rectangle.Empty;
            int num = 0;
            for (int i = 0; i < this.parentsCount; i++)
            {
                int num3 = (int) this.rowHeights[i];
                if (this.parents[i] == dgs)
                {
                    empty.X = this.layout.leftArrow.IsEmpty ? this.layout.data.X : this.layout.leftArrow.Right;
                    empty.Height = num3;
                    empty.Y = num;
                    empty.Width = this.layout.data.Width;
                    return empty;
                }
                num += num3;
            }
            return empty;
        }

        private int GetColBoxWidth(Graphics g, Font font, int colNum)
        {
            int num = 0;
            for (int i = 0; i < this.parentsCount; i++)
            {
                DataGridState state = (DataGridState) this.parents[i];
                GridColumnStylesCollection gridColumnStyles = state.GridColumnStyles;
                if (colNum < gridColumnStyles.Count)
                {
                    string text = gridColumnStyles[colNum].HeaderText + " :";
                    int width = (int) g.MeasureString(text, font).Width;
                    num = Math.Max(width, num);
                }
            }
            return num;
        }

        private int GetColDataBoxWidth(Graphics g, int colNum)
        {
            int num = 0;
            for (int i = 0; i < this.parentsCount; i++)
            {
                DataGridState state = (DataGridState) this.parents[i];
                GridColumnStylesCollection gridColumnStyles = state.GridColumnStyles;
                if (colNum < gridColumnStyles.Count)
                {
                    object columnValueAtRow = gridColumnStyles[colNum].GetColumnValueAtRow((CurrencyManager) this.dataGrid.BindingContext[state.DataSource, state.DataMember], state.LinkingRow.RowNumber);
                    num = Math.Max(gridColumnStyles[colNum].GetPreferredSize(g, columnValueAtRow).Width, num);
                }
            }
            return num;
        }

        private Bitmap GetLeftArrowBitmap()
        {
            if (leftArrow == null)
            {
                leftArrow = this.GetBitmap("DataGridParentRows.LeftArrow.bmp", Color.White);
            }
            return leftArrow;
        }

        private Bitmap GetRightArrowBitmap()
        {
            if (rightArrow == null)
            {
                rightArrow = this.GetBitmap("DataGridParentRows.RightArrow.bmp", Color.White);
            }
            return rightArrow;
        }

        private int GetTableBoxWidth(Graphics g, Font font)
        {
            Font font2 = font;
            try
            {
                font2 = new Font(font, FontStyle.Bold);
            }
            catch
            {
            }
            int num = 0;
            for (int i = 0; i < this.parentsCount; i++)
            {
                DataGridState state = (DataGridState) this.parents[i];
                string text = state.ListManager.GetListName() + " :";
                int width = (int) g.MeasureString(text, font2).Width;
                num = Math.Max(width, num);
            }
            return num;
        }

        internal DataGridState GetTopParent()
        {
            if (this.parentsCount < 1)
            {
                return null;
            }
            return (DataGridState) ((ICloneable) this.parents[this.parentsCount - 1]).Clone();
        }

        internal void Invalidate()
        {
            if (this.dataGrid != null)
            {
                this.dataGrid.InvalidateParentRows();
            }
        }

        internal void InvalidateRect(Rectangle rect)
        {
            if (this.dataGrid != null)
            {
                Rectangle r = new Rectangle(rect.X, rect.Y, rect.Width + this.borderWidth, rect.Height + this.borderWidth);
                this.dataGrid.InvalidateParentRowsRect(r);
            }
        }

        internal bool IsEmpty()
        {
            return (this.parentsCount == 0);
        }

        private void LeftArrowClick(int cellCount)
        {
            if (this.horizOffset > 0)
            {
                this.ResetMouseInfo();
                this.horizOffset--;
                this.Invalidate();
            }
            else
            {
                this.ResetMouseInfo();
                this.InvalidateRect(this.layout.leftArrow);
            }
        }

        private int MirrorRect(Rectangle surroundingRect, Rectangle containedRect, bool alignToRight)
        {
            if (alignToRight)
            {
                return ((surroundingRect.Right - containedRect.Right) + surroundingRect.X);
            }
            return containedRect.X;
        }

        internal void OnLayout()
        {
            if (this.parentsCount != this.rowHeights.Count)
            {
                int num = 0;
                if (this.totalHeight == 0)
                {
                    this.totalHeight += 2 * this.borderWidth;
                }
                this.textRegionHeight = this.dataGrid.Font.Height + 2;
                if (this.parentsCount > this.rowHeights.Count)
                {
                    for (int i = this.rowHeights.Count; i < this.parentsCount; i++)
                    {
                        DataGridState state = (DataGridState) this.parents[i];
                        GridColumnStylesCollection gridColumnStyles = state.GridColumnStyles;
                        int num4 = 0;
                        for (int j = 0; j < gridColumnStyles.Count; j++)
                        {
                            num4 = Math.Max(num4, gridColumnStyles[j].GetMinimumHeight());
                        }
                        num = Math.Max(num4, this.textRegionHeight) + 1;
                        this.rowHeights.Add(num);
                        this.totalHeight += num;
                    }
                }
                else
                {
                    if (this.parentsCount == 0)
                    {
                        this.totalHeight = 0;
                    }
                    else
                    {
                        this.totalHeight -= (int) this.rowHeights[this.rowHeights.Count - 1];
                    }
                    this.rowHeights.RemoveAt(this.rowHeights.Count - 1);
                }
            }
        }

        internal void OnMouseDown(int x, int y, bool alignToRight)
        {
            if (!this.layout.rightArrow.IsEmpty)
            {
                int cellCount = this.CellCount();
                if (this.layout.rightArrow.Contains(x, y))
                {
                    this.downRightArrow = true;
                    if (alignToRight)
                    {
                        this.LeftArrowClick(cellCount);
                    }
                    else
                    {
                        this.RightArrowClick(cellCount);
                    }
                }
                else if (this.layout.leftArrow.Contains(x, y))
                {
                    this.downLeftArrow = true;
                    if (alignToRight)
                    {
                        this.RightArrowClick(cellCount);
                    }
                    else
                    {
                        this.LeftArrowClick(cellCount);
                    }
                }
                else
                {
                    if (this.downLeftArrow)
                    {
                        this.downLeftArrow = false;
                        this.InvalidateRect(this.layout.leftArrow);
                    }
                    if (this.downRightArrow)
                    {
                        this.downRightArrow = false;
                        this.InvalidateRect(this.layout.rightArrow);
                    }
                }
            }
        }

        internal void OnMouseLeave()
        {
            if (this.downLeftArrow)
            {
                this.downLeftArrow = false;
                this.InvalidateRect(this.layout.leftArrow);
            }
            if (this.downRightArrow)
            {
                this.downRightArrow = false;
                this.InvalidateRect(this.layout.rightArrow);
            }
        }

        internal void OnMouseMove(int x, int y)
        {
            if (this.downLeftArrow)
            {
                this.downLeftArrow = false;
                this.InvalidateRect(this.layout.leftArrow);
            }
            if (this.downRightArrow)
            {
                this.downRightArrow = false;
                this.InvalidateRect(this.layout.rightArrow);
            }
        }

        internal void OnMouseUp(int x, int y)
        {
            this.ResetMouseInfo();
            if (!this.layout.rightArrow.IsEmpty && this.layout.rightArrow.Contains(x, y))
            {
                this.InvalidateRect(this.layout.rightArrow);
            }
            else if (!this.layout.leftArrow.IsEmpty && this.layout.leftArrow.Contains(x, y))
            {
                this.InvalidateRect(this.layout.leftArrow);
            }
        }

        internal void OnResize(Rectangle oldBounds)
        {
            this.Invalidate();
        }

        internal void Paint(Graphics g, Rectangle visualbounds, bool alignRight)
        {
            Rectangle bounds = visualbounds;
            if (this.borderWidth > 0)
            {
                this.PaintBorder(g, bounds);
                bounds.Inflate(-this.borderWidth, -this.borderWidth);
            }
            this.PaintParentRows(g, bounds, alignRight);
        }

        private void PaintBitmap(Graphics g, Bitmap b, Rectangle bounds)
        {
            int x = bounds.X + ((bounds.Width - b.Width) / 2);
            int y = bounds.Y + ((bounds.Height - b.Height) / 2);
            Rectangle rect = new Rectangle(x, y, b.Width, b.Height);
            g.FillRectangle(this.BackBrush, rect);
            ImageAttributes imageAttr = new ImageAttributes();
            this.colorMap[0].NewColor = this.ForeColor;
            imageAttr.SetRemapTable(this.colorMap, ColorAdjustType.Bitmap);
            g.DrawImage(b, rect, 0, 0, rect.Width, rect.Height, GraphicsUnit.Pixel, imageAttr);
            imageAttr.Dispose();
        }

        private void PaintBorder(Graphics g, Rectangle bounds)
        {
            Rectangle rect = bounds;
            rect.Height = this.borderWidth;
            g.FillRectangle(this.borderBrush, rect);
            rect.Y = bounds.Bottom - this.borderWidth;
            g.FillRectangle(this.borderBrush, rect);
            rect = new Rectangle(bounds.X, bounds.Y + this.borderWidth, this.borderWidth, bounds.Height - (2 * this.borderWidth));
            g.FillRectangle(this.borderBrush, rect);
            rect.X = bounds.Right - this.borderWidth;
            g.FillRectangle(this.borderBrush, rect);
        }

        private int PaintColumns(Graphics g, Rectangle bounds, DataGridState dgs, Font font, bool alignToRight, int[] colsNameWidths, int[] colsDataWidths, int skippedCells)
        {
            Rectangle containedRect = bounds;
            GridColumnStylesCollection gridColumnStyles = dgs.GridColumnStyles;
            int num = 0;
            for (int i = 0; i < gridColumnStyles.Count; i++)
            {
                if (num >= bounds.Width)
                {
                    return num;
                }
                if (((this.dataGrid.ParentRowsLabelStyle == DataGridParentRowsLabelStyle.ColumnName) || (this.dataGrid.ParentRowsLabelStyle == DataGridParentRowsLabelStyle.Both)) && (skippedCells >= this.horizOffset))
                {
                    containedRect.X = bounds.X + num;
                    containedRect.Width = Math.Min(bounds.Width - num, colsNameWidths[i]);
                    containedRect.X = this.MirrorRect(bounds, containedRect, alignToRight);
                    string text = gridColumnStyles[i].HeaderText + ": ";
                    this.PaintText(g, containedRect, text, font, false, alignToRight);
                    num += containedRect.Width;
                }
                if (num >= bounds.Width)
                {
                    return num;
                }
                if (skippedCells < this.horizOffset)
                {
                    skippedCells++;
                }
                else
                {
                    containedRect.X = bounds.X + num;
                    containedRect.Width = Math.Min(bounds.Width - num, colsDataWidths[i]);
                    containedRect.X = this.MirrorRect(bounds, containedRect, alignToRight);
                    gridColumnStyles[i].Paint(g, containedRect, (CurrencyManager) this.dataGrid.BindingContext[dgs.DataSource, dgs.DataMember], this.dataGrid.BindingContext[dgs.DataSource, dgs.DataMember].Position, this.BackBrush, this.ForeBrush, alignToRight);
                    num += containedRect.Width;
                    g.DrawLine(new Pen(SystemColors.ControlDark), alignToRight ? containedRect.X : containedRect.Right, containedRect.Y, alignToRight ? containedRect.X : containedRect.Right, containedRect.Bottom);
                    num++;
                    if (i < (gridColumnStyles.Count - 1))
                    {
                        containedRect.X = bounds.X + num;
                        containedRect.Width = Math.Min(bounds.Width - num, 3);
                        containedRect.X = this.MirrorRect(bounds, containedRect, alignToRight);
                        g.FillRectangle(this.BackBrush, containedRect);
                        num += 3;
                    }
                }
            }
            return num;
        }

        private void PaintDownButton(Graphics g, Rectangle bounds)
        {
            g.DrawLine(Pens.Black, bounds.X, bounds.Y, bounds.X + bounds.Width, bounds.Y);
            g.DrawLine(Pens.White, bounds.X + bounds.Width, bounds.Y, bounds.X + bounds.Width, bounds.Y + bounds.Height);
            g.DrawLine(Pens.White, bounds.X + bounds.Width, bounds.Y + bounds.Height, bounds.X, bounds.Y + bounds.Height);
            g.DrawLine(Pens.Black, bounds.X, bounds.Y + bounds.Height, bounds.X, bounds.Y);
        }

        private void PaintLeftArrow(Graphics g, Rectangle bounds, bool alignToRight)
        {
            Bitmap leftArrowBitmap = this.GetLeftArrowBitmap();
            if (this.downLeftArrow)
            {
                this.PaintDownButton(g, bounds);
                this.layout.leftArrow.Inflate(-1, -1);
                lock (leftArrowBitmap)
                {
                    this.PaintBitmap(g, leftArrowBitmap, bounds);
                }
                this.layout.leftArrow.Inflate(1, 1);
            }
            else
            {
                lock (leftArrowBitmap)
                {
                    this.PaintBitmap(g, leftArrowBitmap, bounds);
                }
            }
        }

        private void PaintParentRows(Graphics g, Rectangle bounds, bool alignToRight)
        {
            int tableNameBoxWidth = 0;
            int num2 = this.ColsCount();
            int[] colsNameWidths = new int[num2];
            int[] colsDataWidths = new int[num2];
            if ((this.dataGrid.ParentRowsLabelStyle == DataGridParentRowsLabelStyle.TableName) || (this.dataGrid.ParentRowsLabelStyle == DataGridParentRowsLabelStyle.Both))
            {
                tableNameBoxWidth = this.GetTableBoxWidth(g, this.dataGrid.Font);
            }
            for (int i = 0; i < num2; i++)
            {
                if ((this.dataGrid.ParentRowsLabelStyle == DataGridParentRowsLabelStyle.ColumnName) || (this.dataGrid.ParentRowsLabelStyle == DataGridParentRowsLabelStyle.Both))
                {
                    colsNameWidths[i] = this.GetColBoxWidth(g, this.dataGrid.Font, i);
                }
                else
                {
                    colsNameWidths[i] = 0;
                }
                colsDataWidths[i] = this.GetColDataBoxWidth(g, i);
            }
            this.ComputeLayout(bounds, tableNameBoxWidth, colsNameWidths, colsDataWidths);
            if (!this.layout.leftArrow.IsEmpty)
            {
                g.FillRectangle(this.BackBrush, this.layout.leftArrow);
                this.PaintLeftArrow(g, this.layout.leftArrow, alignToRight);
            }
            Rectangle data = this.layout.data;
            for (int j = 0; j < this.parentsCount; j++)
            {
                data.Height = (int) this.rowHeights[j];
                if (data.Y > bounds.Bottom)
                {
                    break;
                }
                int num5 = this.PaintRow(g, data, j, this.dataGrid.Font, alignToRight, tableNameBoxWidth, colsNameWidths, colsDataWidths);
                if (j == (this.parentsCount - 1))
                {
                    break;
                }
                g.DrawLine(this.gridLinePen, data.X, data.Bottom, data.X + num5, data.Bottom);
                data.Y += data.Height;
            }
            if (!this.layout.rightArrow.IsEmpty)
            {
                g.FillRectangle(this.BackBrush, this.layout.rightArrow);
                this.PaintRightArrow(g, this.layout.rightArrow, alignToRight);
            }
        }

        private void PaintRightArrow(Graphics g, Rectangle bounds, bool alignToRight)
        {
            Bitmap rightArrowBitmap = this.GetRightArrowBitmap();
            if (this.downRightArrow)
            {
                this.PaintDownButton(g, bounds);
                this.layout.rightArrow.Inflate(-1, -1);
                lock (rightArrowBitmap)
                {
                    this.PaintBitmap(g, rightArrowBitmap, bounds);
                }
                this.layout.rightArrow.Inflate(1, 1);
            }
            else
            {
                lock (rightArrowBitmap)
                {
                    this.PaintBitmap(g, rightArrowBitmap, bounds);
                }
            }
        }

        private int PaintRow(Graphics g, Rectangle bounds, int row, Font font, bool alignToRight, int tableNameBoxWidth, int[] colsNameWidths, int[] colsDataWidths)
        {
            DataGridState dgs = (DataGridState) this.parents[row];
            Rectangle containedRect = bounds;
            Rectangle rectangle2 = bounds;
            containedRect.Height = (int) this.rowHeights[row];
            rectangle2.Height = (int) this.rowHeights[row];
            int num = 0;
            int skippedCells = 0;
            if ((this.dataGrid.ParentRowsLabelStyle == DataGridParentRowsLabelStyle.TableName) || (this.dataGrid.ParentRowsLabelStyle == DataGridParentRowsLabelStyle.Both))
            {
                if (skippedCells < this.horizOffset)
                {
                    skippedCells++;
                }
                else
                {
                    containedRect.Width = Math.Min(containedRect.Width, tableNameBoxWidth);
                    containedRect.X = this.MirrorRect(bounds, containedRect, alignToRight);
                    string text = dgs.ListManager.GetListName() + ": ";
                    this.PaintText(g, containedRect, text, font, true, alignToRight);
                    num += containedRect.Width;
                }
            }
            if (num >= bounds.Width)
            {
                return bounds.Width;
            }
            rectangle2.Width -= num;
            rectangle2.X += alignToRight ? 0 : num;
            num += this.PaintColumns(g, rectangle2, dgs, font, alignToRight, colsNameWidths, colsDataWidths, skippedCells);
            if (num < bounds.Width)
            {
                containedRect.X = bounds.X + num;
                containedRect.Width = bounds.Width - num;
                containedRect.X = this.MirrorRect(bounds, containedRect, alignToRight);
                g.FillRectangle(this.BackBrush, containedRect);
            }
            return num;
        }

        private int PaintText(Graphics g, Rectangle textBounds, string text, Font font, bool bold, bool alignToRight)
        {
            Font font2 = font;
            if (bold)
            {
                try
                {
                    font2 = new Font(font, FontStyle.Bold);
                }
                catch
                {
                }
            }
            else
            {
                font2 = font;
            }
            g.FillRectangle(this.BackBrush, textBounds);
            StringFormat format = new StringFormat();
            if (alignToRight)
            {
                format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                format.Alignment = StringAlignment.Far;
            }
            format.FormatFlags |= StringFormatFlags.NoWrap;
            textBounds.Offset(0, 2);
            textBounds.Height -= 2;
            g.DrawString(text, font2, this.ForeBrush, textBounds, format);
            format.Dispose();
            return textBounds.Width;
        }

        internal DataGridState PopTop()
        {
            if (this.parentsCount < 1)
            {
                return null;
            }
            this.SetParentCount(this.parentsCount - 1);
            DataGridState state = (DataGridState) this.parents[this.parentsCount];
            state.RemoveChangeNotification();
            this.parents.RemoveAt(this.parentsCount);
            return state;
        }

        private void ResetMouseInfo()
        {
            this.downLeftArrow = false;
            this.downRightArrow = false;
        }

        private void RightArrowClick(int cellCount)
        {
            if (this.horizOffset < (cellCount - 1))
            {
                this.ResetMouseInfo();
                this.horizOffset++;
                this.Invalidate();
            }
            else
            {
                this.ResetMouseInfo();
                this.InvalidateRect(this.layout.rightArrow);
            }
        }

        internal void SetParentCount(int count)
        {
            this.parentsCount = count;
            this.dataGrid.Caption.BackButtonVisible = (this.parentsCount > 0) && this.dataGrid.AllowNavigation;
        }

        private int TotalWidth(int tableNameBoxWidth, int[] colsNameWidths, int[] colsDataWidths)
        {
            int num = 0;
            num += tableNameBoxWidth;
            for (int i = 0; i < colsNameWidths.Length; i++)
            {
                num += colsNameWidths[i];
                num += colsDataWidths[i];
            }
            return (num + (3 * (colsNameWidths.Length - 1)));
        }

        public System.Windows.Forms.AccessibleObject AccessibleObject
        {
            get
            {
                if (this.accessibleObject == null)
                {
                    this.accessibleObject = new DataGridParentRowsAccessibleObject(this);
                }
                return this.accessibleObject;
            }
        }

        internal SolidBrush BackBrush
        {
            get
            {
                return this.backBrush;
            }
            set
            {
                if (value != this.backBrush)
                {
                    this.CheckNull(value, "BackBrush");
                    this.backBrush = value;
                    this.Invalidate();
                }
            }
        }

        internal Color BackColor
        {
            get
            {
                return this.backBrush.Color;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "Parent Rows BackColor" }));
                }
                if (value != this.backBrush.Color)
                {
                    this.backBrush = new SolidBrush(value);
                    this.Invalidate();
                }
            }
        }

        internal Brush BorderBrush
        {
            get
            {
                return this.borderBrush;
            }
            set
            {
                if (value != this.borderBrush)
                {
                    this.borderBrush = value;
                    this.Invalidate();
                }
            }
        }

        internal SolidBrush ForeBrush
        {
            get
            {
                return this.foreBrush;
            }
            set
            {
                if (value != this.foreBrush)
                {
                    this.CheckNull(value, "BackBrush");
                    this.foreBrush = value;
                    this.Invalidate();
                }
            }
        }

        internal Color ForeColor
        {
            get
            {
                return this.foreBrush.Color;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "Parent Rows ForeColor" }));
                }
                if (value != this.foreBrush.Color)
                {
                    this.foreBrush = new SolidBrush(value);
                    this.Invalidate();
                }
            }
        }

        internal int Height
        {
            get
            {
                return this.totalHeight;
            }
        }

        internal bool Visible
        {
            get
            {
                return this.dataGrid.ParentRowsVisible;
            }
            set
            {
                this.dataGrid.ParentRowsVisible = value;
            }
        }

        [ComVisible(true)]
        internal protected class DataGridParentRowsAccessibleObject : AccessibleObject
        {
            private DataGridParentRows owner;

            public DataGridParentRowsAccessibleObject(DataGridParentRows owner)
            {
                this.owner = owner;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                this.owner.dataGrid.NavigateBack();
            }

            public override AccessibleObject GetChild(int index)
            {
                return ((DataGridState) this.owner.parents[index]).ParentRowAccessibleObject;
            }

            public override int GetChildCount()
            {
                return this.owner.parentsCount;
            }

            public override AccessibleObject GetFocused()
            {
                return null;
            }

            internal AccessibleObject GetNext(AccessibleObject child)
            {
                int childCount = this.GetChildCount();
                bool flag = false;
                for (int i = 0; i < childCount; i++)
                {
                    if (flag)
                    {
                        return this.GetChild(i);
                    }
                    if (this.GetChild(i) == child)
                    {
                        flag = true;
                    }
                }
                return null;
            }

            internal AccessibleObject GetPrev(AccessibleObject child)
            {
                int childCount = this.GetChildCount();
                bool flag = false;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    if (flag)
                    {
                        return this.GetChild(i);
                    }
                    if (this.GetChild(i) == child)
                    {
                        flag = true;
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
                        return this.Parent.GetChild(this.GetChildCount() - 1);

                    case AccessibleNavigation.Down:
                    case AccessibleNavigation.Right:
                    case AccessibleNavigation.Next:
                        return this.Parent.GetChild(1);

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
            }

            public override Rectangle Bounds
            {
                get
                {
                    return this.owner.dataGrid.RectangleToScreen(this.owner.dataGrid.ParentRowsBounds);
                }
            }

            public override string DefaultAction
            {
                get
                {
                    return System.Windows.Forms.SR.GetString("AccDGNavigateBack");
                }
            }

            public override string Name
            {
                get
                {
                    return System.Windows.Forms.SR.GetString("AccDGParentRows");
                }
            }

            internal DataGridParentRows Owner
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
                    return this.owner.dataGrid.AccessibilityObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.List;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates readOnly = AccessibleStates.ReadOnly;
                    if (this.owner.parentsCount == 0)
                    {
                        readOnly |= AccessibleStates.Invisible;
                    }
                    if (this.owner.dataGrid.ParentRowsVisible)
                    {
                        return (readOnly | AccessibleStates.Expanded);
                    }
                    return (readOnly | AccessibleStates.Collapsed);
                }
            }

            public override string Value
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return null;
                }
            }
        }

        private class Layout
        {
            public Rectangle data = Rectangle.Empty;
            public Rectangle leftArrow = Rectangle.Empty;
            public Rectangle rightArrow = Rectangle.Empty;

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder(200);
                builder.Append("ParentRows Layout: \n");
                builder.Append("data = ");
                builder.Append(this.data.ToString());
                builder.Append("\n leftArrow = ");
                builder.Append(this.leftArrow.ToString());
                builder.Append("\n rightArrow = ");
                builder.Append(this.rightArrow.ToString());
                builder.Append("\n");
                return builder.ToString();
            }
        }
    }
}

