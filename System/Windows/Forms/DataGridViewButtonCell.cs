namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Windows.Forms.ButtonInternal;
    using System.Windows.Forms.Internal;
    using System.Windows.Forms.VisualStyles;

    public class DataGridViewButtonCell : DataGridViewCell
    {
        private static readonly VisualStyleElement ButtonElement = VisualStyleElement.Button.PushButton.Normal;
        private static System.Type cellType = typeof(DataGridViewButtonCell);
        private const byte DATAGRIDVIEWBUTTONCELL_horizontalTextMargin = 2;
        private const byte DATAGRIDVIEWBUTTONCELL_textPadding = 5;
        private const byte DATAGRIDVIEWBUTTONCELL_themeMargin = 100;
        private const byte DATAGRIDVIEWBUTTONCELL_verticalTextMargin = 1;
        private static System.Type defaultFormattedValueType = typeof(string);
        private static System.Type defaultValueType = typeof(object);
        private static bool mouseInContentBounds = false;
        private static readonly int PropButtonCellFlatStyle = PropertyStore.CreateKey();
        private static readonly int PropButtonCellState = PropertyStore.CreateKey();
        private static readonly int PropButtonCellUseColumnTextForButtonValue = PropertyStore.CreateKey();
        private static Rectangle rectThemeMargins = new Rectangle(-1, -1, 0, 0);

        public override object Clone()
        {
            DataGridViewButtonCell cell;
            System.Type type = base.GetType();
            if (type == cellType)
            {
                cell = new DataGridViewButtonCell();
            }
            else
            {
                cell = (DataGridViewButtonCell) Activator.CreateInstance(type);
            }
            base.CloneInternal(cell);
            cell.FlatStyleInternal = this.FlatStyle;
            cell.UseColumnTextForButtonValueInternal = this.UseColumnTextForButtonValue;
            return cell;
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new DataGridViewButtonCellAccessibleObject(this);
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
            int num3;
            int num4;
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
            if (base.DataGridView.ApplyVisualStylesToInnerCells)
            {
                Rectangle themeMargins = GetThemeMargins(graphics);
                num3 = themeMargins.X + themeMargins.Width;
                num4 = themeMargins.Y + themeMargins.Height;
            }
            else
            {
                num3 = num4 = 5;
            }
            switch (freeDimensionFromConstraint)
            {
                case DataGridViewFreeDimension.Height:
                    if (((cellStyle.WrapMode != DataGridViewTriState.True) || (str.Length <= 1)) || ((((constraintSize.Width - num) - num3) - 4) <= 0))
                    {
                        size = new Size(0, DataGridViewCell.MeasureTextSize(graphics, str, cellStyle.Font, flags).Height);
                        break;
                    }
                    size = new Size(0, DataGridViewCell.MeasureTextHeight(graphics, str, cellStyle.Font, ((constraintSize.Width - num) - num3) - 4, flags));
                    break;

                case DataGridViewFreeDimension.Width:
                    if (((cellStyle.WrapMode != DataGridViewTriState.True) || (str.Length <= 1)) || ((((constraintSize.Height - num2) - num4) - 2) <= 0))
                    {
                        size = new Size(DataGridViewCell.MeasureTextSize(graphics, str, cellStyle.Font, flags).Width, 0);
                        break;
                    }
                    size = new Size(DataGridViewCell.MeasureTextWidth(graphics, str, cellStyle.Font, ((constraintSize.Height - num2) - num4) - 2, flags), 0);
                    break;

                default:
                    if ((cellStyle.WrapMode == DataGridViewTriState.True) && (str.Length > 1))
                    {
                        size = DataGridViewCell.MeasureTextPreferredSize(graphics, str, cellStyle.Font, 5f, flags);
                    }
                    else
                    {
                        size = DataGridViewCell.MeasureTextSize(graphics, str, cellStyle.Font, flags);
                    }
                    break;
            }
            if (freeDimensionFromConstraint != DataGridViewFreeDimension.Height)
            {
                size.Width += (num + num3) + 4;
                if (base.DataGridView.ShowCellErrors)
                {
                    size.Width = Math.Max(size.Width, (num + 8) + 12);
                }
            }
            if (freeDimensionFromConstraint != DataGridViewFreeDimension.Width)
            {
                size.Height += (num2 + num4) + 2;
                if (base.DataGridView.ShowCellErrors)
                {
                    size.Height = Math.Max(size.Height, (num2 + 8) + 11);
                }
            }
            return size;
        }

        private static Rectangle GetThemeMargins(Graphics g)
        {
            if (rectThemeMargins.X == -1)
            {
                Rectangle bounds = new Rectangle(0, 0, 100, 100);
                Rectangle backgroundContentRectangle = DataGridViewButtonCellRenderer.DataGridViewButtonRenderer.GetBackgroundContentRectangle(g, bounds);
                rectThemeMargins.X = backgroundContentRectangle.X;
                rectThemeMargins.Y = backgroundContentRectangle.Y;
                rectThemeMargins.Width = 100 - backgroundContentRectangle.Right;
                rectThemeMargins.Height = 100 - backgroundContentRectangle.Bottom;
            }
            return rectThemeMargins;
        }

        protected override object GetValue(int rowIndex)
        {
            if (((this.UseColumnTextForButtonValue && (base.DataGridView != null)) && ((base.DataGridView.NewRowIndex != rowIndex) && (base.OwningColumn != null))) && (base.OwningColumn is DataGridViewButtonColumn))
            {
                return ((DataGridViewButtonColumn) base.OwningColumn).Text;
            }
            return base.GetValue(rowIndex);
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
                bool mouseInContentBounds = DataGridViewButtonCell.mouseInContentBounds;
                DataGridViewButtonCell.mouseInContentBounds = base.GetContentBounds(e.RowIndex).Contains(e.X, e.Y);
                if (mouseInContentBounds != DataGridViewButtonCell.mouseInContentBounds)
                {
                    if ((base.DataGridView.ApplyVisualStylesToInnerCells || (this.FlatStyle == System.Windows.Forms.FlatStyle.Flat)) || (this.FlatStyle == System.Windows.Forms.FlatStyle.Popup))
                    {
                        base.DataGridView.InvalidateCell(base.ColumnIndex, e.RowIndex);
                    }
                    if (((e.ColumnIndex == base.DataGridView.MouseDownCellAddress.X) && (e.RowIndex == base.DataGridView.MouseDownCellAddress.Y)) && (Control.MouseButtons == MouseButtons.Left))
                    {
                        if ((((this.ButtonState & System.Windows.Forms.ButtonState.Pushed) == System.Windows.Forms.ButtonState.Normal) && DataGridViewButtonCell.mouseInContentBounds) && base.DataGridView.CellMouseDownInContentBounds)
                        {
                            this.UpdateButtonState(this.ButtonState | System.Windows.Forms.ButtonState.Pushed, e.RowIndex);
                        }
                        else if (((this.ButtonState & System.Windows.Forms.ButtonState.Pushed) != System.Windows.Forms.ButtonState.Normal) && !DataGridViewButtonCell.mouseInContentBounds)
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
            Rectangle empty;
            Point currentCellAddress = base.DataGridView.CurrentCellAddress;
            bool flag = (elementState & DataGridViewElementStates.Selected) != DataGridViewElementStates.None;
            bool flag2 = (currentCellAddress.X == base.ColumnIndex) && (currentCellAddress.Y == rowIndex);
            string text = formattedValue as string;
            SolidBrush cachedBrush = base.DataGridView.GetCachedBrush((DataGridViewCell.PaintSelectionBackground(paintParts) && flag) ? cellStyle.SelectionBackColor : cellStyle.BackColor);
            SolidBrush brush2 = base.DataGridView.GetCachedBrush(flag ? cellStyle.SelectionForeColor : cellStyle.ForeColor);
            if (paint && DataGridViewCell.PaintBorder(paintParts))
            {
                this.PaintBorder(g, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
            }
            Rectangle rect = cellBounds;
            Rectangle rectangle3 = this.BorderWidths(advancedBorderStyle);
            rect.Offset(rectangle3.X, rectangle3.Y);
            rect.Width -= rectangle3.Right;
            rect.Height -= rectangle3.Bottom;
            if ((rect.Height <= 0) || (rect.Width <= 0))
            {
                return Rectangle.Empty;
            }
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
            Rectangle cellValueBounds = rect;
            if (((rect.Height <= 0) || (rect.Width <= 0)) || (!paint && !computeContentBounds))
            {
                if (computeErrorIconBounds)
                {
                    if (!string.IsNullOrEmpty(errorText))
                    {
                        empty = base.ComputeErrorIconBounds(cellValueBounds);
                    }
                    else
                    {
                        empty = Rectangle.Empty;
                    }
                }
                else
                {
                    empty = Rectangle.Empty;
                }
                goto Label_06AD;
            }
            if ((this.FlatStyle == System.Windows.Forms.FlatStyle.Standard) || (this.FlatStyle == System.Windows.Forms.FlatStyle.System))
            {
                if (base.DataGridView.ApplyVisualStylesToInnerCells)
                {
                    if (paint && DataGridViewCell.PaintContentBackground(paintParts))
                    {
                        PushButtonState normal = PushButtonState.Normal;
                        if ((this.ButtonState & (System.Windows.Forms.ButtonState.Checked | System.Windows.Forms.ButtonState.Pushed)) != System.Windows.Forms.ButtonState.Normal)
                        {
                            normal = PushButtonState.Pressed;
                        }
                        else if (((base.DataGridView.MouseEnteredCellAddress.Y == rowIndex) && (base.DataGridView.MouseEnteredCellAddress.X == base.ColumnIndex)) && mouseInContentBounds)
                        {
                            normal = PushButtonState.Hot;
                        }
                        if ((DataGridViewCell.PaintFocus(paintParts) && flag2) && (base.DataGridView.ShowFocusCues && base.DataGridView.Focused))
                        {
                            normal |= PushButtonState.Default;
                        }
                        DataGridViewButtonCellRenderer.DrawButton(g, rect, (int) normal);
                    }
                    empty = rect;
                    rect = DataGridViewButtonCellRenderer.DataGridViewButtonRenderer.GetBackgroundContentRectangle(g, rect);
                }
                else
                {
                    if (paint && DataGridViewCell.PaintContentBackground(paintParts))
                    {
                        ControlPaint.DrawBorder(g, rect, SystemColors.Control, (this.ButtonState == System.Windows.Forms.ButtonState.Normal) ? ButtonBorderStyle.Outset : ButtonBorderStyle.Inset);
                    }
                    empty = rect;
                    rect.Inflate(-SystemInformation.Border3DSize.Width, -SystemInformation.Border3DSize.Height);
                }
                goto Label_06AD;
            }
            if (this.FlatStyle != System.Windows.Forms.FlatStyle.Flat)
            {
                rect.Inflate(-1, -1);
                if (paint && DataGridViewCell.PaintContentBackground(paintParts))
                {
                    if ((this.ButtonState & (System.Windows.Forms.ButtonState.Checked | System.Windows.Forms.ButtonState.Pushed)) != System.Windows.Forms.ButtonState.Normal)
                    {
                        ButtonBaseAdapter.ColorData data2 = ButtonBaseAdapter.PaintPopupRender(g, cellStyle.ForeColor, cellStyle.BackColor, base.DataGridView.Enabled).Calculate();
                        ButtonBaseAdapter.DrawDefaultBorder(g, rect, data2.options.highContrast ? data2.windowText : data2.windowFrame, true);
                        ControlPaint.DrawBorder(g, rect, data2.options.highContrast ? data2.windowText : data2.buttonShadow, ButtonBorderStyle.Solid);
                    }
                    else if (((base.DataGridView.MouseEnteredCellAddress.Y == rowIndex) && (base.DataGridView.MouseEnteredCellAddress.X == base.ColumnIndex)) && mouseInContentBounds)
                    {
                        ButtonBaseAdapter.ColorData colors = ButtonBaseAdapter.PaintPopupRender(g, cellStyle.ForeColor, cellStyle.BackColor, base.DataGridView.Enabled).Calculate();
                        ButtonBaseAdapter.DrawDefaultBorder(g, rect, colors.options.highContrast ? colors.windowText : colors.buttonShadow, false);
                        ButtonBaseAdapter.Draw3DLiteBorder(g, rect, colors, true);
                    }
                    else
                    {
                        ButtonBaseAdapter.ColorData data4 = ButtonBaseAdapter.PaintPopupRender(g, cellStyle.ForeColor, cellStyle.BackColor, base.DataGridView.Enabled).Calculate();
                        ButtonBaseAdapter.DrawDefaultBorder(g, rect, data4.options.highContrast ? data4.windowText : data4.buttonShadow, false);
                        ButtonBaseAdapter.DrawFlatBorder(g, rect, data4.options.highContrast ? data4.windowText : data4.buttonShadow);
                    }
                }
                empty = rect;
                goto Label_06AD;
            }
            rect.Inflate(-1, -1);
            if (paint && DataGridViewCell.PaintContentBackground(paintParts))
            {
                ButtonBaseAdapter.DrawDefaultBorder(g, rect, brush2.Color, true);
                if (cachedBrush.Color.A == 0xff)
                {
                    if ((this.ButtonState & (System.Windows.Forms.ButtonState.Checked | System.Windows.Forms.ButtonState.Pushed)) != System.Windows.Forms.ButtonState.Normal)
                    {
                        ButtonBaseAdapter.ColorData data = ButtonBaseAdapter.PaintFlatRender(g, cellStyle.ForeColor, cellStyle.BackColor, base.DataGridView.Enabled).Calculate();
                        IntPtr hdc = g.GetHdc();
                        try
                        {
                            using (WindowsGraphics graphics = WindowsGraphics.FromHdc(hdc))
                            {
                                WindowsBrush brush3;
                                if (data.options.highContrast)
                                {
                                    brush3 = new WindowsSolidBrush(graphics.DeviceContext, data.buttonShadow);
                                }
                                else
                                {
                                    brush3 = new WindowsSolidBrush(graphics.DeviceContext, data.lowHighlight);
                                }
                                try
                                {
                                    ButtonBaseAdapter.PaintButtonBackground(graphics, rect, brush3);
                                }
                                finally
                                {
                                    brush3.Dispose();
                                }
                            }
                            goto Label_04CF;
                        }
                        finally
                        {
                            g.ReleaseHdc();
                        }
                    }
                    if (((base.DataGridView.MouseEnteredCellAddress.Y == rowIndex) && (base.DataGridView.MouseEnteredCellAddress.X == base.ColumnIndex)) && mouseInContentBounds)
                    {
                        IntPtr hDc = g.GetHdc();
                        try
                        {
                            using (WindowsGraphics graphics2 = WindowsGraphics.FromHdc(hDc))
                            {
                                Color controlDark = SystemColors.ControlDark;
                                using (WindowsBrush brush4 = new WindowsSolidBrush(graphics2.DeviceContext, controlDark))
                                {
                                    ButtonBaseAdapter.PaintButtonBackground(graphics2, rect, brush4);
                                }
                            }
                        }
                        finally
                        {
                            g.ReleaseHdc();
                        }
                    }
                }
            }
        Label_04CF:
            empty = rect;
        Label_06AD:
            if (((paint && DataGridViewCell.PaintFocus(paintParts)) && (flag2 && base.DataGridView.ShowFocusCues)) && ((base.DataGridView.Focused && (rect.Width > ((2 * SystemInformation.Border3DSize.Width) + 1))) && (rect.Height > ((2 * SystemInformation.Border3DSize.Height) + 1))))
            {
                if ((this.FlatStyle == System.Windows.Forms.FlatStyle.System) || (this.FlatStyle == System.Windows.Forms.FlatStyle.Standard))
                {
                    ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(rect, -1, -1), Color.Empty, SystemColors.Control);
                }
                else if (this.FlatStyle == System.Windows.Forms.FlatStyle.Flat)
                {
                    if (((this.ButtonState & (System.Windows.Forms.ButtonState.Checked | System.Windows.Forms.ButtonState.Pushed)) != System.Windows.Forms.ButtonState.Normal) || ((base.DataGridView.CurrentCellAddress.Y == rowIndex) && (base.DataGridView.CurrentCellAddress.X == base.ColumnIndex)))
                    {
                        ButtonBaseAdapter.ColorData data5 = ButtonBaseAdapter.PaintFlatRender(g, cellStyle.ForeColor, cellStyle.BackColor, base.DataGridView.Enabled).Calculate();
                        string str2 = (text != null) ? text : string.Empty;
                        ButtonBaseAdapter.LayoutOptions options = ButtonFlatAdapter.PaintFlatLayout(g, true, SystemInformation.HighContrast, 1, rect, Padding.Empty, false, cellStyle.Font, str2, base.DataGridView.Enabled, DataGridViewUtilities.ComputeDrawingContentAlignmentForCellStyleAlignment(cellStyle.Alignment), base.DataGridView.RightToLeft);
                        options.everettButtonCompat = false;
                        ButtonBaseAdapter.LayoutData data6 = options.Layout();
                        ButtonBaseAdapter.DrawFlatFocus(g, data6.focus, data5.options.highContrast ? data5.windowText : data5.constrastButtonShadow);
                    }
                }
                else if (((this.ButtonState & (System.Windows.Forms.ButtonState.Checked | System.Windows.Forms.ButtonState.Pushed)) != System.Windows.Forms.ButtonState.Normal) || ((base.DataGridView.CurrentCellAddress.Y == rowIndex) && (base.DataGridView.CurrentCellAddress.X == base.ColumnIndex)))
                {
                    bool up = this.ButtonState == System.Windows.Forms.ButtonState.Normal;
                    string str3 = (text != null) ? text : string.Empty;
                    ButtonBaseAdapter.LayoutOptions options2 = ButtonPopupAdapter.PaintPopupLayout(g, up, SystemInformation.HighContrast ? 2 : 1, rect, Padding.Empty, false, cellStyle.Font, str3, base.DataGridView.Enabled, DataGridViewUtilities.ComputeDrawingContentAlignmentForCellStyleAlignment(cellStyle.Alignment), base.DataGridView.RightToLeft);
                    options2.everettButtonCompat = false;
                    ButtonBaseAdapter.LayoutData data7 = options2.Layout();
                    ControlPaint.DrawFocusRectangle(g, data7.focus, cellStyle.ForeColor, cellStyle.BackColor);
                }
            }
            if (((text != null) && paint) && DataGridViewCell.PaintContentForeground(paintParts))
            {
                rect.Offset(2, 1);
                rect.Width -= 4;
                rect.Height -= 2;
                if ((((this.ButtonState & (System.Windows.Forms.ButtonState.Checked | System.Windows.Forms.ButtonState.Pushed)) != System.Windows.Forms.ButtonState.Normal) && (this.FlatStyle != System.Windows.Forms.FlatStyle.Flat)) && (this.FlatStyle != System.Windows.Forms.FlatStyle.Popup))
                {
                    rect.Offset(1, 1);
                    rect.Width--;
                    rect.Height--;
                }
                if ((rect.Width > 0) && (rect.Height > 0))
                {
                    Color color;
                    if (base.DataGridView.ApplyVisualStylesToInnerCells && ((this.FlatStyle == System.Windows.Forms.FlatStyle.System) || (this.FlatStyle == System.Windows.Forms.FlatStyle.Standard)))
                    {
                        color = DataGridViewButtonCellRenderer.DataGridViewButtonRenderer.GetColor(ColorProperty.TextColor);
                    }
                    else
                    {
                        color = brush2.Color;
                    }
                    TextFormatFlags flags = DataGridViewUtilities.ComputeTextFormatFlagsForCellStyleAlignment(base.DataGridView.RightToLeftInternal, cellStyle.Alignment, cellStyle.WrapMode);
                    TextRenderer.DrawText(g, text, cellStyle.Font, rect, color, flags);
                }
            }
            if ((base.DataGridView.ShowCellErrors && paint) && DataGridViewCell.PaintErrorIcon(paintParts))
            {
                base.PaintErrorIcon(g, cellStyle, rowIndex, cellBounds, cellValueBounds, errorText);
            }
            return empty;
        }

        public override string ToString()
        {
            return ("DataGridViewButtonCell { ColumnIndex=" + base.ColumnIndex.ToString(CultureInfo.CurrentCulture) + ", RowIndex=" + base.RowIndex.ToString(CultureInfo.CurrentCulture) + " }");
        }

        private void UpdateButtonState(System.Windows.Forms.ButtonState newButtonState, int rowIndex)
        {
            if (this.ButtonState != newButtonState)
            {
                this.ButtonState = newButtonState;
                base.DataGridView.InvalidateCell(base.ColumnIndex, rowIndex);
            }
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

        public override System.Type EditType
        {
            get
            {
                return null;
            }
        }

        [DefaultValue(2)]
        public System.Windows.Forms.FlatStyle FlatStyle
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropButtonCellFlatStyle, out flag);
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
                    base.Properties.SetInteger(PropButtonCellFlatStyle, (int) value);
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
                    base.Properties.SetInteger(PropButtonCellFlatStyle, (int) value);
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

        [DefaultValue(false)]
        public bool UseColumnTextForButtonValue
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropButtonCellUseColumnTextForButtonValue, out flag);
                if (!flag)
                {
                    return false;
                }
                return (integer != 0);
            }
            set
            {
                if (value != this.UseColumnTextForButtonValue)
                {
                    base.Properties.SetInteger(PropButtonCellUseColumnTextForButtonValue, value ? 1 : 0);
                    base.OnCommonChange();
                }
            }
        }

        internal bool UseColumnTextForButtonValueInternal
        {
            set
            {
                if (value != this.UseColumnTextForButtonValue)
                {
                    base.Properties.SetInteger(PropButtonCellUseColumnTextForButtonValue, value ? 1 : 0);
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

        protected class DataGridViewButtonCellAccessibleObject : DataGridViewCell.DataGridViewCellAccessibleObject
        {
            public DataGridViewButtonCellAccessibleObject(DataGridViewCell owner) : base(owner)
            {
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                DataGridViewButtonCell owner = (DataGridViewButtonCell) base.Owner;
                DataGridView dataGridView = owner.DataGridView;
                if ((dataGridView != null) && (owner.RowIndex == -1))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidOperationOnSharedCell"));
                }
                if ((owner.OwningColumn != null) && (owner.OwningRow != null))
                {
                    dataGridView.OnCellClickInternal(new DataGridViewCellEventArgs(owner.ColumnIndex, owner.RowIndex));
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
                    return System.Windows.Forms.SR.GetString("DataGridView_AccButtonCellDefaultAction");
                }
            }
        }

        private class DataGridViewButtonCellRenderer
        {
            private static VisualStyleRenderer visualStyleRenderer;

            private DataGridViewButtonCellRenderer()
            {
            }

            public static void DrawButton(Graphics g, Rectangle bounds, int buttonState)
            {
                DataGridViewButtonRenderer.SetParameters(DataGridViewButtonCell.ButtonElement.ClassName, DataGridViewButtonCell.ButtonElement.Part, buttonState);
                DataGridViewButtonRenderer.DrawBackground(g, bounds, Rectangle.Truncate(g.ClipBounds));
            }

            public static VisualStyleRenderer DataGridViewButtonRenderer
            {
                get
                {
                    if (visualStyleRenderer == null)
                    {
                        visualStyleRenderer = new VisualStyleRenderer(DataGridViewButtonCell.ButtonElement);
                    }
                    return visualStyleRenderer;
                }
            }
        }
    }
}

