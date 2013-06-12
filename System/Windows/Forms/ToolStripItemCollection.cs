namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Windows.Forms.Layout;

    [ListBindable(false), Editor("System.Windows.Forms.Design.ToolStripCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UIPermission(SecurityAction.InheritanceDemand, Window=UIPermissionWindow.AllWindows)]
    public class ToolStripItemCollection : ArrangedElementCollection, IList, ICollection, IEnumerable
    {
        private bool isReadOnly;
        private bool itemsCollection;
        private int lastAccessedIndex;
        private ToolStrip owner;

        internal ToolStripItemCollection(ToolStrip owner, bool itemsCollection) : this(owner, itemsCollection, false)
        {
        }

        public ToolStripItemCollection(ToolStrip owner, ToolStripItem[] value)
        {
            this.lastAccessedIndex = -1;
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            this.owner = owner;
            this.AddRange(value);
        }

        internal ToolStripItemCollection(ToolStrip owner, bool itemsCollection, bool isReadOnly)
        {
            this.lastAccessedIndex = -1;
            this.owner = owner;
            this.itemsCollection = itemsCollection;
            this.isReadOnly = isReadOnly;
        }

        public ToolStripItem Add(Image image)
        {
            return this.Add(null, image, null);
        }

        public ToolStripItem Add(string text)
        {
            return this.Add(text, null, null);
        }

        public int Add(ToolStripItem value)
        {
            this.CheckCanAddOrInsertItem(value);
            this.SetOwner(value);
            int num = base.InnerList.Add(value);
            if (this.itemsCollection && (this.owner != null))
            {
                this.owner.OnItemAdded(new ToolStripItemEventArgs(value));
            }
            return num;
        }

        public ToolStripItem Add(string text, Image image)
        {
            return this.Add(text, image, null);
        }

        public ToolStripItem Add(string text, Image image, EventHandler onClick)
        {
            ToolStripItem item = this.owner.CreateDefaultItem(text, image, onClick);
            this.Add(item);
            return item;
        }

        public void AddRange(ToolStripItem[] toolStripItems)
        {
            if (toolStripItems == null)
            {
                throw new ArgumentNullException("toolStripItems");
            }
            if (this.IsReadOnly)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripItemCollectionIsReadOnly"));
            }
            using (new LayoutTransaction(this.owner, this.owner, PropertyNames.Items))
            {
                for (int i = 0; i < toolStripItems.Length; i++)
                {
                    this.Add(toolStripItems[i]);
                }
            }
        }

        public void AddRange(ToolStripItemCollection toolStripItems)
        {
            if (toolStripItems == null)
            {
                throw new ArgumentNullException("toolStripItems");
            }
            if (this.IsReadOnly)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripItemCollectionIsReadOnly"));
            }
            using (new LayoutTransaction(this.owner, this.owner, PropertyNames.Items))
            {
                int count = toolStripItems.Count;
                for (int i = 0; i < count; i++)
                {
                    this.Add(toolStripItems[i]);
                }
            }
        }

        private void CheckCanAddOrInsertItem(ToolStripItem value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (this.IsReadOnly)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripItemCollectionIsReadOnly"));
            }
            ToolStripDropDown owner = this.owner as ToolStripDropDown;
            if (owner != null)
            {
                if (owner.OwnerItem == value)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripItemCircularReference"));
                }
                if (((value is ToolStripControlHost) && !(value is ToolStripScrollButton)) && owner.IsRestrictedWindow)
                {
                    System.Windows.Forms.IntSecurity.AllWindows.Demand();
                }
            }
        }

        public virtual void Clear()
        {
            if (this.IsReadOnly)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripItemCollectionIsReadOnly"));
            }
            if (this.Count != 0)
            {
                ToolStripOverflow overflow = null;
                if ((this.owner != null) && !this.owner.IsDisposingItems)
                {
                    this.owner.SuspendLayout();
                    overflow = this.owner.GetOverflow();
                    if (overflow != null)
                    {
                        overflow.SuspendLayout();
                    }
                }
                try
                {
                    while (this.Count != 0)
                    {
                        this.RemoveAt(this.Count - 1);
                    }
                }
                finally
                {
                    if (overflow != null)
                    {
                        overflow.ResumeLayout(false);
                    }
                    if ((this.owner != null) && !this.owner.IsDisposingItems)
                    {
                        this.owner.ResumeLayout();
                    }
                }
            }
        }

        public bool Contains(ToolStripItem value)
        {
            return base.InnerList.Contains(value);
        }

        public virtual bool ContainsKey(string key)
        {
            return this.IsValidIndex(this.IndexOfKey(key));
        }

        public void CopyTo(ToolStripItem[] array, int index)
        {
            base.InnerList.CopyTo(array, index);
        }

        public ToolStripItem[] Find(string key, bool searchAllChildren)
        {
            if ((key == null) || (key.Length == 0))
            {
                throw new ArgumentNullException("key", System.Windows.Forms.SR.GetString("FindKeyMayNotBeEmptyOrNull"));
            }
            ArrayList list = this.FindInternal(key, searchAllChildren, this, new ArrayList());
            ToolStripItem[] array = new ToolStripItem[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        private ArrayList FindInternal(string key, bool searchAllChildren, ToolStripItemCollection itemsToLookIn, ArrayList foundItems)
        {
            if ((itemsToLookIn == null) || (foundItems == null))
            {
                return null;
            }
            try
            {
                for (int i = 0; i < itemsToLookIn.Count; i++)
                {
                    if ((itemsToLookIn[i] != null) && WindowsFormsUtils.SafeCompareStrings(itemsToLookIn[i].Name, key, true))
                    {
                        foundItems.Add(itemsToLookIn[i]);
                    }
                }
                if (!searchAllChildren)
                {
                    return foundItems;
                }
                for (int j = 0; j < itemsToLookIn.Count; j++)
                {
                    ToolStripDropDownItem item = itemsToLookIn[j] as ToolStripDropDownItem;
                    if ((item != null) && item.HasDropDownItems)
                    {
                        foundItems = this.FindInternal(key, searchAllChildren, item.DropDownItems, foundItems);
                    }
                }
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                {
                    throw;
                }
            }
            return foundItems;
        }

        public int IndexOf(ToolStripItem value)
        {
            return base.InnerList.IndexOf(value);
        }

        public virtual int IndexOfKey(string key)
        {
            if ((key != null) && (key.Length != 0))
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

        public void Insert(int index, ToolStripItem value)
        {
            this.CheckCanAddOrInsertItem(value);
            this.SetOwner(value);
            base.InnerList.Insert(index, value);
            if (this.itemsCollection && (this.owner != null))
            {
                if (this.owner.IsHandleCreated)
                {
                    LayoutTransaction.DoLayout(this.owner, value, PropertyNames.Parent);
                }
                else
                {
                    CommonProperties.xClearPreferredSizeCache(this.owner);
                }
                this.owner.OnItemAdded(new ToolStripItemEventArgs(value));
            }
        }

        private bool IsValidIndex(int index)
        {
            return ((index >= 0) && (index < this.Count));
        }

        internal void MoveItem(ToolStripItem value)
        {
            if (value.ParentInternal != null)
            {
                int index = value.ParentInternal.Items.IndexOf(value);
                if (index >= 0)
                {
                    value.ParentInternal.Items.RemoveAt(index);
                }
            }
            this.Add(value);
        }

        internal void MoveItem(int index, ToolStripItem value)
        {
            if (index == this.Count)
            {
                this.MoveItem(value);
            }
            else
            {
                if (value.ParentInternal != null)
                {
                    int num = value.ParentInternal.Items.IndexOf(value);
                    if (num >= 0)
                    {
                        value.ParentInternal.Items.RemoveAt(num);
                        if ((value.ParentInternal == this.owner) && (index > num))
                        {
                            index--;
                        }
                    }
                }
                this.Insert(index, value);
            }
        }

        private void OnAfterRemove(ToolStripItem item)
        {
            if (this.itemsCollection)
            {
                ToolStrip parentInternal = null;
                if (item != null)
                {
                    parentInternal = item.ParentInternal;
                    item.SetOwner(null);
                }
                if ((this.owner != null) && !this.owner.IsDisposingItems)
                {
                    ToolStripItemEventArgs e = new ToolStripItemEventArgs(item);
                    this.owner.OnItemRemoved(e);
                    if ((parentInternal != null) && (parentInternal != this.owner))
                    {
                        parentInternal.OnItemVisibleChanged(e, false);
                    }
                }
            }
        }

        public void Remove(ToolStripItem value)
        {
            if (this.IsReadOnly)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripItemCollectionIsReadOnly"));
            }
            base.InnerList.Remove(value);
            this.OnAfterRemove(value);
        }

        public void RemoveAt(int index)
        {
            if (this.IsReadOnly)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripItemCollectionIsReadOnly"));
            }
            ToolStripItem item = null;
            if ((index < this.Count) && (index >= 0))
            {
                item = (ToolStripItem) base.InnerList[index];
            }
            base.InnerList.RemoveAt(index);
            this.OnAfterRemove(item);
        }

        public virtual void RemoveByKey(string key)
        {
            if (this.IsReadOnly)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripItemCollectionIsReadOnly"));
            }
            int index = this.IndexOfKey(key);
            if (this.IsValidIndex(index))
            {
                this.RemoveAt(index);
            }
        }

        private void SetOwner(ToolStripItem item)
        {
            if (this.itemsCollection && (item != null))
            {
                if (item.Owner != null)
                {
                    item.Owner.Items.Remove(item);
                }
                item.SetOwner(this.owner);
                if (item.Renderer != null)
                {
                    item.Renderer.InitializeItem(item);
                }
            }
        }

        int IList.Add(object value)
        {
            return this.Add(value as ToolStripItem);
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            return base.InnerList.Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf(value as ToolStripItem);
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index, value as ToolStripItem);
        }

        void IList.Remove(object value)
        {
            this.Remove(value as ToolStripItem);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        public override bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public virtual ToolStripItem this[int index]
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (ToolStripItem) base.InnerList[index];
            }
        }

        public virtual ToolStripItem this[string key]
        {
            get
            {
                if ((key != null) && (key.Length != 0))
                {
                    int index = this.IndexOfKey(key);
                    if (this.IsValidIndex(index))
                    {
                        return (ToolStripItem) base.InnerList[index];
                    }
                }
                return null;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return base.InnerList.IsFixedSize;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return base.InnerList[index];
            }
            set
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripCollectionMustInsertAndRemove"));
            }
        }
    }
}

