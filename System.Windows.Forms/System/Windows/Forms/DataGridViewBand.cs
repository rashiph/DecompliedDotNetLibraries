namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    public class DataGridViewBand : DataGridViewElement, ICloneable, IDisposable
    {
        private int bandIndex = -1;
        internal bool bandIsRow;
        private int cachedThickness;
        internal const int maxBandThickness = 0x10000;
        internal const int minBandThickness = 2;
        private int minimumThickness;
        private static readonly int PropContextMenuStrip = PropertyStore.CreateKey();
        private static readonly int PropDefaultCellStyle = PropertyStore.CreateKey();
        private static readonly int PropDefaultHeaderCellType = PropertyStore.CreateKey();
        private static readonly int PropDividerThickness = PropertyStore.CreateKey();
        private PropertyStore propertyStore = new PropertyStore();
        private static readonly int PropHeaderCell = PropertyStore.CreateKey();
        private static readonly int PropUserData = PropertyStore.CreateKey();
        private int thickness;

        internal DataGridViewBand()
        {
        }

        public virtual object Clone()
        {
            DataGridViewBand dataGridViewBand = (DataGridViewBand) Activator.CreateInstance(base.GetType());
            if (dataGridViewBand != null)
            {
                this.CloneInternal(dataGridViewBand);
            }
            return dataGridViewBand;
        }

        internal void CloneInternal(DataGridViewBand dataGridViewBand)
        {
            dataGridViewBand.propertyStore = new PropertyStore();
            dataGridViewBand.bandIndex = -1;
            dataGridViewBand.bandIsRow = this.bandIsRow;
            if ((!this.bandIsRow || (this.bandIndex >= 0)) || (base.DataGridView == null))
            {
                dataGridViewBand.StateInternal = this.State & ~(DataGridViewElementStates.Selected | DataGridViewElementStates.Displayed);
            }
            dataGridViewBand.thickness = this.Thickness;
            dataGridViewBand.MinimumThickness = this.MinimumThickness;
            dataGridViewBand.cachedThickness = this.CachedThickness;
            dataGridViewBand.DividerThickness = this.DividerThickness;
            dataGridViewBand.Tag = this.Tag;
            if (this.HasDefaultCellStyle)
            {
                dataGridViewBand.DefaultCellStyle = new DataGridViewCellStyle(this.DefaultCellStyle);
            }
            if (this.HasDefaultHeaderCellType)
            {
                dataGridViewBand.DefaultHeaderCellType = this.DefaultHeaderCellType;
            }
            if (this.ContextMenuStripInternal != null)
            {
                dataGridViewBand.ContextMenuStrip = this.ContextMenuStripInternal.Clone();
            }
        }

        private void DetachContextMenuStrip(object sender, EventArgs e)
        {
            this.ContextMenuStripInternal = null;
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

        ~DataGridViewBand()
        {
            this.Dispose(false);
        }

        internal void GetHeightInfo(int rowIndex, out int height, out int minimumHeight)
        {
            if (((base.DataGridView != null) && (base.DataGridView.VirtualMode || (base.DataGridView.DataSource != null))) && (base.DataGridView.AutoSizeRowsMode == DataGridViewAutoSizeRowsMode.None))
            {
                DataGridViewRowHeightInfoNeededEventArgs args = base.DataGridView.OnRowHeightInfoNeeded(rowIndex, this.thickness, this.minimumThickness);
                height = args.Height;
                minimumHeight = args.MinimumHeight;
            }
            else
            {
                height = this.thickness;
                minimumHeight = this.minimumThickness;
            }
        }

        protected override void OnDataGridViewChanged()
        {
            if (this.HasDefaultCellStyle)
            {
                if (base.DataGridView == null)
                {
                    this.DefaultCellStyle.RemoveScope(this.bandIsRow ? DataGridViewCellStyleScopes.Row : DataGridViewCellStyleScopes.Column);
                }
                else
                {
                    this.DefaultCellStyle.AddScope(base.DataGridView, this.bandIsRow ? DataGridViewCellStyleScopes.Row : DataGridViewCellStyleScopes.Column);
                }
            }
            base.OnDataGridViewChanged();
        }

        internal void OnStateChanged(DataGridViewElementStates elementState)
        {
            if (base.DataGridView != null)
            {
                if (this.bandIsRow)
                {
                    base.DataGridView.Rows.InvalidateCachedRowCount(elementState);
                    base.DataGridView.Rows.InvalidateCachedRowsHeight(elementState);
                    if (this.bandIndex != -1)
                    {
                        base.DataGridView.OnDataGridViewElementStateChanged(this, -1, elementState);
                    }
                }
                else
                {
                    base.DataGridView.Columns.InvalidateCachedColumnCount(elementState);
                    base.DataGridView.Columns.InvalidateCachedColumnsWidth(elementState);
                    base.DataGridView.OnDataGridViewElementStateChanged(this, -1, elementState);
                }
            }
        }

        private void OnStateChanging(DataGridViewElementStates elementState)
        {
            if (base.DataGridView != null)
            {
                if (this.bandIsRow)
                {
                    if (this.bandIndex != -1)
                    {
                        base.DataGridView.OnDataGridViewElementStateChanging(this, -1, elementState);
                    }
                }
                else
                {
                    base.DataGridView.OnDataGridViewElementStateChanging(this, -1, elementState);
                }
            }
        }

        private bool ShouldSerializeDefaultHeaderCellType()
        {
            System.Type type = (System.Type) this.Properties.GetObject(PropDefaultHeaderCellType);
            return (type != null);
        }

        internal bool ShouldSerializeResizable()
        {
            return ((this.State & DataGridViewElementStates.ResizableSet) != DataGridViewElementStates.None);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x24);
            builder.Append("DataGridViewBand { Index=");
            builder.Append(this.Index.ToString(CultureInfo.CurrentCulture));
            builder.Append(" }");
            return builder.ToString();
        }

        internal int CachedThickness
        {
            get
            {
                return this.cachedThickness;
            }
            set
            {
                this.cachedThickness = value;
            }
        }

        [DefaultValue((string) null)]
        public virtual System.Windows.Forms.ContextMenuStrip ContextMenuStrip
        {
            get
            {
                if (this.bandIsRow)
                {
                    return ((DataGridViewRow) this).GetContextMenuStrip(this.Index);
                }
                return this.ContextMenuStripInternal;
            }
            set
            {
                this.ContextMenuStripInternal = value;
            }
        }

        internal System.Windows.Forms.ContextMenuStrip ContextMenuStripInternal
        {
            get
            {
                return (System.Windows.Forms.ContextMenuStrip) this.Properties.GetObject(PropContextMenuStrip);
            }
            set
            {
                System.Windows.Forms.ContextMenuStrip strip = (System.Windows.Forms.ContextMenuStrip) this.Properties.GetObject(PropContextMenuStrip);
                if (strip != value)
                {
                    EventHandler handler = new EventHandler(this.DetachContextMenuStrip);
                    if (strip != null)
                    {
                        strip.Disposed -= handler;
                    }
                    this.Properties.SetObject(PropContextMenuStrip, value);
                    if (value != null)
                    {
                        value.Disposed += handler;
                    }
                    if (base.DataGridView != null)
                    {
                        base.DataGridView.OnBandContextMenuStripChanged(this);
                    }
                }
            }
        }

        [Browsable(false)]
        public virtual DataGridViewCellStyle DefaultCellStyle
        {
            get
            {
                DataGridViewCellStyle style = (DataGridViewCellStyle) this.Properties.GetObject(PropDefaultCellStyle);
                if (style == null)
                {
                    style = new DataGridViewCellStyle();
                    style.AddScope(base.DataGridView, this.bandIsRow ? DataGridViewCellStyleScopes.Row : DataGridViewCellStyleScopes.Column);
                    this.Properties.SetObject(PropDefaultCellStyle, style);
                }
                return style;
            }
            set
            {
                DataGridViewCellStyle defaultCellStyle = null;
                if (this.HasDefaultCellStyle)
                {
                    defaultCellStyle = this.DefaultCellStyle;
                    defaultCellStyle.RemoveScope(this.bandIsRow ? DataGridViewCellStyleScopes.Row : DataGridViewCellStyleScopes.Column);
                }
                if ((value != null) || this.Properties.ContainsObject(PropDefaultCellStyle))
                {
                    if (value != null)
                    {
                        value.AddScope(base.DataGridView, this.bandIsRow ? DataGridViewCellStyleScopes.Row : DataGridViewCellStyleScopes.Column);
                    }
                    this.Properties.SetObject(PropDefaultCellStyle, value);
                }
                if (((((defaultCellStyle != null) && (value == null)) || ((defaultCellStyle == null) && (value != null))) || (((defaultCellStyle != null) && (value != null)) && !defaultCellStyle.Equals(this.DefaultCellStyle))) && (base.DataGridView != null))
                {
                    base.DataGridView.OnBandDefaultCellStyleChanged(this);
                }
            }
        }

        [Browsable(false)]
        public System.Type DefaultHeaderCellType
        {
            get
            {
                System.Type type = (System.Type) this.Properties.GetObject(PropDefaultHeaderCellType);
                if (type != null)
                {
                    return type;
                }
                if (this.bandIsRow)
                {
                    return typeof(DataGridViewRowHeaderCell);
                }
                return typeof(DataGridViewColumnHeaderCell);
            }
            set
            {
                if ((value != null) || this.Properties.ContainsObject(PropDefaultHeaderCellType))
                {
                    if (!System.Type.GetType("System.Windows.Forms.DataGridViewHeaderCell").IsAssignableFrom(value))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_WrongType", new object[] { "DefaultHeaderCellType", "System.Windows.Forms.DataGridViewHeaderCell" }));
                    }
                    this.Properties.SetObject(PropDefaultHeaderCellType, value);
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool Displayed
        {
            get
            {
                return ((this.State & DataGridViewElementStates.Displayed) != DataGridViewElementStates.None);
            }
        }

        internal bool DisplayedInternal
        {
            set
            {
                if (value)
                {
                    base.StateInternal = this.State | DataGridViewElementStates.Displayed;
                }
                else
                {
                    base.StateInternal = this.State & ~DataGridViewElementStates.Displayed;
                }
                if (base.DataGridView != null)
                {
                    this.OnStateChanged(DataGridViewElementStates.Displayed);
                }
            }
        }

        internal int DividerThickness
        {
            get
            {
                bool flag;
                int integer = this.Properties.GetInteger(PropDividerThickness, out flag);
                if (!flag)
                {
                    return 0;
                }
                return integer;
            }
            set
            {
                if (value < 0)
                {
                    if (this.bandIsRow)
                    {
                        object[] objArray = new object[] { "DividerHeight", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("DividerHeight", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", objArray));
                    }
                    object[] args = new object[] { "DividerWidth", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("DividerWidth", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                if (value > 0x10000)
                {
                    if (this.bandIsRow)
                    {
                        object[] objArray3 = new object[] { "DividerHeight", value.ToString(CultureInfo.CurrentCulture), 0x10000.ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("DividerHeight", System.Windows.Forms.SR.GetString("InvalidHighBoundArgumentEx", objArray3));
                    }
                    object[] objArray4 = new object[] { "DividerWidth", value.ToString(CultureInfo.CurrentCulture), 0x10000.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("DividerWidth", System.Windows.Forms.SR.GetString("InvalidHighBoundArgumentEx", objArray4));
                }
                if (value != this.DividerThickness)
                {
                    this.Properties.SetInteger(PropDividerThickness, value);
                    if (base.DataGridView != null)
                    {
                        base.DataGridView.OnBandDividerThicknessChanged(this);
                    }
                }
            }
        }

        [DefaultValue(false)]
        public virtual bool Frozen
        {
            get
            {
                return ((this.State & DataGridViewElementStates.Frozen) != DataGridViewElementStates.None);
            }
            set
            {
                if (((this.State & DataGridViewElementStates.Frozen) != DataGridViewElementStates.None) != value)
                {
                    this.OnStateChanging(DataGridViewElementStates.Frozen);
                    if (value)
                    {
                        base.StateInternal = this.State | DataGridViewElementStates.Frozen;
                    }
                    else
                    {
                        base.StateInternal = this.State & ~DataGridViewElementStates.Frozen;
                    }
                    this.OnStateChanged(DataGridViewElementStates.Frozen);
                }
            }
        }

        [Browsable(false)]
        public bool HasDefaultCellStyle
        {
            get
            {
                return (this.Properties.ContainsObject(PropDefaultCellStyle) && (this.Properties.GetObject(PropDefaultCellStyle) != null));
            }
        }

        internal bool HasDefaultHeaderCellType
        {
            get
            {
                return (this.Properties.ContainsObject(PropDefaultHeaderCellType) && (this.Properties.GetObject(PropDefaultHeaderCellType) != null));
            }
        }

        internal bool HasHeaderCell
        {
            get
            {
                return (this.Properties.ContainsObject(PropHeaderCell) && (this.Properties.GetObject(PropHeaderCell) != null));
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        protected DataGridViewHeaderCell HeaderCellCore
        {
            get
            {
                DataGridViewHeaderCell cell = (DataGridViewHeaderCell) this.Properties.GetObject(PropHeaderCell);
                if (cell == null)
                {
                    cell = (DataGridViewHeaderCell) System.Windows.Forms.SecurityUtils.SecureCreateInstance(this.DefaultHeaderCellType);
                    cell.DataGridViewInternal = base.DataGridView;
                    if (this.bandIsRow)
                    {
                        cell.OwningRowInternal = (DataGridViewRow) this;
                        this.Properties.SetObject(PropHeaderCell, cell);
                        return cell;
                    }
                    DataGridViewColumn column = this as DataGridViewColumn;
                    cell.OwningColumnInternal = column;
                    this.Properties.SetObject(PropHeaderCell, cell);
                    if ((base.DataGridView != null) && (base.DataGridView.SortedColumn == column))
                    {
                        DataGridViewColumnHeaderCell cell2 = cell as DataGridViewColumnHeaderCell;
                        cell2.SortGlyphDirection = base.DataGridView.SortOrder;
                    }
                }
                return cell;
            }
            set
            {
                DataGridViewHeaderCell cell = (DataGridViewHeaderCell) this.Properties.GetObject(PropHeaderCell);
                if ((value != null) || this.Properties.ContainsObject(PropHeaderCell))
                {
                    if (cell != null)
                    {
                        cell.DataGridViewInternal = null;
                        if (this.bandIsRow)
                        {
                            cell.OwningRowInternal = null;
                        }
                        else
                        {
                            cell.OwningColumnInternal = null;
                            ((DataGridViewColumnHeaderCell) cell).SortGlyphDirectionInternal = SortOrder.None;
                        }
                    }
                    if (value != null)
                    {
                        if (this.bandIsRow)
                        {
                            if (!(value is DataGridViewRowHeaderCell))
                            {
                                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_WrongType", new object[] { "HeaderCell", "System.Windows.Forms.DataGridViewRowHeaderCell" }));
                            }
                            if (value.OwningRow != null)
                            {
                                value.OwningRow.HeaderCell = null;
                            }
                            value.OwningRowInternal = (DataGridViewRow) this;
                        }
                        else
                        {
                            if (!(value is DataGridViewColumnHeaderCell))
                            {
                                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_WrongType", new object[] { "HeaderCell", "System.Windows.Forms.DataGridViewColumnHeaderCell" }));
                            }
                            if (value.OwningColumn != null)
                            {
                                value.OwningColumn.HeaderCell = null;
                            }
                            value.OwningColumnInternal = (DataGridViewColumn) this;
                        }
                        value.DataGridViewInternal = base.DataGridView;
                    }
                    this.Properties.SetObject(PropHeaderCell, value);
                }
                if (((((value == null) && (cell != null)) || ((value != null) && (cell == null))) || (((value != null) && (cell != null)) && !cell.Equals(value))) && (base.DataGridView != null))
                {
                    base.DataGridView.OnBandHeaderCellChanged(this);
                }
            }
        }

        [Browsable(false)]
        public int Index
        {
            get
            {
                return this.bandIndex;
            }
        }

        internal int IndexInternal
        {
            set
            {
                this.bandIndex = value;
            }
        }

        [Browsable(false)]
        public virtual DataGridViewCellStyle InheritedStyle
        {
            get
            {
                return null;
            }
        }

        protected bool IsRow
        {
            get
            {
                return this.bandIsRow;
            }
        }

        internal int MinimumThickness
        {
            get
            {
                if (this.bandIsRow && (this.bandIndex > -1))
                {
                    int num;
                    int num2;
                    this.GetHeightInfo(this.bandIndex, out num, out num2);
                    return num2;
                }
                return this.minimumThickness;
            }
            set
            {
                if (this.minimumThickness != value)
                {
                    if (value < 2)
                    {
                        if (this.bandIsRow)
                        {
                            object[] objArray = new object[] { 2.ToString(CultureInfo.CurrentCulture) };
                            throw new ArgumentOutOfRangeException("MinimumHeight", value, System.Windows.Forms.SR.GetString("DataGridViewBand_MinimumHeightSmallerThanOne", objArray));
                        }
                        object[] args = new object[] { 2.ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("MinimumWidth", value, System.Windows.Forms.SR.GetString("DataGridViewBand_MinimumWidthSmallerThanOne", args));
                    }
                    if (this.Thickness < value)
                    {
                        if ((base.DataGridView != null) && !this.bandIsRow)
                        {
                            base.DataGridView.OnColumnMinimumWidthChanging((DataGridViewColumn) this, value);
                        }
                        this.Thickness = value;
                    }
                    this.minimumThickness = value;
                    if (base.DataGridView != null)
                    {
                        base.DataGridView.OnBandMinimumThicknessChanged(this);
                    }
                }
            }
        }

        internal PropertyStore Properties
        {
            get
            {
                return this.propertyStore;
            }
        }

        [DefaultValue(false)]
        public virtual bool ReadOnly
        {
            get
            {
                return (((this.State & DataGridViewElementStates.ReadOnly) != DataGridViewElementStates.None) || ((base.DataGridView != null) && base.DataGridView.ReadOnly));
            }
            set
            {
                if (base.DataGridView != null)
                {
                    if (!base.DataGridView.ReadOnly)
                    {
                        if (this.bandIsRow)
                        {
                            if (this.bandIndex == -1)
                            {
                                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertySetOnSharedRow", new object[] { "ReadOnly" }));
                            }
                            this.OnStateChanging(DataGridViewElementStates.ReadOnly);
                            base.DataGridView.SetReadOnlyRowCore(this.bandIndex, value);
                        }
                        else
                        {
                            this.OnStateChanging(DataGridViewElementStates.ReadOnly);
                            base.DataGridView.SetReadOnlyColumnCore(this.bandIndex, value);
                        }
                    }
                }
                else if (((this.State & DataGridViewElementStates.ReadOnly) != DataGridViewElementStates.None) != value)
                {
                    if (value)
                    {
                        if (this.bandIsRow)
                        {
                            foreach (DataGridViewCell cell in ((DataGridViewRow) this).Cells)
                            {
                                if (cell.ReadOnly)
                                {
                                    cell.ReadOnlyInternal = false;
                                }
                            }
                        }
                        base.StateInternal = this.State | DataGridViewElementStates.ReadOnly;
                    }
                    else
                    {
                        base.StateInternal = this.State & ~DataGridViewElementStates.ReadOnly;
                    }
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
                this.OnStateChanged(DataGridViewElementStates.ReadOnly);
            }
        }

        [Browsable(true)]
        public virtual DataGridViewTriState Resizable
        {
            get
            {
                if ((this.State & DataGridViewElementStates.ResizableSet) != DataGridViewElementStates.None)
                {
                    if ((this.State & DataGridViewElementStates.Resizable) == DataGridViewElementStates.None)
                    {
                        return DataGridViewTriState.False;
                    }
                    return DataGridViewTriState.True;
                }
                if (base.DataGridView == null)
                {
                    return DataGridViewTriState.NotSet;
                }
                if (!base.DataGridView.AllowUserToResizeColumns)
                {
                    return DataGridViewTriState.False;
                }
                return DataGridViewTriState.True;
            }
            set
            {
                DataGridViewTriState resizable = this.Resizable;
                if (value == DataGridViewTriState.NotSet)
                {
                    base.StateInternal = this.State & ~DataGridViewElementStates.ResizableSet;
                }
                else
                {
                    base.StateInternal = this.State | DataGridViewElementStates.ResizableSet;
                    if (((this.State & DataGridViewElementStates.Resizable) != DataGridViewElementStates.None) != (value == DataGridViewTriState.True))
                    {
                        if (value == DataGridViewTriState.True)
                        {
                            base.StateInternal = this.State | DataGridViewElementStates.Resizable;
                        }
                        else
                        {
                            base.StateInternal = this.State & ~DataGridViewElementStates.Resizable;
                        }
                    }
                }
                if (resizable != this.Resizable)
                {
                    this.OnStateChanged(DataGridViewElementStates.Resizable);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual bool Selected
        {
            get
            {
                return ((this.State & DataGridViewElementStates.Selected) != DataGridViewElementStates.None);
            }
            set
            {
                if (base.DataGridView != null)
                {
                    if (!this.bandIsRow)
                    {
                        if ((base.DataGridView.SelectionMode == DataGridViewSelectionMode.FullColumnSelect) || (base.DataGridView.SelectionMode == DataGridViewSelectionMode.ColumnHeaderSelect))
                        {
                            base.DataGridView.SetSelectedColumnCoreInternal(this.bandIndex, value);
                        }
                    }
                    else
                    {
                        if (this.bandIndex == -1)
                        {
                            throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_InvalidPropertySetOnSharedRow", new object[] { "Selected" }));
                        }
                        if ((base.DataGridView.SelectionMode == DataGridViewSelectionMode.FullRowSelect) || (base.DataGridView.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect))
                        {
                            base.DataGridView.SetSelectedRowCoreInternal(this.bandIndex, value);
                        }
                    }
                }
                else if (value)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewBand_CannotSelect"));
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
                    this.OnStateChanged(DataGridViewElementStates.Selected);
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Tag
        {
            get
            {
                return this.Properties.GetObject(PropUserData);
            }
            set
            {
                if ((value != null) || this.Properties.ContainsObject(PropUserData))
                {
                    this.Properties.SetObject(PropUserData, value);
                }
            }
        }

        internal int Thickness
        {
            get
            {
                if (this.bandIsRow && (this.bandIndex > -1))
                {
                    int num;
                    int num2;
                    this.GetHeightInfo(this.bandIndex, out num, out num2);
                    return num;
                }
                return this.thickness;
            }
            set
            {
                int minimumThickness = this.MinimumThickness;
                if (value < minimumThickness)
                {
                    value = minimumThickness;
                }
                if (value > 0x10000)
                {
                    if (this.bandIsRow)
                    {
                        object[] objArray = new object[] { "Height", value.ToString(CultureInfo.CurrentCulture), 0x10000.ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("Height", System.Windows.Forms.SR.GetString("InvalidHighBoundArgumentEx", objArray));
                    }
                    object[] args = new object[] { "Width", value.ToString(CultureInfo.CurrentCulture), 0x10000.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("Width", System.Windows.Forms.SR.GetString("InvalidHighBoundArgumentEx", args));
                }
                bool flag = true;
                if (this.bandIsRow)
                {
                    if ((base.DataGridView != null) && (base.DataGridView.AutoSizeRowsMode != DataGridViewAutoSizeRowsMode.None))
                    {
                        this.cachedThickness = value;
                        flag = false;
                    }
                }
                else
                {
                    DataGridViewColumn dataGridViewColumn = (DataGridViewColumn) this;
                    DataGridViewAutoSizeColumnMode inheritedAutoSizeMode = dataGridViewColumn.InheritedAutoSizeMode;
                    if (((inheritedAutoSizeMode != DataGridViewAutoSizeColumnMode.Fill) && (inheritedAutoSizeMode != DataGridViewAutoSizeColumnMode.None)) && (inheritedAutoSizeMode != DataGridViewAutoSizeColumnMode.NotSet))
                    {
                        this.cachedThickness = value;
                        flag = false;
                    }
                    else if (((inheritedAutoSizeMode == DataGridViewAutoSizeColumnMode.Fill) && (base.DataGridView != null)) && dataGridViewColumn.Visible)
                    {
                        IntPtr handle = base.DataGridView.Handle;
                        base.DataGridView.AdjustFillingColumn(dataGridViewColumn, value);
                        flag = false;
                    }
                }
                if (flag && (this.thickness != value))
                {
                    if (base.DataGridView != null)
                    {
                        base.DataGridView.OnBandThicknessChanging();
                    }
                    this.ThicknessInternal = value;
                }
            }
        }

        internal int ThicknessInternal
        {
            get
            {
                return this.thickness;
            }
            set
            {
                this.thickness = value;
                if (base.DataGridView != null)
                {
                    base.DataGridView.OnBandThicknessChanged(this);
                }
            }
        }

        [DefaultValue(true)]
        public virtual bool Visible
        {
            get
            {
                return ((this.State & DataGridViewElementStates.Visible) != DataGridViewElementStates.None);
            }
            set
            {
                if (((this.State & DataGridViewElementStates.Visible) != DataGridViewElementStates.None) != value)
                {
                    if ((((base.DataGridView != null) && this.bandIsRow) && ((base.DataGridView.NewRowIndex != -1) && (base.DataGridView.NewRowIndex == this.bandIndex))) && !value)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewBand_NewRowCannotBeInvisible"));
                    }
                    this.OnStateChanging(DataGridViewElementStates.Visible);
                    if (value)
                    {
                        base.StateInternal = this.State | DataGridViewElementStates.Visible;
                    }
                    else
                    {
                        base.StateInternal = this.State & ~DataGridViewElementStates.Visible;
                    }
                    this.OnStateChanged(DataGridViewElementStates.Visible);
                }
            }
        }
    }
}

