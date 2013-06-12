namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;

    public class DataGridViewTextBoxCell : DataGridViewCell
    {
        private static System.Type cellType = typeof(DataGridViewTextBoxCell);
        private const byte DATAGRIDVIEWTEXTBOXCELL_horizontalTextMarginLeft = 0;
        private const byte DATAGRIDVIEWTEXTBOXCELL_horizontalTextMarginRight = 0;
        private const byte DATAGRIDVIEWTEXTBOXCELL_horizontalTextOffsetLeft = 3;
        private const byte DATAGRIDVIEWTEXTBOXCELL_horizontalTextOffsetRight = 4;
        private const byte DATAGRIDVIEWTEXTBOXCELL_ignoreNextMouseClick = 1;
        private const int DATAGRIDVIEWTEXTBOXCELL_maxInputLength = 0x7fff;
        private const byte DATAGRIDVIEWTEXTBOXCELL_verticalTextMarginBottom = 1;
        private const byte DATAGRIDVIEWTEXTBOXCELL_verticalTextMarginTopWithoutWrapping = 2;
        private const byte DATAGRIDVIEWTEXTBOXCELL_verticalTextMarginTopWithWrapping = 1;
        private const byte DATAGRIDVIEWTEXTBOXCELL_verticalTextOffsetBottom = 1;
        private const byte DATAGRIDVIEWTEXTBOXCELL_verticalTextOffsetTop = 2;
        private static System.Type defaultFormattedValueType = typeof(string);
        private static System.Type defaultValueType = typeof(object);
        private byte flagsState;
        private static readonly int PropTextBoxCellEditingTextBox = PropertyStore.CreateKey();
        private static readonly int PropTextBoxCellMaxInputLength = PropertyStore.CreateKey();

        internal override void CacheEditingControl()
        {
            this.EditingTextBox = base.DataGridView.EditingControl as DataGridViewTextBoxEditingControl;
        }

        public override object Clone()
        {
            DataGridViewTextBoxCell cell;
            System.Type type = base.GetType();
            if (type == cellType)
            {
                cell = new DataGridViewTextBoxCell();
            }
            else
            {
                cell = (DataGridViewTextBoxCell) Activator.CreateInstance(type);
            }
            base.CloneInternal(cell);
            cell.MaxInputLength = this.MaxInputLength;
            return cell;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override void DetachEditingControl()
        {
            DataGridView dataGridView = base.DataGridView;
            if ((dataGridView == null) || (dataGridView.EditingControl == null))
            {
                throw new InvalidOperationException();
            }
            TextBox editingControl = dataGridView.EditingControl as TextBox;
            if (editingControl != null)
            {
                editingControl.ClearUndo();
            }
            this.EditingTextBox = null;
            base.DetachEditingControl();
        }

        private Rectangle GetAdjustedEditingControlBounds(Rectangle editingControlBounds, DataGridViewCellStyle cellStyle)
        {
            int height;
            TextBox editingControl = base.DataGridView.EditingControl as TextBox;
            int width = editingControlBounds.Width;
            if (editingControl != null)
            {
                switch (cellStyle.Alignment)
                {
                    case DataGridViewContentAlignment.TopLeft:
                    case DataGridViewContentAlignment.MiddleLeft:
                    case DataGridViewContentAlignment.BottomLeft:
                        if (base.DataGridView.RightToLeftInternal)
                        {
                            editingControlBounds.X++;
                            editingControlBounds.Width = Math.Max(0, (editingControlBounds.Width - 3) - 2);
                        }
                        else
                        {
                            editingControlBounds.X += 3;
                            editingControlBounds.Width = Math.Max(0, (editingControlBounds.Width - 3) - 1);
                        }
                        break;

                    case DataGridViewContentAlignment.TopCenter:
                    case DataGridViewContentAlignment.MiddleCenter:
                    case DataGridViewContentAlignment.BottomCenter:
                        editingControlBounds.X++;
                        editingControlBounds.Width = Math.Max(0, editingControlBounds.Width - 3);
                        break;

                    case DataGridViewContentAlignment.TopRight:
                    case DataGridViewContentAlignment.BottomRight:
                    case DataGridViewContentAlignment.MiddleRight:
                        if (base.DataGridView.RightToLeftInternal)
                        {
                            editingControlBounds.X += 3;
                            editingControlBounds.Width = Math.Max(0, editingControlBounds.Width - 4);
                        }
                        else
                        {
                            editingControlBounds.X++;
                            editingControlBounds.Width = Math.Max(0, (editingControlBounds.Width - 4) - 1);
                        }
                        break;
                }
                switch (cellStyle.Alignment)
                {
                    case DataGridViewContentAlignment.TopLeft:
                    case DataGridViewContentAlignment.TopCenter:
                    case DataGridViewContentAlignment.TopRight:
                        editingControlBounds.Y += 2;
                        editingControlBounds.Height = Math.Max(0, editingControlBounds.Height - 2);
                        break;

                    case DataGridViewContentAlignment.MiddleLeft:
                    case DataGridViewContentAlignment.MiddleCenter:
                    case DataGridViewContentAlignment.MiddleRight:
                        editingControlBounds.Height++;
                        break;

                    case DataGridViewContentAlignment.BottomCenter:
                    case DataGridViewContentAlignment.BottomRight:
                    case DataGridViewContentAlignment.BottomLeft:
                        editingControlBounds.Height = Math.Max(0, editingControlBounds.Height - 1);
                        break;
                }
                if (cellStyle.WrapMode == DataGridViewTriState.False)
                {
                    height = editingControl.PreferredSize.Height;
                }
                else
                {
                    string editingControlFormattedValue = (string) ((IDataGridViewEditingControl) editingControl).GetEditingControlFormattedValue(DataGridViewDataErrorContexts.Formatting);
                    if (string.IsNullOrEmpty(editingControlFormattedValue))
                    {
                        editingControlFormattedValue = " ";
                    }
                    TextFormatFlags flags = DataGridViewUtilities.ComputeTextFormatFlagsForCellStyleAlignment(base.DataGridView.RightToLeftInternal, cellStyle.Alignment, cellStyle.WrapMode);
                    using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
                    {
                        height = DataGridViewCell.MeasureTextHeight(graphics, editingControlFormattedValue, cellStyle.Font, width, flags);
                    }
                }
                if (height < editingControlBounds.Height)
                {
                    DataGridViewContentAlignment alignment = cellStyle.Alignment;
                    if (alignment <= DataGridViewContentAlignment.MiddleCenter)
                    {
                        switch (alignment)
                        {
                            case DataGridViewContentAlignment.TopLeft:
                            case DataGridViewContentAlignment.TopCenter:
                            case (DataGridViewContentAlignment.TopCenter | DataGridViewContentAlignment.TopLeft):
                            case DataGridViewContentAlignment.TopRight:
                                return editingControlBounds;

                            case DataGridViewContentAlignment.MiddleLeft:
                            case DataGridViewContentAlignment.MiddleCenter:
                                goto Label_031C;
                        }
                        return editingControlBounds;
                    }
                    if (alignment <= DataGridViewContentAlignment.BottomLeft)
                    {
                        switch (alignment)
                        {
                            case DataGridViewContentAlignment.MiddleRight:
                                goto Label_031C;

                            case DataGridViewContentAlignment.BottomLeft:
                                goto Label_0337;
                        }
                        return editingControlBounds;
                    }
                    switch (alignment)
                    {
                        case DataGridViewContentAlignment.BottomCenter:
                        case DataGridViewContentAlignment.BottomRight:
                            goto Label_0337;
                    }
                }
            }
            return editingControlBounds;
        Label_031C:
            editingControlBounds.Y += (editingControlBounds.Height - height) / 2;
            return editingControlBounds;
        Label_0337:
            editingControlBounds.Y += editingControlBounds.Height - height;
            return editingControlBounds;
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
            base.ComputeBorderStyleCellStateAndCellBounds(rowIndex, out style, out states, out rectangle);
            return this.PaintPrivate(graphics, rectangle, rectangle, rowIndex, states, null, this.GetErrorText(rowIndex), cellStyle, style, DataGridViewPaintParts.ContentForeground, false, true, false);
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
                        size = new Size(0, DataGridViewCell.MeasureTextHeight(graphics, str, cellStyle.Font, Math.Max(1, constraintSize.Width - num), flags));
                        goto Label_01C3;

                    case DataGridViewFreeDimension.Width:
                        size = new Size(DataGridViewCell.MeasureTextWidth(graphics, str, cellStyle.Font, Math.Max(1, ((constraintSize.Height - num2) - 1) - 1), flags), 0);
                        goto Label_01C3;
                }
                size = DataGridViewCell.MeasureTextPreferredSize(graphics, str, cellStyle.Font, 5f, flags);
            }
            else
            {
                switch (freeDimensionFromConstraint)
                {
                    case DataGridViewFreeDimension.Height:
                        size = new Size(0, DataGridViewCell.MeasureTextSize(graphics, str, cellStyle.Font, flags).Height);
                        goto Label_01C3;

                    case DataGridViewFreeDimension.Width:
                        size = new Size(DataGridViewCell.MeasureTextSize(graphics, str, cellStyle.Font, flags).Width, 0);
                        goto Label_01C3;
                }
                size = DataGridViewCell.MeasureTextSize(graphics, str, cellStyle.Font, flags);
            }
        Label_01C3:
            if (freeDimensionFromConstraint != DataGridViewFreeDimension.Height)
            {
                size.Width += num;
                if (base.DataGridView.ShowCellErrors)
                {
                    size.Width = Math.Max(size.Width, (num + 8) + 12);
                }
            }
            if (freeDimensionFromConstraint != DataGridViewFreeDimension.Width)
            {
                int num3 = (cellStyle.WrapMode == DataGridViewTriState.True) ? 1 : 2;
                size.Height += (num3 + 1) + num2;
                if (base.DataGridView.ShowCellErrors)
                {
                    size.Height = Math.Max(size.Height, (num2 + 8) + 11);
                }
            }
            return size;
        }

        public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            TextBox editingControl = base.DataGridView.EditingControl as TextBox;
            if (editingControl != null)
            {
                editingControl.BorderStyle = BorderStyle.None;
                editingControl.AcceptsReturn = editingControl.Multiline = dataGridViewCellStyle.WrapMode == DataGridViewTriState.True;
                editingControl.MaxLength = this.MaxInputLength;
                string str = initialFormattedValue as string;
                if (str == null)
                {
                    editingControl.Text = string.Empty;
                }
                else
                {
                    editingControl.Text = str;
                }
                this.EditingTextBox = base.DataGridView.EditingControl as DataGridViewTextBoxEditingControl;
            }
        }

        public override bool KeyEntersEditMode(KeyEventArgs e)
        {
            return (((((char.IsLetterOrDigit((char) ((ushort) e.KeyCode)) && ((e.KeyCode < Keys.F1) || (e.KeyCode > Keys.F24))) || ((e.KeyCode >= Keys.NumPad0) && (e.KeyCode <= Keys.Divide))) || (((e.KeyCode >= Keys.Oem1) && (e.KeyCode <= Keys.Oem102)) || ((e.KeyCode == Keys.Space) && !e.Shift))) && (!e.Alt && !e.Control)) || base.KeyEntersEditMode(e));
        }

        protected override void OnEnter(int rowIndex, bool throughMouseClick)
        {
            if ((base.DataGridView != null) && throughMouseClick)
            {
                this.flagsState = (byte) (this.flagsState | 1);
            }
        }

        protected override void OnLeave(int rowIndex, bool throughMouseClick)
        {
            if (base.DataGridView != null)
            {
                this.flagsState = (byte) (this.flagsState & -2);
            }
        }

        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            if (base.DataGridView != null)
            {
                Point currentCellAddress = base.DataGridView.CurrentCellAddress;
                if (((currentCellAddress.X == e.ColumnIndex) && (currentCellAddress.Y == e.RowIndex)) && (e.Button == MouseButtons.Left))
                {
                    if ((this.flagsState & 1) != 0)
                    {
                        this.flagsState = (byte) (this.flagsState & -2);
                    }
                    else if (base.DataGridView.EditMode != DataGridViewEditMode.EditProgrammatically)
                    {
                        base.DataGridView.BeginEdit(true);
                    }
                }
            }
        }

        private bool OwnsEditingTextBox(int rowIndex)
        {
            return (((rowIndex != -1) && (this.EditingTextBox != null)) && (rowIndex == this.EditingTextBox.EditingControlRowIndex));
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            this.PaintPrivate(graphics, clipBounds, cellBounds, rowIndex, cellState, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts, false, false, true);
        }

        private Rectangle PaintPrivate(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts, bool computeContentBounds, bool computeErrorIconBounds, bool paint)
        {
            SolidBrush cachedBrush;
            Rectangle empty = Rectangle.Empty;
            if (paint && DataGridViewCell.PaintBorder(paintParts))
            {
                this.PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
            }
            Rectangle rectangle2 = this.BorderWidths(advancedBorderStyle);
            Rectangle rect = cellBounds;
            rect.Offset(rectangle2.X, rectangle2.Y);
            rect.Width -= rectangle2.Right;
            rect.Height -= rectangle2.Bottom;
            Point currentCellAddress = base.DataGridView.CurrentCellAddress;
            bool flag = (currentCellAddress.X == base.ColumnIndex) && (currentCellAddress.Y == rowIndex);
            bool flag2 = flag && (base.DataGridView.EditingControl != null);
            bool flag3 = (cellState & DataGridViewElementStates.Selected) != DataGridViewElementStates.None;
            if ((DataGridViewCell.PaintSelectionBackground(paintParts) && flag3) && !flag2)
            {
                cachedBrush = base.DataGridView.GetCachedBrush(cellStyle.SelectionBackColor);
            }
            else
            {
                cachedBrush = base.DataGridView.GetCachedBrush(cellStyle.BackColor);
            }
            if (((paint && DataGridViewCell.PaintBackground(paintParts)) && ((cachedBrush.Color.A == 0xff) && (rect.Width > 0))) && (rect.Height > 0))
            {
                graphics.FillRectangle(cachedBrush, rect);
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
            if (((paint && flag) && (!flag2 && DataGridViewCell.PaintFocus(paintParts))) && ((base.DataGridView.ShowFocusCues && base.DataGridView.Focused) && ((rect.Width > 0) && (rect.Height > 0))))
            {
                ControlPaint.DrawFocusRectangle(graphics, rect, Color.Empty, cachedBrush.Color);
            }
            Rectangle cellValueBounds = rect;
            string text = formattedValue as string;
            if ((text != null) && ((paint && !flag2) || computeContentBounds))
            {
                int y = (cellStyle.WrapMode == DataGridViewTriState.True) ? 1 : 2;
                rect.Offset(0, y);
                rect.Width = rect.Width;
                rect.Height -= y + 1;
                if ((rect.Width > 0) && (rect.Height > 0))
                {
                    TextFormatFlags flags = DataGridViewUtilities.ComputeTextFormatFlagsForCellStyleAlignment(base.DataGridView.RightToLeftInternal, cellStyle.Alignment, cellStyle.WrapMode);
                    if (paint)
                    {
                        if (DataGridViewCell.PaintContentForeground(paintParts))
                        {
                            if ((flags & TextFormatFlags.SingleLine) != TextFormatFlags.Default)
                            {
                                flags |= TextFormatFlags.EndEllipsis;
                            }
                            TextRenderer.DrawText(graphics, text, cellStyle.Font, rect, flag3 ? cellStyle.SelectionForeColor : cellStyle.ForeColor, flags);
                        }
                    }
                    else
                    {
                        empty = DataGridViewUtilities.GetTextBounds(rect, text, flags, cellStyle);
                    }
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

        public override void PositionEditingControl(bool setLocation, bool setSize, Rectangle cellBounds, Rectangle cellClip, DataGridViewCellStyle cellStyle, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedColumn, bool isFirstDisplayedRow)
        {
            Rectangle editingControlBounds = this.PositionEditingPanel(cellBounds, cellClip, cellStyle, singleVerticalBorderAdded, singleHorizontalBorderAdded, isFirstDisplayedColumn, isFirstDisplayedRow);
            editingControlBounds = this.GetAdjustedEditingControlBounds(editingControlBounds, cellStyle);
            base.DataGridView.EditingControl.Location = new Point(editingControlBounds.X, editingControlBounds.Y);
            base.DataGridView.EditingControl.Size = new Size(editingControlBounds.Width, editingControlBounds.Height);
        }

        public override string ToString()
        {
            return ("DataGridViewTextBoxCell { ColumnIndex=" + base.ColumnIndex.ToString(CultureInfo.CurrentCulture) + ", RowIndex=" + base.RowIndex.ToString(CultureInfo.CurrentCulture) + " }");
        }

        private DataGridViewTextBoxEditingControl EditingTextBox
        {
            get
            {
                return (DataGridViewTextBoxEditingControl) base.Properties.GetObject(PropTextBoxCellEditingTextBox);
            }
            set
            {
                if ((value != null) || base.Properties.ContainsObject(PropTextBoxCellEditingTextBox))
                {
                    base.Properties.SetObject(PropTextBoxCellEditingTextBox, value);
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

        [DefaultValue(0x7fff)]
        public virtual int MaxInputLength
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropTextBoxCellMaxInputLength, out flag);
                if (flag)
                {
                    return integer;
                }
                return 0x7fff;
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "MaxInputLength", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("MaxInputLength", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                base.Properties.SetInteger(PropTextBoxCellMaxInputLength, value);
                if (this.OwnsEditingTextBox(base.RowIndex))
                {
                    this.EditingTextBox.MaxLength = value;
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
    }
}

