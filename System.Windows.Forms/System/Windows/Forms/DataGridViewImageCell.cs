namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Security.Permissions;

    public class DataGridViewImageCell : DataGridViewCell
    {
        private static System.Type cellType = typeof(DataGridViewImageCell);
        private static ColorMap[] colorMap = new ColorMap[] { new ColorMap() };
        private const byte DATAGRIDVIEWIMAGECELL_valueIsIcon = 1;
        private static System.Type defaultTypeIcon = typeof(Icon);
        private static System.Type defaultTypeImage = typeof(Image);
        private static Bitmap errorBmp = null;
        private static Icon errorIco = null;
        private byte flags;
        private static readonly int PropImageCellDescription = PropertyStore.CreateKey();
        private static readonly int PropImageCellLayout = PropertyStore.CreateKey();

        public DataGridViewImageCell() : this(false)
        {
        }

        public DataGridViewImageCell(bool valueIsIcon)
        {
            if (valueIsIcon)
            {
                this.flags = 1;
            }
        }

        public override object Clone()
        {
            DataGridViewImageCell cell;
            System.Type type = base.GetType();
            if (type == cellType)
            {
                cell = new DataGridViewImageCell();
            }
            else
            {
                cell = (DataGridViewImageCell) Activator.CreateInstance(type);
            }
            base.CloneInternal(cell);
            cell.ValueIsIconInternal = this.ValueIsIcon;
            cell.Description = this.Description;
            cell.ImageLayoutInternal = this.ImageLayout;
            return cell;
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new DataGridViewImageCellAccessibleObject(this);
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
            if (((base.DataGridView == null) || (rowIndex < 0)) || (base.OwningColumn == null))
            {
                return Rectangle.Empty;
            }
            object obj2 = this.GetValue(rowIndex);
            object formattedValue = this.GetFormattedValue(obj2, rowIndex, ref cellStyle, null, null, DataGridViewDataErrorContexts.Formatting);
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
            if (((base.DataGridView == null) || (rowIndex < 0)) || (((base.OwningColumn == null) || !base.DataGridView.ShowCellErrors) || string.IsNullOrEmpty(this.GetErrorText(rowIndex))))
            {
                return Rectangle.Empty;
            }
            object obj2 = this.GetValue(rowIndex);
            object formattedValue = this.GetFormattedValue(obj2, rowIndex, ref cellStyle, null, null, DataGridViewDataErrorContexts.Formatting);
            base.ComputeBorderStyleCellStateAndCellBounds(rowIndex, out style, out states, out rectangle);
            return this.PaintPrivate(graphics, rectangle, rectangle, rowIndex, states, formattedValue, this.GetErrorText(rowIndex), cellStyle, style, DataGridViewPaintParts.ContentForeground, false, true, false);
        }

        protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
        {
            if ((context & DataGridViewDataErrorContexts.ClipboardContent) != 0)
            {
                return this.Description;
            }
            object obj2 = base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
            if ((obj2 == null) && (cellStyle.NullValue == null))
            {
                return null;
            }
            if (this.ValueIsIcon)
            {
                Icon errorIcon = obj2 as Icon;
                if (errorIcon == null)
                {
                    errorIcon = ErrorIcon;
                }
                return errorIcon;
            }
            Image errorBitmap = obj2 as Image;
            if (errorBitmap == null)
            {
                errorBitmap = ErrorBitmap;
            }
            return errorBitmap;
        }

        protected override Size GetPreferredSize(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
        {
            Size empty;
            if (base.DataGridView == null)
            {
                return new Size(-1, -1);
            }
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            Rectangle stdBorderWidths = base.StdBorderWidths;
            int num = (stdBorderWidths.Left + stdBorderWidths.Width) + cellStyle.Padding.Horizontal;
            int num2 = (stdBorderWidths.Top + stdBorderWidths.Height) + cellStyle.Padding.Vertical;
            DataGridViewFreeDimension freeDimensionFromConstraint = DataGridViewCell.GetFreeDimensionFromConstraint(constraintSize);
            object obj2 = base.GetFormattedValue(rowIndex, ref cellStyle, DataGridViewDataErrorContexts.PreferredSize | DataGridViewDataErrorContexts.Formatting);
            Image image = obj2 as Image;
            Icon icon = null;
            if (image == null)
            {
                icon = obj2 as Icon;
            }
            if ((freeDimensionFromConstraint == DataGridViewFreeDimension.Height) && (this.ImageLayout == DataGridViewImageCellLayout.Zoom))
            {
                if ((image != null) || (icon != null))
                {
                    if (image != null)
                    {
                        int num3 = constraintSize.Width - num;
                        if ((num3 <= 0) || (image.Width == 0))
                        {
                            empty = Size.Empty;
                        }
                        else
                        {
                            empty = new Size(0, Math.Min(image.Height, decimal.ToInt32((image.Height * num3) / image.Width)));
                        }
                    }
                    else
                    {
                        int num4 = constraintSize.Width - num;
                        if ((num4 <= 0) || (icon.Width == 0))
                        {
                            empty = Size.Empty;
                        }
                        else
                        {
                            empty = new Size(0, Math.Min(icon.Height, decimal.ToInt32((icon.Height * num4) / icon.Width)));
                        }
                    }
                }
                else
                {
                    empty = new Size(0, 1);
                }
            }
            else if ((freeDimensionFromConstraint == DataGridViewFreeDimension.Width) && (this.ImageLayout == DataGridViewImageCellLayout.Zoom))
            {
                if ((image != null) || (icon != null))
                {
                    if (image != null)
                    {
                        int num5 = constraintSize.Height - num2;
                        if ((num5 <= 0) || (image.Height == 0))
                        {
                            empty = Size.Empty;
                        }
                        else
                        {
                            empty = new Size(Math.Min(image.Width, decimal.ToInt32((image.Width * num5) / image.Height)), 0);
                        }
                    }
                    else
                    {
                        int num6 = constraintSize.Height - num2;
                        if ((num6 <= 0) || (icon.Height == 0))
                        {
                            empty = Size.Empty;
                        }
                        else
                        {
                            empty = new Size(Math.Min(icon.Width, decimal.ToInt32((icon.Width * num6) / icon.Height)), 0);
                        }
                    }
                }
                else
                {
                    empty = new Size(1, 0);
                }
            }
            else
            {
                if (image != null)
                {
                    empty = new Size(image.Width, image.Height);
                }
                else if (icon != null)
                {
                    empty = new Size(icon.Width, icon.Height);
                }
                else
                {
                    empty = new Size(1, 1);
                }
                switch (freeDimensionFromConstraint)
                {
                    case DataGridViewFreeDimension.Height:
                        empty.Width = 0;
                        break;

                    case DataGridViewFreeDimension.Width:
                        empty.Height = 0;
                        break;
                }
            }
            if (freeDimensionFromConstraint != DataGridViewFreeDimension.Height)
            {
                empty.Width += num;
                if (base.DataGridView.ShowCellErrors)
                {
                    empty.Width = Math.Max(empty.Width, (num + 8) + 12);
                }
            }
            if (freeDimensionFromConstraint != DataGridViewFreeDimension.Width)
            {
                empty.Height += num2;
                if (base.DataGridView.ShowCellErrors)
                {
                    empty.Height = Math.Max(empty.Height, (num2 + 8) + 11);
                }
            }
            return empty;
        }

        protected override object GetValue(int rowIndex)
        {
            object obj2 = base.GetValue(rowIndex);
            if (obj2 == null)
            {
                DataGridViewImageColumn owningColumn = base.OwningColumn as DataGridViewImageColumn;
                if (owningColumn == null)
                {
                    return obj2;
                }
                if (defaultTypeImage.IsAssignableFrom(this.ValueType))
                {
                    Image image = owningColumn.Image;
                    if (image != null)
                    {
                        return image;
                    }
                    return obj2;
                }
                if (defaultTypeIcon.IsAssignableFrom(this.ValueType))
                {
                    Icon icon = owningColumn.Icon;
                    if (icon != null)
                    {
                        return icon;
                    }
                }
            }
            return obj2;
        }

        private Rectangle ImgBounds(Rectangle bounds, int imgWidth, int imgHeight, DataGridViewImageCellLayout imageLayout, DataGridViewCellStyle cellStyle)
        {
            Rectangle empty = Rectangle.Empty;
            switch (imageLayout)
            {
                case DataGridViewImageCellLayout.NotSet:
                case DataGridViewImageCellLayout.Normal:
                    empty = new Rectangle(bounds.X, bounds.Y, imgWidth, imgHeight);
                    break;

                case DataGridViewImageCellLayout.Zoom:
                    if ((imgWidth * bounds.Height) >= (imgHeight * bounds.Width))
                    {
                        empty = new Rectangle(bounds.X, bounds.Y, bounds.Width, decimal.ToInt32((imgHeight * bounds.Width) / imgWidth));
                        break;
                    }
                    empty = new Rectangle(bounds.X, bounds.Y, decimal.ToInt32((imgWidth * bounds.Height) / imgHeight), bounds.Height);
                    break;
            }
            if (base.DataGridView.RightToLeftInternal)
            {
                switch (cellStyle.Alignment)
                {
                    case DataGridViewContentAlignment.MiddleRight:
                        empty.X = bounds.X;
                        break;

                    case DataGridViewContentAlignment.BottomLeft:
                        empty.X = bounds.Right - empty.Width;
                        break;

                    case DataGridViewContentAlignment.BottomRight:
                        empty.X = bounds.X;
                        break;

                    case DataGridViewContentAlignment.TopLeft:
                        empty.X = bounds.Right - empty.Width;
                        break;

                    case DataGridViewContentAlignment.TopRight:
                        empty.X = bounds.X;
                        break;

                    case DataGridViewContentAlignment.MiddleLeft:
                        empty.X = bounds.Right - empty.Width;
                        break;
                }
            }
            else
            {
                switch (cellStyle.Alignment)
                {
                    case DataGridViewContentAlignment.MiddleRight:
                        empty.X = bounds.Right - empty.Width;
                        break;

                    case DataGridViewContentAlignment.BottomLeft:
                        empty.X = bounds.X;
                        break;

                    case DataGridViewContentAlignment.BottomRight:
                        empty.X = bounds.Right - empty.Width;
                        break;

                    case DataGridViewContentAlignment.TopLeft:
                        empty.X = bounds.X;
                        break;

                    case DataGridViewContentAlignment.TopRight:
                        empty.X = bounds.Right - empty.Width;
                        break;

                    case DataGridViewContentAlignment.MiddleLeft:
                        empty.X = bounds.X;
                        break;
                }
            }
            switch (cellStyle.Alignment)
            {
                case DataGridViewContentAlignment.TopCenter:
                case DataGridViewContentAlignment.MiddleCenter:
                case DataGridViewContentAlignment.BottomCenter:
                    empty.X = bounds.X + ((bounds.Width - empty.Width) / 2);
                    break;
            }
            DataGridViewContentAlignment alignment = cellStyle.Alignment;
            if (alignment <= DataGridViewContentAlignment.MiddleCenter)
            {
                switch (alignment)
                {
                    case DataGridViewContentAlignment.TopLeft:
                    case DataGridViewContentAlignment.TopCenter:
                    case DataGridViewContentAlignment.TopRight:
                        empty.Y = bounds.Y;
                        return empty;

                    case (DataGridViewContentAlignment.TopCenter | DataGridViewContentAlignment.TopLeft):
                        return empty;

                    case DataGridViewContentAlignment.MiddleLeft:
                    case DataGridViewContentAlignment.MiddleCenter:
                        goto Label_030C;
                }
                return empty;
            }
            if (alignment <= DataGridViewContentAlignment.BottomLeft)
            {
                switch (alignment)
                {
                    case DataGridViewContentAlignment.MiddleRight:
                        goto Label_030C;

                    case DataGridViewContentAlignment.BottomLeft:
                        goto Label_032E;
                }
                return empty;
            }
            switch (alignment)
            {
                case DataGridViewContentAlignment.BottomCenter:
                case DataGridViewContentAlignment.BottomRight:
                    goto Label_032E;

                default:
                    return empty;
            }
        Label_030C:
            empty.Y = bounds.Y + ((bounds.Height - empty.Height) / 2);
            return empty;
        Label_032E:
            empty.Y = bounds.Bottom - empty.Height;
            return empty;
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            this.PaintPrivate(graphics, clipBounds, cellBounds, rowIndex, elementState, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts, false, false, true);
        }

        private Rectangle PaintPrivate(Graphics g, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts, bool computeContentBounds, bool computeErrorIconBounds, bool paint)
        {
            Rectangle empty;
            Point point;
            if (paint && DataGridViewCell.PaintBorder(paintParts))
            {
                this.PaintBorder(g, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
            }
            Rectangle cellValueBounds = cellBounds;
            Rectangle rectangle3 = this.BorderWidths(advancedBorderStyle);
            cellValueBounds.Offset(rectangle3.X, rectangle3.Y);
            cellValueBounds.Width -= rectangle3.Right;
            cellValueBounds.Height -= rectangle3.Bottom;
            if (((cellValueBounds.Width <= 0) || (cellValueBounds.Height <= 0)) || (!paint && !computeContentBounds))
            {
                if (computeErrorIconBounds)
                {
                    if (!string.IsNullOrEmpty(errorText))
                    {
                        return base.ComputeErrorIconBounds(cellValueBounds);
                    }
                    return Rectangle.Empty;
                }
                return Rectangle.Empty;
            }
            Rectangle destRect = cellValueBounds;
            if (cellStyle.Padding != Padding.Empty)
            {
                if (base.DataGridView.RightToLeftInternal)
                {
                    destRect.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
                }
                else
                {
                    destRect.Offset(cellStyle.Padding.Left, cellStyle.Padding.Top);
                }
                destRect.Width -= cellStyle.Padding.Horizontal;
                destRect.Height -= cellStyle.Padding.Vertical;
            }
            bool flag = (elementState & DataGridViewElementStates.Selected) != DataGridViewElementStates.None;
            SolidBrush cachedBrush = base.DataGridView.GetCachedBrush((DataGridViewCell.PaintSelectionBackground(paintParts) && flag) ? cellStyle.SelectionBackColor : cellStyle.BackColor);
            if ((destRect.Width > 0) && (destRect.Height > 0))
            {
                Image image = formattedValue as Image;
                Icon icon = null;
                if (image == null)
                {
                    icon = formattedValue as Icon;
                }
                if ((icon != null) || (image != null))
                {
                    DataGridViewImageCellLayout imageLayout = this.ImageLayout;
                    switch (imageLayout)
                    {
                        case DataGridViewImageCellLayout.NotSet:
                            if (base.OwningColumn is DataGridViewImageColumn)
                            {
                                imageLayout = ((DataGridViewImageColumn) base.OwningColumn).ImageLayout;
                            }
                            else
                            {
                                imageLayout = DataGridViewImageCellLayout.Normal;
                            }
                            break;

                        case DataGridViewImageCellLayout.Stretch:
                            if (paint)
                            {
                                if (DataGridViewCell.PaintBackground(paintParts))
                                {
                                    DataGridViewCell.PaintPadding(g, cellValueBounds, cellStyle, cachedBrush, base.DataGridView.RightToLeftInternal);
                                }
                                if (DataGridViewCell.PaintContentForeground(paintParts))
                                {
                                    if (image != null)
                                    {
                                        ImageAttributes imageAttr = new ImageAttributes();
                                        imageAttr.SetWrapMode(WrapMode.TileFlipXY);
                                        g.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttr);
                                        imageAttr.Dispose();
                                    }
                                    else
                                    {
                                        g.DrawIcon(icon, destRect);
                                    }
                                }
                            }
                            empty = destRect;
                            goto Label_037E;
                    }
                    Rectangle a = this.ImgBounds(destRect, (image == null) ? icon.Width : image.Width, (image == null) ? icon.Height : image.Height, imageLayout, cellStyle);
                    empty = a;
                    if (paint)
                    {
                        if (DataGridViewCell.PaintBackground(paintParts) && (cachedBrush.Color.A == 0xff))
                        {
                            g.FillRectangle(cachedBrush, cellValueBounds);
                        }
                        if (DataGridViewCell.PaintContentForeground(paintParts))
                        {
                            Region clip = g.Clip;
                            g.SetClip(Rectangle.Intersect(Rectangle.Intersect(a, destRect), Rectangle.Truncate(g.VisibleClipBounds)));
                            if (image != null)
                            {
                                g.DrawImage(image, a);
                            }
                            else
                            {
                                g.DrawIconUnstretched(icon, a);
                            }
                            g.Clip = clip;
                        }
                    }
                }
                else
                {
                    if ((paint && DataGridViewCell.PaintBackground(paintParts)) && (cachedBrush.Color.A == 0xff))
                    {
                        g.FillRectangle(cachedBrush, cellValueBounds);
                    }
                    empty = Rectangle.Empty;
                }
            }
            else
            {
                if ((paint && DataGridViewCell.PaintBackground(paintParts)) && (cachedBrush.Color.A == 0xff))
                {
                    g.FillRectangle(cachedBrush, cellValueBounds);
                }
                empty = Rectangle.Empty;
            }
        Label_037E:
            point = base.DataGridView.CurrentCellAddress;
            if (((paint && DataGridViewCell.PaintFocus(paintParts)) && ((point.X == base.ColumnIndex) && (point.Y == rowIndex))) && (base.DataGridView.ShowFocusCues && base.DataGridView.Focused))
            {
                ControlPaint.DrawFocusRectangle(g, cellValueBounds, Color.Empty, cachedBrush.Color);
            }
            if ((base.DataGridView.ShowCellErrors && paint) && DataGridViewCell.PaintErrorIcon(paintParts))
            {
                base.PaintErrorIcon(g, cellStyle, rowIndex, cellBounds, cellValueBounds, errorText);
            }
            return empty;
        }

        public override string ToString()
        {
            return ("DataGridViewImageCell { ColumnIndex=" + base.ColumnIndex.ToString(CultureInfo.CurrentCulture) + ", RowIndex=" + base.RowIndex.ToString(CultureInfo.CurrentCulture) + " }");
        }

        public override object DefaultNewRowValue
        {
            get
            {
                if (defaultTypeImage.IsAssignableFrom(this.ValueType))
                {
                    return ErrorBitmap;
                }
                if (defaultTypeIcon.IsAssignableFrom(this.ValueType))
                {
                    return ErrorIcon;
                }
                return null;
            }
        }

        [DefaultValue("")]
        public string Description
        {
            get
            {
                object obj2 = base.Properties.GetObject(PropImageCellDescription);
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) || base.Properties.ContainsObject(PropImageCellDescription))
                {
                    base.Properties.SetObject(PropImageCellDescription, value);
                }
            }
        }

        public override System.Type EditType
        {
            get
            {
                return null;
            }
        }

        internal static Bitmap ErrorBitmap
        {
            get
            {
                if (errorBmp == null)
                {
                    errorBmp = new Bitmap(typeof(DataGridView), "ImageInError.bmp");
                }
                return errorBmp;
            }
        }

        internal static Icon ErrorIcon
        {
            get
            {
                if (errorIco == null)
                {
                    errorIco = new Icon(typeof(DataGridView), "IconInError.ico");
                }
                return errorIco;
            }
        }

        public override System.Type FormattedValueType
        {
            get
            {
                if (this.ValueIsIcon)
                {
                    return defaultTypeIcon;
                }
                return defaultTypeImage;
            }
        }

        [DefaultValue(0)]
        public DataGridViewImageCellLayout ImageLayout
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropImageCellLayout, out flag);
                if (flag)
                {
                    return (DataGridViewImageCellLayout) integer;
                }
                return DataGridViewImageCellLayout.Normal;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DataGridViewImageCellLayout));
                }
                if (this.ImageLayout != value)
                {
                    base.Properties.SetInteger(PropImageCellLayout, (int) value);
                    base.OnCommonChange();
                }
            }
        }

        internal DataGridViewImageCellLayout ImageLayoutInternal
        {
            set
            {
                if (this.ImageLayout != value)
                {
                    base.Properties.SetInteger(PropImageCellLayout, (int) value);
                }
            }
        }

        [DefaultValue(false)]
        public bool ValueIsIcon
        {
            get
            {
                return ((this.flags & 1) != 0);
            }
            set
            {
                if (this.ValueIsIcon != value)
                {
                    this.ValueIsIconInternal = value;
                    if (base.DataGridView != null)
                    {
                        if (base.RowIndex != -1)
                        {
                            base.DataGridView.InvalidateCell(this);
                        }
                        else
                        {
                            base.DataGridView.InvalidateColumnInternal(base.ColumnIndex);
                        }
                    }
                }
            }
        }

        internal bool ValueIsIconInternal
        {
            set
            {
                if (this.ValueIsIcon != value)
                {
                    if (value)
                    {
                        this.flags = (byte) (this.flags | 1);
                    }
                    else
                    {
                        this.flags = (byte) (this.flags & -2);
                    }
                    if ((((base.DataGridView != null) && (base.RowIndex != -1)) && ((base.DataGridView.NewRowIndex == base.RowIndex) && !base.DataGridView.VirtualMode)) && ((value && (base.Value == ErrorBitmap)) || (!value && (base.Value == ErrorIcon))))
                    {
                        base.Value = this.DefaultNewRowValue;
                    }
                }
            }
        }

        public override System.Type ValueType
        {
            get
            {
                System.Type valueType = base.ValueType;
                if (valueType != null)
                {
                    return valueType;
                }
                if (this.ValueIsIcon)
                {
                    return defaultTypeIcon;
                }
                return defaultTypeImage;
            }
            set
            {
                base.ValueType = value;
                this.ValueIsIcon = (value != null) && defaultTypeIcon.IsAssignableFrom(value);
            }
        }

        protected class DataGridViewImageCellAccessibleObject : DataGridViewCell.DataGridViewCellAccessibleObject
        {
            public DataGridViewImageCellAccessibleObject(DataGridViewCell owner) : base(owner)
            {
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
            }

            public override int GetChildCount()
            {
                return 0;
            }

            public override string DefaultAction
            {
                get
                {
                    return string.Empty;
                }
            }

            public override string Description
            {
                get
                {
                    DataGridViewImageCell owner = base.Owner as DataGridViewImageCell;
                    if (owner != null)
                    {
                        return owner.Description;
                    }
                    return null;
                }
            }

            public override string Value
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return base.Value;
                }
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                set
                {
                }
            }
        }
    }
}

