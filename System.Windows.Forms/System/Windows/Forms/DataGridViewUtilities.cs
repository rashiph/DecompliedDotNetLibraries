namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    internal class DataGridViewUtilities
    {
        private const byte DATAGRIDVIEWROWHEADERCELL_contentMarginHeight = 3;
        private const byte DATAGRIDVIEWROWHEADERCELL_contentMarginWidth = 3;
        private const byte DATAGRIDVIEWROWHEADERCELL_horizontalTextMarginLeft = 1;
        private const byte DATAGRIDVIEWROWHEADERCELL_horizontalTextMarginRight = 2;
        private const byte DATAGRIDVIEWROWHEADERCELL_iconMarginHeight = 2;
        private const byte DATAGRIDVIEWROWHEADERCELL_iconMarginWidth = 3;
        private const byte DATAGRIDVIEWROWHEADERCELL_iconsHeight = 11;
        private const byte DATAGRIDVIEWROWHEADERCELL_iconsWidth = 12;
        private const byte DATAGRIDVIEWROWHEADERCELL_verticalTextMargin = 1;

        internal static ContentAlignment ComputeDrawingContentAlignmentForCellStyleAlignment(DataGridViewContentAlignment alignment)
        {
            switch (alignment)
            {
                case DataGridViewContentAlignment.TopLeft:
                    return ContentAlignment.TopLeft;

                case DataGridViewContentAlignment.TopCenter:
                    return ContentAlignment.TopCenter;

                case DataGridViewContentAlignment.TopRight:
                    return ContentAlignment.TopRight;

                case DataGridViewContentAlignment.MiddleLeft:
                    return ContentAlignment.MiddleLeft;

                case DataGridViewContentAlignment.MiddleCenter:
                    return ContentAlignment.MiddleCenter;

                case DataGridViewContentAlignment.MiddleRight:
                    return ContentAlignment.MiddleRight;

                case DataGridViewContentAlignment.BottomLeft:
                    return ContentAlignment.BottomLeft;

                case DataGridViewContentAlignment.BottomCenter:
                    return ContentAlignment.BottomCenter;

                case DataGridViewContentAlignment.BottomRight:
                    return ContentAlignment.BottomRight;
            }
            return ContentAlignment.MiddleCenter;
        }

        internal static TextFormatFlags ComputeTextFormatFlagsForCellStyleAlignment(bool rightToLeft, DataGridViewContentAlignment alignment, DataGridViewTriState wrapMode)
        {
            TextFormatFlags horizontalCenter;
            switch (alignment)
            {
                case DataGridViewContentAlignment.TopLeft:
                    horizontalCenter = TextFormatFlags.Default;
                    if (!rightToLeft)
                    {
                        horizontalCenter = horizontalCenter;
                    }
                    else
                    {
                        horizontalCenter |= TextFormatFlags.Right;
                    }
                    break;

                case DataGridViewContentAlignment.TopCenter:
                    horizontalCenter = TextFormatFlags.HorizontalCenter;
                    break;

                case DataGridViewContentAlignment.TopRight:
                    horizontalCenter = TextFormatFlags.Default;
                    if (!rightToLeft)
                    {
                        horizontalCenter |= TextFormatFlags.Right;
                    }
                    else
                    {
                        horizontalCenter = horizontalCenter;
                    }
                    break;

                case DataGridViewContentAlignment.MiddleLeft:
                    horizontalCenter = TextFormatFlags.VerticalCenter;
                    if (rightToLeft)
                    {
                        horizontalCenter |= TextFormatFlags.Right;
                    }
                    else
                    {
                        horizontalCenter = horizontalCenter;
                    }
                    break;

                case DataGridViewContentAlignment.MiddleCenter:
                    horizontalCenter = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
                    break;

                case DataGridViewContentAlignment.BottomCenter:
                    horizontalCenter = TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
                    break;

                case DataGridViewContentAlignment.BottomRight:
                    horizontalCenter = TextFormatFlags.Bottom;
                    if (rightToLeft)
                    {
                        horizontalCenter = horizontalCenter;
                    }
                    else
                    {
                        horizontalCenter |= TextFormatFlags.Right;
                    }
                    break;

                case DataGridViewContentAlignment.MiddleRight:
                    horizontalCenter = TextFormatFlags.VerticalCenter;
                    if (rightToLeft)
                    {
                        horizontalCenter = horizontalCenter;
                    }
                    else
                    {
                        horizontalCenter |= TextFormatFlags.Right;
                    }
                    break;

                case DataGridViewContentAlignment.BottomLeft:
                    horizontalCenter = TextFormatFlags.Bottom;
                    if (rightToLeft)
                    {
                        horizontalCenter |= TextFormatFlags.Right;
                    }
                    else
                    {
                        horizontalCenter = horizontalCenter;
                    }
                    break;

                default:
                    horizontalCenter = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
                    break;
            }
            if (wrapMode == DataGridViewTriState.False)
            {
                horizontalCenter |= TextFormatFlags.SingleLine;
            }
            else
            {
                horizontalCenter |= TextFormatFlags.WordBreak;
            }
            horizontalCenter |= TextFormatFlags.NoPrefix;
            horizontalCenter |= TextFormatFlags.PreserveGraphicsClipping;
            if (rightToLeft)
            {
                horizontalCenter |= TextFormatFlags.RightToLeft;
            }
            return horizontalCenter;
        }

        internal static Size GetPreferredRowHeaderSize(Graphics graphics, string val, DataGridViewCellStyle cellStyle, int borderAndPaddingWidths, int borderAndPaddingHeights, bool showRowErrors, bool showGlyph, Size constraintSize, TextFormatFlags flags)
        {
            int width;
            int num2;
            switch (DataGridViewCell.GetFreeDimensionFromConstraint(constraintSize))
            {
                case DataGridViewFreeDimension.Height:
                {
                    int num4 = 1;
                    int height = 1;
                    int maxWidth = constraintSize.Width - borderAndPaddingWidths;
                    if (string.IsNullOrEmpty(val))
                    {
                        if ((showGlyph || showRowErrors) && (maxWidth >= 0x12))
                        {
                            num4 = 15;
                        }
                    }
                    else
                    {
                        if (showGlyph && (maxWidth >= 0x12))
                        {
                            num4 = 15;
                            maxWidth -= 0x12;
                        }
                        if (showRowErrors && (maxWidth >= 0x12))
                        {
                            num4 = 15;
                            maxWidth -= 0x12;
                        }
                        if (maxWidth > 9)
                        {
                            maxWidth -= 9;
                            if (cellStyle.WrapMode == DataGridViewTriState.True)
                            {
                                height = DataGridViewCell.MeasureTextHeight(graphics, val, cellStyle.Font, maxWidth, flags);
                            }
                            else
                            {
                                height = DataGridViewCell.MeasureTextSize(graphics, val, cellStyle.Font, flags).Height;
                            }
                            height += 2;
                        }
                    }
                    return new Size(0, Math.Max(num4, height) + borderAndPaddingHeights);
                }
                case DataGridViewFreeDimension.Width:
                {
                    width = 0;
                    num2 = constraintSize.Height - borderAndPaddingHeights;
                    if (string.IsNullOrEmpty(val))
                    {
                        goto Label_007B;
                    }
                    int maxHeight = num2 - 2;
                    if (maxHeight <= 0)
                    {
                        goto Label_007B;
                    }
                    if (cellStyle.WrapMode != DataGridViewTriState.True)
                    {
                        width = DataGridViewCell.MeasureTextSize(graphics, val, cellStyle.Font, flags).Width;
                        break;
                    }
                    width = DataGridViewCell.MeasureTextWidth(graphics, val, cellStyle.Font, maxHeight, flags);
                    break;
                }
                default:
                    Size size;
                    if (!string.IsNullOrEmpty(val))
                    {
                        if (cellStyle.WrapMode == DataGridViewTriState.True)
                        {
                            size = DataGridViewCell.MeasureTextPreferredSize(graphics, val, cellStyle.Font, 5f, flags);
                        }
                        else
                        {
                            size = DataGridViewCell.MeasureTextSize(graphics, val, cellStyle.Font, flags);
                        }
                        size.Width += 9;
                        size.Height += 2;
                    }
                    else
                    {
                        size = new Size(0, 1);
                    }
                    if (showGlyph)
                    {
                        size.Width += 0x12;
                    }
                    if (showRowErrors)
                    {
                        size.Width += 0x12;
                    }
                    if (showGlyph || showRowErrors)
                    {
                        size.Height = Math.Max(size.Height, 15);
                    }
                    size.Width += borderAndPaddingWidths;
                    size.Height += borderAndPaddingHeights;
                    return size;
            }
            width += 9;
        Label_007B:
            if (num2 >= 15)
            {
                if (showGlyph)
                {
                    width += 0x12;
                }
                if (showRowErrors)
                {
                    width += 0x12;
                }
            }
            return new Size(Math.Max(width, 1) + borderAndPaddingWidths, 0);
        }

        internal static Rectangle GetTextBounds(Rectangle cellBounds, string text, TextFormatFlags flags, DataGridViewCellStyle cellStyle)
        {
            return GetTextBounds(cellBounds, text, flags, cellStyle, cellStyle.Font);
        }

        internal static Rectangle GetTextBounds(Rectangle cellBounds, string text, TextFormatFlags flags, DataGridViewCellStyle cellStyle, Font font)
        {
            if (((flags & TextFormatFlags.SingleLine) != TextFormatFlags.Default) && (TextRenderer.MeasureText(text, font, new Size(0x7fffffff, 0x7fffffff), flags).Width > cellBounds.Width))
            {
                flags |= TextFormatFlags.EndEllipsis;
            }
            Size proposedSize = new Size(cellBounds.Width, cellBounds.Height);
            Size size = TextRenderer.MeasureText(text, font, proposedSize, flags);
            if (size.Width > proposedSize.Width)
            {
                size.Width = proposedSize.Width;
            }
            if (size.Height > proposedSize.Height)
            {
                size.Height = proposedSize.Height;
            }
            if (size == proposedSize)
            {
                return cellBounds;
            }
            return new Rectangle(GetTextLocation(cellBounds, size, flags, cellStyle), size);
        }

        internal static Point GetTextLocation(Rectangle cellBounds, Size sizeText, TextFormatFlags flags, DataGridViewCellStyle cellStyle)
        {
            Point point = new Point(0, 0);
            DataGridViewContentAlignment middleLeft = cellStyle.Alignment;
            if ((flags & TextFormatFlags.RightToLeft) != TextFormatFlags.Default)
            {
                switch (middleLeft)
                {
                    case DataGridViewContentAlignment.MiddleRight:
                        middleLeft = DataGridViewContentAlignment.MiddleLeft;
                        break;

                    case DataGridViewContentAlignment.BottomLeft:
                        middleLeft = DataGridViewContentAlignment.BottomRight;
                        break;

                    case DataGridViewContentAlignment.BottomRight:
                        middleLeft = DataGridViewContentAlignment.BottomLeft;
                        break;

                    case DataGridViewContentAlignment.TopLeft:
                        middleLeft = DataGridViewContentAlignment.TopRight;
                        break;

                    case DataGridViewContentAlignment.TopRight:
                        middleLeft = DataGridViewContentAlignment.TopLeft;
                        break;

                    case DataGridViewContentAlignment.MiddleLeft:
                        middleLeft = DataGridViewContentAlignment.MiddleRight;
                        break;
                }
            }
            DataGridViewContentAlignment alignment3 = middleLeft;
            if (alignment3 <= DataGridViewContentAlignment.MiddleCenter)
            {
                switch (alignment3)
                {
                    case DataGridViewContentAlignment.TopLeft:
                        point.X = cellBounds.X;
                        point.Y = cellBounds.Y;
                        return point;

                    case DataGridViewContentAlignment.TopCenter:
                        point.X = cellBounds.X + ((cellBounds.Width - sizeText.Width) / 2);
                        point.Y = cellBounds.Y;
                        return point;

                    case (DataGridViewContentAlignment.TopCenter | DataGridViewContentAlignment.TopLeft):
                        return point;

                    case DataGridViewContentAlignment.TopRight:
                        point.X = cellBounds.Right - sizeText.Width;
                        point.Y = cellBounds.Y;
                        return point;

                    case DataGridViewContentAlignment.MiddleLeft:
                        point.X = cellBounds.X;
                        point.Y = cellBounds.Y + ((cellBounds.Height - sizeText.Height) / 2);
                        return point;

                    case DataGridViewContentAlignment.MiddleCenter:
                        point.X = cellBounds.X + ((cellBounds.Width - sizeText.Width) / 2);
                        point.Y = cellBounds.Y + ((cellBounds.Height - sizeText.Height) / 2);
                        return point;
                }
                return point;
            }
            if (alignment3 <= DataGridViewContentAlignment.BottomLeft)
            {
                switch (alignment3)
                {
                    case DataGridViewContentAlignment.MiddleRight:
                        point.X = cellBounds.Right - sizeText.Width;
                        point.Y = cellBounds.Y + ((cellBounds.Height - sizeText.Height) / 2);
                        return point;

                    case DataGridViewContentAlignment.BottomLeft:
                        point.X = cellBounds.X;
                        point.Y = cellBounds.Bottom - sizeText.Height;
                        return point;
                }
                return point;
            }
            switch (alignment3)
            {
                case DataGridViewContentAlignment.BottomCenter:
                    point.X = cellBounds.X + ((cellBounds.Width - sizeText.Width) / 2);
                    point.Y = cellBounds.Bottom - sizeText.Height;
                    return point;

                case DataGridViewContentAlignment.BottomRight:
                    point.X = cellBounds.Right - sizeText.Width;
                    point.Y = cellBounds.Bottom - sizeText.Height;
                    return point;
            }
            return point;
        }

        internal static bool ValidTextFormatFlags(TextFormatFlags flags)
        {
            return ((flags & ~(TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PrefixOnly | TextFormatFlags.HidePrefix | TextFormatFlags.NoFullWidthCharacterBreak | TextFormatFlags.WordEllipsis | TextFormatFlags.RightToLeft | TextFormatFlags.ModifyString | TextFormatFlags.EndEllipsis | TextFormatFlags.PathEllipsis | TextFormatFlags.TextBoxControl | TextFormatFlags.Internal | TextFormatFlags.NoPrefix | TextFormatFlags.ExternalLeading | TextFormatFlags.NoClipping | TextFormatFlags.ExpandTabs | TextFormatFlags.SingleLine | TextFormatFlags.WordBreak | TextFormatFlags.Bottom | TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.HorizontalCenter)) == TextFormatFlags.Default);
        }
    }
}

