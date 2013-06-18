namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms.Layout;

    [ComVisible(true), System.Windows.Forms.SRDescription("DescriptionTabControl"), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultProperty("TabPages"), Designer("System.Windows.Forms.Design.TabControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultEvent("SelectedIndexChanged")]
    public class TabControl : Control
    {
        private TabAlignment alignment;
        private TabAppearance appearance;
        private Rectangle cachedDisplayRect = Rectangle.Empty;
        private Size cachedSize = Size.Empty;
        private string controlTipText = string.Empty;
        private bool currentlyScaling;
        private static readonly Size DEFAULT_ITEMSIZE = Size.Empty;
        private static readonly Point DEFAULT_PADDING = new Point(6, 3);
        private TabDrawMode drawMode;
        private static readonly object EVENT_DESELECTED = new object();
        private static readonly object EVENT_DESELECTING = new object();
        private static readonly object EVENT_RIGHTTOLEFTLAYOUTCHANGED = new object();
        private static readonly object EVENT_SELECTED = new object();
        private static readonly object EVENT_SELECTING = new object();
        private bool handleInTable;
        private System.Windows.Forms.ImageList imageList;
        private Size itemSize = DEFAULT_ITEMSIZE;
        private int lastSelection;
        private Point padding = DEFAULT_PADDING;
        private bool rightToLeftLayout;
        private int selectedIndex = -1;
        private TabSizeMode sizeMode;
        private bool skipUpdateSize;
        private readonly int tabBaseReLayoutMessage = System.Windows.Forms.SafeNativeMethods.RegisterWindowMessage(Application.WindowMessagesVersion + "_TabBaseReLayout");
        private TabPageCollection tabCollection;
        private BitVector32 tabControlState = new BitVector32(0);
        private const int TABCONTROLSTATE_autoSize = 0x100;
        private const int TABCONTROLSTATE_fromCreateHandles = 0x10;
        private const int TABCONTROLSTATE_getTabRectfromItemSize = 8;
        private const int TABCONTROLSTATE_hotTrack = 1;
        private const int TABCONTROLSTATE_insertingItem = 0x80;
        private const int TABCONTROLSTATE_multiline = 2;
        private const int TABCONTROLSTATE_selectFirstControl = 0x40;
        private const int TABCONTROLSTATE_showToolTips = 4;
        private const int TABCONTROLSTATE_UISelection = 0x20;
        private int tabPageCount;
        private TabPage[] tabPages;

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler BackColorChanged
        {
            add
            {
                base.BackColorChanged += value;
            }
            remove
            {
                base.BackColorChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [System.Windows.Forms.SRDescription("TabControlDeselectedEventDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event TabControlEventHandler Deselected
        {
            add
            {
                base.Events.AddHandler(EVENT_DESELECTED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DESELECTED, value);
            }
        }

        [System.Windows.Forms.SRDescription("TabControlDeselectingEventDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event TabControlCancelEventHandler Deselecting
        {
            add
            {
                base.Events.AddHandler(EVENT_DESELECTING, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DESELECTING, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("drawItemEventDescr")]
        public event DrawItemEventHandler DrawItem;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("TabControlSelectedEventDescr")]
        public event TabControlEventHandler Selected
        {
            add
            {
                base.Events.AddHandler(EVENT_SELECTED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_SELECTED, value);
            }
        }

        [System.Windows.Forms.SRDescription("selectedIndexChangedEventDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler SelectedIndexChanged;

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("TabControlSelectingEventDescr")]
        public event TabControlCancelEventHandler Selecting
        {
            add
            {
                base.Events.AddHandler(EVENT_SELECTING, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_SELECTING, value);
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

        public TabControl()
        {
            this.tabCollection = new TabPageCollection(this);
            base.SetStyle(ControlStyles.UserPaint, false);
        }

        internal int AddNativeTabPage(System.Windows.Forms.NativeMethods.TCITEM_T tcitem)
        {
            int num = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.TCM_INSERTITEM, this.tabPageCount + 1, tcitem);
            System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, base.Handle), this.tabBaseReLayoutMessage, IntPtr.Zero, IntPtr.Zero);
            return num;
        }

        internal int AddTabPage(TabPage tabPage, System.Windows.Forms.NativeMethods.TCITEM_T tcitem)
        {
            int index = this.AddNativeTabPage(tcitem);
            if (index >= 0)
            {
                this.Insert(index, tabPage);
            }
            return index;
        }

        internal void ApplyItemSize()
        {
            if (base.IsHandleCreated && this.ShouldSerializeItemSize())
            {
                base.SendMessage(0x1329, 0, (int) System.Windows.Forms.NativeMethods.Util.MAKELPARAM(this.itemSize.Width, this.itemSize.Height));
            }
            this.cachedDisplayRect = Rectangle.Empty;
        }

        internal void BeginUpdate()
        {
            base.BeginUpdateInternal();
        }

        protected override Control.ControlCollection CreateControlsInstance()
        {
            return new ControlCollection(this);
        }

        protected override void CreateHandle()
        {
            if (!base.RecreatingHandle)
            {
                IntPtr userCookie = System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Activate();
                try
                {
                    System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX icc = new System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX {
                        dwICC = 8
                    };
                    System.Windows.Forms.SafeNativeMethods.InitCommonControlsEx(icc);
                }
                finally
                {
                    System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Deactivate(userCookie);
                }
            }
            base.CreateHandle();
        }

        public void DeselectTab(int index)
        {
            TabPage tabPage = this.GetTabPage(index);
            if (this.SelectedTab == tabPage)
            {
                if ((0 <= index) && (index < (this.TabPages.Count - 1)))
                {
                    this.SelectedTab = this.GetTabPage(++index);
                }
                else
                {
                    this.SelectedTab = this.GetTabPage(0);
                }
            }
        }

        public void DeselectTab(string tabPageName)
        {
            if (tabPageName == null)
            {
                throw new ArgumentNullException("tabPageName");
            }
            TabPage tabPage = this.TabPages[tabPageName];
            this.DeselectTab(tabPage);
        }

        public void DeselectTab(TabPage tabPage)
        {
            if (tabPage == null)
            {
                throw new ArgumentNullException("tabPage");
            }
            int index = this.FindTabPage(tabPage);
            this.DeselectTab(index);
        }

        private void DetachImageList(object sender, EventArgs e)
        {
            this.ImageList = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.imageList != null))
            {
                this.imageList.Disposed -= new EventHandler(this.DetachImageList);
            }
            base.Dispose(disposing);
        }

        internal void EndUpdate()
        {
            this.EndUpdate(true);
        }

        internal void EndUpdate(bool invalidate)
        {
            base.EndUpdateInternal(invalidate);
        }

        internal int FindTabPage(TabPage tabPage)
        {
            if (this.tabPages != null)
            {
                for (int i = 0; i < this.tabPageCount; i++)
                {
                    if (this.tabPages[i].Equals(tabPage))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public Control GetControl(int index)
        {
            return this.GetTabPage(index);
        }

        protected virtual object[] GetItems()
        {
            TabPage[] destinationArray = new TabPage[this.tabPageCount];
            if (this.tabPageCount > 0)
            {
                Array.Copy(this.tabPages, 0, destinationArray, 0, this.tabPageCount);
            }
            return destinationArray;
        }

        protected virtual object[] GetItems(System.Type baseType)
        {
            object[] destinationArray = (object[]) Array.CreateInstance(baseType, this.tabPageCount);
            if (this.tabPageCount > 0)
            {
                Array.Copy(this.tabPages, 0, destinationArray, 0, this.tabPageCount);
            }
            return destinationArray;
        }

        internal TabPage GetTabPage(int index)
        {
            if ((index < 0) || (index >= this.tabPageCount))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            return this.tabPages[index];
        }

        internal TabPage[] GetTabPages()
        {
            return (TabPage[]) this.GetItems();
        }

        public Rectangle GetTabRect(int index)
        {
            if ((index < 0) || ((index >= this.tabPageCount) && !this.tabControlState[8]))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            this.tabControlState[8] = false;
            System.Windows.Forms.NativeMethods.RECT lparam = new System.Windows.Forms.NativeMethods.RECT();
            if (!base.IsHandleCreated)
            {
                this.CreateHandle();
            }
            base.SendMessage(0x130a, index, ref lparam);
            return Rectangle.FromLTRB(lparam.left, lparam.top, lparam.right, lparam.bottom);
        }

        protected string GetToolTipText(object item)
        {
            return ((TabPage) item).ToolTipText;
        }

        private void ImageListRecreateHandle(object sender, EventArgs e)
        {
            if (base.IsHandleCreated)
            {
                base.SendMessage(0x1303, 0, this.ImageList.Handle);
            }
        }

        internal void Insert(int index, TabPage tabPage)
        {
            if (this.tabPages == null)
            {
                this.tabPages = new TabPage[4];
            }
            else if (this.tabPages.Length == this.tabPageCount)
            {
                TabPage[] destinationArray = new TabPage[this.tabPageCount * 2];
                Array.Copy(this.tabPages, 0, destinationArray, 0, this.tabPageCount);
                this.tabPages = destinationArray;
            }
            if (index < this.tabPageCount)
            {
                Array.Copy(this.tabPages, index, this.tabPages, index + 1, this.tabPageCount - index);
            }
            this.tabPages[index] = tabPage;
            this.tabPageCount++;
            this.cachedDisplayRect = Rectangle.Empty;
            this.ApplyItemSize();
            if (this.Appearance == TabAppearance.FlatButtons)
            {
                base.Invalidate();
            }
        }

        private void InsertItem(int index, TabPage tabPage)
        {
            if ((index < 0) || ((this.tabPages != null) && (index > this.tabPageCount)))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            if (tabPage == null)
            {
                throw new ArgumentNullException("tabPage");
            }
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.NativeMethods.TCITEM_T tCITEM = tabPage.GetTCITEM();
                int num = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.TCM_INSERTITEM, index, tCITEM);
                if (num >= 0)
                {
                    this.Insert(num, tabPage);
                }
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if ((keyData & Keys.Alt) == Keys.Alt)
            {
                return false;
            }
            switch ((keyData & Keys.KeyCode))
            {
                case Keys.PageUp:
                case Keys.Next:
                case Keys.End:
                case Keys.Home:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected virtual void OnDeselected(TabControlEventArgs e)
        {
            TabControlEventHandler handler = (TabControlEventHandler) base.Events[EVENT_DESELECTED];
            if (handler != null)
            {
                handler(this, e);
            }
            if (this.SelectedTab != null)
            {
                this.SelectedTab.FireLeave(EventArgs.Empty);
            }
        }

        protected virtual void OnDeselecting(TabControlCancelEventArgs e)
        {
            TabControlCancelEventHandler handler = (TabControlCancelEventHandler) base.Events[EVENT_DESELECTING];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDrawItem(DrawItemEventArgs e)
        {
            if (this.onDrawItem != null)
            {
                this.onDrawItem(this, e);
            }
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            if (this.SelectedTab != null)
            {
                this.SelectedTab.FireEnter(e);
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.cachedDisplayRect = Rectangle.Empty;
            this.UpdateSize();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            NativeWindow.AddWindowToIDTable(this, base.Handle);
            this.handleInTable = true;
            if (!this.padding.IsEmpty)
            {
                base.SendMessage(0x132b, 0, System.Windows.Forms.NativeMethods.Util.MAKELPARAM(this.padding.X, this.padding.Y));
            }
            base.OnHandleCreated(e);
            this.cachedDisplayRect = Rectangle.Empty;
            this.ApplyItemSize();
            if (this.imageList != null)
            {
                base.SendMessage(0x1303, 0, this.imageList.Handle);
            }
            if (this.ShowToolTips)
            {
                IntPtr handle = base.SendMessage(0x132d, 0, 0);
                if (handle != IntPtr.Zero)
                {
                    System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this, handle), System.Windows.Forms.NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, 0x13);
                }
            }
            foreach (TabPage page in this.TabPages)
            {
                this.AddNativeTabPage(page.GetTCITEM());
            }
            this.ResizePages();
            if (this.selectedIndex != -1)
            {
                try
                {
                    this.tabControlState[0x10] = true;
                    this.SelectedIndex = this.selectedIndex;
                }
                finally
                {
                    this.tabControlState[0x10] = false;
                }
                this.selectedIndex = -1;
            }
            this.UpdateTabSelection(false);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (!base.Disposing)
            {
                this.selectedIndex = this.SelectedIndex;
            }
            if (this.handleInTable)
            {
                this.handleInTable = false;
                NativeWindow.RemoveWindowFromIDTable(base.Handle);
            }
            base.OnHandleDestroyed(e);
        }

        protected override void OnKeyDown(KeyEventArgs ke)
        {
            if ((ke.KeyCode == Keys.Tab) && ((ke.KeyData & Keys.Control) != Keys.None))
            {
                bool forward = (ke.KeyData & Keys.Shift) == Keys.None;
                this.SelectNextTab(ke, forward);
            }
            if ((ke.KeyCode == Keys.Next) && ((ke.KeyData & Keys.Control) != Keys.None))
            {
                this.SelectNextTab(ke, true);
            }
            if ((ke.KeyCode == Keys.PageUp) && ((ke.KeyData & Keys.Control) != Keys.None))
            {
                this.SelectNextTab(ke, false);
            }
            base.OnKeyDown(ke);
        }

        protected override void OnLeave(EventArgs e)
        {
            if (this.SelectedTab != null)
            {
                this.SelectedTab.FireLeave(e);
            }
            base.OnLeave(e);
        }

        internal override void OnParentHandleRecreated()
        {
            this.skipUpdateSize = true;
            try
            {
                base.OnParentHandleRecreated();
            }
            finally
            {
                this.skipUpdateSize = false;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.cachedDisplayRect = Rectangle.Empty;
            this.UpdateTabSelection(false);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnRightToLeftLayoutChanged(EventArgs e)
        {
            if (!base.GetAnyDisposingInHierarchy())
            {
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    base.RecreateHandle();
                }
                EventHandler handler = base.Events[EVENT_RIGHTTOLEFTLAYOUTCHANGED] as EventHandler;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
        }

        protected virtual void OnSelected(TabControlEventArgs e)
        {
            TabControlEventHandler handler = (TabControlEventHandler) base.Events[EVENT_SELECTED];
            if (handler != null)
            {
                handler(this, e);
            }
            if (this.SelectedTab != null)
            {
                this.SelectedTab.FireEnter(EventArgs.Empty);
            }
        }

        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            int selectedIndex = this.SelectedIndex;
            this.cachedDisplayRect = Rectangle.Empty;
            this.UpdateTabSelection(this.tabControlState[0x20]);
            this.tabControlState[0x20] = false;
            if (this.onSelectedIndexChanged != null)
            {
                this.onSelectedIndexChanged(this, e);
            }
        }

        protected virtual void OnSelecting(TabControlCancelEventArgs e)
        {
            TabControlCancelEventHandler handler = (TabControlCancelEventHandler) base.Events[EVENT_SELECTING];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnStyleChanged(EventArgs e)
        {
            base.OnStyleChanged(e);
            this.cachedDisplayRect = Rectangle.Empty;
            this.UpdateTabSelection(false);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessKeyPreview(ref Message m)
        {
            return (this.ProcessKeyEventArgs(ref m) || base.ProcessKeyPreview(ref m));
        }

        internal override void RecreateHandleCore()
        {
            TabPage[] tabPages = this.GetTabPages();
            int num = ((tabPages.Length > 0) && (this.SelectedIndex == -1)) ? 0 : this.SelectedIndex;
            if (base.IsHandleCreated)
            {
                base.SendMessage(0x1309, 0, 0);
            }
            this.tabPages = null;
            this.tabPageCount = 0;
            base.RecreateHandleCore();
            for (int i = 0; i < tabPages.Length; i++)
            {
                this.TabPages.Add(tabPages[i]);
            }
            try
            {
                this.tabControlState[0x10] = true;
                this.SelectedIndex = num;
            }
            finally
            {
                this.tabControlState[0x10] = false;
            }
            this.UpdateSize();
        }

        protected void RemoveAll()
        {
            base.Controls.Clear();
            base.SendMessage(0x1309, 0, 0);
            this.tabPages = null;
            this.tabPageCount = 0;
        }

        internal void RemoveTabPage(int index)
        {
            if ((index < 0) || (index >= this.tabPageCount))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            this.tabPageCount--;
            if (index < this.tabPageCount)
            {
                Array.Copy(this.tabPages, index + 1, this.tabPages, index, this.tabPageCount - index);
            }
            this.tabPages[this.tabPageCount] = null;
            if (base.IsHandleCreated)
            {
                base.SendMessage(0x1308, index, 0);
            }
            this.cachedDisplayRect = Rectangle.Empty;
        }

        private void ResetItemSize()
        {
            this.ItemSize = DEFAULT_ITEMSIZE;
        }

        private void ResetPadding()
        {
            this.Padding = DEFAULT_PADDING;
        }

        private void ResizePages()
        {
            Rectangle displayRectangle = this.DisplayRectangle;
            TabPage[] tabPages = this.GetTabPages();
            for (int i = 0; i < tabPages.Length; i++)
            {
                tabPages[i].Bounds = displayRectangle;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void ScaleCore(float dx, float dy)
        {
            this.currentlyScaling = true;
            base.ScaleCore(dx, dy);
            this.currentlyScaling = false;
        }

        private void SelectNextTab(KeyEventArgs ke, bool forward)
        {
            bool focused = this.Focused;
            if (this.WmSelChanging())
            {
                this.tabControlState[0x20] = false;
            }
            else if (base.ValidationCancelled)
            {
                this.tabControlState[0x20] = false;
            }
            else
            {
                int selectedIndex = this.SelectedIndex;
                if (selectedIndex != -1)
                {
                    int tabCount = this.TabCount;
                    if (forward)
                    {
                        selectedIndex = (selectedIndex + 1) % tabCount;
                    }
                    else
                    {
                        selectedIndex = ((selectedIndex + tabCount) - 1) % tabCount;
                    }
                    try
                    {
                        this.tabControlState[0x20] = true;
                        this.tabControlState[0x40] = true;
                        this.SelectedIndex = selectedIndex;
                        this.tabControlState[0x40] = !focused;
                        this.WmSelChange();
                    }
                    finally
                    {
                        this.tabControlState[0x40] = false;
                        ke.Handled = true;
                    }
                }
            }
        }

        public void SelectTab(int index)
        {
            TabPage tabPage = this.GetTabPage(index);
            if (tabPage != null)
            {
                this.SelectedTab = tabPage;
            }
        }

        public void SelectTab(string tabPageName)
        {
            if (tabPageName == null)
            {
                throw new ArgumentNullException("tabPageName");
            }
            TabPage tabPage = this.TabPages[tabPageName];
            this.SelectTab(tabPage);
        }

        public void SelectTab(TabPage tabPage)
        {
            if (tabPage == null)
            {
                throw new ArgumentNullException("tabPage");
            }
            int index = this.FindTabPage(tabPage);
            this.SelectTab(index);
        }

        internal void SetTabPage(int index, TabPage tabPage, System.Windows.Forms.NativeMethods.TCITEM_T tcitem)
        {
            if ((index < 0) || (index >= this.tabPageCount))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.TCM_SETITEM, index, tcitem);
            }
            if (base.DesignMode && base.IsHandleCreated)
            {
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x130c, (IntPtr) index, IntPtr.Zero);
            }
            this.tabPages[index] = tabPage;
        }

        internal void SetToolTip(ToolTip toolTip, string controlToolTipText)
        {
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x132e, new HandleRef(toolTip, toolTip.Handle), 0);
            this.controlTipText = controlToolTipText;
        }

        internal override bool ShouldPerformContainerValidation()
        {
            return true;
        }

        private bool ShouldSerializeItemSize()
        {
            return !this.itemSize.Equals(DEFAULT_ITEMSIZE);
        }

        private bool ShouldSerializePadding()
        {
            return !this.padding.Equals(DEFAULT_PADDING);
        }

        public override string ToString()
        {
            string str = base.ToString();
            if (this.TabPages != null)
            {
                str = str + ", TabPages.Count: " + this.TabPages.Count.ToString(CultureInfo.CurrentCulture);
                if (this.TabPages.Count > 0)
                {
                    str = str + ", TabPages[0]: " + this.TabPages[0].ToString();
                }
            }
            return str;
        }

        internal void UpdateSize()
        {
            if (!this.skipUpdateSize)
            {
                this.BeginUpdate();
                Size size = base.Size;
                base.Size = new Size(size.Width + 1, size.Height);
                base.Size = size;
                this.EndUpdate();
            }
        }

        internal void UpdateTab(TabPage tabPage)
        {
            int index = this.FindTabPage(tabPage);
            this.SetTabPage(index, tabPage, tabPage.GetTCITEM());
            this.cachedDisplayRect = Rectangle.Empty;
            this.UpdateTabSelection(false);
        }

        protected void UpdateTabSelection(bool updateFocus)
        {
            if (base.IsHandleCreated)
            {
                int selectedIndex = this.SelectedIndex;
                TabPage[] tabPages = this.GetTabPages();
                if (selectedIndex != -1)
                {
                    if (this.currentlyScaling)
                    {
                        tabPages[selectedIndex].SuspendLayout();
                    }
                    tabPages[selectedIndex].Bounds = this.DisplayRectangle;
                    tabPages[selectedIndex].Invalidate();
                    if (this.currentlyScaling)
                    {
                        tabPages[selectedIndex].ResumeLayout(false);
                    }
                    tabPages[selectedIndex].Visible = true;
                    if (updateFocus && (!this.Focused || this.tabControlState[0x40]))
                    {
                        this.tabControlState[0x20] = false;
                        bool flag = false;
                        System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                        try
                        {
                            flag = tabPages[selectedIndex].SelectNextControl(null, true, true, false, false);
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                        if (flag)
                        {
                            if (!base.ContainsFocus)
                            {
                                IContainerControl containerControlInternal = base.GetContainerControlInternal();
                                if (containerControlInternal != null)
                                {
                                    while (containerControlInternal.ActiveControl is ContainerControl)
                                    {
                                        containerControlInternal = (IContainerControl) containerControlInternal.ActiveControl;
                                    }
                                    if (containerControlInternal.ActiveControl != null)
                                    {
                                        containerControlInternal.ActiveControl.FocusInternal();
                                    }
                                }
                            }
                        }
                        else
                        {
                            IContainerControl control2 = base.GetContainerControlInternal();
                            if ((control2 != null) && !base.DesignMode)
                            {
                                if (control2 is ContainerControl)
                                {
                                    ((ContainerControl) control2).SetActiveControlInternal(this);
                                }
                                else
                                {
                                    System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                                    try
                                    {
                                        control2.ActiveControl = this;
                                    }
                                    finally
                                    {
                                        CodeAccessPermission.RevertAssert();
                                    }
                                }
                            }
                        }
                    }
                }
                for (int i = 0; i < tabPages.Length; i++)
                {
                    if (i != this.SelectedIndex)
                    {
                        tabPages[i].Visible = false;
                    }
                }
            }
        }

        private void WmNeedText(ref Message m)
        {
            System.Windows.Forms.NativeMethods.TOOLTIPTEXT lParam = (System.Windows.Forms.NativeMethods.TOOLTIPTEXT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.TOOLTIPTEXT));
            int idFrom = (int) lParam.hdr.idFrom;
            string toolTipText = this.GetToolTipText(this.GetTabPage(idFrom));
            if (!string.IsNullOrEmpty(toolTipText))
            {
                lParam.lpszText = toolTipText;
            }
            else
            {
                lParam.lpszText = this.controlTipText;
            }
            lParam.hinst = IntPtr.Zero;
            if (this.RightToLeft == RightToLeft.Yes)
            {
                lParam.uFlags |= 4;
            }
            Marshal.StructureToPtr(lParam, m.LParam, false);
        }

        private void WmReflectDrawItem(ref Message m)
        {
            System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT lParam = (System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT));
            IntPtr handle = Control.SetUpPalette(lParam.hDC, false, false);
            using (Graphics graphics = Graphics.FromHdcInternal(lParam.hDC))
            {
                this.OnDrawItem(new DrawItemEventArgs(graphics, this.Font, Rectangle.FromLTRB(lParam.rcItem.left, lParam.rcItem.top, lParam.rcItem.right, lParam.rcItem.bottom), lParam.itemID, (DrawItemState) lParam.itemState));
            }
            if (handle != IntPtr.Zero)
            {
                System.Windows.Forms.SafeNativeMethods.SelectPalette(new HandleRef(null, lParam.hDC), new HandleRef(null, handle), 0);
            }
            m.Result = (IntPtr) 1;
        }

        private bool WmSelChange()
        {
            TabControlCancelEventArgs e = new TabControlCancelEventArgs(this.SelectedTab, this.SelectedIndex, false, TabControlAction.Selecting);
            this.OnSelecting(e);
            if (!e.Cancel)
            {
                this.OnSelected(new TabControlEventArgs(this.SelectedTab, this.SelectedIndex, TabControlAction.Selected));
                this.OnSelectedIndexChanged(EventArgs.Empty);
            }
            else
            {
                base.SendMessage(0x130c, this.lastSelection, 0);
                this.UpdateTabSelection(true);
            }
            return e.Cancel;
        }

        private bool WmSelChanging()
        {
            IContainerControl containerControlInternal = base.GetContainerControlInternal();
            if ((containerControlInternal != null) && !base.DesignMode)
            {
                if (containerControlInternal is ContainerControl)
                {
                    ((ContainerControl) containerControlInternal).SetActiveControlInternal(this);
                }
                else
                {
                    System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                    try
                    {
                        containerControlInternal.ActiveControl = this;
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
            this.lastSelection = this.SelectedIndex;
            TabControlCancelEventArgs e = new TabControlCancelEventArgs(this.SelectedTab, this.SelectedIndex, false, TabControlAction.Deselecting);
            this.OnDeselecting(e);
            if (!e.Cancel)
            {
                this.OnDeselected(new TabControlEventArgs(this.SelectedTab, this.SelectedIndex, TabControlAction.Deselected));
            }
            return e.Cancel;
        }

        private void WmTabBaseReLayout(ref Message m)
        {
            this.BeginUpdate();
            this.cachedDisplayRect = Rectangle.Empty;
            this.UpdateTabSelection(false);
            this.EndUpdate();
            base.Invalidate(true);
            System.Windows.Forms.NativeMethods.MSG msg = new System.Windows.Forms.NativeMethods.MSG();
            IntPtr handle = base.Handle;
            while (System.Windows.Forms.UnsafeNativeMethods.PeekMessage(ref msg, new HandleRef(this, handle), this.tabBaseReLayoutMessage, this.tabBaseReLayoutMessage, 1))
            {
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x202b:
                    this.WmReflectDrawItem(ref m);
                    break;

                case 0x204e:
                case 0x4e:
                {
                    System.Windows.Forms.NativeMethods.NMHDR lParam = (System.Windows.Forms.NativeMethods.NMHDR) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHDR));
                    switch (lParam.code)
                    {
                        case -552:
                            if (!this.WmSelChanging())
                            {
                                if (base.ValidationCancelled)
                                {
                                    m.Result = (IntPtr) 1;
                                    this.tabControlState[0x20] = false;
                                    return;
                                }
                                this.tabControlState[0x20] = true;
                                break;
                            }
                            m.Result = (IntPtr) 1;
                            this.tabControlState[0x20] = false;
                            return;

                        case -551:
                            if (!this.WmSelChange())
                            {
                                this.tabControlState[0x20] = true;
                                break;
                            }
                            m.Result = (IntPtr) 1;
                            this.tabControlState[0x20] = false;
                            return;

                        case -530:
                        case -520:
                            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(lParam, lParam.hwndFrom), 0x418, 0, SystemInformation.MaxWindowTrackSize.Width);
                            this.WmNeedText(ref m);
                            m.Result = (IntPtr) 1;
                            return;
                    }
                    break;
                }
            }
            if (m.Msg == this.tabBaseReLayoutMessage)
            {
                this.WmTabBaseReLayout(ref m);
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        [Localizable(true), DefaultValue(0), RefreshProperties(RefreshProperties.All), System.Windows.Forms.SRDescription("TabBaseAlignmentDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public TabAlignment Alignment
        {
            get
            {
                return this.alignment;
            }
            set
            {
                if (this.alignment != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(TabAlignment));
                    }
                    this.alignment = value;
                    if ((this.alignment == TabAlignment.Left) || (this.alignment == TabAlignment.Right))
                    {
                        this.Multiline = true;
                    }
                    base.RecreateHandle();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TabBaseAppearanceDescr"), Localizable(true), DefaultValue(0)]
        public TabAppearance Appearance
        {
            get
            {
                if ((this.appearance == TabAppearance.FlatButtons) && (this.alignment != TabAlignment.Top))
                {
                    return TabAppearance.Buttons;
                }
                return this.appearance;
            }
            set
            {
                if (this.appearance != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(TabAppearance));
                    }
                    this.appearance = value;
                    base.RecreateHandle();
                    this.OnStyleChanged(EventArgs.Empty);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override Color BackColor
        {
            get
            {
                return SystemColors.Control;
            }
            set
            {
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

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ClassName = "SysTabControl32";
                if (this.Multiline)
                {
                    createParams.Style |= 0x200;
                }
                if (this.drawMode == TabDrawMode.OwnerDrawFixed)
                {
                    createParams.Style |= 0x2000;
                }
                if (this.ShowToolTips && !base.DesignMode)
                {
                    createParams.Style |= 0x4000;
                }
                if ((this.alignment == TabAlignment.Bottom) || (this.alignment == TabAlignment.Right))
                {
                    createParams.Style |= 2;
                }
                if ((this.alignment == TabAlignment.Left) || (this.alignment == TabAlignment.Right))
                {
                    createParams.Style |= 640;
                }
                if (this.tabControlState[1])
                {
                    createParams.Style |= 0x40;
                }
                if (this.appearance == TabAppearance.Normal)
                {
                    createParams.Style = createParams.Style;
                }
                else
                {
                    createParams.Style |= 0x100;
                    if ((this.appearance == TabAppearance.FlatButtons) && (this.alignment == TabAlignment.Top))
                    {
                        createParams.Style |= 8;
                    }
                }
                switch (this.sizeMode)
                {
                    case TabSizeMode.Normal:
                        createParams.Style |= 0x800;
                        break;

                    case TabSizeMode.FillToRight:
                        createParams.Style = createParams.Style;
                        break;

                    case TabSizeMode.Fixed:
                        createParams.Style |= 0x400;
                        break;
                }
                if ((this.RightToLeft == RightToLeft.Yes) && this.RightToLeftLayout)
                {
                    createParams.ExStyle |= 0x500000;
                    createParams.ExStyle &= -28673;
                }
                return createParams;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(200, 100);
            }
        }

        public override Rectangle DisplayRectangle
        {
            get
            {
                if (!this.cachedDisplayRect.IsEmpty)
                {
                    return this.cachedDisplayRect;
                }
                Rectangle bounds = base.Bounds;
                System.Windows.Forms.NativeMethods.RECT lparam = System.Windows.Forms.NativeMethods.RECT.FromXYWH(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                if (!base.IsDisposed)
                {
                    if (!base.IsActiveX && !base.IsHandleCreated)
                    {
                        this.CreateHandle();
                    }
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x1328, 0, ref lparam);
                    }
                }
                Rectangle rectangle2 = Rectangle.FromLTRB(lparam.left, lparam.top, lparam.right, lparam.bottom);
                Point location = base.Location;
                rectangle2.X -= location.X;
                rectangle2.Y -= location.Y;
                this.cachedDisplayRect = rectangle2;
                return rectangle2;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override bool DoubleBuffered
        {
            get
            {
                return base.DoubleBuffered;
            }
            set
            {
                base.DoubleBuffered = value;
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRDescription("TabBaseDrawModeDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public TabDrawMode DrawMode
        {
            get
            {
                return this.drawMode;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(TabDrawMode));
                }
                if (this.drawMode != value)
                {
                    this.drawMode = value;
                    base.RecreateHandle();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [System.Windows.Forms.SRDescription("TabBaseHotTrackDescr"), DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool HotTrack
        {
            get
            {
                return this.tabControlState[1];
            }
            set
            {
                if (this.HotTrack != value)
                {
                    this.tabControlState[1] = value;
                    if (base.IsHandleCreated)
                    {
                        base.RecreateHandle();
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue((string) null), System.Windows.Forms.SRDescription("TabBaseImageListDescr"), RefreshProperties(RefreshProperties.Repaint)]
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
                    EventHandler handler2 = new EventHandler(this.DetachImageList);
                    if (this.imageList != null)
                    {
                        this.imageList.RecreateHandle -= handler;
                        this.imageList.Disposed -= handler2;
                    }
                    this.imageList = value;
                    IntPtr lparam = (value != null) ? value.Handle : IntPtr.Zero;
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x1303, IntPtr.Zero, lparam);
                    }
                    foreach (TabPage page in this.TabPages)
                    {
                        page.ImageIndexer.ImageList = value;
                    }
                    if (value != null)
                    {
                        value.RecreateHandle += handler;
                        value.Disposed += handler2;
                    }
                }
            }
        }

        private bool InsertingItem
        {
            get
            {
                return this.tabControlState[0x80];
            }
            set
            {
                this.tabControlState[0x80] = value;
            }
        }

        [System.Windows.Forms.SRDescription("TabBaseItemSizeDescr"), System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true)]
        public Size ItemSize
        {
            get
            {
                if (!this.itemSize.IsEmpty)
                {
                    return this.itemSize;
                }
                if (base.IsHandleCreated)
                {
                    this.tabControlState[8] = true;
                    return this.GetTabRect(0).Size;
                }
                return DEFAULT_ITEMSIZE;
            }
            set
            {
                if ((value.Width < 0) || (value.Height < 0))
                {
                    throw new ArgumentOutOfRangeException("ItemSize", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "ItemSize", value.ToString() }));
                }
                this.itemSize = value;
                this.ApplyItemSize();
                this.UpdateSize();
                base.Invalidate();
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TabBaseMultilineDescr"), DefaultValue(false)]
        public bool Multiline
        {
            get
            {
                return this.tabControlState[2];
            }
            set
            {
                if (this.Multiline != value)
                {
                    this.tabControlState[2] = value;
                    if (!this.Multiline && ((this.alignment == TabAlignment.Left) || (this.alignment == TabAlignment.Right)))
                    {
                        this.alignment = TabAlignment.Top;
                    }
                    base.RecreateHandle();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true), System.Windows.Forms.SRDescription("TabBasePaddingDescr")]
        public Point Padding
        {
            get
            {
                return this.padding;
            }
            set
            {
                if ((value.X < 0) || (value.Y < 0))
                {
                    throw new ArgumentOutOfRangeException("Padding", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "Padding", value.ToString() }));
                }
                if (this.padding != value)
                {
                    this.padding = value;
                    if (base.IsHandleCreated)
                    {
                        base.RecreateHandle();
                    }
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("ControlRightToLeftLayoutDescr"), System.Windows.Forms.SRCategory("CatAppearance"), Localizable(true)]
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

        [System.Windows.Forms.SRDescription("TabBaseRowCountDescr"), System.Windows.Forms.SRCategory("CatAppearance"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int RowCount
        {
            get
            {
                return (int) ((long) base.SendMessage(0x132c, 0, 0));
            }
        }

        [System.Windows.Forms.SRDescription("selectedIndexDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(-1), Browsable(false)]
        public int SelectedIndex
        {
            get
            {
                if (base.IsHandleCreated)
                {
                    return (int) ((long) base.SendMessage(0x130b, 0, 0));
                }
                return this.selectedIndex;
            }
            set
            {
                if (value < -1)
                {
                    object[] args = new object[] { "SelectedIndex", value.ToString(CultureInfo.CurrentCulture), -1.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("SelectedIndex", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                if (this.SelectedIndex != value)
                {
                    if (base.IsHandleCreated)
                    {
                        if (!this.tabControlState[0x10] && !this.tabControlState[0x40])
                        {
                            this.tabControlState[0x20] = true;
                            if (this.WmSelChanging())
                            {
                                this.tabControlState[0x20] = false;
                                return;
                            }
                            if (base.ValidationCancelled)
                            {
                                this.tabControlState[0x20] = false;
                                return;
                            }
                        }
                        base.SendMessage(0x130c, value, 0);
                        if (!this.tabControlState[0x10] && !this.tabControlState[0x40])
                        {
                            this.tabControlState[0x40] = true;
                            if (this.WmSelChange())
                            {
                                this.tabControlState[0x20] = false;
                                this.tabControlState[0x40] = false;
                            }
                            else
                            {
                                this.tabControlState[0x40] = false;
                            }
                        }
                    }
                    else
                    {
                        this.selectedIndex = value;
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("TabControlSelectedTabDescr"), Browsable(false), System.Windows.Forms.SRCategory("CatAppearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TabPage SelectedTab
        {
            get
            {
                return this.SelectedTabInternal;
            }
            set
            {
                this.SelectedTabInternal = value;
            }
        }

        internal TabPage SelectedTabInternal
        {
            get
            {
                int selectedIndex = this.SelectedIndex;
                if (selectedIndex == -1)
                {
                    return null;
                }
                return this.tabPages[selectedIndex];
            }
            set
            {
                int num = this.FindTabPage(value);
                this.SelectedIndex = num;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TabBaseShowToolTipsDescr"), DefaultValue(false), Localizable(true)]
        public bool ShowToolTips
        {
            get
            {
                return this.tabControlState[4];
            }
            set
            {
                if (this.ShowToolTips != value)
                {
                    this.tabControlState[4] = value;
                    base.RecreateHandle();
                }
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRDescription("TabBaseSizeModeDescr"), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRCategory("CatBehavior")]
        public TabSizeMode SizeMode
        {
            get
            {
                return this.sizeMode;
            }
            set
            {
                if (this.sizeMode != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(TabSizeMode));
                    }
                    this.sizeMode = value;
                    base.RecreateHandle();
                }
            }
        }

        [System.Windows.Forms.SRDescription("TabBaseTabCountDescr"), System.Windows.Forms.SRCategory("CatAppearance"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TabCount
        {
            get
            {
                return this.tabPageCount;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MergableProperty(false), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TabControlTabsDescr"), Editor("System.Windows.Forms.Design.TabPageCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public TabPageCollection TabPages
        {
            get
            {
                return this.tabCollection;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Bindable(false), Browsable(false)]
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

        [ComVisible(false)]
        public class ControlCollection : Control.ControlCollection
        {
            private TabControl owner;

            public ControlCollection(TabControl owner) : base(owner)
            {
                this.owner = owner;
            }

            public override void Add(Control value)
            {
                if (!(value is TabPage))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("TabControlInvalidTabPageType", new object[] { value.GetType().Name }));
                }
                TabPage tabPage = (TabPage) value;
                if (!this.owner.InsertingItem)
                {
                    if (this.owner.IsHandleCreated)
                    {
                        this.owner.AddTabPage(tabPage, tabPage.GetTCITEM());
                    }
                    else
                    {
                        this.owner.Insert(this.owner.TabCount, tabPage);
                    }
                }
                base.Add(tabPage);
                tabPage.Visible = false;
                if (this.owner.IsHandleCreated)
                {
                    tabPage.Bounds = this.owner.DisplayRectangle;
                }
                ISite site = this.owner.Site;
                if ((site != null) && (tabPage.Site == null))
                {
                    IContainer container = site.Container;
                    if (container != null)
                    {
                        container.Add(tabPage);
                    }
                }
                this.owner.ApplyItemSize();
                this.owner.UpdateTabSelection(false);
            }

            public override void Remove(Control value)
            {
                base.Remove(value);
                if (value is TabPage)
                {
                    int index = this.owner.FindTabPage((TabPage) value);
                    int selectedIndex = this.owner.SelectedIndex;
                    if (index != -1)
                    {
                        this.owner.RemoveTabPage(index);
                        if (index == selectedIndex)
                        {
                            this.owner.SelectedIndex = 0;
                        }
                    }
                    this.owner.UpdateTabSelection(false);
                }
            }
        }

        public class TabPageCollection : IList, ICollection, IEnumerable
        {
            private int lastAccessedIndex = -1;
            private TabControl owner;

            public TabPageCollection(TabControl owner)
            {
                if (owner == null)
                {
                    throw new ArgumentNullException("owner");
                }
                this.owner = owner;
            }

            public void Add(string text)
            {
                TabPage page = new TabPage {
                    Text = text
                };
                this.Add(page);
            }

            public void Add(TabPage value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.owner.Controls.Add(value);
            }

            public void Add(string key, string text)
            {
                TabPage page = new TabPage {
                    Name = key,
                    Text = text
                };
                this.Add(page);
            }

            public void Add(string key, string text, int imageIndex)
            {
                TabPage page = new TabPage {
                    Name = key,
                    Text = text,
                    ImageIndex = imageIndex
                };
                this.Add(page);
            }

            public void Add(string key, string text, string imageKey)
            {
                TabPage page = new TabPage {
                    Name = key,
                    Text = text,
                    ImageKey = imageKey
                };
                this.Add(page);
            }

            public void AddRange(TabPage[] pages)
            {
                if (pages == null)
                {
                    throw new ArgumentNullException("pages");
                }
                foreach (TabPage page in pages)
                {
                    this.Add(page);
                }
            }

            public virtual void Clear()
            {
                this.owner.RemoveAll();
            }

            public bool Contains(TabPage page)
            {
                if (page == null)
                {
                    throw new ArgumentNullException("value");
                }
                return (this.IndexOf(page) != -1);
            }

            public virtual bool ContainsKey(string key)
            {
                return this.IsValidIndex(this.IndexOfKey(key));
            }

            public IEnumerator GetEnumerator()
            {
                TabPage[] tabPages = this.owner.GetTabPages();
                if (tabPages != null)
                {
                    return tabPages.GetEnumerator();
                }
                return new TabPage[0].GetEnumerator();
            }

            public int IndexOf(TabPage page)
            {
                if (page == null)
                {
                    throw new ArgumentNullException("value");
                }
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i] == page)
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
                TabPage tabPage = new TabPage {
                    Text = text
                };
                this.Insert(index, tabPage);
            }

            public void Insert(int index, TabPage tabPage)
            {
                this.owner.InsertItem(index, tabPage);
                try
                {
                    this.owner.InsertingItem = true;
                    this.owner.Controls.Add(tabPage);
                }
                finally
                {
                    this.owner.InsertingItem = false;
                }
                this.owner.Controls.SetChildIndex(tabPage, index);
            }

            public void Insert(int index, string key, string text)
            {
                TabPage tabPage = new TabPage {
                    Name = key,
                    Text = text
                };
                this.Insert(index, tabPage);
            }

            public void Insert(int index, string key, string text, int imageIndex)
            {
                TabPage tabPage = new TabPage {
                    Name = key,
                    Text = text
                };
                this.Insert(index, tabPage);
                tabPage.ImageIndex = imageIndex;
            }

            public void Insert(int index, string key, string text, string imageKey)
            {
                TabPage tabPage = new TabPage {
                    Name = key,
                    Text = text
                };
                this.Insert(index, tabPage);
                tabPage.ImageKey = imageKey;
            }

            private bool IsValidIndex(int index)
            {
                return ((index >= 0) && (index < this.Count));
            }

            public void Remove(TabPage value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.owner.Controls.Remove(value);
            }

            public void RemoveAt(int index)
            {
                this.owner.Controls.RemoveAt(index);
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
                    Array.Copy(this.owner.GetTabPages(), 0, dest, index, this.Count);
                }
            }

            int IList.Add(object value)
            {
                if (!(value is TabPage))
                {
                    throw new ArgumentException("value");
                }
                this.Add((TabPage) value);
                return this.IndexOf((TabPage) value);
            }

            bool IList.Contains(object page)
            {
                return ((page is TabPage) && this.Contains((TabPage) page));
            }

            int IList.IndexOf(object page)
            {
                if (page is TabPage)
                {
                    return this.IndexOf((TabPage) page);
                }
                return -1;
            }

            void IList.Insert(int index, object tabPage)
            {
                if (!(tabPage is TabPage))
                {
                    throw new ArgumentException("tabPage");
                }
                this.Insert(index, (TabPage) tabPage);
            }

            void IList.Remove(object value)
            {
                if (value is TabPage)
                {
                    this.Remove((TabPage) value);
                }
            }

            [Browsable(false)]
            public int Count
            {
                get
                {
                    return this.owner.tabPageCount;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public virtual TabPage this[int index]
            {
                get
                {
                    return this.owner.GetTabPage(index);
                }
                set
                {
                    this.owner.SetTabPage(index, value, value.GetTCITEM());
                }
            }

            public virtual TabPage this[string key]
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
                    if (!(value is TabPage))
                    {
                        throw new ArgumentException("value");
                    }
                    this[index] = (TabPage) value;
                }
            }
        }
    }
}

