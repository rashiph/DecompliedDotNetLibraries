namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ToolboxItemFilter("System.Windows.Forms"), ListBindable(false)]
    public abstract class Menu : Component
    {
        private int _itemCount;
        internal const int CHANGE_ITEMADDED = 4;
        internal const int CHANGE_ITEMS = 0;
        internal const int CHANGE_MDI = 2;
        internal const int CHANGE_MERGE = 3;
        internal const int CHANGE_VISIBLE = 1;
        internal bool created;
        public const int FindHandle = 0;
        public const int FindShortcut = 1;
        internal IntPtr handle;
        internal MenuItem[] items;
        private MenuItemCollection itemsCollection;
        private string name;
        private object userData;

        protected Menu(MenuItem[] items)
        {
            if (items != null)
            {
                this.MenuItems.AddRange(items);
            }
        }

        private bool CheckOwnerDrawItemNoMnemonic(MenuItem mi, char key)
        {
            return (((mi.OwnerDraw && (mi.Mnemonic == '\0')) && (mi.Text.Length > 0)) && (char.ToUpper(mi.Text[0], CultureInfo.CurrentCulture) == key));
        }

        private bool CheckOwnerDrawItemWithMnemonic(MenuItem mi, char key)
        {
            return (mi.OwnerDraw && (mi.Mnemonic == key));
        }

        internal void ClearHandles()
        {
            if (this.handle != IntPtr.Zero)
            {
                System.Windows.Forms.UnsafeNativeMethods.DestroyMenu(new HandleRef(this, this.handle));
            }
            this.handle = IntPtr.Zero;
            if (this.created)
            {
                for (int i = 0; i < this.ItemCount; i++)
                {
                    this.items[i].ClearHandles();
                }
                this.created = false;
            }
        }

        protected internal void CloneMenu(Menu menuSrc)
        {
            MenuItem[] items = null;
            if (menuSrc.items != null)
            {
                int count = menuSrc.MenuItems.Count;
                items = new MenuItem[count];
                for (int i = 0; i < count; i++)
                {
                    items[i] = menuSrc.MenuItems[i].CloneMenu();
                }
            }
            this.MenuItems.Clear();
            if (items != null)
            {
                this.MenuItems.AddRange(items);
            }
        }

        [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected virtual IntPtr CreateMenuHandle()
        {
            return System.Windows.Forms.UnsafeNativeMethods.CreatePopupMenu();
        }

        internal void CreateMenuItems()
        {
            if (!this.created)
            {
                for (int i = 0; i < this.ItemCount; i++)
                {
                    this.items[i].CreateMenuItem();
                }
                this.created = true;
            }
        }

        internal void DestroyMenuItems()
        {
            if (this.created)
            {
                for (int i = 0; i < this.ItemCount; i++)
                {
                    this.items[i].ClearHandles();
                }
                while (System.Windows.Forms.UnsafeNativeMethods.GetMenuItemCount(new HandleRef(this, this.handle)) > 0)
                {
                    System.Windows.Forms.UnsafeNativeMethods.RemoveMenu(new HandleRef(this, this.handle), 0, 0x400);
                }
                this.created = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                while (this.ItemCount > 0)
                {
                    MenuItem component = this.items[--this._itemCount];
                    if ((component.Site != null) && (component.Site.Container != null))
                    {
                        component.Site.Container.Remove(component);
                    }
                    component.Menu = null;
                    component.Dispose();
                }
                this.items = null;
            }
            if (this.handle != IntPtr.Zero)
            {
                System.Windows.Forms.UnsafeNativeMethods.DestroyMenu(new HandleRef(this, this.handle));
                this.handle = IntPtr.Zero;
                if (disposing)
                {
                    this.ClearHandles();
                }
            }
            base.Dispose(disposing);
        }

        public MenuItem FindMenuItem(int type, IntPtr value)
        {
            System.Windows.Forms.IntSecurity.ControlFromHandleOrLocation.Demand();
            return this.FindMenuItemInternal(type, value);
        }

        private MenuItem FindMenuItemInternal(int type, IntPtr value)
        {
            for (int i = 0; i < this.ItemCount; i++)
            {
                MenuItem item = this.items[i];
                switch (type)
                {
                    case 0:
                        if (!(item.handle == value))
                        {
                            break;
                        }
                        return item;

                    case 1:
                        if (item.Shortcut != ((int) value))
                        {
                            break;
                        }
                        return item;
                }
                item = item.FindMenuItemInternal(type, value);
                if (item != null)
                {
                    return item;
                }
            }
            return null;
        }

        protected int FindMergePosition(int mergeOrder)
        {
            int num = 0;
            int itemCount = this.ItemCount;
            while (num < itemCount)
            {
                int index = (num + itemCount) / 2;
                if (this.items[index].MergeOrder <= mergeOrder)
                {
                    num = index + 1;
                }
                else
                {
                    itemCount = index;
                }
            }
            return num;
        }

        public ContextMenu GetContextMenu()
        {
            Menu menu = this;
            while (!(menu is ContextMenu))
            {
                if (!(menu is MenuItem))
                {
                    return null;
                }
                menu = ((MenuItem) menu).Menu;
            }
            return (ContextMenu) menu;
        }

        public MainMenu GetMainMenu()
        {
            Menu menu = this;
            while (!(menu is MainMenu))
            {
                if (!(menu is MenuItem))
                {
                    return null;
                }
                menu = ((MenuItem) menu).Menu;
            }
            return (MainMenu) menu;
        }

        internal virtual void ItemsChanged(int change)
        {
            switch (change)
            {
                case 0:
                case 1:
                    this.DestroyMenuItems();
                    return;
            }
        }

        private IntPtr MatchKeyToMenuItem(int startItem, char key, MenuItemKeyComparer comparer)
        {
            int low = -1;
            bool flag = false;
            for (int i = 0; (i < this.items.Length) && !flag; i++)
            {
                int index = (startItem + i) % this.items.Length;
                MenuItem mi = this.items[index];
                if ((mi != null) && comparer(mi, key))
                {
                    if (low < 0)
                    {
                        low = mi.MenuIndex;
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            if (low < 0)
            {
                return IntPtr.Zero;
            }
            int high = flag ? 3 : 2;
            return (IntPtr) System.Windows.Forms.NativeMethods.Util.MAKELONG(low, high);
        }

        public virtual void MergeMenu(Menu menuSrc)
        {
            if (menuSrc == this)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("MenuMergeWithSelf"), "menuSrc");
            }
            if ((menuSrc.items != null) && (this.items == null))
            {
                this.MenuItems.Clear();
            }
            for (int i = 0; i < menuSrc.ItemCount; i++)
            {
                MenuItem itemSrc = menuSrc.items[i];
                switch (itemSrc.MergeType)
                {
                    case MenuMerge.Add:
                        this.MenuItems.Add(this.FindMergePosition(itemSrc.MergeOrder), itemSrc.MergeMenu());
                        break;

                    case MenuMerge.Replace:
                    case MenuMerge.MergeItems:
                    {
                        int mergeOrder = itemSrc.MergeOrder;
                        int index = this.xFindMergePosition(mergeOrder);
                        while (true)
                        {
                            if (index >= this.ItemCount)
                            {
                                this.MenuItems.Add(index, itemSrc.MergeMenu());
                                break;
                            }
                            MenuItem item2 = this.items[index];
                            if (item2.MergeOrder != mergeOrder)
                            {
                                this.MenuItems.Add(index, itemSrc.MergeMenu());
                                break;
                            }
                            if (item2.MergeType != MenuMerge.Add)
                            {
                                if ((itemSrc.MergeType != MenuMerge.MergeItems) || (item2.MergeType != MenuMerge.MergeItems))
                                {
                                    item2.Dispose();
                                    this.MenuItems.Add(index, itemSrc.MergeMenu());
                                }
                                else
                                {
                                    item2.MergeMenu(itemSrc);
                                }
                                break;
                            }
                            index++;
                        }
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected internal virtual bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            MenuItem item = this.FindMenuItemInternal(1, (IntPtr) keyData);
            if (item == null)
            {
                return false;
            }
            return item.ShortcutClick();
        }

        internal virtual bool ProcessInitMenuPopup(IntPtr handle)
        {
            MenuItem item = this.FindMenuItemInternal(0, handle);
            if (item != null)
            {
                item._OnInitMenuPopup(EventArgs.Empty);
                item.CreateMenuItems();
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return (base.ToString() + ", Items.Count: " + this.ItemCount.ToString(CultureInfo.CurrentCulture));
        }

        internal void UpdateRtl(bool setRightToLeftBit)
        {
            foreach (MenuItem item in this.MenuItems)
            {
                item.UpdateItemRtl(setRightToLeftBit);
                item.UpdateRtl(setRightToLeftBit);
            }
        }

        internal void WmMenuChar(ref Message m)
        {
            Menu menu = (m.LParam == this.handle) ? this : this.FindMenuItemInternal(0, m.LParam);
            if (menu != null)
            {
                char key = char.ToUpper((char) System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam), CultureInfo.CurrentCulture);
                m.Result = menu.WmMenuCharInternal(key);
            }
        }

        internal IntPtr WmMenuCharInternal(char key)
        {
            int startItem = (this.SelectedMenuItemIndex + 1) % this.items.Length;
            IntPtr ptr = this.MatchKeyToMenuItem(startItem, key, new MenuItemKeyComparer(this.CheckOwnerDrawItemWithMnemonic));
            if (ptr == IntPtr.Zero)
            {
                ptr = this.MatchKeyToMenuItem(startItem, key, new MenuItemKeyComparer(this.CheckOwnerDrawItemNoMnemonic));
            }
            return ptr;
        }

        internal int xFindMergePosition(int mergeOrder)
        {
            int num = 0;
            for (int i = 0; i < this.ItemCount; i++)
            {
                if (this.items[i].MergeOrder > mergeOrder)
                {
                    return num;
                }
                if (this.items[i].MergeOrder < mergeOrder)
                {
                    num = i + 1;
                }
                else if (mergeOrder == this.items[i].MergeOrder)
                {
                    return i;
                }
            }
            return num;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlHandleDescr"), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public IntPtr Handle
        {
            get
            {
                if (this.handle == IntPtr.Zero)
                {
                    this.handle = this.CreateMenuHandle();
                }
                this.CreateMenuItems();
                return this.handle;
            }
        }

        [System.Windows.Forms.SRDescription("MenuIsParentDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual bool IsParent
        {
            [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                return ((this.items != null) && (this.ItemCount > 0));
            }
        }

        internal int ItemCount
        {
            get
            {
                return this._itemCount;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("MenuMDIListItemDescr"), Browsable(false)]
        public MenuItem MdiListItem
        {
            get
            {
                for (int i = 0; i < this.ItemCount; i++)
                {
                    MenuItem mdiListItem = this.items[i];
                    if (mdiListItem.MdiList)
                    {
                        return mdiListItem;
                    }
                    if (mdiListItem.IsParent)
                    {
                        mdiListItem = mdiListItem.MdiListItem;
                        if (mdiListItem != null)
                        {
                            return mdiListItem;
                        }
                    }
                }
                return null;
            }
        }

        [MergableProperty(false), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), System.Windows.Forms.SRDescription("MenuMenuItemsDescr")]
        public MenuItemCollection MenuItems
        {
            get
            {
                if (this.itemsCollection == null)
                {
                    this.itemsCollection = new MenuItemCollection(this);
                }
                return this.itemsCollection;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string Name
        {
            get
            {
                return WindowsFormsUtils.GetComponentName(this, this.name);
            }
            set
            {
                if ((value == null) || (value.Length == 0))
                {
                    this.name = null;
                }
                else
                {
                    this.name = value;
                }
                if (this.Site != null)
                {
                    this.Site.Name = this.name;
                }
            }
        }

        internal virtual bool RenderIsRightToLeft
        {
            get
            {
                return false;
            }
        }

        internal int SelectedMenuItemIndex
        {
            get
            {
                for (int i = 0; i < this.items.Length; i++)
                {
                    MenuItem item = this.items[i];
                    if ((item != null) && item.Selected)
                    {
                        return i;
                    }
                }
                return -1;
            }
        }

        [System.Windows.Forms.SRDescription("ControlTagDescr"), Bindable(true), TypeConverter(typeof(StringConverter)), DefaultValue((string) null), Localizable(false), System.Windows.Forms.SRCategory("CatData")]
        public object Tag
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

        [ListBindable(false)]
        public class MenuItemCollection : IList, ICollection, IEnumerable
        {
            private int lastAccessedIndex = -1;
            private Menu owner;

            public MenuItemCollection(Menu owner)
            {
                this.owner = owner;
            }

            public virtual MenuItem Add(string caption)
            {
                MenuItem item = new MenuItem(caption);
                this.Add(item);
                return item;
            }

            public virtual int Add(MenuItem item)
            {
                return this.Add(this.owner.ItemCount, item);
            }

            public virtual int Add(int index, MenuItem item)
            {
                if (item.Menu != null)
                {
                    if (this.owner is MenuItem)
                    {
                        for (MenuItem item2 = (MenuItem) this.owner; item2 != null; item2 = (MenuItem) item2.Parent)
                        {
                            if (item2.Equals(item))
                            {
                                throw new ArgumentException(System.Windows.Forms.SR.GetString("MenuItemAlreadyExists", new object[] { item.Text }), "item");
                            }
                            if (!(item2.Parent is MenuItem))
                            {
                                break;
                            }
                        }
                    }
                    if (item.Menu.Equals(this.owner) && (index > 0))
                    {
                        index--;
                    }
                    item.Menu.MenuItems.Remove(item);
                }
                if ((index < 0) || (index > this.owner.ItemCount))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                if ((this.owner.items == null) || (this.owner.items.Length == this.owner.ItemCount))
                {
                    MenuItem[] destinationArray = new MenuItem[(this.owner.ItemCount < 2) ? 4 : (this.owner.ItemCount * 2)];
                    if (this.owner.ItemCount > 0)
                    {
                        Array.Copy(this.owner.items, 0, destinationArray, 0, this.owner.ItemCount);
                    }
                    this.owner.items = destinationArray;
                }
                Array.Copy(this.owner.items, index, this.owner.items, index + 1, this.owner.ItemCount - index);
                this.owner.items[index] = item;
                this.owner._itemCount++;
                item.Menu = this.owner;
                this.owner.ItemsChanged(0);
                if (this.owner is MenuItem)
                {
                    ((MenuItem) this.owner).ItemsChanged(4, item);
                }
                return index;
            }

            public virtual MenuItem Add(string caption, EventHandler onClick)
            {
                MenuItem item = new MenuItem(caption, onClick);
                this.Add(item);
                return item;
            }

            public virtual MenuItem Add(string caption, MenuItem[] items)
            {
                MenuItem item = new MenuItem(caption, items);
                this.Add(item);
                return item;
            }

            public virtual void AddRange(MenuItem[] items)
            {
                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                foreach (MenuItem item in items)
                {
                    this.Add(item);
                }
            }

            public virtual void Clear()
            {
                if (this.owner.ItemCount > 0)
                {
                    for (int i = 0; i < this.owner.ItemCount; i++)
                    {
                        this.owner.items[i].Menu = null;
                    }
                    this.owner._itemCount = 0;
                    this.owner.items = null;
                    this.owner.ItemsChanged(0);
                    if (this.owner is MenuItem)
                    {
                        ((MenuItem) this.owner).UpdateMenuItem(true);
                    }
                }
            }

            public bool Contains(MenuItem value)
            {
                return (this.IndexOf(value) != -1);
            }

            public virtual bool ContainsKey(string key)
            {
                return this.IsValidIndex(this.IndexOfKey(key));
            }

            public void CopyTo(Array dest, int index)
            {
                if (this.owner.ItemCount > 0)
                {
                    Array.Copy(this.owner.items, 0, dest, index, this.owner.ItemCount);
                }
            }

            public MenuItem[] Find(string key, bool searchAllChildren)
            {
                if ((key == null) || (key.Length == 0))
                {
                    throw new ArgumentNullException("key", System.Windows.Forms.SR.GetString("FindKeyMayNotBeEmptyOrNull"));
                }
                ArrayList list = this.FindInternal(key, searchAllChildren, this, new ArrayList());
                MenuItem[] array = new MenuItem[list.Count];
                list.CopyTo(array, 0);
                return array;
            }

            private ArrayList FindInternal(string key, bool searchAllChildren, Menu.MenuItemCollection menuItemsToLookIn, ArrayList foundMenuItems)
            {
                if ((menuItemsToLookIn == null) || (foundMenuItems == null))
                {
                    return null;
                }
                for (int i = 0; i < menuItemsToLookIn.Count; i++)
                {
                    if ((menuItemsToLookIn[i] != null) && WindowsFormsUtils.SafeCompareStrings(menuItemsToLookIn[i].Name, key, true))
                    {
                        foundMenuItems.Add(menuItemsToLookIn[i]);
                    }
                }
                if (searchAllChildren)
                {
                    for (int j = 0; j < menuItemsToLookIn.Count; j++)
                    {
                        if (((menuItemsToLookIn[j] != null) && (menuItemsToLookIn[j].MenuItems != null)) && (menuItemsToLookIn[j].MenuItems.Count > 0))
                        {
                            foundMenuItems = this.FindInternal(key, searchAllChildren, menuItemsToLookIn[j].MenuItems, foundMenuItems);
                        }
                    }
                }
                return foundMenuItems;
            }

            public IEnumerator GetEnumerator()
            {
                return new WindowsFormsUtils.ArraySubsetEnumerator(this.owner.items, this.owner.ItemCount);
            }

            public int IndexOf(MenuItem value)
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

            private bool IsValidIndex(int index)
            {
                return ((index >= 0) && (index < this.Count));
            }

            public virtual void Remove(MenuItem item)
            {
                if (item.Menu == this.owner)
                {
                    this.RemoveAt(item.Index);
                }
            }

            public virtual void RemoveAt(int index)
            {
                if ((index < 0) || (index >= this.owner.ItemCount))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                MenuItem item = this.owner.items[index];
                item.Menu = null;
                this.owner._itemCount--;
                Array.Copy(this.owner.items, index + 1, this.owner.items, index, this.owner.ItemCount - index);
                this.owner.items[this.owner.ItemCount] = null;
                this.owner.ItemsChanged(0);
                if (this.owner.ItemCount == 0)
                {
                    this.Clear();
                }
            }

            public virtual void RemoveByKey(string key)
            {
                int index = this.IndexOfKey(key);
                if (this.IsValidIndex(index))
                {
                    this.RemoveAt(index);
                }
            }

            int IList.Add(object value)
            {
                if (!(value is MenuItem))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("MenuBadMenuItem"), "value");
                }
                return this.Add((MenuItem) value);
            }

            bool IList.Contains(object value)
            {
                return ((value is MenuItem) && this.Contains((MenuItem) value));
            }

            int IList.IndexOf(object value)
            {
                if (value is MenuItem)
                {
                    return this.IndexOf((MenuItem) value);
                }
                return -1;
            }

            void IList.Insert(int index, object value)
            {
                if (!(value is MenuItem))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("MenuBadMenuItem"), "value");
                }
                this.Add(index, (MenuItem) value);
            }

            void IList.Remove(object value)
            {
                if (value is MenuItem)
                {
                    this.Remove((MenuItem) value);
                }
            }

            public int Count
            {
                get
                {
                    return this.owner.ItemCount;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public virtual MenuItem this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.owner.ItemCount))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    return this.owner.items[index];
                }
            }

            public virtual MenuItem this[string key]
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
                    throw new NotSupportedException();
                }
            }
        }

        private delegate bool MenuItemKeyComparer(MenuItem mi, char key);
    }
}

