namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.ButtonInternal;
    using System.Windows.Forms.VisualStyles;

    public class DataGridViewCheckBoxCell : DataGridViewCell, IDataGridViewEditingCell
    {
        private static readonly DataGridViewContentAlignment anyBottom = (DataGridViewContentAlignment.BottomRight | DataGridViewContentAlignment.BottomCenter | DataGridViewContentAlignment.BottomLeft);
        private static readonly DataGridViewContentAlignment anyCenter = (DataGridViewContentAlignment.BottomCenter | DataGridViewContentAlignment.MiddleCenter | DataGridViewContentAlignment.TopCenter);
        private static readonly DataGridViewContentAlignment anyLeft = (DataGridViewContentAlignment.BottomLeft | DataGridViewContentAlignment.MiddleLeft | DataGridViewContentAlignment.TopLeft);
        private static readonly DataGridViewContentAlignment anyMiddle = (DataGridViewContentAlignment.MiddleRight | DataGridViewContentAlignment.MiddleCenter | DataGridViewContentAlignment.MiddleLeft);
        private static readonly DataGridViewContentAlignment anyRight = (DataGridViewContentAlignment.BottomRight | DataGridViewContentAlignment.MiddleRight | DataGridViewContentAlignment.TopRight);
        private static System.Type cellType = typeof(DataGridViewCheckBoxCell);
        private static readonly VisualStyleElement CheckBoxElement = VisualStyleElement.Button.CheckBox.UncheckedNormal;
        private static Bitmap checkImage = null;
        private const byte DATAGRIDVIEWCHECKBOXCELL_checked = 0x10;
        private const byte DATAGRIDVIEWCHECKBOXCELL_indeterminate = 0x20;
        private const byte DATAGRIDVIEWCHECKBOXCELL_margin = 2;
        private const byte DATAGRIDVIEWCHECKBOXCELL_threeState = 1;
        private const byte DATAGRIDVIEWCHECKBOXCELL_valueChanged = 2;
        private static System.Type defaultBooleanType = typeof(bool);
        private static System.Type defaultCheckStateType = typeof(CheckState);
        private byte flags;
        private static bool mouseInContentBounds = false;
        private static readonly int PropButtonCellState = PropertyStore.CreateKey();
        private static readonly int PropFalseValue = PropertyStore.CreateKey();
        private static readonly int PropFlatStyle = PropertyStore.CreateKey();
        private static readonly int PropIndeterminateValue = PropertyStore.CreateKey();
        private static readonly int PropTrueValue = PropertyStore.CreateKey();

        public DataGridViewCheckBoxCell() : this(false)
        {
        }

        public DataGridViewCheckBoxCell(bool threeState)
        {
            if (threeState)
            {
                this.flags = 1;
            }
        }

        public override object Clone()
        {
            DataGridViewCheckBoxCell cell;
            System.Type type = base.GetType();
            if (type == cellType)
            {
                cell = new DataGridViewCheckBoxCell();
            }
            else
            {
                cell = (DataGridViewCheckBoxCell) Activator.CreateInstance(type);
            }
            base.CloneInternal(cell);
            cell.ThreeStateInternal = this.ThreeState;
            cell.TrueValueInternal = this.TrueValue;
            cell.FalseValueInternal = this.FalseValue;
            cell.IndeterminateValueInternal = this.IndeterminateValue;
            cell.FlatStyleInternal = this.FlatStyle;
            return cell;
        }

        private bool CommonContentClickUnsharesRow(DataGridViewCellEventArgs e)
        {
            Point currentCellAddress = base.DataGridView.CurrentCellAddress;
            return (((currentCellAddress.X == base.ColumnIndex) && (currentCellAddress.Y == e.RowIndex)) && base.DataGridView.IsCurrentCellInEditMode);
        }

        protected override bool ContentClickUnsharesRow(DataGridViewCellEventArgs e)
        {
            return this.CommonContentClickUnsharesRow(e);
        }

        protected override bool ContentDoubleClickUnsharesRow(DataGridViewCellEventArgs e)
        {
            return this.CommonContentClickUnsharesRow(e);
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new DataGridViewCheckBoxCellAccessibleObject(this);
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
            base.ComputeBorderStyleCellStateAndCellBounds(rowIndex, out style, out states, out rectangle);
            return this.PaintPrivate(graphics, rectangle, rectangle, rowIndex, states, null, null, cellStyle, style, DataGridViewPaintParts.ContentForeground, true, false, false);
        }

        public virtual object GetEditingCellFormattedValue(DataGridViewDataErrorContexts context)
        {
            if (this.FormattedValueType == null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCell_FormattedValueTypeNull"));
            }
            if (this.FormattedValueType.IsAssignableFrom(defaultCheckStateType))
            {
                if ((this.flags & 0x10) != 0)
                {
                    if ((context & DataGridViewDataErrorContexts.ClipboardContent) != 0)
                    {
                        return System.Windows.Forms.SR.GetString("DataGridViewCheckBoxCell_ClipboardChecked");
                    }
                    return CheckState.Checked;
                }
                if ((this.flags & 0x20) != 0)
                {
                    if ((context & DataGridViewDataErrorContexts.ClipboardContent) != 0)
                    {
                        return System.Windows.Forms.SR.GetString("DataGridViewCheckBoxCell_ClipboardIndeterminate");
                    }
                    return CheckState.Indeterminate;
                }
                if ((context & DataGridViewDataErrorContexts.ClipboardContent) != 0)
                {
                    return System.Windows.Forms.SR.GetString("DataGridViewCheckBoxCell_ClipboardUnchecked");
                }
                return CheckState.Unchecked;
            }
            if (!this.FormattedValueType.IsAssignableFrom(defaultBooleanType))
            {
                return null;
            }
            bool flag = (this.flags & 0x10) != 0;
            if ((context & DataGridViewDataErrorContexts.ClipboardContent) != 0)
            {
                return System.Windows.Forms.SR.GetString(flag ? "DataGridViewCheckBoxCell_ClipboardTrue" : "DataGridViewCheckBoxCell_ClipboardFalse");
            }
            return flag;
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
            Point currentCellAddress = base.DataGridView.CurrentCellAddress;
            if (((currentCellAddress.X == base.ColumnIndex) && (currentCellAddress.Y == rowIndex)) && base.DataGridView.IsCurrentCellInEditMode)
            {
                return Rectangle.Empty;
            }
            base.ComputeBorderStyleCellStateAndCellBounds(rowIndex, out style, out states, out rectangle);
            return this.PaintPrivate(graphics, rectangle, rectangle, rowIndex, states, null, this.GetErrorText(rowIndex), cellStyle, style, DataGridViewPaintParts.ContentForeground, false, true, false);
        }

        protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
        {
            if (value != null)
            {
                if (this.ThreeState)
                {
                    if (value.Equals(this.TrueValue) || ((value is int) && (((int) value) == 1)))
                    {
                        value = CheckState.Checked;
                    }
                    else if (value.Equals(this.FalseValue) || ((value is int) && (((int) value) == 0)))
                    {
                        value = CheckState.Unchecked;
                    }
                    else if (value.Equals(this.IndeterminateValue) || ((value is int) && (((int) value) == 2)))
                    {
                        value = CheckState.Indeterminate;
                    }
                }
                else if (value.Equals(this.TrueValue) || ((value is int) && (((int) value) != 0)))
                {
                    value = true;
                }
                else if (value.Equals(this.FalseValue) || ((value is int) && (((int) value) == 0)))
                {
                    value = false;
                }
            }
            object obj2 = base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
            if ((obj2 == null) || ((context & DataGridViewDataErrorContexts.ClipboardContent) == 0))
            {
                return obj2;
            }
            if (obj2 is bool)
            {
                if ((bool) obj2)
                {
                    return System.Windows.Forms.SR.GetString(this.ThreeState ? "DataGridViewCheckBoxCell_ClipboardChecked" : "DataGridViewCheckBoxCell_ClipboardTrue");
                }
                return System.Windows.Forms.SR.GetString(this.ThreeState ? "DataGridViewCheckBoxCell_ClipboardUnchecked" : "DataGridViewCheckBoxCell_ClipboardFalse");
            }
            if (!(obj2 is CheckState))
            {
                return obj2;
            }
            switch (((CheckState) obj2))
            {
                case CheckState.Checked:
                    return System.Windows.Forms.SR.GetString(this.ThreeState ? "DataGridViewCheckBoxCell_ClipboardChecked" : "DataGridViewCheckBoxCell_ClipboardTrue");

                case CheckState.Unchecked:
                    return System.Windows.Forms.SR.GetString(this.ThreeState ? "DataGridViewCheckBoxCell_ClipboardUnchecked" : "DataGridViewCheckBoxCell_ClipboardFalse");
            }
            return System.Windows.Forms.SR.GetString("DataGridViewCheckBoxCell_ClipboardIndeterminate");
        }

        protected override Size GetPreferredSize(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
        {
            Size size;
            int num3;
            DataGridViewAdvancedBorderStyle style;
            DataGridViewElementStates states;
            Rectangle rectangle2;
            if (base.DataGridView == null)
            {
                return new Size(-1, -1);
            }
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            DataGridViewFreeDimension freeDimensionFromConstraint = DataGridViewCell.GetFreeDimensionFromConstraint(constraintSize);
            Rectangle stdBorderWidths = base.StdBorderWidths;
            int num = (stdBorderWidths.Left + stdBorderWidths.Width) + cellStyle.Padding.Horizontal;
            int num2 = (stdBorderWidths.Top + stdBorderWidths.Height) + cellStyle.Padding.Vertical;
            if (!base.DataGridView.ApplyVisualStylesToInnerCells)
            {
                switch (this.FlatStyle)
                {
                    case System.Windows.Forms.FlatStyle.Flat:
                        num3 = CheckBoxRenderer.GetGlyphSize(graphics, CheckBoxState.UncheckedNormal).Width - 3;
                        goto Label_01A9;

                    case System.Windows.Forms.FlatStyle.Popup:
                        num3 = CheckBoxRenderer.GetGlyphSize(graphics, CheckBoxState.UncheckedNormal).Width - 2;
                        goto Label_01A9;
                }
                num3 = ((SystemInformation.Border3DSize.Width * 2) + 9) + 4;
            }
            else
            {
                Size glyphSize = CheckBoxRenderer.GetGlyphSize(graphics, CheckBoxState.UncheckedNormal);
                switch (this.FlatStyle)
                {
                    case System.Windows.Forms.FlatStyle.Flat:
                        glyphSize.Width -= 3;
                        glyphSize.Height -= 3;
                        break;

                    case System.Windows.Forms.FlatStyle.Popup:
                        glyphSize.Width -= 2;
                        glyphSize.Height -= 2;
                        break;
                }
                switch (freeDimensionFromConstraint)
                {
                    case DataGridViewFreeDimension.Height:
                        size = new Size(0, (glyphSize.Height + num2) + 4);
                        goto Label_01EA;

                    case DataGridViewFreeDimension.Width:
                        size = new Size((glyphSize.Width + num) + 4, 0);
                        goto Label_01EA;

                    default:
                        size = new Size((glyphSize.Width + num) + 4, (glyphSize.Height + num2) + 4);
                        goto Label_01EA;
                }
            }
        Label_01A9:
            switch (freeDimensionFromConstraint)
            {
                case DataGridViewFreeDimension.Height:
                    size = new Size(0, num3 + num2);
                    break;

                case DataGridViewFreeDimension.Width:
                    size = new Size(num3 + num, 0);
                    break;

                default:
                    size = new Size(num3 + num, num3 + num2);
                    break;
            }
        Label_01EA:
            base.ComputeBorderStyleCellStateAndCellBounds(rowIndex, out style, out states, out rectangle2);
            Rectangle rectangle3 = this.BorderWidths(style);
            size.Width += rectangle3.X;
            size.Height += rectangle3.Y;
            if (base.DataGridView.ShowCellErrors)
            {
                if (freeDimensionFromConstraint != DataGridViewFreeDimension.Height)
                {
                    size.Width = Math.Max(size.Width, (num + 8) + 12);
                }
                if (freeDimensionFromConstraint != DataGridViewFreeDimension.Width)
                {
                    size.Height = Math.Max(size.Height, (num2 + 8) + 11);
                }
            }
            return size;
        }

        protected override bool KeyDownUnsharesRow(KeyEventArgs e, int rowIndex)
        {
            return ((((e.KeyCode == Keys.Space) && !e.Alt) && !e.Control) && !e.Shift);
        }

        protected override bool KeyUpUnsharesRow(KeyEventArgs e, int rowIndex)
        {
            return (e.KeyCode == Keys.Space);
        }

        protected override bool MouseDownUnsharesRow(DataGridViewCellMouseEventArgs e)
        {
            return (e.Button == MouseButtons.Left);
        }

        protected override bool MouseEnterUnsharesRow(int rowIndex)
        {
            return ((base.ColumnIndex == base.DataGridView.MouseDownCellAddress.X) && (rowIndex == base.DataGridView.MouseDownCellAddress.Y));
        }

        protected override bool MouseLeaveUnsharesRow(int rowIndex)
        {
            return ((this.ButtonState & System.Windows.Forms.ButtonState.Pushed) != System.Windows.Forms.ButtonState.Normal);
        }

        protected override bool MouseUpUnsharesRow(DataGridViewCellMouseEventArgs e)
        {
            return (e.Button == MouseButtons.Left);
        }

        private void NotifyDataGridViewOfValueChange()
        {
            this.flags = (byte) (this.flags | 2);
            base.DataGridView.NotifyCurrentCellDirty(true);
        }

        private void NotifyMASSClient(Point position)
        {
            int num = base.DataGridView.Rows.GetRowCount(DataGridViewElementStates.Visible, 0, position.Y);
            int num2 = base.DataGridView.Columns.ColumnIndexToActualDisplayIndex(position.X, DataGridViewElementStates.Visible);
            int num3 = base.DataGridView.ColumnHeadersVisible ? 1 : 0;
            int num4 = base.DataGridView.RowHeadersVisible ? 1 : 0;
            int objectID = (num + num3) + 1;
            int childID = num2 + num4;
            (base.DataGridView.AccessibilityObject as Control.ControlAccessibleObject).NotifyClients(AccessibleEvents.StateChange, objectID, childID);
        }

        private void OnCommonContentClick(DataGridViewCellEventArgs e)
        {
            if (base.DataGridView != null)
            {
                Point currentCellAddress = base.DataGridView.CurrentCellAddress;
                if (((currentCellAddress.X == base.ColumnIndex) && (currentCellAddress.Y == e.RowIndex)) && (base.DataGridView.IsCurrentCellInEditMode && this.SwitchFormattedValue()))
                {
                    this.NotifyDataGridViewOfValueChange();
                }
            }
        }

        protected override void OnContentClick(DataGridViewCellEventArgs e)
        {
            this.OnCommonContentClick(e);
        }

        protected override void OnContentDoubleClick(DataGridViewCellEventArgs e)
        {
            this.OnCommonContentClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e, int rowIndex)
        {
            if ((base.DataGridView != null) && (((e.KeyCode == Keys.Space) && !e.Alt) && (!e.Control && !e.Shift)))
            {
                this.UpdateButtonState(this.ButtonState | System.Windows.Forms.ButtonState.Checked, rowIndex);
                e.Handled = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e, int rowIndex)
        {
            if ((base.DataGridView != null) && (e.KeyCode == Keys.Space))
            {
                this.UpdateButtonState(this.ButtonState & ~System.Windows.Forms.ButtonState.Checked, rowIndex);
                if ((!e.Alt && !e.Control) && !e.Shift)
                {
                    base.RaiseCellClick(new DataGridViewCellEventArgs(base.ColumnIndex, rowIndex));
                    if (((base.DataGridView != null) && (base.ColumnIndex < base.DataGridView.Columns.Count)) && (rowIndex < base.DataGridView.Rows.Count))
                    {
                        base.RaiseCellContentClick(new DataGridViewCellEventArgs(base.ColumnIndex, rowIndex));
                    }
                    e.Handled = true;
                }
                this.NotifyMASSClient(new Point(base.ColumnIndex, rowIndex));
            }
        }

        protected override void OnLeave(int rowIndex, bool throughMouseClick)
        {
            if ((base.DataGridView != null) && (this.ButtonState != System.Windows.Forms.ButtonState.Normal))
            {
                this.UpdateButtonState(System.Windows.Forms.ButtonState.Normal, rowIndex);
            }
        }

        protected override void OnMouseDown(DataGridViewCellMouseEventArgs e)
        {
            if ((base.DataGridView != null) && ((e.Button == MouseButtons.Left) && mouseInContentBounds))
            {
                this.UpdateButtonState(this.ButtonState | System.Windows.Forms.ButtonState.Pushed, e.RowIndex);
            }
        }

        protected override void OnMouseLeave(int rowIndex)
        {
            if (base.DataGridView != null)
            {
                if (mouseInContentBounds)
                {
                    mouseInContentBounds = false;
                    if (((base.ColumnIndex >= 0) && (rowIndex >= 0)) && ((base.DataGridView.ApplyVisualStylesToInnerCells || (this.FlatStyle == System.Windows.Forms.FlatStyle.Flat)) || (this.FlatStyle == System.Windows.Forms.FlatStyle.Popup)))
                    {
                        base.DataGridView.InvalidateCell(base.ColumnIndex, rowIndex);
                    }
                }
                if ((((this.ButtonState & System.Windows.Forms.ButtonState.Pushed) != System.Windows.Forms.ButtonState.Normal) && (base.ColumnIndex == base.DataGridView.MouseDownCellAddress.X)) && (rowIndex == base.DataGridView.MouseDownCellAddress.Y))
                {
                    this.UpdateButtonState(this.ButtonState & ~System.Windows.Forms.ButtonState.Pushed, rowIndex);
                }
            }
        }

        protected override void OnMouseMove(DataGridViewCellMouseEventArgs e)
        {
            if (base.DataGridView != null)
            {
                bool mouseInContentBounds = DataGridViewCheckBoxCell.mouseInContentBounds;
                DataGridViewCheckBoxCell.mouseInContentBounds = base.GetContentBounds(e.RowIndex).Contains(e.X, e.Y);
                if (mouseInContentBounds != DataGridViewCheckBoxCell.mouseInContentBounds)
                {
                    if ((base.DataGridView.ApplyVisualStylesToInnerCells || (this.FlatStyle == System.Windows.Forms.FlatStyle.Flat)) || (this.FlatStyle == System.Windows.Forms.FlatStyle.Popup))
                    {
                        base.DataGridView.InvalidateCell(base.ColumnIndex, e.RowIndex);
                    }
                    if (((e.ColumnIndex == base.DataGridView.MouseDownCellAddress.X) && (e.RowIndex == base.DataGridView.MouseDownCellAddress.Y)) && (Control.MouseButtons == MouseButtons.Left))
                    {
                        if ((((this.ButtonState & System.Windows.Forms.ButtonState.Pushed) == System.Windows.Forms.ButtonState.Normal) && DataGridViewCheckBoxCell.mouseInContentBounds) && base.DataGridView.CellMouseDownInContentBounds)
                        {
                            this.UpdateButtonState(this.ButtonState | System.Windows.Forms.ButtonState.Pushed, e.RowIndex);
                        }
                        else if (((this.ButtonState & System.Windows.Forms.ButtonState.Pushed) != System.Windows.Forms.ButtonState.Normal) && !DataGridViewCheckBoxCell.mouseInContentBounds)
                        {
                            this.UpdateButtonState(this.ButtonState & ~System.Windows.Forms.ButtonState.Pushed, e.RowIndex);
                        }
                    }
                }
                base.OnMouseMove(e);
            }
        }

        protected override void OnMouseUp(DataGridViewCellMouseEventArgs e)
        {
            if ((base.DataGridView != null) && (e.Button == MouseButtons.Left))
            {
                this.UpdateButtonState(this.ButtonState & ~System.Windows.Forms.ButtonState.Pushed, e.RowIndex);
                this.NotifyMASSClient(new Point(e.ColumnIndex, e.RowIndex));
            }
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
            Rectangle checkBounds;
            CheckState @unchecked;
            System.Windows.Forms.ButtonState normal;
            Size glyphSize;
            if (paint && DataGridViewCell.PaintBorder(paintParts))
            {
                this.PaintBorder(g, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
            }
            Rectangle rectangle2 = cellBounds;
            Rectangle rectangle3 = this.BorderWidths(advancedBorderStyle);
            rectangle2.Offset(rectangle3.X, rectangle3.Y);
            rectangle2.Width -= rectangle3.Right;
            rectangle2.Height -= rectangle3.Bottom;
            bool flag = (elementState & DataGridViewElementStates.Selected) != DataGridViewElementStates.None;
            bool isMixed = false;
            bool flag3 = true;
            Point currentCellAddress = base.DataGridView.CurrentCellAddress;
            if (((currentCellAddress.X == base.ColumnIndex) && (currentCellAddress.Y == rowIndex)) && base.DataGridView.IsCurrentCellInEditMode)
            {
                flag3 = false;
            }
            if ((formattedValue != null) && (formattedValue is CheckState))
            {
                @unchecked = (CheckState) formattedValue;
                normal = (@unchecked == CheckState.Unchecked) ? System.Windows.Forms.ButtonState.Normal : System.Windows.Forms.ButtonState.Checked;
                isMixed = @unchecked == CheckState.Indeterminate;
            }
            else if ((formattedValue != null) && (formattedValue is bool))
            {
                if ((bool) formattedValue)
                {
                    @unchecked = CheckState.Checked;
                    normal = System.Windows.Forms.ButtonState.Checked;
                }
                else
                {
                    @unchecked = CheckState.Unchecked;
                    normal = System.Windows.Forms.ButtonState.Normal;
                }
            }
            else
            {
                normal = System.Windows.Forms.ButtonState.Normal;
                @unchecked = CheckState.Unchecked;
            }
            if ((this.ButtonState & (System.Windows.Forms.ButtonState.Checked | System.Windows.Forms.ButtonState.Pushed)) != System.Windows.Forms.ButtonState.Normal)
            {
                normal |= System.Windows.Forms.ButtonState.Pushed;
            }
            SolidBrush cachedBrush = base.DataGridView.GetCachedBrush((DataGridViewCell.PaintSelectionBackground(paintParts) && flag) ? cellStyle.SelectionBackColor : cellStyle.BackColor);
            if ((paint && DataGridViewCell.PaintBackground(paintParts)) && (cachedBrush.Color.A == 0xff))
            {
                g.FillRectangle(cachedBrush, rectangle2);
            }
            if (cellStyle.Padding != Padding.Empty)
            {
                if (base.DataGridView.RightToLeftInternal)
                {
                    rectangle2.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
                }
                else
                {
                    rectangle2.Offset(cellStyle.Padding.Left, cellStyle.Padding.Top);
                }
                rectangle2.Width -= cellStyle.Padding.Horizontal;
                rectangle2.Height -= cellStyle.Padding.Vertical;
            }
            if (((paint && DataGridViewCell.PaintFocus(paintParts)) && (base.DataGridView.ShowFocusCues && base.DataGridView.Focused)) && ((currentCellAddress.X == base.ColumnIndex) && (currentCellAddress.Y == rowIndex)))
            {
                ControlPaint.DrawFocusRectangle(g, rectangle2, Color.Empty, cachedBrush.Color);
            }
            Rectangle cellValueBounds = rectangle2;
            rectangle2.Inflate(-2, -2);
            CheckBoxState uncheckedNormal = CheckBoxState.UncheckedNormal;
            if (base.DataGridView.ApplyVisualStylesToInnerCells)
            {
                uncheckedNormal = CheckBoxRenderer.ConvertFromButtonState(normal, isMixed, ((base.DataGridView.MouseEnteredCellAddress.Y == rowIndex) && (base.DataGridView.MouseEnteredCellAddress.X == base.ColumnIndex)) && mouseInContentBounds);
                glyphSize = CheckBoxRenderer.GetGlyphSize(g, uncheckedNormal);
                switch (this.FlatStyle)
                {
                    case System.Windows.Forms.FlatStyle.Flat:
                        glyphSize.Width -= 3;
                        glyphSize.Height -= 3;
                        break;

                    case System.Windows.Forms.FlatStyle.Popup:
                        glyphSize.Width -= 2;
                        glyphSize.Height -= 2;
                        break;
                }
            }
            else
            {
                switch (this.FlatStyle)
                {
                    case System.Windows.Forms.FlatStyle.Flat:
                        glyphSize = CheckBoxRenderer.GetGlyphSize(g, CheckBoxState.UncheckedNormal);
                        glyphSize.Width -= 3;
                        glyphSize.Height -= 3;
                        goto Label_03EF;

                    case System.Windows.Forms.FlatStyle.Popup:
                        glyphSize = CheckBoxRenderer.GetGlyphSize(g, CheckBoxState.UncheckedNormal);
                        glyphSize.Width -= 2;
                        glyphSize.Height -= 2;
                        goto Label_03EF;
                }
                glyphSize = new Size((SystemInformation.Border3DSize.Width * 2) + 9, (SystemInformation.Border3DSize.Width * 2) + 9);
            }
        Label_03EF:
            if (((rectangle2.Width >= glyphSize.Width) && (rectangle2.Height >= glyphSize.Height)) && (paint || computeContentBounds))
            {
                int x = 0;
                int y = 0;
                if ((!base.DataGridView.RightToLeftInternal && ((cellStyle.Alignment & anyRight) != DataGridViewContentAlignment.NotSet)) || (base.DataGridView.RightToLeftInternal && ((cellStyle.Alignment & anyLeft) != DataGridViewContentAlignment.NotSet)))
                {
                    x = rectangle2.Right - glyphSize.Width;
                }
                else if ((cellStyle.Alignment & anyCenter) != DataGridViewContentAlignment.NotSet)
                {
                    x = rectangle2.Left + ((rectangle2.Width - glyphSize.Width) / 2);
                }
                else
                {
                    x = rectangle2.Left;
                }
                if ((cellStyle.Alignment & anyBottom) != DataGridViewContentAlignment.NotSet)
                {
                    y = rectangle2.Bottom - glyphSize.Height;
                }
                else if ((cellStyle.Alignment & anyMiddle) != DataGridViewContentAlignment.NotSet)
                {
                    y = rectangle2.Top + ((rectangle2.Height - glyphSize.Height) / 2);
                }
                else
                {
                    y = rectangle2.Top;
                }
                if ((base.DataGridView.ApplyVisualStylesToInnerCells && (this.FlatStyle != System.Windows.Forms.FlatStyle.Flat)) && (this.FlatStyle != System.Windows.Forms.FlatStyle.Popup))
                {
                    if (paint && DataGridViewCell.PaintContentForeground(paintParts))
                    {
                        DataGridViewCheckBoxCellRenderer.DrawCheckBox(g, new Rectangle(x, y, glyphSize.Width, glyphSize.Height), (int) uncheckedNormal);
                    }
                    checkBounds = new Rectangle(x, y, glyphSize.Width, glyphSize.Height);
                }
                else if ((this.FlatStyle == System.Windows.Forms.FlatStyle.System) || (this.FlatStyle == System.Windows.Forms.FlatStyle.Standard))
                {
                    if (paint && DataGridViewCell.PaintContentForeground(paintParts))
                    {
                        if (isMixed)
                        {
                            ControlPaint.DrawMixedCheckBox(g, x, y, glyphSize.Width, glyphSize.Height, normal);
                        }
                        else
                        {
                            ControlPaint.DrawCheckBox(g, x, y, glyphSize.Width, glyphSize.Height, normal);
                        }
                    }
                    checkBounds = new Rectangle(x, y, glyphSize.Width, glyphSize.Height);
                }
                else if (this.FlatStyle == System.Windows.Forms.FlatStyle.Flat)
                {
                    Rectangle bounds = new Rectangle(x, y, glyphSize.Width, glyphSize.Height);
                    SolidBrush brush2 = null;
                    SolidBrush brush3 = null;
                    Color empty = Color.Empty;
                    if (paint && DataGridViewCell.PaintContentForeground(paintParts))
                    {
                        brush2 = base.DataGridView.GetCachedBrush(flag ? cellStyle.SelectionForeColor : cellStyle.ForeColor);
                        brush3 = base.DataGridView.GetCachedBrush((DataGridViewCell.PaintSelectionBackground(paintParts) && flag) ? cellStyle.SelectionBackColor : cellStyle.BackColor);
                        empty = ControlPaint.LightLight(brush3.Color);
                        if (((base.DataGridView.MouseEnteredCellAddress.Y == rowIndex) && (base.DataGridView.MouseEnteredCellAddress.X == base.ColumnIndex)) && mouseInContentBounds)
                        {
                            float percentage = 0.9f;
                            if (empty.GetBrightness() < 0.5)
                            {
                                percentage = 1.2f;
                            }
                            empty = Color.FromArgb(ButtonBaseAdapter.ColorOptions.Adjust255(percentage, empty.R), ButtonBaseAdapter.ColorOptions.Adjust255(percentage, empty.G), ButtonBaseAdapter.ColorOptions.Adjust255(percentage, empty.B));
                        }
                        empty = g.GetNearestColor(empty);
                        using (Pen pen = new Pen(brush2.Color))
                        {
                            g.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right - 1, bounds.Top);
                            g.DrawLine(pen, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom - 1);
                        }
                    }
                    bounds.Inflate(-1, -1);
                    bounds.Width++;
                    bounds.Height++;
                    if (paint && DataGridViewCell.PaintContentForeground(paintParts))
                    {
                        if (@unchecked == CheckState.Indeterminate)
                        {
                            ButtonBaseAdapter.DrawDitheredFill(g, brush3.Color, empty, bounds);
                        }
                        else
                        {
                            using (SolidBrush brush4 = new SolidBrush(empty))
                            {
                                g.FillRectangle(brush4, bounds);
                            }
                        }
                        if (@unchecked != CheckState.Unchecked)
                        {
                            Rectangle destination = new Rectangle(x - 1, y - 1, glyphSize.Width + 3, glyphSize.Height + 3);
                            destination.Width++;
                            destination.Height++;
                            if (((checkImage == null) || (checkImage.Width != destination.Width)) || (checkImage.Height != destination.Height))
                            {
                                if (checkImage != null)
                                {
                                    checkImage.Dispose();
                                    checkImage = null;
                                }
                                System.Windows.Forms.NativeMethods.RECT rect = System.Windows.Forms.NativeMethods.RECT.FromXYWH(0, 0, destination.Width, destination.Height);
                                Bitmap image = new Bitmap(destination.Width, destination.Height);
                                using (Graphics graphics = Graphics.FromImage(image))
                                {
                                    graphics.Clear(Color.Transparent);
                                    IntPtr hdc = graphics.GetHdc();
                                    try
                                    {
                                        System.Windows.Forms.SafeNativeMethods.DrawFrameControl(new HandleRef(graphics, hdc), ref rect, 2, 1);
                                    }
                                    finally
                                    {
                                        graphics.ReleaseHdcInternal(hdc);
                                    }
                                }
                                image.MakeTransparent();
                                checkImage = image;
                            }
                            destination.Y--;
                            ControlPaint.DrawImageColorized(g, checkImage, destination, (@unchecked == CheckState.Indeterminate) ? ControlPaint.LightLight(brush2.Color) : brush2.Color);
                        }
                    }
                    checkBounds = bounds;
                }
                else
                {
                    Rectangle rectangle7;
                    rectangle7 = new Rectangle(x, y, glyphSize.Width - 1, glyphSize.Height - 1) {
                        Y = rectangle7.Y - 3
                    };
                    if ((this.ButtonState & (System.Windows.Forms.ButtonState.Checked | System.Windows.Forms.ButtonState.Pushed)) != System.Windows.Forms.ButtonState.Normal)
                    {
                        ButtonBaseAdapter.LayoutOptions options = CheckBoxPopupAdapter.PaintPopupLayout(g, true, glyphSize.Width, rectangle7, Padding.Empty, false, cellStyle.Font, string.Empty, base.DataGridView.Enabled, DataGridViewUtilities.ComputeDrawingContentAlignmentForCellStyleAlignment(cellStyle.Alignment), base.DataGridView.RightToLeft);
                        options.everettButtonCompat = false;
                        ButtonBaseAdapter.LayoutData layout = options.Layout();
                        if (paint && DataGridViewCell.PaintContentForeground(paintParts))
                        {
                            ButtonBaseAdapter.ColorData colors = ButtonBaseAdapter.PaintPopupRender(g, cellStyle.ForeColor, cellStyle.BackColor, base.DataGridView.Enabled).Calculate();
                            CheckBoxBaseAdapter.DrawCheckBackground(base.DataGridView.Enabled, @unchecked, g, layout.checkBounds, colors.windowText, colors.buttonFace, true, colors);
                            CheckBoxBaseAdapter.DrawPopupBorder(g, layout.checkBounds, colors);
                            CheckBoxBaseAdapter.DrawCheckOnly(glyphSize.Width, (@unchecked == CheckState.Checked) || (@unchecked == CheckState.Indeterminate), base.DataGridView.Enabled, @unchecked, g, layout, colors, colors.windowText, colors.buttonFace, true);
                        }
                        checkBounds = layout.checkBounds;
                    }
                    else if (((base.DataGridView.MouseEnteredCellAddress.Y == rowIndex) && (base.DataGridView.MouseEnteredCellAddress.X == base.ColumnIndex)) && mouseInContentBounds)
                    {
                        ButtonBaseAdapter.LayoutOptions options2 = CheckBoxPopupAdapter.PaintPopupLayout(g, true, glyphSize.Width, rectangle7, Padding.Empty, false, cellStyle.Font, string.Empty, base.DataGridView.Enabled, DataGridViewUtilities.ComputeDrawingContentAlignmentForCellStyleAlignment(cellStyle.Alignment), base.DataGridView.RightToLeft);
                        options2.everettButtonCompat = false;
                        ButtonBaseAdapter.LayoutData data3 = options2.Layout();
                        if (paint && DataGridViewCell.PaintContentForeground(paintParts))
                        {
                            ButtonBaseAdapter.ColorData data4 = ButtonBaseAdapter.PaintPopupRender(g, cellStyle.ForeColor, cellStyle.BackColor, base.DataGridView.Enabled).Calculate();
                            CheckBoxBaseAdapter.DrawCheckBackground(base.DataGridView.Enabled, @unchecked, g, data3.checkBounds, data4.windowText, data4.options.highContrast ? data4.buttonFace : data4.highlight, true, data4);
                            CheckBoxBaseAdapter.DrawPopupBorder(g, data3.checkBounds, data4);
                            CheckBoxBaseAdapter.DrawCheckOnly(glyphSize.Width, (@unchecked == CheckState.Checked) || (@unchecked == CheckState.Indeterminate), base.DataGridView.Enabled, @unchecked, g, data3, data4, data4.windowText, data4.highlight, true);
                        }
                        checkBounds = data3.checkBounds;
                    }
                    else
                    {
                        ButtonBaseAdapter.LayoutOptions options3 = CheckBoxPopupAdapter.PaintPopupLayout(g, false, glyphSize.Width, rectangle7, Padding.Empty, false, cellStyle.Font, string.Empty, base.DataGridView.Enabled, DataGridViewUtilities.ComputeDrawingContentAlignmentForCellStyleAlignment(cellStyle.Alignment), base.DataGridView.RightToLeft);
                        options3.everettButtonCompat = false;
                        ButtonBaseAdapter.LayoutData data5 = options3.Layout();
                        if (paint && DataGridViewCell.PaintContentForeground(paintParts))
                        {
                            ButtonBaseAdapter.ColorData data6 = ButtonBaseAdapter.PaintPopupRender(g, cellStyle.ForeColor, cellStyle.BackColor, base.DataGridView.Enabled).Calculate();
                            CheckBoxBaseAdapter.DrawCheckBackground(base.DataGridView.Enabled, @unchecked, g, data5.checkBounds, data6.windowText, data6.options.highContrast ? data6.buttonFace : data6.highlight, true, data6);
                            ButtonBaseAdapter.DrawFlatBorder(g, data5.checkBounds, data6.buttonShadow);
                            CheckBoxBaseAdapter.DrawCheckOnly(glyphSize.Width, (@unchecked == CheckState.Checked) || (@unchecked == CheckState.Indeterminate), base.DataGridView.Enabled, @unchecked, g, data5, data6, data6.windowText, data6.highlight, true);
                        }
                        checkBounds = data5.checkBounds;
                    }
                }
            }
            else if (computeErrorIconBounds)
            {
                if (!string.IsNullOrEmpty(errorText))
                {
                    checkBounds = base.ComputeErrorIconBounds(cellValueBounds);
                }
                else
                {
                    checkBounds = Rectangle.Empty;
                }
            }
            else
            {
                checkBounds = Rectangle.Empty;
            }
            if ((paint && DataGridViewCell.PaintErrorIcon(paintParts)) && (flag3 && base.DataGridView.ShowCellErrors))
            {
                base.PaintErrorIcon(g, cellStyle, rowIndex, cellBounds, cellValueBounds, errorText);
            }
            return checkBounds;
        }

        public override object ParseFormattedValue(object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter)
        {
            if (formattedValue != null)
            {
                if (formattedValue is bool)
                {
                    if (!((bool) formattedValue))
                    {
                        if (this.FalseValue != null)
                        {
                            return this.FalseValue;
                        }
                        if ((this.ValueType != null) && this.ValueType.IsAssignableFrom(defaultBooleanType))
                        {
                            return false;
                        }
                        if ((this.ValueType != null) && this.ValueType.IsAssignableFrom(defaultCheckStateType))
                        {
                            return CheckState.Unchecked;
                        }
                    }
                    else
                    {
                        if (this.TrueValue != null)
                        {
                            return this.TrueValue;
                        }
                        if ((this.ValueType != null) && this.ValueType.IsAssignableFrom(defaultBooleanType))
                        {
                            return true;
                        }
                        if ((this.ValueType != null) && this.ValueType.IsAssignableFrom(defaultCheckStateType))
                        {
                            return CheckState.Checked;
                        }
                    }
                }
                else if (formattedValue is CheckState)
                {
                    switch (((CheckState) formattedValue))
                    {
                        case CheckState.Unchecked:
                            if (this.FalseValue == null)
                            {
                                if ((this.ValueType != null) && this.ValueType.IsAssignableFrom(defaultBooleanType))
                                {
                                    return false;
                                }
                                if ((this.ValueType == null) || !this.ValueType.IsAssignableFrom(defaultCheckStateType))
                                {
                                    break;
                                }
                                return CheckState.Unchecked;
                            }
                            return this.FalseValue;

                        case CheckState.Checked:
                            if (this.TrueValue == null)
                            {
                                if ((this.ValueType != null) && this.ValueType.IsAssignableFrom(defaultBooleanType))
                                {
                                    return true;
                                }
                                if ((this.ValueType == null) || !this.ValueType.IsAssignableFrom(defaultCheckStateType))
                                {
                                    break;
                                }
                                return CheckState.Checked;
                            }
                            return this.TrueValue;

                        case CheckState.Indeterminate:
                            if (this.IndeterminateValue == null)
                            {
                                if ((this.ValueType != null) && this.ValueType.IsAssignableFrom(defaultCheckStateType))
                                {
                                    return CheckState.Indeterminate;
                                }
                                break;
                            }
                            return this.IndeterminateValue;
                    }
                }
            }
            return base.ParseFormattedValue(formattedValue, cellStyle, formattedValueTypeConverter, valueTypeConverter);
        }

        public virtual void PrepareEditingCellForEdit(bool selectAll)
        {
        }

        private bool SwitchFormattedValue()
        {
            if (this.FormattedValueType == null)
            {
                return false;
            }
            IDataGridViewEditingCell cell = this;
            if (this.FormattedValueType.IsAssignableFrom(typeof(CheckState)))
            {
                if ((this.flags & 0x10) != 0)
                {
                    cell.EditingCellFormattedValue = CheckState.Indeterminate;
                }
                else if ((this.flags & 0x20) != 0)
                {
                    cell.EditingCellFormattedValue = CheckState.Unchecked;
                }
                else
                {
                    cell.EditingCellFormattedValue = CheckState.Checked;
                }
            }
            else if (this.FormattedValueType.IsAssignableFrom(defaultBooleanType))
            {
                cell.EditingCellFormattedValue = !((bool) cell.GetEditingCellFormattedValue(DataGridViewDataErrorContexts.Formatting));
            }
            return true;
        }

        public override string ToString()
        {
            return ("DataGridViewCheckBoxCell { ColumnIndex=" + base.ColumnIndex.ToString(CultureInfo.CurrentCulture) + ", RowIndex=" + base.RowIndex.ToString(CultureInfo.CurrentCulture) + " }");
        }

        private void UpdateButtonState(System.Windows.Forms.ButtonState newButtonState, int rowIndex)
        {
            this.ButtonState = newButtonState;
            base.DataGridView.InvalidateCell(base.ColumnIndex, rowIndex);
        }

        private System.Windows.Forms.ButtonState ButtonState
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropButtonCellState, out flag);
                if (flag)
                {
                    return (System.Windows.Forms.ButtonState) integer;
                }
                return System.Windows.Forms.ButtonState.Normal;
            }
            set
            {
                if (this.ButtonState != value)
                {
                    base.Properties.SetInteger(PropButtonCellState, (int) value);
                }
            }
        }

        public virtual object EditingCellFormattedValue
        {
            get
            {
                return this.GetEditingCellFormattedValue(DataGridViewDataErrorContexts.Formatting);
            }
            set
            {
                if (this.FormattedValueType == null)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridViewCell_FormattedValueTypeNull"));
                }
                if ((value == null) || !this.FormattedValueType.IsAssignableFrom(value.GetType()))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridViewCheckBoxCell_InvalidValueType"));
                }
                if (value is CheckState)
                {
                    if (((CheckState) value) == CheckState.Checked)
                    {
                        this.flags = (byte) (this.flags | 0x10);
                        this.flags = (byte) (this.flags & -33);
                    }
                    else if (((CheckState) value) == CheckState.Indeterminate)
                    {
                        this.flags = (byte) (this.flags | 0x20);
                        this.flags = (byte) (this.flags & -17);
                    }
                    else
                    {
                        this.flags = (byte) (this.flags & -17);
                        this.flags = (byte) (this.flags & -33);
                    }
                }
                else
                {
                    if (!(value is bool))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridViewCheckBoxCell_InvalidValueType"));
                    }
                    if ((bool) value)
                    {
                        this.flags = (byte) (this.flags | 0x10);
                    }
                    else
                    {
                        this.flags = (byte) (this.flags & -17);
                    }
                    this.flags = (byte) (this.flags & -33);
                }
            }
        }

        public virtual bool EditingCellValueChanged
        {
            get
            {
                return ((this.flags & 2) != 0);
            }
            set
            {
                if (value)
                {
                    this.flags = (byte) (this.flags | 2);
                }
                else
                {
                    this.flags = (byte) (this.flags & -3);
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

        [DefaultValue((string) null)]
        public object FalseValue
        {
            get
            {
                return base.Properties.GetObject(PropFalseValue);
            }
            set
            {
                if ((value != null) || base.Properties.ContainsObject(PropFalseValue))
                {
                    base.Properties.SetObject(PropFalseValue, value);
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

        internal object FalseValueInternal
        {
            set
            {
                if ((value != null) || base.Properties.ContainsObject(PropFalseValue))
                {
                    base.Properties.SetObject(PropFalseValue, value);
                }
            }
        }

        [DefaultValue(2)]
        public System.Windows.Forms.FlatStyle FlatStyle
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropFlatStyle, out flag);
                if (flag)
                {
                    return (System.Windows.Forms.FlatStyle) integer;
                }
                return System.Windows.Forms.FlatStyle.Standard;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.FlatStyle));
                }
                if (value != this.FlatStyle)
                {
                    base.Properties.SetInteger(PropFlatStyle, (int) value);
                    base.OnCommonChange();
                }
            }
        }

        internal System.Windows.Forms.FlatStyle FlatStyleInternal
        {
            set
            {
                if (value != this.FlatStyle)
                {
                    base.Properties.SetInteger(PropFlatStyle, (int) value);
                }
            }
        }

        public override System.Type FormattedValueType
        {
            get
            {
                if (this.ThreeState)
                {
                    return defaultCheckStateType;
                }
                return defaultBooleanType;
            }
        }

        [DefaultValue((string) null)]
        public object IndeterminateValue
        {
            get
            {
                return base.Properties.GetObject(PropIndeterminateValue);
            }
            set
            {
                if ((value != null) || base.Properties.ContainsObject(PropIndeterminateValue))
                {
                    base.Properties.SetObject(PropIndeterminateValue, value);
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

        internal object IndeterminateValueInternal
        {
            set
            {
                if ((value != null) || base.Properties.ContainsObject(PropIndeterminateValue))
                {
                    base.Properties.SetObject(PropIndeterminateValue, value);
                }
            }
        }

        [DefaultValue(false)]
        public bool ThreeState
        {
            get
            {
                return ((this.flags & 1) != 0);
            }
            set
            {
                if (this.ThreeState != value)
                {
                    this.ThreeStateInternal = value;
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

        internal bool ThreeStateInternal
        {
            set
            {
                if (this.ThreeState != value)
                {
                    if (value)
                    {
                        this.flags = (byte) (this.flags | 1);
                    }
                    else
                    {
                        this.flags = (byte) (this.flags & -2);
                    }
                }
            }
        }

        [DefaultValue((string) null)]
        public object TrueValue
        {
            get
            {
                return base.Properties.GetObject(PropTrueValue);
            }
            set
            {
                if ((value != null) || base.Properties.ContainsObject(PropTrueValue))
                {
                    base.Properties.SetObject(PropTrueValue, value);
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

        internal object TrueValueInternal
        {
            set
            {
                if ((value != null) || base.Properties.ContainsObject(PropTrueValue))
                {
                    base.Properties.SetObject(PropTrueValue, value);
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
                if (this.ThreeState)
                {
                    return defaultCheckStateType;
                }
                return defaultBooleanType;
            }
            set
            {
                base.ValueType = value;
                this.ThreeState = (value != null) && defaultCheckStateType.IsAssignableFrom(value);
            }
        }

        protected class DataGridViewCheckBoxCellAccessibleObject : DataGridViewCell.DataGridViewCellAccessibleObject
        {
            public DataGridViewCheckBoxCellAccessibleObject(DataGridViewCell owner) : base(owner)
            {
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                DataGridViewCheckBoxCell owner = (DataGridViewCheckBoxCell) base.Owner;
                DataGridView dataGridView = owner.DataGridView;
                if ((dataGridView != null) && (owner.RowIndex == -1))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidOperationOnSharedCell"));
                }
                if ((!owner.ReadOnly && (owner.OwningColumn != null)) && (owner.OwningRow != null))
                {
                    dataGridView.CurrentCell = owner;
                    bool flag = false;
                    if (!dataGridView.IsCurrentCellInEditMode)
                    {
                        flag = true;
                        dataGridView.BeginEdit(false);
                    }
                    if (dataGridView.IsCurrentCellInEditMode)
                    {
                        if (owner.SwitchFormattedValue())
                        {
                            owner.NotifyDataGridViewOfValueChange();
                            dataGridView.InvalidateCell(owner.ColumnIndex, owner.RowIndex);
                            DataGridViewCheckBoxCell cell2 = base.Owner as DataGridViewCheckBoxCell;
                            if (cell2 != null)
                            {
                                cell2.NotifyMASSClient(new Point(owner.ColumnIndex, owner.RowIndex));
                            }
                        }
                        if (flag)
                        {
                            dataGridView.EndEdit();
                        }
                    }
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
                    if (base.Owner.ReadOnly)
                    {
                        return string.Empty;
                    }
                    bool flag = true;
                    object formattedValue = base.Owner.FormattedValue;
                    if (formattedValue is CheckState)
                    {
                        flag = ((CheckState) formattedValue) == CheckState.Unchecked;
                    }
                    else if (formattedValue is bool)
                    {
                        flag = !((bool) formattedValue);
                    }
                    if (flag)
                    {
                        return System.Windows.Forms.SR.GetString("DataGridView_AccCheckBoxCellDefaultActionCheck");
                    }
                    return System.Windows.Forms.SR.GetString("DataGridView_AccCheckBoxCellDefaultActionUncheck");
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    if (((DataGridViewCheckBoxCell) base.Owner).EditedFormattedValue is CheckState)
                    {
                        switch (((CheckState) ((DataGridViewCheckBoxCell) base.Owner).EditedFormattedValue))
                        {
                            case CheckState.Checked:
                                return (AccessibleStates.Checked | base.State);

                            case CheckState.Indeterminate:
                                return (AccessibleStates.Indeterminate | base.State);
                        }
                    }
                    else if ((((DataGridViewCheckBoxCell) base.Owner).EditedFormattedValue is bool) && ((bool) ((DataGridViewCheckBoxCell) base.Owner).EditedFormattedValue))
                    {
                        return (AccessibleStates.Checked | base.State);
                    }
                    return base.State;
                }
            }
        }

        private class DataGridViewCheckBoxCellRenderer
        {
            private static VisualStyleRenderer visualStyleRenderer;

            private DataGridViewCheckBoxCellRenderer()
            {
            }

            public static void DrawCheckBox(Graphics g, Rectangle bounds, int state)
            {
                CheckBoxRenderer.SetParameters(DataGridViewCheckBoxCell.CheckBoxElement.ClassName, DataGridViewCheckBoxCell.CheckBoxElement.Part, state);
                CheckBoxRenderer.DrawBackground(g, bounds, Rectangle.Truncate(g.ClipBounds));
            }

            public static VisualStyleRenderer CheckBoxRenderer
            {
                get
                {
                    if (visualStyleRenderer == null)
                    {
                        visualStyleRenderer = new VisualStyleRenderer(DataGridViewCheckBoxCell.CheckBoxElement);
                    }
                    return visualStyleRenderer;
                }
            }
        }
    }
}

