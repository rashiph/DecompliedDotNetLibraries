namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Text;

    [ToolboxItem(false), TypeConverter(typeof(DataGridViewColumnConverter)), DesignTimeVisible(false), Designer("System.Windows.Forms.Design.DataGridViewColumnDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class DataGridViewColumn : DataGridViewBand, IComponent, IDisposable
    {
        private DataGridViewAutoSizeColumnMode autoSizeMode;
        private TypeConverter boundColumnConverter;
        private int boundColumnIndex;
        private DataGridViewCell cellTemplate;
        private const byte DATAGRIDVIEWCOLUMN_automaticSort = 1;
        private const float DATAGRIDVIEWCOLUMN_defaultFillWeight = 100f;
        private const int DATAGRIDVIEWCOLUMN_defaultMinColumnThickness = 5;
        private const int DATAGRIDVIEWCOLUMN_defaultWidth = 100;
        private const byte DATAGRIDVIEWCOLUMN_displayIndexHasChangedInternal = 0x10;
        private const byte DATAGRIDVIEWCOLUMN_isBrowsableInternal = 8;
        private const byte DATAGRIDVIEWCOLUMN_isDataBound = 4;
        private const byte DATAGRIDVIEWCOLUMN_programmaticSort = 2;
        private string dataPropertyName;
        private int desiredFillWidth;
        private int desiredMinimumWidth;
        private int displayIndex;
        private float fillWeight;
        private byte flags;
        private string name;
        private static readonly int PropDataGridViewColumnValueType = PropertyStore.CreateKey();
        private ISite site;
        private float usedFillWeight;

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public event EventHandler Disposed;

        public DataGridViewColumn() : this(null)
        {
        }

        public DataGridViewColumn(DataGridViewCell cellTemplate)
        {
            this.boundColumnIndex = -1;
            this.dataPropertyName = string.Empty;
            this.fillWeight = 100f;
            this.usedFillWeight = 100f;
            base.Thickness = 100;
            base.MinimumThickness = 5;
            this.name = string.Empty;
            base.bandIsRow = false;
            this.displayIndex = -1;
            this.cellTemplate = cellTemplate;
            this.autoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
        }

        public override object Clone()
        {
            DataGridViewColumn dataGridViewColumn = (DataGridViewColumn) Activator.CreateInstance(base.GetType());
            if (dataGridViewColumn != null)
            {
                this.CloneInternal(dataGridViewColumn);
            }
            return dataGridViewColumn;
        }

        internal void CloneInternal(DataGridViewColumn dataGridViewColumn)
        {
            base.CloneInternal(dataGridViewColumn);
            dataGridViewColumn.name = this.Name;
            dataGridViewColumn.displayIndex = -1;
            dataGridViewColumn.HeaderText = this.HeaderText;
            dataGridViewColumn.DataPropertyName = this.DataPropertyName;
            if (dataGridViewColumn.CellTemplate != null)
            {
                dataGridViewColumn.cellTemplate = (DataGridViewCell) this.CellTemplate.Clone();
            }
            else
            {
                dataGridViewColumn.cellTemplate = null;
            }
            if (base.HasHeaderCell)
            {
                dataGridViewColumn.HeaderCell = (DataGridViewColumnHeaderCell) this.HeaderCell.Clone();
            }
            dataGridViewColumn.AutoSizeMode = this.AutoSizeMode;
            dataGridViewColumn.SortMode = this.SortMode;
            dataGridViewColumn.FillWeightInternal = this.FillWeight;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    lock (this)
                    {
                        if ((this.site != null) && (this.site.Container != null))
                        {
                            this.site.Container.Remove(this);
                        }
                        if (this.disposed != null)
                        {
                            this.disposed(this, EventArgs.Empty);
                        }
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal DataGridViewAutoSizeColumnMode GetInheritedAutoSizeMode(DataGridView dataGridView)
        {
            if ((dataGridView == null) || (this.autoSizeMode != DataGridViewAutoSizeColumnMode.NotSet))
            {
                return this.autoSizeMode;
            }
            switch (dataGridView.AutoSizeColumnsMode)
            {
                case DataGridViewAutoSizeColumnsMode.ColumnHeader:
                    return DataGridViewAutoSizeColumnMode.ColumnHeader;

                case DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader:
                    return DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;

                case DataGridViewAutoSizeColumnsMode.AllCells:
                    return DataGridViewAutoSizeColumnMode.AllCells;

                case DataGridViewAutoSizeColumnsMode.DisplayedCellsExceptHeader:
                    return DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;

                case DataGridViewAutoSizeColumnsMode.DisplayedCells:
                    return DataGridViewAutoSizeColumnMode.DisplayedCells;

                case DataGridViewAutoSizeColumnsMode.Fill:
                    return DataGridViewAutoSizeColumnMode.Fill;
            }
            return DataGridViewAutoSizeColumnMode.None;
        }

        public virtual int GetPreferredWidth(DataGridViewAutoSizeColumnMode autoSizeColumnMode, bool fixedHeight)
        {
            if (((autoSizeColumnMode == DataGridViewAutoSizeColumnMode.NotSet) || (autoSizeColumnMode == DataGridViewAutoSizeColumnMode.None)) || (autoSizeColumnMode == DataGridViewAutoSizeColumnMode.Fill))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridView_NeedColumnAutoSizingCriteria", new object[] { "autoSizeColumnMode" }));
            }
            switch (autoSizeColumnMode)
            {
                case DataGridViewAutoSizeColumnMode.NotSet:
                case DataGridViewAutoSizeColumnMode.None:
                case DataGridViewAutoSizeColumnMode.ColumnHeader:
                case DataGridViewAutoSizeColumnMode.AllCellsExceptHeader:
                case DataGridViewAutoSizeColumnMode.AllCells:
                case DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader:
                case DataGridViewAutoSizeColumnMode.DisplayedCells:
                case DataGridViewAutoSizeColumnMode.Fill:
                {
                    int preferredWidth;
                    int num3;
                    DataGridViewRow row;
                    DataGridView dataGridView = base.DataGridView;
                    if (dataGridView == null)
                    {
                        return -1;
                    }
                    DataGridViewAutoSizeColumnCriteriaInternal internal2 = (DataGridViewAutoSizeColumnCriteriaInternal) autoSizeColumnMode;
                    int num = 0;
                    if (dataGridView.ColumnHeadersVisible && ((internal2 & DataGridViewAutoSizeColumnCriteriaInternal.Header) != DataGridViewAutoSizeColumnCriteriaInternal.NotSet))
                    {
                        if (fixedHeight)
                        {
                            preferredWidth = this.HeaderCell.GetPreferredWidth(-1, dataGridView.ColumnHeadersHeight);
                        }
                        else
                        {
                            preferredWidth = this.HeaderCell.GetPreferredSize(-1).Width;
                        }
                        if (num < preferredWidth)
                        {
                            num = preferredWidth;
                        }
                    }
                    if ((internal2 & DataGridViewAutoSizeColumnCriteriaInternal.AllRows) != DataGridViewAutoSizeColumnCriteriaInternal.NotSet)
                    {
                        for (num3 = dataGridView.Rows.GetFirstRow(DataGridViewElementStates.Visible); num3 != -1; num3 = dataGridView.Rows.GetNextRow(num3, DataGridViewElementStates.Visible))
                        {
                            row = dataGridView.Rows.SharedRow(num3);
                            if (fixedHeight)
                            {
                                preferredWidth = row.Cells[base.Index].GetPreferredWidth(num3, row.Thickness);
                            }
                            else
                            {
                                preferredWidth = row.Cells[base.Index].GetPreferredSize(num3).Width;
                            }
                            if (num < preferredWidth)
                            {
                                num = preferredWidth;
                            }
                        }
                        return num;
                    }
                    if ((internal2 & DataGridViewAutoSizeColumnCriteriaInternal.DisplayedRows) != DataGridViewAutoSizeColumnCriteriaInternal.NotSet)
                    {
                        int height = dataGridView.LayoutInfo.Data.Height;
                        int num5 = 0;
                        for (num3 = dataGridView.Rows.GetFirstRow(DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen); (num3 != -1) && (num5 < height); num3 = dataGridView.Rows.GetNextRow(num3, DataGridViewElementStates.Visible | DataGridViewElementStates.Frozen))
                        {
                            row = dataGridView.Rows.SharedRow(num3);
                            if (fixedHeight)
                            {
                                preferredWidth = row.Cells[base.Index].GetPreferredWidth(num3, row.Thickness);
                            }
                            else
                            {
                                preferredWidth = row.Cells[base.Index].GetPreferredSize(num3).Width;
                            }
                            if (num < preferredWidth)
                            {
                                num = preferredWidth;
                            }
                            num5 += row.Thickness;
                        }
                        if (num5 >= height)
                        {
                            return num;
                        }
                        for (num3 = dataGridView.DisplayedBandsInfo.FirstDisplayedScrollingRow; (num3 != -1) && (num5 < height); num3 = dataGridView.Rows.GetNextRow(num3, DataGridViewElementStates.Visible))
                        {
                            row = dataGridView.Rows.SharedRow(num3);
                            if (fixedHeight)
                            {
                                preferredWidth = row.Cells[base.Index].GetPreferredWidth(num3, row.Thickness);
                            }
                            else
                            {
                                preferredWidth = row.Cells[base.Index].GetPreferredSize(num3).Width;
                            }
                            if (num < preferredWidth)
                            {
                                num = preferredWidth;
                            }
                            num5 += row.Thickness;
                        }
                    }
                    return num;
                }
            }
            throw new InvalidEnumArgumentException("value", (int) autoSizeColumnMode, typeof(DataGridViewAutoSizeColumnMode));
        }

        private bool ShouldSerializeDefaultCellStyle()
        {
            if (!base.HasDefaultCellStyle)
            {
                return false;
            }
            DataGridViewCellStyle defaultCellStyle = this.DefaultCellStyle;
            if ((((defaultCellStyle.BackColor.IsEmpty && defaultCellStyle.ForeColor.IsEmpty) && (defaultCellStyle.SelectionBackColor.IsEmpty && defaultCellStyle.SelectionForeColor.IsEmpty)) && (((defaultCellStyle.Font == null) && defaultCellStyle.IsNullValueDefault) && (defaultCellStyle.IsDataSourceNullValueDefault && string.IsNullOrEmpty(defaultCellStyle.Format)))) && ((defaultCellStyle.FormatProvider.Equals(CultureInfo.CurrentCulture) && (defaultCellStyle.Alignment == DataGridViewContentAlignment.NotSet)) && ((defaultCellStyle.WrapMode == DataGridViewTriState.NotSet) && (defaultCellStyle.Tag == null))))
            {
                return !defaultCellStyle.Padding.Equals(Padding.Empty);
            }
            return true;
        }

        private bool ShouldSerializeHeaderText()
        {
            return (base.HasHeaderCell && this.HeaderCell.ContainsLocalValue);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x40);
            builder.Append("DataGridViewColumn { Name=");
            builder.Append(this.Name);
            builder.Append(", Index=");
            builder.Append(base.Index.ToString(CultureInfo.CurrentCulture));
            builder.Append(" }");
            return builder.ToString();
        }

        [System.Windows.Forms.SRDescription("DataGridViewColumn_AutoSizeModeDescr"), System.Windows.Forms.SRCategory("CatLayout"), DefaultValue(0), RefreshProperties(RefreshProperties.Repaint)]
        public DataGridViewAutoSizeColumnMode AutoSizeMode
        {
            get
            {
                return this.autoSizeMode;
            }
            set
            {
                switch (value)
                {
                    case DataGridViewAutoSizeColumnMode.NotSet:
                    case DataGridViewAutoSizeColumnMode.None:
                    case DataGridViewAutoSizeColumnMode.ColumnHeader:
                    case DataGridViewAutoSizeColumnMode.AllCellsExceptHeader:
                    case DataGridViewAutoSizeColumnMode.AllCells:
                    case DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader:
                    case DataGridViewAutoSizeColumnMode.DisplayedCells:
                    case DataGridViewAutoSizeColumnMode.Fill:
                        if (this.autoSizeMode != value)
                        {
                            if (this.Visible && (base.DataGridView != null))
                            {
                                if (!base.DataGridView.ColumnHeadersVisible && ((value == DataGridViewAutoSizeColumnMode.ColumnHeader) || ((value == DataGridViewAutoSizeColumnMode.NotSet) && (base.DataGridView.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.ColumnHeader))))
                                {
                                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumn_AutoSizeCriteriaCannotUseInvisibleHeaders"));
                                }
                                if (this.Frozen && ((value == DataGridViewAutoSizeColumnMode.Fill) || ((value == DataGridViewAutoSizeColumnMode.NotSet) && (base.DataGridView.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.Fill))))
                                {
                                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumn_FrozenColumnCannotAutoFill"));
                                }
                            }
                            DataGridViewAutoSizeColumnMode inheritedAutoSizeMode = this.InheritedAutoSizeMode;
                            bool flag = ((inheritedAutoSizeMode != DataGridViewAutoSizeColumnMode.Fill) && (inheritedAutoSizeMode != DataGridViewAutoSizeColumnMode.None)) && (inheritedAutoSizeMode != DataGridViewAutoSizeColumnMode.NotSet);
                            this.autoSizeMode = value;
                            if (base.DataGridView == null)
                            {
                                if (((this.InheritedAutoSizeMode == DataGridViewAutoSizeColumnMode.Fill) || (this.InheritedAutoSizeMode == DataGridViewAutoSizeColumnMode.None)) || (this.InheritedAutoSizeMode == DataGridViewAutoSizeColumnMode.NotSet))
                                {
                                    if ((base.Thickness != base.CachedThickness) && flag)
                                    {
                                        base.ThicknessInternal = base.CachedThickness;
                                        return;
                                    }
                                }
                                else if (!flag)
                                {
                                    base.CachedThickness = base.Thickness;
                                    return;
                                }
                            }
                            else
                            {
                                base.DataGridView.OnAutoSizeColumnModeChanged(this, inheritedAutoSizeMode);
                            }
                        }
                        return;
                }
                throw new InvalidEnumArgumentException("value", (int) value, typeof(DataGridViewAutoSizeColumnMode));
            }
        }

        internal TypeConverter BoundColumnConverter
        {
            get
            {
                return this.boundColumnConverter;
            }
            set
            {
                this.boundColumnConverter = value;
            }
        }

        internal int BoundColumnIndex
        {
            get
            {
                return this.boundColumnIndex;
            }
            set
            {
                this.boundColumnIndex = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual DataGridViewCell CellTemplate
        {
            get
            {
                return this.cellTemplate;
            }
            set
            {
                this.cellTemplate = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public System.Type CellType
        {
            get
            {
                if (this.cellTemplate != null)
                {
                    return this.cellTemplate.GetType();
                }
                return null;
            }
        }

        [DefaultValue((string) null), System.Windows.Forms.SRDescription("DataGridView_ColumnContextMenuStripDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public override System.Windows.Forms.ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return base.ContextMenuStrip;
            }
            set
            {
                base.ContextMenuStrip = value;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridView_ColumnDataPropertyNameDescr"), System.Windows.Forms.SRCategory("CatData"), Browsable(true), DefaultValue(""), TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Editor("System.Windows.Forms.Design.DataGridViewColumnDataPropertyNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string DataPropertyName
        {
            get
            {
                return this.dataPropertyName;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                if (value != this.dataPropertyName)
                {
                    this.dataPropertyName = value;
                    if (base.DataGridView != null)
                    {
                        base.DataGridView.OnColumnDataPropertyNameChanged(this);
                    }
                }
            }
        }

        [Browsable(true), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("DataGridView_ColumnDefaultCellStyleDescr")]
        public override DataGridViewCellStyle DefaultCellStyle
        {
            get
            {
                return base.DefaultCellStyle;
            }
            set
            {
                base.DefaultCellStyle = value;
            }
        }

        internal int DesiredFillWidth
        {
            get
            {
                return this.desiredFillWidth;
            }
            set
            {
                this.desiredFillWidth = value;
            }
        }

        internal int DesiredMinimumWidth
        {
            get
            {
                return this.desiredMinimumWidth;
            }
            set
            {
                this.desiredMinimumWidth = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int DisplayIndex
        {
            get
            {
                return this.displayIndex;
            }
            set
            {
                if (this.displayIndex != value)
                {
                    if (value == 0x7fffffff)
                    {
                        object[] args = new object[] { 0x7fffffff.ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("DisplayIndex", value, System.Windows.Forms.SR.GetString("DataGridViewColumn_DisplayIndexTooLarge", args));
                    }
                    if (base.DataGridView != null)
                    {
                        if (value < 0)
                        {
                            throw new ArgumentOutOfRangeException("DisplayIndex", value, System.Windows.Forms.SR.GetString("DataGridViewColumn_DisplayIndexNegative"));
                        }
                        if (value >= base.DataGridView.Columns.Count)
                        {
                            throw new ArgumentOutOfRangeException("DisplayIndex", value, System.Windows.Forms.SR.GetString("DataGridViewColumn_DisplayIndexExceedsColumnCount"));
                        }
                        base.DataGridView.OnColumnDisplayIndexChanging(this, value);
                        this.displayIndex = value;
                        try
                        {
                            base.DataGridView.InDisplayIndexAdjustments = true;
                            base.DataGridView.OnColumnDisplayIndexChanged_PreNotification();
                            base.DataGridView.OnColumnDisplayIndexChanged(this);
                            base.DataGridView.OnColumnDisplayIndexChanged_PostNotification();
                            return;
                        }
                        finally
                        {
                            base.DataGridView.InDisplayIndexAdjustments = false;
                        }
                    }
                    if (value < -1)
                    {
                        throw new ArgumentOutOfRangeException("DisplayIndex", value, System.Windows.Forms.SR.GetString("DataGridViewColumn_DisplayIndexTooNegative"));
                    }
                    this.displayIndex = value;
                }
            }
        }

        internal bool DisplayIndexHasChanged
        {
            get
            {
                return ((this.flags & 0x10) != 0);
            }
            set
            {
                if (value)
                {
                    this.flags = (byte) (this.flags | 0x10);
                }
                else
                {
                    this.flags = (byte) (this.flags & -17);
                }
            }
        }

        internal int DisplayIndexInternal
        {
            set
            {
                this.displayIndex = value;
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("DataGridView_ColumnDividerWidthDescr")]
        public int DividerWidth
        {
            get
            {
                return base.DividerThickness;
            }
            set
            {
                base.DividerThickness = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), DefaultValue((float) 100f), System.Windows.Forms.SRDescription("DataGridViewColumn_FillWeightDescr")]
        public float FillWeight
        {
            get
            {
                return this.fillWeight;
            }
            set
            {
                if (value <= 0f)
                {
                    object[] args = new object[] { "FillWeight", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("FillWeight", System.Windows.Forms.SR.GetString("InvalidLowBoundArgument", args));
                }
                if (value > 65535f)
                {
                    object[] objArray2 = new object[] { "FillWeight", value.ToString(CultureInfo.CurrentCulture), ((ushort) 0xffff).ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("FillWeight", System.Windows.Forms.SR.GetString("InvalidHighBoundArgumentEx", objArray2));
                }
                if (base.DataGridView != null)
                {
                    base.DataGridView.OnColumnFillWeightChanging(this, value);
                    this.fillWeight = value;
                    base.DataGridView.OnColumnFillWeightChanged(this);
                }
                else
                {
                    this.fillWeight = value;
                }
            }
        }

        internal float FillWeightInternal
        {
            set
            {
                this.fillWeight = value;
            }
        }

        [RefreshProperties(RefreshProperties.All), DefaultValue(false), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("DataGridView_ColumnFrozenDescr")]
        public override bool Frozen
        {
            get
            {
                return base.Frozen;
            }
            set
            {
                base.Frozen = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataGridViewColumnHeaderCell HeaderCell
        {
            get
            {
                return (DataGridViewColumnHeaderCell) base.HeaderCellCore;
            }
            set
            {
                base.HeaderCellCore = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), Localizable(true), System.Windows.Forms.SRDescription("DataGridView_ColumnHeaderTextDescr")]
        public string HeaderText
        {
            get
            {
                if (base.HasHeaderCell)
                {
                    string str = this.HeaderCell.Value as string;
                    if (str != null)
                    {
                        return str;
                    }
                }
                return string.Empty;
            }
            set
            {
                if (((value != null) || base.HasHeaderCell) && ((this.HeaderCell.ValueType != null) && this.HeaderCell.ValueType.IsAssignableFrom(typeof(string))))
                {
                    this.HeaderCell.Value = value;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Advanced)]
        public DataGridViewAutoSizeColumnMode InheritedAutoSizeMode
        {
            get
            {
                return this.GetInheritedAutoSizeMode(base.DataGridView);
            }
        }

        [Browsable(false)]
        public override DataGridViewCellStyle InheritedStyle
        {
            get
            {
                DataGridViewCellStyle style = null;
                if (base.HasDefaultCellStyle)
                {
                    style = this.DefaultCellStyle;
                }
                if (base.DataGridView == null)
                {
                    return style;
                }
                DataGridViewCellStyle style2 = new DataGridViewCellStyle();
                DataGridViewCellStyle defaultCellStyle = base.DataGridView.DefaultCellStyle;
                if ((style != null) && !style.BackColor.IsEmpty)
                {
                    style2.BackColor = style.BackColor;
                }
                else
                {
                    style2.BackColor = defaultCellStyle.BackColor;
                }
                if ((style != null) && !style.ForeColor.IsEmpty)
                {
                    style2.ForeColor = style.ForeColor;
                }
                else
                {
                    style2.ForeColor = defaultCellStyle.ForeColor;
                }
                if ((style != null) && !style.SelectionBackColor.IsEmpty)
                {
                    style2.SelectionBackColor = style.SelectionBackColor;
                }
                else
                {
                    style2.SelectionBackColor = defaultCellStyle.SelectionBackColor;
                }
                if ((style != null) && !style.SelectionForeColor.IsEmpty)
                {
                    style2.SelectionForeColor = style.SelectionForeColor;
                }
                else
                {
                    style2.SelectionForeColor = defaultCellStyle.SelectionForeColor;
                }
                if ((style != null) && (style.Font != null))
                {
                    style2.Font = style.Font;
                }
                else
                {
                    style2.Font = defaultCellStyle.Font;
                }
                if ((style != null) && !style.IsNullValueDefault)
                {
                    style2.NullValue = style.NullValue;
                }
                else
                {
                    style2.NullValue = defaultCellStyle.NullValue;
                }
                if ((style != null) && !style.IsDataSourceNullValueDefault)
                {
                    style2.DataSourceNullValue = style.DataSourceNullValue;
                }
                else
                {
                    style2.DataSourceNullValue = defaultCellStyle.DataSourceNullValue;
                }
                if ((style != null) && (style.Format.Length != 0))
                {
                    style2.Format = style.Format;
                }
                else
                {
                    style2.Format = defaultCellStyle.Format;
                }
                if ((style != null) && !style.IsFormatProviderDefault)
                {
                    style2.FormatProvider = style.FormatProvider;
                }
                else
                {
                    style2.FormatProvider = defaultCellStyle.FormatProvider;
                }
                if ((style != null) && (style.Alignment != DataGridViewContentAlignment.NotSet))
                {
                    style2.AlignmentInternal = style.Alignment;
                }
                else
                {
                    style2.AlignmentInternal = defaultCellStyle.Alignment;
                }
                if ((style != null) && (style.WrapMode != DataGridViewTriState.NotSet))
                {
                    style2.WrapModeInternal = style.WrapMode;
                }
                else
                {
                    style2.WrapModeInternal = defaultCellStyle.WrapMode;
                }
                if ((style != null) && (style.Tag != null))
                {
                    style2.Tag = style.Tag;
                }
                else
                {
                    style2.Tag = defaultCellStyle.Tag;
                }
                if ((style != null) && (style.Padding != Padding.Empty))
                {
                    style2.PaddingInternal = style.Padding;
                    return style2;
                }
                style2.PaddingInternal = defaultCellStyle.Padding;
                return style2;
            }
        }

        internal bool IsBrowsableInternal
        {
            get
            {
                return ((this.flags & 8) != 0);
            }
            set
            {
                if (value)
                {
                    this.flags = (byte) (this.flags | 8);
                }
                else
                {
                    this.flags = (byte) (this.flags & -9);
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDataBound
        {
            get
            {
                return this.IsDataBoundInternal;
            }
        }

        internal bool IsDataBoundInternal
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

        [System.Windows.Forms.SRDescription("DataGridView_ColumnMinimumWidthDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatLayout"), RefreshProperties(RefreshProperties.Repaint), DefaultValue(5)]
        public int MinimumWidth
        {
            get
            {
                return base.MinimumThickness;
            }
            set
            {
                base.MinimumThickness = value;
            }
        }

        [Browsable(false)]
        public string Name
        {
            get
            {
                if ((this.Site != null) && !string.IsNullOrEmpty(this.Site.Name))
                {
                    this.name = this.Site.Name;
                }
                return this.name;
            }
            set
            {
                string name = this.name;
                if (string.IsNullOrEmpty(value))
                {
                    this.name = string.Empty;
                }
                else
                {
                    this.name = value;
                }
                if ((base.DataGridView != null) && !string.Equals(this.name, name, StringComparison.Ordinal))
                {
                    base.DataGridView.OnColumnNameChanged(this);
                }
            }
        }

        [System.Windows.Forms.SRDescription("DataGridView_ColumnReadOnlyDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public override bool ReadOnly
        {
            get
            {
                return base.ReadOnly;
            }
            set
            {
                if (((this.IsDataBound && (base.DataGridView != null)) && ((base.DataGridView.DataConnection != null) && (this.boundColumnIndex != -1))) && (base.DataGridView.DataConnection.DataFieldIsReadOnly(this.boundColumnIndex) && !value))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridView_ColumnBoundToAReadOnlyFieldMustRemainReadOnly"));
                }
                base.ReadOnly = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("DataGridView_ColumnResizableDescr")]
        public override DataGridViewTriState Resizable
        {
            get
            {
                return base.Resizable;
            }
            set
            {
                base.Resizable = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISite Site
        {
            get
            {
                return this.site;
            }
            set
            {
                this.site = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(0), System.Windows.Forms.SRDescription("DataGridView_ColumnSortModeDescr")]
        public DataGridViewColumnSortMode SortMode
        {
            get
            {
                if ((this.flags & 1) != 0)
                {
                    return DataGridViewColumnSortMode.Automatic;
                }
                if ((this.flags & 2) != 0)
                {
                    return DataGridViewColumnSortMode.Programmatic;
                }
                return DataGridViewColumnSortMode.NotSortable;
            }
            set
            {
                if (value != this.SortMode)
                {
                    if (value != DataGridViewColumnSortMode.NotSortable)
                    {
                        if ((((base.DataGridView != null) && !base.DataGridView.InInitialization) && (value == DataGridViewColumnSortMode.Automatic)) && ((base.DataGridView.SelectionMode == DataGridViewSelectionMode.FullColumnSelect) || (base.DataGridView.SelectionMode == DataGridViewSelectionMode.ColumnHeaderSelect)))
                        {
                            throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridViewColumn_SortModeAndSelectionModeClash", new object[] { value.ToString(), base.DataGridView.SelectionMode.ToString() }));
                        }
                        if (value == DataGridViewColumnSortMode.Automatic)
                        {
                            this.flags = (byte) (this.flags & -3);
                            this.flags = (byte) (this.flags | 1);
                        }
                        else
                        {
                            this.flags = (byte) (this.flags & -2);
                            this.flags = (byte) (this.flags | 2);
                        }
                    }
                    else
                    {
                        this.flags = (byte) (this.flags & -2);
                        this.flags = (byte) (this.flags & -3);
                    }
                    if (base.DataGridView != null)
                    {
                        base.DataGridView.OnColumnSortModeChanged(this);
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("DataGridView_ColumnToolTipTextDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue("")]
        public string ToolTipText
        {
            get
            {
                return this.HeaderCell.ToolTipText;
            }
            set
            {
                if (string.Compare(this.ToolTipText, value, false, CultureInfo.InvariantCulture) != 0)
                {
                    this.HeaderCell.ToolTipText = value;
                    if (base.DataGridView != null)
                    {
                        base.DataGridView.OnColumnToolTipTextChanged(this);
                    }
                }
            }
        }

        internal float UsedFillWeight
        {
            get
            {
                return this.usedFillWeight;
            }
            set
            {
                this.usedFillWeight = value;
            }
        }

        [DefaultValue((string) null), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Type ValueType
        {
            get
            {
                return (System.Type) base.Properties.GetObject(PropDataGridViewColumnValueType);
            }
            set
            {
                base.Properties.SetObject(PropDataGridViewColumnValueType, value);
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("DataGridView_ColumnVisibleDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(true)]
        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), RefreshProperties(RefreshProperties.Repaint), Localizable(true), System.Windows.Forms.SRDescription("DataGridView_ColumnWidthDescr")]
        public int Width
        {
            get
            {
                return base.Thickness;
            }
            set
            {
                base.Thickness = value;
            }
        }
    }
}

