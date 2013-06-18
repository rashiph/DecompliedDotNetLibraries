namespace System.Windows.Forms
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Forms.Internal;
    using System.Windows.Forms.Layout;

    [System.Windows.Forms.SRDescription("DescriptionToolStrip"), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultEvent("ItemClicked"), ComVisible(true), DefaultProperty("Items"), DesignerSerializer("System.Windows.Forms.Design.ToolStripCodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Designer("System.Windows.Forms.Design.ToolStripDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class ToolStrip : ScrollableControl, IArrangedElement, IComponent, IDisposable, ISupportToolStripPanel
    {
        private ArrayList activeDropDowns;
        private bool alreadyHooked;
        private CachedItemHdcInfo cachedItemHdcInfo;
        internal static readonly TraceSwitch ControlTabDebug;
        private ToolStripItem currentlyActiveTooltipItem;
        private System.Type currentRendererType;
        private System.Drawing.Font defaultFont;
        private ToolStripItemCollection displayedItems;
        private NativeWindow dropDownOwnerWindow;
        internal static readonly TraceSwitch DropTargetDebug;
        private ToolStripDropTargetManager dropTargetManager;
        private static readonly object EventBeginDrag = new object();
        private static readonly object EventEndDrag = new object();
        private static readonly object EventItemAdded = new object();
        private static readonly object EventItemClicked = new object();
        private static readonly object EventItemRemoved = new object();
        private static readonly object EventLayoutCompleted = new object();
        private static readonly object EventLayoutStyleChanged = new object();
        private static readonly object EventLocationChanging = new object();
        private static readonly object EventPaintGrip = new object();
        private static readonly object EventRendererChanged = new object();
        internal static readonly TraceSwitch FlickerDebug;
        private IntPtr hwndThatLostFocus;
        private System.Windows.Forms.ImageList imageList;
        private Size imageScalingSize;
        internal const int INSERTION_BEAM_WIDTH = 6;
        internal static Point InvalidMouseEnter = new Point(0x7fffffff, 0x7fffffff);
        internal static readonly TraceSwitch ItemReorderDebug;
        private ISupportOleDropSource itemReorderDropSource;
        private IDropTarget itemReorderDropTarget;
        private Size largestDisplayedItemSize;
        private Rectangle lastInsertionMarkRect;
        private ToolStripItem lastMouseActiveItem;
        private ToolStripItem lastMouseDownedItem;
        internal static readonly TraceSwitch LayoutDebugSwitch;
        private System.Windows.Forms.Layout.LayoutEngine layoutEngine;
        private bool layoutRequired;
        private System.Windows.Forms.LayoutSettings layoutSettings;
        private ToolStripLayoutStyle layoutStyle;
        internal static readonly TraceSwitch MDIMergeDebug;
        internal static readonly TraceSwitch MenuAutoExpandDebug;
        internal static readonly TraceSwitch MergeDebug;
        private Stack<MergeHistory> mergeHistoryStack;
        internal static readonly TraceSwitch MouseActivateDebug;
        private byte mouseDownID;
        private Point mouseEnterWhenShown;
        private System.Windows.Forms.MouseHoverTimer mouseHoverTimer;
        private static Size onePixel = new Size(1, 1);
        private System.Windows.Forms.Orientation orientation;
        private ToolStripItemCollection overflowItems;
        private static readonly int PropBindingContext = PropertyStore.CreateKey();
        private static readonly int PropTextDirection = PropertyStore.CreateKey();
        private static readonly int PropToolStripPanelCell = PropertyStore.CreateKey();
        private static readonly int PropToolTip = PropertyStore.CreateKey();
        private ToolStripRenderer renderer;
        private RestoreFocusMessageFilter restoreFocusFilter;
        internal static readonly TraceSwitch SelectionDebug;
        private Hashtable shortcuts;
        private bool showItemToolTips;
        internal static readonly TraceSwitch SnapFocusDebug;
        internal const int STATE_ALLOWITEMREORDER = 2;
        internal const int STATE_ALLOWMERGE = 0x80;
        internal const int STATE_CANOVERFLOW = 1;
        internal const int STATE_DISPOSINGITEMS = 4;
        internal const int STATE_DRAGGING = 0x800;
        internal const int STATE_HASVISIBLEITEMS = 0x1000;
        internal const int STATE_LASTMOUSEDOWNEDITEMCAPTURE = 0x4000;
        internal const int STATE_LOCATIONCHANGING = 0x400;
        internal const int STATE_MENUACTIVE = 0x8000;
        internal const int STATE_MENUAUTOEXPAND = 8;
        internal const int STATE_MENUAUTOEXPANDDEFAULT = 0x10;
        internal const int STATE_RAFTING = 0x100;
        internal const int STATE_SCROLLBUTTONS = 0x20;
        internal const int STATE_STRETCH = 0x200;
        internal const int STATE_SUSPENDCAPTURE = 0x2000;
        internal const int STATE_USEDEFAULTRENDERER = 0x40;
        private ToolStripDropDownDirection toolStripDropDownDirection;
        private ToolStripGrip toolStripGrip;
        private ToolStripGripStyle toolStripGripStyle;
        private ToolStripItemCollection toolStripItemCollection;
        private ToolStripOverflowButton toolStripOverflowButton;
        private int toolStripState;

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnAutoSizeChangedDescr"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler AutoSizeChanged
        {
            add
            {
                base.AutoSizeChanged += value;
            }
            remove
            {
                base.AutoSizeChanged -= value;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripOnBeginDrag"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler BeginDrag
        {
            add
            {
                base.Events.AddHandler(EventBeginDrag, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventBeginDrag, value);
            }
        }

        [Browsable(false)]
        public event EventHandler CausesValidationChanged
        {
            add
            {
                base.CausesValidationChanged += value;
            }
            remove
            {
                base.CausesValidationChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event ControlEventHandler ControlAdded
        {
            add
            {
                base.ControlAdded += value;
            }
            remove
            {
                base.ControlAdded -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event ControlEventHandler ControlRemoved
        {
            add
            {
                base.ControlRemoved += value;
            }
            remove
            {
                base.ControlRemoved -= value;
            }
        }

        [Browsable(false)]
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

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ToolStripOnEndDrag")]
        public event EventHandler EndDrag
        {
            add
            {
                base.Events.AddHandler(EventEndDrag, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventEndDrag, value);
            }
        }

        [Browsable(false)]
        public event EventHandler ForeColorChanged
        {
            add
            {
                base.ForeColorChanged += value;
            }
            remove
            {
                base.ForeColorChanged -= value;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemAddedDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public event ToolStripItemEventHandler ItemAdded
        {
            add
            {
                base.Events.AddHandler(EventItemAdded, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventItemAdded, value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemOnClickDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event ToolStripItemClickedEventHandler ItemClicked
        {
            add
            {
                base.Events.AddHandler(EventItemClicked, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventItemClicked, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripItemRemovedDescr")]
        public event ToolStripItemEventHandler ItemRemoved
        {
            add
            {
                base.Events.AddHandler(EventItemRemoved, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventItemRemoved, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripLayoutCompleteDescr")]
        public event EventHandler LayoutCompleted
        {
            add
            {
                base.Events.AddHandler(EventLayoutCompleted, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLayoutCompleted, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripLayoutStyleChangedDescr")]
        public event EventHandler LayoutStyleChanged
        {
            add
            {
                base.Events.AddHandler(EventLayoutStyleChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLayoutStyleChanged, value);
            }
        }

        internal event ToolStripLocationCancelEventHandler LocationChanging
        {
            add
            {
                base.Events.AddHandler(EventLocationChanging, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLocationChanging, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripPaintGripDescr")]
        public event PaintEventHandler PaintGrip
        {
            add
            {
                base.Events.AddHandler(EventPaintGrip, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPaintGrip, value);
            }
        }

        public event EventHandler RendererChanged
        {
            add
            {
                base.Events.AddHandler(EventRendererChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRendererChanged, value);
            }
        }

        public ToolStrip()
        {
            this.hwndThatLostFocus = IntPtr.Zero;
            this.lastInsertionMarkRect = Rectangle.Empty;
            this.toolStripGripStyle = ToolStripGripStyle.Visible;
            this.activeDropDowns = new ArrayList(1);
            this.currentRendererType = typeof(System.Type);
            this.toolStripDropDownDirection = ToolStripDropDownDirection.Default;
            this.largestDisplayedItemSize = Size.Empty;
            this.imageScalingSize = new Size(0x10, 0x10);
            this.mouseEnterWhenShown = InvalidMouseEnter;
            base.SuspendLayout();
            this.CanOverflow = true;
            this.TabStop = false;
            this.MenuAutoExpand = false;
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);
            base.SetStyle(ControlStyles.Selectable, false);
            this.SetToolStripState(0xc0, true);
            base.SetState2(0x810, true);
            ToolStripManager.ToolStrips.Add(this);
            this.layoutEngine = new ToolStripSplitStackLayout(this);
            this.Dock = this.DefaultDock;
            this.AutoSize = true;
            this.CausesValidation = false;
            Size defaultSize = this.DefaultSize;
            base.SetAutoSizeMode(AutoSizeMode.GrowAndShrink);
            this.ShowItemToolTips = this.DefaultShowItemToolTips;
            base.ResumeLayout(true);
        }

        public ToolStrip(params ToolStripItem[] items) : this()
        {
            this.Items.AddRange(items);
        }

        internal virtual void ChangeSelection(ToolStripItem nextItem)
        {
            if (nextItem != null)
            {
                ToolStripControlHost host = nextItem as ToolStripControlHost;
                if (base.ContainsFocus && !this.Focused)
                {
                    this.FocusInternal();
                    if (host == null)
                    {
                        this.KeyboardActive = true;
                    }
                }
                if (host != null)
                {
                    if (this.hwndThatLostFocus == IntPtr.Zero)
                    {
                        this.SnapFocus(System.Windows.Forms.UnsafeNativeMethods.GetFocus());
                    }
                    host.Control.Select();
                    host.Control.FocusInternal();
                }
                nextItem.Select();
                ToolStripMenuItem item = nextItem as ToolStripMenuItem;
                if ((item != null) && !this.IsDropDown)
                {
                    item.HandleAutoExpansion();
                }
            }
        }

        private void ClearAllSelections()
        {
            this.ClearAllSelectionsExcept(null);
        }

        private void ClearAllSelectionsExcept(ToolStripItem item)
        {
            Rectangle rect = (item == null) ? Rectangle.Empty : item.Bounds;
            using (Region region = null)
            {
                for (int i = 0; i < this.DisplayedItems.Count; i++)
                {
                    if (this.DisplayedItems[i] != item)
                    {
                        if ((item != null) && this.DisplayedItems[i].Pressed)
                        {
                            ToolStripDropDownItem item2 = this.DisplayedItems[i] as ToolStripDropDownItem;
                            if ((item2 != null) && item2.HasDropDownItems)
                            {
                                item2.AutoHide(item);
                            }
                        }
                        bool flag = false;
                        if (this.DisplayedItems[i].Selected)
                        {
                            this.DisplayedItems[i].Unselect();
                            flag = true;
                        }
                        if (flag)
                        {
                            if (region == null)
                            {
                                region = new Region(rect);
                            }
                            region.Union(this.DisplayedItems[i].Bounds);
                        }
                    }
                }
                if (region != null)
                {
                    base.Invalidate(region, true);
                    base.Update();
                }
                else if (rect != Rectangle.Empty)
                {
                    base.Invalidate(rect, true);
                    base.Update();
                }
            }
            if (base.IsHandleCreated && (item != null))
            {
                int index = this.DisplayedItems.IndexOf(item);
                base.AccessibilityNotifyClients(AccessibleEvents.Focus, index);
            }
        }

        internal void ClearInsertionMark()
        {
            if (this.lastInsertionMarkRect != Rectangle.Empty)
            {
                Rectangle lastInsertionMarkRect = this.lastInsertionMarkRect;
                this.lastInsertionMarkRect = Rectangle.Empty;
                base.Invalidate(lastInsertionMarkRect);
            }
        }

        private void ClearLastMouseDownedItem()
        {
            ToolStripItem lastMouseDownedItem = this.lastMouseDownedItem;
            this.lastMouseDownedItem = null;
            if (this.IsSelectionSuspended)
            {
                this.SetToolStripState(0x4000, false);
                if (lastMouseDownedItem != null)
                {
                    lastMouseDownedItem.Invalidate();
                }
            }
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new ToolStripAccessibleObject(this);
        }

        protected override Control.ControlCollection CreateControlsInstance()
        {
            return new WindowsFormsUtils.ReadOnlyControlCollection(this, !base.DesignMode);
        }

        protected internal virtual ToolStripItem CreateDefaultItem(string text, Image image, EventHandler onClick)
        {
            if (text == "-")
            {
                return new ToolStripSeparator();
            }
            return new ToolStripButton(text, image, onClick);
        }

        protected virtual System.Windows.Forms.LayoutSettings CreateLayoutSettings(ToolStripLayoutStyle layoutStyle)
        {
            switch (layoutStyle)
            {
                case ToolStripLayoutStyle.Flow:
                    return new FlowLayoutSettings(this);

                case ToolStripLayoutStyle.Table:
                    return new TableLayoutSettings(this);
            }
            return null;
        }

        private bool DeferOverflowDropDownLayout()
        {
            if (!base.IsLayoutSuspended && this.OverflowButton.DropDown.Visible)
            {
                return !this.OverflowButton.DropDown.IsHandleCreated;
            }
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ToolStripOverflow overflow = this.GetOverflow();
                try
                {
                    base.SuspendLayout();
                    if (overflow != null)
                    {
                        overflow.SuspendLayout();
                    }
                    this.SetToolStripState(4, true);
                    this.lastMouseDownedItem = null;
                    this.HookStaticEvents(false);
                    System.Windows.Forms.ToolStripPanelCell cell = base.Properties.GetObject(PropToolStripPanelCell) as System.Windows.Forms.ToolStripPanelCell;
                    if (cell != null)
                    {
                        cell.Dispose();
                    }
                    if (this.cachedItemHdcInfo != null)
                    {
                        this.cachedItemHdcInfo.Dispose();
                    }
                    if (this.mouseHoverTimer != null)
                    {
                        this.mouseHoverTimer.Dispose();
                    }
                    System.Windows.Forms.ToolTip tip = (System.Windows.Forms.ToolTip) base.Properties.GetObject(PropToolTip);
                    if (tip != null)
                    {
                        tip.Dispose();
                    }
                    if (!this.Items.IsReadOnly)
                    {
                        for (int i = this.Items.Count - 1; i >= 0; i--)
                        {
                            this.Items[i].Dispose();
                        }
                        this.Items.Clear();
                    }
                    if (this.toolStripGrip != null)
                    {
                        this.toolStripGrip.Dispose();
                    }
                    if (this.toolStripOverflowButton != null)
                    {
                        this.toolStripOverflowButton.Dispose();
                    }
                    if (this.restoreFocusFilter != null)
                    {
                        Application.ThreadContext.FromCurrent().RemoveMessageFilter(this.restoreFocusFilter);
                        this.restoreFocusFilter = null;
                    }
                    bool flag = false;
                    if (ToolStripManager.ModalMenuFilter.GetActiveToolStrip() == this)
                    {
                        flag = true;
                    }
                    ToolStripManager.ModalMenuFilter.RemoveActiveToolStrip(this);
                    if (flag && (ToolStripManager.ModalMenuFilter.GetActiveToolStrip() == null))
                    {
                        ToolStripManager.ModalMenuFilter.ExitMenuMode();
                    }
                    ToolStripManager.ToolStrips.Remove(this);
                }
                finally
                {
                    base.ResumeLayout(false);
                    if (overflow != null)
                    {
                        overflow.ResumeLayout(false);
                    }
                    this.SetToolStripState(4, false);
                }
            }
            base.Dispose(disposing);
        }

        internal void DoLayoutIfHandleCreated(ToolStripItemEventArgs e)
        {
            if (base.IsHandleCreated)
            {
                LayoutTransaction.DoLayout(this, e.Item, PropertyNames.Items);
                base.Invalidate();
                if (this.CanOverflow && this.OverflowButton.HasDropDown)
                {
                    if (this.DeferOverflowDropDownLayout())
                    {
                        CommonProperties.xClearPreferredSizeCache(this.OverflowButton.DropDown);
                        this.OverflowButton.DropDown.LayoutRequired = true;
                    }
                    else
                    {
                        LayoutTransaction.DoLayout(this.OverflowButton.DropDown, e.Item, PropertyNames.Items);
                        this.OverflowButton.DropDown.Invalidate();
                    }
                }
            }
            else
            {
                CommonProperties.xClearPreferredSizeCache(this);
                this.LayoutRequired = true;
                if (this.CanOverflow && this.OverflowButton.HasDropDown)
                {
                    this.OverflowButton.DropDown.LayoutRequired = true;
                }
            }
        }

        private void EraseCorners(PaintEventArgs e, Region transparentRegion)
        {
            if (transparentRegion != null)
            {
                base.PaintTransparentBackground(e, base.ClientRectangle, transparentRegion);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Control GetChildAtPoint(Point point)
        {
            return base.GetChildAtPoint(point);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Control GetChildAtPoint(Point pt, GetChildAtPointSkip skipValue)
        {
            return base.GetChildAtPoint(pt, skipValue);
        }

        internal override Control GetFirstChildControlInTabOrder(bool forward)
        {
            return null;
        }

        public ToolStripItem GetItemAt(Point point)
        {
            Rectangle rect = new Rectangle(point, onePixel);
            if (((this.lastMouseActiveItem != null) && this.lastMouseActiveItem.Bounds.IntersectsWith(rect)) && (this.lastMouseActiveItem.ParentInternal == this))
            {
                return this.lastMouseActiveItem;
            }
            for (int i = 0; i < this.DisplayedItems.Count; i++)
            {
                if ((this.DisplayedItems[i] != null) && (this.DisplayedItems[i].ParentInternal == this))
                {
                    Rectangle bounds = this.DisplayedItems[i].Bounds;
                    if ((this.toolStripGrip != null) && (this.DisplayedItems[i] == this.toolStripGrip))
                    {
                        bounds = LayoutUtils.InflateRect(bounds, this.GripMargin);
                    }
                    if (bounds.IntersectsWith(rect))
                    {
                        return this.DisplayedItems[i];
                    }
                }
            }
            return null;
        }

        public ToolStripItem GetItemAt(int x, int y)
        {
            return this.GetItemAt(new Point(x, y));
        }

        internal static Graphics GetMeasurementGraphics()
        {
            return WindowsFormsUtils.CreateMeasurementGraphics();
        }

        internal byte GetMouseId()
        {
            if (this.mouseDownID == 0)
            {
                this.mouseDownID = (byte) (this.mouseDownID + 1);
            }
            return this.mouseDownID;
        }

        public virtual ToolStripItem GetNextItem(ToolStripItem start, ArrowDirection direction)
        {
            if (!WindowsFormsUtils.EnumValidator.IsValidArrowDirection(direction))
            {
                throw new InvalidEnumArgumentException("direction", (int) direction, typeof(ArrowDirection));
            }
            switch (direction)
            {
                case ArrowDirection.Left:
                    return this.GetNextItemHorizontal(start, false);

                case ArrowDirection.Up:
                    return this.GetNextItemVertical(start, false);

                case ArrowDirection.Right:
                    return this.GetNextItemHorizontal(start, true);

                case ArrowDirection.Down:
                    return this.GetNextItemVertical(start, true);
            }
            return null;
        }

        internal virtual ToolStripItem GetNextItem(ToolStripItem start, ArrowDirection direction, bool rtlAware)
        {
            if (rtlAware && (this.RightToLeft == RightToLeft.Yes))
            {
                if (direction == ArrowDirection.Right)
                {
                    direction = ArrowDirection.Left;
                }
                else if (direction == ArrowDirection.Left)
                {
                    direction = ArrowDirection.Right;
                }
            }
            return this.GetNextItem(start, direction);
        }

        private ToolStripItem GetNextItemHorizontal(ToolStripItem start, bool forward)
        {
            if (this.DisplayedItems.Count > 0)
            {
                if (start == null)
                {
                    start = forward ? this.DisplayedItems[this.DisplayedItems.Count - 1] : this.DisplayedItems[0];
                }
                int index = this.DisplayedItems.IndexOf(start);
                if (index == -1)
                {
                    return null;
                }
                int count = this.DisplayedItems.Count;
                do
                {
                    if (forward)
                    {
                        index = ++index % count;
                    }
                    else
                    {
                        index = (--index < 0) ? (count + index) : index;
                    }
                    ToolStripDropDown down = this as ToolStripDropDown;
                    if (((down != null) && (down.OwnerItem != null)) && down.OwnerItem.IsInDesignMode)
                    {
                        return this.DisplayedItems[index];
                    }
                    if (this.DisplayedItems[index].CanKeyboardSelect)
                    {
                        return this.DisplayedItems[index];
                    }
                }
                while (this.DisplayedItems[index] != start);
            }
            return null;
        }

        private ToolStripItem GetNextItemVertical(ToolStripItem selectedItem, bool down)
        {
            ToolStripItem item = null;
            ToolStripItem item2 = null;
            double maxValue = double.MaxValue;
            double num2 = double.MaxValue;
            double num3 = double.MaxValue;
            if (selectedItem == null)
            {
                return this.GetNextItemHorizontal(selectedItem, down);
            }
            ToolStripDropDown down2 = this as ToolStripDropDown;
            if (((down2 != null) && (down2.OwnerItem != null)) && (down2.OwnerItem.IsInDesignMode || ((down2.OwnerItem.Owner != null) && down2.OwnerItem.Owner.IsInDesignMode)))
            {
                return this.GetNextItemHorizontal(selectedItem, down);
            }
            Point point = new Point(selectedItem.Bounds.X + (selectedItem.Width / 2), selectedItem.Bounds.Y + (selectedItem.Height / 2));
            for (int i = 0; i < this.DisplayedItems.Count; i++)
            {
                ToolStripItem item5 = this.DisplayedItems[i];
                if ((((item5 != selectedItem) && item5.CanKeyboardSelect) && (down || (item5.Bounds.Bottom <= selectedItem.Bounds.Top))) && (!down || (item5.Bounds.Top >= selectedItem.Bounds.Bottom)))
                {
                    Point point2 = new Point(item5.Bounds.X + (item5.Width / 2), down ? item5.Bounds.Top : item5.Bounds.Bottom);
                    int num5 = point2.X - point.X;
                    int num6 = point2.Y - point.Y;
                    double num7 = Math.Sqrt((double) ((num6 * num6) + (num5 * num5)));
                    if (num6 != 0)
                    {
                        double num8 = Math.Abs(Math.Atan((double) (num5 / num6)));
                        num2 = Math.Min(num2, num8);
                        maxValue = Math.Min(maxValue, num7);
                        if ((num2 == num8) && (num2 != double.NaN))
                        {
                            item = item5;
                        }
                        if (maxValue == num7)
                        {
                            item2 = item5;
                            num3 = num8;
                        }
                    }
                }
            }
            if ((item == null) || (item2 == null))
            {
                return this.GetNextItemHorizontal(null, down);
            }
            if (num3 != num2)
            {
                if (!down && (item.Bounds.Bottom <= item2.Bounds.Top))
                {
                    return item2;
                }
                if (!down || (item.Bounds.Top <= item2.Bounds.Bottom))
                {
                    return item;
                }
            }
            return item2;
        }

        internal ToolStripOverflow GetOverflow()
        {
            if ((this.toolStripOverflowButton != null) && this.toolStripOverflowButton.HasDropDown)
            {
                return (this.toolStripOverflowButton.DropDown as ToolStripOverflow);
            }
            return null;
        }

        internal virtual Control GetOwnerControl()
        {
            return this;
        }

        private static Size GetPreferredItemSize(ToolStripItem item)
        {
            if (!item.AutoSize)
            {
                return item.Size;
            }
            return item.GetPreferredSize(Size.Empty);
        }

        internal override Size GetPreferredSizeCore(Size proposedSize)
        {
            if (proposedSize.Width == 1)
            {
                proposedSize.Width = 0x7fffffff;
            }
            if (proposedSize.Height == 1)
            {
                proposedSize.Height = 0x7fffffff;
            }
            Padding padding = base.Padding;
            Size preferredSize = this.LayoutEngine.GetPreferredSize(this, proposedSize - padding.Size);
            Padding padding2 = base.Padding;
            if (padding != padding2)
            {
                CommonProperties.xClearPreferredSizeCache(this);
            }
            return (preferredSize + padding2.Size);
        }

        internal static Size GetPreferredSizeHorizontal(IArrangedElement container, Size proposedConstraints)
        {
            Size empty = Size.Empty;
            ToolStrip strip = container as ToolStrip;
            Size size2 = strip.DefaultSize - strip.Padding.Size;
            empty.Height = Math.Max(0, size2.Height);
            bool flag = false;
            bool flag2 = false;
            for (int i = 0; i < strip.Items.Count; i++)
            {
                ToolStripItem item = strip.Items[i];
                if (((IArrangedElement) item).ParticipatesInLayout)
                {
                    flag2 = true;
                    if (item.Overflow != ToolStripItemOverflow.Always)
                    {
                        Padding margin = item.Margin;
                        Size preferredItemSize = GetPreferredItemSize(item);
                        empty.Width += margin.Horizontal + preferredItemSize.Width;
                        empty.Height = Math.Max(empty.Height, margin.Vertical + preferredItemSize.Height);
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            if ((strip.Items.Count == 0) || !flag2)
            {
                empty = size2;
            }
            if (flag)
            {
                ToolStripOverflowButton overflowButton = strip.OverflowButton;
                Padding padding2 = overflowButton.Margin;
                empty.Width += padding2.Horizontal + overflowButton.Bounds.Width;
            }
            else
            {
                empty.Width += 2;
            }
            if (strip.GripStyle == ToolStripGripStyle.Visible)
            {
                Padding gripMargin = strip.GripMargin;
                empty.Width += gripMargin.Horizontal + strip.Grip.GripThickness;
            }
            return LayoutUtils.IntersectSizes(empty, proposedConstraints);
        }

        internal static Size GetPreferredSizeVertical(IArrangedElement container, Size proposedConstraints)
        {
            Size empty = Size.Empty;
            bool flag = false;
            ToolStrip element = container as ToolStrip;
            bool flag2 = false;
            for (int i = 0; i < element.Items.Count; i++)
            {
                ToolStripItem item = element.Items[i];
                if (((IArrangedElement) item).ParticipatesInLayout)
                {
                    flag2 = true;
                    if (item.Overflow != ToolStripItemOverflow.Always)
                    {
                        Size preferredItemSize = GetPreferredItemSize(item);
                        Padding margin = item.Margin;
                        empty.Height += margin.Vertical + preferredItemSize.Height;
                        empty.Width = Math.Max(empty.Width, margin.Horizontal + preferredItemSize.Width);
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            if ((element.Items.Count == 0) || !flag2)
            {
                empty = LayoutUtils.FlipSize(element.DefaultSize);
            }
            if (flag)
            {
                ToolStripOverflowButton overflowButton = element.OverflowButton;
                Padding padding2 = overflowButton.Margin;
                empty.Height += padding2.Vertical + overflowButton.Bounds.Height;
            }
            else
            {
                empty.Height += 2;
            }
            if (element.GripStyle == ToolStripGripStyle.Visible)
            {
                Padding gripMargin = element.GripMargin;
                empty.Height += gripMargin.Vertical + element.Grip.GripThickness;
            }
            if (element.Size != empty)
            {
                CommonProperties.xClearPreferredSizeCache(element);
            }
            return empty;
        }

        internal ToolStripItem GetSelectedItem()
        {
            ToolStripItem item = null;
            for (int i = 0; i < this.DisplayedItems.Count; i++)
            {
                if (this.DisplayedItems[i].Selected)
                {
                    item = this.DisplayedItems[i];
                }
            }
            return item;
        }

        internal bool GetToolStripState(int flag)
        {
            return ((this.toolStripState & flag) != 0);
        }

        internal virtual ToolStrip GetToplevelOwnerToolStrip()
        {
            return this;
        }

        internal void HandleItemClick(ToolStripItem dismissingItem)
        {
            ToolStripItemClickedEventArgs e = new ToolStripItemClickedEventArgs(dismissingItem);
            this.OnItemClicked(e);
            if (!this.IsDropDown && dismissingItem.IsOnOverflow)
            {
                this.OverflowButton.DropDown.HandleItemClick(dismissingItem);
            }
        }

        internal virtual void HandleItemClicked(ToolStripItem dismissingItem)
        {
            ToolStripDropDownItem item = dismissingItem as ToolStripDropDownItem;
            if ((item != null) && !item.HasDropDownItems)
            {
                this.KeyboardActive = false;
            }
        }

        private void HandleMouseLeave()
        {
            if (this.lastMouseActiveItem != null)
            {
                if (!base.DesignMode)
                {
                    this.MouseHoverTimer.Cancel(this.lastMouseActiveItem);
                }
                try
                {
                    this.lastMouseActiveItem.FireEvent(EventArgs.Empty, ToolStripItemEventType.MouseLeave);
                }
                finally
                {
                    this.lastMouseActiveItem = null;
                }
            }
            ToolStripMenuItem.MenuTimer.HandleToolStripMouseLeave(this);
        }

        private void HookStaticEvents(bool hook)
        {
            if (hook)
            {
                if (!this.alreadyHooked)
                {
                    try
                    {
                        ToolStripManager.RendererChanged += new EventHandler(this.OnDefaultRendererChanged);
                        SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
                    }
                    finally
                    {
                        this.alreadyHooked = true;
                    }
                }
            }
            else if (this.alreadyHooked)
            {
                try
                {
                    ToolStripManager.RendererChanged -= new EventHandler(this.OnDefaultRendererChanged);
                    SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
                }
                finally
                {
                    this.alreadyHooked = false;
                }
            }
        }

        private void ImageListRecreateHandle(object sender, EventArgs e)
        {
            base.Invalidate();
        }

        private void InitializeRenderer(ToolStripRenderer renderer)
        {
            using (LayoutTransaction.CreateTransactionIf(this.AutoSize, this, this, PropertyNames.Renderer))
            {
                renderer.Initialize(this);
                for (int i = 0; i < this.Items.Count; i++)
                {
                    renderer.InitializeItem(this.Items[i]);
                }
            }
            base.Invalidate(this.Controls.Count > 0);
        }

        private void InvalidateLayout()
        {
            if (base.IsHandleCreated)
            {
                LayoutTransaction.DoLayout(this, this, null);
            }
        }

        internal void InvalidateTextItems()
        {
            using (new LayoutTransaction(this, this, "ShowKeyboardFocusCues", base.Visible))
            {
                for (int i = 0; i < this.DisplayedItems.Count; i++)
                {
                    if ((this.DisplayedItems[i].DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text)
                    {
                        this.DisplayedItems[i].InvalidateItemLayout("ShowKeyboardFocusCues");
                    }
                }
            }
        }

        internal void InvokePaintItem(ToolStripItem item)
        {
            base.Invalidate(item.Bounds);
            base.Update();
        }

        protected override bool IsInputChar(char charCode)
        {
            ToolStripItem selectedItem = this.GetSelectedItem();
            return (((selectedItem != null) && selectedItem.IsInputChar(charCode)) || base.IsInputChar(charCode));
        }

        protected override bool IsInputKey(Keys keyData)
        {
            ToolStripItem selectedItem = this.GetSelectedItem();
            return (((selectedItem != null) && selectedItem.IsInputKey(keyData)) || base.IsInputKey(keyData));
        }

        private static bool IsPseudoMnemonic(char charCode, string text)
        {
            if (!string.IsNullOrEmpty(text) && !WindowsFormsUtils.ContainsMnemonic(text))
            {
                char ch = char.ToUpper(charCode, CultureInfo.CurrentCulture);
                if ((char.ToUpper(text[0], CultureInfo.CurrentCulture) == ch) || (char.ToLower(charCode, CultureInfo.CurrentCulture) == char.ToLower(text[0], CultureInfo.CurrentCulture)))
                {
                    return true;
                }
            }
            return false;
        }

        internal void NotifySelectionChange(ToolStripItem item)
        {
            if (item == null)
            {
                this.ClearAllSelections();
            }
            else if (item.Selected)
            {
                this.ClearAllSelectionsExcept(item);
            }
        }

        protected virtual void OnBeginDrag(EventArgs e)
        {
            this.SetToolStripState(0x800, true);
            this.ClearAllSelections();
            this.UpdateToolTip(null);
            EventHandler handler = (EventHandler) base.Events[EventBeginDrag];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void OnDefaultFontChanged()
        {
            this.defaultFont = null;
            if (!base.IsFontSet())
            {
                this.OnFontChanged(EventArgs.Empty);
            }
        }

        private void OnDefaultRendererChanged(object sender, EventArgs e)
        {
            if (this.GetToolStripState(0x40))
            {
                this.OnRendererChanged(e);
            }
        }

        protected override void OnDockChanged(EventArgs e)
        {
            base.OnDockChanged(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            for (int i = 0; i < this.Items.Count; i++)
            {
                if ((this.Items[i] != null) && (this.Items[i].ParentInternal == this))
                {
                    this.Items[i].OnParentEnabledChanged(e);
                }
            }
        }

        protected virtual void OnEndDrag(EventArgs e)
        {
            this.SetToolStripState(0x800, false);
            EventHandler handler = (EventHandler) base.Events[EventEndDrag];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            for (int i = 0; i < this.Items.Count; i++)
            {
                this.Items[i].OnOwnerFontChanged(e);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if ((this.AllowDrop || this.AllowItemReorder) && (this.DropTargetManager != null))
            {
                this.DropTargetManager.EnsureRegistered(this);
            }
            base.OnHandleCreated(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (this.DropTargetManager != null)
            {
                this.DropTargetManager.EnsureUnRegistered(this);
            }
            base.OnHandleDestroyed(e);
        }

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            base.OnInvalidated(e);
        }

        protected internal virtual void OnItemAdded(ToolStripItemEventArgs e)
        {
            this.DoLayoutIfHandleCreated(e);
            if ((!this.HasVisibleItems && (e.Item != null)) && ((IArrangedElement) e.Item).ParticipatesInLayout)
            {
                this.HasVisibleItems = true;
            }
            ToolStripItemEventHandler handler = (ToolStripItemEventHandler) base.Events[EventItemAdded];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            ToolStripItemClickedEventHandler handler = (ToolStripItemClickedEventHandler) base.Events[EventItemClicked];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal virtual void OnItemRemoved(ToolStripItemEventArgs e)
        {
            this.OnItemVisibleChanged(e, true);
            ToolStripItemEventHandler handler = (ToolStripItemEventHandler) base.Events[EventItemRemoved];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void OnItemVisibleChanged(ToolStripItemEventArgs e, bool performLayout)
        {
            if (e.Item == this.lastMouseActiveItem)
            {
                this.lastMouseActiveItem = null;
            }
            if (e.Item == this.LastMouseDownedItem)
            {
                this.lastMouseDownedItem = null;
            }
            if (e.Item == this.currentlyActiveTooltipItem)
            {
                this.UpdateToolTip(null);
            }
            if (performLayout)
            {
                this.DoLayoutIfHandleCreated(e);
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            this.LayoutRequired = false;
            ToolStripOverflow overflow = this.GetOverflow();
            if (overflow != null)
            {
                overflow.SuspendLayout();
                this.toolStripOverflowButton.Size = this.toolStripOverflowButton.GetPreferredSize(this.DisplayRectangle.Size - base.Padding.Size);
            }
            for (int i = 0; i < this.Items.Count; i++)
            {
                this.Items[i].OnLayout(e);
            }
            base.OnLayout(e);
            this.SetDisplayedItems();
            this.OnLayoutCompleted(EventArgs.Empty);
            base.Invalidate();
            if (overflow != null)
            {
                overflow.ResumeLayout();
            }
        }

        protected virtual void OnLayoutCompleted(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventLayoutCompleted];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnLayoutStyleChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventLayoutStyleChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            if (!this.IsDropDown)
            {
                Application.ThreadContext.FromCurrent().RemoveMessageFilter(this.RestoreFocusFilter);
            }
        }

        internal virtual void OnLocationChanging(ToolStripLocationCancelEventArgs e)
        {
            ToolStripLocationCancelEventHandler handler = (ToolStripLocationCancelEventHandler) base.Events[EventLocationChanging];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            this.ClearAllSelections();
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            if (!this.GetToolStripState(0x2000))
            {
                this.Grip.MovingToolStrip = false;
            }
            this.ClearLastMouseDownedItem();
            base.OnMouseCaptureChanged(e);
        }

        protected override void OnMouseDown(MouseEventArgs mea)
        {
            this.mouseDownID = (byte) (this.mouseDownID + 1);
            ToolStripItem itemAt = this.GetItemAt(mea.X, mea.Y);
            if (itemAt != null)
            {
                if (!this.IsDropDown && !(itemAt is ToolStripDropDownItem))
                {
                    this.SetToolStripState(0x4000, true);
                    base.CaptureInternal = true;
                }
                this.MenuAutoExpand = true;
                if (mea != null)
                {
                    Point point = itemAt.TranslatePoint(new Point(mea.X, mea.Y), ToolStripPointType.ToolStripCoords, ToolStripPointType.ToolStripItemCoords);
                    mea = new MouseEventArgs(mea.Button, mea.Clicks, point.X, point.Y, mea.Delta);
                }
                this.lastMouseDownedItem = itemAt;
                itemAt.FireEvent(mea, ToolStripItemEventType.MouseDown);
            }
            else
            {
                base.OnMouseDown(mea);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.HandleMouseLeave();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs mea)
        {
            ToolStripItem itemAt = this.GetItemAt(mea.X, mea.Y);
            if (!this.Grip.MovingToolStrip)
            {
                if (itemAt != this.lastMouseActiveItem)
                {
                    this.HandleMouseLeave();
                    this.lastMouseActiveItem = (itemAt is ToolStripControlHost) ? null : itemAt;
                    if (this.lastMouseActiveItem != null)
                    {
                        itemAt.FireEvent(new EventArgs(), ToolStripItemEventType.MouseEnter);
                    }
                    if (!base.DesignMode)
                    {
                        this.MouseHoverTimer.Start(this.lastMouseActiveItem);
                    }
                }
            }
            else
            {
                itemAt = this.Grip;
            }
            if (itemAt != null)
            {
                Point point = itemAt.TranslatePoint(new Point(mea.X, mea.Y), ToolStripPointType.ToolStripCoords, ToolStripPointType.ToolStripItemCoords);
                mea = new MouseEventArgs(mea.Button, mea.Clicks, point.X, point.Y, mea.Delta);
                itemAt.FireEvent(mea, ToolStripItemEventType.MouseMove);
            }
            else
            {
                base.OnMouseMove(mea);
            }
        }

        protected override void OnMouseUp(MouseEventArgs mea)
        {
            ToolStripItem item = this.Grip.MovingToolStrip ? this.Grip : this.GetItemAt(mea.X, mea.Y);
            if (item != null)
            {
                if (mea != null)
                {
                    Point point = item.TranslatePoint(new Point(mea.X, mea.Y), ToolStripPointType.ToolStripCoords, ToolStripPointType.ToolStripItemCoords);
                    mea = new MouseEventArgs(mea.Button, mea.Clicks, point.X, point.Y, mea.Delta);
                }
                item.FireEvent(mea, ToolStripItemEventType.MouseUp);
            }
            else
            {
                base.OnMouseUp(mea);
            }
            this.ClearLastMouseDownedItem();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Size largestDisplayedItemSize = this.largestDisplayedItemSize;
            bool flag = false;
            Rectangle displayRectangle = this.DisplayRectangle;
            using (Region region = this.Renderer.GetTransparentRegion(this))
            {
                if (!LayoutUtils.IsZeroWidthOrHeight(largestDisplayedItemSize))
                {
                    if (region != null)
                    {
                        region.Intersect(g.Clip);
                        g.ExcludeClip(region);
                        flag = true;
                    }
                    using (WindowsGraphics graphics2 = WindowsGraphics.FromGraphics(g, ApplyGraphicsProperties.Clipping))
                    {
                        HandleRef toolStripHDC = new HandleRef(this, graphics2.GetHdc());
                        HandleRef cachedItemDC = this.ItemHdcInfo.GetCachedItemDC(toolStripHDC, largestDisplayedItemSize);
                        using (Graphics graphics3 = Graphics.FromHdcInternal(cachedItemDC.Handle))
                        {
                            for (int i = 0; i < this.DisplayedItems.Count; i++)
                            {
                                ToolStripItem item = this.DisplayedItems[i];
                                if (item != null)
                                {
                                    Rectangle clipRectangle = e.ClipRectangle;
                                    Rectangle bounds = item.Bounds;
                                    if (!this.IsDropDown && (item.Owner == this))
                                    {
                                        clipRectangle.Intersect(displayRectangle);
                                    }
                                    clipRectangle.Intersect(bounds);
                                    if (!LayoutUtils.IsZeroWidthOrHeight(clipRectangle))
                                    {
                                        Size size = item.Size;
                                        if (!LayoutUtils.AreWidthAndHeightLarger(largestDisplayedItemSize, size))
                                        {
                                            this.largestDisplayedItemSize = size;
                                            largestDisplayedItemSize = size;
                                            graphics3.Dispose();
                                            cachedItemDC = this.ItemHdcInfo.GetCachedItemDC(toolStripHDC, largestDisplayedItemSize);
                                            graphics3 = Graphics.FromHdcInternal(cachedItemDC.Handle);
                                        }
                                        clipRectangle.Offset(-bounds.X, -bounds.Y);
                                        System.Windows.Forms.SafeNativeMethods.BitBlt(cachedItemDC, 0, 0, item.Size.Width, item.Size.Height, toolStripHDC, item.Bounds.X, item.Bounds.Y, 0xcc0020);
                                        using (PaintEventArgs args = new PaintEventArgs(graphics3, clipRectangle))
                                        {
                                            item.FireEvent(args, ToolStripItemEventType.Paint);
                                        }
                                        System.Windows.Forms.SafeNativeMethods.BitBlt(toolStripHDC, item.Bounds.X, item.Bounds.Y, item.Size.Width, item.Size.Height, cachedItemDC, 0, 0, 0xcc0020);
                                    }
                                }
                            }
                        }
                    }
                }
                this.Renderer.DrawToolStripBorder(new ToolStripRenderEventArgs(g, this));
                if (flag)
                {
                    g.SetClip(region, CombineMode.Union);
                }
                this.PaintInsertionMark(g);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            Graphics g = e.Graphics;
            System.Drawing.Drawing2D.GraphicsState gstate = g.Save();
            try
            {
                using (Region region = this.Renderer.GetTransparentRegion(this))
                {
                    if (region != null)
                    {
                        this.EraseCorners(e, region);
                        g.ExcludeClip(region);
                    }
                }
                this.Renderer.DrawToolStripBackground(new ToolStripRenderEventArgs(g, this));
            }
            finally
            {
                if (gstate != null)
                {
                    g.Restore(gstate);
                }
            }
        }

        protected internal virtual void OnPaintGrip(PaintEventArgs e)
        {
            this.Renderer.DrawGrip(new ToolStripGripRenderEventArgs(e.Graphics, this));
            PaintEventHandler handler = (PaintEventHandler) base.Events[EventPaintGrip];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnRendererChanged(EventArgs e)
        {
            this.InitializeRenderer(this.Renderer);
            EventHandler handler = (EventHandler) base.Events[EventRendererChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            using (new LayoutTransaction(this, this, PropertyNames.RightToLeft))
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    this.Items[i].OnParentRightToLeftChanged(e);
                }
                if (this.toolStripOverflowButton != null)
                {
                    this.toolStripOverflowButton.OnParentRightToLeftChanged(e);
                }
                if (this.toolStripGrip != null)
                {
                    this.toolStripGrip.OnParentRightToLeftChanged(e);
                }
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            if ((se.Type != ScrollEventType.ThumbTrack) && (se.NewValue != se.OldValue))
            {
                this.ScrollInternal(se.OldValue - se.NewValue);
            }
            base.OnScroll(se);
        }

        protected override void OnTabStopChanged(EventArgs e)
        {
            base.SetStyle(ControlStyles.Selectable, this.TabStop);
            base.OnTabStopChanged(e);
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            switch (e.Category)
            {
                case UserPreferenceCategory.General:
                    this.InvalidateTextItems();
                    break;

                case UserPreferenceCategory.Window:
                    this.OnDefaultFontChanged();
                    break;
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!base.Disposing && !base.IsDisposed)
            {
                this.HookStaticEvents(base.Visible);
            }
        }

        internal void PaintInsertionMark(Graphics g)
        {
            if (this.lastInsertionMarkRect != Rectangle.Empty)
            {
                int num = 6;
                if (this.Orientation == System.Windows.Forms.Orientation.Horizontal)
                {
                    int x = this.lastInsertionMarkRect.X;
                    int num3 = x + 2;
                    Point[] points = new Point[] { new Point(num3, this.lastInsertionMarkRect.Y), new Point(num3, this.lastInsertionMarkRect.Bottom - 1), new Point(num3 + 1, this.lastInsertionMarkRect.Y), new Point(num3 + 1, this.lastInsertionMarkRect.Bottom - 1) };
                    g.DrawLines(SystemPens.ControlText, points);
                    Point[] pointArray2 = new Point[] { new Point(x, this.lastInsertionMarkRect.Bottom - 1), new Point((x + num) - 1, this.lastInsertionMarkRect.Bottom - 1), new Point(x + 1, this.lastInsertionMarkRect.Bottom - 2), new Point((x + num) - 2, this.lastInsertionMarkRect.Bottom - 2) };
                    g.DrawLines(SystemPens.ControlText, pointArray2);
                    Point[] pointArray3 = new Point[] { new Point(x, this.lastInsertionMarkRect.Y), new Point((x + num) - 1, this.lastInsertionMarkRect.Y), new Point(x + 1, this.lastInsertionMarkRect.Y + 1), new Point((x + num) - 2, this.lastInsertionMarkRect.Y + 1) };
                    g.DrawLines(SystemPens.ControlText, pointArray3);
                }
                else
                {
                    num = 6;
                    int y = this.lastInsertionMarkRect.Y;
                    int num5 = y + 2;
                    Point[] pointArray4 = new Point[] { new Point(this.lastInsertionMarkRect.X, num5), new Point(this.lastInsertionMarkRect.Right - 1, num5), new Point(this.lastInsertionMarkRect.X, num5 + 1), new Point(this.lastInsertionMarkRect.Right - 1, num5 + 1) };
                    g.DrawLines(SystemPens.ControlText, pointArray4);
                    Point[] pointArray5 = new Point[] { new Point(this.lastInsertionMarkRect.X, y), new Point(this.lastInsertionMarkRect.X, (y + num) - 1), new Point(this.lastInsertionMarkRect.X + 1, y + 1), new Point(this.lastInsertionMarkRect.X + 1, (y + num) - 2) };
                    g.DrawLines(SystemPens.ControlText, pointArray5);
                    Point[] pointArray6 = new Point[] { new Point(this.lastInsertionMarkRect.Right - 1, y), new Point(this.lastInsertionMarkRect.Right - 1, (y + num) - 1), new Point(this.lastInsertionMarkRect.Right - 2, y + 1), new Point(this.lastInsertionMarkRect.Right - 2, (y + num) - 2) };
                    g.DrawLines(SystemPens.ControlText, pointArray6);
                }
            }
        }

        internal void PaintInsertionMark(Rectangle insertionRect)
        {
            if (this.lastInsertionMarkRect != insertionRect)
            {
                this.ClearInsertionMark();
                this.lastInsertionMarkRect = insertionRect;
                base.Invalidate(insertionRect);
            }
        }

        internal void PaintParentRegion(Graphics g, Region region)
        {
        }

        internal override void PrintToMetaFileRecursive(HandleRef hDC, IntPtr lParam, Rectangle bounds)
        {
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    IntPtr hdc = graphics.GetHdc();
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x317, hdc, (IntPtr) 30);
                    IntPtr handle = hDC.Handle;
                    System.Windows.Forms.SafeNativeMethods.BitBlt(new HandleRef(this, handle), bounds.X, bounds.Y, bounds.Width, bounds.Height, new HandleRef(graphics, hdc), 0, 0, 0xcc0020);
                    graphics.ReleaseHdcInternal(hdc);
                }
            }
        }

        internal virtual bool ProcessArrowKey(Keys keyCode)
        {
            bool flag = false;
            ToolStripMenuItem.MenuTimer.Cancel();
            switch (keyCode)
            {
                case Keys.Left:
                case Keys.Right:
                    return this.ProcessLeftRightArrowKey(keyCode == Keys.Right);

                case Keys.Up:
                case Keys.Down:
                    if (this.IsDropDown || (this.Orientation != System.Windows.Forms.Orientation.Horizontal))
                    {
                        ToolStripItem selectedItem = this.GetSelectedItem();
                        if (keyCode == Keys.Down)
                        {
                            ToolStripItem item2 = this.GetNextItem(selectedItem, ArrowDirection.Down);
                            if (item2 != null)
                            {
                                this.ChangeSelection(item2);
                                flag = true;
                            }
                            return flag;
                        }
                        ToolStripItem nextItem = this.GetNextItem(selectedItem, ArrowDirection.Up);
                        if (nextItem != null)
                        {
                            this.ChangeSelection(nextItem);
                            flag = true;
                        }
                    }
                    return flag;
            }
            return flag;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            if ((ToolStripManager.IsMenuKey(keyData) && !this.IsDropDown) && ToolStripManager.ModalMenuFilter.InMenuMode)
            {
                this.ClearAllSelections();
                ToolStripManager.ModalMenuFilter.MenuKeyToggle = true;
                ToolStripManager.ModalMenuFilter.ExitMenuMode();
            }
            ToolStripItem selectedItem = this.GetSelectedItem();
            if ((selectedItem != null) && selectedItem.ProcessCmdKey(ref m, keyData))
            {
                return true;
            }
            foreach (ToolStripItem item2 in this.Items)
            {
                if ((item2 != selectedItem) && item2.ProcessCmdKey(ref m, keyData))
                {
                    return true;
                }
            }
            if ((!this.IsDropDown && (((keyData & Keys.Control) == Keys.Control) && ((keyData & Keys.KeyCode) == Keys.Tab))) && (!this.TabStop && this.HasKeyboardInput))
            {
                bool flag2 = false;
                if ((keyData & Keys.Shift) == Keys.None)
                {
                    flag2 = ToolStripManager.SelectNextToolStrip(this, true);
                }
                else
                {
                    flag2 = ToolStripManager.SelectNextToolStrip(this, false);
                }
                if (flag2)
                {
                    return true;
                }
            }
            return base.ProcessCmdKey(ref m, keyData);
        }

        internal bool ProcessCmdKeyInternal(ref Message m, Keys keyData)
        {
            return this.ProcessCmdKey(ref m, keyData);
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            bool flag = false;
            ToolStripItem selectedItem = this.GetSelectedItem();
            if ((selectedItem != null) && selectedItem.ProcessDialogKey(keyData))
            {
                return true;
            }
            bool flag2 = (keyData & (Keys.Alt | Keys.Control)) != Keys.None;
            Keys keyCode = keyData & Keys.KeyCode;
            switch (keyCode)
            {
                case Keys.Back:
                    if (!base.ContainsFocus)
                    {
                        flag = this.ProcessTabKey(false);
                    }
                    break;

                case Keys.Tab:
                    if (!flag2)
                    {
                        flag = this.ProcessTabKey((keyData & Keys.Shift) == Keys.None);
                    }
                    break;

                case Keys.Escape:
                    if (!flag2 && !this.TabStop)
                    {
                        this.RestoreFocusInternal();
                        flag = true;
                    }
                    break;

                case Keys.End:
                    this.SelectNextToolStripItem(null, false);
                    flag = true;
                    break;

                case Keys.Home:
                    this.SelectNextToolStripItem(null, true);
                    flag = true;
                    break;

                case Keys.Left:
                case Keys.Up:
                case Keys.Right:
                case Keys.Down:
                    flag = this.ProcessArrowKey(keyCode);
                    break;
            }
            if (flag)
            {
                return flag;
            }
            return base.ProcessDialogKey(keyData);
        }

        internal virtual void ProcessDuplicateMnemonic(ToolStripItem item, char charCode)
        {
            if (this.CanProcessMnemonic() && (item != null))
            {
                this.SetFocusUnsafe();
                item.Select();
            }
        }

        private bool ProcessLeftRightArrowKey(bool right)
        {
            this.GetSelectedItem();
            this.SelectNextToolStripItem(this.GetSelectedItem(), right);
            return true;
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessMnemonic(char charCode)
        {
            if (!this.CanProcessMnemonic())
            {
                return false;
            }
            if (this.Focused || base.ContainsFocus)
            {
                return this.ProcessMnemonicInternal(charCode);
            }
            bool inMenuMode = ToolStripManager.ModalMenuFilter.InMenuMode;
            if (!inMenuMode && (Control.ModifierKeys == Keys.Alt))
            {
                return this.ProcessMnemonicInternal(charCode);
            }
            return ((inMenuMode && (ToolStripManager.ModalMenuFilter.GetActiveToolStrip() == this)) && this.ProcessMnemonicInternal(charCode));
        }

        private bool ProcessMnemonicInternal(char charCode)
        {
            if (!this.CanProcessMnemonic())
            {
                return false;
            }
            ToolStripItem selectedItem = this.GetSelectedItem();
            int index = 0;
            if (selectedItem != null)
            {
                index = this.DisplayedItems.IndexOf(selectedItem);
            }
            index = Math.Max(0, index);
            ToolStripItem item = null;
            bool flag = false;
            int num2 = index;
            for (int i = 0; i < this.DisplayedItems.Count; i++)
            {
                ToolStripItem item3 = this.DisplayedItems[num2];
                num2 = (num2 + 1) % this.DisplayedItems.Count;
                if ((!string.IsNullOrEmpty(item3.Text) && item3.Enabled) && ((item3.DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text))
                {
                    flag = flag || (item3 is ToolStripMenuItem);
                    if (Control.IsMnemonic(charCode, item3.Text))
                    {
                        if (item == null)
                        {
                            item = item3;
                        }
                        else
                        {
                            if (item == selectedItem)
                            {
                                this.ProcessDuplicateMnemonic(item3, charCode);
                            }
                            else
                            {
                                this.ProcessDuplicateMnemonic(item, charCode);
                            }
                            return true;
                        }
                    }
                }
            }
            if (item != null)
            {
                return item.ProcessMnemonic(charCode);
            }
            if (!flag)
            {
                return false;
            }
            num2 = index;
            for (int j = 0; j < this.DisplayedItems.Count; j++)
            {
                ToolStripItem item4 = this.DisplayedItems[num2];
                num2 = (num2 + 1) % this.DisplayedItems.Count;
                if ((((item4 is ToolStripMenuItem) && !string.IsNullOrEmpty(item4.Text)) && (item4.Enabled && ((item4.DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text))) && IsPseudoMnemonic(charCode, item4.Text))
                {
                    if (item != null)
                    {
                        if (item == selectedItem)
                        {
                            this.ProcessDuplicateMnemonic(item4, charCode);
                        }
                        else
                        {
                            this.ProcessDuplicateMnemonic(item, charCode);
                        }
                        return true;
                    }
                    item = item4;
                }
            }
            return ((item != null) && item.ProcessMnemonic(charCode));
        }

        private bool ProcessTabKey(bool forward)
        {
            if (this.TabStop)
            {
                return false;
            }
            if (this.RightToLeft == RightToLeft.Yes)
            {
                forward = !forward;
            }
            this.SelectNextToolStripItem(this.GetSelectedItem(), forward);
            return true;
        }

        private void ResetGripMargin()
        {
            this.GripMargin = this.Grip.DefaultMargin;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ResetMinimumSize()
        {
            CommonProperties.SetMinimumSize(this, new Size(-1, -1));
        }

        internal virtual void ResetRenderMode()
        {
            this.RenderMode = ToolStripRenderMode.ManagerRenderMode;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void RestoreFocus()
        {
            bool flag = false;
            if ((this.hwndThatLostFocus != IntPtr.Zero) && (this.hwndThatLostFocus != base.Handle))
            {
                Control control = Control.FromHandleInternal(this.hwndThatLostFocus);
                this.hwndThatLostFocus = IntPtr.Zero;
                if ((control != null) && control.Visible)
                {
                    flag = control.FocusInternal();
                }
            }
            this.hwndThatLostFocus = IntPtr.Zero;
            if (!flag)
            {
                System.Windows.Forms.UnsafeNativeMethods.SetFocus(System.Windows.Forms.NativeMethods.NullHandleRef);
            }
        }

        internal void RestoreFocusInternal()
        {
            ToolStripManager.ModalMenuFilter.MenuKeyToggle = false;
            this.ClearAllSelections();
            this.lastMouseDownedItem = null;
            ToolStripManager.ModalMenuFilter.ExitMenuMode();
            if (!this.IsDropDown)
            {
                Application.ThreadContext.FromCurrent().RemoveMessageFilter(this.RestoreFocusFilter);
                this.MenuAutoExpand = false;
                if ((!base.DesignMode && !this.TabStop) && (this.Focused || base.ContainsFocus))
                {
                    this.RestoreFocus();
                }
            }
            if ((this.KeyboardActive && !this.Focused) && !base.ContainsFocus)
            {
                this.KeyboardActive = false;
            }
        }

        private void RestoreFocusInternal(bool wasInMenuMode)
        {
            if (wasInMenuMode == ToolStripManager.ModalMenuFilter.InMenuMode)
            {
                this.RestoreFocusInternal();
            }
        }

        internal void ResumeCaputureMode()
        {
            this.SetToolStripState(0x2000, false);
        }

        internal virtual void ScrollInternal(int delta)
        {
            base.SuspendLayout();
            foreach (ToolStripItem item in this.Items)
            {
                Point location = item.Bounds.Location;
                location.Y -= delta;
                this.SetItemLocation(item, location);
            }
            base.ResumeLayout(false);
            base.Invalidate();
        }

        protected override void Select(bool directed, bool forward)
        {
            bool flag = true;
            if (this.ParentInternal != null)
            {
                IContainerControl containerControlInternal = this.ParentInternal.GetContainerControlInternal();
                if (containerControlInternal != null)
                {
                    containerControlInternal.ActiveControl = this;
                    flag = containerControlInternal.ActiveControl == this;
                }
            }
            if (directed && flag)
            {
                this.SelectNextToolStripItem(null, forward);
            }
        }

        internal ToolStripItem SelectNextToolStripItem(ToolStripItem start, bool forward)
        {
            ToolStripItem nextItem = this.GetNextItem(start, forward ? ArrowDirection.Right : ArrowDirection.Left, true);
            this.ChangeSelection(nextItem);
            return nextItem;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetAutoScrollMargin(int x, int y)
        {
            base.SetAutoScrollMargin(x, y);
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            Point location = base.Location;
            if ((!this.IsCurrentlyDragging && !this.IsLocationChanging) && this.IsInToolStripPanel)
            {
                ToolStripLocationCancelEventArgs e = new ToolStripLocationCancelEventArgs(new Point(x, y), false);
                try
                {
                    if ((location.X != x) || (location.Y != y))
                    {
                        this.SetToolStripState(0x400, true);
                        this.OnLocationChanging(e);
                    }
                    if (!e.Cancel)
                    {
                        base.SetBoundsCore(x, y, width, height, specified);
                    }
                }
                finally
                {
                    this.SetToolStripState(0x400, false);
                }
            }
            else
            {
                if (this.IsCurrentlyDragging)
                {
                    Region transparentRegion = this.Renderer.GetTransparentRegion(this);
                    if ((transparentRegion != null) && ((location.X != x) || (location.Y != y)))
                    {
                        try
                        {
                            base.Invalidate(transparentRegion);
                            base.Update();
                        }
                        finally
                        {
                            transparentRegion.Dispose();
                        }
                    }
                }
                this.SetToolStripState(0x400, false);
                base.SetBoundsCore(x, y, width, height, specified);
            }
        }

        protected virtual void SetDisplayedItems()
        {
            this.DisplayedItems.Clear();
            this.OverflowItems.Clear();
            this.HasVisibleItems = false;
            Size empty = Size.Empty;
            if (this.LayoutEngine is ToolStripSplitStackLayout)
            {
                if (ToolStripGripStyle.Visible == this.GripStyle)
                {
                    this.DisplayedItems.Add(this.Grip);
                    this.SetupGrip();
                }
                Rectangle displayRectangle = this.DisplayRectangle;
                int num = -1;
                for (int i = 0; i < 2; i++)
                {
                    int num3 = 0;
                    if (i == 1)
                    {
                        num3 = num;
                    }
                    while ((num3 >= 0) && (num3 < this.Items.Count))
                    {
                        ToolStripItem item = this.Items[num3];
                        ToolStripItemPlacement placement = item.Placement;
                        if (((IArrangedElement) item).ParticipatesInLayout)
                        {
                            if (placement == ToolStripItemPlacement.Main)
                            {
                                bool flag = false;
                                switch (i)
                                {
                                    case 0:
                                        flag = item.Alignment == ToolStripItemAlignment.Left;
                                        if (!flag)
                                        {
                                            num = num3;
                                        }
                                        break;

                                    case 1:
                                        flag = item.Alignment == ToolStripItemAlignment.Right;
                                        break;
                                }
                                if (flag)
                                {
                                    this.HasVisibleItems = true;
                                    empty = LayoutUtils.UnionSizes(empty, item.Bounds.Size);
                                    this.DisplayedItems.Add(item);
                                }
                            }
                            else if ((placement == ToolStripItemPlacement.Overflow) && !(item is ToolStripSeparator))
                            {
                                if ((item is ToolStripControlHost) && this.OverflowButton.DropDown.IsRestrictedWindow)
                                {
                                    item.SetPlacement(ToolStripItemPlacement.None);
                                }
                                else
                                {
                                    this.OverflowItems.Add(item);
                                }
                            }
                        }
                        else
                        {
                            item.SetPlacement(ToolStripItemPlacement.None);
                        }
                        num3 = (i == 0) ? (num3 + 1) : (num3 - 1);
                    }
                }
                ToolStripOverflow overflow = this.GetOverflow();
                if (overflow != null)
                {
                    overflow.LayoutRequired = true;
                }
                if (this.OverflowItems.Count == 0)
                {
                    this.OverflowButton.Visible = false;
                }
                else if (this.CanOverflow)
                {
                    this.DisplayedItems.Add(this.OverflowButton);
                }
            }
            else
            {
                Rectangle clientRectangle = base.ClientRectangle;
                bool flag2 = true;
                for (int j = 0; j < this.Items.Count; j++)
                {
                    ToolStripItem item2 = this.Items[j];
                    if (((IArrangedElement) item2).ParticipatesInLayout)
                    {
                        item2.ParentInternal = this;
                        bool flag3 = !this.IsDropDown;
                        bool flag4 = item2.Bounds.IntersectsWith(clientRectangle);
                        if (!(clientRectangle.Contains(clientRectangle.X, item2.Bounds.Top) && clientRectangle.Contains(clientRectangle.X, item2.Bounds.Bottom)))
                        {
                            flag2 = false;
                        }
                        if (!flag3 || flag4)
                        {
                            this.HasVisibleItems = true;
                            empty = LayoutUtils.UnionSizes(empty, item2.Bounds.Size);
                            this.DisplayedItems.Add(item2);
                            item2.SetPlacement(ToolStripItemPlacement.Main);
                        }
                    }
                    else
                    {
                        item2.SetPlacement(ToolStripItemPlacement.None);
                    }
                }
                this.AllItemsVisible = flag2;
            }
            this.SetLargestItemSize(empty);
        }

        internal void SetFocusUnsafe()
        {
            if (this.TabStop)
            {
                this.FocusInternal();
            }
            else
            {
                ToolStripManager.ModalMenuFilter.SetActiveToolStrip(this, false);
            }
        }

        protected internal void SetItemLocation(ToolStripItem item, Point location)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (item.Owner != this)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripCanOnlyPositionItsOwnItems"));
            }
            item.SetBounds(new Rectangle(location, item.Size));
        }

        protected static void SetItemParent(ToolStripItem item, ToolStrip parent)
        {
            item.Parent = parent;
        }

        internal void SetLargestItemSize(Size size)
        {
            if ((this.toolStripOverflowButton != null) && this.toolStripOverflowButton.Visible)
            {
                size = LayoutUtils.UnionSizes(size, this.toolStripOverflowButton.Bounds.Size);
            }
            if ((this.toolStripGrip != null) && this.toolStripGrip.Visible)
            {
                size = LayoutUtils.UnionSizes(size, this.toolStripGrip.Bounds.Size);
            }
            this.largestDisplayedItemSize = size;
        }

        internal void SetToolStripState(int flag, bool value)
        {
            this.toolStripState = value ? (this.toolStripState | flag) : (this.toolStripState & ~flag);
        }

        private void SetupGrip()
        {
            Rectangle empty = Rectangle.Empty;
            Rectangle displayRectangle = this.DisplayRectangle;
            if (this.Orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                empty.X = Math.Max(0, displayRectangle.X - this.Grip.GripThickness);
                empty.Y = Math.Max(0, displayRectangle.Top - this.Grip.Margin.Top);
                empty.Width = this.Grip.GripThickness;
                empty.Height = displayRectangle.Height;
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    empty.X = (base.ClientRectangle.Right - empty.Width) - this.Grip.Margin.Horizontal;
                    empty.X += this.Grip.Margin.Left;
                }
                else
                {
                    empty.X -= this.Grip.Margin.Right;
                }
            }
            else
            {
                empty.X = displayRectangle.Left;
                empty.Y = displayRectangle.Top - (this.Grip.GripThickness + this.Grip.Margin.Bottom);
                empty.Width = displayRectangle.Width;
                empty.Height = this.Grip.GripThickness;
            }
            if (this.Grip.Bounds != empty)
            {
                this.Grip.SetBounds(empty);
            }
        }

        protected override void SetVisibleCore(bool visible)
        {
            if (visible)
            {
                this.SnapMouseLocation();
            }
            else
            {
                if (!base.Disposing && !base.IsDisposed)
                {
                    this.ClearAllSelections();
                }
                CachedItemHdcInfo cachedItemHdcInfo = this.cachedItemHdcInfo;
                this.cachedItemHdcInfo = null;
                this.lastMouseDownedItem = null;
                if (cachedItemHdcInfo != null)
                {
                    cachedItemHdcInfo.Dispose();
                }
            }
            base.SetVisibleCore(visible);
        }

        internal bool ShouldSelectItem()
        {
            if (this.mouseEnterWhenShown == InvalidMouseEnter)
            {
                return true;
            }
            Point lastCursorPoint = WindowsFormsUtils.LastCursorPoint;
            if (this.mouseEnterWhenShown != lastCursorPoint)
            {
                this.mouseEnterWhenShown = InvalidMouseEnter;
                return true;
            }
            return false;
        }

        private bool ShouldSerializeDefaultDropDownDirection()
        {
            return (this.toolStripDropDownDirection != ToolStripDropDownDirection.Default);
        }

        private bool ShouldSerializeGripMargin()
        {
            return (this.GripMargin != this.DefaultGripMargin);
        }

        internal virtual bool ShouldSerializeLayoutStyle()
        {
            return (this.layoutStyle != ToolStripLayoutStyle.StackWithOverflow);
        }

        internal override bool ShouldSerializeMinimumSize()
        {
            Size defaultMinimumSize = new Size(-1, -1);
            return (CommonProperties.GetMinimumSize(this, defaultMinimumSize) != defaultMinimumSize);
        }

        internal virtual bool ShouldSerializeRenderMode()
        {
            return ((this.RenderMode != ToolStripRenderMode.ManagerRenderMode) && (this.RenderMode != ToolStripRenderMode.Custom));
        }

        private void SnapFocus(IntPtr otherHwnd)
        {
            if (!this.TabStop && !this.IsDropDown)
            {
                bool flag = false;
                if (this.Focused && (otherHwnd != base.Handle))
                {
                    flag = true;
                }
                else if (!base.ContainsFocus && !this.Focused)
                {
                    flag = true;
                }
                if (flag)
                {
                    this.SnapMouseLocation();
                    HandleRef hWndParent = new HandleRef(this, base.Handle);
                    HandleRef hwnd = new HandleRef(null, otherHwnd);
                    if ((hWndParent.Handle != hwnd.Handle) && !System.Windows.Forms.UnsafeNativeMethods.IsChild(hWndParent, hwnd))
                    {
                        HandleRef rootHWnd = WindowsFormsUtils.GetRootHWnd(this);
                        HandleRef ref5 = WindowsFormsUtils.GetRootHWnd(hwnd);
                        if ((rootHWnd.Handle == ref5.Handle) && (rootHWnd.Handle != IntPtr.Zero))
                        {
                            this.hwndThatLostFocus = hwnd.Handle;
                        }
                    }
                }
            }
        }

        internal void SnapFocusChange(ToolStrip otherToolStrip)
        {
            otherToolStrip.hwndThatLostFocus = this.hwndThatLostFocus;
        }

        internal void SnapMouseLocation()
        {
            this.mouseEnterWhenShown = WindowsFormsUtils.LastCursorPoint;
        }

        internal void SuspendCaputureMode()
        {
            this.SetToolStripState(0x2000, true);
        }

        void ISupportToolStripPanel.BeginDrag()
        {
            this.OnBeginDrag(EventArgs.Empty);
        }

        void ISupportToolStripPanel.EndDrag()
        {
            ToolStripPanel.ClearDragFeedback();
            this.OnEndDrag(EventArgs.Empty);
        }

        void IArrangedElement.SetBounds(Rectangle bounds, BoundsSpecified specified)
        {
            this.SetBoundsCore(bounds.X, bounds.Y, bounds.Width, bounds.Height, specified);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(base.ToString());
            builder.Append(", Name: ");
            builder.Append(base.Name);
            builder.Append(", Items: ").Append(this.Items.Count);
            return builder.ToString();
        }

        private void UpdateLayoutStyle(DockStyle newDock)
        {
            if ((!this.IsInToolStripPanel && (this.layoutStyle != ToolStripLayoutStyle.HorizontalStackWithOverflow)) && (this.layoutStyle != ToolStripLayoutStyle.VerticalStackWithOverflow))
            {
                using (new LayoutTransaction(this, this, PropertyNames.Orientation))
                {
                    if ((newDock == DockStyle.Left) || (newDock == DockStyle.Right))
                    {
                        this.UpdateOrientation(System.Windows.Forms.Orientation.Vertical);
                    }
                    else
                    {
                        this.UpdateOrientation(System.Windows.Forms.Orientation.Horizontal);
                    }
                }
                this.OnLayoutStyleChanged(EventArgs.Empty);
                if (this.ParentInternal != null)
                {
                    LayoutTransaction.DoLayout(this.ParentInternal, this, PropertyNames.Orientation);
                }
            }
        }

        private void UpdateLayoutStyle(System.Windows.Forms.Orientation newRaftingRowOrientation)
        {
            if ((this.layoutStyle != ToolStripLayoutStyle.HorizontalStackWithOverflow) && (this.layoutStyle != ToolStripLayoutStyle.VerticalStackWithOverflow))
            {
                using (new LayoutTransaction(this, this, PropertyNames.Orientation))
                {
                    this.UpdateOrientation(newRaftingRowOrientation);
                    if ((this.LayoutEngine is ToolStripSplitStackLayout) && (this.layoutStyle == ToolStripLayoutStyle.StackWithOverflow))
                    {
                        this.OnLayoutStyleChanged(EventArgs.Empty);
                    }
                    return;
                }
            }
            this.UpdateOrientation(newRaftingRowOrientation);
        }

        private void UpdateOrientation(System.Windows.Forms.Orientation newOrientation)
        {
            if (newOrientation != this.orientation)
            {
                Size size = CommonProperties.GetSpecifiedBounds(this).Size;
                this.orientation = newOrientation;
                this.SetupGrip();
            }
        }

        internal void UpdateToolTip(ToolStripItem item)
        {
            if ((this.ShowItemToolTips && (item != this.currentlyActiveTooltipItem)) && (this.ToolTip != null))
            {
                System.Windows.Forms.IntSecurity.AllWindows.Assert();
                try
                {
                    this.ToolTip.Hide(this);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                this.ToolTip.Active = false;
                this.currentlyActiveTooltipItem = item;
                if ((this.currentlyActiveTooltipItem != null) && !this.GetToolStripState(0x800))
                {
                    System.Windows.Forms.Cursor currentInternal = System.Windows.Forms.Cursor.CurrentInternal;
                    if (currentInternal != null)
                    {
                        this.ToolTip.Active = true;
                        Point position = System.Windows.Forms.Cursor.Position;
                        position.Y += this.Cursor.Size.Height - currentInternal.HotSpot.Y;
                        position = WindowsFormsUtils.ConstrainToScreenBounds(new Rectangle(position, onePixel)).Location;
                        System.Windows.Forms.IntSecurity.AllWindows.Assert();
                        try
                        {
                            this.ToolTip.Show(this.currentlyActiveTooltipItem.ToolTipText, this, base.PointToClient(position), this.ToolTip.AutoPopDelay);
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 7)
            {
                this.SnapFocus(m.WParam);
            }
            if (m.Msg == 0x21)
            {
                Point point = base.PointToClient(WindowsFormsUtils.LastCursorPoint);
                if (System.Windows.Forms.UnsafeNativeMethods.ChildWindowFromPointEx(new HandleRef(null, base.Handle), point.X, point.Y, 7) == base.Handle)
                {
                    this.lastMouseDownedItem = null;
                    m.Result = (IntPtr) 3;
                    if (!this.IsDropDown && !this.IsInDesignMode)
                    {
                        HandleRef rootHWnd = WindowsFormsUtils.GetRootHWnd(this);
                        if ((rootHWnd.Handle != IntPtr.Zero) && (System.Windows.Forms.UnsafeNativeMethods.GetActiveWindow() != rootHWnd.Handle))
                        {
                            m.Result = (IntPtr) 2;
                        }
                    }
                    return;
                }
                this.SnapFocus(System.Windows.Forms.UnsafeNativeMethods.GetFocus());
                if (!this.IsDropDown && !this.TabStop)
                {
                    Application.ThreadContext.FromCurrent().AddMessageFilter(this.RestoreFocusFilter);
                }
            }
            base.WndProc(ref m);
            if ((m.Msg == 130) && (this.dropDownOwnerWindow != null))
            {
                this.dropDownOwnerWindow.DestroyHandle();
            }
        }

        internal ArrayList ActiveDropDowns
        {
            get
            {
                return this.activeDropDowns;
            }
        }

        internal virtual bool AllItemsVisible
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        public override bool AllowDrop
        {
            get
            {
                return base.AllowDrop;
            }
            set
            {
                if (value && this.AllowItemReorder)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ToolStripAllowItemReorderAndAllowDropCannotBeSetToTrue"));
                }
                base.AllowDrop = value;
                if (value)
                {
                    this.DropTargetManager.EnsureRegistered(this);
                }
                else
                {
                    this.DropTargetManager.EnsureUnRegistered(this);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false), System.Windows.Forms.SRDescription("ToolStripAllowItemReorderDescr")]
        public bool AllowItemReorder
        {
            get
            {
                return this.GetToolStripState(2);
            }
            set
            {
                if (this.GetToolStripState(2) != value)
                {
                    if (this.AllowDrop && value)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("ToolStripAllowItemReorderAndAllowDropCannotBeSetToTrue"));
                    }
                    this.SetToolStripState(2, value);
                    if (value)
                    {
                        ToolStripSplitStackDragDropHandler handler = new ToolStripSplitStackDragDropHandler(this);
                        this.ItemReorderDropSource = handler;
                        this.ItemReorderDropTarget = handler;
                        this.DropTargetManager.EnsureRegistered(this);
                    }
                    else
                    {
                        this.DropTargetManager.EnsureUnRegistered(this);
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), System.Windows.Forms.SRDescription("ToolStripAllowMergeDescr")]
        public bool AllowMerge
        {
            get
            {
                return this.GetToolStripState(0x80);
            }
            set
            {
                if (this.GetToolStripState(0x80) != value)
                {
                    this.SetToolStripState(0x80, value);
                }
            }
        }

        public override AnchorStyles Anchor
        {
            get
            {
                return base.Anchor;
            }
            set
            {
                using (new LayoutTransaction(this, this, PropertyNames.Anchor))
                {
                    base.Anchor = value;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool AutoScroll
        {
            get
            {
                return base.AutoScroll;
            }
            set
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripDoesntSupportAutoScroll"));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Size AutoScrollMargin
        {
            get
            {
                return base.AutoScrollMargin;
            }
            set
            {
                base.AutoScrollMargin = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public Size AutoScrollMinSize
        {
            get
            {
                return base.AutoScrollMinSize;
            }
            set
            {
                base.AutoScrollMinSize = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public Point AutoScrollPosition
        {
            get
            {
                return base.AutoScrollPosition;
            }
            set
            {
                base.AutoScrollPosition = value;
            }
        }

        [DefaultValue(true), EditorBrowsable(EditorBrowsableState.Always), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Browsable(true)]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                if ((this.IsInToolStripPanel && base.AutoSize) && !value)
                {
                    Rectangle specifiedBounds = CommonProperties.GetSpecifiedBounds(this);
                    specifiedBounds.Location = base.Location;
                    CommonProperties.UpdateSpecifiedBounds(this, specifiedBounds.X, specifiedBounds.Y, specifiedBounds.Width, specifiedBounds.Height, BoundsSpecified.Location);
                }
                base.AutoSize = value;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripBackColorDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public System.Drawing.Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        public override System.Windows.Forms.BindingContext BindingContext
        {
            get
            {
                System.Windows.Forms.BindingContext context = (System.Windows.Forms.BindingContext) base.Properties.GetObject(PropBindingContext);
                if (context != null)
                {
                    return context;
                }
                Control parentInternal = this.ParentInternal;
                if ((parentInternal != null) && parentInternal.CanAccessProperties)
                {
                    return parentInternal.BindingContext;
                }
                return null;
            }
            set
            {
                if (base.Properties.GetObject(PropBindingContext) != value)
                {
                    base.Properties.SetObject(PropBindingContext, value);
                    this.OnBindingContextChanged(EventArgs.Empty);
                }
            }
        }

        internal bool CanHotTrack
        {
            get
            {
                if (!this.Focused)
                {
                    return !base.ContainsFocus;
                }
                return true;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripCanOverflowDescr"), DefaultValue(true), System.Windows.Forms.SRCategory("CatLayout")]
        public bool CanOverflow
        {
            get
            {
                return this.GetToolStripState(1);
            }
            set
            {
                if (this.GetToolStripState(1) != value)
                {
                    this.SetToolStripState(1, value);
                    this.InvalidateLayout();
                }
            }
        }

        [DefaultValue(false), Browsable(false)]
        public bool CausesValidation
        {
            get
            {
                return base.CausesValidation;
            }
            set
            {
                base.CausesValidation = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control.ControlCollection Controls
        {
            get
            {
                return base.Controls;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
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

        protected virtual DockStyle DefaultDock
        {
            get
            {
                return DockStyle.Top;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripDefaultDropDownDirectionDescr"), System.Windows.Forms.SRCategory("CatBehavior"), Browsable(false)]
        public virtual ToolStripDropDownDirection DefaultDropDownDirection
        {
            get
            {
                ToolStripDropDownDirection toolStripDropDownDirection = this.toolStripDropDownDirection;
                if (toolStripDropDownDirection != ToolStripDropDownDirection.Default)
                {
                    return toolStripDropDownDirection;
                }
                if (this.Orientation == System.Windows.Forms.Orientation.Vertical)
                {
                    if (this.IsInToolStripPanel)
                    {
                        DockStyle style = (this.ParentInternal != null) ? this.ParentInternal.Dock : DockStyle.Left;
                        toolStripDropDownDirection = (style == DockStyle.Right) ? ToolStripDropDownDirection.Left : ToolStripDropDownDirection.Right;
                        if (base.DesignMode && (style == DockStyle.Left))
                        {
                            toolStripDropDownDirection = ToolStripDropDownDirection.Right;
                        }
                        return toolStripDropDownDirection;
                    }
                    toolStripDropDownDirection = ((this.Dock == DockStyle.Right) && (this.RightToLeft == RightToLeft.No)) ? ToolStripDropDownDirection.Left : ToolStripDropDownDirection.Right;
                    if (base.DesignMode && (this.Dock == DockStyle.Left))
                    {
                        toolStripDropDownDirection = ToolStripDropDownDirection.Right;
                    }
                    return toolStripDropDownDirection;
                }
                DockStyle dock = this.Dock;
                if (this.IsInToolStripPanel && (this.ParentInternal != null))
                {
                    dock = this.ParentInternal.Dock;
                }
                if (dock == DockStyle.Bottom)
                {
                    return ((this.RightToLeft == RightToLeft.Yes) ? ToolStripDropDownDirection.AboveLeft : ToolStripDropDownDirection.AboveRight);
                }
                return ((this.RightToLeft == RightToLeft.Yes) ? ToolStripDropDownDirection.BelowLeft : ToolStripDropDownDirection.BelowRight);
            }
            set
            {
                switch (value)
                {
                    case ToolStripDropDownDirection.AboveLeft:
                    case ToolStripDropDownDirection.AboveRight:
                    case ToolStripDropDownDirection.BelowLeft:
                    case ToolStripDropDownDirection.BelowRight:
                    case ToolStripDropDownDirection.Left:
                    case ToolStripDropDownDirection.Right:
                    case ToolStripDropDownDirection.Default:
                        this.toolStripDropDownDirection = value;
                        return;
                }
                throw new InvalidEnumArgumentException("value", (int) value, typeof(ToolStripDropDownDirection));
            }
        }

        protected virtual Padding DefaultGripMargin
        {
            get
            {
                if (this.toolStripGrip != null)
                {
                    return this.toolStripGrip.DefaultMargin;
                }
                return new Padding(2);
            }
        }

        protected override Padding DefaultMargin
        {
            get
            {
                return Padding.Empty;
            }
        }

        protected override Padding DefaultPadding
        {
            get
            {
                return new Padding(0, 0, 1, 0);
            }
        }

        protected virtual bool DefaultShowItemToolTips
        {
            get
            {
                return true;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(100, 0x19);
            }
        }

        protected internal virtual ToolStripItemCollection DisplayedItems
        {
            get
            {
                if (this.displayedItems == null)
                {
                    this.displayedItems = new ToolStripItemCollection(this, false);
                }
                return this.displayedItems;
            }
        }

        public override Rectangle DisplayRectangle
        {
            get
            {
                Rectangle displayRectangle = base.DisplayRectangle;
                if ((this.LayoutEngine is ToolStripSplitStackLayout) && (this.GripStyle == ToolStripGripStyle.Visible))
                {
                    if (this.Orientation == System.Windows.Forms.Orientation.Horizontal)
                    {
                        int num = this.Grip.GripThickness + this.Grip.Margin.Horizontal;
                        displayRectangle.Width -= num;
                        displayRectangle.X += (this.RightToLeft == RightToLeft.No) ? num : 0;
                        return displayRectangle;
                    }
                    int num2 = this.Grip.GripThickness + this.Grip.Margin.Vertical;
                    displayRectangle.Y += num2;
                    displayRectangle.Height -= num2;
                }
                return displayRectangle;
            }
        }

        [DefaultValue(1)]
        public override DockStyle Dock
        {
            get
            {
                return base.Dock;
            }
            set
            {
                if (value != this.Dock)
                {
                    using (new LayoutTransaction(this, this, PropertyNames.Dock))
                    {
                        using (new LayoutTransaction(this.ParentInternal, this, PropertyNames.Dock))
                        {
                            DefaultLayout.SetDock(this, value);
                            this.UpdateLayoutStyle(this.Dock);
                        }
                    }
                    this.OnDockChanged(EventArgs.Empty);
                }
            }
        }

        internal virtual NativeWindow DropDownOwnerWindow
        {
            get
            {
                if (this.dropDownOwnerWindow == null)
                {
                    this.dropDownOwnerWindow = new NativeWindow();
                }
                if (this.dropDownOwnerWindow.Handle == IntPtr.Zero)
                {
                    CreateParams cp = new CreateParams {
                        ExStyle = 0x80
                    };
                    this.dropDownOwnerWindow.CreateHandle(cp);
                }
                return this.dropDownOwnerWindow;
            }
        }

        internal ToolStripDropTargetManager DropTargetManager
        {
            get
            {
                if (this.dropTargetManager == null)
                {
                    this.dropTargetManager = new ToolStripDropTargetManager(this);
                }
                return this.dropTargetManager;
            }
            set
            {
                this.dropTargetManager = value;
            }
        }

        public override System.Drawing.Font Font
        {
            get
            {
                if (base.IsFontSet())
                {
                    return base.Font;
                }
                if (this.defaultFont == null)
                {
                    this.defaultFont = ToolStripManager.DefaultFont;
                }
                return this.defaultFont;
            }
            set
            {
                base.Font = value;
            }
        }

        [Browsable(false)]
        public System.Drawing.Color ForeColor
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

        internal ToolStripGrip Grip
        {
            get
            {
                if (this.toolStripGrip == null)
                {
                    this.toolStripGrip = new ToolStripGrip();
                    this.toolStripGrip.Overflow = ToolStripItemOverflow.Never;
                    this.toolStripGrip.Visible = this.toolStripGripStyle == ToolStripGripStyle.Visible;
                    this.toolStripGrip.AutoSize = false;
                    this.toolStripGrip.ParentInternal = this;
                    this.toolStripGrip.Margin = this.DefaultGripMargin;
                }
                return this.toolStripGrip;
            }
        }

        [Browsable(false)]
        public ToolStripGripDisplayStyle GripDisplayStyle
        {
            get
            {
                if (this.LayoutStyle != ToolStripLayoutStyle.HorizontalStackWithOverflow)
                {
                    return ToolStripGripDisplayStyle.Horizontal;
                }
                return ToolStripGripDisplayStyle.Vertical;
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ToolStripGripDisplayStyleDescr")]
        public Padding GripMargin
        {
            get
            {
                return this.Grip.Margin;
            }
            set
            {
                this.Grip.Margin = value;
            }
        }

        [Browsable(false)]
        public Rectangle GripRectangle
        {
            get
            {
                if (this.GripStyle != ToolStripGripStyle.Visible)
                {
                    return Rectangle.Empty;
                }
                return this.Grip.Bounds;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(1), System.Windows.Forms.SRDescription("ToolStripGripStyleDescr")]
        public ToolStripGripStyle GripStyle
        {
            get
            {
                return this.toolStripGripStyle;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ToolStripGripStyle));
                }
                if (this.toolStripGripStyle != value)
                {
                    this.toolStripGripStyle = value;
                    this.Grip.Visible = this.toolStripGripStyle == ToolStripGripStyle.Visible;
                    LayoutTransaction.DoLayout(this, this, PropertyNames.GripStyle);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool HasChildren
        {
            get
            {
                return base.HasChildren;
            }
        }

        private bool HasKeyboardInput
        {
            get
            {
                return (base.ContainsFocus || (ToolStripManager.ModalMenuFilter.InMenuMode && (ToolStripManager.ModalMenuFilter.GetActiveToolStrip() == this)));
            }
        }

        internal bool HasVisibleItems
        {
            get
            {
                if (base.IsHandleCreated)
                {
                    return this.GetToolStripState(0x1000);
                }
                foreach (ToolStripItem item in this.Items)
                {
                    if (((IArrangedElement) item).ParticipatesInLayout)
                    {
                        this.SetToolStripState(0x1000, true);
                        return true;
                    }
                }
                this.SetToolStripState(0x1000, false);
                return false;
            }
            set
            {
                this.SetToolStripState(0x1000, value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public HScrollProperties HorizontalScroll
        {
            get
            {
                return base.HorizontalScroll;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripImageListDescr"), DefaultValue((string) null), Browsable(false)]
        public System.Windows.Forms.ImageList ImageList
        {
            get
            {
                return this.imageList;
            }
            set
            {
                if (this.imageList != value)
                {
                    EventHandler handler = new EventHandler(this.ImageListRecreateHandle);
                    if (this.imageList != null)
                    {
                        this.imageList.RecreateHandle -= handler;
                    }
                    this.imageList = value;
                    if (value != null)
                    {
                        value.RecreateHandle += handler;
                    }
                    foreach (ToolStripItem item in this.Items)
                    {
                        item.InvalidateImageListImage();
                    }
                    base.Invalidate();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(typeof(Size), "16,16"), System.Windows.Forms.SRDescription("ToolStripImageScalingSizeDescr")]
        public Size ImageScalingSize
        {
            get
            {
                return this.ImageScalingSizeInternal;
            }
            set
            {
                this.ImageScalingSizeInternal = value;
            }
        }

        internal virtual Size ImageScalingSizeInternal
        {
            get
            {
                return this.imageScalingSize;
            }
            set
            {
                if (this.imageScalingSize != value)
                {
                    this.imageScalingSize = value;
                    LayoutTransaction.DoLayoutIf(this.Items.Count > 0, this, this, PropertyNames.ImageScalingSize);
                    foreach (ToolStripItem item in this.Items)
                    {
                        item.OnImageScalingSizeChanged(EventArgs.Empty);
                    }
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool IsCurrentlyDragging
        {
            get
            {
                return this.GetToolStripState(0x800);
            }
        }

        internal bool IsDisposingItems
        {
            get
            {
                return this.GetToolStripState(4);
            }
        }

        [Browsable(false)]
        public bool IsDropDown
        {
            get
            {
                return (this is ToolStripDropDown);
            }
        }

        internal bool IsInDesignMode
        {
            get
            {
                return base.DesignMode;
            }
        }

        internal bool IsInToolStripPanel
        {
            get
            {
                return (this.ToolStripPanelRow != null);
            }
        }

        private bool IsLocationChanging
        {
            get
            {
                return this.GetToolStripState(0x400);
            }
        }

        internal override bool IsMnemonicsListenerAxSourced
        {
            get
            {
                return true;
            }
        }

        internal bool IsSelectionSuspended
        {
            get
            {
                return this.GetToolStripState(0x4000);
            }
        }

        private CachedItemHdcInfo ItemHdcInfo
        {
            get
            {
                if (this.cachedItemHdcInfo == null)
                {
                    this.cachedItemHdcInfo = new CachedItemHdcInfo();
                }
                return this.cachedItemHdcInfo;
            }
        }

        internal ISupportOleDropSource ItemReorderDropSource
        {
            get
            {
                return this.itemReorderDropSource;
            }
            set
            {
                this.itemReorderDropSource = value;
            }
        }

        internal IDropTarget ItemReorderDropTarget
        {
            get
            {
                return this.itemReorderDropTarget;
            }
            set
            {
                this.itemReorderDropTarget = value;
            }
        }

        [MergableProperty(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), System.Windows.Forms.SRDescription("ToolStripItemsDescr"), System.Windows.Forms.SRCategory("CatData")]
        public virtual ToolStripItemCollection Items
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (this.toolStripItemCollection == null)
                {
                    this.toolStripItemCollection = new ToolStripItemCollection(this, true);
                }
                return this.toolStripItemCollection;
            }
        }

        internal virtual bool KeyboardActive
        {
            get
            {
                return this.GetToolStripState(0x8000);
            }
            set
            {
                this.SetToolStripState(0x8000, value);
            }
        }

        internal ToolStripItem LastMouseDownedItem
        {
            get
            {
                if ((this.lastMouseDownedItem != null) && (this.lastMouseDownedItem.IsDisposed || (this.lastMouseDownedItem.ParentInternal != this)))
                {
                    this.lastMouseDownedItem = null;
                }
                return this.lastMouseDownedItem;
            }
        }

        public override System.Windows.Forms.Layout.LayoutEngine LayoutEngine
        {
            get
            {
                return this.layoutEngine;
            }
        }

        internal bool LayoutRequired
        {
            get
            {
                return this.layoutRequired;
            }
            set
            {
                this.layoutRequired = value;
            }
        }

        [DefaultValue((string) null), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Windows.Forms.LayoutSettings LayoutSettings
        {
            get
            {
                return this.layoutSettings;
            }
            set
            {
                this.layoutSettings = value;
            }
        }

        [AmbientValue(0), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ToolStripLayoutStyle")]
        public ToolStripLayoutStyle LayoutStyle
        {
            get
            {
                if (this.layoutStyle == ToolStripLayoutStyle.StackWithOverflow)
                {
                    switch (this.Orientation)
                    {
                        case System.Windows.Forms.Orientation.Horizontal:
                            return ToolStripLayoutStyle.HorizontalStackWithOverflow;

                        case System.Windows.Forms.Orientation.Vertical:
                            return ToolStripLayoutStyle.VerticalStackWithOverflow;
                    }
                }
                return this.layoutStyle;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 4))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ToolStripLayoutStyle));
                }
                if (this.layoutStyle != value)
                {
                    this.layoutStyle = value;
                    switch (value)
                    {
                        case ToolStripLayoutStyle.Flow:
                            if (!(this.layoutEngine is FlowLayout))
                            {
                                this.layoutEngine = FlowLayout.Instance;
                            }
                            this.UpdateOrientation(System.Windows.Forms.Orientation.Horizontal);
                            break;

                        case ToolStripLayoutStyle.Table:
                            if (!(this.layoutEngine is TableLayout))
                            {
                                this.layoutEngine = TableLayout.Instance;
                            }
                            this.UpdateOrientation(System.Windows.Forms.Orientation.Horizontal);
                            break;

                        default:
                            if (value != ToolStripLayoutStyle.StackWithOverflow)
                            {
                                this.UpdateOrientation((value == ToolStripLayoutStyle.VerticalStackWithOverflow) ? System.Windows.Forms.Orientation.Vertical : System.Windows.Forms.Orientation.Horizontal);
                            }
                            else if (this.IsInToolStripPanel)
                            {
                                this.UpdateLayoutStyle(this.ToolStripPanelRow.Orientation);
                            }
                            else
                            {
                                this.UpdateLayoutStyle(this.Dock);
                            }
                            if (!(this.layoutEngine is ToolStripSplitStackLayout))
                            {
                                this.layoutEngine = new ToolStripSplitStackLayout(this);
                            }
                            break;
                    }
                    using (LayoutTransaction.CreateTransactionIf(base.IsHandleCreated, this, this, PropertyNames.LayoutStyle))
                    {
                        this.LayoutSettings = this.CreateLayoutSettings(this.layoutStyle);
                    }
                    this.OnLayoutStyleChanged(EventArgs.Empty);
                }
            }
        }

        protected internal virtual Size MaxItemSize
        {
            get
            {
                return this.DisplayRectangle.Size;
            }
        }

        internal bool MenuAutoExpand
        {
            get
            {
                if (base.DesignMode || !this.GetToolStripState(8))
                {
                    return false;
                }
                if (!this.IsDropDown && !ToolStripManager.ModalMenuFilter.InMenuMode)
                {
                    this.SetToolStripState(8, false);
                    return false;
                }
                return true;
            }
            set
            {
                if (!base.DesignMode)
                {
                    this.SetToolStripState(8, value);
                }
            }
        }

        internal Stack<MergeHistory> MergeHistoryStack
        {
            get
            {
                if (this.mergeHistoryStack == null)
                {
                    this.mergeHistoryStack = new Stack<MergeHistory>();
                }
                return this.mergeHistoryStack;
            }
        }

        private System.Windows.Forms.MouseHoverTimer MouseHoverTimer
        {
            get
            {
                if (this.mouseHoverTimer == null)
                {
                    this.mouseHoverTimer = new System.Windows.Forms.MouseHoverTimer();
                }
                return this.mouseHoverTimer;
            }
        }

        [Browsable(false)]
        public System.Windows.Forms.Orientation Orientation
        {
            get
            {
                return this.orientation;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public ToolStripOverflowButton OverflowButton
        {
            get
            {
                if (this.toolStripOverflowButton == null)
                {
                    this.toolStripOverflowButton = new ToolStripOverflowButton(this);
                    this.toolStripOverflowButton.Overflow = ToolStripItemOverflow.Never;
                    this.toolStripOverflowButton.ParentInternal = this;
                    this.toolStripOverflowButton.Alignment = ToolStripItemAlignment.Right;
                    this.toolStripOverflowButton.Size = this.toolStripOverflowButton.GetPreferredSize(this.DisplayRectangle.Size - base.Padding.Size);
                }
                return this.toolStripOverflowButton;
            }
        }

        internal ToolStripItemCollection OverflowItems
        {
            get
            {
                if (this.overflowItems == null)
                {
                    this.overflowItems = new ToolStripItemCollection(this, false);
                }
                return this.overflowItems;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ToolStripRenderer Renderer
        {
            get
            {
                if (this.IsDropDown)
                {
                    ToolStripDropDown down = this as ToolStripDropDown;
                    if (((down is ToolStripOverflow) || down.IsAutoGenerated) && (down.OwnerToolStrip != null))
                    {
                        return down.OwnerToolStrip.Renderer;
                    }
                }
                if (this.RenderMode == ToolStripRenderMode.ManagerRenderMode)
                {
                    return ToolStripManager.Renderer;
                }
                this.SetToolStripState(0x40, false);
                if (this.renderer == null)
                {
                    this.Renderer = ToolStripManager.CreateRenderer(this.RenderMode);
                }
                return this.renderer;
            }
            set
            {
                if (this.renderer != value)
                {
                    this.SetToolStripState(0x40, value == null);
                    this.renderer = value;
                    this.currentRendererType = (this.renderer != null) ? this.renderer.GetType() : typeof(System.Type);
                    this.OnRendererChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripRenderModeDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public ToolStripRenderMode RenderMode
        {
            get
            {
                if (this.GetToolStripState(0x40))
                {
                    return ToolStripRenderMode.ManagerRenderMode;
                }
                if ((this.renderer == null) || this.renderer.IsAutoGenerated)
                {
                    if (this.currentRendererType == ToolStripManager.ProfessionalRendererType)
                    {
                        return ToolStripRenderMode.Professional;
                    }
                    if (this.currentRendererType == ToolStripManager.SystemRendererType)
                    {
                        return ToolStripRenderMode.System;
                    }
                }
                return ToolStripRenderMode.Custom;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ToolStripRenderMode));
                }
                if (value == ToolStripRenderMode.Custom)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripRenderModeUseRendererPropertyInstead"));
                }
                if (value == ToolStripRenderMode.ManagerRenderMode)
                {
                    if (!this.GetToolStripState(0x40))
                    {
                        this.SetToolStripState(0x40, true);
                        this.OnRendererChanged(EventArgs.Empty);
                    }
                }
                else
                {
                    this.SetToolStripState(0x40, false);
                    this.Renderer = ToolStripManager.CreateRenderer(value);
                }
            }
        }

        internal RestoreFocusMessageFilter RestoreFocusFilter
        {
            get
            {
                if (this.restoreFocusFilter == null)
                {
                    this.restoreFocusFilter = new RestoreFocusMessageFilter(this);
                }
                return this.restoreFocusFilter;
            }
        }

        internal Hashtable Shortcuts
        {
            get
            {
                if (this.shortcuts == null)
                {
                    this.shortcuts = new Hashtable(1);
                }
                return this.shortcuts;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripShowItemToolTipsDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true)]
        public bool ShowItemToolTips
        {
            get
            {
                return this.showItemToolTips;
            }
            set
            {
                if (this.showItemToolTips != value)
                {
                    this.showItemToolTips = value;
                    if (!this.showItemToolTips)
                    {
                        this.UpdateToolTip(null);
                    }
                }
            }
        }

        internal bool ShowKeyboardCuesInternal
        {
            get
            {
                return this.ShowKeyboardCues;
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ToolStripStretchDescr"), DefaultValue(false)]
        public bool Stretch
        {
            get
            {
                return this.GetToolStripState(0x200);
            }
            set
            {
                if (this.Stretch != value)
                {
                    this.SetToolStripState(0x200, value);
                }
            }
        }

        System.Windows.Forms.ToolStripPanelCell ISupportToolStripPanel.ToolStripPanelCell
        {
            get
            {
                System.Windows.Forms.ToolStripPanelCell cell = null;
                if (!this.IsDropDown && !base.IsDisposed)
                {
                    if (base.Properties.ContainsObject(PropToolStripPanelCell))
                    {
                        return (System.Windows.Forms.ToolStripPanelCell) base.Properties.GetObject(PropToolStripPanelCell);
                    }
                    cell = new System.Windows.Forms.ToolStripPanelCell(this);
                    base.Properties.SetObject(PropToolStripPanelCell, cell);
                }
                return cell;
            }
        }

        System.Windows.Forms.ToolStripPanelRow ISupportToolStripPanel.ToolStripPanelRow
        {
            get
            {
                if (this.ToolStripPanelCell == null)
                {
                    return null;
                }
                return this.ToolStripPanelCell.ToolStripPanelRow;
            }
            set
            {
                System.Windows.Forms.ToolStripPanelRow toolStripPanelRow = this.ToolStripPanelRow;
                if (toolStripPanelRow != value)
                {
                    System.Windows.Forms.ToolStripPanelCell toolStripPanelCell = this.ToolStripPanelCell;
                    if (toolStripPanelCell != null)
                    {
                        toolStripPanelCell.ToolStripPanelRow = value;
                        if (value != null)
                        {
                            if ((toolStripPanelRow == null) || (toolStripPanelRow.Orientation != value.Orientation))
                            {
                                if (this.layoutStyle == ToolStripLayoutStyle.StackWithOverflow)
                                {
                                    this.UpdateLayoutStyle(value.Orientation);
                                }
                                else
                                {
                                    this.UpdateOrientation(value.Orientation);
                                }
                            }
                        }
                        else
                        {
                            if ((toolStripPanelRow != null) && toolStripPanelRow.ControlsInternal.Contains(this))
                            {
                                toolStripPanelRow.ControlsInternal.Remove(this);
                            }
                            this.UpdateLayoutStyle(this.Dock);
                        }
                    }
                }
            }
        }

        ArrangedElementCollection IArrangedElement.Children
        {
            get
            {
                return this.Items;
            }
        }

        bool IArrangedElement.ParticipatesInLayout
        {
            get
            {
                return base.GetState(2);
            }
        }

        [System.Windows.Forms.SRDescription("ControlTabStopDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DispId(-516), DefaultValue(false)]
        public bool TabStop
        {
            get
            {
                return base.TabStop;
            }
            set
            {
                base.TabStop = value;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripTextDirectionDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(1)]
        public virtual ToolStripTextDirection TextDirection
        {
            get
            {
                ToolStripTextDirection inherit = ToolStripTextDirection.Inherit;
                if (base.Properties.ContainsObject(PropTextDirection))
                {
                    inherit = (ToolStripTextDirection) base.Properties.GetObject(PropTextDirection);
                }
                if (inherit == ToolStripTextDirection.Inherit)
                {
                    inherit = ToolStripTextDirection.Horizontal;
                }
                return inherit;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ToolStripTextDirection));
                }
                base.Properties.SetObject(PropTextDirection, value);
                using (new LayoutTransaction(this, this, "TextDirection"))
                {
                    for (int i = 0; i < this.Items.Count; i++)
                    {
                        this.Items[i].OnOwnerTextDirectionChanged();
                    }
                }
            }
        }

        internal System.Windows.Forms.ToolStripPanelCell ToolStripPanelCell
        {
            get
            {
                return ((ISupportToolStripPanel) this).ToolStripPanelCell;
            }
        }

        internal System.Windows.Forms.ToolStripPanelRow ToolStripPanelRow
        {
            get
            {
                return ((ISupportToolStripPanel) this).ToolStripPanelRow;
            }
        }

        internal System.Windows.Forms.ToolTip ToolTip
        {
            get
            {
                if (!base.Properties.ContainsObject(PropToolTip))
                {
                    System.Windows.Forms.ToolTip tip = new System.Windows.Forms.ToolTip();
                    base.Properties.SetObject(PropToolTip, tip);
                    return tip;
                }
                return (System.Windows.Forms.ToolTip) base.Properties.GetObject(PropToolTip);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public VScrollProperties VerticalScroll
        {
            get
            {
                return base.VerticalScroll;
            }
        }

        private delegate void BooleanMethodInvoker(bool arg);

        internal class RestoreFocusMessageFilter : IMessageFilter
        {
            private ToolStrip ownerToolStrip;

            public RestoreFocusMessageFilter(ToolStrip ownerToolStrip)
            {
                this.ownerToolStrip = ownerToolStrip;
            }

            public bool PreFilterMessage(ref Message m)
            {
                if ((!this.ownerToolStrip.Disposing && !this.ownerToolStrip.IsDisposed) && !this.ownerToolStrip.IsDropDown)
                {
                    switch (m.Msg)
                    {
                        case 0x201:
                        case 0x204:
                        case 0x207:
                        case 0xa1:
                        case 0xa4:
                        case 0xa7:
                            if (this.ownerToolStrip.ContainsFocus && !System.Windows.Forms.UnsafeNativeMethods.IsChild(new HandleRef(this, this.ownerToolStrip.Handle), new HandleRef(this, m.HWnd)))
                            {
                                HandleRef rootHWnd = WindowsFormsUtils.GetRootHWnd(this.ownerToolStrip);
                                if ((rootHWnd.Handle == m.HWnd) || System.Windows.Forms.UnsafeNativeMethods.IsChild(rootHWnd, new HandleRef(this, m.HWnd)))
                                {
                                    this.RestoreFocusInternal();
                                }
                            }
                            return false;
                    }
                }
                return false;
            }

            private void RestoreFocusInternal()
            {
                this.ownerToolStrip.BeginInvoke(new ToolStrip.BooleanMethodInvoker(this.ownerToolStrip.RestoreFocusInternal), new object[] { ToolStripManager.ModalMenuFilter.InMenuMode });
                Application.ThreadContext.FromCurrent().RemoveMessageFilter(this);
            }
        }

        [ComVisible(true)]
        public class ToolStripAccessibleObject : Control.ControlAccessibleObject
        {
            private ToolStrip owner;

            public ToolStripAccessibleObject(ToolStrip owner) : base(owner)
            {
                this.owner = owner;
            }

            public override AccessibleObject GetChild(int index)
            {
                if ((this.owner == null) || (this.owner.Items == null))
                {
                    return null;
                }
                if ((index == 0) && this.owner.Grip.Visible)
                {
                    return this.owner.Grip.AccessibilityObject;
                }
                if (this.owner.Grip.Visible && (index > 0))
                {
                    index--;
                }
                if (index >= this.owner.Items.Count)
                {
                    if ((this.owner.CanOverflow && this.owner.OverflowButton.Visible) && (index == this.owner.Items.Count))
                    {
                        return this.owner.OverflowButton.AccessibilityObject;
                    }
                    return null;
                }
                ToolStripItem item = null;
                int num = 0;
                for (int i = 0; i < this.owner.Items.Count; i++)
                {
                    if (this.owner.Items[i].Available && (this.owner.Items[i].Alignment == ToolStripItemAlignment.Left))
                    {
                        if (num == index)
                        {
                            item = this.owner.Items[i];
                            break;
                        }
                        num++;
                    }
                }
                if (item == null)
                {
                    for (int j = 0; j < this.owner.Items.Count; j++)
                    {
                        if (this.owner.Items[j].Available && (this.owner.Items[j].Alignment == ToolStripItemAlignment.Right))
                        {
                            if (num == index)
                            {
                                item = this.owner.Items[j];
                                break;
                            }
                            num++;
                        }
                    }
                }
                if (item == null)
                {
                    return null;
                }
                if (item.Placement == ToolStripItemPlacement.Overflow)
                {
                    return new ToolStrip.ToolStripAccessibleObjectWrapperForItemsOnOverflow(item);
                }
                return item.AccessibilityObject;
            }

            public override int GetChildCount()
            {
                if ((this.owner == null) || (this.owner.Items == null))
                {
                    return -1;
                }
                int num = 0;
                for (int i = 0; i < this.owner.Items.Count; i++)
                {
                    if (this.owner.Items[i].Available)
                    {
                        num++;
                    }
                }
                if (this.owner.Grip.Visible)
                {
                    num++;
                }
                if (this.owner.CanOverflow && this.owner.OverflowButton.Visible)
                {
                    num++;
                }
                return num;
            }

            public override AccessibleObject HitTest(int x, int y)
            {
                Point point = this.owner.PointToClient(new Point(x, y));
                ToolStripItem itemAt = this.owner.GetItemAt(point);
                if ((itemAt != null) && (itemAt.AccessibilityObject != null))
                {
                    return itemAt.AccessibilityObject;
                }
                return base.HitTest(x, y);
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
                    return AccessibleRole.ToolBar;
                }
            }
        }

        private class ToolStripAccessibleObjectWrapperForItemsOnOverflow : ToolStripItem.ToolStripItemAccessibleObject
        {
            public ToolStripAccessibleObjectWrapperForItemsOnOverflow(ToolStripItem item) : base(item)
            {
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates states = base.State | AccessibleStates.Offscreen;
                    return (states | AccessibleStates.Invisible);
                }
            }
        }
    }
}

