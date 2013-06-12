namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DefaultProperty("Header"), DesignTimeVisible(false), ToolboxItem(false)]
    public abstract class DataGridColumnStyle : Component, IDataGridColumnStyleEditingNotificationService
    {
        private HorizontalAlignment alignment;
        private System.Windows.Forms.DataGridTableStyle dataGridTableStyle;
        private static readonly object EventAlignment = new object();
        private static readonly object EventHeaderText = new object();
        private static readonly object EventMappingName = new object();
        private static readonly object EventNullText = new object();
        private static readonly object EventPropertyDescriptor = new object();
        private static readonly object EventReadOnly = new object();
        private static readonly object EventWidth = new object();
        private Font font;
        internal int fontHeight;
        private AccessibleObject headerAccessibleObject;
        private string headerName;
        private bool invalid;
        private bool isDefault;
        private string mappingName;
        private string nullText;
        private System.ComponentModel.PropertyDescriptor propertyDescriptor;
        private bool readOnly;
        private bool updating;
        internal int width;

        public event EventHandler AlignmentChanged
        {
            add
            {
                base.Events.AddHandler(EventAlignment, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventAlignment, value);
            }
        }

        public event EventHandler FontChanged
        {
            add
            {
            }
            remove
            {
            }
        }

        public event EventHandler HeaderTextChanged
        {
            add
            {
                base.Events.AddHandler(EventHeaderText, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventHeaderText, value);
            }
        }

        public event EventHandler MappingNameChanged
        {
            add
            {
                base.Events.AddHandler(EventMappingName, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMappingName, value);
            }
        }

        public event EventHandler NullTextChanged
        {
            add
            {
                base.Events.AddHandler(EventNullText, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventNullText, value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler PropertyDescriptorChanged
        {
            add
            {
                base.Events.AddHandler(EventPropertyDescriptor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPropertyDescriptor, value);
            }
        }

        public event EventHandler ReadOnlyChanged
        {
            add
            {
                base.Events.AddHandler(EventReadOnly, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventReadOnly, value);
            }
        }

        public event EventHandler WidthChanged
        {
            add
            {
                base.Events.AddHandler(EventWidth, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventWidth, value);
            }
        }

        public DataGridColumnStyle()
        {
            this.fontHeight = -1;
            this.mappingName = "";
            this.headerName = "";
            this.nullText = System.Windows.Forms.SR.GetString("DataGridNullText");
            this.width = -1;
        }

        public DataGridColumnStyle(System.ComponentModel.PropertyDescriptor prop) : this()
        {
            this.PropertyDescriptor = prop;
            if (prop != null)
            {
                this.readOnly = prop.IsReadOnly;
            }
        }

        internal DataGridColumnStyle(System.ComponentModel.PropertyDescriptor prop, bool isDefault) : this(prop)
        {
            this.isDefault = isDefault;
            if (isDefault)
            {
                this.headerName = prop.Name;
                this.mappingName = prop.Name;
            }
        }

        protected internal abstract void Abort(int rowNum);
        protected void BeginUpdate()
        {
            this.updating = true;
        }

        protected void CheckValidDataSource(CurrencyManager value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value", "DataGridColumnStyle.CheckValidDataSource(DataSource value), value == null");
            }
            if (this.PropertyDescriptor == null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridColumnUnbound", new object[] { this.HeaderText }));
            }
        }

        protected internal virtual void ColumnStartedEditing(Control editingControl)
        {
            this.DataGridTableStyle.DataGrid.ColumnStartedEditing(editingControl);
        }

        protected internal abstract bool Commit(CurrencyManager dataSource, int rowNum);
        protected internal virtual void ConcedeFocus()
        {
        }

        protected virtual AccessibleObject CreateHeaderAccessibleObject()
        {
            return new DataGridColumnHeaderAccessibleObject(this);
        }

        protected internal virtual void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
        {
            this.Edit(source, rowNum, bounds, readOnly, null, true);
        }

        protected internal virtual void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string displayText)
        {
            this.Edit(source, rowNum, bounds, readOnly, displayText, true);
        }

        protected internal abstract void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string displayText, bool cellIsVisible);
        protected void EndUpdate()
        {
            this.updating = false;
            if (this.invalid)
            {
                this.invalid = false;
                this.Invalidate();
            }
        }

        protected internal virtual void EnterNullValue()
        {
        }

        protected internal virtual object GetColumnValueAtRow(CurrencyManager source, int rowNum)
        {
            this.CheckValidDataSource(source);
            if (this.PropertyDescriptor == null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridColumnNoPropertyDescriptor"));
            }
            return this.PropertyDescriptor.GetValue(source[rowNum]);
        }

        internal virtual string GetDisplayText(object value)
        {
            return value.ToString();
        }

        protected internal abstract int GetMinimumHeight();
        protected internal abstract int GetPreferredHeight(Graphics g, object value);
        protected internal abstract Size GetPreferredSize(Graphics g, object value);
        protected virtual void Invalidate()
        {
            if (this.updating)
            {
                this.invalid = true;
            }
            else
            {
                System.Windows.Forms.DataGridTableStyle dataGridTableStyle = this.DataGridTableStyle;
                if (dataGridTableStyle != null)
                {
                    dataGridTableStyle.InvalidateColumn(this);
                }
            }
        }

        internal virtual bool KeyPress(int rowNum, Keys keyData)
        {
            if (this.ReadOnly || (((this.DataGridTableStyle != null) && (this.DataGridTableStyle.DataGrid != null)) && this.DataGridTableStyle.DataGrid.ReadOnly))
            {
                return false;
            }
            if ((keyData != (Keys.Control | Keys.NumPad0)) && (keyData != (Keys.Control | Keys.D0)))
            {
                return false;
            }
            this.EnterNullValue();
            return true;
        }

        internal virtual bool MouseDown(int rowNum, int x, int y)
        {
            return false;
        }

        private void OnAlignmentChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventAlignment] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnHeaderTextChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventHeaderText] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnMappingNameChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventMappingName] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnNullTextChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventNullText] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnPropertyDescriptorChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventPropertyDescriptor] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnReadOnlyChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventReadOnly] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnWidthChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventWidth] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal abstract void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum);
        protected internal abstract void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, bool alignToRight);
        protected internal virtual void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
        {
            this.Paint(g, bounds, source, rowNum, alignToRight);
        }

        protected internal virtual void ReleaseHostedControl()
        {
        }

        public void ResetHeaderText()
        {
            this.HeaderText = "";
        }

        private void ResetNullText()
        {
            this.NullText = System.Windows.Forms.SR.GetString("DataGridNullText");
        }

        protected internal virtual void SetColumnValueAtRow(CurrencyManager source, int rowNum, object value)
        {
            this.CheckValidDataSource(source);
            if (source.Position != rowNum)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridColumnListManagerPosition"), "rowNum");
            }
            if (source[rowNum] is IEditableObject)
            {
                ((IEditableObject) source[rowNum]).BeginEdit();
            }
            this.PropertyDescriptor.SetValue(source[rowNum], value);
        }

        protected virtual void SetDataGrid(DataGrid value)
        {
            this.SetDataGridInColumn(value);
        }

        protected virtual void SetDataGridInColumn(DataGrid value)
        {
            if ((this.PropertyDescriptor == null) && (value != null))
            {
                CurrencyManager listManager = value.ListManager;
                if (listManager != null)
                {
                    PropertyDescriptorCollection itemProperties = listManager.GetItemProperties();
                    int count = itemProperties.Count;
                    for (int i = 0; i < itemProperties.Count; i++)
                    {
                        System.ComponentModel.PropertyDescriptor descriptor = itemProperties[i];
                        if (!typeof(IList).IsAssignableFrom(descriptor.PropertyType) && descriptor.Name.Equals(this.HeaderText))
                        {
                            this.PropertyDescriptor = descriptor;
                            return;
                        }
                    }
                }
            }
        }

        internal void SetDataGridInternalInColumn(DataGrid value)
        {
            if ((value != null) && !value.Initializing)
            {
                this.SetDataGridInColumn(value);
            }
        }

        internal void SetDataGridTableInColumn(System.Windows.Forms.DataGridTableStyle value, bool force)
        {
            if (((this.dataGridTableStyle == null) || !this.dataGridTableStyle.Equals(value)) || force)
            {
                if (((value != null) && (value.DataGrid != null)) && !value.DataGrid.Initializing)
                {
                    this.SetDataGridInColumn(value.DataGrid);
                }
                this.dataGridTableStyle = value;
            }
        }

        private bool ShouldSerializeFont()
        {
            return (this.font != null);
        }

        private bool ShouldSerializeHeaderText()
        {
            return (this.headerName.Length != 0);
        }

        private bool ShouldSerializeNullText()
        {
            return !System.Windows.Forms.SR.GetString("DataGridNullText").Equals(this.nullText);
        }

        void IDataGridColumnStyleEditingNotificationService.ColumnStartedEditing(Control editingControl)
        {
            this.ColumnStartedEditing(editingControl);
        }

        protected internal virtual void UpdateUI(CurrencyManager source, int rowNum, string displayText)
        {
        }

        [DefaultValue(0), Localizable(true), System.Windows.Forms.SRCategory("CatDisplay")]
        public virtual HorizontalAlignment Alignment
        {
            get
            {
                return this.alignment;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DataGridLineStyle));
                }
                if (this.alignment != value)
                {
                    this.alignment = value;
                    this.OnAlignmentChanged(EventArgs.Empty);
                    this.Invalidate();
                }
            }
        }

        [Browsable(false)]
        public virtual System.Windows.Forms.DataGridTableStyle DataGridTableStyle
        {
            get
            {
                return this.dataGridTableStyle;
            }
        }

        protected int FontHeight
        {
            get
            {
                if (this.fontHeight != -1)
                {
                    return this.fontHeight;
                }
                if (this.DataGridTableStyle != null)
                {
                    return this.DataGridTableStyle.DataGrid.FontHeight;
                }
                return System.Windows.Forms.DataGridTableStyle.defaultFontHeight;
            }
        }

        [Browsable(false)]
        public AccessibleObject HeaderAccessibleObject
        {
            get
            {
                if (this.headerAccessibleObject == null)
                {
                    this.headerAccessibleObject = this.CreateHeaderAccessibleObject();
                }
                return this.headerAccessibleObject;
            }
        }

        [Localizable(true), System.Windows.Forms.SRCategory("CatDisplay")]
        public virtual string HeaderText
        {
            get
            {
                return this.headerName;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                if (!this.headerName.Equals(value))
                {
                    this.headerName = value;
                    this.OnHeaderTextChanged(EventArgs.Empty);
                    if (this.PropertyDescriptor != null)
                    {
                        this.Invalidate();
                    }
                }
            }
        }

        [DefaultValue(""), Localizable(true), Editor("System.Windows.Forms.Design.DataGridColumnStyleMappingNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string MappingName
        {
            get
            {
                return this.mappingName;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                if (!this.mappingName.Equals(value))
                {
                    string mappingName = this.mappingName;
                    this.mappingName = value;
                    try
                    {
                        if (this.dataGridTableStyle != null)
                        {
                            this.dataGridTableStyle.GridColumnStyles.CheckForMappingNameDuplicates(this);
                        }
                    }
                    catch
                    {
                        this.mappingName = mappingName;
                        throw;
                    }
                    this.OnMappingNameChanged(EventArgs.Empty);
                }
            }
        }

        [Localizable(true), System.Windows.Forms.SRCategory("CatDisplay")]
        public virtual string NullText
        {
            get
            {
                return this.nullText;
            }
            set
            {
                if ((this.nullText == null) || !this.nullText.Equals(value))
                {
                    this.nullText = value;
                    this.OnNullTextChanged(EventArgs.Empty);
                    this.Invalidate();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), DefaultValue((string) null)]
        public virtual System.ComponentModel.PropertyDescriptor PropertyDescriptor
        {
            get
            {
                return this.propertyDescriptor;
            }
            set
            {
                if (this.propertyDescriptor != value)
                {
                    this.propertyDescriptor = value;
                    this.OnPropertyDescriptorChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(false)]
        public virtual bool ReadOnly
        {
            get
            {
                return this.readOnly;
            }
            set
            {
                if (this.readOnly != value)
                {
                    this.readOnly = value;
                    this.OnReadOnlyChanged(EventArgs.Empty);
                }
            }
        }

        internal virtual bool WantArrows
        {
            get
            {
                return false;
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), Localizable(true), DefaultValue(100)]
        public virtual int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                if (this.width != value)
                {
                    this.width = value;
                    DataGrid grid = (this.DataGridTableStyle == null) ? null : this.DataGridTableStyle.DataGrid;
                    if (grid != null)
                    {
                        grid.PerformLayout();
                        grid.InvalidateInside();
                    }
                    this.OnWidthChanged(EventArgs.Empty);
                }
            }
        }

        protected class CompModSwitches
        {
            private static TraceSwitch dgEditColumnEditing;

            public static TraceSwitch DGEditColumnEditing
            {
                get
                {
                    if (dgEditColumnEditing == null)
                    {
                        dgEditColumnEditing = new TraceSwitch("DGEditColumnEditing", "Editing related tracing");
                    }
                    return dgEditColumnEditing;
                }
            }
        }

        [ComVisible(true)]
        protected class DataGridColumnHeaderAccessibleObject : AccessibleObject
        {
            private DataGridColumnStyle owner;

            public DataGridColumnHeaderAccessibleObject()
            {
            }

            public DataGridColumnHeaderAccessibleObject(DataGridColumnStyle owner) : this()
            {
                this.owner = owner;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation navdir)
            {
                switch (navdir)
                {
                    case AccessibleNavigation.Up:
                    case AccessibleNavigation.Left:
                    case AccessibleNavigation.Previous:
                        return this.Parent.GetChild((1 + this.Owner.dataGridTableStyle.GridColumnStyles.IndexOf(this.Owner)) - 1);

                    case AccessibleNavigation.Down:
                    case AccessibleNavigation.Right:
                    case AccessibleNavigation.Next:
                        return this.Parent.GetChild((1 + this.Owner.dataGridTableStyle.GridColumnStyles.IndexOf(this.Owner)) + 1);
                }
                return null;
            }

            public override Rectangle Bounds
            {
                get
                {
                    if (this.owner.PropertyDescriptor == null)
                    {
                        return Rectangle.Empty;
                    }
                    System.Windows.Forms.DataGrid dataGrid = this.DataGrid;
                    if (dataGrid.DataGridRowsLength == 0)
                    {
                        return Rectangle.Empty;
                    }
                    GridColumnStylesCollection gridColumnStyles = this.owner.dataGridTableStyle.GridColumnStyles;
                    int col = -1;
                    for (int i = 0; i < gridColumnStyles.Count; i++)
                    {
                        if (gridColumnStyles[i] == this.owner)
                        {
                            col = i;
                            break;
                        }
                    }
                    Rectangle cellBounds = dataGrid.GetCellBounds(0, col);
                    cellBounds.Y = dataGrid.GetColumnHeadersRect().Y;
                    return dataGrid.RectangleToScreen(cellBounds);
                }
            }

            private System.Windows.Forms.DataGrid DataGrid
            {
                get
                {
                    return this.owner.dataGridTableStyle.dataGrid;
                }
            }

            public override string Name
            {
                get
                {
                    return this.Owner.headerName;
                }
            }

            protected DataGridColumnStyle Owner
            {
                get
                {
                    return this.owner;
                }
            }

            public override AccessibleObject Parent
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return this.DataGrid.AccessibilityObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.ColumnHeader;
                }
            }
        }
    }
}

