namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms.VisualStyles;

    public class DataGridViewHeaderCell : DataGridViewCell
    {
        private const string AEROTHEMEFILENAME = "Aero.msstyles";
        private static System.Type cellType = typeof(DataGridViewHeaderCell);
        private const byte DATAGRIDVIEWHEADERCELL_themeMargin = 100;
        private static System.Type defaultFormattedValueType = typeof(string);
        private static System.Type defaultValueType = typeof(object);
        private static readonly int PropButtonState = PropertyStore.CreateKey();
        private static readonly int PropFlipXPThemesBitmap = PropertyStore.CreateKey();
        private static readonly int PropValueType = PropertyStore.CreateKey();
        private static Rectangle rectThemeMargins = new Rectangle(-1, -1, 0, 0);

        public override object Clone()
        {
            DataGridViewHeaderCell cell;
            System.Type type = base.GetType();
            if (type == cellType)
            {
                cell = new DataGridViewHeaderCell();
            }
            else
            {
                cell = (DataGridViewHeaderCell) Activator.CreateInstance(type);
            }
            base.CloneInternal(cell);
            cell.Value = base.Value;
            return cell;
        }

        protected override void Dispose(bool disposing)
        {
            if ((this.FlipXPThemesBitmap != null) && disposing)
            {
                this.FlipXPThemesBitmap.Dispose();
            }
            base.Dispose(disposing);
        }

        public override ContextMenuStrip GetInheritedContextMenuStrip(int rowIndex)
        {
            ContextMenuStrip contextMenuStrip = base.GetContextMenuStrip(rowIndex);
            if (contextMenuStrip != null)
            {
                return contextMenuStrip;
            }
            if (base.DataGridView != null)
            {
                return base.DataGridView.ContextMenuStrip;
            }
            return null;
        }

        public override DataGridViewElementStates GetInheritedState(int rowIndex)
        {
            DataGridViewElementStates states = DataGridViewElementStates.ResizableSet | DataGridViewElementStates.ReadOnly;
            if (base.OwningRow != null)
            {
                if (((base.DataGridView == null) && (rowIndex != -1)) || ((base.DataGridView != null) && ((rowIndex < 0) || (rowIndex >= base.DataGridView.Rows.Count))))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "rowIndex", rowIndex.ToString(CultureInfo.CurrentCulture) }));
                }
                if ((base.DataGridView != null) && (base.DataGridView.Rows.SharedRow(rowIndex) != base.OwningRow))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "rowIndex", rowIndex.ToString(CultureInfo.CurrentCulture) }));
                }
                states |= base.OwningRow.GetState(rowIndex) & DataGridViewElementStates.Frozen;
                if ((base.OwningRow.GetResizable(rowIndex) == DataGridViewTriState.True) || ((base.DataGridView != null) && (base.DataGridView.RowHeadersWidthSizeMode == DataGridViewRowHeadersWidthSizeMode.EnableResizing)))
                {
                    states |= DataGridViewElementStates.Resizable;
                }
                if (base.OwningRow.GetVisible(rowIndex) && ((base.DataGridView == null) || base.DataGridView.RowHeadersVisible))
                {
                    states |= DataGridViewElementStates.Visible;
                    if (base.OwningRow.GetDisplayed(rowIndex))
                    {
                        states |= DataGridViewElementStates.Displayed;
                    }
                }
                return states;
            }
            if (base.OwningColumn != null)
            {
                if (rowIndex != -1)
                {
                    throw new ArgumentOutOfRangeException("rowIndex");
                }
                states |= base.OwningColumn.State & DataGridViewElementStates.Frozen;
                if ((base.OwningColumn.Resizable == DataGridViewTriState.True) || ((base.DataGridView != null) && (base.DataGridView.ColumnHeadersHeightSizeMode == DataGridViewColumnHeadersHeightSizeMode.EnableResizing)))
                {
                    states |= DataGridViewElementStates.Resizable;
                }
                if (base.OwningColumn.Visible && ((base.DataGridView == null) || base.DataGridView.ColumnHeadersVisible))
                {
                    states |= DataGridViewElementStates.Visible;
                    if (base.OwningColumn.Displayed)
                    {
                        states |= DataGridViewElementStates.Displayed;
                    }
                }
                return states;
            }
            if (base.DataGridView != null)
            {
                if (rowIndex != -1)
                {
                    throw new ArgumentOutOfRangeException("rowIndex");
                }
                states |= DataGridViewElementStates.Frozen;
                if ((base.DataGridView.RowHeadersWidthSizeMode == DataGridViewRowHeadersWidthSizeMode.EnableResizing) || (base.DataGridView.ColumnHeadersHeightSizeMode == DataGridViewColumnHeadersHeightSizeMode.EnableResizing))
                {
                    states |= DataGridViewElementStates.Resizable;
                }
                if (base.DataGridView.RowHeadersVisible && base.DataGridView.ColumnHeadersVisible)
                {
                    states |= DataGridViewElementStates.Visible;
                    if (base.DataGridView.LayoutInfo.TopLeftHeader != Rectangle.Empty)
                    {
                        states |= DataGridViewElementStates.Displayed;
                    }
                }
            }
            return states;
        }

        protected override Size GetSize(int rowIndex)
        {
            if (base.DataGridView == null)
            {
                if (rowIndex != -1)
                {
                    throw new ArgumentOutOfRangeException("rowIndex");
                }
                return new Size(-1, -1);
            }
            if (base.OwningColumn != null)
            {
                if (rowIndex != -1)
                {
                    throw new ArgumentOutOfRangeException("rowIndex");
                }
                return new Size(base.OwningColumn.Thickness, base.DataGridView.ColumnHeadersHeight);
            }
            if (base.OwningRow != null)
            {
                if ((rowIndex < 0) || (rowIndex >= base.DataGridView.Rows.Count))
                {
                    throw new ArgumentOutOfRangeException("rowIndex");
                }
                if (base.DataGridView.Rows.SharedRow(rowIndex) != base.OwningRow)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "rowIndex", rowIndex.ToString(CultureInfo.CurrentCulture) }));
                }
                return new Size(base.DataGridView.RowHeadersWidth, base.OwningRow.GetHeight(rowIndex));
            }
            if (rowIndex != -1)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            return new Size(base.DataGridView.RowHeadersWidth, base.DataGridView.ColumnHeadersHeight);
        }

        internal static Rectangle GetThemeMargins(Graphics g)
        {
            if (rectThemeMargins.X == -1)
            {
                Rectangle bounds = new Rectangle(0, 0, 100, 100);
                Rectangle backgroundContentRectangle = DataGridViewHeaderCellRenderer.VisualStyleRenderer.GetBackgroundContentRectangle(g, bounds);
                rectThemeMargins.X = backgroundContentRectangle.X;
                rectThemeMargins.Y = backgroundContentRectangle.Y;
                rectThemeMargins.Width = 100 - backgroundContentRectangle.Right;
                rectThemeMargins.Height = 100 - backgroundContentRectangle.Bottom;
                if ((rectThemeMargins.X == 3) && (((rectThemeMargins.Y + rectThemeMargins.Width) + rectThemeMargins.Height) == 0))
                {
                    rectThemeMargins = new Rectangle(0, 0, 2, 3);
                }
                else
                {
                    try
                    {
                        if (string.Equals(Path.GetFileName(VisualStyleInformation.ThemeFilename), "Aero.msstyles", StringComparison.OrdinalIgnoreCase))
                        {
                            rectThemeMargins = new Rectangle(2, 1, 0, 2);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            return rectThemeMargins;
        }

        protected override object GetValue(int rowIndex)
        {
            if (rowIndex != -1)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            return base.Properties.GetObject(DataGridViewCell.PropCellValue);
        }

        protected override bool MouseDownUnsharesRow(DataGridViewCellMouseEventArgs e)
        {
            return ((e.Button == MouseButtons.Left) && base.DataGridView.ApplyVisualStylesToHeaderCells);
        }

        protected override bool MouseEnterUnsharesRow(int rowIndex)
        {
            return (((base.ColumnIndex == base.DataGridView.MouseDownCellAddress.X) && (rowIndex == base.DataGridView.MouseDownCellAddress.Y)) && base.DataGridView.ApplyVisualStylesToHeaderCells);
        }

        protected override bool MouseLeaveUnsharesRow(int rowIndex)
        {
            return ((this.ButtonState != System.Windows.Forms.ButtonState.Normal) && base.DataGridView.ApplyVisualStylesToHeaderCells);
        }

        protected override bool MouseUpUnsharesRow(DataGridViewCellMouseEventArgs e)
        {
            return ((e.Button == MouseButtons.Left) && base.DataGridView.ApplyVisualStylesToHeaderCells);
        }

        protected override void OnMouseDown(DataGridViewCellMouseEventArgs e)
        {
            if ((base.DataGridView != null) && (((e.Button == MouseButtons.Left) && base.DataGridView.ApplyVisualStylesToHeaderCells) && !base.DataGridView.ResizingOperationAboutToStart))
            {
                this.UpdateButtonState(System.Windows.Forms.ButtonState.Pushed, e.RowIndex);
            }
        }

        protected override void OnMouseEnter(int rowIndex)
        {
            if ((base.DataGridView != null) && base.DataGridView.ApplyVisualStylesToHeaderCells)
            {
                if ((((base.ColumnIndex == base.DataGridView.MouseDownCellAddress.X) && (rowIndex == base.DataGridView.MouseDownCellAddress.Y)) && ((this.ButtonState == System.Windows.Forms.ButtonState.Normal) && (Control.MouseButtons == MouseButtons.Left))) && !base.DataGridView.ResizingOperationAboutToStart)
                {
                    this.UpdateButtonState(System.Windows.Forms.ButtonState.Pushed, rowIndex);
                }
                base.DataGridView.InvalidateCell(base.ColumnIndex, rowIndex);
            }
        }

        protected override void OnMouseLeave(int rowIndex)
        {
            if ((base.DataGridView != null) && base.DataGridView.ApplyVisualStylesToHeaderCells)
            {
                if (this.ButtonState != System.Windows.Forms.ButtonState.Normal)
                {
                    this.UpdateButtonState(System.Windows.Forms.ButtonState.Normal, rowIndex);
                }
                base.DataGridView.InvalidateCell(base.ColumnIndex, rowIndex);
            }
        }

        protected override void OnMouseUp(DataGridViewCellMouseEventArgs e)
        {
            if ((base.DataGridView != null) && ((e.Button == MouseButtons.Left) && base.DataGridView.ApplyVisualStylesToHeaderCells))
            {
                this.UpdateButtonState(System.Windows.Forms.ButtonState.Normal, e.RowIndex);
            }
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates dataGridViewElementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            if (DataGridViewCell.PaintBorder(paintParts))
            {
                this.PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
            }
            if (DataGridViewCell.PaintBackground(paintParts))
            {
                Rectangle rect = cellBounds;
                Rectangle rectangle2 = this.BorderWidths(advancedBorderStyle);
                rect.Offset(rectangle2.X, rectangle2.Y);
                rect.Width -= rectangle2.Right;
                rect.Height -= rectangle2.Bottom;
                bool flag = (dataGridViewElementState & DataGridViewElementStates.Selected) != DataGridViewElementStates.None;
                SolidBrush cachedBrush = base.DataGridView.GetCachedBrush((DataGridViewCell.PaintSelectionBackground(paintParts) && flag) ? cellStyle.SelectionBackColor : cellStyle.BackColor);
                if (cachedBrush.Color.A == 0xff)
                {
                    graphics.FillRectangle(cachedBrush, rect);
                }
            }
        }

        public override string ToString()
        {
            return ("DataGridViewHeaderCell { ColumnIndex=" + base.ColumnIndex.ToString(CultureInfo.CurrentCulture) + ", RowIndex=" + base.RowIndex.ToString(CultureInfo.CurrentCulture) + " }");
        }

        private void UpdateButtonState(System.Windows.Forms.ButtonState newButtonState, int rowIndex)
        {
            this.ButtonStatePrivate = newButtonState;
            base.DataGridView.InvalidateCell(base.ColumnIndex, rowIndex);
        }

        protected System.Windows.Forms.ButtonState ButtonState
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropButtonState, out flag);
                if (flag)
                {
                    return (System.Windows.Forms.ButtonState) integer;
                }
                return System.Windows.Forms.ButtonState.Normal;
            }
        }

        private System.Windows.Forms.ButtonState ButtonStatePrivate
        {
            set
            {
                if (this.ButtonState != value)
                {
                    base.Properties.SetInteger(PropButtonState, (int) value);
                }
            }
        }

        [Browsable(false)]
        public override bool Displayed
        {
            get
            {
                if ((base.DataGridView == null) || !base.DataGridView.Visible)
                {
                    return false;
                }
                if (base.OwningRow != null)
                {
                    return (base.DataGridView.RowHeadersVisible && base.OwningRow.Displayed);
                }
                if (base.OwningColumn == null)
                {
                    return (base.DataGridView.LayoutInfo.TopLeftHeader != Rectangle.Empty);
                }
                return (base.DataGridView.ColumnHeadersVisible && base.OwningColumn.Displayed);
            }
        }

        internal Bitmap FlipXPThemesBitmap
        {
            get
            {
                return (Bitmap) base.Properties.GetObject(PropFlipXPThemesBitmap);
            }
            set
            {
                if ((value != null) || base.Properties.ContainsObject(PropFlipXPThemesBitmap))
                {
                    base.Properties.SetObject(PropFlipXPThemesBitmap, value);
                }
            }
        }

        public override System.Type FormattedValueType
        {
            get
            {
                return defaultFormattedValueType;
            }
        }

        [Browsable(false)]
        public override bool Frozen
        {
            get
            {
                if (base.OwningRow != null)
                {
                    return base.OwningRow.Frozen;
                }
                if (base.OwningColumn != null)
                {
                    return base.OwningColumn.Frozen;
                }
                return (base.DataGridView != null);
            }
        }

        internal override bool HasValueType
        {
            get
            {
                return (base.Properties.ContainsObject(PropValueType) && (base.Properties.GetObject(PropValueType) != null));
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool ReadOnly
        {
            get
            {
                return true;
            }
            set
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_HeaderCellReadOnlyProperty", new object[] { "ReadOnly" }));
            }
        }

        [Browsable(false)]
        public override bool Resizable
        {
            get
            {
                if (base.OwningRow != null)
                {
                    return ((base.OwningRow.Resizable == DataGridViewTriState.True) || ((base.DataGridView != null) && (base.DataGridView.RowHeadersWidthSizeMode == DataGridViewRowHeadersWidthSizeMode.EnableResizing)));
                }
                if (base.OwningColumn != null)
                {
                    return ((base.OwningColumn.Resizable == DataGridViewTriState.True) || ((base.DataGridView != null) && (base.DataGridView.ColumnHeadersHeightSizeMode == DataGridViewColumnHeadersHeightSizeMode.EnableResizing)));
                }
                if (base.DataGridView == null)
                {
                    return false;
                }
                if (base.DataGridView.RowHeadersWidthSizeMode != DataGridViewRowHeadersWidthSizeMode.EnableResizing)
                {
                    return (base.DataGridView.ColumnHeadersHeightSizeMode == DataGridViewColumnHeadersHeightSizeMode.EnableResizing);
                }
                return true;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public override bool Selected
        {
            get
            {
                return false;
            }
            set
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_HeaderCellReadOnlyProperty", new object[] { "Selected" }));
            }
        }

        public override System.Type ValueType
        {
            get
            {
                System.Type type = (System.Type) base.Properties.GetObject(PropValueType);
                if (type != null)
                {
                    return type;
                }
                return defaultValueType;
            }
            set
            {
                if ((value != null) || base.Properties.ContainsObject(PropValueType))
                {
                    base.Properties.SetObject(PropValueType, value);
                }
            }
        }

        [Browsable(false)]
        public override bool Visible
        {
            get
            {
                if (base.OwningRow != null)
                {
                    if (!base.OwningRow.Visible)
                    {
                        return false;
                    }
                    if (base.DataGridView != null)
                    {
                        return base.DataGridView.RowHeadersVisible;
                    }
                    return true;
                }
                if (base.OwningColumn != null)
                {
                    if (!base.OwningColumn.Visible)
                    {
                        return false;
                    }
                    if (base.DataGridView != null)
                    {
                        return base.DataGridView.ColumnHeadersVisible;
                    }
                    return true;
                }
                if (base.DataGridView == null)
                {
                    return false;
                }
                return (base.DataGridView.RowHeadersVisible && base.DataGridView.ColumnHeadersVisible);
            }
        }

        private class DataGridViewHeaderCellRenderer
        {
            private static System.Windows.Forms.VisualStyles.VisualStyleRenderer visualStyleRenderer;

            private DataGridViewHeaderCellRenderer()
            {
            }

            public static System.Windows.Forms.VisualStyles.VisualStyleRenderer VisualStyleRenderer
            {
                get
                {
                    if (visualStyleRenderer == null)
                    {
                        visualStyleRenderer = new System.Windows.Forms.VisualStyles.VisualStyleRenderer(VisualStyleElement.Header.Item.Normal);
                    }
                    return visualStyleRenderer;
                }
            }
        }
    }
}

