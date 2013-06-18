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
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, ToolboxItem(false), DesignTimeVisible(false), DefaultProperty("Text"), TypeConverter(typeof(ListViewItemConverter))]
    public class ListViewItem : ICloneable, ISerializable
    {
        internal ListViewGroup group;
        private string groupName;
        internal int ID;
        private ListViewItemImageIndexer imageIndexer;
        private int indentCount;
        private int lastIndex;
        internal System.Windows.Forms.ListView listView;
        private ListViewSubItemCollection listViewSubItemCollection;
        private const int MAX_SUBITEMS = 0x1000;
        private Point position;
        private static readonly BitVector32.Section SavedStateImageIndexSection = BitVector32.CreateSection(15, StateWholeRowOneStyleSection);
        private BitVector32 state;
        private static readonly BitVector32.Section StateImageMaskSet = BitVector32.CreateSection(1, StateSelectedSection);
        private static readonly BitVector32.Section StateSelectedSection = BitVector32.CreateSection(1);
        private static readonly BitVector32.Section StateWholeRowOneStyleSection = BitVector32.CreateSection(1, StateImageMaskSet);
        private static readonly BitVector32.Section SubItemCountSection = BitVector32.CreateSection(0x1000, SavedStateImageIndexSection);
        private ListViewSubItem[] subItems;
        private string toolTipText;
        private object userData;

        public ListViewItem()
        {
            this.position = new Point(-1, -1);
            this.lastIndex = -1;
            this.ID = -1;
            this.state = new BitVector32();
            this.toolTipText = string.Empty;
            this.StateSelected = false;
            this.UseItemStyleForSubItems = true;
            this.SavedStateImageIndex = -1;
        }

        public ListViewItem(string text) : this(text, -1)
        {
        }

        public ListViewItem(string[] items) : this(items, -1)
        {
        }

        public ListViewItem(ListViewGroup group) : this()
        {
            this.Group = group;
        }

        public ListViewItem(string text, int imageIndex) : this()
        {
            this.ImageIndexer.Index = imageIndex;
            this.Text = text;
        }

        public ListViewItem(string[] items, int imageIndex) : this()
        {
            this.ImageIndexer.Index = imageIndex;
            if ((items != null) && (items.Length > 0))
            {
                this.subItems = new ListViewSubItem[items.Length];
                for (int i = 0; i < items.Length; i++)
                {
                    this.subItems[i] = new ListViewSubItem(this, items[i]);
                }
                this.SubItemCount = items.Length;
            }
        }

        protected ListViewItem(SerializationInfo info, StreamingContext context) : this()
        {
            this.Deserialize(info, context);
        }

        public ListViewItem(string text, string imageKey) : this()
        {
            this.ImageIndexer.Key = imageKey;
            this.Text = text;
        }

        public ListViewItem(string[] items, string imageKey) : this()
        {
            this.ImageIndexer.Key = imageKey;
            if ((items != null) && (items.Length > 0))
            {
                this.subItems = new ListViewSubItem[items.Length];
                for (int i = 0; i < items.Length; i++)
                {
                    this.subItems[i] = new ListViewSubItem(this, items[i]);
                }
                this.SubItemCount = items.Length;
            }
        }

        public ListViewItem(string text, ListViewGroup group) : this(text)
        {
            this.Group = group;
        }

        public ListViewItem(string[] items, ListViewGroup group) : this(items)
        {
            this.Group = group;
        }

        public ListViewItem(ListViewSubItem[] subItems, int imageIndex) : this()
        {
            this.ImageIndexer.Index = imageIndex;
            this.subItems = subItems;
            this.SubItemCount = this.subItems.Length;
            for (int i = 0; i < subItems.Length; i++)
            {
                subItems[i].owner = this;
            }
        }

        public ListViewItem(ListViewSubItem[] subItems, string imageKey) : this()
        {
            this.ImageIndexer.Key = imageKey;
            this.subItems = subItems;
            this.SubItemCount = this.subItems.Length;
            for (int i = 0; i < subItems.Length; i++)
            {
                subItems[i].owner = this;
            }
        }

        public ListViewItem(string text, int imageIndex, ListViewGroup group) : this(text, imageIndex)
        {
            this.Group = group;
        }

        public ListViewItem(string[] items, int imageIndex, ListViewGroup group) : this(items, imageIndex)
        {
            this.Group = group;
        }

        public ListViewItem(string text, string imageKey, ListViewGroup group) : this(text, imageKey)
        {
            this.Group = group;
        }

        public ListViewItem(string[] items, string imageKey, ListViewGroup group) : this(items, imageKey)
        {
            this.Group = group;
        }

        public ListViewItem(ListViewSubItem[] subItems, int imageIndex, ListViewGroup group) : this(subItems, imageIndex)
        {
            this.Group = group;
        }

        public ListViewItem(ListViewSubItem[] subItems, string imageKey, ListViewGroup group) : this(subItems, imageKey)
        {
            this.Group = group;
        }

        public ListViewItem(string[] items, int imageIndex, Color foreColor, Color backColor, System.Drawing.Font font) : this(items, imageIndex)
        {
            this.ForeColor = foreColor;
            this.BackColor = backColor;
            this.Font = font;
        }

        public ListViewItem(string[] items, string imageKey, Color foreColor, Color backColor, System.Drawing.Font font) : this(items, imageKey)
        {
            this.ForeColor = foreColor;
            this.BackColor = backColor;
            this.Font = font;
        }

        public ListViewItem(string[] items, int imageIndex, Color foreColor, Color backColor, System.Drawing.Font font, ListViewGroup group) : this(items, imageIndex, foreColor, backColor, font)
        {
            this.Group = group;
        }

        public ListViewItem(string[] items, string imageKey, Color foreColor, Color backColor, System.Drawing.Font font, ListViewGroup group) : this(items, imageKey, foreColor, backColor, font)
        {
            this.Group = group;
        }

        public void BeginEdit()
        {
            if (this.Index >= 0)
            {
                System.Windows.Forms.ListView listView = this.ListView;
                if (!listView.LabelEdit)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListViewBeginEditFailed"));
                }
                if (!listView.Focused)
                {
                    listView.FocusInternal();
                }
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(listView, listView.Handle), System.Windows.Forms.NativeMethods.LVM_EDITLABEL, this.Index, 0);
            }
        }

        public virtual object Clone()
        {
            ListViewSubItem[] subItems = new ListViewSubItem[this.SubItems.Count];
            for (int i = 0; i < this.SubItems.Count; i++)
            {
                ListViewSubItem item = this.SubItems[i];
                subItems[i] = new ListViewSubItem(null, item.Text, item.ForeColor, item.BackColor, item.Font);
                subItems[i].Tag = item.Tag;
            }
            System.Type type = base.GetType();
            ListViewItem item2 = null;
            if (type == typeof(ListViewItem))
            {
                item2 = new ListViewItem(subItems, this.ImageIndexer.Index);
            }
            else
            {
                item2 = (ListViewItem) Activator.CreateInstance(type);
            }
            item2.subItems = subItems;
            item2.ImageIndexer.Index = this.ImageIndexer.Index;
            item2.SubItemCount = this.SubItemCount;
            item2.Checked = this.Checked;
            item2.UseItemStyleForSubItems = this.UseItemStyleForSubItems;
            item2.Tag = this.Tag;
            if (!string.IsNullOrEmpty(this.ImageIndexer.Key))
            {
                item2.ImageIndexer.Key = this.ImageIndexer.Key;
            }
            item2.indentCount = this.indentCount;
            item2.StateImageIndex = this.StateImageIndex;
            item2.toolTipText = this.toolTipText;
            item2.BackColor = this.BackColor;
            item2.ForeColor = this.ForeColor;
            item2.Font = this.Font;
            item2.Text = this.Text;
            item2.Group = this.Group;
            return item2;
        }

        protected virtual void Deserialize(SerializationInfo info, StreamingContext context)
        {
            bool flag = false;
            string str = null;
            int num = -1;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SerializationEntry current = enumerator.Current;
                if (current.Name == "Text")
                {
                    this.Text = info.GetString(current.Name);
                }
                else
                {
                    if (current.Name == "ImageIndex")
                    {
                        num = info.GetInt32(current.Name);
                        continue;
                    }
                    if (current.Name == "ImageKey")
                    {
                        str = info.GetString(current.Name);
                        continue;
                    }
                    if (current.Name == "SubItemCount")
                    {
                        this.SubItemCount = info.GetInt32(current.Name);
                        if (this.SubItemCount > 0)
                        {
                            flag = true;
                        }
                        continue;
                    }
                    if (current.Name == "BackColor")
                    {
                        this.BackColor = (Color) info.GetValue(current.Name, typeof(Color));
                        continue;
                    }
                    if (current.Name == "Checked")
                    {
                        this.Checked = info.GetBoolean(current.Name);
                        continue;
                    }
                    if (current.Name == "Font")
                    {
                        this.Font = (System.Drawing.Font) info.GetValue(current.Name, typeof(System.Drawing.Font));
                        continue;
                    }
                    if (current.Name == "ForeColor")
                    {
                        this.ForeColor = (Color) info.GetValue(current.Name, typeof(Color));
                        continue;
                    }
                    if (current.Name == "UseItemStyleForSubItems")
                    {
                        this.UseItemStyleForSubItems = info.GetBoolean(current.Name);
                        continue;
                    }
                    if (current.Name == "Group")
                    {
                        ListViewGroup group = (ListViewGroup) info.GetValue(current.Name, typeof(ListViewGroup));
                        this.groupName = group.Name;
                    }
                }
            }
            if (str != null)
            {
                this.ImageKey = str;
            }
            else if (num != -1)
            {
                this.ImageIndex = num;
            }
            if (flag)
            {
                ListViewSubItem[] itemArray = new ListViewSubItem[this.SubItemCount];
                for (int i = 1; i < this.SubItemCount; i++)
                {
                    ListViewSubItem item = (ListViewSubItem) info.GetValue("SubItem" + i.ToString(CultureInfo.InvariantCulture), typeof(ListViewSubItem));
                    item.owner = this;
                    itemArray[i] = item;
                }
                itemArray[0] = this.subItems[0];
                this.subItems = itemArray;
            }
        }

        public virtual void EnsureVisible()
        {
            if ((this.listView != null) && this.listView.IsHandleCreated)
            {
                this.listView.EnsureVisible(this.Index);
            }
        }

        public ListViewItem FindNearestItem(SearchDirectionHint searchDirection)
        {
            Rectangle bounds = this.Bounds;
            switch (searchDirection)
            {
                case SearchDirectionHint.Left:
                    return this.ListView.FindNearestItem(searchDirection, bounds.Left, bounds.Top);

                case SearchDirectionHint.Up:
                    return this.ListView.FindNearestItem(searchDirection, bounds.Left, bounds.Top);

                case SearchDirectionHint.Right:
                    return this.ListView.FindNearestItem(searchDirection, bounds.Right, bounds.Top);

                case SearchDirectionHint.Down:
                    return this.ListView.FindNearestItem(searchDirection, bounds.Left, bounds.Bottom);
            }
            return null;
        }

        public Rectangle GetBounds(ItemBoundsPortion portion)
        {
            if ((this.listView != null) && this.listView.IsHandleCreated)
            {
                return this.listView.GetItemRect(this.Index, portion);
            }
            return new Rectangle();
        }

        public ListViewSubItem GetSubItemAt(int x, int y)
        {
            if (((this.listView != null) && this.listView.IsHandleCreated) && (this.listView.View == View.Details))
            {
                int iItem = -1;
                int iSubItem = -1;
                this.listView.GetSubItemAt(x, y, out iItem, out iSubItem);
                if (((iItem == this.Index) && (iSubItem != -1)) && (iSubItem < this.SubItems.Count))
                {
                    return this.SubItems[iSubItem];
                }
            }
            return null;
        }

        internal void Host(System.Windows.Forms.ListView parent, int ID, int index)
        {
            this.ID = ID;
            this.listView = parent;
            if (index != -1)
            {
                this.UpdateStateToListView(index);
            }
        }

        internal void InvalidateListView()
        {
            if ((this.listView != null) && this.listView.IsHandleCreated)
            {
                this.listView.Invalidate();
            }
        }

        public virtual void Remove()
        {
            if (this.listView != null)
            {
                this.listView.Items.Remove(this);
            }
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.SerializationFormatter), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        protected virtual void Serialize(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Text", this.Text);
            info.AddValue("ImageIndex", this.ImageIndexer.Index);
            if (!string.IsNullOrEmpty(this.ImageIndexer.Key))
            {
                info.AddValue("ImageKey", this.ImageIndexer.Key);
            }
            if (this.SubItemCount > 1)
            {
                info.AddValue("SubItemCount", this.SubItemCount);
                for (int i = 1; i < this.SubItemCount; i++)
                {
                    info.AddValue("SubItem" + i.ToString(CultureInfo.InvariantCulture), this.subItems[i], typeof(ListViewSubItem));
                }
            }
            info.AddValue("BackColor", this.BackColor);
            info.AddValue("Checked", this.Checked);
            info.AddValue("Font", this.Font);
            info.AddValue("ForeColor", this.ForeColor);
            info.AddValue("UseItemStyleForSubItems", this.UseItemStyleForSubItems);
            if (this.Group != null)
            {
                info.AddValue("Group", this.Group);
            }
        }

        internal void SetItemIndex(System.Windows.Forms.ListView listView, int index)
        {
            this.listView = listView;
            this.lastIndex = index;
        }

        private bool ShouldSerializePosition()
        {
            return !this.position.Equals(new Point(-1, -1));
        }

        internal bool ShouldSerializeText()
        {
            return false;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            this.Serialize(info, context);
        }

        public override string ToString()
        {
            return ("ListViewItem: {" + this.Text + "}");
        }

        internal void UnHost(bool checkSelection)
        {
            this.UnHost(this.Index, checkSelection);
        }

        internal void UnHost(int displayIndex, bool checkSelection)
        {
            this.UpdateStateFromListView(displayIndex, checkSelection);
            if (((this.listView != null) && ((this.listView.Site == null) || !this.listView.Site.DesignMode)) && (this.group != null))
            {
                this.group.Items.Remove(this);
            }
            this.ID = -1;
            this.listView = null;
        }

        internal void UpdateGroupFromName()
        {
            if (!string.IsNullOrEmpty(this.groupName))
            {
                ListViewGroup group = this.listView.Groups[this.groupName];
                this.Group = group;
                this.groupName = null;
            }
        }

        internal void UpdateStateFromListView(int displayIndex, bool checkSelection)
        {
            if (((this.listView != null) && this.listView.IsHandleCreated) && (displayIndex != -1))
            {
                System.Windows.Forms.NativeMethods.LVITEM lParam = new System.Windows.Forms.NativeMethods.LVITEM {
                    mask = 0x10c
                };
                if (checkSelection)
                {
                    lParam.stateMask = 2;
                }
                lParam.stateMask |= 0xf000;
                if (lParam.stateMask != 0)
                {
                    lParam.iItem = displayIndex;
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.listView, this.listView.Handle), System.Windows.Forms.NativeMethods.LVM_GETITEM, 0, ref lParam);
                    if (checkSelection)
                    {
                        this.StateSelected = (lParam.state & 2) != 0;
                    }
                    this.SavedStateImageIndex = ((lParam.state & 0xf000) >> 12) - 1;
                    this.group = null;
                    foreach (ListViewGroup group in this.ListView.Groups)
                    {
                        if (group.ID == lParam.iGroupId)
                        {
                            this.group = group;
                            break;
                        }
                    }
                }
            }
        }

        internal void UpdateStateToListView(int index)
        {
            System.Windows.Forms.NativeMethods.LVITEM lvItem = new System.Windows.Forms.NativeMethods.LVITEM();
            this.UpdateStateToListView(index, ref lvItem, true);
        }

        internal void UpdateStateToListView(int index, ref System.Windows.Forms.NativeMethods.LVITEM lvItem, bool updateOwner)
        {
            if (index == -1)
            {
                index = this.Index;
            }
            else
            {
                this.lastIndex = index;
            }
            int num = 0;
            int num2 = 0;
            if (this.StateSelected)
            {
                num |= 2;
                num2 |= 2;
            }
            if (this.SavedStateImageIndex > -1)
            {
                num |= (this.SavedStateImageIndex + 1) << 12;
                num2 |= 0xf000;
            }
            lvItem.mask |= 8;
            lvItem.iItem = index;
            lvItem.stateMask |= num2;
            lvItem.state |= num;
            if (this.listView.GroupsEnabled)
            {
                lvItem.mask |= 0x100;
                lvItem.iGroupId = this.listView.GetNativeGroupId(this);
            }
            if (updateOwner)
            {
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.listView, this.listView.Handle), System.Windows.Forms.NativeMethods.LVM_SETITEM, 0, ref lvItem);
            }
        }

        internal void UpdateSubItems(int index)
        {
            this.UpdateSubItems(index, this.SubItemCount);
        }

        internal void UpdateSubItems(int index, int oldCount)
        {
            if ((this.listView != null) && this.listView.IsHandleCreated)
            {
                int subItemCount = this.SubItemCount;
                int itemIndex = this.Index;
                if (index != -1)
                {
                    this.listView.SetItemText(itemIndex, index, this.subItems[index].Text);
                }
                else
                {
                    for (int j = 0; j < subItemCount; j++)
                    {
                        this.listView.SetItemText(itemIndex, j, this.subItems[j].Text);
                    }
                }
                for (int i = subItemCount; i < oldCount; i++)
                {
                    this.listView.SetItemText(itemIndex, i, string.Empty);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatAppearance")]
        public Color BackColor
        {
            get
            {
                if (this.SubItemCount != 0)
                {
                    return this.subItems[0].BackColor;
                }
                if (this.listView != null)
                {
                    return this.listView.BackColor;
                }
                return SystemColors.Window;
            }
            set
            {
                this.SubItems[0].BackColor = value;
            }
        }

        [Browsable(false)]
        public Rectangle Bounds
        {
            get
            {
                if (this.listView != null)
                {
                    return this.listView.GetItemRect(this.Index);
                }
                return new Rectangle();
            }
        }

        [RefreshProperties(RefreshProperties.Repaint), DefaultValue(false), System.Windows.Forms.SRCategory("CatAppearance")]
        public bool Checked
        {
            get
            {
                return (this.StateImageIndex > 0);
            }
            set
            {
                if (this.Checked != value)
                {
                    if ((this.listView != null) && this.listView.IsHandleCreated)
                    {
                        this.StateImageIndex = value ? 1 : 0;
                        if (((this.listView != null) && !this.listView.UseCompatibleStateImageBehavior) && !this.listView.CheckBoxes)
                        {
                            this.listView.UpdateSavedCheckedItems(this, value);
                        }
                    }
                    else
                    {
                        this.SavedStateImageIndex = value ? 1 : 0;
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool Focused
        {
            get
            {
                return (((this.listView != null) && this.listView.IsHandleCreated) && (this.listView.GetItemState(this.Index, 1) != 0));
            }
            set
            {
                if ((this.listView != null) && this.listView.IsHandleCreated)
                {
                    this.listView.SetItemState(this.Index, value ? 1 : 0, 1);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Localizable(true), System.Windows.Forms.SRCategory("CatAppearance")]
        public System.Drawing.Font Font
        {
            get
            {
                if (this.SubItemCount != 0)
                {
                    return this.subItems[0].Font;
                }
                if (this.listView != null)
                {
                    return this.listView.Font;
                }
                return Control.DefaultFont;
            }
            set
            {
                this.SubItems[0].Font = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatAppearance")]
        public Color ForeColor
        {
            get
            {
                if (this.SubItemCount != 0)
                {
                    return this.subItems[0].ForeColor;
                }
                if (this.listView != null)
                {
                    return this.listView.ForeColor;
                }
                return SystemColors.WindowText;
            }
            set
            {
                this.SubItems[0].ForeColor = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue((string) null), Localizable(true)]
        public ListViewGroup Group
        {
            get
            {
                return this.group;
            }
            set
            {
                if (this.group != value)
                {
                    if (value != null)
                    {
                        value.Items.Add(this);
                    }
                    else
                    {
                        this.group.Items.Remove(this);
                    }
                }
                this.groupName = null;
            }
        }

        [TypeConverter(typeof(NoneExcludedImageIndexConverter)), DefaultValue(-1), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), Localizable(true), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ListViewItemImageIndexDescr")]
        public int ImageIndex
        {
            get
            {
                if (((this.ImageIndexer.Index != -1) && (this.ImageList != null)) && (this.ImageIndexer.Index >= this.ImageList.Images.Count))
                {
                    return (this.ImageList.Images.Count - 1);
                }
                return this.ImageIndexer.Index;
            }
            set
            {
                if (value < -1)
                {
                    object[] args = new object[] { "ImageIndex", value.ToString(CultureInfo.CurrentCulture), -1.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("ImageIndex", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                this.ImageIndexer.Index = value;
                if ((this.listView != null) && this.listView.IsHandleCreated)
                {
                    this.listView.SetItemImage(this.Index, this.ImageIndexer.ActualIndex);
                }
            }
        }

        internal ListViewItemImageIndexer ImageIndexer
        {
            get
            {
                if (this.imageIndexer == null)
                {
                    this.imageIndexer = new ListViewItemImageIndexer(this);
                }
                return this.imageIndexer;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRCategory("CatBehavior"), TypeConverter(typeof(ImageKeyConverter)), DefaultValue(""), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), Localizable(true)]
        public string ImageKey
        {
            get
            {
                return this.ImageIndexer.Key;
            }
            set
            {
                this.ImageIndexer.Key = value;
                if ((this.listView != null) && this.listView.IsHandleCreated)
                {
                    this.listView.SetItemImage(this.Index, this.ImageIndexer.ActualIndex);
                }
            }
        }

        [Browsable(false)]
        public System.Windows.Forms.ImageList ImageList
        {
            get
            {
                if (this.listView != null)
                {
                    switch (this.listView.View)
                    {
                        case View.LargeIcon:
                        case View.Tile:
                            return this.listView.LargeImageList;

                        case View.Details:
                        case View.SmallIcon:
                        case View.List:
                            return this.listView.SmallImageList;
                    }
                }
                return null;
            }
        }

        [System.Windows.Forms.SRCategory("CatDisplay"), DefaultValue(0), System.Windows.Forms.SRDescription("ListViewItemIndentCountDescr")]
        public int IndentCount
        {
            get
            {
                return this.indentCount;
            }
            set
            {
                if (value != this.indentCount)
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("IndentCount", System.Windows.Forms.SR.GetString("ListViewIndentCountCantBeNegative"));
                    }
                    this.indentCount = value;
                    if ((this.listView != null) && this.listView.IsHandleCreated)
                    {
                        this.listView.SetItemIndentCount(this.Index, this.indentCount);
                    }
                }
            }
        }

        [Browsable(false)]
        public int Index
        {
            get
            {
                if (this.listView == null)
                {
                    return -1;
                }
                if (!this.listView.VirtualMode)
                {
                    this.lastIndex = this.listView.GetDisplayIndex(this, this.lastIndex);
                }
                return this.lastIndex;
            }
        }

        [Browsable(false)]
        public System.Windows.Forms.ListView ListView
        {
            get
            {
                return this.listView;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Localizable(true), Browsable(false)]
        public string Name
        {
            get
            {
                if (this.SubItemCount == 0)
                {
                    return string.Empty;
                }
                return this.subItems[0].Name;
            }
            set
            {
                this.SubItems[0].Name = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), System.Windows.Forms.SRCategory("CatDisplay")]
        public Point Position
        {
            get
            {
                if ((this.listView != null) && this.listView.IsHandleCreated)
                {
                    this.position = this.listView.GetItemPosition(this.Index);
                }
                return this.position;
            }
            set
            {
                if (!value.Equals(this.position))
                {
                    this.position = value;
                    if (((this.listView != null) && this.listView.IsHandleCreated) && !this.listView.VirtualMode)
                    {
                        this.listView.SetItemPosition(this.Index, this.position.X, this.position.Y);
                    }
                }
            }
        }

        internal int RawStateImageIndex
        {
            get
            {
                return ((this.SavedStateImageIndex + 1) << 12);
            }
        }

        private int SavedStateImageIndex
        {
            get
            {
                return (this.state[SavedStateImageIndexSection] - 1);
            }
            set
            {
                this.state[StateImageMaskSet] = (value == -1) ? 0 : 1;
                this.state[SavedStateImageIndexSection] = value + 1;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool Selected
        {
            get
            {
                if ((this.listView != null) && this.listView.IsHandleCreated)
                {
                    return (this.listView.GetItemState(this.Index, 2) != 0);
                }
                return this.StateSelected;
            }
            set
            {
                if ((this.listView != null) && this.listView.IsHandleCreated)
                {
                    this.listView.SetItemState(this.Index, value ? 2 : 0, 2);
                    this.listView.SetSelectionMark(this.Index);
                }
                else
                {
                    this.StateSelected = value;
                    if ((this.listView != null) && this.listView.IsHandleCreated)
                    {
                        this.listView.CacheSelectedStateForItem(this, value);
                    }
                }
            }
        }

        [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), RelatedImageList("ListView.StateImageList"), TypeConverter(typeof(NoneExcludedImageIndexConverter)), Localizable(true), DefaultValue(-1), System.Windows.Forms.SRDescription("ListViewItemStateImageIndexDescr"), System.Windows.Forms.SRCategory("CatBehavior"), RefreshProperties(RefreshProperties.Repaint)]
        public int StateImageIndex
        {
            get
            {
                if ((this.listView != null) && this.listView.IsHandleCreated)
                {
                    return ((this.listView.GetItemState(this.Index, 0xf000) >> 12) - 1);
                }
                return this.SavedStateImageIndex;
            }
            set
            {
                if ((value < -1) || (value > 14))
                {
                    throw new ArgumentOutOfRangeException("StateImageIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "StateImageIndex", value.ToString(CultureInfo.CurrentCulture) }));
                }
                if ((this.listView != null) && this.listView.IsHandleCreated)
                {
                    this.state[StateImageMaskSet] = (value == -1) ? 0 : 1;
                    int state = (value + 1) << 12;
                    this.listView.SetItemState(this.Index, state, 0xf000);
                }
                this.SavedStateImageIndex = value;
            }
        }

        internal bool StateImageSet
        {
            get
            {
                return (this.state[StateImageMaskSet] != 0);
            }
        }

        internal bool StateSelected
        {
            get
            {
                return (this.state[StateSelectedSection] == 1);
            }
            set
            {
                this.state[StateSelectedSection] = value ? 1 : 0;
            }
        }

        private int SubItemCount
        {
            get
            {
                return this.state[SubItemCountSection];
            }
            set
            {
                this.state[SubItemCountSection] = value;
            }
        }

        [System.Windows.Forms.SRDescription("ListViewItemSubItemsDescr"), System.Windows.Forms.SRCategory("CatData"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Editor("System.Windows.Forms.Design.ListViewSubItemCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public ListViewSubItemCollection SubItems
        {
            get
            {
                if (this.SubItemCount == 0)
                {
                    this.subItems = new ListViewSubItem[] { new ListViewSubItem(this, string.Empty) };
                    this.SubItemCount = 1;
                }
                if (this.listViewSubItemCollection == null)
                {
                    this.listViewSubItemCollection = new ListViewSubItemCollection(this);
                }
                return this.listViewSubItemCollection;
            }
        }

        [System.Windows.Forms.SRDescription("ControlTagDescr"), System.Windows.Forms.SRCategory("CatData"), Bindable(true), DefaultValue((string) null), Localizable(false), TypeConverter(typeof(StringConverter))]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Localizable(true), System.Windows.Forms.SRCategory("CatAppearance")]
        public string Text
        {
            get
            {
                if (this.SubItemCount == 0)
                {
                    return string.Empty;
                }
                return this.subItems[0].Text;
            }
            set
            {
                this.SubItems[0].Text = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue("")]
        public string ToolTipText
        {
            get
            {
                return this.toolTipText;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                if (!WindowsFormsUtils.SafeCompareStrings(this.toolTipText, value, false))
                {
                    this.toolTipText = value;
                    if ((this.listView != null) && this.listView.IsHandleCreated)
                    {
                        this.listView.ListViewItemToolTipChanged(this);
                    }
                }
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRCategory("CatAppearance")]
        public bool UseItemStyleForSubItems
        {
            get
            {
                return (this.state[StateWholeRowOneStyleSection] == 1);
            }
            set
            {
                this.state[StateWholeRowOneStyleSection] = value ? 1 : 0;
            }
        }

        internal class ListViewItemImageIndexer : System.Windows.Forms.ImageList.Indexer
        {
            private ListViewItem owner;

            public ListViewItemImageIndexer(ListViewItem item)
            {
                this.owner = item;
            }

            public override System.Windows.Forms.ImageList ImageList
            {
                get
                {
                    if (this.owner != null)
                    {
                        return this.owner.ImageList;
                    }
                    return null;
                }
                set
                {
                }
            }
        }

        [Serializable, TypeConverter(typeof(ListViewSubItemConverter)), DefaultProperty("Text"), ToolboxItem(false), DesignTimeVisible(false)]
        public class ListViewSubItem
        {
            [OptionalField(VersionAdded=2)]
            private string name;
            [NonSerialized]
            internal ListViewItem owner;
            private SubItemStyle style;
            private string text;
            [OptionalField(VersionAdded=2)]
            private object userData;

            public ListViewSubItem()
            {
            }

            public ListViewSubItem(ListViewItem owner, string text)
            {
                this.owner = owner;
                this.text = text;
            }

            public ListViewSubItem(ListViewItem owner, string text, Color foreColor, Color backColor, System.Drawing.Font font)
            {
                this.owner = owner;
                this.text = text;
                this.style = new SubItemStyle();
                this.style.foreColor = foreColor;
                this.style.backColor = backColor;
                this.style.font = font;
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext ctx)
            {
                this.name = null;
                this.userData = null;
            }

            [OnDeserializing]
            private void OnDeserializing(StreamingContext ctx)
            {
            }

            [OnSerialized]
            private void OnSerialized(StreamingContext ctx)
            {
            }

            [OnSerializing]
            private void OnSerializing(StreamingContext ctx)
            {
            }

            public void ResetStyle()
            {
                if (this.style != null)
                {
                    this.style = null;
                    if (this.owner != null)
                    {
                        this.owner.InvalidateListView();
                    }
                }
            }

            public override string ToString()
            {
                return ("ListViewSubItem: {" + this.Text + "}");
            }

            public Color BackColor
            {
                get
                {
                    if ((this.style != null) && (this.style.backColor != Color.Empty))
                    {
                        return this.style.backColor;
                    }
                    if ((this.owner != null) && (this.owner.listView != null))
                    {
                        return this.owner.listView.BackColor;
                    }
                    return SystemColors.Window;
                }
                set
                {
                    if (this.style == null)
                    {
                        this.style = new SubItemStyle();
                    }
                    if (this.style.backColor != value)
                    {
                        this.style.backColor = value;
                        if (this.owner != null)
                        {
                            this.owner.InvalidateListView();
                        }
                    }
                }
            }

            [Browsable(false)]
            public Rectangle Bounds
            {
                get
                {
                    if (((this.owner != null) && (this.owner.listView != null)) && this.owner.listView.IsHandleCreated)
                    {
                        return this.owner.listView.GetSubItemRect(this.owner.Index, this.owner.SubItems.IndexOf(this));
                    }
                    return Rectangle.Empty;
                }
            }

            internal bool CustomBackColor
            {
                get
                {
                    return ((this.style != null) && !this.style.backColor.IsEmpty);
                }
            }

            internal bool CustomFont
            {
                get
                {
                    return ((this.style != null) && (this.style.font != null));
                }
            }

            internal bool CustomForeColor
            {
                get
                {
                    return ((this.style != null) && !this.style.foreColor.IsEmpty);
                }
            }

            internal bool CustomStyle
            {
                get
                {
                    return (this.style != null);
                }
            }

            [Localizable(true)]
            public System.Drawing.Font Font
            {
                get
                {
                    if ((this.style != null) && (this.style.font != null))
                    {
                        return this.style.font;
                    }
                    if ((this.owner != null) && (this.owner.listView != null))
                    {
                        return this.owner.listView.Font;
                    }
                    return Control.DefaultFont;
                }
                set
                {
                    if (this.style == null)
                    {
                        this.style = new SubItemStyle();
                    }
                    if (this.style.font != value)
                    {
                        this.style.font = value;
                        if (this.owner != null)
                        {
                            this.owner.InvalidateListView();
                        }
                    }
                }
            }

            public Color ForeColor
            {
                get
                {
                    if ((this.style != null) && (this.style.foreColor != Color.Empty))
                    {
                        return this.style.foreColor;
                    }
                    if ((this.owner != null) && (this.owner.listView != null))
                    {
                        return this.owner.listView.ForeColor;
                    }
                    return SystemColors.WindowText;
                }
                set
                {
                    if (this.style == null)
                    {
                        this.style = new SubItemStyle();
                    }
                    if (this.style.foreColor != value)
                    {
                        this.style.foreColor = value;
                        if (this.owner != null)
                        {
                            this.owner.InvalidateListView();
                        }
                    }
                }
            }

            [Localizable(true)]
            public string Name
            {
                get
                {
                    if (this.name != null)
                    {
                        return this.name;
                    }
                    return "";
                }
                set
                {
                    this.name = value;
                    if (this.owner != null)
                    {
                        this.owner.UpdateSubItems(-1);
                    }
                }
            }

            [Localizable(false), DefaultValue((string) null), TypeConverter(typeof(StringConverter)), System.Windows.Forms.SRDescription("ControlTagDescr"), System.Windows.Forms.SRCategory("CatData"), Bindable(true)]
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

            [Localizable(true)]
            public string Text
            {
                get
                {
                    if (this.text != null)
                    {
                        return this.text;
                    }
                    return "";
                }
                set
                {
                    this.text = value;
                    if (this.owner != null)
                    {
                        this.owner.UpdateSubItems(-1);
                    }
                }
            }

            [Serializable]
            private class SubItemStyle
            {
                public Color backColor = Color.Empty;
                public Font font;
                public Color foreColor = Color.Empty;
            }
        }

        public class ListViewSubItemCollection : IList, ICollection, IEnumerable
        {
            private int lastAccessedIndex = -1;
            private ListViewItem owner;

            public ListViewSubItemCollection(ListViewItem owner)
            {
                this.owner = owner;
            }

            public ListViewItem.ListViewSubItem Add(string text)
            {
                ListViewItem.ListViewSubItem item = new ListViewItem.ListViewSubItem(this.owner, text);
                this.Add(item);
                return item;
            }

            public ListViewItem.ListViewSubItem Add(ListViewItem.ListViewSubItem item)
            {
                int num;
                this.EnsureSubItemSpace(1, -1);
                item.owner = this.owner;
                this.owner.subItems[this.owner.SubItemCount] = item;
                this.owner.SubItemCount = (num = this.owner.SubItemCount) + 1;
                this.owner.UpdateSubItems(num);
                return item;
            }

            public ListViewItem.ListViewSubItem Add(string text, Color foreColor, Color backColor, Font font)
            {
                ListViewItem.ListViewSubItem item = new ListViewItem.ListViewSubItem(this.owner, text, foreColor, backColor, font);
                this.Add(item);
                return item;
            }

            public void AddRange(string[] items)
            {
                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                this.EnsureSubItemSpace(items.Length, -1);
                foreach (string str in items)
                {
                    if (str != null)
                    {
                        int num2;
                        this.owner.SubItemCount = (num2 = this.owner.SubItemCount) + 1;
                        this.owner.subItems[num2] = new ListViewItem.ListViewSubItem(this.owner, str);
                    }
                }
                this.owner.UpdateSubItems(-1);
            }

            public void AddRange(ListViewItem.ListViewSubItem[] items)
            {
                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                this.EnsureSubItemSpace(items.Length, -1);
                foreach (ListViewItem.ListViewSubItem item in items)
                {
                    if (item != null)
                    {
                        int num2;
                        this.owner.SubItemCount = (num2 = this.owner.SubItemCount) + 1;
                        this.owner.subItems[num2] = item;
                    }
                }
                this.owner.UpdateSubItems(-1);
            }

            public void AddRange(string[] items, Color foreColor, Color backColor, Font font)
            {
                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                this.EnsureSubItemSpace(items.Length, -1);
                foreach (string str in items)
                {
                    if (str != null)
                    {
                        int num2;
                        this.owner.SubItemCount = (num2 = this.owner.SubItemCount) + 1;
                        this.owner.subItems[num2] = new ListViewItem.ListViewSubItem(this.owner, str, foreColor, backColor, font);
                    }
                }
                this.owner.UpdateSubItems(-1);
            }

            public void Clear()
            {
                int subItemCount = this.owner.SubItemCount;
                if (subItemCount > 0)
                {
                    this.owner.SubItemCount = 0;
                    this.owner.UpdateSubItems(-1, subItemCount);
                }
            }

            public bool Contains(ListViewItem.ListViewSubItem subItem)
            {
                return (this.IndexOf(subItem) != -1);
            }

            public virtual bool ContainsKey(string key)
            {
                return this.IsValidIndex(this.IndexOfKey(key));
            }

            private void EnsureSubItemSpace(int size, int index)
            {
                if (this.owner.SubItemCount == 0x1000)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ErrorCollectionFull"));
                }
                if ((this.owner.SubItemCount + size) > this.owner.subItems.Length)
                {
                    if (this.owner.subItems == null)
                    {
                        int num = (size > 4) ? size : 4;
                        this.owner.subItems = new ListViewItem.ListViewSubItem[num];
                    }
                    else
                    {
                        int num2 = this.owner.subItems.Length * 2;
                        while ((num2 - this.owner.SubItemCount) < size)
                        {
                            num2 *= 2;
                        }
                        ListViewItem.ListViewSubItem[] destinationArray = new ListViewItem.ListViewSubItem[num2];
                        if (index != -1)
                        {
                            Array.Copy(this.owner.subItems, 0, destinationArray, 0, index);
                            Array.Copy(this.owner.subItems, index, destinationArray, index + size, this.owner.SubItemCount - index);
                        }
                        else
                        {
                            Array.Copy(this.owner.subItems, destinationArray, this.owner.SubItemCount);
                        }
                        this.owner.subItems = destinationArray;
                    }
                }
                else if (index != -1)
                {
                    for (int i = this.owner.SubItemCount - 1; i >= index; i--)
                    {
                        this.owner.subItems[i + size] = this.owner.subItems[i];
                    }
                }
            }

            public IEnumerator GetEnumerator()
            {
                if (this.owner.subItems != null)
                {
                    return new WindowsFormsUtils.ArraySubsetEnumerator(this.owner.subItems, this.owner.SubItemCount);
                }
                return new ListViewItem.ListViewSubItem[0].GetEnumerator();
            }

            public int IndexOf(ListViewItem.ListViewSubItem subItem)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this.owner.subItems[i] == subItem)
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

            public void Insert(int index, ListViewItem.ListViewSubItem item)
            {
                if ((index < 0) || (index > this.Count))
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                item.owner = this.owner;
                this.EnsureSubItemSpace(1, index);
                this.owner.subItems[index] = item;
                this.owner.SubItemCount++;
                this.owner.UpdateSubItems(-1);
            }

            private bool IsValidIndex(int index)
            {
                return ((index >= 0) && (index < this.Count));
            }

            public void Remove(ListViewItem.ListViewSubItem item)
            {
                int index = this.IndexOf(item);
                if (index != -1)
                {
                    this.RemoveAt(index);
                }
            }

            public void RemoveAt(int index)
            {
                if ((index < 0) || (index >= this.Count))
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                for (int i = index + 1; i < this.owner.SubItemCount; i++)
                {
                    this.owner.subItems[i - 1] = this.owner.subItems[i];
                }
                int subItemCount = this.owner.SubItemCount;
                this.owner.SubItemCount--;
                this.owner.subItems[this.owner.SubItemCount] = null;
                this.owner.UpdateSubItems(-1, subItemCount);
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
                    Array.Copy(this.owner.subItems, 0, dest, index, this.Count);
                }
            }

            int IList.Add(object item)
            {
                if (!(item is ListViewItem.ListViewSubItem))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ListViewSubItemCollectionInvalidArgument"));
                }
                return this.IndexOf(this.Add((ListViewItem.ListViewSubItem) item));
            }

            bool IList.Contains(object subItem)
            {
                return ((subItem is ListViewItem.ListViewSubItem) && this.Contains((ListViewItem.ListViewSubItem) subItem));
            }

            int IList.IndexOf(object subItem)
            {
                if (subItem is ListViewItem.ListViewSubItem)
                {
                    return this.IndexOf((ListViewItem.ListViewSubItem) subItem);
                }
                return -1;
            }

            void IList.Insert(int index, object item)
            {
                if (!(item is ListViewItem.ListViewSubItem))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ListViewBadListViewSubItem"), "item");
                }
                this.Insert(index, (ListViewItem.ListViewSubItem) item);
            }

            void IList.Remove(object item)
            {
                if (item is ListViewItem.ListViewSubItem)
                {
                    this.Remove((ListViewItem.ListViewSubItem) item);
                }
            }

            [Browsable(false)]
            public int Count
            {
                get
                {
                    return this.owner.SubItemCount;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public ListViewItem.ListViewSubItem this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.Count))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    return this.owner.subItems[index];
                }
                set
                {
                    if ((index < 0) || (index >= this.Count))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    this.owner.subItems[index] = value;
                    this.owner.UpdateSubItems(index);
                }
            }

            public virtual ListViewItem.ListViewSubItem this[string key]
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
                    if (!(value is ListViewItem.ListViewSubItem))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("ListViewBadListViewSubItem"), "value");
                    }
                    this[index] = (ListViewItem.ListViewSubItem) value;
                }
            }
        }
    }
}

