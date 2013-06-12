namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Windows.Forms.VisualStyles;

    public class DataGridViewTopLeftHeaderCell : DataGridViewColumnHeaderCell
    {
        private const byte DATAGRIDVIEWTOPLEFTHEADERCELL_horizontalTextMarginLeft = 1;
        private const byte DATAGRIDVIEWTOPLEFTHEADERCELL_horizontalTextMarginRight = 2;
        private const byte DATAGRIDVIEWTOPLEFTHEADERCELL_verticalTextMargin = 1;
        private static readonly VisualStyleElement HeaderElement = VisualStyleElement.Header.Item.Normal;

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new DataGridViewTopLeftHeaderCellAccessibleObject(this);
        }

        protected override Rectangle GetContentBounds(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
        {
            DataGridViewAdvancedBorderStyle style;
            DataGridViewElementStates states;
            Rectangle rectangle;
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            if (rowIndex != -1)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            if (base.DataGridView == null)
            {
                return Rectangle.Empty;
            }
            object formattedValue = this.GetValue(rowIndex);
            base.ComputeBorderStyleCellStateAndCellBounds(rowIndex, out style, out states, out rectangle);
            return this.PaintPrivate(graphics, rectangle, rectangle, rowIndex, states, formattedValue, null, cellStyle, style, DataGridViewPaintParts.ContentForeground, true, false, false);
        }

        protected override Rectangle GetErrorIconBounds(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
        {
            DataGridViewAdvancedBorderStyle style;
            DataGridViewElementStates states;
            Rectangle rectangle;
            if (rowIndex != -1)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            if (base.DataGridView == null)
            {
                return Rectangle.Empty;
            }
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            base.ComputeBorderStyleCellStateAndCellBounds(rowIndex, out style, out states, out rectangle);
            return this.PaintPrivate(graphics, rectangle, rectangle, rowIndex, states, null, this.GetErrorText(rowIndex), cellStyle, style, DataGridViewPaintParts.ContentForeground, false, true, false);
        }

        protected override Size GetPreferredSize(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
        {
            if (rowIndex != -1)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            if (base.DataGridView == null)
            {
                return new Size(-1, -1);
            }
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            Rectangle rectangle = this.BorderWidths(base.DataGridView.AdjustedTopLeftHeaderBorderStyle);
            int borderAndPaddingWidths = (rectangle.Left + rectangle.Width) + cellStyle.Padding.Horizontal;
            int borderAndPaddingHeights = (rectangle.Top + rectangle.Height) + cellStyle.Padding.Vertical;
            TextFormatFlags flags = DataGridViewUtilities.ComputeTextFormatFlagsForCellStyleAlignment(base.DataGridView.RightToLeftInternal, cellStyle.Alignment, cellStyle.WrapMode);
            object obj2 = this.GetValue(rowIndex);
            if (!(obj2 is string))
            {
                obj2 = null;
            }
            return DataGridViewUtilities.GetPreferredRowHeaderSize(graphics, (string) obj2, cellStyle, borderAndPaddingWidths, borderAndPaddingHeights, base.DataGridView.ShowCellErrors, false, constraintSize, flags);
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            this.PaintPrivate(graphics, clipBounds, cellBounds, rowIndex, cellState, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts, false, false, true);
        }

        protected override void PaintBorder(Graphics graphics, Rectangle clipBounds, Rectangle bounds, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle)
        {
            if (base.DataGridView != null)
            {
                base.PaintBorder(graphics, clipBounds, bounds, cellStyle, advancedBorderStyle);
                if (!base.DataGridView.RightToLeftInternal && base.DataGridView.ApplyVisualStylesToHeaderCells)
                {
                    if (base.DataGridView.AdvancedColumnHeadersBorderStyle.All == DataGridViewAdvancedCellBorderStyle.Inset)
                    {
                        Pen darkPen = null;
                        Pen lightPen = null;
                        base.GetContrastedPens(cellStyle.BackColor, ref darkPen, ref lightPen);
                        graphics.DrawLine(darkPen, bounds.X, bounds.Y, bounds.X, bounds.Bottom - 1);
                        graphics.DrawLine(darkPen, bounds.X, bounds.Y, bounds.Right - 1, bounds.Y);
                    }
                    else if (base.DataGridView.AdvancedColumnHeadersBorderStyle.All == DataGridViewAdvancedCellBorderStyle.Outset)
                    {
                        Pen pen3 = null;
                        Pen pen4 = null;
                        base.GetContrastedPens(cellStyle.BackColor, ref pen3, ref pen4);
                        graphics.DrawLine(pen4, bounds.X, bounds.Y, bounds.X, bounds.Bottom - 1);
                        graphics.DrawLine(pen4, bounds.X, bounds.Y, bounds.Right - 1, bounds.Y);
                    }
                    else if (base.DataGridView.AdvancedColumnHeadersBorderStyle.All == DataGridViewAdvancedCellBorderStyle.InsetDouble)
                    {
                        Pen pen5 = null;
                        Pen pen6 = null;
                        base.GetContrastedPens(cellStyle.BackColor, ref pen5, ref pen6);
                        graphics.DrawLine(pen5, (int) (bounds.X + 1), (int) (bounds.Y + 1), (int) (bounds.X + 1), (int) (bounds.Bottom - 1));
                        graphics.DrawLine(pen5, (int) (bounds.X + 1), (int) (bounds.Y + 1), (int) (bounds.Right - 1), (int) (bounds.Y + 1));
                    }
                }
            }
        }

        private Rectangle PaintPrivate(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts, bool computeContentBounds, bool computeErrorIconBounds, bool paint)
        {
            Rectangle empty = Rectangle.Empty;
            Rectangle bounds = cellBounds;
            Rectangle rectangle3 = this.BorderWidths(advancedBorderStyle);
            bounds.Offset(rectangle3.X, rectangle3.Y);
            bounds.Width -= rectangle3.Right;
            bounds.Height -= rectangle3.Bottom;
            bool flag = (cellState & DataGridViewElementStates.Selected) != DataGridViewElementStates.None;
            if (paint && DataGridViewCell.PaintBackground(paintParts))
            {
                if (base.DataGridView.ApplyVisualStylesToHeaderCells)
                {
                    int headerState = 1;
                    if (base.ButtonState != ButtonState.Normal)
                    {
                        headerState = 3;
                    }
                    else if ((base.DataGridView.MouseEnteredCellAddress.Y == rowIndex) && (base.DataGridView.MouseEnteredCellAddress.X == base.ColumnIndex))
                    {
                        headerState = 2;
                    }
                    bounds.Inflate(0x10, 0x10);
                    DataGridViewTopLeftHeaderCellRenderer.DrawHeader(graphics, bounds, headerState);
                    bounds.Inflate(-16, -16);
                }
                else
                {
                    SolidBrush cachedBrush = base.DataGridView.GetCachedBrush((DataGridViewCell.PaintSelectionBackground(paintParts) && flag) ? cellStyle.SelectionBackColor : cellStyle.BackColor);
                    if (cachedBrush.Color.A == 0xff)
                    {
                        graphics.FillRectangle(cachedBrush, bounds);
                    }
                }
            }
            if (paint && DataGridViewCell.PaintBorder(paintParts))
            {
                this.PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
            }
            if (cellStyle.Padding != Padding.Empty)
            {
                if (base.DataGridView.RightToLeftInternal)
                {
                    bounds.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
                }
                else
                {
                    bounds.Offset(cellStyle.Padding.Left, cellStyle.Padding.Top);
                }
                bounds.Width -= cellStyle.Padding.Horizontal;
                bounds.Height -= cellStyle.Padding.Vertical;
            }
            Rectangle cellValueBounds = bounds;
            string str = formattedValue as string;
            bounds.Offset(1, 1);
            bounds.Width -= 3;
            bounds.Height -= 2;
            if ((((bounds.Width > 0) && (bounds.Height > 0)) && !string.IsNullOrEmpty(str)) && (paint || computeContentBounds))
            {
                Color color;
                if (base.DataGridView.ApplyVisualStylesToHeaderCells)
                {
                    color = DataGridViewTopLeftHeaderCellRenderer.VisualStyleRenderer.GetColor(ColorProperty.TextColor);
                }
                else
                {
                    color = flag ? cellStyle.SelectionForeColor : cellStyle.ForeColor;
                }
                TextFormatFlags flags = DataGridViewUtilities.ComputeTextFormatFlagsForCellStyleAlignment(base.DataGridView.RightToLeftInternal, cellStyle.Alignment, cellStyle.WrapMode);
                if (paint)
                {
                    if (DataGridViewCell.PaintContentForeground(paintParts))
                    {
                        if ((flags & TextFormatFlags.SingleLine) != TextFormatFlags.Default)
                        {
                            flags |= TextFormatFlags.EndEllipsis;
                        }
                        TextRenderer.DrawText(graphics, str, cellStyle.Font, bounds, color, flags);
                    }
                }
                else
                {
                    empty = DataGridViewUtilities.GetTextBounds(bounds, str, flags, cellStyle);
                }
            }
            else if (computeErrorIconBounds && !string.IsNullOrEmpty(errorText))
            {
                empty = base.ComputeErrorIconBounds(cellValueBounds);
            }
            if ((base.DataGridView.ShowCellErrors && paint) && DataGridViewCell.PaintErrorIcon(paintParts))
            {
                base.PaintErrorIcon(graphics, cellStyle, rowIndex, cellBounds, cellValueBounds, errorText);
            }
            return empty;
        }

        public override string ToString()
        {
            return "DataGridViewTopLeftHeaderCell";
        }

        protected class DataGridViewTopLeftHeaderCellAccessibleObject : DataGridViewColumnHeaderCell.DataGridViewColumnHeaderCellAccessibleObject
        {
            public DataGridViewTopLeftHeaderCellAccessibleObject(DataGridViewTopLeftHeaderCell owner) : base(owner)
            {
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                base.Owner.DataGridView.SelectAll();
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation navigationDirection)
            {
                switch (navigationDirection)
                {
                    case AccessibleNavigation.Left:
                        if (base.Owner.DataGridView.RightToLeft != RightToLeft.No)
                        {
                            return this.NavigateForward();
                        }
                        return null;

                    case AccessibleNavigation.Right:
                        if (base.Owner.DataGridView.RightToLeft != RightToLeft.No)
                        {
                            return null;
                        }
                        return this.NavigateForward();

                    case AccessibleNavigation.Next:
                        return this.NavigateForward();

                    case AccessibleNavigation.Previous:
                        return null;
                }
                return null;
            }

            private AccessibleObject NavigateForward()
            {
                if (base.Owner.DataGridView.Columns.GetColumnCount(DataGridViewElementStates.Visible) == 0)
                {
                    return null;
                }
                return base.Owner.DataGridView.AccessibilityObject.GetChild(0).GetChild(1);
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void Select(AccessibleSelection flags)
            {
                if (base.Owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellAccessibleObject_OwnerNotSet"));
                }
                if ((flags & AccessibleSelection.TakeFocus) == AccessibleSelection.TakeFocus)
                {
                    base.Owner.DataGridView.FocusInternal();
                    if ((base.Owner.DataGridView.Columns.GetColumnCount(DataGridViewElementStates.Visible) > 0) && (base.Owner.DataGridView.Rows.GetRowCount(DataGridViewElementStates.Visible) > 0))
                    {
                        DataGridViewRow row = base.Owner.DataGridView.Rows[base.Owner.DataGridView.Rows.GetFirstRow(DataGridViewElementStates.Visible)];
                        DataGridViewColumn firstColumn = base.Owner.DataGridView.Columns.GetFirstColumn(DataGridViewElementStates.Visible);
                        base.Owner.DataGridView.SetCurrentCellAddressCoreInternal(firstColumn.Index, row.Index, false, true, false);
                    }
                }
                if (((flags & AccessibleSelection.AddSelection) == AccessibleSelection.AddSelection) && base.Owner.DataGridView.MultiSelect)
                {
                    base.Owner.DataGridView.SelectAll();
                }
                if (((flags & AccessibleSelection.RemoveSelection) == AccessibleSelection.RemoveSelection) && ((flags & AccessibleSelection.AddSelection) == AccessibleSelection.None))
                {
                    base.Owner.DataGridView.ClearSelection();
                }
            }

            public override Rectangle Bounds
            {
                get
                {
                    Rectangle r = base.Owner.DataGridView.GetCellDisplayRectangle(-1, -1, false);
                    return base.Owner.DataGridView.RectangleToScreen(r);
                }
            }

            public override string DefaultAction
            {
                get
                {
                    if (base.Owner.DataGridView.MultiSelect)
                    {
                        return System.Windows.Forms.SR.GetString("DataGridView_AccTopLeftColumnHeaderCellDefaultAction");
                    }
                    return string.Empty;
                }
            }

            public override string Name
            {
                get
                {
                    object obj2 = base.Owner.Value;
                    if ((obj2 != null) && !(obj2 is string))
                    {
                        return string.Empty;
                    }
                    string str = obj2 as string;
                    if (!string.IsNullOrEmpty(str))
                    {
                        return string.Empty;
                    }
                    if (base.Owner.DataGridView == null)
                    {
                        return string.Empty;
                    }
                    if (base.Owner.DataGridView.RightToLeft == RightToLeft.No)
                    {
                        return System.Windows.Forms.SR.GetString("DataGridView_AccTopLeftColumnHeaderCellName");
                    }
                    return System.Windows.Forms.SR.GetString("DataGridView_AccTopLeftColumnHeaderCellNameRTL");
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates selectable = AccessibleStates.Selectable;
                    if ((base.State & AccessibleStates.Offscreen) == AccessibleStates.Offscreen)
                    {
                        selectable |= AccessibleStates.Offscreen;
                    }
                    if (base.Owner.DataGridView.AreAllCellsSelected(false))
                    {
                        selectable |= AccessibleStates.Selected;
                    }
                    return selectable;
                }
            }

            public override string Value
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return string.Empty;
                }
            }
        }

        private class DataGridViewTopLeftHeaderCellRenderer
        {
            private static System.Windows.Forms.VisualStyles.VisualStyleRenderer visualStyleRenderer;

            private DataGridViewTopLeftHeaderCellRenderer()
            {
            }

            public static void DrawHeader(Graphics g, Rectangle bounds, int headerState)
            {
                VisualStyleRenderer.SetParameters(DataGridViewTopLeftHeaderCell.HeaderElement.ClassName, DataGridViewTopLeftHeaderCell.HeaderElement.Part, headerState);
                VisualStyleRenderer.DrawBackground(g, bounds, Rectangle.Truncate(g.ClipBounds));
            }

            public static System.Windows.Forms.VisualStyles.VisualStyleRenderer VisualStyleRenderer
            {
                get
                {
                    if (visualStyleRenderer == null)
                    {
                        visualStyleRenderer = new System.Windows.Forms.VisualStyles.VisualStyleRenderer(DataGridViewTopLeftHeaderCell.HeaderElement);
                    }
                    return visualStyleRenderer;
                }
            }
        }
    }
}

