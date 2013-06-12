namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    internal class DataGridRelationshipRow : DataGridRow
    {
        private const bool defaultOpen = false;
        private bool expanded;
        private const int expandoBoxWidth = 14;
        private const int indentWidth = 20;
        private const int triangleSize = 5;

        public DataGridRelationshipRow(DataGrid dataGrid, DataGridTableStyle dgTable, int rowNumber) : base(dataGrid, dgTable, rowNumber)
        {
        }

        private void Collapse()
        {
            if (this.expanded)
            {
                this.expanded = false;
                this.FocusedRelation = -1;
                base.DataGrid.OnRowHeightChanged(this);
            }
        }

        protected override AccessibleObject CreateAccessibleObject()
        {
            return new DataGridRelationshipRowAccessibleObject(this);
        }

        private void Expand()
        {
            if ((!this.expanded && (base.DataGrid != null)) && ((base.dgTable != null) && (base.dgTable.RelationsList.Count > 0)))
            {
                this.expanded = true;
                this.FocusedRelation = -1;
                base.DataGrid.OnRowHeightChanged(this);
            }
        }

        public override Rectangle GetCellBounds(int col)
        {
            Rectangle cellBounds = base.GetCellBounds(col);
            cellBounds.Height = base.Height - 1;
            return cellBounds;
        }

        public override Rectangle GetNonScrollableArea()
        {
            if (this.expanded)
            {
                return this.GetRelationshipRect();
            }
            return Rectangle.Empty;
        }

        private Rectangle GetOutlineRect(int xOrigin, int yOrigin)
        {
            return new Rectangle(xOrigin + 2, yOrigin + 2, 9, 9);
        }

        private Rectangle GetRelationshipRect()
        {
            Rectangle relationshipRect = base.dgTable.RelationshipRect;
            relationshipRect.Y = base.Height - base.dgTable.BorderWidth;
            return relationshipRect;
        }

        private Rectangle GetRelationshipRectWithMirroring()
        {
            Rectangle relationshipRect = this.GetRelationshipRect();
            if (base.dgTable.IsDefault ? base.DataGrid.RowHeadersVisible : base.dgTable.RowHeadersVisible)
            {
                int num = base.dgTable.IsDefault ? base.DataGrid.RowHeaderWidth : base.dgTable.RowHeaderWidth;
                relationshipRect.X += base.DataGrid.GetRowHeaderRect().X + num;
            }
            relationshipRect.X = this.MirrorRelationshipRectangle(relationshipRect, base.DataGrid.GetRowHeaderRect(), base.DataGrid.RightToLeft == RightToLeft.Yes);
            return relationshipRect;
        }

        internal override void LoseChildFocus(Rectangle rowHeaders, bool alignToRight)
        {
            if ((this.FocusedRelation != -1) && this.expanded)
            {
                this.FocusedRelation = -1;
                Rectangle relationshipRect = this.GetRelationshipRect();
                relationshipRect.X += rowHeaders.X + base.dgTable.RowHeaderWidth;
                relationshipRect.X = this.MirrorRelationshipRectangle(relationshipRect, rowHeaders, alignToRight);
                this.InvalidateRowRect(relationshipRect);
            }
        }

        protected internal override int MinimumRowHeight(DataGridTableStyle dgTable)
        {
            return (base.MinimumRowHeight(dgTable) + (this.expanded ? this.GetRelationshipRect().Height : 0));
        }

        protected internal override int MinimumRowHeight(GridColumnStylesCollection cols)
        {
            return (base.MinimumRowHeight(cols) + (this.expanded ? this.GetRelationshipRect().Height : 0));
        }

        private int MirrorRectangle(int x, int width, Rectangle rect, bool alignToRight)
        {
            if (alignToRight)
            {
                return (((rect.Right + rect.X) - width) - x);
            }
            return x;
        }

        private int MirrorRelationshipRectangle(Rectangle relRect, Rectangle rowHeader, bool alignToRight)
        {
            if (alignToRight)
            {
                return (rowHeader.X - relRect.Width);
            }
            return relRect.X;
        }

        public override bool OnKeyPress(Keys keyData)
        {
            if (((keyData & ~Keys.KeyCode) == Keys.Shift) && ((keyData & Keys.KeyCode) != Keys.Tab))
            {
                return false;
            }
            switch ((keyData & Keys.KeyCode))
            {
                case Keys.Tab:
                    return false;

                case Keys.Enter:
                    if (this.FocusedRelation != -1)
                    {
                        base.DataGrid.NavigateTo((string) base.dgTable.RelationsList[this.FocusedRelation], this, true);
                        this.FocusedRelation = -1;
                        return true;
                    }
                    return false;

                case Keys.F5:
                    if (((base.dgTable == null) || (base.dgTable.DataGrid == null)) || !base.dgTable.DataGrid.AllowNavigation)
                    {
                        return false;
                    }
                    if (this.expanded)
                    {
                        this.Collapse();
                    }
                    else
                    {
                        this.Expand();
                    }
                    this.FocusedRelation = -1;
                    return true;

                case Keys.NumLock:
                    if (this.FocusedRelation != -1)
                    {
                        return false;
                    }
                    return base.OnKeyPress(keyData);
            }
            this.FocusedRelation = -1;
            return base.OnKeyPress(keyData);
        }

        public override bool OnMouseDown(int x, int y, Rectangle rowHeaders, bool alignToRight)
        {
            if ((base.dgTable.IsDefault ? base.DataGrid.RowHeadersVisible : base.dgTable.RowHeadersVisible) && this.PointOverPlusMinusGlyph(x, y, rowHeaders, alignToRight))
            {
                if (base.dgTable.RelationsList.Count == 0)
                {
                    return false;
                }
                if (this.expanded)
                {
                    this.Collapse();
                }
                else
                {
                    this.Expand();
                }
                base.DataGrid.OnNodeClick(EventArgs.Empty);
                return true;
            }
            if (!this.expanded)
            {
                return base.OnMouseDown(x, y, rowHeaders, alignToRight);
            }
            if (!this.GetRelationshipRectWithMirroring().Contains(x, y))
            {
                return base.OnMouseDown(x, y, rowHeaders, alignToRight);
            }
            int num = this.RelationFromY(y);
            if (num != -1)
            {
                this.FocusedRelation = -1;
                base.DataGrid.NavigateTo((string) base.dgTable.RelationsList[num], this, true);
            }
            return true;
        }

        public override void OnMouseLeft()
        {
            if (this.expanded)
            {
                if (this.FocusedRelation != -1)
                {
                    this.InvalidateRow();
                    this.FocusedRelation = -1;
                }
                base.OnMouseLeft();
            }
        }

        public override void OnMouseLeft(Rectangle rowHeaders, bool alignToRight)
        {
            if (this.expanded)
            {
                Rectangle relationshipRect = this.GetRelationshipRect();
                relationshipRect.X += rowHeaders.X + base.dgTable.RowHeaderWidth;
                relationshipRect.X = this.MirrorRelationshipRectangle(relationshipRect, rowHeaders, alignToRight);
                if (this.FocusedRelation != -1)
                {
                    this.InvalidateRowRect(relationshipRect);
                    this.FocusedRelation = -1;
                }
            }
        }

        public override bool OnMouseMove(int x, int y, Rectangle rowHeaders, bool alignToRight)
        {
            if (!this.expanded)
            {
                return false;
            }
            if (this.GetRelationshipRectWithMirroring().Contains(x, y))
            {
                base.DataGrid.Cursor = Cursors.Hand;
                return true;
            }
            base.DataGrid.Cursor = Cursors.Default;
            return base.OnMouseMove(x, y, rowHeaders, alignToRight);
        }

        public override int Paint(Graphics g, Rectangle bounds, Rectangle trueRowBounds, int firstVisibleColumn, int numVisibleColumns)
        {
            return this.Paint(g, bounds, trueRowBounds, firstVisibleColumn, numVisibleColumns, false);
        }

        public override int Paint(Graphics g, Rectangle bounds, Rectangle trueRowBounds, int firstVisibleColumn, int numVisibleColumns, bool alignToRight)
        {
            bool traceVerbose = System.ComponentModel.CompModSwitches.DGRelationShpRowPaint.TraceVerbose;
            int borderWidth = base.dgTable.BorderWidth;
            Rectangle rectangle = bounds;
            rectangle.Height = base.Height - borderWidth;
            int dataWidth = this.PaintData(g, rectangle, firstVisibleColumn, numVisibleColumns, alignToRight);
            int num3 = (dataWidth + bounds.X) - trueRowBounds.X;
            rectangle.Offset(0, borderWidth);
            if (borderWidth > 0)
            {
                this.PaintBottomBorder(g, rectangle, dataWidth, borderWidth, alignToRight);
            }
            if (this.expanded && (base.dgTable.RelationsList.Count > 0))
            {
                Rectangle rectangle2 = new Rectangle(trueRowBounds.X, rectangle.Bottom, trueRowBounds.Width, (trueRowBounds.Height - rectangle.Height) - (2 * borderWidth));
                this.PaintRelations(g, rectangle2, trueRowBounds, num3, firstVisibleColumn, numVisibleColumns, alignToRight);
                rectangle2.Height++;
                if (borderWidth > 0)
                {
                    this.PaintBottomBorder(g, rectangle2, num3, borderWidth, alignToRight);
                }
            }
            return dataWidth;
        }

        protected override void PaintCellContents(Graphics g, Rectangle cellBounds, DataGridColumnStyle column, Brush backBr, Brush foreBrush, bool alignToRight)
        {
            CurrencyManager listManager = base.DataGrid.ListManager;
            string str = string.Empty;
            Rectangle visualBounds = cellBounds;
            object obj2 = base.DataGrid.ListManager[base.number];
            if (obj2 is IDataErrorInfo)
            {
                str = ((IDataErrorInfo) obj2)[column.PropertyDescriptor.Name];
            }
            if (!string.IsNullOrEmpty(str))
            {
                Rectangle rectangle2;
                int num;
                Bitmap errorBitmap = base.GetErrorBitmap();
                lock (errorBitmap)
                {
                    rectangle2 = base.PaintIcon(g, visualBounds, true, alignToRight, errorBitmap, backBr);
                }
                if (alignToRight)
                {
                    visualBounds.Width -= rectangle2.Width + 3;
                }
                else
                {
                    visualBounds.X += rectangle2.Width + 3;
                }
                DataGrid dataGrid = base.DataGrid;
                dataGrid.ToolTipId = (num = dataGrid.ToolTipId) + 1;
                base.DataGrid.ToolTipProvider.AddToolTip(str, (IntPtr) num, rectangle2);
            }
            column.Paint(g, visualBounds, listManager, base.RowNumber, backBr, foreBrush, alignToRight);
        }

        public override void PaintHeader(Graphics g, Rectangle bounds, bool alignToRight, bool isDirty)
        {
            DataGrid dataGrid = base.DataGrid;
            Rectangle rectangle = bounds;
            if (!dataGrid.FlatMode)
            {
                ControlPaint.DrawBorder3D(g, rectangle, Border3DStyle.RaisedInner);
                rectangle.Inflate(-1, -1);
            }
            if (base.dgTable.IsDefault)
            {
                this.PaintHeaderInside(g, rectangle, base.DataGrid.HeaderBackBrush, alignToRight, isDirty);
            }
            else
            {
                this.PaintHeaderInside(g, rectangle, base.dgTable.HeaderBackBrush, alignToRight, isDirty);
            }
        }

        public void PaintHeaderInside(Graphics g, Rectangle bounds, Brush backBr, bool alignToRight, bool isDirty)
        {
            bool flag = (base.dgTable.RelationsList.Count > 0) && base.dgTable.DataGrid.AllowNavigation;
            int x = this.MirrorRectangle(bounds.X, bounds.Width - (flag ? 14 : 0), bounds, alignToRight);
            Rectangle visualBounds = new Rectangle(x, bounds.Y, bounds.Width - (flag ? 14 : 0), bounds.Height);
            base.PaintHeader(g, visualBounds, alignToRight, isDirty);
            int num2 = this.MirrorRectangle(bounds.X + visualBounds.Width, 14, bounds, alignToRight);
            Rectangle rectangle2 = new Rectangle(num2, bounds.Y, 14, bounds.Height);
            if (flag)
            {
                this.PaintPlusMinusGlyph(g, rectangle2, backBr, alignToRight);
            }
        }

        private void PaintPlusMinusGlyph(Graphics g, Rectangle bounds, Brush backBr, bool alignToRight)
        {
            bool traceVerbose = System.ComponentModel.CompModSwitches.DGRelationShpRowPaint.TraceVerbose;
            Rectangle outlineRect = this.GetOutlineRect(bounds.X, bounds.Y);
            outlineRect = Rectangle.Intersect(bounds, outlineRect);
            if (!outlineRect.IsEmpty)
            {
                g.FillRectangle(backBr, bounds);
                bool flag2 = System.ComponentModel.CompModSwitches.DGRelationShpRowPaint.TraceVerbose;
                Pen pen = base.dgTable.IsDefault ? base.DataGrid.HeaderForePen : base.dgTable.HeaderForePen;
                g.DrawRectangle(pen, outlineRect.X, outlineRect.Y, outlineRect.Width - 1, outlineRect.Height - 1);
                int num = 2;
                g.DrawLine(pen, (int) (outlineRect.X + num), (int) (outlineRect.Y + (outlineRect.Width / 2)), (int) ((outlineRect.Right - num) - 1), (int) (outlineRect.Y + (outlineRect.Width / 2)));
                if (!this.expanded)
                {
                    g.DrawLine(pen, (int) (outlineRect.X + (outlineRect.Height / 2)), (int) (outlineRect.Y + num), (int) (outlineRect.X + (outlineRect.Height / 2)), (int) ((outlineRect.Bottom - num) - 1));
                }
                else
                {
                    Point[] pointArray;
                    pointArray = new Point[] { new Point(outlineRect.X + (outlineRect.Height / 2), outlineRect.Bottom), new Point(pointArray[0].X, (bounds.Y + (2 * num)) + base.Height), new Point(alignToRight ? bounds.X : bounds.Right, pointArray[1].Y) };
                    g.DrawLines(pen, pointArray);
                }
            }
        }

        private void PaintRelations(Graphics g, Rectangle bounds, Rectangle trueRowBounds, int dataWidth, int firstCol, int nCols, bool alignToRight)
        {
            Rectangle relationshipRect = this.GetRelationshipRect();
            relationshipRect.X = alignToRight ? (bounds.Right - relationshipRect.Width) : bounds.X;
            relationshipRect.Y = bounds.Y;
            int num = Math.Max(dataWidth, relationshipRect.Width);
            Region clip = g.Clip;
            g.ExcludeClip(relationshipRect);
            g.FillRectangle(base.GetBackBrush(), alignToRight ? (bounds.Right - dataWidth) : bounds.X, bounds.Y, dataWidth, bounds.Height);
            g.SetClip(bounds);
            relationshipRect.Height -= base.dgTable.BorderWidth;
            g.DrawRectangle(SystemPens.ControlText, relationshipRect.X, relationshipRect.Y, relationshipRect.Width - 1, relationshipRect.Height - 1);
            relationshipRect.Inflate(-1, -1);
            int num2 = this.PaintRelationText(g, relationshipRect, alignToRight);
            if (num2 < relationshipRect.Height)
            {
                g.FillRectangle(base.GetBackBrush(), relationshipRect.X, relationshipRect.Y + num2, relationshipRect.Width, relationshipRect.Height - num2);
            }
            g.Clip = clip;
            if (num < bounds.Width)
            {
                int gridLineWidth;
                if (base.dgTable.IsDefault)
                {
                    gridLineWidth = base.DataGrid.GridLineWidth;
                }
                else
                {
                    gridLineWidth = base.dgTable.GridLineWidth;
                }
                g.FillRectangle(base.DataGrid.BackgroundBrush, alignToRight ? bounds.X : (bounds.X + num), bounds.Y, ((bounds.Width - num) - gridLineWidth) + 1, bounds.Height);
                if (gridLineWidth > 0)
                {
                    Brush gridLineBrush;
                    if (base.dgTable.IsDefault)
                    {
                        gridLineBrush = base.DataGrid.GridLineBrush;
                    }
                    else
                    {
                        gridLineBrush = base.dgTable.GridLineBrush;
                    }
                    g.FillRectangle(gridLineBrush, alignToRight ? ((bounds.Right - gridLineWidth) - num) : ((bounds.X + num) - gridLineWidth), bounds.Y, gridLineWidth, bounds.Height);
                }
            }
        }

        private int PaintRelationText(Graphics g, Rectangle bounds, bool alignToRight)
        {
            g.FillRectangle(base.GetBackBrush(), bounds.X, bounds.Y, bounds.Width, 1);
            int relationshipHeight = base.dgTable.RelationshipHeight;
            Rectangle rect = new Rectangle(bounds.X, bounds.Y + 1, bounds.Width, relationshipHeight);
            int num2 = 1;
            for (int i = 0; i < base.dgTable.RelationsList.Count; i++)
            {
                if (num2 > bounds.Height)
                {
                    return num2;
                }
                Brush brush = base.dgTable.IsDefault ? base.DataGrid.LinkBrush : base.dgTable.LinkBrush;
                Font linkFont = base.DataGrid.Font;
                brush = base.dgTable.IsDefault ? base.DataGrid.LinkBrush : base.dgTable.LinkBrush;
                linkFont = base.DataGrid.LinkFont;
                g.FillRectangle(base.GetBackBrush(), rect);
                StringFormat format = new StringFormat();
                if (alignToRight)
                {
                    format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                    format.Alignment = StringAlignment.Far;
                }
                g.DrawString((string) base.dgTable.RelationsList[i], linkFont, brush, rect, format);
                if ((i == this.FocusedRelation) && (base.number == base.DataGrid.CurrentCell.RowNumber))
                {
                    rect.Width = base.dgTable.FocusedTextWidth;
                    ControlPaint.DrawFocusRectangle(g, rect, ((SolidBrush) brush).Color, ((SolidBrush) base.GetBackBrush()).Color);
                    rect.Width = bounds.Width;
                }
                format.Dispose();
                rect.Y += relationshipHeight;
                num2 += rect.Height;
            }
            return num2;
        }

        private bool PointOverPlusMinusGlyph(int x, int y, Rectangle rowHeaders, bool alignToRight)
        {
            if (((base.dgTable == null) || (base.dgTable.DataGrid == null)) || !base.dgTable.DataGrid.AllowNavigation)
            {
                return false;
            }
            Rectangle rect = rowHeaders;
            if (!base.DataGrid.FlatMode)
            {
                rect.Inflate(-1, -1);
            }
            Rectangle outlineRect = this.GetOutlineRect(rect.Right - 14, 0);
            outlineRect.X = this.MirrorRectangle(outlineRect.X, outlineRect.Width, rect, alignToRight);
            return outlineRect.Contains(x, y);
        }

        internal override bool ProcessTabKey(Keys keyData, Rectangle rowHeaders, bool alignToRight)
        {
            if (((base.dgTable.RelationsList.Count == 0) || (base.dgTable.DataGrid == null)) || !base.dgTable.DataGrid.AllowNavigation)
            {
                return false;
            }
            if (!this.expanded)
            {
                this.Expand();
            }
            if ((keyData & Keys.Shift) == Keys.Shift)
            {
                if (this.FocusedRelation == 0)
                {
                    this.FocusedRelation = -1;
                    return false;
                }
                Rectangle relRect = this.GetRelationshipRect();
                relRect.X += rowHeaders.X + base.dgTable.RowHeaderWidth;
                relRect.X = this.MirrorRelationshipRectangle(relRect, rowHeaders, alignToRight);
                this.InvalidateRowRect(relRect);
                if (this.FocusedRelation == -1)
                {
                    this.FocusedRelation = base.dgTable.RelationsList.Count - 1;
                }
                else
                {
                    this.FocusedRelation--;
                }
                return true;
            }
            if (this.FocusedRelation == (base.dgTable.RelationsList.Count - 1))
            {
                this.FocusedRelation = -1;
                return false;
            }
            Rectangle relationshipRect = this.GetRelationshipRect();
            relationshipRect.X += rowHeaders.X + base.dgTable.RowHeaderWidth;
            relationshipRect.X = this.MirrorRelationshipRectangle(relationshipRect, rowHeaders, alignToRight);
            this.InvalidateRowRect(relationshipRect);
            this.FocusedRelation++;
            return true;
        }

        private int RelationFromY(int y)
        {
            int num = -1;
            int relationshipHeight = base.dgTable.RelationshipHeight;
            Rectangle relationshipRect = this.GetRelationshipRect();
            int num3 = (base.Height - base.dgTable.BorderWidth) + 1;
            while (num3 < relationshipRect.Bottom)
            {
                if (num3 > y)
                {
                    break;
                }
                num3 += relationshipHeight;
                num++;
            }
            if (num >= base.dgTable.RelationsList.Count)
            {
                return -1;
            }
            return num;
        }

        public virtual bool Expanded
        {
            get
            {
                return this.expanded;
            }
            set
            {
                if (this.expanded != value)
                {
                    if (this.expanded)
                    {
                        this.Collapse();
                    }
                    else
                    {
                        this.Expand();
                    }
                }
            }
        }

        private int FocusedRelation
        {
            get
            {
                return base.dgTable.FocusedRelation;
            }
            set
            {
                base.dgTable.FocusedRelation = value;
            }
        }

        public override int Height
        {
            get
            {
                int height = base.Height;
                if (this.expanded)
                {
                    return (height + this.GetRelationshipRect().Height);
                }
                return height;
            }
            set
            {
                if (this.expanded)
                {
                    base.Height = value - this.GetRelationshipRect().Height;
                }
                else
                {
                    base.Height = value;
                }
            }
        }

        [ComVisible(true)]
        protected class DataGridRelationshipAccessibleObject : AccessibleObject
        {
            private DataGridRelationshipRow owner;
            private int relationship;

            public DataGridRelationshipAccessibleObject(DataGridRelationshipRow owner, int relationship)
            {
                this.owner = owner;
                this.relationship = relationship;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                this.Owner.Expanded = true;
                this.owner.FocusedRelation = -1;
                this.DataGrid.NavigateTo((string) this.owner.dgTable.RelationsList[this.relationship], this.owner, true);
                this.DataGrid.BeginInvoke(new MethodInvoker(this.ResetAccessibilityLayer));
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation navdir)
            {
                switch (navdir)
                {
                    case AccessibleNavigation.Up:
                    case AccessibleNavigation.Left:
                    case AccessibleNavigation.Previous:
                        if (this.relationship <= 0)
                        {
                            break;
                        }
                        return this.Parent.GetChild(((this.Parent.GetChildCount() - this.owner.dgTable.RelationsList.Count) + this.relationship) - 1);

                    case AccessibleNavigation.Down:
                    case AccessibleNavigation.Right:
                    case AccessibleNavigation.Next:
                        if ((this.relationship + 1) >= this.owner.dgTable.RelationsList.Count)
                        {
                            break;
                        }
                        return this.Parent.GetChild(((this.Parent.GetChildCount() - this.owner.dgTable.RelationsList.Count) + this.relationship) + 1);
                }
                return null;
            }

            private void ResetAccessibilityLayer()
            {
                ((System.Windows.Forms.DataGrid.DataGridAccessibleObject) this.DataGrid.AccessibilityObject).NotifyClients(AccessibleEvents.Reorder, 0);
                ((System.Windows.Forms.DataGrid.DataGridAccessibleObject) this.DataGrid.AccessibilityObject).NotifyClients(AccessibleEvents.Focus, this.DataGrid.CurrentCellAccIndex);
                ((System.Windows.Forms.DataGrid.DataGridAccessibleObject) this.DataGrid.AccessibilityObject).NotifyClients(AccessibleEvents.Selection, this.DataGrid.CurrentCellAccIndex);
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
                    this.Owner.FocusedRelation = this.relationship;
                }
            }

            public override Rectangle Bounds
            {
                get
                {
                    Rectangle rowBounds = this.DataGrid.GetRowBounds(this.owner);
                    Rectangle r = this.owner.Expanded ? this.owner.GetRelationshipRectWithMirroring() : Rectangle.Empty;
                    r.Y += this.owner.dgTable.RelationshipHeight * this.relationship;
                    r.Height = this.owner.Expanded ? this.owner.dgTable.RelationshipHeight : 0;
                    if (!this.owner.Expanded)
                    {
                        r.X += rowBounds.X;
                    }
                    r.Y += rowBounds.Y;
                    return this.owner.DataGrid.RectangleToScreen(r);
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
                    return System.Windows.Forms.SR.GetString("AccDGNavigate");
                }
            }

            public override string Name
            {
                get
                {
                    return (string) this.owner.dgTable.RelationsList[this.relationship];
                }
            }

            protected DataGridRelationshipRow Owner
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
                    return this.owner.AccessibleObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.Link;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    if (Array.IndexOf<DataGridRow>(this.DataGrid.DataGridRows, this.owner) == -1)
                    {
                        return AccessibleStates.Unavailable;
                    }
                    AccessibleStates states = AccessibleStates.Linked | AccessibleStates.Selectable | AccessibleStates.Focusable;
                    if (!this.owner.Expanded)
                    {
                        states |= AccessibleStates.Invisible;
                    }
                    if (this.DataGrid.Focused && (this.Owner.dgTable.FocusedRelation == this.relationship))
                    {
                        states |= AccessibleStates.Focused;
                    }
                    return states;
                }
            }

            public override string Value
            {
                get
                {
                    if (Array.IndexOf<DataGridRow>(this.DataGrid.DataGridRows, this.owner) == -1)
                    {
                        return null;
                    }
                    return (string) this.owner.dgTable.RelationsList[this.relationship];
                }
                set
                {
                }
            }
        }

        [ComVisible(true)]
        protected class DataGridRelationshipRowAccessibleObject : DataGridRow.DataGridRowAccessibleObject
        {
            public DataGridRelationshipRowAccessibleObject(DataGridRow owner) : base(owner)
            {
            }

            protected override void AddChildAccessibleObjects(IList children)
            {
                base.AddChildAccessibleObjects(children);
                DataGridRelationshipRow owner = (DataGridRelationshipRow) base.Owner;
                if (owner.dgTable.RelationsList != null)
                {
                    for (int i = 0; i < owner.dgTable.RelationsList.Count; i++)
                    {
                        children.Add(new DataGridRelationshipRow.DataGridRelationshipAccessibleObject(owner, i));
                    }
                }
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                if (this.Row.dgTable.RelationsList.Count > 0)
                {
                    ((DataGridRelationshipRow) base.Owner).Expanded = !((DataGridRelationshipRow) base.Owner).Expanded;
                }
            }

            public override AccessibleObject GetFocused()
            {
                DataGridRelationshipRow owner = (DataGridRelationshipRow) base.Owner;
                int focusedRelation = owner.dgTable.FocusedRelation;
                if (focusedRelation == -1)
                {
                    return base.GetFocused();
                }
                return this.GetChild((this.GetChildCount() - owner.dgTable.RelationsList.Count) + focusedRelation);
            }

            public override string DefaultAction
            {
                get
                {
                    if (this.Row.dgTable.RelationsList.Count <= 0)
                    {
                        return null;
                    }
                    if (this.Row.Expanded)
                    {
                        return System.Windows.Forms.SR.GetString("AccDGCollapse");
                    }
                    return System.Windows.Forms.SR.GetString("AccDGExpand");
                }
            }

            private DataGridRelationshipRow Row
            {
                get
                {
                    return (DataGridRelationshipRow) base.Owner;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates state = base.State;
                    if (this.Row.dgTable.RelationsList.Count <= 0)
                    {
                        return state;
                    }
                    if (((DataGridRelationshipRow) base.Owner).Expanded)
                    {
                        return (state | AccessibleStates.Expanded);
                    }
                    return (state | AccessibleStates.Collapsed);
                }
            }
        }
    }
}

