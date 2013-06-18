namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Security.Permissions;

    public class DataGridViewLinkCell : DataGridViewCell
    {
        private static readonly DataGridViewContentAlignment anyBottom = (DataGridViewContentAlignment.BottomRight | DataGridViewContentAlignment.BottomCenter | DataGridViewContentAlignment.BottomLeft);
        private static readonly DataGridViewContentAlignment anyLeft = (DataGridViewContentAlignment.BottomLeft | DataGridViewContentAlignment.MiddleLeft | DataGridViewContentAlignment.TopLeft);
        private static readonly DataGridViewContentAlignment anyRight = (DataGridViewContentAlignment.BottomRight | DataGridViewContentAlignment.MiddleRight | DataGridViewContentAlignment.TopRight);
        private static System.Type cellType = typeof(DataGridViewLinkCell);
        private static Cursor dataGridViewCursor = null;
        private const byte DATAGRIDVIEWLINKCELL_horizontalTextMarginLeft = 1;
        private const byte DATAGRIDVIEWLINKCELL_horizontalTextMarginRight = 2;
        private const byte DATAGRIDVIEWLINKCELL_verticalTextMarginBottom = 1;
        private const byte DATAGRIDVIEWLINKCELL_verticalTextMarginTop = 1;
        private static System.Type defaultFormattedValueType = typeof(string);
        private static System.Type defaultValueType = typeof(object);
        private bool linkVisited;
        private bool linkVisitedSet;
        private static readonly int PropLinkCellActiveLinkColor = PropertyStore.CreateKey();
        private static readonly int PropLinkCellLinkBehavior = PropertyStore.CreateKey();
        private static readonly int PropLinkCellLinkColor = PropertyStore.CreateKey();
        private static readonly int PropLinkCellLinkState = PropertyStore.CreateKey();
        private static readonly int PropLinkCellTrackVisitedState = PropertyStore.CreateKey();
        private static readonly int PropLinkCellUseColumnTextForLinkValue = PropertyStore.CreateKey();
        private static readonly int PropLinkCellVisitedLinkColor = PropertyStore.CreateKey();

        public override object Clone()
        {
            DataGridViewLinkCell cell;
            System.Type type = base.GetType();
            if (type == cellType)
            {
                cell = new DataGridViewLinkCell();
            }
            else
            {
                cell = (DataGridViewLinkCell) Activator.CreateInstance(type);
            }
            base.CloneInternal(cell);
            if (base.Properties.ContainsObject(PropLinkCellActiveLinkColor))
            {
                cell.ActiveLinkColorInternal = this.ActiveLinkColor;
            }
            if (base.Properties.ContainsInteger(PropLinkCellUseColumnTextForLinkValue))
            {
                cell.UseColumnTextForLinkValueInternal = this.UseColumnTextForLinkValue;
            }
            if (base.Properties.ContainsInteger(PropLinkCellLinkBehavior))
            {
                cell.LinkBehaviorInternal = this.LinkBehavior;
            }
            if (base.Properties.ContainsObject(PropLinkCellLinkColor))
            {
                cell.LinkColorInternal = this.LinkColor;
            }
            if (base.Properties.ContainsInteger(PropLinkCellTrackVisitedState))
            {
                cell.TrackVisitedStateInternal = this.TrackVisitedState;
            }
            if (base.Properties.ContainsObject(PropLinkCellVisitedLinkColor))
            {
                cell.VisitedLinkColorInternal = this.VisitedLinkColor;
            }
            if (this.linkVisitedSet)
            {
                cell.LinkVisited = this.LinkVisited;
            }
            return cell;
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new DataGridViewLinkCellAccessibleObject(this);
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

        protected override Size GetPreferredSize(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
        {
            Size size;
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
            string str = base.GetFormattedValue(rowIndex, ref cellStyle, DataGridViewDataErrorContexts.PreferredSize | DataGridViewDataErrorContexts.Formatting) as string;
            if (string.IsNullOrEmpty(str))
            {
                str = " ";
            }
            TextFormatFlags flags = DataGridViewUtilities.ComputeTextFormatFlagsForCellStyleAlignment(base.DataGridView.RightToLeftInternal, cellStyle.Alignment, cellStyle.WrapMode);
            if ((cellStyle.WrapMode == DataGridViewTriState.True) && (str.Length > 1))
            {
                switch (freeDimensionFromConstraint)
                {
                    case DataGridViewFreeDimension.Height:
                        size = new Size(0, DataGridViewCell.MeasureTextHeight(graphics, str, cellStyle.Font, Math.Max(1, ((constraintSize.Width - num) - 1) - 2), flags));
                        goto Label_01DF;

                    case DataGridViewFreeDimension.Width:
                    {
                        int num3 = ((constraintSize.Height - num2) - 1) - 1;
                        if ((cellStyle.Alignment & anyBottom) != DataGridViewContentAlignment.NotSet)
                        {
                            num3--;
                        }
                        size = new Size(DataGridViewCell.MeasureTextWidth(graphics, str, cellStyle.Font, Math.Max(1, num3), flags), 0);
                        goto Label_01DF;
                    }
                }
                size = DataGridViewCell.MeasureTextPreferredSize(graphics, str, cellStyle.Font, 5f, flags);
            }
            else
            {
                switch (freeDimensionFromConstraint)
                {
                    case DataGridViewFreeDimension.Height:
                        size = new Size(0, DataGridViewCell.MeasureTextSize(graphics, str, cellStyle.Font, flags).Height);
                        goto Label_01DF;

                    case DataGridViewFreeDimension.Width:
                        size = new Size(DataGridViewCell.MeasureTextSize(graphics, str, cellStyle.Font, flags).Width, 0);
                        goto Label_01DF;
                }
                size = DataGridViewCell.MeasureTextSize(graphics, str, cellStyle.Font, flags);
            }
        Label_01DF:
            if (freeDimensionFromConstraint != DataGridViewFreeDimension.Height)
            {
                size.Width += 3 + num;
                if (base.DataGridView.ShowCellErrors)
                {
                    size.Width = Math.Max(size.Width, (num + 8) + 12);
                }
            }
            if (freeDimensionFromConstraint != DataGridViewFreeDimension.Width)
            {
                size.Height += 2 + num2;
                if ((cellStyle.Alignment & anyBottom) != DataGridViewContentAlignment.NotSet)
                {
                    size.Height++;
                }
                if (base.DataGridView.ShowCellErrors)
                {
                    size.Height = Math.Max(size.Height, (num2 + 8) + 11);
                }
            }
            return size;
        }

        protected override object GetValue(int rowIndex)
        {
            if (((this.UseColumnTextForLinkValue && (base.DataGridView != null)) && ((base.DataGridView.NewRowIndex != rowIndex) && (base.OwningColumn != null))) && (base.OwningColumn is DataGridViewLinkColumn))
            {
                return ((DataGridViewLinkColumn) base.OwningColumn).Text;
            }
            return base.GetValue(rowIndex);
        }

        protected override bool KeyUpUnsharesRow(KeyEventArgs e, int rowIndex)
        {
            return ((((e.KeyCode != Keys.Space) || e.Alt) || (e.Control || e.Shift)) || (this.TrackVisitedState && !this.LinkVisited));
        }

        private bool LinkBoundsContainPoint(int x, int y, int rowIndex)
        {
            return base.GetContentBounds(rowIndex).Contains(x, y);
        }

        protected override bool MouseDownUnsharesRow(DataGridViewCellMouseEventArgs e)
        {
            return this.LinkBoundsContainPoint(e.X, e.Y, e.RowIndex);
        }

        protected override bool MouseLeaveUnsharesRow(int rowIndex)
        {
            return (this.LinkState != System.Windows.Forms.LinkState.Normal);
        }

        protected override bool MouseMoveUnsharesRow(DataGridViewCellMouseEventArgs e)
        {
            if (this.LinkBoundsContainPoint(e.X, e.Y, e.RowIndex))
            {
                if ((this.LinkState & System.Windows.Forms.LinkState.Hover) == System.Windows.Forms.LinkState.Normal)
                {
                    return true;
                }
            }
            else if ((this.LinkState & System.Windows.Forms.LinkState.Hover) != System.Windows.Forms.LinkState.Normal)
            {
                return true;
            }
            return false;
        }

        protected override bool MouseUpUnsharesRow(DataGridViewCellMouseEventArgs e)
        {
            return (this.TrackVisitedState && this.LinkBoundsContainPoint(e.X, e.Y, e.RowIndex));
        }

        protected override void OnKeyUp(KeyEventArgs e, int rowIndex)
        {
            if ((base.DataGridView != null) && (((e.KeyCode == Keys.Space) && !e.Alt) && (!e.Control && !e.Shift)))
            {
                base.RaiseCellClick(new DataGridViewCellEventArgs(base.ColumnIndex, rowIndex));
                if (((base.DataGridView != null) && (base.ColumnIndex < base.DataGridView.Columns.Count)) && (rowIndex < base.DataGridView.Rows.Count))
                {
                    base.RaiseCellContentClick(new DataGridViewCellEventArgs(base.ColumnIndex, rowIndex));
                    if (this.TrackVisitedState)
                    {
                        this.LinkVisited = true;
                    }
                }
                e.Handled = true;
            }
        }

        protected override void OnMouseDown(DataGridViewCellMouseEventArgs e)
        {
            if (base.DataGridView != null)
            {
                if (this.LinkBoundsContainPoint(e.X, e.Y, e.RowIndex))
                {
                    this.LinkState |= System.Windows.Forms.LinkState.Active;
                    base.DataGridView.InvalidateCell(base.ColumnIndex, e.RowIndex);
                }
                base.OnMouseDown(e);
            }
        }

        protected override void OnMouseLeave(int rowIndex)
        {
            if (base.DataGridView != null)
            {
                if (dataGridViewCursor != null)
                {
                    base.DataGridView.Cursor = dataGridViewCursor;
                    dataGridViewCursor = null;
                }
                if (this.LinkState != System.Windows.Forms.LinkState.Normal)
                {
                    this.LinkState = System.Windows.Forms.LinkState.Normal;
                    base.DataGridView.InvalidateCell(base.ColumnIndex, rowIndex);
                }
                base.OnMouseLeave(rowIndex);
            }
        }

        protected override void OnMouseMove(DataGridViewCellMouseEventArgs e)
        {
            if (base.DataGridView != null)
            {
                if (this.LinkBoundsContainPoint(e.X, e.Y, e.RowIndex))
                {
                    if ((this.LinkState & System.Windows.Forms.LinkState.Hover) == System.Windows.Forms.LinkState.Normal)
                    {
                        this.LinkState |= System.Windows.Forms.LinkState.Hover;
                        base.DataGridView.InvalidateCell(base.ColumnIndex, e.RowIndex);
                    }
                    if (dataGridViewCursor == null)
                    {
                        dataGridViewCursor = base.DataGridView.UserSetCursor;
                    }
                    if (base.DataGridView.Cursor != Cursors.Hand)
                    {
                        base.DataGridView.Cursor = Cursors.Hand;
                    }
                }
                else if ((this.LinkState & System.Windows.Forms.LinkState.Hover) != System.Windows.Forms.LinkState.Normal)
                {
                    this.LinkState &= ~System.Windows.Forms.LinkState.Hover;
                    base.DataGridView.Cursor = dataGridViewCursor;
                    base.DataGridView.InvalidateCell(base.ColumnIndex, e.RowIndex);
                }
                base.OnMouseMove(e);
            }
        }

        protected override void OnMouseUp(DataGridViewCellMouseEventArgs e)
        {
            if ((base.DataGridView != null) && (this.LinkBoundsContainPoint(e.X, e.Y, e.RowIndex) && this.TrackVisitedState))
            {
                this.LinkVisited = true;
            }
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            this.PaintPrivate(graphics, clipBounds, cellBounds, rowIndex, cellState, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts, false, false, true);
        }

        private Rectangle PaintPrivate(Graphics g, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts, bool computeContentBounds, bool computeErrorIconBounds, bool paint)
        {
            if (paint && DataGridViewCell.PaintBorder(paintParts))
            {
                this.PaintBorder(g, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
            }
            Rectangle empty = Rectangle.Empty;
            Rectangle rectangle2 = this.BorderWidths(advancedBorderStyle);
            Rectangle rect = cellBounds;
            rect.Offset(rectangle2.X, rectangle2.Y);
            rect.Width -= rectangle2.Right;
            rect.Height -= rectangle2.Bottom;
            Point currentCellAddress = base.DataGridView.CurrentCellAddress;
            bool flag = (currentCellAddress.X == base.ColumnIndex) && (currentCellAddress.Y == rowIndex);
            bool flag2 = (cellState & DataGridViewElementStates.Selected) != DataGridViewElementStates.None;
            SolidBrush cachedBrush = base.DataGridView.GetCachedBrush((DataGridViewCell.PaintSelectionBackground(paintParts) && flag2) ? cellStyle.SelectionBackColor : cellStyle.BackColor);
            if ((paint && DataGridViewCell.PaintBackground(paintParts)) && (cachedBrush.Color.A == 0xff))
            {
                g.FillRectangle(cachedBrush, rect);
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
            Rectangle rectangle = rect;
            string text = formattedValue as string;
            if ((text != null) && (paint || computeContentBounds))
            {
                rect.Offset(1, 1);
                rect.Width -= 3;
                rect.Height -= 2;
                if ((cellStyle.Alignment & anyBottom) != DataGridViewContentAlignment.NotSet)
                {
                    rect.Height--;
                }
                Font linkFont = null;
                Font hoverLinkFont = null;
                LinkUtilities.EnsureLinkFonts(cellStyle.Font, this.LinkBehavior, ref linkFont, ref hoverLinkFont);
                TextFormatFlags flags = DataGridViewUtilities.ComputeTextFormatFlagsForCellStyleAlignment(base.DataGridView.RightToLeftInternal, cellStyle.Alignment, cellStyle.WrapMode);
                if (paint)
                {
                    if ((rect.Width > 0) && (rect.Height > 0))
                    {
                        Color activeLinkColor;
                        if ((flag && base.DataGridView.ShowFocusCues) && (base.DataGridView.Focused && DataGridViewCell.PaintFocus(paintParts)))
                        {
                            Rectangle rectangle5 = DataGridViewUtilities.GetTextBounds(rect, text, flags, cellStyle, (this.LinkState == System.Windows.Forms.LinkState.Hover) ? hoverLinkFont : linkFont);
                            if ((cellStyle.Alignment & anyLeft) != DataGridViewContentAlignment.NotSet)
                            {
                                rectangle5.X--;
                                rectangle5.Width++;
                            }
                            else if ((cellStyle.Alignment & anyRight) != DataGridViewContentAlignment.NotSet)
                            {
                                rectangle5.X++;
                                rectangle5.Width++;
                            }
                            rectangle5.Height += 2;
                            ControlPaint.DrawFocusRectangle(g, rectangle5, Color.Empty, cachedBrush.Color);
                        }
                        if ((this.LinkState & System.Windows.Forms.LinkState.Active) == System.Windows.Forms.LinkState.Active)
                        {
                            activeLinkColor = this.ActiveLinkColor;
                        }
                        else if (this.LinkVisited)
                        {
                            activeLinkColor = this.VisitedLinkColor;
                        }
                        else
                        {
                            activeLinkColor = this.LinkColor;
                        }
                        if (DataGridViewCell.PaintContentForeground(paintParts))
                        {
                            if ((flags & TextFormatFlags.SingleLine) != TextFormatFlags.Default)
                            {
                                flags |= TextFormatFlags.EndEllipsis;
                            }
                            TextRenderer.DrawText(g, text, (this.LinkState == System.Windows.Forms.LinkState.Hover) ? hoverLinkFont : linkFont, rect, activeLinkColor, flags);
                        }
                    }
                    else if (((flag && base.DataGridView.ShowFocusCues) && (base.DataGridView.Focused && DataGridViewCell.PaintFocus(paintParts))) && ((rectangle.Width > 0) && (rectangle.Height > 0)))
                    {
                        ControlPaint.DrawFocusRectangle(g, rectangle, Color.Empty, cachedBrush.Color);
                    }
                }
                else
                {
                    empty = DataGridViewUtilities.GetTextBounds(rect, text, flags, cellStyle, (this.LinkState == System.Windows.Forms.LinkState.Hover) ? hoverLinkFont : linkFont);
                }
                linkFont.Dispose();
                hoverLinkFont.Dispose();
            }
            else if (paint || computeContentBounds)
            {
                if (((flag && base.DataGridView.ShowFocusCues) && (base.DataGridView.Focused && DataGridViewCell.PaintFocus(paintParts))) && ((paint && (rect.Width > 0)) && (rect.Height > 0)))
                {
                    ControlPaint.DrawFocusRectangle(g, rect, Color.Empty, cachedBrush.Color);
                }
            }
            else if (computeErrorIconBounds && !string.IsNullOrEmpty(errorText))
            {
                empty = base.ComputeErrorIconBounds(rectangle);
            }
            if ((base.DataGridView.ShowCellErrors && paint) && DataGridViewCell.PaintErrorIcon(paintParts))
            {
                base.PaintErrorIcon(g, cellStyle, rowIndex, cellBounds, rectangle, errorText);
            }
            return empty;
        }

        private bool ShouldSerializeActiveLinkColor()
        {
            return !this.ActiveLinkColor.Equals(LinkUtilities.IEActiveLinkColor);
        }

        private bool ShouldSerializeLinkColor()
        {
            return !this.LinkColor.Equals(LinkUtilities.IELinkColor);
        }

        private bool ShouldSerializeLinkVisited()
        {
            return (this.linkVisitedSet = true);
        }

        private bool ShouldSerializeVisitedLinkColor()
        {
            return !this.VisitedLinkColor.Equals(LinkUtilities.IEVisitedLinkColor);
        }

        public override string ToString()
        {
            return ("DataGridViewLinkCell { ColumnIndex=" + base.ColumnIndex.ToString(CultureInfo.CurrentCulture) + ", RowIndex=" + base.RowIndex.ToString(CultureInfo.CurrentCulture) + " }");
        }

        public Color ActiveLinkColor
        {
            get
            {
                if (base.Properties.ContainsObject(PropLinkCellActiveLinkColor))
                {
                    return (Color) base.Properties.GetObject(PropLinkCellActiveLinkColor);
                }
                return LinkUtilities.IEActiveLinkColor;
            }
            set
            {
                if (!value.Equals(this.ActiveLinkColor))
                {
                    base.Properties.SetObject(PropLinkCellActiveLinkColor, value);
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

        internal Color ActiveLinkColorInternal
        {
            set
            {
                if (!value.Equals(this.ActiveLinkColor))
                {
                    base.Properties.SetObject(PropLinkCellActiveLinkColor, value);
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

        public override System.Type FormattedValueType
        {
            get
            {
                return defaultFormattedValueType;
            }
        }

        [DefaultValue(0)]
        public System.Windows.Forms.LinkBehavior LinkBehavior
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropLinkCellLinkBehavior, out flag);
                if (flag)
                {
                    return (System.Windows.Forms.LinkBehavior) integer;
                }
                return System.Windows.Forms.LinkBehavior.SystemDefault;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.LinkBehavior));
                }
                if (value != this.LinkBehavior)
                {
                    base.Properties.SetInteger(PropLinkCellLinkBehavior, (int) value);
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

        internal System.Windows.Forms.LinkBehavior LinkBehaviorInternal
        {
            set
            {
                if (value != this.LinkBehavior)
                {
                    base.Properties.SetInteger(PropLinkCellLinkBehavior, (int) value);
                }
            }
        }

        public Color LinkColor
        {
            get
            {
                if (base.Properties.ContainsObject(PropLinkCellLinkColor))
                {
                    return (Color) base.Properties.GetObject(PropLinkCellLinkColor);
                }
                return LinkUtilities.IELinkColor;
            }
            set
            {
                if (!value.Equals(this.LinkColor))
                {
                    base.Properties.SetObject(PropLinkCellLinkColor, value);
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

        internal Color LinkColorInternal
        {
            set
            {
                if (!value.Equals(this.LinkColor))
                {
                    base.Properties.SetObject(PropLinkCellLinkColor, value);
                }
            }
        }

        private System.Windows.Forms.LinkState LinkState
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropLinkCellLinkState, out flag);
                if (flag)
                {
                    return (System.Windows.Forms.LinkState) integer;
                }
                return System.Windows.Forms.LinkState.Normal;
            }
            set
            {
                if (this.LinkState != value)
                {
                    base.Properties.SetInteger(PropLinkCellLinkState, (int) value);
                }
            }
        }

        public bool LinkVisited
        {
            get
            {
                return (this.linkVisitedSet && this.linkVisited);
            }
            set
            {
                this.linkVisitedSet = true;
                if (value != this.LinkVisited)
                {
                    this.linkVisited = value;
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

        [DefaultValue(true)]
        public bool TrackVisitedState
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropLinkCellTrackVisitedState, out flag);
                return (!flag || (integer != 0));
            }
            set
            {
                if (value != this.TrackVisitedState)
                {
                    base.Properties.SetInteger(PropLinkCellTrackVisitedState, value ? 1 : 0);
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

        internal bool TrackVisitedStateInternal
        {
            set
            {
                if (value != this.TrackVisitedState)
                {
                    base.Properties.SetInteger(PropLinkCellTrackVisitedState, value ? 1 : 0);
                }
            }
        }

        [DefaultValue(false)]
        public bool UseColumnTextForLinkValue
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropLinkCellUseColumnTextForLinkValue, out flag);
                if (!flag)
                {
                    return false;
                }
                return (integer != 0);
            }
            set
            {
                if (value != this.UseColumnTextForLinkValue)
                {
                    base.Properties.SetInteger(PropLinkCellUseColumnTextForLinkValue, value ? 1 : 0);
                    base.OnCommonChange();
                }
            }
        }

        internal bool UseColumnTextForLinkValueInternal
        {
            set
            {
                if (value != this.UseColumnTextForLinkValue)
                {
                    base.Properties.SetInteger(PropLinkCellUseColumnTextForLinkValue, value ? 1 : 0);
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
                return defaultValueType;
            }
        }

        public Color VisitedLinkColor
        {
            get
            {
                if (base.Properties.ContainsObject(PropLinkCellVisitedLinkColor))
                {
                    return (Color) base.Properties.GetObject(PropLinkCellVisitedLinkColor);
                }
                return LinkUtilities.IEVisitedLinkColor;
            }
            set
            {
                if (!value.Equals(this.VisitedLinkColor))
                {
                    base.Properties.SetObject(PropLinkCellVisitedLinkColor, value);
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

        internal Color VisitedLinkColorInternal
        {
            set
            {
                if (!value.Equals(this.VisitedLinkColor))
                {
                    base.Properties.SetObject(PropLinkCellVisitedLinkColor, value);
                }
            }
        }

        protected class DataGridViewLinkCellAccessibleObject : DataGridViewCell.DataGridViewCellAccessibleObject
        {
            public DataGridViewLinkCellAccessibleObject(DataGridViewCell owner) : base(owner)
            {
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                DataGridViewLinkCell owner = (DataGridViewLinkCell) base.Owner;
                DataGridView dataGridView = owner.DataGridView;
                if ((dataGridView != null) && (owner.RowIndex == -1))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidOperationOnSharedCell"));
                }
                if ((owner.OwningColumn != null) && (owner.OwningRow != null))
                {
                    dataGridView.OnCellContentClickInternal(new DataGridViewCellEventArgs(owner.ColumnIndex, owner.RowIndex));
                }
            }

            public override int GetChildCount()
            {
                return 0;
            }

            public override string DefaultAction
            {
                get
                {
                    return System.Windows.Forms.SR.GetString("DataGridView_AccLinkCellDefaultAction");
                }
            }
        }
    }
}

