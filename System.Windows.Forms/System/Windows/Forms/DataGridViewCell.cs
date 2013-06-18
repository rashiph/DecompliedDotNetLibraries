namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [TypeConverter(typeof(DataGridViewCellConverter))]
    public abstract class DataGridViewCell : DataGridViewElement, ICloneable, IDisposable
    {
        private const int DATAGRIDVIEWCELL_constrastThreshold = 0x3e8;
        private const byte DATAGRIDVIEWCELL_flagAreaNotSet = 0;
        private const byte DATAGRIDVIEWCELL_flagDataArea = 1;
        private const byte DATAGRIDVIEWCELL_flagErrorArea = 2;
        private const int DATAGRIDVIEWCELL_highConstrastThreshold = 0x7d0;
        internal const byte DATAGRIDVIEWCELL_iconMarginHeight = 4;
        internal const byte DATAGRIDVIEWCELL_iconMarginWidth = 4;
        internal const byte DATAGRIDVIEWCELL_iconsHeight = 11;
        internal const byte DATAGRIDVIEWCELL_iconsWidth = 12;
        private const int DATAGRIDVIEWCELL_maxToolTipCutOff = 0x100;
        private const int DATAGRIDVIEWCELL_maxToolTipLength = 0x120;
        private const string DATAGRIDVIEWCELL_toolTipEllipsis = "...";
        private const int DATAGRIDVIEWCELL_toolTipEllipsisLength = 3;
        private static Bitmap errorBmp = null;
        private byte flags;
        private DataGridViewColumn owningColumn;
        private DataGridViewRow owningRow;
        private static readonly int PropCellAccessibilityObject = PropertyStore.CreateKey();
        private static readonly int PropCellContextMenuStrip = PropertyStore.CreateKey();
        private static readonly int PropCellErrorText = PropertyStore.CreateKey();
        private static readonly int PropCellStyle = PropertyStore.CreateKey();
        private static readonly int PropCellTag = PropertyStore.CreateKey();
        private static readonly int PropCellToolTipText = PropertyStore.CreateKey();
        internal static readonly int PropCellValue = PropertyStore.CreateKey();
        private static readonly int PropCellValueType = PropertyStore.CreateKey();
        private PropertyStore propertyStore = new PropertyStore();
        private static System.Type stringType = typeof(string);
        private const TextFormatFlags textFormatSupportedFlags = (TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.WordBreak);

        protected DataGridViewCell()
        {
            base.StateInternal = DataGridViewElementStates.None;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual DataGridViewAdvancedBorderStyle AdjustCellBorderStyle(DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStyleInput, DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedColumn, bool isFirstDisplayedRow)
        {
            switch (dataGridViewAdvancedBorderStyleInput.All)
            {
                case DataGridViewAdvancedCellBorderStyle.NotSet:
                    if ((base.DataGridView != null) && (base.DataGridView.AdvancedCellBorderStyle == dataGridViewAdvancedBorderStyleInput))
                    {
                        switch (base.DataGridView.CellBorderStyle)
                        {
                            case DataGridViewCellBorderStyle.SingleVertical:
                                if (base.DataGridView.RightToLeftInternal)
                                {
                                    dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.Single;
                                    dataGridViewAdvancedBorderStylePlaceholder.RightInternal = (isFirstDisplayedColumn && singleVerticalBorderAdded) ? DataGridViewAdvancedCellBorderStyle.Single : DataGridViewAdvancedCellBorderStyle.None;
                                }
                                else
                                {
                                    dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = (isFirstDisplayedColumn && singleVerticalBorderAdded) ? DataGridViewAdvancedCellBorderStyle.Single : DataGridViewAdvancedCellBorderStyle.None;
                                    dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.Single;
                                }
                                dataGridViewAdvancedBorderStylePlaceholder.TopInternal = DataGridViewAdvancedCellBorderStyle.None;
                                dataGridViewAdvancedBorderStylePlaceholder.BottomInternal = DataGridViewAdvancedCellBorderStyle.None;
                                return dataGridViewAdvancedBorderStylePlaceholder;

                            case DataGridViewCellBorderStyle.SingleHorizontal:
                                dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.None;
                                dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.None;
                                dataGridViewAdvancedBorderStylePlaceholder.TopInternal = (isFirstDisplayedRow && singleHorizontalBorderAdded) ? DataGridViewAdvancedCellBorderStyle.Single : DataGridViewAdvancedCellBorderStyle.None;
                                dataGridViewAdvancedBorderStylePlaceholder.BottomInternal = DataGridViewAdvancedCellBorderStyle.Single;
                                return dataGridViewAdvancedBorderStylePlaceholder;
                        }
                    }
                    return dataGridViewAdvancedBorderStyleInput;

                case DataGridViewAdvancedCellBorderStyle.None:
                    return dataGridViewAdvancedBorderStyleInput;

                case DataGridViewAdvancedCellBorderStyle.Single:
                    if ((base.DataGridView == null) || !base.DataGridView.RightToLeftInternal)
                    {
                        dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = (isFirstDisplayedColumn && singleVerticalBorderAdded) ? DataGridViewAdvancedCellBorderStyle.Single : DataGridViewAdvancedCellBorderStyle.None;
                        dataGridViewAdvancedBorderStylePlaceholder.RightInternal = DataGridViewAdvancedCellBorderStyle.Single;
                        break;
                    }
                    dataGridViewAdvancedBorderStylePlaceholder.LeftInternal = DataGridViewAdvancedCellBorderStyle.Single;
                    dataGridViewAdvancedBorderStylePlaceholder.RightInternal = (isFirstDisplayedColumn && singleVerticalBorderAdded) ? DataGridViewAdvancedCellBorderStyle.Single : DataGridViewAdvancedCellBorderStyle.None;
                    break;

                case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
                    return dataGridViewAdvancedBorderStyleInput;

                default:
                    return dataGridViewAdvancedBorderStyleInput;
            }
            dataGridViewAdvancedBorderStylePlaceholder.TopInternal = (isFirstDisplayedRow && singleHorizontalBorderAdded) ? DataGridViewAdvancedCellBorderStyle.Single : DataGridViewAdvancedCellBorderStyle.None;
            dataGridViewAdvancedBorderStylePlaceholder.BottomInternal = DataGridViewAdvancedCellBorderStyle.Single;
            return dataGridViewAdvancedBorderStylePlaceholder;
        }

        protected virtual Rectangle BorderWidths(DataGridViewAdvancedBorderStyle advancedBorderStyle)
        {
            Rectangle rectangle = new Rectangle {
                X = (advancedBorderStyle.Left == DataGridViewAdvancedCellBorderStyle.None) ? 0 : 1
            };
            if ((advancedBorderStyle.Left == DataGridViewAdvancedCellBorderStyle.OutsetDouble) || (advancedBorderStyle.Left == DataGridViewAdvancedCellBorderStyle.InsetDouble))
            {
                rectangle.X++;
            }
            rectangle.Y = (advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.None) ? 0 : 1;
            if ((advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.OutsetDouble) || (advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.InsetDouble))
            {
                rectangle.Y++;
            }
            rectangle.Width = (advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.None) ? 0 : 1;
            if ((advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.OutsetDouble) || (advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.InsetDouble))
            {
                rectangle.Width++;
            }
            rectangle.Height = (advancedBorderStyle.Bottom == DataGridViewAdvancedCellBorderStyle.None) ? 0 : 1;
            if ((advancedBorderStyle.Bottom == DataGridViewAdvancedCellBorderStyle.OutsetDouble) || (advancedBorderStyle.Bottom == DataGridViewAdvancedCellBorderStyle.InsetDouble))
            {
                rectangle.Height++;
            }
            if (this.owningColumn != null)
            {
                if ((base.DataGridView != null) && base.DataGridView.RightToLeftInternal)
                {
                    rectangle.X += this.owningColumn.DividerWidth;
                }
                else
                {
                    rectangle.Width += this.owningColumn.DividerWidth;
                }
            }
            if (this.owningRow != null)
            {
                rectangle.Height += this.owningRow.DividerHeight;
            }
            return rectangle;
        }

        internal virtual void CacheEditingControl()
        {
        }

        internal DataGridViewElementStates CellStateFromColumnRowStates(DataGridViewElementStates rowState)
        {
            DataGridViewElementStates states = DataGridViewElementStates.Selected | DataGridViewElementStates.Resizable | DataGridViewElementStates.ReadOnly;
            DataGridViewElementStates states2 = DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen | DataGridViewElementStates.Displayed;
            DataGridViewElementStates states3 = this.owningColumn.State & states;
            states3 |= rowState & states;
            return (states3 | ((this.owningColumn.State & states2) & (rowState & states2)));
        }

        protected virtual bool ClickUnsharesRow(DataGridViewCellEventArgs e)
        {
            return false;
        }

        internal bool ClickUnsharesRowInternal(DataGridViewCellEventArgs e)
        {
            return this.ClickUnsharesRow(e);
        }

        public virtual object Clone()
        {
            DataGridViewCell dataGridViewCell = (DataGridViewCell) Activator.CreateInstance(base.GetType());
            this.CloneInternal(dataGridViewCell);
            return dataGridViewCell;
        }

        internal void CloneInternal(DataGridViewCell dataGridViewCell)
        {
            if (this.HasValueType)
            {
                dataGridViewCell.ValueType = this.ValueType;
            }
            if (this.HasStyle)
            {
                dataGridViewCell.Style = new DataGridViewCellStyle(this.Style);
            }
            if (this.HasErrorText)
            {
                dataGridViewCell.ErrorText = this.ErrorTextInternal;
            }
            if (this.HasToolTipText)
            {
                dataGridViewCell.ToolTipText = this.ToolTipTextInternal;
            }
            if (this.ContextMenuStripInternal != null)
            {
                dataGridViewCell.ContextMenuStrip = this.ContextMenuStripInternal.Clone();
            }
            dataGridViewCell.StateInternal = this.State & ~DataGridViewElementStates.Selected;
            dataGridViewCell.Tag = this.Tag;
        }

        internal static int ColorDistance(Color color1, Color color2)
        {
            int num = color1.R - color2.R;
            int num2 = color1.G - color2.G;
            int num3 = color1.B - color2.B;
            return (((num * num) + (num2 * num2)) + (num3 * num3));
        }

        internal void ComputeBorderStyleCellStateAndCellBounds(int rowIndex, out DataGridViewAdvancedBorderStyle dgvabsEffective, out DataGridViewElementStates cellState, out Rectangle cellBounds)
        {
            bool singleVerticalBorderAdded = !base.DataGridView.RowHeadersVisible && (base.DataGridView.AdvancedCellBorderStyle.All == DataGridViewAdvancedCellBorderStyle.Single);
            bool singleHorizontalBorderAdded = !base.DataGridView.ColumnHeadersVisible && (base.DataGridView.AdvancedCellBorderStyle.All == DataGridViewAdvancedCellBorderStyle.Single);
            DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder = new DataGridViewAdvancedBorderStyle();
            if ((rowIndex > -1) && (this.OwningColumn != null))
            {
                dgvabsEffective = this.AdjustCellBorderStyle(base.DataGridView.AdvancedCellBorderStyle, dataGridViewAdvancedBorderStylePlaceholder, singleVerticalBorderAdded, singleHorizontalBorderAdded, this.ColumnIndex == base.DataGridView.FirstDisplayedColumnIndex, rowIndex == base.DataGridView.FirstDisplayedRowIndex);
                DataGridViewElementStates rowState = base.DataGridView.Rows.GetRowState(rowIndex);
                cellState = this.CellStateFromColumnRowStates(rowState);
                cellState |= this.State;
            }
            else if (this.OwningColumn != null)
            {
                DataGridViewColumn lastColumn = base.DataGridView.Columns.GetLastColumn(DataGridViewElementStates.Visible, DataGridViewElementStates.None);
                bool isLastVisibleColumn = (lastColumn != null) && (lastColumn.Index == this.ColumnIndex);
                dgvabsEffective = base.DataGridView.AdjustColumnHeaderBorderStyle(base.DataGridView.AdvancedColumnHeadersBorderStyle, dataGridViewAdvancedBorderStylePlaceholder, this.ColumnIndex == base.DataGridView.FirstDisplayedColumnIndex, isLastVisibleColumn);
                cellState = this.OwningColumn.State | this.State;
            }
            else if (this.OwningRow != null)
            {
                dgvabsEffective = this.OwningRow.AdjustRowHeaderBorderStyle(base.DataGridView.AdvancedRowHeadersBorderStyle, dataGridViewAdvancedBorderStylePlaceholder, singleVerticalBorderAdded, singleHorizontalBorderAdded, rowIndex == base.DataGridView.FirstDisplayedRowIndex, rowIndex == base.DataGridView.Rows.GetLastRow(DataGridViewElementStates.Visible));
                cellState = this.OwningRow.GetState(rowIndex) | this.State;
            }
            else
            {
                dgvabsEffective = base.DataGridView.AdjustedTopLeftHeaderBorderStyle;
                cellState = this.State;
            }
            cellBounds = new Rectangle(new Point(0, 0), this.GetSize(rowIndex));
        }

        internal Rectangle ComputeErrorIconBounds(Rectangle cellValueBounds)
        {
            if ((cellValueBounds.Width >= 20) && (cellValueBounds.Height >= 0x13))
            {
                return new Rectangle(base.DataGridView.RightToLeftInternal ? (cellValueBounds.Left + 4) : ((cellValueBounds.Right - 4) - 12), cellValueBounds.Y + ((cellValueBounds.Height - 11) / 2), 12, 11);
            }
            return Rectangle.Empty;
        }

        protected virtual bool ContentClickUnsharesRow(DataGridViewCellEventArgs e)
        {
            return false;
        }

        internal bool ContentClickUnsharesRowInternal(DataGridViewCellEventArgs e)
        {
            return this.ContentClickUnsharesRow(e);
        }

        protected virtual bool ContentDoubleClickUnsharesRow(DataGridViewCellEventArgs e)
        {
            return false;
        }

        internal bool ContentDoubleClickUnsharesRowInternal(DataGridViewCellEventArgs e)
        {
            return this.ContentDoubleClickUnsharesRow(e);
        }

        protected virtual AccessibleObject CreateAccessibilityInstance()
        {
            return new DataGridViewCellAccessibleObject(this);
        }

        private void DetachContextMenuStrip(object sender, EventArgs e)
        {
            this.ContextMenuStripInternal = null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual void DetachEditingControl()
        {
            DataGridView dataGridView = base.DataGridView;
            if ((dataGridView == null) || (dataGridView.EditingControl == null))
            {
                throw new InvalidOperationException();
            }
            if (dataGridView.EditingControl.ParentInternal != null)
            {
                if (dataGridView.EditingControl.ContainsFocus)
                {
                    ContainerControl containerControlInternal = dataGridView.GetContainerControlInternal() as ContainerControl;
                    if ((containerControlInternal != null) && ((dataGridView.EditingControl == containerControlInternal.ActiveControl) || dataGridView.EditingControl.Contains(containerControlInternal.ActiveControl)))
                    {
                        dataGridView.FocusInternal();
                    }
                    else
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetFocus(new HandleRef(null, IntPtr.Zero));
                    }
                }
                dataGridView.EditingPanel.Controls.Remove(dataGridView.EditingControl);
            }
            if (dataGridView.EditingPanel.ParentInternal != null)
            {
                ((DataGridView.DataGridViewControlCollection) dataGridView.Controls).RemoveInternal(dataGridView.EditingPanel);
            }
            this.CurrentMouseLocation = 0;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                System.Windows.Forms.ContextMenuStrip contextMenuStripInternal = this.ContextMenuStripInternal;
                if (contextMenuStripInternal != null)
                {
                    contextMenuStripInternal.Disposed -= new EventHandler(this.DetachContextMenuStrip);
                }
            }
        }

        protected virtual bool DoubleClickUnsharesRow(DataGridViewCellEventArgs e)
        {
            return false;
        }

        internal bool DoubleClickUnsharesRowInternal(DataGridViewCellEventArgs e)
        {
            return this.DoubleClickUnsharesRow(e);
        }

        protected virtual bool EnterUnsharesRow(int rowIndex, bool throughMouseClick)
        {
            return false;
        }

        internal bool EnterUnsharesRowInternal(int rowIndex, bool throughMouseClick)
        {
            return this.EnterUnsharesRow(rowIndex, throughMouseClick);
        }

        ~DataGridViewCell()
        {
            this.Dispose(false);
        }

        internal static void FormatPlainText(string s, bool csv, TextWriter output, ref bool escapeApplied)
        {
            if (s != null)
            {
                int length = s.Length;
                for (int i = 0; i < length; i++)
                {
                    char ch = s[i];
                    switch (ch)
                    {
                        case '\t':
                            if (!csv)
                            {
                                output.Write(' ');
                            }
                            else
                            {
                                output.Write('\t');
                            }
                            break;

                        case '"':
                            if (csv)
                            {
                                output.Write("\"\"");
                                escapeApplied = true;
                            }
                            else
                            {
                                output.Write('"');
                            }
                            break;

                        case ',':
                            if (csv)
                            {
                                escapeApplied = true;
                            }
                            output.Write(',');
                            break;

                        default:
                            output.Write(ch);
                            break;
                    }
                }
                if (escapeApplied)
                {
                    output.Write('"');
                }
            }
        }

        internal static void FormatPlainTextAsHtml(string s, TextWriter output)
        {
            if (s != null)
            {
                int length = s.Length;
                char ch = '\0';
                for (int i = 0; i < length; i++)
                {
                    char ch2 = s[i];
                    switch (ch2)
                    {
                        case '\n':
                            output.Write("<br>");
                            goto Label_0113;

                        case '\r':
                            goto Label_0113;

                        case ' ':
                            if (ch != ' ')
                            {
                                break;
                            }
                            output.Write("&nbsp;");
                            goto Label_0113;

                        case '"':
                            output.Write("&quot;");
                            goto Label_0113;

                        case '&':
                            output.Write("&amp;");
                            goto Label_0113;

                        case '<':
                            output.Write("&lt;");
                            goto Label_0113;

                        case '>':
                            output.Write("&gt;");
                            goto Label_0113;

                        default:
                            if ((ch2 >= '\x00a0') && (ch2 < 'Ā'))
                            {
                                output.Write("&#");
                                output.Write(((int) ch2).ToString(NumberFormatInfo.InvariantInfo));
                                output.Write(';');
                            }
                            else
                            {
                                output.Write(ch2);
                            }
                            goto Label_0113;
                    }
                    output.Write(ch2);
                Label_0113:
                    ch = ch2;
                }
            }
        }

        private static Bitmap GetBitmap(string bitmapName)
        {
            Bitmap bitmap = new Bitmap(typeof(DataGridViewCell), bitmapName);
            bitmap.MakeTransparent();
            return bitmap;
        }

        protected virtual object GetClipboardContent(int rowIndex, bool firstCell, bool lastCell, bool inFirstRow, bool inLastRow, string format)
        {
            if (base.DataGridView == null)
            {
                return null;
            }
            if ((rowIndex < 0) || (rowIndex >= base.DataGridView.Rows.Count))
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            DataGridViewCellStyle dataGridViewCellStyle = this.GetInheritedStyle(null, rowIndex, false);
            object obj2 = null;
            if (base.DataGridView.IsSharedCellSelected(this, rowIndex))
            {
                obj2 = this.GetEditedFormattedValue(this.GetValue(rowIndex), rowIndex, ref dataGridViewCellStyle, DataGridViewDataErrorContexts.ClipboardContent | DataGridViewDataErrorContexts.Formatting);
            }
            StringBuilder sb = new StringBuilder(0x40);
            if (string.Equals(format, DataFormats.Html, StringComparison.OrdinalIgnoreCase))
            {
                if (firstCell)
                {
                    if (inFirstRow)
                    {
                        sb.Append("<TABLE>");
                    }
                    sb.Append("<TR>");
                }
                sb.Append("<TD>");
                if (obj2 != null)
                {
                    FormatPlainTextAsHtml(obj2.ToString(), new StringWriter(sb, CultureInfo.CurrentCulture));
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
                if ((firstCell && lastCell) && (inFirstRow && inLastRow))
                {
                    sb.Append(obj2.ToString());
                }
                else
                {
                    bool escapeApplied = false;
                    int length = sb.Length;
                    FormatPlainText(obj2.ToString(), csv, new StringWriter(sb, CultureInfo.CurrentCulture), ref escapeApplied);
                    if (escapeApplied)
                    {
                        sb.Insert(length, '"');
                    }
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

        internal object GetClipboardContentInternal(int rowIndex, bool firstCell, bool lastCell, bool inFirstRow, bool inLastRow, string format)
        {
            return this.GetClipboardContent(rowIndex, firstCell, lastCell, inFirstRow, inLastRow, format);
        }

        public Rectangle GetContentBounds(int rowIndex)
        {
            if (base.DataGridView == null)
            {
                return Rectangle.Empty;
            }
            DataGridViewCellStyle cellStyle = this.GetInheritedStyle(null, rowIndex, false);
            using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
            {
                return this.GetContentBounds(graphics, cellStyle, rowIndex);
            }
        }

        protected virtual Rectangle GetContentBounds(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
        {
            return Rectangle.Empty;
        }

        internal System.Windows.Forms.ContextMenuStrip GetContextMenuStrip(int rowIndex)
        {
            System.Windows.Forms.ContextMenuStrip contextMenuStripInternal = this.ContextMenuStripInternal;
            if ((base.DataGridView == null) || (!base.DataGridView.VirtualMode && (base.DataGridView.DataSource == null)))
            {
                return contextMenuStripInternal;
            }
            return base.DataGridView.OnCellContextMenuStripNeeded(this.ColumnIndex, rowIndex, contextMenuStripInternal);
        }

        internal void GetContrastedPens(Color baseline, ref Pen darkPen, ref Pen lightPen)
        {
            int num = ColorDistance(baseline, SystemColors.ControlDark);
            int num2 = ColorDistance(baseline, SystemColors.ControlLightLight);
            if (SystemInformation.HighContrast)
            {
                if (num < 0x7d0)
                {
                    darkPen = base.DataGridView.GetCachedPen(ControlPaint.DarkDark(baseline));
                }
                else
                {
                    darkPen = base.DataGridView.GetCachedPen(SystemColors.ControlDark);
                }
                if (num2 < 0x7d0)
                {
                    lightPen = base.DataGridView.GetCachedPen(ControlPaint.LightLight(baseline));
                }
                else
                {
                    lightPen = base.DataGridView.GetCachedPen(SystemColors.ControlLightLight);
                }
            }
            else
            {
                if (num < 0x3e8)
                {
                    darkPen = base.DataGridView.GetCachedPen(ControlPaint.Dark(baseline));
                }
                else
                {
                    darkPen = base.DataGridView.GetCachedPen(SystemColors.ControlDark);
                }
                if (num2 < 0x3e8)
                {
                    lightPen = base.DataGridView.GetCachedPen(ControlPaint.Light(baseline));
                }
                else
                {
                    lightPen = base.DataGridView.GetCachedPen(SystemColors.ControlLightLight);
                }
            }
        }

        public object GetEditedFormattedValue(int rowIndex, DataGridViewDataErrorContexts context)
        {
            if (base.DataGridView == null)
            {
                return null;
            }
            DataGridViewCellStyle dataGridViewCellStyle = this.GetInheritedStyle(null, rowIndex, false);
            return this.GetEditedFormattedValue(this.GetValue(rowIndex), rowIndex, ref dataGridViewCellStyle, context);
        }

        internal object GetEditedFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle dataGridViewCellStyle, DataGridViewDataErrorContexts context)
        {
            Point currentCellAddress = base.DataGridView.CurrentCellAddress;
            if ((this.ColumnIndex == currentCellAddress.X) && (rowIndex == currentCellAddress.Y))
            {
                IDataGridViewEditingControl editingControl = (IDataGridViewEditingControl) base.DataGridView.EditingControl;
                if (editingControl != null)
                {
                    return editingControl.GetEditingControlFormattedValue(context);
                }
                IDataGridViewEditingCell cell = this as IDataGridViewEditingCell;
                if ((cell != null) && base.DataGridView.IsCurrentCellInEditMode)
                {
                    return cell.GetEditingCellFormattedValue(context);
                }
            }
            return this.GetFormattedValue(value, rowIndex, ref dataGridViewCellStyle, null, null, context);
        }

        internal Rectangle GetErrorIconBounds(int rowIndex)
        {
            DataGridViewCellStyle cellStyle = this.GetInheritedStyle(null, rowIndex, false);
            using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
            {
                return this.GetErrorIconBounds(graphics, cellStyle, rowIndex);
            }
        }

        protected virtual Rectangle GetErrorIconBounds(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
        {
            return Rectangle.Empty;
        }

        protected internal virtual string GetErrorText(int rowIndex)
        {
            string errorText = string.Empty;
            object obj2 = this.Properties.GetObject(PropCellErrorText);
            if (obj2 != null)
            {
                errorText = (string) obj2;
            }
            else if ((((base.DataGridView != null) && (rowIndex != -1)) && ((rowIndex != base.DataGridView.NewRowIndex) && (this.OwningColumn != null))) && (this.OwningColumn.IsDataBound && (base.DataGridView.DataConnection != null)))
            {
                errorText = base.DataGridView.DataConnection.GetError(this.OwningColumn.BoundColumnIndex, this.ColumnIndex, rowIndex);
            }
            if (((base.DataGridView != null) && (base.DataGridView.VirtualMode || (base.DataGridView.DataSource != null))) && ((this.ColumnIndex >= 0) && (rowIndex >= 0)))
            {
                errorText = base.DataGridView.OnCellErrorTextNeeded(this.ColumnIndex, rowIndex, errorText);
            }
            return errorText;
        }

        internal object GetFormattedValue(int rowIndex, ref DataGridViewCellStyle cellStyle, DataGridViewDataErrorContexts context)
        {
            if (base.DataGridView == null)
            {
                return null;
            }
            return this.GetFormattedValue(this.GetValue(rowIndex), rowIndex, ref cellStyle, null, null, context);
        }

        protected virtual object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
        {
            if (base.DataGridView == null)
            {
                return null;
            }
            DataGridViewCellFormattingEventArgs args = base.DataGridView.OnCellFormatting(this.ColumnIndex, rowIndex, value, this.FormattedValueType, cellStyle);
            cellStyle = args.CellStyle;
            bool formattingApplied = args.FormattingApplied;
            object obj2 = args.Value;
            bool flag2 = true;
            if ((!formattingApplied && (this.FormattedValueType != null)) && ((obj2 == null) || !this.FormattedValueType.IsAssignableFrom(obj2.GetType())))
            {
                try
                {
                    obj2 = Formatter.FormatObject(obj2, this.FormattedValueType, (valueTypeConverter == null) ? this.ValueTypeConverter : valueTypeConverter, (formattedValueTypeConverter == null) ? this.FormattedValueTypeConverter : formattedValueTypeConverter, cellStyle.Format, cellStyle.FormatProvider, cellStyle.NullValue, cellStyle.DataSourceNullValue);
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                    {
                        throw;
                    }
                    DataGridViewDataErrorEventArgs e = new DataGridViewDataErrorEventArgs(exception, this.ColumnIndex, rowIndex, context);
                    base.RaiseDataError(e);
                    if (e.ThrowException)
                    {
                        throw e.Exception;
                    }
                    flag2 = false;
                }
            }
            if (flag2 && (((obj2 == null) || (this.FormattedValueType == null)) || !this.FormattedValueType.IsAssignableFrom(obj2.GetType())))
            {
                if (((obj2 == null) && (cellStyle.NullValue == null)) && ((this.FormattedValueType != null) && !typeof(System.ValueType).IsAssignableFrom(this.FormattedValueType)))
                {
                    return null;
                }
                Exception exception2 = null;
                if (this.FormattedValueType == null)
                {
                    exception2 = new FormatException(System.Windows.Forms.SR.GetString("DataGridViewCell_FormattedValueTypeNull"));
                }
                else
                {
                    exception2 = new FormatException(System.Windows.Forms.SR.GetString("DataGridViewCell_FormattedValueHasWrongType"));
                }
                DataGridViewDataErrorEventArgs args3 = new DataGridViewDataErrorEventArgs(exception2, this.ColumnIndex, rowIndex, context);
                base.RaiseDataError(args3);
                if (args3.ThrowException)
                {
                    throw args3.Exception;
                }
            }
            return obj2;
        }

        internal static DataGridViewFreeDimension GetFreeDimensionFromConstraint(System.Drawing.Size constraintSize)
        {
            if ((constraintSize.Width < 0) || (constraintSize.Height < 0))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "constraintSize", constraintSize.ToString() }));
            }
            if (constraintSize.Width == 0)
            {
                if (constraintSize.Height == 0)
                {
                    return DataGridViewFreeDimension.Both;
                }
                return DataGridViewFreeDimension.Width;
            }
            if (constraintSize.Height != 0)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "constraintSize", constraintSize.ToString() }));
            }
            return DataGridViewFreeDimension.Height;
        }

        internal int GetHeight(int rowIndex)
        {
            if (base.DataGridView == null)
            {
                return -1;
            }
            return this.owningRow.GetHeight(rowIndex);
        }

        public virtual System.Windows.Forms.ContextMenuStrip GetInheritedContextMenuStrip(int rowIndex)
        {
            if (base.DataGridView != null)
            {
                if ((rowIndex < 0) || (rowIndex >= base.DataGridView.Rows.Count))
                {
                    throw new ArgumentOutOfRangeException("rowIndex");
                }
                if (this.ColumnIndex < 0)
                {
                    throw new InvalidOperationException();
                }
            }
            System.Windows.Forms.ContextMenuStrip contextMenuStrip = this.GetContextMenuStrip(rowIndex);
            if (contextMenuStrip != null)
            {
                return contextMenuStrip;
            }
            if (this.owningRow != null)
            {
                contextMenuStrip = this.owningRow.GetContextMenuStrip(rowIndex);
                if (contextMenuStrip != null)
                {
                    return contextMenuStrip;
                }
            }
            if (this.owningColumn != null)
            {
                contextMenuStrip = this.owningColumn.ContextMenuStrip;
                if (contextMenuStrip != null)
                {
                    return contextMenuStrip;
                }
            }
            if (base.DataGridView != null)
            {
                return base.DataGridView.ContextMenuStrip;
            }
            return null;
        }

        public virtual DataGridViewElementStates GetInheritedState(int rowIndex)
        {
            DataGridViewElementStates states = this.State | DataGridViewElementStates.ResizableSet;
            if (base.DataGridView == null)
            {
                if (rowIndex != -1)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "rowIndex", rowIndex.ToString(CultureInfo.CurrentCulture) }));
                }
                if (this.owningRow != null)
                {
                    states |= this.owningRow.GetState(-1) & (DataGridViewElementStates.Visible | DataGridViewElementStates.Selected | DataGridViewElementStates.ReadOnly | DataGridViewElementStates.Frozen);
                    if (this.owningRow.GetResizable(rowIndex) == DataGridViewTriState.True)
                    {
                        states |= DataGridViewElementStates.Resizable;
                    }
                }
                return states;
            }
            if ((rowIndex < 0) || (rowIndex >= base.DataGridView.Rows.Count))
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            if (base.DataGridView.Rows.SharedRow(rowIndex) != this.owningRow)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "rowIndex", rowIndex.ToString(CultureInfo.CurrentCulture) }));
            }
            DataGridViewElementStates rowState = base.DataGridView.Rows.GetRowState(rowIndex);
            states |= rowState & (DataGridViewElementStates.Selected | DataGridViewElementStates.ReadOnly);
            states |= this.owningColumn.State & (DataGridViewElementStates.Selected | DataGridViewElementStates.ReadOnly);
            if ((this.owningRow.GetResizable(rowIndex) == DataGridViewTriState.True) || (this.owningColumn.Resizable == DataGridViewTriState.True))
            {
                states |= DataGridViewElementStates.Resizable;
            }
            if (this.owningColumn.Visible && this.owningRow.GetVisible(rowIndex))
            {
                states |= DataGridViewElementStates.Visible;
                if (this.owningColumn.Displayed && this.owningRow.GetDisplayed(rowIndex))
                {
                    states |= DataGridViewElementStates.Displayed;
                }
            }
            if (this.owningColumn.Frozen && this.owningRow.GetFrozen(rowIndex))
            {
                states |= DataGridViewElementStates.Frozen;
            }
            return states;
        }

        public virtual DataGridViewCellStyle GetInheritedStyle(DataGridViewCellStyle inheritedCellStyle, int rowIndex, bool includeColors)
        {
            DataGridViewCellStyle placeholderCellStyle;
            if (base.DataGridView == null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_CellNeedsDataGridViewForInheritedStyle"));
            }
            if ((rowIndex < 0) || (rowIndex >= base.DataGridView.Rows.Count))
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }
            if (this.ColumnIndex < 0)
            {
                throw new InvalidOperationException();
            }
            if (inheritedCellStyle == null)
            {
                placeholderCellStyle = base.DataGridView.PlaceholderCellStyle;
                if (!includeColors)
                {
                    placeholderCellStyle.BackColor = Color.Empty;
                    placeholderCellStyle.ForeColor = Color.Empty;
                    placeholderCellStyle.SelectionBackColor = Color.Empty;
                    placeholderCellStyle.SelectionForeColor = Color.Empty;
                }
            }
            else
            {
                placeholderCellStyle = inheritedCellStyle;
            }
            DataGridViewCellStyle style = null;
            if (this.HasStyle)
            {
                style = this.Style;
            }
            DataGridViewCellStyle style3 = null;
            if (base.DataGridView.Rows.SharedRow(rowIndex).HasDefaultCellStyle)
            {
                style3 = base.DataGridView.Rows.SharedRow(rowIndex).DefaultCellStyle;
            }
            DataGridViewCellStyle style4 = null;
            if (this.owningColumn.HasDefaultCellStyle)
            {
                style4 = this.owningColumn.DefaultCellStyle;
            }
            DataGridViewCellStyle defaultCellStyle = base.DataGridView.DefaultCellStyle;
            if (includeColors)
            {
                if ((style != null) && !style.BackColor.IsEmpty)
                {
                    placeholderCellStyle.BackColor = style.BackColor;
                }
                else if ((style3 != null) && !style3.BackColor.IsEmpty)
                {
                    placeholderCellStyle.BackColor = style3.BackColor;
                }
                else if (!base.DataGridView.RowsDefaultCellStyle.BackColor.IsEmpty && (((rowIndex % 2) == 0) || base.DataGridView.AlternatingRowsDefaultCellStyle.BackColor.IsEmpty))
                {
                    placeholderCellStyle.BackColor = base.DataGridView.RowsDefaultCellStyle.BackColor;
                }
                else if (((rowIndex % 2) == 1) && !base.DataGridView.AlternatingRowsDefaultCellStyle.BackColor.IsEmpty)
                {
                    placeholderCellStyle.BackColor = base.DataGridView.AlternatingRowsDefaultCellStyle.BackColor;
                }
                else if ((style4 != null) && !style4.BackColor.IsEmpty)
                {
                    placeholderCellStyle.BackColor = style4.BackColor;
                }
                else
                {
                    placeholderCellStyle.BackColor = defaultCellStyle.BackColor;
                }
                if ((style != null) && !style.ForeColor.IsEmpty)
                {
                    placeholderCellStyle.ForeColor = style.ForeColor;
                }
                else if ((style3 != null) && !style3.ForeColor.IsEmpty)
                {
                    placeholderCellStyle.ForeColor = style3.ForeColor;
                }
                else if (!base.DataGridView.RowsDefaultCellStyle.ForeColor.IsEmpty && (((rowIndex % 2) == 0) || base.DataGridView.AlternatingRowsDefaultCellStyle.ForeColor.IsEmpty))
                {
                    placeholderCellStyle.ForeColor = base.DataGridView.RowsDefaultCellStyle.ForeColor;
                }
                else if (((rowIndex % 2) == 1) && !base.DataGridView.AlternatingRowsDefaultCellStyle.ForeColor.IsEmpty)
                {
                    placeholderCellStyle.ForeColor = base.DataGridView.AlternatingRowsDefaultCellStyle.ForeColor;
                }
                else if ((style4 != null) && !style4.ForeColor.IsEmpty)
                {
                    placeholderCellStyle.ForeColor = style4.ForeColor;
                }
                else
                {
                    placeholderCellStyle.ForeColor = defaultCellStyle.ForeColor;
                }
                if ((style != null) && !style.SelectionBackColor.IsEmpty)
                {
                    placeholderCellStyle.SelectionBackColor = style.SelectionBackColor;
                }
                else if ((style3 != null) && !style3.SelectionBackColor.IsEmpty)
                {
                    placeholderCellStyle.SelectionBackColor = style3.SelectionBackColor;
                }
                else if (!base.DataGridView.RowsDefaultCellStyle.SelectionBackColor.IsEmpty && (((rowIndex % 2) == 0) || base.DataGridView.AlternatingRowsDefaultCellStyle.SelectionBackColor.IsEmpty))
                {
                    placeholderCellStyle.SelectionBackColor = base.DataGridView.RowsDefaultCellStyle.SelectionBackColor;
                }
                else if (((rowIndex % 2) == 1) && !base.DataGridView.AlternatingRowsDefaultCellStyle.SelectionBackColor.IsEmpty)
                {
                    placeholderCellStyle.SelectionBackColor = base.DataGridView.AlternatingRowsDefaultCellStyle.SelectionBackColor;
                }
                else if ((style4 != null) && !style4.SelectionBackColor.IsEmpty)
                {
                    placeholderCellStyle.SelectionBackColor = style4.SelectionBackColor;
                }
                else
                {
                    placeholderCellStyle.SelectionBackColor = defaultCellStyle.SelectionBackColor;
                }
                if ((style != null) && !style.SelectionForeColor.IsEmpty)
                {
                    placeholderCellStyle.SelectionForeColor = style.SelectionForeColor;
                }
                else if ((style3 != null) && !style3.SelectionForeColor.IsEmpty)
                {
                    placeholderCellStyle.SelectionForeColor = style3.SelectionForeColor;
                }
                else if (!base.DataGridView.RowsDefaultCellStyle.SelectionForeColor.IsEmpty && (((rowIndex % 2) == 0) || base.DataGridView.AlternatingRowsDefaultCellStyle.SelectionForeColor.IsEmpty))
                {
                    placeholderCellStyle.SelectionForeColor = base.DataGridView.RowsDefaultCellStyle.SelectionForeColor;
                }
                else if (((rowIndex % 2) == 1) && !base.DataGridView.AlternatingRowsDefaultCellStyle.SelectionForeColor.IsEmpty)
                {
                    placeholderCellStyle.SelectionForeColor = base.DataGridView.AlternatingRowsDefaultCellStyle.SelectionForeColor;
                }
                else if ((style4 != null) && !style4.SelectionForeColor.IsEmpty)
                {
                    placeholderCellStyle.SelectionForeColor = style4.SelectionForeColor;
                }
                else
                {
                    placeholderCellStyle.SelectionForeColor = defaultCellStyle.SelectionForeColor;
                }
            }
            if ((style != null) && (style.Font != null))
            {
                placeholderCellStyle.Font = style.Font;
            }
            else if ((style3 != null) && (style3.Font != null))
            {
                placeholderCellStyle.Font = style3.Font;
            }
            else if ((base.DataGridView.RowsDefaultCellStyle.Font != null) && (((rowIndex % 2) == 0) || (base.DataGridView.AlternatingRowsDefaultCellStyle.Font == null)))
            {
                placeholderCellStyle.Font = base.DataGridView.RowsDefaultCellStyle.Font;
            }
            else if (((rowIndex % 2) == 1) && (base.DataGridView.AlternatingRowsDefaultCellStyle.Font != null))
            {
                placeholderCellStyle.Font = base.DataGridView.AlternatingRowsDefaultCellStyle.Font;
            }
            else if ((style4 != null) && (style4.Font != null))
            {
                placeholderCellStyle.Font = style4.Font;
            }
            else
            {
                placeholderCellStyle.Font = defaultCellStyle.Font;
            }
            if ((style != null) && !style.IsNullValueDefault)
            {
                placeholderCellStyle.NullValue = style.NullValue;
            }
            else if ((style3 != null) && !style3.IsNullValueDefault)
            {
                placeholderCellStyle.NullValue = style3.NullValue;
            }
            else if (!base.DataGridView.RowsDefaultCellStyle.IsNullValueDefault && (((rowIndex % 2) == 0) || base.DataGridView.AlternatingRowsDefaultCellStyle.IsNullValueDefault))
            {
                placeholderCellStyle.NullValue = base.DataGridView.RowsDefaultCellStyle.NullValue;
            }
            else if (((rowIndex % 2) == 1) && !base.DataGridView.AlternatingRowsDefaultCellStyle.IsNullValueDefault)
            {
                placeholderCellStyle.NullValue = base.DataGridView.AlternatingRowsDefaultCellStyle.NullValue;
            }
            else if ((style4 != null) && !style4.IsNullValueDefault)
            {
                placeholderCellStyle.NullValue = style4.NullValue;
            }
            else
            {
                placeholderCellStyle.NullValue = defaultCellStyle.NullValue;
            }
            if ((style != null) && !style.IsDataSourceNullValueDefault)
            {
                placeholderCellStyle.DataSourceNullValue = style.DataSourceNullValue;
            }
            else if ((style3 != null) && !style3.IsDataSourceNullValueDefault)
            {
                placeholderCellStyle.DataSourceNullValue = style3.DataSourceNullValue;
            }
            else if (!base.DataGridView.RowsDefaultCellStyle.IsDataSourceNullValueDefault && (((rowIndex % 2) == 0) || base.DataGridView.AlternatingRowsDefaultCellStyle.IsDataSourceNullValueDefault))
            {
                placeholderCellStyle.DataSourceNullValue = base.DataGridView.RowsDefaultCellStyle.DataSourceNullValue;
            }
            else if (((rowIndex % 2) == 1) && !base.DataGridView.AlternatingRowsDefaultCellStyle.IsDataSourceNullValueDefault)
            {
                placeholderCellStyle.DataSourceNullValue = base.DataGridView.AlternatingRowsDefaultCellStyle.DataSourceNullValue;
            }
            else if ((style4 != null) && !style4.IsDataSourceNullValueDefault)
            {
                placeholderCellStyle.DataSourceNullValue = style4.DataSourceNullValue;
            }
            else
            {
                placeholderCellStyle.DataSourceNullValue = defaultCellStyle.DataSourceNullValue;
            }
            if ((style != null) && (style.Format.Length != 0))
            {
                placeholderCellStyle.Format = style.Format;
            }
            else if ((style3 != null) && (style3.Format.Length != 0))
            {
                placeholderCellStyle.Format = style3.Format;
            }
            else if ((base.DataGridView.RowsDefaultCellStyle.Format.Length != 0) && (((rowIndex % 2) == 0) || (base.DataGridView.AlternatingRowsDefaultCellStyle.Format.Length == 0)))
            {
                placeholderCellStyle.Format = base.DataGridView.RowsDefaultCellStyle.Format;
            }
            else if (((rowIndex % 2) == 1) && (base.DataGridView.AlternatingRowsDefaultCellStyle.Format.Length != 0))
            {
                placeholderCellStyle.Format = base.DataGridView.AlternatingRowsDefaultCellStyle.Format;
            }
            else if ((style4 != null) && (style4.Format.Length != 0))
            {
                placeholderCellStyle.Format = style4.Format;
            }
            else
            {
                placeholderCellStyle.Format = defaultCellStyle.Format;
            }
            if ((style != null) && !style.IsFormatProviderDefault)
            {
                placeholderCellStyle.FormatProvider = style.FormatProvider;
            }
            else if ((style3 != null) && !style3.IsFormatProviderDefault)
            {
                placeholderCellStyle.FormatProvider = style3.FormatProvider;
            }
            else if (!base.DataGridView.RowsDefaultCellStyle.IsFormatProviderDefault && (((rowIndex % 2) == 0) || base.DataGridView.AlternatingRowsDefaultCellStyle.IsFormatProviderDefault))
            {
                placeholderCellStyle.FormatProvider = base.DataGridView.RowsDefaultCellStyle.FormatProvider;
            }
            else if (((rowIndex % 2) == 1) && !base.DataGridView.AlternatingRowsDefaultCellStyle.IsFormatProviderDefault)
            {
                placeholderCellStyle.FormatProvider = base.DataGridView.AlternatingRowsDefaultCellStyle.FormatProvider;
            }
            else if ((style4 != null) && !style4.IsFormatProviderDefault)
            {
                placeholderCellStyle.FormatProvider = style4.FormatProvider;
            }
            else
            {
                placeholderCellStyle.FormatProvider = defaultCellStyle.FormatProvider;
            }
            if ((style != null) && (style.Alignment != DataGridViewContentAlignment.NotSet))
            {
                placeholderCellStyle.AlignmentInternal = style.Alignment;
            }
            else if ((style3 != null) && (style3.Alignment != DataGridViewContentAlignment.NotSet))
            {
                placeholderCellStyle.AlignmentInternal = style3.Alignment;
            }
            else if ((base.DataGridView.RowsDefaultCellStyle.Alignment != DataGridViewContentAlignment.NotSet) && (((rowIndex % 2) == 0) || (base.DataGridView.AlternatingRowsDefaultCellStyle.Alignment == DataGridViewContentAlignment.NotSet)))
            {
                placeholderCellStyle.AlignmentInternal = base.DataGridView.RowsDefaultCellStyle.Alignment;
            }
            else if (((rowIndex % 2) == 1) && (base.DataGridView.AlternatingRowsDefaultCellStyle.Alignment != DataGridViewContentAlignment.NotSet))
            {
                placeholderCellStyle.AlignmentInternal = base.DataGridView.AlternatingRowsDefaultCellStyle.Alignment;
            }
            else if ((style4 != null) && (style4.Alignment != DataGridViewContentAlignment.NotSet))
            {
                placeholderCellStyle.AlignmentInternal = style4.Alignment;
            }
            else
            {
                placeholderCellStyle.AlignmentInternal = defaultCellStyle.Alignment;
            }
            if ((style != null) && (style.WrapMode != DataGridViewTriState.NotSet))
            {
                placeholderCellStyle.WrapModeInternal = style.WrapMode;
            }
            else if ((style3 != null) && (style3.WrapMode != DataGridViewTriState.NotSet))
            {
                placeholderCellStyle.WrapModeInternal = style3.WrapMode;
            }
            else if ((base.DataGridView.RowsDefaultCellStyle.WrapMode != DataGridViewTriState.NotSet) && (((rowIndex % 2) == 0) || (base.DataGridView.AlternatingRowsDefaultCellStyle.WrapMode == DataGridViewTriState.NotSet)))
            {
                placeholderCellStyle.WrapModeInternal = base.DataGridView.RowsDefaultCellStyle.WrapMode;
            }
            else if (((rowIndex % 2) == 1) && (base.DataGridView.AlternatingRowsDefaultCellStyle.WrapMode != DataGridViewTriState.NotSet))
            {
                placeholderCellStyle.WrapModeInternal = base.DataGridView.AlternatingRowsDefaultCellStyle.WrapMode;
            }
            else if ((style4 != null) && (style4.WrapMode != DataGridViewTriState.NotSet))
            {
                placeholderCellStyle.WrapModeInternal = style4.WrapMode;
            }
            else
            {
                placeholderCellStyle.WrapModeInternal = defaultCellStyle.WrapMode;
            }
            if ((style != null) && (style.Tag != null))
            {
                placeholderCellStyle.Tag = style.Tag;
            }
            else if ((style3 != null) && (style3.Tag != null))
            {
                placeholderCellStyle.Tag = style3.Tag;
            }
            else if ((base.DataGridView.RowsDefaultCellStyle.Tag != null) && (((rowIndex % 2) == 0) || (base.DataGridView.AlternatingRowsDefaultCellStyle.Tag == null)))
            {
                placeholderCellStyle.Tag = base.DataGridView.RowsDefaultCellStyle.Tag;
            }
            else if (((rowIndex % 2) == 1) && (base.DataGridView.AlternatingRowsDefaultCellStyle.Tag != null))
            {
                placeholderCellStyle.Tag = base.DataGridView.AlternatingRowsDefaultCellStyle.Tag;
            }
            else if ((style4 != null) && (style4.Tag != null))
            {
                placeholderCellStyle.Tag = style4.Tag;
            }
            else
            {
                placeholderCellStyle.Tag = defaultCellStyle.Tag;
            }
            if ((style != null) && (style.Padding != Padding.Empty))
            {
                placeholderCellStyle.PaddingInternal = style.Padding;
                return placeholderCellStyle;
            }
            if ((style3 != null) && (style3.Padding != Padding.Empty))
            {
                placeholderCellStyle.PaddingInternal = style3.Padding;
                return placeholderCellStyle;
            }
            if ((base.DataGridView.RowsDefaultCellStyle.Padding != Padding.Empty) && (((rowIndex % 2) == 0) || (base.DataGridView.AlternatingRowsDefaultCellStyle.Padding == Padding.Empty)))
            {
                placeholderCellStyle.PaddingInternal = base.DataGridView.RowsDefaultCellStyle.Padding;
                return placeholderCellStyle;
            }
            if (((rowIndex % 2) == 1) && (base.DataGridView.AlternatingRowsDefaultCellStyle.Padding != Padding.Empty))
            {
                placeholderCellStyle.PaddingInternal = base.DataGridView.AlternatingRowsDefaultCellStyle.Padding;
                return placeholderCellStyle;
            }
            if ((style4 != null) && (style4.Padding != Padding.Empty))
            {
                placeholderCellStyle.PaddingInternal = style4.Padding;
                return placeholderCellStyle;
            }
            placeholderCellStyle.PaddingInternal = defaultCellStyle.Padding;
            return placeholderCellStyle;
        }

        internal DataGridViewCellStyle GetInheritedStyleInternal(int rowIndex)
        {
            return this.GetInheritedStyle(null, rowIndex, true);
        }

        internal int GetPreferredHeight(int rowIndex, int width)
        {
            if (base.DataGridView == null)
            {
                return -1;
            }
            DataGridViewCellStyle cellStyle = this.GetInheritedStyle(null, rowIndex, false);
            using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
            {
                return this.GetPreferredSize(graphics, cellStyle, rowIndex, new System.Drawing.Size(width, 0)).Height;
            }
        }

        internal System.Drawing.Size GetPreferredSize(int rowIndex)
        {
            if (base.DataGridView == null)
            {
                return new System.Drawing.Size(-1, -1);
            }
            DataGridViewCellStyle cellStyle = this.GetInheritedStyle(null, rowIndex, false);
            using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
            {
                return this.GetPreferredSize(graphics, cellStyle, rowIndex, System.Drawing.Size.Empty);
            }
        }

        protected virtual System.Drawing.Size GetPreferredSize(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, System.Drawing.Size constraintSize)
        {
            return new System.Drawing.Size(-1, -1);
        }

        internal static int GetPreferredTextHeight(Graphics g, bool rightToLeft, string text, DataGridViewCellStyle cellStyle, int maxWidth, out bool widthTruncated)
        {
            TextFormatFlags flags = DataGridViewUtilities.ComputeTextFormatFlagsForCellStyleAlignment(rightToLeft, cellStyle.Alignment, cellStyle.WrapMode);
            if (cellStyle.WrapMode == DataGridViewTriState.True)
            {
                return MeasureTextHeight(g, text, cellStyle.Font, maxWidth, flags, out widthTruncated);
            }
            System.Drawing.Size size = MeasureTextSize(g, text, cellStyle.Font, flags);
            widthTruncated = size.Width > maxWidth;
            return size.Height;
        }

        internal int GetPreferredWidth(int rowIndex, int height)
        {
            if (base.DataGridView == null)
            {
                return -1;
            }
            DataGridViewCellStyle cellStyle = this.GetInheritedStyle(null, rowIndex, false);
            using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
            {
                return this.GetPreferredSize(graphics, cellStyle, rowIndex, new System.Drawing.Size(0, height)).Width;
            }
        }

        protected virtual System.Drawing.Size GetSize(int rowIndex)
        {
            if (base.DataGridView == null)
            {
                return new System.Drawing.Size(-1, -1);
            }
            if (rowIndex == -1)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertyGetOnSharedCell", new object[] { "Size" }));
            }
            return new System.Drawing.Size(this.owningColumn.Thickness, this.owningRow.GetHeight(rowIndex));
        }

        private string GetToolTipText(int rowIndex)
        {
            string toolTipTextInternal = this.ToolTipTextInternal;
            if ((base.DataGridView == null) || (!base.DataGridView.VirtualMode && (base.DataGridView.DataSource == null)))
            {
                return toolTipTextInternal;
            }
            return base.DataGridView.OnCellToolTipTextNeeded(this.ColumnIndex, rowIndex, toolTipTextInternal);
        }

        protected virtual object GetValue(int rowIndex)
        {
            DataGridView dataGridView = base.DataGridView;
            if (dataGridView != null)
            {
                if ((rowIndex < 0) || (rowIndex >= dataGridView.Rows.Count))
                {
                    throw new ArgumentOutOfRangeException("rowIndex");
                }
                if (this.ColumnIndex < 0)
                {
                    throw new InvalidOperationException();
                }
            }
            if ((((dataGridView == null) || ((dataGridView.AllowUserToAddRowsInternal && (rowIndex > -1)) && ((rowIndex == dataGridView.NewRowIndex) && (rowIndex != dataGridView.CurrentCellAddress.Y)))) || ((!dataGridView.VirtualMode && (this.OwningColumn != null)) && !this.OwningColumn.IsDataBound)) || ((rowIndex == -1) || (this.ColumnIndex == -1)))
            {
                return this.Properties.GetObject(PropCellValue);
            }
            if ((this.OwningColumn == null) || !this.OwningColumn.IsDataBound)
            {
                return dataGridView.OnCellValueNeeded(this.ColumnIndex, rowIndex);
            }
            DataGridView.DataGridViewDataConnection dataConnection = dataGridView.DataConnection;
            if (dataConnection == null)
            {
                return null;
            }
            if (dataConnection.CurrencyManager.Count <= rowIndex)
            {
                return this.Properties.GetObject(PropCellValue);
            }
            return dataConnection.GetValue(this.OwningColumn.BoundColumnIndex, this.ColumnIndex, rowIndex);
        }

        internal object GetValueInternal(int rowIndex)
        {
            return this.GetValue(rowIndex);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            DataGridView dataGridView = base.DataGridView;
            if ((dataGridView == null) || (dataGridView.EditingControl == null))
            {
                throw new InvalidOperationException();
            }
            if (dataGridView.EditingControl.ParentInternal == null)
            {
                dataGridView.EditingControl.CausesValidation = dataGridView.CausesValidation;
                dataGridView.EditingPanel.CausesValidation = dataGridView.CausesValidation;
                dataGridView.EditingControl.Visible = true;
                dataGridView.EditingPanel.Visible = false;
                dataGridView.Controls.Add(dataGridView.EditingPanel);
                dataGridView.EditingPanel.Controls.Add(dataGridView.EditingControl);
            }
        }

        protected virtual bool KeyDownUnsharesRow(KeyEventArgs e, int rowIndex)
        {
            return false;
        }

        internal bool KeyDownUnsharesRowInternal(KeyEventArgs e, int rowIndex)
        {
            return this.KeyDownUnsharesRow(e, rowIndex);
        }

        public virtual bool KeyEntersEditMode(KeyEventArgs e)
        {
            return false;
        }

        protected virtual bool KeyPressUnsharesRow(KeyPressEventArgs e, int rowIndex)
        {
            return false;
        }

        internal bool KeyPressUnsharesRowInternal(KeyPressEventArgs e, int rowIndex)
        {
            return this.KeyPressUnsharesRow(e, rowIndex);
        }

        protected virtual bool KeyUpUnsharesRow(KeyEventArgs e, int rowIndex)
        {
            return false;
        }

        internal bool KeyUpUnsharesRowInternal(KeyEventArgs e, int rowIndex)
        {
            return this.KeyUpUnsharesRow(e, rowIndex);
        }

        protected virtual bool LeaveUnsharesRow(int rowIndex, bool throughMouseClick)
        {
            return false;
        }

        internal bool LeaveUnsharesRowInternal(int rowIndex, bool throughMouseClick)
        {
            return this.LeaveUnsharesRow(rowIndex, throughMouseClick);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static int MeasureTextHeight(Graphics graphics, string text, Font font, int maxWidth, TextFormatFlags flags)
        {
            bool flag;
            return MeasureTextHeight(graphics, text, font, maxWidth, flags, out flag);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static int MeasureTextHeight(Graphics graphics, string text, Font font, int maxWidth, TextFormatFlags flags, out bool widthTruncated)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (font == null)
            {
                throw new ArgumentNullException("font");
            }
            if (maxWidth <= 0)
            {
                object[] args = new object[] { "maxWidth", maxWidth.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                throw new ArgumentOutOfRangeException("maxWidth", System.Windows.Forms.SR.GetString("InvalidLowBoundArgument", args));
            }
            if (!DataGridViewUtilities.ValidTextFormatFlags(flags))
            {
                throw new InvalidEnumArgumentException("flags", (int) flags, typeof(TextFormatFlags));
            }
            flags &= TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.WordBreak;
            System.Drawing.Size size = TextRenderer.MeasureText(text, font, new System.Drawing.Size(maxWidth, 0x7fffffff), flags);
            widthTruncated = size.Width > maxWidth;
            return size.Height;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static System.Drawing.Size MeasureTextPreferredSize(Graphics graphics, string text, Font font, float maxRatio, TextFormatFlags flags)
        {
            System.Drawing.Size size2;
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (font == null)
            {
                throw new ArgumentNullException("font");
            }
            if (maxRatio <= 0f)
            {
                throw new ArgumentOutOfRangeException("maxRatio", System.Windows.Forms.SR.GetString("InvalidLowBoundArgument", new object[] { "maxRatio", maxRatio.ToString(CultureInfo.CurrentCulture), "0.0" }));
            }
            if (!DataGridViewUtilities.ValidTextFormatFlags(flags))
            {
                throw new InvalidEnumArgumentException("flags", (int) flags, typeof(TextFormatFlags));
            }
            if (string.IsNullOrEmpty(text))
            {
                return new System.Drawing.Size(0, 0);
            }
            System.Drawing.Size size = MeasureTextSize(graphics, text, font, flags);
            if ((size.Width / size.Height) <= maxRatio)
            {
                return size;
            }
            flags &= TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.WordBreak;
            float num = ((((float) (size.Width * size.Width)) / ((float) size.Height)) / maxRatio) * 1.1f;
            do
            {
                size2 = TextRenderer.MeasureText(text, font, new System.Drawing.Size((int) num, 0x7fffffff), flags);
                if (((size2.Width / size2.Height) <= maxRatio) || (size2.Width > ((int) num)))
                {
                    return size2;
                }
                num = size2.Width * 0.9f;
            }
            while (num > 1f);
            return size2;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static System.Drawing.Size MeasureTextSize(Graphics graphics, string text, Font font, TextFormatFlags flags)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (font == null)
            {
                throw new ArgumentNullException("font");
            }
            if (!DataGridViewUtilities.ValidTextFormatFlags(flags))
            {
                throw new InvalidEnumArgumentException("flags", (int) flags, typeof(TextFormatFlags));
            }
            flags &= TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.WordBreak;
            return TextRenderer.MeasureText(text, font, new System.Drawing.Size(0x7fffffff, 0x7fffffff), flags);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static int MeasureTextWidth(Graphics graphics, string text, Font font, int maxHeight, TextFormatFlags flags)
        {
            if (maxHeight <= 0)
            {
                object[] args = new object[] { "maxHeight", maxHeight.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                throw new ArgumentOutOfRangeException("maxHeight", System.Windows.Forms.SR.GetString("InvalidLowBoundArgument", args));
            }
            System.Drawing.Size size = MeasureTextSize(graphics, text, font, flags);
            if ((size.Height >= maxHeight) || ((flags & TextFormatFlags.SingleLine) != TextFormatFlags.Default))
            {
                return size.Width;
            }
            flags &= TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.WordBreak;
            int width = size.Width;
            float num2 = width * 0.9f;
            do
            {
                System.Drawing.Size size2 = TextRenderer.MeasureText(text, font, new System.Drawing.Size((int) num2, maxHeight), flags);
                if ((size2.Height > maxHeight) || (size2.Width > ((int) num2)))
                {
                    return width;
                }
                width = (int) num2;
                num2 = size2.Width * 0.9f;
            }
            while (num2 > 1f);
            return width;
        }

        protected virtual bool MouseClickUnsharesRow(DataGridViewCellMouseEventArgs e)
        {
            return false;
        }

        internal bool MouseClickUnsharesRowInternal(DataGridViewCellMouseEventArgs e)
        {
            return this.MouseClickUnsharesRow(e);
        }

        protected virtual bool MouseDoubleClickUnsharesRow(DataGridViewCellMouseEventArgs e)
        {
            return false;
        }

        internal bool MouseDoubleClickUnsharesRowInternal(DataGridViewCellMouseEventArgs e)
        {
            return this.MouseDoubleClickUnsharesRow(e);
        }

        protected virtual bool MouseDownUnsharesRow(DataGridViewCellMouseEventArgs e)
        {
            return false;
        }

        internal bool MouseDownUnsharesRowInternal(DataGridViewCellMouseEventArgs e)
        {
            return this.MouseDownUnsharesRow(e);
        }

        protected virtual bool MouseEnterUnsharesRow(int rowIndex)
        {
            return false;
        }

        internal bool MouseEnterUnsharesRowInternal(int rowIndex)
        {
            return this.MouseEnterUnsharesRow(rowIndex);
        }

        protected virtual bool MouseLeaveUnsharesRow(int rowIndex)
        {
            return false;
        }

        internal bool MouseLeaveUnsharesRowInternal(int rowIndex)
        {
            return this.MouseLeaveUnsharesRow(rowIndex);
        }

        protected virtual bool MouseMoveUnsharesRow(DataGridViewCellMouseEventArgs e)
        {
            return false;
        }

        internal bool MouseMoveUnsharesRowInternal(DataGridViewCellMouseEventArgs e)
        {
            return this.MouseMoveUnsharesRow(e);
        }

        protected virtual bool MouseUpUnsharesRow(DataGridViewCellMouseEventArgs e)
        {
            return false;
        }

        internal bool MouseUpUnsharesRowInternal(DataGridViewCellMouseEventArgs e)
        {
            return this.MouseUpUnsharesRow(e);
        }

        private void OnCellDataAreaMouseEnterInternal(int rowIndex)
        {
            if (!base.DataGridView.ShowCellToolTips)
            {
                return;
            }
            Point currentCellAddress = base.DataGridView.CurrentCellAddress;
            if (((currentCellAddress.X != -1) && (currentCellAddress.X == this.ColumnIndex)) && ((currentCellAddress.Y == rowIndex) && (base.DataGridView.EditingControl != null)))
            {
                return;
            }
            string toolTipText = this.GetToolTipText(rowIndex);
            if (string.IsNullOrEmpty(toolTipText))
            {
                if (!(this.FormattedValueType == stringType))
                {
                    goto Label_01E4;
                }
                if ((rowIndex != -1) && (this.OwningColumn != null))
                {
                    int preferredWidth = this.GetPreferredWidth(rowIndex, this.OwningRow.Height);
                    int preferredHeight = this.GetPreferredHeight(rowIndex, this.OwningColumn.Width);
                    if ((this.OwningColumn.Width < preferredWidth) || (this.OwningRow.Height < preferredHeight))
                    {
                        DataGridViewCellStyle dataGridViewCellStyle = this.GetInheritedStyle(null, rowIndex, false);
                        string str2 = this.GetEditedFormattedValue(this.GetValue(rowIndex), rowIndex, ref dataGridViewCellStyle, DataGridViewDataErrorContexts.Display) as string;
                        if (!string.IsNullOrEmpty(str2))
                        {
                            toolTipText = TruncateToolTipText(str2);
                        }
                    }
                    goto Label_01E4;
                }
                if ((((rowIndex == -1) || (this.OwningRow == null)) || ((!base.DataGridView.RowHeadersVisible || (base.DataGridView.RowHeadersWidth <= 0)) || (this.OwningColumn != null))) && (rowIndex != -1))
                {
                    goto Label_01E4;
                }
                string str3 = this.GetValue(rowIndex) as string;
                if (string.IsNullOrEmpty(str3))
                {
                    goto Label_01E4;
                }
                DataGridViewCellStyle cellStyle = this.GetInheritedStyle(null, rowIndex, false);
                using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
                {
                    Rectangle rectangle = this.GetContentBounds(graphics, cellStyle, rowIndex);
                    bool widthTruncated = false;
                    int num3 = 0;
                    if (rectangle.Width > 0)
                    {
                        num3 = GetPreferredTextHeight(graphics, base.DataGridView.RightToLeftInternal, str3, cellStyle, rectangle.Width, out widthTruncated);
                    }
                    else
                    {
                        widthTruncated = true;
                    }
                    if ((num3 > rectangle.Height) || widthTruncated)
                    {
                        toolTipText = TruncateToolTipText(str3);
                    }
                    goto Label_01E4;
                }
            }
            if (base.DataGridView.IsRestricted)
            {
                toolTipText = TruncateToolTipText(toolTipText);
            }
        Label_01E4:
            if (!string.IsNullOrEmpty(toolTipText))
            {
                base.DataGridView.ActivateToolTip(true, toolTipText, this.ColumnIndex, rowIndex);
            }
        }

        private void OnCellDataAreaMouseLeaveInternal()
        {
            if (!base.DataGridView.IsDisposed)
            {
                base.DataGridView.ActivateToolTip(false, string.Empty, -1, -1);
            }
        }

        private void OnCellErrorAreaMouseEnterInternal(int rowIndex)
        {
            string errorText = this.GetErrorText(rowIndex);
            base.DataGridView.ActivateToolTip(true, errorText, this.ColumnIndex, rowIndex);
        }

        private void OnCellErrorAreaMouseLeaveInternal()
        {
            base.DataGridView.ActivateToolTip(false, string.Empty, -1, -1);
        }

        protected virtual void OnClick(DataGridViewCellEventArgs e)
        {
        }

        internal void OnClickInternal(DataGridViewCellEventArgs e)
        {
            this.OnClick(e);
        }

        internal void OnCommonChange()
        {
            if (((base.DataGridView != null) && !base.DataGridView.IsDisposed) && !base.DataGridView.Disposing)
            {
                if (this.RowIndex == -1)
                {
                    base.DataGridView.OnColumnCommonChange(this.ColumnIndex);
                }
                else
                {
                    base.DataGridView.OnCellCommonChange(this.ColumnIndex, this.RowIndex);
                }
            }
        }

        protected virtual void OnContentClick(DataGridViewCellEventArgs e)
        {
        }

        internal void OnContentClickInternal(DataGridViewCellEventArgs e)
        {
            this.OnContentClick(e);
        }

        protected virtual void OnContentDoubleClick(DataGridViewCellEventArgs e)
        {
        }

        internal void OnContentDoubleClickInternal(DataGridViewCellEventArgs e)
        {
            this.OnContentDoubleClick(e);
        }

        protected override void OnDataGridViewChanged()
        {
            if (this.HasStyle)
            {
                if (base.DataGridView == null)
                {
                    this.Style.RemoveScope(DataGridViewCellStyleScopes.Cell);
                }
                else
                {
                    this.Style.AddScope(base.DataGridView, DataGridViewCellStyleScopes.Cell);
                }
            }
            base.OnDataGridViewChanged();
        }

        protected virtual void OnDoubleClick(DataGridViewCellEventArgs e)
        {
        }

        internal void OnDoubleClickInternal(DataGridViewCellEventArgs e)
        {
            this.OnDoubleClick(e);
        }

        protected virtual void OnEnter(int rowIndex, bool throughMouseClick)
        {
        }

        internal void OnEnterInternal(int rowIndex, bool throughMouseClick)
        {
            this.OnEnter(rowIndex, throughMouseClick);
        }

        protected virtual void OnKeyDown(KeyEventArgs e, int rowIndex)
        {
        }

        internal void OnKeyDownInternal(KeyEventArgs e, int rowIndex)
        {
            this.OnKeyDown(e, rowIndex);
        }

        protected virtual void OnKeyPress(KeyPressEventArgs e, int rowIndex)
        {
        }

        internal void OnKeyPressInternal(KeyPressEventArgs e, int rowIndex)
        {
            this.OnKeyPress(e, rowIndex);
        }

        protected virtual void OnKeyUp(KeyEventArgs e, int rowIndex)
        {
        }

        internal void OnKeyUpInternal(KeyEventArgs e, int rowIndex)
        {
            this.OnKeyUp(e, rowIndex);
        }

        protected virtual void OnLeave(int rowIndex, bool throughMouseClick)
        {
        }

        internal void OnLeaveInternal(int rowIndex, bool throughMouseClick)
        {
            this.OnLeave(rowIndex, throughMouseClick);
        }

        protected virtual void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
        }

        internal void OnMouseClickInternal(DataGridViewCellMouseEventArgs e)
        {
            this.OnMouseClick(e);
        }

        protected virtual void OnMouseDoubleClick(DataGridViewCellMouseEventArgs e)
        {
        }

        internal void OnMouseDoubleClickInternal(DataGridViewCellMouseEventArgs e)
        {
            this.OnMouseDoubleClick(e);
        }

        protected virtual void OnMouseDown(DataGridViewCellMouseEventArgs e)
        {
        }

        internal void OnMouseDownInternal(DataGridViewCellMouseEventArgs e)
        {
            base.DataGridView.CellMouseDownInContentBounds = this.GetContentBounds(e.RowIndex).Contains(e.X, e.Y);
            if ((((this.ColumnIndex < 0) || (e.RowIndex < 0)) && base.DataGridView.ApplyVisualStylesToHeaderCells) || (((this.ColumnIndex >= 0) && (e.RowIndex >= 0)) && base.DataGridView.ApplyVisualStylesToInnerCells))
            {
                base.DataGridView.InvalidateCell(this.ColumnIndex, e.RowIndex);
            }
            this.OnMouseDown(e);
        }

        protected virtual void OnMouseEnter(int rowIndex)
        {
        }

        internal void OnMouseEnterInternal(int rowIndex)
        {
            this.OnMouseEnter(rowIndex);
        }

        protected virtual void OnMouseLeave(int rowIndex)
        {
        }

        internal void OnMouseLeaveInternal(int rowIndex)
        {
            switch (this.CurrentMouseLocation)
            {
                case 1:
                    this.OnCellDataAreaMouseLeaveInternal();
                    break;

                case 2:
                    this.OnCellErrorAreaMouseLeaveInternal();
                    break;
            }
            this.CurrentMouseLocation = 0;
            this.OnMouseLeave(rowIndex);
        }

        protected virtual void OnMouseMove(DataGridViewCellMouseEventArgs e)
        {
        }

        internal void OnMouseMoveInternal(DataGridViewCellMouseEventArgs e)
        {
            byte currentMouseLocation = this.CurrentMouseLocation;
            this.UpdateCurrentMouseLocation(e);
            switch (currentMouseLocation)
            {
                case 0:
                    if (this.CurrentMouseLocation != 1)
                    {
                        this.OnCellErrorAreaMouseEnterInternal(e.RowIndex);
                        break;
                    }
                    this.OnCellDataAreaMouseEnterInternal(e.RowIndex);
                    break;

                case 1:
                    if (this.CurrentMouseLocation == 2)
                    {
                        this.OnCellDataAreaMouseLeaveInternal();
                        this.OnCellErrorAreaMouseEnterInternal(e.RowIndex);
                    }
                    break;

                case 2:
                    if (this.CurrentMouseLocation == 1)
                    {
                        this.OnCellErrorAreaMouseLeaveInternal();
                        this.OnCellDataAreaMouseEnterInternal(e.RowIndex);
                    }
                    break;
            }
            this.OnMouseMove(e);
        }

        protected virtual void OnMouseUp(DataGridViewCellMouseEventArgs e)
        {
        }

        internal void OnMouseUpInternal(DataGridViewCellMouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;
            if ((((this.ColumnIndex < 0) || (e.RowIndex < 0)) && base.DataGridView.ApplyVisualStylesToHeaderCells) || (((this.ColumnIndex >= 0) && (e.RowIndex >= 0)) && base.DataGridView.ApplyVisualStylesToInnerCells))
            {
                base.DataGridView.InvalidateCell(this.ColumnIndex, e.RowIndex);
            }
            if ((e.Button == MouseButtons.Left) && this.GetContentBounds(e.RowIndex).Contains(x, y))
            {
                base.DataGridView.OnCommonCellContentClick(e.ColumnIndex, e.RowIndex, e.Clicks > 1);
            }
            if (((base.DataGridView != null) && (e.ColumnIndex < base.DataGridView.Columns.Count)) && (e.RowIndex < base.DataGridView.Rows.Count))
            {
                this.OnMouseUp(e);
            }
        }

        protected virtual void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
        }

        internal static bool PaintBackground(DataGridViewPaintParts paintParts)
        {
            return ((paintParts & DataGridViewPaintParts.Background) != DataGridViewPaintParts.None);
        }

        internal static bool PaintBorder(DataGridViewPaintParts paintParts)
        {
            return ((paintParts & DataGridViewPaintParts.Border) != DataGridViewPaintParts.None);
        }

        protected virtual void PaintBorder(Graphics graphics, Rectangle clipBounds, Rectangle bounds, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle)
        {
            int num;
            int num2;
            int x;
            int num5;
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            if (base.DataGridView == null)
            {
                return;
            }
            Pen darkPen = null;
            Pen lightPen = null;
            Pen cachedPen = base.DataGridView.GetCachedPen(cellStyle.BackColor);
            Pen gridPen = base.DataGridView.GridPen;
            this.GetContrastedPens(cellStyle.BackColor, ref darkPen, ref lightPen);
            int width = (this.owningColumn == null) ? 0 : this.owningColumn.DividerWidth;
            if (width != 0)
            {
                Color controlLightLight;
                if (width > bounds.Width)
                {
                    width = bounds.Width;
                }
                switch (advancedBorderStyle.Right)
                {
                    case DataGridViewAdvancedCellBorderStyle.Single:
                        controlLightLight = base.DataGridView.GridPen.Color;
                        break;

                    case DataGridViewAdvancedCellBorderStyle.Inset:
                        controlLightLight = SystemColors.ControlLightLight;
                        break;

                    default:
                        controlLightLight = SystemColors.ControlDark;
                        break;
                }
                graphics.FillRectangle(base.DataGridView.GetCachedBrush(controlLightLight), base.DataGridView.RightToLeftInternal ? bounds.X : (bounds.Right - width), bounds.Y, width, bounds.Height);
                if (base.DataGridView.RightToLeftInternal)
                {
                    bounds.X += width;
                }
                bounds.Width -= width;
                if (bounds.Width <= 0)
                {
                    return;
                }
            }
            width = (this.owningRow == null) ? 0 : this.owningRow.DividerHeight;
            if (width != 0)
            {
                Color color;
                if (width > bounds.Height)
                {
                    width = bounds.Height;
                }
                switch (advancedBorderStyle.Bottom)
                {
                    case DataGridViewAdvancedCellBorderStyle.Single:
                        color = base.DataGridView.GridPen.Color;
                        break;

                    case DataGridViewAdvancedCellBorderStyle.Inset:
                        color = SystemColors.ControlLightLight;
                        break;

                    default:
                        color = SystemColors.ControlDark;
                        break;
                }
                graphics.FillRectangle(base.DataGridView.GetCachedBrush(color), bounds.X, bounds.Bottom - width, bounds.Width, width);
                bounds.Height -= width;
                if (bounds.Height <= 0)
                {
                    return;
                }
            }
            if (advancedBorderStyle.All == DataGridViewAdvancedCellBorderStyle.None)
            {
                return;
            }
            switch (advancedBorderStyle.Left)
            {
                case DataGridViewAdvancedCellBorderStyle.Single:
                    graphics.DrawLine(gridPen, bounds.X, bounds.Y, bounds.X, bounds.Bottom - 1);
                    goto Label_0432;

                case DataGridViewAdvancedCellBorderStyle.Inset:
                    graphics.DrawLine(darkPen, bounds.X, bounds.Y, bounds.X, bounds.Bottom - 1);
                    goto Label_0432;

                case DataGridViewAdvancedCellBorderStyle.InsetDouble:
                    num = bounds.Y + 1;
                    num2 = bounds.Bottom - 1;
                    if ((advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.OutsetPartial) || (advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.None))
                    {
                        num--;
                    }
                    if (advancedBorderStyle.Bottom == DataGridViewAdvancedCellBorderStyle.OutsetPartial)
                    {
                        num2++;
                    }
                    graphics.DrawLine(lightPen, bounds.X, bounds.Y, bounds.X, bounds.Bottom - 1);
                    graphics.DrawLine(darkPen, bounds.X + 1, num, bounds.X + 1, num2);
                    goto Label_0432;

                case DataGridViewAdvancedCellBorderStyle.Outset:
                    graphics.DrawLine(lightPen, bounds.X, bounds.Y, bounds.X, bounds.Bottom - 1);
                    goto Label_0432;

                case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
                    num = bounds.Y + 1;
                    num2 = bounds.Bottom - 1;
                    if ((advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.OutsetPartial) || (advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.None))
                    {
                        num--;
                    }
                    if (advancedBorderStyle.Bottom == DataGridViewAdvancedCellBorderStyle.OutsetPartial)
                    {
                        num2++;
                    }
                    graphics.DrawLine(darkPen, bounds.X, bounds.Y, bounds.X, bounds.Bottom - 1);
                    graphics.DrawLine(lightPen, bounds.X + 1, num, bounds.X + 1, num2);
                    goto Label_0432;

                case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
                    num = bounds.Y + 2;
                    num2 = bounds.Bottom - 3;
                    if ((advancedBorderStyle.Top != DataGridViewAdvancedCellBorderStyle.OutsetDouble) && (advancedBorderStyle.Top != DataGridViewAdvancedCellBorderStyle.InsetDouble))
                    {
                        if (advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.None)
                        {
                            num--;
                        }
                        break;
                    }
                    num++;
                    break;

                default:
                    goto Label_0432;
            }
            graphics.DrawLine(cachedPen, bounds.X, bounds.Y, bounds.X, bounds.Bottom - 1);
            graphics.DrawLine(lightPen, bounds.X, num, bounds.X, num2);
        Label_0432:
            switch (advancedBorderStyle.Right)
            {
                case DataGridViewAdvancedCellBorderStyle.Single:
                    graphics.DrawLine(gridPen, bounds.Right - 1, bounds.Y, bounds.Right - 1, bounds.Bottom - 1);
                    goto Label_067D;

                case DataGridViewAdvancedCellBorderStyle.Inset:
                    graphics.DrawLine(lightPen, bounds.Right - 1, bounds.Y, bounds.Right - 1, bounds.Bottom - 1);
                    goto Label_067D;

                case DataGridViewAdvancedCellBorderStyle.InsetDouble:
                    num = bounds.Y + 1;
                    num2 = bounds.Bottom - 1;
                    if ((advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.OutsetPartial) || (advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.None))
                    {
                        num--;
                    }
                    if ((advancedBorderStyle.Bottom == DataGridViewAdvancedCellBorderStyle.OutsetPartial) || (advancedBorderStyle.Bottom == DataGridViewAdvancedCellBorderStyle.Inset))
                    {
                        num2++;
                    }
                    graphics.DrawLine(lightPen, bounds.Right - 2, bounds.Y, bounds.Right - 2, bounds.Bottom - 1);
                    graphics.DrawLine(darkPen, bounds.Right - 1, num, bounds.Right - 1, num2);
                    goto Label_067D;

                case DataGridViewAdvancedCellBorderStyle.Outset:
                    graphics.DrawLine(darkPen, bounds.Right - 1, bounds.Y, bounds.Right - 1, bounds.Bottom - 1);
                    goto Label_067D;

                case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
                    num = bounds.Y + 1;
                    num2 = bounds.Bottom - 1;
                    if ((advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.OutsetPartial) || (advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.None))
                    {
                        num--;
                    }
                    if (advancedBorderStyle.Bottom == DataGridViewAdvancedCellBorderStyle.OutsetPartial)
                    {
                        num2++;
                    }
                    graphics.DrawLine(darkPen, bounds.Right - 2, bounds.Y, bounds.Right - 2, bounds.Bottom - 1);
                    graphics.DrawLine(lightPen, bounds.Right - 1, num, bounds.Right - 1, num2);
                    goto Label_067D;

                case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
                    num = bounds.Y + 2;
                    num2 = bounds.Bottom - 3;
                    if ((advancedBorderStyle.Top != DataGridViewAdvancedCellBorderStyle.OutsetDouble) && (advancedBorderStyle.Top != DataGridViewAdvancedCellBorderStyle.InsetDouble))
                    {
                        if (advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.None)
                        {
                            num--;
                        }
                        break;
                    }
                    num++;
                    break;

                default:
                    goto Label_067D;
            }
            graphics.DrawLine(cachedPen, bounds.Right - 1, bounds.Y, bounds.Right - 1, bounds.Bottom - 1);
            graphics.DrawLine(darkPen, bounds.Right - 1, num, bounds.Right - 1, num2);
        Label_067D:
            switch (advancedBorderStyle.Top)
            {
                case DataGridViewAdvancedCellBorderStyle.Single:
                    graphics.DrawLine(gridPen, bounds.X, bounds.Y, bounds.Right - 1, bounds.Y);
                    break;

                case DataGridViewAdvancedCellBorderStyle.Inset:
                    x = bounds.X;
                    num5 = bounds.Right - 1;
                    if ((advancedBorderStyle.Left == DataGridViewAdvancedCellBorderStyle.OutsetDouble) || (advancedBorderStyle.Left == DataGridViewAdvancedCellBorderStyle.InsetDouble))
                    {
                        x++;
                    }
                    if ((advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.Inset) || (advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.Outset))
                    {
                        num5--;
                    }
                    graphics.DrawLine(darkPen, x, bounds.Y, num5, bounds.Y);
                    break;

                case DataGridViewAdvancedCellBorderStyle.InsetDouble:
                    x = bounds.X;
                    if ((advancedBorderStyle.Left != DataGridViewAdvancedCellBorderStyle.OutsetPartial) && (advancedBorderStyle.Left != DataGridViewAdvancedCellBorderStyle.None))
                    {
                        x++;
                    }
                    num5 = bounds.Right - 2;
                    if ((advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.OutsetPartial) || (advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.None))
                    {
                        num5++;
                    }
                    graphics.DrawLine(lightPen, bounds.X, bounds.Y, bounds.Right - 1, bounds.Y);
                    graphics.DrawLine(darkPen, x, bounds.Y + 1, num5, bounds.Y + 1);
                    break;

                case DataGridViewAdvancedCellBorderStyle.Outset:
                    x = bounds.X;
                    num5 = bounds.Right - 1;
                    if ((advancedBorderStyle.Left == DataGridViewAdvancedCellBorderStyle.OutsetDouble) || (advancedBorderStyle.Left == DataGridViewAdvancedCellBorderStyle.InsetDouble))
                    {
                        x++;
                    }
                    if ((advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.Inset) || (advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.Outset))
                    {
                        num5--;
                    }
                    graphics.DrawLine(lightPen, x, bounds.Y, num5, bounds.Y);
                    break;

                case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
                    x = bounds.X;
                    if ((advancedBorderStyle.Left != DataGridViewAdvancedCellBorderStyle.OutsetPartial) && (advancedBorderStyle.Left != DataGridViewAdvancedCellBorderStyle.None))
                    {
                        x++;
                    }
                    num5 = bounds.Right - 2;
                    if ((advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.OutsetPartial) || (advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.None))
                    {
                        num5++;
                    }
                    graphics.DrawLine(darkPen, bounds.X, bounds.Y, bounds.Right - 1, bounds.Y);
                    graphics.DrawLine(lightPen, x, bounds.Y + 1, num5, bounds.Y + 1);
                    break;

                case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
                    x = bounds.X;
                    num5 = bounds.Right - 1;
                    if (advancedBorderStyle.Left != DataGridViewAdvancedCellBorderStyle.None)
                    {
                        x++;
                        if ((advancedBorderStyle.Left == DataGridViewAdvancedCellBorderStyle.OutsetDouble) || (advancedBorderStyle.Left == DataGridViewAdvancedCellBorderStyle.InsetDouble))
                        {
                            x++;
                        }
                    }
                    if (advancedBorderStyle.Right != DataGridViewAdvancedCellBorderStyle.None)
                    {
                        num5--;
                        if ((advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.OutsetDouble) || (advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.InsetDouble))
                        {
                            num5--;
                        }
                    }
                    graphics.DrawLine(cachedPen, x, bounds.Y, num5, bounds.Y);
                    graphics.DrawLine(lightPen, x + 1, bounds.Y, num5 - 1, bounds.Y);
                    break;
            }
            switch (advancedBorderStyle.Bottom)
            {
                case DataGridViewAdvancedCellBorderStyle.Single:
                    graphics.DrawLine(gridPen, bounds.X, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
                    return;

                case DataGridViewAdvancedCellBorderStyle.Inset:
                    num5 = bounds.Right - 1;
                    if (advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.InsetDouble)
                    {
                        num5--;
                    }
                    graphics.DrawLine(lightPen, bounds.X, bounds.Bottom - 1, num5, bounds.Bottom - 1);
                    return;

                case DataGridViewAdvancedCellBorderStyle.InsetDouble:
                case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
                    break;

                case DataGridViewAdvancedCellBorderStyle.Outset:
                    x = bounds.X;
                    num5 = bounds.Right - 1;
                    if ((advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.InsetDouble) || (advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.OutsetDouble))
                    {
                        num5--;
                    }
                    graphics.DrawLine(darkPen, x, bounds.Bottom - 1, num5, bounds.Bottom - 1);
                    return;

                case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
                    x = bounds.X;
                    num5 = bounds.Right - 1;
                    if (advancedBorderStyle.Left != DataGridViewAdvancedCellBorderStyle.None)
                    {
                        x++;
                        if ((advancedBorderStyle.Left == DataGridViewAdvancedCellBorderStyle.OutsetDouble) || (advancedBorderStyle.Left == DataGridViewAdvancedCellBorderStyle.InsetDouble))
                        {
                            x++;
                        }
                    }
                    if (advancedBorderStyle.Right != DataGridViewAdvancedCellBorderStyle.None)
                    {
                        num5--;
                        if ((advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.OutsetDouble) || (advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.InsetDouble))
                        {
                            num5--;
                        }
                    }
                    graphics.DrawLine(cachedPen, x, bounds.Bottom - 1, num5, bounds.Bottom - 1);
                    graphics.DrawLine(darkPen, (int) (x + 1), (int) (bounds.Bottom - 1), (int) (num5 - 1), (int) (bounds.Bottom - 1));
                    break;

                default:
                    return;
            }
        }

        internal static bool PaintContentBackground(DataGridViewPaintParts paintParts)
        {
            return ((paintParts & DataGridViewPaintParts.ContentBackground) != DataGridViewPaintParts.None);
        }

        internal static bool PaintContentForeground(DataGridViewPaintParts paintParts)
        {
            return ((paintParts & DataGridViewPaintParts.ContentForeground) != DataGridViewPaintParts.None);
        }

        internal static bool PaintErrorIcon(DataGridViewPaintParts paintParts)
        {
            return ((paintParts & DataGridViewPaintParts.ErrorIcon) != DataGridViewPaintParts.None);
        }

        private static void PaintErrorIcon(Graphics graphics, Rectangle iconBounds)
        {
            Bitmap errorBitmap = ErrorBitmap;
            if (errorBitmap != null)
            {
                lock (errorBitmap)
                {
                    graphics.DrawImage(errorBitmap, iconBounds, 0, 0, 12, 11, GraphicsUnit.Pixel);
                }
            }
        }

        protected virtual void PaintErrorIcon(Graphics graphics, Rectangle clipBounds, Rectangle cellValueBounds, string errorText)
        {
            if ((!string.IsNullOrEmpty(errorText) && (cellValueBounds.Width >= 20)) && (cellValueBounds.Height >= 0x13))
            {
                PaintErrorIcon(graphics, this.ComputeErrorIconBounds(cellValueBounds));
            }
        }

        internal void PaintErrorIcon(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Rectangle cellBounds, Rectangle cellValueBounds, string errorText)
        {
            if ((!string.IsNullOrEmpty(errorText) && (cellValueBounds.Width >= 20)) && (cellValueBounds.Height >= 0x13))
            {
                Rectangle iconBounds = this.GetErrorIconBounds(graphics, cellStyle, rowIndex);
                if ((iconBounds.Width >= 4) && (iconBounds.Height >= 11))
                {
                    iconBounds.X += cellBounds.X;
                    iconBounds.Y += cellBounds.Y;
                    PaintErrorIcon(graphics, iconBounds);
                }
            }
        }

        internal static bool PaintFocus(DataGridViewPaintParts paintParts)
        {
            return ((paintParts & DataGridViewPaintParts.Focus) != DataGridViewPaintParts.None);
        }

        internal void PaintInternal(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            this.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
        }

        internal static void PaintPadding(Graphics graphics, Rectangle bounds, DataGridViewCellStyle cellStyle, Brush br, bool rightToLeft)
        {
            Rectangle rectangle;
            if (rightToLeft)
            {
                rectangle = new Rectangle(bounds.X, bounds.Y, cellStyle.Padding.Right, bounds.Height);
                graphics.FillRectangle(br, rectangle);
                rectangle.X = bounds.Right - cellStyle.Padding.Left;
                rectangle.Width = cellStyle.Padding.Left;
                graphics.FillRectangle(br, rectangle);
                rectangle.X = bounds.Left + cellStyle.Padding.Right;
            }
            else
            {
                rectangle = new Rectangle(bounds.X, bounds.Y, cellStyle.Padding.Left, bounds.Height);
                graphics.FillRectangle(br, rectangle);
                rectangle.X = bounds.Right - cellStyle.Padding.Right;
                rectangle.Width = cellStyle.Padding.Right;
                graphics.FillRectangle(br, rectangle);
                rectangle.X = bounds.Left + cellStyle.Padding.Left;
            }
            rectangle.Y = bounds.Y;
            rectangle.Width = bounds.Width - cellStyle.Padding.Horizontal;
            rectangle.Height = cellStyle.Padding.Top;
            graphics.FillRectangle(br, rectangle);
            rectangle.Y = bounds.Bottom - cellStyle.Padding.Bottom;
            rectangle.Height = cellStyle.Padding.Bottom;
            graphics.FillRectangle(br, rectangle);
        }

        internal static bool PaintSelectionBackground(DataGridViewPaintParts paintParts)
        {
            return ((paintParts & DataGridViewPaintParts.SelectionBackground) != DataGridViewPaintParts.None);
        }

        internal void PaintWork(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            object obj2;
            DataGridView dataGridView = base.DataGridView;
            int columnIndex = this.ColumnIndex;
            object obj3 = this.GetValue(rowIndex);
            string errorText = this.GetErrorText(rowIndex);
            if ((columnIndex > -1) && (rowIndex > -1))
            {
                obj2 = this.GetEditedFormattedValue(obj3, rowIndex, ref cellStyle, DataGridViewDataErrorContexts.Display | DataGridViewDataErrorContexts.Formatting);
            }
            else
            {
                obj2 = obj3;
            }
            DataGridViewCellPaintingEventArgs cellPaintingEventArgs = dataGridView.CellPaintingEventArgs;
            cellPaintingEventArgs.SetProperties(graphics, clipBounds, cellBounds, rowIndex, columnIndex, cellState, obj3, obj2, errorText, cellStyle, advancedBorderStyle, paintParts);
            dataGridView.OnCellPainting(cellPaintingEventArgs);
            if (!cellPaintingEventArgs.Handled)
            {
                this.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, obj3, obj2, errorText, cellStyle, advancedBorderStyle, paintParts);
            }
        }

        public virtual object ParseFormattedValue(object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter)
        {
            return this.ParseFormattedValueInternal(this.ValueType, formattedValue, cellStyle, formattedValueTypeConverter, valueTypeConverter);
        }

        internal object ParseFormattedValueInternal(System.Type valueType, object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter)
        {
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            if (this.FormattedValueType == null)
            {
                throw new FormatException(System.Windows.Forms.SR.GetString("DataGridViewCell_FormattedValueTypeNull"));
            }
            if (valueType == null)
            {
                throw new FormatException(System.Windows.Forms.SR.GetString("DataGridViewCell_ValueTypeNull"));
            }
            if ((formattedValue == null) || !this.FormattedValueType.IsAssignableFrom(formattedValue.GetType()))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridViewCell_FormattedValueHasWrongType"), "formattedValue");
            }
            return Formatter.ParseObject(formattedValue, valueType, this.FormattedValueType, (valueTypeConverter == null) ? this.ValueTypeConverter : valueTypeConverter, (formattedValueTypeConverter == null) ? this.FormattedValueTypeConverter : formattedValueTypeConverter, cellStyle.FormatProvider, cellStyle.NullValue, cellStyle.IsDataSourceNullValueDefault ? Formatter.GetDefaultDataSourceNullValue(valueType) : cellStyle.DataSourceNullValue);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual void PositionEditingControl(bool setLocation, bool setSize, Rectangle cellBounds, Rectangle cellClip, DataGridViewCellStyle cellStyle, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedColumn, bool isFirstDisplayedRow)
        {
            Rectangle rectangle = this.PositionEditingPanel(cellBounds, cellClip, cellStyle, singleVerticalBorderAdded, singleHorizontalBorderAdded, isFirstDisplayedColumn, isFirstDisplayedRow);
            if (setLocation)
            {
                base.DataGridView.EditingControl.Location = new Point(rectangle.X, rectangle.Y);
            }
            if (setSize)
            {
                base.DataGridView.EditingControl.Size = new System.Drawing.Size(rectangle.Width, rectangle.Height);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual Rectangle PositionEditingPanel(Rectangle cellBounds, Rectangle cellClip, DataGridViewCellStyle cellStyle, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedColumn, bool isFirstDisplayedRow)
        {
            int num;
            int num5;
            if (base.DataGridView == null)
            {
                throw new InvalidOperationException();
            }
            DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder = new DataGridViewAdvancedBorderStyle();
            DataGridViewAdvancedBorderStyle advancedBorderStyle = this.AdjustCellBorderStyle(base.DataGridView.AdvancedCellBorderStyle, dataGridViewAdvancedBorderStylePlaceholder, singleVerticalBorderAdded, singleHorizontalBorderAdded, isFirstDisplayedColumn, isFirstDisplayedRow);
            Rectangle rectangle = this.BorderWidths(advancedBorderStyle);
            rectangle.X += cellStyle.Padding.Left;
            rectangle.Y += cellStyle.Padding.Top;
            rectangle.Width += cellStyle.Padding.Right;
            rectangle.Height += cellStyle.Padding.Bottom;
            int width = cellBounds.Width;
            int height = cellBounds.Height;
            if ((cellClip.X - cellBounds.X) >= rectangle.X)
            {
                num = cellClip.X;
                width -= cellClip.X - cellBounds.X;
            }
            else
            {
                num = cellBounds.X + rectangle.X;
                width -= rectangle.X;
            }
            if (cellClip.Right <= (cellBounds.Right - rectangle.Width))
            {
                width -= cellBounds.Right - cellClip.Right;
            }
            else
            {
                width -= rectangle.Width;
            }
            int x = cellBounds.X - cellClip.X;
            int num4 = (cellBounds.Width - rectangle.X) - rectangle.Width;
            if ((cellClip.Y - cellBounds.Y) >= rectangle.Y)
            {
                num5 = cellClip.Y;
                height -= cellClip.Y - cellBounds.Y;
            }
            else
            {
                num5 = cellBounds.Y + rectangle.Y;
                height -= rectangle.Y;
            }
            if (cellClip.Bottom <= (cellBounds.Bottom - rectangle.Height))
            {
                height -= cellBounds.Bottom - cellClip.Bottom;
            }
            else
            {
                height -= rectangle.Height;
            }
            int y = cellBounds.Y - cellClip.Y;
            int num8 = (cellBounds.Height - rectangle.Y) - rectangle.Height;
            base.DataGridView.EditingPanel.Location = new Point(num, num5);
            base.DataGridView.EditingPanel.Size = new System.Drawing.Size(width, height);
            return new Rectangle(x, y, num4, num8);
        }

        protected virtual bool SetValue(int rowIndex, object value)
        {
            object obj2 = null;
            DataGridView dataGridView = base.DataGridView;
            if ((dataGridView != null) && !dataGridView.InSortOperation)
            {
                obj2 = this.GetValue(rowIndex);
            }
            if (((dataGridView != null) && (this.OwningColumn != null)) && this.OwningColumn.IsDataBound)
            {
                DataGridView.DataGridViewDataConnection dataConnection = dataGridView.DataConnection;
                if (dataConnection == null)
                {
                    return false;
                }
                if (dataConnection.CurrencyManager.Count <= rowIndex)
                {
                    if ((value != null) || this.Properties.ContainsObject(PropCellValue))
                    {
                        this.Properties.SetObject(PropCellValue, value);
                    }
                }
                else
                {
                    if (!dataConnection.PushValue(this.OwningColumn.BoundColumnIndex, this.ColumnIndex, rowIndex, value))
                    {
                        return false;
                    }
                    if (((base.DataGridView == null) || (this.OwningRow == null)) || (this.OwningRow.DataGridView == null))
                    {
                        return true;
                    }
                    if (this.OwningRow.Index == base.DataGridView.CurrentCellAddress.Y)
                    {
                        base.DataGridView.IsCurrentRowDirtyInternal = true;
                    }
                }
            }
            else if (((dataGridView == null) || !dataGridView.VirtualMode) || ((rowIndex == -1) || (this.ColumnIndex == -1)))
            {
                if ((value != null) || this.Properties.ContainsObject(PropCellValue))
                {
                    this.Properties.SetObject(PropCellValue, value);
                }
            }
            else
            {
                dataGridView.OnCellValuePushed(this.ColumnIndex, rowIndex, value);
            }
            if (((dataGridView != null) && !dataGridView.InSortOperation) && ((((obj2 == null) && (value != null)) || ((obj2 != null) && (value == null))) || ((obj2 != null) && !value.Equals(obj2))))
            {
                base.RaiseCellValueChanged(new DataGridViewCellEventArgs(this.ColumnIndex, rowIndex));
            }
            return true;
        }

        internal bool SetValueInternal(int rowIndex, object value)
        {
            return this.SetValue(rowIndex, value);
        }

        internal static bool TextFitsInBounds(Graphics graphics, string text, Font font, System.Drawing.Size maxBounds, TextFormatFlags flags)
        {
            bool flag;
            return ((MeasureTextHeight(graphics, text, font, maxBounds.Width, flags, out flag) <= maxBounds.Height) && !flag);
        }

        public override string ToString()
        {
            return ("DataGridViewCell { ColumnIndex=" + this.ColumnIndex.ToString(CultureInfo.CurrentCulture) + ", RowIndex=" + this.RowIndex.ToString(CultureInfo.CurrentCulture) + " }");
        }

        private static string TruncateToolTipText(string toolTipText)
        {
            if (toolTipText.Length > 0x120)
            {
                StringBuilder builder = new StringBuilder(toolTipText.Substring(0, 0x100), 0x103);
                builder.Append("...");
                return builder.ToString();
            }
            return toolTipText;
        }

        private void UpdateCurrentMouseLocation(DataGridViewCellMouseEventArgs e)
        {
            if (this.GetErrorIconBounds(e.RowIndex).Contains(e.X, e.Y))
            {
                this.CurrentMouseLocation = 2;
            }
            else
            {
                this.CurrentMouseLocation = 1;
            }
        }

        [Browsable(false)]
        public AccessibleObject AccessibilityObject
        {
            get
            {
                AccessibleObject obj2 = (AccessibleObject) this.Properties.GetObject(PropCellAccessibilityObject);
                if (obj2 == null)
                {
                    obj2 = this.CreateAccessibilityInstance();
                    this.Properties.SetObject(PropCellAccessibilityObject, obj2);
                }
                return obj2;
            }
        }

        public int ColumnIndex
        {
            get
            {
                if (this.owningColumn == null)
                {
                    return -1;
                }
                return this.owningColumn.Index;
            }
        }

        [Browsable(false)]
        public Rectangle ContentBounds
        {
            get
            {
                return this.GetContentBounds(this.RowIndex);
            }
        }

        [DefaultValue((string) null)]
        public virtual System.Windows.Forms.ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return this.GetContextMenuStrip(this.RowIndex);
            }
            set
            {
                this.ContextMenuStripInternal = value;
            }
        }

        private System.Windows.Forms.ContextMenuStrip ContextMenuStripInternal
        {
            get
            {
                return (System.Windows.Forms.ContextMenuStrip) this.Properties.GetObject(PropCellContextMenuStrip);
            }
            set
            {
                System.Windows.Forms.ContextMenuStrip strip = (System.Windows.Forms.ContextMenuStrip) this.Properties.GetObject(PropCellContextMenuStrip);
                if (strip != value)
                {
                    EventHandler handler = new EventHandler(this.DetachContextMenuStrip);
                    if (strip != null)
                    {
                        strip.Disposed -= handler;
                    }
                    this.Properties.SetObject(PropCellContextMenuStrip, value);
                    if (value != null)
                    {
                        value.Disposed += handler;
                    }
                    if (base.DataGridView != null)
                    {
                        base.DataGridView.OnCellContextMenuStripChanged(this);
                    }
                }
            }
        }

        private byte CurrentMouseLocation
        {
            get
            {
                return (byte) (this.flags & 3);
            }
            set
            {
                this.flags = (byte) (this.flags & -4);
                this.flags = (byte) (this.flags | value);
            }
        }

        [Browsable(false)]
        public virtual object DefaultNewRowValue
        {
            get
            {
                return null;
            }
        }

        [Browsable(false)]
        public virtual bool Displayed
        {
            get
            {
                if (base.DataGridView == null)
                {
                    return false;
                }
                if (((base.DataGridView == null) || (this.RowIndex < 0)) || (this.ColumnIndex < 0))
                {
                    return false;
                }
                return (this.owningColumn.Displayed && this.owningRow.Displayed);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public object EditedFormattedValue
        {
            get
            {
                if (base.DataGridView == null)
                {
                    return null;
                }
                DataGridViewCellStyle dataGridViewCellStyle = this.GetInheritedStyle(null, this.RowIndex, false);
                return this.GetEditedFormattedValue(this.GetValue(this.RowIndex), this.RowIndex, ref dataGridViewCellStyle, DataGridViewDataErrorContexts.Formatting);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual System.Type EditType
        {
            get
            {
                return typeof(DataGridViewTextBoxEditingControl);
            }
        }

        private static Bitmap ErrorBitmap
        {
            get
            {
                if (errorBmp == null)
                {
                    errorBmp = GetBitmap("DataGridViewRow.error.bmp");
                }
                return errorBmp;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public Rectangle ErrorIconBounds
        {
            get
            {
                return this.GetErrorIconBounds(this.RowIndex);
            }
        }

        [Browsable(false)]
        public string ErrorText
        {
            get
            {
                return this.GetErrorText(this.RowIndex);
            }
            set
            {
                this.ErrorTextInternal = value;
            }
        }

        private string ErrorTextInternal
        {
            get
            {
                object obj2 = this.Properties.GetObject(PropCellErrorText);
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                string errorTextInternal = this.ErrorTextInternal;
                if (!string.IsNullOrEmpty(value) || this.Properties.ContainsObject(PropCellErrorText))
                {
                    this.Properties.SetObject(PropCellErrorText, value);
                }
                if ((base.DataGridView != null) && !errorTextInternal.Equals(this.ErrorTextInternal))
                {
                    base.DataGridView.OnCellErrorTextChanged(this);
                }
            }
        }

        [Browsable(false)]
        public object FormattedValue
        {
            get
            {
                if (base.DataGridView == null)
                {
                    return null;
                }
                DataGridViewCellStyle cellStyle = this.GetInheritedStyle(null, this.RowIndex, false);
                return this.GetFormattedValue(this.RowIndex, ref cellStyle, DataGridViewDataErrorContexts.Formatting);
            }
        }

        [Browsable(false)]
        public virtual System.Type FormattedValueType
        {
            get
            {
                return this.ValueType;
            }
        }

        private TypeConverter FormattedValueTypeConverter
        {
            get
            {
                TypeConverter converter = null;
                if (this.FormattedValueType == null)
                {
                    return converter;
                }
                if (base.DataGridView != null)
                {
                    return base.DataGridView.GetCachedTypeConverter(this.FormattedValueType);
                }
                return TypeDescriptor.GetConverter(this.FormattedValueType);
            }
        }

        [Browsable(false)]
        public virtual bool Frozen
        {
            get
            {
                if (((base.DataGridView != null) && (this.RowIndex >= 0)) && (this.ColumnIndex >= 0))
                {
                    return (this.owningColumn.Frozen && this.owningRow.Frozen);
                }
                if ((this.owningRow == null) || ((this.owningRow.DataGridView != null) && (this.RowIndex < 0)))
                {
                    return false;
                }
                return this.owningRow.Frozen;
            }
        }

        internal bool HasErrorText
        {
            get
            {
                return (this.Properties.ContainsObject(PropCellErrorText) && (this.Properties.GetObject(PropCellErrorText) != null));
            }
        }

        [Browsable(false)]
        public bool HasStyle
        {
            get
            {
                return (this.Properties.ContainsObject(PropCellStyle) && (this.Properties.GetObject(PropCellStyle) != null));
            }
        }

        internal bool HasToolTipText
        {
            get
            {
                return (this.Properties.ContainsObject(PropCellToolTipText) && (this.Properties.GetObject(PropCellToolTipText) != null));
            }
        }

        internal bool HasValue
        {
            get
            {
                return (this.Properties.ContainsObject(PropCellValue) && (this.Properties.GetObject(PropCellValue) != null));
            }
        }

        internal virtual bool HasValueType
        {
            get
            {
                return (this.Properties.ContainsObject(PropCellValueType) && (this.Properties.GetObject(PropCellValueType) != null));
            }
        }

        [Browsable(false)]
        public DataGridViewElementStates InheritedState
        {
            get
            {
                return this.GetInheritedState(this.RowIndex);
            }
        }

        [Browsable(false)]
        public DataGridViewCellStyle InheritedStyle
        {
            get
            {
                return this.GetInheritedStyleInternal(this.RowIndex);
            }
        }

        [Browsable(false)]
        public bool IsInEditMode
        {
            get
            {
                if (base.DataGridView == null)
                {
                    return false;
                }
                if (this.RowIndex == -1)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidOperationOnSharedCell"));
                }
                Point currentCellAddress = base.DataGridView.CurrentCellAddress;
                return ((((currentCellAddress.X != -1) && (currentCellAddress.X == this.ColumnIndex)) && (currentCellAddress.Y == this.RowIndex)) && base.DataGridView.IsCurrentCellInEditMode);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public DataGridViewColumn OwningColumn
        {
            get
            {
                return this.owningColumn;
            }
        }

        internal DataGridViewColumn OwningColumnInternal
        {
            set
            {
                this.owningColumn = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public DataGridViewRow OwningRow
        {
            get
            {
                return this.owningRow;
            }
        }

        internal DataGridViewRow OwningRowInternal
        {
            set
            {
                this.owningRow = value;
            }
        }

        [Browsable(false)]
        public System.Drawing.Size PreferredSize
        {
            get
            {
                return this.GetPreferredSize(this.RowIndex);
            }
        }

        internal PropertyStore Properties
        {
            get
            {
                return this.propertyStore;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool ReadOnly
        {
            get
            {
                return (((this.State & DataGridViewElementStates.ReadOnly) != DataGridViewElementStates.None) || ((((this.owningRow != null) && ((this.owningRow.DataGridView == null) || (this.RowIndex >= 0))) && this.owningRow.ReadOnly) || ((((base.DataGridView != null) && (this.RowIndex >= 0)) && (this.ColumnIndex >= 0)) && this.owningColumn.ReadOnly)));
            }
            set
            {
                if (base.DataGridView != null)
                {
                    if (this.RowIndex == -1)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidOperationOnSharedCell"));
                    }
                    if ((value != this.ReadOnly) && !base.DataGridView.ReadOnly)
                    {
                        base.DataGridView.OnDataGridViewElementStateChanging(this, -1, DataGridViewElementStates.ReadOnly);
                        base.DataGridView.SetReadOnlyCellCore(this.ColumnIndex, this.RowIndex, value);
                    }
                }
                else if (this.owningRow == null)
                {
                    if (value != this.ReadOnly)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCell_CannotSetReadOnlyState"));
                    }
                }
                else
                {
                    this.owningRow.SetReadOnlyCellCore(this, value);
                }
            }
        }

        internal bool ReadOnlyInternal
        {
            set
            {
                if (value)
                {
                    base.StateInternal = this.State | DataGridViewElementStates.ReadOnly;
                }
                else
                {
                    base.StateInternal = this.State & ~DataGridViewElementStates.ReadOnly;
                }
                if (base.DataGridView != null)
                {
                    base.DataGridView.OnDataGridViewElementStateChanged(this, -1, DataGridViewElementStates.ReadOnly);
                }
            }
        }

        [Browsable(false)]
        public virtual bool Resizable
        {
            get
            {
                return ((((this.owningRow != null) && ((this.owningRow.DataGridView == null) || (this.RowIndex >= 0))) && (this.owningRow.Resizable == DataGridViewTriState.True)) || ((((base.DataGridView != null) && (this.RowIndex >= 0)) && (this.ColumnIndex >= 0)) && (this.owningColumn.Resizable == DataGridViewTriState.True)));
            }
        }

        [Browsable(false)]
        public int RowIndex
        {
            get
            {
                if (this.owningRow == null)
                {
                    return -1;
                }
                return this.owningRow.Index;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool Selected
        {
            get
            {
                return (((this.State & DataGridViewElementStates.Selected) != DataGridViewElementStates.None) || ((((this.owningRow != null) && ((this.owningRow.DataGridView == null) || (this.RowIndex >= 0))) && this.owningRow.Selected) || ((((base.DataGridView != null) && (this.RowIndex >= 0)) && (this.ColumnIndex >= 0)) && this.owningColumn.Selected)));
            }
            set
            {
                if (base.DataGridView != null)
                {
                    if (this.RowIndex == -1)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidOperationOnSharedCell"));
                    }
                    base.DataGridView.SetSelectedCellCoreInternal(this.ColumnIndex, this.RowIndex, value);
                }
                else if (value)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCell_CannotSetSelectedState"));
                }
            }
        }

        internal bool SelectedInternal
        {
            set
            {
                if (value)
                {
                    base.StateInternal = this.State | DataGridViewElementStates.Selected;
                }
                else
                {
                    base.StateInternal = this.State & ~DataGridViewElementStates.Selected;
                }
                if (base.DataGridView != null)
                {
                    base.DataGridView.OnDataGridViewElementStateChanged(this, -1, DataGridViewElementStates.Selected);
                }
            }
        }

        [Browsable(false)]
        public System.Drawing.Size Size
        {
            get
            {
                return this.GetSize(this.RowIndex);
            }
        }

        internal Rectangle StdBorderWidths
        {
            get
            {
                if (base.DataGridView != null)
                {
                    DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder = new DataGridViewAdvancedBorderStyle();
                    DataGridViewAdvancedBorderStyle advancedBorderStyle = this.AdjustCellBorderStyle(base.DataGridView.AdvancedCellBorderStyle, dataGridViewAdvancedBorderStylePlaceholder, false, false, false, false);
                    return this.BorderWidths(advancedBorderStyle);
                }
                return Rectangle.Empty;
            }
        }

        [Browsable(true)]
        public DataGridViewCellStyle Style
        {
            get
            {
                DataGridViewCellStyle style = (DataGridViewCellStyle) this.Properties.GetObject(PropCellStyle);
                if (style == null)
                {
                    style = new DataGridViewCellStyle();
                    style.AddScope(base.DataGridView, DataGridViewCellStyleScopes.Cell);
                    this.Properties.SetObject(PropCellStyle, style);
                }
                return style;
            }
            set
            {
                DataGridViewCellStyle style = null;
                if (this.HasStyle)
                {
                    style = this.Style;
                    style.RemoveScope(DataGridViewCellStyleScopes.Cell);
                }
                if ((value != null) || this.Properties.ContainsObject(PropCellStyle))
                {
                    if (value != null)
                    {
                        value.AddScope(base.DataGridView, DataGridViewCellStyleScopes.Cell);
                    }
                    this.Properties.SetObject(PropCellStyle, value);
                }
                if (((((style != null) && (value == null)) || ((style == null) && (value != null))) || (((style != null) && (value != null)) && !style.Equals(this.Style))) && (base.DataGridView != null))
                {
                    base.DataGridView.OnCellStyleChanged(this);
                }
            }
        }

        [System.Windows.Forms.SRDescription("ControlTagDescr"), System.Windows.Forms.SRCategory("CatData"), Localizable(false), Bindable(true), DefaultValue((string) null), TypeConverter(typeof(StringConverter))]
        public object Tag
        {
            get
            {
                return this.Properties.GetObject(PropCellTag);
            }
            set
            {
                if ((value != null) || this.Properties.ContainsObject(PropCellTag))
                {
                    this.Properties.SetObject(PropCellTag, value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string ToolTipText
        {
            get
            {
                return this.GetToolTipText(this.RowIndex);
            }
            set
            {
                this.ToolTipTextInternal = value;
            }
        }

        private string ToolTipTextInternal
        {
            get
            {
                object obj2 = this.Properties.GetObject(PropCellToolTipText);
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                string toolTipTextInternal = this.ToolTipTextInternal;
                if (!string.IsNullOrEmpty(value) || this.Properties.ContainsObject(PropCellToolTipText))
                {
                    this.Properties.SetObject(PropCellToolTipText, value);
                }
                if ((base.DataGridView != null) && !toolTipTextInternal.Equals(this.ToolTipTextInternal))
                {
                    base.DataGridView.OnCellToolTipTextChanged(this);
                }
            }
        }

        [Browsable(false)]
        public object Value
        {
            get
            {
                return this.GetValue(this.RowIndex);
            }
            set
            {
                this.SetValue(this.RowIndex, value);
            }
        }

        [Browsable(false)]
        public virtual System.Type ValueType
        {
            get
            {
                System.Type valueType = (System.Type) this.Properties.GetObject(PropCellValueType);
                if ((valueType == null) && (this.OwningColumn != null))
                {
                    valueType = this.OwningColumn.ValueType;
                }
                return valueType;
            }
            set
            {
                if ((value != null) || this.Properties.ContainsObject(PropCellValueType))
                {
                    this.Properties.SetObject(PropCellValueType, value);
                }
            }
        }

        private TypeConverter ValueTypeConverter
        {
            get
            {
                TypeConverter boundColumnConverter = null;
                if (this.OwningColumn != null)
                {
                    boundColumnConverter = this.OwningColumn.BoundColumnConverter;
                }
                if ((boundColumnConverter != null) || (this.ValueType == null))
                {
                    return boundColumnConverter;
                }
                if (base.DataGridView != null)
                {
                    return base.DataGridView.GetCachedTypeConverter(this.ValueType);
                }
                return TypeDescriptor.GetConverter(this.ValueType);
            }
        }

        [Browsable(false)]
        public virtual bool Visible
        {
            get
            {
                if (((base.DataGridView != null) && (this.RowIndex >= 0)) && (this.ColumnIndex >= 0))
                {
                    return (this.owningColumn.Visible && this.owningRow.Visible);
                }
                if ((this.owningRow == null) || ((this.owningRow.DataGridView != null) && (this.RowIndex < 0)))
                {
                    return false;
                }
                return this.owningRow.Visible;
            }
        }

        [ComVisible(true)]
        protected class DataGridViewCellAccessibleObject : AccessibleObject
        {
            private DataGridViewCell owner;

            public DataGridViewCellAccessibleObject()
            {
            }

            public DataGridViewCellAccessibleObject(DataGridViewCell owner)
            {
                this.owner = owner;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                if (this.owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellAccessibleObject_OwnerNotSet"));
                }
                DataGridViewCell owner = this.Owner;
                DataGridView dataGridView = owner.DataGridView;
                if (!(owner is DataGridViewHeaderCell))
                {
                    if ((dataGridView != null) && (owner.RowIndex == -1))
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidOperationOnSharedCell"));
                    }
                    this.Select(AccessibleSelection.TakeSelection | AccessibleSelection.TakeFocus);
                    if ((!owner.ReadOnly && (owner.EditType != null)) && (!dataGridView.InBeginEdit && !dataGridView.InEndEdit))
                    {
                        if (dataGridView.IsCurrentCellInEditMode)
                        {
                            dataGridView.EndEdit();
                        }
                        else if (dataGridView.EditMode != DataGridViewEditMode.EditProgrammatically)
                        {
                            dataGridView.BeginEdit(true);
                        }
                    }
                }
            }

            internal Rectangle GetAccessibleObjectBounds(AccessibleObject parentAccObject)
            {
                Rectangle columnDisplayRectangle;
                if (this.owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellAccessibleObject_OwnerNotSet"));
                }
                if (this.owner.OwningColumn == null)
                {
                    return Rectangle.Empty;
                }
                Rectangle bounds = parentAccObject.Bounds;
                int num = this.owner.DataGridView.Columns.ColumnIndexToActualDisplayIndex(this.owner.DataGridView.FirstDisplayedScrollingColumnIndex, DataGridViewElementStates.Visible);
                int num2 = this.owner.DataGridView.Columns.ColumnIndexToActualDisplayIndex(this.owner.ColumnIndex, DataGridViewElementStates.Visible);
                bool rowHeadersVisible = this.owner.DataGridView.RowHeadersVisible;
                if (num2 < num)
                {
                    columnDisplayRectangle = parentAccObject.GetChild((num2 + 1) + (rowHeadersVisible ? 1 : 0)).Bounds;
                    if (this.Owner.DataGridView.RightToLeft == RightToLeft.No)
                    {
                        columnDisplayRectangle.X -= this.owner.OwningColumn.Width;
                    }
                    else
                    {
                        columnDisplayRectangle.X = columnDisplayRectangle.Right;
                    }
                    columnDisplayRectangle.Width = this.owner.OwningColumn.Width;
                }
                else if (num2 == num)
                {
                    columnDisplayRectangle = this.owner.DataGridView.GetColumnDisplayRectangle(this.owner.ColumnIndex, false);
                    int firstDisplayedScrollingColumnHiddenWidth = this.owner.DataGridView.FirstDisplayedScrollingColumnHiddenWidth;
                    if (firstDisplayedScrollingColumnHiddenWidth != 0)
                    {
                        if (this.owner.DataGridView.RightToLeft == RightToLeft.No)
                        {
                            columnDisplayRectangle.X -= firstDisplayedScrollingColumnHiddenWidth;
                        }
                        columnDisplayRectangle.Width += firstDisplayedScrollingColumnHiddenWidth;
                    }
                    columnDisplayRectangle = this.owner.DataGridView.RectangleToScreen(columnDisplayRectangle);
                }
                else
                {
                    columnDisplayRectangle = parentAccObject.GetChild((num2 - 1) + (rowHeadersVisible ? 1 : 0)).Bounds;
                    if (this.owner.DataGridView.RightToLeft == RightToLeft.No)
                    {
                        columnDisplayRectangle.X = columnDisplayRectangle.Right;
                    }
                    else
                    {
                        columnDisplayRectangle.X -= this.owner.OwningColumn.Width;
                    }
                    columnDisplayRectangle.Width = this.owner.OwningColumn.Width;
                }
                bounds.X = columnDisplayRectangle.X;
                bounds.Width = columnDisplayRectangle.Width;
                return bounds;
            }

            private AccessibleObject GetAccessibleObjectParent()
            {
                if ((((this.owner is DataGridViewButtonCell) || (this.owner is DataGridViewCheckBoxCell)) || ((this.owner is DataGridViewComboBoxCell) || (this.owner is DataGridViewImageCell))) || ((this.owner is DataGridViewLinkCell) || (this.owner is DataGridViewTextBoxCell)))
                {
                    return this.ParentPrivate;
                }
                return this.Parent;
            }

            public override AccessibleObject GetChild(int index)
            {
                if (this.owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellAccessibleObject_OwnerNotSet"));
                }
                if (((this.owner.DataGridView.EditingControl != null) && this.owner.DataGridView.IsCurrentCellInEditMode) && ((this.owner.DataGridView.CurrentCell == this.owner) && (index == 0)))
                {
                    return this.owner.DataGridView.EditingControl.AccessibilityObject;
                }
                return null;
            }

            public override int GetChildCount()
            {
                if (this.owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellAccessibleObject_OwnerNotSet"));
                }
                if (((this.owner.DataGridView.EditingControl != null) && this.owner.DataGridView.IsCurrentCellInEditMode) && (this.owner.DataGridView.CurrentCell == this.owner))
                {
                    return 1;
                }
                return 0;
            }

            public override AccessibleObject GetFocused()
            {
                return null;
            }

            public override AccessibleObject GetSelected()
            {
                return null;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation navigationDirection)
            {
                if (this.owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellAccessibleObject_OwnerNotSet"));
                }
                if ((this.owner.OwningColumn != null) && (this.owner.OwningRow != null))
                {
                    switch (navigationDirection)
                    {
                        case AccessibleNavigation.Up:
                            if (this.owner.OwningRow.Index != this.owner.DataGridView.Rows.GetFirstRow(DataGridViewElementStates.Visible))
                            {
                                int previousRow = this.owner.DataGridView.Rows.GetPreviousRow(this.owner.OwningRow.Index, DataGridViewElementStates.Visible);
                                return this.owner.DataGridView.Rows[previousRow].Cells[this.owner.OwningColumn.Index].AccessibilityObject;
                            }
                            if (!this.owner.DataGridView.ColumnHeadersVisible)
                            {
                                return null;
                            }
                            return this.owner.OwningColumn.HeaderCell.AccessibilityObject;

                        case AccessibleNavigation.Down:
                            if (this.owner.OwningRow.Index != this.owner.DataGridView.Rows.GetLastRow(DataGridViewElementStates.Visible))
                            {
                                int nextRow = this.owner.DataGridView.Rows.GetNextRow(this.owner.OwningRow.Index, DataGridViewElementStates.Visible);
                                return this.owner.DataGridView.Rows[nextRow].Cells[this.owner.OwningColumn.Index].AccessibilityObject;
                            }
                            return null;

                        case AccessibleNavigation.Left:
                            if (this.owner.DataGridView.RightToLeft != RightToLeft.No)
                            {
                                return this.NavigateForward(true);
                            }
                            return this.NavigateBackward(true);

                        case AccessibleNavigation.Right:
                            if (this.owner.DataGridView.RightToLeft != RightToLeft.No)
                            {
                                return this.NavigateBackward(true);
                            }
                            return this.NavigateForward(true);

                        case AccessibleNavigation.Next:
                            return this.NavigateForward(false);

                        case AccessibleNavigation.Previous:
                            return this.NavigateBackward(false);
                    }
                }
                return null;
            }

            private AccessibleObject NavigateBackward(bool wrapAround)
            {
                if (this.owner.OwningColumn == this.owner.DataGridView.Columns.GetFirstColumn(DataGridViewElementStates.Visible))
                {
                    if (wrapAround)
                    {
                        AccessibleObject obj2 = this.Owner.OwningRow.AccessibilityObject.Navigate(AccessibleNavigation.Previous);
                        if ((obj2 != null) && (obj2.GetChildCount() > 0))
                        {
                            return obj2.GetChild(obj2.GetChildCount() - 1);
                        }
                        return null;
                    }
                    if (this.owner.DataGridView.RowHeadersVisible)
                    {
                        return this.owner.OwningRow.AccessibilityObject.GetChild(0);
                    }
                    return null;
                }
                int index = this.owner.DataGridView.Columns.GetPreviousColumn(this.owner.OwningColumn, DataGridViewElementStates.Visible, DataGridViewElementStates.None).Index;
                return this.owner.OwningRow.Cells[index].AccessibilityObject;
            }

            private AccessibleObject NavigateForward(bool wrapAround)
            {
                if (this.owner.OwningColumn == this.owner.DataGridView.Columns.GetLastColumn(DataGridViewElementStates.Visible, DataGridViewElementStates.None))
                {
                    if (!wrapAround)
                    {
                        return null;
                    }
                    AccessibleObject obj2 = this.Owner.OwningRow.AccessibilityObject.Navigate(AccessibleNavigation.Next);
                    if ((obj2 == null) || (obj2.GetChildCount() <= 0))
                    {
                        return null;
                    }
                    if (this.Owner.DataGridView.RowHeadersVisible)
                    {
                        return obj2.GetChild(1);
                    }
                    return obj2.GetChild(0);
                }
                int index = this.owner.DataGridView.Columns.GetNextColumn(this.owner.OwningColumn, DataGridViewElementStates.Visible, DataGridViewElementStates.None).Index;
                return this.owner.OwningRow.Cells[index].AccessibilityObject;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void Select(AccessibleSelection flags)
            {
                if (this.owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellAccessibleObject_OwnerNotSet"));
                }
                if ((flags & AccessibleSelection.TakeFocus) == AccessibleSelection.TakeFocus)
                {
                    this.owner.DataGridView.FocusInternal();
                }
                if ((flags & AccessibleSelection.TakeSelection) == AccessibleSelection.TakeSelection)
                {
                    this.owner.Selected = true;
                    this.owner.DataGridView.CurrentCell = this.owner;
                }
                if ((flags & AccessibleSelection.AddSelection) == AccessibleSelection.AddSelection)
                {
                    this.owner.Selected = true;
                }
                if (((flags & AccessibleSelection.RemoveSelection) == AccessibleSelection.RemoveSelection) && ((flags & (AccessibleSelection.AddSelection | AccessibleSelection.TakeSelection)) == AccessibleSelection.None))
                {
                    this.owner.Selected = false;
                }
            }

            public override Rectangle Bounds
            {
                get
                {
                    return this.GetAccessibleObjectBounds(this.GetAccessibleObjectParent());
                }
            }

            public override string DefaultAction
            {
                get
                {
                    if (this.Owner == null)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellAccessibleObject_OwnerNotSet"));
                    }
                    if (!this.Owner.ReadOnly)
                    {
                        return System.Windows.Forms.SR.GetString("DataGridView_AccCellDefaultAction");
                    }
                    return string.Empty;
                }
            }

            public override string Help
            {
                get
                {
                    return (this.owner.GetType().Name + "(" + this.owner.GetType().BaseType.Name + ")");
                }
            }

            public override string Name
            {
                get
                {
                    if (this.owner == null)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellAccessibleObject_OwnerNotSet"));
                    }
                    if (this.owner.OwningColumn != null)
                    {
                        return System.Windows.Forms.SR.GetString("DataGridView_AccDataGridViewCellName", new object[] { this.owner.OwningColumn.HeaderText, this.owner.OwningRow.Index });
                    }
                    return string.Empty;
                }
            }

            public DataGridViewCell Owner
            {
                get
                {
                    return this.owner;
                }
                set
                {
                    if (this.owner != null)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellAccessibleObject_OwnerAlreadySet"));
                    }
                    this.owner = value;
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
                    if (this.owner == null)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellAccessibleObject_OwnerNotSet"));
                    }
                    if (this.owner.OwningRow == null)
                    {
                        return null;
                    }
                    return this.owner.OwningRow.AccessibilityObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.Cell;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    Rectangle rectangle;
                    if (this.owner == null)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellAccessibleObject_OwnerNotSet"));
                    }
                    AccessibleStates states = AccessibleStates.Selectable | AccessibleStates.Focusable;
                    if (this.owner == this.owner.DataGridView.CurrentCell)
                    {
                        states |= AccessibleStates.Focused;
                    }
                    if (this.owner.Selected)
                    {
                        states |= AccessibleStates.Selected;
                    }
                    if ((this.owner.OwningColumn != null) && (this.owner.OwningRow != null))
                    {
                        rectangle = this.owner.DataGridView.GetCellDisplayRectangle(this.owner.OwningColumn.Index, this.owner.OwningRow.Index, false);
                    }
                    else if (this.owner.OwningRow != null)
                    {
                        rectangle = this.owner.DataGridView.GetCellDisplayRectangle(-1, this.owner.OwningRow.Index, false);
                    }
                    else if (this.owner.OwningColumn != null)
                    {
                        rectangle = this.owner.DataGridView.GetCellDisplayRectangle(this.owner.OwningColumn.Index, -1, false);
                    }
                    else
                    {
                        rectangle = this.owner.DataGridView.GetCellDisplayRectangle(-1, -1, false);
                    }
                    if (!rectangle.IntersectsWith(this.owner.DataGridView.ClientRectangle))
                    {
                        states |= AccessibleStates.Offscreen;
                    }
                    return states;
                }
            }

            public override string Value
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    if (this.owner == null)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewCellAccessibleObject_OwnerNotSet"));
                    }
                    object formattedValue = this.owner.FormattedValue;
                    string str = formattedValue as string;
                    if ((formattedValue == null) || ((str != null) && string.IsNullOrEmpty(str)))
                    {
                        return System.Windows.Forms.SR.GetString("DataGridView_AccNullValue");
                    }
                    if (str != null)
                    {
                        return str;
                    }
                    if (this.owner.OwningColumn == null)
                    {
                        return string.Empty;
                    }
                    TypeConverter formattedValueTypeConverter = this.owner.FormattedValueTypeConverter;
                    if ((formattedValueTypeConverter != null) && formattedValueTypeConverter.CanConvertTo(typeof(string)))
                    {
                        return formattedValueTypeConverter.ConvertToString(formattedValue);
                    }
                    return formattedValue.ToString();
                }
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                set
                {
                    if ((!(this.owner is DataGridViewHeaderCell) && !this.owner.ReadOnly) && (this.owner.OwningRow != null))
                    {
                        if (this.owner.DataGridView.IsCurrentCellInEditMode)
                        {
                            this.owner.DataGridView.EndEdit();
                        }
                        DataGridViewCellStyle inheritedStyle = this.owner.InheritedStyle;
                        object formattedValue = this.owner.GetFormattedValue(value, this.owner.OwningRow.Index, ref inheritedStyle, null, null, DataGridViewDataErrorContexts.Formatting);
                        this.owner.Value = this.owner.ParseFormattedValue(formattedValue, inheritedStyle, null, null);
                    }
                }
            }
        }
    }
}

