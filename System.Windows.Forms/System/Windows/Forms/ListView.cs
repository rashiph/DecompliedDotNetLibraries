namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Forms.Layout;
    using System.Windows.Forms.VisualStyles;

    [ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), Docking(DockingBehavior.Ask), DefaultEvent("SelectedIndexChanged"), System.Windows.Forms.SRDescription("DescriptionListView"), Designer("System.Windows.Forms.Design.ListViewDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("Items")]
    public class ListView : Control
    {
        private ItemActivation activation;
        private ListViewAlignment alignStyle = ListViewAlignment.Top;
        private string backgroundImageFileName = string.Empty;
        private const int BKIMGARRAYSIZE = 8;
        private string[] bkImgFileNames;
        private int bkImgFileNamesCount = -1;
        private System.Windows.Forms.BorderStyle borderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        private CheckedIndexCollection checkedIndexCollection;
        private CheckedListViewItemCollection checkedListViewItemCollection;
        private ColumnHeader columnHeaderClicked;
        private int columnHeaderClickedWidth;
        private ColumnHeaderCollection columnHeaderCollection;
        private ColumnHeader[] columnHeaders;
        private int columnIndex;
        private ListViewGroup defaultGroup;
        private MouseButtons downButton;
        private static readonly object EVENT_CACHEVIRTUALITEMS = new object();
        private static readonly object EVENT_COLUMNREORDERED = new object();
        private static readonly object EVENT_COLUMNWIDTHCHANGED = new object();
        private static readonly object EVENT_COLUMNWIDTHCHANGING = new object();
        private static readonly object EVENT_DRAWCOLUMNHEADER = new object();
        private static readonly object EVENT_DRAWITEM = new object();
        private static readonly object EVENT_DRAWSUBITEM = new object();
        private static readonly object EVENT_ITEMSELECTIONCHANGED = new object();
        private static readonly object EVENT_RETRIEVEVIRTUALITEM = new object();
        private static readonly object EVENT_RIGHTTOLEFTLAYOUTCHANGED = new object();
        private static readonly object EVENT_SEARCHFORVIRTUALITEM = new object();
        private static readonly object EVENT_SELECTEDINDEXCHANGED = new object();
        private static readonly object EVENT_VIRTUALITEMSSELECTIONRANGECHANGED = new object();
        private ListViewGroupCollection groups;
        private ColumnHeaderStyle headerStyle = ColumnHeaderStyle.Clickable;
        private bool hoveredAlready;
        private ImageList imageListLarge;
        private ImageList imageListSmall;
        private ImageList imageListState;
        private ListViewInsertionMark insertionMark;
        private int itemCount;
        private ListViewItemCollection listItemCollection;
        private ArrayList listItemsArray = new ArrayList();
        private IComparer listItemSorter;
        private Hashtable listItemsTable = new Hashtable();
        private BitVector32 listViewState = new BitVector32(0x800e44);
        private const int LISTVIEWSTATE_allowColumnReorder = 2;
        private const int LISTVIEWSTATE_autoArrange = 4;
        private const int LISTVIEWSTATE_backgroundImageTiled = 0x10000;
        private const int LISTVIEWSTATE_checkBoxes = 8;
        private const int LISTVIEWSTATE_columnClicked = 0x20000;
        private const int LISTVIEWSTATE_columnResizeCancelled = 0x40000000;
        private const int LISTVIEWSTATE_comctlSupportsVisualStyles = 0x200000;
        private const int LISTVIEWSTATE_comctlSupportsVisualStylesTested = 0x400000;
        private const int LISTVIEWSTATE_doubleclickFired = 0x40000;
        private const int LISTVIEWSTATE_expectingMouseUp = 0x100000;
        private const int LISTVIEWSTATE_flipViewToLargeIconAndSmallIcon = 0x10000000;
        private const int LISTVIEWSTATE_fullRowSelect = 0x10;
        private const int LISTVIEWSTATE_gridLines = 0x20;
        private const int LISTVIEWSTATE_handleDestroyed = 0x1000000;
        private const int LISTVIEWSTATE_headerControlTracking = 0x4000000;
        private const int LISTVIEWSTATE_headerDividerDblClick = 0x20000000;
        private const int LISTVIEWSTATE_hideSelection = 0x40;
        private const int LISTVIEWSTATE_hotTracking = 0x80;
        private const int LISTVIEWSTATE_hoverSelection = 0x1000;
        private const int LISTVIEWSTATE_inLabelEdit = 0x4000;
        private const int LISTVIEWSTATE_itemCollectionChangedInMouseDown = 0x8000000;
        private const int LISTVIEWSTATE_labelEdit = 0x100;
        private const int LISTVIEWSTATE_labelWrap = 0x200;
        private const int LISTVIEWSTATE_mouseUpFired = 0x80000;
        private const int LISTVIEWSTATE_multiSelect = 0x400;
        private const int LISTVIEWSTATE_nonclickHdr = 0x2000;
        private const int LISTVIEWSTATE_ownerDraw = 1;
        private const int LISTVIEWSTATE_scrollable = 0x800;
        private const int LISTVIEWSTATE_showGroups = 0x800000;
        private const int LISTVIEWSTATE_showItemToolTips = 0x8000;
        private const int LISTVIEWSTATE_virtualMode = 0x2000000;
        private BitVector32 listViewState1 = new BitVector32(8);
        private const int LISTVIEWSTATE1_cancelledColumnWidthChanging = 2;
        private const int LISTVIEWSTATE1_disposingImageLists = 4;
        private const int LISTVIEWSTATE1_insertingItemsNatively = 1;
        private const int LISTVIEWSTATE1_selectedIndexChangedSkipped = 0x10;
        private const int LISTVIEWSTATE1_useCompatibleStateImageBehavior = 8;
        private const int LVTOOLTIPTRACKING = 0x30;
        private const int MASK_HITTESTFLAG = 0xf7;
        private const int MAXTILECOLUMNS = 20;
        private int newWidthForColumnWidthChangingCancelled = -1;
        private int nextID;
        private Color odCacheBackColor = SystemColors.Window;
        private Font odCacheFont;
        private IntPtr odCacheFontHandle = IntPtr.Zero;
        private Control.FontHandleWrapper odCacheFontHandleWrapper;
        private Color odCacheForeColor = SystemColors.WindowText;
        private ListViewItem prevHoveredItem;
        private static readonly int PropDelayedUpdateItems = PropertyStore.CreateKey();
        private bool rightToLeftLayout;
        private List<ListViewItem> savedCheckedItems;
        private List<ListViewItem> savedSelectedItems;
        private SelectedIndexCollection selectedIndexCollection;
        private SelectedListViewItemCollection selectedListViewItemCollection;
        private SortOrder sorting;
        private Size tileSize = Size.Empty;
        private string toolTipCaption = string.Empty;
        private int topIndex;
        private int updateCounter;
        private System.Windows.Forms.View viewStyle;
        private int virtualListSize;

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ListViewAfterLabelEditDescr")]
        public event LabelEditEventHandler AfterLabelEdit;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [System.Windows.Forms.SRDescription("ListViewBeforeLabelEditDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event LabelEditEventHandler BeforeLabelEdit;

        [System.Windows.Forms.SRDescription("ListViewCacheVirtualItemsEventDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event CacheVirtualItemsEventHandler CacheVirtualItems
        {
            add
            {
                base.Events.AddHandler(EVENT_CACHEVIRTUALITEMS, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_CACHEVIRTUALITEMS, value);
            }
        }

        [System.Windows.Forms.SRDescription("ListViewColumnClickDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event ColumnClickEventHandler ColumnClick;

        [System.Windows.Forms.SRDescription("ListViewColumnReorderedDscr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event ColumnReorderedEventHandler ColumnReordered
        {
            add
            {
                base.Events.AddHandler(EVENT_COLUMNREORDERED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_COLUMNREORDERED, value);
            }
        }

        [System.Windows.Forms.SRDescription("ListViewColumnWidthChangedDscr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event ColumnWidthChangedEventHandler ColumnWidthChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_COLUMNWIDTHCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_COLUMNWIDTHCHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ListViewColumnWidthChangingDscr")]
        public event ColumnWidthChangingEventHandler ColumnWidthChanging
        {
            add
            {
                base.Events.AddHandler(EVENT_COLUMNWIDTHCHANGING, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_COLUMNWIDTHCHANGING, value);
            }
        }

        [System.Windows.Forms.SRDescription("ListViewDrawColumnHeaderEventDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event DrawListViewColumnHeaderEventHandler DrawColumnHeader
        {
            add
            {
                base.Events.AddHandler(EVENT_DRAWCOLUMNHEADER, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DRAWCOLUMNHEADER, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ListViewDrawItemEventDescr")]
        public event DrawListViewItemEventHandler DrawItem
        {
            add
            {
                base.Events.AddHandler(EVENT_DRAWITEM, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DRAWITEM, value);
            }
        }

        [System.Windows.Forms.SRDescription("ListViewDrawSubItemEventDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event DrawListViewSubItemEventHandler DrawSubItem
        {
            add
            {
                base.Events.AddHandler(EVENT_DRAWSUBITEM, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DRAWSUBITEM, value);
            }
        }

        [System.Windows.Forms.SRDescription("ListViewItemClickDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event EventHandler ItemActivate;

        [System.Windows.Forms.SRDescription("CheckedListBoxItemCheckDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event ItemCheckEventHandler ItemCheck;

        [System.Windows.Forms.SRDescription("ListViewItemCheckedDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event ItemCheckedEventHandler ItemChecked;

        [System.Windows.Forms.SRDescription("ListViewItemDragDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event ItemDragEventHandler ItemDrag;

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("ListViewItemMouseHoverDescr")]
        public event ListViewItemMouseHoverEventHandler ItemMouseHover;

        [System.Windows.Forms.SRDescription("ListViewItemSelectionChangedDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event ListViewItemSelectionChangedEventHandler ItemSelectionChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_ITEMSELECTIONCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_ITEMSELECTIONCHANGED, value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler PaddingChanged
        {
            add
            {
                base.PaddingChanged += value;
            }
            remove
            {
                base.PaddingChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event PaintEventHandler Paint
        {
            add
            {
                base.Paint += value;
            }
            remove
            {
                base.Paint -= value;
            }
        }

        [System.Windows.Forms.SRDescription("ListViewRetrieveVirtualItemEventDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event RetrieveVirtualItemEventHandler RetrieveVirtualItem
        {
            add
            {
                base.Events.AddHandler(EVENT_RETRIEVEVIRTUALITEM, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_RETRIEVEVIRTUALITEM, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnRightToLeftLayoutChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler RightToLeftLayoutChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_RIGHTTOLEFTLAYOUTCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_RIGHTTOLEFTLAYOUTCHANGED, value);
            }
        }

        [System.Windows.Forms.SRDescription("ListViewSearchForVirtualItemDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event SearchForVirtualItemEventHandler SearchForVirtualItem
        {
            add
            {
                base.Events.AddHandler(EVENT_SEARCHFORVIRTUALITEM, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_SEARCHFORVIRTUALITEM, value);
            }
        }

        [System.Windows.Forms.SRDescription("ListViewSelectedIndexChangedDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler SelectedIndexChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_SELECTEDINDEXCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_SELECTEDINDEXCHANGED, value);
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

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ListViewVirtualItemsSelectionRangeChangedDescr")]
        public event ListViewVirtualItemsSelectionRangeChangedEventHandler VirtualItemsSelectionRangeChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_VIRTUALITEMSSELECTIONRANGECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_VIRTUALITEMSSELECTIONRANGECHANGED, value);
            }
        }

        public ListView()
        {
            base.SetStyle(ControlStyles.UserPaint, false);
            base.SetStyle(ControlStyles.StandardClick, false);
            base.SetStyle(ControlStyles.UseTextForAccessibility, false);
            this.odCacheFont = this.Font;
            this.odCacheFontHandle = base.FontHandle;
            base.SetBounds(0, 0, 0x79, 0x61);
            this.listItemCollection = new ListViewItemCollection(new ListViewNativeItemCollection(this));
            this.columnHeaderCollection = new ColumnHeaderCollection(this);
        }

        private void ApplyUpdateCachedItems()
        {
            ArrayList list = (ArrayList) base.Properties.GetObject(PropDelayedUpdateItems);
            if (list != null)
            {
                base.Properties.SetObject(PropDelayedUpdateItems, null);
                ListViewItem[] items = (ListViewItem[]) list.ToArray(typeof(ListViewItem));
                if (items.Length > 0)
                {
                    this.InsertItems(this.itemCount, items, false);
                }
            }
        }

        public void ArrangeIcons()
        {
            this.ArrangeIcons(ListViewAlignment.Default);
        }

        public void ArrangeIcons(ListViewAlignment value)
        {
            if (this.viewStyle == System.Windows.Forms.View.SmallIcon)
            {
                switch (((int) value))
                {
                    case 0:
                    case 1:
                    case 2:
                    case 5:
                        if (base.IsHandleCreated)
                        {
                            System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, base.Handle), 0x1016, (int) value, 0);
                        }
                        if (!this.VirtualMode && (this.sorting != SortOrder.None))
                        {
                            this.Sort();
                        }
                        return;
                }
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "value", value.ToString() }));
            }
        }

        public void AutoResizeColumn(int columnIndex, ColumnHeaderAutoResizeStyle headerAutoResize)
        {
            if (!base.IsHandleCreated)
            {
                this.CreateHandle();
            }
            this.SetColumnWidth(columnIndex, headerAutoResize);
        }

        public void AutoResizeColumns(ColumnHeaderAutoResizeStyle headerAutoResize)
        {
            if (!base.IsHandleCreated)
            {
                this.CreateHandle();
            }
            this.UpdateColumnWidths(headerAutoResize);
        }

        public void BeginUpdate()
        {
            base.BeginUpdateInternal();
            if ((this.updateCounter++ == 0) && (base.Properties.GetObject(PropDelayedUpdateItems) == null))
            {
                base.Properties.SetObject(PropDelayedUpdateItems, new ArrayList());
            }
        }

        internal void CacheSelectedStateForItem(ListViewItem lvi, bool selected)
        {
            if (selected)
            {
                if (this.savedSelectedItems == null)
                {
                    this.savedSelectedItems = new List<ListViewItem>();
                }
                if (!this.savedSelectedItems.Contains(lvi))
                {
                    this.savedSelectedItems.Add(lvi);
                }
            }
            else if ((this.savedSelectedItems != null) && this.savedSelectedItems.Contains(lvi))
            {
                this.savedSelectedItems.Remove(lvi);
            }
        }

        private void CleanPreviousBackgroundImageFiles()
        {
            if (this.bkImgFileNames != null)
            {
                new FileIOPermission(PermissionState.Unrestricted).Assert();
                try
                {
                    for (int i = 0; i <= this.bkImgFileNamesCount; i++)
                    {
                        FileInfo info = new FileInfo(this.bkImgFileNames[i]);
                        if (info.Exists)
                        {
                            try
                            {
                                info.Delete();
                            }
                            catch (IOException)
                            {
                            }
                        }
                    }
                }
                finally
                {
                    PermissionSet.RevertAssert();
                }
                this.bkImgFileNames = null;
                this.bkImgFileNamesCount = -1;
            }
        }

        public void Clear()
        {
            this.Items.Clear();
            this.Columns.Clear();
        }

        private int CompareFunc(IntPtr lparam1, IntPtr lparam2, IntPtr lparamSort)
        {
            if (this.listItemSorter != null)
            {
                return this.listItemSorter.Compare(this.listItemsTable[(int) lparam1], this.listItemsTable[(int) lparam2]);
            }
            return 0;
        }

        private int CompensateColumnHeaderResize(int columnIndex, bool columnResizeCancelled)
        {
            if ((((this.ComctlSupportsVisualStyles && (this.View == System.Windows.Forms.View.Details)) && (!columnResizeCancelled && (this.Items.Count > 0))) && (columnIndex == 0)) && ((((this.columnHeaders != null) && (this.columnHeaders.Length > 0)) ? this.columnHeaders[0] : null) != null))
            {
                if (this.SmallImageList == null)
                {
                    return 2;
                }
                bool flag = true;
                for (int i = 0; i < this.Items.Count; i++)
                {
                    if (this.Items[i].ImageIndexer.ActualIndex > -1)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    return 0x12;
                }
            }
            return 0;
        }

        private int CompensateColumnHeaderResize(Message m, bool columnResizeCancelled)
        {
            if ((this.ComctlSupportsVisualStyles && (this.View == System.Windows.Forms.View.Details)) && (!columnResizeCancelled && (this.Items.Count > 0)))
            {
                System.Windows.Forms.NativeMethods.NMHEADER lParam = (System.Windows.Forms.NativeMethods.NMHEADER) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHEADER));
                return this.CompensateColumnHeaderResize(lParam.iItem, columnResizeCancelled);
            }
            return 0;
        }

        protected override void CreateHandle()
        {
            if (!base.RecreatingHandle)
            {
                IntPtr userCookie = System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Activate();
                try
                {
                    System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX icc = new System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX {
                        dwICC = 1
                    };
                    System.Windows.Forms.SafeNativeMethods.InitCommonControlsEx(icc);
                }
                finally
                {
                    System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Deactivate(userCookie);
                }
            }
            base.CreateHandle();
            if (this.BackgroundImage != null)
            {
                this.SetBackgroundImage();
            }
        }

        private unsafe void CustomDraw(ref Message m)
        {
            bool flag = false;
            bool drawDefault = false;
            try
            {
                int dwItemSpec;
                Rectangle itemRectOrEmpty;
                System.Windows.Forms.NativeMethods.NMLVCUSTOMDRAW* lParam = (System.Windows.Forms.NativeMethods.NMLVCUSTOMDRAW*) m.LParam;
                switch (lParam->nmcd.dwDrawStage)
                {
                    case 1:
                        if (this.OwnerDraw)
                        {
                            m.Result = (IntPtr) 0x20;
                        }
                        else
                        {
                            m.Result = (IntPtr) 0x22;
                            this.odCacheBackColor = this.BackColor;
                            this.odCacheForeColor = this.ForeColor;
                            this.odCacheFont = this.Font;
                            this.odCacheFontHandle = base.FontHandle;
                            if (lParam->dwItemType == 1)
                            {
                                if (this.odCacheFontHandleWrapper != null)
                                {
                                    this.odCacheFontHandleWrapper.Dispose();
                                }
                                this.odCacheFont = new Font(this.odCacheFont, FontStyle.Bold);
                                this.odCacheFontHandleWrapper = new Control.FontHandleWrapper(this.odCacheFont);
                                this.odCacheFontHandle = this.odCacheFontHandleWrapper.Handle;
                                System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(lParam->nmcd, lParam->nmcd.hdc), new HandleRef(this.odCacheFontHandleWrapper, this.odCacheFontHandleWrapper.Handle));
                                m.Result = (IntPtr) 2;
                            }
                        }
                        return;

                    case 0x10001:
                        dwItemSpec = (int) lParam->nmcd.dwItemSpec;
                        itemRectOrEmpty = this.GetItemRectOrEmpty(dwItemSpec);
                        if (!base.ClientRectangle.IntersectsWith(itemRectOrEmpty))
                        {
                            return;
                        }
                        if (this.OwnerDraw)
                        {
                            Graphics graphics = Graphics.FromHdcInternal(lParam->nmcd.hdc);
                            DrawListViewItemEventArgs e = null;
                            try
                            {
                                e = new DrawListViewItemEventArgs(graphics, this.Items[(int) lParam->nmcd.dwItemSpec], itemRectOrEmpty, (int) lParam->nmcd.dwItemSpec, (ListViewItemStates) lParam->nmcd.uItemState);
                                this.OnDrawItem(e);
                            }
                            finally
                            {
                                graphics.Dispose();
                            }
                            drawDefault = e.DrawDefault;
                            if (this.viewStyle == System.Windows.Forms.View.Details)
                            {
                                m.Result = (IntPtr) 0x20;
                            }
                            else if (!e.DrawDefault)
                            {
                                m.Result = (IntPtr) 4;
                            }
                            if (!e.DrawDefault)
                            {
                                return;
                            }
                        }
                        if ((this.viewStyle == System.Windows.Forms.View.Details) || (this.viewStyle == System.Windows.Forms.View.Tile))
                        {
                            m.Result = (IntPtr) 0x22;
                            flag = true;
                        }
                        break;

                    case 0x30001:
                        break;

                    default:
                        goto Label_06C1;
                }
                dwItemSpec = (int) lParam->nmcd.dwItemSpec;
                itemRectOrEmpty = this.GetItemRectOrEmpty(dwItemSpec);
                if (base.ClientRectangle.IntersectsWith(itemRectOrEmpty))
                {
                    if (this.OwnerDraw && !drawDefault)
                    {
                        Graphics graphics2 = Graphics.FromHdcInternal(lParam->nmcd.hdc);
                        DrawListViewSubItemEventArgs args2 = null;
                        bool flag3 = true;
                        try
                        {
                            if (lParam->iSubItem < this.Items[dwItemSpec].SubItems.Count)
                            {
                                Rectangle subItemRect = this.GetSubItemRect(dwItemSpec, lParam->iSubItem);
                                if ((lParam->iSubItem == 0) && (this.Items[dwItemSpec].SubItems.Count > 1))
                                {
                                    subItemRect.Width = this.columnHeaders[0].Width;
                                }
                                if (base.ClientRectangle.IntersectsWith(subItemRect))
                                {
                                    args2 = new DrawListViewSubItemEventArgs(graphics2, subItemRect, this.Items[dwItemSpec], this.Items[dwItemSpec].SubItems[lParam->iSubItem], dwItemSpec, lParam->iSubItem, this.columnHeaders[lParam->iSubItem], (ListViewItemStates) lParam->nmcd.uItemState);
                                    this.OnDrawSubItem(args2);
                                    flag3 = !args2.DrawDefault;
                                }
                            }
                        }
                        finally
                        {
                            graphics2.Dispose();
                        }
                        if (flag3)
                        {
                            m.Result = (IntPtr) 4;
                            return;
                        }
                    }
                    ListViewItem item = this.Items[(int) lParam->nmcd.dwItemSpec];
                    if (flag && item.UseItemStyleForSubItems)
                    {
                        m.Result = (IntPtr) 2;
                    }
                    int uItemState = lParam->nmcd.uItemState;
                    if (!this.HideSelection && ((this.GetItemState((int) lParam->nmcd.dwItemSpec) & 2) == 0))
                    {
                        uItemState &= -2;
                    }
                    int num4 = ((lParam->nmcd.dwDrawStage & 0x20000) != 0) ? lParam->iSubItem : 0;
                    Font font = null;
                    Color empty = Color.Empty;
                    Color backColor = Color.Empty;
                    bool flag4 = false;
                    bool flag5 = false;
                    if ((item != null) && (num4 < item.SubItems.Count))
                    {
                        flag4 = true;
                        if (((num4 == 0) && ((uItemState & 0x40) != 0)) && this.HotTracking)
                        {
                            flag5 = true;
                            font = new Font(item.SubItems[0].Font, FontStyle.Underline);
                        }
                        else
                        {
                            font = item.SubItems[num4].Font;
                        }
                        if ((num4 > 0) || ((uItemState & 0x47) == 0))
                        {
                            empty = item.SubItems[num4].ForeColor;
                            backColor = item.SubItems[num4].BackColor;
                        }
                    }
                    Color c = Color.Empty;
                    Color color4 = Color.Empty;
                    if (flag4)
                    {
                        c = empty;
                        color4 = backColor;
                    }
                    bool flag6 = true;
                    if (!base.Enabled)
                    {
                        flag6 = false;
                    }
                    else if (((this.activation == ItemActivation.OneClick) || (this.activation == ItemActivation.TwoClick)) && ((uItemState & 0x47) != 0))
                    {
                        flag6 = false;
                    }
                    if (flag6)
                    {
                        if (!flag4 || c.IsEmpty)
                        {
                            lParam->clrText = ColorTranslator.ToWin32(this.odCacheForeColor);
                        }
                        else
                        {
                            lParam->clrText = ColorTranslator.ToWin32(c);
                        }
                        if (lParam->clrText == ColorTranslator.ToWin32(SystemColors.HotTrack))
                        {
                            int num5 = 0;
                            bool flag7 = false;
                            int num6 = 0xff0000;
                            do
                            {
                                int num7 = lParam->clrText & num6;
                                if ((num7 != 0) || (num6 == 0xff))
                                {
                                    int num8 = 0x10 - num5;
                                    if (num7 == num6)
                                    {
                                        num7 = ((num7 >> (num8 & 0x1f)) - 1) << num8;
                                    }
                                    else
                                    {
                                        num7 = ((num7 >> (num8 & 0x1f)) + 1) << num8;
                                    }
                                    lParam->clrText = (lParam->clrText & ~num6) | num7;
                                    flag7 = true;
                                }
                                else
                                {
                                    num6 = num6 >> 8;
                                    num5 += 8;
                                }
                            }
                            while (!flag7);
                        }
                        if (!flag4 || color4.IsEmpty)
                        {
                            lParam->clrTextBk = ColorTranslator.ToWin32(this.odCacheBackColor);
                        }
                        else
                        {
                            lParam->clrTextBk = ColorTranslator.ToWin32(color4);
                        }
                    }
                    if (!flag4 || (font == null))
                    {
                        if (this.odCacheFont != null)
                        {
                            System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(lParam->nmcd, lParam->nmcd.hdc), new HandleRef(null, this.odCacheFontHandle));
                        }
                    }
                    else
                    {
                        if (this.odCacheFontHandleWrapper != null)
                        {
                            this.odCacheFontHandleWrapper.Dispose();
                        }
                        this.odCacheFontHandleWrapper = new Control.FontHandleWrapper(font);
                        System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(lParam->nmcd, lParam->nmcd.hdc), new HandleRef(this.odCacheFontHandleWrapper, this.odCacheFontHandleWrapper.Handle));
                    }
                    if (!flag)
                    {
                        m.Result = (IntPtr) 2;
                    }
                    if (flag5)
                    {
                        font.Dispose();
                    }
                }
                return;
            Label_06C1:
                m.Result = IntPtr.Zero;
            }
            catch (Exception)
            {
                m.Result = IntPtr.Zero;
            }
        }

        private void DeleteFileName(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                new FileIOPermission(PermissionState.Unrestricted).Assert();
                try
                {
                    FileInfo info = new FileInfo(fileName);
                    if (info.Exists)
                    {
                        try
                        {
                            info.Delete();
                        }
                        catch (IOException)
                        {
                        }
                    }
                }
                finally
                {
                    PermissionSet.RevertAssert();
                }
            }
        }

        private void DestroyLVGROUP(System.Windows.Forms.NativeMethods.LVGROUP lvgroup)
        {
            if (lvgroup.pszHeader != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(lvgroup.pszHeader);
            }
        }

        private void DetachImageList(object sender, EventArgs e)
        {
            this.listViewState1[4] = true;
            try
            {
                if (sender == this.imageListSmall)
                {
                    this.SmallImageList = null;
                }
                if (sender == this.imageListLarge)
                {
                    this.LargeImageList = null;
                }
                if (sender == this.imageListState)
                {
                    this.StateImageList = null;
                }
            }
            finally
            {
                this.listViewState1[4] = false;
            }
            this.UpdateListViewItemsLocations();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.imageListSmall != null)
                {
                    this.imageListSmall.Disposed -= new EventHandler(this.DetachImageList);
                    this.imageListSmall = null;
                }
                if (this.imageListLarge != null)
                {
                    this.imageListLarge.Disposed -= new EventHandler(this.DetachImageList);
                    this.imageListLarge = null;
                }
                if (this.imageListState != null)
                {
                    this.imageListState.Disposed -= new EventHandler(this.DetachImageList);
                    this.imageListState = null;
                }
                if (this.columnHeaders != null)
                {
                    for (int i = this.columnHeaders.Length - 1; i >= 0; i--)
                    {
                        this.columnHeaders[i].OwnerListview = null;
                        this.columnHeaders[i].Dispose();
                    }
                    this.columnHeaders = null;
                }
                this.Items.Clear();
                if (this.odCacheFontHandleWrapper != null)
                {
                    this.odCacheFontHandleWrapper.Dispose();
                    this.odCacheFontHandleWrapper = null;
                }
                if (!string.IsNullOrEmpty(this.backgroundImageFileName) || (this.bkImgFileNames != null))
                {
                    new FileIOPermission(PermissionState.Unrestricted).Assert();
                    try
                    {
                        FileInfo info;
                        if (!string.IsNullOrEmpty(this.backgroundImageFileName))
                        {
                            info = new FileInfo(this.backgroundImageFileName);
                            try
                            {
                                info.Delete();
                            }
                            catch (IOException)
                            {
                            }
                            this.backgroundImageFileName = string.Empty;
                        }
                        for (int j = 0; j <= this.bkImgFileNamesCount; j++)
                        {
                            info = new FileInfo(this.bkImgFileNames[j]);
                            try
                            {
                                info.Delete();
                            }
                            catch (IOException)
                            {
                            }
                        }
                        this.bkImgFileNames = null;
                        this.bkImgFileNamesCount = -1;
                    }
                    finally
                    {
                        PermissionSet.RevertAssert();
                    }
                }
            }
            base.Dispose(disposing);
        }

        public void EndUpdate()
        {
            if ((--this.updateCounter == 0) && (base.Properties.GetObject(PropDelayedUpdateItems) != null))
            {
                this.ApplyUpdateCachedItems();
            }
            base.EndUpdateInternal();
        }

        private void EnsureDefaultGroup()
        {
            if ((base.IsHandleCreated && this.ComctlSupportsVisualStyles) && (this.GroupsEnabled && (base.SendMessage(0x10a1, this.DefaultGroup.ID, 0) == IntPtr.Zero)))
            {
                this.UpdateGroupView();
                this.InsertGroupNative(0, this.DefaultGroup);
            }
        }

        public void EnsureVisible(int index)
        {
            if ((index < 0) || (index >= this.Items.Count))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1013, index, 0);
            }
        }

        private ListViewItem FindItem(bool isTextSearch, string text, bool isPrefixSearch, Point pt, SearchDirectionHint dir, int startIndex, bool includeSubItemsInSearch)
        {
            if (this.Items.Count != 0)
            {
                if (!base.IsHandleCreated)
                {
                    this.CreateHandle();
                }
                if (this.VirtualMode)
                {
                    SearchForVirtualItemEventArgs e = new SearchForVirtualItemEventArgs(isTextSearch, isPrefixSearch, includeSubItemsInSearch, text, pt, dir, startIndex);
                    this.OnSearchForVirtualItem(e);
                    if (e.Index != -1)
                    {
                        return this.Items[e.Index];
                    }
                    return null;
                }
                System.Windows.Forms.NativeMethods.LVFINDINFO lParam = new System.Windows.Forms.NativeMethods.LVFINDINFO();
                if (isTextSearch)
                {
                    lParam.flags = 2;
                    lParam.flags |= isPrefixSearch ? 8 : 0;
                    lParam.psz = text;
                }
                else
                {
                    lParam.flags = 0x40;
                    lParam.ptX = pt.X;
                    lParam.ptY = pt.Y;
                    lParam.vkDirection = (int) dir;
                }
                lParam.lParam = IntPtr.Zero;
                int num = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.LVM_FINDITEM, startIndex - 1, ref lParam);
                if (num >= 0)
                {
                    return this.Items[num];
                }
                if (isTextSearch && includeSubItemsInSearch)
                {
                    for (int i = startIndex; i < this.Items.Count; i++)
                    {
                        ListViewItem item = this.Items[i];
                        for (int j = 0; j < item.SubItems.Count; j++)
                        {
                            ListViewItem.ListViewSubItem item2 = item.SubItems[j];
                            if (string.Equals(text, item2.Text, StringComparison.OrdinalIgnoreCase))
                            {
                                return item;
                            }
                            if (isPrefixSearch && CultureInfo.CurrentCulture.CompareInfo.IsPrefix(item2.Text, text, CompareOptions.IgnoreCase))
                            {
                                return item;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public ListViewItem FindItemWithText(string text)
        {
            if (this.Items.Count == 0)
            {
                return null;
            }
            return this.FindItemWithText(text, true, 0, true);
        }

        public ListViewItem FindItemWithText(string text, bool includeSubItemsInSearch, int startIndex)
        {
            return this.FindItemWithText(text, includeSubItemsInSearch, startIndex, true);
        }

        public ListViewItem FindItemWithText(string text, bool includeSubItemsInSearch, int startIndex, bool isPrefixSearch)
        {
            if ((startIndex < 0) || (startIndex >= this.Items.Count))
            {
                throw new ArgumentOutOfRangeException("startIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "startIndex", startIndex.ToString(CultureInfo.CurrentCulture) }));
            }
            return this.FindItem(true, text, isPrefixSearch, new Point(0, 0), SearchDirectionHint.Down, startIndex, includeSubItemsInSearch);
        }

        public ListViewItem FindNearestItem(SearchDirectionHint dir, Point point)
        {
            return this.FindNearestItem(dir, point.X, point.Y);
        }

        public ListViewItem FindNearestItem(SearchDirectionHint searchDirection, int x, int y)
        {
            if ((this.View != System.Windows.Forms.View.SmallIcon) && (this.View != System.Windows.Forms.View.LargeIcon))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewFindNearestItemWorksOnlyInIconView"));
            }
            if ((searchDirection < SearchDirectionHint.Left) || (searchDirection > SearchDirectionHint.Down))
            {
                throw new ArgumentOutOfRangeException("searchDirection", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "searchDirection", searchDirection.ToString() }));
            }
            ListViewItem itemAt = this.GetItemAt(x, y);
            if (itemAt != null)
            {
                Rectangle bounds = itemAt.Bounds;
                Rectangle itemRect = this.GetItemRect(itemAt.Index, ItemBoundsPortion.Icon);
                switch (searchDirection)
                {
                    case SearchDirectionHint.Left:
                        x = Math.Max(bounds.Left, itemRect.Left) - 1;
                        break;

                    case SearchDirectionHint.Up:
                        y = Math.Max(bounds.Top, itemRect.Top) - 1;
                        break;

                    case SearchDirectionHint.Right:
                        x = Math.Max(bounds.Left, itemRect.Left) + 1;
                        break;

                    case SearchDirectionHint.Down:
                        y = Math.Max(bounds.Top, itemRect.Top) + 1;
                        break;
                }
            }
            return this.FindItem(false, string.Empty, false, new Point(x, y), searchDirection, -1, false);
        }

        private void ForceCheckBoxUpdate()
        {
            if (this.CheckBoxes && base.IsHandleCreated)
            {
                base.SendMessage(0x1036, 4, 0);
                base.SendMessage(0x1036, 4, 4);
                if (this.AutoArrange)
                {
                    this.ArrangeIcons(this.Alignment);
                }
            }
        }

        private string GenerateRandomName()
        {
            Random random;
            Bitmap bitmap = new Bitmap(this.BackgroundImage);
            int seed = 0;
            try
            {
                seed = (int) ((long) bitmap.GetHicon());
            }
            catch
            {
                bitmap.Dispose();
            }
            if (seed == 0)
            {
                random = new Random((int) DateTime.Now.Ticks);
            }
            else
            {
                random = new Random(seed);
            }
            return random.Next().ToString(CultureInfo.InvariantCulture);
        }

        private int GenerateUniqueID()
        {
            int num = this.nextID++;
            if (num == -1)
            {
                num = 0;
                this.nextID = 1;
            }
            return num;
        }

        internal int GetColumnIndex(ColumnHeader ch)
        {
            if (this.columnHeaders != null)
            {
                for (int i = 0; i < this.columnHeaders.Length; i++)
                {
                    if (this.columnHeaders[i] == ch)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        internal int GetDisplayIndex(ListViewItem item, int lastIndex)
        {
            this.ApplyUpdateCachedItems();
            if (base.IsHandleCreated && !this.ListViewHandleDestroyed)
            {
                System.Windows.Forms.NativeMethods.LVFINDINFO lParam = new System.Windows.Forms.NativeMethods.LVFINDINFO {
                    lParam = (IntPtr) item.ID,
                    flags = 1
                };
                int num = -1;
                if (lastIndex != -1)
                {
                    num = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.LVM_FINDITEM, lastIndex - 1, ref lParam);
                }
                if (num == -1)
                {
                    num = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.LVM_FINDITEM, -1, ref lParam);
                }
                return num;
            }
            int num2 = 0;
            foreach (object obj2 in this.listItemsArray)
            {
                if (obj2 == item)
                {
                    return num2;
                }
                num2++;
            }
            return -1;
        }

        private int GetIndexOfClickedItem(System.Windows.Forms.NativeMethods.LVHITTESTINFO lvhi)
        {
            Point position = Cursor.Position;
            position = base.PointToClientInternal(position);
            lvhi.pt_x = position.X;
            lvhi.pt_y = position.Y;
            return (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1012, 0, lvhi);
        }

        public ListViewItem GetItemAt(int x, int y)
        {
            System.Windows.Forms.NativeMethods.LVHITTESTINFO lParam = new System.Windows.Forms.NativeMethods.LVHITTESTINFO {
                pt_x = x,
                pt_y = y
            };
            int num = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1012, 0, lParam);
            ListViewItem item = null;
            if ((num >= 0) && ((lParam.flags & 14) != 0))
            {
                item = this.Items[num];
            }
            return item;
        }

        internal Point GetItemPosition(int index)
        {
            System.Windows.Forms.NativeMethods.POINT lParam = new System.Windows.Forms.NativeMethods.POINT();
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1010, index, lParam);
            return new Point(lParam.x, lParam.y);
        }

        public Rectangle GetItemRect(int index)
        {
            return this.GetItemRect(index, ItemBoundsPortion.Entire);
        }

        public Rectangle GetItemRect(int index, ItemBoundsPortion portion)
        {
            if ((index < 0) || (index >= this.Items.Count))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(portion, (int) portion, 0, 3))
            {
                throw new InvalidEnumArgumentException("portion", (int) portion, typeof(ItemBoundsPortion));
            }
            if ((this.View == System.Windows.Forms.View.Details) && (this.Columns.Count == 0))
            {
                return Rectangle.Empty;
            }
            System.Windows.Forms.NativeMethods.RECT lparam = new System.Windows.Forms.NativeMethods.RECT {
                left = (int) portion
            };
            if (((int) ((long) base.SendMessage(0x100e, index, ref lparam))) == 0)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            return Rectangle.FromLTRB(lparam.left, lparam.top, lparam.right, lparam.bottom);
        }

        private Rectangle GetItemRectOrEmpty(int index)
        {
            if ((index < 0) || (index >= this.Items.Count))
            {
                return Rectangle.Empty;
            }
            if ((this.View == System.Windows.Forms.View.Details) && (this.Columns.Count == 0))
            {
                return Rectangle.Empty;
            }
            System.Windows.Forms.NativeMethods.RECT lparam = new System.Windows.Forms.NativeMethods.RECT {
                left = 0
            };
            if (((int) ((long) base.SendMessage(0x100e, index, ref lparam))) == 0)
            {
                return Rectangle.Empty;
            }
            return Rectangle.FromLTRB(lparam.left, lparam.top, lparam.right, lparam.bottom);
        }

        internal int GetItemState(int index)
        {
            return this.GetItemState(index, 0xff0f);
        }

        internal int GetItemState(int index, int mask)
        {
            if (((index < 0) || (this.VirtualMode && (index >= this.VirtualListSize))) || (!this.VirtualMode && (index >= this.itemCount)))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            return (int) ((long) base.SendMessage(0x102c, index, mask));
        }

        private Font GetListHeaderFont()
        {
            IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x101f, 0, 0);
            IntPtr hfont = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, handle), 0x31, 0, 0);
            System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Assert();
            return Font.FromHfont(hfont);
        }

        private System.Windows.Forms.NativeMethods.LVGROUP GetLVGROUP(ListViewGroup group)
        {
            System.Windows.Forms.NativeMethods.LVGROUP lvgroup = new System.Windows.Forms.NativeMethods.LVGROUP {
                mask = 0x19
            };
            string header = group.Header;
            lvgroup.pszHeader = Marshal.StringToHGlobalAuto(header);
            lvgroup.cchHeader = header.Length;
            lvgroup.iGroupId = group.ID;
            switch (group.HeaderAlignment)
            {
                case HorizontalAlignment.Left:
                    lvgroup.uAlign = 1;
                    return lvgroup;

                case HorizontalAlignment.Right:
                    lvgroup.uAlign = 4;
                    return lvgroup;

                case HorizontalAlignment.Center:
                    lvgroup.uAlign = 2;
                    return lvgroup;
            }
            return lvgroup;
        }

        internal int GetNativeGroupId(ListViewItem item)
        {
            item.UpdateGroupFromName();
            if ((item.Group != null) && this.Groups.Contains(item.Group))
            {
                return item.Group.ID;
            }
            this.EnsureDefaultGroup();
            return this.DefaultGroup.ID;
        }

        internal void GetSubItemAt(int x, int y, out int iItem, out int iSubItem)
        {
            System.Windows.Forms.NativeMethods.LVHITTESTINFO lParam = new System.Windows.Forms.NativeMethods.LVHITTESTINFO {
                pt_x = x,
                pt_y = y
            };
            int num = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1039, 0, lParam);
            if (num > -1)
            {
                iItem = lParam.iItem;
                iSubItem = lParam.iSubItem;
            }
            else
            {
                iItem = -1;
                iSubItem = -1;
            }
        }

        internal Rectangle GetSubItemRect(int itemIndex, int subItemIndex)
        {
            return this.GetSubItemRect(itemIndex, subItemIndex, ItemBoundsPortion.Entire);
        }

        internal Rectangle GetSubItemRect(int itemIndex, int subItemIndex, ItemBoundsPortion portion)
        {
            if (this.View != System.Windows.Forms.View.Details)
            {
                return Rectangle.Empty;
            }
            if ((itemIndex < 0) || (itemIndex >= this.Items.Count))
            {
                throw new ArgumentOutOfRangeException("itemIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "itemIndex", itemIndex.ToString(CultureInfo.CurrentCulture) }));
            }
            int count = this.Items[itemIndex].SubItems.Count;
            if ((subItemIndex < 0) || (subItemIndex >= count))
            {
                throw new ArgumentOutOfRangeException("subItemIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "subItemIndex", subItemIndex.ToString(CultureInfo.CurrentCulture) }));
            }
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(portion, (int) portion, 0, 3))
            {
                throw new InvalidEnumArgumentException("portion", (int) portion, typeof(ItemBoundsPortion));
            }
            if (this.Columns.Count == 0)
            {
                return Rectangle.Empty;
            }
            System.Windows.Forms.NativeMethods.RECT lparam = new System.Windows.Forms.NativeMethods.RECT {
                left = (int) portion,
                top = subItemIndex
            };
            if (((int) ((long) base.SendMessage(0x1038, itemIndex, ref lparam))) == 0)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "itemIndex", itemIndex.ToString(CultureInfo.CurrentCulture) }));
            }
            return Rectangle.FromLTRB(lparam.left, lparam.top, lparam.right, lparam.bottom);
        }

        public ListViewHitTestInfo HitTest(Point point)
        {
            return this.HitTest(point.X, point.Y);
        }

        public ListViewHitTestInfo HitTest(int x, int y)
        {
            int num;
            if (!base.ClientRectangle.Contains(x, y))
            {
                return new ListViewHitTestInfo(null, null, ListViewHitTestLocations.None);
            }
            System.Windows.Forms.NativeMethods.LVHITTESTINFO lParam = new System.Windows.Forms.NativeMethods.LVHITTESTINFO {
                pt_x = x,
                pt_y = y
            };
            if (this.View == System.Windows.Forms.View.Details)
            {
                num = (int) ((long) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1039, 0, lParam));
            }
            else
            {
                num = (int) ((long) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1012, 0, lParam));
            }
            ListViewItem hitItem = (num == -1) ? null : this.Items[num];
            ListViewHitTestLocations none = ListViewHitTestLocations.None;
            if ((hitItem == null) && ((8 & lParam.flags) == 8))
            {
                none = (((ListViewHitTestLocations) 0xf7) & ((ListViewHitTestLocations) lParam.flags)) | ListViewHitTestLocations.AboveClientArea;
            }
            else if ((hitItem != null) && ((8 & lParam.flags) == 8))
            {
                none = (((ListViewHitTestLocations) 0xf7) & ((ListViewHitTestLocations) lParam.flags)) | ListViewHitTestLocations.StateImage;
            }
            else
            {
                none = (ListViewHitTestLocations) lParam.flags;
            }
            if (((this.View == System.Windows.Forms.View.Details) && (hitItem != null)) && (lParam.iSubItem < hitItem.SubItems.Count))
            {
                return new ListViewHitTestInfo(hitItem, hitItem.SubItems[lParam.iSubItem], none);
            }
            return new ListViewHitTestInfo(hitItem, null, none);
        }

        internal ColumnHeader InsertColumn(int index, ColumnHeader ch)
        {
            return this.InsertColumn(index, ch, true);
        }

        internal ColumnHeader InsertColumn(int index, ColumnHeader ch, bool refreshSubItems)
        {
            int num;
            if (ch == null)
            {
                throw new ArgumentNullException("ch");
            }
            if (ch.OwnerListview != null)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("OnlyOneControl", new object[] { ch.Text }), "ch");
            }
            if (base.IsHandleCreated && (this.View != System.Windows.Forms.View.Tile))
            {
                num = this.InsertColumnNative(index, ch);
            }
            else
            {
                num = index;
            }
            if (-1 == num)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewAddColumnFailed"));
            }
            int length = (this.columnHeaders == null) ? 0 : this.columnHeaders.Length;
            if (length > 0)
            {
                ColumnHeader[] destinationArray = new ColumnHeader[length + 1];
                if (length > 0)
                {
                    Array.Copy(this.columnHeaders, 0, destinationArray, 0, length);
                }
                this.columnHeaders = destinationArray;
            }
            else
            {
                this.columnHeaders = new ColumnHeader[1];
            }
            if (num < length)
            {
                Array.Copy(this.columnHeaders, num, this.columnHeaders, num + 1, length - num);
            }
            this.columnHeaders[num] = ch;
            ch.OwnerListview = this;
            if (((ch.ActualImageIndex_Internal != -1) && base.IsHandleCreated) && (this.View != System.Windows.Forms.View.Tile))
            {
                this.SetColumnInfo(0x10, ch);
            }
            int[] indices = new int[this.Columns.Count];
            for (int i = 0; i < this.Columns.Count; i++)
            {
                ColumnHeader header = this.Columns[i];
                if (header == ch)
                {
                    header.DisplayIndexInternal = index;
                }
                else if (header.DisplayIndex >= index)
                {
                    header.DisplayIndexInternal++;
                }
                indices[i] = header.DisplayIndexInternal;
            }
            this.SetDisplayIndices(indices);
            if (base.IsHandleCreated && (this.View == System.Windows.Forms.View.Tile))
            {
                this.RecreateHandleInternal();
                return ch;
            }
            if (base.IsHandleCreated && refreshSubItems)
            {
                this.RealizeAllSubItems();
            }
            return ch;
        }

        private int InsertColumnNative(int index, ColumnHeader ch)
        {
            System.Windows.Forms.NativeMethods.LVCOLUMN_T lParam = new System.Windows.Forms.NativeMethods.LVCOLUMN_T {
                mask = 7
            };
            if ((ch.OwnerListview != null) && (ch.ActualImageIndex_Internal != -1))
            {
                lParam.mask |= 0x10;
                lParam.iImage = ch.ActualImageIndex_Internal;
            }
            lParam.fmt = (int) ch.TextAlign;
            lParam.cx = ch.Width;
            lParam.pszText = ch.Text;
            return (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.LVM_INSERTCOLUMN, index, lParam);
        }

        internal void InsertGroupInListView(int index, ListViewGroup group)
        {
            bool flag = (this.groups.Count == 1) && this.GroupsEnabled;
            this.UpdateGroupView();
            this.EnsureDefaultGroup();
            this.InsertGroupNative(index, group);
            if (flag)
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    ListViewItem item = this.Items[i];
                    if (item.Group == null)
                    {
                        item.UpdateStateToListView(item.Index);
                    }
                }
            }
        }

        private void InsertGroupNative(int index, ListViewGroup group)
        {
            System.Windows.Forms.NativeMethods.LVGROUP lParam = new System.Windows.Forms.NativeMethods.LVGROUP();
            try
            {
                lParam = this.GetLVGROUP(group);
                int num1 = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1091, index, lParam);
            }
            finally
            {
                this.DestroyLVGROUP(lParam);
            }
        }

        private void InsertItems(int displayIndex, ListViewItem[] items, bool checkHosting)
        {
            if ((items != null) && (items.Length != 0))
            {
                if ((base.IsHandleCreated && (this.Items.Count == 0)) && ((this.View == System.Windows.Forms.View.SmallIcon) && this.ComctlSupportsVisualStyles))
                {
                    this.FlipViewToLargeIconAndSmallIcon = true;
                }
                if ((this.updateCounter > 0) && (base.Properties.GetObject(PropDelayedUpdateItems) != null))
                {
                    if (checkHosting)
                    {
                        for (int j = 0; j < items.Length; j++)
                        {
                            if (items[j].listView != null)
                            {
                                throw new ArgumentException(System.Windows.Forms.SR.GetString("OnlyOneControl", new object[] { items[j].Text }), "item");
                            }
                        }
                    }
                    ArrayList list = (ArrayList) base.Properties.GetObject(PropDelayedUpdateItems);
                    if (list != null)
                    {
                        list.AddRange(items);
                    }
                    for (int i = 0; i < items.Length; i++)
                    {
                        items[i].Host(this, this.GenerateUniqueID(), -1);
                    }
                    this.FlipViewToLargeIconAndSmallIcon = false;
                }
                else
                {
                    for (int k = 0; k < items.Length; k++)
                    {
                        ListViewItem item = items[k];
                        if (checkHosting && (item.listView != null))
                        {
                            throw new ArgumentException(System.Windows.Forms.SR.GetString("OnlyOneControl", new object[] { item.Text }), "item");
                        }
                        int key = this.GenerateUniqueID();
                        this.listItemsTable.Add(key, item);
                        this.itemCount++;
                        item.Host(this, key, -1);
                        if (!base.IsHandleCreated)
                        {
                            this.listItemsArray.Insert(displayIndex + k, item);
                        }
                    }
                    if (base.IsHandleCreated)
                    {
                        this.InsertItemsNative(displayIndex, items);
                    }
                    base.Invalidate();
                    this.ArrangeIcons(this.alignStyle);
                    if (!this.VirtualMode)
                    {
                        this.Sort();
                    }
                }
            }
        }

        private int InsertItemsNative(int index, ListViewItem[] items)
        {
            if ((items == null) || (items.Length == 0))
            {
                return 0;
            }
            if (index == (this.itemCount - 1))
            {
                index++;
            }
            System.Windows.Forms.NativeMethods.LVITEM lvItem = new System.Windows.Forms.NativeMethods.LVITEM();
            int num = -1;
            IntPtr zero = IntPtr.Zero;
            int cColumns = 0;
            this.listViewState1[1] = true;
            try
            {
                base.SendMessage(0x102f, this.itemCount, 0);
                for (int i = 0; i < items.Length; i++)
                {
                    int num5;
                    ListViewItem item = items[i];
                    lvItem.Reset();
                    lvItem.mask = 0x17;
                    lvItem.iItem = index + i;
                    lvItem.pszText = item.Text;
                    lvItem.iImage = item.ImageIndexer.ActualIndex;
                    lvItem.iIndent = item.IndentCount;
                    lvItem.lParam = (IntPtr) item.ID;
                    if (this.GroupsEnabled)
                    {
                        lvItem.mask |= 0x100;
                        lvItem.iGroupId = this.GetNativeGroupId(item);
                    }
                    lvItem.mask |= 0x200;
                    lvItem.cColumns = (this.columnHeaders != null) ? Math.Min(20, this.columnHeaders.Length) : 0;
                    if ((lvItem.cColumns > cColumns) || (zero == IntPtr.Zero))
                    {
                        if (zero != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(zero);
                        }
                        zero = Marshal.AllocHGlobal((int) (lvItem.cColumns * Marshal.SizeOf(typeof(int))));
                        cColumns = lvItem.cColumns;
                    }
                    lvItem.puColumns = zero;
                    int[] source = new int[lvItem.cColumns];
                    for (int j = 0; j < lvItem.cColumns; j++)
                    {
                        source[j] = j + 1;
                    }
                    Marshal.Copy(source, 0, lvItem.puColumns, lvItem.cColumns);
                    ItemCheckEventHandler onItemCheck = this.onItemCheck;
                    this.onItemCheck = null;
                    try
                    {
                        item.UpdateStateToListView(lvItem.iItem, ref lvItem, false);
                        num5 = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.LVM_INSERTITEM, 0, ref lvItem);
                        if (num == -1)
                        {
                            num = num5;
                            index = num;
                        }
                    }
                    finally
                    {
                        this.onItemCheck = onItemCheck;
                    }
                    if (-1 == num5)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewAddItemFailed"));
                    }
                    for (int k = 1; k < item.SubItems.Count; k++)
                    {
                        this.SetItemText(num5, k, item.SubItems[k].Text, ref lvItem);
                    }
                    if (item.StateImageSet || item.StateSelected)
                    {
                        this.SetItemState(num5, lvItem.state, lvItem.stateMask);
                    }
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
                this.listViewState1[1] = false;
            }
            if (this.listViewState1[0x10])
            {
                this.listViewState1[0x10] = false;
                this.OnSelectedIndexChanged(EventArgs.Empty);
            }
            if (this.FlipViewToLargeIconAndSmallIcon)
            {
                this.FlipViewToLargeIconAndSmallIcon = false;
                this.View = System.Windows.Forms.View.LargeIcon;
                this.View = System.Windows.Forms.View.SmallIcon;
            }
            return num;
        }

        private void InvalidateColumnHeaders()
        {
            if ((this.viewStyle == System.Windows.Forms.View.Details) && base.IsHandleCreated)
            {
                IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x101f, 0, 0);
                if (handle != IntPtr.Zero)
                {
                    System.Windows.Forms.SafeNativeMethods.InvalidateRect(new HandleRef(this, handle), (System.Windows.Forms.NativeMethods.COMRECT) null, true);
                }
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if ((keyData & Keys.Alt) != Keys.Alt)
            {
                switch ((keyData & Keys.KeyCode))
                {
                    case Keys.PageUp:
                    case Keys.Next:
                    case Keys.End:
                    case Keys.Home:
                        return true;
                }
                if (base.IsInputKey(keyData))
                {
                    return true;
                }
                if (this.listViewState[0x4000])
                {
                    switch ((keyData & Keys.KeyCode))
                    {
                        case Keys.Enter:
                        case Keys.Escape:
                            return true;
                    }
                }
            }
            return false;
        }

        private void LargeImageListChangedHandle(object sender, EventArgs e)
        {
            if ((!this.VirtualMode && (sender != null)) && ((sender == this.imageListLarge) && base.IsHandleCreated))
            {
                foreach (ListViewItem item in this.Items)
                {
                    if ((item.ImageIndexer.ActualIndex != -1) && (item.ImageIndexer.ActualIndex >= this.imageListLarge.Images.Count))
                    {
                        this.SetItemImage(item.Index, this.imageListLarge.Images.Count - 1);
                    }
                    else
                    {
                        this.SetItemImage(item.Index, item.ImageIndexer.ActualIndex);
                    }
                }
            }
        }

        private void LargeImageListRecreateHandle(object sender, EventArgs e)
        {
            if (base.IsHandleCreated)
            {
                IntPtr lparam = (this.LargeImageList == null) ? IntPtr.Zero : this.LargeImageList.Handle;
                base.SendMessage(0x1003, IntPtr.Zero, lparam);
                this.ForceCheckBoxUpdate();
            }
        }

        internal void ListViewItemToolTipChanged(ListViewItem item)
        {
            if (base.IsHandleCreated)
            {
                this.SetItemText(item.Index, 0, item.Text);
            }
        }

        private void LvnBeginDrag(MouseButtons buttons, System.Windows.Forms.NativeMethods.NMLISTVIEW nmlv)
        {
            ListViewItem item = this.Items[nmlv.iItem];
            this.OnItemDrag(new ItemDragEventArgs(buttons, item));
        }

        protected virtual void OnAfterLabelEdit(LabelEditEventArgs e)
        {
            if (this.onAfterLabelEdit != null)
            {
                this.onAfterLabelEdit(this, e);
            }
        }

        protected override void OnBackgroundImageChanged(EventArgs e)
        {
            if (base.IsHandleCreated)
            {
                this.SetBackgroundImage();
            }
            base.OnBackgroundImageChanged(e);
        }

        protected virtual void OnBeforeLabelEdit(LabelEditEventArgs e)
        {
            if (this.onBeforeLabelEdit != null)
            {
                this.onBeforeLabelEdit(this, e);
            }
        }

        protected virtual void OnCacheVirtualItems(CacheVirtualItemsEventArgs e)
        {
            CacheVirtualItemsEventHandler handler = (CacheVirtualItemsEventHandler) base.Events[EVENT_CACHEVIRTUALITEMS];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnColumnClick(ColumnClickEventArgs e)
        {
            if (this.onColumnClick != null)
            {
                this.onColumnClick(this, e);
            }
        }

        protected virtual void OnColumnReordered(ColumnReorderedEventArgs e)
        {
            ColumnReorderedEventHandler handler = (ColumnReorderedEventHandler) base.Events[EVENT_COLUMNREORDERED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnColumnWidthChanged(ColumnWidthChangedEventArgs e)
        {
            ColumnWidthChangedEventHandler handler = (ColumnWidthChangedEventHandler) base.Events[EVENT_COLUMNWIDTHCHANGED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnColumnWidthChanging(ColumnWidthChangingEventArgs e)
        {
            ColumnWidthChangingEventHandler handler = (ColumnWidthChangingEventHandler) base.Events[EVENT_COLUMNWIDTHCHANGING];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            DrawListViewColumnHeaderEventHandler handler = (DrawListViewColumnHeaderEventHandler) base.Events[EVENT_DRAWCOLUMNHEADER];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDrawItem(DrawListViewItemEventArgs e)
        {
            DrawListViewItemEventHandler handler = (DrawListViewItemEventHandler) base.Events[EVENT_DRAWITEM];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            DrawListViewSubItemEventHandler handler = (DrawListViewSubItemEventHandler) base.Events[EVENT_DRAWSUBITEM];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if ((!this.VirtualMode && base.IsHandleCreated) && this.AutoArrange)
            {
                this.BeginUpdate();
                try
                {
                    base.SendMessage(0x102a, -1, 0);
                }
                finally
                {
                    this.EndUpdate();
                }
            }
            this.InvalidateColumnHeaders();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            this.listViewState[0x400000] = false;
            this.FlipViewToLargeIconAndSmallIcon = false;
            base.OnHandleCreated(e);
            int num = (int) ((long) base.SendMessage(0x2008, 0, 0));
            if (num < 5)
            {
                base.SendMessage(0x2007, 5, 0);
            }
            this.UpdateExtendedStyles();
            this.RealizeProperties();
            int lparam = ColorTranslator.ToWin32(this.BackColor);
            base.SendMessage(0x1001, 0, lparam);
            base.SendMessage(0x1024, 0, ColorTranslator.ToWin32(base.ForeColor));
            base.SendMessage(0x1026, 0, -1);
            if (!this.Scrollable)
            {
                int windowLong = (int) ((long) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, base.Handle), -16));
                windowLong |= 0x2000;
                System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, base.Handle), -16, new HandleRef(null, (IntPtr) windowLong));
            }
            if (this.VirtualMode)
            {
                int wParam = (int) ((long) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x100a, 0, 0));
                wParam |= 0xf000;
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x100b, wParam, 0);
            }
            if (this.ComctlSupportsVisualStyles)
            {
                base.SendMessage(0x108e, (int) this.viewStyle, 0);
                this.UpdateGroupView();
                if (this.groups != null)
                {
                    for (int i = 0; i < this.groups.Count; i++)
                    {
                        this.InsertGroupNative(i, this.groups[i]);
                    }
                }
                if (this.viewStyle == System.Windows.Forms.View.Tile)
                {
                    this.UpdateTileView();
                }
            }
            this.ListViewHandleDestroyed = false;
            ListViewItem[] items = null;
            if (this.listItemsArray != null)
            {
                items = (ListViewItem[]) this.listItemsArray.ToArray(typeof(ListViewItem));
                this.listItemsArray = null;
            }
            int num6 = (this.columnHeaders == null) ? 0 : this.columnHeaders.Length;
            if (num6 > 0)
            {
                int[] indices = new int[this.columnHeaders.Length];
                int index = 0;
                foreach (ColumnHeader header in this.columnHeaders)
                {
                    indices[index] = header.DisplayIndex;
                    this.InsertColumnNative(index++, header);
                }
                this.SetDisplayIndices(indices);
            }
            if ((this.itemCount > 0) && (items != null))
            {
                this.InsertItemsNative(0, items);
            }
            if ((this.VirtualMode && (this.VirtualListSize > -1)) && !base.DesignMode)
            {
                base.SendMessage(0x102f, this.VirtualListSize, 0);
            }
            if (num6 > 0)
            {
                this.UpdateColumnWidths(ColumnHeaderAutoResizeStyle.None);
            }
            this.ArrangeIcons(this.alignStyle);
            this.UpdateListViewItemsLocations();
            if (!this.VirtualMode)
            {
                this.Sort();
            }
            if (this.ComctlSupportsVisualStyles && (this.InsertionMark.Index > 0))
            {
                this.InsertionMark.UpdateListView();
            }
            this.savedCheckedItems = null;
            if (!this.CheckBoxes && !this.VirtualMode)
            {
                for (int j = 0; j < this.Items.Count; j++)
                {
                    if (this.Items[j].Checked)
                    {
                        this.UpdateSavedCheckedItems(this.Items[j], true);
                    }
                }
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (!base.Disposing && !this.VirtualMode)
            {
                int count = this.Items.Count;
                for (int i = 0; i < count; i++)
                {
                    this.Items[i].UpdateStateFromListView(i, true);
                }
                if ((this.SelectedItems != null) && !this.VirtualMode)
                {
                    ListViewItem[] itemArray = new ListViewItem[this.SelectedItems.Count];
                    this.SelectedItems.CopyTo(itemArray, 0);
                    this.savedSelectedItems = new List<ListViewItem>(itemArray.Length);
                    for (int j = 0; j < itemArray.Length; j++)
                    {
                        this.savedSelectedItems.Add(itemArray[j]);
                    }
                }
                ListViewItem[] dest = null;
                ListViewItemCollection items = this.Items;
                if (items != null)
                {
                    dest = new ListViewItem[items.Count];
                    items.CopyTo(dest, 0);
                }
                if (dest != null)
                {
                    this.listItemsArray = new ArrayList(dest.Length);
                    this.listItemsArray.AddRange(dest);
                }
                this.ListViewHandleDestroyed = true;
            }
            base.OnHandleDestroyed(e);
        }

        protected virtual void OnItemActivate(EventArgs e)
        {
            if (this.onItemActivate != null)
            {
                this.onItemActivate(this, e);
            }
        }

        protected virtual void OnItemCheck(ItemCheckEventArgs ice)
        {
            if (this.onItemCheck != null)
            {
                this.onItemCheck(this, ice);
            }
        }

        protected virtual void OnItemChecked(ItemCheckedEventArgs e)
        {
            if (this.onItemChecked != null)
            {
                this.onItemChecked(this, e);
            }
        }

        protected virtual void OnItemDrag(ItemDragEventArgs e)
        {
            if (this.onItemDrag != null)
            {
                this.onItemDrag(this, e);
            }
        }

        protected virtual void OnItemMouseHover(ListViewItemMouseHoverEventArgs e)
        {
            if (this.onItemMouseHover != null)
            {
                this.onItemMouseHover(this, e);
            }
        }

        protected virtual void OnItemSelectionChanged(ListViewItemSelectionChangedEventArgs e)
        {
            ListViewItemSelectionChangedEventHandler handler = (ListViewItemSelectionChangedEventHandler) base.Events[EVENT_ITEMSELECTIONCHANGED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnMouseHover(EventArgs e)
        {
            ListViewItem itemAt = null;
            if (this.Items.Count > 0)
            {
                Point position = Cursor.Position;
                position = base.PointToClientInternal(position);
                itemAt = this.GetItemAt(position.X, position.Y);
            }
            if ((itemAt != this.prevHoveredItem) && (itemAt != null))
            {
                this.OnItemMouseHover(new ListViewItemMouseHoverEventArgs(itemAt));
                this.prevHoveredItem = itemAt;
            }
            if (!this.hoveredAlready)
            {
                base.OnMouseHover(e);
                this.hoveredAlready = true;
            }
            base.ResetMouseEventArgs();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.hoveredAlready = false;
            base.OnMouseLeave(e);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (base.IsHandleCreated)
            {
                this.RecreateHandleInternal();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            if (((this.View == System.Windows.Forms.View.Details) && !this.Scrollable) && base.IsHandleCreated)
            {
                this.PositionHeader();
            }
            base.OnResize(e);
        }

        protected virtual void OnRetrieveVirtualItem(RetrieveVirtualItemEventArgs e)
        {
            RetrieveVirtualItemEventHandler handler = (RetrieveVirtualItemEventHandler) base.Events[EVENT_RETRIEVEVIRTUALITEM];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnRightToLeftLayoutChanged(EventArgs e)
        {
            if (!base.GetAnyDisposingInHierarchy())
            {
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    this.RecreateHandleInternal();
                }
                EventHandler handler = base.Events[EVENT_RIGHTTOLEFTLAYOUTCHANGED] as EventHandler;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
        }

        protected virtual void OnSearchForVirtualItem(SearchForVirtualItemEventArgs e)
        {
            SearchForVirtualItemEventHandler handler = (SearchForVirtualItemEventHandler) base.Events[EVENT_SEARCHFORVIRTUALITEM];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_SELECTEDINDEXCHANGED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnSystemColorsChanged(EventArgs e)
        {
            base.OnSystemColorsChanged(e);
            if (base.IsHandleCreated)
            {
                int lparam = ColorTranslator.ToWin32(this.BackColor);
                base.SendMessage(0x1001, 0, lparam);
                base.SendMessage(0x1026, 0, -1);
            }
        }

        protected virtual void OnVirtualItemsSelectionRangeChanged(ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            ListViewVirtualItemsSelectionRangeChangedEventHandler handler = (ListViewVirtualItemsSelectionRangeChangedEventHandler) base.Events[EVENT_VIRTUALITEMSSELECTIONRANGECHANGED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void PositionHeader()
        {
            IntPtr window = System.Windows.Forms.UnsafeNativeMethods.GetWindow(new HandleRef(this, base.Handle), 5);
            if (window != IntPtr.Zero)
            {
                IntPtr zero = IntPtr.Zero;
                IntPtr ptr = IntPtr.Zero;
                zero = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.RECT)));
                if (zero != IntPtr.Zero)
                {
                    try
                    {
                        ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.WINDOWPOS)));
                        if (zero != IntPtr.Zero)
                        {
                            System.Windows.Forms.UnsafeNativeMethods.GetClientRect(new HandleRef(this, base.Handle), zero);
                            System.Windows.Forms.NativeMethods.HDLAYOUT hdlayout = new System.Windows.Forms.NativeMethods.HDLAYOUT {
                                prc = zero,
                                pwpos = ptr
                            };
                            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, window), 0x1205, 0, ref hdlayout);
                            System.Windows.Forms.NativeMethods.WINDOWPOS windowpos = (System.Windows.Forms.NativeMethods.WINDOWPOS) Marshal.PtrToStructure(ptr, typeof(System.Windows.Forms.NativeMethods.WINDOWPOS));
                            System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this, window), new HandleRef(this, windowpos.hwndInsertAfter), windowpos.x, windowpos.y, windowpos.cx, windowpos.cy, windowpos.flags | 0x40);
                        }
                    }
                    finally
                    {
                        if (zero != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(zero);
                        }
                        if (ptr != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(ptr);
                        }
                    }
                }
            }
        }

        private void RealizeAllSubItems()
        {
            System.Windows.Forms.NativeMethods.LVITEM lvItem = new System.Windows.Forms.NativeMethods.LVITEM();
            for (int i = 0; i < this.itemCount; i++)
            {
                int count = this.Items[i].SubItems.Count;
                for (int j = 0; j < count; j++)
                {
                    this.SetItemText(i, j, this.Items[i].SubItems[j].Text, ref lvItem);
                }
            }
        }

        protected void RealizeProperties()
        {
            Color backColor = this.BackColor;
            if (backColor != SystemColors.Window)
            {
                base.SendMessage(0x1001, 0, ColorTranslator.ToWin32(backColor));
            }
            backColor = this.ForeColor;
            if (backColor != SystemColors.WindowText)
            {
                base.SendMessage(0x1024, 0, ColorTranslator.ToWin32(backColor));
            }
            if (this.imageListLarge != null)
            {
                base.SendMessage(0x1003, 0, this.imageListLarge.Handle);
            }
            if (this.imageListSmall != null)
            {
                base.SendMessage(0x1003, 1, this.imageListSmall.Handle);
            }
            if (this.imageListState != null)
            {
                base.SendMessage(0x1003, 2, this.imageListState.Handle);
            }
        }

        internal void RecreateHandleInternal()
        {
            if (base.IsHandleCreated && (this.StateImageList != null))
            {
                base.SendMessage(0x1003, 2, IntPtr.Zero);
            }
            base.RecreateHandle();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void RedrawItems(int startIndex, int endIndex, bool invalidateOnly)
        {
            if (this.VirtualMode)
            {
                if ((startIndex < 0) || (startIndex >= this.VirtualListSize))
                {
                    throw new ArgumentOutOfRangeException("startIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "startIndex", startIndex.ToString(CultureInfo.CurrentCulture) }));
                }
                if ((endIndex < 0) || (endIndex >= this.VirtualListSize))
                {
                    throw new ArgumentOutOfRangeException("endIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "endIndex", endIndex.ToString(CultureInfo.CurrentCulture) }));
                }
            }
            else
            {
                if ((startIndex < 0) || (startIndex >= this.Items.Count))
                {
                    throw new ArgumentOutOfRangeException("startIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "startIndex", startIndex.ToString(CultureInfo.CurrentCulture) }));
                }
                if ((endIndex < 0) || (endIndex >= this.Items.Count))
                {
                    throw new ArgumentOutOfRangeException("endIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "endIndex", endIndex.ToString(CultureInfo.CurrentCulture) }));
                }
            }
            if (startIndex > endIndex)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("ListViewStartIndexCannotBeLargerThanEndIndex"));
            }
            if (base.IsHandleCreated)
            {
                int num1 = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1015, startIndex, endIndex);
                if ((this.View == System.Windows.Forms.View.LargeIcon) || (this.View == System.Windows.Forms.View.SmallIcon))
                {
                    Rectangle bounds = this.Items[startIndex].Bounds;
                    for (int i = startIndex + 1; i <= endIndex; i++)
                    {
                        bounds = Rectangle.Union(bounds, this.Items[i].Bounds);
                    }
                    if (startIndex > 0)
                    {
                        bounds = Rectangle.Union(bounds, this.Items[startIndex - 1].Bounds);
                    }
                    else
                    {
                        bounds.Width += bounds.X;
                        bounds.Height += bounds.Y;
                        bounds.X = bounds.Y = 0;
                    }
                    if (endIndex < (this.Items.Count - 1))
                    {
                        bounds = Rectangle.Union(bounds, this.Items[endIndex + 1].Bounds);
                    }
                    else
                    {
                        bounds.Height += base.ClientRectangle.Bottom - bounds.Bottom;
                        bounds.Width += base.ClientRectangle.Right - bounds.Right;
                    }
                    if (this.View == System.Windows.Forms.View.LargeIcon)
                    {
                        bounds.Inflate(1, this.Font.Height + 1);
                    }
                    base.Invalidate(bounds);
                }
                if (!invalidateOnly)
                {
                    base.Update();
                }
            }
        }

        internal void RemoveGroupFromListView(ListViewGroup group)
        {
            this.EnsureDefaultGroup();
            foreach (ListViewItem item in group.Items)
            {
                if (item.ListView == this)
                {
                    item.UpdateStateToListView(item.Index);
                }
            }
            this.RemoveGroupNative(group);
            this.UpdateGroupView();
        }

        private void RemoveGroupNative(ListViewGroup group)
        {
            int num1 = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1096, group.ID, IntPtr.Zero);
        }

        private void Scroll(int fromLVItem, int toLVItem)
        {
            int num = 0;
            int num2 = Math.Max(fromLVItem, toLVItem);
            for (int i = Math.Min(fromLVItem, toLVItem); i < num2; i++)
            {
                ListViewItem item = this.Items[i];
                int height = 0;
                if (item.ImageIndex != -1)
                {
                    height = item.ImageList.Images[item.ImageIndex].Size.Height;
                }
                int num6 = 0;
                if (!string.IsNullOrEmpty(item.Text))
                {
                    using (Graphics graphics = base.CreateGraphicsInternal())
                    {
                        num6 = Size.Ceiling(graphics.MeasureString(item.Text, this.Font)).Height;
                    }
                }
                num += Math.Max(num6, height);
            }
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1014, 0, (fromLVItem < toLVItem) ? num : -num);
        }

        private void SetBackgroundImage()
        {
            Application.OleRequired();
            System.Windows.Forms.NativeMethods.LVBKIMAGE lParam = new System.Windows.Forms.NativeMethods.LVBKIMAGE {
                xOffset = 0,
                yOffset = 0
            };
            string backgroundImageFileName = this.backgroundImageFileName;
            if (this.BackgroundImage != null)
            {
                EnvironmentPermission perm = new EnvironmentPermission(EnvironmentPermissionAccess.Read, "TEMP");
                FileIOPermission permission2 = new FileIOPermission(PermissionState.Unrestricted);
                PermissionSet set = new PermissionSet(PermissionState.Unrestricted);
                set.AddPermission(perm);
                set.AddPermission(permission2);
                set.Assert();
                try
                {
                    string tempPath = Path.GetTempPath();
                    StringBuilder sb = new StringBuilder(0x400);
                    System.Windows.Forms.UnsafeNativeMethods.GetTempFileName(tempPath, this.GenerateRandomName(), 0, sb);
                    this.backgroundImageFileName = sb.ToString();
                    this.BackgroundImage.Save(this.backgroundImageFileName, ImageFormat.Bmp);
                }
                finally
                {
                    PermissionSet.RevertAssert();
                }
                lParam.pszImage = this.backgroundImageFileName;
                lParam.cchImageMax = this.backgroundImageFileName.Length + 1;
                lParam.ulFlags = 2;
                if (this.BackgroundImageTiled)
                {
                    lParam.ulFlags |= 0x10;
                }
                else
                {
                    lParam.ulFlags = lParam.ulFlags;
                }
            }
            else
            {
                lParam.ulFlags = 0;
                this.backgroundImageFileName = string.Empty;
            }
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.LVM_SETBKIMAGE, 0, lParam);
            if (!string.IsNullOrEmpty(backgroundImageFileName))
            {
                if (this.bkImgFileNames == null)
                {
                    this.bkImgFileNames = new string[8];
                    this.bkImgFileNamesCount = -1;
                }
                if (this.bkImgFileNamesCount == 7)
                {
                    this.DeleteFileName(this.bkImgFileNames[0]);
                    this.bkImgFileNames[0] = this.bkImgFileNames[1];
                    this.bkImgFileNames[1] = this.bkImgFileNames[2];
                    this.bkImgFileNames[2] = this.bkImgFileNames[3];
                    this.bkImgFileNames[3] = this.bkImgFileNames[4];
                    this.bkImgFileNames[4] = this.bkImgFileNames[5];
                    this.bkImgFileNames[5] = this.bkImgFileNames[6];
                    this.bkImgFileNames[6] = this.bkImgFileNames[7];
                    this.bkImgFileNames[7] = null;
                    this.bkImgFileNamesCount--;
                }
                this.bkImgFileNamesCount++;
                this.bkImgFileNames[this.bkImgFileNamesCount] = backgroundImageFileName;
                this.Refresh();
            }
        }

        internal void SetColumnInfo(int mask, ColumnHeader ch)
        {
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.NativeMethods.LVCOLUMN lParam = new System.Windows.Forms.NativeMethods.LVCOLUMN {
                    mask = mask
                };
                if (((mask & 0x10) != 0) || ((mask & 1) != 0))
                {
                    lParam.mask |= 1;
                    if (ch.ActualImageIndex_Internal > -1)
                    {
                        lParam.iImage = ch.ActualImageIndex_Internal;
                        lParam.fmt |= 0x800;
                    }
                    lParam.fmt |= ch.TextAlign;
                }
                if ((mask & 4) != 0)
                {
                    lParam.pszText = Marshal.StringToHGlobalAuto(ch.Text);
                }
                int num = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.LVM_SETCOLUMN, ch.Index, lParam);
                if ((mask & 4) != 0)
                {
                    Marshal.FreeHGlobal(lParam.pszText);
                }
                if (num == 0)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewColumnInfoSet"));
                }
                this.InvalidateColumnHeaders();
            }
        }

        private void SetColumnWidth(int index, int width)
        {
            if (base.IsHandleCreated)
            {
                base.SendMessage(0x101e, index, System.Windows.Forms.NativeMethods.Util.MAKELPARAM(width, 0));
            }
        }

        internal void SetColumnWidth(int columnIndex, ColumnHeaderAutoResizeStyle headerAutoResize)
        {
            if (((columnIndex < 0) || ((columnIndex >= 0) && (this.columnHeaders == null))) || (columnIndex >= this.columnHeaders.Length))
            {
                throw new ArgumentOutOfRangeException("columnIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "columnIndex", columnIndex.ToString(CultureInfo.CurrentCulture) }));
            }
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(headerAutoResize, (int) headerAutoResize, 0, 2))
            {
                throw new InvalidEnumArgumentException("headerAutoResize", (int) headerAutoResize, typeof(ColumnHeaderAutoResizeStyle));
            }
            int low = 0;
            int num2 = 0;
            if (headerAutoResize == ColumnHeaderAutoResizeStyle.None)
            {
                low = this.columnHeaders[columnIndex].WidthInternal;
                switch (low)
                {
                    case -2:
                        headerAutoResize = ColumnHeaderAutoResizeStyle.HeaderSize;
                        break;

                    case -1:
                        headerAutoResize = ColumnHeaderAutoResizeStyle.ColumnContent;
                        break;
                }
            }
            if (headerAutoResize == ColumnHeaderAutoResizeStyle.HeaderSize)
            {
                num2 = this.CompensateColumnHeaderResize(columnIndex, false);
                low = -2;
            }
            else if (headerAutoResize == ColumnHeaderAutoResizeStyle.ColumnContent)
            {
                num2 = this.CompensateColumnHeaderResize(columnIndex, false);
                low = -1;
            }
            if (base.IsHandleCreated)
            {
                base.SendMessage(0x101e, columnIndex, System.Windows.Forms.NativeMethods.Util.MAKELPARAM(low, 0));
            }
            if ((base.IsHandleCreated && ((headerAutoResize == ColumnHeaderAutoResizeStyle.ColumnContent) || (headerAutoResize == ColumnHeaderAutoResizeStyle.HeaderSize))) && (num2 != 0))
            {
                int num3 = this.columnHeaders[columnIndex].Width + num2;
                base.SendMessage(0x101e, columnIndex, System.Windows.Forms.NativeMethods.Util.MAKELPARAM(num3, 0));
            }
        }

        private void SetDisplayIndices(int[] indices)
        {
            int[] lParam = new int[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                this.Columns[i].DisplayIndexInternal = indices[i];
                lParam[indices[i]] = i;
            }
            if (base.IsHandleCreated && !base.Disposing)
            {
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x103a, lParam.Length, lParam);
            }
        }

        internal void SetItemImage(int index, int image)
        {
            if (((index < 0) || (this.VirtualMode && (index >= this.VirtualListSize))) || (!this.VirtualMode && (index >= this.itemCount)))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.NativeMethods.LVITEM lParam = new System.Windows.Forms.NativeMethods.LVITEM {
                    mask = 2,
                    iItem = index,
                    iImage = image
                };
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.LVM_SETITEM, 0, ref lParam);
            }
        }

        internal void SetItemIndentCount(int index, int indentCount)
        {
            if (((index < 0) || (this.VirtualMode && (index >= this.VirtualListSize))) || (!this.VirtualMode && (index >= this.itemCount)))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.NativeMethods.LVITEM lParam = new System.Windows.Forms.NativeMethods.LVITEM {
                    mask = 0x10,
                    iItem = index,
                    iIndent = indentCount
                };
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.LVM_SETITEM, 0, ref lParam);
            }
        }

        internal void SetItemPosition(int index, int x, int y)
        {
            if (!this.VirtualMode)
            {
                if ((index < 0) || (index >= this.itemCount))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                System.Windows.Forms.NativeMethods.POINT lParam = new System.Windows.Forms.NativeMethods.POINT {
                    x = x,
                    y = y
                };
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1031, index, lParam);
            }
        }

        internal void SetItemState(int index, int state, int mask)
        {
            if (((index < -1) || (this.VirtualMode && (index >= this.VirtualListSize))) || (!this.VirtualMode && (index >= this.itemCount)))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.NativeMethods.LVITEM lParam = new System.Windows.Forms.NativeMethods.LVITEM {
                    mask = 8,
                    state = state,
                    stateMask = mask
                };
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x102b, index, ref lParam);
            }
        }

        internal void SetItemText(int itemIndex, int subItemIndex, string text)
        {
            System.Windows.Forms.NativeMethods.LVITEM lvItem = new System.Windows.Forms.NativeMethods.LVITEM();
            this.SetItemText(itemIndex, subItemIndex, text, ref lvItem);
        }

        private void SetItemText(int itemIndex, int subItemIndex, string text, ref System.Windows.Forms.NativeMethods.LVITEM lvItem)
        {
            if ((this.View == System.Windows.Forms.View.List) && (subItemIndex == 0))
            {
                int num = (int) ((long) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x101d, 0, 0));
                Graphics graphics = base.CreateGraphicsInternal();
                int width = 0;
                try
                {
                    width = Size.Ceiling(graphics.MeasureString(text, this.Font)).Width;
                }
                finally
                {
                    graphics.Dispose();
                }
                if (width > num)
                {
                    this.SetColumnWidth(0, width);
                }
            }
            lvItem.mask = 1;
            lvItem.iItem = itemIndex;
            lvItem.iSubItem = subItemIndex;
            lvItem.pszText = text;
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.LVM_SETITEMTEXT, itemIndex, ref lvItem);
        }

        internal void SetSelectionMark(int itemIndex)
        {
            if ((itemIndex >= 0) && (itemIndex < this.Items.Count))
            {
                base.SendMessage(0x1043, 0, itemIndex);
            }
        }

        internal void SetToolTip(System.Windows.Forms.ToolTip toolTip, string toolTipCaption)
        {
            this.toolTipCaption = toolTipCaption;
            IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x104a, 0, new HandleRef(toolTip, toolTip.Handle));
            System.Windows.Forms.UnsafeNativeMethods.DestroyWindow(new HandleRef(null, handle));
        }

        private bool ShouldSerializeTileSize()
        {
            return !this.tileSize.Equals(Size.Empty);
        }

        private void SmallImageListRecreateHandle(object sender, EventArgs e)
        {
            if (base.IsHandleCreated)
            {
                IntPtr lparam = (this.SmallImageList == null) ? IntPtr.Zero : this.SmallImageList.Handle;
                base.SendMessage(0x1003, (IntPtr) 1, lparam);
                this.ForceCheckBoxUpdate();
            }
        }

        public void Sort()
        {
            if (this.VirtualMode)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewSortNotAllowedInVirtualListView"));
            }
            this.ApplyUpdateCachedItems();
            if (base.IsHandleCreated && (this.listItemSorter != null))
            {
                System.Windows.Forms.NativeMethods.ListViewCompareCallback pfnCompare = new System.Windows.Forms.NativeMethods.ListViewCompareCallback(this.CompareFunc);
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1030, IntPtr.Zero, pfnCompare);
            }
        }

        private void StateImageListRecreateHandle(object sender, EventArgs e)
        {
            if (base.IsHandleCreated)
            {
                IntPtr zero = IntPtr.Zero;
                if (this.StateImageList != null)
                {
                    zero = this.imageListState.Handle;
                }
                base.SendMessage(0x1003, (IntPtr) 2, zero);
            }
        }

        public override string ToString()
        {
            string str = base.ToString();
            if (this.listItemsArray != null)
            {
                str = str + ", Items.Count: " + this.listItemsArray.Count.ToString(CultureInfo.CurrentCulture);
                if (this.listItemsArray.Count > 0)
                {
                    string str2 = this.listItemsArray[0].ToString();
                    string str3 = (str2.Length > 40) ? str2.Substring(0, 40) : str2;
                    str = str + ", Items[0]: " + str3;
                }
                return str;
            }
            if (this.Items != null)
            {
                str = str + ", Items.Count: " + this.Items.Count.ToString(CultureInfo.CurrentCulture);
                if ((this.Items.Count > 0) && !this.VirtualMode)
                {
                    string str4 = (this.Items[0] == null) ? "null" : this.Items[0].ToString();
                    string str5 = (str4.Length > 40) ? str4.Substring(0, 40) : str4;
                    str = str + ", Items[0]: " + str5;
                }
            }
            return str;
        }

        private void UpdateColumnWidths(ColumnHeaderAutoResizeStyle headerAutoResize)
        {
            if (this.columnHeaders != null)
            {
                for (int i = 0; i < this.columnHeaders.Length; i++)
                {
                    this.SetColumnWidth(i, headerAutoResize);
                }
            }
        }

        protected void UpdateExtendedStyles()
        {
            if (base.IsHandleCreated)
            {
                int lparam = 0;
                int wparam = 0x10cfd;
                switch (this.activation)
                {
                    case ItemActivation.OneClick:
                        lparam |= 0x40;
                        break;

                    case ItemActivation.TwoClick:
                        lparam |= 0x80;
                        break;
                }
                if (this.AllowColumnReorder)
                {
                    lparam |= 0x10;
                }
                if (this.CheckBoxes)
                {
                    lparam |= 4;
                }
                if (this.DoubleBuffered)
                {
                    lparam |= 0x10000;
                }
                if (this.FullRowSelect)
                {
                    lparam |= 0x20;
                }
                if (this.GridLines)
                {
                    lparam |= 1;
                }
                if (this.HoverSelection)
                {
                    lparam |= 8;
                }
                if (this.HotTracking)
                {
                    lparam |= 0x800;
                }
                if (this.ShowItemToolTips)
                {
                    lparam |= 0x400;
                }
                base.SendMessage(0x1036, wparam, lparam);
                base.Invalidate();
            }
        }

        internal void UpdateGroupNative(ListViewGroup group)
        {
            System.Windows.Forms.NativeMethods.LVGROUP lParam = new System.Windows.Forms.NativeMethods.LVGROUP();
            try
            {
                lParam = this.GetLVGROUP(group);
                int num1 = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1093, group.ID, lParam);
            }
            finally
            {
                this.DestroyLVGROUP(lParam);
            }
            base.Invalidate();
        }

        internal void UpdateGroupView()
        {
            if ((base.IsHandleCreated && this.ComctlSupportsVisualStyles) && !this.VirtualMode)
            {
                long num1 = (long) base.SendMessage(0x109d, this.GroupsEnabled ? 1 : 0, 0);
            }
        }

        internal void UpdateListViewItemsLocations()
        {
            if (((!this.VirtualMode && base.IsHandleCreated) && this.AutoArrange) && ((this.View == System.Windows.Forms.View.LargeIcon) || (this.View == System.Windows.Forms.View.SmallIcon)))
            {
                try
                {
                    this.BeginUpdate();
                    base.SendMessage(0x102a, -1, 0);
                }
                finally
                {
                    this.EndUpdate();
                }
            }
        }

        internal void UpdateSavedCheckedItems(ListViewItem item, bool addItem)
        {
            if (addItem && (this.savedCheckedItems == null))
            {
                this.savedCheckedItems = new List<ListViewItem>();
            }
            if (addItem)
            {
                this.savedCheckedItems.Add(item);
            }
            else if (this.savedCheckedItems != null)
            {
                this.savedCheckedItems.Remove(item);
            }
        }

        private void UpdateTileView()
        {
            System.Windows.Forms.NativeMethods.LVTILEVIEWINFO lvtileviewinfo;
            lvtileviewinfo = new System.Windows.Forms.NativeMethods.LVTILEVIEWINFO {
                dwMask = 2,
                cLines = (this.columnHeaders != null) ? this.columnHeaders.Length : 0,
                dwMask = lvtileviewinfo.dwMask | 1,
                dwFlags = 3,
                sizeTile = new System.Windows.Forms.NativeMethods.SIZE(this.TileSize.Width, this.TileSize.Height)
            };
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x10a2, 0, lvtileviewinfo);
        }

        private void WmMouseDown(ref Message m, MouseButtons button, int clicks)
        {
            this.listViewState[0x80000] = false;
            this.listViewState[0x100000] = true;
            this.FocusInternal();
            int x = System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam);
            int y = System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam);
            this.OnMouseDown(new MouseEventArgs(button, clicks, x, y, 0));
            if (!base.ValidationCancelled)
            {
                if ((this.CheckBoxes && (this.imageListState != null)) && (this.imageListState.Images.Count < 2))
                {
                    if (this.HitTest(x, y).Location != ListViewHitTestLocations.StateImage)
                    {
                        this.DefWndProc(ref m);
                    }
                }
                else
                {
                    this.DefWndProc(ref m);
                }
            }
        }

        private void WmNmClick(ref Message m)
        {
            if (this.CheckBoxes)
            {
                Point position = Cursor.Position;
                position = base.PointToClientInternal(position);
                System.Windows.Forms.NativeMethods.LVHITTESTINFO lParam = new System.Windows.Forms.NativeMethods.LVHITTESTINFO {
                    pt_x = position.X,
                    pt_y = position.Y
                };
                int num = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1012, 0, lParam);
                if ((num != -1) && ((lParam.flags & 8) != 0))
                {
                    ListViewItem item = this.Items[num];
                    if (item.Selected)
                    {
                        bool flag = !item.Checked;
                        if (!this.VirtualMode)
                        {
                            foreach (ListViewItem item2 in this.SelectedItems)
                            {
                                if (item2 != item)
                                {
                                    item2.Checked = flag;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void WmNmDblClick(ref Message m)
        {
            if (this.CheckBoxes)
            {
                Point position = Cursor.Position;
                position = base.PointToClientInternal(position);
                System.Windows.Forms.NativeMethods.LVHITTESTINFO lParam = new System.Windows.Forms.NativeMethods.LVHITTESTINFO {
                    pt_x = position.X,
                    pt_y = position.Y
                };
                int num = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1012, 0, lParam);
                if ((num != -1) && ((lParam.flags & 14) != 0))
                {
                    ListViewItem item = this.Items[num];
                    item.Checked = !item.Checked;
                }
            }
        }

        private unsafe bool WmNotify(ref Message m)
        {
            System.Windows.Forms.NativeMethods.NMHDR* lParam = (System.Windows.Forms.NativeMethods.NMHDR*) m.LParam;
            if ((lParam->code == -12) && this.OwnerDraw)
            {
                try
                {
                    System.Windows.Forms.NativeMethods.NMCUSTOMDRAW* nmcustomdrawPtr = (System.Windows.Forms.NativeMethods.NMCUSTOMDRAW*) m.LParam;
                    switch (nmcustomdrawPtr->dwDrawStage)
                    {
                        case 1:
                            m.Result = (IntPtr) 0x20;
                            return true;

                        case 0x10001:
                        {
                            Graphics graphics = Graphics.FromHdcInternal(nmcustomdrawPtr->hdc);
                            Rectangle bounds = Rectangle.FromLTRB(nmcustomdrawPtr->rc.left, nmcustomdrawPtr->rc.top, nmcustomdrawPtr->rc.right, nmcustomdrawPtr->rc.bottom);
                            DrawListViewColumnHeaderEventArgs e = null;
                            try
                            {
                                Color foreColor = ColorTranslator.FromWin32(System.Windows.Forms.SafeNativeMethods.GetTextColor(new HandleRef(this, nmcustomdrawPtr->hdc)));
                                Color backColor = ColorTranslator.FromWin32(System.Windows.Forms.SafeNativeMethods.GetBkColor(new HandleRef(this, nmcustomdrawPtr->hdc)));
                                Font listHeaderFont = this.GetListHeaderFont();
                                e = new DrawListViewColumnHeaderEventArgs(graphics, bounds, (int) nmcustomdrawPtr->dwItemSpec, this.columnHeaders[(int) nmcustomdrawPtr->dwItemSpec], (ListViewItemStates) nmcustomdrawPtr->uItemState, foreColor, backColor, listHeaderFont);
                                this.OnDrawColumnHeader(e);
                            }
                            finally
                            {
                                graphics.Dispose();
                            }
                            if (e.DrawDefault)
                            {
                                m.Result = IntPtr.Zero;
                                return false;
                            }
                            m.Result = (IntPtr) 4;
                            return true;
                        }
                    }
                    return false;
                }
                catch (Exception)
                {
                    m.Result = IntPtr.Zero;
                }
            }
            if ((lParam->code == -16) && this.listViewState[0x20000])
            {
                this.listViewState[0x20000] = false;
                this.OnColumnClick(new ColumnClickEventArgs(this.columnIndex));
            }
            if ((lParam->code == -306) || (lParam->code == -326))
            {
                this.listViewState[0x4000000] = true;
                this.listViewState1[2] = false;
                this.newWidthForColumnWidthChangingCancelled = -1;
                this.listViewState1[2] = false;
                System.Windows.Forms.NativeMethods.NMHEADER nmheader = (System.Windows.Forms.NativeMethods.NMHEADER) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHEADER));
                if ((this.columnHeaders != null) && (this.columnHeaders.Length > nmheader.iItem))
                {
                    this.columnHeaderClicked = this.columnHeaders[nmheader.iItem];
                    this.columnHeaderClickedWidth = this.columnHeaderClicked.Width;
                }
                else
                {
                    this.columnHeaderClickedWidth = -1;
                    this.columnHeaderClicked = null;
                }
            }
            if ((lParam->code == -300) || (lParam->code == -320))
            {
                System.Windows.Forms.NativeMethods.NMHEADER nmheader2 = (System.Windows.Forms.NativeMethods.NMHEADER) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHEADER));
                if (((this.columnHeaders != null) && (nmheader2.iItem < this.columnHeaders.Length)) && (this.listViewState[0x4000000] || this.listViewState[0x20000000]))
                {
                    System.Windows.Forms.NativeMethods.HDITEM2 hditem = (System.Windows.Forms.NativeMethods.HDITEM2) System.Windows.Forms.UnsafeNativeMethods.PtrToStructure(nmheader2.pItem, typeof(System.Windows.Forms.NativeMethods.HDITEM2));
                    int newWidth = ((hditem.mask & 1) != 0) ? hditem.cxy : -1;
                    ColumnWidthChangingEventArgs args2 = new ColumnWidthChangingEventArgs(nmheader2.iItem, newWidth);
                    this.OnColumnWidthChanging(args2);
                    m.Result = args2.Cancel ? ((IntPtr) 1) : IntPtr.Zero;
                    if (!args2.Cancel)
                    {
                        return false;
                    }
                    hditem.cxy = args2.NewWidth;
                    if (this.listViewState[0x20000000])
                    {
                        this.listViewState[0x40000000] = true;
                    }
                    this.listViewState1[2] = true;
                    this.newWidthForColumnWidthChangingCancelled = args2.NewWidth;
                    return true;
                }
            }
            if (((lParam->code == -301) || (lParam->code == -321)) && !this.listViewState[0x4000000])
            {
                System.Windows.Forms.NativeMethods.NMHEADER nmheader3 = (System.Windows.Forms.NativeMethods.NMHEADER) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHEADER));
                if ((this.columnHeaders != null) && (nmheader3.iItem < this.columnHeaders.Length))
                {
                    int width = this.columnHeaders[nmheader3.iItem].Width;
                    if ((this.columnHeaderClicked == null) || (((this.columnHeaderClicked == this.columnHeaders[nmheader3.iItem]) && (this.columnHeaderClickedWidth != -1)) && (this.columnHeaderClickedWidth != width)))
                    {
                        if (this.listViewState[0x20000000])
                        {
                            if (this.CompensateColumnHeaderResize(m, this.listViewState[0x40000000]) == 0)
                            {
                                this.OnColumnWidthChanged(new ColumnWidthChangedEventArgs(nmheader3.iItem));
                            }
                        }
                        else
                        {
                            this.OnColumnWidthChanged(new ColumnWidthChangedEventArgs(nmheader3.iItem));
                        }
                    }
                }
                this.columnHeaderClicked = null;
                this.columnHeaderClickedWidth = -1;
                ISite site = this.Site;
                if (site != null)
                {
                    IComponentChangeService service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                    if (service != null)
                    {
                        try
                        {
                            service.OnComponentChanging(this, null);
                        }
                        catch (CheckoutException exception)
                        {
                            if (exception != CheckoutException.Canceled)
                            {
                                throw exception;
                            }
                            return false;
                        }
                    }
                }
            }
            if ((lParam->code == -307) || (lParam->code == -327))
            {
                this.listViewState[0x4000000] = false;
                if (!this.listViewState1[2])
                {
                    return false;
                }
                m.Result = (IntPtr) 1;
                if (this.newWidthForColumnWidthChangingCancelled != -1)
                {
                    System.Windows.Forms.NativeMethods.NMHEADER nmheader4 = (System.Windows.Forms.NativeMethods.NMHEADER) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHEADER));
                    if ((this.columnHeaders != null) && (this.columnHeaders.Length > nmheader4.iItem))
                    {
                        this.columnHeaders[nmheader4.iItem].Width = this.newWidthForColumnWidthChangingCancelled;
                    }
                }
                this.listViewState1[2] = false;
                this.newWidthForColumnWidthChangingCancelled = -1;
                return true;
            }
            if (lParam->code == -311)
            {
                System.Windows.Forms.NativeMethods.NMHEADER nmheader5 = (System.Windows.Forms.NativeMethods.NMHEADER) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHEADER));
                if (nmheader5.pItem != IntPtr.Zero)
                {
                    System.Windows.Forms.NativeMethods.HDITEM2 hditem2 = (System.Windows.Forms.NativeMethods.HDITEM2) System.Windows.Forms.UnsafeNativeMethods.PtrToStructure(nmheader5.pItem, typeof(System.Windows.Forms.NativeMethods.HDITEM2));
                    if ((hditem2.mask & 0x80) == 0x80)
                    {
                        int displayIndex = this.Columns[nmheader5.iItem].DisplayIndex;
                        int iOrder = hditem2.iOrder;
                        if (displayIndex == iOrder)
                        {
                            return false;
                        }
                        if (iOrder < 0)
                        {
                            return false;
                        }
                        ColumnReorderedEventArgs args3 = new ColumnReorderedEventArgs(displayIndex, iOrder, this.Columns[nmheader5.iItem]);
                        this.OnColumnReordered(args3);
                        if (args3.Cancel)
                        {
                            m.Result = new IntPtr(1);
                            return true;
                        }
                        int num5 = Math.Min(displayIndex, iOrder);
                        int num6 = Math.Max(displayIndex, iOrder);
                        bool flag = iOrder > displayIndex;
                        ColumnHeader header = null;
                        int[] indices = new int[this.Columns.Count];
                        for (int i = 0; i < this.Columns.Count; i++)
                        {
                            ColumnHeader header2 = this.Columns[i];
                            if (header2.DisplayIndex == displayIndex)
                            {
                                header = header2;
                            }
                            else if ((header2.DisplayIndex >= num5) && (header2.DisplayIndex <= num6))
                            {
                                header2.DisplayIndexInternal -= flag ? 1 : -1;
                            }
                            indices[i] = header2.DisplayIndexInternal;
                        }
                        header.DisplayIndexInternal = iOrder;
                        indices[header.Index] = header.DisplayIndexInternal;
                        this.SetDisplayIndices(indices);
                    }
                }
            }
            if ((lParam->code != -305) && (lParam->code != -325))
            {
                return false;
            }
            this.listViewState[0x20000000] = true;
            this.listViewState[0x40000000] = false;
            bool columnResizeCancelled = false;
            try
            {
                this.DefWndProc(ref m);
            }
            finally
            {
                this.listViewState[0x20000000] = false;
                columnResizeCancelled = this.listViewState[0x40000000];
                this.listViewState[0x40000000] = false;
            }
            this.columnHeaderClicked = null;
            this.columnHeaderClickedWidth = -1;
            if (columnResizeCancelled)
            {
                if (this.newWidthForColumnWidthChangingCancelled != -1)
                {
                    System.Windows.Forms.NativeMethods.NMHEADER nmheader6 = (System.Windows.Forms.NativeMethods.NMHEADER) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHEADER));
                    if ((this.columnHeaders != null) && (this.columnHeaders.Length > nmheader6.iItem))
                    {
                        this.columnHeaders[nmheader6.iItem].Width = this.newWidthForColumnWidthChangingCancelled;
                    }
                }
                m.Result = (IntPtr) 1;
            }
            else
            {
                int num8 = this.CompensateColumnHeaderResize(m, columnResizeCancelled);
                if (num8 != 0)
                {
                    ColumnHeader header3 = this.columnHeaders[0];
                    header3.Width += num8;
                }
            }
            return true;
        }

        private void WmPrint(ref Message m)
        {
            base.WndProc(ref m);
            if ((((2 & ((int) m.LParam)) != 0) && Application.RenderWithVisualStyles) && (this.BorderStyle == System.Windows.Forms.BorderStyle.Fixed3D))
            {
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    using (Graphics graphics = Graphics.FromHdc(m.WParam))
                    {
                        Rectangle rect = new Rectangle(0, 0, base.Size.Width - 1, base.Size.Height - 1);
                        graphics.DrawRectangle(new Pen(VisualStyleInformation.TextControlBorder), rect);
                        rect.Inflate(-1, -1);
                        graphics.DrawRectangle(SystemPens.Window, rect);
                    }
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
        }

        private unsafe void WmReflectNotify(ref Message m)
        {
            System.Windows.Forms.NativeMethods.LVHITTESTINFO lvhittestinfo2;
            System.Windows.Forms.NativeMethods.NMHDR* lParam = (System.Windows.Forms.NativeMethods.NMHDR*) m.LParam;
            switch (lParam->code)
            {
                case -176:
                case -106:
                {
                    this.listViewState[0x4000] = false;
                    System.Windows.Forms.NativeMethods.NMLVDISPINFO nmlvdispinfo = (System.Windows.Forms.NativeMethods.NMLVDISPINFO) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMLVDISPINFO));
                    LabelEditEventArgs e = new LabelEditEventArgs(nmlvdispinfo.item.iItem, nmlvdispinfo.item.pszText);
                    this.OnAfterLabelEdit(e);
                    m.Result = e.CancelEdit ? IntPtr.Zero : ((IntPtr) 1);
                    if (!e.CancelEdit && (nmlvdispinfo.item.pszText != null))
                    {
                        this.Items[nmlvdispinfo.item.iItem].Text = nmlvdispinfo.item.pszText;
                    }
                    return;
                }
                case -175:
                case -105:
                {
                    System.Windows.Forms.NativeMethods.NMLVDISPINFO_NOTEXT nmlvdispinfo_notext = (System.Windows.Forms.NativeMethods.NMLVDISPINFO_NOTEXT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMLVDISPINFO_NOTEXT));
                    LabelEditEventArgs args = new LabelEditEventArgs(nmlvdispinfo_notext.item.iItem);
                    this.OnBeforeLabelEdit(args);
                    m.Result = args.CancelEdit ? ((IntPtr) 1) : IntPtr.Zero;
                    this.listViewState[0x4000] = !args.CancelEdit;
                    return;
                }
                case -155:
                    if (this.CheckBoxes)
                    {
                        System.Windows.Forms.NativeMethods.NMLVKEYDOWN nmlvkeydown = (System.Windows.Forms.NativeMethods.NMLVKEYDOWN) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMLVKEYDOWN));
                        if (nmlvkeydown.wVKey == 0x20)
                        {
                            ListViewItem focusedItem = this.FocusedItem;
                            if (focusedItem != null)
                            {
                                bool flag = !focusedItem.Checked;
                                if (!this.VirtualMode)
                                {
                                    foreach (ListViewItem item2 in this.SelectedItems)
                                    {
                                        if (item2 != focusedItem)
                                        {
                                            item2.Checked = flag;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return;

                case -114:
                    this.OnItemActivate(EventArgs.Empty);
                    return;

                case -113:
                {
                    System.Windows.Forms.NativeMethods.NMLVCACHEHINT nmlvcachehint = (System.Windows.Forms.NativeMethods.NMLVCACHEHINT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMLVCACHEHINT));
                    this.OnCacheVirtualItems(new CacheVirtualItemsEventArgs(nmlvcachehint.iFrom, nmlvcachehint.iTo));
                    return;
                }
                case -111:
                    if (!this.ItemCollectionChangedInMouseDown)
                    {
                        System.Windows.Forms.NativeMethods.NMLISTVIEW nmlv = (System.Windows.Forms.NativeMethods.NMLISTVIEW) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMLISTVIEW));
                        this.LvnBeginDrag(MouseButtons.Right, nmlv);
                    }
                    return;

                case -109:
                    if (!this.ItemCollectionChangedInMouseDown)
                    {
                        System.Windows.Forms.NativeMethods.NMLISTVIEW nmlistview2 = (System.Windows.Forms.NativeMethods.NMLISTVIEW) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMLISTVIEW));
                        this.LvnBeginDrag(MouseButtons.Left, nmlistview2);
                    }
                    return;

                case -108:
                {
                    System.Windows.Forms.NativeMethods.NMLISTVIEW nmlistview = (System.Windows.Forms.NativeMethods.NMLISTVIEW) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMLISTVIEW));
                    this.listViewState[0x20000] = true;
                    this.columnIndex = nmlistview.iSubItem;
                    return;
                }
                case -101:
                {
                    System.Windows.Forms.NativeMethods.NMLISTVIEW* nmlistviewPtr2 = (System.Windows.Forms.NativeMethods.NMLISTVIEW*) m.LParam;
                    if ((nmlistviewPtr2->uChanged & 8) != 0)
                    {
                        CheckState state3 = (((nmlistviewPtr2->uOldState & 0xf000) >> 12) == 1) ? CheckState.Unchecked : CheckState.Checked;
                        CheckState state4 = (((nmlistviewPtr2->uNewState & 0xf000) >> 12) == 1) ? CheckState.Unchecked : CheckState.Checked;
                        if (state4 != state3)
                        {
                            ItemCheckedEventArgs args4 = new ItemCheckedEventArgs(this.Items[nmlistviewPtr2->iItem]);
                            this.OnItemChecked(args4);
                        }
                        int num = nmlistviewPtr2->uOldState & 2;
                        int num2 = nmlistviewPtr2->uNewState & 2;
                        if (num2 == num)
                        {
                            return;
                        }
                        if (this.VirtualMode && (nmlistviewPtr2->iItem == -1))
                        {
                            if (this.VirtualListSize > 0)
                            {
                                ListViewVirtualItemsSelectionRangeChangedEventArgs args5 = new ListViewVirtualItemsSelectionRangeChangedEventArgs(0, this.VirtualListSize - 1, num2 != 0);
                                this.OnVirtualItemsSelectionRangeChanged(args5);
                            }
                        }
                        else if (this.Items.Count > 0)
                        {
                            ListViewItemSelectionChangedEventArgs args6 = new ListViewItemSelectionChangedEventArgs(this.Items[nmlistviewPtr2->iItem], nmlistviewPtr2->iItem, num2 != 0);
                            this.OnItemSelectionChanged(args6);
                        }
                        if ((this.Items.Count == 0) || (this.Items[this.Items.Count - 1] != null))
                        {
                            this.listViewState1[0x10] = false;
                            this.OnSelectedIndexChanged(EventArgs.Empty);
                            return;
                        }
                        this.listViewState1[0x10] = true;
                    }
                    return;
                }
                case -100:
                {
                    System.Windows.Forms.NativeMethods.NMLISTVIEW* nmlistviewPtr = (System.Windows.Forms.NativeMethods.NMLISTVIEW*) m.LParam;
                    if ((nmlistviewPtr->uChanged & 8) != 0)
                    {
                        CheckState currentValue = (((nmlistviewPtr->uOldState & 0xf000) >> 12) == 1) ? CheckState.Unchecked : CheckState.Checked;
                        CheckState newCheckValue = (((nmlistviewPtr->uNewState & 0xf000) >> 12) == 1) ? CheckState.Unchecked : CheckState.Checked;
                        if (currentValue == newCheckValue)
                        {
                            return;
                        }
                        ItemCheckEventArgs ice = new ItemCheckEventArgs(nmlistviewPtr->iItem, newCheckValue, currentValue);
                        this.OnItemCheck(ice);
                        m.Result = (((ice.NewValue == CheckState.Unchecked) ? CheckState.Unchecked : CheckState.Checked) == currentValue) ? ((IntPtr) 1) : IntPtr.Zero;
                    }
                    return;
                }
                case -12:
                    this.CustomDraw(ref m);
                    return;

                case -6:
                    goto Label_0517;

                case -5:
                    break;

                case -3:
                    this.WmNmDblClick(ref m);
                    goto Label_0517;

                case -2:
                    this.WmNmClick(ref m);
                    break;

                default:
                    if (lParam->code == System.Windows.Forms.NativeMethods.LVN_GETDISPINFO)
                    {
                        if (this.VirtualMode && (m.LParam != IntPtr.Zero))
                        {
                            System.Windows.Forms.NativeMethods.NMLVDISPINFO_NOTEXT structure = (System.Windows.Forms.NativeMethods.NMLVDISPINFO_NOTEXT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMLVDISPINFO_NOTEXT));
                            RetrieveVirtualItemEventArgs args7 = new RetrieveVirtualItemEventArgs(structure.item.iItem);
                            this.OnRetrieveVirtualItem(args7);
                            ListViewItem item = args7.Item;
                            if (item == null)
                            {
                                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewVirtualItemRequired"));
                            }
                            item.SetItemIndex(this, structure.item.iItem);
                            if ((structure.item.mask & 1) != 0)
                            {
                                string text;
                                if (structure.item.iSubItem == 0)
                                {
                                    text = item.Text;
                                }
                                else
                                {
                                    if (item.SubItems.Count <= structure.item.iSubItem)
                                    {
                                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewVirtualModeCantAccessSubItem"));
                                    }
                                    text = item.SubItems[structure.item.iSubItem].Text;
                                }
                                if (structure.item.cchTextMax <= text.Length)
                                {
                                    text = text.Substring(0, structure.item.cchTextMax - 1);
                                }
                                if (Marshal.SystemDefaultCharSize == 1)
                                {
                                    Marshal.Copy(Encoding.Default.GetBytes(text + "\0"), 0, structure.item.pszText, text.Length + 1);
                                }
                                else
                                {
                                    Marshal.Copy((text + "\0").ToCharArray(), 0, structure.item.pszText, text.Length + 1);
                                }
                            }
                            if (((structure.item.mask & 2) != 0) && (item.ImageIndex != -1))
                            {
                                structure.item.iImage = item.ImageIndex;
                            }
                            if ((structure.item.mask & 0x10) != 0)
                            {
                                structure.item.iIndent = item.IndentCount;
                            }
                            if ((structure.item.stateMask & 0xf000) != 0)
                            {
                                structure.item.state |= item.RawStateImageIndex;
                            }
                            Marshal.StructureToPtr(structure, m.LParam, false);
                            return;
                        }
                    }
                    else if (lParam->code == -115)
                    {
                        if (this.VirtualMode && (m.LParam != IntPtr.Zero))
                        {
                            System.Windows.Forms.NativeMethods.NMLVODSTATECHANGE nmlvodstatechange = (System.Windows.Forms.NativeMethods.NMLVODSTATECHANGE) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMLVODSTATECHANGE));
                            if ((nmlvodstatechange.uNewState & 2) != (nmlvodstatechange.uOldState & 2))
                            {
                                ListViewVirtualItemsSelectionRangeChangedEventArgs args8 = new ListViewVirtualItemsSelectionRangeChangedEventArgs(nmlvodstatechange.iFrom, nmlvodstatechange.iTo - 1, (nmlvodstatechange.uNewState & 2) != 0);
                                this.OnVirtualItemsSelectionRangeChanged(args8);
                                return;
                            }
                        }
                    }
                    else if (lParam->code == System.Windows.Forms.NativeMethods.LVN_GETINFOTIP)
                    {
                        if (this.ShowItemToolTips && (m.LParam != IntPtr.Zero))
                        {
                            System.Windows.Forms.NativeMethods.NMLVGETINFOTIP nmlvgetinfotip = (System.Windows.Forms.NativeMethods.NMLVGETINFOTIP) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMLVGETINFOTIP));
                            ListViewItem item4 = this.Items[nmlvgetinfotip.item];
                            if ((item4 != null) && !string.IsNullOrEmpty(item4.ToolTipText))
                            {
                                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, lParam->hwndFrom), 0x418, 0, SystemInformation.MaxWindowTrackSize.Width);
                                if (Marshal.SystemDefaultCharSize == 1)
                                {
                                    byte[] bytes = Encoding.Default.GetBytes(item4.ToolTipText + "\0");
                                    Marshal.Copy(bytes, 0, nmlvgetinfotip.lpszText, Math.Min(bytes.Length, nmlvgetinfotip.cchTextMax));
                                }
                                else
                                {
                                    char[] source = (item4.ToolTipText + "\0").ToCharArray();
                                    Marshal.Copy(source, 0, nmlvgetinfotip.lpszText, Math.Min(source.Length, nmlvgetinfotip.cchTextMax));
                                }
                                Marshal.StructureToPtr(nmlvgetinfotip, m.LParam, false);
                                return;
                            }
                        }
                    }
                    else if ((lParam->code == System.Windows.Forms.NativeMethods.LVN_ODFINDITEM) && this.VirtualMode)
                    {
                        System.Windows.Forms.NativeMethods.NMLVFINDITEM nmlvfinditem = (System.Windows.Forms.NativeMethods.NMLVFINDITEM) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMLVFINDITEM));
                        if ((nmlvfinditem.lvfi.flags & 1) != 0)
                        {
                            m.Result = (IntPtr) (-1);
                            return;
                        }
                        bool isTextSearch = ((nmlvfinditem.lvfi.flags & 2) != 0) || ((nmlvfinditem.lvfi.flags & 8) != 0);
                        bool isPrefixSearch = (nmlvfinditem.lvfi.flags & 8) != 0;
                        string psz = string.Empty;
                        if (isTextSearch)
                        {
                            psz = nmlvfinditem.lvfi.psz;
                        }
                        Point empty = Point.Empty;
                        if ((nmlvfinditem.lvfi.flags & 0x40) != 0)
                        {
                            empty = new Point(nmlvfinditem.lvfi.ptX, nmlvfinditem.lvfi.ptY);
                        }
                        SearchDirectionHint down = SearchDirectionHint.Down;
                        if ((nmlvfinditem.lvfi.flags & 0x40) != 0)
                        {
                            down = (SearchDirectionHint) nmlvfinditem.lvfi.vkDirection;
                        }
                        if (nmlvfinditem.iStart >= this.VirtualListSize)
                        {
                        }
                        SearchForVirtualItemEventArgs args9 = new SearchForVirtualItemEventArgs(isTextSearch, isPrefixSearch, false, psz, empty, down, nmlvfinditem.iStart);
                        this.OnSearchForVirtualItem(args9);
                        if (args9.Index != -1)
                        {
                            m.Result = (IntPtr) args9.Index;
                            return;
                        }
                        m.Result = (IntPtr) (-1);
                    }
                    return;
            }
            System.Windows.Forms.NativeMethods.LVHITTESTINFO lvhi = new System.Windows.Forms.NativeMethods.LVHITTESTINFO();
            int indexOfClickedItem = this.GetIndexOfClickedItem(lvhi);
            MouseButtons button = (lParam->code == -2) ? MouseButtons.Left : MouseButtons.Right;
            Point position = Cursor.Position;
            position = base.PointToClientInternal(position);
            if (!base.ValidationCancelled && (indexOfClickedItem != -1))
            {
                this.OnClick(EventArgs.Empty);
                this.OnMouseClick(new MouseEventArgs(button, 1, position.X, position.Y, 0));
            }
            if (!this.listViewState[0x80000])
            {
                this.OnMouseUp(new MouseEventArgs(button, 1, position.X, position.Y, 0));
                this.listViewState[0x80000] = true;
            }
            return;
        Label_0517:
            lvhittestinfo2 = new System.Windows.Forms.NativeMethods.LVHITTESTINFO();
            if (this.GetIndexOfClickedItem(lvhittestinfo2) != -1)
            {
                this.listViewState[0x40000] = true;
            }
            this.listViewState[0x80000] = false;
            base.CaptureInternal = true;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x4e:
                    if (!this.WmNotify(ref m))
                    {
                        break;
                    }
                    return;

                case 0x113:
                    if ((((int) ((long) m.WParam)) != 0x30) || !this.ComctlSupportsVisualStyles)
                    {
                        base.WndProc(ref m);
                    }
                    return;

                case 7:
                    base.WndProc(ref m);
                    if ((!base.RecreatingHandle && !this.ListViewHandleDestroyed) && ((this.FocusedItem == null) && (this.Items.Count > 0)))
                    {
                        this.Items[0].Focused = true;
                    }
                    return;

                case 15:
                    base.WndProc(ref m);
                    base.BeginInvoke(new MethodInvoker(this.CleanPreviousBackgroundImageFiles));
                    return;

                case 0x200:
                    if ((this.listViewState[0x100000] && !this.listViewState[0x80000]) && (Control.MouseButtons == MouseButtons.None))
                    {
                        this.OnMouseUp(new MouseEventArgs(this.downButton, 1, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                        this.listViewState[0x80000] = true;
                    }
                    base.CaptureInternal = false;
                    base.WndProc(ref m);
                    return;

                case 0x201:
                    this.ItemCollectionChangedInMouseDown = false;
                    this.WmMouseDown(ref m, MouseButtons.Left, 1);
                    this.downButton = MouseButtons.Left;
                    return;

                case 0x202:
                case 0x205:
                case 520:
                {
                    System.Windows.Forms.NativeMethods.LVHITTESTINFO lvhi = new System.Windows.Forms.NativeMethods.LVHITTESTINFO();
                    int indexOfClickedItem = this.GetIndexOfClickedItem(lvhi);
                    if ((!base.ValidationCancelled && this.listViewState[0x40000]) && (indexOfClickedItem != -1))
                    {
                        this.listViewState[0x40000] = false;
                        this.OnDoubleClick(EventArgs.Empty);
                        this.OnMouseDoubleClick(new MouseEventArgs(this.downButton, 2, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                    }
                    if (!this.listViewState[0x80000])
                    {
                        this.OnMouseUp(new MouseEventArgs(this.downButton, 1, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                        this.listViewState[0x100000] = false;
                    }
                    this.ItemCollectionChangedInMouseDown = false;
                    this.listViewState[0x80000] = true;
                    base.CaptureInternal = false;
                    return;
                }
                case 0x203:
                    this.ItemCollectionChangedInMouseDown = false;
                    base.CaptureInternal = true;
                    this.WmMouseDown(ref m, MouseButtons.Left, 2);
                    return;

                case 0x204:
                    this.WmMouseDown(ref m, MouseButtons.Right, 1);
                    this.downButton = MouseButtons.Right;
                    return;

                case 0x206:
                    this.WmMouseDown(ref m, MouseButtons.Right, 2);
                    return;

                case 0x207:
                    this.WmMouseDown(ref m, MouseButtons.Middle, 1);
                    this.downButton = MouseButtons.Middle;
                    return;

                case 0x209:
                    this.WmMouseDown(ref m, MouseButtons.Middle, 2);
                    return;

                case 0x2a1:
                    if (!this.HoverSelection)
                    {
                        this.OnMouseHover(EventArgs.Empty);
                        return;
                    }
                    base.WndProc(ref m);
                    return;

                case 0x2a3:
                    this.prevHoveredItem = null;
                    base.WndProc(ref m);
                    return;

                case 0x317:
                    this.WmPrint(ref m);
                    return;

                case 0x204e:
                    this.WmReflectNotify(ref m);
                    return;
            }
            base.WndProc(ref m);
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(0), System.Windows.Forms.SRDescription("ListViewActivationDescr")]
        public ItemActivation Activation
        {
            get
            {
                return this.activation;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ItemActivation));
                }
                if (this.HotTracking && (value != ItemActivation.OneClick))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ListViewActivationMustBeOnWhenHotTrackingIsOn"), "value");
                }
                if (this.activation != value)
                {
                    this.activation = value;
                    this.UpdateExtendedStyles();
                }
            }
        }

        [System.Windows.Forms.SRDescription("ListViewAlignmentDescr"), DefaultValue(2), Localizable(true), System.Windows.Forms.SRCategory("CatBehavior")]
        public ListViewAlignment Alignment
        {
            get
            {
                return this.alignStyle;
            }
            set
            {
                int[] enumValues = new int[4];
                enumValues[1] = 2;
                enumValues[2] = 1;
                enumValues[3] = 5;
                if (!System.Windows.Forms.ClientUtils.IsEnumValid_NotSequential(value, (int) value, enumValues))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ListViewAlignment));
                }
                if (this.alignStyle != value)
                {
                    this.alignStyle = value;
                    this.RecreateHandleInternal();
                }
            }
        }

        [System.Windows.Forms.SRDescription("ListViewAllowColumnReorderDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool AllowColumnReorder
        {
            get
            {
                return this.listViewState[2];
            }
            set
            {
                if (this.AllowColumnReorder != value)
                {
                    this.listViewState[2] = value;
                    this.UpdateExtendedStyles();
                }
            }
        }

        [System.Windows.Forms.SRDescription("ListViewAutoArrangeDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true)]
        public bool AutoArrange
        {
            get
            {
                return this.listViewState[4];
            }
            set
            {
                if (this.AutoArrange != value)
                {
                    this.listViewState[4] = value;
                    base.UpdateStyles();
                }
            }
        }

        public override Color BackColor
        {
            get
            {
                if (this.ShouldSerializeBackColor())
                {
                    return base.BackColor;
                }
                return SystemColors.Window;
            }
            set
            {
                base.BackColor = value;
                if (base.IsHandleCreated)
                {
                    base.SendMessage(0x1001, 0, ColorTranslator.ToWin32(this.BackColor));
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(false), System.Windows.Forms.SRDescription("ListViewBackgroundImageTiledDescr")]
        public bool BackgroundImageTiled
        {
            get
            {
                return this.listViewState[0x10000];
            }
            set
            {
                if (this.BackgroundImageTiled != value)
                {
                    this.listViewState[0x10000] = value;
                    if (base.IsHandleCreated && (this.BackgroundImage != null))
                    {
                        System.Windows.Forms.NativeMethods.LVBKIMAGE lParam = new System.Windows.Forms.NativeMethods.LVBKIMAGE {
                            xOffset = 0,
                            yOffset = 0
                        };
                        if (this.BackgroundImageTiled)
                        {
                            lParam.ulFlags = 0x10;
                        }
                        else
                        {
                            lParam.ulFlags = 0;
                        }
                        lParam.ulFlags |= 2;
                        lParam.pszImage = this.backgroundImageFileName;
                        lParam.cchImageMax = this.backgroundImageFileName.Length + 1;
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.LVM_SETBKIMAGE, 0, lParam);
                    }
                }
            }
        }

        [DefaultValue(2), System.Windows.Forms.SRDescription("borderStyleDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DispId(-504)]
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
                    base.UpdateStyles();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ListViewCheckBoxesDescr"), DefaultValue(false)]
        public bool CheckBoxes
        {
            get
            {
                return this.listViewState[8];
            }
            set
            {
                if (this.UseCompatibleStateImageBehavior)
                {
                    if (this.CheckBoxes != value)
                    {
                        if (value && (this.View == System.Windows.Forms.View.Tile))
                        {
                            throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListViewCheckBoxesNotSupportedInTileView"));
                        }
                        if (this.CheckBoxes)
                        {
                            this.savedCheckedItems = new List<ListViewItem>(this.CheckedItems.Count);
                            ListViewItem[] dest = new ListViewItem[this.CheckedItems.Count];
                            this.CheckedItems.CopyTo(dest, 0);
                            for (int i = 0; i < dest.Length; i++)
                            {
                                this.savedCheckedItems.Add(dest[i]);
                            }
                        }
                        this.listViewState[8] = value;
                        this.UpdateExtendedStyles();
                        if (this.CheckBoxes && (this.savedCheckedItems != null))
                        {
                            if (this.savedCheckedItems.Count > 0)
                            {
                                foreach (ListViewItem item in this.savedCheckedItems)
                                {
                                    item.Checked = true;
                                }
                            }
                            this.savedCheckedItems = null;
                        }
                        if (this.AutoArrange)
                        {
                            this.ArrangeIcons(this.Alignment);
                        }
                    }
                }
                else if (this.CheckBoxes != value)
                {
                    if (value && (this.View == System.Windows.Forms.View.Tile))
                    {
                        throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListViewCheckBoxesNotSupportedInTileView"));
                    }
                    if (this.CheckBoxes)
                    {
                        this.savedCheckedItems = new List<ListViewItem>(this.CheckedItems.Count);
                        ListViewItem[] itemArray2 = new ListViewItem[this.CheckedItems.Count];
                        this.CheckedItems.CopyTo(itemArray2, 0);
                        for (int j = 0; j < itemArray2.Length; j++)
                        {
                            this.savedCheckedItems.Add(itemArray2[j]);
                        }
                    }
                    this.listViewState[8] = value;
                    if ((((!value && (this.StateImageList != null)) && base.IsHandleCreated) || ((!value && (this.Alignment == ListViewAlignment.Left)) && base.IsHandleCreated)) || (((value && (this.View == System.Windows.Forms.View.List)) && base.IsHandleCreated) || ((value && ((this.View == System.Windows.Forms.View.SmallIcon) || (this.View == System.Windows.Forms.View.LargeIcon))) && base.IsHandleCreated)))
                    {
                        this.RecreateHandleInternal();
                    }
                    else
                    {
                        this.UpdateExtendedStyles();
                    }
                    if (this.CheckBoxes && (this.savedCheckedItems != null))
                    {
                        if (this.savedCheckedItems.Count > 0)
                        {
                            foreach (ListViewItem item2 in this.savedCheckedItems)
                            {
                                item2.Checked = true;
                            }
                        }
                        this.savedCheckedItems = null;
                    }
                    if (base.IsHandleCreated && (this.imageListState != null))
                    {
                        if (this.CheckBoxes)
                        {
                            base.SendMessage(0x1003, 2, this.imageListState.Handle);
                        }
                        else
                        {
                            base.SendMessage(0x1003, 2, IntPtr.Zero);
                        }
                    }
                    if (this.AutoArrange)
                    {
                        this.ArrangeIcons(this.Alignment);
                    }
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CheckedIndexCollection CheckedIndices
        {
            get
            {
                if (this.checkedIndexCollection == null)
                {
                    this.checkedIndexCollection = new CheckedIndexCollection(this);
                }
                return this.checkedIndexCollection;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public CheckedListViewItemCollection CheckedItems
        {
            get
            {
                if (this.checkedListViewItemCollection == null)
                {
                    this.checkedListViewItemCollection = new CheckedListViewItemCollection(this);
                }
                return this.checkedListViewItemCollection;
            }
        }

        [MergableProperty(false), System.Windows.Forms.SRDescription("ListViewColumnsDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatBehavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Editor("System.Windows.Forms.Design.ColumnHeaderCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public ColumnHeaderCollection Columns
        {
            get
            {
                return this.columnHeaderCollection;
            }
        }

        private bool ComctlSupportsVisualStyles
        {
            get
            {
                if (!this.listViewState[0x400000])
                {
                    this.listViewState[0x400000] = true;
                    this.listViewState[0x200000] = Application.ComCtlSupportsVisualStyles;
                }
                return this.listViewState[0x200000];
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ClassName = "SysListView32";
                if (base.IsHandleCreated)
                {
                    int windowLong = (int) ((long) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, base.Handle), -16));
                    createParams.Style |= windowLong & 0x300000;
                }
                createParams.Style |= 0x40;
                switch (this.alignStyle)
                {
                    case ListViewAlignment.Left:
                        createParams.Style |= 0x800;
                        break;

                    case ListViewAlignment.Top:
                        createParams.Style = createParams.Style;
                        break;
                }
                if (this.AutoArrange)
                {
                    createParams.Style |= 0x100;
                }
                switch (this.borderStyle)
                {
                    case System.Windows.Forms.BorderStyle.FixedSingle:
                        createParams.Style |= 0x800000;
                        break;

                    case System.Windows.Forms.BorderStyle.Fixed3D:
                        createParams.ExStyle |= 0x200;
                        break;
                }
                switch (this.headerStyle)
                {
                    case ColumnHeaderStyle.None:
                        createParams.Style |= 0x4000;
                        break;

                    case ColumnHeaderStyle.Nonclickable:
                        createParams.Style |= 0x8000;
                        break;
                }
                if (this.LabelEdit)
                {
                    createParams.Style |= 0x200;
                }
                if (!this.LabelWrap)
                {
                    createParams.Style |= 0x80;
                }
                if (!this.HideSelection)
                {
                    createParams.Style |= 8;
                }
                if (!this.MultiSelect)
                {
                    createParams.Style |= 4;
                }
                if (this.listItemSorter == null)
                {
                    switch (this.sorting)
                    {
                        case SortOrder.Ascending:
                            createParams.Style |= 0x10;
                            break;

                        case SortOrder.Descending:
                            createParams.Style |= 0x20;
                            break;
                    }
                }
                if (this.VirtualMode)
                {
                    createParams.Style |= 0x1000;
                }
                if (this.viewStyle != System.Windows.Forms.View.Tile)
                {
                    createParams.Style |= this.viewStyle;
                }
                if ((this.RightToLeft == RightToLeft.Yes) && this.RightToLeftLayout)
                {
                    createParams.ExStyle |= 0x400000;
                    createParams.ExStyle &= -28673;
                }
                return createParams;
            }
        }

        internal ListViewGroup DefaultGroup
        {
            get
            {
                if (this.defaultGroup == null)
                {
                    this.defaultGroup = new ListViewGroup(System.Windows.Forms.SR.GetString("ListViewGroupDefaultGroup", new object[] { "1" }));
                }
                return this.defaultGroup;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(0x79, 0x61);
            }
        }

        protected override bool DoubleBuffered
        {
            get
            {
                return base.DoubleBuffered;
            }
            set
            {
                if (this.DoubleBuffered != value)
                {
                    base.DoubleBuffered = value;
                    this.UpdateExtendedStyles();
                }
            }
        }

        internal bool ExpectingMouseUp
        {
            get
            {
                return this.listViewState[0x100000];
            }
        }

        private bool FlipViewToLargeIconAndSmallIcon
        {
            get
            {
                return this.listViewState[0x10000000];
            }
            set
            {
                this.listViewState[0x10000000] = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ListViewFocusedItemDescr"), Browsable(false)]
        public ListViewItem FocusedItem
        {
            get
            {
                if (base.IsHandleCreated)
                {
                    int num = (int) ((long) base.SendMessage(0x100c, -1, 1));
                    if (num > -1)
                    {
                        return this.Items[num];
                    }
                }
                return null;
            }
            set
            {
                if (base.IsHandleCreated && (value != null))
                {
                    value.Focused = true;
                }
            }
        }

        public override Color ForeColor
        {
            get
            {
                if (this.ShouldSerializeForeColor())
                {
                    return base.ForeColor;
                }
                return SystemColors.WindowText;
            }
            set
            {
                base.ForeColor = value;
                if (base.IsHandleCreated)
                {
                    base.SendMessage(0x1024, 0, ColorTranslator.ToWin32(this.ForeColor));
                }
            }
        }

        [System.Windows.Forms.SRDescription("ListViewFullRowSelectDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(false)]
        public bool FullRowSelect
        {
            get
            {
                return this.listViewState[0x10];
            }
            set
            {
                if (this.FullRowSelect != value)
                {
                    this.listViewState[0x10] = value;
                    this.UpdateExtendedStyles();
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ListViewGridLinesDescr")]
        public bool GridLines
        {
            get
            {
                return this.listViewState[0x20];
            }
            set
            {
                if (this.GridLines != value)
                {
                    this.listViewState[0x20] = value;
                    this.UpdateExtendedStyles();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), Editor("System.Windows.Forms.Design.ListViewGroupCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), MergableProperty(false), Localizable(true), System.Windows.Forms.SRDescription("ListViewGroupsDescr")]
        public ListViewGroupCollection Groups
        {
            get
            {
                if (this.groups == null)
                {
                    this.groups = new ListViewGroupCollection(this);
                }
                return this.groups;
            }
        }

        internal bool GroupsEnabled
        {
            get
            {
                return (((this.ShowGroups && (this.groups != null)) && ((this.groups.Count > 0) && this.ComctlSupportsVisualStyles)) && !this.VirtualMode);
            }
        }

        [DefaultValue(2), System.Windows.Forms.SRDescription("ListViewHeaderStyleDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public ColumnHeaderStyle HeaderStyle
        {
            get
            {
                return this.headerStyle;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ColumnHeaderStyle));
                }
                if (this.headerStyle != value)
                {
                    this.headerStyle = value;
                    if ((this.listViewState[0x2000] && (value == ColumnHeaderStyle.Clickable)) || (!this.listViewState[0x2000] && (value == ColumnHeaderStyle.Nonclickable)))
                    {
                        this.listViewState[0x2000] = !this.listViewState[0x2000];
                        this.RecreateHandleInternal();
                    }
                    else
                    {
                        base.UpdateStyles();
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("ListViewHideSelectionDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true)]
        public bool HideSelection
        {
            get
            {
                return this.listViewState[0x40];
            }
            set
            {
                if (this.HideSelection != value)
                {
                    this.listViewState[0x40] = value;
                    base.UpdateStyles();
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ListViewHotTrackingDescr")]
        public bool HotTracking
        {
            get
            {
                return this.listViewState[0x80];
            }
            set
            {
                if (this.HotTracking != value)
                {
                    this.listViewState[0x80] = value;
                    if (value)
                    {
                        this.HoverSelection = true;
                        this.Activation = ItemActivation.OneClick;
                    }
                    this.UpdateExtendedStyles();
                }
            }
        }

        [System.Windows.Forms.SRDescription("ListViewHoverSelectDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool HoverSelection
        {
            get
            {
                return this.listViewState[0x1000];
            }
            set
            {
                if (this.HoverSelection != value)
                {
                    if (this.HotTracking && !value)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("ListViewHoverMustBeOnWhenHotTrackingIsOn"), "value");
                    }
                    this.listViewState[0x1000] = value;
                    this.UpdateExtendedStyles();
                }
            }
        }

        internal bool InsertingItemsNatively
        {
            get
            {
                return this.listViewState1[1];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), System.Windows.Forms.SRDescription("ListViewInsertionMarkDescr")]
        public ListViewInsertionMark InsertionMark
        {
            get
            {
                if (this.insertionMark == null)
                {
                    this.insertionMark = new ListViewInsertionMark(this);
                }
                return this.insertionMark;
            }
        }

        private bool ItemCollectionChangedInMouseDown
        {
            get
            {
                return this.listViewState[0x8000000];
            }
            set
            {
                this.listViewState[0x8000000] = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), Editor("System.Windows.Forms.Design.ListViewItemCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Localizable(true), System.Windows.Forms.SRDescription("ListViewItemsDescr")]
        public ListViewItemCollection Items
        {
            get
            {
                return this.listItemCollection;
            }
        }

        [System.Windows.Forms.SRDescription("ListViewLabelEditDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool LabelEdit
        {
            get
            {
                return this.listViewState[0x100];
            }
            set
            {
                if (this.LabelEdit != value)
                {
                    this.listViewState[0x100] = value;
                    base.UpdateStyles();
                }
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true), System.Windows.Forms.SRDescription("ListViewLabelWrapDescr")]
        public bool LabelWrap
        {
            get
            {
                return this.listViewState[0x200];
            }
            set
            {
                if (this.LabelWrap != value)
                {
                    this.listViewState[0x200] = value;
                    base.UpdateStyles();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ListViewLargeImageListDescr"), DefaultValue((string) null)]
        public ImageList LargeImageList
        {
            get
            {
                return this.imageListLarge;
            }
            set
            {
                if (value != this.imageListLarge)
                {
                    EventHandler handler = new EventHandler(this.LargeImageListRecreateHandle);
                    EventHandler handler2 = new EventHandler(this.DetachImageList);
                    EventHandler handler3 = new EventHandler(this.LargeImageListChangedHandle);
                    if (this.imageListLarge != null)
                    {
                        this.imageListLarge.RecreateHandle -= handler;
                        this.imageListLarge.Disposed -= handler2;
                        this.imageListLarge.ChangeHandle -= handler3;
                    }
                    this.imageListLarge = value;
                    if (value != null)
                    {
                        value.RecreateHandle += handler;
                        value.Disposed += handler2;
                        value.ChangeHandle += handler3;
                    }
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x1003, IntPtr.Zero, (value == null) ? IntPtr.Zero : value.Handle);
                        if (this.AutoArrange && !this.listViewState1[4])
                        {
                            this.UpdateListViewItemsLocations();
                        }
                    }
                }
            }
        }

        internal bool ListViewHandleDestroyed
        {
            get
            {
                return this.listViewState[0x1000000];
            }
            set
            {
                this.listViewState[0x1000000] = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ListViewItemSorterDescr")]
        public IComparer ListViewItemSorter
        {
            get
            {
                return this.listItemSorter;
            }
            set
            {
                if (this.listItemSorter != value)
                {
                    this.listItemSorter = value;
                    if (!this.VirtualMode)
                    {
                        this.Sort();
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), System.Windows.Forms.SRDescription("ListViewMultiSelectDescr")]
        public bool MultiSelect
        {
            get
            {
                return this.listViewState[0x400];
            }
            set
            {
                if (this.MultiSelect != value)
                {
                    this.listViewState[0x400] = value;
                    base.UpdateStyles();
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("ListViewOwnerDrawDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool OwnerDraw
        {
            get
            {
                return this.listViewState[1];
            }
            set
            {
                if (this.OwnerDraw != value)
                {
                    this.listViewState[1] = value;
                    base.Invalidate(true);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public System.Windows.Forms.Padding Padding
        {
            get
            {
                return base.Padding;
            }
            set
            {
                base.Padding = value;
            }
        }

        [Localizable(true), DefaultValue(false), System.Windows.Forms.SRDescription("ControlRightToLeftLayoutDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public virtual bool RightToLeftLayout
        {
            get
            {
                return this.rightToLeftLayout;
            }
            set
            {
                if (value != this.rightToLeftLayout)
                {
                    this.rightToLeftLayout = value;
                    using (new LayoutTransaction(this, this, PropertyNames.RightToLeftLayout))
                    {
                        this.OnRightToLeftLayoutChanged(EventArgs.Empty);
                    }
                }
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ListViewScrollableDescr")]
        public bool Scrollable
        {
            get
            {
                return this.listViewState[0x800];
            }
            set
            {
                if (this.Scrollable != value)
                {
                    this.listViewState[0x800] = value;
                    this.RecreateHandleInternal();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public SelectedIndexCollection SelectedIndices
        {
            get
            {
                if (this.selectedIndexCollection == null)
                {
                    this.selectedIndexCollection = new SelectedIndexCollection(this);
                }
                return this.selectedIndexCollection;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ListViewSelectedItemsDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public SelectedListViewItemCollection SelectedItems
        {
            get
            {
                if (this.selectedListViewItemCollection == null)
                {
                    this.selectedListViewItemCollection = new SelectedListViewItemCollection(this);
                }
                return this.selectedListViewItemCollection;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), System.Windows.Forms.SRDescription("ListViewShowGroupsDescr")]
        public bool ShowGroups
        {
            get
            {
                return this.listViewState[0x800000];
            }
            set
            {
                if (value != this.ShowGroups)
                {
                    this.listViewState[0x800000] = value;
                    if (base.IsHandleCreated)
                    {
                        this.UpdateGroupView();
                    }
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("ListViewShowItemToolTipsDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool ShowItemToolTips
        {
            get
            {
                return this.listViewState[0x8000];
            }
            set
            {
                if (this.ShowItemToolTips != value)
                {
                    this.listViewState[0x8000] = value;
                    this.RecreateHandleInternal();
                }
            }
        }

        [System.Windows.Forms.SRDescription("ListViewSmallImageListDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue((string) null)]
        public ImageList SmallImageList
        {
            get
            {
                return this.imageListSmall;
            }
            set
            {
                if (this.imageListSmall != value)
                {
                    EventHandler handler = new EventHandler(this.SmallImageListRecreateHandle);
                    EventHandler handler2 = new EventHandler(this.DetachImageList);
                    if (this.imageListSmall != null)
                    {
                        this.imageListSmall.RecreateHandle -= handler;
                        this.imageListSmall.Disposed -= handler2;
                    }
                    this.imageListSmall = value;
                    if (value != null)
                    {
                        value.RecreateHandle += handler;
                        value.Disposed += handler2;
                    }
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x1003, (IntPtr) 1, (value == null) ? IntPtr.Zero : value.Handle);
                        if (this.View == System.Windows.Forms.View.SmallIcon)
                        {
                            this.View = System.Windows.Forms.View.LargeIcon;
                            this.View = System.Windows.Forms.View.SmallIcon;
                        }
                        else if (!this.listViewState1[4])
                        {
                            this.UpdateListViewItemsLocations();
                        }
                        if (this.View == System.Windows.Forms.View.Details)
                        {
                            base.Invalidate(true);
                        }
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ListViewSortingDescr"), DefaultValue(0)]
        public SortOrder Sorting
        {
            get
            {
                return this.sorting;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(SortOrder));
                }
                if (this.sorting != value)
                {
                    this.sorting = value;
                    if ((this.View == System.Windows.Forms.View.LargeIcon) || (this.View == System.Windows.Forms.View.SmallIcon))
                    {
                        if (this.listItemSorter == null)
                        {
                            this.listItemSorter = new IconComparer(this.sorting);
                        }
                        else if (this.listItemSorter is IconComparer)
                        {
                            ((IconComparer) this.listItemSorter).SortOrder = this.sorting;
                        }
                    }
                    else if (value == SortOrder.None)
                    {
                        this.listItemSorter = null;
                    }
                    if (value == SortOrder.None)
                    {
                        base.UpdateStyles();
                    }
                    else
                    {
                        this.RecreateHandleInternal();
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ListViewStateImageListDescr"), DefaultValue((string) null)]
        public ImageList StateImageList
        {
            get
            {
                return this.imageListState;
            }
            set
            {
                if (this.UseCompatibleStateImageBehavior)
                {
                    if (this.imageListState != value)
                    {
                        EventHandler handler = new EventHandler(this.StateImageListRecreateHandle);
                        EventHandler handler2 = new EventHandler(this.DetachImageList);
                        if (this.imageListState != null)
                        {
                            this.imageListState.RecreateHandle -= handler;
                            this.imageListState.Disposed -= handler2;
                        }
                        this.imageListState = value;
                        if (value != null)
                        {
                            value.RecreateHandle += handler;
                            value.Disposed += handler2;
                        }
                        if (base.IsHandleCreated)
                        {
                            base.SendMessage(0x1003, 2, (value == null) ? IntPtr.Zero : value.Handle);
                        }
                    }
                }
                else if (this.imageListState != value)
                {
                    EventHandler handler3 = new EventHandler(this.StateImageListRecreateHandle);
                    EventHandler handler4 = new EventHandler(this.DetachImageList);
                    if (this.imageListState != null)
                    {
                        this.imageListState.RecreateHandle -= handler3;
                        this.imageListState.Disposed -= handler4;
                    }
                    if ((base.IsHandleCreated && (this.imageListState != null)) && this.CheckBoxes)
                    {
                        base.SendMessage(0x1003, 2, IntPtr.Zero);
                    }
                    this.imageListState = value;
                    if (value != null)
                    {
                        value.RecreateHandle += handler3;
                        value.Disposed += handler4;
                    }
                    if (base.IsHandleCreated)
                    {
                        if (this.CheckBoxes)
                        {
                            this.RecreateHandleInternal();
                        }
                        else
                        {
                            base.SendMessage(0x1003, 2, ((this.imageListState == null) || (this.imageListState.Images.Count == 0)) ? IntPtr.Zero : this.imageListState.Handle);
                        }
                        if (!this.listViewState1[4])
                        {
                            this.UpdateListViewItemsLocations();
                        }
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), Bindable(false)]
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

        [Browsable(true), System.Windows.Forms.SRDescription("ListViewTileSizeDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public Size TileSize
        {
            get
            {
                if (!this.tileSize.IsEmpty)
                {
                    return this.tileSize;
                }
                if (base.IsHandleCreated)
                {
                    System.Windows.Forms.NativeMethods.LVTILEVIEWINFO lParam = new System.Windows.Forms.NativeMethods.LVTILEVIEWINFO {
                        dwMask = 1
                    };
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x10a3, 0, lParam);
                    return new Size(lParam.sizeTile.cx, lParam.sizeTile.cy);
                }
                return Size.Empty;
            }
            set
            {
                if (this.tileSize != value)
                {
                    if ((value.IsEmpty || (value.Height <= 0)) || (value.Width <= 0))
                    {
                        throw new ArgumentOutOfRangeException("TileSize", System.Windows.Forms.SR.GetString("ListViewTileSizeMustBePositive"));
                    }
                    this.tileSize = value;
                    if (base.IsHandleCreated)
                    {
                        System.Windows.Forms.NativeMethods.LVTILEVIEWINFO lParam = new System.Windows.Forms.NativeMethods.LVTILEVIEWINFO {
                            dwMask = 1,
                            dwFlags = 3,
                            sizeTile = new System.Windows.Forms.NativeMethods.SIZE(this.tileSize.Width, this.tileSize.Height)
                        };
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x10a2, 0, lParam);
                        if (this.AutoArrange)
                        {
                            this.UpdateListViewItemsLocations();
                        }
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ListViewTopItemDescr"), System.Windows.Forms.SRCategory("CatAppearance"), Browsable(false)]
        public ListViewItem TopItem
        {
            get
            {
                if (((this.viewStyle == System.Windows.Forms.View.LargeIcon) || (this.viewStyle == System.Windows.Forms.View.SmallIcon)) || (this.viewStyle == System.Windows.Forms.View.Tile))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewGetTopItem"));
                }
                if (!base.IsHandleCreated)
                {
                    if (this.Items.Count > 0)
                    {
                        return this.Items[0];
                    }
                    return null;
                }
                this.topIndex = (int) ((long) base.SendMessage(0x1027, 0, 0));
                if ((this.topIndex >= 0) && (this.topIndex < this.Items.Count))
                {
                    return this.Items[this.topIndex];
                }
                return null;
            }
            set
            {
                if (((this.viewStyle == System.Windows.Forms.View.LargeIcon) || (this.viewStyle == System.Windows.Forms.View.SmallIcon)) || (this.viewStyle == System.Windows.Forms.View.Tile))
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewSetTopItem"));
                }
                if ((value != null) && (value.ListView == this))
                {
                    if (!base.IsHandleCreated)
                    {
                        this.CreateHandle();
                    }
                    if (value != this.TopItem)
                    {
                        this.EnsureVisible(value.Index);
                        ListViewItem topItem = this.TopItem;
                        if ((topItem == null) && (this.topIndex == this.Items.Count))
                        {
                            topItem = value;
                            if (this.Scrollable)
                            {
                                this.EnsureVisible(0);
                                this.Scroll(0, value.Index);
                            }
                        }
                        else if ((value.Index != topItem.Index) && this.Scrollable)
                        {
                            this.Scroll(topItem.Index, value.Index);
                        }
                    }
                }
            }
        }

        [DefaultValue(true), EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public bool UseCompatibleStateImageBehavior
        {
            get
            {
                return this.listViewState1[8];
            }
            set
            {
                this.listViewState1[8] = value;
            }
        }

        [System.Windows.Forms.SRDescription("ListViewViewDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(0)]
        public System.Windows.Forms.View View
        {
            get
            {
                return this.viewStyle;
            }
            set
            {
                if ((value == System.Windows.Forms.View.Tile) && this.CheckBoxes)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListViewTileViewDoesNotSupportCheckBoxes"));
                }
                this.FlipViewToLargeIconAndSmallIcon = false;
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 4))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.View));
                }
                if ((value == System.Windows.Forms.View.Tile) && this.VirtualMode)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListViewCantSetViewToTileViewInVirtualMode"));
                }
                if (this.viewStyle != value)
                {
                    this.viewStyle = value;
                    if (base.IsHandleCreated && this.ComctlSupportsVisualStyles)
                    {
                        base.SendMessage(0x108e, (int) this.viewStyle, 0);
                        this.UpdateGroupView();
                        if (this.viewStyle == System.Windows.Forms.View.Tile)
                        {
                            this.UpdateTileView();
                        }
                    }
                    else
                    {
                        base.UpdateStyles();
                    }
                    this.UpdateListViewItemsLocations();
                }
            }
        }

        [RefreshProperties(RefreshProperties.Repaint), DefaultValue(0), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ListViewVirtualListSizeDescr")]
        public int VirtualListSize
        {
            get
            {
                return this.virtualListSize;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ListViewVirtualListSizeInvalidArgument", new object[] { "value", value.ToString(CultureInfo.CurrentCulture) }));
                }
                if (value != this.virtualListSize)
                {
                    bool flag = ((base.IsHandleCreated && this.VirtualMode) && (this.View == System.Windows.Forms.View.Details)) && !base.DesignMode;
                    int num = -1;
                    if (flag)
                    {
                        num = (int) ((long) base.SendMessage(0x1027, 0, 0));
                    }
                    this.virtualListSize = value;
                    if ((base.IsHandleCreated && this.VirtualMode) && !base.DesignMode)
                    {
                        base.SendMessage(0x102f, this.virtualListSize, 0);
                    }
                    if (flag)
                    {
                        num = Math.Min(num, this.VirtualListSize - 1);
                        if (num > 0)
                        {
                            ListViewItem item = this.Items[num];
                            this.TopItem = item;
                        }
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("ListViewVirtualModeDescr"), DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior"), RefreshProperties(RefreshProperties.Repaint)]
        public bool VirtualMode
        {
            get
            {
                return this.listViewState[0x2000000];
            }
            set
            {
                if (value != this.VirtualMode)
                {
                    if (value && (this.Items.Count > 0))
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewVirtualListViewRequiresNoItems"));
                    }
                    if (value && (this.CheckedItems.Count > 0))
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewVirtualListViewRequiresNoCheckedItems"));
                    }
                    if (value && (this.SelectedItems.Count > 0))
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewVirtualListViewRequiresNoSelectedItems"));
                    }
                    if (value && (this.View == System.Windows.Forms.View.Tile))
                    {
                        throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListViewCantSetVirtualModeWhenInTileView"));
                    }
                    this.listViewState[0x2000000] = value;
                    this.RecreateHandleInternal();
                }
            }
        }

        [ListBindable(false)]
        public class CheckedIndexCollection : IList, ICollection, IEnumerable
        {
            private System.Windows.Forms.ListView owner;

            public CheckedIndexCollection(System.Windows.Forms.ListView owner)
            {
                this.owner = owner;
            }

            public bool Contains(int checkedIndex)
            {
                return this.owner.Items[checkedIndex].Checked;
            }

            public IEnumerator GetEnumerator()
            {
                int[] indicesArray = this.IndicesArray;
                if (indicesArray != null)
                {
                    return indicesArray.GetEnumerator();
                }
                return new int[0].GetEnumerator();
            }

            public int IndexOf(int checkedIndex)
            {
                int[] indicesArray = this.IndicesArray;
                for (int i = 0; i < indicesArray.Length; i++)
                {
                    if (indicesArray[i] == checkedIndex)
                    {
                        return i;
                    }
                }
                return -1;
            }

            void ICollection.CopyTo(Array dest, int index)
            {
                if (this.Count > 0)
                {
                    Array.Copy(this.IndicesArray, 0, dest, index, this.Count);
                }
            }

            int IList.Add(object value)
            {
                throw new NotSupportedException();
            }

            void IList.Clear()
            {
                throw new NotSupportedException();
            }

            bool IList.Contains(object checkedIndex)
            {
                return ((checkedIndex is int) && this.Contains((int) checkedIndex));
            }

            int IList.IndexOf(object checkedIndex)
            {
                if (checkedIndex is int)
                {
                    return this.IndexOf((int) checkedIndex);
                }
                return -1;
            }

            void IList.Insert(int index, object value)
            {
                throw new NotSupportedException();
            }

            void IList.Remove(object value)
            {
                throw new NotSupportedException();
            }

            void IList.RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            [Browsable(false)]
            public int Count
            {
                get
                {
                    if (!this.owner.CheckBoxes)
                    {
                        return 0;
                    }
                    int num = 0;
                    foreach (ListViewItem item in this.owner.Items)
                    {
                        if ((item != null) && item.Checked)
                        {
                            num++;
                        }
                    }
                    return num;
                }
            }

            private int[] IndicesArray
            {
                get
                {
                    int[] numArray = new int[this.Count];
                    int num = 0;
                    for (int i = 0; (i < this.owner.Items.Count) && (num < numArray.Length); i++)
                    {
                        if (this.owner.Items[i].Checked)
                        {
                            numArray[num++] = i;
                        }
                    }
                    return numArray;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public int this[int index]
            {
                get
                {
                    if (index < 0)
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    int count = this.owner.Items.Count;
                    int num2 = 0;
                    for (int i = 0; i < count; i++)
                    {
                        ListViewItem item = this.owner.Items[i];
                        if (item.Checked)
                        {
                            if (num2 == index)
                            {
                                return i;
                            }
                            num2++;
                        }
                    }
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
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
                    return true;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    throw new NotSupportedException();
                }
            }
        }

        [ListBindable(false)]
        public class CheckedListViewItemCollection : IList, ICollection, IEnumerable
        {
            private int lastAccessedIndex = -1;
            private System.Windows.Forms.ListView owner;

            public CheckedListViewItemCollection(System.Windows.Forms.ListView owner)
            {
                this.owner = owner;
            }

            public bool Contains(ListViewItem item)
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessCheckedItemsCollectionWhenInVirtualMode"));
                }
                return (((item != null) && (item.ListView == this.owner)) && item.Checked);
            }

            public virtual bool ContainsKey(string key)
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessCheckedItemsCollectionWhenInVirtualMode"));
                }
                return this.IsValidIndex(this.IndexOfKey(key));
            }

            public void CopyTo(Array dest, int index)
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessCheckedItemsCollectionWhenInVirtualMode"));
                }
                if (this.Count > 0)
                {
                    Array.Copy(this.ItemArray, 0, dest, index, this.Count);
                }
            }

            public IEnumerator GetEnumerator()
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessCheckedItemsCollectionWhenInVirtualMode"));
                }
                ListViewItem[] itemArray = this.ItemArray;
                if (itemArray != null)
                {
                    return itemArray.GetEnumerator();
                }
                return new ListViewItem[0].GetEnumerator();
            }

            public int IndexOf(ListViewItem item)
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessCheckedItemsCollectionWhenInVirtualMode"));
                }
                ListViewItem[] itemArray = this.ItemArray;
                for (int i = 0; i < itemArray.Length; i++)
                {
                    if (itemArray[i] == item)
                    {
                        return i;
                    }
                }
                return -1;
            }

            public virtual int IndexOfKey(string key)
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessCheckedItemsCollectionWhenInVirtualMode"));
                }
                if (!string.IsNullOrEmpty(key))
                {
                    if (this.IsValidIndex(this.lastAccessedIndex) && WindowsFormsUtils.SafeCompareStrings(this[this.lastAccessedIndex].Name, key, true))
                    {
                        return this.lastAccessedIndex;
                    }
                    for (int i = 0; i < this.Count; i++)
                    {
                        if (WindowsFormsUtils.SafeCompareStrings(this[i].Name, key, true))
                        {
                            this.lastAccessedIndex = i;
                            return i;
                        }
                    }
                    this.lastAccessedIndex = -1;
                }
                return -1;
            }

            private bool IsValidIndex(int index)
            {
                return ((index >= 0) && (index < this.Count));
            }

            int IList.Add(object value)
            {
                throw new NotSupportedException();
            }

            void IList.Clear()
            {
                throw new NotSupportedException();
            }

            bool IList.Contains(object item)
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessCheckedItemsCollectionWhenInVirtualMode"));
                }
                return ((item is ListViewItem) && this.Contains((ListViewItem) item));
            }

            int IList.IndexOf(object item)
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessCheckedItemsCollectionWhenInVirtualMode"));
                }
                if (item is ListViewItem)
                {
                    return this.IndexOf((ListViewItem) item);
                }
                return -1;
            }

            void IList.Insert(int index, object value)
            {
                throw new NotSupportedException();
            }

            void IList.Remove(object value)
            {
                throw new NotSupportedException();
            }

            void IList.RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            [Browsable(false)]
            public int Count
            {
                get
                {
                    if (this.owner.VirtualMode)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessCheckedItemsCollectionWhenInVirtualMode"));
                    }
                    return this.owner.CheckedIndices.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public ListViewItem this[int index]
            {
                get
                {
                    if (this.owner.VirtualMode)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessCheckedItemsCollectionWhenInVirtualMode"));
                    }
                    int num = this.owner.CheckedIndices[index];
                    return this.owner.Items[num];
                }
            }

            public virtual ListViewItem this[string key]
            {
                get
                {
                    if (this.owner.VirtualMode)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessCheckedItemsCollectionWhenInVirtualMode"));
                    }
                    if (!string.IsNullOrEmpty(key))
                    {
                        int index = this.IndexOfKey(key);
                        if (this.IsValidIndex(index))
                        {
                            return this[index];
                        }
                    }
                    return null;
                }
            }

            private ListViewItem[] ItemArray
            {
                get
                {
                    ListViewItem[] itemArray = new ListViewItem[this.Count];
                    int num = 0;
                    for (int i = 0; (i < this.owner.Items.Count) && (num < itemArray.Length); i++)
                    {
                        if (this.owner.Items[i].Checked)
                        {
                            itemArray[num++] = this.owner.Items[i];
                        }
                    }
                    return itemArray;
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
                    return true;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    if (this.owner.VirtualMode)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessCheckedItemsCollectionWhenInVirtualMode"));
                    }
                    return this[index];
                }
                set
                {
                    throw new NotSupportedException();
                }
            }
        }

        [ListBindable(false)]
        public class ColumnHeaderCollection : IList, ICollection, IEnumerable
        {
            private int lastAccessedIndex = -1;
            private System.Windows.Forms.ListView owner;

            public ColumnHeaderCollection(System.Windows.Forms.ListView owner)
            {
                this.owner = owner;
            }

            public virtual ColumnHeader Add(string text)
            {
                ColumnHeader ch = new ColumnHeader {
                    Text = text
                };
                return this.owner.InsertColumn(this.Count, ch);
            }

            public virtual int Add(ColumnHeader value)
            {
                int count = this.Count;
                this.owner.InsertColumn(count, value);
                return count;
            }

            public virtual ColumnHeader Add(string text, int width)
            {
                ColumnHeader ch = new ColumnHeader {
                    Text = text,
                    Width = width
                };
                return this.owner.InsertColumn(this.Count, ch);
            }

            public virtual ColumnHeader Add(string key, string text)
            {
                ColumnHeader ch = new ColumnHeader {
                    Name = key,
                    Text = text
                };
                return this.owner.InsertColumn(this.Count, ch);
            }

            public virtual ColumnHeader Add(string text, int width, HorizontalAlignment textAlign)
            {
                ColumnHeader ch = new ColumnHeader {
                    Text = text,
                    Width = width,
                    TextAlign = textAlign
                };
                return this.owner.InsertColumn(this.Count, ch);
            }

            public virtual ColumnHeader Add(string key, string text, int width)
            {
                ColumnHeader ch = new ColumnHeader {
                    Name = key,
                    Text = text,
                    Width = width
                };
                return this.owner.InsertColumn(this.Count, ch);
            }

            public virtual ColumnHeader Add(string key, string text, int width, HorizontalAlignment textAlign, int imageIndex)
            {
                ColumnHeader ch = new ColumnHeader(imageIndex) {
                    Name = key,
                    Text = text,
                    Width = width,
                    TextAlign = textAlign
                };
                return this.owner.InsertColumn(this.Count, ch);
            }

            public virtual ColumnHeader Add(string key, string text, int width, HorizontalAlignment textAlign, string imageKey)
            {
                ColumnHeader ch = new ColumnHeader(imageKey) {
                    Name = key,
                    Text = text,
                    Width = width,
                    TextAlign = textAlign
                };
                return this.owner.InsertColumn(this.Count, ch);
            }

            public virtual void AddRange(ColumnHeader[] values)
            {
                if (values == null)
                {
                    throw new ArgumentNullException("values");
                }
                Hashtable hashtable = new Hashtable();
                int[] indices = new int[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i].DisplayIndex == -1)
                    {
                        values[i].DisplayIndexInternal = i;
                    }
                    if ((!hashtable.ContainsKey(values[i].DisplayIndex) && (values[i].DisplayIndex >= 0)) && (values[i].DisplayIndex < values.Length))
                    {
                        hashtable.Add(values[i].DisplayIndex, i);
                    }
                    indices[i] = values[i].DisplayIndex;
                    this.Add(values[i]);
                }
                if (hashtable.Count == values.Length)
                {
                    this.owner.SetDisplayIndices(indices);
                }
            }

            public virtual void Clear()
            {
                if (this.owner.columnHeaders != null)
                {
                    if (this.owner.View == View.Tile)
                    {
                        for (int i = this.owner.columnHeaders.Length - 1; i >= 0; i--)
                        {
                            int width = this.owner.columnHeaders[i].Width;
                            this.owner.columnHeaders[i].OwnerListview = null;
                        }
                        this.owner.columnHeaders = null;
                        if (this.owner.IsHandleCreated)
                        {
                            this.owner.RecreateHandleInternal();
                        }
                    }
                    else
                    {
                        for (int j = this.owner.columnHeaders.Length - 1; j >= 0; j--)
                        {
                            int num3 = this.owner.columnHeaders[j].Width;
                            if (this.owner.IsHandleCreated)
                            {
                                this.owner.SendMessage(0x101c, j, 0);
                            }
                            this.owner.columnHeaders[j].OwnerListview = null;
                        }
                        this.owner.columnHeaders = null;
                    }
                }
            }

            public bool Contains(ColumnHeader value)
            {
                return (this.IndexOf(value) != -1);
            }

            public virtual bool ContainsKey(string key)
            {
                return this.IsValidIndex(this.IndexOfKey(key));
            }

            public IEnumerator GetEnumerator()
            {
                if (this.owner.columnHeaders != null)
                {
                    return this.owner.columnHeaders.GetEnumerator();
                }
                return new ColumnHeader[0].GetEnumerator();
            }

            public int IndexOf(ColumnHeader value)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i] == value)
                    {
                        return i;
                    }
                }
                return -1;
            }

            public virtual int IndexOfKey(string key)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    if (this.IsValidIndex(this.lastAccessedIndex) && WindowsFormsUtils.SafeCompareStrings(this[this.lastAccessedIndex].Name, key, true))
                    {
                        return this.lastAccessedIndex;
                    }
                    for (int i = 0; i < this.Count; i++)
                    {
                        if (WindowsFormsUtils.SafeCompareStrings(this[i].Name, key, true))
                        {
                            this.lastAccessedIndex = i;
                            return i;
                        }
                    }
                    this.lastAccessedIndex = -1;
                }
                return -1;
            }

            public void Insert(int index, string text)
            {
                ColumnHeader header = new ColumnHeader {
                    Text = text
                };
                this.Insert(index, header);
            }

            public void Insert(int index, ColumnHeader value)
            {
                if ((index < 0) || (index > this.Count))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                this.owner.InsertColumn(index, value);
            }

            public void Insert(int index, string text, int width)
            {
                ColumnHeader header = new ColumnHeader {
                    Text = text,
                    Width = width
                };
                this.Insert(index, header);
            }

            public void Insert(int index, string key, string text)
            {
                ColumnHeader header = new ColumnHeader {
                    Name = key,
                    Text = text
                };
                this.Insert(index, header);
            }

            public void Insert(int index, string text, int width, HorizontalAlignment textAlign)
            {
                ColumnHeader header = new ColumnHeader {
                    Text = text,
                    Width = width,
                    TextAlign = textAlign
                };
                this.Insert(index, header);
            }

            public void Insert(int index, string key, string text, int width)
            {
                ColumnHeader header = new ColumnHeader {
                    Name = key,
                    Text = text,
                    Width = width
                };
                this.Insert(index, header);
            }

            public void Insert(int index, string key, string text, int width, HorizontalAlignment textAlign, int imageIndex)
            {
                ColumnHeader header = new ColumnHeader(imageIndex) {
                    Name = key,
                    Text = text,
                    Width = width,
                    TextAlign = textAlign
                };
                this.Insert(index, header);
            }

            public void Insert(int index, string key, string text, int width, HorizontalAlignment textAlign, string imageKey)
            {
                ColumnHeader header = new ColumnHeader(imageKey) {
                    Name = key,
                    Text = text,
                    Width = width,
                    TextAlign = textAlign
                };
                this.Insert(index, header);
            }

            private bool IsValidIndex(int index)
            {
                return ((index >= 0) && (index < this.Count));
            }

            public virtual void Remove(ColumnHeader column)
            {
                int index = this.IndexOf(column);
                if (index != -1)
                {
                    this.RemoveAt(index);
                }
            }

            public virtual void RemoveAt(int index)
            {
                if ((index < 0) || (index >= this.owner.columnHeaders.Length))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                int width = this.owner.columnHeaders[index].Width;
                if ((this.owner.IsHandleCreated && (this.owner.View != View.Tile)) && (((int) ((long) this.owner.SendMessage(0x101c, index, 0))) == 0))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                int[] indices = new int[this.Count - 1];
                ColumnHeader header = this[index];
                for (int i = 0; i < this.Count; i++)
                {
                    ColumnHeader header2 = this[i];
                    if (i != index)
                    {
                        if (header2.DisplayIndex >= header.DisplayIndex)
                        {
                            header2.DisplayIndexInternal--;
                        }
                        indices[(i > index) ? (i - 1) : i] = header2.DisplayIndexInternal;
                    }
                }
                header.DisplayIndexInternal = -1;
                this.owner.columnHeaders[index].OwnerListview = null;
                int length = this.owner.columnHeaders.Length;
                if (length == 1)
                {
                    this.owner.columnHeaders = null;
                }
                else
                {
                    ColumnHeader[] destinationArray = new ColumnHeader[--length];
                    if (index > 0)
                    {
                        Array.Copy(this.owner.columnHeaders, 0, destinationArray, 0, index);
                    }
                    if (index < length)
                    {
                        Array.Copy(this.owner.columnHeaders, index + 1, destinationArray, index, length - index);
                    }
                    this.owner.columnHeaders = destinationArray;
                }
                if (this.owner.IsHandleCreated && (this.owner.View == View.Tile))
                {
                    this.owner.RecreateHandleInternal();
                }
                this.owner.SetDisplayIndices(indices);
            }

            public virtual void RemoveByKey(string key)
            {
                int index = this.IndexOfKey(key);
                if (this.IsValidIndex(index))
                {
                    this.RemoveAt(index);
                }
            }

            void ICollection.CopyTo(Array dest, int index)
            {
                if (this.Count > 0)
                {
                    Array.Copy(this.owner.columnHeaders, 0, dest, index, this.Count);
                }
            }

            int IList.Add(object value)
            {
                if (!(value is ColumnHeader))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ColumnHeaderCollectionInvalidArgument"));
                }
                return this.Add((ColumnHeader) value);
            }

            bool IList.Contains(object value)
            {
                return ((value is ColumnHeader) && this.Contains((ColumnHeader) value));
            }

            int IList.IndexOf(object value)
            {
                if (value is ColumnHeader)
                {
                    return this.IndexOf((ColumnHeader) value);
                }
                return -1;
            }

            void IList.Insert(int index, object value)
            {
                if (value is ColumnHeader)
                {
                    this.Insert(index, (ColumnHeader) value);
                }
            }

            void IList.Remove(object value)
            {
                if (value is ColumnHeader)
                {
                    this.Remove((ColumnHeader) value);
                }
            }

            [Browsable(false)]
            public int Count
            {
                get
                {
                    if (this.owner.columnHeaders != null)
                    {
                        return this.owner.columnHeaders.Length;
                    }
                    return 0;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public virtual ColumnHeader this[int index]
            {
                get
                {
                    if (((this.owner.columnHeaders == null) || (index < 0)) || (index >= this.owner.columnHeaders.Length))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    return this.owner.columnHeaders[index];
                }
            }

            public virtual ColumnHeader this[string key]
            {
                get
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        int index = this.IndexOfKey(key);
                        if (this.IsValidIndex(index))
                        {
                            return this[index];
                        }
                    }
                    return null;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return true;
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

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    throw new NotSupportedException();
                }
            }
        }

        internal class IconComparer : IComparer
        {
            private System.Windows.Forms.SortOrder sortOrder;

            public IconComparer(System.Windows.Forms.SortOrder currentSortOrder)
            {
                this.sortOrder = currentSortOrder;
            }

            public int Compare(object obj1, object obj2)
            {
                ListViewItem item = (ListViewItem) obj1;
                ListViewItem item2 = (ListViewItem) obj2;
                if (this.sortOrder == System.Windows.Forms.SortOrder.Ascending)
                {
                    return string.Compare(item.Text, item2.Text, false, CultureInfo.CurrentCulture);
                }
                return string.Compare(item2.Text, item.Text, false, CultureInfo.CurrentCulture);
            }

            public System.Windows.Forms.SortOrder SortOrder
            {
                set
                {
                    this.sortOrder = value;
                }
            }
        }

        [ListBindable(false)]
        public class ListViewItemCollection : IList, ICollection, IEnumerable
        {
            private IInnerList innerList;
            private int lastAccessedIndex;

            public ListViewItemCollection(System.Windows.Forms.ListView owner)
            {
                this.lastAccessedIndex = -1;
                this.innerList = new System.Windows.Forms.ListView.ListViewNativeItemCollection(owner);
            }

            internal ListViewItemCollection(IInnerList innerList)
            {
                this.lastAccessedIndex = -1;
                this.innerList = innerList;
            }

            public virtual ListViewItem Add(string text)
            {
                return this.Add(text, -1);
            }

            public virtual ListViewItem Add(ListViewItem value)
            {
                this.InnerList.Add(value);
                return value;
            }

            public virtual ListViewItem Add(string text, int imageIndex)
            {
                ListViewItem item = new ListViewItem(text, imageIndex);
                this.Add(item);
                return item;
            }

            public virtual ListViewItem Add(string text, string imageKey)
            {
                ListViewItem item = new ListViewItem(text, imageKey);
                this.Add(item);
                return item;
            }

            public virtual ListViewItem Add(string key, string text, int imageIndex)
            {
                ListViewItem item = new ListViewItem(text, imageIndex) {
                    Name = key
                };
                this.Add(item);
                return item;
            }

            public virtual ListViewItem Add(string key, string text, string imageKey)
            {
                ListViewItem item = new ListViewItem(text, imageKey) {
                    Name = key
                };
                this.Add(item);
                return item;
            }

            public void AddRange(ListViewItem[] items)
            {
                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                this.InnerList.AddRange(items);
            }

            public void AddRange(System.Windows.Forms.ListView.ListViewItemCollection items)
            {
                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                ListViewItem[] dest = new ListViewItem[items.Count];
                items.CopyTo(dest, 0);
                this.InnerList.AddRange(dest);
            }

            public virtual void Clear()
            {
                this.InnerList.Clear();
            }

            public bool Contains(ListViewItem item)
            {
                return this.InnerList.Contains(item);
            }

            public virtual bool ContainsKey(string key)
            {
                return this.IsValidIndex(this.IndexOfKey(key));
            }

            public void CopyTo(Array dest, int index)
            {
                this.InnerList.CopyTo(dest, index);
            }

            public ListViewItem[] Find(string key, bool searchAllSubItems)
            {
                ArrayList list = this.FindInternal(key, searchAllSubItems, this, new ArrayList());
                ListViewItem[] array = new ListViewItem[list.Count];
                list.CopyTo(array, 0);
                return array;
            }

            private ArrayList FindInternal(string key, bool searchAllSubItems, System.Windows.Forms.ListView.ListViewItemCollection listViewItems, ArrayList foundItems)
            {
                if ((listViewItems == null) || (foundItems == null))
                {
                    return null;
                }
                for (int i = 0; i < listViewItems.Count; i++)
                {
                    if (WindowsFormsUtils.SafeCompareStrings(listViewItems[i].Name, key, true))
                    {
                        foundItems.Add(listViewItems[i]);
                    }
                    else if (searchAllSubItems)
                    {
                        for (int j = 1; j < listViewItems[i].SubItems.Count; j++)
                        {
                            if (WindowsFormsUtils.SafeCompareStrings(listViewItems[i].SubItems[j].Name, key, true))
                            {
                                foundItems.Add(listViewItems[i]);
                                break;
                            }
                        }
                    }
                }
                return foundItems;
            }

            public IEnumerator GetEnumerator()
            {
                if (this.InnerList.OwnerIsVirtualListView && !this.InnerList.OwnerIsDesignMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantGetEnumeratorInVirtualMode"));
                }
                return this.InnerList.GetEnumerator();
            }

            public int IndexOf(ListViewItem item)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i] == item)
                    {
                        return i;
                    }
                }
                return -1;
            }

            public virtual int IndexOfKey(string key)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    if (this.IsValidIndex(this.lastAccessedIndex) && WindowsFormsUtils.SafeCompareStrings(this[this.lastAccessedIndex].Name, key, true))
                    {
                        return this.lastAccessedIndex;
                    }
                    for (int i = 0; i < this.Count; i++)
                    {
                        if (WindowsFormsUtils.SafeCompareStrings(this[i].Name, key, true))
                        {
                            this.lastAccessedIndex = i;
                            return i;
                        }
                    }
                    this.lastAccessedIndex = -1;
                }
                return -1;
            }

            public ListViewItem Insert(int index, string text)
            {
                return this.Insert(index, new ListViewItem(text));
            }

            public ListViewItem Insert(int index, ListViewItem item)
            {
                if ((index < 0) || (index > this.Count))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                this.InnerList.Insert(index, item);
                return item;
            }

            public ListViewItem Insert(int index, string text, int imageIndex)
            {
                return this.Insert(index, new ListViewItem(text, imageIndex));
            }

            public ListViewItem Insert(int index, string text, string imageKey)
            {
                return this.Insert(index, new ListViewItem(text, imageKey));
            }

            public virtual ListViewItem Insert(int index, string key, string text, int imageIndex)
            {
                ListViewItem item = new ListViewItem(text, imageIndex) {
                    Name = key
                };
                return this.Insert(index, item);
            }

            public virtual ListViewItem Insert(int index, string key, string text, string imageKey)
            {
                ListViewItem item = new ListViewItem(text, imageKey) {
                    Name = key
                };
                return this.Insert(index, item);
            }

            private bool IsValidIndex(int index)
            {
                return ((index >= 0) && (index < this.Count));
            }

            public virtual void Remove(ListViewItem item)
            {
                this.InnerList.Remove(item);
            }

            public virtual void RemoveAt(int index)
            {
                if ((index < 0) || (index >= this.Count))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                this.InnerList.RemoveAt(index);
            }

            public virtual void RemoveByKey(string key)
            {
                int index = this.IndexOfKey(key);
                if (this.IsValidIndex(index))
                {
                    this.RemoveAt(index);
                }
            }

            int IList.Add(object item)
            {
                if (item is ListViewItem)
                {
                    return this.IndexOf(this.Add((ListViewItem) item));
                }
                if (item != null)
                {
                    return this.IndexOf(this.Add(item.ToString()));
                }
                return -1;
            }

            bool IList.Contains(object item)
            {
                return ((item is ListViewItem) && this.Contains((ListViewItem) item));
            }

            int IList.IndexOf(object item)
            {
                if (item is ListViewItem)
                {
                    return this.IndexOf((ListViewItem) item);
                }
                return -1;
            }

            void IList.Insert(int index, object item)
            {
                if (item is ListViewItem)
                {
                    this.Insert(index, (ListViewItem) item);
                }
                else if (item != null)
                {
                    this.Insert(index, item.ToString());
                }
            }

            void IList.Remove(object item)
            {
                if ((item != null) && (item is ListViewItem))
                {
                    this.Remove((ListViewItem) item);
                }
            }

            [Browsable(false)]
            public int Count
            {
                get
                {
                    return this.InnerList.Count;
                }
            }

            private IInnerList InnerList
            {
                get
                {
                    return this.innerList;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public virtual ListViewItem this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.InnerList.Count))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    return this.InnerList[index];
                }
                set
                {
                    if ((index < 0) || (index >= this.InnerList.Count))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    this.InnerList[index] = value;
                }
            }

            public virtual ListViewItem this[string key]
            {
                get
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        int index = this.IndexOfKey(key);
                        if (this.IsValidIndex(index))
                        {
                            return this[index];
                        }
                    }
                    return null;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return true;
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

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    if (value is ListViewItem)
                    {
                        this[index] = (ListViewItem) value;
                    }
                    else if (value != null)
                    {
                        this[index] = new ListViewItem(value.ToString(), -1);
                    }
                }
            }

            internal interface IInnerList
            {
                ListViewItem Add(ListViewItem item);
                void AddRange(ListViewItem[] items);
                void Clear();
                bool Contains(ListViewItem item);
                void CopyTo(Array dest, int index);
                IEnumerator GetEnumerator();
                int IndexOf(ListViewItem item);
                ListViewItem Insert(int index, ListViewItem item);
                void Remove(ListViewItem item);
                void RemoveAt(int index);

                int Count { get; }

                ListViewItem this[int index] { get; set; }

                bool OwnerIsDesignMode { get; }

                bool OwnerIsVirtualListView { get; }
            }
        }

        internal class ListViewNativeItemCollection : System.Windows.Forms.ListView.ListViewItemCollection.IInnerList
        {
            private System.Windows.Forms.ListView owner;

            public ListViewNativeItemCollection(System.Windows.Forms.ListView owner)
            {
                this.owner = owner;
            }

            public ListViewItem Add(ListViewItem value)
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAddItemsToAVirtualListView"));
                }
                bool flag = value.Checked;
                this.owner.InsertItems(this.owner.itemCount, new ListViewItem[] { value }, true);
                if ((this.owner.IsHandleCreated && !this.owner.CheckBoxes) && flag)
                {
                    this.owner.UpdateSavedCheckedItems(value, true);
                }
                if (this.owner.ExpectingMouseUp)
                {
                    this.owner.ItemCollectionChangedInMouseDown = true;
                }
                return value;
            }

            public void AddRange(ListViewItem[] values)
            {
                if (values == null)
                {
                    throw new ArgumentNullException("values");
                }
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAddItemsToAVirtualListView"));
                }
                IComparer listItemSorter = this.owner.listItemSorter;
                this.owner.listItemSorter = null;
                bool[] flagArray = null;
                if (this.owner.IsHandleCreated && !this.owner.CheckBoxes)
                {
                    flagArray = new bool[values.Length];
                    for (int i = 0; i < values.Length; i++)
                    {
                        flagArray[i] = values[i].Checked;
                    }
                }
                try
                {
                    this.owner.BeginUpdate();
                    this.owner.InsertItems(this.owner.itemCount, values, true);
                    if (this.owner.IsHandleCreated && !this.owner.CheckBoxes)
                    {
                        for (int j = 0; j < values.Length; j++)
                        {
                            if (flagArray[j])
                            {
                                this.owner.UpdateSavedCheckedItems(values[j], true);
                            }
                        }
                    }
                }
                finally
                {
                    this.owner.listItemSorter = listItemSorter;
                    this.owner.EndUpdate();
                }
                if (this.owner.ExpectingMouseUp)
                {
                    this.owner.ItemCollectionChangedInMouseDown = true;
                }
                if ((listItemSorter != null) || ((this.owner.Sorting != SortOrder.None) && !this.owner.VirtualMode))
                {
                    this.owner.Sort();
                }
            }

            public void Clear()
            {
                if (this.owner.itemCount > 0)
                {
                    this.owner.ApplyUpdateCachedItems();
                    if (this.owner.IsHandleCreated && !this.owner.ListViewHandleDestroyed)
                    {
                        int count = this.owner.Items.Count;
                        int wParam = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.owner, this.owner.Handle), 0x100c, -1, 2);
                        for (int i = 0; i < count; i++)
                        {
                            ListViewItem item = this.owner.Items[i];
                            if (item != null)
                            {
                                if (i == wParam)
                                {
                                    item.StateSelected = true;
                                    wParam = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.owner, this.owner.Handle), 0x100c, wParam, 2);
                                }
                                else
                                {
                                    item.StateSelected = false;
                                }
                                item.UnHost(i, false);
                            }
                        }
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.owner, this.owner.Handle), 0x1009, 0, 0);
                        if (this.owner.View == View.SmallIcon)
                        {
                            if (this.owner.ComctlSupportsVisualStyles)
                            {
                                this.owner.FlipViewToLargeIconAndSmallIcon = true;
                            }
                            else
                            {
                                this.owner.View = View.LargeIcon;
                                this.owner.View = View.SmallIcon;
                            }
                        }
                    }
                    else
                    {
                        int num4 = this.owner.Items.Count;
                        for (int j = 0; j < num4; j++)
                        {
                            ListViewItem item2 = this.owner.Items[j];
                            if (item2 != null)
                            {
                                item2.UnHost(j, true);
                            }
                        }
                        this.owner.listItemsArray.Clear();
                    }
                    this.owner.listItemsTable.Clear();
                    if (this.owner.IsHandleCreated && !this.owner.CheckBoxes)
                    {
                        this.owner.savedCheckedItems = null;
                    }
                    this.owner.itemCount = 0;
                    if (this.owner.ExpectingMouseUp)
                    {
                        this.owner.ItemCollectionChangedInMouseDown = true;
                    }
                }
            }

            public bool Contains(ListViewItem item)
            {
                this.owner.ApplyUpdateCachedItems();
                if (this.owner.IsHandleCreated && !this.owner.ListViewHandleDestroyed)
                {
                    return (this.owner.listItemsTable[item.ID] == item);
                }
                return this.owner.listItemsArray.Contains(item);
            }

            public void CopyTo(Array dest, int index)
            {
                if (this.owner.itemCount > 0)
                {
                    for (int i = 0; i < this.Count; i++)
                    {
                        dest.SetValue(this[i], index++);
                    }
                }
            }

            private int DisplayIndexToID(int displayIndex)
            {
                if (this.owner.IsHandleCreated && !this.owner.ListViewHandleDestroyed)
                {
                    System.Windows.Forms.NativeMethods.LVITEM lParam = new System.Windows.Forms.NativeMethods.LVITEM {
                        mask = 4,
                        iItem = displayIndex
                    };
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.owner, this.owner.Handle), System.Windows.Forms.NativeMethods.LVM_GETITEM, 0, ref lParam);
                    return (int) lParam.lParam;
                }
                return this[displayIndex].ID;
            }

            public IEnumerator GetEnumerator()
            {
                ListViewItem[] dest = new ListViewItem[this.owner.itemCount];
                this.CopyTo(dest, 0);
                return dest.GetEnumerator();
            }

            public int IndexOf(ListViewItem item)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (item == this[i])
                    {
                        return i;
                    }
                }
                return -1;
            }

            public ListViewItem Insert(int index, ListViewItem item)
            {
                int count = 0;
                if (this.owner.VirtualMode)
                {
                    count = this.Count;
                }
                else
                {
                    count = this.owner.itemCount;
                }
                if ((index < 0) || (index > count))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAddItemsToAVirtualListView"));
                }
                if (index < count)
                {
                    this.owner.ApplyUpdateCachedItems();
                }
                this.owner.InsertItems(index, new ListViewItem[] { item }, true);
                if ((this.owner.IsHandleCreated && !this.owner.CheckBoxes) && item.Checked)
                {
                    this.owner.UpdateSavedCheckedItems(item, true);
                }
                if (this.owner.ExpectingMouseUp)
                {
                    this.owner.ItemCollectionChangedInMouseDown = true;
                }
                return item;
            }

            public void Remove(ListViewItem item)
            {
                int index = this.owner.VirtualMode ? (this.Count - 1) : this.IndexOf(item);
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantRemoveItemsFromAVirtualListView"));
                }
                if (index != -1)
                {
                    this.RemoveAt(index);
                }
            }

            public void RemoveAt(int index)
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantRemoveItemsFromAVirtualListView"));
                }
                if ((index < 0) || (index >= this.owner.itemCount))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                if ((this.owner.IsHandleCreated && !this.owner.CheckBoxes) && this[index].Checked)
                {
                    this.owner.UpdateSavedCheckedItems(this[index], false);
                }
                this.owner.ApplyUpdateCachedItems();
                int key = this.DisplayIndexToID(index);
                this[index].UnHost(true);
                if (this.owner.IsHandleCreated)
                {
                    if (((int) ((long) this.owner.SendMessage(0x1008, index, 0))) == 0)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                }
                else
                {
                    this.owner.listItemsArray.RemoveAt(index);
                }
                this.owner.itemCount--;
                this.owner.listItemsTable.Remove(key);
                if (this.owner.ExpectingMouseUp)
                {
                    this.owner.ItemCollectionChangedInMouseDown = true;
                }
            }

            public int Count
            {
                get
                {
                    this.owner.ApplyUpdateCachedItems();
                    if (this.owner.VirtualMode)
                    {
                        return this.owner.VirtualListSize;
                    }
                    return this.owner.itemCount;
                }
            }

            public ListViewItem this[int displayIndex]
            {
                get
                {
                    this.owner.ApplyUpdateCachedItems();
                    if (this.owner.VirtualMode)
                    {
                        RetrieveVirtualItemEventArgs e = new RetrieveVirtualItemEventArgs(displayIndex);
                        this.owner.OnRetrieveVirtualItem(e);
                        e.Item.SetItemIndex(this.owner, displayIndex);
                        return e.Item;
                    }
                    if ((displayIndex < 0) || (displayIndex >= this.owner.itemCount))
                    {
                        throw new ArgumentOutOfRangeException("displayIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "displayIndex", displayIndex.ToString(CultureInfo.CurrentCulture) }));
                    }
                    if (this.owner.IsHandleCreated && !this.owner.ListViewHandleDestroyed)
                    {
                        return (ListViewItem) this.owner.listItemsTable[this.DisplayIndexToID(displayIndex)];
                    }
                    return (ListViewItem) this.owner.listItemsArray[displayIndex];
                }
                set
                {
                    this.owner.ApplyUpdateCachedItems();
                    if (this.owner.VirtualMode)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantModifyTheItemCollInAVirtualListView"));
                    }
                    if ((displayIndex < 0) || (displayIndex >= this.owner.itemCount))
                    {
                        throw new ArgumentOutOfRangeException("displayIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "displayIndex", displayIndex.ToString(CultureInfo.CurrentCulture) }));
                    }
                    if (this.owner.ExpectingMouseUp)
                    {
                        this.owner.ItemCollectionChangedInMouseDown = true;
                    }
                    this.RemoveAt(displayIndex);
                    this.Insert(displayIndex, value);
                }
            }

            public bool OwnerIsDesignMode
            {
                get
                {
                    return this.owner.DesignMode;
                }
            }

            public bool OwnerIsVirtualListView
            {
                get
                {
                    return this.owner.VirtualMode;
                }
            }
        }

        [ListBindable(false)]
        public class SelectedIndexCollection : IList, ICollection, IEnumerable
        {
            private System.Windows.Forms.ListView owner;

            public SelectedIndexCollection(System.Windows.Forms.ListView owner)
            {
                this.owner = owner;
            }

            public int Add(int itemIndex)
            {
                if (this.owner.VirtualMode)
                {
                    if ((itemIndex < 0) || (itemIndex >= this.owner.VirtualListSize))
                    {
                        throw new ArgumentOutOfRangeException("itemIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "itemIndex", itemIndex.ToString(CultureInfo.CurrentCulture) }));
                    }
                    if (this.owner.IsHandleCreated)
                    {
                        this.owner.SetItemState(itemIndex, 2, 2);
                        return this.Count;
                    }
                    return -1;
                }
                if ((itemIndex < 0) || (itemIndex >= this.owner.Items.Count))
                {
                    throw new ArgumentOutOfRangeException("itemIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "itemIndex", itemIndex.ToString(CultureInfo.CurrentCulture) }));
                }
                this.owner.Items[itemIndex].Selected = true;
                return this.Count;
            }

            public void Clear()
            {
                if (!this.owner.VirtualMode)
                {
                    this.owner.savedSelectedItems = null;
                }
                if (this.owner.IsHandleCreated)
                {
                    this.owner.SetItemState(-1, 0, 2);
                }
            }

            public bool Contains(int selectedIndex)
            {
                return this.owner.Items[selectedIndex].Selected;
            }

            public void CopyTo(Array dest, int index)
            {
                if (this.Count > 0)
                {
                    Array.Copy(this.IndicesArray, 0, dest, index, this.Count);
                }
            }

            public IEnumerator GetEnumerator()
            {
                int[] indicesArray = this.IndicesArray;
                if (indicesArray != null)
                {
                    return indicesArray.GetEnumerator();
                }
                return new int[0].GetEnumerator();
            }

            public int IndexOf(int selectedIndex)
            {
                int[] indicesArray = this.IndicesArray;
                for (int i = 0; i < indicesArray.Length; i++)
                {
                    if (indicesArray[i] == selectedIndex)
                    {
                        return i;
                    }
                }
                return -1;
            }

            public void Remove(int itemIndex)
            {
                if (this.owner.VirtualMode)
                {
                    if ((itemIndex < 0) || (itemIndex >= this.owner.VirtualListSize))
                    {
                        throw new ArgumentOutOfRangeException("itemIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "itemIndex", itemIndex.ToString(CultureInfo.CurrentCulture) }));
                    }
                    if (this.owner.IsHandleCreated)
                    {
                        this.owner.SetItemState(itemIndex, 0, 2);
                    }
                }
                else
                {
                    if ((itemIndex < 0) || (itemIndex >= this.owner.Items.Count))
                    {
                        throw new ArgumentOutOfRangeException("itemIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "itemIndex", itemIndex.ToString(CultureInfo.CurrentCulture) }));
                    }
                    this.owner.Items[itemIndex].Selected = false;
                }
            }

            int IList.Add(object value)
            {
                if (!(value is int))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "value", value.ToString() }));
                }
                return this.Add((int) value);
            }

            void IList.Clear()
            {
                this.Clear();
            }

            bool IList.Contains(object selectedIndex)
            {
                return ((selectedIndex is int) && this.Contains((int) selectedIndex));
            }

            int IList.IndexOf(object selectedIndex)
            {
                if (selectedIndex is int)
                {
                    return this.IndexOf((int) selectedIndex);
                }
                return -1;
            }

            void IList.Insert(int index, object value)
            {
                throw new NotSupportedException();
            }

            void IList.Remove(object value)
            {
                if (!(value is int))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "value", value.ToString() }));
                }
                this.Remove((int) value);
            }

            void IList.RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            [Browsable(false)]
            public int Count
            {
                get
                {
                    if (this.owner.IsHandleCreated)
                    {
                        return (int) ((long) this.owner.SendMessage(0x1032, 0, 0));
                    }
                    if (this.owner.savedSelectedItems != null)
                    {
                        return this.owner.savedSelectedItems.Count;
                    }
                    return 0;
                }
            }

            private int[] IndicesArray
            {
                get
                {
                    int count = this.Count;
                    int[] numArray = new int[count];
                    if (this.owner.IsHandleCreated)
                    {
                        int wparam = -1;
                        for (int j = 0; j < count; j++)
                        {
                            int num4 = (int) ((long) this.owner.SendMessage(0x100c, wparam, 2));
                            if (num4 <= -1)
                            {
                                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("SelectedNotEqualActual"));
                            }
                            numArray[j] = num4;
                            wparam = num4;
                        }
                        return numArray;
                    }
                    for (int i = 0; i < count; i++)
                    {
                        numArray[i] = this.owner.savedSelectedItems[i].Index;
                    }
                    return numArray;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public int this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.Count))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    if (!this.owner.IsHandleCreated)
                    {
                        return this.owner.savedSelectedItems[index].Index;
                    }
                    int wparam = -1;
                    for (int i = 0; i <= index; i++)
                    {
                        wparam = (int) ((long) this.owner.SendMessage(0x100c, wparam, 2));
                    }
                    return wparam;
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

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    throw new NotSupportedException();
                }
            }
        }

        [ListBindable(false)]
        public class SelectedListViewItemCollection : IList, ICollection, IEnumerable
        {
            private int lastAccessedIndex = -1;
            private System.Windows.Forms.ListView owner;

            public SelectedListViewItemCollection(System.Windows.Forms.ListView owner)
            {
                this.owner = owner;
            }

            public void Clear()
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessSelectedItemsCollectionWhenInVirtualMode"));
                }
                ListViewItem[] selectedItemArray = this.SelectedItemArray;
                for (int i = 0; i < selectedItemArray.Length; i++)
                {
                    selectedItemArray[i].Selected = false;
                }
            }

            public bool Contains(ListViewItem item)
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessSelectedItemsCollectionWhenInVirtualMode"));
                }
                return (this.IndexOf(item) != -1);
            }

            public virtual bool ContainsKey(string key)
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessSelectedItemsCollectionWhenInVirtualMode"));
                }
                return this.IsValidIndex(this.IndexOfKey(key));
            }

            public void CopyTo(Array dest, int index)
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessSelectedItemsCollectionWhenInVirtualMode"));
                }
                if (this.Count > 0)
                {
                    Array.Copy(this.SelectedItemArray, 0, dest, index, this.Count);
                }
            }

            public IEnumerator GetEnumerator()
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessSelectedItemsCollectionWhenInVirtualMode"));
                }
                ListViewItem[] selectedItemArray = this.SelectedItemArray;
                if (selectedItemArray != null)
                {
                    return selectedItemArray.GetEnumerator();
                }
                return new ListViewItem[0].GetEnumerator();
            }

            public int IndexOf(ListViewItem item)
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessSelectedItemsCollectionWhenInVirtualMode"));
                }
                ListViewItem[] selectedItemArray = this.SelectedItemArray;
                for (int i = 0; i < selectedItemArray.Length; i++)
                {
                    if (selectedItemArray[i] == item)
                    {
                        return i;
                    }
                }
                return -1;
            }

            public virtual int IndexOfKey(string key)
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessSelectedItemsCollectionWhenInVirtualMode"));
                }
                if (!string.IsNullOrEmpty(key))
                {
                    if (this.IsValidIndex(this.lastAccessedIndex) && WindowsFormsUtils.SafeCompareStrings(this[this.lastAccessedIndex].Name, key, true))
                    {
                        return this.lastAccessedIndex;
                    }
                    for (int i = 0; i < this.Count; i++)
                    {
                        if (WindowsFormsUtils.SafeCompareStrings(this[i].Name, key, true))
                        {
                            this.lastAccessedIndex = i;
                            return i;
                        }
                    }
                    this.lastAccessedIndex = -1;
                }
                return -1;
            }

            private bool IsValidIndex(int index)
            {
                return ((index >= 0) && (index < this.Count));
            }

            int IList.Add(object value)
            {
                throw new NotSupportedException();
            }

            bool IList.Contains(object item)
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessSelectedItemsCollectionWhenInVirtualMode"));
                }
                return ((item is ListViewItem) && this.Contains((ListViewItem) item));
            }

            int IList.IndexOf(object item)
            {
                if (this.owner.VirtualMode)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessSelectedItemsCollectionWhenInVirtualMode"));
                }
                if (item is ListViewItem)
                {
                    return this.IndexOf((ListViewItem) item);
                }
                return -1;
            }

            void IList.Insert(int index, object value)
            {
                throw new NotSupportedException();
            }

            void IList.Remove(object value)
            {
                throw new NotSupportedException();
            }

            void IList.RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            [Browsable(false)]
            public int Count
            {
                get
                {
                    if (this.owner.VirtualMode)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessSelectedItemsCollectionWhenInVirtualMode"));
                    }
                    if (this.owner.IsHandleCreated)
                    {
                        return (int) ((long) this.owner.SendMessage(0x1032, 0, 0));
                    }
                    if (this.owner.savedSelectedItems != null)
                    {
                        return this.owner.savedSelectedItems.Count;
                    }
                    return 0;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public ListViewItem this[int index]
            {
                get
                {
                    if (this.owner.VirtualMode)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessSelectedItemsCollectionWhenInVirtualMode"));
                    }
                    if ((index < 0) || (index >= this.Count))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    if (!this.owner.IsHandleCreated)
                    {
                        return this.owner.savedSelectedItems[index];
                    }
                    int wparam = -1;
                    for (int i = 0; i <= index; i++)
                    {
                        wparam = (int) ((long) this.owner.SendMessage(0x100c, wparam, 2));
                    }
                    return this.owner.Items[wparam];
                }
            }

            public virtual ListViewItem this[string key]
            {
                get
                {
                    if (this.owner.VirtualMode)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessSelectedItemsCollectionWhenInVirtualMode"));
                    }
                    if (!string.IsNullOrEmpty(key))
                    {
                        int index = this.IndexOfKey(key);
                        if (this.IsValidIndex(index))
                        {
                            return this[index];
                        }
                    }
                    return null;
                }
            }

            private ListViewItem[] SelectedItemArray
            {
                get
                {
                    if (this.owner.IsHandleCreated)
                    {
                        int num = (int) ((long) this.owner.SendMessage(0x1032, 0, 0));
                        ListViewItem[] itemArray = new ListViewItem[num];
                        int wparam = -1;
                        for (int j = 0; j < num; j++)
                        {
                            int num4 = (int) ((long) this.owner.SendMessage(0x100c, wparam, 2));
                            if (num4 <= -1)
                            {
                                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("SelectedNotEqualActual"));
                            }
                            itemArray[j] = this.owner.Items[num4];
                            wparam = num4;
                        }
                        return itemArray;
                    }
                    if (this.owner.savedSelectedItems == null)
                    {
                        return new ListViewItem[0];
                    }
                    ListViewItem[] itemArray2 = new ListViewItem[this.owner.savedSelectedItems.Count];
                    for (int i = 0; i < this.owner.savedSelectedItems.Count; i++)
                    {
                        itemArray2[i] = this.owner.savedSelectedItems[i];
                    }
                    return itemArray2;
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
                    return true;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    if (this.owner.VirtualMode)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewCantAccessSelectedItemsCollectionWhenInVirtualMode"));
                    }
                    return this[index];
                }
                set
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}

