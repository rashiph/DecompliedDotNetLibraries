namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Forms.VisualStyles;

    public class DataGridViewColumnHeaderCell : DataGridViewHeaderCell
    {
        private static System.Type cellType = typeof(DataGridViewColumnHeaderCell);
        private const byte DATAGRIDVIEWCOLUMNHEADERCELL_horizontalTextMarginLeft = 2;
        private const byte DATAGRIDVIEWCOLUMNHEADERCELL_horizontalTextMarginRight = 2;
        private const byte DATAGRIDVIEWCOLUMNHEADERCELL_sortGlyphHeight = 7;
        private const byte DATAGRIDVIEWCOLUMNHEADERCELL_sortGlyphHorizontalMargin = 4;
        private const byte DATAGRIDVIEWCOLUMNHEADERCELL_sortGlyphSeparatorWidth = 2;
        private const byte DATAGRIDVIEWCOLUMNHEADERCELL_sortGlyphWidth = 9;
        private const byte DATAGRIDVIEWCOLUMNHEADERCELL_verticalMargin = 1;
        private static readonly VisualStyleElement HeaderElement = VisualStyleElement.Header.Item.Normal;
        private SortOrder sortGlyphDirection = SortOrder.None;

        public override object Clone()
        {
            DataGridViewColumnHeaderCell cell;
            System.Type type = base.GetType();
            if (type == cellType)
            {
                cell = new DataGridViewColumnHeaderCell();
            }
            else
            {
                cell = (DataGridViewColumnHeaderCell) Activator.CreateInstance(type);
            }
            base.CloneInternal(cell);
            cell.Value = base.Value;
            return cell;
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new DataGridViewColumnHeaderCellAccessibleObject(this);
        }

        protected override object GetClipboardContent(int rowIndex, bool firstCell, bool lastCell, bool inFirstRow, bool inLastRow, string format)
        {
            if (rowIndex != -1)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            if (base.DataGridView == null)
            {
                return null;
            }
            object obj2 = this.GetValue(rowIndex);
            StringBuilder sb = new StringBuilder(0x40);
            if (string.Equals(format, DataFormats.Html, StringComparison.OrdinalIgnoreCase))
            {
                if (firstCell)
                {
                    sb.Append("<TABLE>");
                    sb.Append("<THEAD>");
                }
                sb.Append("<TH>");
                if (obj2 != null)
                {
                    DataGridViewCell.FormatPlainTextAsHtml(obj2.ToString(), new StringWriter(sb, CultureInfo.CurrentCulture));
                }
                else
                {
                    sb.Append("&nbsp;");
                }
                sb.Append("</TH>");
                if (lastCell)
                {
                    sb.Append("</THEAD>");
                    if (inLastRow)
                    {
                        sb.Append("</TABLE>");
                    }
                }
                return sb.ToString();
            }
            bool csv = string.Equals(format, DataFormats.CommaSeparatedValue, StringComparison.OrdinalIgnoreCase);
            if ((!csv && !string.Equals(format, DataFormats.Text, StringComparison.OrdinalIgnoreCase)) && !string.Equals(format, DataFormats.UnicodeText, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            if (obj2 != null)
            {
                bool escapeApplied = false;
                int length = sb.Length;
                DataGridViewCell.FormatPlainText(obj2.ToString(), csv, new StringWriter(sb, CultureInfo.CurrentCulture), ref escapeApplied);
                if (escapeApplied)
                {
                    sb.Insert(length, '"');
                }
            }
            if (lastCell)
            {
                if (!inLastRow)
                {
                    sb.Append('\r');
                    sb.Append('\n');
                }
            }
            else
            {
                sb.Append(csv ? ',' : '\t');
            }
            return sb.ToString();
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
            if ((base.DataGridView == null) || (base.OwningColumn == null))
            {
                return Rectangle.Empty;
            }
            object formattedValue = this.GetValue(rowIndex);
            base.ComputeBorderStyleCellStateAndCellBounds(rowIndex, out style, out states, out rectangle);
            return this.PaintPrivate(graphics, rectangle, rectangle, rowIndex, states, formattedValue, cellStyle, style, DataGridViewPaintParts.ContentForeground, false);
        }

        public override ContextMenuStrip GetInheritedContextMenuStrip(int rowIndex)
        {
            if (rowIndex != -1)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            ContextMenuStrip contextMenuStrip = base.GetContextMenuStrip(-1);
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

        public override DataGridViewCellStyle GetInheritedStyle(DataGridViewCellStyle inheritedCellStyle, int rowIndex, bool includeColors)
        {
            if (rowIndex != -1)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            DataGridViewCellStyle style = (inheritedCellStyle == null) ? new DataGridViewCellStyle() : inheritedCellStyle;
            DataGridViewCellStyle style2 = null;
            if (base.HasStyle)
            {
                style2 = base.Style;
            }
            DataGridViewCellStyle columnHeadersDefaultCellStyle = base.DataGridView.ColumnHeadersDefaultCellStyle;
            DataGridViewCellStyle defaultCellStyle = base.DataGridView.DefaultCellStyle;
            if (includeColors)
            {
                if ((style2 != null) && !style2.BackColor.IsEmpty)
                {
                    style.BackColor = style2.BackColor;
                }
                else if (!columnHeadersDefaultCellStyle.BackColor.IsEmpty)
                {
                    style.BackColor = columnHeadersDefaultCellStyle.BackColor;
                }
                else
                {
                    style.BackColor = defaultCellStyle.BackColor;
                }
                if ((style2 != null) && !style2.ForeColor.IsEmpty)
                {
                    style.ForeColor = style2.ForeColor;
                }
                else if (!columnHeadersDefaultCellStyle.ForeColor.IsEmpty)
                {
                    style.ForeColor = columnHeadersDefaultCellStyle.ForeColor;
                }
                else
                {
                    style.ForeColor = defaultCellStyle.ForeColor;
                }
                if ((style2 != null) && !style2.SelectionBackColor.IsEmpty)
                {
                    style.SelectionBackColor = style2.SelectionBackColor;
                }
                else if (!columnHeadersDefaultCellStyle.SelectionBackColor.IsEmpty)
                {
                    style.SelectionBackColor = columnHeadersDefaultCellStyle.SelectionBackColor;
                }
                else
                {
                    style.SelectionBackColor = defaultCellStyle.SelectionBackColor;
                }
                if ((style2 != null) && !style2.SelectionForeColor.IsEmpty)
                {
                    style.SelectionForeColor = style2.SelectionForeColor;
                }
                else if (!columnHeadersDefaultCellStyle.SelectionForeColor.IsEmpty)
                {
                    style.SelectionForeColor = columnHeadersDefaultCellStyle.SelectionForeColor;
                }
                else
                {
                    style.SelectionForeColor = defaultCellStyle.SelectionForeColor;
                }
            }
            if ((style2 != null) && (style2.Font != null))
            {
                style.Font = style2.Font;
            }
            else if (columnHeadersDefaultCellStyle.Font != null)
            {
                style.Font = columnHeadersDefaultCellStyle.Font;
            }
            else
            {
                style.Font = defaultCellStyle.Font;
            }
            if ((style2 != null) && !style2.IsNullValueDefault)
            {
                style.NullValue = style2.NullValue;
            }
            else if (!columnHeadersDefaultCellStyle.IsNullValueDefault)
            {
                style.NullValue = columnHeadersDefaultCellStyle.NullValue;
            }
            else
            {
                style.NullValue = defaultCellStyle.NullValue;
            }
            if ((style2 != null) && !style2.IsDataSourceNullValueDefault)
            {
                style.DataSourceNullValue = style2.DataSourceNullValue;
            }
            else if (!columnHeadersDefaultCellStyle.IsDataSourceNullValueDefault)
            {
                style.DataSourceNullValue = columnHeadersDefaultCellStyle.DataSourceNullValue;
            }
            else
            {
                style.DataSourceNullValue = defaultCellStyle.DataSourceNullValue;
            }
            if ((style2 != null) && (style2.Format.Length != 0))
            {
                style.Format = style2.Format;
            }
            else if (columnHeadersDefaultCellStyle.Format.Length != 0)
            {
                style.Format = columnHeadersDefaultCellStyle.Format;
            }
            else
            {
                style.Format = defaultCellStyle.Format;
            }
            if ((style2 != null) && !style2.IsFormatProviderDefault)
            {
                style.FormatProvider = style2.FormatProvider;
            }
            else if (!columnHeadersDefaultCellStyle.IsFormatProviderDefault)
            {
                style.FormatProvider = columnHeadersDefaultCellStyle.FormatProvider;
            }
            else
            {
                style.FormatProvider = defaultCellStyle.FormatProvider;
            }
            if ((style2 != null) && (style2.Alignment != DataGridViewContentAlignment.NotSet))
            {
                style.AlignmentInternal = style2.Alignment;
            }
            else if (columnHeadersDefaultCellStyle.Alignment != DataGridViewContentAlignment.NotSet)
            {
                style.AlignmentInternal = columnHeadersDefaultCellStyle.Alignment;
            }
            else
            {
                style.AlignmentInternal = defaultCellStyle.Alignment;
            }
            if ((style2 != null) && (style2.WrapMode != DataGridViewTriState.NotSet))
            {
                style.WrapModeInternal = style2.WrapMode;
            }
            else if (columnHeadersDefaultCellStyle.WrapMode != DataGridViewTriState.NotSet)
            {
                style.WrapModeInternal = columnHeadersDefaultCellStyle.WrapMode;
            }
            else
            {
                style.WrapModeInternal = defaultCellStyle.WrapMode;
            }
            if ((style2 != null) && (style2.Tag != null))
            {
                style.Tag = style2.Tag;
            }
            else if (columnHeadersDefaultCellStyle.Tag != null)
            {
                style.Tag = columnHeadersDefaultCellStyle.Tag;
            }
            else
            {
                style.Tag = defaultCellStyle.Tag;
            }
            if ((style2 != null) && (style2.Padding != Padding.Empty))
            {
                style.PaddingInternal = style2.Padding;
                return style;
            }
            if (columnHeadersDefaultCellStyle.Padding != Padding.Empty)
            {
                style.PaddingInternal = columnHeadersDefaultCellStyle.Padding;
                return style;
            }
            style.PaddingInternal = defaultCellStyle.Padding;
            return style;
        }

        protected override Size GetPreferredSize(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
        {
            Size size;
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
            DataGridViewFreeDimension freeDimensionFromConstraint = DataGridViewCell.GetFreeDimensionFromConstraint(constraintSize);
            DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder = new DataGridViewAdvancedBorderStyle();
            DataGridViewAdvancedBorderStyle advancedBorderStyle = base.DataGridView.AdjustColumnHeaderBorderStyle(base.DataGridView.AdvancedColumnHeadersBorderStyle, dataGridViewAdvancedBorderStylePlaceholder, false, false);
            Rectangle rectangle = this.BorderWidths(advancedBorderStyle);
            int num = (rectangle.Left + rectangle.Width) + cellStyle.Padding.Horizontal;
            int num2 = (rectangle.Top + rectangle.Height) + cellStyle.Padding.Vertical;
            TextFormatFlags flags = DataGridViewUtilities.ComputeTextFormatFlagsForCellStyleAlignment(base.DataGridView.RightToLeftInternal, cellStyle.Alignment, cellStyle.WrapMode);
            string str = this.GetValue(rowIndex) as string;
            switch (freeDimensionFromConstraint)
            {
                case DataGridViewFreeDimension.Height:
                {
                    Size empty;
                    int num3 = constraintSize.Width - num;
                    size = new Size(0, 0);
                    if (((num3 < 0x11) || (base.OwningColumn == null)) || (base.OwningColumn.SortMode == DataGridViewColumnSortMode.NotSortable))
                    {
                        empty = Size.Empty;
                    }
                    else
                    {
                        empty = new Size(0x11, 7);
                    }
                    if ((((num3 - 2) - 2) > 0) && !string.IsNullOrEmpty(str))
                    {
                        if (cellStyle.WrapMode == DataGridViewTriState.True)
                        {
                            if ((empty.Width > 0) && (((((num3 - 2) - 2) - 2) - empty.Width) > 0))
                            {
                                size = new Size(0, DataGridViewCell.MeasureTextHeight(graphics, str, cellStyle.Font, (((num3 - 2) - 2) - 2) - empty.Width, flags));
                            }
                            else
                            {
                                size = new Size(0, DataGridViewCell.MeasureTextHeight(graphics, str, cellStyle.Font, (num3 - 2) - 2, flags));
                            }
                        }
                        else
                        {
                            size = new Size(0, DataGridViewCell.MeasureTextSize(graphics, str, cellStyle.Font, flags).Height);
                        }
                    }
                    size.Height = Math.Max(size.Height, empty.Height);
                    size.Height = Math.Max(size.Height, 1);
                    goto Label_0391;
                }
                case DataGridViewFreeDimension.Width:
                    size = new Size(0, 0);
                    if (!string.IsNullOrEmpty(str))
                    {
                        if (cellStyle.WrapMode != DataGridViewTriState.True)
                        {
                            size = new Size(DataGridViewCell.MeasureTextSize(graphics, str, cellStyle.Font, flags).Width, 0);
                            break;
                        }
                        size = new Size(DataGridViewCell.MeasureTextWidth(graphics, str, cellStyle.Font, Math.Max(1, (constraintSize.Height - num2) - 2), flags), 0);
                    }
                    break;

                default:
                    if (!string.IsNullOrEmpty(str))
                    {
                        if (cellStyle.WrapMode == DataGridViewTriState.True)
                        {
                            size = DataGridViewCell.MeasureTextPreferredSize(graphics, str, cellStyle.Font, 5f, flags);
                        }
                        else
                        {
                            size = DataGridViewCell.MeasureTextSize(graphics, str, cellStyle.Font, flags);
                        }
                    }
                    else
                    {
                        size = new Size(0, 0);
                    }
                    if ((base.OwningColumn != null) && (base.OwningColumn.SortMode != DataGridViewColumnSortMode.NotSortable))
                    {
                        size.Width += 0x11;
                        if (!string.IsNullOrEmpty(str))
                        {
                            size.Width += 2;
                        }
                        size.Height = Math.Max(size.Height, 7);
                    }
                    size.Width = Math.Max(size.Width, 1);
                    size.Height = Math.Max(size.Height, 1);
                    goto Label_0391;
            }
            if (((((constraintSize.Height - num2) - 2) > 7) && (base.OwningColumn != null)) && (base.OwningColumn.SortMode != DataGridViewColumnSortMode.NotSortable))
            {
                size.Width += 0x11;
                if (!string.IsNullOrEmpty(str))
                {
                    size.Width += 2;
                }
            }
            size.Width = Math.Max(size.Width, 1);
        Label_0391:
            if (freeDimensionFromConstraint != DataGridViewFreeDimension.Height)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    size.Width += 4;
                }
                size.Width += num;
            }
            if (freeDimensionFromConstraint != DataGridViewFreeDimension.Width)
            {
                size.Height += 2 + num2;
            }
            if (base.DataGridView.ApplyVisualStylesToHeaderCells)
            {
                Rectangle themeMargins = DataGridViewHeaderCell.GetThemeMargins(graphics);
                if (freeDimensionFromConstraint != DataGridViewFreeDimension.Height)
                {
                    size.Width += themeMargins.X + themeMargins.Width;
                }
                if (freeDimensionFromConstraint != DataGridViewFreeDimension.Width)
                {
                    size.Height += themeMargins.Y + themeMargins.Height;
                }
            }
            return size;
        }

        protected override object GetValue(int rowIndex)
        {
            if (rowIndex != -1)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            if (this.ContainsLocalValue)
            {
                return base.Properties.GetObject(DataGridViewCell.PropCellValue);
            }
            if (base.OwningColumn != null)
            {
                return base.OwningColumn.Name;
            }
            return null;
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates dataGridViewElementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            this.PaintPrivate(graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, formattedValue, cellStyle, advancedBorderStyle, paintParts, true);
        }

        private Rectangle PaintPrivate(Graphics g, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates dataGridViewElementState, object formattedValue, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts, bool paint)
        {
            Rectangle empty = Rectangle.Empty;
            if (paint && DataGridViewCell.PaintBorder(paintParts))
            {
                this.PaintBorder(g, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
            }
            Rectangle bounds = cellBounds;
            Rectangle rectangle3 = this.BorderWidths(advancedBorderStyle);
            bounds.Offset(rectangle3.X, rectangle3.Y);
            bounds.Width -= rectangle3.Right;
            bounds.Height -= rectangle3.Bottom;
            Rectangle destRect = bounds;
            bool flag = (dataGridViewElementState & DataGridViewElementStates.Selected) != DataGridViewElementStates.None;
            if (base.DataGridView.ApplyVisualStylesToHeaderCells)
            {
                if ((cellStyle.Padding != Padding.Empty) && (cellStyle.Padding != Padding.Empty))
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
                if ((paint && DataGridViewCell.PaintBackground(paintParts)) && ((destRect.Width > 0) && (destRect.Height > 0)))
                {
                    int headerState = 1;
                    if (((base.OwningColumn != null) && (base.OwningColumn.SortMode != DataGridViewColumnSortMode.NotSortable)) || ((base.DataGridView.SelectionMode == DataGridViewSelectionMode.FullColumnSelect) || (base.DataGridView.SelectionMode == DataGridViewSelectionMode.ColumnHeaderSelect)))
                    {
                        if (base.ButtonState != ButtonState.Normal)
                        {
                            headerState = 3;
                        }
                        else if ((base.DataGridView.MouseEnteredCellAddress.Y == rowIndex) && (base.DataGridView.MouseEnteredCellAddress.X == base.ColumnIndex))
                        {
                            headerState = 2;
                        }
                        else if (flag)
                        {
                            headerState = 3;
                        }
                    }
                    if (base.DataGridView.RightToLeftInternal)
                    {
                        Bitmap flipXPThemesBitmap = base.FlipXPThemesBitmap;
                        if (((flipXPThemesBitmap == null) || (flipXPThemesBitmap.Width < destRect.Width)) || (((flipXPThemesBitmap.Width > (2 * destRect.Width)) || (flipXPThemesBitmap.Height < destRect.Height)) || (flipXPThemesBitmap.Height > (2 * destRect.Height))))
                        {
                            flipXPThemesBitmap = base.FlipXPThemesBitmap = new Bitmap(destRect.Width, destRect.Height);
                        }
                        DataGridViewColumnHeaderCellRenderer.DrawHeader(Graphics.FromImage(flipXPThemesBitmap), new Rectangle(0, 0, destRect.Width, destRect.Height), headerState);
                        flipXPThemesBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        g.DrawImage(flipXPThemesBitmap, destRect, new Rectangle(flipXPThemesBitmap.Width - destRect.Width, 0, destRect.Width, destRect.Height), GraphicsUnit.Pixel);
                    }
                    else
                    {
                        DataGridViewColumnHeaderCellRenderer.DrawHeader(g, destRect, headerState);
                    }
                }
                Rectangle themeMargins = DataGridViewHeaderCell.GetThemeMargins(g);
                bounds.Y += themeMargins.Y;
                bounds.Height -= themeMargins.Y + themeMargins.Height;
                if (base.DataGridView.RightToLeftInternal)
                {
                    bounds.X += themeMargins.Width;
                    bounds.Width -= themeMargins.X + themeMargins.Width;
                }
                else
                {
                    bounds.X += themeMargins.X;
                    bounds.Width -= themeMargins.X + themeMargins.Width;
                }
            }
            else
            {
                if ((paint && DataGridViewCell.PaintBackground(paintParts)) && ((destRect.Width > 0) && (destRect.Height > 0)))
                {
                    SolidBrush cachedBrush = base.DataGridView.GetCachedBrush((DataGridViewCell.PaintSelectionBackground(paintParts) && flag) ? cellStyle.SelectionBackColor : cellStyle.BackColor);
                    if (cachedBrush.Color.A == 0xff)
                    {
                        g.FillRectangle(cachedBrush, destRect);
                    }
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
            }
            bool flag2 = false;
            Point point = new Point(0, 0);
            string str = formattedValue as string;
            bounds.Y++;
            bounds.Height -= 2;
            if (((((bounds.Width - 2) - 2) > 0) && (bounds.Height > 0)) && !string.IsNullOrEmpty(str))
            {
                Color color;
                bounds.Offset(2, 0);
                bounds.Width -= 4;
                if (base.DataGridView.ApplyVisualStylesToHeaderCells)
                {
                    color = DataGridViewColumnHeaderCellRenderer.VisualStyleRenderer.GetColor(ColorProperty.TextColor);
                }
                else
                {
                    color = flag ? cellStyle.SelectionForeColor : cellStyle.ForeColor;
                }
                if ((base.OwningColumn != null) && (base.OwningColumn.SortMode != DataGridViewColumnSortMode.NotSortable))
                {
                    bool flag3;
                    int maxWidth = ((bounds.Width - 2) - 9) - 8;
                    if (((maxWidth > 0) && (DataGridViewCell.GetPreferredTextHeight(g, base.DataGridView.RightToLeftInternal, str, cellStyle, maxWidth, out flag3) <= bounds.Height)) && !flag3)
                    {
                        flag2 = this.SortGlyphDirection != SortOrder.None;
                        bounds.Width -= 0x13;
                        if (base.DataGridView.RightToLeftInternal)
                        {
                            bounds.X += 0x13;
                            point = new Point((((bounds.Left - 2) - 2) - 4) - 9, bounds.Top + ((bounds.Height - 7) / 2));
                        }
                        else
                        {
                            point = new Point(((bounds.Right + 2) + 2) + 4, bounds.Top + ((bounds.Height - 7) / 2));
                        }
                    }
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
                        TextRenderer.DrawText(g, str, cellStyle.Font, bounds, color, flags);
                    }
                }
                else
                {
                    empty = DataGridViewUtilities.GetTextBounds(bounds, str, flags, cellStyle);
                }
            }
            else if ((paint && (this.SortGlyphDirection != SortOrder.None)) && ((bounds.Width >= 0x11) && (bounds.Height >= 7)))
            {
                flag2 = true;
                point = new Point(bounds.Left + ((bounds.Width - 9) / 2), bounds.Top + ((bounds.Height - 7) / 2));
            }
            if ((paint && flag2) && DataGridViewCell.PaintContentBackground(paintParts))
            {
                Pen darkPen = null;
                Pen lightPen = null;
                base.GetContrastedPens(cellStyle.BackColor, ref darkPen, ref lightPen);
                if (this.SortGlyphDirection != SortOrder.Ascending)
                {
                    switch (advancedBorderStyle.Right)
                    {
                        case DataGridViewAdvancedCellBorderStyle.Inset:
                            g.DrawLine(lightPen, point.X, point.Y + 1, (point.X + 4) - 1, (point.Y + 7) - 1);
                            g.DrawLine(lightPen, (int) (point.X + 1), (int) (point.Y + 1), (int) ((point.X + 4) - 1), (int) ((point.Y + 7) - 1));
                            g.DrawLine(darkPen, (int) (point.X + 4), (int) ((point.Y + 7) - 1), (int) ((point.X + 9) - 2), (int) (point.Y + 1));
                            g.DrawLine(darkPen, (int) (point.X + 4), (int) ((point.Y + 7) - 1), (int) ((point.X + 9) - 3), (int) (point.Y + 1));
                            g.DrawLine(darkPen, point.X, point.Y, (point.X + 9) - 2, point.Y);
                            return empty;

                        case DataGridViewAdvancedCellBorderStyle.Outset:
                        case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
                        case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
                            g.DrawLine(darkPen, point.X, point.Y + 1, (point.X + 4) - 1, (point.Y + 7) - 1);
                            g.DrawLine(darkPen, (int) (point.X + 1), (int) (point.Y + 1), (int) ((point.X + 4) - 1), (int) ((point.Y + 7) - 1));
                            g.DrawLine(lightPen, (int) (point.X + 4), (int) ((point.Y + 7) - 1), (int) ((point.X + 9) - 2), (int) (point.Y + 1));
                            g.DrawLine(lightPen, (int) (point.X + 4), (int) ((point.Y + 7) - 1), (int) ((point.X + 9) - 3), (int) (point.Y + 1));
                            g.DrawLine(lightPen, point.X, point.Y, (point.X + 9) - 2, point.Y);
                            return empty;
                    }
                    for (int j = 0; j < 4; j++)
                    {
                        g.DrawLine(darkPen, (int) (point.X + j), (int) ((point.Y + j) + 2), (int) (((point.X + 9) - j) - 1), (int) ((point.Y + j) + 2));
                    }
                    g.DrawLine(darkPen, (int) (point.X + 4), (int) ((point.Y + 4) + 1), (int) (point.X + 4), (int) ((point.Y + 4) + 2));
                    return empty;
                }
                switch (advancedBorderStyle.Right)
                {
                    case DataGridViewAdvancedCellBorderStyle.Inset:
                        g.DrawLine(lightPen, point.X, (point.Y + 7) - 2, (point.X + 4) - 1, point.Y);
                        g.DrawLine(lightPen, point.X + 1, (point.Y + 7) - 2, (point.X + 4) - 1, point.Y);
                        g.DrawLine(darkPen, point.X + 4, point.Y, (point.X + 9) - 2, (point.Y + 7) - 2);
                        g.DrawLine(darkPen, point.X + 4, point.Y, (point.X + 9) - 3, (point.Y + 7) - 2);
                        g.DrawLine(darkPen, point.X, (point.Y + 7) - 1, (point.X + 9) - 2, (point.Y + 7) - 1);
                        return empty;

                    case DataGridViewAdvancedCellBorderStyle.Outset:
                    case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
                    case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
                        g.DrawLine(darkPen, point.X, (point.Y + 7) - 2, (point.X + 4) - 1, point.Y);
                        g.DrawLine(darkPen, point.X + 1, (point.Y + 7) - 2, (point.X + 4) - 1, point.Y);
                        g.DrawLine(lightPen, point.X + 4, point.Y, (point.X + 9) - 2, (point.Y + 7) - 2);
                        g.DrawLine(lightPen, point.X + 4, point.Y, (point.X + 9) - 3, (point.Y + 7) - 2);
                        g.DrawLine(lightPen, point.X, (point.Y + 7) - 1, (point.X + 9) - 2, (point.Y + 7) - 1);
                        return empty;
                }
                for (int i = 0; i < 4; i++)
                {
                    g.DrawLine(darkPen, (int) (point.X + i), (int) (((point.Y + 7) - i) - 1), (int) (((point.X + 9) - i) - 1), (int) (((point.Y + 7) - i) - 1));
                }
                g.DrawLine(darkPen, (int) (point.X + 4), (int) (((point.Y + 7) - 4) - 1), (int) (point.X + 4), (int) ((point.Y + 7) - 4));
            }
            return empty;
        }

        protected override bool SetValue(int rowIndex, object value)
        {
            if (rowIndex != -1)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            object obj2 = this.GetValue(rowIndex);
            base.Properties.SetObject(DataGridViewCell.PropCellValue, value);
            if ((base.DataGridView != null) && (obj2 != value))
            {
                base.RaiseCellValueChanged(new DataGridViewCellEventArgs(base.ColumnIndex, -1));
            }
            return true;
        }

        public override string ToString()
        {
            return ("DataGridViewColumnHeaderCell { ColumnIndex=" + base.ColumnIndex.ToString(CultureInfo.CurrentCulture) + " }");
        }

        internal bool ContainsLocalValue
        {
            get
            {
                return base.Properties.ContainsObject(DataGridViewCell.PropCellValue);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SortOrder SortGlyphDirection
        {
            get
            {
                return this.sortGlyphDirection;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(SortOrder));
                }
                if ((base.OwningColumn == null) || (base.DataGridView == null))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_CellDoesNotYetBelongToDataGridView"));
                }
                if (value != this.sortGlyphDirection)
                {
                    if ((base.OwningColumn.SortMode == DataGridViewColumnSortMode.NotSortable) && (value != SortOrder.None))
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumnHeaderCell_SortModeAndSortGlyphDirectionClash", new object[] { value.ToString() }));
                    }
                    this.sortGlyphDirection = value;
                    base.DataGridView.OnSortGlyphDirectionChanged(this);
                }
            }
        }

        internal SortOrder SortGlyphDirectionInternal
        {
            set
            {
                this.sortGlyphDirection = value;
            }
        }

        protected class DataGridViewColumnHeaderCellAccessibleObject : DataGridViewCell.DataGridViewCellAccessibleObject
        {
            public DataGridViewColumnHeaderCellAccessibleObject(DataGridViewColumnHeaderCell owner) : base(owner)
            {
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                DataGridViewColumnHeaderCell owner = (DataGridViewColumnHeaderCell) base.Owner;
                DataGridView dataGridView = owner.DataGridView;
                if (owner.OwningColumn != null)
                {
                    if (owner.OwningColumn.SortMode == DataGridViewColumnSortMode.Automatic)
                    {
                        ListSortDirection ascending = ListSortDirection.Ascending;
                        if ((dataGridView.SortedColumn == owner.OwningColumn) && (dataGridView.SortOrder == SortOrder.Ascending))
                        {
                            ascending = ListSortDirection.Descending;
                        }
                        dataGridView.Sort(owner.OwningColumn, ascending);
                    }
                    else if ((dataGridView.SelectionMode == DataGridViewSelectionMode.FullColumnSelect) || (dataGridView.SelectionMode == DataGridViewSelectionMode.ColumnHeaderSelect))
                    {
                        owner.OwningColumn.Selected = true;
                    }
                }
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation navigationDirection)
            {
                if (base.Owner.OwningColumn != null)
                {
                    switch (navigationDirection)
                    {
                        case AccessibleNavigation.Left:
                            if (base.Owner.DataGridView.RightToLeft != RightToLeft.No)
                            {
                                return this.NavigateForward();
                            }
                            return this.NavigateBackward();

                        case AccessibleNavigation.Right:
                            if (base.Owner.DataGridView.RightToLeft != RightToLeft.No)
                            {
                                return this.NavigateBackward();
                            }
                            return this.NavigateForward();

                        case AccessibleNavigation.Next:
                            return this.NavigateForward();

                        case AccessibleNavigation.Previous:
                            return this.NavigateBackward();

                        case AccessibleNavigation.FirstChild:
                            return base.Owner.DataGridView.AccessibilityObject.GetChild(0).GetChild(0);

                        case AccessibleNavigation.LastChild:
                        {
                            AccessibleObject child = base.Owner.DataGridView.AccessibilityObject.GetChild(0);
                            return child.GetChild(child.GetChildCount() - 1);
                        }
                    }
                }
                return null;
            }

            private AccessibleObject NavigateBackward()
            {
                if (base.Owner.OwningColumn == base.Owner.DataGridView.Columns.GetFirstColumn(DataGridViewElementStates.Visible))
                {
                    if (base.Owner.DataGridView.RowHeadersVisible)
                    {
                        return this.Parent.GetChild(0);
                    }
                    return null;
                }
                int index = base.Owner.DataGridView.Columns.GetPreviousColumn(base.Owner.OwningColumn, DataGridViewElementStates.Visible, DataGridViewElementStates.None).Index;
                int num2 = base.Owner.DataGridView.Columns.ColumnIndexToActualDisplayIndex(index, DataGridViewElementStates.Visible);
                if (base.Owner.DataGridView.RowHeadersVisible)
                {
                    return this.Parent.GetChild(num2 + 1);
                }
                return this.Parent.GetChild(num2);
            }

            private AccessibleObject NavigateForward()
            {
                if (base.Owner.OwningColumn == base.Owner.DataGridView.Columns.GetLastColumn(DataGridViewElementStates.Visible, DataGridViewElementStates.None))
                {
                    return null;
                }
                int index = base.Owner.DataGridView.Columns.GetNextColumn(base.Owner.OwningColumn, DataGridViewElementStates.Visible, DataGridViewElementStates.None).Index;
                int num2 = base.Owner.DataGridView.Columns.ColumnIndexToActualDisplayIndex(index, DataGridViewElementStates.Visible);
                if (base.Owner.DataGridView.RowHeadersVisible)
                {
                    return this.Parent.GetChild(num2 + 1);
                }
                return this.Parent.GetChild(num2);
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void Select(AccessibleSelection flags)
            {
                if (base.Owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellAccessibleObject_OwnerNotSet"));
                }
                DataGridViewColumnHeaderCell owner = (DataGridViewColumnHeaderCell) base.Owner;
                DataGridView dataGridView = owner.DataGridView;
                if (dataGridView != null)
                {
                    if ((flags & AccessibleSelection.TakeFocus) == AccessibleSelection.TakeFocus)
                    {
                        dataGridView.FocusInternal();
                    }
                    if ((owner.OwningColumn != null) && ((dataGridView.SelectionMode == DataGridViewSelectionMode.FullColumnSelect) || (dataGridView.SelectionMode == DataGridViewSelectionMode.ColumnHeaderSelect)))
                    {
                        if ((flags & (AccessibleSelection.AddSelection | AccessibleSelection.TakeSelection)) != AccessibleSelection.None)
                        {
                            owner.OwningColumn.Selected = true;
                        }
                        else if ((flags & AccessibleSelection.RemoveSelection) == AccessibleSelection.RemoveSelection)
                        {
                            owner.OwningColumn.Selected = false;
                        }
                    }
                }
            }

            public override Rectangle Bounds
            {
                get
                {
                    return base.GetAccessibleObjectBounds(this.ParentPrivate);
                }
            }

            public override string DefaultAction
            {
                get
                {
                    if (base.Owner.OwningColumn == null)
                    {
                        return string.Empty;
                    }
                    if (base.Owner.OwningColumn.SortMode == DataGridViewColumnSortMode.Automatic)
                    {
                        return System.Windows.Forms.SR.GetString("DataGridView_AccColumnHeaderCellDefaultAction");
                    }
                    if ((base.Owner.DataGridView.SelectionMode != DataGridViewSelectionMode.FullColumnSelect) && (base.Owner.DataGridView.SelectionMode != DataGridViewSelectionMode.ColumnHeaderSelect))
                    {
                        return string.Empty;
                    }
                    return System.Windows.Forms.SR.GetString("DataGridView_AccColumnHeaderCellSelectDefaultAction");
                }
            }

            public override string Name
            {
                get
                {
                    if (base.Owner.OwningColumn != null)
                    {
                        return base.Owner.OwningColumn.HeaderText;
                    }
                    return string.Empty;
                }
            }

            public override AccessibleObject Parent
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return this.ParentPrivate;
                }
            }

            private AccessibleObject ParentPrivate
            {
                get
                {
                    return base.Owner.DataGridView.AccessibilityObject.GetChild(0);
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.ColumnHeader;
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
                    if (((base.Owner.DataGridView.SelectionMode == DataGridViewSelectionMode.FullColumnSelect) || (base.Owner.DataGridView.SelectionMode == DataGridViewSelectionMode.ColumnHeaderSelect)) && ((base.Owner.OwningColumn != null) && base.Owner.OwningColumn.Selected))
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
                    return this.Name;
                }
            }
        }

        private class DataGridViewColumnHeaderCellRenderer
        {
            private static System.Windows.Forms.VisualStyles.VisualStyleRenderer visualStyleRenderer;

            private DataGridViewColumnHeaderCellRenderer()
            {
            }

            public static void DrawHeader(Graphics g, Rectangle bounds, int headerState)
            {
                Rectangle rect = Rectangle.Truncate(g.ClipBounds);
                if (2 == headerState)
                {
                    VisualStyleRenderer.SetParameters(DataGridViewColumnHeaderCell.HeaderElement);
                    Rectangle clipRectangle = new Rectangle(bounds.Left, bounds.Bottom - 2, 2, 2);
                    clipRectangle.Intersect(rect);
                    VisualStyleRenderer.DrawBackground(g, bounds, clipRectangle);
                    clipRectangle = new Rectangle(bounds.Right - 2, bounds.Bottom - 2, 2, 2);
                    clipRectangle.Intersect(rect);
                    VisualStyleRenderer.DrawBackground(g, bounds, clipRectangle);
                }
                VisualStyleRenderer.SetParameters(DataGridViewColumnHeaderCell.HeaderElement.ClassName, DataGridViewColumnHeaderCell.HeaderElement.Part, headerState);
                VisualStyleRenderer.DrawBackground(g, bounds, rect);
            }

            public static System.Windows.Forms.VisualStyles.VisualStyleRenderer VisualStyleRenderer
            {
                get
                {
                    if (visualStyleRenderer == null)
                    {
                        visualStyleRenderer = new System.Windows.Forms.VisualStyles.VisualStyleRenderer(DataGridViewColumnHeaderCell.HeaderElement);
                    }
                    return visualStyleRenderer;
                }
            }
        }
    }
}

