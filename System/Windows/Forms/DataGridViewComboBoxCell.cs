namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms.VisualStyles;

    public class DataGridViewComboBoxCell : DataGridViewCell
    {
        private static int cachedDropDownWidth = -1;
        private static System.Type cellType = typeof(DataGridViewComboBoxCell);
        private const byte DATAGRIDVIEWCOMBOBOXCELL_autoComplete = 8;
        private const byte DATAGRIDVIEWCOMBOBOXCELL_createItemsFromDataSource = 4;
        private const byte DATAGRIDVIEWCOMBOBOXCELL_dataSourceInitializedHookedUp = 0x10;
        internal const int DATAGRIDVIEWCOMBOBOXCELL_defaultMaxDropDownItems = 8;
        private const byte DATAGRIDVIEWCOMBOBOXCELL_dropDownHookedUp = 0x20;
        private const byte DATAGRIDVIEWCOMBOBOXCELL_horizontalTextMarginLeft = 0;
        private const byte DATAGRIDVIEWCOMBOBOXCELL_ignoreNextMouseClick = 1;
        private const byte DATAGRIDVIEWCOMBOBOXCELL_margin = 3;
        private const byte DATAGRIDVIEWCOMBOBOXCELL_nonXPTriangleHeight = 4;
        private const byte DATAGRIDVIEWCOMBOBOXCELL_nonXPTriangleWidth = 7;
        private const byte DATAGRIDVIEWCOMBOBOXCELL_sorted = 2;
        private const byte DATAGRIDVIEWCOMBOBOXCELL_verticalTextMarginTopWithoutWrapping = 1;
        private const byte DATAGRIDVIEWCOMBOBOXCELL_verticalTextMarginTopWithWrapping = 0;
        private static System.Type defaultEditType = typeof(DataGridViewComboBoxEditingControl);
        private static System.Type defaultFormattedValueType = typeof(string);
        private static System.Type defaultValueType = typeof(object);
        private byte flags = 8;
        private static bool mouseInDropDownButtonBounds = false;
        private static readonly int PropComboBoxCellColumnTemplate = PropertyStore.CreateKey();
        private static readonly int PropComboBoxCellDataManager = PropertyStore.CreateKey();
        private static readonly int PropComboBoxCellDataSource = PropertyStore.CreateKey();
        private static readonly int PropComboBoxCellDisplayMember = PropertyStore.CreateKey();
        private static readonly int PropComboBoxCellDisplayMemberProp = PropertyStore.CreateKey();
        private static readonly int PropComboBoxCellDisplayStyle = PropertyStore.CreateKey();
        private static readonly int PropComboBoxCellDisplayStyleForCurrentCellOnly = PropertyStore.CreateKey();
        private static readonly int PropComboBoxCellDropDownWidth = PropertyStore.CreateKey();
        private static readonly int PropComboBoxCellEditingComboBox = PropertyStore.CreateKey();
        private static readonly int PropComboBoxCellFlatStyle = PropertyStore.CreateKey();
        private static readonly int PropComboBoxCellItems = PropertyStore.CreateKey();
        private static readonly int PropComboBoxCellMaxDropDownItems = PropertyStore.CreateKey();
        private static readonly int PropComboBoxCellValueMember = PropertyStore.CreateKey();
        private static readonly int PropComboBoxCellValueMemberProp = PropertyStore.CreateKey();

        internal override void CacheEditingControl()
        {
            this.EditingComboBox = base.DataGridView.EditingControl as DataGridViewComboBoxEditingControl;
        }

        private void CheckDropDownList(int x, int y, int rowIndex)
        {
            int num;
            DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder = new DataGridViewAdvancedBorderStyle();
            DataGridViewAdvancedBorderStyle advancedBorderStyle = this.AdjustCellBorderStyle(base.DataGridView.AdvancedCellBorderStyle, dataGridViewAdvancedBorderStylePlaceholder, false, false, false, false);
            DataGridViewCellStyle cellStyle = this.GetInheritedStyle(null, rowIndex, false);
            Rectangle rectangle = this.BorderWidths(advancedBorderStyle);
            rectangle.X += cellStyle.Padding.Left;
            rectangle.Y += cellStyle.Padding.Top;
            rectangle.Width += cellStyle.Padding.Right;
            rectangle.Height += cellStyle.Padding.Bottom;
            Size size = this.GetSize(rowIndex);
            Size size2 = new Size((size.Width - rectangle.X) - rectangle.Width, (size.Height - rectangle.Y) - rectangle.Height);
            using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
            {
                num = Math.Min(this.GetDropDownButtonHeight(graphics, cellStyle), size2.Height - 2);
            }
            int num2 = Math.Min(SystemInformation.HorizontalScrollBarThumbWidth, (size2.Width - 6) - 1);
            if (((num > 0) && (num2 > 0)) && ((y >= (rectangle.Y + 1)) && (y <= ((rectangle.Y + 1) + num))))
            {
                if (base.DataGridView.RightToLeftInternal)
                {
                    if ((x >= (rectangle.X + 1)) && (x <= ((rectangle.X + num2) + 1)))
                    {
                        this.EditingComboBox.DroppedDown = true;
                    }
                }
                else if ((x >= (((size.Width - rectangle.Width) - num2) - 1)) && (x <= ((size.Width - rectangle.Width) - 1)))
                {
                    this.EditingComboBox.DroppedDown = true;
                }
            }
        }

        private void CheckNoDataSource()
        {
            if (this.DataSource != null)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataSourceLocksItems"));
            }
        }

        public override object Clone()
        {
            DataGridViewComboBoxCell cell;
            System.Type type = base.GetType();
            if (type == cellType)
            {
                cell = new DataGridViewComboBoxCell();
            }
            else
            {
                cell = (DataGridViewComboBoxCell) Activator.CreateInstance(type);
            }
            base.CloneInternal(cell);
            cell.DropDownWidth = this.DropDownWidth;
            cell.MaxDropDownItems = this.MaxDropDownItems;
            cell.CreateItemsFromDataSource = false;
            cell.DataSource = this.DataSource;
            cell.DisplayMember = this.DisplayMember;
            cell.ValueMember = this.ValueMember;
            if ((this.HasItems && (this.DataSource == null)) && (this.Items.Count > 0))
            {
                cell.Items.AddRangeInternal(this.Items.InnerArray.ToArray());
            }
            cell.AutoComplete = this.AutoComplete;
            cell.Sorted = this.Sorted;
            cell.FlatStyleInternal = this.FlatStyle;
            cell.DisplayStyleInternal = this.DisplayStyle;
            cell.DisplayStyleForCurrentCellOnlyInternal = this.DisplayStyleForCurrentCellOnly;
            return cell;
        }

        private void ComboBox_DropDown(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox editingComboBox = this.EditingComboBox;
            DataGridViewComboBoxColumn owningColumn = base.OwningColumn as DataGridViewComboBoxColumn;
            if (owningColumn != null)
            {
                switch (owningColumn.GetInheritedAutoSizeMode(base.DataGridView))
                {
                    case DataGridViewAutoSizeColumnMode.ColumnHeader:
                    case DataGridViewAutoSizeColumnMode.Fill:
                    case DataGridViewAutoSizeColumnMode.None:
                    {
                        int num2 = (int) ((long) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(editingComboBox, editingComboBox.Handle), 0x15f, 0, 0));
                        if (num2 != this.DropDownWidth)
                        {
                            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(editingComboBox, editingComboBox.Handle), 0x160, this.DropDownWidth, 0);
                        }
                        break;
                    }
                    default:
                        if (this.DropDownWidth != 1)
                        {
                            break;
                        }
                        if (cachedDropDownWidth == -1)
                        {
                            int width = -1;
                            if ((this.HasItems || this.CreateItemsFromDataSource) && (this.Items.Count > 0))
                            {
                                foreach (object obj2 in this.Items)
                                {
                                    Size size = TextRenderer.MeasureText(editingComboBox.GetItemText(obj2), editingComboBox.Font);
                                    if (size.Width > width)
                                    {
                                        width = size.Width;
                                    }
                                }
                            }
                            cachedDropDownWidth = (width + 2) + SystemInformation.VerticalScrollBarWidth;
                        }
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(editingComboBox, editingComboBox.Handle), 0x160, cachedDropDownWidth, 0);
                        return;
                }
            }
        }

        private void DataSource_Disposed(object sender, EventArgs e)
        {
            this.DataSource = null;
        }

        private void DataSource_Initialized(object sender, EventArgs e)
        {
            ISupportInitializeNotification dataSource = this.DataSource as ISupportInitializeNotification;
            if (dataSource != null)
            {
                dataSource.Initialized -= new EventHandler(this.DataSource_Initialized);
            }
            this.flags = (byte) (this.flags & -17);
            this.InitializeDisplayMemberPropertyDescriptor(this.DisplayMember);
            this.InitializeValueMemberPropertyDescriptor(this.ValueMember);
        }

        public override void DetachEditingControl()
        {
            DataGridView dataGridView = base.DataGridView;
            if ((dataGridView == null) || (dataGridView.EditingControl == null))
            {
                throw new InvalidOperationException();
            }
            if ((this.EditingComboBox != null) && ((this.flags & 0x20) != 0))
            {
                this.EditingComboBox.DropDown -= new EventHandler(this.ComboBox_DropDown);
                this.flags = (byte) (this.flags & -33);
            }
            this.EditingComboBox = null;
            base.DetachEditingControl();
        }

        protected override Rectangle GetContentBounds(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
        {
            DataGridViewAdvancedBorderStyle style;
            DataGridViewElementStates states;
            Rectangle rectangle;
            Rectangle rectangle2;
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            if (((base.DataGridView == null) || (rowIndex < 0)) || (base.OwningColumn == null))
            {
                return Rectangle.Empty;
            }
            object obj2 = this.GetValue(rowIndex);
            object formattedValue = base.GetEditedFormattedValue(obj2, rowIndex, ref cellStyle, DataGridViewDataErrorContexts.Formatting);
            base.ComputeBorderStyleCellStateAndCellBounds(rowIndex, out style, out states, out rectangle);
            return this.PaintPrivate(graphics, rectangle, rectangle, rowIndex, states, formattedValue, null, cellStyle, style, out rectangle2, DataGridViewPaintParts.ContentForeground, true, false, false, false);
        }

        private CurrencyManager GetDataManager(DataGridView dataGridView)
        {
            CurrencyManager manager = (CurrencyManager) base.Properties.GetObject(PropComboBoxCellDataManager);
            if ((((manager == null) && (this.DataSource != null)) && ((dataGridView != null) && (dataGridView.BindingContext != null))) && (this.DataSource != Convert.DBNull))
            {
                ISupportInitializeNotification dataSource = this.DataSource as ISupportInitializeNotification;
                if ((dataSource != null) && !dataSource.IsInitialized)
                {
                    if ((this.flags & 0x10) == 0)
                    {
                        dataSource.Initialized += new EventHandler(this.DataSource_Initialized);
                        this.flags = (byte) (this.flags | 0x10);
                    }
                    return manager;
                }
                manager = (CurrencyManager) dataGridView.BindingContext[this.DataSource];
                this.DataManager = manager;
            }
            return manager;
        }

        private int GetDropDownButtonHeight(Graphics graphics, DataGridViewCellStyle cellStyle)
        {
            int num = 4;
            if (this.PaintXPThemes)
            {
                if (PostXPThemesExist)
                {
                    num = 8;
                }
                else
                {
                    num = 6;
                }
            }
            return (DataGridViewCell.MeasureTextHeight(graphics, " ", cellStyle.Font, 0x7fffffff, TextFormatFlags.Default) + num);
        }

        protected override Rectangle GetErrorIconBounds(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
        {
            DataGridViewAdvancedBorderStyle style;
            DataGridViewElementStates states;
            Rectangle rectangle;
            Rectangle rectangle2;
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            if (((base.DataGridView == null) || (rowIndex < 0)) || (((base.OwningColumn == null) || !base.DataGridView.ShowCellErrors) || string.IsNullOrEmpty(this.GetErrorText(rowIndex))))
            {
                return Rectangle.Empty;
            }
            object obj2 = this.GetValue(rowIndex);
            object formattedValue = base.GetEditedFormattedValue(obj2, rowIndex, ref cellStyle, DataGridViewDataErrorContexts.Formatting);
            base.ComputeBorderStyleCellStateAndCellBounds(rowIndex, out style, out states, out rectangle);
            return this.PaintPrivate(graphics, rectangle, rectangle, rowIndex, states, formattedValue, this.GetErrorText(rowIndex), cellStyle, style, out rectangle2, DataGridViewPaintParts.ContentForeground, false, true, false, false);
        }

        protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
        {
            if (valueTypeConverter == null)
            {
                if (this.ValueMemberProperty != null)
                {
                    valueTypeConverter = this.ValueMemberProperty.Converter;
                }
                else if (this.DisplayMemberProperty != null)
                {
                    valueTypeConverter = this.DisplayMemberProperty.Converter;
                }
            }
            if ((value == null) || (((this.ValueType != null) && !this.ValueType.IsAssignableFrom(value.GetType())) && (value != DBNull.Value)))
            {
                if (value == null)
                {
                    return base.GetFormattedValue(null, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
                }
                if (base.DataGridView != null)
                {
                    DataGridViewDataErrorEventArgs e = new DataGridViewDataErrorEventArgs(new FormatException(System.Windows.Forms.SR.GetString("DataGridViewComboBoxCell_InvalidValue")), base.ColumnIndex, rowIndex, context);
                    base.RaiseDataError(e);
                    if (e.ThrowException)
                    {
                        throw e.Exception;
                    }
                }
                return base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
            }
            string str = value as string;
            if (((this.DataManager != null) && ((this.ValueMemberProperty != null) || (this.DisplayMemberProperty != null))) || (!string.IsNullOrEmpty(this.ValueMember) || !string.IsNullOrEmpty(this.DisplayMember)))
            {
                object obj2;
                if (!this.LookupDisplayValue(rowIndex, value, out obj2))
                {
                    if (value == DBNull.Value)
                    {
                        obj2 = DBNull.Value;
                    }
                    else if (((str != null) && string.IsNullOrEmpty(str)) && (this.DisplayType == typeof(string)))
                    {
                        obj2 = string.Empty;
                    }
                    else if (base.DataGridView != null)
                    {
                        DataGridViewDataErrorEventArgs args2 = new DataGridViewDataErrorEventArgs(new ArgumentException(System.Windows.Forms.SR.GetString("DataGridViewComboBoxCell_InvalidValue")), base.ColumnIndex, rowIndex, context);
                        base.RaiseDataError(args2);
                        if (args2.ThrowException)
                        {
                            throw args2.Exception;
                        }
                        if (this.OwnsEditingComboBox(rowIndex))
                        {
                            this.EditingComboBox.EditingControlValueChanged = true;
                            base.DataGridView.NotifyCurrentCellDirty(true);
                        }
                    }
                }
                return base.GetFormattedValue(obj2, rowIndex, ref cellStyle, this.DisplayTypeConverter, formattedValueTypeConverter, context);
            }
            if ((!this.Items.Contains(value) && (value != DBNull.Value)) && (!(value is string) || !string.IsNullOrEmpty(str)))
            {
                if (base.DataGridView != null)
                {
                    DataGridViewDataErrorEventArgs args3 = new DataGridViewDataErrorEventArgs(new ArgumentException(System.Windows.Forms.SR.GetString("DataGridViewComboBoxCell_InvalidValue")), base.ColumnIndex, rowIndex, context);
                    base.RaiseDataError(args3);
                    if (args3.ThrowException)
                    {
                        throw args3.Exception;
                    }
                }
                if (this.Items.Count > 0)
                {
                    value = this.Items[0];
                }
                else
                {
                    value = string.Empty;
                }
            }
            return base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
        }

        internal string GetItemDisplayText(object item)
        {
            object itemDisplayValue = this.GetItemDisplayValue(item);
            if (itemDisplayValue == null)
            {
                return string.Empty;
            }
            return Convert.ToString(itemDisplayValue, CultureInfo.CurrentCulture);
        }

        internal object GetItemDisplayValue(object item)
        {
            bool flag = false;
            object obj2 = null;
            if (this.DisplayMemberProperty != null)
            {
                obj2 = this.DisplayMemberProperty.GetValue(item);
                flag = true;
            }
            else if (this.ValueMemberProperty != null)
            {
                obj2 = this.ValueMemberProperty.GetValue(item);
                flag = true;
            }
            else if (!string.IsNullOrEmpty(this.DisplayMember))
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(item).Find(this.DisplayMember, true);
                if (descriptor != null)
                {
                    obj2 = descriptor.GetValue(item);
                    flag = true;
                }
            }
            else if (!string.IsNullOrEmpty(this.ValueMember))
            {
                PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(item).Find(this.ValueMember, true);
                if (descriptor2 != null)
                {
                    obj2 = descriptor2.GetValue(item);
                    flag = true;
                }
            }
            if (!flag)
            {
                obj2 = item;
            }
            return obj2;
        }

        internal ObjectCollection GetItems(DataGridView dataGridView)
        {
            ObjectCollection objects = (ObjectCollection) base.Properties.GetObject(PropComboBoxCellItems);
            if (objects == null)
            {
                objects = new ObjectCollection(this);
                base.Properties.SetObject(PropComboBoxCellItems, objects);
            }
            if (this.CreateItemsFromDataSource)
            {
                objects.ClearInternal();
                CurrencyManager dataManager = this.GetDataManager(dataGridView);
                if ((dataManager != null) && (dataManager.Count != -1))
                {
                    object[] items = new object[dataManager.Count];
                    for (int i = 0; i < items.Length; i++)
                    {
                        items[i] = dataManager[i];
                    }
                    objects.AddRangeInternal(items);
                }
                if ((dataManager == null) && ((this.flags & 0x10) != 0))
                {
                    return objects;
                }
                this.CreateItemsFromDataSource = false;
            }
            return objects;
        }

        internal object GetItemValue(object item)
        {
            bool flag = false;
            object obj2 = null;
            if (this.ValueMemberProperty != null)
            {
                obj2 = this.ValueMemberProperty.GetValue(item);
                flag = true;
            }
            else if (this.DisplayMemberProperty != null)
            {
                obj2 = this.DisplayMemberProperty.GetValue(item);
                flag = true;
            }
            else if (!string.IsNullOrEmpty(this.ValueMember))
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(item).Find(this.ValueMember, true);
                if (descriptor != null)
                {
                    obj2 = descriptor.GetValue(item);
                    flag = true;
                }
            }
            if (!flag && !string.IsNullOrEmpty(this.DisplayMember))
            {
                PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(item).Find(this.DisplayMember, true);
                if (descriptor2 != null)
                {
                    obj2 = descriptor2.GetValue(item);
                    flag = true;
                }
            }
            if (!flag)
            {
                obj2 = item;
            }
            return obj2;
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
            Size empty = Size.Empty;
            DataGridViewFreeDimension freeDimensionFromConstraint = DataGridViewCell.GetFreeDimensionFromConstraint(constraintSize);
            Rectangle stdBorderWidths = base.StdBorderWidths;
            int num = (stdBorderWidths.Left + stdBorderWidths.Width) + cellStyle.Padding.Horizontal;
            int num2 = (stdBorderWidths.Top + stdBorderWidths.Height) + cellStyle.Padding.Vertical;
            TextFormatFlags flags = DataGridViewUtilities.ComputeTextFormatFlagsForCellStyleAlignment(base.DataGridView.RightToLeftInternal, cellStyle.Alignment, cellStyle.WrapMode);
            string str = base.GetFormattedValue(rowIndex, ref cellStyle, DataGridViewDataErrorContexts.PreferredSize | DataGridViewDataErrorContexts.Formatting) as string;
            if (!string.IsNullOrEmpty(str))
            {
                empty = DataGridViewCell.MeasureTextSize(graphics, str, cellStyle.Font, flags);
            }
            else
            {
                empty = DataGridViewCell.MeasureTextSize(graphics, " ", cellStyle.Font, flags);
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
            if (freeDimensionFromConstraint != DataGridViewFreeDimension.Height)
            {
                empty.Width += ((SystemInformation.HorizontalScrollBarThumbWidth + 1) + 6) + num;
                if (base.DataGridView.ShowCellErrors)
                {
                    empty.Width = Math.Max(empty.Width, (((num + SystemInformation.HorizontalScrollBarThumbWidth) + 1) + 8) + 12);
                }
            }
            if (freeDimensionFromConstraint != DataGridViewFreeDimension.Width)
            {
                if ((this.FlatStyle == System.Windows.Forms.FlatStyle.Flat) || (this.FlatStyle == System.Windows.Forms.FlatStyle.Popup))
                {
                    empty.Height += 6;
                }
                else
                {
                    empty.Height += 8;
                }
                empty.Height += num2;
                if (base.DataGridView.ShowCellErrors)
                {
                    empty.Height = Math.Max(empty.Height, (num2 + 8) + 11);
                }
            }
            return empty;
        }

        private void InitializeComboBoxText()
        {
            this.EditingComboBox.EditingControlValueChanged = false;
            int editingControlRowIndex = this.EditingComboBox.EditingControlRowIndex;
            DataGridViewCellStyle cellStyle = this.GetInheritedStyle(null, editingControlRowIndex, false);
            this.EditingComboBox.Text = (string) this.GetFormattedValue(this.GetValue(editingControlRowIndex), editingControlRowIndex, ref cellStyle, null, null, DataGridViewDataErrorContexts.Formatting);
        }

        private void InitializeDisplayMemberPropertyDescriptor(string displayMember)
        {
            if (this.DataManager != null)
            {
                if (string.IsNullOrEmpty(displayMember))
                {
                    this.DisplayMemberProperty = null;
                }
                else
                {
                    BindingMemberInfo info = new BindingMemberInfo(displayMember);
                    this.DataManager = base.DataGridView.BindingContext[this.DataSource, info.BindingPath] as CurrencyManager;
                    PropertyDescriptor descriptor = this.DataManager.GetItemProperties().Find(info.BindingField, true);
                    if (descriptor == null)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridViewComboBoxCell_FieldNotFound", new object[] { displayMember }));
                    }
                    this.DisplayMemberProperty = descriptor;
                }
            }
        }

        public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            System.Windows.Forms.ComboBox editingControl = base.DataGridView.EditingControl as System.Windows.Forms.ComboBox;
            if (editingControl != null)
            {
                if ((this.GetInheritedState(rowIndex) & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
                {
                    base.DataGridView.EditingPanel.BackColor = dataGridViewCellStyle.SelectionBackColor;
                }
                if (editingControl.ParentInternal != null)
                {
                    IntPtr ptr1 = editingControl.ParentInternal.Handle;
                }
                IntPtr handle = editingControl.Handle;
                editingControl.DropDownStyle = ComboBoxStyle.DropDownList;
                editingControl.FormattingEnabled = true;
                editingControl.MaxDropDownItems = this.MaxDropDownItems;
                editingControl.DropDownWidth = this.DropDownWidth;
                editingControl.DataSource = null;
                editingControl.ValueMember = null;
                editingControl.Items.Clear();
                editingControl.DataSource = this.DataSource;
                editingControl.DisplayMember = this.DisplayMember;
                editingControl.ValueMember = this.ValueMember;
                if ((this.HasItems && (this.DataSource == null)) && (this.Items.Count > 0))
                {
                    editingControl.Items.AddRange(this.Items.InnerArray.ToArray());
                }
                editingControl.Sorted = this.Sorted;
                editingControl.FlatStyle = this.FlatStyle;
                if (this.AutoComplete)
                {
                    editingControl.AutoCompleteSource = AutoCompleteSource.ListItems;
                    editingControl.AutoCompleteMode = AutoCompleteMode.Append;
                }
                else
                {
                    editingControl.AutoCompleteMode = AutoCompleteMode.None;
                    editingControl.AutoCompleteSource = AutoCompleteSource.None;
                }
                string str = initialFormattedValue as string;
                if (str == null)
                {
                    str = string.Empty;
                }
                editingControl.Text = str;
                if ((this.flags & 0x20) == 0)
                {
                    editingControl.DropDown += new EventHandler(this.ComboBox_DropDown);
                    this.flags = (byte) (this.flags | 0x20);
                }
                cachedDropDownWidth = -1;
                this.EditingComboBox = base.DataGridView.EditingControl as DataGridViewComboBoxEditingControl;
                if (base.GetHeight(rowIndex) > 0x15)
                {
                    Rectangle rc = base.DataGridView.GetCellDisplayRectangle(base.ColumnIndex, rowIndex, true);
                    rc.Y += 0x15;
                    rc.Height -= 0x15;
                    base.DataGridView.Invalidate(rc);
                }
            }
        }

        private void InitializeValueMemberPropertyDescriptor(string valueMember)
        {
            if (this.DataManager != null)
            {
                if (string.IsNullOrEmpty(valueMember))
                {
                    this.ValueMemberProperty = null;
                }
                else
                {
                    BindingMemberInfo info = new BindingMemberInfo(valueMember);
                    this.DataManager = base.DataGridView.BindingContext[this.DataSource, info.BindingPath] as CurrencyManager;
                    PropertyDescriptor descriptor = this.DataManager.GetItemProperties().Find(info.BindingField, true);
                    if (descriptor == null)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridViewComboBoxCell_FieldNotFound", new object[] { valueMember }));
                    }
                    this.ValueMemberProperty = descriptor;
                }
            }
        }

        private object ItemFromComboBoxDataSource(PropertyDescriptor property, object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            object obj2 = null;
            if ((this.DataManager.List is IBindingList) && ((IBindingList) this.DataManager.List).SupportsSearching)
            {
                int num = ((IBindingList) this.DataManager.List).Find(property, key);
                if (num != -1)
                {
                    obj2 = this.DataManager.List[num];
                }
                return obj2;
            }
            for (int i = 0; i < this.DataManager.List.Count; i++)
            {
                object component = this.DataManager.List[i];
                object obj4 = property.GetValue(component);
                if (key.Equals(obj4))
                {
                    return component;
                }
            }
            return obj2;
        }

        private object ItemFromComboBoxItems(int rowIndex, string field, object key)
        {
            object component = null;
            if (this.OwnsEditingComboBox(rowIndex))
            {
                component = this.EditingComboBox.SelectedItem;
                object obj3 = null;
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component).Find(field, true);
                if (descriptor != null)
                {
                    obj3 = descriptor.GetValue(component);
                }
                if ((obj3 == null) || !obj3.Equals(key))
                {
                    component = null;
                }
            }
            if (component == null)
            {
                foreach (object obj4 in this.Items)
                {
                    object obj5 = null;
                    PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(obj4).Find(field, true);
                    if (descriptor2 != null)
                    {
                        obj5 = descriptor2.GetValue(obj4);
                    }
                    if ((obj5 != null) && obj5.Equals(key))
                    {
                        component = obj4;
                        break;
                    }
                }
            }
            if (component == null)
            {
                if (this.OwnsEditingComboBox(rowIndex))
                {
                    component = this.EditingComboBox.SelectedItem;
                    if ((component == null) || !component.Equals(key))
                    {
                        component = null;
                    }
                }
                if ((component == null) && this.Items.Contains(key))
                {
                    component = key;
                }
            }
            return component;
        }

        public override bool KeyEntersEditMode(KeyEventArgs e)
        {
            return ((((((char.IsLetterOrDigit((char) ((ushort) e.KeyCode)) && ((e.KeyCode < Keys.F1) || (e.KeyCode > Keys.F24))) || ((e.KeyCode >= Keys.NumPad0) && (e.KeyCode <= Keys.Divide))) || (((e.KeyCode >= Keys.Oem1) && (e.KeyCode <= Keys.Oem102)) || ((e.KeyCode == Keys.Space) && !e.Shift))) || ((e.KeyCode == Keys.F4) || (((e.KeyCode == Keys.Down) || (e.KeyCode == Keys.Up)) && e.Alt))) && (((!e.Alt || (e.KeyCode == Keys.Down)) || (e.KeyCode == Keys.Up)) && !e.Control)) || base.KeyEntersEditMode(e));
        }

        private bool LookupDisplayValue(int rowIndex, object value, out object displayValue)
        {
            object item = null;
            if ((this.DisplayMemberProperty != null) || (this.ValueMemberProperty != null))
            {
                item = this.ItemFromComboBoxDataSource((this.ValueMemberProperty != null) ? this.ValueMemberProperty : this.DisplayMemberProperty, value);
            }
            else
            {
                item = this.ItemFromComboBoxItems(rowIndex, string.IsNullOrEmpty(this.ValueMember) ? this.DisplayMember : this.ValueMember, value);
            }
            if (item == null)
            {
                displayValue = null;
                return false;
            }
            displayValue = this.GetItemDisplayValue(item);
            return true;
        }

        private bool LookupValue(object formattedValue, out object value)
        {
            if (formattedValue == null)
            {
                value = null;
                return true;
            }
            object item = null;
            if ((this.DisplayMemberProperty != null) || (this.ValueMemberProperty != null))
            {
                item = this.ItemFromComboBoxDataSource((this.DisplayMemberProperty != null) ? this.DisplayMemberProperty : this.ValueMemberProperty, formattedValue);
            }
            else
            {
                item = this.ItemFromComboBoxItems(base.RowIndex, string.IsNullOrEmpty(this.DisplayMember) ? this.ValueMember : this.DisplayMember, formattedValue);
            }
            if (item == null)
            {
                value = null;
                return false;
            }
            value = this.GetItemValue(item);
            return true;
        }

        protected override void OnDataGridViewChanged()
        {
            if (base.DataGridView != null)
            {
                this.InitializeDisplayMemberPropertyDescriptor(this.DisplayMember);
                this.InitializeValueMemberPropertyDescriptor(this.ValueMember);
            }
            base.OnDataGridViewChanged();
        }

        protected override void OnEnter(int rowIndex, bool throughMouseClick)
        {
            if ((base.DataGridView != null) && (throughMouseClick && (base.DataGridView.EditMode != DataGridViewEditMode.EditOnEnter)))
            {
                this.flags = (byte) (this.flags | 1);
            }
        }

        private void OnItemsCollectionChanged()
        {
            if (this.TemplateComboBoxColumn != null)
            {
                this.TemplateComboBoxColumn.OnItemsCollectionChanged();
            }
            cachedDropDownWidth = -1;
            if (this.OwnsEditingComboBox(base.RowIndex))
            {
                this.InitializeComboBoxText();
            }
            else
            {
                base.OnCommonChange();
            }
        }

        protected override void OnLeave(int rowIndex, bool throughMouseClick)
        {
            if (base.DataGridView != null)
            {
                this.flags = (byte) (this.flags & -2);
            }
        }

        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            if (base.DataGridView != null)
            {
                Point currentCellAddress = base.DataGridView.CurrentCellAddress;
                if ((currentCellAddress.X == e.ColumnIndex) && (currentCellAddress.Y == e.RowIndex))
                {
                    if ((this.flags & 1) != 0)
                    {
                        this.flags = (byte) (this.flags & -2);
                    }
                    else if (((this.EditingComboBox == null) || !this.EditingComboBox.DroppedDown) && (((base.DataGridView.EditMode != DataGridViewEditMode.EditProgrammatically) && base.DataGridView.BeginEdit(true)) && ((this.EditingComboBox != null) && (this.DisplayStyle != DataGridViewComboBoxDisplayStyle.Nothing))))
                    {
                        this.CheckDropDownList(e.X, e.Y, e.RowIndex);
                    }
                }
            }
        }

        protected override void OnMouseEnter(int rowIndex)
        {
            if (base.DataGridView != null)
            {
                if ((this.DisplayStyle == DataGridViewComboBoxDisplayStyle.ComboBox) && (this.FlatStyle == System.Windows.Forms.FlatStyle.Popup))
                {
                    base.DataGridView.InvalidateCell(base.ColumnIndex, rowIndex);
                }
                base.OnMouseEnter(rowIndex);
            }
        }

        protected override void OnMouseLeave(int rowIndex)
        {
            if (base.DataGridView != null)
            {
                if (mouseInDropDownButtonBounds)
                {
                    mouseInDropDownButtonBounds = false;
                    if ((((base.ColumnIndex >= 0) && (rowIndex >= 0)) && ((this.FlatStyle == System.Windows.Forms.FlatStyle.Standard) || (this.FlatStyle == System.Windows.Forms.FlatStyle.System))) && base.DataGridView.ApplyVisualStylesToInnerCells)
                    {
                        base.DataGridView.InvalidateCell(base.ColumnIndex, rowIndex);
                    }
                }
                if ((this.DisplayStyle == DataGridViewComboBoxDisplayStyle.ComboBox) && (this.FlatStyle == System.Windows.Forms.FlatStyle.Popup))
                {
                    base.DataGridView.InvalidateCell(base.ColumnIndex, rowIndex);
                }
                base.OnMouseEnter(rowIndex);
            }
        }

        protected override void OnMouseMove(DataGridViewCellMouseEventArgs e)
        {
            if (base.DataGridView != null)
            {
                if (((this.FlatStyle == System.Windows.Forms.FlatStyle.Standard) || (this.FlatStyle == System.Windows.Forms.FlatStyle.System)) && base.DataGridView.ApplyVisualStylesToInnerCells)
                {
                    Rectangle rectangle2;
                    int rowIndex = e.RowIndex;
                    DataGridViewCellStyle cellStyle = this.GetInheritedStyle(null, rowIndex, false);
                    bool singleVerticalBorderAdded = !base.DataGridView.RowHeadersVisible && (base.DataGridView.AdvancedCellBorderStyle.All == DataGridViewAdvancedCellBorderStyle.Single);
                    bool singleHorizontalBorderAdded = !base.DataGridView.ColumnHeadersVisible && (base.DataGridView.AdvancedCellBorderStyle.All == DataGridViewAdvancedCellBorderStyle.Single);
                    bool isFirstDisplayedColumn = rowIndex == base.DataGridView.FirstDisplayedScrollingRowIndex;
                    bool isFirstDisplayedRow = base.OwningColumn.Index == base.DataGridView.FirstDisplayedColumnIndex;
                    bool flag5 = base.OwningColumn.Index == base.DataGridView.FirstDisplayedScrollingColumnIndex;
                    DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder = new DataGridViewAdvancedBorderStyle();
                    DataGridViewAdvancedBorderStyle advancedBorderStyle = this.AdjustCellBorderStyle(base.DataGridView.AdvancedCellBorderStyle, dataGridViewAdvancedBorderStylePlaceholder, singleVerticalBorderAdded, singleHorizontalBorderAdded, isFirstDisplayedColumn, isFirstDisplayedRow);
                    Rectangle clipBounds = base.DataGridView.GetCellDisplayRectangle(base.OwningColumn.Index, rowIndex, false);
                    if (flag5)
                    {
                        clipBounds.X -= base.DataGridView.FirstDisplayedScrollingColumnHiddenWidth;
                        clipBounds.Width += base.DataGridView.FirstDisplayedScrollingColumnHiddenWidth;
                    }
                    DataGridViewElementStates rowState = base.DataGridView.Rows.GetRowState(rowIndex);
                    DataGridViewElementStates elementState = base.CellStateFromColumnRowStates(rowState) | this.State;
                    using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
                    {
                        this.PaintPrivate(graphics, clipBounds, clipBounds, rowIndex, elementState, null, null, cellStyle, advancedBorderStyle, out rectangle2, DataGridViewPaintParts.ContentForeground, false, false, true, false);
                    }
                    bool flag6 = rectangle2.Contains(base.DataGridView.PointToClient(Control.MousePosition));
                    if (flag6 != mouseInDropDownButtonBounds)
                    {
                        mouseInDropDownButtonBounds = flag6;
                        base.DataGridView.InvalidateCell(e.ColumnIndex, rowIndex);
                    }
                }
                base.OnMouseMove(e);
            }
        }

        private bool OwnsEditingComboBox(int rowIndex)
        {
            return (((rowIndex != -1) && (this.EditingComboBox != null)) && (rowIndex == this.EditingComboBox.EditingControlRowIndex));
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            Rectangle rectangle;
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }
            this.PaintPrivate(graphics, clipBounds, cellBounds, rowIndex, elementState, formattedValue, errorText, cellStyle, advancedBorderStyle, out rectangle, paintParts, false, false, false, true);
        }

        private Rectangle PaintPrivate(Graphics g, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, out Rectangle dropDownButtonRect, DataGridViewPaintParts paintParts, bool computeContentBounds, bool computeErrorIconBounds, bool computeDropDownButtonRect, bool paint)
        {
            SolidBrush cachedBrush;
            Rectangle empty = Rectangle.Empty;
            dropDownButtonRect = Rectangle.Empty;
            bool flag = (this.FlatStyle == System.Windows.Forms.FlatStyle.Flat) || (this.FlatStyle == System.Windows.Forms.FlatStyle.Popup);
            bool flag2 = ((this.FlatStyle == System.Windows.Forms.FlatStyle.Popup) && (base.DataGridView.MouseEnteredCellAddress.Y == rowIndex)) && (base.DataGridView.MouseEnteredCellAddress.X == base.ColumnIndex);
            bool flag3 = !flag && base.DataGridView.ApplyVisualStylesToInnerCells;
            bool flag4 = flag3 && PostXPThemesExist;
            ComboBoxState normal = ComboBoxState.Normal;
            if (((base.DataGridView.MouseEnteredCellAddress.Y == rowIndex) && (base.DataGridView.MouseEnteredCellAddress.X == base.ColumnIndex)) && mouseInDropDownButtonBounds)
            {
                normal = ComboBoxState.Hot;
            }
            if (paint && DataGridViewCell.PaintBorder(paintParts))
            {
                this.PaintBorder(g, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
            }
            Rectangle rectangle2 = this.BorderWidths(advancedBorderStyle);
            Rectangle bounds = cellBounds;
            bounds.Offset(rectangle2.X, rectangle2.Y);
            bounds.Width -= rectangle2.Right;
            bounds.Height -= rectangle2.Bottom;
            Point currentCellAddress = base.DataGridView.CurrentCellAddress;
            bool flag5 = (currentCellAddress.X == base.ColumnIndex) && (currentCellAddress.Y == rowIndex);
            bool flag6 = flag5 && (base.DataGridView.EditingControl != null);
            bool flag7 = (elementState & DataGridViewElementStates.Selected) != DataGridViewElementStates.None;
            bool flag8 = (this.DisplayStyle == DataGridViewComboBoxDisplayStyle.ComboBox) && ((this.DisplayStyleForCurrentCellOnly && flag5) || !this.DisplayStyleForCurrentCellOnly);
            bool flag9 = (this.DisplayStyle != DataGridViewComboBoxDisplayStyle.Nothing) && ((this.DisplayStyleForCurrentCellOnly && flag5) || !this.DisplayStyleForCurrentCellOnly);
            if ((DataGridViewCell.PaintSelectionBackground(paintParts) && flag7) && !flag6)
            {
                cachedBrush = base.DataGridView.GetCachedBrush(cellStyle.SelectionBackColor);
            }
            else
            {
                cachedBrush = base.DataGridView.GetCachedBrush(cellStyle.BackColor);
            }
            if (((paint && DataGridViewCell.PaintBackground(paintParts)) && ((cachedBrush.Color.A == 0xff) && (bounds.Width > 0))) && (bounds.Height > 0))
            {
                DataGridViewCell.PaintPadding(g, bounds, cellStyle, cachedBrush, base.DataGridView.RightToLeftInternal);
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
            if ((paint && (bounds.Width > 0)) && (bounds.Height > 0))
            {
                if (flag3 && flag8)
                {
                    if ((flag4 && DataGridViewCell.PaintBackground(paintParts)) && (cachedBrush.Color.A == 0xff))
                    {
                        g.FillRectangle(cachedBrush, bounds.Left, bounds.Top, bounds.Width, bounds.Height);
                    }
                    if (DataGridViewCell.PaintContentBackground(paintParts))
                    {
                        if (flag4)
                        {
                            DataGridViewComboBoxCellRenderer.DrawBorder(g, bounds);
                        }
                        else
                        {
                            DataGridViewComboBoxCellRenderer.DrawTextBox(g, bounds, normal);
                        }
                    }
                    if (((!flag4 && DataGridViewCell.PaintBackground(paintParts)) && ((cachedBrush.Color.A == 0xff) && (bounds.Width > 2))) && (bounds.Height > 2))
                    {
                        g.FillRectangle(cachedBrush, (int) (bounds.Left + 1), (int) (bounds.Top + 1), (int) (bounds.Width - 2), (int) (bounds.Height - 2));
                    }
                }
                else if (DataGridViewCell.PaintBackground(paintParts) && (cachedBrush.Color.A == 0xff))
                {
                    if ((flag4 && flag9) && !flag8)
                    {
                        g.DrawRectangle(SystemPens.ControlLightLight, new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1));
                    }
                    else
                    {
                        g.FillRectangle(cachedBrush, bounds.Left, bounds.Top, bounds.Width, bounds.Height);
                    }
                }
            }
            int width = Math.Min(SystemInformation.HorizontalScrollBarThumbWidth, (bounds.Width - 6) - 1);
            if (!flag6)
            {
                int num2;
                if (flag3 || flag)
                {
                    num2 = Math.Min(this.GetDropDownButtonHeight(g, cellStyle), flag4 ? bounds.Height : (bounds.Height - 2));
                }
                else
                {
                    num2 = Math.Min(this.GetDropDownButtonHeight(g, cellStyle), bounds.Height - 4);
                }
                if ((width > 0) && (num2 > 0))
                {
                    Rectangle rectangle4;
                    if (flag3 || flag)
                    {
                        if (flag4)
                        {
                            rectangle4 = new Rectangle(base.DataGridView.RightToLeftInternal ? bounds.Left : (bounds.Right - width), bounds.Top, width, num2);
                        }
                        else
                        {
                            rectangle4 = new Rectangle(base.DataGridView.RightToLeftInternal ? (bounds.Left + 1) : ((bounds.Right - width) - 1), bounds.Top + 1, width, num2);
                        }
                    }
                    else
                    {
                        rectangle4 = new Rectangle(base.DataGridView.RightToLeftInternal ? (bounds.Left + 2) : ((bounds.Right - width) - 2), bounds.Top + 2, width, num2);
                    }
                    if ((flag4 && flag9) && !flag8)
                    {
                        dropDownButtonRect = bounds;
                    }
                    else
                    {
                        dropDownButtonRect = rectangle4;
                    }
                    if (paint && DataGridViewCell.PaintContentBackground(paintParts))
                    {
                        if (flag9)
                        {
                            if (flag)
                            {
                                g.FillRectangle(SystemBrushes.Control, rectangle4);
                            }
                            else if (flag3)
                            {
                                if (flag4)
                                {
                                    if (flag8)
                                    {
                                        DataGridViewComboBoxCellRenderer.DrawDropDownButton(g, rectangle4, normal, base.DataGridView.RightToLeftInternal);
                                    }
                                    else
                                    {
                                        DataGridViewComboBoxCellRenderer.DrawReadOnlyButton(g, bounds, normal);
                                        DataGridViewComboBoxCellRenderer.DrawDropDownButton(g, rectangle4, ComboBoxState.Normal);
                                    }
                                }
                                else
                                {
                                    DataGridViewComboBoxCellRenderer.DrawDropDownButton(g, rectangle4, normal);
                                }
                            }
                            else
                            {
                                g.FillRectangle(SystemBrushes.Control, rectangle4);
                            }
                        }
                        if ((!flag && !flag3) && (flag8 || flag9))
                        {
                            Color nearestColor;
                            Color controlDarkDark;
                            Color controlLightLight;
                            Pen controlLight;
                            Color control = SystemColors.Control;
                            Color color = control;
                            bool flag10 = control.ToKnownColor() == SystemColors.Control.ToKnownColor();
                            bool highContrast = SystemInformation.HighContrast;
                            if (control == SystemColors.Control)
                            {
                                nearestColor = SystemColors.ControlDark;
                                controlDarkDark = SystemColors.ControlDarkDark;
                                controlLightLight = SystemColors.ControlLightLight;
                            }
                            else
                            {
                                nearestColor = ControlPaint.Dark(control);
                                controlLightLight = ControlPaint.LightLight(control);
                                if (highContrast)
                                {
                                    controlDarkDark = ControlPaint.LightLight(control);
                                }
                                else
                                {
                                    controlDarkDark = ControlPaint.DarkDark(control);
                                }
                            }
                            nearestColor = g.GetNearestColor(nearestColor);
                            controlDarkDark = g.GetNearestColor(controlDarkDark);
                            color = g.GetNearestColor(color);
                            controlLightLight = g.GetNearestColor(controlLightLight);
                            if (flag10)
                            {
                                if (SystemInformation.HighContrast)
                                {
                                    controlLight = SystemPens.ControlLight;
                                }
                                else
                                {
                                    controlLight = SystemPens.Control;
                                }
                            }
                            else
                            {
                                controlLight = new Pen(controlLightLight);
                            }
                            if (flag9)
                            {
                                g.DrawLine(controlLight, rectangle4.X, rectangle4.Y, (rectangle4.X + rectangle4.Width) - 1, rectangle4.Y);
                                g.DrawLine(controlLight, rectangle4.X, rectangle4.Y, rectangle4.X, (rectangle4.Y + rectangle4.Height) - 1);
                            }
                            if (flag8)
                            {
                                g.DrawLine(controlLight, bounds.X, (bounds.Y + bounds.Height) - 1, (bounds.X + bounds.Width) - 1, (bounds.Y + bounds.Height) - 1);
                                g.DrawLine(controlLight, (bounds.X + bounds.Width) - 1, bounds.Y, (bounds.X + bounds.Width) - 1, (bounds.Y + bounds.Height) - 1);
                            }
                            if (flag10)
                            {
                                controlLight = SystemPens.ControlDarkDark;
                            }
                            else
                            {
                                controlLight.Color = controlDarkDark;
                            }
                            if (flag9)
                            {
                                g.DrawLine(controlLight, rectangle4.X, (rectangle4.Y + rectangle4.Height) - 1, (rectangle4.X + rectangle4.Width) - 1, (rectangle4.Y + rectangle4.Height) - 1);
                                g.DrawLine(controlLight, (rectangle4.X + rectangle4.Width) - 1, rectangle4.Y, (rectangle4.X + rectangle4.Width) - 1, (rectangle4.Y + rectangle4.Height) - 1);
                            }
                            if (flag8)
                            {
                                g.DrawLine(controlLight, bounds.X, bounds.Y, (bounds.X + bounds.Width) - 2, bounds.Y);
                                g.DrawLine(controlLight, bounds.X, bounds.Y, bounds.X, (bounds.Y + bounds.Height) - 1);
                            }
                            if (flag10)
                            {
                                controlLight = SystemPens.ControlLightLight;
                            }
                            else
                            {
                                controlLight.Color = color;
                            }
                            if (flag9)
                            {
                                g.DrawLine(controlLight, (int) (rectangle4.X + 1), (int) (rectangle4.Y + 1), (int) ((rectangle4.X + rectangle4.Width) - 2), (int) (rectangle4.Y + 1));
                                g.DrawLine(controlLight, (int) (rectangle4.X + 1), (int) (rectangle4.Y + 1), (int) (rectangle4.X + 1), (int) ((rectangle4.Y + rectangle4.Height) - 2));
                            }
                            if (flag10)
                            {
                                controlLight = SystemPens.ControlDark;
                            }
                            else
                            {
                                controlLight.Color = nearestColor;
                            }
                            if (flag9)
                            {
                                g.DrawLine(controlLight, (int) (rectangle4.X + 1), (int) ((rectangle4.Y + rectangle4.Height) - 2), (int) ((rectangle4.X + rectangle4.Width) - 2), (int) ((rectangle4.Y + rectangle4.Height) - 2));
                                g.DrawLine(controlLight, (int) ((rectangle4.X + rectangle4.Width) - 2), (int) (rectangle4.Y + 1), (int) ((rectangle4.X + rectangle4.Width) - 2), (int) ((rectangle4.Y + rectangle4.Height) - 2));
                            }
                            if (!flag10)
                            {
                                controlLight.Dispose();
                            }
                        }
                        if (((width >= 5) && (num2 >= 3)) && flag9)
                        {
                            if (flag)
                            {
                                Point point2;
                                point2 = new Point(rectangle4.Left + (rectangle4.Width / 2), rectangle4.Top + (rectangle4.Height / 2)) {
                                    X = point2.X + (rectangle4.Width % 2),
                                    Y = point2.Y + (rectangle4.Height % 2)
                                };
                                Point[] points = new Point[] { new Point(point2.X - 2, point2.Y - 1), new Point(point2.X + 3, point2.Y - 1), new Point(point2.X, point2.Y + 2) };
                                g.FillPolygon(SystemBrushes.ControlText, points);
                            }
                            else if (!flag3)
                            {
                                Point point3;
                                rectangle4.X--;
                                rectangle4.Width++;
                                point3 = new Point(rectangle4.Left + ((rectangle4.Width - 1) / 2), rectangle4.Top + ((rectangle4.Height + 4) / 2)) {
                                    X = point3.X + ((rectangle4.Width + 1) % 2),
                                    Y = point3.Y + (rectangle4.Height % 2)
                                };
                                Point point4 = new Point(point3.X - 3, point3.Y - 4);
                                Point point5 = new Point(point3.X + 3, point3.Y - 4);
                                Point[] pointArray2 = new Point[] { point4, point5, point3 };
                                g.FillPolygon(SystemBrushes.ControlText, pointArray2);
                                g.DrawLine(SystemPens.ControlText, point4.X, point4.Y, point5.X, point5.Y);
                                rectangle4.X++;
                                rectangle4.Width--;
                            }
                        }
                        if (flag2 && flag8)
                        {
                            rectangle4.Y--;
                            rectangle4.Height++;
                            g.DrawRectangle(SystemPens.ControlDark, rectangle4);
                        }
                    }
                }
            }
            Rectangle cellValueBounds = bounds;
            Rectangle rectangle = Rectangle.Inflate(bounds, -2, -2);
            if (flag4)
            {
                if (!base.DataGridView.RightToLeftInternal)
                {
                    rectangle.X--;
                }
                rectangle.Width++;
            }
            if (flag9)
            {
                if (flag3 || flag)
                {
                    cellValueBounds.Width -= width;
                    rectangle.Width -= width;
                    if (base.DataGridView.RightToLeftInternal)
                    {
                        cellValueBounds.X += width;
                        rectangle.X += width;
                    }
                }
                else
                {
                    cellValueBounds.Width -= width + 1;
                    rectangle.Width -= width + 1;
                    if (base.DataGridView.RightToLeftInternal)
                    {
                        cellValueBounds.X += width + 1;
                        rectangle.X += width + 1;
                    }
                }
            }
            if ((rectangle.Width > 1) && (rectangle.Height > 1))
            {
                if (((flag5 && !flag6) && (DataGridViewCell.PaintFocus(paintParts) && base.DataGridView.ShowFocusCues)) && (base.DataGridView.Focused && paint))
                {
                    if (flag)
                    {
                        Rectangle rectangle7 = rectangle;
                        if (!base.DataGridView.RightToLeftInternal)
                        {
                            rectangle7.X--;
                        }
                        rectangle7.Width++;
                        rectangle7.Y--;
                        rectangle7.Height += 2;
                        ControlPaint.DrawFocusRectangle(g, rectangle7, Color.Empty, cachedBrush.Color);
                    }
                    else if (flag4)
                    {
                        Rectangle rectangle8 = rectangle;
                        rectangle8.X++;
                        rectangle8.Width -= 2;
                        rectangle8.Y++;
                        rectangle8.Height -= 2;
                        if ((rectangle8.Width > 0) && (rectangle8.Height > 0))
                        {
                            ControlPaint.DrawFocusRectangle(g, rectangle8, Color.Empty, cachedBrush.Color);
                        }
                    }
                    else
                    {
                        ControlPaint.DrawFocusRectangle(g, rectangle, Color.Empty, cachedBrush.Color);
                    }
                }
                if (flag2)
                {
                    bounds.Width--;
                    bounds.Height--;
                    if ((!flag6 && paint) && (DataGridViewCell.PaintContentBackground(paintParts) && flag8))
                    {
                        g.DrawRectangle(SystemPens.ControlDark, bounds);
                    }
                }
                string text = formattedValue as string;
                if (text != null)
                {
                    int y = (cellStyle.WrapMode == DataGridViewTriState.True) ? 0 : 1;
                    if (base.DataGridView.RightToLeftInternal)
                    {
                        rectangle.Offset(0, y);
                        rectangle.Width += 2;
                    }
                    else
                    {
                        rectangle.Offset(-1, y);
                        rectangle.Width++;
                    }
                    rectangle.Height -= y;
                    if ((rectangle.Width > 0) && (rectangle.Height > 0))
                    {
                        TextFormatFlags flags = DataGridViewUtilities.ComputeTextFormatFlagsForCellStyleAlignment(base.DataGridView.RightToLeftInternal, cellStyle.Alignment, cellStyle.WrapMode);
                        if (!flag6 && paint)
                        {
                            if (DataGridViewCell.PaintContentForeground(paintParts))
                            {
                                Color color6;
                                if ((flags & TextFormatFlags.SingleLine) != TextFormatFlags.Default)
                                {
                                    flags |= TextFormatFlags.EndEllipsis;
                                }
                                if (flag4 && (flag9 || flag8))
                                {
                                    color6 = DataGridViewComboBoxCellRenderer.VisualStyleRenderer.GetColor(ColorProperty.TextColor);
                                }
                                else
                                {
                                    color6 = flag7 ? cellStyle.SelectionForeColor : cellStyle.ForeColor;
                                }
                                TextRenderer.DrawText(g, text, cellStyle.Font, rectangle, color6, flags);
                            }
                        }
                        else if (computeContentBounds)
                        {
                            empty = DataGridViewUtilities.GetTextBounds(rectangle, text, flags, cellStyle);
                        }
                    }
                }
                if ((base.DataGridView.ShowCellErrors && paint) && DataGridViewCell.PaintErrorIcon(paintParts))
                {
                    base.PaintErrorIcon(g, cellStyle, rowIndex, cellBounds, cellValueBounds, errorText);
                    if (flag6)
                    {
                        return Rectangle.Empty;
                    }
                }
            }
            if (!computeErrorIconBounds)
            {
                return empty;
            }
            if (!string.IsNullOrEmpty(errorText))
            {
                return base.ComputeErrorIconBounds(cellValueBounds);
            }
            return Rectangle.Empty;
        }

        public override object ParseFormattedValue(object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter)
        {
            if (valueTypeConverter == null)
            {
                if (this.ValueMemberProperty != null)
                {
                    valueTypeConverter = this.ValueMemberProperty.Converter;
                }
                else if (this.DisplayMemberProperty != null)
                {
                    valueTypeConverter = this.DisplayMemberProperty.Converter;
                }
            }
            if (((this.DataManager == null) || ((this.DisplayMemberProperty == null) && (this.ValueMemberProperty == null))) && (string.IsNullOrEmpty(this.DisplayMember) && string.IsNullOrEmpty(this.ValueMember)))
            {
                return base.ParseFormattedValueInternal(this.ValueType, formattedValue, cellStyle, formattedValueTypeConverter, valueTypeConverter);
            }
            object obj2 = base.ParseFormattedValueInternal(this.DisplayType, formattedValue, cellStyle, formattedValueTypeConverter, this.DisplayTypeConverter);
            object obj3 = obj2;
            if (this.LookupValue(obj3, out obj2))
            {
                return obj2;
            }
            if (obj3 != DBNull.Value)
            {
                throw new FormatException(string.Format(CultureInfo.CurrentCulture, System.Windows.Forms.SR.GetString("Formatter_CantConvert"), new object[] { obj2, this.DisplayType }));
            }
            return DBNull.Value;
        }

        public override string ToString()
        {
            return ("DataGridViewComboBoxCell { ColumnIndex=" + base.ColumnIndex.ToString(CultureInfo.CurrentCulture) + ", RowIndex=" + base.RowIndex.ToString(CultureInfo.CurrentCulture) + " }");
        }

        private void UnwireDataSource()
        {
            IComponent dataSource = this.DataSource as IComponent;
            if (dataSource != null)
            {
                dataSource.Disposed -= new EventHandler(this.DataSource_Disposed);
            }
            ISupportInitializeNotification notification = this.DataSource as ISupportInitializeNotification;
            if ((notification != null) && ((this.flags & 0x10) != 0))
            {
                notification.Initialized -= new EventHandler(this.DataSource_Initialized);
                this.flags = (byte) (this.flags & -17);
            }
        }

        private void WireDataSource(object dataSource)
        {
            IComponent component = dataSource as IComponent;
            if (component != null)
            {
                component.Disposed += new EventHandler(this.DataSource_Disposed);
            }
        }

        [DefaultValue(true)]
        public virtual bool AutoComplete
        {
            get
            {
                return ((this.flags & 8) != 0);
            }
            set
            {
                if (value != this.AutoComplete)
                {
                    if (value)
                    {
                        this.flags = (byte) (this.flags | 8);
                    }
                    else
                    {
                        this.flags = (byte) (this.flags & -9);
                    }
                    if (this.OwnsEditingComboBox(base.RowIndex))
                    {
                        if (value)
                        {
                            this.EditingComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
                            this.EditingComboBox.AutoCompleteMode = AutoCompleteMode.Append;
                        }
                        else
                        {
                            this.EditingComboBox.AutoCompleteMode = AutoCompleteMode.None;
                            this.EditingComboBox.AutoCompleteSource = AutoCompleteSource.None;
                        }
                    }
                }
            }
        }

        private bool CreateItemsFromDataSource
        {
            get
            {
                return ((this.flags & 4) != 0);
            }
            set
            {
                if (value)
                {
                    this.flags = (byte) (this.flags | 4);
                }
                else
                {
                    this.flags = (byte) (this.flags & -5);
                }
            }
        }

        private CurrencyManager DataManager
        {
            get
            {
                return this.GetDataManager(base.DataGridView);
            }
            set
            {
                if ((value != null) || base.Properties.ContainsObject(PropComboBoxCellDataManager))
                {
                    base.Properties.SetObject(PropComboBoxCellDataManager, value);
                }
            }
        }

        public virtual object DataSource
        {
            get
            {
                return base.Properties.GetObject(PropComboBoxCellDataSource);
            }
            set
            {
                if (((value != null) && !(value is IList)) && !(value is IListSource))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("BadDataSourceForComplexBinding"));
                }
                if (this.DataSource != value)
                {
                    this.DataManager = null;
                    this.UnwireDataSource();
                    base.Properties.SetObject(PropComboBoxCellDataSource, value);
                    this.WireDataSource(value);
                    this.CreateItemsFromDataSource = true;
                    cachedDropDownWidth = -1;
                    try
                    {
                        this.InitializeDisplayMemberPropertyDescriptor(this.DisplayMember);
                    }
                    catch (Exception exception)
                    {
                        if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                        {
                            throw;
                        }
                        this.DisplayMemberInternal = null;
                    }
                    try
                    {
                        this.InitializeValueMemberPropertyDescriptor(this.ValueMember);
                    }
                    catch (Exception exception2)
                    {
                        if (System.Windows.Forms.ClientUtils.IsCriticalException(exception2))
                        {
                            throw;
                        }
                        this.ValueMemberInternal = null;
                    }
                    if (value == null)
                    {
                        this.DisplayMemberInternal = null;
                        this.ValueMemberInternal = null;
                    }
                    if (this.OwnsEditingComboBox(base.RowIndex))
                    {
                        this.EditingComboBox.DataSource = value;
                        this.InitializeComboBoxText();
                    }
                    else
                    {
                        base.OnCommonChange();
                    }
                }
            }
        }

        [DefaultValue("")]
        public virtual string DisplayMember
        {
            get
            {
                object obj2 = base.Properties.GetObject(PropComboBoxCellDisplayMember);
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.DisplayMemberInternal = value;
                if (this.OwnsEditingComboBox(base.RowIndex))
                {
                    this.EditingComboBox.DisplayMember = value;
                    this.InitializeComboBoxText();
                }
                else
                {
                    base.OnCommonChange();
                }
            }
        }

        private string DisplayMemberInternal
        {
            set
            {
                this.InitializeDisplayMemberPropertyDescriptor(value);
                if (((value != null) && (value.Length > 0)) || base.Properties.ContainsObject(PropComboBoxCellDisplayMember))
                {
                    base.Properties.SetObject(PropComboBoxCellDisplayMember, value);
                }
            }
        }

        private PropertyDescriptor DisplayMemberProperty
        {
            get
            {
                return (PropertyDescriptor) base.Properties.GetObject(PropComboBoxCellDisplayMemberProp);
            }
            set
            {
                if ((value != null) || base.Properties.ContainsObject(PropComboBoxCellDisplayMemberProp))
                {
                    base.Properties.SetObject(PropComboBoxCellDisplayMemberProp, value);
                }
            }
        }

        [DefaultValue(1)]
        public DataGridViewComboBoxDisplayStyle DisplayStyle
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropComboBoxCellDisplayStyle, out flag);
                if (flag)
                {
                    return (DataGridViewComboBoxDisplayStyle) integer;
                }
                return DataGridViewComboBoxDisplayStyle.DropDownButton;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DataGridViewComboBoxDisplayStyle));
                }
                if (value != this.DisplayStyle)
                {
                    base.Properties.SetInteger(PropComboBoxCellDisplayStyle, (int) value);
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

        [DefaultValue(false)]
        public bool DisplayStyleForCurrentCellOnly
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropComboBoxCellDisplayStyleForCurrentCellOnly, out flag);
                if (!flag)
                {
                    return false;
                }
                return (integer != 0);
            }
            set
            {
                if (value != this.DisplayStyleForCurrentCellOnly)
                {
                    base.Properties.SetInteger(PropComboBoxCellDisplayStyleForCurrentCellOnly, value ? 1 : 0);
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

        internal bool DisplayStyleForCurrentCellOnlyInternal
        {
            set
            {
                if (value != this.DisplayStyleForCurrentCellOnly)
                {
                    base.Properties.SetInteger(PropComboBoxCellDisplayStyleForCurrentCellOnly, value ? 1 : 0);
                }
            }
        }

        internal DataGridViewComboBoxDisplayStyle DisplayStyleInternal
        {
            set
            {
                if (value != this.DisplayStyle)
                {
                    base.Properties.SetInteger(PropComboBoxCellDisplayStyle, (int) value);
                }
            }
        }

        private System.Type DisplayType
        {
            get
            {
                if (this.DisplayMemberProperty != null)
                {
                    return this.DisplayMemberProperty.PropertyType;
                }
                if (this.ValueMemberProperty != null)
                {
                    return this.ValueMemberProperty.PropertyType;
                }
                return defaultFormattedValueType;
            }
        }

        private TypeConverter DisplayTypeConverter
        {
            get
            {
                if (base.DataGridView != null)
                {
                    return base.DataGridView.GetCachedTypeConverter(this.DisplayType);
                }
                return TypeDescriptor.GetConverter(this.DisplayType);
            }
        }

        [DefaultValue(1)]
        public virtual int DropDownWidth
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropComboBoxCellDropDownWidth, out flag);
                if (!flag)
                {
                    return 1;
                }
                return integer;
            }
            set
            {
                if (value < 1)
                {
                    object[] args = new object[] { 1.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("DropDownWidth", value, System.Windows.Forms.SR.GetString("DataGridViewComboBoxCell_DropDownWidthOutOfRange", args));
                }
                base.Properties.SetInteger(PropComboBoxCellDropDownWidth, value);
                if (this.OwnsEditingComboBox(base.RowIndex))
                {
                    this.EditingComboBox.DropDownWidth = value;
                }
            }
        }

        private DataGridViewComboBoxEditingControl EditingComboBox
        {
            get
            {
                return (DataGridViewComboBoxEditingControl) base.Properties.GetObject(PropComboBoxCellEditingComboBox);
            }
            set
            {
                if ((value != null) || base.Properties.ContainsObject(PropComboBoxCellEditingComboBox))
                {
                    base.Properties.SetObject(PropComboBoxCellEditingComboBox, value);
                }
            }
        }

        public override System.Type EditType
        {
            get
            {
                return defaultEditType;
            }
        }

        [DefaultValue(2)]
        public System.Windows.Forms.FlatStyle FlatStyle
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropComboBoxCellFlatStyle, out flag);
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
                    base.Properties.SetInteger(PropComboBoxCellFlatStyle, (int) value);
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
                    base.Properties.SetInteger(PropComboBoxCellFlatStyle, (int) value);
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

        internal bool HasItems
        {
            get
            {
                return (base.Properties.ContainsObject(PropComboBoxCellItems) && (base.Properties.GetObject(PropComboBoxCellItems) != null));
            }
        }

        [Browsable(false)]
        public virtual ObjectCollection Items
        {
            get
            {
                return this.GetItems(base.DataGridView);
            }
        }

        [DefaultValue(8)]
        public virtual int MaxDropDownItems
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropComboBoxCellMaxDropDownItems, out flag);
                if (flag)
                {
                    return integer;
                }
                return 8;
            }
            set
            {
                if ((value < 1) || (value > 100))
                {
                    object[] args = new object[] { 1.ToString(CultureInfo.CurrentCulture), 100.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("MaxDropDownItems", value, System.Windows.Forms.SR.GetString("DataGridViewComboBoxCell_MaxDropDownItemsOutOfRange", args));
                }
                base.Properties.SetInteger(PropComboBoxCellMaxDropDownItems, value);
                if (this.OwnsEditingComboBox(base.RowIndex))
                {
                    this.EditingComboBox.MaxDropDownItems = value;
                }
            }
        }

        private bool PaintXPThemes
        {
            get
            {
                return (((this.FlatStyle != System.Windows.Forms.FlatStyle.Flat) && (this.FlatStyle != System.Windows.Forms.FlatStyle.Popup)) && base.DataGridView.ApplyVisualStylesToInnerCells);
            }
        }

        private static bool PostXPThemesExist
        {
            get
            {
                return VisualStyleRenderer.IsElementDefined(VisualStyleElement.ComboBox.ReadOnlyButton.Normal);
            }
        }

        [DefaultValue(false)]
        public virtual bool Sorted
        {
            get
            {
                return ((this.flags & 2) != 0);
            }
            set
            {
                if (value != this.Sorted)
                {
                    if (value)
                    {
                        if (this.DataSource != null)
                        {
                            throw new ArgumentException(System.Windows.Forms.SR.GetString("ComboBoxSortWithDataSource"));
                        }
                        this.Items.SortInternal();
                        this.flags = (byte) (this.flags | 2);
                    }
                    else
                    {
                        this.flags = (byte) (this.flags & -3);
                    }
                    if (this.OwnsEditingComboBox(base.RowIndex))
                    {
                        this.EditingComboBox.Sorted = value;
                    }
                }
            }
        }

        internal DataGridViewComboBoxColumn TemplateComboBoxColumn
        {
            get
            {
                return (DataGridViewComboBoxColumn) base.Properties.GetObject(PropComboBoxCellColumnTemplate);
            }
            set
            {
                base.Properties.SetObject(PropComboBoxCellColumnTemplate, value);
            }
        }

        [DefaultValue("")]
        public virtual string ValueMember
        {
            get
            {
                object obj2 = base.Properties.GetObject(PropComboBoxCellValueMember);
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ValueMemberInternal = value;
                if (this.OwnsEditingComboBox(base.RowIndex))
                {
                    this.EditingComboBox.ValueMember = value;
                    this.InitializeComboBoxText();
                }
                else
                {
                    base.OnCommonChange();
                }
            }
        }

        private string ValueMemberInternal
        {
            set
            {
                this.InitializeValueMemberPropertyDescriptor(value);
                if (((value != null) && (value.Length > 0)) || base.Properties.ContainsObject(PropComboBoxCellValueMember))
                {
                    base.Properties.SetObject(PropComboBoxCellValueMember, value);
                }
            }
        }

        private PropertyDescriptor ValueMemberProperty
        {
            get
            {
                return (PropertyDescriptor) base.Properties.GetObject(PropComboBoxCellValueMemberProp);
            }
            set
            {
                if ((value != null) || base.Properties.ContainsObject(PropComboBoxCellValueMemberProp))
                {
                    base.Properties.SetObject(PropComboBoxCellValueMemberProp, value);
                }
            }
        }

        public override System.Type ValueType
        {
            get
            {
                if (this.ValueMemberProperty != null)
                {
                    return this.ValueMemberProperty.PropertyType;
                }
                if (this.DisplayMemberProperty != null)
                {
                    return this.DisplayMemberProperty.PropertyType;
                }
                System.Type valueType = base.ValueType;
                if (valueType != null)
                {
                    return valueType;
                }
                return defaultValueType;
            }
        }

        private class DataGridViewComboBoxCellRenderer
        {
            private static readonly VisualStyleElement ComboBoxBorder = VisualStyleElement.ComboBox.Border.Normal;
            private static readonly VisualStyleElement ComboBoxDropDownButtonLeft = VisualStyleElement.ComboBox.DropDownButtonLeft.Normal;
            private static readonly VisualStyleElement ComboBoxDropDownButtonRight = VisualStyleElement.ComboBox.DropDownButtonRight.Normal;
            private static readonly VisualStyleElement ComboBoxReadOnlyButton = VisualStyleElement.ComboBox.ReadOnlyButton.Normal;
            [ThreadStatic]
            private static System.Windows.Forms.VisualStyles.VisualStyleRenderer visualStyleRenderer;

            private DataGridViewComboBoxCellRenderer()
            {
            }

            public static void DrawBorder(Graphics g, Rectangle bounds)
            {
                if (visualStyleRenderer == null)
                {
                    visualStyleRenderer = new System.Windows.Forms.VisualStyles.VisualStyleRenderer(ComboBoxBorder);
                }
                else
                {
                    visualStyleRenderer.SetParameters(ComboBoxBorder.ClassName, ComboBoxBorder.Part, ComboBoxBorder.State);
                }
                visualStyleRenderer.DrawBackground(g, bounds);
            }

            public static void DrawDropDownButton(Graphics g, Rectangle bounds, ComboBoxState state)
            {
                ComboBoxRenderer.DrawDropDownButton(g, bounds, state);
            }

            public static void DrawDropDownButton(Graphics g, Rectangle bounds, ComboBoxState state, bool rightToLeft)
            {
                if (rightToLeft)
                {
                    if (visualStyleRenderer == null)
                    {
                        visualStyleRenderer = new System.Windows.Forms.VisualStyles.VisualStyleRenderer(ComboBoxDropDownButtonLeft.ClassName, ComboBoxDropDownButtonLeft.Part, (int) state);
                    }
                    else
                    {
                        visualStyleRenderer.SetParameters(ComboBoxDropDownButtonLeft.ClassName, ComboBoxDropDownButtonLeft.Part, (int) state);
                    }
                }
                else if (visualStyleRenderer == null)
                {
                    visualStyleRenderer = new System.Windows.Forms.VisualStyles.VisualStyleRenderer(ComboBoxDropDownButtonRight.ClassName, ComboBoxDropDownButtonRight.Part, (int) state);
                }
                else
                {
                    visualStyleRenderer.SetParameters(ComboBoxDropDownButtonRight.ClassName, ComboBoxDropDownButtonRight.Part, (int) state);
                }
                visualStyleRenderer.DrawBackground(g, bounds);
            }

            public static void DrawReadOnlyButton(Graphics g, Rectangle bounds, ComboBoxState state)
            {
                if (visualStyleRenderer == null)
                {
                    visualStyleRenderer = new System.Windows.Forms.VisualStyles.VisualStyleRenderer(ComboBoxReadOnlyButton.ClassName, ComboBoxReadOnlyButton.Part, (int) state);
                }
                else
                {
                    visualStyleRenderer.SetParameters(ComboBoxReadOnlyButton.ClassName, ComboBoxReadOnlyButton.Part, (int) state);
                }
                visualStyleRenderer.DrawBackground(g, bounds);
            }

            public static void DrawTextBox(Graphics g, Rectangle bounds, ComboBoxState state)
            {
                ComboBoxRenderer.DrawTextBox(g, bounds, state);
            }

            public static System.Windows.Forms.VisualStyles.VisualStyleRenderer VisualStyleRenderer
            {
                get
                {
                    if (visualStyleRenderer == null)
                    {
                        visualStyleRenderer = new System.Windows.Forms.VisualStyles.VisualStyleRenderer(ComboBoxReadOnlyButton);
                    }
                    return visualStyleRenderer;
                }
            }
        }

        private sealed class ItemComparer : IComparer
        {
            private DataGridViewComboBoxCell dataGridViewComboBoxCell;

            public ItemComparer(DataGridViewComboBoxCell dataGridViewComboBoxCell)
            {
                this.dataGridViewComboBoxCell = dataGridViewComboBoxCell;
            }

            public int Compare(object item1, object item2)
            {
                if (item1 == null)
                {
                    if (item2 == null)
                    {
                        return 0;
                    }
                    return -1;
                }
                if (item2 == null)
                {
                    return 1;
                }
                string itemDisplayText = this.dataGridViewComboBoxCell.GetItemDisplayText(item1);
                string str2 = this.dataGridViewComboBoxCell.GetItemDisplayText(item2);
                return Application.CurrentCulture.CompareInfo.Compare(itemDisplayText, str2, CompareOptions.StringSort);
            }
        }

        [ListBindable(false)]
        public class ObjectCollection : IList, ICollection, IEnumerable
        {
            private IComparer comparer;
            private ArrayList items;
            private DataGridViewComboBoxCell owner;

            public ObjectCollection(DataGridViewComboBoxCell owner)
            {
                this.owner = owner;
            }

            public int Add(object item)
            {
                this.owner.CheckNoDataSource();
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }
                int index = this.InnerArray.Add(item);
                bool flag = false;
                if (this.owner.Sorted)
                {
                    try
                    {
                        this.InnerArray.Sort(this.Comparer);
                        index = this.InnerArray.IndexOf(item);
                        flag = true;
                    }
                    finally
                    {
                        if (!flag)
                        {
                            this.InnerArray.Remove(item);
                        }
                    }
                }
                this.owner.OnItemsCollectionChanged();
                return index;
            }

            public void AddRange(params object[] items)
            {
                this.owner.CheckNoDataSource();
                this.AddRangeInternal(items);
                this.owner.OnItemsCollectionChanged();
            }

            public void AddRange(DataGridViewComboBoxCell.ObjectCollection value)
            {
                this.owner.CheckNoDataSource();
                this.AddRangeInternal(value);
                this.owner.OnItemsCollectionChanged();
            }

            internal void AddRangeInternal(ICollection items)
            {
                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                using (IEnumerator enumerator = items.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current == null)
                        {
                            throw new InvalidOperationException(System.Windows.Forms.SR.GetString("InvalidNullItemInCollection"));
                        }
                    }
                }
                this.InnerArray.AddRange(items);
                if (this.owner.Sorted)
                {
                    this.InnerArray.Sort(this.Comparer);
                }
            }

            public void Clear()
            {
                if (this.InnerArray.Count > 0)
                {
                    this.owner.CheckNoDataSource();
                    this.InnerArray.Clear();
                    this.owner.OnItemsCollectionChanged();
                }
            }

            internal void ClearInternal()
            {
                this.InnerArray.Clear();
            }

            public bool Contains(object value)
            {
                return (this.IndexOf(value) != -1);
            }

            public void CopyTo(object[] destination, int arrayIndex)
            {
                int count = this.InnerArray.Count;
                for (int i = 0; i < count; i++)
                {
                    destination[i + arrayIndex] = this.InnerArray[i];
                }
            }

            public IEnumerator GetEnumerator()
            {
                return this.InnerArray.GetEnumerator();
            }

            public int IndexOf(object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                return this.InnerArray.IndexOf(value);
            }

            public void Insert(int index, object item)
            {
                this.owner.CheckNoDataSource();
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }
                if ((index < 0) || (index > this.InnerArray.Count))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                if (this.owner.Sorted)
                {
                    this.Add(item);
                }
                else
                {
                    this.InnerArray.Insert(index, item);
                    this.owner.OnItemsCollectionChanged();
                }
            }

            public void Remove(object value)
            {
                int index = this.InnerArray.IndexOf(value);
                if (index != -1)
                {
                    this.RemoveAt(index);
                }
            }

            public void RemoveAt(int index)
            {
                this.owner.CheckNoDataSource();
                if ((index < 0) || (index >= this.InnerArray.Count))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                this.InnerArray.RemoveAt(index);
                this.owner.OnItemsCollectionChanged();
            }

            internal void SortInternal()
            {
                this.InnerArray.Sort(this.Comparer);
            }

            void ICollection.CopyTo(Array destination, int index)
            {
                int count = this.InnerArray.Count;
                for (int i = 0; i < count; i++)
                {
                    destination.SetValue(this.InnerArray[i], (int) (i + index));
                }
            }

            int IList.Add(object item)
            {
                return this.Add(item);
            }

            private IComparer Comparer
            {
                get
                {
                    if (this.comparer == null)
                    {
                        this.comparer = new DataGridViewComboBoxCell.ItemComparer(this.owner);
                    }
                    return this.comparer;
                }
            }

            public int Count
            {
                get
                {
                    return this.InnerArray.Count;
                }
            }

            internal ArrayList InnerArray
            {
                get
                {
                    if (this.items == null)
                    {
                        this.items = new ArrayList();
                    }
                    return this.items;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public virtual object this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.InnerArray.Count))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    return this.InnerArray[index];
                }
                set
                {
                    this.owner.CheckNoDataSource();
                    if (value == null)
                    {
                        throw new ArgumentNullException("value");
                    }
                    if ((index < 0) || (index >= this.InnerArray.Count))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    this.InnerArray[index] = value;
                    this.owner.OnItemsCollectionChanged();
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return this;
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }
        }
    }
}

