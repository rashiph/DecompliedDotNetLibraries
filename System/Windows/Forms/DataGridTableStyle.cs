namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;

    [DesignTimeVisible(false), ToolboxItem(false)]
    public class DataGridTableStyle : Component, IDataGridEditingService
    {
        private bool allowSorting;
        private SolidBrush alternatingBackBrush;
        private SolidBrush backBrush;
        private bool columnHeadersVisible;
        internal System.Windows.Forms.DataGrid dataGrid;
        private const bool defaultAllowSorting = true;
        internal static readonly Font defaultFont = Control.DefaultFont;
        internal static readonly int defaultFontHeight = defaultFont.Height;
        private const DataGridLineStyle defaultGridLineStyle = DataGridLineStyle.Solid;
        private const int defaultPreferredColumnWidth = 0x4b;
        private const int defaultRowHeaderWidth = 0x23;
        public static readonly DataGridTableStyle DefaultTableStyle = new DataGridTableStyle(true);
        private static readonly object EventAllowSorting = new object();
        private static readonly object EventAlternatingBackColor = new object();
        private static readonly object EventBackColor = new object();
        private static readonly object EventColumnHeadersVisible = new object();
        private static readonly object EventForeColor = new object();
        private static readonly object EventGridLineColor = new object();
        private static readonly object EventGridLineStyle = new object();
        private static readonly object EventHeaderBackColor = new object();
        private static readonly object EventHeaderFont = new object();
        private static readonly object EventHeaderForeColor = new object();
        private static readonly object EventLinkColor = new object();
        private static readonly object EventLinkHoverColor = new object();
        private static readonly object EventMappingName = new object();
        private static readonly object EventPreferredColumnWidth = new object();
        private static readonly object EventPreferredRowHeight = new object();
        private static readonly object EventReadOnly = new object();
        private static readonly object EventRowHeadersVisible = new object();
        private static readonly object EventRowHeaderWidth = new object();
        private static readonly object EventSelectionBackColor = new object();
        private static readonly object EventSelectionForeColor = new object();
        private int focusedRelation;
        private int focusedTextWidth;
        private SolidBrush foreBrush;
        private GridColumnStylesCollection gridColumns;
        private SolidBrush gridLineBrush;
        private DataGridLineStyle gridLineStyle;
        internal SolidBrush headerBackBrush;
        internal Font headerFont;
        internal SolidBrush headerForeBrush;
        internal Pen headerForePen;
        private bool isDefaultTableStyle;
        private SolidBrush linkBrush;
        private string mappingName;
        internal int preferredColumnWidth;
        private int prefferedRowHeight;
        private bool readOnly;
        private int relationshipHeight;
        private Rectangle relationshipRect;
        internal const int relationshipSpacing = 1;
        private ArrayList relationsList;
        private bool rowHeadersVisible;
        private int rowHeaderWidth;
        private SolidBrush selectionBackBrush;
        private SolidBrush selectionForeBrush;

        public event EventHandler AllowSortingChanged
        {
            add
            {
                base.Events.AddHandler(EventAllowSorting, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventAllowSorting, value);
            }
        }

        public event EventHandler AlternatingBackColorChanged
        {
            add
            {
                base.Events.AddHandler(EventAlternatingBackColor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventAlternatingBackColor, value);
            }
        }

        public event EventHandler BackColorChanged
        {
            add
            {
                base.Events.AddHandler(EventBackColor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventBackColor, value);
            }
        }

        public event EventHandler ColumnHeadersVisibleChanged
        {
            add
            {
                base.Events.AddHandler(EventColumnHeadersVisible, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventColumnHeadersVisible, value);
            }
        }

        public event EventHandler ForeColorChanged
        {
            add
            {
                base.Events.AddHandler(EventForeColor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventForeColor, value);
            }
        }

        public event EventHandler GridLineColorChanged
        {
            add
            {
                base.Events.AddHandler(EventGridLineColor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventGridLineColor, value);
            }
        }

        public event EventHandler GridLineStyleChanged
        {
            add
            {
                base.Events.AddHandler(EventGridLineStyle, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventGridLineStyle, value);
            }
        }

        public event EventHandler HeaderBackColorChanged
        {
            add
            {
                base.Events.AddHandler(EventHeaderBackColor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventHeaderBackColor, value);
            }
        }

        public event EventHandler HeaderFontChanged
        {
            add
            {
                base.Events.AddHandler(EventHeaderFont, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventHeaderFont, value);
            }
        }

        public event EventHandler HeaderForeColorChanged
        {
            add
            {
                base.Events.AddHandler(EventHeaderForeColor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventHeaderForeColor, value);
            }
        }

        public event EventHandler LinkColorChanged
        {
            add
            {
                base.Events.AddHandler(EventLinkColor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLinkColor, value);
            }
        }

        public event EventHandler LinkHoverColorChanged
        {
            add
            {
                base.Events.AddHandler(EventLinkHoverColor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLinkHoverColor, value);
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

        public event EventHandler PreferredColumnWidthChanged
        {
            add
            {
                base.Events.AddHandler(EventPreferredColumnWidth, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPreferredColumnWidth, value);
            }
        }

        public event EventHandler PreferredRowHeightChanged
        {
            add
            {
                base.Events.AddHandler(EventPreferredRowHeight, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPreferredRowHeight, value);
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

        public event EventHandler RowHeadersVisibleChanged
        {
            add
            {
                base.Events.AddHandler(EventRowHeadersVisible, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRowHeadersVisible, value);
            }
        }

        public event EventHandler RowHeaderWidthChanged
        {
            add
            {
                base.Events.AddHandler(EventRowHeaderWidth, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRowHeaderWidth, value);
            }
        }

        public event EventHandler SelectionBackColorChanged
        {
            add
            {
                base.Events.AddHandler(EventSelectionBackColor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSelectionBackColor, value);
            }
        }

        public event EventHandler SelectionForeColorChanged
        {
            add
            {
                base.Events.AddHandler(EventSelectionForeColor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSelectionForeColor, value);
            }
        }

        public DataGridTableStyle() : this(false)
        {
        }

        public DataGridTableStyle(bool isDefaultTableStyle)
        {
            this.relationshipRect = Rectangle.Empty;
            this.focusedRelation = -1;
            this.relationsList = new ArrayList(2);
            this.mappingName = "";
            this.allowSorting = true;
            this.alternatingBackBrush = DefaultAlternatingBackBrush;
            this.backBrush = DefaultBackBrush;
            this.foreBrush = DefaultForeBrush;
            this.gridLineBrush = DefaultGridLineBrush;
            this.gridLineStyle = DataGridLineStyle.Solid;
            this.headerBackBrush = DefaultHeaderBackBrush;
            this.headerForeBrush = DefaultHeaderForeBrush;
            this.headerForePen = DefaultHeaderForePen;
            this.linkBrush = DefaultLinkBrush;
            this.preferredColumnWidth = 0x4b;
            this.prefferedRowHeight = defaultFontHeight + 3;
            this.selectionBackBrush = DefaultSelectionBackBrush;
            this.selectionForeBrush = DefaultSelectionForeBrush;
            this.rowHeaderWidth = 0x23;
            this.rowHeadersVisible = true;
            this.columnHeadersVisible = true;
            this.gridColumns = new GridColumnStylesCollection(this, isDefaultTableStyle);
            this.gridColumns.CollectionChanged += new CollectionChangeEventHandler(this.OnColumnCollectionChanged);
            this.isDefaultTableStyle = isDefaultTableStyle;
        }

        public DataGridTableStyle(CurrencyManager listManager) : this()
        {
            this.mappingName = listManager.GetListName();
            this.SetGridColumnStylesCollection(listManager);
        }

        public bool BeginEdit(DataGridColumnStyle gridColumn, int rowNumber)
        {
            System.Windows.Forms.DataGrid dataGrid = this.DataGrid;
            if (dataGrid == null)
            {
                return false;
            }
            return dataGrid.BeginEdit(gridColumn, rowNumber);
        }

        private Rectangle ComputeRelationshipRect()
        {
            if (this.relationshipRect.IsEmpty && this.DataGrid.AllowNavigation)
            {
                Graphics graphics = this.DataGrid.CreateGraphicsInternal();
                this.relationshipRect = new Rectangle();
                this.relationshipRect.X = 0;
                int num = 0;
                for (int i = 0; i < this.RelationsList.Count; i++)
                {
                    int num3 = (int) Math.Ceiling((double) graphics.MeasureString((string) this.RelationsList[i], this.DataGrid.LinkFont).Width);
                    if (num3 > num)
                    {
                        num = num3;
                    }
                }
                graphics.Dispose();
                this.relationshipRect.Width = num + 5;
                this.relationshipRect.Width += 2;
                this.relationshipRect.Height = this.BorderWidth + (this.relationshipHeight * this.RelationsList.Count);
                this.relationshipRect.Height += 2;
                if (this.RelationsList.Count > 0)
                {
                    this.relationshipRect.Height += 2;
                }
            }
            return this.relationshipRect;
        }

        protected internal virtual DataGridColumnStyle CreateGridColumn(PropertyDescriptor prop)
        {
            return this.CreateGridColumn(prop, false);
        }

        protected internal virtual DataGridColumnStyle CreateGridColumn(PropertyDescriptor prop, bool isDefault)
        {
            System.Type propertyType = prop.PropertyType;
            if (propertyType.Equals(typeof(bool)))
            {
                return new DataGridBoolColumn(prop, isDefault);
            }
            if (!propertyType.Equals(typeof(string)))
            {
                if (propertyType.Equals(typeof(DateTime)))
                {
                    return new DataGridTextBoxColumn(prop, "d", isDefault);
                }
                if (((propertyType.Equals(typeof(short)) || propertyType.Equals(typeof(int))) || (propertyType.Equals(typeof(long)) || propertyType.Equals(typeof(ushort)))) || (((propertyType.Equals(typeof(uint)) || propertyType.Equals(typeof(ulong))) || (propertyType.Equals(typeof(decimal)) || propertyType.Equals(typeof(double)))) || ((propertyType.Equals(typeof(float)) || propertyType.Equals(typeof(byte))) || propertyType.Equals(typeof(sbyte)))))
                {
                    return new DataGridTextBoxColumn(prop, "G", isDefault);
                }
            }
            return new DataGridTextBoxColumn(prop, isDefault);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GridColumnStylesCollection gridColumnStyles = this.GridColumnStyles;
                if (gridColumnStyles != null)
                {
                    for (int i = 0; i < gridColumnStyles.Count; i++)
                    {
                        gridColumnStyles[i].Dispose();
                    }
                }
            }
            base.Dispose(disposing);
        }

        public bool EndEdit(DataGridColumnStyle gridColumn, int rowNumber, bool shouldAbort)
        {
            System.Windows.Forms.DataGrid dataGrid = this.DataGrid;
            if (dataGrid == null)
            {
                return false;
            }
            return dataGrid.EndEdit(gridColumn, rowNumber, shouldAbort);
        }

        internal void InvalidateColumn(DataGridColumnStyle column)
        {
            int index = this.GridColumnStyles.IndexOf(column);
            if ((index >= 0) && (this.DataGrid != null))
            {
                this.DataGrid.InvalidateColumn(index);
            }
        }

        private void InvalidateInside()
        {
            if (this.DataGrid != null)
            {
                this.DataGrid.InvalidateInside();
            }
        }

        protected virtual void OnAllowSortingChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventAllowSorting] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnAlternatingBackColorChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventAlternatingBackColor] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnBackColorChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventForeColor] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnColumnCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            this.gridColumns.CollectionChanged -= new CollectionChangeEventHandler(this.OnColumnCollectionChanged);
            try
            {
                System.Windows.Forms.DataGrid dataGrid = this.DataGrid;
                DataGridColumnStyle element = e.Element as DataGridColumnStyle;
                if (e.Action == CollectionChangeAction.Add)
                {
                    if (element != null)
                    {
                        element.SetDataGridInternalInColumn(dataGrid);
                    }
                }
                else if (e.Action == CollectionChangeAction.Remove)
                {
                    if (element != null)
                    {
                        element.SetDataGridInternalInColumn(null);
                    }
                }
                else if (e.Element != null)
                {
                    for (int i = 0; i < this.gridColumns.Count; i++)
                    {
                        this.gridColumns[i].SetDataGridInternalInColumn(null);
                    }
                }
                if (dataGrid != null)
                {
                    dataGrid.OnColumnCollectionChanged(this, e);
                }
            }
            finally
            {
                this.gridColumns.CollectionChanged += new CollectionChangeEventHandler(this.OnColumnCollectionChanged);
            }
        }

        protected virtual void OnColumnHeadersVisibleChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventColumnHeadersVisible] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnForeColorChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventBackColor] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnGridLineColorChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventGridLineColor] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnGridLineStyleChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventGridLineStyle] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnHeaderBackColorChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventHeaderBackColor] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnHeaderFontChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventHeaderFont] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnHeaderForeColorChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventHeaderForeColor] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnLinkColorChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventLinkColor] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnLinkHoverColorChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventLinkHoverColor] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnMappingNameChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventMappingName] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnPreferredColumnWidthChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventPreferredColumnWidth] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnPreferredRowHeightChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventPreferredRowHeight] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnReadOnlyChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventReadOnly] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnRowHeadersVisibleChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventRowHeadersVisible] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnRowHeaderWidthChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventRowHeaderWidth] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSelectionBackColorChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventSelectionBackColor] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSelectionForeColorChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventSelectionForeColor] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private static bool PropertyDescriptorIsARelation(PropertyDescriptor prop)
        {
            return (typeof(IList).IsAssignableFrom(prop.PropertyType) && !typeof(Array).IsAssignableFrom(prop.PropertyType));
        }

        public void ResetAlternatingBackColor()
        {
            if (this.ShouldSerializeAlternatingBackColor())
            {
                this.AlternatingBackColor = DefaultAlternatingBackBrush.Color;
                this.InvalidateInside();
            }
        }

        public void ResetBackColor()
        {
            if (!this.backBrush.Equals(DefaultBackBrush))
            {
                this.BackColor = DefaultBackBrush.Color;
            }
        }

        public void ResetForeColor()
        {
            if (!this.foreBrush.Equals(DefaultForeBrush))
            {
                this.ForeColor = DefaultForeBrush.Color;
            }
        }

        public void ResetGridLineColor()
        {
            if (this.ShouldSerializeGridLineColor())
            {
                this.GridLineColor = DefaultGridLineBrush.Color;
            }
        }

        public void ResetHeaderBackColor()
        {
            if (this.ShouldSerializeHeaderBackColor())
            {
                this.HeaderBackColor = DefaultHeaderBackBrush.Color;
            }
        }

        public void ResetHeaderFont()
        {
            if (this.headerFont != null)
            {
                this.headerFont = null;
                this.OnHeaderFontChanged(EventArgs.Empty);
            }
        }

        public void ResetHeaderForeColor()
        {
            if (this.ShouldSerializeHeaderForeColor())
            {
                this.HeaderForeColor = DefaultHeaderForeBrush.Color;
            }
        }

        public void ResetLinkColor()
        {
            if (this.ShouldSerializeLinkColor())
            {
                this.LinkColor = DefaultLinkBrush.Color;
            }
        }

        public void ResetLinkHoverColor()
        {
        }

        private void ResetPreferredRowHeight()
        {
            this.PreferredRowHeight = defaultFontHeight + 3;
        }

        internal void ResetRelationsList()
        {
            if (this.isDefaultTableStyle)
            {
                this.relationsList.Clear();
            }
        }

        internal void ResetRelationsUI()
        {
            this.relationshipRect = Rectangle.Empty;
            this.focusedRelation = -1;
            this.relationshipHeight = this.dataGrid.LinkFontHeight + 1;
        }

        public void ResetSelectionBackColor()
        {
            if (this.ShouldSerializeSelectionBackColor())
            {
                this.SelectionBackColor = DefaultSelectionBackBrush.Color;
            }
        }

        public void ResetSelectionForeColor()
        {
            if (this.ShouldSerializeSelectionForeColor())
            {
                this.SelectionForeColor = DefaultSelectionForeBrush.Color;
            }
        }

        internal void SetGridColumnStylesCollection(CurrencyManager listManager)
        {
            this.gridColumns.CollectionChanged -= new CollectionChangeEventHandler(this.OnColumnCollectionChanged);
            PropertyDescriptorCollection itemProperties = listManager.GetItemProperties();
            if (this.relationsList.Count > 0)
            {
                this.relationsList.Clear();
            }
            int count = itemProperties.Count;
            for (int i = 0; i < count; i++)
            {
                PropertyDescriptor prop = itemProperties[i];
                if (prop.IsBrowsable)
                {
                    if (PropertyDescriptorIsARelation(prop))
                    {
                        this.relationsList.Add(prop.Name);
                    }
                    else
                    {
                        DataGridColumnStyle column = this.CreateGridColumn(prop, this.isDefaultTableStyle);
                        if (this.isDefaultTableStyle)
                        {
                            this.gridColumns.AddDefaultColumn(column);
                        }
                        else
                        {
                            column.MappingName = prop.Name;
                            column.HeaderText = prop.Name;
                            this.gridColumns.Add(column);
                        }
                    }
                }
            }
            this.gridColumns.CollectionChanged += new CollectionChangeEventHandler(this.OnColumnCollectionChanged);
        }

        internal void SetInternalDataGrid(System.Windows.Forms.DataGrid dG, bool force)
        {
            if (((this.dataGrid == null) || !this.dataGrid.Equals(dG)) || force)
            {
                this.dataGrid = dG;
                if ((dG == null) || !dG.Initializing)
                {
                    int count = this.gridColumns.Count;
                    for (int i = 0; i < count; i++)
                    {
                        this.gridColumns[i].SetDataGridInternalInColumn(dG);
                    }
                }
            }
        }

        internal void SetRelationsList(CurrencyManager listManager)
        {
            PropertyDescriptorCollection itemProperties = listManager.GetItemProperties();
            int count = itemProperties.Count;
            if (this.relationsList.Count > 0)
            {
                this.relationsList.Clear();
            }
            for (int i = 0; i < count; i++)
            {
                PropertyDescriptor prop = itemProperties[i];
                if (PropertyDescriptorIsARelation(prop))
                {
                    this.relationsList.Add(prop.Name);
                }
            }
        }

        protected virtual bool ShouldSerializeAlternatingBackColor()
        {
            return !this.AlternatingBackBrush.Equals(DefaultAlternatingBackBrush);
        }

        protected bool ShouldSerializeBackColor()
        {
            return !DefaultBackBrush.Equals(this.backBrush);
        }

        protected bool ShouldSerializeForeColor()
        {
            return !DefaultForeBrush.Equals(this.foreBrush);
        }

        protected virtual bool ShouldSerializeGridLineColor()
        {
            return !this.GridLineBrush.Equals(DefaultGridLineBrush);
        }

        protected virtual bool ShouldSerializeHeaderBackColor()
        {
            return !this.HeaderBackBrush.Equals(DefaultHeaderBackBrush);
        }

        private bool ShouldSerializeHeaderFont()
        {
            return (this.headerFont != null);
        }

        protected virtual bool ShouldSerializeHeaderForeColor()
        {
            return !this.HeaderForePen.Equals(DefaultHeaderForePen);
        }

        protected virtual bool ShouldSerializeLinkColor()
        {
            return !this.LinkBrush.Equals(DefaultLinkBrush);
        }

        protected virtual bool ShouldSerializeLinkHoverColor()
        {
            return false;
        }

        protected bool ShouldSerializePreferredRowHeight()
        {
            return (this.prefferedRowHeight != (defaultFontHeight + 3));
        }

        protected bool ShouldSerializeSelectionBackColor()
        {
            return !DefaultSelectionBackBrush.Equals(this.selectionBackBrush);
        }

        protected virtual bool ShouldSerializeSelectionForeColor()
        {
            return !this.SelectionForeBrush.Equals(DefaultSelectionForeBrush);
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), System.Windows.Forms.SRDescription("DataGridAllowSortingDescr")]
        public bool AllowSorting
        {
            get
            {
                return this.allowSorting;
            }
            set
            {
                if (this.isDefaultTableStyle)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultTableSet", new object[] { "AllowSorting" }));
                }
                if (this.allowSorting != value)
                {
                    this.allowSorting = value;
                    this.OnAllowSortingChanged(EventArgs.Empty);
                }
            }
        }

        internal SolidBrush AlternatingBackBrush
        {
            get
            {
                return this.alternatingBackBrush;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridAlternatingBackColorDescr"), System.Windows.Forms.SRCategory("CatColors")]
        public Color AlternatingBackColor
        {
            get
            {
                return this.alternatingBackBrush.Color;
            }
            set
            {
                if (this.isDefaultTableStyle)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultTableSet", new object[] { "AlternatingBackColor" }));
                }
                if (System.Windows.Forms.DataGrid.IsTransparentColor(value))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridTableStyleTransparentAlternatingBackColorNotAllowed"));
                }
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "AlternatingBackColor" }));
                }
                if (!this.alternatingBackBrush.Color.Equals(value))
                {
                    this.alternatingBackBrush = new SolidBrush(value);
                    this.InvalidateInside();
                    this.OnAlternatingBackColorChanged(EventArgs.Empty);
                }
            }
        }

        internal SolidBrush BackBrush
        {
            get
            {
                return this.backBrush;
            }
        }

        [System.Windows.Forms.SRDescription("ControlBackColorDescr"), System.Windows.Forms.SRCategory("CatColors")]
        public Color BackColor
        {
            get
            {
                return this.backBrush.Color;
            }
            set
            {
                if (this.isDefaultTableStyle)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultTableSet", new object[] { "BackColor" }));
                }
                if (System.Windows.Forms.DataGrid.IsTransparentColor(value))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridTableStyleTransparentBackColorNotAllowed"));
                }
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "BackColor" }));
                }
                if (!this.backBrush.Color.Equals(value))
                {
                    this.backBrush = new SolidBrush(value);
                    this.InvalidateInside();
                    this.OnBackColorChanged(EventArgs.Empty);
                }
            }
        }

        internal int BorderWidth
        {
            get
            {
                DataGridLineStyle gridLineStyle;
                int gridLineWidth;
                if (this.DataGrid == null)
                {
                    return 0;
                }
                if (this.IsDefault)
                {
                    gridLineStyle = this.DataGrid.GridLineStyle;
                    gridLineWidth = this.DataGrid.GridLineWidth;
                }
                else
                {
                    gridLineStyle = this.GridLineStyle;
                    gridLineWidth = this.GridLineWidth;
                }
                if (gridLineStyle == DataGridLineStyle.None)
                {
                    return 0;
                }
                return gridLineWidth;
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRCategory("CatDisplay"), System.Windows.Forms.SRDescription("DataGridColumnHeadersVisibleDescr")]
        public bool ColumnHeadersVisible
        {
            get
            {
                return this.columnHeadersVisible;
            }
            set
            {
                if (this.columnHeadersVisible != value)
                {
                    this.columnHeadersVisible = value;
                    this.OnColumnHeadersVisibleChanged(EventArgs.Empty);
                }
            }
        }

        [Browsable(false)]
        public virtual System.Windows.Forms.DataGrid DataGrid
        {
            get
            {
                return this.dataGrid;
            }
            set
            {
                this.SetInternalDataGrid(value, true);
            }
        }

        internal static SolidBrush DefaultAlternatingBackBrush
        {
            get
            {
                return (SolidBrush) SystemBrushes.Window;
            }
        }

        internal static SolidBrush DefaultBackBrush
        {
            get
            {
                return (SolidBrush) SystemBrushes.Window;
            }
        }

        internal static SolidBrush DefaultForeBrush
        {
            get
            {
                return (SolidBrush) SystemBrushes.WindowText;
            }
        }

        private static SolidBrush DefaultGridLineBrush
        {
            get
            {
                return (SolidBrush) SystemBrushes.Control;
            }
        }

        private static SolidBrush DefaultHeaderBackBrush
        {
            get
            {
                return (SolidBrush) SystemBrushes.Control;
            }
        }

        private static SolidBrush DefaultHeaderForeBrush
        {
            get
            {
                return (SolidBrush) SystemBrushes.ControlText;
            }
        }

        private static Pen DefaultHeaderForePen
        {
            get
            {
                return new Pen(SystemColors.ControlText);
            }
        }

        private static SolidBrush DefaultLinkBrush
        {
            get
            {
                return (SolidBrush) SystemBrushes.HotTrack;
            }
        }

        private static SolidBrush DefaultSelectionBackBrush
        {
            get
            {
                return (SolidBrush) SystemBrushes.ActiveCaption;
            }
        }

        private static SolidBrush DefaultSelectionForeBrush
        {
            get
            {
                return (SolidBrush) SystemBrushes.ActiveCaptionText;
            }
        }

        internal int FocusedRelation
        {
            get
            {
                return this.focusedRelation;
            }
            set
            {
                if (this.focusedRelation != value)
                {
                    this.focusedRelation = value;
                    if (this.focusedRelation == -1)
                    {
                        this.focusedTextWidth = 0;
                    }
                    else
                    {
                        Graphics graphics = this.DataGrid.CreateGraphicsInternal();
                        this.focusedTextWidth = (int) Math.Ceiling((double) graphics.MeasureString((string) this.RelationsList[this.focusedRelation], this.DataGrid.LinkFont).Width);
                        graphics.Dispose();
                    }
                }
            }
        }

        internal int FocusedTextWidth
        {
            get
            {
                return this.focusedTextWidth;
            }
        }

        internal SolidBrush ForeBrush
        {
            get
            {
                return this.foreBrush;
            }
        }

        [System.Windows.Forms.SRDescription("ControlForeColorDescr"), System.Windows.Forms.SRCategory("CatColors")]
        public Color ForeColor
        {
            get
            {
                return this.foreBrush.Color;
            }
            set
            {
                if (this.isDefaultTableStyle)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultTableSet", new object[] { "ForeColor" }));
                }
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "BackColor" }));
                }
                if (!this.foreBrush.Color.Equals(value))
                {
                    this.foreBrush = new SolidBrush(value);
                    this.InvalidateInside();
                    this.OnForeColorChanged(EventArgs.Empty);
                }
            }
        }

        [Localizable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public virtual GridColumnStylesCollection GridColumnStyles
        {
            get
            {
                return this.gridColumns;
            }
        }

        internal SolidBrush GridLineBrush
        {
            get
            {
                return this.gridLineBrush;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridGridLineColorDescr"), System.Windows.Forms.SRCategory("CatColors")]
        public Color GridLineColor
        {
            get
            {
                return this.gridLineBrush.Color;
            }
            set
            {
                if (this.isDefaultTableStyle)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultTableSet", new object[] { "GridLineColor" }));
                }
                if (this.gridLineBrush.Color != value)
                {
                    if (value.IsEmpty)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "GridLineColor" }));
                    }
                    this.gridLineBrush = new SolidBrush(value);
                    this.OnGridLineColorChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRDescription("DataGridGridLineStyleDescr"), DefaultValue(1), System.Windows.Forms.SRCategory("CatAppearance")]
        public DataGridLineStyle GridLineStyle
        {
            get
            {
                return this.gridLineStyle;
            }
            set
            {
                if (this.isDefaultTableStyle)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultTableSet", new object[] { "GridLineStyle" }));
                }
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DataGridLineStyle));
                }
                if (this.gridLineStyle != value)
                {
                    this.gridLineStyle = value;
                    this.OnGridLineStyleChanged(EventArgs.Empty);
                }
            }
        }

        internal int GridLineWidth
        {
            get
            {
                if (this.GridLineStyle != DataGridLineStyle.Solid)
                {
                    return 0;
                }
                return 1;
            }
        }

        internal SolidBrush HeaderBackBrush
        {
            get
            {
                return this.headerBackBrush;
            }
        }

        [System.Windows.Forms.SRCategory("CatColors"), System.Windows.Forms.SRDescription("DataGridHeaderBackColorDescr")]
        public Color HeaderBackColor
        {
            get
            {
                return this.headerBackBrush.Color;
            }
            set
            {
                if (this.isDefaultTableStyle)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultTableSet", new object[] { "HeaderBackColor" }));
                }
                if (System.Windows.Forms.DataGrid.IsTransparentColor(value))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridTableStyleTransparentHeaderBackColorNotAllowed"));
                }
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "HeaderBackColor" }));
                }
                if (!value.Equals(this.headerBackBrush.Color))
                {
                    this.headerBackBrush = new SolidBrush(value);
                    this.OnHeaderBackColorChanged(EventArgs.Empty);
                }
            }
        }

        [Localizable(true), AmbientValue((string) null), System.Windows.Forms.SRDescription("DataGridHeaderFontDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public Font HeaderFont
        {
            get
            {
                if (this.headerFont != null)
                {
                    return this.headerFont;
                }
                if (this.DataGrid != null)
                {
                    return this.DataGrid.Font;
                }
                return Control.DefaultFont;
            }
            set
            {
                if (this.isDefaultTableStyle)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultTableSet", new object[] { "HeaderFont" }));
                }
                if (((value == null) && (this.headerFont != null)) || ((value != null) && !value.Equals(this.headerFont)))
                {
                    this.headerFont = value;
                    this.OnHeaderFontChanged(EventArgs.Empty);
                }
            }
        }

        internal SolidBrush HeaderForeBrush
        {
            get
            {
                return this.headerForeBrush;
            }
        }

        [System.Windows.Forms.SRCategory("CatColors"), System.Windows.Forms.SRDescription("DataGridHeaderForeColorDescr")]
        public Color HeaderForeColor
        {
            get
            {
                return this.headerForePen.Color;
            }
            set
            {
                if (this.isDefaultTableStyle)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultTableSet", new object[] { "HeaderForeColor" }));
                }
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "HeaderForeColor" }));
                }
                if (!value.Equals(this.headerForePen.Color))
                {
                    this.headerForePen = new Pen(value);
                    this.headerForeBrush = new SolidBrush(value);
                    this.OnHeaderForeColorChanged(EventArgs.Empty);
                }
            }
        }

        internal Pen HeaderForePen
        {
            get
            {
                return this.headerForePen;
            }
        }

        internal bool IsDefault
        {
            get
            {
                return this.isDefaultTableStyle;
            }
        }

        internal Brush LinkBrush
        {
            get
            {
                return this.linkBrush;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridLinkColorDescr"), System.Windows.Forms.SRCategory("CatColors")]
        public Color LinkColor
        {
            get
            {
                return this.linkBrush.Color;
            }
            set
            {
                if (this.isDefaultTableStyle)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultTableSet", new object[] { "LinkColor" }));
                }
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "LinkColor" }));
                }
                if (!this.linkBrush.Color.Equals(value))
                {
                    this.linkBrush = new SolidBrush(value);
                    this.OnLinkColorChanged(EventArgs.Empty);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), System.Windows.Forms.SRDescription("DataGridLinkHoverColorDescr"), System.Windows.Forms.SRCategory("CatColors"), Browsable(false)]
        public Color LinkHoverColor
        {
            get
            {
                return this.LinkColor;
            }
            set
            {
            }
        }

        [Editor("System.Windows.Forms.Design.DataGridTableStyleMappingNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue("")]
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
                if (!value.Equals(this.mappingName))
                {
                    string mappingName = this.MappingName;
                    this.mappingName = value;
                    try
                    {
                        if (this.DataGrid != null)
                        {
                            this.DataGrid.TableStyles.CheckForMappingNameDuplicates(this);
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

        [System.Windows.Forms.SRDescription("DataGridPreferredColumnWidthDescr"), Localizable(true), DefaultValue(0x4b), System.Windows.Forms.SRCategory("CatLayout"), TypeConverter(typeof(DataGridPreferredColumnWidthTypeConverter))]
        public int PreferredColumnWidth
        {
            get
            {
                return this.preferredColumnWidth;
            }
            set
            {
                if (this.isDefaultTableStyle)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultTableSet", new object[] { "PreferredColumnWidth" }));
                }
                if (value < 0)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridColumnWidth"), "PreferredColumnWidth");
                }
                if (this.preferredColumnWidth != value)
                {
                    this.preferredColumnWidth = value;
                    this.OnPreferredColumnWidthChanged(EventArgs.Empty);
                }
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("DataGridPreferredRowHeightDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public int PreferredRowHeight
        {
            get
            {
                return this.prefferedRowHeight;
            }
            set
            {
                if (this.isDefaultTableStyle)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultTableSet", new object[] { "PrefferedRowHeight" }));
                }
                if (value < 0)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridRowRowHeight"));
                }
                this.prefferedRowHeight = value;
                this.OnPreferredRowHeightChanged(EventArgs.Empty);
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

        internal int RelationshipHeight
        {
            get
            {
                return this.relationshipHeight;
            }
        }

        internal Rectangle RelationshipRect
        {
            get
            {
                if (this.relationshipRect.IsEmpty)
                {
                    this.ComputeRelationshipRect();
                }
                return this.relationshipRect;
            }
        }

        internal ArrayList RelationsList
        {
            get
            {
                return this.relationsList;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridRowHeadersVisibleDescr"), System.Windows.Forms.SRCategory("CatDisplay"), DefaultValue(true)]
        public bool RowHeadersVisible
        {
            get
            {
                return this.rowHeadersVisible;
            }
            set
            {
                if (this.rowHeadersVisible != value)
                {
                    this.rowHeadersVisible = value;
                    this.OnRowHeadersVisibleChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(0x23), System.Windows.Forms.SRDescription("DataGridRowHeaderWidthDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatLayout")]
        public int RowHeaderWidth
        {
            get
            {
                return this.rowHeaderWidth;
            }
            set
            {
                if (this.DataGrid != null)
                {
                    value = Math.Max(this.DataGrid.MinimumRowHeaderWidth(), value);
                }
                if (this.rowHeaderWidth != value)
                {
                    this.rowHeaderWidth = value;
                    this.OnRowHeaderWidthChanged(EventArgs.Empty);
                }
            }
        }

        internal SolidBrush SelectionBackBrush
        {
            get
            {
                return this.selectionBackBrush;
            }
        }

        [System.Windows.Forms.SRCategory("CatColors"), System.Windows.Forms.SRDescription("DataGridSelectionBackColorDescr")]
        public Color SelectionBackColor
        {
            get
            {
                return this.selectionBackBrush.Color;
            }
            set
            {
                if (this.isDefaultTableStyle)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultTableSet", new object[] { "SelectionBackColor" }));
                }
                if (System.Windows.Forms.DataGrid.IsTransparentColor(value))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridTableStyleTransparentSelectionBackColorNotAllowed"));
                }
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "SelectionBackColor" }));
                }
                if (!value.Equals(this.selectionBackBrush.Color))
                {
                    this.selectionBackBrush = new SolidBrush(value);
                    this.InvalidateInside();
                    this.OnSelectionBackColorChanged(EventArgs.Empty);
                }
            }
        }

        internal SolidBrush SelectionForeBrush
        {
            get
            {
                return this.selectionForeBrush;
            }
        }

        [System.Windows.Forms.SRCategory("CatColors"), System.Windows.Forms.SRDescription("DataGridSelectionForeColorDescr"), Description("The foreground color for the current data grid row")]
        public Color SelectionForeColor
        {
            get
            {
                return this.selectionForeBrush.Color;
            }
            set
            {
                if (this.isDefaultTableStyle)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridDefaultTableSet", new object[] { "SelectionForeColor" }));
                }
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "SelectionForeColor" }));
                }
                if (!value.Equals(this.selectionForeBrush.Color))
                {
                    this.selectionForeBrush = new SolidBrush(value);
                    this.InvalidateInside();
                    this.OnSelectionForeColorChanged(EventArgs.Empty);
                }
            }
        }
    }
}

