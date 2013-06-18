namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [DefaultEvent("Navigate"), ComplexBindingProperties("DataSource", "DataMember"), ClassInterface(ClassInterfaceType.AutoDispatch), Designer("System.Windows.Forms.Design.DataGridDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("DataSource"), ComVisible(true)]
    public class DataGrid : Control, ISupportInitialize, IDataGridEditingService
    {
        private DataGridAddNewRow addNewRow;
        internal bool allowColumnResize = true;
        internal bool allowRowResize = true;
        private SolidBrush alternatingBackBrush = DefaultAlternatingBackBrush;
        private SolidBrush backBrush = DefaultBackBrush;
        private EventHandler backButtonHandler;
        private SolidBrush backgroundBrush = DefaultBackgroundBrush;
        private System.Windows.Forms.BorderStyle borderStyle;
        private System.Windows.Forms.NativeMethods.RECT[] cachedScrollableRegion;
        private DataGridCaption caption;
        private int captionFontHeight = -1;
        internal bool checkHierarchy = true;
        private EventHandler currentChangedHandler;
        internal int currentCol;
        internal int currentRow;
        internal TraceSwitch DataGridAcc;
        private DataGridRow[] dataGridRows = new DataGridRow[0];
        private int dataGridRowsLength;
        internal GridTableStylesCollection dataGridTables;
        private CollectionChangeEventHandler dataGridTableStylesCollectionChanged;
        private string dataMember = "";
        private object dataSource;
        private const System.Windows.Forms.BorderStyle defaultBorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        private const bool defaultCaptionVisible = true;
        private static int defaultFontHeight = Control.DefaultFont.Height;
        private const DataGridLineStyle defaultGridLineStyle = DataGridLineStyle.Solid;
        private const DataGridParentRowsLabelStyle defaultParentRowsLabelStyle = DataGridParentRowsLabelStyle.Both;
        private const bool defaultParentRowsVisible = true;
        private const int defaultPreferredColumnWidth = 0x4b;
        private const int defaultRowHeaderWidth = 0x23;
        private DataGridTableStyle defaultTableStyle = new DataGridTableStyle(true);
        private EventHandler downButtonHandler;
        private DataGridColumnStyle editColumn;
        private DataGridRow editRow;
        private const int errorRowBitmapWidth = 15;
        private static readonly object EVENT_ALLOWNAVIGATIONCHANGED = new object();
        private static readonly object EVENT_BACKBUTTONCLICK = new object();
        private static readonly object EVENT_BACKGROUNDCOLORCHANGED = new object();
        private static readonly object EVENT_BORDERSTYLECHANGED = new object();
        private static readonly object EVENT_CAPTIONVISIBLECHANGED = new object();
        private static readonly object EVENT_CURRENTCELLCHANGED = new object();
        private static readonly object EVENT_DATASOURCECHANGED = new object();
        private static readonly object EVENT_DOWNBUTTONCLICK = new object();
        private static readonly object EVENT_FLATMODECHANGED = new object();
        private static readonly object EVENT_NODECLICKED = new object();
        private static readonly object EVENT_PARENTROWSLABELSTYLECHANGED = new object();
        private static readonly object EVENT_PARENTROWSVISIBLECHANGED = new object();
        private static readonly object EVENT_READONLYCHANGED = new object();
        private static readonly object EVENT_SCROLL = new object();
        internal int firstVisibleCol;
        internal int firstVisibleRow;
        private int fontHeight = -1;
        private SolidBrush foreBrush = DefaultForeBrush;
        private SolidBrush gridLineBrush = DefaultGridLineBrush;
        private DataGridLineStyle gridLineStyle = DataGridLineStyle.Solid;
        private BitVector32 gridState;
        private const int GRIDSTATE_allowNavigation = 0x2000;
        private const int GRIDSTATE_allowSorting = 1;
        private const int GRIDSTATE_canFocus = 0x800;
        private const int GRIDSTATE_childLinkFocused = 0x80000;
        private const int GRIDSTATE_columnHeadersVisible = 2;
        private const int GRIDSTATE_dragging = 0x100;
        private const int GRIDSTATE_editControlChanging = 0x10000;
        private const int GRIDSTATE_exceptionInPaint = 0x800000;
        private const int GRIDSTATE_inAddNewRow = 0x100000;
        private const int GRIDSTATE_inDeleteRow = 0x400;
        private const int GRIDSTATE_inListAddNew = 0x200;
        private const int GRIDSTATE_inSetListManager = 0x200000;
        private const int GRIDSTATE_isEditing = 0x8000;
        private const int GRIDSTATE_isFlatMode = 0x40;
        private const int GRIDSTATE_isLedgerStyle = 0x20;
        private const int GRIDSTATE_isNavigating = 0x4000;
        private const int GRIDSTATE_isScrolling = 0x20000;
        private const int GRIDSTATE_layoutSuspended = 0x1000000;
        private const int GRIDSTATE_listHasErrors = 0x80;
        private const int GRIDSTATE_metaDataChanged = 0x400000;
        private const int GRIDSTATE_overCaption = 0x40000;
        private const int GRIDSTATE_readOnlyMode = 0x1000;
        private const int GRIDSTATE_rowHeadersVisible = 4;
        private const int GRIDSTATE_trackColResize = 8;
        private const int GRIDSTATE_trackRowResize = 0x10;
        private SolidBrush headerBackBrush = DefaultHeaderBackBrush;
        private Font headerFont;
        private int headerFontHeight = -1;
        private SolidBrush headerForeBrush = DefaultHeaderForeBrush;
        private Pen headerForePen = DefaultHeaderForePen;
        private int horizontalOffset;
        private ScrollBar horizScrollBar = new HScrollBar();
        internal bool inInit;
        private ItemChangedEventHandler itemChangedHandler;
        private int lastRowSelected = -1;
        private MouseEventArgs lastSplitBar;
        private int lastTotallyVisibleCol;
        private LayoutData layout = new LayoutData();
        private SolidBrush linkBrush = DefaultLinkBrush;
        private Font linkFont;
        private int linkFontHeight = -1;
        private CurrencyManager listManager;
        private EventHandler metaDataChangedHandler;
        private int minRowHeaderWidth;
        internal DataGridTableStyle myGridTable;
        private int negOffset;
        private const int NumRowsForAutoResize = 10;
        private int numSelectedRows;
        private int numTotallyVisibleRows;
        private int numVisibleCols;
        private int numVisibleRows;
        private int oldRow = -1;
        private DataGridState originalState;
        private DataGridParentRows parentRows;
        internal DataGridParentRowsLabelStyle parentRowsLabels = DataGridParentRowsLabelStyle.Both;
        private Policy policy = new Policy();
        private EventHandler positionChangedHandler;
        private int preferredColumnWidth = 0x4b;
        private int prefferedRowHeight = (defaultFontHeight + 3);
        private int rowHeaderWidth = 0x23;
        private SolidBrush selectionBackBrush = DefaultSelectionBackBrush;
        private SolidBrush selectionForeBrush = DefaultSelectionForeBrush;
        private Control toBeDisposedEditingControl;
        private int toolTipId;
        private DataGridToolTip toolTipProvider;
        private int trackColAnchor;
        private int trackColumn;
        private PropertyDescriptor trackColumnHeader;
        private int trackRow;
        private int trackRowAnchor;
        private ScrollBar vertScrollBar = new VScrollBar();
        private int wheelDelta;

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("DataGridOnNavigationModeChangedDescr")]
        public event EventHandler AllowNavigationChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_ALLOWNAVIGATIONCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_ALLOWNAVIGATIONCHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("DataGridBackButtonClickDescr")]
        public event EventHandler BackButtonClick
        {
            add
            {
                base.Events.AddHandler(EVENT_BACKBUTTONCLICK, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_BACKBUTTONCLICK, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("DataGridOnBackgroundColorChangedDescr")]
        public event EventHandler BackgroundColorChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_BACKGROUNDCOLORCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_BACKGROUNDCOLORCHANGED, value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler BackgroundImageChanged
        {
            add
            {
                base.BackgroundImageChanged += value;
            }
            remove
            {
                base.BackgroundImageChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler BackgroundImageLayoutChanged
        {
            add
            {
                base.BackgroundImageLayoutChanged += value;
            }
            remove
            {
                base.BackgroundImageLayoutChanged -= value;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridOnBorderStyleChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler BorderStyleChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_BORDERSTYLECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_BORDERSTYLECHANGED, value);
            }
        }

        [System.Windows.Forms.SRDescription("DataGridOnCaptionVisibleChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler CaptionVisibleChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_CAPTIONVISIBLECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_CAPTIONVISIBLECHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("DataGridOnCurrentCellChangedDescr")]
        public event EventHandler CurrentCellChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_CURRENTCELLCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_CURRENTCELLCHANGED, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler CursorChanged
        {
            add
            {
                base.CursorChanged += value;
            }
            remove
            {
                base.CursorChanged -= value;
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("DataGridOnDataSourceChangedDescr")]
        public event EventHandler DataSourceChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_DATASOURCECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DATASOURCECHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("DataGridOnFlatModeChangedDescr")]
        public event EventHandler FlatModeChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_FLATMODECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_FLATMODECHANGED, value);
            }
        }

        [System.Windows.Forms.SRDescription("DataGridNavigateEventDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event NavigateEventHandler Navigate;

        [System.Windows.Forms.SRDescription("DataGridNodeClickEventDescr"), System.Windows.Forms.SRCategory("CatAction")]
        internal event EventHandler NodeClick
        {
            add
            {
                base.Events.AddHandler(EVENT_NODECLICKED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_NODECLICKED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("DataGridOnParentRowsLabelStyleChangedDescr")]
        public event EventHandler ParentRowsLabelStyleChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_PARENTROWSLABELSTYLECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_PARENTROWSLABELSTYLECHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("DataGridOnParentRowsVisibleChangedDescr")]
        public event EventHandler ParentRowsVisibleChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_PARENTROWSVISIBLECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_PARENTROWSVISIBLECHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("DataGridOnReadOnlyChangedDescr")]
        public event EventHandler ReadOnlyChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_READONLYCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_READONLYCHANGED, value);
            }
        }

        protected event EventHandler RowHeaderClick;

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("DataGridScrollEventDescr")]
        public event EventHandler Scroll
        {
            add
            {
                base.Events.AddHandler(EVENT_SCROLL, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_SCROLL, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("DataGridDownButtonClickDescr")]
        public event EventHandler ShowParentDetailsButtonClick
        {
            add
            {
                base.Events.AddHandler(EVENT_DOWNBUTTONCLICK, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DOWNBUTTONCLICK, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler TextChanged
        {
            add
            {
                base.TextChanged += value;
            }
            remove
            {
                base.TextChanged -= value;
            }
        }

        public DataGrid()
        {
            base.SetStyle(ControlStyles.UserPaint, true);
            base.SetStyle(ControlStyles.Opaque, false);
            base.SetStyle(ControlStyles.SupportsTransparentBackColor, false);
            base.SetStyle(ControlStyles.UserMouse, true);
            this.gridState = new BitVector32(0x42827);
            this.dataGridTables = new GridTableStylesCollection(this);
            this.layout = this.CreateInitialLayoutState();
            this.parentRows = new DataGridParentRows(this);
            this.horizScrollBar.Top = base.ClientRectangle.Height - this.horizScrollBar.Height;
            this.horizScrollBar.Left = 0;
            this.horizScrollBar.Visible = false;
            this.horizScrollBar.Scroll += new ScrollEventHandler(this.GridHScrolled);
            base.Controls.Add(this.horizScrollBar);
            this.vertScrollBar.Top = 0;
            this.vertScrollBar.Left = base.ClientRectangle.Width - this.vertScrollBar.Width;
            this.vertScrollBar.Visible = false;
            this.vertScrollBar.Scroll += new ScrollEventHandler(this.GridVScrolled);
            base.Controls.Add(this.vertScrollBar);
            this.BackColor = DefaultBackBrush.Color;
            this.ForeColor = DefaultForeBrush.Color;
            this.borderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.currentChangedHandler = new EventHandler(this.DataSource_RowChanged);
            this.positionChangedHandler = new EventHandler(this.DataSource_PositionChanged);
            this.itemChangedHandler = new ItemChangedEventHandler(this.DataSource_ItemChanged);
            this.metaDataChangedHandler = new EventHandler(this.DataSource_MetaDataChanged);
            this.dataGridTableStylesCollectionChanged = new CollectionChangeEventHandler(this.TableStylesCollectionChanged);
            this.dataGridTables.CollectionChanged += this.dataGridTableStylesCollectionChanged;
            this.SetDataGridTable(this.defaultTableStyle, true);
            this.backButtonHandler = new EventHandler(this.OnBackButtonClicked);
            this.downButtonHandler = new EventHandler(this.OnShowParentDetailsButtonClicked);
            this.caption = new DataGridCaption(this);
            this.caption.BackwardClicked += this.backButtonHandler;
            this.caption.DownClicked += this.downButtonHandler;
            this.RecalculateFonts();
            base.Size = new Size(130, 80);
            base.Invalidate();
            base.PerformLayout();
        }

        private void AbortEdit()
        {
            this.gridState[0x10000] = true;
            this.editColumn.Abort(this.editRow.RowNumber);
            this.gridState[0x10000] = false;
            this.gridState[0x8000] = false;
            this.editRow = null;
            this.editColumn = null;
        }

        internal void AddNewRow()
        {
            this.EnsureBound();
            this.ResetSelection();
            this.UpdateListManager();
            this.gridState[0x200] = true;
            this.gridState[0x100000] = true;
            try
            {
                this.ListManager.AddNew();
            }
            catch
            {
                this.gridState[0x200] = false;
                this.gridState[0x100000] = false;
                base.PerformLayout();
                this.InvalidateInside();
                throw;
            }
            this.gridState[0x200] = false;
        }

        private void AllowSortingChanged(object sender, EventArgs e)
        {
            if (!this.myGridTable.AllowSorting && (this.listManager != null))
            {
                IList list = this.listManager.List;
                if (list is IBindingList)
                {
                    ((IBindingList) list).RemoveSort();
                }
            }
        }

        public bool BeginEdit(DataGridColumnStyle gridColumn, int rowNumber)
        {
            if ((this.DataSource == null) || (this.myGridTable == null))
            {
                return false;
            }
            if (this.gridState[0x8000])
            {
                return false;
            }
            int c = -1;
            c = this.myGridTable.GridColumnStyles.IndexOf(gridColumn);
            if (c < 0)
            {
                return false;
            }
            this.CurrentCell = new DataGridCell(rowNumber, c);
            this.ResetSelection();
            this.Edit();
            return true;
        }

        public void BeginInit()
        {
            if (this.inInit)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridBeginInit"));
            }
            this.inInit = true;
        }

        private Rectangle CalcColResizeFeedbackRect(MouseEventArgs e)
        {
            Rectangle rectangle2;
            Rectangle data = this.layout.Data;
            return new Rectangle(e.X, data.Y, 3, data.Height) { X = Math.Min(data.Right - 3, rectangle2.X), X = Math.Max(rectangle2.X, 0) };
        }

        private Rectangle CalcRowResizeFeedbackRect(MouseEventArgs e)
        {
            Rectangle rectangle2;
            Rectangle data = this.layout.Data;
            return new Rectangle(data.X, e.Y, data.Width, 3) { Y = Math.Min(data.Bottom - 3, rectangle2.Y), Y = Math.Max(rectangle2.Y, 0) };
        }

        private void CancelCursorUpdate()
        {
            if (this.listManager != null)
            {
                this.EndEdit();
                this.listManager.CancelCurrentEdit();
            }
        }

        protected virtual void CancelEditing()
        {
            this.CancelCursorUpdate();
            if (this.gridState[0x100000])
            {
                this.gridState[0x100000] = false;
                DataGridRow[] dataGridRows = this.DataGridRows;
                dataGridRows[this.DataGridRowsLength - 1] = new DataGridAddNewRow(this, this.myGridTable, this.DataGridRowsLength - 1);
                this.SetDataGridRows(dataGridRows, this.DataGridRowsLength);
            }
        }

        private void CheckHierarchyState()
        {
            if (((this.checkHierarchy && (this.listManager != null)) && (this.myGridTable != null)) && (this.myGridTable != null))
            {
                for (int i = 0; i < this.myGridTable.GridColumnStyles.Count; i++)
                {
                    DataGridColumnStyle style1 = this.myGridTable.GridColumnStyles[i];
                }
                this.checkHierarchy = false;
            }
        }

        private void ClearRegionCache()
        {
            this.cachedScrollableRegion = null;
        }

        private void ColAutoResize(int col)
        {
            this.EndEdit();
            CurrencyManager listManager = this.listManager;
            if (listManager != null)
            {
                using (Graphics graphics = base.CreateGraphicsInternal())
                {
                    Font headerFont;
                    DataGridColumnStyle style = this.myGridTable.GridColumnStyles[col];
                    string headerText = style.HeaderText;
                    if (this.myGridTable.IsDefault)
                    {
                        headerFont = this.HeaderFont;
                    }
                    else
                    {
                        headerFont = this.myGridTable.HeaderFont;
                    }
                    int num = (((int) graphics.MeasureString(headerText, headerFont).Width) + this.layout.ColumnHeaders.Height) + 1;
                    int count = listManager.Count;
                    for (int i = 0; i < count; i++)
                    {
                        object columnValueAtRow = style.GetColumnValueAtRow(listManager, i);
                        int width = style.GetPreferredSize(graphics, columnValueAtRow).Width;
                        if (width > num)
                        {
                            num = width;
                        }
                    }
                    if (style.Width != num)
                    {
                        style.width = num;
                        this.ComputeVisibleColumns();
                        bool flag = true;
                        if (this.lastTotallyVisibleCol != -1)
                        {
                            for (int j = this.lastTotallyVisibleCol + 1; j < this.myGridTable.GridColumnStyles.Count; j++)
                            {
                                if (this.myGridTable.GridColumnStyles[j].PropertyDescriptor != null)
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            flag = false;
                        }
                        if (flag && ((this.negOffset != 0) || (this.horizontalOffset != 0)))
                        {
                            style.width = num;
                            int num6 = 0;
                            int num7 = this.myGridTable.GridColumnStyles.Count;
                            int num8 = this.layout.Data.Width;
                            GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
                            this.negOffset = 0;
                            this.horizontalOffset = 0;
                            this.firstVisibleCol = 0;
                            for (int k = num7 - 1; k >= 0; k--)
                            {
                                if (gridColumnStyles[k].PropertyDescriptor != null)
                                {
                                    num6 += gridColumnStyles[k].Width;
                                    if (num6 > num8)
                                    {
                                        if (this.negOffset == 0)
                                        {
                                            this.firstVisibleCol = k;
                                            this.negOffset = num6 - num8;
                                            this.horizontalOffset = this.negOffset;
                                            this.numVisibleCols++;
                                        }
                                        else
                                        {
                                            this.horizontalOffset += gridColumnStyles[k].Width;
                                        }
                                    }
                                    else
                                    {
                                        this.numVisibleCols++;
                                    }
                                }
                            }
                            base.PerformLayout();
                            base.Invalidate(Rectangle.Union(this.layout.Data, this.layout.ColumnHeaders));
                        }
                        else
                        {
                            base.PerformLayout();
                            Rectangle data = this.layout.Data;
                            if (this.layout.ColumnHeadersVisible)
                            {
                                data = Rectangle.Union(data, this.layout.ColumnHeaders);
                            }
                            int colBeg = this.GetColBeg(col);
                            if (!this.isRightToLeft())
                            {
                                data.Width -= colBeg - data.X;
                                data.X = colBeg;
                            }
                            else
                            {
                                data.Width -= colBeg;
                            }
                            base.Invalidate(data);
                        }
                    }
                }
                if (this.horizScrollBar.Visible)
                {
                    this.horizScrollBar.Value = this.HorizontalOffset;
                }
            }
        }

        public void Collapse(int row)
        {
            this.SetRowExpansionState(row, false);
        }

        private void ColResizeBegin(MouseEventArgs e, int col)
        {
            int x = e.X;
            this.EndEdit();
            Rectangle r = Rectangle.Union(this.layout.ColumnHeaders, this.layout.Data);
            if (this.isRightToLeft())
            {
                r.Width = (this.GetColBeg(col) - this.layout.Data.X) - 2;
            }
            else
            {
                int colBeg = this.GetColBeg(col);
                r.X = colBeg + 3;
                r.Width = ((this.layout.Data.X + this.layout.Data.Width) - colBeg) - 2;
            }
            base.CaptureInternal = true;
            System.Windows.Forms.Cursor.ClipInternal = base.RectangleToScreen(r);
            this.gridState[8] = true;
            this.trackColAnchor = x;
            this.trackColumn = col;
            this.DrawColSplitBar(e);
            this.lastSplitBar = e;
        }

        private void ColResizeEnd(MouseEventArgs e)
        {
            this.gridState[0x1000000] = true;
            try
            {
                if (this.lastSplitBar != null)
                {
                    this.DrawColSplitBar(this.lastSplitBar);
                    this.lastSplitBar = null;
                }
                bool flag = this.isRightToLeft();
                int num = flag ? Math.Max(e.X, this.layout.Data.X) : Math.Min(e.X, this.layout.Data.Right + 1);
                int num2 = num - this.GetColEnd(this.trackColumn);
                if (flag)
                {
                    num2 = -num2;
                }
                if ((this.trackColAnchor != num) && (num2 != 0))
                {
                    DataGridColumnStyle style = this.myGridTable.GridColumnStyles[this.trackColumn];
                    int num3 = style.Width + num2;
                    num3 = Math.Max(num3, 3);
                    style.Width = num3;
                    this.ComputeVisibleColumns();
                    bool flag2 = true;
                    for (int i = this.lastTotallyVisibleCol + 1; i < this.myGridTable.GridColumnStyles.Count; i++)
                    {
                        if (this.myGridTable.GridColumnStyles[i].PropertyDescriptor != null)
                        {
                            flag2 = false;
                            break;
                        }
                    }
                    if (flag2 && ((this.negOffset != 0) || (this.horizontalOffset != 0)))
                    {
                        int num5 = 0;
                        int count = this.myGridTable.GridColumnStyles.Count;
                        int width = this.layout.Data.Width;
                        GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
                        this.negOffset = 0;
                        this.horizontalOffset = 0;
                        this.firstVisibleCol = 0;
                        for (int j = count - 1; j > -1; j--)
                        {
                            if (gridColumnStyles[j].PropertyDescriptor != null)
                            {
                                num5 += gridColumnStyles[j].Width;
                                if (num5 > width)
                                {
                                    if (this.negOffset == 0)
                                    {
                                        this.negOffset = num5 - width;
                                        this.firstVisibleCol = j;
                                        this.horizontalOffset = this.negOffset;
                                        this.numVisibleCols++;
                                    }
                                    else
                                    {
                                        this.horizontalOffset += gridColumnStyles[j].Width;
                                    }
                                }
                                else
                                {
                                    this.numVisibleCols++;
                                }
                            }
                        }
                        base.Invalidate(Rectangle.Union(this.layout.Data, this.layout.ColumnHeaders));
                    }
                    else
                    {
                        Rectangle rc = Rectangle.Union(this.layout.ColumnHeaders, this.layout.Data);
                        int colBeg = this.GetColBeg(this.trackColumn);
                        rc.Width -= flag ? (rc.Right - colBeg) : (colBeg - rc.X);
                        rc.X = flag ? this.layout.Data.X : colBeg;
                        base.Invalidate(rc);
                    }
                }
            }
            finally
            {
                System.Windows.Forms.Cursor.ClipInternal = Rectangle.Empty;
                base.CaptureInternal = false;
                this.gridState[0x1000000] = false;
            }
            base.PerformLayout();
            if (this.horizScrollBar.Visible)
            {
                this.horizScrollBar.Value = this.HorizontalOffset;
            }
        }

        private void ColResizeMove(MouseEventArgs e)
        {
            if (this.lastSplitBar != null)
            {
                this.DrawColSplitBar(this.lastSplitBar);
                this.lastSplitBar = e;
            }
            this.DrawColSplitBar(e);
        }

        private void ColumnHeaderClicked(PropertyDescriptor prop)
        {
            if (this.CommitEdit())
            {
                bool allowSorting;
                if (this.myGridTable.IsDefault)
                {
                    allowSorting = this.AllowSorting;
                }
                else
                {
                    allowSorting = this.myGridTable.AllowSorting;
                }
                if (allowSorting)
                {
                    ListSortDirection sortDirection = this.ListManager.GetSortDirection();
                    PropertyDescriptor sortProperty = this.ListManager.GetSortProperty();
                    if ((sortProperty != null) && sortProperty.Equals(prop))
                    {
                        sortDirection = (sortDirection == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
                    }
                    else
                    {
                        sortDirection = ListSortDirection.Ascending;
                    }
                    if (this.listManager.Count != 0)
                    {
                        this.ListManager.SetSort(prop, sortDirection);
                        this.ResetSelection();
                        this.InvalidateInside();
                    }
                }
            }
        }

        private void ColumnHeadersVisibleChanged(object sender, EventArgs e)
        {
            this.layout.ColumnHeadersVisible = (this.myGridTable != null) && this.myGridTable.ColumnHeadersVisible;
            base.PerformLayout();
            this.InvalidateInside();
        }

        protected internal virtual void ColumnStartedEditing(Rectangle bounds)
        {
            DataGridRow[] dataGridRows = this.DataGridRows;
            if ((bounds.IsEmpty && (this.editColumn is DataGridTextBoxColumn)) && ((this.currentRow != -1) && (this.currentCol != -1)))
            {
                DataGridTextBoxColumn editColumn = this.editColumn as DataGridTextBoxColumn;
                Rectangle cellBounds = this.GetCellBounds(this.currentRow, this.currentCol);
                this.gridState[0x10000] = true;
                try
                {
                    editColumn.TextBox.Bounds = cellBounds;
                }
                finally
                {
                    this.gridState[0x10000] = false;
                }
            }
            if (this.gridState[0x100000])
            {
                int dataGridRowsLength = this.DataGridRowsLength;
                DataGridRow[] newRows = new DataGridRow[dataGridRowsLength + 1];
                for (int i = 0; i < dataGridRowsLength; i++)
                {
                    newRows[i] = dataGridRows[i];
                }
                newRows[dataGridRowsLength] = new DataGridAddNewRow(this, this.myGridTable, dataGridRowsLength);
                this.SetDataGridRows(newRows, dataGridRowsLength + 1);
                this.Edit();
                this.gridState[0x100000] = false;
                this.gridState[0x8000] = true;
                this.gridState[0x4000] = false;
            }
            else
            {
                this.gridState[0x8000] = true;
                this.gridState[0x4000] = false;
                this.InvalidateRowHeader(this.currentRow);
                dataGridRows[this.currentRow].LoseChildFocus(this.layout.RowHeaders, this.isRightToLeft());
            }
        }

        protected internal virtual void ColumnStartedEditing(Control editingControl)
        {
            this.ColumnStartedEditing(editingControl.Bounds);
        }

        private bool CommitEdit()
        {
            if ((!this.gridState[0x8000] && !this.gridState[0x4000]) || (this.gridState[0x10000] && !this.gridState[0x20000]))
            {
                return true;
            }
            this.gridState[0x10000] = true;
            if (this.editColumn.ReadOnly || this.gridState[0x100000])
            {
                bool flag = false;
                if (base.ContainsFocus)
                {
                    flag = true;
                }
                if (flag && this.gridState[0x800])
                {
                    this.FocusInternal();
                }
                this.editColumn.ConcedeFocus();
                if ((flag && this.gridState[0x800]) && (base.CanFocus && !this.Focused))
                {
                    this.FocusInternal();
                }
                this.gridState[0x10000] = false;
                return true;
            }
            bool flag2 = this.editColumn.Commit(this.ListManager, this.currentRow);
            this.gridState[0x10000] = false;
            if (flag2)
            {
                this.gridState[0x8000] = false;
            }
            return flag2;
        }

        private int ComputeDeltaRows(int targetRow)
        {
            if (this.firstVisibleRow == targetRow)
            {
                return 0;
            }
            int firstVisibleRowLogicalTop = -1;
            int num3 = -1;
            int dataGridRowsLength = this.DataGridRowsLength;
            int num5 = 0;
            DataGridRow[] dataGridRows = this.DataGridRows;
            for (int i = 0; i < dataGridRowsLength; i++)
            {
                if (i == this.firstVisibleRow)
                {
                    firstVisibleRowLogicalTop = num5;
                }
                if (i == targetRow)
                {
                    num3 = num5;
                }
                if ((num3 != -1) && (firstVisibleRowLogicalTop != -1))
                {
                    break;
                }
                num5 += dataGridRows[i].Height;
            }
            int num7 = num3 + dataGridRows[targetRow].Height;
            int num8 = this.layout.Data.Height + firstVisibleRowLogicalTop;
            if (num7 > num8)
            {
                int num9 = num7 - num8;
                firstVisibleRowLogicalTop += num9;
            }
            else
            {
                if (firstVisibleRowLogicalTop < num3)
                {
                    return 0;
                }
                int num10 = firstVisibleRowLogicalTop - num3;
                firstVisibleRowLogicalTop -= num10;
            }
            return (this.ComputeFirstVisibleRow(firstVisibleRowLogicalTop) - this.firstVisibleRow);
        }

        private int ComputeFirstVisibleColumn()
        {
            int num = 0;
            if (this.horizontalOffset == 0)
            {
                this.negOffset = 0;
                return 0;
            }
            if (((this.myGridTable != null) && (this.myGridTable.GridColumnStyles != null)) && (this.myGridTable.GridColumnStyles.Count != 0))
            {
                GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
                int num2 = 0;
                int count = gridColumnStyles.Count;
                if (gridColumnStyles[0].Width == -1)
                {
                    this.negOffset = 0;
                    return 0;
                }
                num = 0;
                while (num < count)
                {
                    if (gridColumnStyles[num].PropertyDescriptor != null)
                    {
                        num2 += gridColumnStyles[num].Width;
                    }
                    if (num2 > this.horizontalOffset)
                    {
                        break;
                    }
                    num++;
                }
                if (num == count)
                {
                    this.negOffset = 0;
                    return 0;
                }
                this.negOffset = gridColumnStyles[num].Width - (num2 - this.horizontalOffset);
            }
            return num;
        }

        private int ComputeFirstVisibleRow(int firstVisibleRowLogicalTop)
        {
            int dataGridRowsLength = this.DataGridRowsLength;
            int num3 = 0;
            DataGridRow[] dataGridRows = this.DataGridRows;
            int index = 0;
            while (index < dataGridRowsLength)
            {
                if (num3 >= firstVisibleRowLogicalTop)
                {
                    return index;
                }
                num3 += dataGridRows[index].Height;
                index++;
            }
            return index;
        }

        private void ComputeLayout()
        {
            bool flag = !this.isRightToLeft();
            Rectangle resizeBoxRect = this.layout.ResizeBoxRect;
            this.EndEdit();
            this.ClearRegionCache();
            LayoutData data = new LayoutData(this.layout) {
                Inside = base.ClientRectangle
            };
            Rectangle inside = data.Inside;
            int borderWidth = this.BorderWidth;
            inside.Inflate(-borderWidth, -borderWidth);
            Rectangle rectangle3 = inside;
            if (this.layout.CaptionVisible)
            {
                int num2 = this.captionFontHeight + 6;
                Rectangle caption = data.Caption;
                caption = rectangle3;
                caption.Height = num2;
                rectangle3.Y += num2;
                rectangle3.Height -= num2;
                data.Caption = caption;
            }
            else
            {
                data.Caption = Rectangle.Empty;
            }
            if (this.layout.ParentRowsVisible)
            {
                Rectangle parentRows = data.ParentRows;
                int height = this.parentRows.Height;
                parentRows = rectangle3;
                parentRows.Height = height;
                rectangle3.Y += height;
                rectangle3.Height -= height;
                data.ParentRows = parentRows;
            }
            else
            {
                data.ParentRows = Rectangle.Empty;
            }
            int num4 = this.headerFontHeight + 6;
            if (this.layout.ColumnHeadersVisible)
            {
                Rectangle columnHeaders = data.ColumnHeaders;
                columnHeaders = rectangle3;
                columnHeaders.Height = num4;
                rectangle3.Y += num4;
                rectangle3.Height -= num4;
                data.ColumnHeaders = columnHeaders;
            }
            else
            {
                data.ColumnHeaders = Rectangle.Empty;
            }
            bool flag2 = this.myGridTable.IsDefault ? this.RowHeadersVisible : this.myGridTable.RowHeadersVisible;
            int num5 = this.myGridTable.IsDefault ? this.RowHeaderWidth : this.myGridTable.RowHeaderWidth;
            data.RowHeadersVisible = flag2;
            if ((this.myGridTable != null) && flag2)
            {
                Rectangle rowHeaders = data.RowHeaders;
                if (flag)
                {
                    rowHeaders = rectangle3;
                    rowHeaders.Width = num5;
                    rectangle3.X += num5;
                    rectangle3.Width -= num5;
                }
                else
                {
                    rowHeaders = rectangle3;
                    rowHeaders.Width = num5;
                    rowHeaders.X = rectangle3.Right - num5;
                    rectangle3.Width -= num5;
                }
                data.RowHeaders = rowHeaders;
                if (this.layout.ColumnHeadersVisible)
                {
                    Rectangle topLeftHeader = data.TopLeftHeader;
                    Rectangle rectangle9 = data.ColumnHeaders;
                    if (flag)
                    {
                        topLeftHeader = rectangle9;
                        topLeftHeader.Width = num5;
                        rectangle9.Width -= num5;
                        rectangle9.X += num5;
                    }
                    else
                    {
                        topLeftHeader = rectangle9;
                        topLeftHeader.Width = num5;
                        topLeftHeader.X = rectangle9.Right - num5;
                        rectangle9.Width -= num5;
                    }
                    data.TopLeftHeader = topLeftHeader;
                    data.ColumnHeaders = rectangle9;
                }
                else
                {
                    data.TopLeftHeader = Rectangle.Empty;
                }
            }
            else
            {
                data.RowHeaders = Rectangle.Empty;
                data.TopLeftHeader = Rectangle.Empty;
            }
            data.Data = rectangle3;
            data.Inside = inside;
            this.layout = data;
            this.LayoutScrollBars();
            if (!resizeBoxRect.Equals(this.layout.ResizeBoxRect) && !this.layout.ResizeBoxRect.IsEmpty)
            {
                base.Invalidate(this.layout.ResizeBoxRect);
            }
            this.layout.dirty = false;
        }

        internal void ComputeMinimumRowHeaderWidth()
        {
            this.minRowHeaderWidth = 15;
            if (this.ListHasErrors)
            {
                this.minRowHeaderWidth += 15;
            }
            if ((this.myGridTable != null) && (this.myGridTable.RelationsList.Count != 0))
            {
                this.minRowHeaderWidth += 15;
            }
        }

        private int ComputeRowDelta(int from, int to)
        {
            int num = from;
            int num2 = to;
            int num3 = -1;
            if (num > num2)
            {
                num = to;
                num2 = from;
                num3 = 1;
            }
            DataGridRow[] dataGridRows = this.DataGridRows;
            int num4 = 0;
            for (int i = num; i < num2; i++)
            {
                num4 += dataGridRows[i].Height;
            }
            return (num3 * num4);
        }

        private void ComputeVisibleColumns()
        {
            this.EnsureBound();
            GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
            int count = gridColumnStyles.Count;
            int num2 = -this.negOffset;
            int num3 = 0;
            int width = this.layout.Data.Width;
            int firstVisibleCol = this.firstVisibleCol;
            if ((width >= 0) && (gridColumnStyles.Count != 0))
            {
                while ((num2 < width) && (firstVisibleCol < count))
                {
                    if (gridColumnStyles[firstVisibleCol].PropertyDescriptor != null)
                    {
                        num2 += gridColumnStyles[firstVisibleCol].Width;
                    }
                    firstVisibleCol++;
                    num3++;
                }
                this.numVisibleCols = num3;
                if (num2 >= width)
                {
                    goto Label_0133;
                }
                for (int i = this.firstVisibleCol - 1; i > 0; i--)
                {
                    if ((num2 + gridColumnStyles[i].Width) > width)
                    {
                        break;
                    }
                    if (gridColumnStyles[i].PropertyDescriptor != null)
                    {
                        num2 += gridColumnStyles[i].Width;
                    }
                    num3++;
                    this.firstVisibleCol--;
                }
            }
            else
            {
                this.numVisibleCols = this.firstVisibleCol = 0;
                this.lastTotallyVisibleCol = -1;
                return;
            }
            if (this.numVisibleCols != num3)
            {
                base.Invalidate(this.layout.Data);
                base.Invalidate(this.layout.ColumnHeaders);
                this.numVisibleCols = num3;
            }
        Label_0133:
            this.lastTotallyVisibleCol = (this.firstVisibleCol + this.numVisibleCols) - 1;
            if (num2 > width)
            {
                if ((this.numVisibleCols <= 1) || ((this.numVisibleCols == 2) && (this.negOffset != 0)))
                {
                    this.lastTotallyVisibleCol = -1;
                }
                else
                {
                    this.lastTotallyVisibleCol--;
                }
            }
        }

        private void ComputeVisibleRows()
        {
            this.EnsureBound();
            int height = this.layout.Data.Height;
            int num2 = 0;
            int num3 = 0;
            DataGridRow[] dataGridRows = this.DataGridRows;
            int dataGridRowsLength = this.DataGridRowsLength;
            if (height < 0)
            {
                this.numVisibleRows = this.numTotallyVisibleRows = 0;
            }
            else
            {
                for (int i = this.firstVisibleRow; i < dataGridRowsLength; i++)
                {
                    if (num2 > height)
                    {
                        break;
                    }
                    num2 += dataGridRows[i].Height;
                    num3++;
                }
                if (num2 < height)
                {
                    for (int j = this.firstVisibleRow - 1; j >= 0; j--)
                    {
                        int num7 = dataGridRows[j].Height;
                        if ((num2 + num7) > height)
                        {
                            break;
                        }
                        num2 += num7;
                        this.firstVisibleRow--;
                        num3++;
                    }
                }
                this.numVisibleRows = this.numTotallyVisibleRows = num3;
                if (num2 > height)
                {
                    this.numTotallyVisibleRows--;
                }
            }
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new DataGridAccessibleObject(this);
        }

        private DataGridState CreateChildState(string relationName, DataGridRow source)
        {
            string str;
            DataGridState state = new DataGridState();
            if (string.IsNullOrEmpty(this.DataMember))
            {
                str = relationName;
            }
            else
            {
                str = this.DataMember + "." + relationName;
            }
            CurrencyManager manager = (CurrencyManager) this.BindingContext[this.DataSource, str];
            state.DataSource = this.DataSource;
            state.DataMember = str;
            state.ListManager = manager;
            state.DataGridRows = null;
            state.DataGridRowsLength = manager.Count + (this.policy.AllowAdd ? 1 : 0);
            return state;
        }

        private void CreateDataGridRows()
        {
            CurrencyManager listManager = this.ListManager;
            DataGridTableStyle myGridTable = this.myGridTable;
            this.InitializeColumnWidths();
            if (listManager == null)
            {
                this.SetDataGridRows(new DataGridRow[0], 0);
            }
            else
            {
                int count = listManager.Count;
                if (this.policy.AllowAdd)
                {
                    count++;
                }
                DataGridRow[] newRows = new DataGridRow[count];
                for (int i = 0; i < listManager.Count; i++)
                {
                    newRows[i] = new DataGridRelationshipRow(this, myGridTable, i);
                }
                if (this.policy.AllowAdd)
                {
                    this.addNewRow = new DataGridAddNewRow(this, myGridTable, count - 1);
                    newRows[count - 1] = this.addNewRow;
                }
                else
                {
                    this.addNewRow = null;
                }
                this.SetDataGridRows(newRows, count);
            }
        }

        protected virtual DataGridColumnStyle CreateGridColumn(PropertyDescriptor prop)
        {
            if (this.myGridTable != null)
            {
                return this.myGridTable.CreateGridColumn(prop);
            }
            return null;
        }

        protected virtual DataGridColumnStyle CreateGridColumn(PropertyDescriptor prop, bool isDefault)
        {
            if (this.myGridTable != null)
            {
                return this.myGridTable.CreateGridColumn(prop, isDefault);
            }
            return null;
        }

        private LayoutData CreateInitialLayoutState()
        {
            return new LayoutData { Inside = new Rectangle(), TopLeftHeader = new Rectangle(), ColumnHeaders = new Rectangle(), RowHeaders = new Rectangle(), Data = new Rectangle(), Caption = new Rectangle(), ParentRows = new Rectangle(), ResizeBoxRect = new Rectangle(), ColumnHeadersVisible = true, RowHeadersVisible = true, CaptionVisible = true, ParentRowsVisible = true, ClientRectangle = base.ClientRectangle };
        }

        private System.Windows.Forms.NativeMethods.RECT[] CreateScrollableRegion(Rectangle scroll)
        {
            if (this.cachedScrollableRegion == null)
            {
                bool rightToLeft = this.isRightToLeft();
                using (Region region = new Region(scroll))
                {
                    int numVisibleRows = this.numVisibleRows;
                    int y = this.layout.Data.Y;
                    int x = this.layout.Data.X;
                    DataGridRow[] dataGridRows = this.DataGridRows;
                    for (int i = this.firstVisibleRow; i < numVisibleRows; i++)
                    {
                        int height = dataGridRows[i].Height;
                        Rectangle nonScrollableArea = dataGridRows[i].GetNonScrollableArea();
                        nonScrollableArea.X += x;
                        nonScrollableArea.X = this.MirrorRectangle(nonScrollableArea, this.layout.Data, rightToLeft);
                        if (!nonScrollableArea.IsEmpty)
                        {
                            region.Exclude(new Rectangle(nonScrollableArea.X, nonScrollableArea.Y + y, nonScrollableArea.Width, nonScrollableArea.Height));
                        }
                        y += height;
                    }
                    using (Graphics graphics = base.CreateGraphicsInternal())
                    {
                        IntPtr hrgn = region.GetHrgn(graphics);
                        if (hrgn != IntPtr.Zero)
                        {
                            this.cachedScrollableRegion = System.Windows.Forms.UnsafeNativeMethods.GetRectsFromRegion(hrgn);
                            System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Assert();
                            try
                            {
                                region.ReleaseHrgn(hrgn);
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                            }
                        }
                    }
                }
            }
            return this.cachedScrollableRegion;
        }

        private bool DataGridSourceHasErrors()
        {
            if (this.listManager != null)
            {
                for (int i = 0; i < this.listManager.Count; i++)
                {
                    object obj2 = this.listManager[i];
                    if (obj2 is IDataErrorInfo)
                    {
                        string error = ((IDataErrorInfo) obj2).Error;
                        if ((error != null) && (error.Length != 0))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void DataSource_Changed(object sender, EventArgs ea)
        {
            this.policy.UpdatePolicy(this.ListManager, this.ReadOnly);
            if (this.gridState[0x200])
            {
                DataGridRow[] dataGridRows = this.DataGridRows;
                int dataGridRowsLength = this.DataGridRowsLength;
                dataGridRows[dataGridRowsLength - 1] = new DataGridRelationshipRow(this, this.myGridTable, dataGridRowsLength - 1);
                this.SetDataGridRows(dataGridRows, dataGridRowsLength);
            }
            else if (this.gridState[0x100000] && !this.gridState[0x400])
            {
                this.listManager.CancelCurrentEdit();
                this.gridState[0x100000] = false;
                this.RecreateDataGridRows();
            }
            else if (!this.gridState[0x400])
            {
                this.RecreateDataGridRows();
                this.currentRow = Math.Min(this.currentRow, this.listManager.Count);
            }
            bool listHasErrors = this.ListHasErrors;
            this.ListHasErrors = this.DataGridSourceHasErrors();
            if (listHasErrors == this.ListHasErrors)
            {
                this.InvalidateInside();
            }
        }

        private void DataSource_ItemChanged(object sender, ItemChangedEventArgs ea)
        {
            if (ea.Index == -1)
            {
                this.DataSource_Changed(sender, EventArgs.Empty);
            }
            else
            {
                object obj2 = this.listManager[ea.Index];
                bool listHasErrors = this.ListHasErrors;
                if (obj2 is IDataErrorInfo)
                {
                    if (((IDataErrorInfo) obj2).Error.Length != 0)
                    {
                        this.ListHasErrors = true;
                    }
                    else if (this.ListHasErrors)
                    {
                        this.ListHasErrors = this.DataGridSourceHasErrors();
                    }
                }
                if (listHasErrors == this.ListHasErrors)
                {
                    this.InvalidateRow(ea.Index);
                }
                if ((this.editColumn != null) && (ea.Index == this.currentRow))
                {
                    this.editColumn.UpdateUI(this.ListManager, ea.Index, null);
                }
            }
        }

        internal void DataSource_MetaDataChanged(object sender, EventArgs e)
        {
            this.MetaDataChanged();
        }

        private void DataSource_PositionChanged(object sender, EventArgs ea)
        {
            if ((this.DataGridRowsLength > (this.listManager.Count + (this.policy.AllowAdd ? 1 : 0))) && !this.gridState[0x400])
            {
                this.RecreateDataGridRows();
            }
            if (this.ListManager.Position != this.currentRow)
            {
                this.CurrentCell = new DataGridCell(this.listManager.Position, this.currentCol);
            }
        }

        private void DataSource_RowChanged(object sender, EventArgs ea)
        {
            DataGridRow[] dataGridRows = this.DataGridRows;
            if (this.currentRow < this.DataGridRowsLength)
            {
                this.InvalidateRow(this.currentRow);
            }
        }

        private void DeleteDataGridRows(int deletedRows)
        {
            if (deletedRows != 0)
            {
                int dataGridRowsLength = this.DataGridRowsLength;
                int newRowsLength = (dataGridRowsLength - deletedRows) + (this.gridState[0x100000] ? 1 : 0);
                DataGridRow[] newRows = new DataGridRow[newRowsLength];
                DataGridRow[] dataGridRows = this.DataGridRows;
                int num3 = 0;
                for (int i = 0; i < dataGridRowsLength; i++)
                {
                    if (dataGridRows[i].Selected)
                    {
                        num3++;
                    }
                    else
                    {
                        newRows[i - num3] = dataGridRows[i];
                        newRows[i - num3].number = i - num3;
                    }
                }
                if (this.gridState[0x100000])
                {
                    newRows[dataGridRowsLength - num3] = new DataGridAddNewRow(this, this.myGridTable, dataGridRowsLength - num3);
                    this.gridState[0x100000] = false;
                }
                this.SetDataGridRows(newRows, newRowsLength);
            }
        }

        private void DeleteRows(DataGridRow[] localGridRows)
        {
            int deletedRows = 0;
            int num2 = (this.listManager == null) ? 0 : this.listManager.Count;
            if (base.Visible)
            {
                base.BeginUpdateInternal();
            }
            try
            {
                if (this.ListManager != null)
                {
                    for (int i = 0; i < this.DataGridRowsLength; i++)
                    {
                        if (localGridRows[i].Selected)
                        {
                            if (localGridRows[i] is DataGridAddNewRow)
                            {
                                localGridRows[i].Selected = false;
                            }
                            else
                            {
                                this.ListManager.RemoveAt(i - deletedRows);
                                deletedRows++;
                            }
                        }
                    }
                }
            }
            catch
            {
                this.RecreateDataGridRows();
                this.gridState[0x400] = false;
                if (base.Visible)
                {
                    base.EndUpdateInternal();
                }
                throw;
            }
            if ((this.listManager != null) && (num2 == (this.listManager.Count + deletedRows)))
            {
                this.DeleteDataGridRows(deletedRows);
            }
            else
            {
                this.RecreateDataGridRows();
            }
            this.gridState[0x400] = false;
            if (base.Visible)
            {
                base.EndUpdateInternal();
            }
            if ((this.listManager != null) && (num2 != (this.listManager.Count + deletedRows)))
            {
                base.Invalidate();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.vertScrollBar != null)
                {
                    this.vertScrollBar.Dispose();
                }
                if (this.horizScrollBar != null)
                {
                    this.horizScrollBar.Dispose();
                }
                if (this.toBeDisposedEditingControl != null)
                {
                    this.toBeDisposedEditingControl.Dispose();
                    this.toBeDisposedEditingControl = null;
                }
                GridTableStylesCollection tableStyles = this.TableStyles;
                if (tableStyles != null)
                {
                    for (int i = 0; i < tableStyles.Count; i++)
                    {
                        tableStyles[i].Dispose();
                    }
                }
            }
            base.Dispose(disposing);
        }

        private void DrawColSplitBar(MouseEventArgs e)
        {
            Rectangle r = this.CalcColResizeFeedbackRect(e);
            this.DrawSplitBar(r);
        }

        private void DrawRowSplitBar(MouseEventArgs e)
        {
            Rectangle r = this.CalcRowResizeFeedbackRect(e);
            this.DrawSplitBar(r);
        }

        private void DrawSplitBar(Rectangle r)
        {
            IntPtr handle = base.Handle;
            IntPtr ptr2 = System.Windows.Forms.UnsafeNativeMethods.GetDCEx(new HandleRef(this, handle), System.Windows.Forms.NativeMethods.NullHandleRef, 0x402);
            IntPtr ptr3 = ControlPaint.CreateHalftoneHBRUSH();
            IntPtr ptr4 = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(this, ptr2), new HandleRef(null, ptr3));
            System.Windows.Forms.SafeNativeMethods.PatBlt(new HandleRef(this, ptr2), r.X, r.Y, r.Width, r.Height, 0x5a0049);
            System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(this, ptr2), new HandleRef(null, ptr4));
            System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, ptr3));
            System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(new HandleRef(this, handle), new HandleRef(this, ptr2));
        }

        private void Edit()
        {
            this.Edit(null);
        }

        private void Edit(string displayText)
        {
            this.EnsureBound();
            bool cellIsVisible = true;
            this.EndEdit();
            DataGridRow[] dataGridRows = this.DataGridRows;
            if (this.DataGridRowsLength != 0)
            {
                dataGridRows[this.currentRow].OnEdit();
                this.editRow = dataGridRows[this.currentRow];
                if (this.myGridTable.GridColumnStyles.Count != 0)
                {
                    this.editColumn = this.myGridTable.GridColumnStyles[this.currentCol];
                    if (this.editColumn.PropertyDescriptor != null)
                    {
                        Rectangle empty = Rectangle.Empty;
                        if (((this.currentRow < this.firstVisibleRow) || (this.currentRow > (this.firstVisibleRow + this.numVisibleRows))) || (((this.currentCol < this.firstVisibleCol) || (this.currentCol > ((this.firstVisibleCol + this.numVisibleCols) - 1))) || ((this.currentCol == this.firstVisibleCol) && (this.negOffset != 0))))
                        {
                            cellIsVisible = false;
                        }
                        else
                        {
                            empty = this.GetCellBounds(this.currentRow, this.currentCol);
                        }
                        this.gridState[0x4000] = true;
                        this.gridState[0x8000] = false;
                        this.gridState[0x10000] = true;
                        this.editColumn.Edit(this.ListManager, this.currentRow, empty, (this.myGridTable.ReadOnly || this.ReadOnly) || !this.policy.AllowEdit, displayText, cellIsVisible);
                        this.gridState[0x10000] = false;
                    }
                }
            }
        }

        private void EndEdit()
        {
            if ((this.gridState[0x8000] || this.gridState[0x4000]) && !this.CommitEdit())
            {
                this.AbortEdit();
            }
        }

        public bool EndEdit(DataGridColumnStyle gridColumn, int rowNumber, bool shouldAbort)
        {
            bool flag = false;
            if (!this.gridState[0x8000])
            {
                return flag;
            }
            DataGridColumnStyle editColumn = this.editColumn;
            int num1 = this.editRow.RowNumber;
            if (shouldAbort)
            {
                this.AbortEdit();
                return true;
            }
            return this.CommitEdit();
        }

        public void EndInit()
        {
            this.inInit = false;
            if ((this.myGridTable == null) && (this.ListManager != null))
            {
                this.SetDataGridTable(this.TableStyles[this.ListManager.GetListName()], true);
            }
            if (this.myGridTable != null)
            {
                this.myGridTable.DataGrid = this;
            }
        }

        private void EnforceValidDataMember(object value)
        {
            if (((this.DataMember != null) && (this.DataMember.Length != 0)) && (this.BindingContext != null))
            {
                try
                {
                    BindingManagerBase base1 = this.BindingContext[value, this.dataMember];
                }
                catch
                {
                    this.dataMember = "";
                }
            }
        }

        private void EnsureBound()
        {
            if (!this.Bound)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridUnbound"));
            }
        }

        private void EnsureVisible(int row, int col)
        {
            if ((row < this.firstVisibleRow) || (row >= (this.firstVisibleRow + this.numTotallyVisibleRows)))
            {
                int rows = this.ComputeDeltaRows(row);
                this.ScrollDown(rows);
            }
            if (((this.firstVisibleCol != 0) || (this.numVisibleCols != 0)) || (this.lastTotallyVisibleCol != -1))
            {
                int firstVisibleCol = this.firstVisibleCol;
                int negOffset = this.negOffset;
                for (int i = this.lastTotallyVisibleCol; (((col < this.firstVisibleCol) || ((col == this.firstVisibleCol) && (this.negOffset != 0))) || ((this.lastTotallyVisibleCol == -1) && (col > this.firstVisibleCol))) || ((this.lastTotallyVisibleCol > -1) && (col > this.lastTotallyVisibleCol)); i = this.lastTotallyVisibleCol)
                {
                    this.ScrollToColumn(col);
                    if (((firstVisibleCol == this.firstVisibleCol) && (negOffset == this.negOffset)) && (i == this.lastTotallyVisibleCol))
                    {
                        return;
                    }
                    firstVisibleCol = this.firstVisibleCol;
                    negOffset = this.negOffset;
                }
            }
        }

        public void Expand(int row)
        {
            this.SetRowExpansionState(row, true);
        }

        public Rectangle GetCellBounds(DataGridCell dgc)
        {
            return this.GetCellBounds(dgc.RowNumber, dgc.ColumnNumber);
        }

        public Rectangle GetCellBounds(int row, int col)
        {
            Rectangle cellBounds = this.DataGridRows[row].GetCellBounds(col);
            cellBounds.Y += this.GetRowTop(row);
            cellBounds.X += this.layout.Data.X - this.negOffset;
            cellBounds.X = this.MirrorRectangle(cellBounds, this.layout.Data, this.isRightToLeft());
            return cellBounds;
        }

        internal int GetColBeg(int col)
        {
            int x = this.layout.Data.X - this.negOffset;
            GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
            int num2 = Math.Min(col, gridColumnStyles.Count);
            for (int i = this.firstVisibleCol; i < num2; i++)
            {
                if (gridColumnStyles[i].PropertyDescriptor != null)
                {
                    x += gridColumnStyles[i].Width;
                }
            }
            return this.MirrorPoint(x, this.layout.Data, this.isRightToLeft());
        }

        internal int GetColEnd(int col)
        {
            int colBeg = this.GetColBeg(col);
            int width = this.myGridTable.GridColumnStyles[col].Width;
            if (!this.isRightToLeft())
            {
                return (colBeg + width);
            }
            return (colBeg - width);
        }

        private int GetColFromX(int x)
        {
            if (this.myGridTable != null)
            {
                Rectangle data = this.layout.Data;
                x = this.MirrorPoint(x, data, this.isRightToLeft());
                GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
                int count = gridColumnStyles.Count;
                int num2 = data.X - this.negOffset;
                for (int i = this.firstVisibleCol; (num2 < (data.Width + data.X)) && (i < count); i++)
                {
                    if (gridColumnStyles[i].PropertyDescriptor != null)
                    {
                        num2 += gridColumnStyles[i].Width;
                    }
                    if (num2 > x)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        internal Rectangle GetColumnHeadersRect()
        {
            return this.layout.ColumnHeaders;
        }

        private int GetColumnWidthSum()
        {
            int num = 0;
            if ((this.myGridTable != null) && (this.myGridTable.GridColumnStyles != null))
            {
                GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
                int count = gridColumnStyles.Count;
                for (int i = 0; i < count; i++)
                {
                    if (gridColumnStyles[i].PropertyDescriptor != null)
                    {
                        num += gridColumnStyles[i].Width;
                    }
                }
            }
            return num;
        }

        public Rectangle GetCurrentCellBounds()
        {
            DataGridCell currentCell = this.CurrentCell;
            return this.GetCellBounds(currentCell.RowNumber, currentCell.ColumnNumber);
        }

        private DataGridRelationshipRow[] GetExpandableRows()
        {
            int dataGridRowsLength = this.DataGridRowsLength;
            DataGridRow[] dataGridRows = this.DataGridRows;
            if (this.policy.AllowAdd)
            {
                dataGridRowsLength = Math.Max(dataGridRowsLength - 1, 0);
            }
            DataGridRelationshipRow[] rowArray2 = new DataGridRelationshipRow[dataGridRowsLength];
            for (int i = 0; i < dataGridRowsLength; i++)
            {
                rowArray2[i] = (DataGridRelationshipRow) dataGridRows[i];
            }
            return rowArray2;
        }

        protected virtual string GetOutputTextDelimiter()
        {
            return "\t";
        }

        private int GetRowBottom(int row)
        {
            DataGridRow[] dataGridRows = this.DataGridRows;
            return (this.GetRowTop(row) + dataGridRows[row].Height);
        }

        internal Rectangle GetRowBounds(DataGridRow row)
        {
            return new Rectangle { Y = this.GetRowTop(row.RowNumber), X = this.layout.Data.X, Height = row.Height, Width = this.layout.Data.Width };
        }

        private int GetRowFromY(int y)
        {
            Rectangle data = this.layout.Data;
            int num = data.Y;
            int firstVisibleRow = this.firstVisibleRow;
            int dataGridRowsLength = this.DataGridRowsLength;
            DataGridRow[] dataGridRows = this.DataGridRows;
            int bottom = data.Bottom;
            while ((num < bottom) && (firstVisibleRow < dataGridRowsLength))
            {
                num += dataGridRows[firstVisibleRow].Height;
                if (num > y)
                {
                    return firstVisibleRow;
                }
                firstVisibleRow++;
            }
            return -1;
        }

        internal Rectangle GetRowHeaderRect()
        {
            return this.layout.RowHeaders;
        }

        private Rectangle GetRowRect(int rowNumber)
        {
            Rectangle data = this.layout.Data;
            int y = data.Y;
            DataGridRow[] dataGridRows = this.DataGridRows;
            for (int i = this.firstVisibleRow; i <= rowNumber; i++)
            {
                if (y > data.Bottom)
                {
                    break;
                }
                if (i == rowNumber)
                {
                    Rectangle rectangle2 = new Rectangle(data.X, y, data.Width, dataGridRows[i].Height);
                    if (this.layout.RowHeadersVisible)
                    {
                        rectangle2.Width += this.layout.RowHeaders.Width;
                        rectangle2.X -= this.isRightToLeft() ? 0 : this.layout.RowHeaders.Width;
                    }
                    return rectangle2;
                }
                y += dataGridRows[i].Height;
            }
            return Rectangle.Empty;
        }

        private int GetRowTop(int row)
        {
            DataGridRow[] dataGridRows = this.DataGridRows;
            int y = this.layout.Data.Y;
            int num2 = Math.Min(row, this.DataGridRowsLength);
            for (int i = this.firstVisibleRow; i < num2; i++)
            {
                y += dataGridRows[i].Height;
            }
            for (int j = this.firstVisibleRow; j > num2; j--)
            {
                y -= dataGridRows[j].Height;
            }
            return y;
        }

        protected virtual void GridHScrolled(object sender, ScrollEventArgs se)
        {
            if (base.Enabled && (this.DataSource != null))
            {
                this.gridState[0x20000] = true;
                if ((se.Type == ScrollEventType.SmallIncrement) || (se.Type == ScrollEventType.SmallDecrement))
                {
                    int columns = (se.Type == ScrollEventType.SmallIncrement) ? 1 : -1;
                    if ((se.Type == ScrollEventType.SmallDecrement) && (this.negOffset == 0))
                    {
                        GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
                        for (int i = this.firstVisibleCol - 1; (i >= 0) && (gridColumnStyles[i].Width == 0); i--)
                        {
                            columns--;
                        }
                    }
                    if ((se.Type == ScrollEventType.SmallIncrement) && (this.negOffset == 0))
                    {
                        GridColumnStylesCollection styless2 = this.myGridTable.GridColumnStyles;
                        for (int j = this.firstVisibleCol; ((j > -1) && (j < styless2.Count)) && (styless2[j].Width == 0); j++)
                        {
                            columns++;
                        }
                    }
                    this.ScrollRight(columns);
                    se.NewValue = this.HorizontalOffset;
                }
                else if (se.Type != ScrollEventType.EndScroll)
                {
                    this.HorizontalOffset = se.NewValue;
                }
                this.gridState[0x20000] = false;
            }
        }

        private void GridLineColorChanged(object sender, EventArgs e)
        {
            base.Invalidate(this.layout.Data);
        }

        private void GridLineStyleChanged(object sender, EventArgs e)
        {
            this.myGridTable.ResetRelationsUI();
            base.Invalidate(this.layout.Data);
        }

        protected virtual void GridVScrolled(object sender, ScrollEventArgs se)
        {
            if (base.Enabled && (this.DataSource != null))
            {
                this.gridState[0x20000] = true;
                try
                {
                    se.NewValue = Math.Min(se.NewValue, this.DataGridRowsLength - this.numTotallyVisibleRows);
                    int rows = se.NewValue - this.firstVisibleRow;
                    this.ScrollDown(rows);
                }
                finally
                {
                    this.gridState[0x20000] = false;
                }
            }
        }

        private void HandleEndCurrentEdit()
        {
            int currentRow = this.currentRow;
            int currentCol = this.currentCol;
            string message = null;
            try
            {
                this.listManager.EndCurrentEdit();
            }
            catch (Exception exception)
            {
                message = exception.Message;
            }
            if (message != null)
            {
                if (RTLAwareMessageBox.Show(null, System.Windows.Forms.SR.GetString("DataGridPushedIncorrectValueIntoColumn", new object[] { message }), System.Windows.Forms.SR.GetString("DataGridErrorMessageBoxCaption"), MessageBoxButtons.YesNo, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes)
                {
                    this.currentRow = currentRow;
                    this.currentCol = currentCol;
                    this.InvalidateRowHeader(this.currentRow);
                    this.Edit();
                }
                else
                {
                    this.listManager.PositionChanged -= this.positionChangedHandler;
                    this.listManager.CancelCurrentEdit();
                    this.listManager.Position = this.currentRow;
                    this.listManager.PositionChanged += this.positionChangedHandler;
                }
            }
        }

        private void HeaderBackColorChanged(object sender, EventArgs e)
        {
            if (this.layout.RowHeadersVisible)
            {
                base.Invalidate(this.layout.RowHeaders);
            }
            if (this.layout.ColumnHeadersVisible)
            {
                base.Invalidate(this.layout.ColumnHeaders);
            }
            base.Invalidate(this.layout.TopLeftHeader);
        }

        private void HeaderFontChanged(object sender, EventArgs e)
        {
            this.RecalculateFonts();
            base.PerformLayout();
            base.Invalidate(this.layout.Inside);
        }

        private void HeaderForeColorChanged(object sender, EventArgs e)
        {
            if (this.layout.RowHeadersVisible)
            {
                base.Invalidate(this.layout.RowHeaders);
            }
            if (this.layout.ColumnHeadersVisible)
            {
                base.Invalidate(this.layout.ColumnHeaders);
            }
            base.Invalidate(this.layout.TopLeftHeader);
        }

        public HitTestInfo HitTest(Point position)
        {
            return this.HitTest(position.X, position.Y);
        }

        public HitTestInfo HitTest(int x, int y)
        {
            int num1 = this.layout.Data.Y;
            HitTestInfo info = new HitTestInfo();
            if (this.layout.CaptionVisible && this.layout.Caption.Contains(x, y))
            {
                info.type = HitTestType.Caption;
                return info;
            }
            if (this.layout.ParentRowsVisible && this.layout.ParentRows.Contains(x, y))
            {
                info.type = HitTestType.ParentRows;
                return info;
            }
            if (!this.layout.Inside.Contains(x, y))
            {
                return info;
            }
            if (this.layout.TopLeftHeader.Contains(x, y))
            {
                return info;
            }
            if (this.layout.ColumnHeaders.Contains(x, y))
            {
                info.type = HitTestType.ColumnHeader;
                info.col = this.GetColFromX(x);
                if (info.col < 0)
                {
                    return HitTestInfo.Nowhere;
                }
                int colBeg = this.GetColBeg(info.col + 1);
                bool flag = this.isRightToLeft();
                if ((flag && ((x - colBeg) < 8)) || (!flag && ((colBeg - x) < 8)))
                {
                    info.type = HitTestType.ColumnResize;
                }
                if (!this.allowColumnResize)
                {
                    return HitTestInfo.Nowhere;
                }
                return info;
            }
            if (this.layout.RowHeaders.Contains(x, y))
            {
                info.type = HitTestType.RowHeader;
                info.row = this.GetRowFromY(y);
                if (info.row < 0)
                {
                    return HitTestInfo.Nowhere;
                }
                DataGridRow[] dataGridRows = this.DataGridRows;
                int num2 = this.GetRowTop(info.row) + dataGridRows[info.row].Height;
                if ((((num2 - y) - this.BorderWidth) < 2) && !(dataGridRows[info.row] is DataGridAddNewRow))
                {
                    info.type = HitTestType.RowResize;
                }
                if (!this.allowRowResize)
                {
                    return HitTestInfo.Nowhere;
                }
                return info;
            }
            if (!this.layout.Data.Contains(x, y))
            {
                return info;
            }
            info.type = HitTestType.Cell;
            info.col = this.GetColFromX(x);
            info.row = this.GetRowFromY(y);
            if ((info.col >= 0) && (info.row >= 0))
            {
                return info;
            }
            return HitTestInfo.Nowhere;
        }

        private void InitializeColumnWidths()
        {
            if (this.myGridTable != null)
            {
                GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
                int count = gridColumnStyles.Count;
                int num2 = this.myGridTable.IsDefault ? this.PreferredColumnWidth : this.myGridTable.PreferredColumnWidth;
                for (int i = 0; i < count; i++)
                {
                    if (gridColumnStyles[i].width == -1)
                    {
                        gridColumnStyles[i].width = num2;
                    }
                }
            }
        }

        internal void InvalidateCaption()
        {
            if (this.layout.CaptionVisible)
            {
                base.Invalidate(this.layout.Caption);
            }
        }

        internal void InvalidateCaptionRect(Rectangle r)
        {
            if (this.layout.CaptionVisible)
            {
                base.Invalidate(r);
            }
        }

        internal void InvalidateColumn(int column)
        {
            GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
            if ((((column >= 0) && (gridColumnStyles != null)) && (gridColumnStyles.Count > column)) && ((column >= this.firstVisibleCol) && (column <= ((this.firstVisibleCol + this.numVisibleCols) - 1))))
            {
                Rectangle rectangle = new Rectangle {
                    Height = this.layout.Data.Height,
                    Width = gridColumnStyles[column].Width,
                    Y = this.layout.Data.Y
                };
                int num = this.layout.Data.X - this.negOffset;
                int count = gridColumnStyles.Count;
                for (int i = this.firstVisibleCol; i < count; i++)
                {
                    if (i == column)
                    {
                        break;
                    }
                    num += gridColumnStyles[i].Width;
                }
                rectangle.X = num;
                rectangle.X = this.MirrorRectangle(rectangle, this.layout.Data, this.isRightToLeft());
                base.Invalidate(rectangle);
            }
        }

        internal void InvalidateInside()
        {
            base.Invalidate(this.layout.Inside);
        }

        internal void InvalidateParentRows()
        {
            if (this.layout.ParentRowsVisible)
            {
                base.Invalidate(this.layout.ParentRows);
            }
        }

        internal void InvalidateParentRowsRect(Rectangle r)
        {
            Rectangle parentRows = this.layout.ParentRows;
            base.Invalidate(r);
            bool isEmpty = parentRows.IsEmpty;
        }

        internal void InvalidateRow(int rowNumber)
        {
            Rectangle rowRect = this.GetRowRect(rowNumber);
            if (!rowRect.IsEmpty)
            {
                base.Invalidate(rowRect);
            }
        }

        private void InvalidateRowHeader(int rowNumber)
        {
            if (((rowNumber >= this.firstVisibleRow) && (rowNumber < (this.firstVisibleRow + this.numVisibleRows))) && this.layout.RowHeadersVisible)
            {
                Rectangle rc = new Rectangle {
                    Y = this.GetRowTop(rowNumber),
                    X = this.layout.RowHeaders.X,
                    Width = this.layout.RowHeaders.Width,
                    Height = this.DataGridRows[rowNumber].Height
                };
                base.Invalidate(rc);
            }
        }

        internal void InvalidateRowRect(int rowNumber, Rectangle r)
        {
            Rectangle rowRect = this.GetRowRect(rowNumber);
            if (!rowRect.IsEmpty)
            {
                Rectangle rc = new Rectangle(rowRect.X + r.X, rowRect.Y + r.Y, r.Width, r.Height);
                if (this.vertScrollBar.Visible && this.isRightToLeft())
                {
                    rc.X -= this.vertScrollBar.Width;
                }
                base.Invalidate(rc);
            }
        }

        public bool IsExpanded(int rowNumber)
        {
            if ((rowNumber < 0) || (rowNumber > this.DataGridRowsLength))
            {
                throw new ArgumentOutOfRangeException("rowNumber");
            }
            DataGridRow row = this.DataGridRows[rowNumber];
            if (row is DataGridRelationshipRow)
            {
                DataGridRelationshipRow row2 = (DataGridRelationshipRow) row;
                return row2.Expanded;
            }
            return false;
        }

        private bool isRightToLeft()
        {
            return (this.RightToLeft == RightToLeft.Yes);
        }

        public bool IsSelected(int row)
        {
            return this.DataGridRows[row].Selected;
        }

        internal static bool IsTransparentColor(Color color)
        {
            return (color.A < 0xff);
        }

        private void LayoutScrollBars()
        {
            if ((this.listManager == null) || (this.myGridTable == null))
            {
                this.horizScrollBar.Visible = false;
                this.vertScrollBar.Visible = false;
            }
            else
            {
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                bool flag4 = this.isRightToLeft();
                int count = this.myGridTable.GridColumnStyles.Count;
                DataGridRow[] dataGridRows = this.DataGridRows;
                int num2 = Math.Max(0, this.GetColumnWidthSum());
                if ((num2 > this.layout.Data.Width) && !flag)
                {
                    int height = this.horizScrollBar.Height;
                    this.layout.Data.Height -= height;
                    if (this.layout.RowHeadersVisible)
                    {
                        this.layout.RowHeaders.Height -= height;
                    }
                    flag = true;
                }
                int firstVisibleRow = this.firstVisibleRow;
                this.ComputeVisibleRows();
                if ((this.numTotallyVisibleRows != this.DataGridRowsLength) && !flag2)
                {
                    int width = this.vertScrollBar.Width;
                    this.layout.Data.Width -= width;
                    if (this.layout.ColumnHeadersVisible)
                    {
                        if (flag4)
                        {
                            this.layout.ColumnHeaders.X += width;
                        }
                        this.layout.ColumnHeaders.Width -= width;
                    }
                    flag2 = true;
                }
                this.firstVisibleCol = this.ComputeFirstVisibleColumn();
                this.ComputeVisibleColumns();
                if ((flag2 && (num2 > this.layout.Data.Width)) && !flag)
                {
                    this.firstVisibleRow = firstVisibleRow;
                    int num6 = this.horizScrollBar.Height;
                    this.layout.Data.Height -= num6;
                    if (this.layout.RowHeadersVisible)
                    {
                        this.layout.RowHeaders.Height -= num6;
                    }
                    flag = true;
                    flag3 = true;
                }
                if (flag3)
                {
                    this.ComputeVisibleRows();
                    if ((this.numTotallyVisibleRows != this.DataGridRowsLength) && !flag2)
                    {
                        int num7 = this.vertScrollBar.Width;
                        this.layout.Data.Width -= num7;
                        if (this.layout.ColumnHeadersVisible)
                        {
                            if (flag4)
                            {
                                this.layout.ColumnHeaders.X += num7;
                            }
                            this.layout.ColumnHeaders.Width -= num7;
                        }
                        flag2 = true;
                    }
                }
                this.layout.ResizeBoxRect = new Rectangle();
                if (flag2 && flag)
                {
                    Rectangle data = this.layout.Data;
                    this.layout.ResizeBoxRect = new Rectangle(flag4 ? data.X : data.Right, data.Bottom, this.vertScrollBar.Width, this.horizScrollBar.Height);
                }
                if (flag && (count > 0))
                {
                    int num8 = num2 - this.layout.Data.Width;
                    this.horizScrollBar.Minimum = 0;
                    this.horizScrollBar.Maximum = num2;
                    this.horizScrollBar.SmallChange = 1;
                    this.horizScrollBar.LargeChange = Math.Max(num2 - num8, 0);
                    this.horizScrollBar.Enabled = base.Enabled;
                    this.horizScrollBar.RightToLeft = this.RightToLeft;
                    this.horizScrollBar.Bounds = new Rectangle(flag4 ? (this.layout.Inside.X + this.layout.ResizeBoxRect.Width) : this.layout.Inside.X, this.layout.Data.Bottom, this.layout.Inside.Width - this.layout.ResizeBoxRect.Width, this.horizScrollBar.Height);
                    this.horizScrollBar.Visible = true;
                }
                else
                {
                    this.HorizontalOffset = 0;
                    this.horizScrollBar.Visible = false;
                }
                if (flag2)
                {
                    int y = this.layout.Data.Y;
                    if (this.layout.ColumnHeadersVisible)
                    {
                        y = this.layout.ColumnHeaders.Y;
                    }
                    this.vertScrollBar.LargeChange = (this.numTotallyVisibleRows != 0) ? this.numTotallyVisibleRows : 1;
                    this.vertScrollBar.Bounds = new Rectangle(flag4 ? this.layout.Data.X : this.layout.Data.Right, y, this.vertScrollBar.Width, this.layout.Data.Height + this.layout.ColumnHeaders.Height);
                    this.vertScrollBar.Enabled = base.Enabled;
                    this.vertScrollBar.Visible = true;
                    if (flag4)
                    {
                        this.layout.Data.X += this.vertScrollBar.Width;
                    }
                }
                else
                {
                    this.vertScrollBar.Visible = false;
                }
            }
        }

        private void LinkColorChanged(object sender, EventArgs e)
        {
            base.Invalidate(this.layout.Data);
        }

        private void LinkHoverColorChanged(object sender, EventArgs e)
        {
            base.Invalidate(this.layout.Data);
        }

        private void MetaDataChanged()
        {
            this.parentRows.Clear();
            this.caption.BackButtonActive = this.caption.DownButtonActive = this.caption.BackButtonVisible = false;
            this.caption.SetDownButtonDirection(!this.layout.ParentRowsVisible);
            this.gridState[0x400000] = true;
            try
            {
                if (this.originalState != null)
                {
                    this.Set_ListManager(this.originalState.DataSource, this.originalState.DataMember, true);
                    this.originalState = null;
                }
                else
                {
                    this.Set_ListManager(this.DataSource, this.DataMember, true);
                }
            }
            finally
            {
                this.gridState[0x400000] = false;
            }
        }

        internal int MinimumRowHeaderWidth()
        {
            return this.minRowHeaderWidth;
        }

        private int MirrorPoint(int x, Rectangle rect, bool rightToLeft)
        {
            if (rightToLeft)
            {
                return ((rect.Right + rect.X) - x);
            }
            return x;
        }

        private int MirrorRectangle(Rectangle R1, Rectangle rect, bool rightToLeft)
        {
            if (rightToLeft)
            {
                return ((rect.Right + rect.X) - R1.Right);
            }
            return R1.X;
        }

        private int MoveLeftRight(GridColumnStylesCollection cols, int startCol, bool goRight)
        {
            int num;
            if (goRight)
            {
                num = startCol + 1;
                while (num < cols.Count)
                {
                    if (cols[num].PropertyDescriptor != null)
                    {
                        return num;
                    }
                    num++;
                }
                return num;
            }
            num = startCol - 1;
            while (num >= 0)
            {
                if (cols[num].PropertyDescriptor != null)
                {
                    return num;
                }
                num--;
            }
            return num;
        }

        public void NavigateBack()
        {
            if (this.CommitEdit() && !this.parentRows.IsEmpty())
            {
                if (this.gridState[0x100000])
                {
                    this.gridState[0x100000] = false;
                    try
                    {
                        this.listManager.CancelCurrentEdit();
                    }
                    catch
                    {
                    }
                }
                else
                {
                    this.UpdateListManager();
                }
                DataGridState state = this.parentRows.PopTop();
                this.ResetMouseState();
                state.PullState(this, false);
                if (this.parentRows.GetTopParent() == null)
                {
                    this.originalState = null;
                }
                DataGridRow[] dataGridRows = this.DataGridRows;
                if ((this.ReadOnly || !this.policy.AllowAdd) == (dataGridRows[this.DataGridRowsLength - 1] is DataGridAddNewRow))
                {
                    int num = (this.ReadOnly || !this.policy.AllowAdd) ? (this.DataGridRowsLength - 1) : (this.DataGridRowsLength + 1);
                    DataGridRow[] newRows = new DataGridRow[num];
                    for (int i = 0; i < Math.Min(num, this.DataGridRowsLength); i++)
                    {
                        newRows[i] = this.DataGridRows[i];
                    }
                    if (!this.ReadOnly && this.policy.AllowAdd)
                    {
                        newRows[num - 1] = new DataGridAddNewRow(this, this.myGridTable, num - 1);
                    }
                    this.SetDataGridRows(newRows, num);
                }
                dataGridRows = this.DataGridRows;
                if (((dataGridRows != null) && (dataGridRows.Length != 0)) && (dataGridRows[0].DataGridTableStyle != this.myGridTable))
                {
                    for (int j = 0; j < dataGridRows.Length; j++)
                    {
                        dataGridRows[j].DataGridTableStyle = this.myGridTable;
                    }
                }
                if ((this.myGridTable.GridColumnStyles.Count > 0) && (this.myGridTable.GridColumnStyles[0].Width == -1))
                {
                    this.InitializeColumnWidths();
                }
                this.currentRow = (this.ListManager.Position == -1) ? 0 : this.ListManager.Position;
                if (!this.AllowNavigation)
                {
                    this.RecreateDataGridRows();
                }
                this.caption.BackButtonActive = (this.parentRows.GetTopParent() != null) && this.AllowNavigation;
                this.caption.BackButtonVisible = this.caption.BackButtonActive;
                this.caption.DownButtonActive = this.parentRows.GetTopParent() != null;
                base.PerformLayout();
                base.Invalidate();
                if (this.vertScrollBar.Visible)
                {
                    this.vertScrollBar.Value = this.firstVisibleRow;
                }
                if (this.horizScrollBar.Visible)
                {
                    this.horizScrollBar.Value = this.HorizontalOffset + this.negOffset;
                }
                this.Edit();
                this.OnNavigate(new NavigateEventArgs(false));
            }
        }

        private void NavigateTo(DataGridState childState)
        {
            this.EndEdit();
            this.gridState[0x4000] = false;
            this.ResetMouseState();
            childState.PullState(this, true);
            if (this.listManager.Position != this.currentRow)
            {
                this.currentRow = (this.listManager.Position == -1) ? 0 : this.listManager.Position;
            }
            if (this.parentRows.GetTopParent() != null)
            {
                this.caption.BackButtonActive = this.AllowNavigation;
                this.caption.BackButtonVisible = this.caption.BackButtonActive;
                this.caption.DownButtonActive = true;
            }
            this.HorizontalOffset = 0;
            base.PerformLayout();
            base.Invalidate();
        }

        public void NavigateTo(int rowNumber, string relationName)
        {
            if (this.AllowNavigation)
            {
                DataGridRow[] dataGridRows = this.DataGridRows;
                if ((rowNumber < 0) || (rowNumber > (this.DataGridRowsLength - (this.policy.AllowAdd ? 2 : 1))))
                {
                    throw new ArgumentOutOfRangeException("rowNumber");
                }
                this.EnsureBound();
                DataGridRow source = dataGridRows[rowNumber];
                this.NavigateTo(relationName, source, false);
            }
        }

        internal void NavigateTo(string relationName, DataGridRow source, bool fromRow)
        {
            if (this.AllowNavigation && this.CommitEdit())
            {
                DataGridState state;
                try
                {
                    state = this.CreateChildState(relationName, source);
                }
                catch
                {
                    this.NavigateBack();
                    return;
                }
                try
                {
                    this.listManager.EndCurrentEdit();
                }
                catch
                {
                    return;
                }
                DataGridState dgs = new DataGridState(this) {
                    LinkingRow = source
                };
                if (source.RowNumber != this.CurrentRow)
                {
                    this.listManager.Position = source.RowNumber;
                }
                if (this.parentRows.GetTopParent() == null)
                {
                    this.originalState = dgs;
                }
                this.parentRows.AddParent(dgs);
                this.NavigateTo(state);
                this.OnNavigate(new NavigateEventArgs(true));
            }
        }

        private Point NormalizeToRow(int x, int y, int row)
        {
            Point point = new Point(0, this.layout.Data.Y);
            DataGridRow[] dataGridRows = this.DataGridRows;
            for (int i = this.firstVisibleRow; i < row; i++)
            {
                point.Y += dataGridRows[i].Height;
            }
            return new Point(x, y - point.Y);
        }

        private void ObjectSiteChange(IContainer container, IComponent component, bool site)
        {
            if (site)
            {
                if (component.Site == null)
                {
                    container.Add(component);
                }
            }
            else if ((component.Site != null) && (component.Site.Container == container))
            {
                container.Remove(component);
            }
        }

        protected virtual void OnAllowNavigationChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_ALLOWNAVIGATIONCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnBackButtonClicked(object sender, EventArgs e)
        {
            this.NavigateBack();
            EventHandler handler = (EventHandler) base.Events[EVENT_BACKBUTTONCLICK];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            this.backBrush = new SolidBrush(this.BackColor);
            base.Invalidate();
            base.OnBackColorChanged(e);
        }

        protected virtual void OnBackgroundColorChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_BACKGROUNDCOLORCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnBindingContextChanged(EventArgs e)
        {
            if ((this.DataSource != null) && !this.gridState[0x200000])
            {
                try
                {
                    this.Set_ListManager(this.DataSource, this.DataMember, true, false);
                }
                catch
                {
                    if ((this.Site == null) || !this.Site.DesignMode)
                    {
                        throw;
                    }
                    RTLAwareMessageBox.Show(null, System.Windows.Forms.SR.GetString("DataGridExceptionInPaint"), null, MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, 0);
                    if (base.Visible)
                    {
                        base.BeginUpdateInternal();
                    }
                    this.ResetParentRows();
                    this.Set_ListManager(null, string.Empty, true);
                    if (base.Visible)
                    {
                        base.EndUpdateInternal();
                    }
                }
            }
            base.OnBindingContextChanged(e);
        }

        protected virtual void OnBorderStyleChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_BORDERSTYLECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnCaptionVisibleChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_CAPTIONVISIBLECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void OnColumnCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            DataGridTableStyle style = (DataGridTableStyle) sender;
            if (style.Equals(this.myGridTable))
            {
                if (!this.myGridTable.IsDefault && ((e.Action != CollectionChangeAction.Refresh) || (e.Element == null)))
                {
                    this.PairTableStylesAndGridColumns(this.listManager, this.myGridTable, false);
                }
                base.Invalidate();
                base.PerformLayout();
            }
        }

        protected virtual void OnCurrentCellChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_CURRENTCELLCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDataSourceChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_DATASOURCECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnEnter(EventArgs e)
        {
            if (this.gridState[0x800] && !this.gridState[0x10000])
            {
                if (this.Bound)
                {
                    this.Edit();
                }
                base.OnEnter(e);
            }
        }

        protected virtual void OnFlatModeChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_FLATMODECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            this.Caption.OnGridFontChanged();
            this.RecalculateFonts();
            this.RecreateDataGridRows();
            if (this.originalState != null)
            {
                Stack stack = new Stack();
                while (!this.parentRows.IsEmpty())
                {
                    DataGridState state = this.parentRows.PopTop();
                    int dataGridRowsLength = state.DataGridRowsLength;
                    for (int i = 0; i < dataGridRowsLength; i++)
                    {
                        state.DataGridRows[i].Height = state.DataGridRows[i].MinimumRowHeight(state.GridColumnStyles);
                    }
                    stack.Push(state);
                }
                while (stack.Count != 0)
                {
                    this.parentRows.AddParent((DataGridState) stack.Pop());
                }
            }
            base.OnFontChanged(e);
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            this.foreBrush = new SolidBrush(this.ForeColor);
            base.Invalidate();
            base.OnForeColorChanged(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            this.toolTipProvider = new DataGridToolTip(this);
            this.toolTipProvider.CreateToolTipHandle();
            this.toolTipId = 0;
            base.PerformLayout();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            this.toolTipProvider.Destroy();
            this.toolTipProvider = null;
            this.toolTipId = 0;
        }

        protected override void OnKeyDown(KeyEventArgs ke)
        {
            base.OnKeyDown(ke);
            this.ProcessGridKey(ke);
        }

        protected override void OnKeyPress(KeyPressEventArgs kpe)
        {
            base.OnKeyPress(kpe);
            GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
            if ((((gridColumnStyles != null) && (this.currentCol > 0)) && ((this.currentCol < gridColumnStyles.Count) && !gridColumnStyles[this.currentCol].ReadOnly)) && (kpe.KeyChar > ' '))
            {
                this.Edit(new string(new char[] { kpe.KeyChar }));
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (!this.gridState[0x10000])
            {
                base.OnLayout(levent);
                if (!this.gridState[0x1000000])
                {
                    this.gridState[0x800] = false;
                    try
                    {
                        if (base.IsHandleCreated)
                        {
                            if (this.layout.ParentRowsVisible)
                            {
                                this.parentRows.OnLayout();
                            }
                            if (this.ToolTipProvider != null)
                            {
                                this.ResetToolTip();
                            }
                            this.ComputeLayout();
                        }
                    }
                    finally
                    {
                        this.gridState[0x800] = true;
                    }
                }
            }
        }

        protected override void OnLeave(EventArgs e)
        {
            this.OnLeave_Grid();
            base.OnLeave(e);
        }

        private void OnLeave_Grid()
        {
            this.gridState[0x800] = false;
            try
            {
                this.EndEdit();
                if ((this.listManager != null) && !this.gridState[0x10000])
                {
                    if (this.gridState[0x100000])
                    {
                        this.listManager.CancelCurrentEdit();
                        DataGridRow[] dataGridRows = this.DataGridRows;
                        dataGridRows[this.DataGridRowsLength - 1] = new DataGridAddNewRow(this, this.myGridTable, this.DataGridRowsLength - 1);
                        this.SetDataGridRows(dataGridRows, this.DataGridRowsLength);
                    }
                    else
                    {
                        this.HandleEndCurrentEdit();
                    }
                }
            }
            finally
            {
                this.gridState[0x800] = true;
                if (!this.gridState[0x10000])
                {
                    this.gridState[0x100000] = false;
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            HitTestInfo info;
            base.OnMouseDown(e);
            this.gridState[0x80000] = false;
            this.gridState[0x100] = false;
            if (this.listManager != null)
            {
                info = this.HitTest(e.X, e.Y);
                Keys modifierKeys = Control.ModifierKeys;
                bool flag = ((modifierKeys & Keys.Control) == Keys.Control) && ((modifierKeys & Keys.Alt) == Keys.None);
                bool flag2 = (modifierKeys & Keys.Shift) == Keys.Shift;
                if (e.Button != MouseButtons.Left)
                {
                    return;
                }
                if (info.type == HitTestType.ColumnResize)
                {
                    if (e.Clicks > 1)
                    {
                        this.ColAutoResize(info.col);
                        return;
                    }
                    this.ColResizeBegin(e, info.col);
                    return;
                }
                if (info.type == HitTestType.RowResize)
                {
                    if (e.Clicks > 1)
                    {
                        this.RowAutoResize(info.row);
                        return;
                    }
                    this.RowResizeBegin(e, info.row);
                    return;
                }
                if (info.type == HitTestType.ColumnHeader)
                {
                    this.trackColumnHeader = this.myGridTable.GridColumnStyles[info.col].PropertyDescriptor;
                    return;
                }
                if (info.type == HitTestType.Caption)
                {
                    Rectangle caption = this.layout.Caption;
                    this.caption.MouseDown(e.X - caption.X, e.Y - caption.Y);
                    return;
                }
                if (this.layout.Data.Contains(e.X, e.Y) || this.layout.RowHeaders.Contains(e.X, e.Y))
                {
                    int rowFromY = this.GetRowFromY(e.Y);
                    if (rowFromY > -1)
                    {
                        Point point = this.NormalizeToRow(e.X, e.Y, rowFromY);
                        if (this.DataGridRows[rowFromY].OnMouseDown(point.X, point.Y, this.layout.RowHeaders, this.isRightToLeft()))
                        {
                            this.CommitEdit();
                            DataGridRow[] rowArray = this.DataGridRows;
                            if (((rowFromY < this.DataGridRowsLength) && (rowArray[rowFromY] is DataGridRelationshipRow)) && ((DataGridRelationshipRow) rowArray[rowFromY]).Expanded)
                            {
                                this.EnsureVisible(rowFromY, 0);
                            }
                            this.Edit();
                            return;
                        }
                    }
                }
                if (info.type != HitTestType.RowHeader)
                {
                    if (info.type == HitTestType.ParentRows)
                    {
                        this.EndEdit();
                        this.parentRows.OnMouseDown(e.X, e.Y, this.isRightToLeft());
                    }
                    if ((info.type == HitTestType.Cell) && !this.myGridTable.GridColumnStyles[info.col].MouseDown(info.row, e.X, e.Y))
                    {
                        DataGridCell cell = new DataGridCell(info.row, info.col);
                        if (this.policy.AllowEdit && this.CurrentCell.Equals(cell))
                        {
                            this.ResetSelection();
                            this.EnsureVisible(this.currentRow, this.currentCol);
                            this.Edit();
                        }
                        else
                        {
                            this.ResetSelection();
                            this.CurrentCell = cell;
                        }
                    }
                    return;
                }
                this.EndEdit();
                if (!(this.DataGridRows[info.row] is DataGridAddNewRow))
                {
                    int currentRow = this.currentRow;
                    this.CurrentCell = new DataGridCell(info.row, this.currentCol);
                    if (((info.row != currentRow) && (this.currentRow != info.row)) && (this.currentRow == currentRow))
                    {
                        return;
                    }
                }
                if (flag)
                {
                    if (this.IsSelected(info.row))
                    {
                        this.UnSelect(info.row);
                    }
                    else
                    {
                        this.Select(info.row);
                    }
                    goto Label_035E;
                }
                if ((this.lastRowSelected == -1) || !flag2)
                {
                    this.ResetSelection();
                    this.Select(info.row);
                    goto Label_035E;
                }
                int num3 = Math.Min(this.lastRowSelected, info.row);
                int num4 = Math.Max(this.lastRowSelected, info.row);
                int lastRowSelected = this.lastRowSelected;
                this.ResetSelection();
                this.lastRowSelected = lastRowSelected;
                DataGridRow[] dataGridRows = this.DataGridRows;
                for (int i = num3; i <= num4; i++)
                {
                    dataGridRows[i].Selected = true;
                    this.numSelectedRows++;
                }
                this.EndEdit();
            }
            return;
        Label_035E:
            this.lastRowSelected = info.row;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (this.oldRow != -1)
            {
                this.DataGridRows[this.oldRow].OnMouseLeft(this.layout.RowHeaders, this.isRightToLeft());
            }
            if (this.gridState[0x40000])
            {
                this.caption.MouseLeft();
            }
            this.Cursor = null;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (this.listManager != null)
            {
                HitTestInfo info = this.HitTest(e.X, e.Y);
                bool alignToRight = this.isRightToLeft();
                if (this.gridState[8])
                {
                    this.ColResizeMove(e);
                }
                if (this.gridState[0x10])
                {
                    this.RowResizeMove(e);
                }
                if (this.gridState[8] || (info.type == HitTestType.ColumnResize))
                {
                    this.Cursor = Cursors.SizeWE;
                }
                else if (this.gridState[0x10] || (info.type == HitTestType.RowResize))
                {
                    this.Cursor = Cursors.SizeNS;
                }
                else
                {
                    this.Cursor = null;
                    if (this.layout.Data.Contains(e.X, e.Y) || (this.layout.RowHeadersVisible && this.layout.RowHeaders.Contains(e.X, e.Y)))
                    {
                        DataGridRow[] dataGridRows = this.DataGridRows;
                        int rowFromY = this.GetRowFromY(e.Y);
                        if ((this.lastRowSelected != -1) && !this.gridState[0x100])
                        {
                            int rowTop = this.GetRowTop(this.lastRowSelected);
                            int num3 = rowTop + dataGridRows[this.lastRowSelected].Height;
                            int height = SystemInformation.DragSize.Height;
                            this.gridState[0x100] = (((e.Y - rowTop) < height) && ((rowTop - e.Y) < height)) || (((e.Y - num3) < height) && ((num3 - e.Y) < height));
                        }
                        if (rowFromY > -1)
                        {
                            Point point = this.NormalizeToRow(e.X, e.Y, rowFromY);
                            if (!dataGridRows[rowFromY].OnMouseMove(point.X, point.Y, this.layout.RowHeaders, alignToRight) && this.gridState[0x100])
                            {
                                MouseButtons mouseButtons = Control.MouseButtons;
                                if (((this.lastRowSelected != -1) && ((mouseButtons & MouseButtons.Left) == MouseButtons.Left)) && (((Control.ModifierKeys & Keys.Control) != Keys.Control) || ((Control.ModifierKeys & Keys.Alt) != Keys.None)))
                                {
                                    int lastRowSelected = this.lastRowSelected;
                                    this.ResetSelection();
                                    this.lastRowSelected = lastRowSelected;
                                    int num6 = Math.Min(this.lastRowSelected, rowFromY);
                                    int num7 = Math.Max(this.lastRowSelected, rowFromY);
                                    DataGridRow[] rowArray2 = this.DataGridRows;
                                    for (int i = num6; i <= num7; i++)
                                    {
                                        rowArray2[i].Selected = true;
                                        this.numSelectedRows++;
                                    }
                                }
                            }
                        }
                        if ((this.oldRow != rowFromY) && (this.oldRow != -1))
                        {
                            dataGridRows[this.oldRow].OnMouseLeft(this.layout.RowHeaders, alignToRight);
                        }
                        this.oldRow = rowFromY;
                    }
                    if ((info.type == HitTestType.ParentRows) && (this.parentRows != null))
                    {
                        this.parentRows.OnMouseMove(e.X, e.Y);
                    }
                    if (info.type == HitTestType.Caption)
                    {
                        this.gridState[0x40000] = true;
                        Rectangle caption = this.layout.Caption;
                        this.caption.MouseOver(e.X - caption.X, e.Y - caption.Y);
                    }
                    else if (this.gridState[0x40000])
                    {
                        this.gridState[0x40000] = false;
                        this.caption.MouseLeft();
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            this.gridState[0x100] = false;
            if ((this.listManager != null) && (this.myGridTable != null))
            {
                if (this.gridState[8])
                {
                    this.ColResizeEnd(e);
                }
                if (this.gridState[0x10])
                {
                    this.RowResizeEnd(e);
                }
                this.gridState[8] = false;
                this.gridState[0x10] = false;
                HitTestInfo info = this.HitTest(e.X, e.Y);
                if ((info.type & HitTestType.Caption) == HitTestType.Caption)
                {
                    this.caption.MouseUp(e.X, e.Y);
                }
                if ((info.type == HitTestType.ColumnHeader) && (this.myGridTable.GridColumnStyles[info.col].PropertyDescriptor == this.trackColumnHeader))
                {
                    this.ColumnHeaderClicked(this.trackColumnHeader);
                }
                this.trackColumnHeader = null;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e is HandledMouseEventArgs)
            {
                if (((HandledMouseEventArgs) e).Handled)
                {
                    return;
                }
                ((HandledMouseEventArgs) e).Handled = true;
            }
            bool flag = true;
            if ((Control.ModifierKeys & Keys.Control) != Keys.None)
            {
                flag = false;
            }
            if ((this.listManager != null) && (this.myGridTable != null))
            {
                ScrollBar bar = flag ? this.vertScrollBar : this.horizScrollBar;
                if (bar.Visible)
                {
                    this.gridState[0x20000] = true;
                    this.wheelDelta += e.Delta;
                    float num = ((float) this.wheelDelta) / 120f;
                    int num2 = (int) (SystemInformation.MouseWheelScrollLines * num);
                    if (num2 != 0)
                    {
                        this.wheelDelta = 0;
                        if (flag)
                        {
                            int num3 = this.firstVisibleRow - num2;
                            num3 = Math.Max(0, Math.Min(num3, this.DataGridRowsLength - this.numTotallyVisibleRows));
                            this.ScrollDown(num3 - this.firstVisibleRow);
                        }
                        else
                        {
                            int num4 = this.horizScrollBar.Value + (((num2 < 0) ? 1 : -1) * this.horizScrollBar.LargeChange);
                            this.HorizontalOffset = num4;
                        }
                    }
                    this.gridState[0x20000] = false;
                }
            }
        }

        protected void OnNavigate(NavigateEventArgs e)
        {
            if (this.onNavigate != null)
            {
                this.onNavigate(this, e);
            }
        }

        internal void OnNodeClick(EventArgs e)
        {
            base.PerformLayout();
            GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
            if (((this.firstVisibleCol > -1) && (this.firstVisibleCol < gridColumnStyles.Count)) && (gridColumnStyles[this.firstVisibleCol] == this.editColumn))
            {
                this.Edit();
            }
            EventHandler handler = (EventHandler) base.Events[EVENT_NODECLICKED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            try
            {
                this.CheckHierarchyState();
                if (this.layout.dirty)
                {
                    this.ComputeLayout();
                }
                Graphics g = pe.Graphics;
                Region clip = g.Clip;
                if (this.layout.CaptionVisible)
                {
                    this.caption.Paint(g, this.layout.Caption, this.isRightToLeft());
                }
                if (this.layout.ParentRowsVisible)
                {
                    g.FillRectangle(SystemBrushes.AppWorkspace, this.layout.ParentRows);
                    this.parentRows.Paint(g, this.layout.ParentRows, this.isRightToLeft());
                }
                Rectangle data = this.layout.Data;
                if (this.layout.RowHeadersVisible)
                {
                    data = Rectangle.Union(data, this.layout.RowHeaders);
                }
                if (this.layout.ColumnHeadersVisible)
                {
                    data = Rectangle.Union(data, this.layout.ColumnHeaders);
                }
                g.SetClip(data);
                this.PaintGrid(g, data);
                g.Clip = clip;
                clip.Dispose();
                this.PaintBorder(g, this.layout.ClientRectangle);
                g.FillRectangle(DefaultHeaderBackBrush, this.layout.ResizeBoxRect);
                base.OnPaint(pe);
            }
            catch
            {
                if ((this.Site == null) || !this.Site.DesignMode)
                {
                    throw;
                }
                this.gridState[0x800000] = true;
                try
                {
                    RTLAwareMessageBox.Show(null, System.Windows.Forms.SR.GetString("DataGridExceptionInPaint"), null, MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, 0);
                    if (base.Visible)
                    {
                        base.BeginUpdateInternal();
                    }
                    this.ResetParentRows();
                    this.Set_ListManager(null, string.Empty, true);
                }
                finally
                {
                    this.gridState[0x800000] = false;
                    if (base.Visible)
                    {
                        base.EndUpdateInternal();
                    }
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs ebe)
        {
        }

        protected virtual void OnParentRowsLabelStyleChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_PARENTROWSLABELSTYLECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnParentRowsVisibleChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_PARENTROWSVISIBLECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnReadOnlyChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_READONLYCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            if (this.layout.CaptionVisible)
            {
                base.Invalidate(this.layout.Caption);
            }
            if (this.layout.ParentRowsVisible)
            {
                this.parentRows.OnResize(this.layout.ParentRows);
            }
            int borderWidth = this.BorderWidth;
            Rectangle clientRectangle = this.layout.ClientRectangle;
            Rectangle rc = new Rectangle((clientRectangle.X + clientRectangle.Width) - borderWidth, clientRectangle.Y, borderWidth, clientRectangle.Height);
            Rectangle rectangle2 = new Rectangle(clientRectangle.X, (clientRectangle.Y + clientRectangle.Height) - borderWidth, clientRectangle.Width, borderWidth);
            Rectangle rectangle4 = base.ClientRectangle;
            if (rectangle4.Width != clientRectangle.Width)
            {
                base.Invalidate(rc);
                rc = new Rectangle((rectangle4.X + rectangle4.Width) - borderWidth, rectangle4.Y, borderWidth, rectangle4.Height);
                base.Invalidate(rc);
            }
            if (rectangle4.Height != clientRectangle.Height)
            {
                base.Invalidate(rectangle2);
                rectangle2 = new Rectangle(rectangle4.X, (rectangle4.Y + rectangle4.Height) - borderWidth, rectangle4.Width, borderWidth);
                base.Invalidate(rectangle2);
            }
            if (!this.layout.ResizeBoxRect.IsEmpty)
            {
                base.Invalidate(this.layout.ResizeBoxRect);
            }
            this.layout.ClientRectangle = rectangle4;
            int firstVisibleRow = this.firstVisibleRow;
            base.OnResize(e);
            if (this.isRightToLeft() || (firstVisibleRow != this.firstVisibleRow))
            {
                base.Invalidate();
            }
        }

        protected void OnRowHeaderClick(EventArgs e)
        {
            if (this.onRowHeaderClick != null)
            {
                this.onRowHeaderClick(this, e);
            }
        }

        internal void OnRowHeightChanged(DataGridRow row)
        {
            this.ClearRegionCache();
            int rowTop = this.GetRowTop(row.RowNumber);
            if (rowTop > 0)
            {
                Rectangle rc = new Rectangle {
                    Y = rowTop,
                    X = this.layout.Inside.X,
                    Width = this.layout.Inside.Width,
                    Height = this.layout.Inside.Bottom - rowTop
                };
                base.Invalidate(rc);
            }
        }

        protected void OnScroll(EventArgs e)
        {
            if (this.ToolTipProvider != null)
            {
                this.ResetToolTip();
            }
            EventHandler handler = (EventHandler) base.Events[EVENT_SCROLL];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnShowParentDetailsButtonClicked(object sender, EventArgs e)
        {
            this.ParentRowsVisible = !this.caption.ToggleDownButtonDirection();
            EventHandler handler = (EventHandler) base.Events[EVENT_DOWNBUTTONCLICK];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void PaintBorder(Graphics g, Rectangle bounds)
        {
            if (this.BorderStyle != System.Windows.Forms.BorderStyle.None)
            {
                if (this.BorderStyle == System.Windows.Forms.BorderStyle.Fixed3D)
                {
                    Border3DStyle sunken = Border3DStyle.Sunken;
                    ControlPaint.DrawBorder3D(g, bounds, sunken);
                }
                else if (this.BorderStyle == System.Windows.Forms.BorderStyle.FixedSingle)
                {
                    Brush headerForeBrush;
                    if (this.myGridTable.IsDefault)
                    {
                        headerForeBrush = this.HeaderForeBrush;
                    }
                    else
                    {
                        headerForeBrush = this.myGridTable.HeaderForeBrush;
                    }
                    g.FillRectangle(headerForeBrush, bounds.X, bounds.Y, bounds.Width + 2, 2);
                    g.FillRectangle(headerForeBrush, bounds.Right - 2, bounds.Y, 2, bounds.Height + 2);
                    g.FillRectangle(headerForeBrush, bounds.X, bounds.Bottom - 2, bounds.Width + 2, 2);
                    g.FillRectangle(headerForeBrush, bounds.X, bounds.Y, 2, bounds.Height + 2);
                }
                else
                {
                    Pen windowFrame = SystemPens.WindowFrame;
                    bounds.Width--;
                    bounds.Height--;
                    g.DrawRectangle(windowFrame, bounds);
                }
            }
        }

        private void PaintColumnHeaders(Graphics g)
        {
            bool flag = this.isRightToLeft();
            Rectangle columnHeaders = this.layout.ColumnHeaders;
            if (!flag)
            {
                columnHeaders.X -= this.negOffset;
            }
            columnHeaders.Width += this.negOffset;
            int num = this.PaintColumnHeaderText(g, columnHeaders);
            if (flag)
            {
                columnHeaders.X = columnHeaders.Right - num;
            }
            columnHeaders.Width = num;
            if (!this.FlatMode)
            {
                ControlPaint.DrawBorder3D(g, columnHeaders, Border3DStyle.RaisedInner);
                columnHeaders.Inflate(-1, -1);
                columnHeaders.Width--;
                columnHeaders.Height--;
                g.DrawRectangle(SystemPens.Control, columnHeaders);
            }
        }

        private int PaintColumnHeaderText(Graphics g, Rectangle boundingRect)
        {
            int num = 0;
            Rectangle rect = boundingRect;
            GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
            bool flag = this.isRightToLeft();
            int count = gridColumnStyles.Count;
            PropertyDescriptor sortProperty = null;
            sortProperty = this.ListManager.GetSortProperty();
            for (int i = this.firstVisibleCol; i < count; i++)
            {
                if (gridColumnStyles[i].PropertyDescriptor != null)
                {
                    Brush headerBackBrush;
                    if (num > boundingRect.Width)
                    {
                        break;
                    }
                    bool flag2 = (sortProperty != null) && sortProperty.Equals(gridColumnStyles[i].PropertyDescriptor);
                    TriangleDirection up = TriangleDirection.Up;
                    if (flag2 && (this.ListManager.GetSortDirection() == ListSortDirection.Descending))
                    {
                        up = TriangleDirection.Down;
                    }
                    if (flag)
                    {
                        rect.Width = gridColumnStyles[i].Width - (flag2 ? rect.Height : 0);
                        rect.X = (boundingRect.Right - num) - rect.Width;
                    }
                    else
                    {
                        rect.X = boundingRect.X + num;
                        rect.Width = gridColumnStyles[i].Width - (flag2 ? rect.Height : 0);
                    }
                    if (this.myGridTable.IsDefault)
                    {
                        headerBackBrush = this.HeaderBackBrush;
                    }
                    else
                    {
                        headerBackBrush = this.myGridTable.HeaderBackBrush;
                    }
                    g.FillRectangle(headerBackBrush, rect);
                    if (flag)
                    {
                        rect.X -= 2;
                        rect.Y += 2;
                    }
                    else
                    {
                        rect.X += 2;
                        rect.Y += 2;
                    }
                    StringFormat format = new StringFormat();
                    HorizontalAlignment alignment = gridColumnStyles[i].Alignment;
                    format.Alignment = (alignment == HorizontalAlignment.Right) ? StringAlignment.Far : ((alignment == HorizontalAlignment.Center) ? StringAlignment.Center : StringAlignment.Near);
                    format.FormatFlags |= StringFormatFlags.NoWrap;
                    if (flag)
                    {
                        format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                        format.Alignment = StringAlignment.Near;
                    }
                    g.DrawString(gridColumnStyles[i].HeaderText, this.myGridTable.IsDefault ? this.HeaderFont : this.myGridTable.HeaderFont, this.myGridTable.IsDefault ? this.HeaderForeBrush : this.myGridTable.HeaderForeBrush, rect, format);
                    format.Dispose();
                    if (flag)
                    {
                        rect.X += 2;
                        rect.Y -= 2;
                    }
                    else
                    {
                        rect.X -= 2;
                        rect.Y -= 2;
                    }
                    if (flag2)
                    {
                        Rectangle rectangle2 = new Rectangle(flag ? (rect.X - rect.Height) : rect.Right, rect.Y, rect.Height, rect.Height);
                        g.FillRectangle(headerBackBrush, rectangle2);
                        int num4 = Math.Max(0, (rect.Height - 5) / 2);
                        rectangle2.Inflate(-num4, -num4);
                        Pen pen = new Pen(this.BackgroundBrush);
                        Pen pen2 = new Pen(this.myGridTable.BackBrush);
                        Triangle.Paint(g, rectangle2, up, headerBackBrush, pen, pen2, pen, true);
                        pen.Dispose();
                        pen2.Dispose();
                    }
                    int num5 = rect.Width + (flag2 ? rect.Height : 0);
                    if (!this.FlatMode)
                    {
                        if (flag && flag2)
                        {
                            rect.X -= rect.Height;
                        }
                        rect.Width = num5;
                        ControlPaint.DrawBorder3D(g, rect, Border3DStyle.RaisedInner);
                    }
                    num += num5;
                }
            }
            if (num < boundingRect.Width)
            {
                rect = boundingRect;
                if (!flag)
                {
                    rect.X += num;
                }
                rect.Width -= num;
                g.FillRectangle(this.backgroundBrush, rect);
            }
            return num;
        }

        private void PaintGrid(Graphics g, Rectangle gridBounds)
        {
            Rectangle boundingRect = gridBounds;
            if (this.listManager != null)
            {
                if (this.layout.ColumnHeadersVisible)
                {
                    Region clip = g.Clip;
                    g.SetClip(this.layout.ColumnHeaders);
                    this.PaintColumnHeaders(g);
                    g.Clip = clip;
                    clip.Dispose();
                    int height = this.layout.ColumnHeaders.Height;
                    boundingRect.Y += height;
                    boundingRect.Height -= height;
                }
                if (this.layout.TopLeftHeader.Width > 0)
                {
                    if (this.myGridTable.IsDefault)
                    {
                        g.FillRectangle(this.HeaderBackBrush, this.layout.TopLeftHeader);
                    }
                    else
                    {
                        g.FillRectangle(this.myGridTable.HeaderBackBrush, this.layout.TopLeftHeader);
                    }
                    if (!this.FlatMode)
                    {
                        ControlPaint.DrawBorder3D(g, this.layout.TopLeftHeader, Border3DStyle.RaisedInner);
                    }
                }
                this.PaintRows(g, ref boundingRect);
            }
            if (boundingRect.Height > 0)
            {
                g.FillRectangle(this.backgroundBrush, boundingRect);
            }
        }

        private void PaintRows(Graphics g, ref Rectangle boundingRect)
        {
            int num = 0;
            bool alignToRight = this.isRightToLeft();
            Rectangle rect = boundingRect;
            Rectangle empty = Rectangle.Empty;
            bool rowHeadersVisible = this.layout.RowHeadersVisible;
            Rectangle rectangle3 = Rectangle.Empty;
            int dataGridRowsLength = this.DataGridRowsLength;
            DataGridRow[] dataGridRows = this.DataGridRows;
            int numVisibleColumns = this.myGridTable.GridColumnStyles.Count - this.firstVisibleCol;
            for (int i = this.firstVisibleRow; i < dataGridRowsLength; i++)
            {
                if (num > boundingRect.Height)
                {
                    break;
                }
                rect = boundingRect;
                rect.Height = dataGridRows[i].Height;
                rect.Y = boundingRect.Y + num;
                if (rowHeadersVisible)
                {
                    rectangle3 = rect;
                    rectangle3.Width = this.layout.RowHeaders.Width;
                    if (alignToRight)
                    {
                        rectangle3.X = rect.Right - rectangle3.Width;
                    }
                    if (g.IsVisible(rectangle3))
                    {
                        dataGridRows[i].PaintHeader(g, rectangle3, alignToRight, this.gridState[0x8000]);
                        g.ExcludeClip(rectangle3);
                    }
                    if (!alignToRight)
                    {
                        rect.X += rectangle3.Width;
                    }
                    rect.Width -= rectangle3.Width;
                }
                if (g.IsVisible(rect))
                {
                    empty = rect;
                    if (!alignToRight)
                    {
                        empty.X -= this.negOffset;
                    }
                    empty.Width += this.negOffset;
                    dataGridRows[i].Paint(g, empty, rect, this.firstVisibleCol, numVisibleColumns, alignToRight);
                }
                num += rect.Height;
            }
            boundingRect.Y += num;
            boundingRect.Height -= num;
        }

        private void PairTableStylesAndGridColumns(CurrencyManager lm, DataGridTableStyle gridTable, bool forceColumnCreation)
        {
            PropertyDescriptorCollection itemProperties = lm.GetItemProperties();
            GridColumnStylesCollection gridColumnStyles = gridTable.GridColumnStyles;
            if (!gridTable.IsDefault && (string.Compare(lm.GetListName(), gridTable.MappingName, true, CultureInfo.InvariantCulture) == 0))
            {
                if ((gridTable.GridColumnStyles.Count == 0) && !base.DesignMode)
                {
                    if (forceColumnCreation)
                    {
                        gridTable.SetGridColumnStylesCollection(lm);
                    }
                    else
                    {
                        gridTable.SetRelationsList(lm);
                    }
                }
                else
                {
                    for (int i = 0; i < gridColumnStyles.Count; i++)
                    {
                        gridColumnStyles[i].PropertyDescriptor = null;
                    }
                    for (int j = 0; j < itemProperties.Count; j++)
                    {
                        DataGridColumnStyle style = gridColumnStyles.MapColumnStyleToPropertyName(itemProperties[j].Name);
                        if (style != null)
                        {
                            style.PropertyDescriptor = itemProperties[j];
                        }
                    }
                    gridTable.SetRelationsList(lm);
                }
            }
            else
            {
                gridTable.SetGridColumnStylesCollection(lm);
                if ((gridTable.GridColumnStyles.Count > 0) && (gridTable.GridColumnStyles[0].Width == -1))
                {
                    this.InitializeColumnWidths();
                }
            }
        }

        internal void ParentRowsDataChanged()
        {
            this.parentRows.Clear();
            this.caption.BackButtonActive = this.caption.DownButtonActive = this.caption.BackButtonVisible = false;
            this.caption.SetDownButtonDirection(!this.layout.ParentRowsVisible);
            object dataSource = this.originalState.DataSource;
            string dataMember = this.originalState.DataMember;
            this.originalState = null;
            this.Set_ListManager(dataSource, dataMember, true);
        }

        internal bool ParentRowsIsEmpty()
        {
            return this.parentRows.IsEmpty();
        }

        private void PreferredColumnWidthChanged(object sender, EventArgs e)
        {
            this.SetDataGridRows(null, this.DataGridRowsLength);
            base.PerformLayout();
            base.Invalidate();
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            DataGridRow[] dataGridRows = this.DataGridRows;
            if (((this.listManager != null) && (this.DataGridRowsLength > 0)) && dataGridRows[this.currentRow].OnKeyPress(keyData))
            {
                return true;
            }
            switch ((keyData & Keys.KeyCode))
            {
                case Keys.Escape:
                case Keys.Space:
                case Keys.PageUp:
                case Keys.Next:
                case Keys.Left:
                case Keys.Up:
                case Keys.Right:
                case Keys.Down:
                case Keys.Enter:
                case Keys.Tab:
                case Keys.A:
                case Keys.Delete:
                case Keys.Add:
                case Keys.Subtract:
                case Keys.Oemplus:
                case Keys.OemMinus:
                {
                    KeyEventArgs ke = new KeyEventArgs(keyData);
                    if (!this.ProcessGridKey(ke))
                    {
                        break;
                    }
                    return true;
                }
                case Keys.C:
                    if ((((keyData & Keys.Control) != Keys.None) && ((keyData & Keys.Alt) == Keys.None)) && this.Bound)
                    {
                        if (this.numSelectedRows != 0)
                        {
                            int num = 0;
                            string data = "";
                            for (int i = 0; i < this.DataGridRowsLength; i++)
                            {
                                if (dataGridRows[i].Selected)
                                {
                                    GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
                                    int count = gridColumnStyles.Count;
                                    for (int j = 0; j < count; j++)
                                    {
                                        DataGridColumnStyle style2 = gridColumnStyles[j];
                                        data = data + style2.GetDisplayText(style2.GetColumnValueAtRow(this.ListManager, i));
                                        if (j < (count - 1))
                                        {
                                            data = data + this.GetOutputTextDelimiter();
                                        }
                                    }
                                    if (num < (this.numSelectedRows - 1))
                                    {
                                        data = data + "\r\n";
                                    }
                                    num++;
                                }
                            }
                            Clipboard.SetDataObject(data);
                            return true;
                        }
                        if (this.currentRow < this.ListManager.Count)
                        {
                            GridColumnStylesCollection styless = this.myGridTable.GridColumnStyles;
                            if ((this.currentCol >= 0) && (this.currentCol < styless.Count))
                            {
                                DataGridColumnStyle style = styless[this.currentCol];
                                Clipboard.SetDataObject(style.GetDisplayText(style.GetColumnValueAtRow(this.ListManager, this.currentRow)));
                                return true;
                            }
                        }
                    }
                    break;
            }
            return base.ProcessDialogKey(keyData);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected bool ProcessGridKey(KeyEventArgs ke)
        {
            if ((this.listManager == null) || (this.myGridTable == null))
            {
                return false;
            }
            DataGridRow[] dataGridRows = this.DataGridRows;
            KeyEventArgs args = ke;
            if (this.isRightToLeft())
            {
                switch (ke.KeyCode)
                {
                    case Keys.Left:
                        args = new KeyEventArgs(Keys.Right | ke.Modifiers);
                        break;

                    case Keys.Right:
                        args = new KeyEventArgs(Keys.Left | ke.Modifiers);
                        break;
                }
            }
            GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
            int num = 0;
            int count = gridColumnStyles.Count;
            for (int i = 0; i < gridColumnStyles.Count; i++)
            {
                if (gridColumnStyles[i].PropertyDescriptor != null)
                {
                    num = i;
                    break;
                }
            }
            for (int j = gridColumnStyles.Count - 1; j >= 0; j--)
            {
                if (gridColumnStyles[j].PropertyDescriptor != null)
                {
                    count = j;
                    break;
                }
            }
            switch (args.KeyCode)
            {
                case Keys.Escape:
                    this.gridState[0x80000] = false;
                    this.ResetSelection();
                    if (this.gridState[0x8000])
                    {
                        this.AbortEdit();
                        if (this.layout.RowHeadersVisible && (this.currentRow > -1))
                        {
                            Rectangle rowRect = this.GetRowRect(this.currentRow);
                            rowRect.Width = this.layout.RowHeaders.Width;
                            base.Invalidate(rowRect);
                        }
                        this.Edit();
                    }
                    else
                    {
                        this.CancelEditing();
                        this.Edit();
                        return false;
                    }
                    break;

                case Keys.Space:
                    this.gridState[0x80000] = false;
                    if (this.dataGridRowsLength != 0)
                    {
                        if (args.Shift)
                        {
                            this.ResetSelection();
                            this.EndEdit();
                            this.DataGridRows[this.currentRow].Selected = true;
                            this.numSelectedRows = 1;
                            return true;
                        }
                        return false;
                    }
                    return true;

                case Keys.PageUp:
                    if (this.dataGridRowsLength != 0)
                    {
                        this.gridState[0x80000] = false;
                        if (args.Shift)
                        {
                            int currentRow = this.currentRow;
                            this.CurrentRow = Math.Max(0, this.CurrentRow - this.numTotallyVisibleRows);
                            DataGridRow[] rowArray8 = this.DataGridRows;
                            for (int k = currentRow; k >= this.currentRow; k--)
                            {
                                if (!rowArray8[k].Selected)
                                {
                                    rowArray8[k].Selected = true;
                                    this.numSelectedRows++;
                                }
                            }
                            this.EndEdit();
                        }
                        else if (args.Control && !args.Alt)
                        {
                            this.ParentRowsVisible = false;
                        }
                        else
                        {
                            this.ResetSelection();
                            this.CurrentRow = Math.Max(0, this.CurrentRow - this.numTotallyVisibleRows);
                        }
                        break;
                    }
                    return true;

                case Keys.Next:
                    this.gridState[0x80000] = false;
                    if (this.dataGridRowsLength != 0)
                    {
                        if (args.Shift)
                        {
                            int num9 = this.currentRow;
                            this.CurrentRow = Math.Min((int) (this.DataGridRowsLength - (this.policy.AllowAdd ? 2 : 1)), (int) (this.currentRow + this.numTotallyVisibleRows));
                            DataGridRow[] rowArray7 = this.DataGridRows;
                            for (int m = num9; m <= this.currentRow; m++)
                            {
                                if (!rowArray7[m].Selected)
                                {
                                    rowArray7[m].Selected = true;
                                    this.numSelectedRows++;
                                }
                            }
                            this.EndEdit();
                        }
                        else if (args.Control && !args.Alt)
                        {
                            this.ParentRowsVisible = true;
                        }
                        else
                        {
                            this.ResetSelection();
                            this.CurrentRow = Math.Min((int) (this.DataGridRowsLength - (this.policy.AllowAdd ? 2 : 1)), (int) (this.CurrentRow + this.numTotallyVisibleRows));
                        }
                        break;
                    }
                    return true;

                case Keys.End:
                    this.gridState[0x80000] = false;
                    if (this.dataGridRowsLength != 0)
                    {
                        this.ResetSelection();
                        this.CurrentColumn = count;
                        if (args.Control && !args.Alt)
                        {
                            int num18 = this.currentRow;
                            this.CurrentRow = Math.Max(0, this.DataGridRowsLength - (this.policy.AllowAdd ? 2 : 1));
                            if (args.Shift)
                            {
                                DataGridRow[] rowArray10 = this.DataGridRows;
                                for (int n = num18; n <= this.currentRow; n++)
                                {
                                    rowArray10[n].Selected = true;
                                }
                                this.numSelectedRows = (this.currentRow - num18) + 1;
                                this.EndEdit();
                            }
                            return true;
                        }
                        break;
                    }
                    return true;

                case Keys.Home:
                    this.gridState[0x80000] = false;
                    if (this.dataGridRowsLength != 0)
                    {
                        this.ResetSelection();
                        this.CurrentColumn = 0;
                        if (!args.Control || args.Alt)
                        {
                            break;
                        }
                        int num16 = this.currentRow;
                        this.CurrentRow = 0;
                        if (args.Shift)
                        {
                            DataGridRow[] rowArray9 = this.DataGridRows;
                            for (int num17 = 0; num17 <= num16; num17++)
                            {
                                rowArray9[num17].Selected = true;
                                this.numSelectedRows++;
                            }
                            this.EndEdit();
                        }
                        return true;
                    }
                    return true;

                case Keys.Left:
                    this.gridState[0x80000] = false;
                    this.ResetSelection();
                    if ((args.Modifiers & ~Keys.KeyCode) == Keys.Alt)
                    {
                        if (this.Caption.BackButtonVisible)
                        {
                            this.NavigateBack();
                        }
                        return true;
                    }
                    if ((args.Modifiers & Keys.Control) == Keys.Control)
                    {
                        this.CurrentColumn = num;
                    }
                    else if ((this.currentCol == num) && (this.currentRow != 0))
                    {
                        this.CurrentRow--;
                        int num13 = this.MoveLeftRight(this.myGridTable.GridColumnStyles, this.myGridTable.GridColumnStyles.Count, false);
                        this.CurrentColumn = num13;
                    }
                    else
                    {
                        int num14 = this.MoveLeftRight(this.myGridTable.GridColumnStyles, this.currentCol, false);
                        if (num14 == -1)
                        {
                            if (this.currentRow == 0)
                            {
                                return true;
                            }
                            this.CurrentRow--;
                            this.CurrentColumn = count;
                        }
                        else
                        {
                            this.CurrentColumn = num14;
                        }
                    }
                    break;

                case Keys.Up:
                    this.gridState[0x80000] = false;
                    if (this.dataGridRowsLength != 0)
                    {
                        if (args.Control && !args.Alt)
                        {
                            if (args.Shift)
                            {
                                DataGridRow[] rowArray2 = this.DataGridRows;
                                int num5 = this.currentRow;
                                this.CurrentRow = 0;
                                this.ResetSelection();
                                for (int num6 = 0; num6 <= num5; num6++)
                                {
                                    rowArray2[num6].Selected = true;
                                }
                                this.numSelectedRows = num5 + 1;
                                this.EndEdit();
                                return true;
                            }
                            this.ResetSelection();
                            this.CurrentRow = 0;
                            return true;
                        }
                        if (args.Shift)
                        {
                            DataGridRow[] rowArray3 = this.DataGridRows;
                            if (rowArray3[this.currentRow].Selected)
                            {
                                if (this.currentRow >= 1)
                                {
                                    if (rowArray3[this.currentRow - 1].Selected)
                                    {
                                        if ((this.currentRow >= (this.DataGridRowsLength - 1)) || !rowArray3[this.currentRow + 1].Selected)
                                        {
                                            this.numSelectedRows--;
                                            rowArray3[this.currentRow].Selected = false;
                                        }
                                    }
                                    else
                                    {
                                        this.numSelectedRows += rowArray3[this.currentRow - 1].Selected ? 0 : 1;
                                        rowArray3[this.currentRow - 1].Selected = true;
                                    }
                                    this.CurrentRow--;
                                }
                            }
                            else
                            {
                                this.numSelectedRows++;
                                rowArray3[this.currentRow].Selected = true;
                                if (this.currentRow >= 1)
                                {
                                    this.numSelectedRows += rowArray3[this.currentRow - 1].Selected ? 0 : 1;
                                    rowArray3[this.currentRow - 1].Selected = true;
                                    this.CurrentRow--;
                                }
                            }
                            this.EndEdit();
                            return true;
                        }
                        if (args.Alt)
                        {
                            this.SetRowExpansionState(-1, false);
                            return true;
                        }
                        this.ResetSelection();
                        this.CurrentRow--;
                        this.Edit();
                        break;
                    }
                    return true;

                case Keys.Right:
                    this.gridState[0x80000] = false;
                    this.ResetSelection();
                    if (((args.Modifiers & Keys.Control) != Keys.Control) || args.Alt)
                    {
                        if ((this.currentCol == count) && (this.currentRow != (this.DataGridRowsLength - 1)))
                        {
                            this.CurrentRow++;
                            this.CurrentColumn = num;
                        }
                        else
                        {
                            int num15 = this.MoveLeftRight(this.myGridTable.GridColumnStyles, this.currentCol, true);
                            if (num15 == (gridColumnStyles.Count + 1))
                            {
                                this.CurrentColumn = num;
                                this.CurrentRow++;
                            }
                            else
                            {
                                this.CurrentColumn = num15;
                            }
                        }
                        break;
                    }
                    this.CurrentColumn = count;
                    break;

                case Keys.Down:
                    this.gridState[0x80000] = false;
                    if (this.dataGridRowsLength != 0)
                    {
                        if (args.Control && !args.Alt)
                        {
                            if (args.Shift)
                            {
                                int num7 = this.currentRow;
                                this.CurrentRow = Math.Max(0, this.DataGridRowsLength - (this.policy.AllowAdd ? 2 : 1));
                                DataGridRow[] rowArray4 = this.DataGridRows;
                                this.ResetSelection();
                                for (int num8 = num7; num8 <= this.currentRow; num8++)
                                {
                                    rowArray4[num8].Selected = true;
                                }
                                this.numSelectedRows = (this.currentRow - num7) + 1;
                                this.EndEdit();
                                return true;
                            }
                            this.ResetSelection();
                            this.CurrentRow = Math.Max(0, this.DataGridRowsLength - (this.policy.AllowAdd ? 2 : 1));
                            return true;
                        }
                        if (args.Shift)
                        {
                            DataGridRow[] rowArray5 = this.DataGridRows;
                            if (rowArray5[this.currentRow].Selected)
                            {
                                if (this.currentRow < ((this.DataGridRowsLength - (this.policy.AllowAdd ? 1 : 0)) - 1))
                                {
                                    if (rowArray5[this.currentRow + 1].Selected)
                                    {
                                        if ((this.currentRow == 0) || !rowArray5[this.currentRow - 1].Selected)
                                        {
                                            this.numSelectedRows--;
                                            rowArray5[this.currentRow].Selected = false;
                                        }
                                    }
                                    else
                                    {
                                        this.numSelectedRows += rowArray5[this.currentRow + 1].Selected ? 0 : 1;
                                        rowArray5[this.currentRow + 1].Selected = true;
                                    }
                                    this.CurrentRow++;
                                }
                            }
                            else
                            {
                                this.numSelectedRows++;
                                rowArray5[this.currentRow].Selected = true;
                                if (this.currentRow < ((this.DataGridRowsLength - (this.policy.AllowAdd ? 1 : 0)) - 1))
                                {
                                    this.CurrentRow++;
                                    this.numSelectedRows += rowArray5[this.currentRow].Selected ? 0 : 1;
                                    rowArray5[this.currentRow].Selected = true;
                                }
                            }
                            this.EndEdit();
                            return true;
                        }
                        if (args.Alt)
                        {
                            this.SetRowExpansionState(-1, true);
                            return true;
                        }
                        this.ResetSelection();
                        this.Edit();
                        this.CurrentRow++;
                        break;
                    }
                    return true;

                case Keys.Delete:
                    this.gridState[0x80000] = false;
                    if (!this.policy.AllowRemove || (this.numSelectedRows <= 0))
                    {
                        return false;
                    }
                    this.gridState[0x400] = true;
                    this.DeleteRows(dataGridRows);
                    this.currentRow = (this.listManager.Count == 0) ? 0 : this.listManager.Position;
                    this.numSelectedRows = 0;
                    break;

                case Keys.Enter:
                    this.gridState[0x80000] = false;
                    this.ResetSelection();
                    if (!this.gridState[0x8000])
                    {
                        return false;
                    }
                    if (((args.Modifiers & Keys.Control) != Keys.None) && !args.Alt)
                    {
                        this.EndEdit();
                        this.HandleEndCurrentEdit();
                        this.Edit();
                    }
                    else
                    {
                        this.CurrentRow = this.currentRow + 1;
                    }
                    break;

                case Keys.Tab:
                    return this.ProcessTabKey(args.KeyData);

                case Keys.Oemplus:
                case Keys.Add:
                    this.gridState[0x80000] = false;
                    if (args.Control)
                    {
                        this.SetRowExpansionState(-1, true);
                        this.EndEdit();
                        return true;
                    }
                    return false;

                case Keys.OemMinus:
                case Keys.Subtract:
                    this.gridState[0x80000] = false;
                    if (args.Control && !args.Alt)
                    {
                        this.SetRowExpansionState(-1, false);
                        return true;
                    }
                    return false;

                case Keys.F2:
                    this.gridState[0x80000] = false;
                    this.ResetSelection();
                    this.Edit();
                    break;

                case Keys.A:
                {
                    this.gridState[0x80000] = false;
                    if (!args.Control || args.Alt)
                    {
                        return false;
                    }
                    DataGridRow[] rowArray11 = this.DataGridRows;
                    for (int num20 = 0; num20 < this.DataGridRowsLength; num20++)
                    {
                        if (rowArray11[num20] is DataGridRelationshipRow)
                        {
                            rowArray11[num20].Selected = true;
                        }
                    }
                    this.numSelectedRows = this.DataGridRowsLength - (this.policy.AllowAdd ? 1 : 0);
                    this.EndEdit();
                    return true;
                }
            }
            return true;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessKeyPreview(ref Message m)
        {
            if (m.Msg == 0x100)
            {
                KeyEventArgs ke = new KeyEventArgs(((Keys) ((int) ((long) m.WParam))) | Control.ModifierKeys);
                switch (ke.KeyCode)
                {
                    case Keys.Escape:
                    case Keys.Space:
                    case Keys.PageUp:
                    case Keys.Next:
                    case Keys.End:
                    case Keys.Home:
                    case Keys.Left:
                    case Keys.Up:
                    case Keys.Right:
                    case Keys.Down:
                    case Keys.Delete:
                    case Keys.Enter:
                    case Keys.Tab:
                    case Keys.Oemplus:
                    case Keys.OemMinus:
                    case Keys.F2:
                    case Keys.Add:
                    case Keys.Subtract:
                    case Keys.A:
                        return this.ProcessGridKey(ke);
                }
            }
            else if (m.Msg == 0x101)
            {
                KeyEventArgs args2 = new KeyEventArgs(((Keys) ((int) ((long) m.WParam))) | Control.ModifierKeys);
                if (args2.KeyCode == Keys.Tab)
                {
                    return this.ProcessGridKey(args2);
                }
            }
            return base.ProcessKeyPreview(ref m);
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected bool ProcessTabKey(Keys keyData)
        {
            if ((this.listManager == null) || (this.myGridTable == null))
            {
                return false;
            }
            bool flag = false;
            int count = this.myGridTable.GridColumnStyles.Count;
            this.isRightToLeft();
            this.ResetSelection();
            if (this.gridState[0x8000])
            {
                flag = true;
                if (!this.CommitEdit())
                {
                    this.Edit();
                    return true;
                }
            }
            if ((keyData & Keys.Control) == Keys.Control)
            {
                if ((keyData & Keys.Alt) == Keys.Alt)
                {
                    return true;
                }
                Keys keys = keyData & ~Keys.Control;
                this.EndEdit();
                this.gridState[0x10000] = true;
                try
                {
                    this.FocusInternal();
                }
                finally
                {
                    this.gridState[0x10000] = false;
                }
                bool flag2 = false;
                System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                try
                {
                    flag2 = base.ProcessDialogKey(keys);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                return flag2;
            }
            DataGridRow[] dataGridRows = this.DataGridRows;
            GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
            int num = 0;
            int num2 = gridColumnStyles.Count - 1;
            if (dataGridRows.Length == 0)
            {
                this.EndEdit();
                bool flag3 = false;
                System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                try
                {
                    flag3 = base.ProcessDialogKey(keyData);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                return flag3;
            }
            for (int i = 0; i < gridColumnStyles.Count; i++)
            {
                if (gridColumnStyles[i].PropertyDescriptor != null)
                {
                    num2 = i;
                    break;
                }
            }
            for (int j = gridColumnStyles.Count - 1; j >= 0; j--)
            {
                if (gridColumnStyles[j].PropertyDescriptor != null)
                {
                    num = j;
                    break;
                }
            }
            if (this.CurrentColumn == num)
            {
                if ((this.gridState[0x80000] || (!this.gridState[0x80000] && ((keyData & Keys.Shift) != Keys.Shift))) && dataGridRows[this.CurrentRow].ProcessTabKey(keyData, this.layout.RowHeaders, this.isRightToLeft()))
                {
                    if (gridColumnStyles.Count > 0)
                    {
                        gridColumnStyles[this.CurrentColumn].ConcedeFocus();
                    }
                    this.gridState[0x80000] = true;
                    if ((this.gridState[0x800] && base.CanFocus) && !this.Focused)
                    {
                        this.FocusInternal();
                    }
                    return true;
                }
                if ((this.currentRow == (this.DataGridRowsLength - 1)) && ((keyData & Keys.Shift) == Keys.None))
                {
                    this.EndEdit();
                    bool flag4 = false;
                    System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                    try
                    {
                        flag4 = base.ProcessDialogKey(keyData);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    return flag4;
                }
            }
            if (this.CurrentColumn == num2)
            {
                if (!this.gridState[0x80000])
                {
                    if (((this.CurrentRow != 0) && ((keyData & Keys.Shift) == Keys.Shift)) && dataGridRows[this.CurrentRow - 1].ProcessTabKey(keyData, this.layout.RowHeaders, this.isRightToLeft()))
                    {
                        this.CurrentRow--;
                        if (gridColumnStyles.Count > 0)
                        {
                            gridColumnStyles[this.CurrentColumn].ConcedeFocus();
                        }
                        this.gridState[0x80000] = true;
                        if ((this.gridState[0x800] && base.CanFocus) && !this.Focused)
                        {
                            this.FocusInternal();
                        }
                        return true;
                    }
                }
                else
                {
                    if (!dataGridRows[this.CurrentRow].ProcessTabKey(keyData, this.layout.RowHeaders, this.isRightToLeft()))
                    {
                        this.gridState[0x80000] = false;
                        this.CurrentColumn = num;
                    }
                    return true;
                }
                if ((this.currentRow == 0) && ((keyData & Keys.Shift) == Keys.Shift))
                {
                    this.EndEdit();
                    bool flag5 = false;
                    System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                    try
                    {
                        flag5 = base.ProcessDialogKey(keyData);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    return flag5;
                }
            }
            if ((keyData & Keys.Shift) != Keys.Shift)
            {
                if (this.CurrentColumn == num)
                {
                    if (this.CurrentRow != (this.DataGridRowsLength - 1))
                    {
                        this.CurrentColumn = num2;
                    }
                    this.CurrentRow++;
                }
                else
                {
                    int num5 = this.MoveLeftRight(gridColumnStyles, this.currentCol, true);
                    this.CurrentColumn = num5;
                }
            }
            else if (this.CurrentColumn == num2)
            {
                if (this.CurrentRow != 0)
                {
                    this.CurrentColumn = num;
                }
                if (!this.gridState[0x80000])
                {
                    this.CurrentRow--;
                }
            }
            else if (this.gridState[0x80000] && (this.CurrentColumn == num))
            {
                this.InvalidateRow(this.currentRow);
                this.Edit();
            }
            else
            {
                int num6 = this.MoveLeftRight(gridColumnStyles, this.currentCol, false);
                this.CurrentColumn = num6;
            }
            this.gridState[0x80000] = false;
            if (flag)
            {
                this.ResetSelection();
                this.Edit();
            }
            return true;
        }

        internal void RecalculateFonts()
        {
            try
            {
                this.linkFont = new Font(this.Font, FontStyle.Underline);
            }
            catch
            {
            }
            this.fontHeight = this.Font.Height;
            this.linkFontHeight = this.LinkFont.Height;
            this.captionFontHeight = this.CaptionFont.Height;
            if ((this.myGridTable == null) || this.myGridTable.IsDefault)
            {
                this.headerFontHeight = this.HeaderFont.Height;
            }
            else
            {
                this.headerFontHeight = this.myGridTable.HeaderFont.Height;
            }
        }

        private void RecreateDataGridRows()
        {
            int newRowsLength = 0;
            CurrencyManager listManager = this.ListManager;
            if (listManager != null)
            {
                newRowsLength = listManager.Count;
                if (this.policy.AllowAdd)
                {
                    newRowsLength++;
                }
            }
            this.SetDataGridRows(null, newRowsLength);
        }

        public void ResetAlternatingBackColor()
        {
            if (this.ShouldSerializeAlternatingBackColor())
            {
                this.AlternatingBackColor = DefaultAlternatingBackBrush.Color;
                this.InvalidateInside();
            }
        }

        public override void ResetBackColor()
        {
            if (!this.BackColor.Equals(DefaultBackBrush.Color))
            {
                this.BackColor = DefaultBackBrush.Color;
            }
        }

        private void ResetBackgroundColor()
        {
            if ((this.backgroundBrush != null) && (this.BackgroundBrush != DefaultBackgroundBrush))
            {
                this.backgroundBrush.Dispose();
                this.backgroundBrush = null;
            }
            this.backgroundBrush = DefaultBackgroundBrush;
        }

        private void ResetCaptionBackColor()
        {
            this.Caption.ResetBackColor();
        }

        private void ResetCaptionFont()
        {
            this.Caption.ResetFont();
        }

        private void ResetCaptionForeColor()
        {
            this.Caption.ResetForeColor();
        }

        public override void ResetForeColor()
        {
            if (!this.ForeColor.Equals(DefaultForeBrush.Color))
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
                this.RecalculateFonts();
                base.PerformLayout();
                base.Invalidate(this.layout.Inside);
            }
        }

        public void ResetHeaderForeColor()
        {
            if (this.ShouldSerializeHeaderForeColor())
            {
                this.HeaderForeColor = DefaultHeaderForeBrush.Color;
            }
        }

        private void ResetHorizontalOffset()
        {
            this.horizontalOffset = 0;
            this.negOffset = 0;
            this.firstVisibleCol = 0;
            this.numVisibleCols = 0;
            this.lastTotallyVisibleCol = -1;
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

        private void ResetMouseState()
        {
            this.oldRow = -1;
            this.gridState[0x40000] = true;
        }

        private void ResetParentRows()
        {
            this.parentRows.Clear();
            this.originalState = null;
            this.caption.BackButtonActive = this.caption.DownButtonActive = this.caption.BackButtonVisible = false;
            this.caption.SetDownButtonDirection(!this.layout.ParentRowsVisible);
        }

        private void ResetParentRowsBackColor()
        {
            if (this.ShouldSerializeParentRowsBackColor())
            {
                this.parentRows.BackBrush = DefaultParentRowsBackBrush;
            }
        }

        private void ResetParentRowsForeColor()
        {
            if (this.ShouldSerializeParentRowsForeColor())
            {
                this.parentRows.ForeBrush = DefaultParentRowsForeBrush;
            }
        }

        private void ResetPreferredRowHeight()
        {
            this.prefferedRowHeight = defaultFontHeight + 3;
        }

        protected void ResetSelection()
        {
            if (this.numSelectedRows > 0)
            {
                DataGridRow[] dataGridRows = this.DataGridRows;
                for (int i = 0; i < this.DataGridRowsLength; i++)
                {
                    if (dataGridRows[i].Selected)
                    {
                        dataGridRows[i].Selected = false;
                    }
                }
            }
            this.numSelectedRows = 0;
            this.lastRowSelected = -1;
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

        private void ResetToolTip()
        {
            for (int i = 0; i < this.ToolTipId; i++)
            {
                this.ToolTipProvider.RemoveToolTip(new IntPtr(i));
            }
            if (!this.parentRows.IsEmpty())
            {
                bool alignRight = this.isRightToLeft();
                int detailsButtonWidth = this.Caption.GetDetailsButtonWidth();
                Rectangle rectangle = this.Caption.GetBackButtonRect(this.layout.Caption, alignRight, detailsButtonWidth);
                Rectangle detailsButtonRect = this.Caption.GetDetailsButtonRect(this.layout.Caption, alignRight);
                rectangle.X = this.MirrorRectangle(rectangle, this.layout.Inside, this.isRightToLeft());
                detailsButtonRect.X = this.MirrorRectangle(detailsButtonRect, this.layout.Inside, this.isRightToLeft());
                this.ToolTipProvider.AddToolTip(System.Windows.Forms.SR.GetString("DataGridCaptionBackButtonToolTip"), new IntPtr(0), rectangle);
                this.ToolTipProvider.AddToolTip(System.Windows.Forms.SR.GetString("DataGridCaptionDetailsButtonToolTip"), new IntPtr(1), detailsButtonRect);
                this.ToolTipId = 2;
            }
            else
            {
                this.ToolTipId = 0;
            }
        }

        private void ResetUIState()
        {
            this.gridState[0x80000] = false;
            this.ResetSelection();
            this.ResetMouseState();
            base.PerformLayout();
            base.Invalidate();
            if (this.horizScrollBar.Visible)
            {
                this.horizScrollBar.Invalidate();
            }
            if (this.vertScrollBar.Visible)
            {
                this.vertScrollBar.Invalidate();
            }
        }

        private void RowAutoResize(int row)
        {
            this.EndEdit();
            CurrencyManager listManager = this.ListManager;
            if (listManager != null)
            {
                using (Graphics graphics = base.CreateGraphicsInternal())
                {
                    GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
                    DataGridRow row2 = this.DataGridRows[row];
                    int count = listManager.Count;
                    int num = 0;
                    int num2 = gridColumnStyles.Count;
                    for (int i = 0; i < num2; i++)
                    {
                        object columnValueAtRow = gridColumnStyles[i].GetColumnValueAtRow(listManager, row);
                        num = Math.Max(num, gridColumnStyles[i].GetPreferredHeight(graphics, columnValueAtRow));
                    }
                    if (row2.Height != num)
                    {
                        row2.Height = num;
                        base.PerformLayout();
                        Rectangle data = this.layout.Data;
                        if (this.layout.RowHeadersVisible)
                        {
                            data = Rectangle.Union(data, this.layout.RowHeaders);
                        }
                        int rowTop = this.GetRowTop(row);
                        data.Height -= data.Y - rowTop;
                        data.Y = rowTop;
                        base.Invalidate(data);
                    }
                }
            }
        }

        private void RowHeadersVisibleChanged(object sender, EventArgs e)
        {
            this.layout.RowHeadersVisible = (this.myGridTable != null) && this.myGridTable.RowHeadersVisible;
            base.PerformLayout();
            this.InvalidateInside();
        }

        private void RowHeaderWidthChanged(object sender, EventArgs e)
        {
            if (this.layout.RowHeadersVisible)
            {
                base.PerformLayout();
                this.InvalidateInside();
            }
        }

        private void RowResizeBegin(MouseEventArgs e, int row)
        {
            int y = e.Y;
            this.EndEdit();
            Rectangle r = Rectangle.Union(this.layout.RowHeaders, this.layout.Data);
            int rowTop = this.GetRowTop(row);
            r.Y = rowTop + 3;
            r.Height = ((this.layout.Data.Y + this.layout.Data.Height) - rowTop) - 2;
            base.CaptureInternal = true;
            System.Windows.Forms.Cursor.ClipInternal = base.RectangleToScreen(r);
            this.gridState[0x10] = true;
            this.trackRowAnchor = y;
            this.trackRow = row;
            this.DrawRowSplitBar(e);
            this.lastSplitBar = e;
        }

        private void RowResizeEnd(MouseEventArgs e)
        {
            try
            {
                if (this.lastSplitBar != null)
                {
                    this.DrawRowSplitBar(this.lastSplitBar);
                    this.lastSplitBar = null;
                }
                int num = Math.Min(e.Y, (this.layout.Data.Y + this.layout.Data.Height) + 1);
                int num2 = num - this.GetRowBottom(this.trackRow);
                if ((this.trackRowAnchor != num) && (num2 != 0))
                {
                    DataGridRow row = this.DataGridRows[this.trackRow];
                    int num3 = row.Height + num2;
                    num3 = Math.Max(num3, 3);
                    row.Height = num3;
                    base.PerformLayout();
                    Rectangle rc = Rectangle.Union(this.layout.RowHeaders, this.layout.Data);
                    int rowTop = this.GetRowTop(this.trackRow);
                    rc.Height -= rc.Y - rowTop;
                    rc.Y = rowTop;
                    base.Invalidate(rc);
                }
            }
            finally
            {
                System.Windows.Forms.Cursor.ClipInternal = Rectangle.Empty;
                base.CaptureInternal = false;
            }
        }

        private void RowResizeMove(MouseEventArgs e)
        {
            if (this.lastSplitBar != null)
            {
                this.DrawRowSplitBar(this.lastSplitBar);
                this.lastSplitBar = e;
            }
            this.DrawRowSplitBar(e);
        }

        private void ScrollDown(int rows)
        {
            if (rows != 0)
            {
                this.ClearRegionCache();
                int to = Math.Max(0, Math.Min((int) (this.firstVisibleRow + rows), (int) (this.DataGridRowsLength - 1)));
                int firstVisibleRow = this.firstVisibleRow;
                this.firstVisibleRow = to;
                this.vertScrollBar.Value = to;
                bool flag = this.gridState[0x8000];
                this.ComputeVisibleRows();
                if (this.gridState[0x20000])
                {
                    this.Edit();
                    this.gridState[0x20000] = false;
                }
                else
                {
                    this.EndEdit();
                }
                int nYAmount = this.ComputeRowDelta(firstVisibleRow, to);
                Rectangle data = this.layout.Data;
                if (this.layout.RowHeadersVisible)
                {
                    data = Rectangle.Union(data, this.layout.RowHeaders);
                }
                System.Windows.Forms.NativeMethods.RECT rectScrollRegion = System.Windows.Forms.NativeMethods.RECT.FromXYWH(data.X, data.Y, data.Width, data.Height);
                System.Windows.Forms.SafeNativeMethods.ScrollWindow(new HandleRef(this, base.Handle), 0, nYAmount, ref rectScrollRegion, ref rectScrollRegion);
                this.OnScroll(EventArgs.Empty);
                if (flag)
                {
                    this.InvalidateRowHeader(this.currentRow);
                }
            }
        }

        private void ScrollRectangles(System.Windows.Forms.NativeMethods.RECT[] rects, int change)
        {
            if (rects != null)
            {
                if (this.isRightToLeft())
                {
                    change = -change;
                }
                for (int i = 0; i < rects.Length; i++)
                {
                    System.Windows.Forms.NativeMethods.RECT rectScrollRegion = rects[i];
                    System.Windows.Forms.SafeNativeMethods.ScrollWindow(new HandleRef(this, base.Handle), change, 0, ref rectScrollRegion, ref rectScrollRegion);
                }
            }
        }

        private void ScrollRight(int columns)
        {
            int num = this.firstVisibleCol + columns;
            GridColumnStylesCollection gridColumnStyles = this.myGridTable.GridColumnStyles;
            int num2 = 0;
            int count = gridColumnStyles.Count;
            int num4 = 0;
            if (this.myGridTable.IsDefault)
            {
                num4 = count;
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    if (gridColumnStyles[i].PropertyDescriptor != null)
                    {
                        num4++;
                    }
                }
            }
            if (((this.lastTotallyVisibleCol != (num4 - 1)) || (columns <= 0)) && (((this.firstVisibleCol != 0) || (columns >= 0)) || (this.negOffset != 0)))
            {
                num = Math.Min(num, count - 1);
                for (int j = 0; j < num; j++)
                {
                    if (gridColumnStyles[j].PropertyDescriptor != null)
                    {
                        num2 += gridColumnStyles[j].Width;
                    }
                }
                this.HorizontalOffset = num2;
            }
        }

        private void ScrollToColumn(int targetCol)
        {
            int columns = targetCol - this.firstVisibleCol;
            if ((targetCol > this.lastTotallyVisibleCol) && (this.lastTotallyVisibleCol != -1))
            {
                columns = targetCol - this.lastTotallyVisibleCol;
            }
            if ((columns != 0) || (this.negOffset != 0))
            {
                this.ScrollRight(columns);
            }
        }

        public void Select(int row)
        {
            DataGridRow[] dataGridRows = this.DataGridRows;
            if (!dataGridRows[row].Selected)
            {
                dataGridRows[row].Selected = true;
                this.numSelectedRows++;
            }
            this.EndEdit();
        }

        internal void Set_ListManager(object newDataSource, string newDataMember, bool force)
        {
            this.Set_ListManager(newDataSource, newDataMember, force, true);
        }

        internal void Set_ListManager(object newDataSource, string newDataMember, bool force, bool forceColumnCreation)
        {
            bool flag = this.DataSource != newDataSource;
            bool flag2 = this.DataMember != newDataMember;
            if ((force || flag) || (flag2 || !this.gridState[0x200000]))
            {
                this.gridState[0x200000] = true;
                if (this.toBeDisposedEditingControl != null)
                {
                    base.Controls.Remove(this.toBeDisposedEditingControl);
                    this.toBeDisposedEditingControl = null;
                }
                bool flag3 = true;
                try
                {
                    this.UpdateListManager();
                    if (this.listManager != null)
                    {
                        this.UnWireDataSource();
                    }
                    CurrencyManager listManager = this.listManager;
                    bool flag4 = false;
                    if (((newDataSource != null) && (this.BindingContext != null)) && (newDataSource != Convert.DBNull))
                    {
                        this.listManager = (CurrencyManager) this.BindingContext[newDataSource, newDataMember];
                    }
                    else
                    {
                        this.listManager = null;
                    }
                    this.dataSource = newDataSource;
                    this.dataMember = (newDataMember == null) ? "" : newDataMember;
                    flag4 = this.listManager != listManager;
                    if (this.listManager != null)
                    {
                        this.WireDataSource();
                        this.policy.UpdatePolicy(this.listManager, this.ReadOnly);
                    }
                    if (!this.Initializing && (this.listManager == null))
                    {
                        if (base.ContainsFocus && (this.ParentInternal == null))
                        {
                            for (int i = 0; i < base.Controls.Count; i++)
                            {
                                if (base.Controls[i].Focused)
                                {
                                    this.toBeDisposedEditingControl = base.Controls[i];
                                    break;
                                }
                            }
                            if ((this.toBeDisposedEditingControl == this.horizScrollBar) || (this.toBeDisposedEditingControl == this.vertScrollBar))
                            {
                                this.toBeDisposedEditingControl = null;
                            }
                        }
                        this.SetDataGridRows(null, 0);
                        this.defaultTableStyle.GridColumnStyles.Clear();
                        this.SetDataGridTable(this.defaultTableStyle, forceColumnCreation);
                        if (this.toBeDisposedEditingControl != null)
                        {
                            base.Controls.Add(this.toBeDisposedEditingControl);
                        }
                    }
                    if (flag4 || this.gridState[0x400000])
                    {
                        if (base.Visible)
                        {
                            base.BeginUpdateInternal();
                        }
                        if (this.listManager != null)
                        {
                            this.defaultTableStyle.GridColumnStyles.ResetDefaultColumnCollection();
                            DataGridTableStyle newTable = this.dataGridTables[this.listManager.GetListName()];
                            if (newTable == null)
                            {
                                this.SetDataGridTable(this.defaultTableStyle, forceColumnCreation);
                            }
                            else
                            {
                                this.SetDataGridTable(newTable, forceColumnCreation);
                            }
                            this.currentRow = (this.listManager.Position == -1) ? 0 : this.listManager.Position;
                        }
                        this.RecreateDataGridRows();
                        if (base.Visible)
                        {
                            base.EndUpdateInternal();
                        }
                        flag3 = false;
                        this.ComputeMinimumRowHeaderWidth();
                        if (this.myGridTable.IsDefault)
                        {
                            this.RowHeaderWidth = Math.Max(this.minRowHeaderWidth, this.RowHeaderWidth);
                        }
                        else
                        {
                            this.myGridTable.RowHeaderWidth = Math.Max(this.minRowHeaderWidth, this.RowHeaderWidth);
                        }
                        this.ListHasErrors = this.DataGridSourceHasErrors();
                        this.ResetUIState();
                        this.OnDataSourceChanged(EventArgs.Empty);
                    }
                }
                finally
                {
                    this.gridState[0x200000] = false;
                    if (flag3 && base.Visible)
                    {
                        base.EndUpdateInternal();
                    }
                }
            }
        }

        public void SetDataBinding(object dataSource, string dataMember)
        {
            this.parentRows.Clear();
            this.originalState = null;
            this.caption.BackButtonActive = this.caption.DownButtonActive = this.caption.BackButtonVisible = false;
            this.caption.SetDownButtonDirection(!this.layout.ParentRowsVisible);
            this.Set_ListManager(dataSource, dataMember, false);
        }

        internal void SetDataGridRows(DataGridRow[] newRows, int newRowsLength)
        {
            this.dataGridRows = newRows;
            this.dataGridRowsLength = newRowsLength;
            this.vertScrollBar.Maximum = Math.Max(0, this.DataGridRowsLength - 1);
            if (this.firstVisibleRow > newRowsLength)
            {
                this.vertScrollBar.Value = 0;
                this.firstVisibleRow = 0;
            }
            this.ResetUIState();
        }

        internal void SetDataGridTable(DataGridTableStyle newTable, bool forceColumnCreation)
        {
            if (this.myGridTable != null)
            {
                this.UnWireTableStylePropChanged(this.myGridTable);
                if (this.myGridTable.IsDefault)
                {
                    this.myGridTable.GridColumnStyles.ResetPropertyDescriptors();
                    this.myGridTable.ResetRelationsList();
                }
            }
            this.myGridTable = newTable;
            this.WireTableStylePropChanged(this.myGridTable);
            this.layout.RowHeadersVisible = newTable.IsDefault ? this.RowHeadersVisible : newTable.RowHeadersVisible;
            if (newTable != null)
            {
                newTable.DataGrid = this;
            }
            if (this.listManager != null)
            {
                this.PairTableStylesAndGridColumns(this.listManager, this.myGridTable, forceColumnCreation);
            }
            if (newTable != null)
            {
                newTable.ResetRelationsUI();
            }
            this.gridState[0x4000] = false;
            this.horizScrollBar.Value = 0;
            this.firstVisibleRow = 0;
            this.currentCol = 0;
            if (this.listManager == null)
            {
                this.currentRow = 0;
            }
            else
            {
                this.currentRow = (this.listManager.Position == -1) ? 0 : this.listManager.Position;
            }
            this.ResetHorizontalOffset();
            this.negOffset = 0;
            this.ResetUIState();
            this.checkHierarchy = true;
        }

        internal void SetParentRowsVisibility(bool visible)
        {
            Rectangle parentRows = this.layout.ParentRows;
            Rectangle data = this.layout.Data;
            if (this.layout.RowHeadersVisible)
            {
                data.X -= this.isRightToLeft() ? 0 : this.layout.RowHeaders.Width;
                data.Width += this.layout.RowHeaders.Width;
            }
            if (this.layout.ColumnHeadersVisible)
            {
                data.Y -= this.layout.ColumnHeaders.Height;
                data.Height += this.layout.ColumnHeaders.Height;
            }
            this.EndEdit();
            if (visible)
            {
                this.layout.ParentRowsVisible = true;
                base.PerformLayout();
                base.Invalidate();
            }
            else
            {
                System.Windows.Forms.NativeMethods.RECT rectScrollRegion = System.Windows.Forms.NativeMethods.RECT.FromXYWH(data.X, data.Y - this.layout.ParentRows.Height, data.Width, data.Height + this.layout.ParentRows.Height);
                System.Windows.Forms.SafeNativeMethods.ScrollWindow(new HandleRef(this, base.Handle), 0, -parentRows.Height, ref rectScrollRegion, ref rectScrollRegion);
                if (this.vertScrollBar.Visible)
                {
                    Rectangle bounds = this.vertScrollBar.Bounds;
                    bounds.Y -= parentRows.Height;
                    bounds.Height += parentRows.Height;
                    base.Invalidate(bounds);
                }
                this.layout.ParentRowsVisible = false;
                base.PerformLayout();
            }
        }

        private void SetRowExpansionState(int row, bool expanded)
        {
            if ((row < -1) || (row > (this.DataGridRowsLength - (this.policy.AllowAdd ? 2 : 1))))
            {
                throw new ArgumentOutOfRangeException("row");
            }
            DataGridRow[] dataGridRows = this.DataGridRows;
            if (row == -1)
            {
                DataGridRelationshipRow[] expandableRows = this.GetExpandableRows();
                bool flag = false;
                for (int i = 0; i < expandableRows.Length; i++)
                {
                    if (expandableRows[i].Expanded != expanded)
                    {
                        expandableRows[i].Expanded = expanded;
                        flag = true;
                    }
                }
                if (flag && (this.gridState[0x4000] || this.gridState[0x8000]))
                {
                    this.ResetSelection();
                    this.Edit();
                }
            }
            else if (dataGridRows[row] is DataGridRelationshipRow)
            {
                DataGridRelationshipRow row2 = (DataGridRelationshipRow) dataGridRows[row];
                if (row2.Expanded != expanded)
                {
                    if (this.gridState[0x4000] || this.gridState[0x8000])
                    {
                        this.ResetSelection();
                        this.Edit();
                    }
                    row2.Expanded = expanded;
                }
            }
        }

        protected virtual bool ShouldSerializeAlternatingBackColor()
        {
            return !this.AlternatingBackBrush.Equals(DefaultAlternatingBackBrush);
        }

        internal override bool ShouldSerializeBackColor()
        {
            return !DefaultBackBrush.Color.Equals(this.BackColor);
        }

        protected virtual bool ShouldSerializeBackgroundColor()
        {
            return !this.BackgroundBrush.Equals(DefaultBackgroundBrush);
        }

        protected virtual bool ShouldSerializeCaptionBackColor()
        {
            return this.Caption.ShouldSerializeBackColor();
        }

        private bool ShouldSerializeCaptionFont()
        {
            return this.Caption.ShouldSerializeFont();
        }

        protected virtual bool ShouldSerializeCaptionForeColor()
        {
            return this.Caption.ShouldSerializeForeColor();
        }

        internal override bool ShouldSerializeForeColor()
        {
            return !DefaultForeBrush.Color.Equals(this.ForeColor);
        }

        protected virtual bool ShouldSerializeGridLineColor()
        {
            return !this.GridLineBrush.Equals(DefaultGridLineBrush);
        }

        protected virtual bool ShouldSerializeHeaderBackColor()
        {
            return !this.HeaderBackBrush.Equals(DefaultHeaderBackBrush);
        }

        protected bool ShouldSerializeHeaderFont()
        {
            return (this.headerFont != null);
        }

        protected virtual bool ShouldSerializeHeaderForeColor()
        {
            return !this.HeaderForePen.Equals(DefaultHeaderForePen);
        }

        internal virtual bool ShouldSerializeLinkColor()
        {
            return !this.LinkBrush.Equals(DefaultLinkBrush);
        }

        protected virtual bool ShouldSerializeLinkHoverColor()
        {
            return false;
        }

        protected virtual bool ShouldSerializeParentRowsBackColor()
        {
            return !this.ParentRowsBackBrush.Equals(DefaultParentRowsBackBrush);
        }

        protected virtual bool ShouldSerializeParentRowsForeColor()
        {
            return !this.ParentRowsForeBrush.Equals(DefaultParentRowsForeBrush);
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

        public void SubObjectsSiteChange(bool site)
        {
            DataGrid grid = this;
            if (grid.DesignMode && (grid.Site != null))
            {
                IDesignerHost service = (IDesignerHost) grid.Site.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    DesignerTransaction transaction = service.CreateTransaction();
                    try
                    {
                        IContainer container = grid.Site.Container;
                        DataGridTableStyle[] ar = new DataGridTableStyle[grid.TableStyles.Count];
                        grid.TableStyles.CopyTo(ar, 0);
                        for (int i = 0; i < ar.Length; i++)
                        {
                            DataGridTableStyle component = ar[i];
                            this.ObjectSiteChange(container, component, site);
                            DataGridColumnStyle[] styleArray2 = new DataGridColumnStyle[component.GridColumnStyles.Count];
                            component.GridColumnStyles.CopyTo(styleArray2, 0);
                            for (int j = 0; j < styleArray2.Length; j++)
                            {
                                DataGridColumnStyle style2 = styleArray2[j];
                                this.ObjectSiteChange(container, style2, site);
                            }
                        }
                    }
                    finally
                    {
                        transaction.Commit();
                    }
                }
            }
        }

        private void TableStylesCollectionChanged(object sender, CollectionChangeEventArgs ccea)
        {
            if ((sender == this.dataGridTables) && (this.listManager != null))
            {
                if (ccea.Action == CollectionChangeAction.Add)
                {
                    DataGridTableStyle element = (DataGridTableStyle) ccea.Element;
                    if (this.listManager.GetListName().Equals(element.MappingName))
                    {
                        this.SetDataGridTable(element, true);
                        this.SetDataGridRows(null, 0);
                    }
                }
                else if (ccea.Action == CollectionChangeAction.Remove)
                {
                    DataGridTableStyle style2 = (DataGridTableStyle) ccea.Element;
                    if (this.myGridTable.MappingName.Equals(style2.MappingName))
                    {
                        this.defaultTableStyle.GridColumnStyles.ResetDefaultColumnCollection();
                        this.SetDataGridTable(this.defaultTableStyle, true);
                        this.SetDataGridRows(null, 0);
                    }
                }
                else
                {
                    DataGridTableStyle newTable = this.dataGridTables[this.listManager.GetListName()];
                    if (newTable == null)
                    {
                        if (!this.myGridTable.IsDefault)
                        {
                            this.defaultTableStyle.GridColumnStyles.ResetDefaultColumnCollection();
                            this.SetDataGridTable(this.defaultTableStyle, true);
                            this.SetDataGridRows(null, 0);
                        }
                    }
                    else
                    {
                        this.SetDataGridTable(newTable, true);
                        this.SetDataGridRows(null, 0);
                    }
                }
            }
        }

        internal void TextBoxOnMouseWheel(MouseEventArgs e)
        {
            this.OnMouseWheel(e);
        }

        public void UnSelect(int row)
        {
            DataGridRow[] dataGridRows = this.DataGridRows;
            if (dataGridRows[row].Selected)
            {
                dataGridRows[row].Selected = false;
                this.numSelectedRows--;
            }
        }

        private void UnWireDataSource()
        {
            this.listManager.CurrentChanged -= this.currentChangedHandler;
            this.listManager.PositionChanged -= this.positionChangedHandler;
            this.listManager.ItemChanged -= this.itemChangedHandler;
            this.listManager.MetaDataChanged -= this.metaDataChangedHandler;
        }

        private void UnWireTableStylePropChanged(DataGridTableStyle gridTable)
        {
            gridTable.GridLineColorChanged -= new EventHandler(this.GridLineColorChanged);
            gridTable.GridLineStyleChanged -= new EventHandler(this.GridLineStyleChanged);
            gridTable.HeaderBackColorChanged -= new EventHandler(this.HeaderBackColorChanged);
            gridTable.HeaderFontChanged -= new EventHandler(this.HeaderFontChanged);
            gridTable.HeaderForeColorChanged -= new EventHandler(this.HeaderForeColorChanged);
            gridTable.LinkColorChanged -= new EventHandler(this.LinkColorChanged);
            gridTable.LinkHoverColorChanged -= new EventHandler(this.LinkHoverColorChanged);
            gridTable.PreferredColumnWidthChanged -= new EventHandler(this.PreferredColumnWidthChanged);
            gridTable.RowHeadersVisibleChanged -= new EventHandler(this.RowHeadersVisibleChanged);
            gridTable.ColumnHeadersVisibleChanged -= new EventHandler(this.ColumnHeadersVisibleChanged);
            gridTable.RowHeaderWidthChanged -= new EventHandler(this.RowHeaderWidthChanged);
            gridTable.AllowSortingChanged -= new EventHandler(this.AllowSortingChanged);
        }

        private void UpdateListManager()
        {
            try
            {
                if (this.listManager != null)
                {
                    this.EndEdit();
                    this.listManager.EndCurrentEdit();
                }
            }
            catch
            {
            }
        }

        private void WireDataSource()
        {
            this.listManager.CurrentChanged += this.currentChangedHandler;
            this.listManager.PositionChanged += this.positionChangedHandler;
            this.listManager.ItemChanged += this.itemChangedHandler;
            this.listManager.MetaDataChanged += this.metaDataChangedHandler;
        }

        private void WireTableStylePropChanged(DataGridTableStyle gridTable)
        {
            gridTable.GridLineColorChanged += new EventHandler(this.GridLineColorChanged);
            gridTable.GridLineStyleChanged += new EventHandler(this.GridLineStyleChanged);
            gridTable.HeaderBackColorChanged += new EventHandler(this.HeaderBackColorChanged);
            gridTable.HeaderFontChanged += new EventHandler(this.HeaderFontChanged);
            gridTable.HeaderForeColorChanged += new EventHandler(this.HeaderForeColorChanged);
            gridTable.LinkColorChanged += new EventHandler(this.LinkColorChanged);
            gridTable.LinkHoverColorChanged += new EventHandler(this.LinkHoverColorChanged);
            gridTable.PreferredColumnWidthChanged += new EventHandler(this.PreferredColumnWidthChanged);
            gridTable.RowHeadersVisibleChanged += new EventHandler(this.RowHeadersVisibleChanged);
            gridTable.ColumnHeadersVisibleChanged += new EventHandler(this.ColumnHeadersVisibleChanged);
            gridTable.RowHeaderWidthChanged += new EventHandler(this.RowHeaderWidthChanged);
            gridTable.AllowSortingChanged += new EventHandler(this.AllowSortingChanged);
        }

        [DefaultValue(true), System.Windows.Forms.SRDescription("DataGridNavigationModeDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool AllowNavigation
        {
            get
            {
                return this.gridState[0x2000];
            }
            set
            {
                if (this.AllowNavigation != value)
                {
                    this.gridState[0x2000] = value;
                    this.Caption.BackButtonActive = !this.parentRows.IsEmpty() && value;
                    this.Caption.BackButtonVisible = this.Caption.BackButtonActive;
                    this.RecreateDataGridRows();
                    this.OnAllowNavigationChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRDescription("DataGridAllowSortingDescr"), DefaultValue(true), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool AllowSorting
        {
            get
            {
                return this.gridState[1];
            }
            set
            {
                if (this.AllowSorting != value)
                {
                    this.gridState[1] = value;
                    if (!value && (this.listManager != null))
                    {
                        IList list = this.listManager.List;
                        if (list is IBindingList)
                        {
                            ((IBindingList) list).RemoveSort();
                        }
                    }
                }
            }
        }

        internal Brush AlternatingBackBrush
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
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "AlternatingBackColor" }));
                }
                if (IsTransparentColor(value))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridTransparentAlternatingBackColorNotAllowed"));
                }
                if (!this.alternatingBackBrush.Color.Equals(value))
                {
                    this.alternatingBackBrush = new SolidBrush(value);
                    this.InvalidateInside();
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
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                if (IsTransparentColor(value))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridTransparentBackColorNotAllowed"));
                }
                base.BackColor = value;
            }
        }

        internal SolidBrush BackgroundBrush
        {
            get
            {
                return this.backgroundBrush;
            }
        }

        [System.Windows.Forms.SRCategory("CatColors"), System.Windows.Forms.SRDescription("DataGridBackgroundColorDescr")]
        public Color BackgroundColor
        {
            get
            {
                return this.backgroundBrush.Color;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "BackgroundColor" }));
                }
                if (!value.Equals(this.backgroundBrush.Color))
                {
                    if ((this.backgroundBrush != null) && (this.BackgroundBrush != DefaultBackgroundBrush))
                    {
                        this.backgroundBrush.Dispose();
                        this.backgroundBrush = null;
                    }
                    this.backgroundBrush = new SolidBrush(value);
                    base.Invalidate(this.layout.Inside);
                    this.OnBackgroundColorChanged(EventArgs.Empty);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override Image BackgroundImage
        {
            get
            {
                return base.BackgroundImage;
            }
            set
            {
                base.BackgroundImage = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override ImageLayout BackgroundImageLayout
        {
            get
            {
                return base.BackgroundImageLayout;
            }
            set
            {
                base.BackgroundImageLayout = value;
            }
        }

        [DefaultValue(2), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("DataGridBorderStyleDescr"), DispId(-504)]
        public System.Windows.Forms.BorderStyle BorderStyle
        {
            get
            {
                return this.borderStyle;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.BorderStyle));
                }
                if (this.borderStyle != value)
                {
                    this.borderStyle = value;
                    base.PerformLayout();
                    base.Invalidate();
                    this.OnBorderStyleChanged(EventArgs.Empty);
                }
            }
        }

        private int BorderWidth
        {
            get
            {
                if (this.BorderStyle == System.Windows.Forms.BorderStyle.Fixed3D)
                {
                    return SystemInformation.Border3DSize.Width;
                }
                if (this.BorderStyle == System.Windows.Forms.BorderStyle.FixedSingle)
                {
                    return 2;
                }
                return 0;
            }
        }

        private bool Bound
        {
            get
            {
                return ((this.listManager != null) && (this.myGridTable != null));
            }
        }

        internal DataGridCaption Caption
        {
            get
            {
                return this.caption;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridCaptionBackColorDescr"), System.Windows.Forms.SRCategory("CatColors")]
        public Color CaptionBackColor
        {
            get
            {
                return this.Caption.BackColor;
            }
            set
            {
                if (IsTransparentColor(value))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridTransparentCaptionBackColorNotAllowed"));
                }
                this.Caption.BackColor = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), Localizable(true), AmbientValue((string) null), System.Windows.Forms.SRDescription("DataGridCaptionFontDescr")]
        public Font CaptionFont
        {
            get
            {
                return this.Caption.Font;
            }
            set
            {
                this.Caption.Font = value;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridCaptionForeColorDescr"), System.Windows.Forms.SRCategory("CatColors")]
        public Color CaptionForeColor
        {
            get
            {
                return this.Caption.ForeColor;
            }
            set
            {
                this.Caption.ForeColor = value;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridCaptionTextDescr"), Localizable(true), DefaultValue(""), System.Windows.Forms.SRCategory("CatAppearance")]
        public string CaptionText
        {
            get
            {
                return this.Caption.Text;
            }
            set
            {
                this.Caption.Text = value;
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRCategory("CatDisplay"), System.Windows.Forms.SRDescription("DataGridCaptionVisibleDescr")]
        public bool CaptionVisible
        {
            get
            {
                return this.layout.CaptionVisible;
            }
            set
            {
                if (this.layout.CaptionVisible != value)
                {
                    this.layout.CaptionVisible = value;
                    base.PerformLayout();
                    base.Invalidate();
                    this.OnCaptionVisibleChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRDescription("DataGridColumnHeadersVisibleDescr"), System.Windows.Forms.SRCategory("CatDisplay")]
        public bool ColumnHeadersVisible
        {
            get
            {
                return this.gridState[2];
            }
            set
            {
                if (this.ColumnHeadersVisible != value)
                {
                    this.gridState[2] = value;
                    this.layout.ColumnHeadersVisible = value;
                    base.PerformLayout();
                    this.InvalidateInside();
                }
            }
        }

        [System.Windows.Forms.SRDescription("DataGridCurrentCellDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public DataGridCell CurrentCell
        {
            get
            {
                return new DataGridCell(this.currentRow, this.currentCol);
            }
            set
            {
                if (this.layout.dirty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridSettingCurrentCellNotGood"));
                }
                if (((value.RowNumber != this.currentRow) || (value.ColumnNumber != this.currentCol)) && (((this.DataGridRowsLength != 0) && (this.myGridTable.GridColumnStyles != null)) && (this.myGridTable.GridColumnStyles.Count != 0)))
                {
                    this.EnsureBound();
                    int currentRow = this.currentRow;
                    int currentCol = this.currentCol;
                    bool flag = this.gridState[0x8000];
                    bool flag2 = false;
                    bool flag3 = false;
                    int columnNumber = value.ColumnNumber;
                    int rowNumber = value.RowNumber;
                    string message = null;
                    try
                    {
                        int count = this.myGridTable.GridColumnStyles.Count;
                        if (columnNumber < 0)
                        {
                            columnNumber = 0;
                        }
                        if (columnNumber >= count)
                        {
                            columnNumber = count - 1;
                        }
                        int dataGridRowsLength = this.DataGridRowsLength;
                        DataGridRow[] dataGridRows = this.DataGridRows;
                        if (rowNumber < 0)
                        {
                            rowNumber = 0;
                        }
                        if (rowNumber >= dataGridRowsLength)
                        {
                            rowNumber = dataGridRowsLength - 1;
                        }
                        if (this.currentCol != columnNumber)
                        {
                            flag2 = true;
                            int position = this.ListManager.Position;
                            int num8 = this.ListManager.List.Count;
                            this.EndEdit();
                            if ((this.ListManager.Position != position) || (num8 != this.ListManager.List.Count))
                            {
                                this.RecreateDataGridRows();
                                if (this.ListManager.List.Count > 0)
                                {
                                    this.currentRow = this.ListManager.Position;
                                    this.Edit();
                                }
                                else
                                {
                                    this.currentRow = -1;
                                }
                                return;
                            }
                            this.currentCol = columnNumber;
                            this.InvalidateRow(this.currentRow);
                        }
                        if (this.currentRow != rowNumber)
                        {
                            flag2 = true;
                            int num9 = this.ListManager.Position;
                            int num10 = this.ListManager.List.Count;
                            this.EndEdit();
                            if ((this.ListManager.Position != num9) || (num10 != this.ListManager.List.Count))
                            {
                                this.RecreateDataGridRows();
                                if (this.ListManager.List.Count > 0)
                                {
                                    this.currentRow = this.ListManager.Position;
                                    this.Edit();
                                }
                                else
                                {
                                    this.currentRow = -1;
                                }
                                return;
                            }
                            if (this.currentRow < dataGridRowsLength)
                            {
                                dataGridRows[this.currentRow].OnRowLeave();
                            }
                            dataGridRows[rowNumber].OnRowEnter();
                            this.currentRow = rowNumber;
                            if (currentRow < dataGridRowsLength)
                            {
                                this.InvalidateRow(currentRow);
                            }
                            this.InvalidateRow(this.currentRow);
                            if (currentRow != this.listManager.Position)
                            {
                                flag3 = true;
                                if (this.gridState[0x8000])
                                {
                                    this.AbortEdit();
                                }
                            }
                            else if (this.gridState[0x100000])
                            {
                                this.ListManager.PositionChanged -= this.positionChangedHandler;
                                this.ListManager.CancelCurrentEdit();
                                this.ListManager.Position = this.currentRow;
                                this.ListManager.PositionChanged += this.positionChangedHandler;
                                dataGridRows[this.DataGridRowsLength - 1] = new DataGridAddNewRow(this, this.myGridTable, this.DataGridRowsLength - 1);
                                this.SetDataGridRows(dataGridRows, this.DataGridRowsLength);
                                this.gridState[0x100000] = false;
                            }
                            else
                            {
                                this.ListManager.EndCurrentEdit();
                                if (dataGridRowsLength != this.DataGridRowsLength)
                                {
                                    this.currentRow = (this.currentRow == (dataGridRowsLength - 1)) ? (this.DataGridRowsLength - 1) : this.currentRow;
                                }
                                if ((this.currentRow == (this.dataGridRowsLength - 1)) && this.policy.AllowAdd)
                                {
                                    this.AddNewRow();
                                }
                                else
                                {
                                    this.ListManager.Position = this.currentRow;
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        message = exception.Message;
                    }
                    if (message != null)
                    {
                        if (RTLAwareMessageBox.Show(null, System.Windows.Forms.SR.GetString("DataGridPushedIncorrectValueIntoColumn", new object[] { message }), System.Windows.Forms.SR.GetString("DataGridErrorMessageBoxCaption"), MessageBoxButtons.YesNo, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes)
                        {
                            this.currentRow = currentRow;
                            this.currentCol = currentCol;
                            this.InvalidateRowHeader(rowNumber);
                            this.InvalidateRowHeader(this.currentRow);
                            if (flag)
                            {
                                this.Edit();
                            }
                        }
                        else
                        {
                            if (((this.currentRow == (this.DataGridRowsLength - 1)) && (currentRow == (this.DataGridRowsLength - 2))) && (this.DataGridRows[this.currentRow] is DataGridAddNewRow))
                            {
                                rowNumber = currentRow;
                            }
                            this.currentRow = rowNumber;
                            this.listManager.PositionChanged -= this.positionChangedHandler;
                            this.listManager.CancelCurrentEdit();
                            this.listManager.Position = rowNumber;
                            this.listManager.PositionChanged += this.positionChangedHandler;
                            this.currentRow = rowNumber;
                            this.currentCol = columnNumber;
                            if (flag)
                            {
                                this.Edit();
                            }
                        }
                    }
                    if (flag2)
                    {
                        this.EnsureVisible(this.currentRow, this.currentCol);
                        this.OnCurrentCellChanged(EventArgs.Empty);
                        if (!flag3)
                        {
                            this.Edit();
                        }
                        base.AccessibilityNotifyClients(AccessibleEvents.Focus, this.CurrentCellAccIndex);
                        base.AccessibilityNotifyClients(AccessibleEvents.Selection, this.CurrentCellAccIndex);
                    }
                }
            }
        }

        internal int CurrentCellAccIndex
        {
            get
            {
                int num = 0;
                num++;
                num += this.myGridTable.GridColumnStyles.Count;
                num += this.DataGridRows.Length;
                if (this.horizScrollBar.Visible)
                {
                    num++;
                }
                if (this.vertScrollBar.Visible)
                {
                    num++;
                }
                return (num + ((this.currentRow * this.myGridTable.GridColumnStyles.Count) + this.currentCol));
            }
        }

        private int CurrentColumn
        {
            get
            {
                return this.CurrentCell.ColumnNumber;
            }
            set
            {
                this.CurrentCell = new DataGridCell(this.currentRow, value);
            }
        }

        private int CurrentRow
        {
            get
            {
                return this.CurrentCell.RowNumber;
            }
            set
            {
                this.CurrentCell = new DataGridCell(value, this.currentCol);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), System.Windows.Forms.SRDescription("DataGridSelectedIndexDescr")]
        public int CurrentRowIndex
        {
            get
            {
                if (this.originalState == null)
                {
                    if (this.listManager != null)
                    {
                        return this.listManager.Position;
                    }
                    return -1;
                }
                if (this.BindingContext == null)
                {
                    return -1;
                }
                CurrencyManager manager = (CurrencyManager) this.BindingContext[this.originalState.DataSource, this.originalState.DataMember];
                return manager.Position;
            }
            set
            {
                if (this.listManager == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataGridSetSelectIndex"));
                }
                if (this.originalState == null)
                {
                    this.listManager.Position = value;
                    this.currentRow = value;
                }
                else
                {
                    CurrencyManager manager = (CurrencyManager) this.BindingContext[this.originalState.DataSource, this.originalState.DataMember];
                    manager.Position = value;
                    this.originalState.LinkingRow = this.originalState.DataGridRows[value];
                    base.Invalidate();
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override System.Windows.Forms.Cursor Cursor
        {
            get
            {
                return base.Cursor;
            }
            set
            {
                base.Cursor = value;
            }
        }

        internal DataGridRow[] DataGridRows
        {
            get
            {
                if (this.dataGridRows == null)
                {
                    this.CreateDataGridRows();
                }
                return this.dataGridRows;
            }
        }

        internal int DataGridRowsLength
        {
            get
            {
                return this.dataGridRowsLength;
            }
        }

        [System.Windows.Forms.SRCategory("CatData"), System.Windows.Forms.SRDescription("DataGridDataMemberDescr"), DefaultValue((string) null), Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string DataMember
        {
            get
            {
                return this.dataMember;
            }
            set
            {
                if ((this.dataMember == null) || !this.dataMember.Equals(value))
                {
                    this.ResetParentRows();
                    this.Set_ListManager(this.DataSource, value, false);
                }
            }
        }

        [AttributeProvider(typeof(IListSource)), System.Windows.Forms.SRDescription("DataGridDataSourceDescr"), System.Windows.Forms.SRCategory("CatData"), DefaultValue((string) null), RefreshProperties(RefreshProperties.Repaint)]
        public object DataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                if (((value != null) && !(value is IList)) && !(value is IListSource))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("BadDataSourceForComplexBinding"));
                }
                if ((this.dataSource == null) || !this.dataSource.Equals(value))
                {
                    if (((value == null) || (value == Convert.DBNull)) && ((this.DataMember != null) && (this.DataMember.Length != 0)))
                    {
                        this.dataSource = null;
                        this.DataMember = "";
                    }
                    else
                    {
                        if (value != null)
                        {
                            this.EnforceValidDataMember(value);
                        }
                        this.ResetParentRows();
                        this.Set_ListManager(value, this.DataMember, false);
                    }
                }
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

        private static SolidBrush DefaultBackgroundBrush
        {
            get
            {
                return (SolidBrush) SystemBrushes.AppWorkspace;
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

        internal static SolidBrush DefaultParentRowsBackBrush
        {
            get
            {
                return (SolidBrush) SystemBrushes.Control;
            }
        }

        internal static SolidBrush DefaultParentRowsForeBrush
        {
            get
            {
                return (SolidBrush) SystemBrushes.WindowText;
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

        protected override Size DefaultSize
        {
            get
            {
                return new Size(130, 80);
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("DataGridFirstVisibleColumnDescr")]
        public int FirstVisibleColumn
        {
            get
            {
                return this.firstVisibleCol;
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("DataGridFlatModeDescr")]
        public bool FlatMode
        {
            get
            {
                return this.gridState[0x40];
            }
            set
            {
                if (value != this.FlatMode)
                {
                    this.gridState[0x40] = value;
                    base.Invalidate(this.layout.Inside);
                    this.OnFlatModeChanged(EventArgs.Empty);
                }
            }
        }

        internal int FontHeight
        {
            get
            {
                return this.fontHeight;
            }
        }

        internal SolidBrush ForeBrush
        {
            get
            {
                return this.foreBrush;
            }
        }

        [System.Windows.Forms.SRCategory("CatColors"), System.Windows.Forms.SRDescription("ControlForeColorDescr")]
        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
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
                if (this.gridLineBrush.Color != value)
                {
                    if (value.IsEmpty)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "GridLineColor" }));
                    }
                    this.gridLineBrush = new SolidBrush(value);
                    base.Invalidate(this.layout.Data);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("DataGridGridLineStyleDescr"), DefaultValue(1)]
        public DataGridLineStyle GridLineStyle
        {
            get
            {
                return this.gridLineStyle;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DataGridLineStyle));
                }
                if (this.gridLineStyle != value)
                {
                    this.gridLineStyle = value;
                    this.myGridTable.ResetRelationsUI();
                    base.Invalidate(this.layout.Data);
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
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "HeaderBackColor" }));
                }
                if (IsTransparentColor(value))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridTransparentHeaderBackColorNotAllowed"));
                }
                if (!value.Equals(this.headerBackBrush.Color))
                {
                    this.headerBackBrush = new SolidBrush(value);
                    if (this.layout.RowHeadersVisible)
                    {
                        base.Invalidate(this.layout.RowHeaders);
                    }
                    if (this.layout.ColumnHeadersVisible)
                    {
                        base.Invalidate(this.layout.ColumnHeaders);
                    }
                    base.Invalidate(this.layout.TopLeftHeader);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("DataGridHeaderFontDescr")]
        public Font HeaderFont
        {
            get
            {
                if (this.headerFont != null)
                {
                    return this.headerFont;
                }
                return this.Font;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("HeaderFont");
                }
                if (!value.Equals(this.headerFont))
                {
                    this.headerFont = value;
                    this.RecalculateFonts();
                    base.PerformLayout();
                    base.Invalidate(this.layout.Inside);
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

        [System.Windows.Forms.SRDescription("DataGridHeaderForeColorDescr"), System.Windows.Forms.SRCategory("CatColors")]
        public Color HeaderForeColor
        {
            get
            {
                return this.headerForePen.Color;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "HeaderForeColor" }));
                }
                if (!value.Equals(this.headerForePen.Color))
                {
                    this.headerForePen = new Pen(value);
                    this.headerForeBrush = new SolidBrush(value);
                    if (this.layout.RowHeadersVisible)
                    {
                        base.Invalidate(this.layout.RowHeaders);
                    }
                    if (this.layout.ColumnHeadersVisible)
                    {
                        base.Invalidate(this.layout.ColumnHeaders);
                    }
                    base.Invalidate(this.layout.TopLeftHeader);
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

        internal int HorizontalOffset
        {
            get
            {
                return this.horizontalOffset;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                int num2 = this.GetColumnWidthSum() - this.layout.Data.Width;
                if ((value > num2) && (num2 > 0))
                {
                    value = num2;
                }
                if (value != this.horizontalOffset)
                {
                    int change = this.horizontalOffset - value;
                    this.horizScrollBar.Value = value;
                    Rectangle data = this.layout.Data;
                    if (this.layout.ColumnHeadersVisible)
                    {
                        data = Rectangle.Union(data, this.layout.ColumnHeaders);
                    }
                    this.horizontalOffset = value;
                    this.firstVisibleCol = this.ComputeFirstVisibleColumn();
                    this.ComputeVisibleColumns();
                    if (this.gridState[0x20000])
                    {
                        if (((this.currentCol >= this.firstVisibleCol) && (this.currentCol < ((this.firstVisibleCol + this.numVisibleCols) - 1))) && (this.gridState[0x8000] || this.gridState[0x4000]))
                        {
                            this.Edit();
                        }
                        else
                        {
                            this.EndEdit();
                        }
                        this.gridState[0x20000] = false;
                    }
                    else
                    {
                        this.EndEdit();
                    }
                    System.Windows.Forms.NativeMethods.RECT[] rects = this.CreateScrollableRegion(data);
                    this.ScrollRectangles(rects, change);
                    this.OnScroll(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRDescription("DataGridHorizScrollBarDescr")]
        protected ScrollBar HorizScrollBar
        {
            get
            {
                return this.horizScrollBar;
            }
        }

        internal bool Initializing
        {
            get
            {
                return this.inInit;
            }
        }

        public object this[int rowIndex, int columnIndex]
        {
            get
            {
                this.EnsureBound();
                if ((rowIndex < 0) || (rowIndex >= this.DataGridRowsLength))
                {
                    throw new ArgumentOutOfRangeException("rowIndex");
                }
                if ((columnIndex < 0) || (columnIndex >= this.myGridTable.GridColumnStyles.Count))
                {
                    throw new ArgumentOutOfRangeException("columnIndex");
                }
                CurrencyManager listManager = this.listManager;
                DataGridColumnStyle style = this.myGridTable.GridColumnStyles[columnIndex];
                return style.GetColumnValueAtRow(listManager, rowIndex);
            }
            set
            {
                this.EnsureBound();
                if ((rowIndex < 0) || (rowIndex >= this.DataGridRowsLength))
                {
                    throw new ArgumentOutOfRangeException("rowIndex");
                }
                if ((columnIndex < 0) || (columnIndex >= this.myGridTable.GridColumnStyles.Count))
                {
                    throw new ArgumentOutOfRangeException("columnIndex");
                }
                CurrencyManager listManager = this.listManager;
                if (listManager.Position != rowIndex)
                {
                    listManager.Position = rowIndex;
                }
                this.myGridTable.GridColumnStyles[columnIndex].SetColumnValueAtRow(listManager, rowIndex, value);
                if (((columnIndex >= this.firstVisibleCol) && (columnIndex <= ((this.firstVisibleCol + this.numVisibleCols) - 1))) && ((rowIndex >= this.firstVisibleRow) && (rowIndex <= (this.firstVisibleRow + this.numVisibleRows))))
                {
                    Rectangle cellBounds = this.GetCellBounds(rowIndex, columnIndex);
                    base.Invalidate(cellBounds);
                }
            }
        }

        public object this[DataGridCell cell]
        {
            get
            {
                return this[cell.RowNumber, cell.ColumnNumber];
            }
            set
            {
                this[cell.RowNumber, cell.ColumnNumber] = value;
            }
        }

        internal bool LedgerStyle
        {
            get
            {
                return this.gridState[0x20];
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
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "LinkColor" }));
                }
                if (!this.linkBrush.Color.Equals(value))
                {
                    this.linkBrush = new SolidBrush(value);
                    base.Invalidate(this.layout.Data);
                }
            }
        }

        internal Font LinkFont
        {
            get
            {
                return this.linkFont;
            }
        }

        internal int LinkFontHeight
        {
            get
            {
                return this.linkFontHeight;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), System.Windows.Forms.SRCategory("CatColors"), System.Windows.Forms.SRDescription("DataGridLinkHoverColorDescr")]
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

        private bool ListHasErrors
        {
            get
            {
                return this.gridState[0x80];
            }
            set
            {
                if (this.ListHasErrors != value)
                {
                    this.gridState[0x80] = value;
                    this.ComputeMinimumRowHeaderWidth();
                    if (this.layout.RowHeadersVisible)
                    {
                        if (value)
                        {
                            if (this.myGridTable.IsDefault)
                            {
                                this.RowHeaderWidth += 15;
                            }
                            else
                            {
                                this.myGridTable.RowHeaderWidth += 15;
                            }
                        }
                        else if (this.myGridTable.IsDefault)
                        {
                            this.RowHeaderWidth -= 15;
                        }
                        else
                        {
                            this.myGridTable.RowHeaderWidth -= 15;
                        }
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRDescription("DataGridListManagerDescr"), Browsable(false)]
        protected internal CurrencyManager ListManager
        {
            get
            {
                if (((this.listManager == null) && (this.BindingContext != null)) && (this.DataSource != null))
                {
                    return (CurrencyManager) this.BindingContext[this.DataSource, this.DataMember];
                }
                return this.listManager;
            }
            set
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("DataGridSetListManager"));
            }
        }

        internal AccessibleObject ParentRowsAccessibleObject
        {
            get
            {
                return this.parentRows.AccessibleObject;
            }
        }

        internal SolidBrush ParentRowsBackBrush
        {
            get
            {
                return this.parentRows.BackBrush;
            }
        }

        [System.Windows.Forms.SRCategory("CatColors"), System.Windows.Forms.SRDescription("DataGridParentRowsBackColorDescr")]
        public Color ParentRowsBackColor
        {
            get
            {
                return this.parentRows.BackColor;
            }
            set
            {
                if (IsTransparentColor(value))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridTransparentParentRowsBackColorNotAllowed"));
                }
                this.parentRows.BackColor = value;
            }
        }

        internal Rectangle ParentRowsBounds
        {
            get
            {
                return this.layout.ParentRows;
            }
        }

        internal SolidBrush ParentRowsForeBrush
        {
            get
            {
                return this.parentRows.ForeBrush;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridParentRowsForeColorDescr"), System.Windows.Forms.SRCategory("CatColors")]
        public Color ParentRowsForeColor
        {
            get
            {
                return this.parentRows.ForeColor;
            }
            set
            {
                this.parentRows.ForeColor = value;
            }
        }

        [DefaultValue(3), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatDisplay"), System.Windows.Forms.SRDescription("DataGridParentRowsLabelStyleDescr")]
        public DataGridParentRowsLabelStyle ParentRowsLabelStyle
        {
            get
            {
                return this.parentRowsLabels;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DataGridParentRowsLabelStyle));
                }
                if (this.parentRowsLabels != value)
                {
                    this.parentRowsLabels = value;
                    base.Invalidate(this.layout.ParentRows);
                    this.OnParentRowsLabelStyleChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRDescription("DataGridParentRowsVisibleDescr"), System.Windows.Forms.SRCategory("CatDisplay"), DefaultValue(true)]
        public bool ParentRowsVisible
        {
            get
            {
                return this.layout.ParentRowsVisible;
            }
            set
            {
                if (this.layout.ParentRowsVisible != value)
                {
                    this.SetParentRowsVisibility(value);
                    this.caption.SetDownButtonDirection(!value);
                    this.OnParentRowsVisibleChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), TypeConverter(typeof(DataGridPreferredColumnWidthTypeConverter)), DefaultValue(0x4b), System.Windows.Forms.SRDescription("DataGridPreferredColumnWidthDescr")]
        public int PreferredColumnWidth
        {
            get
            {
                return this.preferredColumnWidth;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridColumnWidth"), "PreferredColumnWidth");
                }
                if (this.preferredColumnWidth != value)
                {
                    this.preferredColumnWidth = value;
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("DataGridPreferredRowHeightDescr")]
        public int PreferredRowHeight
        {
            get
            {
                return this.prefferedRowHeight;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridRowRowHeight"));
                }
                this.prefferedRowHeight = value;
            }
        }

        [System.Windows.Forms.SRDescription("DataGridReadOnlyDescr"), DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool ReadOnly
        {
            get
            {
                return this.gridState[0x1000];
            }
            set
            {
                if (this.ReadOnly != value)
                {
                    bool allowAdd = false;
                    if (value)
                    {
                        allowAdd = this.policy.AllowAdd;
                        this.policy.AllowRemove = false;
                        this.policy.AllowEdit = false;
                        this.policy.AllowAdd = false;
                    }
                    else
                    {
                        allowAdd |= this.policy.UpdatePolicy(this.listManager, value);
                    }
                    this.gridState[0x1000] = value;
                    DataGridRow[] dataGridRows = this.DataGridRows;
                    if (allowAdd)
                    {
                        this.RecreateDataGridRows();
                        DataGridRow[] rowArray2 = this.DataGridRows;
                        int num = Math.Min(rowArray2.Length, dataGridRows.Length);
                        for (int i = 0; i < num; i++)
                        {
                            if (dataGridRows[i].Selected)
                            {
                                rowArray2[i].Selected = true;
                            }
                        }
                    }
                    base.PerformLayout();
                    this.InvalidateInside();
                    this.OnReadOnlyChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatDisplay"), System.Windows.Forms.SRDescription("DataGridRowHeadersVisibleDescr"), DefaultValue(true)]
        public bool RowHeadersVisible
        {
            get
            {
                return this.gridState[4];
            }
            set
            {
                if (this.RowHeadersVisible != value)
                {
                    this.gridState[4] = value;
                    base.PerformLayout();
                    this.InvalidateInside();
                }
            }
        }

        [DefaultValue(0x23), System.Windows.Forms.SRDescription("DataGridRowHeaderWidthDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public int RowHeaderWidth
        {
            get
            {
                return this.rowHeaderWidth;
            }
            set
            {
                value = Math.Max(this.minRowHeaderWidth, value);
                if (this.rowHeaderWidth != value)
                {
                    this.rowHeaderWidth = value;
                    if (this.layout.RowHeadersVisible)
                    {
                        base.PerformLayout();
                        this.InvalidateInside();
                    }
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

        [System.Windows.Forms.SRDescription("DataGridSelectionBackColorDescr"), System.Windows.Forms.SRCategory("CatColors")]
        public Color SelectionBackColor
        {
            get
            {
                return this.selectionBackBrush.Color;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "SelectionBackColor" }));
                }
                if (IsTransparentColor(value))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridTransparentSelectionBackColorNotAllowed"));
                }
                if (!value.Equals(this.selectionBackBrush.Color))
                {
                    this.selectionBackBrush = new SolidBrush(value);
                    this.InvalidateInside();
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

        [System.Windows.Forms.SRCategory("CatColors"), System.Windows.Forms.SRDescription("DataGridSelectionForeColorDescr")]
        public Color SelectionForeColor
        {
            get
            {
                return this.selectionForeBrush.Color;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataGridEmptyColor", new object[] { "SelectionForeColor" }));
                }
                if (!value.Equals(this.selectionForeBrush.Color))
                {
                    this.selectionForeBrush = new SolidBrush(value);
                    this.InvalidateInside();
                }
            }
        }

        public override ISite Site
        {
            get
            {
                return base.Site;
            }
            set
            {
                ISite site = this.Site;
                base.Site = value;
                if ((value != site) && !base.Disposing)
                {
                    this.SubObjectsSiteChange(false);
                    this.SubObjectsSiteChange(true);
                }
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("DataGridGridTablesDescr"), System.Windows.Forms.SRCategory("CatData"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public GridTableStylesCollection TableStyles
        {
            get
            {
                return this.dataGridTables;
            }
        }

        [Browsable(false), Bindable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        internal int ToolTipId
        {
            get
            {
                return this.toolTipId;
            }
            set
            {
                this.toolTipId = value;
            }
        }

        internal DataGridToolTip ToolTipProvider
        {
            get
            {
                return this.toolTipProvider;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), System.Windows.Forms.SRDescription("DataGridVertScrollBarDescr")]
        protected ScrollBar VertScrollBar
        {
            get
            {
                return this.vertScrollBar;
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("DataGridVisibleColumnCountDescr")]
        public int VisibleColumnCount
        {
            get
            {
                return Math.Min(this.numVisibleCols, (this.myGridTable == null) ? 0 : this.myGridTable.GridColumnStyles.Count);
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("DataGridVisibleRowCountDescr")]
        public int VisibleRowCount
        {
            get
            {
                return this.numVisibleRows;
            }
        }

        [ComVisible(true)]
        internal class DataGridAccessibleObject : Control.ControlAccessibleObject
        {
            public DataGridAccessibleObject(System.Windows.Forms.DataGrid owner) : base(owner)
            {
            }

            public override AccessibleObject GetChild(int index)
            {
                System.Windows.Forms.DataGrid owner = (System.Windows.Forms.DataGrid) base.Owner;
                int columnCount = this.ColumnCount;
                int rowCount = this.RowCount;
                if (owner.dataGridRows == null)
                {
                    owner.CreateDataGridRows();
                }
                if (index < 1)
                {
                    return owner.ParentRowsAccessibleObject;
                }
                index--;
                if (index < columnCount)
                {
                    return owner.myGridTable.GridColumnStyles[index].HeaderAccessibleObject;
                }
                index -= columnCount;
                if (index < rowCount)
                {
                    return owner.dataGridRows[index].AccessibleObject;
                }
                index -= rowCount;
                if (owner.horizScrollBar.Visible)
                {
                    if (index == 0)
                    {
                        return owner.horizScrollBar.AccessibilityObject;
                    }
                    index--;
                }
                if (owner.vertScrollBar.Visible)
                {
                    if (index == 0)
                    {
                        return owner.vertScrollBar.AccessibilityObject;
                    }
                    index--;
                }
                int count = owner.myGridTable.GridColumnStyles.Count;
                DataGridRow[] dataGridRows = owner.dataGridRows;
                int num4 = index / count;
                int num5 = index % count;
                if ((num4 < owner.dataGridRows.Length) && (num5 < owner.myGridTable.GridColumnStyles.Count))
                {
                    return owner.dataGridRows[num4].AccessibleObject.GetChild(num5);
                }
                return null;
            }

            public override int GetChildCount()
            {
                int num = (1 + this.ColumnCount) + ((System.Windows.Forms.DataGrid) base.Owner).DataGridRowsLength;
                if (this.DataGrid.horizScrollBar.Visible)
                {
                    num++;
                }
                if (this.DataGrid.vertScrollBar.Visible)
                {
                    num++;
                }
                return (num + (this.DataGrid.DataGridRows.Length * this.DataGrid.myGridTable.GridColumnStyles.Count));
            }

            public override AccessibleObject GetFocused()
            {
                if (this.DataGrid.Focused)
                {
                    return this.GetSelected();
                }
                return null;
            }

            public override AccessibleObject GetSelected()
            {
                if ((this.DataGrid.DataGridRows.Length == 0) || (this.DataGrid.myGridTable.GridColumnStyles.Count == 0))
                {
                    return null;
                }
                DataGridCell currentCell = this.DataGrid.CurrentCell;
                AccessibleObject child = this.GetChild((1 + this.ColumnCount) + currentCell.RowNumber);
                return child.GetChild(currentCell.ColumnNumber);
            }

            public override AccessibleObject HitTest(int x, int y)
            {
                Point point = this.DataGrid.PointToClient(new Point(x, y));
                System.Windows.Forms.DataGrid.HitTestInfo info = this.DataGrid.HitTest(point.X, point.Y);
                System.Windows.Forms.DataGrid.HitTestType type = info.Type;
                if (type <= System.Windows.Forms.DataGrid.HitTestType.RowResize)
                {
                    switch (type)
                    {
                        case System.Windows.Forms.DataGrid.HitTestType.Cell:
                            return this.GetChild((1 + this.ColumnCount) + info.Row).GetChild(info.Column);

                        case System.Windows.Forms.DataGrid.HitTestType.ColumnHeader:
                            return this.GetChild(1 + info.Column);

                        case System.Windows.Forms.DataGrid.HitTestType.RowHeader:
                            return this.GetChild((1 + this.ColumnCount) + info.Row);
                    }
                }
                else if ((type != System.Windows.Forms.DataGrid.HitTestType.Caption) && (type == System.Windows.Forms.DataGrid.HitTestType.ParentRows))
                {
                    return this.DataGrid.ParentRowsAccessibleObject;
                }
                return null;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation navdir)
            {
                if (this.GetChildCount() > 0)
                {
                    switch (navdir)
                    {
                        case AccessibleNavigation.FirstChild:
                            return this.GetChild(0);

                        case AccessibleNavigation.LastChild:
                            return this.GetChild(this.GetChildCount() - 1);
                    }
                }
                return null;
            }

            private int ColumnCount
            {
                get
                {
                    return ((System.Windows.Forms.DataGrid) base.Owner).myGridTable.GridColumnStyles.Count;
                }
            }

            internal System.Windows.Forms.DataGrid DataGrid
            {
                get
                {
                    return (System.Windows.Forms.DataGrid) base.Owner;
                }
            }

            public override string Name
            {
                get
                {
                    string accessibleName = base.Owner.AccessibleName;
                    if (accessibleName != null)
                    {
                        return accessibleName;
                    }
                    return "DataGrid";
                }
                set
                {
                    base.Owner.AccessibleName = value;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    AccessibleRole accessibleRole = base.Owner.AccessibleRole;
                    if (accessibleRole != AccessibleRole.Default)
                    {
                        return accessibleRole;
                    }
                    return AccessibleRole.Table;
                }
            }

            private int RowCount
            {
                get
                {
                    return ((System.Windows.Forms.DataGrid) base.Owner).dataGridRows.Length;
                }
            }
        }

        public sealed class HitTestInfo
        {
            internal int col;
            public static readonly DataGrid.HitTestInfo Nowhere = new DataGrid.HitTestInfo();
            internal int row;
            internal DataGrid.HitTestType type;

            internal HitTestInfo()
            {
                this.type = DataGrid.HitTestType.None;
                this.row = this.col = -1;
            }

            internal HitTestInfo(DataGrid.HitTestType type)
            {
                this.type = type;
                this.row = this.col = -1;
            }

            public override bool Equals(object value)
            {
                if (!(value is DataGrid.HitTestInfo))
                {
                    return false;
                }
                DataGrid.HitTestInfo info = (DataGrid.HitTestInfo) value;
                return (((this.type == info.type) && (this.row == info.row)) && (this.col == info.col));
            }

            public override int GetHashCode()
            {
                return ((((int) this.type) + (this.row << 8)) + (this.col << 0x10));
            }

            public override string ToString()
            {
                return ("{ " + this.type.ToString() + "," + this.row.ToString(CultureInfo.InvariantCulture) + "," + this.col.ToString(CultureInfo.InvariantCulture) + "}");
            }

            public int Column
            {
                get
                {
                    return this.col;
                }
            }

            public int Row
            {
                get
                {
                    return this.row;
                }
            }

            public DataGrid.HitTestType Type
            {
                get
                {
                    return this.type;
                }
            }
        }

        [Flags]
        public enum HitTestType
        {
            Caption = 0x20,
            Cell = 1,
            ColumnHeader = 2,
            ColumnResize = 8,
            None = 0,
            ParentRows = 0x40,
            RowHeader = 4,
            RowResize = 0x10
        }

        internal class LayoutData
        {
            public Rectangle Caption;
            public bool CaptionVisible;
            public Rectangle ClientRectangle;
            public Rectangle ColumnHeaders;
            public bool ColumnHeadersVisible;
            public Rectangle Data;
            internal bool dirty;
            public Rectangle Inside;
            public Rectangle ParentRows;
            public bool ParentRowsVisible;
            public Rectangle ResizeBoxRect;
            public Rectangle RowHeaders;
            public bool RowHeadersVisible;
            public Rectangle TopLeftHeader;

            public LayoutData()
            {
                this.dirty = true;
                this.Inside = Rectangle.Empty;
                this.RowHeaders = Rectangle.Empty;
                this.TopLeftHeader = Rectangle.Empty;
                this.ColumnHeaders = Rectangle.Empty;
                this.Data = Rectangle.Empty;
                this.Caption = Rectangle.Empty;
                this.ParentRows = Rectangle.Empty;
                this.ResizeBoxRect = Rectangle.Empty;
                this.ClientRectangle = Rectangle.Empty;
            }

            public LayoutData(DataGrid.LayoutData src)
            {
                this.dirty = true;
                this.Inside = Rectangle.Empty;
                this.RowHeaders = Rectangle.Empty;
                this.TopLeftHeader = Rectangle.Empty;
                this.ColumnHeaders = Rectangle.Empty;
                this.Data = Rectangle.Empty;
                this.Caption = Rectangle.Empty;
                this.ParentRows = Rectangle.Empty;
                this.ResizeBoxRect = Rectangle.Empty;
                this.ClientRectangle = Rectangle.Empty;
                this.GrabLayout(src);
            }

            private void GrabLayout(DataGrid.LayoutData src)
            {
                this.Inside = src.Inside;
                this.TopLeftHeader = src.TopLeftHeader;
                this.ColumnHeaders = src.ColumnHeaders;
                this.RowHeaders = src.RowHeaders;
                this.Data = src.Data;
                this.Caption = src.Caption;
                this.ParentRows = src.ParentRows;
                this.ResizeBoxRect = src.ResizeBoxRect;
                this.ColumnHeadersVisible = src.ColumnHeadersVisible;
                this.RowHeadersVisible = src.RowHeadersVisible;
                this.CaptionVisible = src.CaptionVisible;
                this.ParentRowsVisible = src.ParentRowsVisible;
                this.ClientRectangle = src.ClientRectangle;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder(200);
                builder.Append(base.ToString());
                builder.Append(" { \n");
                builder.Append("Inside = ");
                builder.Append(this.Inside.ToString());
                builder.Append('\n');
                builder.Append("TopLeftHeader = ");
                builder.Append(this.TopLeftHeader.ToString());
                builder.Append('\n');
                builder.Append("ColumnHeaders = ");
                builder.Append(this.ColumnHeaders.ToString());
                builder.Append('\n');
                builder.Append("RowHeaders = ");
                builder.Append(this.RowHeaders.ToString());
                builder.Append('\n');
                builder.Append("Data = ");
                builder.Append(this.Data.ToString());
                builder.Append('\n');
                builder.Append("Caption = ");
                builder.Append(this.Caption.ToString());
                builder.Append('\n');
                builder.Append("ParentRows = ");
                builder.Append(this.ParentRows.ToString());
                builder.Append('\n');
                builder.Append("ResizeBoxRect = ");
                builder.Append(this.ResizeBoxRect.ToString());
                builder.Append('\n');
                builder.Append("ColumnHeadersVisible = ");
                builder.Append(this.ColumnHeadersVisible.ToString());
                builder.Append('\n');
                builder.Append("RowHeadersVisible = ");
                builder.Append(this.RowHeadersVisible.ToString());
                builder.Append('\n');
                builder.Append("CaptionVisible = ");
                builder.Append(this.CaptionVisible.ToString());
                builder.Append('\n');
                builder.Append("ParentRowsVisible = ");
                builder.Append(this.ParentRowsVisible.ToString());
                builder.Append('\n');
                builder.Append("ClientRectangle = ");
                builder.Append(this.ClientRectangle.ToString());
                builder.Append(" } ");
                return builder.ToString();
            }
        }

        private class Policy
        {
            private bool allowAdd = true;
            private bool allowEdit = true;
            private bool allowRemove = true;

            public bool UpdatePolicy(CurrencyManager listManager, bool gridReadOnly)
            {
                bool flag = false;
                IBindingList list = (listManager == null) ? null : (listManager.List as IBindingList);
                if (listManager == null)
                {
                    if (!this.allowAdd)
                    {
                        flag = true;
                    }
                    this.allowAdd = this.allowEdit = this.allowRemove = true;
                    return flag;
                }
                if ((this.AllowAdd != listManager.AllowAdd) && !gridReadOnly)
                {
                    flag = true;
                }
                this.AllowAdd = ((listManager.AllowAdd && !gridReadOnly) && (list != null)) && list.SupportsChangeNotification;
                this.AllowEdit = listManager.AllowEdit && !gridReadOnly;
                this.AllowRemove = ((listManager.AllowRemove && !gridReadOnly) && (list != null)) && list.SupportsChangeNotification;
                return flag;
            }

            public bool AllowAdd
            {
                get
                {
                    return this.allowAdd;
                }
                set
                {
                    if (this.allowAdd != value)
                    {
                        this.allowAdd = value;
                    }
                }
            }

            public bool AllowEdit
            {
                get
                {
                    return this.allowEdit;
                }
                set
                {
                    if (this.allowEdit != value)
                    {
                        this.allowEdit = value;
                    }
                }
            }

            public bool AllowRemove
            {
                get
                {
                    return this.allowRemove;
                }
                set
                {
                    if (this.allowRemove != value)
                    {
                        this.allowRemove = value;
                    }
                }
            }
        }
    }
}

