namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    [DefaultEvent("Click"), DesignTimeVisible(false), DefaultProperty("Text"), ToolboxItem(false)]
    public class MenuItem : System.Windows.Forms.Menu
    {
        private static Hashtable allCreatedMenuItems = new Hashtable();
        private MenuItemData data;
        private int dataVersion;
        private const uint firstUniqueID = 0xc0000000;
        private bool hasHandle;
        private System.Windows.Forms.Menu menu;
        private bool menuItemIsCreated;
        private IntPtr msaaMenuInfoPtr;
        private MenuItem nextLinkedItem;
        private static long nextUniqueID = 0xc0000000L;
        internal const int STATE_BARBREAK = 0x20;
        internal const int STATE_BREAK = 0x40;
        internal const int STATE_CHECKED = 8;
        internal const int STATE_CLONE_MASK = 0x3136b;
        internal const int STATE_DEFAULT = 0x1000;
        internal const int STATE_DISABLED = 3;
        internal const int STATE_HIDDEN = 0x10000;
        internal const int STATE_HILITE = 0x80;
        internal const int STATE_INMDIPOPUP = 0x200;
        internal const int STATE_MDILIST = 0x20000;
        internal const int STATE_OWNERDRAW = 0x100;
        internal const int STATE_RADIOCHECK = 0x200;
        private uint uniqueID;

        [System.Windows.Forms.SRDescription("MenuItemOnClickDescr")]
        public event EventHandler Click;

        [System.Windows.Forms.SRDescription("drawItemEventDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event DrawItemEventHandler DrawItem;

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("measureItemEventDescr")]
        public event MeasureItemEventHandler MeasureItem;

        [System.Windows.Forms.SRDescription("MenuItemOnInitDescr")]
        public event EventHandler Popup;

        [System.Windows.Forms.SRDescription("MenuItemOnSelectDescr")]
        public event EventHandler Select;

        public MenuItem() : this(MenuMerge.Add, 0, System.Windows.Forms.Shortcut.None, null, null, null, null, null)
        {
        }

        public MenuItem(string text) : this(MenuMerge.Add, 0, System.Windows.Forms.Shortcut.None, text, null, null, null, null)
        {
        }

        internal MenuItem(MenuItemData data) : base(null)
        {
            this.msaaMenuInfoPtr = IntPtr.Zero;
            data.AddItem(this);
        }

        public MenuItem(string text, EventHandler onClick) : this(MenuMerge.Add, 0, System.Windows.Forms.Shortcut.None, text, onClick, null, null, null)
        {
        }

        public MenuItem(string text, MenuItem[] items) : this(MenuMerge.Add, 0, System.Windows.Forms.Shortcut.None, text, null, null, null, items)
        {
        }

        public MenuItem(string text, EventHandler onClick, System.Windows.Forms.Shortcut shortcut) : this(MenuMerge.Add, 0, shortcut, text, onClick, null, null, null)
        {
        }

        public MenuItem(MenuMerge mergeType, int mergeOrder, System.Windows.Forms.Shortcut shortcut, string text, EventHandler onClick, EventHandler onPopup, EventHandler onSelect, MenuItem[] items) : base(items)
        {
            this.msaaMenuInfoPtr = IntPtr.Zero;
            new MenuItemData(this, mergeType, mergeOrder, shortcut, true, text, onClick, onPopup, onSelect, null, null);
        }

        internal virtual void _OnInitMenuPopup(EventArgs e)
        {
            this.OnInitMenuPopup(e);
        }

        private IntPtr AllocMsaaMenuInfo()
        {
            this.FreeMsaaMenuInfo();
            this.msaaMenuInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MsaaMenuInfoWithId)));
            int size = IntPtr.Size;
            MsaaMenuInfoWithId structure = new MsaaMenuInfoWithId(this.data.caption, this.uniqueID);
            Marshal.StructureToPtr(structure, this.msaaMenuInfoPtr, false);
            return this.msaaMenuInfoPtr;
        }

        private static void CleanListItems(MenuItem senderMenu)
        {
            for (int i = senderMenu.MenuItems.Count - 1; i >= 0; i--)
            {
                MenuItem item = senderMenu.MenuItems[i];
                if (item.data.UserData is MdiListUserData)
                {
                    item.Dispose();
                }
            }
        }

        public virtual MenuItem CloneMenu()
        {
            MenuItem item = new MenuItem();
            item.CloneMenu(this);
            return item;
        }

        protected void CloneMenu(MenuItem itemSrc)
        {
            base.CloneMenu(itemSrc);
            int state = itemSrc.data.State;
            new MenuItemData(this, itemSrc.MergeType, itemSrc.MergeOrder, itemSrc.Shortcut, itemSrc.ShowShortcut, itemSrc.Text, itemSrc.data.onClick, itemSrc.data.onPopup, itemSrc.data.onSelect, itemSrc.data.onDrawItem, itemSrc.data.onMeasureItem);
            this.data.SetState(state & 0x3136b, true);
        }

        internal virtual void CreateMenuItem()
        {
            if ((this.data.State & 0x10000) == 0)
            {
                System.Windows.Forms.NativeMethods.MENUITEMINFO_T lpmii = this.CreateMenuItemInfo();
                System.Windows.Forms.UnsafeNativeMethods.InsertMenuItem(new HandleRef(this.menu, this.menu.handle), -1, true, lpmii);
                this.hasHandle = lpmii.hSubMenu != IntPtr.Zero;
                this.dataVersion = this.data.version;
                this.menuItemIsCreated = true;
                if (this.RenderIsRightToLeft)
                {
                    this.Menu.UpdateRtl(true);
                }
            }
        }

        private System.Windows.Forms.NativeMethods.MENUITEMINFO_T CreateMenuItemInfo()
        {
            System.Windows.Forms.NativeMethods.MENUITEMINFO_T menuiteminfo_t = new System.Windows.Forms.NativeMethods.MENUITEMINFO_T {
                fMask = 0x37,
                fType = this.data.State & 0x360
            };
            bool flag = false;
            if (this.menu == base.GetMainMenu())
            {
                flag = true;
            }
            if (this.data.caption.Equals("-"))
            {
                if (flag)
                {
                    this.data.caption = " ";
                    menuiteminfo_t.fType |= 0x40;
                }
                else
                {
                    menuiteminfo_t.fType |= 0x800;
                }
            }
            menuiteminfo_t.fState = this.data.State & 0x100b;
            menuiteminfo_t.wID = this.MenuID;
            if (this.IsParent)
            {
                menuiteminfo_t.hSubMenu = base.Handle;
            }
            menuiteminfo_t.hbmpChecked = IntPtr.Zero;
            menuiteminfo_t.hbmpUnchecked = IntPtr.Zero;
            if (this.uniqueID == 0)
            {
                lock (allCreatedMenuItems)
                {
                    this.uniqueID = (uint) Interlocked.Increment(ref nextUniqueID);
                    allCreatedMenuItems.Add(this.uniqueID, new WeakReference(this));
                }
            }
            if (IntPtr.Size == 4)
            {
                if (this.data.OwnerDraw)
                {
                    menuiteminfo_t.dwItemData = this.AllocMsaaMenuInfo();
                }
                else
                {
                    menuiteminfo_t.dwItemData = (IntPtr) this.uniqueID;
                }
            }
            else
            {
                menuiteminfo_t.dwItemData = this.AllocMsaaMenuInfo();
            }
            if ((this.data.showShortcut && (this.data.shortcut != System.Windows.Forms.Shortcut.None)) && (!this.IsParent && !flag))
            {
                menuiteminfo_t.dwTypeData = this.data.caption + "\t" + TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString((Keys) this.data.shortcut);
            }
            else
            {
                menuiteminfo_t.dwTypeData = (this.data.caption.Length == 0) ? " " : this.data.caption;
            }
            menuiteminfo_t.cch = 0;
            return menuiteminfo_t;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.menu != null)
                {
                    this.menu.MenuItems.Remove(this);
                }
                if (this.data != null)
                {
                    this.data.RemoveItem(this);
                }
                lock (allCreatedMenuItems)
                {
                    allCreatedMenuItems.Remove(this.uniqueID);
                }
                this.uniqueID = 0;
            }
            this.FreeMsaaMenuInfo();
            base.Dispose(disposing);
        }

        internal Form[] FindMdiForms()
        {
            Form[] mdiChildren = null;
            MainMenu mainMenu = base.GetMainMenu();
            Form formUnsafe = null;
            if (mainMenu != null)
            {
                formUnsafe = mainMenu.GetFormUnsafe();
            }
            if (formUnsafe != null)
            {
                mdiChildren = formUnsafe.MdiChildren;
            }
            if (mdiChildren == null)
            {
                mdiChildren = new Form[0];
            }
            return mdiChildren;
        }

        private void FreeMsaaMenuInfo()
        {
            if (this.msaaMenuInfoPtr != IntPtr.Zero)
            {
                Marshal.DestroyStructure(this.msaaMenuInfoPtr, typeof(MsaaMenuInfoWithId));
                Marshal.FreeHGlobal(this.msaaMenuInfoPtr);
                this.msaaMenuInfoPtr = IntPtr.Zero;
            }
        }

        internal static MenuItem GetMenuItemFromItemData(IntPtr itemData)
        {
            uint uniqueID = (uint) ((long) itemData);
            if (uniqueID == 0)
            {
                return null;
            }
            if (IntPtr.Size == 4)
            {
                if (uniqueID < 0xc0000000)
                {
                    MsaaMenuInfoWithId id = (MsaaMenuInfoWithId) Marshal.PtrToStructure(itemData, typeof(MsaaMenuInfoWithId));
                    uniqueID = id.uniqueID;
                }
            }
            else
            {
                MsaaMenuInfoWithId id2 = (MsaaMenuInfoWithId) Marshal.PtrToStructure(itemData, typeof(MsaaMenuInfoWithId));
                uniqueID = id2.uniqueID;
            }
            return GetMenuItemFromUniqueID(uniqueID);
        }

        internal static MenuItem GetMenuItemFromUniqueID(uint uniqueID)
        {
            WeakReference reference = (WeakReference) allCreatedMenuItems[uniqueID];
            if ((reference != null) && reference.IsAlive)
            {
                return (MenuItem) reference.Target;
            }
            return null;
        }

        internal override void ItemsChanged(int change)
        {
            base.ItemsChanged(change);
            if (change == 0)
            {
                if ((this.menu != null) && this.menu.created)
                {
                    this.UpdateMenuItem(true);
                    base.CreateMenuItems();
                }
            }
            else
            {
                if (!this.hasHandle && this.IsParent)
                {
                    this.UpdateMenuItem(true);
                }
                MainMenu mainMenu = base.GetMainMenu();
                if ((mainMenu != null) && ((this.data.State & 0x200) == 0))
                {
                    mainMenu.ItemsChanged(change, this);
                }
            }
        }

        internal void ItemsChanged(int change, MenuItem item)
        {
            if (((change == 4) && (this.data != null)) && ((this.data.baseItem != null) && this.data.baseItem.MenuItems.Contains(item)))
            {
                if ((this.menu != null) && this.menu.created)
                {
                    this.UpdateMenuItem(true);
                    base.CreateMenuItems();
                }
                else if (this.data != null)
                {
                    for (MenuItem item2 = this.data.firstItem; item2 != null; item2 = item2.nextLinkedItem)
                    {
                        if (item2.created)
                        {
                            MenuItem item3 = item.CloneMenu();
                            item.data.AddItem(item3);
                            item2.MenuItems.Add(item3);
                            return;
                        }
                    }
                }
            }
        }

        public virtual MenuItem MergeMenu()
        {
            MenuItem item = (MenuItem) Activator.CreateInstance(base.GetType());
            this.data.AddItem(item);
            item.MergeMenu(this);
            return item;
        }

        public void MergeMenu(MenuItem itemSrc)
        {
            base.MergeMenu(itemSrc);
            itemSrc.data.AddItem(this);
        }

        protected virtual void OnClick(EventArgs e)
        {
            if (this.data.UserData is MdiListUserData)
            {
                ((MdiListUserData) this.data.UserData).OnClick(e);
            }
            else if (this.data.baseItem != this)
            {
                this.data.baseItem.OnClick(e);
            }
            else if (this.data.onClick != null)
            {
                this.data.onClick(this, e);
            }
        }

        protected virtual void OnDrawItem(DrawItemEventArgs e)
        {
            if (this.data.baseItem != this)
            {
                this.data.baseItem.OnDrawItem(e);
            }
            else if (this.data.onDrawItem != null)
            {
                this.data.onDrawItem(this, e);
            }
        }

        protected virtual void OnInitMenuPopup(EventArgs e)
        {
            this.OnPopup(e);
        }

        protected virtual void OnMeasureItem(MeasureItemEventArgs e)
        {
            if (this.data.baseItem != this)
            {
                this.data.baseItem.OnMeasureItem(e);
            }
            else if (this.data.onMeasureItem != null)
            {
                this.data.onMeasureItem(this, e);
            }
        }

        protected virtual void OnPopup(EventArgs e)
        {
            bool flag = false;
            for (int i = 0; i < base.ItemCount; i++)
            {
                if (base.items[i].MdiList)
                {
                    flag = true;
                    base.items[i].UpdateMenuItem(true);
                }
            }
            if (flag || (this.hasHandle && !this.IsParent))
            {
                this.UpdateMenuItem(true);
            }
            if (this.data.baseItem != this)
            {
                this.data.baseItem.OnPopup(e);
            }
            else if (this.data.onPopup != null)
            {
                this.data.onPopup(this, e);
            }
            for (int j = 0; j < base.ItemCount; j++)
            {
                base.items[j].UpdateMenuItemIfDirty();
            }
            if (this.MdiList)
            {
                this.PopulateMdiList();
            }
        }

        protected virtual void OnSelect(EventArgs e)
        {
            if (this.data.baseItem != this)
            {
                this.data.baseItem.OnSelect(e);
            }
            else if (this.data.onSelect != null)
            {
                this.data.onSelect(this, e);
            }
        }

        public void PerformClick()
        {
            this.OnClick(EventArgs.Empty);
        }

        public virtual void PerformSelect()
        {
            this.OnSelect(EventArgs.Empty);
        }

        private void PopulateMdiList()
        {
            MenuItem item = this;
            this.data.SetState(0x200, true);
            try
            {
                CleanListItems(this);
                Form[] formArray = this.FindMdiForms();
                if ((formArray != null) && (formArray.Length > 0))
                {
                    Form activeMdiChild = base.GetMainMenu().GetFormUnsafe().ActiveMdiChild;
                    if (item.MenuItems.Count > 0)
                    {
                        MenuItem item2 = (MenuItem) Activator.CreateInstance(base.GetType());
                        item2.data.UserData = new MdiListUserData();
                        item2.Text = "-";
                        item.MenuItems.Add(item2);
                    }
                    int num = 0;
                    int num2 = 1;
                    int num3 = 0;
                    bool flag = false;
                    for (int i = 0; i < formArray.Length; i++)
                    {
                        if (formArray[i].Visible)
                        {
                            num++;
                            if (((flag && (num3 < 9)) || (!flag && (num3 < 8))) || formArray[i].Equals(activeMdiChild))
                            {
                                MenuItem item3 = (MenuItem) Activator.CreateInstance(base.GetType());
                                item3.data.UserData = new MdiListFormData(this, i);
                                if (formArray[i].Equals(activeMdiChild))
                                {
                                    item3.Checked = true;
                                    flag = true;
                                }
                                item3.Text = string.Format(CultureInfo.CurrentUICulture, "&{0} {1}", new object[] { num2, formArray[i].Text });
                                num2++;
                                num3++;
                                item.MenuItems.Add(item3);
                            }
                        }
                    }
                    if (num > 9)
                    {
                        MenuItem item4 = (MenuItem) Activator.CreateInstance(base.GetType());
                        item4.data.UserData = new MdiListMoreWindowsData(this);
                        item4.Text = System.Windows.Forms.SR.GetString("MDIMenuMoreWindows");
                        item.MenuItems.Add(item4);
                    }
                }
            }
            finally
            {
                this.data.SetState(0x200, false);
            }
        }

        internal virtual bool ShortcutClick()
        {
            if (this.menu is MenuItem)
            {
                MenuItem menu = (MenuItem) this.menu;
                if (!menu.ShortcutClick() || (this.menu != menu))
                {
                    return false;
                }
            }
            if ((this.data.State & 3) != 0)
            {
                return false;
            }
            if (base.ItemCount > 0)
            {
                this.OnPopup(EventArgs.Empty);
            }
            else
            {
                this.OnClick(EventArgs.Empty);
            }
            return true;
        }

        public override string ToString()
        {
            string str = base.ToString();
            string caption = string.Empty;
            if ((this.data != null) && (this.data.caption != null))
            {
                caption = this.data.caption;
            }
            return (str + ", Text: " + caption);
        }

        internal void UpdateItemRtl(bool setRightToLeftBit)
        {
            if (this.menuItemIsCreated)
            {
                System.Windows.Forms.NativeMethods.MENUITEMINFO_T menuiteminfo_t;
                menuiteminfo_t = new System.Windows.Forms.NativeMethods.MENUITEMINFO_T {
                    fMask = 0x15,
                    dwTypeData = new string('\0', this.Text.Length + 2),
                    cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.MENUITEMINFO_T)),
                    cch = menuiteminfo_t.dwTypeData.Length - 1
                };
                System.Windows.Forms.UnsafeNativeMethods.GetMenuItemInfo(new HandleRef(this.menu, this.menu.handle), this.MenuID, false, menuiteminfo_t);
                if (setRightToLeftBit)
                {
                    menuiteminfo_t.fType |= 0x6000;
                }
                else
                {
                    menuiteminfo_t.fType &= -24577;
                }
                System.Windows.Forms.UnsafeNativeMethods.SetMenuItemInfo(new HandleRef(this.menu, this.menu.handle), this.MenuID, false, menuiteminfo_t);
            }
        }

        internal void UpdateMenuItem(bool force)
        {
            if (((this.menu != null) && this.menu.created) && ((force || (this.menu is MainMenu)) || (this.menu is ContextMenu)))
            {
                System.Windows.Forms.NativeMethods.MENUITEMINFO_T lpmii = this.CreateMenuItemInfo();
                System.Windows.Forms.UnsafeNativeMethods.SetMenuItemInfo(new HandleRef(this.menu, this.menu.handle), this.MenuID, false, lpmii);
                if (this.hasHandle && (lpmii.hSubMenu == IntPtr.Zero))
                {
                    base.ClearHandles();
                }
                this.hasHandle = lpmii.hSubMenu != IntPtr.Zero;
                this.dataVersion = this.data.version;
                if (this.menu is MainMenu)
                {
                    Form formUnsafe = ((MainMenu) this.menu).GetFormUnsafe();
                    if (formUnsafe != null)
                    {
                        System.Windows.Forms.SafeNativeMethods.DrawMenuBar(new HandleRef(formUnsafe, formUnsafe.Handle));
                    }
                }
            }
        }

        internal void UpdateMenuItemIfDirty()
        {
            if (this.dataVersion != this.data.version)
            {
                this.UpdateMenuItem(true);
            }
        }

        internal void WmDrawItem(ref Message m)
        {
            System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT lParam = (System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT));
            IntPtr handle = Control.SetUpPalette(lParam.hDC, false, false);
            try
            {
                using (Graphics graphics = Graphics.FromHdcInternal(lParam.hDC))
                {
                    this.OnDrawItem(new DrawItemEventArgs(graphics, SystemInformation.MenuFont, Rectangle.FromLTRB(lParam.rcItem.left, lParam.rcItem.top, lParam.rcItem.right, lParam.rcItem.bottom), this.Index, (DrawItemState) lParam.itemState));
                }
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    System.Windows.Forms.SafeNativeMethods.SelectPalette(new HandleRef(null, lParam.hDC), new HandleRef(null, handle), 0);
                }
            }
            m.Result = (IntPtr) 1;
        }

        internal void WmMeasureItem(ref Message m)
        {
            System.Windows.Forms.NativeMethods.MEASUREITEMSTRUCT lParam = (System.Windows.Forms.NativeMethods.MEASUREITEMSTRUCT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.MEASUREITEMSTRUCT));
            IntPtr dC = System.Windows.Forms.UnsafeNativeMethods.GetDC(System.Windows.Forms.NativeMethods.NullHandleRef);
            Graphics graphics = Graphics.FromHdcInternal(dC);
            MeasureItemEventArgs e = new MeasureItemEventArgs(graphics, this.Index);
            try
            {
                this.OnMeasureItem(e);
            }
            finally
            {
                graphics.Dispose();
            }
            System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(System.Windows.Forms.NativeMethods.NullHandleRef, new HandleRef(null, dC));
            lParam.itemHeight = e.ItemHeight;
            lParam.itemWidth = e.ItemWidth;
            Marshal.StructureToPtr(lParam, m.LParam, false);
            m.Result = (IntPtr) 1;
        }

        [Browsable(false), DefaultValue(false)]
        public bool BarBreak
        {
            get
            {
                return ((this.data.State & 0x20) != 0);
            }
            set
            {
                this.data.SetState(0x20, value);
            }
        }

        [DefaultValue(false), Browsable(false)]
        public bool Break
        {
            get
            {
                return ((this.data.State & 0x40) != 0);
            }
            set
            {
                this.data.SetState(0x40, value);
            }
        }

        [System.Windows.Forms.SRDescription("MenuItemCheckedDescr"), DefaultValue(false)]
        public bool Checked
        {
            get
            {
                return ((this.data.State & 8) != 0);
            }
            set
            {
                if (value && ((base.ItemCount != 0) || ((this.Parent != null) && (this.Parent is MainMenu))))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("MenuItemInvalidCheckProperty"));
                }
                this.data.SetState(8, value);
            }
        }

        [System.Windows.Forms.SRDescription("MenuItemDefaultDescr"), DefaultValue(false)]
        public bool DefaultItem
        {
            get
            {
                return ((this.data.State & 0x1000) != 0);
            }
            set
            {
                if (this.menu != null)
                {
                    if (value)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetMenuDefaultItem(new HandleRef(this.menu, this.menu.handle), this.MenuID, false);
                    }
                    else if (this.DefaultItem)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetMenuDefaultItem(new HandleRef(this.menu, this.menu.handle), -1, false);
                    }
                }
                this.data.SetState(0x1000, value);
            }
        }

        [System.Windows.Forms.SRDescription("MenuItemEnabledDescr"), Localizable(true), DefaultValue(true)]
        public bool Enabled
        {
            get
            {
                return ((this.data.State & 3) == 0);
            }
            set
            {
                this.data.SetState(3, !value);
            }
        }

        [Browsable(false)]
        public int Index
        {
            get
            {
                if (this.menu != null)
                {
                    for (int i = 0; i < this.menu.ItemCount; i++)
                    {
                        if (this.menu.items[i] == this)
                        {
                            return i;
                        }
                    }
                }
                return -1;
            }
            set
            {
                int index = this.Index;
                if (index >= 0)
                {
                    if ((value < 0) || (value >= this.menu.ItemCount))
                    {
                        throw new ArgumentOutOfRangeException("Index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "Index", value.ToString(CultureInfo.CurrentCulture) }));
                    }
                    if (value != index)
                    {
                        System.Windows.Forms.Menu menu = this.menu;
                        menu.MenuItems.RemoveAt(index);
                        menu.MenuItems.Add(value, this);
                    }
                }
            }
        }

        [Browsable(false)]
        public override bool IsParent
        {
            get
            {
                bool flag = false;
                if ((this.data == null) || !this.MdiList)
                {
                    return base.IsParent;
                }
                for (int i = 0; i < base.ItemCount; i++)
                {
                    if (!(base.items[i].data.UserData is MdiListUserData))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag && (this.FindMdiForms().Length > 0))
                {
                    flag = true;
                }
                if ((!flag && (this.menu != null)) && !(this.menu is MenuItem))
                {
                    flag = true;
                }
                return flag;
            }
        }

        [System.Windows.Forms.SRDescription("MenuItemMDIListDescr"), DefaultValue(false)]
        public bool MdiList
        {
            get
            {
                return ((this.data.State & 0x20000) != 0);
            }
            set
            {
                this.data.MdiList = value;
                CleanListItems(this);
            }
        }

        internal System.Windows.Forms.Menu Menu
        {
            get
            {
                return this.menu;
            }
            set
            {
                this.menu = value;
            }
        }

        protected int MenuID
        {
            get
            {
                return this.data.GetMenuID();
            }
        }

        internal int MenuIndex
        {
            get
            {
                if (this.menu != null)
                {
                    int menuItemCount = System.Windows.Forms.UnsafeNativeMethods.GetMenuItemCount(new HandleRef(this.menu, this.menu.Handle));
                    int menuID = this.MenuID;
                    System.Windows.Forms.NativeMethods.MENUITEMINFO_T lpmii = new System.Windows.Forms.NativeMethods.MENUITEMINFO_T {
                        cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.MENUITEMINFO_T)),
                        fMask = 6
                    };
                    for (int i = 0; i < menuItemCount; i++)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.GetMenuItemInfo(new HandleRef(this.menu, this.menu.handle), i, true, lpmii);
                        if (((lpmii.hSubMenu == IntPtr.Zero) || (lpmii.hSubMenu == base.Handle)) && (lpmii.wID == menuID))
                        {
                            return i;
                        }
                    }
                }
                return -1;
            }
        }

        [System.Windows.Forms.SRDescription("MenuItemMergeOrderDescr"), DefaultValue(0)]
        public int MergeOrder
        {
            get
            {
                return this.data.mergeOrder;
            }
            set
            {
                this.data.MergeOrder = value;
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRDescription("MenuItemMergeTypeDescr")]
        public MenuMerge MergeType
        {
            get
            {
                return this.data.mergeType;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(MenuMerge));
                }
                this.data.MergeType = value;
            }
        }

        [Browsable(false)]
        public char Mnemonic
        {
            get
            {
                return this.data.Mnemonic;
            }
        }

        [System.Windows.Forms.SRDescription("MenuItemOwnerDrawDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool OwnerDraw
        {
            get
            {
                return ((this.data.State & 0x100) != 0);
            }
            set
            {
                this.data.SetState(0x100, value);
            }
        }

        [Browsable(false)]
        public System.Windows.Forms.Menu Parent
        {
            get
            {
                return this.menu;
            }
        }

        [System.Windows.Forms.SRDescription("MenuItemRadioCheckDescr"), DefaultValue(false)]
        public bool RadioCheck
        {
            get
            {
                return ((this.data.State & 0x200) != 0);
            }
            set
            {
                this.data.SetState(0x200, value);
            }
        }

        internal override bool RenderIsRightToLeft
        {
            get
            {
                if (this.Parent == null)
                {
                    return false;
                }
                return this.Parent.RenderIsRightToLeft;
            }
        }

        internal bool Selected
        {
            get
            {
                if (this.menu == null)
                {
                    return false;
                }
                System.Windows.Forms.NativeMethods.MENUITEMINFO_T lpmii = new System.Windows.Forms.NativeMethods.MENUITEMINFO_T {
                    cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.MENUITEMINFO_T)),
                    fMask = 1
                };
                System.Windows.Forms.UnsafeNativeMethods.GetMenuItemInfo(new HandleRef(this.menu, this.menu.handle), this.MenuID, false, lpmii);
                return ((lpmii.fState & 0x80) != 0);
            }
        }

        [System.Windows.Forms.SRDescription("MenuItemShortCutDescr"), Localizable(true), DefaultValue(0)]
        public System.Windows.Forms.Shortcut Shortcut
        {
            get
            {
                return this.data.shortcut;
            }
            set
            {
                if (!Enum.IsDefined(typeof(System.Windows.Forms.Shortcut), value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.Shortcut));
                }
                this.data.shortcut = value;
                this.UpdateMenuItem(true);
            }
        }

        [Localizable(true), DefaultValue(true), System.Windows.Forms.SRDescription("MenuItemShowShortCutDescr")]
        public bool ShowShortcut
        {
            get
            {
                return this.data.showShortcut;
            }
            set
            {
                if (value != this.data.showShortcut)
                {
                    this.data.showShortcut = value;
                    this.UpdateMenuItem(true);
                }
            }
        }

        [System.Windows.Forms.SRDescription("MenuItemTextDescr"), Localizable(true)]
        public string Text
        {
            get
            {
                return this.data.caption;
            }
            set
            {
                this.data.SetCaption(value);
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("MenuItemVisibleDescr"), DefaultValue(true)]
        public bool Visible
        {
            get
            {
                return ((this.data.State & 0x10000) == 0);
            }
            set
            {
                this.data.Visible = value;
            }
        }

        private class MdiListFormData : MenuItem.MdiListUserData
        {
            private int boundIndex;
            private MenuItem parent;

            public MdiListFormData(MenuItem parentItem, int boundFormIndex)
            {
                this.boundIndex = boundFormIndex;
                this.parent = parentItem;
            }

            public override void OnClick(EventArgs e)
            {
                if (this.boundIndex != -1)
                {
                    System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                    try
                    {
                        Form[] formArray = this.parent.FindMdiForms();
                        if ((formArray != null) && (formArray.Length > this.boundIndex))
                        {
                            Form form = formArray[this.boundIndex];
                            form.Activate();
                            if ((form.ActiveControl != null) && !form.ActiveControl.Focused)
                            {
                                form.ActiveControl.Focus();
                            }
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
        }

        private class MdiListMoreWindowsData : MenuItem.MdiListUserData
        {
            private MenuItem parent;

            public MdiListMoreWindowsData(MenuItem parent)
            {
                this.parent = parent;
            }

            public override void OnClick(EventArgs e)
            {
                Form[] all = this.parent.FindMdiForms();
                Form activeMdiChild = this.parent.GetMainMenu().GetFormUnsafe().ActiveMdiChild;
                if (((all != null) && (all.Length > 0)) && (activeMdiChild != null))
                {
                    System.Windows.Forms.IntSecurity.AllWindows.Assert();
                    try
                    {
                        using (MdiWindowDialog dialog = new MdiWindowDialog())
                        {
                            dialog.SetItems(activeMdiChild, all);
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                dialog.ActiveChildForm.Activate();
                                if ((dialog.ActiveChildForm.ActiveControl != null) && !dialog.ActiveChildForm.ActiveControl.Focused)
                                {
                                    dialog.ActiveChildForm.ActiveControl.Focus();
                                }
                            }
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
        }

        private class MdiListUserData
        {
            public virtual void OnClick(EventArgs e)
            {
            }
        }

        internal class MenuItemData : ICommandExecutor
        {
            internal MenuItem baseItem;
            internal string caption;
            internal Command cmd;
            internal MenuItem firstItem;
            internal int mergeOrder;
            internal MenuMerge mergeType;
            internal short mnemonic;
            internal EventHandler onClick;
            internal DrawItemEventHandler onDrawItem;
            internal MeasureItemEventHandler onMeasureItem;
            internal EventHandler onPopup;
            internal EventHandler onSelect;
            internal Shortcut shortcut;
            internal bool showShortcut;
            private int state;
            private object userData;
            internal int version;

            internal MenuItemData(MenuItem baseItem, MenuMerge mergeType, int mergeOrder, Shortcut shortcut, bool showShortcut, string caption, EventHandler onClick, EventHandler onPopup, EventHandler onSelect, DrawItemEventHandler onDrawItem, MeasureItemEventHandler onMeasureItem)
            {
                this.AddItem(baseItem);
                this.mergeType = mergeType;
                this.mergeOrder = mergeOrder;
                this.shortcut = shortcut;
                this.showShortcut = showShortcut;
                this.caption = (caption == null) ? "" : caption;
                this.onClick = onClick;
                this.onPopup = onPopup;
                this.onSelect = onSelect;
                this.onDrawItem = onDrawItem;
                this.onMeasureItem = onMeasureItem;
                this.version = 1;
                this.mnemonic = -1;
            }

            internal void AddItem(MenuItem item)
            {
                if (item.data != this)
                {
                    if (item.data != null)
                    {
                        item.data.RemoveItem(item);
                    }
                    item.nextLinkedItem = this.firstItem;
                    this.firstItem = item;
                    if (this.baseItem == null)
                    {
                        this.baseItem = item;
                    }
                    item.data = this;
                    item.dataVersion = 0;
                    item.UpdateMenuItem(false);
                }
            }

            public void Execute()
            {
                if (this.baseItem != null)
                {
                    this.baseItem.OnClick(EventArgs.Empty);
                }
            }

            internal int GetMenuID()
            {
                if (this.cmd == null)
                {
                    this.cmd = new Command(this);
                }
                return this.cmd.ID;
            }

            internal bool HasState(int flag)
            {
                return ((this.State & flag) == flag);
            }

            internal void ItemsChanged(int change)
            {
                for (MenuItem item = this.firstItem; item != null; item = item.nextLinkedItem)
                {
                    if (item.menu != null)
                    {
                        item.menu.ItemsChanged(change);
                    }
                }
            }

            internal void RemoveItem(MenuItem item)
            {
                if (item == this.firstItem)
                {
                    this.firstItem = item.nextLinkedItem;
                }
                else
                {
                    MenuItem firstItem = this.firstItem;
                    while (item != firstItem.nextLinkedItem)
                    {
                        firstItem = firstItem.nextLinkedItem;
                    }
                    firstItem.nextLinkedItem = item.nextLinkedItem;
                }
                item.nextLinkedItem = null;
                item.data = null;
                item.dataVersion = 0;
                if (item == this.baseItem)
                {
                    this.baseItem = this.firstItem;
                }
                if (this.firstItem == null)
                {
                    this.onClick = null;
                    this.onPopup = null;
                    this.onSelect = null;
                    this.onDrawItem = null;
                    this.onMeasureItem = null;
                    if (this.cmd != null)
                    {
                        this.cmd.Dispose();
                        this.cmd = null;
                    }
                }
            }

            internal void SetCaption(string value)
            {
                if (value == null)
                {
                    value = "";
                }
                if (!this.caption.Equals(value))
                {
                    this.caption = value;
                    this.UpdateMenuItems();
                }
            }

            internal void SetState(int flag, bool value)
            {
                if (((this.state & flag) != 0) != value)
                {
                    this.state = value ? (this.state | flag) : (this.state & ~flag);
                    this.UpdateMenuItems();
                }
            }

            internal void UpdateMenuItems()
            {
                this.version++;
                for (MenuItem item = this.firstItem; item != null; item = item.nextLinkedItem)
                {
                    item.UpdateMenuItem(true);
                }
            }

            internal bool MdiList
            {
                get
                {
                    return this.HasState(0x20000);
                }
                set
                {
                    if (((this.state & 0x20000) != 0) != value)
                    {
                        this.SetState(0x20000, value);
                        for (MenuItem item = this.firstItem; item != null; item = item.nextLinkedItem)
                        {
                            item.ItemsChanged(2);
                        }
                    }
                }
            }

            internal int MergeOrder
            {
                get
                {
                    return this.mergeOrder;
                }
                set
                {
                    if (this.mergeOrder != value)
                    {
                        this.mergeOrder = value;
                        this.ItemsChanged(3);
                    }
                }
            }

            internal MenuMerge MergeType
            {
                get
                {
                    return this.mergeType;
                }
                set
                {
                    if (this.mergeType != value)
                    {
                        this.mergeType = value;
                        this.ItemsChanged(3);
                    }
                }
            }

            internal char Mnemonic
            {
                get
                {
                    if (this.mnemonic == -1)
                    {
                        this.mnemonic = (short) WindowsFormsUtils.GetMnemonic(this.caption, true);
                    }
                    return (char) ((ushort) this.mnemonic);
                }
            }

            internal bool OwnerDraw
            {
                get
                {
                    return ((this.State & 0x100) != 0);
                }
                set
                {
                    this.SetState(0x100, value);
                }
            }

            internal int State
            {
                get
                {
                    return this.state;
                }
            }

            internal object UserData
            {
                get
                {
                    return this.userData;
                }
                set
                {
                    this.userData = value;
                }
            }

            internal bool Visible
            {
                get
                {
                    return ((this.state & 0x10000) == 0);
                }
                set
                {
                    if (((this.state & 0x10000) == 0) != value)
                    {
                        this.state = value ? (this.state & -65537) : (this.state | 0x10000);
                        this.ItemsChanged(1);
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MsaaMenuInfoWithId
        {
            public System.Windows.Forms.NativeMethods.MSAAMENUINFO msaaMenuInfo;
            public uint uniqueID;
            public MsaaMenuInfoWithId(string text, uint uniqueID)
            {
                this.msaaMenuInfo = new System.Windows.Forms.NativeMethods.MSAAMENUINFO(text);
                this.uniqueID = uniqueID;
            }
        }
    }
}

