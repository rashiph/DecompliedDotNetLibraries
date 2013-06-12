namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Forms.VisualStyles;

    public class DataGridViewRowHeaderCell : DataGridViewHeaderCell
    {
        private static System.Type cellType = typeof(DataGridViewRowHeaderCell);
        private static ColorMap[] colorMap = new ColorMap[] { new ColorMap() };
        private const byte DATAGRIDVIEWROWHEADERCELL_contentMarginHeight = 3;
        private const byte DATAGRIDVIEWROWHEADERCELL_contentMarginWidth = 3;
        private const byte DATAGRIDVIEWROWHEADERCELL_horizontalTextMarginLeft = 1;
        private const byte DATAGRIDVIEWROWHEADERCELL_horizontalTextMarginRight = 2;
        private const byte DATAGRIDVIEWROWHEADERCELL_iconMarginHeight = 2;
        private const byte DATAGRIDVIEWROWHEADERCELL_iconMarginWidth = 3;
        private const byte DATAGRIDVIEWROWHEADERCELL_iconsHeight = 11;
        private const byte DATAGRIDVIEWROWHEADERCELL_iconsWidth = 12;
        private const byte DATAGRIDVIEWROWHEADERCELL_verticalTextMargin = 1;
        private static readonly VisualStyleElement HeaderElement = VisualStyleElement.Header.Item.Normal;
        private static Bitmap leftArrowBmp = null;
        private static Bitmap leftArrowStarBmp;
        private static Bitmap pencilLTRBmp = null;
        private static Bitmap pencilRTLBmp = null;
        private static Bitmap rightArrowBmp = null;
        private static Bitmap rightArrowStarBmp;
        private static Bitmap starBmp = null;

        public override object Clone()
        {
            DataGridViewRowHeaderCell cell;
            System.Type type = base.GetType();
            if (type == cellType)
            {
                cell = new DataGridViewRowHeaderCell();
            }
            else
            {
                cell = (DataGridViewRowHeaderCell) Activator.CreateInstance(type);
            }
            base.CloneInternal(cell);
            cell.Value = base.Value;
            return cell;
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new DataGridViewRowHeaderCellAccessibleObject(this);
        }

        private static Bitmap GetArrowBitmap(bool rightToLeft)
        {
            if (!rightToLeft)
            {
                return RightArrowBitmap;
            }
            return LeftArrowBitmap;
        }

        private static Bitmap GetArrowStarBitmap(bool rightToLeft)
        {
            if (!rightToLeft)
            {
                return RightArrowStarBitmap;
            }
            return LeftArrowStarBitmap;
        }

        private static Bitmap GetBitmap(string bitmapName)
        {
            Bitmap bitmap = new Bitmap(typeof(DataGridViewRowHeaderCell), bitmapName);
            bitmap.MakeTransparent();
            return bitmap;
        }

        protected override object GetClipboardContent(int rowIndex, bool firstCell, bool lastCell, bool inFirstRow, bool inLastRow, string format)
        {
            if (base.DataGridView == null)
            {
                return null;
            }
            if ((rowIndex < 0) || (rowIndex >= base.DataGridView.Rows.Count))
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            object obj2 = this.GetValue(rowIndex);
            StringBuilder sb = new StringBuilder(0x40);
            if (string.Equals(format, DataFormats.Html, StringComparison.OrdinalIgnoreCase))
            {
                if (inFirstRow)
                {
                    sb.Append("<TABLE>");
                }
                sb.Append("<TR>");
                sb.Append("<TD ALIGN=\"center\">");
                if (obj2 != null)
                {
                    sb.Append("<B>");
                    DataGridViewCell.FormatPlainTextAsHtml(obj2.ToString(), new StringWriter(sb, CultureInfo.CurrentCulture));
                    sb.Append("</B>");
                }
                else
                {
                    sb.Append("&nbsp;");
                }
                sb.Append("</TD>");
                if (lastCell)
                {
                    sb.Append("</TR>");
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
            if ((base.DataGridView == null) || (base.OwningRow == null))
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
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            if (((base.DataGridView == null) || (rowIndex < 0)) || (!base.DataGridView.ShowRowErrors || string.IsNullOrEmpty(this.GetErrorText(rowIndex))))
            {
                return Rectangle.Empty;
            }
            base.ComputeBorderStyleCellStateAndCellBounds(rowIndex, out style, out states, out rectangle);
            object obj2 = this.GetValue(rowIndex);
            object formattedValue = this.GetFormattedValue(obj2, rowIndex, ref cellStyle, null, null, DataGridViewDataErrorContexts.Formatting);
            return this.PaintPrivate(graphics, rectangle, rectangle, rowIndex, states, formattedValue, this.GetErrorText(rowIndex), cellStyle, style, DataGridViewPaintParts.ContentForeground, false, true, false);
        }

        protected internal override string GetErrorText(int rowIndex)
        {
            if (base.OwningRow == null)
            {
                return base.GetErrorText(rowIndex);
            }
            return base.OwningRow.GetErrorText(rowIndex);
        }

        public override ContextMenuStrip GetInheritedContextMenuStrip(int rowIndex)
        {
            if ((base.DataGridView != null) && ((rowIndex < 0) || (rowIndex >= base.DataGridView.Rows.Count)))
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
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

        public override DataGridViewCellStyle GetInheritedStyle(DataGridViewCellStyle inheritedCellStyle, int rowIndex, bool includeColors)
        {
            DataGridViewCellStyle style = (inheritedCellStyle == null) ? new DataGridViewCellStyle() : inheritedCellStyle;
            DataGridViewCellStyle style2 = null;
            if (base.HasStyle)
            {
                style2 = base.Style;
            }
            DataGridViewCellStyle rowHeadersDefaultCellStyle = base.DataGridView.RowHeadersDefaultCellStyle;
            DataGridViewCellStyle defaultCellStyle = base.DataGridView.DefaultCellStyle;
            if (includeColors)
            {
                if ((style2 != null) && !style2.BackColor.IsEmpty)
                {
                    style.BackColor = style2.BackColor;
                }
                else if (!rowHeadersDefaultCellStyle.BackColor.IsEmpty)
                {
                    style.BackColor = rowHeadersDefaultCellStyle.BackColor;
                }
                else
                {
                    style.BackColor = defaultCellStyle.BackColor;
                }
                if ((style2 != null) && !style2.ForeColor.IsEmpty)
                {
                    style.ForeColor = style2.ForeColor;
                }
                else if (!rowHeadersDefaultCellStyle.ForeColor.IsEmpty)
                {
                    style.ForeColor = rowHeadersDefaultCellStyle.ForeColor;
                }
                else
                {
                    style.ForeColor = defaultCellStyle.ForeColor;
                }
                if ((style2 != null) && !style2.SelectionBackColor.IsEmpty)
                {
                    style.SelectionBackColor = style2.SelectionBackColor;
                }
                else if (!rowHeadersDefaultCellStyle.SelectionBackColor.IsEmpty)
                {
                    style.SelectionBackColor = rowHeadersDefaultCellStyle.SelectionBackColor;
                }
                else
                {
                    style.SelectionBackColor = defaultCellStyle.SelectionBackColor;
                }
                if ((style2 != null) && !style2.SelectionForeColor.IsEmpty)
                {
                    style.SelectionForeColor = style2.SelectionForeColor;
                }
                else if (!rowHeadersDefaultCellStyle.SelectionForeColor.IsEmpty)
                {
                    style.SelectionForeColor = rowHeadersDefaultCellStyle.SelectionForeColor;
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
            else if (rowHeadersDefaultCellStyle.Font != null)
            {
                style.Font = rowHeadersDefaultCellStyle.Font;
            }
            else
            {
                style.Font = defaultCellStyle.Font;
            }
            if ((style2 != null) && !style2.IsNullValueDefault)
            {
                style.NullValue = style2.NullValue;
            }
            else if (!rowHeadersDefaultCellStyle.IsNullValueDefault)
            {
                style.NullValue = rowHeadersDefaultCellStyle.NullValue;
            }
            else
            {
                style.NullValue = defaultCellStyle.NullValue;
            }
            if ((style2 != null) && !style2.IsDataSourceNullValueDefault)
            {
                style.DataSourceNullValue = style2.DataSourceNullValue;
            }
            else if (!rowHeadersDefaultCellStyle.IsDataSourceNullValueDefault)
            {
                style.DataSourceNullValue = rowHeadersDefaultCellStyle.DataSourceNullValue;
            }
            else
            {
                style.DataSourceNullValue = defaultCellStyle.DataSourceNullValue;
            }
            if ((style2 != null) && (style2.Format.Length != 0))
            {
                style.Format = style2.Format;
            }
            else if (rowHeadersDefaultCellStyle.Format.Length != 0)
            {
                style.Format = rowHeadersDefaultCellStyle.Format;
            }
            else
            {
                style.Format = defaultCellStyle.Format;
            }
            if ((style2 != null) && !style2.IsFormatProviderDefault)
            {
                style.FormatProvider = style2.FormatProvider;
            }
            else if (!rowHeadersDefaultCellStyle.IsFormatProviderDefault)
            {
                style.FormatProvider = rowHeadersDefaultCellStyle.FormatProvider;
            }
            else
            {
                style.FormatProvider = defaultCellStyle.FormatProvider;
            }
            if ((style2 != null) && (style2.Alignment != DataGridViewContentAlignment.NotSet))
            {
                style.AlignmentInternal = style2.Alignment;
            }
            else if (rowHeadersDefaultCellStyle.Alignment != DataGridViewContentAlignment.NotSet)
            {
                style.AlignmentInternal = rowHeadersDefaultCellStyle.Alignment;
            }
            else
            {
                style.AlignmentInternal = defaultCellStyle.Alignment;
            }
            if ((style2 != null) && (style2.WrapMode != DataGridViewTriState.NotSet))
            {
                style.WrapModeInternal = style2.WrapMode;
            }
            else if (rowHeadersDefaultCellStyle.WrapMode != DataGridViewTriState.NotSet)
            {
                style.WrapModeInternal = rowHeadersDefaultCellStyle.WrapMode;
            }
            else
            {
                style.WrapModeInternal = defaultCellStyle.WrapMode;
            }
            if ((style2 != null) && (style2.Tag != null))
            {
                style.Tag = style2.Tag;
            }
            else if (rowHeadersDefaultCellStyle.Tag != null)
            {
                style.Tag = rowHeadersDefaultCellStyle.Tag;
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
            if (rowHeadersDefaultCellStyle.Padding != Padding.Empty)
            {
                style.PaddingInternal = rowHeadersDefaultCellStyle.Padding;
                return style;
            }
            style.PaddingInternal = defaultCellStyle.Padding;
            return style;
        }

        private static Bitmap GetPencilBitmap(bool rightToLeft)
        {
            if (!rightToLeft)
            {
                return PencilLTRBitmap;
            }
            return PencilRTLBitmap;
        }

        protected override Size GetPreferredSize(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
        {
            if (base.DataGridView == null)
            {
                return new Size(-1, -1);
            }
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder = new DataGridViewAdvancedBorderStyle();
            DataGridViewAdvancedBorderStyle advancedBorderStyle = base.OwningRow.AdjustRowHeaderBorderStyle(base.DataGridView.AdvancedRowHeadersBorderStyle, dataGridViewAdvancedBorderStylePlaceholder, false, false, false, false);
            Rectangle rectangle = this.BorderWidths(advancedBorderStyle);
            int borderAndPaddingWidths = (rectangle.Left + rectangle.Width) + cellStyle.Padding.Horizontal;
            int borderAndPaddingHeights = (rectangle.Top + rectangle.Height) + cellStyle.Padding.Vertical;
            TextFormatFlags flags = DataGridViewUtilities.ComputeTextFormatFlagsForCellStyleAlignment(base.DataGridView.RightToLeftInternal, cellStyle.Alignment, cellStyle.WrapMode);
            if (base.DataGridView.ApplyVisualStylesToHeaderCells)
            {
                Rectangle themeMargins = DataGridViewHeaderCell.GetThemeMargins(graphics);
                borderAndPaddingWidths += themeMargins.Y;
                borderAndPaddingWidths += themeMargins.Height;
                borderAndPaddingHeights += themeMargins.X;
                borderAndPaddingHeights += themeMargins.Width;
            }
            object obj2 = this.GetValue(rowIndex);
            if (!(obj2 is string))
            {
                obj2 = null;
            }
            return DataGridViewUtilities.GetPreferredRowHeaderSize(graphics, (string) obj2, cellStyle, borderAndPaddingWidths, borderAndPaddingHeights, base.DataGridView.ShowRowErrors, true, constraintSize, flags);
        }

        protected override object GetValue(int rowIndex)
        {
            if ((base.DataGridView != null) && ((rowIndex < -1) || (rowIndex >= base.DataGridView.Rows.Count)))
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            return base.Properties.GetObject(DataGridViewCell.PropCellValue);
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            this.PaintPrivate(graphics, clipBounds, cellBounds, rowIndex, cellState, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts, false, false, true);
        }

        private void PaintIcon(Graphics g, Bitmap bmp, Rectangle bounds, Color foreColor)
        {
            Rectangle destRect = new Rectangle(base.DataGridView.RightToLeftInternal ? ((bounds.Right - 3) - 12) : (bounds.Left + 3), bounds.Y + ((bounds.Height - 11) / 2), 12, 11);
            colorMap[0].NewColor = foreColor;
            colorMap[0].OldColor = Color.Black;
            ImageAttributes imageAttr = new ImageAttributes();
            imageAttr.SetRemapTable(colorMap, ColorAdjustType.Bitmap);
            g.DrawImage(bmp, destRect, 0, 0, 12, 11, GraphicsUnit.Pixel, imageAttr);
            imageAttr.Dispose();
        }

        private Rectangle PaintPrivate(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates dataGridViewElementState, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts, bool computeContentBounds, bool computeErrorIconBounds, bool paint)
        {
            Rectangle empty = Rectangle.Empty;
            if (paint && DataGridViewCell.PaintBorder(paintParts))
            {
                this.PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
            }
            Rectangle rect = cellBounds;
            Rectangle rectangle3 = this.BorderWidths(advancedBorderStyle);
            rect.Offset(rectangle3.X, rectangle3.Y);
            rect.Width -= rectangle3.Right;
            rect.Height -= rectangle3.Bottom;
            Rectangle destRect = rect;
            bool flag = (dataGridViewElementState & DataGridViewElementStates.Selected) != DataGridViewElementStates.None;
            if (base.DataGridView.ApplyVisualStylesToHeaderCells)
            {
                if (cellStyle.Padding != Padding.Empty)
                {
                    if (base.DataGridView.RightToLeftInternal)
                    {
                        rect.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
                    }
                    else
                    {
                        rect.Offset(cellStyle.Padding.Left, cellStyle.Padding.Top);
                    }
                    rect.Width -= cellStyle.Padding.Horizontal;
                    rect.Height -= cellStyle.Padding.Vertical;
                }
                if ((destRect.Width > 0) && (destRect.Height > 0))
                {
                    if (paint && DataGridViewCell.PaintBackground(paintParts))
                    {
                        int headerState = 1;
                        if ((base.DataGridView.SelectionMode == DataGridViewSelectionMode.FullRowSelect) || (base.DataGridView.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect))
                        {
                            if (base.ButtonState != ButtonState.Normal)
                            {
                                headerState = 3;
                            }
                            else if ((base.DataGridView.MouseEnteredCellAddress.Y == rowIndex) && (base.DataGridView.MouseEnteredCellAddress.X == -1))
                            {
                                headerState = 2;
                            }
                            else if (flag)
                            {
                                headerState = 3;
                            }
                        }
                        using (Bitmap bitmap = new Bitmap(destRect.Height, destRect.Width))
                        {
                            using (Graphics graphics2 = Graphics.FromImage(bitmap))
                            {
                                DataGridViewRowHeaderCellRenderer.DrawHeader(graphics2, new Rectangle(0, 0, destRect.Height, destRect.Width), headerState);
                                bitmap.RotateFlip(base.DataGridView.RightToLeftInternal ? RotateFlipType.Rotate90FlipNone : RotateFlipType.Rotate90FlipX);
                                graphics.DrawImage(bitmap, destRect, new Rectangle(0, 0, destRect.Width, destRect.Height), GraphicsUnit.Pixel);
                            }
                        }
                    }
                    Rectangle themeMargins = DataGridViewHeaderCell.GetThemeMargins(graphics);
                    if (base.DataGridView.RightToLeftInternal)
                    {
                        rect.X += themeMargins.Height;
                    }
                    else
                    {
                        rect.X += themeMargins.Y;
                    }
                    rect.Width -= themeMargins.Y + themeMargins.Height;
                    rect.Height -= themeMargins.X + themeMargins.Width;
                    rect.Y += themeMargins.X;
                }
            }
            else
            {
                if ((rect.Width > 0) && (rect.Height > 0))
                {
                    SolidBrush cachedBrush = base.DataGridView.GetCachedBrush((DataGridViewCell.PaintSelectionBackground(paintParts) && flag) ? cellStyle.SelectionBackColor : cellStyle.BackColor);
                    if ((paint && DataGridViewCell.PaintBackground(paintParts)) && (cachedBrush.Color.A == 0xff))
                    {
                        graphics.FillRectangle(cachedBrush, rect);
                    }
                }
                if (cellStyle.Padding != Padding.Empty)
                {
                    if (base.DataGridView.RightToLeftInternal)
                    {
                        rect.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
                    }
                    else
                    {
                        rect.Offset(cellStyle.Padding.Left, cellStyle.Padding.Top);
                    }
                    rect.Width -= cellStyle.Padding.Horizontal;
                    rect.Height -= cellStyle.Padding.Vertical;
                }
            }
            Bitmap bmp = null;
            if ((rect.Width > 0) && (rect.Height > 0))
            {
                Rectangle cellValueBounds = rect;
                string str = formattedValue as string;
                if (!string.IsNullOrEmpty(str))
                {
                    if ((rect.Width >= 0x12) && (rect.Height >= 15))
                    {
                        if (paint && DataGridViewCell.PaintContentBackground(paintParts))
                        {
                            if (base.DataGridView.CurrentCellAddress.Y == rowIndex)
                            {
                                if (base.DataGridView.VirtualMode)
                                {
                                    if (base.DataGridView.IsCurrentRowDirty && base.DataGridView.ShowEditingIcon)
                                    {
                                        bmp = GetPencilBitmap(base.DataGridView.RightToLeftInternal);
                                    }
                                    else if (base.DataGridView.NewRowIndex == rowIndex)
                                    {
                                        bmp = GetArrowStarBitmap(base.DataGridView.RightToLeftInternal);
                                    }
                                    else
                                    {
                                        bmp = GetArrowBitmap(base.DataGridView.RightToLeftInternal);
                                    }
                                }
                                else if (base.DataGridView.IsCurrentCellDirty && base.DataGridView.ShowEditingIcon)
                                {
                                    bmp = GetPencilBitmap(base.DataGridView.RightToLeftInternal);
                                }
                                else if (base.DataGridView.NewRowIndex == rowIndex)
                                {
                                    bmp = GetArrowStarBitmap(base.DataGridView.RightToLeftInternal);
                                }
                                else
                                {
                                    bmp = GetArrowBitmap(base.DataGridView.RightToLeftInternal);
                                }
                            }
                            else if (base.DataGridView.NewRowIndex == rowIndex)
                            {
                                bmp = StarBitmap;
                            }
                            if (bmp != null)
                            {
                                Color color;
                                if (base.DataGridView.ApplyVisualStylesToHeaderCells)
                                {
                                    color = DataGridViewRowHeaderCellRenderer.VisualStyleRenderer.GetColor(ColorProperty.TextColor);
                                }
                                else
                                {
                                    color = flag ? cellStyle.SelectionForeColor : cellStyle.ForeColor;
                                }
                                lock (bmp)
                                {
                                    this.PaintIcon(graphics, bmp, rect, color);
                                }
                            }
                        }
                        if (!base.DataGridView.RightToLeftInternal)
                        {
                            rect.X += 0x12;
                        }
                        rect.Width -= 0x12;
                    }
                    rect.Offset(4, 1);
                    rect.Width -= 9;
                    rect.Height -= 2;
                    if ((rect.Width > 0) && (rect.Height > 0))
                    {
                        TextFormatFlags flags = DataGridViewUtilities.ComputeTextFormatFlagsForCellStyleAlignment(base.DataGridView.RightToLeftInternal, cellStyle.Alignment, cellStyle.WrapMode);
                        if (base.DataGridView.ShowRowErrors && (rect.Width > 0x12))
                        {
                            Size maxBounds = new Size((rect.Width - 12) - 6, rect.Height);
                            if (DataGridViewCell.TextFitsInBounds(graphics, str, cellStyle.Font, maxBounds, flags))
                            {
                                if (base.DataGridView.RightToLeftInternal)
                                {
                                    rect.X += 0x12;
                                }
                                rect.Width -= 0x12;
                            }
                        }
                        if (DataGridViewCell.PaintContentForeground(paintParts))
                        {
                            if (paint)
                            {
                                Color color2;
                                if (base.DataGridView.ApplyVisualStylesToHeaderCells)
                                {
                                    color2 = DataGridViewRowHeaderCellRenderer.VisualStyleRenderer.GetColor(ColorProperty.TextColor);
                                }
                                else
                                {
                                    color2 = flag ? cellStyle.SelectionForeColor : cellStyle.ForeColor;
                                }
                                if ((flags & TextFormatFlags.SingleLine) != TextFormatFlags.Default)
                                {
                                    flags |= TextFormatFlags.EndEllipsis;
                                }
                                TextRenderer.DrawText(graphics, str, cellStyle.Font, rect, color2, flags);
                            }
                            else if (computeContentBounds)
                            {
                                empty = DataGridViewUtilities.GetTextBounds(rect, str, flags, cellStyle);
                            }
                        }
                    }
                    if (cellValueBounds.Width >= 0x21)
                    {
                        if ((paint && base.DataGridView.ShowRowErrors) && DataGridViewCell.PaintErrorIcon(paintParts))
                        {
                            this.PaintErrorIcon(graphics, clipBounds, cellValueBounds, errorText);
                            return empty;
                        }
                        if (computeErrorIconBounds && !string.IsNullOrEmpty(errorText))
                        {
                            empty = base.ComputeErrorIconBounds(cellValueBounds);
                        }
                    }
                    return empty;
                }
                if (((rect.Width >= 0x12) && (rect.Height >= 15)) && (paint && DataGridViewCell.PaintContentBackground(paintParts)))
                {
                    if (base.DataGridView.CurrentCellAddress.Y == rowIndex)
                    {
                        if (base.DataGridView.VirtualMode)
                        {
                            if (base.DataGridView.IsCurrentRowDirty && base.DataGridView.ShowEditingIcon)
                            {
                                bmp = GetPencilBitmap(base.DataGridView.RightToLeftInternal);
                            }
                            else if (base.DataGridView.NewRowIndex == rowIndex)
                            {
                                bmp = GetArrowStarBitmap(base.DataGridView.RightToLeftInternal);
                            }
                            else
                            {
                                bmp = GetArrowBitmap(base.DataGridView.RightToLeftInternal);
                            }
                        }
                        else if (base.DataGridView.IsCurrentCellDirty && base.DataGridView.ShowEditingIcon)
                        {
                            bmp = GetPencilBitmap(base.DataGridView.RightToLeftInternal);
                        }
                        else if (base.DataGridView.NewRowIndex == rowIndex)
                        {
                            bmp = GetArrowStarBitmap(base.DataGridView.RightToLeftInternal);
                        }
                        else
                        {
                            bmp = GetArrowBitmap(base.DataGridView.RightToLeftInternal);
                        }
                    }
                    else if (base.DataGridView.NewRowIndex == rowIndex)
                    {
                        bmp = StarBitmap;
                    }
                    if (bmp != null)
                    {
                        lock (bmp)
                        {
                            Color color3;
                            if (base.DataGridView.ApplyVisualStylesToHeaderCells)
                            {
                                color3 = DataGridViewRowHeaderCellRenderer.VisualStyleRenderer.GetColor(ColorProperty.TextColor);
                            }
                            else
                            {
                                color3 = flag ? cellStyle.SelectionForeColor : cellStyle.ForeColor;
                            }
                            this.PaintIcon(graphics, bmp, rect, color3);
                        }
                    }
                }
                if (cellValueBounds.Width >= 0x21)
                {
                    if ((paint && base.DataGridView.ShowRowErrors) && DataGridViewCell.PaintErrorIcon(paintParts))
                    {
                        base.PaintErrorIcon(graphics, cellStyle, rowIndex, cellBounds, cellValueBounds, errorText);
                        return empty;
                    }
                    if (computeErrorIconBounds && !string.IsNullOrEmpty(errorText))
                    {
                        empty = base.ComputeErrorIconBounds(cellValueBounds);
                    }
                }
            }
            return empty;
        }

        protected override bool SetValue(int rowIndex, object value)
        {
            object obj2 = this.GetValue(rowIndex);
            if ((value != null) || base.Properties.ContainsObject(DataGridViewCell.PropCellValue))
            {
                base.Properties.SetObject(DataGridViewCell.PropCellValue, value);
            }
            if ((base.DataGridView != null) && (obj2 != value))
            {
                base.RaiseCellValueChanged(new DataGridViewCellEventArgs(-1, rowIndex));
            }
            return true;
        }

        public override string ToString()
        {
            return ("DataGridViewRowHeaderCell { RowIndex=" + base.RowIndex.ToString(CultureInfo.CurrentCulture) + " }");
        }

        private static Bitmap LeftArrowBitmap
        {
            get
            {
                if (leftArrowBmp == null)
                {
                    leftArrowBmp = GetBitmap("DataGridViewRow.left.bmp");
                }
                return leftArrowBmp;
            }
        }

        private static Bitmap LeftArrowStarBitmap
        {
            get
            {
                if (leftArrowStarBmp == null)
                {
                    leftArrowStarBmp = GetBitmap("DataGridViewRow.leftstar.bmp");
                }
                return leftArrowStarBmp;
            }
        }

        private static Bitmap PencilLTRBitmap
        {
            get
            {
                if (pencilLTRBmp == null)
                {
                    pencilLTRBmp = GetBitmap("DataGridViewRow.pencil_ltr.bmp");
                }
                return pencilLTRBmp;
            }
        }

        private static Bitmap PencilRTLBitmap
        {
            get
            {
                if (pencilRTLBmp == null)
                {
                    pencilRTLBmp = GetBitmap("DataGridViewRow.pencil_rtl.bmp");
                }
                return pencilRTLBmp;
            }
        }

        private static Bitmap RightArrowBitmap
        {
            get
            {
                if (rightArrowBmp == null)
                {
                    rightArrowBmp = GetBitmap("DataGridViewRow.right.bmp");
                }
                return rightArrowBmp;
            }
        }

        private static Bitmap RightArrowStarBitmap
        {
            get
            {
                if (rightArrowStarBmp == null)
                {
                    rightArrowStarBmp = GetBitmap("DataGridViewRow.rightstar.bmp");
                }
                return rightArrowStarBmp;
            }
        }

        private static Bitmap StarBitmap
        {
            get
            {
                if (starBmp == null)
                {
                    starBmp = GetBitmap("DataGridViewRow.star.bmp");
                }
                return starBmp;
            }
        }

        protected class DataGridViewRowHeaderCellAccessibleObject : DataGridViewCell.DataGridViewCellAccessibleObject
        {
            public DataGridViewRowHeaderCellAccessibleObject(DataGridViewRowHeaderCell owner) : base(owner)
            {
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                if (((base.Owner.DataGridView.SelectionMode == DataGridViewSelectionMode.FullRowSelect) || (base.Owner.DataGridView.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect)) && (base.Owner.OwningRow != null))
                {
                    base.Owner.OwningRow.Selected = true;
                }
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation navigationDirection)
            {
                switch (navigationDirection)
                {
                    case AccessibleNavigation.Up:
                        if (base.Owner.OwningRow != null)
                        {
                            if (base.Owner.OwningRow.Index == base.Owner.DataGridView.Rows.GetFirstRow(DataGridViewElementStates.Visible))
                            {
                                if (base.Owner.DataGridView.ColumnHeadersVisible)
                                {
                                    return base.Owner.DataGridView.AccessibilityObject.GetChild(0).GetChild(0);
                                }
                                return null;
                            }
                            int previousRow = base.Owner.DataGridView.Rows.GetPreviousRow(base.Owner.OwningRow.Index, DataGridViewElementStates.Visible);
                            int index = base.Owner.DataGridView.Rows.GetRowCount(DataGridViewElementStates.Visible, 0, previousRow);
                            if (base.Owner.DataGridView.ColumnHeadersVisible)
                            {
                                return base.Owner.DataGridView.AccessibilityObject.GetChild(index + 1).GetChild(0);
                            }
                            return base.Owner.DataGridView.AccessibilityObject.GetChild(index).GetChild(0);
                        }
                        return null;

                    case AccessibleNavigation.Down:
                        if (base.Owner.OwningRow != null)
                        {
                            if (base.Owner.OwningRow.Index == base.Owner.DataGridView.Rows.GetLastRow(DataGridViewElementStates.Visible))
                            {
                                return null;
                            }
                            int nextRow = base.Owner.DataGridView.Rows.GetNextRow(base.Owner.OwningRow.Index, DataGridViewElementStates.Visible);
                            int num2 = base.Owner.DataGridView.Rows.GetRowCount(DataGridViewElementStates.Visible, 0, nextRow);
                            if (base.Owner.DataGridView.ColumnHeadersVisible)
                            {
                                return base.Owner.DataGridView.AccessibilityObject.GetChild(1 + num2).GetChild(0);
                            }
                            return base.Owner.DataGridView.AccessibilityObject.GetChild(num2).GetChild(0);
                        }
                        return null;

                    case AccessibleNavigation.Next:
                        if ((base.Owner.OwningRow == null) || (base.Owner.DataGridView.Columns.GetColumnCount(DataGridViewElementStates.Visible) <= 0))
                        {
                            return null;
                        }
                        return this.ParentPrivate.GetChild(1);

                    case AccessibleNavigation.Previous:
                        return null;
                }
                return null;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void Select(AccessibleSelection flags)
            {
                if (base.Owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellAccessibleObject_OwnerNotSet"));
                }
                DataGridViewRowHeaderCell owner = (DataGridViewRowHeaderCell) base.Owner;
                DataGridView dataGridView = owner.DataGridView;
                if (dataGridView != null)
                {
                    if ((flags & AccessibleSelection.TakeFocus) == AccessibleSelection.TakeFocus)
                    {
                        dataGridView.FocusInternal();
                    }
                    if ((owner.OwningRow != null) && ((dataGridView.SelectionMode == DataGridViewSelectionMode.FullRowSelect) || (dataGridView.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect)))
                    {
                        if ((flags & (AccessibleSelection.AddSelection | AccessibleSelection.TakeSelection)) != AccessibleSelection.None)
                        {
                            owner.OwningRow.Selected = true;
                        }
                        else if ((flags & AccessibleSelection.RemoveSelection) == AccessibleSelection.RemoveSelection)
                        {
                            owner.OwningRow.Selected = false;
                        }
                    }
                }
            }

            public override Rectangle Bounds
            {
                get
                {
                    if (base.Owner.OwningRow == null)
                    {
                        return Rectangle.Empty;
                    }
                    Rectangle bounds = this.ParentPrivate.Bounds;
                    bounds.Width = base.Owner.DataGridView.RowHeadersWidth;
                    return bounds;
                }
            }

            public override string DefaultAction
            {
                get
                {
                    if ((base.Owner.DataGridView.SelectionMode != DataGridViewSelectionMode.FullRowSelect) && (base.Owner.DataGridView.SelectionMode != DataGridViewSelectionMode.RowHeaderSelect))
                    {
                        return string.Empty;
                    }
                    return System.Windows.Forms.SR.GetString("DataGridView_RowHeaderCellAccDefaultAction");
                }
            }

            public override string Name
            {
                get
                {
                    if (this.ParentPrivate != null)
                    {
                        return this.ParentPrivate.Name;
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
                    if (base.Owner.OwningRow == null)
                    {
                        return null;
                    }
                    return base.Owner.OwningRow.AccessibilityObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.RowHeader;
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
                    if (((base.Owner.DataGridView.SelectionMode == DataGridViewSelectionMode.FullRowSelect) || (base.Owner.DataGridView.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect)) && ((base.Owner.OwningRow != null) && base.Owner.OwningRow.Selected))
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

        private class DataGridViewRowHeaderCellRenderer
        {
            private static System.Windows.Forms.VisualStyles.VisualStyleRenderer visualStyleRenderer;

            private DataGridViewRowHeaderCellRenderer()
            {
            }

            public static void DrawHeader(Graphics g, Rectangle bounds, int headerState)
            {
                VisualStyleRenderer.SetParameters(DataGridViewRowHeaderCell.HeaderElement.ClassName, DataGridViewRowHeaderCell.HeaderElement.Part, headerState);
                VisualStyleRenderer.DrawBackground(g, bounds, Rectangle.Truncate(g.ClipBounds));
            }

            public static System.Windows.Forms.VisualStyles.VisualStyleRenderer VisualStyleRenderer
            {
                get
                {
                    if (visualStyleRenderer == null)
                    {
                        visualStyleRenderer = new System.Windows.Forms.VisualStyles.VisualStyleRenderer(DataGridViewRowHeaderCell.HeaderElement);
                    }
                    return visualStyleRenderer;
                }
            }
        }
    }
}

